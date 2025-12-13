using System.IO;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Settings;

public class SettingsManager : JsonSettingsManager
{
    private static string Namespaced(string propertyName) => $"{propertyName}";

    private readonly ToggleSetting _runAsAdministrator = new(
        Namespaced(nameof(RunAsAdministrator)),
        Resources.settings_run_as_administrator,
        "",
        false);


    private readonly ToggleSetting _jetbrainsProductArchive = new(
        Namespaced(nameof(JetBrainsProductArchive)),
        Resources.settings_jetbrains_product_archive,
        "",
        false);


    private readonly ToggleSetting _jetbrainsCustomOption = new(
        Namespaced(nameof(JetBrainsCustomOption)),
        Resources.settings_custom_options,
        Resources.settings_custom_options_description,
        false);


    private readonly TextSetting _toolsInstallLocation = new(
        Namespaced(nameof(ToolsInstallLocation)),
        Resources.settings_tools_install_location,
        Resources.settings_tools_install_location,
        "");


    private readonly TextSetting _systemConfigLogslLocation = new(
        Namespaced(nameof(SystemConfigLogsLocation)),
        Resources.settings_systemconfiglogs_directory,
        Resources.settings_systemconfiglogs_directory,
        "");

    public bool RunAsAdministrator => _runAsAdministrator.Value;
    public bool JetBrainsProductArchive => _jetbrainsProductArchive.Value;
    public bool JetBrainsCustomOption => _jetbrainsCustomOption.Value;
    public string ToolsInstallLocation => _toolsInstallLocation.Value ?? string.Empty;
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
        Settings.Add(_jetbrainsCustomOption);
        Settings.Add(_toolsInstallLocation);
        Settings.Add(_systemConfigLogslLocation);

        base.LoadSettings();
        Settings.SettingsChanged += (s, a) => SaveSettings();
    }
}