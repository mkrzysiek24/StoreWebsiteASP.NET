using MongoDB.Driver;

namespace SneakersPlanet.Models
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient mongoClient, string databaseName)
        {
            _database = mongoClient.GetDatabase(databaseName);
        }

        public IMongoCollection<Item> Items => _database.GetCollection<Item>("items");
    }
}

