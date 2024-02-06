using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AErenderLauncher.Classes.System.Dialogs.Views;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DynamicData.Kernel;

namespace AErenderLauncher.Classes.System.Dialogs;

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
    
    public static async Task<DialogButtons> ShowGenericDialogAsync(this Window OwnerWindow, GenericDialogParams @params) {
        return await MakeGenericDialog(@params).ShowDialog<DialogButtons>(OwnerWindow);
    }

    public static async Task<List<IStorageFile>?> ShowOpenFileDialogAsync(this Window OwnerWindow, List<Tuple<string, string>> Filters, string StartingPath = "", bool AllowMultiple = false) {
        var provider = TopLevel.GetTopLevel(OwnerWindow)?.StorageProvider;
        if (provider == null) return null;
        
        List<FilePickerFileType> filter = new();
        foreach (Tuple<string, string> tuple in Filters) {
            filter.Add(new(tuple.Item1) {
                Patterns = new List<string> { tuple.Item2 }
            });
        }
        
        return (await provider.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = AllowMultiple,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(StartingPath),
            FileTypeFilter = Filters.Count > 0 ? filter : null
        })).AsList();
    }
    
    public static async Task<IStorageFile?> ShowSaveFileDialogAsync(this Window OwnerWindow, List<Tuple<string, string>> Filters, string StartingPath = "", string SuggestedFileName = "Untitled") {
        var provider = TopLevel.GetTopLevel(OwnerWindow)?.StorageProvider;
        if (provider == null) return null;
        
        List<FilePickerFileType> filter = new();
        foreach (Tuple<string, string> tuple in Filters) {
            filter.Add(new(tuple.Item1) {
                Patterns = new List<string> { tuple.Item2 }
            });
        }
        
        return await provider.SaveFilePickerAsync(new FilePickerSaveOptions {
            SuggestedFileName = SuggestedFileName,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(StartingPath),
            FileTypeChoices = Filters.Count > 0 ? filter : null
        });
    }
}