using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

public static class IconHelper
{
    internal static IconInfo DefaultIconInfo { get; } = IconHelpers.FromRelativePath("Assets\\Project.svg");
}