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
using System.Collections.Generic;
using System.Drawing;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// nur zur Verwaltung der reinen Pixeldaten eines Bitmaps
   /// </summary>
   public class PixData {

      /// <summary>
      /// Bildbreite
      /// </summary>
      public uint Width { get; protected set; }

      /// <summary>
      /// Bildhöhe
      /// </summary>
      public uint Height { get; protected set; }

      /// <summary>
      /// Bits je Pixel
      /// </summary>
      public uint BpP { get; private set; }

      /// <summary>
      /// Pixeldaten (entsprechend dem Typfileinhalt)
      /// </summary>
      public byte[] rawimgdata { get; private set; }


      /// <summary>
      /// erzeugt den entsprechenden Datenpuffer und liest ihn ev. aus dem Stream
      /// </summary>
      /// <param name="width">Bildbreite</param>
      /// <param name="height">Bildhöhe</param>
      /// <param name="iBpp">Bits je Pixel</param>
      /// <param name="br">Stream</param>
      public PixData(uint width, uint height, uint bpp, BinaryReaderWriter? br = null) {
         Width = width;
         Height = height;
         BpP = bpp;
         rawimgdata = new byte[Bytes4BitmapLine(Width, BpP) * Height];
         if (br != null)
            Read(br);
      }

      /// <summary>
      /// erzeugt eine Kopie
      /// </summary>
      /// <param name="pix"></param>
      public PixData(PixData pix) :
         this(pix.Width, pix.Height, pix.BpP) {
         pix.rawimgdata.CopyTo(rawimgdata, 0);
      }

      /// <summary>
      /// erzeugt den entsprechenden Datenpuffer aus dem Bitmap
      /// </summary>
      /// <param name="bm">Bitmap</param>
      /// <param name="iBpp">Bits je Pixel</param>
      /// <param name="Col">Farbtabelle als Dictionary (Farbe, Index)</param>
      public PixData(Bitmap bm, uint iBpp, Dictionary<Color, int>? Col = null) :
         this((uint)bm.Width, (uint)bm.Height, iBpp) {
         rawimgdata = Convert2Data(bm, iBpp, Col);
      }

      /// <summary>
      /// liest die Daten aus dem Stream
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         rawimgdata = br.ReadBytes(rawimgdata.Length);
      }

      /// <summary>
      /// schreibt die Daten in den Stream
      /// </summary>
      /// <param name="bw">Ausgabestream</param>
      public void Write(BinaryReaderWriter bw) {
         bw.Write(rawimgdata);
      }

      /// <summary>
      /// liefert ein Bitmap entsprechend der Farbtabelle
      /// </summary>
      /// <param name="Col">Farbtabelle</param>
      /// <param name="colDummy">Dummyfarbe, i.A. Transparent</param>
      /// <returns></returns>
      public Bitmap AsBitmap(Color[] Col, Color colDummy) {
         return Convert2Bitmap(Width, Height, BpP, rawimgdata, Col, colDummy);
      }

      /// <summary>
      /// liefert ein Bitmap, falls die Daten die Farben direkt enthalten
      /// </summary>
      /// <returns></returns>
      public Bitmap AsBitmap() {
         return Convert2Bitmap(Width, Height, BpP, rawimgdata, null, Color.Transparent);
      }

      /// <summary>
      /// falls BpP == 1 werden alle Bits invertiert
      /// </summary>
      public void InvertBits() {
         if (BpP == 1)
            for (int i = 0; i < rawimgdata.Length; i++)
               rawimgdata[i] = (byte)(~rawimgdata[i]);
      }

      /// <summary>
      /// alle Bits auf 1 setzen
      /// </summary>
      public void SetAllBits() {
         for (int i = 0; i < rawimgdata.Length; i++)
            rawimgdata[i] = (byte)0xff;
      }

      /// <summary>
      /// erittelt die Anzahl der nötigen Bytes für eine Bitmapzeile
      /// </summary>
      /// <param name="iWidth">Bildbreite</param>
      /// <param name="iBpp">Bits je Pixel</param>
      /// <returns></returns>
      protected static uint Bytes4BitmapLine(uint iWidth, uint iBpp) {
         uint bytes4line = iWidth * iBpp;
         return bytes4line / 8 + (uint)(bytes4line % 8 > 0 ? 1 : 0);
      }

      /// <summary>
      /// die Daten im Puffer werden in ein Bitmap umgewandelt
      /// </summary>
      /// <param name="iWidth">Bildbreite</param>
      /// <param name="iHeight">Bildhöhe</param>
      /// <param name="iBpp">Bits je Pixel</param>
      /// <param name="data">Rohdaten</param>
      /// <param name="Col">Farbtabelle</param>
      /// <param name="colDummy">Dummyfarbe, i.A. Transparent</param>
      /// <returns></returns>
      protected static Bitmap Convert2Bitmap(uint iWidth, uint iHeight, uint iBpp, byte[] data, Color[]? Col, Color colDummy) {
         Bitmap bm = new Bitmap((int)iWidth, (int)iHeight);

         // Daten in Bitmap umwandeln
         if (Col != null && Col.Length > 0) {               // mit Farbtabelle

            uint pixel4byte = 8 / iBpp;
            uint bytes4line = Bytes4BitmapLine(iWidth, iBpp);
            switch (iBpp) {
               case 1:
                  for (uint y = 0; y < iHeight; y++) {
                     uint linestart = bytes4line * y;
                     for (int x = 0; x < iWidth; x++) {
                        byte dat = data[linestart + x / 8];
                        dat >>= x % 8;
                        int idx = dat & 0x1;
                        // Wenn 2 Farben def. sind bedeutet '1' Farbe 1 und '0' Farbe 2.
                        // Wenn nur 1 Farbe def. ist bedeutet '1' Farbe 1 und '0' transparent.
                        if (Col.Length == 1)
                           bm.SetPixel((int)x, (int)y, idx == 1 ? Col[0] : colDummy);
                        else
                           bm.SetPixel((int)x, (int)y, idx < Col.Length ? (idx == 1 ? Col[0] : Col[1]) : colDummy);
                     }
                  }
                  break;

               case 2:
                  for (uint y = 0; y < iHeight; y++) {
                     uint linestart = bytes4line * y;
                     for (int x = 0; x < iWidth; x++) {
                        byte dat = data[linestart + x / 4];
                        int idx = 0;
                        switch (x % 4) {
                           case 0: idx = dat & 0x3; break;           // Bit 0, 1
                           case 1: idx = (dat & 0xc) >> 2; break;    // Bit 2, 3
                           case 2: idx = (dat & 0x30) >> 4; break;   // Bit 4, 5
                           case 3: idx = (dat & 0xc0) >> 6; break;   // Bit 6, 7
                        }
                        bm.SetPixel((int)x, (int)y, idx < Col.Length ? Col[idx] : colDummy);
                     }
                  }
                  break;

               case 4:
                  for (uint y = 0; y < iHeight; y++) {
                     uint linestart = bytes4line * y;
                     for (int x = 0; x < iWidth; x++) {
                        byte dat = data[linestart + x / 2];
                        int idx = x % 2 == 0 ? (dat & 0xf) : (dat >> 4);
                        bm.SetPixel((int)x, (int)y, idx < Col.Length ? Col[idx] : colDummy);
                     }
                  }
                  break;

               case 8:
                  for (uint y = 0; y < iHeight; y++) {
                     uint linestart = bytes4line * y;
                     for (int x = 0; x < iWidth; x++) {
                        int idx = data[linestart + x];
                        bm.SetPixel((int)x, (int)y, idx < Col.Length ? Col[idx] : colDummy);
                     }
                  }
                  break;

               default:
                  throw new Exception(string.Format("Falsche Anzahl Bits/Pixel: {0}. Erlaubt sind nur 1, 2, 4 und 8.", iBpp));
            }

         } else {       // ohne Farbtabelle

            switch (iBpp) {
               case 1:
                  for (int y = 0; y < iHeight; y++)
                     for (int x = 0; x < iWidth; x++)
                        bm.SetPixel((int)x, (int)y, Color.Transparent);
                  break;

               case 24:
                  for (int y = 0; y < iHeight; y++) {
                     for (int x = 0; x < iWidth; x++) {
                        int idx = (int)iWidth * y + 3 * x;
                        byte blue = data[idx++];
                        byte green = data[idx++];
                        byte red = data[idx++];
                        bm.SetPixel((int)x, (int)y, Color.FromArgb(red, green, blue));
                     }
                  }
                  break;

               case 32:    // ? unklar ?
                  for (int y = 0; y < iHeight; y++) {
                     for (int x = 0; x < iWidth; x++) {
                        int idx = (int)iWidth * y + 3 * x;
                        byte blue = data[idx++];
                        byte green = data[idx++];
                        byte red = data[idx++];
                        byte alpha = data[idx++];
                        bm.SetPixel((int)x, (int)y, Color.FromArgb(alpha, red, green, blue));
                     }
                  }
                  break;

               default:
                  throw new Exception(string.Format("Falsche Anzahl Bits/Pixel: {0}. Erlaubt sind nur 1, 24 und 32.", iBpp));
            }

         }
         return bm;
      }

      /// <summary>
      /// aus dem Bitmap wird ein Datenpuffer mit Rohdaten erzeugt
      /// </summary>
      /// <param name="iWidth">Bildbreite</param>
      /// <param name="iHeight">Bildhöhe</param>
      /// <param name="iBpp">Bits je Pixel</param>
      /// <param name="bm">Bitmap</param>
      /// <param name="Col">Farbtabelle als Dictionary (Farbe, Index)</param>
      /// <returns></returns>
      protected static byte[] Convert2Data(Bitmap bm, uint iBpp, Dictionary<Color, int>? Col) {
         uint pixel4byte = 8 / iBpp;
         int bytes4line = (int)Bytes4BitmapLine((uint)bm.Width, iBpp);
         byte[] data = new byte[bytes4line * bm.Height];
         int idx = 0;

         if (Col != null && Col.Count > 0) {    // mit Farbtabelle

            switch (iBpp) {
               case 1:
                  for (int y = 0; y < bm.Height; y++) {
                     idx = y * bytes4line;
                     // Pixel jeweils in Gruppen von 8 verarbeiten
                     for (int x = 0; x < bm.Width; x += 8, idx++) {
                        data[idx] = 0;
                        for (int xp = 0; xp < 8 && x + xp < bm.Width; xp++)
                           data[idx] |= (byte)(GetColorIdx(Col, bm.GetPixel(x + xp, y)) << xp);
                     }
                  }
                  break;

               case 2:
                  for (int y = 0; y < bm.Height; y++) {
                     idx = y * bytes4line;
                     // Pixel jeweils in Gruppen von 4 verarbeiten
                     for (int x = 0; x < bm.Width; x += 4, idx++) {
                        data[idx] = 0;
                        for (int xp = 0; xp < 4 && x + xp < bm.Width; xp++)
                           data[idx] |= (byte)(GetColorIdx(Col, bm.GetPixel(x + xp, y)) << (2 * xp));
                     }
                  }
                  break;

               case 4:
                  for (int y = 0; y < bm.Height; y++) {
                     idx = y * bytes4line;
                     // Pixel jeweils in Gruppen von 2 verarbeiten
                     for (int x = 0; x < bm.Width; x += 2, idx++) {
                        data[idx] = 0;
                        for (int xp = 0; xp < 2 && x + xp < bm.Width; xp++)
                           data[idx] |= (byte)(GetColorIdx(Col, bm.GetPixel(x + xp, y)) << (4 * xp));
                     }
                  }
                  break;

               case 8:
                  for (int y = 0; y < bm.Height; y++) {
                     idx = y * bytes4line;
                     for (int x = 0; x < bm.Width; x++, idx++)
                        data[idx] = (byte)GetColorIdx(Col, bm.GetPixel(x, y));
                  }
                  break;

            }

         } else {          // ohne Farbtabelle

            throw new Exception("Die Erzeugung eines Datenbereiches ohne Farbtabelle ist noch nicht implementiert.");

         }
         return data;
      }

      /// <summary>
      /// liefert den Farbindex aus der Farbtabelle zur Farbe
      /// </summary>
      /// <param name="Col"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      private static int GetColorIdx(Dictionary<Color, int> Col, Color col) {
         if (Col.ContainsKey(col))
            return Col[col];
         if (Col.ContainsKey(PixMap.TransparentColor))
            return Col[PixMap.TransparentColor];
         if (Col.ContainsKey(Color.Transparent))
            return Col[Color.Transparent];
         return Col.Count;
      }

   }

}
