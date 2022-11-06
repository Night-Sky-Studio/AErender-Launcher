using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AErenderLauncher.Classes.System.Extensions; 

public static class ListExtensions {
    public static string ToString(IList list) {
        string result = "[";
        for (int i = 0; i < list.Count; i++) {
            if (i > 0 && i < list.Count) {
                result += ", ";
            }
            result += list[i].ToString();
        }
        result += "]";

        return result;
    }
}