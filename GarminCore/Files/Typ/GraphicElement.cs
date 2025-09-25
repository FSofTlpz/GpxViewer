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
   /// Basisklasse für die eigentlichen Grafikelemente
   /// </summary>
   public abstract class GraphicElement : IComparable {

      public enum Fontdata {
         Default = 0x0,
         Nolabel = 0x1,
         Small = 0x2,
         Normal = 0x3,
         Large = 0x4
      }

      public enum FontColours {
         No = 0x0,
         Day = 0x8,
         Night = 0x10,
         DayAndNight = 0x18
      }

      /// <summary>
      /// Typ des Elements
      /// </summary>
      public uint Type { get; set; }
      /// <summary>
      /// Subtyp des Elements
      /// </summary>
      public uint Subtype { get; set; }

      /// <summary>
      /// Multisprachlicher Text
      /// </summary>
      public MultiText Text { get; protected set; }

      protected Color[] colDayColor;            // Vorder-, Hintergrund
      protected Color[] colNightColor;          // Vorder-, Hintergrund
      protected Color[] colFontColour;          // zusätzliche (Text-)Farben für Tag, Nacht

      public PixMap? XBitmapDay { get; protected set; }
      public PixMap? XBitmapNight { get; protected set; }

      public byte Options { get; protected set; }
      public byte ExtOptions { get; protected set; }

      /// <summary>
      /// Fontdaten (Bit 0,1,2)
      /// </summary>
      public Fontdata FontType {
         get { return (Fontdata)(ExtOptions & 0x7); }
         set { ExtOptions = (byte)((ExtOptions & 0xf8) + value); SetExtendedOptions(); }
      }
      /// <summary>
      /// Fontfarben (Bit 3,4)
      /// </summary>
      public FontColours FontColType {
         get { return (FontColours)(ExtOptions & 0x18); }
         set { ExtOptions = (byte)((ExtOptions & 0x7) + value); SetExtendedOptions(); }
      }

      /// <summary>
      /// setz 'WithExtendedOptions'
      /// </summary>
      protected void SetExtendedOptions() {
         WithExtendedOptions = !(FontType == Fontdata.Default &&
                                 FontColType == FontColours.No);
      }


      public GraphicElement() {
         Type = Subtype = 0;
         colDayColor = new Color[2];
         colDayColor[0] = colDayColor[1] = PixMap.TransparentColor;
         colNightColor = new Color[2];
         colNightColor[0] = colNightColor[1] = PixMap.TransparentColor;
         colFontColour = new Color[2];
         colFontColour[0] = colFontColour[1] = PixMap.TransparentColor;
         Options = ExtOptions = 0;
         Text = new MultiText();
         XBitmapDay = null;
         XBitmapNight = null;
      }

      public GraphicElement(GraphicElement ge)
         : this() {
         Type = ge.Type;
         Subtype = ge.Subtype;
         Text = new MultiText(ge.Text);
         for (int i = 0; i < colDayColor.Length; i++)
            colDayColor[i] = ge.colDayColor[i];
         for (int i = 0; i < colNightColor.Length; i++)
            colNightColor[i] = ge.colNightColor[i];
         for (int i = 0; i < colFontColour.Length; i++)
            colFontColour[i] = ge.colFontColour[i];
         Options = ge.Options;
         ExtOptions = ge.ExtOptions;
         FontType = ge.FontType;
         FontColType = ge.FontColType;
         if (ge.XBitmapDay != null)
            XBitmapDay = new PixMap(ge.XBitmapDay);
         if (ge.XBitmapNight != null)
            XBitmapNight = new PixMap(ge.XBitmapNight);
      }

      /// <summary>
      /// 1. Fontfarbe
      /// </summary>
      public Color FontColor1 {
         get { return colFontColour[0]; }
         set { colFontColour[0] = value; }
      }
      /// <summary>
      /// 2. Fontfarbe
      /// </summary>
      public Color FontColor2 {
         get { return colFontColour[1]; }
         set { colFontColour[1] = value; }
      }

      /// <summary>
      /// Ex. ein Text?
      /// </summary>
      public abstract bool WithString { get; protected set; }

      /// <summary>
      /// Sind spezielle Texteigenschaften festgelegt?
      /// </summary>
      public abstract bool WithExtendedOptions { get; protected set; }

      /// <summary>
      /// Gibt es ein Bitmap für den Tag?
      /// </summary>
      public abstract bool WithDayBitmap { get; }

      /// <summary>
      /// Gibt es ein Bitmap für die Nacht?
      /// </summary>
      public abstract bool WithNightBitmap { get; }
      /// <summary>
      /// liefert das Element als Bitmap
      /// </summary>
      /// <param name="b4Day">für Tag oder Nacht</param>
      /// <param name="bExt">auch wenn intern nicht als Bitmap def.</param>
      /// <returns></returns>
      public abstract Bitmap? AsBitmap(bool b4Day, bool bExt);

      /// <summary>
      /// Breite des Elements
      /// </summary>
      public abstract uint Width { get; protected set; }
      /// <summary>
      /// Höhe des Elements
      /// </summary>
      public abstract uint Height { get; protected set; }

      /// <summary>
      /// setzt den Multitext neu
      /// </summary>
      /// <param name="txt"></param>
      public void SetText(MultiText txt) {
         bool bWithtxt = false;
         for (int i = 0; i < txt.Count; i++)
            if (txt.Get(i).Txt.Length > 0) {
               bWithtxt = true;
               break;
            }
         Text = new MultiText(txt);
         WithString = bWithtxt;
      }

      /// <summary>
      /// setzt ein Bit im Byte auf 0 oder 1
      /// </summary>
      /// <param name="b"></param>
      /// <param name="bit"></param>
      /// <param name="set"></param>
      /// <returns></returns>
      protected byte SetBit(byte b, byte bit, bool set = true) {
         return (byte)Bit.Set(b, bit, set);
      }

      /// <summary>
      /// testet, ob das Bit gesetzt ist
      /// </summary>
      /// <param name="b"></param>
      /// <param name="bit"></param>
      /// <returns></returns>
      protected bool BitIsSet(byte b, byte bit) {
         return Bit.IsSet(b, bit);
      }

      // Die Farben haben je nach PolygonType/PolylineType nicht immer alle eine Bedeutung!

      public Color DayColor1 {
         get { return colDayColor[0]; }
         set {
            colDayColor[0] = value;
            if (XBitmapDay != null)
               XBitmapDay.SetNewColor(0, value);
         }
      }
      public Color DayColor2 {
         get { return colDayColor[1]; }
         set {
            colDayColor[1] = value;
            if (XBitmapDay != null && XBitmapDay.Colors > 1)
               XBitmapDay.SetNewColor(1, value);
         }
      }
      public Color NightColor1 {
         get { return colNightColor[0]; }
         set {
            colNightColor[0] = value;
            if (XBitmapNight != null)
               XBitmapNight.SetNewColor(0, value);
         }
      }
      public Color NightColor2 {
         get { return colNightColor[1]; }
         set {
            colNightColor[1] = value;
            if (XBitmapNight != null && XBitmapNight.Colors > 1)
               XBitmapNight.SetNewColor(1, value);
         }
      }

      /// <summary>
      /// liefert Farbinfos und die Farbtabelle zu einem Bitmap; bei bWithTransparent==true steht diese Farbe immer an letzter
      /// Stelle der Tabelle
      /// </summary>
      /// <param name="bm"></param>
      /// <param name="bWithTransparent"></param>
      /// <param name="bWithAlpha"></param>
      /// <returns></returns>
      public static Color[]? GetBitmapColorInfo(Bitmap bm, out bool bWithTransparent, out bool bWithAlpha) {
         Color[]? colColorTable = null;
         bWithTransparent = false;
         bWithAlpha = false;

         // verwendete Farben "einsammeln"
         Dictionary<Color, int> Colors = new Dictionary<Color, int>();
         for (int x = 0; x < bm.Width; x++)
            for (int y = 0; y < bm.Height; y++) {
               Color col = bm.GetPixel(x, y);
               if (col.A == 0)                        // jede Farbe mit A=0 gilt als die eine transp. Farbe
                  col = PixMap.TransparentColor;
               if (!Colors.ContainsKey(col)) {
                  Colors.Add(col, 0);
                  if (!bWithAlpha && (col.A > 0 && col.A < 255))
                     bWithAlpha = true;
               }
            }
         bWithTransparent = Colors.ContainsKey(PixMap.TransparentColor);      // mit transp. Pixeln
         if (bWithTransparent)
            Colors.Remove(PixMap.TransparentColor);

         // Farben in die Tabelle übernehmen
         colColorTable = new Color[Colors.Count + (bWithTransparent ? 1 : 0)];
         int idx = 0;
         foreach (Color col in Colors.Keys)
            colColorTable[idx++] = col;
         if (bWithTransparent)
            colColorTable[idx] = PixMap.TransparentColor;

         return colColorTable;
      }


      protected static byte[] GarminColorGray = new byte[]{
         0x00,0x10,0x20,0x31,0x41,0x52,0x62,0x73,0x83,0x94,0xa4,0xb4,0xc5,0xd5,0xe6,0xff
      };
      protected static byte[] GarminColorRed = new byte[]{
         0x00,0x39,0x7b,0xbd,0xff
      };
      protected static byte[] GarminColorGreen = new byte[]{
         0x00,0x30,0x65,0x95,0xca,0xff
      };
      protected static byte[] GarminColorBlue = new byte[]{
         0x00,0x20,0x41,0x6a,0x8b,0xb4,0xd5,0xff
      };

      /// <summary>
      /// liefert die nächstliegende Garminfarbe (wird wohl nur bei POIs verwendet)
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      public static Color GetNearestGarminColor(Color col) {
         if (col.R == col.G && col.G == col.B) {      // Graustufe
            byte v = NearestColorComponent(col.R, GarminColorGray);
            return Color.FromArgb(v, v, v);
         }
         return Color.FromArgb(NearestColorComponent(col.R, GarminColorRed),
                               NearestColorComponent(col.G, GarminColorGreen),
                               NearestColorComponent(col.B, GarminColorBlue));
      }

      /// <summary>
      /// liefert den nächstliegenden Wert aus der Tabelle
      /// </summary>
      /// <param name="v"></param>
      /// <param name="table"></param>
      /// <returns></returns>
      protected static byte NearestColorComponent(byte v, byte[] table) {
         int i = 0;
         for (i = 0; i < table.Length; i++)
            if (v <= table[i])
               break;
         if (v == table[i])
            return v;
         else {
            if (v - table[i - 1] > table[i] - v)
               return table[i];
            else
               return table[i - 1];
         }
      }

      /// <summary>
      /// Daten kopieren, die  NICHT mit MemberwiseClone() erfasst werden
      /// </summary>
      /// <param name="ge"></param>
      protected void CopyExtData(GraphicElement ge) {
         ge.colDayColor = new Color[colDayColor.Length];
         Array.Copy(colDayColor, ge.colDayColor, colDayColor.Length);
         ge.colNightColor = new Color[colNightColor.Length];
         Array.Copy(colNightColor, ge.colNightColor, colNightColor.Length);
         ge.colFontColour = new Color[colFontColour.Length];
         Array.Copy(colFontColour, ge.colFontColour, colFontColour.Length);
         if (XBitmapDay != null)
            ge.XBitmapDay = new PixMap(XBitmapDay);
         if (XBitmapNight != null)
            ge.XBitmapNight = new PixMap(XBitmapNight);
      }

      /// <summary>
      /// Hilfsfunktion zum Vergleichen für die Sortierung (über Typ und Subtyp)
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public int CompareTo(object? obj) {
         if (obj is GraphicElement) {
            GraphicElement ge = (GraphicElement)obj;
            if (ge == null) return 1;
            if (Type == ge.Type) {
               if (Subtype > ge.Subtype) return 1;
               if (Subtype < ge.Subtype) return -1;
               else return 0;
            } else
               if (Type > ge.Type) return 1;
            else return -1;
         }
         throw new ArgumentException("Falsche Objektart beim Vergleich.");
      }

   }

}
