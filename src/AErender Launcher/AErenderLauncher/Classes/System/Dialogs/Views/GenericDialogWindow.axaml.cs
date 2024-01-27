using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AErenderLauncher.Classes.System.Dialogs.Views;

public partial class GenericDialogWindow : Window {
    private string? Header {
        get => DialogHeader.Text;
        set {
            DialogHeader.Text = value;
            DialogHeader.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    private string? Body {
        get => DialogContent.Text;
        set {
            DialogContent.Text = value;
            DialogContent.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    private string? Link {
        get => DialogLink.Text;
        set {
            DialogLink.Text = value;
            DialogLinkRoot.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    private IImage? HeaderImage {
        get => DialogImage.Source;
        set {
            DialogImage.Source = value;
            DialogImage.IsVisible = value != null;
        }
    }

    private string? PrimaryText {
        get => Primary.Content?.ToString();
        set {
            Primary.Content = value;
            Primary.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    private string? SecondaryText {
        get => Secondary.Content?.ToString();
        set {
            Secondary.Content = value;
            Secondary.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    private string? CancelText {
        get => Cancel.Content?.ToString();
        set {
            Cancel.Content = value;
            Cancel.IsVisible = !string.IsNullOrEmpty(value);
        }
    }
    public GenericDialogWindow() {
        InitializeComponent();
    }
    public GenericDialogWindow(GenericDialogParams @params) {
        InitializeComponent();

        Title = @params.Title;
        Header = @params.Title;
        Body = @params.Body;
        Link = @params.Link;
        HeaderImage = @params.HeaderImage;
        PrimaryText = @params.PrimaryText;
        SecondaryText = @params.SecondaryText;
        CancelText = @params.CancelText;
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e) {
        Close(DialogButtons.Cancel);
    }

    private void Secondary_OnClick(object sender, RoutedEventArgs e) {
        Close(DialogButtons.Secondary);
    }

    private void Primary_OnClick(object sender, RoutedEventArgs e) {
        Close(DialogButtons.Primary);
    }
}