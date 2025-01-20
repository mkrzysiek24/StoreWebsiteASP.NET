using System;
using System.Collections.Generic;

namespace SneakersPlanet.Models
{
    public class OrderDetailsViewModel
    {
        public string OrderId { get; set; }
        public string UserName { get; set; } 
        public string UserAddress { get; set; }
        public string UserPostal { get; set; }
        public string UserCity { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalValue { get; set; }
        public List<OrderItemDetails> Items { get; set; }
    }

    public class OrderItemDetails
    {
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
