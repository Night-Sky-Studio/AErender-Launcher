using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AErenderLauncher.Classes.System.Dialogs;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AErenderLauncher.Classes.System;

#if MACOS
public static class MacApi {
    private static class Native {
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
        public static extern IntPtr SelRegisterName(string name);
        
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern void ObjcMsgSend(IntPtr receiver, IntPtr selector, bool value);
        
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
        public static extern IntPtr GetClass(string className);
    
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector);
    
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);
    }
    
    private static IntPtr GetNSWindow(Window window) {
        return window.TryGetPlatformHandle() is not { } handle ? IntPtr.Zero : handle.Handle;
    }

    public static void SetDocumentEdited(this Window window, bool edited) {
        var nsWindow = GetNSWindow(window);

        if (nsWindow != IntPtr.Zero) {
            var selector = Native.SelRegisterName("setDocumentEdited:");
            Native.ObjcMsgSend(nsWindow, selector, edited);
        }
    }
    
    public static IntPtr ToNSString(this string str) {
        var nsStringClass = Native.GetClass("NSString");
        var allocSelector = Native.SelRegisterName("alloc");
        var initSelector = Native.SelRegisterName("initWithUTF8String:");
        var nsStringAlloc = Native.ObjcMsgSend(nsStringClass, allocSelector);
        var nsString = Native.ObjcMsgSend(nsStringAlloc, initSelector, Marshal.StringToHGlobalAuto(str));
        return nsString;
    }

    public abstract class NSObject {
        private IntPtr Class { get; set; }
        private IntPtr Handle { get; set; }

        private void Alloc() {
            if (Class == IntPtr.Zero)
                throw new NullReferenceException("Cannot allocate a NULL object");
            
            Handle = Native.ObjcMsgSend(Class, Native.SelRegisterName("alloc"));
        }

        private void Init() {
            Handle = Call("init");
        }

        private void AllocAndInit(string className) {
            Class = Native.GetClass(className);
            Alloc();
            Init();
        }

        protected void AllocAndInit<T>() => AllocAndInit(typeof(T).Name);
        
        protected IntPtr Call(string selector) =>
            Native.ObjcMsgSend(Handle, Native.SelRegisterName(selector));
        protected IntPtr Call(string selector, IntPtr arg1) =>
            Native.ObjcMsgSend(Handle, Native.SelRegisterName(selector), arg1);
    }

    public class NSAlert : NSObject {
        private DialogParams _params = new();

        private DialogParams Options {
            get => _params;
            set {
                _params = value;
                UpdateDialog();
            }
        }
        
        public NSAlert() {
            AllocAndInit<NSAlert>();
        }

        public NSAlert(DialogParams @params) {
            AllocAndInit<NSAlert>();
            Options = @params;
        }

        private void UpdateDialog() {
            if (Options.Title is not null)
                Call("setMessageText:", Options.Title.ToNSString());

            if (Options.Body is not null)
                Call("setInformativeText:", Options.Body.ToNSString());

            foreach (var btn in Options.Buttons) {
                switch (btn) {
                    case DialogButton.Primary:
                        Call("addButtonWithTitle:", Options.PrimaryText.ToNSString());
                        break;
                    case DialogButton.Secondary:
                        Call("addButtonWithTitle:", Options.SecondaryText.ToNSString());
                        break;
                    case DialogButton.Cancel:
                        Call("addButtonWithTitle:", Options.CancelText.ToNSString());
                        break;
                    default:
                        throw new InvalidEnumArgumentException("How did you create a DialogButton?");
                }
            }
        }

        public DialogButton Show() {
            var result = Call("runModal").ToInt32();
            return result switch {
                1000 => DialogButton.Primary,
                1001 => DialogButton.Secondary,
                1002 => DialogButton.Cancel,
                _ => throw new ArgumentOutOfRangeException(nameof(result), $"Unexpected DialogButton: {result}"),
            };
        }

        public Task<DialogButton> ShowAsync() {
            var tcs = new TaskCompletionSource<DialogButton>();

            Dispatcher.UIThread.Invoke(() => {
                try {
                    var result = Show();
                    tcs.SetResult(result);
                } catch (Exception ex) {
                    tcs.SetException(ex);
                }
            });
            
            return tcs.Task;
        }
        
        public static DialogButton ShowDialog(DialogParams @params) {
            var dialog = new NSAlert(@params);
            return dialog.Show();
        }
        
        public static Task<DialogButton> ShowDialogAsync(DialogParams @params) {
            var dialog = new NSAlert(@params);
            return dialog.ShowAsync();
        }
    }
}
#endif