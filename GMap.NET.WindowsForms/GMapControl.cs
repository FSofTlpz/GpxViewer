//#define WITHORGCODE
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.Projections;
#if GMAP4SKIA
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xamarin.Essentials;
#else
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
#endif

#if GMAP4SKIA
namespace GMap.NET.Skia {
#else
namespace GMap.NET.WindowsForms {
#endif
   /// <summary>
   ///     GMap.NET control for Windows Forms
   /// </summary>
   public partial class GMapControl :
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

         public ExceptionThrownEventArgs(Exception exception) {
            Exception = exception;
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
         public readonly float RenderTranform;
         public readonly Graphics Graphics;

         public DrawExtendedEventArgs(Graphics g, float rendertranform) {
            RenderTranform = rendertranform;
            Graphics = g;
         }
      }

      public class GMapMarkerEventArgs {
         public readonly GMapMarker Marker;

         public readonly MouseEventArgs Mea;

         public GMapMarkerEventArgs(GMapMarker marker, MouseEventArgs e) {
            Marker = marker;
            Mea = e;
         }
      }

      public class GMapTrackEventArgs {
         public readonly GMapTrack Track;

         public readonly MouseEventArgs Mea;

         public GMapTrackEventArgs(GMapTrack track, MouseEventArgs e) {
            Track = track;
            Mea = e;
         }
      }

      public class GMapPolygonEventArgs {
         public readonly GMapPolygon Polygon;

         public readonly MouseEventArgs Mea;

         public GMapPolygonEventArgs(GMapPolygon polygon, MouseEventArgs e) {
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
      /// Core: occurs when current position is changed
      /// </summary>
      public event EventHandler<PositionChangedEventArgs> OnMapPositionChanged;

      /// <summary>
      /// Core: occurs on map drag
      /// </summary>
      public event EventHandler OnMapDrag;

      /// <summary>
      /// Core: occurs on map zoom changed
      /// </summary>
      public event EventHandler OnMapZoomChanged;

      /// <summary>
      /// Core: occurs when tile set is starting to load
      /// </summary>
      public event EventHandler OnMapTileLoadStart;

      /// <summary>
      /// Core: occurs on empty tile displayed
      /// </summary>
      public event EventHandler<EmptyTileErrorEventArgs> OnMapEmptyTileError;

      /// <summary>
      /// Core: occurs when tile set load is complete
      /// </summary>
      public event EventHandler<TileLoadCompleteEventArgs> OnMapTileLoadComplete;

      /// <summary>
      /// Core: occurs on map type changed
      /// </summary>
      public event EventHandler<MapTypeChangedEventArgs> OnMapTypeChanged;


      /// <summary>
      /// wenn der Zoom geändert wurde (NICHT <see cref="OnMapZoomChanged"/>! Das wird wird nur bei gazzahligen Änderungen aufgerufen.)
      /// </summary>
      public event EventHandler OnMapFracionalZoomChanged;

      /// <summary>
      ///   occurs when clicked on map.
      /// </summary>
      public event EventHandler<MapClickEventArgs> OnMapClick;

      /// <summary>
      ///     occurs when double clicked on map.
      /// </summary>
      public event EventHandler<MapClickEventArgs> OnMapDoubleClick;


      /// <summary>
      ///     occurs when clicked on marker
      /// </summary>
      public event EventHandler<GMapMarkerEventArgs> OnMapMarkerClick;

      /// <summary>
      ///     occurs when double clicked on marker
      /// </summary>
      public event EventHandler<GMapMarkerEventArgs> OnMapMarkerDoubleClick;

      /// <summary>
      ///     occurs on mouse enters marker area
      /// </summary>
      public event EventHandler<GMapMarkerEventArgs> OnMapMarkerEnter;

      /// <summary>
      ///     occurs on mouse leaves marker area
      /// </summary>
      public event EventHandler<GMapMarkerEventArgs> OnMapMarkerLeave;


      /// <summary>
      ///     occurs when clicked on track
      /// </summary>
      public event EventHandler<GMapTrackEventArgs> OnMapTrackClick;

      /// <summary>
      ///     occurs when double clicked on track
      /// </summary>
      public event EventHandler<GMapTrackEventArgs> OnMapTrackDoubleClick;

      /// <summary>
      ///     occurs on mouse enters track area
      /// </summary>
      public event EventHandler<GMapTrackEventArgs> OnMapTrackEnter;

      /// <summary>
      ///     occurs on mouse leaves track area
      /// </summary>
      public event EventHandler<GMapTrackEventArgs> OnMapTrackLeave;


      /// <summary>
      ///     occurs when clicked on polygon
      /// </summary>
      public event EventHandler<GMapPolygonEventArgs> OnMapPolygonClick;

      /// <summary>
      ///     occurs when double clicked on polygon
      /// </summary>
      public event EventHandler<GMapPolygonEventArgs> OnMapPolygonDoubleClick;

      /// <summary>
      ///     occurs on mouse enters Polygon area
      /// </summary>
      public event EventHandler<GMapPolygonEventArgs> OnMapPolygonEnter;

      /// <summary>
      ///     occurs on mouse leaves Polygon area
      /// </summary>
      public event EventHandler<GMapPolygonEventArgs> OnMapPolygonLeave;


      /// <summary>
      ///     occurs when mouse selection is changed
      /// </summary>
      public event EventHandler<SelectionChangeEventArgs> OnMapSelectionChange;

      /// <summary>
      ///     occurs when an exception is thrown inside the map control
      /// </summary>
      public event ExceptionThrown OnMapExceptionThrown;

#if GMAP4SKIA

      // Im Nicht-Windows-System ex. die folgenden Events noch nicht. Sie werden def. und bei Bedarf selbst ausgelöst.

      public event EventHandler<PaintEventArgs> Paint;

      public event EventHandler<MouseEventArgs> MouseClick;

      public event EventHandler<MouseEventArgs> MouseDoubleClick;

      /// <summary>
      /// Tritt ein, wenn sich der Mauszeiger über dem Steuerelement befindet und eine Maustaste gedrückt wird.
      /// </summary>
      public event EventHandler<MouseEventArgs> MouseDown;

      /// <summary>
      /// Tritt ein, wenn sich der Mauszeiger über dem Steuerelement befindet und eine Maustaste losgelassen wird.
      /// </summary>
      public event EventHandler<MouseEventArgs> MouseUp;

      /// <summary>
      /// Tritt ein, wenn der Mauszeiger über dem Steuerelement bewegt wird.
      /// </summary>
      public event EventHandler<MouseEventArgs> MouseMove;

      /// <summary>
      /// Tritt ein, wenn der Mauszeiger den Bereich des Steuerelements verlässt.
      /// </summary>
      public event EventHandler MouseLeave;

#endif

      #endregion

      #region public Props / Vars

      public static int Map_ThreadPoolSize {
         get => PublicCore.ThreadPoolSize;
         set => PublicCore.ThreadPoolSize = value;
      }

      /// <summary>
      /// current map center position (lat/lgn)
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public PointLatLng Map_Position {
         get => core.Position;
         set {
            if (core.Position != value) {
               core.Position = value;
               if (core.IsStarted)
                  forceUpdateOverlays();
            }
         }
      }

      /// <summary>
      ///     location of cache
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public string Map_CacheLocation {
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

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      [Browsable(false)]
      public RectLatLng Map_ViewArea => core.ViewArea;

      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public GMapProvider Map_Provider {
         get => core.Provider;
         set {
            if (core.Provider == null || !core.Provider.Equals(value)) {
               var viewarea = Map_SelectedArea;

               if (viewarea != RectLatLng.Empty) {
                  Map_Position = new PointLatLng(viewarea.Lat - viewarea.HeightLat / 2,
                      viewarea.Lng + viewarea.WidthLng / 2);
               } else {
                  viewarea = Map_ViewArea;
               }

               core.Provider = value;

               if (core.IsStarted) {
                  if (core.ZoomToArea) {
                     // restore zoomrect as close as possible
                     if (viewarea != RectLatLng.Empty && viewarea != Map_ViewArea) {
                        int bestZoom = core.GetMaxZoomToFitRect(viewarea);
                        if (bestZoom > 0 && Map_Zoom != bestZoom) {
                           Map_Zoom = bestZoom;
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
      [Category("GMap.NET")]
      public bool Map_CanDragMap {
         get => core.CanDragMap;
         set => core.CanDragMap = value;
      }

      bool _isDragging = false;

      public bool Map_IsDragging {
         get => _isDragging;
         set => _isDragging = value;
      }

      ///// <summary>
      ///// map render mode
      ///// </summary>
      //[Browsable(false)]
      //public RenderMode MapRenderMode {
      //   get => Core.RenderMode;
      //   internal set => Core.RenderMode = value;
      //}

      [Category("GMap.NET")]
      [Description("map scale type")]
      public ScaleModes Map_ScaleMode { get; set; } = ScaleModes.Integer;

      /// <summary>
      /// zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)
      /// </summary>
      [Browsable(false)]
      public float Map_RenderZoom2RealDevice {
         get => _map_RenderZoom2RealDevice ?? 1;
         set {
            if (value == 1)
               _map_RenderZoom2RealDevice = null;
            else
               _map_RenderZoom2RealDevice = value;
         }
      }

      /// <summary>
      /// retry count to get tile
      /// </summary>
      [Browsable(false)]
      public int Map_RetryLoadTile {
         get => core.RetryLoadTile;
         set => core.RetryLoadTile = value;
      }

      /// <summary>
      /// how many levels of tiles are staying decompresed in memory
      /// </summary>
      [Browsable(false)]
      public int Map_LevelsKeepInMemory {
         get => core.LevelsKeepInMemory;
         set => core.LevelsKeepInMemory = value;
      }

      double _zoomReal;

      /// <summary>
      /// akt. Zoom (exponentiell, d.h. +1 bedeutet Verdopplung)
      /// </summary>
      [Category("GMap.NET")]
      [DefaultValue(12)]
      public double Map_Zoom {
         get => _zoomReal;
         set {
            if (_zoomReal != value) {
               if (value > Map_MaxZoom) {
                  _zoomReal = Map_MaxZoom;
               } else if (value < Map_MinZoom) {
                  _zoomReal = Map_MinZoom;
               } else {
                  _zoomReal = value;
               }

               double remainder = value % 1;
               if (Map_ScaleMode == ScaleModes.Fractional && remainder != 0) {
                  //_extraZoom = 1 + remainder;
                  float scaleValue = (float)Math.Pow(2d, remainder);
                  _map_RenderTransform = scaleValue;

                  map_ZoomStep = Convert.ToInt32(value - remainder);
               } else {
                  _map_RenderTransform = null;
                  _zoomReal = map_ZoomStep = (int)Math.Floor(value);
               }

               if (core.IsStarted && !_isDragging) {
                  forceUpdateOverlays();

                  OnMapFracionalZoomChanged?.Invoke(this, new EventArgs());
               }
            }
         }
      }

      /// <summary>
      /// (linearer) Zoomfaktor von <see cref="Map_Zoom"/> (bezogen auf <see cref="Map_MinZoom"/>)
      /// </summary>
      [Category("GMap.NET")]
      public double Map_ZoomLinear {
         get => Math.Pow(2.0, Map_Zoom - Map_MinZoom);
         set {
            if (value >= 1)
               Map_Zoom = Math.Log(value, 2) + Map_MinZoom;
         }
      }

      /// <summary>
      /// min zoom
      /// </summary>
      [Category("GMap.NET")]
      [Description("minimum zoom level of map")]
      public int Map_MinZoom {
         get => core.MinZoom;
         set => core.MinZoom = value;
      }

      /// <summary>
      /// max zoom
      /// </summary>
      [Category("GMap.NET")]
      [Description("maximum zoom level of map")]
      public int Map_MaxZoom {
         get => core.MaxZoom;
         set => core.MaxZoom = value;
      }

      /// <summary>
      /// is tracks enabled
      /// </summary>
      [Category("GMap.NET")]
      public bool Map_TracksEnabled {
         get => core.RoutesEnabled; set => core.RoutesEnabled = value;
      }

      /// <summary>
      /// is polygons enabled
      /// </summary>
      [Category("GMap.NET")]
      public bool Map_PolygonsEnabled {
         get => core.PolygonsEnabled; set => core.PolygonsEnabled = value;
      }

      /// <summary>
      /// is markers enabled
      /// </summary>
      [Category("GMap.NET")]
      public bool Map_MarkersEnabled {
         get => core.MarkersEnabled; set => core.MarkersEnabled = value;
      }

      /// <summary>
      /// Ist der Mapservice bereit?
      /// </summary>
      public bool Map_ServiceIsReady => core != null && core.IsStarted;

      /// <summary>
      /// map dragg button
      /// </summary>
      [Category("GMap.NET")]
      public MouseButtons Map_DragButton = MouseButtons.Right;

      /// <summary>
      /// map zooming type for mouse wheel
      /// </summary>
      [Category("GMap.NET")]
      [Description("map zooming type for mouse wheel")]
      public MouseWheelZoomType Map_MouseWheelZoomType {
         get => core.MouseWheelZoomType;
         set => core.MouseWheelZoomType = value;
      }

      /// <summary>
      /// vervielfacht die scheinbare Trackbreite (bei 1 wird nur die echte Trackbreite verwendet)
      /// </summary>
      public float Map_ClickTolerance4Tracks { get => map_ClickTolerance4Tracks; set => map_ClickTolerance4Tracks = value; }

      /// <summary>
      /// if true, selects area just by holding mouse and moving
      /// </summary>
      [Category("GMap.NET")]
      public bool Map_DisableAltForSelection = false;

      RectLatLng _selectedArea;

      /// <summary>
      /// current selected area in map
      /// </summary>
      [Browsable(false)]
      public RectLatLng Map_SelectedArea {
         get => _selectedArea;
         protected set {
            _selectedArea = value;
            if (core.IsStarted)
               Map_CoreInvalidate();
         }
      }

      [Browsable(false)]
      public RoutingProvider Map_RoutingProvider {
         get {
            var dp = Map_Provider as RoutingProvider;
            if (dp == null)
               dp = GMapProviders.OpenStreetMap as RoutingProvider; // use OpenStreetMap if provider does not implement routing
            return dp;
         }
      }

      [Browsable(false)]
      public DirectionsProvider Map_DirectionsProvider {
         get {
            var dp = Map_Provider as DirectionsProvider;
            if (dp == null)
               dp = GMapProviders.OpenStreetMap as DirectionsProvider; // use OpenStreetMap if provider does not implement routing
            return dp;
         }
      }

      [Browsable(false)]
      public GeocodingProvider Map_GeocodingProvider {
         get {
            var dp = Map_Provider as GeocodingProvider;
            if (dp == null)
               dp = GMapProviders.OpenStreetMap as GeocodingProvider; // use OpenStreetMap if provider does not implement routing
            return dp;
         }
      }

      [Browsable(false)]
      public RoadsProvider Map_RoadsProvider {
         get {
            var dp = Map_Provider as RoadsProvider;
            if (dp == null)
               dp = GMapProviders.GoogleMap as RoadsProvider; // use GoogleMap if provider does not implement routing
            return dp;
         }
      }

      #region Def. einiger Anzeigen

      /// <summary>
      /// backgroundcolor for map
      /// </summary>
      [Category("GMap.NET")]
      public Color Map_EmptyMapBackgroundColor { get; set; } = Color.WhiteSmoke;

      #region Def. der Darstellung für leere Tiles

      /// <summary>
      /// enables filling empty tiles using lower level images
      /// </summary>
      [Category("GMap.NET")]
      public bool Map_FillEmptyTiles { get => core.FillEmptyTiles; set => core.FillEmptyTiles = value; }

      /// <summary>
      /// text on empty tiles
      /// </summary>
      [Category("GMap.NET")]
      public string Map_EmptyTileText { get; set; } = "no image";

      [Category("GMap.NET")]
      public Font Map_EmptyTileFont = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold);

      Color _emptyTileColor = Color.Navy;
      Brush _emptyTileBrush = new SolidBrush(Color.Navy);

      /// <summary>
      /// color of empty tile background
      /// </summary>
      [Category("GMap.NET")]
      [Description("background color of the empty tile")]
      public Color Map_EmptyTileColor {
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
      [Category("GMap.NET")]
      public Pen Map_EmptyTileBordersPen = new Pen(Brushes.White, 1);

      #endregion

      #region Def. für Darstellung Auswahl

      /// <summary>
      /// area selection pen
      /// </summary>
      [Category("GMap.NET")]
      public Pen Map_SelectionPen = new Pen(Brushes.Blue, 10);

      Brush _selectedAreaFillBrush = new SolidBrush(Color.FromArgb(33, Color.RoyalBlue));

      Color _selectedAreaFillColor = Color.FromArgb(33, Color.RoyalBlue);

      /// <summary>
      /// background of selected area
      /// </summary>
      [Category("GMap.NET")]
      [Description("background color od the selected area")]
      public Color Map_SelectedAreaFillColor {
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
      [Category("GMap.NET")]
      [Description("shows tile gridlines")]
      public bool Map_ShowTileGridLines {
         get {
            return _showTileGridLines;
         }
         set {
            _showTileGridLines = value;
            Map_CoreInvalidate();
         }
      }

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
      [Category("GMap.NET")]
      public Pen Map_TileGridLinesPen = new Pen(Brushes.White, 5);

      [Category("GMap.NET")]
      public Font Map_TileGridLinesFont = new Font(FontFamily.GenericSansSerif,
#if GMAP4SKIA
                                                  35,
#else
                                                  7,
#endif
                                                  FontStyle.Bold);

      #endregion

      [Category("GMap.NET")]
      public Font Map_CopyrightFont { get; set; } = new Font(FontFamily.GenericSansSerif,
#if GMAP4SKIA
                                              35,
#else
                                              7,
#endif
                                              FontStyle.Regular);

      #endregion

      #endregion

      #region internal Props / Vars

      /// <summary>
      /// stops immediate marker/track/polygon invalidations;
      /// call Refresh to perform single refresh and reset invalidation state
      /// </summary>
      [Browsable(false)]
      internal bool HoldInvalidation;

      bool _isMouseOverMarker;
      int _overObjectCount;

      /// <summary>
      /// is mouse over marker
      /// </summary>
      [Browsable(false)]
      internal bool Map_IsMouseOverMarker {
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
      internal bool Map_IsMouseOverTrack {
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
      internal bool Map_IsMouseOverPolygon {
         get => _isMouseOverPolygon;
         set {
            _isMouseOverPolygon = value;
            _overObjectCount += value ? 1 : -1;
         }
      }

      #endregion

      #region protected Props / Vars

      static readonly bool IsDesignerHosted =
#if GMAP4SKIA
         // Damit wurde ursprünglich das Control im Design-Modus passiv gehalten. Jetzt fkt. allerdings der Debug-Modus damit nicht mehr.
         false;  // = Xamarin.Forms.DesignMode.IsDesignModeEnabled ???
#else
         LicenseManager.UsageMode == LicenseUsageMode.Designtime;
#endif

      protected int map_ZoomStep {
         get => core.Zoom;
         set => core.Zoom = value;
      }

      protected float map_RenderTransform => _map_RenderTransform ?? 1;

      /// <summary>
      /// gets map manager (<see cref="GMaps.Instance"/>)
      /// </summary>
      [Browsable(false)]
      protected GMaps map_Manager => GMaps.Instance;

      /// <summary>
      /// list of overlays, should be thread safe
      /// </summary>
      protected readonly ObservableCollectionThreadSafe<GMapOverlay> map_Overlays = new ObservableCollectionThreadSafe<GMapOverlay>();

      bool _showCoreDataForTest = false;

      /// <summary>
      /// nur für Test: zeigt einige akt. Daten des Core auf dem Bildschirm an
      /// </summary>
      [Browsable(false)]
      protected bool map_ShowCoreData4Test {
         get => _showCoreDataForTest;
         set {
            if (_showCoreDataForTest != value) {
               _showCoreDataForTest = value;
               Map_CoreInvalidate();
            }
         }
      }

      /// <summary>
      /// Stift für Mittelpunktmarkierung
      /// </summary>
      [Category("GMap.NET")]
      protected Pen map_CenterPen = new Pen(Color.Red,
#if GMAP4SKIA
         5
#else
         1
#endif
         );

      #endregion

      #region privat Vars

      //#if !GMAP4SKIA
      //      ColorMatrix _colorMatrix;

      //      protected ColorMatrix MapColorMatrix {
      //         get => _colorMatrix;
      //         set {
      //            _colorMatrix = value;
      //            if (GMapImageProxy.TileImageProxy != null && GMapImageProxy.TileImageProxy is GMapImageProxy) {
      //               (GMapImageProxy.TileImageProxy as GMapImageProxy).ColorMatrix = value;
      //               if (Core.IsStarted) {
      //                  MapReload();
      //               }
      //            }
      //         }
      //      }
      //#endif

      GPoint map_CoreRenderOffset => core.RenderOffset;

      /// <summary>
      /// map boundaries
      /// </summary>
      RectLatLng? boundsOfMap = null;

      /// <summary>
      /// prevents focusing map if mouse enters it's area
      /// </summary>
      bool map_DisableFocusOnMouseEnter = false;

      /// <summary>
      /// reverses MouseWheel zooming direction
      /// </summary>
      bool map_InvertedMouseWheelZooming = false;

      /// <summary>
      /// lets you zoom by MouseWheel even when pointer is in area of marker
      /// </summary>
      bool map_IgnoreMarkerOnMouseWheel = false;

      /// <summary>
      /// Zusätzliche Skalierung zu <see cref="Map_Zoom"/> (null oder 1.0..2.0)?
      /// </summary>
      float? _map_RenderTransform;

      /// <summary>
      /// zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)
      /// </summary>
      float? _map_RenderZoom2RealDevice = null;

      PointLatLng selectionStart;
      PointLatLng selectionEnd;

      readonly Font missingDataFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
      readonly StringFormat centerFormat = new StringFormat();
      readonly StringFormat bottomFormat = new StringFormat();
      bool isSelected;

#if !GMAP4SKIA
      readonly ImageAttributes tileFlipXYAttributes = new ImageAttributes();
      Cursor cursorBefore = Cursors.Default;
#else
      Cursor cursorBefore = null;
#endif

#if GMAP4SKIA
      /// <summary>
      /// Gets the width and height of a rectangle centered on the point the mouse button was pressed, within which a drag operation will not begin.
      /// </summary>
      readonly Size map_DragSize = new Size(5, 5);
#else
      /// <summary>
      /// Gets the width and height of a rectangle centered on the point the mouse button was pressed, within which a drag operation will not begin.
      /// </summary>
      readonly Size map_DragSize = SystemInformation.DragSize;
#endif

      Graphics map_GraphicsBackBuffer = null;

      readonly PublicCore core = new PublicCore();

      #endregion


      static GMapControl() {
         if (!IsDesignerHosted) {
            GMapImageProxy.Enable();
            GMaps.Instance.SQLitePing();
         }
      }

#if !DESIGN
      public GMapControl() {
         if (!IsDesignerHosted) {
#if GMAP4SKIA
            Font = new Font("Arial", 35);
#else
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            ResizeRedraw = true;

            tileFlipXYAttributes.SetWrapMode(WrapMode.TileFlipXY);

            centerFormat.Alignment = StringAlignment.Center;
            centerFormat.LineAlignment = StringAlignment.Center;
            bottomFormat.Alignment = StringAlignment.Center;
            bottomFormat.LineAlignment = StringAlignment.Far;

            if (GMaps.Instance.IsRunningOnMono) // no imports to move pointer
               Map_MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;

#endif

            core.OnCurrentPositionChanged += Core_OnCurrentPositionChanged;
            core.OnEmptyTileError += Core_OnEmptyTileError;
            core.OnMapDrag += Core_OnMapDrag;
            core.OnMapTypeChanged += Core_OnMapTypeChanged;
            core.OnMapZoomChanged += Core_OnMapZoomChanged;
            core.OnTileLoadComplete += Core_OnTileLoadComplete;
            core.OnTileLoadStart += Core_OnTileLoadStart;

            map_Overlays.CollectionChanged += overlays_CollectionChanged;
         }
      }

#endif

      #region Umsetzung der Core-Events

      private void Core_OnTileLoadStart() {
         OnMapTileLoadStart?.Invoke(this, EventArgs.Empty);
      }

      private void Core_OnTileLoadComplete(long elapsedMilliseconds) {
         OnMapTileLoadComplete?.Invoke(this, new TileLoadCompleteEventArgs(elapsedMilliseconds));
      }

      private void Core_OnMapZoomChanged() {
         OnMapZoomChanged?.Invoke(this, EventArgs.Empty);
      }

      private void Core_OnMapTypeChanged(GMapProvider type) {
         OnMapTypeChanged?.Invoke(this, new MapTypeChangedEventArgs(type));
      }

      private void Core_OnMapDrag() {
         OnMapDrag?.Invoke(this, EventArgs.Empty);
      }

      private void Core_OnEmptyTileError(int zoom, GPoint pos) {
         OnMapEmptyTileError?.Invoke(this, new EmptyTileErrorEventArgs(zoom, pos));
      }

      private void Core_OnCurrentPositionChanged(PointLatLng point) {
         OnMapPositionChanged?.Invoke(this, new PositionChangedEventArgs(point));
      }

      #endregion

      #region Event-Funktionen

#if !GMAP4SKIA
      protected override void OnMouseDown(MouseEventArgs e) {
         base.OnMouseDown(e);
#else
      protected void OnMouseDown(MouseEventArgs e) {
         MouseDown?.Invoke(this, e);
#endif
         if (!Map_IsMouseOverMarker) {
            if (e.Button == Map_DragButton && Map_CanDragMap) {
               core.MouseDown = new GPoint(e.X, e.Y);
               Map_CoreInvalidate();
            } else if (!isSelected) {
               isSelected = true;
               Map_SelectedArea = RectLatLng.Empty;
               selectionEnd = PointLatLng.Empty;
               selectionStart = Map_FromLocalToLatLng(e.X, e.Y);
            }
         }
      }

#if !GMAP4SKIA
#else
#endif

#if !GMAP4SKIA
      protected override void OnMouseUp(MouseEventArgs e) {
         base.OnMouseUp(e);
#else
      protected void OnMouseUp(MouseEventArgs e) {
         MouseUp?.Invoke(this, e);
#endif
         isSelected = false;

         if (core.IsDragging) {
            if (_isDragging) {
               _isDragging = false;
#if !GMAP4SKIA
               Cursor = cursorBefore;
               cursorBefore = null;
#endif
            }

            core.EndDrag();

            if (boundsOfMap.HasValue &&
                !boundsOfMap.Value.Contains(Map_Position) &&
                core.LastLocationInBounds.HasValue)
               Map_Position = core.LastLocationInBounds.Value;

            //////////////////////////////////////////////////////////
            float scale = this.scale();
            if (Math.Floor(scale) != scale)
               Map_Position = Map_FromLocalToLatLng(Width / 2, Height / 2);   // Bei nichtganzzahligem Zoom ist eine Korrektur nötig!
            //////////////////////////////////////////////////////////

         } else {
            if (e.Button == Map_DragButton)
               core.MouseDown = GPoint.Empty;

            if (!selectionEnd.IsEmpty &&
                !selectionStart.IsEmpty) {
               bool zoomtofit = false;

               if (!Map_SelectedArea.IsEmpty && ModifierKeys == Keys.Shift)
                  zoomtofit = Map_SetZoomToFitRect(Map_SelectedArea);

               OnMapSelectionChange?.Invoke(this, new SelectionChangeEventArgs(Map_SelectedArea, zoomtofit));
            } else {
               Map_CoreInvalidate();
            }
         }
      }

#if !GMAP4SKIA
      protected override void OnMouseClick(MouseEventArgs e) {
         base.OnMouseClick(e);
         _onMouseClick(e, false, false, out _, out _, out _);
      }
#else
      PointLatLng OnMouseClick(MouseEventArgs e,
                               bool all,
                               out List<GMapMarker> markers,
                               out List<GMapTrack> tracks,
                               out List<GMapPolygon> polygons) {
         MouseClick?.Invoke(this, e);
         return _onMouseClick(e,
                              false,
                              all,
                              out markers,
                              out tracks,
                              out polygons);
      }
#endif

      /// <summary>
      /// liefert die Objektlisten an der Position und löst für die betroffenen Objekte die Click-Events aus
      /// </summary>
      /// <param name="e"></param>
      /// <param name="doubleclick"></param>
      /// <param name="all"></param>
      /// <param name="markers"></param>
      /// <param name="tracks"></param>
      /// <param name="polygons"></param>
      /// <returns></returns>
      PointLatLng _onMouseClick(MouseEventArgs e,
                                bool doubleclick,
                                bool all,
                                out List<GMapMarker> markers,
                                out List<GMapTrack> tracks,
                                out List<GMapPolygon> polygons) {

         GPoint rp = core.RealClientPoint(e.X, e.Y);

         PointLatLng point = PointLatLng.Empty;
         markers = new List<GMapMarker>();
         tracks = new List<GMapTrack>();
         polygons = new List<GMapPolygon>();

         if (!core.IsDragging) {
            bool overlayObject = false;

            for (int i = map_Overlays.Count - 1; i >= 0; i--) {
               var o = map_Overlays[i];

               if (o != null && o.IsVisibile && o.IsHitTestVisible) {

                  List<GMapMarker> markers4o = new List<GMapMarker>(getMarkers4Point(o, rp, all));
                  List<GMapTrack> tracks4o = new List<GMapTrack>(getTracks4Point(o, rp, all, Map_ClickTolerance4Tracks));
                  List<GMapPolygon> polygons4o = new List<GMapPolygon>(getPolygons4Point(o, e.X, e.Y, all));

                  markers.AddRange(markers4o);
                  tracks.AddRange(tracks4o);
                  polygons.AddRange(polygons4o);

                  foreach (var m in markers4o) {
                     if (doubleclick)
                        OnMapMarkerDoubleClick?.Invoke(this, new GMapMarkerEventArgs(m, e));
                     else
                        OnMapMarkerClick?.Invoke(this, new GMapMarkerEventArgs(m, e));
                  }

                  foreach (var t in tracks4o) {
                     if (doubleclick)
                        OnMapTrackDoubleClick?.Invoke(this, new GMapTrackEventArgs(t, e));
                     else
                        OnMapTrackClick?.Invoke(this, new GMapTrackEventArgs(t, e));
                  }

                  foreach (var p in polygons4o) {
                     if (doubleclick)
                        OnMapPolygonDoubleClick?.Invoke(p, new GMapPolygonEventArgs(p, e));
                     else
                        OnMapPolygonClick?.Invoke(p, new GMapPolygonEventArgs(p, e));
                  }

               }
               overlayObject = markers.Count > 0 || tracks.Count > 0 || polygons.Count > 0;
               if (overlayObject && !all)
                  break;
            }

            if (!overlayObject && core.MouseDown != GPoint.Empty)
               point = Map_FromLocalToLatLng(e.X, e.Y);
         }

         return point;
      }

#if !GMAP4SKIA
      protected override void OnMouseDoubleClick(MouseEventArgs e) {
         base.OnMouseDoubleClick(e);
         _onMouseClick(e, true, false, out _, out _, out _);
      }
#else
      PointLatLng OnMouseDoubleClick(MouseEventArgs e,
                                     bool all,
                                     out List<GMapMarker> markers,
                                     out List<GMapTrack> tracks,
                                     out List<GMapPolygon> polygons) {
         MouseDoubleClick?.Invoke(this, e);
         return _onMouseClick(e,
                              true,
                              all,
                              out markers,
                              out tracks,
                              out polygons);
      }
#endif

#if !GMAP4SKIA
      protected override void OnMouseMove(MouseEventArgs e) {
         base.OnMouseMove(e);
#else
      /// <summary>
      /// zur Simulation einer Mausbewegung (auch mit ModifierKeys == Keys.Alt für Selektion!)
      /// </summary>
      /// <param name="e"></param>
      protected virtual void OnMouseMove(MouseEventArgs e) {
         MouseMove?.Invoke(this, e);
#endif
         if (!core.IsDragging &&             // noch nicht gestartet ...
             !core.MouseDown.IsEmpty) {      // ... und Startpunkt bei MouseDown registriert ...
            var p = new GPoint(e.X, e.Y);
            if (Math.Abs(p.X - core.MouseDown.X) * 2 >= map_DragSize.Width ||  // ... und Mindestweite der Bewegung vorhanden
                Math.Abs(p.Y - core.MouseDown.Y) * 2 >= map_DragSize.Height)
               core.BeginDrag(core.MouseDown);  // Dragging mit diesen Clientkoordinaten starten
         }

         if (core.IsDragging) {
            if (!_isDragging) {
               _isDragging = true;
#if !GMAP4SKIA
               cursorBefore = Cursor;
               Cursor = Cursors.SizeAll;
#endif
            }

            if (boundsOfMap.HasValue && !boundsOfMap.Value.Contains(Map_Position)) {
               // ...
            } else {
               GPoint pt = new GPoint(e.X, e.Y);
               core.Drag(pt);
               forceUpdateOverlays();
               controlInvalidate();
            }
         } else {
            if (isSelected &&
                !selectionStart.IsEmpty &&
                (ModifierKeys == Keys.Alt || ModifierKeys == Keys.Shift || Map_DisableAltForSelection)) {
               selectionEnd = Map_FromLocalToLatLng(e.X, e.Y);
               {
                  var p1 = selectionStart;
                  var p2 = selectionEnd;

                  double x1 = Math.Min(p1.Lng, p2.Lng);
                  double y1 = Math.Max(p1.Lat, p2.Lat);
                  double x2 = Math.Max(p1.Lng, p2.Lng);
                  double y2 = Math.Min(p1.Lat, p2.Lat);

                  Map_SelectedArea = new RectLatLng(y1, x1, x2 - x1, y1 - y2);
               }
            } else
            if (core.MouseDown.IsEmpty) {
               for (int i = map_Overlays.Count - 1; i >= 0; i--) {
                  GPoint rp = core.RealClientPoint(e.X, e.Y);

                  var o = map_Overlays[i];
                  if (o != null && o.IsVisibile && o.IsHitTestVisible) {

                     List<GMapMarker> markers = getMarkers4Point(o, rp, true);
                     List<GMapTrack> tracks = getTracks4Point(o, rp, true, Map_ClickTolerance4Tracks);
                     List<GMapPolygon> polygons = getPolygons4Point(o, e.X, e.Y, true);

                     foreach (var m in o.Markers) {
                        if (m.IsVisible &&
                            m.IsHitTestVisible) {
                           if (markers.Contains(m)) {
                              if (!m.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 m.IsMouseOver = true;
                                 Map_IsMouseOverMarker = true;
                                 OnMapMarkerEnter?.Invoke(this, new GMapMarkerEventArgs(m, null));
                                 Map_CoreInvalidate();
                              }
                           } else if (m.IsMouseOver) {
                              m.IsMouseOver = false;
                              Map_IsMouseOverMarker = false;
                              RestoreCursorOnLeave();
                              OnMapMarkerLeave?.Invoke(this, new GMapMarkerEventArgs(m, null));
                              Map_CoreInvalidate();
                           }
                        }
                     }

                     foreach (var t in o.Tracks) {
                        if (t.IsVisible &&
                            t.IsHitTestVisible) {
                           if (tracks.Contains(t)) {
                              if (!t.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 t.IsMouseOver = true;
                                 Map_IsMouseOverTrack = true;
                                 OnMapTrackEnter?.Invoke(this, new GMapTrackEventArgs(t, null));
                                 Map_CoreInvalidate();
                              }
                           } else if (t.IsMouseOver) {
                              t.IsMouseOver = false;
                              Map_IsMouseOverTrack = false;
                              RestoreCursorOnLeave();
                              OnMapTrackLeave?.Invoke(this, new GMapTrackEventArgs(t, null));
                              Map_CoreInvalidate();
                           }
                        }
                     }

                     foreach (var p in o.Polygons) {
                        if (p.IsVisible &&
                            p.IsHitTestVisible) {
                           if (polygons.Contains(p)) {
                              if (!p.IsMouseOver) {
                                 setCursorHandOnEnter();
                                 p.IsMouseOver = true;
                                 Map_IsMouseOverPolygon = true;
                                 OnMapPolygonEnter?.Invoke(this, new GMapPolygonEventArgs(p, null));
                                 Map_CoreInvalidate();
                              }
                           } else if (p.IsMouseOver) {
                              p.IsMouseOver = false;
                              Map_IsMouseOverPolygon = false;
                              RestoreCursorOnLeave();
                              OnMapPolygonLeave?.Invoke(this, new GMapPolygonEventArgs(p, null));
                              Map_CoreInvalidate();
                           }
                        }
                     }
                  }
               }
            }
         }
      }


#if !GMAP4SKIA
      protected override void OnPaint(PaintEventArgs e) {
#else
      protected virtual void OnPaint(PaintEventArgs e) {
#endif
         try {
            if (map_GraphicsBackBuffer != null) {
               drawGraphics(map_GraphicsBackBuffer);
               e.Graphics.DrawImage(_backBuffer, 0, 0);
            } else
               drawGraphics(e.Graphics);
#if !GMAP4SKIA
            base.OnPaint(e);
#endif
         } catch (Exception ex) {
            if (OnMapExceptionThrown != null)
               OnMapExceptionThrown.Invoke(new Exception("OnPaint", ex));
            else
               throw;
         }
#if GMAP4SKIA
         Paint?.Invoke(this, e);
#endif
      }

#if !GMAP4SKIA
      protected override void OnSizeChanged(EventArgs e) {
         base.OnSizeChanged(e);
#else
      protected virtual void OnSizeChanged(EventArgs e) {
         MainThread.InvokeOnMainThreadAsync(() => {
            Width = (int)Math.Round(XamarinX2SkiaX(base.Width));
            Height = (int)Math.Round(XamarinY2SkiaY(base.Height));
         }).Wait();
#endif
         if (Width == 0 || Height == 0)
            return;

         //if (Width == core.Width && Height == core.Height)
         //   return;

         if (!IsDesignerHosted) {
            core.OnMapSizeChanged(Width, Height);

            if (Visible &&
                IsHandleCreated &&
                core.IsStarted) {

               forceUpdateOverlays();
            }
         }
      }

#if !GMAP4SKIA
      protected override void OnLoad(EventArgs e) {
#else
      protected virtual void OnLoad(EventArgs e) {
#endif
         try {
#if !GMAP4SKIA
            base.OnLoad(e);
#else
            // Skia-Event auf OnSizeChanged umlenken
            SizeChanged += (object sender, EventArgs ea) => {
               OnSizeChanged(ea);
            };

            // Skia-Event auf OnPaint umlenken
            PaintSurface += (object sender, SKPaintSurfaceEventArgs ea) => {
               Graphics g = new Graphics(ea.Surface.Canvas);
               OnPaint(new PaintEventArgs(g, new Rectangle(0, 0, ea.Info.Width, ea.Info.Height)));
               g.Dispose();
            };
#endif
            if (!IsDesignerHosted) {
               //MethodInvoker m = delegate
               //{
               //   Thread.Sleep(444);

               //OnSizeChanged(null);

               if (_lazyEvents) {
                  _lazyEvents = false;

                  if (_lazySetZoomToFitRect.HasValue) {
                     Map_SetZoomToFitRect(_lazySetZoomToFitRect.Value);
                     _lazySetZoomToFitRect = null;
                  }
               }

               core.OnMapOpen().ProgressChanged += invalidatorEngage;
               forceUpdateOverlays();
               //};
               //this.BeginInvoke(m);
            }
         } catch (Exception ex) {
            if (OnMapExceptionThrown != null)
               OnMapExceptionThrown.Invoke(new Exception("OnLoad", ex));
            else
               throw;
         }
      }

      /// <summary>
      /// override, to render something more
      /// </summary>
      /// <param name="g"></param>
      protected virtual void OnPaintOverlays(Graphics g) {
         try {
            g.SmoothingMode = SmoothingMode.HighQuality;
            foreach (var o in map_Overlays)
               if (o.IsVisibile)
                  o.OnRender(g);

            // separate tooltip drawing
            foreach (var o in map_Overlays)
               if (o.IsVisibile)
                  o.OnRenderToolTips(g);

            g.ResetTransform();

            if (!Map_SelectedArea.IsEmpty) {
               var p1 = Map_FromLatLngToLocal(Map_SelectedArea.LocationTopLeft);
               var p2 = Map_FromLatLngToLocal(Map_SelectedArea.LocationRightBottom);

               long x1 = p1.X;
               long y1 = p1.Y;
               long x2 = p2.X;
               long y2 = p2.Y;

               g.DrawRectangle(Map_SelectionPen, x1, y1, x2 - x1, y2 - y1);
               g.FillRectangle(_selectedAreaFillBrush, x1, y1, x2 - x1, y2 - y1);
            }

            #region -- copyright --

            if (!string.IsNullOrEmpty(core.Provider.Copyright)) {
#if GMAP4SKIA
               g.DrawString(core.Provider.Copyright,
                            Map_CopyrightFont,
                            Brushes.Navy as SolidBrush,
                            new PointF(15, Height - Map_CopyrightFont.GetHeight() - 25));
#else
               g.DrawString(core.Provider.Copyright,
                            Map_CopyrightFont,
                            Brushes.Navy,
                            3,
                            Height - Map_CopyrightFont.Height - 5);
#endif
            }

            #endregion

            OnMapDrawCenter(new DrawExtendedEventArgs(g, map_RenderTransform));

            OnMapDrawScale(new DrawExtendedEventArgs(g, map_RenderTransform));

            OnMapDrawOnTop(new DrawExtendedEventArgs(g, map_RenderTransform));

         } catch (Exception ex) {
            if (OnMapExceptionThrown != null)
               OnMapExceptionThrown.Invoke(new Exception("OnPaintOverlays", ex));
            else
               throw new Exception("OnPaintOverlays: " + ex.Message);
         }
      }

      protected virtual void OnMapDrawCenter(DrawExtendedEventArgs e) {
         int r = Math.Min(Width, Height) / 50;
         e.Graphics.DrawLine(map_CenterPen,
                             Width / 2 - r,
                             Height / 2,
                             Width / 2 + r,
                             Height / 2);
         e.Graphics.DrawLine(map_CenterPen,
                             Width / 2,
                             Height / 2 - r,
                             Width / 2,
                             Height / 2 + r);
      }

      protected virtual void OnMapDrawScale(DrawExtendedEventArgs e) {
         //         int top = MapScaleInfoPosition == MapScaleInfoPositions.Top ?
         //                           10 :
         //#if !GMAP4SKIA
         //                           Bottom
         //#else
         //                                 Height
         //#endif
         //                           - 30;
         //         int left = 10;
         //         int bottom = top + 7;

         //         if (Width > Core.PxRes5000Km)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes5000Km, bottom, left, "5000 km");

         //         if (Width > Core.PxRes1000Km)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes1000Km, bottom, left, "1000 km");

         //         if (Width > Core.PxRes100Km && MapZoom > 2)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes100Km, bottom, left, "100 km");

         //         if (Width > Core.PxRes10Km && MapZoom > 5)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes10Km, bottom, left, "10 km");

         //         if (Width > Core.PxRes1000M && MapZoom >= 10)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes1000M, bottom, left, "1000 m");

         //         if (Width > Core.PxRes100M && MapZoom > 11)
         //            drawSimpleScale(e.Graphics, top, left + Core.PxRes100M, bottom, left, "100 m");
      }

      protected virtual void OnMapDrawOnTop(DrawExtendedEventArgs e) { }

      #region Event-Funktionen nur für Standard-Windows-Control-Events

#if !GMAP4SKIA
      protected override void OnCreateControl() {
         try {
            base.OnCreateControl();

            if (!IsDesignerHosted) {
               var f = ParentForm;
               if (f != null) {
                  while (f.ParentForm != null)
                     f = f.ParentForm;

                  if (f != null)
                     f.FormClosing += ParentForm_FormClosing;
               }
            }
         } catch (Exception ex) {
            if (OnMapExceptionThrown != null)
               OnMapExceptionThrown.Invoke(ex);
            else
               throw;
         }
      }

      void ParentForm_FormClosing(object sender, FormClosingEventArgs e) {
         //if (e.CloseReason == CloseReason.WindowsShutDown ||
         //    e.CloseReason == CloseReason.TaskManagerClosing)
         map_Manager.CancelTileCaching();
         core.Dispose();
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         base.OnKeyDown(e);
      }

      protected override void OnKeyUp(KeyEventArgs e) {
         base.OnKeyUp(e);
      }

      bool _mouseIn;

      protected override void OnMouseEnter(EventArgs e) {
         base.OnMouseEnter(e);

         if (!map_DisableFocusOnMouseEnter)
            Focus();

         _mouseIn = true;
      }

      protected override void OnMouseLeave(EventArgs e) {
         base.OnMouseLeave(e);
         _mouseIn = false;
      }

      public bool MouseWheelZoomEnabled = true;

      protected override void OnMouseWheel(MouseEventArgs e) {
         base.OnMouseWheel(e);

         if (MouseWheelZoomEnabled &&
             _mouseIn &&
             (!Map_IsMouseOverMarker || map_IgnoreMarkerOnMouseWheel) &&
             !core.IsDragging) {
            if (core.MouseLastZoom.X != e.X && core.MouseLastZoom.Y != e.Y) {
               switch (Map_MouseWheelZoomType) {
                  case MouseWheelZoomType.MousePositionAndCenter:
                  case MouseWheelZoomType.MousePositionWithoutCenter:
                     core.SetPositionDirect(Map_FromLocalToLatLng(e.X, e.Y));
                     break;
                  case MouseWheelZoomType.ViewCenter:
                     core.SetPositionDirect(Map_FromLocalToLatLng(Width / 2, Height / 2));
                     break;
               }
               core.MouseLastZoom = new GPoint(e.X, e.Y);
            }

            // set mouse position to map center
            if (Map_MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter) {
               if (!GMaps.Instance.IsRunningOnMono) {
                  var p = PointToScreen(new Point(Width / 2, Height / 2));
                  PublicCore.SetMousePosition(p.X, p.Y);
               }
            }

            core.MouseWheelZooming = true;

            if (e.Delta > 0) {
               if (!map_InvertedMouseWheelZooming)
                  Map_Zoom = (int)Map_Zoom + 1;
               else
                  Map_Zoom = (int)(Map_Zoom + 0.99) - 1;
            } else if (e.Delta < 0) {
               if (!map_InvertedMouseWheelZooming)
                  Map_Zoom = (int)(Map_Zoom + 0.99) - 1;
               else
                  Map_Zoom = (int)Map_Zoom + 1;
            }

            core.MouseWheelZooming = false;
         }
      }

#endif

      #endregion

      #endregion

      #region Public-Funktionen

      /// <summary>
      ///     Call it to empty tile cache & reload tiles
      /// </summary>
      public void Map_Reload() => core.ReloadMap();

      /// <summary>
      ///     call this to stop HoldInvalidation and perform single forced instant refresh
      /// </summary>
      public void Map_Refresh() {
         HoldInvalidation = false;
         lock (core.InvalidationLock) {
            core.LastInvalidation = DateTime.Now;
         }

#if GMAP4SKIA
         InvalidateSurface();
#else
         base.Refresh();
#endif
      }

      /// <summary>
      /// alle Aktionen die nicht zum akt. Zoom gehören abbrechen
      /// </summary>
      /// <param name="all">alle oder alle außer dem akt. Zoom</param>
      public void Map_CancelUnnecessaryThreads(bool all = false) => core.CancelUnnecessaryThreads(all);

      /// <summary>
      /// Anzahl der Tiles die noch in der Warteschlange stehen
      /// </summary>
      /// <returns></returns>
      public int Map_TilesInQueue() => core.MapTilesInQueue();

      /// <summary>
      ///     gets world coordinate from local control coordinate
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public PointLatLng Map_FromLocalToLatLng(int x, int y) => core.FromLocalToLatLng(x, y, scale());

      /// <summary>
      ///     gets local coordinate from world coordinate
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      public GPoint Map_FromLatLngToLocal(PointLatLng point) => core.FromLatLngToLocal(point, scale());

      RectLatLng? _lazySetZoomToFitRect;
      bool _lazyEvents = true;

      /// <summary>
      /// sets zoom to max to fit rect
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool Map_SetZoomToFitRect(RectLatLng rect) {
         if (_lazyEvents) {
            _lazySetZoomToFitRect = rect;
         } else {
            int maxZoom = core.GetMaxZoomToFitRect(rect);
            if (maxZoom > 0) {
               var center = new PointLatLng(rect.Lat - rect.HeightLat / 2, rect.Lng + rect.WidthLng / 2);
               Map_Position = center;
               if (maxZoom > Map_MaxZoom)
                  maxZoom = Map_MaxZoom;
               if ((int)Map_Zoom != maxZoom)
                  Map_Zoom = maxZoom;
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// ermittelt das jeweils erste Objekt der Objektarten <see cref="GMapMarker"/>, <see cref="GMapTrack"/> und <see cref="GMapPolygon"/>, 
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
      public PointLatLng Map_Tapped(int clientx,
                                   int clienty,
                                   bool doubleclick,
                                   MouseButtons button,
                                   bool all,
                                   out List<GMapMarker> marker,
                                   out List<GMapTrack> track,
                                   out List<GMapPolygon> polygon) {
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

         marker = new List<GMapMarker>();
         track = new List<GMapTrack>();
         polygon = new List<GMapPolygon>();
         return new PointLatLng(0, 0);
#endif
      }

#if GMAP4SKIA
      /// <summary>
      /// gets image of the current view
      /// </summary>
      /// <returns></returns>
      public Bitmap MapToImage() {
         Bitmap ret = null;
         try {
            UpdateBackBuffer();
            Map_Refresh();
            //Application.DoEvents();

            using (var ms = new MemoryStream()) {
               using (var frame = _backBuffer.Clone() as Bitmap) {
                  frame.Save(ms, ImageFormat.Png);
               }
               ret = Bitmap.FromStream(ms);
            }
         } catch (Exception) {
            throw;
         } finally {
            ClearBackBuffer();
         }
         return ret;
      }
#else
      /// <summary>
      /// gets image of the current view
      /// </summary>
      /// <returns></returns>
      public Image MapToImage() {
         Image ret = null;
         try {
            UpdateBackBuffer();
            Map_Refresh();
            Application.DoEvents();

            using (var ms = new MemoryStream()) {
               using (var frame = _backBuffer.Clone() as Bitmap)
                  frame.Save(ms, ImageFormat.Png);
               ret = Image.FromStream(ms);
            }
         } catch (Exception) {
            throw;
         } finally {
            ClearBackBuffer();
         }
         return ret;
      }
#endif

#if !GMAP4SKIA && WITHORGCODE

      /// <summary>
      ///     shows map db export dialog
      /// </summary>
      /// <returns></returns>
      public bool Map_ShowExportDialog() {
         using (FileDialog dlg = new SaveFileDialog()) {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gmdb";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: Export map to db, if file exsist only new data will be added";
            dlg.FileName = "DataExp";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK) {
               bool ok = GMaps.Instance.ExportToGMDB(dlg.FileName);
               if (ok) {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
               } else {
                  MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }

         return false;
      }

      /// <summary>
      ///     shows map dbimport dialog
      /// </summary>
      /// <returns></returns>
      public bool Map_ShowImportDialog() {
         using (FileDialog dlg = new OpenFileDialog()) {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gmdb";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: Import to db, only new data will be added";
            dlg.FileName = "DataImport";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK) {
               bool ok = GMaps.Instance.ImportFromGMDB(dlg.FileName);
               if (ok) {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                  MapReload();
               } else {
                  MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }

         return false;
      }

#endif

#if WITHORGCODE

      /// <summary>
      ///     sets to max zoom to fit all markers and centers them in map
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public bool MapZoomAndCenterMarkers(string overlayId) {
         return MapZoomAndCenterRect(GetRectOfAllMarkers(overlayId));
      }

      /// <summary>
      ///     zooms and centers all tracks
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public bool MapZoomAndCenterTracks(string overlayId) {
         return MapZoomAndCenterRect(GetRectOfAllTracks(overlayId));
      }

      /// <summary>
      ///     zooms and centers tracks
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool MapZoomAndCenterTrack(MapRoute track) {
         return MapZoomAndCenterRect(GetRectOfTrack(track));
      }

      public bool MapZoomAndCenterRect(RectLatLng? rect) {
         return rect.HasValue ?
                        MapSetZoomToFitRect(rect.Value) :
                        false;
      }

#endif

      #endregion

      #region Internal-Funktionen

#if !DESIGN
      /// <summary>
      ///     enque built-in thread safe invalidation (only for internal use)
      /// </summary>
      internal void Map_CoreInvalidate() {
         core.StartRefresh();
#if GMAP4SKIA
         //MainThread.BeginInvokeOnMainThread(() => InvalidateSurface());
         //InvalidateSurface();
#endif
      }
#endif

      /// <summary>
      ///     updates markers local position (only for internal use)
      /// </summary>
      /// <param name="marker"></param>
      internal void Map_UpdateMarkerLocalPosition(GMapMarker marker) {
         GPoint p = core.FromLatLngToLocal(marker.Position, scale(), true);
         marker.LocalPosition = new Point((int)p.X, (int)p.Y);
      }

      /*    Es werden LocalPolyline.Count GPoints verworfen.
       *    Dann werden genauso viele GPoints neu erzeugt und in der Liste registriert.
       *    Das läßt sich leider nicht vermeiden und belastet die Speicherverwaltung.
       */


      ///// <summary>
      ///// updates tracks local position (only for internal use)
      ///// <para>optimized for creating lesser objects</para>
      ///// </summary>
      ///// <param name="track"></param>
      //internal void Map_UpdateTrackLocalPosition(GMapTrack track) {
      //   if (track.LocalPolyline.Count > track.Points.Count)
      //      track.LocalPolyline.RemoveRange(track.Points.Count, track.LocalPolyline.Count - track.Points.Count);
      //   for (int i = 0; i < track.Points.Count; i++) {
      //      GPoint p = core.FromLatLngToLocal(track.Points[i], scale(), true);
      //      if (track.LocalPolyline.Count <= i)
      //         track.LocalPolyline.Add(p);
      //      else { // etwas umständlich, aber es wird nur ein schon vorhandenes GPoint-Objekt verändert
      //         // Ersatz für: track.LocalPolyline.Add(p);

      //         track.LocalPolyline[i] = p;

      //         //track.LocalPolyline[i].OffsetNegative(track.LocalPolyline[i]);
      //         //track.LocalPolyline[i].Offset(p);
      //      }
      //   }
      //   track.UpdateVisualParts(map_CoreRenderOffset);
      //}

      internal void Map_UpdateTrackLocalPosition(GMapTrack track) {
         track.LocalPolyline = new Point[track.Points.Count];
         for (int i = 0; i < track.Points.Count; i++) {
            GPoint p = core.FromLatLngToLocal(track.Points[i], scale(), true);
            track.LocalPolyline[i] = new Point((int)p.X, (int)p.Y);
         }
         track.UpdateVisualParts(map_CoreRenderOffset);
      }

      /// <summary>
      ///     updates polygons local position (only for internal use)
      /// </summary>
      /// <param name="polygon"></param>
      internal void Map_UpdatePolygonLocalPosition(GMapPolygon polygon) {
         polygon.LocalPoints.Clear();
         for (int i = 0; i < polygon.Points.Count; i++) {
            GPoint p = core.FromLatLngToLocal(polygon.Points[i], scale(), true);
            polygon.LocalPoints.Add(p);
         }
         polygon.UpdateGraphicsPath();
      }

      internal void RestoreCursorOnLeave() {
#if !GMAP4SKIA
         if (_overObjectCount <= 0 && cursorBefore != null) {
            _overObjectCount = 0;
            Cursor = cursorBefore;
            cursorBefore = null;
         }
#endif
      }

      #endregion

      #region Privat-Funktionen

      /// <summary>
      ///     update objects when map is draged/zoomed
      /// </summary>
      void forceUpdateOverlays() {
         try {
            HoldInvalidation = true;
            foreach (var o in map_Overlays) {
               if (o.IsVisibile)
                  o.ForceUpdate();
            }
         } finally {
            Map_Refresh();
         }
      }

      List<GMapMarker> getMarkers4Point(GMapOverlay o, GPoint clientwithoffset, bool all = true) {
         List<GMapMarker> markers = new List<GMapMarker>();
         foreach (var m in o.Markers) {
            if (m.IsVisible && m.IsHitTestVisible) {
               if (m.LocalArea.Contains((int)clientwithoffset.X, (int)clientwithoffset.Y)) {
                  markers.Add(m);
                  if (!all)
                     break;
               }
            }
         }
         return markers;
      }

      List<GMapTrack> getTracks4Point(GMapOverlay o, GPoint clientwithoffset, bool all = true, float tolerance = 1F) {
         List<GMapTrack> tracks = new List<GMapTrack>();
         foreach (var t in o.Tracks) {
            if (t.IsVisible && t.IsHitTestVisible) {
               if (t.IsInside((int)clientwithoffset.X, (int)clientwithoffset.Y, tolerance)) {
                  tracks.Add(t);
                  if (!all)
                     break;
               }
            }
         }
         return tracks;
      }

      List<GMapPolygon> getPolygons4Point(GMapOverlay o, int clientx, int clienty, bool all = true) {
         List<GMapPolygon> polys = new List<GMapPolygon>();
         foreach (var p in o.Polygons) {
            if (p.IsVisible && p.IsHitTestVisible) {
               if (p.IsInside(Map_FromLocalToLatLng(clientx, clienty))) {
                  polys.Add(p);
                  if (!all)
                     break;
               }
            }
         }
         return polys;
      }

      #region Backbuffer (für die Ausgabe eines Gesamtbildes)

      Bitmap _backBuffer;

      /// <summary>
      /// neues Bitmap (<see cref="_backBuffer"/>) und Graphics (<see cref="map_GraphicsBackBuffer"/>) passend zur akt. Clientgröße erzeugen
      /// </summary>
      void UpdateBackBuffer() {
         ClearBackBuffer();

         _backBuffer = new Bitmap(Width, Height);
         map_GraphicsBackBuffer = Graphics.FromImage(_backBuffer);
      }

      void ClearBackBuffer() {
         if (_backBuffer != null) {
            _backBuffer.Dispose();
            _backBuffer = null;
         }

         if (map_GraphicsBackBuffer != null) {
            map_GraphicsBackBuffer.Dispose();
            map_GraphicsBackBuffer = null;
         }
      }

      #endregion

      //static double radians(double degrees) => degrees * (Math.PI / 180);

      //static double degrees(double radians) => radians * (180 / Math.PI);

      float scale() => map_RenderTransform * Map_RenderZoom2RealDevice;

      void overlays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
         if (e.NewItems != null) {
            foreach (GMapOverlay obj in e.NewItems)
               if (obj != null)
                  obj.Control = this;

            if (core.IsStarted &&
                !HoldInvalidation)
               Map_CoreInvalidate();
         }
      }

      void invalidatorEngage(object sender, ProgressChangedEventArgs e) {
         controlInvalidate();
      }

      void controlInvalidate() {
#if !GMAP4SKIA
         base.Invalidate();
#else
         MainThread.BeginInvokeOnMainThread(() => InvalidateSurface());
         //InvalidateSurface();
#endif
      }

      /// <summary>
      /// print some Core-Data to screen (only for testing)
      /// </summary>
      /// <param name="g"></param>
      void showCoreData(Graphics g) {
         g.ResetTransform();
         float h = Map_EmptyTileFont.GetHeight();
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

      void setCursorHandOnEnter() {
#if !GMAP4SKIA
         if (_overObjectCount <= 0 && Cursor != Cursors.Hand) {
            _overObjectCount = 0;
            cursorBefore = Cursor;
            Cursor = Cursors.Hand;
         }
#endif
      }

#if WITHORGCODE

      void drawSimpleScale(Graphics g, int top, int right, int bottom, int left, string caption) {
         g.DrawLine(MapScalePenBorder, left, top, left, bottom);
         g.DrawLine(MapScalePenBorder, left, bottom, right, bottom);
         g.DrawLine(MapScalePenBorder, right, bottom, right, top);

         g.DrawLine(MapScalePen, left, top, left, bottom);
         g.DrawLine(MapScalePen, left, bottom, right, bottom);
         g.DrawLine(MapScalePen, right, bottom, right, top);

         g.DrawString(caption, scaleFont, Brushes.Black, right + 3, top - 5);
      }

      /// <summary>
      /// apply transformation if in rotation mode
      /// </summary>
      GPoint applyRotation(int x, int y) {
         var ret = new GPoint(x, y);

         if (MapIsRotated) {
            var tt = new[] { new Point(x, y) };
            rotationMatrix.TransformPoints(tt);
            var f = tt[0];

            ret.X = f.X;
            ret.Y = f.Y;
         }

         return ret;
      }

      /// <summary>
      ///     gets rectangle with all objects inside
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all except zoomInsignificant</param>
      /// <returns></returns>
      protected RectLatLng? GetRectOfAllMarkers(string overlayId) {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         foreach (var o in MapOverlays) {
            if (overlayId == null && o.IsZoomSignificant || o.Id == overlayId) {
               if (o.IsVisibile && o.Markers.Count > 0) {
                  foreach (var m in o.Markers) {
                     if (m.IsVisible) {
                        // left
                        if (m.Position.Lng < left)
                           left = m.Position.Lng;
                        // top
                        if (m.Position.Lat > top)
                           top = m.Position.Lat;
                        // right
                        if (m.Position.Lng > right)
                           right = m.Position.Lng;
                        // bottom
                        if (m.Position.Lat < bottom)
                           bottom = m.Position.Lat;
                     }
                  }
               }
            }
         }

         if (left != double.MaxValue &&
             right != double.MinValue &&
             top != double.MinValue &&
             bottom != double.MaxValue)
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         return ret;
      }

      /// <summary>
      ///     gets rectangle with all objects inside
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all except zoomInsignificant</param>
      /// <returns></returns>
      protected RectLatLng? GetRectOfAllTracks(string overlayId) {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         foreach (var o in MapOverlays) {
            if (overlayId == null && o.IsZoomSignificant || o.Id == overlayId) {
               if (o.IsVisibile && o.Tracks.Count > 0) {
                  foreach (var t in o.Tracks) {
                     if (t.IsVisible && t.From.HasValue && t.To.HasValue) {
                        foreach (var p in t.Points) {
                           // left
                           if (p.Lng < left)
                              left = p.Lng;
                           // top
                           if (p.Lat > top)
                              top = p.Lat;
                           // right
                           if (p.Lng > right)
                              right = p.Lng;
                           // bottom
                           if (p.Lat < bottom)
                              bottom = p.Lat;
                        }
                     }
                  }
               }
            }
         }

         if (left != double.MaxValue &&
             right != double.MinValue &&
             top != double.MinValue &&
             bottom != double.MaxValue)
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         return ret;
      }

      /// <summary>
      ///     gets rect of track
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      protected RectLatLng? GetRectOfTrack(MapRoute track) {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         if (track.From.HasValue && track.To.HasValue) {
            foreach (var p in track.Points) {
               // left
               if (p.Lng < left)
                  left = p.Lng;
               // top
               if (p.Lat > top)
                  top = p.Lat;
               // right
               if (p.Lng > right)
                  right = p.Lng;
               // bottom
               if (p.Lat < bottom)
                  bottom = p.Lat;
            }
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         }
         return ret;
      }

#endif

      #endregion

      #region drawing the map (from OnPaint())

      /// <summary>
      /// draw the map and the overlays to graphics
      /// </summary>
      /// <param name="g"></param>
      void drawGraphics(Graphics g) {
         float scale = this.scale();
         if (scale == 1) {
            g.TranslateTransform(map_CoreRenderOffset.X, map_CoreRenderOffset.Y, MatrixOrder.Prepend);
            drawMap(g);
         } else {
            g.ScaleTransform(scale, scale, MatrixOrder.Append);
            g.TranslateTransform(map_CoreRenderOffset.X, map_CoreRenderOffset.Y, MatrixOrder.Append);
            drawMap(g);

            // neu setzen für OnPaintOverlays()
            g.ResetTransform();
            g.TranslateTransform(map_CoreRenderOffset.X, map_CoreRenderOffset.Y, MatrixOrder.Append);
         }
         OnPaintOverlays(g);

         if (map_ShowCoreData4Test)
            showCoreData(g);
      }

      void drawMap(Graphics g) {
         g.Clear(Map_EmptyMapBackgroundColor);

         if (core.UpdatingBounds ||
             Map_Provider == EmptyProvider.Instance ||
             Map_Provider == null)
            return;

         core.LockImageStore();

         g.TextRenderingHint = TextRenderingHint.AntiAlias;
         g.SmoothingMode = SmoothingMode.AntiAlias;
#if !GMAP4SKIA
         g.CompositingQuality = CompositingQuality.HighQuality;
         g.InterpolationMode = InterpolationMode.HighQualityBicubic;
#endif
         try {
            GPoint[] tilePosXYlist = core.GetTilePosXYDrawingList();
            for (int t = 0; t < tilePosXYlist.Length; t++) {
               GPoint tileLocation = core.GetTileDestination(t, out GSize tileSize);
               Rectangle rectTile = new Rectangle((int)tileLocation.X,
                                                  (int)tileLocation.Y,
                                                  (int)tileSize.Width,
                                                  (int)tileSize.Height);
               RectangleF rectTileF = new RectangleF(rectTile.X, rectTile.Y, rectTile.Width, rectTile.Height);

               GPoint tilePosXY = tilePosXYlist[t];
               Tile tile = core.GetTile(tilePosXY);
               bool found = false;
               if (tile.NotEmpty) {

                  foreach (GMapImage img in tile.Overlays) {               // jedes Image (?) des Tiles
                     if (img != null && img.Img != null) {
                        if (!found)
                           found = true;

#if GMAP4SKIA
                        if (!img.IsParent) {

                           g.SKCanvas.DrawBitmap(img.Img, rectConvert(rectTile));

                        } else {
                           getImageSourceArea(img, out int x, out int y, out int width, out int height);
                           g.SKCanvas.DrawBitmap(img.Img,
                                                 new SKRect(x,
                                                            y,
                                                            width,
                                                            height),
                                                 rectConvert(rectTile));

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
                                       tileFlipXYAttributes);
                        }
                     }
#endif
                  }

               } else if (Map_FillEmptyTiles &&
                          Map_Provider.Projection is MercatorProjection) {

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


                  g.FillRectangle(_selectedAreaFillBrush, rectTile);


               }

               // add text if tile is missing
               if (!found) {
                  Exception ex = core.GetException4FailedLoad(tilePosXY);
                  if (ex != null) {
                     g.FillRectangle(_emptyTileBrush, rectTile);

                     g.DrawString("Exception: " + ex.Message,
                                  missingDataFont,
                                  Brushes.Red as SolidBrush,
                                  new RectangleF(rectTile.X + 11,
                                                 rectTile.Y + 11,
                                                 rectTile.Width - 11,
                                                 rectTile.Height - 11));

                     g.DrawString(Map_EmptyTileText,
                                  missingDataFont,
                                  Brushes.Blue as SolidBrush,
                                  rectTileF,
                                  centerFormat);

                     g.DrawRectangle(Map_EmptyTileBordersPen, rectTile);
                  }
               }

               if (Map_ShowTileGridLines) {
                  g.DrawRectangle(Map_EmptyTileBordersPen, rectTile);
#if GMAP4SKIA
                  g.DrawString(tilePosXY.ToString(),
                               missingDataFont,
                               Brushes.Red as SolidBrush,
                               new PointF(rectTile.X, rectTile.Y));

                  //g.DrawString(_tilePointPosPixel[tp].ToString(),
                  //             MapTileGridLinesFont,
                  //             Brushes.Red as SolidBrush,
                  //             new PointF(rectTile.X, rectTile.Y + MapTileGridLinesFont.GetHeight()));
#else
                  g.DrawString(tilePosXY.ToString(),
                               missingDataFont,
                               Brushes.Red,
                               rectTileF,
                               centerFormat);
#endif
               }
            }
         } catch (Exception ex) {

            Debug.WriteLine(">>> Exception in DrawMap(): " + ex.Message);

         } finally {
            core.ReleaseImageStore();
         }
      }

      /// <summary>
      /// liefert den benötigten Bildausschnitt
      /// </summary>
      /// <param name="img"></param>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="width"></param>
      /// <param name="height"></param>
      void getImageSourceArea(GMapImage img, out int x, out int y, out int width, out int height) {
         getImageSourceArea(img, (int)img.Ix, (int)img.Xoff, (int)img.Yoff, out x, out y, out width, out height);
      }

      void getImageSourceArea(GMapImage img, int ix, int xoff, int yoff, out int x, out int y, out int width, out int height) {
         x = xoff * (img.Img.Width / ix);
         y = yoff * (img.Img.Height / ix);
         width = img.Img.Width / ix;
         height = img.Img.Height / ix;

      }

      #endregion

#if GMAP4SKIA

      #region static Konvertierungen Xamarin <-> Skia (Control-Koordinaten)

      public static float XamarinX2SkiaX(double x) {
         return (float)(x * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density);
      }

      public static float XamarinY2SkiaY(double y) {
         return (float)(y * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density);
      }

      public static double SkiaX2XamarinX(double x) {
         return x / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
      }

      public static double SkiaY2XamarinY(double y) {
         return y / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
      }

      /// <summary>
      /// rechnet einen Xamarin-Punkt in einen Skia-Punkt um
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      public static SKPoint Xamarin2Skia(Xamarin.Forms.Point pt) {
         return new SKPoint(XamarinX2SkiaX(pt.X),
                            XamarinY2SkiaY(pt.Y));
      }

      public static Xamarin.Forms.Point Xamarin2Skia(SKPoint pt) {
         return new Xamarin.Forms.Point(SkiaX2XamarinX(pt.X),
                                        SkiaY2XamarinY(pt.Y));
      }

      #endregion

      public void MapServiceStart(double lon,
                                  double lat,
                                  string cachepath,
                                  int zoom = 12,
                                  ScaleModes scaleModes = ScaleModes.Fractional) {
         if (!IsDesignerHosted &&
             !Map_ServiceIsReady) {

            if (!string.IsNullOrEmpty(cachepath)) {
               if (!Directory.Exists(cachepath))
                  Directory.CreateDirectory(cachepath);
               Map_CacheLocation = cachepath;
            }
            GMaps.Instance.Mode = string.IsNullOrEmpty(cachepath) ?
                                       AccessMode.ServerOnly :
                                       AccessMode.ServerAndCache;

            OnLoad(new EventArgs());

            Map_MinZoom = 0;
            Map_MaxZoom = 24;
            Map_Zoom = zoom;
            Map_ScaleMode = scaleModes;

            Map_Position = new PointLatLng(lat, lon);
         }
      }

      /// <summary>
      /// beendet den Mapservice
      /// </summary>
      public void MapServiceEnd() {
         if (Map_ServiceIsReady) {
            core.OnMapClose(); // Dispose
         }
      }

      #region simulating Mouse-Action for None-Windows-System

      /// <summary>
      /// Ruft die Position des Mauszeigers in Bildschirmkoordinaten ab. (z.Z. nicht verwendet)
      /// </summary>
      static System.Drawing.Point MousePosition { get; set; }

      enum MouseAction {
         MouseDown,
         MouseUp,
         //MouseClick,
         //MouseDoubleClick,
         MouseMove,
         //MouseWheel,
      }

      /// <summary>
      /// setzt die Mauspos. als Control-Koordinaten! (i.A. im Zusammenhang mit Touch-Ereignissen)
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      static void simulateMousePosition(int clientx, int clienty) {
         MousePosition = new Point(clientx, clienty);
      }

      /// <summary>
      /// zum Simulieren einer Mausaktion (i.A. im Zusammenhang mit Touch-Ereignissen)
      /// </summary>
      /// <param name="action"></param>
      /// <param name="button"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="delta"></param>
      Task simulateMouseAction(MouseAction action, MouseButtons button, int clientx, int clienty, int delta) {
         return Task.Run(() => {
            simulateMousePosition(clientx, clienty);
            MouseEventArgs ea = new MouseEventArgs(button, clientx, clienty, delta);
            switch (action) {
               case MouseAction.MouseDown: OnMouseDown(ea); break;
               case MouseAction.MouseUp: OnMouseUp(ea); break;
               case MouseAction.MouseMove: OnMouseMove(ea); break;
            }
         });
      }

      /// <summary>
      /// Beginn der Kartenverschiebung
      /// </summary>
      /// <param name="startpt"></param>
      public Task MapDragStart(Xamarin.Forms.Point startpt) {
         return simulateMouseAction(MouseAction.MouseDown,
                                    MouseButtons.Right,
                                    (int)XamarinX2SkiaX(startpt.X),
                                    (int)XamarinY2SkiaY(startpt.Y),
                                    0);
      }

      /// <summary>
      /// Kartenverschiebung
      /// </summary>
      /// <param name="actualpt"></param>
      public Task MapDrag(Xamarin.Forms.Point actualpt) {
         return simulateMouseAction(MouseAction.MouseMove,
                                    MouseButtons.Right,
                                    (int)XamarinX2SkiaX(actualpt.X),
                                    (int)XamarinY2SkiaY(actualpt.Y),
                                    0);
      }

      /// <summary>
      /// Kartenverschiebung beendet
      /// </summary>
      /// <param name="endpt"></param>
      public Task MapDragEnd(Xamarin.Forms.Point endpt) {
         return simulateMouseAction(MouseAction.MouseUp,
                                    MouseButtons.Right,
                                    (int)XamarinX2SkiaX(endpt.X),
                                    (int)XamarinY2SkiaY(endpt.Y),
                                    0);
      }

      /// <summary>
      /// verschiebt die Karte (i.A. um eine größere Entfernung)
      /// </summary>
      /// <param name="deltalon"></param>
      /// <param name="deltalat"></param>
      public Task MapMove(double deltalon, double deltalat) {
         return Task.Run(() => {
            Map_Position = new PointLatLng(Map_Position.Lat + deltalat,
                                           Map_Position.Lng - deltalon);
         });
      }

      #endregion

      SKRect rectConvert(GRect rect) {
         return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
      }

      SKRect rectConvert(Rectangle rect) {
         return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
      }


      /// <summary>
      /// Ersatz für den Windows Control-Font
      /// </summary>
      public Font Font { get; protected set; }

      int _width, _height;

      /// <summary>
      /// Ruft die Höhe des Steuerelements ab (in Skia-Koordinaten)
      /// </summary>
      public new int Height {
         get => _height;
         set => _height = value;
      }

      /// <summary>
      /// Ruft die Breite des Steuerelements ab (in Skia-Koordinaten)
      /// </summary>
      public new int Width {
         get => _width;
         set => _width = value;
      }

      public int ClientSizeWidth => Width;

      public int ClientSizeHeight => Height;

      /// <summary>
      /// Setzt den Eingabefokus auf das Steuerelement.
      /// </summary>
      public new void Focus() => base.Focus();

      /// <summary>
      /// Ruft einen Wert ab, der angibt, ob dem Steuerelement ein Handle zugeordnet ist.
      /// </summary>
      public bool IsHandleCreated => base.Width > 0 && base.Height > 0;

      /// <summary>
      /// Ruft einen Wert ab, mit dem angegeben wird, ob das Steuerelement und alle untergeordneten Steuerelemente angezeigt werden, oder legt diesen Wert fest.
      /// </summary>
      public bool Visible => base.IsVisible;

      /// <summary>
      /// Ruft einen Wert ab, der angibt, welche der Zusatztasten (Umschalttaste, STRG und ALT) gedrückt ist.
      /// <para>Gibt es bei Android nicht.</para>
      /// </summary>
      public System.Windows.Forms.Keys ModifierKeys => System.Windows.Forms.Keys.None;

#endif

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      bool _isdisposed = false;
      private float map_ClickTolerance4Tracks = 1F;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
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

               map_Overlays.CollectionChanged -= overlays_CollectionChanged;

               foreach (var o in map_Overlays)
                  o.Dispose();

               map_Overlays.Clear();

               ClearBackBuffer();
               if (core != null &&
                   core.IsStarted)
                  core.OnMapClose();
               core.Dispose();


               //MapEmptyTileBordersPen.Dispose();
               //_emptyTileBrush.Dispose();
               //MapEmptyTileFont.Dispose();

               //MapScaleFont.Dispose();
               //MapScalePen.Dispose();

               //_selectedAreaFillBrush.Dispose();
               //MapSelectionPen.Dispose();

               //MapCenterPen.Dispose();

               //MapTileGridLinesPen.Dispose();
               //MapTileGridLinesFont.Dispose();

               //CenterFormat.Dispose();
               //BottomFormat.Dispose();
               //MapCopyrightFont.Dispose();

               //MapEmptyTileBorders.Dispose();

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
