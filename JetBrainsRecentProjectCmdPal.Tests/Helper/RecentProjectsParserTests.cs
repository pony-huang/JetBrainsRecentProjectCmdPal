using System.Globalization;
using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Helper;
using Xunit.Abstractions;

namespace JetBrainsRecentProjectCmdPal.Tests.Helper;

public class RecentProjectsParserTests
{
    private readonly ITestOutputHelper _output;

    public RecentProjectsParserTests(ITestOutputHelper output)
    {
        _output = output;
    }


    #region Integration Tests

    [Fact]
    public void IntegrationTestParseFromRealJetBrainsPath()
    {
        // Arrange - 使用常见的 JetBrains 配置路径
        // var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // var jetbrainsPath = Path.Combine(roamingPath, "JetBrains");
        var jetbrainsPath = "E:\\AppData\\Jetbrains";
        _output.WriteLine($"Path: {jetbrainsPath}");
        // Act
        var projects = JetBrainsHelper.SearchRecentProjects(jetbrainsPath, true);
        // Assert & Output
        _output.WriteLine($"All project: {projects.Count}");
        foreach (var project in projects)
        {
            ShowProjectDetails(project);
        }
    }

    [Fact]
    public void IntegrationTestGetInstalledProducts()
    {
        // var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        // var jetbrainsPath = Path.Combine(programFiles, "JetBrains");
        // var products = JetBrainsService.GetInstalledProducts(jetbrainsPath);
        var products = JetBrainsHelper.GetInstalledProducts("E:\\Jetbrains");
        _output.WriteLine($"All {products.Count} ");
        var options = new JsonSerializerOptions { WriteIndented = true };
        foreach (var product in products)
        {
            // show json format
            _output.WriteLine($"LaunchInfo: {JsonSerializer.Serialize(product, options)}");
            _output.WriteLine("------------------------");
        }
    }

    [Fact]
    public void IntegrationTestGetInstalledProductsContains()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var products = JetBrainsHelper.GetInstalledProducts("E:\\Jetbrains");
        var result = JetBrainsHelper.SearchRecentProjects("E:\\AppData\\Jetbrains", true);

        var installedProductsDict = products
            .GroupBy(p => p.ProductCode)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.BuildNumber)
                    .ToDictionary(p => p.BuildNumber)
            );
        foreach (var var in result)
        {
            var currentProducts = installedProductsDict[var.ProductionCode];
            foreach (var key in currentProducts.Keys)
            {
                if (var.Build.Contains(key))
                {
                    var product = currentProducts.GetValueOrDefault(key);
                    _output.WriteLine($"匹配: {product.Name} {product.Version}");
                }
            }
        }
    }

    #endregion

    #region Helper Methods

    private void ShowProjectDetails(RecentProject project)
    {
        _output.WriteLine($"项目名称: {project.Name}");
        _output.WriteLine($"项目路径: {project.Key}");
        _output.WriteLine($"窗口标题: {project.FrameTitle}");
        _output.WriteLine($"是否打开: {project.IsOpened}");
        _output.WriteLine($"工作区ID: {project.ProjectWorkspaceId}");
        _output.WriteLine(
            $"激活时间: {(project.ActivationTimestamp > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(project.ActivationTimestamp).ToString(CultureInfo.InvariantCulture) : "未知")}");
        _output.WriteLine($"构建版本: {project.Build}");
        _output.WriteLine($"产品代码: {project.ProductionCode}");
        _output.WriteLine("---");
    }

    #endregion
}