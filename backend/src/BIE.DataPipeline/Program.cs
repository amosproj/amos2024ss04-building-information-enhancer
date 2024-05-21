// See https://aka.ms/new-console-template for more information

using BIE.DataPipeline;
using BIE.DataPipeline.Import;


Console.WriteLine("Parser Started");
DataSourceDescription description = YamlImporter.GetSourceDescription(args[0]);


CsvImporter csvImporter = new CsvImporter(description);

DBHelper.CreateDBConnection();
DBHelper.CreateTable(description);

//Console.WriteLine(csvImporter.GetHeaderString());
string line = "";
bool notEOF = csvImporter.ReadLine(out line);

while  (notEOF)
{
    DBHelper.InsertData(csvImporter.GetTableName(),csvImporter.GetHeaderString(),line);
    notEOF = csvImporter.ReadLine(out line);
}

Console.WriteLine("Parser End");
