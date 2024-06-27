using MongoDB.Driver;

namespace BieMetadata;

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

    public MetadataObject GetMetadata(string dataset)
    {
        // Get the collection
        var collection = mDatabase.GetCollection<MetadataObject>("datasets");

        // Find the dataset object with the given ID
        var metadataObject = collection
            .Find(g => g.basicData.DatasetId == dataset)
            .FirstOrDefault();

        return metadataObject;
    }

    public bool UpdateMetadata(string dataset, string tableName, int numberOfLines, BoundingBox boundingBox)
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

        // Load the existing table
        var existingTable = metadataObject.additionalData.Tables.Find(t => t.Name == tableName);
        if (existingTable == null)
        {
            // Create a new table object if not present
            var newTable = new MetadataObject.TableData()
            {
                Name = tableName,
                NumberOfLines = numberOfLines,
                BoundingBox = boundingBox
            };
            metadataObject.additionalData.Tables.Add(newTable);
            collection.ReplaceOne(g => g.basicData.DatasetId == dataset, metadataObject);
            return true;
        }

        // Table info already exists, for now just choose the larger number of lines number.
        existingTable.NumberOfLines = existingTable.NumberOfLines < numberOfLines
            ? numberOfLines
            : existingTable.NumberOfLines;

        // always write the current Bounding box
        existingTable.BoundingBox = boundingBox;

        collection.ReplaceOne(g => g.basicData.DatasetId == dataset, metadataObject);
        return true;
    }
}
