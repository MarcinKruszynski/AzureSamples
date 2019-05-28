using System;
using System.Collections.Generic;
using System.Text;

namespace RedisCacheWebApp.Model
{
    public class CustomerBasket
    {
        public string BuyerId { get; set; }
        public DateTime Date { get; set; }
        public List<BasketItem> Items { get; set; }

        public CustomerBasket(string customerId)
        {
            BuyerId = customerId;
            Items = new List<BasketItem>();
        }
    }
}
