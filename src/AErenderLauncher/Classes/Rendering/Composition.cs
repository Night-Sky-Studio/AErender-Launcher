using System;
using AErenderLauncher.Interfaces;

namespace AErenderLauncher.Classes.Rendering;

public class Composition : ReactiveObject, ICloneable<Composition> {
    private string _compositionName = "";
    public string CompositionName {
        get => _compositionName; 
        set => RaiseAndSetIfChanged(ref _compositionName, value);
    }
    private FrameSpan _frames = new(0, 0);
    public FrameSpan Frames {
        get => _frames; 
        set => RaiseAndSetIfChanged(ref _frames, value);
    }
    private uint _split = 1;
    public uint Split { 
        get => _split; 
        set => RaiseAndSetIfChanged(ref _split, value < 1 ? 1 : value);
    }
    public FrameSpan[] SplitFrameSpans => Frames.Split(Split);

    public Composition() { }
    public Composition(string name, FrameSpan frames, uint split) {
        CompositionName = name;
        Frames = frames;
        _split = split;
    }
    
    public override string ToString() {
        return $"Comp({CompositionName}, [{Frames.StartFrame}, {Frames.EndFrame}], {Split})";
    }
    public Composition Clone() {
        return new Composition(CompositionName, Frames, Split);
    }
}