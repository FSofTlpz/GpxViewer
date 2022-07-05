using FSofTUtils.Geography.GeoCoding;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormSearch : Form {

      public class GoToPointEventArgs : EventArgs {
         public readonly string Name;
         public readonly double Longitude;
         public readonly double Latitude;

         public GoToPointEventArgs(double lon, double lat, string name = null) {
            Longitude = lon;
            Latitude = lat;
            Name = name;
         }
      }

      public class GoToAreaEventArgs : GoToPointEventArgs {
         public readonly double Left;
         public readonly double Right;
         public readonly double Top;
         public readonly double Bottom;

         public GoToAreaEventArgs(double lon, double lat, double left, double right, double bottom, double top, string name = null) :
            base(lon, lat, name) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
         }
      }

      public event EventHandler<GoToPointEventArgs> GoToPointEvent;
      public event EventHandler<GoToAreaEventArgs> GoToAreaEvent;



      public FormSearch() {
         InitializeComponent();
      }

      private void FormGarminInfo_Load(object sender, EventArgs e) {
      }

      protected override void OnClosing(CancelEventArgs e) {
         base.OnClosing(e);
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void FormGarminInfo_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyData) {
            case Keys.Escape:
               //case Keys.Enter:
               Close();
               break;
         }
      }

      private void textBox1_TextChanged(object sender, EventArgs e) {
         button_Start.Enabled = (sender as TextBox).Text.Trim() != "";
      }

      private void button_Start_Click(object sender, EventArgs e) {
         listView_Result.Items.Clear();
         button_Start.Enabled = false;
         Cursor cursor = Cursor;
         Cursor = Cursors.WaitCursor;
         //GeoCodingResultBase[] geoCodingResult = null;
         GeoCodingResultOsm[] geoCodingResultOsm = null;
         try {
            geoCodingResultOsm = GeoCodingResultOsm.Get(textBox1.Text.Trim());
            foreach (GeoCodingResultOsm item in geoCodingResultOsm) {
               ListViewItem lvi = new ListViewItem(new string[] {
               item.Name,
               string.Format("{0:N6}° {1:N6}°, {2}: {3}",
                             item.Longitude,
                             item.Latitude,
                             item.OsmClass,
                             item.OsmValue),
            }) {
                  Tag = item,
               };
               listView_Result.Items.Add(lvi);
            }
            listView_Result.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
         } catch (Exception ex) {
            Cursor = cursor;
            MessageBox.Show(ex.Message, "Fehler bei der Suche", MessageBoxButtons.OK, MessageBoxIcon.Error);
            listView_Result.Items.Clear();
         }
         button_Start.Enabled = true;
         Cursor = cursor;
      }


      GeoCodingResultOsm actualGeoCodingResult = null;

      private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
         actualGeoCodingResult = null;
         Point pt = listView_Result.PointToClient(MousePosition);
         ListViewItem lvi = listView_Result.GetItemAt(pt.X, pt.Y);
         if (lvi != null && lvi.Tag != null && lvi.Tag is GeoCodingResultOsm) {
            actualGeoCodingResult = lvi.Tag as GeoCodingResultOsm;
            ToolStripMenuItem_ShowArea.Enabled =
            ToolStripMenuItem_ShowAreaAndMarker.Enabled = actualGeoCodingResult.BoundingRight - actualGeoCodingResult.BoundingLeft != 0;
         } else
            e.Cancel = true;
      }

      private void ToolStripMenuItem_ShowPosition_Click(object sender, EventArgs e) {
         if (actualGeoCodingResult != null)
            GoToPointEvent?.Invoke(this, new GoToPointEventArgs(actualGeoCodingResult.Longitude, 
                                                                actualGeoCodingResult.Latitude));
         actualGeoCodingResult = null;
      }

      private void ToolStripMenuItem_ShowArea_Click(object sender, EventArgs e) {
         if (actualGeoCodingResult != null)
            GoToAreaEvent?.Invoke(this, new GoToAreaEventArgs(actualGeoCodingResult.Longitude,
                                                              actualGeoCodingResult.Latitude,
                                                              actualGeoCodingResult.BoundingLeft, 
                                                              actualGeoCodingResult.BoundingRight, 
                                                              actualGeoCodingResult.BoundingBottom, 
                                                              actualGeoCodingResult.BoundingTop));
         actualGeoCodingResult = null;
      }

      private void ToolStripMenuItem_ShowPositionAndMarker_Click(object sender, EventArgs e) {
         if (actualGeoCodingResult != null)
            GoToPointEvent?.Invoke(this, new GoToPointEventArgs(actualGeoCodingResult.Longitude, 
                                                                actualGeoCodingResult.Latitude, 
                                                                actualGeoCodingResult.Name));
         actualGeoCodingResult = null;
      }

      private void ToolStripMenuItem_ShowAreaAndMarker_Click(object sender, EventArgs e) {
         if (actualGeoCodingResult != null)
            GoToAreaEvent?.Invoke(this, new GoToAreaEventArgs(actualGeoCodingResult.Longitude,
                                                              actualGeoCodingResult.Latitude, 
                                                              actualGeoCodingResult.BoundingLeft, 
                                                              actualGeoCodingResult.BoundingRight, 
                                                              actualGeoCodingResult.BoundingBottom, 
                                                              actualGeoCodingResult.BoundingTop, 
                                                              actualGeoCodingResult.Name));
         actualGeoCodingResult = null;
      }
   }
}
