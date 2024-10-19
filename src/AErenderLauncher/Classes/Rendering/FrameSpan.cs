namespace AErenderLauncher.Classes.Rendering;

public static class FrameSpanExtensions {
    public static string AsString(this FrameSpan[] spans) {
        string result = "";
        foreach (FrameSpan span in spans) {
            result += span + "\n";
        }

        return result;
    }
}

public struct FrameSpan {
    public uint StartFrame { get; set; }
    public uint EndFrame { get; set; }

    public FrameSpan(uint startFrame, uint endFrame) {
        StartFrame = startFrame;
        EndFrame = endFrame;
    }

    public FrameSpan(string startFrame, string endFrame) {
        StartFrame = uint.Parse(startFrame);
        EndFrame = uint.Parse(endFrame);
    }

    public FrameSpan[] Split(uint count) {
        FrameSpan[] result = new FrameSpan[count];
        result[0].StartFrame = StartFrame;
        uint j = EndFrame - StartFrame;
        uint k = j / count;
        result[0].EndFrame = StartFrame + k;
        for (uint i = 1; i < count; i++) {
            result[i].StartFrame = StartFrame + k * i + 1;
            result[i].EndFrame = StartFrame + k * (i + 1);
        }
        result[count - 1].EndFrame = EndFrame;
        return result;
    }
    
    public override string ToString() {
        return $"[{StartFrame}, {EndFrame}]";
    }
    public string ToString(string format) {
        format = format != "" ? format : "[{0}, {1}]";
        return string.Format(format, StartFrame, EndFrame);
    }
}