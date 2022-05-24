using System;
/*
Copyright (C) 2011 Frank Stinner

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
using System.Collections.Generic;
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// Hilfsklasse um die Zeichenreihenfolge zu behandeln
   /// </summary>
   public class PolygonDraworderTableItem {

      /// <summary>
      /// Polygontyp (wenn größer als 0xFF, dann mit Subtypes)
      /// </summary>
      public uint Type { get; private set; }

      /// <summary>
      /// Subtypen (1 oder mehrere)
      /// </summary>
      public List<uint> Subtypes { get; private set; }

      /// <summary>
      /// Draworder
      /// </summary>
      public uint Level { get; private set; }

      /// <summary>
      /// erzeugt einen Eintrag für eine Drawordertabelle (noch ohne Subtypes!)
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="level"></param>
      public PolygonDraworderTableItem(uint typ = 0, uint level = 0) {
         Subtypes = new List<uint>();
         Type = typ;
         Level = level;
      }

      /// <summary>
      /// liest einen Draworder-Eintrag ein (Typ 0 steht für das Ende eines Levels)
      /// </summary>
      /// <param name="br"></param>
      /// <param name="length"></param>
      /// <param name="level"></param>
      public PolygonDraworderTableItem(BinaryReaderWriter br, int length, uint level)
         : this() {
         if (length > 9)
            throw new Exception("Ein PolygonDraworderTableItem darf max. 9 Byte lang sein.");
         Level = level;
         Type = br.ReadByte();
         length--;

         // insgesamt 32 Bit für 31 Sublevel
         // 1. Byte: Bit 0 für Sublevel 0
         //          Bit 1 für Sublevel 1
         //          ...
         //          Bit 7 für Sublevel 7
         // 2. Byte: Bit 0 für Sublevel 8
         //          Bit 1 für Sublevel 9
         //          ...
         //          Bit 7 für Sublevel F
         // 3. Byte: Bit 0 für Sublevel 10
         //          Bit 1 für Sublevel 11
         //          ...
         //          Bit 7 für Sublevel 17
         // 4. Byte: Bit 0 für Sublevel 18
         //          Bit 1 für Sublevel 19
         //          ...
         //          Bit 7 für Sublevel 1F         
         for (uint b = 0; b < length; b++) {          // alle (4) Bytes für Subtypes 0x00 ... 0x1F einlesen
            byte bv = br.ReadByte();
            byte mask = 0x01;
            for (uint bit = 0; bit < 8; bit++) {
               if ((bv & mask) != 0x0)
                  SetSubtype(bit + 8 * b);
               mask <<= 1;
            }
         }

         if (Subtypes.Count == 0)
            Subtypes.Add(0);
         else
            Type += 0x100;
      }

      /// <summary>
      /// fügt einen zusätzlichen Subtyp hinzu
      /// </summary>
      /// <param name="subtyp"></param>
      public void SetSubtype(uint subtyp) {
         if (!Subtypes.Contains(subtyp))
            Subtypes.Add(subtyp);
      }

      /// <summary>
      /// schreibt den Eintrag für eine Drawordertabelle (Typ 0 steht für das Ende eines Levels)
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="length"></param>
      public void Write(BinaryReaderWriter bw, int length) {
         bw.Write((byte)(Type & 0xff));
         length--;

         byte[] b = new byte[length];
         if (0xFF < Type)                  // dann auch Subtypes berücksichtigen
            for (int i = 0; i < Subtypes.Count; i++) {
               uint bidx = Subtypes[i] / 8;        // Byte-Index (0..)
               b[bidx] |= (byte)(0x01 << (int)(Subtypes[i] % 8));
            }

         bw.Write(b);
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("Draworder=[");
         sb.Append("Level " + Level.ToString());
         sb.Append(", Typ 0x" + Type.ToString("x"));
         if (Subtypes.Count > 0)
            sb.Append(", Subtyp/s");
         for (int i = 0; i < Subtypes.Count; i++)
            sb.Append(" 0x" + Subtypes[i].ToString("x"));
         sb.Append("]");
         return sb.ToString();
      }

   }

}
