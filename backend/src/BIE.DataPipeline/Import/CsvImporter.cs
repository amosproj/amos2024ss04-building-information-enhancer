using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BEI.DataPipeline.Data;
using Microsoft.VisualBasic.FileIO;

namespace BEI.DataPipeline.Import
{
    internal class CsvImporter : IImporter
    {
        private TextFieldParser parser;
        private string[] header;
        private Type[] columnTypes;
        public CsvImporter(string filePath)
        {
            //YAML Arguments:
            string path = "";
            string delimiter = ";";
            int headerRow = 11; //The row with the column titels
            columnTypes = new Type[]{ typeof(string), typeof(string), typeof(string), typeof(string), typeof(int), typeof(string), typeof(string), typeof(string), typeof(float), typeof(float), typeof(string), typeof(float), typeof(string), typeof(int), typeof(string), typeof(float), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) };


            //Setup Parser
            SetupParser(filePath, delimiter);

            //Skip lines until header
            SkipNlines(headerRow - 1);

            //read header
            header = ReadHeader();
            PrintRow(header);

            
            //Test Read File
            int noLines = 10;
            int line = 0;
            while (!parser.EndOfData && line < noLines)
            {
                //Processing row
                string[] fields = parser.ReadFields();
                PrintRow(fields, header);
                line++;
            }
        }

        public Dictionary<string, object> ReadLine()
        {
            if (parser.EndOfData)
            {
                return null;
            }
            Dictionary<string, object> res = new Dictionary<string, object>();
            string[] line = parser.ReadFields();
            if(line.Length == 0)
            {
                Console.WriteLine("Empty");
            }
            else
            {
                for(int i = 0; i < line.Length; i++)
                {
                    try
                    {
                        res.Add(header[i], Convert.ChangeType(line[i], columnTypes[i]));
                    }
                    catch (System.FormatException ex)
                    {
                        Console.Error.WriteLine(string.Format("{3} Fauld parsing {0} to type {1} in column {2}", line[i], columnTypes[i], header[i], i));
                    }
                }
            }

            return res;
        }

        private bool ValidateFilePath(string path)
        {
            bool isLocalFile = new Uri(path).IsFile;
            if (isLocalFile)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The given file is not found");
                }
            }
            return isLocalFile;
        }

        private void SetupParser(string path, string delimiter)
        {
            bool isLocalFile = ValidateFilePath(path);

            if (isLocalFile)
            {
                //local path
                parser = new TextFieldParser(path);
            }
            else
            {
                //Http path
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(path);
                parser = new TextFieldParser(stream);
            }

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(delimiter);
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
