using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper
{
    /// <summary>
    /// Unified management class for JetBrains services, integrating product information, icons, shell scripts and other functions
    /// Provides core functionality for product discovery, icon retrieval, executable file path lookup and recent project search
    /// </summary>
    public static class JetBrainsService
    {
        /// <summary>
        /// Product information cache manager with 15-second cache duration
        /// </summary>
        private static readonly  CacheManager<List<ProductInfo>> ProductInfoCache = new(TimeSpan.FromSeconds(15));
        
        /// <summary>
        /// Icon path cache manager with 15-second cache duration
        /// </summary>
        private static readonly CacheManager<string> IconPathCache = new(TimeSpan.FromSeconds(15));
        
        /// <summary>
        /// JSON serialization options with case-insensitive property names
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = AppJsonContext.Default
        };

        /// <summary>
        /// Gets the list of installed product information for the specified installation location
        /// </summary>
        /// <param name="installLocation">JetBrains product installation root directory</param>
        /// <returns>List of product information</returns>
        public static List<ProductInfo> GetInstalledProducts(string installLocation)
        {
            var cacheKey = $"all_products_{installLocation}";
            if (ProductInfoCache.TryGet(cacheKey, out var cachedProducts))
                return cachedProducts;
            
            var products = LoadProductInfosFromDirectory(installLocation);
            ProductInfoCache.Set(cacheKey, products);
            return products;
        }

        /// <summary>
        /// Finds specific product information by product code
        /// </summary>
        /// <param name="productCode">Product code (e.g., IU, PS, WS, etc.)</param>
        /// <param name="installLocation">Installation location</param>
        /// <returns>Matching product information, returns null if not found</returns>
        public static ProductInfo FindProductByCode(string productCode, string installLocation)
        {
            var products = GetInstalledProducts(installLocation);
            return products.FirstOrDefault(p =>
                string.Equals(p.ProductCode, productCode, StringComparison.OrdinalIgnoreCase))!;
        }

        /// <summary>
        /// Gets the complete file path of the product icon
        /// </summary>
        /// <param name="productInfo">Product information object</param>
        /// <param name="installLocation">Installation location</param>
        /// <returns>Complete path of the icon file, returns empty string if not found</returns>
        public static string GetProductIconPath(ProductInfo productInfo, string installLocation)
        {
            if (string.IsNullOrEmpty(productInfo.SvgIconPath))
                return "";
            
            var cacheKey = $"icon_{productInfo.ProductCode}";

            if (IconPathCache.TryGet(cacheKey, out var cachedPath))
                return cachedPath;

            var iconPath = FindIconPath(productInfo, installLocation);
            if (!string.IsNullOrEmpty(iconPath))
            {
                IconPathCache.Set(cacheKey, iconPath);
                return iconPath;
            }

            ExtensionHost.LogMessage($"JBCML: No icon found for product code {productInfo}");
            return "";
        }

        /// <summary>
        /// Gets the complete path of the executable file based on product code
        /// </summary>
        /// <param name="productCode">Product code</param>
        /// <param name="installLocation">Installation location</param>
        /// <returns>Executable file path using forward slash separators</returns>
        public static string GetExecutablePath(string productCode, string installLocation)
        {
            var product = FindProductByCode(productCode, installLocation);
            var binaryPath = FindLauncherPath(product, installLocation);
            if (!string.IsNullOrEmpty(binaryPath))
            {
                return binaryPath.Replace("\\", "/");
            }

            ExtensionHost.LogMessage($"JBCML: No executable found for product code {productCode}");
            return "";
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
        public static ProductInfo LoadProductInfoFromDirectory(string productDir)
        {
            var productInfoPath = Path.Combine(productDir, "product-info.json");
            if (!File.Exists(productInfoPath))
                return null;
        
            try
            {
                var json = File.ReadAllText(productInfoPath);
                return JsonSerializer.Deserialize<ProductInfo>(json, JsonOptions)!;
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
        /// <param name="productInfo">Product information</param>
        /// <param name="installLocation">Installation location</param>
        /// <returns>Complete path of the icon file</returns>
        private static string FindIconPath(ProductInfo productInfo, string installLocation)
        {
            try
            {
                var productDirs = Directory.GetDirectories(installLocation)
                    .Where(dir => IsMatchingProductDirectory(dir, productInfo.ProductCode));

                foreach (var productDir in productDirs)
                {
                    var fullIconPath = Path.Combine(productDir, productInfo.SvgIconPath);
                    if (File.Exists(fullIconPath))
                    {
                        return fullIconPath.Replace("\\", "/");
                    }
                }
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage($"JBCML: Error getting icon path for {productInfo.Name}: {ex.Message}");
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
                var info =  JsonSerializer.Deserialize<ProductInfo>(json, JsonOptions)!;
                return string.Equals(info?.ProductCode, productCode, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the launcher executable file path for the product
        /// </summary>
        /// <param name="product">Product information</param>
        /// <param name="installLocation">Installation location</param>
        /// <returns>Complete path of the launcher file</returns>
        private static string FindLauncherPath(ProductInfo product, String installLocation)
        {
            // Find Windows platform launch configuration
            var windowsLaunch = product.Launch?.FirstOrDefault(l =>
                string.Equals(l.Os, "Windows", StringComparison.OrdinalIgnoreCase));

            if (windowsLaunch == null || string.IsNullOrEmpty(windowsLaunch.LauncherPath))
                return "";

            var productDirs = Directory.GetDirectories(installLocation)
                .Where(dir => IsMatchingProductDirectory(dir, product.ProductCode));

            foreach (var productDir in productDirs)
            {
                var launcherPath = Path.Combine(productDir, windowsLaunch.LauncherPath);
                if (File.Exists(launcherPath))
                {
                    return launcherPath;
                }
            }

            ExtensionHost.LogMessage($"JBCML: No launcher found for product code {product.ProductCode}");
            return "";
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
    }
}