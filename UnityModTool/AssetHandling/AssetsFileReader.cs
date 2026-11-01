using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using UnityModTool.ViewModels;

namespace UnityModTool.AssetHandling;

public static class AssetFileReader
{
    public static bool IsAssetsFileOpened(ObservableCollection<AssetsFileViewModel> files, string filePath)
    {
        return files.Any(f => f.FilePath == filePath);
    }

    public static AssetsFileViewModel OpenAssetsFile(AssetsManager manager, string filePath)
    {
        var assetsFileInstance = manager.LoadAssetsFile(filePath, false);
        return new AssetsFileViewModel(filePath, assetsFileInstance);
    }

    public static List<ComponentViewModel> GetComponents(AssetsFile assetsFile)
    {
        var components = new List<ComponentViewModel>();
        foreach (var asset in assetsFile.AssetInfos)
        {
            var component = new ComponentViewModel(asset);
            components.Add(component);
        }
        return components;
    }

    public static List<PropertyViewModel> GetProperties(AssetsManager manager, AssetsFileInstance assetsFileInstance, ComponentViewModel component)
    {
        manager.LoadClassDatabaseFromPackage(assetsFileInstance.file.Metadata.UnityVersion);
        var root = manager.GetBaseField(assetsFileInstance, component.AssetFileInfo);
        return root.Children
            .Select(child => CreateProperty(child, root, component))
            .ToList();
    }

    private static PropertyViewModel CreateProperty(AssetTypeValueField field, AssetTypeValueField root, ComponentViewModel component)
    {
        return new PropertyViewModel(
            field.TemplateField.Name,
            field,
            () => component.AssetFileInfo.SetNewData(root));
    }
}