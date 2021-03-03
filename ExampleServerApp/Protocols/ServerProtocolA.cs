using ExampleAppModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleServerApp.Protocols
{
    class ServerProtocolA : IProtocol
    {
        private readonly IProtocolCallback _invokeProtocol;

        public ServerProtocolA(IProtocolCallback invokeProtocol)
        {
            _invokeProtocol = invokeProtocol;
        }

        // run op

        public int CSSomeExceptionMethod()
        {
            Console.WriteLine("ServerProtocolA.CSSomeExceptionMethod()");
            throw new NotImplementedException();
        }

        public int CSOp4(int z)
        {
            Console.WriteLine($"ServerProtocolA.CSOp4({z})");
            return z;
        }

        public Task<int> CSOp3(int z)
        {
            Console.WriteLine($"ServerProtocolA.CSOp3({z})");
            return Task.FromResult(z);
        }

        public void CSOp1(int x, int y)
        {
            Console.WriteLine($"ServerProtocolA.CSOp1({x}, {y})");
            _invokeProtocol.SCOp1(y, x);
        }

        public void CSOpt2(List<TestClasss> list)
        {
            Console.WriteLine($"ServerProtocolA.CSOp1({JsonConvert.SerializeObject(list)})");
        }
    }
}
