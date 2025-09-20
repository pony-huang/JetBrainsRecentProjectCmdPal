using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JetBrainsRecentProjectCmdPal.Settings;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper
{
    /// <summary>
    /// 产品信息模型，对应 product-info.json 文件结构
    /// </summary>
    public class ProductInfo
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string BuildNumber { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string EnvVarBaseName { get; set; } = "";
        public string DataDirectoryName { get; set; } = "";
        public string SvgIconPath { get; set; } = "";
        public string ProductVendor { get; set; } = "";
        public string MajorVersionReleaseDate { get; set; } = "";
        public LaunchInfo[] Launch { get; set; } = Array.Empty<LaunchInfo>();
    }

    /// <summary>
    /// 启动信息模型
    /// </summary>
    public class LaunchInfo
    {
        public string Os { get; set; } = "";
        public string Arch { get; set; } = "";
        public string LauncherPath { get; set; } = "";
        public string JavaExecutablePath { get; set; } = "";
        public string VmOptionsFilePath { get; set; } = "";
    }

    public static class Query
    {
        public static readonly SettingsManager Settings = new();

        private static readonly Dictionary<string, string> Code2ShellDictDict = new()
        {
            { "IC", "idea" },
            { "IE", "ideaiu" },
            { "IU", "ideaiu" },
            { "PS", "PhpStorm" },
            { "WS", "webstorm" },
            { "PY", "pycharm" },
            { "PC", "pycharm" },
            { "PE", "pycharm" },
            { "RM", "rubymine" },
            { "OC", "appcode" },
            { "CL", "clion" },
            { "GO", "goland" },
            { "RD", "rider" },
            { "AI", "studio64" },
            { "RR", "rustrover" },
        };


        // 缓存产品信息列表
        private static List<ProductInfo>? _cachedProductInfos = null;

        public static string GetShellBySettings(string productCode)
        {
            var location = Settings.ShellScripts;
            location = string.IsNullOrEmpty(location)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JetBrains",
                    "Toolbox", "scripts")
                : location;

            if (!Code2ShellDictDict.TryGetValue(productCode, out var shell))
            {
                ExtensionHost.LogMessage($"JBCML: Shell for product code {productCode} is not found");
                return "";
            }

            // 首先尝试从 shell scripts 目录查找
            if (!string.IsNullOrEmpty(location))
            {
                var bestShellPath = FindBestShellFile(location, shell);
                if (!string.IsNullOrEmpty(bestShellPath))
                {
                    ExtensionHost.LogMessage($"JBCML: Found shell script at {bestShellPath}");
                    return bestShellPath.Replace("\\", "/");
                }

                ExtensionHost.LogMessage($"JBCML: Shell file for {shell} not found in {location}");
            }
            else
            {
                ExtensionHost.LogMessage("JBCML: Shell scripts location is not set");
            }

            // 如果找不到 shell 脚本，尝试从安装目录通过 product-info.json 查找二进制文件
            var installLocation = Settings.ToolsInstallLocation;
            if (!string.IsNullOrEmpty(installLocation))
            {
                var binaryPath = FindBinaryFromInstallLocationByProductInfo(productCode, installLocation);
                if (!string.IsNullOrEmpty(binaryPath))
                {
                    ExtensionHost.LogMessage($"JBCML: Found binary executable via product-info.json at {binaryPath}");
                    return binaryPath.Replace("\\", "/");
                }
            }
            else
            {
                ExtensionHost.LogMessage("JBCML: Tools install location is not set");
            }

            ExtensionHost.LogMessage($"JBCML: No executable found for product code {productCode}");
            return "";
        }

        /// <summary>
        /// 通过读取 product-info.json 文件从安装目录查找对应的二进制执行文件
        /// </summary>
        /// <param name="productCode">产品代码</param>
        /// <param name="installLocation">安装根目录</param>
        /// <returns>二进制文件的完整路径，如果找不到则返回空字符串</returns>
        private static string FindBinaryFromInstallLocationByProductInfo(string productCode, string installLocation)
        {
            if (!Directory.Exists(installLocation))
            {
                ExtensionHost.LogMessage($"JBCML: Install location does not exist: {installLocation}");
                return "";
            }

            try
            {
                var productInfos = GetAllProductInfos(installLocation);
                var matchingProduct = productInfos.FirstOrDefault(p =>
                    string.Equals(p.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));

                if (matchingProduct == null)
                {
                    ExtensionHost.LogMessage(
                        $"JBCML: No product found with code {productCode} in product-info.json files");
                    return "";
                }

                // 查找 Windows 平台的启动信息
                var windowsLaunch = matchingProduct.Launch?.FirstOrDefault(l =>
                    string.Equals(l.Os, "Windows", StringComparison.OrdinalIgnoreCase));

                if (windowsLaunch == null || string.IsNullOrEmpty(windowsLaunch.LauncherPath))
                {
                    ExtensionHost.LogMessage($"JBCML: No Windows launcher found for product {matchingProduct.Name}");
                    return "";
                }

                // 查找产品安装目录
                var productDirs = Directory.GetDirectories(installLocation)
                    .Where(dir =>
                    {
                        var productInfoPath = Path.Combine(dir, "product-info.json");
                        if (!File.Exists(productInfoPath)) return false;

                        try
                        {
                            var json = File.ReadAllText(productInfoPath);
                            var info = JsonSerializer.Deserialize<ProductInfo>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return string.Equals(info?.ProductCode, productCode, StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            return false;
                        }
                    });

                foreach (var productDir in productDirs)
                {
                    var launcherPath = Path.Combine(productDir, windowsLaunch.LauncherPath);
                    if (File.Exists(launcherPath))
                    {
                        ExtensionHost.LogMessage(
                            $"JBCML: Found launcher at {launcherPath} for product {matchingProduct.Name}");
                        return launcherPath;
                    }
                }

                ExtensionHost.LogMessage($"JBCML: Launcher file not found for product {matchingProduct.Name}");
                return "";
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage($"JBCML: Error reading product-info.json files: {ex.Message}");
                return "";
            }
        }


        /// <summary>
        /// 获取安装目录下所有产品的信息
        /// </summary>
        /// <param name="installLocation">安装根目录</param>
        /// <returns>产品信息列表</returns>
        private static List<ProductInfo> GetAllProductInfos(string installLocation)
        {
            // 使用缓存避免重复读取
            if (_cachedProductInfos != null)
            {
                return _cachedProductInfos;
            }

            var productInfos = new List<ProductInfo>();

            if (!Directory.Exists(installLocation))
            {
                return productInfos;
            }

            try
            {
                var productDirs = Directory.GetDirectories(installLocation);
                foreach (var productDir in productDirs)
                {
                    var productInfoPath = Path.Combine(productDir, "product-info.json");
                    if (!File.Exists(productInfoPath)) continue;

                    try
                    {
                        var json = File.ReadAllText(productInfoPath);
                        var productInfo = JsonSerializer.Deserialize<ProductInfo>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (productInfo != null && !string.IsNullOrEmpty(productInfo.ProductCode))
                        {
                            if (productInfo.ProductCode.Equals("IC"))
                            {
                                productInfo.Name = "IntelliJ IDEA";
                            }
                            else if (productInfo.ProductCode.Equals("IU"))
                            {
                                productInfo.Name = "IntelliJ IDEA Ultimate";
                            }
                            else if (productInfo.ProductCode.Equals("PE") || productInfo.ProductCode.Equals("PY"))
                            {
                                productInfo.Name = "PyCharm";
                            }
                            else if ( productInfo.ProductCode.Equals("PC"))
                            {
                                productInfo.Name = "PyCharm Community";
                            }

                            productInfos.Add(productInfo);
                            ExtensionHost.LogMessage(
                                $"JBCML: Loaded product info for {productInfo.Name} (Code: {productInfo.ProductCode})");
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtensionHost.LogMessage(
                            $"JBCML: Error parsing product-info.json in {productDir}: {ex.Message}");
                    }
                }

                // 缓存结果
                _cachedProductInfos = productInfos;
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage($"JBCML: Error scanning install location {installLocation}: {ex.Message}");
            }

            return productInfos;
        }

        /// <summary>
        /// 清除产品信息缓存
        /// </summary>
        public static void ClearProductInfoCache()
        {
            _cachedProductInfos = null;
            ExtensionHost.LogMessage("JBCML: Product info cache cleared");
        }

        /// <summary>
        /// 查找最佳的shell文件，优先选择带最大数字后缀的版本
        /// </summary>
        /// <param name="location">shell脚本目录</param>
        /// <param name="shell">基础shell名称</param>
        /// <returns>最佳shell文件的完整路径</returns>
        public static string FindBestShellFile(string location, string shell)
        {
            if (!Directory.Exists(location))
            {
                return "";
            }

            // 获取所有可能的shell文件（包括.cmd和.bat扩展名）
            var possibleExtensions = new[] { ".cmd", ".bat", "" };
            var candidateFiles = new List<(string path, int number)>();

            foreach (var extension in possibleExtensions)
            {
                // 检查基础文件（如 idea.cmd）
                var basePath = Path.Combine(location, shell + extension);
                if (File.Exists(basePath))
                {
                    candidateFiles.Add((basePath, 0)); // 基础文件优先级为0
                }

                // 检查带数字后缀的文件（如 idea1.cmd, idea2.cmd）
                for (int i = 1; i <= 10; i++)
                {
                    var numberedPath = Path.Combine(location, shell + i + extension);
                    if (File.Exists(numberedPath))
                    {
                        candidateFiles.Add((numberedPath, i));
                    }
                }
            }

            if (!candidateFiles.Any())
            {
                return "";
            }

            // 选择数字最大的文件，如果有多个相同数字的文件，优先选择.cmd扩展名
            var bestFile = candidateFiles
                .OrderByDescending(f => f.number)
                .ThenBy(f => Path.GetExtension(f.path) == ".cmd" ? 0 : 1)
                .First();

            return bestFile.path;
        }

        public static List<RecentProject> SearchRecentProjectsBySettings()
        {
            var projects = new List<RecentProject>();
            var location = Settings.SystemConfigLogsLocation;
            if (string.IsNullOrEmpty(location))
            {
                return projects;
            }

            foreach (var path in SearchRecentProjectXml(location))
            {
                projects.AddRange(RecentProjectsParser.ParseFromFile(path));
            }

            return projects;
        }

        public static List<string> SearchRecentProjectXml(string directPath)
        {
            var projects = new List<string>();

            if (Directory.Exists(directPath))
            {
                foreach (var dir in Directory.GetDirectories(directPath))
                {
                    var productName = Path.GetFileName(dir);
                    var recentProjectsPath = GetRecentProjectsPath(dir, productName);

                    if (string.IsNullOrEmpty(recentProjectsPath) || !File.Exists(recentProjectsPath)) continue;
                    projects.Add(recentProjectsPath);
                }
            }

            return projects;
        }

        private static string GetRecentProjectsPath(string dir, string productName)
        {
            bool isRider = productName.ToLower().Contains("rider");
            string fileName = isRider ? "recentSolutions.xml" : "recentProjects.xml";
            return Path.Combine(dir, "config", "options", fileName);
        }


        /// <summary>
        /// 获取已安装的产品信息列表（公共方法）
        /// </summary>
        /// <returns>已安装的产品信息列表</returns>
        public static List<ProductInfo> GetInstalledProducts()
        {
            var installLocation = Settings.ToolsInstallLocation;
            if (string.IsNullOrEmpty(installLocation))
            {
                ExtensionHost.LogMessage("JBCML: Tools install location is not set");
                return new List<ProductInfo>();
            }

            return GetAllProductInfos(installLocation);
        }

        /// <summary>
        /// 根据产品安装目录获取图标路径
        /// </summary>
        /// <param name="productInfo">产品信息</param>
        /// <param name="installLocation">安装根目录</param>
        /// <returns>图标的完整路径，如果找不到则返回空字符串</returns>
        public static string GetProductIconPath(ProductInfo productInfo, string installLocation)
        {
            if (productInfo == null || string.IsNullOrEmpty(productInfo.SvgIconPath))
            {
                return "";
            }

            try
            {
                // 查找产品安装目录
                var productDirs = Directory.GetDirectories(installLocation)
                    .Where(dir =>
                    {
                        var productInfoPath = Path.Combine(dir, "product-info.json");
                        if (!File.Exists(productInfoPath)) return false;

                        try
                        {
                            var json = File.ReadAllText(productInfoPath);
                            var info = JsonSerializer.Deserialize<ProductInfo>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return string.Equals(info?.ProductCode, productInfo.ProductCode,
                                StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            return false;
                        }
                    });

                foreach (var productDir in productDirs)
                {
                    var iconPath = Path.Combine(productDir, productInfo.SvgIconPath);
                    if (File.Exists(iconPath))
                    {
                        return iconPath;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage($"JBCML: Error getting icon path for {productInfo.Name}: {ex.Message}");
            }

            return "";
        }
    }
}