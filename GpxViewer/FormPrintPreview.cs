using System;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormPrintPreview : Form {

      public PrintDocument Document;


      public FormPrintPreview() {
         InitializeComponent();
      }

      private void FormPrintPreview_Load(object sender, EventArgs e) {
         printPreviewControl1.Document = Document;
      }

      private void button_goon_Click(object sender, EventArgs e) {
         Close();
      }

      private void button_cancel_Click(object sender, EventArgs e) {
         Close();
      }
   }
}
