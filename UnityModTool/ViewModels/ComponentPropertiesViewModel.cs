using System;
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

    private string? propertiesError;

    public string? PropertiesError
    {
        get => propertiesError;
        private set
        {
            if (SetProperty(ref propertiesError, value))
                OnPropertyChanged(nameof(HasPropertiesError));
        }
    }

    public bool HasPropertiesError => PropertiesError is not null;

    public ComponentPropertiesViewModel(AssetWorkspace assetWorkspace, SceneHierarchyViewModel sceneHierarchyViewModel)
    {
        _assetWorkspace = assetWorkspace;
        _sceneHierarchyViewModel = sceneHierarchyViewModel;

        _sceneHierarchyViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SceneHierarchyViewModel.SelectedComponent))
            {
                if (SelectedComponent is not null)
                    SelectedComponent.DataReplaced -= OnSelectedComponentDataReplaced;

                SelectedComponent = _sceneHierarchyViewModel.SelectedComponent;
                if (SelectedComponent is not null)
                    SelectedComponent.DataReplaced += OnSelectedComponentDataReplaced;

                RefreshProperties();
            }
        };

        SelectedComponent = _sceneHierarchyViewModel.SelectedComponent;
        if (SelectedComponent is not null)
            SelectedComponent.DataReplaced += OnSelectedComponentDataReplaced;
        RefreshProperties();
    }

    private void OnSelectedComponentDataReplaced(object? sender, EventArgs e)
    {
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        Properties.Clear();
        PropertiesError = null;

        if (SelectedComponent is { } component
            && _sceneHierarchyViewModel.SelectedFile is { } file)
        {
            try
            {
                foreach (var property in GetProperties(
                             _assetWorkspace.Manager,
                             file.AssetsFileInstance,
                             component))
                    Properties.Add(property);
            }
            catch (Exception exception)
            {
                PropertiesError = $"Could not read component properties: {exception.Message}";
            }
        }
    }
}
