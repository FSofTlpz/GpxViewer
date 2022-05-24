using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormMapLocation : Form {

      FormMain formmain;

      class Position {
         public double Zoom;
         public double Lon;
         public double Lat;
         public string Name;

         public Position(string name, double zoom, double lon, double lat) {
            Zoom = zoom;
            Lon = lon;
            Lat = lat;
            Name = name;
         }

         public override string ToString() {
            return string.Format("{0}: zoom={1}, lon={2}, lat={3}", Name, Zoom, Lon, Lat);
         }
      }

      readonly List<Position> poslst = new List<Position>();

      string RoamingConfigPath {
         get {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                Application.CompanyName,
                                Application.ProductName);
         }
      }

      string Filename {
         get {
            return Path.Combine(RoamingConfigPath, "locations.txt");
         }
      }


      public FormMapLocation() {
         InitializeComponent();
      }

      private void FormMapLocation_Load(object sender, EventArgs e) {
         formmain = Owner as FormMain;
         LoadFromFile();
      }

      private void FormMapLocation_FormClosing(object sender, FormClosingEventArgs e) {
         SaveAsFile();
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void FormMapLocation_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyData) {
            case Keys.Escape:
               //case Keys.Enter:
               Close();
               break;
         }
      }

      private void toolStripButton_Save_Click(object sender, EventArgs e) {
         double zoom = formmain.GetMapLocationAndZoom(out double lon, out double lat);
         FormMapLocationName form = new FormMapLocationName {
            PositionName = string.Format("Lon {0}°, Lat {1}°", lon, lat),
         };
         if (form.ShowDialog() == DialogResult.OK) {
            poslst.Insert(0, new Position(form.PositionName, zoom, lon, lat));
            listBox_Locations.Items.Insert(0, poslst[0].Name);
            listBox_Locations.SelectedIndex = 0;
         }
      }

      private void toolStripButton_Delete_Click(object sender, EventArgs e) {
         int idx = listBox_Locations.SelectedIndex;
         if (idx >= 0) {
            listBox_Locations.Items.RemoveAt(idx);
            poslst.RemoveAt(idx);
            if (idx > 0)
               listBox_Locations.SelectedIndex = idx - 1;
            else
               if (listBox_Locations.Items.Count > 0)
               listBox_Locations.SelectedIndex = 0;
         }
      }

      private void toolStripButton_Go_Click(object sender, EventArgs e) {
         int idx = listBox_Locations.SelectedIndex;
         if (idx >= 0) {
            Position pos = poslst[idx];
            // anzeigen ...
            formmain.SetMapLocationAndZoom(pos.Zoom, pos.Lon, pos.Lat);
            // ... und an die erste Stelle holen
            poslst.RemoveAt(idx);
            poslst.Insert(0, pos);
            listBox_Locations.Items.RemoveAt(idx);
            listBox_Locations.Items.Insert(0, pos.Name);
            listBox_Locations.SelectedIndex = 0;
         }
      }

      private void listBox_Locations_MouseDoubleClick(object sender, MouseEventArgs e) {
         int idx = (sender as ListBox).IndexFromPoint(e.Location);
         if (idx != ListBox.NoMatches) {
            FormMapLocationName form = new FormMapLocationName {
               PositionName = poslst[idx].Name,
            };
            if (form.ShowDialog() == DialogResult.OK) {
               listBox_Locations.Items[idx] =
               poslst[idx].Name = form.PositionName;
            }
         }
      }

      void SaveAsFile() {
         try {
            if (!File.Exists(Filename)) {
               if (!Directory.Exists(RoamingConfigPath))
                  Directory.CreateDirectory(RoamingConfigPath);
               using (FileStream stream = File.Create(Filename)) { }
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(Filename, FileMode.OpenOrCreate))) {
               foreach (Position pos in poslst) {
                  sw.WriteLine(pos.Zoom.ToString() + "\t" + pos.Lon.ToString() + "\t" + pos.Lat.ToString() + "\t" + pos.Name.Trim());
               }
            }
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler beim Speichern", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      void LoadFromFile() {
         try {
            if (File.Exists(Filename))
               using (StreamReader sr = new StreamReader(Filename)) {
                  poslst.Clear();
                  string line;
                  while ((line = sr.ReadLine()) != null) {
                     line = line.Trim();
                     if (line.Length > 0) {
                        string[] fields = line.Split(new char[] { '\t' });
                        if (fields.Length >= 3) {
                           poslst.Add(new Position(fields[3], Convert.ToDouble(fields[0]), Convert.ToDouble(fields[1]), Convert.ToDouble(fields[2])));
                           listBox_Locations.Items.Add(fields[3]);
                        }
                     }
                  }
               }
            if (listBox_Locations.Items.Count > 0)
               listBox_Locations.SelectedIndex = 0;
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler beim Lesen", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

   }
}
