using System.Collections.Generic;
using AErenderLauncher.Classes.Rendering;
using NUnit.Framework;

namespace AErenderLauncher.Test;

[TestFixture]
public class RenderTaskTests {
    // FrameSpan Tests
    [Test]
    public void TestFrameSpan() {
        FrameSpan span = new (0, 59);
        Assert.That(span.ToString() == "[0, 59]");
        
        span = new ("0", "59");
        Assert.That(span.ToString("({0}..{1})") == "(0..59)");
    }

    [Test]
    public void TestFrameSpanSplit() {
        FrameSpan[] spans = new FrameSpan(0, 599).Split(4);

        Assert.That(spans.AsString() == "[0, 149]\n[150, 298]\n[299, 447]\n[448, 599]\n");
    }
    
    // Composition tests
    [Test]
    public void TestComposition() {
        Composition comp = new ("Game Icons", new FrameSpan(0, 599), 1);
        Assert.That(comp.ToString() == "Comp(Game Icons, [0, 599], 1)");

        Composition newComp = comp.Clone();
        Assert.That(newComp.ToString() == "Comp(Game Icons, [0, 599], 1)");

        comp = new();
        Assert.That(comp.ToString() == "Comp(, [0, 0], 1)");
    }

    [Test]
    public void TestCompositionSplit() {
        Composition comp = new ("Game Icons", new FrameSpan(0, 599), 4);
        
        Assert.That(comp.SplitFrameSpans.AsString() == "[0, 149]\n[150, 298]\n[299, 447]\n[448, 599]\n");
        
        comp.Split = 2;
        
        Assert.That(comp.SplitFrameSpans.AsString() == "[0, 299]\n[300, 599]\n");
    }
    
    // RenderTask tests
    [Test]
    public void TestRenderTask() {
        string taskStr = "(C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep, C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension], Lossless, Best settings, True, True, False, -reuse, 100, 5, [Comp(Game Icons, [0, 599], 1), Comp(Web Icons, [0, 599], 1), Comp(Ecology Icons, [0, 599], 1), Comp(Medical Icons, [0, 599], 1)])";
        RenderTask task = new () {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
            OutputModule = "Lossless",
            RenderSettings = "Best settings",
            MissingFiles = true,
            Sound = true,
            Multiprocessing = false,
            CustomProperties = "-reuse",
            CacheLimit = 100,
            MemoryLimit = 5,
            Compositions = [
                new Composition("Game Icons", new FrameSpan(0, 599), 1),
                new Composition("Web Icons", new FrameSpan(0, 599), 1),
                new Composition("Ecology Icons", new FrameSpan(0, 599), 1),
                new Composition("Medical Icons", new FrameSpan(0, 599), 1),
            ]
        };
        Assert.That(task.ToString() == taskStr);
        Assert.That(task.ID != 0);
        Assert.That(task.ProjectName != "");
        Assert.That(task.State == RenderTask.RenderState.Waiting);

        var queue = task.Enqueue();
        Assert.That(queue.Count == 4);
        Assert.That(queue[0].Name == "Game Icons");
        
        var newTask = task.Clone();
        Assert.That(newTask.ToString() == taskStr);
        
        newTask.Output = "C:\\Users\\lunam\\Desktop\\Mograph Icons\\[compName].[fileExtension]";
        queue = newTask.Enqueue();
        Assert.That(queue[0].FullCommand.Contains(newTask.Output));

        task = new() {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[projectName]\\[compName].[fileExtension]",
            Compositions = [
                new Composition("Game Icons", new FrameSpan(0, 0), 1),
                new Composition("Web Icons", new FrameSpan(0, 599), 2)
            ]
        };
        queue = task.Enqueue();
        Assert.That(queue.Count == 3);
        Assert.That(queue[0].FullCommand.Contains("-s 0 -e 0") == false);
        Assert.That(queue[1].FullCommand.Contains("-s 0 -e 299") && queue[1].FullCommand.Contains("[compName]_0"));
        Assert.That(Directory.Exists("C:\\Users\\lunam\\Desktop\\Mograph Icons\\Mograph Icons"));
    }
}