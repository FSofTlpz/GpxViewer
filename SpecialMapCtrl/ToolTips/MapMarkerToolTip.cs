using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpecialMapCtrl.ToolTips {

   public class MapMarkerToolTip : MapToolTip {

      static readonly Pen penOutline;
      static readonly StringFormat sf;
      public static readonly Font SpecFont;
      public static readonly Brush SpecForeground;

      static float fontFactor = 1;

      static MapMarkerToolTip() {
         penOutline = new Pen(Color.LightYellow, 3) {      // Stift für die Umrandung der Schrift (bessere Lesbarkeit)
            LineJoin = LineJoin.Round                 // damit u.a. die M's nicht spitz sind
         };

         sf = new StringFormat() {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Far,
         };

         SpecFont = new Font(FontFamily.GenericSansSerif, DefaultFont.Size, FontStyle.Italic, GraphicsUnit.Pixel);
         SpecForeground = new SolidBrush(Color.Black);
      }

      public MapMarkerToolTip(MapMarker marker, float fontfactor = 1)
          : base(marker) {
         fontFactor = fontfactor;
      }

      public override void OnRender(Graphics canvas) {
         if (Marker != null &&
             Marker.IsOnClientVisible) {
            Point pt = Marker.LocalToolTipPosition;
            canvas.SmoothingMode = SmoothingMode.HighQuality;

            GraphicsPath path = new GraphicsPath();
            path.AddString(Marker.ToolTipText,
#if !GMAP4SKIA
                           SpecFont.FontFamily,
#else
                           SpecFont.FontFamilyname,
#endif
                           (int)SpecFont.Style,
#if !GMAP4SKIA
                           fontFactor * canvas.DpiY * SpecFont.SizeInPoints / 72,       // point -> em size
#else
                           fontFactor * SpecFont.SizeInPoints,
#endif
                           new Point(pt.X, pt.Y),
                           sf);
            canvas.DrawPath(penOutline, path);
            canvas.FillPath(SpecForeground, path);
            path.Dispose();
         }
      }

   }
}
