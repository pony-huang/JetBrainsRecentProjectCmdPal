using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
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
            // Read file with automatic encoding detection
            var xmlContent = ReadXmlFileWithEncoding(xmlFilePath);
            if (string.IsNullOrEmpty(xmlContent))
                return new List<RecentProject>();

            return ParseFromXmlString(xmlContent);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to parse recent projects from XML file: {ex.Message}, XML file path: {xmlFilePath}");
            return new List<RecentProject>();
        }
    }

    /// <summary>
    /// Reads XML file with automatic encoding detection
    /// </summary>
    private static string ReadXmlFileWithEncoding(string filePath)
    {
        // Use a buffer to detect encoding from BOM or XML declaration
        var buffer = new byte[4096];
        int bytesRead;

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            bytesRead = fs.Read(buffer, 0, buffer.Length);
        }

        if (bytesRead == 0)
            return string.Empty;

        // Detect encoding from BOM
        Encoding encoding;
        if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            encoding = Encoding.UTF8;
        }
        else if (bytesRead >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
        {
            encoding = Encoding.Unicode; // UTF-16 LE
        }
        else if (bytesRead >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
        {
            encoding = Encoding.BigEndianUnicode; // UTF-16 BE
        }
        else
        {
            // Try to detect from XML declaration
            var preamble = Encoding.ASCII.GetString(buffer, 0, Math.Min(bytesRead, 100));
            var xmlDeclMatch = System.Text.RegularExpressions.Regex.Match(preamble, @"<\?xml[^>]+encoding\s*=\s*[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            encoding = xmlDeclMatch.Success
                ? Encoding.GetEncoding(xmlDeclMatch.Groups[1].Value)
                : Encoding.UTF8;
        }

        return encoding.GetString(buffer, 0, bytesRead);
    }

    public static List<RecentProject> ParseFromXmlString(string xmlContent)
    {
        ArgumentNullException.ThrowIfNull(xmlContent);

        var projects = new List<RecentProject>();

        if (string.IsNullOrWhiteSpace(xmlContent))
            return projects;

        try
        {
            // Use XmlReader for forward-only, memory-efficient parsing
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            // Parse using forward-only reader for better compatibility
            projects = ParseXmlForward(xmlReader);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to parse recent projects from XML string: {ex.Message}");
        }

        return projects;
    }

    /// <summary>
    /// Parses XML using forward-only XmlReader for maximum compatibility
    /// </summary>
    private static List<RecentProject> ParseXmlForward(XmlReader reader)
    {
        var projects = new List<RecentProject>();
        RecentProject? currentProject = null;
        string? currentElement = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.Name;

                    if (reader.Name == "entry")
                    {
                        var key = reader.GetAttribute("key");
                        if (!string.IsNullOrEmpty(key))
                        {
                            currentProject = new RecentProject { Key = key };
                        }
                    }
                    else if (reader.Name == "RecentProjectMetaInfo" && currentProject != null)
                    {
                        // Will process child elements
                    }
                    else if (reader.Name == "option" && currentProject != null)
                    {
                        var name = reader.GetAttribute("name");
                        var value = reader.GetAttribute("value");

                        if (!string.IsNullOrEmpty(name) && value != null)
                        {
                            switch (name)
                            {
                                case "displayName":
                                    currentProject.Name = value;
                                    break;
                                case "frameTitle":
                                    currentProject.FrameTitle = value;
                                    break;
                                case "productionCode":
                                    currentProject.ProductionCode = value;
                                    break;
                                case "activationTimestamp":
                                    if (long.TryParse(value, out var activationTime))
                                        currentProject.ActivationTimestamp = activationTime;
                                    break;
                                case "projectOpenTimestamp":
                                    if (long.TryParse(value, out var openTime))
                                        currentProject.ProjectOpenTimestamp = openTime;
                                    break;
                                case "build":
                                    currentProject.Build = value;
                                    break;
                                case "projectWorkspaceId":
                                    currentProject.ProjectWorkspaceId = value;
                                    break;
                                case "opened":
                                    if (bool.TryParse(value, out var isOpened))
                                        currentProject.IsOpened = isOpened;
                                    break;
                            }
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name == "entry" && currentProject != null)
                    {
                        // If no display name, use the last part of the path
                        if (string.IsNullOrEmpty(currentProject.Name))
                        {
                            currentProject.Name = Path.GetFileName(currentProject.Key) ?? currentProject.Key;
                        }
                        projects.Add(currentProject);
                        currentProject = null;
                    }
                    currentElement = null;
                    break;
            }
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
            var xmlContent = ReadXmlFileWithEncoding(xmlFilePath);
            if (string.IsNullOrEmpty(xmlContent))
                return null;

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            // Forward-only parsing for lastOpenedProject option
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "option")
                {
                    var name = xmlReader.GetAttribute("name");
                    if (name == "lastOpenedProject")
                    {
                        return xmlReader.GetAttribute("value");
                    }
                }
            }
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
            var xmlContent = ReadXmlFileWithEncoding(xmlFilePath);
            if (string.IsNullOrEmpty(xmlContent))
                return null;

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            // Forward-only parsing for lastProjectLocation option
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "option")
                {
                    var name = xmlReader.GetAttribute("name");
                    if (name == "lastProjectLocation")
                    {
                        return xmlReader.GetAttribute("value");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"Failed to get last project location: {ex.Message}, XML file path: {xmlFilePath}");
        }

        return null;
    }
}
