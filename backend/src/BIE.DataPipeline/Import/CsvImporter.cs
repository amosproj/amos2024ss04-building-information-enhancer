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
        public CsvImporter(DataSourceDescription dataSourceDescription)
        {
            //YAML Arguments:
            this.dataSourceDescription = dataSourceDescription;
            columnTypes = ParseColumnTypes();
            //Setup Parser
            SetupParser();

            //Skip lines until header
            SkipNlines(dataSourceDescription.options.skip_lines);

            //read header
            fileHeader = ReadFileHeader();
            yamlHeader = ReadYamlHeader();
            //PrintRow(fileHeader);
            //PrintRow(yamlHeader);
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
            nextLine = "";
            while(nextLine == "")
            {
                // Console.Write($"trying to write");

                if (parser.EndOfData)
                {
                    return false;
                }
                string[] line = parser.ReadFields();

                if(line.Length == 0)
                {
                    //TODO what to do with empty lines
                    Console.WriteLine("Line is empty");
                    //Read next line
                    nextLine = "";
                    continue;

                }

                int yamlIndex = 0;
                for(int i = 0; i < line.Length; i++)
                {

                    //check if fileheader is equals to yaml header
                    // TODO: this means yaml always needs to be in the same sequence as the headers in the file
                    // also fails if there is an encoding error for a header.
                    if (fileHeader[i].Equals(yamlHeader[yamlIndex]))
                    {
                        //check if the value can be empty
                        if (dataSourceDescription.table_cols[yamlIndex].is_not_nullable && line[i] == "")
                        {
                            Console.WriteLine("Line does not match not null criteria");
                            //Read next line
                            nextLine = "";
                            continue;
                        }

                        try
                        {
                            line[i] = line[i].Replace("'", "''");
                            line[i] = line[i].Replace(",", ".");

                            if (columnTypes[i] == typeof(string))
                            {
                                nextLine += $"'{line[i]}',";
                            }
                            else
                            {
                                // nextLine += string.Format("{0},", Convert.ChangeType(line[i], columnTypes[i]));
                                nextLine += $"{line[i]},";
                            }
                        }
                        catch (System.FormatException ex)
                        {
                            Console.Error.WriteLine(string.Format("{3} Fauld parsing {0} to type {1} in column {2}",
                                                                  line[i],
                                                                  columnTypes[yamlIndex],
                                                                  fileHeader[i],
                                                                  i));
                            return false;
                        }

                        yamlIndex++;
                        continue;

                    }
                    
                    Console.WriteLine("line is: ");
                }
            }
            nextLine = RemoveLastComma(nextLine);

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

        private Type[] ParseColumnTypes()
        {
            Type[] res = new Type[dataSourceDescription.table_cols.Count];
            for(int i = 0; i < dataSourceDescription.table_cols.Count; i++)
            {
                res[i] = SQLTypeToCSharpType(dataSourceDescription.table_cols[i].type);
            }

            return res;
        }

        private static Type SQLTypeToCSharpType(string sqlType)
        {
            string shortType = RemoveLastBrackets(sqlType); //Makes VARCHAR(50) -> VARCHAR
            switch (shortType)
            {
                case "VARCHAR":
                    return typeof(string);
                case "BOOL":
                    return typeof(bool);
                case "BOOLEAN":
                    return typeof(bool);
                case "INT":
                    return typeof(int);
                case "INTEGER":
                    return typeof(int);
                case "FLOAT":
                    return typeof(float);
                case "DOUBLE":
                case "DECIMAL":
                case "DECIMAL(8,6)":
                case "DECIMAL(9,6)":
                    return typeof(double);
                default:
                    throw new NotSupportedException(string.Format("The type {0} is currently not supporteted.", shortType));
            }
        }

        private static string RemoveLastBrackets(string s)
        {
            int lastOpeningParenthesisIndex = s.LastIndexOf('(');
            if (lastOpeningParenthesisIndex != -1)
            {
                return s.Substring(0, lastOpeningParenthesisIndex);
            }
            else
            {
                return s; // No opening parenthesis found, return original string
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
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(dataSourceDescription.source.location);
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

        private void SkipNlines(int noLines)
        {
            for (int i = 0; i < noLines; i++)
            {
                //check if parser has reached end of the file
                if (parser.EndOfData)
                {
                    // Handle case where file has less than 10 lines
                    Console.WriteLine(string.Format("File has less than {0} lines", noLines));
                    return;
                }

                parser.ReadLine(); // Read and discard line
            }
        }

        private string[] ReadFileHeader()
        {
            //check if parser has reached end of the file
            if (parser.EndOfData)
            {
                //Handel case of no data
                throw new Exception("No header found");
            }

            return parser.ReadFields();
        }

        private string[] ReadYamlHeader()
        {
            string[] res = new string[dataSourceDescription.table_cols.Count];
            for(int i = 0; i < dataSourceDescription.table_cols.Count; i++)
            {
                res[i] = dataSourceDescription.table_cols[i].name;
            }

            return res;
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
