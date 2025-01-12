using System;
using AErenderLauncher.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AErenderLauncher.Classes.Rendering;

public partial class Composition : ObservableObject, ICloneable<Composition> {
    [ObservableProperty]
    private string _compositionName = "";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(SplitFrameSpans))]
    private FrameSpan _frames = new(0, 0);
    
    // No workarounds for custom value setter has been found, yet...
    private uint _split = 1;
    public uint Split { 
        get => _split; 
        set => SetProperty(ref _split, value < 1 ? 1 : value);
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