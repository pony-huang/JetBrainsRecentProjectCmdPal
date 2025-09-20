using System.IO;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Commands;

public partial class RunAsCommand : InvokableCommand
{
    private readonly string _fullPath;
    private readonly string _arguments;

    internal RunAsCommand(string fullname, string arguments)
    {
        _fullPath = fullname;
        _arguments = arguments;
        Name = Resources.msg_run_as;
        Icon = new("\uE7EE");
    }

    public override CommandResult Invoke()
    {
        string msg = string.Empty;
        if (ShellHelper.OpenInShell(_fullPath, ref msg, _arguments))
        {
            return CommandResult.Dismiss();
        }
        else
        {
            ExtensionHost.ShowStatus(new StatusMessage() { Message = $"{Resources.run_as_exception}\n{_fullPath}\n{msg}", State = MessageState.Error }, StatusContext.Page);
            return CommandResult.KeepOpen();
        }
    }
}