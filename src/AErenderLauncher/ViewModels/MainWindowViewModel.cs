using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using Semver;

namespace AErenderLauncher.ViewModels;

public class MainWindowViewModel : ReactiveObject {
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

    public bool HasUpdates => _update is not null;
    private SemVersion? _update = null;
    public SemVersion? Update {
        get => _update;
        set {
            RaiseAndSetIfChanged(ref _update, value);
            RaisePropertyChanged(new (nameof(HasUpdates)));
        }
    }
}