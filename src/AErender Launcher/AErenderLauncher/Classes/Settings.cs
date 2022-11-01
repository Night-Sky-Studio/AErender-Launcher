using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using AErenderLauncher.Classes.Rendering;
using Avalonia.Platform;
using Newtonsoft.Json;

namespace AErenderLauncher.Classes;

public struct AErender {
    public string Name { get; set; } 
    public string Path { get; set; } 
    public string Version { get; set; } 
    
    public bool IsEmpty => string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(Version);
}

public class Settings {
    public static readonly string SettingsPath = Helpers.Platform == OperatingSystemType.OSX 
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AErender Launcher", "Settings.json") 
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AErender Launcher", "Settings.json");

    public string SettingsFolder => Helpers.GetCurrentDirectory(SettingsPath);
    
    public static readonly string LegacySettingsPath = Helpers.Platform == OperatingSystemType.OSX 
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AErender", "AErenderConfiguration.xml")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AErender", "AErenderConfiguration.xml");
    
    // Language ?
    // Style  ?

    public string AErenderPath { get; set; } = "C:\\Program Files\\Adobe\\Adobe After Effects 2022\\Support Files\\aerender.exe";
    public string DefaultProjectsPath { get; set; }
    public string DefaultOutputPath { get; set; }

    public int OnRenderStart { get; set; }
    
    public int ThreadsLimit { get; set; }

    public List<RenderTask> RenderTasks { get; set; } = new List<RenderTask>();
    
    public string LastProjectPath { get; set; }
    public string LastOutputPath { get; set; }
    public string TempSavePath { get; set; }

    public bool MissingFiles { get; set; }
    public bool Sound { get; set; }
    public bool Multithreaded { get; set; }
    public string CustomProperties { get; set; }
    
    public float MemoryLimit { get; set; }
    public float CacheLimit { get; set; }

    public int OutputModuleIndex { get; set; }
    public OutputModule? ActiveOutputModule => OutputModules[OutputModuleIndex];
    public List<OutputModule> OutputModules { get; set; } = new List<OutputModule>();
    public string RenderSettings { get; set; }

    public List<string> RecentProjects { get; set; } = new List<string>();

    private void InitOutputModules() {
        OutputModules.Add(new OutputModule { Module = "Lossless", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "AIFF 48kHz", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "Alpha Only", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "AVI DV NTSC 48kHz", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "AVI DV PAL 48kHz", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "Lossless with Alpha", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "Multi-Machine Sequence", Mask = "[compName]_[#####].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "Photoshop", Mask = "[compName]_[#####].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "Save Current Preview", Mask = "[compName].[fileExtension]", IsImported = false });
        OutputModules.Add(new OutputModule { Module = "TIFF Sequence with Alph", Mask = "[compName]_[#####].[fileExtension]", IsImported = false });
    }
    
    public Settings() {
        Debug.WriteLine($"Current settings path: {SettingsPath}");
        
        AErenderPath = "";
        DefaultProjectsPath = "";
        DefaultOutputPath = "";
        OnRenderStart = -1;
        ThreadsLimit = 4;
        LastProjectPath = "";
        LastOutputPath = "";
        TempSavePath = "";
        MissingFiles = false;
        Sound = false;
        Multithreaded = false;
        CustomProperties = "";
        MemoryLimit = 100f;
        CacheLimit = 100f;
        OutputModuleIndex = 0;
        RenderSettings = "Best Settings";
        InitOutputModules();
        
    }

    public static Settings? LoadLegacy(string XmlPath) {
        XmlDocument document = new XmlDocument();
        document.Load(XmlPath);
        
        XmlNode RootNode = document.DocumentElement!;

        Settings result = new Settings {
            AErenderPath = RootNode["aerender"]?.InnerText ?? "",
            DefaultProjectsPath = RootNode["defprgpath"]?.InnerText ?? "",
            DefaultOutputPath = RootNode["defoutpath"]?.InnerText ?? "",
            OnRenderStart = int.Parse(RootNode["onRenderStart"]?.InnerText ?? "-1"),
            ThreadsLimit = 4,
            LastProjectPath = "",
            LastOutputPath = "",
            TempSavePath = RootNode["tempSavePath"]?.InnerText ?? "",
            MissingFiles = bool.Parse(RootNode["missingFiles"]?.InnerText ?? "false"),
            Sound = bool.Parse(RootNode["sound"]?.InnerText ?? "false"),
            Multithreaded = bool.Parse(RootNode["thread"]?.InnerText ?? "false"),
            CustomProperties = RootNode["prop"]?.InnerText ?? "",
            MemoryLimit = float.Parse(RootNode["memoryLimit"]?.InnerText ?? "100"),
            CacheLimit = float.Parse(RootNode["cacheLimit"]?.InnerText ?? "100"),
            OutputModuleIndex = int.Parse(RootNode["outputModule"]?.Attributes["selected"]?.InnerText ?? "-1"),
            RenderSettings = "Best Settings"
        };
        
        List<OutputModule> ParsedModules = new List<OutputModule>();

        try {
            foreach (XmlNode node in RootNode["outputModule"]!.ChildNodes) {
                ParsedModules.Add(new OutputModule {
                    Module = node["moduleName"]!.InnerText,
                    Mask = node["filemask"]!.InnerText,
                    IsImported = bool.Parse(node.Attributes!["imported"]!.InnerText)
                });
            }
        } catch {
            // yeet
        }
        
        result.OutputModules = ParsedModules;

        return result;
    }

    public static Settings? Load(string JsonPath)  {
        string file = File.ReadAllText(JsonPath);

        return JsonConvert.DeserializeObject<Settings>(file);
    }

    public void Save() {
        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this));
    }

    public static bool Exists() {
        return File.Exists(SettingsPath);
    }

    public static bool ExistsLegacy() {
        return File.Exists(LegacySettingsPath);
    }
    
    public static List<AErender> DetectAerender() {
        List<AErender> result = new List<AErender>();
        
        string adobeFolder = Helpers.Platform == OperatingSystemType.OSX
            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe");
        
        foreach (string path in Directory.GetDirectories(adobeFolder)) {
            if (path.Contains("Adobe After Effects")) {
                string aerender = "";

                if (Helpers.Platform == OperatingSystemType.OSX) {
                    aerender = Path.Combine(path, "aerender");
                    result.Add(new() {
                        Path = aerender,
                        Version = Helpers.GetPackageVersionStringDarwin($"{Path.Combine(path, Helpers.GetCurrentDirectory(path))}.app") ?? "",
                        Name = Helpers.GetCurrentDirectory(path)
                    });
                } else {    
                    aerender = Path.Combine(path, "Support Files", "aerender.exe");
                    result.Add(new() {
                        Path = aerender,
                        Version = FileVersionInfo.GetVersionInfo(aerender).FileVersion ?? "",
                        Name = Helpers.GetCurrentDirectory(path)
                    });
                }
                //result.Add(FileVersionInfo.GetVersionInfo(aerender).FileVersion);
            }
        }
        return result;
    }
}