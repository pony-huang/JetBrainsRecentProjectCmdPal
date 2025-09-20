using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace JetBrainsRecentProjectCmdPal.Helper;

/// <summary>
/// 用于解析JetBrains IntelliJ IDEA的recentProjects.xml文件的解析器
/// </summary>
public static class RecentProjectsParser
{
    /// <summary>
    /// 从XML文件解析最近项目列表
    /// </summary>
    /// <param name="xmlFilePath">recentProjects.xml文件路径</param>
    /// <returns>最近项目列表</returns>
    public static List<RecentProject> ParseFromFile(string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(xmlFilePath);
        
        if (!File.Exists(xmlFilePath))
            return new List<RecentProject>();

        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            return ParseFromXmlString(xmlContent);
        }
        catch (Exception ex)
        {
            // 记录错误或处理异常
            Console.WriteLine($"解析XML文件时出错: {ex.Message}");
            return new List<RecentProject>();
        }
    }

    /// <summary>
    /// 从XML字符串解析最近项目列表
    /// </summary>
    /// <param name="xmlContent">XML内容字符串</param>
    /// <returns>最近项目列表</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through XML attributes")]
    public static List<RecentProject> ParseFromXmlString(string xmlContent)
    {
        ArgumentNullException.ThrowIfNull(xmlContent);
        
        var projects = new List<RecentProject>();

        try
        {
            var serializer = new XmlSerializer(typeof(RecentProjectsApplication));
            
            // 创建安全的 XmlReader 设置
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                // 查找additionalInfo选项
                var additionalInfoOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "additionalInfo");

                if (additionalInfoOption?.Map != null)
                {
                    // 转换每个项目条目为RecentProject
                    projects = additionalInfoOption.Map.Entries
                        .Select(RecentProject.FromEntry)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            // 记录错误或处理异常
            Console.WriteLine($"解析XML内容时出错: {ex.Message}");
        }

        return projects;
    }

    /// <summary>
    /// 获取最后打开的项目路径
    /// </summary>
    /// <param name="xmlFilePath">recentProjects.xml文件路径</param>
    /// <returns>最后打开的项目路径</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through XML attributes")]
    public static string? GetLastOpenedProject(string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(xmlFilePath);
        
        if (!File.Exists(xmlFilePath))
            return null;

        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            var serializer = new XmlSerializer(typeof(RecentProjectsApplication));
            
            // 创建安全的 XmlReader 设置
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastOpenedOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastOpenedProject");
                
                return lastOpenedOption?.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取最后打开项目时出错: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 获取最后项目位置
    /// </summary>
    /// <param name="xmlFilePath">recentProjects.xml文件路径</param>
    /// <returns>最后项目位置</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through XML attributes")]
    public static string? GetLastProjectLocation(string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(xmlFilePath);
        
        if (!File.Exists(xmlFilePath))
            return null;

        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            var serializer = new XmlSerializer(typeof(RecentProjectsApplication));
            
            // 创建安全的 XmlReader 设置
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastLocationOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastProjectLocation");
                
                return lastLocationOption?.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取最后项目位置时出错: {ex.Message}");
        }

        return null;
    }
}