using Microsoft.VisualBasic.FileIO;
using MySql.Data.MySqlClient.X.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using YamlDotNet.Core;
using System.Data.SqlClient;
using NetTopologySuite.IO.ShapeFile.Extended;
using Microsoft.SqlServer;
using NetTopologySuite.Utilities;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Security.Policy;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using ProjNet.IO.CoordinateSystems;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;


namespace BIE.DataPipeline.Import
{
    internal class ShapeImporter : IImporter
    {
        private DataSourceDescription dataSourceDescription;
        private string[] fileHeader;
        private string[] yamlHeader;
        private string headerString = "";
        private List<(int, int)> columnIndexes;
        private ShapefileDataReader parser;
        DbaseFileHeader header;
        StringBuilder builder;

        public ShapeImporter(DataSourceDescription dataSourceDescription)
        {
            //YAML Arguments:
            this.dataSourceDescription = dataSourceDescription;
            builder = new StringBuilder();
            //Setup Parser
            SetupParser();

            //Skip lines until header
            // SkipNlines(0); //(dataSourceDescription.options.skip_lines);

            //read header
            fileHeader = ReadFileHeader();
            ImporterHelper.PrintRow(fileHeader);
            /*yamlHeader = ImporterHelper.ReadYamlHeader(dataSourceDescription);

            // get all the indexes and descriptions that interest us
            columnIndexes = new List<(int, int)>();
            for (int i = 0; i < fileHeader.Length; i++)
            {
                for (int j = 0; j < yamlHeader.Length; j++)
                {
                    if (fileHeader[i] == yamlHeader[j])
                    {
                        columnIndexes.Add((i, j));
                        // Console.WriteLine($"adding columnindexes: {i}, {j}");
                        break;
                    }
                }
            }*/
        }

        public void SetupParser()
        {
            // handle Zip file:

            Console.WriteLine($"Grabbing Webfile {dataSourceDescription.source.location}");
            var client = new HttpClient();
            var zipStream = client.GetStreamAsync(dataSourceDescription.source.location).Result;

            Console.WriteLine("opening Zip file");
            var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            var shpStream = new MemoryStream();
            var dbfStream = new MemoryStream();
            foreach (var zipArchiveEntry in zipArchive.Entries)
            {
                if (zipArchiveEntry.Name.EndsWith(".shp"))
                {
                    zipArchiveEntry.Open().CopyTo(shpStream);
                    shpStream.Position = 0;
                    continue;
                }

                if (zipArchiveEntry.Name.EndsWith(".dbf"))
                {
                    zipArchiveEntry.Open().CopyTo(dbfStream);
                    dbfStream.Position = 0;
                }
            }

            if (shpStream == null || dbfStream == null)
            {
                throw new FileNotFoundException("the required .shp and .bdf files could not be found.");
            }

            shpStream.Position = 0;
            dbfStream.Position = 0;
            Console.WriteLine("File loaded.");

            parser =
                Shapefile.CreateDataReader(
                                new ShapefileStreamProviderRegistry(
                                                new ByteStreamProvider(
                                                                       StreamTypes.Shape, shpStream),
                                                  new ByteStreamProvider(
                                                                         StreamTypes.Data, dbfStream),
                                                true,
                                                true),
                                           GeometryFactory.Default);

            // parser = new ShapefileDataReader(@"D:\datasets\093_Oberpfalz_Hausumringe\hausumringe",
            //                                  NetTopologySuite.Geometries.GeometryFactory.Default);
            header = parser.DbaseHeader;
        }


        public bool ReadLine(out string nextLine)
        {
            nextLine = "";
            try
            {
                if (!parser.Read())
                {
                    Console.WriteLine("EOF");
                    return false;
                }

                /*int fieldCount = header.NumFields;
                for (int i = 0; i < fieldCount; i++)
                {
                    var value = parser.GetValue(i);
                    if (value is string)
                    {
                        builder.Append($"'{value.ToString().Replace("'", "''")}'");
                    }
                    else if (value is DateTime dateTime)
                    {
                        builder.Append($"'{dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}'");
                    }
                    else if (value is double || value is float)
                    {
                        builder.Append($"{Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        builder.Append(value);
                    }
                    if (i < fieldCount - 1)
                        builder.Append(", ");
                }*/

                // Append geometry as WKT (Well-Known Text)
                Geometry geometry = parser.Geometry;
                geometry = Convert(geometry);

                string geo = ($"geography::STGeomFromText('{geometry.AsText()}', 4326)");

                // Print the SQL insert statement
                Console.WriteLine(builder.ToString());
                nextLine = geo;
                return true;
            }
            catch (Exception ex)
            {
                nextLine = "";
                return false;
            }
        }

        private string[] ReadFileHeader()
        {
            int fieldCount = header.NumFields;
            string[] res = new string[fieldCount];

            // Append column names
            for (int i = 0; i < fieldCount; i++)
            {
                res[i] = header.Fields[i].Name;
            }

            return res;
        }


        private void SkipNlines(int noLines)
        {
            for (int i = 0; i < noLines; i++)
            {
                // Read and discard line
                if (!parser.Read())
                {
                    // Handle case where file has less than 10 lines
                    Console.WriteLine(string.Format("File has less than {0} lines", noLines));
                    return;
                }
            }
        }

        public string GetTableName()
        {
            return this.dataSourceDescription.table_name;
        }

        public string GetHeaderString()
        {
            if (headerString.Equals(""))
            {
                foreach (DataSourceDescription.DataSourceColumn col in this.dataSourceDescription.table_cols)
                {
                    headerString += col.name_in_table + ",";
                }

                //remove last ,
                headerString = RemoveLastComma(headerString);
            }

            return headerString;
        }

        private static string RemoveLastComma(string input)
        {
            int lastCommaIndex = input.LastIndexOf(',');
            if (lastCommaIndex != -1)
            {
                return input.Remove(lastCommaIndex, 1);
            }
            else
            {
                return input; // No comma found, return original string
            }
        }

        public static Geometry Convert(Geometry polygon)
        {
            // Define the source and target coordinate systems
            IInfo utmZone32 =
                CoordinateSystemWktReader
                    .Parse("PROJCS[\"WGS 84 / UTM zone 32N\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",9],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AUTHORITY[\"EPSG\",\"32632\"]]");
            GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            // Create coordinate transformation
            var transformation =
                new CoordinateTransformationFactory().CreateFromCoordinateSystems((CoordinateSystem)utmZone32, wgs84);


            // Convert UTM coordinates to latitude and longitude
            foreach (Coordinate coordinate in polygon.Coordinates)
            {
                double utmEasting = coordinate.X;
                double utmNorthing = coordinate.Y;
                double[] utmPoint = { utmEasting, utmNorthing };
                double[] wgs84Point = transformation.MathTransform.Transform(utmPoint);

                // Extract latitude and longitude
                double latitude = wgs84Point[1];
                double longitude = wgs84Point[0];
                coordinate.X = latitude;
                coordinate.Y = longitude;
                //Console.WriteLine("Latitude: " + latitude);
                //Console.WriteLine("Longitude: " + longitude);
            }

            return polygon;
        }
    }
}