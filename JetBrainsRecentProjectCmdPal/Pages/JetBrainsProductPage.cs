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

public  partial class JetBrainsProductPage : BaseJetBrainsPage
{
    private string ProductCode { get; set; }
    private IconInfo ProductIcon { get; set; }

    public JetBrainsProductPage(String name, String title, String productCode, IconInfo icon)
    {
        Icon = icon;
        Title = title;
        Name = name;
        ProductCode = productCode;
        ProductIcon = icon;
        PlaceholderText = string.Format(Resources.search_recent_projects_placeholder, title);
    }

    protected override List<RecentProject> FilterByProduct(List<RecentProject> allProjects)
    {
        return allProjects.Where(project =>
                string.Equals(project.ProductionCode, ProductCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    protected override IconInfo GetProjectIcon(RecentProject project)
    {
        return ProductIcon;
    }
}