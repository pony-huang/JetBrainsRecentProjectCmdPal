using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Helper;

public  class ShellHelper
{
    /// <summary>
    /// commonly used helper to launch things in shell differently
    /// </summary>
    /// <param name="path">the thing to launch, ie "explorer.exe"</param>
    /// <param name="msg">variable used to log any exception messages</param>
    /// <param name="args">optional arguments to pass</param>
    /// <param name="dir">working directory, ie for when opening in terminal</param>
    /// <param name="verb">runAs, runAsUser, open</param>
    /// <returns>bool to indicate execution success</returns>
    public static bool OpenInShell(string path, ref string msg, string? args = null, string? dir = null,
        string verb = "")
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = path,
            Arguments = string.IsNullOrEmpty(args) ? string.Empty : args,
            UseShellExecute = true,
            Verb = verb,
            WorkingDirectory = string.IsNullOrEmpty(dir) ? string.Empty : dir,
        };
        try
        {
            ExtensionHost.LogMessage($"JBCML: Launching: {path} {args}");
            Process.Start(startInfo);
            return true;
        }
        catch (Exception e)
        {
            msg = e.Message;
            ExtensionHost.LogMessage($"JBCML: Failed to launch: {e.Message}");
            return false;
        }
    }
}