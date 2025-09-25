using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using GarminCore;

namespace GarminImageCreator.Garmin {
   /// <summary>
   /// zur Umrechnung der Geo-Koordinaten in Bitmap-Pixel
   /// </summary>
   class GeoConverter {

      readonly double LonBase;
      readonly double LatBase;

      /// <summary>
      /// lon je Pixel
      /// </summary>
      public readonly double DeltaLonPerPixel;

      /// <summary>
      /// lat je Pixel
      /// </summary>
      public readonly double DeltaLatPerPixel;

      protected double deltalonminsize;
      protected double deltalatminsize;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="lonbase">linker Rand des Bitmap</param>
      /// <param name="latbase">unterer Rand des Bitmap</param>
      /// <param name="deltalonperpixel">lon je Pixel</param>
      /// <param name="deltalatperpixel">lat je Pixel</param>
      public GeoConverter(double lonbase, double latbase, double deltalonperpixel, double deltalatperpixel) {
         LonBase = lonbase;
         LatBase = latbase;
         DeltaLonPerPixel = deltalonperpixel;
         DeltaLatPerPixel = deltalatperpixel;

         deltalonminsize = 3 * DeltaLonPerPixel;
         deltalatminsize = 3 * DeltaLatPerPixel;
      }

      float xpixel(double lon) {
         return (float)((lon - LonBase) / DeltaLonPerPixel);
      }

      float ypixel(double lat) {
         return (float)((lat - LatBase) / DeltaLatPerPixel);
      }

      public PointF Convert(double lon, double lat) {
         return new PointF(xpixel(lon), ypixel(lat));
      }

      public PointF Convert(GeoPoint pt) {
         return new PointF(xpixel(pt.Point.X), ypixel(pt.Point.Y));
      }

      public PointF[] Convert(GeoPoly poly) {
         if (poly.Points != null) {
            PointF[] pt = new PointF[poly.Points.Length];
            if (poly.DirectionIndicator) // Richtung umkehren
               for (int i = 0; i < pt.Length; i++) {
                  pt[pt.Length - 1 - i] = Convert(poly.Points[i].X, poly.Points[i].Y);
               }
            else
               for (int i = 0; i < pt.Length; i++) {
                  pt[i] = Convert(poly.Points[i].X, poly.Points[i].Y);
               }
            return removeNearPoints(pt);
         }
         return Array.Empty<PointF>();
      }

      const float MINDELTA4POINTS = 1.0F;

      /// <summary>
      /// Punkte, die "zu nah" beieinander liegen, werden entfernt
      /// </summary>
      /// <param name="pts"></param>
      /// <returns></returns>
      protected PointF[] removeNearPoints(PointF[] pts) {
         if (pts.Length < 3)
            return pts;

         List<bool> remove = new List<bool>(100) { false }; // 1. Punkt niemals löschen
         int needremove = 0;
         for (int s = 0, e = 1; e < pts.Length; e++) {
            // Wenn der Abstand zwischen P[s] und P[e] zu klein ist, wird P[e] entfernt.
            if (Math.Abs(pts[s].X - pts[e].X) < MINDELTA4POINTS &&
                Math.Abs(pts[s].Y - pts[e].Y) < MINDELTA4POINTS) {
               remove.Add(true);
               needremove++;
            } else {
               remove.Add(false);
               s++;
            }
         }
         if (remove[pts.Length - 1]) {
            remove[pts.Length - 1] = false;
            needremove--;
            if (!remove[pts.Length - 2]) {
               remove[pts.Length - 2] = true;
               needremove++;
            }
         }

         if (needremove == 0)
            return pts;
         else {
            if (needremove == pts.Length - 2 &&    // Linie mit 2 Punkten
                pts[0] == pts[pts.Length - 1])     // Anfangspunkt == Endpunkt
               return new PointF[0];

            PointF[] ptsnew = new PointF[pts.Length - needremove];
            for (int i = 0, j = 0; i < remove.Count; i++) {
               if (remove[i])
                  continue;
               ptsnew[j++] = pts[i];
            }
            return ptsnew;
         }
      }

      public bool HasMinSize(Bound bound) {
         return bound.WidthDegree >= deltalonminsize ||
                bound.HeightDegree >= deltalatminsize;
      }

      public override string ToString() {
         return string.Format("[LonBase={0}°, LatBase={1}°, DeltaLonPerPixel={2}°, DeltaLatPerPixel={3}°]",
                              LonBase,
                              LatBase,
                              DeltaLonPerPixel,
                              DeltaLatPerPixel);
      }

   }

}
