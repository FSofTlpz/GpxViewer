using System.Drawing;
using FSofTUtils.Geography.DEM;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl.EditHelper {
   /// <summary>
   /// Hilfsfunktionen für das Editieren der <see cref="Marker"/>
   /// </summary>
   public class EditMarkerHelper : EditHelper {

      public class MarkerEventArgs {

         public Marker Marker;

         public MarkerEventArgs(Marker marker) {
            Marker = marker;
         }

      }

      public delegate void MarkerShouldInsertEventHandler(object sender, MarkerEventArgs e);
      /// <summary>
      /// ein neuer Marker sollte eingefügt werden
      /// </summary>
      public event MarkerShouldInsertEventHandler MarkerShouldInsertEvent;


      /// <summary>
      /// akt. zu verschiebender Marker
      /// </summary>
      Marker markerInEdit;

      /// <summary>
      /// Bearbeitung "in Arbeit" (kann nur eine Verschiebung sein)
      /// </summary>
      public bool InWork {
         get {
            return markerInEdit != null;
         }
      }


      public EditMarkerHelper(SmallMapCtrl mapControl,
                              PoorGpxAllExt editableGpx) :
         base(mapControl, editableGpx) { }

      /// <summary>
      /// Cursor akt. (nur als Reaktion auf OnMarkerLeave nötig)
      /// </summary>
      public void RefreshCursor() {
         if (InWork) {
            Marker tmp = markerInEdit;
            RefreshProgramState();  // sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
            markerInEdit = tmp;
         }
      }

      /// <summary>
      /// Anzeige akt.
      /// </summary>
      public new void Refresh() {
         if (InWork)
            base.Refresh();
      }


      /// <summary>
      /// Start für Marker verschieben (marker ungleich null) oder neuen einfügen
      /// </summary>
      public void EditStart(Marker marker = null) {
         markerInEdit = marker;
      }

      /// <summary>
      /// Erweiterung zu Paint()
      /// </summary>
      public void EditDrawDestinationLine(Graphics canvas, Point ptLastMouseLocation) {
         if (InWork) {
            Point pt = mapControl.MapLonLat2Client(markerInEdit.Waypoint);
            canvas.DrawLine(pen4Moving, pt.X, pt.Y, ptLastMouseLocation.X, ptLastMouseLocation.Y);
         }
      }

      /// <summary>
      /// setzt die (neue) Position (implizit erfolgt auch <see cref="EditEnd"/>())
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      public void EditSetNewPos(Point ptclient, DemData dem) {
         Gpx.GpxWaypoint wp = GetGpxWaypoint(ptclient, dem);
         if (markerInEdit != null) {
            // (nur) Pos. und Höhe neu setzen
            markerInEdit.Longitude = wp.Lon;
            markerInEdit.Latitude = wp.Lat;
            markerInEdit.Elevation = wp.Elevation;
            RefreshOnMap(markerInEdit);
         } else
            MarkerShouldInsertEvent?.Invoke(this, new MarkerEventArgs(new Marker(wp, Marker.MarkerType.EditableStandard, null)));  // neuer Marker
         EditEnd();
      }

      public void EditEnd() {
         markerInEdit = null;
      }

      /// <summary>
      /// neu anzeigen (weil sich die Daten geändert haben)
      /// </summary>
      /// <param name="marker"></param>
      public void RefreshOnMap(Marker marker) {
         marker.UpdateVisualMarker(mapControl);
      }

      /// <summary>
      /// fügt eine Kopie des <see cref="Marker"/> an den akt. Container und die ListBox an (oder ein)
      /// </summary>
      /// <param name="orgmarker"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Marker InsertCopy(Marker orgmarker, int pos = -1) {
         return editablegpx.MarkerInsertCopy(orgmarker, pos, Marker.MarkerType.EditableStandard);
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus der Auflistung
      /// </summary>
      /// <param name="marker"></param>
      public void Remove(Marker marker) {
         showMarker(marker, false);
         editablegpx.MarkerRemove(marker);
      }

      /// <summary>
      /// entfernt alle Marker aus der internen Liste, seine Gpx-Daten und aus der ListBox
      /// </summary>
      public void RemoveAll() {
         while (editablegpx.MarkerList.Count > 0)
            Remove(editablegpx.MarkerList[0]);
      }


      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> in der Auflistung
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void ChangeOrder(int fromidx, int toidx) {
         editablegpx.MarkerOrderChange(fromidx, toidx);

         Marker m = editablegpx.MarkerList[toidx];
         if (m.IsVisible)
            m.UpdateVisualMarker(mapControl);
      }

      /// <summary>
      /// ändert die Ansicht des <see cref="Marker"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      void showMarker(Marker marker, bool visible) {
         mapControl.MapShowMarker(marker,
                                  visible,
                                  visible ?
                                       editablegpx.NextVisibleEditableMarker(marker) :
                                       null);
      }


      public override string ToString() {
         return string.Format("MoveIsActiv={0}{1}",
                              InWork,
                              InWork ?
                                 ", " + markerInEdit.ToString() :
                                 ""); ;
      }
   }
}
