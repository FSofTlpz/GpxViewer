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
   /// Punkt auf Basis der MapUnits (int-Werte)
   /// </summary>
   public class MapUnitPoint {

      Longitude _lon;
      Latitude _lat;

      /// <summary>
      /// Länge in MapUnits (-0x800000 .. 0x800000 <-> -180°..180°)
      /// </summary>
      public int Longitude {
         get {
            return _lon;
         }
         set {
            _lon = value;
         }
      }

      /// <summary>
      /// Breite in MapUnits (-0x400000 .. 0x400000 <-> -90°..90°)
      /// </summary>
      public int Latitude {
         get {
            return _lat;
         }
         set {
            _lat = value;
         }
      }

      /// <summary>
      /// Länge in Grad (intern umgerechnet aus/in <see cref="Longitude"/>)
      /// </summary>
      public double LongitudeDegree {
         get {
            return Coord.MapUnits2Degree(Longitude);
         }
         set {
            Longitude = Coord.Degree2MapUnits(value);
         }
      }

      /// <summary>
      /// Breite in Grad (intern umgerechnet aus/in <see cref="Longitude"/>)
      /// </summary>
      public double LatitudeDegree {
         get {
            return Coord.MapUnits2Degree(Latitude);
         }
         set {
            Latitude = Coord.Degree2MapUnits(value);
         }
      }

      /// <summary>
      /// identisch zur Länge in MapUnits
      /// </summary>
      public int X {
         get {
            return _lon;
         }
         set {
            _lon = value;
         }
      }

      /// <summary>
      /// identisch zur Breite in MapUnits
      /// </summary>
      public int Y {
         get {
            return _lat;
         }
         set {
            _lat = value;
         }
      }


      /// <summary>
      /// erzeugt einen Punkt aus MapUnits
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public MapUnitPoint(int lon = 0, int lat = 0) {
         _lon = new Longitude(lon);
         _lat = new Latitude(lat);
      }

      /// <summary>
      /// erzeugt einen Punkt aus Grad
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public MapUnitPoint(double lon, double lat) {
         _lon = new Longitude(lon);
         _lat = new Latitude(lat);
      }

      /// <summary>
      /// erzeugt eine Kopie des Punktes
      /// </summary>
      /// <param name="pt"></param>
      public MapUnitPoint(MapUnitPoint pt) :
         this(pt._lon, pt._lat) { }

      /// <summary>
      /// erzeugt einen Punkt aus RawUnits
      /// </summary>
      /// <param name="rawlon"></param>
      /// <param name="rawlat"></param>
      /// <param name="coordbits"></param>
      public MapUnitPoint(int rawlon, int rawlat, int coordbits) {
         _lon = new Longitude(rawlon, coordbits);
         _lat = new Latitude(rawlat, coordbits);
      }


      /// <summary>
      /// Länge in RawUnits
      /// </summary>
      public int LongitudeRawUnits(int coordbits) {
         return _lon.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// setzt die Länge in RawUnits
      /// </summary>
      public void LongitudeRawUnits(int rawunits, int coordbits) {
         _lon.ValueRawUnits(rawunits, coordbits);
      }

      /// <summary>
      /// Breite in RawUnits
      /// </summary>
      public int LatitudeRawUnits(int coordbits) {
         return _lat.ValueRawUnits(coordbits);
      }

      /// <summary>
      /// setzt die Breite in RawUnits
      /// </summary>
      public void LatitudeRawUnits(int rawunits, int coordbits) {
         _lat.ValueRawUnits(rawunits, coordbits);
      }


      public bool Equals(MapUnitPoint? p) {
         if ((object?)p == null) // NICHT "p == null" usw. --> führt zur Endlosschleife
            return false;
         return Latitude == p.Latitude &&
                Longitude == p.Longitude;
      }

      public override bool Equals(object? obj) {
         if (obj == null)
            return false;

         MapUnitPoint? p = (MapUnitPoint?)obj;
         if (p == null)
            return false;

         return Latitude == p.Latitude &&
                Longitude == p.Longitude;
      }

      public override int GetHashCode() {
         return Latitude ^ Longitude;
      }

      public static bool operator ==(MapUnitPoint? p1, MapUnitPoint? p2) {
         if (ReferenceEquals(p1, p2))
            return true;

         return (object?)p1 != null && // NICHT "a == null" usw. --> Endlosschleife
                 p1.Equals(p2);
      }

      public static bool operator !=(MapUnitPoint x, MapUnitPoint y) {
         return !(x == y);
      }

      public static MapUnitPoint operator +(MapUnitPoint pt1, MapUnitPoint pt2) {
         return new MapUnitPoint(pt1.Longitude + pt2.Longitude, pt1.Latitude + pt2.Latitude);
      }

      public static MapUnitPoint operator -(MapUnitPoint pt1, MapUnitPoint pt2) {
         return new MapUnitPoint(pt1.Longitude - pt2.Longitude, pt1.Latitude - pt2.Latitude);
      }

      public static MapUnitPoint operator /(MapUnitPoint pt, double f) {
         return new MapUnitPoint((int)(pt.Longitude / f + .5), (int)(pt.Latitude / f + .5));
      }

      public static MapUnitPoint operator *(MapUnitPoint pt, double f) {
         return new MapUnitPoint((int)(pt.Longitude * f + .5), (int)(pt.Latitude * f + .5));
      }


      public void Add(int lonraw, int latraw, int coordbits) {
         _lon.Add(lonraw, coordbits);
         _lat.Add(latraw, coordbits);
      }

      public void Add(int lon, int lat) {
         _lon.Add(lon);
         _lat.Add(lat);
      }

      public void Add(double lon, double lat) {
         _lon.Add(lon);
         _lat.Add(lat);
      }

      public void Add(MapUnitPoint pt) {
         _lon.Add(pt.Longitude);
         _lat.Add(pt.Latitude);
      }


      public void Sub(int lonraw, int latraw, int coordbits) {
         _lon.Sub(lonraw, coordbits);
         _lat.Sub(latraw, coordbits);
      }

      public void Sub(int lon, int lat) {
         _lon.Sub(lon);
         _lat.Sub(lat);
      }

      public void Sub(double lon, double lat) {
         _lon.Sub(lon);
         _lat.Sub(lat);
      }

      public void Sub(MapUnitPoint pt) {
         _lon.Sub(pt.Longitude);
         _lat.Sub(pt.Latitude);
      }


      /// <summary>
      /// rundet die Koordinaten binär, d.h. die niederwertigsten Bits werden auf 0 gesetzt
      /// </summary>
      /// <param name="bits"></param>
      public void RoundBinary(int bits) {
         _lat.RoundBinary(bits);
         _lon.RoundBinary(bits);
      }


      public override string ToString() {
         return string.Format("Lon {0}° Lat {1}° (MU {2} {3})", LongitudeDegree, LatitudeDegree, Longitude, Latitude);
      }

   }
}
