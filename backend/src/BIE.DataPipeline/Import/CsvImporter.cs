using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIE.DataPipeline;
using Microsoft.VisualBasic.FileIO;

[assembly: InternalsVisibleTo("BIE.Tests")]
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

        /// <summary>
        /// Returns the SQL table name for the data set given by the yaml file.
        /// </summary>
        /// <returns>The SQL table name.</returns>
        public string GetTableName()
        {
            return this.dataSourceDescription.table_name;
        }

        /// <summary>
        /// Creates a comma seperated sting with all column names for the SQL table.
        /// </summary>
        /// <returns>The SQL column name string.</returns>
        public string GetHeaderString()
        {
            //if the string is not empty the result can be returned instantly
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

        /// <summary>
        /// Reads a line from the csv file.
        /// </summary>
        /// <param name="nextLine">An output parameter that returns a line or an empty string.</param>
        /// <returns>A boolean indicating if the end of the file has been reached.</returns>
        public bool ReadLine(out string nextLine)
        {
            string[]? line;

            builder.Clear();

            while (builder.Length == 0)
            {
                line = parser.ReadFields();
                if (line == null)
                {
                    nextLine = "";
                    return false;
                }

                if (line.Length == 0)
                {
                    Console.WriteLine("Line is empty");
                    //Read next line
                    nextLine = "";
                    continue;
                }


                foreach (var (i, yamlIndex)in columnIndexes)
                {
                    //checks if the line has not enougth content for the expected yaml columns.
                    if(i >= line.Length)
                    {
                        Console.WriteLine("Line does not match the number of expected columns");
                        //Read next line
                        builder.Clear();
                        break;
                    }

                    //check if the value can be empty
                    if (dataSourceDescription.table_cols[yamlIndex].is_not_nullable && line[i] == "")
                    {
                        Console.WriteLine("Line does not match not null criteria");
                        //Read next line
                        builder.Clear();
                        break;
                    }

                    line[i] = line[i].Replace("'", "''");
                    line[i] = line[i].Replace(",", ".");

                    if (columnTypes[i] == typeof(string))
                    {
                        builder.Append($"'{line[i]}',");
                        continue;
                    }

                    builder.Append($"{line[i]},");
                }
            }

            builder.Length--; // this removes the last comma

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