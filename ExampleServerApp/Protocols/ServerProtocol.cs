using ExampleAppModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleServerApp.Protocols
{
    class ServerProtocol : IProtocol
    {
        private readonly IProtocolCallback _invokeProtocol;

        public ServerProtocol(IProtocolCallback invokeProtocol)
        {
            _invokeProtocol = invokeProtocol;
        }

        // run op

        public int CSSomeExceptionMethod()
        {
            throw new NotImplementedException();
        }

        public int CSOp4(int z)
        {
            Console.WriteLine(z);
            return z;
        }

        public Task<int> CSOp3(int z)
        {
            Console.WriteLine(z);
            return Task.FromResult(z);
        }

        public void CSOp1(int x, int y)
        {
            Console.WriteLine($"IProtoA.CSOp1 {x} {y}");
            _invokeProtocol.SCOp1(y, x);
        }

        public void CSOpt2(List<TestClasss> list)
        {
            Console.WriteLine(JsonConvert.SerializeObject(list));
        }
    }
}
