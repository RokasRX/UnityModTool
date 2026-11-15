using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using UnityModTool.ViewModels;

namespace UnityModTool.Views;

public partial class OpenFilesView : UserControl
{
    public OpenFilesView()
    {
        InitializeComponent();
        DragDrop.AddDragOverHandler(DropTarget, OnDragOver);
        DragDrop.AddDropHandler(DropTarget, OnDrop);
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.TryGetFiles() is { Length: > 0 }
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;

        if (DataContext is not OpenFilesViewModel viewModel)
        {
            return;
        }

        var filesToOpen = new List<string>();
        foreach (var item in e.DataTransfer.TryGetFiles() ?? [])
        {
            var path = item.Path?.LocalPath;
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (File.Exists(path))
            {
                filesToOpen.Add(path);
            }
            else if (Directory.Exists(path))
            {
                filesToOpen.AddRange(Directory.EnumerateFiles(path));
            }
        }

        viewModel.OpenFiles(filesToOpen);
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not OpenFilesViewModel viewModel || GetContextFile(sender) is not { } file)
            return;

        viewModel.SelectedFile = file;
        viewModel.SaveFile(file);
    }

    private async void OnSaveAsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not OpenFilesViewModel viewModel || GetContextFile(sender) is not { } file)
            return;

        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            viewModel.ReportSaveError("The save file picker is unavailable.");
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
                Title = "Save asset file as",
                SuggestedFileName = file.Name,
                SuggestedStartLocation = suggestedStartLocation,
                FileTypeChoices = [new FilePickerFileType("All files") { Patterns = ["*"] }]
            });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            viewModel.ReportSaveError(exception.Message);
            return;
        }

        var destinationPath = destination?.TryGetLocalPath();
        if (destinationPath is null)
        {
            if (destination is not null)
                viewModel.ReportSaveError("The selected destination is not a local file path.");
            return;
        }

        viewModel.SelectedFile = file;
        viewModel.SaveFileAs(file, destinationPath);
    }

    private static AssetsFileViewModel? GetContextFile(object? sender)
    {
        return sender is MenuItem { DataContext: AssetsFileViewModel file }
            ? file
            : null;
    }
}
