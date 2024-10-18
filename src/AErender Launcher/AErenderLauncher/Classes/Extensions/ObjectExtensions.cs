using System;
using Newtonsoft.Json;

namespace AErenderLauncher.Classes.Extensions;

public static class ObjectExtensions {
    public static T? AsOrDefault<T>(this object obj, T? defaultValue = default) {
        try {
            return (T)obj;
        } catch {
            return defaultValue;
        }
    }

    public static T Clone<T>(this T source) {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!; // xd
    }

    public static bool JsonEquals(this object source, object other) {
        return JsonConvert.SerializeObject(other) == JsonConvert.SerializeObject(source);
    }
}