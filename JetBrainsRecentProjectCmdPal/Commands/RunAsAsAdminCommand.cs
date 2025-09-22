using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Commands
{
    public partial class RunAsAsAdminCommand : InvokableCommand
    {
        private readonly string _fullPath;
        private readonly string _arguments;

        internal RunAsAsAdminCommand(string fullname, string arguments) 
        {
            _fullPath = fullname;
            _arguments = arguments;
            Name = Resources.msg_run_as_administrator;
            Icon = new IconInfo("\uE7EF");
        }

        public override CommandResult Invoke()
        {
            string msg = string.Empty;
            if (ShellHelper.OpenInShell(_fullPath, ref msg, _arguments, null, "runAs"))
            {
                return CommandResult.Dismiss();
            }
            else
            {
                ExtensionHost.ShowStatus(
                    new StatusMessage() 
                    { 
                        Message = $"{Resources.run_admin_exception}\n{_fullPath}\n{msg}", 
                        State = MessageState.Error 
                    }, 
                    StatusContext.Page);
                return CommandResult.KeepOpen();
            }
        }
    }
}