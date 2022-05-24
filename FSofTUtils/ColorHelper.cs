using System;
using System.Drawing;

namespace FSofTUtils {
   public class ColorHelper {

      // http://csharphelper.com/blog/2016/08/convert-between-rgb-and-hls-color-models-in-c/

      /*

          Hue           determines the color with a 0 to 360 degree direction on a color wheel.
                                                    H°    S%    L%       R%    G%	 B%
                              Rot                    0   100    50      100     0     0
                              Zinnober              15   100    50      100    25     0
                              Braun                 20    60  22,5       36    18     9
                              Orange                30   100    50      100    50     0
                              Safran                45   100    50      100    75     0
                              Gelb                  60   100    50      100   100     0
                              Leichtes Grüngelb     75   100    50       75   100     0
                              Grüngelb              90   100    50       50   100     0
                              Limett               105   100    50       25   100     0
                              Dunkelgrün           120   100    25        0    50     0
                              Grün                 120   100    50        0   100     0
                              Leichtes Blaugrün    135   100    50        0   100    25
                              Blaugrün             150   100    50        0   100    50
                              Grüncyan             165   100    50        0   100    75
                              Cyan                 180   100    50        0   100   100
                              Blaucyan             195   100    50        0    75   100
                              Grünblau             210   100    50        0    50   100
                              Leichtes Grünblau    225   100    50        0    25   100
                              Blau                 240   100    50        0     0   100
                              Indigo               255   100    50       25     0   100
                              Violett              270   100    50       50     0   100
                              Blaumagenta          285   100    50       75     0   100
                              Magenta              300   100    50      100     0   100
                              Rotmagenta           315   100    50      100     0    75
                              Blaurot              330   100    50      100     0    50
                              Leichtes Blaurot     345   100    50      100     0    25
                              Schwarz                –     –     0        0     0     0
                              Weiß                   –     –   100      100   100   100
          Lightness     indicates how much light is in the color. 
                        When lightness = 0, the color is black. 
                        When lightness = 0.5, the color is as “pure” as possible.
                        When lightness = 1, the color is white. 
          Saturation    indicates the amount of color added. 
                        You can think of this as the opposite of “grayness.” 
                        When saturation = 0, the color is pure gray. 
                        In this case, if lightness = 0.5 you get a neutral color. 
                        When saturation is 1, the color is “pure.” 


      */


      /// <summary>
      /// Convert an RGB value into an HLS value.
      /// </summary>
      /// <param name="r"></param>
      /// <param name="g"></param>
      /// <param name="b"></param>
      /// <param name="h"></param>
      /// <param name="l"></param>
      /// <param name="s"></param>
      public static void RgbToHls(int r, int g, int b, out double h, out double l, out double s) {
         // Convert RGB to a 0.0 to 1.0 range.
         double double_r = r / 255.0;
         double double_g = g / 255.0;
         double double_b = b / 255.0;

         // Get the maximum and minimum RGB components.
         double max = double_r;
         if (max < double_g)
            max = double_g;
         if (max < double_b)
            max = double_b;

         double min = double_r;
         if (min > double_g)
            min = double_g;
         if (min > double_b)
            min = double_b;

         double diff = max - min;
         l = (max + min) / 2;
         if (Math.Abs(diff) < 0.00001) {
            s = 0;
            h = 0;  // H is really undefined.
         } else {
            if (l <= 0.5)
               s = diff / (max + min);
            else
               s = diff / (2 - max - min);

            double r_dist = (max - double_r) / diff;
            double g_dist = (max - double_g) / diff;
            double b_dist = (max - double_b) / diff;

            if (double_r == max)
               h = b_dist - g_dist;
            else if (double_g == max)
               h = 2 + r_dist - b_dist;
            else
               h = 4 + g_dist - r_dist;

            h = h * 60;
            if (h < 0)
               h += 360;
         }
      }

      public static void RgbToHls(Color col, out double h, out double l, out double s) {
         RgbToHls(col.R, col.G, col.B, out h, out s, out l);
      }

      /// <summary>
      /// Convert an HLS value into an RGB value.
      /// </summary>
      /// <param name="h"></param>
      /// <param name="l"></param>
      /// <param name="s"></param>
      /// <param name="r"></param>
      /// <param name="g"></param>
      /// <param name="b"></param>
      public static void HlsToRgb(double h, double l, double s, out int r, out int g, out int b) {
         double p2;
         if (l <= 0.5)
            p2 = l * (1 + s);
         else
            p2 = l + s - l * s;

         double p1 = 2 * l - p2;
         double double_r, double_g, double_b;
         if (s == 0) {
            double_r = l;
            double_g = l;
            double_b = l;
         } else {
            double_r = qqhToRgb(p1, p2, h + 120);
            double_g = qqhToRgb(p1, p2, h);
            double_b = qqhToRgb(p1, p2, h - 120);
         }

         // Convert RGB to the 0 to 255 range.
         r = (int)(double_r * 255.0);
         g = (int)(double_g * 255.0);
         b = (int)(double_b * 255.0);
      }

      /// <summary>
      /// Convert an HLS value into an <see cref="System.Drawing.Color"/>
      /// </summary>
      /// <param name="h"></param>
      /// <param name="l"></param>
      /// <param name="s"></param>
      /// <returns></returns>
      public static Color HlsToRgb(double h, double l, double s) {
         HlsToRgb(h, l, s, out int r, out int g, out int b);
         return System.Drawing.Color.FromArgb(r, g, b);
      }

      private static double qqhToRgb(double q1, double q2, double hue) {
         if (hue > 360)
            hue -= 360;
         else if (hue < 0)
            hue += 360;

         if (hue < 60)
            return q1 + (q2 - q1) * hue / 60;
         if (hue < 180)
            return q2;
         if (hue < 240)
            return q1 + (q2 - q1) * (240 - hue) / 60;
         return q1;
      }

      /// <summary>
      /// liefert die BGR-Farbe als int
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      public static int Bgr(Color col) {
         return col.B << 16 | col.G << 8 | col.R;
      }



   }
}
