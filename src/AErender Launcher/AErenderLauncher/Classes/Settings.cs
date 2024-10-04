using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.Extensions;
using Newtonsoft.Json;

namespace AErenderLauncher.Classes;

public struct AfterFx {
    public string Name { get; set; }
    public string Path { get; set; }
    public string Version { get; set; }

    public bool IsEmpty => string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(Version);
}

public class Settings {
    public static readonly string SettingsPath = Helpers.Platform == OS.macOS
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents",
            "AErender Launcher", "Settings.json")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AErender Launcher",
            "Settings.json");

    public string SettingsFolder => Helpers.GetCurrentDirectory(SettingsPath);

    public static readonly string LegacySettingsPath = Helpers.Platform == OS.macOS
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AErender",
            "AErenderConfiguration.xml")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AErender",
            "AErenderConfiguration.xml");

    public static Settings Current { get; set; } = new();

    // Language ?
    // Style  ?

    public string AfterEffectsPath { get; set; } = "";
    public string DefaultProjectsPath { get; set; } = "";
    public string DefaultOutputPath { get; set; } = "";

    public int OnRenderStart { get; set; }

    public int ThreadsLimit { get; set; }

    public List<RenderTask> RenderTasks { get; set; } = [];

    public string LastProjectPath { get; set; } = "";
    public string LastOutputPath { get; set; } = "";
    public string TempSavePath { get; set; } = "";

    public bool MissingFiles { get; set; }
    public bool Sound { get; set; }
    public bool Multithreaded { get; set; }
    public string CustomProperties { get; set; } = "";

    public float MemoryLimit { get; set; }
    public float CacheLimit { get; set; }

    public int OutputModuleIndex { get; set; }
    public OutputModule? ActiveOutputModule => OutputModules.Get(OutputModuleIndex);
    public List<OutputModule> OutputModules { get; set; } = [];
    public string RenderSettings { get; set; } = "Best Settings";

    public List<string> RecentProjects { get; set; } = [];

    public RenderingMode ThreadsRenderMode { get; set; } = RenderingMode.Tiled;

    private void InitOutputModules() {
        OutputModules.AddRange([
            new () { Module = "Lossless", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "AIFF 48kHz", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "Alpha Only", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "AVI DV NTSC 48kHz", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "AVI DV PAL 48kHz", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "Lossless with Alpha", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "Multi-Machine Sequence", Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
            new () { Module = "Photoshop", Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
            new () { Module = "Save Current Preview", Mask = "[compName].[fileExtension]", IsImported = false },
            new () { Module = "TIFF Sequence with Alph", Mask = "[compName]_[#####].[fileExtension]", IsImported = false },
        ]);
    }

    public void Init() {
        AfterEffectsPath = "C:\\Program Files\\Adobe\\Adobe After Effects 2023\\Support Files\\aerender.exe";
        DefaultProjectsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        DefaultOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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
        ThreadsRenderMode = RenderingMode.Tiled;
        InitOutputModules();
    }

    public static Settings? LoadLegacy(string xmlPath) {
        XmlDocument document = new XmlDocument();
        document.Load(xmlPath);

        XmlNode rootNode = document.DocumentElement!;

        Settings result = new Settings {
            AfterEffectsPath = rootNode["aerender"]?.InnerText ?? "",
            DefaultProjectsPath = rootNode["defprgpath"]?.InnerText ??
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            DefaultOutputPath = rootNode["defoutpath"]?.InnerText ??
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            OnRenderStart = int.Parse(rootNode["onRenderStart"]?.InnerText ?? "-1"),
            ThreadsLimit = 4,
            LastProjectPath = "",
            LastOutputPath = "",
            TempSavePath = rootNode["tempSavePath"]?.InnerText ?? "",
            MissingFiles = bool.Parse(rootNode["missingFiles"]?.InnerText ?? "false"),
            Sound = bool.Parse(rootNode["sound"]?.InnerText ?? "false"),
            Multithreaded = bool.Parse(rootNode["thread"]?.InnerText ?? "false"),
            CustomProperties = rootNode["prop"]?.InnerText ?? "",
            MemoryLimit = float.Parse(rootNode["memoryLimit"]?.InnerText ?? "100", CultureInfo.InvariantCulture),
            CacheLimit = float.Parse(rootNode["cacheLimit"]?.InnerText ?? "100", CultureInfo.InvariantCulture),
            OutputModuleIndex = int.Parse(rootNode["outputModule"]?.Attributes["selected"]?.InnerText ?? "-1"),
            RenderSettings = "Best Settings",
            ThreadsRenderMode = RenderingMode.Tiled
        };

        result.DefaultProjectsPath = result.DefaultProjectsPath == ""
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : result.DefaultProjectsPath;

        result.DefaultOutputPath = result.DefaultOutputPath == ""
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : result.DefaultOutputPath;

        List<OutputModule> parsedModules = [];

        try {
            foreach (XmlNode node in rootNode["outputModule"]!.ChildNodes) {
                parsedModules.Add(new OutputModule {
                    Module = node["moduleName"]!.InnerText,
                    Mask = node["filemask"]!.InnerText,
                    IsImported = bool.Parse(node.Attributes!["imported"]!.InnerText)
                });
            }
        } catch {
            // yeet
        }

        result.OutputModules = parsedModules;

        return result;
    }

    public static Settings? Load(string jsonPath) {
        string file = File.ReadAllText(jsonPath);

        return JsonConvert.DeserializeObject<Settings>(file);
    }

    public void Save() {
        if (!Directory.Exists(SettingsFolder)) {
            Directory.CreateDirectory(SettingsFolder);
        }

        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this));
    }

    public static bool Exists() => File.Exists(SettingsPath);

    public static bool ExistsLegacy() => File.Exists(LegacySettingsPath);

    public static List<AfterFx> DetectAfterEffects() {
        List<AfterFx> result = [];

        string adobeFolder = Helpers.Platform == OS.macOS
            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe");

        foreach (string path in Directory.GetDirectories(adobeFolder)) {
            if (path.Contains("Adobe After Effects")) {
                string afterFx;

                if (Helpers.Platform == OS.macOS) {
                    var aeName = Helpers.GetCurrentDirectoryName(path);

                    afterFx = Path.Combine(path, $"{aeName}.app", "Contents", "aerendercore.app");
                    result.Add(new() {
                        Path = Path.Combine(afterFx, "Contents", "MacOS", "aerendercore"),
                        Version = Helpers.GetPackageVersionStringDarwin(afterFx) ?? "Unknown",
                        Name = Helpers.GetCurrentDirectoryName(path)
                    });
                } else {
                    afterFx = Path.Combine(path, "Support Files", "AfterFX.com");
                    result.Add(new() {
                        Path = afterFx,
                        Version = FileVersionInfo.GetVersionInfo(afterFx).FileVersion ?? "Unknown",
                        Name = Helpers.GetCurrentDirectoryName(path)
                    });
                }
                //result.Add(FileVersionInfo.GetVersionInfo(aerender).FileVersion);
            }
        }

        return result;
    }
}