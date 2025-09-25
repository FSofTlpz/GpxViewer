using System;
using System.Drawing;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl {
   public class Marker {

      /// <summary>
      /// Container alle GPX-Daten zu dem der <see cref="ExtMarker"/> gehört
      /// </summary>
      public GpxData? GpxDataContainer { get; protected set; } = null;

      /// <summary>
      /// Gpx-Waypoint
      /// </summary>
      public Gpx.GpxWaypoint Waypoint { get; protected set; }

      /// <summary>
      /// liefert den akt. Index des <see cref="Waypoint"/> in <see cref="GpxData"/> oder -1
      /// </summary>
      /// <returns></returns>
      public int GpxDataContainerIndex {
         get {
            if (GpxDataContainer != null)
               switch (Markertype) {
                  case MarkerType.Foto:
                     return GpxDataContainer.MarkerListPictures.IndexOf(this);

                  case MarkerType.Standard:
                  case MarkerType.EditableStandard:
                     return GpxDataContainer.MarkerList.IndexOf(this);

                  default:
                     throw new Exception("Unknown MarkerType");
               }
            return -1;
         }
      }

      /// <summary>
      /// Typ des Markers
      /// </summary>
      public enum MarkerType {
         /// <summary>
         /// Standard-Marker
         /// </summary>
         Standard,
         /// <summary>
         /// Standard-Marker, editierbar
         /// </summary>
         EditableStandard,
         /// <summary>
         /// Marker für ein Foto
         /// </summary>
         Foto,
         /// <summary>
         /// Marker für Geotagging
         /// </summary>
         GeoTagging,
      }

      /// <summary>
      /// der Typ des Markers
      /// </summary>
      public MarkerType Markertype { get; private set; } = MarkerType.Standard;

      /// <summary>
      /// Ist der Marker editierbar (wird vom <see cref="GpxDataContainer"/> vorgegeben)?
      /// </summary>
      public bool IsEditable => GpxDataContainer == null || GpxDataContainer.GpxFileEditable;

      /// <summary>
      /// Wird <see cref="Track"/> angezeigt?
      /// </summary>
      public bool IsVisible {
         get => VisualMarker != null && VisualMarker.IsVisible;
         set {
            if (VisualMarker != null)
               VisualMarker.IsVisible = value;
            else if (value == true) {
               UpdateVisualMarker(); // keine Anzeige
               if (VisualMarker != null)
                  VisualMarker.IsVisible = value;
            }
         }
      }

      /// <summary>
      /// Name des <see cref="Marker"/>
      /// </summary>
      public string Text {
         get => Waypoint.Name;
         set {
            Waypoint.Name = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0 && GpxDataContainer != null)
               GpxDataContainer.Waypoints[idx].Name = value;
            if (VisualMarker != null)
               VisualMarker.ToolTipText = value;
         }
      }

      /// <summary>
      /// geografische Breite des <see cref="Marker"/>
      /// </summary>
      public double Longitude {
         get => Waypoint.Lon;
         set {
            Waypoint.Lon = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0 && GpxDataContainer != null)
               GpxDataContainer.Waypoints[idx].Lon = value;
         }
      }

      /// <summary>
      /// geografische Länge des <see cref="Marker"/>
      /// </summary>
      public double Latitude {
         get => Waypoint.Lat;
         set {
            Waypoint.Lat = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0 && GpxDataContainer != null)
               GpxDataContainer.Waypoints[idx].Lat = value;
         }
      }

      /// <summary>
      /// Höhe des <see cref="Marker"/>
      /// </summary>
      public double Elevation {
         get => Waypoint.Elevation;
         set {
            Waypoint.Elevation = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0 && GpxDataContainer != null)
               GpxDataContainer.Waypoints[idx].Elevation = value;
         }
      }

      /// <summary>
      /// Symbolname (wird z.Z. nur für editierbare Marker verwendet)
      /// </summary>
      public string Symbolname {
         get => Waypoint.Symbol;
         set {
            Waypoint.Symbol = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0 && GpxDataContainer != null)
               GpxDataContainer.Waypoints[idx].Symbol = value;
         }
      }

      public Bitmap? Bitmap => VisualMarker.Bitmap4Style(getVisualStyle(), Symbolname, Symbolzoom);

      /// <summary>
      /// nur zum Anzeigen des Markers nötig
      /// </summary>
      public VisualMarker? VisualMarker { get; protected set; }

      double _symbolzoom = 1;
      public double Symbolzoom {
         get => _symbolzoom;
         set => _symbolzoom = value;      // hier sollte auch der VisualMarker korrigiert werden
      }


      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="wpt"></param>
      /// <param name="markertype"></param>
      /// <param name="symbolname"></param>
      /// <param name="symbolzoom"></param>
      public Marker(Gpx.GpxWaypoint? wpt, MarkerType markertype, string? symbolname, double symbolzoom = 1) {
         if (wpt == null)
            throw new ArgumentException("null-Argument", nameof(wpt));
         Waypoint = new Gpx.GpxWaypoint(wpt);
         Markertype = markertype;
         Symbolname = symbolname != null ? symbolname : string.Empty;
         Symbolzoom = symbolzoom;
      }

      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="m"></param>
      public Marker(Marker m) :
         this(m.Waypoint, m.Markertype, m.Symbolname, m.Symbolzoom) { }


      /// <summary>
      /// erzeugt einen <see cref="Marker"/> für den schon in <see cref="GpxData"/> vorhandenen <see cref="Gpx.GpxWaypoint"/> mit dem angegebenen Index 
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="wpidx"></param>
      /// <param name="markertype"></param>
      /// <param name="symbolzoom"></param>
      /// <returns></returns>
      static public Marker Create(GpxData gpx,
                                  int wpidx,
                                  MarkerType markertype,
                                  double symbolzoom = 1) {
         Gpx.GpxWaypoint wp = gpx.Waypoints[wpidx];
         return Create(gpx, wp, markertype, symbolzoom);
      }

      /// <summary>
      /// erzeugt einen <see cref="Marker"/> für den schon in <see cref="GpxData"/> vorhandenen <see cref="Gpx.GpxWaypoint"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="wp"></param>
      /// <param name="markertype"></param>
      /// <param name="symbolzoom"></param>
      /// <returns></returns>
      static public Marker Create(GpxData gpx,
                                  Gpx.GpxWaypoint wp,
                                  MarkerType markertype,
                                  double symbolzoom = 1) {
         Marker marker = new Marker(wp, markertype, wp.Symbol, symbolzoom) {
            GpxDataContainer = gpx,
         };
         return marker;
      }

      /// <summary>
      /// liefert den <see cref="VisualMarker.VisualStyle"/>
      /// </summary>
      /// <returns></returns>
      VisualMarker.VisualStyle getVisualStyle() {
         switch (Markertype) {
            case MarkerType.Standard:
               return VisualMarker.VisualStyle.StandardMarker;

            case MarkerType.EditableStandard:
               return VisualMarker.VisualStyle.StandardEditableMarker;

            case MarkerType.Foto:
               return VisualMarker.VisualStyle.FotoMarker;

            case MarkerType.GeoTagging:
               return VisualMarker.VisualStyle.GeoTagging;
         }
         return VisualMarker.VisualStyle.PointMarker;
      }

      /// <summary>
      /// <see cref="VisualMarker"/> entsprechend der akt. Daten (neu) erzeugen
      /// </summary>
      /// <param name="mapControl">wenn ungleich null, dann auch anzeigen</param>
      public void UpdateVisualMarker(SpecialMapCtrl? mapControl = null) {
         bool visible = IsVisible;

         if (mapControl != null)
            mapControl.M_ShowMarker(this, false);

         VisualMarker = new VisualMarker(this,
                                         string.Empty,
                                         getVisualStyle(),
                                         Symbolname,
                                         Symbolzoom);
         if (!visible)
            VisualMarker.IsVisible = false;

         if (mapControl != null &&
             visible) {  // dann auch neu anzeigen
            mapControl.M_ShowMarker(this,
                                         true,
                                         IsEditable && GpxDataContainer != null ?
                                                 GpxDataContainer.NextVisibleMarker(this) :
                                                 null);
         }
      }

      /// <summary>
      /// Anzeige aktualisieren (falls akt. sichtbar)
      /// </summary>
      public void Refresh() {
         if (IsVisible)
            VisualMarker?.Refresh();
      }

      public override string ToString() {
         return string.Format("[Markertype={0}, Text={1}, Waypoint={2}, Gpx={3}]", Markertype, Text, Waypoint, GpxDataContainer);
      }

   }
}
