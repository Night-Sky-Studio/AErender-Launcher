using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Views.Dialogs; 

public partial class AErenderDetectDialog : Window {
    private List<string> _paths { get; set; } = new List<string>();
    public string _output { get; private set; } = "";
    
    public AErenderDetectDialog() {
        InitializeComponent();
    }

    public AErenderDetectDialog(List<string> Paths) {
        InitializeComponent();
        _paths = Paths;
    }

    private void Dialog_Closed(object? sender, EventArgs e) { ;
    }
}