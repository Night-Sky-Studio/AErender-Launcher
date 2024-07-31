using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;
using ThreadState = AErenderLauncher.Enums.ThreadState;

namespace AErenderLauncher.Classes.System;

public class ConsoleThread : ReactiveObject {
    private string _executable { get; }
    private List<string> _args { get; set; }
    private Command _process { get; set; }
    private CancellationTokenSource _cts = new ();
    private CancellationToken _cancellationToken => _cts.Token;

    // private Action<string>? _outputReceived;
    // private Action<string>? _errorReceived;

    private event Action<ConsoleThread?, ThreadState>? StateChanged;

    public string FullCommand => $"\"{_executable}\" {string.Join(" ", _args)}";

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

    private TcpListener _listener = null!;
    private TcpClient _client = null!;
    private NetworkStream? _stream;
    [NotNullIfNotNull(nameof(_stream))]
    private StreamReader? Reader { get; set; }
    
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new (IPAddress.IPv6Loopback, port: 0);

    public static IPEndPoint? GetAvailableEndpoint() {
        using var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(DefaultLoopbackEndpoint);
        if (socket.LocalEndPoint is IPEndPoint endPoint) {
            return endPoint;
        }
        return null;
    }

    [NotNull]
    private IPEndPoint? _currentEndpoint = DefaultLoopbackEndpoint;
        
    public ConsoleThread(string executable, List<string> args) {
        _executable = executable;
        _args = args;
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
        _currentEndpoint = GetAvailableEndpoint();
        if (_currentEndpoint is null)
            throw new NoNullAllowedException("System does not have any available ports? (0-65535)");

        _listener = new(_currentEndpoint);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        
        _listener.Start();
        
        Command process = Cli.Wrap(_executable.Replace("aerender.exe", "AfterFX.com"))
            .WithArguments(["-noui", "-m", "-s", 
                $"if (typeof gAECommandLineRenderer == 'undefined') {{ app.exitCode = 13; }} else {{ try {{ gAECommandLineRenderer.Render('-port','[::1]:{_currentEndpoint.Port}',{ string.Join(",", _args.Select(s => $"'{s}'")) }); }} catch(e) {{ alert(e); }} }}"
            ])
            // .WithStandardOutputPipe(PipeTarget.ToDelegate(s => Console.WriteLine($"AfterFX OUT: {s}")))
            // .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Console.Error.WriteLine($"AfterFX ERR: {s}")))
            .WithValidation(CommandResultValidation.ZeroExitCode);

        return process;
    }

    private void UpdateStreams() {
        while (!_client.Connected) { }
        _stream = _client.GetStream();
        Reader = new(_stream, Encoding.ASCII, leaveOpen: true);
    }
    
    private async Task ReceiveMessage() {
        try {
            while (State is not (ThreadState.Error or ThreadState.Finished or ThreadState.Stopped)) {
                if (Reader is null) {
                    continue;
                }

                var response = await Reader.ReadLineAsync();
                if (response is null || response.Length == 0) continue;

                if (response.ToLower().Contains("error")) {
                    ErrorReceived?.Invoke(this, response);
                } else {
                    OutputReceived?.Invoke(this, response);
                }
            }
        } catch (Exception e) {
            // TODO: Change to ErrorReceived
            OutputReceived?.Invoke(this, $"Socket error: {e.Message}");
            // await Console.Error.WriteLineAsync($"Error: {e.Message}");
        }
    }
    
    public async Task StartAsync() {
        try {
            State = ThreadState.Running;
            _process.ExecuteAsync(_cancellationToken);
            OutputReceived?.Invoke(this, "AfterFX started, waiting for connection...");
            _client = await _listener.AcceptTcpClientAsync(_cancellationToken);
            UpdateStreams();
            OutputReceived?.Invoke(this, $"Connected to AfterFX on {_currentEndpoint.Address}:{_currentEndpoint.Port}");
            await Task.Run(ReceiveMessage, _cancellationToken);

            // await Task.WhenAll(
            //     _process.ExecuteAsync(_cancellationToken),
            //     Task.Run(async () => {
            //         await _client.ConnectAsync(_currentEndpoint, _cancellationToken).AsTask();
            //         UpdateStreams();
            //         return Task.CompletedTask;
            //     }, _cancellationToken),
            //     Task.Run(ReceiveMessage, _cancellationToken)
            // );
            //await foreach (var evt in _process.ListenAsync(_cancellationToken)) {
            //  await _process.Observe(Encoding.ASCII, _cancellationToken).ForEachAsync(evt => {
            //      switch (evt) {
            //          case StartedCommandEvent started:
            //              State = ThreadState.Running;
            //              break;
            //          case StandardOutputCommandEvent stdOut:
            //              Dispatcher.UIThread.Post(() => OutputReceived?.Invoke(this, stdOut.Text));
            //              break;
            //          case StandardErrorCommandEvent stdErr:
            //              Dispatcher.UIThread.Post(() => ErrorReceived?.Invoke(this, stdErr.Text));
            //              break;
            //          case ExitedCommandEvent exited:
            //              State = ThreadState.Finished;
            //              break;
            //      }
            // }, _cancellationToken);
        } catch (CommandExecutionException e) {
            State = ThreadState.Error;
        } catch (OperationCanceledException e) {
            State = ThreadState.Stopped;
        } finally {
            State = ThreadState.Finished;
        }
    }

    public void Abort() {
        _cts.Cancel();
    }
}