using System;
using System.Collections.Specialized;
using System.Diagnostics;
using AErenderLauncher.Classes.System;
using Avalonia.Threading;

namespace AErenderLauncher.Classes.Rendering;

public class RenderThread(string executable, string command) : ConsoleThread(executable, command) {
    public int ID { get; set; }
    public string Name { get; set; } = "";
    public RenderProgress? Progress { get; set; } = new ();

    public event Action<RenderProgress?, string>? OnProgressChanged;

    private AerenderFrameData? _frameData = null;
    private Timecode? _duration = null;
    private double? _framerate = null;
    
    protected override void StdoutOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        string data = $"{e.NewItems?[0]}";

        if (data.StartsWith("PROGRESS:  ")) {
            _frameData = AerenderParser.ParseFrameData(data);
            _duration ??= AerenderParser.ParseDuration(data);
            _framerate ??= AerenderParser.ParseFramerate(data);
            
            if (_frameData != null && _duration != null && _framerate != null) {
                Progress = new RenderProgress {
                    CurrentFrame = _frameData.Value.Frame,   // weird nullable types flex...
                    EndFrame = _duration.Value.ToFrames(_framerate.Value)
                };
            }
        }
        
        OnProgressChanged?.Invoke(Progress, $"{e.NewItems?[0]}");
    }
    
    // protected override void OnErrorReceived(string data) { }

    // protected override void OnOutputReceived(string data) { }
}