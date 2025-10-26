using System;
using System.Collections.Generic;
using System.IO;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Settings;

namespace JetBrainsRecentProjectCmdPal.Helper
{
    public static class Search
    {

        public static readonly SettingsManager Settings = new();
        

        /// <summary>
        /// Gets the executable file path for the specified product based on settings
        /// Traverses all installation locations to find matching executable files
        /// </summary>
        /// <param name="productCode">Product code (e.g., IU, PS, WS, etc.)</param>
        /// <returns>Executable file path, returns empty string if not found</returns>
        public static string GetProductBinBySettings(string productCode)
        {
            foreach (var installLocation in GetInstalledProductPaths())
            {
                var executablePath = JetBrainsHelper.GetExecutablePath(productCode, installLocation);
                if (!string.IsNullOrEmpty(executablePath))
                {
                    return executablePath;
                }
            }

            return "";
        }

        /// <summary>
        /// Searches for recent projects list based on user settings
        /// </summary>
        /// <returns>List of recent projects</returns>
        public static List<RecentProject> SearchRecentProjectsBySettings()
        {
            List<RecentProject> recentProjects = new();
            
            if (!Settings.JetBrainsCustomOption)
            {
                var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var jetbrainsRoamingPath = Path.Combine(roamingPath, "JetBrains");
                var googleRoamingPath = Path.Combine(roamingPath, "Google");
                
                recentProjects.AddRange(
                    JetBrainsHelper.SearchRecentProjects(jetbrainsRoamingPath, false));
                recentProjects.AddRange(
                    JetBrainsHelper.SearchRecentProjects(googleRoamingPath, false));
            }
            else
            {
                recentProjects.AddRange(
                    JetBrainsHelper.SearchRecentProjects(Settings.SystemConfigLogsLocation, true));
            }

            return recentProjects;
        }

        /// <summary>
        /// Gets a list of all installed JetBrains product information
        /// </summary>
        /// <returns>List of product information</returns>
        public static List<ProductInfo> GetInstalledProducts()
        {
            List<ProductInfo> allProductInfos = new();
            
            foreach (var installLocation in GetInstalledProductPaths())
            {
                allProductInfos.AddRange(JetBrainsHelper.GetInstalledProducts(installLocation));
            }

            return allProductInfos;
        }

        /// <summary>
        /// Gets the product icon path based on product information
        /// Searches for matching icon files in all installation locations
        /// </summary>
        /// <param name="productInfo">Product information object</param>
        /// <returns>Icon file path, returns empty string if not found</returns>
        public static string GetProductIconPath(ProductInfo productInfo)
        {
            foreach (var installLocation in GetInstalledProductPaths())
            {
                var productIconPath = JetBrainsHelper.GetProductIconPath(productInfo, installLocation);
                if (!string.IsNullOrEmpty(productIconPath))
                    return productIconPath;
            }

            return "";
        }

        /// <summary>
        /// Gets the list of JetBrains installation paths
        /// Returns custom paths or system default paths based on user settings
        /// </summary>
        /// <returns>List of installation paths</returns>
        private static List<String> GetInstalledProductPaths()
        {
            List<String> installLocations = new();
            
            if (Settings.JetBrainsCustomOption)
            {
                installLocations.Add(Settings.ToolsInstallLocation);
            }
            else
            {
                // Use the system's default installation location
                var localInstallLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var jetbrainsInstallLocation = Path.Combine(localInstallLocation, "JetBrains");
                // Android Studio installation location
                var googleInstallLocation = Path.Combine(localInstallLocation, "Google");
                
                installLocations.Add(jetbrainsInstallLocation);
                installLocations.Add(googleInstallLocation);
            }

            return installLocations;
        }
    }
}