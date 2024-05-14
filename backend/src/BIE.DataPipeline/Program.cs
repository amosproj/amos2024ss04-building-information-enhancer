// See https://aka.ms/new-console-template for more information
using BEI.DataPipeline.Import;

Console.WriteLine("Hello, World!");

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