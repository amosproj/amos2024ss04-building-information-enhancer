using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BIE.DataPipeline.Data;
using Microsoft.VisualBasic.FileIO;

namespace BIE.DataPipeline.Import
{
    internal class CsvImporter : IImporter
    {
        private TextFieldParser parser;
        private string[] header;
        private DataSourceDescription dataSourceDescription;
        private Type[] columnTypes;
        private string tableName;
        public CsvImporter(DataSourceDescription dataSourceDescription)
        {
            //YAML Arguments:
            this.dataSourceDescription = dataSourceDescription;
            //columnTypes = ParseColumnTypes();
            columnTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(int), typeof(string), typeof(string), typeof(string), typeof(float), typeof(float), typeof(string), typeof(float), typeof(string), typeof(int), typeof(string), typeof(float), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) };

            //Setup Parser
            SetupParser();

            //Skip lines until header
            SkipNlines(dataSourceDescription.options.skip_lines);

            //read header
            header = ReadHeader();
            PrintRow(header);
        }

        //tablename = name
        public string GetTableName()
        {
            return this.dataSourceDescription.table_name;
        }

        public string GetHeaderString()
        {
            string res = "";
            foreach(string field in header)
            {
                res += field + ",";
            }

            //remove last ,
            res.Remove(res.Length - 1);
            return res;
        }

        //column = col1,col2
        //string = 'name',1,'string2';
        public bool ReadLine(out string nextLine)
        {
            nextLine = "";
            if (parser.EndOfData)
            {
                return false;
            }
            string[] line = parser.ReadFields();
            if(line.Length == 0)
            {
                //TODO what to do with empty lines
                Console.WriteLine("Empty");
            }
            else
            {
                for(int i = 0; i < line.Length; i++)
                {
                    try
                    {
                        if (columnTypes[i] == typeof(string))
                        {
                            nextLine += string.Format("'{0}',", line[i]);
                        }
                        else
                        {
                            nextLine += string.Format("{0},", Convert.ChangeType(line[i], columnTypes[i]));
                        }
                    }
                    catch (System.FormatException ex)
                    {
                        Console.Error.WriteLine(string.Format("{3} Fauld parsing {0} to type {1} in column {2}", line[i], columnTypes[i], header[i], i));
                        return false;
                    }
                }
            }

            return true;
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
            Console.WriteLine(string.Format("shorten {0} to {1}", sqlType, shortType));
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
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(dataSourceDescription.source.location);
                parser = new TextFieldParser(stream);
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

        private string[] ReadHeader()
        {
            //check if parser has reached end of the file
            if (parser.EndOfData)
            {
                //Handel case of no data
                throw new Exception("No header found");
            }

            return parser.ReadFields();
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
