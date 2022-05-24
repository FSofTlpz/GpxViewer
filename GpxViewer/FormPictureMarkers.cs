using SmallMapControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormPictureMarkers : Form {

      List<Marker> MarkerLst;

      public FormPictureMarkers() {
         InitializeComponent();
      }

      public FormPictureMarkers(List<Marker> markerlst) :
         this() {
         MarkerLst = markerlst;
      }

      private void FormPictureMarkers_Load(object sender, EventArgs e) {
         foreach (Marker marker in MarkerLst) {
            if (marker.Markertype == Marker.MarkerType.Foto) {
               FormPicture pictform = (Owner as FormMain).GetForm4Picture(marker.Waypoint.Name);
               checkedListBox1.Items.Add(Path.GetFileName(marker.Waypoint.Name), pictform != null);
            }
         }
      }

      private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e) {
         Marker marker = MarkerLst[e.Index];
         switch (e.NewValue) {
            case CheckState.Checked:
               (Owner as FormMain).ShowPicture(marker.Waypoint);
               break;

            case CheckState.Unchecked:
               FormPicture pictform = (Owner as FormMain).GetForm4Picture(marker.Waypoint.Name);
               if (pictform != null)
                  pictform.Close();
               break;
         }
      }

      private void FormPictureMarkers_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyData) {
            case Keys.Escape:
               //case Keys.Enter:
               Close();
               break;
         }
      }
   }
}
