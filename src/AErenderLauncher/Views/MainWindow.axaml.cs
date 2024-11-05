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
using AErenderLauncher.ViewModels;
using AErenderLauncher.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace AErenderLauncher.Views;

public partial class MainWindow : Window {
    private RenderingWindow? _renderingWindow;

    private MainWindowViewModel ViewModel { get; } = new();

    public MainWindow() {
        InitializeComponent();
        
        DataContext = ViewModel;
        
        Title = $"AErender Launcher (v{App.Version.WithoutMetadata()})";
#if DEBUG
        // ViewModel.Tasks.Add(new RenderTask {
        //     Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
        //     Output = "C:\\Users\\lilystilson\\Desktop\\[projectName]\\[compName].[fileExtension]",
        //     OutputModule = "Lossless",
        //     RenderSettings = "Best Settings",
        //     MissingFiles = true,
        //     Sound = true,
        //     Multiprocessing = false,
        //     CacheLimit = 100,
        //     MemoryLimit = 5,
        //     Compositions = [
        //         new("Game Icons", new FrameSpan(0, 600), 1),
        //         new("Web Icons", new FrameSpan(0, 600), 1),
        //         new("Ecology Icons", new FrameSpan(0, 600), 1),
        //         new("Medical Icons", new FrameSpan(0, 600), 1)
        //     ]
        // });
        ViewModel.Tasks.Add(new RenderTask {
            Project = Helpers.Platform == OS.Windows 
                ? "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\SuperEffectiveBros - Mograph Practice\\AEPRTC_Eclipse_rev57(ForDist)_2024.aep"
                : "/Users/lilystilson/Yandex.Disk.localized/Acer/Footages (AE)/AErender Launcher Benchmark Projects/SuperEffectiveBros - Mograph Practice/AEPRTC_Eclipse_rev57(ForDist)_2024.aep",
            Output = Helpers.Platform == OS.Windows 
                ? "C:\\Users\\LilyStilson\\Desktop\\[projectName]\\[compName].[fileExtension]"
                : "/Users/lilystilson/Desktop/Mograph Practice/[compName].[fileExtension]",
            MissingFiles = true, Sound = true,
            MemoryLimit = 5,
            Compositions = [
                new("Main", new FrameSpan(3600, 6130), 2),
            ]
        });
#endif

        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,48") : new RowDefinitions("32,32,*,48");
    }

    private async void NewTaskButton_OnClick(object sender, RoutedEventArgs e) {
        var aep = await this.ShowOpenFileDialogAsync(
            [ new("After Effects project", "*.aep") ],
            startingPath: Settings.Current.DefaultProjectsPath
        );
        
        if (aep == null || aep.Count == 0) return;
        
        // parse dat
        Overlay.IsVisible = true;
        
        if (aep.First().TryGetLocalPath() is { } path && await ParseProject(path) is { } task) {
            ViewModel.Tasks.Add(task);
        } else {
            // TODO: Fallback, manual input
            System.Diagnostics.Debug.WriteLine("Failed to parse project");
        }
        
        Overlay.IsVisible = false;
    }

    private async Task<RenderTask?> ParseProject(string projectPath) {
        List<ProjectItem>? project = await AeProjectParser.ParseProjectAsync(projectPath);
        
        if (project != null) {
            Settings.Current.LastProjectPath = projectPath;
        
            ProjectImportDialog dialog = new(project.ToArray(), projectPath);
            RenderTask? task = await dialog.ShowDialog<RenderTask?>(this);
        
            return task;
        }
        
        return null;
    }

    private async void Launch_OnClick(object sender, RoutedEventArgs e) {
        var aggregatedTasks = ViewModel.Tasks.Aggregate(new List<RenderThread>(), (threads, task) => {
            threads.AddRange(task.Enqueue());
            return threads;
        });
        
        _renderingWindow = new(aggregatedTasks);

        await Task.WhenAll([
            _renderingWindow.Start(Settings.Current.ThreadsRenderMode, Settings.Current.ThreadsLimit),
            _renderingWindow.ShowDialog(this)
        ]);
    }

    private async void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
        SettingsWindow settings = new();
        await settings.ShowDialog(this);
    }

    private async void NewTaskEmpty_OnClick(object? sender, RoutedEventArgs e) {
        var aep = await this.ShowOpenFileDialogAsync(
            [ new("After Effects project", "*.aep") ]
        );

        if (aep != null && aep.Count != 0 && aep.First().TryGetLocalPath() is { } prgPath) {
            TaskEditor editor = new(new RenderTask {
                Project = prgPath,
                Output = ""
            });
            
            RenderTask? result = await editor.ShowDialog<RenderTask?>(this);
            if (result != null) {
                ViewModel.Tasks.Add(result);
            }
        }
    }

    private void MoveTaskUp_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        ViewModel.MoveTaskUp(ViewModel.GetTaskById(int.Parse($"{btn.Tag}")));

    }

    private void MoveTaskDown_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        ViewModel.MoveTaskDown(ViewModel.GetTaskById(int.Parse($"{btn.Tag}")));
    }

    private void RemoveTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        ViewModel.Tasks.Remove(ViewModel.GetTaskById(int.Parse($"{btn.Tag}")));
    }

    // TODO: questionable need for this...
    private void DuplicateTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        var task = ViewModel.GetTaskById(int.Parse($"{btn.Tag}"));
        ViewModel.Tasks.Insert(ViewModel.Tasks.IndexOf(task) + 1, task.Clone());
    }
    
    private async void Composition_OnDoubleTapped(object? sender, TappedEventArgs e) {
        if (sender is not ListBoxItem { DataContext: Composition comp } lb) return;
        if (lb.Parent?.Parent is not ListBox bx || int.TryParse($"{bx.Tag}", out var id) == false) return;
        var task = ViewModel.GetTaskById(id);
        TaskEditor editor = new(task) {
            EditorCarousel = {
                SelectedIndex = 1
            },
            CompList = {
                SelectedIndex = task.Compositions.IndexOf(comp)
            }
        };
        var newTask = await editor.ShowDialog<RenderTask?>(this);
        if (newTask is not null) ViewModel.Tasks[ViewModel.Tasks.IndexOf(task)] = newTask;
    }
    
    private async void EditTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        var task = ViewModel.GetTaskById(int.Parse($"{btn.Tag}"));
        TaskEditor editor = new(task, true);
        var newTask = await editor.ShowDialog<RenderTask?>(this);
        if (newTask != null) ViewModel.Tasks[ViewModel.Tasks.IndexOf(task)] = newTask;
    }

    private async void QueueButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_renderingWindow != null) await _renderingWindow.ShowDialog(this);
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
        Settings.Current.Save();
    }

    private async void OutputModuleEditorMenuItem_OnClick(object? sender, EventArgs e) {
        var omEditorWindow = new OutputModuleEditorWindow();
        await omEditorWindow.ShowDialog(this);
    }

    private async void MainWindow_OnOpened(object? sender, EventArgs e) {
        ViewModel.Update = (await Helpers.CheckForUpdates())?.version;
    }
}