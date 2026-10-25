namespace UnityModTool.Factories;

using Dock.Model.Mvvm;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Controls;
using Dock.Avalonia.Controls;
using System.Collections.Generic;

public class DefaultLayoutFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        var hierarchyTool = new Tool { Id = "Hierarchy", Title = "Scene Hierarchy" };
        var filesTool = new Tool { Id = "Files", Title = "Open Files" };
        var propertiesTool = new Tool { Id = "Properties", Title = "Properties" };
        var previewTool = new Tool { Id = "Preview", Title = "Visual Preview" };
        var jsonTool = new Tool { Id = "Json", Title = "JSON Preview" };

        var hierarchyToolDock = CreateToolDock();
        hierarchyToolDock.VisibleDockables = CreateList<IDockable>(hierarchyTool);
        hierarchyToolDock.ActiveDockable = hierarchyTool;

        var filesToolDock = CreateToolDock();
        filesToolDock.VisibleDockables = CreateList<IDockable>(filesTool);
        filesToolDock.ActiveDockable = filesTool;

        var leftDock = CreateProportionalDock();
        leftDock.VisibleDockables = CreateList<IDockable>(hierarchyToolDock, new ProportionalDockSplitter(), filesToolDock);
        leftDock.Orientation = Orientation.Vertical;
        leftDock.Proportion = 0.25;

        var middleDock = CreateToolDock();
        middleDock.VisibleDockables = CreateList<IDockable>(propertiesTool);
        middleDock.ActiveDockable = propertiesTool;
        middleDock.Proportion = 0.35;

        var previewToolDock = CreateToolDock();
        previewToolDock.VisibleDockables = CreateList<IDockable>(previewTool);
        previewToolDock.ActiveDockable = previewTool;

        var jsonToolDock = CreateToolDock();
        jsonToolDock.VisibleDockables = CreateList<IDockable>(jsonTool);
        jsonToolDock.ActiveDockable = jsonTool;

        var rightDock = CreateProportionalDock();
        rightDock.VisibleDockables = CreateList<IDockable>(previewToolDock, new ProportionalDockSplitter(), jsonToolDock);
        rightDock.Orientation = Orientation.Vertical;
        rightDock.Proportion = 0.40;

        var mainLayout = CreateProportionalDock();
        mainLayout.Orientation = Orientation.Horizontal;
        mainLayout.VisibleDockables = CreateList<IDockable>(leftDock, new ProportionalDockSplitter(), middleDock, new ProportionalDockSplitter(), rightDock);

        var root = CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = CreateList<IDockable>(mainLayout);
        root.DefaultDockable = mainLayout;
        
        HostWindowLocator = new Dictionary<string, System.Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        return root;
    }
}