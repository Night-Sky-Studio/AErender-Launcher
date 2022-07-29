using System;
using System.Collections.Generic;
using System.IO;
using AErenderLauncher.Classes.Rendering;

namespace TestConsoleApp;

public static class Program {
    private const string ProjectPath = @"C:\Users\lunam\Desktop\Project.aep";
    private const string OutputPath  = @"C:\Users\lunam\Desktop\[projectName]\[compName].[fileExtension]";

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

    
    
    public static void Main() {
        RenderTask task = new RenderTask {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
            OutputModule = "Lossless",
            RenderSettings = "Best Settings",
            MissingFiles = true,
            Sound = true,
            Multiprocessing = false,
            CacheLimit = 100,
            MemoryLimit = 5,
            Compositions = new List<Composition>() {
                new Composition("Game Icons", new FrameSpan(0, 599), 1),
                new Composition("Web Icons", new FrameSpan(0, 599), 1),
                new Composition("Ecology Icons", new FrameSpan(0, 599), 1),
                new Composition("Medical Icons", new FrameSpan(0, 599), 1),
            }
        };
        
        Console.WriteLine(task.Enqueue()[0].FullCommand);
        
        //Console.WriteLine(Path.GetDirectoryName("C:\\Users\\lunam\\Desktop\\Mograph Icons\\[compName].[fileExtension]"));
    }
}
