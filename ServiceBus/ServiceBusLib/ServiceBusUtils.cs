using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusLib
{
    public static class ServiceBusUtils
    {
        const string ServiceBusConnectionString = "Endpoint=sb://emkaservicebusapp.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=bqRQKo9+OrUCZunO1z4FQYDerfID/b/ndYglWpZWsxk=";
        const string QueueName = "salesmessages";
        static IQueueClient queueClient;

        static Action<string> action;

        const string TopicName = "GroupMessageTopic";
        const string SubscriptionName = "GroupMessageSubscription";
        static ISubscriptionClient subscriptionClient;

        static Action<string> topicAction;


        public static async Task SendMessage(string message)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
           
            var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
            await queueClient.SendAsync(encodedMessage);

            await queueClient.CloseAsync();            
        }


        public static void ReceiveMessage(Action<string> a)
        {
            action = a;

            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            RegisterMessageHandler();            
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

        public static async Task CloseQueue()
        {
            if (queueClient != null)
                await queueClient.CloseAsync();
        }



        public static async Task SendMessageToTopic(string message)
        {
            var topicClient = new TopicClient(ServiceBusConnectionString, TopicName);            

            var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
            await topicClient.SendAsync(encodedMessage);

            await topicClient.CloseAsync();
        }

        public static void ReceiveMessageFromSubscription(Action<string> a)
        {
            topicAction = a;

            subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);

            RegisterSubscriptionMessageHandler();
        }

        static void RegisterSubscriptionMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(SubscriptionExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            subscriptionClient.RegisterMessageHandler(ProcessSubscriptionMessagesAsync, messageHandlerOptions);            
        }

        static async Task ProcessSubscriptionMessagesAsync(Message message, CancellationToken token)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{body}");

            topicAction?.Invoke(body);

            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task SubscriptionExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public static async Task CloseSubscription()
        {
            if (subscriptionClient != null)
                await subscriptionClient.CloseAsync();
        }
    }
}
