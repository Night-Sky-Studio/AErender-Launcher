using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AErenderLauncher.Controls; 

public partial class GroupBox : UserControl {
    /// Title
    public static readonly DirectProperty<GroupBox, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<GroupBox, string>(nameof(Title), o => o.Title, (o, v) => o.Title = v);

    public string Title {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }
    private string _title = "";
    
    public static readonly AttachedProperty<IBrush> StrokeProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, IBrush>(
            nameof(Foreground),
            Brushes.Black,
            inherits: true);

    public IBrush Stroke {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public static readonly DirectProperty<GroupBox, Avalonia.Controls.Controls> ItemsProperty =
        AvaloniaProperty.RegisterDirect<GroupBox, Avalonia.Controls.Controls>(nameof(Items), o => o.Items, (o, v) => o.Items = v);
    
    public Avalonia.Controls.Controls Items {
        get => _items; 
        set => SetAndRaise(ItemsProperty, ref _items, value); 
    }
    private Avalonia.Controls.Controls _items = new Avalonia.Controls.Controls();

    //public Avalonia.Controls.Controls Items { get; } = new Avalonia.Controls.Controls();
    
    public GroupBox() {
        InitializeComponent();
    }
}