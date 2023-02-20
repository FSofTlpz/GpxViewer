using System;
using System.Drawing;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl.EditHelper {
   /// <summary>
   /// Hilfsfunktionen für das Editieren
   /// </summary>
   public class EditHelper {

      public delegate void RefreshProgramStateEventHandler(object sender, EventArgs e);
      public event RefreshProgramStateEventHandler RefreshProgramStateEvent;


      protected SpecialMapCtrl mapControl;
      protected GpxAllExt editablegpx;

      /// <summary>
      /// Pen für Hilfslinnien beim Editieren
      /// </summary>
      Pen penHelper;

      public bool IsNew { get; protected set; }


      public EditHelper(SpecialMapCtrl mapControl,
                        GpxAllExt editableGpx,
                        Color helperPenColor,
                        float helperPenWidth) {
         this.mapControl = mapControl;
         editablegpx = editableGpx;
         penHelper = new Pen(helperPenColor) {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            Width = helperPenWidth,
         };
      }

      /// <summary>
      /// Anzeige akt.
      /// </summary>
      protected void Refresh() => mapControl.Map_Refresh();

      /// <summary>
      /// der Programmstatus wird aktualisiert
      /// <para>
      /// sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
      /// </para>
      /// </summary>
      protected void RefreshProgramState() => RefreshProgramStateEvent?.Invoke(this, new EventArgs());

      /// <summary>
      /// liefert die Geodaten für den Clientpunkt
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <param name="lon">geografische Höhe</param>
      /// <param name="lat">geografische Länge</param>
      /// <returns>Höhe</returns>
      double getGeoDat4ClientPoint(Point ptclient, DemData dem, out double lon, out double lat) {
         PointD ptgeo = GetPointD4ClientPoint(ptclient);
         double h = dem != null ? dem.GetHeight(ptgeo.X, ptgeo.Y) : DEM1x1.DEMNOVALUE;
         if (h == DEM1x1.DEMNOVALUE)
            h = Gpx.BaseElement.NOTVALID_DOUBLE;
         lon = ptgeo.X;
         lat = ptgeo.Y;
         return h;
      }

      /// <summary>
      /// liefert die Geodaten für den Clientpunkt
      /// </summary>
      /// <param name="ptclient"></param>
      /// <returns></returns>
      protected PointD GetPointD4ClientPoint(Point ptclient) => mapControl.SpecMapClient2LonLat(ptclient);

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxWaypoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      protected Gpx.GpxWaypoint GetGpxWaypoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxWaypoint(lon, lat, ele);
      }

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxTrackPoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      protected Gpx.GpxTrackPoint GetGpxTrackPoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxTrackPoint(lon, lat, ele);
      }

      /// <summary>
      /// liefert die Höhe zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public double GetHeight(Point ptclient, DemData dem) => getGeoDat4ClientPoint(ptclient, dem, out _, out _);

      /// <summary>
      /// zeichnet die Hilfslinie
      /// </summary>
      /// <param name="g"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      protected void DrawHelperLine(Graphics g, Point from, Point to) => g.DrawLine(penHelper, from, to);

   }

}
