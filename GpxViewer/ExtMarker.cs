using System;
using System.Drawing;
using GMap.NET.Ext.WindowsForms.ToolTips;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public class ExtMarker : GMap.NET.WindowsForms.Markers.GMarkerGoogle {

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
      /// Zuordnung von Bitmap und Bitmap-Offset für die geograf. Pos. für einen Marker
      /// </summary>
      public class MarkerPicture {

         public Bitmap Picture { get; private set; }

         public Point Offset { get; private set; }

         public MarkerPicture(int offsetx, int offsety, Bitmap bm) {
            Picture = bm;
            Offset = new Point(offsetx, offsety);
         }

      }

      public readonly static MarkerPicture FotoMarker = new MarkerPicture(-12, -12, Properties.Resources.Foto);
      public readonly static MarkerPicture FlagBlueMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagBlue);
      //readonly static MarkerPicture FlagRedMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagRed);
      public readonly static MarkerPicture FlagGreenMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagGreen);
      //readonly static MarkerPicture PinBlueMarker = new MarkerPicture(0, -24, Properties.Resources.PinBlue);
      //readonly static MarkerPicture PinRedMarker = new MarkerPicture(0, -24, Properties.Resources.PinRed);
      //readonly static MarkerPicture PinGreenMarker = new MarkerPicture(0, -24, Properties.Resources.PinGreen);
      public readonly static MarkerPicture PointMarker = new MarkerPicture(-2, -2, Properties.Resources.Point1);


      static ExtMarker() { }

      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="bm"></param>
      public ExtMarker(GMap.NET.PointLatLng pt, Bitmap bm) :
         base(pt, bm ?? FlagBlueMarker.Picture) {
         Waypoint = new Gpx.GpxWaypoint(pt.Lng, pt.Lat);
      }

      /// <summary>
      /// ohne <see cref="GpxDataContainer"/> (sonst <see cref="Create"/>() verwenden)
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="bm"></param>
      public ExtMarker(Gpx.GpxWaypoint pt, Bitmap bm) :
         base(new GMap.NET.PointLatLng(pt.Lat, pt.Lon), bm ?? FlagBlueMarker.Picture) {
         Waypoint = new Gpx.GpxWaypoint(pt);
      }

      /// <summary>
      /// erzeugt einen <see cref="ExtMarker"/> für den schon in <see cref="PoorGpxAllExt"/> vorhandenen <see cref="Gpx.GpxWaypoint"/> mit dem angegebenen Index 
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="ptidx"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      static public ExtMarker Create(PoorGpxAllExt gpx, int ptidx, MarkerType markertype) {
         MarkerPicture mp = initBitmap(markertype, gpx.GpxFileEditable);
         ExtMarker marker = new ExtMarker(new GMap.NET.PointLatLng(0, 0), mp.Picture) {
            GpxDataContainer = gpx,
            Markertype = markertype,
         };
         marker.init(ptidx);
         return marker;
      }

      /// <summary>
      /// erzeugt einen <see cref="ExtMarker"/> für den schon in <see cref="PoorGpxAllExt"/> vorhandenen <see cref="Gpx.GpxWaypoint"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="wp"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      static public ExtMarker Create(PoorGpxAllExt gpx, Gpx.GpxWaypoint wp, MarkerType markertype) {
         MarkerPicture mp = initBitmap(markertype, gpx.GpxFileEditable);
         ExtMarker marker = new ExtMarker(new GMap.NET.PointLatLng(0, 0), mp.Picture) {
            GpxDataContainer = gpx,
            Markertype = markertype,
         };

         int idx;
         switch (marker.Markertype) {
            case MarkerType.Foto:
               idx = marker.GpxDataContainer.PicturePoints.IndexOf(wp);
               if (idx >= 0)
                  marker.init(idx);
               else
                  throw new Exception("Waypoint not found");
               break;

            case MarkerType.Standard:
               idx = marker.GpxDataContainer.Waypoints.IndexOf(wp);
               if (idx >= 0)
                  marker.init(idx);
               else
                  throw new Exception("Waypoint not found");
               break;

            default:
               throw new Exception("Unknown MarkerType");
         }
         return marker;
      }

      static MarkerPicture initBitmap(MarkerType mtype, bool editable) {
         switch (mtype) {
            case MarkerType.Foto:
               return FotoMarker;

            case MarkerType.Standard:
               return editable ? FlagGreenMarker : FlagBlueMarker;

            default:
               throw new Exception("Unknown MarkerType");
         }
      }

      /// <summary>
      /// grundlegende Initialisierung; wenn der Index kleiner ist, muss <see cref="Waypoint"/> schon gesetzt sein
      /// </summary>
      /// <param name="wpidx"></param>
      void init(int wpidx) {
         MarkerPicture mp;
         switch (Markertype) {
            case MarkerType.Foto:
               if (wpidx >= 0)
                  Waypoint = GpxDataContainer.PicturePoints[wpidx];
               mp = FotoMarker;
               break;

            case MarkerType.Standard:
               if (wpidx >= 0)
                  Waypoint = GpxDataContainer.Waypoints[wpidx];
               mp = IsEditable ? FlagGreenMarker : FlagBlueMarker;
               break;

            default:
               throw new Exception("Unknown MarkerType");
         }
         Position = new GMap.NET.PointLatLng(Waypoint.Lat, Waypoint.Lon);

         //Offset = new Point(-bmStdMarker.Width / 2, -bmStdMarker.Height / 2),    // Mittelpunkt des Bitmap
         Offset = mp.Offset; // new Point(0, -bmStdMarker.Height),     // Ecke links unten des Bitmap
                             //ToolTipText = txt;
                             //ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.OnMouseOver;
         IsHitTestVisible = true; // true für Enter/Leave Events
         IsVisible = true;

         if (Markertype != MarkerType.Foto) {    // bei Fotos KEIN Tooltip anzeigen
            ToolTip = new GMapMarkerToolTip(this);
            ToolTipText = Waypoint.Name;
            //ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.OnMouseOver;
            ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.Always;
         }
      }

      /// <summary>
      /// liefert den akt. Index des <see cref="Waypoint"/> in <see cref="PoorGpxAllExt"/>
      /// </summary>
      /// <returns></returns>
      public int GetGpxIndex() {
         if (GpxDataContainer != null)
            switch (Markertype) {
               case MarkerType.Foto:
                  return GpxDataContainer.PicturePoints.IndexOf(Waypoint);

               case MarkerType.Standard:
                  return GpxDataContainer.Waypoints.IndexOf(Waypoint);

               default:
                  throw new Exception("Unknown MarkerType");
            }
         return -1;
      }

      /// <summary>
      /// ändert nur die Position
      /// </summary>
      /// <param name="pt"></param>
      public void ChangePos(Gpx.GpxWaypoint pt) {
         if (pt.Lat != Waypoint.Lat ||
             pt.Lon != Waypoint.Lon) {
            if (GpxDataContainer != null)
               GpxDataContainer.GpxFileChanged = true;
            if (Waypoint != null) {
               Waypoint.Lat = pt.Lat;
               Waypoint.Lon = pt.Lon;
            }
            Position = new GMap.NET.PointLatLng(pt.Lat, pt.Lon);
         }
      }

      /// <summary>
      /// liefert den <see cref="ExtMarker"/> zum <see cref="Gpx.GpxWaypoint"/>, falls dieser auf der Karte angezeigt wird
      /// </summary>
      /// <param name="mapControl"></param>
      /// <param name="wp"></param>
      /// <returns></returns>
      static public ExtMarker GetVisibleMarker4Waypoint(MapControl mapControl, Gpx.GpxWaypoint wp) {
         foreach (var item in mapControl.MapGetActualExtMarkers(false)) {
            if (item.Waypoint.Equals(wp))
               return item;
         }
         return null;
      }

      public override string ToString() {
         return string.Format("[Markertype={0}, Text={1}, Waypoint={2}, Gpx={3}]", Markertype, ToolTipText, Waypoint, GpxDataContainer);
      }

   }
}
