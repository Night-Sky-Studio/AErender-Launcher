using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;

namespace AErenderLauncher.Views;

public partial class RenderingWindow : Window {
    public ObservableCollection<RenderThread> Threads { get; set; } = new ();

    private void Init() {
        InitializeComponent();
                
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,144") : new RowDefinitions("32,32,*,144");
    }
    
    public RenderingWindow() { 
        Init();
    }

    public RenderingWindow(IList<RenderThread> threads) {
        Init();
        Threads.AddRange(threads);
    }

    public async Task Start() {
        // Starting one by one
        // foreach (var thread in Threads) await thread.StartAsync();
        
        // Starting all at once
        await Task.WhenAll(Threads.Select(thread => thread.StartAsync()));
    }

    private void AbortRendering_OnClick(object? sender, RoutedEventArgs e) {
        // Kill threads
        foreach (var thread in Threads) thread.Abort();
        // additionally, kill AfterFX.com
        Process.GetProcessesByName(Helpers.Platform == OS.Windows ? "AfterFX.com" : "aerendercore").ToList().ForEach(p => p.Kill());
    }
}