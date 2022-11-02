using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using AErenderLauncher.Classes.System;
using Avalonia.Platform;
using Newtonsoft.Json;
using TaskExtensions = AErenderLauncher.Classes.System.TaskExtensions;

namespace AErenderLauncher.Classes.Project; 

public class AeProjectParser {
    public static readonly string ParserPath = Helpers.Platform == OperatingSystemType.OSX 
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AErender Launcher", "aeparser_mac") 
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AErender Launcher", "aeparser_win.exe");

    private ConsoleThread ParserThread { get; set; }
    
    public AeProjectParser() {
        if (File.Exists(ParserPath)) {
            ParserThread = new ConsoleThread(ParserPath);
            //ParserThread.Output.CollectionChanged += OutputOnCollectionChanged;
        } else throw new FileNotFoundException("aeparser is not found in the application configuration directory");
    }

    public async Task<ProjectItem[]?> ParseProject(string ProjectPath) {
        // It does require double quotes...
        ParserThread.Command = $"\"{ProjectPath}\"";
        ParserThread.Start();
        
        // yeet
        await TaskExtensions.WaitUntil(() => ParserThread.Output.Count == 3);

        // double yeet
        //
        // will probably break sometime in the future, lol
        // unless I find a way to grab CDTA from AEP myself
        return JsonConvert.DeserializeObject<ProjectItem[]>(ParserThread.Output[1]) ?? null;
    }
    
}