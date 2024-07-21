using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Collections.Internals
{
    /// <remarks>
    /// https://github.com/meziantou/Meziantou.Framework/blob/347ee70a4cbcf93bfe69ea3e42c110ac7f6faecc/src/Meziantou.Framework.WPF/Collections/Internals/ObservableCollectionBase.cs#L7
    /// </remarks>
    internal abstract class ObservableCollectionBase<T> : INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private protected List<T> Items { get; }

        protected ObservableCollectionBase()
        {
            Items = new List<T>();
        }

        protected ObservableCollectionBase(IEnumerable<T> items)
        {
            if (items is null)
            {
                Items = new List<T>();
            }
            else
            {
                Items = new List<T>(items);
            }
        }

#if NET6_0_OR_GREATER
        public void EnsureCapacity(int capacity)
        {
            Items.EnsureCapacity(capacity);
        }
#elif NET461 || NET462
#else
#error Platform not supported
#endif

        protected void ReplaceItem(int index, T item)
        {
            var oldItem = Items[index];
            Items[index] = item;

            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        protected void InsertItem(int index, T item)
        {
            Items.Insert(index, item);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected void InsertItems(int index, ImmutableList<T> items)
        {
            Items.InsertRange(index, items);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
        }

        protected void AddItem(T item)
        {
            var index = Items.Count;
            Items.Add(item);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected void AddItems(ImmutableList<T> items)
        {
            var index = Items.Count;
            Items.AddRange(items);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
        }

        protected void RemoveItemAt(int index)
        {
            var item = Items[index];
            Items.RemoveAt(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected bool RemoveItem(T item)
        {
            var index = Items.IndexOf(item);
            if (index < 0)
                return false;

            Items.RemoveAt(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        protected void ClearItems()
        {
            Items.Clear();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            CollectionChanged?.Invoke(this, EventArgsCache.ResetCollectionChanged);
        }

        protected void Reset(ImmutableList<T> items)
        {
            Items.Clear();
            Items.AddRange(items);
            OnIndexerPropertyChanged();
            OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
    }
}
