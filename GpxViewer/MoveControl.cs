using System.Drawing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class MoveControl : UserControl {

      public enum Direction {
         Up,
         Down,
         Left,
         Right
      }


      public class DirectionEventArgs {

         public Direction Direction;

         public DirectionEventArgs(Direction direction) {
            Direction = direction;
         }

      }

      public delegate void DirectionEventHandler(object sender, DirectionEventArgs e);

      public event DirectionEventHandler DirectionEvent;



      Color colLight1 = Color.FromArgb(200, 200, 200);
      Color colDark1 = Color.FromArgb(120, 120, 120);
      Color colDark2 = Color.FromArgb(120, 120, 120);

      float d = 3.0F;
      float d2 = 3.0F;


      public MoveControl() {
         InitializeComponent();
      }

      private void MoveControl_Load(object sender, System.EventArgs e) {
         colLight1 = NewColor(BackColor, 0.6);
         colDark1 = NewColor(BackColor, -0.3);
      }

      private void MoveControl_Paint(object sender, PaintEventArgs e) {
         Brush brushLight1 = new SolidBrush(colLight1);
         Brush brushDark1 = new SolidBrush(colDark1);

         float w = ClientRectangle.Width;
         float h = ClientRectangle.Height;

         e.Graphics.FillPolygon(brushLight1,
                                new PointF[] {
                                   new PointF(0, h),
                                   new PointF(d, h - d),
                                   new PointF(d, d),
                                   new PointF(w - d, d),
                                   new PointF(w, 0),
                                   new PointF(0, 0),
                                });

         e.Graphics.FillPolygon(brushDark1,
                                new PointF[] {
                                   new PointF(0, h),
                                   new PointF(w, h),
                                   new PointF(w, 0),
                                   new PointF(w - d, d),
                                   new PointF(w - d, h - d),
                                   new PointF(d, h - d),
                                });

         e.Graphics.FillPolygon(brushDark1,
                                new PointF[] {
                                   new PointF(d, d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2, h / 2 - d2),
                                   new PointF(d + d2, d),
                                });
         e.Graphics.FillPolygon(brushLight1,
                                new PointF[] {
                                   new PointF(d, d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2 - d2, h / 2),
                                   new PointF(d, d + d2),
                                });

         e.Graphics.FillPolygon(brushDark1,
                                new PointF[] {
                                   new PointF(w - d, d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2, h / 2 - d2),
                                   new PointF(w - d - d2, d),
                                });
         e.Graphics.FillPolygon(brushLight1,
                                new PointF[] {
                                   new PointF(w - d, d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2 + d2, h / 2),
                                   new PointF(w - d, d + d2),
                                });

         e.Graphics.FillPolygon(brushLight1,
                                new PointF[] {
                                   new PointF(d, h - d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2, h / 2 + d2),
                                   new PointF(d + d2, h - d),
                                });
         e.Graphics.FillPolygon(brushDark1,
                                new PointF[] {
                                   new PointF(d, h - d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2 - d2, h / 2),
                                   new PointF(d, h - d - d2),
                                });

         e.Graphics.FillPolygon(brushLight1,
                                new PointF[] {
                                   new PointF(w - d, h - d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2, h / 2 + d2),
                                   new PointF(w - d - d2, h - d),
                                });
         e.Graphics.FillPolygon(brushDark1,
                                new PointF[] {
                                   new PointF(w - d, h - d),
                                   new PointF(w / 2, h / 2),
                                   new PointF(w / 2 + d2, h / 2),
                                   new PointF(w - d, h - d - d2),
                                });

         DrawArrow(e.Graphics, new PointF(0.65F * w, h / 2), w * .26F, 0);
         DrawArrow(e.Graphics, new PointF(0.35F * w, h / 2), w * .26F, 180);
         DrawArrow(e.Graphics, new PointF(w / 2, 0.65F * h), h * .26F, 90);
         DrawArrow(e.Graphics, new PointF(w / 2, 0.35F * h), h * .26F, 270);
      }

      void DrawArrow(Graphics canvas, PointF from, float length, float angle) {
         canvas.TranslateTransform(from.X, from.Y);
         canvas.RotateTransform(angle);

         Pen pen = new Pen(colDark2, 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
         };

         canvas.DrawLine(pen, 0, 0, length, 0);
         float d = length / 4;

         canvas.DrawLine(pen, length, 0, length - d, -d);
         canvas.DrawLine(pen, length, 0, length - d, d);

         canvas.ResetTransform();
      }

      private void MoveControl_MouseClick(object sender, MouseEventArgs e) {
         if (e.Button == MouseButtons.Left) {
            Direction direction;

            // Bereich ermitteln
            // Linie y = h / w * x
            bool righttop = (double)ClientRectangle.Height / ClientRectangle.Width * e.X > e.Y;
            // Linie y = h - h / w * x
            bool lefttop = (double)ClientRectangle.Height - (double)ClientRectangle.Height / ClientRectangle.Width * e.X > e.Y;

            direction = righttop ?
                           (lefttop ? Direction.Up : Direction.Right) :
                           (lefttop ? Direction.Left : Direction.Down);

            DirectionEvent?.Invoke(this, new DirectionEventArgs(direction));
         }
      }

      private void MoveControl_BackColorChanged(object sender, System.EventArgs e) {
         MoveControl_Load(sender, null);
      }

      Color NewColor(Color colbase, double k) {
         return k > 0 ?
                    Color.FromArgb((byte)(colbase.R + (255 - colbase.R) * k),
                                   (byte)(colbase.G + (255 - colbase.G) * k),
                                   (byte)(colbase.B + (255 - colbase.B) * k)) :
                    Color.FromArgb((byte)(colbase.R * (1 + k)),
                                   (byte)(colbase.G * (1 + k)),
                                   (byte)(colbase.B * (1 + k)));
      }

   }
}
