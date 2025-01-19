using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SneakersPlanet.Models
{
    [BsonIgnoreExtraElements]
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("slug")]
        public string Slug { get; set; }

        [BsonElement("imageURL")]
        public string ImageURL { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("brand")]
        public string Brand { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("sizes")]
        public List<SizeValue> Sizes { get; set; }
    }

    public class SizeValue
    {
        [BsonId]
        public ObjectId Id { get; set; } 
        
        [BsonElement("size")]
        public string Size { get; set; } 

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}
