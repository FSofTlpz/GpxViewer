using FSofTUtils.Geography.PoorGpx;

namespace SpecialMapCtrl {
   internal class RouteCrossing {

      /// <summary>
      /// Berührt eine Polylinie in irgendeiner Weise das Rechteck?
      /// </summary>
      /// <param name="bounds">Rechteck</param>
      /// <param name="points">Punktliste der Polylinie</param>
      /// <param name="boundofpoints">umgebendes Rechteck der Polylinie (wird bei null automatisch gebildet)</param>
      /// <returns></returns>
      public static bool IsRouteCrossing<T>(GpxBounds bounds, ListTS<T> points, GpxBounds? boundofpoints = null) where T : GpxPointBase {
         if (boundofpoints == null) {
            boundofpoints = new GpxBounds();
            boundofpoints.Union(points);
         }

         //Debug.WriteLine(string.Format("IsCrossing(): {0}", this));
         if (bounds.IntersectsWith(boundofpoints)) {   // sonst lohnt eine Untersuchung nicht

            for (int i = 0; i < points.Count; i++) {  // min. 1 Punkt innerhalb des Rechtecks?
               if (bounds.MinLon <= points[i].Lon && points[i].Lon <= bounds.MaxLon &&
                   bounds.MinLat <= points[i].Lat && points[i].Lat <= bounds.MaxLat) {
                  //Debug.WriteLine(string.Format("IsCrossing(): Point in rect: {0} / {1}", pt, this));
                  return true;
               }
            }
            //Debug.WriteLine(string.Format("IsCrossing(): no Point in rect: {0}", this));

            // vermutlich relativ selten:
            //    es gibt eine gemeinsame Teilmenge der beiden Rechtecke aber kein Punkt liegt im Zielrechteck
            //    eine Verbindung von 2 Punkten könnte das Zielrechteck durchschneiden

            // Schneidet eine Verbindung zwischen 2 Punkten das Rechteck?
            for (int i = 1; i < points.Count; i++) {
               if (iscrossing(points[i - 1].Lon,
                              points[i - 1].Lat,
                              points[i].Lon,
                              points[i].Lat,
                              bounds.MinLon,
                              bounds.MaxLon,
                              bounds.MinLat,
                              bounds.MaxLat)) {
                  //Debug.WriteLine(string.Format("IsCrossing(): iscrossing: {0} / {1}, {2}", this, points[i - 1], points[i]));
                  return true;
               }
            }
         }
         //else
         //   Debug.WriteLine(string.Format("IsCrossing(): no overlapping: {0} / {1}", this, boundofpoints));
         return false;
      }

      /// <summary>
      /// Überlappen sich die 2 Rechtecke?
      /// </summary>
      /// <param name="rect1"></param>
      /// <param name="rect2"></param>
      /// <returns></returns>
      //bool isoverlapping(GpxBounds rect1, GpxBounds rect2) {
      //   return isoverlapping(rect1.MinLat, rect1.MaxLat, rect2.MinLat, rect2.MaxLat) &&
      //          isoverlapping(rect1.MinLon, rect1.MaxLon, rect2.MinLon, rect2.MaxLon);
      //}

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
      /// Schneidet oder berührt die Strecke das (achsenparallele) Rechteck?
      /// </summary>
      /// <param name="ax">x-Wert Punkt A der Strecke</param>
      /// <param name="ay">y-Wert Punkt A der Strecke</param>
      /// <param name="bx">x-Wert Punkt B der Strecke</param>
      /// <param name="by">y-Wert Punkt B der Strecke</param>
      /// <param name="left">x-Wert der linken Rechteckseite</param>
      /// <param name="right">x-Wert der rechten Rechteckseite</param>
      /// <param name="bottom">y-Wert der unteren Rechteckseite</param>
      /// <param name="top">y-Wert der oberen Rechteckseite</param>
      /// <returns>true wenn Schnittpunkt/Berührungspunkt</returns>
      static bool iscrossing(double ax, double ay, double bx, double by, double left, double right, double bottom, double top) {
         if (ax > bx) {
            swap(ref ax, ref bx);
            swap(ref ay, ref by);
         }
         if (left > right)
            swap(ref left, ref right);
         if (bottom > top)
            swap(ref bottom, ref top);

         double dx = bx - ax;
         double dy = by - ay;

         // senkrechte Rechteckseiten
         if (dx == 0) { // Sonderfall
            if (ax == left || ax == right) { // ev. Überlappung
               return isoverlapping(bottom, top, ay, by);
            }
         } else {
            if (ax <= left && left <= bx) {
               double y = ay + dy / dx * (left - ax);
               if (bottom <= y && y <= top)
                  return true;            // Schnittpunkt mit linker Seite
            }
            if (ax <= right && right <= bx) {
               double y = ay + dy / dx * (right - ax);
               if (bottom <= y && y <= top)
                  return true;            // Schnittpunkt mit rechter Seite
            }
         }

         // waagerechte Rechteckseiten
         if (dy == 0) { // Sonderfall
            if (ay == bottom || ay == top) { // ev. Überlappung
               return isoverlapping(left, right, ax, bx);
            }
         } else {
            if ((ay <= bottom && bottom <= by) ||
                (by <= bottom && bottom <= ay)) {
               double x = ax + (bottom - ay) * dx / dy;
               if (left <= x && x <= right)
                  return true;            // Schnittpunkt mit unterer Seite
            }
            if ((ay <= top && top <= by) ||
                (by <= top && top <= ay)) {
               double x = ax + (top - ay) * dx / dy;
               if (left <= x && x <= right)
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
      static void swap(ref double v1, ref double v2) {
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
      static bool isoverlapping(double va1, double va2, double vb1, double vb2) {
         if (va1 > va2)
            swap(ref va1, ref va2);
         if (vb1 > vb2)
            swap(ref vb1, ref vb2);
         return vb2 >= va1 && vb1 <= va2;
      }

   }
}
