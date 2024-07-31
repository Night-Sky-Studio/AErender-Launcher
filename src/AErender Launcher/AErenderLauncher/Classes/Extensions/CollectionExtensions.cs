using System;
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
    
    public static IList<T> RemoveAll<T>(this IList<T> list, Predicate<T> match) {
        for (int i = 0; i < list.Count; i++) {
            if (!match(list[i])) continue;
            list.RemoveAt(i);
            i--;
        }

        return list;
    }
    
    public static bool TryCast<T>(this IList list, out IList<T>? result) {
        try {
            result = list.Cast<T>().ToList();
            return true;
        } catch {
            result = null;
            return false;
        }
    }

    public static void AddMany<T>(this IList<T> list, params T?[] items) {
        foreach (T? item in items) {
            if (item is null) continue;
            list.Add(item);
        }
    }
}