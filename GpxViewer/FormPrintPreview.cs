using System.Drawing.Printing;

namespace GpxViewer {
   public partial class FormPrintPreview : Form {

      public PrintDocument? Document;


      public FormPrintPreview() {
         InitializeComponent();
      }

      private void FormPrintPreview_Load(object sender, EventArgs e) {
         printPreviewControl1.Document = Document;

         printPreviewControl1.Document.PrintPage += Document_PrintPage;
         printPreviewControl1.Document.BeginPrint += Document_BeginPrint;
         printPreviewControl1.Document.QueryPageSettings += Document_QueryPageSettings;


      }

      // ein merkwürdiger Fehler bei Netzwerkdruckern, deren Eigenschaften nicht schnell genug geholt werden:

      private void Document_QueryPageSettings(object sender, QueryPageSettingsEventArgs e) {
         try {
            // Die Eigenschaften von e.PageSettings sind entweder problemlos vorhanden oder werden "im Hintergrund" angefordert.
            // Die Anforderung kann vom Anwender abgebrochen werden. 
            // Werden sie benötigt werden sie daraufhin aber wieder im Hintergrund (mit Abbruchdialog) angefordert.
            // Nur die Eigenschaften HardMarginX und HardMarginY führem zu einer Exception (Division durch 0).
            // Dann sollte aber ein Abbruch erfolgen.

            //Debug.WriteLine(e.PageSettings.Bounds);
            //Debug.WriteLine(e.PageSettings.Color);
            //Debug.WriteLine(e.PageSettings.Landscape);
            //Debug.WriteLine(e.PageSettings.Margins);
            //Debug.WriteLine(e.PageSettings.PaperSize);
            //Debug.WriteLine(e.PageSettings.PaperSource);
            //Debug.WriteLine(e.PageSettings.PrintableArea);
            //Debug.WriteLine(e.PageSettings.PrinterResolution);
            //Debug.WriteLine(e.PageSettings.PrinterSettings);
            //Debug.WriteLine(e.PageSettings.HardMarginX);
            //Debug.WriteLine(e.PageSettings.HardMarginY);

            if (e.PageSettings.HardMarginX < 0 ||
                e.PageSettings.HardMarginY < 0) {
               throw new Exception();
            }

         } catch {
            e.Cancel = true;
         }
      }

      private void Document_BeginPrint(object sender, PrintEventArgs e) {

      }

      private void Document_PrintPage(object sender, PrintPageEventArgs e) {

      }

      private void button_goon_Click(object sender, EventArgs e) {
         Close();
      }

      private void button_cancel_Click(object sender, EventArgs e) {
         Close();
      }
   }
}
