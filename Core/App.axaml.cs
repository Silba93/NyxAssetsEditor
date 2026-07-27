using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using NyxAssetsEditor.Services.Persistence;
using NyxAssetsEditor.ViewModels.Shell;
using NyxAssetsEditor.Views.Shell;

namespace NyxAssetsEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        try
        {
            var locatorType = typeof(IAssetLoader).Assembly.GetType("Avalonia.AvaloniaLocator");
            if (locatorType != null)
            {
                var currentProp = locatorType.GetProperty("CurrentMutable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var currentLocator = currentProp?.GetValue(null);

                if (currentLocator != null)
                {
                    var registryField = locatorType.GetField("_registry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var registry = registryField?.GetValue(currentLocator) as System.Collections.IDictionary;

                    if (registry != null)
                    {
                        var getServiceMethod = currentLocator.GetType().GetMethod("GetService", new Type[] { typeof(Type) })
                            ?? currentLocator.GetType().GetMethods().FirstOrDefault(m => m.Name == "GetService" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Type));

                        var defaultLoader = getServiceMethod?.Invoke(currentLocator, new object[] { typeof(IAssetLoader) }) as IAssetLoader;
                        if (defaultLoader != null && defaultLoader.GetType() != typeof(Core.FileSystemAssetLoader))
                        {
                            var wrappedLoader = new Core.FileSystemAssetLoader(defaultLoader);
                            registry[typeof(IAssetLoader)] = new Func<object?>(() => wrappedLoader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileSystemAssetLoader Error] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        PersistenceService.LoadSettings();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}