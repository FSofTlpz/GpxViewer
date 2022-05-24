using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
#if GMAP4SKIA
using GMap.NET.Skia;
#else
using GMap.NET.WindowsForms;
#endif
using System;
using System.Drawing;

namespace SmallMapControl {
   public partial class SmallMapCtrl : GMapControl {

      /*
       * Dieser Teil stellt ein Interface dar, der dem anderen Teil einen kleinen Teil der Basisklasse über die SMC_-Elemente zur Verfügung stellt. Hier muss bei Bedarf
       * die Anpasung an eine veränderte Basisklasse erfolgen.
       * Der andere Teil muss die  eventSMC_On...-Funktionen enthalten.
       */

      /// <summary>
      /// akt. aktiver <see cref="GMapProvider"/>
      /// </summary>
      public GMapProvider SMC_MapProvider {
         get => MapProvider;
         set => MapProvider = value;
      }
      /// <summary>
      /// Pfad zum Cache, falls nicht der Standardpfad
      /// </summary>
      public string SMC_CacheLocation {
         get => CacheLocation;
         set => CacheLocation = value;
      }
      /// <summary>
      /// Darüber erfolgt der Zugriff auf die Caches.
      /// </summary>
      public GMaps SMC_Manager {
         get => Manager;
      }
      /// <summary>
      /// Hintergrundfarbe für die Karte
      /// </summary>
      public Color SMC_EmptyMapBackground {
         get => EmptyMapBackground;
         set => EmptyMapBackground = value;
      }

      /// <summary>
      /// akt. Kartenpos. (Mitte)
      /// </summary>
      public PointLatLng SMC_Position {
         get => Position;
         set => Position = value;
      }

      /// <summary>
      /// zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...) und die Darstellung damit zu klein ist
      /// </summary>
      public float SMC_MapRenderZoom2RealDevice {
         get => MapRenderZoom2RealDevice;
         set => MapRenderZoom2RealDevice = value;
      }

      public float SMC_MapRenderTransform => MapRenderTransform;

      /// <summary>
      /// max. Zoom (i.A. 24)
      /// </summary>
      public int SMC_MaxZoom {
         get => MaxZoom;
         set => MaxZoom = value;
      }
      /// <summary>
      /// min. Zoom (i.A. 0)
      /// </summary>
      public int SMC_MinZoom {
         get => MinZoom;
         set => MinZoom = value;
      }
      /// <summary>
      /// akt. Zoom (<see cref="SMC_MinZoom"/> ... <see cref="SMC_MaxZoom"/>)
      /// </summary>
      public double SMC_Zoom {
         get => Zoom;
         set => Zoom = value;
      }
      /// <summary>
      /// akt. angezeigter Bereich in geogr. Koordinaten
      /// </summary>
      public RectLatLng SMC_ViewArea {
         get => ViewArea;
      }

      /// <summary>
      /// kleines rotes Kreuz (nur sinnvoll für Debug)
      /// </summary>
      public bool SMC_ShowCenter {
         get => ShowCenter;
         set => ShowCenter = value;
      }
      /// <summary>
      /// Ränder der Tiles anzeigen (nur sinnvoll für Debug)
      /// </summary>
      public bool SMC_ShowTileGridLines {
         get => ShowTileGridLines;
         set => ShowTileGridLines = value;
      }

      /// <summary>
      /// Farbe leerer Tiles
      /// </summary>
      public Color SMC_EmptyTileColor {
         get => EmptyTileColor;
         set => EmptyTileColor = value;
      }
      /// <summary>
      /// Text für leere Tiles
      /// </summary>
      public string SMC_EmptyTileText {
         get => EmptyTileText;
         set => EmptyTileText = value;
      }

      /// <summary>
      /// akt. Cursor für die Karte
      /// </summary>
      public System.Windows.Forms.Cursor SMC_Cursor {
         get => Cursor;
         set => Cursor = value;
      }
      /// <summary>
      /// Mausbutton der zum "ziehen" verwendet wird
      /// </summary>
      public System.Windows.Forms.MouseButtons SMC_DragButton {
         get => DragButton;
         set => DragButton = value;
      }

      /// <summary>
      /// Liste der Karten-Overlays (für Tracks und Marker)
      /// </summary>
      public ObservableCollectionThreadSafe<GMapOverlay> SMC_Overlays {
         get => Overlays;
      }


      public SmallMapCtrl() { }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);

         // Original-Events auf eventSMC_On-Funktionen im anderen Teil umleiten

         Paint += eventSMC_OnPaint;

         MouseDown += eventSMC_OnMouseDown;
         MouseMove += eventSMC_OnMouseMove;
         MouseUp += eventSMC_OnMouseUp;
         MouseLeave += eventSMC_OnMouseLeave;
         MouseClick += eventSMC_OnMouseClick;

         OnMapZoomChanged += eventSMC_OnMapZoomChanged;

         OnTileLoadStart += eventSMC_OnTileLoadStart;
         OnTileLoadComplete += eventSMC_OnTileLoadComplete;

         OnMarkerClick += eventSMC_OnMarkerClick;
         OnMarkerEnter += eventSMC_OnMarkerEnter;
         OnMarkerLeave += eventSMC_OnMarkerLeave;

         OnRouteClick += eventSMC_OnRouteClick;
         OnRouteEnter += eventSMC_OnRouteEnter;
         OnRouteLeave += eventSMC_OnRouteLeave;

#if !GMAP4SKIA
         Bearing = 0F;
         GrayScaleMode = false;
         HelperLineOption = HelperLineOptions.DontShow;
         MouseWheelZoomEnabled = true;
         MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
         NegativeMode = false;
#endif

         CanDragMap = true;
         LevelsKeepInMemory = 5;
         MarkersEnabled = true;
         PolygonsEnabled = true;
         RetryLoadTile = 0;
         RoutesEnabled = true;
         ScaleMode = ScaleModes.Fractional;
         SelectedAreaFillColor = System.Drawing.Color.FromArgb(33, 65, 105, 225);
         MapScaleInfoEnabled = false;

         SMC_MapRenderZoom2RealDevice = 1F;

         eventSMC_OnLoad();
      }

      /// <summary>
      /// Umrechnung der Client-Koordinaten in geogr. Koordinaten
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public PointLatLng SMC_FromLocalToLatLng(int x, int y) {
         return FromLocalToLatLng(x, y);
      }

      /// <summary>
      /// Umrechnung der geogr. Koordinaten in Client-Koordinaten
      /// </summary>
      /// <param name="ptgeo"></param>
      /// <returns></returns>
      public GPoint SMC_FromLatLngToLocal(PointLatLng ptgeo) {
         return FromLatLngToLocal(ptgeo);
      }

      /// <summary>
      /// sorgt dafür, dass der gewünschte Bereich auf jeden Fall engezeigt wird
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool SMC_SetZoomToFitRect(RectLatLng rect) {
         return SetZoomToFitRect(rect);
      }

#if GMAP4SKIA

      // Skia-Zusatz
      System.Windows.Forms.Cursor Cursor = null;
      System.Windows.Forms.MouseButtons DragButton = System.Windows.Forms.MouseButtons.Right;
      Color EmptyMapBackground;


      /// <summary>
      /// liefert die akt. angezeigte Karte als Bild
      /// </summary>
      /// <returns></returns>
      public Bitmap SMC_ToImage() {
         return ToImage();
      }
#else
      /// <summary>
      /// liefert die akt. angezeigte Karte als Bild
      /// </summary>
      /// <returns></returns>
      public Image SMC_ToImage() {
         return ToImage();
      }
#endif

      public void SMC_Refresh() {
         Refresh();
      }

      public void SMC_ReloadMap() {
         ReloadMap();
      }
   }
}
