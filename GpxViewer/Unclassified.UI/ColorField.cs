using System.Drawing;
using System.Windows.Forms;

namespace Unclassified.UI {
   public partial class ColorField : Panel {

      public Color Color {
         get => BackColor;
         set => BackColor = value;
      }

      public new bool Enabled {
         get => base.Enabled;
         set {
            if (base.Enabled != value) {
               base.Enabled = value;
               Refresh();
            }
         }
      }

      public ColorField() {
         InitializeComponent();
      }

      protected override void OnPaint(PaintEventArgs e) {
         if (Enabled)
            base.OnPaint(e);
         else {
            Brush fillbrush = new SolidBrush(Color.LightGray);
            e.Graphics.FillRectangle(fillbrush, 0, 0, Bounds.Width, Bounds.Height);

            Pen pen = new Pen(Color.Gray, 0);
            e.Graphics.DrawLine(pen, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
            e.Graphics.DrawLine(pen, 0, ClientRectangle.Height, ClientRectangle.Width, 0);

            pen.Dispose();
            fillbrush.Dispose();
         }
      }

   }
}
