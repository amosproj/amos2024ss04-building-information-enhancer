using System;
using System.Globalization;
using BIE.Core.API.Controllers;
using BieMetadata;

namespace BIE.Core.API;

public static class ApiHelper
{
    /// <summary>
    /// Get the Bounding Box from the query parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static BoundingBox GetBoundingBoxFromParameters(DatasetController.QueryParameters parameters)
    {
        var boundingBox = new BoundingBox()
        {
            minX = parameters.BottomLong,
            minY = parameters.BottomLat,
            maxX = parameters.TopLong,
            maxY = parameters.TopLat
        };

        return boundingBox;
    }

    /// <summary>
    /// get the WKT (well known text) polygon from a bounding box.
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <returns></returns>
    public static string GetPolygonFromBoundingBox(BoundingBox boundingBox)
    {
        // Lat = Y Long = X
        var culture = new CultureInfo("en-US");
        var bottomLong = boundingBox.minX.ToString(culture);
        var bottomLat = boundingBox.minY.ToString(culture);
        var topLong = boundingBox.maxX.ToString(culture);
        var topLat = boundingBox.maxY.ToString(culture);

        // Create polygon WKT from bounding box
        return
            $"POLYGON(({bottomLong} {bottomLat}," +
            $" {topLong} {bottomLat}," +
            $" {topLong} {topLat}," +
            $" {bottomLong} {topLat}," +
            $" {bottomLong} {bottomLat}))";
    }

    /// <summary>
    /// Get the polygon of the bounding box given in the queryparameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string GetPolygonFromQueryParameters(DatasetController.QueryParameters parameters)
    {
        var boundingBox = GetBoundingBoxFromParameters(parameters);

        // Create polygon WKT from bounding box
        return GetPolygonFromBoundingBox(boundingBox);
    }

    /// <summary>
    /// returns true if two boxes are intersecting.
    /// </summary>
    /// <param name="box1"></param>
    /// <param name="box2"></param>
    /// <returns></returns>
    public static bool BoxIntersection(BoundingBox box1, BoundingBox box2)
    {
        var left1 = box1.minX;
        var bottom1 = box1.minY;
        var right1 = box1.maxX;
        var top1 = box1.maxY;

        var left2 = box2.minX;
        var bottom2 = box2.minY;
        var right2 = box2.maxX;
        var top2 = box2.maxY;

        Console.WriteLine($"left1: {left1}, left2: {left2}");

        return !(right1 < left2 || right2 < left1 || top1 < bottom2 || top2 < bottom1);
    }

    /// <summary>
    /// Gets the Query to filter a table via polygon Intersection. Presents the FROM part of a Query.
    /// </summary>
    /// <param name="tableName">the name of the table to filter</param>
    /// <param name="polygon">the polygon string</param>
    /// <returns></returns>
    public static string FromTableIntersectsPolygon(string tableName, string polygon)
    {
        return $@"
FROM dbo.{tableName}
WHERE Location.STIntersects(geometry::STGeomFromText('{polygon}', 4326)) = 1;";
    }
}