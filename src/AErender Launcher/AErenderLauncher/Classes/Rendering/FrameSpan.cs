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
    
    public override string ToString() {
        return $"[{StartFrame}, {EndFrame}]";
    }
    public string ToString(string Format) {
        Format = Format != "" ? Format : "[{0}, {1}]";
        return string.Format(Format, StartFrame, EndFrame);
    }
}