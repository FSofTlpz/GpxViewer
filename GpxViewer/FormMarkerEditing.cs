using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public partial class FormMarkerEditing : Form {

      /// <summary>
      /// zu bearbeitender <see cref="Marker"/>
      /// </summary>
      public Marker? Marker;

      /// <summary>
      /// Wurden Daten geändert?
      /// </summary>
      public bool WaypointChanged { get; private set; }

      /// <summary>
      /// Kann der Marker verändert werden?
      /// </summary>
      public bool MarkerIsReadOnly = false;

      public string[]? Proposals = null;

      public List<GarminSymbol>? GarminMarkerSymbols;


      string symbolname = string.Empty;

      bool fromsavebutton = false;

      bool isMarkerChanged {
         get {
            if (!MarkerIsReadOnly)
               return isChanged(Marker.Waypoint.Name, comboBox_Name.Text.Trim()) ||                                   // Name geändert
                      isChanged(Marker.Waypoint.Description, textBoxDescription.Text) ||
                      isChanged(Marker.Waypoint.Comment, textBoxComment.Text) ||
                      (Marker.Waypoint.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE) != checkBox_Height.Checked ||    // Ungültigkeitsstatus für Höhe geändert
                      (checkBox_Height.Checked && isChanged(Marker.Waypoint.Elevation, numericUpDownHeight.Value)) || // Höhe geändert
                      (Marker.Waypoint.Time != Gpx.BaseElement.NOTVALID_TIME) != dateTimePickerDT.Checked ||          // Ungültigkeitsstatus für Zeitpunkt geändert
                      (dateTimePickerDT.Checked && (Marker.Waypoint.Time != dateTimePickerDT.Value)) ||               // Zeit geändert
                      isChanged(Marker.Waypoint.Lon, numericUpDownLon.Value) ||                                       // geogr. Länge geändert
                      isChanged(Marker.Waypoint.Lat, numericUpDownLat.Value) ||                                       // geogr. Breite geändert
                      Marker.Symbolname != symbolname;                                                                // Symbol geändert

            return false;
         }
      }


      public FormMarkerEditing() {
         InitializeComponent();
      }

      private void FormMarkerEditing_Load(object sender, EventArgs e) {
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
         textBoxComment.ReadOnly = !Marker.IsEditable || MarkerIsReadOnly;
         numericUpDownHeight.Enabled =
         numericUpDownLon.Enabled =
         numericUpDownLat.Enabled =
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

      private void FormMarkerEditing_FormClosing(object sender, FormClosingEventArgs e) {
         base.OnClosing(e);

         if (!MarkerIsReadOnly &&
             e.CloseReason == CloseReason.UserClosing) {
            if (!fromsavebutton)
               if (isMarkerChanged)
                  if (MessageBox.Show("Geänderte Daten übernehmen?",
                                      "Speichern",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question,
                                      MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                     Save();
         }

         if (!Modal)
            Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void checkBox_Height_CheckedChanged(object sender, EventArgs e) {
         CheckBox? cb = sender as CheckBox;
         if (cb != null)
            numericUpDownHeight.Enabled = cb.Checked;
      }

      /// <summary>
      /// übernimmt die akt. Daten
      /// </summary>
      void Save() {
         if (!MarkerIsReadOnly) {
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

            WaypointChanged = true;
         }
      }

      bool isChanged(string txt1, string txt2) {
         if (txt1 == null)
            txt1 = "";
         if (txt2 == null)
            txt2 = "";
         return txt1.Trim() != txt2.Trim();
      }

      bool isChanged(decimal v1, decimal v2) => v1 != v2;

      bool isChanged(double v1, decimal v2) => (decimal)v1 != v2;

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

      private void button_Save_Click(object sender, EventArgs e) {
         if (isMarkerChanged)
            Save();
         fromsavebutton = true;
      }

      private void button_Cancel_Click(object sender, EventArgs e) {
         Close();
      }
   }
}
