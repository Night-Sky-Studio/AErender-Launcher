using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AErenderLauncher.Controls;

public partial class RenderThreadProgress : UserControl {
    
    public static readonly StyledProperty<uint> CurrentFrameProperty =
        AvaloniaProperty.Register<RenderThreadProgress, uint>(nameof(CurrentFrame));
    
    public uint CurrentFrame {
        get => GetValue(CurrentFrameProperty);
        set => SetValue(CurrentFrameProperty, value);
    }
    
    public static readonly StyledProperty<uint> EndFrameProperty =
        AvaloniaProperty.Register<RenderThreadProgress, uint>(nameof(EndFrame));
    
    public uint EndFrame {
        get => GetValue(EndFrameProperty);
        set => SetValue(EndFrameProperty, value);
    }
    
    public static readonly StyledProperty<string> LogProperty =
        AvaloniaProperty.Register<RenderThreadProgress, string>(nameof(Log));
    
    public string Log {
        get => GetValue(LogProperty);
        set => SetValue(LogProperty, value);
    }
    
    public static readonly StyledProperty<string> CompositionProperty =
        AvaloniaProperty.Register<RenderThreadProgress, string>(nameof(Composition));
    
    public string Composition {
        get => GetValue(CompositionProperty);
        set => SetValue(CompositionProperty, value);
    }
    
    public float Percentage => (float)CurrentFrame / EndFrame * 100;
    public bool GotError => CurrentFrame == uint.MaxValue && EndFrame == uint.MaxValue;
    public bool WaitingForAerender => CurrentFrame == 0 && EndFrame == 0;
    public bool Finished => CurrentFrame == EndFrame;

    private void UpdateText() {
        if (WaitingForAerender) {
            StatusText.Text = "Waiting for aerender...";
            RenderProgressBar.IsIndeterminate = true;
            return;
        }
        
        if (GotError) {
            StatusText.Text = "ERROR: See log for more details";
            // RenderProgressBar.IsIndeterminate = true;
            return;
        }
        
        StatusText.Text = $"{CurrentFrame} / {EndFrame} ({Percentage:F1}%)";
        // RenderProgressBar.Maximum = Progress.EndFrame;
        // RenderProgressBar.Value = Progress.CurrentFrame;
        RenderProgressBar.IsIndeterminate = false;
        
        if (CurrentFrame == EndFrame) {
            StatusText.Text = "Rendering finished";
        }
    }

    public RenderThreadProgress() {
        InitializeComponent();
        UpdateText();
    }


    private void LogBtn_IsCheckedChanged(object? sender, RoutedEventArgs e) {
        Root.Height = LogBtn.IsChecked == true ? 240 : 48;
    }

    private void RenderProgressBar_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        UpdateText();
    }

    private void LogBox_OnTextChanged(object? sender, TextChangedEventArgs e) {
        LogBox.CaretIndex = int.MaxValue;
    }
}