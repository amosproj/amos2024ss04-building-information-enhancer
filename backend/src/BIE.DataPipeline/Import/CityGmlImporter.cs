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
        private DataSourceDescription description;

        public CityGmlImporter(DataSourceDescription description)
        {
            this.description = description;
        }

        public XmlNodeList ReadBuildings()
        {
            XmlDocument doc = new XmlDocument();
            XmlDocument doc2 = new XmlDocument();
            doc.Load(description.source.location);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
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
            Console.WriteLine(buildingNodes.Count + " building in this file");
            return buildingNodes;
        }
        public bool ReadLine(out string nextLine)
        {
            /*
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
            }*/
            nextLine = "";
            return false;
        }
    }
}
