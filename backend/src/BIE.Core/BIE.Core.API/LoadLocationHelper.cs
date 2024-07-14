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
using NetTopologySuite.Algorithm;
using NetTopologySuite.Utilities;


namespace BIE.Core.API
{
    public class LoadLocationHelper
    {
        private static MetadataDbHelper mMetadataDbHelper;
        private static readonly WKTReader GeoReader = new(new NtsGeometryServices(new PrecisionModel(), 4326));
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

        private static readonly HashSet<string> cityGmlColumns = new() { "Id", "GroundArea", "BuildingWallHeight", "LivingArea", "RoofArea", "BuildingVolume", "RoofArea", "SolarPotential" };
        private static readonly HashSet<string> airPollutionColumns = new() { "station_name", "pm10", "pm2_5", "no2" };
        private static readonly HashSet<string> chargingStationsColumns = new() { "operator", "Id", "rated_power_kw", "charging_equipment_type" };
        private static readonly HashSet<string> actualUseColumns = new() { "nutzart" };
        private static readonly HashSet<string> houseFootprintsColumns = new() { "Id" };


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

        /// <summary>
        /// Returns data from the data lake that is associated with the given point.
        /// This can be intersecting polygons and/or closest Points of Interest.
        /// </summary>
        /// <param name="request">Must only contain 1 entry with 1 single point</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static LocationDataResponse LoadLocationDataForSinglePoint(LocationDataRequest request)
        {
            var indivData = new List<DatasetItem>();

            var p = request.Location.First().First();
            Point pointForCalculations = new Point(p.ElementAt(0), p.ElementAt(1));

            if (request.Location.Count != 1 || request.Location.First().Count != 1)
                throw new ArgumentException("Single point location data can't be retrieved with the amount of request objects");

            // the aggregation functions generate individual response items for each object and summary objects.
            // the summary objects are placed in the first parameter list.
            // since we only have a single point here, a summary would be unnecessary so we ignore it
            var housefootprints = new List<Dictionary<String, String>>();
            GetDataForPoint(housefootprints, houseFootprintsColumns, "house_footprints", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForHousefootprints(new(), indivData, 0.0, housefootprints);

            var lod2buildings = new List<Dictionary<String, String>>();
            GetDataForPoint(lod2buildings, cityGmlColumns, "building_models", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForLod2Citygml(new(), indivData, 0.0, lod2buildings);

            var actualUse = new List<Dictionary<String, String>>();
            GetDataForPoint(actualUse, actualUseColumns, "actual_use", pointForCalculations.Y, pointForCalculations.X);
            AggregateDataForActualUse(new(), indivData, 0.0, actualUse);

            // closest charging stations and number of charging stations
            var chargingStations = new List<Dictionary<String, String>>();
            GetNClosestDataForPoint(chargingStations, chargingStationsColumns, "EV_charging_stations", pointForCalculations.Y, pointForCalculations.X, 3);
            AggregateDataForChargingStations(new(), indivData, chargingStations, true);

            var pollutantStations = new List<Dictionary<String, String>>();
            GetNClosestDataForPoint(pollutantStations, airPollutionColumns, "air_pollutants", pointForCalculations.Y, pointForCalculations.X, 3);
            AggregateDataForAirPollutant(new(), indivData, pollutantStations, true);

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = indivData
            };
            return locationDataResponse;
        }

        /// <summary>
        /// Returns data from the data lake that is associated with the given polygons.
        /// This can be data that intersects with given polygons or Points of Interest in the polygons.
        /// Data will also be aggregated for array and added in the response.
        /// </summary>
        /// <param name="request">Must contain at least one object with more than 2 points</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static LocationDataResponse LoadLocationDataforPolygons(LocationDataRequest request)
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

            var pollutantStations = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                GetDataInPolygon(pollutantStations, airPollutionColumns, "air_pollutants", polygonWkt);
            AggregateDataForAirPollutant(generalData, individualData, pollutantStations, true);

            var chargingStations = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                GetDataInPolygon(chargingStations, chargingStationsColumns, "EV_charging_stations", polygonWkt);
            AggregateDataForChargingStations(generalData, individualData, chargingStations);

            var actualUse = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                GetDataInPolygon(actualUse, actualUseColumns, "actual_use", polygonWkt);
            AggregateDataForActualUse(generalData, individualData, totalAreaSearchPolygon, actualUse);

            var lod2buildings = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                GetDataInPolygon(lod2buildings, cityGmlColumns, "building_models", polygonWkt);
            AggregateDataForLod2Citygml(generalData, individualData, totalAreaSearchPolygon, lod2buildings);

            var housefootprints = new List<Dictionary<String, String>>();
            foreach (var polygonWkt in polygons)
                GetDataInPolygon(housefootprints, houseFootprintsColumns, "house_footprints", polygonWkt);
            AggregateDataForHousefootprints(generalData, individualData, totalAreaSearchPolygon, housefootprints);

            LocationDataResponse locationDataResponse = new()
            {
                SelectionData = generalData,
                IndividualData = individualData.Take(200).ToList()

            };

            return locationDataResponse;
        }

        private static void AggregateDataForChargingStations(List<DatasetItem> generalData, List<DatasetItem> individualData, List<Dictionary<string, string>> chargingStations, bool addDistance = false)
        {
            long count = chargingStations.Count;
            if (count == 0)
                return;

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
                if (addDistance && h.ContainsKey("Distance"))
                    item.Value = "Distance: " + h["Distance"] + "m";

                chargingStationsItems.Add(item);
            }
            AppendLimitedEntries(individualData, chargingStationsItems, "EV_charging_stations");
            generalData.Add(new DatasetItem
            {
                DatasetId = "EV_charging_stations",
                DisplayName = "Total number of charging stations",
                Value = chargingStationsItems.Count.ToString()
            });
        }
        private static void AggregateDataForAirPollutant(List<DatasetItem> generalData, List<DatasetItem> individualData, List<Dictionary<string, string>> airPollutants, bool addDistance = false)
        {
            long count = airPollutants.Count;
            if (count == 0)
                return;

            var airPollutionItems = new List<DatasetItem>();
            List<double> avgPm10 = new();
            List<double> avgPm25 = new();
            List<double> avgNo2 = new();
            foreach (var h in airPollutants)
            {
                Point location = GeoReader.Read(h["Location"]) as Point;
                double pm10 = double.Parse(h["pm10"]);
                double pm25 = double.Parse(h["pm2_5"]);
                double no2 = double.Parse(h["no2"]);

                List<SubdataItem> subdataItems = new();
                if (pm10 > 0)
                {
                    avgPm10.Add(pm10);
                    subdataItems.Add(new()
                    {
                        Key = "Particulate matter PM₁₀",
                        Value = h["pm10"] + "μg/m³"
                    });
                }
                if (pm25 > 0)
                {
                    avgPm25.Add(pm25);
                    subdataItems.Add(new()
                    {
                        Key = "Fine particulate matter PM₂,₅",
                        Value = h["pm2_5"] + "μg/m³"
                    });
                }
                if (no2 > 0)
                {
                    avgNo2.Add(no2);
                    subdataItems.Add(new()
                    {
                        Key = "Nitrogen oxide NO₂",
                        Value = h["no2"] + "μg/m³"
                    });
                }

                var item = new DatasetItem
                {
                    DisplayName = "Measuring station: " + h["station_name"],
                    DatasetId = "air_pollutants",
                    Coordinate = new double[] { location.Y, location.X },
                    Subdata = subdataItems
                };
                if (addDistance && h.ContainsKey("Distance"))
                    item.Value = "Distance: " + h["Distance"] + "m";

                airPollutionItems.Add(item);
            }
            individualData.AddRange(airPollutionItems);
            generalData.Add(new DatasetItem
            {
                DatasetId = "air_pollutants",
                DisplayName = "Total number of air pollution measuring stations",
                Value = airPollutionItems.Count.ToString()
            });
            generalData.Add(new DatasetItem
            {
                DatasetId = "air_pollutants",
                DisplayName = "Averaged air pollution",
                Subdata = new()
                {
                    new()
                    {
                        Key = "Average Particulate matter PM₁₀",
                        Value = avgPm10.Average().ToString("N2") + "μg/m³"
                    },
                    new()
                    {
                        Key = "Average Fine particulate matter PM₂.₅",
                        Value = avgPm25.Average().ToString("N2") + "μg/m³"
                    },
                    new()
                    {
                        Key = "Average Nitrogen oxide NO₂",
                        Value = avgNo2.Average().ToString("N2") + "μg/m³"
                    }
                }
            });
        }

        private static void AggregateDataForActualUse(List<DatasetItem> generalData, List<DatasetItem> individualData, double totalAreaSearchPolygon, List<Dictionary<string, string>> dataEntries)
        {
            long count = dataEntries.Count;
            if (count == 0)
                return;

            var usageAreaMap = new Dictionary<string, double>();
            List<DatasetItem> allEntriesForindividualData = new();
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
                allEntriesForindividualData.Add(item);
            }

            AppendLimitedEntries(individualData, allEntriesForindividualData, "actual_use");

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
            long totalCountHouseFootprints = housefootprints.Count;
            if (totalCountHouseFootprints == 0)
                return;
            double totalBuildingArea = 0.0;
            List<double> totalBuildingAreas = new List<double>();
            List<DatasetItem> allItems = new();
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
                allItems.Add(item);
            }
            AppendLimitedEntries(individualData, allItems, "house_footprints");

            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential area for Geothermal Use",
                DatasetId = "house_footprints",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("N2") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total building number",
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

            // list of values for statistics
            List<double> totalBuildingAreas = new();
            List<double> totalBuildingLivingAreas = new();
            List<double> totalBuildingRoofAreas = new();
            List<double> totalBuildingVolumes = new();
            List<double> totalSolarPotentials = new();

            List<DatasetItem> allItems = new();

            foreach (var h in citygmldata)
            {
                double area = double.Parse(h["GroundArea"]);
                totalBuildingAreas.Add(area);

                double livingArea = double.Parse(h["LivingArea"]);
                totalBuildingLivingAreas.Add(livingArea);

                double roofArea = double.Parse(h["RoofArea"]);
                totalBuildingRoofAreas.Add(roofArea);

                double wallHeight = double.Parse(h["BuildingWallHeight"]);

                double buildingVolume = double.Parse(h["BuildingVolume"]);
                totalBuildingVolumes.Add(buildingVolume);

                double solarPotential = double.Parse(h["SolarPotential"]);
                totalSolarPotentials.Add(solarPotential);

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
                        new() { Key = "Building wall height", Value = wallHeight.ToString("N2") + "m" },
                        new() { Key = "Building volume", Value = buildingVolume.ToString("N2") + "m³" },
                        new() { Key = "Roof surface", Value = roofArea.ToString("N2") + "m²" },
                        new() { Key = "Usable Area for Solar Panels", Value = solarPotential.ToString("N2") + "m²" }
                    }
                };
                allItems.Add(item);
            }

            AppendLimitedEntries(individualData, allItems, "building_models");

            double totalBuildingArea = totalBuildingAreas.Sum();
            generalData.Add(new DatasetItem
            {
                DisplayName = "Potential area for geothermal use",
                Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("N2") + "m²",
            });
            generalData.Add(new DatasetItem
            {
                DisplayName = "Total building number (LOD2)",
                DatasetId = "building_models",
                Value = totalCountLod2Buildings.ToString()
            });
            if (totalBuildingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingAreas, "Total ground area", "building_models"));
            if (totalBuildingLivingAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingLivingAreas, "Total living area", "building_models"));
            if (totalBuildingRoofAreas.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingRoofAreas, "Total roof surface", "building_models"));
            if (totalBuildingVolumes.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalBuildingVolumes, "Total building volume", "building_models", "m³"));
            if (totalSolarPotentials.Count > 0)
                generalData.Add(GenerateDatalistStatisticsEntry(totalSolarPotentials, "Total usable area for Solar Panels", "building_models"));

        }

        private static void AppendLimitedEntries(List<DatasetItem> individualData, List<DatasetItem> allEntriesForindividualData, string datasetId)
        {
            if (allEntriesForindividualData.Count > 10)
            {
                individualData.AddRange(allEntriesForindividualData.Take(10));
                individualData.Add(new()
                {
                    DisplayName = $"Skipped {allEntriesForindividualData.Count - 10} additional entries",
                    DatasetId = datasetId,
                });
            }
            else
                individualData.AddRange(allEntriesForindividualData);
        }

        private static DatasetItem GenerateDatalistStatisticsEntry(List<double> listOfDataValues, string displayText, string id, string unit = "m²")
        {
            var subdata = new List<SubdataItem>
            {
                new() { Key = "Average", Value = listOfDataValues.Average().ToString("N2") + unit },
                new() { Key = "Median", Value = listOfDataValues.Median().ToString("N2") + unit}
            };

            // Add Variance only if the list contains more than 2 elements
            if (listOfDataValues.Count > 2)
                subdata.Add(new SubdataItem { Key = "Variance", Value = listOfDataValues.Variance().ToString("N2") });

            return new DatasetItem
            {
                DisplayName = displayText,
                DatasetId = id,
                Value = listOfDataValues.Sum().ToString("N2") + unit,
                Subdata = subdata
            };
        }

        private static void GetDataInPolygon(List<Dictionary<string, string>> targetList, HashSet<string> columns, string datasetId, string polygonWkt)
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

        private static void GetDataForPoint(List<Dictionary<string, string>> targetList, HashSet<string> columns, string datasetId, double longitude, double latitude)
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

        private static void GetNClosestDataForPoint(List<Dictionary<string, string>> targetList, HashSet<string> columns, string datasetId, double longitude, double latitude, int n)
        {
            var metadata = MetadataDbHelper.GetMetadata(datasetId);
            if (metadata != null)
            {
                List<Dictionary<string, string>> allResults = new List<Dictionary<string, string>>();

                foreach (var table in metadata.additionalData.Tables)
                {
                    string command = $@"
                        SELECT TOP {n}
                            {string.Join(", ", columns)},
                            Location.STAsText() AS Location
                        FROM 
                            dbo.{datasetId}
                        WHERE
                            geometry::Point({latitude}, {longitude}, 4326).STBuffer({10000}).STIntersects(Location) = 1
                        ORDER BY 
                            geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";

                    var results = DbHelper.GetData(command).ToList();

                    foreach (var result in results)
                    {
                        var wkt = result["Location"];
                        var point2_wrong = GeoReader.Read(wkt) as Point;
                        var distance = ApiHelper.getDistance(longitude, latitude, point2_wrong);
                        result["Distance"] = distance.ToString("N2");
                        allResults.Add(result);
                    }
                }
                var sortedResults = allResults.OrderBy(r => double.Parse(r["Distance"])).Take(n).ToList();

                // Add the top n results to the target list
                targetList.AddRange(sortedResults);
            }
        }
    }


}
