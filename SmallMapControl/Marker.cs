using System;
using System.Drawing;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl {
   public class Marker {

      /// <summary>
      /// Gpx-Waypoint
      /// </summary>
      public Gpx.GpxWaypoint Waypoint { get; protected set; } = null;

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
         Foto
      }

      /// <summary>
      /// der Typ des Markers
      /// </summary>
      public MarkerType Markertype { get; private set; } = MarkerType.Standard;

      /// <summary>
      /// Container alle GPX-Daten zu dem der <see cref="ExtMarker"/> gehört
      /// </summary>
      public PoorGpxAllExt GpxDataContainer { get; protected set; } = null;

      /// <summary>
      /// Ist der Marker editierbar (wird vom <see cref="GpxDataContainer"/> vorgegeben)?
      /// </summary>
      public bool IsEditable {
         get {
            return GpxDataContainer == null || GpxDataContainer.GpxFileEditable;
         }
      }

      /// <summary>
      /// Wird <see cref="Track"/> angezeigt?
      /// </summary>
      public bool IsVisible {
         get {
            return VisualMarker != null && VisualMarker.IsVisible;
         }
         set {
            if (VisualMarker != null)
               VisualMarker.IsVisible = value;
            else if (value == true) {
               UpdateVisualMarker(); // keine Anzeige
               VisualMarker.IsVisible = value;
            }
         }
      }

      /// <summary>
      /// Name des Markers
      /// </summary>
      public string Text {
         get {
            return Waypoint.Name;
         }
         set {
            Waypoint.Name = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0)
               GpxDataContainer.Waypoints[idx].Name = value;
         }
      }

      public double Longitude {
         get {
            return Waypoint.Lon;
         }
         set {
            Waypoint.Lon = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0)
               GpxDataContainer.Waypoints[idx].Lon = value;
         }
      }

      public double Latitude {
         get {
            return Waypoint.Lat;
         }
         set {
            Waypoint.Lat = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0)
               GpxDataContainer.Waypoints[idx].Lat = value;
         }
      }

      public double Elevation {
         get {
            return Waypoint.Elevation;
         }
         set {
            Waypoint.Elevation = value;
            int idx = GpxDataContainerIndex;
            if (idx >= 0)
               GpxDataContainer.Waypoints[idx].Elevation = value;
         }
      }

      /// <summary>
      /// nur zum Anzeigen des Markers nötig
      /// </summary>
      public VisualMarker VisualMarker { get; protected set; }

      /// <summary>
      /// Symbolname (wird z.Z. nur für editierbare Marker verwendet)
      /// </summary>
      public string Symbolname {
         get {
            return Waypoint.Symbol;
         }
         set {
            Waypoint.Symbol = value;
         }
      }



      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="name"></param>
      /// <param name="markertype"></param>
      //public Marker(double lon = 0, double lat = 0, string name = "", MarkerType markertype = MarkerType.Standard) {
      //   Waypoint = new Gpx.GpxWaypoint(lon, lat);
      //   Waypoint.Name = name;
      //   Markertype = markertype;
      //}

      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="wpt"></param>
      /// <param name="markertype"></param>
      /// <param name="symbolname"></param>
      public Marker(Gpx.GpxWaypoint wpt, MarkerType markertype, string symbolname) {
         Waypoint = new Gpx.GpxWaypoint(wpt);
         Markertype = markertype;
         Symbolname = symbolname;
      }

      /// <summary>
      /// erzeugt einen <see cref="Marker"/> für den schon in <see cref="PoorGpxAllExt"/> vorhandenen <see cref="Gpx.GpxWaypoint"/> mit dem angegebenen Index 
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="wpidx"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      static public Marker Create(PoorGpxAllExt gpx,
                                  int wpidx,
                                  MarkerType markertype) {
         Marker marker = new Marker(gpx.Waypoints[wpidx], markertype, gpx.Waypoints[wpidx].Symbol) {
            GpxDataContainer = gpx,
         };
         return marker;
      }

      /// <summary>
      /// erzeugt einen <see cref="Marker"/> für den schon in <see cref="PoorGpxAllExt"/> vorhandenen <see cref="Gpx.GpxWaypoint"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="wp"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      static public Marker Create(PoorGpxAllExt gpx,
                                  Gpx.GpxWaypoint wp,
                                  MarkerType markertype) {
         Marker marker = new Marker(wp, markertype, wp.Symbol) {
            GpxDataContainer = gpx,
         };
         return marker;
      }

      /// <summary>
      /// liefert den akt. Index des <see cref="Waypoint"/> in <see cref="PoorGpxAllExt"/>
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
      /// liefert den <see cref="VisualTrack.VisualStyle"/>
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
         }
         return VisualMarker.VisualStyle.PointMarker;
      }

      /// <summary>
      /// <see cref="VisualTrack"/> entsprechend der akt. Daten (neu) erzeugen
      /// </summary>
      /// <param name="mapControl">wenn ungleich null, dann auch anzeigen</param>
      public void UpdateVisualMarker(SmallMapCtrl mapControl = null) {
         bool visible = IsVisible;

         if (mapControl != null)
            mapControl.MapShowMarker(this, false);

         VisualMarker = new VisualMarker(this,
                                         "",
                                         getVisualStyle(),
                                         Symbolname);
         if (mapControl != null &&
             visible) {  // dann auch neu anzeigen
            mapControl.MapShowMarker(this,
                                     true,
                                     IsEditable && GpxDataContainer != null ?
                                             GpxDataContainer.NextVisibleEditableMarker(this) :
                                             null);
         }
      }

      public Bitmap Bitmap {
         get {
            return VisualMarker.Bitmap4Style(getVisualStyle(), Symbolname);
         }
      }


      public override string ToString() {
         return string.Format("[Markertype={0}, Text={1}, Waypoint={2}, Gpx={3}]", Markertype, Text, Waypoint, GpxDataContainer);
      }

   }
}
