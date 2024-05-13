using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BEI.DataPipeline.Data;
using Microsoft.VisualBasic.FileIO;

namespace BEI.DataPipeline.Import
{
    internal class CsvImporter : IImporter
    {
        private string mFileName;
        private TextFieldParser parser;
        public CsvImporter(string filePath)
        {
            //YAML Arguments:
            string delimiter = ";";
            int headerRow = 11; //The row with the column titels
            //TODO checkPath

            //Setup Parser
            SetupParser(filePath, delimiter);

            //Skip lines until header
            SkipNlines(headerRow - 1);

            //read header
            string[] header = ReadHeader();
            PrintRow(header);

            /*
            //Test Read File
            int noLines = 10;
            int line = 0;
            while (!parser.EndOfData && line < noLines)
            {
                //Processing row
                string[] fields = parser.ReadFields();
                foreach (string field in fields)
                {
                    //TODO: Process field
                    Console.WriteLine(string.Format("Filed {0}", field));
                }
                line++;
            }
            */
        }

        public async Task<ITableData> GetData()
        {
            throw new NotImplementedException();
        }

        private void SetupParser(string path, string delimiter)
        {
            parser = new TextFieldParser(path);
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
