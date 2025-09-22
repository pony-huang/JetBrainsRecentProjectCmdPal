// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages;

/// <summary>
/// Page that displays recent projects filtered by a specific JetBrains product.
/// This page shows only projects that were opened with a particular JetBrains IDE (e.g., IntelliJ IDEA, WebStorm, etc.).
/// </summary>
public partial class JetBrainsProductPage : BaseJetBrainsPage
{
    /// <summary>
    /// Gets or sets the product code used for filtering projects (e.g., "IU" for IntelliJ IDEA Ultimate)
    /// </summary>
    private string ProductCode { get; set; }
    
    /// <summary>
    /// Gets or sets the icon information for the specific JetBrains product
    /// </summary>
    private IconInfo ProductIcon { get; set; }

    /// <summary>
    /// Initializes a new instance of the JetBrainsProductPage class for a specific JetBrains product.
    /// </summary>
    /// <param name="name">The internal name identifier for the page</param>
    /// <param name="title">The display title shown in the command palette</param>
    /// <param name="productCode">The JetBrains product code used for filtering (e.g., "IU", "WS", "PS")</param>
    /// <param name="icon">The icon information representing the specific JetBrains product</param>
    public JetBrainsProductPage(String name, String title, String productCode, IconInfo icon)
    {
        Icon = icon;
        Title = title;
        Name = name;
        ProductCode = productCode;
        ProductIcon = icon;
        PlaceholderText = string.Format(Resources.search_recent_projects_placeholder, title);
    }

    /// <summary>
    /// Implements the product filtering logic for this specific product page.
    /// Filters the complete project list to show only projects that match the configured product code.
    /// </summary>
    /// <param name="allProjects">Complete list of recent projects from all JetBrains products</param>
    /// <returns>Filtered list containing only projects that were opened with the specific JetBrains product</returns>
    protected override List<RecentProject> FilterByProduct(List<RecentProject> allProjects)
    {
        return allProjects.Where(project =>
                string.Equals(project.ProductionCode, ProductCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}