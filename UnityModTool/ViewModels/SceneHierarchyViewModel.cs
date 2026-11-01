using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using UnityModTool.AssetHandling;
using static UnityModTool.AssetHandling.AssetFileReader;

namespace UnityModTool.ViewModels;

public sealed partial class SceneHierarchyViewModel : Tool
{
    private readonly AssetWorkspace _assetWorkspace;
    private OpenFilesViewModel _openFilesViewModel;

    public AssetsFileViewModel? SelectedFile => _openFilesViewModel.SelectedFile;

    public ObservableCollection<ComponentViewModel> Components { get; } = [];

    [ObservableProperty]
    private ComponentViewModel? selectedComponent;

    public SceneHierarchyViewModel(AssetWorkspace assetWorkspace, OpenFilesViewModel openFilesViewModel)
    {
        _assetWorkspace = assetWorkspace;
        _openFilesViewModel = openFilesViewModel;

        _openFilesViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OpenFilesViewModel.SelectedFile))
                RefreshComponents();
        };

        RefreshComponents();
    }

    private void RefreshComponents()
    {
        SelectedComponent = null;
        Components.Clear();

        if (_openFilesViewModel.SelectedFile is { } file)
        {
            foreach (var component in GetComponents(file.AssetsFileInstance.file))
                Components.Add(component);
        }
    }
}
