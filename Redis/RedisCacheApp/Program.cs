using ServiceStack.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisCacheApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "";

            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                IDatabase db = cache.GetDatabase();


                bool setValue = await db.StringSetAsync("test:key", "aaa");
                Console.WriteLine($"SET: {setValue}");

                string getValue = await db.StringGetAsync("test:key");
                Console.WriteLine($"GET: {getValue}");


                setValue = await db.StringSetAsync("counter", "100");
                Console.WriteLine($"SET: {setValue}");

                long newValue = await db.StringIncrementAsync("counter", 50);
                Console.WriteLine($"INCR new value = {newValue}");

                getValue = await db.StringGetAsync("counter");
                Console.WriteLine($"GET: {getValue}");


                var basket = new CustomerBasket("144");
                basket.Items.Add(new BasketItem { Id = "1", ProductId = "26", ProductName = "Kubek Kamerzysta", UnitPrice = 29.9m, Quantity = 2 });
                basket.Items.Add(new BasketItem { Id = "2", ProductId = "33", ProductName = "T-shirt Kamerzysta", UnitPrice = 59m, Quantity = 1 });


                string serializedValue = Newtonsoft.Json.JsonConvert.SerializeObject(basket);
                bool added = await db.StringSetAsync("basket:144-26:2-33:1", serializedValue);

                var cbStr = await db.StringGetAsync("basket:144-26:2-33:1");
                var cb = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerBasket>(cbStr.ToString());
                Console.WriteLine(cb.BuyerId); 


                var result = await db.ExecuteAsync("ping");
                Console.WriteLine($"PING = {result.Type} : {result}");
            }


            //using (var redisClient = new RedisClient(""))
            //{                
            //    string key = "test:expire";
                
            //    redisClient.SetValue(key, "bbb");
               
            //    redisClient.Expire(key, 15);
            //}

            Console.WriteLine("Hello World!");
        }
    }
}
