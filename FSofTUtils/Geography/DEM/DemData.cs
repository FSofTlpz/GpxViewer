using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FSofTUtils.Geography.DEM {
   /// <summary>
   /// liefert die Höhendaten eines Punktes
   /// </summary>
   public class DemData : IDisposable {

      // GeoTif wird noch NICHT direkt unterstützt (sind nicht unbedingt im 1°x1° Raster und haben keine Standardnamen)

      class DemCache {
         /// <summary>
         /// Liste im Hauptspeicher: die akt. Position ist am Ende
         /// </summary>
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

         /// <summary>
         /// Pfad zum Cache im Dateisystem (nur sinnvoll bei Shading)
         /// </summary>
         string cachepath;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="maxsize">max. Anzahl der Elemente im Hauptspeicher</param>
         /// <param name="cachepath">Pfad zum Cache im Dateisystem (nur sinnvoll bei Shading; sonst besser <see cref="string.Empty"/>)</param>
         public DemCache(int maxsize, string cachepath) {
            MaxSize = maxsize;
            cache = new List<DEM1x1>(maxsize);
            this.cachepath = "";
            if (!Directory.Exists(cachepath)) {
               try {
                  Directory.CreateDirectory(cachepath);
                  this.cachepath = cachepath;
               } catch { }
            } else
               this.cachepath = cachepath;
         }

         int getPosInMem(double left, double bottom) {
            for (int i = cache.Count - 1; i >= 0; i--)
               if (cache[i].Left == left &&
                   cache[i].Bottom == bottom)
                  return i;
            return -1;
         }

         /// <summary>
         /// fügt ein Element in den Cache ein
         /// </summary>
         /// <param name="dem"></param>
         public void Add(DEM1x1 dem) {
            lock (locker) {
               int pos = getPosInMem(dem.Left, dem.Bottom);
               if (pos < 0) {    // sonst ist das Objekt schon vorhanden
                  if (cache.Count >= MaxSize) {
                     if (cachepath != string.Empty)
                        writeToCachePath(cache[0]);
                     cache.RemoveAt(0);   // "ältestes" Element entfernen
                  }
                  cache.Add(dem);  // an letzte (aktuellste) Position
               }
            }
         }

         /// <summary>
         /// liefert ein Element und aktualisiert die Position im Cache
         /// </summary>
         /// <param name="cmp"></param>
         /// <returns></returns>
         public DEM1x1? Get(int left, int bottom) {
            int pos = -1;
            DEM1x1? dem = null;
            lock (locker) {
               pos = getPosInMem(left, bottom);
               if (pos < 0) {          // nicht im Hauptspeicher
                  if (cachepath != string.Empty) {
                     dem = readFromCachePath(left, bottom);
                     if (dem != null)
                        Add(dem);
                  }
                  return dem;
               }
               dem = cache[pos];
               cache.RemoveAt(pos);    // an letzte (akt.) Pos. schieben
               cache.Add(dem);
            }
            return dem;
         }

         /// <summary>
         /// liefert ALLE akt. Elemente im Hauptspeicher
         /// </summary>
         /// <returns></returns>
         public DEM1x1[] GetAll() {
            DEM1x1[]? dem = null;
            lock (locker) {
               dem = new DEM1x1[cache.Count];
               cache.CopyTo(dem);
            }
            return dem;
         }

         /// <summary>
         /// löscht den gesamten Cacheinhalt
         /// </summary>
         /// <param name="onlymem">bei true nur im Hauptspeicher</param>
         public void Clear(bool onlymem = true) {
            lock (locker) {
               for (int i = 0; i < cache.Count; i++)
                  cache[i].Dispose();
               cache.Clear();
               if (!onlymem) {
                  //DirectoryInfo di = new DirectoryInfo(cachepath);
                  //foreach (var item in di.EnumerateFiles(cachepath, SearchOption.TopDirectoryOnly)) {
                  //   File.Delete(item.FullName);
                  //}
                  new DirectoryInfo(cachepath).Delete(true);
               }
            }
         }

         string getCacheName(int left, int bottom) {
            string name = Path.Combine(cachepath, "dem");
            name += left < 0 ? "-" : "+";
            name += Math.Abs(left).ToString("d3");
            name += bottom < 0 ? "-" : "+";
            name += Math.Abs(bottom).ToString("d2");
            return name;
         }

         //DEM1x1 readFromCachePath1(int left, int bottom) {
         //   string name = getCacheName(left, bottom);
         //   if (File.Exists(name)) {
         //      using (Stream openFileStream = File.OpenRead(name)) {
         //         BinaryFormatter deserializer = new BinaryFormatter();
         //         return deserializer.Deserialize(openFileStream) as DEM1x1;
         //      }
         //   }
         //   return null;
         //}

         //void writeToCachePath1(DEM1x1 dem) {
         //   string name = getCacheName((int)dem.Left, (int)dem.Bottom);
         //   if (!File.Exists(name)) {
         //      using (Stream saveFileStream = File.Create(name)) {
         //         BinaryFormatter serializer = new BinaryFormatter();
         //         serializer.Serialize(saveFileStream, dem);
         //      }
         //   }
         //}

         /// <summary>
         /// bei einem Lesefehler wird versucht, die Datei zu löschen
         /// </summary>
         /// <param name="left"></param>
         /// <param name="bottom"></param>
         /// <returns></returns>
         DEM1x1? readFromCachePath(int left, int bottom) {
            string name = getCacheName(left, bottom);
            if (File.Exists(name)) {
               try {
                  using (FileStream zipstream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                     if (zipstream != null) {
                        using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Read)) {
                           if (zip.Entries.Count > 0) {
                              ZipArchiveEntry entry = zip.Entries[0];
                              using (Stream dat = entry.Open()) {
                                 //BinaryFormatter deserializer = new BinaryFormatter();
                                 //return deserializer.Deserialize(dat) as DEM1x1;

                                 XmlSerializer serializer = new XmlSerializer(typeof(DEM1x1));
                                 return serializer.Deserialize(dat) as DEM1x1;
                              }
                           }
                        }
                     }
                  }
               } catch (Exception ex) {
                  tryDeleteFile(name, 20);
               }
            }
            return null;
         }

         /// <summary>
         /// bei einem Schreibfehler wird versucht, die Datei zu löschen
         /// </summary>
         /// <param name="dem"></param>
         void writeToCachePath(DEM1x1 dem) {
            string name = getCacheName((int)dem.Left, (int)dem.Bottom);
            if (!File.Exists(name)) {
               try {
                  using (FileStream zipstream = new FileStream(name, FileMode.Create, FileAccess.ReadWrite, FileShare.None)) {
                     if (zipstream != null) {
                        using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Update)) {
                           ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Fastest);
                           using (Stream sw = entry.Open()) {
                              //BinaryFormatter serializer = new BinaryFormatter();
                              //serializer.Serialize(sw, dem);

                              XmlSerializer serializer = new XmlSerializer(typeof(DEM1x1));
                              serializer.Serialize(sw, dem);
                           }
                        }
                     }
                  }
               } catch {
                  tryDeleteFile(name, 20);
               }
            }
         }

         static void tryDeleteFile(string name, int trys) {
            for (int i = 0; i < trys; i++) {
               try {
                  if (File.Exists(name))
                     File.Delete(name);
                  break;
               } catch {
                  Thread.Sleep(50);
               }
            }
         }


         /*
            FileStream zipstream = null;

            if (File.Exists(filename + ".zip"))
               zipstream = new FileStream(filename + ".zip", FileMode.Open, FileAccess.Read, FileShare.Read);
            else if (File.Exists(filename.Substring(0, filename.Length - 4) + ".zip"))
               zipstream = new FileStream(filename.Substring(0, filename.Length - 4) + ".zip", FileMode.Open, FileAccess.Read, FileShare.Read);

            if (zipstream != null) {

               using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Read)) {
                  filename = Path.GetFileName(filename).ToUpper();
                  ZipArchiveEntry entry = null;
                  foreach (var item in zip.Entries) {
                     if (filename == item.Name.ToUpper()) {
                        entry = item;
                        break;
                     }
                  }
                  if (entry == null)
                     throw new Exception(string.Format("file '{0}.zip' not include file '{0}'.", filename));
                  Stream dat = entry.Open();
                  ReadFromStream(dat, entry.Length);
                  dat.Close();
               }
               zipstream.Dispose();

          */

         public override string ToString() {
            return string.Format("Size {0}, MaxSize {1}, cachepath {2}", Size, MaxSize, cachepath);
         }

      }

      /// <summary>
      /// verhindert, das für eine bestimmte Koordinate gleichzeitig und mehrfach ein Ladeprozess startet
      /// </summary>
      class DemTilePipeline {

         class TaskWithCounter {
            public Task<DEM1x1?> task;
            int counter;

            public TaskWithCounter(Task<DEM1x1?> t, int count = 0) {
               this.task = t;
               counter = count;
            }

            /// <summary>
            /// Inkrementiert den Zähler und liefert den neuen Wert
            /// </summary>
            /// <returns></returns>
            public int Increment() => Interlocked.Increment(ref counter);

            /// <summary>
            /// Inkrementiert den Zähler und liefert den neuen Wert
            /// </summary>
            /// <returns></returns>
            public int Decrement() => Interlocked.Decrement(ref counter);

            /// <summary>
            /// liefert den akt. Zählerstand
            /// </summary>
            public int Counter => Interlocked.Add(ref counter, 0);

         }

         Dictionary<string, TaskWithCounter> gettiletasks = new Dictionary<string, TaskWithCounter>();

         object locker = new object();


         public DemTilePipeline() { }


         // Problem: Wenn das Tile angefordert wird, wird es SOFORT mit seinen Daten benötigt.

         public DEM1x1? GetTile(DemData dd, int lon, int lat) {
            string taskname = (lon < 0 ? "-" : "+") + Math.Abs(lon).ToString("d3") +
                              (lat < 0 ? "-" : "+") + Math.Abs(lat).ToString("d2");
            TaskWithCounter? twc;

            lock (locker) {
               if (!gettiletasks.TryGetValue(taskname, out twc)) {
                  twc = new TaskWithCounter(Task.Run(() => dd.getTile(lon, lat)), 1);
                  gettiletasks.Add(taskname, twc);
               } else {
                  int count = twc.Increment();

                  //Debug.WriteLine("!!! DemTilePipeline: Increment() -> " + count);

               }
            }
            twc?.task.Wait();
            DEM1x1? result = twc?.task.Result;

            lock (locker) {
               if (twc?.Decrement() == 0)
                  gettiletasks.Remove(taskname);

               //Debug.WriteLine("!!! DemTilePipeline Count=" + gettiletasks.Count);
            }

            return result;
         }


         //public DEM1x1? GetTile1(DemData dd, int lon, int lat) {
         //   string taskname = (lon < 0 ? "-" : "+") + Math.Abs(lon).ToString("d3") +
         //                     (lat < 0 ? "-" : "+") + Math.Abs(lat).ToString("d2");
         //   TaskWithCounter? twc;

         //   lock (locker) {
         //      if (!gettiletasks.TryGetValue(taskname, out twc)) {
         //         Task<DEM1x1?> t = Task.Run(() => dd.getTile(lon, lat));
         //         if (t != null) {
         //            twc = new TaskWithCounter(t, 1);
         //            gettiletasks.Add(taskname, twc);
         //            twc.t.Start();
         //         }
         //      } else {
         //         int count = twc.Increment();

         //         //Debug.WriteLine("!!! DemTilePipeline: Increment() -> " + count);

         //      }
         //   }
         //   twc?.t.Wait();
         //   DEM1x1? result = twc?.t.Result;

         //   lock (locker) {
         //      if (twc?.Decrement() == 0)
         //         gettiletasks.Remove(taskname);

         //      //Debug.WriteLine("!!! DemTilePipeline Count=" + gettiletasks.Count);
         //   }

         //   return result;
         //}
      }

      DemCache demCache;

      DemTilePipeline demTilePipeline = new DemTilePipeline();

      string path;

      int _withHillshade;
      public bool WithHillshade {
         get {
            return Interlocked.Exchange(ref _withHillshade, _withHillshade) != 0;
         }
         set {
            Interlocked.Exchange(ref _withHillshade, value ? 1 : 0);
         }
      }

      double shadingAzimut = 315;
      double shadingAltitude = 45;
      double shadingScale = 20;

      /// <summary>
      /// Minimalzoom für Anwendung 
      /// <para>Wird klassenintern NICHT verwendet sondern muss extern ausgewertet werden. Entsprechend muss dort <see cref="IsActiv"/> gesetzt werden.</para>
      /// </summary>
      public readonly int MinimalZoom;

      /// <summary>
      /// Wenn false werden keine Daten bei <see cref="GetHeight(double, double)"/> und <see cref="GetShadingValueArray(double, double, double, double, int, int)"/> geliefert.
      /// </summary>
      public bool IsActiv { get; set; }


      public DemData(string path, int maxtilecache, string cachepath = "", int minzzom = 10) {
         this.path = path;
         demCache = new DemCache(maxtilecache, cachepath);
         MinimalZoom = minzzom;
      }

      public int GetHeight(double lon, double lat) {
         if (!IsActiv)
            return DEM1x1.DEMNOVALUE;

         getLeftBottom4Coord(lon, lat, out int left, out int bottom);
         DEM1x1? dem = demTilePipeline.GetTile(this, left, bottom);
         double h = dem != null ? dem.InterpolatedHeight(lon, lat, DEM1x1.InterpolationType.standard) : 0;
         return h == DEM1x1.NOVALUED ?
                     DEM1x1.DEMNOVALUE :
                     (int)Math.Round(h);
      }

      private const byte V0 = 255;

      //byte getShadingValue(double lon, double lat) {
      //   DEM1x1 dem = getTile(lon, lat);
      //   return dem.ExistsHillShadeData ?
      //                  dem.InterpolatedShadingValue(lon, lat, DEM1x1.InterpolationType.standard) :
      //                  V0;
      //}

      /// <summary>
      /// liefert ein Array mit 1 Byte je Pixel
      /// <para>
      /// Die niedrigen Werte sollten dunkel, die hohen hell dargestellt werden.
      /// </para>
      /// </summary>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="right"></param>
      /// <param name="top"></param>
      /// <param name="pixelshorizontal"></param>
      /// <param name="pixelsvertical"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public byte[]? GetShadingValueArray(double left,
                                         double bottom,
                                         double right,
                                         double top,
                                         int pixelshorizontal,
                                         int pixelsvertical,
                                         CancellationToken? cancellationToken) {
         if (!IsActiv)
            return null;

         double deltalon = (right - left) / pixelshorizontal;
         double deltalat = -(bottom - top) / pixelsvertical;
         byte[] array = new byte[pixelshorizontal * pixelsvertical];  // für die Bitmapdaten (ACHTUNG: Ausrichtung nach unten)

         // alle nötigen 1x1 DEM's ermitteln und dann immer 1 DEM komplett abarbeiten
         int minlon4dem = (int)Math.Floor(left);         // das einfache "(int)left" ist für negative Werte falsch
         int maxlon4dem = (int)Math.Ceiling(right);
         int minlat4dem = (int)Math.Floor(bottom);
         int maxlat4dem = (int)Math.Ceiling(top);

         for (int latdem = minlat4dem; latdem < maxlat4dem; latdem++)
            for (int londem = minlon4dem; londem < maxlon4dem; londem++) {
               if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                  return null;

               getLeftBottom4Coord(londem, latdem, out int _left, out int _bottom);
               DEM1x1? dem = demTilePipeline.GetTile(this, _left, _bottom);

               if (dem != null) {
                  // gemeinsamer Bereich von DEM und Gesamtbereich
                  double l = dem.Left < left ? left : dem.Left;
                  double r = dem.Right < right ? dem.Right : right;
                  double b = dem.Bottom < bottom ? bottom : dem.Bottom;
                  double t = dem.Top < top ? dem.Top : top;

                  if (l < r && b < t) { // es ex. eine nichtleere Teilmenge
                                        // Pixelbereich für die Teilmenge bestimmen
                     int pixlonfrom = Math.Min(pixelshorizontal - 1, (int)Math.Ceiling((l - left) / deltalon));
                     int pixlonto = Math.Min(pixelshorizontal - 1, (int)((r - left) / deltalon));
                     int pixlatfrom = Math.Min(pixelsvertical - 1, (int)Math.Ceiling((b - bottom) / deltalat));
                     int pixlatto = Math.Min(pixelsvertical - 1, (int)((t - bottom) / deltalat));

                     for (int y = pixlatfrom; y <= pixlatto; y++) {
                        for (int x = pixlonfrom; x <= pixlonto; x++) {
                           // ACHTUNG: y ist im Array "abwärts" gerichtet0
                           array[(pixelsvertical - 1 - y) * pixelshorizontal + x] = dem.ExistsHillShadeData ?
                                                            dem.InterpolatedShadingValue(left + x * deltalon,
                                                                                         bottom + y * deltalat,
                                                                                         DEM1x1.InterpolationType.standard) :
                                                            V0;
                        }
                     }
                  }
               }

               // trivial:
               //for (int y = 0; y < height; y++)
               //   for (int x = 0; x < width; x++) {
               //      double lon = left + x * deltalon;
               //      double lat = top - y * deltalat;
               //      if (londem <= lon && lon < londem + 1 &&
               //          latdem <= lat && lat < latdem + 1)
               //         array[y * width + x] = dem.ExistsHillShadeData ?
               //                                          dem.InterpolatedShadingValue(lon, lat, DEM1x1.InterpolationType.standard) :
               //                                          V0;
               //   }
            }
         return array;
      }

      void getLeftBottom4Coord(double lon, double lat, out int left, out int bottom) {
         left = (int)Math.Floor(lon);
         bottom = (int)Math.Floor(lat);
      }

      DEM1x1? getTile(double lon, double lat) {
         getLeftBottom4Coord(lon, lat, out int left, out int bottom);
         DEM1x1? dem = demCache.Get(left, bottom);
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
         }

         if (WithHillshade &&
             (!dem.ExistsHillShadeData ||
               dem.HillShadeAzimut != shadingAzimut ||
               dem.HillShadeAltitude != shadingAltitude ||
               dem.HillShadeScale != shadingScale))
            dem.ComputeHillShadeData(shadingAzimut, shadingAltitude, shadingScale);

         if (dem != null)
            demCache.Add(dem);

         return dem;
      }

      public void SetNewHillshadingData(double shadingazimut = 315, double shadingaltitude = 45, double shadingscale = 1) {
         shadingAzimut = shadingazimut;
         shadingAltitude = shadingaltitude;
         shadingScale = shadingscale;

         if (WithHillshade) {
            DEM1x1[] dem = demCache.GetAll();
            foreach (var item in dem) {
               if (!(item.ExistsHillShadeData ||
                     item.HillShadeAzimut != shadingAzimut ||
                     item.HillShadeAltitude != shadingAltitude ||
                     item.HillShadeScale != shadingScale))
                  item.ComputeHillShadeData(shadingAzimut, shadingAltitude, shadingScale);
            }
         }
      }

      //static bool isCancel(ref long cancel) => Interlocked.Read(ref cancel) != 0;

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
               demCache.Clear(true);
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
