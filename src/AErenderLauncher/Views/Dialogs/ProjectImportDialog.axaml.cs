using System;
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
    public ObservableCollection<ProjectItem> Items { get; set; } = [];

    private void Init() {
        InitializeComponent();
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new ("0,32,*,32") : new ("32,32,*,32");
    }

    public ProjectImportDialog() {
        Init();
    }

    public ProjectImportDialog(ProjectItem[] items, string projectPath) {
        Init();
        
        Items.AddRange(items);
        ProjectPath.Text = projectPath;
    }
    
    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        if (CompositionsList.SelectedItems is not { Count: > 0 } compList) {
            Close(null);
            return;
        }
        
        List<Composition> compositions = new();
        
        foreach (ProjectItem? item in compList) {
            if (item != null) {
                compositions.Add(new() {
                    CompositionName = item.Name,
                    Frames = new() {
                        StartFrame = Convert.ToUInt32(item.Frames[0]),
                        EndFrame = Convert.ToUInt32(item.Frames[1]) - 1 // if not for this, we'd get 1 extra frame
                    },
                });
            }
        }
        
        Close(new RenderTask {
            Project = Settings.Current.LastProjectPath,
            Output = Settings.Current.LastOutputPath,
            Multiprocessing = Settings.Current.Multithreaded,
            MissingFiles = Settings.Current.MissingFiles,
            Sound = Settings.Current.Sound,
            CacheLimit = Settings.Current.CacheLimit,
            MemoryLimit = Settings.Current.MemoryLimit,
            CustomProperties = Settings.Current.CustomProperties,
            OutputModule = Settings.Current.ActiveOutputModule?.Module ?? "Lossless",
            RenderSettings = Settings.Current.RenderSettings,
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