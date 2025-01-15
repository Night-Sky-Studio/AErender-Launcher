using System;
using System.Collections.Generic;
using System.Linq;
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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto), Obsolete("Use TaskDialog")]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [Flags]
        public enum TaskDialogFlags : uint {
            EnableHyperlinks = 0x0001,
            UseHIconMain = 0x0002,
            UseHIconFooter = 0x0004,
            AllowDialogCancellation = 0x0008,
            UseCommandLinks = 0x0010,
            UseCommandLinksNoIcon = 0x0020,
            ExpandFooterArea = 0x0040,
            ExpandedByDefault = 0x0080,
            VerificationFlagChecked = 0x0100,
            ShowProgressBar = 0x0200,
            ShowMarqueeProgressBar = 0x0400,
            CallbackTimer = 0x0800,
            PositionRelativeToWindow = 0x1000,
            RtlLayout = 0x2000,
            NoDefaultRadioButton = 0x4000,
            CanBeMinimized = 0x8000,
            SizeToContent = 0x01000000 // I wish I didn't have to dig this value from CommCtrl.h 
        }

        [Flags]
        public enum TaskDialogCommonButtonFlags : uint {
            OkButton = 0x0001,
            YesButton = 0x0002,
            NoButton = 0x0004,
            CancelButton = 0x0008,
            RetryButton = 0x0010,
            CloseButton = 0x0020
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct TaskDialogConfigIconUnion {
            [FieldOffset(0)] public int hMainIcon;
            [FieldOffset(0)] public int pszIcon;
            [FieldOffset(0)] public IntPtr spacer;
        }
        
        public delegate int TaskDialogCallback(
            IntPtr hwnd,
            uint msg,
            IntPtr wParam,
            IntPtr lParam,
            IntPtr lpRefData);
        
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        public struct TaskDialogConfig {
            public uint cbSize;
            public IntPtr hwndParent;
            public IntPtr hInstance;
            public TaskDialogFlags dwFlags;
            public TaskDialogCommonButtonFlags dwCommonButtons;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszWindowTitle;
            public TaskDialogConfigIconUnion MainIcon;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszMainInstruction;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszContent;
            public uint cButtons;
            public IntPtr pButtons;
            public int nDefaultButton;
            public uint cRadioButtons;
            public IntPtr pRadioButtons;
            public int nDefaultRadioButton;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszVerificationText;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszExpandedInformation;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszExpandedControlText;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszCollapsedControlText;
            public TaskDialogConfigIconUnion FooterIcon;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszFooter;
            public TaskDialogCallback pfCallback;
            public IntPtr lpCallbackData;
            public uint cxWidth;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        public struct TaskDialogButton {
            public int nButtonID;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszButtonText;
        }

        [DllImport("Comctl32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int TaskDialogIndirect(
            [In] ref TaskDialogConfig pTaskConfig,
            out int pnButton, out int pnRadioButton,
            [MarshalAs(UnmanagedType.Bool)] out bool pfVerificationFlagChecked);
    }
    
    public static DialogButton ShowDialog(this Window window, DialogParams @params) {
        var handle = TopLevel.GetTopLevel(window)?.TryGetPlatformHandle()?.Handle;
        if (handle is null || handle == IntPtr.Zero) {
            throw new NullReferenceException("The top-level platform handle is null. Are you running on desktop?");
        }

        if (@params.Body is null || @params.Title is null) {
            throw new ArgumentNullException(nameof(@params), "Body or title is null. Can't display an empty dialog.");
        }

        var buttons = new Native.TaskDialogButton[@params.Buttons.Length];
        for (var i = 0; i < buttons.Length; i++) {
            switch (@params.Buttons[i]) {
                case DialogButton.Primary:
                    buttons[i] = new Native.TaskDialogButton {
                        nButtonID = 0,
                        pszButtonText = @params.PrimaryText
                    };
                    break;
                case DialogButton.Secondary:
                    buttons[i] = new Native.TaskDialogButton {
                        nButtonID = 1,
                        pszButtonText = @params.SecondaryText
                    };
                    break;
                case DialogButton.Cancel:
                    buttons[i] = new Native.TaskDialogButton {
                        nButtonID = 2,
                        pszButtonText = @params.CancelText
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttons), "Other buttons are not supported!");
            }
        }
        
        var pButtons = Marshal.AllocHGlobal(Marshal.SizeOf<Native.TaskDialogButton>() * buttons.Length);
        var currentBtn = pButtons;
        foreach (var button in buttons) {
            Marshal.StructureToPtr(button, currentBtn, false);
            checked {
                currentBtn = (IntPtr)(currentBtn.ToInt64() + Marshal.SizeOf<Native.TaskDialogButton>());
            }
        }
        
        Native.TaskDialogConfig config = new() {
            cbSize = (uint)Marshal.SizeOf<Native.TaskDialogConfig>(),
            hwndParent = handle.Value,
            dwFlags = Native.TaskDialogFlags.UseHIconMain 
                      | Native.TaskDialogFlags.AllowDialogCancellation
                      | Native.TaskDialogFlags.SizeToContent,
            pszWindowTitle = @params.Title,
            pszMainInstruction = @params.Title,
            pszContent = @params.Body,
            pButtons = pButtons,
            cButtons = Convert.ToUInt32(buttons.Length),
            nDefaultButton = buttons.Last().nButtonID
        };

        var result = Native.TaskDialogIndirect(ref config,
            out var pnButton,
            out var pnRadioButton,
            out bool pfVerificationFlagChecked);

        if (result != 0) {
            Console.Error.WriteLine($"TaskDialog failed with HRESULT: 0x{result:X8}");
            Marshal.ThrowExceptionForHR(result);
        }

        Marshal.FreeHGlobal(pButtons);

        return pnButton switch {
            0 => DialogButton.Primary,
            1 => DialogButton.Secondary,
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