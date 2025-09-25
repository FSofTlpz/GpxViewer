using GMap.NET;
using System.Drawing;

namespace SpecialMapCtrl {

   /// <summary>
   /// Marker ohne Symbol, aber mit Textausgabe
   /// </summary>
   public class MarkerText : MapMarker {

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
      public MarkerText(PointLatLng txtpt, string txt) : base(txtpt) {
         Text = txt;
         StringFormat = new StringFormat {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
         };
         IsHitTestVisible = false;
      }

      public override void OnRender(Graphics g) {
         if (IsVisible &&
             !string.IsNullOrEmpty(Text)&&
             IsOnClientVisible) {

               g.DrawString(Text,
                            Font,
                            Brush,
                            ActiveClientPosition.X + LocalOffset.X,
                            ActiveClientPosition.Y + LocalOffset.Y,
                            StringFormat);

         }
      }

      public override void Dispose() {
         base.Dispose();
      }

      public override string ToString() {
         return string.Format("{{Position={0}, ActiveLocalPosition={1}, \"{2}\"}}", Position, ActiveClientPosition, Text);
      }
   }
}
