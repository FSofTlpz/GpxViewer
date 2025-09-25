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
using System.Drawing;
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// alle notwendigen Daten für ein Polygon
   /// </summary>
   public class Polygone : GraphicElement {

      public enum ColorType {
         /// <summary>
         /// 1 solid colour
         /// </summary>
         Day1 = 0x6,
         /// <summary>
         /// 1 solid daycolour / 1 solid nightcolour
         /// </summary>
         Day1_Night1 = 0x7,
         /// <summary>
         /// 2 colour bitmap
         /// </summary>
         BM_Day2 = 0x8,
         /// <summary>
         /// 2 solid daycolours / 2 solid nightcolours
         /// </summary>
         BM_Day2_Night2 = 0x9,
         /// <summary>
         /// 1 solid daycolour + 1 transparent colour / 2 solid nightcolours
         /// </summary>
         BM_Day1_Night2 = 0xB,
         /// <summary>
         /// 2 colour bitmap / 1 solid nightcolour + 1 transparent colour
         /// </summary>
         BM_Day2_Night1 = 0xD,
         /// <summary>
         /// 1 colour 1 transparent colour bitmap
         /// </summary>
         BM_Day1 = 0xE,
         /// <summary>
         /// 1 solid daycolour + 1 transparent colour / 1 solid nightcolour + 1 transparent colour
         /// </summary>
         BM_Day1_Night1 = 0xF
      }

      /// <summary>
      /// Zeichenebene (>=1)
      /// </summary>
      public uint Draworder { get; set; }

      /// <summary>
      /// mit Text (Bit 4)
      /// </summary>
      public override bool WithString {
         get {
            return BitIsSet(Options, 4);
         }
         protected set {
            Options = SetBit(Options, 4, value);
         }
      }
      /// <summary>
      /// erweiterte Optionen (Bit 5)
      /// </summary>
      public override bool WithExtendedOptions {
         get {
            return BitIsSet(Options, 5);
         }
         protected set {
            Options = SetBit(Options, 5, value);
         }
      }
      /// <summary>
      /// Polygontyp (Bit 0..4)
      /// </summary>
      public ColorType Colortype {
         get { return (ColorType)(Options & 0x0F); }
         set {
            Options = (byte)((Options & 0xf0) | (int)value);
            // XPixMap erzeugen oder verwerfen
            switch (Colortype) {
               case ColorType.Day1:                             // keine Bitmaps, nur Solid-Farbe
               case ColorType.Day1_Night1:
                  if (XBitmapDay != null)
                     XBitmapDay = null;
                  if (XBitmapNight != null)
                     XBitmapNight = null;
                  break;
               case ColorType.BM_Day1:
               case ColorType.BM_Day2:
                  XBitmapDay = GetDummyXPixMap(Colortype == ColorType.BM_Day1 ?
                                                         BitmapColorMode.POLY1TR :
                                                         BitmapColorMode.POLY2,
                                               true,
                                               XBitmapDay);
                  if (XBitmapNight != null)
                     XBitmapNight = null;
                  break;
               case ColorType.BM_Day1_Night1:
               case ColorType.BM_Day1_Night2:
               case ColorType.BM_Day2_Night1:
               case ColorType.BM_Day2_Night2:
                  XBitmapDay = GetDummyXPixMap((Colortype == ColorType.BM_Day1_Night1 || Colortype == ColorType.BM_Day1_Night2) ?
                                                   BitmapColorMode.POLY1TR :
                                                   BitmapColorMode.POLY2,
                                                   true,
                                                   XBitmapDay);
                  XBitmapNight = GetDummyXPixMap((Colortype == ColorType.BM_Day1_Night1 || Colortype == ColorType.BM_Day2_Night1) ?
                                                   BitmapColorMode.POLY1TR :
                                                   BitmapColorMode.POLY2,
                                                   false,
                                                   XBitmapNight);
                  break;
            }
            if (XBitmapDay != null)
               if (XBitmapDay.Colors == 1)
                  DayColor2 = PixMap.TransparentColor;
               else
                  if (DayColor2 == PixMap.TransparentColor)
                  DayColor2 = Color.MediumVioletRed;              // irgendetwas, nur eben nicht transparent
            if (XBitmapNight != null)
               if (XBitmapNight.Colors == 1)
                  NightColor2 = PixMap.TransparentColor;
               else
                  if (NightColor2 == PixMap.TransparentColor)
                  NightColor2 = Color.MediumVioletRed;           // irgendetwas, nur eben nicht transparent
         }
      }
      /// <summary>
      /// unbekannte Bits 5, 6, 7
      /// </summary>
      public byte UnknownFlags {
         get { return (byte)(Options & 0xE0); }
      }

      /// <summary>
      /// Breite des Polygons (konstant)
      /// </summary>
      public override uint Width { get { return 32; } protected set { } }

      /// <summary>
      /// Höhe des Polygons (konstant)
      /// </summary>
      public override uint Height { get { return 32; } protected set { } }

      /// <summary>
      /// Wird das Polygon durch ein Bitmap beschrieben?
      /// </summary>
      public override bool WithDayBitmap { get { return XBitmapDay != null; } }

      /// <summary>
      /// Ex. ein Nacht-Bitmap?
      /// </summary>
      public override bool WithNightBitmap { get { return XBitmapNight != null; } }

      public Polygone()
         : this(1, 0) { }

      public Polygone(uint iTyp, uint iSubtyp)
         : base() {
         Type = iTyp;
         Subtype = iSubtyp;
         Draworder = 1;
         Colortype = ColorType.Day1;
      }

      public void Read(BinaryReaderWriter br) {
         try {
            Options = br.ReadByte();

            // es folgen 1 bis max. 4 Farben und ev. 1 Bitmap
            switch (Colortype) {  // hier muss sicher noch überprüft werden, ob die Typen wirklich richtig interpretiert werden
               case ColorType.Day1:
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  break;

               case ColorType.Day1_Night1:
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  colNightColor[0] = BinaryColor.ReadColor(br);
                  break;

               case ColorType.BM_Day2:           // 2 Farben + 1x Pixeldaten
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY2, br);
                  break;

               case ColorType.BM_Day2_Night2:    // 4 Farben + 1x Pixeldaten
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  colNightColor = BinaryColor.ReadColorTable(br, 2);
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY2, br);
                  XBitmapNight = new PixMap(XBitmapDay);
                  XBitmapNight.SetNewColors(colNightColor);
                  break;

               case ColorType.BM_Day1_Night2:    // 3 Farben + 1x Pixeldaten
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  colDayColor[1] = PixMap.TransparentColor;
                  colNightColor = BinaryColor.ReadColorTable(br, 2);
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY2, br);
                  XBitmapNight = new PixMap(XBitmapDay);
                  XBitmapNight.ChangeColorMode(BitmapColorMode.POLY2);
                  XBitmapNight.SetNewColors(colNightColor);
                  break;

               case ColorType.BM_Day2_Night1:    // 3 Farben + 1x Pixeldaten
                  colDayColor = BinaryColor.ReadColorTable(br, 2);
                  colNightColor[0] = BinaryColor.ReadColor(br);
                  colNightColor[1] = PixMap.TransparentColor;
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY2, br);
                  XBitmapNight = new PixMap(XBitmapDay);
                  XBitmapNight.ChangeColorMode(BitmapColorMode.POLY1TR);
                  XBitmapNight.SetNewColors(colNightColor);
                  break;

               case ColorType.BM_Day1:           // 1 Farbe + 1x Pixeldaten
                  colDayColor[0] = colNightColor[0] = BinaryColor.ReadColor(br);
                  colDayColor[1] = colNightColor[1] = PixMap.TransparentColor;
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY1TR, br);
                  break;

               case ColorType.BM_Day1_Night1:    // 2 Farben + 1x Pixeldaten
                  colDayColor[0] = BinaryColor.ReadColor(br);
                  colDayColor[1] = PixMap.TransparentColor;
                  colNightColor[0] = BinaryColor.ReadColor(br);
                  colNightColor[1] = PixMap.TransparentColor;
                  XBitmapDay = new PixMap(32, 32, colDayColor, BitmapColorMode.POLY1TR, br);
                  XBitmapNight = new PixMap(XBitmapDay);
                  XBitmapNight.SetNewColors(colNightColor);
                  break;
            }
            if (WithDayBitmap && XBitmapDay != null) {
               // sicherheitshalber nochmal die Bitmapfarben übernehmen
               DayColor1 = XBitmapDay.GetColor(0);
               if (XBitmapDay.Colors > 1)
                  DayColor2 = XBitmapDay.GetColor(1);
               if (XBitmapNight != null) {
                  NightColor1 = XBitmapNight.GetColor(0);
                  if (XBitmapNight.Colors > 1)
                     NightColor2 = XBitmapNight.GetColor(1);
               }
            }

            if (WithString)
               Text = new MultiText(br);

            if (WithExtendedOptions) {    // es folgen weitere (max. 2) Farben
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
            throw new Exception(string.Format("Fehler beim Lesen des Polygons 0x{0:x} 0x{1:x}: {2}", Type, Subtype, ex.Message));
         }
      }

      public void Write(BinaryReaderWriter bw, int iCodepage) {
         bw.Write(Options);

         switch (Colortype) {
            case ColorType.Day1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               break;

            case ColorType.Day1_Night1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               BinaryColor.WriteColor(bw, colNightColor[0]);
               break;

            case ColorType.BM_Day2:
               XBitmapDay?.WriteColorTable(bw);
               XBitmapDay?.WriteRawdata(bw);
               break;

            case ColorType.BM_Day2_Night2:
               BinaryColor.WriteColorTable(bw, colDayColor);
               BinaryColor.WriteColorTable(bw, colNightColor);
               XBitmapDay?.WriteRawdata(bw);
               break;

            case ColorType.BM_Day1_Night2:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               BinaryColor.WriteColorTable(bw, colNightColor);
               XBitmapDay?.WriteRawdata(bw);
               break;

            case ColorType.BM_Day2_Night1:
               BinaryColor.WriteColorTable(bw, colDayColor);
               BinaryColor.WriteColor(bw, colNightColor[0]);
               XBitmapDay?.WriteRawdata(bw);
               break;

            case ColorType.BM_Day1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               XBitmapDay?.WriteRawdata(bw);
               break;

            case ColorType.BM_Day1_Night1:
               BinaryColor.WriteColor(bw, colDayColor[0]);
               BinaryColor.WriteColor(bw, colNightColor[0]);
               XBitmapDay?.WriteRawdata(bw);
               break;
         }
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

      private PixMap GetDummyXPixMap(BitmapColorMode bcm, bool b4Day, PixMap? old) {
         if (old != null) {
            if (old.Colormode != bcm) {
               switch (old.Colormode) {
                  case BitmapColorMode.POLY1TR:
                     old.ChangeColorMode(BitmapColorMode.POLY2);
                     old.InvertBits();
                     old.SetNewColors([old.GetColor(0), Color.White]);        // 2. Farbe einfach Weiß
                     break;

                  case BitmapColorMode.POLY2:
                     old.ChangeColorMode(BitmapColorMode.POLY1TR);
                     old.InvertBits();
                     old.SetNewColors([old.GetColor(0)]);
                     break;
               }
            }
            return old;
         }

         PixMap pic = new PixMap(32, 32, 2, bcm);
         if (b4Day) {
            pic.SetNewColor(0, DayColor1);
            if (bcm == BitmapColorMode.POLY2) pic.SetNewColor(1, DayColor2);
         } else {
            pic.SetNewColor(0, NightColor1);
            if (bcm == BitmapColorMode.POLY2) pic.SetNewColor(1, NightColor2);
         }
         return pic;
      }

      /// <summary>
      /// liefert das Bitmap (falls vorhanden)
      /// </summary>
      /// <param name="b4Day">für Tag oder Nacht</param>
      /// <param name="bExt">auch "bitmaplose" Polygone als Bitmap 32 x 32</param>
      /// <returns></returns>
      public override Bitmap AsBitmap(bool b4Day, bool bExt) {
         if (b4Day) {
            if (XBitmapDay != null)
               return XBitmapDay.AsBitmap();
            else
               return GetDummyXPixMap(BitmapColorMode.POLY1TR, true, null).AsBitmap(); ;
         } else {
            if (XBitmapNight != null)
               return XBitmapNight.AsBitmap();
            else
               return GetDummyXPixMap(BitmapColorMode.POLY1TR, false, null).AsBitmap(); ;
         }
      }

      /// <summary>
      /// setzt der oder die 32x32-Bitmaps für die Darstellung
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="bmday"></param>
      /// <param name="bmnight"></param>
      public void SetBitmaps(ColorType typ, Bitmap bmday, Bitmap? bmnight = null) {
         if (bmday == null)
            throw new Exception("Kein Bitmap angegeben.");
         if (typ == ColorType.Day1 ||
             typ == ColorType.Day1_Night1)
            throw new Exception("Falscher Typ für eine Darstellung mit Bitmap.");
         if (bmday.Width != 32 || bmday.Height != 32)
            throw new Exception("Das Bitmap muß 32x32 groß sein.");
         if (Colortype != typ)
            Colortype = typ;
         switch (typ) {
            case ColorType.BM_Day1:
            case ColorType.BM_Day1_Night1:
            case ColorType.BM_Day1_Night2:
               XBitmapDay = new PixMap(bmday, BitmapColorMode.POLY1TR);
               DayColor1 = XBitmapDay.GetColor(0);
               break;
            case ColorType.BM_Day2:
            case ColorType.BM_Day2_Night1:
            case ColorType.BM_Day2_Night2:
               XBitmapDay = new PixMap(bmday, BitmapColorMode.POLY2);
               DayColor1 = XBitmapDay.GetColor(0);
               DayColor2 = XBitmapDay.GetColor(1);
               break;
         }

         if (bmnight != null) {
            if (bmday.Width != bmnight.Width || bmday.Height != bmnight.Height)
               throw new Exception("Beide Bitmaps müßen die gleiche Größe haben.");
            switch (typ) {
               case ColorType.BM_Day1_Night1:
               case ColorType.BM_Day2_Night1:
                  XBitmapNight = new PixMap(bmnight, BitmapColorMode.POLY1TR);
                  NightColor1 = XBitmapNight.GetColor(0);
                  break;
               case ColorType.BM_Day1_Night2:
               case ColorType.BM_Day2_Night2:
                  XBitmapNight = new PixMap(bmnight, BitmapColorMode.POLY1TR);
                  NightColor1 = XBitmapNight.GetColor(0);
                  NightColor2 = XBitmapNight.GetColor(1);
                  break;
            }
         } else {
            XBitmapNight = null;
         }
      }

      /// <summary>
      /// setzt die Farben für die Darstellung ohne Bitmap
      /// </summary>
      /// <param name="day1"></param>
      /// <param name="night1"></param>
      public void SetSolidColors(Color day1, Color night1) {
         XBitmapDay = null;
         XBitmapNight = null;
         Colortype = ColorType.Day1_Night1;
         DayColor1 = day1;
         NightColor1 = night1;
      }

      /// <summary>
      /// setzt die Farbe für die Darstellung ohne Bitmap
      /// </summary>
      /// <param name="day1"></param>
      public void SetSolidColor(Color day1) {
         XBitmapDay = null;
         XBitmapNight = null;
         Colortype = ColorType.Day1;
         DayColor1 = day1;
      }

      /// <summary>
      /// vertauscht (wenn möglich) die Farben bzw. die Transparenz
      /// </summary>
      /// <param name="b4Day"></param>
      public void SwapColors(bool b4Day) {
         Color tmp;
         if (b4Day) {
            switch (Colortype) {
               case Polygone.ColorType.Day1:
               case Polygone.ColorType.Day1_Night1:
                  break;
               case Polygone.ColorType.BM_Day1:
               case Polygone.ColorType.BM_Day1_Night2:
               case Polygone.ColorType.BM_Day1_Night1:
                  XBitmapDay?.InvertBits();
                  break;
               case Polygone.ColorType.BM_Day2_Night1:
               case Polygone.ColorType.BM_Day2:
               case Polygone.ColorType.BM_Day2_Night2:
                  tmp = DayColor1;
                  DayColor1 = DayColor2;
                  DayColor2 = tmp;
                  break;
            }
         } else
            switch (Colortype) {
               case Polygone.ColorType.Day1:
               case Polygone.ColorType.BM_Day1:
               case Polygone.ColorType.BM_Day2:
               case Polygone.ColorType.Day1_Night1:
                  break;
               case Polygone.ColorType.BM_Day1_Night1:
               case Polygone.ColorType.BM_Day2_Night1:
                  XBitmapNight?.InvertBits();
                  break;
               case Polygone.ColorType.BM_Day1_Night2:
               case Polygone.ColorType.BM_Day2_Night2:
                  tmp = DayColor1;
                  DayColor1 = DayColor2;
                  DayColor2 = tmp;
                  break;
            }
      }

      /// <summary>
      /// liefert (bis auf den Typ) eine Kopie
      /// </summary>
      /// <param name="iTyp"></param>
      /// <param name="iSubtyp"></param>
      /// <returns></returns>
      public Polygone GetCopy(uint iTyp, uint iSubtyp) {
         Polygone n = (Polygone)MemberwiseClone();
         CopyExtData(n);
         n.Type = iTyp;
         n.Subtype = iSubtyp;
         return n;
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("Polygone=[Typ=0x");
         sb.Append(Type.ToString("x2"));
         if (Type >= 0x100)
            sb.Append(Subtype.ToString("x2"));
         sb.Append(" " + Colortype.ToString() + " ");
         switch (Colortype) {
            case ColorType.Day1:
               sb.Append(colDayColor.ToString());
               break;
            case ColorType.Day1_Night1:
               sb.Append(colDayColor.ToString());
               sb.Append(colNightColor.ToString());
               break;
            case ColorType.BM_Day1_Night1:
               sb.Append(colDayColor[0].ToString());
               sb.Append(colNightColor[0].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
            case ColorType.BM_Day1_Night2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(colNightColor[0].ToString());
               sb.Append(colNightColor[1].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
            case ColorType.BM_Day2_Night2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(colDayColor[1].ToString());
               sb.Append(colNightColor[0].ToString());
               sb.Append(colNightColor[1].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
            case ColorType.BM_Day2:
               sb.Append(colDayColor[0].ToString());
               sb.Append(colDayColor[1].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
            case ColorType.BM_Day2_Night1:
               sb.Append(colDayColor[0].ToString());
               sb.Append(colDayColor[1].ToString());
               sb.Append(colNightColor[0].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
            case ColorType.BM_Day1:
               sb.Append(colDayColor[0].ToString());
               sb.Append(XBitmapDay?.ToString());
               break;
         }
         if (WithExtendedOptions)
            sb.Append(" Options=[" + FontType.ToString() + "|" + FontColType.ToString() + "]");
         if (WithString)
            sb.Append(" " + Text.ToString());
         sb.Append(" ]");
         return sb.ToString();
      }

   }

}
