using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AErenderLauncher.Classes.Rendering;

namespace AErenderLauncher.Classes.Extensions;

public static class CollectionExtensions {
    
    public static string ToString<T>(this IList<T> list) {
        string result = "[";
        for (int i = 0; i < list.Count; i++) {
            if (i > 0 && i < list.Count) {
                result += ", ";
            }
            result += list[i]?.ToString() ?? "(null)";
        }
        result += "]";

        return result;
    }
    
    public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }
    
    public static RenderTask GetTaskById(this IEnumerable<RenderTask> tasks, int id) => tasks.First(x => x.ID == id);
    
    public static Composition GetCompositionByName(this IEnumerable<Composition> compositions, string name) => compositions.First(x => x.CompositionName == name);
}