using System;
using System.Collections.Generic;
using System.Threading;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Services;
using MessageQueue.Log.NLog.Concrete;
using MessageQueue.Core.Abstract.Inbound;
using MessageQueue.CofigurationProvider.AppSettings.Concrete;

namespace MessageQueue.RaR.Server
{
    class Program
    {
        #region Private Data Members
        private static int workTimeInMs = 0;
        private static IInboundRaRMq<string, string> inboundMessageQueue;
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Please select the Message Queue (Server):" + Environment.NewLine + "1- ZeroMq" + Environment.NewLine + "2- RabbitMq");
            var option = Console.ReadKey();

            switch (option.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Test_ZeroMqRaR();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Test_RabbitMqRaR();
                    break;

                default:
                    Test_ZeroMqRaR();
                    break;
            }

            Console.ReadKey();
        }

        #region ZeroMq
        public static void Test_ZeroMqRaR()
        {
            try
            {
                Console.WriteLine("ZeroMq test inbound messages (server) [work time per message {0}ms].", workTimeInMs);
                Console.WriteLine(nameof(Test_ZeroMqRaR));

                // Creating queue object from factory.
                inboundMessageQueue = MessagingQueueFactory.CreateInboundRaR<string, string>(new AppSettingsConfigurationProvider(), "ZeroMqRaRServer", new NQueueLogger("Default"));
                inboundMessageQueue.OnRequestReady += ZeroMqInboundMessageQueue_OnRequestReady;

                Console.WriteLine("Press any key to start receiving...");
                Console.ReadKey();

                inboundMessageQueue.StartReceivingRequest();

                Console.WriteLine("Server is ready and listening at: {0}", inboundMessageQueue.Address);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void ZeroMqInboundMessageQueue_OnRequestReady(RequestMessage<string, string> request)
        {
            Console.WriteLine("Request received: " + Environment.NewLine + request.RequestData);

            Thread.Sleep(workTimeInMs);

            var message = string.Format("You sent me: '{0}' (received at: {1})", request.RequestData, DateTime.Now.ToLongTimeString());
            request.Response(message);
            Console.WriteLine("Response sent: " + Environment.NewLine + message);
            Console.WriteLine("------------------------------------");
        }
        #endregion

        #region RabbitMq
        public static void Test_RabbitMqRaR()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("RabbitMq test inbound messages (server) [work time per message {0}ms].", workTimeInMs);

                // Creating queue object from factory.
                inboundMessageQueue = MessagingQueueFactory.CreateInboundRaR<string, string>(new AppSettingsConfigurationProvider(), "RabbitMqRaRServer", new NQueueLogger("Default"));
                inboundMessageQueue.OnRequestReady += RabbitMqInboundMessageQueue_OnRequestReady;

                Console.WriteLine("Press any key to start receiving...");
                Console.ReadKey();

                inboundMessageQueue.StartReceivingRequest();

                Console.WriteLine("Server is ready and listening at: {0}", inboundMessageQueue.Address);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void RabbitMqInboundMessageQueue_OnRequestReady(RequestMessage<string, string> request)
        {
            Console.WriteLine("Request received: " + Environment.NewLine + request.RequestData);

            Thread.Sleep(workTimeInMs);

            var message = string.Format("You sent me: '{0}' (received at: {1})", request.RequestData, DateTime.Now.ToLongTimeString());
            request.Response(message);
            Console.WriteLine("Response sent: " + Environment.NewLine + message);
            Console.WriteLine("------------------------------------");
        }
        #endregion
    }
}
