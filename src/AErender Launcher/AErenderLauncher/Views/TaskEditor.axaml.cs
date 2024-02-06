using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System.Dialogs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using static AErenderLauncher.App;
namespace AErenderLauncher.Views;

public partial class TaskEditor : Window {
    public RenderTask Task { get; set; }
    public ObservableCollection<OutputModule> outputModules { get; set; } = new(ApplicationSettings.OutputModules);
    public ObservableCollection<string> renderSettings { get; set; } = new() {
        "Best Settings",
        "Current Settings",
        "DV Settings",
        "Draft Settings",
        "Multi-Machine Settings"
    };

    public bool IsEditing { get; set; } = false;
    
    private long _totalMemory = Helpers.GetPlatformMemory();
    
    private static List<double> CalculateMemoryMarks() {
        var memory = Helpers.GetPlatformMemory();

        return Enumerable.Range(0, (int)Math.Log2(memory) + 1)
            .Select(i => Math.Pow(2, i))
            .Where(i => i >= 1024)
            .Select(i => i / memory)
            .ToList();
    }

    public AvaloniaList<double> MemoryTickMarks { get; set; } = new(CalculateMemoryMarks());
    
    public TaskEditor() {
        InitializeComponent();
        Task = new RenderTask();
    }

    public TaskEditor(RenderTask task, bool isEditing = false) {
        InitializeComponent();
        IsEditing = isEditing;

        Task = task;

        // TODO:    Make bindings work
        // Remarks: If somebody will be able to make two-way
        //          bindings work without using "viewmodel" hell,
        //          I will be very grateful
        ProjectPath.Text = task.Project;
        OutputPath.Text = task.Output;
        OutputModuleBox.SelectedIndex = outputModules.IndexOf(outputModules.First(x => x.Module == task.OutputModule));
        RenderSettings.Text = task.RenderSettings;
        
        MissingCheckbox.IsChecked = task.MissingFiles;
        SoundCheckbox.IsChecked = task.Sound;
        ThreadedCheckbox.IsChecked = task.Multiprocessing;
        
        CustomCheckbox.IsChecked = task.CustomProperties != "";
        CustomProperties.Text = task.CustomProperties;
        
        CacheSlider.Value = task.CacheLimit;
        MemorySlider.Value = task.MemoryLimit;

    }
    private void CompositionsButton_OnClick(object? sender, RoutedEventArgs e) => EditorCarousel.Next();
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close(null);
    private void ProjectSetupButton_OnClick(object? sender, RoutedEventArgs e) => EditorCarousel.Previous();
    private void SaveTaskButton_OnClick(object? sender, RoutedEventArgs e) => Close(Task);
    
    private async void OutputPathButton_OnClick(object? sender, RoutedEventArgs e) {
        IStorageFile? file = await this.ShowSaveFileDialogAsync(
            [],// [ new ("[fileExtension]", "*.[fileExtension]") ],
            SuggestedFileName: outputModules[OutputModuleBox.SelectedIndex].Mask
        );

        if (file == null) return;

        if (file.TryGetLocalPath() is { } path) {
            OutputPath.Text = path;
            Task.Output = path;
        }
    }
    
    private void MemorySlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        MemoryTextBlock.Text = MemorySlider.Maximum - e.NewValue < 0.0000001 ? "Unlimited" : $"{Math.Truncate(e.NewValue / 100 * _totalMemory)} MB";
        Task.MemoryLimit = e.NewValue;
    }
    private void CacheSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        CacheTextBlock.Text = CacheSlider.Maximum - e.NewValue < 0.0000001 ? "Unlimited" : $"{Math.Truncate(e.NewValue)}%";
        Task.CacheLimit = e.NewValue;
    }

    private bool TryParseCache(string input, out double result) {
        if (input.EndsWith("%")) {
            if (double.TryParse(input.TrimEnd('%'), out result)) {
                result = Math.Clamp(result, 0, 100);
                return true;
            }
        }
        else if (double.TryParse(input, out result)) {
            result = Math.Clamp(result, 0, CacheSlider.Maximum);
            return true;
        }

        result = 0;
        return false;
    }
    private void CacheTextBlock_OnSubmit(object? sender, RoutedEventArgs e) {
        if (CacheTextBlock.Text.ToLower().StartsWith("unl"))
            CacheSlider.Value = CacheSlider.Maximum;
        
        if (TryParseCache(CacheTextBlock.Text, out var r))
            CacheSlider.Value = r;
    }
    private bool TryParseMemory(string input, out double result) {
        if (input.EndsWith("MB")) {
            if (double.TryParse(input.Delete("MB"), out result)) { // mb -> %
                result = Math.Clamp(result, 0, _totalMemory) / _totalMemory * 100;
                return true;
            }
        } else if (input.EndsWith("GB")) {
            if (double.TryParse(input.Delete("GB"), out result)) { // gb -> %
                result = Math.Clamp(result, 0, _totalMemory) / _totalMemory * 1024 * 100;
                return true;
            }
        } else if (input.EndsWith("%")) {
            if (double.TryParse(input.Delete("%"), out result)) { // % -> %
                result = Math.Clamp(result, 0, 100);
                return true;
            }
        } else if (double.TryParse(input, out result)) { // default (mb) -> %
            result = Math.Clamp(result, 0, _totalMemory) / _totalMemory * 100;
            return true;
        }

        result = 0;
        return false;
    }
    private void MemoryTextBlock_OnSubmit(object? sender, RoutedEventArgs e) {
        if (MemoryTextBlock.Text.ToLower().StartsWith("unl"))
            MemorySlider.Value = MemorySlider.Maximum;
        
        if (TryParseMemory(MemoryTextBlock.Text, out var r))
            MemorySlider.Value = r;
    }
    private void PropsCheckbox_OnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        // update all checkboxes at the same time
        Task.MissingFiles = MissingCheckbox.IsChecked ?? false;
        Task.Sound = SoundCheckbox.IsChecked ?? false;
        Task.Multiprocessing = ThreadedCheckbox.IsChecked ?? false;
    }
    private void CustomProperties_OnTextChanged(object? sender, TextChangedEventArgs e) =>
        Task.CustomProperties = CustomProperties.Text ?? "";
    
    private void OutputModuleBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        Task.OutputModule = outputModules[OutputModuleBox.SelectedIndex].Module;

    private void RenderSettings_OnTextChanged(object? sender, TextChangedEventArgs e) {
        Task.RenderSettings = RenderSettings.Text;
    }
}