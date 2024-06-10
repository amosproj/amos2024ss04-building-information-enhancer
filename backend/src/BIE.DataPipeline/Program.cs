using BIE.DataPipeline;
using BIE.DataPipeline.Import;
using Mono.Options;

// setup command line options.
var tableInsertBehaviour = InsertBehaviour.none;
var options = new OptionSet
{
    {
        "b=|behavior=", @"Behaviour when inserting into a database. Options:
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
                    Console.WriteLine($"{behaviour} is not a recognized behaviour.\navailable options: replace, skip, ignore");
                    Environment.Exit(1);
                    break;
            }
        }
    }
};

// parse command line options
var rest = options.Parse(args);
if (rest == null || rest.Count != 1)
{
    Console.WriteLine("Could not determine filename: too many or not enough arguments.");
    Console.WriteLine("Usage: BIE.DataPipeline [options] filename.yaml");
    options.WriteOptionDescriptions(Console.Out);
    Environment.Exit(1);
}

Console.WriteLine("Parser Started");

DataSourceDescription description;
try
{
     description = YamlImporter.GetSourceDescription(rest[0]);
}
catch (Exception e)
{
    Console.WriteLine(e);
    return 1;
}

if (tableInsertBehaviour != InsertBehaviour.none)
{
    description.options.if_table_exists = tableInsertBehaviour;
    Console.WriteLine($"Overwriting Yaml: Using {tableInsertBehaviour} Behaviour for insertion.");
}

var dbHelper = new DBHelper();
IImporter importer;

try
{
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
            Console.WriteLine($"Unknown data format: {description.source.data_format}");
            return 1;
    }
}
catch (Exception e)
{
    Console.WriteLine("Error While setting up Importer.");
    Console.WriteLine(e);
    return 1;
}

if (!dbHelper.CreateTable(description))
{
    return 0;
}

try
{
    var line = "";
    var notEof = importer.ReadLine(out line);

    Console.WriteLine("Inserting into Database");

    var count = 0;
    while (notEof)
    {
        dbHelper.InsertData(line);
        notEof = importer.ReadLine(out line);
        count++;
        Console.Write($"\rLines: {count}");
    }

    Console.WriteLine();
    Console.WriteLine("Finished Inserting");
}
catch (Exception e)
{
    Console.WriteLine("Error inserting into Database:");
    Console.WriteLine(e);
    return 1;
}

return 0;