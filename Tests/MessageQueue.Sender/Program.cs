using System;
using System.Threading;
using MessageQueue.Core.Services;
using MessageQueue.Log.NLog.Concrete;
using MessageQueue.CofigurationProvider.AppSettings.Concrete;

namespace MessageQueue.Sender
{
    class Program
    {
        #region Private Data Members
        private const ushort messageCount = 25;
        private const ushort delayBetweenMessagesInMs = 200;
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Please select the Message Queue (Sender):" + Environment.NewLine + "1- ZeroMq" + Environment.NewLine + "2- RabbitMq" + Environment.NewLine + "3- ServiceBus");
            var option = Console.ReadKey();

            switch (option.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Test_ZeroMqFaF_Send();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Test_RabbitMqFaF_Send();
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    Test_ServiceBusFaF_Send();
                    break;

                default:
                    Test_ZeroMqFaF_Send();
                    break;
            }

            Console.ReadKey();
        }

        #region ZeroMq
        public static async void Test_ZeroMqFaF_Send()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("ZeroMq test outbound messages... (press any key to start sending)");
                Console.ReadKey();
                Console.WriteLine("Started (message count: {0})....", messageCount);

                using (var outboundMessageQueue = MessagingQueueFactory.CreateOutboundFaF<string>(new AppSettingsConfigurationProvider(), "ZeroMqFaFOutbound", new NQueueLogger("Default")))
                {
                    for (int i = 0; i < messageCount; i++)
                    {
                        var message = $"This is FAF message number: {i} (sent at: {DateTime.Now.ToLongTimeString()})";

                        Console.WriteLine("Pushing: " + Environment.NewLine + message);

                        outboundMessageQueue.SendMessage(message);
                        //await outboundMessageQueue.SendMessageAsync(message);

                        Console.WriteLine("Pushed successfully..." + Environment.NewLine);

                        // Adding delay.
                        Thread.Sleep(delayBetweenMessagesInMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region RabbitMq
        public static async void Test_RabbitMqFaF_Send()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("RabbitMq test outbound messages (press any key to start sending)...");
                Console.ReadKey();
                Console.WriteLine("Started (message count: {0})....", messageCount);

                // Creating queue object from factory.
                using (var outboundMessageQueue = MessagingQueueFactory.CreateOutboundFaF<string>(new AppSettingsConfigurationProvider(), "RabbitMqFaFOutbound", new NQueueLogger("Default")))
                {
                    for (int i = 0; i < messageCount; i++)
                    {
                        var message = $"This is FAF message number: {i} (sent at: {DateTime.Now.ToLongTimeString()})";

                        Console.WriteLine("Pushing: " + Environment.NewLine + message);

                        outboundMessageQueue.SendMessage(message);
                        //await outboundMessageQueue.SendMessageAsync(message);

                        Console.WriteLine("Pushed successfully..." + Environment.NewLine);

                        Thread.Sleep(delayBetweenMessagesInMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region ServiceBus
        public static async void Test_ServiceBusFaF_Send()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("ServiceBus test outbound messages (press any key to start sending)...");
                Console.ReadKey();
                Console.WriteLine("Started (message count: {0})....", messageCount);

                // Creating queue object from factory.
                using (var outboundMessageQueue = MessagingQueueFactory.CreateOutboundFaF<string>(new AppSettingsConfigurationProvider(), "ServiceBusFaFOutbound", new NQueueLogger("Default")))
                {
                    for (int i = 0; i < messageCount; i++)
                    {
                        var message = $"This is FAF message number: {i} (sent at: {DateTime.Now.ToLongTimeString()})";

                        Console.WriteLine("Pushing: " + Environment.NewLine + message);

                        outboundMessageQueue.SendMessage(message);
                        //await outboundMessageQueue.SendMessageAsync(message);

                        Console.WriteLine("Pushed successfully..." + Environment.NewLine);

                        Thread.Sleep(delayBetweenMessagesInMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion
    }
}
