using FSofTUtils.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl {

   /// <summary>
   /// Trackdaten (für ein einzelnes Tracksegment), geografisch, visuell usw.
   /// <para>Die geografischen Daten werden als <see cref="Gpx.GpxTrack"/> gespeichert.</para>
   /// </summary>
   public class Track {

      /// <summary>
      /// Container aller GPX-Daten zu dem die <see cref="Track"/> gehört
      /// </summary>
      public GpxData? GpxDataContainer { get; protected set; } = null;

      /// <summary>
      /// Originaltrack aus dem <see cref="GpxDataContainer"/>
      /// </summary>
      public Gpx.GpxTrack GpxTrack { get; protected set; }

      /// <summary>
      /// Originalsegment aus dem <see cref="GpxTrack"/> aus dem <see cref="GpxDataContainer"/>
      /// </summary>
      public Gpx.GpxTrackSegment? GpxSegment { get; protected set; }

      /// <summary>
      /// Boundingbox
      /// </summary>
      public Gpx.GpxBounds? Bounds { get; protected set; }

      /// <summary>
      /// liefert den akt. Index der <see cref="Track"/> in <see cref="GpxData"/>
      /// </summary>
      /// <returns></returns>
      public int GpxDataContainerIndex {
         get => GpxDataContainer != null ? GpxDataContainer.TrackList.IndexOf(this) : -1;
      }

      /// <summary>
      /// Name des (gesamten) Tracks
      /// </summary>
      public string Trackname {
         get => GpxTrack != null && GpxTrack.Name != null ? GpxTrack.Name : string.Empty;
         set {
            if (GpxTrack != null)
               GpxTrack.Name = value;
         }
      }

      /// <summary>
      /// anzuzeigender Name (muss nicht mit <see cref="Trackname"/> übereinstimmen)
      /// </summary>
      public string VisualName { get; set; }

      /// <summary>
      /// nur zum Anzeigen des Tracks nötig
      /// </summary>
      public VisualTrack? VisualTrack { get; protected set; }

      /// <summary>
      /// Ist der Track editierbar (wird, wenn vorhanden, vom <see cref="GpxDataContainer"/> vorgegeben, sonst true)?
      /// </summary>
      public bool IsEditable =>
         GpxDataContainer == null || GpxDataContainer.GpxFileEditable;

      bool _iseditinwork = false;
      /// <summary>
      /// Ist der (editierbare) Track gerade "in Bearbeitung"?
      /// </summary>
      public bool IsOnEdit {
         get => _iseditinwork;
         set {
            if (IsEditable) {
               if (_iseditinwork != value) {
                  _iseditinwork = value;
                  setStyle4VisualTrack();
               }
            }
         }
      }

      public bool IsOnLiveDraw { get; set; } = false;

      bool _isMarked = false;
      /// <summary>
      /// <see cref="Track"/> ist in der Anzeige markiert
      /// </summary>
      public bool IsMarked {
         get => _isMarked;
         set {
            if (value != _isMarked) {
               _isMarked = value;
               setStyle4VisualTrack();
            }
         }
      }

      bool _isMarked4Edit = false;
      /// <summary>
      /// <see cref="Track"/> ist für die Bearbeitung markiert
      /// </summary>
      public bool IsMarked4Edit {
         get => _isMarked4Edit;
         set {
            if (value != _isMarked4Edit) {
               _isMarked4Edit = value;
               setStyle4VisualTrack();
            }
         }
      }

      /// <summary>
      /// <see cref="Track"/> ist nur ein Teil-Track (spez. Anzeige)
      /// </summary>
      public bool IsSelectedPart { get; set; } = false;

      /// <summary>
      /// Wird <see cref="Track"/> angezeigt?
      /// </summary>
      public bool IsVisible {
         get => VisualTrack != null && VisualTrack.IsVisible;
         set {
            if (VisualTrack != null)
               VisualTrack.IsVisible = value;
            else if (value == true) {
               UpdateVisualTrack();
               if (VisualTrack != null)
                  VisualTrack.IsVisible = value;
            }
         }
      }

      bool _isSlopeVisible = false;
      /// <summary>
      /// Soll der Anstieg angezeigt werden?
      /// </summary>
      public bool IsSlopeVisible {
         get => _isSlopeVisible;
         set {
            if (value != _isSlopeVisible) {
               _isSlopeVisible = value;
               //if (VisualTrack != null && !IsMarked4Edit)
               //   UpdateVisualTrack();
            }
         }
      }

      Color _lineColor;
      /// <summary>
      /// Farbe des <see cref="Track"/>
      /// </summary>
      public Color LineColor {
         get => _lineColor;
         set {
            if (_lineColor.ToArgb() != value.ToArgb()) { // MS: ... For example, Black and FromArgb(0,0,0) are not considered equal, since Black is a named color and FromArgb(0,0,0) is not.
               _lineColor = value;
               if (GpxDataContainer != null) { // wenn alle Tracks die gleiche Farbe haben, dann auch die Containerfarbe setzen
                  bool different = false;
                  foreach (var item in GpxDataContainer.TrackList) {
                     if (_lineColor != item.LineColor) {
                        different = true;
                        break;
                     }
                  }
                  if (!different)
                     GpxDataContainer.TrackColor = _lineColor;
               }
               setStyle4VisualTrack();
            }
         }
      }

      double _lineWidth;
      /// <summary>
      /// Breite des <see cref="Track"/>
      /// </summary>
      public double LineWidth {
         get => _lineWidth;
         set {
            if (_lineWidth != value) {
               _lineWidth = value;
               setStyle4VisualTrack();
            }
         }
      }

      #region statistische Daten (threadsicher)

      ThreadSafeDoubleVariable _statMinHeigth = new ThreadSafeDoubleVariable(double.MaxValue);
      ThreadSafeDoubleVariable _statMaxHeigth = new ThreadSafeDoubleVariable(double.MinValue);
      ThreadSafeDoubleVariable _statElevationUp = new ThreadSafeDoubleVariable(0);
      ThreadSafeDoubleVariable _statElevationDown = new ThreadSafeDoubleVariable(0);
      ThreadSafeIntVariable _statMinDateTimeIdx = new ThreadSafeIntVariable(-1);
      ThreadSafeVariable<DateTime> _statMinDateTime = new ThreadSafeVariable<DateTime>(DateTime.MaxValue);
      ThreadSafeIntVariable _statMaxDateTimeIdx = new ThreadSafeIntVariable(-1);
      ThreadSafeVariable<DateTime> _statMaxDateTime = new ThreadSafeVariable<DateTime>(DateTime.MinValue);
      ThreadSafeDoubleVariable _statLength = new ThreadSafeDoubleVariable(0);
      ThreadSafeDoubleVariable _satLengthWithTime = new ThreadSafeDoubleVariable(0);

      public double StatMinHeigth { get => _statMinHeigth.Value; protected set => _statMinHeigth.Value = value; }
      public double StatMaxHeigth { get => _statMaxHeigth.Value; protected set => _statMaxHeigth.Value = value; }
      public double StatElevationUp { get => _statElevationUp.Value; protected set => _statElevationUp.Value = value; }
      public double StatElevationDown { get => _statElevationDown.Value; protected set => _statElevationDown.Value = value; }
      public DateTime StatMinDateTime { get => _statMinDateTime.Value; protected set => _statMinDateTime.Value = value; }
      public int StatMinDateTimeIdx { get => _statMinDateTimeIdx.Value; protected set => _statMinDateTimeIdx.Value = value; }
      public DateTime StatMaxDateTime { get => _statMaxDateTime.Value; protected set => _statMaxDateTime.Value = value; }
      public int StatMaxDateTimeIdx { get => _statMaxDateTimeIdx.Value; protected set => _statMaxDateTimeIdx.Value = value; }
      public double StatLength { get => _statLength.Value; protected set => _statLength.Value = value; }
      public double StatLengthWithTime { get => _satLengthWithTime.Value; protected set => _satLengthWithTime.Value = value; }

      #endregion


      /// <summary>
      /// erzeugt intern einen <see cref="Gpx.GpxTrack"/> mit 1 Segment ohne Punkte
      /// <para>Es gibt keinen Verweis auf einen <see cref="GpxData"/>.</para>
      /// </summary>
      /// <param name="visualname"></param>
      public Track(string visualname) {
         VisualName = visualname;
         GpxTrack = new Gpx.GpxTrack();
         GpxTrack.Segments.Add(new Gpx.GpxTrackSegment());
      }

      /// <summary>
      /// erzeugt intern einen <see cref="Gpx.GpxTrack"/> mit 1 Segment und einer Kopie der Punkte
      /// <para>Es gibt keinen Verweis auf einen <see cref="GpxData"/>.</para>
      /// </summary>
      /// <param name="gpxpoints"></param>
      /// <param name="visualname">anzuzeigender Name (wird auch als Trackname übernommen)</param>
      public Track(IList<Gpx.GpxTrackPoint> gpxpoints, string visualname) {
         VisualName = visualname;
         GpxSegment = new Gpx.GpxTrackSegment();
         GpxSegment.Points.AddRange(clonePoints(gpxpoints));
         GpxTrack = new Gpx.GpxTrack();
         GpxTrack.Segments.Add(GpxSegment);
         GpxTrack.Name = VisualName;
         CalculateStats();
         Bounds = CalculateBounds(GpxSegment.Points);
      }

      /// <summary>
      /// erzeugt eine Kopie der Punktliste
      /// </summary>
      /// <param name="gpxptlst"></param>
      /// <returns></returns>
      static Gpx.GpxTrackPoint[] clonePoints(IList<Gpx.GpxTrackPoint> gpxptlst) {
         if (gpxptlst != null &&
             gpxptlst.Count > 0) {
            Gpx.GpxTrackPoint[] pt = new Gpx.GpxTrackPoint[gpxptlst.Count];
            for (int i = 0; i < gpxptlst.Count; i++) {
               pt[i] = new Gpx.GpxTrackPoint(gpxptlst[i]);
            }
            return pt;
         }
         return Array.Empty<Gpx.GpxTrackPoint>();
      }

      /// <summary>
      /// erzeugt den <see cref="Track"/> mit diesen Daten, fügt ihn aber noch nicht ihn die Trackliste in <see cref="GpxData"/> ein
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="trackno"></param>
      /// <param name="segmentno"></param>
      /// <param name="visualname"></param>
      /// <returns></returns>
      public static Track Create(GpxData gpx, int trackno, int segmentno, string visualname) {
         Track track = new Track(visualname) {
            GpxDataContainer = gpx,
            GpxTrack = gpx.Tracks[trackno],
            GpxSegment = gpx.Tracks[trackno].Segments[segmentno],
         };
         track.CalculateStats();
         track.Bounds = CalculateBounds(track.GpxSegment.Points);
         return track;
      }

      /// <summary>
      /// erzeugt eine Kopie des <see cref="Track"/>, fügt ihn aber noch nicht ihn die Trackliste in <see cref="GpxData"/> ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="destgpx"></param>
      /// <param name="useorgprops">bei false wird (wenn möglich) die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public static Track CreateCopy(Track orgtrack, GpxData? destgpx = null, bool useorgprops = false) {
         Track track = new Track(orgtrack.VisualName) {
            GpxDataContainer = destgpx,
            GpxTrack = new Gpx.GpxTrack(orgtrack.GpxTrack),      // vollständige Kopie,
         };
         track.GpxSegment = track.GpxTrack.Segments[0];
         if (destgpx != null && !useorgprops) {
            track.LineColor = destgpx.TrackColor;
            track.LineWidth = destgpx.TrackWidth;
         } else {
            track.LineColor = orgtrack.LineColor;
            track.LineWidth = orgtrack.LineWidth;
         }
         track.CalculateStats();
         track.Bounds = CalculateBounds(track.GpxSegment.Points);

         track.VisualName = orgtrack.VisualName;

         return track;
      }

      /// <summary>
      /// ermittelt die Boundingbox
      /// </summary>
      /// <param name="gpxpt"></param>
      /// <returns></returns>
      protected static Gpx.GpxBounds CalculateBounds(Gpx.ListTS<Gpx.GpxTrackPoint>? gpxpt) =>
         gpxpt == null || gpxpt.Count == 0 ?
                     new Gpx.GpxBounds(0, 0, 0, 0) :
                     new Gpx.GpxBounds(gpxpt);

      /// <summary>
      /// berechnet <see cref="Bounds"/> neu
      /// </summary>
      public void RefreshBoundingbox() {
         if (GpxTrack.Segments != null &&
             GpxTrack.Segments.Count > 0 &&
             GpxTrack.Segments[0].Points != null)
            Bounds = CalculateBounds(GpxTrack.Segments[0].Points);
      }

      /// <summary>
      /// berechnet stat. Daten (neu)
      /// </summary>
      public void CalculateStats() {
         double ascentdescentthreshold = 1;

         StatMinHeigth = double.MaxValue;
         StatMaxHeigth = double.MinValue;
         StatElevationUp = 0;
         StatElevationDown = 0;
         StatMinDateTime = DateTime.MaxValue;
         StatMaxDateTime = DateTime.MinValue;
         StatMinDateTimeIdx = -1;
         StatMaxDateTimeIdx = -1;
         StatLength = 0;
         StatLengthWithTime = 0;

         if (GpxSegment != null &&
             GpxSegment.Points != null) {
            Gpx.GpxTrackPoint[] ptlst = GpxSegment.Points.ToArray();
            FSofTUtils.Geography.GpxInfos.PointListInfo info = new FSofTUtils.Geography.GpxInfos.PointListInfo(ptlst, ascentdescentthreshold);

            StatMinHeigth = info.Minheight;
            StatMaxHeigth = info.Maxheight;
            StatElevationUp = info.Ascent;
            StatElevationDown = info.Descent;
            StatMinDateTime = info.FirstTime;
            StatMaxDateTime = info.LastTime;
            StatMinDateTimeIdx = info.FirstTimeIdx;
            StatMaxDateTimeIdx = info.LastTimeIdx;
            StatLength = info.Length;
            StatLengthWithTime = info.LengthWithTime;
         }
      }

      /// <summary>
      /// Länge einer Teilstrecke
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      /// <returns></returns>
      public double Length(int fromidx, int toidx) {
         double length = 0;
         if (GpxSegment != null) {
            if (fromidx < toidx) {
               fromidx = Math.Max(0, fromidx);
               Gpx.ListTS<Gpx.GpxTrackPoint> pt = GpxSegment.Points;
               for (int i = fromidx + 1; i <= toidx && i < GpxSegment.Points.Count; i++)
                  length += FSofTUtils.Geography.GeoHelper.Wgs84Distance(pt[i].Lon, 
                                                                         pt[i - 1].Lon, 
                                                                         pt[i].Lat, 
                                                                         pt[i - 1].Lat, 
                                                                         FSofTUtils.Geography.GeoHelper.Wgs84DistanceCompute.ellipsoid);
            }
         }
         return length;
      }

      public double Length() =>
         GpxSegment != null ? Length(0, GpxSegment.Points.Count - 1) : 0;

      /// <summary>
      /// Länge einer Teilstrecke (threadsicher)
      /// </summary>
      /// <param name="fromidx"></param>
      /// <param name="toidx"></param>
      /// <returns></returns>
      public double LengthTS(int fromidx, int toidx) {
         double length = 0;
         if (GpxSegment != null) {
            if (fromidx < toidx) {
               fromidx = Math.Max(0, fromidx);

               List<Gpx.GpxTrackPoint> pt = GpxSegment.Points.GetCopy();
               for (int i = fromidx + 1; i <= toidx && i < GpxSegment.Points.Count; i++)
                  length += FSofTUtils.Geography.GeoHelper.Wgs84Distance(pt[i].Lon, 
                                                                         pt[i - 1].Lon, 
                                                                         pt[i].Lat, 
                                                                         pt[i - 1].Lat,
                                                                         FSofTUtils.Geography.GeoHelper.Wgs84DistanceCompute.ellipsoid);
            }
         }
         return length;
      }

      public double LengthTS() => GpxSegment != null ? LengthTS(0, int.MaxValue) : 0;

      /// <summary>
      /// liefert die stat. Daten als Text
      /// </summary>
      /// <returns></returns>
      public string GetSimpleStatsText() {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine(VisualName);
         sb.AppendFormat("Länge: {0:F1} km ({1:F0} m)", StatLength / 1000, StatLength);
         sb.AppendLine();
         if (StatMinHeigth != double.MaxValue &&
             StatMaxHeigth != double.MinValue) {
            sb.AppendFormat("Höhe: {0:F0} m .. {1:F0} m", StatMinHeigth, StatMaxHeigth);
            if (StatElevationUp >= 0 && StatElevationDown >= 0)
               sb.AppendFormat(", Anstieg {0:F0} m, Abstieg {1:F0} m", StatElevationUp, StatElevationDown);
            sb.AppendLine();
         }
         sb.AppendFormat("Punkte: {0}", GpxSegment != null ? GpxSegment.Points.Count : 0);
         sb.AppendLine();
         if (StatMinDateTimeIdx >= 0 &&
             StatMaxDateTimeIdx > StatMinDateTimeIdx) {
            TimeSpan ts = StatMaxDateTime.Subtract(StatMinDateTime);
            sb.AppendFormat("Datum/Zeit: {0} .. {1} (Dauer: {2} Stunden)",
                            StatMinDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                            StatMaxDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                            ts.ToString(@"h\:mm\:ss"));
            sb.AppendLine();
            sb.AppendFormat("Durchschnittsgeschwindigkeit: {0:F1} km/h", StatLengthWithTime / ts.TotalSeconds * 3.6);
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// liefert den Index des Punktes der dem angegebenen Punkt am nächsten liegt (oder einen neg. Wert)
      /// </summary>
      /// <param name="geopt"></param>
      /// <returns></returns>
      public int GetNearestPtIdx(FSofTUtils.Geometry.PointD geopt) {
         int idx = -1;
         double dist = double.MaxValue;
         if (GpxSegment != null)
            for (int i = 0; i < GpxSegment.Points.Count; i++) {
               double d = FSofTUtils.Geography.GeoHelper.Wgs84Distance(GpxSegment.Points[i].Lon, 
                                                                       geopt.X, 
                                                                       GpxSegment.Points[i].Lat, 
                                                                       geopt.Y);
               if (d < dist) {
                  dist = d;
                  idx = i;
               }
            }
         return idx;
      }

      /// <summary>
      /// liefert den (originalen) GPX-Punkt
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Gpx.GpxTrackPoint? GetGpxPoint(int idx) => GpxSegment?.Points[idx];

      /// <summary>
      /// Anzeige aktualisieren (falls akt. sichtbar)
      /// </summary>
      public void Refresh() {
         if (VisualTrack != null && IsVisible)
            VisualTrack.Refresh();
      }

      #region Punktliste ändern

      /// <summary>
      /// ersetzt die Punkte des Tracks mit einer Kopie des gelieferten Tracks
      /// </summary>
      /// <param name="segment"></param>
      public void ReplaceAllPoints(Gpx.GpxTrackSegment segment) {
         GpxSegment.Points.Clear();
         GpxSegment.Points.AddRange(segment.Points);
      }

      /// <summary>
      /// den Punkt mit diesem Index entfernen
      /// </summary>
      /// <param name="idx"></param>
      public void RemovePoint(int idx) {
         if (GpxSegment != null && 0 <= idx && idx < GpxSegment.Points.Count) {
            GpxSegment.Points.RemoveAt(idx);
            if (VisualTrack != null)
               VisualTrack.Points.RemoveAt(idx);
         }
      }

      ///// <summary>
      ///// einen neuen Punkt an dieser Stelle einfügen
      ///// </summary>
      ///// <param name="idx"></param>
      ///// <param name="lat"></param>
      ///// <param name="lon"></param>
      ///// <param name="elevation"></param>
      //public void InsertPoint(int idx,
      //                        double lat,
      //                        double lon,
      //                        double elevation = Gpx.BaseElement.NOTVALID_DOUBLE) {
      //   Gpx.GpxTrackPoint newpt = new Gpx.GpxTrackPoint(lon, lat, elevation);
      //   if (GpxSegment != null && (idx < 0 || GpxSegment.Points.Count <= idx)) {
      //      GpxSegment.Points.Add(newpt);
      //      if (VisualTrack != null)
      //         VisualTrack.Points.Add(new GMap.NET.PointLatLng(newpt.Lat, newpt.Lon));
      //   } else {
      //      if (GpxSegment != null)
      //         GpxSegment.Points.Insert(idx, newpt);
      //      if (VisualTrack != null)
      //         VisualTrack.Points.Insert(idx, new GMap.NET.PointLatLng(newpt.Lat, newpt.Lon));
      //   }
      //}

      ///// <summary>
      ///// den Punkt an dieser Stelle verändern
      ///// </summary>
      ///// <param name="idx"></param>
      ///// <param name="lat"></param>
      ///// <param name="lon"></param>
      ///// <param name="elevation"></param>
      //public void ChangePoint(int idx,
      //                        double lat = Gpx.BaseElement.NOTVALID_DOUBLE,
      //                        double lon = Gpx.BaseElement.NOTVALID_DOUBLE,
      //                        double elevation = Gpx.BaseElement.NOTVALID_DOUBLE) {
      //   if (GpxSegment != null && (idx < 0 || idx >= GpxSegment.Points.Count)) {
      //      GpxSegment.Points[idx] = new Gpx.GpxTrackPoint(lat != Gpx.BaseElement.NOTVALID_DOUBLE ? lat : GpxSegment.Points[idx].Lat,
      //                                                         lon != Gpx.BaseElement.NOTVALID_DOUBLE ? lon : GpxSegment.Points[idx].Lon,
      //                                                         elevation != Gpx.BaseElement.NOTVALID_DOUBLE ? elevation : GpxSegment.Points[idx].Elevation);
      //      if (VisualTrack != null)
      //         VisualTrack.Points[idx] = new GMap.NET.PointLatLng(GpxSegment.Points[idx].Lat, GpxSegment.Points[idx].Lon);
      //   }
      //}

      /// <summary>
      /// änder die Trackrichtung
      /// </summary>
      public void ChangeDirection() {
         GpxSegment?.ChangeDirection();
         if (VisualTrack != null) {
            List<GMap.NET.PointLatLng> tmp = new List<GMap.NET.PointLatLng>();
            tmp.AddRange(VisualTrack.Points);
            VisualTrack.Points.Clear();
            for (int i = tmp.Count - 1; i >= 0; i--)
               VisualTrack.Points.Add(tmp[i]);
         }
      }

      #endregion

      /// <summary>
      /// berührt der <see cref="Track"/> in irgendeiner Weise das Rechteck?
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool IsCrossing(Gpx.GpxBounds rect) => GpxSegment != null && GpxSegment.Points != null ?
                                                         RouteCrossing.IsRouteCrossing(rect, GpxSegment.Points) :
                                                         false;

      /// <summary>
      /// liefert den <see cref="VisualTrack.VisualStyle"/> für den akt. Zustand des Tracks
      /// </summary>
      /// <returns></returns>
      VisualTrack.VisualStyle getVisualStyle() {
         VisualTrack.VisualStyle style =
            IsSelectedPart ?
               VisualTrack.VisualStyle.SelectedPart :
               IsOnEdit ?
                  VisualTrack.VisualStyle.InEdit :
                  IsMarked4Edit ?
                  VisualTrack.VisualStyle.Marked4Edit :
                     IsMarked ?
                        VisualTrack.VisualStyle.Marked :
                        IsOnLiveDraw ?
                           VisualTrack.VisualStyle.LiveDraw :
                           IsEditable ?
                              VisualTrack.VisualStyle.Editable :
                              VisualTrack.VisualStyle.Standard;

         if (style == VisualTrack.VisualStyle.Standard) {
            if (LineColor == VisualTrack.StandardColor) {

            } else if (LineColor == VisualTrack.StandardColor2) {
               style = VisualTrack.VisualStyle.Standard2;
            } else if (LineColor == VisualTrack.StandardColor3) {
               style = VisualTrack.VisualStyle.Standard3;
            } else if (LineColor == VisualTrack.StandardColor4) {
               style = VisualTrack.VisualStyle.Standard4;
            } else if (LineColor == VisualTrack.StandardColor5) {
               style = VisualTrack.VisualStyle.Standard5;
            }
         }
         return style;
      }

      /// <summary>
      /// <para>
      /// Falls der <see cref="VisualTrack"/> ex., wird der <see cref="VisualTrack.VisualStyle"/> 
      /// des Tracks für den akt. Zustand gesetzt.
      /// </para>
      /// <para>
      /// Nur wenn Track-Stil <see cref="VisualTrack.VisualStyle.Editable"/> oder StandardX ist, 
      /// wird die individuelle Farbe/Linienbreite verwendet.
      /// </para>
      /// </summary>
      void setStyle4VisualTrack() {
         //if (IsVisible) {
         VisualTrack.VisualStyle style = getVisualStyle();
         // Normalerweise erhält der VisualTrack die Farbe/Linienbreite die dem Track-Stil entspricht.
         // Nur wenn Track-Stil Editable oder StandardX ist, wird die individuelle Farbe/Linienbreite verwendet.

         if (VisualTrack != null) {
            if (VisualTrack.IsChangeableStyle(style))
               VisualTrack.SetVisualStyle(LineColor, LineWidth);
            else
               VisualTrack.SetVisualStyle(style);
         }
         //}
      }

      /// <summary>
      /// <see cref="VisualTrack"/> (neu) erzeugen
      /// </summary>
      /// <param name="mapControl">wenn ungleich null, dann auch anzeigen</param>
      public void UpdateVisualTrack(SpecialMapCtrl? mapControl = null) {
         bool visible = IsVisible;

         if (mapControl != null)
            mapControl.M_ShowTrack(this, false, null); // ev. vorhandenen VisualTrack aus dem Control entfernen
         else {
            //if (IsVisible)
            //   IsVisible = false;
            if (IsVisible &&
                VisualTrack != null &&
                VisualTrack.Overlay != null &&
                VisualTrack.Overlay.Tracks.Contains(VisualTrack)) // es ex. noch ein VisualTrack und dieser ist in einem Overlay enthalten
               VisualTrack.Overlay.Tracks.Remove(VisualTrack);
         }
         VisualTrack = new VisualTrack(this, VisualName, LineColor, LineWidth, getVisualStyle()); // neuen VisualTrack erzeugen

         if (mapControl != null &&
             visible) { // neuen VisualTrack anzeigen
            mapControl.M_ShowTrack(this,
                                    true,
                                    IsEditable && GpxDataContainer != null ?
                                             GpxDataContainer.NextVisibleTrack(this) :
                                             null);
         }
      }

      public override string ToString() {
         return string.Format("[Visualname={0}, IsVisible={1}, {2} points, Bounds={3}, LineWidth={4}, LineColor={5}]",
                              VisualName,
                              IsVisible,
                              GpxSegment != null ? GpxSegment.Points.Count : 0,
                              Bounds,
                              LineWidth,
                              LineColor.ToString());
      }

   }
}
