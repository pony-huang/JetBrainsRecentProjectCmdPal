using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Helper;
using Xunit.Abstractions;

namespace JetBrainsRecentProjectCmdPal.Tests.Helper;

public class UnitTest
{
    private readonly ITestOutputHelper _output;

    public UnitTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void Test1()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        // var searchProjects = Search.SearchProjects();
        var products = JetBrainsHelper.GetInstalledProducts("E:\\Jetbrains");
        var result = JetBrainsHelper.SearchRecentProjects("E:\\AppData\\Jetbrains", true);
        var searchProjects = JetBrainsHelper.MergerProductProject(result, products);
        foreach (var project in searchProjects)
        {
            _output.WriteLine($"json: {JsonSerializer.Serialize(project, options)}");
        }
    }

}