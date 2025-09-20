using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

internal sealed class Icons
{
    internal static IconInfo AndroidStudioIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\androidstudio.png");
    internal static IconInfo AppCodeIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\appcode.png");
    internal static IconInfo CLionIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\clion.png");
    internal static IconInfo DataGripIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\datagrip.svg");
    internal static IconInfo DataSpellIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\dataspell.png");
    internal static IconInfo FleetIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\fleet.png");
    internal static IconInfo GoLandIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\goland.png");
    internal static IconInfo IdeaIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\idea.png");
    internal static IconInfo IdeaCIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\ideac.png");
    internal static IconInfo PhpStormIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\phpstorm.png");
    internal static IconInfo PyCharmIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\pycharm.png");
    internal static IconInfo PyCharmCIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\pycharmc.png");
    internal static IconInfo RiderIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\rider.png");
    internal static IconInfo RubyMineIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\rubymine.png");
    internal static IconInfo RustRoverIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\rustrover.png");
    internal static IconInfo WebStormIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\webstorm.png");
    internal static IconInfo JetBrainsIcon { get; } = IconHelpers.FromRelativePath("Assets\\Images\\JetBrains.svg");

    /// <summary>
    /// 根据产品名称获取对应的图标
    /// </summary>
    /// <param name="productName">产品名称</param>
    /// <returns>对应的图标，如果找不到则返回默认图标</returns>
    internal static IconInfo GetIconForProduct(string productName)
    {
        if (string.IsNullOrEmpty(productName))
            return IdeaIcon; // 默认图标

        return productName.ToLowerInvariant() switch
        {
            var name when name.Contains("intellij idea ultimate") => IdeaIcon,
            var name when name.Contains("intellij idea community") => IdeaCIcon,
            var name when name.Contains("intellij idea educational") => IdeaCIcon,
            var name when name.Contains("phpstorm") => PhpStormIcon,
            var name when name.Contains("webstorm") => WebStormIcon,
            var name when name.Contains("pycharm professional") => PyCharmIcon,
            var name when name.Contains("pycharm community") => PyCharmCIcon,
            var name when name.Contains("pycharm educational") => PyCharmCIcon,
            var name when name.Contains("rubymine") => RubyMineIcon,
            var name when name.Contains("appcode") => AppCodeIcon,
            var name when name.Contains("clion") => CLionIcon,
            var name when name.Contains("goland") => GoLandIcon,
            var name when name.Contains("datagrip") => DataGripIcon,
            var name when name.Contains("rider") => RiderIcon,
            var name when name.Contains("android studio") => AndroidStudioIcon,
            var name when name.Contains("rustrover") => RustRoverIcon,
            var name when name.Contains("dataspell") => DataSpellIcon,
            var name when name.Contains("fleet") => FleetIcon,
            _ => IdeaIcon // 默认图标
        };
    }

    /// <summary>
    /// 根据产品信息获取对应的图标，优先使用产品自带的图标
    /// </summary>
    /// <param name="productInfo">产品信息</param>
    /// <returns>对应的图标，如果找不到则返回默认图标</returns>
    public static IconInfo GetIconForProductInfo(ProductInfo productInfo)
    {
        if (productInfo == null)
            return IdeaIcon;

        // 首先尝试使用产品自带的图标
        var installLocation = Query.Settings.ToolsInstallLocation;
        if (!string.IsNullOrEmpty(installLocation))
        {
            var iconPath = Query.GetProductIconPath(productInfo, installLocation);
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    return  new IconInfo(iconPath);;
                }
                catch (Exception ex)
                {
                    // 如果加载自定义图标失败，记录日志并回退到内置图标
                    ExtensionHost.LogMessage($"JBCML: Failed to load icon from {iconPath}: {ex.Message}");
                }
            }
        }

        // 回退到基于产品名称的内置图标
        return GetIconForProduct(productInfo.Name);
    }

    /// <summary>
    /// 根据产品代码获取对应的图标
    /// </summary>
    /// <param name="productCode">产品代码</param>
    /// <returns>对应的图标，如果找不到则返回默认图标</returns>
    public static IconInfo GetIconForProductCode(string productCode)
    {
        if (string.IsNullOrEmpty(productCode))
            return IdeaIcon;

        return productCode.ToUpperInvariant() switch
        {
            "IC" => IdeaCIcon,
            "IE" or "IU" => IdeaIcon,
            "PS" => PhpStormIcon,
            "WS" => WebStormIcon,
            "PY" or "PC" or "PE" => PyCharmIcon,
            "RM" => RubyMineIcon,
            "OC" or "AC" => AppCodeIcon,
            "CL" => CLionIcon,
            "GO" => GoLandIcon,
            "DG" => DataGripIcon,
            "DS" => DataSpellIcon,
            "FL" => FleetIcon,
            "RD" => RiderIcon,
            "AI" => AndroidStudioIcon,
            "RR" or "RO" => RustRoverIcon,
            _ => IdeaIcon
        };
    }
}