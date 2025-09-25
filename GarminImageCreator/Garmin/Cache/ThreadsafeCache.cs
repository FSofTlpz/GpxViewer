using System.Collections.Generic;
using System.Diagnostics;

namespace GarminImageCreator.Garmin.Cache {

   /// <summary>
   /// it is a threadsafe cache vor objects from type T
   /// </summary>
   /// <typeparam name="T"></typeparam>
   abstract public class ThreadsafeCache<T> {
      protected readonly List<T> cache;
      readonly int maxsize;
      readonly object access_lock = new object();

      /// <summary>
      /// max. Füllstand
      /// </summary>
      public int MaxSize {
         get {
            return maxsize;
         }
      }

      /// <summary>
      /// akt. Füllstand
      /// </summary>
      public int Size {
         get {
            lock (access_lock) {
               return cache.Count;
            }
         }
      }


      public ThreadsafeCache(int maxsize) {
         this.maxsize = maxsize;
         cache = new List<T>(maxsize);
      }

      protected abstract bool found(T t, object obj);

      int getPos(object? obj) {
         if (obj != null)
            for (int i = cache.Count - 1; i >= 0; i--)
               if (found(cache[i], obj))
                  return i;
         return -1;
      }

      /// <summary>
      /// fügt ein Element in den Cache ein
      /// </summary>
      /// <param name="t"></param>
      public void Add(T t) {
         lock (access_lock) {
            int pos = getPos(t);
            if (pos < 0) { // sonst ist das Objekt schon vorhanden
               if (cache.Count == maxsize && maxsize > 0) {
                  Debug.WriteLine("Cache is full: remove " + (t != null ? t.ToString() : "?"));
                  cache.RemoveAt(0);
               }
               if (cache.Count < maxsize)
                  cache.Add(t);
            }
         }
      }

      /// <summary>
      /// liefert ein Element und aktualisiert die Position im Cache
      /// </summary>
      /// <param name="cmp"></param>
      /// <returns></returns>
      protected T? Get(object cmp) {
         T? t = default;
         lock (access_lock) {
            int pos = getPos(cmp);
            if (pos < 0)
               return default;
            t = cache[pos];
            cache.RemoveAt(pos);    // an letzte (akt.) Pos. schieben
            cache.Add(t);
         }
         return t;
      }

      /// <summary>
      /// löscht den gesamten Cacheinhalt
      /// </summary>
      public void Clear() {
         lock (access_lock) {
            cache.Clear();
         }
      }

      public override string ToString() {
         return string.Format("Size {0}, MaxSize {1}", Size, MaxSize);
      }

   }

}
