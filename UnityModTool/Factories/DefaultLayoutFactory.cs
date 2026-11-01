namespace UnityModTool.Factories;

using Dock.Model.Mvvm;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Controls;
using Dock.Avalonia.Controls;
using System.Collections.Generic;
using UnityModTool.AssetHandling;
using UnityModTool.ViewModels;

public class DefaultLayoutFactory : Factory
{
    private readonly AssetWorkspace _assetWorkspace;

    public DefaultLayoutFactory(AssetWorkspace assetWorkspace)
    {
        _assetWorkspace = assetWorkspace;
    }

    public override IRootDock CreateLayout()
    {
        var quickAccessTool = new Tool { Id = "QuickAccess", Title = "Quick Access" };
        var filesTool = new OpenFilesViewModel(_assetWorkspace) { Id = "Files", Title = "Open Files" };
        var previewTool = new Tool { Id = "Preview", Title = "Visual Preview" };
        var jsonTool = new Tool { Id = "Json", Title = "JSON Preview" };
        var hierarchyTool = new SceneHierarchyViewModel(_assetWorkspace, filesTool) { Id = "Hierarchy", Title = "Scene Hierarchy" };
        var propertiesTool = new ComponentPropertiesViewModel(_assetWorkspace, hierarchyTool) { Id = "Properties", Title = "Properties" };

        var hierarchyToolDock = CreateToolDock();
        hierarchyToolDock.VisibleDockables = CreateList<IDockable>(hierarchyTool);
        hierarchyToolDock.ActiveDockable = hierarchyTool;

        var quickAccessToolDock = CreateToolDock();
        quickAccessToolDock.VisibleDockables = CreateList<IDockable>(quickAccessTool);
        quickAccessToolDock.ActiveDockable = quickAccessTool;

        var filesToolDock = CreateToolDock();
        filesToolDock.VisibleDockables = CreateList<IDockable>(filesTool);
        filesToolDock.ActiveDockable = filesTool;

        var leftDock = CreateProportionalDock();
        leftDock.VisibleDockables = CreateList<IDockable>(hierarchyToolDock, new ProportionalDockSplitter(), quickAccessToolDock, new ProportionalDockSplitter(), filesToolDock);
        leftDock.Orientation = Orientation.Vertical;
        leftDock.Proportion = 0.2;

        var middleDock = CreateToolDock();
        middleDock.VisibleDockables = CreateList<IDockable>(propertiesTool);
        middleDock.ActiveDockable = propertiesTool;
        middleDock.Proportion = 0.6;

        var previewToolDock = CreateToolDock();
        previewToolDock.VisibleDockables = CreateList<IDockable>(previewTool);
        previewToolDock.ActiveDockable = previewTool;

        var jsonToolDock = CreateToolDock();
        jsonToolDock.VisibleDockables = CreateList<IDockable>(jsonTool);
        jsonToolDock.ActiveDockable = jsonTool;

        var rightDock = CreateProportionalDock();
        rightDock.VisibleDockables = CreateList<IDockable>(previewToolDock, new ProportionalDockSplitter(), jsonToolDock);
        rightDock.Orientation = Orientation.Vertical;
        rightDock.Proportion = 0.2;

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