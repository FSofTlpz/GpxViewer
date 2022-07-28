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
using System.Threading.Tasks;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl {
   public partial class SmallMapCtrl {
      /*
       * In diesem Teil sollen keinesfalls Elemente der Basisklasse, sondern nur die SMC_-Elemente des anderen Teils verwendet werden. Die SMC_-Elemente stellen ein minimales Interface
       * zur Basisklasse dar. Insofern sollte dieser Teil weitgehend unabhängig von einer konkreten Realisierung der Basisklasse sein.
       * 
       * Die Basisklasse sollte jedoch folgende Funktionen bereitstellen, auch wenn sie (z.B. bei Smartphones) z.T. bedeutungslos sind:
       *    OnPaint(PaintEventArgs e)

       *    OnMouseLeave(EventArgs e)
       *    OnMouseDown(MouseEventArgs e)
       *    OnMouseMove(MouseEventArgs e)
       *    OnMouseUp(MouseEventArgs e)
       *    OnMouseClick(MouseEventArgs e)
       *    
       * OnPaint() bietet die Möglichkeit, die Karte vor der Ausgabe noch zu ergänzen.
       * 
       * Der Name aller öffentlichen Properties und Funktionen, die sich auf die Karte beziehen, beginnt mit Map...
       * Die spezielle Event heißen Map...Event.
       */

      #region eigene Events

      public class TileLoadCompleteEventArgs {
         /// <summary>
         /// complete (or start)
         /// </summary>
         public readonly bool Complete;
         /// <summary>
         /// Milliseconds
         /// </summary>
         public readonly long Ms;

         public TileLoadCompleteEventArgs(bool complete, long ms = 0) {
            Complete = complete;
            Ms = ms;
         }
      }

      public delegate void TileLoadCompleteEventHandler(object sender, TileLoadCompleteEventArgs e);
      event TileLoadCompleteEventHandler mapTileLoadCompleteEvent;

      public delegate void TileLoadCompleteEventDelegate(object sender, TileLoadCompleteEventArgs e);
      TileLoadCompleteEventDelegate myMapWrapper_TileLoadCompleteEventDelegate;
      public event TileLoadCompleteEventHandler MapTileLoadCompleteEvent;

      public delegate void MapZoomChangedEventHandler(object sender, EventArgs e);
      public event MapZoomChangedEventHandler MapZoomChangedEvent;

      public delegate void MapPaintEventHandler(object sender, PaintEventArgs e);
      public event MapPaintEventHandler MapPaintEvent;

      public event EventHandler MapZoomRangeChangedEvent;


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

      public delegate void MapMouseEventHandler(object sender, MapMouseEventArgs e);
      public event MapMouseEventHandler MapMouseEvent;

      public delegate void MapTrackSearch4PolygonEventHandler(object sender, MouseEventArgs e);
      public event MapTrackSearch4PolygonEventHandler MapTrackSearch4PolygonEvent;

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

      public delegate void MapTrackEventHandler(object sender, TrackEventArgs e);
      public event MapTrackEventHandler MapTrackEvent;

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

      public delegate void MapMarkerEventHandler(object sender, MarkerEventArgs e);
      public event MapMarkerEventHandler MapMarkerEvent;

      #endregion

      #region Behandlung der Original-Events (d.h. deren Weiterleitung aus dem anderen Teil)

      /// <summary>
      /// um ev. das Kartenbild noch zu ergänzen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnPaint(object sender, PaintEventArgs e) {
         if (scale != null)
            scale.Draw(e.Graphics, SMC_MapRenderTransform * SMC_MapRenderZoom2RealDevice);
         MapPaintEvent?.Invoke(this, e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnMouseDown(object sender, MouseEventArgs e) {
         MapSetAreaSelectionStartPoint(e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnMouseMove(object sender, MouseEventArgs e) {
         MapDoMouseMove(e);
      }

      /// <summary>
      /// nur für Bereichsselektion nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnMouseUp(object sender, MouseEventArgs e) {
         MapSetAreaSelectionEndPoint(e);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnMouseClick(object sender, MouseEventArgs e) {
         MapDoMouseClick(e);
      }

      /// <summary>
      /// die Maus verläßt den Bereich der Karte
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void eventSMC_OnMouseLeave(object sender, EventArgs e) {
         MapMouseEvent?.Invoke(this, new MapMouseEventArgs(MapMouseEventArgs.EventType.Leave, MouseButtons.None, 0, 0, 0, 0, 0, 0));
      }

      void eventSMC_OnMapZoomChanged() {
         MapZoomChangedEvent?.Invoke(this, new EventArgs());
      }

      void eventSMC_OnTileLoadStart() {
         //TileLoadCompleteEvent?.Invoke(this, new TileLoadCompleteEventArgs(false));
         mapTileLoadCompleteEvent?.BeginInvoke(this, new TileLoadCompleteEventArgs(false), null, null);     // asynchrone, threadsichere Weitergabe
      }

      void eventSMC_OnTileLoadComplete(long elapsedMilliseconds) {
         //TileLoadCompleteEvent?.Invoke(this, new TileLoadCompleteEventArgs(true, ElapsedMilliseconds));
         mapTileLoadCompleteEvent?.BeginInvoke(this, new TileLoadCompleteEventArgs(true, elapsedMilliseconds), null, null);      // asynchrone, threadsichere Weitergabe
      }

      private void ts_MapWrapper_TileLoadCompleteEvent(object sender, TileLoadCompleteEventArgs e) {
         MapTileLoadCompleteEvent?.Invoke(sender, e);
      }

      private void control_TileLoadCompleteEvent(object sender, TileLoadCompleteEventArgs e) {
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

      private void eventSMC_OnMarkerEnter(GMapMarker item) {
         if (item != null &&
             item is VisualMarker)
            MapMarkerEvent?.Invoke(this, new MarkerEventArgs((item as VisualMarker).RealMarker, MapMouseEventArgs.EventType.Enter));
      }

      private void eventSMC_OnMarkerLeave(GMapMarker item) {
         if (item != null &&
             item is VisualMarker)
            MapMarkerEvent?.Invoke(this, new MarkerEventArgs((item as VisualMarker).RealMarker, MapMouseEventArgs.EventType.Leave));
      }

      private void eventSMC_OnMarkerClick(GMapMarker item, MouseEventArgs e) {
         if ((ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
              ptUsedLastClick != e.Location) &&
             item is VisualMarker) {
            Client2LonLat(this, e.X, e.Y, out double lon, out double lat);
            MarkerEventArgs me = new MarkerEventArgs((item as VisualMarker).RealMarker,
                                                     MapMouseEventArgs.EventType.Click,
                                                     e.Button,
                                                     e.X,
                                                     e.Y,
                                                     lon,
                                                     lat);
            MapMarkerEvent?.Invoke(this, me);
            if (me.IsHandled)
               ptUsedLastClick = e.Location;
         }
      }

      #endregion

      #region Maus-Events für Tracks (löst das MapTrackEvent aus)

      void eventSMC_OnRouteEnter(GMapRoute item) {
         if (item is VisualTrack) {
            Track track = (item as VisualTrack).RealTrack;
            if (track != null)
               MapTrackEvent?.Invoke(this, new TrackEventArgs(track, MapMouseEventArgs.EventType.Enter));
         }
      }

      void eventSMC_OnRouteLeave(GMapRoute item) {
         if (item is VisualTrack) {
            Track track = (item as VisualTrack).RealTrack;
            if (track != null)
               MapTrackEvent?.Invoke(this, new TrackEventArgs(track, MapMouseEventArgs.EventType.Leave));
         }
      }

      void eventSMC_OnRouteClick(GMapRoute item, MouseEventArgs e) {
         if ((ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
              ptUsedLastClick != e.Location) &&
             item is VisualTrack) {
            clickOnRoute(item, e.X, e.Y, e.Button);

            Track track = (item as VisualTrack).RealTrack;
            if (track != null) {
               Client2LonLat(this, e.X, e.Y, out double lon, out double lat);
               TrackEventArgs te = new TrackEventArgs(track,
                                                      MapMouseEventArgs.EventType.Click,
                                                      e.Button,
                                                      e.X,
                                                      e.Y,
                                                      lon,
                                                      lat);
               MapTrackEvent?.Invoke(this, te);
               if (te.IsHandled)
                  ptUsedLastClick = e.Location;
            }
         }
      }

      /// <summary>
      /// auf die <see cref="GMapRoute"/> wurde am angegebenen Client-Punkt geklickt
      /// </summary>
      /// <param name="item"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="mouseButton"></param>
      void clickOnRoute(GMapRoute item, int clientx, int clienty, MouseButtons mouseButton) {
         if ((ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
              (ptUsedLastClick.X != clientx || ptUsedLastClick.Y != clienty)) &&
             item is VisualTrack) {
            Track track = (item as VisualTrack).RealTrack;
            if (track != null) {
               Client2LonLat(this, clientx, clienty, out double lon, out double lat);
               TrackEventArgs te = new TrackEventArgs(track,
                                                      MapMouseEventArgs.EventType.Click,
                                                      mouseButton,
                                                      clientx,
                                                      clienty,
                                                      lon,
                                                      lat);
               MapTrackEvent?.Invoke(this, te);
               if (te.IsHandled) {
                  ptUsedLastClick.X = clientx;
                  ptUsedLastClick.Y = clienty;
               }
            }
         }
      }

      #endregion

      #endregion

      /// <summary>
      /// Maßstab für das <see cref="SmallMapCtrl"/>
      /// </summary>
      Scale4Map scale;

      /// <summary>
      /// Liste der registrierten Karten-Provider
      /// </summary>
      public List<MapProviderDefinition> MapProviderDefinitions = new List<MapProviderDefinition>();

      /// <summary>
      /// letzte registrierte Mausposition im Karten-Control
      /// </summary>
      public Point MapLastMouseLocation { get; protected set; } = Point.Empty;

      /* Mehrfache Auswertungen z.B. eines Mausklicks können nicht so einfach verhindert werden. Wenn z.B. der Mausklick auf einen Marker und einen Track erfolgt, erhalten BEIDE diesen
       * Klick. Andererseits wird der Klick NICHT für weitere Marker/Tracks ausgewertet.
       */
      /// <summary>
      /// Pos. des letzten ausgewerteten Klicks
      /// </summary>
      Point ptUsedLastClick = new Point();

      /// <summary>
      /// zur internen Erzeugung der Garminkarten und zur (externen) Objektsuche
      /// </summary>
      GarminImageCreator.ImageCreator garminImageCreator;

      /// <summary>
      /// Overlay für die GPX-Daten
      /// </summary>
      readonly GMapOverlay GpxReadOnlyOverlay = new GMapOverlay("GPXro");
      readonly GMapOverlay GpxOverlay = new GMapOverlay("GPX");
      readonly GMapOverlay GpxSelectedPartsOverlay = new GMapOverlay("GPXselparts");

      /// <summary>
      /// auch den Cache verwenden oder nur den Karten-Server (gilt global)
      /// </summary>
      public static bool MapCacheIsActiv {
         get => GMaps.Instance.Mode != AccessMode.ServerOnly;
         set => GMaps.Instance.Mode = value ?
                                          AccessMode.ServerAndCache :
                                          AccessMode.ServerOnly;
      }

      /// <summary>
      /// Cache-Verzeichnis
      /// </summary>
      public string MapCacheLocation {
         get => SMC_CacheLocation;
         set => SMC_CacheLocation = value;
      }

      /// <summary>
      /// min. Zoom für die Karte
      /// </summary>
      public int MapMinZoom {
         get => SMC_MinZoom;
         set {
            SMC_MinZoom = value;
            MapZoomRangeChangedEvent?.Invoke(this, new EventArgs());
         }
      }

      /// <summary>
      /// max. Zoom für die Karte
      /// </summary>
      public int MapMaxZoom {
         get => SMC_MaxZoom;
         set {
            SMC_MaxZoom = value;
            MapZoomRangeChangedEvent?.Invoke(this, new EventArgs());
         }
      }

      /// <summary>
      /// Zoom für die Karte (intern int)
      /// </summary>
      public double MapZoom {
         get => SMC_Zoom;
         set {
            SMC_Zoom = Math.Max(SMC_MinZoom, Math.Min(SMC_MaxZoom, value));
            // Bei einem nicht-ganzzahligen Zoom löst der Core und GMapControl KEIN ZoomChangedEvent aus.
            // Deshalb bei Bedarf an dieser Stelle:
            if ((MapZoom % 1) != 0)
               MapZoomChangedEvent?.Invoke(this, new EventArgs());
         }
      }

      /// <summary>
      /// geogr. Länge des Mittelpunktes der Karte
      /// </summary>
      public double MapCenterLon {
         get => SMC_Position.Lng;
      }

      /// <summary>
      /// geogr. Breite des Mittelpunktes der Karte
      /// </summary>
      public double MapCenterLat {
         get => SMC_Position.Lat;
      }

      /// <summary>
      /// Cursor der Karte
      /// </summary>
      public Cursor MapCursor {
         get {
            return SMC_Cursor;
         }
         set {
            SMC_Cursor = value;
         }
      }

      /// <summary>
      /// Der Maus-Button zum "Ziehen" der Karte
      /// </summary>
      public MouseButtons MapDragButton {
         get => SMC_DragButton;
         set => SMC_DragButton = value;
      }


      /// <summary>
      /// wird NACH OnLoad() ausgeführt
      /// </summary>
      void eventSMC_OnLoad() {
         //  vor .NET 4.5
         ServicePointManager.Expect100Continue = true;
         ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
         //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //SecurityProtocolType.SystemDefault;

         SMC_MapProvider = GMap.NET.MapProviders.EmptyProvider.Instance;   // gMapProviders[startprovideridx];
         SMC_EmptyTileText = "no data";                                    // Hinweistext für "Tile ohne Daten"

         SMC_MinZoom = 0;
         SMC_MaxZoom = 24;
         SMC_Zoom = 20;

         SMC_ShowTileGridLines = false;
         SMC_ShowCenter = false;                        // shows a little red cross on the map to show you exactly where the center is
         SMC_EmptyMapBackground = Color.LightGray;      // Tile (noch) ohne Daten
         SMC_EmptyTileText = "keine Daten";             // Hinweistext für "Tile ohne Daten"
         SMC_EmptyTileColor = Color.DarkGray;           // Tile (endgültig) ohne Daten

         SMC_Overlays.Add(GpxReadOnlyOverlay);
         GpxReadOnlyOverlay.IsVisibile = true;

         SMC_Overlays.Add(GpxOverlay);
         GpxOverlay.IsVisibile = true;

         SMC_Overlays.Add(GpxSelectedPartsOverlay);
         GpxSelectedPartsOverlay.IsVisibile = true;

         myMapWrapper_TileLoadCompleteEventDelegate = new TileLoadCompleteEventDelegate(ts_MapWrapper_TileLoadCompleteEvent);
         mapTileLoadCompleteEvent += control_TileLoadCompleteEvent;

         scale = new Scale4Map(this);
      }

      /// <summary>
      /// ein echter oder simulierter Mausklick erfolgt an dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void MapDoMouseClick(MouseEventArgs e) {
         ptUsedLastClick.X = int.MinValue;

         if (MapMouseEvent != null) {
            MapLastMouseLocation = e.Location;
            Client2LonLat(this, e.X, e.Y, out double lon, out double lat);
            MapMouseEventArgs mme = new MapMouseEventArgs(MapMouseEventArgs.EventType.Click,
                                                          e.Button,
                                                          e.X,
                                                          e.Y,
                                                          lon,
                                                          lat);
            MapMouseEvent(this, mme);
            if (mme.IsHandled)
               ptUsedLastClick = e.Location;
         }
      }

      /// <summary>
      /// eine echte oder simulierte Mausbewegung erfolgt zu dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void MapDoMouseMove(MouseEventArgs e) {
         if (MapMouseEvent != null) {
            PointLatLng ptlatlon = SMC_FromLocalToLatLng(e.X, e.Y);
            MapLastMouseLocation = e.Location;

            if (MapSelectionAreaIsStarted) {
               // DrawReversibleFrame() fkt. NICHT, da der Bildinhalt intern immer wieder akt. (überschrieben) wird!
               // daher diese (langsame aber ausreichende) Methode:
               if (polySelection != null)
                  GpxReadOnlyOverlay.Polygons.Remove(polySelection);
               if (startPointSelectionArea != GMap.NET.PointLatLng.Empty) {
                  polySelection = buildSelectionRectangle(startPointSelectionArea, ptlatlon);
                  GpxReadOnlyOverlay.Polygons.Add(polySelection);
               }
            }

            MapMouseEvent(this,
                          new MapMouseEventArgs(MapMouseEventArgs.EventType.Move,
                                                e.Button,
                                                e.X,
                                                e.Y,
                                                ptlatlon.Lng,
                                                ptlatlon.Lat));
            ptUsedLastClick.X = int.MinValue;   // ein nachfolgender Klick ist dann immer "neu"
         }
      }

      #region Konvertierungfunktionen

      static void Client2LonLat(SmallMapCtrl gMapControl, int clientx, int clientxy, out double lon, out double lat) {
         PointD ptgeo = Client2LonLat(gMapControl, clientx, clientxy);
         lon = ptgeo.X;
         lat = ptgeo.Y;
      }

      static PointD Client2LonLat(SmallMapCtrl gMapControl, int clientx, int clienty) {
         PointLatLng ptlatlon = gMapControl.SMC_FromLocalToLatLng(clientx, clienty);
         return MapPointLatLng2PointD(ptlatlon);
      }

      static PointD Client2LonLat(SmallMapCtrl gMapControl, Point ptclient) {
         return Client2LonLat(gMapControl, (int)ptclient.X, (int)ptclient.Y);
      }

      static PointD Client2LonLat(SmallMapCtrl gMapControl, GPoint ptclient) {
         return Client2LonLat(gMapControl, (int)ptclient.X, (int)ptclient.Y);
      }

      static void LonLat2Client(SmallMapCtrl gMapControl, double lon, double lat, out int clientx, out int clienty) {
         Point pt = LonLat2Client(gMapControl, new PointLatLng(lat, lon));
         clientx = pt.X;
         clienty = pt.Y;
      }

      static Point LonLat2Client(SmallMapCtrl gMapControl, double lon, double lat) {
         return LonLat2Client(gMapControl, new PointLatLng(lat, lon));
      }

      static Point LonLat2Client(SmallMapCtrl gMapControl, Gpx.GpxTrackPoint ptgeo) {
         return LonLat2Client(gMapControl, new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      static Point LonLat2Client(SmallMapCtrl gMapControl, Gpx.GpxWaypoint ptgeo) {
         return LonLat2Client(gMapControl, new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      static Point LonLat2Client(SmallMapCtrl gMapControl, PointD ptgeo) {
         return LonLat2Client(gMapControl, new PointLatLng(ptgeo.Y, ptgeo.X));
      }

      static Point LonLat2Client(SmallMapCtrl gMapControl, PointLatLng ptgeo) {
         GPoint pt = gMapControl.SMC_FromLatLngToLocal(ptgeo);
         return new Point((int)pt.X, (int)pt.Y);
      }

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clientxy"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void MapClient2LonLat(int clientx, int clientxy, out double lon, out double lat) {
         Client2LonLat(this, clientx, clientxy, out lon, out lat);
      }

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(int clientx, int clienty) {
         return Client2LonLat(this, clientx, clienty);
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
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD MapClient2LonLat(GPoint ptclient) {
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
         MapLonLat2Client(lon, lat, out clientx, out clienty);
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(double lon, double lat) {
         return MapLonLat2Client(new PointLatLng(lat, lon));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(Gpx.GpxTrackPoint ptgeo) {
         return MapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(Gpx.GpxWaypoint ptgeo) {
         return MapLonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(FSofTUtils.Geometry.PointD ptgeo) {
         return MapLonLat2Client(new PointLatLng(ptgeo.Y, ptgeo.X));
      }

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point MapLonLat2Client(PointLatLng ptgeo) {
         GPoint pt = SMC_FromLatLngToLocal(ptgeo);
         return new Point((int)pt.X, (int)pt.Y);
      }

      public static PointD MapPointLatLng2PointD(PointLatLng ptgeo) {
         return new PointD(ptgeo.Lng, ptgeo.Lat);
      }

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

      #endregion

      #region Handling Auswahlrechteck

      Cursor cursorOrgOnSelection = null;
      PointLatLng startPointSelectionArea = PointLatLng.Empty;
      GMapPolygon polySelection = null;

      /// <summary>
      /// am Clientpunkt beginnt die Auswahl der Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void MapSetAreaSelectionStartPoint(MouseEventArgs e) {
         if (MapSelectionAreaIsStarted) {
            startPointSelectionArea = SMC_FromLocalToLatLng(e.X, e.Y);
            polySelection = buildSelectionRectangle(startPointSelectionArea, startPointSelectionArea);
            GpxReadOnlyOverlay.Polygons.Add(polySelection);
         }
      }

      /// <summary>
      /// am Clientpunkt ended die Auswahl des Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void MapSetAreaSelectionEndPoint(MouseEventArgs e) {
         if (MapSelectionAreaIsStarted)
            MapTrackSearch4PolygonEvent?.Invoke(this, e); // Ende der Eingabe simulieren
      }

      /// <summary>
      /// Wird gerade ein Auswahlrechteck gezeichnet? (mit <see cref="MapStartSelectionArea"/>() gestartet)
      /// </summary>
      public bool MapSelectionAreaIsStarted { get; protected set; } = false;

      /// <summary>
      /// merkt sich den Originalbutton während der Auswahl eines Auswahlrechtecks
      /// </summary>
      MouseButtons orgMapDragButton;

      /// <summary>
      /// startet die Auswahl einer Fläche
      /// </summary>
      public void MapStartSelectionArea() {
         MapSelectionAreaIsStarted = true;
         cursorOrgOnSelection = Cursor;
#if GMAP4SKIA
#else
         Cursor = Cursors.Cross;
#endif
         startPointSelectionArea = GMap.NET.PointLatLng.Empty;
         // den Dragbutton für die Maus merken und deaktivieren
         orgMapDragButton = SMC_DragButton;
         SMC_DragButton = MouseButtons.None;
      }

      /// <summary>
      /// liefert eine ausgewählte Fläche oder null
      /// </summary>
      /// <returns></returns>
      public Gpx.GpxBounds MapEndSelectionArea() {
         MapSelectionAreaIsStarted = false;
         Cursor = cursorOrgOnSelection;
         SMC_DragButton = orgMapDragButton;         // den Dragbutton für die Maus wieder aktivieren
         Gpx.GpxBounds bounds = null;
         if (polySelection != null) {
            GpxReadOnlyOverlay.Polygons.Remove(polySelection);
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
         return new GMapPolygon(new List<GMap.NET.PointLatLng>() { start,
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
      public static void MapSetProxy(string proxy, int proxyport, string user, string password) {
         if (string.IsNullOrEmpty(proxy)) {
            GMap.NET.MapProviders.GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
         } else {
            GMap.NET.MapProviders.GMapProvider.IsSocksProxy = true;
            GMap.NET.MapProviders.GMapProvider.WebProxy = new WebProxy(proxy, proxyport) {
               Credentials = new System.Net.NetworkCredential(user, password)
            };
         }
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
         MapProviderDefinitions.Clear();

         if (providernames != null)
            for (int i = 0; i < providernames.Count; i++) {
               MapProviderDefinition def = provdefs[i];

               if (providernames[i] == GarminProvider.Instance.Name &&
                   def is GarminProvider.GarminMapDefinitionData) {

                  def.Provider = GarminProvider.Instance;
                  MapProviderDefinitions.Add(def as GarminProvider.GarminMapDefinitionData);

               } else if (providernames[i] == GarminKmzProvider.Instance.Name &&
                          def is GarminKmzProvider.KmzMapDefinition) {

                  def.Provider = GarminKmzProvider.Instance;
                  MapProviderDefinitions.Add(def as GarminKmzProvider.KmzMapDefinition);

               } else if (providernames[i] == WMSProvider.Instance.Name &&
                          def is WMSProvider.WMSMapDefinition) {

                  def.Provider = WMSProvider.Instance;
                  MapProviderDefinitions.Add(def as WMSProvider.WMSMapDefinition);

               } else {

                  for (int p = 0; p < GMapProviders.List.Count; p++) {
                     if (GMapProviders.List[p].Name == providernames[i]) { // Provider ist vorhanden

                        def.Provider = GMapProviders.List[p];
                        MapProviderDefinitions.Add(def);

                     }
                  }

               }
            }
      }

      /// <summary>
      /// setzt den aktiven Karten-Provider
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      public void MapSetActivProvider(int idx, int demalpha, DemData dem = null) {
         MapProviderDefinition def = MapProviderDefinitions[idx];
         GMapProvider newprov = def.Provider;

         GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = false;

         // ev. Zoom behandeln
         double newzoom = -1;
         if (SMC_Zoom < def.MinZoom || def.MaxZoom < SMC_Zoom) {
            newzoom = Math.Min(Math.Max(SMC_Zoom, def.MinZoom), def.MaxZoom);
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
                                                                "",                                   // <- path for sqlite cache
                                                                gdef.Levels4LocalCache,
                                                                gdef.MaxSubdivs,
                                                                gdef.TextFactor,
                                                                gdef.LineFactor,
                                                                gdef.SymbolFactor));
            }

            garminImageCreator.SetGarminMapDefs(mapdata);
            (newprov as GarminProvider).GarminImageCreater = garminImageCreator;      // hier werden die Daten im Provider indirekt über den ImageCreator gesetzt

         } else if (newprov is WMSProvider) {

            (newprov as WMSProvider).ChangeDbId(WMSProvider.StandardDbId + (def as WMSProvider.WMSMapDefinition).DbIdDelta);
            (newprov as WMSProvider).SetDef(def as WMSProvider.WMSMapDefinition);

         } else if (newprov is GarminKmzProvider) {

            (newprov as GarminKmzProvider).ChangeDbId(GarminKmzProvider.StandardDbId + (def as GarminKmzProvider.KmzMapDefinition).DbIdDelta);
            (newprov as GarminKmzProvider).SetDef(def as GarminKmzProvider.KmzMapDefinition);

         }

         // jetzt wird der neue Provider und ev. auch der Zoom gesetzt
         SMC_MapRenderZoom2RealDevice = 1;
         if (def.Zoom4Display != 1)
            SMC_MapRenderZoom2RealDevice = (float)def.Zoom4Display;

         SMC_MapProvider = newprov;
         if (newzoom >= 0)
            SMC_Zoom = newzoom;
         MapMinZoom = def.MinZoom;
         MapMaxZoom = def.MaxZoom;
         MapRefresh(true, false);

         GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = true; // ohne false/true-Wechsel passt die Darstellung des Overlays manchmal nicht zur Karte
      }

      public Task MapSetActivProviderAsync(int idx, int demalpha, DemData dem = null) {
         return Task.Run(() => {
            MapSetActivProvider(idx, demalpha, dem);
         });
      }

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="MapProviderDefinitions"/>-Liste
      /// </summary>
      /// <returns></returns>
      public int MapGetActiveProviderIdx() {
         if (SMC_MapProvider != null &&
             MapProviderDefinitions != null)
            for (int i = 0; i < MapProviderDefinitions.Count; i++) {
               if (SMC_MapProvider.Equals(MapProviderDefinitions[i].Provider))
                  return i;
            }
         return -1;
      }

      /// <summary>
      /// zeichnet die Karte neu
      /// </summary>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      public void MapRefresh(bool clearmemcache, bool clearcache) {
         //if (gMapControl.MapProvider is WMSProvider ||
         //    gMapControl.MapProvider is GarminKmzProvider ||
         //    gMapControl.MapProvider is GarminProvider)
         //   gMapControl.Manager.MemoryCache.Clear(); // Cache muss gelöscht werden

         if (clearmemcache)
            SMC_Manager.MemoryCache.Clear(); // Die Tiles in diesem Cache haben KEINE DbId!

         if (clearcache) {
            if (SMC_Manager.PrimaryCache != null) {
               SMC_Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, SMC_MapProvider.DbId);
            }
            if (SMC_Manager.SecondaryCache != null) {
               SMC_Manager.SecondaryCache.DeleteOlderThan(DateTime.Now, SMC_MapProvider.DbId);
            }
         }

         SMC_ReloadMap();
         SMC_Refresh();
      }

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public void MapSetLocationAndZoom(double zoom, double centerlon, double centerlat) {
         SMC_Position = new GMap.NET.PointLatLng(centerlat, centerlon);
         SMC_Zoom = zoom;
      }

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public void MapMoveView(double dxpercent, double dypercent) {
         SMC_Position = new GMap.NET.PointLatLng(SMC_Position.Lat + SMC_ViewArea.HeightLat * dypercent,
                                                 SMC_Position.Lng + SMC_ViewArea.WidthLng * dxpercent);
      }

      /// <summary>
      /// zum Bereich zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      public void MapZoomToRange(PointD topleft, PointD bottomright) {
         SMC_SetZoomToFitRect(new RectLatLng(topleft.Y,
                                             topleft.X,
                                             Math.Abs(topleft.X - bottomright.X),
                                             Math.Abs(topleft.Y - bottomright.Y))); // Ecke links-oben, Breite, Höhe
      }

#if GMAP4SKIA
      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <returns></returns>
      public Bitmap MapGetViewAsImage() {
         return SMC_ToImage();
      }
#else
      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <returns></returns>
      public Image MapGetViewAsImage(bool withscale = true) {
         Image img = SMC_ToImage();
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
      public int MapClearCache(GMapProvider provider = null) {
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
      /// <param name="idx">bezieht sich auf die Liste der <see cref="MapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public int MapClearCache(int idx) {
         return MapClearCache(idx < 0 ? null : MapProviderDefinitions[idx].Provider);
      }

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void MapClearMemoryCache() {
         GMaps.Instance.MemoryCache.Clear();
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
         List<Marker> markerlst = new List<Marker>();
         foreach (GMapMarker marker in GpxReadOnlyOverlay.Markers) {
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
      public List<Marker> MapGetPictureMarkersAround(Point localcenter, int deltax, int deltay) {
         // Distanz um den akt. Punkt (1.5 x Markerbildgröße)
         PointLatLng lefttop = SMC_FromLocalToLatLng(localcenter.X - deltax / 2,
                                                             localcenter.Y - deltay / 2);
         PointLatLng rightbottom = SMC_FromLocalToLatLng(localcenter.X + deltax / 2,
                                                                 localcenter.Y + deltay / 2);
         return MapGetPictureMarkersInArea(lefttop.Lng, rightbottom.Lng, rightbottom.Lat, lefttop.Lat);
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> MapGetGarminObjectInfos(Point ptclient, int deltax, int deltay) {
         List<GarminImageCreator.SearchObject> info = new List<GarminImageCreator.SearchObject>();
         if (SMC_MapProvider is GarminProvider) {
            PointLatLng ptlatlon = SMC_FromLocalToLatLng(ptclient.X, ptclient.Y);
            PointLatLng ptdelta = SMC_FromLocalToLatLng(ptclient.X + deltax, ptclient.Y + deltay);
            double groundresolution = SMC_MapProvider.Projection.GetGroundResolution((int)SMC_Zoom, ptlatlon.Lat);  // Meter je Pixel
            info = garminImageCreator.GetObjectInfo(ptlatlon.Lng,
                                                    ptlatlon.Lat,
                                                    ptdelta.Lng - ptlatlon.Lng,
                                                    ptlatlon.Lat - ptdelta.Lat,
                                                    groundresolution);

         }
         return info;
      }


      public List<PointD> GetPositionByKeywords(string keywords, GeocodingProvider specgp = null) {
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

         GeocodingProvider gp = MapProvider as GeocodingProvider;
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
      public void MapShowTrack(Track track, bool on, Track posttrack) {
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
      public void MapShowTrack(IList<Track> tracks, bool on) {
         for (int i = tracks.Count - 1; i >= 0; i--) {
            MapShowTrack(tracks[i],
                         on,
                         on && i > 0 ? tracks[i - 1] : null);
         }
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

               GMapOverlay ov = GpxReadOnlyOverlay;
               switch (layer) {
                  case TrackLayer.Editable:
                     ov = GpxOverlay;
                     break;

                  case TrackLayer.SelectedParts:
                     ov = GpxSelectedPartsOverlay;
                     break;
               }

               vt.IsVisible = true;
               if (!ov.Routes.Contains(vt))
                  collectionInsert(ov.Routes, vt, collectionIndexOf(ov.Routes, postvt));

            } else {

               if (vt.Overlay != null &&
                   vt.IsVisible) {
                  vt.IsVisible = false;
                  vt.Overlay.Routes.Remove(vt);
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
      public void MapShowSelectedParts(Track mastertrack, IList<int> idxlst) {
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
      public List<Track> MapGetVisibleTracks(bool onlyeditable) {
         List<Track> lst = new List<Track>();
         if (!onlyeditable)
            foreach (var item in GpxReadOnlyOverlay.Routes) {
               if (item is VisualTrack &&
                   (item as VisualTrack).RealTrack != null)
                  lst.Add((item as VisualTrack).RealTrack);
            }
         foreach (var item in GpxOverlay.Routes) {
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
      public bool MapChangeEditableTrackDrawOrder(IList<Track> trackorder) {
         bool changed = false;

         List<Track> visibletracks = MapGetVisibleTracks(true);
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
            GpxOverlay.Routes.Clear();
            foreach (Track t in neworder) {
               GpxOverlay.Routes.Add(t.VisualTrack);
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
      public void MapShowMarker(Marker marker, bool on, Marker postmarker = null) {
         if (marker != null) {
            if (on) {

               if (!marker.IsVisible)
                  marker.IsVisible = true;
               MapShowVisualMarker(marker.VisualMarker,
                                   true,
                                   marker.IsEditable,
                                   postmarker != null ?
                                          postmarker.VisualMarker :
                                          null);

            } else {

               if (marker.IsVisible)
                  MapShowVisualMarker(marker.VisualMarker,
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
      public void MapShowMarker(IList<Marker> markers, bool on) {
         for (int i = 0; i < markers.Count; i++) {
            MapShowMarker(markers[i],
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
      public void MapShowVisualMarker(VisualMarker vm, bool on, bool toplayer, VisualMarker postvm = null) {
         if (vm != null)
            if (on) {

               GMapOverlay ov = toplayer ?
                                       GpxOverlay :
                                       GpxReadOnlyOverlay;
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
      public List<Marker> MapGetVisibleMarkers(bool onlyeditable) {
         List<Marker> lst = new List<Marker>();
         if (!onlyeditable)
            foreach (var item in GpxReadOnlyOverlay.Markers) {
               if (item is VisualMarker)
                  lst.Add((item as VisualMarker).RealMarker);
            }
         foreach (var item in GpxOverlay.Markers) {
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
      public bool MapChangeEditableMarkerDrawOrder(IList<Marker> markerorder) {
         bool changed = false;

         List<Marker> visiblemarkers = MapGetVisibleMarkers(true);
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
            GpxOverlay.Markers.Clear();
            foreach (Marker m in neworder) {
               GpxOverlay.Markers.Add(m.VisualMarker);
            }
         }

         return changed;
      }

      #endregion

   }
}
