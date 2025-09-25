namespace GpxViewer.PictureEdit {
   public partial class FormEditPictureDateTime : Form {

      public DateTime DateTime { get; set; }

      public bool IsValid {
         get => dateTimePicker1.Checked;
         set => dateTimePicker1.Checked = value;
      }


      public FormEditPictureDateTime() {
         InitializeComponent();
      }

      private void FormEditPictureFilename_Shown(object sender, EventArgs e) {
         if (DateTime < dateTimePicker1.MinDate)
            IsValid = false;

         if (IsValid)
            dateTimePicker1.Value = DateTime;
      }

      private void FormEditPictureFilename_FormClosed(object sender, FormClosedEventArgs e) {
         DateTime = dateTimePicker1.Value;
      }

      private void FormEditPictureFilename_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyCode) {
            case Keys.Enter:
               DialogResult = DialogResult.OK;
               break;

            case Keys.Escape:
               DialogResult = DialogResult.Cancel;
               break;

            case Keys.Insert:
               if (e.Control) {        // Strg+Ins
                  if (dateTimePicker1.Checked)
                     Clipboard.SetText(dateTimePicker1.Value.ToString("G"));
               } else if (e.Shift) {   // Shift+Ins
                  if (dateTimePicker1.Checked)
                     try {
                        string txt = Clipboard.GetText();
                        dateTimePicker1.Value = DateTime.Parse(txt);
                     } catch { }
               }
               break;

            case Keys.C:
               if (e.Control) {        // Strg+V
                  if (dateTimePicker1.Checked)
                     Clipboard.SetText(dateTimePicker1.Value.ToString("G"));
               }
               break;

            case Keys.V:
               if (e.Control) {        // Strg+C
                  if (dateTimePicker1.Checked) 
                     try {
                        string txt = Clipboard.GetText();
                        dateTimePicker1.Value = DateTime.Parse(txt);
                     } catch { }
               }
               break;

         }
      }
   }
}
