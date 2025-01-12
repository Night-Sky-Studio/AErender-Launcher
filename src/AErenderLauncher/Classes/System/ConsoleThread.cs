using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;
using CommunityToolkit.Mvvm.ComponentModel;
using ThreadState = AErenderLauncher.Enums.ThreadState;

namespace AErenderLauncher.Classes.System;

[Obsolete("Use NetworkThread instead")]
public class ConsoleThread : ObservableObject {
    private string Executable { get; }
    private string Command { get; set; }
    private Command Process { get; set; }
    private readonly CancellationTokenSource _cts = new ();
    private CancellationToken CancellationToken => _cts.Token;

    // private Action<string>? _outputReceived;
    // private Action<string>? _errorReceived;

    private event Action<ConsoleThread?, ThreadState>? StateChanged;

    public string FullCommand => $"\"{Executable}\" {Command}";

    private ThreadState _state = ThreadState.Stopped;
    public ThreadState State {
        get => _state;
        private set {
            _state = value;
            StateChanged?.Invoke(this, value);
        }
    } 

    // public ObservableCollection<string> Stdout { get; } = new ();
    // public ObservableCollection<string> Stderr { get; } = new ();

    // private CancellationToken _cancellationToken = new CancellationToken();

    public event Action<ConsoleThread?, string>? OutputReceived;
    public event Action<ConsoleThread?, string>? ErrorReceived;
    
    public ConsoleThread(string executable, string command = "") {
        Executable = executable;
        Command = command;
        Process = CreateProcess();
        OutputReceived += OnOutputReceived;
        ErrorReceived += OnErrorReceived;
        StateChanged += OnStateChanged;
    }

    protected virtual void OnOutputReceived(ConsoleThread? sender, string obj) {
        throw new NotImplementedException();
    }
    
    protected virtual void OnErrorReceived(ConsoleThread? sender, string obj) {
        throw new NotImplementedException();
    }

    protected virtual void OnStateChanged(ConsoleThread? sender, ThreadState state) {
        throw new NotImplementedException();
    }
    
    private Command CreateProcess() {
        Command process = Cli.Wrap(Executable)
            .WithArguments(Command)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(data => {
                // Dispatcher.UIThread.Post(() => {
                    OutputReceived?.Invoke(this, $"{data}");
                // }, DispatcherPriority.Render);
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(data => {
                // Dispatcher.UIThread.Post(() => {
                    ErrorReceived?.Invoke(this, $"{data}");
                // }, DispatcherPriority.Render);
            }))
            .WithValidation(CommandResultValidation.ZeroExitCode);

        return process;
    }

    public async Task StartAsync() {
        try {
            State = ThreadState.Running;
            await Process.ExecuteAsync(CancellationToken);
        } catch (CommandExecutionException) {
            State = ThreadState.Error;
        } catch (OperationCanceledException) {
            State = ThreadState.Stopped;
        } finally {
            State = ThreadState.Finished;
        }
    }

    public void Abort() {
        _cts.Cancel();
    }
}