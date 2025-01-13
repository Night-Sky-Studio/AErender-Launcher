using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System.Dialogs;
using AErenderLauncher.ViewModels;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
    private TaskEditorViewModel ViewModel { get; } = new ();
    
    public TaskEditor() {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public TaskEditor(RenderTask task, bool isEditing = false) {
        ViewModel = new TaskEditorViewModel(task, isEditing);
        
        InitializeComponent();
        
        DataContext = ViewModel;
    }
    private void CompositionsButton_OnClick(object? sender, RoutedEventArgs e) => EditorCarousel.Next();
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close(null);
    private void ProjectSetupButton_OnClick(object? sender, RoutedEventArgs e) => EditorCarousel.Previous();
    private void SaveTaskButton_OnClick(object? sender, RoutedEventArgs e) => Close(ViewModel.ToRenderTask());
    
    private async void OutputPathButton_OnClick(object? sender, RoutedEventArgs e) {
        IStorageFile? file = await this.ShowSaveFileDialogAsync(
            [],// [ new ("[fileExtension]", "*.[fileExtension]") ],
            suggestedFileName: ViewModel.OutputModules[ViewModel.SelectedOutputModule].Mask,
            startingPath: Settings.Current.DefaultOutputPath
        );

        if (file == null) return;

        if (file.TryGetLocalPath() is { } path) {
            OutputPath.Text = path;
            ViewModel.OutputPath = path;
        }
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
        if (CacheTextBlock.Text.StartsWith("unl", StringComparison.InvariantCultureIgnoreCase))
            ViewModel.CacheLimit = TaskEditorViewModel.MaxCacheAndMemoryLimit;
        
        if (TryParseCache(CacheTextBlock.Text, out var r))
            ViewModel.CacheLimit = r;
    }
    private bool TryParseMemory(string input, out double result) {
        if (input.EndsWith("MB")) {
            if (double.TryParse(input.Delete("MB"), out result)) { // mb -> %
                result = Math.Clamp(result, 0, TaskEditorViewModel.TotalMemory) / TaskEditorViewModel.TotalMemory * 100;
                return true;
            }
        } else if (input.EndsWith("GB")) {
            if (double.TryParse(input.Delete("GB"), out result)) { // gb -> %
                result = Math.Clamp(result, 0, TaskEditorViewModel.TotalMemory) / TaskEditorViewModel.TotalMemory * 1024 * 100;
                return true;
            }
        } else if (input.EndsWith("%")) {
            if (double.TryParse(input.Delete("%"), out result)) { // % -> %
                result = Math.Clamp(result, 0, 100);
                return true;
            }
        } else if (double.TryParse(input, out result)) { // default (mb) -> %
            result = Math.Clamp(result, 0, TaskEditorViewModel.TotalMemory) / TaskEditorViewModel.TotalMemory * 100;
            return true;
        }

        result = 0;
        return false;
    }
    private void MemoryTextBlock_OnSubmit(object? sender, RoutedEventArgs e) {
        if (MemoryTextBlock.Text.StartsWith("unl", StringComparison.InvariantCultureIgnoreCase))
            ViewModel.MemoryLimit = TaskEditorViewModel.MaxCacheAndMemoryLimit;
        
        if (TryParseMemory(MemoryTextBlock.Text, out var r))
            ViewModel.MemoryLimit = r;
    }

    private void RemoveComp_OnClick(object? sender, RoutedEventArgs e) {
        if (CompList.SelectedItem is Composition comp) {
            CompList.SelectedIndex -= 1;
            ViewModel.Compositions.Remove(comp);
        }
    }
    private void AddComp_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Compositions.Add(new Composition("", new FrameSpan(0, 1), 1));
        CompList.SelectedIndex = CompList.Items.Count - 1;
    }
    private void CompList_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Composition comp) {
            CompName.Text = comp.CompositionName;
            StartFrame.Text = comp.Frames.StartFrame.ToString();
            EndFrame.Text = comp.Frames.EndFrame.ToString();
            Split.Text = comp.Split.ToString();
        }
    }
    private void CompName_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (CompList.SelectedItem is not Composition comp) return;
        comp.CompositionName = CompName.Text!;
    }
    private void StartFrame_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (CompList.SelectedItem is not Composition comp) return;
        if (!uint.TryParse(StartFrame.Text, out uint result)) return;
        comp.Frames = comp.Frames with {
            StartFrame = result
        };
    }
    private void EndFrame_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (CompList.SelectedItem is not Composition comp) return;
        if (!uint.TryParse(EndFrame.Text, out uint result)) return;
        comp.Frames = comp.Frames with {
            EndFrame = result
        };
    }
    private void Split_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (CompList.SelectedItem is not Composition comp) return;
        if (!uint.TryParse(Split.Text, out uint result)) return;
        comp.Split = result;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) {
        // <PageSlide Duration="0.5" Orientation="Horizontal" SlideInEasing="CircularEaseInOut" SlideOutEasing="CircularEaseInOut" />
        // Applying transition this way, allows us to double-click on composition in MainWindow to open it here
        EditorCarousel.PageTransition = new PageSlide(TimeSpan.FromMilliseconds(500)) {
            SlideInEasing = new CircularEaseInOut(),
            SlideOutEasing = new CircularEaseInOut()
        };
    }
}