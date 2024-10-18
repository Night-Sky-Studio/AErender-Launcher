using System;
using System.Collections.Generic;
using AErenderLauncher.Classes.Extensions;

namespace AErenderLauncher.Classes.Rendering;

public class OutputModule(string module, string mask, bool isImported = false) : ReactiveObject, 
    IEquatable<OutputModule> {
    private string _module = module;
    private string _mask = mask;
    
    public string Module { get => _module; set => RaiseAndSetIfChanged(ref _module, value); }
    public string Mask { get => _mask; set => RaiseAndSetIfChanged(ref _mask, value); }
    public bool IsImported { get; set; } = isImported;
    
    public static readonly List<OutputModule> DefaultModules = [
        new ("Lossless",                "[compName].[fileExtension]"),
        new ("AIFF 48kHz",              "[compName].[fileExtension]"),
        new ("Alpha Only",              "[compName].[fileExtension]"),
        new ("AVI DV NTSC 48kHz",       "[compName].[fileExtension]"),
        new ("AVI DV PAL 48kHz",        "[compName].[fileExtension]"),
        new ("Lossless with Alpha",     "[compName].[fileExtension]"),
        new ("Multi-Machine Sequence",  "[compName]_[#####].[fileExtension]"),
        new ("Photoshop",               "[compName]_[#####].[fileExtension]"),
        new ("Save Current Preview",    "[compName].[fileExtension]"),
        new ("TIFF Sequence with Alpha","[compName]_[#####].[fileExtension]")
    ];
    
    public static implicit operator OutputModule? (Dictionary<string, object> module) {
        var mask = module["Mask"].AsOrDefault<string>();
        var moduleName = module["Module"].AsOrDefault<string>();
        var isImported = module["IsImported"].AsOrDefault<bool>();

        if (mask is null || moduleName is null)
            return null;
        
        return new (moduleName, mask, isImported);
    }
    
    public string DisplayName => Module + (IsImported? " (imported)" : "");

    public bool Equals(OutputModule? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Module == other.Module && Mask == other.Mask && IsImported == other.IsImported;
    }
}