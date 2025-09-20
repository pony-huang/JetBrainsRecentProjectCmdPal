using System.IO;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Settings
{
    public class SettingsManager : JsonSettingsManager
    {
        private static string Namespaced(string propertyName) => $"{propertyName}";

        private readonly ToggleSetting _runAsAdministrator = new(
            Namespaced(nameof(RunAsAdministrator)),
            Resources.settings_run_as_administrator,
            "",
            false);

        public bool RunAsAdministrator => _runAsAdministrator.Value;
        
        
        private readonly ToggleSetting _jetbrainsProductArchive = new(
            Namespaced(nameof(JetBrainsProductArchive)),
            Resources.settings_jetbrains_product_archive,
            "",
            false);

        public bool JetBrainsProductArchive => _jetbrainsProductArchive.Value;
        
        private readonly TextSetting _shellScripts = new(
            Namespaced(nameof(ShellScripts)),
            Resources.settings_shell_script_location,
            Resources.settings_shell_script_location,
            "");

        public string ShellScripts => _shellScripts.Value ?? string.Empty;

        private readonly TextSetting _toolsInstallLocation = new(
            Namespaced(nameof(ToolsInstallLocation)),
            Resources.settings_tools_install_location,
            Resources.settings_tools_install_location,
            "");

        public string ToolsInstallLocation => _toolsInstallLocation.Value ?? string.Empty;
        
        
        private readonly TextSetting _systemConfigLogslLocation = new(
            Namespaced(nameof(SystemConfigLogsLocation)),
            Resources.settings_systemconfiglogs_directory,
            Resources.settings_systemconfiglogs_directory,
            "");

        public string SystemConfigLogsLocation => _systemConfigLogslLocation.Value ?? string.Empty;

        private static string SettingsJsonPath()
        {
            var dir = Utilities.BaseSettingsPath("Microsoft.CmdPal");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "JetBrainsRecentProjectCmdPal.json");
        }

        public SettingsManager()
        {
            FilePath = SettingsJsonPath();
            Settings.Add(_runAsAdministrator);
            Settings.Add(_jetbrainsProductArchive);
            Settings.Add(_shellScripts);
            Settings.Add(_toolsInstallLocation);
            Settings.Add(_systemConfigLogslLocation);
            
            base.LoadSettings();
            Settings.SettingsChanged += (s, a) => SaveSettings();
        }
    }
}