using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
}