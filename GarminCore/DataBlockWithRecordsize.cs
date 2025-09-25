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
   /// beschreibt einen Dateibereich mit Offset und Länge (4 + 4 Byte) und einer Satzlänge (2 Byte)
   /// </summary>
   public class DataBlockWithRecordsize : DataBlock {
    
      /// <summary>
      /// Satzlänge im Dateibereich
      /// </summary>
      public UInt16 Recordsize { get; set; }
      
      /// <summary>
      /// berechnete Datensatzanzahl
      /// </summary>
      public int Count {
         get => Recordsize > 0 ? (int)Length / Recordsize : 0;
      }


      public DataBlockWithRecordsize()
         : base() {
         Recordsize = 0;
      }

      public DataBlockWithRecordsize(DataBlockWithRecordsize? bl)
         : base((DataBlock?)bl) {
         Recordsize = bl != null ? bl.Recordsize : (ushort)0;
      }

      public DataBlockWithRecordsize(DataBlock? bl, UInt16 recordsize)
         : base(bl) {
         Recordsize = recordsize;
      }

      public DataBlockWithRecordsize(BinaryReaderWriter br)
         : this() {
         Read(br);
      }

      /// <summary>
      /// liest die Blockdaten
      /// </summary>
      /// <param name="br"></param>
      public new void Read(BinaryReaderWriter br) {
         base.Read(br);
         Recordsize = br.Read2AsUShort();
      }

      /// <summary>
      /// schreibt die Blockdaten
      /// </summary>
      /// <param name="bw"></param>
      public new void Write(BinaryReaderWriter bw) {
         base.Write(bw);
         bw.Write(Recordsize);
      }

      public override string ToString() {
         return string.Format("Block=[Offset 0x{0:X}, Length 0x{1:X}, Recordsize 0x{2:X}]", Offset, Length, Recordsize);
      }
   }

}
