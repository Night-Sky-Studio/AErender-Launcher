using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AErenderLauncher.Classes;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using DynamicData;

namespace AErenderLauncher.Views.Dialogs;

public partial class AErenderDetectDialog : Window {
    public ObservableCollection<AErender> _paths { get; set; } = new ObservableCollection<AErender>();
    public AErender? _output { get; private set; }
    
    public AErenderDetectDialog() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
    }

    public AErenderDetectDialog(List<AErender> Paths) {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
        
        _paths.AddRange(Paths);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        _output = _paths[(e.Source as ListBox)?.SelectedIndex ?? 0];
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
        Close(_output);
    }
}