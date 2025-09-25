using GpxViewer.Common;

namespace GpxViewer {
   public partial class LocationControl : UserControl {

      public class ZoomAndPositionEventArgs : EventArgs {
         public double Zoom;
         public double Longitude;
         public double Latitude;
         public bool Cancel = false;

         public ZoomAndPositionEventArgs(double zoom, double lon, double lat) {
            Zoom = zoom;
            Longitude = lon;
            Latitude = lat;
         }
      }

      public event EventHandler<ZoomAndPositionEventArgs>? GetZoomAndPositionEvent;
      public event EventHandler<ZoomAndPositionEventArgs>? SetZoomAndPositionEvent;



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

         public override string ToString() => string.Format("{0}: zoom={1}, lon={2}, lat={3}", Name, Zoom, Lon, Lat);

      }

      public AppData? AppData = null;

      readonly List<Position> poslst = new List<Position>();

      string roamingConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                               Application.CompanyName ?? string.Empty,
                                               Application.ProductName ?? string.Empty);

      string locationsFilename => Path.Combine(roamingConfigPath, "locations.txt");


      public LocationControl() {
         InitializeComponent();
      }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);
         loadLocation();
      }

      /// <summary>
      /// speichert die akt. Daten der Liste in einer Datei
      /// </summary>
      void saveData() {
         if (AppData == null)
            saveAsFile();
         else
            saveAsList();
      }

      void loadLocation() {
         if (AppData == null)
            loadFromFile();
         else
            loadFromList();

         toolStripButton_Go.Click += (s, e) => {
            int idx = listBox_Locations.SelectedIndex;
            if (idx >= 0)
               gotoLocation(idx);
         };

         listBox_Locations.MouseDoubleClick += (s, e) => {
            int idx = (s as ListBox).IndexFromPoint(e.Location);
            if (idx != ListBox.NoMatches)
               gotoLocation(idx);
         };
      }

      private void toolStripButton_Save_Click(object? sender, EventArgs e) {
         if (GetZoomAndPositionEvent != null) {
            ZoomAndPositionEventArgs evargs = new ZoomAndPositionEventArgs(0, 0, 0);
            GetZoomAndPositionEvent.Invoke(this, evargs);
            if (!evargs.Cancel) {
               FormTextInput dlg = new FormTextInput() {
                  Usercaption = "Name für Position",
                  Usertext = string.Format("Lon {0}°, Lat {1}°", evargs.Longitude, evargs.Latitude),
               };
               if (dlg.ShowDialog() == DialogResult.OK &&
                   dlg.Usertext.Trim() != "") {
                  poslst.Insert(0, new Position(dlg.Usertext, evargs.Zoom, evargs.Longitude, evargs.Latitude));
                  listBox_Locations.Items.Insert(0, poslst[0].Name);
                  listBox_Locations.SelectedIndex = 0;
                  saveData();
               }
            }
         }
      }

      private void toolStripButton_Delete_Click(object? sender, EventArgs e) {
         int idx = listBox_Locations.SelectedIndex;
         if (idx >= 0) {
            listBox_Locations.Items.RemoveAt(idx);
            poslst.RemoveAt(idx);
            if (idx > 0)
               listBox_Locations.SelectedIndex = idx - 1;
            else
               if (listBox_Locations.Items.Count > 0)
               listBox_Locations.SelectedIndex = 0;
            saveData();
         }
      }

      private void toolStripButton_Edit_Click(object? sender, EventArgs e) {
         int idx = listBox_Locations.SelectedIndex;
         if (idx >= 0) {
            FormTextInput dlg = new FormTextInput() {
               Usercaption = "Name für Position",
               Usertext = poslst[idx].Name,
            };
            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.Usertext.Trim() != "") {
               listBox_Locations.Items[idx] =
               poslst[idx].Name = dlg.Usertext;
               saveData();
            }
         }
      }

      void gotoLocation(int idx) {
         Position pos = poslst[idx];
         // anzeigen ...
         SetZoomAndPositionEvent?.Invoke(this, new ZoomAndPositionEventArgs(pos.Zoom, pos.Lon, pos.Lat));
         // ... und an die erste Stelle holen
         poslst.RemoveAt(idx);
         poslst.Insert(0, pos);
         listBox_Locations.Items.RemoveAt(idx);
         listBox_Locations.Items.Insert(0, pos.Name);
         listBox_Locations.SelectedIndex = 0;
      }

      void saveAsFile() {
         try {
            if (!File.Exists(locationsFilename)) {
               if (!Directory.Exists(roamingConfigPath))
                  Directory.CreateDirectory(roamingConfigPath);
               using (FileStream stream = File.Create(locationsFilename)) { }
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(locationsFilename, FileMode.OpenOrCreate))) {
               foreach (Position pos in poslst) {
                  sw.WriteLine(pos.Zoom.ToString() + "\t" + pos.Lon.ToString() + "\t" + pos.Lat.ToString() + "\t" + pos.Name.Trim());
               }
            }
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler beim Speichern", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      void loadFromFile() {
         try {
            if (File.Exists(locationsFilename))
               using (StreamReader sr = new StreamReader(locationsFilename)) {
                  poslst.Clear();
                  string? line;
                  while ((line = sr.ReadLine()) != null) {
                     line = line.Trim();
                     if (line.Length > 0) {
                        string[] fields = line.Split(['\t']);
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

      void saveAsList() {
         List<string> lst = new List<string>();
         foreach (Position pos in poslst)
            lst.Add(pos.Zoom.ToString() + "\t" + pos.Lon.ToString() + "\t" + pos.Lat.ToString() + "\t" + pos.Name.Trim());
         AppData.PositionList = lst;
      }

      void loadFromList() {
         foreach (string line in AppData.PositionList) {
            string txt = line.Trim();
            if (txt.Length > 0) {
               string[] fields = txt.Split(new char[] { '\t' });
               if (fields.Length >= 3) {
                  poslst.Add(new Position(fields[3], Convert.ToDouble(fields[0]), Convert.ToDouble(fields[1]), Convert.ToDouble(fields[2])));
                  listBox_Locations.Items.Add(fields[3]);
               }
            }
         }
      }

   }
}
