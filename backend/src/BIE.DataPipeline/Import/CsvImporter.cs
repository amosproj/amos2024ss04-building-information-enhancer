using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIE.DataPipeline;
using Microsoft.VisualBasic.FileIO;
using NetTopologySuite.Geometries;
// ReSharper disable InconsistentNaming

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
        private CultureInfo cultureInfo;

        private StringBuilder builder;

        public CsvImporter(DataSourceDescription dataSourceDescription)
        {
            // CultureInfo
            cultureInfo = new CultureInfo("en-US");

            // YAML Arguments
            this.dataSourceDescription = dataSourceDescription;
            columnTypes = ImporterHelper.ParseColumnTypes(dataSourceDescription);

            // Setup Parser
            SetupParser();

            // Skip lines until header
            SkipNlines(dataSourceDescription.options.skip_lines);

            // Create the StringBuilder used for creating the strings
            builder = new StringBuilder();

            // Read header
            fileHeader = ReadFileHeader();
            yamlHeader = ImporterHelper.ReadYamlHeader(dataSourceDescription);

            // Get all the indexes and descriptions that interest us
            columnIndexes = new List<(int, int)>();
            for (int j = 0; j < yamlHeader.Length; j++)
            {
                for (int i = 0; i < fileHeader.Length; i++)
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
        /// Creates a comma separated string with all column names for the SQL table.
        /// </summary>
        /// <returns>The SQL column name string.</returns>
        public string GetHeaderString()
        {
            // If the string is not empty the result can be returned instantly
            if (!headerString.Equals(""))
            {
                return headerString;
            }

            foreach (var col in dataSourceDescription.table_cols)
            {
                headerString += col.name_in_table + ",";
            }

            if (dataSourceDescription.options.location_to_SQL_point != null)
            {
                headerString += dataSourceDescription.options.location_to_SQL_point.name_in_table + ",";
            }

            // Remove last comma
            headerString = RemoveLastComma(headerString);

            return headerString;
        }

        /// <summary>
        /// Reads a line from the CSV file.
        /// </summary>
        /// <param name="nextLine">An output parameter that returns a line or an empty string.</param>
        /// <returns>A boolean indicating if the end of the file has been reached.</returns>
        public bool ReadLine(out string nextLine)
        {
            builder.Clear();

            while (true)
            {
                // Check if EOF has been reached before reading the next line
                if (parser.EndOfData)
                {
                    nextLine = "";
                    return false;
                }

                var line = parser.ReadFields();
                if (line == null)
                {
                    nextLine = "";
                    return false;
                }

                if (line.Length == 0)
                {
                    Console.WriteLine("Line is empty");
                    // Read next line
                    continue;
                }

                bool lineProcessed = false;
                foreach (var (fileIndex, yamlIndex) in columnIndexes)
                {
                    // Check if the line has not enough content for the expected yaml columns
                    if (fileIndex >= line.Length)
                    {
                        Console.WriteLine("Line does not match the number of expected columns");
                        // Read next line
                        builder.Clear();
                        break;
                    }

                    // Check if the value can be empty
                    if (dataSourceDescription.table_cols[yamlIndex].is_not_nullable && line[fileIndex] == "")
                    {
                        Console.WriteLine("Line does not match not null criteria, skipping...");
                        // Read next line
                        builder.Clear();
                        break;
                    }

                    line[fileIndex] = line[fileIndex].Replace("'", "''");
                    line[fileIndex] = line[fileIndex].Replace(",", ".");

                    if (columnTypes[yamlIndex] == typeof(string))
                    {
                        builder.Append($"'{line[fileIndex]}',");
                    }
                    else
                    {
                        builder.Append($"{line[fileIndex]},");
                    }

                    lineProcessed = true;
                }

                if (lineProcessed && builder.Length > 0)
                {
                    if (dataSourceDescription.options.location_to_SQL_point != null)
                    {
                        double lon;
                        double lat;
                        bool success = double.TryParse(Regex.Replace(line[dataSourceDescription.options.location_to_SQL_point.index_lon], ",", "."), NumberStyles.Any, cultureInfo, out lon);
                        success = double.TryParse(Regex.Replace(line[dataSourceDescription.options.location_to_SQL_point.index_lat], ",", "."), NumberStyles.Any, cultureInfo, out lat) && success;
                        if (success)
                        {
                            builder.Append($"{LocationToPoint(lon, lat)},");
                        }
                    }

                    builder.Length--; // This removes the last comma

                    nextLine = builder.ToString();
                    return true;
                }
            }
        }

        public string GetCreationHeader()
        {
            return headerString;
        }

        public string GetInsertHeader()
        {
            return headerString;
        }

        public IEnumerable<string> GetHeaders()
        {
            return headerString.Split(",").Select(s => s.Trim());
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

        private string LocationToPoint(double longitude, double latitude)
        {
            return "GEOMETRY::Point(" + latitude.ToString(cultureInfo) + "," + longitude.ToString(cultureInfo) + ", 4326)";
        }

        private void SetupParser()
        {
            ValidateFilePath(dataSourceDescription.source);

            if (dataSourceDescription.source.type.Equals("filepath"))
            {
                // Local path
                parser = new TextFieldParser(dataSourceDescription.source.location);
            }
            else
            {
                // Http path
                Console.WriteLine($"Grabbing Web file {dataSourceDescription.source.location}");
                var client = new HttpClient();
                Stream stream = null;

                // Retry logic
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var response = client.GetAsync(dataSourceDescription.source.location).Result;
                        response.EnsureSuccessStatusCode();
                        stream = response.Content.ReadAsStreamAsync().Result;
                        break; // Exit the loop if successful
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                        if (i == 2) throw; // Rethrow exception if all attempts fail
                    }
                }

                if (stream != null)
                {
                    parser = new TextFieldParser(stream);
                    Console.WriteLine("File loaded.");
                }
                else
                    Console.WriteLine("Failed file loading.");
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
            // Check if parser has reached end of the file
            if (parser == null || parser.EndOfData)
            {
                // Handle case of no data
                throw new Exception("No header found");
            }

            return parser.ReadFields();
        }

        private void SkipNlines(int noLines)
        {
            for (int i = 0; i < noLines; i++)
            {
                // Check if parser has reached end of the file
                if (parser.EndOfData)
                {
                    // Handle case where file has less than the specified number of lines
                    Console.WriteLine($"File has less than {noLines} lines");
                    return;
                }

                parser.ReadLine(); // Read and discard line
            }
        }
    }
}
