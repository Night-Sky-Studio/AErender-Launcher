using System.Linq;

namespace AErenderLauncher.Classes.Extensions;

public static class StringExtensions {
    public static string Delete(this string source, string substring) => source.Replace(substring, "");

    public static string Delete(this string source, params string[] substrings) => substrings.Aggregate(source, (current, substring) => current.Delete(substring));
}