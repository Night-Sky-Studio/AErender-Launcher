using System;
using System.Collections.Specialized;
using System.Diagnostics;
using AErenderLauncher.Classes.System;
using Avalonia.Threading;

namespace AErenderLauncher.Classes.Rendering;

public class RenderThread(string executable, string command) : ConsoleThread(executable, command) {
    public int ID { get; set; }
    public string Name { get; set; } = "";

    private RenderProgress _progress = new ();
    // public RenderProgress Progress => _progress;

    public event Action<RenderProgress, string>? OnProgressChanged;

    private AerenderFrameData? _frameData = null;
    private Timecode? _duration = null;
    private double? _framerate = null;
    
    protected override void StdoutOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        string data = $"{e.NewItems?[0]}";

        if (!_progress.GotError && data.StartsWith("PROGRESS:  ")) {
            _frameData = AerenderParser.ParseFrameData(data);
            _duration ??= AerenderParser.ParseDuration(data);
            _framerate ??= AerenderParser.ParseFramerate(data);
            
            if (_frameData != null && _duration != null && _framerate != null) {
                _progress.CurrentFrame = _frameData.Value.Frame;   // weird nullable types flex...
                _progress.EndFrame = _duration.Value.ToFrames(_framerate.Value);
            }
        }

        if (data.Contains("error", StringComparison.CurrentCultureIgnoreCase)) {
            _progress.CurrentFrame = uint.MaxValue;
            _progress.EndFrame = uint.MaxValue;
        }
        
        OnProgressChanged?.Invoke(_progress, $"{e.NewItems?[0]}");
    }
    
    // protected override void OnErrorReceived(string data) { }

    // protected override void OnOutputReceived(string data) { }
}