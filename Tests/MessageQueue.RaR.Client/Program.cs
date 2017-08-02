using System;
using System.Threading;
using MessageQueue.Core.Services;
using MessageQueue.Log.NLog.Concrete;
using MessageQueue.CofigurationProvider.AppSettings.Concrete;

namespace MessageQueue.RaR.Client
{
    class Program
    {
        #region Private Data Members
        private const int messageCount = 10;
        private static int clientNumber;
        private static int gapeBetweenMessagesInMs = 0;
        #endregion

        static void Main(string[] args)
        {
            // Setting random gape time between messages.
            var randomeNumberGenerator = new Random();
            gapeBetweenMessagesInMs = randomeNumberGenerator.Next(1, 1000) > 500 ? 100 : 200;

            // Setting client number.
            var rnd = new Random();
            clientNumber = rnd.Next(1, 10000);

            Console.WriteLine("Please select the Message Queue (Client):" + Environment.NewLine + "1- ZeroMq" + Environment.NewLine + "2- RabbitMq");
            var option = Console.ReadKey();

            switch (option.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Test_ZeroMqRaR_Request();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Test_RabbitMqRaR_Request();
                    break;

                default:
                    Test_ZeroMqRaR_Request();
                    break;
            }

            Console.ReadKey();
        }

        #region ZeroMq
        public static void Test_ZeroMqRaR_Request()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("ZeroMq test outbound messages (client)... (press any key to start sending)");
                Console.ReadKey();
                Console.WriteLine("Started (client Id: {0}, message count: {1}, gape between messages: {2}ms)....", clientNumber, messageCount, gapeBetweenMessagesInMs);

                // Creating queue object from factory.
                var outboundMessageQueue = MessagingQueueFactory.CreateOutboundRaR<string, string>(new AppSettingsConfigurationProvider(), "ZeroMqRaRClient", new NQueueLogger("Default"));
                outboundMessageQueue.OnResponseReady += ZeroMqOutboundMessageQueue_OnResponseReady;

                for (int i = 1; i <= messageCount; i++)
                {
                    var message = string.Format("RAR message number: {0} client {1} (sent at: {2})", i,
                        clientNumber, DateTime.Now.ToLongTimeString());

                    Console.WriteLine("Requesting: " + Environment.NewLine + message);
                    outboundMessageQueue.SendRequest(message);

                    Thread.Sleep(gapeBetweenMessagesInMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void ZeroMqOutboundMessageQueue_OnResponseReady(string responseData)
        {
            Console.WriteLine("Reply from server:" + Environment.NewLine + responseData);
            Console.WriteLine("------------------------------");
        }
        #endregion

        #region RabbitMq
        public static void Test_RabbitMqRaR_Request()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("RabbitMq test outbound messages (client)... (press any key to start sending)");
                Console.ReadKey();
                Console.WriteLine("Started (client Id: {0}, message count: {1}, gape between messages: {2}ms)....",
                    clientNumber, messageCount, gapeBetweenMessagesInMs);

                // Creating queue object from factory.
                var outboundMessageQueue = MessagingQueueFactory.CreateOutboundRaR<string, string>(new AppSettingsConfigurationProvider(),
                    "RabbitMqRaRClient", new NQueueLogger("Default"));
                outboundMessageQueue.OnResponseReady += RabbitMqOutboundMessageQueue_OnResponseReady;

                for (int i = 1; i <= messageCount; i++)
                {
                    var message = string.Format("RAR message number: {0} client {1} (sent at: {2})", i, clientNumber,
                        DateTime.Now.ToLongTimeString());

                    Console.WriteLine("Requesting: " + Environment.NewLine + message);
                    outboundMessageQueue.SendRequest(message);

                    Thread.Sleep(gapeBetweenMessagesInMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void RabbitMqOutboundMessageQueue_OnResponseReady(string responseData)
        {
            Console.WriteLine("Reply from server:" + Environment.NewLine + responseData);
            Console.WriteLine("------------------------------");
        }
        #endregion
    }
}
