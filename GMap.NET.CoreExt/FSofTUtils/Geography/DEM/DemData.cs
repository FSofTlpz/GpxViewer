using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FSofTUtils.Geography.DEM {
   /// <summary>
   /// liefert die Höhendaten eines Punktes
   /// </summary>
   public class DemData : IDisposable {

      // GeoTif wird noch NICHT direkt unterstützt (sind nicht unbedingt im 1°x1° Raster und haben keine Standardnamen)

      class DemCache {
         protected readonly List<DEM1x1> cache;

         object locker = new object();

         /// <summary>
         /// max. Füllstand
         /// </summary>
         public int MaxSize { get; }

         /// <summary>
         /// akt. Füllstand
         /// </summary>
         public int Size {
            get {
               lock (locker) {
                  return cache.Count;
               }
            }
         }


         public DemCache(int maxsize) {
            MaxSize = maxsize;
            cache = new List<DEM1x1>(maxsize);
         }

         int getPos(double left, double bottom) {
            for (int i = cache.Count - 1; i >= 0; i--)
               if (cache[i].Left == left &&
                   cache[i].Bottom == bottom)
                  return i;
            return -1;
         }

         /// <summary>
         /// fügt ein Element in den Cache ein
         /// </summary>
         /// <param name="t"></param>
         public void Add(DEM1x1 t) {
            lock (locker) {
               int pos = getPos(t.Left, t.Bottom);
               if (pos < 0) { // sonst ist das Objekt schon vorhanden
                  if (cache.Count == MaxSize)
                     cache.RemoveAt(0);
                  cache.Add(t);
               }
            }
         }

         /// <summary>
         /// liefert ein Element und aktualisiert die Position im Cache
         /// </summary>
         /// <param name="cmp"></param>
         /// <returns></returns>
         public DEM1x1 Get(double left, double bottom) {
            int pos = -1;
            DEM1x1 t = null;
            lock (locker) {
               pos = getPos(left, bottom);
               if (pos < 0)
                  return null;
               t = cache[pos];
               cache.RemoveAt(pos);    // an letzte (akt.) Pos. schieben
               cache.Add(t);
            }
            return t;
         }

         /// <summary>
         /// liefert ALLE akt. Elemente
         /// </summary>
         /// <returns></returns>
         public DEM1x1[] GetAll() {
            DEM1x1[] dem = null;
            lock (locker) {
               dem = new DEM1x1[cache.Count];
               cache.CopyTo(dem);
            }
            return dem;
         }

         /// <summary>
         /// löscht den gesamten Cacheinhalt
         /// </summary>
         public void Clear() {
            lock (locker) {
               for (int i = 0; i < cache.Count; i++)
                  cache[i].Dispose();
               cache.Clear();
            }
         }

         public override string ToString() {
            return string.Format("Size {0}, MaxSize {1}", Size, MaxSize);
         }

      }

      string path;
      DemCache demCache;

      int _withHillshade;
      public bool WithHillshade {
         get {
            return Interlocked.Exchange(ref _withHillshade, _withHillshade) != 0;
         }
         set {
            Interlocked.Exchange(ref _withHillshade, value ? 1 : 0);
         }
      }
      double ShadingAzimut = 315;
      double ShadingAltitude = 45;
      double ShadingScale = 2000;
      double ShadingZ = 1;


      public DemData(string path, int maxtilecache) {
         this.path = path;
         demCache = new DemCache(maxtilecache);
      }

      public int GetHeight(double lon, double lat) {
         DEM1x1 dem = getTile(lon, lat);
         double h = dem.InterpolatedHeight(lon, lat, DEM1x1.InterpolationType.standard);
         return h == DEM1x1.NOVALUED ?
                     DEM1x1.DEMNOVALUE :
                     (int)Math.Round(h);
      }

      private const byte V0 = (byte)0;

      public byte GetShadingValue(double lon, double lat) {
         DEM1x1 dem = getTile(lon, lat);
         return dem.ExistsHillShadeData ?
                        dem.InterpolatedShadingValue(lon, lat, DEM1x1.InterpolationType.standard) :
                        V0;
      }

      DEM1x1 getTile(double lon, double lat) {
         int left = (int)Math.Floor(lon);
         int bottom = (int)Math.Floor(lat);
         DEM1x1 dem = demCache.Get(left, bottom);
         if (dem == null) {
            try {
               dem = new DEMHGTReader(left, bottom, path);
               dem.SetDataArray();
            } catch (Exception ex) {
               Debug.WriteLine(string.Format("Exception in DemData.getTile({0}, {0}): {2}", lon, lat, ex.Message));
            }
            if (dem == null ||
                dem.Rows == 0) { // nichts eingelesen
               dem = new DEMNoValues(left, bottom);
               dem.SetDataArray();
            }
            demCache.Add(dem);
         }
         if (WithHillshade && !dem.ExistsHillShadeData)
            dem.ComputeHillShadeData(ShadingAzimut, ShadingAltitude, ShadingScale, ShadingZ);

         return dem;
      }

      public void SetNewHillshadingData(double shadingazimut = 315, double shadingaltitude = 45, double shadingscale = 1, double shadingz = 1) {
         ShadingAzimut = shadingazimut;
         ShadingAltitude = shadingaltitude;
         ShadingScale = shadingscale;
         ShadingZ = shadingz;

         if (WithHillshade) {
            DEM1x1[] dem = demCache.GetAll();
            foreach (var item in dem) {
               item.ComputeHillShadeData(ShadingAzimut, ShadingAltitude, ShadingScale, ShadingZ);
            }
         }
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               demCache.Clear();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
