using BIE.DataPipeline.Import;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BIE.DataPipeline.Import.DataSourceDescription;

namespace BIE.Tests
{
    internal class YamlImporterTest
    {
        [Test]
        public void TestGetSourceDescription()
        {
            DataSourceDescription expected = new DataSourceDescription();
            DataSourceLocation expectedLocation = new DataSourceLocation();
            expectedLocation.type = "filepath";
            expectedLocation.data_format = "CSV";
            expectedLocation.location = ".\\..\\..\\..\\testData\\unitTest.csv";
            expected.source = expectedLocation;
            expected.delimiter = ';';
            DataSourceOptions expectedOptions = new DataSourceOptions();
            expectedOptions.discard_null_rows = false;
            expectedOptions.skip_lines = 7;
            expectedOptions.if_table_exists = InsertBehaviour.replace;
            expected.options = expectedOptions;
            DataSourceColumn expectedColumn = new DataSourceColumn();
            expectedColumn.is_not_nullable = false;
            expectedColumn.name = "colName";
            expectedColumn.name_in_table = "colTableName";
            expectedColumn.type = "VARCHAR";
            expected.table_cols = new List<DataSourceColumn>
            {
                expectedColumn,
            };
            expected.table_name = "unitTestTable";

            DataSourceDescription actual = YamlImporter.GetSourceDescription(@".\yaml\common\unitTestShort.yaml");
            Assert.True(actual.Equals(expected));
        }
    }
}
