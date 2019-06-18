using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Model;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet(Name = "GetProducts")]
        public IEnumerable<ProductItem> Get()
        {
            return Config.GetProductItems();            
        }

        [HttpGet("{id}", Name = "GetProductDetails")]
        public ProductItem Get(int id)
        {
            var items = Config.GetProductItems();

            return items.SingleOrDefault(ci => ci.Id == id);            
        }
    }
}