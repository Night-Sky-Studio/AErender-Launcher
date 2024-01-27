using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Controls;

public partial class IsPlatform : UserControl {
    public static readonly StyledProperty<object> DarwinProperty = 
        AvaloniaProperty.Register<IsPlatform, object>(nameof(Darwin));

    public static readonly StyledProperty<object> WindowsProperty = 
        AvaloniaProperty.Register<IsPlatform, object>(nameof(Windows));
    
    public object Darwin {
        get => GetValue(DarwinProperty);
        set => SetValue(DarwinProperty, value);
    }

    public object Windows {
        get => GetValue(WindowsProperty);
        set => SetValue(WindowsProperty, value);
    }

    public IsPlatform() {
        InitializeComponent();
    }

    private void AvaloniaObject_OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) { 
        Content = Helpers.Platform == OS.macOS ? Darwin : Windows;
    }
}