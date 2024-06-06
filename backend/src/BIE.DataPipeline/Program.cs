// See https://aka.ms/new-console-template for more information
using BIE.DataPipeline;
using BIE.DataPipeline.Import;
using Mono.Options;

var tableInsertBehaviour = InsertBehaviour.none;
// setup command line options.
var options = new OptionSet
{
    {
        "b|behaviour", @"Behaviour when inserting into a database. Options:
replace: drop existing table before inserting.
skip: do not insert when table already exists.
ignore: always try to insert regardles if table already exists or not.",
        behaviour =>
        {
            switch (behaviour)
            {
                case "replace":
                    tableInsertBehaviour = InsertBehaviour.replace;
                    break;
                case "skip":
                    tableInsertBehaviour = InsertBehaviour.skip;
                    break;
                case "ignore":
                    tableInsertBehaviour = InsertBehaviour.ignore;
                    break;
                default:
                    Console.WriteLine($"{behaviour} is not a recognized behaviour.");
                    Environment.Exit(42);
                    break;
            }
        }
    }
};
options.Parse(args);

Console.WriteLine("Parser Started");
DataSourceDescription description = YamlImporter.GetSourceDescription(args[0]);

if (tableInsertBehaviour != InsertBehaviour.none)
{
    description.options.if_table_exists = tableInsertBehaviour;
    Console.WriteLine($"Using {tableInsertBehaviour} Behaviour for insertion.");
}

var dbHelper = new DBHelper();
IImporter importer;

switch (description.source.data_format)
{
    case "CSV":
        var csvImporter = new CsvImporter(description);
        dbHelper.SetInfo(description.table_name, csvImporter.GetHeaderString());
        importer = csvImporter;
        break;

    case "SHAPE":
        importer = new ShapeImporter(description);
        dbHelper.SetInfo( description.table_name, "Location");
        break;

    default:
        throw new ArgumentException($"Could not determine data format: {description.source.data_format}");
}

if (!dbHelper.CreateTable(description))
{
    return 0;
}


//Console.WriteLine(csvImporter.GetHeaderString());
var line = "";
bool notEof = importer.ReadLine(out line);

Console.WriteLine("Ready to write.");

var count = 0;
while (notEof)
{
    dbHelper.InsertData(line);
    notEof = importer.ReadLine(out line);
    count++;
    Console.Write($"\rLines: {count}");
}

Console.WriteLine();
Console.WriteLine("Parser End");

return 0;