using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Accord.Math;
using BIE.Core.API.Controllers;
using BieMetadata;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.IO.CoordinateSystems;

namespace BIE.Core.API;

public static class ApiHelper
{
    private static CultureInfo sCultureInfo = new CultureInfo("en-US");
    private static string epsg27700 = @"PROJCS[""North_America_Albers_Equal_Area_Conic"",
                        GEOGCS[""NAD83"",
                            DATUM[""North_American_Datum_1983"",
                                SPHEROID[""GRS 1980"",6378137,298.257222101,
                                    AUTHORITY[""EPSG"",""7019""]],
                                AUTHORITY[""EPSG"",""6269""]],
                            PRIMEM[""Greenwich"",0,
                                AUTHORITY[""EPSG"",""8901""]],
                            UNIT[""degree"",0.0174532925199433,
                                AUTHORITY[""EPSG"",""9122""]],
                            AUTHORITY[""EPSG"",""4269""]],
                        PROJECTION[""Albers_Conic_Equal_Area""],
                        PARAMETER[""latitude_of_center"",40],
                        PARAMETER[""longitude_of_center"",-96],
                        PARAMETER[""standard_parallel_1"",20],
                        PARAMETER[""standard_parallel_2"",60],
                        PARAMETER[""false_easting"",0],
                        PARAMETER[""false_northing"",0],
                        UNIT[""metre"",1,
                            AUTHORITY[""EPSG"",""9001""]],
                        AXIS[""Easting"",EAST],
                        AXIS[""Northing"",NORTH],
                        AUTHORITY[""ESRI"",""102008""]
                        "; // see http://epsg.io/27700
    private static ICoordinateTransformation mTransformation = new CoordinateTransformationFactory().
        CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, 
        (CoordinateSystem)CoordinateSystemWktReader.Parse(epsg27700));

    public static double getDistance(double longitude, double latitude, Point p)
    {
        double[] sourceInWGS84_arr = { longitude, latitude };
        double[] sourceInEPSG27700_arr = mTransformation.MathTransform.Transform(sourceInWGS84_arr);
        Coordinate sourceInEPSG27700 = new Coordinate(sourceInEPSG27700_arr[0], sourceInEPSG27700_arr[1]);

        double[] targetPointInWGS84 = { p.Y, p.X };
        double[] targetPointInEpsg27700 = mTransformation.MathTransform.Transform(targetPointInWGS84);
        Coordinate targetInEPSG27700 = new Coordinate(targetPointInEpsg27700[0], targetPointInEpsg27700[1]);
        var distance = sourceInEPSG27700.Distance(targetInEPSG27700);
        return distance;
    }

    public static double getArea(Polygon p)
    {
        // convert each point of the polygon into a new coordinate (revert x and y
        // convert it into EPSG27700
        // return area
        var coordinates = p.Coordinates;
        Console.WriteLine($"coordinates {coordinates}");
        var transformedCoordinates = new Coordinate[coordinates.Length];
        // Transform each coordinate
        for (int i = 0; i < coordinates.Length; i++)
        {
            double[] pointWGS84 = { coordinates[i].Y, coordinates[i].X }; // Switch X and Y
            double[] pointEPSG27700 = mTransformation.MathTransform.Transform(pointWGS84);
            transformedCoordinates[i] = new Coordinate(pointEPSG27700[0], pointEPSG27700[1]);
            Console.WriteLine($"transformedCoordinates[i] {transformedCoordinates[i]}");
        }
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 27700);
        var transformedPolygon = new Polygon(new LinearRing(transformedCoordinates), geometryFactory);
        Console.WriteLine($"area {transformedPolygon.Area}");
        Console.WriteLine($"transformed coords {transformedPolygon.Coordinates}");



        // Return the area of the transformed polygon
        return transformedPolygon.Area;
    }

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

    public static List<string> ConvertToWktPolygons(List<List<List<double>>> locations)
    {
        var culture = new CultureInfo("en-US");
        var wktPolygons = new List<string>();

        foreach (var polygon in locations)
        {
            var wktString = "POLYGON((";
            foreach (var point in polygon)
            {
                if (point.Count != 2)
                {
                    throw new ArgumentException("Each point should have exactly two coordinates.");
                }

                var longitude = point[0].ToString(culture);
                var latitude = point[1].ToString(culture);
                wktString += $"{longitude} {latitude}, ";
            }

            // Close the polygon by repeating the first point
            var firstPoint = polygon[0];
            var firstLongitude = firstPoint[0].ToString(culture);
            var firstLatitude = firstPoint[1].ToString(culture);
            wktString += $"{firstLongitude} {firstLatitude}))";

            wktPolygons.Add(wktString);
        }

        return wktPolygons;
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

        //Console.WriteLine($"left1: {left1}, left2: {left2}");

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

    /// <summary>
    /// Gets the float coordinates from the polygon string
    /// </summary>
    /// <param name="polygon"></param>
    /// <returns></returns>
    public static float[][][] GetCoordinatesFromPolygon(string polygon)
    {
        // example:
        // POLYGON ((49.496927347229494 11.060226859896797, ..., 49.496927347229494 11.060226859896797))
        var substring = polygon.Substring(10, polygon.Length - 12);
        var coordinatePairs = substring.Replace("(", "").Replace(")", "").Split(",");
        var floatList = coordinatePairs.Select(pair => pair.Trim().Split(" ").Select(StringToFloat).ToArray());
        return new []{floatList.ToArray()};
    }

    /// <summary>
    /// Casts a string to a Float using the en-US culture.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static float StringToFloat(string s)
    {
        return float.Parse(s, sCultureInfo);
    }
}