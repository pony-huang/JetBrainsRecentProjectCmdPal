using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.System;
using JetBrainsRecentProjectCmdPal.Commands;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Models;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages
{
    /// <summary>
    /// Abstract base class for JetBrains project pages that provides common functionality
    /// for displaying and managing recent JetBrains projects in the command palette.
    /// Implements dynamic list functionality with search, filtering, and project management capabilities.
    /// </summary>
    public abstract class BaseJetBrainsPage : DynamicListPage, IDisposable, IFallbackHandler
    {
        /// <summary>
        /// Collection of list items representing filtered and processed recent projects
        /// </summary>
        private readonly List<IListItem> _results = [];

        /// <summary>
        /// Initializes a new instance of the BaseJetBrainsPage class
        /// and enables detailed view for project information
        /// </summary>
        protected BaseJetBrainsPage()
        {
            ShowDetails = true;
        }

        /// <summary>
        /// Handles search text updates by triggering a new query with the updated search term
        /// </summary>
        /// <param name="oldSearch">The previous search text</param>
        /// <param name="newSearch">The new search text to apply</param>
        public override void UpdateSearchText(string oldSearch, string newSearch)
        {
            UpdateQuery(newSearch);
        }

        /// <summary>
        /// Returns the current collection of filtered and processed list items
        /// </summary>
        /// <returns>Array of IListItem representing the current search results</returns>
        public override IListItem[] GetItems() => [.. _results];

        /// <summary>
        /// Disposes resources and cancels any ongoing operations
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates the search query and refreshes the project list with filtered results.
        /// This method performs the complete search pipeline: retrieval, filtering, sorting, and UI creation.
        /// </summary>
        /// <param name="query">The search query string to filter projects</param>
        public void UpdateQuery(string query)
        {
            IsLoading = true;
            try
            {
                _results.Clear();

                // Step 1: Retrieve all recent projects from JetBrains settings
                var allProjects = Search.SearchRecentProjectsBySettings();
                
                // Step 2: Apply product-specific filtering (implemented by derived classes)
                var filteredByProduct = FilterByProduct(allProjects);
                
                // Step 3: Apply search query filtering
                var finalProjects = FilterByQuery(filteredByProduct, query);
                
                // Step 4: Sort projects by relevance and recency
                finalProjects = SortProjects(finalProjects);

                // Step 5: Convert projects to UI list items
                var installedProducts = Search.GetInstalledProducts();
                var installedProductsDict = installedProducts.ToDictionary(p => p.ProductCode, p => p);
                
                foreach (var project in finalProjects)
                {
                    _results.Add(CreateListItemFromProject(project, installedProductsDict));
                }

                IsLoading = false;
                RaiseItemsChanged(_results.Count);
            }
            catch (Exception)
            {
                // Ensure loading state is cleared even on exceptions
                IsLoading = false;
                RaiseItemsChanged(_results.Count);
            }
            // if _result is empty
            if (_results.Count == 0)
            {
                _results.Add( new ListItem(new NoOpCommand()) { Title = "No recent projects found" });
            }
        }

        /// <summary>
        /// Abstract method for product-specific filtering logic.
        /// Derived classes must implement this to filter projects by their specific product criteria.
        /// </summary>
        /// <param name="allProjects">Complete list of recent projects to filter</param>
        /// <returns>Filtered list of projects matching the product criteria</returns>
        protected abstract List<RecentProject> FilterByProduct(List<RecentProject> allProjects);

        /// <summary>
        /// Filters projects based on the search query by matching against project name,
        /// frame title, and project key (path) using case-insensitive comparison.
        /// </summary>
        /// <param name="projects">List of projects to filter</param>
        /// <param name="query">Search query string</param>
        /// <returns>Filtered list of projects matching the search criteria</returns>
        private List<RecentProject> FilterByQuery(List<RecentProject> projects, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return projects;

            return projects.Where(project =>
                project.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                project.FrameTitle.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                project.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Sorts projects by priority: currently opened projects first,
        /// then by most recent activity (activation or opening timestamp).
        /// </summary>
        /// <param name="projects">List of projects to sort</param>
        /// <returns>Sorted list of projects</returns>
        private List<RecentProject> SortProjects(List<RecentProject> projects)
        {
            return projects
                .OrderByDescending(p => p.IsOpened)
                .ThenByDescending(p => Math.Max(p.ActivationTimestamp, p.ProjectOpenTimestamp))
                .ToList();
        }

        /// <summary>
        /// Creates a command palette list item from a recent project with appropriate
        /// commands, icons, and detailed information display.
        /// </summary>
        /// <param name="project">The recent project to convert</param>
        /// <param name="installedProductsDict">Dictionary of installed JetBrains products for icon resolution</param>
        /// <returns>Configured IListItem for display in the command palette</returns>
        private IListItem CreateListItemFromProject(RecentProject project, Dictionary<string, ProductInfo> installedProductsDict)
        {
            string shell = Search.GetProductBinBySettings(project.ProductionCode);
            var arg = $"\"{project.Key}\"";

            // Create primary command based on administrator settings
            InvokableCommand primaryCommand = Search.Settings.RunAsAdministrator
                ? new RunAsAsAdminCommand(shell, arg)
                : new RunAsCommand(shell, arg);

            var productInfo = installedProductsDict.GetValueOrDefault(project.ProductionCode);

            return new ListItem(primaryCommand)
            {
                Title = project.Name ?? string.Empty,
                Subtitle = project.Key ?? string.Empty,
                Icon = IconHelper.GetIconForProductInfo(productInfo),
                Details = CreateProjectDetails(project, productInfo),
                MoreCommands = CreateMoreCommand(project),
            };
        }

        /// <summary>
        /// Creates context menu commands for a project including copy path, open in explorer,
        /// and alternative run commands with keyboard shortcuts.
        /// </summary>
        /// <param name="project">The project to create commands for</param>
        /// <returns>Array of context menu items with associated keyboard shortcuts</returns>
        private static IContextItem[] CreateMoreCommand(RecentProject project)
        {
            if (string.IsNullOrEmpty(project.Key))
            {
                return [];
            }

            var projectPath = project.Key;
            var quotedPath = $"\"{projectPath}\"";
            var shell = Search.GetProductBinBySettings(project.ProductionCode);
            
            // For .sln files, open the containing directory in explorer
            var explorerOpenPath = projectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) 
                ? Path.GetDirectoryName(projectPath) ?? projectPath
                : projectPath;
            var quotedExplorerPath = $"\"{explorerOpenPath}\"";

            // Define keyboard shortcuts for context commands
            var copyShortcut = KeyChordHelpers.FromModifiers(true, true, false, false, (int)VirtualKey.C, 0);
            var explorerShortcut = KeyChordHelpers.FromModifiers(true, true, false, false, (int)VirtualKey.E, 0);
            var runShortcut = KeyChordHelpers.FromModifiers(false, true, false, false, (int)VirtualKey.Enter, 0);

            var moreCommands = new IContextItem[]
            {
                // Copy project path to clipboard (Ctrl+Shift+C)
                new CommandContextItem(new CopyPathCommand(projectPath))
                {
                    RequestedShortcut = copyShortcut
                },
                // Open project location in Windows Explorer (Ctrl+Shift+E)
                new CommandContextItem(new OpenExplorerCommand(quotedExplorerPath))
                {
                    RequestedShortcut = explorerShortcut
                },
                // Alternative run command with opposite admin privilege (Alt+Enter)
                new CommandContextItem(Search.Settings.RunAsAdministrator 
                    ? new RunAsCommand(shell, quotedPath)
                    : new RunAsAsAdminCommand(shell, quotedPath))
                {
                    RequestedShortcut = runShortcut
                }
            };

            return moreCommands;
        }

        /// <summary>
        /// Creates detailed project information display including path, status, timestamps,
        /// build information, and existence validation.
        /// </summary>
        /// <param name="project">The project to create details for</param>
        /// <param name="productInfo">Associated JetBrains product information</param>
        /// <returns>Details object containing formatted project metadata</returns>
        private Details CreateProjectDetails(RecentProject project, ProductInfo productInfo)
        {
            var metadata = new List<IDetailsElement>();
            
            metadata.Add(new DetailsElement { Data = new DetailsSeparator() });
            
            // Jetbrains Product
            metadata.Add(new DetailsElement
            {
                Key = Resources.jetbrains_product_label,
                Data = new DetailsLink { Text = JetBrainsHelper.GetProductName(productInfo)}
            });
            
            
            // Display project location/path
            metadata.Add(new DetailsElement
            {
                Key = Resources.project_path_label,
                Data = new DetailsLink { Text = project.Key }
            });


            // Note: Product code display is commented out but preserved for potential future use
            // JetBrains Product
            // if (!string.IsNullOrEmpty(project.ProductionCode))
            // {
            //     metadata.Add(new DetailsElement
            //     {
            //         Key = Resources.product_code_label,
            //         Data = new DetailsTags
            //         {
            //             Tags = [new Tag(project.ProductionCode)
            //             {
            //                 Foreground = ColorHelpers.FromRgb(51, 51, 51),
            //                 Background = ColorHelpers.FromRgb(224, 204, 179)
            //             }]
            //         }
            //     });
            // }

            // Display project status (opened/closed) with color-coded tags
            metadata.Add(new DetailsElement
            {
                Key = Resources.project_status_label,
                Data = new DetailsTags
                {
                    Tags = [new Tag(project.IsOpened ? Resources.project_status_opened : Resources.project_status_closed)
                    {
                        Foreground = ColorHelpers.FromRgb(255, 255, 255),
                        Background = project.IsOpened
                            ? ColorHelpers.FromRgb(76, 175, 80)  // Green for opened
                            : ColorHelpers.FromRgb(158, 158, 158) // Gray for closed
                    }]
                }
            });

            // Add timestamp information (last opened, project opened)
            AddTimestampDetails(metadata, project);

            // Display build information if available
            if (!string.IsNullOrEmpty(project.Build))
            {
                metadata.Add(new DetailsElement
                {
                    Key = Resources.build_label,
                    Data = new DetailsLink { Text = project.Build }
                });
            }

            // Display workspace ID if available
            if (!string.IsNullOrEmpty(project.ProjectWorkspaceId))
            {
                metadata.Add(new DetailsElement
                {
                    Key = Resources.project_workspace_id_label,
                    Data = new DetailsLink { Text = project.ProjectWorkspaceId }
                });
            }

            // Validate project existence and show warning if not found
            var projectExists = Directory.Exists(project.Key) || File.Exists(project.Key);
            if (!projectExists)
            {
                metadata.Add(new DetailsElement { Data = new DetailsSeparator() });
                metadata.Add(new DetailsElement
                {
                    Key = Resources.alarm_label,
                    Data = new DetailsTags
                    {
                        Tags = [new Tag(Resources.project_not_found)
                        {
                            Foreground = ColorHelpers.FromRgb(255, 255, 255),
                            Background = ColorHelpers.FromRgb(244, 67, 54) // Red for error
                        }]
                    }
                });
            }

            return new Details
            {
                HeroImage = IconHelper.GetIconForProductInfo(productInfo),
                Metadata = metadata.ToArray()
            };
        }

        /// <summary>
        /// Adds timestamp details (activation time and project open time) to the metadata list
        /// if the timestamps are available and valid.
        /// </summary>
        /// <param name="metadata">The metadata list to add timestamp details to</param>
        /// <param name="project">The project containing timestamp information</param>
        private void AddTimestampDetails(List<IDetailsElement> metadata, RecentProject project)
        {
            // Add last activation timestamp if available
            if (project.ActivationTimestamp > 0)
            {
                var activationTime = DateTimeOffset.FromUnixTimeMilliseconds(project.ActivationTimestamp);
                metadata.Add(new DetailsElement
                {
                    Key = Resources.lasted_open_time_label,
                    Data = new DetailsLink
                    {
                        Text = activationTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
                    }
                });
            }

            // Add project open timestamp if available
            if (project.ProjectOpenTimestamp > 0)
            {
                var openTime = DateTimeOffset.FromUnixTimeMilliseconds(project.ProjectOpenTimestamp);
                metadata.Add(new DetailsElement
                {
                    Key = Resources.open_time_label,
                    Data = new DetailsLink
                    {
                        Text = openTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
                    }
                });
            }
        }
    }
}