/*
Copyright (C) 2011, 2016 Frank Stinner

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
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// ColorMode für Bitmaps (PixMap)
   /// </summary>
   public enum BitmapColorMode {
      POI_SIMPLE = 0,

      /// <summary>
      /// eine zusätzlicher Farbcode (nicht in der Farbtabelle enthalten) steht für transparente Pixel
      /// </summary>
      POI_TR = 0x10,

      /// <summary>
      /// jede Farbe kann ihre eigene Transparenz haben
      /// </summary>
      POI_ALPHA = 0x20,

      /// <summary>
      /// 1 Bit je Pixel, 2 Farben
      /// </summary>
      POLY2 = 0xfffd,

      /// <summary>
      /// 1 Bit je Pixel, 1 Farbe (Code 1) + Transparenz
      /// </summary>
      POLY1TR = 0xfffe,

      unknown = 0xffff
   }

   /// <summary>
   /// zur Behandlung der verschiedenen Bitmaps
   /// </summary>
   public class PixMap {

      public PixData data { get; private set; } = new PixData(0, 0, 0);

      /// <summary>
      /// Bildbreite
      /// </summary>
      public uint Width { get { return data == null ? 0 : data.Width; } }
      /// <summary>
      /// Bildhöhe
      /// </summary>
      public uint Height { get { return data == null ? 0 : data.Height; } }

      /// <summary>
      /// Bits je Pixel
      /// </summary>
      public uint BpP { get { return data == null ? 0 : data.BpP; } }

      /// <summary>
      /// Anzahl der Farben
      /// </summary>
      public uint Colors { get { return colColorTable != null ? (uint)colColorTable.Length : 0; } }

      /// <summary>
      /// liefert den BitmapColorMode des Bildes
      /// </summary>
      public BitmapColorMode Colormode { get; private set; }

      /// <summary>
      /// Farbe für volle Transparenz; stimmt im Prinzip mit Color.Transparent überein, hat aber intert eine andere Name-Eigenschaft
      /// </summary>
      public static Color TransparentColor { get; private set; }

      /// <summary>
      /// Farbtabelle (oder null, wenn keine benötigt wird)
      /// </summary>
      protected Color[] colColorTable = [];


      static PixMap() {
         TransparentColor = Color.FromArgb(0, 255, 255, 255);
      }

      /// <summary>
      /// erzeugt das Bild aus dem Stream mit vorgegebener Farbtabelle (es werden nur noch die Pixeldaten eingelesen)
      /// </summary>
      /// <param name="br"></param>
      /// <param name="col">Farbtabelle</param>
      /// <param name="iWidth"></param>
      /// <param name="iHeight"></param>
      public PixMap(uint iWidth, uint iHeight, Color[] col, BitmapColorMode cm, BinaryReaderWriter? br = null)
         : this(iWidth, iHeight, col.Length, cm) {
         col.CopyTo(colColorTable, 0);
         if (br != null)
            data.Read(br);
      }

      /// <summary>
      /// erzeugt ein Bild mit max. 256 Farben (auch 0 Farben); transparent ist die "Dummy"-Farbe; liest die Farbtabelle und die Daten ev. aus dem Stream
      /// </summary>
      /// <param name="iWidth"></param>
      /// <param name="iHeight"></param>
      /// <param name="iColors">Anzahl der einzulesenden Farben</param>
      /// <param name="cm"></param>
      /// <param name="br"></param>
      public PixMap(uint iWidth, uint iHeight, int iColors, BitmapColorMode cm, BinaryReaderWriter? br = null) {
         data = new PixData(iWidth, iHeight, BitsPerPixel4BitmapColorMode(cm, iColors));
         Colormode = cm;
         colColorTable = new Color[iColors];
         for (int i = 0; i < iColors; i++)      // Init. der Farbtabelle
            colColorTable[i] = TransparentColor;
         if (cm == BitmapColorMode.POLY1TR)     // in diesem Spezialfall alle Pixel/Bits auf 1 setzen
            data.SetAllBits();

         if (br != null)
            if (iColors == 0) {        // keine Farbtabelle, d.h. Pixel sind direkt durch ihre Farben definiert
               colColorTable = new Color[0];
               switch (Colormode) {
                  case BitmapColorMode.POI_SIMPLE:
                     data = new PixData(iWidth, iHeight, BpP, br);
                     break;
                  case BitmapColorMode.POI_TR:
                     /* Keine Ahnung warum. Eigentlich wären überhaupt keine Daten nötig.
                      * Mit dem 1 Bit könnten 2 Farben gemeint sein: 
                      * 0 --> transparent
                      * 1 --> ???
                      * In der Praxis scheint aber immer 0 darin zu stehen.
                      */
                     data = new PixData(iWidth, iHeight, BpP, br);
                     //for(int i=0;i<rawimgdata.Length;i++)
                     //   if (rawimgdata[i] != 0) {
                     //      Debug.WriteLine("Bitmap mit iColors==0 und NICHT alle Daten=0");
                     //      break;
                     //   }
                     break;
                  case BitmapColorMode.POI_ALPHA:
                     data = new PixData(iWidth, iHeight, BpP, br);
                     break;
                  case BitmapColorMode.POLY1TR:
                  case BitmapColorMode.POLY2:
                     throw new Exception(string.Format("Für den ColorMode {0} muß eine Farbtabelle mit eingelesen werden.", Colormode));
                  default:
                     throw new Exception(string.Format("Unbekannter ColorMode für Bitmap ({0:x2}).", Colormode));
               }
            } else {
               switch (Colormode) {
                  case BitmapColorMode.POI_SIMPLE:
                  case BitmapColorMode.POI_TR:
                     colColorTable = BinaryColor.ReadColorTable(br, iColors, false);
                     break;
                  case BitmapColorMode.POI_ALPHA:
                     colColorTable = BinaryColor.ReadColorTable(br, iColors, true);
                     break;
                  case BitmapColorMode.POLY1TR:
                     if (iColors != 1)
                        throw new Exception(string.Format("Für den ColorMode {0} kann nur 1 Farbe (nicht {1}) eingelesen werden.",
                                             Colormode, iColors));
                     colColorTable = BinaryColor.ReadColorTable(br, 1, false);
                     break;
                  case BitmapColorMode.POLY2:
                     if (iColors != 2)
                        throw new Exception(string.Format("Für den ColorMode {0} können nur 2 Farben (nicht {1}) eingelesen werden.",
                                             Colormode, iColors));
                     colColorTable = BinaryColor.ReadColorTable(br, 2, false);
                     break;
                  default:
                     throw new Exception(string.Format("Unbekannter ColorMode für Bitmap ({0:x2}).", Colormode));
               }
               data = new PixData(iWidth, iHeight, BpP, br);
            }
      }

      /// <summary>
      /// erzeugt eine Kopie des Bildes
      /// </summary>
      /// <param name="xpm"></param>
      public PixMap(PixMap xpm) {
         data = new PixData(xpm.data);
         Colormode = xpm.Colormode;
         colColorTable = new Color[xpm.colColorTable.Length];
         xpm.colColorTable.CopyTo(colColorTable, 0);
      }

      /// <summary>
      /// erzeugt ein Bild aus dem Bitmap
      /// </summary>
      /// <param name="bm">Bitmap</param>
      /// <param name="cm"></param>
      /// <param name="bExtended">true für Bilder mit mehr 255 Farben</param>
      public PixMap(Bitmap bm, BitmapColorMode cm, bool bExtended = false) {
         Colormode = cm;
         Color[]? coltab = GraphicElement.GetBitmapColorInfo(bm, out bool bWithTransp, out bool bWithAlpha);
         if (coltab == null)
            throw new Exception("missing colortable");
         CreateFromBitmap(bm, Colormode, bExtended, coltab, bWithTransp, bWithAlpha);
      }

      /// <summary>
      /// erzeugt ein Bild aus dem Bitmap
      /// </summary>
      /// <param name="bm">Bitmap</param>
      /// <param name="bAsPoi">Bild für einen POI oder Polygon/Linie</param>
      /// <param name="bExtended"></param>
      public PixMap(Bitmap bm, bool bAsPoi, bool bExtended) {
         Colormode = AnalyseBitmap(bm, bAsPoi, out Color[]? coltab, out bool bWithTransp, out bool bWithAlpha);
         if (coltab == null)
            throw new Exception("missing colortable");
         CreateFromBitmap(bm, Colormode, bExtended, coltab, bWithTransp, bWithAlpha);
      }

      protected void CreateFromBitmap(Bitmap bm, BitmapColorMode cm, bool bExtended, Color[] coltab, bool bWithTransp, bool bWithAlpha) {
         if (Colormode == BitmapColorMode.unknown)
            throw new Exception("Unsinniger BitmapColorMode.");
         // diverse Fehler abweisen
         if (!bExtended && coltab.Length > 255)
            throw new Exception("Es sind max. 255 Farben im Bild möglich.");
         if (coltab.Length <= 255)
            bExtended = false;

         if (bWithAlpha && Colormode != BitmapColorMode.POI_ALPHA)
            throw new Exception(string.Format("Der gewählte Farbmodus {0} ist für halbtransparente Farben nicht verwendbar.", Colormode));

         if (Colormode == BitmapColorMode.POLY2 && (coltab.Length > 2 || bWithTransp))
            throw new Exception(string.Format("Beim Farbmodus {0} sind max. 2 Farben im Bild möglich. Davon darf keine transparent sein.", Colormode));

         if (Colormode == BitmapColorMode.POLY1TR && ((!bWithTransp && coltab.Length > 1) ||
                                                      (bWithTransp && coltab.Length > 2)))
            throw new Exception(string.Format("Beim Farbmodus {0} ist außer Transparenz nur 1 Farbe im Bild möglich.", Colormode));

         if (!bExtended) {
            // Farben in die Tabelle übernehmen
            switch (Colormode) {
               case BitmapColorMode.POLY1TR:
                  colColorTable = new Color[1] { coltab[0] };
                  break;

               case BitmapColorMode.POI_TR:
                  colColorTable = new Color[coltab.Length];
                  for (int i = 0; i < coltab.Length; i++)     // die letzte Farbe (nicht in der Tabelle enthalten) ist immer transp.
                     colColorTable[i] = coltab[i];
                  break;

               default:
                  colColorTable = coltab;
                  break;
            }
            // verwendete Farben "einsammeln"
            Dictionary<Color, int> Colors = new Dictionary<Color, int>();
            // Farbindex setzen
            for (int i = 0; i < colColorTable.Length; i++)
               Colors[colColorTable[i]] = i;
            if (Colormode == BitmapColorMode.POLY2 && Colors.Count == 1)
               Colors.Add(Color.White, Colors.Count);          // es war nur 1 Farbe enthalten, deshalb zusätzlich eine Dummy-Farbe aufnehmen
            int tr = 0;
            if (Colormode == BitmapColorMode.POLY1TR ||
                Colormode == BitmapColorMode.POI_TR) {
               tr = 1;
               Colors[colColorTable[0]] = 1;
               Colors[TransparentColor] = 0; // transp. Farbe als 0

               //Colors.Add(TransparentColor, Colors.Count);     // als höchster Index die transp. Farbe
            }
            //data = new PixData(bm, BitsPerPixel4BitmapColorMode(Colormode, coltab.Length), Colors);
            data = new PixData(bm, BitsPerPixel4BitmapColorMode(Colormode, Colors.Count - tr), Colors);
         } else {
            colColorTable = new Color[0];
            data = new PixData(bm, BitsPerPixel4BitmapColorMode(Colormode, 0));
         }
      }

      /// <summary>
      /// setzt den Colormode neu (falls das entsprechend des BpP möglich ist)
      /// </summary>
      /// <param name="cm"></param>
      public void ChangeColorMode(BitmapColorMode cm) {
         switch (Colormode) {
            case BitmapColorMode.POI_SIMPLE:
            case BitmapColorMode.POI_TR:
            case BitmapColorMode.POI_ALPHA:
               if (BpP > 1 && (cm == BitmapColorMode.POLY1TR || cm == BitmapColorMode.POLY2))
                  throw new Exception(string.Format("Der Colormode kann nicht von {0} in {1} geändert werden.", Colormode, cm));
               break;
            case BitmapColorMode.POLY1TR:
            case BitmapColorMode.POLY2:
               break;
         }
         Colormode = cm;
      }

      /// <summary>
      /// setzt eine neue Farbtabelle
      /// </summary>
      /// <param name="newCol"></param>
      public void SetNewColors(Color[] newCol) {
         colColorTable = new Color[newCol.Length];
         newCol.CopyTo(colColorTable, 0);
      }

      /// <summary>
      /// setzt eine Farbe der Farbtabelle neu
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="newCol"></param>
      public void SetNewColor(uint idx, Color newCol) {
         if (idx >= colColorTable.Length)
            throw new Exception("Falscher Index für die Farbtabelle");
         colColorTable[idx] = newCol;
      }

      /// <summary>
      /// liefert eine Farbe der Farbtabelle
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Color GetColor(uint idx) {
         if (idx >= colColorTable.Length)
            throw new Exception("Falscher Index für die Farbtabelle");
         return colColorTable[idx];
      }

      /// <summary>
      /// tauscht in den Bitmapdaten Index 0 und 1 aus (nur für BpP == 1)
      /// </summary>
      public void InvertBits() {
         if (data != null)
            data.InvertBits();
      }

      /// <summary>
      /// liefert das Bild als Bitmap
      /// </summary>
      /// <returns></returns>
      public Bitmap AsBitmap() {
         return data.AsBitmap(colColorTable, TransparentColor);
      }

      /// <summary>
      /// schreibt nur die Farbtabelle (für Polygon/Polyline)
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="bWithAlpha"></param>
      public void WriteColorTable(BinaryReaderWriter bw) {
         BinaryColor.WriteColorTable(bw, colColorTable, false);
      }

      /// <summary>
      /// schreibt nur die Daten (für Polygon/Polyline)
      /// </summary>
      /// <param name="bw"></param>
      public void WriteRawdata(BinaryReaderWriter bw) {
         data.Write(bw);
      }

      /// <summary>
      /// schreibt das Bitmap als POI in den Stream (falls der Colormode stimmt, sonst Exception)
      /// </summary>
      /// <param name="bw"></param>
      public void WriteAsPoi(BinaryReaderWriter bw) {
         switch (Colormode) {
            case BitmapColorMode.POI_SIMPLE:
            case BitmapColorMode.POI_TR:
               bw.Write((byte)Width);
               bw.Write((byte)Height);
               bw.Write((byte)colColorTable.Length);
               bw.Write((byte)Colormode);
               BinaryColor.WriteColorTable(bw, colColorTable, false);
               data.Write(bw);
               break;

            case BitmapColorMode.POI_ALPHA:
               bw.Write((byte)Width);
               bw.Write((byte)Height);
               bw.Write((byte)colColorTable.Length);
               bw.Write((byte)Colormode);
               BinaryColor.WriteColorTable(bw, colColorTable, true);
               data.Write(bw);
               break;

            default:
               throw new Exception(string.Format("Unerlaubter ColorMode für Bitmap ({0:x2}).", Colormode));
         }

      }

      /// <summary>
      /// liefert die Bits je Pixel in Abhängigkeit vom BitmapColorMode und der Farbanzahl (auch für 0 Farben)
      /// </summary>
      /// <param name="cm"></param>
      /// <param name="cols">Farbanzahl (ohne transparente Farbe bei POI_TR und POLY1TR)</param>
      /// <returns></returns>
      protected static uint BitsPerPixel4BitmapColorMode(BitmapColorMode cm, int cols) {
         // vgl. auch misch (http://ati.land.cz/)
         uint iBpp = 0;
         if (cols > 255)
            throw new Exception("Zuviele Farben.");
         switch (cm) {
            case BitmapColorMode.POI_SIMPLE:
               switch (cols) {
                  case 0: iBpp = 24; break;        // 16?, "Truecolor"
                  case 1: iBpp = 1; break;
                  case 2:
                  case 3: iBpp = 2; break;
                  default:
                     if (cols <= 15) iBpp = 4;
                     else iBpp = 8;
                     break;
               }
               break;

            case BitmapColorMode.POI_TR:         // Die transparente Farbe kommt noch dazu.
               //switch (cols) {
               //   case 0: iBpp = 1; break;        // "Truecolor"
               //   case 1:
               //   case 2:
               //   case 3: iBpp = 2; break;
               //   default:
               //      if (cols <= 15) iBpp = 4;
               //      else iBpp = 8;
               //      break;
               //}
               switch (cols) {
                  case 0: iBpp = 1; break;        // "Truecolor"
                  case 1:
                  case 2: iBpp = 2; break;
                  default:
                     if (cols <= 14) iBpp = 4;
                     else iBpp = 8;
                     break;
               }
               break;

            case BitmapColorMode.POI_ALPHA:
               switch (cols) {
                  case 0: iBpp = 32; break;        // 16?, "Truecolor"
                  case 1: iBpp = 1; break;
                  case 2:
                  case 3: iBpp = 2; break;
                  default:
                     if (cols <= 15) iBpp = 4;
                     else iBpp = 8;
                     break;
               }
               break;

            case BitmapColorMode.POLY1TR:
               if (cols > 2)
                  throw new Exception(string.Format("Zuviele Farben ({0}) für den BitmapColorMode {1}.", cols, cm));
               iBpp = 1;
               break;

            case BitmapColorMode.POLY2:
               if (cols > 2)
                  throw new Exception(string.Format("Zuviele Farben ({0}) für den BitmapColorMode {1}.", cols, cm));
               iBpp = 1;
               break;

            default:
               throw new Exception(string.Format("Unbekannter ColorMode für Bitmap ({0:x2}).", cm));
         }
         return iBpp;
      }

      /// <summary>
      /// analysiert ein vorgegebenes Bitmap
      /// </summary>
      /// <param name="bm">Bitmap</param>
      /// <param name="b4Poi">für POI's oder Polygone/Linien</param>
      /// <param name="coltab">liefert die Farbtabelle des Bitmap zurück (falls vorhanden auch mit der transp. Farbe)</param>
      /// <param name="bWithTransp">liefert die Info ob es transparente Pixel gibt</param>
      /// <param name="bWithAlpha">liefert die Info ob es Farben mit Alphaanteil gibt</param>
      /// <returns>liefert den einfachstmöglichen BitmapColorMode</returns>
      public static BitmapColorMode AnalyseBitmap(Bitmap bm, bool b4Poi, out Color[]? coltab, out bool bWithTransp, out bool bWithAlpha) {
         coltab = new Color[0];
         bWithTransp = bWithAlpha = false;
         if (b4Poi) {
            if (bm == null)
               return BitmapColorMode.POI_SIMPLE;
            coltab = GraphicElement.GetBitmapColorInfo(bm, out bWithTransp, out bWithAlpha);
            if (!bWithAlpha)
               if (bWithTransp)
                  return BitmapColorMode.POI_TR;
               else
                  return BitmapColorMode.POI_SIMPLE;
            return BitmapColorMode.POI_ALPHA;
         } else {
            if (bm == null)
               return BitmapColorMode.POLY1TR;
            coltab = GraphicElement.GetBitmapColorInfo(bm, out bWithTransp, out bWithAlpha);
            if (bWithAlpha)
               throw new Exception("Ein Bitmap mit Alpha kann nicht für Polygone/Linien verwendet werden.");
            if (coltab != null) {
               if (coltab.Length == 1 ||
                  (coltab.Length == 2 && bWithTransp))
                  return BitmapColorMode.POLY1TR;
               if (coltab.Length != 2)
                  throw new Exception("Ein Bitmap mit mehr als 2 Farben kann nicht für Polygone/Linien verwendet werden.");
            }
            return BitmapColorMode.POLY2;
         }
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("XPixMap=[ " + Width.ToString() + "x" + Height.ToString() + "," + Colormode.ToString() + ",");
         for (int i = 0; i < colColorTable.Length; i++)
            sb.Append(" " + colColorTable[i].ToString());
         sb.Append(" ]");
         return sb.ToString();
      }

   }

}
