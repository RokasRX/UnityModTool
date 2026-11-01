using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UnityModTool.AssetHandling;
using UnityModTool.ViewModels;
using UnityModTool.Views;

namespace UnityModTool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var assetWorkspace = new AssetWorkspace();
            desktop.Exit += (_, _) => assetWorkspace.Dispose();
            desktop.MainWindow = new MainWindow(assetWorkspace)
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}