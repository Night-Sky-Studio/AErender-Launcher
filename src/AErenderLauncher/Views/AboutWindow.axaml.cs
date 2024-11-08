using AErenderLauncher.Classes;
using AErenderLauncher.Classes.System.Dialogs;
using AErenderLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AErenderLauncher.Views;

public partial class AboutWindow : Window {
    private AboutWindowViewModel ViewModel { get; } = new();
    
    public AboutWindow() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new ("0,*") : new ("32,*");
        
        DataContext = ViewModel;
    }
}