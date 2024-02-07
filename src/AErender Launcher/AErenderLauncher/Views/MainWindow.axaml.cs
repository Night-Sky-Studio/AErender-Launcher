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
        
        if (aep == null || !aep.Any()) return;
        
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

    private async Task<RenderTask?> ParseProject(string ProjectPath) {
        List<ProjectItem>? project = await AeProjectParser.ParseProjectAsync(ProjectPath);
        
        if (project != null) {
            ApplicationSettings.LastProjectPath = ProjectPath;
        
            ProjectImportDialog dialog = new(project.ToArray(), ProjectPath);
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
        // OpenFileDialog dialog = new() {
        //     AllowMultiple = false,
        //     Directory = ApplicationSettings.DefaultProjectsPath,
        //     Filters = new() {
        //         new() { Name = "After Effects project", Extensions = { "aep" } }
        //     },
        //     Title = "Open After Effects project"
        // };

        var provider = GetTopLevel(this)?.StorageProvider;
        
        IEnumerable<IStorageFile> aep = await provider?.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = false,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(new Uri(ApplicationSettings.DefaultProjectsPath)),
            FileTypeFilter = new List<FilePickerFileType> {
                new ("After effects project") {
                    Patterns = new List<string> { "*.aep" }
                }
            }
        })!;
        
        if (!aep.Any()) return;
        
        // System.Diagnostics.Debug.WriteLine(aep.First());
        
        TaskEditor editor = new(new RenderTask {
            Project = aep.First().Path.AbsolutePath
        });
            
        RenderTask? result = await editor.ShowDialog<RenderTask?>(this);
        if (result != null) {
            Tasks.Add(result);
        }
    }

    //private List<RenderThread> queue;
    
    private async void StartBtnClick(object? sender, RoutedEventArgs e) {
        // queue = _task.Enqueue();
        // TestLog.Text = "";
        // queue[0].OnProgressChanged += (progress, data) => {
        //     TestLog.Text += $"{data}\n";
        //     TestLog.CaretIndex = int.MaxValue;
        //     TestProgressBar.IsIndeterminate = progress == null;
        //     TestProgressBar.Minimum = 0;
        //     TestProgressBar.Maximum = progress?.EndFrame ?? 1;
        //     TestProgressBar.Value = progress?.CurrentFrame ?? 0;
        // };
        // await queue[0].StartAsync();
    }

    private void StopBtnClick(object? sender, RoutedEventArgs e) {
        //queue[0].Abort();
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

    private void Composition_OnDoubleTapped(object? sender, TappedEventArgs e) {
        // TODO: Open task editor on composition screen
        System.Diagnostics.Debug.WriteLine("Double tapped");
    }

    private void EditTask_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        TaskEditor editor = new(Tasks.GetTaskById(int.Parse($"{btn.Tag}")), true);
        editor.ShowDialog(this);
    }

    private async void QueueButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_renderingWindow != null) await _renderingWindow.ShowDialog(this);
    }
}