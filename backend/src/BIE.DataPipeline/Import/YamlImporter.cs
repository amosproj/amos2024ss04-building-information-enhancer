using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using YamlDotNet.Serialization;

// ReSharper disable InconsistentNaming

[assembly: InternalsVisibleTo("BIE.Tests")]
namespace BIE.DataPipeline.Import
{
    public static class YamlImporter
    {
        /// <summary>
        /// read in a file and extract the datasource description from it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FormatException"></exception>
        public static DataSourceDescription? GetSourceDescription(string path)
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

            var yaml = File.ReadAllText(path);

            var deserializer = new Deserializer();

            var description = deserializer.Deserialize<DataSourceDescription>(yaml);

            return description;
        }
    }

    public class DataSourceDescription
    {
        /// <summary>
        /// The location of the data source.
        /// </summary>
        public DataSourceLocation source { get; set; }

        /// <summary>
        /// Options related to the data source.
        /// </summary>
        public DataSourceOptions options { get; set; }

        /// <summary>
        /// The name of the database table associated with the data source.
        /// </summary>
        public string table_name { get; set; }
        
        /// <summary>
        /// The name of the associated dataset this source belongs to
        /// </summary>
        public string dataset { get; set; }


        private char mDelimiter = ';';

        /// <summary>
        /// the delimiter used in the csv datasource.
        /// </summary>
        public char delimiter
        {
            get => mDelimiter;
            set => mDelimiter = value;
        }

        /// <summary>
        /// The relevant columns that should be imported from the datasource.
        /// </summary>
        public List<DataSourceColumn> table_cols { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            DataSourceDescription des = obj as DataSourceDescription;

            if (!source.Equals(des.source))
            {
                return false;
            }

            if (!options.Equals(des.options))
            {
                return false;
            }

            if (!table_name.Equals(des.table_name))
            {
                return false;
            }

            for (int i = 0; i < table_cols.Count; i++)
            {
                if (!table_cols[i].Equals(des.table_cols[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public class DataSourceLocation
        {
            /// <summary>
            /// The type of location. options: filepath | URL
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// The actual location.
            /// </summary>
            public string location { get; set; }

            /// <summary>
            /// the format of the data
            /// </summary>
            public string data_format { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                {
                    return false;
                }

                DataSourceLocation des = obj as DataSourceLocation;

                if (type != des.type)
                {
                    return false;
                }

                if (location != des.location)
                {
                    return false;
                }

                if (data_format != des.data_format)
                {
                    return false;
                }

                return true;
            }
        }

        public class DataSourceOptions
        {
            /// <summary>
            /// The number of initial lines to skip while reading the data source.
            /// </summary>
            public int skip_lines { get; set; }

            /// <summary>
            /// Indicates whether to discard rows with null values.
            /// </summary>
            public bool discard_null_rows { get; set; }

            private InsertBehaviour mIf_table_exists = InsertBehaviour.skip;

            /// <summary>
            /// How to deal with an existing table in the database. Options:
            /// "skip": skip this dataset, is the default
            /// "ignore": do nothing special
            /// "replace": DROP the existing table before inserting data.
            /// </summary>
            public InsertBehaviour if_table_exists
            {
                get => mIf_table_exists;
                set => mIf_table_exists = value;
            }


            [DefaultValue(null)]
            public LocationToSQLPoint location_to_SQL_point { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                {
                    return false;
                }

                DataSourceOptions des = obj as DataSourceOptions;

                if (skip_lines != des.skip_lines)
                {
                    return false;
                }

                if (discard_null_rows != des.discard_null_rows)
                {
                    return false;
                }

                if (if_table_exists != des.if_table_exists)
                {
                    return false;
                }

                return true;
            }

        }

        public class DataSourceColumn
        {
            /// <summary>
            /// The name of the column in the datasource.
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// The name of the column in the database table.
            /// </summary>
            public string name_in_table { get; set; }

            private string mType = "VARCHAR(500)";

            /// <summary>
            /// The data type of the column.
            public string type
            {
                get => mType;
                set => mType = value;
            }

            /// <summary>
            /// True if the column is not nullable in the database.
            /// </summary>
            [DefaultValue(false)]
            public bool is_not_nullable { get; set; }


            public override bool Equals(object? obj)
            {
                if (obj == null)
                {
                    return false;
                }

                DataSourceColumn des = obj as DataSourceColumn;

                if (name != des.name)
                {
                    return false;
                }

                if (name_in_table != des.name_in_table)
                {
                    return false;
                }

                if (type != des.type)
                {
                    return false;
                }

                if (is_not_nullable != des.is_not_nullable)
                {
                    return false;
                }

                return true;
            }
        }

        public class LocationToSQLPoint
        {
            /// <summary>
            /// The name of the column in the database table.
            /// </summary>
            public string name_in_table { get; set; }

            /// <summary>
            /// The index of the lonitude column in the file .
            /// </summary>
            public int index_lon { get; set; }

            /// <summary>
            /// The index of the latitude column in the file .
            /// </summary>
            public int index_lat { get; set; }
        }
    }

    public enum InsertBehaviour
    {
        ignore,
        skip,
        replace,
        none
    }
}