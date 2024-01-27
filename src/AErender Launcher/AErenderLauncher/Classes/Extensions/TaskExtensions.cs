using System;
using System.Threading.Tasks;

namespace AErenderLauncher.Classes.Extensions;

public class TaskExtensions {
    public static async Task WaitUntil(Func<bool> Condition, int Frequency = 25, int Timeout = -1) {
        Task WaitTask = Task.Run(async () => {
            while (!Condition()) {
                await Task.Delay(Frequency);
            }
        });
        
        if (WaitTask != await Task.WhenAny(WaitTask, Task.Delay(Timeout))) 
            throw new TimeoutException();
    }
}