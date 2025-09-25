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
   /// beschreibt einen Dateibereich mit Offset und Länge (4 + 4 Byte)
   /// </summary>
   public class DataBlock {
      /// <summary>
      /// Offset auf die Daten
      /// </summary>
      public UInt32 Offset { get; set; }
      /// <summary>
      /// Länge der Daten
      /// </summary>
      public UInt32 Length { get; set; }

      public DataBlock() {
         Offset = Length = 0;
      }

      public DataBlock(BinaryReaderWriter br)
         : this() {
         Read(br);
      }

      public DataBlock(DataBlock? bl)
         : this() {
         if (bl != null) {
            Offset = bl.Offset;
            Length = bl.Length;
         }
      }

      public DataBlock(ShortDataBlock? bl)
         : this() {
         if (bl != null) {
            Offset = bl.Offset;
            Length = bl.Length;
         }
      }

      public DataBlock(DataBlockWithRecordsize? bl)
         : this() {
         if (bl != null) {
            Offset = bl.Offset;
            Length = bl.Length;
         }
      }

      public DataBlock(UInt32 offset, UInt32 length) {
         Offset = offset;
         Length = length;
      }

      /// <summary>
      /// liest die Blockdaten
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         Offset = br.Read4UInt();
         Length = br.Read4UInt();
      }

      /// <summary>
      /// schreibt die Blockdaten
      /// </summary>
      /// <param name="bw"></param>
      public void Write(BinaryReaderWriter bw) {
         bw.Write(Offset);
         bw.Write(Length);
      }
      
      /// <summary>
      /// Offset um die Differenz verändern
      /// </summary>
      /// <param name="offsetdiff"></param>
      public void AdjustOffset(int offsetdiff) {
         if (offsetdiff > 0)
            Offset += (uint)offsetdiff;
         else
            Offset -= (uint)-offsetdiff;
      }

      public override string ToString() {
         return string.Format("Block=[Offset 0x{0:X}, Length 0x{1:X}]", Offset, Length);
      }
   }

}
