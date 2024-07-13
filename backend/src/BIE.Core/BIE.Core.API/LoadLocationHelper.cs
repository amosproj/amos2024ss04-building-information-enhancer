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
                Value = totalAreaSearchPolygon.ToString("N2") + "m²"
            });

            var chargingStations = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                getDataInPolygon(chargingStations, new() { "operator", "Id", "rated_power_kw", "charging_equipment_type" }, "EV_charging_stations", polygonWkt);
            var chargingStationsItems = new List<DatasetItem>();
            foreach (var h in chargingStations)
            {
                Point location = GeoReader.Read(h["Location"]) as Point;
                var item = new DatasetItem
                {
                    DisplayName = "Charging station: " + h["operator"],
                    DatasetId = "EV_charging_stations",
                    Coordinate = new double[] { location.Y, location.X },
                    Subdata = new() {
                        new ()
                        {
                            Key = "Rated Power",
                            Value = h["rated_power_kw"] + "kw"
                        },
                        new ()
                        {
                            Key = "Charging Equipment Type",
                            Value = h["charging_equipment_type"]
                        }
                    }
                };
                chargingStationsItems.Add(item);                
            }
            individualData.AddRange(chargingStationsItems);
            generalData.Add(new DatasetItem
            {
                DatasetId = "EV_charging_stations",
                DisplayName = "Total number of charging stations",
                Value = chargingStationsItems.Count.ToString()
            });

            var actualUse = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                getDataInPolygon(actualUse, new() { "nutzart" }, "actual_use", polygonWkt);
            AggregateDataForActualUse(generalData, individualData, totalAreaSearchPolygon, actualUse);

            var lod2buildings = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                getDataInPolygon(lod2buildings, new() { "Id", "GroundArea", "BuildingWallHeight", "LivingArea", "RoofArea" }, "building_models", polygonWkt);
            AggregateDataForLod2Citygml(generalData, individualData, totalAreaSearchPolygon, lod2buildings);

            var housefootprints = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                getDataInPolygon(housefootprints, new() { "Id" }, "house_footprints", polygonWkt);
            AggregateDataForHousefootprints(generalData, individualData, totalAreaSearchPolygon, housefootprints);

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = generalData,
                IndividualData = individualData.Take(200).ToList()

            };

            return locationDataResponse;
        }

        private static void AggregateDataForActualUse(List<DatasetItem> generalData, List<DatasetItem> individualData, double totalAreaSearchPolygon, List<Dictionary<string, string>> dataEntries)
        {
            long count = dataEntries.Count();
            if (count == 0)
                return;

            var usageAreaMap = new Dictionary<string, double>();
            foreach (var h in dataEntries)
            {
                Polygon p = GeoReader.Read(h["Location"]) as Polygon;
                double area = ApiHelper.getArea(p);

                string usageType = h.ContainsKey("nutzart") ? h["nutzart"] : "Unknown";

                if (!usageAreaMap.ContainsKey(usageType))
                    usageAreaMap.Add(usageType, area);
                else
                    usageAreaMap[usageType] += area;

                var item = new DatasetItem
                {
                    DisplayName = "Nutzart: " + (h.ContainsKey("nutzart") ? h["nutzart"] : ""),
                    DatasetId = "actual_use",
                    Value = area.ToString("N2") + "m²",
                    Coordinate = new double[] { p.Centroid.Y, p.Centroid.X }
                };
                individualData.Add(item);
            }


            var usageCount = usageAreaMap.Keys.Count;
            var summaryItemActualUse = new DatasetItem
            {
                DisplayName = "Actual use",
                DatasetId = "actual_use",
                Value = usageCount == 0 ? "No usages" : usageCount == 1 ? "1 usage" : usageCount + " different usages",
            };
            var subdataItems = usageAreaMap.Select(kv => new SubdataItem
            {
                Key = kv.Key,
                Value = kv.Value.ToString("N2") + "m²"
            }).ToList();

            summaryItemActualUse.Subdata.AddRange(subdataItems);
            generalData.Add(summaryItemActualUse);


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
                    DatasetId = "house_footprints",
                    Value = area.ToString("N2") + "m²",
                    Coordinate = new double[] { p.Centroid.Y, p.Centroid.X }
                };
                individualData.Add(item);
            }

            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential Area for geothermal use",
                DatasetId = "house_footprints",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("N2") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total number of buildings",
                DatasetId = "house_footprints",
                Value = totalCountHouseFootprints.ToString()
            });
            if (totalBuildingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingAreas, "Total building area", "house_footprints"));

        }

        private static void AggregateDataForLod2Citygml(List<DatasetItem> generalData, List<DatasetItem> individualData, double totalAreaSearchPolygon, List<Dictionary<string, string>> citygmldata)
        {
            long totalCountLod2Buildings = citygmldata.Count;
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
                    Value = area.ToString("N2") + "m²",
                    Coordinate = new double[] { p.Y, p.X },
                    Subdata = new List<SubdataItem>
                    {
                        new() { Key = "Ground area", Value = area.ToString("N2") + "m²" },
                        new() { Key = "Living area", Value = livingArea.ToString("N2") + "m²" },
                        new() { Key = "Roof surface", Value = roofArea.ToString("N2") + "m²" },
                        new() { Key = "Building wall height", Value = wallHeight.ToString("N2") + "m" }
                    }
                };
                individualData.Add(item);
            }

            double totalBuildingArea = totalBuildingAreas.Sum();
            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential Area for geothermal use",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("N2") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total number of LOD2 buildings",
                DatasetId = "building_models",
                Value = totalCountLod2Buildings.ToString()
            });
            if (totalBuildingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingAreas, "Total ground area", "building_models"));
            if (totalBuildingLivingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingLivingAreas, "Total living area", "building_models"));
            if (totalBuildingRoofAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingRoofAreas, "Total roof surface", "building_models"));
        }

        private static DatasetItem GenerateDatalistStatisticsEntry(List<double> listOfDataValues, string displayText, string id)
        {
            var subdata = new List<SubdataItem>
            {
                new() { Key = "Average", Value = listOfDataValues.Average().ToString("N2") + "m²" },
                new() { Key = "Median", Value = listOfDataValues.Median().ToString("N2") + "m²"}
            };

            // Add Variance only if the list contains more than 2 elements
            if (listOfDataValues.Count > 2)
                subdata.Add(new SubdataItem { Key = "Variance", Value = listOfDataValues.Variance().ToString("N2") });

            return new DatasetItem
            {
                DisplayName = displayText,
                DatasetId = id,
                Value = listOfDataValues.Sum().ToString("N2") + "m²",
                Subdata = subdata
            };
        }

        private static void getDataInPolygon(List<Dictionary<string, string>> targetList, HashSet<string> columns, string datasetId, string polygonWkt)
        {
            var metadata = MetadataDbHelper.GetMetadata(datasetId);
            if (metadata != null)
            {
                foreach (var table in metadata.additionalData.Tables)
                {
                    var sqlQuery = $"SELECT {string.Join(", ", columns)}, Location.STAsText() AS Location" +
                        ApiHelper.FromTableWhereIntersectsPolygon(table.Name, polygonWkt);
                    Console.WriteLine(sqlQuery);
                    targetList.AddRange(DbHelper.GetData(sqlQuery));
                }
            }
        }

        private static void getDataForPoint(List<Dictionary<string, string>> targetList, HashSet<string> columns, string datasetId, double longitude, double latitude)
        {
            var metadata = MetadataDbHelper.GetMetadata(datasetId);
            if (metadata != null)
            {
                foreach (var table in metadata.additionalData.Tables)
                {
                    // Define the SQL command with the columns string
                    string command = $@"
                        SELECT {string.Join(", ", columns)},
                            Location.STAsText() AS Location
                        FROM 
                            dbo.{table.Name}
                        WHERE
                            Location.STIntersects(geometry::Point({latitude}, {longitude}, 4326)) = 1";
                    Console.WriteLine(command);


                    // Get data from the database
                    targetList.AddRange(DbHelper.GetData(command));
                }
            }
        }

        public static LocationDataResponse loadLocationDataForSinglePoint(LocationDataRequest request)
        {
            var indivData = new List<DatasetItem>();

            var p = request.Location.First().First();
            Point pointForCalculations = new Point(p.ElementAt(0), p.ElementAt(1));

            if (request.Location.Count != 1 || request.Location.First().Count != 1)
                throw new ArgumentException("Single point location data can't be retrieved with the amount of request objects");

            var housefootprints = new List<Dictionary<String, String>>();
            getDataForPoint(housefootprints, new() { "Id" }, "house_footprints", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForHousefootprints(new(), indivData, 0.0, housefootprints);

            var lod2buildings = new List<Dictionary<String, String>>();
            getDataForPoint(lod2buildings, new() { "Id", "GroundArea", "BuildingWallHeight", "LivingArea", "RoofArea" }, "building_models", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForLod2Citygml(new(), indivData, 0.0, lod2buildings);

            var actualUse = new List<Dictionary<String, String>>();
            getDataForPoint(actualUse, new() { "nutzart" }, "actual_use", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForActualUse(new(), indivData, 0.0, actualUse);

            // closest charging stations and number of charging stations
            var radius = 10000000; // Define the radius as needed, but its weirdly in some kind of wgs84 degrees..
            var columns = new HashSet<string> { "operator", "Id", "rated_power_kw", "charging_equipment_type" };
            var metadata = MetadataDbHelper.GetMetadata("EV_charging_stations");
            if (metadata != null)
            {
                var tables = metadata.additionalData.Tables;
                if (tables.Count == 1)
                {
                    var chargingStations = getNclosestObjects(tables[0].Name, pointForCalculations.Y, pointForCalculations.X, radius, 3, columns);

                    var chargingStationsItems = new List<DatasetItem>();
                    foreach (var h in chargingStations)
                    {
                        Point location = GeoReader.Read(h["Location"]) as Point;
                        var item = new DatasetItem
                        {
                            DisplayName = "Charging station: " + h["operator"],
                            Value = "Distance: " + h["Distance"] + "m",
                            DatasetId = "EV_charging_stations",
                            Coordinate = new double[] { location.Y, location.X },
                            Subdata = new() {
                                new ()
                                {
                                    Key = "Rated Power",
                                    Value = h["rated_power_kw"] + "kw"
                                },
                                new ()
                                {
                                    Key = "Charging Equipment Type",
                                    Value = h["charging_equipment_type"]
                                }
                            }
                        };
                        chargingStationsItems.Add(item);
                    }
                    indivData.AddRange(chargingStationsItems);
                }
            }

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = indivData
            };
            return locationDataResponse;
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
                var wkt = result["Location"];
                var point2_wrong = GeoReader.Read(wkt) as Point;
                var distance = ApiHelper.getDistance(longitude, latitude, point2_wrong);
                result["Distance"] = distance.ToString("N2");
            }

            return results;
        }
    }


}
