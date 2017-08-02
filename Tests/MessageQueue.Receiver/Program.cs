using System;
using System.Threading;
using System.Threading.Tasks;
using MessageQueue.Core.Services;
using MessageQueue.Core.Abstract;
using MessageQueue.Log.NLog.Concrete;
using MessageQueue.Core.Abstract.Inbound;
using MessageQueue.CofigurationProvider.AppSettings.Concrete;

namespace MessageQueue.Receiver
{
    class Program
    {
        #region Private Data Members
        private static int workTimeInMs;
        private static IInboundFaFMq<string> inboundMessageQueue;
        #endregion

        static void Main(string[] args)
        {
            // Setting random working time per message.
            var randomeNumberGenerator = new Random();
            workTimeInMs = randomeNumberGenerator.Next(1, 1000) > 500 ? 1000 : 500;

            Console.WriteLine("Please select the Message Queue (Receiver):" + Environment.NewLine + "1- ZeroMq" + Environment.NewLine + "2- RabbitMq" + Environment.NewLine + "3- ServiceBus");
            var option = Console.ReadKey();

            switch (option.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Test_ZeroMqFaf_Receive();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Test_RabbitMqFaF_Receive();
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    Test_ServiceBusFaF_Receive();
                    break;

                default:
                    Test_ZeroMqFaf_Receive();
                    break;
            }

            Console.ReadKey();
        }

        #region ZeorMq
        public static void Test_ZeroMqFaf_Receive()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("ZeroMq test inbound messages [work time per message {0}ms].", workTimeInMs);

                // Creating queue object from factory.
                // NOTE: it should only be disposed when exiting the program. Otherwise, it will not receive messages.
                inboundMessageQueue = MessagingQueueFactory.CreateInboundFaF<string>(new AppSettingsConfigurationProvider(), "ZeroMqFaFInbound", new NQueueLogger("Default"));
                inboundMessageQueue.OnMessageReady += ZeroMqInboundMessageQueue_OnMessageReady;
                //inboundMessageQueue.OnMessageReadyAsync += ZeroMqInboundMessageQueue_OnMessageReadyAsync;

                // Ready to start.
                Console.WriteLine("Press any key to start receiving...");
                Console.ReadKey();

                // Checking is there any message.
                Console.WriteLine("Queue has message: {0}", inboundMessageQueue.HasMessage());

                // Starting.
                inboundMessageQueue.StartReceivingMessage();

                Console.WriteLine("Started....");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void ZeroMqInboundMessageQueue_OnMessageReady(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (ZeroMq)..." + Environment.NewLine + message + Environment.NewLine);
            Thread.Sleep(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }

        private static async Task ZeroMqInboundMessageQueue_OnMessageReadyAsync(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (ZeroMq)..." + Environment.NewLine + message + Environment.NewLine);
            await Task.Delay(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }
        #endregion

        #region RabbitMq
        public static void Test_RabbitMqFaF_Receive()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("RabbitMq test inbound messages [work time per message {0}ms].", workTimeInMs);

                // Creating queue object from factory.
                inboundMessageQueue = MessagingQueueFactory.CreateInboundFaF<string>(new AppSettingsConfigurationProvider(), "RabbitMqFaFInbound", new NQueueLogger("Default"));
                inboundMessageQueue.OnMessageReady += RabbitMqInboundMessageQueue_OnMessageReady;
                //inboundMessageQueue.OnMessageReadyAsync += RabbitMqInboundMessageQueue_OnMessageReadyAsync;

                // Ready to start.
                Console.WriteLine("Press any key to start receiving...");
                Console.ReadKey();

                // Checking is there any message.
                Console.WriteLine("Queue has message: {0}", inboundMessageQueue.HasMessage());

                // Starting.
                inboundMessageQueue.StartReceivingMessage();

                Console.WriteLine("Started....");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void RabbitMqInboundMessageQueue_OnMessageReady(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (RabbitMq)..." + Environment.NewLine + message + Environment.NewLine);
            Thread.Sleep(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }

        private static async Task RabbitMqInboundMessageQueue_OnMessageReadyAsync(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (RabbitMq)..." + Environment.NewLine + message + Environment.NewLine);
            await Task.Delay(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }
        #endregion

        #region ServiceBus
        public static void Test_ServiceBusFaF_Receive()
        {
            try
            {
                Console.WriteLine("ServiceBus test inbound messages [work time per message {0}ms].", workTimeInMs);

                // Creating queue object from factory.
                inboundMessageQueue = MessagingQueueFactory.CreateInboundFaF<string>(new AppSettingsConfigurationProvider(), "ServiceBusFaFInbound", new NQueueLogger("Default"));
                //inboundMessageQueue.OnMessageReadyAsync += ServiceBusInboundMessageQueue_OnMessageReadyAsync;
                inboundMessageQueue.OnMessageReady += ServiceBusInboundMessageQueue_OnMessageReady;

                // Ready to start.
                Console.WriteLine("Press any key to start receiving...");
                Console.ReadKey();

                // Checking is there any message.
                Console.WriteLine("Queue has message: {0}", inboundMessageQueue.HasMessage());

                // Starting.
                inboundMessageQueue.StartReceivingMessage();

                Console.WriteLine("Started....");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void ServiceBusInboundMessageQueue_OnMessageReady(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (ServiceBus)..." + Environment.NewLine + message + Environment.NewLine);
            Thread.Sleep(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }

        private static async Task ServiceBusInboundMessageQueue_OnMessageReadyAsync(string message, IMessageReceiveOptions messageReceiveOptions)
        {
            Console.WriteLine("Pulled message successfully (ServiceBus)..." + Environment.NewLine + message + Environment.NewLine);

            await Task.Delay(workTimeInMs);

            if (messageReceiveOptions.IsAcknowledgmentConfigured)
            {
                messageReceiveOptions.Acknowledge();
            }
        }
        #endregion
    }
}
