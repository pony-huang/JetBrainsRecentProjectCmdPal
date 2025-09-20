using System.Globalization;
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

    #region RecentProjectsParser Tests

    [Fact]
    public void ParseFromFileWithNonExistentFileShouldExecuteWithoutException()
    {
        // Arrange
        var path = "E:\\AppData\\Jetbrains\\ideaIU\\config\\options\\recentProjects.xml";

        // Act
        var result = RecentProjectsParser.ParseFromFile(path);

        // Assert & Print Results
        _output.WriteLine($"解析结果: 找到 {result.Count} 个项目");

        if (result.Count > 0)
        {
            _output.WriteLine("项目详情:");
            for (int i = 0; i < result.Count; i++)
            {
                var project = result[i];
                show(i, project);
            }
        }
        else
        {
            _output.WriteLine("没有找到任何项目或文件不存在");
        }
    }


    [Fact]
    public void Test1()
    {
        // Arrange
        var path = "E:\\AppData\\Jetbrains";
        var searchRecentProjectXmlByCustom = Query.SearchRecentProjectXml(path);
        foreach (var se in searchRecentProjectXmlByCustom)
        {
            var result = RecentProjectsParser.ParseFromFile(se);
            _output.WriteLine($"解析结果: 找到 {result.Count} 个项目");
            for (int i = 0; i < result.Count; i++)
            {
                var project = result[i];
                show(i, project);
            }
        }
    }
    
    [Fact]
    public void Test2()
    {
        // Arrange
        var path = "IU";
        var searchRecentProjectXmlByCustom = Query.GetShellBySettings(path);
        _output.WriteLine($"解析结果: 找到 {searchRecentProjectXmlByCustom}");
    }

    void show(int i, RecentProject project)
    {
        _output.WriteLine($"  [{i + 1}] 项目名称: {project.Name}");
        _output.WriteLine($"      项目路径: {project.Key}");
        _output.WriteLine($"      窗口标题: {project.FrameTitle}");
        _output.WriteLine($"      是否打开: {project.IsOpened}");
        _output.WriteLine($"      工作区ID: {project.ProjectWorkspaceId}");
        _output.WriteLine(
            $"      激活时间: {(project.ActivationTimestamp > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(project.ActivationTimestamp).ToString(CultureInfo.InvariantCulture) : "未知")}");
        _output.WriteLine($"      构建版本: {project.Build}");
        _output.WriteLine($"      产品代码: {project.ProductionCode}");
        _output.WriteLine("");
    }

    #endregion
}