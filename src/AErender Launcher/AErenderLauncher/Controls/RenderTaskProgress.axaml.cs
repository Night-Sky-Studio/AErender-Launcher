using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Controls; 

public partial class RenderTaskProgress : UserControl {
    public static readonly DirectProperty<RenderTaskProgress, int> CurrentFrameProperty =
        AvaloniaProperty.RegisterDirect<RenderTaskProgress, int>(nameof(CurrentFrame), o => o.CurrentFrame, (o, v) => o.CurrentFrame = v);
    public int CurrentFrame {
        get => _currentFrame;
        set => SetAndRaise(CurrentFrameProperty, ref _currentFrame, value);
    }
    private int _currentFrame;
    
    public static readonly DirectProperty<RenderTaskProgress, int> EndFrameProperty =
        AvaloniaProperty.RegisterDirect<RenderTaskProgress, int>(nameof(EndFrame), o => o.EndFrame, (o, v) => o.EndFrame = v);
    public int EndFrame {
        get => _endFrame;
        set => SetAndRaise(EndFrameProperty, ref _endFrame, value);
    }
    private int _endFrame;

    public static readonly DirectProperty<RenderTaskProgress, string> FramesProperty =
        AvaloniaProperty.RegisterDirect<RenderTaskProgress, string>(nameof(Frames), o => o.Frames);
    public string Frames => $"{CurrentFrame} / {EndFrame}";
    
    public static readonly DirectProperty<RenderTaskProgress, string> CompProperty =
        AvaloniaProperty.RegisterDirect<RenderTaskProgress, string>(nameof(Comp), o => o.Comp, (o, v) => o.Comp = v);
    public string Comp {
        get => _comp;
        set => SetAndRaise(CompProperty, ref _comp, value);
    }
    private string _comp = "";

    public static readonly StyledProperty<object> ItemsProperty = 
        AvaloniaProperty.Register<IsPlatform, object>(nameof(Items));
    public object Items {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    
    
    public RenderTaskProgress() {
        InitializeComponent();
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e) {
        if ((sender as ToggleButton)!.IsChecked == true) {
            Root.Height = 244;
            Root.RowDefinitions = new RowDefinitions("48,196");
        } else {
            Root.Height = 48;
            Root.RowDefinitions = new RowDefinitions("48,0");
        }
    }
}