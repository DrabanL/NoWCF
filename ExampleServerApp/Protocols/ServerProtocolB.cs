using ExampleAppModels;
using System;

namespace ExampleServerApp.Protocols
{
    class ServerProtocolB : IProtocol2
    {
        public void TEST(string str)
        {
            Console.WriteLine($"ServerProtocolB.TEST: {str}");
        }
    }
}
