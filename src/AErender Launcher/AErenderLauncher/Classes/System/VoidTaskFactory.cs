using System;
using System.Threading.Tasks;

namespace AErenderLauncher.Classes.System;

public class VoidTaskFactory {
    public Task Task { get; set; }
    private readonly TaskCompletionSource<bool> _taskCompletionSource = new ();
    public Task CompletionSource => _taskCompletionSource.Task;

    public VoidTaskFactory(Func<Task> func) {
        async Task Action() {
            await func();
            _taskCompletionSource.SetResult(true);
        }
        async void TaskFunc() => await Action();
        Task = new Task(TaskFunc);
    }
    
    public bool TryStart() {
        if (Task.Status != TaskStatus.Running && Task.Status != TaskStatus.RanToCompletion) {
            Task.Start();
            return true;
        }

        return false;
    }
}