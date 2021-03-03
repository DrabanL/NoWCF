using NoWCF.Models;
using NoWCF.Utilities;
using System;
using System.Net.Sockets;

namespace NoWCF
{
    public class NoWCFClient : ConnectionBase
    {
        /// <summary>
        /// Client Mode
        /// </summary>
        public NoWCFClient(NoWCFSettings settings = null) : base(RandomString.Next(16), settings)
        { }

        /// <summary>
        /// Client Mode
        /// </summary>
        public void Connect(string host, int port)
        {
            if (Connected)
                throw new Exception("Already connected.");

            if (Disconnected)
                throw new Exception("Already used up. You should create a new instance.");

            var socket = _setings.SocketFactory();

            try
            {
                socket.Connect(host, port);
            }
            catch
            {
                socket?.Dispose();
                throw;
            }

            Console.WriteLine($"Connected ({host}:{port})");

            _socket = socket;
            _networkStream = new NetworkStream(socket, false);
        }

        /// <summary>
        /// Server Mode
        /// </summary>
        public NoWCFClient(Socket socket, NoWCFSettings settings) : base(settings)
        {
            Console.WriteLine($"Connected ({socket.RemoteEndPoint})");

            _socket = socket;
            _networkStream = new NetworkStream(socket, false);
        }
    }
}
