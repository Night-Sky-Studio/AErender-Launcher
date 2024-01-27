using System;
using System.IO;
using System.Xml;

namespace AErenderLauncher.Classes;

public enum OS {
    Windows,
    macOS
}

public static class Helpers {
    public static OS Platform = OperatingSystem.IsWindows() ? OS.Windows : OS.macOS;//AvaloniaLocator.Current.GetService<IRuntimePlatform>()?.GetRuntimeInfo();

    public static string? GetPackageVersionStringDarwin(string path) {
        string plist_path = "";
        // macos app versions are stored in the plist file
        if (path.EndsWith(".app")) 
            plist_path = Path.Combine(path, "Contents", "Info.plist");
        

        if (path.EndsWith(".plist")) 
            plist_path = path;
        

        if (plist_path != "" && File.Exists(plist_path)) {
            XmlDocument plist = new XmlDocument();
            plist.Load(plist_path);
            return plist.SelectSingleNode("/plist/dict/key[text() = 'CFBundleShortVersionString']/following-sibling::string[1]")?.InnerText;
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
}