namespace BIE.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Setup");
    }

    [Test]
    public void Test1()
    {
        Console.WriteLine("Test1");
        Assert.Pass();
    }
}