namespace AErenderLauncher.Classes.Rendering; 

public struct Composition {
    public string CompositionName { get; set; }
    public FrameSpan Frames { get; set; }
    public int Split { get; set; }
    public FrameSpan[] SplitFrameSpans => Frames.Split(Split);

    public Composition(string Name, FrameSpan Frames, int Split) {
        this.CompositionName = Name;
        this.Frames = Frames;
        this.Split = Split;
    }
}