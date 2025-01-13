using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AErenderLauncher.ViewModels;

public partial class TaskEditorViewModel : ObservableObject {
    public static readonly long TotalMemory = Helpers.GetPlatformMemory();
    
    public ObservableCollection<OutputModule> OutputModules { get; set; } = new(Settings.Current.OutputModules);
    public ObservableCollection<string> RenderSettingsPresets { get; set; } = [
        "Best Settings",
        "Current Settings",
        "DV Settings",
        "Draft Settings",
        "Multi-Machine Settings"
    ];

    [ObservableProperty]
    private bool _isEditing;

    private static IEnumerable<double> CalculateMemoryTicks() {
        return Enumerable.Range(0, (int)Math.Log2(TotalMemory) + 1)
            .Select(i => Math.Pow(2, i))
            .Where(i => i >= 1024)
            .Select(i => i / TotalMemory);
    }

    public ObservableCollection<double> MemoryTickMarks { get; } = new(CalculateMemoryTicks());

    [ObservableProperty]
    private string _projectPath = string.Empty;
    
    [ObservableProperty]
    private string _outputPath = Settings.Current.LastOutputPath;
    
    [ObservableProperty]
    private int _selectedOutputModule = Settings.Current.OutputModuleIndex;

    [ObservableProperty] 
    private string _renderSettings = Settings.Current.RenderSettings;
    
    [ObservableProperty]
    private bool _missingFiles = Settings.Current.MissingFiles;

    [ObservableProperty] 
    private bool _sound = Settings.Current.Sound;

    [ObservableProperty] 
    private bool _multiprocessing = Settings.Current.Sound;
    
    [ObservableProperty]
    private string _customProperties = Settings.Current.CustomProperties;

    [ObservableProperty] 
    private bool _customPropertiesEnabled;

    // Stored as percentages of all available RAM
    // Adobe decided that way, not me...
    public const double MaxCacheAndMemoryLimit = 100d;
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CacheLimitString))] 
    private double _cacheLimit = Settings.Current.CacheLimit;
    public string CacheLimitString => MaxCacheAndMemoryLimit - CacheLimit < 0.001 ? "Unlimited" : $"{Math.Truncate(CacheLimit)}%";
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(MemoryLimitString))]
    private double _memoryLimit = Settings.Current.MemoryLimit;
    public string MemoryLimitString => MaxCacheAndMemoryLimit - MemoryLimit < 0.001 ? "Unlimited" : $"{Math.Truncate(MemoryLimit / 100 * TotalMemory)} MB";
    
    public ObservableCollection<Composition> Compositions { get; private set; } = [];
    
    public TaskEditorViewModel() { }
    
    public TaskEditorViewModel(string projectPath, bool isEditing = false) {
        IsEditing = isEditing;
        ProjectPath = projectPath;
    }

    public TaskEditorViewModel(RenderTask renderTask, bool isEditing = false) {
        IsEditing = isEditing;
        ProjectPath = renderTask.Project;
        OutputPath = renderTask.Output;
        SelectedOutputModule = OutputModules.FindIndex(om => om.Module == renderTask.OutputModule);
        RenderSettings = renderTask.RenderSettings;
        
        MissingFiles = renderTask.MissingFiles;
        Sound = renderTask.Sound;
        Multiprocessing = renderTask.Multiprocessing;

        CustomPropertiesEnabled = renderTask.CustomProperties != "";
        CustomProperties = renderTask.CustomProperties;
        
        CacheLimit = renderTask.CacheLimit;
        MemoryLimit = renderTask.MemoryLimit;
        
        Compositions = new(renderTask.Compositions);
    }

    public RenderTask ToRenderTask() => new RenderTask {
        Project = ProjectPath,
        Output = OutputPath,
        OutputModule = OutputModules[SelectedOutputModule].Module,
        RenderSettings = RenderSettings,
        MissingFiles = MissingFiles,
        Sound = Sound,
        Multiprocessing = Multiprocessing,
        CustomProperties = CustomPropertiesEnabled ? CustomProperties : "",
        CacheLimit = CacheLimit,
        MemoryLimit = MemoryLimit,
        Compositions = Compositions.ToList()
    };
}