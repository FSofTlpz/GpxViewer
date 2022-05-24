// Copyright (c) 2012, Yves Goergen, http://unclassified.software/source/colormath
//
// Copying and distribution of this file, with or without modification, are permitted provided the
// copyright notice and this notice are preserved. This file is offered as-is, without any warranty.

using System;
using System.Drawing;

namespace Unclassified.Drawing {
   public static class ColorMath {
      /// <summary>
      /// Blends two colors in the specified ratio.
      /// </summary>
      /// <param name="color1">First color.</param>
      /// <param name="color2">Second color.</param>
      /// <param name="ratio">Ratio between both colors. 0 for first color, 1 for second color.</param>
      /// <returns></returns>
      public static Color Blend(Color color1, Color color2, double ratio) {
         int a = (int)Math.Round(color1.A * (1 - ratio) + color2.A * ratio);
         int r = (int)Math.Round(color1.R * (1 - ratio) + color2.R * ratio);
         int g = (int)Math.Round(color1.G * (1 - ratio) + color2.G * ratio);
         int b = (int)Math.Round(color1.B * (1 - ratio) + color2.B * ratio);
         return Color.FromArgb(a, r, g, b);
      }

      public static Color Darken(Color color, double ratio) {
         return Blend(color, Color.Black, ratio);
      }

      public static Color Lighten(Color color, double ratio) {
         return Blend(color, Color.White, ratio);
      }

      /// <summary>
      /// Computes the real modulus value, not the division remainder.
      /// This differs from the % operator only for negative numbers.
      /// </summary>
      /// <param name="dividend">Dividend.</param>
      /// <param name="divisor">Divisor.</param>
      /// <returns></returns>
      private static int Mod(int dividend, int divisor) {
         if (divisor <= 0) throw new ArgumentOutOfRangeException("divisor", "The divisor cannot be zero or negative.");
         int i = dividend % divisor;
         if (i < 0) i += divisor;
         return i;
      }

      /// <summary>
      /// Computes the grey value value of a color.
      /// </summary>
      /// <param name="c"></param>
      /// <returns></returns>
      public static byte ToGray(Color c) {
         return (byte)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
      }

      /// <summary>
      /// Determines whether the color is dark or light.
      /// </summary>
      /// <param name="c"></param>
      /// <returns></returns>
      public static bool IsDarkColor(Color c) {
         return ToGray(c) < 0x90;
      }
   }

}
