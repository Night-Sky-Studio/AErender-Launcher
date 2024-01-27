using System.Collections.Generic;
using AErenderLauncher.Classes.Rendering;
using NUnit.Framework;

namespace AErenderLauncher.Tests;

[TestFixture]
public class RenderTaskTests {
    // FrameSpan Tests
    [Test]
    public void TestFrameSpan() {
        FrameSpan span = new (0, 59);
        Assert.That(span.ToString() == "[0, 59]");
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
    }

    [Test]
    public void TestCompositionSplit() {
        Composition comp = new ("Game Icons", new FrameSpan(0, 599), 4);
        
        Assert.That(comp.SplitFrameSpans.AsString() == "[0, 149]\n[150, 298]\n[299, 447]\n[448, 599]\n");
    }
    
    // RenderTask tests
    [Test]
    public void TestRenderTask() {
        RenderTask task = new RenderTask {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
            OutputModule = "Lossless",
            RenderSettings = "Best settings",
            MissingFiles = true,
            Sound = true,
            Multiprocessing = false,
            CacheLimit = 100,
            MemoryLimit = 5,
            Compositions = [
                new Composition("Game Icons", new FrameSpan(0, 599), 1),
                new Composition("Web Icons", new FrameSpan(0, 599), 1),
                new Composition("Ecology Icons", new FrameSpan(0, 599), 1),
                new Composition("Medical Icons", new FrameSpan(0, 599), 1),
            ]
        };
        Assert.That(task.ToString() == "(C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep, C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension], Lossless, Best settings, True, True, False, , 100, 5, [Comp(Game Icons, [0, 599], 1), Comp(Web Icons, [0, 599], 1), Comp(Ecology Icons, [0, 599], 1), Comp(Medical Icons, [0, 599], 1)])"
        );
    }
}