//#define GETDATASEQUENTIEL           // Daten NACHEINANDER ausliefern (ohne Multithreading einfacher zu debuggen)
//#define USEHIGHRESOLUTIONWATCH      // zum Testen

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using GarminCore;
using GarminCore.DskImg;
using GarminCore.Files;
using GarminImageCreator.Garmin;

namespace GarminImageCreator {
   /// <summary>
   /// für alle Tilemaps einer gesamten Garmin-Karte
   /// </summary>
   public class DetailMapManager : IDisposable {

#if USEHIGHRESOLUTIONWATCH   
      static readonly FsoftUtils.HighResolutionWatch hrw4test = new FsoftUtils.HighResolutionWatch();
#endif

      #region Cache-Klassen

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

         int getPos(object obj) {
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
                  if (cache.Count == maxsize) {
                     Debug.WriteLine("Cache is full: remove " + t.ToString());
                     cache.RemoveAt(0);
                  }
                  cache.Add(t);
               }
            }
         }

         /// <summary>
         /// liefert ein Element und aktualisiert die Position im Cache
         /// </summary>
         /// <param name="cmp"></param>
         /// <returns></returns>
         protected T Get(object cmp) {
            T t = default;
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

      /// <summary>
      /// hält die <see cref="SimpleFilesystemReadOnly"/> (jeweils Daten einer Tilemap) in einem Cache mit Größenbegrenzung (threadsicher)
      /// </summary>
      protected class SimpleFilesystemReadOnlyCache : ThreadsafeCache<SimpleFilesystemReadOnlyCache.CachData> {

         public class CachData {

            public uint Mapnumber;

            public SimpleFilesystemReadOnly Sf;

            public CachData(uint mapnumber, SimpleFilesystemReadOnly sf) {
               Mapnumber = mapnumber;
               Sf = sf;
            }

            public override string ToString() {
               return string.Format("Mapnumber {0}, {1}", Mapnumber, Sf);
            }
         }

         public SimpleFilesystemReadOnlyCache(int maxsize) : base(maxsize) { }

         protected override bool found(CachData t, object obj) {
            if (obj is CachData)
               return t.Mapnumber == (obj as CachData).Mapnumber;
            else if (obj.GetType().Equals(typeof(uint)))
               return t.Mapnumber == (uint)obj;
            else
               throw new Exception("false type for compare: SimpleFilesystemReadOnlyCache");
         }

         public void Add(SimpleFilesystemReadOnly sf, uint mapnumber) {
            Add(new CachData(mapnumber, sf));
         }

         public SimpleFilesystemReadOnly Get(uint mapnumber) {
            CachData cd = base.Get(new CachData(mapnumber, null));
            return cd?.Sf;
         }

         public new void Clear() {
            for (int i = 0; i < cache.Count; i++)
               cache[i].Sf.Dispose();
            base.Clear();
         }

      }

      /// <summary>
      /// hält die <see cref="DetailMapExt"/> (jeweils Daten eines Subdivs) in einem Cache mit Größenbegrenzung (threadsicher)
      /// </summary>
      protected class DetailMapExtCache : ThreadsafeCache<DetailMapExt> {

         public DetailMapExtCache(int maxsize) : base(maxsize) { }

         protected override bool found(DetailMapExt t, object obj) {
            if (obj is DetailMapExt)
               return t.ID == (obj as DetailMapExt).ID;
            else if (obj is DetailMapIdentifier)
               return t.ID == obj as DetailMapIdentifier;
            else
               throw new Exception("false type for compare: DetailmapdataCache");
         }

         public DetailMapExt Get(DetailMapIdentifier id) {
            return base.Get(id);
         }

         public new void Clear() {
            for (int i = 0; i < cache.Count; i++)
               cache[i].Dispose();
            base.Clear();
         }

      }

      /// <summary>
      /// hält die <see cref="GarminTilemapReader"/> (jeweils für eine Tilemap) in einem Cache mit Größenbegrenzung (threadsicher)
      /// </summary>
      protected class GarminTilemapReaderCache : ThreadsafeCache<GarminTilemapReader> {

         public GarminTilemapReaderCache(int maxsize) : base(maxsize) { }

         protected override bool found(GarminTilemapReader t, object obj) {
            if (obj is GarminTilemapReader)
               return t.MapNumber == (obj as GarminTilemapReader).MapNumber;
            else if (obj.GetType().Equals(typeof(uint)))
               return t.MapNumber == (uint)obj;
            else
               throw new Exception("false type for compare: GarminTilemapReaderCache");
         }

         public GarminTilemapReader Get(uint mapnumber) {
            return base.Get(mapnumber);
         }

         public new void Clear() {
            for (int i = 0; i < cache.Count; i++)
               cache[i].Dispose();
            base.Clear();
         }

      }

      protected class SQLiteCache {
         /// <summary>
         /// Extension der SQLite-Datenbanken
         /// </summary>
         const string DBEXTENSION = ".cache";

         /// <summary>
         /// Pfad für den Cache
         /// </summary>
         public string CachePath { get; protected set; }

         /// <summary>
         /// gültige Level (oder alle)
         /// </summary>
         public int[] ValidLevels { get; protected set; }


         /// <summary>
         /// Pfad für den Cache
         /// </summary>
         /// <param name="path"></param>
         /// <param name="validlevels">gültige Level (oder alle)</param>
         public SQLiteCache(string path, IList<int> validlevels = null) {
            CachePath = path;
            if (validlevels == null)
               ValidLevels = new int[0];
            else {
               ValidLevels = new int[validlevels.Count];
               validlevels.CopyTo(ValidLevels, 0);
            }
         }

         Sqlite4Garmin.GarminTileDatabase openSQLiteDB(uint tilemapnumber) {
            if (!Directory.Exists(CachePath))
               Directory.CreateDirectory(CachePath);
            return new Sqlite4Garmin.GarminTileDatabase(GetDBPath(tilemapnumber));
         }

         static Sqlite4Garmin.GeoPoint[] convertPoints(DetailMap.GeoPoly poly) {
            Sqlite4Garmin.GeoPoint[] pts = new Sqlite4Garmin.GeoPoint[poly.Points.Length];
            for (int i = 0; i < poly.Points.Length; i++)
               pts[i] = new Sqlite4Garmin.GeoPoint(poly.Points[i].X, poly.Points[i].Y);
            return pts;
         }

         static Sqlite4Garmin.GarminPolypoint convertPoly(DetailMap.GeoPoly poly) {
            return new Sqlite4Garmin.GarminPolypoint(
                           poly.Type,
                           poly.Text,
                           (float)poly.Bound.TopDegree,
                           (float)poly.Bound.RightDegree,
                           (float)poly.Bound.BottomDegree,
                           (float)poly.Bound.LeftDegree,
                           convertPoints(poly)
                        );
         }

         static Sqlite4Garmin.GarminPoint convertPoint(DetailMap.GeoPoint pt) {
            return new Sqlite4Garmin.GarminPoint(
                           pt.Type,
                           (float)pt.Point.X,
                           (float)pt.Point.Y,
                           pt.Text
                        );
         }

         static PointF[] convertPoints(Sqlite4Garmin.GeoPoint[] poly) {
            PointF[] pts = new PointF[poly.Length];
            for (int i = 0; i < poly.Length; i++)
               pts[i] = new PointF(poly[i].Lon, poly[i].Lat);
            return pts;
         }

         static DetailMap.GeoPoly convertPoly(Sqlite4Garmin.GarminPolypoint poly) {
            return new DetailMap.GeoPoly(poly.FullType,
                           poly.Label,
                           convertPoints(poly.Pointlist),
                           poly.West,
                           poly.East,
                           poly.South,
                           poly.North,
                           false
                        );
         }

         static DetailMap.GeoPoint convertPoint(Sqlite4Garmin.GarminPoint pt) {
            return new DetailMap.GeoPoint(
                           pt.FullType,
                           pt.Label,
                           pt.Point.Lon,
                           pt.Point.Lat
                        );
         }

         /// <summary>
         /// falls die <see cref="Detailmapdata"/> einen gültigen Level haben werden sie im Cache gespeichert
         /// </summary>
         /// <param name="dmlst"></param>
         public void AddDetailmapdata(IList<DetailMapExt> dmlst) {
            SortedSet<uint> tilemapnumbers = new SortedSet<uint>();
            foreach (DetailMapExt dmd in dmlst)
               tilemapnumbers.Add(dmd.ID.TilemapNumber);

            foreach (uint tilemapnumber in tilemapnumbers) {
               Sqlite4Garmin.GarminTileDatabase db = openSQLiteDB(tilemapnumber);

               db.Optimize4Write();
               db.StartTransaction();

               int[] sdidx = db.GetAllSubdivIdx(); // alle schon enthaltenen Detailmaps/Subdivs ausschließen

               foreach (DetailMapExt dmd in dmlst) {
                  if (dmd.ID.TilemapNumber == tilemapnumber &&    // Daten für diese Garmin-Tilemap
                      !sdidx.Contains(dmd.ID.SubdivIndex) &&      // Subdiv/Detailmap ist noch nicht enthalten
                      (ValidLevels.Length == 0 || ValidLevels.Contains(dmd.Level))) {          // Level der Subdiv/Detailmap ist erlaubt
                     db.RegisterSubdiv(dmd.ID.SubdivIndex,
                                       dmd.Level,
                                       (float)dmd.Bound.TopDegree,
                                       (float)dmd.Bound.RightDegree,
                                       (float)dmd.Bound.BottomDegree,
                                       (float)dmd.Bound.TopDegree);

                     foreach (DetailMap.GeoPoly item in dmd.Areas)
                        db.RegisterArea(convertPoly(item), dmd.ID.SubdivIndex);

                     foreach (DetailMap.GeoPoly item in dmd.Lines)
                        db.RegisterLine(convertPoly(item), dmd.ID.SubdivIndex);


                     foreach (DetailMap.GeoPoint item in dmd.Points)
                        db.RegisterPoint(convertPoint(item), dmd.ID.SubdivIndex);
                  }
               }
               db.EndTransaction();
               db.Dispose();
            }
         }

         /// <summary>
         /// erzeugt eine <see cref="Detailmapdata"/>-Liste der gleichen Größe wie die Indexliste und die Daten, falls im Cache vorhanden
         /// </summary>
         /// <param name="tilemapnumber"></param>
         /// <param name="subdividx"></param>
         /// <returns></returns>
         public DetailMapExt[] GetDetailmapdata(uint tilemapnumber, IList<int> subdividx) {
            DetailMapExt[] dmlst = new DetailMapExt[subdividx.Count];

            Sqlite4Garmin.GarminTileDatabase db = openSQLiteDB(tilemapnumber);
            int[] sdidx = db.GetAllSubdivs(out int[] level,
                                           out float[] north,
                                           out float[] east,
                                           out float[] south,
                                           out float[] west); // alle schon enthaltenen Detailmaps/Subdivs

            try {
               for (int i = 0; i < subdividx.Count; i++) {

                  int pos = -1;
                  for (int j = 0; j < sdidx.Length; j++)
                     if (sdidx[j] == subdividx[i]) {
                        pos = j;
                        break;
                     }

                  if (pos >= 0) { //sdidx.Contains(subdividx[i])) {

                     List<DetailMap.GeoPoly> areas = new List<DetailMap.GeoPoly>();
                     List<DetailMap.GeoPoly> lines = new List<DetailMap.GeoPoly>();
                     List<DetailMap.GeoPoint> points = new List<DetailMap.GeoPoint>();

                     foreach (Sqlite4Garmin.GarminPolypoint pp in db.GetAllAreas4Subdiv(subdividx[i]))
                        areas.Add(convertPoly(pp));

                     foreach (Sqlite4Garmin.GarminPolypoint pp in db.GetAllLines4Subdiv(subdividx[i]))
                        lines.Add(convertPoly(pp));

                     foreach (Sqlite4Garmin.GarminPoint pt in db.GetAllPoints4Subdiv(subdividx[i]))
                        points.Add(convertPoint(pt));

                     DetailMapExt dmd = new DetailMapExt(tilemapnumber,
                                                         subdividx[i],
                                                         level[pos],
                                                         new Bound((double)west[pos],
                                                                   (double)east[pos],
                                                                   (double)south[pos],
                                                                   (double)north[pos]));

                     dmd.SetAreas(areas);
                     dmd.SetLines(lines);
                     dmd.SetPoints(points);
                     dmlst[i] = dmd;
                  }
               }

               db.Dispose();
            } catch (Exception ex) {
               throw new Exception("error reading sqlite subdiv data: " + ex.Message);
            }

            return dmlst;
         }

         /// <summary>
         /// liefert den Namen der DB für die Tilemap
         /// </summary>
         /// <param name="tilemapnumber"></param>
         /// <returns></returns>
         public string GetDBPath(uint tilemapnumber) {
            return Path.Combine(CachePath, tilemapnumber.ToString()) + DBEXTENSION;
         }

         /// <summary>
         /// löscht die DB für die Tilemap
         /// </summary>
         /// <param name="tilemapnumber"></param>
         public void RemoveDB(uint tilemapnumber) {
            string dbpath = GetDBPath(tilemapnumber);
            if (File.Exists(dbpath))
               File.Delete(dbpath);
         }

         public override string ToString() {
            return "CachePath=" + CachePath;
         }

      }

      #endregion

      #region Hilfsklassen

      /// <summary>
      /// Infos für eine Tilemap
      /// </summary>
      protected class TileInfo {

         /*    
         Eine DetailMap wird eindeutig bezeichnet durch 
            den Namen der TRE/RGN-Datei
            den (1-basierten) Index der SubdivInfo(Basic) in der TRE-Datei

            Name TRE-Datei
            Bound
            |
            Scale 0     Scale 1     ...
            |
            Subdiv-Idx
            Bound


            Ein Subdiv für Scale n verweist auf die ("seine") untergeordneten Subdiv's n+1. (ev. für die Suche nützlich)

          */

         /// <summary>
         /// Infos für eine Subdiv
         /// </summary>
         public class Subdivinfo {
            public StdFile_TRE.SubdivInfoBasic Sdi { get; protected set; }
            public Bound Bound { get; protected set; }
            public int Index { get; protected set; }
            public int Level { get; protected set; }


            public Subdivinfo(StdFile_TRE.SubdivInfoBasic sdi, int idx, int level, int coordbits) {
               Sdi = sdi;
               Index = idx;
               Bound = sdi.GetBound(coordbits);
               Level = level;
            }

            /// <summary>
            /// liefert ein Array der Indexe aller untergeordneneten Subdivs
            /// </summary>
            /// <returns></returns>
            public int[] ChildSubdivIdx() {
               if (Sdi is StdFile_TRE.SubdivInfoBasic)
                  return new int[0];
               else {
                  int[] ret = new int[(Sdi as StdFile_TRE.SubdivInfo).ChildSubdivInfos];
                  if (ret.Length > 0) {
                     ret[0] = (Sdi as StdFile_TRE.SubdivInfo).FirstChildSubdivIdx1 - 1;
                     for (int i = 1; i < ret.Length; i++)
                        ret[i] = ret[i - 1] + 1;
                  }
                  return ret;
               }
            }

            public override string ToString() {
               return string.Format("Level={0}, Index={1}, Bound={2}", Level, Index, Bound);
            }

         }

         /// <summary>
         /// Beschreibung des Tilemaps
         /// </summary>
         public string Description { get; protected set; }

         public Bound Bound { get; protected set; }

         /// <summary>
         /// eindeutige Nummer des Tilemaps
         /// </summary>
         public uint TilemapNumber { get; protected set; }

         /// <summary>
         /// zugehörige Dateien
         /// </summary>
         public string[] Filenames { get; protected set; }

         /// <summary>
         /// Bitanzahl je Level
         /// </summary>
         public Dictionary<int, int> Coordbits4Level = new Dictionary<int, int>();

         /// <summary>
         /// Liste der <see cref="Subdivinfo"/> je Level
         /// </summary>
         public Dictionary<int, Subdivinfo[]> Subdivinfos4Level = new Dictionary<int, Subdivinfo[]>();

         /// <summary>
         /// nur gesetzt, wenn nicht der <see cref="TilemapNumber"/> entsprechend
         /// </summary>
         public string ImgFilename { get; protected set; }


         long infosPresent = 0;

         /// <summary>
         /// Infos für <see cref="Coordbits4Level"/> und <see cref="Subdivinfos4Level"/> schon eingelesen (threadsicher)?
         /// </summary>
         public bool InfosPresent {
            get {
               return Interlocked.Read(ref infosPresent) != 0;
            }
            protected set {
               Interlocked.Exchange(ref infosPresent, value ? 1 : 0);
            }
         }

         readonly object lock_read_tre = new object();


         /// <summary>
         /// für "normale" Tilemaps deren Name sich aus der Tilemapnummer ergibt
         /// </summary>
         /// <param name="description"></param>
         /// <param name="tilemapnumber"></param>
         /// <param name="west"></param>
         /// <param name="east"></param>
         /// <param name="south"></param>
         /// <param name="north"></param>
         /// <param name="filenames"></param>
         public TileInfo(string description,
                         uint tilemapnumber,
                         double west,
                         double east,
                         double south,
                         double north,
                         IList<string> filenames) {
            Bound = new Bound(west, east, south, north);
            Description = description;
            TilemapNumber = tilemapnumber;
            Coordbits4Level = new Dictionary<int, int>();
            Subdivinfos4Level = new Dictionary<int, Subdivinfo[]>();
            Filenames = new string[filenames.Count];
            filenames.CopyTo(Filenames, 0);
            ImgFilename = "";
         }

         /// <summary>
         /// für Overviewmap nötig, da deren IMG einen von der Tilemapnummer unabhängigen Namen haben kann
         /// </summary>
         /// <param name="description"></param>
         /// <param name="tilemapnumber"></param>
         /// <param name="west"></param>
         /// <param name="east"></param>
         /// <param name="south"></param>
         /// <param name="north"></param>
         /// <param name="imgfilename"></param>
         public TileInfo(string description,
                         uint tilemapnumber,
                         double west,
                         double east,
                         double south,
                         double north,
                         string imgfilename) :
            this(description, tilemapnumber, west, east, south, north, new string[] { }) {
            ImgFilename = imgfilename;
         }

         /// <summary>
         /// setzt die <see cref="Coordbits4Level"/> und <see cref="Subdivinfos4Level"/> entsprechend der TRE neu
         /// </summary>
         /// <param name="tre"></param>
         void setTreInfos(StdFile_TRE tre) {
            Coordbits4Level.Clear();
            Subdivinfos4Level.Clear();
            for (int level = 0; level < tre.SymbolicScaleDenominatorAndBitsLevel.Count; level++) {
               int bits = tre.SymbolicScaleDenominatorAndBitsLevel.Bits(level);
               int count = tre.SymbolicScaleDenominatorAndBitsLevel.Subdivs(level);
               int firstidx = tre.SymbolicScaleDenominatorAndBitsLevel.FirstSubdivChildIdx(level) - 1;

               Coordbits4Level.Add(level, bits);
               Subdivinfos4Level.Add(level, new Subdivinfo[count]);
               for (int i = 0; i < count; i++) {
                  int idx = firstidx + i;
                  Subdivinfos4Level[level][i] = new Subdivinfo(tre.SubdivInfoList[idx], idx, level, bits);
               }
            }
         }

         /// <summary>
         /// liest threadsicher und nur 1x die TRE-Infos ein
         /// </summary>
         /// <param name="dmm"></param>
         /// <param name="mapdirectory"></param>
         public void SetTreInfos(DetailMapManager dmm, string mapdirectory) {
            if (!InfosPresent) {
               lock (lock_read_tre) {
                  if (!InfosPresent) {
                     StdFile_TRE tre = new StdFile_TRE();
                     string basename = TilemapNumber.ToString();
                     try {
                        SimpleFilesystemReadOnly fs = ImgFilename.Length == 0 ?
                                                         dmm.getSimpleFilesystemReadOnly(TilemapNumber, mapdirectory) :
                                                         dmm.getSimpleFilesystemReadOnly(TilemapNumber, mapdirectory, ImgFilename);
                        if (fs == null)
                           throw new Exception("error reading IMG " + (ImgFilename == "" ? (mapdirectory + "/" + basename + ".img") : ImgFilename));
                        BinaryReaderWriter brtre = fs.GetBinaryReaderWriter4File((basename + ".tre").ToUpper());
                        if (brtre != null)
                           tre.Read(brtre);
                        setTreInfos(tre);
                     } catch (Exception ex) {
                        throw new Exception("error on reading TRE for tilemap " + basename + ": " + ex.Message);
                     }
                     tre.Dispose();
                     InfosPresent = true;
                  }
               }
            }
         }

         /// <summary>
         /// liefert ein Array aller Indexe der betroffenen Subdivs
         /// </summary>
         /// <param name="bound"></param>
         /// <param name="level"></param>
         /// <returns></returns>
         public int[] GetSubdivIndex4BoundAndLevel(Bound bound, int level) {
            if (!InfosPresent)
               throw new Exception("error on GetSubdivIndex4BoundAndLevel(): no InfosPresent");
            if (Subdivinfos4Level.TryGetValue(level, out Subdivinfo[] sdlst)) {
               List<Subdivinfo> tmp = new List<Subdivinfo>();
               for (int i = 0; i < sdlst.Length; i++)
                  if (sdlst[i].Bound.IsOverlapped(bound))
                     tmp.Add(sdlst[i]);

               int[] ret = new int[tmp.Count];
               for (int i = 0; i < tmp.Count; i++)
                  ret[i] = tmp[i].Index;
               return ret;
            }
            return new int[0];
         }

         public override string ToString() {
            return string.Format("TilemapNumber {0}, Bound {1}, InfosPresent {2}", TilemapNumber, Bound, InfosPresent);
         }
      }

      /// <summary>
      /// zum Lesen der Daten aus einer einzigen Garmin-Tilemap
      /// </summary>
      protected class GarminTilemapReader : IDisposable {

         /// <summary>
         /// Nummer des Tiles
         /// </summary>
         public readonly uint MapNumber;

         /// <summary>
         /// Verzeichnis der Tile-(IMG-)-Dateien
         /// </summary>
         public readonly string MapDirectory;

         /// <summary>
         /// falls gesetzt, handelt es sich um eine Overviewmap
         /// </summary>
         public readonly string OverviewMapName;


         /// <summary>
         /// der zugehörige <see cref="DetailMapManager"/>
         /// </summary>
         readonly DetailMapManager dmm;

         SimpleFilesystemReadOnly fs;  // hält nach dem Öffnen die gesamte IMG-Datei im Hauptspeicher
         StdFile_TRE tre;
         StdFile_LBL lbl;
         StdFile_RGN rgn;
         StdFile_NET net;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="dmm"></param>
         /// <param name="mapnumber">Nummer der Tilemap</param>
         /// <param name="mapdirectory">Verzeichnis mit den Tilemaps</param>
         public GarminTilemapReader(DetailMapManager dmm, uint mapnumber, string mapdirectory) {
            this.dmm = dmm;
            MapNumber = mapnumber;
            MapDirectory = mapdirectory;
            fs = null;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="dmm"></param>
         /// <param name="ovname">Name der Overviewmap (muss NICHT identisch zur Nummer sein)</param>
         /// <param name="mapnumber">Nummer der Tilemap</param>
         /// <param name="mapdirectory">Verzeichnis mit den Tilemaps</param>
         public GarminTilemapReader(DetailMapManager dmm, string ovname, uint mapnumber, string mapdirectory) :
            this(dmm, mapnumber, mapdirectory) {
            OverviewMapName = ovname;
         }


         readonly object open_locker = new object();

         void open() {
            if (fs == null)         // SimpleFilesystem muss noch erzeugt werden
               lock (open_locker) {
                  if (fs == null) { // SimpleFilesystem wurd inzwischen schon von einem anderen Thread erzeugt

                     string basename = MapNumber.ToString();
                     SimpleFilesystemReadOnly fstmp = string.IsNullOrEmpty(OverviewMapName) ?
                                                   dmm.getSimpleFilesystemReadOnly(MapNumber, MapDirectory) :
                                                   dmm.getSimpleFilesystemReadOnly(MapNumber, MapDirectory, Path.Combine(MapDirectory, OverviewMapName));

                     tre = new StdFile_TRE();
                     lbl = new StdFile_LBL();
                     rgn = new StdFile_RGN(tre);
                     net = new StdFile_NET();

                     BinaryReaderWriter br2;

                     br2 = fstmp.GetBinaryReaderWriter4File(basename + ".TRE");
                     if (br2 != null) {
                        tre.Read(br2);
                        br2.Dispose();
                     }

                     br2 = fstmp.GetBinaryReaderWriter4File(basename + ".LBL");
                     if (br2 != null) {
                        lbl.Read(br2);       // <-- Zeit !!!
                        br2.Dispose();
                     }

                     br2 = fstmp.GetBinaryReaderWriter4File(basename + ".NET");
                     if (br2 != null) {
                        net.Lbl = lbl;
                        net.Read(br2);       // <-- Zeit !!!
                        br2.Dispose();
                     }

                     Interlocked.Exchange(ref fs, fstmp);

                  }
               }
         }

         readonly object readonly_locker = new object();



         /// <summary>
         /// liest die Daten der <see cref="DetailMapExt"/> aus den Garmin-Dateien entsprechend der Index-Liste
         /// </summary>
         /// <param name="subdividx"></param>
         /// <returns></returns>
         DetailMapExt[] createDetailmapsFromGarminFiles(IList<int> subdividx) {
            open();

            lock (readonly_locker) {
               // liest die Daten der Subdivs ein ...
               rgn.ReadOnlySpecialSubdivs(fs.GetBinaryReaderWriter4File(MapNumber.ToString() + ".RGN"),
                                          subdividx,
                                          tre);
            }
            // ... und erzeugt daraus die Detailmaps
            DetailMapExt[] detailMaps = new DetailMapExt[subdividx.Count];
            for (int i = 0; i < subdividx.Count; i++)
               detailMaps[i] = new DetailMapExt(MapNumber,
                                                subdividx[i],
                                                tre,
                                                lbl,
                                                rgn,
                                                net);

            return detailMaps;
         }

         /// <summary>
         /// liefert die <see cref="DetailMapExt"/> zu den Subdivs mit den entsprechenden Indexnummern 
         /// aus dem Hauptspeicher-Cache, dem SQLite-Cache oder den Garmin-Dateien
         /// und fügt sie gegebenenfalls in den Hauptspeicher-Cache und/oder SQLite-Cache ein
         /// </summary>
         /// <param name="subdividx"></param>
         /// <param name="uselocalcache">SQLite-Cache verwenden?</param>
         /// <returns></returns>
         public DetailMapExt[] GetDetailmaps(int[] subdividx, bool uselocalcache) {
            DetailMapExt[] dmdlst = new DetailMapExt[subdividx.Length];
            if (subdividx.Length > 0) {
               string basename = MapNumber.ToString();
               try {
#if DEBUG
                  string debugtxt = "GetDetailmaps " + basename + ", " + subdividx.Length.ToString() + " subdivs: ";
                  Debug.Write("GetDetailmaps " + basename + ", " + subdividx.Length.ToString() + " subdivs: ");
                  foreach (var item in subdividx)
                     debugtxt += " " + item.ToString();
                  debugtxt += " ->";
#endif
                  List<DetailMapExt> result = new List<DetailMapExt>();

                  List<int> sdidx = new List<int>();

                  // ev. im Hauptspeicher-Cache vorhandene Daten holen
                  for (int i = 0; i < subdividx.Length; i++) {
                     DetailMapExt dmd = dmm.dmCache.Get(new DetailMapIdentifier(MapNumber, subdividx[i]));
                     if (dmd == null)
                        sdidx.Add(subdividx[i]);   // noch fehlende einsammeln
                     else
                        result.Add(dmd);
                  }
                  int fromcache = result.Count();
#if DEBUG
                  debugtxt += " from cache: " + fromcache.ToString();
#endif

                  if (uselocalcache) {
                     // noch fehlende ev. aus dem SQLite-Cache holen
                     DetailMapExt[] dmsqlitecache = dmm.sqliteCache != null ?
                                                         dmm.sqliteCache.GetDetailmapdata(MapNumber, sdidx) :
                                                         new DetailMapExt[subdividx.Length];                  // alles mit null init.

                     for (int i = dmsqlitecache.Length - 1; i >= 0; i--) {
                        if (dmsqlitecache[i] != null) {
                           DetailMapExt dmd = dmsqlitecache[i];
                           dmm.dmCache.Add(dmd);
                           result.Add(dmd);
                           sdidx.RemoveAt(i);   // gefundene entfernen
                        }
                     }
                  }
#if DEBUG
                  int fromcache2 = result.Count();
                  debugtxt += ", from SQLite-cache: " + (fromcache2 - fromcache).ToString();
#endif

                  // noch fehlende aus den Garmin-Dateien lesen ...
                  List<DetailMapExt> new4sqlite = new List<DetailMapExt>();
                  if (sdidx.Count > 0) {
                     DetailMapExt[] dmgarmin = createDetailmapsFromGarminFiles(sdidx);
                     for (int i = 0; i < dmgarmin.Length; i++)
                        if (dmgarmin[i] != null)
                           dmm.dmCache.Add(dmgarmin[i]);
                     result.AddRange(dmgarmin);
                     new4sqlite.AddRange(dmgarmin);
                  }
#if DEBUG
                  debugtxt += ", Garmin-Files: " + (result.Count - fromcache2).ToString();
#endif

                  // ... und ev. im SQLite-Cache speichern
                  if (uselocalcache &&
                      new4sqlite.Count > 0 &&
                      dmm.sqliteCache != null) {
                     dmm.sqliteCache.AddDetailmapdata(new4sqlite);
#if DEBUG
                     debugtxt += ", to SQLite-cache: " + new4sqlite.Count.ToString();
                     Debug.WriteLine(debugtxt);
#endif
                  }

                  result.CopyTo(dmdlst);

               } catch (Exception ex) {
                  throw new Exception("error on reading " + MapNumber.ToString() + ".IMG " + basename + ": " + ex.Message);
               }
            }
            return dmdlst;
         }

         public override string ToString() {
            return string.Format("MapNumber {0}, MapDirectory {1}", MapNumber, MapDirectory);
         }

         ~GarminTilemapReader() {
            Dispose(false);
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
                  if (net != null)
                     net.Dispose();
                  if (rgn != null)
                     rgn.Dispose();
                  if (lbl != null)
                     lbl.Dispose();
                  if (tre != null)
                     tre.Dispose();
                  if (fs != null)
                     fs.Dispose();
               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion

      }

      #endregion


      /// <summary>
      /// Kartenverzeichnis
      /// </summary>
      protected readonly string mapDirectory;

      /// <summary>
      /// <see cref="TileInfo"/> je Mapnumber
      /// <para>eindeutige Kachelnummer (z.B. Name des Verzeichnisses der TRE, LBL usw. Dateien oder Name der IMG-Datei)</para>
      /// <para>0x234F1D9 kann für das Verzeichnis 37024217 stehen</para>
      /// </summary>
      protected readonly Dictionary<uint, TileInfo> TileInfo4Tilemapnumber = new Dictionary<uint, TileInfo>();

      /// <summary>
      /// Cache für die <see cref="SimpleFilesystemReadOnly"/> (wird sowohl für die <see cref="TileInfo"/> als auch die <see cref="GarminTilemapReader"/> verwendet)
      /// </summary>
      protected readonly SimpleFilesystemReadOnlyCache sfsCache;

      /// <summary>
      /// Cache für die <see cref="Detailmapdata"/> im Hauptspeicher
      /// </summary>
      protected readonly DetailMapExtCache dmCache;

      /// <summary>
      /// Cache für die <see cref="GarminTilemapReader"/> im Hauptspeicher (für jede Tilemap ist ein Reader nötig)
      /// </summary>
      protected readonly GarminTilemapReaderCache garminTilemapReaderCache;

      /// <summary>
      /// Cache im Dateisystem
      /// </summary>
      protected SQLiteCache sqliteCache;

      /// <summary>
      /// max. Anzahl Subdivs je Bild (unbegrenzt wenn kleiner 1)
      /// </summary>
      protected int maxSubdivs;

      /// <summary>
      /// Infos zur Overviewmap (oder null)
      /// </summary>
      protected TileInfo overviewTileInfo;



      /// <summary>
      /// verwaltet die Daten für eine Garmin-Karte
      /// </summary>
      /// <param name="tdbfilename">Pfad der Garmin-TDB-Datei</param>
      /// <param name="cachepath">Pfad für einen Datencache</param>
      /// <param name="validlevels4cachepath">gültige Level für den Cache im Dateisystem</param>
      /// <param name="subdivcachesize">Größe für den Cache der Subdivs im Hauptspeicher</param>
      /// <param name="tilemapreadercachesize">Größe für den Cache der Tilemap-Reader im Hauptspeicher</param>
      /// <param name="maxsubdiv">max. Anzahl Subdivs je Bild (unbegrenzt wenn kleiner 1)</param>
      public DetailMapManager(string tdbfilename,
                              string cachepath,
                              IList<int> validlevels4cachepath = null,
                              int subdivcachesize = 1000,
                              int tilemapreadercachesize = 50,
                              int maxsubdiv = -1) {
         dmCache = new DetailMapExtCache(subdivcachesize);
         if (!string.IsNullOrEmpty(cachepath) &&
             validlevels4cachepath.Count > 0)
            sqliteCache = new SQLiteCache(cachepath, validlevels4cachepath);

         File_TDB tdb;
         try {
            BinaryReaderWriter br = new BinaryReaderWriter(tdbfilename, true);
            tdb = new File_TDB();
            tdb.Read(br);
         } catch (Exception ex) {
            throw new Exception("error reading TDB '" + tdbfilename + "': " + ex.Message);
         }
         mapDirectory = Path.GetDirectoryName(tdbfilename);

         // alle zugehörigen Tilemaps registrieren
         TileInfo4Tilemapnumber = new Dictionary<uint, TileInfo>();
         foreach (var item in tdb.Tilemap) {
            TileInfo4Tilemapnumber.Add(item.Mapnumber,
                                       new TileInfo(item.Description, item.Mapnumber, item.West, item.East, item.South, item.North, item.Name));
         }

         // TileInfo für eine ev. vorhandene Overviewmap erzeugen
         overviewTileInfo = null;
         if (tdb.Overviewmap != null &&
             tdb.Overviewmap.Mapnumber > 0) {
            string overviewMapFilename = "";
            foreach (string imgfile in Directory.GetFiles(mapDirectory, "*.img", SearchOption.TopDirectoryOnly)) {
               string basename = Path.GetFileNameWithoutExtension(imgfile);

               int number = basenameAsNumber(basename);
               if (number > 0) {
                  if (number == tdb.Overviewmap.Mapnumber) {
                     overviewMapFilename = imgfile;
                     break;
                  }
               } else {
                  SimpleFilesystemReadOnly sro = new SimpleFilesystemReadOnly(imgfile);
                  for (int i = 0; i < sro.FileCount; i++) {
                     string filename = sro.Filename(i);
                     if (Path.GetExtension(filename).ToUpper() == ".TRE") {
                        number = basenameAsNumber(Path.GetFileNameWithoutExtension(filename));
                        if (number == tdb.Overviewmap.Mapnumber) {
                           overviewMapFilename = imgfile;
                           break;
                        }
                     }
                  }
               }
               if (!string.IsNullOrEmpty(overviewMapFilename)) {
                  overviewTileInfo = new TileInfo(tdb.Overviewmap.Description,
                                                  tdb.Overviewmap.Mapnumber,
                                                  tdb.Overviewmap.West,
                                                  tdb.Overviewmap.East,
                                                  tdb.Overviewmap.South,
                                                  tdb.Overviewmap.North,
                                                  overviewMapFilename);
                  //TileInfo4Tilemapnumber.Add(overviewTileInfo.TilemapNumber, overviewTileInfo);
                  break;
               }
            }
         }

         sfsCache = new SimpleFilesystemReadOnlyCache(tilemapreadercachesize);
         garminTilemapReaderCache = new GarminTilemapReaderCache(tilemapreadercachesize);

         maxSubdivs = maxsubdiv;
      }

      /// <summary>
      /// liefert die Nummer zum Namen oder -1
      /// </summary>
      /// <param name="basename"></param>
      /// <returns></returns>
      int basenameAsNumber(string basename) {
         for (int i = 0; i < basename.Length; i++)
            if (!char.IsDigit(basename[i]))
               return -1;
         return Convert.ToInt32(basename);
      }

      /// <summary>
      /// liefert das eingelesene <see cref="SimpleFilesystemReadOnly"/> aus dem <see cref="SimpleFilesystemReadOnlyCache"/> oder liest es neu ein und speichert es im Cache
      /// </summary>
      /// <param name="mapnumber"></param>
      /// <param name="imgfile">nur nötig, wenn es kein IMG für eine Tilemap ist</param>
      /// <returns></returns>
      protected SimpleFilesystemReadOnly getSimpleFilesystemReadOnly(uint mapnumber, string mapdirectory, string imgfile = null) {
         SimpleFilesystemReadOnly fs = null;
         if (sfsCache != null) {
            fs = sfsCache.Get(mapnumber);
            if (fs != null)
               Debug.WriteLine("GetSimpleFilesystemReadOnly(" + mapnumber.ToString() + ") from Cache");
         }
         if (fs == null) {
            if (string.IsNullOrEmpty(imgfile)) {
               fs = new SimpleFilesystemReadOnly(Path.Combine(mapdirectory, mapnumber.ToString() + ".img"));
               Debug.WriteLine("GetSimpleFilesystemReadOnly(" + mapnumber.ToString() + ") new read");
            } else
               fs = new SimpleFilesystemReadOnly(imgfile);
            if (sfsCache != null &&
                fs != null)
               sfsCache.Add(fs, mapnumber);
         }
         return fs;
      }

      /// <summary>
      /// liefert den <see cref="GarminTilemapReader"/> aus dem Cache order erzeugt ihn neu und speichert ihn im Cache
      /// </summary>
      /// <param name="mapnumber"></param>
      /// <returns></returns>
      GarminTilemapReader getGarminTilemapReader(uint mapnumber) {
         GarminTilemapReader garminTilemapReader = garminTilemapReaderCache.Get(mapnumber);
         if (garminTilemapReader == null) {
            garminTilemapReader = overviewTileInfo != null &&
                                  overviewTileInfo.TilemapNumber == mapnumber ?
                                          new GarminTilemapReader(this,
                                                                  overviewTileInfo.ImgFilename,
                                                                  mapnumber,
                                                                  mapDirectory) :
                                          new GarminTilemapReader(this,
                                                                  mapnumber,
                                                                  mapDirectory);
            garminTilemapReaderCache.Add(garminTilemapReader);
         }
         return garminTilemapReader;
      }

      /// <summary>
      /// Sollte bei für diese Bitanzahl die Overviewmap verwendet werden?
      /// </summary>
      /// <param name="bits"></param>
      /// <returns></returns>
      bool useOverviewmap(int bits) {
         if (overviewTileInfo != null) {
            if (!overviewTileInfo.InfosPresent)
               overviewTileInfo.SetTreInfos(this, mapDirectory);

            if (overviewTileInfo.Coordbits4Level.Values.Max() >= bits)
               return true;

            // Wenn für die normalen Tiles Level 0 entsteht soll trotzdem der höchste Overview-Level verwendet werden!!!
            if (TileInfo4Tilemapnumber.Count > 0) {
               foreach (TileInfo ti in TileInfo4Tilemapnumber.Values) {
                  if (!ti.InfosPresent)
                     ti.SetTreInfos(this, mapDirectory);
                  if (getLevel4Bits(bits, ti) == 0)
                     return true;
                  break;      // nur das 1. Tile testen
               }
            }

         }
         return false;
      }


      /// <summary>
      /// liefert für die betroffenen Tilemapnummern jeweils die Liste aller Indexe der betroffenen Subdivs
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="groundresolution">Meter je Bildschirmpixel</param>
      /// <param name="level">Ev. ein Problem: Es wird hier angenommen, dass der Level für alle Tilemaps identisch ist!</param>
      /// <returns></returns>
      List<DetailMapIdentifier> getSubdivs4BoundAndGroundresolution(Bound bound, double groundresolution, out int level) {
         List<DetailMapIdentifier> needdmd = new List<DetailMapIdentifier>();
         level = -1;
         int bits = getDesiredBits4Resolution(groundresolution);

         if (useOverviewmap(bits)) {
            level = sampleSubdivs(bound, bits, overviewTileInfo, needdmd);
         } else {
            foreach (TileInfo ti in TileInfo4Tilemapnumber.Values)
               level = sampleSubdivs(bound, bits, ti, needdmd);
         }
         return needdmd;
      }

      /// <summary>
      /// fügt die passenden Subdivs (<see cref="DetailMapIdentifier"/>) an die Liste an
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="bits"></param>
      /// <param name="ti"></param>
      /// <param name="needdmd"></param>
      /// <returns></returns>
      int sampleSubdivs(Bound bound, int bits, TileInfo ti, List<DetailMapIdentifier> needdmd) {
         int level = -1;
         if (ti.Bound.IsOverlapped(bound)) {
            if (!ti.InfosPresent)
               ti.SetTreInfos(this, mapDirectory);
            level = getLevel4Bits(bits, ti);
            int[] idxlst = ti.GetSubdivIndex4BoundAndLevel(bound, level);
            if (idxlst.Length > 0) {
               foreach (int idx in idxlst)
                  needdmd.Add(new DetailMapIdentifier(ti.TilemapNumber, idx));
            }
         }
         return level;
      }

      /// <summary>
      /// aus der Auflösung auf die min. notwendige Bitanzahl schließen (max. 24, real. min. 10?)
      /// </summary>
      /// <param name="groundresolution">Meter je Bildschirmpixel</param>
      /// <returns></returns>
      int getDesiredBits4Resolution(double groundresolution) {
         /*
          * 1 << (24 - coordbits)        
          * ---------------------- * 360.0
          *        1 << 24                
          *        
          *  1 Garmin-RawUnit steht für:
          *  
          *  bits          Grad                       m am Äquator (Umfang 40075,017km)
          *  24            0,000021457672119140625       2,388657152652740478515625
          *  23            0,00004291534423828125        4,77731430530548095703125
          *  22            0,0000858306884765625         9,5546286106109619140625
          *  21            0,000171661376953125         19,109257221221923828125
          *  20            0,00034332275390625          38,21851444244384765625
          *  19            0,0006866455078125           76,4370288848876953125
          *  18            0,001373291015625           152,874057769775390625
          *  17            0,00274658203125            305,74811553955078125
          *  16            0,0054931640625             611,4962310791015625
          *  15            0,010986328125             1222,992462158203125
          *  14            0,02197265625              2445,98492431640625
          *  13            0,0439453125               4891,9698486328125
          *  12            0,087890625                9783,939697265625
          *  11            0,17578125                19567,87939453125
          *  10            0,3515625                 39135,7587890625
          *  
          *  Bei 24 Bit können Punkte also nur in einem Raster von etwa 239 cm dargestellt werden.
          */
         //if (groundresolution < 10) return 24;
         //else if (groundresolution < 20) return 23;
         //else if (groundresolution < 40) return 22;
         //else if (groundresolution < 80) return 21;
         //else if (groundresolution < 160) return 20;
         //else if (groundresolution < 320) return 19;
         //else if (groundresolution < 640) return 18;
         //else if (groundresolution < 1280) return 17;
         //else if (groundresolution < 2560) return 16;
         //else if (groundresolution < 5120) return 15;
         //else if (groundresolution < 10240) return 14;
         //else if (groundresolution < 20480) return 13;
         //else if (groundresolution < 40960) return 12;
         //else if (groundresolution < 81920) return 11;

         //if (groundresolution < 5) return 24;
         //else if (groundresolution < 10) return 23;
         //else if (groundresolution < 20) return 22;
         //else if (groundresolution < 40) return 21;
         //else if (groundresolution < 80) return 20;
         //else if (groundresolution < 160) return 19;
         //else if (groundresolution < 320) return 18;
         //else if (groundresolution < 640) return 17;
         //else if (groundresolution < 1280) return 16;
         //else if (groundresolution < 2560) return 15;
         //else if (groundresolution < 5120) return 14;
         //else if (groundresolution < 10240) return 13;
         //else if (groundresolution < 20480) return 12;
         //else if (groundresolution < 40960) return 11;

         if (groundresolution < 2.5) return 24;
         else if (groundresolution < 5) return 23;
         else if (groundresolution < 10) return 22;
         else if (groundresolution < 20) return 21;
         else if (groundresolution < 40) return 20;
         else if (groundresolution < 80) return 19;
         else if (groundresolution < 160) return 18;
         else if (groundresolution < 320) return 17;
         else if (groundresolution < 640) return 16;
         else if (groundresolution < 1280) return 15;
         else if (groundresolution < 2560) return 14;
         else if (groundresolution < 5120) return 13;
         else if (groundresolution < 10240) return 12;
         else if (groundresolution < 20480) return 11;


         return 10;
      }

      /// <summary>
      /// liefert einen passenden Garmin-Maplevel für die gewünschte Bitanzahl
      /// </summary>
      /// <param name="bits"></param>
      /// <param name="ti"></param>
      /// <returns></returns>
      int getLevel4Bits(int bits, TileInfo ti) {
         int maplevel = ti.Coordbits4Level.Count - 1;
         for (int i = 0; i < ti.Coordbits4Level.Count; i++) {  // Level 0 hat die niedrigste Bitanzahl und ist i.A. (immer ?) leer
            if (ti.Coordbits4Level[i] >= bits) {
               maplevel = i;
               break;
            }
         }
         return maplevel;
      }

      #region SQLiteCache

      /// <summary>
      /// erzeugt für alle in <see cref="TileInfo4Tilemapnumber"/> registrierten Garmin-Tilemaps für die angegebenen Level jeweils eine SQLite-DB im Cache im Dateiverzeichnis
      /// </summary>
      /// <param name="levels"></param>
      public void SQLiteCacheImport(int[] levels) {
         foreach (uint mapnumber in TileInfo4Tilemapnumber.Keys)
            SQLiteCacheImport(mapnumber, levels);
      }

      /// <summary>
      /// erzeugt für alle angegebenen Garmin-Tilemaps für die angegebenen Level jeweils eine SQLite-DB im Cache im Dateiverzeichnis
      /// </summary>
      /// <param name="mapnumbers"></param>
      /// <param name="levels"></param>
      public void SQLiteCacheImport(uint[] mapnumbers, int[] levels) {
         foreach (uint mapnumber in mapnumbers)
            SQLiteCacheImport(mapnumber, levels);
      }

      /// <summary>
      /// erzeugt für die Garmin-Tilemap für die angegebenen Level eine SQLite-DB im Cache im Dateiverzeichnis
      /// </summary>
      /// <param name="mapnumber"></param>
      /// <param name="levels"></param>
      public void SQLiteCacheImport(uint mapnumber, int[] levels) {
         //SQLiteCache sqliteCache = new SQLiteCache(sqlitePath);
         TileInfo ti = TileInfo4Tilemapnumber[mapnumber];
         ti.SetTreInfos(this, mapDirectory);
         foreach (int level in levels) {
            int[] sdidx = ti.GetSubdivIndex4BoundAndLevel(ti.Bound, level);
            if (sdidx.Length > 0) {
               GarminTilemapReader garminTilemapReader = new GarminTilemapReader(this, mapnumber, mapDirectory);
               garminTilemapReader.GetDetailmaps(sdidx, true);
               garminTilemapReader.Dispose();
            }
         }
      }

      /// <summary>
      /// löscht die Daten für dieses Tilemap im Cache im Dateiverzeichnis
      /// </summary>
      /// <param name="mapnumber"></param>
      public void ClearSQLiteCache(uint mapnumber) {
         sqliteCache.RemoveDB(mapnumber);
      }

      /// <summary>
      /// löscht alle Daten für die akt. Karte aus dem Cache im Dateiverzeichnis
      /// </summary>
      public void ClearSQLiteCache() {
         foreach (var item in TileInfo4Tilemapnumber) {
            ClearSQLiteCache(item.Key);
         }
      }

      #endregion


      /// <summary>
      /// liefert die geogr. Breite des Kartenmittelpunktes
      /// </summary>
      /// <returns></returns>
      public double GetMapCenterLat() {
         if (overviewTileInfo != null) {
            if (!overviewTileInfo.InfosPresent)
               overviewTileInfo.SetTreInfos(this, mapDirectory);
            return overviewTileInfo.Bound.CenterYDegree;
         } else {
            double north = -90;
            double south = 90;
            foreach (uint mapnumber in TileInfo4Tilemapnumber.Keys) {
               TileInfo ti = TileInfo4Tilemapnumber[mapnumber];
               if (!ti.InfosPresent)
                  ti.SetTreInfos(this, mapDirectory);
               north = Math.Max(north, ti.Bound.TopDegree);
               south = Math.Min(south, ti.Bound.BottomDegree);
            }
            return south + (north - south) / 2;
         }
      }

#if GETDATASEQUENTIEL
      readonly object lock4getdatasequentiel = new object();
#endif

      /// <summary>
      /// liefert alle geogr. Daten, die den Bereich berühren
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="groundresolution"></param>
      /// <param name="pointlst"></param>
      /// <param name="linelst"></param>
      /// <param name="arealst"></param>
      public void GetAllData(Bound bound,
                          double groundresolution,
                          out SortedList<int, List<DetailMap.GeoPoint>> pointlst,
                          out SortedList<int, List<DetailMap.GeoPoly>> linelst,
                          out SortedList<int, List<DetailMap.GeoPoly>> arealst) {
#if GETDATASEQUENTIEL
         lock (lock4getdatasequentiel) {
#endif

         pointlst = new SortedList<int, List<DetailMap.GeoPoint>>();
         linelst = new SortedList<int, List<DetailMap.GeoPoly>>();
         arealst = new SortedList<int, List<DetailMap.GeoPoly>>();

         //         if (!(
         //11.953 <= bound.LeftDegree && bound.LeftDegree <= 11.954 &&
         //12.656 <= bound.RightDegree && bound.RightDegree <= 12.657 &&
         //51.179 <= bound.BottomDegree && bound.BottomDegree <= 51.180 &&
         //51.618 <= bound.TopDegree && bound.TopDegree <= 51.619
         //))
         //            return;

         List<DetailMapIdentifier> needdm = getSubdivs4BoundAndGroundresolution(bound, groundresolution, out int level);  // Liste der notwendigen Subdivs/Tilemaps

         //hrw.Store(string.Format("GetAllData needdmd {0} ({1})", needdmd.Count, bound));

         if (needdm.Count > 0) {
            if (maxSubdivs > 0 &&
                maxSubdivs < needdm.Count)
               throw new Exception("too much subdivs (" + needdm.Count.ToString() + ", max=" + maxSubdivs.ToString() + ")");

            // Liste nach Tilemap-Nummern und Indexen sortieren
            needdm.Sort(delegate (DetailMapIdentifier i1, DetailMapIdentifier i2) { return i1.CompareTo(i2); });

            // Liste der gelieferten DetailMapExt
            List<DetailMapExt> dmlst = new List<DetailMapExt>();

#if USEHIGHRESOLUTIONWATCH
            //if (hrw4test.Count == 0)
            hrw4test.Start();
#endif

            for (int i = 0; i < needdm.Count; i++) {
               // jeweils für 1 Tilemap-Nummer eine Liste der Subdiv-Indexe bilden
               int len = 1;
               for (int j = i + 1; j < needdm.Count; j++) {
                  if (needdm[j].TilemapNumber == needdm[i].TilemapNumber)
                     len++;
                  else
                     break;
               }
               int[] subdividx = new int[len];
               for (int j = 0; j < len; j++)
                  subdividx[j] = needdm[i + j].SubdivIndex;

               // GarminTilemapReader für die akt. Tilemap holen/erzeugen und ...
               GarminTilemapReader garminTilemapReader = getGarminTilemapReader(needdm[i].TilemapNumber);
               // ... die gesuchten DetailMapExt holen (aus Hauptspeicher-Cache, SQLite-Cache oder den Garmin-Dateien)
               dmlst.AddRange(garminTilemapReader.GetDetailmaps(subdividx,
                                                                sqliteCache != null &&
                                                                sqliteCache.ValidLevels != null &&
                                                                sqliteCache.ValidLevels.Contains(level)));
#if USEHIGHRESOLUTIONWATCH
               hrw4test.Store(string.Format("GetAllData() Mapnumber: {0} ({1})", needdm[i].TilemapNumber, bound));
#endif

               //GC.Collect(); // bremst relativ stark
               //GC.Collect(1, GCCollectionMode.Optimized, false);   // hat keinen sinnvollen Einfluss

               i += len - 1; // -1 wegen i++ in for()
            }

            //hrw.Store(string.Format("GetAllData() all Detailmaps readed ({0})", bound));

#if USEHIGHRESOLUTIONWATCH
            hrw4test.Store(string.Format("GetAllData() all Detailmaps readed ({0})", bound));
#endif

            // temp. Ergebnislisten füllen
            List<DetailMap.GeoPoint> tmppointlst = new List<DetailMap.GeoPoint>();
            List<DetailMap.GeoPoly> tmplinelst = new List<DetailMap.GeoPoly>();
            List<DetailMap.GeoPoly> tmparealst = new List<DetailMap.GeoPoly>();
            foreach (DetailMapExt dmd in dmlst) {

               if (dmd.Bound.IsEnclosed(12.37281561, 51.30170980))
                  Debug.WriteLine(">>>> " + dmd.ToString());

               dmd.AddAreas2List(tmparealst, bound);
               dmd.AddLines2List(tmplinelst, bound);
               dmd.AddPoints2List(tmppointlst, bound);
            }

            // Ergebnislisten füllen
            foreach (DetailMap.GeoPoint point in tmppointlst) {
               if (!pointlst.ContainsKey(point.Type))
                  pointlst.Add(point.Type, new List<DetailMap.GeoPoint>());
               pointlst[point.Type].Add(point);
            }

            foreach (DetailMap.GeoPoly poly in tmparealst) {
               if (!arealst.ContainsKey(poly.Type))
                  arealst.Add(poly.Type, new List<DetailMap.GeoPoly>());
               arealst[poly.Type].Add(poly);
            }

            foreach (DetailMap.GeoPoly poly in tmplinelst) {
               if (!linelst.ContainsKey(poly.Type))
                  linelst.Add(poly.Type, new List<DetailMap.GeoPoly>());
               linelst[poly.Type].Add(poly);
            }

            tmppointlst.Clear();
            tmplinelst.Clear();
            tmparealst.Clear();

#if USEHIGHRESOLUTIONWATCH
            hrw4test.Store(string.Format("GetAllData() ready ({0})", bound));
            hrw4test.Stop();

            for (int t = 0; t < hrw4test.Count; t++) {
               Debug.WriteLine(string.Format("HighResolutionWatch: {0}  {1}; {2}; {3}", t, hrw4test.Seconds(t), hrw4test.StepSeconds(t), hrw4test.Description(t)));
            }

#endif
         }
#if GETDATASEQUENTIEL
         }
#endif

         //GC.Collect();

      }

      public override string ToString() {
         return string.Format("{0}, {1} Tilemaps, TilemapReaderCache {2}, DetailMapExtCache {3}",
                              mapDirectory,
                              TileInfo4Tilemapnumber.Count,
                              garminTilemapReaderCache.ToString(),
                              dmCache.ToString()); ;
      }

      ~DetailMapManager() {
         Dispose(false);
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

               TileInfo4Tilemapnumber?.Clear();
               sfsCache?.Clear();
               dmCache?.Clear();
               garminTilemapReaderCache?.Clear();

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion


   }
}
