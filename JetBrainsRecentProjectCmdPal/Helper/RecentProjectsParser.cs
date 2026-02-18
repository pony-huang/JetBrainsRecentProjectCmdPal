using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

/// <summary>
/// Parser for JetBrains IntelliJ IDEA's recentProjects.xml file
/// </summary>
public static class RecentProjectsParser
{
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
            
            var doc = XDocument.Load(xmlReader);
            var component = doc.Element("application")
                             ?.Elements("component")
                             .FirstOrDefault(e => e.Attribute("name")?.Value == "RecentProjectsManager");

            if (component == null) return projects;

            var additionalInfoOption = component.Elements("option")
                .FirstOrDefault(e => e.Attribute("name")?.Value == "additionalInfo");

            var map = additionalInfoOption?.Element("map");
            if (map == null) return projects;

            foreach (var entry in map.Elements("entry"))
            {
                var key = entry.Attribute("key")?.Value;
                if (string.IsNullOrEmpty(key)) continue;

                var project = new RecentProject { Key = key };
                var metaInfo = entry.Element("value")?.Element("RecentProjectMetaInfo");
                
                if (metaInfo != null)
                {
                    foreach (var option in metaInfo.Elements("option"))
                    {
                        var name = option.Attribute("name")?.Value;
                        var value = option.Attribute("value")?.Value;

                        if (string.IsNullOrEmpty(name) || value == null) continue;

                        switch (name)
                        {
                            case "displayName":
                                project.Name = value;
                                break;
                            case "frameTitle":
                                project.FrameTitle = value;
                                break;
                            case "productionCode":
                                project.ProductionCode = value;
                                break;
                            case "activationTimestamp":
                                if (long.TryParse(value, out var activationTime))
                                    project.ActivationTimestamp = activationTime;
                                break;
                            case "projectOpenTimestamp":
                                if (long.TryParse(value, out var openTime))
                                    project.ProjectOpenTimestamp = openTime;
                                break;
                            case "build":
                                project.Build = value;
                                break;
                            case "projectWorkspaceId":
                                project.ProjectWorkspaceId = value;
                                break;
                            case "opened":
                                if (bool.TryParse(value, out var isOpened))
                                    project.IsOpened = isOpened;
                                break;
                        }
                    }
                }

                // If no display name, use the last part of the path
                if (string.IsNullOrEmpty(project.Name))
                {
                    project.Name = Path.GetFileName(project.Key) ?? project.Key;
                }

                projects.Add(project);
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to parse recent projects from XML string: {ex.Message}, XML content: {xmlContent}");
        }

        return projects;
    }

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
            
            var doc = XDocument.Load(xmlReader);
            var component = doc.Element("application")
                             ?.Elements("component")
                             .FirstOrDefault(e => e.Attribute("name")?.Value == "RecentProjectsManager");

            var lastOpenedOption = component?.Elements("option")
                .FirstOrDefault(e => e.Attribute("name")?.Value == "lastOpenedProject");
            
            return lastOpenedOption?.Attribute("value")?.Value;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last opened project: {ex.Message}, XML file path: {xmlFilePath}");
        }

        return null;
    }

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
            
            var doc = XDocument.Load(xmlReader);
            var component = doc.Element("application")
                             ?.Elements("component")
                             .FirstOrDefault(e => e.Attribute("name")?.Value == "RecentProjectsManager");

            var lastLocationOption = component?.Elements("option")
                .FirstOrDefault(e => e.Attribute("name")?.Value == "lastProjectLocation");
            
            return lastLocationOption?.Attribute("value")?.Value;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last project location: {ex.Message}, XML file path: {xmlFilePath}");
        }

        return null;
    }
}
