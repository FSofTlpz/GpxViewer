//#define LOCALDEBUG

using GMap.NET.Internals;
using GMap.NET.MapProviders;
using System;
using System.ComponentModel;
#if LOCALDEBUG
using System.Diagnostics;
using System.Threading;
#endif

namespace GMap.NET.FSofTExtented {
   public class PublicCore {

      Core core;

      #region readonly

      /// <summary>
      /// Größe der "sichtbaren Karte" im Core
      /// </summary>
      public GPoint CoreSize => new GPoint(core.Width, core.Height);

      /// <summary>
      /// beim Providerwechsel wird zum vorherigen Gebiet gezoomt
      /// </summary>
      public bool ZoomToArea => core.ZoomToArea;

      /// <summary>
      ///     gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      /// <returns></returns>
      public RectLatLng ViewArea => core.ViewArea;

      public GPoint RenderOffset => core.RenderOffset;

      public PointLatLng? LastLocationInBounds => core.LastLocationInBounds;

      /// <summary>
      /// true nach <see cref="Core.OnMapDrag"/>
      /// </summary>
      public bool IsStarted => core.IsStarted;

      /// <summary>
      /// true, wenn Dragging in Aktion ist
      /// </summary>
      public bool IsDragging => core.IsDragging;

      /// <summary>
      /// true wenn in <see cref="Core.UpdateBounds"/>
      /// </summary>
      public bool UpdatingBounds => core.UpdatingBounds;

      #endregion


      /// <summary>
      /// Anzahl der Threads zum Laden der Daten (Standard 4)
      /// </summary>
      public static int ThreadPoolSize {
         get => Core.GThreadPoolSize;
         set => Core.GThreadPoolSize = value;
      }

      /// <summary>
      /// wird bei UserControl.OnMouseDown(MouseEventArgs e) auf die Clientkoordinaten der Maus und
      /// bei <see cref="EndDrag"/> im <see cref="Core"/> wieder auf <see cref="GPoint.Empty"/> gesetzt
      /// </summary>
      public GPoint MouseDown {
         get => core.MouseDown; set => core.MouseDown = value;
      }

      public GPoint MouseLastZoom {
         get => core.MouseLastZoom; set => core.MouseLastZoom = value;
      }

      /// <summary>
      /// regelt, ob der Zoom bezüglich der Kartenmitte oder der Mausposition (und wie) erfolgt
      /// </summary>
      public MouseWheelZoomType MouseWheelZoomType {
         get => core.MouseWheelZoomType; set => core.MouseWheelZoomType = value;
      }

      /// <summary>
      /// für <see cref="Core.GoToCurrentPositionOnZoom"/> beim Zoomen per Mausrad
      /// </summary>
      public bool MouseWheelZooming {
         get => core.MouseWheelZooming; set => core.MouseWheelZooming = value;
      }

      /// <summary>
      /// max. Zoom (i.A. 24)
      /// </summary>
      public int MaxZoom {
         get => core.MaxZoom; set => core.MaxZoom = value;
      }

      /// <summary>
      /// min. Zoom (i.A. 2)
      /// </summary>
      public int MinZoom {
         get => core.MinZoom; set => core.MinZoom = value;
      }

      /// <summary>
      /// akt. (ganzzahliger) Zoom
      /// </summary>
      public int Zoom {
         get => core.Zoom;
         set {
            if (value > MaxZoom) {
               core.Zoom = MaxZoom;
            } else if (value < MinZoom) {
               core.Zoom = MinZoom;
            } else {
               core.Zoom = value;
            }
         }
      }

      /// <summary>
      /// akt. Kartenzentrum in Lat/Lgn
      /// </summary>
      public PointLatLng Position {
         get => core.Position; set => core.Position = value;
      }

      /// <summary>
      /// akt. verwendeter Karten-Provider
      /// </summary>
      public GMapProvider Provider {
         get => core.Provider; set => core.Provider = value;
      }

      /// <summary>
      ///     is polygons enabled
      /// </summary>
      public bool PolygonsEnabled {
         get => core.PolygonsEnabled; set => core.PolygonsEnabled = value;
      }

      /// <summary>
      ///     is routes enabled
      /// </summary>
      public bool RoutesEnabled {
         get => core.RoutesEnabled; set => core.RoutesEnabled = value;
      }

      /// <summary>
      ///     is markers enabled
      /// </summary>
      public bool MarkersEnabled {
         get => core.MarkersEnabled; set => core.MarkersEnabled = value;
      }

      /// <summary>
      ///     can user drag map
      /// </summary>
      public bool CanDragMap {
         get => core.CanDragMap; set => core.CanDragMap = value;
      }

      /// <summary>
      ///     retry count to get tile
      /// </summary>
      public int RetryLoadTile {
         get => core.RetryLoadTile; set => core.RetryLoadTile = value;
      }

      /// <summary>
      /// enables filling empty tiles using lower level images
      /// </summary>
      public bool FillEmptyTiles {
         get => core.FillEmptyTiles; set => core.FillEmptyTiles = value;
      }

      /// <summary>
      ///     how many levels of tiles are staying decompressed in memory
      /// </summary>
      public int LevelsKeepInMemory {
         get => core.LevelsKeepInMemory; set => core.LevelsKeepInMemory = value;
      }

      /// <summary>
      /// threadsicherer Zugriff auf den letzten Zeitpunkt der Invalidation
      /// </summary>
      public DateTime LastInvalidation {
         get {
            lock (core.InvalidationLock)
               return core.LastInvalidation;
         }
         set {
            lock (core.InvalidationLock)
               core.LastInvalidation = value;
         }
      }


      #region Events

      ///// <summary>
      /////     occurs when current position is changed
      ///// </summary>
      //public event PositionChanged OnCurrentPositionChanged;

      ///// <summary>
      /////     occurs when tile set load is complete
      ///// </summary>
      //public event TileLoadComplete OnTileLoadComplete;

      ///// <summary>
      /////     occurs when tile set is starting to load
      ///// </summary>
      //public event TileLoadStart OnTileLoadStart;

      ///// <summary>
      /////     occurs on empty tile displayed
      ///// </summary>
      //public event EmptyTileError OnEmptyTileError;

      ///// <summary>
      /////     occurs on map drag
      ///// </summary>
      //public event MapDrag OnMapDrag;

      ///// <summary>
      /////     occurs on map zoom changed
      ///// </summary>
      //public event MapZoomChanged OnMapZoomChanged;

      ///// <summary>
      /////     occurs on map type changed
      ///// </summary>
      //public event MapTypeChanged OnMapTypeChanged;


      /// <summary>
      ///     occurs when current position is changed
      /// </summary>
      public event PositionChanged OnCurrentPositionChanged;

      /// <summary>
      ///     occurs when tile set load is complete
      /// </summary>
      public event TileLoadComplete OnTileLoadComplete;

      /// <summary>
      ///     occurs when tile set is starting to load
      /// </summary>
      public event TileLoadStart OnTileLoadStart;

      /// <summary>
      ///     occurs on empty tile displayed
      /// </summary>
      public event EmptyTileError OnEmptyTileError;

      /// <summary>
      ///     occurs on map drag
      /// </summary>
      public event MapDrag OnMapDrag;

      /// <summary>
      ///     occurs on map zoom changed
      /// </summary>
      public event MapZoomChanged OnMapZoomChanged;

      /// <summary>
      ///     occurs on map type changed
      /// </summary>
      public event MapTypeChanged OnMapTypeChanged;

      #endregion


      static PublicCore() {
         // eigene Provider die keine spez. Definitionen benötigen werden hier ganz normal zusätzlich registriert:
         GMapProviders.List.Add(OpenStreetMapDEProvider.Instance);

      }


      public PublicCore() {
         core = new Core();

         core.OnCurrentPositionChanged += (point) => OnCurrentPositionChanged?.Invoke(point);
         core.OnEmptyTileError += (zoom, pos) => OnEmptyTileError?.Invoke(zoom, pos);
         core.OnMapDrag += () => OnMapDrag?.Invoke();
         core.OnMapTypeChanged += (type) => OnMapTypeChanged?.Invoke(type);
         core.OnMapZoomChanged += () => OnMapZoomChanged?.Invoke();
         core.OnTileLoadComplete += (elapsedMilliseconds) => OnTileLoadComplete?.Invoke(elapsedMilliseconds);
         core.OnTileLoadStart += () => OnTileLoadStart?.Invoke();
      }

      /// <summary>
      /// öffnet ? und liefert einen Backgroundworker der die Invalidations überwacht und der bei ProgressChanged ...
      /// </summary>
      /// <returns></returns>
      public BackgroundWorker MapOpen() => core.OnMapOpen();

      public void MapOpenExt(Action shoulddrawaction) {
         BackgroundWorker bw = core.OnMapOpen();
         bw.ProgressChanged += (s, e) => shoulddrawaction();
      }

      public void MapClose() => core.OnMapClose();

      /// <summary>
      /// Info an Core
      /// </summary>
      /// <param name="corewidth"></param>
      /// <param name="coreheight"></param>
      public void MapSizeChanged(int corewidth, int coreheight) => core.OnMapSizeChanged(corewidth, coreheight);

      /// <summary>
      ///     initiates map dragging
      /// </summary>
      /// <param name="pt"></param>
      public void BeginDrag(GPoint pt) => core.BeginDrag(pt);

      /// <summary>
      ///     ends map dragging
      /// </summary>
      public void EndDrag() => core.EndDrag();

      /// <summary>
      /// drag map
      /// </summary>
      /// <param name="pt"></param>
      public void Drag(GPoint pt) => core.Drag(pt);

      /// <summary>
      ///     reloads map
      /// </summary>
      public void ReloadMap() => core.ReloadMap();

      /// <summary>
      ///     gets max zoom level to fit rectangle
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public int GetMaxZoomToFitRect(RectLatLng rect) => core.GetMaxZoomToFitRect(rect);

      /// <summary>
      /// sperrt den ImageStore gegen Veränderungen (beim Zeichnen der Karte nötig)
      /// </summary>
      public void LockImageStore() {
         core.TileDrawingListLock.AcquireReaderLock();
         core.Matrix.EnterReadLock();
      }

      /// <summary>
      /// hebt die Veränderungssperre des ImageStore auf
      /// </summary>
      public void ReleaseImageStore() {
         core.Matrix.LeaveReadLock();
         core.TileDrawingListLock.ReleaseReaderLock();
      }

      /// <summary>
      /// get the tile if exists without lock from the tile-matrix
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="posxy"></param>
      /// <returns></returns>
      public Tile GetTile(int zoom, GPoint posxy) => core.Matrix.GetTileWithNoLock(zoom, posxy);   // das zugehörige Tile holen

      /// <summary>
      /// liefert das <see cref="Tile"/> zur gewünschten Position und dem akt. <see cref="core.Zoom"/>
      /// </summary>
      /// <param name="posxy"></param>
      /// <returns></returns>
      public Tile GetTile(GPoint posxy) => core.Matrix.GetTileWithNoLock(Zoom, posxy);   // das zugehörige Tile holen

      /// <summary>
      /// get the exception if load of the tile fails or null
      /// </summary>
      /// <param name="posxy"></param>
      /// <returns></returns>
      public Exception GetException4FailedLoad(GPoint posxy) {
         var lt = new LoadTask(posxy, core.Zoom);
         lock (core.FailedLoads) {
            if (core.FailedLoads.ContainsKey(lt))
               return core.FailedLoads[lt];
         }
         return null;
      }

      /// <summary>
      /// liefert eine Kopie der internen <see cref="Core.TileDrawingList"/> (Tiles die z.B. akt. für die Bilderzeugung benötigt werden)
      /// </summary>
      /// <returns></returns>
      public GPoint[] GetTilePosXYDrawingList() {
         GPoint[] posxy = new GPoint[core.TileDrawingList.Count];
         for (int i = 0; i < core.TileDrawingList.Count; i++)
            posxy[i] = core.TileDrawingList[i].PosXY;
         return posxy;
      }

      /// <summary>
      /// liefert die Position und Größe des Tiles in globalen Koordinaten
      /// </summary>
      /// <param name="tileno"></param>
      /// <param name="tileSize"></param>
      /// <returns></returns>
      public GPoint GetTileDestination(int tileno, out GSize tileSize) {
         GPoint pospixel = core.TileDrawingList[tileno].PosPixel;
         pospixel.OffsetNegative(core.CompensationOffset);
         tileSize = core.TileRect.Size;
         return pospixel;
      }

      /// <summary>
      /// set the internal <see cref="Core._position"/> direct (<see cref="Position"/> is not the same)
      /// </summary>
      /// <param name="point"></param>
      public void SetPositionDirect(PointLatLng point) => core._position = point;

      /// <summary>
      /// führt zur Auslösung der Action aus <see cref="MapOpenExt(Action)"/>
      /// </summary>
      public void InitiateRefresh() => core.Refresh?.Set();

      #region internals from PureImage and PureImageProxy

      public static long GetImageXoff(PureImage img) => img.Xoff;

      public static long GetImageYoff(PureImage img) => img.Yoff;

      public static long GetImageIx(PureImage img) => img.Ix;

      public static bool GetImageIsParent(PureImage img) => img.IsParent;

      public static PureImageProxy TileImageProxy {
         get => GMapProvider.TileImageProxy;
         set => GMapProvider.TileImageProxy = value;
      }

      #endregion

      #region internals from Stuff

      public static bool IsRunningOnWin7OrLater => Stuff.IsRunningOnWin7OrLater();

      public static bool SetMousePosition(int corex, int corey) => Stuff.SetCursorPos(corex, corey);

      //public static void Shuffle<T>(List<T> deck) => Stuff.Shuffle(deck);

      #endregion

      #region internals from CacheLocator

      public static string MapCacheLocation {
         get => CacheLocator.Location;
         set {
            if (CacheLocator.Location != value)
               CacheLocator.Location = value;
         }
      }

      #endregion


      /// <summary>
      /// Anzahl der Tiles die noch auf ihre Bearbeitung warten (threadsicher)
      /// </summary>
      /// <returns></returns>
      public int WaitingTiles() {
         int count = 0; // core.TileDrawingList.Count;

#if !NETFRAMEWORK
         lock (core.TileLoadQueue)
            count += core.TileLoadQueue.Count;
#else
         lock (Core.TileLoadQueue4) {
            count += Core.TileLoadQueue4.Count;
#if LOCALDEBUG
            LoadTask[] t = Core.TileLoadQueue4.ToArray();
            for (int i = 0; i < t.Length; i++)
               Debug.WriteLine("   TileLoadQueue[" + i + "]: " + t[i]);
#endif
         }
#endif
         return count;
      }

      /// <summary>
      /// leert die Liste der wartenden Tiles/Tasks (aber nicht die Liste der bereits in Bearbeitung befindlichen!)
      /// </summary>
      public void ClearWaitingTaskList() {
         core.TileDrawingList.Clear();
         core.CancelAsyncTasks();         // Aber die bereits in Arbeit befindlichen Tiles werden weiter bearbeitet!
         //foreach (var t in core._gThreadPool)
         //   t.Abort();
      }

      /// <summary>
      /// liefert Lat/Lon für die core-Koordinaten
      /// </summary>
      /// <param name="corex"></param>
      /// <param name="corey"></param>
      /// <returns></returns>
      public PointLatLng FromLocalCoreToLatLng(int corex, int corey) => core.FromLocalToLatLng(corex, corey);

      /// <summary>
      /// liefert die core-Koordinaten zu Lat/Lon
      /// </summary>
      /// <param name="latlng"></param>
      /// <returns></returns>
      public GPoint FromLatLngToLocalCore(PointLatLng latlng) => core.FromLatLngToLocal(latlng);


      public void Dispose() => core.Dispose();

   }
}
