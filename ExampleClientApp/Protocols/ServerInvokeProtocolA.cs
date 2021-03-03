using ExampleAppModels;
using NoWCF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleClientApp.Protocols
{
    class ServerInvokeProtocolA : IProtocol, IInvokeProtocol
    {
        // serialize & send

        private static readonly string _protocolName = nameof(IProtocol);
        private readonly IConnectionBase _protoBase;

        public ServerInvokeProtocolA(IConnectionBase protoBase)
        {
            _protoBase = protoBase;
        }

        public void CSOp1(int x, int y)
        {
            var method = nameof(CSOp1);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(x), x },
                { nameof(y), y }
            };

            _protoBase.Invoke(_protocolName, method, parameters);
        }

        public void CSOpt2(List<TestClasss> list)
        {
            var method = nameof(CSOpt2);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(list), list },
            };

            _protoBase.Invoke(_protocolName, method, parameters);
        }

        public async Task<int> CSOp3(int z)
        {
            var method = nameof(CSOp3);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(z), z },
            };

            return await _protoBase.InvokeWithResponseAsync<int>(_protocolName, method, parameters);
        }

        public int CSOp4(int z)
        {
            var method = nameof(CSOp4);
            var parameters = new Dictionary<string, object>()
            {
                { nameof(z), z },
            };

            return _protoBase.InvokeWithResponse<int>(_protocolName, method, parameters);
        }

        public int CSSomeExceptionMethod()
        {
            var method = nameof(CSSomeExceptionMethod);
            var parameters = new Dictionary<string, object>();

            return _protoBase.InvokeWithResponse<int>(_protocolName, method, parameters);
        }
    }
}
