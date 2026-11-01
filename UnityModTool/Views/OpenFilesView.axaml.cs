using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
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
}
