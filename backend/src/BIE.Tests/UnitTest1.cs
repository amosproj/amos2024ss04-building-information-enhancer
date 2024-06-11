namespace BIE.Tests;
using BIE.DataPipeline;
using BIE.DataPipeline.Import;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Setup");
    }

    [Test]
    public void TestYamlImporter()
    {
        Console.WriteLine("Test Yaml Importer");
        DataSourceDescription description = YamlImporter.GetSourceDescription(@".\yaml\test.yaml");
        Assert.IsNotNull(description);
        char result = description.delimiter;
        Assert.That(result, Is.EqualTo(';'), "Delimiter should be ;");
    }
}