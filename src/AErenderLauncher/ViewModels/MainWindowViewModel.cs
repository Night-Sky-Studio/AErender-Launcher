using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;
using Semver;

namespace AErenderLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject {
    [ObservableProperty]
    private SemVersion _version = App.Version.WithoutMetadata();
    
    public ObservableCollection<RenderTask> Tasks { get; set; } = [];

    public ObservableCollection<RenderThread> Threads { get; set; } = [];
    
    public RenderTask GetTaskById(int id) => Tasks.First(x => x.Id == id);

    public void MoveTaskUp(RenderTask task) {
        int index = Tasks.IndexOf(task);
        if (index > 0) {
            Tasks.Swap(index, index - 1);
        }
    }
    public void MoveTaskDown(RenderTask task) {
        int index = Tasks.IndexOf(task);
        if (index < Tasks.Count - 1) {
            Tasks.Swap(index, index + 1);
        }
    }

    public bool HasUpdates => Update is not null;
    
    [ObservableProperty]
    private SemVersion? _update = null;
}