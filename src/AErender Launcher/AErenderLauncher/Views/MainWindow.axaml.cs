using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Project;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System.Dialogs;
using AErenderLauncher.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views;

public partial class MainWindow : Window {
    private RenderingWindow? _renderingWindow;
    public static ObservableCollection<RenderTask> Tasks { get; set; } = new ();

    public static ObservableCollection<RenderThread> Threads { get; set; } = new ();

    public MainWindow() {
        InitializeComponent();
#if DEBUG
        DebugLabel.IsVisible = true;
        // Tasks.Add(new RenderTask {
        //     Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
        //     Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
        //     OutputModule = "Lossless",
        //     RenderSettings = "Best Settings",
        //     MissingFiles = true,
        //     Sound = true,
        //     Multiprocessing = false,
        //     CacheLimit = 100,
        //     MemoryLimit = 5,
        //     Compositions = [
        //         new("Game Icons", new FrameSpan(0, 120), 1),
        //         new("Web Icons", new FrameSpan(0, 120), 1),
        //         new("Ecology Icons", new FrameSpan(0, 120), 1),
        //         new("Medical Icons", new FrameSpan(0, 120), 1)
        //     ]
        // });
        Tasks.Add(new RenderTask {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\SuperEffectiveBros - Mograph Practice\\AEPRTC_Eclipse_rev57(ForDist)_2022.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
            Compositions = [
                new("Main", new FrameSpan(3600, 6130), 1),
            ]
        });
        DebugLabel.Text = $"Tasks: {Tasks.Count}";
#endif

        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,48") : new RowDefinitions("32,32,*,48");
    }

    private async void NewTaskButton_OnClick(object sender, RoutedEventArgs e) {
        List<IStorageFile>? aep = await this.ShowOpenFileDialogAsync(
            [ new("After Effects project", "*.aep") ]
        );
        
        if (aep == null || aep.Count == 0) return;
        
        // parse dat
        Overlay.IsVisible = true;
        
        if (aep.First().TryGetLocalPath() is { } path && await ParseProject(path) is { } task) {
            Tasks.Add(task);
        } else {
            // TODO: Fallback, manual input
            System.Diagnostics.Debug.WriteLine("Failed to parse project");
        }
        
        Overlay.IsVisible = false;
    }

    private async Task<RenderTask?> ParseProject(string projectPath) {
        List<ProjectItem>? project = await AeProjectParser.ParseProjectAsync(projectPath);
        
        if (project != null) {
            ApplicationSettings.LastProjectPath = projectPath;
        
            ProjectImportDialog dialog = new(project.ToArray(), projectPath);
            RenderTask? task = await dialog.ShowDialog<RenderTask?>(this);
        
            return task;
        }
        
        return null;
    }

    private async void Launch_OnClick(object sender, RoutedEventArgs e) {
        var aggregatedTasks = Tasks.Aggregate(new List<RenderThread>(), (threads, task) => {
            threads.AddRange(task.Enqueue());
            return threads;
        });
        
        _renderingWindow = new(aggregatedTasks);

        await Task.WhenAll([
            _renderingWindow.Start(),
            _renderingWindow.ShowDialog(this)
        ]);
    }

    private async void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
        SettingsWindow settings = new();
        await settings.ShowDialog(this);
    }

    private async void NewTaskEmpty_OnClick(object? sender, RoutedEventArgs e) {
        var provider = GetTopLevel(this)?.StorageProvider;
        
        List<IStorageFile>? aep = await this.ShowOpenFileDialogAsync(
            [ new("After Effects project", "*.aep") ]
        );

        if (aep != null && aep.Count != 0 && aep.First().TryGetLocalPath() is { } prgPath) {
            TaskEditor editor = new(new RenderTask {
                Project = prgPath
            });
            
            RenderTask? result = await editor.ShowDialog<RenderTask?>(this);
            if (result != null) {
                Tasks.Add(result);
            }
        }
    }

    private void MoveTaskUp_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        RenderTask task = Tasks.GetTaskById(int.Parse($"{btn.Tag}"));
        int index = Tasks.IndexOf(task);
        if (index > 0) {
            Tasks.Swap(index, index - 1);
        }
    }

    private void MoveTaskDown_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        RenderTask task = Tasks.GetTaskById(int.Parse($"{btn.Tag}"));
        int index = Tasks.IndexOf(task);
        if (index < Tasks.Count - 1) {
            Tasks.Swap(index, index + 1);
        }
    }

    private void RemoveTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        RenderTask task = Tasks.GetTaskById(int.Parse($"{btn.Tag}"));
        Tasks.Remove(task);
    }

    // TODO: questionable need for this...
    private void DuplicateTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        RenderTask task = Tasks.GetTaskById(int.Parse($"{btn.Tag}"));
        Tasks.Insert(Tasks.IndexOf(task) + 1, task);
    }
    

    private async void Composition_OnDoubleTapped(object? sender, TappedEventArgs e) {
#pragma warning disable 0162
        // BUG: This works, but there are two problems
        //      1. Somehow changing CompList selection before the dialog is shown
        //         messes up the layout of the ListBoxItem's Content
        //      2. Transition of EditorCarousel is being triggered regardless
        //         of the method it's index is being changed (Next() or SelectedIndex)
        return; // Disabled, unless the problem above is fixed
        if (sender is not ListBoxItem { DataContext: Composition comp } lb) return;
        if (lb.Parent?.Parent is not ListBox bx || int.TryParse($"{bx.Tag}", out var id) == false) return;
        var task = Tasks.GetTaskById(id);
        TaskEditor editor = new(task) {
            EditorCarousel = {
                SelectedIndex = 1
            },
            CompList = {
                SelectedIndex = task.Compositions.IndexOf(comp)
            }
        };
        var newTask = await editor.ShowDialog<RenderTask?>(this);
        if (newTask != null) Tasks[Tasks.IndexOf(task)] = newTask;
#pragma warning restore 0162
    }

    
    private async void EditTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        var task = Tasks.GetTaskById(int.Parse($"{btn.Tag}"));
        TaskEditor editor = new(Tasks.GetTaskById(int.Parse($"{btn.Tag}")), true);
        var newTask = await editor.ShowDialog<RenderTask?>(this);
        if (newTask != null) Tasks[Tasks.IndexOf(task)] = newTask;
    }

    private async void QueueButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_renderingWindow != null) await _renderingWindow.ShowDialog(this);
    }
}