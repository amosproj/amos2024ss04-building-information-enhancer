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
        public CsvImporter(string filepath)
        {
            //YAML Arguments:
            string delimiter = ";";
            //TODO checkPath

            //Setup Parser
            parser = new TextFieldParser(filepath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(delimiter);

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
        }

        public async Task<ITableData> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
