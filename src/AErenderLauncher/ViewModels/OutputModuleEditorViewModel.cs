using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using DynamicData;
using DynamicData.Binding;

namespace AErenderLauncher.ViewModels;

public class OutputModuleEditorViewModel : ReactiveObject {
    private List<OutputModule> InitialState { get; set; } = Settings.Current.OutputModules.Clone();
    public ObservableCollectionEx<OutputModule> OutputModules { get; } = new(Settings.Current.OutputModules);

    public ObservableCollection<string> ProjectButtons { get; } = [
        "[projectName]", "[compName]", "[renderSettingsName]", "[outputModuleName]", "[fileExtension]", "[projectFolder]"
    ];

    public ObservableCollection<string> CompositionButtons { get; } = [
        "[width]", "[height]", "[frameRate]", "[startFrame]", "[endFrame]", "[durationFrames]", "[#####]"
    ];
    
    public ObservableCollection<string> TimecodeButtons { get; } = [
        "[startTimecode]", "[endTimecode]", "[durationTimecode]"
    ];
    
    public ObservableCollection<string> ImageButtons { get; } = [
        "[channels]", "[projectColorDepth]", "[outputColorDepth]", "[compressor]", "[fieldOrder]", "[pulldownPhase]"
    ];
    
    public ObservableCollection<string> DateButtons { get; } = [
        "[dateYear]", "[dateMonth]", "[dateDay]", "[timeHour]", "[timeMins]", "[timeSecs]", "[timeZone]"
    ];
    
    private int _selectedIndex = 0;
    public int SelectedIndex {
        get => _selectedIndex;
        set {
            RaiseAndSetIfChanged(ref _selectedIndex, value); 
            RaisePropertyChanged(new (nameof(SelectedModule)));
        }
    }
    
    public OutputModule? SelectedModule => OutputModules.Get(SelectedIndex);

    public bool IsEdited => !OutputModules.JsonEquals(InitialState);

    public void Add(string module = "Untitled", string mask = "") {
        OutputModules.Add(new (module, mask));
        SelectedIndex = OutputModules.Count - 1;
    }

    public void RemoveSelected() {
        SelectedIndex--;
        OutputModules.RemoveAt(SelectedIndex + 1);
    }
    
    public void Save() {
        InitialState = Settings.Current.OutputModules = OutputModules.ToList();
    }

    public void Restore() {
        OutputModules.Clear();
        OutputModules.AddRange(InitialState);
        Settings.Current.OutputModules = InitialState;
    }
}