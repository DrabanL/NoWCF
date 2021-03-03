using ExampleServerApp.Protocols;
using NoWCF;
using NoWCF.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ExampleServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var nowcfSettings = new NoWCFSettings()            
            {
                //FindEncryptionCertificateSubjectName = "MyCustomCertificate",
                BasicCommunicationEncryptionKey = "jshgntjh43t",                 
            }.InitializeEncryption();

            var server = new NoWCFServer(8866, nowcfSettings);
            server.HandleException = (e) => Console.WriteLine(e);
            server.AddDuplexProtocol<ServerProtocolA, ClientInvokeProtocol>();
            server.AddProtocol<ServerProtocolB>();
            while (true)
            {
                Console.WriteLine("Starting server..");
                server.Start();
                Console.WriteLine("Server started.");
                Console.ReadKey();
                Console.WriteLine("Stopping server..");
                server.Stop();
            }
        }
    }
}
