using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Models;
using Xunit.Abstractions;

namespace JetBrainsRecentProjectCmdPal.Tests.Helper
{
    public class JetBrainsHelperTests
    {
        private readonly ITestOutputHelper _output;

        public JetBrainsHelperTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void LoadProductInfoFromDirectoryTest()
        {
            // Arrange
            var tempDir = "E:\\Jetbrains\\IntelliJ IDEA Community Edition";
            // Act
            ProductInfo result = JetBrainsHelper.LoadProductInfoFromDirectory(tempDir);
            _output.WriteLine($"ProductInfo: {result}");
        }

        [Fact]
        public void SearchRecentProjectsTest()
        {
            var result = JetBrainsHelper.SearchRecentProjects("E:\\AppData\\Jetbrains", true);
            var options = new JsonSerializerOptions { WriteIndented = true };
            foreach (var v in result)
            {
                // show json format pertty
        
                _output.WriteLine($"RecentProject: {JsonSerializer.Serialize(v, options)}");
                // _output.WriteLine($"RecentProject: {JsonSerializer.Serialize(v)}");
                _output.WriteLine("------------------------");
            }
        }
    }
}