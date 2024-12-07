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
    
    public AErenderDetectDialog() {
        InitializeComponent();
    }

    public AErenderDetectDialog(List<AfterFx> paths) {
        InitializeComponent();
        Paths.AddRange(paths);
    }
    
    private void AerenderListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        Result = Paths[(e.Source as ListBox)?.SelectedIndex ?? 0];
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
        Close(Result);
    }
}