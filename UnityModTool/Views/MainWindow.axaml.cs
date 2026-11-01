using Avalonia.Controls;
using Dock.Model.Core;
using UnityModTool.AssetHandling;
using UnityModTool.Factories;

namespace UnityModTool.Views;

public partial class MainWindow : Window
{
    public MainWindow() : this(new AssetWorkspace())
    {
    }

    public MainWindow(AssetWorkspace assetWorkspace)
    {
        InitializeComponent();

        var factory = new DefaultLayoutFactory(assetWorkspace);
        var layout = factory.CreateLayout();
        
        if (layout != null)
        {
            factory.InitLayout(layout);
            DockControl.Layout = layout;
        }
    }
}