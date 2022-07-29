using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace AErenderLauncher.Controls;

public partial class ComboEdit : UserControl {
    /// Items
    public static readonly DirectProperty<ItemsControl, IEnumerable?> ElementsProperty =
        AvaloniaProperty.RegisterDirect<ItemsControl, IEnumerable?>(nameof(Elements), o => o.Items, (o, v) => o.Items = v);

    public IEnumerable? Elements {
        get => _elements; 
        set => SetAndRaise(ElementsProperty, ref _elements, value); 
    }
    private IEnumerable? _elements = new AvaloniaList<object>();

    /// Text
    public static readonly DirectProperty<ComboEdit, string> TextProperty =
        AvaloniaProperty.RegisterDirect<ComboEdit, string>(nameof(Text), o => o.Text, (o, v) => o.Text = v);

    public string Text {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }
    private string _text = "";

    public ComboEdit() {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        Text = (ComboEditBox.SelectedItem as ComboBoxItem)?.Content as string ?? "";
    }
}