using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using GMap.NET.WindowsForms;

namespace GMap.NET.CoreExt.WindowsForms.ToolTips {

   [Serializable]
   public class GMapMarkerToolTip : GMapToolTip, ISerializable {

      static readonly Pen penOutline;
      static readonly StringFormat sf;
      public static readonly Font SpecFont;
      public static readonly Brush SpecForeground;


      static GMapMarkerToolTip() {
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

      public GMapMarkerToolTip(GMapMarker marker)
          : base(marker) {
      }

      public override void OnRender(Graphics canvas) {
         canvas.SmoothingMode = SmoothingMode.HighQuality;

         GraphicsPath path = new GraphicsPath();
         path.AddString(Marker.ToolTipText,
                        SpecFont.FontFamily,
                        (int)SpecFont.Style,
                        canvas.DpiY * SpecFont.SizeInPoints / 72,       // point -> em size
                        new Point(Marker.LocalToolTipPosition.X,
                                  Marker.LocalToolTipPosition.Y),
                        sf);
         canvas.DrawPath(penOutline, path);
         canvas.FillPath(SpecForeground, path);
         path.Dispose();
      }

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
         base.GetObjectData(info, context);
      }

      protected GMapMarkerToolTip(SerializationInfo info, StreamingContext context)
          : base(info, context) {
      }

      #endregion
   }
}
