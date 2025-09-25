using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// eine (rudimentär) threadsichere generische Liste
   /// <para>Es sind beliebig viele parallele Lesevorgänge möglich.</para>
   /// <para>Die Veränderung eines Listenobjektes selbst ist NICHT threadsicher.</para>
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class ListTS<T> : IList<T> {


      protected List<T> _interalList;

      ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

      /// <summary>
      /// Anzahl der Objekte
      /// </summary>
      public int Count {
         get {
            try {
               EnterReadLock();
               return _interalList.Count;
            } finally {
               ExitReadLock();
            }
         }
      }

      public bool IsReadOnly => ((ICollection<T>)_interalList).IsReadOnly;

      public List<T> InternalList => _interalList;


      public ListTS() {
         _interalList = new List<T>();
      }

      public ListTS(int startsize) {
         _interalList = new List<T>(startsize);
      }

      public ListTS(IList<T> data) {
         _interalList = new List<T>(data);
      }

      public ListTS(ListTS<T> data) {
         _interalList = new List<T>(data._interalList);
      }

      /// <summary>
      /// Tries to enter the lock in read mode. (Läßt parallele ReadLocks zu)
      /// <para>Multiple threads can enter read mode at the same time.</para>
      /// <para>This method blocks until the calling thread enters the lock, and therefore might never return!</para>
      /// <para>If one or more threads are waiting to enter write mode, a thread that calls the EnterReadLock method 
      /// blocks until those threads have either timed out or entered write mode and then exited from it.</para>
      /// <para>If a lock allows recursion, a thread that has entered the lock in read mode can enter 
      /// read mode recursively, even if other threads are waiting to enter write mode.</para>
      /// </summary>
      public void EnterReadLock() => readerWriterLock.EnterReadLock();

      public void ExitReadLock() => readerWriterLock.ExitReadLock();

      /// <summary>
      /// Tries to enter the lock in write mode. (Exklusiver Lock nur für diesen Thread)
      /// <para>If other threads have entered the lock in read mode, a thread that calls the EnterWriteLock method 
      /// blocks until those threads have exited read mode.</para>
      /// <para>When there are threads waiting to enter write mode, additional threads that try to enter read mode 
      /// or upgradeable mode block until all the threads waiting to enter write mode have either timed out or 
      /// entered write mode and then exited from it.</para>
      /// <para></para>
      /// <para></para>
      /// <para>This method blocks until the calling thread enters the lock, and therefore might never return!</para>
      /// <para>If a lock allows recursion, a thread that has entered the lock in write mode can enter 
      /// write mode recursively, even if other threads are waiting to enter write mode.</para>
      /// </summary>
      public void EnterWriteLock() => readerWriterLock.EnterWriteLock();

      public void ExitWriteLock() => readerWriterLock.ExitWriteLock();

      public bool Contains(T item) {
         try {
            EnterReadLock();
            return _interalList.Contains(item);
         } finally {
            ExitReadLock();
         }
      }

      public void CopyTo(T[] array, int arrayIndex) {
         try {
            EnterReadLock();
            _interalList.CopyTo(array, arrayIndex);
         } finally {
            ExitReadLock();
         }
      }

      /// <summary>
      /// kopiert alle Elemente in eine Standardliste
      /// </summary>
      /// <returns></returns>
      public List<T> GetCopy() {
         try {
            EnterReadLock();
            return new List<T>(_interalList);
         } finally {
            ExitReadLock();
         }
      }

      public T[] ToArray() {
         try {
            EnterReadLock();
            return _interalList.ToArray();
         } finally {
            ExitReadLock();
         }
      }


      public T this[int index] {
         get {
            try {
               EnterReadLock();
               return ((IList<T>)_interalList)[index];
            } finally {
               ExitReadLock();
            }
         }
         set {
            try {
               EnterWriteLock();
               _interalList[index] = value;
            } finally {
               ExitWriteLock();
            }
         }
      }

      public void Clear() {
         try {
            EnterWriteLock();
            _interalList.Clear();
         } finally {
            ExitWriteLock();
         }
      }

      public void Add(T item) {
         try {
            EnterWriteLock();
            _interalList.Add(item);
         } finally {
            ExitWriteLock();
         }
      }

      public void Insert(int idx, T item) {
         try {
            EnterWriteLock();
            _interalList.Insert(idx, item);
         } finally {
            ExitWriteLock();
         }
      }

      public void InsertRange(int idx, IList<T> collection) {
         try {
            EnterWriteLock();
            _interalList.InsertRange(idx, collection);
         } finally {
            ExitWriteLock();
         }
      }

      public void InsertRange(int idx, ListTS<T> collection) {
         try {
            EnterWriteLock();
            _interalList.InsertRange(idx, collection._interalList);
         } finally {
            ExitWriteLock();
         }
      }

      public int IndexOf(T item) {
         try {
            EnterReadLock();
            return _interalList.IndexOf(item);
         } finally {
            ExitReadLock();
         }
      }

      public bool Remove(T item) {
         try {
            EnterWriteLock();
            return _interalList.Remove(item);
         } finally {
            ExitWriteLock();
         }
      }

      public void RemoveAt(int index) {
         try {
            EnterWriteLock();
            _interalList.RemoveAt(index);
         } finally {
            ExitWriteLock();
         }
      }

      public void RemoveRange(int index, int count) {
         try {
            EnterWriteLock();
            _interalList.RemoveRange(index, count);
         } finally {
            ExitWriteLock();
         }
      }

      public void AddRange(IEnumerable<T> collection) {
         try {
            EnterWriteLock();
            _interalList.AddRange(collection);
         } finally {
            ExitWriteLock();
         }
      }

      public void AddRange(ListTS<T> collection) {
         try {
            EnterWriteLock();
            _interalList.AddRange(collection._interalList);
         } finally {
            ExitWriteLock();
         }
      }

      public void Reverse() {
         try {
            EnterWriteLock();
            if (_interalList.Count > 1) {
               T tmp;
               int lastidx = _interalList.Count - 1;
               int idxend = lastidx / 2;
               if (lastidx % 2 == 0)
                  idxend--;
               for (int i = 0; i <= lastidx / 2; i++) {
                  tmp = _interalList[i];
                  _interalList[i] = _interalList[lastidx - i];
                  _interalList[lastidx - i] = tmp;
               }
            }
         } finally {
            ExitWriteLock();
         }
      }

      public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_interalList).GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() {
         try {
            EnterReadLock();
            return ((IEnumerable)_interalList).GetEnumerator();
         } finally {
            ExitReadLock();
         }
      }
   }
}
