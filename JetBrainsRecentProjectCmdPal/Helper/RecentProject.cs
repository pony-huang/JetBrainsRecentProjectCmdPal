namespace JetBrainsRecentProjectCmdPal.Helper;

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
}