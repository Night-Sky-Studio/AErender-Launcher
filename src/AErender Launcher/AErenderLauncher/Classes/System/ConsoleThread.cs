using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace AErenderLauncher.Classes.System; 

public class ConsoleThread {
    public enum ThreadState {
        Running,
        Suspended,
        Stopped
    };
    private string _executable { get; }
    private string _command { get; set; } = "";
    private Process _process { get; set; }

    // public string Command {
    //     get => _command;
    //     set {
    //         if (State == ThreadState.Running || State == ThreadState.Suspended) {
    //             throw new ThreadStateException("Cannot change command while thread is running");
    //         }
    //
    //         Dispose();
    //         _command = value;
    //         _process = CreateProcess();
    //     }
    // }

    public string FullCommand => $"\"{_executable}\" {_command}";
    public ThreadState State { get; private set; } = ThreadState.Stopped;
    public ObservableCollection<string> Output { get; } = new ObservableCollection<string>();

    public ConsoleThread(string executable, string command = "") {
        _executable = executable;
        _command = command;
        _process = CreateProcess();
    }

    private Process CreateProcess() {
        Process process = new Process();
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += ProcessOnOutputDataReceived;
        process.ErrorDataReceived += ProcessOnErrorDataReceived;
        process.Exited += ProcessOnExited;
        
        process.StartInfo.FileName = _executable;
        process.StartInfo.Arguments = _command;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        return process;
    }

    private void ProcessOnExited(object? sender, EventArgs e) {
        Dispatcher.UIThread.Post(() => {
            //Output.Add($"process exited with code {_process.ExitCode}\n");
            Dispose();
        });
    }

    protected void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e) {
        Dispatcher.UIThread.Post(() => { Output.Add(e.Data + '\n'); });
    }

    protected void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e) {
        Dispatcher.UIThread.Post(() => { Output.Add(e.Data + '\n'); });
    }

    public void Start() {
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        State = ThreadState.Running;
        _process.WaitForExit();
    }

    public void Restart() {
        Abort();
        Dispose();
        _process = CreateProcess();
        Start();
    }

    public void Abort() {
        if (State == ThreadState.Stopped) throw new ThreadStateException("Can't kill a stopped thread");
        _process.Close();
        State = ThreadState.Stopped;
    }

    public void Dispose() {
        if (State != ThreadState.Stopped) Abort();
        _process.Dispose();
        Output.Clear();
    }

    ~ConsoleThread() => Dispose();
}