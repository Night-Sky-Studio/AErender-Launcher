using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AErenderLauncher.Classes.System.Dialogs.Views;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DynamicData.Kernel;

namespace AErenderLauncher.Classes.System.Dialogs;

public enum DialogButton {
    Primary,
    Secondary,
    Cancel
}

public static class DialogButtons {
    public static readonly DialogButton[] PrimaryCancel = [DialogButton.Primary, DialogButton.Cancel];
    public static readonly DialogButton[] All = [DialogButton.Primary, DialogButton.Secondary, DialogButton.Cancel];
}

public class DialogParams {
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Link { get; set; }
    public string PrimaryText { get; set; } = "Yes";
    public string SecondaryText { get; set; } = "No";
    public string CancelText { get; set; } = "Cancel";
    public DialogButton[] Buttons { get; set; } = [DialogButton.Cancel];
}

public class GenericDialogParams : DialogParams {
    public IImage? HeaderImage { get; set; }
}

public static class DialogService {
    public static GenericDialogWindow MakeGenericDialog(GenericDialogParams @params) {
        return new(@params);
    }

    public static async Task<DialogButton?> ShowGenericDialogAsync(this Window ownerWindow,
        GenericDialogParams @params) {
        return await MakeGenericDialog(@params).ShowDialog<DialogButton?>(ownerWindow);
    }

    public static async Task<DialogButton> ShowConfirmationDialogAsync(this Window window, DialogParams @params) {
#if MACOS
        return await MacApi.NSAlert.ShowDialogAsync(@params);
#else
        return await window.ShowDialogAsync(@params);
#endif
    }

    public static async Task ShowAlertAsync(this Window window, string title, string message) {
        var @params = new DialogParams {
            Title = title,
            Body = message,
            Buttons = [DialogButton.Cancel]
        };
#if MACOS
        await MacApi.NSAlert.ShowDialogAsync(@params);
#else
        await window.ShowDialogAsync(@params);
#endif
    }

    public static async Task<IReadOnlyList<IStorageFile>?> ShowOpenFileDialogAsync(this Window ownerWindow,
        List<Tuple<string, string>> filters, string startingPath = "", bool allowMultiple = false) {
        var provider = TopLevel.GetTopLevel(ownerWindow)?.StorageProvider;
        if (provider == null) return null;

        List<FilePickerFileType> filter = new();
        foreach (Tuple<string, string> tuple in filters) {
            filter.Add(new(tuple.Item1) {
                Patterns = new List<string> { tuple.Item2 }
            });
        }

        return (await provider.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = allowMultiple,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(startingPath),
            FileTypeFilter = filters.Count > 0 ? filter : null
        })).AsList();
    }

    public static async Task<IStorageFile?> ShowSaveFileDialogAsync(this Window ownerWindow,
        List<Tuple<string, string>> filters, string startingPath = "", string suggestedFileName = "Untitled") {
        var provider = TopLevel.GetTopLevel(ownerWindow)?.StorageProvider;
        if (provider == null) return null;

        List<FilePickerFileType> filter = new();
        foreach (Tuple<string, string> tuple in filters) {
            filter.Add(new(tuple.Item1) {
                Patterns = new List<string> { tuple.Item2 }
            });
        }

        return await provider.SaveFilePickerAsync(new FilePickerSaveOptions {
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(startingPath),
            FileTypeChoices = filters.Count > 0 ? filter : null
        });
    }

    public static async Task<IReadOnlyList<IStorageFolder>?> ShowOpenFolderDialogAsync(this Window ownerWindow,
        string startingPath = "", bool allowMultiple = false) {
        var provider = TopLevel.GetTopLevel(ownerWindow)?.StorageProvider;
        if (provider is null) return null;

        return await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            AllowMultiple = allowMultiple,
            SuggestedStartLocation = await provider.TryGetFolderFromPathAsync(startingPath)
        });
    }
}