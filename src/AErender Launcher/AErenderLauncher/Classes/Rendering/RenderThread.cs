using System;
using AErenderLauncher.Classes.System;

namespace AErenderLauncher.Classes.Rendering; 

public class RenderThread : ConsoleThread {
    public int ID { get; set; }
    public int CurrentFrame { get; set; } = 0;
    public int EndFrame { get; set; } = 1;
    public string CompositionName { get; set; } = "";
    public RenderThread(string executable, string command) : base(executable, command) { }
    
}