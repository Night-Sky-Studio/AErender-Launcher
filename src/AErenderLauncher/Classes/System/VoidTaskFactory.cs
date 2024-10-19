using System;
using System.Threading;
using System.Threading.Tasks;

namespace AErenderLauncher.Classes.System;

public class VoidTaskFactory {
    /// Must be the same number as <see cref="Rendering.RenderThread.Id">RenderThread.Id</see>
    public int Id { get; init; }
    private Task Task { get; }
    private readonly TaskCompletionSource<bool> _taskCompletionSource = new ();
    public Task CompletionSource => _taskCompletionSource.Task;

    public VoidTaskFactory(int id, Func<Task> func, CancellationToken cancellationToken) {
        Id = id;
        async Task Action() {
            await func();
            _taskCompletionSource.SetResult(true);
        }
        async void TaskFunc() => await Action();
        Task = new (TaskFunc, cancellationToken);
    }
    
    public bool TryStart() {
        if (Task.Status != TaskStatus.Running && Task.Status != TaskStatus.RanToCompletion) {
            Task.Start();
            return true;
        }

        return false;
    }
}