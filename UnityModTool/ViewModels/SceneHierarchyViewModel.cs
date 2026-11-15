using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    private AssetsFileViewModel? observedFile;

    public AssetsFileViewModel? SelectedFile => _openFilesViewModel.SelectedFile;

    public ObservableCollection<ComponentViewModel> Components { get; } = [];

    [ObservableProperty]
    private ComponentViewModel? selectedComponent;

    private string? componentActionError;

    public string? ComponentActionError
    {
        get => componentActionError;
        private set
        {
            if (SetProperty(ref componentActionError, value))
                OnPropertyChanged(nameof(HasComponentActionError));
        }
    }

    public bool HasComponentActionError => ComponentActionError is not null;

    public SceneHierarchyViewModel(AssetWorkspace assetWorkspace, OpenFilesViewModel openFilesViewModel)
    {
        _assetWorkspace = assetWorkspace;
        _openFilesViewModel = openFilesViewModel;

        _openFilesViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OpenFilesViewModel.SelectedFile))
            {
                if (observedFile is not null)
                    observedFile.PropertyChanged -= OnObservedFilePropertyChanged;

                observedFile = _openFilesViewModel.SelectedFile;
                if (observedFile is not null)
                    observedFile.PropertyChanged += OnObservedFilePropertyChanged;

                RefreshComponents();
            }
        };

        observedFile = _openFilesViewModel.SelectedFile;
        if (observedFile is not null)
            observedFile.PropertyChanged += OnObservedFilePropertyChanged;

        RefreshComponents();
    }

    private void OnObservedFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AssetsFileViewModel.AssetsFileInstance))
            RefreshComponents();
    }

    public void ExportComponent(ComponentViewModel component, string destinationPath)
    {
        try
        {
            if (SelectedFile is not { } file || !Components.Contains(component))
                throw new InvalidOperationException("The selected component is not part of the active asset file.");

            File.WriteAllBytes(
                destinationPath,
                component.GetRawBytes(_assetWorkspace.Manager, file.AssetsFileInstance));
            ComponentActionError = null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            ComponentActionError = exception.Message;
        }
    }

    public void ReplaceComponentData(ComponentViewModel component, byte[] bytes)
    {
        try
        {
            if (SelectedFile is null || !Components.Contains(component))
                throw new InvalidOperationException("The dropped component is not part of the active asset file.");

            component.ReplaceRawData(bytes);
            ComponentActionError = null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            ComponentActionError = exception.Message;
        }
    }

    public void ReportComponentActionError(string message)
    {
        ComponentActionError = message;
    }

    private void RefreshComponents()
    {
        SelectedComponent = null;
        Components.Clear();

        if (_openFilesViewModel.SelectedFile is { } file)
        {
            var components = GetComponents(file.AssetsFileInstance.file);
            file.SetComponents(components);
            foreach (var component in components)
                Components.Add(component);
        }
    }
}
