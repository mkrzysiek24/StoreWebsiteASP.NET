using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SneakersPlanet.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; }

        [BsonElement("totalValue")]
        public decimal TotalValue { get; set; }

        [BsonElement("orderDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime OrderDate { get; set; }
    }

    public class OrderItem
    {
        [BsonElement("sneakerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SneakerId { get; set; }

        [BsonElement("size")]
        public string Size { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}
