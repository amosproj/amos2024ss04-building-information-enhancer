using BIE.DataPipeline;
using BIE.DataPipeline.Import;
using Mono.Options;
using System.Xml;

string xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
        <CityModel xmlns:tex=""http://www.opengis.net/citygml/texturedsurface/1.0"" xmlns:sch=""http://www.ascc.net/xml/schematron"" xmlns:veg=""http://www.opengis.net/citygml/vegetation/1.0"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:tran=""http://www.opengis.net/citygml/transportation/1.0"" xmlns:grp=""http://www.opengis.net/citygml/cityobjectgroup/1.0"" xmlns:base=""http://www.citygml.org/citygml/profiles/base/1.0"" xmlns:bldg=""http://www.opengis.net/citygml/building/1.0"" xmlns:wtr=""http://www.opengis.net/citygml/waterbody/1.0"" xmlns:dem=""http://www.opengis.net/citygml/relief/1.0"" xmlns:gen=""http://www.opengis.net/citygml/generics/1.0"" xmlns:app=""http://www.opengis.net/citygml/appearance/1.0"" xmlns:frn=""http://www.opengis.net/citygml/cityfurniture/1.0"" xmlns:smil20=""http://www.w3.org/2001/SMIL20/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:smil20lang=""http://www.w3.org/2001/SMIL20/Language"" xmlns:xAL=""urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"" xmlns=""http://www.opengis.net/citygml/1.0"" xmlns:luse=""http://www.opengis.net/citygml/landuse/1.0"" xsi:schemaLocation=""http://www.opengis.net/citygml/1.0 http://repository.gdi-de.org/schemas/adv/citygml/1.0/cityGMLBaseLoD2.xsd http://www.opengis.net/citygml/building/1.0 http://repository.gdi-de.org/schemas/adv/citygml/building/1.0/buildingLoD2.xsd http://www.opengis.net/citygml/appearance/1.0 http://repository.gdi-de.org/schemas/adv/citygml/appearance/1.0/appearanceLoD2.xsd http://www.opengis.net/citygml/generics/1.0 http://repository.gdi-de.org/schemas/adv/citygml/generics/1.0/genericsLoD2.xsd"">
        <gml:name>LoD2_32_652_5496_2_BY</gml:name>
        <gml:boundedBy>
        <gml:Envelope srsName=""urn:adv:crs:ETRS89_UTM32*DE_DHHN2016_NH"" srsDimension=""3"">
        <gml:lowerCorner>652011.2 5495982.502 301.5</gml:lowerCorner>
        <gml:upperCorner>654012.1 5498005.797 357.447</gml:upperCorner>
        </gml:Envelope>
        </gml:boundedBy>
        <cityObjectMember xmlns:bldg='http://www.opengis.net/citygml/building/2.0' xmlns:gml='http://www.opengis.net/gml'>
            <bldg:Building gml:id='DEBY_LOD2_107309624'>
                <creationDate>2021-11-24</creationDate>
                <externalReference>
                    <informationSystem>http://repository.gdi-de.org/schemas/adv/citygml/fdv/art.htm#_9100</informationSystem>
                    <externalObject>
                        <name>DEBYvAAAAABsfzSK</name>
                    </externalObject>
                </externalReference>
            </bldg:Building>
        </cityObjectMember>
        </CityModel>
";



string filePath = @"C:\Users\nicol\downloads\652_5496.gml";

CityGmlImporter reader = new CityGmlImporter(filePath);
List<Building> buildings = reader.ReadBuildings();

return 0;

foreach (Building building in buildings)
{
    Console.WriteLine($"Building ID: {building.Id}");
    Console.WriteLine($"Building Name: {building.Name}");
    Console.WriteLine("Positions:");
    foreach (string pos in building.Positions)
    {
        Console.WriteLine(pos);
    }
    Console.WriteLine();
}

return 0;

// setup command line options.
var tableInsertBehaviour = InsertBehaviour.none;

var filename = HandleCliArguments();

Console.WriteLine("Starting datapipeline");

var description = GetDataSourceDescription(filename);
if (description == null)
{
    return 1;
}

Console.WriteLine($@"Running with {filename}:
type:       {description.source.type}
format:     {description.source.data_format}
location:   {description.source.location}
table name: {description.table_name}

");

if (tableInsertBehaviour != InsertBehaviour.none)
{
    description.options.if_table_exists = tableInsertBehaviour;
    Console.WriteLine($"Overwriting Description: Using {tableInsertBehaviour} Behaviour for insertion.");
}

// create connection to database;
var dbHelper = new DbHelper();

// End if Connection not possible.
if (!dbHelper.CheckConnection())
{
    Console.WriteLine("Could not establish Database Connection, exiting...");
    return 1;
}
Console.WriteLine("Established Database Connection.");

// End if Dataset can be skipped
if (dbHelper.CanSkip(description))
{
    return 1;
}
Console.WriteLine("Starting Importer");

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
    if (description.source.data_format == "SHAPE")
    {
        Console.WriteLine("Creating Indexes");
        dbHelper.CreateIndexes(description);
        Console.WriteLine("Indexes Created");
    }

}
catch (Exception e)
{
    Console.WriteLine("Error inserting into Database:");
    Console.WriteLine(e);
    return 1;
}

return 0;

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