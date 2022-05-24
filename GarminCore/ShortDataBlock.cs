/*
Copyright (C) 2015 Frank Stinner

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
   /// beschreibt einen Dateibereich mit Offset und kurzer Länge (4 + 2 Byte)
   /// </summary>
   public class ShortDataBlock {
      /// <summary>
      /// Offset auf die Daten
      /// </summary>
      public UInt32 Offset { get; set; }
      /// <summary>
      /// Länge der Daten
      /// </summary>
      public UInt16 Length { get; set; }

      public ShortDataBlock() {
         Offset = Length = 0;
      }

      public ShortDataBlock(BinaryReaderWriter br)
         : this() {
         Read(br);
      }

      public ShortDataBlock(ShortDataBlock bl) {
         Offset = bl.Offset;
         Length = bl.Length;
      }

      public ShortDataBlock(UInt32 offset, UInt16 length) {
         Offset = offset;
         Length = length;
      }

      /// <summary>
      /// liest die Blockdaten
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         Offset = br.Read4UInt();
         Length = br.Read2AsUShort();
      }
      /// <summary>
      /// schreibt die Blockdaten
      /// </summary>
      /// <param name="bw"></param>
      public void Write(BinaryReaderWriter bw) {
         bw.Write(Offset);
         bw.Write(Length);
      }

      public override string ToString() {
         return string.Format("Block=[Offset 0x{0:x}, Länge 0x{1:x}]", Offset, Length);
      }
   }
}
