using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Project;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AErenderLauncher.Views;
using Avalonia.Platform;

namespace AErenderLauncher {
    public partial class App : Application {
        public static Settings ApplicationSettings { get; private set; } = new Settings();

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

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

        public override void OnFrameworkInitializationCompleted() {
            
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}