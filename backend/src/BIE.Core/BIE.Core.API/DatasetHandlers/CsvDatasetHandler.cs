using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BIE.Core.API.Controllers;
using BIE.Core.DBRepository;
using BieMetadata;

namespace BIE.Core.API.DatasetHandlers;

/// <summary>
/// Handler for the CSV Datatype. Assumes a single table. Uses a Geometry row named
/// </summary>
public class CsvDatasetHandler : IDatasetHandler
{
    private MetadataObject mMetadata;

    /// <summary>
    /// Handler for the CSV Datatype. Assumes a single table. Uses a Geometry row named
    /// </summary>
    /// <param name="metadata"></param>
    public CsvDatasetHandler(MetadataObject metadata)
    {
        mMetadata = metadata;
    }

    public string GetDataInsideArea(BoundingBox boundingBox)
    {
        if (!mMetadata.additionalData.Tables.Any())
        {
            Console.WriteLine($"Dataset {mMetadata.basicData.DatasetId} does not contain any tables!");
            return "";
        }

        var polygon = ApiHelper.GetPolygonFromBoundingBox(boundingBox);
        var tableName = mMetadata.additionalData.Tables[0].Name;

        var query = "SELECT top 1000  operator, Location.AsTextZM() AS Location" +
                    ApiHelper.FromTableIntersectsPolygon(tableName, polygon);

        // the list of features from combined datasets.
        var features = new List<Dictionary<string, object>>();


        foreach (var row in DbHelper.GetData(query))
        {
            var location = row["Location"];
            // Extract the coordinates from the POINT string
            var coordinates = 
                location
                .Replace("POINT (", "")
                .Replace(")", "")
                .Split(' ');
            
            var longitude = coordinates[0];
            var latitude = coordinates[1];

            var feature = new Dictionary<string, object>
            {
                { "type", "Feature" },
                {
                    "geometry", new Dictionary<string, object>
                    {
                        { "type", $"{row["Type"]}" },
                        {
                            "coordinates", $"[{longitude}, {latitude}]"
                        }
                    }
                },
                {
                    "properties", new Dictionary<string, object>
                    {
                        { "text", $"{row["operator"]}" }
                    }
                }
            };

            features.Add(feature);
        }

        // the response object
        var responseObj = new Dictionary<string, object>
        {
            { "type", "FeatureCollection" },
            { "features", features }
        };
        return JsonSerializer.Serialize(responseObj);
    }
}