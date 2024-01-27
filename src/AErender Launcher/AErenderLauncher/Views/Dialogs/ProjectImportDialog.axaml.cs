﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Project;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views.Dialogs;

public partial class ProjectImportDialog : Window {
    public ObservableCollection<ProjectItem> Items { get; set; } = new ObservableCollection<ProjectItem>();

    public ProjectImportDialog() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
    }

    public ProjectImportDialog(ProjectItem[] items, string projectPath) {
        InitializeComponent();

        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
        
        Items.AddRange(items);
        ProjectPath.Text = projectPath;
    }
    
    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        List<Composition> compositions = new();
        foreach (ProjectItem? item in CompositionsList.SelectedItems) {
            if (item != null) {
                compositions.Add(new() {
                    CompositionName = item.Name,
                    Frames = new() {
                        StartFrame = Convert.ToInt32(item.Frames[0]),
                        EndFrame = Convert.ToInt32(item.Frames[1])
                    },
                });
            }
        }
        
        if (compositions.Count == 0) 
            Close(null);
        else
            Close(new RenderTask {
                Project = ApplicationSettings.LastProjectPath,
                Output = ApplicationSettings.LastOutputPath,
                Multiprocessing = ApplicationSettings.Multithreaded,
                MissingFiles = ApplicationSettings.MissingFiles,
                Sound = ApplicationSettings.Sound,
                CacheLimit = ApplicationSettings.CacheLimit,
                MemoryLimit = ApplicationSettings.MemoryLimit,
                CustomProperties = ApplicationSettings.CustomProperties,
                OutputModule = ApplicationSettings.ActiveOutputModule?.Module ?? "Lossless",
                RenderSettings = ApplicationSettings.RenderSettings,
                Compositions = new (compositions)
            });
    }

    private void TopLevel_OnClosed(object sender, EventArgs e) {
        
    }

    private void CompositionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        ProjectItem? added   = e.AddedItems.Count > 0 ? e.AddedItems[^1] as ProjectItem ?? null : null;
        ProjectItem? removed = e.RemovedItems.Count > 0 ? e.RemovedItems[^1] as ProjectItem ?? null : null;

        ProjectItem? item = added ?? removed ?? null;

        if (item != null) {
            CompLabel.Content = $"Composition: {item.Name}";
            ResLabel.Content = $"Resolution: {item.FootageDimensions[0]}x{item.FootageDimensions[1]}";
            FramerateLabel.Content = $"Framerate: {Math.Round(item.FootageFramerate, 3)}";
            SFrameLabel.Content = $"Range start: {item.Frames[0]}";
            EFrameLabel.Content = $"Range end: {item.Frames[1]}"; 
        }

    }
}