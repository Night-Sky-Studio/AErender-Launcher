﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.System.Dialogs;
using AErenderLauncher.ViewModels;
using AErenderLauncher.Views.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views;

public partial class SettingsWindow : Window {
    public SettingsViewModel ViewModel { get; set; } = new();
    
    public SettingsWindow() {
        InitializeComponent();
        
        DataContext = ViewModel;
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new ("0,32,*,32") : new ("32,32,*,32");
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        Settings.Current.Save();
        Close();
    }

    private async void AerenderPathSelectButton_OnClick(object? sender, RoutedEventArgs e) {
        List<IStorageFile>? result = await this.ShowOpenFileDialogAsync(
            [ new ("aerender", Helpers.Platform == OS.Windows ? "exe" : "*") ],
            StartingPath: Environment.GetFolderPath(Environment.SpecialFolder.Programs)
        );

        if (result == null) return;
        if (result.Count == 0) return;
        if (result.First().TryGetLocalPath() is not { } path) return;

        Settings.Current.AErenderPath = path;
        ViewModel.AErenderPath = Settings.Current.AErenderPath;
    }

    private async void AerenderDetectButton_OnClick(object? sender, RoutedEventArgs e) {
        List<AErender> paths = Settings.DetectAerender();
        AErender? result;
        
        if (paths.Count == 1)
            result = paths[0];
        else {
            AErenderDetectDialog dialog = new(paths);
            result = await dialog.ShowDialog<AErender?>(this);
        }

        if (result != null) {
            Settings.Current.AErenderPath = result.Value.Path;
            ViewModel.AErenderPath = Settings.Current.AErenderPath;
        }
    }

    private void SettingsWindow_OnClosed(object? sender, EventArgs e) {
        ViewModel.WriteToSettings();
    }
}