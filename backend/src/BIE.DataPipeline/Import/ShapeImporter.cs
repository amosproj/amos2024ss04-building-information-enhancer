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

namespace BIE.DataPipeline.Import
{
    internal class ShapeImporter : IImporter
    {
        private TextFieldParser parser;
        private DataSourceDescription dataSourceDescription;
        private Type[] columnTypes;
        private string[] fileHeader;
        private string[] yamlHeader;
        private string headerString = "";
        private List<(int, int)> columnIndexes;
        private StringBuilder builder;
        private StringBuilder sqlInsert;
        private ShapefileDataReader reader;
        DbaseFileHeader header;
        public ShapeImporter(DataSourceDescription dataSourceDescription)
        {
            //YAML Arguments:
            this.dataSourceDescription = dataSourceDescription;
            columnTypes = ImporterHelper.ParseColumnTypes(dataSourceDescription);
            //Setup Parser
            SetupParser();

            //Skip lines until header
            ImporterHelper.SkipNlines(parser, dataSourceDescription.options.skip_lines);

            // create the stringbuilder used for creating the strings.
            builder = new StringBuilder();

            //read header
            fileHeader = ImporterHelper.ReadFileHeader(parser);
            yamlHeader = ImporterHelper.ReadYamlHeader(dataSourceDescription);

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
            }
            
        }

        public void SetupParser()
        {
            reader = new ShapefileDataReader(dataSourceDescription.source.ToString(), NetTopologySuite.Geometries.GeometryFactory.Default);
            header = reader.DbaseHeader;

            sqlInsert = new StringBuilder();
        }


        public bool ReadLine(out string nextLine)
        {
            try
            {
                int fieldCount = header.NumFields;
                sqlInsert.Clear();
                /*sqlInsert.Append($"INSERT INTO {tableName} (");

                // Append column names
                for (int i = 0; i < fieldCount; i++)
                {
                    sqlInsert.Append(header.Fields[i].Name);
                    if (i < fieldCount - 1)
                        sqlInsert.Append(", ");
                }
                sqlInsert.Append(", Geometry) VALUES (");*/

                // Append values
                for (int i = 0; i < fieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    if (value is string)
                    {
                        sqlInsert.Append($"'{value.ToString().Replace("'", "''")}'");
                    }
                    else if (value is DateTime dateTime)
                    {
                        sqlInsert.Append($"'{dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}'");
                    }
                    else if (value is double || value is float)
                    {
                        sqlInsert.Append($"{Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        sqlInsert.Append(value);
                    }
                    if (i < fieldCount - 1)
                        sqlInsert.Append(", ");
                }

                // Append geometry as WKT (Well-Known Text)
                Geometry geometry = reader.Geometry;
                sqlInsert.Append($", '{geometry.AsText()}'");
                sqlInsert.Append(");");

                // Print the SQL insert statement
                Console.WriteLine(sqlInsert.ToString());
                nextLine = sqlInsert.ToString();
                return true;
            }catch(Exception ex)
            {
                nextLine = "";
                return false;
            }
        }

        
    }
}
