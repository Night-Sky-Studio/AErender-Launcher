using System;
using AErenderLauncher.Classes.System;

namespace AErenderLauncher.Classes.Rendering; 

public class RenderQueueObject : ConsoleThread {
    public int ID { get; set; }
    public int CurrentFrame { get; set; } = 0;
    public int EndFrame { get; set; } = 1;
    public string CompositionName { get; set; } = "";
    public RenderQueueObject(string executable, string command) : base(executable, command) { }
}