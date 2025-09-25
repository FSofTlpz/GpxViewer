using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SpecialMapCtrl.NET.ObjectModel {
   public class ThreadSafeEnumerator<T> : IEnumerator<T> {
      // this is the (thread-unsafe) enumerator of the underlying collection
      private readonly IEnumerator<T> _inner;

      // this is the object we shall lock on. 
      private readonly object _lock;

      public ThreadSafeEnumerator(IEnumerator<T> inner, object lockobj) {
         _inner = inner;
         _lock = lockobj;
         // entering lock in constructor
         Monitor.Enter(_lock);
      }

      #region Implementation of IDisposable

      public void Dispose() {
         // .. and exiting lock on Dispose()
         // This will be called when foreach loop finishes
         Monitor.Exit(_lock);
      }

      #endregion

      #region Implementation of IEnumerator

      // we just delegate actual implementation to the inner enumerator, that actually iterates over some collection

      public bool MoveNext() => _inner.MoveNext();

      public void Reset() => _inner.Reset();

      public T Current => _inner.Current;

      object? IEnumerator.Current => Current;

      #endregion
   }
}
