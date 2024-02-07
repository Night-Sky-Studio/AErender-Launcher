using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace AErenderLauncher.Classes;

public class ReactiveObject : IReactiveObject {
    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;
    public void RaisePropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);
    public void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    protected void RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        RaisePropertyChanging(new PropertyChangingEventArgs(propertyName));
        field = value;
        RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}