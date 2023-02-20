using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using GMap.NET;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.MapProviders;
using SpecialMapCtrl;
using SpecialMapCtrl.EditHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace GpxViewer {
   /// <summary>
   /// Das Control besteht i.W. aus einem <see cref="SpecialMapCtrl"/>. Dazu kommt ein Control für den Zoom und ein weiteres zum Verschieben der Karte.
   /// I.A. sollten hier nur die Map-Elemente des <see cref="SpecialMapCtrl"/> verwendet werden.
   /// </summary>
   public partial class MapControl : UserControl {

      #region Karten-Events

      public event EventHandler<MapCtrl.TileLoadEventArgs> MapTileLoadCompleteEvent;

      public event EventHandler MapZoomChangedEvent;

      public event EventHandler<MapCtrl.MapMouseEventArgs> MapMouseEvent;

      public event EventHandler<MapCtrl.DrawExtendedEventArgs> MapDrawOnTopEvent;

      public event EventHandler<MouseEventArgs> MapTrackSearch4PolygonEvent;

      public event EventHandler<MapCtrl.TrackEventArgs> MapTrackEvent;

      public event EventHandler<MapCtrl.MarkerEventArgs> MapMarkerEvent;

      #endregion


      bool _trackBarZoomInternSet = false;

      /// <summary>
      /// min. Zoom für die Karte
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(0)]
      public int MapMinZoom {
         get => smc.SpecMapMinZoom;
         set {
            if (value <= smc.SpecMapMaxZoom) {
               smc.SpecMapMinZoom = value;
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
         get => smc.SpecMapMaxZoom;
         set {
            if (value >= smc.SpecMapMinZoom) {
               smc.SpecMapMaxZoom = value;
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
         get => smc.SpecMapZoom;
         set {
            value = Math.Min(MapMaxZoom, Math.Max(MapMinZoom, value));
            smc.SpecMapZoom = value;

            _trackBarZoomInternSet = true;
            trackBarZoom.Value = (int)Math.Round(value * 10);
            _trackBarZoomInternSet = false;
         }
      }

      /// <summary>
      /// letzte registrierte Mausposition im Karten-Control
      /// </summary>
      [Browsable(false)]
      public Point MapLastMouseLocation => smc.SpecMapLastMouseLocation;

      /// <summary>
      /// Der Maus-Button zum "Ziehen" der Karte
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(MouseButtons.Left)]
      public MouseButtons MapDragButton {
         get => smc.Map_DragButton;
         set => smc.Map_DragButton = value;
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


      public MapControl() {
         InitializeComponent();
      }

      private void MapControl_Load(object sender, EventArgs e) {
         smc.SpecMapDrawOnTop += Smc_MapPaintEvent;
         smc.SpecMapMouseEvent += Smc_MapMouseEvent;
         smc.SpecMapZoomChangedEvent += Smc_MapZoomChangedEvent;
         smc.SpecMapZoomRangeChangedEvent += Smc_MapZoomRangeChanged;
         smc.SpecMapTileLoadCompleteEvent += Smc_MapTileLoadCompleteEvent;
         smc.SpecMapMarkerEvent += Smc_MapMarkerEvent;
         smc.SpecMapTrackEvent += Smc_MapTrackEvent;
         smc.SpecMapTrackSearch4PolygonEvent += Smc_MapTrackSearch4PolygonEvent;

         moveControl1.DirectionEvent += moveControl1_DirectionEvent;

         // Dummy-Werte
         MapMinZoom = 3;
         MapMaxZoom = 24;
      }

      protected override void OnHandleDestroyed(EventArgs e) {
         base.OnHandleDestroyed(e);

         smc.SpecMapDrawOnTop -= Smc_MapPaintEvent;
         smc.SpecMapMouseEvent -= Smc_MapMouseEvent;
         smc.SpecMapZoomChangedEvent -= Smc_MapZoomChangedEvent;
         smc.SpecMapZoomRangeChangedEvent -= Smc_MapZoomRangeChanged;
         smc.SpecMapTileLoadCompleteEvent -= Smc_MapTileLoadCompleteEvent;
         smc.SpecMapMarkerEvent -= Smc_MapMarkerEvent;
         smc.SpecMapTrackEvent -= Smc_MapTrackEvent;
         smc.SpecMapTrackSearch4PolygonEvent -= Smc_MapTrackSearch4PolygonEvent;

         smc.SpecMapDrawOnTop -= Smc_MapPaintEvent;

      }

      private void MapControl_SizeChanged(object sender, EventArgs e) {
         smc.Width = MapZoomHandleVisible ?
                                    ClientSize.Width - trackBarZoom.Width :
                                    ClientSize.Width;
         trackBarZoom.Height = ClientSize.Height - moveControl1.Height;
      }

      #region Behandlung der SmallMapControl-Events (i.W. Weiterleitung nach "außen")

      private void Smc_Load(object sender, EventArgs e) { }

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

      private void Smc_MapTileLoadCompleteEvent(object sender, MapCtrl.TileLoadEventArgs e) =>
         // => ev. nach "außen" weiterleiten
         MapTileLoadCompleteEvent?.Invoke(sender, e);

      private void Smc_MapTrackEvent(object sender, MapCtrl.TrackEventArgs e) => MapTrackEvent?.Invoke(sender, e);

      private void Smc_MapTrackSearch4PolygonEvent(object sender, MouseEventArgs e) => MapTrackSearch4PolygonEvent?.Invoke(sender, e);

      private void Smc_MapMarkerEvent(object sender, MapCtrl.MarkerEventArgs e) => MapMarkerEvent?.Invoke(sender, e);

      private void Smc_MapMouseEvent(object sender, MapCtrl.MapMouseEventArgs e) => MapMouseEvent?.Invoke(sender, e);

      private void Smc_MapPaintEvent(object sender, MapCtrl.DrawExtendedEventArgs e) => MapDrawOnTopEvent?.Invoke(sender, e);

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


      public EditTrackHelper MapCreateEditTrackHelper(GpxAllExt editablegpx, Color helperLineColor, float helperLineWidth) =>
         editablegpx != null ?
                     new EditTrackHelper(smc, editablegpx, helperLineColor, helperLineWidth) :
                     null;

      public EditMarkerHelper MapCreateEditMarkerHelper(GpxAllExt editablegpx, Color helperLineColor, float helperLineWidth) =>
         editablegpx != null ?
                     new EditMarkerHelper(smc, editablegpx, helperLineColor, helperLineWidth) :
                     null;

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
      public void MapShowToolTip(ToolTip tt, string text, int clientx, int clienty) =>
         tt.Show(text,
                 smc,
                 clientx,
                 clienty);

      /// <summary>
      /// entfernt den ToolTip
      /// </summary>
      /// <param name="tt"></param>
      public void MapHideToolTip(ToolTip tt) => tt.Hide(smc);

      /// <summary>
      /// zeigt das Kontextmenü an der Position an
      /// </summary>
      /// <param name="cms"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void MapShowContextMenu(ContextMenuStrip cms, int clientx, int clienty) => cms.Show(smc, clientx, clienty);

      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <param name="withscale"></param>
      /// <returns></returns>
      public Image MapGetViewAsImage(bool withscale = true) => smc.SpecMapGetViewAsImage(withscale);

      #region Weiterleitungen an gleichnamige SmallMapControl-Properties

      /// <summary>
      /// Cache-Verzeichnis
      /// </summary>
      [Browsable(true),
       ReadOnly(true),
       Category("Map")]
      public string MapCacheLocation {
         get => smc.SpecMapCacheLocation;
         set => smc.SpecMapCacheLocation = value;
      }

      /// <summary>
      /// auch den Cache verwenden oder nur den Karten-Server (gilt global)
      /// </summary>
      [Browsable(true), Category("Map"), DefaultValue(true)]
      public static bool MapCacheIsActiv {
         get => MapCtrl.SpecMapCacheIsActiv;
         set => MapCtrl.SpecMapCacheIsActiv = value;
      }

      /// <summary>
      /// Liste der registrierten Karten-Provider
      /// </summary>
      [Browsable(false)]
      public List<MapProviderDefinition> MapProviderDefinitions => smc.SpecMapProviderDefinitions;

      /// <summary>
      /// geogr. Länge des Mittelpunktes der Karte
      /// </summary>
      [Browsable(false)]
      public double MapCenterLon => smc.SpecMapCenterLon;

      /// <summary>
      /// geogr. Breite des Mittelpunktes der Karte
      /// </summary>
      [Browsable(false)]
      public double MapCenterLat => smc.SpecMapCenterLat;

      /// <summary>
      /// Cursor der Karte
      /// </summary>
      [Browsable(true), Category("Map")]
      public Cursor MapCursor {
         get => smc.SpecMapCursor;
         set => smc.SpecMapCursor = value;
      }

      #endregion

      #region Weiterleitungen an gleichnamige SmallMapControl-Funktionen

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(int clientx, int clienty) => smc.SpecMapClient2LonLat(clientx, clienty);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(Point ptclient) => smc.SpecMapClient2LonLat(ptclient.X, ptclient.Y);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void MapLonLat2Client(double lon, double lat, out int clientx, out int clienty) => smc.SpecMapLonLat2Client(lon, lat, out clientx, out clienty);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(double lon, double lat) => smc.SpecMapLonLat2Client(new PointLatLng(lat, lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(PointLatLng ptgeo) => smc.SpecMapLonLat2Client(ptgeo);

      public Point MapLonLat2Client(Gpx.GpxWaypoint ptgeo) => smc.SpecMapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(Gpx.GpxTrackPoint ptgeo) => smc.SpecMapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// Wird gerade ein Auswahlrechteck gezeichnet?
      /// </summary>
      [Browsable(false)]
      public bool MapSelectionAreaIsStarted => smc.SpecMapSelectionAreaIsStarted;

      /// <summary>
      /// startet die Auswahl einer Fläche
      /// </summary>
      public void MapStartSelectionArea() => smc.SpecMapStartSelectionArea();

      /// <summary>
      /// liefert eine ausgewählte Fläche oder null
      /// </summary>
      /// <returns></returns>
      public Gpx.GpxBounds MapEndSelectionArea() => smc.SpecMapEndSelectionArea();


      /// <summary>
      /// set a internet proxy
      /// </summary>
      /// <param name="proxy">proxy hostname (if null or empty <see cref="System.Net.WebRequest.DefaultWebProxy"/>)</param>
      /// <param name="proxyport">proxy portnumber</param>
      /// <param name="user">username</param>
      /// <param name="password">userpassword</param>
      public static void MapSetProxy(string proxy, int proxyport, string user, string password) =>
         MapCtrl.SpecMapSetProxy(proxy, proxyport, user, password);

      /// <summary>
      /// registriert die zu verwendenden Karten-Provider in der Liste <see cref="MapProviderDefinitions"/>
      /// </summary>
      /// <param name="providernames"></param>
      /// <param name="garmindefs"></param>
      /// <param name="wmsdefs"></param>
      /// <param name="kmzdefs"></param>
      public void MapRegisterProviders(IList<string> providernames,
                                       List<MapProviderDefinition> provdefs) => smc.SpecMapRegisterProviders(providernames, provdefs);

      /// <summary>
      /// setzt den aktiven Karten-Provider
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      public void MapSetActivProvider(int idx, int demalpha, DemData dem = null) => smc.SpecMapSetActivProvider(idx, demalpha, dem);

      /// <summary>
      /// zeichnet die Karte neu
      /// </summary>
      /// <param name="reload">löst auch ein Reload aus</param>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      public void MapRefresh(bool reload, bool clearmemcache, bool clearcache) => smc.SpecMapRefresh(reload, clearmemcache, clearcache);

      public void MapCancelLoad() => smc.SpecMapCancelUnnecessaryLoadings();

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public void MapSetLocationAndZoom(double zoom, double centerlon, double centerlat) => smc.SpecMapSetLocationAndZoom(zoom, centerlon, centerlat);

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public void MapMoveView(double dxpercent, double dypercent) => smc.SpecMapMoveView(dxpercent, dypercent);

      /// <summary>
      /// zum Bereich zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      public void MapZoomToRange(PointD topleft, PointD bottomright) => smc.SpecMapZoomToRange(topleft, bottomright, false);

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="MapProviderDefinitions"/>-Liste
      /// </summary>
      /// <returns></returns>
      public int MapGetActiveProviderIdx() => smc.SpecMapGetActiveProviderIdx();

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="provider"></param>
      /// <returns>Anzahl der Tiles</returns>
      public int MapClearCache(GMapProvider provider = null) => smc.SpecMapClearCache(provider);

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="idx">bezieht sich auf die Liste der <see cref="MapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public int MapClearCache(int idx) => smc.SpecMapClearCache(idx);

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void MapClearMemoryCache() => smc.SpecMapClearMemoryCache();

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich
      /// </summary>
      /// <param name="minlon"></param>
      /// <param name="maxlon"></param>
      /// <param name="minlat"></param>
      /// <param name="maxlat"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersInArea(double minlon, double maxlon, double minlat, double maxlat) =>
         smc.SpecMapGetPictureMarkersInArea(minlon, maxlon, minlat, maxlat);

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich um den Client-Punkt herum
      /// </summary>
      /// <param name="localcenter"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersAround(Point localcenter, int deltax, int deltay) =>
         smc.SpecMapGetPictureMarkersAround(localcenter, deltax, deltay);

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> MapGetGarminObjectInfos(Point ptclient, int deltax, int deltay) =>
         smc.SpecMapGetGarminObjectInfos(ptclient, deltax, deltay);

      /// <summary>
      /// zeigt einen <see cref="Track"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="posttrack">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowTrack(Track track, bool on, Track posttrack) => smc.SpecMapShowTrack(track, on, posttrack);

      /// <summary>
      /// zeigt alle <see cref="Track"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="on"></param>
      public void MapShowTrack(IList<Track> tracks, bool on) => smc.SpecMapShowTrack(tracks, on);

      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="idxlst"></param>
      public void MapShowSelectedParts(Track mastertrack, IList<int> idxlst) => smc.SpecMapShowSelectedParts(mastertrack, idxlst);

      /// <summary>
      /// liefert alle aktuell angezeigten Tracks
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Track> MapGetVisibleTracks(bool onlyeditable) => smc.SpecMapGetVisibleTracks(onlyeditable);

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Track"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Track"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableTrackDrawOrder(IList<Track> trackorder) => smc.SpecMapChangeEditableTrackDrawOrder(trackorder);

      /// <summary>
      /// zeigt einen <see cref="Marker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="on"></param>
      /// <param name="postmarker">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowMarker(Marker marker, bool on, Marker postmarker = null) => smc.SpecMapShowMarker(marker, on, postmarker);

      /// <summary>
      /// zeigt alle <see cref="Marker"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="markers"></param>
      /// <param name="on"></param>
      public void MapShowMarker(IList<Marker> markers, bool on) => smc.SpecMapShowMarker(markers, on);

      /// <summary>
      /// zeigt einen <see cref="VisualMarker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vm"></param>
      /// <param name="on"></param>
      /// <param name="toplayer"></param>
      /// <param name="postvm">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowVisualMarker(VisualMarker vm, bool on, bool toplayer, VisualMarker postvm = null) => smc.SpecMapShowVisualMarker(vm, on, toplayer, postvm);

      /// <summary>
      /// liefert alle aktuell angezeigten Marker
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Marker> MapGetVisibleMarkers(bool onlyeditable) => MapGetVisibleMarkers(onlyeditable);

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Marker"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Marker"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableMarkerDrawOrder(IList<Marker> markerorder) => MapChangeEditableMarkerDrawOrder(markerorder);

      public List<PointD> MapGetPointsForText(string txt, int mapprovideridx = -1) {
         GMapProvider gMapProvider = mapprovideridx >= 0 ? smc.SpecMapProviderDefinitions[mapprovideridx].Provider : null;
         return smc.SpecMapGetPositionByKeywords(txt, gMapProvider as GeocodingProvider);
      }

      #endregion

      public void UpdateVisualTrack(Track t) => t.UpdateVisualTrack(smc);

      public void UpdateVisualMarker(Marker m) => m.UpdateVisualMarker(smc);

   }
}
