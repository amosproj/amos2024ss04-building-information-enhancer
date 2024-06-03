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

        [Test]
        public void TestRemoveLastBrackets()
        {
            string input = "string with brackets (something)";
            string expected = "string with brackets ";
            string result = ImporterHelper.RemoveLastBrackets(input);
            Assert.That(result, Is.EquivalentTo(expected));

            input = "string without brackets";
            expected = "string without brackets";
            result = ImporterHelper.RemoveLastBrackets(input);
            Assert.That(result, Is.EquivalentTo(expected));


            input = "DOUBLE(1,2)";
            expected = "DOUBLE";
            result = ImporterHelper.RemoveLastBrackets(input);
            Assert.That(result, Is.EquivalentTo(expected));

            input = "BOOLEAN";
            expected = "BOOLEAN";
            result = ImporterHelper.RemoveLastBrackets(input);
            Assert.That(result, Is.EquivalentTo(expected));
        }
    }
}
