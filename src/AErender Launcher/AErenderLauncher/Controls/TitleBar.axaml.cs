using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AErenderLauncher.Controls;

public partial class TitleBar : UserControl {
    private IImage? _icon;
    public IImage? Icon {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }
    public static readonly DirectProperty<TitleBar, IImage?> IconProperty = 
        AvaloniaProperty.RegisterDirect<TitleBar, IImage?>(nameof(Icon), 
            o => o.Icon, 
            (o, v) => o.Icon = v
        );

    private string? _title;
    public string? Title {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }
    public static readonly DirectProperty<TitleBar, string?> TitleProperty = 
        AvaloniaProperty.RegisterDirect<TitleBar, string?>(nameof(Title), 
            o => o.Title, 
            (o, v) => o.Title = v
        );
    
    
    private Window? _parentWindow;

    private static readonly Geometry ExpandIconData = Geometry.Parse("m0 0 600 0 0 600-150-150 0-300-300 0z");
    private static readonly Geometry CollapseIconData = Geometry.Parse("m600 600-600 0 0-600 150 150 0 300 300 0z");
    
    public TitleBar() {
        InitializeComponent();
        _parentWindow = TopLevel.GetTopLevel(this) as Window;
        // Title ??= _parentWindow?.Title;
    }

    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        if (_parentWindow != null)
            _parentWindow.Position = new PixelPoint(_parentWindow.Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
                _parentWindow.Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_parentWindow is { WindowState: WindowState.Maximized or WindowState.FullScreen }) return;

        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        
        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }

    private void CloseBtn_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        _parentWindow?.Close();
    }

    private void MaximizeBtn_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        if (_parentWindow != null) {
            _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            MaximizeIcon.Data = _parentWindow.WindowState == WindowState.Maximized ? CollapseIconData : ExpandIconData;
        }
    }

    private void Minimize_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        if (_parentWindow != null)
            _parentWindow.WindowState = WindowState.Minimized;
    }
}