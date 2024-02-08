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
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }

    public FrameSpan(int startFrame, int endFrame) {
        StartFrame = startFrame;
        EndFrame = endFrame;
    }

    public FrameSpan(string startFrame, string endFrame) {
        StartFrame = int.Parse(startFrame);
        EndFrame = int.Parse(endFrame);
    }

    public FrameSpan[] Split(int count) {
        FrameSpan[] result = new FrameSpan[count];
        result[0].StartFrame = StartFrame;
        int j = EndFrame - StartFrame;
        int k = j / count;
        result[0].EndFrame = StartFrame + k;
        for (int i = 1; i < count; i++) {
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