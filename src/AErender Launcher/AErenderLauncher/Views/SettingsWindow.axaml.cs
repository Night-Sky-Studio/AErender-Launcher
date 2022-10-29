using System;
using System.Collections.Generic;
using System.IO;
using AErenderLauncher.Classes;
using AErenderLauncher.Views.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

using static AErenderLauncher.App;

namespace AErenderLauncher.Views; 

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
        
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private async void AerenderPathSelectButton_OnClick(object? sender, RoutedEventArgs e) {
        OpenFileDialog dialog = new OpenFileDialog() {
            Title = "Select aerender",
            AllowMultiple = false,
            Filters = new() {
                new() {
                    Extensions = { Helpers.Platform == OperatingSystemType.WinNT ? "aerender.exe" : "aerender" }, Name = "aerender"
                }
            },
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.Programs)
        };
        string[]? result = await dialog.ShowAsync(this);

        ApplicationSettings.AErenderPath = result?.Length == 0 ? "" : result?[0].Split('\n')[2] ?? "";
        AerenderPath.Text = ApplicationSettings.AErenderPath;
    }

    private async void AerenderDetectButton_OnClick(object? sender, RoutedEventArgs e) {
        List<string> paths = Settings.DetectAerender();
        string result;
        if (paths.Count == 1)
            result = paths[0];
        else {
            AErenderDetectDialog dialog = new AErenderDetectDialog(paths);

            await dialog.ShowDialog(this);

            result = dialog._output;
        }

        ApplicationSettings.AErenderPath = result;
        AerenderPath.Text = result;
    }
}