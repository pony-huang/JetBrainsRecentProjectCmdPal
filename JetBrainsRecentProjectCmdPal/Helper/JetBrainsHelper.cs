using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

/// <summary>
/// Unified management class for JetBrains services, integrating product information, icons, shell scripts and other functions
/// Provides core functionality for product discovery, icon retrieval, executable file path lookup and recent project search
/// </summary>
public static class JetBrainsHelper
{
    /// <summary>
    /// Gets the list of installed product information for the specified installation location
    /// </summary>
    /// <param name="installLocation">JetBrains product installation root directory</param>
    /// <returns>List of product information</returns>
    public static List<ProductInfo> GetInstalledProducts(string installLocation)
    {
        return LoadProductInfosFromDirectory(installLocation);
    }

    /// <summary>
    /// Loads all product information from the specified directory
    /// </summary>
    /// <param name="installLocation">Installation root directory</param>
    /// <returns>List of product information</returns>
    private static List<ProductInfo> LoadProductInfosFromDirectory(string installLocation)
    {
        var products = new List<ProductInfo>();

        if (!Directory.Exists(installLocation))
            return products;

        try
        {
            var productDirs = Directory.GetDirectories(installLocation);
            foreach (var productDir in productDirs)
            {
                var productInfo = LoadProductInfoFromDirectory(productDir);
                if (productInfo != null)
                {
                    var launchInfo = productInfo.Launch[0];
                    productInfo.ExecPath = Path.Combine(productDir, launchInfo.LauncherPath).Replace("\\", "/");
                    productInfo.AbsoluteSvgIconPath = FindIconPath(productInfo.ProductCode, productInfo.SvgIconPath, installLocation);
                    products.Add(productInfo);
                }
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"JBCML: Error loading product infos: {ex.Message}");
        }

        return products;
    }

    /// <summary>
    /// Loads product information from a single product directory
    /// </summary>
    /// <param name="productDir">Product directory path</param>
    /// <returns>Product information object, returns null if loading fails</returns>
    public static ProductInfo? LoadProductInfoFromDirectory(string productDir)
    {
        var productInfoPath = Path.Combine(productDir, "product-info.json");
        if (!File.Exists(productInfoPath))
            return null;

        try
        {
            var json = File.ReadAllText(productInfoPath);
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.ProductInfo);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"JBCML: Error parsing product-info.json in {productDir}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Finds the actual file path of the product icon
    /// </summary>
    /// <param name="code">Installation location</param>
    /// <param name="svgIconPath">SVG icon path</param>
    /// <param name="installLocation">Installation location</param>
    /// <returns>Complete path of the icon file</returns>
    private static string FindIconPath(string code, string svgIconPath, string installLocation)
    {
        try
        {
            var productDirs = Directory.GetDirectories(installLocation)
                .Where(dir => IsMatchingProductDirectory(dir, code));

            foreach (var productDir in productDirs)
            {
                var fullIconPath = Path.Combine(productDir, svgIconPath);
                if (File.Exists(fullIconPath))
                {
                    return fullIconPath.Replace("\\", "/");
                }
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"JBCML: Error getting icon path. exception: {ex.Message}");
        }

        return "";
    }

    /// <summary>
    /// Checks if the directory matches the specified product code
    /// </summary>
    /// <param name="dir">Directory path</param>
    /// <param name="productCode">Product code</param>
    /// <returns>Whether it matches</returns>
    private static bool IsMatchingProductDirectory(string dir, string productCode)
    {
        var productInfoPath = Path.Combine(dir, "product-info.json");
        if (!File.Exists(productInfoPath)) return false;

        try
        {
            var json = File.ReadAllText(productInfoPath);
            var info = JsonSerializer.Deserialize(json, AppJsonContext.Default.ProductInfo)!;
            return string.Equals(info?.ProductCode, productCode, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Searches for recent project XML files in the specified directory
    /// </summary>
    /// <param name="directPath">Root directory path to search</param>
    /// <param name="isCustom">Whether it's a custom configuration path</param>
    /// <returns>List of found XML file paths</returns>
    public static List<string> SearchRecentProjectXml(string directPath, bool isCustom)
    {
        var projects = new List<string>();

        if (!Directory.Exists(directPath))
        {
            return projects;
        }

        foreach (var dir in Directory.GetDirectories(directPath))
        {
            var productName = Path.GetFileName(dir);
            var recentProjectsPath = isCustom
                ? GetRecentProjectsCustomPath(dir, productName)
                : GetRecentProjectsPath(dir, productName);

            if (!string.IsNullOrEmpty(recentProjectsPath) && File.Exists(recentProjectsPath))
            {
                projects.Add(recentProjectsPath);
            }
        }

        return projects;
    }

    /// <summary>
    /// Searches and parses recent project information
    /// </summary>
    /// <param name="configLocation">Configuration file location</param>
    /// <param name="isCustom">Whether it's a custom configuration</param>
    /// <returns>List of recent projects</returns>
    public static List<RecentProject> SearchRecentProjects(string configLocation, bool isCustom)
    {
        var projects = new List<RecentProject>();
        foreach (var path in SearchRecentProjectXml(configLocation, isCustom))
        {
            projects.AddRange(RecentProjectsParser.ParseFromFile(path));
        }

        return projects;
    }
    
    public static List<ProjectItem> MergerProductProject(List<RecentProject> projects, List<ProductInfo> products)
    {
        var productDict = products.GroupBy(p => p.ProductCode)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.BuildNumber)
                    .ToDictionary(p => p.BuildNumber)
            );
        
        List<ProjectItem> projectItems = new();

        foreach (var project in projects)
        {
            var productInfos = productDict.GetValueOrDefault(project.ProductionCode);
            if (productInfos != null && productInfos.Count != 0)
            {
                foreach (var v in productInfos.Keys)
                {
                    if (project.Build.Contains(v))
                    {
                        var product = productInfos[v];
                        projectItems.Add(new ProjectItem
                        {
                            WorkPlacePath = project.Key,
                            ProjectIsOpened = project.IsOpened,
                            ProjectName = project.Name,
                            ProjectFrameTitle = project.FrameTitle,
                            Code = project.ProductionCode,
                            ProjectBuild = project.Build,
                            ProductVersion = product.Version,
                            ProductBuildNumber = product.BuildNumber,
                            ProductVendor = product.ProductVendor,
                            Bin = product.ExecPath,
                            Project = project,
                            Product = product
                        });
                    }
                
                }
            }

        }
        return projectItems;
    }

    /// <summary>
    /// Gets the recent projects file path under custom configuration path
    /// </summary>
    /// <param name="dir">Product directory</param>
    /// <param name="productName">Product name</param>
    /// <returns>Recent projects file path</returns>
    private static string GetRecentProjectsCustomPath(string dir, string productName)
    {
        // Rider uses recentSolutions.xml, other products use recentProjects.xml
        bool isRider = productName.Contains("rider", StringComparison.CurrentCultureIgnoreCase);
        string fileName = isRider ? "recentSolutions.xml" : "recentProjects.xml";
        return Path.Combine(dir, "config", "options", fileName);
    }

    /// <summary>
    /// Gets the recent projects file path under standard configuration path
    /// </summary>
    /// <param name="dir">Product directory</param>
    /// <param name="productName">Product name</param>
    /// <returns>Recent projects file path</returns>
    private static string GetRecentProjectsPath(string dir, string productName)
    {
        // Rider uses recentSolutions.xml, other products use recentProjects.xml
        bool isRider = productName.Contains("rider", StringComparison.CurrentCultureIgnoreCase);
        string fileName = isRider ? "recentSolutions.xml" : "recentProjects.xml";
        return Path.Combine(dir, "options", fileName);
    }

    private static readonly Dictionary<string, string> ProductCodeNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IU"] = "IntelliJ IDEA Ultimate",
        ["IC"] = "IntelliJ IDEA Community",
        ["IE"] = "IntelliJ IDEA Educational",
        ["PS"] = "PhpStorm",
        ["WS"] = "WebStorm",
        ["PY"] = "PyCharm Professional",
        ["PC"] = "PyCharm Community",
        ["PE"] = "PyCharm Educational",
        ["RM"] = "RubyMine",
        ["OC"] = "AppCode",
        ["CL"] = "CLion",
        ["GO"] = "GoLand",
        ["DB"] = "DataGrip",
        ["RD"] = "Rider",
        ["AI"] = "Android Studio",
        ["RR"] = "RustRover",
        ["QA"] = "Aqua"
    };

    public static string GetProductName(string code , string defaultName)
    {
        if (ProductCodeNameMap.TryGetValue(code, out var name))
            return name;
        return string.IsNullOrWhiteSpace(defaultName) ? "Unknown" : defaultName;
    }
}

public class ProjectItem
{
    public required string Bin { get; set; }
    public required string ProjectName { get; set; }
    public required string ProjectFrameTitle { get; set; }
    public required string WorkPlacePath { get; set; }
    public required bool ProjectIsOpened { get; set; }
    public required string Code { get; set; }
    public required string ProjectBuild { get; set; }
    public required string ProductVersion { get; set; }
    public required string ProductBuildNumber { get; set; }
    public required string ProductVendor { get; set; }
    public required RecentProject Project { get; set; }
    public required ProductInfo Product { get; set; }
}
