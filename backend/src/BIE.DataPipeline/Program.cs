using BIE.DataPipeline;
using BIE.DataPipeline.Import;
using BIE.DataPipeline.Metadata;
using Mono.Options;

// Setup the command line options
var tableInsertBehaviour = InsertBehaviour.none;
var filename = HandleCliArguments();

var description = GetDataSourceDescription(filename);
if (description == null)
{
    return 1;
}

Console.WriteLine($@"Starting the Data Pipeline for {filename}:
type:       {description.source.type}
format:     {description.source.data_format}
location:   {description.source.location}
table name: {description.table_name}
");

// Check if the table insert behaviour is overwritten
if (tableInsertBehaviour != InsertBehaviour.none)
{
    description.options.if_table_exists = tableInsertBehaviour;
    Console.WriteLine($"Overwriting description: Using {tableInsertBehaviour} Behaviour for insertion.");
}

// Create the connection to the database
var dbHelper = new DbHelper();

// Exit when the connection is not possible
if (!dbHelper.CheckConnection())
{
    Console.WriteLine("Could not establish database connection, exiting...");
    return 1;
}
Console.WriteLine("Established the database connection.");

// End if Dataset can be skipped
if (DbHelper.CanSkip(description))
{
    return 1;
}

// Establish Connection to Metadata DB
var metadataDbHelper = new MetadataDbHelper();
if (!metadataDbHelper.CreateConnection())
{
    // maybe make optional?
    return 1;
}

Console.WriteLine("Starting the importer...");

// Import the data based on the specified data type
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
            dbHelper.SetInfo(description.table_name, "Location");
            break;

        default:
            Console.WriteLine($"Unknown or missing data format: {description.source.data_format}");
            return 1;
    }
}
catch (Exception e)
{
    Console.WriteLine("Error while setting up the importer.");
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
    Console.WriteLine("Inserting the data into the database...");

    // Read all lines
    var count = 0;
    while (notEof)
    {
        try
        {
            dbHelper.InsertData(line);
            notEof = importer.ReadLine(out line);
            count++;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error inserting line {count} into the database:");
            Console.WriteLine(e.Message);
            break;
            // Optionally, you can decide to break the loop or continue based on the type of error
        }
    }
    Console.WriteLine($"Finished inserting {count} lines of data.");

    if (dbHelper.CheckIfColumnExists(description))
    {

        dbHelper.CreateIndexes(description);
    }    
    Console.WriteLine("Updating the metadata...");
    metadataDbHelper.UpdateMetadata(description, count);
    Console.WriteLine("The metadata was updated.");
    Console.WriteLine("--------------------------------------------------------------");
}
catch (Exception e)
{
    Console.WriteLine("Error inserting the data into the database:");
    Console.WriteLine(e);
    return 1;
}

try
{
}
catch (Exception e)
{
    Console.WriteLine("Could not insert into Metadata DB");
    Console.WriteLine(e);
    throw;
}

return 0;

// -------------------------------------------------------
// FUNCTIONS
// -------------------------------------------------------

string HandleCliArguments()
{
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
    var remainingArgs = options.Parse(args);
    if (remainingArgs == null || remainingArgs.Count != 1)
    {
        Console.WriteLine("Could not determine filename: too many or not enough arguments.");
        Console.WriteLine("Usage: BIE.DataPipeline [options] filename.yaml");
        options.WriteOptionDescriptions(Console.Out);
        Environment.Exit(1);
        return "";
    }

    return remainingArgs[0];
}

DataSourceDescription? GetDataSourceDescription(string name)
{
    try
    {
        return YamlImporter.GetSourceDescription(name);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Could not get valid description from {name}");
        // Console.Error.WriteLine(e);
        return null;
    }
}