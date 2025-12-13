using System;
using System.Collections.Generic;
using System.IO;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Settings;

namespace JetBrainsRecentProjectCmdPal.Helper;

public static class Search
{
    public static readonly SettingsManager Settings = new();
    
    /// <summary>
    /// Product information cache manager with 15-second cache duration
    /// </summary>
    private static readonly CacheManager<List<ProjectItem>> ProductInfoCache = new(TimeSpan.FromSeconds(30));

    /// <summary>
    /// Searches for recent projects list based on user settings
    /// </summary>
    /// <returns>List of recent projects</returns>
    public static List<RecentProject> SearchRecentProjects()
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
    public static List<ProductInfo> SearchInstalledProducts()
    {
        List<ProductInfo> allProductInfos = new();

        foreach (var installLocation in GetInstalledProductPaths())
        {
            allProductInfos.AddRange(JetBrainsHelper.GetInstalledProducts(installLocation));
        }

        return allProductInfos;
    }

    public static List<ProjectItem> SearchProjects()
    {
        var cacheKey = $"all_project";
        var tryGet = ProductInfoCache.TryGet(cacheKey, out var cachedProducts);
        if (tryGet)
            return cachedProducts;
        var searchInstalledProducts = SearchInstalledProducts();
        var searchRecentProjects = SearchRecentProjects();
        var items = JetBrainsHelper.MergerProductProject(searchRecentProjects, searchInstalledProducts);
        ProductInfoCache.Set(cacheKey, items);
        return items;
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