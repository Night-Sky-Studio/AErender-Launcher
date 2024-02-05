namespace AErenderLauncher.Classes.Rendering;

public struct RenderProgress(uint CurrentFrame, uint EndFrame) {
    public uint CurrentFrame { get; set; } = CurrentFrame;
    public uint EndFrame { get; set; } = EndFrame;
    public readonly float Percentage => (float)CurrentFrame / EndFrame;

    public bool GotError => CurrentFrame == uint.MaxValue && EndFrame == uint.MaxValue;
    public bool WaitingForAerender => CurrentFrame == 0 && EndFrame == 0;
    public bool Finished => CurrentFrame == EndFrame;
    
    public override string ToString() {
        return $"{CurrentFrame} / {EndFrame} ({Percentage * 100}%)";
    }
}