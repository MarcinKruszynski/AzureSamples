using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventHubLib
{
    public class SimpleEventProcessor : IEventProcessor
    {
        public static Action<string> action;        

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {            
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {            
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {            
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                action?.Invoke(data);
            }

            return context.CheckpointAsync();
        }
    }
}
