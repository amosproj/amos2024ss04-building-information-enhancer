using System.ComponentModel;
using System.Text;
using YamlDotNet.Serialization;

namespace BIE.DataPipeline.Import
{
    internal static class YamlImporter
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
                throw new FormatException(path + " is not a yaml file");
            }

            var yaml = File.ReadAllText(path, Encoding.GetEncoding("iso-8859-1"));

            var deserializer = new Deserializer();

            var description = deserializer.Deserialize<DataSourceDescription>(yaml);

            return description;
        }
    }

    internal struct DataSourceDescription
    {
        /// <summary>
        /// The location of the data source.
        /// </summary>
        public DataSourceLocation source;

        /// <summary>
        /// Options related to the data source.
        /// </summary>
        public DataSourceOptions options;

        /// <summary>
        /// The name of the database table associated with the data source.
        /// </summary>
        public string table_name;

        /// <summary>
        /// the delimiter used in the csv datasource.
        /// </summary>
        public char delimiter;

        /// <summary>
        /// The relevant columns that should be imported from the datasource.
        /// </summary>
        public List<DataSourceColumn> table_cols;

        internal struct DataSourceLocation
        {
            /// <summary>
            /// The type of location. options: filepath | URL
            /// </summary>
            public string type;

            /// <summary>
            /// The actual location.
            /// </summary>
            public string location;
        }

        internal struct DataSourceOptions
        {
            /// <summary>
            /// The number of initial lines to skip while reading the data source.
            /// </summary>
            public int skip_lines;

            /// <summary>
            /// Indicates whether to discard rows with null values.
            /// </summary>
            public bool discard_null_rows;
        }

        internal struct DataSourceColumn
        {
            /// <summary>
            /// The name of the column in the datasource.
            /// </summary>
            public string name;

            /// <summary>
            /// The name of the column in the database table.
            /// </summary>
            public string name_in_table;

            /// <summary>
            /// The data type of the column.
            /// </summary>
            public string type;

            /// <summary>
            /// True if the column is not nullable in the database.
            /// </summary>
            public bool is_not_nullable;
        }
    }

}
