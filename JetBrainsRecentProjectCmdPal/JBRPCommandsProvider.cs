// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal;

public partial class JBRPCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public JBRPCommandsProvider()
    {
        Id = "JetBrainsRecentProject";
        DisplayName = "JetBrainsRecentProject";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands =
        [
            new ListItem(new JetBrainsTopLevelPage())
        ];
        Settings = Search.Settings.Settings;
    }
    
    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
