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

namespace GarminCore {
   /// <summary>
   /// Basisfunktionalität zum Umgang mit Koordinaten
   /// <para>Intern erfolgt die Speicherung in MapUnits (Garmin-Umrechnung von Dezimalgrad in einen int-Wert)</para>
   /// <para>Bei der Umrechnung in RawUnits (in Dateien gespeicherte Werte) ist jeweils die Angabe der Bitanzahl (max. 24) für die Speicherung nötig</para>
   /// <para><see cref="Coord"/> erleichert die Umrechnung von Grad, MapUnits und RawUnits ineinander und grenzt den Wertebereich ein</para>
   /// </summary>
   public class Coord {

      // 360.0 / (1 << 24)
      // 360.0 / 0x1000000

      /// <summary>
      /// MapUnits für 360°
      /// </summary>
      public const int MAPUNITS360DEGREE = 0x1000000; // 1 << 24

      /// <summary>
      /// MapUnits für 180°
      /// </summary>
      public const int MAPUNITS180DEGREE = MAPUNITS360DEGREE >> 1;

      /// <summary>
      /// MapUnits für 90°
      /// </summary>
      const int MAPUNITS90DEGREE = MAPUNITS360DEGREE >> 2;

      /// <summary>
      /// MapUnits je Grad
      /// </summary>
      const double DEGREE_FACTOR = 360.0 / MAPUNITS360DEGREE;

      /*
       * 1 << (24 - coordbits)        
       * ---------------------- * 360.0
       *        1 << 24                
       *        
       *  1 RawUnit steht für:
       *  
       *  coordbits     Grad                       m am Äquator (Umfang 40075,017km)
       *  24            0,000021457672119140625       2,388657152652740478515625
       *  23            0,00004291534423828125        4,77731430530548095703125
       *  22            0,0000858306884765625         9,5546286106109619140625
       *  21            0,000171661376953125         19,109257221221923828125
       *  20            0,00034332275390625          38,21851444244384765625
       *  19            0,0006866455078125           76,4370288848876953125
       *  18            0,001373291015625           152,874057769775390625
       *  17            0,00274658203125            305,74811553955078125
       *  16            0,0054931640625             611,4962310791015625
       *  15            0,010986328125             1222,992462158203125
       *  14            0,02197265625              2445,98492431640625
       *  13            0,0439453125               4891,9698486328125
       *  12            0,087890625                9783,939697265625
       *  11            0,17578125                19567,87939453125
       *  10            0,3515625                 39135,7587890625
       *  
       *  Bei 24 Bit können Punkte also nur in einem Raster von etwa 239 cm dargestellt werden.
       */

      #region static-Funktionen für Umrechnungen und Einschränkungen von Koordinaten

      /// <summary>
      /// wandelt die Rohdaten (z.B. von GetBitstreamAsRawUnits()) in Abhängigkeit vom Maplevel in echte MapUnits um
      /// </summary>
      /// <param name="ru"></param>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public static int RawUnits2MapUnits(int ru, int coordbits) {
         return ru << (24 - coordbits);
      }

      /// <summary>
      /// wandelt die Rohdaten (z.B. von GetBitstreamAsRawUnits()) in Abhängigkeit vom Maplevel in Grad um
      /// </summary>
      /// <param name="ru"></param>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public static double RawUnits2Degree(int ru, int coordbits) {
         return MapUnits2Degree(RawUnits2MapUnits(ru, coordbits));
      }

      /// <summary>
      /// Übernahme aus MKGMAP: A map unit is an integer value that is 1/(2^24) degrees of latitude or longitude. 
      /// </summary>
      /// <param name="degree">Lat oder Lon in Grad</param>
      /// <returns></returns>
      public static int Degree2MapUnits(double degree) {
         return (int)(degree / DEGREE_FACTOR + (degree > 0 ? .5 : -.5)); // mit Pseudorundung
      }

      /// <summary>
      /// Übernahme aus MKGMAP: Convert an angle in map units to degrees.
      /// </summary>
      /// <param name="mu"></param>
      /// <returns></returns>
      public static double MapUnits2Degree(int mu) {
         return mu * DEGREE_FACTOR;
      }

      /// <summary>
      /// wandelt die MapUnits in Abhängigkeit vom Maplevel in Rohdaten um (mit Rundung)
      /// </summary>
      /// <param name="mu"></param>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public static int MapUnits2RawUnits(int mu, int coordbits) {
         return RoundBinary(mu, 24 - coordbits) >> (24 - coordbits); // mit Rundung
         //return mu >> (24 - coordbits); // Rundung ???
      }

      /// <summary>
      /// wandelt die Grad in Abhängigkeit vom Maplevel in Rohdaten um (mit Rundung)
      /// </summary>
      /// <param name="mu"></param>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public static int Degree2RawUnits(double degree, int coordbits) {
         return MapUnits2RawUnits(Degree2MapUnits(degree), coordbits);
      }

      /// <summary>
      /// liefert einen Wert in Mapunits für die Länge, der immer dem Bereich -180°..180° entspricht
      /// </summary>
      /// <param name="lon"></param>
      /// <returns></returns>
      public static int AdjustLongitudeMapUnits(int lon) {
         if (-MAPUNITS180DEGREE < lon && lon <= MAPUNITS180DEGREE) // 0x800000 <-> 180°
            return lon;

         lon += MAPUNITS180DEGREE;
         while (lon > MAPUNITS360DEGREE)
            lon -= MAPUNITS360DEGREE;
         while (lon < 0)
            lon += MAPUNITS360DEGREE;
         return lon - MAPUNITS180DEGREE;
      }

      /// <summary>
      /// liefert einen Wert in Mapunits für die Breite, der immer dem Bereich -90°..90° entspricht
      /// </summary>
      /// <param name="lon"></param>
      /// <returns></returns>
      public static int AdjustLatitudeMapUnits(int lon) {
         return Math.Max(-MAPUNITS90DEGREE, Math.Min(MAPUNITS90DEGREE, lon)); // 0x400000 <-> 90°
      }

      /// <summary>
      /// grenzt die Länge auf -180°..180° ein; zu große oder zu kleine Werte werden auf diesen Bereich umgerechnet
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      public static double AdjustLongitudeDegree(double v) {
         if (-180 < v && v <= 180)
            return v;
         v += 180;
         while (v > 360)
            v -= 360;
         while (v < 0)
            v += 360;
         return v - 180;
      }

      /// <summary>
      /// grenzt die Breite auf -90°..90° ein; zu große oder zu kleine Werte werden auf die jeweiligen Grenzwerte gesetzt
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      public static double AdjustLatitudeDegree(double v) {
         return Math.Max(-90, Math.Min(90, v));
      }


      /// <summary>
      /// Rundet binär, d.h. die niederwertigsten Bits werden auf 0 gesetzt
      /// <para>Beim Runden gehen die niederwertigsten Bits nicht einfach verloren, sondern es wird bei Bedarf auch auf den nächsthöheren Wert gerunden</para>
      /// </summary>
      /// <param name="val"></param>
      /// <param name="bits">Anzahl der auf 0 zu setztenden Bits</param>
      /// <returns></returns>
      public static int RoundBinary(int val, int bits) {
         if (bits <= 0)
            return val;
         int mask = ~((1 << bits) - 1); // zum 0-setzen der niedrigen Bits
         int half = 1 << (bits - 1); // die Hälfte des Betrages der niedrigen Bits
         /* z.B. bits = 3:
          * mask = ~((1 << bits) - 1) = ~((1 << 3) - 1)
          *      = ~7
          *      = bin1..1000
          * half = 1 << (bits - 1)
          *      = 4
          *      = bin100
          */
         return (val + half) & mask;
      }

      #endregion

      int _val;

      /// <summary>
      /// Koordinate in MapUnits
      /// </summary>
      public virtual int Value {
         get {
            return _val;
         }
         set {
            _val = value;
         }
      }

      /// <summary>
      /// Koordinate in Grad (intern umgerechnet aus/in <see cref="Value"/>)
      /// </summary>
      public double ValueDegree {
         get {
            return MapUnits2Degree(Value);
         }
         set {
            Value = Degree2MapUnits(value);
         }
      }


      /// <summary>
      /// Koordinate aus MapUnits
      /// <para>Länge: -<see cref="MAPUNITS180DEGREE"/> .. <see cref="MAPUNITS180DEGREE"/></para>
      /// <para>Breite: -<see cref="MAPUNITS90DEGREE"/> .. <see cref="MAPUNITS90DEGREE"/></para>
      /// </summary>
      /// <param name="val"></param>
      /// <param name="coordbits">wenn größer 0, wird der Wert als RawUnit interpretiert und umgerechnet</param>
      public Coord(int val = 0, int coordbits = 0) {
         Value = coordbits < 1 ? val : RawUnits2MapUnits(val, coordbits);
      }

      /// <summary>
      /// Koordinate aus Grad
      /// </summary>
      /// <param name="val"></param>
      public Coord(double val) {
         ValueDegree = val;
      }

      /// <summary>
      /// Kopie einer Koordinate
      /// </summary>
      /// <param name="val"></param>
      public Coord(Coord val) {
         Value = val.Value;
      }


      /// <summary>
      /// für die Zuweisung Coord=int
      /// </summary>
      /// <param name="value"></param>
      public static implicit operator Coord(int value) {
         return new Coord(value);
      }

      /// <summary>
      /// für die Zuweisung int=Coord
      /// </summary>
      /// <param name="value"></param>
      public static implicit operator int(Coord value) {
         return value._val;
      }

      public static Coord operator +(Coord c1, Coord c2) {
         return new Coord(c1.Value + c2.Value);
      }

      public static Coord operator -(Coord c1, Coord c2) {
         return new Coord(c1.Value - c2.Value);
      }

      public static Coord operator /(Coord c1, double f) {
         return new Coord((int)((c1.Value + .5) / f));
      }

      public static Coord operator *(Coord c1, double f) {
         return new Coord((int)((c1.Value + .5) * f));
      }


      /// <summary>
      /// liefert die Koordinate in RawUnits (mit Rundung)
      /// </summary>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public int ValueRawUnits(int coordbits) {
         return MapUnits2RawUnits(Value, coordbits);
      }

      /// <summary>
      /// setzt die Koordinate aus RawUnits
      /// </summary>
      /// <param name="rawunits"></param>
      /// <param name="coordbits"></param>
      public void ValueRawUnits(int rawunits, int coordbits) {
         _val = RawUnits2MapUnits(rawunits, coordbits);
      }


      public void Add(int valraw, int coordbits) {
         Value += RawUnits2MapUnits(valraw, coordbits);
      }

      public void Add(int val) {
         Value += val;
      }

      public void Add(double val) {
         ValueDegree += val;
      }


      public void Sub(int valraw, int coordbits) {
         Value += RawUnits2MapUnits(valraw, coordbits);
      }

      public void Sub(int val) {
         Value += val;
      }

      public void Sub(double val) {
         ValueDegree += val;
      }


      /// <summary>
      /// rundet die Koordinate binär, d.h. die niederwertigsten Bits werden auf 0 gesetzt
      /// </summary>
      /// <param name="bits"></param>
      public void RoundBinary(int bits) {
         _val = RoundBinary(_val, bits);
      }


      public override string ToString() {
         return string.Format("{0} / {1}°", Value, ValueDegree);
      }

   }
}
