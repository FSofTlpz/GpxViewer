using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using GMap.NET;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.MapProviders;
#if GMAP4SKIA
using GMap.NET.Skia;
#else
using GMap.NET.WindowsForms;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl {
   /// <summary>
   /// Erweiterung von <see cref="GMapControl"/>
   /// <para>ACHTUNG: extern nur Properties und Funktionen mit Spec... verwenden</para>
   /// </summary>
   public partial class SpecialMapCtrl : GMapControl {

      #region eigene Events

      public class TileLoadEventArgs {

         /// <summary>
         /// complete (or start)
         /// </summary>
         public readonly bool Complete;

         /// <summary>
         /// Milliseconds
         /// </summary>
         public readonly long Ms;

         public TileLoadEventArgs(bool complete, long ms = 0) {
            Complete = complete;
            Ms = ms;
         }
      }

      event EventHandler<TileLoadEventArgs> mapTileLoadCompleteEvent;                // für threadsichere Weitergabe

      delegate void TileLoadCompleteEventDelegate(object sender, TileLoadEventArgs e);
      TileLoadCompleteEventDelegate myMapWrapper_TileLoadCompleteEventDelegate;              // für threadsichere Weitergabe

      public event EventHandler<TileLoadEventArgs> SpecMapTileLoadCompleteEvent;

      public event EventHandler SpecMapZoomChangedEvent;

      public event EventHandler<DrawExtendedEventArgs> SpecMapDrawOnTop;

      public event EventHandler SpecMapZoomRangeChangedEvent;


      public class MapMouseEventArgs : MouseEventArgs {

         public enum EventType {
            Move,
            Enter,
            Leave,
            Click,
         }

         public EventType Eventtype;
         public double Lon, Lat;
         public bool IsHandled;

         public MapMouseEventArgs(EventType eventtype, MouseButtons buttons, int clicks, int x, int y, int delta, double lon, double lat) :
            base(buttons, clicks, x, y, delta) {
            Eventtype = eventtype;
            Lon = lon;
            Lat = lat;
            IsHandled = false;
         }

         public MapMouseEventArgs(EventType eventtype, MouseButtons buttons, int x, int y, double lon, double lat) :
            this(eventtype, buttons, 0, x, y, 0, lon, lat) { }

         public MapMouseEventArgs(EventType eventtype, MouseButtons buttons, int x, int y) :
            this(eventtype, buttons, 0, x, y, 0, 0, 0) { }

         /// <summary>
         /// i.A. für "Leave"
         /// </summary>
         /// <param name="eventtype"></param>
         public MapMouseEventArgs(EventType eventtype) :
            this(eventtype, MouseButtons.None, 0, 0, 0, 0, 0, 0) { }

      }

      public event EventHandler<MapMouseEventArgs> SpecMapMouseEvent;

      public event EventHandler<MouseEventArgs> SpecMapTrackSearch4PolygonEvent;


      public class TrackEventArgs : MapMouseEventArgs {

         public Track Track;

         public TrackEventArgs(Track track, EventType eventtype) :
            base(eventtype) {
            Track = track;
         }

         public TrackEventArgs(Track track, EventType eventtype, MouseButtons buttons, int x, int y, double lon, double lat) :
            base(eventtype, buttons, 0, x, y, 0, lon, lat) {
            Track = track;
         }

      }

      public event EventHandler<TrackEventArgs> SpecMapTrackEvent;


      public class MarkerEventArgs : MapMouseEventArgs {

         public Marker Marker;

         public MarkerEventArgs(Marker marker, EventType eventtype) :
            base(eventtype) {
            Marker = marker;
         }

         public MarkerEventArgs(Marker marker, EventType eventtype, MouseButtons buttons, int x, int y, double lon, double lat) :
            base(eventtype, buttons, 0, x, y, 0, lon, lat) {
            Marker = marker;
         }

      }

      public event EventHandler<MarkerEventArgs> SpecMapMarkerEvent;

      #endregion


      /// <summary>
      /// Liste der registrierten Karten-Provider
      /// </summary>
      public List<MapProviderDefinition> SpecMapProviderDefinitions = new List<MapProviderDefinition>();

      /// <summary>
      /// akt. Kartenindex in der Liste <see cref="SpecMapProviderDefinitions"/>
      /// </summary>
      public int SpecMapActualMapIdx { get; protected set; } = -1;

      /// <summary>
      /// letzte registrierte Mausposition im Karten-Control
      /// </summary>
      public Point SpecMapLastMouseLocation { get; protected set; } = Point.Empty;

      /// <summary>
      /// auch den Cache verwenden oder nur den Karten-Server (gilt global)
      /// </summary>
      public static bool SpecMapCacheIsActiv {
         get => GMaps.Instance.Mode != AccessMode.ServerOnly;
         set => GMaps.Instance.Mode = value ?
                                          AccessMode.ServerAndCache :
                                          AccessMode.ServerOnly;
      }

      public string SpecMapCacheLocation {
         get => Map_CacheLocation;
         set => Map_CacheLocation = value;
      }

      /// <summary>
      /// min. Zoom für die Karte
      /// </summary>
      public int SpecMapMinZoom {
         get => Map_MinZoom;
         set {
            if (Map_MinZoom != value) {
               Map_MinZoom = value;
               SpecMapZoomRangeChangedEvent?.Invoke(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// max. Zoom für die Karte
      /// </summary>
      public int SpecMapMaxZoom {
         get => Map_MaxZoom;
         set {
            if (Map_MaxZoom != value) {
               Map_MaxZoom = value;
               SpecMapZoomRangeChangedEvent?.Invoke(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// Zoom für die Karte (intern int)
      /// </summary>
      public double SpecMapZoom {
         get => Map_Zoom;
         set {
            if (value != Map_Zoom) {
               Map_Zoom = Math.Max(Map_MinZoom, Math.Min(Map_MaxZoom, value));
               // Bei einem nicht-ganzzahligen Zoom löst der Core und GMapControl KEIN ZoomChangedEvent aus.
               // Deshalb bei Bedarf an dieser Stelle:
               if ((SpecMapZoom % 1) != 0)
                  SpecMapZoomChangedEvent?.Invoke(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// geogr. Länge des Mittelpunktes der Karte
      /// </summary>
      public double SpecMapCenterLon => Map_Position.Lng;

      /// <summary>
      /// geogr. Breite des Mittelpunktes der Karte
      /// </summary>
      public double SpecMapCenterLat => Map_Position.Lat;

#if GMAP4SKIA
      Cursor Cursor = null;
#endif

      /// <summary>
      /// Cursor der Karte
      /// </summary>
      public Cursor SpecMapCursor {
         get => Cursor;
         set {
            if (Cursor != value)
               Cursor = value;
         }
      }

      /* Mehrfache Auswertungen z.B. eines Mausklicks können nicht so einfach verhindert werden. Wenn z.B. der Mausklick auf einen Marker und einen Track erfolgt, erhalten BEIDE diesen
       * Klick. Andererseits wird der Klick NICHT für weitere Marker/Tracks ausgewertet.
       */
      /// <summary>
      /// Pos. des letzten ausgewerteten Klicks
      /// </summary>
      Point ptUsedLastClick = new Point();

      /// <summary>
      /// Maßstab für das <see cref="SpecialMapCtrl"/>
      /// </summary>
      Scale4Map scale;

      /// <summary>
      /// zur internen Erzeugung der Garminkarten und zur (externen) Objektsuche
      /// </summary>
      GarminImageCreator.ImageCreator garminImageCreator;

      /// <summary>
      /// Overlay für die GPX-Daten
      /// </summary>
      readonly GMapOverlay gpxReadOnlyOverlay = new GMapOverlay("GPXro");
      readonly GMapOverlay gpxOverlay = new GMapOverlay("GPX");
      readonly GMapOverlay gpxSelectedPartsOverlay = new GMapOverlay("GPXselparts");

      /// <summary>
      /// Mittelpunkt anzeigen
      /// </summary>
      public bool SpecMapShowCenter { get; set; } = false;


      public SpecialMapCtrl() {
         Map_RetryLoadTile = 0;
         Map_FillEmptyTiles = false;
         //RetryLoadTile


      }

      #region interne Konvertierungsfunktionen

      void client2LonLat(int clientx, int clientxy, out double lon, out double lat) {
         PointD ptgeo = client2LonLat(clientx, clientxy);
         lon = ptgeo.X;
         lat = ptgeo.Y;
      }

      PointD client2LonLat(int clientx, int clienty) {
         return SpecMapPointLatLng2PointD(Map_FromLocalToLatLng(clientx, clienty));
      }

      #endregion

      /// <summary>
      /// hier werden einige <see cref="GMapControl"/>-Eigenschaften direkt gesetzt und einige <see cref="GMapControl"/>-Events "angezapft"
      /// </summary>
      /// <param name="e"></param>
      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);

         // Original-Events auf evGMapControl_On-Funktionen im anderen Teil umleiten

         MouseDown += evGMapControl_OnMouseDown;
         MouseMove += evGMapControl_OnMouseMove;
         MouseUp += evGMapControl_OnMouseUp;
         MouseLeave += evGMapControl_OnMouseLeave;
         MouseClick += evGMapControl_OnMouseClick;

         OnMapZoomChanged += evGMapControl_OnMapZoomChanged;

         OnMapTileLoadStart += evGMapControl_OnTileLoadStart;
         OnMapTileLoadComplete += evGMapControl_OnTileLoadComplete;

         OnMapMarkerClick += evGMapControl_OnMarkerClick;
         OnMapMarkerEnter += evGMapControl_OnMarkerEnter;
         OnMapMarkerLeave += evGMapControl_OnMarkerLeave;

         OnMapTrackClick += evGMapControl_OnRouteClick;
         OnMapTrackEnter += evGMapControl_OnRouteEnter;
         OnMapTrackLeave += evGMapControl_OnRouteLeave;

#if !GMAP4SKIA
         //MapBearing = 0F;
         Map_MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
#endif

         Map_CanDragMap = true;
         Map_LevelsKeepInMemory = 5;
         Map_MarkersEnabled = true;
         Map_PolygonsEnabled = true;
         Map_RetryLoadTile = 0;
         Map_FillEmptyTiles = false;      // keinen niedrigeren Zoom verwenden (notwendig für korrekten Abbruch bei Garmin-Provider)
         Map_TracksEnabled = true;
         Map_ScaleMode = ScaleModes.Fractional;
         Map_SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);

         Map_DeviceZoom = 1F;

         evGMapControl_OnLoad();
      }

      #region Behandlung der GMapControl-Events (d.h. deren Weiterleitung)

      /// <summary>
      /// wird NACH OnLoad() ausgeführt
      /// </summary>
      void evGMapControl_OnLoad() {
         //  vor .NET 4.5
         ServicePointManager.Expect100Continue = true;
         ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
         //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //SecurityProtocolType.SystemDefault;

         Map_Provider = EmptyProvider.Instance;   // gMapProviders[startprovideridx];
         Map_EmptyTileText = "no data";                                    // Hinweistext für "Tile ohne Daten"

         SpecMapMinZoom = 0;
         SpecMapMaxZoom = 24;
         SpecMapZoom = 20;

#if DEBUG
         Map_ShowTileGridLines = true;
#else
         Map_ShowTileGridLines = false;
#endif
         Map_EmptyMapBackgroundColor = Color.LightGray;      // Tile (noch) ohne Daten
         Map_EmptyTileText = "keine Daten";             // Hinweistext für "Tile ohne Daten"
         Map_EmptyTileColor = Color.DarkGray;           // Tile (endgültig) ohne Daten

         map_Overlays.Add(gpxReadOnlyOverlay);
         gpxReadOnlyOverlay.IsVisibile = true;

         map_Overlays.Add(gpxOverlay);
         gpxOverlay.IsVisibile = true;

         map_Overlays.Add(gpxSelectedPartsOverlay);
         gpxSelectedPartsOverlay.IsVisibile = true;

         myMapWrapper_TileLoadCompleteEventDelegate = new TileLoadCompleteEventDelegate(ts_MapWrapper_TileLoadCompleteEvent);
         mapTileLoadCompleteEvent += control_TileLoadCompleteEvent;

         scale = new Scale4Map(this);
      }

      protected override void OnMapDrawScale(DrawExtendedEventArgs e) {
         scale?.Draw(e.Graphics, (float)scale4device);
      }

      protected override void OnMapDrawCenter(DrawExtendedEventArgs e) {
         if (SpecMapShowCenter) {
#if GMAP4SKIA
            float ro = Math.Min(Width, Height) / 30;
            float ri = ro / 4;
            float cx = Width / 2;
            float cy = Height / 2;
            float left = cx - ro;
            float right = cx + ro;
            float top = cy - ro;
            float bottom = cy + ro;

            e.Graphics.DrawLine(map_CenterPen,
                                left,
                                cy,
                                cx - ri,
                                cy);
            e.Graphics.DrawLine(map_CenterPen,
                                cx + ri,
                                cy,
                                right,
                                cy);
            e.Graphics.DrawLine(map_CenterPen,
                                cx,
                                top,
                                cx,
                                cy - ri);
            e.Graphics.DrawLine(map_CenterPen,
                                cx,
                                cy + ri,
                                cx,
                                bottom);
            e.Graphics.DrawEllipse(map_CenterPen, left, top, 2 * ro, 2 * ro);
#endif
         }
      }

      protected override void OnMapDrawOnTop(DrawExtendedEventArgs e) {
         SpecMapDrawOnTop?.Invoke(this, e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void evGMapControl_OnMouseDown(object sender, MouseEventArgs e) {
         SpecMapSetAreaSelectionStartPoint(e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void evGMapControl_OnMouseMove(object sender, MouseEventArgs e) {
         SpecMapDoMouseMove(e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void evGMapControl_OnMouseUp(object sender, MouseEventArgs e) {
         SpecMapSetAreaSelectionEndPoint(e);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void evGMapControl_OnMouseClick(object sender, MouseEventArgs e) {
         SpecMapDoMouseClick(e);
      }

      /// <summary>
      /// die Maus verläßt den Bereich der Karte
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void evGMapControl_OnMouseLeave(object sender, EventArgs e) {
         SpecMapMouseEvent?.Invoke(this, new MapMouseEventArgs(MapMouseEventArgs.EventType.Leave, MouseButtons.None, 0, 0, 0, 0, 0, 0));
      }

      void evGMapControl_OnMapZoomChanged(object sender, EventArgs e) {
         SpecMapZoomChangedEvent?.Invoke(this, new EventArgs());
      }

      void evGMapControl_OnTileLoadStart(object sender, EventArgs e) {
         //TileLoadCompleteEvent?.Invoke(this, new TileLoadCompleteEventArgs(false));
         mapTileLoadCompleteEvent?.BeginInvoke(this, new TileLoadEventArgs(false), null, null);     // asynchrone, threadsichere Weitergabe
      }

      void evGMapControl_OnTileLoadComplete(object sender, GMapControl.TileLoadCompleteEventArgs e) {
         //TileLoadCompleteEvent?.Invoke(this, new TileLoadCompleteEventArgs(true, ElapsedMilliseconds));
         mapTileLoadCompleteEvent?.BeginInvoke(this, new TileLoadEventArgs(true, e.ElapsedMilliseconds), null, null);      // asynchrone, threadsichere Weitergabe
      }

      void ts_MapWrapper_TileLoadCompleteEvent(object sender, TileLoadEventArgs e) {
         SpecMapTileLoadCompleteEvent?.Invoke(sender, e);
      }

      void control_TileLoadCompleteEvent(object sender, TileLoadEventArgs e) {
#if GMAP4SKIA
         myMapWrapper_TileLoadCompleteEventDelegate(this, e);
#else
         // threadsicher weiterleiten
         if ((sender as Control).IsHandleCreated)
            Invoke(myMapWrapper_TileLoadCompleteEventDelegate, new object[] { sender, e });

         // Zusammenfassung:
         //     Führt mit der angegebenen Argumentliste den angegebenen Delegaten für den Thread aus, der das dem Steuerelement zugrunde liegende Fensterhandle besitzt.
         //
         // Parameter:
         //   method:
         //     Ein Delegat einer Methode, der Parameter derselben Anzahl und desselben Typs der im args-Parameter enthaltenen Parameter annimmt.
         //
         //   args:
         //     Ein Array von Objekten, die als Argumente an die angegebene Methode übergeben werden sollen. Dieser Parameter kann null sein, wenn die Methode keine Argumente annimmt.
         //
         // Rückgabewerte:
         //     Ein System.Object, das den Rückgabewert des aufgerufenen Delegaten enthält, oder null, wenn der Delegat keinen Wert zurückgibt.
         //public object Invoke(Delegate method, params object[] args);
#endif
      }

      #region Maus-Events für Marker (löst das MapMarkerEvent aus)

      void evGMapControl_OnMarkerEnter(object sender, GMapMarkerEventArgs e) {
         if (e.Marker != null &&
             e.Marker is VisualMarker)
            SpecMapMarkerEvent?.Invoke(this, new MarkerEventArgs((e.Marker as VisualMarker).RealMarker, MapMouseEventArgs.EventType.Enter));
      }

      void evGMapControl_OnMarkerLeave(object sender, GMapMarkerEventArgs e) {
         if (e.Marker != null &&
             e.Marker is VisualMarker)
            SpecMapMarkerEvent?.Invoke(this, new MarkerEventArgs((e.Marker as VisualMarker).RealMarker, MapMouseEventArgs.EventType.Leave));
      }

      void evGMapControl_OnMarkerClick(object sender, GMapMarkerEventArgs e) {
         if ((ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
              ptUsedLastClick != e.Mea.Location) &&
             e.Marker is VisualMarker) {
            client2LonLat(e.Mea.X, e.Mea.Y, out double lon, out double lat);
            MarkerEventArgs me = new MarkerEventArgs((e.Marker as VisualMarker).RealMarker,
                                                     MapMouseEventArgs.EventType.Click,
                                                     e.Mea.Button,
                                                     e.Mea.X,
                                                     e.Mea.Y,
                                                     lon,
                                                     lat);
            SpecMapMarkerEvent?.Invoke(this, me);
            if (me.IsHandled)
               ptUsedLastClick = e.Mea.Location;
         }
      }

      #endregion

      #region Maus-Events für Tracks (löst das MapTrackEvent aus)

      void evGMapControl_OnRouteEnter(object sender, GMapTrackEventArgs e) {
         if (e.Track is VisualTrack) {
            Track track = (e.Track as VisualTrack).RealTrack;
            if (track != null)
               SpecMapTrackEvent?.Invoke(this, new TrackEventArgs(track, MapMouseEventArgs.EventType.Enter));
         }
      }

      void evGMapControl_OnRouteLeave(object sender, GMapTrackEventArgs e) {
         if (e.Track is VisualTrack) {
            Track track = (e.Track as VisualTrack).RealTrack;
            if (track != null)
               SpecMapTrackEvent?.Invoke(this, new TrackEventArgs(track, MapMouseEventArgs.EventType.Leave));
         }
      }

      void evGMapControl_OnRouteClick(object sender, GMapTrackEventArgs e) {
         if ((ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
              ptUsedLastClick != e.Mea.Location) &&
             e.Track is VisualTrack) {
            Track track = (e.Track as VisualTrack).RealTrack;
            if (track != null) {
               client2LonLat(e.Mea.X, e.Mea.Y, out double lon, out double lat);
               TrackEventArgs te = new TrackEventArgs(track,
                                                      MapMouseEventArgs.EventType.Click,
                                                      e.Mea.Button,
                                                      e.Mea.X,
                                                      e.Mea.Y,
                                                      lon,
                                                      lat);
               SpecMapTrackEvent?.Invoke(this, te);
               if (te.IsHandled)
                  ptUsedLastClick = e.Mea.Location;
            }
         }
      }

      #endregion

      #endregion

      void collectionInsert<T>(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<T> collection, T item, int idx) {
         collection.Add(item);
         if (0 <= idx && idx < collection.Count - 1)
            collection.Move(collection.Count - 1, idx);
      }

      int collectionIndexOf<T>(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<T> collection, T item) {
         if (item != null)
            for (int idx = 0; idx < collection.Count; idx++)
               if (collection[idx].Equals(item))
                  return idx;
         return -1;
      }

      #region Konvertierungfunktionen

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clientxy"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void SpecMapClient2LonLat(int clientx, int clientxy, out double lon, out double lat) => client2LonLat(clientx, clientxy, out lon, out lat);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <returns></returns>
      public PointD SpecMapClient2LonLat(int clientx, int clienty) => client2LonLat(clientx, clienty);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD SpecMapClient2LonLat(Point ptclient) => SpecMapClient2LonLat(ptclient.X, ptclient.Y);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD SpecMapClient2LonLat(GPoint ptclient) => SpecMapClient2LonLat((int)ptclient.X, (int)ptclient.Y);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void SpecMapLonLat2Client(double lon, double lat, out int clientx, out int clienty) => SpecMapLonLat2Client(lon, lat, out clientx, out clienty);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public Point SpecMapLonLat2Client(double lon, double lat) => SpecMapLonLat2Client(new PointLatLng(lat, lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point SpecMapLonLat2Client(Gpx.GpxTrackPoint ptgeo) => SpecMapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point SpecMapLonLat2Client(Gpx.GpxWaypoint ptgeo) => SpecMapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point SpecMapLonLat2Client(FSofTUtils.Geometry.PointD ptgeo) => SpecMapLonLat2Client(new PointLatLng(ptgeo.Y, ptgeo.X));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point SpecMapLonLat2Client(PointLatLng ptgeo) => convert2Point(Map_FromLatLngToLocal(ptgeo));

      public static PointD SpecMapPointLatLng2PointD(PointLatLng ptgeo) => new PointD(ptgeo.Lng, ptgeo.Lat);

      //public static PoorGpx.GpxTrackPoint MapPointLatLng2GpxTrackPoint(PointLatLng ptgeo) {
      //   return new PoorGpx.GpxTrackPoint(ptgeo.Lng, ptgeo.Lat);
      //}

      //public static PointLatLng MapPointD2PointLatLng(FSofTUtils.PointD ptgeo) {
      //   return new PointLatLng(ptgeo.Y, ptgeo.X);
      //}

      //public static PoorGpx.GpxTrackPoint MapPointD2GpxTrackPoint(FSofTUtils.PointD ptgeo) {
      //   return new PoorGpx.GpxTrackPoint(ptgeo.X, ptgeo.Y);
      //}

      //public static FSofTUtils.PointD MapGpxTrackPoint2PointD(PoorGpx.GpxTrackPoint ptgeo) {
      //   return new FSofTUtils.PointD(ptgeo.Lon, ptgeo.Lat);
      //}

      //public static PointLatLng MapGpxTrackPoint2PointLatLng(PoorGpx.GpxTrackPoint ptgeo) {
      //   return new PointLatLng(ptgeo.Lat, ptgeo.Lon);
      //}

      Point convert2Point(GPoint pt) => new Point((int)pt.X, (int)pt.Y);

      #endregion

      #region Handling Auswahlrechteck

      Cursor cursorOrgOnSelection = null;
      PointLatLng startPointSelectionArea = PointLatLng.Empty;
      GMapPolygon polySelection = null;

      /// <summary>
      /// am Clientpunkt beginnt die Auswahl der Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void SpecMapSetAreaSelectionStartPoint(MouseEventArgs e) {
         if (SpecMapSelectionAreaIsStarted) {
            startPointSelectionArea = Map_FromLocalToLatLng(e.X, e.Y);
            polySelection = buildSelectionRectangle(startPointSelectionArea, startPointSelectionArea);
            gpxReadOnlyOverlay.Polygons.Add(polySelection);
         }
      }

      /// <summary>
      /// am Clientpunkt ended die Auswahl des Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void SpecMapSetAreaSelectionEndPoint(MouseEventArgs e) {
         if (SpecMapSelectionAreaIsStarted)
            SpecMapTrackSearch4PolygonEvent?.Invoke(this, e); // Ende der Eingabe simulieren
      }

      /// <summary>
      /// Wird gerade ein Auswahlrechteck gezeichnet? (mit <see cref="SpecMapStartSelectionArea"/>() gestartet)
      /// </summary>
      public bool SpecMapSelectionAreaIsStarted { get; protected set; } = false;

      /// <summary>
      /// merkt sich den Originalbutton während der Auswahl eines Auswahlrechtecks
      /// </summary>
      MouseButtons orgMapDragButton;

      /// <summary>
      /// startet die Auswahl einer Fläche
      /// </summary>
      public void SpecMapStartSelectionArea() {
         SpecMapSelectionAreaIsStarted = true;
         cursorOrgOnSelection = Cursor;
#if !GMAP4SKIA
         Cursor = Cursors.Cross;
#endif
         startPointSelectionArea = GMap.NET.PointLatLng.Empty;
         // den Dragbutton für die Maus merken und deaktivieren
         orgMapDragButton = Map_DragButton;
         Map_DragButton = MouseButtons.None;
      }

      /// <summary>
      /// liefert eine ausgewählte Fläche oder null
      /// </summary>
      /// <returns></returns>
      public Gpx.GpxBounds SpecMapEndSelectionArea() {
         SpecMapSelectionAreaIsStarted = false;
         Cursor = cursorOrgOnSelection;
         Map_DragButton = orgMapDragButton;         // den Dragbutton für die Maus wieder aktivieren
         Gpx.GpxBounds bounds = null;
         if (polySelection != null) {
            gpxReadOnlyOverlay.Polygons.Remove(polySelection);
            if (polySelection.Points[0].Lng != polySelection.Points[2].Lng &&
                polySelection.Points[0].Lat != polySelection.Points[2].Lat) // Ex. eine Fläche?
               bounds = new Gpx.GpxBounds(Math.Min(polySelection.Points[0].Lat, polySelection.Points[2].Lat),
                                          Math.Max(polySelection.Points[0].Lat, polySelection.Points[2].Lat),
                                          Math.Min(polySelection.Points[0].Lng, polySelection.Points[2].Lng),
                                          Math.Max(polySelection.Points[0].Lng, polySelection.Points[2].Lng));
            polySelection = null;
         }
         return bounds;
      }

      GMapPolygon buildSelectionRectangle(PointLatLng start, PointLatLng end) {
         return new GMapPolygon(new List<PointLatLng>() { start,
                                                          new PointLatLng(start.Lat, end.Lng),
                                                          end,
                                                          new PointLatLng(end.Lat, start.Lng),
                                                          start },
                                "polySelection") {
            IsVisible = true,
            Stroke = new Pen(Color.LightGray, 0),
            Fill = new SolidBrush(Color.FromArgb(40, Color.Black)),
         };
      }

      #endregion

      /// <summary>
      /// set a internet proxy
      /// </summary>
      /// <param name="proxy">proxy hostname (if null or empty <see cref="System.Net.WebRequest.DefaultWebProxy"/>)</param>
      /// <param name="proxyport">proxy portnumber</param>
      /// <param name="user">username</param>
      /// <param name="password">userpassword</param>
      public static void SpecMapSetProxy(string proxy, int proxyport, string user, string password) {
         if (string.IsNullOrEmpty(proxy))
            GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
         else {
            GMapProvider.IsSocksProxy = true;
            GMapProvider.WebProxy = new WebProxy(proxy, proxyport) {
               Credentials = new System.Net.NetworkCredential(user, password)
            };
         }
      }

      /// <summary>
      /// registriert die zu verwendenden Karten-Provider in der Liste <see cref="SpecMapProviderDefinitions"/>
      /// </summary>
      /// <param name="providernames"></param>
      /// <param name="garmindefs"></param>
      /// <param name="wmsdefs"></param>
      /// <param name="kmzdefs"></param>
      public void SpecMapRegisterProviders(IList<string> providernames,
                                           List<MapProviderDefinition> provdefs) {
         SpecMapProviderDefinitions.Clear();
         Map_Provider = GMapProviders.EmptyProvider;

         if (providernames != null)
            for (int i = 0; i < providernames.Count; i++) {
               MapProviderDefinition def = provdefs[i];

               if (providernames[i] == GarminProvider.Instance.Name &&
                   def is GarminProvider.GarminMapDefinitionData) {

                  def.Provider = GarminProvider.Instance;
                  SpecMapProviderDefinitions.Add(def as GarminProvider.GarminMapDefinitionData);

               } else if (providernames[i] == GarminKmzProvider.Instance.Name &&
                          def is GarminKmzProvider.KmzMapDefinition) {

                  def.Provider = GarminKmzProvider.Instance;
                  SpecMapProviderDefinitions.Add(def as GarminKmzProvider.KmzMapDefinition);

               } else if (providernames[i] == WMSProvider.Instance.Name &&
                          def is WMSProvider.WMSMapDefinition) {

                  def.Provider = WMSProvider.Instance;
                  SpecMapProviderDefinitions.Add(def as WMSProvider.WMSMapDefinition);

               } else {

                  for (int p = 0; p < GMapProviders.List.Count; p++) {
                     if (GMapProviders.List[p].Name == providernames[i]) { // Provider ist vorhanden

                        def.Provider = GMapProviders.List[p];
                        SpecMapProviderDefinitions.Add(def);

                     }
                  }

               }
            }

         if (SpecMapProviderDefinitions.Count == 0)
            return;
         SpecMapActualMapIdx = -1;
         //SpecMapProviderDefinitions.Count > 0 ?
         //               Math.Max(0, Math.Min(SpecMapActualMapIdx, SpecMapProviderDefinitions.Count - 1)) :
         //               -1;
      }

      /// <summary>
      /// setzt den aktiven Karten-Provider entsprechend dem Index und der Liste der <see cref="GarminProvider.GarminMapDefinitionData"/>
      /// </summary>
      /// <param name="idx">Index für die <see cref="SpecMapProviderDefinitions"/></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      public void SpecMapSetActivProvider(int idx, int demalpha, DemData dem = null) {
         if (SpecMapProviderDefinitions.Count == 0)
            return;
         SpecMapActualMapIdx = Math.Max(0, Math.Min(idx, SpecMapProviderDefinitions.Count - 1));

         MapProviderDefinition def = SpecMapProviderDefinitions[SpecMapActualMapIdx];
         GMapProvider newprov = def.Provider;

         gpxReadOnlyOverlay.IsVisibile = gpxOverlay.IsVisibile = false;

         // ev. Zoom behandeln
         double newzoom = -1;
         if (Map_Zoom < def.MinZoom || def.MaxZoom < Map_Zoom) {
            newzoom = Math.Min(Math.Max(Map_Zoom, def.MinZoom), def.MaxZoom);
         }

         if (newprov is GMapProviderWithHillshade) {
            (newprov as GMapProviderWithHillshade).DEM = dem;
            (newprov as GMapProviderWithHillshade).Alpha = demalpha;
         }

         if (newprov is GarminProvider) {

            if (garminImageCreator == null)
               garminImageCreator = new GarminImageCreator.ImageCreator();

            GarminProvider.GarminMapDefinitionData gdef = def as GarminProvider.GarminMapDefinitionData;
            (newprov as GarminProvider).ChangeDbId(GarminProvider.StandardDbId + gdef.DbIdDelta);

            List<GarminImageCreator.GarminMapData> mapdata = new List<GarminImageCreator.GarminMapData>();
            for (int i = 0; i < gdef.TDBfile.Count && i < gdef.TYPfile.Count; i++) {
               mapdata.Add(new GarminImageCreator.GarminMapData(gdef.TDBfile[i],
                                                                gdef.TYPfile[i],
#if GMAP4SKIA
                                                                "StdFont",
#else
                                                                "Arial",
#endif
                                                                gdef.TextFactor,
                                                                gdef.LineFactor,
                                                                gdef.SymbolFactor));
            }

            garminImageCreator.SetGarminMapDefs(mapdata);
            (newprov as GarminProvider).GarminImageCreator = garminImageCreator;      // hier werden die Daten im Provider indirekt über den ImageCreator gesetzt

         } else if (newprov is WMSProvider) {

            (newprov as WMSProvider).ChangeDbId(WMSProvider.StandardDbId + (def as WMSProvider.WMSMapDefinition).DbIdDelta);
            (newprov as WMSProvider).SetDef(def as WMSProvider.WMSMapDefinition);

         } else if (newprov is GarminKmzProvider) {

            (newprov as GarminKmzProvider).ChangeDbId(GarminKmzProvider.StandardDbId + (def as GarminKmzProvider.KmzMapDefinition).DbIdDelta);
            (newprov as GarminKmzProvider).SetDef(def as GarminKmzProvider.KmzMapDefinition);

         }

         // jetzt wird der neue Provider und ev. auch der Zoom gesetzt
         Map_DeviceZoom = 1;
         if (def.Zoom4Display != 1)
            Map_DeviceZoom = (float)def.Zoom4Display;

         Map_Provider = newprov;
         if (newzoom >= 0)
            SpecMapZoom = newzoom;
         SpecMapMinZoom = def.MinZoom;
         SpecMapMaxZoom = def.MaxZoom;
         SpecMapRefresh(true, true, false);

         gpxReadOnlyOverlay.IsVisibile = gpxOverlay.IsVisibile = true; // ohne false/true-Wechsel passt die Darstellung des Overlays manchmal nicht zur Karte
      }

      public Task SpecMapSetActivProviderAsync(int idx, int demalpha, DemData dem = null) {
         return Task.Run(() => {
            SpecMapSetActivProvider(idx, demalpha, dem);
         });
      }

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="SpecMapProviderDefinitions"/>-Liste 
      /// (ABER NICHT DEN KARTENINDEX wenn z.B. 2x der Garminprovider verwendet wird)
      /// </summary>
      /// <returns></returns>
      public int SpecMapGetActiveProviderIdx() {
         if (Map_Provider != null &&
             SpecMapProviderDefinitions != null)
            for (int i = 0; i < SpecMapProviderDefinitions.Count; i++) {
               if (Map_Provider.Equals(SpecMapProviderDefinitions[i].Provider))
                  return i;
            }
         return -1;
      }

      /// <summary>
      /// zeichnet die Karte neu
      /// </summary>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      public void SpecMapRefresh(bool reload, bool clearmemcache, bool clearcache) {
         if (clearmemcache)
            map_Manager.MemoryCache.Clear(); // Die Tiles in diesem Cache haben KEINE DbId!

         if (clearcache) {
            if (map_Manager.PrimaryCache != null) {
               map_Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, Map_Provider.DbId);
            }
            if (map_Manager.SecondaryCache != null) {
               map_Manager.SecondaryCache.DeleteOlderThan(DateTime.Now, Map_Provider.DbId);
            }
         }

         if (reload)
            Map_Reload();

         Map_Refresh();
      }


      /// <summary>
      /// Anzahl der Tiles die noch in der Warteschlange stehen
      /// </summary>
      /// <returns></returns>
      public int SpecMapWaitingTasks() => Map_WaitingTasks();

      /// <summary>
      /// Warteschlange der Tiles wird geleert
      /// </summary>
      public void SpecMapClearWaitingTaskList() => Map_ClearWaitingTaskList();

      /// <summary>
      /// es wird versucht, die Tile-Erzeugung abzubrechen (kann nur für "lokale" Provider fkt.)
      /// <para>(d.h., die überschriebene Methode <see cref="GMapProvider.GetTileImage(GPoint, int)"/> wird nach Möglichkeit abgebrochen)</para>
      /// </summary>
      public void SpecMapCancelTileBuilds() {
         GarminProvider.CancelGetTileImage();


      }


      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public void SpecMapSetLocationAndZoom(double zoom, double centerlon, double centerlat) {
         Map_Position = new PointLatLng(centerlat, centerlon);
         SpecMapZoom = zoom;
      }

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public void SpecMapMoveView(double dxpercent, double dypercent) {
         Map_Position = new PointLatLng(Map_Position.Lat + Map_ViewArea.HeightLat * dypercent,
                                       Map_Position.Lng + Map_ViewArea.WidthLng * dxpercent);
      }

      /// <summary>
      /// zum Bereich zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      /// <param name="fractionalzoom">wenn false, dann nur Anpassung an ganzzahligen Zoom</param>
      public void SpecMapZoomToRange(PointD topleft, PointD bottomright, bool fractionalzoom) {
         Map_SetZoomToFitRect(new RectLatLng(topleft.Y,
                                            topleft.X,
                                            Math.Abs(topleft.X - bottomright.X),
                                            Math.Abs(topleft.Y - bottomright.Y))); // Ecke links-oben, Breite, Höhe
         if (fractionalzoom &&
             scale4device != 1) {
            Point ptTopLeft = SpecMapLonLat2Client(topleft);
            PointD ptEdgeTopLeft = SpecMapClient2LonLat(0, 0);

            if (ptTopLeft.X < 0 ||
                ptTopLeft.Y < 0) {
               double corrx = (SpecMapCenterLon - topleft.X) / (SpecMapCenterLon - ptEdgeTopLeft.X);
               double corry = (topleft.Y - SpecMapCenterLat) / (ptEdgeTopLeft.Y - SpecMapCenterLat);
               double corr = Math.Max(corrx, corry);                    // linearer Korrekturfaktor
               SpecMapZoom = SpecMapZoom - Math.Log(corr, 2.0) - .1;
            }
         }
      }

#if GMAP4SKIA
      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <returns></returns>
      public Bitmap SpecMapGetViewAsImage(bool withscale = true) {
         Bitmap img = MapToImage();
         if (withscale && scale != null) {
            Graphics g = Graphics.FromImage(img);
            scale.Draw(g);
         }
         return img;
      }
#else
      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <returns></returns>
      public Image SpecMapGetViewAsImage(bool withscale = true) {
         Image img = MapToImage();
         if (withscale && scale != null) {
            Graphics g = Graphics.FromImage(img);
            scale.Draw(g);
         }
         return img;
      }
#endif

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="provider"></param>
      /// <returns>Anzahl der Tiles</returns>
      public int SpecMapClearCache(GMapProvider provider = null) {
         int count = 0;
         if (provider == null) {

            count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, null);           // i.A. lokal (SQLite)
            if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
               count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, null);

         } else {

            count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, provider.DbId);  // i.A. lokal (SQLite)
            if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
               count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, GarminProvider.Instance.DbId);

         }
         return count;
      }

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="idx">bezieht sich auf die Liste der <see cref="SpecMapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public int SpecMapClearCache(int idx) {
         return SpecMapClearCache(idx < 0 ? null : SpecMapProviderDefinitions[idx].Provider);
      }

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void SpecMapClearMemoryCache() => GMaps.Instance.MemoryCache.Clear();

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich
      /// </summary>
      /// <param name="minlon"></param>
      /// <param name="maxlon"></param>
      /// <param name="minlat"></param>
      /// <param name="maxlat"></param>
      /// <returns></returns>
      public List<Marker> SpecMapGetPictureMarkersInArea(double minlon, double maxlon, double minlat, double maxlat) {
         List<Marker> markerlst = new List<Marker>();
         foreach (GMapMarker marker in gpxReadOnlyOverlay.Markers) {
            if (marker is VisualMarker &&
                (marker as VisualMarker).RealMarker.Markertype == Marker.MarkerType.Foto)
               if (minlon <= marker.Position.Lng && marker.Position.Lng <= maxlon &&
                   minlat <= marker.Position.Lat && marker.Position.Lat <= maxlat) {
                  markerlst.Add((marker as VisualMarker).RealMarker);
               }
         }
         return markerlst;
      }

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich um den Client-Punkt herum
      /// </summary>
      /// <param name="localcenter"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<Marker> SpecMapGetPictureMarkersAround(Point localcenter, int deltax, int deltay) {
         // Distanz um den akt. Punkt (1.5 x Markerbildgröße)
         PointLatLng lefttop = Map_FromLocalToLatLng(localcenter.X - deltax / 2,
                                                             localcenter.Y - deltay / 2);
         PointLatLng rightbottom = Map_FromLocalToLatLng(localcenter.X + deltax / 2,
                                                                 localcenter.Y + deltay / 2);
         return SpecMapGetPictureMarkersInArea(lefttop.Lng, rightbottom.Lng, rightbottom.Lat, lefttop.Lat);
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax">client-x +/- delta</param>
      /// <param name="deltay">client-y +/- delta</param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> SpecMapGetGarminObjectInfos(Point ptclient, int deltax, int deltay) {
         List<GarminImageCreator.SearchObject> info = new List<GarminImageCreator.SearchObject>();
         if (Map_Provider is GarminProvider) {
            PointLatLng ptlatlon = Map_FromLocalToLatLng(ptclient.X, ptclient.Y);
            PointLatLng ptdelta = Map_FromLocalToLatLng(ptclient.X + deltax, ptclient.Y + deltay);
            double groundresolution = Map_Provider.Projection.GetGroundResolution((int)Map_Zoom, ptlatlon.Lat);  // Meter je Pixel

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            info = garminImageCreator.GetObjectInfo(ptlatlon.Lng,
                                                    ptlatlon.Lat,
                                                    ptdelta.Lng - ptlatlon.Lng,
                                                    ptlatlon.Lat - ptdelta.Lat,
                                                    groundresolution,
                                                    cancellationTokenSource.Token);

         }
         return info;
      }

      public List<PointD> SpecMapGetPositionByKeywords(string keywords, GeocodingProvider specgp = null) {
         //GeoCoderStatusCode geoCoderStatusCode = GetPositionByKeywords(keywords, out PointLatLng point);

         /*

            point = new PointLatLng();

            var status = GeoCoderStatusCode.UNKNOWN_ERROR;
            var gp = MapProvider as GeocodingProvider;

            if (gp == null)
                gp = GMapProviders.OpenStreetMap as GeocodingProvider;

            if (gp != null)
            {
                var pt = gp.GetPoint(keys.Replace("#", "%23"), out status);

                if (status == GeoCoderStatusCode.OK && pt.HasValue)
                    point = pt.Value;
            }

            return status;
          
          
          */

         List<PointD> result = new List<PointD>();

         GeocodingProvider gp = base.Map_Provider as GeocodingProvider;
         if (specgp != null)
            gp = specgp;

         if (gp == null)
            gp = GMapProviders.OpenStreetMap as GeocodingProvider;

         //gp = GMapProviders.GoogleMap as GeocodingProvider;
         //gp = GMapProviders.BingMap as GeocodingProvider;

         if (gp != null) {
            GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;

            //PointLatLng? pt = gp.GetPoint(keywords.Replace("#", "%23"), out status);

            //if (status == GeoCoderStatusCode.OK && pt.HasValue)
            //   result.Add(new PointD(pt.Value.Lat, pt.Value.Lng));


            status = gp.GetPoints(keywords.Replace("#", "%23"), out List<PointLatLng> pts);
            if (status == GeoCoderStatusCode.OK)
               foreach (var pt in pts) {
                  result.Add(new PointD(pt.Lat, pt.Lng));
               }

         }
         return result;
      }


      /// <summary>
      /// ein echter oder simulierter Mausklick erfolgt an dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void SpecMapDoMouseClick(MouseEventArgs e) {
         ptUsedLastClick.X = int.MinValue;

         if (SpecMapMouseEvent != null) {
            SpecMapLastMouseLocation = e.Location;
            client2LonLat(e.X, e.Y, out double lon, out double lat);
            MapMouseEventArgs mme = new MapMouseEventArgs(MapMouseEventArgs.EventType.Click,
                                                          e.Button,
                                                          e.X,
                                                          e.Y,
                                                          lon,
                                                          lat);
            SpecMapMouseEvent(this, mme);
            if (mme.IsHandled)
               ptUsedLastClick = e.Location;
         }
      }

      /// <summary>
      /// eine echte oder simulierte Mausbewegung erfolgt zu dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void SpecMapDoMouseMove(MouseEventArgs e) {
         if (SpecMapMouseEvent != null) {
            PointLatLng ptlatlon = Map_FromLocalToLatLng(e.X, e.Y);
            SpecMapLastMouseLocation = e.Location;

            if (SpecMapSelectionAreaIsStarted) {
               // DrawReversibleFrame() fkt. NICHT, da der Bildinhalt intern immer wieder akt. (überschrieben) wird!
               // daher diese (langsame aber ausreichende) Methode:
               if (polySelection != null)
                  gpxReadOnlyOverlay.Polygons.Remove(polySelection);
               if (startPointSelectionArea != PointLatLng.Empty) {
                  polySelection = buildSelectionRectangle(startPointSelectionArea, ptlatlon);
                  gpxReadOnlyOverlay.Polygons.Add(polySelection);
               }
            }

            SpecMapMouseEvent(this,
                          new MapMouseEventArgs(MapMouseEventArgs.EventType.Move,
                                                e.Button,
                                                e.X,
                                                e.Y,
                                                ptlatlon.Lng,
                                                ptlatlon.Lat));
            ptUsedLastClick.X = int.MinValue;   // ein nachfolgender Klick ist dann immer "neu"
         }
      }

      /// <summary>
      /// löst die "Click"-Events für Objekte aus und liefert die Listen der betroffenen Objekte
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="all"></param>
      /// <param name="button"></param>
      /// <param name="markerlst">"getroffene" Marker</param>
      /// <param name="tracklst">"getroffene" Tracks</param>
      public void SpecMapDoMouseClick(int clientx,
                                      int clienty,
                                      bool all,
                                      MouseButtons button,
                                      out List<Marker> markerlst,
                                      out List<Track> tracklst) {
         Map_Tapped(clientx, clienty, false, button, all, out List<GMapMarker> marker, out List<GMapTrack> route, out List<GMapPolygon> polygon);

         markerlst = new List<Marker>();
         tracklst = new List<Track>();

         foreach (var item in marker) {
            if (item is VisualMarker)
               markerlst.Add((item as VisualMarker).RealMarker);
         }

         foreach (var item in route) {
            if (item is VisualTrack)
               tracklst.Add((item as VisualTrack).RealTrack);
         }
      }

      #region Tracks

      enum TrackLayer {
         Readonly,
         Editable,
         SelectedParts,
      }


      /// <summary>
      /// zeigt einen <see cref="Track"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="posttrack">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void SpecMapShowTrack(Track track, bool on, Track posttrack) {
         if (track != null) {
            if (on) {

               if (track.VisualTrack == null)
                  track.UpdateVisualTrack();
               mapShowVisualTrack(track.VisualTrack,
                                  true,
                                  track.IsEditable ? TrackLayer.Editable : TrackLayer.Readonly,
                                  posttrack != null ?
                                          posttrack.VisualTrack :
                                          null);

            } else {

               if (track.IsVisible)
                  mapShowVisualTrack(track.VisualTrack,
                                     false,
                                     track.IsEditable ? TrackLayer.Editable : TrackLayer.Readonly);

            }
         }
      }

      /// <summary>
      /// zeigt alle <see cref="Track"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="on"></param>
      public void SpecMapShowTrack(IList<Track> tracks, bool on) {
         for (int i = tracks.Count - 1; i >= 0; i--)
            SpecMapShowTrack(tracks[i],
                         on,
                         on && i > 0 ? tracks[i - 1] : null);
      }

      /// <summary>
      /// zeigt einen <see cref="VisualTrack"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="layer"></param>
      /// <param name="postvt">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      void mapShowVisualTrack(VisualTrack vt, bool on, TrackLayer layer, VisualTrack postvt = null) {
         if (vt != null &&
             vt.Points.Count > 0) {
            if (on) {

               GMapOverlay ov = gpxReadOnlyOverlay;
               switch (layer) {
                  case TrackLayer.Editable:
                     ov = gpxOverlay;
                     break;

                  case TrackLayer.SelectedParts:
                     ov = gpxSelectedPartsOverlay;
                     break;
               }

               vt.IsVisible = true;
               if (!ov.Tracks.Contains(vt))
                  collectionInsert(ov.Tracks, vt, collectionIndexOf(ov.Tracks, postvt));

            } else {

               if (vt.Overlay != null &&
                   vt.IsVisible) {
                  vt.IsVisible = false;
                  vt.Overlay.Tracks.Remove(vt);
               }

            }
         }
      }

      #region selektierte Teil-Tracks

      /// <summary>
      /// akt. selektierte Teil-Tracks
      /// </summary>
      Dictionary<Track, List<VisualTrack>> selectedPartsOfTracks = new Dictionary<Track, List<VisualTrack>>();


      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="idxlst"></param>
      public void SpecMapShowSelectedParts(Track mastertrack, IList<int> idxlst) {
         List<List<Gpx.GpxTrackPoint>> parts = null;
         if (idxlst != null &&
             idxlst.Count > 0) {

            parts = new List<List<Gpx.GpxTrackPoint>>();
            int partstart = 0;
            while (partstart < idxlst.Count) {
               int partend;
               for (partend = partstart + 1; partend < idxlst.Count; partend++) {
                  if (idxlst[partend - 1] + 1 != idxlst[partend]) { // NICHT der nachfolgende Index
                     partend--;
                     break;
                  }
               }
               if (idxlst.Count <= partend)
                  partend--;

               List<Gpx.GpxTrackPoint> ptlst = new List<Gpx.GpxTrackPoint>();
               for (int idx = partstart; idx <= partend; idx++)
                  ptlst.Add(mastertrack.GpxSegment.Points[idxlst[idx]]);
               parts.Add(ptlst);

               partstart = partend + 1;
            }

         }
         mapShowSelectedParts(mastertrack, parts);
      }


      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="ptlst"></param>
      void mapShowSelectedParts(Track mastertrack, List<List<Gpx.GpxTrackPoint>> ptlst) {
         if (mastertrack == null) { // alle VisualTrack entfernen

            foreach (var track in selectedPartsOfTracks.Keys)
               mapHideSelectedPartsOfTrack(track);
            selectedPartsOfTracks.Clear();

         } else {

            mapHideSelectedPartsOfTrack(mastertrack); // alle VisualTrack dieses Tracks entfernen

            if (ptlst != null) {
               if (!selectedPartsOfTracks.TryGetValue(mastertrack, out List<VisualTrack> pseudotracklist)) {
                  pseudotracklist = new List<VisualTrack>();
                  selectedPartsOfTracks.Add(mastertrack, pseudotracklist);
               }

               for (int part = 0; part < ptlst.Count; part++) {
                  VisualTrack pseudotrack = new VisualTrack(new Track(ptlst[part], ""), "", VisualTrack.VisualStyle.SelectedPart);
                  pseudotracklist.Add(pseudotrack);
                  mapShowVisualTrack(pseudotrack,
                                     true,
                                     TrackLayer.SelectedParts);
               }
            } else
               selectedPartsOfTracks.Remove(mastertrack);

         }
      }

      /// <summary>
      /// entfernt alle <see cref="VisualTrack"/> für die Selektion dieses Tracks
      /// </summary>
      /// <param name="track"></param>
      void mapHideSelectedPartsOfTrack(Track track) {
         if (selectedPartsOfTracks.TryGetValue(track, out List<VisualTrack> pseudotracklist)) {
            for (int i = pseudotracklist.Count - 1; i >= 0; i--) {
               mapShowVisualTrack(pseudotracklist[i], false, TrackLayer.SelectedParts);
               pseudotracklist[i].Dispose();
            }
            pseudotracklist.Clear();
         }
      }

      #endregion

      /// <summary>
      /// liefert alle aktuell angezeigten Tracks
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Track> SpecMapGetVisibleTracks(bool onlyeditable) {
         List<Track> lst = new List<Track>();
         if (!onlyeditable)
            foreach (var item in gpxReadOnlyOverlay.Tracks) {
               if (item is VisualTrack &&
                   (item as VisualTrack).RealTrack != null)
                  lst.Add((item as VisualTrack).RealTrack);
            }
         foreach (var item in gpxOverlay.Tracks) {
            if (item is VisualTrack &&
                (item as VisualTrack).RealTrack != null)
               lst.Add((item as VisualTrack).RealTrack);
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Track"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Track"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool SpecMapChangeEditableTrackDrawOrder(IList<Track> trackorder) {
         bool changed = false;

         List<Track> visibletracks = SpecMapGetVisibleTracks(true);
         List<Track> neworder = new List<Track>();
         foreach (Track track in trackorder) {
            if (track.IsVisible &&
                visibletracks.Contains(track))
               neworder.Add(track);
         }

         if (neworder.Count != visibletracks.Count)
            changed = true;
         else {
            for (int i = 0; i < neworder.Count; i++) {
               if (!neworder[i].Equals(visibletracks[i])) {
                  changed = true;
                  break;
               }
            }
         }

         if (changed) {
            gpxOverlay.Tracks.Clear();
            foreach (Track t in neworder) {
               gpxOverlay.Tracks.Add(t.VisualTrack);
            }
         }

         return changed;
      }

      #endregion

      #region Marker

      /// <summary>
      /// zeigt einen <see cref="Marker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="on"></param>
      /// <param name="postmarker">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void SpecMapShowMarker(Marker marker, bool on, Marker postmarker = null) {
         if (marker != null) {
            if (on) {

               if (!marker.IsVisible)
                  marker.IsVisible = true;
               SpecMapShowVisualMarker(marker.VisualMarker,
                                   true,
                                   marker.IsEditable,
                                   postmarker != null ?
                                          postmarker.VisualMarker :
                                          null);

            } else {

               if (marker.IsVisible)
                  SpecMapShowVisualMarker(marker.VisualMarker,
                                      false,
                                      marker.IsEditable);

            }
         }
      }

      /// <summary>
      /// zeigt alle <see cref="Marker"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="markers"></param>
      /// <param name="on"></param>
      public void SpecMapShowMarker(IList<Marker> markers, bool on) {
         for (int i = 0; i < markers.Count; i++) {
            SpecMapShowMarker(markers[i],
                          on,
                          on && i < markers.Count - 1 ? markers[i - 1] : null);
         }
      }

      /// <summary>
      /// zeigt einen <see cref="VisualMarker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vm"></param>
      /// <param name="on"></param>
      /// <param name="toplayer"></param>
      /// <param name="postvm">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void SpecMapShowVisualMarker(VisualMarker vm, bool on, bool toplayer, VisualMarker postvm = null) {
         if (vm != null)
            if (on) {

               GMapOverlay ov = toplayer ?
                                       gpxOverlay :
                                       gpxReadOnlyOverlay;
               vm.IsVisible = true;
               if (!ov.Markers.Contains(vm))
                  collectionInsert(ov.Markers, vm, collectionIndexOf(ov.Markers, postvm));

            } else {

               if (vm.Overlay != null &&
                   vm.IsVisible) {
                  vm.IsVisible = false;
                  vm.Overlay.Markers.Remove(vm);
               }

            }
      }

      /// <summary>
      /// liefert alle aktuell angezeigten Marker
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Marker> SpecMapGetVisibleMarkers(bool onlyeditable) {
         List<Marker> lst = new List<Marker>();
         if (!onlyeditable)
            foreach (var item in gpxReadOnlyOverlay.Markers) {
               if (item is VisualMarker)
                  lst.Add((item as VisualMarker).RealMarker);
            }
         foreach (var item in gpxOverlay.Markers) {
            if (item is VisualMarker)
               lst.Add((item as VisualMarker).RealMarker);
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Marker"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Marker"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool SpecMapChangeEditableMarkerDrawOrder(IList<Marker> markerorder) {
         bool changed = false;

         List<Marker> visiblemarkers = SpecMapGetVisibleMarkers(true);
         List<Marker> neworder = new List<Marker>();
         foreach (Marker m in markerorder) {
            if (m.IsVisible &&
                visiblemarkers.Contains(m))
               neworder.Add(m);
         }

         if (neworder.Count != visiblemarkers.Count)
            changed = true;
         else {
            for (int i = 0; i < neworder.Count; i++) {
               if (!neworder[i].Equals(visiblemarkers[i])) {
                  changed = true;
                  break;
               }
            }
         }

         if (changed) {
            gpxOverlay.Markers.Clear();
            foreach (Marker m in neworder) {
               gpxOverlay.Markers.Add(m.VisualMarker);
            }
         }

         return changed;
      }

      #endregion

   }
}
