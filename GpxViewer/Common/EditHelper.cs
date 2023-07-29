using System;
using System.Drawing;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using SpecialMapCtrl;
//using Microsoft.Win32;
using Gpx = FSofTUtils.Geography.PoorGpx;

#if Android
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif
   /// <summary>
   /// Hilfsfunktionen für das Editieren
   /// </summary>
   public class EditHelper {

      public class MarkerEventArgs {

         public Marker Marker;

         public MarkerEventArgs(Marker marker) {
            Marker = marker;
         }

      }

      public class TrackEventArgs {

         public Track Track;

         public TrackEventArgs(Track track) {
            Track = track;
         }

      }

      /// <summary>
      /// ein neuer Marker sollte eingefügt werden
      /// </summary>
      public event EventHandler<MarkerEventArgs> MarkerShouldInsertEvent;

      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event EventHandler<TrackEventArgs> TrackEditShowEvent;

      public event EventHandler RefreshProgramStateEvent;


      /// <summary>
      /// Farbe der Hilfslinie 
      /// </summary>
      public Color HelperLineColor {
         get => penHelper.Color;
         set {
            penHelper = new Pen(value) {
               DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
               Width = penHelper.Width,
            };
         }
      }

      /// <summary>
      /// Breite der Hilfslinie
      /// </summary>
      public float HelperLineWidth {
         get => penHelper.Width;
         set => penHelper.Width = value;
      }

      /// <summary>
      /// Bearbeitung "in Arbeit" (kann nur eine Verschiebung sein)
      /// </summary>
      public bool MarkerInWork => markerInEdit != null;

      /// <summary>
      /// eine Bearbeitung ist "in Arbeit"
      /// </summary>
      public bool TrackInWork => TrackInEdit != null;

      /// <summary>
      /// akt. bearbeiteter Track
      /// </summary>
      public Track TrackInEdit { get; protected set; }


      /// <summary>
      /// akt. zu verschiebender Marker
      /// </summary>
      Marker markerInEdit;

      /// <summary>
      /// Kopie des Markers
      /// </summary>
      Marker markerCopy;

      /// <summary>
      /// Kopie des Tracks
      /// </summary>
      Track trackCopy;

      bool trackchanged = false;

      bool trackIsNew;

      SpecialMapCtrl.SpecialMapCtrl mapControl;

      GpxAllExt gpx;

      /// <summary>
      /// Pen für Hilfslinien beim Editieren
      /// </summary>
      Pen penHelper;


      public EditHelper(SpecialMapCtrl.SpecialMapCtrl mapControl,
                        GpxAllExt editableGpx,
                        Color helperPenColor,
                        float helperPenWidth) {
         this.mapControl = mapControl;
         gpx = editableGpx;
         penHelper = new Pen(helperPenColor) {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            Width = helperPenWidth,
         };
      }

      #region private

      /// <summary>
      /// der Programmstatus wird aktualisiert
      /// <para>
      /// sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
      /// </para>
      /// </summary>
      void refreshProgramState() => RefreshProgramStateEvent?.Invoke(this, new EventArgs());

      /// <summary>
      /// liefert die Geodaten für den Clientpunkt
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <param name="lon">geografische Höhe</param>
      /// <param name="lat">geografische Länge</param>
      /// <returns>Höhe</returns>
      double getGeoDat4ClientPoint(Point ptclient, DemData dem, out double lon, out double lat) {
         PointD ptgeo = getPointD4ClientPoint(ptclient);
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
      PointD getPointD4ClientPoint(Point ptclient) => mapControl.SpecMapClient2LonLat(ptclient);

      Point convertTrackPoint2Point(Gpx.GpxTrackPoint pt) => mapControl.SpecMapLonLat2Client(pt);

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxWaypoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      Gpx.GpxWaypoint GetGpxWaypoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxWaypoint(lon, lat, ele);
      }

      /// <summary>
      /// liefert einen <see cref="Gpx.GpxTrackPoint"/> zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      Gpx.GpxTrackPoint getGpxTrackPoint(Point ptclient, DemData dem) {
         double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
         return new Gpx.GpxTrackPoint(lon, lat, ele);
      }

      /// <summary>
      /// zeichnet die Hilfslinie
      /// </summary>
      /// <param name="g"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      void drawHelperLine(Graphics g, Point from, Point to) => g.DrawLine(penHelper, from, to);

      /// <summary>
      /// ändert die Sichtbarkeit des <see cref="Marker"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      void showMarker(Marker marker, bool visible) =>
         mapControl.SpecMapShowMarker(marker,
                                      visible,
                                      visible ?
                                         gpx.NextVisibleMarker(marker) :
                                         null);

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
                                                                                    gpx.NextVisibleTrack(track) :
                                                                                    null);

      #endregion

      /// <summary>
      /// Anzeige akt.
      /// </summary>
      public void Refresh() {
         if (MarkerInWork ||
             (TrackInWork && TrackInEdit.GpxSegment.Points.Count > 0))
            mapControl.Map_Refresh();
      }

      /// <summary>
      /// liefert die Höhe zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public double GetHeight(Point ptclient, DemData dem) => getGeoDat4ClientPoint(ptclient, dem, out _, out _);

      #region Marker

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
         gpx.MarkerInsertCopy(orgmarker, pos, Marker.MarkerType.EditableStandard);

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus dem Container
      /// </summary>
      /// <param name="marker"></param>
      public void Remove(Marker marker) {
         showMarker(marker, false);          // Sichtbarkeit ausschalten
         gpx.MarkerRemove(marker);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> im Container
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void MarkerChangeOrder(int fromidx, int toidx) {
         gpx.MarkerOrderChange(fromidx, toidx);

         Marker m = gpx.MarkerList[toidx];
         if (m.IsVisible)
            m.UpdateVisualMarker(mapControl);
      }

      /// <summary>
      /// Cursor akt. (nur als Reaktion auf OnMarkerLeave nötig)
      /// </summary>
      public void RefreshCursor() {
         if (MarkerInWork) {
            Marker tmp = markerInEdit;
            refreshProgramState();  // sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
            markerInEdit = tmp;
         }
      }

      #region Marker editieren

      /// <summary>
      /// Start für Marker verschieben oder neuen einfügen (marker == null)
      /// </summary>
      public void MarkerEditStart(Marker marker = null) {
         markerInEdit = marker;
         markerCopy = marker != null ?
                           new Marker(marker) :
                           null;
      }

      /// <summary>
      /// Erweiterung zu Paint() (wenn <see cref="MarkerInWork"/>==true); Hilfslinie anzeigen
      /// </summary>
      public void MarkerEditDrawDestinationLine(Graphics canvas, Point ptLastMouseLocation) {
         if (MarkerInWork)
            drawHelperLine(canvas, mapControl.SpecMapLonLat2Client(markerInEdit.Waypoint), ptLastMouseLocation);
      }

      /// <summary>
      /// setzt die (neue) Position (implizit erfolgt auch <see cref="MarkerEditEnd"/>)
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      public void MarkerEditSetNewPos(Point ptclient, DemData dem) {
         Gpx.GpxWaypoint wp = GetGpxWaypoint(ptclient, dem);
         if (markerInEdit != null) {
            // (nur) Pos. und Höhe neu setzen
            markerInEdit.Longitude = wp.Lon;
            markerInEdit.Latitude = wp.Lat;
            markerInEdit.Elevation = wp.Elevation;
            gpx.GpxDataChanged = true;  // muss explizit gesetzt werden, weill die Eigenschaften eines vorhandenen Objekts geändert werden
            RefreshOnMap(markerInEdit);
         } else
            MarkerShouldInsertEvent?.Invoke(this, new MarkerEventArgs(new Marker(wp, Marker.MarkerType.EditableStandard, null)));  // neuer Marker
         MarkerEditEnd();
      }

      public void MarkerEditEnd(bool cancel = false) {
         if (markerInEdit != null && cancel) {
            markerInEdit.Longitude = markerCopy.Longitude;
            markerInEdit.Latitude = markerCopy.Latitude;
            markerInEdit.Elevation = markerCopy.Elevation;
         }
         markerInEdit = null;
      }

      #endregion

      #endregion

      #region Track

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

      #region Hilfslinien

      /// <summary>
      /// Vorschau für neuen zusätzlichen Punkt
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptDestination"></param>
      public void TrackDrawDestinationLine(Graphics canvas, Point ptDestination) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {
            drawHelperLine(canvas,
                           convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                           ptDestination);
         }
      }

      /// <summary>
      /// Vorschau auf den Punkt für die Trennung
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptLastMouseLocation"></param>
      public void TrackDrawSplitPoint(Graphics canvas, Point ptLastMouseLocation) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {
            int ptidx = TrackInEdit.GetNearestPtIdx(getPointD4ClientPoint(ptLastMouseLocation));
            drawHelperLine(canvas,
                           convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[ptidx]),
                           ptLastMouseLocation);
         }
      }

      /// <summary>
      /// Vorschau für die Verbindung zweier Tracks
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="trackappend"></param>
      public void TrackDrawConcatLine(Graphics canvas, Track trackappend) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0 &&
             trackappend != null &&
             trackappend.GpxSegment.Points.Count > 0)
            drawHelperLine(canvas,
                           convertTrackPoint2Point(TrackInEdit.GpxSegment.Points[TrackInEdit.GpxSegment.Points.Count - 1]),
                           convertTrackPoint2Point(trackappend.GpxSegment.Points[0]));
      }

      #endregion

      #region Track editieren

      /// <summary>
      /// bestehenden <see cref="Track"/> bearbeiten oder einen neuen erzeugen
      /// </summary>
      /// <param name="track"></param>
      public void TrackEditStart(Track track = null) {
         trackchanged = false;
         trackIsNew = track == null;
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
      public void TrackEditDraw_AppendPoint(Point ptclient, DemData dem) {
         if (TrackInEdit == null) {                // 1. Punkt für neuen Track
            TrackInEdit = InsertCopy(new Track(new Gpx.GpxTrackPoint[0],
                                               "Track " + DateTime.Now.ToString(@"d.MM.yyyy, H:mm:ss")),
                                     0);
            TrackInEdit.IsOnEdit = true;
            showTrack(TrackInEdit, true);           // als editierbaren Track anzeigen
         }

         if (TrackInEdit != null) {
            showTrackWithEvent(TrackInEdit, false);                         // Anzeige des bisherigen Tracks löschen
            TrackInEdit.GpxSegment.Points.Add(getGpxTrackPoint(ptclient, dem));  // neuen Punkt aufnehmen
            gpx.GpxDataChanged = true;
            TrackInEdit.UpdateVisualTrack(mapControl);
            showTrackWithEvent(TrackInEdit);                                // veränderten Track anzeigen
            trackchanged = true;
         }
      }

      /// <summary>
      /// letzten Punkt wieder entfernen
      /// </summary>
      public void TrackEditDraw_RemoveLastPoint() {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 1) {
            showTrackWithEvent(TrackInEdit, false);                         // Anzeige des bisherigen Tracks löschen
            TrackInEdit.GpxSegment.Points.RemoveAt(TrackInEdit.GpxSegment.Points.Count - 1);
            gpx.GpxDataChanged = true;
            TrackInEdit.UpdateVisualTrack(mapControl);
            showTrackWithEvent(TrackInEdit);                                // veränderten Track anzeigen
            trackchanged = true;
         }
      }

      /// <summary>
      /// Abschluss des <see cref="Track"/>-Zeichnen
      /// </summary>
      /// <param name="cancel"></param>
      public void TrackEditEndDraw(bool cancel = false) {
         if (TrackInEdit != null) {
            showTrackWithEvent(TrackInEdit, false);          // Anzeige des bisherigen Tracks löschen
            TrackInEdit.IsOnEdit = false;

            if (trackchanged) {
               if (cancel) {           // Abbruch

                  if (!trackIsNew) {        // alte Version wiederherstellen
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
      public Track TrackEndSplit(Point ptLastMouseLocation) {
         Track resulttrack = null;
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0) {

            showTrackWithEvent(TrackInEdit, false);                     // Anzeige des bisherigen Tracks ausschalten
            TrackInEdit.IsOnEdit = false;

            if (ptLastMouseLocation != Point.Empty) {                   // Aktion NICHT abgebrochen
               int ptidx = TrackInEdit.GetNearestPtIdx(getPointD4ClientPoint(ptLastMouseLocation));
               Track newtrack = gpx.TrackSplit(TrackInEdit, ptidx);
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
      public void TrackEndConcat(Track track) {
         if (TrackInEdit != null &&
             TrackInEdit.GpxSegment.Points.Count > 0 &&
             (track == null || (track != null && track.IsEditable))) {
            showTrackWithEvent(TrackInEdit, false);     // Anzeige des bisherigen Tracks löschen
            TrackInEdit.IsOnEdit = false;

            if (track != null) {                         // sonst Aktion abgebrochen
               showTrackWithEvent(track, false);
               gpx.TrackConcat(TrackInEdit, track);
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
         gpx.TrackInsertCopy(orgtrack, pos, useorgprops);

      /// <summary>
      /// entfernt den <see cref="Track"/> aus dem Container
      /// </summary>
      /// <param name="track"></param>
      /// <param name="lb"></param>
      public void Remove(Track track) {
         showTrack(track, false);            // Sichtbarkeit ausschalten
         gpx.TrackRemove(track);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Track"/> im Container
      /// </summary>
      /// <param name="lb"></param>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void TrackChangeOrder(int fromidx, int toidx) => gpx.TrackOrderChange(fromidx, toidx);

      #endregion
   }

}
