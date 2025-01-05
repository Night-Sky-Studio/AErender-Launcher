using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlTypes;
using AErenderLauncher.Classes.Extensions;

namespace AErenderLauncher.Classes;

public class ItemPropertyChangedEventArgs<T>(T item, string propertyName) : EventArgs {
    public T Item { get; } = item;
    public string PropertyName { get; } = propertyName;
}

public class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged {
    // Define an event that will notify subscribers when any item's property changes
    public event EventHandler<ItemPropertyChangedEventArgs<T>>? ItemPropertyChanged;

    public ObservableCollectionEx(IList<T> collection) : base(collection) {
        SubscribeToItems(collection);
    }

    // Subscribe to PropertyChanged event for each item in the collection
    private void SubscribeToItems(IEnumerable<T> items) {
        foreach (var item in items) {
            item.PropertyChanged += Item_PropertyChanged;
        }
    }

    // Unsubscribe from PropertyChanged event for each item in the collection
    private void UnsubscribeFromItems(IEnumerable<T> items) {
        foreach (var item in items) {
            item.PropertyChanged -= Item_PropertyChanged;
        }
    }

    // Override CollectionChanged event handling to subscribe/unsubscribe when items are added/removed
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        base.OnCollectionChanged(e);

        if (e.NewItems != null) {
            foreach (T item in e.NewItems) {
                if (item != null) {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (T item in e.OldItems) {
                if (item != null) {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }
    }

    // This event will be triggered when any property of any item in the collection changes
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is T item && e.PropertyName is not null) {
            // Raise the custom ItemPropertyChanged event
            ItemPropertyChanged?.Invoke(this, new ItemPropertyChangedEventArgs<T>(item, e.PropertyName));
        }
    }
}