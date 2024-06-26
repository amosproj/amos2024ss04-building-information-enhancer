using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using APIGateway.Models;

namespace BIE.Core.API.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IConfiguration configuration)
        {
            var client = new MongoClient("mongodb://readonly:readonly@metadata-database:27017/bci-metadata");
            _database = client.GetDatabase("bci-metadata");
        }

        public IMongoCollection<MetadataObject> GetDatasetsCollection()
        {
            return _database.GetCollection<MetadataObject>("datasets");
        }
    }
}
