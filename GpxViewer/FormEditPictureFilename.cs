using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormEditPictureFilename : Form {

      public string EditText = "";

      public FormEditPictureFilename() {
         InitializeComponent();
      }

      private void FormEditPictureFilename_Shown(object sender, EventArgs e) {
         textBox1.Text = EditText;
      }

      private void FormEditPictureFilename_FormClosed(object sender, FormClosedEventArgs e) {
         EditText = textBox1.Text.Trim();
      }

      private void FormEditPictureFilename_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyCode) {
            case Keys.Enter:
               DialogResult = DialogResult.OK;
               break;

            case Keys.Escape:
               DialogResult = DialogResult.Cancel;
               break;
         }
      }
   }
}
