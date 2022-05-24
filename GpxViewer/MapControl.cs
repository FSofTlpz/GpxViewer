using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using GMap.NET;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.MapProviders;
using SmallMapControl;
using SmallMapControl.EditHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   /// <summary>
   /// Das Control besteht i.W. aus einem <see cref="SmallMapCtrl"/>. Dazu kommt ein Control für den Zoom und ein weiteres zum Verschieben der Karte.
   /// I.A. sollten hier nur die Map-Elemente des <see cref="SmallMapCtrl"/> verwendet werden.
   /// </summary>
   public partial class MapControl : UserControl {

      #region Karten-Events

      event SmallMapCtrl.TileLoadCompleteEventHandler mapTileLoadCompleteEvent;

      //public delegate void TileLoadCompleteEventDelegate(object sender, TileLoadCompleteEventArgs e);
      //TileLoadCompleteEventDelegate myMapWrapper_TileLoadCompleteEventDelegate;
      public event SmallMapCtrl.TileLoadCompleteEventHandler MapTileLoadCompleteEvent;

      public event SmallMapCtrl.MapZoomChangedEventHandler MapZoomChangedEvent;

      public event SmallMapCtrl.MapPaintEventHandler MapPaintEvent;

      public event SmallMapCtrl.MapMouseEventHandler MapMouseEvent;

      public event SmallMapCtrl.MapTrackSearch4PolygonEventHandler MapTrackSearch4PolygonEvent;

      public event SmallMapCtrl.MapTrackEventHandler MapTrackEvent;

      public event SmallMapCtrl.MapMarkerEventHandler MapMarkerEvent;

      #endregion


      bool _trackBarZoomInternSet = false;

      /// <summary>
      /// min. Zoom für die Karte
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(0)]
      public int MapMinZoom {
         get => smc.MapMinZoom;
         set {
            if (value <= smc.MapMaxZoom) {
               smc.MapMinZoom = value;
               _trackBarZoomInternSet = true;
               trackBarZoom.Minimum = value * 10;
               _trackBarZoomInternSet = false;
            }
         }
      }

      /// <summary>
      /// max. Zoom für die Karte
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(24)]
      public int MapMaxZoom {
         get => smc.MapMaxZoom;
         set {
            if (value >= smc.MapMinZoom) {
               smc.MapMaxZoom = value;
               _trackBarZoomInternSet = true;
               trackBarZoom.Maximum = value * 10;
               _trackBarZoomInternSet = false;
            }
         }
      }

      /// <summary>
      /// Zoom für die Karte (intern int)
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(20.0)]
      public double MapZoom {
         get {
            return smc.MapZoom;
         }
         set {
            value = Math.Min(MapMaxZoom, Math.Max(MapMinZoom, value));
            smc.MapZoom = value;

            _trackBarZoomInternSet = true;
            trackBarZoom.Value = (int)Math.Round(value * 10);
            _trackBarZoomInternSet = false;
         }
      }

      /// <summary>
      /// letzte registrierte Mausposition im Karten-Control
      /// </summary>
      [Browsable(false)]
      public Point MapLastMouseLocation {
         get => smc.MapLastMouseLocation;
      }

      /// <summary>
      /// Der Maus-Button zum "Ziehen" der Karte
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(MouseButtons.Left)]
      public MouseButtons MapDragButton {
         get => smc.MapDragButton;
         set => smc.MapDragButton = value;
      }

      bool _mapZoomHandleVisible = true;

      [Browsable(true), Category("Map"), DefaultValue(true)]
      public bool MapZoomHandleVisible {
         get {
            return _mapZoomHandleVisible;
         }
         set {
            if (value != _mapZoomHandleVisible) {
               _mapZoomHandleVisible = value;
               MapControl_SizeChanged(null, null);
               trackBarZoom.Visible = _mapZoomHandleVisible;
            }
         }
      }

      /// <summary>
      /// Maßstab für das <see cref="SmallMapCtrl"/>
      /// </summary>
      Scale4Map scale;


      public MapControl() {
         InitializeComponent();
      }

      private void MapControl_Load(object sender, EventArgs e) {
         smc.MapPaintEvent += Smc_MapPaintEvent;
         smc.MapMouseEvent += Smc_MapMouseEvent;
         smc.MapZoomChangedEvent += Smc_MapZoomChangedEvent;
         smc.MapZoomRangeChangedEvent += Smc_MapZoomRangeChanged;
         smc.MapTileLoadCompleteEvent += Smc_MapTileLoadCompleteEvent;
         smc.MapMarkerEvent += Smc_MapMarkerEvent;
         smc.MapTrackEvent += Smc_MapTrackEvent;
         smc.MapTrackSearch4PolygonEvent += Smc_MapTrackSearch4PolygonEvent;

         moveControl1.DirectionEvent += moveControl1_DirectionEvent;

         // Dummy-Werte
         MapMinZoom = 3;
         MapMaxZoom = 24;
      }

      protected override void OnHandleDestroyed(EventArgs e) {
         base.OnHandleDestroyed(e);

         smc.MapPaintEvent -= Smc_MapPaintEvent;
         smc.MapMouseEvent -= Smc_MapMouseEvent;
         smc.MapZoomChangedEvent -= Smc_MapZoomChangedEvent;
         smc.MapZoomRangeChangedEvent -= Smc_MapZoomRangeChanged;
         smc.MapTileLoadCompleteEvent -= Smc_MapTileLoadCompleteEvent;
         smc.MapMarkerEvent -= Smc_MapMarkerEvent;
         smc.MapTrackEvent -= Smc_MapTrackEvent;
         smc.MapTrackSearch4PolygonEvent -= Smc_MapTrackSearch4PolygonEvent;

         smc.MapPaintEvent -= Smc_MapPaintEvent;

      }

      private void MapControl_SizeChanged(object sender, EventArgs e) {
         smc.Width = MapZoomHandleVisible ?
                                    ClientSize.Width - trackBarZoom.Width :
                                    ClientSize.Width;
         trackBarZoom.Height = ClientSize.Height - moveControl1.Height;
      }

      #region Behandlung der SmallMapControl-Events (i.W. Weiterleitung nach "außen")

      private void Smc_Load(object sender, EventArgs e) {
         scale = new Scale4Map(sender as SmallMapControl.SmallMapCtrl);
      }

      private void Smc_MapZoomChangedEvent(object sender, EventArgs e) {
         _trackBarZoomInternSet = true;
         trackBarZoom.Value = (int)Math.Round(MapZoom * 10);
         _trackBarZoomInternSet = false;

         // => ev. nach "außen" weiterleiten
         MapZoomChangedEvent?.Invoke(sender, e);
      }

      private void Smc_MapZoomRangeChanged(object sender, EventArgs e) {
         _trackBarZoomInternSet = true;
         trackBarZoom.Minimum = MapMinZoom * 10;
         trackBarZoom.Maximum = MapMaxZoom * 10;
         _trackBarZoomInternSet = false;
      }

      private void Smc_MapTileLoadCompleteEvent(object sender, SmallMapCtrl.TileLoadCompleteEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapTileLoadCompleteEvent?.Invoke(sender, e);
      }

      private void Smc_MapTrackEvent(object sender, SmallMapCtrl.TrackEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapTrackEvent?.Invoke(sender, e);
      }

      private void Smc_MapTrackSearch4PolygonEvent(object sender, MouseEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapTrackSearch4PolygonEvent?.Invoke(sender, e);
      }

      private void Smc_MapMarkerEvent(object sender, SmallMapCtrl.MarkerEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapMarkerEvent?.Invoke(sender, e);
      }

      private void Smc_MapMouseEvent(object sender, SmallMapCtrl.MapMouseEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapMouseEvent?.Invoke(sender, e);
      }

      private void Smc_MapPaintEvent(object sender, PaintEventArgs e) {
         // => ev. nach "außen" weiterleiten
         MapPaintEvent?.Invoke(sender, e);
      }

      #endregion

      private void trackBarZoom_ValueChanged(object sender, EventArgs e) {
         if (!_trackBarZoomInternSet)
            MapZoom = trackBarZoom.Value / 10.0;
      }

      private void moveControl1_DirectionEvent(object sender, MoveControl.DirectionEventArgs e) {
         switch (e.Direction) {
            // Sicht auf die Karte verschieben
            case MoveControl.Direction.Left:
               MapMoveView(-.3, 0);
               break;

            case MoveControl.Direction.Right:
               MapMoveView(.3, 0);
               break;

            case MoveControl.Direction.Up:
               MapMoveView(0, .3);
               break;

            case MoveControl.Direction.Down:
               MapMoveView(0, -.3);
               break;

         }
      }


      public EditTrackHelper MapCreateEditTrackHelper(PoorGpxAllExt editablegpx) {
         return editablegpx != null ?
                     new EditTrackHelper(smc, editablegpx) :
                     null;
      }

      public EditMarkerHelper MapCreateEditMarkerHelper(PoorGpxAllExt editablegpx) {
         return editablegpx != null ?
                     new EditMarkerHelper(smc, editablegpx) :
                     null;
      }

      /// <summary>
      /// Ist die Sichtbarkeit aller <see cref="Track"/> im gemeinsamen <see cref="GpxDataContainer"/> dieses <see cref="Track"/> gleich?
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool MapIsSameTrackVisibilityInContainer(Track track) {
         if (track.GpxDataContainer != null) {
            if (track.GpxDataContainer.TrackList.Count == 0)
               return true;

            List<Track> visibletracks = MapGetVisibleTracks(false);
            // alle Tracks des akt. Containers testen
            bool isin = visibletracks.Contains(track.GpxDataContainer.TrackList[0]);      // 1. Track
            for (int i = 1; i < track.GpxDataContainer.TrackList.Count; i++)
               if (visibletracks.Contains(track.GpxDataContainer.TrackList[i]) != isin)
                  return false;

            return true;
         }
         return false;
      }


      /// <summary>
      /// zeigt ein ToolTip an der Position an
      /// </summary>
      /// <param name="tt"></param>
      /// <param name="text"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void MapShowToolTip(ToolTip tt, string text, int clientx, int clienty) {
         tt.Show(text,
                 smc,
                 clientx,
                 clienty);
      }

      /// <summary>
      /// entfernt den ToolTip
      /// </summary>
      /// <param name="tt"></param>
      public void MapHideToolTip(ToolTip tt) {
         tt.Hide(smc);
      }

      /// <summary>
      /// zeigt das Kontextmenü an der Position an
      /// </summary>
      /// <param name="cms"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void MapShowContextMenu(ContextMenuStrip cms, int clientx, int clienty) {
         cms.Show(smc, clientx, clienty);
      }

      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <param name="withscale"></param>
      /// <returns></returns>
      public Image MapGetViewAsImage(bool withscale = true) {
         Image img = smc.MapGetViewAsImage();
         if (withscale && scale != null) {
            Graphics g = Graphics.FromImage(img);
            scale.Draw(g);
         }
         return img;
      }

      #region Weiterleitungen an gleichnamige SmallMapControl-Properties

      /// <summary>
      /// Cache-Verzeichnis
      /// </summary>
      [Browsable(true),
       ReadOnly(true),
       Category("Map")]
      public string MapCacheLocation {
         get => smc.MapCacheLocation;
         set => smc.MapCacheLocation = value;
      }

      /// <summary>
      /// auch den Cache verwenden oder nur den Karten-Server (gilt global)
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(true)]
      public static bool MapCacheIsActiv {
         get => SmallMapCtrl.MapCacheIsActiv;
         set => SmallMapCtrl.MapCacheIsActiv = value;
      }

      /// <summary>
      /// Liste der registrierten Karten-Provider
      /// </summary>
      [Browsable(false)]
      public List<MapProviderDefinition> MapProviderDefinitions {
         get => smc.MapProviderDefinitions;
      }

      /// <summary>
      /// geogr. Länge des Mittelpunktes der Karte
      /// </summary>
      [Browsable(false)]
      public double MapCenterLon {
         get => smc.MapCenterLon;
      }

      /// <summary>
      /// geogr. Breite des Mittelpunktes der Karte
      /// </summary>
      [Browsable(false)]
      public double MapCenterLat {
         get => smc.MapCenterLat;
      }

      /// <summary>
      /// Cursor der Karte
      /// </summary>
      [Browsable(true), Category("Map")]
      public Cursor MapCursor {
         get => smc.MapCursor;
         set => smc.MapCursor = value;
      }

      #endregion

      #region Weiterleitungen an gleichnamige SmallMapControl-Funktionen

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(int clientx, int clienty) {
         return smc.MapClient2LonLat(clientx, clienty);
      }

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(Point ptclient) {
         return MapClient2LonLat((int)ptclient.X, (int)ptclient.Y);
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void MapLonLat2Client(double lon, double lat, out int clientx, out int clienty) {
         smc.MapLonLat2Client(lon, lat, out clientx, out clienty);
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(double lon, double lat) {
         return smc.MapLonLat2Client(new PointLatLng(lat, lon));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(PointLatLng ptgeo) {
         return smc.MapLonLat2Client(ptgeo);
      }

      public Point MapLonLat2Client(Gpx.GpxWaypoint ptgeo) {
         return smc.MapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(Gpx.GpxTrackPoint ptgeo) {
         return smc.MapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      /// <summary>
      /// Wird gerade ein Auswahlrechteck gezeichnet?
      /// </summary>
      [Browsable(false)]
      public bool MapSelectionAreaIsStarted {
         get => smc.MapSelectionAreaIsStarted;
      }

      /// <summary>
      /// startet die Auswahl einer Fläche
      /// </summary>
      public void MapStartSelectionArea() {
         smc.MapStartSelectionArea();
      }

      /// <summary>
      /// liefert eine ausgewählte Fläche oder null
      /// </summary>
      /// <returns></returns>
      public Gpx.GpxBounds MapEndSelectionArea() {
         return smc.MapEndSelectionArea();
      }


      /// <summary>
      /// set a internet proxy
      /// </summary>
      /// <param name="proxy">proxy hostname (if null or empty <see cref="System.Net.WebRequest.DefaultWebProxy"/>)</param>
      /// <param name="proxyport">proxy portnumber</param>
      /// <param name="user">username</param>
      /// <param name="password">userpassword</param>
      public static void MapSetProxy(string proxy, int proxyport, string user, string password) {
         SmallMapCtrl.MapSetProxy(proxy, proxyport, user, password);
      }

      /// <summary>
      /// registriert die zu verwendenden Karten-Provider in der Liste <see cref="MapProviderDefinitions"/>
      /// </summary>
      /// <param name="providernames"></param>
      /// <param name="garmindefs"></param>
      /// <param name="wmsdefs"></param>
      /// <param name="kmzdefs"></param>
      public void MapRegisterProviders(IList<string> providernames,
                                       List<MapProviderDefinition> provdefs) {
         smc.MapRegisterProviders(providernames, provdefs);
      }

      /// <summary>
      /// setzt den aktiven Karten-Provider
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      public void MapSetActivProvider(int idx, int demalpha, DemData dem = null) {
         smc.MapSetActivProvider(idx, demalpha, dem);
      }

      /// <summary>
      /// zeichnet die Karte neu
      /// </summary>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      public void MapRefresh(bool clearmemcache, bool clearcache) {
         smc.MapRefresh(clearmemcache, clearcache);
      }

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public void MapSetLocationAndZoom(double zoom, double centerlon, double centerlat) {
         smc.MapSetLocationAndZoom(zoom, centerlon, centerlat);
      }

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public void MapMoveView(double dxpercent, double dypercent) {
         smc.MapMoveView(dxpercent, dypercent);
      }

      /// <summary>
      /// zum Bereich zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      public void MapZoomToRange(PointD topleft, PointD bottomright) {
         smc.MapZoomToRange(topleft, bottomright);
      }

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="MapProviderDefinitions"/>-Liste
      /// </summary>
      /// <returns></returns>
      public int MapGetActiveProviderIdx() {
         return smc.MapGetActiveProviderIdx();
      }

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="provider"></param>
      /// <returns>Anzahl der Tiles</returns>
      public int MapClearCache(GMapProvider provider = null) {
         return smc.MapClearCache(provider);
      }

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="idx">bezieht sich auf die Liste der <see cref="MapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public int MapClearCache(int idx) {
         return smc.MapClearCache(idx);
      }

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void MapClearMemoryCache() {
         smc.MapClearMemoryCache();
      }

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich
      /// </summary>
      /// <param name="minlon"></param>
      /// <param name="maxlon"></param>
      /// <param name="minlat"></param>
      /// <param name="maxlat"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersInArea(double minlon, double maxlon, double minlat, double maxlat) {
         return smc.MapGetPictureMarkersInArea(minlon, maxlon, minlat, maxlat);
      }

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich um den Client-Punkt herum
      /// </summary>
      /// <param name="localcenter"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersAround(Point localcenter, int deltax, int deltay) {
         return smc.MapGetPictureMarkersAround(localcenter, deltax, deltay);
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> MapGetGarminObjectInfos(Point ptclient, int deltax, int deltay) {
         return smc.MapGetGarminObjectInfos(ptclient, deltax, deltay);
      }

      /// <summary>
      /// zeigt einen <see cref="Track"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="posttrack">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowTrack(Track track, bool on, Track posttrack) {
         smc.MapShowTrack(track, on, posttrack);
      }

      /// <summary>
      /// zeigt alle <see cref="Track"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="on"></param>
      public void MapShowTrack(IList<Track> tracks, bool on) {
         smc.MapShowTrack(tracks, on);
      }

      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="idxlst"></param>
      public void MapShowSelectedParts(Track mastertrack, IList<int> idxlst) {
         smc.MapShowSelectedParts(mastertrack, idxlst);
      }

      /// <summary>
      /// liefert alle aktuell angezeigten Tracks
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Track> MapGetVisibleTracks(bool onlyeditable) {
         return smc.MapGetVisibleTracks(onlyeditable);
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Track"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Track"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableTrackDrawOrder(IList<Track> trackorder) {
         return smc.MapChangeEditableTrackDrawOrder(trackorder);
      }

      /// <summary>
      /// zeigt einen <see cref="Marker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="on"></param>
      /// <param name="postmarker">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowMarker(Marker marker, bool on, Marker postmarker = null) {
         smc.MapShowMarker(marker, on, postmarker);
      }

      /// <summary>
      /// zeigt alle <see cref="Marker"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="markers"></param>
      /// <param name="on"></param>
      public void MapShowMarker(IList<Marker> markers, bool on) {
         smc.MapShowMarker(markers, on);
      }

      /// <summary>
      /// zeigt einen <see cref="VisualMarker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vm"></param>
      /// <param name="on"></param>
      /// <param name="toplayer"></param>
      /// <param name="postvm">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowVisualMarker(VisualMarker vm, bool on, bool toplayer, VisualMarker postvm = null) {
         smc.MapShowVisualMarker(vm, on, toplayer, postvm);
      }

      /// <summary>
      /// liefert alle aktuell angezeigten Marker
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Marker> MapGetVisibleMarkers(bool onlyeditable) {
         return MapGetVisibleMarkers(onlyeditable);
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Marker"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Marker"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableMarkerDrawOrder(IList<Marker> markerorder) {
         return MapChangeEditableMarkerDrawOrder(markerorder);
      }

      #endregion

      public void UpdateVisualTrack(Track t) {
         t.UpdateVisualTrack(smc);
      }

      public void UpdateVisualMarker(Marker m) {
         m.UpdateVisualMarker(smc);
      }

   }
}
