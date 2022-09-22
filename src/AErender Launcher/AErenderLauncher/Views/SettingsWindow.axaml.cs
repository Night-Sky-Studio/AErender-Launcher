using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace AErenderLauncher.Views; 

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
        
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }
}