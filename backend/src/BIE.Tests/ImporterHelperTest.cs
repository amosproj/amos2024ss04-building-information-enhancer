using BIE.DataPipeline.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIE.Tests
{
    public class ImporterHelperTest
    {
        private DataSourceDescription description;

        [SetUp]
        public void SetUp()
        {
            description = YamlImporter.GetSourceDescription(@".\yaml\unitTest.yaml");
        }

        [Test]
        public void TestParseColumnTypes()
        {
            Type[] expected = { 
                typeof(string), 
                typeof(string),
                typeof(bool),
                typeof(bool),
                typeof(int),
                typeof(int),
                typeof(float),
                typeof(double),
                typeof(double)
            }; 
            Type[] result = ImporterHelper.ParseColumnTypes(description);
            Assert.That(result, Is.EquivalentTo(expected));
        }
    }
}
