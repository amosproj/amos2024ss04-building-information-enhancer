using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BIE.DataPipeline.Import
{
    internal class CityGmlImporter : IImporter
    {
        private string filePath;

        public CityGmlImporter(string filePath)
        {
            this.filePath = filePath;
        }

        public List<Building> ReadBuildings()
        {
            List<Building> buildings = new List<Building>();
            XmlDocument doc = new XmlDocument();
            XmlDocument doc2 = new XmlDocument();
            doc2.Load(filePath);
            doc.LoadXml(doc2.InnerXml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bldg", "http://www.opengis.net/citygml/building/2.0");

            XmlNodeList buildingNodes = doc.SelectNodes("//bldg:Building", nsmgr);
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
