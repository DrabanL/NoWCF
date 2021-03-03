using ExampleAppModels;
using ExampleClientApp.Protocols;
using NoWCF;
using NoWCF.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ExampleClientApp
{
    class Program
    {
        static Random rand = new Random();

        static void Main(string[] args)
        {
            var nowcfSettings = new NoWCFSettings()
            {
                //FindEncryptionCertificateSubjectName = "MyCustomCertificate",
                BasicCommunicationEncryptionKey = "jshgntjh43t",                   
            }.InitializeEncryption();


            Console.Write("Number of connections to spawn: ");
            var connCnt = int.Parse(Console.ReadLine());
            Console.WriteLine();

            for (int i = 0; i < connCnt; ++i)
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        var client = new NoWCFClient(nowcfSettings)
                        {
                            HandleException = (E) => Console.WriteLine(E),
                            HandleConnectionClosed = (cl) =>
                            {
                                using (cl)
                                    Console.WriteLine("CLOSED");
                            }
                        };

                        try
                        {
                            client.Connect("127.0.0.1", 8866);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);

                            Thread.Sleep(1000);
                            continue;
                        }

                        client.RegisterDuplexProtocol<ClientProtocol, ServerInvokeProtocolA>();
                        client.RegisterInvokeProtocol<ServerInvokeProtocolB>();
                        client.BeginReceive();

                        while (true)
                        {
                            client.GetProtocol<ClientProtocol, ServerInvokeProtocolA>()?.CSOp1(rand.Next(0, 999999), 8);
                            client.GetProtocol<ServerInvokeProtocolB>()?.TEST("HAHAHA");
                            client.GetProtocol<ClientProtocol, ServerInvokeProtocolA>()?.CSOpt2(new List<TestClasss>()
                            {
                                new TestClasss()
                                {
                                    Member2 = 255,
                                    Member = new List<TestClass2>()
                                    {
                                        { new TestClass2() { Mem = 669 } },
                                        { new TestClass2() { Mem = 670 } }
                                    }
                                },
                                new TestClasss()
                                {
                                    Member2 = 255,
                                    Member = new List<TestClass2>()
                                    {
                                        { new TestClass2(){ Mem = 669 } },
                                        { new TestClass2(){ Mem = 670 } }
                                    }
                                }
                            });
                            Console.WriteLine($"ServerInvokeProtocolA.CSOp4 result: {client.GetProtocol<ClientProtocol, ServerInvokeProtocolA>()?.CSOp4(rand.Next(0, 10000))}");
                            try
                            {
                                client.GetProtocol<ClientProtocol, ServerInvokeProtocolA>().CSSomeExceptionMethod();
                            }
                            catch (Exception ex) { Console.WriteLine(ex); }

                            Thread.Sleep(750);

                            if (rand.Next(0, 10) == 0 || !client.Connected)
                                break;
                        }

                        client.Disconnect();

                    }
                }).Start();
            }
        }
    }
}
