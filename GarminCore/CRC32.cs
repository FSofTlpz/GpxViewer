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
   public class CRC32 {
      const UInt32 Poly = 0xEDB88320;    // für Standard-CRC32: Wert kann verändert werden

      long crc = 0xFFFFFFFF;
      static UInt32[] crc_table;

      static CRC32() {
         // Tabelle füllen
         crc_table = new UInt32[256];
         UInt32 crc;
         UInt32 i, j;

         for (i = 0; i < 256; i++) {
            crc = i;
            for (j = 0; j < 8; j++)
               if ((crc & 0x1) == 1)
                  crc = (crc >> 1) ^ Poly;
               else
                  crc = (crc >> 1);
            crc_table[i] = crc;
         }
      }

      public CRC32() { }

      /// <summary>
      /// fügt ein neues Byte hinzu
      /// </summary>
      /// <param name="c"></param>
      /// <returns>liefert die akt. CRC</returns>
      public UInt32 update(byte c) {
         crc = ((crc & 0xFFFFFF00) / 0x100) & 0xFFFFFF ^ crc_table[c ^ crc & 0xFF];
         return Value;
      }

      /// <summary>
      /// akt. Wert der Checksumme
      /// </summary>
      public UInt32 Value {
         get {
            return (UInt32)((-(crc)) - 1);         // !(CRC)
         }
      }
   }
}
