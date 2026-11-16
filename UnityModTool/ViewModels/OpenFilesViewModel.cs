using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using UnityModTool.AssetHandling;
using static UnityModTool.AssetHandling.AssetFileReader;

namespace UnityModTool.ViewModels;

public sealed partial class OpenFilesViewModel : Tool
{
    private readonly AssetWorkspace _assetWorkspace;

    public ObservableCollection<AssetsFileViewModel> Files { get; } = [];

    [ObservableProperty]
    private AssetsFileViewModel? selectedFile;

    private string? saveError;

    public string? SaveError
    {
        get => saveError;
        private set
        {
            if (SetProperty(ref saveError, value))
                OnPropertyChanged(nameof(HasSaveError));
        }
    }

    public bool HasSaveError => SaveError is not null;

    public OpenFilesViewModel(AssetWorkspace assetWorkspace)
    {
        _assetWorkspace = assetWorkspace;
    }

    public void OpenFiles(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (!IsAssetsFileOpened(Files, filePath))
            {
                if (TryOpenAssetsFile(_assetWorkspace.Manager, filePath) is { } file)
                    Files.Add(file);
            }
        }
    }

    public void SaveFile(AssetsFileViewModel file)
    {
        TrySave(file.Save);
    }

    public void SaveFileAs(AssetsFileViewModel file, string destinationPath)
    {
        TrySave(() => file.SaveAs(destinationPath));
    }

    public void ReportSaveError(string message)
    {
        SaveError = message;
    }

    private void TrySave(Action save)
    {
        try
        {
            save();
            SaveError = null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            SaveError = exception.Message;
        }
    }
}
