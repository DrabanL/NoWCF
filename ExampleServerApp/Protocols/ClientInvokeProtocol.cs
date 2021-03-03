using ExampleAppModels;
using NoWCF.Models;
using System.Collections.Generic;

namespace ExampleServerApp.Protocols
{
    class ClientInvokeProtocol : IProtocolCallback, IInvokeProtocol
    {
        // serialize & send

        private static readonly string _protocolName = nameof(IProtocolCallback);
        private readonly IConnectionBase _protoBase;

        public ClientInvokeProtocol(IConnectionBase protoBase)
        {
            _protoBase = protoBase;
        }

        public void SCOp1(int x, int y)
        {
            var method = nameof(SCOp1);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(x), x },
                { nameof(y), y }
            };

            _protoBase.Invoke(_protocolName, method, parameters);
        }
    }
}
