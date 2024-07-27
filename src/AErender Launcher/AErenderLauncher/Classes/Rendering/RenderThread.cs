using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using AErenderLauncher.Classes.System;
using ThreadState = AErenderLauncher.Enums.ThreadState;

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
    
    protected override void OnOutputReceived(ConsoleThread? sender, string output) {
        string data = output;
        Log += data + Environment.NewLine;

        if (Log == "")
            throw new SystemException("What the fuck");

        if (!GotError && data.StartsWith("PROGRESS:  ")) {
            _frameData = AerenderParser.ParseFrameData(data);
            _duration ??= AerenderParser.ParseDuration(data);
            _framerate ??= AerenderParser.ParseFramerate(data);
            
            if (_frameData != null && _duration != null && _framerate != null) {
                CurrentFrame = _frameData.Value.Frame;  
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
    }

    protected override void OnStateChanged(ConsoleThread? sender, ThreadState state) {
        Debug.WriteLine($"Thread {ID} state: {state.ToString()}");
        switch (state) {
            case ThreadState.Running:
                if (Log.Contains("Started rendering" + Environment.NewLine))
                    throw new SystemException("What the fuck");
                Log += "Started rendering" + Environment.NewLine;
                break;
            case ThreadState.Finished:
                Log += "Finished rendering" + Environment.NewLine;
                break;
            case ThreadState.Error:
                Log += "Error occurred" + Environment.NewLine;
                break;
            case ThreadState.Stopped:
                Log += "Stopped rendering" + Environment.NewLine;
                break;
        }
    }
    
    // protected override void OnErrorReceived(string data) { }

    // protected override void OnOutputReceived(string data) { }
}