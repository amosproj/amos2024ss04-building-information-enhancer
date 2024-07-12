using MongoDB.Driver;

namespace BieMetadata;

public class MetadataDbHelper
{
    private string mMetaDataDbUrl;

    private IMongoDatabase mDatabase;

    public bool Connected { get; private set; }

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
            Connected = true;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not establish the connection to Metadata Database.");
            return false;
        }
    }

    /// <summary>
    /// get the Metadata for a specified dataset. Returns null if dataset is not found.
    /// </summary>
    /// <param name="dataset"></param>
    /// <returns></returns>
    public MetadataObject? GetMetadata(string dataset)
    {
        // Get the collection
        var collection = mDatabase.GetCollection<MetadataObject>("datasets");

        // Find the dataset object with the given ID
        var metadataObject = collection
            .Find(g => g.basicData.DatasetId == dataset)
            .FirstOrDefault();

        return metadataObject;
    }

    public bool UpdateMetadata(string dataset, MetadataObject.TableData tableData)
    {
        // Load the collection
        var collection = mDatabase.GetCollection<MetadataObject>("datasets");

        // Find the dataset object
        var metadataObject = collection
            .Find(g => g.basicData.DatasetId == dataset)
            .FirstOrDefault();

        if (metadataObject == null)
        {
            Console.WriteLine($"Could not find Metadata for dataset {dataset}.");
            return false;
        }

        // Load and remove any existing table
        var existingTable = metadataObject.additionalData.Tables.Find(t => t.Name == tableData.Name);
        if (existingTable != null)
        {
            metadataObject.additionalData.Tables.Remove(existingTable);
        }

        // Insert the new Table object.
        metadataObject.additionalData.Tables.Add(tableData);
        collection.ReplaceOne(g => g.basicData.DatasetId == dataset, metadataObject);
        return true;
    }

    public bool UpdateMetadata(string dataset, string tableName, int numberOfLines, BoundingBox boundingBox)
    {
        return UpdateMetadata(dataset,
                       new MetadataObject.TableData()
                       {
                           Name = tableName,
                           NumberOfLines = numberOfLines,
                           BoundingBox = new BoundingBox(),
                           RowHeaders = new List<string>()
                       });
    }
}