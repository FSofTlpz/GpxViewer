using FSofTUtils.Geography.Garmin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SmallMapControl {
   public class PoorGpxAllExt : Gpx.GpxAll {

      #region Events

      /// <summary>
      /// wird ausgelöst, wenn <see cref="GpxFileChanged"/> geändert wurde
      /// </summary>
      public event EventHandler ChangeIsSet;

      protected virtual void OnChangeIsSet(EventArgs e) {
         ChangeIsSet?.Invoke(this, e);
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
         /// bei <see cref="Kind.Move"/> Ausgangspos.; bei <see cref="Kind.Add"/> nicht verwendet; bei <see cref="Kind.Remove"/> Pos. des <see cref="Marker"/>
         /// </summary>
         public int From { get; private set; } = -1;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Zielpos.; bei <see cref="Kind.Add"/> Pos. des neuen <see cref="Marker"/>; bei <see cref="Kind.Remove"/> nicht verwendet
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
         /// bei <see cref="Kind.Move"/> Ausgangspos.; bei <see cref="Kind.Add"/> durch Split Pos. des alten <see cref="Track"/>; bei <see cref="Kind.Remove"/> Pos. des <see cref="Track"/>
         /// </summary>
         public int From { get; private set; } = -1;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Zielpos.; bei <see cref="Kind.Add"/> Pos. des neuen <see cref="Track"/>; bei <see cref="Kind.Remove"/> nicht verwendet
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

      #endregion

      public PoorGpxAllExt ParentGpx { get; set; }

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
      public bool GpxFileChanged {
         get {
            return _changed;
         }
         set {
            if (_changed != value) {
               _changed = value;
               OnChangeIsSet(EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// Tracksegmente anzeigen?
      /// </summary>
      public bool TrackSegsVisible { get; protected set; }

      private bool _markersVisible;
      /// <summary>
      /// Marker anzeigen?
      /// </summary>
      public bool Markers4StandardVisible {
         get => _markersVisible;
         set {
            if (_markersVisible != value) {
               _markersVisible = value;
               if (value && Waypoints.Count == 0)
                  return;
               VisualRefreshMarkers();
            }
         }
      }

      private bool _picturesVisible;
      /// <summary>
      /// Zugehörige Bilder anzeigen?
      /// </summary>
      public bool Markers4PicturesVisible {
         get => _picturesVisible;
         set {
            if (_picturesVisible != value) {
               _picturesVisible = value;
               if (value && MarkerListPictures.Count == 0)
                  return;
               VisualRefreshPictureMarkers();
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
               VisualRefreshTrackSegs();
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
               VisualRefreshTrackSegs();
            }
         }
      }

      /// <summary>
      /// alle Segmente dieser XML-Datei als <see cref="Track"/>
      /// </summary>
      public List<Track> TrackList { get; private set; }

      public List<Marker> MarkerList { get; private set; }

      /// <summary>
      /// spez. Liste, die nur extern gefüllt wird (aus <see cref="GpxPictureFilename"/>)
      /// </summary>
      public List<Marker> MarkerListPictures { get; private set; }


      public PoorGpxAllExt(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) {
         ParentGpx = null;
         TrackList = new List<Track>();
         MarkerList = new List<Marker>();
         MarkerListPictures = new List<Marker>();
         TrackSegsVisible = false;
         Markers4StandardVisible = false;
         Markers4PicturesVisible = false;
         TrackWidth = 1;
         TrackColor = Color.Black;
      }

      #region Track-Segment als Track einfügen oder entfernen

      /// <summary>
      /// liefert den Listenindex des Tracks
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public int TrackIndex(Track track) {
         return TrackList.IndexOf(track);
      }

      /// <summary>
      /// fügt eine Kopie der <see cref="Track"/> an den akt. Container an (oder ein)
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="newroutetype"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Track TrackInsertCopy(Track orgtrack, int pos = -1) {
         Track track = Track.CreateCopy(orgtrack, this);
         track.GpxTrack.Name = getUniqueTrackname(track.GpxTrack.Name);
         if (track.VisualName != track.GpxTrack.Name)
            track.SetVisualname(track.GpxTrack.Name);

         if (pos < 0 || TrackList.Count <= pos) {
            TrackList.Add(track);
            Tracks.Add(track.GpxTrack);
            pos = TrackList.Count - 1;
         } else {
            TrackList.Insert(pos, track);
            Tracks.Insert(pos, track.GpxTrack);
         }
         GpxFileChanged = true;
         OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Add, -1, pos));
         return track;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> (und seine Gpx-Daten) an der angegebenen Position
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

               GpxFileChanged = true;
               OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Remove, pos, -1));
            }
         }
         return tracksegment;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> (und seine Gpx-Daten)
      /// </summary>
      /// <param name="tracksegment"></param>
      /// <returns></returns>
      public Track TrackRemove(Track tracksegment) {
         return TrackRemove(TrackIndex(tracksegment));
      }

      /// <summary>
      /// trennt den vorhandenen Track (der erste Track wird nur gekürzt, der zweite an die Liste angehängt)
      /// </summary>
      /// <param name="track"></param>
      /// <param name="splitptidx"></param>
      /// <returns>true, wenn geteilt</returns>
      public bool TrackSplit(Track track, int splitptidx) {
         int pos = TrackList.IndexOf(track);
         if (pos >= 0) {
            Gpx.GpxTrack orgtrack = Tracks[pos];
            if (0 < splitptidx && splitptidx < orgtrack.Segments[0].Points.Count - 1) {
               Gpx.GpxTrack newtrack = new Gpx.GpxTrack(orgtrack);
               newtrack.Name = getUniqueTrackname(newtrack.Name);
               orgtrack.Segments[0].Points.RemoveRange(splitptidx + 1, orgtrack.Segments[0].Points.Count - splitptidx - 1);
               newtrack.Segments[0].Points.RemoveRange(0, splitptidx);
               Tracks.Add(newtrack);

               TrackList[pos].CalculateStats();

               TrackList.Add(createTrackFromSegment(Tracks.Count - 1, 0));
               TrackList[TrackList.Count - 1].CalculateStats();
               GpxFileChanged = true;
               OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Add, pos, TrackList.Count - 1));
               return true;
            }
         }
         return false;
      }

      string getUniqueTrackname(string name) {
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
      /// liefert einen neuen Track für dieses Tracksegment
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
      /// verschiebt die Position des Tracks in der Auflistung
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

            Gpx.GpxTrack gpxtrack = Tracks[fromidx];
            Tracks.RemoveAt(fromidx);
            Tracks.Insert(toidx, gpxtrack);

            GpxFileChanged = true;
            OnTracklistChanged(new TracklistChangedEventArgs(TracklistChangedEventArgs.Kind.Move, fromidx, toidx));

            return true;
         }
         return false;
      }

      #endregion

      #region Marker/Waypoints einfügen oder entfernen

      /// <summary>
      /// liefert den Listenindex für diesen <see cref="Marker"/>
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
         Gpx.GpxWaypoint wp = new Gpx.GpxWaypoint(orgmarker.Waypoint);
         Marker marker = Marker.Create(this, wp, markertype);

         if (pos < 0 || Waypoints.Count <= pos) {
            Waypoints.Add(wp);
            MarkerList.Add(marker);
            pos = Waypoints.Count - 1;
         } else {
            Waypoints.Insert(pos, wp);
            MarkerList.Insert(pos, marker);
         }

         GpxFileChanged = true;
         OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Add, -1, pos));
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> an der angegebenen Position
      /// </summary>
      /// <param name="pos"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker MarkerRemove(int pos) {
         Marker marker = null;
         if (0 <= pos && pos < Waypoints.Count) {
            Waypoints.RemoveAt(pos);
            marker = MarkerList[pos];
            MarkerList.RemoveAt(pos);
            GpxFileChanged = true;
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Remove, pos, -1));
         }
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> (und seine Gpx-Daten)
      /// </summary>
      /// <param name="marker"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker MarkerRemove(Marker marker) {
         return MarkerRemove(MarkerIndex(marker));
      }

      /// <summary>
      /// verschiebt die Position des Markers in der Auflistung
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      /// <returns></returns>
      public bool MarkerOrderChange(int fromidx, int toidx) {
         if (fromidx != toidx &&
             0 <= fromidx && fromidx < Waypoints.Count &&
             0 <= toidx && toidx < Waypoints.Count) {

            Gpx.GpxWaypoint wp = Waypoints[fromidx];
            Waypoints.RemoveAt(fromidx);
            Waypoints.Insert(toidx, wp);

            Marker marker = MarkerList[fromidx];
            MarkerList.RemoveAt(fromidx);
            MarkerList.Insert(toidx, marker);

            GpxFileChanged = true;
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Move, fromidx, toidx));

            return true;
         }
         return false;
      }

      #endregion


      /// <summary>
      /// liefert einen neuen Marker für diesen Waypoint
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      //public Marker CreateMarker(int idx, Marker.MarkerType markertype = Marker.MarkerType.Standard) {
      //   return Marker.Create(this, idx, markertype);
      //}

      /// <summary>
      /// liefert einen neuen Marker für diesen Waypoint
      /// </summary>
      /// <param name="wp"></param>
      /// <param name="markertype"></param>
      /// <returns></returns>
      //public Marker CreateMarker(Gpx.GpxWaypoint wp, Marker.MarkerType markertype = Marker.MarkerType.Standard) {
      //   return Marker.Create(this, wp, markertype);
      //}

      #region erneutes Zeichnen der Objekte auslösen

      /// <summary>
      /// Anzeige akt. (falls akt. sichtbar)
      /// </summary>
      public void VisualRefresh() {
         VisualRefreshTrackSegs();
         VisualRefreshMarkers();
         VisualRefreshPictureMarkers();
      }

      void VisualRefreshTrackSegs() {
         if (TrackSegsVisible)
            foreach (Track r in TrackList)
               r.Refresh();
      }

      void VisualRefreshMarkers() {
         if (Markers4StandardVisible) {

         }
      }

      void VisualRefreshPictureMarkers() {
         if (Markers4PicturesVisible) {

         }
      }

      #endregion



      /// <summary>
      /// liefert den nächsten sichtbaren <see cref="Track"/> oder null
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public Track NextVisibleEditableTrack(Track track) {
         int idx = TrackList.IndexOf(track);
         if (idx >= 0) {
            for (int i = idx - 1; i >= 0; i--)
               if (TrackList[i].IsVisible)
                  return TrackList[i];
         }
         return null;
      }

      /// <summary>
      /// liefert den nächsten sichtbaren <see cref="Marker"/> oder null
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      public Marker NextVisibleEditableMarker(Marker marker) {
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
      /// aus jedem Track(-Segment) eine <see cref="Track"/> (neu) erzeugen
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

      List<Marker> rebuildMarkerList() {
         MarkerList.Clear();
         OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Remove, -1, -1));
         for (int m = 0; m < Waypoints.Count; m++)
            MarkerList.Add(Marker.Create(this, m, Marker.MarkerType.Standard));
         if (MarkerList.Count > 0)
            OnMarkerlistChanged(new MarkerlistChangedEventArgs(MarkerlistChangedEventArgs.Kind.Add, -1, -1));
         return MarkerList;
      }

      /// <summary>
      /// Daten aus der Datei lesen
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="removenamespace"></param>
      public void Load(string filename, bool removenamespace = false) {
         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx") {

            FromXml(File.ReadAllText(filename), removenamespace);
            rebuildTrackList();
            rebuildMarkerList();

            for (int i = 0; i < Tracks.Count; i++) {
               foreach (var item in Tracks[i].UnhandledChildXml) {
                  if (item.Value.StartsWith("<extensions>")) {
                     Color col = getGarminColor(item.Value);
                     if (col != Color.Empty)
                        TrackList[i].LineColor = col;
                  }
               }
            }

         } else if (ext == ".gdb") {

            List<GDB.Object> objlst = GDB.ReadGDBObjectList(filename);
            long dtunix = new DateTime(1970, 1, 1).Ticks;
            foreach (GDB.Object obj in objlst) {
               switch (obj.ObjectHeader.ObjectType) {
                  case GDB.ObjectHeader.GDBObjectType.WAYPOINT:
                     GDB.Waypoint wp = obj as GDB.Waypoint;
                     InsertWaypoint(new Gpx.GpxWaypoint(wp.Lon, wp.Lat) {
                        Elevation = wp.Ele == double.MinValue ? double.MinValue : wp.Ele,
                        Name = wp.Name,
                        Description = wp.Description,
                        Time = wp.CreationTime,
                     });
                     break;

                  case GDB.ObjectHeader.GDBObjectType.TRACK:
                     GDB.Track track = obj as GDB.Track;
                     Gpx.GpxTrackSegment gpxsegment = new Gpx.GpxTrackSegment();
                     foreach (GDB.TrackPoint pt in track.Points) {
                        if (pt.DateTime.Ticks > dtunix)
                           gpxsegment.InsertPoint(new Gpx.GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele,
                                                  pt.DateTime));
                        else
                           gpxsegment.InsertPoint(new Gpx.GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele));
                     }
                     Gpx.GpxTrack gpxtrack = new Gpx.GpxTrack() {
                        Name = track.Name,
                     };
                     gpxtrack.InsertSegment(gpxsegment);
                     InsertTrack(gpxtrack);
                     break;

                  //case GDB.ObjectHeader.GDBObjectType.ROUTE:

                  //   break;
               }
            }
            rebuildTrackList();
            rebuildMarkerList();

         } else if (ext == ".kml" || ext == ".kmz") {

            Gpx.GpxAll gpx4kml = new FSofTUtils.Geography.KmlReader().Read(filename, out List<Color> colors);

            foreach (var wp in gpx4kml.Waypoints)
               InsertWaypoint(wp);

            foreach (var track in gpx4kml.Tracks)
               InsertTrack(track);

            rebuildTrackList();
            rebuildMarkerList();

            for (int i = 0; i < TrackList.Count; i++)
               if (colors[i] != Color.Transparent)
                  TrackList[i].LineColor = colors[i];
         }

      }

      /// <summary>
      /// Daten als Datei abspeichern
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="creator"></param>
      /// <param name="withgarminext"></param>
      /// <param name="gpxversion"></param>
      public void Save(string filename, string creator, bool withgarminext, string gpxversion = "1.1") {
         RebuildMetadataBounds();
         Metadata.Time = DateTime.Now;

         if (withgarminext) {
            // Trackfarben
            for (int i = 0; i < TrackList.Count; i++) {
               GarminTrackColors.Colorname col = GarminTrackColors.GetColorname(TrackList[i].LineColor, true);
               if (col != GarminTrackColors.Colorname.Unknown) {
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
      /// <returns></returns>
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
                     start = xmlextension.IndexOf("<", start + 1);
                     int end = xmlextension.IndexOf("</gpxx:DisplayColor>", start + 1);
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



      public override string ToString() {
         return string.Format("[{0} ExtTracks, {1} MarkerList, {2} MarkerListPictures, TrackSegsVisible={3}, MarkersVisible={4}, PicturesVisible={5}, LineWidth={6}, LineColor={7}]",
                              TrackList.Count,
                              MarkerList.Count,
                              MarkerListPictures.Count,
                              TrackSegsVisible,
                              Markers4StandardVisible,
                              Markers4PicturesVisible,
                              TrackWidth,
                              TrackColor.ToString());
      }

   }
}
