using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JetBrainsRecentProjectCmdPal.Helper;

/// <summary>
/// 表示JetBrains IntelliJ IDEA的recentProjects.xml文件的根结构
/// </summary>
[XmlRoot("application")]
public class RecentProjectsApplication
{
    [XmlElement("component")]
    public RecentProjectsManager Component { get; set; } = new();
}

/// <summary>
/// 表示RecentProjectsManager组件
/// </summary>
public class RecentProjectsManager
{
    [XmlAttribute("name")]
    public string Name { get; set; } = "RecentProjectsManager";

    [XmlElement("option")]
    public List<RecentProjectManagerOption> Options { get; set; } = new();
}

/// <summary>
/// 表示RecentProjectsManager的选项
/// </summary>
public class RecentProjectManagerOption
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string Value { get; set; } = string.Empty;

    [XmlElement("map")]
    public RecentProjectMap? Map { get; set; }
}

/// <summary>
/// 表示项目映射
/// </summary>
public class RecentProjectMap
{
    [XmlElement("entry")]
    public List<RecentProjectEntry> Entries { get; set; } = new();
}

/// <summary>
/// 表示单个项目条目
/// </summary>
public class RecentProjectEntry
{
    [XmlAttribute("key")]
    public string Key { get; set; } = string.Empty;

    [XmlElement("value")]
    public RecentProjectValue Value { get; set; } = new();
}

/// <summary>
/// 表示项目值
/// </summary>
public class RecentProjectValue
{
    [XmlElement("RecentProjectMetaInfo")]
    public RecentProjectMetaInfo MetaInfo { get; set; } = new();
}

/// <summary>
/// 表示项目元信息
/// </summary>
public class RecentProjectMetaInfo
{
    [XmlAttribute("frameTitle")]
    public string FrameTitle { get; set; } = string.Empty;

    [XmlAttribute("projectWorkspaceId")]
    public string ProjectWorkspaceId { get; set; } = string.Empty;

    [XmlAttribute("opened")]
    public string Opened { get; set; } = string.Empty;

    [XmlElement("option")]
    public List<RecentProjectOption> Options { get; set; } = new();

    [XmlElement("frame")]
    public FrameInfo Frame { get; set; } = new();
}

/// <summary>
/// 表示项目选项
/// </summary>
public class RecentProjectOption
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string Value { get; set; } = string.Empty;

    [XmlElement("RecentProjectColorInfo")]
    public RecentProjectColorInfo? ColorInfo { get; set; }
}

/// <summary>
/// 表示项目颜色信息
/// </summary>
public class RecentProjectColorInfo
{
    [XmlAttribute("associatedIndex")]
    public int AssociatedIndex { get; set; }
}

/// <summary>
/// 表示窗口框架信息
/// </summary>
public class FrameInfo
{
    [XmlAttribute("x")]
    public int X { get; set; }

    [XmlAttribute("y")]
    public int Y { get; set; }

    [XmlAttribute("width")]
    public int Width { get; set; }

    [XmlAttribute("height")]
    public int Height { get; set; }
}

/// <summary>
/// 简化的最近项目类，用于业务逻辑处理
/// </summary>
public class RecentProject
{
    /// <summary>
    /// 项目路径（作为键）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 项目名称（从FrameTitle提取）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 二进制文件夹路径
    /// </summary>
    public string BinFolder { get; set; } = string.Empty;

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string FrameTitle { get; set; } = string.Empty;

    /// <summary>
    /// 项目工作区ID
    /// </summary>
    public string ProjectWorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// 激活时间戳
    /// </summary>
    public long ActivationTimestamp { get; set; }

    /// <summary>
    /// 构建时间戳
    /// </summary>
    public long BuildTimestamp { get; set; }

    /// <summary>
    /// 构建版本
    /// </summary>
    public string Build { get; set; } = string.Empty;

    /// <summary>
    /// 产品代码
    /// </summary>
    public string ProductionCode { get; set; } = string.Empty;

    /// <summary>
    /// 项目打开时间戳
    /// </summary>
    public long ProjectOpenTimestamp { get; set; }

    /// <summary>
    /// 是否当前打开
    /// </summary>
    public bool IsOpened { get; set; }

    /// <summary>
    /// 颜色关联索引
    /// </summary>
    public int ColorAssociatedIndex { get; set; }

    /// <summary>
    /// 窗口位置和大小信息
    /// </summary>
    public FrameInfo Frame { get; set; } = new();

    /// <summary>
    /// 从RecentProjectEntry创建RecentProject实例
    /// </summary>
    /// <param name="entry">项目条目</param>
    /// <returns>RecentProject实例</returns>
    public static RecentProject FromEntry(RecentProjectEntry entry)
    {
        var metaInfo = entry.Value.MetaInfo;
        var project = new RecentProject
        {
            Key = entry.Key,
            FrameTitle = metaInfo.FrameTitle,
            ProjectWorkspaceId = metaInfo.ProjectWorkspaceId,
            IsOpened = !string.IsNullOrEmpty(metaInfo.Opened) && string.Equals(metaInfo.Opened, "true", StringComparison.OrdinalIgnoreCase),
            Frame = metaInfo.Frame
        };

        // 从FrameTitle提取项目名称
        if (!string.IsNullOrEmpty(metaInfo.FrameTitle))
        {
            var parts = metaInfo.FrameTitle.Split('–', StringSplitOptions.RemoveEmptyEntries);
            project.Name = parts[0].Trim();
        }

        // 解析选项
        foreach (var option in metaInfo.Options)
        {
            switch (option.Name)
            {
                case "activationTimestamp":
                    if (long.TryParse(option.Value, out var activationTime))
                        project.ActivationTimestamp = activationTime;
                    break;
                case "binFolder":
                    project.BinFolder = option.Value;
                    break;
                case "build":
                    project.Build = option.Value;
                    break;
                case "buildTimestamp":
                    if (long.TryParse(option.Value, out var buildTime))
                        project.BuildTimestamp = buildTime;
                    break;
                case "productionCode":
                    project.ProductionCode = option.Value;
                    break;
                case "projectOpenTimestamp":
                    if (long.TryParse(option.Value, out var openTime))
                        project.ProjectOpenTimestamp = openTime;
                    break;
                case "colorInfo":
                    if (option.ColorInfo != null)
                        project.ColorAssociatedIndex = option.ColorInfo.AssociatedIndex;
                    break;
            }
        }

        return project;
    }
}