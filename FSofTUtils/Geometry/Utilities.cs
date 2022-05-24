using System;

namespace FSofTUtils.Geometry {
   public class Utilities {

      #region ex. Schnittpunkt von 2 Strecken

      /// <summary>
      /// Schneiden (oder berühren) sich die (echten) Strecken PQ und RS?
      /// <para>Eine "echte" Strecke hat eine Länge > 0.</para>
      /// </summary>
      /// <param name="px"></param>
      /// <param name="py"></param>
      /// <param name="qx"></param>
      /// <param name="qy"></param>
      /// <param name="rx"></param>
      /// <param name="ry"></param>
      /// <param name="sx"></param>
      /// <param name="sy"></param>
      /// <returns></returns>
      public static bool IsLineSegmentIntersect(double px, double py,
                                                double qx, double qy,
                                                double rx, double ry,
                                                double sx, double sy) {
         if (!boundingBoxesIntersect(px, py, qx, qy, rx, ry, sx, sy)) // keine Chance auf einen Schnittpunkt
            return false;

         double d1 = direction(px, py, qx, qy, rx, ry); // Kreuzprodukt der Vektoren: (Q-P) x (R-P)
         double d2 = direction(px, py, qx, qy, sx, sy); // Kreuzprodukt der Vektoren: (Q-P) x (S-P)

         // Liegen R und S auf der selben Seite/Halbebene von PQ?
         if ((d1 > 0 && d2 > 0) ||
             (d1 < 0 && d2 < 0)) return false;

         if (d1 == 0 || d2 == 0) {
            // Liegt r (oder s) auf pq -> min. Berührung?
            if ((d1 == 0 && onLineSegment(px, py, qx, qy, rx, ry)) ||
                (d2 == 0 && onLineSegment(px, py, qx, qy, sx, sy)))
               return true;

            if (d1 != 0 || d2 != 0) // R oder S auf der Linie, der andere Punkt nicht
               return false;

            // sonst: d1==0 && d2==0, d.h. R und S auf der Linie
         }

         // R und S liegen in unterschiedlichen Halbebenen bzgl. PQ (oder beide auf der Linie durch PQ) ...

         // ... deshalb gleiche Untersuchung  für P und Q bzgl. RS nötig
         double d3 = direction(rx, ry, sx, sy, px, py);
         double d4 = direction(rx, ry, sx, sy, qx, qy);

         // P und Q auf der selben Seite von RS?
         if ((d3 > 0 && d4 > 0) ||
             (d3 < 0 && d4 < 0)) return false;

         // Liegt P (oder Q) auf RS?
         if (d3 == 0 || d4 == 0) {
            if ((d3 == 0 && onLineSegment(rx, ry, sx, sy, px, py)) ||
                (d4 == 0 && onLineSegment(rx, ry, sx, sy, qx, qy)))
               return true;

            if (d3 != 0 || d4 != 0) // R oder S auf der Linie, der andere Punkt nicht
               return false;
         }

         return true;
      }

      /// <summary>
      /// Die umschließenden (min.) Rechtecke der beiden Strecken PQ und RS müssen ein gemeinsames Teilgebiet haben, 
      /// damit es wenigstens die Chance auf einen Schnittpunkt gibt.
      /// </summary>
      /// <param name="px"></param>
      /// <param name="py"></param>
      /// <param name="qx"></param>
      /// <param name="qy"></param>
      /// <param name="rx"></param>
      /// <param name="ry"></param>
      /// <param name="sx"></param>
      /// <param name="sy"></param>
      /// <returns></returns>
      static bool boundingBoxesIntersect(double px, double py,
                                         double qx, double qy,
                                         double rx, double ry,
                                         double sx, double sy) {
         //        (     maxRS       <     minPQ       ) || (     maxPQ       <      minRS      )    -> keine Überschneidung
         return !(((Math.Max(rx, sx) < Math.Min(px, qx)) || (Math.Max(px, qx) < Math.Min(rx, sx))) &&
                  ((Math.Max(ry, sy) < Math.Min(py, qy)) || (Math.Max(py, qy) < Math.Min(ry, sy))));
      }


      /// <summary>
      /// Das Kreuzprodukt (B - A) x (C - A) ist je nach Halbebene in der C bzgl. AB liegt positiv (C links von AB) oder negativ (C rechts von AB).
      /// (Der resultierende Vektor steht orthogonal auf der Ebene (in A), die durch die 3 Punkte gebildet wird.)
      /// </summary>
      /// <param name="ax"></param>
      /// <param name="ay"></param>
      /// <param name="bx"></param>
      /// <param name="by"></param>
      /// <param name="cx"></param>
      /// <param name="cy"></param>
      /// <returns></returns>
      static double direction(double ax, double ay,
                              double bx, double by,
                              double cx, double cy) {
         return (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
      }

      /// <summary>
      /// Liegt T auf der Strecke PQ (wenn T auf der Gerade durch P und Q liegt).
      /// </summary>
      /// <param name="px"></param>
      /// <param name="py"></param>
      /// <param name="qx"></param>
      /// <param name="qy"></param>
      /// <param name="tx"></param>
      /// <param name="ty"></param>
      /// <returns></returns>
      static bool onLineSegment(double px, double py, double qx, double qy, double tx, double ty) {
         return (Math.Min(px, qx) <= tx) && (tx <= Math.Max(px, qx)) &&
                (Math.Min(py, qy) <= ty) && (ty <= Math.Max(py, qy));
      }

      //public static bool IsSegmentIntersect(float[] p, float[] q, float[] r, float[] s) {

      //   float d1 = direction(p, q, r), d2 = direction(p, q, s);
      //   // liegt r bzw. s auf pq?
      //   if (d1 == 0 && onSegment(p, q, r)) return true;
      //   if (d2 == 0 && onSegment(p, q, s)) return true;
      //   // r und s auf der selben Seite von pq?
      //   if ((d1 > 0 && d2 > 0) || (d1 < 0 && d2 < 0)) return false;

      //   float d3 = direction(r, s, p), d4 = direction(r, s, q);
      //   // liegt p bzw. q auf rs?
      //   if (d3 == 0 && onSegment(r, s, p)) return true;
      //   if (d4 == 0 && onSegment(r, s, q)) return true;
      //   // p und q auf der selben Seite von rs?
      //   if ((d3 > 0 && d4 > 0) || (d3 < 0 && d4 < 0)) return false;

      //   if (d1 == d2 == d3 == d4 == 0) return false;
      //   return true;
      //}

      //static float det(float[] a, float[] b) {
      //   return a[0] * b[1] - a[1] * b[0];
      //}

      //// Richtung des Knicks zwischen pq und qr?
      //static float direction(float[] p, float[] q, float[] r) {
      //   float[] a = { q[0] - p[0], q[1] - p[1] }; // q-p
      //   float[] b = { r[0] - q[0], r[1] - q[1] }; // r-q
      //   return det(a, b);
      //}

      //// Vorbedingung: x liegt auf (der Verlängerung von) pq.
      //// Teste, ob x auch zwischen p und q liegt.
      //static bool onSegment(float[] p, float[] q, float[] x) {
      //   float[] topright = { Math.Max(p[0], q[0]), Math.Max(p[1], q[1]) };
      //   float[] botleft = { Math.Min(p[0], q[0]), Math.Min(p[1], q[1]) };
      //   // return (botleft <= x <= topright);
      //   return (x[0] <= topright[0]) && (x[1] <= topright[1]) &&
      //  (botleft[0] <= x[0]) && (botleft[1] <= x[1]);
      //}

      #endregion

      /// <summary>
      /// Überschneiden (oder berühren) sich die 2 Rechtecke ABCD und EFGH?
      /// </summary>
      /// <param name="ax"></param>
      /// <param name="ay"></param>
      /// <param name="bx"></param>
      /// <param name="by"></param>
      /// <param name="cx"></param>
      /// <param name="cy"></param>
      /// <param name="dx"></param>
      /// <param name="dy"></param>
      /// <param name="ex"></param>
      /// <param name="ey"></param>
      /// <param name="fx"></param>
      /// <param name="fy"></param>
      /// <param name="gx"></param>
      /// <param name="gy"></param>
      /// <param name="hx"></param>
      /// <param name="hy"></param>
      /// <returns></returns>
      public static bool IsRectangleIntersect(double ax, double ay,
                                              double bx, double by,
                                              double cx, double cy,
                                              double dx, double dy,
                                              double ex, double ey,
                                              double fx, double fy,
                                              double gx, double gy,
                                              double hx, double hy) {
         return IsLineSegmentIntersect(ax, ay, bx, by, ex, ey, fx, fy) ||
                IsLineSegmentIntersect(ax, ay, bx, by, fx, fy, gx, gy) ||
                IsLineSegmentIntersect(ax, ay, bx, by, gx, gy, hx, hy) ||
                IsLineSegmentIntersect(ax, ay, bx, by, hx, hy, ex, ey) ||

                IsLineSegmentIntersect(bx, by, cx, cy, ex, ey, fx, fy) ||
                IsLineSegmentIntersect(bx, by, cx, cy, fx, fy, gx, gy) ||
                IsLineSegmentIntersect(bx, by, cx, cy, gx, gy, hx, hy) ||
                IsLineSegmentIntersect(bx, by, cx, cy, hx, hy, ex, ey) ||

                IsLineSegmentIntersect(cx, cy, dx, dy, ex, ey, fx, fy) ||
                IsLineSegmentIntersect(cx, cy, dx, dy, fx, fy, gx, gy) ||
                IsLineSegmentIntersect(cx, cy, dx, dy, gx, gy, hx, hy) ||
                IsLineSegmentIntersect(cx, cy, dx, dy, hx, hy, ex, ey) ||

                IsLineSegmentIntersect(dx, dy, ax, ay, ex, ey, fx, fy) ||
                IsLineSegmentIntersect(dx, dy, ax, ay, fx, fy, gx, gy) ||
                IsLineSegmentIntersect(dx, dy, ax, ay, gx, gy, hx, hy) ||
                IsLineSegmentIntersect(dx, dy, ax, ay, hx, hy, ex, ey);
      }

   }
}
