using System.Collections.Generic;
using System.Collections.ObjectModel;
using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;

namespace AErenderLauncher.Views.Dialogs;

public partial class AErenderDetectDialog : Window {
    public ObservableCollection<AfterFx> Paths { get; set; } = new();
    public AfterFx? Result { get; private set; }

    private void Init() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
    }
    
    public AErenderDetectDialog() {
        Init();
    }

    public AErenderDetectDialog(List<AfterFx> paths) {
        Init();
        Paths.AddRange(paths);
    }
    
    private void AerenderListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        Result = Paths[(e.Source as ListBox)?.SelectedIndex ?? 0];
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
        Close(Result);
    }
}