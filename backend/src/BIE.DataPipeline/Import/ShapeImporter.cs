using Microsoft.VisualBasic.FileIO;
using MySql.Data.MySqlClient.X.XDevAPI.Common;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using YamlDotNet.Core;

namespace BIE.DataPipeline.Import
{
    internal class ShapeImporter : IImporter
    {
        private DataSourceDescription dataSourceDescription;
        private Type[] columnTypes;
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
            columnTypes = ImporterHelper.ParseColumnTypes(dataSourceDescription);
            builder = new StringBuilder();
            //Setup Parser
            SetupParser();

            //Skip lines until header
            SkipNlines(0);//(dataSourceDescription.options.skip_lines);

            //read header
            fileHeader = ReadFileHeader();
            //ImporterHelper.PrintRow(fileHeader);
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
            parser = new ShapefileDataReader(@"C:\Users\nicol\Downloads\091_Oberbayern_Hausumringe\hausumringe", NetTopologySuite.Geometries.GeometryFactory.Default);
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

                int fieldCount = header.NumFields;
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
                }

                // Append geometry as WKT (Well-Known Text)
                Geometry geometry = parser.Geometry;
                builder.Append($", '{geometry.AsText()}'");
                builder.Append(");");

                // Print the SQL insert statement
                Console.WriteLine(builder.ToString());
                nextLine = builder.ToString();
                return true;
            }catch(Exception ex)
            {
                nextLine = "";
                return false;
            }
        }

        private string[] ReadFileHeader()
        {
            //check if parser has reached end of the file
            if (!parser.Read())
            {
                //Handel case of no data
                throw new Exception("No header found");
            }

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


    }
}
