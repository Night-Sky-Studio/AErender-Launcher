using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;

namespace AErenderLauncher.ViewModels;

public class RenderingViewModel : ReactiveObject {
    private long _totalFrames = 1;
    public long TotalFrames {
        get => _totalFrames;
        set => RaiseAndSetIfChanged(ref _totalFrames, value);
    }
    
    private long _currentFrames = 0;
    public long CurrentFrames {
        get => _currentFrames;
        set => RaiseAndSetIfChanged(ref _currentFrames, value);
    }
    
    private string _progressString = "Waiting for aerender...";
    public string ProgressString {
        get => _progressString;
        set => RaiseAndSetIfChanged(ref _progressString, value);
    }
    
    private double _progressValue = 0;
    public double ProgressValue {
        get => _progressValue;
        set => RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public void ResetProgress() {
        TotalFrames = 1;
        CurrentFrames = 0;
        ProgressString = "Waiting for aerender...";
        ProgressValue = 0;
    }
    // nested viewmodels, let's go
    public ObservableCollection<RenderThread> Threads { get; set; } = new ();
    public ObservableCollection<RenderThread> Queue { get; set; } = new ();
    public Stopwatch SW { get; } = new ();

    public RenderingViewModel() {
        Threads.CollectionChanged += ThreadsOnCollectionChanged;
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
            TotalFrames += Threads.Sum(thread => thread.EndFrame != uint.MaxValue ? thread.EndFrame : 0);
        }
        if (e.PropertyName == "CurrentFrame") {
            CurrentFrames = Threads.Sum(thread => thread.CurrentFrame != uint.MaxValue ? thread.CurrentFrame : 0);
        }
        if (CurrentFrames == TotalFrames)
            ProgressString = "Rendering finished!";
        else if (CurrentFrames < TotalFrames && TotalFrames != 1) {
            ProgressString = $"{CurrentFrames} / {TotalFrames}";
            ProgressValue = Math.Round((double)CurrentFrames / TotalFrames, 2);
        }
    }
}