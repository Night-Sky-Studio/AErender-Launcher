using AErenderLauncher.Classes.System;

namespace AErenderLauncher.Classes.Rendering; 

public class RenderQueueObject : ConsoleThread {
    public RenderQueueObject(string executable, string command) : base(executable, command) { }
}