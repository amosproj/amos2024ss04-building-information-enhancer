using System;
using System.Collections.Generic;
using BIE.Core.API.Controllers;
using BIE.Core.DBRepository;
using BieMetadata;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BIE.Core.API.DatasetHandlers;

public class ShapeDatasetHandler : IDatasetHandler
{
    public MetadataDbHelper MetadataDb { get; set; }

    /// <summary>
    /// Get the data inside a bounding box
    /// </summary>
    /// <param name="dataset"></param>
    /// <param name="boundingBox"></param>
    /// <returns></returns>
    public string GetDataInsideArea(string dataset, BoundingBox boundingBox)
    {
        if (MetadataDb == null || !MetadataDb.Connected)
        {
            Console.WriteLine("Metadata DB is not reachable or not set.");
            return "";
        }

        var metadata = MetadataDb.GetMetadata(dataset);

        if (metadata == null)
        {
            Console.WriteLine($"Dataset {dataset} is not present in the metadata");
            return "";
        }

        var polygon = ApiHelper.GetPolygonFromBoundingBox(boundingBox);

        // the list of features from combined datasets.
        var features = new List<Dictionary<string, object>>();

        foreach (var table in metadata.additionalData.Tables)
        {
            if (!table.BoundingBox.HasValue)
            {
                continue;
            }

            if (!ApiHelper.BoxIntersection(boundingBox, table.BoundingBox.Value))
            {
                var bb = table.BoundingBox.Value;
                // Console.WriteLine($"request-- x: {boundingBox.minX}, y: {boundingBox.minY} || x: {boundingBox.maxX}, y: {boundingBox.maxY}");
                // Console.WriteLine($"x: {bb.minX}, y: {bb.minY} || x: {bb.maxX}, y: {bb.maxY}");
                continue;
            }

            // bounding boxes intersect.
            // get data

            // SQL Query to find intersecting points

            var sqlQuery = $@"
SELECT top 1000  Location.AsTextZM() AS Location, Location.STGeometryType() AS Type
FROM dbo.{table.Name}
WHERE Location.STIntersects(geometry::STGeomFromText('{polygon}', 4326)) = 1;";


            foreach (var row in DbHelper.GetData(sqlQuery))
            {
                var feature = new Dictionary<string, object>
                {
                    { "type", "Feature" },
                    {
                        "geometry", new Dictionary<string, object>
                        {
                            { "type", $"{row["Type"]}" },
                            {
                                "coordinates",
                                $"[{DatasetController.QueryParameters.GetPolygonCordinates(row["Location"])}]"
                            }
                        }
                    },
                    {
                        "properties", new Dictionary<string, object>
                        {
                            { "text", $"{row["Type"]}" }
                        }
                    }
                };

                features.Add(feature);
            }
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