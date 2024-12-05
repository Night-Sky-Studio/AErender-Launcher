using AErenderLauncher.Classes.Extensions;
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

    private bool _closeBtnHidesWindow = false;
    public bool CloseBtnHidesWindow {
        get => _closeBtnHidesWindow;
        set => SetAndRaise(CloseBtnHidesWindowProperty, ref _closeBtnHidesWindow, value);
    }
    public static readonly DirectProperty<TitleBar, bool> CloseBtnHidesWindowProperty = 
        AvaloniaProperty.RegisterDirect<TitleBar, bool>(nameof(CloseBtnHidesWindow), 
            o => o.CloseBtnHidesWindow, 
            (o, v) => o.CloseBtnHidesWindow = v
        );
    
    public Control? Toolbar {
        get => GetValue(ToolbarProperty); 
        set => SetValue(ToolbarProperty, value); 
    }
    public static readonly StyledProperty<Control?> ToolbarProperty = 
        AvaloniaProperty.Register<TitleBar, Control?>(nameof(Toolbar));
    
    private Window? _parentWindow;

    private static readonly Geometry ExpandIconData = Geometry.Parse("m0 0 600 0 0 600-150-150 0-300-300 0z");
    private static readonly Geometry CollapseIconData = Geometry.Parse("m600 600-600 0 0-600 150 150 0 300 300 0z");
    
    public TitleBar() {
        InitializeComponent();
        _parentWindow = TopLevel.GetTopLevel(this) as Window;
        Title ??= _parentWindow?.Title;
    }

    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e) {
        if (!_mouseDownForWindowMoving) return;

        if (_parentWindow is null) return;
        PointerPoint currentPoint = e.GetCurrentPoint(this);
        _parentWindow.Position = new (_parentWindow.Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            _parentWindow.Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (_parentWindow is { WindowState: WindowState.Maximized or WindowState.FullScreen }) return;

        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        
        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
        _mouseDownForWindowMoving = false;
    }

    private void CloseBtn_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        if (_closeBtnHidesWindow)
            _parentWindow?.Hide();
        else
            _parentWindow?.Close();
    }

    private void MaximizeBtn_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        if (_parentWindow is null) return;
        _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        MaximizeIcon.Data = _parentWindow.WindowState == WindowState.Maximized ? CollapseIconData : ExpandIconData;
    }

    private void Minimize_OnClick(object? sender, RoutedEventArgs e) {
        _parentWindow ??= TopLevel.GetTopLevel(this) as Window;
        if (_parentWindow is null) return;
        _parentWindow.WindowState = WindowState.Minimized;
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property.Name == nameof(Toolbar)) {
            ToolbarRoot.IsVisible = Toolbar is not null;
            ToolbarRoot.Child = Toolbar;
        }
    }
}