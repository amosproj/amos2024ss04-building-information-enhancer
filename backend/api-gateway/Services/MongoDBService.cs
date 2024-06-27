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

        public IMongoCollection<DatasetData> GetDatasetsCollection()
        {
            return _database.GetCollection<DatasetData>("datasets");
        }

        /// <summary>
        /// Get the Metadata for a specific dataset.
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public DatasetData? GetDatasetMetadata(string dataset)
        {
            var collection = GetDatasetsCollection();

            return collection.Find(g => g.BasicData.DatasetId == dataset).FirstOrDefault();
        }
    }
}
