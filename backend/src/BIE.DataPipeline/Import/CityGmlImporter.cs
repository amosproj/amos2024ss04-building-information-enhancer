using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.IO.CoordinateSystems;
using NetTopologySuite.Algorithm;

namespace BIE.DataPipeline.Import
{
    class CityGmlImporter : IImporter
    {
        private DataSourceDescription description;
        private int buildingIndex = 0;
        private XmlNodeList buildingNodes;
        private XmlNamespaceManager nsmgr;

        private readonly ICoordinateTransformation? mTransformation;

        public CityGmlImporter(DataSourceDescription description)
        {
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
            this.description = description;
            this.buildingNodes = ReadBuildings();
        }

        public XmlNodeList ReadBuildings()
        {
            XmlDocument doc = new XmlDocument();
            this.nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bldg", "http://www.opengis.net/citygml/building/1.0");
            nsmgr.AddNamespace("tex", "http://www.opengis.net/citygml/texturedsurface/1.0");
            nsmgr.AddNamespace("sch", "http://www.ascc.net/xml/schematron");
            nsmgr.AddNamespace("veg", "http://www.opengis.net/citygml/vegetation/1.0");
            nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
            nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
            nsmgr.AddNamespace("tran", "http://www.opengis.net/citygml/transportation/1.0");
            nsmgr.AddNamespace("grp", "http://www.opengis.net/citygml/cityobjectgroup/1.0");
            nsmgr.AddNamespace("base", "http://www.citygml.org/citygml/profiles/base/1.0");
            nsmgr.AddNamespace("wtr", "http://www.opengis.net/citygml/waterbody/1.0");
            nsmgr.AddNamespace("dem", "http://www.opengis.net/citygml/relief/1.0");
            nsmgr.AddNamespace("gen", "http://www.opengis.net/citygml/generics/1.0");
            nsmgr.AddNamespace("app", "http://www.opengis.net/citygml/appearance/1.0");
            nsmgr.AddNamespace("frn", "http://www.opengis.net/citygml/cityfurniture/1.0");
            nsmgr.AddNamespace("smil20", "http://www.w3.org/2001/SMIL20/");
            nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsmgr.AddNamespace("smil20lang", "http://www.w3.org/2001/SMIL20/Language");
            nsmgr.AddNamespace("xAL", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");
            nsmgr.AddNamespace(string.Empty, "http://www.opengis.net/citygml/1.0"); // Default namespace
            nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
            doc.Load(description.source.location);



            XmlNodeList buildingNodes = doc.SelectNodes("//bldg:Building", nsmgr);
            buildingIndex = 0;
            Console.WriteLine(buildingNodes.Count + " building in this file");
            return buildingNodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextLine"></param>
        /// <returns>SQL Polygon, xml data</returns>
        public bool ReadLine(out string nextLine)
        {
            if(this.buildingIndex < buildingNodes.Count)
            {
                XmlNode buildingNode = buildingNodes[this.buildingIndex];
                XmlNode groundSurfaceNode = buildingNode.SelectSingleNode(".//bldg:GroundSurface", this.nsmgr); 
                if(groundSurfaceNode == null)
                {
                    Console.WriteLine("No Ground surface found");
                    this.buildingIndex++;
                    nextLine = "";
                    return true;
                }
                XmlNode positionNode = groundSurfaceNode.SelectSingleNode(".//gml:posList", this.nsmgr);
                if (positionNode == null)
                {
                    Console.WriteLine("The ground surface has no position");
                    this.buildingIndex++;
                    nextLine = "";
                    return true;
                }

                Geometry geometry = UtmCoordinatesToGeometry(positionNode.InnerText);

                float groundHeight = GetBuildingGroundHeight(buildingNode);
                string districtKey = GetBuildingDistrictKey(buildingNode);

                nextLine = $"geography::STGeomFromText('{geometry.AsText()}', 4326)";
                nextLine += string.Format(",'{0}'", buildingNode.InnerXml);
                nextLine += string.Format(",'{0}'", groundHeight.ToString());//TODO add culture info
                nextLine += string.Format(",'{0}'", districtKey);

                this.buildingIndex++;
                return true;
            }
            else
            {
                nextLine = "";
                return false;
            }
        }

        private Geometry UtmCoordinatesToGeometry(string utmCoordinates)
        {
            //Console.WriteLine(utmCoordinates);
            //Parse Coordinate string
            var coordPairs = new List<(double Easting, double Northing)>();
            var coords = utmCoordinates.Split(' ');

            for (int i = 0; i < coords.Length; i += 3)
            {
                if (double.TryParse(coords[i], out double easting) && double.TryParse(coords[i + 1], out double northing))
                {
                    coordPairs.Add((easting, northing));
                }
            }

            //convert to wgs84
            var wgs84Coordinates = new List<(double Lon, double Lat)>();

            var csFactory = new CoordinateSystemFactory();
            var utmZone32 = CoordinateSystemWktReader
            .Parse("PROJCS[\"WGS 84 / UTM zone 32N\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS" +
                   " 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]]" +
                   ",PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433," +
                   "AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION" +
                   "[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER" +
                   "[\"central_meridian\",9],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\"" +
                   ",500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]" +
                   ",AUTHORITY[\"EPSG\",\"32632\"]]");
            var utm32 = csFactory.CreateFromWkt(utmZone32.ToString());
            var wgs84 = GeographicCoordinateSystem.WGS84;

            var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(utm32, wgs84).MathTransform;

            foreach (var utmCoord in coordPairs)
            {
                double[] utmPoint = { utmCoord.Easting, utmCoord.Northing };
                double[] wgs84Point = transform.Transform(utmPoint);
                wgs84Coordinates.Add((wgs84Point[0], wgs84Point[1]));
            }

            //Convert to polygon
            if (wgs84Coordinates.Count < 4 || !wgs84Coordinates[0].Equals(wgs84Coordinates[wgs84Coordinates.Count - 1]))
            {
                throw new ArgumentException("The coordinates must form a closed ring with at least 4 points.");
            }

            var coordinateArray = new Coordinate[wgs84Coordinates.Count];
            for (int i = 0; i < wgs84Coordinates.Count; i++)
            {
                coordinateArray[i] = new Coordinate(wgs84Coordinates[i].Lon, wgs84Coordinates[i].Lat);
            }

            var geometryFactory = new GeometryFactory();
            var linearRing = new LinearRing(coordinateArray);
            return new Polygon(linearRing);
        }

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

        private float GetBuildingGroundHeight(XmlNode buildingNode)
        {
            XmlNode groundHeightNode = buildingNode.SelectSingleNode(".//gen:stringAttribute[@name='DatenquelleBodenhoehe']/gen:value", this.nsmgr);

            if(groundHeightNode == null)
            {
                Console.WriteLine("No ground height node");
                return -1;
            }

            float result = 0;
            if(!float.TryParse(groundHeightNode.InnerText, out result)) //TODO add culture info
            {
                Console.WriteLine("Unable to get ground height");
                return -1;
            }

            return result;
        }

        private string GetBuildingDistrictKey(XmlNode buildingNode)
        {
            XmlNode distrcitKeyNode = buildingNode.SelectSingleNode(".//gen:stringAttribute[@name='Gemeindeschluessel']/gen:value", this.nsmgr);

            if (distrcitKeyNode == null)
            {
                Console.WriteLine("No ground height node");
                return "";
            }

            return distrcitKeyNode.InnerText;
        }
    }
}
