using System;
using System.IO;
using System.Reflection;
using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AErenderLauncher.Views;
using Semver;

namespace AErenderLauncher;

public partial class App : Application {
    public static SemVersion Version => SemVersion.Parse(Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
    public App() {
        Name = "AErender Launcher";
            
        // Load settings
        if (Settings.Exists() && Settings.Load(Settings.SettingsPath) is { } settings) {
            Settings.Current = settings;
        } else if (Settings.ExistsLegacy() && Settings.LoadLegacy(Settings.LegacySettingsPath) is { } legacySettings) {
            Settings.Current = legacySettings;
        } else {
            Settings.Current = new ();
            Settings.Current.Init();
            Settings.Current.Save();
        }
            
        // Create Launcher folder if there isn't one
        if (!Directory.Exists(Settings.Current.SettingsFolder)) {
            Directory.CreateDirectory(Settings.Current.SettingsFolder);
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

    public static void OpenAboutWindow() {
        var aboutWindow = new AboutWindow();
        aboutWindow.Show();
    }

    private void AboutMenu_OnClick(object? sender, EventArgs e) => OpenAboutWindow();
}