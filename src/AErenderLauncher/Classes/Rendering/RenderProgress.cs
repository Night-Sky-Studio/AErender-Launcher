using System;
using System.Reactive;

namespace AErenderLauncher.Classes.Rendering;

public class RenderProgress(uint currentFrame, uint endFrame) : ReactiveObject {
    public uint CurrentFrame { 
        get => currentFrame;
        set => RaiseAndSetIfChanged(ref currentFrame, value);
    }
    public uint EndFrame {
        get => endFrame; 
        set => RaiseAndSetIfChanged(ref endFrame, value);
    }
    public float Percentage => (float)CurrentFrame / EndFrame;

    public bool GotError => CurrentFrame == uint.MaxValue && EndFrame == uint.MaxValue;
    public bool WaitingForAerender => CurrentFrame == 0 && EndFrame == 0;
    public bool Finished => CurrentFrame == EndFrame;
    
    public static RenderProgress Waiting() => new (0, 0);

    public override string ToString() {
        return $"{CurrentFrame} / {EndFrame} ({Percentage * 100}%)";
    }
}