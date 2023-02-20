using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public partial class FormMarkerEditing : Form {

      /// <summary>
      /// zu bearbeitender <see cref="Marker"/>
      /// </summary>
      public Marker Marker;

      /// <summary>
      /// Wurden Daten geändert?
      /// </summary>
      public bool WaypointChanged { get; private set; }

      /// <summary>
      /// Kann der Marker verändert werden?
      /// </summary>
      public bool MarkerIsReadOnly = false;

      public string[] Proposals = null;

      public List<GarminSymbol> GarminMarkerSymbols;

      string symbolname;


      public FormMarkerEditing() {
         InitializeComponent();
      }

      private void FormExtMarkerEditing_Load(object sender, EventArgs e) {
         if (Proposals != null &&
             Proposals.Length > 0) {
            comboBox_Name.Items.AddRange(Proposals);
            comboBox_Name.SelectedIndex = 0;
         } else
            comboBox_Name.Text = Marker.Waypoint.Name;
         textBoxDescription.Text = Marker.Waypoint.Description;
         textBoxComment.Text = Marker.Waypoint.Comment;

         if (Marker.Waypoint.Elevation == Gpx.BaseElement.NOTVALID_DOUBLE) {
            numericUpDownHeight.Value = 0;
            numericUpDownHeight.Enabled =
            checkBox_Height.Checked = false;
         } else {
            numericUpDownHeight.Value = (decimal)Marker.Waypoint.Elevation;
            numericUpDownHeight.Enabled =
            checkBox_Height.Checked = true;
         }

         if (Marker.Waypoint.Time == Gpx.BaseElement.NOTVALID_TIME) {
            dateTimePickerDT.Value = DateTime.Now;
            dateTimePickerDT.Checked = false;
         } else {
            dateTimePickerDT.Value = Marker.Waypoint.Time;
            dateTimePickerDT.Checked = true;
         }

         numericUpDownLon.Value = (decimal)Marker.Waypoint.Lon;

         numericUpDownLat.Value = (decimal)Marker.Waypoint.Lat;

         textBoxDescription.ReadOnly =
         textBoxComment.ReadOnly =
         numericUpDownHeight.ReadOnly =
         numericUpDownLon.ReadOnly =
         numericUpDownLat.ReadOnly = !Marker.IsEditable || MarkerIsReadOnly;

         checkBox_Height.Enabled =
         dateTimePickerDT.Enabled =
         button_Save.Enabled =
         comboBox_Name.Enabled = !textBoxDescription.ReadOnly;

         button_Marker.Enabled = !MarkerIsReadOnly && GarminMarkerSymbols.Count > 0;
         button_Marker.Image = Marker.Bitmap;

         symbolname = Marker.Symbolname;

         Text = comboBox_Name.Enabled ?
                     "Eigenschaften des Markers bearbeiten" :
                     "Eigenschaften des Markers";
      }

      private void checkBox_Height_CheckedChanged(object sender, EventArgs e) {
         CheckBox cb = sender as CheckBox;
         numericUpDownHeight.Enabled = cb.Checked;
      }

      private void button_Save_Click(object sender, EventArgs e) {
         if (IsMarkerChanged())
            Save();
         fromsavebutton = true;
      }

      bool fromsavebutton = false;

      private void FormExtMarkerEditing_FormClosing(object sender, FormClosingEventArgs e) {
         base.OnClosing(e);

         if (!MarkerIsReadOnly &&
             e.CloseReason == CloseReason.UserClosing) {
            if (!fromsavebutton)
               if (IsMarkerChanged())
                  if (MessageBox.Show("Geänderte Daten übernehmen?", "Speichern", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                     Save();
         }

         if (!Modal)
            Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      bool IsMarkerChanged() {
         if (!MarkerIsReadOnly)
            WaypointChanged = Marker.Waypoint.Name != comboBox_Name.Text.Trim() ||                                               // Name geändert
                              Marker.Waypoint.Description != textBoxDescription.Text ||
                              Marker.Waypoint.Comment != textBoxComment.Text ||
                              (Marker.Waypoint.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE) != checkBox_Height.Checked ||       // Ungültigkeitsstatus für Höhe geändert
                              (checkBox_Height.Checked && Marker.Waypoint.Elevation != (double)numericUpDownHeight.Value) ||     // Höhe geändert
                              (Marker.Waypoint.Time != Gpx.BaseElement.NOTVALID_TIME) != dateTimePickerDT.Checked ||             // Ungültigkeitsstatus für Zeitpunkt geändert
                              (dateTimePickerDT.Checked && (Marker.Waypoint.Time != dateTimePickerDT.Value)) ||                  // Zeit geändert
                              (decimal)Marker.Waypoint.Lon != numericUpDownLon.Value ||                                          // geogr. Länge geändert
                              (decimal)Marker.Waypoint.Lat != numericUpDownLat.Value ||                                          // geogr. Breite geändert
                              Marker.Symbolname != symbolname;                                                                   // Symbol geändert

         return WaypointChanged;
      }

      /// <summary>
      /// übernimmt die akt. Daten
      /// </summary>
      void Save() {
         if (WaypointChanged &&
             !MarkerIsReadOnly) {
            if (Marker.GpxDataContainer != null)
               Marker.GpxDataContainer.GpxDataChanged = true;

            Marker.Waypoint.Name = comboBox_Name.Text.Trim();
            Marker.Waypoint.Description = textBoxDescription.Text.Trim();
            Marker.Waypoint.Comment = textBoxComment.Text.Trim();

            Marker.Waypoint.Elevation = checkBox_Height.Checked ?
                                             (double)numericUpDownHeight.Value :
                                             Gpx.BaseElement.NOTVALID_DOUBLE;
            Marker.Waypoint.Lon = (double)numericUpDownLon.Value;
            Marker.Waypoint.Lat = (double)numericUpDownLat.Value;
            Marker.Waypoint.Time = dateTimePickerDT.Checked ?
                                             dateTimePickerDT.Value :
                                             Gpx.BaseElement.NOTVALID_TIME;
            Marker.Waypoint.Symbol = symbolname;
         }
      }

      private void button_Cancel_Click(object sender, EventArgs e) {
         Close();
      }

      private void button_Marker_Click(object sender, EventArgs e) {
         FormChooseMarkerTyp dlg = new FormChooseMarkerTyp() {
            GarminMarkerSymbols = GarminMarkerSymbols,
            GarminSymbolName = Marker.Symbolname,
         };
         if (dlg.ShowDialog() == DialogResult.OK) {

            string tmpname = Marker.Symbolname; // nur ein Trick um das neue Bild einfach zu ermitteln
            symbolname = Marker.Symbolname = dlg.GarminSymbolName;
            button_Marker.Image = Marker.Bitmap;
            Marker.Symbolname = tmpname;

         }

      }
   }
}
