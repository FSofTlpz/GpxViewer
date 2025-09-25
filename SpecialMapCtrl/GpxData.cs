using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialMapCtrl {
   public class GpxData : FSofTUtils.Geography.GpxFileGarmin {

      /*
       * Multithreadprobleme sollten nur im Zusammenhang mit der Trackaufzeichnung (Punkte hinzufügen bzw. entfernen)
       * entstehen.
       * Für die Darstellung auf dem Bildschirm wird u.U. parallel auf die Punktliste des 1. Segmentes
       * zugegriffen.
       * 
       */

      #region Events

      /// <summary>
      /// wird ausgelöst, wenn auf <see cref="GpxDataChanged"/> schreibend zugegriffen wurde
      /// </summary>
      public event EventHandler? ChangeIsSet;

      protected virtual void OnChangeIsSet(EventArgs e) => ChangeIsSet?.Invoke(this, e);

      /// <summary>
      /// wird ausgelöst, wenn <see cref="GpxDataChanged"/> geändert wurde
      /// </summary>
      public event EventHandler? ChangeIsChanged;

      protected virtual void OnChangeIsChanged(EventArgs e) => ChangeIsChanged?.Invoke(this, e);


      public class ObjectListChangedEventArgs : EventArgs {

         public const int LISTPOS_MEANINGLESS = -1;
         public const int LISTPOS_ALL = -2;

         public enum Kind {
            /// <summary>
            /// hinzufügen
            /// </summary>
            Add,
            /// <summary>
            /// entfernen
            /// </summary>
            Remove,
            /// <summary>
            /// verschieben
            /// </summary>
            Move
         }

         /// <summary>
         /// Art der Änderung
         /// </summary>
         public readonly Kind KindOfChanging;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Listenausgangsposition des Objektes;
         /// bei <see cref="Kind.Add"/> beim Split eines <see cref="Track"/> die Listenposition des alten <see cref="Track"/>,
         /// sonst nicht verwendet; 
         /// bei <see cref="Kind.Remove"/> Listenposition des zu löschenden Objektes, bei <see cref="LISTPOS_ALL"/> alle Objekte löschen
         /// </summary>
         public int From { get; private set; } = LISTPOS_MEANINGLESS;

         /// <summary>
         /// bei <see cref="Kind.Move"/> Listenzielposition; 
         /// bei <see cref="Kind.Add"/> Listenzielposition des neuen Objektes, bei <see cref="LISTPOS_ALL"/> alle Objekte hinzufügen; 
         /// bei <see cref="Kind.Remove"/> nicht verwendet
         /// </summary>
         public int To { get; private set; } = LISTPOS_MEANINGLESS;


         public ObjectListChangedEventArgs(Kind kind, int fromidx, int toidx) {
            KindOfChanging = kind;
            From = fromidx;
            To = toidx;
         }

         /// <summary>
         /// bei <see cref="Kind.Add"/> für Übernahme an die Listenposition;
         /// bei <see cref="Kind.Remove"/> für Löschen an der Listenposition;
         /// <see cref="Kind.Move"/> nicht erlaubt
         /// </summary>
         /// <param name="kind"></param>
         /// <param name="idx"></param>
         /// <exception cref="ArgumentException"></exception>
         public ObjectListChangedEventArgs(Kind kind, int idx) {
            KindOfChanging = kind;
            switch (kind) {
               case Kind.Add:
                  From = LISTPOS_MEANINGLESS;
                  To = idx;
                  break;

               case Kind.Remove:
                  From = idx;
                  To = LISTPOS_MEANINGLESS;
                  break;

               case Kind.Move:
                  throw new ArgumentException(nameof(ObjectListChangedEventArgs) + "(kind, idx) mit falscher Argumentangabe");
            }
         }

         /// <summary>
         /// bei <see cref="Kind.Add"/> für Übernahme der gesamten List;
         /// bei <see cref="Kind.Remove"/> für Löschen der gesamten List;
         /// <see cref="Kind.Move"/> nicht erlaubt
         /// </summary>
         /// <param name="kind"></param>
         /// <exception cref="ArgumentException"></exception>
         public ObjectListChangedEventArgs(Kind kind) {
            KindOfChanging = kind;
            switch (kind) {
               case Kind.Add:
                  From = LISTPOS_MEANINGLESS;
                  To = LISTPOS_ALL;
                  break;

               case Kind.Remove:
                  From = LISTPOS_ALL;
                  To = LISTPOS_MEANINGLESS;
                  break;

               case Kind.Move:
                  throw new ArgumentException(nameof(ObjectListChangedEventArgs) + "(kind) mit falscher Argumentangabe");
            }
         }

         public override string ToString() => string.Format("{0}: From={1} To={2}", KindOfChanging, From, To);

      }

      /// <summary>
      /// wird ausgelöst, wenn sich die Trackliste verändert hat
      /// </summary>
      public event EventHandler<ObjectListChangedEventArgs>? TracklistChanged;

      public virtual void OnTracklistChanged(ObjectListChangedEventArgs e) => TracklistChanged?.Invoke(this, e);

      /// <summary>
      /// wird ausgelöst, wenn sich die Waypointliste verändert hat
      /// </summary>
      public event EventHandler<ObjectListChangedEventArgs>? MarkerlistlistChanged;

      public virtual void OnMarkerlistChanged(ObjectListChangedEventArgs e) => MarkerlistlistChanged?.Invoke(this, e);

      new public class ExtLoadEventArgs {
         public enum Reason {
            ReadXml,
            ReadGDB,
            ReadKml,
            InsertWaypoints,
            InsertTracks,
            InsertWaypoint,
            InsertTrack,

            SplitMultiSegmentTracks,
            RemoveEmptyTracks,
            RebuildTrackList,
            RebuildMarkerList,
         }

         public Reason LoadReason;


         public ExtLoadEventArgs(Reason reason) => LoadReason = reason;
      }

      new public event EventHandler<ExtLoadEventArgs>? ExtLoadEvent;

      #endregion

      /// <summary>
      /// Name der zugehörigen GPX-Datei (nur zur Info)
      /// </summary>
      public string? GpxFilename { get; set; }

      /// <summary>
      /// Name der zugehörigen GPX-Bilder-Datei (nur zur Info)
      /// </summary>
      public string? GpxPictureFilename { get; set; }

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

      bool _tracksVisible = true;
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

      private bool _markersVisible = true;
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

      private bool _picturesVisible = true;
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

      private Color lineColor = Color.Black;
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

      private double _lineWidth = 1;
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
      public List<Track> TrackList { get; private set; } = [];

      /// <summary>
      /// alle <see cref="GpxWaypoint"/> dieser Datei als <see cref="Marker"/>
      /// </summary>
      public List<Marker> MarkerList { get; private set; } = [];

      /// <summary>
      /// spez. Liste, die nur extern gefüllt wird (aus <see cref="GpxPictureFilename"/>)
      /// </summary>
      public List<Marker> MarkerListPictures { get; private set; } = [];


      public GpxData(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) {
         //if (!string.IsNullOrEmpty(xmltext))
         //   postImportXml(out List<Color> _);
      }

      /// <summary>
      /// ev. notwendige Aufbereitung der GPX-Daten
      /// </summary>
      override protected void postLoad() {
         sendLoadInfo(ExtLoadEventArgs.Reason.SplitMultiSegmentTracks);
         splitMultiSegmentTracksWithLock();
         sendLoadInfo(ExtLoadEventArgs.Reason.RemoveEmptyTracks);
         removeEmptyTracksWithLock();
         sendLoadInfo(ExtLoadEventArgs.Reason.RebuildTrackList);
         rebuildTrackListWithLock();
         sendLoadInfo(ExtLoadEventArgs.Reason.RebuildMarkerList);
         rebuildMarkerListWithLock();
      }

      protected override void postImportXml(out List<Color> trackcolor) {
         base.postImportXml(out trackcolor);

         for (int i = 0; i < Tracks.Count && i < trackcolor.Count; i++)
            TrackList[i].LineColor = trackcolor[i];
      }


      SemaphoreSlim semaphoreSlim4write = new SemaphoreSlim(1);

      protected void writeLock() {
         semaphoreSlim4write.Wait();
      }

      protected void writeUnlock() {
         semaphoreSlim4write.Release();
      }


      #region Trackpunkt hinzufügen/entfernen mit Lock

      public bool AppendTrackSegmentPointWithLock(int trackno, int segmentno, GpxTrackPoint pt) {
         writeLock();
         bool ok = false;
         ListTS<GpxTrackPoint>? lst = GetTrack(trackno)?.GetSegment(segmentno)?.Points;
         if (lst != null) {
            lst.Add(pt);
            ok = true;
         }
         writeUnlock();
         return ok;
      }

      public bool InsertTrackSegmentPointWithLock(int trackno, int segmentno, int ptno, GpxTrackPoint pt) {
         writeLock();
         bool ok = false;
         ListTS<GpxTrackPoint>? lst = GetTrack(trackno)?.GetSegment(segmentno)?.Points;
         if (lst != null) {
            if (0 <= ptno && ptno < lst.Count)
               lst.Insert(ptno, pt);
            else if (ptno < 0)
               lst.Insert(0, pt);
            else
               lst.Add(pt);
            ok = true;
         }
         writeUnlock();
         return ok;
      }

      public bool RemoveTrackSegmentPointWithLock(int trackno, int segmentno, int ptno) {
         writeLock();
         bool ok = false;
         ListTS<GpxTrackPoint>? lst = GetTrack(trackno)?.GetSegment(segmentno)?.Points;
         if (lst != null && 0 <= ptno && ptno < lst.Count) {
            lst?.RemoveAt(ptno);
            ok = true;
         }
         writeUnlock();
         return ok;
      }

      #endregion

      /// <summary>
      /// liefert den Listenindex des <see cref="Track"/> in <see cref="TrackList"/>
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public int TrackIndex(Track track) => TrackList.IndexOf(track);

      /// <summary>
      /// falls der Name für die <see cref="Track"/> der <see cref="TrackList"/> nicht eindeutig ist, wird so lange
      /// " *" angehängt bis der Name eindeutig ist und geliefert
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public string GetUniqueTrackname(string? name) {
         bool found = true;
         name ??= string.Empty;
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

      #region Track einfügen oder entfernen

      /// <summary>
      /// fügt eine Kopie der <see cref="Track"/> in den akt. Container als <see cref="GpxTrack"/> und 
      /// als <see cref="Track"/> in <see cref="TrackList"/> ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <param name="useorgprops">bei false wird die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public Track TrackInsertCopyWithLock(Track orgtrack, int pos = -1, bool useorgprops = false) {
         writeLock();
         Track track = Track.CreateCopy(orgtrack, this, useorgprops);
         track.GpxTrack.Name = GetUniqueTrackname(track.GpxTrack.Name);
         if (track.VisualName != track.GpxTrack.Name)
            track.VisualName = track.GpxTrack.Name;

         if (pos < 0 || TrackList.Count <= pos) { // Position "ungültig" -> anhängen
            TrackList.Add(track);
            Tracks.Add(track.GpxTrack);
            pos = TrackList.Count - 1;
         } else {
            TrackList.Insert(pos, track);
            Tracks.Insert(pos, track.GpxTrack);
         }
         GpxDataChanged = true;
         writeUnlock();
         OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Add, pos));
         return track;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> aus <see cref="TrackList"/> und den dazugehörigen <see cref="GpxTrack"/> 
      /// an der angegebenen Position
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="withlock"></param>
      /// <returns></returns>
      public Track? TrackRemoveWithLock(int pos, bool withlock = true) {
         Track? tracksegment = null;
         if (0 <= pos && pos < TrackList.Count) {
            tracksegment = TrackList[pos];
            if (tracksegment.GpxTrack != null &&
                tracksegment.GpxSegment != null) {
               if (withlock)
                  writeLock();
               tracksegment.GpxTrack.Segments.Remove(tracksegment.GpxSegment);

               if (tracksegment.GpxTrack.Segments.Count == 0)
                  Tracks.Remove(tracksegment.GpxTrack);

               TrackList.Remove(tracksegment);

               GpxDataChanged = true;
               if (withlock)
                  writeUnlock();
               OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Remove, pos));
            }
         }
         return tracksegment;
      }

      /// <summary>
      /// entfernt den <see cref="Track"/> aus <see cref="TrackList"/> und den dazugehörigen <see cref="GpxTrack"/> 
      /// </summary>
      /// <param name="tracksegment"></param>
      /// <param name="withlock"></param>
      /// <returns></returns>
      public Track? TrackRemoveWithLock(Track tracksegment, bool withlock = true) =>
         TrackRemoveWithLock(TrackIndex(tracksegment), withlock);

      /// <summary>
      /// entfernt alle <see cref="Track"/> aus <see cref="TrackList"/> und die dazugehörigen <see cref="GpxTrack"/> 
      /// </summary>
      public void TrackRemoveAllWithLock() {
         while (TrackList.Count > 0)
            TrackRemoveWithLock(0);
      }

      /// <summary>
      /// trennt den vorhandenen <see cref="Track"/> (der erste <see cref="Track"/> wird nur gekürzt, der zweite hinter 
      /// dem 1. in der <see cref="TrackList"/> eingefügt) und passt auch die dazugehörigen <see cref="GpxTrack"/> an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="splitptidx"></param>
      /// <returns>neuer <see cref="Track"/></returns>
      public Track? TrackSplitWithLock(Track track, int splitptidx) =>
         TrackSplitWithLock(TrackList.IndexOf(track), splitptidx);

      /// <summary>
      /// trennt den vorhandenen <see cref="Track"/> (der erste <see cref="Track"/> wird nur gekürzt, der zweite hinter 
      /// dem 1. in der <see cref="TrackList"/> eingefügt) und passt auch die dazugehörigen <see cref="GpxTrack"/> an
      /// </summary>
      /// <param name="trackidx"></param>
      /// <param name="splitptidx"></param>
      /// <returns>neuer <see cref="Track"/></returns>
      public Track? TrackSplitWithLock(int trackidx, int splitptidx) {
         if (0 <= trackidx && trackidx < Tracks.Count) {
            GpxTrack orgtrack = Tracks[trackidx];
            if (0 < splitptidx && splitptidx < orgtrack.Segments[0].Points.Count - 1) {
               GpxTrack trackcopy = new(orgtrack);  // (unechte) Kopie erzeugen
               // Die "unechte" Kopie ist kein Problem, weil beide Tracks zwar zunächst auf die selben Punkte
               // verweisen. Aber nach dem RemoveRange für die beiden Tracks sind die Doppelverweise beseitigt.
               // Vorteil: keine aufwendige Kopie für alle Punkte nötig

               writeLock();
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
               writeUnlock();
               OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Add,
                                                                 trackidx,
                                                                 TrackList.Count - 1));

               TrackOrderChangeWithLock(TrackList.Count - 1, trackidx);  // direkt vor den Originaltrack schieben

               return newtrack;
            }
         }
         return null;
      }

      public bool TrackRemovePointWithLock(Track track, int ptidx) =>
         TrackRemovePointWithLock(TrackList.IndexOf(track), ptidx);

      public bool TrackRemovePointWithLock(int trackidx, int ptidx) {
         if (0 <= trackidx && trackidx < Tracks.Count) {
            GpxTrack track = Tracks[trackidx];
            if (0 <= ptidx && ptidx < track.Segments[0].Points.Count) {
               writeLock();
               track.Segments[0].Points.RemoveAt(ptidx);
               TrackList[trackidx].CalculateStats();
               GpxDataChanged = true;
               writeUnlock();
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// <see cref="Track"/> 2 wird an <see cref="Track"/> 1 angehängt und die dazugehörigen <see cref="GpxTrack"/> angepasst
      /// </summary>
      /// <param name="track1"></param>
      /// <param name="track2"></param>
      /// <returns><see cref="Track"/> 1</returns>
      public Track? TrackConcatWithLock(Track track1, Track track2) => TrackConcatWithLock(TrackList.IndexOf(track1),
                                                                                           TrackList.IndexOf(track2));

      /// <summary>
      /// <see cref="Track"/> 2 wird an <see cref="Track"/> 1 angehängt und die dazugehörigen <see cref="GpxTrack"/> angepasst
      /// </summary>
      /// <param name="track1idx"></param>
      /// <param name="track2idx"></param>
      /// <returns>Track 1</returns>
      public Track? TrackConcatWithLock(int track1idx, int track2idx) {
         Track? track = null;
         if (0 <= track1idx && track1idx < Tracks.Count &&
             0 <= track2idx && track2idx < Tracks.Count &&
             track1idx != track2idx) {
            writeLock();
            Track t1 = TrackList[track1idx];
            Track t2 = TrackList[track2idx];

            if (t1.GpxSegment != null && t2.GpxSegment != null)
               if (t1.GpxSegment.Points.Count > 0 &&
                   t2.GpxSegment.Points.Count > 0) {
                  GpxTrackPoint lastPt = t1.GpxSegment.Points[t1.GpxSegment.Points.Count - 1];
                  GpxTrackPoint firstPt = t2.GpxSegment.Points[0];
                  if (lastPt.AsXml(9) == firstPt.AsXml(9))  // Punkte sind völlig identisch
                     t1.GpxSegment.Points.Remove(lastPt);
               }

            if (t1.GpxSegment != null && t2.GpxSegment != null)
               t1.GpxSegment.Points.AddRange(t2.GpxSegment.Points);
            t1.CalculateStats();
            writeUnlock();
            TrackRemoveWithLock(track2idx);
            track = t1;
         }
         return track;
      }

      /// <summary>
      /// liefert einen neuen <see cref="Track"/> für dieses Segment
      /// <para>Das Segment wird nur in einem <see cref="Track"/> "verpackt" aber keine Kopie der Daten erzeugt.</para>
      /// </summary>
      /// <param name="trackno"></param>
      /// <param name="segmentno"></param>
      /// <returns></returns>
      Track createTrackFromSegment(int trackno, int segmentno) {
         GpxTrack t = Tracks[trackno];
         string name = t.Name ?? string.Empty;
         Track track = Track.Create(this,
                                    trackno,
                                    segmentno,
                                    name);
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
      public bool TrackOrderChangeWithLock(int fromidx, int toidx) {
         bool ok = false;
         writeLock();
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
            OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Move, fromidx, toidx));

            ok = true;
         }
         writeUnlock();
         return ok;
      }

      #endregion

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

      #region Marker einfügen oder entfernen

      /// <summary>
      /// fügt eine Kopie des <see cref="Marker"/> an den akt. Container an (oder ein)
      /// </summary>
      /// <param name="orgwp"></param>
      /// <returns></returns>
      public Marker MarkerInsertCopyWithLock(
                                 Marker orgmarker,
                                 int pos = -1,
                                 Marker.MarkerType markertype = Marker.MarkerType.Standard) {
         writeLock();
         if (orgmarker.Waypoint == null)
            throw new ArgumentException("Waypoint is null");
         GpxWaypoint wp = new(orgmarker.Waypoint);
         Marker marker = Marker.Create(this, wp, markertype);

         if (pos < 0 || Waypoints.Count <= pos) {  // Position "ungültig" -> anhängen
            Waypoints.Add(wp);
            MarkerList.Add(marker);
            pos = Waypoints.Count - 1;
         } else {
            Waypoints.Insert(pos, wp);
            MarkerList.Insert(pos, marker);
         }

         GpxDataChanged = true;
         writeUnlock();
         OnMarkerlistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Add, pos));
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> an der angegebenen Position aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="pos"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker? MarkerRemoveWithLock(int pos) {
         Marker? marker = null;
         if (0 <= pos && pos < Waypoints.Count) {
            writeLock();
            Waypoints.RemoveAt(pos);
            marker = MarkerList[pos];
            MarkerList.RemoveAt(pos);
            GpxDataChanged = true;
            writeUnlock();
            OnMarkerlistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Remove, pos));
         }
         return marker;
      }

      /// <summary>
      /// entfernt den <see cref="Marker"/> aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <returns>entfernter <see cref="Marker"/> oder null</returns>
      public Marker? MarkerRemoveWithLock(Marker marker) => MarkerRemoveWithLock(MarkerIndex(marker));

      /// <summary>
      /// alle <see cref="Marker"/> aus <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/> entfernen
      /// </summary>
      public void MarkerRemoveAllWithLock() {
         while (MarkerList.Count > 0)
            MarkerRemoveWithLock(0);
      }

      /// <summary>
      /// verschiebt die Position des <see cref="Marker"/> in <see cref="MarkerList"/> und <see cref="GpxAll.Waypoints"/>
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      /// <returns></returns>
      public bool MarkerOrderChangeWithLock(int fromidx, int toidx) {
         bool ok = false;
         writeLock();
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
            ok = true;
         }
         writeUnlock();
         if (ok)
            OnMarkerlistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Move, fromidx, toidx));
         return ok;
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
      public Track? NextVisibleTrack(Track track) {
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
      public Marker? NextVisibleMarker(Marker marker) {
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
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      public void SaveWithLock(
                     string filename,
                     string creator,
                     bool withgarminext,
                     IList<Color> colors,
                     string gpxversion = STDGPXVERSION,
                     bool withxmlcolor = false) {
         writeLock();
         Save(
            filename,
            creator,
            withgarminext,
            null,
            colors,
            null,
            gpxversion,
            withxmlcolor);
         writeUnlock();
      }

      /// <summary>
      /// Daten asynchron als Datei abspeichern
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="creator"></param>
      /// <param name="withgarminext"></param>
      /// <param name="colorname">diese Farbe wird NICHT gespeichert</param>
      /// <param name="gpxversion"></param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      public async Task SaveAsyncWithLock(
                                    string filename,
                                    string creator,
                                    bool withgarminext,
                                    IList<Color> colors,
                                    string gpxversion = STDGPXVERSION,
                                    bool withxmlcolor = false) =>
         await Task.Run(() => SaveWithLock(filename, creator, withgarminext, colors, gpxversion, withxmlcolor));

      void sendLoadInfo(ExtLoadEventArgs.Reason reason) => ExtLoadEvent?.Invoke(this, new ExtLoadEventArgs(reason));

      ///// <summary>
      ///// Daten aus der Datei lesen
      ///// </summary>
      ///// <param name="filename"></param>
      ///// <param name="removenamespace"></param>
      //public void Load(string filename,
      //                 bool removenamespace = false) {
      //   string ext = Path.GetExtension(filename).ToLower();
      //   if (ext == ".gpx") {

      //      LoadInfoEvent += base_LoadInfoEvent;
      //      sendLoadInfo(ExtLoadEventArgs.Reason.ReadXml);
      //      FromXml(File.ReadAllText(filename), removenamespace);
      //      postImportXml();
      //      LoadInfoEvent -= base_LoadInfoEvent;

      //   } else if (ext == ".gdb") {

      //      sendLoadInfo(ExtLoadEventArgs.Reason.ReadGDB);
      //      List<GDB.Object> objlst = GDB.ReadGDBObjectList(filename);
      //      long dtunix = new DateTime(1970, 1, 1).Ticks;
      //      foreach (GDB.Object obj in objlst) {
      //         switch (obj.ObjectHeader.ObjectType) {
      //            case GDB.ObjectHeader.GDBObjectType.WAYPOINT:
      //               GDB.Waypoint wp = (GDB.Waypoint)obj;
      //               string? symbol = GDB.GetIconName4Symbolnumber(wp.IconIdx);
      //               GpxWaypoint waypoint = new GpxWaypoint(wp.Lon, wp.Lat) {
      //                  Elevation = wp.Ele == double.MinValue ? double.MinValue : wp.Ele,
      //                  Name = wp.Name,
      //                  Description = wp.Description,
      //                  Time = wp.CreationTime,
      //                  Symbol = symbol != null ? symbol : string.Empty,
      //               };
      //               if (wp.IconIdx > 0) {
      //                  string? name = GDB.GetIconName4Symbolnumber(wp.IconIdx);
      //                  if (!string.IsNullOrEmpty(name))
      //                     waypoint.Symbol = name;
      //               }
      //               InsertWaypoint(waypoint);
      //               sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoint);
      //               break;

      //            case GDB.ObjectHeader.GDBObjectType.TRACK:
      //               GDB.Track track = (GDB.Track)obj;
      //               GpxTrackSegment gpxsegment = new GpxTrackSegment();
      //               foreach (GDB.TrackPoint pt in track.Points) {
      //                  if (pt.DateTime.Ticks > dtunix)
      //                     gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
      //                                            pt.Lat,
      //                                            pt.Ele == double.MinValue ? double.MinValue : pt.Ele,
      //                                            pt.DateTime));
      //                  else
      //                     gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
      //                                            pt.Lat,
      //                                            pt.Ele == double.MinValue ? double.MinValue : pt.Ele));
      //               }
      //               GpxTrack gpxtrack = new GpxTrack() {
      //                  Name = track.Name,
      //               };
      //               gpxtrack.InsertSegment(gpxsegment);
      //               InsertTrack(gpxtrack);
      //               sendLoadInfo(ExtLoadEventArgs.Reason.InsertTrack);
      //               break;

      //               //case GDB.ObjectHeader.GDBObjectType.ROUTE:

      //               //   break;
      //         }
      //      }
      //      postLoad();

      //   } else if (ext == ".kml" || ext == ".kmz") {

      //      sendLoadInfo(ExtLoadEventArgs.Reason.ReadKml);
      //      GpxAll gpx4kml = new FSofTUtils.Geography.KmlReader().Read(filename, out List<Color> colors);

      //      sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoints);
      //      for (int i = 0; i < gpx4kml.Waypoints.Count; i++) {
      //         InsertWaypoint(gpx4kml.Waypoints[i]);
      //         sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoint);
      //      }

      //      sendLoadInfo(ExtLoadEventArgs.Reason.InsertTracks);
      //      for (int i = 0; i < gpx4kml.Tracks.Count; i++) {
      //         InsertTrack(gpx4kml.Tracks[i]);
      //         sendLoadInfo(ExtLoadEventArgs.Reason.InsertTrack);
      //      }

      //      postLoad();

      //      for (int i = 0; i < TrackList.Count; i++)
      //         if (colors[i] != Color.Transparent)
      //            TrackList[i].LineColor = colors[i];
      //   }
      //}

      const string FULLTAGENDMETADATA = "</" + GpxMetadata1_1.NODENAME + ">";

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
                                    Func<GpxBounds, ListTS<GpxTrackPoint>, bool> isroutecrossing) {
         // zuerst nur ev. vorhandene Metadaten testen
         string xmltxt;
         using (StreamReader sr = new(filename)) {
            char[] txtbuff = new char[1000];
            int chars = sr.ReadBlock(txtbuff, 0, txtbuff.Length);
            xmltxt = new string(txtbuff);

            int start = xmltxt.IndexOf(TAGMETADATA);
            if (start >= 0) {

               int end = xmltxt.IndexOf(FULLTAGENDMETADATA);
               if (end >= 0 && start < end) {
                  GpxMetadata1_1 metadata = new(xmltxt.Substring(start, end - start + 1 + TAGMETADATA.Length));
                  if (metadata.Bounds != null &&
                      metadata.Bounds.MinLon != metadata.Bounds.MaxLon &&
                      metadata.Bounds.MinLat != metadata.Bounds.MaxLat)
                     if (!metadata.Bounds.IntersectsWith(testarea))
                        return false;
               }

            }

            // restliche Datei einlesen
            xmltxt += sr.ReadToEnd();
         }

         // vollständige Datei testen
         string? firsttag = getFirstXmlTag(xmltxt);
         if (firsttag != null && firsttag.StartsWith("<?xml "))
            xmltxt = xmltxt.Substring(firsttag.Length);     // "<?xml " entfernen

         UnhandledChildXml = getChildCollection(xmltxt, true);  // alle Childs erstmal als UnhandledChildXml registrieren
         if (UnhandledChildXml != null) {
            for (int i = 0; i < UnhandledChildXml.Count; i++) {
               string childtxt = UnhandledChildXml[i];
               string? tag = getFirstXmlTag(childtxt);
               if (tag != null) {
                  if (withwaypoints && tag.StartsWith(TAGWAYPOINT)) {
                     GpxWaypoint wp = new(childtxt);
                     if (testarea.IntersectsWith(new GpxBounds(wp.Lat, wp.Lat, wp.Lon, wp.Lon)))
                        return true;
                  } else if (tag == TAGROUTE) {
                     GpxRoute r = new(childtxt);
                     GpxBounds bounds = new();
                     if (r.Points != null) {
                        bounds.Union(r.Points);
                        if (testarea.IntersectsWith(bounds))
                           return true;
                     }
                  } else if (tag == TAGTRACK) {
                     GpxTrack track = new(childtxt);
                     for (int j = 0; j < track.Segments.Count; j++) {
                        ListTS<GpxTrackPoint>? pts = track.Segments[j].Points;
                        if (pts != null) {
                           if (isroutecrossing == null) {
                              GpxBounds bounds = new();
                              bounds.Union(pts);
                              if (testarea.IntersectsWith(bounds))
                                 return true;
                           } else {
                              if (isroutecrossing(testarea, pts))
                                 return true;
                           }
                        }
                     }
                  }
               }
            }
         }
         return false;
      }

      /// <summary>
      /// testet, ob ein rechteckiges Testgebiet von den Daten der GPX-Datei betroffen ist
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="testarea"></param>
      /// <param name="withwaypoints"></param>
      /// <param name="isroutecrossing"></param>
      /// <returns></returns>
      public async Task<bool> CheckAreaFromFileAsync(string filename,
                                                     GpxBounds testarea,
                                                     bool withwaypoints,
                                                     Func<GpxBounds, ListTS<GpxTrackPoint>, bool> isroutecrossing) {
         // zuerst nur ev. vorhandene Metadaten testen
         string xmltxt;
         using (StreamReader sr = new(filename)) {
            char[] txtbuff = new char[1000];
            int chars = sr.ReadBlock(txtbuff, 0, txtbuff.Length);
            xmltxt = new string(txtbuff);

            int start = xmltxt.IndexOf(TAGMETADATA);
            if (start >= 0) {

               int end = xmltxt.IndexOf(FULLTAGENDMETADATA);
               if (end >= 0 && start < end) {
                  GpxMetadata1_1 metadata = new(xmltxt.Substring(start, end - start + 1 + TAGMETADATA.Length));
                  if (metadata.Bounds != null &&
                      metadata.Bounds.MinLon != metadata.Bounds.MaxLon &&
                      metadata.Bounds.MinLat != metadata.Bounds.MaxLat)
                     if (!metadata.Bounds.IntersectsWith(testarea))
                        return false;
               }

            }

            // restliche Datei einlesen
            xmltxt += await sr.ReadToEndAsync();
         }

         // vollständige Datei testen
         string? firsttag = getFirstXmlTag(xmltxt);
         if (firsttag != null && firsttag.StartsWith("<?xml "))
            xmltxt = xmltxt.Substring(firsttag.Length);     // "<?xml " entfernen

         UnhandledChildXml = getChildCollection(xmltxt, true);  // alle Childs erstmal als UnhandledChildXml registrieren
         if (UnhandledChildXml != null) {
            for (int i = 0; i < UnhandledChildXml.Count; i++) {
               string childtxt = UnhandledChildXml[i];
               string? tag = getFirstXmlTag(childtxt);
               if (tag != null) {
                  if (withwaypoints && tag.StartsWith(TAGWAYPOINT)) {
                     GpxWaypoint wp = new(childtxt);
                     if (testarea.IntersectsWith(new GpxBounds(wp.Lat, wp.Lat, wp.Lon, wp.Lon)))
                        return true;
                  } else if (tag == TAGROUTE) {
                     GpxRoute r = new(childtxt);
                     GpxBounds bounds = new();
                     if (r.Points != null) {
                        bounds.Union(r.Points);
                        if (testarea.IntersectsWith(bounds))
                           return true;
                     }
                  } else if (tag == TAGTRACK) {
                     GpxTrack track = new(childtxt);
                     for (int j = 0; j < track.Segments.Count; j++) {
                        ListTS<GpxTrackPoint>? pts = track.Segments[j].Points;
                        if (pts != null) {
                           if (isroutecrossing == null) {
                              GpxBounds bounds = new();
                              bounds.Union(pts);
                              if (testarea.IntersectsWith(bounds))
                                 return true;
                           } else {
                              if (isroutecrossing(testarea, pts))
                                 return true;
                           }
                        }
                     }
                  }
               }
            }
         }
         return false;
      }

      /// <summary>
      /// aus Tracks mit mehreren Segmenten werden Tracks mit nur einem Segment
      /// </summary>
      void splitMultiSegmentTracksWithLock() {
         writeLock();
         for (int t = Tracks.Count - 1; t >= 0; t--) {
            GpxTrack track = Tracks[t];
            while (track.Segments.Count > 1) {
               GpxTrackSegment segment = track.Segments[1];
               if (segment.Points.Count > 0) {
                  GpxTrack newtrack = new() { Name = track.Name, };
                  newtrack.Segments.Add(segment);
                  Tracks.Insert(t + 1, newtrack);
               }
               track.Segments.RemoveAt(1);
            }
         }
         writeUnlock();
      }

      /// <summary>
      /// Tracks ohne Punkte werden entfernt
      /// </summary>
      void removeEmptyTracksWithLock() {
         writeLock();
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
         writeUnlock();
      }

      /// <summary>
      /// die <see cref="TrackList"/> wird neu mit neu erzeugten <see cref="Track"/> aus den <see cref="GpxAll.Tracks"/> gebildet
      /// </summary>
      /// <returns></returns>
      List<Track> rebuildTrackListWithLock() {
         writeLock();
         TrackList.Clear();
         OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Remove));
         for (int t = 0; t < Tracks.Count; t++)
            for (int s = 0; s < Tracks[t].Segments.Count; s++)
               TrackList.Add(createTrackFromSegment(t, s));
         if (TrackList.Count > 0)
            OnTracklistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Add));
         writeUnlock();
         return TrackList;
      }

      /// <summary>
      /// die <see cref="MarkerList"/> wird neu mit neu erzeugten <see cref="Marker"/> aus den <see cref="GpxAll.Waypoints"/> gebildet
      /// </summary>
      /// <returns></returns>
      List<Marker> rebuildMarkerListWithLock() {
         writeLock();
         MarkerList.Clear();
         OnMarkerlistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Remove));
         for (int m = 0; m < Waypoints.Count; m++)
            MarkerList.Add(Marker.Create(this, m, GpxFileEditable ? Marker.MarkerType.EditableStandard : Marker.MarkerType.Standard));
         if (MarkerList.Count > 0)
            OnMarkerlistChanged(new ObjectListChangedEventArgs(ObjectListChangedEventArgs.Kind.Add));
         writeUnlock();
         return MarkerList;
      }

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
