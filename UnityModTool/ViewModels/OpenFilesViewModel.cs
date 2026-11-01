using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                Files.Add(OpenAssetsFile(_assetWorkspace.Manager, filePath));
            }
        }
    }
}
