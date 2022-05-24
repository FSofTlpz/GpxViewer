using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormInfo : Form {

      public string InfoText { get; private set; } = "";


      public FormInfo() {
         InitializeComponent();
      }

      public FormInfo(string info) : this() {
         InfoText = info;
      }

      private void FormInfo_Load(object sender, EventArgs e) {
         pictureBox1.Image = SystemIcons.Asterisk.ToBitmap();
         textBoxInfo.Text = InfoText;
         textBoxInfo.Select(0, 0);
      }

      protected override void OnClosing(CancelEventArgs e) {
         base.OnClosing(e);
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void FormInfo_KeyDown(object sender, KeyEventArgs e) {
         if (e.KeyCode == Keys.Escape) {
            Close();
         }
      }

      private void button_End_Click(object sender, EventArgs e) {
         Close();
      }
   }
}
