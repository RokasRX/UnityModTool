using Avalonia.Controls;
using Dock.Model.Core;
using UnityModTool.Factories;

namespace UnityModTool.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var factory = new DefaultLayoutFactory();
        var layout = factory.CreateLayout();
        
        if (layout != null)
        {
            factory.InitLayout(layout);
            DockControl.Layout = layout;
        }
    }
}