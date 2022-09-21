using System.IO;
using System.Xml;
using Avalonia;
using Avalonia.Platform;

namespace AErenderLauncher.Classes; 

public static class Helpers {
    public static OperatingSystemType Platform = AvaloniaLocator.Current.GetService<IRuntimePlatform>()!.GetRuntimeInfo().OperatingSystem;

    public static string? GetPackageVersionStringDarwin(string path) {
        string plist_path = "";
        // macos app versions are stored in the plist file
        if (path.EndsWith(".app")) 
            plist_path = Path.Combine(path, "Contents", "Info.plist");
        

        if (path.EndsWith(".plist")) 
            plist_path = path;
        

        if (plist_path != "") {
            XmlDocument plist = new XmlDocument();
            plist.Load(plist_path);
            return plist.SelectSingleNode("/plist/dict/key[text() = 'CFBundleShortVersionString']/following-sibling::string[1]")?.InnerText;
        }
        
        return null;
    }

    public static string GetCurrentDirectory(string path) {
        return new DirectoryInfo(path).Name;
    }
    
}