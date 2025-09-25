#if GMAP4SKIA

using GMap.NET;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpecialMapCtrl {

   public partial class SpecialMapCtrl {

      Cursor Cursor = Cursor.Current;

      /// <summary>
      /// Ersatz für den Windows Control-Font
      /// </summary>
      public Font? Font { get; protected set; }

      int _width, _height;

      /// <summary>
      /// Ruft die Höhe des Steuerelements ab (in Skia-Koordinaten)
      /// </summary>
      public new int Height {
         get => _height;
         protected set => _height = value;
      }

      /// <summary>
      /// Ruft die Breite des Steuerelements ab (in Skia-Koordinaten)
      /// </summary>
      public new int Width {
         get => _width;
         protected set => _width = value;
      }

      /// <summary>
      /// Höhe des Steuerelements (in Maui-Koordinaten)
      /// </summary>
      public double ControlHeight => base.Height;

      /// <summary>
      /// Breite des Steuerelements (in Maui-Koordinaten)
      /// </summary>
      public double ControlWidth => base.Width;

      /// <summary>
      /// Setzt den Eingabefokus auf das Steuerelement.
      /// </summary>
      public new void Focus() => base.Focus();

      /// <summary>
      /// Ruft einen Wert ab, der angibt, ob dem Steuerelement ein Handle zugeordnet ist.
      /// </summary>
      public bool IsHandleCreated => base.Width > 0 && base.Height > 0;

      /// <summary>
      /// Ruft einen Wert ab, mit dem angegeben wird, ob das Steuerelement und alle untergeordneten Steuerelemente angezeigt werden, oder legt diesen Wert fest.
      /// </summary>
      public bool Visible => IsVisible;

      /// <summary>
      /// Ruft einen Wert ab, der angibt, welche der Zusatztasten (Umschalttaste, STRG und ALT) gedrückt ist.
      /// <para>Gibt es bei Android nicht.</para>
      /// </summary>
      public System.Windows.Forms.Keys ModifierKeys => System.Windows.Forms.Keys.None;



      #region static Konvertierungen Maui <-> Skia (Control-Koordinaten)

      public static float MauiX2SkiaX(double x) => (float)(x * DeviceDisplay.MainDisplayInfo.Density);

      public static float MauiY2SkiaY(double y) => (float)(y * DeviceDisplay.MainDisplayInfo.Density);

      public static double SkiaX2MauiX(double x) => x / DeviceDisplay.MainDisplayInfo.Density;

      public static double SkiaY2MauiY(double y) => y / DeviceDisplay.MainDisplayInfo.Density;

      /// <summary>
      /// rechnet einen Maui-Punkt in einen Skia-Punkt um
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      public static SKPoint Maui2Skia(Microsoft.Maui.Graphics.Point pt) => new SKPoint(MauiX2SkiaX(pt.X), MauiY2SkiaY(pt.Y));

      public static Microsoft.Maui.Graphics.Point Maui2Skia(SKPoint pt) =>
         new Microsoft.Maui.Graphics.Point(SkiaX2MauiX(pt.X),
                                           SkiaY2MauiY(pt.Y));

      #endregion

      public async Task MapServiceStartAsync(double lon,
                                             double lat,
                                             string cachepath,
                                             int zoom = 12,
                                             ScaleModes scaleModes = ScaleModes.Fractional) {
         if (!g_IsDesignerHosted &&
             !g_ServiceIsReady) {

            if (!string.IsNullOrEmpty(cachepath)) {
               if (!Directory.Exists(cachepath))
                  Directory.CreateDirectory(cachepath);
               M_CacheLocation = cachepath;
            }
            GMaps.Instance.Mode = string.IsNullOrEmpty(cachepath) ?
                                       AccessMode.ServerOnly :
                                       AccessMode.ServerAndCache;

            await OnLoad(new EventArgs());

            M_MinZoom = 0;
            M_MaxZoom = 24;
            M_ScaleMode = scaleModes;
            await M_SetLocationAndZoomAsync(zoom, lon, lat);
         }
      }

      /// <summary>
      /// beendet den Mapservice
      /// </summary>
      public void MapServiceEnd() {
         if (g_ServiceIsReady)
            g_core.MapClose(); // Dispose
      }

      #region simulating Mouse-Action for None-Windows-System

      /// <summary>
      /// Ruft die Position des Mauszeigers in Bildschirmkoordinaten ab. (z.Z. nicht verwendet)
      /// </summary>
      static System.Drawing.Point MousePosition { get; set; }

      enum MouseAction {
         MouseDown,
         MouseUp,
         //MouseClick,
         //MouseDoubleClick,
         MouseMove,
         //MouseWheel,
      }

      /// <summary>
      /// setzt die Mauspos. als Control-Koordinaten! (i.A. im Zusammenhang mit Touch-Ereignissen)
      /// </summary>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      static void simulateMousePosition(int clientx, int clienty) => MousePosition = new Point(clientx, clienty);

      /// <summary>
      /// zum Simulieren einer Mausaktion (i.A. im Zusammenhang mit Touch-Ereignissen)
      /// </summary>
      /// <param name="action"></param>
      /// <param name="button"></param>
      /// <param name="clientx"></param>
      /// <param name="clienty"></param>
      /// <param name="delta"></param>
      async Task simulateMouseActionAsync(MouseAction action, MouseButtons button, int clientx, int clienty, int delta) =>
        await Task.Run(async () => {
           simulateMousePosition(clientx, clienty);
           MouseEventArgs ea = new MouseEventArgs(button, clientx, clienty, delta);
           switch (action) {
              case MouseAction.MouseDown: OnMouseDown(ea); break;
              case MouseAction.MouseUp: await OnMouseUp(ea); break;
              case MouseAction.MouseMove: OnMouseMove(ea); break;
           }
        });

      /// <summary>
      /// Beginn der Kartenverschiebung
      /// </summary>
      /// <param name="startpt"></param>
      public Task MapDragStart(Microsoft.Maui.Graphics.Point startpt) =>
         simulateMouseActionAsync(MouseAction.MouseDown,
                             MouseButtons.Right,
                             (int)MauiX2SkiaX(startpt.X),
                             (int)MauiY2SkiaY(startpt.Y),
                             0);

      /// <summary>
      /// Kartenverschiebung
      /// </summary>
      /// <param name="actualpt"></param>
      public Task MapDrag(Microsoft.Maui.Graphics.Point actualpt) =>
         simulateMouseActionAsync(MouseAction.MouseMove,
                             MouseButtons.Right,
                             (int)MauiX2SkiaX(actualpt.X),
                             (int)MauiY2SkiaY(actualpt.Y),
                             0);

      /// <summary>
      /// Kartenverschiebung beendet
      /// </summary>
      /// <param name="endpt"></param>
      public Task MapDragEnd(Microsoft.Maui.Graphics.Point endpt) =>
         simulateMouseActionAsync(MouseAction.MouseUp,
                             MouseButtons.Right,
                             (int)MauiX2SkiaX(endpt.X),
                             (int)MauiY2SkiaY(endpt.Y),
                             0);

      /// <summary>
      /// verschiebt die Karte (i.A. um eine größere Entfernung)
      /// </summary>
      /// <param name="deltalon"></param>
      /// <param name="deltalat"></param>
      public async Task MapMoveAsync(double deltalon, double deltalat) =>
         await M_SetLocationAsync(M_Position.Lng - deltalon,
                                  M_Position.Lat + deltalat);

      #endregion

      PointLatLng OnMouseClick(MouseEventArgs e,
                               bool all,
                               out List<MapMarker> markers,
                               out List<MapTrack> tracks,
                               out List<MapPolygon> polygons) {
         MouseClick?.Invoke(this, e);
         return onMouseClick(e,
                             false,
                             all,
                             out markers,
                             out tracks,
                             out polygons);
      }

      PointLatLng OnMouseDoubleClick(MouseEventArgs e,
                                     bool all,
                                     out List<MapMarker> markers,
                                     out List<MapTrack> tracks,
                                     out List<MapPolygon> polygons) {
         MouseDoubleClick?.Invoke(this, e);
         return onMouseClick(e,
                             true,
                             all,
                             out markers,
                             out tracks,
                             out polygons);
      }

      protected void OnMouseDown(MouseEventArgs e) {
         MouseDown?.Invoke(this, e);
         onMouseDown(e);
      }

      protected async Task OnMouseUp(MouseEventArgs e) {
         MouseUp?.Invoke(this, e);
         await onMouseUp(e);
      }

      /// <summary>
      /// zur Simulation einer Mausbewegung (auch mit ModifierKeys == Keys.Alt für Selektion!)
      /// </summary>
      /// <param name="e"></param>
      protected virtual void OnMouseMove(MouseEventArgs e) {
         MouseMove?.Invoke(this, e);
         onMouseMove(e);
      }

      protected virtual void OnSizeChanged(EventArgs e) {
         MainThread.InvokeOnMainThreadAsync(() => {
            Width = (int)Math.Round(MauiX2SkiaX(base.Width));
            Height = (int)Math.Round(MauiY2SkiaY(base.Height));
         }).Wait();
         onSizeChanged(e);
      }


      SKRect rectConvert(GRect rect) => new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

      SKRect rectConvert(Rectangle rect) => new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

   }
}
#endif
