using System;
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


      public FormGeoTagger() {
         InitializeComponent();

         pictureManager1.OnShowExtern += PictureManager1_OnShowExtern;
         pictureManager1.OnNeedNewData += PictureManager1_OnNeedNewData;
         pictureManager1.OnDeselectPictures += PictureManager1_OnDeselectPictures;
      }

      private void FormGeoTagger_FormClosing(object sender, FormClosingEventArgs e) {
         if (pictureManager1.CancelNewLoad())
            e.Cancel = true;
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
         if (lastFilename4OnShowExtern == null ||
             lastFilename4OnShowExtern != e.Filename)
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
         pictureManager1.SetExtern(filename, longitude, latitude);
      }

   }
}
