#if ANDROID
using System.Drawing;
#endif
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geometry;
using SpecialMapCtrl;
using Gpx = FSofTUtils.Geography.PoorGpx;
using MyDrawing = System.Drawing;

#if ANDROID
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif
   /// <summary>
   /// Hilfsfunktionen für das Editieren (wird nur von <see cref="GpxWorkbench"/> verwendet)
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
      public event EventHandler<MarkerEventArgs>? MarkerShouldInsertEvent;

      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event EventHandler<TrackEventArgs>? TrackEditShowEvent;


      /// <summary>
      /// Farbe der Hilfslinie 
      /// </summary>
      public MyDrawing.Color HelperLineColor {
         get => penHelper.Color;
         set {
            penHelper = new Pen(value) {
               DashStyle = MyDrawing.Drawing2D.DashStyle.Dash,
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
      /// Ist ein Markerbearbeitung gestartet?
      /// </summary>
      public bool MarkerIsInWork => markerinwork;

      /// <summary>
      /// Ist ein Trackbearbeitung gestartet?
      /// </summary>
      public bool TrackIsInWork => trackinwork;

      /// <summary>
      /// akt. bearbeiteter Track
      /// </summary>
      public Track? TrackInEdit { get; protected set; }


      /// <summary>
      /// akt. zu verschiebender Marker
      /// </summary>
      Marker? markerInEdit;

      /// <summary>
      /// Läuft eine Markerbearbeitung?
      /// </summary>
      bool markerinwork = false;

      /// <summary>
      /// Kopie des Markers
      /// </summary>
      Marker? markerCopy;

      /// <summary>
      /// Kopie des Tracks
      /// </summary>
      Track? trackCopy;

      /// <summary>
      /// Läuft eine Trackbearbeitung?
      /// </summary>
      bool trackinwork = false;

      /// <summary>
      /// Wurde <see cref="TrackInEdit"/> verändert?
      /// </summary>
      bool trackchanged;

      /// <summary>
      /// Ist <see cref="TrackInEdit"/> ein neuer Track?
      /// </summary>
      bool trackIsNew;

      /// <summary>
      /// Control für die Anzeige des akt. bearbeitetenden Objektes
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl mapCtrl;

      /// <summary>
      /// Container des akt. bearbeitetenden Objektes
      /// </summary>
      GpxData gpx;

      /// <summary>
      /// Pen für Hilfslinien beim Editieren
      /// </summary>
      Pen penHelper;


      public EditHelper(SpecialMapCtrl.SpecialMapCtrl mapControl,
                        GpxData editableGpx,
                        MyDrawing.Color helperPenColor,
                        float helperPenWidth) {
         mapCtrl = mapControl;
         gpx = editableGpx;
         penHelper = new Pen(helperPenColor) {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            Width = helperPenWidth,
         };
      }

      #region private

      /// <summary>
      /// liefert die Geodaten für den Clientpunkt
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <param name="lon">geografische Höhe</param>
      /// <param name="lat">geografische Länge</param>
      /// <returns>Höhe</returns>
      double getGeoDat4ClientPoint(MyDrawing.Point ptclient, DemData? dem, out double lon, out double lat) {
         PointD ptgeo = mapCtrl.M_Client2LonLat(ptclient);
         double h = dem != null ? dem.GetHeight(ptgeo.X, ptgeo.Y) : DEM1x1.DEMNOVALUE;
         if (h == DEM1x1.DEMNOVALUE)
            h = Gpx.BaseElement.NOTVALID_DOUBLE;
         lon = ptgeo.X;
         lat = ptgeo.Y;
         return h;
      }

      /// <summary>
      /// zeichnet die Hilfslinie
      /// </summary>
      /// <param name="g"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      void drawHelperLine(Graphics g, MyDrawing.Point from, MyDrawing.Point to) => g.DrawLine(penHelper, from, to);

      #endregion

      /// <summary>
      /// Anzeige akt. falls ein Objekt in Arbeit ist
      /// </summary>
      public void Refresh() {
         if (MarkerIsInWork || TrackIsInWork)
            mapCtrl.M_Refresh();
      }

      /// <summary>
      /// liefert die Höhe zum Punkt des Kartenclients
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <returns></returns>
      public double GetHeight(MyDrawing.Point ptclient, DemData? dem) => getGeoDat4ClientPoint(ptclient, dem, out _, out _);

      #region Marker

      #region private

      /// <summary>
      /// ändert die Sichtbarkeit des <see cref="Marker"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      void showMarker(Marker marker, bool visible) =>
         mapCtrl.M_ShowMarker(marker,
                              visible,
                              visible ?
                                 gpx.NextVisibleMarker(marker) :
                                 null);

      #endregion

      /// <summary>
      /// neu anzeigen (weil sich die Daten geändert haben)
      /// </summary>
      /// <param name="marker"></param>
      public void RefreshOnMap(Marker marker) => marker.UpdateVisualMarker(mapCtrl);

      /// <summary>
      /// fügt eine Kopie des <see cref="Marker"/> in den Container ein
      /// </summary>
      /// <param name="orgmarker"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Marker InsertCopy(Marker orgmarker, int pos = -1) =>
         gpx.MarkerInsertCopyWithLock(orgmarker, pos, Marker.MarkerType.EditableStandard);

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus dem Container
      /// </summary>
      /// <param name="marker"></param>
      public void Remove(Marker marker) {
         showMarker(marker, false);          // Sichtbarkeit ausschalten
         gpx.MarkerRemoveWithLock(marker);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> im Container
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void MarkerChangeOrder(int fromidx, int toidx) {
         gpx.MarkerOrderChangeWithLock(fromidx, toidx);

         Marker m = gpx.MarkerList[toidx];
         if (m.IsVisible)
            m.UpdateVisualMarker(mapCtrl);
      }

      /// <summary>
      /// Cursor akt. (nur als Reaktion auf OnMarkerLeave nötig)
      /// </summary>
      public void RefreshCursor() {
         if (MarkerIsInWork) {
            Marker? tmp = markerInEdit;
            // sieht blöd aus, aber: Der Cursor wird intern beim Leave wieder auf Standard umgestellt. Mit diesem Trick erscheint wieder der richtige.
            mapCtrl.M_Refresh(false, false, false, false); 
            markerInEdit = tmp;
         }
      }

      #region Marker editieren

      /// <summary>
      /// Erweiterung zu Paint() (wenn <see cref="MarkerIsInWork"/>==true); Hilfslinie anzeigen
      /// </summary>
      public void DrawHelperLine2NewMarkerPosition(Graphics canvas, MyDrawing.Point ptLastMouseLocation) {
         if (MarkerIsInWork && markerInEdit != null)
            canvas.DrawLine(penHelper, mapCtrl.M_LonLat2Client(markerInEdit.Waypoint), ptLastMouseLocation);
      }

      /// <summary>
      /// Start für Marker verschieben oder neuen einfügen (marker == null)
      /// </summary>
      /// <returns>true wenn erfolgreich</returns>
      public bool MarkerEdit_Start(bool cancellast, Marker? marker) {
         if (cancellast && markerinwork)
            MarkerEdit_End(true);

         if (!markerinwork) {
            markerinwork = true;
            markerInEdit = marker;
            markerCopy = marker != null ?
                              new Marker(marker) :
                              null;
            return true;
         }
         return false;
      }

      /// <summary>
      /// neuen Marker setzen oder vorhandenen Marker an neue Position setzen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      /// <param name="cancel"></param>
      public void MarkerEdit_End(MyDrawing.Point ptclient, DemData? dem, bool cancel) {
         if (markerinwork) {
            markerinwork = false;
            if (cancel) {
               markerCopy = null;
               if (markerInEdit != null)
                  RefreshOnMap(markerInEdit);
               markerInEdit = null;
            } else {
               double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
               Gpx.GpxWaypoint wp = new Gpx.GpxWaypoint(lon, lat, ele);
               if (markerInEdit == null) {
                  MarkerShouldInsertEvent?.Invoke(this,
                                                  new MarkerEventArgs(new Marker(wp, Marker.MarkerType.EditableStandard, null)));  // neuer Marker
               } else {
                  // (nur) Pos. und Höhe neu setzen
                  markerInEdit.Longitude = wp.Lon;
                  markerInEdit.Latitude = wp.Lat;
                  markerInEdit.Elevation = wp.Elevation;
                  gpx.GpxDataChanged = true;  // muss explizit gesetzt werden, weil die Eigenschaften eines vorhandenen Objekts geändert werden
                  RefreshOnMap(markerInEdit);
               }
            }
         }
      }

      public void MarkerEdit_End(bool cancel) =>
         MarkerEdit_End(MyDrawing.Point.Empty, null, cancel);

      #endregion

      #endregion

      #region Track

      #region private

      Gpx.ListTS<Gpx.GpxTrackPoint>? getTrackPoints(Track? track) =>
                                            track != null &&
                                            track.GpxSegment != null &&
                                            track.GpxSegment.Points.Count >= 0 ? track.GpxSegment.Points : null;

      /// <summary>
      /// ändert die Sichtbarkeit des <see cref="Track"/> und informiert (nur) bei true vorher (!) per Event darüber
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      void showTrackWithEvent(Track track, bool visible = true) {
         if (visible)
            TrackEditShowEvent?.Invoke(this, new TrackEventArgs(track));
         showTrack(track, visible);
      }

      void showTrack(Track track, bool visible) => mapCtrl.M_ShowTrack(track,
                                                                       visible,
                                                                       visible ?
                                                                            gpx.NextVisibleTrack(track) :
                                                                            null);

      bool appendPoint(Track? track, MyDrawing.Point ptclient, DemData? dem) {
         if (track != null) {
            Gpx.ListTS<Gpx.GpxTrackPoint>? points = getTrackPoints(track);
            if (points != null) {
               showTrackWithEvent(track, false);         // Anzeige des bisherigen Tracks löschen
               double ele = getGeoDat4ClientPoint(ptclient, dem, out double lon, out double lat);
               points.Add(new Gpx.GpxTrackPoint(lon, lat, ele)); // neuen Punkt aufnehmen
               trackchanged = true;
               track.CalculateStats();
               track.UpdateVisualTrack(mapCtrl);
               showTrackWithEvent(track);       // veränderten Track anzeigen
               return true;
            }
         }
         return false;
      }

      bool removeLastPoint(Track? track) {
         if (track != null) {
            Gpx.ListTS<Gpx.GpxTrackPoint>? points = getTrackPoints(track);
            if (points != null &&
                points.Count > 0) {
               showTrackWithEvent(track, false);         // Anzeige des bisherigen Tracks löschen
               points.RemoveAt(points.Count - 1);
               trackchanged = true;
               track.CalculateStats();
               track.UpdateVisualTrack(mapCtrl);
               showTrackWithEvent(track);                                // veränderten Track anzeigen
            }
            return true;
         }
         return false;
      }

      bool removeNextPoint(Track? track, MyDrawing.Point ptClient) {
         if (track != null) {
            if (ptClient != MyDrawing.Point.Empty) {                   // Aktion NICHT abgebrochen
               bool ok = false;
               int ptidx = track.GetNearestPtIdx(mapCtrl.M_Client2LonLat(ptClient));
               if (ptidx >= 0) {
                  showTrackWithEvent(track, false);                     // Anzeige des bisherigen Tracks ausschalten
                  if (gpx.TrackRemovePointWithLock(track, ptidx)) {
                     trackchanged = true;
                     track.CalculateStats();
                     track.UpdateVisualTrack();
                     ok = true;
                  }
                  showTrackWithEvent(track);     // wieder anzeigen
               }
               return ok;
            }
         }
         return false;
      }

      /// <summary>
      /// trennt den Track am Trackpunkt, der dem Clientpunkt am nächsten liegt und liefert den neuen Track
      /// </summary>
      /// <param name="track"></param>
      /// <param name="ptClient"></param>
      /// <returns>wenn erfolgreich, neuer Track (bei Abbruch auch null)</returns>
      Track? trackSplit(Track? track, MyDrawing.Point ptClient, bool cancel) {
         Track? newtrack = null;
         if (track != null) {
            int ptidx = track.GetNearestPtIdx(mapCtrl.M_Client2LonLat(ptClient));
            if (ptidx >= 0) {
               showTrackWithEvent(track, false);                     // Anzeige des bisherigen Tracks ausschalten
               track.IsOnEdit = false;
               if (!cancel) {                                        // Aktion NICHT abgebrochen
                  newtrack = gpx.TrackSplitWithLock(track, ptidx);
                  track.IsMarked4Edit = false;
                  track.UpdateVisualTrack();
                  if (newtrack != null) {
                     newtrack.LineColor = track.LineColor;
                     newtrack.LineWidth = track.LineWidth;
                     newtrack.UpdateVisualTrack();
                     showTrackWithEvent(newtrack);
                  }
               }
               showTrackWithEvent(track);     // wieder anzeigen
            }
         }
         return newtrack;
      }

      /// <summary>
      /// beendet das Verknüpfen von 2 Tracks
      /// </summary>
      /// <param name="track">1. Track</param>
      /// <param name="trackappend">wenn null, dann Abbruch</param>
      /// <returns>true wenn erfolgreich oder abgebrochen</returns>
      bool trackConcat(Track? track, Track? trackappend) {
         if (track != null) {
            if (trackappend == null ||
                (trackappend != null && trackappend.IsEditable)) {
               showTrackWithEvent(track, false);               // Anzeige des bisherigen Tracks löschen
               track.IsOnEdit = false;
               if (trackappend != null) {                         // sonst Aktion abgebrochen
                  showTrackWithEvent(trackappend, false);
                  gpx.TrackConcatWithLock(track, trackappend);
                  track.UpdateVisualTrack();
               }
               showTrackWithEvent(track);     // wieder anzeigen
               return true;
            }
         }
         return false;
      }

      #endregion

      #region Hilfslinien

      /// <summary>
      /// zeichnet eine Hilfslinie vom Ende des akt. bearbeiteten Tracks zum angegebenen Punkt
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptClient"></param>
      public void DrawHelperLine2LastTrackPoint(Graphics canvas, MyDrawing.Point ptClient) {
         Gpx.ListTS<Gpx.GpxTrackPoint>? points = getTrackPoints(TrackInEdit);
         if (points != null && points.Count > 0)
            canvas.DrawLine(penHelper, mapCtrl.M_LonLat2Client(points[points.Count - 1]), ptClient);
      }

      /// <summary>
      /// zeichnet eine Hilfslinie vom angegebenen Punkt zum nächstgelegenen Trackpunkt des akt. bearbeiteten Tracks
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="ptClient"></param>
      public void DrawHelperLine2NextTrackPoint(Graphics canvas, MyDrawing.Point ptClient) {
         Gpx.ListTS<Gpx.GpxTrackPoint>? points = getTrackPoints(TrackInEdit);
         if (points != null && TrackInEdit != null) {
            int ptidx = TrackInEdit.GetNearestPtIdx(mapCtrl.M_Client2LonLat(ptClient));
            if (ptidx >= 0)
               canvas.DrawLine(penHelper, mapCtrl.M_LonLat2Client(points[ptidx]), ptClient);
         }
      }

      /// <summary>
      /// zeichnet eine Hilfslinie vom Ende des akt. bearbeiteten Tracks zum Anfang des angegebenen Tracks
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="trackappend"></param>
      public void DrawHelperLine2NextTrack(Graphics canvas, Track trackappend) {
         Gpx.ListTS<Gpx.GpxTrackPoint>? points = getTrackPoints(TrackInEdit);
         Gpx.ListTS<Gpx.GpxTrackPoint>? pointsapp = getTrackPoints(trackappend);
         if (points != null && points.Count > 0 &&
             pointsapp != null && pointsapp.Count > 0)
            canvas.DrawLine(penHelper,
                            mapCtrl.M_LonLat2Client(points[points.Count - 1]),
                            mapCtrl.M_LonLat2Client(pointsapp[0]));
      }

      #endregion

      /// <summary>
      /// liefert true wenn genau dieser Track gerade bearbeitet wird
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool ThisTrackIsInWork(Track track) => TrackIsInWork &&
                                                    track != null &&
                                                    Equals(track, TrackInEdit);

      /// <summary>
      /// fügt eine Kopie des <see cref="Track"/> in den Container ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <param name="useorgprops">bei false wird die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public Track InsertCopy(Track orgtrack, int pos = -1, bool useorgprops = false) =>
         gpx.TrackInsertCopyWithLock(orgtrack, pos, useorgprops);

      /// <summary>
      /// entfernt den <see cref="Track"/> aus dem Container
      /// </summary>
      /// <param name="track"></param>
      public void Remove(Track track) {
         if (!track.IsOnLiveDraw) {
            showTrack(track, false);            // Sichtbarkeit ausschalten
            gpx.TrackRemoveWithLock(track);
         }
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Track"/> im Container
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      public void TrackChangeOrder(int fromidx, int toidx) => gpx.TrackOrderChangeWithLock(fromidx, toidx);

      #region Track editieren

      /// <summary>
      /// Start der Bearbeitung eines bestehenden <see cref="Track"/> (mit Erzeugung einer Kopie) 
      /// oder einen neuen Track erzeugen
      /// </summary>
      /// <param name="cancellast">bei true wird eine ev. noch laufende Aktion abgebrochen</param>
      /// <param name="track"></param>
      /// <returns>true wenn erfolgreich</returns>
      public bool TrackEdit_Start(bool cancellast, Track? track) {
         if (cancellast && trackinwork)
            TrackEdit_End(true);

         if (!trackinwork) {
            // Init.
            trackchanged = false;
            trackIsNew = track == null;
            trackCopy = null;

            if (track != null) {    // bestehender Track
               TrackInEdit = track;
               trackCopy = Track.CreateCopy(track);

               showTrackWithEvent(track, false);   // normale Darstellung ausschalten
               TrackInEdit.IsOnEdit = true;
               TrackInEdit.UpdateVisualTrack();
               showTrackWithEvent(TrackInEdit);    // "Edit"-Darstellung einschalten
            }
            trackinwork = true;
            return true;
         }
         return false;
      }

      /// <summary>
      /// neuen Punkt anhängen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="dem"></param>
      public bool TrackEdit_AppendPoint(MyDrawing.Point ptclient, DemData? dem) {
         if (trackinwork) {
            if (TrackInEdit == null) {                // 1. Punkt für neuen Track
               TrackInEdit = InsertCopy(new Track([], "Track " + DateTime.Now.ToString(@"d.MM.yyyy, H:mm:ss")),
                                        0);
               TrackInEdit.IsOnEdit = true;
               TrackInEdit.UpdateVisualTrack();
               showTrackWithEvent(TrackInEdit);       // "Edit"-Darstellung einschalten
            }
            if (appendPoint(TrackInEdit, ptclient, dem)) {
               gpx.GpxDataChanged = true;
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// letzten Punkt wieder entfernen
      /// </summary>
      public bool TrackEdit_RemoveLastPoint() {
         if (trackinwork &&
             TrackInEdit != null &&
             TrackInEdit.GpxSegment != null) {
            if (removeLastPoint(TrackInEdit)) {
               gpx.GpxDataChanged = true;
               if (TrackInEdit.GpxSegment.Points.Count == 0)
                  TrackEdit_End(false);
               return true;
            }
         }
         return false;
      }

      public bool TrackEdit_RemoveNextPoint(MyDrawing.Point ptClient) {
         if (trackinwork &&
             TrackInEdit != null &&
             TrackInEdit.GpxSegment != null) {
            if (removeNextPoint(TrackInEdit, ptClient)) {
               gpx.GpxDataChanged = true;
               if (TrackInEdit.GpxSegment.Points.Count == 0)
                  TrackEdit_End(false);
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Abschluss des <see cref="Track"/>-Zeichnen
      /// </summary>
      /// <param name="cancel">wenn true dann Abbruch der Bearbeitung</param>
      /// <returns>true wenn die Trackeditierung beendet oder abgebrochen werden konnte</returns>
      public bool TrackEdit_End(bool cancel) {
         if (trackinwork) {
            trackinwork = false;
            if (TrackInEdit != null) {
               showTrackWithEvent(TrackInEdit, false);          // Anzeige des bisherigen Tracks löschen
               TrackInEdit.IsOnEdit = false;

               if (trackchanged) {
                  if (cancel) {           // Abbruch

                     if (trackIsNew)      // neuen Track entfernen
                        Remove(TrackInEdit);
                     else {               // alte Version wiederherstellen ...
                        if (trackCopy != null &&
                            trackCopy.GpxSegment != null) {
                           showTrackWithEvent(TrackInEdit, false);
                           TrackInEdit.ReplaceAllPoints(trackCopy.GpxSegment);
                           showTrackWithEvent(TrackInEdit);                // ... und wieder anzeigen
                        }
                     }

                  } else {

                     trackCopy = null;

                     if (TrackInEdit.GpxSegment != null &&
                         TrackInEdit.GpxSegment.Points.Count > 1) {     // Trackaufzeichnung beenden und Track im Container speichern
                        showTrackWithEvent(TrackInEdit);                // wieder anzeigen
                        TrackInEdit.RefreshBoundingbox();
                        TrackInEdit.CalculateStats();
                     } else {       // zu wenig Punkte
                        Remove(TrackInEdit);
                     }

                  }
               } else
                  showTrackWithEvent(TrackInEdit);                      // wieder anzeigen

               TrackInEdit = null;
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Abschluss des <see cref="Track"/>-Trennen
      /// </summary>
      /// <param name="ptClient">wenn </param>
      /// <param name="newtrack">neuer Track</param>
      /// <param name="cancel">wenn true dann Abbruch der Bearbeitung</param>
      /// <returns>true wenn die Trackeditierung beendet oder abgebrochen werden konnte</returns>
      public bool TrackEdit_End(MyDrawing.Point ptClient, out Track? newtrack, bool cancel) {
         if (trackinwork) {
            newtrack = trackSplit(TrackInEdit, ptClient, cancel);
            if (!cancel && newtrack != null)
               gpx.GpxDataChanged = true;
            return TrackEdit_End(cancel) ||
                   newtrack != null;
         }
         newtrack = null;
         return false;
      }

      /// <summary>
      /// Verknüpft den <see cref="TrackInEdit"/> mit dem <see cref="Track"/>
      /// </summary>
      /// <param name="track"></param>
      /// <returns>true wenn erfolgreich oder abgebrochen</returns>
      public bool TrackEdit_End(Track? track, bool cancel) {
         if (trackinwork) {
            if (!cancel && trackConcat(TrackInEdit, track)) {
               gpx.GpxDataChanged = true;
               return TrackEdit_End(cancel);
            }
            if (cancel)
               return TrackEdit_End(true);
            else
               TrackEdit_End(true);
         }
         return false;
      }

      #endregion

      #endregion
   }

}
