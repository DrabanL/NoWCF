
using ExampleAppModels;
using System;

namespace ExampleClientApp.Protocols
{
    class ClientProtocol : IProtocolCallback
    {
        private readonly IProtocol _serverInvoke;

        public ClientProtocol(IProtocol serverInvoke)
        {
            _serverInvoke = serverInvoke;
        }

        // run op

        public void SCOp1(int x, int y)
        {
            Console.WriteLine($"ClientProtocol.SCOp1({x}, {y})");
        }

    }
}
