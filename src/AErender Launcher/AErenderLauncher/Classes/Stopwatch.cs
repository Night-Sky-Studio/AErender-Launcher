using System;
using Avalonia.Threading;

namespace AErenderLauncher.Classes;

public class Stopwatch : ReactiveObject {
    private TimeSpan _elapsedTime;
    private bool _isRunning;
    private DispatcherTimer _timer;

    public Stopwatch() {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    public TimeSpan ElapsedTime {
        get => _elapsedTime;
        private set => RaiseAndSetIfChanged(ref _elapsedTime, value);
    }

    public bool IsRunning {
        get => _isRunning;
        private set => RaiseAndSetIfChanged(ref _isRunning, value);
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