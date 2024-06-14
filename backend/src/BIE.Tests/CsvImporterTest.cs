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

        [Test]
        public void TestGetHeaderString()
        {
            string expected = "testDefault,testVarChar,checkValue,testBoolean,testInt,testInteger,testFloat,testDouble,testDecimal";
            string actual = csvImporter.GetHeaderString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TestReadLine()
        {
            string[] expected =
            {
                "'name1','1',1.5,true,,,,,",
                "'name3','3',3.5,true,,,,,",
                "'name4','4',4.5,false,,,,,",
                "'name5','5',5.5,false,,,,,",
                string.Empty,
            };

            for (int i = 0; i < 5; i++) {
                string actual = "";
                csvImporter.ReadLine(out actual);
                Assert.That(actual, Is.EqualTo(expected[i]));
            }
        }
    }
}
