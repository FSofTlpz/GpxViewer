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

   /// <summary>
   /// Hilfsfunktionen um Bits zu setzen oder zu testen
   /// </summary>
   public class Bit {

      /// <summary>
      /// setzt ein Bit auf 0 oder 1 und liefert den neuen Wert
      /// </summary>
      /// <param name="v"></param>
      /// <param name="bit">0..31</param>
      /// <param name="set"></param>
      /// <returns></returns>
      static public uint Set(uint v, byte bit, bool set = true) {
         if (bit >= 32)
            throw new System.ArgumentException("Nur Bit 0 bis 31 erlaubt.", "bit");
         if (set)
            v |= (uint)(0x01 << bit);
         else
            v &= (uint)(~(0x01 << bit));
         return v;
      }

      /// <summary>
      /// setzt ein Bit auf 0 oder 1
      /// </summary>
      /// <param name="v"></param>
      /// <param name="bit">0..31</param>
      /// <param name="set"></param>
      static public void Set(ref uint v, byte bit, bool set = true) {
         v = Set(v, bit, set);
      }

      /// <summary>
      /// testet, ob das Bit gesetzt ist
      /// </summary>
      /// <param name="v"></param>
      /// <param name="bit"></param>
      /// <returns></returns>
      static public bool IsSet(uint v, byte bit) {
         if (bit >= 32)
            throw new System.ArgumentException("Nur Bit 0 bis 31 erlaubt.", "bit");
         return (v & (0x01 << bit)) != 0;
      }

      /// <summary>
      /// liefert <see cref="count"/> Bits ab Bit <see cref="from"/> (als Zahl)
      /// </summary>
      /// <param name="v"></param>
      /// <param name="from">0..31</param>
      /// <param name="count">Bitanzahl</param>
      /// <returns></returns>
      static public uint GetUInt(uint v, byte from, byte count) {
         if (from >= 32)
            throw new System.ArgumentException("Nur Bit 0 bis 31 erlaubt.", "from");
         if (count == 0 || count > 32)
            throw new System.ArgumentException("Nur 1 bis 32 erlaubt.", "count");
         if (from + count > 32)
            throw new System.ArgumentException("Bitanzahl ist zu groß.", "count");

         v >>= from;

         uint mask = 0x01;
         while (--count > 0)
            mask = 0x01 | (mask << 1);

         return v & mask;
      }

      /// <summary>
      /// setzt den Bitbereich mit der Zahl
      /// </summary>
      /// <param name="v"></param>
      /// <param name="val">neue Bits</param>
      /// <param name="from">0..31</param>
      /// <param name="count">Bitanzahl</param>
      /// <returns></returns>
      static public uint SetUInt(uint v, uint val, byte from, byte count) {
         if (from >= 32)
            throw new System.ArgumentException("Nur Bit 0 bis 31 erlaubt.", "from");
         if (count == 0 || count > 32)
            throw new System.ArgumentException("Nur 1 bis 32 erlaubt.", "count");
         if (from + count > 32)
            throw new System.ArgumentException("Bitanzahl ist zu groß.", "count");

         uint mask = 0x01;
         while (--count > 0)
            mask = 0x01 | (mask << 1);
         val &= mask; // auf gültigen Bereich eingeschränkt

         val <<= from;
         mask <<= from;
         return (v | mask) & val;
      }
   }
}
