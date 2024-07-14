using System;
using System.Collections.Generic;
using System.Globalization;
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
        var rowHeaders = mMetadata.additionalData.Tables[0].RowHeaders;
        var query = "";
        if (mMetadata.basicData.DatasetId == "air_pollutants")
        {
            query = "SELECT top 1000  station_name,pm10,pm2_5,no2,Location.AsTextZM() AS Location" +
                    ApiHelper.FromTableWhereIntersectsPolygon(tableName, polygon);
        }
        else if (mMetadata.basicData.DatasetId == "EV_charging_stations")
        {
            query = "SELECT top 1000  operator,Location.AsTextZM() AS Location" +
                        ApiHelper.FromTableWhereIntersectsPolygon(tableName, polygon);
        }
        else
        {
            Console.WriteLine("Provided CSV dataset ID is not supported!");
            throw new Exception("Provided CSV dataset ID is not supported!");
        }
        // the list of features from combined datasets.
        var features = new List<Dictionary<string, object>>();
        var culture = new CultureInfo("en-US");

        foreach (var row in DbHelper.GetData(query))
        {
            var location = row["Location"];
            // Extract the coordinates from the POINT string
            var coordinates =
                location
                .Replace("POINT (", "")
                .Replace(")", "")
                .Split(' ');
            var longitude = float.Parse(coordinates[0], culture);
            var latitude = float.Parse(coordinates[1], culture);
            // Create properties based on the dataset ID
            var properties = new Dictionary<string, string>();
            if (mMetadata.basicData.DatasetId == "air_pollutants")
            {
                properties["station_name"] = row["station_name"];
                properties["pm10"] = row["pm10"] + " µg/m3^3";
                properties["pm2_5"] = row["pm2_5"] + " µg/m3^3";
                properties["no2"] = row["no2"] + " µg/m3^3";
            }
            else if (mMetadata.basicData.DatasetId == "EV_charging_stations")
            {
                properties["operator"] = row["operator"];
            }
            // Create the feature
            var feature = new Dictionary<string, object>
            {
                { "type", "Feature" },
                {
                    "geometry", new Dictionary<string, object>
                    {
                        { "type", "Point" },
                        {
                            "coordinates", new List<float>{longitude, latitude}
                        }
                    }
                },
                {
                    "properties", properties
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