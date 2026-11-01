using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using UnityModTool.ViewModels;

namespace UnityModTool.Views;

public partial class PropertiesView : UserControl
{
    public PropertiesView()
    {
        InitializeComponent();
        DragDrop.AddDragOverHandler(this, OnByteArrayDragOver);
        DragDrop.AddDropHandler(this, OnByteArrayDrop);
    }

    private static PropertyViewModel? GetByteArrayPreview(object? source)
    {
        return source is Control control
            && control.DataContext is PropertyViewModel { CanImportByteArray: true } property
                ? property
                : null;
    }

    private static void OnByteArrayDragOver(object? sender, DragEventArgs e)
    {
        if (GetByteArrayPreview(e.Source) is null)
            return;

        e.DragEffects = e.DataTransfer.TryGetFiles() is { Length: 1 }
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private static void OnByteArrayDrop(object? sender, DragEventArgs e)
    {
        if (GetByteArrayPreview(e.Source) is not { } property)
            return;

        e.Handled = true;
        var files = e.DataTransfer.TryGetFiles();
        if (files is not { Length: 1 } || files[0].Path is not { } filePath)
        {
            property.SetImportError("Drop one file to replace these bytes.");
            return;
        }

        var path = filePath.LocalPath;
        if (!File.Exists(path))
        {
            property.SetImportError("The dropped item is not a file.");
            return;
        }

        try
        {
            property.ImportByteArray(File.ReadAllBytes(path));
        }
        catch (IOException exception)
        {
            property.SetImportError($"Could not read file: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            property.SetImportError($"Could not read file: {exception.Message}");
        }
    }
}
