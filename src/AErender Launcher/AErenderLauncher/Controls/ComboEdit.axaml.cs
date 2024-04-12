using System;
using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace AErenderLauncher.Controls;

public partial class ComboEdit : UserControl {
    // public static readonly DirectProperty<ItemsControl, IEnumerable?> ItemsSourceProperty =
    //     AvaloniaProperty.RegisterDirect<ItemsControl, IEnumerable?>(nameof(ItemsSource), 
    //         o => o.ItemsSource, 
    //         (o, v) => o.ItemsSource = v
    //     );
    //
    // public IEnumerable? ItemsSource {
    //     get => _itemsSource; 
    //     set => SetAndRaise(ItemsSourceProperty, ref _itemsSource, value); 
    // }
    // private IEnumerable? _itemsSource = new AvaloniaList<object>();
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<ItemsControl, IEnumerable?>(nameof(ItemsSource));
    
    public IEnumerable? ItemsSource {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// Text
    public static readonly DirectProperty<ComboEdit, string> TextProperty =
        AvaloniaProperty.RegisterDirect<ComboEdit, string>(nameof(Text), o => o.Text, (o, v) => o.Text = v);
    
    // [InheritDataTypeFromItems(nameof(Elements))]
    // public IDataTemplate? ItemTemplate {
    //     get => GetValue(ItemTemplateProperty);
    //     set => SetValue(ItemTemplateProperty, value);
    // }
    // public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
    //     AvaloniaProperty.Register<ItemsControl, IDataTemplate?>(nameof(ItemTemplate));
    
    public string Text {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }
    private string _text = "";
    
    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<EditTextBlock, TextChangedEventArgs>(
            nameof(TextChanged),
            RoutingStrategies.Bubble
        );
    public event EventHandler<TextChangedEventArgs>? TextChanged {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }
    protected virtual void OnTextChanged() {
        var e = new TextChangedEventArgs(TextChangedEvent);
        RaiseEvent(e);
    }

    public ComboEdit() {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (sender is not ComboBox box) return;
        Text = $"{box.SelectedItem}";
    }
    private void ComboEditText_OnTextChanged(object? sender, TextChangedEventArgs e) {
        OnTextChanged();
    }
}