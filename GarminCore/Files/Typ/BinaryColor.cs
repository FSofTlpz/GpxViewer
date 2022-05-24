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
using System.Drawing;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// static-Methoden zum Lesen und Schreiben von Farben aus Streams
   /// </summary>
   public class BinaryColor {

      /// <summary>
      /// liest eine Farbtabelle ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="iCols">Anzahl der Farben in der Tabelle</param>
      /// <param name="bWithAlpha">Farben mit oder ohne Alphaanteil</param>
      /// <returns>Farbtabelle</returns>
      public static Color[] ReadColorTable(BinaryReaderWriter br, int iCols = 1, bool bWithAlpha = false) {
         Color[] col = new Color[iCols];
         if (!bWithAlpha) {

            for (int i = 0; i < iCols; i++) {
               byte blue = br.ReadByte();
               byte green = br.ReadByte();
               byte red = br.ReadByte();
               col[i] = Color.FromArgb(red, green, blue);
            }

         } else {

            // Länge der Farbtabelle ermitteln
            int len = iCols * 3 + iCols / 2;
            if (iCols % 2 == 1) len++;
            // Farbtabelle einlesen
            byte[] colortable = br.ReadBytes(len);
            byte[] halfbytetable = new byte[2 * len];
            for (int i = 0; i < len; i++) {
               halfbytetable[2 * i] = (byte)(colortable[i] & 0xf);
               halfbytetable[2 * i + 1] = (byte)(colortable[i] >> 4);
            }
            for (int i = 0; i < iCols; i++) {
               byte blue = (byte)(halfbytetable[7 * i] | (halfbytetable[7 * i + 1] << 4));
               byte green = (byte)(halfbytetable[7 * i + 2] | (halfbytetable[7 * i + 3] << 4));
               byte red = (byte)(halfbytetable[7 * i + 4] | (halfbytetable[7 * i + 5] << 4));
               byte alpha = halfbytetable[7 * i + 6];
               alpha = (byte)((255 * alpha) / 15);       // 0x0..0xf --> 0x0..0xff
               col[i] = Color.FromArgb(~alpha & 0xff, red, green, blue);
            }

         }
         return col;
      }

      /// <summary>
      /// liest eine einzelne Farbe ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="bWithAlpha"></param>
      /// <returns></returns>
      public static Color ReadColor(BinaryReaderWriter br, bool bWithAlpha = false) {
         return ReadColorTable(br, 1, false)[0];
      }

      /// <summary>
      /// schreibt eine Farbtabelle in den Stream
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="coltable"></param>
      /// <param name="bWithAlpha"></param>
      public static void WriteColorTable(BinaryReaderWriter bw, Color[] coltable, bool bWithAlpha = false) {
         if (!bWithAlpha) {
            for (int i = 0; i < coltable.Length; i++) {
               bw.Write(coltable[i].B);
               bw.Write(coltable[i].G);
               bw.Write(coltable[i].R);
            }
         } else {
            // Länge der Farbtabelle ermitteln
            int len = coltable.Length * 3 + coltable.Length / 2;
            if (coltable.Length % 2 == 1) len++;
            // Farbtabelle erzeugen
            byte[] colortable = new byte[len];

            byte[] halfbytetable = new byte[2 * len];
            for (int i = 0, j = 0; i < coltable.Length; i++) {
               halfbytetable[j++] = (byte)(coltable[i].B & 0xf);
               halfbytetable[j++] = (byte)(coltable[i].B >> 4);
               halfbytetable[j++] = (byte)(coltable[i].G & 0xf);
               halfbytetable[j++] = (byte)(coltable[i].G >> 4);
               halfbytetable[j++] = (byte)(coltable[i].R & 0xf);
               halfbytetable[j++] = (byte)(coltable[i].R >> 4);
               halfbytetable[j++] = (byte)(0xf - ((float)coltable[i].A / 0xff) * 0xf);
            }
            for (int i = 0; i < colortable.Length; i++) {
               colortable[i] = (byte)(halfbytetable[2 * i] | (halfbytetable[2 * i + 1] << 4));
            }
            //bool bMoveHalfbyte = false;
            //for (int i = 0, dest = 0; i < coltable.Length; i++) {
            //   byte alpha = (byte)(~coltable[i].A / 255f * 15);
            //   if (bMoveHalfbyte) {
            //      colortable[dest++] |= (byte)(coltable[i].B << 4);
            //      colortable[dest] = (byte)(coltable[i].B >> 4);
            //      colortable[dest++] |= (byte)(coltable[i].G << 4);
            //      colortable[dest] = (byte)(coltable[i].G >> 4);
            //      colortable[dest++] |= (byte)(coltable[i].R << 4);
            //      colortable[dest] = (byte)(coltable[i].R >> 4);
            //      colortable[dest++] |= (byte)(alpha << 4);
            //   } else {
            //      colortable[dest++] = coltable[i].B;
            //      colortable[dest++] = coltable[i].G;
            //      colortable[dest++] = coltable[i].R;
            //      colortable[dest] = alpha;
            //   }
            //   bMoveHalfbyte = !bMoveHalfbyte;
            //}
            bw.Write(colortable);
         }
      }

      /// <summary>
      /// schreibt eine einzelne Farbe (ohne Alpha) in den Stream
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="col"></param>
      public static void WriteColor(BinaryReaderWriter bw, Color col) {
         WriteColorTable(bw, new Color[] { col });
      }

   }

}
