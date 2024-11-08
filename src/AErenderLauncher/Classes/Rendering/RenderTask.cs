using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AErenderLauncher.App;
using CollectionExtensions = AErenderLauncher.Classes.Extensions.CollectionExtensions;

namespace AErenderLauncher.Classes.Rendering;

public static class RenderTaskExtensions {
    public static void AppendFromJson(this RenderTask task, string json) {
        JToken parsed = JsonConvert.DeserializeObject<JToken>(json)!;
        List<Composition> compositions = new List<Composition>();

        for (int i = 0; i < parsed.Count(); i++) {
            compositions.Add(new () {
                CompositionName = parsed[i]!["Name"]!.Value<string>()!,
                Frames = new (parsed[i]!["Frames"]![0]!.Value<uint>(), parsed[i]!["Frames"]![1]!.Value<uint>()),
                Split = 1
            });
        }


        task.Compositions = new (compositions);
    }
}

public class RenderTask : ICloneable<RenderTask> {
    // private int _id;
    private static Random _random { get; set; } = new Random();

    public enum RenderState {
        Waiting, Rendering, Finished, Stopped, Error, Suspended
    }

    public int ID { get; init; } = _random.Next(0, 999999);
    public required string Project { get; set; }
    public string ProjectName => Path.GetFileNameWithoutExtension(Project);
    public required string Output { get; set; }
    public string OutputModule { get; set; } = "Lossless";
    public string RenderSettings { get; set; } = "Best Settings";

    public bool MissingFiles { get; set; } = false;
    public bool Sound { get; set; } = true;
    public bool Multiprocessing { get; set; } = false;
    public string CustomProperties { get; set; } = "";
    public double CacheLimit { get; set; } = 100.0;
    public double MemoryLimit { get; set; } = 100.0;

    public ObservableCollection<Composition> Compositions { get; set; } = new();
    public RenderState State { get; set; } = RenderState.Waiting;

    /// <summary>
    /// Checks if there is "[projectName]" in <see cref="Output"/>,
    /// creates a folder with <see cref="Project"/>'s name and changes
    /// <see cref="Output"/> to match that newly created folder
    /// </summary>
    private string ProcessFolder() {
        if (Output.Contains("[projectName]" + Path.DirectorySeparatorChar)) {
            string result = Output.Replace("[projectName]", Path.GetFileNameWithoutExtension(Project));

            if (!Directory.Exists(Path.GetDirectoryName(result)))
                Directory.CreateDirectory(Path.GetDirectoryName(result)!);

            return result;
        }

        return Output;
    }

    private static string ProcessSplit(string path, int index) {
        string GetFilePathWithoutExtension(string _path) =>
            _path.Replace(Path.GetExtension(_path), "");


        // Strip full path of the file extension
        string ProcessedPath = GetFilePathWithoutExtension(path);
        // Append an index to the file name
        ProcessedPath += $"_{index}";
        // Finally, append extension back to path
        ProcessedPath += Path.GetExtension(path);

        return ProcessedPath;
    }

    public List<RenderThread> Enqueue() {
        if (Settings.Current.AfterEffects is null)
            throw new MissingAeException("After Effects is not installed on this system or it's path wasn't specified.");
        
        // string exec = "";
        List<string> exec = [];
        List<RenderThread> result = new List<RenderThread>();

        foreach (Composition comp in Compositions) {
            for (int i = 0; i < comp.Split; i++) {
                // Create directory for project if there isn't one
                string adjustedOutput = ProcessFolder();

                if (comp.Split > 1)
                    adjustedOutput = ProcessSplit(adjustedOutput, i);

                // exec = $"-project \"{Project}\" " +
                //        $"-output \"{AdjustedOutput}\" " +
                //        (Sound ? "-sound ON " : "") +
                //        (Multiprocessing ? "-mp " : "") +
                //        (MissingFiles ? "-continueOnMissingFootage " : "") +
                //        $"-comp \"{comp.CompositionName}\" " +
                //        $"-OMtemplate \"{OutputModule}\" " +
                //        $"-RStemplate \"{RenderSettings}\" " +
                //        $"-mem_usage \"{Math.Truncate(MemoryLimit)}\" \"{Math.Truncate(CacheLimit)}\" " +
                //        (CustomProperties != "" ? CustomProperties.Trim() + " " : "") +
                //        $"-s {comp.SplitFrameSpans[i].StartFrame} -e {comp.SplitFrameSpans[i].EndFrame}";
                
                exec.AddMany(
                    "-project", Project.Replace("\\", "\\\\"),
                    "-output", adjustedOutput.Replace("\\", "\\\\"),
                    Sound ? "-sound" : null, Sound ? "ON" : null,
                    Multiprocessing ? "-mp" : null,
                    MissingFiles ? "-continueOnMissingFootage" : null,
                    "-comp", comp.CompositionName,
                    "-OMtemplate", OutputModule,
                    "-RStemplate", RenderSettings,
                    "-mem_usage", $"{Math.Truncate(MemoryLimit)}", $"{Math.Truncate(CacheLimit)}",
                    CustomProperties != "" ? CustomProperties.Trim() + " " : null,
                    "-s", $"{comp.SplitFrameSpans[i].StartFrame}",
                    "-e", $"{comp.SplitFrameSpans[i].EndFrame}"
                );
                
                if (comp.SplitFrameSpans[i].StartFrame == 0 && comp.SplitFrameSpans[i].EndFrame == 0)
                    exec.RemoveRange(exec.Count - 2, 2);
                
                result.Add(new (Settings.Current.AfterEffects.AerenderPath, exec) {
                    Id = _random.Next(0, 999999),
                    Name = comp.CompositionName
                });
            }
        }

        return result;
    }

    public override string ToString() {
        return $"({Project}, {Output}, {OutputModule}, {RenderSettings}, {MissingFiles}, {Sound}, {Multiprocessing}, {CustomProperties}, {CacheLimit}, {MemoryLimit}, {CollectionExtensions.ToString(Compositions)})";
    }
    public RenderTask Clone() {
        return new RenderTask {
            Project = Project,
            Output = Output,
            OutputModule = OutputModule,
            RenderSettings = RenderSettings,
            MissingFiles = MissingFiles,
            Sound = Sound,
            Multiprocessing = Multiprocessing,
            CustomProperties = CustomProperties,
            CacheLimit = CacheLimit,
            MemoryLimit = MemoryLimit,
            Compositions = new(Compositions.Select(x => x.Clone())),
            State = State
        };
    }

    public static RenderTask Empty() {
        return new () {
            Project = "",
            Output = ""
        };
    }
}