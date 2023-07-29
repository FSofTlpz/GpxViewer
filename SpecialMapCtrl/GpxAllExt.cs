using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace SpecialMapCtrl {
   public class GpxAllExt : GpxAll {

      #region Events

      /// <summary>
      /// wird ausgelöst, wenn auf <see cref="GpxDataChanged"/> schreibend zugegriffen wurde
      /// </summary>
      public event EventHandler ChangeIsSet;

      protected virtual void OnChangeIsSet(EventArgs e) {
         ChangeIsSet?.Invoke(this, e);
      }

      /// <summary>
      /// wird ausgelöst, wenn <see cref="GpxDataChanged"/> geändert wurde
      /// </summary>
      public event EventHandler ChangeIsChanged;

      protected virtual void OnChangeIsChanged(EventArgs e) {
         ChangeIsChanged?.Invoke(this, e);
      }


      public class TracklistChangedEventArgs : EventArgs {

         public enum Kind {
            Add,
            Remove,
            Move
         }

         /// <summary>
         /// Art der Änderung
         /// </summary>
         public Kind KindOfChanging { get; private set; }

         /// <summary>
         /// bei <see cref="Kind.Move"/> Ausgangspos.; 
         /// bei <see cref="Kind.Add"/> durch Split Pos. des alten <see cref="Track"/>; 
         /// bei <see cref="Kind.Remove"/> Pos. des <see cref="Track"/>
         /// </summary>
         public int From { get; private set; } = -1;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Zielpos.; 
         /// bei <see cref="Kind.Add"/> Pos. des neuen <see cref="Track"/>; 
         /// bei <see cref="Kind.Remove"/> nicht verwendet
         /// </summary>
         public int To { get; private set; } = -1;


         public TracklistChangedEventArgs(Kind kind, int fromidx, int toidx) {
            KindOfChanging = kind;
            From = fromidx;
            To = toidx;
         }

      }

      /// <summary>
      /// wird ausgelöst, wenn sich die Trackliste verändert hat
      /// </summary>
      public event EventHandler<TracklistChangedEventArgs> TracklistChanged;

      public virtual void OnTracklistChanged(TracklistChangedEventArgs e) {
         TracklistChanged?.Invoke(this, e);
      }


      public class MarkerlistChangedEventArgs : EventArgs {

         public enum Kind {
            Add,
            Remove,
            Move
         }

         /// <summary>
         /// Art der Änderung
         /// </summary>
         public Kind KindOfChanging { get; private set; }

         /// <summary>
         /// bei <see cref="Kind.Move"/> Ausgangspos.; 
         /// bei <see cref="Kind.Add"/> nicht verwendet; 
         /// bei <see cref="Kind.Remove"/> Pos. des <see cref="Marker"/> (wenn &lt; 0 ALLE <see cref="Marker"/>)
         /// </summary>
         public int From { get; private set; } = -1;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Zielpos.; 
         /// bei <see cref="Kind.Add"/> Pos. des neuen <see cref="Marker"/>  (wenn &lt; 0 ALLE <see cref="Marker"/>);
         /// bei <see cref="Kind.Remove"/> nicht verwendet
         /// </summary>
         public int To { get; private set; } = -1;


         public MarkerlistChangedEventArgs(Kind kind, int fromidx, int toidx) {
            KindOfChanging = kind;
            From = fromidx;
            To = toidx;
         }

      }

      /// <summary>
      /// wird ausgelöst, wenn sich die Waypointliste verändert hat
      /// </summary>
      public event EventHandler<MarkerlistChangedEventArgs> MarkerlistlistChanged;

      public virtual void OnMarkerlistChanged(MarkerlistChangedEventArgs e) {
         MarkerlistlistChanged?.Invoke(this, e);
      }

      public class LoadEventArgs {
         public string Info;

         public LoadEventArgs(string info) {
            Info = info;
         }
      }

      public event EventHandler<LoadEventArgs> LoadInfoEvent;

      #endregion

      /// <summary>
      /// Name der zugehörigen GPX-Datei (nur zur Info)
      /// </summary>
      public string GpxFilename { get; set; }

      /// <summary>
      /// Name der zugehörigen GPX-Bilder-Datei (nur zur Info)
      /// </summary>
      public string GpxPictureFilename { get; set; }

      /// <summary>
      /// Ist die Datei editierbar?
      /// </summary>
      public bool GpxFileEditable { get; set; } = false;

      bool _changed = false;
      /// <summary>
      /// Daten geändert? (nur zur Info)
      /// </summary>
      public bool GpxDataChanged {
         get => _changed;
         set {
            if (_changed != value) {
               _changed = value;
               OnChangeIsChanged(EventArgs.Empty);
            }
            OnChangeIsSet(EventArgs.Empty);
         }
      }

      bool _tracksVisible;
      /// <summary>
      /// Tracks (prinzipiell) anzeigbar?
      /// </summary>
      public bool TracksAreVisible {
         get => _tracksVisible;
         set {
            if (_tracksVisible != value) {
               _tracksVisible = value;
               visualRefreshTracks();
            }
         }
      }

      private bool _markersVisible;
      /// <summary>
      /// Marker  (prinzipiell) anzeigbar?
      /// </summary>
      public bool Markers4StandardAreVisible {
         get => _markersVisible;
         set {
            if (_markersVisible != value) {
               _markersVisible = value;
               visualRefreshMarkers();
            }
         }
      }

      private bool _picturesVisible;
      /// <summary>
      /// Zugehörige Bilder (prinzipiell) anzeigbar?
      /// </summary>
      public bool Markers4PicturesAreVisible {
         get => _picturesVisible;
         set {
            if (_picturesVisible != value) {
               _picturesVisible = value;
               visualRefreshPictureMarkers();
            }
         }
      }

      private Color lineColor;
      /// <summary>
      /// Linienfarbe (i.A. für alle <see cref="Track"/>)
      /// </summary>
      public Color TrackColor {
         get => lineColor;
         set {
            if (lineColor != value) {
               lineColor = value;
               foreach (Track r in TrackList)
                  r.LineColor = lineColor;
               visualRefreshTracks();
            }
         }
      }

      private double _lineWidth;
      /// <summary>
      /// Linienbreite (für alle <see cref="Track"/>)
      /// </summary>
      public double TrackWidth {
         get => _lineWidth;
         set {
            if (_lineWidth != value) {
               _lineWidth = value;
               foreach (Track r in TrackList)
                  r.LineWidth = _lineWidth;
               visualRefreshTracks();
            }
         }
      }

      /// <summary>
      /// alle Segmente (!) dieser XML-Datei als <see cref="Track"/>
      /// </summary>
      public List<Track> TrackList { get; private set; }

      /// <summary>
      /// alle <see cref="GpxWaypoint"/> dieser Datei als <see cref="Marker"/>
      /// </summary>
      public List<Marker> MarkerList { get; private set; }

      /// <summary>
      /// spez. Liste, die nur extern gefüllt wird (aus <see cref="GpxPictureFilename"/>)
      /// </summary>
      public List<Marker> MarkerListPictures { get; private set; }


      public GpxAllExt(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) {
         TrackList = new List<Track>();
         MarkerList = new List<Marker>();
         MarkerListPictures = new List<Marker>();
         TracksAreVisible = true;
         Markers4StandardAreVisible = true;
         Markers4PicturesAreVisible = true;
         TrackWidth = 1;
         TrackColor = Color.Black;

         if (!string.IsNullOrEmpty(xmltext))
            posImportXml();
      }

      void posImportXml() {
         postLoad();

         for (int i = 0; i < Tracks.Count; i++) {
            // falls individuelle Farben def. sind:
            foreach (var item in Tracks[i].UnhandledChildXml) {
               if (item.Value.StartsWith("<extensions>")) {
                  Color col = getGarminColor(item.Value);
                  if (col != Color.Empty)
                     TrackList[i].LineColor = col;
               }
            }
         }
      }

      #region Track einfügen oder entfernen

      /// <summary>
      /// liefert den Listenindex des <see cref="Track"/> in <see cref="TrackList"/>
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public int TrackIndex(Track track) {
         return TrackList.IndexOf(track);
      }

      /// <summary>
      /// fügt eine Kopie der <see cref="Track"/> in den akt. Container als <see cref="GpxTrack"/> und 
      /// als <see cref="Track"/> in <see cref="TrackList"/> ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <param name="useorgprops">bei false wird die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public Track TrackInsertCopy(Track orgtrack, int pos = -1, bool useorgprops = false) {
         Track track = Track.CreateCopy(orgtrack, this, useorgprops);
         track.GpxTrack.Name = GetUniqueTrackname(track.GpxTrack.Name);
         if (track.VisualName != track.GpxTrack.Name)
            track.VisualName = track.GpxTrack.Name;

         if (pos < 0 || TrackList.Count <= pos) {
            TrackList.Add(track);
            Tracks.Add(track.GpxTrack);
            pos = TrackList.Count - 1;
         } else {
            TrackList.Insert(pos, track);
            Tracks.Insert(pos, track.GpxTrack);
         }
         GpxDataChanged = true;
         OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Add, -1, pos));
         return track;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> aus <see cref="TrackList"/> und den dazugehörigen <see cref="GpxTrack"/> 
      /// an der angegebenen Position
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Track TrackRemove(int pos) {
         Track tracksegment = null;
         if (0 <= pos && pos < TrackList.Count) {
            tracksegment = TrackList[pos];
            if (tracksegment.GpxTrack != null &&
                tracksegment.GpxSegment != null) {
               tracksegment.GpxTrack.Segments.Remove(tracksegment.GpxSegment);

               if (tracksegment.GpxTrack.Segments.Count == 0)
                  Tracks.Remove(tracksegment.GpxTrack);

               TrackList.Remove(tracksegment);

               GpxDataChanged = true;
               OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Remove, pos, -1));
            }
         }
         return tracksegment;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> aus <see cref="TrackList"/> und den dazugehörigen <see cref="GpxTrack"/> 
      /// </summary>
      /// <param name="tracksegment"></param>
      /// <returns></returns>
      public Track TrackRemove(Track tracksegment) {
         return TrackRemove(TrackIndex(tracksegment));
      }

      /// <summary>
      /// entfernt alle <see cref="Track"/> aus <see cref="TrackList"/> und die dazugehörigen <see cref="GpxTrack"/> 
      /// </summary>
      public void TrackRemoveAll() {
         while (TrackList.Count > 0)
            TrackRemove(0);
      }

      /// <summary>
      /// trennt den vorhandenen <see cref="Track"/> (der erste <see cref="Track"/> wird nur gekürzt, der zweite hinter 
      /// dem 1. in der <see cref="TrackList"/> eingefügt) und passt auch die dazugehörigen <see cref="GpxTrack"/> an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="splitptidx"></param>
      /// <returns>neuer <see cref="Track"/></returns>
      public Track TrackSplit(Track track, int splitptidx) {
         return TrackSplit(TrackList.IndexOf(track), splitptidx);
      }

      /// <summary>
      /// trennt den vorhandenen <see cref="Track"/> (der erste <see cref="Track"/> wird nur gekürzt, der zweite hinter 
      /// dem 1. in der <see cref="TrackList"/> eingefügt) und passt auch die dazugehörigen <see cref="GpxTrack"/> an
      /// </summary>
      /// <param name="trackidx"></param>
      /// <param name="splitptidx"></param>
      /// <returns>neuer <see cref="Track"/></returns>
      public Track TrackSplit(int trackidx, int splitptidx) {
         if (0 <= trackidx && trackidx < Tracks.Count) {
            GpxTrack orgtrack = Tracks[trackidx];
            if (0 < splitptidx && splitptidx < orgtrack.Segments[0].Points.Count - 1) {
               GpxTrack trackcopy = new GpxTrack(orgtrack);  // Kopie erzeugen

               // alten Track kürzen und Statistik neu ermitteln
               orgtrack.Segments[0].Points.RemoveRange(splitptidx + 1, orgtrack.Segments[0].Points.Count - splitptidx - 1);
               TrackList[trackidx].CalculateStats();

               trackcopy.Name = GetUniqueTrackname(trackcopy.Name);
               trackcopy.Segments[0].Points.RemoveRange(0, splitptidx);
               Tracks.Add(trackcopy);  // erstmal nur an GpxTrack-Liste anhängen

               Track newtrack = createTrackFromSegment(Tracks.Count - 1, 0);
               newtrack.CalculateStats();
               TrackList.Add(newtrack);   // erstmal nur an Track-Liste anhängen
               GpxDataChanged = true;
               OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Add, trackidx, TrackList.Count - 1));

               TrackOrderChange(TrackList.Count - 1, trackidx + 1);  // direkt hinter den Originaltrack schieben

               return newtrack;
            }
         }
         return null;
      }

      /// <summary>
      /// <see cref="Track"/> 2 wird an <see cref="Track"/> 1 angehängt und die dazugehörigen <see cref="GpxTrack"/> angepasst
      /// </summary>
      /// <param name="track1"></param>
      /// <param name="track2"></param>
      /// <returns><see cref="Track"/> 1</returns>
      public Track TrackConcat(Track track1, Track track2) {
         return TrackConcat(TrackList.IndexOf(track1),
                            TrackList.IndexOf(track2));
      }

      /// <summary>
      /// <see cref="Track"/> 2 wird an <see cref="Track"/> 1 angehängt und die dazugehörigen <see cref="GpxTrack"/> angepasst
      /// </summary>
      /// <param name="track1idx"></param>
      /// <param name="track2idx"></param>
      /// <returns>Track 1</returns>
      public Track TrackConcat(int track1idx, int track2idx) {
         if (0 <= track1idx && track1idx < Tracks.Count &&
             0 <= track2idx && track2idx < Tracks.Count &&
             track1idx != track2idx) {
            Track t1 = TrackList[track1idx];
            Track t2 = TrackList[track2idx];

            if (t1.GpxSegment.Points.Count > 0 &&
                t2.GpxSegment.Points.Count > 0) {
               GpxTrackPoint lastPt = t1.GpxSegment.Points[t1.GpxSegment.Points.Count - 1];
               GpxTrackPoint firstPt = t2.GpxSegment.Points[0];
               if (lastPt.AsXml(9) == firstPt.AsXml(9))  // Punkte sind völlig identisch
                  t1.GpxSegment.Points.Remove(lastPt);
            }

            t1.GpxSegment.Points.AddRange(t2.GpxSegment.Points);
            t1.CalculateStats();
            TrackRemove(track2idx);
            return t1;
         }
         return null;
      }

      /// <summary>
      /// falls der Name für die <see cref="Track"/> der <see cref="TrackList"/> nicht eindeutig ist, wird so lange
      /// " *" angehängt bis der Name eindeutig ist und geliefert
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public string GetUniqueTrackname(string name) {
         bool found = true;
         while (found) {
            found = false;
            foreach (var item in TrackList) {
               if (name == item.Trackname) {
                  found = true;
                  break;
               }
            }
            if (found)
               name += " *";
         }
         return name;
      }

      /// <summary>
      /// liefert einen neuen <see cref="Track"/> für dieses Segment
      /// </summary>
      /// <param name="trackno"></param>
      /// <param name="segmentno"></param>
      /// <returns></returns>
      Track createTrackFromSegment(int trackno, int segmentno) {
         Track track = Track.Create(this, trackno, segmentno, Tracks[trackno].Name);
         track.LineColor = TrackColor;
         track.LineWidth = TrackWidth;
         return track;
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Track"/> in der Auflistung und in <see cref="GpxAll.Tracks"/>
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx">Index NACH dem temp. Entfernen des Markers!</param>
      /// <returns></returns>
      public bool TrackOrderChange(int fromidx, int toidx) {
         if (fromidx != toidx &&
             0 <= fromidx && fromidx < TrackList.Count &&
             0 <= toidx && toidx < TrackList.Count) {

            Track track = TrackList[fromidx];
            TrackList.RemoveAt(fromidx);
            TrackList.Insert(toidx, track);

            GpxTrack gpxtrack = Tracks[fromidx];
            Tracks.RemoveAt(fromidx);
            Tracks.Insert(toidx, gpxtrack);

            GpxDataChanged = true;
            OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Move, fromidx, toidx));

            return true;
         }
         return false;
      }

      #endregion

      #region Marker einfügen oder entfernen

      /// <summary>
      /// liefert den Listenindex für diesen <see cref="Marker"/> in <see cref="MarkerList"/> bzw. <see cref="MarkerListPictures"/>
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public int MarkerIndex(Marker marker) {
         switch (marker.Markertype) {
            case Marker.MarkerType.Standard:
            case Marker.MarkerType.EditableStandard:
               return MarkerList.IndexOf(marker);

            case Marker.MarkerType.Foto:
               return MarkerListPictures.IndexOf(marker);

            case Marker.MarkerType.GeoTagging:
               return -1;

            default:
               throw new Exception("Unknown MarkerType");
         }
      }

      /// <summary>
      /// fügt eine Kopie des <see cref="Marker"/> an den akt. Container an (oder ein)
      /// </summary>
      /// <param name="orgwp"></param>
      /// <returns></returns>
      public Marker MarkerInsertCopy(Marker orgmarker, int pos = -1, Marker.MarkerType markertype = Marker.MarkerType.Standard) {
         GpxWaypoint wp = new GpxWaypoint(orgmarker.Waypoint);
         Marker marker = Marker.Create(this, wp, markertype);

         if (pos < 0 || Waypoints.Count <= pos) {
            Waypoints.Add(wp);
            MarkerList.Add(marker);
            pos = Waypoints.Count - 1;
         } else {
            Waypoints.Insert(pos, wp);
            MarkerList.Insert(pos, marker);
         }

         GpxDataChanged = true;
         OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Add, -1, pos));
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> an der angegebenen Position aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="pos"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker MarkerRemove(int pos) {
         Marker marker = null;
         if (0 <= pos && pos < Waypoints.Count) {
            Waypoints.RemoveAt(pos);
            marker = MarkerList[pos];
            MarkerList.RemoveAt(pos);
            GpxDataChanged = true;
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Remove, pos, -1));
         }
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker MarkerRemove(Marker marker) {
         return MarkerRemove(MarkerIndex(marker));
      }

      /// <summary>
      /// alle <see cref="Marker"/> aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/> entfernen
      /// </summary>
      public void MarkerRemoveAll() {
         while (MarkerList.Count > 0)
            MarkerRemove(0);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> in <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      /// <returns></returns>
      public bool MarkerOrderChange(int fromidx, int toidx) {
         if (fromidx != toidx &&
             0 <= fromidx && fromidx < Waypoints.Count &&
             0 <= toidx && toidx < Waypoints.Count) {

            GpxWaypoint wp = Waypoints[fromidx];
            Waypoints.RemoveAt(fromidx);
            Waypoints.Insert(toidx, wp);

            Marker marker = MarkerList[fromidx];
            MarkerList.RemoveAt(fromidx);
            MarkerList.Insert(toidx, marker);

            GpxDataChanged = true;
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Move, fromidx, toidx));

            return true;
         }
         return false;
      }

      #endregion

      #region erneutes Zeichnen der Objekte auslösen

      /// <summary>
      /// Anzeige akt. (falls akt. sichtbar)
      /// </summary>
      public void VisualRefresh() {
         visualRefreshTracks();
         visualRefreshMarkers();
         visualRefreshPictureMarkers();
      }

      void visualRefreshTracks() {
         if (TracksAreVisible)
            foreach (Track r in TrackList)
               r.Refresh();
      }

      void visualRefreshMarkers() {
         if (Markers4StandardAreVisible)
            foreach (Marker m in MarkerList)
               m.Refresh();
      }

      void visualRefreshPictureMarkers() {
         if (Markers4PicturesAreVisible)
            foreach (Marker m in MarkerListPictures)
               m.Refresh();
      }

      #endregion

      /// <summary>
      /// liefert den nächsten sichtbaren <see cref="Track"/> in <see cref="TrackList"/> oder null
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public Track NextVisibleTrack(Track track) {
         int idx = TrackList.IndexOf(track);
         if (idx >= 0) {
            for (int i = idx - 1; i >= 0; i--)
               if (TrackList[i].IsVisible)
                  return TrackList[i];
         }
         return null;
      }

      /// <summary>
      /// liefert den nächsten sichtbaren <see cref="Marker"/> in <see cref="MarkerList"/> oder null
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      public Marker NextVisibleMarker(Marker marker) {
         int idx = MarkerList.IndexOf(marker);
         if (idx >= 0) {
            for (int i = idx - 1; i >= 0; i--)
               if (MarkerList[i].IsVisible &&
                   MarkerList[i].IsEditable)
                  return MarkerList[i];
         }
         return null;
      }

      /// <summary>
      /// Daten als Datei abspeichern
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="creator"></param>
      /// <param name="withgarminext"></param>
      /// <param name="colorname">diese Farbe wird NICHT gespeichert</param>
      /// <param name="gpxversion"></param>
      public void Save(string filename,
                       string creator,
                       bool withgarminext,
                       GarminTrackColors.Colorname colorname = GarminTrackColors.Colorname.Transparent,
                       string gpxversion = "1.1") {
         RebuildMetadataBounds();
         Metadata.Time = DateTime.Now;

         if (withgarminext) {
            // Trackfarben
            for (int i = 0; i < TrackList.Count; i++) {
               GarminTrackColors.Colorname col = GarminTrackColors.GetColorname(TrackList[i].LineColor, true);
               if (col != GarminTrackColors.Colorname.Unknown &&
                   col != GarminTrackColors.Colorname.Transparent &&
                   col != colorname) {    // Farbe setzen
                  int key = -1;
                  string xmlextension = null;
                  foreach (var item in Tracks[i].UnhandledChildXml) {
                     if (item.Value.StartsWith("<extensions>")) {
                        key = item.Key;
                        xmlextension = item.Value;
                        break;
                     }
                  }
                  xmlextension = setGarminColor(xmlextension, col);
                  if (key < 0) {
                     key = Tracks[i].UnhandledChildXml.Count > 0 ?
                              Tracks[i].UnhandledChildXml.Keys.Max() + 1 :    // an die UnhandledChild anhängen
                              1;                                              // 1: davor gibt es nur 1 Child <name>
                     Tracks[i].UnhandledChildXml.Add(key, xmlextension);
                  } else
                     Tracks[i].UnhandledChildXml[key] = xmlextension;
               } else {       // ev. vorhandene Farbe aus den UnhandledChildXml entfernen
                  int key = -1;
                  foreach (var item in Tracks[i].UnhandledChildXml) {
                     /* z.B.:
                              <extensions>
                                <gpxx:TrackExtension xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3">
                                  <gpxx:DisplayColor>DarkGray</gpxx:DisplayColor>
                                </gpxx:TrackExtension>
                              </extensions>
                      */
                     if (item.Value.StartsWith("<extensions>") &&
                         item.Value.Contains("<gpxx:TrackExtension xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">") &&
                         item.Value.Contains("<gpxx:DisplayColor>")) {
                        key = item.Key;
                        break;
                     }
                  }
                  if (0 <= key)
                     Tracks[i].UnhandledChildXml.Remove(key);
               }
            }

            // Markertypen
            for (int i = 0; i < Waypoints.Count; i++) {
               if (!string.IsNullOrEmpty(Waypoints[i].Symbol)) {
                  int key = -1;
                  string xmlextension = null;
                  foreach (var item in Waypoints[i].UnhandledChildXml) {
                     if (item.Value.StartsWith("<extensions>")) {
                        key = item.Key;
                        xmlextension = item.Value;
                        break;
                     }
                  }

                  xmlextension = setGarminSymbolExt(xmlextension);
                  if (key < 0) {
                     key = Waypoints[i].UnhandledChildXml.Count > 0 ?
                              Waypoints[i].UnhandledChildXml.Keys.Max() + 1 :    // an die UnhandledChild anhängen
                              9;                                                 // 9: nur sichergehen, dass sie NACH den bekannten Childs kommen
                     Waypoints[i].UnhandledChildXml.Add(key, xmlextension);
                  } else
                     Waypoints[i].UnhandledChildXml[key] = xmlextension;
               }
            }
         }

         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx") {

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
            sb.Append("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\"");
            sb.Append(" creator=\"" + creator + "\" version = \"" + gpxversion + "\"");
            sb.Append(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
            sb.Append(" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");
            sb.Append(AsXml(9).Substring(5));   // ohne führendes '<gpx>'

            File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);

         } else if (ext == ".kml" || ext == ".kmz") {

            List<Color> kmlcol = new List<Color>();
            List<uint> kmlwidth = new List<uint>();
            for (int i = 0; i < TrackList.Count; i++) {
               kmlcol.Add(TrackList[i].LineColor);
               kmlwidth.Add(1);
            }

            new FSofTUtils.Geography.KmlWriter().Write_gdal(filename, this, true, kmlcol, kmlwidth);
         }
      }

      /// <summary>
      /// Daten aus der Datei lesen
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="removenamespace"></param>
      public void Load(string filename,
                       bool removenamespace = false) {
         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx") {

            sendLoadInfo("read XML");
            FromXml(File.ReadAllText(filename), removenamespace);
            posImportXml();

         } else if (ext == ".gdb") {

            sendLoadInfo("read GDB");
            List<GDB.Object> objlst = GDB.ReadGDBObjectList(filename);
            long dtunix = new DateTime(1970, 1, 1).Ticks;
            foreach (GDB.Object obj in objlst) {
               switch (obj.ObjectHeader.ObjectType) {
                  case GDB.ObjectHeader.GDBObjectType.WAYPOINT:
                     GDB.Waypoint wp = obj as GDB.Waypoint;
                     GpxWaypoint waypoint = new GpxWaypoint(wp.Lon, wp.Lat) {
                        Elevation = wp.Ele == double.MinValue ? double.MinValue : wp.Ele,
                        Name = wp.Name,
                        Description = wp.Description,
                        Time = wp.CreationTime,
                        Symbol = GDB.GetIconName4Symbolnumber(wp.IconIdx),
                     };
                     if (wp.IconIdx > 0) {
                        string name = GDB.GetIconName4Symbolnumber(wp.IconIdx);
                        if (!string.IsNullOrEmpty(name))
                           waypoint.Symbol = name;
                     }
                     InsertWaypoint(waypoint);
                     break;

                  case GDB.ObjectHeader.GDBObjectType.TRACK:
                     GDB.Track track = obj as GDB.Track;
                     GpxTrackSegment gpxsegment = new GpxTrackSegment();
                     foreach (GDB.TrackPoint pt in track.Points) {
                        if (pt.DateTime.Ticks > dtunix)
                           gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele,
                                                  pt.DateTime));
                        else
                           gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele));
                     }
                     GpxTrack gpxtrack = new GpxTrack() {
                        Name = track.Name,
                     };
                     gpxtrack.InsertSegment(gpxsegment);
                     InsertTrack(gpxtrack);
                     break;

                     //case GDB.ObjectHeader.GDBObjectType.ROUTE:

                     //   break;
               }
            }
            postLoad();

         } else if (ext == ".kml" || ext == ".kmz") {

            sendLoadInfo("read KML/KMZ");
            GpxAll gpx4kml = new FSofTUtils.Geography.KmlReader().Read(filename, out List<Color> colors);

            sendLoadInfo("InsertWaypoints");
            foreach (var wp in gpx4kml.Waypoints)
               InsertWaypoint(wp);

            sendLoadInfo("InsertTracks");
            foreach (var track in gpx4kml.Tracks)
               InsertTrack(track);

            postLoad();

            for (int i = 0; i < TrackList.Count; i++)
               if (colors[i] != Color.Transparent)
                  TrackList[i].LineColor = colors[i];
         }
      }

      /// <summary>
      /// testet, ob ein rechteckiges Testgebiet von den Daten der GPX-Datei betroffen ist
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="testarea"></param>
      /// <param name="withwaypoints"></param>
      /// <param name="isroutecrossing"></param>
      /// <returns></returns>
      public bool CheckAreaFromFile(string filename,
                                    GpxBounds testarea,
                                    bool withwaypoints,
                                    Func<GpxBounds, IList<GpxTrackPoint>, bool> isroutecrossing) {
         // zuerst nur ev. vorhandene Metadaten testen
         string xmltxt;
         using (StreamReader sr = new StreamReader(filename)) {
            char[] txtbuff = new char[1000];
            int chars = sr.ReadBlock(txtbuff, 0, txtbuff.Length);
            xmltxt = RemoveNamespace(new string(txtbuff));

            int start = xmltxt.IndexOf("<" + GpxMetadata1_1.NODENAME + ">");
            if (start >= 0) {
               int end = xmltxt.IndexOf("</" + GpxMetadata1_1.NODENAME + ">");
               if (end >= 0) {
                  string metadatatxt = xmltxt.Substring(start, end - start + 3 + GpxMetadata1_1.NODENAME.Length);
                  XPathNavigator navm = GetNavigator4XmlText(metadatatxt);
                  string[] tmpm = XReadOuterXml(navm, "/" + GpxMetadata1_1.NODENAME);
                  if (tmpm != null) {
                     GpxMetadata1_1 metadata = new GpxMetadata1_1(tmpm[0]);
                     if (metadata.Bounds.MinLon != metadata.Bounds.MaxLon &&
                         metadata.Bounds.MinLat != metadata.Bounds.MaxLat) {
                        if (!metadata.Bounds.IntersectsWith(testarea))
                           return false;
                     }
                  }
               }
            }
         }

         // vollständige Datei testen
         GpxBounds bounds;
         string[] tmp;
         XPathNavigator nav = GetNavigator4XmlText(RemoveNamespace(System.IO.File.ReadAllText(filename)));

         if (withwaypoints) {
            tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxWaypoint.NODENAME);
            if (tmp != null) {
               // Lage aller Waypoints testen
               for (int w = 0; w < tmp.Length; w++) {
                  GpxWaypoint wp = new GpxWaypoint(tmp[w]);
                  if (testarea.IntersectsWith(new GpxBounds(wp.Lat, wp.Lat, wp.Lon, wp.Lon)))
                     return true;
               }
            }
         }

         //Routes = new List<GpxRoute>();
         //tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxRoute.NODENAME);
         //if (tmp != null) {
         //   for (int r = 0; r < tmp.Length; r++)
         //      Routes.Add(new GpxRoute(tmp[r]));
         //}

         tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxTrack.NODENAME);
         if (tmp != null) {
            for (int t = 0; t < tmp.Length; t++) {
               GpxTrack track = new GpxTrack(tmp[t]);
               for (int i = 0; i < track.Segments.Count; i++) {
                  List<GpxTrackPoint> pts = track.GetSegment(i).Points;
                  bounds = new GpxBounds();
                  bounds.Union(pts);
                  if (isroutecrossing == null) {
                     if (testarea.IntersectsWith(bounds))
                        return true;
                  } else {
                     if (isroutecrossing(testarea, pts))
                        return true;
                  }
               }
            }
         }

         return false;
      }

      void sendLoadInfo(string txt) => LoadInfoEvent?.Invoke(this, new LoadEventArgs(txt));

      /// <summary>
      /// ev. notwendige Aufbereitung der GPX-Daten
      /// </summary>
      void postLoad() {
         sendLoadInfo("splitMultiSegmentTracks");
         splitMultiSegmentTracks();
         sendLoadInfo("removeEmptyTracks");
         removeEmptyTracks();
         sendLoadInfo("rebuildTrackList");
         rebuildTrackList();
         sendLoadInfo("rebuildMarkerList");
         rebuildMarkerList();
      }

      /// <summary>
      /// aus Tracks mit mehreren Segmenten werden Tracks mit nur einem Segment
      /// </summary>
      void splitMultiSegmentTracks() {
         for (int t = Tracks.Count - 1; t >= 0; t--) {
            GpxTrack track = Tracks[t];
            while (track.Segments.Count > 1) {
               GpxTrackSegment segment = track.Segments[1];
               if (segment.Points.Count > 0) {
                  GpxTrack newtrack = new GpxTrack() { Name = track.Name, };
                  newtrack.Segments.Add(segment);
                  Tracks.Insert(t + 1, newtrack);
               }
               track.Segments.RemoveAt(1);
            }
         }
      }

      /// <summary>
      /// Tracks ohne Punkte werden entfernt
      /// </summary>
      void removeEmptyTracks() {
         for (int t = Tracks.Count - 1; t >= 0; t--) {
            GpxTrack track = Tracks[t];
            for (int s = track.Segments.Count - 1; s >= 0; s--) {
               GpxTrackSegment segment = track.Segments[s];
               if (segment.Points.Count == 0)
                  track.Segments.RemoveAt(s);
            }
            if (track.Segments.Count == 0)
               Tracks.RemoveAt(t);
         }
      }

      /// <summary>
      /// die <see cref="TrackList"/> wird neu mit neu erzeugten <see cref="Track"/> aus den <see cref="GpxAll.Tracks"/> gebildet
      /// </summary>
      /// <returns></returns>
      List<Track> rebuildTrackList() {
         TrackList.Clear();
         OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Remove, -1, -1));
         for (int t = 0; t < Tracks.Count; t++)
            for (int s = 0; s < Tracks[t].Segments.Count; s++)
               TrackList.Add(createTrackFromSegment(t, s));
         if (TrackList.Count > 0)
            OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Add, -1, -1));
         return TrackList;
      }

      /// <summary>
      /// die <see cref="MarkerList"/> wird neu mit neu erzeugten <see cref="Marker"/> aus den <see cref="GpxAll.Waypoints"/> gebildet
      /// </summary>
      /// <returns></returns>
      List<Marker> rebuildMarkerList() {
         MarkerList.Clear();
         OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Remove, -1, -1));
         for (int m = 0; m < Waypoints.Count; m++)
            MarkerList.Add(Marker.Create(this, m, GpxFileEditable ? Marker.MarkerType.EditableStandard : Marker.MarkerType.Standard));
         if (MarkerList.Count > 0)
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Add, -1, -1));
         return MarkerList;
      }

      #region Garmin-Farben und -Symbole

      /* Trackfarbe bei Garmin:

           <trk>
             <name>Track</name>
             <extensions>
               <gpxx:TrackExtension xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3">
                 <gpxx:DisplayColor>DarkRed</gpxx:DisplayColor>
               </gpxx:TrackExtension>
             </extensions>
             <trkseg>
               ...
             </trkseg>
           </trk>
       */

      /// <summary>
      /// etwas getrickst, um eine Garminfarbe zu lesen
      /// </summary>
      /// <param name="xmlextension"></param>
      /// <returns>Color.Empty wenn keine Farbe gelesen wurde</returns>
      Color getGarminColor(string xmlextension) {
         if (xmlextension.StartsWith("<extensions>")) {
            xmlextension = xmlextension.Replace("<gpxx:", "<");
            xmlextension = xmlextension.Replace("</gpxx:", "</");
            xmlextension = xmlextension.Replace(" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\"", "");

            XPathNavigator nav = GetNavigator4XmlText(xmlextension);
            string colortext = XReadString(nav, "/extensions/TrackExtension/DisplayColor");
            return string.IsNullOrEmpty(colortext) ?
                              Color.Empty :
                              GarminTrackColors.Colors[GarminTrackColors.GetColorname(colortext)];
         }
         return Color.Empty;
      }

      /// <summary>
      /// etwas getrickst, um eine Garminfarbe schreiben zu können
      /// </summary>
      /// <param name="xmlextension"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      string setGarminColor(string xmlextension, GarminTrackColors.Colorname col) {
         if (col != GarminTrackColors.Colorname.Unknown)
            if (string.IsNullOrEmpty(xmlextension)) {
               xmlextension = "<extensions><gpxx:TrackExtension xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\"><gpxx:DisplayColor>";
               xmlextension += GarminTrackColors.GetColorname(col);
               xmlextension += "</gpxx:DisplayColor></gpxx:TrackExtension></extensions>";
            } else {
               if (xmlextension.StartsWith("<extensions>")) {
                  int start = xmlextension.IndexOf("<gpxx:DisplayColor>");
                  if (start >= 0) {
                     start += 19;   // Länge von "<gpxx:DisplayColor>"
                     int end = xmlextension.IndexOf("</gpxx:DisplayColor>", start);
                     if (start < end) {
                        xmlextension = xmlextension.Substring(0, start) +
                                       GarminTrackColors.GetColorname(col) +
                                       xmlextension.Substring(end);
                     }
                  } else {
                     start = xmlextension.IndexOf("<extensions><gpxx:TrackExtension");
                     if (start >= 0) {
                        start = xmlextension.IndexOf("<", start + 1);
                        xmlextension = xmlextension.Substring(0, start) +
                                       "<gpxx:DisplayColor>" + GarminTrackColors.GetColorname(col) + "</gpxx:DisplayColor>" +
                                       xmlextension.Substring(start);
                     } else {
                        start = xmlextension.IndexOf("<", 1);
                        xmlextension = xmlextension.Substring(0, start) +
                                       "<extensions><gpxx:TrackExtension xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                                       "<gpxx:DisplayColor>" + GarminTrackColors.GetColorname(col) + "</gpxx:DisplayColor>" +
                                       "</gpxx:TrackExtension></extensions>" +
                                       xmlextension.Substring(start);
                     }
                  }
               }
            }
         return xmlextension;
      }

      /* <wpt>
            <sym>Flag, Red</sym>
            <extensions>
              <gpxx:WaypointExtension xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3">
                <gpxx:DisplayMode>SymbolAndName</gpxx:DisplayMode>
              </gpxx:WaypointExtension>
            </extensions>
      */

      string setGarminSymbolExt(string xmlextension) {
         if (string.IsNullOrEmpty(xmlextension)) {
            xmlextension += "<extensions><gpxx:WaypointExtension xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\"><gpxx:DisplayMode>";
            xmlextension += "SymbolAndName";
            xmlextension += "</gpxx:DisplayMode></gpxx:WaypointExtension></extensions>";
         } else {
            if (xmlextension.StartsWith("<extensions>")) {
               int start = xmlextension.IndexOf("<gpxx:DisplayMode>");
               if (start >= 0) {
                  start = xmlextension.IndexOf("<", start + 1);
                  int end = xmlextension.IndexOf("</gpxx:DisplayMode>", start + 1);
                  if (start < end) {
                     xmlextension = xmlextension.Substring(0, start) +
                                    "SymbolAndName" +
                                    xmlextension.Substring(end);
                  }
               } else {
                  start = xmlextension.IndexOf("<extensions><gpxx:WaypointExtension");
                  if (start >= 0) {
                     start = xmlextension.IndexOf("<", start + 1);
                     xmlextension = xmlextension.Substring(0, start) +
                                    "<gpxx:DisplayMode>SymbolAndName</gpxx:DisplayMode>" +
                                    xmlextension.Substring(start);
                  } else {
                     start = xmlextension.IndexOf("<", 1);
                     xmlextension = xmlextension.Substring(0, start) +
                                    "<extensions><gpxx:WaypointExtension xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\">" +
                                    "<gpxx:DisplayMode>SymbolAndName</gpxx:DisplayMode>" +
                                    "</gpxx:WaypointExtension></extensions>" +
                                    xmlextension.Substring(start);
                  }
               }
            }
         }


         return xmlextension;
      }

      #endregion

      public override string ToString() {
         return string.Format("[{0} ExtTracks, {1} MarkerList, {2} MarkerListPictures, LineWidth={3}, LineColor={4}]",
                              TrackList.Count,
                              MarkerList.Count,
                              MarkerListPictures.Count,
                              TrackWidth,
                              TrackColor.ToString());
      }

   }
}
