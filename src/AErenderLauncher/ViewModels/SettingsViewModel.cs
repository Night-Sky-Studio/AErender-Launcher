using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AErenderLauncher.ViewModels;

public partial class SettingsViewModel : ObservableObject {
    public ObservableCollection<int> ThreadsLimits { get; set; } = new (Enumerable.Range(1, Helpers.GetAvailableCores())
        .Select(i => (int) Math.Pow(2.0, i))
        .Where(i => i <= Helpers.GetAvailableCores() * 2)
        .ToList());
    
    [ObservableProperty]
    private AfterFx? _afterFx = Settings.Current.AfterEffects;

    [ObservableProperty]
    private FFmpeg? _ffmpeg = Settings.Current.FFmpeg;

    public string FFmpegInfo => Ffmpeg is not null ? $"{Ffmpeg.Version} ({Ffmpeg.Path})" : "Not found";
    
    [ObservableProperty]
    private string _defaultOutputPath = Settings.Current.DefaultOutputPath;

    [ObservableProperty]
    private string _defaultProjectsPath = Settings.Current.DefaultProjectsPath;

    [ObservableProperty]
    private RenderingMode _renderingMode = Settings.Current.ThreadsRenderMode;

    [ObservableProperty]
    private int _threadsLimit = Settings.Current.ThreadsLimit;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);
        WriteToSettings();
    }

    public void WriteToSettings() {
        Settings.Current.AfterEffects = AfterFx;
        Settings.Current.FFmpeg = Ffmpeg;
        Settings.Current.DefaultOutputPath = DefaultOutputPath;
        Settings.Current.DefaultProjectsPath = DefaultProjectsPath;
        Settings.Current.ThreadsRenderMode = RenderingMode;
        Settings.Current.ThreadsLimit = ThreadsLimit;
        
        Settings.Current.Save();
    }
}