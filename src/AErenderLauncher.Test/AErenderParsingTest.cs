using System;
using AErenderLauncher.Classes.Rendering;
using NUnit.Framework;

namespace AErenderLauncher.Test;

[TestFixture]
public class AErenderParsingTest {
    const string DurationData = "PROGRESS:  Duration: 0:00:10:00";
    const string FrameRateData = "PROGRESS:  Frame Rate: 23.976 (comp)";
    const string FrameData = "PROGRESS:  0:00:00:00 (1): 0 Seconds";
    
    [Test]
    public void TestParseDuration() {
        Timecode correct = new(0, 0, 10, 0);
        Timecode? duration = AerenderParser.ParseDuration(DurationData);
        Assert.That(duration != null);
        Assert.That(duration.ToString() == correct.ToString());
    }
    
    [Test]
    public void TestParseFrameRate() {
        double correct = 23.976;
        double? framerate = AerenderParser.ParseFramerate(FrameRateData);
        Assert.That(framerate != null);
        Assert.That(Math.Abs((double)framerate! - correct) < 0.00000001);
    }
    
    [Test]
    public void TestParseFrameData() {
        AerenderFrameData correct = new() {
            Timecode = new(0, 0, 0, 0),
            Frame = 1,
            ElapsedTime = 0
        };
        AerenderFrameData? frameData = AerenderParser.ParseFrameData(FrameData);
        Assert.That(frameData != null);
        Assert.That(frameData.ToString() == correct.ToString());
    }
}