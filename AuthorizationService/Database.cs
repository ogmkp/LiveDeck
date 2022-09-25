using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthorizationService
{
    public class Database
    {
        private readonly IMongoDatabase _database;
        
        public Database(string databaseName)
        {
            var mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
            _database = mongoClient.GetDatabase(databaseName);
        }

        public BsonDocument Find(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            var result = collection.Find(filter).FirstOrDefault();
            return result;
        }
        
        public void Insert(string collectionName, BsonDocument document)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            collection.InsertOne(document);
        }
        
        public bool Exists(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            var result =  collection.Find(filter).Any();
            return result;
        }
        
        public void Delete(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            collection.DeleteOne(filter);
        }
    }
}