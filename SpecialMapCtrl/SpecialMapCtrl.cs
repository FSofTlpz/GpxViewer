//#define USEMATRIXTRANSFORM

using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using GMap.NET;
using GMap.NET.CacheProviders;
using GMap.NET.FSofTExtented;
using GMap.NET.FSofTExtented.MapProviders;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using SpecialMapCtrl.NET.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

#if GMAP4SKIA
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Drawing.System.Drawing;
#else
#endif

namespace SpecialMapCtrl {

   /// <summary>
   ///     GMap.NET control for Windows Forms
   /// </summary>
   public partial class SpecialMapCtrl :
#if GMAP4SKIA
      SKCanvasView,
#else
      UserControl,
#endif
      IDisposable {


      public enum ScaleModes {
         /// <summary>
         ///     no scaling
         /// </summary>
         Integer,

         /// <summary>
         /// </summary>
         Fractional,
      }

      #region Events

      #region EventArgs

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

      public class TileLoadChangeEventArgs {

         /// <summary>
         /// Laden beendet?
         /// </summary>
         public readonly bool Complete;

         /// <summary>
         /// Anzahl
         /// </summary>
         public readonly int Count;

         public TileLoadChangeEventArgs(bool complete, int count) {
            Complete = complete;
            Count = count;
         }
      }

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

      public class TrackEventArgs : MapMouseEventArgs {

         public Track? Track;

         public TrackEventArgs(Track? track, EventType eventtype) :
            base(eventtype) {
            Track = track;
         }

         public TrackEventArgs(Track? track, EventType eventtype, MouseButtons buttons, int x, int y, double lon, double lat) :
            base(eventtype, buttons, 0, x, y, 0, lon, lat) {
            Track = track;
         }

      }

      public class MarkerEventArgs : MapMouseEventArgs {

         public Marker? Marker;

         public MarkerEventArgs(Marker? marker, EventType eventtype) :
            base(eventtype) {
            Marker = marker;
         }

         public MarkerEventArgs(Marker? marker, EventType eventtype, MouseButtons buttons, int x, int y, double lon, double lat) :
            base(eventtype, buttons, 0, x, y, 0, lon, lat) {
            Marker = marker;
         }

      }

      public class PositionChangedEventArgs {
         public readonly PointLatLng Point;

         public PositionChangedEventArgs(PointLatLng point) {
            Point = point;
         }
      }

      public class TileLoadCompleteEventArgs {
         public readonly long ElapsedMilliseconds;

         public TileLoadCompleteEventArgs(long elapsedMilliseconds) {
            ElapsedMilliseconds = elapsedMilliseconds;
         }
      }

      public class TileCacheProgressEventArgs {
         public readonly int TilesLeft;

         public TileCacheProgressEventArgs(int tilesLeft) {
            TilesLeft = tilesLeft;
         }
      }

      public class MapTypeChangedEventArgs {
         public readonly GMapProvider Provider;

         public MapTypeChangedEventArgs(GMapProvider provider) {
            Provider = provider;
         }
      }

      public class EmptyTileErrorEventArgs {
         public readonly int Zoom;
         public readonly GPoint Pos;

         public EmptyTileErrorEventArgs(int zoom, GPoint pos) {
            Zoom = zoom;
            Pos = pos;
         }
      }

      public class ExceptionThrownEventArgs {
         public readonly Exception Exception;
         /// <summary>
         /// bei false sollte keine direkte Anzeige erfolgen
         /// </summary>
         public readonly bool ShouldShow;

         public ExceptionThrownEventArgs(Exception exception, bool shouldShow = true) {
            Exception = exception;
            ShouldShow = shouldShow;
         }
      }

      public class MapClickEventArgs {
         public readonly PointLatLng Point;

         public readonly MouseEventArgs Mea;

         public MapClickEventArgs(PointLatLng point, MouseEventArgs e) {
            Point = point;
            Mea = e;
         }
      }

      public class DrawExtendedEventArgs {
         public readonly int CoreZoom;
         public readonly double ExtendedZoom;
         public readonly double DeviceZoom;
         public readonly Graphics Graphics;

         public DrawExtendedEventArgs(Graphics g, int corezoom, double extzoom, double deviceZoom) {
            CoreZoom = corezoom;
            ExtendedZoom = extzoom;
            DeviceZoom = deviceZoom;
            Graphics = g;
         }
      }

      public class MapMarkerEventArgs {
         public readonly MapMarker Marker;

         public readonly MouseEventArgs? Mea;

         public MapMarkerEventArgs(MapMarker marker, MouseEventArgs? e = null) {
            Marker = marker;
            Mea = e;
         }
      }

      public class MapTrackEventArgs {
         public readonly MapTrack Track;

         public readonly MouseEventArgs? Mea;

         public MapTrackEventArgs(MapTrack track, MouseEventArgs? e = null) {
            Track = track;
            Mea = e;
         }
      }

      public class MapPolygonEventArgs {
         public readonly MapPolygon Polygon;

         public readonly MouseEventArgs? Mea;

         public MapPolygonEventArgs(MapPolygon polygon, MouseEventArgs? e = null) {
            Polygon = polygon;
            Mea = e;
         }
      }

      public class SelectionChangeEventArgs {
         public readonly RectLatLng Selection;

         public readonly bool ZoomToFit;

         public SelectionChangeEventArgs(RectLatLng selection, bool zoomToFit) {
            Selection = selection;
            ZoomToFit = zoomToFit;
         }
      }

      #endregion

      /// <summary>
      /// wenn die Karte gezeichnet wurde: jetzt können noch zusätzliche Zeichnungen erfolgen
      /// </summary>
      public event EventHandler<DrawExtendedEventArgs>? M_DrawOnTop;

      /// <summary>
      /// <see cref="M_MinZoom"/> oder <see cref="M_MaxZoom"/> wurde verändert
      /// </summary>
      public event EventHandler? M_ZoomRangeChanged;

      /// <summary>
      /// Mausklick oder -move
      /// </summary>
      public event EventHandler<MapMouseEventArgs>? M_Mouse;

      /// <summary>
      /// Enter, Leave oder Klick auf Track
      /// </summary>
      public event EventHandler<TrackEventArgs>? M_Track;

      /// <summary>
      /// Enter, Leave oder Klick auf Marker
      /// </summary>
      public event EventHandler<MarkerEventArgs>? M_Marker;

      /// <summary>
      /// when current position is changed
      /// </summary>
      public event EventHandler<PositionChangedEventArgs>? M_PositionChanged;

      /// <summary>
      /// on map drag
      /// </summary>
      public event EventHandler? M_Drag;

      /// <summary>
      /// wenn der Zoom des Core (also ganzzahlig!) geändert wurde
      /// </summary>
      public event EventHandler? M_NonFracionalZoomChanged;

      /// <summary>
      /// wenn der Zoom nichtganzzahlig (!) geändert wurde 
      /// (d.h. der Core ist NICHT beteiligt, also KEIN <see cref="M_NonFracionalZoomChanged"/>!)
      /// </summary>
      public event EventHandler? M_FracionalZoomChanged;

      /// <summary>
      /// wenn der Zoom geändert wurde (vgl. <see cref="M_NonFracionalZoomChanged"/> und <see cref="M_FracionalZoomChanged"/>)
      /// </summary>
      public event EventHandler? M_ZoomChanged;

      /// <summary>
      /// on empty tile displayed
      /// </summary>
      public event EventHandler<EmptyTileErrorEventArgs>? M_EmptyTileError;

      /// <summary>
      /// nur zur internen Verwendung (Threadsicherheit)
      /// </summary>
      public event EventHandler<TileLoadEventArgs>? g_TileLoadCompleteEvent;

      /// <summary>
      /// tile set is starting to load
      /// </summary>
      public event EventHandler? M_TileLoadStart;

      /// <summary>
      /// when tile set load is complete
      /// </summary>
      public event EventHandler<TileLoadCompleteEventArgs>? M_TileLoadComplete;

      /// <summary>
      /// die Anzahl der noch zu ladenden Tiles hat sich verändert
      /// </summary>
      public event EventHandler<TileLoadChangeEventArgs>? M_TileLoadChange;

      /// <summary>
      /// on map type changed
      /// </summary>
      public event EventHandler<MapTypeChangedEventArgs>? M_TypeChanged;

      /// <summary>
      /// clicked on marker
      /// </summary>
      event EventHandler<MapMarkerEventArgs>? M_MarkerClick;

      /// <summary>
      /// double clicked on marker
      /// </summary>
      event EventHandler<MapMarkerEventArgs>? M_MarkerDoubleClick;

      /// <summary>
      /// mouse enters marker area
      /// </summary>
      event EventHandler<MapMarkerEventArgs>? M_MarkerEnter;

      /// <summary>
      /// mouse leaves marker area
      /// </summary>
      event EventHandler<MapMarkerEventArgs>? M_MarkerLeave;

      /// <summary>
      /// clicked on track
      /// </summary>
      event EventHandler<MapTrackEventArgs>? M_TrackClick;

      /// <summary>
      /// double clicked on track
      /// </summary>
      event EventHandler<MapTrackEventArgs>? M_TrackDoubleClick;

      /// <summary>
      /// mouse enters track area
      /// </summary>
      event EventHandler<MapTrackEventArgs>? M_TrackEnter;

      /// <summary>
      /// mouse leaves track area
      /// </summary>
      event EventHandler<MapTrackEventArgs>? M_TrackLeave;

      /// <summary>
      /// clicked on polygon
      /// </summary>
      event EventHandler<MapPolygonEventArgs>? M_PolygonClick;

      /// <summary>
      /// double clicked on polygon
      /// </summary>
      event EventHandler<MapPolygonEventArgs>? M_PolygonDoubleClick;

      /// <summary>
      /// mouse enters Polygon area
      /// </summary>
      public event EventHandler<MapPolygonEventArgs>? M_PolygonEnter;

      /// <summary>
      /// mouse leaves Polygon area
      /// </summary>
      public event EventHandler<MapPolygonEventArgs>? M_MapPolygonLeave;

      /// <summary>
      /// mouse selection is changed
      /// </summary>
      public event EventHandler<SelectionChangeEventArgs>? M_SelectionChange;

      /// <summary>
      /// an exception is thrown inside the map control
      /// </summary>
      public event ExceptionThrown? M_ExceptionThrown;

      public event EventHandler<ExceptionThrownEventArgs>? M_InnerExceptionThrown;

      /// <summary>
      /// Selektion mit <see cref="M_SetAreaSelectionEndPoint"/> beendet
      /// </summary>
      public event EventHandler<MouseEventArgs>? M_SetAreaSelectionEndPointEvent;

#if GMAP4SKIA

      // Im Nicht-Windows-System ex. die folgenden Events noch nicht. Sie werden def. und bei Bedarf selbst ausgelöst.

      public event EventHandler<PaintEventArgs>? Paint;

      public event EventHandler<MouseEventArgs>? MouseClick;

      public event EventHandler<MouseEventArgs>? MouseDoubleClick;

      /// <summary>
      /// Tritt ein, wenn sich der Mauszeiger über dem Steuerelement befindet und eine Maustaste gedrückt wird.
      /// </summary>
      public event EventHandler<MouseEventArgs>? MouseDown;

      /// <summary>
      /// Tritt ein, wenn sich der Mauszeiger über dem Steuerelement befindet und eine Maustaste losgelassen wird.
      /// </summary>
      public event EventHandler<MouseEventArgs>? MouseUp;

      /// <summary>
      /// Tritt ein, wenn der Mauszeiger über dem Steuerelement bewegt wird.
      /// </summary>
      public event EventHandler<MouseEventArgs>? MouseMove;

      /// <summary>
      /// Tritt ein, wenn der Mauszeiger den Bereich des Steuerelements verlässt.
      /// </summary>
      public event EventHandler? MouseLeave;

#endif

      #endregion

      #region privat Vars (g_...)

      #region Zoom/Scaling

      /// <summary>
      /// zusätzlicher Zoom zum ganzzahligen Zoom von <see cref="PublicCore.Zoom"/> (1.0 .. 2.0)?
      /// </summary>
      double g_extendedFractionalZoom = 1;

      /// <summary>
      /// Gesamtfaktor für die Darstellung auf einem realen Device
      /// </summary>
      double g_scale4device => g_extendedFractionalZoom * M_DeviceZoom;

      #endregion

      /* Mehrfache Auswertungen z.B. eines Mausklicks können nicht so einfach verhindert werden. 
        * Wenn z.B. der Mausklick auf einen Marker und einen Track erfolgt, erhalten BEIDE diesen
        * Klick. Andererseits wird der Klick NICHT für weitere Marker/Tracks ausgewertet.
        */

      /// <summary>
      /// Pos. des letzten ausgewerteten Klicks
      /// </summary>
      Point g_ptUsedLastClick = new Point();

      /// <summary>
      /// Maßstabanzeige für das <see cref="SpecialMapCtrl"/>
      /// </summary>
      Scale4Map? g_scale;

      /// <summary>
      /// Overlay für die GPX-Daten
      /// </summary>
      readonly MapOverlay g_gpxReadOnlyOverlay = new MapOverlay("GPXro");
      readonly MapOverlay g_gpxOverlay = new MapOverlay("GPX");
      readonly MapOverlay g_gpxSelectedPartsOverlay = new MapOverlay("GPXselparts");

      /// <summary>
      /// map boundaries
      /// </summary>
      RectLatLng? g_boundsOfMap = null;

#if !GMAP4SKIA
      /// <summary>
      /// prevents focusing map if mouse enters it's area
      /// </summary>
      bool g_DisableFocusOnMouseEnter = true;

      /// <summary>
      /// reverses MouseWheel zooming direction
      /// </summary>
      bool g_InvertedMouseWheelZooming = false;

      /// <summary>
      /// lets you zoom by MouseWheel even when pointer is in area of marker
      /// </summary>
      bool g_IgnoreMarkerOnMouseWheel = false;
#endif

      PointLatLng g_selectionStart;
      PointLatLng g_selectionEnd;

      readonly Font g_MissingDataFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
      readonly StringFormat g_centerFormat = new StringFormat();
      readonly StringFormat g_bottomFormat = new StringFormat();
      bool g_IsSelected;

#if !GMAP4SKIA
      readonly ImageAttributes g_tileFlipXYAttributes = new ImageAttributes();
      Cursor? g_cursorBefore = Cursors.Default;
#endif

#if GMAP4SKIA
      /// <summary>
      /// Gets the width and height of a rectangle centered on the point the mouse button was pressed, within which a drag operation will not begin.
      /// </summary>
      readonly Size g_DragSize = new Size(5, 5);
#else
      /// <summary>
      /// Gets the width and height of a rectangle centered on the point the mouse button was pressed, within which a drag operation will not begin.
      /// </summary>
      readonly Size g_DragSize = SystemInformation.DragSize;
#endif

      Graphics? g_GraphicsBackBuffer = null;

      readonly PublicCore g_core = new PublicCore();

      PointLatLng g_clientLeftTop = new PointLatLng(90, -180),
                  g_clientRightBottom = new PointLatLng(-90, 180);

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      RectLatLng g_ViewArea => g_core.ViewArea;

      /// <summary>
      /// Ist der Mapservice bereit?
      /// </summary>
      bool g_ServiceIsReady => g_core != null && g_core.IsStarted;

      /// <summary>
      /// if true, selects area just by holding mouse and moving
      /// </summary>
      bool g_DisableAltForSelection = false;

      static readonly bool g_IsDesignerHosted =
#if GMAP4SKIA
         // Damit wurde ursprünglich das Control im Design-Modus passiv gehalten. Jetzt fkt. allerdings der Debug-Modus damit nicht mehr.
         false;  // = Xamarin.Forms.DesignMode.IsDesignModeEnabled ???
#else
         LicenseManager.UsageMode == LicenseUsageMode.Designtime;
#endif

      /// <summary>
      /// gets map manager (<see cref="GMaps.Instance"/>)
      /// </summary>
      GMaps g_Manager => GMaps.Instance;

      /// <summary>
      /// list of overlays, should be thread safe
      /// </summary>
      readonly ObservableCollectionThreadSafe<MapOverlay> g_Overlays = new ObservableCollectionThreadSafe<MapOverlay>();

      bool _showCoreDataForTest = false;

      /// <summary>
      /// nur für Test: zeigt einige akt. Daten des Core auf dem Bildschirm an
      /// </summary>
      bool g_ShowCoreData4Test {
         get => _showCoreDataForTest;
         set {
            if (_showCoreDataForTest != value) {
               _showCoreDataForTest = value;
               M_CoreInvalidate();
            }
         }
      }

      /// <summary>
      /// Stift für Mittelpunktmarkierung
      /// </summary>
      Pen g_CenterPen = new Pen(Color.Red,
#if GMAP4SKIA
         5
#else
         1
#endif
         );

      #endregion

      #region public/internal Props/Vars (M_...)

      [Category("SpecMAp")]
      [Description("map scale type")]
      public ScaleModes M_ScaleMode { get; set; } = ScaleModes.Integer;

      double _deviceZoom = 1;

      /// <summary>
      /// zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (1.0 ...)
      /// </summary>
      [Category("SpecMAp")]
      [Description("extended zoom for real device")]
      [DefaultValue(1)]
      public double M_DeviceZoom {
         get => _deviceZoom;
         set {
            if (0 < value) {
               _deviceZoom = value;
               OnSizeChanged(EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// min. Zoom für die Karte
      /// </summary>
      [Category("SpecMAp")]
      [Description("minimum zoom level of map")]
      [DefaultValue(2)]
      public int M_MinZoom {
         get => g_core.MinZoom;
         set {
            if (g_core.MinZoom != value) {
               g_core.MinZoom = value;
               M_ZoomRangeChanged?.Invoke(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// max. Zoom für die Karte
      /// </summary>
      [Category("SpecMAp")]
      [Description("maximum zoom level of map")]
      [DefaultValue(24)]
      public int M_MaxZoom {
         get => g_core.MaxZoom;
         set {
            if (g_core.MaxZoom != value) {
               g_core.MaxZoom = value;
               M_ZoomRangeChanged?.Invoke(this, new EventArgs());
            }
         }
      }

      double _zoom;

      /// <summary>
      /// akt. Zoom (exponentiell, d.h. +1 bedeutet Verdopplung); kann je nach <see cref="SpecMapScaleMode"/> ev. nur ganzzahlig sein
      /// </summary>
      [Category("SpecMAp")]
      [DefaultValue(12)]
      public double M_Zoom => _zoom;

      /// <summary>
      /// (linearer) Zoomfaktor von <see cref="SpecMapZoom"/> (bezogen auf <see cref="M_MinZoom"/>)
      /// </summary>
      [Category("SpecMAp")]
      [Browsable(false)]
      public double M_ZoomLinear => zoomLinear(M_Zoom);

      public double M_MinZoomLinear => zoomLinear(M_MinZoom);

      public double M_MaxZoomLinear => zoomLinear(M_MaxZoom);

      /// <summary>
      /// current map center position (lat/lgn)
      /// </summary>
      [Category("SpecMAp")]
      [Browsable(false)]
      public PointLatLng M_Position => g_core.Position;

      /// <summary>
      /// geogr. Länge des Mittelpunktes der Karte
      /// </summary>
      [Category("SpecMAp")]
      [Browsable(false)]
      public double M_CenterLon => M_Position.Lng;

      /// <summary>
      /// geogr. Breite des Mittelpunktes der Karte
      /// </summary>
      [Category("SpecMAp")]
      [Browsable(false)]
      public double M_CenterLat => M_Position.Lat;

      /// <summary>
      /// Zoom auch per Mausrad?
      /// </summary>
      public bool M_MouseWheelZoomEnabled { get; set; } = true;


      /// <summary>
      /// is tracks enabled
      /// </summary>
      [Category("SpecMAp")]
      public bool M_TracksEnabled {
         get => g_core.RoutesEnabled;
         set => g_core.RoutesEnabled = value;
      }

      /// <summary>
      /// is polygons enabled
      /// </summary>
      [Category("SpecMAp")]
      public bool M_PolygonsEnabled {
         get => g_core.PolygonsEnabled;
         set => g_core.PolygonsEnabled = value;
      }

      /// <summary>
      /// is markers enabled
      /// </summary>
      [Category("SpecMAp")]
      public bool M_MarkersEnabled {
         get => g_core.MarkersEnabled;
         set => g_core.MarkersEnabled = value;
      }


      /// <summary>
      /// akt. Kartenprovider
      /// </summary>
      [Category("SpecMAp")]
      [Browsable(false)]
      public GMapProvider M_Provider {
         get => g_core.Provider;
         protected set {
            if (g_core.Provider == null ||                                                                           // kein Provider gesetzt
                !g_core.Provider.Equals(value) ||                                                                    // anderer Provider
                (g_core.Provider is MultiUseBaseProvider &&                                                          // alter und neuer Provider sind MultiUseBaseProvider und
                 g_core.Provider.Equals(value) &&                                                                    //    gleich (auch gleicher Providertyp) und
                 ((MultiUseBaseProvider)g_core.Provider).DeltaDbId != ((MultiUseBaseProvider)value).DeltaDbId)) {    //    haben unterschiedliche DeltaDbId
               var viewarea = g_selectionData.selectedArea;

               if (viewarea != RectLatLng.Empty) {
                  M_SetLocation(viewarea.Lng + viewarea.WidthLng / 2,
                                viewarea.Lat - viewarea.HeightLat / 2);
               } else {
                  viewarea = g_ViewArea;
               }

               g_core.Provider = value;

               if (g_core.IsStarted) {
                  if (g_core.ZoomToArea) {
                     // restore zoomrect as close as possible
                     if (viewarea != RectLatLng.Empty && viewarea != g_ViewArea) {
                        int bestZoom = g_core.GetMaxZoomToFitRect(viewarea);
                        if (bestZoom > 0 && M_Zoom != bestZoom) {
                           M_SetZoom(bestZoom);
                        }
                     }
                  } else {
                     forceUpdateOverlays();
                  }
               }
            }
         }
      }

      /// <summary>
      /// can user drag map
      /// </summary>
      [Category("SpecMAp")]
      public bool M_CanDragMap {
         get => g_core.CanDragMap;
         set => g_core.CanDragMap = value;
      }

      /// <summary>
      /// max. Entfernung (in Clientkoordinaten) von einem Track um einen Klick als Treffer zu werten
      /// </summary>
      public float M_ClickTolerance4Tracks { get; set; } = 1F;

      /// <summary>
      /// how many levels of tiles are staying decompresed in memory
      /// </summary>
      [Browsable(false)]
      public int M_LevelsKeepInMemory {
         get => g_core.LevelsKeepInMemory;
         set => g_core.LevelsKeepInMemory = value;
      }

      /// <summary>
      /// map zooming type for mouse wheel
      /// </summary>
      [Category("SpecMAp")]
      [Description("map zooming type for mouse wheel")]
      public MouseWheelZoomType M_MouseWheelZoomType {
         get => g_core.MouseWheelZoomType;
         set => g_core.MouseWheelZoomType = value;
      }

      /// <summary>
      /// retry count to get tile
      /// </summary>
      [Browsable(false)]
      public int M_RetryLoadTile {
         get => g_core.RetryLoadTile;
         set => g_core.RetryLoadTile = value;
      }

      /// <summary>
      /// map dragg button
      /// </summary>
      [Category("SpecMAp")]
      public MouseButtons M_DragButton { get; set; } = MouseButtons.Right;

      /// <summary>
      /// Wird gerade ein Auswahlrechteck gezeichnet? (mit <see cref="SpecMapStartSelectionArea"/>() gestartet)
      /// </summary>
      public bool M_SelectionAreaIsStarted { get; protected set; } = false;

      public static int M_ThreadPoolSize {
         get => PublicCore.ThreadPoolSize;
         set => PublicCore.ThreadPoolSize = value;
      }

      /// <summary>
      /// Liste der registrierten Karten-Provider
      /// </summary>
      public List<MapProviderDefinition> M_ProviderDefinitions { get; protected set; } = new List<MapProviderDefinition>();

      /// <summary>
      /// akt. Kartenindex in der Liste <see cref="SpecMapProviderDefinitions"/>
      /// </summary>
      public int M_ActualMapIdx { get; protected set; } = -1;

      /// <summary>
      /// letzte registrierte Mausposition im Karten-Control
      /// </summary>
      public Point M_LastMouseLocation { get; protected set; } = Point.Empty;

      public bool M_IsDragging { get; protected set; }

      /// <summary>
      /// auch den Cache verwenden oder nur den Karten-Server (gilt global)
      /// </summary>
      public static bool M_CacheIsActiv {
         get => GMaps.Instance.Mode != AccessMode.ServerOnly;
         set => GMaps.Instance.Mode = value ?
                                          AccessMode.ServerAndCache :
                                          AccessMode.ServerOnly;
      }

      public string M_CacheLocation {
         get {
#if !DESIGN
            return PublicCore.MapCacheLocation;
#else
            return string.Empty;
#endif
         }
         set {
#if !DESIGN
            PublicCore.MapCacheLocation = value;
#endif
         }
      }

#if !GMAP4SKIA
      /// <summary>
      /// Cursor der Karte
      /// </summary>
      public Cursor M_Cursor {
         get => base.Cursor;
         set {
            if (!base.Cursor.Equals(value)) {
               if (g_cursorBefore != null)   // der Cursor wird gerade gesetzt, wenn die Maus intern temp. verändert wurde
                  g_cursorBefore = value;
               else
                  base.Cursor = value;
            }
         }
      }
#endif

      /// <summary>
      /// Mittelpunkt anzeigen
      /// </summary>
      public bool M_ShowCenter { get; set; } = false;

      #region Def. einiger Anzeigen

      /// <summary>
      /// backgroundcolor for map
      /// </summary>
      [Category("SpecMAp")]
      public Color M_EmptyMapBackgroundColor { get; set; } = Color.WhiteSmoke;

      #region Def. der Darstellung für leere Tiles

      /// <summary>
      /// enables filling empty tiles using lower level images
      /// </summary>
      [Category("SpecMAp")]
      public bool M_FillEmptyTiles { get => g_core.FillEmptyTiles; set => g_core.FillEmptyTiles = value; }

      /// <summary>
      /// text on empty tiles
      /// </summary>
      [Category("SpecMAp")]
      public string M_EmptyTileText { get; set; } = "no image";

      [Category("SpecMAp")]
      public Font M_EmptyTileFont = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold);

      Color _emptyTileColor = Color.Navy;
      Brush? _emptyTileBrush = new SolidBrush(Color.Navy);

      /// <summary>
      /// color of empty tile background
      /// </summary>
      [Category("SpecMAp")]
      [Description("background color of the empty tile")]
      public Color M_EmptyTileColor {
         get => _emptyTileColor;
         set {
            if (_emptyTileColor != value) {
               _emptyTileColor = value;

               if (_emptyTileBrush != null) {
                  _emptyTileBrush.Dispose();
                  _emptyTileBrush = null;
               }

               _emptyTileBrush = new SolidBrush(_emptyTileColor);
            }
         }
      }

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
      [Category("SpecMAp")]
      public Pen M_EmptyTileBordersPen = new Pen(Brushes.White, 1);

      #endregion

      #region Def. für Darstellung Auswahl

      /// <summary>
      /// area selection pen
      /// </summary>
      [Category("SpecMAp")]
      public Pen M_SelectionPen = new Pen(Brushes.Blue, 10);

      Brush? _selectedAreaFillBrush = new SolidBrush(Color.FromArgb(33, Color.RoyalBlue));

      Color _selectedAreaFillColor = Color.FromArgb(33, Color.RoyalBlue);

      /// <summary>
      /// background of selected area
      /// </summary>
      [Category("SpecMAp")]
      [Description("background color od the selected area")]
      public Color M_SelectedAreaFillColor {
         get => _selectedAreaFillColor;
         set {
            if (_selectedAreaFillColor != value) {
               _selectedAreaFillColor = value;

               if (_selectedAreaFillBrush != null) {
                  _selectedAreaFillBrush.Dispose();
                  _selectedAreaFillBrush = null;
               }

               _selectedAreaFillBrush = new SolidBrush(_selectedAreaFillColor);
            }
         }
      }

      #endregion

      #region Def. für Grid-Linien

      bool _showTileGridLines = false;

      /// <summary>
      /// shows tile gridlines
      /// </summary>
      [Category("SpecMAp")]
      [Description("shows tile gridlines")]
      public bool M_ShowTileGridLines {
         get => _showTileGridLines;
         set {
            _showTileGridLines = value;
            M_CoreInvalidate();
         }
      }

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
      [Category("SpecMAp")]
      public Pen M_TileGridLinesPen = new Pen(Brushes.White, 5);

      [Category("SpecMAp")]
      public Font M_TileGridLinesFont = new Font(FontFamily.GenericSansSerif,
#if GMAP4SKIA
                                                  35,
#else
                                                  7,
#endif
                                                  FontStyle.Bold);

      #endregion

      [Category("SpecMAp")]
      public Font M_CopyrightFont { get; set; } = new Font(FontFamily.GenericSansSerif,
#if GMAP4SKIA
                                              35,
#else
                                              7,
#endif
                                              FontStyle.Regular);

      /// <summary>
      /// Alphawert für Maßstabsanzeige (erst nach Abarbeitung des Konstruktors setzbar)
      /// </summary>
      public byte M_ScaleAlpha {
         get => g_scale != null ? g_scale.Alpha : (byte)180;
         set { if (g_scale != null) g_scale.Alpha = value; }
      }

      /// <summary>
      /// Maßstabsart (erst nach Abarbeitung des Konstruktors setzbar)
      /// </summary>
      public Scale4Map.ScaleKind M_ScaleKind {
         get => g_scale != null ? g_scale.Kind : Scale4Map.ScaleKind.Around;
         set { if (g_scale != null) g_scale.Kind = value; }
      }

      #endregion

      #region internal Props / Vars

      /// <summary>
      /// stops immediate marker/track/polygon invalidations;
      /// call Refresh to perform single refresh and reset invalidation state
      /// </summary>
      [Browsable(false)]
      internal bool M_HoldInvalidation;

      bool _isMouseOverMarker;
      int _overObjectCount;

      /// <summary>
      /// is mouse over marker
      /// </summary>
      [Browsable(false)]
      internal bool M_IsMouseOverMarker {
         get => _isMouseOverMarker;
         set {
            _isMouseOverMarker = value;
            _overObjectCount += value ? 1 : -1;
         }
      }

      bool _isMouseOverTrack;

      /// <summary>
      /// is mouse over track
      /// </summary>
      [Browsable(false)]
      internal bool M_IsMouseOverTrack {
         get => _isMouseOverTrack;
         set {
            _isMouseOverTrack = value;
            _overObjectCount += value ? 1 : -1;
         }
      }

      bool _isMouseOverPolygon;

      /// <summary>
      /// is mouse over polygon
      /// </summary>
      [Browsable(false)]
      internal bool M_IsMouseOverPolygon {
         get => _isMouseOverPolygon;
         set {
            _isMouseOverPolygon = value;
            _overObjectCount += value ? 1 : -1;
         }
      }

      #endregion

      #endregion


      static SpecialMapCtrl() {
         if (!g_IsDesignerHosted) {
            MapImageProxy.Enable();
            GMaps.Instance.SQLitePing();
         }
      }

      /// <summary>
      /// parameterloser Konstruktor (wird für XAML benötigt)
      /// </summary>
      public SpecialMapCtrl() : this(LanguageType.German) { }

      /// <summary>
      /// Standardkonstruktor für beliebige Kartensprache für die Provider (Google)
      /// </summary>
      public SpecialMapCtrl(GMap.NET.LanguageType language) {
         if (!g_IsDesignerHosted) {
#if GMAP4SKIA
            Font = new Font("Arial", 35);
#else
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            ResizeRedraw = true;

            g_tileFlipXYAttributes.SetWrapMode(WrapMode.TileFlipXY);

            g_centerFormat.Alignment = StringAlignment.Center;
            g_centerFormat.LineAlignment = StringAlignment.Center;
            g_bottomFormat.Alignment = StringAlignment.Center;
            g_bottomFormat.LineAlignment = StringAlignment.Far;

            if (GMaps.Instance.IsRunningOnMono) // no imports to move pointer
               M_MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;

#endif

            g_core.OnCurrentPositionChanged += (PointLatLng point) =>
                           M_PositionChanged?.Invoke(this, new PositionChangedEventArgs(point));
            g_core.OnEmptyTileError += (int zoom, GPoint pos) =>
                           M_EmptyTileError?.Invoke(this, new EmptyTileErrorEventArgs(zoom, pos));
            g_core.OnMapDrag += () => M_Drag?.Invoke(this, EventArgs.Empty);
            g_core.OnMapTypeChanged += (GMapProvider type) => M_TypeChanged?.Invoke(this, new MapTypeChangedEventArgs(type));
            g_core.OnMapZoomChanged += () => M_NonFracionalZoomChanged?.Invoke(this, EventArgs.Empty);
            g_core.OnTileLoadComplete += tileLoadChanged;
            g_core.OnTileLoadStart += () => tileLoadChanged(-1);

            g_Overlays.CollectionChanged += overlays_CollectionChanged;
         }

         M_RetryLoadTile = 0;
         M_FillEmptyTiles = false;

         GMapProvider.Language = language;
      }

      #region public/internal Funktionen (M_...)

      /// <summary>
      /// wird ausgelöst wenn der Core ein Neuzeichnen veranlasst oder direkt
      /// </summary>
      public void M_Refresh() {
         // die letzte Refreshzeit wird im Core registriert
         // (normalerweise über g_core.StartRefresh() gesetzt)
         g_core.LastInvalidation = DateTime.Now;
         controlRefresh();
      }

      /// <summary>
      /// gets image of the current view
      /// </summary>
      /// <returns></returns>
      public
#if GMAP4SKIA
             Bitmap
#else
             Image
#endif
         M_ToImage() {
         try {
            updateBackBuffer();
            M_Refresh();
#if !GMAP4SKIA
            Application.DoEvents();
#endif
            if (_backBuffer != null)
               using (var ms = new MemoryStream()) {
                  using (var frame = (Bitmap)_backBuffer.Clone())
                     frame.Save(ms, ImageFormat.Png);
                  return
#if GMAP4SKIA
                     Bitmap.FromStream(ms);
#else
                     Image.FromStream(ms);
#endif
               }
            throw new Exception(nameof(M_ToImage) + "()");
         } catch (Exception) {
            throw;
         } finally {
            clearBackBuffer();
         }
      }

      #region Handling Map-Provider

      /// <summary>
      /// registriert die zu verwendenden Karten-Provider in der Liste <see cref="SpecMapProviderDefinitions"/>
      /// </summary>
      /// <param name="providernames"></param>
      /// <param name="garmindefs"></param>
      /// <param name="wmsdefs"></param>
      /// <param name="kmzdefs"></param>
      public void M_RegisterProviders(IList<string> providernames,
                                      List<MapProviderDefinition> provdefs) {
         M_ProviderDefinitions.Clear();
         M_Provider = GMapProviders.EmptyProvider;

         if (providernames != null)
            for (int i = 0; i < providernames.Count; i++) {
               MapProviderDefinition def = provdefs[i];
               def.Provider = ProviderHelper.GetProvider4Providername(providernames[i]);

               if (providernames[i] == def.ProviderName &&
                   def is GarminProvider.GarminMapDefinition) {

                  M_ProviderDefinitions.Add((GarminProvider.GarminMapDefinition)def);
                  if (GarminProvider.GarminImagecreator == null)
                     GarminProvider.GarminImagecreator = new GarminImageCreator.ImageCreator();

               } else if (providernames[i] == def.ProviderName &&
                          def is GarminKmzProvider.KmzMapDefinition) {

                  M_ProviderDefinitions.Add((GarminKmzProvider.KmzMapDefinition)def);

               } else if (providernames[i] == def.ProviderName &&
                          def is WMSProvider.WMSMapDefinition) {

                  M_ProviderDefinitions.Add((WMSProvider.WMSMapDefinition)def);

               } else if (providernames[i] == def.ProviderName &&
                          def is HillshadingProvider.HillshadingMapDefinition) {

                  M_ProviderDefinitions.Add((HillshadingProvider.HillshadingMapDefinition)def);

               } else if (providernames[i] == def.ProviderName &&
                          def is MultiMapProvider.MultiMapDefinition) {

                  MultiMapProvider.MultiMapDefinition specdef = (MultiMapProvider.MultiMapDefinition)def;
                  for (int j = 0; j < specdef.Layer; j++)
                     specdef.MapProviderDefinitions[j].Provider = ProviderHelper.GetProvider4Providername(specdef.MapProviderDefinitions[j].ProviderName);

                  M_ProviderDefinitions.Add(specdef);

               } else {

                  for (int p = 0; p < GMapProviders.List.Count; p++) {
                     if (GMapProviders.List[p].Name == def.ProviderName) {

                        M_ProviderDefinitions.Add(def);
                        break;

                     }
                  }

               }
            }

         if (M_ProviderDefinitions.Count == 0)
            return;
         M_ActualMapIdx = -1;
      }

      /// <summary>
      /// setzt den aktiven Karten-Provider entsprechend dem Index und der Liste der <see cref="GarminProvider.GarminMapDefinition"/>
      /// </summary>
      /// <param name="idx">Index für die <see cref="SpecMapProviderDefinitions"/></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      /// <param name="zoom4display">ev. wegen Bildschirm-DPI größer als 1 wählen</param>
      public async Task M_SetActivProviderAsync(int idx, int demalpha, DemData? dem, double zoom4display) {
         if (M_ProviderDefinitions.Count == 0)
            return;
         M_ActualMapIdx = Math.Max(0, Math.Min(idx, M_ProviderDefinitions.Count - 1));

         MapProviderDefinition def = M_ProviderDefinitions[M_ActualMapIdx];

         g_gpxReadOnlyOverlay.IsVisibile = g_gpxOverlay.IsVisibile = false;

         // ev. Zoom behandeln
         double newzoom = -1;
         if (M_Zoom < def.MinZoom || def.MaxZoom < M_Zoom) {
            newzoom = Math.Min(Math.Max(M_Zoom, def.MinZoom), def.MaxZoom);
         }

         GMapProvider newprov = prepareProviderSpecial(def, demalpha, dem);

         // jetzt wird der neue Provider und ev. auch der Zoom gesetzt
         M_DeviceZoom = zoom4display;
         M_Provider = newprov;
         M_MinZoom = def.MinZoom;
         M_MaxZoom = def.MaxZoom;
         if (newzoom >= 0)
            await M_SetZoomAsync(newzoom);
         M_Refresh(true, true, false, false);

         g_gpxReadOnlyOverlay.IsVisibile = g_gpxOverlay.IsVisibile = true; // ohne false/true-Wechsel passt die Darstellung des Overlays manchmal nicht zur Karte
      }

      GMapProvider prepareProviderSpecial(MapProviderDefinition def, int demalpha, DemData? dem) {
         GMapProvider newprov = def.Provider;
         int dbidelta = -1;

         if (newprov is GMapProviderWithHillshade) {

            GMapProviderWithHillshade prov = (GMapProviderWithHillshade)newprov;
            prov.DEM = dem;
            prov.Alpha = demalpha;

         }

         if (newprov is GarminProvider) {

            GarminProvider.GarminMapDefinition? specdef = def as GarminProvider.GarminMapDefinition;
            if (specdef != null) {
               dbidelta = specdef.DbIdDelta;
               trick4SameProvider(newprov, dbidelta);
               GarminProvider prov = (GarminProvider)newprov;
               int newdbid = prov.StandardDbId + dbidelta;
               prov.ChangeDbId(newdbid);
               prov.SetDef(specdef);
               if (prov is IHasJobManager)
                  ((IHasJobManager)prov).SetJobFilter([dbidelta]);
               FilecacheManager.WriteCacheInfo(newdbid, specdef.MapName, newprov.Id);
            }

         } else if (newprov is WMSProvider) {

            WMSProvider.WMSMapDefinition specdef = (WMSProvider.WMSMapDefinition)def;
            if (specdef != null) {
               dbidelta = specdef.DbIdDelta;
               trick4SameProvider(newprov, dbidelta);
               WMSProvider prov = (WMSProvider)newprov;
               int newdbid = prov.StandardDbId + dbidelta;
               prov.ChangeDbId(newdbid);
               prov.SetDef(specdef);
               if (prov is IHasJobManager)
                  ((IHasJobManager)prov).SetJobFilter([dbidelta]);
               FilecacheManager.WriteCacheInfo(newdbid, def.MapName, newprov.Id);
            }

         } else if (newprov is GarminKmzProvider) {

            GarminKmzProvider.KmzMapDefinition specdef = (GarminKmzProvider.KmzMapDefinition)def;
            if (specdef != null) {
               dbidelta = specdef.DbIdDelta;
               trick4SameProvider(newprov, dbidelta);
               GarminKmzProvider prov = (GarminKmzProvider)newprov;
               int newdbid = prov.StandardDbId + dbidelta;
               prov.ChangeDbId(newdbid);
               prov.SetDef(specdef);
               if (prov is IHasJobManager)
                  ((IHasJobManager)prov).SetJobFilter([dbidelta]);
               FilecacheManager.WriteCacheInfo(newdbid, def.MapName, newprov.Id);
            }

         } else if (newprov is HillshadingProvider) {

            HillshadingProvider.HillshadingMapDefinition specdef = (HillshadingProvider.HillshadingMapDefinition)def;
            if (specdef != null) {
               dbidelta = specdef.DbIdDelta;
               trick4SameProvider(newprov, dbidelta);
               HillshadingProvider prov = (HillshadingProvider)newprov;
               int newdbid = prov.StandardDbId + dbidelta;
               prov.ChangeDbId(newdbid);
               prov.SetDef(specdef);
               if (prov is IHasJobManager)
                  ((IHasJobManager)prov).SetJobFilter([dbidelta]);
               FilecacheManager.WriteCacheInfo(newdbid, def.MapName, newprov.Id);
            }

         } else if (newprov is MultiMapProvider) {

            MultiMapProvider.MultiMapDefinition specdef = (MultiMapProvider.MultiMapDefinition)def;
            if (specdef != null) {
               dbidelta = specdef.DbIdDelta;
               trick4SameProvider(newprov, dbidelta);
               MultiMapProvider prov = (MultiMapProvider)newprov;
               int newdbid = prov.StandardDbId + dbidelta;
               prov.ChangeDbId(newdbid);
               prov.SetDef(specdef);

               if (specdef.MapProviderDefinitions.Length > 0)
                  prov.SetNewProjection(specdef.MapProviderDefinitions[0].Provider.Projection);

               //if (prov is IHasJobManager)
               //   ((IHasJobManager)prov).SetJobFilter([dbidelta], g_core.Zoom);
               FilecacheManager.WriteCacheInfo(newdbid, def.MapName, newprov.Id);
            }

         } else {

            FilecacheManager.WriteCacheInfo(newprov.DbId, newprov.Name, newprov.Id);

         }

         if (newprov is IHasJobManager)
            ((IHasJobManager)newprov).SetJobFilter([dbidelta]);

         return newprov;
      }

      /// <summary>
      /// Problem:
      /// <para>
      /// Wenn ein <see cref="MultiUseBaseProvider"/> durch einen anderen <see cref="MultiUseBaseProvider"/> (des gleichen Typs) ersetzt wird, 
      /// wird diese Ersetzung im Core nicht erlaubt (nur wenn "if (_provider == null || !_provider.Equals(value))").
      /// </para>
      /// <para>
      /// Der Trick besteht darin, hier zwischenzeitlich den <see cref="GMapProviders.EmptyProvider"/> zu setzen.
      /// </para>
      /// </summary>
      /// <param name="newprov"></param>
      /// <param name="dbidelta"></param>
      void trick4SameProvider(GMapProvider newprov, int dbidelta) {
         if (M_Provider != null &&
             M_Provider.Equals(newprov) &&               // d.h. implizit auch: gleicher Providertyp
             M_Provider is MultiUseBaseProvider &&
             ((MultiUseBaseProvider)M_Provider).DeltaDbId != dbidelta)
            M_Provider = GMapProviders.EmptyProvider;
      }

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="SpecMapProviderDefinitions"/>-Liste 
      /// (ABER NICHT DEN KARTENINDEX wenn z.B. 2x der Garminprovider verwendet wird)
      /// </summary>
      /// <returns></returns>
      public int M_GetActiveProviderIdx() {
         if (M_Provider != null &&
             M_ProviderDefinitions != null)
            for (int i = 0; i < M_ProviderDefinitions.Count; i++) {
               if (M_Provider.Equals(M_ProviderDefinitions[i].Provider))
                  return i;
            }
         return -1;
      }

      #endregion

      #region Konvertierungfunktionen

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clientxy"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void M_Client2LonLat(int clientx, int clientxy, out double lon, out double lat) =>
         client2LonLat(clientx, clientxy, out lon, out lat);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <returns></returns>
      public PointD M_Client2LonLat(int clientx, int clienty) =>
         M_PointLatLng2PointD(fromLocalToLatLng(clientx, clienty));

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD M_Client2LonLat(Point ptclient) => M_Client2LonLat(ptclient.X, ptclient.Y);

      /// <summary>
      /// rechnet die Clientkoordinaten der Karte in geogr. Koordinaten um
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      public PointD M_Client2LonLat(GPoint ptclient) => M_Client2LonLat((int)ptclient.X, (int)ptclient.Y);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      public void M_LonLat2Client(double lon, double lat, out int clientx, out int clienty) =>
         M_LonLat2Client(lon, lat, out clientx, out clienty);

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public Point M_LonLat2Client(double lon, double lat) => M_LonLat2Client(new PointLatLng(lat, lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point M_LonLat2Client(Gpx.GpxTrackPoint ptgeo) => M_LonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point M_LonLat2Client(Gpx.GpxWaypoint ptgeo) => M_LonLat2Client(new PointLatLng(ptgeo.Lat, ptgeo.Lon));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point M_LonLat2Client(FSofTUtils.Geometry.PointD ptgeo) => M_LonLat2Client(new PointLatLng(ptgeo.Y, ptgeo.X));

      /// <summary>
      /// rechnet die geogr. Koordinaten in Clientkoordinaten der Karte um
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public Point M_LonLat2Client(PointLatLng ptgeo) {
         GPoint pt = fromLatLngToLocal(ptgeo);
         return new Point((int)pt.X, (int)pt.Y);
      }

      /// <summary>
      /// Umrechnung eines <see cref="PointLatLng"/> in einen <see cref="PointD"/>
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public static PointD M_PointLatLng2PointD(PointLatLng ptgeo) => new PointD(ptgeo.Lng, ptgeo.Lat);

      #endregion

      #region echte oder simulierte Mausklicks

      /// <summary>
      /// ein echter oder simulierter Mausklick erfolgt an dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void M_DoMouseClick(MouseEventArgs e) {
         g_ptUsedLastClick.X = int.MinValue;

         if (M_Mouse != null) {
            M_LastMouseLocation = e.Location;
            client2LonLat(e.X, e.Y, out double lon, out double lat);
            MapMouseEventArgs mme = new MapMouseEventArgs(MapMouseEventArgs.EventType.Click,
                                                          e.Button,
                                                          e.X,
                                                          e.Y,
                                                          lon,
                                                          lat);
            M_Mouse(this, mme);
            if (mme.IsHandled)
               g_ptUsedLastClick = e.Location;
         }
      }

      /// <summary>
      /// eine echte oder simulierte Mausbewegung erfolgt zu dieser Position
      /// </summary>
      /// <param name="e"></param>
      public void M_DoMouseMove(MouseEventArgs e) {
         if (M_Mouse != null) {
            PointLatLng ptlatlon = fromLocalToLatLng(e.X, e.Y);
            M_LastMouseLocation = e.Location;

            if (M_SelectionAreaIsStarted) {
               // DrawReversibleFrame() fkt. NICHT, da der Bildinhalt intern immer wieder akt. (überschrieben) wird!
               // daher diese (langsame aber ausreichende) Methode:
               if (g_selectionData.polygon != null)
                  g_gpxReadOnlyOverlay.Polygons.Remove(g_selectionData.polygon);
               if (g_selectionData.startPoint != PointLatLng.Empty) {
                  g_selectionData.polygon = buildSelectionRectangle(g_selectionData.startPoint, ptlatlon);
                  g_gpxReadOnlyOverlay.Polygons.Add(g_selectionData.polygon);
               }
            }

            M_Mouse(this,
                          new MapMouseEventArgs(MapMouseEventArgs.EventType.Move,
                                                e.Button,
                                                e.X,
                                                e.Y,
                                                ptlatlon.Lng,
                                                ptlatlon.Lat));
            g_ptUsedLastClick.X = int.MinValue;   // ein nachfolgender Klick ist dann immer "neu"
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
      public void M_DoMouseClick(int clientx,
                                      int clienty,
                                      bool all,
                                      MouseButtons button,
                                      out List<Marker> markerlst,
                                      out List<Track> tracklst) {
         tapped(clientx, clienty, false, button, all, out List<MapMarker> marker, out List<MapTrack> route, out List<MapPolygon> polygon);

         markerlst = new List<Marker>();
         tracklst = new List<Track>();

         foreach (var item in marker) {
            if (item != null && item is VisualMarker) {
               VisualMarker vm = (VisualMarker)item;
               if (vm.RealMarker != null)
                  markerlst.Add(vm.RealMarker);
            }
         }

         foreach (var item in route) {
            if (item != null && item is VisualTrack) {
               VisualTrack vt = (VisualTrack)item;
               if (vt.RealTrack != null)
                  tracklst.Add(vt.RealTrack);
            }
         }
      }

      #endregion

      #region Handling Auswahlrechteck

      static class g_selectionData {
         static public Cursor? cursorOrg = null;
         static public PointLatLng startPoint = PointLatLng.Empty;
         static public MapPolygon? polygon = null;
         /// <summary>
         /// merkt sich den Originalbutton während der Auswahl eines Auswahlrechtecks
         /// </summary>
         static public MouseButtons orgMapDragButton;
         /// <summary>
         /// akt. Rechteck der "Auswahl"
         /// </summary>
         static public RectLatLng selectedArea;

      }

      /// <summary>
      /// am Clientpunkt beginnt die Auswahl der Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void M_SetAreaSelectionStartPoint(MouseEventArgs e) {
         if (M_SelectionAreaIsStarted) {
            g_selectionData.startPoint = fromLocalToLatLng(e.X, e.Y);
            g_selectionData.polygon = buildSelectionRectangle(g_selectionData.startPoint, g_selectionData.startPoint);
            g_gpxReadOnlyOverlay.Polygons.Add(g_selectionData.polygon);
         }
      }

      /// <summary>
      /// am Clientpunkt ended die Auswahl des Auswahlrechteckes
      /// </summary>
      /// <param name="e"></param>
      public void M_SetAreaSelectionEndPoint(MouseEventArgs e) {
         if (M_SelectionAreaIsStarted)
            M_SetAreaSelectionEndPointEvent?.Invoke(this, e); // Ende der Eingabe simulieren
      }

      /// <summary>
      /// startet die Auswahl einer Fläche
      /// </summary>
      public void M_StartSelectionArea() {
         M_SelectionAreaIsStarted = true;
         g_selectionData.cursorOrg = Cursor;
         setCursor4Selection();
         g_selectionData.startPoint = GMap.NET.PointLatLng.Empty;
         // den Dragbutton für die Maus merken und deaktivieren
         g_selectionData.orgMapDragButton = M_DragButton;
         M_DragButton = MouseButtons.None;
      }

      /// <summary>
      /// liefert eine ausgewählte Fläche oder null
      /// </summary>
      /// <returns></returns>
      public Gpx.GpxBounds? M_EndSelectionArea() {
         M_SelectionAreaIsStarted = false;
         if (g_selectionData.cursorOrg != null)
            Cursor = g_selectionData.cursorOrg;
         M_DragButton = g_selectionData.orgMapDragButton;         // den Dragbutton für die Maus wieder aktivieren
         Gpx.GpxBounds? bounds = null;
         if (g_selectionData.polygon != null) {
            g_gpxReadOnlyOverlay.Polygons.Remove(g_selectionData.polygon);
            if (g_selectionData.polygon.Points[0].Lng != g_selectionData.polygon.Points[2].Lng &&
                g_selectionData.polygon.Points[0].Lat != g_selectionData.polygon.Points[2].Lat) // Ex. eine Fläche?
               bounds = new Gpx.GpxBounds(Math.Min(g_selectionData.polygon.Points[0].Lat, g_selectionData.polygon.Points[2].Lat),
                                          Math.Max(g_selectionData.polygon.Points[0].Lat, g_selectionData.polygon.Points[2].Lat),
                                          Math.Min(g_selectionData.polygon.Points[0].Lng, g_selectionData.polygon.Points[2].Lng),
                                          Math.Max(g_selectionData.polygon.Points[0].Lng, g_selectionData.polygon.Points[2].Lng));
            g_selectionData.polygon = null;
         }
         return bounds;
      }

      MapPolygon buildSelectionRectangle(PointLatLng start, PointLatLng end) =>
         new(new List<PointLatLng>() { start,
                                       new PointLatLng(start.Lat, end.Lng),
                                       end,
                                       new PointLatLng(end.Lat, start.Lng),
                                       start },
             "polySelection") {
            IsVisible = true,
            Stroke = new Pen(Color.LightGray, 0),
            Fill = new SolidBrush(Color.FromArgb(40, Color.Black)),
         };

      #endregion

      /// <summary>
      /// set a internet proxy
      /// </summary>
      /// <param name="proxy">proxy hostname (if null or empty <see cref="System.Net.WebRequest.DefaultWebProxy"/>)</param>
      /// <param name="proxyport">proxy portnumber</param>
      /// <param name="user">username</param>
      /// <param name="password">userpassword</param>
      public static void M_SetProxy(string proxy, int proxyport, string user, string password) {
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
      /// zeichnet die Karte neu und löscht ev. auch den Cache des akt. Providers (oder Teile davon)
      /// </summary>
      /// <param name="reload">Reload der Karte auslösen</param>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      /// <param name="clearpartial">nur der Cache für die akt. Anzeige wird gelöscht</param>
      public void M_Refresh(bool reload, bool clearmemcache, bool clearcache, bool clearpartial) {
         if (clearpartial) {
            clearmemcache = true;
            reload = true;
         }

         if (clearmemcache)
            M_ClearMemoryCache(); // Die Tiles in diesem Cache haben KEINE DbId!

         if (clearcache) {
            if (g_Manager.PrimaryCache != null) {
               g_Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, M_Provider.DbId);
            }
            if (g_Manager.SecondaryCache != null) {
               g_Manager.SecondaryCache.DeleteOlderThan(DateTime.Now, M_Provider.DbId);
            }
         } else {
            if (clearpartial) {  // den Cache für den akt. angezeigten Bereich löschen
               FilecacheManager.ClearCache(g_core.Zoom, 
                                           g_core.GetTilePosXYDrawingList(), 
                                           M_Provider.DbId);
            }
         }

         if (reload)
            g_core.ReloadMap();

         M_Refresh();
      }

      /// <summary>
      /// Anzahl der Tiles die noch in der Warteschlange stehen
      /// </summary>
      /// <returns></returns>
      public int M_WaitingTiles() => g_core.WaitingTiles();

      /// <summary>
      /// Warteschlange der Tiles wird geleert
      /// </summary>
      public void M_ClearWaitingTaskList() => g_core.ClearWaitingTaskList();

      /// <summary>
      /// es wird versucht, die Tile-Erzeugung abzubrechen (kann nur für "lokale" Provider fkt.)
      /// <para>(d.h., die überschriebene Methode <see cref="GMapProvider.GetTileImage(GPoint, int)"/> wird nach Möglichkeit abgebrochen)</para>
      /// </summary>
      public void M_CancelTileBuilds() {

         DateTime now = DateTime.Now;
         foreach (var def in M_ProviderDefinitions)
            if (def.Provider is IHasJobManager)
               ((IHasJobManager)def.Provider).CancelAllJobs(now);
      }

      #region Zoom and Location

      class PosZoom {
         public double Zoom;
         public double Lon;
         public double Lat;

         public PosZoom(double zoom, double lon, double lat) {
            Zoom = zoom;
            Lon = lon;
            Lat = lat;
         }

         public override string ToString() => "Zoom=" + Zoom.ToString() + ", Lon=" + Lon + ", Lat=" + Lat;

      }

      class FiFoExt<T> : IDisposable {

         ConcurrentQueue<T> fifo = new ConcurrentQueue<T>();
         SemaphoreSlim semaphore = new SemaphoreSlim(1);

         public int Count => fifo.Count;


         public FiFoExt() { }

         public void Put(T t) {
            // an das Ende anfügen
            semaphore.Wait();
            fifo.Enqueue(t);
            semaphore.Release();
         }

         public T? Get() {
            T? t = default;
            // das 1. Element holen und entfernen
            semaphore.Wait();
            if (!fifo.TryDequeue(out t))
               t = default;
            semaphore.Release();
            return t;
         }

         public T? GetLast(bool withclear) {
            semaphore.Wait();
            T? t = default;
            T[] all = fifo.ToArray();
            if (all.Length > 0) {
               t = all[all.Length - 1];   // das zuletzt hinzugefügte Element
               fifo.Clear();

               if (all.Length > 1)
                  Debug.WriteLine("FiFoExt: " + (all.Length - 1) + " removed");

               if (!withclear)
                  for (int i = 0; i < all.Length - 1; i++)
                     fifo.Enqueue(all[i]);
            }
            semaphore.Release();
            return t;
         }

         public override string ToString() => Count + " Elements";

         #region Implementierung der IDisposable-Schnittstelle

         /// <summary>
         /// true, wenn schon ein Dispose() erfolgte
         /// </summary>
         bool _isdisposed = false;

         /// <summary>
         /// kann explizit für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
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
         protected void Dispose(bool notfromfinalizer) {
            if (!_isdisposed) {            // bisher noch kein Dispose erfolgt
               if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
                  semaphore.Dispose();
                  fifo.Clear();
               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion
      }

      /// <summary>
      /// Liste der in dieser Reihenfolge gewünschten Positionen/Zooms
      /// </summary>
      FiFoExt<PosZoom> poszoom = new FiFoExt<PosZoom>();

      SemaphoreSlim semaphorePosZoom = new SemaphoreSlim(1);

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      void M_SetLocationAndZoom(double zoom, double centerlon, double centerlat) {
         poszoom.Put(new PosZoom(zoom, centerlon, centerlat));
         try {
            semaphorePosZoom.Wait();
            PosZoom? pz = poszoom.GetLast(true);
            if (pz != null) {    // jetzt tatsächlich eine Position/Zoom-Änderung ausführen
               bool shouldForceUpdateOverlays = false;
               bool sendZoomChanged = false;

               if (M_Position.Lat != centerlat ||
                   M_Position.Lng != centerlon) {
                  g_core.Position = new PointLatLng(centerlat, centerlon);
                  if (g_core.IsStarted)
                     shouldForceUpdateOverlays = true;
               }

               zoom = Math.Min(Math.Max(M_MinZoom, zoom), M_MaxZoom);   // eingrenzen
               if (M_Zoom != zoom) {
                  _zoom = zoom;           // M_Zoom == zoom
                  double remainder = M_Zoom % 1;
                  g_extendedFractionalZoom = M_ScaleMode == ScaleModes.Fractional && remainder != 0 ?
                                                   (float)Math.Pow(2.0, remainder) :  // 1.0 < .. < 2.0
                                                   1;
                  if (g_core.IsStarted && !M_IsDragging) {
                     g_core.Zoom = (int)M_Zoom;                                  // ganzzahliger Anteil
                     OnSizeChanged(EventArgs.Empty);
                     shouldForceUpdateOverlays = true;
                     sendZoomChanged = true;
                  }
               }

               if (shouldForceUpdateOverlays) {
                  forceUpdateOverlays();
               }
               if (sendZoomChanged) {
                  M_FracionalZoomChanged?.Invoke(this, new EventArgs());
               }
            }
         } catch (Exception ex) {
            throw new Exception(ex.Message);
         } finally {
            semaphorePosZoom.Release();
         }

      }

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt)
      /// </summary>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      void M_SetLocation(double centerlon, double centerlat) => M_SetLocationAndZoom(M_Zoom, centerlon, centerlat);

      /// <summary>
      /// setzt den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      void M_SetZoom(double zoom) => M_SetLocationAndZoom(zoom, M_Position.Lng, M_Position.Lat);

      #region asynchron

      /* In einer async-Methode die mit "await Task.Run()" einen Task startet, kann bei schnell aufeinanderfolgenden 
       * Aufrufen NICHT garantiert werden, dass diese Tasks auch in der ursprünglich gewünschten Reihenfolge 
       * abgearbeitet werden.
       * Dadurch kann eine Position/Zoom als letztes eingestellt werden, die gar nicht als letztes angefordert war.
       * 
       * Um das zu vermeiden, werden immer alle noch nicht gestarteten Tasks generell über eine CancellationTokenSource
       * abgebrochen. Bereits gestartete Tasks sind davon NICHT mehr betroffen.
       * 
       * Ein gestarteter Task entfernt trotzdem zunächst "seine" CancellationTokenSource aus der Liste und "disposed" sie.
       * 
       * 
       * In M_SetLocationAndZoomAsync() kann NICHT 
       * 
       */

      /// <summary>
      /// Liste der CancellationTokenSource der akt. gewünschten Tasks 
      /// </summary>
      List<CancellationTokenSource> cancellationTokens = new List<CancellationTokenSource>();

      /// <summary>
      /// zur threadsicheren Verwaltung der <see cref="cancellationTokens"/>
      /// </summary>
      SemaphoreSlim semaphoreCancellationToken = new SemaphoreSlim(1);

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public async Task M_SetLocationAndZoomAsync(double zoom, double centerlon, double centerlat) {
         // alle noch nicht gestarteten Tasks abbrechen
         semaphoreCancellationToken.Wait();
         if (cancellationTokens.Count > 0) {
            //   Debug.WriteLine(">>> Cancel " + cancellationTokens.Count);
            for (int i = 0; i < cancellationTokens.Count; i++)
               cancellationTokens[i].Cancel();
            cancellationTokens.Clear();
         }
         CancellationTokenSource cancelCts = new CancellationTokenSource();
         cancellationTokens.Add(cancelCts);
         semaphoreCancellationToken.Release();

         try {
            await Task.Run(() => {
               // "eigene" CancellationTokenSource aus der Liste entfernen
               semaphoreCancellationToken.Wait();
               cancellationTokens.Remove(cancelCts);
               semaphoreCancellationToken.Release();
               cancelCts.Dispose();

               M_SetLocationAndZoom(zoom, centerlon, centerlat);
            }, cancelCts.Token);
         } catch (TaskCanceledException) { }
      }

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom (linear)
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public async Task M_SetLocationAndZoomLinearAsync(double zoomlinear, double centerlon, double centerlat) =>
         await M_SetLocationAndZoomAsync(Math.Log(zoomlinear, 2) + M_MinZoom,
                                         centerlon,
                                         centerlat);

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt)
      /// </summary>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public async Task M_SetLocationAsync(double centerlon, double centerlat) =>
         await M_SetLocationAndZoomAsync(M_Zoom, centerlon, centerlat);

      /// <summary>
      /// setzt den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      public async Task M_SetZoomAsync(double zoom) =>
         await M_SetLocationAndZoomAsync(zoom, M_Position.Lng, M_Position.Lat);

      /// <summary>
      /// zum Bereich zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      /// <param name="fractionalzoom">wenn false, dann nur Anpassung an ganzzahligen Zoom</param>
      public async Task M_ZoomToRangeAsync(PointD topleft, PointD bottomright, bool fractionalzoom) {
         await setZoomToFitRectAsync(new RectLatLng(topleft.Y,
                                                topleft.X,
                                                Math.Abs(topleft.X - bottomright.X),
                                                Math.Abs(topleft.Y - bottomright.Y))); // Ecke links-oben, Breite, Höhe
         if (fractionalzoom &&
             g_scale4device != 1) {
            Point ptTopLeft = M_LonLat2Client(topleft);
            PointD ptEdgeTopLeft = M_Client2LonLat(0, 0);

            if (ptTopLeft.X < 0 ||
                ptTopLeft.Y < 0) {
               double corrx = (M_CenterLon - topleft.X) / (M_CenterLon - ptEdgeTopLeft.X);
               double corry = (topleft.Y - M_CenterLat) / (ptEdgeTopLeft.Y - M_CenterLat);
               double corr = Math.Max(corrx, corry);                    // linearer Korrekturfaktor
               await M_SetZoomAsync(M_Zoom - Math.Log(corr, 2.0) - .1);
            }
         }
      }

      #endregion

      #endregion

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public async Task M_MoveViewAsync(double dxpercent, double dypercent) {
         if (dxpercent != 0 || dypercent != 0)
            await M_SetLocationAsync(M_Position.Lng + g_ViewArea.WidthLng * dxpercent,
                                     M_Position.Lat + g_ViewArea.HeightLat * dypercent);
      }

      /// <summary>
      /// akt. Ansicht als Bild liefern
      /// </summary>
      /// <returns></returns>
      public
#if GMAP4SKIA
         Bitmap
#else
         Image
#endif
         M_GetViewAsImage(bool withscale = true) {
#if GMAP4SKIA
         Bitmap
#else
         Image
#endif
            img = M_ToImage();
         if (withscale && g_scale != null) {
            Graphics g = Graphics.FromImage(img);
            g_scale.Draw(g);
         }
         return img;
      }

      #region Cache

      /// <summary>
      /// löscht den lokalen und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="provider"></param>
      /// <returns>Anzahl der Tiles</returns>
      public async Task<int> M_ClearCacheAsync(GMapProvider? provider = null) {
         int count = 0;
         await Task.Run(() => {
            if (provider == null) {

               count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, null);           // i.A. lokal (SQLite)
               if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
                  count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, null);

            } else {

               count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, provider.DbId);  // i.A. lokal (SQLite)
               if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
                  count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, GarminProvider.Instance.DbId);

            }
         });

         return count;
      }

      /// <summary>
      /// löscht den lokalen und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="idx">bezieht sich auf die Liste der <see cref="SpecMapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public async Task<int> M_ClearCacheAsync(int idx) =>
         await M_ClearCacheAsync(idx < 0 ? null : M_ProviderDefinitions[idx].Provider);

      /// <summary>
      /// löscht den lokalen Cache (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="id"></param>
      /// <returns>Anzahl der Kartenteile</returns>
      public async Task<int> M_ClearCache4DbIdAsync(int id) {
         int count = 0;
         string error;
         await Task.Run(async () => (count, error) = await FilecacheManager.ClearCache4DbIdAsync(id));
         return count;
      }

      /// <summary>
      /// löscht den lokalen Cache für die ID <paramref name="dbid"/> (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="info"></param>
      /// <returns>Anzahl der gelöschten Dateien; Erfolg der Entfernung der ev. MultiUseProvider-Registrierung (false nur bei Misserfolg); Fehlertext</returns>
      public async Task<(int, bool, string)> ClearFileCache(FilecacheManager.FilecacheInfo.CacheInfo info) => await FilecacheManager.ClearCache(info);

      /// <summary>
      /// löscht den lokalen Cache für die ID <paramref name="dbid"/> (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="info"></param>
      /// <returns>Anzahl der gelöschten Dateien; Erfolg der Entfernung der ev. MultiUseProvider-Registrierung (false nur bei Misserfolg); Fehlertext</returns>
      public async Task<(int, bool, string)> ClearFileCache(IList<FilecacheManager.FilecacheInfo.CacheInfo> info) => await FilecacheManager.ClearCache(info);

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void M_ClearMemoryCache() => GMaps.Instance.MemoryCache.Clear();

      #endregion

      public FilecacheManager.FilecacheInfo? GetFilecacheInfo() =>
         g_Manager.PrimaryCache != null &&
         g_Manager.PrimaryCache is FilePureImageCache ?
                     new FilecacheManager.FilecacheInfo(M_CacheLocation, M_ProviderDefinitions) :
                     null;

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich
      /// </summary>
      /// <param name="minlon"></param>
      /// <param name="maxlon"></param>
      /// <param name="minlat"></param>
      /// <param name="maxlat"></param>
      /// <returns></returns>
      public List<Marker> M_GetPictureMarkersInArea(double minlon, double maxlon, double minlat, double maxlat) {
         List<Marker> markerlst = new List<Marker>();
         foreach (MapMarker marker in g_gpxReadOnlyOverlay.Markers) {
            if (marker is VisualMarker) {
               VisualMarker vm = (VisualMarker)marker;
               if (vm.RealMarker != null) {
                  Marker m = vm.RealMarker;
                  if (m.Markertype == Marker.MarkerType.Foto) {
                     if (minlon <= marker.Position.Lng && marker.Position.Lng <= maxlon &&
                         minlat <= marker.Position.Lat && marker.Position.Lat <= maxlat) {
                        markerlst.Add(m);
                     }
                  }
               }
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
      public List<Marker> M_GetPictureMarkersAround(Point localcenter, int deltax, int deltay) {
         // Distanz um den akt. Punkt (1.5 x Markerbildgröße)
         PointLatLng lefttop = fromLocalToLatLng(localcenter.X - deltax / 2,
                                                             localcenter.Y - deltay / 2);
         PointLatLng rightbottom = fromLocalToLatLng(localcenter.X + deltax / 2,
                                                                 localcenter.Y + deltay / 2);
         return M_GetPictureMarkersInArea(lefttop.Lng, rightbottom.Lng, rightbottom.Lat, lefttop.Lat);
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax">client-x +/- delta</param>
      /// <param name="deltay">client-y +/- delta</param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> M_GetGarminObjectInfos(Point ptclient, int deltax, int deltay) {
         List<GarminImageCreator.SearchObject> info = new List<GarminImageCreator.SearchObject>();
         if (M_Provider is GarminProvider) {
            PointLatLng ptlatlon = fromLocalToLatLng(ptclient.X, ptclient.Y);
            PointLatLng ptdelta = fromLocalToLatLng(ptclient.X + deltax, ptclient.Y + deltay);
            double groundresolution = M_Provider.Projection.GetGroundResolution((int)M_Zoom, ptlatlon.Lat);  // Meter je Pixel

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            info = ((GarminProvider)M_Provider).GetObjectInfo(ptlatlon.Lng,
                                                              ptlatlon.Lat,
                                                              ptdelta.Lng - ptlatlon.Lng,
                                                              ptlatlon.Lat - ptdelta.Lat,
                                                              groundresolution,
                                                              cancellationTokenSource.Token);
         }
         return info;
      }

      /// <summary>
      /// liefert geografische Punkte zu den <paramref name="keywords"/>
      /// <para>Standardmäßig wird für als Geocoding der <see cref="GMapProviders.OpenStreetMap"/> verwendet.
      /// Möglich sind auch <see cref="GMapProviders.GoogleMap"/>, <see cref="GMapProviders.BingMap"/> und <see cref="GMapProviders.YahooMap"/>. 
      /// Dafür sind aber zur Authentifizierung bei bei Google der ApiKey im Provider zu setzen, Microsoft ein ClientKey und 
      /// bei Yahoo eine AppId.</para>
      /// </summary>
      /// <param name="keywords"></param>
      /// <param name="specgp"></param>
      /// <returns></returns>
      public List<PointD> M_GetPositionByKeywords(string keywords, GeocodingProvider? specgp = null) {
         List<PointD> result = new List<PointD>();

         GeocodingProvider? gp = M_Provider as GeocodingProvider;
         if (specgp != null)
            gp = specgp;

         if (gp == null)
            gp = GMapProviders.OpenStreetMap as GeocodingProvider;

         if (gp != null) {
            GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;
            status = gp.GetPoints(keywords.Replace("#", "%23"), out List<PointLatLng> pts);
            if (status == GeoCoderStatusCode.OK)
               foreach (var pt in pts)
                  result.Add(new PointD(pt.Lat, pt.Lng));
         }
         return result;
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax">client-x +/- delta</param>
      /// <param name="deltay">client-y +/- delta</param>
      /// <returns></returns>
      public async Task<List<GarminImageCreator.SearchObject>> M_GetGarminObjectInfosAsync(Point ptclient, int deltax, int deltay) {
         List<GarminImageCreator.SearchObject> info = new List<GarminImageCreator.SearchObject>();
         await Task.Run(() => info = M_GetGarminObjectInfos(ptclient, deltax, deltay));
         return info;
      }

      /// <summary>
      /// liefert geografische Punkte zu den <paramref name="keywords"/>
      /// <para>Standardmäßig wird für als Geocoding der <see cref="GMapProviders.OpenStreetMap"/> verwendet.
      /// Möglich sind auch <see cref="GMapProviders.GoogleMap"/>, <see cref="GMapProviders.BingMap"/> und <see cref="GMapProviders.YahooMap"/>. 
      /// Dafür sind aber zur Authentifizierung bei bei Google der ApiKey im Provider zu setzen, Microsoft ein ClientKey und 
      /// bei Yahoo eine AppId.</para>
      /// </summary>
      /// <param name="keywords"></param>
      /// <param name="specgp"></param>
      /// <returns></returns>
      public async Task<List<PointD>> M_GetPositionByKeywordsAsync(string keywords, GeocodingProvider? specgp = null) {
         List<PointD> result = new List<PointD>();

         await Task.Run(() => {
            GeocodingProvider? gp = M_Provider as GeocodingProvider;
            if (specgp != null)
               gp = specgp;

            if (gp == null)
               gp = GMapProviders.OpenStreetMap as GeocodingProvider;

            if (gp != null) {
               GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;
               status = gp.GetPoints(keywords.Replace("#", "%23"), out List<PointLatLng> pts);
               if (status == GeoCoderStatusCode.OK)
                  foreach (var pt in pts)
                     result.Add(new PointD(pt.Lat, pt.Lng));
            }
         });
         return result;
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
      public void M_ShowTrack(Track? track, bool on, Track? posttrack) {
         if (track != null) {
            if (on) {

               if (track.VisualTrack == null)
                  track.UpdateVisualTrack();
               if (track.VisualTrack != null)
                  mapShowVisualTrack(track.VisualTrack,
                                     true,
                                     track.IsEditable ? TrackLayer.Editable : TrackLayer.Readonly,
                                     posttrack?.VisualTrack);

            } else {

               if (track.IsVisible && track.VisualTrack != null)
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
      public void M_ShowTrack(IList<Track> tracks, bool on) {
         for (int i = tracks.Count - 1; i >= 0; i--)
            M_ShowTrack(tracks[i],
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
      void mapShowVisualTrack(VisualTrack vt, bool on, TrackLayer layer, VisualTrack? postvt = null) {
         if (vt != null &&
             vt.Points.Count > 0) {
            if (on) {

               MapOverlay ov = g_gpxReadOnlyOverlay;
               switch (layer) {
                  case TrackLayer.Editable:
                     ov = g_gpxOverlay;
                     break;

                  case TrackLayer.SelectedParts:
                     ov = g_gpxSelectedPartsOverlay;
                     break;
               }

               //Debug.WriteLine("### ON: " + vt.ToString() + " " + (vt.Overlay != null ? vt.Overlay.Id : "null"));
               vt.IsVisible = true;
               if (!ov.Tracks.Contains(vt)) {
                  int idx = postvt != null ? ov.Tracks.IndexOf(postvt) : -1;
                  if (0 <= idx && idx < ov.Tracks.Count)
                     ov.Tracks.Insert(idx, vt);
                  else
                     ov.Tracks.Add(vt);
               }

            } else {

               //Debug.WriteLine("### OFF " + vt.ToString() + " " + (vt.Overlay != null ? vt.Overlay.Id : "null"));
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
      Dictionary<Track, List<VisualTrack>> g_selectedPartsOfTracks = new Dictionary<Track, List<VisualTrack>>();


      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="idxlst"></param>
      public void M_ShowSelectedParts(Track mastertrack, IList<int>? idxlst) {
         List<List<Gpx.GpxTrackPoint>>? parts = null;
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
                  if (mastertrack.GpxSegment != null)
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
      void mapShowSelectedParts(Track mastertrack, List<List<Gpx.GpxTrackPoint>>? ptlst) {
         if (mastertrack == null) { // alle VisualTrack entfernen

            foreach (var track in g_selectedPartsOfTracks.Keys)
               mapHideSelectedPartsOfTrack(track);
            g_selectedPartsOfTracks.Clear();

         } else {

            mapHideSelectedPartsOfTrack(mastertrack); // alle VisualTrack dieses Tracks entfernen

            if (ptlst != null) {
               if (!g_selectedPartsOfTracks.TryGetValue(mastertrack, out List<VisualTrack>? pseudotracklist)) {
                  pseudotracklist = new List<VisualTrack>();
                  g_selectedPartsOfTracks.Add(mastertrack, pseudotracklist);
               }

               for (int part = 0; part < ptlst.Count; part++) {
                  VisualTrack pseudotrack = new VisualTrack(new Track(ptlst[part], string.Empty), string.Empty, VisualTrack.VisualStyle.SelectedPart);
                  pseudotracklist.Add(pseudotrack);
                  mapShowVisualTrack(pseudotrack,
                                     true,
                                     TrackLayer.SelectedParts);
               }
            } else
               g_selectedPartsOfTracks.Remove(mastertrack);

         }
      }

      /// <summary>
      /// entfernt alle <see cref="VisualTrack"/> für die Selektion dieses Tracks
      /// </summary>
      /// <param name="track"></param>
      void mapHideSelectedPartsOfTrack(Track track) {
         if (g_selectedPartsOfTracks.TryGetValue(track, out List<VisualTrack>? pseudotracklist)) {
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
      public List<Track> M_GetVisibleTracks(bool onlyeditable) {
         List<Track> lst = new List<Track>();
         if (!onlyeditable)
            foreach (var item in g_gpxReadOnlyOverlay.Tracks) {
               if (item is VisualTrack &&
                   ((VisualTrack)item).RealTrack != null)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                  lst.Add(((VisualTrack)item).RealTrack);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
            }
         foreach (var item in g_gpxOverlay.Tracks) {
            if (item is VisualTrack &&
                ((VisualTrack)item).RealTrack != null)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
               lst.Add(((VisualTrack)item).RealTrack);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Track"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Track"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool M_ChangeEditableTrackDrawOrder(IList<Track> trackorder) {
         bool changed = false;

         List<Track> visibletracks = M_GetVisibleTracks(true);
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
            g_gpxOverlay.Tracks.Clear();
            foreach (Track t in neworder) {
               g_gpxOverlay.Tracks.Add(t.VisualTrack);
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
      public void M_ShowMarker(Marker marker, bool on, Marker? postmarker = null) {
         if (marker != null) {
            if (on) {

               if (!marker.IsVisible)
                  marker.IsVisible = true;
               M_ShowVisualMarker(marker.VisualMarker,
                                       true,
                                       marker.IsEditable,
                                       postmarker != null ?
                                              postmarker.VisualMarker :
                                              null);

            } else {

               if (marker.IsVisible)
                  M_ShowVisualMarker(marker.VisualMarker,
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
      public void M_ShowMarker(IList<Marker> markers, bool on) {
         for (int i = 0; i < markers.Count; i++) {
            M_ShowMarker(markers[i],
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
      public void M_ShowVisualMarker(VisualMarker? vm, bool on, bool toplayer, VisualMarker? postvm = null) {
         if (vm != null)
            if (on) {

               MapOverlay ov = toplayer ?
                                       g_gpxOverlay :
                                       g_gpxReadOnlyOverlay;
               vm.IsVisible = true;
               if (!ov.Markers.Contains(vm))
                  ov.Markers.Insert(ov.Markers.IndexOf(postvm), vm);

               if (!ov.Markers.Contains(vm)) {
                  int idx = postvm != null ? ov.Markers.IndexOf(postvm) : -1;
                  if (0 <= idx && idx < ov.Markers.Count)
                     ov.Markers.Insert(idx, vm);
                  else
                     ov.Markers.Add(vm);
               }

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
      public List<Marker> M_GetVisibleMarkers(bool onlyeditable) {
         List<Marker> lst = new List<Marker>();
         if (!onlyeditable)
            foreach (var item in g_gpxReadOnlyOverlay.Markers) {
               if (item is VisualMarker) {
                  VisualMarker vm = (VisualMarker)item;
                  if (vm.RealMarker != null)
                     lst.Add(vm.RealMarker);
               }
            }
         foreach (var item in g_gpxOverlay.Markers) {
            if (item is VisualMarker) {
               VisualMarker vm = (VisualMarker)item;
               if (vm.RealMarker != null)
                  lst.Add(vm.RealMarker);
            }
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Marker"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Marker"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool M_ChangeEditableMarkerDrawOrder(IList<Marker> markerorder) {
         bool changed = false;

         List<Marker> visiblemarkers = M_GetVisibleMarkers(true);
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
            g_gpxOverlay.Markers.Clear();
            foreach (Marker m in neworder) {
               g_gpxOverlay.Markers.Add(m.VisualMarker);
            }
         }

         return changed;
      }

      #endregion

      #region Internal-Funktionen

#if !DESIGN
      /// <summary>
      ///     enque built-in thread safe invalidation (only for internal use)
      /// </summary>
      internal void M_CoreInvalidate() => g_core.InitiateRefresh();
#endif

      /// <summary>
      /// updates markers local position in client coordinates (only for internal use)
      /// </summary>
      /// <param name="marker"></param>
      internal void M_UpdateMarkerLocalPosition(MapMarker marker) {
         // Ist der Punkt innerhalb der Client-Grenzen bzgl. der Map_Position und der Client-Größe?
         // (wenn z.B. Map_Position gerade gesetzt wurde, aber der Client noch nicht angepasst wurde)
         double halfdeltalat = (g_clientLeftTop.Lat - g_clientRightBottom.Lat) / 2;
         double halfdeltalon = (g_clientRightBottom.Lng - g_clientLeftTop.Lng) / 2;
         if (M_Position.Lat - halfdeltalat <= marker.Position.Lat && marker.Position.Lat <= M_Position.Lat + halfdeltalat &&
             M_Position.Lng - halfdeltalon <= marker.Position.Lng && marker.Position.Lng <= M_Position.Lng + halfdeltalon) {
            fromLatLngToLocal(marker.Position, out int xclient, out int yclient);
            marker.SetActiveClientPosition(xclient + marker.LocalOffset.X,
                                           yclient + marker.LocalOffset.Y);
         } else {
            marker.SetActiveClientPosition(int.MinValue, int.MinValue);
         }
      }

      internal void M_UpdateTrackLocalPosition(MapTrack track) {
         if (track.LocalPolyline == null ||
             track.Points.Count != track.LocalPolyline.Length)       // ev. Array der Cientpunkte erzeugen/aktualisieren
            track.LocalPolyline = new Point[track.Points.Count];

         // Ist das Bounding innerhalb der Client-Grenzen bzgl. der Map_Position und der Client-Größe?
         // (wenn z.B. Map_Position gerade gesetzt wurde, aber der Client noch nicht angepasst wurde)
         double halfdeltalat = (g_clientLeftTop.Lat - g_clientRightBottom.Lat) / 2;
         double halfdeltalon = (g_clientRightBottom.Lng - g_clientLeftTop.Lng) / 2;
         if (track.BoundingsIntersects(M_Position.Lat - halfdeltalat,
                                       M_Position.Lat + halfdeltalat,
                                       M_Position.Lng - halfdeltalon,
                                       M_Position.Lng + halfdeltalon)) {
            for (int i = 0; i < track.Points.Count; i++) {
               fromLatLngToLocal(track.Points[i], out int xclient, out int yclient);
               track.LocalPolyline[i].X = xclient;
               track.LocalPolyline[i].Y = yclient;
            }
            track.UpdateVisualParts(true);
         } else
            track.UpdateVisualParts(false);
      }

      /// <summary>
      ///     updates polygons local position (only for internal use)
      /// </summary>
      /// <param name="polygon"></param>
      internal void M_UpdatePolygonLocalPosition(MapPolygon polygon) {
         polygon.LocalPoints.Clear();
         for (int i = 0; i < polygon.Points.Count; i++) {
            GPoint pclient = fromLatLngToLocal(polygon.Points[i]);
            polygon.LocalPoints.Add(pclient);
         }
         polygon.UpdateGraphicsPath();
      }

      #endregion

      #endregion

      #region Funktionen für Win Reaktionen auf Events des Controls; für Skia direkt aufgerufen

      /// <summary>
      /// hier werden einige <see cref="SpecialMapCtrl"/>-Eigenschaften direkt gesetzt und einige <see cref="SpecialMapCtrl"/>-Events "angezapft"
      /// </summary>
      /// <param name="e"></param>
      protected
#if !GMAP4SKIA
      override async void OnLoad(EventArgs e) {
         base.OnLoad(e);
#else
      async Task OnLoad(EventArgs e) {
#endif
         try {
#if GMAP4SKIA
            // Skia-Event auf OnSizeChanged umlenken
            SizeChanged += (object? sender, EventArgs ea) => {
               OnSizeChanged(ea);
            };

            // Skia-Event auf OnPaint umlenken
            PaintSurface += (object? sender, SKPaintSurfaceEventArgs ea) => {
               Graphics g = new Graphics(ea.Surface.Canvas);
               OnPaint(new PaintEventArgs(g, new Rectangle(0, 0, ea.Info.Width, ea.Info.Height)));
               g.Dispose();
            };
#endif
            if (!g_IsDesignerHosted) {
               if (_lazyEvents) {
                  _lazyEvents = false;

                  if (_lazySetZoomToFitRect.HasValue) {
                     await setZoomToFitRectAsync(_lazySetZoomToFitRect.Value);
                     _lazySetZoomToFitRect = null;
                  }
               }

               //g_core.MapOpen().ProgressChanged += (s, e) => controlInvalidate();
               g_core.MapOpenExt(controlRefresh);
               forceUpdateOverlays();
            }
         } catch (Exception ex) {
            if (M_InnerExceptionThrown != null)
               M_InnerExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(new Exception(nameof(OnLoad), ex)));
            else
               throw;
         }

         // Original-Events auf evGMapControl_On-Funktionen im anderen Teil umleiten

         MouseDown += (s, ea) => M_SetAreaSelectionStartPoint(ea); // nur für Bereichsselektion nötig
         MouseUp += (s, ea) => M_SetAreaSelectionEndPoint(ea);     // "
         MouseMove += (s, ea) => M_DoMouseMove(ea);
         MouseLeave += (s, ea) => M_Mouse?.Invoke(
                                       this,
                                       new MapMouseEventArgs(MapMouseEventArgs.EventType.Leave, MouseButtons.None, 0, 0, 0, 0, 0, 0));
         MouseClick += (s, ea) => M_DoMouseClick(ea);

         M_NonFracionalZoomChanged += (s, ea) => M_ZoomChanged?.Invoke(this, EventArgs.Empty);
         M_FracionalZoomChanged += (s, ea) => M_ZoomChanged?.Invoke(this, EventArgs.Empty);
         M_MarkerEnter += (s, ea) => {
            if (ea.Marker != null &&
                ea.Marker is VisualMarker)
               M_Marker?.Invoke(this,
                                          new MarkerEventArgs(((VisualMarker)ea.Marker).RealMarker,
                                                              MapMouseEventArgs.EventType.Enter));
         };
         M_MarkerLeave += (s, ea) => {
            if (ea.Marker != null &&
                ea.Marker is VisualMarker)
               M_Marker?.Invoke(this,
                                          new MarkerEventArgs(((VisualMarker)ea.Marker).RealMarker,
                                                              MapMouseEventArgs.EventType.Leave));
         };
         M_MarkerClick += (s, ea) => {
            if (ea.Mea != null &&
                (g_ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
                 g_ptUsedLastClick != ea.Mea.Location) &&
                ea.Marker is VisualMarker) {
               client2LonLat(ea.Mea.X, ea.Mea.Y, out double lon, out double lat);
               MarkerEventArgs me = new(((VisualMarker)ea.Marker).RealMarker,
                                                        MapMouseEventArgs.EventType.Click,
                                                        ea.Mea.Button,
                                                        ea.Mea.X,
                                                        ea.Mea.Y,
                                                        lon,
                                                        lat);
               M_Marker?.Invoke(this, me);
               if (me.IsHandled)
                  g_ptUsedLastClick = ea.Mea.Location;
            }
         };

         M_TrackEnter += (s, ea) => {
            if (ea.Track is VisualTrack) {
               Track? track = ((VisualTrack)ea.Track).RealTrack;
               if (track != null)
                  M_Track?.Invoke(this,
                                            new TrackEventArgs(track, MapMouseEventArgs.EventType.Enter));
            }
         };
         M_TrackLeave += (s, ea) => {
            if (ea.Track is VisualTrack) {
               Track? track = ((VisualTrack)ea.Track).RealTrack;
               if (track != null)
                  M_Track?.Invoke(this,
                                            new TrackEventArgs(track, MapMouseEventArgs.EventType.Leave));
            }
         };
         M_TrackClick += (s, ea) => {
            if (ea.Mea != null &&
                (g_ptUsedLastClick.X == int.MinValue || // mit Sicherheit ein "neuer" Klick
                 g_ptUsedLastClick != ea.Mea.Location) &&
                ea.Track is VisualTrack) {
               Track? track = ((VisualTrack)ea.Track).RealTrack;
               if (track != null) {
                  client2LonLat(ea.Mea.X, ea.Mea.Y, out double lon, out double lat);
                  TrackEventArgs te = new TrackEventArgs(track,
                                                         MapMouseEventArgs.EventType.Click,
                                                         ea.Mea.Button,
                                                         ea.Mea.X,
                                                         ea.Mea.Y,
                                                         lon,
                                                         lat);
                  M_Track?.Invoke(this, te);
                  if (te.IsHandled)
                     g_ptUsedLastClick = ea.Mea.Location;
               }
            }
         };

#if !GMAP4SKIA
         //MapBearing = 0F;
         M_MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
#endif

         M_CanDragMap = true;
         M_LevelsKeepInMemory = 5;
         M_MarkersEnabled = true;
         M_PolygonsEnabled = true;
         M_RetryLoadTile = 0;
         M_FillEmptyTiles = false;      // keinen niedrigeren Zoom verwenden (notwendig für korrekten Abbruch bei Garmin-Provider)
         M_TracksEnabled = true;
         M_ScaleMode = ScaleModes.Fractional;
         M_SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);

         M_DeviceZoom = 1;

#if !NET5_0_OR_GREATER
         //  vor .NET 4.5
         ServicePointManager.Expect100Continue = true;
         ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
         //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //SecurityProtocolType.SystemDefault;
#endif

         M_Provider = EmptyProvider.Instance;   // gMapProviders[startprovideridx];
         M_EmptyTileText = "no data";                                    // Hinweistext für "Tile ohne Daten"

         M_MinZoom = 0;
         M_MaxZoom = 24;
         M_SetZoom(20);

#if DEBUG
         M_ShowTileGridLines = true;
#else
         M_ShowTileGridLines = false;
#endif
         M_EmptyMapBackgroundColor = Color.LightGray;      // Tile (noch) ohne Daten
         M_EmptyTileText = "keine Daten";             // Hinweistext für "Tile ohne Daten"
         M_EmptyTileColor = Color.DarkGray;           // Tile (endgültig) ohne Daten

         g_Overlays.Add(g_gpxReadOnlyOverlay);
         g_gpxReadOnlyOverlay.IsVisibile = true;

         g_Overlays.Add(g_gpxOverlay);
         g_gpxOverlay.IsVisibile = true;

         g_Overlays.Add(g_gpxSelectedPartsOverlay);
         g_gpxSelectedPartsOverlay.IsVisibile = true;

         g_scale = new Scale4Map(this) {
            Kind = M_ScaleKind,
            Alpha = M_ScaleAlpha,
         };
      }

      void onMouseDown(MouseEventArgs e) {
         if (!M_IsMouseOverMarker) {
            if (e.Button == M_DragButton && M_CanDragMap) {
               g_core.MouseDown = new GPoint(clientx2core(e.X), clienty2core(e.Y));
               M_CoreInvalidate();
            } else if (!g_IsSelected) {
               g_IsSelected = true;
               setSelectedArea(RectLatLng.Empty);
               g_selectionEnd = PointLatLng.Empty;
               g_selectionStart = fromLocalToLatLng(e.X, e.Y);
            }
         }
      }

      async Task onMouseUp(MouseEventArgs e) {
         g_IsSelected = false;

         if (g_core.IsDragging) {
            if (M_IsDragging) {
               M_IsDragging = false;
               setLastCursor();
            }

            g_core.EndDrag();

            if (g_boundsOfMap.HasValue &&
                !g_boundsOfMap.Value.Contains(M_Position) &&
                g_core.LastLocationInBounds.HasValue)
               await M_SetLocationAsync(g_core.LastLocationInBounds.Value.Lng,
                                        g_core.LastLocationInBounds.Value.Lat);
         } else {
            if (e.Button == M_DragButton)
               g_core.MouseDown = GPoint.Empty;

            if (!g_selectionEnd.IsEmpty &&
                !g_selectionStart.IsEmpty) {
               bool zoomtofit = false;

               if (!g_selectionData.selectedArea.IsEmpty && ModifierKeys == Keys.Shift)
                  zoomtofit = await setZoomToFitRectAsync(g_selectionData.selectedArea);

               M_SelectionChange?.Invoke(this, new SelectionChangeEventArgs(g_selectionData.selectedArea, zoomtofit));
            } else {
               M_CoreInvalidate();
            }
         }
      }

      /// <summary>
      /// liefert die Objektlisten an der Position und löst für die betroffenen Objekte die Click-Events aus
      /// <para><see cref="MapMarker"/>: liefert einen oder alle bei denen (xclient, yclient) im <see cref="MapMarker.ActiveClientArea"/> liegt</para>
      /// <para><see cref="MapTrack"/>: liefert den 1. oder alle, die den Kreis mit dem Radius <see cref="SpecMapClickTolerance4Tracks"/> um (xclient, yclient) berühren</para>
      /// </summary>
      /// <param name="e"></param>
      /// <param name="doubleclick"></param>
      /// <param name="all"></param>
      /// <param name="markers"></param>
      /// <param name="tracks"></param>
      /// <param name="polygons"></param>
      /// <returns></returns>
      PointLatLng onMouseClick(MouseEventArgs eclient,
                               bool doubleclick,
                               bool all,
                               out List<MapMarker> markers,
                               out List<MapTrack> tracks,
                               out List<MapPolygon> polygons) {
         PointLatLng point = PointLatLng.Empty;
         markers = new List<MapMarker>();
         tracks = new List<MapTrack>();
         polygons = new List<MapPolygon>();

         if (!g_core.IsDragging) {
            bool overlayObject = false;

            for (int i = g_Overlays.Count - 1; i >= 0; i--) {
               MapOverlay ov = g_Overlays[i];
               if (ov != null &&
                   ov.IsVisibile &&
                   ov.IsHitTestVisible) {

                  List<MapMarker> markers4o = new List<MapMarker>(getMarkers4Point(ov, eclient.X, eclient.Y, all));
                  List<MapTrack> tracks4o = new List<MapTrack>(getTracks4Point(ov, eclient.X, eclient.Y, all, M_ClickTolerance4Tracks));
                  List<MapPolygon> polygons4o = new List<MapPolygon>(getPolygons4Point(ov, fromLocalToLatLng(eclient.X, eclient.Y), all));

                  markers.AddRange(markers4o);
                  tracks.AddRange(tracks4o);
                  polygons.AddRange(polygons4o);

                  foreach (var m in markers4o) {
                     if (doubleclick)
                        M_MarkerDoubleClick?.Invoke(this, new MapMarkerEventArgs(m, eclient));
                     else
                        M_MarkerClick?.Invoke(this, new MapMarkerEventArgs(m, eclient));
                  }

                  foreach (var t in tracks4o) {
                     if (doubleclick)
                        M_TrackDoubleClick?.Invoke(this, new MapTrackEventArgs(t, eclient));
                     else
                        M_TrackClick?.Invoke(this, new MapTrackEventArgs(t, eclient));
                  }

                  foreach (var p in polygons4o) {
                     if (doubleclick)
                        M_PolygonDoubleClick?.Invoke(p, new MapPolygonEventArgs(p, eclient));
                     else
                        M_PolygonClick?.Invoke(p, new MapPolygonEventArgs(p, eclient));
                  }

               }
               overlayObject = markers.Count > 0 || tracks.Count > 0 || polygons.Count > 0;
               if (overlayObject && !all)
                  break;
            }

            if (!overlayObject && g_core.MouseDown != GPoint.Empty)
               point = fromLocalToLatLng(eclient.X, eclient.Y);
         }

         return point;
      }

      void onMouseMove(MouseEventArgs e) {
         if (!g_core.IsDragging &&             // noch nicht gestartet ...
             !g_core.MouseDown.IsEmpty) {      // ... und Startpunkt bei MouseDown registriert ...
            if (Math.Abs(e.X - g_core.MouseDown.X) * 2 >= g_DragSize.Width ||  // ... und Mindestweite der Bewegung vorhanden
                Math.Abs(e.Y - g_core.MouseDown.Y) * 2 >= g_DragSize.Height) {

               g_core.BeginDrag(g_core.MouseDown);  // Dragging mit diesen Clientkoordinaten starten
            }
         }

         if (g_core.IsDragging) {
            if (!M_IsDragging) {
               M_IsDragging = true;
               setCursor4Drag();
            }

            g_clientLeftTop = fromLocalToLatLng(0, 0);
            g_clientRightBottom = fromLocalToLatLng(Width, Height);

            if (g_boundsOfMap.HasValue && !g_boundsOfMap.Value.Contains(M_Position)) {
               // ...
            } else {
               g_core.Drag(new GPoint(clientx2core(e.X), clienty2core(e.Y)));
               forceUpdateOverlays();
               controlInvalidate();
            }
         } else {
            if (g_IsSelected &&
                !g_selectionStart.IsEmpty &&
                (ModifierKeys == Keys.Alt || ModifierKeys == Keys.Shift || g_DisableAltForSelection)) {
               g_selectionEnd = fromLocalToLatLng(e.X, e.Y);
               var p1 = g_selectionStart;
               var p2 = g_selectionEnd;

               double x1 = Math.Min(p1.Lng, p2.Lng);
               double y1 = Math.Max(p1.Lat, p2.Lat);
               double x2 = Math.Max(p1.Lng, p2.Lng);
               double y2 = Math.Min(p1.Lat, p2.Lat);

               setSelectedArea(new RectLatLng(y1, x1, x2 - x1, y1 - y2));
            } else if (g_core.MouseDown.IsEmpty) {
               for (int i = g_Overlays.Count - 1; i >= 0; i--) {
                  MapOverlay ov = g_Overlays[i];
                  if (ov != null &&
                      ov.IsVisibile &&
                      ov.IsHitTestVisible) {

                     List<MapMarker> markers = getMarkers4Point(ov, e.X, e.Y, true);
                     List<MapTrack> tracks = getTracks4Point(ov, e.X, e.Y, true, M_ClickTolerance4Tracks);
                     List<MapPolygon> polygons = getPolygons4Point(ov, fromLocalToLatLng(e.X, e.Y), true);

                     foreach (var m in ov.Markers) {
                        if (m.IsVisible &&
                            m.IsHitTestVisible) {
                           if (markers.Contains(m)) {
                              if (!m.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 m.IsMouseOver = true;
                                 M_IsMouseOverMarker = true;
                                 M_MarkerEnter?.Invoke(this, new MapMarkerEventArgs(m, null));
                                 M_CoreInvalidate();
                              }
                           } else if (m.IsMouseOver) {
                              m.IsMouseOver = false;
                              M_IsMouseOverMarker = false;
                              M_RestoreCursorOnLeave();
                              M_MarkerLeave?.Invoke(this, new MapMarkerEventArgs(m, null));
                              M_CoreInvalidate();
                           }
                        }
                     }

                     foreach (var t in ov.Tracks) {
                        if (t.IsVisible &&
                            t.IsHitTestVisible) {
                           if (tracks.Contains(t)) {
                              if (!t.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 t.IsMouseOver = true;
                                 M_IsMouseOverTrack = true;
                                 M_TrackEnter?.Invoke(this, new MapTrackEventArgs(t, null));
                                 M_CoreInvalidate();
                              }
                           } else if (t.IsMouseOver) {
                              t.IsMouseOver = false;
                              M_IsMouseOverTrack = false;
                              M_RestoreCursorOnLeave();
                              M_TrackLeave?.Invoke(this, new MapTrackEventArgs(t, null));
                              M_CoreInvalidate();
                           }
                        }
                     }

                     foreach (var p in ov.Polygons) {
                        if (p.IsVisible &&
                            p.IsHitTestVisible) {
                           if (polygons.Contains(p)) {
                              if (!p.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 p.IsMouseOver = true;
                                 M_IsMouseOverPolygon = true;
                                 M_PolygonEnter?.Invoke(this, new MapPolygonEventArgs(p, null));
                                 M_CoreInvalidate();
                              }
                           } else if (p.IsMouseOver) {
                              p.IsMouseOver = false;
                              M_IsMouseOverPolygon = false;
                              M_RestoreCursorOnLeave();
                              M_MapPolygonLeave?.Invoke(this, new MapPolygonEventArgs(p, null));
                              M_CoreInvalidate();
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      void onSizeChanged(EventArgs e) {
         if (Width == 0 || Height == 0)
            return;

         if (!g_IsDesignerHosted) {
            g_core.MapSizeChanged((int)Math.Round(Width / M_DeviceZoom),
                                  (int)Math.Round(Height / M_DeviceZoom));
            g_clientLeftTop = fromLocalToLatLng(0, 0);
            g_clientRightBottom = fromLocalToLatLng(Width, Height);
            if (Visible &&
                IsHandleCreated &&
                g_core.IsStarted) {
               forceUpdateOverlays();
            }
         }
      }

      /*
       * Es ist gesichert, dass OnPaint() NICHT parallel arbeiten muss.
       * Außerdem läuft drawGraphics() garantiert im MainThread.
       * Wahrscheinlich ist das aber alles unnötig.
      */

      SemaphoreSlim semaphoreOnPaint = new SemaphoreSlim(1);
      long drawCounterOnPaint = 0;

#if !GMAP4SKIA
      protected override
#else
      protected virtual
#endif
      void OnPaint(PaintEventArgs e) {
         if (Interlocked.Read(ref drawCounterOnPaint) > 1) {   // 1: min. 1 ist noch "in Arbeit" und 1 wartet
            Debug.WriteLine(">>> drawGraphics() unnötig");
            return;
         }
         Interlocked.Increment(ref drawCounterOnPaint);

         try {
            semaphoreOnPaint.Wait();

            if (g_GraphicsBackBuffer != null) {
               RunInMainThread(() => drawGraphics(g_GraphicsBackBuffer));
               if (_backBuffer != null)
                  e.Graphics.DrawImage(_backBuffer, 0, 0);
            } else
               RunInMainThread(() => drawGraphics(e.Graphics));

         } catch (Exception ex) {
            if (M_InnerExceptionThrown != null)
               M_InnerExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(new Exception(nameof(OnPaint), ex), false));
            else
               throw;
         } finally {
#if !GMAP4SKIA
            base.OnPaint(e);
#else
            Paint?.Invoke(this, e);
#endif
            semaphoreOnPaint.Release();
         }

         Interlocked.Decrement(ref drawCounterOnPaint);
      }

      #endregion

      #region (privat) drawing the map (from OnPaint())

      /// <summary>
      /// liefert die Matrix für die Koordinatentransformation von core- in client-Koordinaten
      /// </summary>
      /// <param name="invert">wenn true, dann für Umwandlung von client- in core-Koordinaten</param>
      /// <param name="withrenderoffset">wenn true, wird der <see cref="PublicCore.RenderOffset"/> einbezogen 
      /// (i.A. nur in <see cref="drawGraphics(Graphics)"/> nötig)</param>
      /// <returns></returns>
      Matrix getTransformationMatrix(bool invert,
                                     bool withrenderoffset,
                                     double deviceZoom,
                                     double extendedFractionalZoom,
                                     GPoint renderOffset) {
         Matrix m = new Matrix();
         if (withrenderoffset)
            m.Translate(renderOffset.X,
                        renderOffset.Y,
                        MatrixOrder.Append);
         m.Scale((float)extendedFractionalZoom,
                 (float)extendedFractionalZoom,
                 MatrixOrder.Append);
         m.Scale((float)deviceZoom,
                 (float)deviceZoom,
                 MatrixOrder.Append);
         m.Translate(-(int)(Width * (extendedFractionalZoom - 1) / 2),
                     -(int)(Height * (extendedFractionalZoom - 1) / 2),
                     MatrixOrder.Append);
         if (invert)
            m.Invert();
         return m;
      }

      /// <summary>
      /// draw the map and the overlays to graphics (call only in MainThread!)
      /// </summary>
      /// <param name="g"></param>
      void drawGraphics(Graphics g) {
         int corezoom = g_core.Zoom;

         //logtxt.AppendLine(DateTime.Now.ToString("O") + " drawGraphics: corezoom=" + corezoom
         //   + " deviceZoom=" + _deviceZoom
         //   + " extendedFractionalZoom=" + g_extendedFractionalZoom
         //   + " RenderOffset=" + g_core.RenderOffset
         //   + " IsDragging=" + g_core.IsDragging
         //   + " Position=" + g_core.Position
         //   );

         g.Transform = getTransformationMatrix(false, true, _deviceZoom, g_extendedFractionalZoom, g_core.RenderOffset);

         drawMap(g, corezoom);
         g.ResetTransform();

         drawExtended(g, corezoom);

         if (g_ShowCoreData4Test)
            drawCoreData(g);
      }

      void drawMap(Graphics g, int corezoom) {
         g.Clear(M_EmptyMapBackgroundColor);

         if (g_core.UpdatingBounds ||
             M_Provider == EmptyProvider.Instance ||
             M_Provider == null)
            return;

         g.TextRenderingHint = TextRenderingHint.AntiAlias;
         g.SmoothingMode = SmoothingMode.AntiAlias;
#if !GMAP4SKIA
         g.CompositingQuality = CompositingQuality.HighQuality;
         g.InterpolationMode = InterpolationMode.HighQualityBicubic;
#endif
         try {
            g_core.LockImageStore();

            GPoint[] tilePosXYlist = g_core.GetTilePosXYDrawingList();
            for (int t = 0; t < tilePosXYlist.Length; t++) {
               GPoint tileLocation = g_core.GetTileDestination(t, out GSize tileSize);
               Rectangle rectTile = new Rectangle((int)tileLocation.X,
                                                  (int)tileLocation.Y,
                                                  (int)tileSize.Width,
                                                  (int)tileSize.Height);
               RectangleF rectTileF = new RectangleF(rectTile.X, rectTile.Y, rectTile.Width, rectTile.Height);

               GPoint tilePosXY = tilePosXYlist[t];
               Tile tile = g_core.GetTile(corezoom, tilePosXY);
               //Debug.WriteLine(">>> " + t + ", " + tile.Pos + ", " + rectTile);

               bool found = false;
               if (tile.NotEmpty) {

                  foreach (MapImage img in tile.Overlays) {               // jedes Image (?) des Tiles
                     if (img != null && img.Img != null) {
                        if (!found)
                           found = true;

#if GMAP4SKIA
                        if (!img.IsParent) {

#if GRAPHICS2
                           g.DrawBitmap(img.Img, rectConvert(rectTile));
#else
                           g.SKCanvas.DrawBitmap(img.Img, rectConvert(rectTile));
#endif

                        } else {
                           getImageSourceArea(img, out int x, out int y, out int width, out int height);
#if GRAPHICS2
                           g.DrawBitmap(img.Img,
                                                 new SKRect(x,
                                                            y,
                                                            width,
                                                            height),
                                                 rectConvert(rectTile));
#else
                           g.SKCanvas.DrawBitmap(img.Img,
                                                 new SKRect(x,
                                                            y,
                                                            width,
                                                            height),
                                                 rectConvert(rectTile));
#endif

                        }
                     }
#else
                        if (!img.IsParent) {
                           g.DrawImage(img.Img, rectTile);
                        } else {
                           /* Das Bild hat zwar die übliche Größe (rectTile), ist aber für einen kleineren Zoom.
                            * Deshalb wird nur ein Teilbereich verwendet und auf die notwendige Größe vergrößert. */
                           getImageSourceArea(img, out int x, out int y, out int width, out int height);
                           g.DrawImage(img.Img,
                                       rectTile,
                                       x,
                                       y,
                                       width,
                                       height,
                                       GraphicsUnit.Pixel,
                                       g_tileFlipXYAttributes);
                        }
                     }
#endif
                  }

               } else if (M_FillEmptyTiles &&
                          M_Provider.Projection is MercatorProjection) {

                  //                  int zoomOffset = 1;
                  //                  var parentTile = Tile.Empty;
                  //                  long ix = 0;

                  //                  // suche ein Tile für geringeren Zoom
                  //                  while (!parentTile.NotEmpty &&
                  //                         zoomOffset < core.Zoom &&
                  //                         zoomOffset <= Map_LevelsKeepInMemory) {
                  //                     ix = (long)Math.Pow(2, zoomOffset);
                  //                     parentTile = core.GetTile(core.Zoom - zoomOffset++,
                  //                                               new GPoint((int)(tilePosXY.X / ix),
                  //                                                          (int)(tilePosXY.Y / ix)));
                  //                  }

                  //                  // wenn gefunden, dann anzeigen
                  //                  if (parentTile.NotEmpty) {
                  //                     long xOff = Math.Abs(tilePosXY.X - parentTile.Pos.X * ix);
                  //                     long yOff = Math.Abs(tilePosXY.Y - parentTile.Pos.Y * ix);
                  //                     foreach (GMapImage img in parentTile.Overlays) {
                  //                        if (img != null &&
                  //                            img.Img != null &&
                  //                            !img.IsParent) {
                  //                           if (!found)
                  //                              found = true;

                  //                           getImageSourceArea(img, (int)ix, (int)xOff, (int)yOff, out int x, out int y, out int width, out int height);


                  //#if GMAP4SKIA
                  //                           g.SKCanvas.DrawBitmap(img.Img,
                  //                                                 new SKRect(x,
                  //                                                            y,
                  //                                                            width,
                  //                                                            height),
                  //                                                 rectConvert(rectTile));
                  //#else
                  //                           g.DrawImage(img.Img,
                  //                                       rectTile,
                  //                                       x,
                  //                                       y,
                  //                                       width,
                  //                                       height,
                  //                                       GraphicsUnit.Pixel,
                  //                                       tileFlipXYAttributes);
                  //#endif
                  //                        }
                  //                     }
                  //                  } else
                  //                     g.FillRectangle(_selectedAreaFillBrush, rectTile);

                  if (_selectedAreaFillBrush != null)
                     g.FillRectangle(_selectedAreaFillBrush, rectTile);


               }

               // add text if tile is missing
               if (!found) {
                  Exception ex = g_core.GetException4FailedLoad(tilePosXY);
                  if (ex != null) {
                     M_InnerExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(new Exception(nameof(drawMap), ex), false));

                     if (_emptyTileBrush != null)
                        g.FillRectangle(_emptyTileBrush, rectTile);

                     g.DrawString("Exception: " + ex.Message,
                                  g_MissingDataFont,
                                  (SolidBrush)Brushes.Red,
                                  new RectangleF(rectTile.X + 11,
                                                 rectTile.Y + 11,
                                                 rectTile.Width - 11,
                                                 rectTile.Height - 11));

                     g.DrawString(M_EmptyTileText,
                                  g_MissingDataFont,
                                  (SolidBrush)Brushes.Blue,
                                  rectTileF,
                                  g_centerFormat);

                     g.DrawRectangle(M_EmptyTileBordersPen, rectTile);
                  }
               }

               if (M_ShowTileGridLines) {
                  g.DrawRectangle(M_EmptyTileBordersPen, rectTile);
                  g.DrawString(tilePosXY.ToString(),
                               g_MissingDataFont,
                               Brushes.Red,
#if !GMAP4SKIA
                               rectTileF,
                               g_centerFormat);
#else
                               new PointF(rectTile.X, rectTile.Y));
#endif
               }
            }
         } catch (Exception ex) {
            M_InnerExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(new Exception(nameof(drawMap), ex), false));
         } finally {
            g_core.ReleaseImageStore();
         }
      }

      /// <summary>
      /// zum Zeichnen der Overlays für Marker, Tracks und Tootips sowie Auswahlrechteck, Copyright und Zusätze
      /// </summary>
      /// <param name="g"></param>
      protected virtual void drawExtended(Graphics g, int corezoom) {
         try {
            g.SmoothingMode = SmoothingMode.HighQuality;
            foreach (var o in g_Overlays)
               if (o.IsVisibile)
                  o.OnRender(g);

            // separate tooltip drawing
            foreach (var o in g_Overlays)
               if (o.IsVisibile)
                  o.OnRenderToolTips(g);

            //g.ResetTransform();  // ?????

            #region Auswahlrechteck

            if (!g_selectionData.selectedArea.IsEmpty) {
               var p1 = fromLatLngToLocal(g_selectionData.selectedArea.LocationTopLeft);
               var p2 = fromLatLngToLocal(g_selectionData.selectedArea.LocationRightBottom);

               long x1 = p1.X;
               long y1 = p1.Y;
               long x2 = p2.X;
               long y2 = p2.Y;

               g.DrawRectangle(M_SelectionPen, x1, y1, x2 - x1, y2 - y1);
               if (_selectedAreaFillBrush != null)
                  g.FillRectangle(_selectedAreaFillBrush, x1, y1, x2 - x1, y2 - y1);
            }

            #endregion

            #region Copyright 

            if (!string.IsNullOrEmpty(g_core.Provider.Copyright)) {
               g.DrawString(g_core.Provider.Copyright,
                            M_CopyrightFont,
                            (SolidBrush)Brushes.Navy,
#if GMAP4SKIA
                            new PointF(15, Height - M_CopyrightFont.GetHeight() - 25));
#else
                            3,
                            Height - M_CopyrightFont.Height - 5);
#endif
            }

            #endregion

            #region Center, Maßstab, Zusatz

            mapDrawCenter(new DrawExtendedEventArgs(g, corezoom, g_extendedFractionalZoom, M_DeviceZoom));

            mapDrawScale(new DrawExtendedEventArgs(g, corezoom, g_extendedFractionalZoom, M_DeviceZoom));

            mapDrawOnTop(new DrawExtendedEventArgs(g, corezoom, g_extendedFractionalZoom, M_DeviceZoom));

            #endregion

         } catch (Exception ex) {
            if (M_InnerExceptionThrown != null)
               M_InnerExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(new Exception(nameof(drawExtended), ex), false));
            else
               throw new Exception(nameof(drawExtended) + ": " + ex.Message);
         }
      }

      protected void mapDrawScale(DrawExtendedEventArgs e) => g_scale?.Draw(e.Graphics, (float)(e.ExtendedZoom * e.DeviceZoom));

      protected void mapDrawCenter(DrawExtendedEventArgs e) {
         if (M_ShowCenter) {
#if GMAP4SKIA
            float ro = Math.Min(Width, Height) / 30;
            float ri = ro / 4;
            float cx = Width / 2;
            float cy = Height / 2;
            float left = cx - ro;
            float right = cx + ro;
            float top = cy - ro;
            float bottom = cy + ro;

            e.Graphics.DrawLine(g_CenterPen,
                                left,
                                cy,
                                cx - ri,
                                cy);
            e.Graphics.DrawLine(g_CenterPen,
                                cx + ri,
                                cy,
                                right,
                                cy);
            e.Graphics.DrawLine(g_CenterPen,
                                cx,
                                top,
                                cx,
                                cy - ri);
            e.Graphics.DrawLine(g_CenterPen,
                                cx,
                                cy + ri,
                                cx,
                                bottom);
            e.Graphics.DrawEllipse(g_CenterPen, left, top, 2 * ro, 2 * ro);
#endif
         }
      }

      protected void mapDrawOnTop(DrawExtendedEventArgs e) => M_DrawOnTop?.Invoke(this, e);

      /// <summary>
      /// print some Core-Data to screen (only for testing)
      /// </summary>
      /// <param name="g"></param>
      void drawCoreData(Graphics g) {
         g.ResetTransform();
         float h = M_EmptyTileFont.GetHeight();
         float y = 0;
         //g.DrawString(string.Format("Core.ViewArea {0}", core.ViewArea), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.Position {0}", core.Position), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.PositionPixel {0}", core.PositionPixel), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.CompensationOffset {0}", core.CompensationOffset), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.CenterTileXYLocation {0}", core.CenterTileXYLocation), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.Width x Core.Height {0}x{1}", core.Width, core.Height), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.Zoom {0}", core.Zoom), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.ScaleX {0}, Core.ScaleY {1}", core.ScaleX, core.ScaleY), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         //y += h;
         //g.DrawString(string.Format("Core.Bearing {0}", core.Bearing), MapEmptyTileFont, Brushes.DarkBlue as SolidBrush, new PointF(0, y));
         y += h;
      }

      #endregion

      #region privat-Funktionen

      /// <summary>
      /// liefert den linearen Zoom zum "normalen" Zoom-Wert
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      double zoomLinear(double zoom) => Math.Pow(2.0, zoom - M_MinZoom);

      /// <summary>
      /// registriert das akt. "Auswahl"-Rechteck
      /// </summary>
      /// <param name="selectedArea"></param>
      void setSelectedArea(RectLatLng selectedArea) {
         g_selectionData.selectedArea = selectedArea;
         if (g_core.IsStarted)
            M_CoreInvalidate();
      }

      RectLatLng? _lazySetZoomToFitRect;
      bool _lazyEvents = true;

      /// <summary>
      /// sets zoom to max to fit rect
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      async Task<bool> setZoomToFitRectAsync(RectLatLng rect) {
         if (_lazyEvents) {
            _lazySetZoomToFitRect = rect;
         } else {
            int maxZoom = g_core.GetMaxZoomToFitRect(rect);
            if (maxZoom > 0) {
               await M_SetLocationAsync(rect.Lng + rect.WidthLng / 2, rect.Lat - rect.HeightLat / 2);
               if (maxZoom > M_MaxZoom)
                  maxZoom = M_MaxZoom;
               if ((int)M_Zoom != maxZoom)
                  await M_SetZoomAsync(maxZoom);
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// ermittelt das jeweils erste Objekt der Objektarten <see cref="MapMarker"/>, <see cref="MapTrack"/> und <see cref="MapPolygon"/>, 
      /// dass an diesem Punkt liegt und löst die zugehörigen Events aus (z.Z. nur für Skia !!!)
      /// <para>Gibt es kein Objekt, wird die geohrafische Position geliefert.</para>
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="doubleclick">Klick oder Doppelklick</param>
      /// <param name="button">Mausbutton</param>
      /// <param name="all">bei true werden alle Objekte ermittelt</param>
      /// <param name="marker">Liste der Marker</param>
      /// <param name="track">Liste der Tracks</param>
      /// <param name="polygon">Liste der Polygone</param>
      /// <returns></returns>
      PointLatLng tapped(int clientx,
                         int clienty,
                         bool doubleclick,
                         MouseButtons button,
                         bool all,
                         out List<MapMarker> marker,
                         out List<MapTrack> track,
                         out List<MapPolygon> polygon) {
#if GMAP4SKIA
         simulateMousePosition(clientx, clienty);
         return doubleclick ?
                     OnMouseDoubleClick(new MouseEventArgs(button, clientx, clienty, 0),
                                        all,
                                        out marker,
                                        out track,
                                        out polygon) :

                     OnMouseClick(new MouseEventArgs(button, clientx, clienty, 0),
                                  all,
                                  out marker,
                                  out track,
                                  out polygon);
#else

         // DUMMY

         marker = new List<MapMarker>();
         track = new List<MapTrack>();
         polygon = new List<MapPolygon>();
         return new PointLatLng(0, 0);
#endif
      }

      /// <summary>
      ///     update objects when map is draged/zoomed
      /// </summary>
      void forceUpdateOverlays() {
         try {
            M_HoldInvalidation = true; // ev. laufende Neuzeichnen abbrechen
            foreach (var o in g_Overlays) {
               if (o.IsVisibile)
                  o.ForceUpdate();     // updates local positions of objects (Vorbereitung für Refresh)
            }
         } finally {
            M_Refresh();               // Refresh auslösen
         }
      }

      /// <summary>
      /// liefert einen oder alle Marker bei denen (xclient, yclient) im <see cref="MapMarker.ActiveClientArea"/> liegt
      /// </summary>
      /// <param name="o"></param>
      /// <param name="xclient"></param>
      /// <param name="yclient"></param>
      /// <param name="all"></param>
      /// <returns></returns>
      List<MapMarker> getMarkers4Point(MapOverlay o, int xclient, int yclient, bool all = true) {
         List<MapMarker> markers = new List<MapMarker>();
         foreach (var m in o.Markers) {
            if (m.IsVisible &&
                m.IsHitTestVisible &&
                m.IsOnClientVisible) {
               if (m.ActiveClientArea.Contains(xclient, yclient)) {
                  markers.Add(m);
                  if (!all)
                     break;
               }
            }
         }
         return markers;
      }

      /// <summary>
      /// liefert den 1. oder alle Tracks, die den Kreis mit dem Radius um (xclient, yclient) berühren
      /// </summary>
      /// <param name="o"></param>
      /// <param name="xclient"></param>
      /// <param name="yclient"></param>
      /// <param name="all"></param>
      /// <param name="radius"></param>
      /// <returns></returns>
      List<MapTrack> getTracks4Point(MapOverlay o, int xclient, int yclient, bool all = true, float radius = 1F) {
         List<MapTrack> tracks = new List<MapTrack>();
         foreach (var t in o.Tracks) {
            if (t.IsVisible &&
                t.IsHitTestVisible) {
               if (t.IsInside(xclient, yclient, radius)) {
                  tracks.Add(t);
                  if (!all)
                     break;
               }
            }
         }
         return tracks;
      }

      List<MapPolygon> getPolygons4Point(MapOverlay o, PointLatLng pt, bool all = true) {
         List<MapPolygon> polys = new List<MapPolygon>();
         foreach (var p in o.Polygons) {
            if (p.IsVisible && p.IsHitTestVisible) {
               if (p.IsInside(pt)) {
                  polys.Add(p);
                  if (!all)
                     break;
               }
            }
         }
         return polys;
      }

      #region Backbuffer (für die Ausgabe eines Gesamtbildes)

      Bitmap? _backBuffer;

      /// <summary>
      /// neues Bitmap (<see cref="_backBuffer"/>) und Graphics (<see cref="g_GraphicsBackBuffer"/>) passend zur akt. Clientgröße erzeugen
      /// </summary>
      void updateBackBuffer() {
         clearBackBuffer();

         _backBuffer = new Bitmap(Width, Height);
         g_GraphicsBackBuffer = Graphics.FromImage(_backBuffer);
      }

      void clearBackBuffer() {
         if (_backBuffer != null) {
            _backBuffer.Dispose();
            _backBuffer = null;
         }

         if (g_GraphicsBackBuffer != null) {
            g_GraphicsBackBuffer.Dispose();
            g_GraphicsBackBuffer = null;
         }
      }

      #endregion

      void overlays_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
         if (e.NewItems != null) {
            foreach (MapOverlay obj in e.NewItems)
               if (obj != null)
                  obj.Control = this;

            if (g_core.IsStarted &&
                !M_HoldInvalidation)
               M_CoreInvalidate();
         }
      }

      /// <summary>
      /// Refresh des Controls (unabhängig vom Core)
      /// </summary>
      void controlRefresh() {
         M_HoldInvalidation = false;      // falls Akt. noch mit true gesperrt ist
#if GMAP4SKIA
         if (MainThread.IsMainThread)
            InvalidateSurface();
         else
            MainThread.BeginInvokeOnMainThread(() => InvalidateSurface());
#else
         RunInMainThread(Refresh);
#endif
      }

      void controlInvalidate() =>
#if !GMAP4SKIA
         RunInMainThread(Invalidate);
#else
         MainThread.BeginInvokeOnMainThread(() => InvalidateSurface());
#endif

      /// <summary>
      /// liefert den benötigten Bildausschnitt
      /// </summary>
      /// <param name="img"></param>
      /// <param name="corex"></param>
      /// <param name="corey"></param>
      /// <param name="width"></param>
      /// <param name="height"></param>
      void getImageSourceArea(MapImage img, out int corex, out int corey, out int width, out int height) =>
         getImageSourceArea(img, (int)img.Ix, (int)img.Xoff, (int)img.Yoff, out corex, out corey, out width, out height);

      void getImageSourceArea(MapImage img, int ix, int xoff, int yoff, out int corex, out int corey, out int width, out int height) {
         if (img.Img != null) {
            corex = xoff * (img.Img.Width / ix);
            corey = yoff * (img.Img.Height / ix);
            width = img.Img.Width / ix;
            height = img.Img.Height / ix;
         } else {
            corex =
            corey =
            width =
            height = 0;
         }
      }

      void tileLoadChanged(long elapsedMilliseconds) {
         if (elapsedMilliseconds >= 0) {
            M_TileLoadComplete?.Invoke(this, new TileLoadCompleteEventArgs(elapsedMilliseconds));
            tileLoadChangeLoop();   // Loop notfalls starten
         } else {
            M_TileLoadStart?.Invoke(this, EventArgs.Empty);
         }
      }

      AutoResetEvent autoResetEventEndLoop = new AutoResetEvent(false);
      long tileLoadChangeLoopIsRunning = 0;

      /// <summary>
      /// die Loop fragt in einem eigenen Task die WaitingTiles ab und löst bei einer Veränderung ein Event aus
      /// </summary>
      /// <param name="ms"></param>
      void tileLoadChangeLoop(int ms = 1000) {
         if (Interlocked.CompareExchange(ref tileLoadChangeLoopIsRunning, 1, 0) == 0) {
            Task.Run(() => {
               int lastcount = -1;
               do {
                  int count = g_core.WaitingTiles();
                  if (lastcount != count) {
                     lastcount = count;
                     if (M_TileLoadChange != null)
                        RunInMainThread(() => M_TileLoadChange(this, new TileLoadChangeEventArgs(false, count)));
                  }
               } while (!autoResetEventEndLoop.WaitOne(ms)); // wartet bis zum Set(), aber max. ms Milliesekunden und liefert nur bei Set() true
               Interlocked.Exchange(ref tileLoadChangeLoopIsRunning, 0);
            });
         }
      }


      void RunInMainThread(Action action) {
#if GMAP4SKIA
         if (!MainThread.IsMainThread)
            MainThread.BeginInvokeOnMainThread(action);
#else
         // im Thread dieses Controls (also der UI) starten:
         if (InvokeRequired)
            Invoke(action);
#endif
         else
            action();
      }



      void setCursorHandOnEnter() {
         if (_overObjectCount <= 0) {
            _overObjectCount = 0;
            setCursor4OverObjects();
         }
      }

      internal void M_RestoreCursorOnLeave() {
         if (_overObjectCount <= 0) {
            _overObjectCount = 0;
            setLastCursor();
         }
      }

#if !GMAP4SKIA
      void setCursor4Drag() => setCursor(Cursors.SizeAll);

      void setCursor4Selection() => setCursor(Cursors.Cross);

      void setCursor4OverObjects() => setCursor(Cursors.Hand);

      void setLastCursor() {
         if (g_cursorBefore != null)
            base.Cursor = g_cursorBefore;
         g_cursorBefore = null;
      }
#else
      void setLastCursor() { }
      void setCursor4Drag() { }
      void setCursor4Selection() { }
      void setCursor4OverObjects() { }

#endif

      void setCursor(Cursor newcursor) {
#if !GMAP4SKIA
         if (base.Cursor != newcursor) {
            setLastCursor();
            g_cursorBefore = Cursor;
            base.Cursor = newcursor;
         }
#endif
      }


      #region Provider

      RoutingProvider getRoutingProvider() {
         var dp = M_Provider as RoutingProvider;
         if (dp == null)
            dp = GMapProviders.OpenStreetMap as RoutingProvider; // use OpenStreetMap if provider does not implement routing
         return dp;
      }

      DirectionsProvider? getDirectionsProvider() {
         var dp = M_Provider as DirectionsProvider;
         if (dp == null)
            dp = GMapProviders.OpenStreetMap as DirectionsProvider; // use OpenStreetMap if provider does not implement routing
         return dp;
      }

      GeocodingProvider getGeocodingProvider() {
         var dp = M_Provider as GeocodingProvider;
         if (dp == null)
            dp = GMapProviders.OpenStreetMap as GeocodingProvider; // use OpenStreetMap if provider does not implement routing
         return dp;
      }

      RoadsProvider getRoadsProvider() {
         var dp = M_Provider as RoadsProvider;
         if (dp == null)
            dp = GMapProviders.GoogleMap as RoadsProvider; // use GoogleMap if provider does not implement routing
         return dp;
      }

      #endregion

      #region privat Konvertierungen

      /// <summary>
      /// Umrechnung von Clientkoordinaten in geograf. Länge/Breite
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      void client2LonLat(int clientx, int clienty, out double lon, out double lat) {
         PointD ptgeo = M_PointLatLng2PointD(fromLocalToLatLng(clientx, clienty));
         lon = ptgeo.X;
         lat = ptgeo.Y;
      }

      /// <summary>
      /// Umrechnung eines <see cref="PointLatLng"/> in Clientkoordinaten
      /// </summary>
      /// <param name="latlng"></param>
      /// <param name="xclient"></param>
      /// <param name="yclient"></param>
      void fromLatLngToLocal(PointLatLng latlng, out int xclient, out int yclient) {
         GPoint p = fromLatLngToLocal(latlng);
         xclient = (int)p.X;
         yclient = (int)p.Y;
      }

      /// <summary>
      /// Clientkoordinate (X) in Core-Koordinate umrechnen
      /// </summary>
      /// <param name="clientx"></param>
      /// <returns></returns>
      int clientx2core(int clientx) =>
         (int)Math.Round((clientx + Width * (g_extendedFractionalZoom - 1) / 2) / (g_extendedFractionalZoom * _deviceZoom));

      /// <summary>
      /// Clientkoordinate (Y) in Core-Koordinate umrechnen
      /// </summary>
      /// <param name="clienty"></param>
      /// <returns></returns>
      int clienty2core(int clienty) =>
         (int)Math.Round((clienty + Height * (g_extendedFractionalZoom - 1) / 2) / (g_extendedFractionalZoom * _deviceZoom));


#if USEMATRIXTRANSFORM

      void transform(Matrix m, Point[] pts) => m.TransformPoints(pts);

      void transform(Matrix m, ref int x, ref int y) {
         Point[] pts = new Point[] {
               Point.Empty,
            };
         pts[0].X = x;
         pts[0].Y = y;
         transform(m, pts);
         x = pts[0].X;
         y = pts[0].Y;
      }

      GPoint transform(Matrix m, GPoint pt) {
         Point[] pts = new Point[] {
               Point.Empty,
            };
         pts[0].X = (int)pt.X;
         pts[0].Y = (int)pt.Y;
         transform(m, pts);
         pt.X = pts[0].X;
         pt.Y = pts[0].Y;
         return pt;
      }

      /// <summary>
      /// gets world coordinate from client (control) coordinate
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      PointLatLng fromLocalToLatLng(int clientx, int clienty) {
         transform(getTransformationMatrix(true, false), ref clientx, ref clienty);    // Client -> CoreClient
         return core.FromLocalCoreToLatLng(clientx, clienty);
      }

      /// <summary>
      /// gets client (control) coordinate from world coordinate
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      GPoint fromLatLngToLocal(PointLatLng latlng) => transform(getTransformationMatrix(false, false),
                                                                           core.FromLatLngToLocalCore(latlng));

#else

      /// <summary>
      /// gets world coordinate from client (control) coordinate
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      PointLatLng fromLocalToLatLng(int clientx, int clienty) =>
        g_core.FromLocalCoreToLatLng(clientx2core(clientx), clienty2core(clienty));

      /// <summary>
      /// gets client (control) coordinate from world coordinate
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      GPoint fromLatLngToLocal(PointLatLng latlng) {
         GPoint ptcore = g_core.FromLatLngToLocalCore(latlng);
         ptcore.X = (long)Math.Round(ptcore.X * g_extendedFractionalZoom * _deviceZoom - Width * (g_extendedFractionalZoom - 1) / 2);
         ptcore.Y = (long)Math.Round(ptcore.Y * g_extendedFractionalZoom * _deviceZoom - Height * (g_extendedFractionalZoom - 1) / 2);
         return ptcore;
      }

#endif

      #endregion

      #endregion

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      bool _isdisposed = false;

      /// <summary>
      /// kann explizit für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public
#if !GMAP4SKIA
         new
#endif
         void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected
#if !GMAP4SKIA
         override
#endif
         void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

               g_Overlays.CollectionChanged -= overlays_CollectionChanged;

               foreach (var o in g_Overlays)
                  o.Dispose();

               g_Overlays.Clear();

               clearBackBuffer();
               if (g_core != null &&
                   g_core.IsStarted)
                  g_core.MapClose();
               g_core?.Dispose();

               semaphorePosZoom.Dispose();
               semaphoreOnPaint.Dispose();
               autoResetEventEndLoop.Set();
               autoResetEventEndLoop.Dispose();

               _emptyTileBrush?.Dispose();
               _selectedAreaFillBrush?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist

#if !GMAP4SKIA
            base.Dispose(notfromfinalizer);
#endif
         }
      }

      #endregion

   }
}
