using Avalonia;
using Avalonia.Platform;

namespace AErenderLauncher.Classes; 

public static class Helpers {
    public static OperatingSystemType Platform = AvaloniaLocator.Current.GetService<IRuntimePlatform>()!.GetRuntimeInfo().OperatingSystem;
}