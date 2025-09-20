using JetBrainsRecentProjectCmdPal.Helper;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Commands;

public class OpenExplorerCommand : InvokableCommand
{
    private readonly string _fullPath;

    public OpenExplorerCommand(string fullname)
    {
        _fullPath = fullname;
        Name = "";
        Icon = new("\uE838");
    }

    public override CommandResult Invoke()
    {
        string msg = string.Empty;
        if (ShellHelper.OpenInShell("explorer.exe", ref msg, $"/select,\"{_fullPath}\""))
        {
            return CommandResult.Dismiss();
        }
        else
        {
            ExtensionHost.ShowStatus(
                new StatusMessage()
                    { Message = $"OpenExplorerCommand:\n{_fullPath}\n{msg}", State = MessageState.Error },
                StatusContext.Page);
            return CommandResult.KeepOpen();
        }
    }
}