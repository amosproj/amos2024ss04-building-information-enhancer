using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIE.DataPipeline;
using Microsoft.VisualBasic.FileIO;

namespace BIE.DataPipeline.Import
{
    internal class CsvImporter : IImporter
    {
        private TextFieldParser parser;
        private DataSourceDescription dataSourceDescription;
        private Type[] columnTypes;
        private string[] fileHeader;
        private string[] yamlHeader;
        private string headerString = "";
        private List<(int, int)> columnIndexes;

        private StringBuilder builder;

        public CsvImporter(DataSourceDescription dataSourceDescription)
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

        //tablename = name
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

        //column = col1,col2
        //string = 'name',1,'string2';
        public bool ReadLine(out string nextLine)
        {
            string[]? line;

            builder.Clear();

            while (builder.Length == 0)
            {
                
                // Console.Write($"trying to write");

                // if (parser.EndOfData)
                // {
                //     return false;
                // }

                line = parser.ReadFields();
                if (line == null)
                {
                    nextLine = "";
                    return false;
                }

                if (line.Length == 0)
                {
                    //TODO what to do with empty lines
                    Console.WriteLine("Line is empty");
                    //Read next line
                    nextLine = "";
                    continue;
                }


                foreach (var (i, yamlIndex)in columnIndexes)
                {
                    //check if the value can be empty
                    if (dataSourceDescription.table_cols[yamlIndex].is_not_nullable && line[i] == "")
                    {
                        Console.WriteLine("Line does not match not null criteria");
                        //Read next line
                        nextLine = "";
                        break;
                    }

                    try
                    {
                        line[i] = line[i].Replace("'", "''");
                        line[i] = line[i].Replace(",", ".");

                        if (columnTypes[i] == typeof(string))
                        {
                            // nextLine += $"'{line[i]}',";
                            builder.Append($"'{line[i]}',");
                            continue;
                        }

                        // nextLine += string.Format("{0},", Convert.ChangeType(line[i], columnTypes[i]));
                        // nextLine += $"{line[i]},";
                        builder.Append($"{line[i]},");
                    }
                    catch (System.FormatException ex)
                    {
                        Console.Error.WriteLine(string.Format("{3} Fauld parsing {0} to type {1} in column {2}",
                                                              line[i],
                                                              columnTypes[yamlIndex],
                                                              fileHeader[i],
                                                              i));
                        nextLine = "";
                        return false;
                    }
                }

            }

            builder.Length--; // this removes the last comma

            // nextLine = RemoveLastComma(nextLine);
            nextLine = builder.ToString();

            return true;
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


        private void SetupParser()
        {
            ValidateFilePath(dataSourceDescription.source);

            if (dataSourceDescription.source.type.Equals("filepath"))
            {
                //local path
                parser = new TextFieldParser(dataSourceDescription.source.location);
            }
            else
            {
                //Http path
                Console.WriteLine($"Grabbing Webfile {dataSourceDescription.source.location}");
                var client = new HttpClient();
                var stream = client.GetStreamAsync(dataSourceDescription.source.location).Result;
                // var stream = client.OpenRead(dataSourceDescription.source.location);
                parser = new TextFieldParser(stream);

                Console.WriteLine("File loaded.");
            }

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(dataSourceDescription.delimiter.ToString());
        }

        private void ValidateFilePath(DataSourceDescription.DataSourceLocation sourceLocation)
        {
            if (sourceLocation.type.Equals("filepath"))
            {
                if (!File.Exists(sourceLocation.location))
                {
                    throw new FileNotFoundException("The given file is not found");
                }
            }
        }

        private void PrintRow(string[] row, string[] header = null)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (header != null)
                {
                    Console.WriteLine(string.Format("{0}: {1}", header[i], row[i]));
                }
                else
                {
                    Console.WriteLine(string.Format("{0}: {1}", i, row[i]));
                }
            }
        }
    }
}