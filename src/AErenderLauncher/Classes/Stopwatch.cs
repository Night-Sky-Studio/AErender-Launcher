using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AErenderLauncher.Classes;

public partial class Stopwatch : ObservableObject {
    [ObservableProperty]
    private TimeSpan _elapsedTime;
    
    [ObservableProperty]
    private bool _isRunning;
    
    private readonly DispatcherTimer _timer = new ();

    public Stopwatch() {
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    public void Start() {
        if (!IsRunning) {
            IsRunning = true;
            _timer.Start();
        }
    }

    public void Stop() {
        if (IsRunning) {
            IsRunning = false;
            _timer.Stop();
        }
    }

    public void Reset() {
        Stop();
        ElapsedTime = TimeSpan.Zero;
    }

    private void Timer_Tick(object? sender, EventArgs e) {
        ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
    }
}