using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace BIE.DataPipeline.Import
{
    internal class YamlImporter
    {
        public static DataSourceDescription GetSourceDescription(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("file: " + path + " could not be found.");
            }

            if (!path.EndsWith(".yaml"))
            {
                throw new FormatException(path + "is not a yaml file");
            }

            var yaml = File.ReadAllText(path);

            var deserializer = new Deserializer();

            var description = deserializer.Deserialize<DataSourceDescription>(yaml);

            return description;
        }
    }

    internal struct DataSourceDescription
    {
        public DataSourceLocation source;
        public DataSourceOptions options;
        public string table_name;
        public List<DataSourceColumn> table_cols;

        internal struct DataSourceLocation
        {
            public string type;
            public string location;
        }

        internal struct DataSourceOptions
        {
            public int skip_lines;
            public bool discard_null_rows;
        }

        internal struct DataSourceColumn
        {
            public string name;
            public string name_in_table;
            public string type;
        }

    }
}
