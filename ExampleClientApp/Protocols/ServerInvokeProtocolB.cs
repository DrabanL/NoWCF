using ExampleAppModels;
using NoWCF.Models;
using System.Collections.Generic;

namespace ExampleClientApp.Protocols
{
    class ServerInvokeProtocolB : IProtocol2, IInvokeProtocol
    {
        // serialize & send

        private static readonly string _protocolName = nameof(IProtocol2);
        private readonly IConnectionBase _protoBase;

        public ServerInvokeProtocolB(IConnectionBase protoBase)
        {
            _protoBase = protoBase;
        }

        public void TEST(string str)
        {
            var method = nameof(TEST);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(str), str },
            };

            _protoBase.Invoke(_protocolName, method, parameters);
        }
    }
}
