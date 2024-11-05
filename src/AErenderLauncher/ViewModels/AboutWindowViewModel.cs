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
}