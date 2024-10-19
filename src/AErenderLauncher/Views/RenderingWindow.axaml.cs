using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System;
using AErenderLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using ThreadState = AErenderLauncher.Enums.ThreadState;

namespace AErenderLauncher.Views;

public partial class RenderingWindow : Window {
    public RenderingViewModel ViewModel { get; } = new ();

    private readonly CancellationTokenSource _cts = new ();
    private CancellationToken CancelToken => _cts.Token;

    private void Init() {
        InitializeComponent();
        
        DataContext = ViewModel;
                
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,144") : new RowDefinitions("32,32,*,144");
        
        ViewModel.SW.Reset();
        ViewModel.ResetProgress();
    }

    public RenderingWindow() { 
        Init();
    }

    public RenderingWindow(IList<RenderThread> threads) {
        Init();
        ViewModel.Queue.AddRange(threads);
    }
    
    public async Task Start(RenderingMode mode = RenderingMode.Tiled, int limit = 4) {
        ViewModel.SW.Start();
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
        ViewModel.SW.Stop();
    }

    private async Task StartAll() {
        ViewModel.Threads.AddRange(ViewModel.Queue);
        ViewModel.Queue.Clear();
        await Task.WhenAll(ViewModel.Threads.Select(thread => thread.StartAsync()));
    }
    
    private async Task StartOneByOne() {
        foreach (var thread in ViewModel.Queue) {
            await thread.StartAsync();
        }
    }
    
    private async Task StartTiledRendering(IEnumerable<RenderThread> threads, int maxDegreeOfParallelism = 4) {
        var threadsList = threads.ToList();

        // If there are fewer threads than we allow
        // just start all of them
        if (threadsList.Count <= maxDegreeOfParallelism) {
            await StartAll();
            return;
        }
        
        var waitingTasks = new List<VoidTaskFactory>();
        for (int i = 0; i < threadsList.Count; i++) {
            waitingTasks.Add(new (threadsList[i].Id, threadsList[i].StartAsync, CancelToken));
        }
        
        var runningTasks = new List<VoidTaskFactory>(maxDegreeOfParallelism);

        // Start the first 'maxDegreeOfParallelism' tasks.
        for (int i = 0; i < maxDegreeOfParallelism && waitingTasks.Count > 0; i++) {
            runningTasks.Add(waitingTasks[0]);
            waitingTasks.RemoveAt(0);
        }

        // While there are running tasks.
        while (runningTasks.Count > 0) {
            foreach (var tf in runningTasks) {
                if (tf.TryStart()) {
                    var idx = ViewModel.Queue.IndexOf(ViewModel.Queue.First(rt => rt.Id == tf.Id));
                    ViewModel.Threads.Add(ViewModel.Queue[idx]);
                    ViewModel.Queue.RemoveAt(idx); 
                }
            }

            // Wait for any task to complete.
            var completedTaskFactory = await Task.WhenAny(runningTasks.Select(tf => tf.CompletionSource));
            // callback(completedTaskFactory);
            //Debug.WriteLine($"Task {taskFactories.First(t => t.Value.CompletionSource == completedTaskFactory).Key} completed");
            
            // Remove the completed task.
            runningTasks.Remove(runningTasks.First(tf => tf.CompletionSource.Id == completedTaskFactory.Id));

            // Restart failed tasks.
            ViewModel.Threads.Where(thread => thread.State == ThreadState.Error).ToList().ForEach(thread => {
                ViewModel.Queue.Add(thread);
                ViewModel.Threads.Remove(thread);
            });
            
            // If there are waiting tasks, start a new one.
            if (waitingTasks.Count <= 0) continue;
            runningTasks.Add(waitingTasks[0]);
            waitingTasks.RemoveAt(0);
        }
    }
    
    private async Task StartTiled(int limit = 4) {
        await StartTiledRendering(ViewModel.Queue, limit);
    }

    private void AbortRendering_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.SW.Stop();
        
        _cts.Cancel();
        
        // Kill threads
        foreach (var thread in ViewModel.Queue) thread.Abort();
        foreach (var thread in ViewModel.Threads) thread.Abort();
        
        ViewModel.Queue.Clear();
        ViewModel.Threads.Clear();
        // additionally, kill AE
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore").ToList().ForEach(p => p.Kill());
        // Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "aerender.exe" : "aerender").ToList().ForEach(p => p.Kill());
    }
}