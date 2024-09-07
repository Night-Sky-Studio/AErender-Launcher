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
    
    private string _aerenderPath = Settings.Current.AfterEffectsPath;
    public string AErenderPath {
        get => _aerenderPath; 
        set => RaiseAndSetIfChanged(ref _aerenderPath, value);
    }
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
    
    public void WriteToSettings() {
        Settings.Current.AfterEffectsPath = AErenderPath;
        Settings.Current.DefaultOutputPath = DefaultOutputPath;
        Settings.Current.DefaultProjectsPath = DefaultProjectsPath;
        Settings.Current.ThreadsRenderMode = RenderingMode;
        Settings.Current.ThreadsLimit = ThreadsLimit;
        
        Settings.Current.Save();
    }
}