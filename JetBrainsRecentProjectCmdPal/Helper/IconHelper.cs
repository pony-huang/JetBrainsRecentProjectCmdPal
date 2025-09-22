using System;
using JetBrainsRecentProjectCmdPal.Models;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper
{
    public static class IconHelper
    {
        internal static IconInfo DefaultIconInfo { get; } = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        public static IconInfo GetIconForProductInfo(ProductInfo productInfo)
        {
            var iconPath = Search.GetProductIconPath(productInfo);
            if (string.IsNullOrEmpty(iconPath))
                return DefaultIconInfo;

            try
            {
                return new IconInfo(iconPath);
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage($"JBCML: Failed to load icon from {iconPath}: {ex.Message}");
                return DefaultIconInfo;
            }
        }
    }
}