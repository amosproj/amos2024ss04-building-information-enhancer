// See https://aka.ms/new-console-template for more information

using BIE.DataPipeline;
using BIE.DataPipeline.Import;


Console.WriteLine("Hello, World!");
/*
var description = YamlImporter.GetSourceDescription(args[0]);

DBHelper.CreateDBConnection();
DBHelper.CreateTable(description);

Console.WriteLine(description.source.location);
Console.WriteLine(description.table_name);
Console.WriteLine(description.table_cols[0].type);

Console.ReadKey();
*/

CsvImporter csvImporter = new CsvImporter(@"C:\Users\nicol\Desktop\uni\M3\AMOS\Datasources\Ladesaeulenregister.csv");
//CsvImporter csvImporter = new CsvImporter(@"https://data.bundesnetzagentur.de/Bundesnetzagentur/SharedDocs/Downloads/DE/Sachgebiete/Energie/Unternehmen_Institutionen/E_Mobilitaet/Ladesaeulenregister.csv");
Console.WriteLine(csvImporter.GetHeaderString());
string line = "";
bool notEOF = csvImporter.ReadLine(out line);
int row = 0;
while  (notEOF && row < 10)
{
    Console.WriteLine(line);
    row++;
    notEOF = csvImporter.ReadLine(out line);
}