using System;
using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;

namespace AErenderLauncher.ViewModels;

public class SettingsViewModel : ReactiveObject {
    public ObservableCollection<int> ThreadsLimits { get; set; } = new (Enumerable.Range(1, Helpers.GetAvailableCores())
        .Select(i => (int) Math.Pow(2.0, i))
        .Where(i => i <= Helpers.GetAvailableCores() * 2)
        .ToList());
    
    private AfterFx? _afterFx = Settings.Current.AfterEffects;
    public AfterFx? AfterFx {
        get => _afterFx; 
        set => RaiseAndSetIfChanged(ref _afterFx, value);
    }
    private FFmpeg? _ffmpeg = Settings.Current.FFmpeg;
    public FFmpeg? FFmpeg {
        get => _ffmpeg;
        set {
            RaiseAndSetIfChanged(ref _ffmpeg, value);
            RaisePropertyChanged(new (nameof(FFmpegInfo)));
        }
    }

    public string FFmpegInfo => FFmpeg is not null ? $"{FFmpeg.Version} ({FFmpeg.Path})" : "Not found";
    
    private string _outputPath = Settings.Current.DefaultOutputPath;
    public string DefaultOutputPath {
        get => _outputPath; 
        set => RaiseAndSetIfChanged(ref _outputPath, value);
    }
    private string _projectsPath = Settings.Current.DefaultProjectsPath;
    public string DefaultProjectsPath {
        get => _projectsPath; 
        set => RaiseAndSetIfChanged(ref _projectsPath, value);
    }
    
    private RenderingMode _renderingMode = Settings.Current.ThreadsRenderMode;
    public RenderingMode RenderingMode {
        get => _renderingMode; 
        set => RaiseAndSetIfChanged(ref _renderingMode, value);
    }
    
    private int _threadsLimit = Settings.Current.ThreadsLimit;
    public int ThreadsLimit {
        get => _threadsLimit; 
        set => RaiseAndSetIfChanged(ref _threadsLimit, value);
    }

    protected override void RaiseAndSetIfChanged<T>(ref T field, T value, string? propertyName = null) {
        base.RaiseAndSetIfChanged(ref field, value, propertyName);
        WriteToSettings();
    }

    public void WriteToSettings() {
        Settings.Current.AfterEffects = AfterFx;
        Settings.Current.FFmpeg = FFmpeg;
        Settings.Current.DefaultOutputPath = DefaultOutputPath;
        Settings.Current.DefaultProjectsPath = DefaultProjectsPath;
        Settings.Current.ThreadsRenderMode = RenderingMode;
        Settings.Current.ThreadsLimit = ThreadsLimit;
        
        Settings.Current.Save();
    }
}