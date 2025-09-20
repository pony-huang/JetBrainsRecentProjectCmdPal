using System.Linq;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages;

public sealed class JetBrainsTopLevelPage : JetBrainsAllProductPage
{
    public JetBrainsTopLevelPage()
    {
        Icon = Icons.JetBrainsIcon;
        Title = Resources.jetbrains_recent_projects_title;
        Name = Resources.jetbrains_recent_projects_name;
    }

    public override IListItem[] GetItems()
    {
        if (Query.Settings.JetBrainsProductArchive)
        {
            // 动态获取已安装的产品
            var installedProducts = Query.GetInstalledProducts();

            if (installedProducts.Any())
            {
                // 根据实际安装的产品生成列表项
                return installedProducts
                    .Where(product => !string.IsNullOrEmpty(product.ProductCode)) // 确保有产品代码
                    .OrderBy(product => product.Name) // 按名称排序
                    .Select(product =>
                    {
                        // 获取产品图标，优先使用产品自带的图标
                        var icon = Icons.GetIconForProductInfo(product);

                        return new ListItem(new JetBrainsProductPage(
                            product.Name,
                            product.Name,
                            product.ProductCode,
                            icon));
                    })
                    .ToArray();
            }
            else
            {
                return
                [
                    new ListItem(new NoOpCommand()) { Title = "Please settings Tool install location" }
                ];
            }
        }
        else
        {
            return base.GetItems();
        }
    }
}