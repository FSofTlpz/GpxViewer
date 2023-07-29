using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Forms;
using static GpxViewer.PictureManager;
using static GpxViewer.PictureManager.PictureDataListEventArgs;

namespace GpxViewer {
   public partial class FormGeoTagger : Form {

      /// <summary>
      /// (Bild-)Dateien sollte extern angezeigt werden.
      /// </summary>
      public event EventHandler<PictureDataEventArgs> OnShowExtern;

      /// <summary>
      /// Die externe Anzeige von (Bild-)Dateien sollte beendet werden.
      /// </summary>
      public event EventHandler<PictureDataEventArgs> OnHideExtern;

      /// <summary>
      /// Für die (Bild-)Dateien werden neue Geodaten gewünscht.
      /// </summary>
      public event EventHandler<PictureDataEventArgs> OnNeedNewData;

      string lastFilename4OnShowExtern = null;

      public string PicturePath { get; set; } = "";


      public FormGeoTagger() {
         InitializeComponent();

         pictureManager1.OnShowExtern += PictureManager1_OnShowExtern;
         pictureManager1.OnNeedNewData += PictureManager1_OnNeedNewData;
         pictureManager1.OnDeselectPictures += PictureManager1_OnDeselectPictures;

#if TEST_FORMGEOTAGGER
         ShowInTaskbar = true;
#endif
      }

      private void FormGeoTagger_Load(object sender, EventArgs e) {
#if TEST_FORMGEOTAGGER
         // x();
         //y();

         //ExifGeo.FromDouble(9.58848);
         //MathEx.UFraction32 t = new MathEx.UFraction32(0);
         //t = t + 9.58848;


         //pictureManager1.SetPicturePath(@"..\..\..\..\Gpx\2020_02 Stralsund", false);
         pictureManager1.SetPicturePath("../../Test", false);

#else

         if (!string.IsNullOrEmpty(PicturePath))
            pictureManager1.ActualPicturePath = PicturePath;

#endif
      }

      private void FormGeoTagger_FormClosing(object sender, FormClosingEventArgs e) {
         if (pictureManager1.CancelNewLoad())
            e.Cancel = true;
         PicturePath = pictureManager1.ActualPicturePath;
      }

      private void PictureManager1_OnDeselectPictures(object sender, PictureDataListEventArgs e) {
         foreach (PictureData pd in e.PictureDatas) {
            if (!string.IsNullOrEmpty(lastFilename4OnShowExtern) &&
                pd.Filename == lastFilename4OnShowExtern) {
               OnHideExtern?.Invoke(this, new PictureDataEventArgs(pd.Filename, pd.Longitude, pd.Latitude));
               break;
            }
         }
      }

      private void PictureManager1_OnNeedNewData(object sender, PictureDataEventArgs e) {
         if ((lastFilename4OnShowExtern == null ||
              lastFilename4OnShowExtern != e.Filename) &&
             e.Latitude != double.MinValue &&
             e.Longitude != double.MinValue)
            PictureManager1_OnShowExtern(sender, e);

         OnNeedNewData?.Invoke(this, e);
      }

      private void PictureManager1_OnShowExtern(object sender, PictureDataEventArgs e) {
         if (!string.IsNullOrEmpty(lastFilename4OnShowExtern))
            OnHideExtern?.Invoke(this, new PictureDataEventArgs(lastFilename4OnShowExtern, 0, 0));
         lastFilename4OnShowExtern = e.Filename;
         OnShowExtern?.Invoke(this, e);
      }

      /// <summary>
      /// neue Koordinaten für die Bilddatei setzen
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="longitude"></param>
      /// <param name="latitude"></param>
      public void SetExtern(string filename, double longitude, double latitude) {
         pictureManager1.SetPositionExtern(filename, longitude, latitude);
      }

      /// <summary>
      /// für die "Hotkeys"
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void FormGeoTagger_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyData) {
            // nur für Toolbar-Buttons notwendig

            case Keys.Control | Keys.V:
               pictureManager1.OpenPath();
               e.Handled = true;
               break;

            case Keys.Control | Keys.R:
               pictureManager1.Reload();
               e.Handled = true;
               break;

            case Keys.Control | Keys.W:
               pictureManager1.SwapView();
               e.Handled = true;
               break;

         }
      }

   }
}
