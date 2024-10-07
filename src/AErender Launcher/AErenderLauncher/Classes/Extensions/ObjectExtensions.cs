using System;

namespace AErenderLauncher.Classes.Extensions;

public static class ObjectExtensions {
    public static T? AsOrDefault<T>(this object obj, T? defaultValue = default) {
        try {
            return (T)obj;
        } catch {
            return defaultValue;
        }
    }
}