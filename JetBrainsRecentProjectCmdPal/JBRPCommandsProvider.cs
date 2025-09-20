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
        // _commands = GetItems();
        _commands = new[]
        {
            new ListItem(new JetBrainsTopLevelPage())
        };
        Settings = Query.Settings.Settings;
    }
    
    // public  ICommandItem[] GetItems()
    // {
    //     if (Query.Settings.JetBrainsProductArchive)
    //     {
    //         // 动态获取已安装的产品
    //         var installedProducts = Query.GetInstalledProducts();
    //
    //         if (installedProducts.Any())
    //         {
    //             // 根据实际安装的产品生成列表项
    //             return installedProducts
    //                 .Where(product => !string.IsNullOrEmpty(product.ProductCode)) // 确保有产品代码
    //                 .OrderBy(product => product.Name) // 按名称排序
    //                 .Select(product =>
    //                 {
    //                     // 获取产品图标，优先使用产品自带的图标
    //                     var icon = Icons.GetIconForProductInfo(product);
    //
    //                     return new ListItem(new JetBrainsProductPage(
    //                         product.Name,
    //                         product.Name,
    //                         product.ProductCode,
    //                         icon));
    //                 })
    //                 .ToArray();
    //         }
    //         else
    //         {
    //             return
    //             [
    //                 new ListItem(new NoOpCommand()) { Title = "Please settings Tool install location" }
    //             ];
    //         }
    //     }
    //     else
    //     {
    //         return
    //         [
    //             new ListItem(new JetBrainsAllProductPage())
    //         ];
    //     }
    // }
    //
    // public override ICommandItem[] TopLevelCommands()
    // {
    //     return GetItems();
    // }
    
    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
