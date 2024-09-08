using System.Collections.Generic;

namespace AErenderLauncher.Classes.Rendering;

public static class OMListExtensions {
    public static int IndexOf(this IList<OutputModule> list, string Module) {
        for (int i = 0; i < list.Count; i++) {
            if (list[i].Module == Module) {
                return i;
            }
        }

        return -1;
    }
}

public record struct OutputModule {
    public string Module;
    public string Mask;
    public bool IsImported;

    public static implicit operator OutputModule(Dictionary<string, object> module) {
        return new () { Mask = module["Mask"] as string ?? "", Module = module["Module"] as string ?? "", IsImported = module["IsImported"] as bool? ?? false };
    }
    
    // public static bool operator==(OutputModule a, OutputModule b) {
    //     return a.Module == b.Module && a.Mask == b.Mask && a.IsImported == b.IsImported;
    // }
    //
    // public static bool operator !=(OutputModule a, OutputModule b) {
    //     return !(a == b);
    // }
    
    public readonly string DisplayName => Module + (IsImported? " (imported)" : "");
}