using System;
using AErenderLauncher.Classes;
using CommunityToolkit.Mvvm.ComponentModel;
using Semver;

namespace AErenderLauncher.ViewModels;

public partial class AboutWindowViewModel : ObservableObject {
    [ObservableProperty]
    private SemVersion _version = App.Version;

    public string VersionText => Version.WithoutMetadata().ToString();

    [ObservableProperty]
    private FFmpeg? _ffmpeg = Settings.Current.FFmpeg;

    [ObservableProperty]
    private AfterFx? _afterFx = Settings.Current.AfterEffects;

    public string? FFmpegInfo => Ffmpeg is not null ? $"{Ffmpeg.Version} ({Ffmpeg.Path})" : null;
    public string? AfterFxInfo => AfterFx is not null ? $"{AfterFx.Version} ({AfterFx.Name})" : null;
}