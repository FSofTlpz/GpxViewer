using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FSofTUtils.Geometry {

   /// <summary>
   /// spez. Punktliste für die Polyline-Simplification
   /// </summary>
   public class SimplificationPointList {

      /// <summary>
      /// spezielle Punktklasse
      /// </summary>
      public class Point : PointD {

         public bool IsValid;
         public bool IsLocked;


         public Point(double x, double y) : base(x, y) {
            IsLocked = false;
         }

         public Point(Point p) : this(p.x, p.y) {
            IsValid = p.IsValid;
            IsLocked = p.IsLocked;
         }

         /// <summary>
         /// rundet <see cref="Y"/>
         /// </summary>
         /// <param name="fractionaldigits"></param>
         public void RoundY(int fractionaldigits) => Y = Math.Round(Y, fractionaldigits);

         public override string ToString() => string.Format("({0}, {1}), IsValid={2}, IsLocked={3}", X, Y, IsValid, IsLocked);

      }


      /// <summary>
      /// Liste nötig wegen <see cref="Remove(int)"/>
      /// </summary>
      protected List<Point?> pt;

      public int Length => pt.Count;


      public SimplificationPointList(int iLength) {
         if (iLength < 3)
            throw new ArgumentException("Es sind min. 3 Punkte für die Liste nötig.");
         pt = new List<Point?>(new Point[iLength]);
      }

      public SimplificationPointList(SimplificationPointList pl) {
         Point[] tmp = new Point[pl.Length];
         pl.pt.CopyTo(tmp);
         pt = new List<Point?>(tmp);
      }

      /// <summary>
      /// bei ungültigem Index Exception
      /// </summary>
      /// <param name="no"></param>
      /// <exception cref="ArgumentException"></exception>
      void indexTest(int no) {
         if (no < 0 || Length <= no)
            throw new ArgumentException("Der Punktindex " + no + " liegt außerhalb des gültigen Bereichs.");
      }

      /// <summary>
      /// bei ungültigem Index oder nicht def. Punkt Exception
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      /// <exception cref="ArgumentException"></exception>
      Point get(int idx) {
         indexTest(idx);
         if (Equals(pt[idx], null))
            throw new ArgumentException("Der Punkt " + idx + " ist nicht definiert.");
         else
#pragma warning disable CS8603 // Mögliche Nullverweisrückgabe.
            return pt[idx];
#pragma warning restore CS8603 // Mögliche Nullverweisrückgabe.
      }

      /// <summary>
      /// setzt die Daten des Punktes mit den den Daten von <paramref name="p"/>
      /// </summary>
      /// <param name="no"></param>
      /// <param name="p"></param>
      public void Set(int no, Point p) {
         indexTest(no);
         if (Equals(pt[no], null))
            pt[no] = new Point(p);
         else {
            Point pn = get(no);
            pn.X = p.X;
            pn.Y = p.Y;
            pn.IsLocked = p.IsLocked;
            pn.IsValid = p.IsValid;
         }
      }

      /// <summary>
      /// akt. den Punkt
      /// </summary>
      /// <param name="no"></param>
      /// <param name="x"></param>
      /// <param name="y"></param>
      public void Set(int no, double x, double y) {
         indexTest(no);
         if (Equals(pt[no], null))
            pt[no] = new Point(x, y);
         else {
            Point p = get(no);
            p.X = x;
            p.Y = y;
            p.IsLocked = false;
            p.IsValid = true;
         }
      }

      public void Set(int no, bool locked) {
         Point p = get(no);
         p.IsLocked = locked;
      }

      /// <summary>
      /// liefert eine Kopie dieses Punktes
      /// </summary>
      /// <param name="no"></param>
      /// <returns></returns>
      public Point Get(int no) => new Point(get(no));

      /// <summary>
      /// liefert eine Kopie dieses Punktes
      /// </summary>
      /// <param name="no"></param>
      /// <param name="p"></param>
      public void Get(int no, ref Point p) {
         Point pt = get(no);
         p.X = pt.X;
         p.Y = pt.Y;
         p.IsLocked = pt.IsLocked;
         p.IsValid = pt.IsValid;
      }

      public double GetX(int no) => get(no).X;

      public double GetY(int no) => get(no).Y;

      public bool PointIsValid(int idx) => get(idx).IsValid;

      public bool PointIsLocked(int idx) => get(idx).IsLocked;

      void setPointValid(int idx, bool valid = true) => get(idx).IsValid = valid;

      /// <summary>
      /// liefert den "Vektor" a - b
      /// </summary>
      /// <param name="idxa"></param>
      /// <param name="idxb"></param>
      /// <returns></returns>
      public PointD GetSubstract(int idxa, int idxb) => get(idxa) - get(idxb);

      /// <summary>
      /// entfernt einen Punkt
      /// </summary>
      /// <param name="idx"></param>
      public void Remove(int idx) {
         indexTest(idx);
         pt.RemoveAt(idx);
      }

      /// <summary>
      /// Reumann-Witkam-Algorithmus;
      /// i.A. sollte der letzte Punkt vorher 'gelocked' werden
      /// </summary>
      /// <param name="dWidth">halbe Korridorbreite</param>
      public void ReumannWitkam(double dWidth) {
         int iStart = 0;
         int iNext = 1;
         while (get(iStart).Equals(pt[iNext]) && iNext < Length) iNext++;      // ungleichen Punkt suchen
         int iTest = iNext + 1;
         dWidth *= dWidth;

         for (int i = 0; i < Length; i++)
            setPointValid(i);

         while (iTest < Length) {
            bool bPointIsValid = get(iTest).IsLocked;
            if (!bPointIsValid) {            // Test ist nötig (Normalfall)
               Point p0 = get(iStart);
               Point p1 = get(iNext);
               Point p2 = get(iTest);
               // teste, ob p2 innerhalb der durch p0, p1 und dWidth vorgegebenen Bandbreite liegt

               // Das fkt. sehr einfach mit Hilfe des Skalarproduktes. Es wird nicht der Abstand, sondern das Quadrat des Abstandes
               // berechnet. Man spart sich das Ziehen der Wurzel und es gibt keine Probleme mit ev. negativen Werten.

               PointD p0p1 = p0 - p1;
               PointD p1p2 = p1 - p2;
               double dDotProduct = p0p1.DotProduct(p1p2);
               double dSquare_AbsP0P1 = p0p1.SquareAbsolute();
               double dSquare_WidthTest = (dSquare_AbsP0P1 * p1p2.SquareAbsolute() - dDotProduct * dDotProduct) / dSquare_AbsP0P1;

               bPointIsValid = dSquare_WidthTest > dWidth;
            }

            if (bPointIsValid) {      // p2 bleibt erhalten
               setPointValid(iNext, false);
               iStart = iTest;
               iNext = iStart + 1;
               while (iNext < Length && get(iStart).Equals(pt[iNext])) iNext++;      // ungleichen Punkt suchen
               iTest = iNext + 1;
               while (iTest < Length && get(iNext).Equals(pt[iTest])) iTest++;       // ungleichen Punkt suchen
            } else {
               setPointValid(iTest, false);
               do {
                  iTest++;
               } while (iTest < Length && get(iNext).Equals(pt[iTest]));             // ungleichen Punkt suchen
            }
         }
         setPointValid(Length - 1);
      }

      /// <summary>
      /// Douglas-Peucker-Algorithmus
      /// <para>https://de.wikipedia.org/wiki/Douglas-Peucker-Algorithmus</para>
      /// </summary>
      /// <param name="dWidth"></param>
      public void DouglasPeucker(double dWidth) {
         for (int i = 0; i < Length; i++)
            setPointValid(i, false);
         setPointValid(0);
         setPointValid(Length - 1);
         DouglasPeuckerRecursive(0, Length - 1, dWidth * dWidth);
         for (int i = 0; i < Length; i++)
            if (get(i).IsLocked)
               setPointValid(i);
      }

      /// <summary>
      /// führt eine Integration für 'dWidth' breite Streifen durch und liefert nur noch je Streifen 1 Punkt
      /// </summary>
      /// <param name="dWidth"></param>
      public void HeigthProfileWithIntegral(double dWidth) {
         List<Point> newpt = new List<Point>();
         // Punktliste: X = Entfernung vom Start, Y = Höhe in dieser Entfernung
         if (Length >= 2) {
            double dStripeStart = 0.0;
            double dX0 = get(0).X;
            double dY0 = get(0).Y;
            double dTripLength = get(Length - 1).X;
            for (int i = 0, p = 1;
                 dStripeStart < dTripLength && p < Length;
                 i++, dStripeStart += dWidth) {
               double dStripeEnd = Math.Min(dStripeStart + dWidth, dTripLength);
               // Integral zwischen dStripeStart und dStripeEnd bilden
               double dIntegral = 0;
               dX0 = dStripeStart;
               for (; p < Length; p++) {
                  Point pt = get(p);
                  if (pt.X <= dStripeEnd) {
                     double dx = pt.X - dX0;
                     double dh = pt.Y - dY0;
                     dIntegral += dx * (dY0 + dh / 2);
                     dY0 = pt.Y;
                     dX0 += dx;
                  } else {
                     double dx = dStripeEnd - dX0;
                     double dh = dx * (pt.Y - get(p - 1).Y) / (pt.X - get(p - 1).X);
                     dIntegral += dx * (dY0 + dh / 2);
                     dY0 = get(p - 1).Y + dh;
                     dX0 += dx;
                     break;
                  }
               }
               newpt.Add(new Point(dStripeEnd, dIntegral));
            }
            for (int i = 0; i < newpt.Count; i++) {
               double dx = i == 0 ? newpt[i].X : newpt[i].X - newpt[i - 1].X;
               newpt[i].Y /= dx;
            }
#pragma warning disable CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
            pt = newpt;
#pragma warning restore CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
         }
      }

      //public void HeigthProfileWithSlidingMean1(int count) {
      //   // Punktliste: X = Entfernung vom Start, Y = Höhe in dieser Entfernung
      //   int iFirstIdx = -count / 2;
      //   int iLastIdx = iFirstIdx + count - 1;

      //   List<Point> newpt = new List<Point>();
      //   newpt.Add(new Point(pt[0].X, pt[0].Y));
      //   for (int i = 1; i < Length; i++) {
      //      int iStartIdx = Math.Max(1, i + iFirstIdx);
      //      int iEndIdx = Math.Min(Length - 1, i + iLastIdx);
      //      double dSum = 0;
      //      for (int j = iStartIdx; j <= iEndIdx; j++)      // iStartIdx > 0 !
      //         dSum += pt[j].Y * (pt[j].X - pt[j - 1].X);
      //      dSum /= pt[iEndIdx].X - pt[iStartIdx - 1].X;
      //      newpt.Add(new Point(pt[i].X, dSum));
      //   }
      //   pt = newpt;
      //}

      /// <summary>
      /// berechnet den gleitenden, gewichteten Mittelwert aus 'count' Punkten  um den jeweiligen Punkt durch (+- count/2), 
      /// wobei die Höhe jeweils noch mit der zugehörigen Teil-Streckenlänge gewichtet wird
      /// </summary>
      /// <param name="count"></param>
      public void HeigthProfileWithSlidingMean(int count) {
         // Punktliste: X = Entfernung vom Start, Y = Höhe in dieser Entfernung
         int iFirstIdx = -count / 2;
         int iLastIdx = iFirstIdx + count - 1;

         double[] Y = new double[Length];
         for (int p = 0; p < Length; p++) {
            // Indexbereich festlegen
            int iStartIdx = Math.Max(0, p + iFirstIdx);
            int iEndIdx = Math.Min(Length - 1, p + iLastIdx);

            double dYXSum = 0;
            double dXSum = 0;
            for (int i = iStartIdx; i <= iEndIdx; i++) { // jeder Punkt im Indexbereich ...
               double xa = i >= 1 ? get(i - 1).X : 0;
               double xe = i < Length - 1 ? get(i + 1).X : get(Length - 1).X;
               //(pt[i + 1].X - pt[i].X) / 2 + (pt[i].X - pt[i - 1].X) / 2
               //(pt[i + 1].X - pt[i].X + pt[i].X - pt[i - 1].X) / 2
               //(pt[i + 1].X - pt[i - 1].X) / 2
               double s = (xe - xa) / 2;
               dYXSum += get(i).Y * s;
               dXSum += s;
            }

            Y[p] = dYXSum / dXSum;
         }

         for (int p = 0; p < Length; p++)
            get(p).Y = Y[p];
      }

      //public void HeigthProfileWithSlidingIntegral(double dWidth) {
      //   List<Point> newpt = new List<Point>();
      //   // Punktliste: X = Entfernung vom Start, Y = Höhe in dieser Entfernung
      //   if (Length >= 2) {
      //      newpt.Add(new Point(pt[0]));
      //      dWidth /= 2;
      //      Point pStart = new Point();
      //      Point pEnd = new Point();
      //      for (int i = 1; i < Length; i++) {      // für jeden Punkt außer dem 1.
      //         double dStripeStart = Math.Max(0, pt[i].X - dWidth);
      //         double dStripeEnd = Math.Min(pt[Length - 1].X, pt[i].X + dWidth);
      //         int iStart, iEnd;
      //         for (iStart = i; iStart >= 0; iStart--)
      //            if (pt[iStart].X <= dStripeStart) break;
      //         // --> pt[iStart].X <= dStripeStart
      //         for (iEnd = i; iEnd < Length; iEnd++)
      //            if (pt[iEnd].X >= dStripeEnd) break;
      //         // --> pt[iEnd].X >= dStripeEnd

      //         if (pt[iStart].X < dStripeStart) {     // virtuellen Startpunkt berechnen
      //            pStart.X = dStripeStart;
      //            pStart.Y = pt[iStart].Y + (dStripeStart - pt[iStart].X) * (pt[iStart + 1].Y - pt[iStart].Y) / (pt[iStart + 1].X - pt[iStart].X);
      //         }
      //         if (pt[iEnd].X > dStripeEnd) {         // virtuellen Endpunkt berechnen
      //            pEnd.X = dStripeEnd;
      //            pEnd.Y = pt[iEnd].Y - (pt[iEnd].X - dStripeEnd) * (pt[iEnd].Y - pt[iEnd - 1].Y) / (pt[iEnd].X - pt[iEnd - 1].X);
      //         }

      //         // Integral zwischen dStripeStart und dStripeEnd bilden; die Punkte mit Index iStart ... iEnd sind daran beteiligt
      //         double dIntegral = 0;
      //         for (int p = iStart; p < iEnd; p++) {
      //            Point p1 = pt[p];
      //            if (p1.X < dStripeStart) p1 = pStart;
      //            Point p2 = pt[p + 1];
      //            if (p2.X > dStripeEnd) p2 = pEnd;

      //            dIntegral += (p1.Y + (p2.Y - p1.Y) / 2) * (p2.X - p1.X);
      //         }

      //         newpt.Add(new Point(pt[i].X, dIntegral / (dStripeEnd - dStripeStart)));
      //      }

      //      pt = newpt;
      //   }
      //}

      /// <summary>
      /// führt eine Integration für max. 'dWidth' breite Streifen um den jeweiligen Punkt durch (+- dWidth/2)
      /// </summary>
      /// <param name="dWidth"></param>
      public void HeigthProfileWithSlidingIntegral(double dWidth) {
         // Punktliste: X = Entfernung vom Start, Y = Höhe in dieser Entfernung
         double[] Y = new double[Length];
         if (Length >= 2) {
            dWidth /= 2;
            for (int i = 0; i < Length; i++) {
               // x-Bereich für die Integration bestimmen
               double dStripeXStart = Math.Max(0, get(i).X - dWidth);
               double dStripeXEnd = Math.Min(get(Length - 1).X, get(i).X + dWidth);

               // betroffenen Indexbereich bestimmen
               int iStartIdx, iEndIdx;
               for (iStartIdx = i; iStartIdx >= 0; iStartIdx--)
                  if (get(iStartIdx).X <= dStripeXStart)
                     break;
               for (iEndIdx = i; iEndIdx < Length; iEndIdx++)
                  if (get(iEndIdx).X >= dStripeXEnd)
                     break;


               Point ptStartIdx = get(iStartIdx);        // X <= dStripeXStart
               Point ptStartIdxN = get(iStartIdx + 1);
               double dStripeYStart = ptStartIdx.X == dStripeXStart ?
                           ptStartIdx.Y :
                           ptStartIdx.Y + (dStripeXStart - ptStartIdx.X) * (ptStartIdxN.Y - ptStartIdx.Y) / (ptStartIdxN.X - ptStartIdx.X);

               Point ptEndIdx = get(iEndIdx);            // dStripeXEnd <= X
               Point ptEndIdxB = get(iEndIdx - 1);
               double dStripeYEnd = ptEndIdx.X == dStripeXEnd ?
                           ptEndIdx.Y :
                           ptEndIdx.Y - (ptEndIdx.X - dStripeXEnd) * (ptEndIdx.Y - ptEndIdxB.Y) / (ptEndIdx.X - ptEndIdxB.X);

               // Integral zwischen dStripeStart und dStripeEnd bilden; die Punkte mit Index iStart ... iEnd sind daran beteiligt
               double dIntegral = 0;
               for (int p = iStartIdx; p < iEndIdx; p++) {
                  Point pt = get(p);
                  double x1 = pt.X >= dStripeXStart ? pt.X : dStripeXStart;
                  double y1 = pt.X >= dStripeXStart ? pt.Y : dStripeYStart;
                  pt = get(p + 1);
                  double x2 = pt.X <= dStripeXEnd ? pt.X : dStripeXEnd;
                  double y2 = pt.X <= dStripeXEnd ? pt.Y : dStripeYEnd;

                  // mittlere Höhe zwischen p und p+1
                  dIntegral += (y1 + y2) / 2 * (x2 - x1);
               }

               Y[i] = dIntegral / (dStripeXEnd - dStripeXStart);
            }

            for (int p = 0; p < Length; p++)
               get(p).Y = Y[p];

         }
      }

      ///// <summary>
      ///// führt eine Ridge-Regression (1-dim) aus
      ///// </summary>
      ///// <param name="iWidth"></param>
      ///// <param name="iOverlap"></param>
      ///// <param name="lambda"></param>
      //public void HeigthProfileWithRidgeRegression1(int framesize, int iOverlap, double lambda) {
      //   int framestep = framesize - iOverlap;
      //   RidgeSplineRegression rr = new RidgeSplineRegression();

      //   double[] sumh = new double[Length];
      //   int[] c = new int[Length];

      //   double[] frx = new double[framesize];
      //   double[] fry = new double[framesize];
      //   for (int s = 0; s < Length; s += framestep) {
      //      int actualframesize = Math.Min(framesize, Length - s);
      //      for (int i = 0; i < actualframesize; i++) {
      //         frx[i] = pt[i + s].X;
      //         fry[i] = pt[i + s].Y;
      //      }

      //      if (actualframesize > 2) {
      //         RidgeSplineRegression.Normalize(ref frx, out double minx, out double fx, actualframesize);
      //         RidgeSplineRegression.Normalize(ref fry, out double miny, out double fy, actualframesize);

      //         double[,] xm = RidgeSplineRegression.BuildX(frx, actualframesize);
      //         if (actualframesize == fry.Length)
      //            rr.CalculateParams(xm, fry, lambda);
      //         else {
      //            double[] tmp = new double[actualframesize];
      //            Array.Copy(fry, tmp, actualframesize);
      //            rr.CalculateParams(xm, tmp, lambda);
      //         }
      //         for (int i = 0; i < actualframesize; i++)
      //            fry[i] = RidgeSplineRegression.Denormalize(rr.Calculate(xm, frx[i]), miny, fy);
      //      }

      //      for (int i = 0; i < actualframesize; i++) {
      //         sumh[i + s] += fry[i];   // summiert
      //         c[i + s]++;           // Anzahl der Werte je Punkt
      //      }
      //   }

      //   // Durchschnitt berechnen
      //   for (int i = 0; i < Length; i++)
      //      pt[i].Y = sumh[i] / c[i];
      //}

      /// <summary>
      /// führt eine Ridge-Spline-Regression (1-dim) aus
      /// <para>Die Berechnung kann i.A. nicht für den gesamten Bereceich auf einmal erfolgen, weil die dabei enstehenden 
      /// Matrixe sehr groß werden.</para>
      /// </summary>
      /// <param name="framesize">Framegröße</param>
      /// <param name="iOverlap">überlappender Bereich der Frames</param>
      /// <param name="lambda">Faktor</param>
      /// <param name="normalizefullrange">gesamter Bereich oder frameweise</param>
      public void HeigthProfileWithRidgeRegression(int framesize, int iOverlap, double lambda, bool normalizefullrange) {
         int framestep = framesize - iOverlap;
         RidgeSplineRegression rr = new RidgeSplineRegression();

         double[] sumh = new double[Length];
         int[] c = new int[Length];

         double[]? s = null;
         double[]? h = null;
         double minx, miny = 0, fx, fy = 1;
         if (normalizefullrange) {
            s = new double[Length];
            h = new double[Length];
            for (int i = 0; i < Length; i++) {
               Point p = get(i + 1);
               s[i] = p.X;
               h[i] = p.Y;
            }

            RidgeSplineRegression.Normalize(ref s, out minx, out fx, Length);
            RidgeSplineRegression.Normalize(ref h, out miny, out fy, Length);
         }

         double[] frx = new double[framesize];
         double[] fry = new double[framesize];
         for (int j = 0; j < Length; j += framestep) {
            int actualframesize = Math.Min(framesize, Length - j);
            if (normalizefullrange)
               if (s != null && h != null)
                  for (int i = 0; i < actualframesize; i++) {
                     frx[i] = s[i + j];
                     fry[i] = h[i + j];
                  }
               else
                  for (int i = 0; i < actualframesize; i++) {
                     Point p = get(i + 1);
                     frx[i] = p.X;
                     fry[i] = p.Y;
                  }

            if (actualframesize > 2) {
               if (!normalizefullrange) {
                  RidgeSplineRegression.Normalize(ref frx, out minx, out fx, actualframesize);
                  RidgeSplineRegression.Normalize(ref fry, out miny, out fy, actualframesize);
               }
               double[,] xm = RidgeSplineRegression.BuildX(frx, actualframesize);
               if (actualframesize == fry.Length)
                  rr.CalculateParams(xm, fry, lambda);
               else {
                  double[] tmp = new double[actualframesize];
                  Array.Copy(fry, tmp, actualframesize);
                  rr.CalculateParams(xm, tmp, lambda);
               }
               for (int i = 0; i < actualframesize; i++)
                  fry[i] = RidgeSplineRegression.Denormalize(rr.Calculate(xm, frx[i]), miny, fy);
            }

            for (int i = 0; i < actualframesize; i++) {
               sumh[i + j] += fry[i];   // summiert
               c[i + j]++;           // Anzahl der Werte je Punkt
            }
         }

         // Durchschnitt berechnen
         for (int i = 0; i < Length; i++)
            get(i).Y = sumh[i] / c[i];
      }

      /// <summary>
      /// rundet Y für alle Punkte
      /// </summary>
      /// <param name="fractionaldigits"></param>
      public void RoundHeightProfile(int fractionaldigits) {
         for (int i = 0; i < Length; i++)
            get(i).RoundY(fractionaldigits);
      }

      /// <summary>
      /// versucht, Pausen (scheinbare Bewegungen) zu erkennen und zu beseitigen
      /// <para>
      /// Wenn die Punktfolge innerhalb eines Kreises mit vorgegebenen Radius liegt, dieses Polygon min. die vorgegebene Anzahl von 
      /// Kreuzungen mit sich selbst aufweist und der durchschnittliche Winkel zwischen 2 aufeinanderfolgenden Teilstrecken min.
      /// dem vorgegebenen Winkel entspricht, wird diese Punktfolge bis auf den Punkt gelöscht, der dem Umkreismittelpunkt am nächsten 
      /// liegt.
      /// </para>
      /// <para>
      /// Wenn die Anzahl der Kreuzungen zwischen <see cref="crossing1"/> und <see cref="crossing2"/> liegt, werden der 1. Radius und Winkel
      /// verwendet, wenn die Anzahl der Kreuzungen größer oder gleich <see cref="crossing2"/> ist, werden der 2. Radius und Winkel verwendet.
      /// </para>
      /// </summary>
      /// <param name="ptcount">Länge der Punktfolge</param>
      /// <param name="crossing1">1. Anzahl der notwendigen "Kreuzungen"</param>
      /// <param name="maxradius1">1. Maximalradius</param>
      /// <param name="minturnaround1">1. min. durchschnittliche Winkelabweichung</param>
      /// <param name="crossing2">2. Anzahl der notwendigen "Kreuzungen"</param>
      /// <param name="maxradius2">2. Maximalradius</param>
      /// <param name="minturnaround2">2. min. durchschnittliche Winkelabweichung</param>
      /// <param name="protfile">Dateiname für eine Protokolldatei</param>
      public void RemoveRestingplace(int ptcount,
                                     int crossing1, double maxradius1, double minturnaround1,
                                     int crossing2, double maxradius2, double minturnaround2,
                                     string? protfile = null) {
         StreamWriter? file = null;
         if (!string.IsNullOrEmpty(protfile))
            try {
               file = new StreamWriter(protfile);       // überschreibt vorhandene Datei
            } catch (Exception ex) {
               Console.Error.WriteLine("Fehler beim Erzeugen der Protokolldatei: " + ex.Message);
            }
         if (file != null) {
            file.WriteLine("RemoveRestingplace() für {0} Punkte", ptcount);
            file.WriteLine("   1) {0} Kreuzungen, Radius {1}, Richtungsänderung {2}", crossing1, maxradius1, minturnaround1);
            file.WriteLine("   2) {0} Kreuzungen, Radius {1}, Richtungsänderung {2}", crossing2, maxradius2, minturnaround2);
            file.WriteLine("Punktindex\tKreuzungen\tRadius\tWinkel\tgelöscht");
         }

         minturnaround1 *= Math.PI / 180;
         minturnaround2 *= Math.PI / 180;

         double rx = 0, ry = 0, radius = 0;
         for (int i = 0; i < Length - ptcount; i++) {
            if (PointIsValid(i)) {
               bool delete = false;

               int crossing = getCrossingCount(i, i + ptcount - 1);
               if (crossing1 <= crossing && crossing < crossing2) {
                  if (minturnaround1 <= getMeanTurnaround(i, i + ptcount - 1))
                     if ((radius = getSmallestCircle(i, i + ptcount - 1, out rx, out ry)) <= maxradius1)
                        delete = true;
               } else
                  if (crossing2 < crossing) {
                  if (minturnaround2 <= getMeanTurnaround(i, i + ptcount - 1))
                     if ((radius = getSmallestCircle(i, i + ptcount - 1, out rx, out ry)) <= maxradius2)
                        delete = true;
               }

               if (file != null)
                  file.Write(string.Format("{0}\t{1}\t{2:F1}\t{3:F1}",
                     i,
                     crossing,
                     getSmallestCircle(i, i + ptcount - 1, out rx, out ry),
                     getMeanTurnaround(i, i + ptcount - 1) * 180 / Math.PI));

               if (delete) {
                  // Wenn Kriterien zutreffen:
                  //    * gültiger Punkt aus dem Intervall mit geringstem Abstand zum Mittelpunkt des Kreises suchen -> bleibt als einziger erhalten
                  //    * alle anderen Punkte des Intervalls ungültig
                  //    * alle folgenden Punkte, die auch in diesem Umkreis sind, sind ungültig
                  //    * mit erstem gültigen Punkt nach dem Intervall weitermachen

                  int valididx = -1;
                  double squaredistance = double.MaxValue;
                  Point m = new Point(rx, ry);
                  for (int j = i; j < i + ptcount - 1; j++) {
                     double dist = m.SquareDistance(get(j));
                     if (dist < squaredistance) {
                        squaredistance = dist;
                        valididx = j;
                     }
                  }
                  for (int j = i; j < i + ptcount - 1; j++)
                     if (j != valididx)
                        setPointValid(j, false);
                     else
                        setPointValid(j);

                  radius *= radius;
                  for (int j = i + ptcount; j < Length - ptcount; j++)
                     if (m.SquareDistance(get(j)) < radius)
                        setPointValid(j, false);
                     else
                        break;
               }
               if (file != null)
                  file.WriteLine("\t{0}", PointIsValid(i) ? 0 : 1);

            } else
               if (file != null)
               file.WriteLine("{0}\t{1}\t{2:F1}\t{3:F1}\t{4}",
                  i,
                  getCrossingCount(i, i + ptcount - 1),
                  getSmallestCircle(i, i + ptcount - 1, out rx, out ry),
                  getMeanTurnaround(i, i + ptcount - 1) * 180 / Math.PI,
                  PointIsValid(i) ? 0 : 1);
         }

         // 1. und letzter Punkt sind immer gültig
         if (Length > 0)
            setPointValid(0);
         if (Length > 1)
            setPointValid(Length - 1);

         if (file != null) {
            for (int i = Length - ptcount; i < Length; i++)
               file.WriteLine("{0}\t\t\t\t{1}", i, PointIsValid(i) ? 0 : 1);
            file.Close();
         }
      }

      /// <summary>
      /// liefert Infos falls die Liste ein Höhenprofil darstellt
      /// </summary>
      /// <param name="all">bei true alle Punkte berücksichtigen, sonst nur gültige</param>
      /// <param name="ascentdescentthreshold">Schwellwert für die Berechnung von <paramref name="dAscent"/> und <paramref name="dDescent"/></param>
      /// <param name="dAscent">Anstieg in m</param>
      /// <param name="dDescent">Abstieg in m</param>
      /// <param name="dMaxHeigth">größte Höhe (ungültig wenn <see cref="double.MinValue"/></param>
      /// <param name="dMinHeigth">niedrigste Höhe (ungültig wenn <see cref="double.MaxValue"/></param>
      /// <param name="dStartHeigth">Starthöhe (ungültig wenn <see cref="double.MaxValue"/></param>
      /// <param name="dEndHeigth">Endhöhe (ungültig wenn <see cref="double.MaxValue"/></param>
      public void GetHeightInfos(bool all,
                                 double ascentdescentthreshold,
                                 out double dAscent,
                                 out double dDescent,
                                 out double dMaxHeigth,
                                 out double dMinHeigth,
                                 out double dStartHeigth,
                                 out double dEndHeigth) {
         dAscent = 0.0;
         dDescent = 0.0;
         dMaxHeigth = double.MinValue;
         dMinHeigth = double.MaxValue;
         dStartHeigth = double.MaxValue;
         dEndHeigth = double.MaxValue;

         double dLastHeigth = double.MaxValue;

         for (int i = 0; i < Length; i++)
            if (all || PointIsValid(i)) {
               double heigth = Get(i).Y;

               if (heigth != double.MaxValue) {
                  if (dMaxHeigth < heigth)
                     dMaxHeigth = heigth;
                  if (dMinHeigth > heigth)
                     dMinHeigth = heigth;
                  if (dStartHeigth == double.MaxValue)
                     dStartHeigth = heigth;
                  dEndHeigth = heigth;

                  double diff = 0;
                  if (dLastHeigth == double.MaxValue)
                     dLastHeigth = heigth;
                  else
                     diff = heigth - dLastHeigth;
                  if (Math.Abs(diff) >= ascentdescentthreshold) {
                     if (diff > 0)
                        dAscent += diff;
                     else
                        dDescent -= diff;
                     dLastHeigth = heigth;
                  }
               }
            }
      }





      private void DouglasPeuckerRecursive(int iStart, int iEnd, double dSquareWidth) {
         int idx = GetFarPointIdx4DouglasPeucker(iStart, iEnd, dSquareWidth);
         if (idx > 0) {
            setPointValid(idx);
            if (idx - iStart > 1)
               DouglasPeuckerRecursive(iStart, idx, dSquareWidth);
            if (iEnd - idx > 1)
               DouglasPeuckerRecursive(idx, iEnd, dSquareWidth);
         }
      }

      private int GetFarPointIdx4DouglasPeucker(int iStart, int iEnd, double dSquareWidth) {
         double dMaxSquareWidth = dSquareWidth;
         int idx = -1;
         for (int i = iStart + 1; i < iEnd - 1; i++) {
            PointD pBaseLine = get(iEnd) - get(iStart);
            PointD pTestLine = get(i) - get(iStart);
            double dDotProduct = pBaseLine.DotProduct(pTestLine);
            double dSquare_AbsBaseLine = pBaseLine.SquareAbsolute();
            double dSquare_WidthTest = (dSquare_AbsBaseLine * pTestLine.SquareAbsolute() - dDotProduct * dDotProduct) / dSquare_AbsBaseLine;
            if (dMaxSquareWidth < dSquare_WidthTest) {
               dMaxSquareWidth = dSquare_WidthTest;
               idx = i;
            }
         }
         return idx;
      }

      #region interne Hilfsfunktionen für die Pausenplatz-Erkennung

      /// <summary>
      /// Hilfsklasse zur Berechnung des kleinsten Umkreises
      /// </summary>
      protected class specCircle {

         /// <summary>
         /// Radius des Kreises
         /// </summary>
         public readonly double Radius;
         /// <summary>
         /// Quadrat des Radius des Kreises (bei einigen Berechnungen nützlich)
         /// </summary>
         public readonly double SquareRadius;
         /// <summary>
         /// Mittelpunkt des Kreises
         /// </summary>
         public readonly Point Center;

         public specCircle()
            : this(0, 0, 0) {
         }

         public specCircle(double mx, double my, double r) {
            Radius = r;
            SquareRadius = r * r;
            Center = new Point(mx, my);
         }

         public specCircle(Point Center, double Radius)
            : this(Center.X, Center.Y, Radius) {
         }

         /// <summary>
         /// Kreis durch 3 Punkte auf dem Rand definiert (Umkreis)
         /// <para>Der Radius 0 ist ein Zeichen dafür, dass kein Kreis gefunden werden konnte.</para>
         /// </summary>
         /// <param name="pa"></param>
         /// <param name="pb"></param>
         /// <param name="pc"></param>
         public specCircle(Point pa, Point pb, Point pc)
            : this() {
            /* Der Mittelpunkt des Umkreises ist der Schnittpunkt der 3 Mittelsenkrechten der Seiten des durch A, B, und C
             * gebildeten Dreiecks.
             * Zur einfacheren Berechnung wird zunächst das gesamte Dreieck um -A verschoben, so dass A im Koordinatensprung
             * liegt. Dann werden die beiden Mittelsenkrechten der Strecken AB und AC betrachtet. D sei der Mittelpunkt von AB,
             * E der Mittelpunkt von AC. D ist nun einfach (xb/2, yb/2) und E (xc/2, yc/2). Ein zu AB orthogonaler Vektor (yb, -xb) 
             * beschreibt die Richtung der Mittelsenkrechten in D, der Vektor (yc, -xc) gilt entsprechend für E.
             * Mit M=D+gamma*(yb, -xb) und M=E+delta*(yc, -xc) können 4 Gleichungen formuliert werden, die xm, ym, gamma und delta
             * als Unbekannte enthalten. Die entsprechende Lösung für xm und ym wird für die Berechnung verwendet (s.u.).
             */

            // A temp. als Koordinatenursprung verwenden, um die Berechnung zu vereinfachen -> neue Koordinaten für B und C:
            double xb = pb.X - pa.X;
            double yb = pb.Y - pa.Y;
            double xc = pc.X - pa.X;
            double yc = pc.Y - pa.Y;

            if ((xb == 0 && yb == 0) ||      // A == B
                (xc == 0 && yc == 0) ||      // A == C
                (xb == xc && yb == yc))      // B == C
               return;              // Für (teilweise) identische Punkte wird keine Lösung gesucht.

            /* Wenn der Anstieg mc=yc/xc und mb=yb/xb gleich ist müssen A, B, und auf einer Geraden liegen -> keine Lösung möglich */
            if (xb * yc == xc * yb)
               return;

            double d = 2 * (xb * yc - yb * xc);
            double xm = (yb * (xc * xc + yc * yc) - yc * (xb * xb + yb * yb)) / d;
            double ym = (xb * (xc * xc + yc * yc) - xc * (xb * xb + yb * yb)) / d;

            // Der Radius ist der Abstand von diesem M zum Koordinatenursprung (A).
            SquareRadius = xm * xm + ym * ym;
            Radius = Math.Sqrt(SquareRadius);

            // M noch verschieben, da A i.A. NICHT der Koordinatenursprung ist.
            Center.X = pa.X + xm;
            Center.Y = pa.Y + ym;

            // Da der Mittelpunkt i.A. nur näherungsweise durch double-Zahlen dargestellt werden kann, kann es passieren, dass pa, pb oder pc
            // nicht die Contains()-Funktion erfüllen!
            // Da diese Funktion sehr wichtig ist, wird der Radius notfalls lieber etwas größer gesetzt.
            SquareRadius = Math.Max(Math.Max(squareDistance(pa, Center), squareDistance(pb, Center)), squareDistance(pc, Center));
            Radius = Math.Sqrt(SquareRadius);
         }

         public specCircle(Point A, Point B)
            : this(A.X + (B.X - A.X) / 2,
                   A.Y + (B.Y - A.Y) / 2,
                   Math.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y)) / 2) {
            // Da der Mittelpunkt i.A. nur näherungsweise durch double-Zahlen dargestellt werden kann, kann es passieren, dass A oder B
            // nicht die Contains()-Funktion erfüllen!
            // Da diese Funktion sehr wichtig ist, wird der Radius notfalls lieber etwas größer gesetzt.
            SquareRadius = Math.Max(squareDistance(A, Center), squareDistance(B, Center));
            Radius = Math.Sqrt(SquareRadius);
         }

         /// <summary>
         /// Ist der Punkt innerhalb oder auf dem Kreis?
         /// </summary>
         /// <param name="p"></param>
         /// <returns></returns>
         bool contains(Point p) {
            return squareDistance(p, Center) <= SquareRadius;
         }

         /// <summary>
         /// Quadrat des Abstandes der 2 Punkte
         /// </summary>
         /// <param name="a"></param>
         /// <param name="b"></param>
         /// <returns></returns>
         double squareDistance(Point a, Point b) {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
         }

         /// <summary>
         /// Naiver Brute Force-Algorithmus zum Finden des Bounding Balls - O(n^4), d.h. nur für kleine Punktmengen geeignet
         /// </summary>
         /// <param name="pts"></param>
         /// <returns></returns>
         static specCircle smallestCircleNaive(IList<Point> pts) {
            specCircle best = new specCircle(0, 0, double.MaxValue);     // Start-Kreis (enthält mit Sicherheit alle Punkte)

            // Alle sinnvollen 2-Punkte-Kombinationen ausprobieren und die Beste ermitteln.
            for (int i = 0; i < pts.Count; i++) {
               for (int j = i + 1; j < pts.Count; j++) {
                  specCircle test = new specCircle(pts[i], pts[j]);

                  bool bContainsAll = true;
                  for (int k = 0; k < pts.Count; k++)
                     if (k != i &&
                         k != j &&
                         !test.contains(pts[k])) {
                        bContainsAll = false;
                        break;
                     }

                  if (bContainsAll &&
                      test.Radius < best.Radius)
                     best = test;
               }
            }

            if (best.Radius == double.MaxValue)       // keine 2-Punkte-Kombination gefunden, die alle anderen Punkte enthält
               // Alle sinnvollen 3-Punkte-Kombinationen ausprobieren und die Beste ermitteln.
               for (int i = 0; i < pts.Count; i++) {
                  for (int j = i + 1; j < pts.Count; j++) {
                     for (int k = j + 1; k < pts.Count; k++) {
                        bool bContainsAll = true;
                        specCircle test = new specCircle(pts[i], pts[j], pts[k]);

                        for (int l = 0; l < pts.Count; l++)
                           if (l != i &&
                               l != j &&
                               l != k &&
                               !test.contains(pts[l])) {
                              bContainsAll = false;
                              break;
                           }

                        if (bContainsAll &&
                            test.Radius < best.Radius)
                           best = test;
                     }
                  }
               }
            return best;
         }

         /// <summary>
         /// ermittelt den kleinsten Kreis, der alle gegebenen Punkte einschließt
         /// </summary>
         /// <param name="pts">gegebenen Punkte</param>
         /// <param name="max_generation">damit könnte die Iteration abgebrochen werden</param>
         /// <returns></returns>
         static public specCircle SmallestCircle(IList<Point> pts, int max_generation = int.MaxValue) {
            Random rand = new Random();
            specCircle circle = new specCircle();

            if (pts.Count > 13) {
               List<int> voices = new List<int>();       // "Stimmen" je Punkt
               for (int i = 0; i < pts.Count; i++)
                  voices.Add(1);                         // am Anfang alle gleichberechtigt: je 1 Stimme

               int iGeneration = 0;
               int iMistakes = 0;

               HashSet<int> voiceindices = new HashSet<int>();                            // Indizes der ausgewählten Stimmzettel speichern
               List<Point> randpts = new List<Point>();       // ausgewählte Punkte speichern
               do {
                  iGeneration++;
                  voiceindices.Clear();
                  randpts.Clear();

                  // Gesamtzahl der Stimmen im "Lostopf"
                  int voicessum = 0;
                  for (int i = 0; i < voices.Count; i++)
                     voicessum += voices[i];

                  // 13 zufällige, per Stimmzahl gewichtete Punkte wählen
                  while (randpts.Count < 13) {
                     int idx = rand.Next(0, voicessum);        // "Stimmzettel auslosen"

                     if (!voiceindices.Contains(idx)) {        // dieser Stimmzettel wurde bisher noch nicht gezogen
                        voiceindices.Add(idx);

                        int p = 0;
                        int lastvoiceidx = voices[0] - 1;      // Index des letzten Stimmzettels für den Punkt p

                        while (lastvoiceidx < idx)
                           lastvoiceidx += voices[++p];

                        if (!randpts.Contains(pts[p]))
                           randpts.Add(pts[p]);
                     }
                  }

                  //randpts.Clear();
                  //randpts.Add(pts[4]);
                  //randpts.Add(pts[9]);
                  //randpts.Add(pts[7]);
                  //randpts.Add(pts[6]);
                  //randpts.Add(pts[3]);
                  //randpts.Add(pts[5]);
                  //randpts.Add(pts[0]);
                  //randpts.Add(pts[10]);
                  //randpts.Add(pts[2]);
                  //randpts.Add(pts[14]);
                  //randpts.Add(pts[13]);
                  //randpts.Add(pts[11]);
                  //randpts.Add(pts[8]);

                  circle = smallestCircleNaive(randpts);

                  // Fehler zählen und Stimmen angleichen
                  iMistakes = 0;
                  for (int i = 0; i < pts.Count; i++) {
                     if (!circle.contains(pts[i])) {           // Punkt nicht enthalten
                        voices[i] *= 2;                        // --> seine "Stimmen" verdoppeln
                        iMistakes++;
                     }
                  }
                  //Debug.WriteLine(string.Format("In Generation {0} noch {1} Fehler", m_Generation, iNumMistakes));
               } while (iMistakes > 0 &&
                        max_generation > iGeneration);

            } else
               circle = smallestCircleNaive(pts);

            return circle;
         }

         public override string ToString() {
            return string.Format("Radius {0}, Mittelpunkt {1}", Radius, Center); ;
         }

      }

      /// <summary>
      /// ermittelt den kleinsten Kreis, der die ausgewählten Punkte umschließt
      /// </summary>
      /// <param name="firstidx">Index des 1. zu berücksichtigenden Punktes</param>
      /// <param name="lastidx">Index des letzten zu berücksichtigenden Punktes</param>
      /// <param name="rx">x-Koordinate des Mittepunktes</param>
      /// <param name="ry">y-Koordinate des Mittepunktes</param>
      /// <returns>Radius</returns>
      double getSmallestCircle(int firstidx, int lastidx, out double rx, out double ry) {
         double radius = 0;
         rx = ry = 0;
         List<Point> pts = new List<Point>();
         for (int i = firstidx; i <= lastidx && i < Length; i++)
            pts.Add(get(i));
         if (pts.Count >= 3) {
            specCircle c = specCircle.SmallestCircle(pts);
            radius = c.Radius;
            rx = c.Center.X;
            ry = c.Center.Y;
         } else if (pts.Count == 2) {
            radius = pts[0].Distance(pts[1]) / 2;
            rx = pts[0].X + (pts[1].X - pts[0].X) / 2;
            ry = pts[0].Y + (pts[1].Y - pts[0].Y) / 2;
         } else if (pts.Count == 1) {
            rx = pts[0].X;
            ry = pts[0].Y;
         }
         return radius;
      }

      //static public void Test_SmallestCircle() {
      //   for (int i = 0; i < 20; i++) {
      //      List<Point> pts = new List<Point>();
      //      pts.Add(new Point(0, 0));
      //      pts.Add(new Point(17.8007315345031, 28.6832299716601));
      //      pts.Add(new Point(17.7111520136521, 38.4772763330034));
      //      pts.Add(new Point(17.7303475344727, 60.7119048945307));
      //      pts.Add(new Point(17.7815356473597, 63.6286855901544));
      //      pts.Add(new Point(17.2440604466446, 64.4860141013281));
      //      pts.Add(new Point(19.0996258439946, 72.8729234497667));
      //      pts.Add(new Point(19.3939569624157, 74.9137380578868));
      //      pts.Add(new Point(19.6371000206965, 76.246324765472));
      //      pts.Add(new Point(20.340934295968, 81.7816849354415));
      //      pts.Add(new Point(24.5191373006249, 96.2910381082404));
      //      pts.Add(new Point(28.1982545310626, 106.429879631686));
      //      pts.Add(new Point(32.1716946188981, 116.391664179998));
      //      pts.Add(new Point(33.5857526966127, 120.594437642383));
      //      pts.Add(new Point(36.2667029811656, 124.172852297716));

      //      System.Threading.Thread.Sleep(200);
      //      Circle c = Circle.SmallestCircle(pts);

      //      Console.WriteLine(c);
      //   }
      //}

      /// <summary>
      /// liefert die durchschnittliche Winkelabweichung von Punkt zu Punkt (0..Math.PI)
      /// </summary>
      /// <param name="firstidx"></param>
      /// <param name="lastidx"></param>
      /// <returns></returns>
      double getMeanTurnaround(int firstidx, int lastidx) {
         double ta = 0;
         int count = 0;
         for (int i = firstidx + 2; i <= lastidx && i < Length; i++) {
            count++;
            PointD p1 = get(i - 2);
            PointD p2 = get(i - 1);
            PointD p3 = get(i);
            p3 -= p2;
            p2 -= p1;
            double arc = p2.Arc(p3);
            ta += arc;
            //Debug.WriteLine("Winkel {0}°, Punk1 {1}, Punk2 {2}", arc * 180 / Math.PI, p2, p3);
         }
         return count > 1 ? ta / count : ta;
      }

      /// <summary>
      /// liefert die durchschnittliche Winkelabweichung von Punkt zu Punkt (0..Math.PI) und die zugehörige Standardabweichung
      /// (die Standardabweichung hat leider keine Zusammenhang zur "Pause")
      /// </summary>
      /// <param name="firstidx"></param>
      /// <param name="lastidx"></param>
      /// <param name="sa">Standardabweichung</param>
      /// <returns></returns>
      //double getMeanTurnaround_V2(int firstidx, int lastidx, out double sa) {
      //   List<double> v = new List<double>();
      //   for (int i = firstidx + 2; i <= lastidx && i < Length; i++) {
      //      Point p1 = pt[i - 2];
      //      Point p2 = pt[i - 1];
      //      Point p3 = pt[i];
      //      p3 -= p2;
      //      p2 -= p1;
      //      v.Add(p2.Arc(p3));
      //   }

      //   double ta = 0;
      //   for (int i = 0; i < v.Count; i++)
      //      ta += v[i];
      //   if (v.Count > 0)
      //      ta /= v.Count;          // Durchschnitt

      //   double var = 0;
      //   for (int i = 0; i < v.Count; i++)
      //      var += (ta - v[i]) * (ta - v[i]);
      //   sa = Math.Sqrt(var);

      //   return ta;
      //}

      /// <summary>
      /// liefert die Anzahl der Schnittpunkte für diesen Polygonzug
      /// </summary>
      /// <param name="firstidx"></param>
      /// <param name="lastidx"></param>
      /// <returns></returns>
      int getCrossingCount(int firstidx, int lastidx) {
         int crossing = 0;

         for (int i = firstidx; i <= lastidx - 1 && i < Length - 1; i++) {
            Point p1 = get(i);
            Point p2 = get(i + 1);
            for (int j = i + 1; j <= lastidx - 1 && j < Length - 1; j++) {
               Point p3 = get(j);
               Point p4 = get(j + 1);

               if (inRightHalfplane(p1, p2, p3) * inRightHalfplane(p1, p2, p4) < 0 &&    // p3 und p4 in unterschiedlichen Halbebenen bzgl. p1p2
                   inRightHalfplane(p3, p4, p1) * inRightHalfplane(p3, p4, p2) < 0)      // p1 und p2 in unterschiedlichen Halbebenen bzgl. p3p4
                  crossing++;
            }
         }

         return crossing;
      }

      /// <summary>
      /// ermittelt die Lage von p2 bzgl. des Vektors p0p1
      /// </summary>
      /// <param name="p0"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns>1 wenn p2 rechts liegt, -1 wenn p2 links liegt</returns>
      int inRightHalfplane(Point p0, Point p1, Point p2) {
         //dx1:=p1.x-p0.x; dy1:=p1.y-p0.y;
         //dx2:=p2.x-p0.x; dy2:=p2.y-p0.y;
         //if dx1*dy2>dy1*dx2 then ccw:=1;
         //if dx1*dy2<dy1*dx2 then ccw:=-1;
         //if dx1*dy2=dy1*dx2 then 
         //  begin
         //  if (dx1*dx2<0) or (dy1*dy2<0) then ccw:=-1 else
         //  if (dx1*dx1+dy1*dy1)>=(dx2*dx2+dy2*dy2) then ccw:=0 else ccw:=1;
         //  end;
         double dx1 = p1.X - p0.X;
         double dy1 = p1.Y - p0.Y;
         double dx2 = p2.X - p0.X;
         double dy2 = p2.Y - p0.Y;
         if (dx1 * dy2 > dy1 * dx2)
            return 1;
         if (dx1 * dy2 < dy1 * dx2)
            return -1;
         // dx1 * dy2 == dy1 * dx2  --> Anstieg identisch
         if (dx1 * dx2 < 0 || dy1 * dy2 < 0)
            return -1;
         if ((dx1 * dx1 + dy1 * dy1) >= (dx2 * dx2 + dy2 * dy2))
            return 0;
         return 1;
      }

      #endregion

      #region RidgeSplineRegression

      /// <summary>
      /// führt die Regression für die Punkte aus und berechnet die neue y-Werte
      /// <para>Die neuen Werte werden im <paramref name="Y"/>-Array gespeichert.</para>
      /// </summary>
      /// <param name="rr"></param>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="lambda"></param>
      /// <param name="ptcount">Anzahl der zu berücksichtigenden Punkte</param>
      void ridgeSplineRegression(RidgeSplineRegression rr, double[] X, ref double[] Y, double lambda, int ptcount = 0) {
         if (ptcount > X.Length || ptcount > Y.Length)
            throw new ArgumentException(nameof(ptcount) + " fehlerhaft");
         ptcount = Math.Min(Math.Min(ptcount, X.Length), Y.Length);

         RidgeSplineRegression.Normalize(ref X, out double minx, out double fx, ptcount);
         RidgeSplineRegression.Normalize(ref Y, out double miny, out double fy, ptcount);

         double[,] xm = RidgeSplineRegression.BuildX(X, ptcount);
         if (ptcount == Y.Length)
            rr.CalculateParams(xm, Y, lambda);
         else {
            double[] tmp = new double[ptcount];
            Array.Copy(Y, tmp, ptcount);
            rr.CalculateParams(xm, tmp, lambda);
         }
         for (int i = 0; i < ptcount; i++)
            Y[i] = RidgeSplineRegression.Denormalize(rr.Calculate(xm, X[i]), miny, fy);
      }

      public class RidgeSplineRegression {

         /// <summary>
         /// Hilfsfunktionen für 2dim Matrix
         /// </summary>
         class Matrix {

            static double[,] create(int row, int col) => new double[row, col];

            static double get(double[,] m, int row, int col) => m[row, col];

            static void set(double[,] m, int row, int col, double value) => m[row, col] = value;

            static void add(double[,] m, int row, int col, double value) => m[row, col] += value;

            public static int Cols(double[,] m) => m.GetLength(1);

            public static int Rows(double[,] m) => m.GetLength(0);

            /// <summary>
            /// Adds two matrices of the same size. (Result also in <paramref name="m1"/>)
            /// </summary>
            /// <param name="m1"></param>
            /// <param name="m2"></param>
            /// <returns></returns>
            public static double[,] Add(double[,] m1, double[,] m2) {
               if (Rows(m1) != Rows(m2) ||
                   Cols(m1) != Cols(m2))
                  throw new Exception(nameof(Add) + " nicht möglich");
               for (int r = 0; r < Rows(m1); r++)
                  for (int c = 0; c < Cols(m1); c++)
                     add(m1, r, c, get(m2, r, c));
               return m1;
            }

            public static double[,] Mul(double[,] m, double[] v) {
               double[,] b1;
               if (Cols(m) == v.Length) {
                  b1 = create(v.Length, 1);   // Vektor als Spalte
                  for (int r = 0; r < v.Length; r++)
                     set(b1, r, 0, v[r]);
               } else {
                  b1 = create(1, v.Length);   // Vektor als Zeile
                  for (int c = 0; c < v.Length; c++)
                     set(b1, 0, c, v[c]);
               }
               return Mul(m, b1);
            }

            public static double[,] Mul(double[,] a, double[,] b) {
               if (Cols(a) != Rows(b))
                  throw new Exception(nameof(Mul) + " nicht möglich");
               double[,] m = create(Rows(a), Cols(b));

               for (int r = 0; r < Rows(m); r++) {         // jede Zeile in a ...
                  for (int c = 0; c < Cols(m); c++) {      // ... wird mit jeder Spalte in b ...
                     set(m, r, c, 0);
                     for (int k = 0; k < Cols(a); k++)        // ... multipliziert
                        add(m, r, c, get(a, r, k) * get(b, k, c));
                  }
               }
               return m;
            }

            public static double[,] Transpose(double[,] m) {
               int orgcols = Cols(m);
               int orgrows = Rows(m);
               double[,] t = create(orgcols, orgrows);
               for (int c = 0; c < orgcols; c++)
                  for (int r = 0; r < orgrows; r++)
                     set(t, c, r, get(m, r, c));
               return t;
            }

            public static double[,] Invert(double[,] m) {
               int n = Cols(m);
               if (n != Rows(m))
                  throw new Exception(nameof(Invert) + " nicht möglich");

               double[,] I = new double[n, n];
               double[,] C = new double[n, n];

               for (int i = 0; i < n; i++) {
                  for (int j = 0; j < n; j++) {
                     if (i == j)
                        I[i, j] = 1;
                     else
                        I[i, j] = 0;
                     C[i, j] = m[i, j];
                  }
               }

               for (int i = 0; i < n; i++) {
                  double e = C[i, i];

                  if (e == 0) {
                     for (int ii = i + 1; ii < n; ii++) {
                        if (C[ii, i] != 0) {
                           for (int j = 0; j < n; j++) {
                              e = C[i, j];
                              C[i, j] = C[ii, j];
                              C[ii, j] = e;
                              e = I[i, j];
                              I[i, j] = I[ii, j];
                              I[ii, j] = e;
                           }
                           break;
                        }
                     }

                     e = C[i, i];
                     if (e == 0)
                        throw new Exception("Matrix-Invers nicht möglich.");

                  }

                  for (int j = 0; j < n; j++) {
                     C[i, j] /= e;
                     I[i, j] /= e;
                  }

                  for (int ii = 0; ii < n; ii++) {
                     if (ii == i)
                        continue;
                     e = C[ii, i];
                     for (int j = 0; j < n; j++) {
                        C[ii, j] -= e * C[i, j];
                        I[ii, j] -= e * I[i, j];
                     }
                  }
               }
               return I;
            }

            #region Invert, Version 2

            //public static double[,] inverse(double[,] matrix) {
            //   int n = Rows(matrix);
            //   double[,] result = copy(matrix);

            //   int[] perm;
            //   int toggle;
            //   double[,] lum = decompose(matrix, out perm, out toggle);
            //   if (lum == null)
            //      throw new Exception("Unable to compute inverse");

            //   double[] b = new double[n];
            //   for (int i = 0; i < n; ++i) {
            //      for (int j = 0; j < n; ++j)
            //         b[j] = i == perm[j] ? 1.0 : 0.0;

            //      double[] x = helperSolve(lum, b);

            //      for (int j = 0; j < n; ++j)
            //         result[j, i] = x[j];
            //   }
            //   return result;
            //}

            //static double[,] copy(double[,] matrix) {
            //   double[,] result = create(Rows(matrix), Cols(matrix));
            //   for (int i = 0; i < Rows(matrix); ++i)
            //      for (int j = 0; j < Cols(matrix); ++j)
            //         set(result, i, j, get(matrix, i, j));
            //   return result;
            //}

            //static double[,] decompose(double[,] matrix, out int[] perm, out int toggle) {
            //   int rows = Rows(matrix);
            //   int cols = Cols(matrix); // assume square
            //   if (rows != cols)
            //      throw new Exception("Attempt to decompose a non-square m");

            //   int n = rows; // convenience

            //   double[,] result = copy(matrix);

            //   perm = new int[n]; // set up row permutation result
            //   for (int i = 0; i < n; ++i) { perm[i] = i; }

            //   toggle = 1; // toggle tracks row swaps.
            //               // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            //   for (int j = 0; j < n - 1; ++j) { // each column
            //      double colMax = Math.Abs(result[j, j]); // find largest val in col
            //      int pRow = j;
            //      //for (int i = j + 1; i less-than n; ++i)
            //      //{
            //      //  if (result[i][j] greater-than colMax)
            //      //  {
            //      //    colMax = result[i][j];
            //      //    pRow = i;
            //      //  }
            //      //}

            //      // reader Matt V needed this:
            //      for (int i = j + 1; i < n; ++i) {
            //         if (Math.Abs(result[i, j]) > colMax) {
            //            colMax = Math.Abs(result[i, j]);
            //            pRow = i;
            //         }
            //      }
            //      // Not sure if this approach is needed always, or not.

            //      if (pRow != j) { // if largest value not on pivot, swap rows
            //         // vertausche Zeile pRow und j
            //         //double[] rowPtr = result[pRow];
            //         //result[pRow] = result[j];
            //         //result[j] = rowPtr;
            //         swapRow(result, j, pRow);

            //         int tmp = perm[pRow]; // and swap perm info
            //         perm[pRow] = perm[j];
            //         perm[j] = tmp;

            //         toggle = -toggle; // adjust the row-swap toggle
            //      }

            //      // --------------------------------------------------
            //      // This part added later (not in original) and replaces the 'return null' below.
            //      // If there is a 0 on the diagonal, find a good row from i = j+1 down that doesn't have
            //      // a 0 in column j, and swap that good row with row j
            //      // --------------------------------------------------

            //      if (result[j, j] == 0.0) {
            //         // find a good row to swap
            //         int goodRow = -1;
            //         for (int row = j + 1; row < n; ++row) {
            //            if (result[row, j] != 0.0)
            //               goodRow = row;
            //         }

            //         if (goodRow == -1)
            //            throw new Exception("Cannot use Doolittle's method");

            //         // swap rows so 0.0 no longer on diagonal
            //         //double[] rowPtr = result[goodRow];
            //         //result[goodRow] = result[j];
            //         //result[j] = rowPtr;
            //         swapRow(result, j, goodRow);

            //         int tmp = perm[goodRow]; // and swap perm info
            //         perm[goodRow] = perm[j];
            //         perm[j] = tmp;

            //         toggle = -toggle; // adjust the row-swap toggle
            //      }
            //      // --------------------------------------------------
            //      // if diagonal after swap is zero . .
            //      //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
            //      //  return null; // consider a throw

            //      for (int i = j + 1; i < n; ++i) {
            //         result[i, j] /= result[j, j];
            //         for (int k = j + 1; k < n; ++k) {
            //            result[i, k] -= result[i, j] * result[j, k];
            //         }
            //      }
            //   }
            //   return result;
            //}

            //static void swapRow(double[,] m, int r1, int r2) {
            //   double tmp;
            //   for (int c = 0; c < Cols(m); c++) {
            //      tmp = get(m, r1, c);
            //      set(m, r1, c, get(m, r2, c));
            //      set(m, r2, c, tmp);
            //   }
            //}

            //static double[] helperSolve(double[,] luMatrix, double[] b) {
            //   // before calling this helper, permute b using the perm array from MatrixDecompose that generated luMatrix
            //   int n = luMatrix.Length;
            //   double[] x = new double[n];
            //   b.CopyTo(x, 0);

            //   for (int i = 1; i < n; ++i) {
            //      double sum = x[i];
            //      for (int j = 0; j < i; ++j)
            //         sum -= luMatrix[i, j] * x[j];
            //      x[i] = sum;
            //   }

            //   x[n - 1] /= luMatrix[n - 1, n - 1];
            //   for (int i = n - 2; i >= 0; --i) {
            //      double sum = x[i];
            //      for (int j = i + 1; j < n; ++j)
            //         sum -= luMatrix[i, j] * x[j];
            //      x[i] = sum / luMatrix[i, i];
            //   }

            //   return x;
            //}

            #endregion

            /// <summary>
            /// Returns the identity matrix for a given size times lambda
            /// </summary>
            /// <param name="size"></param>
            /// <param name="lambda"></param>
            /// <returns></returns>
            public static double[,] Identity(int size, double lambda = 1) {
               double[,] m = create(size, size);
               for (int i = 0; i < size; i++)   // Then populate the diagonal with lambda
                  set(m, i, i, lambda);
               return m;
            }

            public static double[]? AsVektor(double[,] m) {
               if (Cols(m) == 1) {
                  int elems = Rows(m);
                  double[] v = new double[elems];
                  for (int r = 0; r < elems; r++)
                     v[r] = get(m, r, 0);
                  return v;
               }

               if (Rows(m) == 1) {
                  int elems = Cols(m);
                  double[] v = new double[elems];
                  for (int c = 0; c < elems; c++)
                     v[c] = get(m, 0, c);
                  return v;
               }

               return null;
            }

         }


         public double[]? CoeffVector { get; protected set; }


         public RidgeSplineRegression() { }

         /// <summary>
         /// erzeugt die Variablenmatrix für die Berechnung
         /// </summary>
         /// <param name="x"></param>
         /// <param name="count"></param>
         /// <returns></returns>
         public static double[,] BuildX(double[] x, int count = 0) {
            int rows = x.Length;
            if (count > 0)
               rows = Math.Min(rows, count);
            double[,] X = new double[rows, 4 + rows];
            for (int i = 0; i < rows; i++) {
               X[i, 0] = 1;                        // x^0
               X[i, 1] = x[i];                     // x^1
               X[i, 2] = x[i] * x[i];              // x^2
               X[i, 3] = x[i] * x[i] * x[i];       // x^3

               for (int c = 0; c < i; c++) {       // 3. Potenz des Abstandes zu den anderen Punkten
                  double dx = x[i] - x[c];
                  X[i, 4 + c] = dx * dx * dx;
               }
            }
            return X;
         }

         /// <summary>
         /// berechnet den <see cref="CoeffVector"/>
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <param name="lambda"></param>
         public void CalculateParams(double[,] x, double[] y, double lambda) {
            //int cols = Matrix.Cols(x);
            //double[,] tx2 = Matrix.Transpose(x);
            //double[,] tmp1 = Matrix.Mul(tx2, x);
            //double[,] tmp2 = Matrix.Identity(cols, lambda);
            //double[,] tmp3 = Matrix.Add(tmp1, tmp2);
            //double[,] tmp4 = Matrix.Invert(tmp3);
            //double[,] tmp5 = Matrix.Mul(tmp4, tx2);
            //double[,] tmp6 = Matrix.Mul(tmp5, y);
            //CoeffVector = Matrix.AsVektor(tmp6);

            double[,] tx = Matrix.Transpose(x);
            CoeffVector = Matrix.AsVektor(
                              Matrix.Mul(
                                 Matrix.Mul(
                                    Matrix.Invert(
                                       Matrix.Add(
                                          Matrix.Mul(tx, x),
                                             Matrix.Identity(Matrix.Cols(x), lambda))), tx), y));
         }

         /// <summary>
         /// normiert die Werte auf den Bereich 0..1
         /// </summary>
         /// <param name="v"></param>
         /// <param name="min">Min. der Werte</param>
         /// <param name="f">Faktor</param>
         /// <param name="count">wenn kleiner oder 0 alle Werte im Array, sonst max. diese Anzahl</param>
         public static void Normalize(ref double[] v, out double min, out double f, int count = 0) {
            double max = 0;
            min = double.MaxValue;
            if (count <= 0)
               count = v.Length;
            else
               count = Math.Min(count, v.Length);
            for (int i = 0; i < count; i++) {
               min = double.Min(min, v[i]);
               max = double.Max(max, v[i]);
            }
            f = max - min;
            for (int i = 0; i < count; i++)
               v[i] = (v[i] - min) / f;
         }

         /// <summary>
         /// liefert den ursprünglichen Wert
         /// </summary>
         /// <param name="v"></param>
         /// <param name="min"></param>
         /// <param name="f"></param>
         /// <returns></returns>
         public static double Denormalize(double v, double min, double f) => v * f + min;

         /// <summary>
         /// berechnet aus der Variablenmatrix <paramref name="m"/> und dem Wert <paramref name="x"/> das angenäherte y
         /// </summary>
         /// <param name="m"></param>
         /// <param name="x"></param>
         /// <returns></returns>
         public double Calculate(double[,] m, double x) {
            int rows = Matrix.Rows(m);

            double[] xv = new double[4 + rows];

            xv[0] = 1;
            xv[1] = x;
            xv[2] = x * x;
            xv[3] = x * x * x;

            if (x > m[rows - 1, 1]) {
               for (int c = 0; c < rows; c++) {
                  double dx = x - m[c, 1];
                  xv[4 + c] = dx * dx * dx;
               }
            } else
               for (int r = 0; r < rows; r++) {
                  if (x <= m[r, 1]) {    // x gehört in den Abschnitt (r-1)..r, d.h.  ?
                     for (int c = 0; c < r; c++) {
                        double dx = x - m[c, 1];
                        xv[4 + c] = dx * dx * dx;
                     }
                     break;
                  }
               }

            return calculate(xv);
         }


         /// <summary>
         /// berechnet den Wert zum Variablenvektor
         /// </summary>
         /// <param name="vars"></param>
         /// <returns></returns>
         double calculate(double[] vars) {
            if (CoeffVector != null && vars.Length == CoeffVector.Length) {
               double sum = 0;
               for (int i = 0; i < CoeffVector.Length; i++)
                  sum += CoeffVector[i] * vars[i];
               return sum;
            }
            return double.NaN;
         }

      }

      #endregion
   }

}