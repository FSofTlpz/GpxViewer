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
using System.Drawing;
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// alle notwendigen Daten für eine Polyline (entweder Bitmap 32 x h und h max. 255 (?) oder Def. mit Linienbreite
   /// innen und Gesamtbreite jeweils max. 255)
   /// </summary>
   public class Polyline : GraphicElement {

      /// <summary>
      /// Die Summe der 2 Ziffern gibt die Anzahl der gespeicherten Farben an, die Ziffern selbst die Zuordnung 
      /// zu Tag- und Nachdarstellung.
      /// Bei den NoBorder-Typen ist keine Linienbreite festgelegt (=0).
      /// Unklar ist, ob jeder Typ sowohl als Bitmap-Typ als auch als Nichtbitmap-Typ ex. kann.
      /// NoBorder_Day2_Night1 wäre als Nichtbitmap-Typ unsinnig.
      /// </summary>
      public enum PolylineType {
         Day2 = 0,
         Day2_Night2 = 1,
         Day1_Night2 = 3,
         NoBorder_Day2_Night1 = 5,
         NoBorder_Day1 = 6,
         NoBorder_Day1_Night1 = 7
      }

      public byte Options2 { get; private set; }

      /// <summary>
      /// Bitmaphöhe (Bit 3..7)
      /// </summary>
      public uint BitmapHeight {
         get { return (uint)(Options >> 3); }
         set { Options = (byte)((value << 3) | ((uint)Options & 0x7)); }
      }

      /// <summary>
      /// Linientyp (Bit 0,1,2)
      /// </summary>
      public PolylineType Polylinetype {
         get { return (PolylineType)(Options & 0x7); }
         set {
            Options = (byte)((Options & 0xf8) | (int)value);

            // so sinnvoll wie möglich die alten Werte weiterverwenden
            if (WithDayBitmap) {     // Vorgänger war durch Bitmap definiert
               // nur Farben korr., Bitmuster bleibt
               switch (Polylinetype) {
                  case PolylineType.Day1_Night2:
                  case PolylineType.NoBorder_Day1:
                  case PolylineType.NoBorder_Day1_Night1:
                     if (DayColor1 == PixMap.TransparentColor) DayColor1 = Color.MediumTurquoise;
                     DayColor2 = PixMap.TransparentColor;
                     break;
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.NoBorder_Day2_Night1:
                     if (DayColor1 == PixMap.TransparentColor) DayColor1 = Color.MediumTurquoise;
                     if (DayColor2 == PixMap.TransparentColor) DayColor2 = Color.MediumVioletRed;
                     break;
               }
               switch (Polylinetype) {
                  case PolylineType.NoBorder_Day1_Night1:
                  case PolylineType.NoBorder_Day2_Night1:
                     if (DayColor1 == PixMap.TransparentColor) DayColor1 = Color.MediumTurquoise;
                     NightColor2 = PixMap.TransparentColor;
                     break;

                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2_Night2:
                     if (NightColor1 == PixMap.TransparentColor) NightColor1 = Color.MediumTurquoise;
                     if (NightColor2 == PixMap.TransparentColor) NightColor2 = Color.MediumVioletRed;
                     break;
               }
            } else {
               if (InnerWidth == 0)
                  InnerWidth = 1;
               // BorderWidth notfalls korr.
               switch (Polylinetype) {
                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                     if (BorderWidth == 0)
                        BorderWidth = 1;
                     break;
                  case PolylineType.NoBorder_Day1:          // eigentlich nicht möglich (?)
                  case PolylineType.NoBorder_Day1_Night1:
                  case PolylineType.NoBorder_Day2_Night1:
                     BorderWidth = 0;
                     break;
               }
               if (DayColor1 == PixMap.TransparentColor) DayColor1 = Color.MediumTurquoise;
               // Farben notfalls korrigieren
               switch (Polylinetype) {
                  case PolylineType.Day1_Night2:
                  case PolylineType.NoBorder_Day1:
                  case PolylineType.NoBorder_Day1_Night1:
                     DayColor2 = PixMap.TransparentColor;
                     break;
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.NoBorder_Day2_Night1:
                     if (DayColor2 == PixMap.TransparentColor) DayColor2 = Color.MediumVioletRed;
                     break;
               }
               switch (Polylinetype) {
                  case PolylineType.NoBorder_Day2_Night1:
                  case PolylineType.NoBorder_Day1_Night1:
                     if (NightColor1 == PixMap.TransparentColor) NightColor1 = Color.MediumTurquoise;
                     NightColor2 = PixMap.TransparentColor;
                     break;
                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2_Night2:
                     if (NightColor1 == PixMap.TransparentColor) NightColor1 = Color.MediumTurquoise;
                     if (NightColor2 == PixMap.TransparentColor) NightColor2 = Color.MediumVioletRed;
                     break;
               }
               SetWidthAndColors(Polylinetype, InnerWidth, BorderWidth, DayColor1, DayColor2, NightColor1, NightColor2);
            }
         }
      }

      /// <summary>
      /// mit Text (Bit 0)
      /// </summary>
      public override bool WithString {
         get {
            return BitIsSet(Options2, 0);
         }
         protected set {
            Options2 = SetBit(Options2, 0, value);
         }
      }

      /// <summary>
      /// mit Textrotation (Bit 1)
      /// </summary>
      public bool WithTextRotation {
         get {
            return BitIsSet(Options2, 1);
         }
         set {
            Options2 = SetBit(Options2, 1, value);
            SetExtendedOptions();
            if (value && !WithExtendedOptions)
               WithExtendedOptions = true;
         }
      }

      /// <summary>
      /// mit zusätzlichen Farben oder Fonteigenschaft (Bit 2)
      /// </summary>
      public override bool WithExtendedOptions {
         get {
            return BitIsSet(Options2, 2);
         }
         protected set {
            Options2 = SetBit(Options2, 2, value);
         }
      }

      /// <summary>
      /// Linienbreite (ohne Berücksichtigung Rand; vermutlich einschließlich 2*Randbreite max. 255)
      /// </summary>
      public uint InnerWidth { get; private set; }

      /// <summary>
      /// Randbreite (für 1 Rand; vermutlich Linienbreite + 2*Randbreite max. 255)
      /// </summary>
      public uint BorderWidth { get; private set; }

      /// <summary>
      /// Breite der Linie (konstant)
      /// </summary>
      public override uint Width { get { return 32; } protected set { } }

      /// <summary>
      /// Höhe (Dicke) der Linie
      /// </summary>
      public override uint Height {
         get { return BitmapHeight > 0 ? BitmapHeight : InnerWidth + 2 * BorderWidth; }
         protected set { }
      }

      /// <summary>
      /// Wird das Polygon durch ein Bitmap beschrieben?
      /// </summary>
      public override bool WithDayBitmap { get { return XBitmapDay != null; } }

      /// <summary>
      /// Ex. ein Nacht-Bitmap?
      /// </summary>
      public override bool WithNightBitmap {
         get {
            switch (Polylinetype) {
               case PolylineType.Day2:
               case PolylineType.NoBorder_Day1:
                  return false;
            }
            return true;
         }
      }

      public Polyline()
         : this(1, 0) { }

      public Polyline(uint iTyp, uint iSubtyp)
         : base() {
         Type = iTyp;
         Subtype = iSubtyp;
         Options2 = 0;
         InnerWidth = 1;
         BorderWidth = 0;
      }

      public void Read(BinaryReaderWriter br) {
         long startpos = br.Position;
         try {
            Options = br.ReadByte();
            Options2 = br.ReadByte();

            // Farben einlesen (1 bis max. 4)
            switch (Polylinetype) {
               case PolylineType.Day2:
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  break;

               case PolylineType.Day2_Night2:
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  colNightColor = BinaryColor.ReadColorTable(br, 2);
                  break;

               case PolylineType.Day1_Night2:
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  colNightColor = BinaryColor.ReadColorTable(br, 2);
                  break;

               case PolylineType.NoBorder_Day2_Night1:
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  colNightColor[0] = BinaryColor.ReadColor(br);
                  break;

               case PolylineType.NoBorder_Day1:
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  break;

               case PolylineType.NoBorder_Day1_Night1:
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  colNightColor[0] = BinaryColor.ReadColor(br);
                  break;
            }

            if (BitmapHeight == 0) { // Linien- und Randbreite ermitteln
               switch (Polylinetype) {
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.Day1_Night2:
                     InnerWidth = br.ReadByte();
                     if (InnerWidth > 0)                 // sonst Bitmap, also KEINE BorderWidth
                        BorderWidth = (br.ReadByte() - InnerWidth) / 2;
                     break;

                  case PolylineType.NoBorder_Day2_Night1:
                  case PolylineType.NoBorder_Day1:
                  case PolylineType.NoBorder_Day1_Night1:
                     InnerWidth = br.ReadByte();         // es folgt KEINE BorderWidth
                     break;
               }
            } else     // Bitmap einlesen (Höhe ist Thickness Pixel, Breite 32 Pixel, Dummyfarbe!)
               XBitmapDay = new PixMap(32, BitmapHeight, new Color[1] { Color.White }, BitmapColorMode.POLY1TR, br);

            if (WithString)
               Text = new MultiText(br);

            if (WithExtendedOptions) {
               ExtOptions = br.ReadByte();
               switch (FontColType) {
                  case FontColours.Day:
                     colFontColour[0] = BinaryColor.ReadColor(br);
                     break;
                  case FontColours.Night:
                     colFontColour[1] = BinaryColor.ReadColor(br);
                     break;
                  case FontColours.DayAndNight:
                     colFontColour = BinaryColor.ReadColorTable(br, 2);
                     break;
               }
            }

         } catch (Exception ex) {
            throw new Exception(string.Format("Fehler beim Lesen der Linie 0x{0:x} 0x{1:x}: {2}", Type, Subtype, ex.Message));
         }
      }

      /// <summary>
      /// liefert das Bitmap (falls vorhanden)
      /// </summary>
      /// <param name="b4Day">für Tag oder Nacht</param>
      /// <param name="bExt">auch "bitmaplose" Linie als Bitmap 32 x n</param>
      /// <returns></returns>
      public override Bitmap? AsBitmap(bool b4Day, bool bExt) {
         Bitmap? bm = null;
         if (WithDayBitmap) {
            PixMap? tmp = XBitmapDay != null ? new PixMap(XBitmapDay) : null;
            // Das Bitmap hat bisher nur eine Dummyfarbe. Jetzt müßen noch die richtigen Farben gesetzt werden.
            if (b4Day) {
               switch (Polylinetype) {
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.NoBorder_Day2_Night1:
                     tmp?.SetNewColors(colDayColor);
                     break;

                  case PolylineType.Day1_Night2:
                  case PolylineType.NoBorder_Day1:
                  case PolylineType.NoBorder_Day1_Night1:
                     tmp?.SetNewColor(0, colDayColor[0]);
                     break;
               }
            } else {
               switch (Polylinetype) {
                  case PolylineType.Day2:
                  case PolylineType.NoBorder_Day1:
                     tmp = null;
                     break;

                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2_Night2:
                     tmp?.SetNewColors(colNightColor);
                     break;

                  case PolylineType.NoBorder_Day2_Night1:
                  case PolylineType.NoBorder_Day1_Night1:
                     tmp?.SetNewColor(0, colNightColor[0]);
                     break;
               }
            }
            if (tmp != null)
               bm = tmp.AsBitmap();
         } else {
            if (bExt) {
               bm = new Bitmap(32, (int)(InnerWidth + 2 * BorderWidth));
               for (int y = 0; y < bm.Height; y++) {
                  bool bBorder = y < BorderWidth || y >= (InnerWidth + BorderWidth);
                  Color col;
                  if (b4Day)
                     col = bBorder ? colDayColor[1] : colDayColor[0];
                  else
                     col = bBorder ? colNightColor[1] : colNightColor[0];
                  for (int x = 0; x < bm.Width; x++)
                     bm.SetPixel(x, y, col);
               }
            }
         }
         return bm;
      }

      /// <summary>
      /// vertauscht (wenn möglich) die Farben
      /// </summary>
      /// <param name="b4Day"></param>
      public void SwapColors(bool b4Day) {
         Color tmp;
         if (WithDayBitmap) {       // durch Bitmap definiert
            if (b4Day) {
               switch (Polylinetype) {
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.NoBorder_Day2_Night1:
                     tmp = DayColor1;
                     DayColor1 = DayColor2;
                     DayColor2 = tmp;
                     break;
                  case PolylineType.Day1_Night2:
                  case PolylineType.NoBorder_Day1:
                  case PolylineType.NoBorder_Day1_Night1:
                     XBitmapDay?.InvertBits();               // wegen Transparenz
                     break;
               }
            } else
               switch (Polylinetype) {
                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2_Night2:
                     tmp = NightColor1;
                     NightColor1 = NightColor2;
                     NightColor2 = tmp;
                     break;
                  case PolylineType.NoBorder_Day1_Night1:
                  case PolylineType.NoBorder_Day2_Night1:
                     XBitmapDay?.InvertBits();
                     break;
               }
         } else {                // kein Bitmap
            if (b4Day) {
               switch (Polylinetype) {
                  case PolylineType.Day2:
                  case PolylineType.Day2_Night2:
                  case PolylineType.NoBorder_Day2_Night1:      // eigentlich nicht möglich (?)
                     tmp = DayColor1;
                     DayColor1 = DayColor2;
                     DayColor2 = tmp;
                     break;
               }
            } else
               switch (Polylinetype) {
                  case PolylineType.Day1_Night2:
                  case PolylineType.Day2_Night2:
                     tmp = NightColor1;
                     NightColor1 = NightColor2;
                     NightColor2 = tmp;
                     break;
               }
         }
      }

      /// <summary>
      /// setzt das Bitmap und die Farben neu
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="bmday"></param>
      public void SetBitmap(PolylineType typ, Bitmap bmday) {
         if (bmday.Width != 32 || bmday.Height > 31)
            throw new Exception("Das Bitmap muß 32 breit sein und darf höchstens 31 hoch sein.");
         if (Polylinetype != typ)
            Polylinetype = typ;
         switch (Polylinetype) {
            case PolylineType.Day1_Night2:
            case PolylineType.NoBorder_Day1:
            case PolylineType.NoBorder_Day1_Night1:
               XBitmapDay = new PixMap(bmday, BitmapColorMode.POLY1TR);
               break;
            case PolylineType.Day2:
            case PolylineType.Day2_Night2:
            case PolylineType.NoBorder_Day2_Night1:
               XBitmapDay = new PixMap(bmday, BitmapColorMode.POLY2);
               break;
         }
         BitmapHeight = (uint)bmday.Height;
         if (XBitmapDay != null) {
            DayColor1 = XBitmapDay.GetColor(0);
            switch (typ) {
               case PolylineType.Day2:
               case PolylineType.Day2_Night2:
               case PolylineType.NoBorder_Day2_Night1:
                  DayColor2 = XBitmapDay.Colors > 1 ? XBitmapDay.GetColor(1) : Color.CornflowerBlue;
                  break;
            }
            switch (typ) {
               case PolylineType.NoBorder_Day1_Night1:
               case PolylineType.NoBorder_Day2_Night1:
                  NightColor1 = DayColor1;
                  break;
               case PolylineType.Day1_Night2:
               case PolylineType.Day2_Night2:
                  base.NightColor1 = DayColor1;
                  base.NightColor2 = DayColor2;
                  break;
            }
         }
      }

      /// <summary>
      /// setzt die Werte für eine Darstellung ohne Bitmap
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="innerwidth"></param>
      /// <param name="borderwidth"></param>
      /// <param name="day1"></param>
      /// <param name="day2"></param>
      /// <param name="night1"></param>
      /// <param name="night2"></param>
      public void SetWidthAndColors(PolylineType typ, uint innerwidth, uint borderwidth,
                                    Color day1, Color day2, Color night1, Color night2) {
         //if (borderwidth == 0 &&
         //    (typ == PolylineType.Day2 ||
         //     typ == PolylineType.Day2_Night2 ||
         //     typ == PolylineType.Day1_Night2))
         //   throw new Exception("Falscher Linientyp bei Randbreite 0.");
         if (borderwidth > 0 &&
             (typ == PolylineType.NoBorder_Day1 ||             // NoBorder-Typen sollten wahrscheinlich gar nicht ohne Bitmap verwendet werden
              typ == PolylineType.NoBorder_Day1_Night1 ||
              typ == PolylineType.NoBorder_Day2_Night1))
            throw new Exception("Falscher Linientyp bei Randbreite > 0.");
         XBitmapDay = null;
         XBitmapNight = null;
         if (Polylinetype != typ)
            Polylinetype = typ;
         InnerWidth = innerwidth;
         BorderWidth = borderwidth;
         BitmapHeight = 0;
         DayColor1 = day1;
         DayColor2 = day2;
         NightColor1 = night1;
         NightColor2 = night2;
      }

      public void SetWidthAndColors(PolylineType typ, uint innerwidth, uint borderwidth,
                                    Color day1, Color day2) {
         SetWidthAndColors(typ, innerwidth, borderwidth, day1, day2, NightColor1, NightColor2);
      }

      public void SetWidthAndColors(PolylineType typ, uint innerwidth, uint borderwidth) {
         SetWidthAndColors(typ, innerwidth, borderwidth, DayColor1, DayColor2, NightColor1, NightColor2);
      }

      public void SetWidthAndColors(PolylineType typ, uint innerwidth) {
         SetWidthAndColors(typ, innerwidth, BorderWidth, DayColor1, DayColor2, NightColor1, NightColor2);
      }

      public void Write(BinaryReaderWriter bw, int iCodepage) {
         bw.Write(Options);
         bw.Write(Options2);
         switch (Polylinetype) {
            case PolylineType.Day2:
               BinaryColor.WriteColorTable(bw, colDayColor);
               break;

            case PolylineType.Day2_Night2:
               BinaryColor.WriteColorTable(bw, colDayColor);
               BinaryColor.WriteColorTable(bw, colNightColor);
               break;

            case PolylineType.Day1_Night2:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               BinaryColor.WriteColorTable(bw, colNightColor);
               break;

            case PolylineType.NoBorder_Day2_Night1:
               BinaryColor.WriteColorTable(bw, colDayColor);
               BinaryColor.WriteColor(bw, colNightColor[0]);
               break;

            case PolylineType.NoBorder_Day1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               break;

            case PolylineType.NoBorder_Day1_Night1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               BinaryColor.WriteColor(bw, colNightColor[0]);
               break;
         }

         if (BitmapHeight == 0) { // Linien- und Randbreite ermitteln
            switch (Polylinetype) {
               case PolylineType.Day2:
               case PolylineType.Day2_Night2:
               case PolylineType.Day1_Night2:
                  bw.Write((byte)InnerWidth);
                  if (InnerWidth > 0)
                     bw.Write((byte)(2 * BorderWidth + InnerWidth));
                  break;

               case PolylineType.NoBorder_Day2_Night1:
               case PolylineType.NoBorder_Day1:
               case PolylineType.NoBorder_Day1_Night1:
                  bw.Write((byte)InnerWidth);
                  break;
            }
         } else
            XBitmapDay?.WriteRawdata(bw);

         if (WithString)
            Text.Write(bw, iCodepage);

         if (WithExtendedOptions) {    // es folgen weitere (max. 2) Farben
            bw.Write(ExtOptions);
            switch (FontColType) {
               case FontColours.Day:
                  BinaryColor.WriteColor(bw, colFontColour[0]);
                  break;
               case FontColours.Night:
                  BinaryColor.WriteColor(bw, colFontColour[1]);
                  break;
               case FontColours.DayAndNight:
                  BinaryColor.WriteColorTable(bw, colFontColour);
                  break;
            }
         }

      }

      /// <summary>
      /// liefert (bis auf den Typ) eine Kopie
      /// </summary>
      /// <param name="iTyp"></param>
      /// <param name="iSubtyp"></param>
      /// <returns></returns>
      public Polyline GetCopy(uint iTyp, uint iSubtyp) {
         Polyline n = (Polyline)MemberwiseClone();
         CopyExtData(n);
         n.Type = iTyp;
         n.Subtype = iSubtyp;
         return n;
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("Polyline=[Typ=0x");
         sb.Append(Type.ToString("x2"));
         if (Type >= 0x100)
            sb.Append(Subtype.ToString("x2"));
         sb.Append(" " + Polylinetype.ToString() + " ");
         switch (Polylinetype) {
            case PolylineType.Day2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(" " + colDayColor[1].ToString());
               break;
            case PolylineType.Day2_Night2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(" " + colDayColor[1].ToString());
               sb.Append(" " + colNightColor[0].ToString());
               sb.Append(" " + colNightColor[1].ToString());
               break;
            case PolylineType.Day1_Night2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(" " + colNightColor[0].ToString());
               sb.Append(" " + colNightColor[1].ToString());
               break;
            case PolylineType.NoBorder_Day2_Night1:
               sb.Append(colDayColor[0].ToString());
               sb.Append(" " + colDayColor[1].ToString());
               sb.Append(" " + colNightColor[0].ToString());
               break;
            case PolylineType.NoBorder_Day1:
               sb.Append(colDayColor[0].ToString());
               break;
            case PolylineType.NoBorder_Day1_Night1:
               sb.Append(colDayColor[0].ToString());
               sb.Append(" " + colNightColor[0].ToString());
               break;
         }
         if (BitmapHeight > 0)
            sb.Append(" " + XBitmapDay?.ToString());
         if (InnerWidth > 0) {
            sb.Append(" Width=" + InnerWidth.ToString());
            if (BorderWidth > 0)
               sb.Append(" Border=" + BorderWidth.ToString());
         }
         if (FontType != Fontdata.Default)
            sb.Append(" Fonttyp=[" + FontType.ToString() + "]");
         if (FontColType != FontColours.No) {
            sb.Append(" CustomColours=[");
            switch (FontColType) {
               case FontColours.Day:
                  sb.Append("Day=" + colFontColour[0].ToString());
                  break;
               case FontColours.Night:
                  sb.Append("Night=" + colFontColour[1].ToString());
                  break;
               case FontColours.DayAndNight:
                  sb.Append("Day=" + colFontColour[0].ToString());
                  sb.Append(" Night=" + colFontColour[0].ToString());
                  break;
            }
            sb.Append("]");
         }
         if (WithString)
            sb.Append(" " + Text.ToString());
         sb.Append(" ]");
         return sb.ToString();
      }

   }

}
