using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using UnityModTool.AssetHandling;
using static UnityModTool.AssetHandling.AssetFileReader;

namespace UnityModTool.ViewModels;

public sealed partial class ComponentPropertiesViewModel : Tool
{
    private readonly AssetWorkspace _assetWorkspace;
    private readonly SceneHierarchyViewModel _sceneHierarchyViewModel;

    public ObservableCollection<PropertyViewModel> Properties { get; } = [];

    [ObservableProperty]
    private ComponentViewModel? selectedComponent;

    public ComponentPropertiesViewModel(AssetWorkspace assetWorkspace, SceneHierarchyViewModel sceneHierarchyViewModel)
    {
        _assetWorkspace = assetWorkspace;
        _sceneHierarchyViewModel = sceneHierarchyViewModel;

        _sceneHierarchyViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SceneHierarchyViewModel.SelectedComponent))
            {
                SelectedComponent = _sceneHierarchyViewModel.SelectedComponent;
                RefreshProperties();
            }
        };

        SelectedComponent = _sceneHierarchyViewModel.SelectedComponent;
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        Properties.Clear();

        if (SelectedComponent is { } component
            && _sceneHierarchyViewModel.SelectedFile is { } file)
        {
            foreach (var property in GetProperties(
                         _assetWorkspace.Manager,
                         file.AssetsFileInstance,
                         component))
                Properties.Add(property);
        }
    }
}
