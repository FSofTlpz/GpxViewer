using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormOffsetPictureGpx : Form {

      public TimeSpan Offset { get; set; }


      public FormOffsetPictureGpx() {
         InitializeComponent();
      }

      private void FormOffsetPictureGpx_Load(object sender, EventArgs e) {
         numericUpDownHours.Value = Offset.Hours;
         numericUpDownMinutes.Value = Offset.Minutes;
      }

      private void button1_Click(object sender, EventArgs e) {
         Offset = new TimeSpan((int)numericUpDownHours.Value, (int)numericUpDownMinutes.Value, 0);
      }
   }
}
