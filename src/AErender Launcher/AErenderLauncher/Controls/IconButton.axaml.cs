using System;
using System.Reactive.PlatformServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Controls;

public partial class IconButton : UserControl {
    public static readonly StyledProperty<object> IconProperty =
        AvaloniaProperty.Register<IsPlatform, object>(nameof(Icon));
    
    public object Icon {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalIconAlignmentProperty =
        AvaloniaProperty.Register<IsPlatform, HorizontalAlignment>(nameof(HorizontalIconAlignment));
    
    public HorizontalAlignment HorizontalIconAlignment {
        get => GetValue(HorizontalIconAlignmentProperty);
        set {
            SetValue(HorizontalIconAlignmentProperty, value);
            AlignmentChanged(value);
        }
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<IsPlatform, string>(nameof(Text));
    
    public string Text {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    } 
    
    public IconButton() {
        InitializeComponent();
    }

    private void AlignmentChanged(HorizontalAlignment value) {
        IconContainer.Margin = value switch {
            HorizontalAlignment.Left => new Thickness(8, 0, 0, 0),
            HorizontalAlignment.Center => new Thickness(0, 0, 0, 0),
            HorizontalAlignment.Right => new Thickness(0, 0, 8, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
        // switch (value) {
        //     case HorizontalAlignment.Left:
        //         break;
        //     case HorizontalAlignment.Center:
        //         break;
        //     case HorizontalAlignment.Right:
        //         break;
        // }
    }
    
    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        RoutedEventArgs args = new(ClickEvent, this);
        RaiseEvent(args);
    }
}