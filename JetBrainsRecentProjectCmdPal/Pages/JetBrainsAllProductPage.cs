using System.Collections.Generic;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages;

/// <summary>
/// Page that displays recent projects from all JetBrains products without any product-specific filtering.
/// This page shows a unified view of all recent projects across different JetBrains IDEs.
/// </summary>
public partial class JetBrainsAllProductPage : BaseJetBrainsPage
{
    /// <summary>
    /// Initializes a new instance of the JetBrainsAllProductPage class.
    /// Sets up the page title, icon, name, and placeholder text for the unified view.
    /// </summary>
    public JetBrainsAllProductPage()
    {
        Title = Resources.jetbrains_recent_projects_title;
        Icon = IconHelper.DefaultIconInfo;
        Name = Resources.jetbrains_recent_projects_name;
        PlaceholderText = string.Format(Resources.search_recent_projects_placeholder, Title);
    }

    /// <summary>
    /// Implements the product filtering logic for the all-products view.
    /// Returns all projects without any filtering since this page displays projects from all JetBrains products.
    /// </summary>
    /// <param name="allProjects">Complete list of recent projects from all JetBrains products</param>
    /// <returns>The same list of projects without any product-specific filtering applied</returns>
    protected override List<RecentProject> FilterByProduct(List<RecentProject> allProjects)
    {
        return allProjects;
    }
}