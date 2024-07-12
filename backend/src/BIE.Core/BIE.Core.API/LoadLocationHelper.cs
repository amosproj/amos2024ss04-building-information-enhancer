using BIE.Core.API.Controllers;
using BIE.Core.DBRepository;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System;
using LinqStatistics;
using BieMetadata;
using System.Linq;
using NetTopologySuite;


namespace BIE.Core.API
{
    public class LoadLocationHelper
    {
        private static MetadataDbHelper mMetadataDbHelper;
        private static readonly WKTReader GeoReader = new(new NtsGeometryServices(new PrecisionModel(), 4326));
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);


        private static MetadataDbHelper MetadataDbHelper
        {
            get
            {
                if (mMetadataDbHelper != null)
                {
                    return mMetadataDbHelper;
                }

                mMetadataDbHelper = new MetadataDbHelper();
                if (mMetadataDbHelper.CreateConnection())
                {
                    return mMetadataDbHelper;
                }

                Console.WriteLine("could not establish Metadata-database Connection");
                throw new Exception("no metadata DB reachable!");
            }
        }

        public static LocationDataResponse loadLocationDataforPolygons(LocationDataRequest request)
        {
            if (request.Location.Count < 1 || request.Location.First().Count <= 1)
                throw new ArgumentException("Polygon data cannot be retrieved with the amount of request objects");

            var generalData = new List<DatasetItem>();
            var individualData = new List<DatasetItem>();

            var polygons = ApiHelper.ConvertToWktPolygons(request.Location);
            WKTReader reader = new WKTReader();
            double totalAreaSearchPolygon = 0.0;
            foreach (var polygonWkt in polygons)
            {
                Geometry geometry = reader.Read(polygonWkt);
                if (geometry is Polygon polygon)
                    totalAreaSearchPolygon += ApiHelper.getArea(polygon);
            }
            generalData.Insert(0, new DatasetItem
            {
                DisplayName = "Searched area",
                Value = totalAreaSearchPolygon.ToString("0.##") + "m²"
            });

            var housefootprints = new List<Dictionary<String, String>>();
            var columns = new HashSet<string> { "Id" };
            var datasetId = "house_footprints";
            foreach (var polygonWkt in polygons)
                getDataInPolygon(housefootprints, columns, datasetId, polygonWkt);

            AggregateDataForHousefootprints(generalData, individualData, totalAreaSearchPolygon, housefootprints);

            var lod2buildings = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                getDataInPolygon(lod2buildings, new() { "Id", "GroundArea", "BuildingWallHeight", "LivingArea", "RoofArea" }, "building_models", polygonWkt);

            AggregateDataForLod2Citygml(generalData, individualData, totalAreaSearchPolygon, lod2buildings);

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = generalData,
                IndividualData = individualData.Take(500).ToList()

            };

            return locationDataResponse;
        }

        private static void AggregateDataForHousefootprints(List<DatasetItem> generalData, List<DatasetItem> individualData, double totalAreaSearchPolygon, List<Dictionary<string, string>> housefootprints)
        {
            long totalCountHouseFootprints = housefootprints.Count();
            if (totalCountHouseFootprints == 0)
                return;
            double totalBuildingArea = 0.0;
            List<double> totalBuildingAreas = new List<double>();
            foreach (var h in housefootprints)
            {
                Polygon p = GeoReader.Read(h["Location"]) as Polygon;
                double area = ApiHelper.getArea(p);

                totalBuildingAreas.Add(area);
                totalBuildingArea += area;

                var item = new DatasetItem
                {
                    DisplayName = "House footprint: " + (h.ContainsKey("Id") ? h["Id"] : ""),
                    DatasetId = "House_footprints",
                    Value = area.ToString("0.##") + "m²",
                    Coordinate = new double[] { p.Centroid.X, p.Centroid.Y },
                    Subdata = new List<SubdataItem>()
                };
                individualData.Add(item);
            }

            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential Area for geothermal use",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("0.##") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total number of buildings",
                Value = totalCountHouseFootprints.ToString()
            });
            if(totalBuildingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingAreas, "Total building area"));

        }

        private static void AggregateDataForLod2Citygml(List<DatasetItem> generalData, List<DatasetItem> individualData, double totalAreaSearchPolygon, List<Dictionary<string, string>> citygmldata)
        {
            long totalCountLod2Buildings = citygmldata.Count();
            if (totalCountLod2Buildings == 0)
                return;

            // GroundHeight, DistrictKey, CheckDate, GroundArea, BuildingWallHeight, LivingArea, RoofArea
            List<double> totalBuildingAreas = new();
            List<double> totalBuildingLivingAreas = new();
            List<double> totalBuildingRoofAreas = new();
            foreach (var h in citygmldata)
            {
                double area = double.Parse(h["GroundArea"]);
                totalBuildingAreas.Add(area);

                double livingArea = double.Parse(h["LivingArea"]);
                totalBuildingLivingAreas.Add(livingArea);

                double roofArea = double.Parse(h["RoofArea"]);
                totalBuildingRoofAreas.Add(roofArea);

                double wallHeight = double.Parse(h["BuildingWallHeight"]);
                totalBuildingRoofAreas.Add(roofArea);

                Point p = (GeoReader.Read(h["Location"]) as Polygon).Centroid;
                var item = new DatasetItem
                {
                    DisplayName = "Building model: " + (h.ContainsKey("Id") ? h["Id"] : ""),
                    DatasetId = "building_models",
                    Value = area.ToString("0.##") + "m²",
                    Coordinate = new double[] { p.X, p.Y },
                    Subdata = new List<SubdataItem>
                    {
                        new() { Key = "Ground area", Value = area.ToString("0.##") + "m²" },
                        new() { Key = "Living area", Value = livingArea.ToString("0.##") + "m²" },
                        new() { Key = "Roof surface", Value = roofArea.ToString("0.##") + "m²" },
                        new() { Key = "Building wall height", Value = wallHeight.ToString("0.##") + "m" }
                    }
                };
                individualData.Add(item);
            }

            double totalBuildingArea = totalBuildingAreas.Sum();
            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential Area for geothermal use",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("0.##") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total number of LOD2 buildings",
                Value = totalCountLod2Buildings.ToString()
            });
            if (totalBuildingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingAreas, "Total ground area"));
            if (totalBuildingLivingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingLivingAreas, "Total living area"));
            if (totalBuildingRoofAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingRoofAreas, "Total roof surface"));
        }

        private static DatasetItem GenerateDatalistStatisticsEntry(List<double> listOfDataValues, string displayText)
        {
            return new DatasetItem
            {
                DisplayName = displayText,
                Value = listOfDataValues.Sum().ToString("0.##") + "m²",
                Subdata = new List<SubdataItem>
                {
                    new() { Key = "Average", Value = listOfDataValues.Average().ToString() + "m²" },
                    new() { Key = "Median", Value = listOfDataValues.Median().ToString() + "m²"},
                    new() { Key = "Variance", Value = listOfDataValues.Variance().ToString()}
                }
            };
        }

        private static void getDataInPolygon(List<Dictionary<string, string>> housefootprints, HashSet<string> columns, string datasetId, string polygonWkt)
        {
            var metadata = MetadataDbHelper.GetMetadata(datasetId);
            if (metadata != null)
            {
                foreach (var table in metadata.additionalData.Tables)
                {
                    var sqlQuery = $"SELECT {string.Join(", ", columns)}, Location.STAsText() AS Location" +
                        ApiHelper.FromTableIntersectsPolygon(table.Name, polygonWkt);
                    Console.WriteLine(sqlQuery);
                    housefootprints.AddRange(DbHelper.GetData(sqlQuery));
                }
            }
        }

        public static LocationDataResponse loadLocationDataForSinglePoint(LocationDataRequest request)
        {
            var generalData = new List<DatasetItem>();


            if (request.Location.Count != 1 || request.Location.First().Count != 1)
                throw new ArgumentException("Single point location data can't be retrieved with the amount of request objects");

            var p = request.Location.First().First();
            Point pointForCalculations = new Point(p.ElementAt(0), p.ElementAt(1));

            // closest charging stations and number of charging stations
            var radius = 10000000; // Define the radius as needed
            var columns = new HashSet<string> { "operator", "Id", "rated_power_kw" };
            var metadata = MetadataDbHelper.GetMetadata("EV_charging_stations");
            if (metadata != null)
            {
                var tables = metadata.additionalData.Tables;
                if (tables.Count == 1)
                {
                    var chargingStations = getNclosestObjects(tables[0].Name, pointForCalculations.Y, pointForCalculations.X, radius, 3, columns);

                    var chargingStationsData = chargingStations.Select(h => new DatasetItem
                    {
                        DisplayName = "Charging station: " + (h.ContainsKey("operator") ? h["operator"] : "No operator defined"),
                        Value = (h.ContainsKey("Distance") ? h["Distance"] + "m" : "-1") + " | " + (h.ContainsKey("rated_power_kw") ? h["rated_power_kw"] + "kw" : "-1"),
                        DatasetId = "EV_charging_stations",
                    });

                    generalData.AddRange(chargingStationsData);
                }
            }


            var housefootprints = new List<Dictionary<String, String>>();

            // Additional section for house footprints 
            columns = new HashSet<string> { "Id" };
            metadata = MetadataDbHelper.GetMetadata("house_footprints");
            if (metadata != null)
            {
                foreach (var table in metadata.additionalData.Tables)
                    housefootprints.AddRange(getMatchingObjects(table.Name, pointForCalculations.Y, pointForCalculations.X, columns));
            }
            var housefootprintsData = housefootprints.Select(h => new DatasetItem
            {
                DisplayName = "House footprint" + (h.ContainsKey("Id") ? h["Id"] : ""),
                Value = h.ContainsKey("Location") ? ApiHelper.getArea(GeoReader.Read(h["Location"]) as Polygon).ToString("0.##") + "m²" : "-1",
                DatasetId = "House_footprints",
            });
            generalData.AddRange(housefootprintsData);

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = generalData
            };
            return locationDataResponse;
        }

        private static IEnumerable<Dictionary<string, string>> getMatchingObjects(
        string datasetId,
        double longitude,
        double latitude,
        IEnumerable<string> columns)
        {
            // Convert the columns set to a comma-separated string
            string columnsString = string.Join(", ", columns);

            // Define the SQL command with the columns string
            string command = $@"
                    SELECT TOP 1000
                        {columnsString},
                        Location.STAsText() AS Location
                    FROM 
                        dbo.{datasetId}
                    WHERE
                        Location.STIntersects(geometry::Point({latitude}, {longitude}, 4326)) = 1
                    ORDER BY 
                        geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";

            // Get data from the database
            var results = DbHelper.GetData(command).ToList();

            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var polygon = GeoReader.Read(wkt) as Polygon;
                    if (polygon != null)
                    {
                        result["Area"] = ApiHelper.getArea(polygon).ToString("0.##");
                    }
                    else
                        result["Area"] = "-1";

                }
                else
                    result["Area"] = "-1 no location";

            }

            return results;
        }

        private static IEnumerable<Dictionary<string, string>> getNclosestObjects(string datasetId, double longitude, double latitude, double radius, int n, IEnumerable<string> columns)
        {
            string columnsString = string.Join(", ", columns);

            string command = $@"
    SELECT TOP {n}
        {columnsString},
        Location.STAsText() AS Location
    FROM 
        dbo.{datasetId}
    WHERE
        geometry::Point({latitude}, {longitude}, 4326).STBuffer({radius}).STIntersects(Location) = 1
    ORDER BY 
        geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";

            var results = DbHelper.GetData(command).ToList();


            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var point2_wrong = GeoReader.Read(wkt) as Point;
                    if (point2_wrong != null)
                    {

                        var distance = ApiHelper.getDistance(longitude, latitude, point2_wrong);
                        result["Distance"] = distance.ToString("0.##");
                    }
                    else
                    {
                        result["Distance"] = "-1";
                    }
                }
                else
                {
                    result["Distance"] = "-1";
                }
            }

            return results;
        }

        private IEnumerable<Dictionary<string, string>> GetMatchingObjects(
        string datasetId,
        double longitude,
        double latitude,
        IEnumerable<string> columns)
        {
            // Convert the columns set to a comma-separated string
            string columnsString = string.Join(", ", columns);

            // Define the SQL command with the columns string
            string command = $@"
                SELECT TOP 1000
                    {columnsString},
                    Location.STAsText() AS Location
                FROM 
                    dbo.{datasetId}
                WHERE
                    Location.STIntersects(geometry::Point({latitude}, {longitude}, 4326)) = 1
                ORDER BY 
                    geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";


            // Get data from the database
            var results = DbHelper.GetData(command).ToList();

            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var polygon = GeoReader.Read(wkt) as Polygon;
                    if (polygon != null)
                    {
                        result["Area"] = ApiHelper.getArea(polygon).ToString("0.##");
                    }
                    else
                        result["Area"] = "-1";

                }
                else
                    result["Area"] = "-1 no location";

            }

            return results;
        }
    }


}
