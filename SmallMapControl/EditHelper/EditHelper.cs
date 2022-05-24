using System;
using System.Drawing;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl.EditHelper {
   /// <summary>
   /// Hilfsfunktionen für das Editieren
   /// </summary>
   public class EditHelper {

      public delegate void RefreshProgramStateEventHandler(object sender, EventArgs e);
      public event RefreshProgramStateEventHandler RefreshProgramStateEvent;


      protected SmallMapCtrl mapControl;
      protected PoorGpxAllExt editablegpx;


      /// <summary>
      /// Pen zum Zeichnen von Linien zum Cursor beim Editieren
      /// </summary>
      protected Pen pen4Moving = new Pen(VisualTrack.InEditableColor) {
         DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
         Width = VisualTrack.InEditableWidth,
      };


      public EditHelper(SmallMapCtrl mapControl,
                        PoorGpxAllExt editableGpx) {
         this.mapControl = mapControl;
         editablegpx = editableGpx;
      }

      /// <summary>
      /// Anzeige akt.
      /// </summary>
      public void Refresh() {
         mapControl.Refresh();
      }

      /// <summary>
      /// der Programmstatus wird aktualisiert
      /// <para>
      /// sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
      /// </para>
      /// </summary>
      protected void RefreshProgramState() {
         RefreshProgramStateEvent?.Invoke(this, new EventArgs());
      }


      /// <summary>
      /// liefert die Geodaten für den Clientpunkt
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      double getGeoDat4ClientPoint(Point ptclient, DemData dem, out double lon, out double lat) {
         PointD ptgeo = GetPointD4ClientPoint(ptclient);
         double h = dem != null ? dem.GetHeight(ptgeo.X, ptgeo.Y) : DEM1x1.DEMNOVALUE;
         if (h == DEM1x1.DEMNOVALUE)
            h = Gpx.BaseElement.NOTVALID_DOUBLE;
         lon = ptgeo.X;
         lat = ptgeo.Y;
         return h;
      }

      protected PointD GetPointD4ClientPoint(Point ptclient) {
         return mapControl.MapClient2LonLat(ptclient);
      }

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxWaypoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public Gpx.GpxWaypoint GetGpxWaypoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxWaypoint(lon, lat, ele);
      }

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxTrackPoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public Gpx.GpxTrackPoint GetGpxTrackPoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxTrackPoint(lon, lat, ele);
      }

      /// <summary>
      /// liefert die Höhe zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public double GetHeight(Point ptclient, DemData dem) {
         return getGeoDat4ClientPoint(ptclient, dem, out _, out _);
      }

   }

}
