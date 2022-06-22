using System;
using System.Collections.Generic;

namespace AErenderLauncher.Classes.Rendering; 

public class RenderTask {
    public enum RenderState {
        Waiting, Rendering, Finished, Stopped, Error, Suspended
    }
    
    
    public string Project { get; set; }
    public string Output { get; set; }
    public string OutputModule { get; set; }
    public string RenderSettings { get; set; }

    public bool MissingFiles { get; set; } = false;
    public bool Sound { get; set; } = true;
    public bool Multiprocessing { get; set; } = false;
    public string CustomProperties { get; set; }
    public double CacheLimit { get; set; }
    public double MemoryLimit { get; set; }
    
    public List<Composition> Compositions { get; set; }
    public RenderState State { get; set; } = RenderState.Waiting;

    RenderTask(string JSON) {

    }

}