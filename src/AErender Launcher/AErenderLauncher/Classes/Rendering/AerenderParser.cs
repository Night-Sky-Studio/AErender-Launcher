using System;
using AErenderLauncher.Classes.Extensions;

namespace AErenderLauncher.Classes.Rendering;

public enum AerenderDataType {
    Information, Progress, Error
}

public struct Timecode(uint Hours, uint Minutes, uint Seconds, uint Frames) {
    public uint H = Hours, MM = Minutes, SS = Seconds, FR = Frames;
    
    public override string ToString() {
        return $"{H}:{MM}:{SS}:{FR}";
    }

    public uint ToSeconds() {
        return H * 60 * 60 + MM * 60 + SS;
    }
    
    public static Timecode FromString(string timecode) {
        string[] parts = timecode.Split(":");
        return new Timecode {
            H = uint.Parse(parts[0]),
            MM = uint.Parse(parts[1]),
            SS = uint.Parse(parts[2]),
            FR = uint.Parse(parts[3])
        };
    }

    public uint ToFrames(double framerate) {
        return Convert.ToUInt32(Math.Truncate(ToSeconds() * framerate + FR));
    }
    
    // public static bool operator==(Timecode? a, Timecode? b) => a.ToString() == b.ToString();
    // public static bool operator!=(Timecode? a, Timecode? b) => a.ToString() != b.ToString();
}

public struct AerenderFrameData {
    public Timecode Timecode { get; set; }
    public uint Frame { get; set; }
    public uint ElapsedTime { get; set; }
    
    public override string ToString() => $"Timecode: {Timecode}, Frame: {Frame}, ElapsedTime: {ElapsedTime}";
    
    // public static bool operator==(AerenderFrameData a, AerenderFrameData b) => a.ToString() == b.ToString();
    // public static bool operator!=(AerenderFrameData a, AerenderFrameData b) => a.ToString() != b.ToString();
}

public static class AerenderParser {
    
    // PROGRESS:  0:00:00:00 (1): 0 Seconds
    public static AerenderFrameData? ParseFrameData(string data) {
        if (!data.StartsWith("PROGRESS:  ")) return null;
        string[] parts = data.Delete("PROGRESS:  ").Split(" ");
        try {
            return new AerenderFrameData {
                Timecode = Timecode.FromString(parts[0]),
                Frame = uint.Parse(parts[1].Delete(["(", "):"])),
                ElapsedTime = uint.Parse(parts[2])
            };
        } catch (Exception) {
            return null;
        }
    }
    
    // PROGRESS:  Duration: 0:00:10:00
    public static Timecode? ParseDuration(string data) {
        if (!data.StartsWith("PROGRESS:  Duration: ")) return null;
        string[] parts = data.Delete("PROGRESS:  ").Split(" ");
        return Timecode.FromString(parts[1]);
    }
    
    // PROGRESS:  Frame Rate: 60.00 (comp)
    public static double? ParseFramerate(string data) {
        if (!data.StartsWith("PROGRESS:  Frame Rate: ")) return null;
        string[] parts = data.Delete("PROGRESS:  ").Split(" ");
        return double.Parse(parts[2]);
    }
}