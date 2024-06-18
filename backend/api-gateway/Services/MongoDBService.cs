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
            var client = new MongoClient("mongodb://readonly_user:readonly_password@metadata-database:27017");
            _database = client.GetDatabase("bci-metadata");
        }

        public IMongoCollection<DatasetData> GetDatasetsCollection()
        {
            return _database.GetCollection<DatasetData>("datasets");
        }
    }
}
