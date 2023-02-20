using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace GMap.NET.CoreExt.WindowsForms.Markers {

   /// <summary>
   /// Marker ohne Symbol, aber mit Textausgabe
   /// </summary>
   public class GMarkerText : GMapMarker {

      public static readonly Font DefaultFont = new Font("Arial", 20);
      public static readonly SolidBrush DefaultBrush = new SolidBrush(Color.Black);

      /// <summary>
      /// anzuzeigender Text
      /// </summary>
      public string Text;
      /// <summary>
      /// Font für die Darstellung
      /// </summary>
      public Font Font = DefaultFont;
      /// <summary>
      /// Brush für die Darstellung
      /// </summary>
      public Brush Brush = DefaultBrush;

      /// <summary>
      /// Ausgabeformat
      /// </summary>
      public StringFormat StringFormat;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtpt">geografischer Bezugspunkt</param>
      /// <param name="txt">Text</param>
      public GMarkerText(PointLatLng txtpt, string txt) : base(txtpt) {
         Text = txt;
         StringFormat = new StringFormat {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
         };
         IsHitTestVisible = false;
      }

      public override void OnRender(Graphics g) {
         if (IsVisible &&
             !string.IsNullOrEmpty(Text)) {

            g.DrawString(Text, 
                         Font, 
                         Brush, 
                         LocalPosition.X + LocalOffset.X, 
                         LocalPosition.Y + LocalOffset.Y,
                         StringFormat);

         }
      }

      public override void Dispose() {
         base.Dispose();
      }

      public override string ToString() {
         return string.Format("{{Position={0}, LocalPosition={1}, \"{2}\"}}", Position, LocalPosition, Text);
      }
   }
}
