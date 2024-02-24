using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System;
using AErenderLauncher.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DynamicData;
using TaskExtensions = AErenderLauncher.Classes.Extensions.TaskExtensions;

namespace AErenderLauncher.Views;

public partial class RenderingWindow : Window {
    public ObservableCollection<RenderThread> Threads { get; set; } = new ();
    public ObservableCollection<RenderThread> Queue { get; set; } = new ();

    private int _cr = 0;

    private int _currentlyRendering {
        get => _cr;
        set {
            _cr = value;
            Debug.WriteLine($"Currently rendering: {_cr}");
        }
    }

    private void Init() {
        InitializeComponent();
                
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,144") : new RowDefinitions("32,32,*,144");
        Threads.CollectionChanged += ThreadsOnCollectionChanged;
    }

    private async void ThreadsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    }

    public RenderingWindow() { 
        Init();
    }

    public RenderingWindow(IList<RenderThread> threads) {
        Init();
        Queue.AddRange(threads);
    }
    
    public async Task Start() {
        // Starting one by one
        // foreach (var thread in Threads) await thread.StartAsync();
        
        // Starting all at once
        //await Task.WhenAll(Threads.Select(thread => thread.StartAsync()));

        await StartTiled();
    }

    private async Task StartAll() {
        // ActivateThreads(Threads);
        // await Task.WhenAll(Queue.Select(thread => thread.StartAsync()));
    }
    
    private async Task StartOneByOne() {
        // foreach (var thread in Queue) {
        //     ActivateThreads([thread]);
        //     await thread.StartAsync();
        // }
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
                }
            });

            // Wait for any task to complete.
            var completedTaskFactory = await Task.WhenAny(runningTasks.Select(tf => tf.CompletionSource));
            // callback(completedTaskFactory);
            Debug.WriteLine($"Task {taskFactories.First(t => t.Value.CompletionSource == completedTaskFactory).Key} completed");
            
            // Remove the completed task.
            runningTasks.Remove(runningTasks.First(tf => tf.CompletionSource == completedTaskFactory));

            // If there are waiting tasks, start a new one.
            if (waitingTasks.Count > 0) {
                runningTasks.Add(waitingTasks[0]);
                waitingTasks.RemoveAt(0);
            }
        }
    }
    
    private async Task StartTiled(int limit = 4) {
        await StartTiledRendering(Queue, limit);

        // while (Queue.Any()) {
        //     if (_currentlyRendering == limit) continue;
        //     TODO: Restart errored threads
        //     // Add threads that finished with errors to the end of the queue
        //     Threads.Where(thread => thread is { Finished: true, GotError: true }).ToList().ForEach(thread => {
        //         Queue.Add(thread);
        //         Threads.Remove(thread);
        //     });
        // }
    }

    private void AbortRendering_OnClick(object? sender, RoutedEventArgs e) {
        // Kill threads
        foreach (var thread in Queue) thread.Abort();
        Queue.Clear();
        Threads.Clear();
        // additionally, kill AfterFX.com
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore").ToList().ForEach(p => p.Kill());
    }
}