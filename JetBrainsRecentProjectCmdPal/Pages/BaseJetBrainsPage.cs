using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.System;
using JetBrainsRecentProjectCmdPal.Commands;
using JetBrainsRecentProjectCmdPal.Helper;
using JetBrainsRecentProjectCmdPal.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JetBrainsRecentProjectCmdPal.Pages;

public abstract class BaseJetBrainsPage : DynamicListPage, IDisposable, IFallbackHandler
{
    protected readonly List<IListItem> _results = [];
    protected readonly Lock _lock = new();
    protected CancellationTokenSource _cts = new();

    protected BaseJetBrainsPage()
    {
        ShowDetails = true;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        UpdateQuery(newSearch);
    }

    public override IListItem[] GetItems() => [.. _results];

    public void Dispose()
    {
        _cts.Cancel();
        GC.SuppressFinalize(this);
    }

    public void UpdateQuery(string query)
    {
        IsLoading = true;
        CancellationTokenSource cts;
        lock (_lock)
        {
            _cts.Cancel();
            _cts = new();
            cts = _cts;
        }

        try
        {
            _results.Clear();

            // 1. 获取所有最近项目
            var allProjects = Query.SearchRecentProjectsBySettings();

            if (cts.IsCancellationRequested)
                return;

            // 2. 应用产品代码过滤（子类实现）
            var filteredByProduct = FilterByProduct(allProjects);

            if (cts.IsCancellationRequested)
                return;

            // 3. 根据query参数过滤项目名称（如果query不为空）
            var finalProjects = string.IsNullOrWhiteSpace(query)
                ? filteredByProduct
                : filteredByProduct.Where(project =>
                        project.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        project.FrameTitle.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        project.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (cts.IsCancellationRequested)
                return;

            // 4. 按最近使用时间排序
            finalProjects = finalProjects
                .OrderByDescending(p => Math.Max(p.ActivationTimestamp, p.ProjectOpenTimestamp))
                .ToList();

            // 5. 创建IListItem实例
            foreach (var project in finalProjects)
            {
                if (cts.IsCancellationRequested)
                    return;

                _results.Add(CreateListItemFromProject(project));
            }

            IsLoading = false;
            RaiseItemsChanged(_results.Count);
        }
        catch (Exception)
        {
            IsLoading = false;
            RaiseItemsChanged(_results.Count);
        }
    }

    /// <summary>
    /// 子类实现此方法来过滤特定产品的项目
    /// </summary>
    /// <param name="allProjects">所有项目</param>
    /// <returns>过滤后的项目列表</returns>
    protected abstract List<RecentProject> FilterByProduct(List<RecentProject> allProjects);

    /// <summary>
    /// 子类实现此方法来获取项目图标
    /// </summary>
    /// <param name="project">项目信息</param>
    /// <returns>项目图标</returns>
    protected abstract IconInfo GetProjectIcon(RecentProject project);

    protected IListItem CreateListItemFromProject(RecentProject project)
    {
        string shell = Query.GetShellBySettings(project.ProductionCode);

        InvokableCommand primaryCommand;
        var arg = $"\"{project.Key}\"";

        if (shell == null)
        {
            primaryCommand = new NoOpCommand();
        }
        else if (Query.Settings.RunAsAdministrator)
        {
            primaryCommand = new RunAsAsAdminCommand(shell, arg);
        }
        else
        {
            primaryCommand = new RunAsCommand(shell, arg);
        }

        var listItem = new ListItem(primaryCommand)
        {
            Title = project.Name ?? string.Empty,
            Subtitle = project.Key ?? string.Empty,
            Icon = GetProjectIcon(project),
            Details = CreateProjectDetails(project),
            MoreCommands = CreateMoreCommand(project),
        };

        return listItem;
    }

    protected static IContextItem[] CreateMoreCommand(RecentProject project)
    {
        var fullPath = $"\"{project.Key}\"";
        var shell = Query.GetShellBySettings(project.ProductionCode);
        var moreCommands = new List<IContextItem>
        {
            new CommandContextItem(new CopyPathCommand(fullPath))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(true, true, false, false, (int)VirtualKey.C, 0)
            },
        };
        if (Query.Settings.RunAsAdministrator)
        {
            moreCommands.Add(new CommandContextItem(new RunAsCommand(shell, fullPath))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(false, true, false, false, (int)VirtualKey.Enter, 0)
            });
        }
        else
        {
            moreCommands.Add(new CommandContextItem(new RunAsAsAdminCommand(shell, fullPath))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(false, true, false, false, (int)VirtualKey.Enter, 0)
            });
        }

        return moreCommands.ToArray();
    }

    protected Details CreateProjectDetails(RecentProject project)
    {
        var metadata = new List<IDetailsElement>();

        // Project Location
        metadata.Add(new DetailsElement
        {
            Key = Resources.project_path_label,
            Data = new DetailsLink
            {
                Text = project.Key
            }
        });

        // -----
        metadata.Add(new DetailsElement
        {
            Data = new DetailsSeparator()
        });

        // JetBrains Product
        if (!string.IsNullOrEmpty(project.ProductionCode))
        {
            metadata.Add(new DetailsElement
            {
                Key = "Product Code",
                Data = new DetailsTags
                {
                    Tags =
                    [
                        new Tag(project.ProductionCode)
                        {
                            Foreground = ColorHelpers.FromRgb(51, 51, 51),
                            Background = ColorHelpers.FromRgb(224, 204, 179)
                        }
                    ]
                }
            });
        }

        // 项目状态
        metadata.Add(new DetailsElement
        {
            Key = "Project Status",
            Data = new DetailsTags
            {
                Tags =
                [
                    new Tag(project.IsOpened ? "Opened" : "Closed")
                    {
                        Foreground = ColorHelpers.FromRgb(255, 255, 255),
                        Background = project.IsOpened
                            ? ColorHelpers.FromRgb(76, 175, 80) // 绿色
                            : ColorHelpers.FromRgb(158, 158, 158) // 灰色
                    }
                ]
            }
        });

        // ActivationTimestamp
        if (project.ActivationTimestamp > 0)
        {
            var activationTime = DateTimeOffset.FromUnixTimeMilliseconds(project.ActivationTimestamp);
            metadata.Add(new DetailsElement
            {
                Key = "Lasted Open Time",
                Data = new DetailsLink
                {
                    Text = activationTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
                }
            });
        }

        // ProjectOpenTimestamp
        if (project.ProjectOpenTimestamp > 0)
        {
            var openTime = DateTimeOffset.FromUnixTimeMilliseconds(project.ProjectOpenTimestamp);
            metadata.Add(new DetailsElement
            {
                Key = "Open Time",
                Data = new DetailsLink
                {
                    Text = openTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
                }
            });
        }

        // Build
        if (!string.IsNullOrEmpty(project.Build))
        {
            metadata.Add(new DetailsElement
            {
                Key = "Build",
                Data = new DetailsLink
                {
                    Text = project.Build
                }
            });
        }

        // Project WorkspaceId
        if (!string.IsNullOrEmpty(project.ProjectWorkspaceId))
        {
            metadata.Add(new DetailsElement
            {
                Key = "Project WorkspaceId",
                Data = new DetailsLink
                {
                    Text = project.ProjectWorkspaceId
                }
            });
        }

        // Project Exists
        var projectExists = Directory.Exists(project.Key) || File.Exists(project.Key);
        if (!projectExists)
        {
            metadata.Add(new DetailsElement
            {
                Data = new DetailsSeparator()
            });

            metadata.Add(new DetailsElement
            {
                Key = "Alarm",
                Data = new DetailsTags
                {
                    Tags =
                    [
                        new Tag("Project Not Found")
                        {
                            Foreground = ColorHelpers.FromRgb(255, 255, 255),
                            Background = ColorHelpers.FromRgb(244, 67, 54) // 红色
                        }
                    ]
                }
            });
        }

        return new Details
        {
            HeroImage = GetProjectIcon(project),
            Metadata = metadata.ToArray()
        };
    }
}