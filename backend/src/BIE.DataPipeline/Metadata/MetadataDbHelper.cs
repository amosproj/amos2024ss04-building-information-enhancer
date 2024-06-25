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
    }

    /// <summary>
    /// Create the connection to the Database. Returns False if connection was not possible.
    /// </summary>
    public bool CreateConnection()
    {
        try
        {
            var connectionString = $"mongodb://datapipeline:datapipeline@{mMetaDataDbUrl}/bci-metadata";
            var client = new MongoClient(connectionString);
            mDatabase = client.GetDatabase("bci-metadata");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not establish the connection to Metadata Database.");
            return false;
        }
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

    public void UpdateMetadata(DataSourceDescription description, int numberOfLines)
    {
        var collection = mDatabase.GetCollection<MetadataObject>("datasets");

        var metadata = collection
            .Find(g => g.basicData.DatasetId == description.dataset)
            .FirstOrDefault();

        var newTable = new MetadataObject.TableData() { Name = description.table_name, NumberOfLines = numberOfLines };

        if (metadata == null)
        {
            metadata = new MetadataObject()
            {
                basicData = new MetadataObject.BasicData()
                {
                    DatasetId = description.dataset,
                    Tables = new List<MetadataObject.TableData>() { newTable }
                }
            };

            collection.InsertOne(metadata);
            return;
        }

        var existingTable = metadata.basicData.Tables.Find(t => t.Name == description.table_name);

        if (existingTable == null)
        {
            metadata.basicData.Tables.Add(newTable);

            collection.ReplaceOne(g => g.basicData.DatasetId == description.dataset, metadata);
            return;
        }

        // table info already exists.
        // for now just choose the larger number of lines number.
        existingTable.NumberOfLines = existingTable.NumberOfLines < numberOfLines
            ? numberOfLines
            : existingTable.NumberOfLines;


        collection.ReplaceOne(g => g.basicData.DatasetId == description.dataset, metadata);
    }
}