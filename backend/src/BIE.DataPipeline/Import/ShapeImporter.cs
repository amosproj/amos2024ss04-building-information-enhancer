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
        private readonly DataSourceDescription? mDataSourceDescription;
        private ShapefileDataReader mParser;
        private DbaseFileHeader mHeader;
        private readonly ICoordinateTransformation? mTransformation;

        public ShapeImporter(DataSourceDescription? dataSourceDescription)
        {
            // YAML Arguments:
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
            MemoryStream shpStream = new MemoryStream();
            MemoryStream dbfStream = new MemoryStream();

            if (mDataSourceDescription.source.type.Equals("filepath"))
            {
                // handle local Zip file:
                Console.WriteLine($"Opening local Zip file {mDataSourceDescription.source.location}");
                using (var zipStream = new FileStream(mDataSourceDescription.source.location, FileMode.Open))
                {
                    var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                    ExtractShapeFilesFromZip(zipArchive, shpStream, dbfStream);
                }
            }
            else
            {
                // handle Zip file from URL:
                Console.WriteLine($"Grabbing Webfile {mDataSourceDescription.source.location}");
                var client = new HttpClient();
                var zipStream = client.GetStreamAsync(mDataSourceDescription.source.location).Result;

                Console.WriteLine("Opening the Zip file...");
                var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                ExtractShapeFilesFromZip(zipArchive, shpStream, dbfStream);
            }

            shpStream.Position = 0;
            dbfStream.Position = 0;

            mParser = Shapefile.CreateDataReader(
                new ShapefileStreamProviderRegistry(
                    new ByteStreamProvider(StreamTypes.Shape, shpStream),
                    new ByteStreamProvider(StreamTypes.Data, dbfStream),
                    true,
                    true),
                GeometryFactory.Default);

            mHeader = mParser.DbaseHeader;
        }

        private void ExtractShapeFilesFromZip(ZipArchive zipArchive, MemoryStream shpStream, MemoryStream dbfStream)
        {
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

            if (shpStream.Length == 0 || dbfStream.Length == 0)
            {
                throw new FileNotFoundException("The required .shp and .dbf files could not be found.");
            }
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
                Console.WriteLine("Reached EOF, finishing.");
                return false;
            }

            // Append geometry as WKT (Well-Known Text)
            var geometry = mParser.Geometry;
            geometry = ConvertUtmToLatLong(geometry);

            var area = CalculateAreaInSquareMeters(geometry);

            nextLine = $"GEOMETRY::STGeomFromText('{geometry.AsText()}', 4326),"+area;

            return true;
        }
        /// <summary>
        /// Calculates Area of Polygon
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private double CalculateAreaInSquareMeters(Geometry geometry)
        {
            // Define the coordinate systems
            var wgs84 = GeographicCoordinateSystem.WGS84;
            var utm32 = ProjectedCoordinateSystem.WGS84_UTM(32, true); // UTM zone 32 for the given coordinates

            var ctfac = new CoordinateTransformationFactory();
            var transform = ctfac.CreateFromCoordinateSystems(wgs84, utm32);

            // Transform the coordinates
            var coordinates = geometry.Coordinates;
            var transformedCoordinates = new Coordinate[coordinates.Length];

            for (int i = 0; i < coordinates.Length; i++)
            {
                double[] transformed = transform.MathTransform.Transform(new double[] { coordinates[i].X, coordinates[i].Y });
                transformedCoordinates[i] = new Coordinate(transformed[0], transformed[1]);
            }

            // Ensure the polygon is closed
            if (!transformedCoordinates[0].Equals2D(transformedCoordinates[transformedCoordinates.Length - 1]))
            {
                Array.Resize(ref transformedCoordinates, transformedCoordinates.Length + 1);
                transformedCoordinates[transformedCoordinates.Length - 1] = transformedCoordinates[0];
            }

            // Create a polygon with transformed coordinates
            var geometryFactory = new GeometryFactory();
            var transformedPolygon = geometryFactory.CreatePolygon(transformedCoordinates);

            // Calculate and return the area
            return transformedPolygon.Area;
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
                // this has probably caused some issues...
                // Lat = Y , Lon = X
                coordinate.X = longitude;
                coordinate.Y = latitude;
            }

            return polygon;
        }
    }
}
