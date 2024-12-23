﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.Extensions;
using CliWrap;
using FFMpegCore;
using Newtonsoft.Json;

namespace AErenderLauncher.Classes;

public record AfterFx {
    public AfterFx() {
        Name = "";
        AfterFxPath = "";
        AerenderPath = "";
        Version = "";
    }
    public AfterFx(string path) {
        var name = Helpers.GetCurrentDirectoryName(path);

        Name = name.Delete(".app");
        AfterFxPath = path;
        
        if (Helpers.Platform == OS.macOS) {
            var app = path.Contains(".app") 
                ? path 
                : Path.Combine(AfterFxPath, $"{name}.app");
            AerenderPath = Path.Combine(app, "Contents", "aerendercore.app", 
                "Contents", "MacOS", "aerendercore");
            Version = Helpers.GetPackageVersionStringDarwin(app) ?? "Unknown";
        } else {
            var supportFiles = path.Contains("AfterFX.exe") 
                ? Path.GetDirectoryName(path)! 
                : Path.Combine(AfterFxPath, "Support Files");
            AerenderPath = Path.Combine(supportFiles, "AfterFX.com");
            Version = FileVersionInfo.GetVersionInfo(AerenderPath).FileVersion ?? "Unknown";
        }
    }
    
    public string Name { get; set; }
    public string AerenderPath { get; set; }
    public string AfterFxPath { get; set; }
    public string Version { get; set; }
}

public record FFmpeg {
    public required string Version { get; set; }
    public required string Compiler { get; set; }
    public required string[] Configuration { get; set; }
    public required string[] Libraries { get; set; }
    public string Path { get; set; } = "";
		
    public override string ToString() {
        return $"Version: {Version}\nCompiler: {Compiler}\nConfiguration: {string.Join(' ', Configuration)}\nLibraries: {string.Join("; ", Libraries)}";
    }
}

public class Settings {
    [JsonIgnore]
    public static readonly string SettingsPath = Helpers.Platform == OS.macOS
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents",
            "AErender Launcher", "Settings.json")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AErender Launcher",
            "Settings.json");
    [JsonIgnore]
    public string SettingsFolder => Helpers.GetCurrentDirectory(SettingsPath);
    [JsonIgnore]
    public static readonly string LegacySettingsPath = Helpers.Platform == OS.macOS
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AErender",
            "AErenderConfiguration.xml")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AErender",
            "AErenderConfiguration.xml");

    public static Settings Current { get; set; } = new();

    // Language ?
    // Style  ?
    public AfterFx? AfterEffects { get; set; } = null;
    private FFmpeg? _ffmpeg = null;

    public FFmpeg? FFmpeg {
        get => _ffmpeg;
        set {
            _ffmpeg = value;
            if (_ffmpeg is not null && Helpers.GetCurrentDirectory(_ffmpeg.Path) is { } path)
                GlobalFFOptions.Configure(new FFOptions {
                    BinaryFolder = path
                });
        }
    }
    
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
        OutputModules.AddRange(OutputModule.DefaultModules);
    }

    public void Init() {
        AfterEffects = null;
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
        FFmpeg = null;
        InitOutputModules();
    }

    public static Settings? LoadLegacy(string xmlPath) {
        if (!File.Exists(xmlPath)) return null;
        
        XmlDocument document = new XmlDocument();
        document.Load(xmlPath);

        XmlNode rootNode = document.DocumentElement!;

        Settings result = new Settings {
            AfterEffects = null,
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
            ThreadsRenderMode = RenderingMode.Tiled,
            FFmpeg = null
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
                parsedModules.Add(new OutputModule(
                    node["moduleName"]!.InnerText,
                    node["filemask"]!.InnerText,
                    bool.Parse(node.Attributes!["imported"]!.InnerText)
                ));
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
                result.Add(new(path));
            }
        }

        return result;
    }
	
    private static FFmpeg? ParseFFmpegOutput(string path, string output) {
        try {
            var parts = output.Split(Environment.NewLine);

            var version = parts[0].Split(' ')[2];

            var compiler = string.Join(' ', parts[1].Split(' ').Skip(2));

            var config = parts[2].Replace("configuration: ", "").Split(' ');

            var libs = parts.Skip(3).ToArray();

            return new FFmpeg {
                Path = path,
                Version = version,
                Compiler = compiler,
                Configuration = config,
                Libraries = libs
            };
        } catch (Exception) {
            return null;
        }
    }

    public static async Task<FFmpeg?> CheckFFmpegVersion(string? path) {
        if (path is null) return null;
        StringBuilder output = new();
        var cmd = await Cli.Wrap(path)
            .WithArguments("-version")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(output))
            .ExecuteAsync();
        
        return cmd.ExitCode == 0 ? ParseFFmpegOutput(path, output.ToString()) : null;
    }    
    public static async Task<FFmpeg?> DetectFFmpeg() {
        var result = await CheckFFmpegVersion("ffmpeg");
        
        var pathVar = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        result ??= await CheckFFmpegVersion(pathVar);

        return result;
    }
}