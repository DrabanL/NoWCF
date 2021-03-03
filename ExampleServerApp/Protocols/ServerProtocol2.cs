using ExampleAppModels;
using System;

namespace ExampleServerApp.Protocols
{
    class ServerProtocol2 : IProtocol2
    {
        public void TEST(string str)
        {
            Console.WriteLine($"TEST: {str}");
        }
    }
}
