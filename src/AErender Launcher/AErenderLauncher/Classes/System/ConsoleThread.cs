using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CliWrap;
using CliWrap.Exceptions;
using Microsoft.VisualBasic;

namespace AErenderLauncher.Classes.System;

public class ConsoleThread : ReactiveObject {
    public enum ThreadState {
        Running,
        Suspended,
        Stopped,
        Finished,
        Error
    };
    private string _executable { get; }
    private string _command { get; set; }
    private Command _process { get; set; }
    private CancellationTokenSource _cts = new ();
    private CancellationToken _cancellationToken => _cts.Token;

    // private Action<string>? _outputReceived;
    // private Action<string>? _errorReceived;

    public string FullCommand => $"\"{_executable}\" {_command}";
    public ThreadState State { get; private set; } = ThreadState.Stopped;
    
    public ObservableCollection<string> Stdout { get; } = new ();
    public ObservableCollection<string> Stderr { get; } = new ();

    // private CancellationToken _cancellationToken = new CancellationToken();
    
    // public event Action<string>? OutputReceived {
    //     add => _outputReceived += value;
    //     remove => _outputReceived -= value; 
    // }
    // public event Action<string>? ErrorReceived {
    //     add => _errorReceived += value;
    //     remove => _errorReceived -= value;
    // }
    
    public ConsoleThread(string executable, string command = "") {
        _executable = executable;
        _command = command;
        _process = CreateProcess();
        Stdout.CollectionChanged += StdoutOnCollectionChanged;
    }

    protected virtual void StdoutOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        throw new NotImplementedException();
    }

    private Command CreateProcess() {
        // Process process = new Process();
        // process.EnableRaisingEvents = true;
        // process.OutputDataReceived += ProcessOnOutputDataReceived;
        // process.ErrorDataReceived += ProcessOnErrorDataReceived;
        // process.Exited += ProcessOnExited;
        //
        // process.StartInfo.FileName = _executable;
        // process.StartInfo.Arguments = _command;
        // process.StartInfo.UseShellExecute = false;
        // process.StartInfo.CreateNoWindow = true;
        // process.StartInfo.RedirectStandardOutput = true;
        // process.StartInfo.RedirectStandardError = true;

        Command process = Cli.Wrap(_executable)
            .WithArguments(_command)
            .WithStandardOutputPipe(PipeTarget.ToDelegate((data) => {
                Dispatcher.UIThread.Post(() => { Stdout.Add($"{data}"); });
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate((data) => {
                Dispatcher.UIThread.Post(() => { Stderr.Add($"{data}"); });
            }))
            .WithValidation(CommandResultValidation.ZeroExitCode);

        return process;
    }

    // private void ProcessOnExited(object? sender, EventArgs e) {
    //     Dispatcher.UIThread.Post(Dispose);
    // }

    // private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e) {
    //     Dispatcher.UIThread.Post(() => { Stdout.Add($"{e.Data}"); });
    //     // _outputReceived?.Invoke($"{e.Data}");
    // }

    // private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e) {
    //     Dispatcher.UIThread.Post(() => {
    //         Stderr.Add($"{e.Data}"); 
    //         // OnErrorReceived($"{e.Data}");
    //     });
    //     // _errorReceived?.Invoke($"{e.Data}");
    //     // Dispatcher.UIThread.Invoke(() => _errorReceived?.Invoke($"{e.Data}"));
    // }
    
    // protected virtual void OnOutputReceived(string data) { }
    // protected virtual void OnErrorReceived(string data) { }

    // public void Start() {
    //     _process.Start();
    //     _process.BeginOutputReadLine();
    //     _process.BeginErrorReadLine();
    //     State = ThreadState.Running;
    //     _process.WaitForExit();
    // }

    public async Task StartAsync() {
        // _process.Start();
        // _process.BeginOutputReadLine();
        // _process.BeginErrorReadLine();
        // State = ThreadState.Running;
        // await _process.WaitForExitAsync();
        // State = _process.ExitCode == 0 ? ThreadState.Finished : ThreadState.Error;
        try {
            await _process.ExecuteAsync(_cancellationToken);
        } catch (CommandExecutionException e) {
            State = ThreadState.Error;
        } catch (OperationCanceledException e) {
            State = ThreadState.Stopped;
        } finally {
            State = ThreadState.Finished;
        }
    }
    

    // public void Restart() {
    //     Abort();
    //     Dispose();
    //     _process = CreateProcess();
    //     Start();
    // }

    public void Abort() {
        _cts.Cancel();
        // if (State == ThreadState.Stopped) return; //throw new ThreadStateException("Can't kill a stopped thread");
        // _process.Close();
        // State = ThreadState.Stopped;
        // Dispose();
        // _process.();
    }

    // public void Dispose() {
    //     if (State != ThreadState.Stopped) Abort();
    //     _process.Dispose();
    //     Stdout.Clear();
    //     Stderr.Clear();
    // }

    // ~ConsoleThread() => Dispose();
}