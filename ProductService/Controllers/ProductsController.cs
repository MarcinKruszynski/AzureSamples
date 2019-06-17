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
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IEnumerable<ProductItem>), (int)HttpStatusCode.OK)]
        public IActionResult Get()
        {
            var items = Config.GetProductItems();

            return Ok(items);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProductItem), (int)HttpStatusCode.OK)]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var items = Config.GetProductItems();

            var item = items.SingleOrDefault(ci => ci.Id == id);
            if (item != null)
            {
                return Ok(item);
            }

            return NotFound();
        }
    }
}