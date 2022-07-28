using System;
using System.Collections.Generic;
using System.Drawing;
using FSofTUtils.Geography.DEM;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl.EditHelper {

   /// <summary>
   /// Hilfsfunktionen für das Editieren der <see cref="Track"/>
   /// </summary>
   public class EditTrackHelper : EditHelper {

      public class TrackEventArgs {

         public Track Track;

         public TrackEventArgs(Track track) {
            Track = track;
         }

      }

      public delegate void TrackEditShowEventHandler(object sender, TrackEventArgs e);
      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event TrackEditShowEventHandler TrackEditShowEvent;


      /// <summary>
      /// akt. bearbeiteter Track
      /// </summary>
      public Track TrackInEdit { get; protected set; }

      bool trackchanged = false;

      /// <summary>
      /// eine Bearbeitung ist "in Arbeit"
      /// </summary>
      public bool InWork {
         get {
            return TrackInEdit != null;
         }
      }


      public EditTrackHelper(SmallMapCtrl mapControl,
                             PoorGpxAllExt editableGpx) :
         base(mapControl, editableGpx) { }


      /// <summary>
      /// liefert true wenn genau dieser Track gerade bearbeitet wird
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool TrackIsInWork(Track track) {
         return track != null &&
                TrackInEdit != null &&
                Equals(track, TrackInEdit);
      }


      /// <summary>
      /// Anzeige akt.
      /// </summary>
      public new void Refresh() {
         if (InWork &&
             TrackInEdit.GpxSegment.Points.Count > 0)
            base.Refresh();
      }

      Point convertTrackPoint2Point(Gpx.GpxTrackPoint pt) {
         return mapControl.MapLonLat2Client(pt);
      }

      /// <summary>
      /// Vorschau für neuen zusätzlichen Punkt
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptLastMouseLocation"></param>
      public void DrawDestinationLine(Graphics canvas, Point ptLastMouseLocation) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {
            drawLine(canvas,
                     convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                     ptLastMouseLocation);
         }
      }

      /// <summary>
      /// Vorschau auf den Punkt für die Trennung
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptLastMouseLocation"></param>
      public void DrawSplitPoint(Graphics canvas, Point ptLastMouseLocation) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {
            int ptidx = TrackInEdit.GetNearestPtIdx(GetPointD4ClientPoint(ptLastMouseLocation));
            drawLine(canvas,
                     convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[ptidx]),
                     ptLastMouseLocation);
         }
      }

      /// <summary>
      /// Vorschau für die Verbindung zweier Tracks
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="trackappend"></param>
      public void DrawConcatLine(Graphics canvas, Track trackappend) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0 &&
             trackappend != null &&
             trackappend.GpxSegment.Points.Count > 0)
            drawLine(canvas,
                     convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                     convertTrackPoint2Point(trackappend.GpxSegment.Points[0]));
      }

      void drawLine(Graphics canvas, Point pt1, Point pt2) {
         canvas.DrawLine(pen4Moving, pt1.X, pt1.Y, pt2.X, pt2.Y);
      }


      /// <summary>
      /// bestehenden <see cref="Track"/> bearbeiten
      /// </summary>
      /// <param name="track"></param>
      public void EditStart(Track track) {
         trackchanged = false;
         TrackInEdit = track;

         showTrackWithEvent(track, false);   // normale Darstellung ausschalten
         TrackInEdit.IsOnEdit = true;
         TrackInEdit.UpdateVisualTrack();
         showTrackWithEvent(TrackInEdit);    // "Edit"-Darstellung einschalten
      }

      /// <summary>
      /// neuen Track starten
      /// </summary>
      public void EditStartNew() {
         trackchanged = false;
      }

      /// <summary>
      /// neuen Punkt anhängen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      public void EditDraw_AppendPoint(Point ptclient, DemData dem) {
         if (TrackInEdit == null) { // 1. Punkt für neuen Track
            TrackInEdit = InsertCopy(new Track(new Gpx.GpxTrackPoint[0],
                                               "Track " + DateTime.Now.ToString(@"d.MM.yyyy, H:mm:ss")),
                                     0);
            TrackInEdit.IsOnEdit = true;
            showTrack(TrackInEdit, true);           // als editierbaren Track anzeigen
         }

         if (TrackInEdit != null) {
            showTrackWithEvent(TrackInEdit, false);                         // Anzeige des bisherigen Tracks löschen
            TrackInEdit.GpxSegment.Points.Add(GetGpxTrackPoint(ptclient, dem));  // neuen Punkt aufnehmen
            editablegpx.GpxFileChanged = true;
            TrackInEdit.UpdateVisualTrack(mapControl);
            showTrackWithEvent(TrackInEdit);                                // veränderten Track anzeigen
            trackchanged = true;
         }
      }

      /// <summary>
      /// letzten Punkt wieder entfernen
      /// </summary>
      public void EditDraw_RemoveLastPoint() {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 1) {
            showTrackWithEvent(TrackInEdit, false);                         // Anzeige des bisherigen Tracks löschen
            TrackInEdit.GpxSegment.Points.RemoveAt(TrackInEdit.GpxSegment.Points.Count - 1);
            editablegpx.GpxFileChanged = true;
            TrackInEdit.UpdateVisualTrack(mapControl);
            showTrackWithEvent(TrackInEdit);                                // veränderten Track anzeigen
            trackchanged = true;
         }
      }

      /// <summary>
      /// Abschluss des Track-Zeichnen
      /// </summary>
      public void EditEndDraw() {
         if (TrackInEdit != null) {
            TrackInEdit.IsOnEdit = false;
            showTrackWithEvent(TrackInEdit, false);          // Anzeige des bisherigen Tracks löschen
            if (trackchanged) {
               if (TrackInEdit.GpxSegment.Points.Count > 1) {     // Trackaufzeichnung beenden und Track im Container speichern
                  showTrackWithEvent(TrackInEdit);                       // wieder anzeigen
                  TrackInEdit.CalculateStats();
               } else {       // zu wenig Punkte
                  Remove(TrackInEdit);
               }
            } else
               showTrackWithEvent(TrackInEdit);                             // wieder anzeigen

         }
         TrackInEdit = null;
      }

      /// <summary>
      /// Abschluss des Track-Trennen
      /// </summary>
      /// <param name="ptLastMouseLocation"></param>
      public void EndSplit(Point ptLastMouseLocation) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {

            showTrackWithEvent(TrackInEdit, false);                              // Anzeige des bisherigen Tracks ausschalten
            TrackInEdit.IsOnEdit = false;

            if (ptLastMouseLocation != Point.Empty) {                   // Aktion abgebrochen
               int ptidx = TrackInEdit.GetNearestPtIdx(GetPointD4ClientPoint(ptLastMouseLocation));
               split(TrackInEdit, ptidx);
               TrackInEdit.IsMarked4Edit = false;
               TrackInEdit.UpdateVisualTrack();
               editablegpx.TrackList[editablegpx.TrackList.Count - 1].IsMarked4Edit = true;
            }

            showTrackWithEvent(TrackInEdit);     // wieder anzeigen
         }
         TrackInEdit = null;
      }

      /// <summary>
      /// splitted den vorhandenen Track (der erste Track wird nur gekürzt, der zweite an die Liste angehängt)
      /// </summary>
      /// <param name="track"></param>
      /// <param name="splitptidx"></param>
      /// <returns>true, wenn geteilt</returns>
      bool split(Track track, int splitptidx) {
         return editablegpx.TrackSplit(track, splitptidx);
      }

      /// <summary>
      /// Abschluss für die Verbindung von 2 Tracks
      /// </summary>
      /// <param name="track"></param>
      public void EndConcat(Track track) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0 &&
             (track == null || (track != null && track.IsEditable))) {
            TrackInEdit.IsOnEdit = false;
            showTrackWithEvent(TrackInEdit, false);     // Anzeige des bisherigen Tracks löschen

            if (track != null) { // sonst Aktion abgebrochen
               TrackInEdit.GpxSegment.Points.AddRange(track.GpxSegment.Points);
               TrackInEdit.CalculateStats();
               TrackInEdit.UpdateVisualTrack();

               showTrackWithEvent(track, false);
               Remove(track);
            }

            showTrackWithEvent(TrackInEdit);     // wieder anzeigen
         }
         TrackInEdit = null;
      }

      /// <summary>
      /// ändert die Ansicht des Tracks und informiert per Event darüber
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      void showTrackWithEvent(Track track, bool visible = true) {
         if (visible)
            TrackEditShowEvent?.Invoke(this, new TrackEventArgs(track));
         showTrack(track, visible);
      }

      void showTrack(Track track, bool visible) {
         mapControl.MapShowTrack(track, 
                                 visible, 
                                 visible ? editablegpx.NextVisibleEditableTrack(track) : null);
      }

      /// <summary>
      /// fügt eine Kopie des <see cref="Track"/> an den akt. Container und die ListBox an (oder ein)
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Track InsertCopy(Track orgtrack, int pos = -1) {
         return editablegpx.TrackInsertCopy(orgtrack, pos);
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> aus der internen Liste, seine Gpx-Daten und aus der ListBox
      /// </summary>
      /// <param name="track"></param>
      /// <param name="lb"></param>
      public void Remove(Track track) {
         showTrack(track, false);
         editablegpx.TrackRemove(track);
      }

      /// <summary>
      /// entfernt alle Tracks aus der internen Liste, seine Gpx-Daten und aus der ListBox
      /// </summary>
      public void RemoveAll() {
         while (editablegpx.TrackList.Count > 0) 
            Remove(editablegpx.TrackList[0]);
      }

      /// <summary>
      /// verschiebt die Position des Tracks in der Auflistung
      /// </summary>
      /// <param name="lb"></param>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void ChangeOrder(int fromidx, int toidx) {
         editablegpx.TrackOrderChange(fromidx, toidx);
      }

      public override string ToString() {
         return string.Format("InWork={0}{1}",
                              InWork,
                              InWork ?
                                 ", " + TrackInEdit.ToString() :
                                 "");
      }
   }
}
