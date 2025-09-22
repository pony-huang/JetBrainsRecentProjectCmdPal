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

    #region ParseFromFile Tests

    [Fact]
    public void ParseFromFile_WithValidFile_ShouldParseSuccessfully()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var sampleXml = CreateSampleRecentProjectsXml();
        File.WriteAllText(tempFile, sampleXml);

        try
        {
            // Act
            var result = RecentProjectsParser.ParseFromFile(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _output.WriteLine($"测试通过: 成功解析文件，找到 {result.Count} 个项目");

            // 验证第一个项目的详细信息
            if (result.Count > 0)
            {
                var firstProject = result[0];
                Assert.NotNull(firstProject.Key);
                Assert.NotEmpty(firstProject.Key);
                _output.WriteLine($"第一个项目路径: {firstProject.Key}");
                _output.WriteLine($"第一个项目名称: {firstProject.Name}");
            }
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region ParseFromXmlString Tests

    [Fact]
    public void ParseFromXmlString_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RecentProjectsParser.ParseFromXmlString(null!));
        _output.WriteLine("测试通过: 空XML内容参数抛出 ArgumentNullException");
    }

    [Fact]
    public void ParseFromXmlString_WithEmptyContent_ShouldReturnEmptyList()
    {
        // Act
        var result = RecentProjectsParser.ParseFromXmlString("");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _output.WriteLine("测试通过: 空XML内容返回空列表");
    }

    [Fact]
    public void ParseFromXmlString_WithInvalidXml_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidXml = "<invalid>xml content</invalid>";

        // Act
        var result = RecentProjectsParser.ParseFromXmlString(invalidXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _output.WriteLine("测试通过: 无效XML内容返回空列表");
    }

    [Fact]
    public void ParseFromXmlString_WithValidXml_ShouldParseCorrectly()
    {
        // Arrange
        var validXml = CreateSampleRecentProjectsXml();

        // Act
        var result = RecentProjectsParser.ParseFromXmlString(validXml);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _output.WriteLine($"测试通过: 成功解析XML字符串，找到 {result.Count} 个项目");

        // 验证项目详细信息
        foreach (var project in result)
        {
            Assert.NotNull(project.Key);
            Assert.NotEmpty(project.Key);
            ShowProjectDetails(project);
        }
    }

    #endregion

    #region GetLastOpenedProject Tests

    [Fact]
    public void GetLastOpenedProject_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Path\\recentProjects.xml";

        // Act
        var result = RecentProjectsParser.GetLastOpenedProject(nonExistentPath);

        // Assert
        Assert.Null(result);
        _output.WriteLine("测试通过: 不存在的文件返回 null");
    }

    [Fact]
    public void GetLastOpenedProject_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RecentProjectsParser.GetLastOpenedProject(null!));
        _output.WriteLine("测试通过: 空路径参数抛出 ArgumentNullException");
    }

    [Fact]
    public void GetLastOpenedProject_WithValidFile_ShouldReturnLastProject()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var sampleXml = CreateSampleRecentProjectsXmlWithLastOpened();
        File.WriteAllText(tempFile, sampleXml);

        try
        {
            // Act
            var result = RecentProjectsParser.GetLastOpenedProject(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _output.WriteLine($"测试通过: 成功获取最后打开的项目: {result}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region GetLastProjectLocation Tests

    [Fact]
    public void GetLastProjectLocation_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Path\\recentProjects.xml";

        // Act
        var result = RecentProjectsParser.GetLastProjectLocation(nonExistentPath);

        // Assert
        Assert.Null(result);
        _output.WriteLine("测试通过: 不存在的文件返回 null");
    }

    [Fact]
    public void GetLastProjectLocation_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RecentProjectsParser.GetLastProjectLocation(null!));
        _output.WriteLine("测试通过: 空路径参数抛出 ArgumentNullException");
    }

    [Fact]
    public void GetLastProjectLocation_WithValidFile_ShouldReturnLastLocation()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var sampleXml = CreateSampleRecentProjectsXmlWithLastLocation();
        File.WriteAllText(tempFile, sampleXml);

        try
        {
            // Act
            var result = RecentProjectsParser.GetLastProjectLocation(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _output.WriteLine($"测试通过: 成功获取最后项目位置: {result}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Integration Tests (保留原有的集成测试)

    [Fact]
    public void IntegrationTest_ParseFromRealJetBrainsPath()
    {
        // Arrange - 使用常见的 JetBrains 配置路径
        var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var jetbrainsPath = Path.Combine(roamingPath, "JetBrains");
        _output.WriteLine($"尝试从路径解析项目: {jetbrainsPath}");
        // Act
        var projects = JetBrainsService.SearchRecentProjects(jetbrainsPath, true);
        // Assert & Output
        _output.WriteLine($"集成测试结果: 找到 {projects.Count} 个项目");
        foreach (var project in projects.Take(5))
        {
            ShowProjectDetails(project);
        }
    }

    [Fact]
    public void IntegrationTest_GetInstalledProducts()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var jetbrainsPath = Path.Combine(programFiles, "JetBrains");
        // Act
        var jetBrainsService = new JetBrainsService();
        var products = jetBrainsService.GetInstalledProducts(jetbrainsPath);
        // Assert & Output
        _output.WriteLine($"集成测试结果: 找到 {products.Count} 个已安装产品");
        foreach (var product in products)
        {
            _output.WriteLine($"产品名称: {product.Name}");
            _output.WriteLine($"产品代码: {product.ProductCode}");
            _output.WriteLine("---");
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

    private static string CreateSampleRecentProjectsXml()
    {
        return """
               <?xml version="1.0" encoding="UTF-8"?>
               <application>
                 <component name="RecentProjectsManager">
                   <option name="additionalInfo">
                     <map>
                       <entry key="C:\Projects\TestProject1">
                         <value>
                           <RecentProjectMetaInfo>
                             <option name="displayName" value="TestProject1" />
                             <option name="frameTitle" value="TestProject1 - IntelliJ IDEA" />
                             <option name="productionCode" value="IU" />
                             <option name="activationTimestamp" value="1640995200000" />
                             <option name="projectOpenTimestamp" value="1640995200000" />
                             <option name="build" value="IU-223.8617.56" />
                             <option name="projectWorkspaceId" value="test-workspace-1" />
                             <option name="opened" value="false" />
                           </RecentProjectMetaInfo>
                         </value>
                       </entry>
                       <entry key="C:\Projects\TestProject2">
                         <value>
                           <RecentProjectMetaInfo>
                             <option name="displayName" value="TestProject2" />
                             <option name="frameTitle" value="TestProject2 - IntelliJ IDEA" />
                             <option name="productionCode" value="IU" />
                             <option name="activationTimestamp" value="1640995300000" />
                             <option name="projectOpenTimestamp" value="1640995300000" />
                             <option name="build" value="IU-223.8617.56" />
                             <option name="projectWorkspaceId" value="test-workspace-2" />
                             <option name="opened" value="true" />
                           </RecentProjectMetaInfo>
                         </value>
                       </entry>
                     </map>
                   </option>
                 </component>
               </application>
               """;
    }

    private static string CreateSampleRecentProjectsXmlWithLastOpened()
    {
        return """
               <?xml version="1.0" encoding="UTF-8"?>
               <application>
                 <component name="RecentProjectsManager">
                   <option name="lastOpenedProject" value="C:\Projects\LastOpenedProject" />
                   <option name="additionalInfo">
                     <map>
                       <entry key="C:\Projects\TestProject1">
                         <value>
                           <RecentProjectMetaInfo>
                             <option name="displayName" value="TestProject1" />
                           </RecentProjectMetaInfo>
                         </value>
                       </entry>
                     </map>
                   </option>
                 </component>
               </application>
               """;
    }

    private static string CreateSampleRecentProjectsXmlWithLastLocation()
    {
        return """
               <?xml version="1.0" encoding="UTF-8"?>
               <application>
                 <component name="RecentProjectsManager">
                   <option name="lastProjectLocation" value="C:\Projects\LastProjectLocation" />
                   <option name="additionalInfo">
                     <map>
                       <entry key="C:\Projects\TestProject1">
                         <value>
                           <RecentProjectMetaInfo>
                             <option name="displayName" value="TestProject1" />
                           </RecentProjectMetaInfo>
                         </value>
                       </entry>
                     </map>
                   </option>
                 </component>
               </application>
               """;
    }

    #endregion
}