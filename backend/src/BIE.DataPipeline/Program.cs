// See https://aka.ms/new-console-template for more information

using BIE.DataPipeline;
using BIE.DataPipeline.Import;


Console.WriteLine("Hello, World!");

var description = YamlImporter.GetSourceDescription(args[0]);

DBHelper.CreateDBConnection();
DBHelper.CreateTable(description);

Console.WriteLine(description.source.location);
Console.WriteLine(description.table_name);
Console.WriteLine(description.table_cols[0].type);

Console.ReadKey();
