using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisCacheWebApp.Model;

namespace RedisCacheWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IDistributedCache _cache;

        public ValuesController(IDistributedCache cache)
        {
            _cache = cache;
        }


        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<CustomerBasket>> Get()
        {
            List<CustomerBasket> baskets;

            var cachedBaskets = _cache.GetString("baskets");

            if (!string.IsNullOrEmpty(cachedBaskets))
            {
                baskets = JsonConvert.DeserializeObject<List<CustomerBasket>>(cachedBaskets);
            }
            else
            {
                //database
                baskets = new List<CustomerBasket>
                {
                    new CustomerBasket("144")
                    {
                        Date = DateTime.Now,
                        Items = new List<BasketItem>
                        {
                            new BasketItem { Id = "1", ProductId = "26", ProductName = "Kubek Kamerzysta", UnitPrice = 29.9m, Quantity = 2 },
                            new BasketItem { Id = "2", ProductId = "33", ProductName = "T-shirt Kamerzysta", UnitPrice = 59m, Quantity = 1 }
                        }
                    }                    
                };

                _cache.SetString("baskets", JsonConvert.SerializeObject(baskets));
            }

            return Ok(baskets);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
