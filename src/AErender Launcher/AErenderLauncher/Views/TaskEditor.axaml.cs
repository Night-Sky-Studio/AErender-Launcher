using System.Collections.ObjectModel;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views;

public partial class TaskEditor : Window {
    public ObservableCollection<OutputModule> outputModules { get; set; } = new(ApplicationSettings.OutputModules);
    public ObservableCollection<string> renderSettings { get; set; } = new() {
        "Best Settings",
        "Current Settings",
        "DV Settings",
        "Draft Settings",
        "Multi-Machine Settings"
    };
    
    public TaskEditor() {
        InitializeComponent();
    }

    public TaskEditor(RenderTask task, bool isEditing = false) {
        InitializeComponent();
    }
}