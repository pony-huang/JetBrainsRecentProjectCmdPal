// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Models;
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
    
    private string ProductVersion { get; set; }
    
    private string ProductBuildNumber { get; set; }
    
    public string ProductVendor { get; set; }
    
    
    /// <summary>
    /// Initializes a new instance of the JetBrainsProductPage class for a specific JetBrains product.
    /// </summary>
    /// <param name="product">Product information for the JetBrains product</param
    public JetBrainsProductPage(ProductInfo product)
    {
        Icon = new IconInfo(product.AbsoluteSvgIconPath);
        var productName = JetBrainsHelper.GetProductName(product.ProductCode, product.Name);
        Title = productName;
        Name = productName;
        ProductCode = product.ProductCode;
        ProductIcon = new IconInfo(product.AbsoluteSvgIconPath);
        PlaceholderText = string.Format(CultureInfo.InvariantCulture, Resources.search_recent_projects_placeholder, productName);
        ProductVersion = product.Version;
        ProductBuildNumber = product.BuildNumber;
        ProductVendor = product.ProductVendor;
    }

    /// <summary>
    /// This method filters projects based on the product code, build number, and product code.
    /// </summary>
    /// <param name="allProjects">Complete list of recent projects from all JetBrains products</param>
    /// <returns>Filtered list of projects for the specific product</returns>
    protected override List<ProjectItem> FilterByProduct(List<ProjectItem> allProjects)
    {
        return allProjects.Where(project =>
                string.Equals(project.Code, ProductCode, StringComparison.OrdinalIgnoreCase)
                && project.ProjectBuild.Contains(ProductBuildNumber)
                )
            .ToList();
    }
}