using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionApp2
{
    public static class QueueListeners
    {
        [FunctionName("QueueListeners")]
        public static async Task Run([QueueTrigger("products", Connection = "AzureWebJobsStorage")]Product product, 
            [Blob("products", Connection = "AzureWebJobsStorage")]CloudBlobContainer container,
            ILogger log)
        {
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference($"{product.Id}.txt");
            await blob.UploadTextAsync($"Created a new product: {product.Name}");

            log.LogInformation($"C# Queue trigger function processed: {product.Name}");
        }
    }
}
