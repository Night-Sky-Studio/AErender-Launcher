using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AErenderLauncher.App;

namespace AErenderLauncher.Classes.Rendering;

public static class RenderTaskExtensions {
    public static void AppendFromJson(this RenderTask task, string json) {
        JToken parsed = JsonConvert.DeserializeObject<JToken>(json)!;
        List<Composition> compositions = new List<Composition>();

        for (int i = 0; i < parsed.Count(); i++) {
            compositions.Add(new Composition {
                CompositionName = parsed[i]!["Name"]!.Value<string>()!,
                Frames = new FrameSpan(parsed[i]!["Frames"]![0]!.Value<int>()!, parsed[i]!["Frames"]![1]!.Value<int>()!),
                Split = 1
            });
        }


        task.Compositions = compositions;
    }
}

public class RenderTask {
    private Random _random { get; set; } = new Random();
    
    public enum RenderState {
        Waiting, Rendering, Finished, Stopped, Error, Suspended
    }
    
    public string Project { get; set; }
    public string Output { get; set; }
    public string OutputModule { get; set; }
    public string RenderSettings { get; set; }

    public bool MissingFiles { get; set; } = false;
    public bool Sound { get; set; } = true;
    public bool Multiprocessing { get; set; } = false;
    public string CustomProperties { get; set; } = "";
    public double CacheLimit { get; set; }
    public double MemoryLimit { get; set; }
    
    public List<Composition> Compositions { get; set; }
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
        string GetFilePathWithoutExtension(string path) {
            return path.Replace(Path.GetExtension(path), "");
        }
        
        /// Strip full path of the file extension
        string ProcessedPath = GetFilePathWithoutExtension(path);
        /// Append an index to the file name
        ProcessedPath += $"_{index}";
        /// Finally, append extension back to path
        ProcessedPath += Path.GetExtension(path);

        return ProcessedPath;
    }
    
    public List<RenderQueueObject> Enqueue() {
        string exec = "";
        List<RenderQueueObject> result = new List<RenderQueueObject>();

        foreach (Composition comp in Compositions) {
            for (int i = 0; i < comp.Split; i++) {
                /// Create directory for project if there isn't one
                string AdjustedOutput = ProcessFolder();

                if (comp.Split > 1) 
                    AdjustedOutput = ProcessSplit(Output, i);

                exec += $"-project \"{Project}\" " +
                        $"-output \"{AdjustedOutput}\" " +
                        (Sound ? "-sound ON " : "") +
                        (Multiprocessing ? "-mp " : "") +
                        (MissingFiles ? "-continueOnMissingFootage " : "") +
                        $"-comp \"{comp.CompositionName}\" " +
                        $"-OMtemplate \"{OutputModule}\" " +
                        $"-RStemplate \"{RenderSettings}\" " +
                        $"-mem_usage \"{Math.Truncate(MemoryLimit)}\" \"{Math.Truncate(CacheLimit)}\" " +
                        (CustomProperties != "" ? CustomProperties.Trim() + " " : "") +
                        $"-s {comp.SplitFrameSpans[i].StartFrame} -e {comp.SplitFrameSpans[i].EndFrame}";
                if (comp.SplitFrameSpans[i].StartFrame == 0 && comp.SplitFrameSpans[i].EndFrame == 0)
                    exec = exec.Replace("-s 0 -e 0", "");
                
                result.Add(new RenderQueueObject(ApplicationSettings.AErenderPath, exec) {
                    ID = _random.Next(0, 999999),
                    CompositionName = comp.CompositionName
                });
            }
        }

        return result;
    }
}