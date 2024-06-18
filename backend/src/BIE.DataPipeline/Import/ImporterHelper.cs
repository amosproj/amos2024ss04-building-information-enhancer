using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace BIE.DataPipeline.Import
{
    public static class ImporterHelper
    {
        /// <summary>
        /// Creates an array of c# types matching to the SQL types given by the DataSourceDescription.
        /// </summary>
        /// <param name="dataSourceDescription">The dataSourceDescription.</param>
        /// <returns>The array of C# types.</returns>
        public static Type[] ParseColumnTypes(DataSourceDescription dataSourceDescription)
        {
            Type[] res = new Type[dataSourceDescription.table_cols.Count];
            for (int i = 0; i < dataSourceDescription.table_cols.Count; i++)
            {
                res[i] = SQLTypeToCSharpType(dataSourceDescription.table_cols[i].type);
            }

            return res;
        }

        public static Type SQLTypeToCSharpType(string sqlType)
        {
            string shortType = RemoveLastBrackets(sqlType); //Makes VARCHAR(50) -> VARCHAR
            switch (shortType)
            {
                case "VARCHAR":
                    return typeof(string);
                case "BOOL":
                    return typeof(bool);
                case "BOOLEAN":
                    return typeof(bool);
                case "INT":
                    return typeof(int);
                case "INTEGER":
                    return typeof(int);
                case "FLOAT":
                    return typeof(float);
                case "DOUBLE":
                case "DECIMAL":
                case "DECIMAL(8,6)":
                case "DECIMAL(9,6)":
                    return typeof(double);
                default:
                    throw new NotSupportedException(string.Format("The type {0} is currently not supporteted.",
                                                                  shortType));
            }
        }

        /// <summary>
        /// Removes everything at the end of a string starting on the last (
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The string with the missing brackets.</returns>
        public static string RemoveLastBrackets(string s)
        {
            int lastOpeningParenthesisIndex = s.LastIndexOf('(');
            if (lastOpeningParenthesisIndex != -1)
            {
                return s.Substring(0, lastOpeningParenthesisIndex);
            }
            else
            {
                return s; // No opening parenthesis found, return original string
            }
        }

        /// <summary>
        /// Creats an array with all column headers of the source file, mentioned in the yaml file.
        /// </summary>
        /// <param name="dataSourceDescription">The description of the source file.</param>
        /// <returns>The array with the header names.</returns>
        public static string[] ReadYamlHeader(DataSourceDescription dataSourceDescription)
        {
            string[] res = new string[dataSourceDescription.table_cols.Count];
            for (int i = 0; i < dataSourceDescription.table_cols.Count; i++)
            {
                res[i] = dataSourceDescription.table_cols[i].name;
            }

            return res;
        }

        /// <summary>
        /// Prints an array of string values.
        /// The values are printed with indices if the header is null. Otherwise with the corresponding header.
        /// </summary>
        /// <param name="row">The array of strings.</param>
        /// <param name="header">The corresponding header names.</param>
        public static void PrintRow(string[] row, string[] header = null)
        {
            if(header != null && header.Length < row.Length)
            {
                throw new System.Exception("The length of header is to short");
            }

            for (int i = 0; i < row.Length; i++)
            {
                if (header != null)
                {
                    Console.WriteLine(string.Format("{0}: {1}", header[i], row[i]));
                }
                else
                {
                    Console.WriteLine(string.Format("{0}: {1}", i, row[i]));
                }
            }
        }
    }
}
