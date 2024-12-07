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
        
        DataContext = ViewModel;
    }
}