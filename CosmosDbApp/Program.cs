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

        const string endpoint = "";
        const string masterKey = "";

        //const string endpoint = "";
        //const string masterKey = "";

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

                var options1 = new RequestOptions { PreTriggerInclude = new[] { "trgValidateProduct" } };

                try
                {
                    var document1 = await client.CreateDocumentAsync(ProductsCollectionUri, document1Definition, options1);
                }
                catch(DocumentClientException ex)
                {
                }

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
                //sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'Kubek')";
                sql = "SELECT * FROM c WHERE udf.udfRegEx(c.name, 'Kubek') != null";
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


                //stored procedure 1
                var uri7 = UriFactory.CreateStoredProcedureUri("Products", "Products", "spHelloWorld");
                var options7 = new RequestOptions { PartitionKey = new PartitionKey(string.Empty) };
                var result7 = await client.ExecuteStoredProcedureAsync<string>(uri7, options7);
                var message7 = result7.Response;

                //stored procedure 2
                dynamic documentData = new
                {
                    productId = "8903205189820",
                    category = "Gadżety",
                    name = "Zestaw Premium",
                    price = "99.90",
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
                var uri8 = UriFactory.CreateStoredProcedureUri("Products", "Products", "spAddProduct");
                var options8 = new RequestOptions { PartitionKey = new PartitionKey("8903205189820") };
                var result8 = await client.ExecuteStoredProcedureAsync<object>(uri8, options8, documentData);
                var doc8 = result8.Response;
                var doc8Object = JsonConvert.DeserializeObject(doc8.ToString());
                var do8Name = doc8Object["name"];
            }

            Console.WriteLine("Hello World!");
        }       
    }
}
