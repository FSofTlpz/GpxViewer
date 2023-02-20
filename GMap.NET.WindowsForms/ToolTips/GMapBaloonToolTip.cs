using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

#if !GMAP4SKIA
namespace GMap.NET.WindowsForms.ToolTips {
#else
namespace GMap.NET.Skia.ToolTips {
#endif
   /// <summary>
   ///     GMap.NET marker
   /// </summary>
   [Serializable]
   public class GMapBaloonToolTip : GMapToolTip, ISerializable {
      public float Radius = 10f;

      public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(140, Color.Navy));

      static GMapBaloonToolTip() {
         DefaultStroke.Width = 3;

         DefaultStroke.LineJoin = LineJoin.Round;
         DefaultStroke.StartCap = LineCap.RoundAnchor;
      }

      public GMapBaloonToolTip(GMapMarker marker)
          : base(marker) {
         Stroke = DefaultStroke;
#if !GMAP4SKIA
         Fill = Brushes.Yellow;
#endif
      }

      public override void OnRender(Graphics g) {
#if !GMAP4SKIA
         var st = g.MeasureString(Marker.ToolTipText, Font).ToSize();
         var rect = new Rectangle(Marker.LocalToolTipPosition.X,
             Marker.LocalToolTipPosition.Y - st.Height,
             st.Width + TextPadding.Width,
             st.Height + TextPadding.Height);
         rect.Offset(Offset.X, Offset.Y);

         using (var objGp = new GraphicsPath()) {
            objGp.AddLine(rect.X + 2 * Radius,
                rect.Y + rect.Height,
                rect.X + Radius,
                rect.Y + rect.Height + Radius);
            objGp.AddLine(rect.X + Radius, rect.Y + rect.Height + Radius, rect.X + Radius, rect.Y + rect.Height);

            objGp.AddArc(rect.X, rect.Y + rect.Height - Radius * 2, Radius * 2, Radius * 2, 90, 90);
            objGp.AddLine(rect.X, rect.Y + rect.Height - Radius * 2, rect.X, rect.Y + Radius);
            objGp.AddArc(rect.X, rect.Y, Radius * 2, Radius * 2, 180, 90);
            objGp.AddLine(rect.X + Radius, rect.Y, rect.X + rect.Width - Radius * 2, rect.Y);
            objGp.AddArc(rect.X + rect.Width - Radius * 2, rect.Y, Radius * 2, Radius * 2, 270, 90);
            objGp.AddLine(rect.X + rect.Width,
                rect.Y + Radius,
                rect.X + rect.Width,
                rect.Y + rect.Height - Radius * 2);
            objGp.AddArc(rect.X + rect.Width - Radius * 2,
                rect.Y + rect.Height - Radius * 2,
                Radius * 2,
                Radius * 2,
                0,
                90); // Corner

            objGp.CloseFigure();

            g.FillPath(Fill, objGp);
            g.DrawPath(Stroke, objGp);
         }

         g.DrawString(Marker.ToolTipText, Font, Foreground, rect, Format);
#endif
      }

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
         info.AddValue("Radius", Radius);

         base.GetObjectData(info, context);
      }

      protected GMapBaloonToolTip(SerializationInfo info, StreamingContext context)
          : base(info, context) {
         Radius = Extensions.GetStruct(info, "Radius", 10f);
      }

      #endregion
   }
}
