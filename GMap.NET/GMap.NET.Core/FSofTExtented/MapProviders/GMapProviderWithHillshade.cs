using FSofTUtils.Drawing;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace GMap.NET.FSofTExtented.MapProviders {

   /// <summary>
   /// allgemeiner Provider um ein zusätzliches Hillshading "innerhalb" (also z.B. zwischen bestimmten Layern) zu ermöglichen
   /// <para>(z.Z. nur für <see cref="GarminProvider"/> sinnvoll))</para>
   /// </summary>
   abstract public class GMapProviderWithHillshade : MultiUseBaseProvider {

      FSofTUtils.Geography.DEM.DemData _dem;

      /// <summary>
      /// setzt oder liefert threadsicher das DEM-Verwaltungsobjekt
      /// </summary>
      public FSofTUtils.Geography.DEM.DemData DEM {
         get => Interlocked.Exchange(ref _dem, _dem);
         set => Interlocked.Exchange(ref _dem, value);
      }

      int _alpha = 100;

      /// <summary>
      /// setzt oder liefert threadsicher den Alpha-Wert für das Hillshading
      /// </summary>
      public int Alpha {
         get => Interlocked.Exchange(ref _alpha, _alpha);
         set => Interlocked.Exchange(ref _alpha, (value & 0xFF));
      }


      /// <summary>
      /// zeichnet das Hillshading über die Karte
      /// </summary>
      /// <param name="dem"></param>
      /// <param name="bm"></param>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="right"></param>
      /// <param name="top"></param>
      /// <param name="alpha"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      static protected Task drawHillshadeAsync(FSofTUtils.Geography.DEM.DemData dem,
                                               Bitmap bm,
                                               double left,
                                               double bottom,
                                               double right,
                                               double top,
                                               int alpha,
                                               CancellationToken? cancellationToken) {
         Task t = Task.Run(() => {
            drawHillshade(dem, bm, left, bottom, right, top, alpha, cancellationToken);
         });
         return t;
      }

      /// <summary>
      /// zeichnet das Hillshading über die Karte
      /// </summary>
      /// <param name="dem"></param>
      /// <param name="bm"></param>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="right"></param>
      /// <param name="top"></param>
      /// <param name="alpha"></param>
      static protected void drawHillshade(FSofTUtils.Geography.DEM.DemData dem,
                                          Bitmap bm,
                                          double left,
                                          double bottom,
                                          double right,
                                          double top,
                                          int alpha,
                                          CancellationToken? cancellationToken) {
         // Shadingarray: Die niedrigen Werte sollten dunkel, die hohen hell dargestellt werden.
         byte[] shadings = dem.GetShadingValueArray(left, bottom, right, top, bm.Width, bm.Height, cancellationToken);
         if (shadings != null) {
            uint[] pixel = new uint[bm.Width * bm.Height];
            for (int i = 0; i < shadings.Length; i++)
               pixel[i] = getShadingColor4ShadingValueV2(shadings[i], alpha);

            using (Bitmap bmhs = BitmapHelper.CreateBitmap32(bm.Width, bm.Height, pixel)) {
               using (Graphics canvas = Graphics.FromImage(bm)) {
                  canvas.DrawImage(bmhs, 0, 0);
               }
            }
         }
      }

      /// <summary>
      /// erzeugt aus dem Shadingwert eine Shadingfarbe
      /// <para>
      /// Variante 1: Aus den Shadingwerten wird jeweils eine Graustufe erzeugt (für 0 Schwarz, für 255 Weiss)
      /// </para>
      /// </summary>
      /// <param name="value"></param>
      /// <param name="basealpha"></param>
      /// <returns></returns>
      static uint getShadingColor4ShadingValueV1(byte value, int basealpha) =>
         BitmapHelper.GetUInt4Color(basealpha, value, value, value);

      /// <summary>
      /// erzeugt aus dem Shadingwert eine Shadingfarbe
      /// <para>
      /// Variante 2: Aus den Shadingwerten wird jeweils Schwarz mit einem entsprechenden Alpha erzeugt (für 0 alpha=basealpha, für 255 alpha=0)
      /// </para>
      /// </summary>
      /// <param name="value"></param>
      /// <param name="basealpha"></param>
      /// <returns></returns>
      static uint getShadingColor4ShadingValueV2(byte value, int basealpha) {
         // Die niedrigen Werte sollten dunkel, die hohen hell dargestellt werden.
         // ==> 0 -> alpha = 255; 255 -> alpha = 0
         // eingegrenzt auf 0 .. basealpha: ==> 0 -> basealpha; 255 -> 0

         //double f = value / 255.0;
         //int delta = (int)Math.Round(f * basealpha);
         //basealpha -= delta;

         basealpha -= (int)Math.Round(value / 255.0 * basealpha);
         return BitmapHelper.GetUInt4Color(basealpha, 0, 0, 0);
      }

   }
}
