using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AErenderLauncher.Classes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using ReactiveUI;

namespace AErenderLauncher.Views.Dialogs;

public partial class AErenderDetectDialog : Window {
    public ObservableCollection<AErender> _paths { get; set; } = new ObservableCollection<AErender>();
    public AErender _output { get; private set; }
    
    public AErenderDetectDialog() {
        InitializeComponent();
    }

    public AErenderDetectDialog(List<AErender> Paths) {
        InitializeComponent();
        
        _paths.AddRange(Paths);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        _output = _paths[(e.Source as ListBox)?.SelectedIndex ?? 0];
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
        Close();
    }
}