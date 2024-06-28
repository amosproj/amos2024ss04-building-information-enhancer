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
    private MetadataObject mMetadata;

    /// <summary>
    /// Handler for the Shape Dataset Type. Scans multiple Tables and combines results.
    /// </summary>
    /// <param name="metadata"></param>
    public ShapeDatasetHandler(MetadataObject metadata)
    {
        mMetadata = metadata;
    }

    /// <summary>
    /// Get the data inside a bounding box
    /// </summary>
    /// <param name="dataset"></param>
    /// <param name="boundingBox"></param>
    /// <returns></returns>
    public string GetDataInsideArea(BoundingBox boundingBox)
    {
        var polygon = ApiHelper.GetPolygonFromBoundingBox(boundingBox);

        // the list of features from combined datasets.
        var features = new List<Dictionary<string, object>>();

        foreach (var table in mMetadata.additionalData.Tables)
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