using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AErenderLauncher.Views;

namespace AErenderLauncher {
    public partial class App : Application {
        public static Settings ApplicationSettings { get; set; } = new Settings();

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow { };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}