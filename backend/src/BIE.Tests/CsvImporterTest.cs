using BIE.DataPipeline.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIE.Tests
{
    internal class CsvImporterTest
    {
        private CsvImporter csvImporter;

        [SetUp]
        public void SetUpCsvImporter()
        {
            csvImporter = new CsvImporter(YamlImporter.GetSourceDescription(@".\yaml\unitTest.yaml"));
        }

        [Test]
        public void TestGetTableName()
        {
            Assert.That(csvImporter.GetTableName(), Is.EqualTo("unitTestTable"));
        }
    }
}
