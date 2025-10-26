using System;
using System.Linq;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;

namespace JetBrainsRecentProjectCmdPal.Pages;

public sealed partial class JetBrainsTopLevelPage : JetBrainsAllProductPage
{
    /// <summary>
    /// Initializes a new instance of the JetBrainsTopLevelPage class.
    /// Sets up the default icon, title, and name for the top-level entry point.
    /// </summary>
    public JetBrainsTopLevelPage()
    {
        Icon = IconHelper.DefaultIconInfo;
        Title = Resources.jetbrains_recent_projects_title;
        Name = Resources.jetbrains_recent_projects_name;
    }

    /// <summary>
    /// Gets the items to display in the command palette.
    /// The behavior depends on the JetBrainsProductArchive setting:
    /// - If enabled: Shows a list of installed JetBrains products as separate pages
    /// - If disabled: Shows all recent projects directly (inherited from base class)
    /// </summary>
    /// <returns>Array of list items representing either product categories or recent projects</returns>
    public override IListItem[] GetItems()
    {
        // Check if product archive mode is enabled (categorized view by product)
        if (Search.Settings.JetBrainsProductArchive)
        {
            var installedProducts = Search.GetInstalledProducts();

            // If JetBrains products are installed, create product-specific pages
            if (installedProducts.Count != 0)
            {
                var items = installedProducts
                    .Where(product => !string.IsNullOrEmpty(product.ProductCode))
                    .OrderBy(product => product.Name)
                    .Select(product =>
                    {
                        // Get the appropriate icon for each JetBrains product
                        var icon = IconHelper.GetIconForProductInfo(product);

                        // Create a list item that navigates to a product-specific page
                        var productName = JetBrainsHelper.GetProductName(product);
                        var item = new ListItem(new JetBrainsProductPage(
                            productName,
                            productName,
                            product.ProductCode,
                            icon));

                        item.Subtitle = product.Version;

                        return item;
                    })
                    .ToArray();
                return items.ToArray<IListItem>();
            }

            // No JetBrains products found
            return
            [
                new ListItem(new NoOpCommand()) { Title = "No JetBrains products found" }
            ];
        }

        // Product archive mode is disabled - show all recent projects directly
        return base.GetItems();
    }
}
