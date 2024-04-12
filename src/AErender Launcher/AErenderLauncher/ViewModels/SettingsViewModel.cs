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
    
    private string _aerenderPath = App.ApplicationSettings.AErenderPath;
    public string AErenderPath {
        get => _aerenderPath; 
        set => RaiseAndSetIfChanged(ref _aerenderPath, value);
    }
    private string _outputPath = App.ApplicationSettings.DefaultOutputPath;
    public string DefaultOutputPath {
        get => _outputPath; 
        set => RaiseAndSetIfChanged(ref _outputPath, value);
    }
    private string _projectsPath = App.ApplicationSettings.DefaultProjectsPath;
    public string DefaultProjectsPath {
        get => _projectsPath; 
        set => RaiseAndSetIfChanged(ref _projectsPath, value);
    }
    
    private RenderingMode _renderingMode = App.ApplicationSettings.ThreadsRenderMode;
    public RenderingMode RenderingMode {
        get => _renderingMode; 
        set => RaiseAndSetIfChanged(ref _renderingMode, value);
    }
    
    private int _threadsLimit = App.ApplicationSettings.ThreadsLimit;
    public int ThreadsLimit {
        get => _threadsLimit; 
        set => RaiseAndSetIfChanged(ref _threadsLimit, value);
    }
    
    public void WriteToSettings() {
        App.ApplicationSettings.AErenderPath = AErenderPath;
        App.ApplicationSettings.DefaultOutputPath = DefaultOutputPath;
        App.ApplicationSettings.DefaultProjectsPath = DefaultProjectsPath;
        App.ApplicationSettings.ThreadsRenderMode = RenderingMode;
        App.ApplicationSettings.ThreadsLimit = ThreadsLimit;
        
        App.ApplicationSettings.Save();
    }
}