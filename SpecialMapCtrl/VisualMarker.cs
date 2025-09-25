using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl.ToolTips;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpecialMapCtrl {

   /// <summary>
   /// Erweiterung der <see cref="MapMarker"/> um Gpx-Daten und grafische Daten
   /// </summary>
   public class VisualMarker : MapMarker {

      /// <summary>
      /// Zuordnung von Bitmap und Bitmap-Offset für die geograf. Pos. für einen Marker
      /// </summary>
      public class MarkerPicture : IDisposable {

         bool ownbitmap = false;

         public Bitmap Picture { get; private set; }

         public Point Offset { get; private set; }

         public MarkerPicture(int offsetx, int offsety, Bitmap bm) {
            Picture = bm;
            Offset = new Point(offsetx, offsety);
         }

         public MarkerPicture(int offsetx, int offsety, byte[] bmdata) :
            this(offsetx, offsety, new Bitmap(new System.IO.MemoryStream(bmdata))) {
         }

         public MarkerPicture(MarkerPicture mp, double zoom) {
            if (zoom != 1 && zoom > 0) {
               Picture = new Bitmap((int)(zoom * mp.Picture.Width), (int)(zoom * mp.Picture.Height));
               Graphics g = Graphics.FromImage(Picture);
               g.DrawImage(mp.Picture, 0, 0, Picture.Width, Picture.Height);
               g.Flush();
               g.Dispose();
               ownbitmap = true;
               Offset = new Point((int)(Math.Round(mp.Offset.X * zoom)), (int)(Math.Round(mp.Offset.Y * zoom)));
            } else {
               Offset = mp.Offset;
               Picture = (Bitmap)mp.Picture.Clone();
            }
         }

         #region Implementierung der IDisposable-Schnittstelle

         /// <summary>
         /// true, wenn schon ein Dispose() erfolgte
         /// </summary>
         private bool _isdisposed = false;

         /// <summary>
         /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
         /// </summary>
         public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
         }

         /// <summary>
         /// überschreibt die Standard-Methode
         /// <para></para>
         /// </summary>
         /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
         protected virtual void Dispose(bool notfromfinalizer) {
            if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
               if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
                  if (ownbitmap)
                     Picture.Dispose();
               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion

      }

      public enum VisualStyle {
         /// <summary>
         /// noch nicht festgelegt
         /// </summary>
         notdefined,
         /// <summary>
         /// für Foto
         /// </summary>
         FotoMarker,
         /// <summary>
         /// Standard
         /// </summary>
         StandardMarker,
         /// <summary>
         /// Standard, editierbar
         /// </summary>
         StandardEditableMarker,
         /// <summary>
         /// Punkt
         /// </summary>
         PointMarker,
         /// <summary>
         /// für GeoTagging
         /// </summary>
         GeoTagging,
      }


      static Dictionary<string, MarkerPicture>? ExternSymbols = null;

      // Die Standard-Marker sind 24x24 Bitmaps.
      // Ohne besondere Vorgabe wird der Bezugspunkt in die Mitte des Bildes gelegt.

#if GMAP4SKIA
      public readonly static MarkerPicture FotoMarker = new MarkerPicture(-12, -12, Skia.Properties.Resources.Foto);
      public readonly static MarkerPicture FlagBlueMarker = new MarkerPicture(-7, -22, Skia.Properties.Resources.FlagBlue);
      public readonly static MarkerPicture FlagGreenMarker = new MarkerPicture(-7, -22, Skia.Properties.Resources.FlagGreen);
      public readonly static MarkerPicture PointMarker = new MarkerPicture(-2, -2, Skia.Properties.Resources.Point1);
      public readonly static MarkerPicture GeoTaggingMarker = new MarkerPicture(-12, -12, Skia.Properties.Resources.GeoTagging);
#else
      public readonly static MarkerPicture FotoMarker = new MarkerPicture(-12, -12, Properties.Resources.Foto);
      public readonly static MarkerPicture FlagBlueMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagBlue);
      //readonly static MarkerPicture FlagRedMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagRed);
      public readonly static MarkerPicture FlagGreenMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagGreen);
      //readonly static MarkerPicture PinBlueMarker = new MarkerPicture(0, -24, Properties.Resources.PinBlue);
      //readonly static MarkerPicture PinRedMarker = new MarkerPicture(0, -24, Properties.Resources.PinRed);
      //readonly static MarkerPicture PinGreenMarker = new MarkerPicture(0, -24, Properties.Resources.PinGreen);
      public readonly static MarkerPicture PointMarker = new MarkerPicture(-2, -2, Properties.Resources.Point1);
      public readonly static MarkerPicture GeoTaggingMarker = new MarkerPicture(-12, -12, Properties.Resources.GeoTagging);
#endif

      static Color colShadow = Color.FromArgb(180, 0, 0, 0);

      object lockobj = new object();

      Bitmap? _bitmap;

      public Bitmap? Bitmap {
         get => _bitmap;
         set => _bitmap = value;
      }

      Bitmap? _bitmapShadow;

      int shadowdeltapix = 0;

      /// <summary>
      /// Gpx-Daten
      /// </summary>
      public Marker? RealMarker { get; private set; } = null;

      /// <summary>
      /// akt. Darstellung
      /// </summary>
      public VisualStyle Visualstyle { get; private set; } = VisualStyle.notdefined;


      /// <summary>
      /// erzeugt den <see cref="VisualMarker"/> zum <see cref="Marker"/>
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="name"></param>
      /// <param name="style"></param>
      /// <param name="symbolname"></param>
      /// <param name="symbolzoom">zur Symbolvergrößerung</param>
      /// <param name="shadowdeltapix">wenn ungleich 0 wird ein Schatten erzeugt</param>
      public VisualMarker(Marker marker,
                          string name,
                          VisualStyle style,
                          string symbolname,
                          double symbolzoom = 1.0,
                          int shadowdeltapix = 3) :
         base(new GMap.NET.PointLatLng(marker.Waypoint.Lat, marker.Waypoint.Lon)) {
         MarkerPicture? mp = markerPicture4Style(style, symbolname, symbolzoom);
         if (mp != null) {
            Bitmap = mp.Picture;
            ActiveClientSize = new Size(Bitmap.Width, Bitmap.Height);
            LocalOffset = mp.Offset;
            ToolTipText = name;
            RealMarker = marker;
            IsHitTestVisible = true;
            IsVisible = true;
            Visualstyle = style;
            this.shadowdeltapix = shadowdeltapix;
            _bitmapShadow = createShadowBitmap(Bitmap, shadowdeltapix);

            if (style != VisualStyle.FotoMarker) {
               ToolTip = new MapMarkerToolTip(this,
                                               Bitmap.Height * (float)symbolzoom *
#if GMAP4SKIA
                                                                                   0.015F);
#else
                                                                                0.05F);
#endif
               ToolTipText = RealMarker.Text;
               //ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.OnMouseOver;
               ToolTipMode = MarkerTooltipMode.Always;
            }
         }
      }

      #region static

      static MarkerPicture? markerPicture4Style(VisualStyle style, string? symbolname = null, double symbolzoom = 1.0) {
         MarkerPicture? mp = null;
         switch (style) {
            case VisualStyle.StandardMarker:
               if (string.IsNullOrEmpty(symbolname) ||
                   ExternSymbols == null ||
                   !ExternSymbols.TryGetValue(symbolname, out mp))
                  mp = FlagBlueMarker;
               break;

            case VisualStyle.StandardEditableMarker:
               if (string.IsNullOrEmpty(symbolname) ||
                   ExternSymbols == null ||
                   !ExternSymbols.TryGetValue(symbolname, out mp))
                  mp = FlagGreenMarker;
               break;

            case VisualStyle.PointMarker: mp = PointMarker; break;
            case VisualStyle.FotoMarker: mp = FotoMarker; break;
            case VisualStyle.GeoTagging: mp = GeoTaggingMarker; break;
         }

         if (mp != null) {
            if (symbolzoom != 1 && symbolzoom > 0)
               return new MarkerPicture(mp, symbolzoom);
         }
         return mp;
      }

      /// <summary>
      /// liefert das Bitmap für diesen Stil (i.A. nur für symbolname sinnvoll)
      /// </summary>
      /// <param name="style"></param>
      /// <param name="symbolname"></param>
      /// <param name="symbolzoom"></param>
      /// <returns></returns>
      public static Bitmap? Bitmap4Style(VisualStyle style, string? symbolname = null, double symbolzoom = 1.0) {
         MarkerPicture? mp = markerPicture4Style(style, symbolname, symbolzoom);
         if (mp != null)
            return mp.Picture;
         return null; ;
      }

      public static void RegisterExternSymbols(IList<GarminSymbol> symbols) {
         ExternSymbols = new Dictionary<string, MarkerPicture>();
         foreach (var item in symbols) {
            ExternSymbols.Add(item.Name, new MarkerPicture(item.OffsetX, item.OffsetY, item.Bitmap));
            //ExternSymbols.Add(item.Name, new MarkerPicture(-item.Bitmap.Width / 2, -item.Bitmap.Height / 2, item.Bitmap));
         }
      }

      static Bitmap? createShadowBitmap(Bitmap bm, int shadowdeltapix) {
         if (shadowdeltapix != 0) {
            Bitmap shbm = (Bitmap)bm.Clone();
            for (int y = 0; y < shbm.Height; y++)
               for (int x = 0; x < shbm.Width; x++)
                  shbm.SetPixel(x, y, shbm.GetPixel(x, y).A > 100 ?
                     colShadow :
                     Color.Transparent);
            return shbm;
         }
         return null;
      }

      #endregion

      /// <summary>
      /// (Pseudo-)Refresh der Anzeige
      /// </summary>
      public void Refresh() {
         if (IsVisible) {
            IsVisible = false;
            IsVisible = true;
         }
      }

      public override void OnRender(Graphics g) {
         if (IsOnClientVisible)
            lock (lockobj) {
               if (_bitmapShadow != null)
                  g.DrawImage(_bitmapShadow,
                              ActiveClientPosition.X + shadowdeltapix,
                              ActiveClientPosition.Y + shadowdeltapix,
                              ActiveClientSize.Width,
                              ActiveClientSize.Height);
               if (Bitmap != null)
                  g.DrawImage(Bitmap,
                              ActiveClientPosition.X,
                              ActiveClientPosition.Y,
                              ActiveClientSize.Width,
                              ActiveClientSize.Height);
            }
      }

      public override string ToString() {
         return string.Format("Visualstyle={0}, GpxMarkerData=[{2}]", Visualstyle, RealMarker);
      }

      #region Implementierung der IDisposable-Schnittstelle

      bool _isdisposed = false;

      protected override void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {
            if (notfromfinalizer) { // nur dann alle managed Ressourcen freigeben
               _bitmapShadow?.Dispose();
               _bitmap?.Dispose();
            }

            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;
            base.Dispose(notfromfinalizer);
         }
      }

      #endregion
   }
}
