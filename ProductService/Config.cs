﻿using ProductService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService
{
    public static class Config
    {
        public static IEnumerable<ProductItem> GetProductItems()
        {
            return new List<ProductItem>
            {
                new ProductItem
                {
                    Id = 1,
                    Name = ".NET Core microservices",
                    StockQuantity = 5000,
                    Price = 200
                },
                new ProductItem
                {
                    Id = 2,
                    Name = "Angular 7 progressive web app",
                    StockQuantity = 6000,
                    Price = 100
                }
            };
        }
    }
}
