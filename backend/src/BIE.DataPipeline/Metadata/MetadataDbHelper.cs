using BIE.DataPipeline.Import;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BIE.DataPipeline.Metadata;

public class MetadataDbHelper
{
    private string mMetaDataDbUrl;

    private IMongoDatabase mDatabase;

    public MetadataDbHelper()
    {
        mMetaDataDbUrl = Environment.GetEnvironmentVariable("METADATA_DB_URL") ?? "";

        if (mMetaDataDbUrl == "")
        {
            throw new Exception("Could not Determine Metadata Url.");
        }

        var client = new MongoClient($"mongodb://datapipeline:datapipeline@{mMetaDataDbUrl}/bci-metadata");
        mDatabase = client.GetDatabase("bci-metadata");
    }

    public MetadataObject GetMetadata(DataSourceDescription description)
    {
        var collection = mDatabase.GetCollection<MetadataObject>("datasets");

        var obj = collection
            .Find(g => g.basicData.DatasetId == description.dataset)
            .FirstOrDefault();

        if (obj == null)
        {
            obj = new MetadataObject()
            {
                basicData = new MetadataObject.BasicData()
                {
                    DatasetId = description.dataset
                }
            };
        }

        return obj;
    }
}