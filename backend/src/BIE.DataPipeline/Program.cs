// See https://aka.ms/new-console-template for more information

using BIE.DataPipeline;
using BIE.DataPipeline.Import;


Console.WriteLine("Parser Started");
DataSourceDescription description = YamlImporter.GetSourceDescription(args[0]);

//CsvImporter csvImporter = new CsvImporter(description);
ShapeImporter shapeImporter = new ShapeImporter(description);

var dbHelper = new DBHelper();
//dbHelper.SetInfo(csvImporter.GetTableName(), csvImporter.GetHeaderString());
//dbHelper.CreateTable(description);

dbHelper.SetInfo("SpatialData", "Location ");
dbHelper.CreateTable(description);


//Console.WriteLine(csvImporter.GetHeaderString());
string line = "";
//bool notEOF = csvImporter.ReadLine(out line);
bool notEOF = shapeImporter.ReadLine(out line);

Console.WriteLine("Ready to write.");

var count = 0;
//while  (notEOF)
//{
//    dbHelper.InsertData(line);
//    notEOF = csvImporter.ReadLine(out line);
//    count++;
//    Console.Write($"\rLines: {count}");
//}

while (notEOF)
{
    line = line.Remove(0, 1);
    dbHelper.InsertData(line);
    notEOF = shapeImporter.ReadLine(out line);
    count++;
    Console.Write($"\rLines: {count}");
}

Console.WriteLine();
Console.WriteLine("Parser End");
