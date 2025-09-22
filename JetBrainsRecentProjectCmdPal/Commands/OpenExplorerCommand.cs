using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Commands
{
    public class OpenExplorerCommand : InvokableCommand
    {
        private readonly string _fullPath;
        private readonly string _arguments;

        public OpenExplorerCommand(string fullname) 
        {
            _fullPath = "explorer.exe";
           _arguments = $"\"{fullname.Replace('/', '\\')}\"";
            Name = Resources.msg_open_in_explorer;
            Icon = new IconInfo("\uE838");
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
                ExtensionHost.ShowStatus(
                    new StatusMessage() 
                    { 
                        Message = $"{Resources.open_explorer_exception}\n{_fullPath}\n{msg}", 
                        State = MessageState.Error 
                    }, 
                    StatusContext.Page);
                return CommandResult.KeepOpen();
            }
        }
    }
}