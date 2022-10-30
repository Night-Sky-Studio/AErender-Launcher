using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AErenderLauncher.Views;

namespace AErenderLauncher {
    public partial class App : Application {
        public static Settings ApplicationSettings { get; private set; } = new Settings();

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public App() {
            Name = "AErender Launcher";
            
            if (Settings.Exists() && Settings.Load(Settings.SettingsPath) is { } settings) {
                ApplicationSettings = settings;
            } else if (Settings.ExistsLegacy() && Settings.LoadLegacy(Settings.LegacySettingsPath) is { } legacySettings) {
                ApplicationSettings = legacySettings;
            } else {
                ApplicationSettings = new Settings();
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