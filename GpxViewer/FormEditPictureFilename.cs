using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormEditPictureFilename : Form {

      public string EditText = "";

      public List<string> ProposalText = new List<string>();


      public FormEditPictureFilename() {
         InitializeComponent();
      }

      private void FormEditPictureFilename_Shown(object sender, EventArgs e) {
         comboBox1.Text = EditText;
         comboBox1.Items.AddRange(ProposalText.ToArray());
      }

      private void FormEditPictureFilename_FormClosed(object sender, FormClosedEventArgs e) {
         EditText = comboBox1.Text.Trim();
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
