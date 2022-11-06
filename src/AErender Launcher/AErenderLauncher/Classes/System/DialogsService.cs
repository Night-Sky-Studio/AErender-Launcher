using System.Threading.Tasks;
using AErenderLauncher.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.Media;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace AErenderLauncher.Classes.System;

public enum DialogButtons {
    Primary, Secondary, Cancel
}

public class DialogParams {
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Link { get; set; }
    public string? PrimaryText { get; set; } = "";
    public string? SecondaryText { get; set; } = "";
    public string? CancelText { get; set; } = "Cancel";
    public DialogButtons[] Buttons { get; set; } = { DialogButtons.Cancel };
}

public class GenericDialogParams : DialogParams {
    public IImage? HeaderImage { get; set; }
}

public static class DialogsService {
    public static GenericDialogWindow MakeGenericDialog(GenericDialogParams @params) {
        return new(@params);
    } 
    
    public static async Task ShowGenericDialogAsync(this Window OwnerWindow, GenericDialogParams @params) {
        await MakeGenericDialog(@params).ShowDialog(OwnerWindow);
    }
}