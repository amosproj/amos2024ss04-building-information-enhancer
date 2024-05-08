using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEI.DataPipeline.Data;

namespace BEI.DataPipeline.Import
{
    internal class CsvImporter : IImporter
    {
        private string mFileName;
        CsvImporter(string filepath)
        {

        }

        public async Task<ITableData> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
