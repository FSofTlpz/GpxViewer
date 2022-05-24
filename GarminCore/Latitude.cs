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

namespace GarminCore {
   public class Latitude : Coord {

      /// <summary>
      /// Breite in MapUnits (-0x400000 .. 0x400000 <-> -90°..90°)
      /// </summary>
      public override int Value {
         get {
            return base.Value;
         }
         set {
            base.Value = AdjustLatitudeMapUnits(value);
         }
      }

 
      public Latitude(int lat = 0) :
         base(lat) { }

      public Latitude(double lat) :
         base(lat) {
      }

      public Latitude(Latitude lat) :
         base(lat.Value) {
      }

      public Latitude(int rawunits, int coordbits) :
         base(rawunits, coordbits) {
      }

      /// <summary>
      /// für die Zuweisung Latitude=int
      /// </summary>
      /// <param name="value"></param>
      public static implicit operator Latitude(int value) {
         return new Latitude(value);
      }

   }
}
