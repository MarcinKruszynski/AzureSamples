using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusLib
{
    public static class ServiceBusUtils
    {
        const string ServiceBusConnectionString = "";
        const string QueueName = "salesmessages";
        static IQueueClient queueClient;

        static Action<string> action;

        public static async Task SendMessage(string message)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
           
            var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
            await queueClient.SendAsync(encodedMessage);

            await queueClient.CloseAsync();
        }


        public static async Task ReceiveMessage(Action<string> a)
        {
            action = a;

            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            RegisterMessageHandler();

            await queueClient.CloseAsync();
        }

        static void RegisterMessageHandler()
        {            
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {                
                MaxConcurrentCalls = 1,                
                AutoComplete = false
            };
            
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{body}");

            action?.Invoke(body);

            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
