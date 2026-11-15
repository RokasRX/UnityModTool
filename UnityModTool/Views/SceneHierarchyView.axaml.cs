using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using UnityModTool.ViewModels;

namespace UnityModTool.Views;

public partial class SceneHierarchyView : UserControl
{
    public SceneHierarchyView()
    {
        InitializeComponent();
        DragDrop.AddDragOverHandler(ComponentsList, OnComponentDragOver);
        DragDrop.AddDropHandler(ComponentsList, OnComponentDrop);
    }

    private async void OnExportBytesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SceneHierarchyViewModel viewModel
            || sender is not MenuItem { DataContext: ComponentViewModel component })
            return;

        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            viewModel.ReportComponentActionError("The save file picker is unavailable.");
            return;
        }

        var file = viewModel.SelectedFile;
        if (file is null)
        {
            viewModel.ReportComponentActionError("No asset file is selected.");
            return;
        }

        IStorageFile? destination;
        try
        {
            var startFolder = Path.GetDirectoryName(file.FilePath);
            var suggestedStartLocation = startFolder is null
                ? null
                : await storageProvider.TryGetFolderFromPathAsync(startFolder);

            destination = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export component bytes",
                SuggestedFileName = CreateExportFileName(component, file),
                SuggestedStartLocation = suggestedStartLocation,
                FileTypeChoices = [new FilePickerFileType("Data files") { Patterns = ["*.dat"] }]
            });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            viewModel.ReportComponentActionError(exception.Message);
            return;
        }

        var destinationPath = destination?.TryGetLocalPath();
        if (destinationPath is null)
        {
            if (destination is not null)
                viewModel.ReportComponentActionError("The selected destination is not a local file path.");
            return;
        }

        viewModel.SelectedComponent = component;
        viewModel.ExportComponent(component, destinationPath);
    }

    private static ComponentViewModel? GetComponent(object? source)
    {
        return source is Control control && control.DataContext is ComponentViewModel component
            ? component
            : null;
    }

    private static void OnComponentDragOver(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        var hasOneLocalFile = files is { Length: 1 }
            && files[0].Path?.IsFile == true
            && !string.IsNullOrEmpty(files[0].Path.LocalPath);

        e.DragEffects = GetComponent(e.Source) is not null && hasOneLocalFile
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnComponentDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;

        if (DataContext is not SceneHierarchyViewModel viewModel
            || GetComponent(e.Source) is not { } component)
            return;

        var files = e.DataTransfer.TryGetFiles();
        if (files is not { Length: 1 } || files[0].Path is not { IsFile: true } filePath)
        {
            viewModel.ReportComponentActionError("Drop one raw bytes file onto a component.");
            return;
        }

        var path = filePath.LocalPath;
        if (string.IsNullOrEmpty(path))
        {
            viewModel.ReportComponentActionError("The dropped file does not have a local path.");
            return;
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(path);
            viewModel.ReplaceComponentData(component, bytes);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            viewModel.ReportComponentActionError($"Could not read dropped file: {exception.Message}");
        }
    }

    private static string CreateExportFileName(ComponentViewModel component, AssetsFileViewModel file)
    {
        var fileName = $"{component.ComponentType}_{component.PathID}_{file.Name}.dat";
        var invalidCharacters = Path.GetInvalidFileNameChars()
            .Concat("<>:\"/\\|?*");

        foreach (var invalidCharacter in invalidCharacters)
            fileName = fileName.Replace(invalidCharacter, '_');

        return fileName;
    }
}
