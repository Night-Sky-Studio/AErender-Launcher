using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AErenderLauncher.Controls;

public partial class EditTextBlock : UserControl {
    public static readonly DirectProperty<EditTextBlock, string> TextProperty =
        AvaloniaProperty.RegisterDirect<EditTextBlock, string>(nameof(Text), 
            o => o.Text, 
            (o, v) => o.Text = v
        );

    public string Text {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }
    private string _text = "";

    public static readonly DirectProperty<EditTextBlock, bool> IsEditingProperty =
        AvaloniaProperty.RegisterDirect<EditTextBlock, bool>(nameof(IsEditing), 
            o => o.IsEditing, 
            (o, v) => o.IsEditing = v
        );

    public bool IsEditing {
        get => _isEditing;
        set => SetAndRaise(IsEditingProperty, ref _isEditing, value);
    }

    private bool _isEditing = false;

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<TextBlock, TextAlignment>(nameof(TextAlignment));
    
    public TextAlignment TextAlignment {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> SubmitEvent =
        RoutedEvent.Register<EditTextBlock, RoutedEventArgs>(nameof(Submit), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Submit {
        add => AddHandler(SubmitEvent, value);
        remove => RemoveHandler(SubmitEvent, value);
    }
    
    protected virtual void OnSubmit() {
        var e = new RoutedEventArgs(SubmitEvent);
        RaiseEvent(e);
    }
    
    public EditTextBlock() {
        InitializeComponent();
    }

    private void Field_OnKeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Enter) {
            IsEditing = false;
            OnSubmit();
        }
    }

    private void Label_OnTapped(object sender, TappedEventArgs e) {
        IsEditing = true;
    }

    private void InputElement_OnLostFocus(object sender, RoutedEventArgs e) {
        IsEditing = false;
    }
}