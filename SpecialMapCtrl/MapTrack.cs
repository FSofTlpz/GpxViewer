//#define ORGUPDATEGRAPHICSPATH
//#define ONEVISUALPART

using GMap.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpecialMapCtrl {

   /// <summary>
   /// Track
   /// </summary>
   public class MapTrack : MapRoute, IDisposable {

      /*
       *    Points            geograf. Daten
       *    LocalPolyline     Polyline in Client-Koordinaten (wird bei jedem Verschieben und jeder Zoomänderung neu berechnet)
       *    visualPolylines   Array der sichtbaren Teillinien in Client-Koordinaten (wird bei jedem Verschieben und 
       *                      jeder Zoomänderung neu berechnet)
       */

      public class SpecialCapDrawEventArgs {

         public readonly Graphics Graphics;

         public readonly Color Color;

         public readonly float Linewidth;

         public readonly float FromX;

         public readonly float FromY;

         public readonly float EndX;

         public readonly float EndY;

         public readonly bool IsStartCap;

         public SpecialCapDrawEventArgs(Graphics graphics,
                                        Color color,
                                        float linewidth,
                                        float fromX,
                                        float fromY,
                                        float endX,
                                        float endY,
                                        bool isStartCap) {
            Graphics = graphics;
            Color = color;
            Linewidth = linewidth;
            FromX = fromX;
            FromY = fromY;
            EndX = endX;
            EndY = endY;
            IsStartCap = isStartCap;
         }

      }

      /// <summary>
      /// ein speziellen Symbol für ein Linienende soll gezeichnet werden
      /// </summary>
      public event EventHandler<SpecialCapDrawEventArgs>? SpecialCapDrawEvent;


      public MapOverlay? Overlay { get; internal set; }

      private bool _visible = true;

      /// <summary>
      /// is marker visible
      /// </summary>
      public bool IsVisible {
         get => _visible;
         set {
            if (value != _visible) {
               _visible = value;

               if (Overlay != null &&
                   Overlay.Control != null) {
                  if (_visible)
                     Overlay.Control.M_UpdateTrackLocalPosition(this);
                  else {
                     if (Overlay.Control.M_IsMouseOverTrack) {
                        Overlay.Control.M_IsMouseOverTrack = false;
                        Overlay.Control.M_RestoreCursorOnLeave();
                     }
                  }
                  if (!Overlay.Control.M_HoldInvalidation)
                     Overlay.Control.M_CoreInvalidate();
               }
            }
         }
      }

      /// <summary>
      /// can receive input
      /// </summary>
      public bool IsHitTestVisible = false;

      /// <summary>
      /// is mouse over
      /// </summary>
      public bool IsMouseOver { get; internal set; }

      public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
      public Pen Stroke = DefaultStroke;

      protected internal Point[] LocalPolyline = [];


      //class IntersectLineAndRectangle {

      //   public static bool Check(PointF a, PointF b, RectangleF rect) {
      //      return Check(a.X, a.Y, b.X, b.Y, rect.Left, rect.Bottom, rect.Right, rect.Top);
      //   }

      //   public static bool Check(float ax, float ay, float bx, float by, float left, float top, float right, float bottom) {
      //      if (ax <= bx) {
      //         if (intersectVerticalLine(ax, ay, bx, by, left, bottom, top))
      //            return true;
      //         if (intersectVerticalLine(ax, ay, bx, by, right, bottom, top))
      //            return true;
      //      } else {
      //         if (intersectVerticalLine(bx, by, ax, ay, left, bottom, top))
      //            return true;
      //         if (intersectVerticalLine(bx, by, ax, ay, right, bottom, top))
      //            return true;
      //      }

      //      if (ay <= by) {
      //         if (intersectHorizontalLine(ax, ay, bx, by, bottom, left, right))
      //            return true;
      //         if (intersectHorizontalLine(ax, ay, bx, by, top, left, right))
      //            return true;
      //      } else {
      //         if (intersectHorizontalLine(bx, by, ax, ay, bottom, left, right))
      //            return true;
      //         if (intersectHorizontalLine(bx, by, ax, ay, top, left, right))
      //            return true;
      //      }
      //      return false;
      //   }

      //   static bool intersectVerticalLine(float leftx, float lefty, float rightx, float righty, float axisx, float bottom, float top) {
      //      if (leftx == rightx) {
      //         if (leftx == axisx)
      //            if (Math.Min(lefty, righty) <= top &&
      //                Math.Max(lefty, righty) >= bottom)
      //               return true;
      //      } else {
      //         if (leftx <= axisx && axisx <= rightx) {
      //            float y = lefty + (axisx - leftx) * (righty - lefty) / (rightx - leftx);
      //            return bottom <= y && y <= top;
      //         }
      //      }
      //      return false;
      //   }

      //   static bool intersectHorizontalLine(float bottomx, float bottomy, float topx, float topy, float axisy, float left, float right) {
      //      if (bottomy == topy) {
      //         if (bottomy == axisy)
      //            if (Math.Min(bottomx, topx) <= right &&
      //                Math.Max(bottomx, topx) >= left)
      //               return true;
      //      } else {
      //         if (bottomy <= axisy && axisy <= topy) {
      //            float x = bottomx + (axisy - bottomy) * (topx - bottomx) / (topy - bottomy);
      //            return left <= x && x <= right;
      //         }
      //      }
      //      return false;
      //   }
      //}

      /// <summary>
      /// sichtbare Teile des Tracks in local/client Koordinaten (Punktlisten sinnvoll wegen DrawLines())
      /// <para>Die Punkte sind nur Verweise auf die Punkte in <see cref="LocalPolyline"/>.</para>
      /// </summary>
      protected List<Point[]> visualPolylines = new List<Point[]>();

      /// <summary>
      /// sichtbare Teile des Tracks (Startindex und Länge)
      /// </summary>
      protected List<(int, int)> visualParts = new List<(int, int)>();

      /// <summary>
      /// is first pt visible
      /// </summary>
      bool startptvisible = true;

      /// <summary>
      /// is last pt visible
      /// </summary>
      bool endptvisible = true;

      double minlat = double.MaxValue, maxlat = double.MinValue,
             minlon = double.MaxValue, maxlon = double.MinValue;


      static MapTrack() {
         DefaultStroke.LineJoin = LineJoin.Round;
         DefaultStroke.Width = 5;
      }

      public MapTrack(IEnumerable<PointLatLng> points, string name)
          : base(points, name) {
         CalculateLatLonBoundings();
      }

      public virtual void OnRender(Graphics g) {
         if (IsVisible &&
             visualPolylines.Count > 0) {

            for (int i = 0; i < visualPolylines.Count; i++) {
               Point[] part = visualPolylines[i];
               if (part.Length > 1)
                  g.DrawLines(Stroke, part);    // Array-Länge 1 löst intern eine Exception aus!

               //Debug.WriteLine("TRACK " + Name + ", Stroke " + Stroke.Color + ", " + part[0] + ", " + part[1] + ", " + part[2]);

               if (part.Length > 1) {
                  if (startptvisible &&
                      i == 0)
                     OnSpecialCapDraw(g,
                                      Stroke.Color,
                                      Stroke.Width,
                                      part[1].X,
                                      part[1].Y,
                                      part[0].X,
                                      part[0].Y,
                                      true);

                  if (endptvisible &&
                      i == visualPolylines.Count - 1)
                     OnSpecialCapDraw(g,
                                      Stroke.Color,
                                      Stroke.Width,
                                      part[part.Length - 2].X,
                                      part[part.Length - 2].Y,
                                      part[part.Length - 1].X,
                                      part[part.Length - 1].Y,
                                      false);
               }
            }
         }
      }

      public void CalculateLatLonBoundings() {
         minlat = double.MaxValue;
         maxlat = double.MinValue;
         minlon = double.MaxValue;
         maxlon = double.MinValue;
         foreach (var item in Points) {
            if (item.Lat < minlat)
               minlat = item.Lat;
            if (maxlat < item.Lat)
               maxlat = item.Lat;
            if (item.Lng < minlon)
               minlon = item.Lng;
            if (maxlon < item.Lng)
               maxlon = item.Lng;
         }
      }

      //public bool BoundingsIntersects(double fromlat, double tolat, double fromlon, double tolon) {
      //   // if rectangle has area 0, no overlap
      //   if (minlat == maxlat || minlon == maxlon || tolat == fromlat || fromlon == tolon)
      //      return false;

      //   // If one rectangle is on left side of other
      //   if (minlat > tolat || fromlat > maxlat)
      //      return false;

      //   // If one rectangle is above other
      //   if (maxlon > fromlon || tolon > minlon)
      //      return false;

      //   return true;
      //}

      public bool BoundingsIntersects(double fromlat, double tolat, double fromlon, double tolon) {
         if (fromlat < maxlat &&
             minlat < tolat &&
             fromlon < maxlon)
            return minlon < tolon;
         return false;
      }




      protected virtual void OnSpecialCapDraw(Graphics graphics,
                                              Color color,
                                              float linewidth,
                                              float fromX,
                                              float fromY,
                                              float endX,
                                              float endY,
                                              bool isStartCap) {
         SpecialCapDrawEvent?.Invoke(this, new SpecialCapDrawEventArgs(graphics,
                                                                       color,
                                                                       linewidth,
                                                                       fromX,
                                                                       fromY,
                                                                       endX,
                                                                       endY,
                                                                       isStartCap));
      }

      /// <summary>
      /// actual visible on client
      /// </summary>
      public bool IsOnClientVisible => visualParts.Count > 0;

      ///// <summary>
      ///// Indicates whether the specified point is contained within this <see cref="visualPolylines"/>
      ///// </summary>
      ///// <param name="x"></param>
      ///// <param name="y"></param>
      ///// <returns></returns>
      //internal bool IsInside(int x, int y, float tolerance = 1F) {
      //   if (visualPolylines.Count > 0) {
      //      tolerance = Math.Max(1F, tolerance);
      //      RectangleF rect = new RectangleF(x - tolerance * Stroke.Width / 2,
      //                                       y - tolerance * Stroke.Width / 2,
      //                                       tolerance * Stroke.Width / 2,
      //                                       tolerance * Stroke.Width / 2);
      //      foreach (Point[] part in visualPolylines) {
      //         if (rect.Contains(part[0]))
      //            return true;
      //         for (int i = 0; i < part.Length - 1; i++) {
      //            if (rect.Contains(part[i + 1]))
      //               return true;
      //            if (IntersectLineAndRectangle.Check(part[i], part[i + 1], rect))
      //               return true;
      //         }
      //      }
      //   }
      //   return false;
      //}

      /// <summary>
      /// Indicates if intersect a part of <see cref="visualPolylines"/> the circle around the specified point
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="radius">Radius für den Kreis um (x,y)</param>
      /// <returns></returns>
      internal bool IsInside(int x, int y, float radius) {
         if (visualPolylines.Count > 0) {
            PointF circle = new PointF(x, y);
            foreach (Point[] part in visualPolylines)
               for (int i = 0; i < part.Length - 1; i++)
                  if (intersectLineAndCircle(circle, radius, part[i], part[i + 1]))
                     return true;
         }
         return false;
      }

      /// <summary>
      /// schneidet oder berührt die Strecke AB den Kreis
      /// </summary>
      /// <param name="pc">Kreismittelpunkt</param>
      /// <param name="radius">Kreisradius</param>
      /// <param name="pa">Punkt A der Strecke</param>
      /// <param name="pb">Punkt B der Strecke</param>
      /// <returns></returns>
      static bool intersectLineAndCircle(PointF pc, float radius, PointF pa, PointF pb) {
         if (pa.X == pb.X) {  // senkrechte Linie
            if (pc.X - radius <= pa.X && pa.X <= pc.X + radius) {
               float d = (float)Math.Sqrt(radius * radius - (pa.X - pc.X) * (pa.X - pc.X));
               float ys = pc.Y + d;
               if ((pa.Y <= ys && ys <= pb.Y) ||
                   (pa.Y >= ys && ys >= pb.Y))
                  return true;
               ys = pc.Y - d;
               if ((pb.Y <= ys && ys <= pa.Y) ||
                   (pb.Y >= ys && ys >= pa.Y))
                  return true;
            }
         } else {
            // Parameter der Gerade y=mx+n
            float m = (pb.Y - pa.Y) / (pb.X - pa.X);
            float n = pa.Y - m * pa.X;

            // Kreisgleichung (x-xc)² + (y-yc)² = r²
            //    -> 0 = (x-xc)² + (y-yc)² - r²
            //       0 = (x-xc)² + ((m*x+n)-yc)² - r²
            //       0 = x² + xc² - 2*x*xc + (m*x+n)² + yc² - 2*(m*x+n)*yc - r²
            //       0 = x² + xc² - 2*x*xc + m²x² + n² + 2*x*m*n + yc² - 2*m*x*yc - 2*n*yc - r²
            //       0 = x² + m²x²   - 2*x*xc + 2*x*m*n - 2*m*x*yc    + xc²+ n² + yc² - 2*n*yc - r² 
            //       0 = (m²+1)x²    + x*2(-xc + m*n - m*yc)          + xc²+ n² + yc² - 2*n*yc - r² 
            //          a = m²+1
            //          b = 2(-xc + m*n - m*yc)             = 2(m*n - xc - m*yc)
            //          c = xc² + yc² + n² - 2*n*yc - r²    = xc² + (yc - n)² - r²
            // Parameter für quadratische Gleichung y=ax²+bx+c
            float a = m * m + 1;
            float b = 2 * (m * n - pc.X - m * pc.Y);
            float c = pc.X * pc.X + (n - pc.Y) * (n - pc.Y) - radius * radius;

            // x = -b/2a ± sqrt((b/2a)²-c/a)
            float tmp = b / (2 * a);
            float D = tmp * tmp - c / a;
            if (0 <= D) {  // sonst kein Schnittpunkt von Kreis und Gerade durch A und B
               float tmp2 = (float)Math.Sqrt(D);
               // Schnittpunkte (ev. identisch)
               float x1 = -tmp + tmp2;
               float x2 = -tmp - tmp2;

               if (x1 < pa.X && x2 < pa.X && 
                   x1 < pb.X && x2 < pb.X)
                  return false;

               if (x1 > pa.X && x2 > pa.X && 
                   x1 > pb.X && x2 > pb.X)
                  return false;

               return true;
            }
         }
         return false;
      }


#if ORGUPDATEGRAPHICSPATH
      // arbeitet Path für den _gesamten_ Track

      internal void UpdateGraphicsPath() {
         if (_graphicsPath == null)
            _graphicsPath = new GraphicsPath();
         else
            _graphicsPath.Reset();

         for (int i = 0; i < LocalPoints.Count; i++) {
            var p2 = LocalPoints[i];

            if (i == 0)
               _graphicsPath.AddLine(p2.X, p2.Y, p2.X, p2.Y);
            else {
               var p = _graphicsPath.GetLastPoint();
               _graphicsPath.AddLine(p.X, p.Y, p2.X, p2.Y);
            }
         }
      }
#else

      /// <summary>
      /// update <see cref="visualPolylines"/>
      /// <para>von <see cref="SpecialMapCtrl"/> aufgerufen</para>
      /// </summary>
      /// <param name="isvisible"></param>
      internal void UpdateVisualParts(bool isvisible) {
         startptvisible = false;
         endptvisible = false;

         visualParts.Clear();

         visualPolylines.Clear();

         if (isvisible &&
             IsVisible &&
             LocalPolyline.Length > 0) {  // min. 1 Punkt (z.B. bei Markierungen)
#if ONEVISUALPART
            visualParts.Add((0, LocalPolyline.Length));
            visualPolylines.Add(new Point[LocalPolyline.Length]);
            for (int i = 0; i < LocalPolyline.Length; i++) {
               visualPolylines[0][i] = new Point(LocalPolyline[i].X, LocalPolyline[i].Y);
            }
#else
            // akt. Boundingbox der Client-Punkte
            long xmin = long.MaxValue;
            long ymin = long.MaxValue;
            long xmax = long.MinValue;
            long ymax = long.MinValue;
            foreach (var item in LocalPolyline) {
               xmin = Math.Min(xmin, item.X);
               ymin = Math.Min(ymin, item.Y);
               xmax = Math.Max(xmax, item.X);
               ymax = Math.Max(ymax, item.Y);
            }
            Rectangle rectBB = new Rectangle((int)xmin,
                                             (int)ymin,
                                             (int)(xmax - xmin),
                                             (int)(ymax - ymin));

            long clientleft = -15,
                 clienttop = -15,
                 clientright = Overlay != null && Overlay.Control != null ? Overlay.Control.Width : 0 + 15,
                 clientbottom = Overlay != null && Overlay.Control != null ? Overlay.Control.Height : 0 + 15;

            if (rectBB.IntersectsWith(new Rectangle((int)clientleft,
                                                    (int)clienttop,
                                                    (int)(clientright - clientleft),
                                                    (int)(clientbottom - clienttop)))) {
               List<Point> actpoints = new List<Point>();
               Point p1 = LocalPolyline[0];
               int startidx = 0;
               int count = 0;

               if (1 < LocalPolyline.Length) {              // jedes Segment testen
                  for (int i = 1; i < LocalPolyline.Length; i++) {
                     Point p2 = LocalPolyline[i];

                     if (localSegmentIsNecessary(p1.X,
                                                 p1.Y,
                                                 p2.X,
                                                 p2.Y,
                                                 clientleft, clienttop,
                                                 clientright, clientbottom)) {
                        if (actpoints.Count > 0) {
                           actpoints.Add(p2);
                           count++;
                        } else {
                           startidx = i - 1;
                           actpoints.Add(p1);
                           actpoints.Add(p2);
                           count = 2;
                        }
                        if (i == 1)
                           startptvisible = true;
                        if (i == LocalPolyline.Length - 1)
                           endptvisible = true;
                     } else {
                        if (actpoints.Count > 0) {
                           visualParts.Add((startidx, count));
                           visualPolylines.Add(actpoints.ToArray());
                        }
                        actpoints.Clear();
                        count = 0;
                     }

                     p1 = p2;
                  }
               } else {    // nur ein einzelner Punkt der aber mit der Boundingbox impliziet schon getestet ist
                  actpoints.Add(p1);
                  startidx = 0;
                  count = 1;
               }

               if (actpoints.Count > 0) {
                  visualParts.Add((startidx, count));
                  visualPolylines.Add(actpoints.ToArray());
               }

            }
#endif
         }
      }

      /* Areas:
       * 
       *  0 | 1  | 2
       * ---+----+---
       *  3 | 4  | 5
       * ---+----+---
       *  6 | 7  | 8
       * 
       */

      /// <summary>
      /// very simple test for visibility of the segment (crossing client area?)
      /// </summary>
      /// <param name="p1x"></param>
      /// <param name="p1y"></param>
      /// <param name="p2x"></param>
      /// <param name="p2y"></param>
      /// <param name="left"></param>
      /// <param name="top"></param>
      /// <param name="right"></param>
      /// <param name="bottom"></param>
      /// <returns></returns>
      bool localSegmentIsNecessary(long p1x, long p1y, long p2x, long p2y, long left, long top, long right, long bottom) {
         int area1 = area4localpoint(p1x, p1y, left, top, right, bottom);
         if (area1 != 4) {  // not in client area
            int area2 = area4localpoint(p2x, p2y, left, top, right, bottom);
            if (area2 != 4) {
               switch (area1) {
                  case 0:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2 ||
                         area2 == 3 ||
                         area2 == 6)
                        return false;
                     break;

                  case 2:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2 ||
                         area2 == 5 ||
                         area2 == 8)
                        return false;
                     break;

                  case 6:
                     if (area2 == 0 ||
                         area2 == 3 ||
                         area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8)
                        return false;
                     break;

                  case 8:
                     if (area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8 ||
                         area2 == 5 ||
                         area2 == 2)
                        return false;
                     break;

                  case 1:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2)
                        return false;
                     break;

                  case 3:
                     if (area2 == 0 ||
                         area2 == 3 ||
                         area2 == 6)
                        return false;
                     break;

                  case 5:
                     if (area2 == 2 ||
                         area2 == 5 ||
                         area2 == 8)
                        return false;
                     break;

                  case 7:
                     if (area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8)
                        return false;
                     break;
               }
            }
         }
         return true;
      }

      /// <summary>
      /// get actual areanumber for point (0..8) (4 is the lefttop/rightbottom area)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="left"></param>
      /// <param name="top"></param>
      /// <param name="right"></param>
      /// <param name="bottom"></param>
      /// <returns></returns>
      int area4localpoint(long x, long y, long left, long top, long right, long bottom) {
         if (x < left) {
            if (y < top)
               return 0;
            if (y < bottom)
               return 3;
            else
               return 6;
         } else if (x < right) {
            if (y < top)
               return 1;
            if (y < bottom)
               return 4;
            else
               return 7;
         } else {
            if (y < top)
               return 2;
            if (y < bottom)
               return 5;
            else
               return 8;
         }
      }

      //void pathInfo(SKPath path) {
      //   Debug.WriteLine("   VerbCount=" + path.VerbCount);
      //   using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
      //      SKPathVerb pathVerb = SKPathVerb.Move;
      //      SKPoint[] points = new SKPoint[4];
      //      SKPoint firstPoint = new SKPoint();
      //      SKPoint lastPoint = new SKPoint();

      //      int moves = 0;
      //      int lines = 0;

      //      while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done) {
      //         switch (pathVerb) {
      //            case SKPathVerb.Move:
      //               lines = 0;
      //               moves++;
      //               break;

      //            case SKPathVerb.Line:
      //               lines++;
      //               break;

      //            case SKPathVerb.Close:
      //               Debug.WriteLine("      Close: Lines=" + lines);
      //               break;
      //         }
      //      }
      //      Debug.WriteLine("      End: Lines=" + lines + ", Moves=" + moves);
      //   }
      //}

      //List<List<SKPoint>> splitPath(SKPath path) {
      //   List<List<SKPoint>> parts = new List<List<SKPoint>>();

      //   using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
      //      SKPathVerb pathVerb = SKPathVerb.Move;
      //      SKPoint[] points = new SKPoint[4];
      //      List<SKPoint> part = null;

      //      while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done) {
      //         switch (pathVerb) {
      //            case SKPathVerb.Move:
      //               part = new List<SKPoint>();
      //               parts.Add(part);
      //               part.Add(new SKPoint(points[0].X, points[0].Y));
      //               break;

      //            case SKPathVerb.Line:
      //               part?.Add(new SKPoint(points[1].X, points[1].Y));
      //               break;

      //            case SKPathVerb.Close:
      //               part?.Add(new SKPoint(points[0].X, points[0].Y));
      //               break;
      //         }
      //      }
      //   }
      //   return parts;
      //}

#endif

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               visualPolylines.Clear();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }

}
