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
using System;
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// Hilfsklasse um den Typ/Subtyp sowie den Offset zu den eigentlichen Daten in der Datei zu behandeln
   /// </summary>
   public class TableItem {

      public UInt16 rawtype { get; private set; }

      /// <summary>
      /// Typ (Bit 5...15, d.h. 0 .. 0x7ff)
      /// </summary>
      public uint Type {
         get { return (uint)(rawtype >> 5); }
         set { rawtype = (UInt16)(Subtype + ((value & 0x7ff) << 5)); }
      }

      /// <summary>
      /// Subtyp (Bit 0...4, d.h. 0 .. 0x1f)
      /// </summary>
      public uint Subtype {
         get { return (uint)(rawtype & 0x1f); }
         set { rawtype = (UInt16)((rawtype & 0xffe0) + (value & 0x1f)); }
      }

      /// <summary>
      /// Offset zum Anfang des jeweiligen Blocks
      /// </summary>
      public int Offset { get; set; }

      public TableItem() {
         Type = Subtype = 0;
         Offset = 0;
      }

      public TableItem(BinaryReaderWriter br, int iItemlength)
         : this() {
         rawtype = br.Read2AsUShort();
         switch (iItemlength) {
            case 3: Offset = br.ReadByte(); break;
            case 4: Offset = br.Read2AsUShort(); break;
            case 5: Offset = (int)br.Read3AsUInt(); break;
         }
      }

      public void Write(BinaryReaderWriter bw, int iItemlength) {
         UInt16 type = (UInt16)((Type << 5) | Subtype);
         bw.Write(type);
         switch (iItemlength) {
            case 3: bw.Write((byte)(Offset & 0xff)); break;
            case 4: bw.Write((UInt16)(Offset & 0xffff)); break;
            case 5:
               bw.Write((UInt16)(Offset & 0xffff));
               bw.Write((byte)((Offset >> 16) & 0xff));
               break;
         }
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("DataItem=[");
         sb.Append("Typ=0x" + Type.ToString("x"));
         sb.Append(" Subtyp=0x" + Subtype.ToString("x"));
         sb.Append(" Offset=0x" + Offset.ToString("x"));
         sb.Append("]");
         return sb.ToString();
      }

   }

}
