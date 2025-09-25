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
   /// alle notwendigen Daten für einen POI (max. Größe 32 x 32, max. 255 Farben)
   /// </summary>
   public class POI : GraphicElement {
      /// <summary>
      /// Bitmapbreite (nur über das Bitmap setzbar)
      /// </summary>
      public override uint Width {
         get {
            return XBitmapDay != null ? XBitmapDay.Width : 0;
         }
         protected set { // bleibt wirkungslos
         }
      }

      /// <summary>
      /// Bitmaphöhe (nur über das Bitmap setzbar)
      /// </summary>
      public override uint Height {
         get {
            return XBitmapDay != null ? XBitmapDay.Height : 0;
         }
         protected set { // bleibt wirkungslos
         }
      }

      /// <summary>
      /// das (ev. vorhandene) Nacht-Bitmap hat eigene Daten (Bit 0)
      /// </summary>
      public bool NightXpmHasData {
         get {
            return BitIsSet(Options, 0);
         }
         private set {
            Options = SetBit(Options, 0, value);
         }
      }

      /// <summary>
      /// mit Nacht-Bitmap (Bit 1)
      /// </summary>
      public bool WithNightXpm {
         get {
            return BitIsSet(Options, 1);
         }
         private set {
            Options = SetBit(Options, 1, value);
         }
      }

      /// <summary>
      /// mit Text (Bit 2)
      /// </summary>
      public override bool WithString {
         get {
            return nonvirtual_WithString;
         }
         protected set {
            nonvirtual_WithString = value;
         }
      }

      /// <summary>
      /// zur Verwendung im Konstruktor
      /// </summary>
      protected bool nonvirtual_WithString {
         get {
            return BitIsSet(Options, 2);
         }
         set {
            Options = SetBit(Options, 2, value);
         }
      }

      /// <summary>
      /// mit zusätzlichen Farben oder Fonteigenschaft (Bit 2)
      /// </summary>
      public override bool WithExtendedOptions {
         get {
            return nonvirtual_WithExtendedOptions;
         }
         protected set {
            nonvirtual_WithExtendedOptions = value;
         }
      }

      /// <summary>
      /// zur Verwendung im Konstruktor
      /// </summary>
      protected bool nonvirtual_WithExtendedOptions {
         get {
            return BitIsSet(Options, 3);
         }
         set {
            Options = SetBit(Options, 3, value);
         }
      }

      /// <summary>
      /// Farbanzahl für das Bitmap
      /// </summary>
      public byte colsday { get; private set; }

      /// <summary>
      /// Farbanzahl für das Bitmap
      /// </summary>
      public byte colsnight { get; private set; }

      /// <summary>
      /// immer true
      /// </summary>
      public override bool WithDayBitmap { get { return true; } }

      /// <summary>
      /// Ex. ein Nacht-Bitmap?
      /// </summary>
      public override bool WithNightBitmap { get { return WithNightXpm; } }

      private BitmapColorMode _ColormodeD;
      private BitmapColorMode _ColormodeN;

      /// <summary>
      /// Farbmodus des Tages-Bitmaps
      /// </summary>
      public BitmapColorMode ColormodeDay {
         get {
            return _ColormodeD;
         }
         private set {
            if (!(value == BitmapColorMode.POI_SIMPLE ||
                  value == BitmapColorMode.POI_TR ||
                  value == BitmapColorMode.POI_ALPHA))
               throw new Exception("Falscher BitmapColorMode.");
            _ColormodeD = value;
         }
      }

      /// <summary>
      /// Farbmodus des Nacht-Bitmaps
      /// </summary>
      public BitmapColorMode ColormodeNight {
         get { return _ColormodeN; }
         private set {
            if (!(value == BitmapColorMode.POI_SIMPLE ||
                  value == BitmapColorMode.POI_TR ||
                  value == BitmapColorMode.POI_ALPHA))
               throw new Exception("Falscher BitmapColorMode.");
            _ColormodeN = value;
         }
      }

      public POI(uint iTyp, uint iSubtyp)
         : base() {
         Type = iTyp;
         Subtype = iSubtyp;
         NightXpmHasData = true;
         WithNightXpm = false;
         nonvirtual_WithString = false;
         nonvirtual_WithString = false;
         XBitmapDay = new PixMap(16, 16, 2, BitmapColorMode.POI_ALPHA);
         XBitmapNight = new PixMap(XBitmapDay);
         ColormodeDay = XBitmapDay.Colormode;
      }

      public POI(uint iTyp, uint iSubtyp, BitmapColorMode cm)
         : this(iTyp, iSubtyp) {
         ColormodeDay = cm;
      }

      public void Read(BinaryReaderWriter br) {
         try {
            Options = br.ReadByte();
            uint iWidth = br.ReadByte();
            uint iHeight = br.ReadByte();
            colsday = br.ReadByte();
            ColormodeDay = (BitmapColorMode)br.ReadByte();
            this.XBitmapDay = new PixMap(iWidth, iHeight, colsday, ColormodeDay, br);
            if (WithNightXpm) {
               colsnight = br.ReadByte();
               ColormodeNight = (BitmapColorMode)br.ReadByte();
               if (!NightXpmHasData) {
                  Color[] col = BinaryColor.ReadColorTable(br, colsnight);
                  XBitmapNight = new PixMap(XBitmapDay);
                  XBitmapNight.SetNewColors(col);
               } else
                  XBitmapNight = new PixMap(Width, Height, colsnight, ColormodeNight, br);
            }
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
            throw new Exception(string.Format("Fehler beim Lesen des Punktes 0x{0:x} 0x{1:x}: {2}", Type, Subtype, ex.Message));
         }
      }

      public void Write(BinaryReaderWriter bw, int iCodepage) {
         bw.Write(Options);
         XBitmapDay?.WriteAsPoi(bw);
         if (WithNightXpm)
            XBitmapNight?.WriteAsPoi(bw);
         if (WithString)
            Text.Write(bw, iCodepage);
         if (WithExtendedOptions) {
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

      // nur um die Sichtbarkeit nach außen zu verhindern, da in dieser Klasse völlig unnötig
      protected new Color DayColor1;
      protected new Color DayColor2;
      protected new Color NightColor1;
      protected new Color NightColor2;

      /// <summary>
      /// liefert das Bitmap
      /// </summary>
      /// <param name="b4Day">für Tag oder Nacht</param>
      /// <returns></returns>
      public Bitmap? AsBitmap(bool b4Day) {
         if (b4Day && XBitmapDay != null) {
            PixMap tmp = new PixMap(XBitmapDay);
            if (tmp.Colors > 0)
               tmp.InvertBits();
            return tmp.AsBitmap();
         }
         if (!b4Day && XBitmapNight != null) {
            PixMap tmp = new PixMap(XBitmapNight);
            if (tmp.Colors > 0)
               tmp.InvertBits();
            return tmp.AsBitmap();
         }
         return null;
      }

      public override Bitmap? AsBitmap(bool b4Day, bool bExt) {
         return AsBitmap(b4Day);
      }

      /// <summary>
      /// setzt die Bitmaps (falls nicht null) mit dem einfachstmöglichen BitmapColorMode
      /// </summary>
      /// <param name="bmday"></param>
      /// <param name="bmnight"></param>
      /// <param name="bOnlyGarminColors">Die jeweils nächstgelegene Garminfarbe verwenden?</param>
      public void SetBitmaps(Bitmap? bmday, Bitmap? bmnight, bool bOnlyGarminColors) {
         if (bmday == null)
            throw new Exception("Wenigstens das Tag-Bitmap muß gesetzt werden.");
         if (bmnight != null)
            if (bmday.Width != bmnight.Width || bmday.Height != bmnight.Height)
               throw new Exception("Beide Bitmaps müßen die gleiche Größe haben.");
         if ((bmday.Width > 255 || bmday.Height > 255) ||
             (bmnight != null && (bmnight.Width > 255 || bmnight.Height > 255)))
            throw new Exception("Das Bitmap darf höchstens 255x255 groß sein.");
         BitmapColorMode cm = GetMinBitmapColorMode(GetMinBitmapColorMode(bmday), GetMinBitmapColorMode(bmnight));
         ColormodeDay = cm;
         if (bmday != null) {      // sonst bleibt das alte bestehen
            XBitmapDay = new PixMap(bmday, ColormodeDay);
            if (bOnlyGarminColors) {
               for (uint i = 0; i < XBitmapDay.Colors; i++)
                  XBitmapDay.SetNewColor(i, GraphicElement.GetNearestGarminColor(XBitmapDay.GetColor(i)));
            }
         }
         if (bmnight != null) {
            XBitmapNight = new PixMap(bmnight, ColormodeDay);
            WithNightXpm = true;
            if (bOnlyGarminColors) {
               for (uint i = 0; i < XBitmapNight.Colors; i++)
                  XBitmapNight.SetNewColor(i, GraphicElement.GetNearestGarminColor(XBitmapNight.GetColor(i)));
            }
         } else {
            XBitmapNight = null;
            WithNightXpm = false;
         }
         Width = XBitmapDay != null ? XBitmapDay.Width : 0;
         Height = XBitmapDay != null ? XBitmapDay.Height : 0;
      }

      /// <summary>
      /// liefert den einfachstmöglichen BitmapColorMode
      /// </summary>
      /// <param name="bm"></param>
      /// <returns></returns>
      protected BitmapColorMode GetMinBitmapColorMode(Bitmap? bm) {
         return bm != null ?
                     PixMap.AnalyseBitmap(bm, true, out Color[]? coltab, out bool bWithTransp, out bool bWithAlpha) :
                     BitmapColorMode.unknown;
      }
      /// <summary>
      /// liefert den min. möglichen Mode für 2 vorgegebene Modes
      /// </summary>
      /// <param name="cm1"></param>
      /// <param name="cm2"></param>
      /// <returns></returns>
      protected BitmapColorMode GetMinBitmapColorMode(BitmapColorMode cm1, BitmapColorMode? cm2) {
         if (cm2 != null)
            switch (cm2) {
               case BitmapColorMode.POI_SIMPLE:
                  return cm1;
               case BitmapColorMode.POI_TR:
                  switch (cm1) {
                     case BitmapColorMode.POI_SIMPLE:
                        return (BitmapColorMode)cm2;
                     case BitmapColorMode.POI_TR:
                     case BitmapColorMode.POI_ALPHA:
                        return cm1;
                  }
                  break;
               default:
                  return BitmapColorMode.POI_ALPHA;
            }
         return BitmapColorMode.POI_ALPHA;
      }

      /// <summary>
      /// liefert eine Kopie mit einem neuen Typ
      /// </summary>
      /// <param name="iTyp"></param>
      /// <param name="iSubtyp"></param>
      /// <returns></returns>
      public POI GetCopy(uint iTyp, uint iSubtyp) {
         POI n = (POI)MemberwiseClone();
         CopyExtData(n);
         n.Type = iTyp;
         n.Subtype = iSubtyp;
         return n;
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("POI=[Typ=0x");
         sb.Append(Type.ToString("x2"));
         if (Subtype > 0x0)
            sb.Append(Subtype.ToString("x2"));
         sb.AppendFormat(" {0}x{1}", Width, Height);
         sb.Append(" ColormodeDay=" + ColormodeDay.ToString());
         sb.Append(" ColormodeNight=" + ColormodeNight.ToString());
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
