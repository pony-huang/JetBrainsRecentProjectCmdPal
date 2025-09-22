using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JetBrainsRecentProjectCmdPal.Helper
{
    
    
    /// <summary>
    /// Represents the root structure of JetBrains IntelliJ IDEA's recentProjects.xml file
    /// </summary>
    [XmlRoot("application")]
    public class RecentProjectsApplication
    {
        [XmlElement("component")]
        public RecentProjectsManager Component { get; set; } = new();
    }

    /// <summary>
    /// Represents the component element in recentProjects.xml file
    /// </summary>
    public class RecentProjectsManager
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = "";

        [XmlElement("option")]
        public List<RecentProjectsOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Represents the option element in recentProjects.xml file
    /// </summary>
    public class RecentProjectsOption
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = "";

        [XmlAttribute("value")]
        public string Value { get; set; } = "";

        [XmlElement("list")]
        public RecentProjectsList List { get; set; } = new();

        [XmlElement("map")]
        public RecentProjectsMap Map { get; set; } = new();
    }

    /// <summary>
    /// Represents the list element in recentProjects.xml file
    /// </summary>
    public class RecentProjectsList
    {
        [XmlElement("option")]
        public List<RecentProjectsListOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Represents the option element within list in recentProjects.xml file
    /// </summary>
    public class RecentProjectsListOption
    {
        [XmlAttribute("value")]
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// Represents the map element in recentProjects.xml file
    /// </summary>
    public class RecentProjectsMap
    {
        [XmlElement("entry")]
        public List<RecentProjectsEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Represents the entry element within map in recentProjects.xml file
    /// </summary>
    public class RecentProjectsEntry
    {
        [XmlAttribute("key")]
        public string Key { get; set; } = "";

        [XmlElement("value")]
        public RecentProjectsEntryValue Value { get; set; } = new();
    }

    /// <summary>
    /// Represents the value element of entry
    /// </summary>
    public class RecentProjectsEntryValue
    {
        [XmlElement("RecentProjectMetaInfo")]
        public RecentProjectMetaInfo RecentProjectMetaInfo { get; set; } = new();
    }

    /// <summary>
    /// Represents the RecentProjectMetaInfo element
    /// </summary>
    public class RecentProjectMetaInfo
    {
        [XmlElement("option")]
        public List<RecentProjectMetaInfoOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Represents the option element within RecentProjectMetaInfo
    /// </summary>
    public class RecentProjectMetaInfoOption
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = "";

        [XmlAttribute("value")]
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// Represents recent project information
    /// </summary>
    public class RecentProject
    {
        /// <summary>
        /// 项目路径
        /// </summary>
        public string Key { get; set; } = "";

        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string FrameTitle { get; set; } = "";

        /// <summary>
        /// 产品代码
        /// </summary>
        public string ProductionCode { get; set; } = "";

        /// <summary>
        /// 激活时间戳
        /// </summary>
        public long ActivationTimestamp { get; set; }

        /// <summary>
        /// 项目打开时间戳
        /// </summary>
        public long ProjectOpenTimestamp { get; set; }

        /// <summary>
        /// 构建信息
        /// </summary>
        public string Build { get; set; } = "";

        /// <summary>
        /// 项目工作区ID
        /// </summary>
        public string ProjectWorkspaceId { get; set; } = "";

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpened { get; set; }

        /// <summary>
        /// Creates a RecentProject instance from XML Entry
        /// </summary>
        /// <param name="entry">XML entry element</param>
        /// <returns>RecentProject instance</returns>
        public static RecentProject FromEntry(RecentProjectsEntry entry)
        {
            var project = new RecentProject
            {
                Key = entry.Key
            };

            if (entry.Value?.RecentProjectMetaInfo?.Options != null)
            {
                foreach (var option in entry.Value.RecentProjectMetaInfo.Options)
                {
                    switch (option.Name)
                    {
                        case "displayName":
                            project.Name = option.Value;
                            break;
                        case "frameTitle":
                            project.FrameTitle = option.Value;
                            break;
                        case "productionCode":
                            project.ProductionCode = option.Value;
                            break;
                        case "activationTimestamp":
                            if (long.TryParse(option.Value, out var activationTime))
                                project.ActivationTimestamp = activationTime;
                            break;
                        case "projectOpenTimestamp":
                            if (long.TryParse(option.Value, out var openTime))
                                project.ProjectOpenTimestamp = openTime;
                            break;
                        case "build":
                            project.Build = option.Value;
                            break;
                        case "projectWorkspaceId":
                            project.ProjectWorkspaceId = option.Value;
                            break;
                        case "opened":
                            if (bool.TryParse(option.Value, out var isOpened))
                                project.IsOpened = isOpened;
                            break;
                    }
                }
            }

            // 如果没有显示名称，使用路径的最后一部分作为名称
            if (string.IsNullOrEmpty(project.Name))
            {
                project.Name = System.IO.Path.GetFileName(project.Key) ?? project.Key;
            }

            return project;
        }
    }
}