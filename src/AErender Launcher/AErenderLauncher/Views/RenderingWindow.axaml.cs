using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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
using DynamicData;

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
        // if (e.Action != NotifyCollectionChangedAction.Add) return;
        // if (e.NewItems == null) return;
        // if (!e.NewItems.TryCast<RenderThread>(out var t) || t is not { } threads) return;
        // _currentlyRendering += threads.Count;
        //
        // // this is dumb
        // //await Task.Factory.ContinueWhenAny(async () => await Task.WhenAll(threads.Select(th => th.StartAsync())));
        //
        // _currentlyRendering -= threads.Count;
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

    private async Task StartThread(RenderThread thread) {
        _currentlyRendering++;
        Threads.Add(thread);
        Queue.Remove(thread);
        await thread.StartAsync();
        _currentlyRendering--;
    }

    private async Task StartTiled(int limit = 4) {
        //! Tiled rendering
        // We have threads [N] and a [queue]
        // 1)   Start first [limit] threads
        // 2)   When any of the thread finishes -
        //      start rendering one from [queue]
        // 3)   If thread errored -
        //      put it in the end of the queue 
        // 4)   Make sure that at each time, unless [queue].Count < limit,
        //      we have [limit] threads rendering at the same time

        // async void ThreadStart() {
        //     var toStart = Queue.Take(limit - _currentlyRendering).ToList();
        //     // Wait for any thread to finish
        //     await Task.WhenAll(toStart.Select(StartThread));
        // }

        // var t = new Thread(ThreadStart);

        var pool = new TaskPool(limit);

        while (Queue.Any()) {
            if (_currentlyRendering == limit) continue;

            pool.Enqueue(async () => {
                var toStart = Queue.Take(limit - _currentlyRendering).ToList();
                await Task.WhenAll(toStart.Select(StartThread));
            });
            

            // await Task.Factory.ContinueWhenAny(toStart.Select(StartThread).ToArray(), task => {
            //     _currentlyRendering--;
            // });
            //await Task.WhenAny(toStart.Select(StartThread)); // TODO: This is not working...

            // Remove all the finished threads without errors
            // Threads.RemoveAll(thread => thread is { Finished: true, GotError: false });

            // Add threads that finished with errors to the end of the queue
            Threads.Where(thread => thread is { Finished: true, GotError: true }).ToList().ForEach(thread => {
                Queue.Add(thread);
                Threads.Remove(thread);
            });
        }
    }

    private void AbortRendering_OnClick(object? sender, RoutedEventArgs e) {
        // Kill threads
        foreach (var thread in Queue) thread.Abort();
        // additionally, kill AfterFX.com
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore").ToList().ForEach(p => p.Kill());
    }
}