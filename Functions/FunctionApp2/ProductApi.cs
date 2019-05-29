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

namespace FunctionApp2
{
    public static class ProductApi
    {
        //temp
        static List<Product> items = new List<Product>();


        [FunctionName("AddProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req, ILogger log)
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

            //temp
            items.Add(product);

            return new OkObjectResult(product);
        }


        [FunctionName("GetProducts")]
        public static IActionResult GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting product list.");

            //temp
            return new OkObjectResult(items);
        }


        [FunctionName("GetProduct")]
        public static IActionResult GetProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequest req, ILogger log, string id)
        {
            //temp
            var product = items.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return new NotFoundResult();

            return new OkObjectResult(product);
        }


        [FunctionName("UpdateProduct")]
        public static async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequest req, ILogger log, string id)
        {
            //temp
            var product = items.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return new NotFoundResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<ProductData>(requestBody);

            product.Name = data.Name;
            product.Price = data.Price;
            product.StockQuantity = data.StockQuantity;

            return new OkObjectResult(product);
        }


        [FunctionName("DeleteProduct")]
        public static IActionResult DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequest req, ILogger log, string id)
        {
            //temp
            var product = items.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return new NotFoundResult();

            //temp
            items.Remove(product);

            return new OkResult();
        }
    }
}
