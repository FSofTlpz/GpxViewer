using System;
using System.Collections.Generic;
using System.Drawing;

namespace FSofTUtils.Geography.Garmin {
   public class GarminTrackColors {

      /*
         <xsd:enumeration value="Black"/>
         <xsd:enumeration value="DarkRed"/>
         <xsd:enumeration value="DarkGreen"/>
         <xsd:enumeration value="DarkYellow"/>
         <xsd:enumeration value="DarkBlue"/>
         <xsd:enumeration value="DarkMagenta"/>
         <xsd:enumeration value="DarkCyan"/>
         <xsd:enumeration value="LightGray"/>
         <xsd:enumeration value="DarkGray"/>
         <xsd:enumeration value="Red"/>
         <xsd:enumeration value="Green"/>
         <xsd:enumeration value="Yellow"/>
         <xsd:enumeration value="Blue"/>
         <xsd:enumeration value="Magenta"/>
         <xsd:enumeration value="Cyan"/>
         <xsd:enumeration value="White"/>
         <xsd:enumeration value="Transparent"/>
       */

      /// <summary>
      /// Garminfarben
      /// </summary>
      public enum Colorname {
         /// <summary>
         /// unbekannt
         /// </summary>
         Unknown,
         Black,
         DarkRed,
         DarkGreen,
         DarkYellow,
         DarkBlue,
         DarkMagenta,
         DarkCyan,
         LightGray,
         DarkGray,
         Red,
         Green,
         Yellow,
         Blue,
         Magenta,
         Cyan,
         White,
         Transparent,
      }

      public readonly static Dictionary<Colorname, Color> Colors;

      static GarminTrackColors() {
         Color mycol = Color.Yellow;
         ColorHelper.RgbToHls(mycol.R, mycol.G, mycol.B, out double h, out double l, out double s);
         mycol = ColorHelper.HlsToRgb(h, l / 2, s);

         Colors = new Dictionary<Colorname, Color>() {
            { Colorname.Unknown, Color.Empty },                // #00000000   schwarz, voll transparent
            { Colorname.Black, Color.Black },                  // #FF000000
            { Colorname.DarkRed, Color.DarkRed },
            { Colorname.DarkGreen, Color.DarkGreen },
            { Colorname.DarkYellow, mycol },
            { Colorname.DarkBlue, Color.DarkBlue },
            { Colorname.DarkMagenta, Color.DarkMagenta },
            { Colorname.DarkCyan, Color.DarkCyan },
            { Colorname.LightGray, Color.LightGray },
            { Colorname.DarkGray, Color.DarkGray },
            { Colorname.Red, Color.Red },                      // #FFFF0000
            { Colorname.Green, Color.Green },
            { Colorname.Yellow, Color.Yellow },
            { Colorname.Blue, Color.Blue },
            { Colorname.Magenta, Color.Magenta },
            { Colorname.Cyan, Color.Cyan },
            { Colorname.White, Color.White },
            { Colorname.Transparent, Color.Transparent },      // #00FFFFFF   weiss, voll transparent
         };
      }

      public static string GetColorname(Colorname colname) => colname.ToString();

      public static Colorname GetColorname(string colname) {
         foreach (Colorname colorname in Enum.GetValues(typeof(Colorname))) {
            if (colorname.ToString() == colname)
               return colorname;
         }
         return Colorname.Unknown;
      }

      /// <summary>
      /// liefert den <see cref="Colorname"/> zur Farbe (ev. auch den nächstliegenden), sonst <see cref="Colorname.Unknown"/>
      /// </summary>
      /// <param name="col"></param>
      /// <param name="nearby"></param>
      /// <returns></returns>
      public static Colorname GetColorname(Color col, bool nearby) {
         foreach (Colorname colorname in Enum.GetValues(typeof(Colorname))) {
            if (Colors[colorname].ToArgb() == col.ToArgb())
               return colorname;
         }
         // nicht gefunden
         if (nearby)
            return NearestGarminColor(col);
         return Colorname.Unknown;
      }

      //public static Colorname NearestGarminColorX(Color col) {
      //   Colorname nearestcolor = GetColorname(col, false);
      //   if (nearestcolor != Colorname.Unknown)
      //      return nearestcolor;

      //   double hslabsdiff = double.MaxValue;
      //   FSofTUtils.ColorHelper.RgbToHls(col, out double h, out double s, out double l);

      //   bool gray = col.A == col.B && col.B == col.G;   // dann Weiss .. Schwarz

      //   foreach (Colorname colorname in Enum.GetValues(typeof(Colorname))) {
      //      if (colorname != Colorname.Unknown) {
      //         bool col4gray = colorname == Colorname.White ||
      //                         colorname == Colorname.LightGray ||
      //                         colorname == Colorname.DarkGray ||
      //                         colorname == Colorname.Black;
      //         if ((gray && !col4gray) ||
      //             (!gray && col4gray))
      //            continue;

      //         Color colg = Colors[colorname];
      //         FSofTUtils.ColorHelper.RgbToHls(colg.R, colg.G, colg.B, out double hg, out double sg, out double lg);

      //         double absdiff = gray ?
      //                              Math.Abs(lg - l) :
      //                              Math.Abs(hg - h);

      //         if (absdiff < hslabsdiff) {
      //            absdiff = hslabsdiff;
      //            nearestcolor = colorname;
      //         }
      //      }
      //   }
      //   return nearestcolor;
      //}

      /// <summary>
      /// liefert die nächstliegende Garminfarbe
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      public static Colorname NearestGarminColor(Color col) {
         Colorname nearestcolor = GetColorname(col, false);
         if (nearestcolor != Colorname.Unknown)
            return nearestcolor;

         double minabsdiff = double.MaxValue;

         foreach (Colorname colorname in Enum.GetValues(typeof(Colorname))) {
            if (colorname == Colorname.Unknown ||
                colorname == Colorname.Transparent)
               continue;

            double absdiff = getHSLDist(col, Colors[colorname]);

            if (minabsdiff > absdiff) {
               minabsdiff = absdiff;
               nearestcolor = colorname;
            }
         }

         return nearestcolor;
      }

      /// <summary>
      /// liefert (das Quadrat) der "eukl. Entfernung"
      /// </summary>
      /// <param name="col1"></param>
      /// <param name="col2"></param>
      /// <returns></returns>
      static double getEuklDist(Color col1, Color col2) {
         int dr = col1.R - col2.R;
         int dg = col1.G - col2.G;
         int db = col1.B - col2.B;
         return Math.Abs(dr * dr + dg * dg + db * db);
      }

      //static double getBrightness(Color c) {
      //   return (c.R * 0.299 + c.G * 0.587 + c.B * 0.114) / 256;
      //}

      //static double getHueDistance(Color col1, Color col2) {
      //   double d = Math.Abs(col1.GetHue() - col2.GetHue());
      //   return d > 180 ?
      //                  360 - d :
      //                  d;
      //}

      /// <summary>
      /// liefert (das Quadrat) der "HSL-Entfernung"
      /// </summary>
      /// <param name="col1"></param>
      /// <param name="col2"></param>
      /// <returns></returns>
      static double getHSLDist(Color col1, Color col2) {
         double dH = col1.GetHue() - col2.GetHue();
         double dS = col1.GetSaturation() - col2.GetSaturation();
         double dL = col1.GetBrightness() - col2.GetBrightness();
         return 0.8 * dH * dH +
                0.1 * dS * dS +
                0.1 * dL * dL;
      }



   }
}
