// See https://aka.ms/new-console-template for more information
using BEI.DataPipeline.Import;

Console.WriteLine("Hello, World!");

CsvImporter csvImporter = new CsvImporter(@"C:\Users\nicol\Desktop\uni\M3\AMOS\Datasources\Ladesaeulenregister.csv");
//CsvImporter csvImporter = new CsvImporter(@"https://data.bundesnetzagentur.de/Bundesnetzagentur/SharedDocs/Downloads/DE/Sachgebiete/Energie/Unternehmen_Institutionen/E_Mobilitaet/Ladesaeulenregister.csv");
Dictionary<string, object> line = csvImporter.ReadLine();
int row = 0;
while  (line != null)
{
    object betreiber;
    bool succes = line.TryGetValue("Betreiber", out betreiber);
    Console.WriteLine(row + " " + betreiber);
    row++;
    line = csvImporter.ReadLine();
}