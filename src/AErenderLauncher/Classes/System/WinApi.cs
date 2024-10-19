using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AErenderLauncher.Classes.System.Dialogs;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AErenderLauncher.Classes.System;

#if WINDOWS
public static class WinApi {
    private static class Native {
        // ReSharper disable InconsistentNaming
        public const uint MB_YESNOCANCEL = 0x00000003;
        public const uint MB_ICONQUESTION = 0x00000020;
        // ReSharper restore InconsistentNaming
        
        [DllImport("user32.dll", SetLastError = true, CharSet= CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }

    // TODO: Use TaskDialog
    // Using TaskDialog allows for custom buttons captions
    // https://learn.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-taskdialog
    public static DialogButton ShowDialog(this Window window, DialogParams @params) {
        var handle = TopLevel.GetTopLevel(window)?.TryGetPlatformHandle()?.Handle;
        if (handle is null || handle == IntPtr.Zero) {
            throw new NullReferenceException("The top-level platform handle is null. Are you running on desktop?");
        }

        if (@params.Body is null || @params.Title is null) {
            throw new ArgumentNullException(nameof(@params), "Body or title is null. Can't display an empty dialog.");
        }
        
        var result = Native.MessageBox(handle.Value, @params.Body, 
            @params.Title, Native.MB_YESNOCANCEL | Native.MB_ICONQUESTION);

        return result switch {
            // IDYES
            6 => DialogButton.Primary,
            // IDNO
            7 => DialogButton.Secondary,
            // IDCANCEL,
            2 => DialogButton.Cancel,
            _ => throw new NotSupportedException("Other button combinations are not supported yet.")
        };
    }

    public static Task<DialogButton> ShowDialogAsync(this Window window, DialogParams @params) {
        var tcs = new TaskCompletionSource<DialogButton>();

        Dispatcher.UIThread.Invoke(() => {
            try {
                var result = window.ShowDialog(@params);
                tcs.SetResult(result);
            } catch (Exception e) {
                tcs.SetException(e);
            }
        });

        return tcs.Task;
    }
}
#endif