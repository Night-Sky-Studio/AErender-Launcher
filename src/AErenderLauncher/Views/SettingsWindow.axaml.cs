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
        Close();
    }

    private async void AerenderPathSelectButton_OnClick(object? sender, RoutedEventArgs e) {
        var result = await this.ShowOpenFileDialogAsync(
            [ new ("After Effects", Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore") ],
            startingPath: Environment.GetFolderPath(Environment.SpecialFolder.Programs)
        );

        if (result == null) return;
        if (result.Count == 0) return;
        if (result.First().TryGetLocalPath() is not { } path) return;

        ViewModel.AErenderPath = Settings.Current.AfterEffectsPath = path;
    }

    private async void AerenderDetectButton_OnClick(object? sender, RoutedEventArgs e) {
        List<AfterFx> paths = Settings.DetectAfterEffects();
        AfterFx? result;
        
        if (paths.Count == 1)
            result = paths[0];
        else {
            AErenderDetectDialog dialog = new(paths);
            result = await dialog.ShowDialog<AfterFx?>(this);
        }

        if (result != null) {
            ViewModel.AErenderPath = Settings.Current.AfterEffectsPath = result.Value.Path;
        }
    }

    private void SettingsWindow_OnClosed(object? sender, EventArgs e) {
        ViewModel.WriteToSettings();
    }

    private async void OutputDirectorySelectButton_OnClick(object? sender, RoutedEventArgs e) {
        var result = await this.ShowOpenFolderDialogAsync();

        if (result is not { Count: > 0 } list) return;
        if (list[0].TryGetLocalPath() is not { } path) return;
        
        ViewModel.DefaultOutputPath = Settings.Current.DefaultOutputPath = path;
    }

    private async void ProjectsDirectorySelectButton_OnClick(object? sender, RoutedEventArgs e) {
        var result = await this.ShowOpenFolderDialogAsync();

        if (result is not { Count: > 0 } list) return;
        if (list[0].TryGetLocalPath() is not { } path) return;
        
        ViewModel.DefaultProjectsPath = Settings.Current.DefaultProjectsPath = path;
    }
}