using System.Text;
using System.IO.Compression;
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
        private readonly DataSourceDescription mDataSourceDescription;
        private ShapefileDataReader mParser;
        private DbaseFileHeader mHeader;
        private readonly ICoordinateTransformation? mTransformation;

        public ShapeImporter(DataSourceDescription dataSourceDescription)
        {
            //YAML Arguments:
            this.mDataSourceDescription = dataSourceDescription;

            new StringBuilder();

            SetupParser();

            ReadFileHeader();

            // Define the source and target coordinate systems
            var utmZone32 = CoordinateSystemWktReader
                .Parse("PROJCS[\"WGS 84 / UTM zone 32N\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS" +
                       " 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]]" +
                       ",PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433," +
                       "AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION" +
                       "[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER" +
                       "[\"central_meridian\",9],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\"" +
                       ",500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]" +
                       ",AUTHORITY[\"EPSG\",\"32632\"]]");
            var wgs84 = GeographicCoordinateSystem.WGS84;
            // Create coordinate transformation
            mTransformation =
                new CoordinateTransformationFactory().CreateFromCoordinateSystems((CoordinateSystem)utmZone32, wgs84);
        }

        private void SetupParser()
        {
            // handle Zip file:

            Console.WriteLine($"Grabbing Webfile {mDataSourceDescription.source.location}");
            var client = new HttpClient();
            var zipStream = client.GetStreamAsync(mDataSourceDescription.source.location).Result;

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

            mParser =
                Shapefile.CreateDataReader(
                                           new ShapefileStreamProviderRegistry(
                                                                               new ByteStreamProvider(
                                                                                                      StreamTypes.Shape,
                                                                                                      shpStream),
                                                                               new ByteStreamProvider(
                                                                                                      StreamTypes.Data,
                                                                                                      dbfStream),
                                                                               true,
                                                                               true),
                                           GeometryFactory.Default);

            // parser = new ShapefileDataReader(@"D:\datasets\093_Oberpfalz_Hausumringe\hausumringe",
            //                                  NetTopologySuite.Geometries.GeometryFactory.Default);
            mHeader = mParser.DbaseHeader;
        }


        /// <summary>
        /// reads the next line from the Shapefile and returns it as WKT (Well known text)
        /// </summary>
        /// <param name="nextLine"></param>
        /// <returns></returns>
        public bool ReadLine(out string nextLine)
        {
            nextLine = "";
            if (!mParser.Read())
            {
                Console.WriteLine("EOF");
                return false;
            }

            // Append geometry as WKT (Well-Known Text)
            var geometry = mParser.Geometry;
            geometry = ConvertUtmToLatLong(geometry);

            nextLine = $"geography::STGeomFromText('{geometry.AsText()}', 4326)";

            return true;
        }

        private string[] ReadFileHeader()
        {
            var fieldCount = mHeader.NumFields;
            var res = new string[fieldCount];

            // Append column names
            for (int i = 0; i < fieldCount; i++)
            {
                res[i] = mHeader.Fields[i].Name;
            }

            return res;
        }

        /// <summary>
        /// Conver UTM coordinates to Latitude and Longitude
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private Geometry ConvertUtmToLatLong(Geometry polygon)
        {
            // Convert UTM coordinates to latitude and longitude
            foreach (Coordinate coordinate in polygon.Coordinates)
            {
                double utmEasting = coordinate.X;
                double utmNorthing = coordinate.Y;
                double[] utmPoint = { utmEasting, utmNorthing };
                double[] wgs84Point = mTransformation!.MathTransform.Transform(utmPoint);

                // Extract latitude and longitude
                double latitude = wgs84Point[1];
                double longitude = wgs84Point[0];
                coordinate.X = latitude;
                coordinate.Y = longitude;
            }

            return polygon;
        }
    }
}