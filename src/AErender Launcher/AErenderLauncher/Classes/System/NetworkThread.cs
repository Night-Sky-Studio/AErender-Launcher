using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

public class NetworkThread : ReactiveObject, IDisposable {
    protected Process Process { get; set; }
    public string Executable { get; }
    public List<string> Args { get; set; }
    
    private readonly CancellationTokenSource _cts = new ();
    private CancellationToken CancelToken => _cts.Token;
    
    protected event Action<NetworkThread?, string>? OutputReceived;
    protected event Action<NetworkThread?, string>? ErrorReceived;
    protected event Action<NetworkThread?, ThreadState>? StateChanged;
    
    private ThreadState _state = ThreadState.Stopped;
    public ThreadState State {
        get => _state;
        protected set {
            _state = value;
            StateChanged?.Invoke(this, value);
        }
    } 
    
    private TcpListener _listener = null!;
    private TcpClient _client = null!;
    private NetworkStream? _stream;
    [NotNullIfNotNull(nameof(_stream))]
    private StreamReader? Reader { get; set; }
    
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new (IPAddress.IPv6Loopback, port: 0);

    private static IPEndPoint? GetAvailableEndpoint() {
        using var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(DefaultLoopbackEndpoint);
        if (socket.LocalEndPoint is IPEndPoint endPoint) {
            return endPoint;
        }
        return null;
    }

    [NotNull]
    private IPEndPoint? _currentEndpoint = DefaultLoopbackEndpoint;

    private Process CreateProcess() {
        _currentEndpoint = GetAvailableEndpoint();
        if (_currentEndpoint is null)
            throw new NoNullAllowedException("System does not have any available ports? (0-65535)");
        
        _listener = new(_currentEndpoint);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        
        _listener.Start();
        
        var arguments = Helpers.Platform == OS.Windows
            ? string.Join(" ", "-noui", "-m", "-s", 
                $$"""
                  if (typeof gAECommandLineRenderer == 'undefined') { 
                      app.exitCode = 13; 
                  } else { 
                      try { 
                          gAECommandLineRenderer.Render('-port','[::1]:{{_currentEndpoint.Port}}',{{string.Join(",", Args.Select(s => $"'{s}'"))}}); 
                      } catch(e) { 
                          alert(e); 
                      } 
                  }
                  """)
            : string.Join(" ", [
                "-noui", 
                .. Args.Select(s => s.Contains(' ') ? $"\"{s}\"" : s), 
                "-port", $"[::1]:{_currentEndpoint.Port}",
                "-aerenderpid", Environment.ProcessId
            ]);
        
        return new () {
            EnableRaisingEvents = true,
            StartInfo = {
                FileName = Executable,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            }
        };;
    }

    protected NetworkThread(string executable, List<string> args) {
        Executable = executable;
        Args = args;
        
        Process = CreateProcess();
        
        OutputReceived += OnOutputReceived;
        ErrorReceived += OnErrorReceived;
        StateChanged += OnStateChanged;
    }

    protected virtual void OnStateChanged(NetworkThread? sender, ThreadState state) { }

    protected virtual void OnErrorReceived(NetworkThread? sender, string output) { }

    protected virtual void OnOutputReceived(NetworkThread? sender, string output) { }

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

                var response = await Reader.ReadLineAsync(CancelToken);
                if (response is null || response.Length == 0) continue;

                if (response.ToLower().Contains("error")) {
                    ErrorReceived?.Invoke(this, $"{response}");
                } else {
                    OutputReceived?.Invoke(this, $"{response}");
                }
            }
        } catch (TaskCanceledException) {
            // yeet
            Console.WriteLine("Thread canceled.");
        } catch (Exception e) {
            ErrorReceived?.Invoke(this, $"Socket error: {e.Message}");
            await Console.Error.WriteLineAsync($"[ERR] Socket error: {e.Message}");
        }
    }
    
    public async Task StartAsync() {
        try {
            State = ThreadState.Running;
            Process.Start();

            OutputReceived?.Invoke(this, "AfterFX started, waiting for connection...");

            _client = await _listener.AcceptTcpClientAsync(CancelToken);
            UpdateStreams();

            OutputReceived?.Invoke(this, $"Connected to AfterFX on {_currentEndpoint.Address}:{_currentEndpoint.Port}");
 
            await Task.Run(ReceiveMessage, CancelToken);
        } catch (CommandExecutionException) {
            State = ThreadState.Error;
        } catch (OperationCanceledException) {
            State = ThreadState.Stopped;
        } catch (Exception e) {
            ErrorReceived?.Invoke(this, $"[ERR] {e.Message}");
            await Console.Error.WriteLineAsync($"[ERR] {e.Message}");
        } finally {
            State = ThreadState.Finished;
        }
    }

    public void Abort() {
        _cts.Cancel();
        if (State != ThreadState.Running) return;
        Process.Kill();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _cts.Dispose();
        _listener.Dispose();
        _client.Dispose();
        _stream?.Dispose();
        Process.Dispose();
        Reader?.Dispose();
        GC.ReRegisterForFinalize(this);
    }

    ~NetworkThread() {
        Dispose();
    }
}