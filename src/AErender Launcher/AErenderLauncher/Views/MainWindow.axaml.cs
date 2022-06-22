using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace AErenderLauncher.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e) {
            // await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams() {
            //     SystemDecorations = SystemDecorations.BorderOnly,
            //     ContentTitle = "Header",
            //     ContentMessage = "Message",
            //     ShowInCenter = true,
            //     ButtonDefinitions = ButtonEnum.Ok,
            //     CanResize = true
            // }).ShowDialog(this);
        }
    }
}