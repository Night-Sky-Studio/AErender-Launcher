using System;
using AErenderLauncher.Classes;
using Semver;

namespace AErenderLauncher.ViewModels;

public class AboutWindowViewModel : ReactiveObject {
    private SemVersion _version = App.Version;
    public SemVersion Version {
        get => _version;
        set {
            RaiseAndSetIfChanged(ref _version, value);
            RaisePropertyChanged(new (nameof(VersionText)));
        }
    }

    public string VersionText => _version.WithoutMetadata().ToString();

    private FFmpeg? _ffmpeg = Settings.Current.FFmpeg;
    public FFmpeg? FFmpeg {
        get => _ffmpeg;
        set {
            RaiseAndSetIfChanged(ref _ffmpeg, value);
            RaisePropertyChanged(new(nameof(FFmpegInfo)));
        }
    }

    private AfterFx? _afterFx = Settings.Current.AfterEffects;
    public AfterFx? AfterFx {
        get => _afterFx;
        set {
            RaiseAndSetIfChanged(ref _afterFx, value);
            RaisePropertyChanged(new(nameof(AfterFxInfo)));
        }
    }

    public string? FFmpegInfo => FFmpeg is not null ? $"{FFmpeg.Version} ({FFmpeg.Path})" : null;
    public string? AfterFxInfo => AfterFx is not null ? $"{AfterFx.Version} ({AfterFx.Name})" : null;
}