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
                continue;
            }

            // bounding boxes intersect.
            // get data

            // SQL Query to find intersecting points
            var sqlQuery = $"SELECT top 1000  Location.AsTextZM() AS Location," +
                           $" Location.STGeometryType() AS Type" +
                           (table.RowHeaders.Count > 0 ? "," + string.Join(',', table.RowHeaders) : "") +
                           ApiHelper.FromTableWhereIntersectsPolygon(table.Name, polygon);

            Console.WriteLine($"requesting:\n{sqlQuery}");
            // create feature object
            foreach (var row in DbHelper.GetData(sqlQuery))
            {
                var feature = GetFeatureFromRow(row, table);
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

    /// <summary>
    /// Get the GeoJson Feature Object corresponding to the object
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetFeatureFromRow(Dictionary<string, string> row, MetadataObject.TableData table)
    {
        var properties = new Dictionary<string, string>();
        foreach (var header in table.RowHeaders)
        {
            properties[header] = row[header];
        }
        
        return new Dictionary<string, object>
        {
            { "type", "Feature" },
            {
                "geometry", new Dictionary<string, object>
                {
                    { "type", $"{row["Type"]}" },
                    {
                        "coordinates", ApiHelper.GetCoordinatesFromPolygon(row["Location"])
                    }
                }
            },
            {
                "properties", properties
            }
        };
    }
}