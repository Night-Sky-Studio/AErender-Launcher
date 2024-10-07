using System;
using System.Collections.Generic;
using AErenderLauncher.Classes.Extensions;

namespace AErenderLauncher.Classes.Rendering;

public static class OMListExtensions {
    public static int IndexOf(this IList<OutputModule> list, string module) {
        return list.IndexOf((om) => om.Module == module);
    }
}

public record struct OutputModule {
    public string Module;
    public string Mask;
    public bool IsImported;

    public static readonly List<OutputModule> DefaultModules = [
        new () { Module = "Lossless",                 Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "AIFF 48kHz",               Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "Alpha Only",               Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "AVI DV NTSC 48kHz",        Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "AVI DV PAL 48kHz",         Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "Lossless with Alpha",      Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "Multi-Machine Sequence",   Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
        new () { Module = "Photoshop",                Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
        new () { Module = "Save Current Preview",     Mask = "[compName].[fileExtension]",         IsImported = false },
        new () { Module = "TIFF Sequence with Alpha", Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
    ];
    
    public static implicit operator OutputModule? (Dictionary<string, object> module) {
        var mask = module["Mask"].AsOrDefault<string>();
        var moduleName = module["Module"].AsOrDefault<string>();
        var isImported = module["IsImported"].AsOrDefault<bool>();

        if (mask is null || moduleName is null)
            return null;
        
        return new () { Mask = mask, Module = moduleName, IsImported = isImported };
    }
    
    public readonly string DisplayName => Module + (IsImported? " (imported)" : "");
}