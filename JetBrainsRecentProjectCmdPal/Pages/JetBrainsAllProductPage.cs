using System.Collections.Generic;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages;

public  partial class JetBrainsAllProductPage : BaseJetBrainsPage
{
    public JetBrainsAllProductPage()
    {
        Title = Resources.jetbrains_recent_projects_title;
        Icon = Icons.JetBrainsIcon;
        Name = Resources.jetbrains_recent_projects_name;
        PlaceholderText = string.Format(Resources.search_recent_projects_placeholder, Title);
    }
    protected override List<RecentProject> FilterByProduct(List<RecentProject> allProjects)
    {
        return allProjects;
    }

    protected override IconInfo GetProjectIcon(RecentProject project)
    {
        return Icons.GetIconForProductCode(project.ProductionCode);
    }
}