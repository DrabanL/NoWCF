using NoWCF.Models;
using NoWCF.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NoWCF
{
    public class NoWCFServer : IDisposable
    {
        private bool _stopping;
        private Socket _socket;
        private readonly int _port;
        private readonly List<(Type, Type)> _duplexProtocols = new List<(Type, Type)>();
        private readonly List<Type> _protocols = new List<Type>();
        private readonly NoWCFSettings _settings;
        private SynchronizedCollection<ConnectionBase> _clients = new SynchronizedCollection<ConnectionBase>();
        public IReadOnlyCollection<ConnectionBase> GetConnectedClients() => _clients.AsReadOnly();
        public Action<Exception> HandleException;

        public NoWCFServer(int port, NoWCFSettings settings = null)
        {
            _settings = settings ?? new NoWCFSettings();
            _port = port;
        }

        public void Start()
        {
            _stopping = false;
            _socket?.Dispose();
            _socket = _settings.SocketFactory();
            _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socket.Listen(_settings.ListenerBacklogCount);
            beginAccept();
        }

        public void Stop()
        {
            _stopping = true;
            _socket?.Close();
            _socket?.Dispose();

            foreach (var client in _clients.ToList())
                client.Disconnect();

            SpinWait.SpinUntil(() => _clients.Count == 0);
        }

        public void AddDuplexProtocol<ImplProtoctol, InvokeProtocol>() where InvokeProtocol : IInvokeProtocol => _duplexProtocols.Add((typeof(ImplProtoctol), typeof(InvokeProtocol)));

        public void AddProtocol<ImplProtoctol>() => _protocols.Add(typeof(ImplProtoctol));

        private async void beginAccept()
        {
            while (true)
            {
                Socket socket;
                try
                {
                    socket = await _socket.AcceptAsync();
                }
                catch (Exception ex)
                {
                    if (!_stopping)
                        HandleException?.Invoke(ex);

                    if (_stopping)
                        return;
                    
                    continue;
                }

                processConnection(socket);
            }
        }

        private void processConnection(Socket socket)
        {
            var client = new NoWCFClient(socket, _settings)
            {
                HandleException = (E) => Console.WriteLine(E),
                HandleConnectionClosed = (cl) =>
                {
                    using (cl)
                    {
                        Console.WriteLine("CLOSED");
                        _clients.Remove(cl);
                    }
                }
            };

            _clients.Add(client);

            foreach (var protocol in _protocols.Distinct())
                client.RegisterProtocol(protocol);
            foreach (var protocol in _duplexProtocols.Distinct())
                client.RegisterDuplexProtocol(protocol.Item1, protocol.Item2);

            client.BeginReceive();
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch { }
        }
    }
}
