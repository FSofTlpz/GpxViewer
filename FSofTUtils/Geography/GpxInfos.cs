using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FSofTUtils.Geography {
   public class GpxInfos {

      public class PointListInfo {

         /// <summary>
         /// Anzahl der Punkte im Segment
         /// </summary>
         public readonly int PointCount;

         /// <summary>
         /// Länge in m
         /// </summary>
         public readonly double Length;

         /// <summary>
         /// Länge für den Zeitbereich <see cref="FirstTime"/> bis <see cref="LastTime"/>
         /// </summary>
         public readonly double LengthWithTime;

         /// <summary>
         /// niedrigste Höhe in m
         /// </summary>
         public readonly double Minheight;

         /// <summary>
         /// höchste Höhe in m
         /// </summary>
         public readonly double Maxheight;

         /// <summary>
         /// Gesamtabstieg in m
         /// </summary>
         public readonly double Descent;

         /// <summary>
         /// Gesamtanstieg in m
         /// </summary>
         public readonly double Ascent;

         /// <summary>
         /// Durchschnittsgeschwindigkeit in m/s für den Bereich <see cref="FirstTime"/> bis <see cref="LastTime"/>
         /// </summary>
         public readonly double AverageSpeed;

         /// <summary>
         /// 1. bekannte Zeit
         /// </summary>
         public readonly DateTime FirstTime;

         /// <summary>
         /// letzte bekannte Zeit
         /// </summary>
         public readonly DateTime LastTime;

         /// <summary>
         /// Index zur 1. bekannte Zeit (oder kleiner 0)
         /// </summary>
         public readonly int FirstTimeIdx;

         /// <summary>
         /// Index zur letzten bekannte Zeit (oder kleiner 0)
         /// </summary>
         public readonly int LastTimeIdx;

         /// <summary>
         /// linke Seite des umschließenden Rechtecks (geograf. Länge)
         /// </summary>
         public readonly double Left;

         /// <summary>
         /// obere Seite des umschließenden Rechtecks (geograf. Breite)
         /// </summary>
         public readonly double Top;

         /// <summary>
         /// rechte Seite des umschließenden Rechtecks (geograf. Länge)
         /// </summary>
         public readonly double Right;

         /// <summary>
         /// untere Seite des umschließenden Rechtecks (geograf. Breite)
         /// </summary>
         public readonly double Bottom;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="segment"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von <see cref="Ascent"/> und <see cref="Descent"/></param>
         public PointListInfo(IList<GpxTrackPoint>? segment, double ascentdescentthreshold = 0) {
            if (segment != null) {
               PointCount = segment.Count;
               Length = getLengthAndMinMaxHeight(segment,
                                                 ascentdescentthreshold,
                                                 out Minheight,
                                                 out Maxheight,
                                                 out Descent,
                                                 out Ascent,
                                                 out FirstTime,
                                                 out LastTime,
                                                 out AverageSpeed,
                                                 out LengthWithTime,
                                                 out FirstTimeIdx,
                                                 out LastTimeIdx);
               GpxBounds bound = new(segment);
               Left = bound.MinLon;
               Top = bound.MaxLat;
               Right = bound.MaxLon;
               Bottom = bound.MinLat;
            }
         }

         /// <summary>
         /// liefert die Länge, die sich aus einem Teil der Punktliste ergibt
         /// </summary>
         /// <param name="pt"></param>
         /// <param name="startidx">Index des 1. Punktes</param>
         /// <param name="count">Länge des Listenteiles (i.A. min. 2 Punkte)</param>
         /// <returns></returns>
         public static double GetLength(IList<GpxTrackPoint> pt, int startidx = 0, int count = -1) {
            count = Math.Min(count, pt.Count - startidx);
            if (count < 0)
               count = pt.Count - startidx;
            double length = 0;
            for (int p = startidx + 1; p < startidx + count; p++)
               length += PointDistance(pt[p - 1], pt[p]);
            return length;
         }

         /// <summary>
         /// Punktabstand
         /// </summary>
         /// <param name="p1"></param>
         /// <param name="p2"></param>
         /// <returns></returns>
         public static double PointDistance(GpxTrackPoint p1, GpxTrackPoint p2) =>
              GeoHelper.Wgs84Distance(p1.Lon, p2.Lon, p1.Lat, p2.Lat, GeoHelper.Wgs84DistanceCompute.ellipsoid);

         static bool isValidValue(double v) => v != BaseElement.NOTVALID_DOUBLE && v != BaseElement.NOTUSE_DOUBLE;

         static bool isValidValue(DateTime v) => v != BaseElement.NOTVALID_TIME && v != BaseElement.NOTUSE_TIME;

         /// <summary>
         /// liefert die Länge und die min. und max. Höhe der Punktliste (falls vorhanden, sonst double.MaxValue bzw. double.MinValue)
         /// </summary>
         /// <param name="pt"></param>
         /// <param name="ascentdescentthreshold"></param>
         /// <param name="minheight"></param>
         /// <param name="maxheight"></param>
         /// <param name="descent"></param>
         /// <param name="ascent"></param>
         /// <param name="averagespeed">Durchschnittsgeschwindigkeit in m/s</param>
         /// <returns></returns>
         static double getLengthAndMinMaxHeight(IList<GpxTrackPoint> pt,
                                                double ascentdescentthreshold,
                                                out double minheight,
                                                out double maxheight,
                                                out double descent,
                                                out double ascent,
                                                out DateTime firsttime,
                                                out DateTime lasttime,
                                                out double averagespeed,
                                                out double lengthwithtime,
                                                out int firsttimeidx,
                                                out int lasttimeidx) {
            minheight = double.MaxValue;
            maxheight = double.MinValue;
            descent = 0;
            ascent = 0;
            averagespeed = 0;
            firsttime = DateTime.MinValue;
            lasttime = DateTime.MinValue;
            firsttimeidx = lasttimeidx = -1;
            lengthwithtime = 0;
            double lastheigth = BaseElement.NOTVALID_DOUBLE;

            double length = 0;
            if (pt.Count > 0) {
               if (isValidValue(pt[0].Elevation)) {
                  minheight = Math.Min(pt[0].Elevation, minheight);
                  maxheight = Math.Max(pt[0].Elevation, maxheight);
                  lastheigth = pt[0].Elevation;
               }

               if (isValidValue(pt[0].Time))
                  firsttimeidx = lasttimeidx = 0;

               for (int p = 1; p < pt.Count; p++) {
                  if (firsttimeidx < 0 && isValidValue(pt[p].Time))
                     firsttimeidx = lasttimeidx = p;
                  else {
                     if (isValidValue(pt[p].Time))
                        lasttimeidx = p;
                  }

                  if (isValidValue(pt[p].Elevation)) {
                     minheight = Math.Min(pt[p].Elevation, minheight);
                     maxheight = Math.Max(pt[p].Elevation, maxheight);
                     double diff = 0;
                     if (!isValidValue(lastheigth))
                        lastheigth = pt[p].Elevation;
                     else
                        diff = pt[p].Elevation - lastheigth;
                     if (Math.Abs(diff) >= ascentdescentthreshold) {
                        if (diff > 0)
                           ascent += diff;
                        else
                           descent -= diff;
                        lastheigth = pt[p].Elevation;
                     }
                  }
               }

               length = GetLength(pt);

               if (firsttimeidx >= 0 &&
                   lasttimeidx >= 0) {
                  firsttime = pt[firsttimeidx].Time;
                  lasttime = pt[lasttimeidx].Time;
                  TimeSpan ts = pt[lasttimeidx].Time.Subtract(pt[firsttimeidx].Time);
                  lengthwithtime = GetLength(pt, firsttimeidx, lasttimeidx);
                  averagespeed = lengthwithtime / ts.TotalSeconds;
               }

            }
            return length;
         }

         public override string ToString() => "Punkte " + PointCount;

      }


      public class SegmentInfo : PointListInfo {

         /// <summary>
         /// 
         /// </summary>
         /// <param name="segment"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von <see cref="Ascent"/> und <see cref="Descent"/></param>
         public SegmentInfo(IList<GpxTrackPoint>? segment, double ascentdescentthreshold = 0) :
            base(segment, ascentdescentthreshold) { }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="segment"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von <see cref="Ascent"/> und <see cref="Descent"/></param>
         public SegmentInfo(GpxTrackSegment? segment, double ascentdescentthreshold = 0) :
            base(segment?.Points.GetCopy(), ascentdescentthreshold) { }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="gpx"></param>
         /// <param name="trackno"></param>
         /// <param name="segmentno"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von <see cref="Ascent"/> und <see cref="Descent"/></param>
         public SegmentInfo(GpxAll gpx, int trackno, int segmentno, double ascentdescentthreshold = 0) :
            base(gpx?.Tracks[trackno].Segments[segmentno].Points.GetCopy(), ascentdescentthreshold) { }

         public override string ToString() => "Punkte " + PointCount;

      }

      public class TrackInfo {

         public readonly string Trackname = string.Empty;

         public readonly int SegmentCount;

         public readonly SegmentInfo[] Segment = [];


         /// <summary>
         /// 
         /// </summary>
         /// <param name="gpx"></param>
         /// <param name="trackno"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von An- und Abstieg</param>
         public TrackInfo(GpxAll? gpx, int trackno, double ascentdescentthreshold = 0) :
            this(gpx?.GetTrack(trackno), ascentdescentthreshold) { }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="track"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von An- und Abstieg</param>
         public TrackInfo(GpxTrack? track, double ascentdescentthreshold = 0) {
            if (track != null) {
               Trackname = track.Name;
               SegmentCount = track.Segments.Count;
               Segment = new SegmentInfo[SegmentCount];
               for (int i = 0; i < SegmentCount; i++)
                  Segment[i] = new SegmentInfo(track.Segments[i], ascentdescentthreshold);
            }
         }


         public override string ToString() => Trackname + " (Segmente " + SegmentCount + ")";

      }

      /// <summary>
      /// gesammelte Infos zu einer GPX-Datei (i.A. nur für die Anzeige bestimmt)
      /// </summary>
      public class GpxInfo {

         public readonly int WaypointCount;

         public readonly string[] Waypointname;

         public readonly int RouteCount;

         public readonly string[] Routename;

         public readonly int TrackCount;

         public readonly TrackInfo[] Tracks;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="gpx"></param>
         /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von An- und Abstieg</param>
         /// <param name="multitasking"></param>
         public GpxInfo(GpxAll gpx,
                        double ascentdescentthreshold = 1.0,
                        bool multitasking = false) {
            WaypointCount = gpx.Waypoints.Count;

            Waypointname = new string[WaypointCount];
            for (int i = 0; i < WaypointCount; i++)
               Waypointname[i] = gpx.Waypoints[i].Name;

            RouteCount = gpx.Routes.Count;

            Routename = new string[RouteCount];
            for (int i = 0; i < RouteCount; i++)
               Routename[i] = gpx.Routes[i].Name;

            TrackCount = gpx.Tracks.Count;

            Tracks = new TrackInfo[TrackCount];

            if (!multitasking || TrackCount <= 1) {

               for (int i = 0; i < TrackCount; i++)
                  Tracks[i] = new TrackInfo(gpx, i, ascentdescentthreshold);

            } else {

               TrackInfo[] tmptrackinfo = new TrackInfo[TrackCount];

               TaskQueue tq = new();
               IProgress<string> progress = new Progress<string>(TaskProgress4TrackInfo);
               for (int i = 0; i < tmptrackinfo.Length; i++)
                  tq.StartTask(gpx, tmptrackinfo, i, tmptrackinfo.Length, ascentdescentthreshold, TaskWorker4TrackInfo, progress, null);
               tq.Wait4EmptyQueue();

               for (int i = 0; i < tmptrackinfo.Length; i++)
                  Tracks[i] = tmptrackinfo[i];

            }
         }

         #region Trackinfos ermitteln mit Multitasking

         static int TaskWorker4TrackInfo(GpxAll gpx,
                                         TrackInfo[] trackinfos,
                                         int idx,
                                         int gesamt,
                                         double ascentdescentthreshold,
                                         CancellationToken tokencancel,
                                         IProgress<string>? progress) {
            ArgumentNullException.ThrowIfNull(trackinfos);

            TaskProgress4TrackInfo(string.Format("ermittle Infos zu Track {0} von {1} ...", idx + 1, gesamt));
            trackinfos[idx] = new TrackInfo(gpx, idx, ascentdescentthreshold);
            return 0;
         }

         static void TaskProgress4TrackInfo(string txt) => Console.Error.WriteLine(txt);

         #endregion

         public override string ToString() => string.Format("{0} Waypoints, {1} Routen, {2} Tracks", WaypointCount, RouteCount, TrackCount);

      }
   }
}
