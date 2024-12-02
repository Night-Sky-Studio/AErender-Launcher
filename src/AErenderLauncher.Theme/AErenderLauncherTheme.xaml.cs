using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AErenderLauncher.Theme;

public class AErenderLauncherTheme : Styles {
    public AErenderLauncherTheme(IServiceProvider? provider = null) 
        => AvaloniaXamlLoader.Load(provider, this);
}