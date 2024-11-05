using AErenderLauncher.ViewModels;
using Avalonia.Controls;

namespace AErenderLauncher.Views;

public partial class AboutWindow : Window {
    private AboutWindowViewModel ViewModel { get; } = new();
    
    public AboutWindow() {
        InitializeComponent();
        
        DataContext = ViewModel;
    }
}