using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIE.DataPipeline.Data;

namespace BIE.DataPipeline.Import
{
    internal interface IImporter
    {
        public bool ReadLine(out string nextLine);
    }
}
