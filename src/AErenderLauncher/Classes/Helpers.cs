using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using AErenderLauncher.Classes.System;
using Newtonsoft.Json;
using Semver;

namespace AErenderLauncher.Classes;

public enum OS {
    Windows,
    macOS
}

public static class Helpers {
    public static OS Platform = OperatingSystem.IsWindows() ? OS.Windows : OS.macOS;//AvaloniaLocator.Current.GetService<IRuntimePlatform>()?.GetRuntimeInfo();

    public static string? GetPackageVersionStringDarwin(string path) {
        string plistPath = "";
        // macos app versions are stored in the plist file
        if (path.EndsWith(".app"))
            plistPath = Path.Combine(path, "Contents", "Info.plist");


        if (path.EndsWith(".plist"))
            plistPath = path;


        if (plistPath != "" && File.Exists(plistPath)) {
            XmlDocument plist = new XmlDocument();
            plist.Load(plistPath);
            return plist
                .SelectSingleNode("/plist/dict/key[text() = 'CFBundleShortVersionString']/following-sibling::string[1]")
                ?.InnerText;
        }

        return null;
    }

    public static bool IsFolder(string path) => Directory.Exists(path) && !File.Exists(path);
    public static bool IsFile(string path) => File.Exists(path) && !Directory.Exists(path);


    public static string GetCurrentDirectory(string path) {
        DirectoryInfo dir = new(path);

        return IsFolder(path)
            ? dir.FullName
            : dir.Parent!.FullName;
    }

    public static string GetCurrentDirectoryName(string path) {
        return new DirectoryInfo(GetCurrentDirectory(path)).Name;
    }

    public static long GetPlatformMemory() {
        return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
    }

    public static int GetAvailableCores() => Environment.ProcessorCount;

    public static async Task<(SemVersion version, string downloadUrl)?> CheckForUpdates() {
        const string apiUrl = "https://api.github.com/repos/{0}/{1}/releases";
        
        var client = new HttpClient {
            DefaultRequestHeaders = {
                { "User-Agent", $"AErender-Launcher/{App.Version} ({Platform}; .NET {Environment.Version})" }
            }
        };
        
        var response = await client.GetAsync(new Uri(string.Format(apiUrl, "Night-Sky-Studio", "AErender-Launcher")));

        if (!response.IsSuccessStatusCode) return null;
        
        var content = await response.Content.ReadAsStringAsync();
        var release = JsonConvert.DeserializeObject<List<GitHub.Release>>(content);

        if (release is null) return null;

        var latest = SemVersion.Parse(release.First().TagName, style: SemVersionStyles.AllowLowerV);
        
        if (App.Version.WithoutMetadata().ComparePrecedenceTo(latest) == -1) {
            return (
                latest,
                release.First().Assets.First(a => a.Name.Contains(Platform == OS.Windows ? "Windows" : "macOS"))
                    .BrowserDownloadUrl
            );
        }

        return null;
    }
}