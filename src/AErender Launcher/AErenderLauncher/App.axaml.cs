using System;
using System.IO;
using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AErenderLauncher.Views;

namespace AErenderLauncher;

public partial class App : Application {
    public static Settings ApplicationSettings { get; private set; } = new();

    public App() {
        Name = "AErender Launcher";
            
        // Load settings
        if (Settings.Exists() && Settings.Load(Settings.SettingsPath) is { } settings) {
            ApplicationSettings = settings;
        } else if (Settings.ExistsLegacy() && Settings.LoadLegacy(Settings.LegacySettingsPath) is { } legacySettings) {
            ApplicationSettings = legacySettings;
        } else {
            ApplicationSettings = new Settings();
        }
            
        // Create Launcher folder if there isn't one
        if (!Directory.Exists(ApplicationSettings.SettingsFolder)) {
            Directory.CreateDirectory(ApplicationSettings.SettingsFolder);
        }
    }
    
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }
#if !DEBUG
        else 
            throw new SystemException("This app can only be run on desktop platforms.");
#endif

        base.OnFrameworkInitializationCompleted();
    }
}