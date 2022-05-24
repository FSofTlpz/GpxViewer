using System;
using System.Drawing;

namespace Unclassified.UI {
   public struct HslColor {
      public byte H { get; set; }
      public byte S { get; set; }
      public byte L { get; set; }
      public byte A { get; set; }

      public static HslColor RgbToHsl(Color rgb) {
         // Translated from JavaScript, part of coati
         double h, s, l;
         double r = (double)rgb.R / 255;
         double g = (double)rgb.G / 255;
         double b = (double)rgb.B / 255;
         double min = Math.Min(Math.Min(r, g), b);
         double max = Math.Max(Math.Max(r, g), b);

         l = (max + min) / 2;

         if (max == min)
            h = 0;
         else if (max == r)
            h = (60 * (g - b) / (max - min)) % 360;
         else if (max == g)
            h = (60 * (b - r) / (max - min) + 120) % 360;
         else //if (max == b)
            h = (60 * (r - g) / (max - min) + 240) % 360;
         if (h < 0)
            h += 360;

         if (max == min)
            s = 0;
         else if (l <= 0.5)
            s = (max - min) / (2 * l);
         else
            s = (max - min) / (2 - 2 * l);

         return new HslColor((byte)Math.Round((h / 360 * 256) % 256), (byte)Math.Round(s * 255), (byte)Math.Round(l * 255), rgb.A);
      }

      public static Color HslToRgb(HslColor hsl) {
         // Translated from JavaScript, part of coati
         double h = (double)hsl.H / 256;
         double s = (double)hsl.S / 255;
         double l = (double)hsl.L / 255;
         double q;
         if (l < 0.5)
            q = l * (1 + s);
         else
            q = l + s - l * s;
         double p = 2 * l - q;
         double[] t = new double[] { h + 1.0 / 3, h, h - 1.0 / 3 };
         byte[] rgb = new byte[3];
         for (int i = 0; i < 3; i++) {
            if (t[i] < 0) t[i]++;
            if (t[i] > 1) t[i]--;
            if (t[i] < 1.0 / 6)
               rgb[i] = (byte)Math.Round((p + ((q - p) * 6 * t[i])) * 255);
            else if (t[i] < 1.0 / 2)
               rgb[i] = (byte)Math.Round(q * 255);
            else if (t[i] < 2.0 / 3)
               rgb[i] = (byte)Math.Round((p + ((q - p) * 6 * (2.0 / 3 - t[i]))) * 255);
            else
               rgb[i] = (byte)Math.Round(p * 255);
         }
         return Color.FromArgb(hsl.A, rgb[0], rgb[1], rgb[2]);
      }


      public HslColor(byte h, byte s, byte l, byte a = 255) {
         H = h;
         S = s;
         L = l;
         A = a;
      }

      public HslColor(HslColor col) {
         H = col.H;
         S = col.S;
         L = col.L;
         A = col.A;
      }

      public HslColor(Color col) {
         HslColor hslColor = RgbToHsl(col);
         H = hslColor.H;
         S = hslColor.S;
         L = hslColor.L;
         A = hslColor.A;
      }

      public Color ToRgb() {
         return HslToRgb(this);
      }

   }
}
