using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormPicture : Form {

      public string PictureFilename { get; private set; }

      public string Info { get; private set; }


      public FormPicture() {
         InitializeComponent();
      }

      public FormPicture(string picturefilename, string info) : this() {
         PictureFilename = picturefilename;
         Info = info;
      }

      private void FormPicture_Load(object sender, EventArgs e) {
         if (!string.IsNullOrEmpty(PictureFilename)) {
            Text = Path.GetFileName(PictureFilename);
            textBoxInfo.AppendText(Info);
            try {
               pictureBox1.Image = new Bitmap(PictureFilename);
               if (pictureBox1.Image.Width * pictureBox1.Height > pictureBox1.Image.Height * pictureBox1.Width) {
                  int newheight = (int)Math.Round((pictureBox1.Image.Height * pictureBox1.Width) / (double)pictureBox1.Image.Width);
                  Height -= (pictureBox1.Height - newheight);
               } else {
                  int newwidth = (int)Math.Round((pictureBox1.Image.Width * pictureBox1.Height) / (double)pictureBox1.Image.Height);
                  Width -= (pictureBox1.Width - newwidth);
               }
            } catch (Exception ex) {
               MessageBox.Show(ex.Message, "Fehler beider Bildanzeige", MessageBoxButtons.OK, MessageBoxIcon.Error);
               Close();
            }
         }
      }

      protected override void OnClosing(CancelEventArgs e) {
         base.OnClosing(e);
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void FormPicture_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyCode) {
            case Keys.Escape:
               //case Keys.Enter:
               Close();
               break;

            case Keys.C:
               if (e.Modifiers == Keys.Control)
                  button_Copy_Click(button_Copy, new EventArgs());
               break;
         }

      }

      private void button_Copy_Click(object sender, EventArgs e) {
         if (pictureBox1.Image != null)
            Clipboard.SetImage(pictureBox1.Image);
      }
   }
}
