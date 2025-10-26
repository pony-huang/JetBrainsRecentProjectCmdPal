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
        public void LoadProductInfoFromDirectory_WithValidJson_ReturnsProductInfo()
        {
            // Arrange
            var tempDir = "E:\\Jetbrains\\IntelliJ IDEA Community Edition";
            // Act
            ProductInfo result = JetBrainsHelper.LoadProductInfoFromDirectory(tempDir);
            _output.WriteLine($"ProductInfo: {result}");
        }

        [Fact]
        public void Test1()
        {
            var result = JetBrainsHelper.SearchRecentProjects("E:\\AppData\\Jetbrains", true);
            foreach (var v in result)
            {
                _output.WriteLine($"{v.Name}");
                _output.WriteLine($"{v.Key}");
            }
        }
    }
}