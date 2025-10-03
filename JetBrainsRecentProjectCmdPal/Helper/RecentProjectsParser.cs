using System;
using System.Collections;
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
    /// <summary>
    /// From XML file parse recent projects list
    /// </summary>
    /// <param name="xmlFilePath">recentProjects.xml file path</param>
    /// <returns>Recent projects list</returns>
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
            ExtensionHost.LogMessage($"Failed to parse recent projects from XML file: {ex.Message}, XML file path: {xmlFilePath}");
            return new List<RecentProject>();
        }
    }

    /// <summary>
    /// From XML string parse recent projects list
    /// </summary>
    /// <param name="xmlContent">XML content string</param>
    /// <returns>Recent projects list</returns>
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
            
            var serializer = CreateRecentProjectsSerializer();
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var additionalInfoOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "additionalInfo");

                if (additionalInfoOption?.Map?.Entries != null)
                {
                    projects = additionalInfoOption.Map.Entries
                        .Select(RecentProject.FromEntry)
                        .Where(p => !string.IsNullOrEmpty(p.Key))
                        .ToList();
                    return projects;
                }
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to parse recent projects from XML string: {ex.Message}, XML content: {xmlContent}");
        }

        return projects;
    }

    /// <summary>
    /// Gets the path of the last opened project
    /// </summary>
    /// <param name="xmlFilePath">Path to recentProjects.xml file</param>
    /// <returns>Path of the last opened project</returns>
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
            
            var serializer = CreateRecentProjectsSerializer();
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastOpenedOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastOpenedProject");
                
                return lastOpenedOption?.Value;
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last opened project: {ex.Message}, XML file path: {xmlFilePath}");
        }

        return null;
    }

    /// <summary>
    /// Gets the last project location
    /// </summary>
    /// <param name="xmlFilePath">Path to recentProjects.xml file</param>
    /// <returns>Last project location</returns>
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
            
            var serializer = CreateRecentProjectsSerializer();
            if (serializer.Deserialize(xmlReader) is RecentProjectsApplication app)
            {
                var lastLocationOption = app.Component.Options
                    .FirstOrDefault(o => o.Name == "lastProjectLocation");
                
                return lastLocationOption?.Value;
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last project location: {ex.Message}, XML file path: {xmlFilePath}");
        }

        return null;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsApplication))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsManager))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsList))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsListOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsMap))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsEntry))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectsEntryValue))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectMetaInfo))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RecentProjectMetaInfoOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsApplication))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsManager))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsList))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsListOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsMap))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsEntry))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectsEntryValue))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectMetaInfo))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields, typeof(RecentProjectMetaInfoOption))]
    private static XmlSerializer CreateRecentProjectsSerializer()
    {
        return new XmlSerializer(typeof(RecentProjectsApplication));
    }
}