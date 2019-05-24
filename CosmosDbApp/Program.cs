using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbApp
{
    class Program
    {
        //partition key: /productId

        //const string endpoint = "https://emkacosmos.documents.azure.com:443/";
        //const string masterKey = "awj0NO2jHEbb1ksQ8btt1Hqs2nV21bjBQBOUp1KrpBqyK27QXl9StgDrM1ZpVnkns0mBEFRoHJaDQCAt6xdHyg==";

        const string endpoint = "https://localhost:8081";
        const string masterKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static Uri ProductsCollectionUri => UriFactory.CreateDocumentCollectionUri("Products", "Products");

        static async Task Main(string[] args)
        {
            using(var client = new DocumentClient(new Uri(endpoint), masterKey))
            {
                //create document 1
                dynamic document1Definition = new
                {                    
                    productId = "5903205189820",
                    category = "Gadżety",
                    name = "Zestaw Kamerzysty",
                    price = "39.90",
                    shipping = new
                    {
                        weight = 2,
                        dimensions = new
                        {
                            width = 50,
                            height = 50,
                            depth = 2
                        }
                    }
                };

                var document1 = await client.CreateDocumentAsync(ProductsCollectionUri, document1Definition);

                //create document 2
                var document2Definition = @"
                {
                    ""productId"": ""5903205189622"",
                    ""category"": ""Gadżety"",
                    ""name"": ""Kubek Kamerzysta"",   
                    ""price"": ""29.90"",
                    ""shipping"": {
                        ""weight"": 1,
                        ""dimensions"": {
                            ""width"": 6,
                            ""height"": 6,
                            ""depth"": 10
                        }
                    }
                }";

                var document2Object = JsonConvert.DeserializeObject(document2Definition);
                var document2 = await client.CreateDocumentAsync(ProductsCollectionUri, document2Object);

                //create document 3
                var document3Definition = new Product
                {
                    ProductId = "5903205189723",
                    Category = "Koszulki",
                    Name = "T-shirt Kamerzysta",
                    Price = "59.00",
                    Shipping = new Shipping
                    {
                        Weight = 1,
                        Dimensions = new Dimensions
                        {
                            Width = 50,
                            Height = 50,
                            Depth = 3
                        }
                    }
                };

                var document3 = await client.CreateDocumentAsync(ProductsCollectionUri, document3Definition);


                //query 1
                var sql = "SELECT * FROM c WHERE c.productId = '5903205189820'";
                var query = client.CreateDocumentQuery<Document>(ProductsCollectionUri, sql);
                var result = query.ToList();

                //query 2
                sql = "SELECT * FROM c WHERE c.shipping.dimensions.width = 50";
                var options = new FeedOptions { EnableCrossPartitionQuery = true };
                var query2 = client.CreateDocumentQuery<Product>(ProductsCollectionUri, sql, options);
                var result2 = query2.ToList();

                //query 3
                sql = "SELECT * FROM c";
                var options3 = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 2 };
                var query3 = client
                    .CreateDocumentQuery(ProductsCollectionUri, sql, options3)
                    .AsDocumentQuery();

                while (query3.HasMoreResults)
                {
                    var documents = await query3.ExecuteNextAsync();
                }

                //query 4
                var query4 =
                    from p in client.CreateDocumentQuery<Product>(ProductsCollectionUri, options)
                    where p.Shipping.Dimensions.Width == 50
                    select new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Width = p.Shipping.Dimensions.Width
                    };
                var documents4 = query4.ToList();


                //replace document
                sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'Kubek')";
                var query5 = client.CreateDocumentQuery(ProductsCollectionUri, sql, options);
                var document = query5.ToList().First();

                document.specialEdition = true;

                var result5 = await client.ReplaceDocumentAsync(document._self, document);


                //delete document
                sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'Zestaw')";
                var query6 = client.CreateDocumentQuery(ProductsCollectionUri, sql, options);
                var documentToRemove = query6.ToList().First();

                var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(documentToRemove.productId) };
                await client.DeleteDocumentAsync(documentToRemove._self, requestOptions);
            }

            Console.WriteLine("Hello World!");
        }       
    }
}
