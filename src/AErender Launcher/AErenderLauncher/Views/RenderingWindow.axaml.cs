using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Views;

public class RenderThreadWrapper {
    public RenderThread Thread { get; set; }
    // public RenderProgress Progress { get; set; }
}

public partial class RenderingWindow : Window {
    ObservableCollection<RenderThreadWrapper> Threads { get; set; } = new ();

    private void Init() {
        InitializeComponent();
                
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions = Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*,144") : new RowDefinitions("32,32,*,144");
    }
    
    public RenderingWindow() { 
        Init();
    }

    public RenderingWindow(IList<RenderThread> threads) {
        foreach (var thread in threads)
            Threads.Add(new RenderThreadWrapper { Thread = thread });
    }
    
}