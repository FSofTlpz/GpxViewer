using System;
using System.Collections.Generic;

namespace FSofTUtils.Geography {

   public class GeoHelper {

      //Console.WriteLine(GeoHelper.Wgs84Distance(8.41321, 8.42182, 49.9917, 50.0049));       // etwa 1591
      //Console.WriteLine(GeoHelper.Wgs84Distance(8.41321, 8.42182, 49.9917, 50.0049, 1));    // etwa 1591
      //Console.WriteLine(GeoHelper.Wgs84Distance(8.41321, 8.42182, 49.9917, 50.0049, 2));    // etwa 1592

      /// <summary>
      /// Berechnungsmethoden für Entfernungen zwischen WGS84-Koordinaten
      /// </summary>
      public enum Wgs84DistanceCompute {
         /// <summary>
         /// für kurze Entfernungen; die Erdoberfläche wird näherungsweise als Fläche angesehen (im Vgl. zu <see cref="ellipsoid"/> etwa 0,2..0,3% zu wenig)
         /// </summary>
         simple,
         /// <summary>
         /// Grosskreisberechnung auf einer Kugel
         /// </summary>
         sphere,
         /// <summary>
         /// Erde als Ellipsoid
         /// </summary>
         ellipsoid
      }

      /// <summary>
      /// näherungsweise Entfernungsberechnung zwischen WGS84-Koordinaten
      /// </summary>
      /// <param name="lon1"></param>
      /// <param name="lon2"></param>
      /// <param name="lat1"></param>
      /// <param name="lat2"></param>
      /// <param name="model">0 für kurze Entfernungen, 1 für Grosskreis auf Kugel, sonst für WGS84-Ellipsoid</param>
      public static double Wgs84Distance(double lon1, double lon2, double lat1, double lat2, Wgs84DistanceCompute model = Wgs84DistanceCompute.simple) {
         if (lon1 == lon2 &&
             lat1 == lat2)
            return 0;

         double radius = 6370000;         // durchschnittlicher Erdradius

         switch (model) {
            case Wgs84DistanceCompute.simple:
               // Annahmen: 
               //    * Die Entfernung ist so kurz, das sich die Erdoberfläche näherungsweise als Fläche ansehen läßt
               //    * Die Erde ist eine Kugel (konstanter Radius).
               double dist4degree = radius * Math.PI / 180;   // 111177,5
               double deltay = dist4degree * (lat1 - lat2);
               dist4degree *= Math.Cos((lat1 + (lat1 - lat2) / 2) / 180 * Math.PI);
               double deltax = dist4degree * (lon1 - lon2);
               return Math.Sqrt(deltax * deltax + deltay * deltay);

            case Wgs84DistanceCompute.sphere:
               // Annahmen: 
               //    * Die Erde ist eine Kugel (konstanter Radius) -> Grosskreisberechnung
               lat1 *= Math.PI / 180;
               lat2 *= Math.PI / 180;
               lon1 *= Math.PI / 180;
               lon2 *= Math.PI / 180;
               return radius * Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1));

            default:
               // Annahmen: 
               //    * WGS84-Ellipsoid
               // vgl. http://de.wikipedia.org/wiki/Entfernungsberechnung#Genauere_Formel_zur_Abstandsberechnung_auf_der_Erde
               double f = 1 / 298.257223563;    // Abplattung der Erde
               double a = 6378137;              // Äquatorradius der Erde

               double F = (lat1 + lat2) / 2 * Math.PI / 180;
               double G = (lat1 - lat2) / 2 * Math.PI / 180;
               double l = (lon1 - lon2) / 2 * Math.PI / 180;
               double S = Math.Pow(Math.Sin(G), 2) * Math.Pow(Math.Cos(l), 2) + Math.Pow(Math.Cos(F), 2) * Math.Pow(Math.Sin(l), 2);
               double C = Math.Pow(Math.Cos(G), 2) * Math.Pow(Math.Cos(l), 2) + Math.Pow(Math.Sin(F), 2) * Math.Pow(Math.Sin(l), 2);
               double w = Math.Atan(Math.Sqrt(S / C));
               double D = 2 * w * a;
               // Der Abstand D muss nun durch die Faktoren H_1 und H_2 korrigiert werden:
               double R = Math.Sqrt(S * C) / w;
               double H1 = (3 * R - 1) / (2 * C);
               double H2 = (3 * R + 1) / (2 * S);
               return D * (1 + f * H1 * Math.Pow(Math.Sin(F), 2) * Math.Pow(Math.Cos(G), 2) - f * H2 * Math.Pow(Math.Cos(F), 2) * Math.Pow(Math.Sin(G), 2));
         }
      }

      /// <summary>
      /// berechnet näherungsweise die Veränderung der x- und der y-Koordinate (für sehr kleine Winkeldifferenzen)
      /// </summary>
      /// <param name="lon1">alte Länge</param>
      /// <param name="lon2">neue Länge</param>
      /// <param name="lat1">alte Breite</param>
      /// <param name="lat2">neue Breite</param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      public static void Wgs84ShortXYDelta(double lon1, double lon2, double lat1, double lat2, out double deltax, out double deltay) {
         double radius = 6370000;         // durchschnittlicher Erdradius
         double dist4degree = radius * Math.PI / 180;   // 111177,5
         deltay = dist4degree * (lat2 - lat1);
         dist4degree *= Math.Cos((lat2 + (lat2 - lat1) / 2) / 180 * Math.PI);
         deltax = dist4degree * (lon2 - lon1);
      }

      /// <summary>
      /// Liegt der Punkt im Polygon (einschließlich Rand)?
      /// </summary>
      /// <param name="ptx"></param>
      /// <param name="pty"></param>
      /// <param name="polyx"></param>
      /// <param name="polyy"></param>
      /// <returns></returns>
      public static bool PointIsInPolygon(double ptx, double pty, IList<double> polyx, IList<double> polyy) {
         /* Punkt-in-Polygon-Test nach Jordan
            Idee:
            Wenn ein beliebiger Strahl ausgehend von einem Punkt bei einem konvexen Polygon genau 1 Kante schneidet (bei anderen Polygonen eine ungerade Anzahl),
            liegt er im Inneren.
          */
         bool bIsIn = false;

         // jede Polygonseite auf Schnittpunkt mit Strahl von (0,0) nach rechts testen:
         double x1, y1, x2 = 0, y2 = 0;
         for (int i = 0; i < polyx.Count; i++) {
            x1 = i > 0 ? x2 : polyx[polyx.Count - 1] - ptx;
            y1 = i > 0 ? y2 : polyy[polyy.Count - 1] - pty; // Normierung auf (0,0)
            x2 = polyx[i] - ptx;
            y2 = polyy[i] - pty;

            // Wenn P1 und P2 in unterschiedlichen Halbebenen und nicht beide "links" von P liegen könnte es einen Schnittpunkt geben.
            if (x1 >= 0 || x2 >= 0) {
               if (y1 <= 0 && y2 >= 0 ||
                   y2 <= 0 && y1 >= 0) {
                  // Sonderfälle für vorzeitigen Abbruch
                  if (y1 == y2 ||                  // P liegt auf der Kante
                      (y1 == 0 && x1 == 0) ||      // P liegt auf Eckpunkt P1
                      (y2 == 0 && x2 == 0))        // P liegt auf Eckpunkt P2
                     return true;                  // Ecke bzw. Kante wird als "innerhalb" angesehen

                  if (x1 == x2 && x1 >= 0) {       // Kante verläuft senkrecht rechts von P -> Schnittpunkt ex.
                     bIsIn = !bIsIn;
                     continue;
                  }        

                  /* Ex. Schnittpunkt der Kante mit x-Achse mit x >= 0:
                   * y = dy/dx*x+n
                   * y1 = (y2-y1)/(x2-x1) * x1 + n
                   * -> n = y1 - (y2-y1)/(x2-x1) * x1
                   * -> y = (y2-y1)/(x2-x1) * x + y1 - (y2-y1)/(x2-x1) * x1
                   * -> 0 =  (y2-y1)/(x2-x1) * x + y1 - (y2-y1)/(x2-x1) * x1
                   * -> (y2-y1)/(x2-x1) * x1 - y1 = (y2-y1)/(x2-x1) * x
                   * -> x = ((y2-y1)/(x2-x1) * x1 - y1) / (y2-y1)/(x2-x1)
                   * -> x = x1 - y1/((y2-y1)/(x2-x1))
                   * -> x = x1 - y1*(x2-x1)/(y2-y1)
                   * -> x1 - y1*(x2-x1)/(y2-y1) >= 0 ?
                   */
                  if (x1 - y1 * (x2 - x1) / (y2 - y1) >= 0)
                     bIsIn = !bIsIn;
               }
            }
         }
         return bIsIn;
      }

      /// <summary>
      /// Liegt der Punkt innerhalb eines max. Abstandes zur Polyline?
      /// </summary>
      /// <param name="ptx"></param>
      /// <param name="pty"></param>
      /// <param name="polyx"></param>
      /// <param name="polyy"></param>
      /// <param name="w"></param>
      /// <returns></returns>
      public static bool PointIsInNearPolyline(double ptx, double pty, IList<double> polyx, IList<double> polyy, double w) {
         w *= w; // Quadrat
         double x1, y1, x2, y2;
         //for (int i = 0; i < polyx.Count; i++) {
         //x1 = i > 0 ? x2 : polyx[polyx.Count - 1];     // wenn geschlossen
         //y1 = i > 0 ? y2 : polyy[polyy.Count - 1];
         //x2 = polyx[i];
         //y2 = polyy[i];
         for (int i = 0; i < polyx.Count - 1; i++) {
            x1 = polyx[i];
            y1 = polyy[i];
            x2 = polyx[i + 1];
            y2 = polyy[i + 1];
            if (SquareDistanceToSegment(ptx, pty, x1, y1, x2, y2) <= w)
               return true;
         }
         return false;
      }

      /// <summary>
      /// liefert das Quadrat des Abstandes vom Punkt p zur Strecke a->b
      /// </summary>
      /// <param name="px"></param>
      /// <param name="py"></param>
      /// <param name="ax"></param>
      /// <param name="ay"></param>
      /// <param name="bx"></param>
      /// <param name="by"></param>
      /// <returns></returns>
      static double SquareDistanceToSegment(double px, double py, double ax, double ay, double bx, double by) {
         double dx = bx - ax;
         double dy = by - ay;
         if ((dx == 0) && (dy == 0)) { // Punkt, kein Segment
            dx = px - ax;
            dy = py - ay;
            return dx * dx + dy * dy;  // Abstand zum Punkt
         }

         // Idee: Vektor p1 -> p2 (mit t im Bereich 0..1 für Strecke)
         /*
             P
             |
         A---F--B

         Bedingung für Fußpunkt:          Skalarprodukt AF * FP ist 0 -> |AF|*|FP| = 0
         
         Richtungsvektor für Gerade:      dx = b.X - a.X
                                          dy = b.Y - a.Y

         Punkt F auf Strecke/Gerade:      f.X = a.X + t*dx 
                                          f.Y = a.Y + t*dy
         (mit t = 0..1 für Strecke)
         
         Berechnung: 
            AF * FP = 0 = (f.X-a.X) * (f.X-p.X) + (f.Y-a.Y) * (f.Y-p.Y)
            mit
               f.X = a.X + t*dx
               f.Y = a.Y + t*dy
            0 = (a.X + t*dx - a.X) * (a.X + t*dx - p.X) + (a.Y + t*dy - a.Y) * (a.Y + t*dy - p.Y)
            0 = t*dx * (a.X + t*dx - p.X) + t*dy * (a.Y + t*dy - p.Y)
            0 = dx * (a.X + t*dx - p.X) + dy * (a.Y + t*dy - p.Y)
            0 = dx*a.X + t*dx² - dx*p.X + dy*a.Y + t*dy² - dy*p.Y
            0 = t*(dx² + dy²) + dx*(a.X - p.X) + dy*(a.Y - p.Y)

            t = -(dx*(a.X - p.X) + dy*(a.Y - p.Y)) / (dx² + dy²)

            Mit t läßt sich F bestimmen (s.o.)

            Abstand:
            d² = (p.X - f.X)² + (p.Y - f.Y)²
            d² = (p.X - (a.X + t*dx))² + (p.Y - (a.Y + t*dy))²
            d² = p.X² + (a.X + t*dx)² - 2*p.X*(a.X + t*dx) + p.Y² + (a.Y + t*dy)² - 2*p.Y*(a.Y + t*dy)
            d² = p.X² + p.Y² + (a.X + t*dx)² - 2*p.X*a.X - 2*p.X*t*dx + (a.Y + t*dy)² - 2*p.Y*a.Y - 2*p.Y*t*dy
            d² = p.X² + p.Y² + a.X² + t²*dx² + 2*a.X*t*dx - 2*p.X*a.X - 2*p.X*t*dx + a.Y² + t²*dy² + 2*a.Y*t*dy - 2*p.Y*a.Y - 2*p.Y*t*dy
            d² = p.X² + a.X² - 2*p.X*a.X  + p.Y² + a.Y² - 2*p.Y*a.Y      + t²*(dx² + dy²) + 2*t*(a.X*dx - p.X*dx + a.Y*dy - p.Y*dy)
            d² = (p.X - a.X)² + (p.Y - a.Y)²      + t²*(dx² + dy²) + 2*t*(dx*(a.X - p.X) + dy*(a.Y - p.Y))
                                                                         --------------------------------
                                                                         -t*(dx² + dy²) = dx*(a.X - p.X) + dy*(a.Y - p.Y)
            d² = (p.X - a.X)² + (p.Y - a.Y)²      + t²*(dx² + dy²) + 2*t*(-t*(dx² + dy²))
            d² = (p.X - a.X)² + (p.Y - a.Y)²      + t²*(dx² + dy²) - 2*t²*(dx² + dy²)
            d² = (p.X - a.X)² + (p.Y - a.Y)² - t²*(dx² + dy²)
          */

         double t = ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy);

         if (t < 0) {            // nächster Punkt auf der Gerade "vor" der Strecke -> AP
            dx = px - ax;
            dy = py - ay;
         } else if (t > 1) {     // nächster Punkt auf der Gerade "nach" der Strecke -> BP
            dx = px - bx;
            dy = py - by;
         } else {                // nächster Punkt auf der Gerade "auf" der Strecke -> FP
            dx = px - (ax + t * dx);
            dy = py - (ay + t * dy);
         }

         return dx * dx + dy * dy;
      }


   }
}
