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
        public static AeProjectParser? ProjectParser { get; private set; }

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
            
            // Extract aep parser, if not found
            if (!File.Exists(Path.Combine(Settings.SettingsPath, Helpers.Platform == OperatingSystemType.OSX ? "aeparser_mac" : "aeparser_win.exe"))) {
                if (AvaloniaLocator.Current.GetService<IAssetLoader>() is { } loader) {
                    if (Helpers.Platform == OperatingSystemType.OSX) {
                        Uri asset = new Uri("avares://AErenderLauncher/Assets/aeparser/aeparser_mac");
                        if (loader.Exists(asset))
                            loader.Open(asset)
                                .CopyTo(File.OpenWrite(Path.Combine(ApplicationSettings.SettingsFolder, "aeparser_mac")));
                        Process.Start("chmod", $"+x \"{Path.Combine(ApplicationSettings.SettingsFolder, "aeparser_mac")}\"");
                    } else
                        loader.Open(new Uri("avares://Assets/aeparser/aeparser_win.exe"))
                            .CopyTo(File.OpenWrite(Path.Combine(ApplicationSettings.SettingsFolder, "aeparser_win.exe")));
                }
            }
            
            // Create Parser
            ProjectParser = new AeProjectParser();
        }

        public override void OnFrameworkInitializationCompleted() {
            
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}