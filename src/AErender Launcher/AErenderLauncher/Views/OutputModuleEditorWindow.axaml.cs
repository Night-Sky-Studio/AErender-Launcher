using System;
using System.Diagnostics;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Extensions;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System;
using AErenderLauncher.Classes.System.Dialogs;
using AErenderLauncher.Classes.System.Dialogs.Views;
using AErenderLauncher.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Views;

public partial class OutputModuleEditorWindow : Window {
    private OutputModuleEditorViewModel ViewModel { get; } = new();

    private void Init() {
        DataContext = ViewModel;

        ViewModel.OutputModules.ItemPropertyChanged += OutputModulesOnItemPropertyChanged;

        InitializeComponent();

        ExtendClientAreaToDecorationsHint = Helpers.Platform != OS.macOS;
        Root.RowDefinitions =
            Helpers.Platform == OS.macOS ? new RowDefinitions("0,32,*") : new RowDefinitions("32,32,*");
        
        MaskTextBox.AddHandler(DragDrop.DragOverEvent, MaskTextBox_OnDragOver);
        MaskTextBox.AddHandler(DragDrop.DropEvent, MaskTextBox_OnDrop);
    }

    private void OutputModulesOnItemPropertyChanged(object? sender, ItemPropertyChangedEventArgs<OutputModule> e) {
#if MACOS
        this.SetDocumentEdited(ViewModel.IsEdited);
#endif
    }

    public OutputModuleEditorWindow() {
        Init();
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Save();
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Restore();
        Close();
    }

    private void AddModuleButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Add();
    }

    private void RemoveModuleButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.RemoveSelected();
    }

    private async void OMEditorWindow_OnClosing(object? sender, WindowClosingEventArgs e) {
        if (ViewModel.IsEdited) {
            var result = await this.ShowConfirmationDialogAsync(new() {
                Title = "Confirm",
                Body = "Output modules was modified. Save changes?",
                Buttons = DialogButtons.All,
                PrimaryText = "Save",
                SecondaryText = "Do not save"
            });

            switch (result) {
                case DialogButton.Primary:
                    ViewModel.Save();
                    e.Cancel = false;
                    break;
                case DialogButton.Secondary:
                    ViewModel.Restore();
                    e.Cancel = false;
                    break;
                case DialogButton.Cancel:
                default:
                    e.Cancel = true;
                    break;
            }
        }
    }

    // Handle the DragOver event to specify the effect
    private void MaskTextBox_OnDragOver(object? sender, DragEventArgs e) {
        // Check if the data being dragged contains a string (button content)
        if (e.Data.Contains(DataFormats.Text)) {
            e.DragEffects = DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy;
        } else {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    // Handle the Drop event to set the TextBox content
    private void MaskTextBox_OnDrop(object? sender, DragEventArgs e) {
        if (e.Data.Contains(DataFormats.Text)) {
            var text = e.Data.GetText();
            if (text is null) return;
            if (sender is TextBox textBox) {
                var pos = textBox.CaretIndex;
                textBox.Text = textBox.Text?.Insert(pos, text);
                textBox.CaretIndex = pos + text.Length;
                textBox.Focus();
            }
        }

        e.Handled = true;
    }

    /// This is so dumb...
    /// 
    /// For some reason, it seems like Avalonia's <see cref="Button"/> ignores <see cref="InputElement.PointerPressed"/>
    /// event, instead relying on <see cref="Button.Click"/> event. But Click event does not provide any
    /// <see cref="PointerEventArgs"/> that are required for the <see cref="DragDrop.DoDragDrop"/>. So we capture those
    /// args from <see cref="InputElement.PointerMoved"/> event and give them to DragDrop later.
    private PointerEventArgs? _pointer;
    private void FlagButton_OnPointerMoved(object? sender, PointerEventArgs e) {
        _pointer = e;
    }
    private void FlagButton_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is Button button) {
            if (button.Content is not string || _pointer is null) return;
            
            var data = new DataObject();
            data.Set("Text", button.Content);

            DragDrop.DoDragDrop(_pointer, data, DragDropEffects.Move | DragDropEffects.Copy);
            e.Handled = true;
        }
    }


}