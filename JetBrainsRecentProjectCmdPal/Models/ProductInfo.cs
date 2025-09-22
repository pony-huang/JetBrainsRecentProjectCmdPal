using System;

namespace JetBrainsRecentProjectCmdPal.Models
{
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

    public class LaunchInfo
    {
        public string Os { get; set; } = "";
        public string Arch { get; set; } = "";
        public string LauncherPath { get; set; } = "";
        public string JavaExecutablePath { get; set; } = "";
        public string VmOptionsFilePath { get; set; } = "";
    }
}