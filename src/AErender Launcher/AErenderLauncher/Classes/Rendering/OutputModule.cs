using System.Collections.Generic;

namespace AErenderLauncher.Classes.Rendering; 

public struct OutputModule {
    public string Module;
    public string Mask;
    public bool IsImported;

    public static implicit operator OutputModule(Dictionary<string, object> module) {
        return new OutputModule() { Mask = module["Mask"] as string ?? "", Module = module["Module"] as string ?? "", IsImported = module["IsImported"] as bool? ?? false };
    }
    
    public static bool operator==(OutputModule a, OutputModule b) {
        return a.Module == b.Module && a.Mask == b.Mask && a.IsImported == b.IsImported;
    }

    public static bool operator !=(OutputModule a, OutputModule b) {
        return !(a == b);
    }
    
    public readonly string DisplayName => Module + (IsImported? " (imported)" : "");
}