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
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer dependencies are preserved via DynamicDependency in CreateRecentProjectsSerializer.")]
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer dependencies are preserved via DynamicDependency in CreateRecentProjectsSerializer.")]
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer dependencies are preserved via DynamicDependency in CreateRecentProjectsSerializer.")]
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer dependencies are preserved via DynamicDependency in CreateRecentProjectsSerializer.")]
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer dependencies are preserved via DynamicDependency on RecentProjects* model types.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsApplication))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsManager))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsList))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsListOption))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsMap))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsEntry))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectsEntryValue))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectMetaInfo))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(RecentProjectMetaInfoOption))]
    private static XmlSerializer CreateRecentProjectsSerializer()
    {
        return new XmlSerializer(typeof(RecentProjectsApplication));
    }
}
