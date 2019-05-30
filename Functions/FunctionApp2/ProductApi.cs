using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace FunctionApp2
{
    public static class ProductApi
    {
        
        [FunctionName("AddProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req, 
            [Table("products", Connection = "AzureWebJobsStorage")] IAsyncCollector<ProductTableEntity> productTable,
            [Queue("products", Connection = "AzureWebJobsStorage")] IAsyncCollector<Product> productQueue,
            ILogger log)
        {
            log.LogInformation("Adding a new product.");  

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<ProductData>(requestBody);

            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = data.Name,
                Price = data.Price,
                StockQuantity = data.StockQuantity
            };

            await productTable.AddAsync(new ProductTableEntity
            {
                PartitionKey = "PRODUCT",
                RowKey = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            });

            await productQueue.AddAsync(product);

            return new OkObjectResult(product);
        }


        [FunctionName("GetProducts")]
        public static async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
            [Table("products", Connection = "AzureWebJobsStorage")] CloudTable productTable,
            ILogger log)
        {
            log.LogInformation("Getting product list.");

            var query = new TableQuery<ProductTableEntity>();
            var segment = await productTable.ExecuteQuerySegmentedAsync(query, null);
            
            return new OkObjectResult(segment
                .Select(item => new Product
                {
                    Id = item.RowKey,
                    Name = item.Name,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity
                }));
        }


        [FunctionName("GetProduct")]
        public static IActionResult GetProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequest req,
            [Table("products", "PRODUCT", "{id}", Connection = "AzureWebJobsStorage")] ProductTableEntity product,
            ILogger log, string id)
        {
            log.LogInformation("Getting product by id.");            

            if (product == null)
                return new NotFoundResult();

            return new OkObjectResult(new Product
            {
                Id = product.RowKey,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            });
        }


        [FunctionName("UpdateProduct")]
        public static async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequest req,
            [Table("products", Connection = "AzureWebJobsStorage")] CloudTable productTable,
            ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ProductData>(requestBody);
            var findOperation = TableOperation.Retrieve<ProductTableEntity>("PRODUCT", id);
            var findResult = await productTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (ProductTableEntity)findResult.Result;
            existingRow.Name = data.Name;
            existingRow.Price = data.Price;
            existingRow.StockQuantity = data.StockQuantity;

            var replaceOperation = TableOperation.Replace(existingRow);
            await productTable.ExecuteAsync(replaceOperation);            

            return new OkObjectResult(new Product
            {
                Id = existingRow.RowKey,
                Name = existingRow.Name,
                Price = existingRow.Price,
                StockQuantity = existingRow.StockQuantity
            });
        }


        [FunctionName("DeleteProduct")]
        public static async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequest req,
            [Table("products", Connection = "AzureWebJobsStorage")] CloudTable productTable,
            ILogger log, string id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity
            {
                PartitionKey = "PRODUCT",
                RowKey = id,
                ETag = "*"
            });

            try
            {
                var deleteResult = await productTable.ExecuteAsync(deleteOperation);
            }
            catch(StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }            

            return new OkResult();
        }
    }
}
