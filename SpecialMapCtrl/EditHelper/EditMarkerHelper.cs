using System;
using System.Drawing;
using FSofTUtils.Geography.DEM;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl.EditHelper {
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

      /// <summary>
      /// ein neuer Marker sollte eingefügt werden
      /// </summary>
      public event EventHandler<MarkerEventArgs> MarkerShouldInsertEvent;


      /// <summary>
      /// akt. zu verschiebender Marker
      /// </summary>
      Marker markerInEdit;

      /// <summary>
      /// Kopie des Markers
      /// </summary>
      Marker markerCopy;

      /// <summary>
      /// Bearbeitung "in Arbeit" (kann nur eine Verschiebung sein)
      /// </summary>
      public bool InWork => markerInEdit != null;


      public EditMarkerHelper(SpecialMapCtrl mapControl,
                              GpxAllExt editableGpx,
                              Color movingPenColor,
                              float movingPenWidth) :
         base(mapControl, editableGpx, movingPenColor, movingPenWidth) { }

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

      #region Marker editieren

      /// <summary>
      /// Start für Marker verschieben oder neuen einfügen (marker == null)
      /// </summary>
      public void EditStart(Marker marker = null) {
         markerInEdit = marker;
         IsNew = marker == null;
         markerCopy = marker != null ?
                           new Marker(marker) :
                           null;
      }

      /// <summary>
      /// Erweiterung zu Paint() (wenn <see cref="InWork"/>==true); Hilfslinie anzeigen
      /// </summary>
      public void EditDrawDestinationLine(Graphics canvas, Point ptLastMouseLocation) {
         if (InWork)
            DrawHelperLine(canvas, mapControl.SpecMapLonLat2Client(markerInEdit.Waypoint), ptLastMouseLocation);
      }

      /// <summary>
      /// setzt die (neue) Position (implizit erfolgt auch <see cref="EditEnd"/>)
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
            editablegpx.GpxDataChanged = true;  // muss explizit gesetzt werden, weill die Eigenschaften eines vorhandenen Objekts geändert werden
            RefreshOnMap(markerInEdit);
         } else
            MarkerShouldInsertEvent?.Invoke(this, new MarkerEventArgs(new Marker(wp, Marker.MarkerType.EditableStandard, null)));  // neuer Marker
         EditEnd();
      }

      public void EditEnd(bool cancel = false) {
         if (markerInEdit != null && cancel) {
            markerInEdit.Longitude = markerCopy.Longitude;
            markerInEdit.Latitude = markerCopy.Latitude;
            markerInEdit.Elevation = markerCopy.Elevation;
         }
         markerInEdit = null;
      }

      #endregion

      /// <summary>
      /// neu anzeigen (weil sich die Daten geändert haben)
      /// </summary>
      /// <param name="marker"></param>
      public void RefreshOnMap(Marker marker) => marker.UpdateVisualMarker(mapControl);

      /// <summary>
      /// fügt eine Kopie des <see cref="Marker"/> in den Container ein
      /// </summary>
      /// <param name="orgmarker"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Marker InsertCopy(Marker orgmarker, int pos = -1) => 
         editablegpx.MarkerInsertCopy(orgmarker, pos, Marker.MarkerType.EditableStandard);

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus dem Container
      /// </summary>
      /// <param name="marker"></param>
      public void Remove(Marker marker) {
         showMarker(marker, false);          // Sichtbarkeit ausschalten
         editablegpx.MarkerRemove(marker);
      }

      /// <summary>
      /// entfernt alle <see cref="Marker"/> aus dem Container
      /// </summary>
      public void RemoveAll() {
         while (editablegpx.MarkerList.Count > 0)
            Remove(editablegpx.MarkerList[0]);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> im Container
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
      /// ändert die Sichtbarkeit des <see cref="Marker"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      void showMarker(Marker marker, bool visible) =>
         mapControl.SpecMapShowMarker(marker,
                                      visible,
                                      visible ?
                                         editablegpx.NextVisibleMarker(marker) :
                                         null);

      public override string ToString() {
         return string.Format("MoveIsActiv={0}{1}",
                              InWork,
                              InWork ?
                                 ", " + markerInEdit.ToString() :
                                 ""); ;
      }
   }
}
