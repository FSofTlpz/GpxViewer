using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpecialMapCtrl.NET.ObjectModel {

   public enum NotifyCollectionChangedAction {
      Add,
      Remove,
      Replace,
      Move,
      Reset
   }

   public class NotifyCollectionChangedEventArgs : EventArgs {
      // Fields
      private NotifyCollectionChangedAction _action;
      private IList? _newItems;
      private int _newStartingIndex;
      private IList? _oldItems;
      private int _oldStartingIndex;

      public NotifyCollectionChangedAction Action => _action;

      public IList? NewItems => _newItems;


      public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action) {
         _newStartingIndex = -1;
         _oldStartingIndex = -1;
         if (action != NotifyCollectionChangedAction.Reset) {
            throw new ArgumentException("WrongActionForCtor", "action");
         }

         InitializeAdd(action, null, -1);
      }

      public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index) {
         _newStartingIndex = -1;
         _oldStartingIndex = -1;
         if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
             action != NotifyCollectionChangedAction.Reset) {
            throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
         }

         if (action == NotifyCollectionChangedAction.Reset) {
            if (changedItem != null) {
               throw new ArgumentException("ResetActionRequiresNullItem", "action");
            }

            if (index != -1) {
               throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");
            }

            InitializeAdd(action, null, -1);
         } else {
            InitializeAddOrRemove(action, new[] { changedItem }, index);
         }
      }

      public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem,
          int index) {
         _newStartingIndex = -1;
         _oldStartingIndex = -1;
         if (action != NotifyCollectionChangedAction.Replace) {
            throw new ArgumentException("WrongActionForCtor", "action");
         }

         InitializeMoveOrReplace(action, new[] { newItem }, new[] { oldItem }, index, index);
      }


      private void InitializeAdd(NotifyCollectionChangedAction action, IList? newItems, int newStartingIndex) {
         _action = action;
         _newItems = newItems == null ? null : ArrayList.ReadOnly(newItems);
         _newStartingIndex = newStartingIndex;
      }

      private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex) {
         if (action == NotifyCollectionChangedAction.Add) {
            InitializeAdd(action, changedItems, startingIndex);
         } else if (action == NotifyCollectionChangedAction.Remove) {
            InitializeRemove(action, changedItems, startingIndex);
         } else {
            throw new ArgumentException(string.Format("InvariantFailure, Unsupported action: {0}",
                action.ToString()));
         }
      }

      private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems,
          int startingIndex, int oldStartingIndex) {
         InitializeAdd(action, newItems, startingIndex);
         InitializeRemove(action, oldItems, oldStartingIndex);
      }

      private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex) {
         _action = action;
         _oldItems = oldItems == null ? null : ArrayList.ReadOnly(oldItems);
         _oldStartingIndex = oldStartingIndex;
      }

   }

   public class ObservableCollection<T> : ICollection<T> { //, INotifyCollectionChanged, INotifyPropertyChanged {
      // Fields
      protected Collection<T> _inner;

      protected object _lock = new object();

      private SimpleMonitor _monitor;
      private const string CountString = "Count";
      private const string IndexerName = "Item[]";

      // Events

      public virtual event EventHandler<NotifyCollectionChangedEventArgs>? CollectionChanged;

      public virtual event EventHandler<PropertyChangedEventArgs>? PropertyChanged;


      public int Count {
         get {
            lock (_lock)
               return _inner.Count;
         }
      }


      // Methods
      public ObservableCollection() {
         _monitor = new SimpleMonitor();
         _inner = new Collection<T>();
      }

      public T this[int index] {
         get {
            lock (_lock)
               return _inner[index];
         }
         set {
            SetItem(index, value);
         }
      }

      public void Add(T? item) => AddItem(item);

      public void Insert(int index, T? item) => InsertItem(index, item);

      public void Clear() {
         ClearItems();
      }

      public bool Contains(T item) {
         lock (_lock)
            return _inner.Contains(item);
      }

      public bool Remove(T item) {
         RemoveItem(_inner.IndexOf(item));
         return true;
      }

      public IEnumerator<T> GetEnumerator() {
         // instead of returning an usafe enumerator, we wrap it into our thread-safe class
         lock (_lock)
            return new ThreadSafeEnumerator<T>(_inner.GetEnumerator(), _lock);
      }

      public int IndexOf(T? item) {
         if (item != null)
            lock (_lock)
               for (int idx = 0; idx < Count; idx++)
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  if (_inner[idx] != null &&
                      _inner[idx].Equals(item))
                     return idx;
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         return -1;
      }



      protected IDisposable BlockReentrancy() {
         _monitor.Enter();
         return _monitor;
      }

      protected void CheckReentrancy() {
         if (_monitor.Busy &&
             CollectionChanged != null &&
             CollectionChanged.GetInvocationList().Length > 1) {
            throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
         }
      }

      protected void ClearItems() {
         lock (_lock) {
            CheckReentrancy();
            _inner.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
         }
      }

      protected void AddItem(T? item) {
         if (item != null)
            lock (_lock) {
               CheckReentrancy();
               _inner.Add(item);
               OnPropertyChanged(CountString);
               OnPropertyChanged(IndexerName);
               OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _inner.Count - 1);
            }
      }

      protected void InsertItem(int index, T? item) {
         if (item != null)
            lock (_lock) {
               CheckReentrancy();
               if (index < 0)
                  index = 0;
               if (index >= _inner.Count)
                  _inner.Add(item);
               else
                  _inner.Insert(index, item);
               OnPropertyChanged(CountString);
               OnPropertyChanged(IndexerName);
               OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
      }

      protected void RemoveItem(int index) {
         lock (_lock) {
            CheckReentrancy();
            var item = _inner[index];
            if (item != null) {
               _inner.RemoveAt(index);
               OnPropertyChanged(CountString);
               OnPropertyChanged(IndexerName);
               OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
         }
      }

      protected void SetItem(int index, T item) {
         if (item != null)
            lock (_lock) {
               CheckReentrancy();
               var oldItem = _inner[index];
               if (oldItem != null) {
                  _inner[index] = item;
                  OnPropertyChanged(IndexerName);
                  OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
               }
            }
      }

      protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
         if (CollectionChanged != null) {
            using (BlockReentrancy()) {
               CollectionChanged(this, e);
            }
         }
      }

      private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index) {
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
      }

      private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem,
          int index) {
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
      }

      private void OnCollectionReset() {
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
         if (PropertyChanged != null) {
            PropertyChanged(this, e);
         }
      }

      private void OnPropertyChanged(string propertyName) {
         OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
      }

      #region ICollection

      public bool IsReadOnly => false;

      public void CopyTo(T[] array, int arrayIndex) {
         //lock (_lock)
         //   _inner.CopyTo(array, arrayIndex);
      }

      IEnumerator IEnumerable.GetEnumerator() {
         return GetEnumerator();
      }

      #endregion

      // Nested Types
      private class SimpleMonitor : IDisposable {
         // Fields
         private int _busyCount;

         public void Dispose() => _busyCount--;

         public void Enter() => _busyCount++;

         // Properties
         public bool Busy => _busyCount > 0;
      }
   }
}
