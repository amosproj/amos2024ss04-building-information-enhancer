// See https://aka.ms/new-console-template for more information

using BIE.DataPipeline;
using BIE.DataPipeline.Import;


Console.WriteLine("Parser Started");
DataSourceDescription description = YamlImporter.GetSourceDescription(args[0]);

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
string line = "";
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