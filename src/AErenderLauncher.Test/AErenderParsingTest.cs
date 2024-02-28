using System;
using AErenderLauncher.Classes.Rendering;
using NUnit.Framework;

namespace AErenderLauncher.Test;

[TestFixture]
public class AErenderParsingTest {
    [Test]
    public void TestParseDuration() {
        Timecode correct = new(0, 0, 10, 0);
        Timecode? duration = AerenderParser.ParseDuration("PROGRESS:  Duration: 0:00:10:00");
        Assert.That(duration != null);
        Assert.That(duration.ToString() == correct.ToString());
        Assert.That(duration?.ToFrames(60) == 600);
    }

    [Test]
    public void TestParseDurationError() {
        Timecode? duration = AerenderParser.ParseDuration("PROGRESS: Duration: a:10:00");
        Assert.That(duration == null);
    }
    
    [Test]
    public void TestParseFrameRate() {
        double correct = 23.976;
        double? framerate = AerenderParser.ParseFramerate("PROGRESS:  Frame Rate: 23.976 (comp)");
        Assert.That(framerate != null);
        Assert.That(Math.Abs((double)framerate! - correct) < 0.00000001);
    }
    
    [Test]
    public void TestParseFrameRateError() {
        double? framerate = AerenderParser.ParseFramerate("PROGRESS: Frame Rate: 23.");
        Assert.That(framerate == null);
    }
    
    [Test]
    public void TestParseFrameData() {
        AerenderFrameData correct = new() {
            Timecode = new(0, 0, 0, 0),
            Frame = 1,
            ElapsedTime = 0
        };
        AerenderFrameData? frameData = AerenderParser.ParseFrameData("PROGRESS:  0:00:00:00 (1): 0 Seconds");
        Assert.That(frameData != null);
        Assert.That(frameData.ToString() == correct.ToString());
    }
    
    [Test]
    public void TestParseFrameDataError() {
        AerenderFrameData? frameData = AerenderParser.ParseFrameData("PROGRESS:  0:00 (22.3): 5 Seconds");
        Assert.That(frameData == null);
        
        frameData = AerenderParser.ParseFrameData(" 0:00 (22.3): 5 Seconds");
        Assert.That(frameData == null);
    }
}