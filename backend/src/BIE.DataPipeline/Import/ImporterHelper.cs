﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace BIE.DataPipeline.Import
{
    internal static class ImporterHelper
    {
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


        public static string[] ReadFileHeader(TextFieldParser parser)
        {
            //check if parser has reached end of the file
            if (parser.EndOfData)
            {
                //Handel case of no data
                throw new Exception("No header found");
            }

            return parser.ReadFields();
        }

        public static string[] ReadYamlHeader(DataSourceDescription dataSourceDescription)
        {
            string[] res = new string[dataSourceDescription.table_cols.Count];
            for (int i = 0; i < dataSourceDescription.table_cols.Count; i++)
            {
                res[i] = dataSourceDescription.table_cols[i].name;
            }

            return res;
        }


        public static void SkipNlines(TextFieldParser parser, int noLines)
        {
            for (int i = 0; i < noLines; i++)
            {
                //check if parser has reached end of the file
                if (parser.EndOfData)
                {
                    // Handle case where file has less than 10 lines
                    Console.WriteLine(string.Format("File has less than {0} lines", noLines));
                    return;
                }

                parser.ReadLine(); // Read and discard line
            }
        }
    }
}
