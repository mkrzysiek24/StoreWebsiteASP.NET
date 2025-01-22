using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SneakersPlanet.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("mail")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("name")]
        public string FirstName { get; set; }

        [BsonElement("surname")]
        public string LastName { get; set; }

        [BsonElement("postal")]
        public string PostalCode { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("adress")]
        public string Address { get; set; }

        [BsonElement("account")]
        public string AccountType { get; set; }
    }
}


