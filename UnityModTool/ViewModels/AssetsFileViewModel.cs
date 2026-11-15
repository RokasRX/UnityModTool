using System;
using System.IO;
using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UnityModTool.ViewModels;

public sealed class AssetsFileViewModel(
    string filePath,
    AssetsFileInstance assetsFileInstance,
    AssetsManager manager) : ViewModelBase
{
    private string filePath = filePath;
    private readonly List<ComponentViewModel> components = [];

    private AssetsFileInstance assetsFileInstance = assetsFileInstance;

    public string FilePath
    {
        get => filePath;
        private set
        {
            if (SetProperty(ref filePath, value))
                OnPropertyChanged(nameof(Name));
        }
    }

    public string OriginalFilePath { get; } = filePath;

    public string Name { get; } = Path.GetFileName(filePath);

    public AssetsFileInstance AssetsFileInstance
    {
        get => assetsFileInstance;
        private set => SetProperty(ref assetsFileInstance, value);
    }

    public AssetsFile assetsFile => AssetsFileInstance.file;

    public void SetComponents(IEnumerable<ComponentViewModel> componentViewModels)
    {
        components.Clear();
        components.AddRange(componentViewModels);
    }

    public void Save()
    {
        SaveTo(FilePath);
    }

    public void SaveAs(string destinationPath)
    {
        SaveTo(destinationPath);
    }

    private void SaveTo(string destinationPath)
    {
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new IOException("The destination directory could not be determined.");
        var tempPath = Path.Combine(directory, $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");
        var previousPath = FilePath;

        try
        {
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new AssetsFileWriter(stream))
                assetsFile.Write(writer, 0);

            manager.UnloadAssetsFile(AssetsFileInstance);
            try
            {
                File.Move(tempPath, fullDestinationPath, true);
                FilePath = fullDestinationPath;
            }
            catch
            {
                AssetsFileInstance = manager.LoadAssetsFile(previousPath, false);
                throw;
            }

            AssetsFileInstance = manager.LoadAssetsFile(FilePath, false);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        MarkComponentsSaved();
    }

    private void MarkComponentsSaved()
    {
        foreach (var component in components)
            component.MarkSaved();
    }
}
