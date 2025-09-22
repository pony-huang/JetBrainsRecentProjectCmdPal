using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

/// <summary>
/// Parser for JetBrains IntelliJ IDEA's recentProjects.xml file
/// </summary>
public static class RecentProjectsParser
{
    // 缓存XmlSerializer实例以提高性能
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private static readonly XmlSerializer Serializer = new(typeof(RecentProjectsApplication));

    /// <summary>
    /// From XML file parse recent projects list
    /// </summary>
    /// <param name="xmlFilePath">recentProjects.xml file path</param>
    /// <returns>Recent projects list</returns>
    [RequiresUnreferencedCode("XML serialization may require types that cannot be statically analyzed")]
    [RequiresDynamicCode("XML serialization may require dynamic code generation")]
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
            ExtensionHost.LogMessage($"解析XML文件时出错: {ex.Message}");
            return new List<RecentProject>();
        }
    }

    /// <summary>
    /// From XML string parse recent projects list
    /// </summary>
    /// <param name="xmlContent">XML content string</param>
    /// <returns>Recent projects list</returns>
    [RequiresUnreferencedCode("XML serialization may require types that cannot be statically analyzed")]
    [RequiresDynamicCode("XML serialization may require dynamic code generation")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through DynamicallyAccessedMembers attributes")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling", Justification = "XML serialization is required for parsing JetBrains configuration files")]
    public static List<RecentProject> ParseFromXmlString(string xmlContent)
    {
        ArgumentNullException.ThrowIfNull(xmlContent);
        
        var projects = new List<RecentProject>();

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (Serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var additionalInfoOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "additionalInfo");

                if (additionalInfoOption?.Map?.Entries != null)
                {
                    projects = additionalInfoOption.Map.Entries
                        .Select(RecentProject.FromEntry)
                        .Where(p => !string.IsNullOrEmpty(p.Key))
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"解析XML内容时出错: {ex.Message}");
        }

        return projects;
    }

    /// <summary>
    /// Gets the path of the last opened project
    /// </summary>
    /// <param name="xmlFilePath">Path to recentProjects.xml file</param>
    /// <returns>Path of the last opened project</returns>
    [RequiresUnreferencedCode("XML serialization may require types that cannot be statically analyzed")]
    [RequiresDynamicCode("XML serialization may require dynamic code generation")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through DynamicallyAccessedMembers attributes")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling", Justification = "XML serialization is required for parsing JetBrains configuration files")]
    public static string? GetLastOpenedProject(string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(xmlFilePath);
        
        if (!File.Exists(xmlFilePath))
            return null;

        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (Serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastOpenedOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastOpenedProject");
                
                return lastOpenedOption?.Value;
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"获取最后打开项目时出错: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Gets the last project location
    /// </summary>
    /// <param name="xmlFilePath">Path to recentProjects.xml file</param>
    /// <returns>Last project location</returns>
    [RequiresUnreferencedCode("XML serialization may require types that cannot be statically analyzed")]
    [RequiresDynamicCode("XML serialization may require dynamic code generation")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "XML serialization types are preserved through DynamicallyAccessedMembers attributes")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling", Justification = "XML serialization is required for parsing JetBrains configuration files")]
    public static string? GetLastProjectLocation(string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(xmlFilePath);
        
        if (!File.Exists(xmlFilePath))
            return null;

        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            if (Serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastLocationOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastProjectLocation");
                
                return lastLocationOption?.Value;
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last project location: {ex.Message}");
        }

        return null;
    }
}