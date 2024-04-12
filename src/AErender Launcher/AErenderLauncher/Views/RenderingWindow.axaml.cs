using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DynamicData;
using ThreadState = AErenderLauncher.Classes.System.ConsoleThread.ThreadState;
using ReactiveObject = AErenderLauncher.Classes.ReactiveObject;
using Stopwatch = AErenderLauncher.Classes.Stopwatch;

using static AErenderLauncher.App;

namespace AErenderLauncher.Views;

public class Progress : ReactiveObject {
    private long _totalFrames = 1;
    private long _currentFrames = 0;
    private string _progressString = "Waiting for aerender...";
    private double _progressValue = 0;
    
    public long TotalFrames {
        get => _totalFrames;
        set => RaiseAndSetIfChanged(ref _totalFrames, value);
    }
    
    public long CurrentFrames {
        get => _currentFrames;
        set => RaiseAndSetIfChanged(ref _currentFrames, value);
    }
    
    public string ProgressString {
        get => _progressString;
        set => RaiseAndSetIfChanged(ref _progressString, value);
    }

    public double ProgressValue {
        get => _progressValue;
        set => RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public void Reset() {
        TotalFrames = 1;
        CurrentFrames = 0;
        ProgressString = "Waiting for aerender...";
        ProgressValue = 0;
    }
}

public partial class RenderingWindow : Window {
    public ObservableCollection<RenderThread> Threads { get; set; } = new ();
    public ObservableCollection<RenderThread> Queue { get; set; } = new ();

    public Progress TotalProgress { get; } = new ();
    public Stopwatch SW { get; } = new ();

    private void Init() {
        InitializeComponent();
                
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,144") : new RowDefinitions("32,32,*,144");
        Threads.CollectionChanged += ThreadsOnCollectionChanged;
        
        SW.Reset();
        TotalProgress.Reset();
    }

    private void ThreadsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            foreach (var thread in e.NewItems)
                if (thread is RenderThread renderThread)
                    renderThread.PropertyChanged += ThreadOnPropertyChanged;

        if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            foreach (var thread in e.OldItems) 
                if (thread is RenderThread renderThread) 
                    renderThread.PropertyChanged -= ThreadOnPropertyChanged;
    }

    private void ThreadOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == "EndFrame") {
            TotalProgress.TotalFrames += Threads.Sum(thread => thread.EndFrame != uint.MaxValue ? thread.EndFrame : 0);
        }
        if (e.PropertyName == "CurrentFrame") {
            TotalProgress.CurrentFrames = Threads.Sum(thread => thread.CurrentFrame != uint.MaxValue ? thread.CurrentFrame : 0);
        }
    }

    public RenderingWindow() { 
        Init();
    }

    public RenderingWindow(IList<RenderThread> threads) {
        Init();
        Queue.AddRange(threads);
    }
    
    public async Task Start(RenderingMode mode = RenderingMode.Tiled, int limit = 4) {
        SW.Start();
        switch (mode) {
            case RenderingMode.Tiled:
                await StartTiled(limit);
                break;
            case RenderingMode.Queue:
                await StartOneByOne();
                break;
            case RenderingMode.AllAtOnce:
                await StartAll();
                break;
        }
        SW.Stop();
    }

    private async Task StartAll() {
        await Task.WhenAll(Queue.Select(thread => thread.StartAsync()));
    }
    
    private async Task StartOneByOne() {
        foreach (var thread in Queue) {
            await thread.StartAsync();
        }
    }


    public async Task StartTiledRendering(IEnumerable<RenderThread> threads, int maxDegreeOfParallelism = 4) {
        var threadsList = threads.ToList();
        Dictionary<int, VoidTaskFactory> taskFactories = new ();
        for (int i = 0; i < threadsList.Count; i++) {
            taskFactories.Add(i, new VoidTaskFactory(threadsList[i].StartAsync));
        }
        
        var waitingTasks = new List<VoidTaskFactory>(taskFactories.Select(tfi => tfi.Value).ToList());
        var runningTasks = new List<VoidTaskFactory>(maxDegreeOfParallelism);

        // Start the first 'maxDegreeOfParallelism' tasks.
        for (int i = 0; i < maxDegreeOfParallelism && waitingTasks.Count > 0; i++) {
            runningTasks.Add(waitingTasks[0]);
            waitingTasks.RemoveAt(0);
        }

        // While there are running tasks.
        while (runningTasks.Count > 0) {
        
            runningTasks.ForEach(tf => {
                if (tf.TryStart()) {
                    Threads.Add(Queue[taskFactories.First(t => t.Value == tf).Key]);
                    Queue.RemoveAt(taskFactories.First(t => t.Value == tf).Key);
                }
            });

            // Wait for any task to complete.
            var completedTaskFactory = await Task.WhenAny(runningTasks.Select(tf => tf.CompletionSource));
            // callback(completedTaskFactory);
            //Debug.WriteLine($"Task {taskFactories.First(t => t.Value.CompletionSource == completedTaskFactory).Key} completed");
            
            // Remove the completed task.
            runningTasks.Remove(runningTasks.First(tf => tf.CompletionSource == completedTaskFactory));

            // Restart failed tasks.
            Threads.Where(thread => thread.State == ThreadState.Error).ToList().ForEach(thread => {
                Queue.Add(thread);
                Threads.Remove(thread);
            });
            
            // If there are waiting tasks, start a new one.
            if (waitingTasks.Count > 0) {
                runningTasks.Add(waitingTasks[0]);
                waitingTasks.RemoveAt(0);
            }
        }
    }
    
    private async Task StartTiled(int limit = 4) {
        await StartTiledRendering(Queue, limit);
    }

    private void AbortRendering_OnClick(object? sender, RoutedEventArgs e) {
        // Kill threads
        foreach (var thread in Queue) thread.Abort();
        SW.Stop();
        Queue.Clear();
        Threads.Clear();
        // additionally, kill AE
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore").ToList().ForEach(p => p.Kill());
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "aerender.exe" : "aerender").ToList().ForEach(p => p.Kill());
    }
}