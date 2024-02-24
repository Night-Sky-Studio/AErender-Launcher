using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AErenderLauncher.Classes.System;
using Avalonia.Threading;
using ReactiveUI;

namespace AErenderLauncher.Classes.Rendering;

public class RenderThread(string executable, string command) : ConsoleThread(executable, command) {
    public int ID { get; set; }
    public string Name { get; set; } = "";

    // private RenderProgress _progress = new (0, 0);
    //
    // public RenderProgress Progress {
    //     get => _progress;
    //     set => RaiseAndSetIfChanged(ref _progress, value);
    // }
    private uint _currentFrame = 0;
    public uint CurrentFrame { 
        get => _currentFrame;
        set => RaiseAndSetIfChanged(ref _currentFrame, value);
    }
    private uint _endFrame = 0;
    public uint EndFrame {
        get => _endFrame; 
        set => RaiseAndSetIfChanged(ref _endFrame, value);
    }
    
    private string _log = "";
    public string Log {
        get => _log;
        set => RaiseAndSetIfChanged(ref _log, value);
    }
    public bool GotError => CurrentFrame == uint.MaxValue && EndFrame == uint.MaxValue;
    public bool WaitingForAerender => CurrentFrame == 0 && EndFrame == 0;
    public bool Finished => CurrentFrame == EndFrame;
    // public event Action<RenderProgress?, string>? OnProgressChanged;

    private AerenderFrameData? _frameData = null;
    private Timecode? _duration = null;
    private double? _framerate = null;
    
    protected override void StdoutOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        string data = $"{e.NewItems?[0]}";
        Log += data + Environment.NewLine;

        if (!GotError && data.StartsWith("PROGRESS:  ")) {
            _frameData = AerenderParser.ParseFrameData(data);
            _duration ??= AerenderParser.ParseDuration(data);
            _framerate ??= AerenderParser.ParseFramerate(data);
            
            if (_frameData != null && _duration != null && _framerate != null) {
                CurrentFrame = _frameData.Value.Frame;   // weird nullable types flex...
                EndFrame = _duration.Value.ToFrames(_framerate.Value);
            }
        }

        if (data.Contains("Finished composition") || State == ThreadState.Finished) {
            CurrentFrame = EndFrame;
            Abort();
        }

        if ((data.Contains("error", StringComparison.CurrentCultureIgnoreCase) || State == ThreadState.Error) && State != ThreadState.Finished) {
            CurrentFrame = uint.MaxValue;
            EndFrame = uint.MaxValue;
            Abort();
        }
        
        // OnProgressChanged?.Invoke(null, $"{e.NewItems?[0]}");
    }
    
    // protected override void OnErrorReceived(string data) { }

    // protected override void OnOutputReceived(string data) { }
}