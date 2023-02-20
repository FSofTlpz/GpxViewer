using FSofTUtils.Geography.DEM;
using System;
using System.Drawing;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl.EditHelper {

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

      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event EventHandler<TrackEventArgs> TrackEditShowEvent;


      /// <summary>
      /// akt. bearbeiteter Track
      /// </summary>
      public Track TrackInEdit { get; protected set; }

      bool trackchanged = false;

      /// <summary>
      /// Kopie des Tracks
      /// </summary>
      Track trackCopy;

      /// <summary>
      /// eine Bearbeitung ist "in Arbeit"
      /// </summary>
      public bool InWork => TrackInEdit != null;


      public EditTrackHelper(SpecialMapCtrl mapControl,
                             GpxAllExt editableGpx,
                             Color movingPenColor,
                             float movingPenWidth) :
         base(mapControl, editableGpx, movingPenColor, movingPenWidth) { }


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

      #region Hilfslinien
      Point convertTrackPoint2Point(Gpx.GpxTrackPoint pt) => mapControl.SpecMapLonLat2Client(pt);

      /// <summary>
      /// Vorschau für neuen zusätzlichen Punkt
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptDestination"></param>
      public void DrawDestinationLine(Graphics canvas, Point ptDestination) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {
            DrawHelperLine(canvas,
                           convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                           ptDestination);
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
            DrawHelperLine(canvas,
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
            DrawHelperLine(canvas,
                           convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                           convertTrackPoint2Point(trackappend.GpxSegment.Points[0]));
      }

      #endregion

      #region Track editieren

      /// <summary>
      /// bestehenden <see cref="Track"/> bearbeiten oder einen neuen erzeugen
      /// </summary>
      /// <param name="track"></param>
      public void EditStart(Track track = null) {
         trackchanged = false;
         IsNew = track == null;
         trackCopy = null;

         if (track != null) {
            TrackInEdit = track;
            trackCopy = Track.CreateCopy(track);

            showTrackWithEvent(track, false);   // normale Darstellung ausschalten
            TrackInEdit.IsOnEdit = true;
            TrackInEdit.UpdateVisualTrack();
            showTrackWithEvent(TrackInEdit);    // "Edit"-Darstellung einschalten
         }
      }

      /// <summary>
      /// neuen Punkt anhängen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      public void EditDraw_AppendPoint(Point ptclient, DemData dem) {
         if (TrackInEdit == null) {                // 1. Punkt für neuen Track
            TrackInEdit = InsertCopy(new Track(new Gpx.GpxTrackPoint[0],
                                               "Track " + DateTime.Now.ToString(@"d.MM.yyyy, H:mm:ss")),
                                     0);
            TrackInEdit.IsOnEdit = true;
            showTrack(TrackInEdit, true);           // als editierbaren Track anzeigen
         }

         if (TrackInEdit != null) {
            showTrackWithEvent(TrackInEdit, false);                         // Anzeige des bisherigen Tracks löschen
            TrackInEdit.GpxSegment.Points.Add(GetGpxTrackPoint(ptclient, dem));  // neuen Punkt aufnehmen
            editablegpx.GpxDataChanged = true;
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
            editablegpx.GpxDataChanged = true;
            TrackInEdit.UpdateVisualTrack(mapControl);
            showTrackWithEvent(TrackInEdit);                                // veränderten Track anzeigen
            trackchanged = true;
         }
      }

      /// <summary>
      /// Abschluss des <see cref="Track"/>-Zeichnen
      /// </summary>
      /// <param name="cancel"></param>
      public void EditEndDraw(bool cancel = false) {
         if (TrackInEdit != null) {
            showTrackWithEvent(TrackInEdit, false);          // Anzeige des bisherigen Tracks löschen
            TrackInEdit.IsOnEdit = false;

            if (trackchanged) {
               if (cancel) {           // Abbruch

                  if (!IsNew) {        // alte Version wiederherstellen
                     TrackInEdit.ReplaceAllPoints(trackCopy.GpxSegment);
                     showTrackWithEvent(TrackInEdit);                // wieder anzeigen
                  } else {             // neuen Track entfernen
                     Remove(TrackInEdit);
                  }

               } else {

                  if (TrackInEdit.GpxSegment.Points.Count > 1) {     // Trackaufzeichnung beenden und Track im Container speichern
                     showTrackWithEvent(TrackInEdit);                // wieder anzeigen
                     TrackInEdit.RefreshBoundingbox();
                     TrackInEdit.CalculateStats();
                  } else {       // zu wenig Punkte
                     Remove(TrackInEdit);
                  }

               }
            } else
               showTrackWithEvent(TrackInEdit);                      // wieder anzeigen

         }
         TrackInEdit = null;
      }

      /// <summary>
      /// Abschluss des <see cref="Track"/>-Trennen
      /// </summary>
      /// <param name="ptLastMouseLocation"></param>
      public Track EndSplit(Point ptLastMouseLocation) {
         Track resulttrack = null;
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {

            showTrackWithEvent(TrackInEdit, false);                     // Anzeige des bisherigen Tracks ausschalten
            TrackInEdit.IsOnEdit = false;

            if (ptLastMouseLocation != Point.Empty) {                   // Aktion NICHT abgebrochen
               int ptidx = TrackInEdit.GetNearestPtIdx(GetPointD4ClientPoint(ptLastMouseLocation));
               Track newtrack = editablegpx.TrackSplit(TrackInEdit, ptidx);
               TrackInEdit.IsMarked4Edit = false;
               TrackInEdit.UpdateVisualTrack();
               newtrack.LineColor = TrackInEdit.LineColor;
               newtrack.LineWidth = TrackInEdit.LineWidth;
               newtrack.IsMarked4Edit = true;
               resulttrack = newtrack;
            }

            showTrackWithEvent(TrackInEdit);     // wieder anzeigen
         }
         TrackInEdit = null;
         return resulttrack;
      }

      /// <summary>
      /// Abschluss für die Verbindung von 2 <see cref="Track"/>
      /// </summary>
      /// <param name="track"></param>
      public void EndConcat(Track track) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0 &&
             (track == null || (track != null && track.IsEditable))) {
            showTrackWithEvent(TrackInEdit, false);     // Anzeige des bisherigen Tracks löschen
            TrackInEdit.IsOnEdit = false;

            if (track != null) {                         // sonst Aktion abgebrochen
               showTrackWithEvent(track, false);
               editablegpx.TrackConcat(TrackInEdit, track);
               TrackInEdit.UpdateVisualTrack();
            }

            showTrackWithEvent(TrackInEdit);     // wieder anzeigen
         }
         TrackInEdit = null;
      }

      #endregion

      /// <summary>
      /// fügt eine Kopie des <see cref="Track"/> in den Container ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <param name="useorgprops">bei false wird die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public Track InsertCopy(Track orgtrack, int pos = -1, bool useorgprops = false) => 
         editablegpx.TrackInsertCopy(orgtrack, pos, useorgprops);

      /// <summary>
      /// entfernt den <see cref="Track"/> aus dem Container
      /// </summary>
      /// <param name="track"></param>
      /// <param name="lb"></param>
      public void Remove(Track track) {
         showTrack(track, false);            // Sichtbarkeit ausschalten
         editablegpx.TrackRemove(track);
      }

      /// <summary>
      /// entfernt alle <see cref="Track"/> aus dem Container
      /// </summary>
      public void RemoveAll() {
         while (editablegpx.TrackList.Count > 0)
            Remove(editablegpx.TrackList[0]);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Track"/> im Container
      /// </summary>
      /// <param name="lb"></param>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void ChangeOrder(int fromidx, int toidx) => editablegpx.TrackOrderChange(fromidx, toidx);

      /// <summary>
      /// ändert die Sichtbarkeit des <see cref="Track"/> und informiert per Event darüber
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      void showTrackWithEvent(Track track, bool visible = true) {
         if (visible)
            TrackEditShowEvent?.Invoke(this, new TrackEventArgs(track));
         showTrack(track, visible);
      }

      void showTrack(Track track, bool visible) => mapControl.SpecMapShowTrack(track,
                                                                               visible,
                                                                               visible ?
                                                                                    editablegpx.NextVisibleTrack(track) :
                                                                                    null);

      public override string ToString() {
         return string.Format("InWork={0}{1}",
                              InWork,
                              InWork ?
                                 ", " + TrackInEdit.ToString() :
                                 "");
      }
   }
}
