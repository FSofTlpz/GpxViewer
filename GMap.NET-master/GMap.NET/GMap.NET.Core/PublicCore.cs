using GMap.NET.Internals;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GMap.NET {
   public class PublicCore {

      Core core;

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
      /// akt. Zoom
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

      public bool ZoomToArea => core.ZoomToArea;

      /// <summary>
      ///     gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      /// <returns></returns>
      public RectLatLng ViewArea => core.ViewArea;

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

      public DateTime LastInvalidation {
         get => core.LastInvalidation; set => core.LastInvalidation = value;
      }

      public object InvalidationLock => core.InvalidationLock;

      public GPoint RenderOffset => core.RenderOffset;

      public PointLatLng? LastLocationInBounds => core.LastLocationInBounds;

      public bool IsStarted => core.IsStarted;

      /// <summary>
      /// true, wenn Dragging in Aktion ist
      /// </summary>
      public bool IsDragging => core.IsDragging;

      public bool UpdatingBounds => core.UpdatingBounds;

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


      public PublicCore() {
         core = new Core();

         core.OnCurrentPositionChanged += Core_OnCurrentPositionChanged;
         core.OnEmptyTileError += Core_OnEmptyTileError;
         core.OnMapDrag += Core_OnMapDrag;
         core.OnMapTypeChanged += Core_OnMapTypeChanged;
         core.OnMapZoomChanged += Core_OnMapZoomChanged;
         core.OnTileLoadComplete += Core_OnTileLoadComplete;
         core.OnTileLoadStart += Core_OnTileLoadStart;
      }

      #region Umsetzung der Core-Events

      private void Core_OnTileLoadStart() => OnTileLoadStart?.Invoke();

      private void Core_OnTileLoadComplete(long elapsedMilliseconds) => OnTileLoadComplete?.Invoke(elapsedMilliseconds);

      private void Core_OnMapZoomChanged() => OnMapZoomChanged?.Invoke();

      private void Core_OnMapTypeChanged(GMapProvider type) => OnMapTypeChanged?.Invoke(type);

      private void Core_OnMapDrag() => OnMapDrag?.Invoke();

      private void Core_OnEmptyTileError(int zoom, GPoint pos) => OnEmptyTileError?.Invoke(zoom, pos);

      private void Core_OnCurrentPositionChanged(PointLatLng point) => OnCurrentPositionChanged?.Invoke(point);

      #endregion

      public PointLatLng FromLocalToLatLng(int x, int y, float scale = 1, bool ondragging = false) {
         if (ondragging) {
            x += (int)core.RenderOffset.X;
            y += (int)core.RenderOffset.Y;
         }
         FromScaledLocalToLocal(ref x, ref y, scale);
         return core.FromLocalToLatLng(x, y);
      }

      public GPoint FromLatLngToLocal(PointLatLng latlng, float scale = 1, bool ondragging = false) {
         GPoint p = core.FromLatLngToLocal(latlng);
         FromLocalToScaledLocal(ref p, scale);
         if (ondragging)
            p.OffsetNegative(core.RenderOffset);
         return p;
      }

      public void FromLocalToScaledLocal(ref GPoint p, float scale = 1) {
         if (scale != 1) {
            p.X = (int)(core.RenderOffset.X + (p.X - core.RenderOffset.X) * scale);
            p.Y = (int)(core.RenderOffset.Y + (p.Y - core.RenderOffset.Y) * scale);
         }
      }

      public void FromScaledLocalToLocal(ref int x, ref int y, float scale = 1) {
         if (scale != 1) {
            x = (int)(core.RenderOffset.X + (x - core.RenderOffset.X) / scale);
            y = (int)(core.RenderOffset.Y + (y - core.RenderOffset.Y) / scale);
         }
      }

      public BackgroundWorker OnMapOpen() => core.OnMapOpen();

      public void OnMapClose() => core.OnMapClose();

      public void OnMapSizeChanged(int width, int height) => core.OnMapSizeChanged(width, height);

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
      /// entfernt alle Ladeaufträge oder alle bis auf die für den akt. Zoom aus den Warteschlangen
      /// </summary>
      /// <param name="all">alle oder alle außer dem akt. Zoom</param>
      public void CancelUnnecessaryThreads(bool all) {
         if (IsStarted) {

            /* MIST
            Bei der Zoomänderung erfolgt schon

               TileLoadQueue.Clear();

            Bei Core.UpdateBounds() (u.a. bei Zoomänderung) erfolgt

               TileDrawingList.Clear();

            Insofern hätte nur das stoppen von Threads in core._gThreadPool einen Sinn.

            */

            ////core.TileDrawingListLock.AcquireWriterLock();
            //core.TileDrawingListLock.AcquireReaderLock();
            //Monitor.Enter(core.TileLoadQueue);
            //try {
            //   if (all) {
            //      core.TileDrawingList.Clear();
            //      core.TileLoadQueue.Clear();

            //   } else {
            //      long zf = (long)Math.Pow(2, core.Zoom);
            //      for (int i = core.TileDrawingList.Count - 1; 0 <= i; i--) {
            //         long p = core.TileDrawingList[i].PosPixel.X / core.TileDrawingList[i].PosXY.X;
            //         if (zf != p)
            //            core.TileDrawingList.RemoveAt(i);
            //      }

            //      LoadTask[] lt = new LoadTask[core.TileLoadQueue.Count];
            //      core.TileLoadQueue.CopyTo(lt, 0);
            //      core.TileLoadQueue.Clear();
            //      for (int i = 0; i < lt.Length; i++) {
            //         if (lt[i].Zoom == core.Zoom)
            //            core.TileLoadQueue.Push(lt[i]);
            //      }

            //      // Die bereits in Bearbeitung befindlichen LoadTask können nicht mehr gestoppt werden. (core._gThreadPool)

            //   }
            //} finally {
            //   Monitor.Exit(core.TileLoadQueue);
            //   core.TileDrawingListLock.ReleaseReaderLock();
            //   //core.TileDrawingListLock.ReleaseWriterLock();
            //}
         }
      }

      /// <summary>
      /// Anzahl der Tiles die noch in der Warteschlange stehen
      /// </summary>
      /// <returns></returns>
      public int MapTilesInQueue() {
         System.Threading.Monitor.Enter(core.TileLoadQueue);
         int queuecount = core.TileLoadQueue.Count;
         System.Threading.Monitor.Exit(core.TileLoadQueue);
         //lock (core._gThreadPool) {
         //   core._gThreadPool.Count;
         //}
         return queuecount;
      }

      /// <summary>
      ///     gets max zoom level to fit rectangle
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public int GetMaxZoomToFitRect(RectLatLng rect) => core.GetMaxZoomToFitRect(rect);

      /// <summary>
      /// get the tile if exists without lock from the tile-matrix
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="posxy"></param>
      /// <returns></returns>
      public Tile GetTile(int zoom, GPoint posxy) => core.Matrix.GetTileWithNoLock(zoom, posxy);   // das zugehörige Tile holen

      public Tile GetTile(GPoint posxy) => core.Matrix.GetTileWithNoLock(core.Zoom, posxy);   // das zugehörige Tile holen

      public void LockImageStore() {
         core.TileDrawingListLock.AcquireReaderLock();
         core.Matrix.EnterReadLock();
      }

      public void ReleaseImageStore() {
         core.Matrix.LeaveReadLock();
         core.TileDrawingListLock.ReleaseReaderLock();
      }

      /// <summary>
      /// get the exception if load of the tile fails or null
      /// </summary>
      /// <param name="posxy"></param>
      /// <returns></returns>
      public Exception GetException4FailedLoad(GPoint posxy) {
         lock (core.FailedLoads) {
            var lt = new LoadTask(posxy, core.Zoom);
            if (core.FailedLoads.ContainsKey(lt))
               return core.FailedLoads[lt];
         }
         return null;
      }

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
      public void SetPositionDirect(PointLatLng point) {
         core._position = point;
      }

      /// <summary>
      /// starts the internal refresh
      /// </summary>
      public void StartRefresh() {
         core.Refresh?.Set();
      }

      public GPoint RealClientPoint(int clientx, int clienty) {
         GPoint p = new GPoint(clientx, clienty);
         p.OffsetNegative(core.RenderOffset);
         return p;
      }


      public GPoint FromClientToGlobal(GPoint ptClient) => FromClientToGlobal(ptClient.X, ptClient.Y);

      public GPoint FromClientToGlobal(long xclient, long yclient) {
         var p = new GPoint(xclient, yclient);  // Client-Koordinaten (bzgl. Ecke link-oben)
         p.OffsetNegative(core.RenderOffset);   // Client-Koordinaten (bzgl. RenderOffset = Client-Mittelpunkt)
         p.Offset(core.CompensationOffset);     // globale Koordinaten (CompensationOffset sind die globalen Koordinaten des Client-Mittelpunkt)
         return p;
      }

      public GPoint FromGlobalToClient(GPoint ptGlobal) => FromGlobalToClient(ptGlobal.X, ptGlobal.Y);

      public GPoint FromGlobalToClient(long xglobal, long yglobal) {
         var p = new GPoint(xglobal, yglobal);
         p.OffsetNegative(core.CompensationOffset);   // Client-Koordinaten (bzgl. RenderOffset = Client-Mittelpunkt)
         p.Offset(core.RenderOffset);                 // Client-Koordinaten (bzgl. Ecke link-oben)
         return p;
      }

      #region internals from PureImage and PureImageProxy

      public static Int64 GetImageXoff(PureImage img) => img.Xoff;

      public static Int64 GetImageYoff(PureImage img) => img.Yoff;

      public static Int64 GetImageIx(PureImage img) => img.Ix;

      public static bool GetImageIsParent(PureImage img) => img.IsParent;

      public static PureImageProxy TileImageProxy {
         get => GMapProvider.TileImageProxy;
         set => GMapProvider.TileImageProxy = value;
      }

      #endregion

      #region internals from Stuff

      public static bool IsRunningOnWin7OrLater => Stuff.IsRunningOnWin7OrLater();

      public static bool SetMousePosition(int x, int y) {
         return Stuff.SetCursorPos(x, y);
      }

      public static void Shuffle<T>(List<T> deck) {
         Stuff.Shuffle(deck);
      }

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


      public void Dispose() => core.Dispose();

   }
}
