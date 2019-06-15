using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventHubLib
{
    public static class EventHubUtils
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "";
        private const string EventHubName = "emkaeventhub";

        private const string StorageContainerName = "messages";
        private const string StorageAccountName = "emkastorage";
        private const string StorageAccountKey = "";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        private static EventProcessorHost eventProcessorHost;

        public static async Task SendMessages()
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub(100);

            await eventHubClient.CloseAsync();
        }

        
        private static async Task SendMessagesToEventHub(int numMessagesToSend)
        {
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    var message = $"Message {i}";                    
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch(Exception ex) 
                {                    
                }

                await Task.Delay(10);
            }            
        }


        public static async Task ReadMessages(Action<string> a)
        {
            eventProcessorHost = new EventProcessorHost(
               EventHubName,
               PartitionReceiver.DefaultConsumerGroupName,
               EventHubConnectionString,
               StorageConnectionString,
               StorageContainerName);

            SimpleEventProcessor.action = a;

            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();            
        }

        public static async Task UnregisterEventProcessor()
        {
            if (eventProcessorHost != null)
                await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
