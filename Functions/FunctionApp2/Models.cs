using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp2
{
    public class Product
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Price { get; set; }

        public int StockQuantity { get; set; }
    }

    public class ProductData
    {
        public string Name { get; set; }

        public string Price { get; set; }

        public int StockQuantity { get; set; }
    }

    public class ProductTableEntity: TableEntity
    {
        public string Name { get; set; }

        public string Price { get; set; }

        public int StockQuantity { get; set; }
    }
}
