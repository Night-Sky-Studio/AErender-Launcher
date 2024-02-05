using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Controls;

public partial class RenderThreadProgress : UserControl {
    public static readonly StyledProperty<RenderThread> RenderThreadProperty =
        AvaloniaProperty.Register<RenderThreadProgress, RenderThread>(nameof(RenderThread));
    
    public RenderThread RenderThread {
        get => GetValue(RenderThreadProperty);
        set => SetValue(RenderThreadProperty, value);
    }
    
    public RenderThreadProgress() {
        InitializeComponent();
        RenderThread.OnProgressChanged += RenderThreadOnOnProgressChanged;
    }

    private void RenderThreadOnOnProgressChanged(RenderProgress progress, string data) {
        if (progress.WaitingForAerender) {
            StatusText.Text = "Waiting for aerender...";
            Progress.IsIndeterminate = true;
            return;
        }
        
        if (progress.GotError) {
            StatusText.Text = "ERROR: See log for more details";
            Progress.IsIndeterminate = true;
            return;
        }
        
        StatusText.Text = $"{progress.CurrentFrame} / {progress.EndFrame} ({progress.Percentage}%)";
        Progress.Maximum = progress.EndFrame;
        Progress.Value = progress.CurrentFrame;
        Progress.IsIndeterminate = false;
        
        if (progress.CurrentFrame == progress.EndFrame) {
            StatusText.Text = "Rendering finished";
        }
    }

    private void LogBtn_IsCheckedChanged(object? sender, RoutedEventArgs e) {
        Root.Height = LogBtn.IsChecked == true ? 240 : 48;
    }
}