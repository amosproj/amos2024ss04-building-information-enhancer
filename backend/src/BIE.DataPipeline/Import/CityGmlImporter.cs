using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BIE.DataPipeline.Import
{
    class CityGmlImporter : IImporter
    {
        private string filePath;

        public CityGmlImporter(string filePath)
        {
            this.filePath = filePath;
        }

        public List<Building> ReadBuildings()
        {
            string xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
        <CityModel xmlns:tex=""http://www.opengis.net/citygml/texturedsurface/1.0"" xmlns:sch=""http://www.ascc.net/xml/schematron"" xmlns:veg=""http://www.opengis.net/citygml/vegetation/1.0"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:tran=""http://www.opengis.net/citygml/transportation/1.0"" xmlns:grp=""http://www.opengis.net/citygml/cityobjectgroup/1.0"" xmlns:base=""http://www.citygml.org/citygml/profiles/base/1.0"" xmlns:bldg=""http://www.opengis.net/citygml/building/1.0"" xmlns:wtr=""http://www.opengis.net/citygml/waterbody/1.0"" xmlns:dem=""http://www.opengis.net/citygml/relief/1.0"" xmlns:gen=""http://www.opengis.net/citygml/generics/1.0"" xmlns:app=""http://www.opengis.net/citygml/appearance/1.0"" xmlns:frn=""http://www.opengis.net/citygml/cityfurniture/1.0"" xmlns:smil20=""http://www.w3.org/2001/SMIL20/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:smil20lang=""http://www.w3.org/2001/SMIL20/Language"" xmlns:xAL=""urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"" xmlns=""http://www.opengis.net/citygml/1.0"" xmlns:luse=""http://www.opengis.net/citygml/landuse/1.0"" xsi:schemaLocation=""http://www.opengis.net/citygml/1.0 http://repository.gdi-de.org/schemas/adv/citygml/1.0/cityGMLBaseLoD2.xsd http://www.opengis.net/citygml/building/1.0 http://repository.gdi-de.org/schemas/adv/citygml/building/1.0/buildingLoD2.xsd http://www.opengis.net/citygml/appearance/1.0 http://repository.gdi-de.org/schemas/adv/citygml/appearance/1.0/appearanceLoD2.xsd http://www.opengis.net/citygml/generics/1.0 http://repository.gdi-de.org/schemas/adv/citygml/generics/1.0/genericsLoD2.xsd"">
        <gml:name>LoD2_32_652_5496_2_BY</gml:name>
        <gml:boundedBy>
        <gml:Envelope srsName=""urn:adv:crs:ETRS89_UTM32*DE_DHHN2016_NH"" srsDimension=""3"">
        <gml:lowerCorner>652011.2 5495982.502 301.5</gml:lowerCorner>
        <gml:upperCorner>654012.1 5498005.797 357.447</gml:upperCorner>
        </gml:Envelope>
        </gml:boundedBy>
        <cityObjectMember xmlns:bldg='http://www.opengis.net/citygml/building/2.0' xmlns:gml='http://www.opengis.net/gml'>
            <bldg:Building gml:id='DEBY_LOD2_107309624'>
                <creationDate>2021-11-24</creationDate>
                <externalReference>
                    <informationSystem>http://repository.gdi-de.org/schemas/adv/citygml/fdv/art.htm#_9100</informationSystem>
                    <externalObject>
                        <name>DEBYvAAAAABsfzSK</name>
                    </externalObject>
                </externalReference>
            </bldg:Building>
        </cityObjectMember>
        </CityModel>
";

            List<Building> buildings = new List<Building>();
            XmlDocument doc = new XmlDocument();
            XmlDocument doc2 = new XmlDocument();
            doc.Load(filePath);
            doc.LoadXml(doc.InnerXml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            /*
            
            */
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


            XmlNodeList buildingNodes = doc.SelectNodes("//bldg:Building", nsmgr);
            Console.WriteLine(buildingNodes.Count);
            foreach (XmlNode node in buildingNodes)
            {
                Console.WriteLine("Building ID: " + node.Attributes["gml:id"].Value);
                Console.WriteLine("Creation Date: " + node["creationDate"].InnerText);
                XmlNode externalReference = node["externalReference"];
                if (externalReference != null)
                {
                    Console.WriteLine("Information System: " + externalReference["informationSystem"].InnerText);
                    XmlNode externalObject = externalReference["externalObject"];
                    if (externalObject != null)
                    {
                        Console.WriteLine("External Object Name: " + externalObject["name"].InnerText);
                    }
                }
            }

            return buildings;
        }
        public bool ReadLine(out string nextLine)
        {
            throw new NotImplementedException();
        }
    }

    public class Building
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Positions { get; set; } = new List<string>();
    }
    




}
