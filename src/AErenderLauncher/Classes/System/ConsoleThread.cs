using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;
using ThreadState = AErenderLauncher.Enums.ThreadState;

namespace AErenderLauncher.Classes.System;

[Obsolete("Use NetworkThread instead")]
public class ConsoleThread : ReactiveObject {
    private string _executable { get; }
    private string _command { get; set; }
    private Command _process { get; set; }
    private CancellationTokenSource _cts = new ();
    private CancellationToken _cancellationToken => _cts.Token;

    // private Action<string>? _outputReceived;
    // private Action<string>? _errorReceived;

    private event Action<ConsoleThread?, ThreadState>? StateChanged;

    public string FullCommand => $"\"{_executable}\" {_command}";

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
        _executable = executable;
        _command = command;
        _process = CreateProcess();
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
        Command process = Cli.Wrap(_executable)
            .WithArguments(_command)
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
            await _process.ExecuteAsync(_cancellationToken);
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