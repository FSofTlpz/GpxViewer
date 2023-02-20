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
      public GpxAllExt GpxDataContainer { get; protected set; } = null;

      /// <summary>
      /// Originaltrack aus dem <see cref="GpxDataContainer"/>
      /// </summary>
      public Gpx.GpxTrack GpxTrack { get; protected set; }

      /// <summary>
      /// Originalsegment aus dem <see cref="GpxTrack"/> aus dem <see cref="GpxDataContainer"/>
      /// </summary>
      public Gpx.GpxTrackSegment GpxSegment { get; protected set; }

      /// <summary>
      /// Boundingbox
      /// </summary>
      public Gpx.GpxBounds Bounds { get; protected set; }

      /// <summary>
      /// liefert den akt. Index der <see cref="Track"/> in <see cref="GpxAllExt"/>
      /// </summary>
      /// <returns></returns>
      public int GpxDataContainerIndex {
         get => GpxDataContainer != null ? GpxDataContainer.TrackList.IndexOf(this) : -1;
      }

      /// <summary>
      /// Name des (gesamten) Tracks
      /// </summary>
      public string Trackname {
         get => GpxTrack?.Name;
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
      public VisualTrack VisualTrack { get; protected set; }

      /// <summary>
      /// Ist der Track editierbar (wird, wenn vorhanden, vom <see cref="GpxDataContainer"/> vorgegeben, sonst true)?
      /// </summary>
      public bool IsEditable => GpxDataContainer == null || GpxDataContainer.GpxFileEditable;

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
                  setVisualStyle();
               }
            }
         }
      }

      bool _isMarked = false;
      /// <summary>
      /// <see cref="Track"/> ist in der Anzeige markiert
      /// </summary>
      public bool IsMarked {
         get => _isMarked;
         set {
            if (value != _isMarked) {
               _isMarked = value;
               setVisualStyle();
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
               setVisualStyle();
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
            if (_lineColor != value) {
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
               setVisualStyle();
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
               setVisualStyle();
            }
         }
      }

      #region statistische Daten

      public double StatMinHeigth { get; protected set; } = double.MaxValue;
      public double StatMaxHeigth { get; protected set; } = double.MinValue;
      public double StatElevationUp { get; protected set; } = 0;
      public double StatElevationDown { get; protected set; } = 0;
      public DateTime StatMinDateTime { get; protected set; } = DateTime.MaxValue;
      public int StatMinDateTimeIdx { get; protected set; } = -1;
      public DateTime StatMaxDateTime { get; protected set; } = DateTime.MinValue;
      public int StatMaxDateTimeIdx { get; protected set; } = -1;
      public double StatLength { get; protected set; } = 0;
      public double StatLengthWithTime { get; protected set; } = 0;

      #endregion


      /// <summary>
      /// erzeugt intern einen <see cref="Gpx.GpxTrack"/> mit 1 Segment ohne Punkte
      /// <para>Es gibt keinen Verweis auf einen <see cref="GpxAllExt"/>.</para>
      /// </summary>
      /// <param name="visualname"></param>
      public Track(string visualname) {
         VisualName = visualname;
         GpxTrack = new Gpx.GpxTrack();
         GpxTrack.Segments.Add(new Gpx.GpxTrackSegment());
      }

      /// <summary>
      /// erzeugt intern einen <see cref="Gpx.GpxTrack"/> mit 1 Segment und einer Kopie der Punkte
      /// <para>Es gibt keinen Verweis auf einen <see cref="GpxAllExt"/>.</para>
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
         return new Gpx.GpxTrackPoint[0];
      }

      /// <summary>
      /// erzeugt den <see cref="Track"/> mit diesen Daten, fügt ihn aber noch nicht ihn die Trackliste in <see cref="GpxAllExt"/> ein
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="trackno"></param>
      /// <param name="segmentno"></param>
      /// <param name="visualname"></param>
      /// <returns></returns>
      public static Track Create(GpxAllExt gpx, int trackno, int segmentno, string visualname) {
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
      /// erzeugt eine Kopie des <see cref="Track"/>, fügt ihn aber noch nicht ihn die Trackliste in <see cref="GpxAllExt"/> ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="destgpx"></param>
      /// <param name="useorgprops">bei false wird (wenn möglich) die Farbe vom Container verwendet</param>
      /// <returns></returns>
      public static Track CreateCopy(Track orgtrack, GpxAllExt destgpx = null, bool useorgprops = false) {
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
      public static Gpx.GpxBounds CalculateBounds(IList<Gpx.GpxTrackPoint> gpxpt) {
         if (gpxpt.Count == 0)
            return new Gpx.GpxBounds(0, 0, 0, 0);
         Gpx.GpxBounds bounds = new Gpx.GpxBounds(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
         for (int i = 0; i < gpxpt.Count; i++) {
            bounds.MinLat = Math.Min(bounds.MinLat, gpxpt[i].Lat);
            bounds.MinLon = Math.Min(bounds.MinLon, gpxpt[i].Lon);
            bounds.MaxLat = Math.Max(bounds.MaxLat, gpxpt[i].Lat);
            bounds.MaxLon = Math.Max(bounds.MaxLon, gpxpt[i].Lon);
         }
         return bounds;
      }

      /// <summary>
      /// berechnet <see cref="Bounds"/> neu
      /// </summary>
      public void RefreshBoundingbox() {
         Bounds = CalculateBounds(GpxTrack.Segments[0].Points);
      }

      /// <summary>
      /// berechnet stat. Daten (neu)
      /// </summary>
      public void CalculateStats() {
         if (GpxSegment != null) {
            List<Gpx.GpxTrackPoint> pt = GpxSegment.Points;

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

            int lastelevationidx = -1;
            for (int i = 0; i < pt.Count; i++) {
               if (pt[i].Elevation != Gpx.BaseElement.NOTVALID_DOUBLE) {
                  StatMinHeigth = Math.Min(StatMinHeigth, pt[i].Elevation);
                  StatMaxHeigth = Math.Max(StatMaxHeigth, pt[i].Elevation);
                  if (lastelevationidx >= 0) {
                     double delta = pt[i].Elevation - pt[lastelevationidx].Elevation;
                     if (delta > 0)
                        StatElevationUp += delta;
                     else
                        StatElevationDown += delta;
                  }
                  lastelevationidx = i;
               }

               if (pt[i].Time != Gpx.BaseElement.NOTVALID_TIME) {
                  if (StatMinDateTime > pt[i].Time) {
                     StatMinDateTime = pt[i].Time;
                     StatMinDateTimeIdx = i;
                  }
                  if (StatMaxDateTime < pt[i].Time) {
                     StatMaxDateTime = pt[i].Time;
                     StatMaxDateTimeIdx = i;
                  }
               }


               if (i > 0) {
                  StatLength += FSofTUtils.Geography.GeoHelper.Wgs84Distance(pt[i].Lon, pt[i - 1].Lon, pt[i].Lat, pt[i - 1].Lat);
               }
            }

            if (StatMinDateTimeIdx >= 0 &&
                StatMinDateTimeIdx < StatMaxDateTimeIdx) { // min. 1 Strecke
               for (int i = StatMinDateTimeIdx + 1; i <= StatMaxDateTimeIdx; i++) {
                  StatLengthWithTime += FSofTUtils.Geography.GeoHelper.Wgs84Distance(pt[i].Lon, pt[i - 1].Lon, pt[i].Lat, pt[i - 1].Lat);
               }
            }

         } else {    // dann nur aus den GMap.NET.PointLatLng-Punkten
            if (GpxSegment.Points != null) {
               StatLength = 0;
               for (int i = 1; i < GpxSegment.Points.Count; i++) {
                  StatLength += FSofTUtils.Geography.GeoHelper.Wgs84Distance(GpxSegment.Points[i].Lon, GpxSegment.Points[i - 1].Lon, GpxSegment.Points[i].Lat, GpxSegment.Points[i - 1].Lat);
               }
            }
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
            if (fromidx < toidx &&
                0 <= fromidx &&
                toidx < GpxSegment.Points.Count) {
               List<Gpx.GpxTrackPoint> pt = GpxSegment.Points;
               for (int i = fromidx + 1; i <= toidx; i++)
                  length += FSofTUtils.Geography.GeoHelper.Wgs84Distance(pt[i].Lon, pt[i - 1].Lon, pt[i].Lat, pt[i - 1].Lat);
            }
         }
         return length;
      }

      public double Length() => GpxSegment != null ? Length(0, GpxSegment.Points.Count - 1) : 0;

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
            if (StatElevationUp >= 0 && StatElevationDown <= 0)
               sb.AppendFormat(", Anstieg {0:F0} m, Abstieg {1:F0} m", StatElevationUp, -StatElevationDown);
            sb.AppendLine();
         }
         sb.AppendFormat("Punkte: {0}", GpxSegment.Points.Count);
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
         for (int i = 0; i < GpxSegment.Points.Count; i++) {
            double d = FSofTUtils.Geography.GeoHelper.Wgs84Distance(GpxSegment.Points[i].Lon, geopt.X, GpxSegment.Points[i].Lat, geopt.Y);
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
      public Gpx.GpxTrackPoint GetGpxPoint(int idx) {
         return GpxSegment?.Points[idx];
      }

      /// <summary>
      /// Anzeige aktualisieren (falls akt. sichtbar)
      /// </summary>
      public void Refresh() {
         if (IsVisible)
            VisualTrack.Refresh();
      }

      #region Punktliste ändern

      /// <summary>
      /// ersetzt die Punkte des Tracks mit einer Kopie der gelieferten Tracks
      /// </summary>
      /// <param name="segment"></param>
      public void ReplaceAllPoints(Gpx.GpxTrackSegment segment) {
         GpxSegment = new Gpx.GpxTrackSegment(segment);
      }

      /// <summary>
      /// den Punkt mit diesem Index entfernen
      /// </summary>
      /// <param name="idx"></param>
      public void RemovePoint(int idx) {
         if (0 <= idx && idx < GpxSegment.Points.Count) {
            GpxSegment.Points.RemoveAt(idx);
            if (VisualTrack != null)
               VisualTrack.Points.RemoveAt(idx);
         }
      }

      /// <summary>
      /// einen neuen Punkt an dieser Stelle einfügen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="lat"></param>
      /// <param name="lon"></param>
      /// <param name="elevation"></param>
      public void InsertPoint(int idx,
                              double lat,
                              double lon,
                              double elevation = Gpx.BaseElement.NOTVALID_DOUBLE) {
         Gpx.GpxTrackPoint newpt = new Gpx.GpxTrackPoint(lon, lat, elevation);
         if (idx < 0 || idx >= GpxSegment.Points.Count) {
            GpxSegment.Points.Add(newpt);
            if (VisualTrack != null)
               VisualTrack.Points.Add(new GMap.NET.PointLatLng(newpt.Lat, newpt.Lon));
         } else {
            GpxSegment.Points.Insert(idx, newpt);
            if (VisualTrack != null)
               VisualTrack.Points.Insert(idx, new GMap.NET.PointLatLng(newpt.Lat, newpt.Lon));
         }
      }

      /// <summary>
      /// den Punkt an dieser Stelle verändern
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="lat"></param>
      /// <param name="lon"></param>
      /// <param name="elevation"></param>
      public void ChangePoint(int idx,
                              double lat = Gpx.BaseElement.NOTVALID_DOUBLE,
                              double lon = Gpx.BaseElement.NOTVALID_DOUBLE,
                              double elevation = Gpx.BaseElement.NOTVALID_DOUBLE) {
         if (idx < 0 || idx >= GpxSegment.Points.Count) {
            GpxSegment.Points[idx] = new Gpx.GpxTrackPoint(lat != Gpx.BaseElement.NOTVALID_DOUBLE ? lat : GpxSegment.Points[idx].Lat,
                                                               lon != Gpx.BaseElement.NOTVALID_DOUBLE ? lon : GpxSegment.Points[idx].Lon,
                                                               elevation != Gpx.BaseElement.NOTVALID_DOUBLE ? elevation : GpxSegment.Points[idx].Elevation);
            if (VisualTrack != null)
               VisualTrack.Points[idx] = new GMap.NET.PointLatLng(GpxSegment.Points[idx].Lat, GpxSegment.Points[idx].Lon);
         }
      }

      /// <summary>
      /// änder die Trackrichtung
      /// </summary>
      public void ChangeDirection() {
         GpxSegment.ChangeDirection();
         if (VisualTrack != null) {
            List<GMap.NET.PointLatLng> tmp = new List<GMap.NET.PointLatLng>();
            tmp.AddRange(VisualTrack.Points);
            VisualTrack.Points.Clear();
            for (int i = tmp.Count - 1; i >= 0; i--)
               VisualTrack.Points.Add(tmp[i]);
         }
      }

      #endregion

      #region Schnittpunkt Segment - Rechteck

      /// <summary>
      /// berührt die <see cref="Track"/> in irgendeiner Weise das Rechteck?
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool IsCrossing(Gpx.GpxBounds rect) {
         Debug.WriteLine(string.Format("IsCrossing(): {0}", this));
         if (isoverlapping(rect, Bounds)) {  // sonst lohnt eine Untersuchung nicht
                                             // min. 1 Punkt innerhalb des Rechtecks?
            foreach (var pt in GpxSegment.Points) {
               if (rect.MinLon <= pt.Lon && pt.Lon <= rect.MaxLon &&
                   rect.MinLat <= pt.Lat && pt.Lat <= rect.MaxLat) {
                  Debug.WriteLine(string.Format("IsCrossing(): Point in rect: {0} / {1}", pt, rect));
                  return true;
               }
            }
            Debug.WriteLine(string.Format("IsCrossing(): no Point in rect: {0}", rect));

            // Schneidet eine Verbindung zwischen 2 Punkten das Rechteck?
            for (int i = 1; i < GpxSegment.Points.Count; i++) {
               if (iscrossing(GpxSegment.Points[i - 1].Lon,
                              GpxSegment.Points[i - 1].Lat,
                              GpxSegment.Points[i].Lon,
                              GpxSegment.Points[i].Lat,
                              rect.MinLon,
                              rect.MaxLon,
                              rect.MinLat,
                              rect.MaxLat)) {
                  Debug.WriteLine(string.Format("IsCrossing(): iscrossing: {0} / {1}, {2}", rect, GpxSegment.Points[i - 1], GpxSegment.Points[i]));
                  return true;
               }
            }
         } else
            Debug.WriteLine(string.Format("IsCrossing(): no overlapping: {0} / {1}", rect, Bounds));
         return false;
      }

      /// <summary>
      /// Überlappen sich die 2 Rechtecke?
      /// </summary>
      /// <param name="rect1"></param>
      /// <param name="rect2"></param>
      /// <returns></returns>
      bool isoverlapping(Gpx.GpxBounds rect1, Gpx.GpxBounds rect2) {
         return isoverlapping(rect1.MinLat, rect1.MaxLat, rect2.MinLat, rect2.MaxLat) &&
                isoverlapping(rect1.MinLon, rect1.MaxLon, rect2.MinLon, rect2.MaxLon);
      }

      /* 2 Strecken: 
       *    (vektoriell) mit Punkt P und P+R, 
       *                     sowie Q und Q+S
       * 
       * Schnittpunkt bei:  P + t*R = Q + u*S      (t 0..1 und u 0..1)
       * mit "x S"
       * (P + t*R) x S = (Q + u*S) x S
       * wegen S x S = 0:     
       *    t*(R x S) = (Q - P) x S
       * also
       *    t = (Q - P) x S / (R x S)
       * und analog mit "x R"         
       *    u = (P - Q) x R / (R x S)
       * 
       * Wenn R x S = 0 und (Q - P) x R = 0     => Strecken liegen auf einer gemeinsamen Gerade -> Überlappung testen
       * Wenn R x S = 0                         => parallel, also kein Schnittpunkt
       * Wenn R x S <> 0 und t 0..1 und u 0..1  => Schnittpunkt in P + t * R = Q + u * S
       * sonst                                  => nicht parallel aber auch kein Schnittpunkt
       */

      /* ABER!
       * Spezialfall mit achsenparallelen Seiten des Rechtsecks ist einfacher zu testen.
       * 
       * Strecke AB ((xa,ya),(xb,yb)) mit xa <= xb; Rechteck mit xl, xr, yo, und yu (xl <= xr, yu <= yo)
       * Streckengleichung:   y = ya + dy / dx * x       dx=xb-xa, dy=yb-ya
       * 
       * Test für senkrechte Seiten des Rechtecks:
       *    Sonderfall dx=0 -> wenn xa=xl bzw. xa=xr liegt die Strecke auf der gleichen Gerade wie die Rechteckseite -> Test Überlappung nötig -> ...
       *                       sonst kein Schnittpunkt
       *    sonst
       *       wenn   yu <= ya + dy / dx * (xl - xa) <= yo  (linke Seite)      dann Schnittpunkt
       *       wenn   yu <= ya + dy / dx * (xr - xa) <= yo  (rechte Seite)     dann Schnittpunkt
       *       
       * Test für waagrechte Seiten des Rechtecks:
       *    Sonderfall dy=0 -> wenn ya=yl bzw. ya=yr liegt die Strecke auf der gleichen Gerade wie die Rechteckseite -> Test Überlappung nötig -> ...
       *                       sonst kein Schnittpunkt
       *    sonst
       *       wenn   xl <= xa + (yu - ya) * dx / dy <= xr  (untere Seite)   dann Schnittpunkt
       *       wenn   xl <= xa + (yo - ya) * dx / dy <= xr  (obere Seite)    dann Schnittpunkt
       *       
       */

      /// <summary>
      /// 
      /// </summary>
      /// <param name="xa">x-Wert Punkt A der Strecke</param>
      /// <param name="ya">y-Wert Punkt A der Strecke</param>
      /// <param name="xb">x-Wert Punkt B der Strecke</param>
      /// <param name="yb">y-Wert Punkt B der Strecke</param>
      /// <param name="xl">x-Wert der linken Rechteckseite</param>
      /// <param name="xr">x-Wert der rechten Rechteckseite</param>
      /// <param name="yu">y-Wert der unteren Rechteckseite</param>
      /// <param name="yo">y-Wert der oberen Rechteckseite</param>
      /// <returns>true wenn Schnittpunkt/Berührungspunkt</returns>
      bool iscrossing(double xa, double ya, double xb, double yb, double xl, double xr, double yu, double yo) {
         if (xa > xb) {
            swap(ref xa, ref xb);
            swap(ref ya, ref yb);
         }
         if (xl > xr)
            swap(ref xl, ref xr);
         if (yu > yo)
            swap(ref yu, ref yo);

         double dx = xb - xa;
         double dy = yb - ya;

         // senkrechte Rechteckseiten
         if (dx == 0) { // Sonderfall
            if (xa == xl || xa == xr) { // ev. Überlappung
               return isoverlapping(yu, yo, ya, yb);
            }
         } else {
            if (xa <= xl && xl <= xb) {
               double y = ya + dy / dx * (xl - xa);
               if (yu <= y && y <= yo)
                  return true;            // Schnittpunkt mit linker Seite
            }
            if (xa <= xr && xr <= xb) {
               double y = ya + dy / dx * (xr - xa);
               if (yu <= y && y <= yo)
                  return true;            // Schnittpunkt mit rechter Seite
            }
         }

         // waagerechte Rechteckseiten
         if (dy == 0) { // Sonderfall
            if (ya == yu || ya == yo) { // ev. Überlappung
               return isoverlapping(xl, xr, xa, xb);
            }
         } else {
            if ((ya <= yu && yu <= yb) ||
                (yb <= yu && yu <= ya)) {
               double x = xa + (yu - ya) * dx / dy;
               if (xl <= x && x <= xr)
                  return true;            // Schnittpunkt mit unterer Seite
            }
            if ((ya <= yo && yo <= yb) ||
                (yb <= yo && yo <= ya)) {
               double x = xa + (yo - ya) * dx / dy;
               if (xl <= x && x <= xr)
                  return true;            // Schnittpunkt mit oberer Seite
            }
         }

         return false;
      }

      /// <summary>
      /// Austausch der Werte
      /// </summary>
      /// <param name="v1"></param>
      /// <param name="v2"></param>
      void swap(ref double v1, ref double v2) {
         double tmp = v1;
         v1 = v2;
         v2 = tmp;
      }

      /// <summary>
      /// Überlappen sich die 2 Bereiche?
      /// </summary>
      /// <param name="va1">Wert 1 Bereich A</param>
      /// <param name="va2">Wert 2 Bereich A</param>
      /// <param name="vb1">Wert 1 Bereich B</param>
      /// <param name="vb2">Wert 2 Bereich B</param>
      /// <returns></returns>
      bool isoverlapping(double va1, double va2, double vb1, double vb2) {
         if (va1 > va2)
            swap(ref va1, ref va2);
         if (vb1 > vb2)
            swap(ref vb1, ref vb2);
         return vb2 >= va1 && vb1 <= va2;
      }

      #endregion

      /// <summary>
      /// liefert den <see cref="VisualTrack.VisualStyle"/> für den akt. Zustand
      /// </summary>
      /// <returns></returns>
      VisualTrack.VisualStyle getVisualStyle() {
         VisualTrack.VisualStyle style = IsSelectedPart ?
               VisualTrack.VisualStyle.SelectedPart :
               IsOnEdit || IsMarked4Edit ?
                  VisualTrack.VisualStyle.InEdit :
                  IsMarked ?
                     VisualTrack.VisualStyle.Marked :
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
      /// falls der <see cref="VisualTrack"/> ex. und sichtbar ist, wird der <see cref="VisualTrack.VisualStyle"/> für den akt. Zustand gesetzt
      /// </summary>
      void setVisualStyle() {
         if (IsVisible) {
            VisualTrack.VisualStyle style = getVisualStyle();
            if ((style == VisualTrack.VisualStyle.Standard &&        // nur wenn der Stil änderbar ist und die Daten nicht dem jeweiligen Stil entsprechen ...
                 (LineColor != VisualTrack.StandardColor ||
                  LineWidth != VisualTrack.StandardWidth)) ||

                (style == VisualTrack.VisualStyle.Standard2 &&
                 (LineColor != VisualTrack.StandardColor2 ||
                  LineWidth != VisualTrack.StandardWidth)) ||

                (style == VisualTrack.VisualStyle.Standard3 &&
                 (LineColor != VisualTrack.StandardColor3 ||
                  LineWidth != VisualTrack.StandardWidth)) ||

                (style == VisualTrack.VisualStyle.Standard4 &&
                 (LineColor != VisualTrack.StandardColor4 ||
                  LineWidth != VisualTrack.StandardWidth)) ||

                (style == VisualTrack.VisualStyle.Standard5 &&
                 (LineColor != VisualTrack.StandardColor5 ||
                  LineWidth != VisualTrack.StandardWidth)) ||

               (style == VisualTrack.VisualStyle.Editable &&
                 (LineColor != VisualTrack.EditableColor ||
                  LineWidth != VisualTrack.EditableWidth))) {
               VisualTrack.SetVisualStyle(LineColor, LineWidth);
            } else
               VisualTrack.SetVisualStyle(style);
         }
      }

      /// <summary>
      /// <see cref="VisualTrack"/> (neu) erzeugen
      /// </summary>
      /// <param name="mapControl">wenn ungleich null, dann auch anzeigen</param>
      public void UpdateVisualTrack(SpecialMapCtrl mapControl = null) {
         bool visible = IsVisible;

         if (mapControl != null)
            mapControl.SpecMapShowTrack(this, false, null); // ev. vorhandenen VisualTrack aus dem Control entfernen

         VisualTrack = new VisualTrack(this, VisualName, LineColor, LineWidth, getVisualStyle()); // neuen VisualTrack erzeugen

         if (mapControl != null &&
             visible) { // neuen VisualTrack anzeigen
            mapControl.SpecMapShowTrack(this,
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
                              GpxSegment.Points.Count,
                              Bounds,
                              LineWidth,
                              LineColor.ToString());
      }

   }
}
