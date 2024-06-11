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
            SkipNlines(dataSourceDescription.options.skip_lines);

            // create the stringbuilder used for creating the strings.
            builder = new StringBuilder();

            //read header
            fileHeader = ReadFileHeader();
            yamlHeader = ImporterHelper.ReadYamlHeader(dataSourceDescription);

            // get all the indexes and descriptions that interest us
            columnIndexes = new List<(int, int)>();
            for (int i = 0; i < fileHeader.Length; i++)
            {
                for (int j = 0; j < yamlHeader.Length; j++)
                {
                    if (fileHeader[i] != yamlHeader[j])
                    {
                        continue;
                    }

                    columnIndexes.Add((i, j));
                    break;
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
            if (!headerString.Equals(""))
            {
                return headerString;
            }

            foreach (var col in dataSourceDescription.table_cols)
            {
                headerString += col.name_in_table + ",";
            }

            //remove last ,
            headerString = RemoveLastComma(headerString);

            return headerString;
        }

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
                Console.WriteLine($"Grabbing Web file {dataSourceDescription.source.location}");
                var client = new HttpClient();
                var stream = client.GetStreamAsync(dataSourceDescription.source.location).Result;

                parser = new TextFieldParser(stream);

                Console.WriteLine("File loaded.");
            }

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(dataSourceDescription.delimiter.ToString());
        }

        private void ValidateFilePath(DataSourceDescription.DataSourceLocation sourceLocation)
        {
            if (!sourceLocation.type.Equals("filepath"))
            {
                return;
            }

            if (!File.Exists(sourceLocation.location))
            {
                throw new FileNotFoundException("The given file is not found");
            }
        }

        private string[] ReadFileHeader()
        {
            //check if parser has reached end of the file
            if (parser == null || parser.EndOfData)
            {
                //Handle case of no data
                throw new Exception("No header found");
            }

            return parser.ReadFields();
        }


        private void SkipNlines(int noLines)
        {
            for (int i = 0; i < noLines; i++)
            {
                //check if parser has reached end of the file
                if (parser.EndOfData)
                {
                    // Handle case where file has less than 10 lines
                    Console.WriteLine($"File has less than {noLines} lines");
                    return;
                }

                parser.ReadLine(); // Read and discard line
            }
        }
    }
}