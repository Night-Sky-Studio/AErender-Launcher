using System.Collections.Generic;
using AErenderLauncher.Classes.System;

namespace AErenderLauncher.Classes.Rendering; 

public struct FrameSpan {
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }

    public FrameSpan(int StartFrame, int EndFrame) {
        this.StartFrame = StartFrame;
        this.EndFrame = EndFrame;
    }

    public FrameSpan(string StartFrame, string EndFrame) {
        this.StartFrame = int.Parse(StartFrame);
        this.EndFrame = int.Parse(EndFrame);
    }

    public FrameSpan[] Split(int Count) {
        FrameSpan[] result = new FrameSpan[Count];
        result[0].StartFrame = StartFrame;
        int j = EndFrame - StartFrame;
        int k = j / Count;
        result[0].EndFrame = StartFrame + k;
        for (int i = 1; i < Count; i++) {
            result[i].StartFrame = StartFrame + k * i + 1;
            result[i].EndFrame = StartFrame + k * (i + 1);
        }
        result[Count - 1].EndFrame = EndFrame;
        return result;
    }
}