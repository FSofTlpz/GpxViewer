/*
Copyright (C) 2016 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// zur Verwaltung eines umgebenden "Rechtecks"
/// </summary>
namespace GarminCore {
   public class Bound {

      /*
       * Bei der oberen und unteren Begrenzung wird immer davon ausgegangen, dass sie nicht kleiner als 90° und nicht größer als +90° sein kann. Intern erfolgt eine Begrenzung
       * auf diese Werte. (Der Nord- bzw. Südpol kann also nicht "überschritten" werden.) 
       * Führt eine Zuweisung dazu, das die obere Begrenzung kleiner als die untere Begrenzung ist, erfolgt intern eine Vertauschung der beiden Werte.
       * 
       * Die linke und rechte Begrenzung kann den Bereich -180° bis +180° nicht überschreiten. Wenn linke und rechte Begrenzung identisch sind, wird davon ausgegangen, dass
       * die Breite 0° ist und NICHT 360°.
       * 
       * Wir ein Punkt zur Umgrenzung hinzugefügt ist normalerweise die Angabe nötig, ob dieser Punkt näher am rechten oder am linken Rand der Umgrenzung liegt.
       * Z.B.: Links = 10°, Rechts = 50°, neuer Punkt 80°
       *       Die neue Umgrenzung könnte von 10° bis 80° mit einer Breite von 70° sein, aber auch von 80° bis 10° mit einer Breite von 290°.
       * Fehlt diese ausdrückliche Angabe, wird der Rand verändert, der näher zum neuen Punkt liegt.
       * Z.B.: Links = 10°, Rechts = 50°, neuer Punkt 80°
       *       Die neue Umgrenzung ist von 10° bis 80° mit einer Breite von 70°, weil 80° näher an 50° liegt (Abstand 30°) als an 10° (Abstand 290°).
       * Bei nicht zu großen Umgrenzungen sollte dieser Automatismus normalerweise sinnvoll sein.
       * 
       * Auch bei der Definition von Polygonen besteht diese Zweideutigkeit. In der Praxis wird dann üblicherweise auch davon ausgegangen, dass immer der kürzere Abstand 
       * zwischen 2 Punkten gemeint ist.
       * 
       * 
       * Hat der rechte Rand einen kleineren Zahlenwert als der linke Rand, wird für Lagevergleiche u.ä. intern zum rechten Rand 360 addiert.
       * Z.B: Links = 80°, Rechts = -60°
       *      90° liegt dazwischen, denn 80° <= 90° und 90° <= -60°+360°.
       *      Analog liegt 70° NICHT dazwischen.
       * 
       */

      Longitude _left, _right;
      Latitude _top, _bottom;


      /// <summary>
      /// linke Grenze (-<see cref="Coord.MAPUNITS180DEGREE"/>..<see cref="Coord.MAPUNITS180DEGREE"/>)
      /// </summary>
      public int Left {
         get {
            return _left;
         }
         set {
            _left = value;
         }
      }

      /// <summary>
      /// rechte Grenze (-<see cref="Coord.MAPUNITS180DEGREE"/>..<see cref="Coord.MAPUNITS180DEGREE"/>)
      /// </summary>
      public int Right {
         get {
            return _right;
         }
         set {
            _right = value;
         }
      }

      /// <summary>
      /// obere Grenze (-<see cref="Coord.MAPUNITS90DEGREE"/>..<see cref="Coord.MAPUNITS90DEGREE"/>)
      /// <para>Beim Zuweisen eines neuen Wertes wird notfalls durch Vertauschung immer gesichert dass <see cref="Top"/> nicht kleiner als <see cref="Bottom"/> ist!</para>
      /// </summary>
      public int Top {
         get {
            return _top;
         }
         set {
            _top = value;
            if (_top < _bottom)
               swapLatitude(_top, _bottom);
         }
      }

      /// <summary>
      /// untere Grenze (-<see cref="Coord.MAPUNITS90DEGREE"/>..<see cref="Coord.MAPUNITS90DEGREE"/>)
      /// <para>Beim Zuweisen eines neuen Wertes wird notfalls durch Vertauschung immer gesichert dass <see cref="Bottom"/> nicht größer als <see cref="Top"/> ist!</para>
      /// </summary>
      public int Bottom {
         get {
            return _bottom;
         }
         set {
            _bottom = value;
            if (_top < _bottom)
               swapLatitude(_top, _bottom);
         }
      }

      /// <summary>
      /// Breite des Bereiches
      /// </summary>
      public int Width {
         get {
            return width(Left, Right);
         }
      }

      /// <summary>
      /// Höhe des Bereiches
      /// </summary>
      public int Height {
         get {
            return Top - Bottom;
         }
      }

      /// <summary>
      /// liefert die Mitte in waagerechter Richtung
      /// </summary>
      public int CenterX {
         get {
            return Left + Width / 2;
         }
      }

      /// <summary>
      /// liefert die Mitte in senkrechter Richtung
      /// </summary>
      public int CenterY {
         get {
            return Bottom + Height / 2;
         }
      }

      /// <summary>
      /// liefert den Mittelpunkt des Rechtecks
      /// </summary>
      public MapUnitPoint Center {
         get {
            return new MapUnitPoint(Left + Width / 2, Bottom + Height / 2);
         }
      }


      /// <summary>
      /// Ist der Bereich leer, d.h. punktförmig?
      /// </summary>
      public bool IsPoint {
         get {
            return Width == 0 && Height == 0;
         }
      }


      /// <summary>
      /// linke Grenze (-180..180)
      /// </summary>
      public double LeftDegree {
         get {
            return _left.ValueDegree;
         }
         set {
            _left.ValueDegree = value;
         }
      }

      /// <summary>
      /// rechte Grenze (-180..180)
      /// </summary>
      public double RightDegree {
         get {
            return _right.ValueDegree;
         }
         set {
            _right.ValueDegree = value;
         }
      }

      /// <summary>
      /// untere Grenze (-90..90)
      /// </summary>
      public double BottomDegree {
         get {
            return _bottom.ValueDegree;
         }
         set {
            _bottom.ValueDegree = value;
         }
      }

      /// <summary>
      /// obere Grenze (-90..90)
      /// </summary>
      public double TopDegree {
         get {
            return _top.ValueDegree;
         }
         set {
            _top.ValueDegree = value;
         }
      }

      /// <summary>
      /// Breite des Bereiches
      /// </summary>
      public double WidthDegree {
         get {
            return Coord.MapUnits2Degree(Width);
         }
      }

      /// <summary>
      /// Höhe des Bereiches
      /// </summary>
      public double HeightDegree {
         get {
            return Coord.MapUnits2Degree(Height);
         }
      }

      /// <summary>
      /// liefert die Mitte in waagerechter Richtung
      /// </summary>
      public double CenterXDegree {
         get {
            return Coord.MapUnits2Degree(CenterX);
         }
      }

      /// <summary>
      /// liefert die Mitte in senkrechter Richtung
      /// </summary>
      public double CenterYDegree {
         get {
            return Coord.MapUnits2Degree(CenterY);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <param name="bottom"></param>
      /// <param name="top"></param>
      /// <param name="coordbits">wenn größer 0, wirden die Werte als RawUnit interpretiert und umgerechnet</param>
      public Bound(int left = 0, int right = 0, int bottom = 0, int top = 0, int coordbits = 0) {
         _left = new Longitude(left, coordbits);
         _right = new Longitude(right, coordbits);
         _bottom = new Latitude(bottom, coordbits);
         _top = new Latitude(top, coordbits);
      }

      public Bound(double left, double right, double bottom, double top) {
         this._left = new Longitude(left);
         this._right = new Longitude(right);
         this._top = new Latitude(top);
         this._bottom = new Latitude(bottom);
      }

      public Bound(MapUnitPoint pt) :
         this(pt.Longitude, pt.Longitude, pt.Latitude, pt.Latitude) { }

      public Bound(IList<MapUnitPoint> pt) :
         this() {
         if (pt.Count > 0) {
            int l = pt[0].Longitude;
            int r = l;
            int b = pt[0].Latitude;
            int t = b;
            for (int i = 1; i < pt.Count; i++) {
               combineLonRange(l, r, pt[i].Longitude, out l, out r);
               b = Math.Min(b, pt[i].Latitude);
               t = Math.Max(t, pt[i].Latitude);
            }
            Left = l;
            Right = r;
            Bottom = b;
            Top = t;
         }

#if DEBUG
         int sl = int.MaxValue;
         int sr = int.MinValue;
         int sb = int.MaxValue;
         int st = int.MinValue;
         for (int i = 0; i < pt.Count; i++) {
            sl = Math.Min(sl, pt[i].Longitude);
            sr = Math.Max(sr, pt[i].Longitude);
            sb = Math.Min(sb, pt[i].Latitude);
            st = Math.Max(st, pt[i].Latitude);
         }
         if ((sl >= 0 && sl != Left) ||
             (sr >= 0 && sr != Right) ||
             (sb > 0 && sb != Bottom) ||
             (st > 0 && st != Top))
            Debug.WriteLine(string.Format("Fehler in Bound() ? {0} <-> {1}", this, new Bound(sl, sr, sb, st)));
#endif
      }

      public Bound(Bound b) :
         this() {
         if (b != null) {
            _left = b._left;
            _right = b._right;
            _bottom = b._bottom;
            _top = b._top;
         }
      }

      public Bound(MapUnitPoint pt1, MapUnitPoint pt2) :
         this(pt1) {
         Embed(pt2);
      }

      public Bound(IList<int> lon, IList<int> lat) :
         this() {
         if (lon != null && lon.Count > 0 &&
             lat != null && lat.Count > 0) {
            int l = lon[0];
            int r = l;
            int b = lat[0];
            int t = b;
            for (int i = 1; i < lon.Count && i < lat.Count; i++) {
               combineLonRange(l, r, lon[i], out l, out r);
               b = Math.Min(b, lat[i]);
               t = Math.Max(t, lat[i]);
            }
            Left = l;
            Right = r;
            Bottom = b;
            Top = t;
         }
      }


      /// <summary>
      /// linke Grenze in RawUnits
      /// </summary>
      public int LeftRawUnits(int coordbits) {
         return _left.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// linke Grenze in RawUnits setzen
      /// </summary>
      public void LeftRawUnits(int rawunits, int coordbits) {
         _left.ValueRawUnits(rawunits, coordbits);
      }

      /// <summary>
      /// rechte Grenze in RawUnits
      /// </summary>
      public int RightRawUnits(int coordbits) {
         return _right.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// rechte Grenze in RawUnits setzen
      /// </summary>
      public void RightRawUnits(int rawunits, int coordbits) {
         _right.ValueRawUnits(rawunits, coordbits);
      }

      /// <summary>
      /// obere Grenze in RawUnits
      /// </summary>
      public int TopRawUnits(int coordbits) {
         return _top.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// obere Grenze in RawUnits setzen
      /// </summary>
      public void TopRawUnits(int rawunits, int coordbits) {
         _top.ValueRawUnits(rawunits, coordbits);
      }

      /// <summary>
      /// untere Grenze in RawUnits
      /// </summary>
      public int BottomRawUnits(int coordbits) {
         return _bottom.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// untere Grenze in RawUnits setzen
      /// </summary>
      public void BottomRawUnits(int rawunits, int coordbits) {
         _bottom.ValueRawUnits(rawunits, coordbits);
      }

      /// <summary>
      /// Breite des Bereiches in RawUnits
      /// </summary>
      public int WidthRawUnits(int coordbits) {
         return Coord.MapUnits2RawUnits(Width, coordbits);
      }

      /// <summary>
      /// Höhe des Bereiches in RawUnits
      /// </summary>
      public int HeightRawUnits(int coordbits) {
         return Coord.MapUnits2RawUnits(Height, coordbits);
      }

      /// <summary>
      /// liefert ein <see cref="Bound"/> das die RawUnit-Werte des akt. <see cref="Bound"/> entsprechend der Bitanzahl enthält
      /// </summary>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public Bound AsRawUnitBound(int coordbits) {
         return new Bound(_left.ValueRawUnits(coordbits), _right.ValueRawUnits(coordbits), _bottom.ValueRawUnits(coordbits), _top.ValueRawUnits(coordbits));
      }


      #region interne static-Funktionen

      /// <summary>
      /// Liegt die Länge zwischen <see cref="left"/> und <see cref="right"/> (bzw. auf dem Rand)?
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <returns></returns>
      static bool isIncluded(int lon, int left, int right) {
         if (left <= right)
            return left <= lon && lon <= right;
         return left <= lon && lon <= right + Coord.MAPUNITS360DEGREE;
      }

      /// <summary>
      /// Liegt die Länge zwischen <see cref="_left"/> und <see cref="_right"/> (NICHT auf dem Rand)?
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <returns></returns>
      static bool isInnner(int lon, int left, int right) {
         if (left <= right)
            return left < lon && lon < right;
         return left < lon && lon < right + Coord.MAPUNITS360DEGREE; ;
      }

      /// <summary>
      /// Breite 0..<see cref="Coord.MAPUNITS360DEGREE"/>
      /// </summary>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <returns></returns>
      static int width(int left, int right) {
         return left <= right ?
                     right - left :
                     right + 360 - left;
      }

      /// <summary>
      /// kürzeste (waagerechte) Entfernung (0..<see cref="Coord.MAPUNITS180DEGREE"/>)
      /// </summary>
      /// <param name="lon1"></param>
      /// <param name="lon2"></param>
      /// <returns></returns>
      static int shortLonDistance(int lon1, int lon2) {
         int dist = lonDiff(lon1, lon2);
         if (dist > Coord.MAPUNITS180DEGREE)
            return Coord.MAPUNITS360DEGREE - dist;
         return dist;
      }

      /// <summary>
      /// bildet die (immer nichtnegative) Differenz lon1 - lon2 und liefert einen Wert zwischen 0 und <see cref="Coord.MAPUNITS360DEGREE"/>
      /// </summary>
      /// <param name="lon1"></param>
      /// <param name="lon2"></param>
      /// <returns></returns>
      static int lonDiff(int lon1, int lon2) {
         if (lon1 < lon2)
            lon1 += Coord.MAPUNITS360DEGREE;
         return lon1 - lon2;
      }

      /// <summary>
      /// verbindet 2 Längenbereiche
      /// <para>Wenn die Längenbereiche sich nicht überschneiden, erfolgt die Verbindung für die kürzere Entfernung.</para>
      /// </summary>
      /// <param name="left1">linker Rand Bereich 1</param>
      /// <param name="right1">rechter Rand Bereich 1</param>
      /// <param name="left2">linker Rand Bereich 2</param>
      /// <param name="right2">rechter Rand Bereich 2</param>
      /// <param name="left">linker Rand Ergebnis</param>
      /// <param name="right">rechter Rand Ergebnis</param>
      static void combineLonRanges(int left1, int right1, int left2, int right2, out int left, out int right) {
         left1 = Coord.AdjustLongitudeMapUnits(left1);
         right1 = Coord.AdjustLongitudeMapUnits(right1);

         left = left1;
         right = right1;

         // "Normierung"
         int width1 = lonDiff(right1, left1);
         int width2 = lonDiff(right2, left2);
         int normleft2 = lonDiff(left2, left1);
         // --> Gebiet 1 beginnt bei 0 mit Breite width1, Gebiet 2 beginnt bei left2 mit Breite width2; alle Angaben im Wertebereich 0 .. Coord.MAPUNITS360DEGREE

         if (normleft2 <= width1) { // 2. Gebiet beginnt innerhalb des 1. Gebietes
            int normright = Math.Min(Math.Max(width1, normleft2 + width2), Coord.MAPUNITS360DEGREE); // max. Coord.MAPUNITS360DEGREE
            right = Coord.AdjustLongitudeMapUnits(normright + left1);
         } else {
            int gap21 = normleft2 - width1; // Lücke zwischen 1. und 2. Gebiet

            int normright = Math.Min(normleft2 + width2, Coord.MAPUNITS360DEGREE); // max. Coord.MAPUNITS360DEGREE
            int gap12 = Coord.MAPUNITS360DEGREE - normright; // Lücke zwischen 2. und 1. Gebiet

            if (gap21 <= gap12) { // dann wird die Lücke zwischen 1. und 2. Gebiet "entfernt"
               left = left1;
               right = gap12 > 0 ? right2 : left1;
            } else { // dann wird die Lücke zwischen 2. und 1. Gebiet ignoriert "entfernt"
               left = left2;
               right = right1;
            }
         }
      }

      /// <summary>
      /// verbindet einen Längenbereich mit einer Länge
      /// <para>Die Verbindung erfolgt für die kürzere Entfernung.</para>
      /// </summary>
      /// <param name="left1">linker Rand Bereich 1</param>
      /// <param name="right1">rechter Rand Bereich 1</param>
      /// <param name="lon">neue Länge</param>
      /// <param name="left">linker Rand Ergebnis</param>
      /// <param name="right">rechter Rand Ergebnis</param>
      static void combineLonRange(int left1, int right1, int lon, out int left, out int right) {
         left1 = Coord.AdjustLongitudeMapUnits(left1);
         right1 = Coord.AdjustLongitudeMapUnits(right1);

         left = left1;
         right = right1;

         if (right1 < left1)
            right1 += Coord.MAPUNITS360DEGREE;

         lon = Coord.AdjustLongitudeMapUnits(lon);
         if (left1 <= lon && lon <= right1)
            return;

         if (lonDiff(lon, right1) < lonDiff(left1, lon))
            right = lon;
         else
            left = lon;
      }

      #endregion

      /// <summary>
      /// näher am rechten als am linken Rand
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      bool isNextToRight(int lon) {
         if (Left == Right) {    // Spezialfall
            if (lon == Left)
               return false;
            return ((lon < Right ? lon + Coord.MAPUNITS360DEGREE : lon) - Right) < (Left - (lon > Left ? (lon - Coord.MAPUNITS360DEGREE) : lon));
         }
         return shortLonDistance(lon, Right) < shortLonDistance(lon, Left);
      }

      void swapLatitude(Latitude a, Latitude b) {
         Latitude tmp = a;
         a = b;
         b = tmp;
      }


      /// <summary>
      /// Ist der Punkt eingeschlossen (oder identisch)?
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public bool IsEnclosed(int lon, int lat) {
         lat = Coord.AdjustLatitudeMapUnits(lat);
         return !isIncluded(Coord.AdjustLongitudeMapUnits(lon), Left, Right) ||
                lat < Bottom ||
                Top < lat ? false : true;
      }

      /// <summary>
      /// Ist der Punkt eingeschlossen (oder identisch)?
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      public bool IsEnclosed(MapUnitPoint pt) {
         return IsEnclosed(pt.Longitude, pt.Latitude);
      }

      /// <summary>
      /// Ist die Umgrenzung eingeschlossen?
      /// </summary>
      /// <param name="b"></param>
      /// <returns></returns>
      public bool IsEnclosed(Bound b) {
         return b == null ?
                     false :
                     IsEnclosed(b.Left, b.Bottom) && IsEnclosed(b.Right, b.Top);
      }

      /// <summary>
      /// Ist der Punkt eingeschlossen (oder identisch)?
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public bool IsEnclosed(double lon, double lat) {
         return IsEnclosed(Coord.Degree2MapUnits(lon), Coord.Degree2MapUnits(lat));
      }

      /// <summary>
      /// erzeugt die Schnittmenge
      /// </summary>
      /// <param name="bound"></param>
      /// <returns></returns>
      public Bound? Intersection(Bound bound) {
         intersection(bound, true, out Bound? result);
         return result;
      }

      /// <summary>
      /// Überlappen sich die 2 Bereiche?
      /// </summary>
      /// <param name="bound"></param>
      /// <returns></returns>
      public bool IsOverlapped(Bound bound) {
         return intersection(bound, false, out _);
      }

      public bool IsOverlapped1(Bound bound) {
         return intersection1(bound, false, out _);
      }

      /// <summary>
      /// Ex. eine Schnittmenge?
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="getresult">wenn true wird, falls möglich, eine Schnittmenge gebildet</param>
      /// <param name="result">Schnittmenge oder</param>
      /// <returns></returns>
      bool intersection1(Bound bound, bool getresult, out Bound? result) {
         int l1 = Left;
         int r1 = Right;
         if (r1 < l1)
            r1 += Coord.MAPUNITS360DEGREE;
         int l2 = bound.Left;
         int r2 = bound.Right;
         if (r2 < l2)
            r2 += Coord.MAPUNITS360DEGREE;

         int l = -Coord.MAPUNITS360DEGREE;
         if (l1 <= l2 && l2 <= r1)
            l = l2;
         else if (l2 <= l1 && l1 <= r2)
            l = l1;

         if (l > -Coord.MAPUNITS360DEGREE) {
            int r = Math.Min(r1, r2);
            int b1 = Bottom;
            int t1 = Top;
            int b2 = bound.Bottom;
            int t2 = bound.Top;

            int b = -Coord.MAPUNITS360DEGREE;
            if (b1 <= b2 && b2 <= t1)
               b = b2;
            else if (b2 <= b1 && b1 <= t2)
               b = b1;

            if (b > -Coord.MAPUNITS360DEGREE) {
               result = getresult ? new Bound(l, r, b, Math.Min(t1, t2)) : null;
               return true;
            }
         }
         result = null;
         return false;
      }

      /// <summary>
      /// Ex. eine Schnittmenge?
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="getresult">wenn true wird, falls möglich, eine Schnittmenge gebildet</param>
      /// <param name="result">Schnittmenge oder</param>
      /// <returns></returns>
      bool intersection(Bound bound, bool getresult, out Bound? result) {
         int l1 = Left;
         int r1 = Right;
         if (r1 < l1)
            r1 += Coord.MAPUNITS360DEGREE;
         int l2 = bound.Left;
         int r2 = bound.Right;
         if (r2 < l2)
            r2 += Coord.MAPUNITS360DEGREE;

         if (overlapped(l1, r1, l2, r2, out int left, out int right)) {
            if (overlapped(Bottom, Top, bound.Bottom, bound.Top, out int bottom, out int top)) {
               result = getresult ? new Bound(left, right, bottom, top) : null;
               return true;
            }
         }

         result = null;
         return false;
      }

      bool overlapped(int start1, int end1,
                      int start2, int end2,
                      out int startoverlapped, out int endoverlapped) {
         if (start2 < start1) {  // Bereiche austauschen (1 immer VOR 2)
            int tmp = start1;
            //start1 = start2;
            start2 = tmp;
            tmp = end1;
            end1 = end2;
            end2 = tmp;
         }

         if (start2 <= end1) {
            startoverlapped = start2;
            endoverlapped = end2 <= end1 ? end2 : end1;
            return true;
         } else {
            startoverlapped = endoverlapped = 0;
            return false;
         }
      }


      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void Embed(int lon, int lat) {
         int lonmu = Coord.AdjustLongitudeMapUnits(lon);
         Embed(lonmu, lat, isNextToRight(lonmu));
      }

      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="onrightside">true, wenn Punkt näher am rechten als am linken Rand liegt</param>
      public void Embed(MapUnitPoint pt) {
         Embed(pt.Longitude, pt.Latitude, isNextToRight(pt.Longitude));
      }

      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="onrightside">true, wenn Punkt näher am rechten als am linken Rand liegt</param>
      public void Embed(int lon, int lat, bool onrightside) {
         lat = Coord.AdjustLatitudeMapUnits(lat);
         lon = Coord.AdjustLongitudeMapUnits(lon);

         if (!isIncluded(lon, Left, Right)) {
            if (onrightside)
               Right = lon;
            else
               Left = lon;
         }
         if (Top < lat)
            Top = lat;
         else if (lat < Bottom)
            Bottom = lat;
      }

      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="onrightside">true, wenn Punkt näher am rechten als am linken Rand liegt</param>
      public void Embed(MapUnitPoint pt, bool onrightside) {
         Embed(pt.Longitude, pt.Latitude, onrightside);
      }


      /// <summary>
      /// bildet die kleinste (!) umschließende Umgrenzung aus der bestehenden Umgrenzung und der zusätzlichen Umgrenzung
      /// </summary>
      /// <param name="left"></param>
      /// <param name="right"></param>
      /// <param name="bottom"></param>
      /// <param name="top"></param>
      public void Embed(int left, int right, int bottom, int top) {
         int l, r;
         combineLonRanges(Left, Right, Coord.AdjustLongitudeMapUnits(left), Coord.AdjustLongitudeMapUnits(right), out l, out r);
         Left = l;
         Right = r;
         Bottom = Math.Min(Bottom, Coord.AdjustLatitudeMapUnits(bottom));
         Top = Math.Max(Top, Coord.AdjustLatitudeMapUnits(top));
      }

      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und der Umgrenzung
      /// </summary>
      /// <param name="b"></param>
      public void Embed(Bound b) {
         if (b != null)
            Embed(b.Left, b.Right, b.Bottom, b.Top);
      }


      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// <para>Die Erweiterung erfolgt nach links oder rechts, je nachdem, ob der Punkt näher am linken oder am rechten Rand liegt.</para>
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void Embed(double lon, double lat) {
         Embed(Coord.Degree2MapUnits(lon), Coord.Degree2MapUnits(lat));
      }

      /// <summary>
      /// bildet die kleinste umschließende Umgrenzung aus der bestehenden Umgrenzung und dem Punkt
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="onrightside">true, wenn Punkt näher am rechten als am linken Rand liegt</param>
      public void Embed(double lon, double lat, bool onrightside) {
         Embed(Coord.Degree2MapUnits(lon),
               Coord.Degree2MapUnits(lat),
               onrightside);
      }


      public override string ToString() {
         return string.Format("Left {0}° .. Rigth {1}°, Bottom {2}° .. Top {3}°, IsPoint {4} (MU {5} .. {6}, {7} .. {8})",
                              LeftDegree, RightDegree, BottomDegree, TopDegree,
                              IsPoint,
                              Left, Right, Bottom, Top);
      }

   }
}
