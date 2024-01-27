namespace AErenderLauncher.Classes.Rendering;

public struct Composition {
    public string CompositionName { get; set; }
    public FrameSpan Frames { get; set; }
    private int _split = 1;
    public int Split { 
        get => _split; 
        set => _split = value < 1 ? 1 : value;
    }
    public FrameSpan[] SplitFrameSpans => Frames.Split(Split);

    public Composition(string Name, FrameSpan Frames, int Split) {
        this.CompositionName = Name;
        this.Frames = Frames;
        this._split = Split;
    }
    
    public override string ToString() {
        return $"Comp({CompositionName}, [{Frames.StartFrame}, {Frames.EndFrame}], {Split})";
    }
}