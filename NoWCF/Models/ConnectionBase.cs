using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoWCF.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NoWCF.Models
{
    public abstract class ConnectionBase : IConnectionBase
    {
        public Action<Exception> HandleException;
        public Action OnConnectionClosed;

        public bool Connected => _networkStream != null && !Disconnected;
        public bool Disconnected;

        protected readonly NoWCFSettings _setings;
        protected NetworkStream _networkStream;
        protected Socket _socket;
        public EndPoint RemoteEndPoint { get; protected set; }

        protected Dictionary<string, ProtocolSpecification> _protocols = new Dictionary<string, ProtocolSpecification>();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _callbackOperations = new ConcurrentDictionary<string, TaskCompletionSource<object>>();
        private readonly string _communicationKey;
        private ICrypt _cryptor;
        private bool _disposing;

        public ConnectionBase(NoWCFSettings settings)
        {
            _setings = settings;
        }

        public ConnectionBase(string communicationKey, NoWCFSettings settings)
        {
            _setings = settings;
            _communicationKey = communicationKey;

            if (_setings.UseEncryption)
                _cryptor = _setings.CryptorFactory().Initialize(communicationKey);
        }

        public void Disconnect()
        {
            try
            {
                _networkStream?.Close(0);
                _socket?.Disconnect(false);
                _socket?.Close(0);
            }
            catch { }
        }

        public InvokeType GetProtocol<ProtocolType, InvokeType>() where InvokeType : IInvokeProtocol
        {
            if (!Connected)
                return default;

            var protocolType = typeof(ProtocolType);
            var key = protocolType.GetInterfaces()[0].Name;

            if (!_protocols.ContainsKey(key))
                throw new Exception("Protocol not found.");

            var invokeObject = (InvokeType)_protocols[key].InvokeObject;
            if (invokeObject == null)
                throw new Exception("Cannot cast invoke type.");

            return invokeObject;
        }

        public InvokeType GetProtocol<InvokeType>() where InvokeType : IInvokeProtocol
        {
            if (!Connected)
                return default;

            var invokeType = typeof(InvokeType);
            var key = invokeType.GetInterfaces()[0].Name;

            if (!_protocols.ContainsKey(key))
                throw new Exception("Protocol not found.");

            var invokeObject = (InvokeType)_protocols[key].InvokeObject;
            if (invokeObject == null)
                throw new Exception("Cannot cast invoke type.");

            return invokeObject;
        }

        public void RegisterDuplexProtocol<ProtocolType, InvokeType>() where InvokeType : IInvokeProtocol => RegisterDuplexProtocol(typeof(ProtocolType), typeof(InvokeType));

        internal void RegisterDuplexProtocol(Type protocolType, Type invokeProtocolType)
        {
            var key = protocolType.GetInterfaces()[0].Name;
            _protocols.Add(key, new ProtocolSpecification(this, protocolType, invokeProtocolType));
        }

        public void RegisterProtocol<ProtocolType>() => RegisterProtocol(typeof(ProtocolType));

        internal void RegisterProtocol(Type protocolType)
        {
            var key = protocolType.GetInterfaces()[0].Name;
            _protocols.Add(key, new ProtocolSpecification(protocolType));
        }

        public void RegisterInvokeProtocol<InvokeType>() where InvokeType : IInvokeProtocol
        {
            var invokeType = typeof(InvokeType);
            var key = invokeType.GetInterfaces()[0].Name;
            _protocols.Add(key, new ProtocolSpecification(this, invokeType));
        }

        private int getEncryptedSizePacketLen()
        {
            var sizeLen = sizeof(int);

            if (!_setings.UseEncryption)
                return sizeLen;

            return (_cryptor ?? _setings.GenericCryptor).GetEncryptedLength(sizeLen);
        }

        async Task<byte[]> readData(int len)
        {
            if (len > _setings.ReceivePacketMaxSize)
            {
                HandleException?.Invoke(new Exception($"{len} > Settings.ReceivePacketMaxSize ({_setings.ReceivePacketMaxSize})"));
                return null;
            }

            var packet = new ConnectionPacket(len);
            while (true)
            {
                try
                {
                    if (await packet.Read(_networkStream, _setings.ReceiveBufferMaxSize))
                        return packet.Data;
                }
                catch
                {
                    handleConnectionClosed();
                    return null;
                }
            }
        }

        public async void BeginReceive()
        {
            if (_cryptor != null)
                if (!await sendCryptInfo())
                    return;

            while (true)
            {               
                var sizeData = transfromData(await readData(getEncryptedSizePacketLen()), false);
                if (sizeData == null || sizeData.Length == 0)
                    return;

                var packetLen = BitConverter.ToInt32(sizeData, 0);
                var rawPacketData = await readData(packetLen);
                if (rawPacketData == null)
                    return;

                if (_setings.UseEncryption)
                {
                    if (_cryptor == null)
                    {
                        if (!initCryptInfo(rawPacketData))
                            return;
                        continue;
                    }
                }

                var packetData = transfromData(rawPacketData, false);
                if (packetData.Length == 0)
                    return;

                var packetStr = Encoding.UTF8.GetString(packetData, 0, packetData.Length);

                SerializedOp op;
                try
                {
                    op = packetStr.Deserialize<SerializedOp>();
                }
                catch (Exception ex)
                {
                    HandleException?.Invoke(new Exception("Packet Deserialization Failure.", ex));
                    continue;
                }

                if (!op.InvokeCompleted)
                {
                    var protocol = _protocols[op.Protocol];

                    var result = (object)default;
                    try
                    {
                        result = protocol.ImplementationType.GetMethod(op.Method).Invoke(protocol.ImplementationObject, op.Parameters);
                    }
                    catch (Exception ex)
                    {
                        if (op.WaitResponse)
                        {
                            if (_setings.PropogateExceptions)
                                await sendAsync(serializeOperation(op.SetError(ex)), false);
                            else
                                await sendAsync(serializeOperation(op.SetError(ex.Message)), false);
                        }
                    }

                    if (!op.Error && op.WaitResponse)
                        await sendAsync(serializeOperation(op.SetResult(result)), false);
                }
                else
                {
                    if (op.WaitResponse)
                    {
                        // returned operation
                        if (_callbackOperations.TryRemove(op.ID, out var callbackTask))
                        {
                            if (!op.Error)
                                callbackTask.SetResult(op.Result);
                            else
                            {
                                callbackTask.SetException(new Exception("Faulted operation.", op.OperationException) ?? new Exception($"Faulted operation; {op.OperationError}"));
                            }
                        }
                    }
                }
            }
        }

        private bool initCryptInfo(byte[] data)
        {
            if (_setings.CertificateCryptor != null)
            {
                try
                {
                    data = _setings.CertificateCryptor.Decrypt(data);
                }
                catch (Exception ex)
                {
                    HandleException?.Invoke(new Exception("Certificate Cryptor Failure.", ex));
                    return false;
                }
                
            }

            try
            {
                data = _setings.GenericCryptor.Decrypt(data);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(new Exception("Simple Cryptor Failure.", ex));
                return false;
            }

            _cryptor = _setings.CryptorFactory().Initialize(Encoding.ASCII.GetString(data));
            return true;
        }

        private async Task<bool> sendCryptInfo()
        {
            if (!_setings.UseEncryption)
                return true;

            var encodedKey = _setings.GenericCryptor.Encrypt(Encoding.ASCII.GetBytes(_communicationKey));
            if (_setings.CertificateCryptor != null)
                encodedKey = _setings.CertificateCryptor.Encrypt(encodedKey);

            var encodedKeyLenData = _setings.GenericCryptor.Encrypt(BitConverter.GetBytes(encodedKey.Length));

            return await sendAsync(encodedKeyLenData.Concat(encodedKey).ToArray(), false);
        }

        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;

            foreach(var protocol in _protocols.Values)
                (protocol.ImplementationObject as IDisposable)?.Dispose();
            _protocols.Clear();

            try
            {
                _networkStream?.Close(0);
                _socket?.Close(0);
            }
            catch { }

            try
            {
                _networkStream?.Dispose();
                _socket?.Dispose();
            }
            catch { }

            foreach (var id in _callbackOperations.Keys.ToList())
                if (_callbackOperations.TryRemove(id, out var callback))
                    callback.SetException(new Exception("Disposed."));
        }

        void IConnectionBase.Invoke(string protocol, string method, Dictionary<string, object> parameters) => send(serializeOperation(createOperation(protocol, method, parameters, false, out _)), true);

        Task IConnectionBase.InvokeAsync(string protocol, string method, Dictionary<string, object> parameters) => sendAsync(serializeOperation(createOperation(protocol, method, parameters, false, out _)), true);

        async Task<N> IConnectionBase.InvokeWithResponseAsync<N>(string protocol, string method, Dictionary<string, object> parameters)
        {
            var data = serializeOperation(createOperation(protocol, method, parameters, true, out var id), true);

            var callbackTask = new TaskCompletionSource<object>();
            _callbackOperations.TryAdd(id, callbackTask);

            await sendAsync(data);

            return parseResponse<N>(await callbackTask.Task);
        }

        private N parseResponse<N>(object response)
        {
            if (response is JToken)
                return JsonConvert.DeserializeObject<N>(response.ToString());

            return (N)response;
        }

        N IConnectionBase.InvokeWithResponse<N>(string protocol, string method, Dictionary<string, object> parameters)
        {
            var data = serializeOperation(createOperation(protocol, method, parameters, true, out var id), true);

            var callbackTask = new TaskCompletionSource<object>();
            _callbackOperations.TryAdd(id, callbackTask);

            send(data);

            return parseResponse<N>(callbackTask.Task.ConfigureAwait(false).GetAwaiter().GetResult());
        }

        private bool send(byte[] data, bool throwExceptions = true)
        {
            if (data.Length == 0)
                return true;

            try
            {
                _networkStream.Write(data, 0, data.Length);
            }
            catch
            {
                handleConnectionClosed();
                if (throwExceptions)
                    throw;

                return false;
            }

            return true;
        }

        private async Task<bool> sendAsync(byte[] data, bool throwExceptions = true)
        {
            if (data.Length == 0)
                return true;

            try
            {
                await _networkStream.WriteAsync(data, 0, data.Length);
            }
            catch
            {
                handleConnectionClosed();
                if (throwExceptions)
                    throw;

                return false;
            }

            return true;
        }

        private void handleConnectionClosed(Exception ex = null)
        {
            Disconnected = true;

            if (_disposing)
                return;

            _networkStream?.Dispose();

            foreach (var id in _callbackOperations.Keys.ToList())
                if (_callbackOperations.TryRemove(id, out var callback))
                    callback.SetException(new Exception("Connection closed."));

            if (ex != null)
                HandleException?.Invoke(ex);

            OnConnectionClosed?.Invoke();

            using (this) ;
        }

        private static SerializedOp createOperation(string protocol, string method, Dictionary<string, object> parameters, bool waitResponse, out string uniqueID)
        {
            uniqueID = Guid.NewGuid().ToString();

            return new SerializedOp()
            {
                ID = uniqueID,
                Parameters = parameters,
                Method = method,
                WaitResponse = waitResponse,
                Protocol = protocol
            };
        }

        private byte[] serializeOperation(SerializedOp op, bool throwExceptions = false) => createPacket(Encoding.UTF8.GetBytes(op.Serialize()), throwExceptions);

        private byte[] createPacket(byte[] data, bool throwExceptions = false)
        {
            data = transfromData(data, true, throwExceptions);
            if (data.Length == 0)
                return Array.Empty<byte>();

            var sizeData = transfromData(BitConverter.GetBytes(data.Length), true, throwExceptions);
            if (sizeData.Length == 0)
                return Array.Empty<byte>();

            return sizeData.Concat(data).ToArray();
        }

        private byte[] transfromData(byte[] data, bool encrypt, bool throwExceptions = false)
        {
            if (data == null)
                return data;

            if (!_setings.UseEncryption)
                return data;

            var encryptionProvider = _cryptor ?? _setings.GenericCryptor;

            try
            {
                return encrypt ? encryptionProvider.Encrypt(data) : encryptionProvider.Decrypt(data);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(new Exception("Simple Cryptor Failure.", ex));
                if (throwExceptions)
                    throw;

                return Array.Empty<byte>();
            }
        }
    }
}
