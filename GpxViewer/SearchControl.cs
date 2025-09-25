using FSofTUtils.Geography.GeoCoding;
using System.ComponentModel;
using System.Text;

namespace GpxViewer {
   public partial class SearchControl : UserControl {

      public class GoToPointEventArgs : EventArgs {
         public readonly string? Name;
         public readonly double Longitude;
         public readonly double Latitude;

         public GoToPointEventArgs(double lon, double lat, string? name = null) {
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

         public GoToAreaEventArgs(double lon, double lat, double left, double right, double bottom, double top, string? name = null) :
            base(lon, lat, name) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
         }
      }

      public event EventHandler<GoToPointEventArgs>? GoToPointEvent;
      public event EventHandler<GoToAreaEventArgs>? GoToAreaEvent;

      GeoCodingResultOsm? actualGeoCodingResult = null;


      public SearchControl() {
         InitializeComponent();
      }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);
         loadSearch();
      }

      public void SetFocus2Inputfield() => textBoxSearch.Focus();

      void loadSearch() {
         textBoxSearch.TextChanged += (s, e) => buttonSearchStart.Enabled = (s as TextBox).Text.Trim() != "";
         buttonSearchStart.Click += buttonSearchStart_Click;
         contextMenuStripSearch.Opening += contextMenuStripSearch_Opening;

         ToolStripMenuItem_ShowPosition.Click += (s, e) => {
            if (actualGeoCodingResult != null) {
               GoToPointEvent?.Invoke(this, new GoToPointEventArgs(actualGeoCodingResult.Longitude,
                                                                   actualGeoCodingResult.Latitude));
               actualGeoCodingResult = null;
            }
         };
         ToolStripMenuItem_ShowArea.Click += (s, e) => {
            if (actualGeoCodingResult != null) {
               GoToAreaEvent?.Invoke(this, new GoToAreaEventArgs(actualGeoCodingResult.Longitude,
                                                                 actualGeoCodingResult.Latitude,
                                                                 actualGeoCodingResult.BoundingLeft,
                                                                 actualGeoCodingResult.BoundingRight,
                                                                 actualGeoCodingResult.BoundingBottom,
                                                                 actualGeoCodingResult.BoundingTop));
               actualGeoCodingResult = null;
            }
         };
         ToolStripMenuItem_ShowPositionAndMarker.Click += (s, e) => {
            if (actualGeoCodingResult != null) {
               GoToPointEvent?.Invoke(this, new GoToPointEventArgs(actualGeoCodingResult.Longitude,
                                                                   actualGeoCodingResult.Latitude,
                                                                   actualGeoCodingResult.Name));
               actualGeoCodingResult = null;
            }
         };
         ToolStripMenuItem_ShowAreaAndMarker.Click += (s, e) => {
            if (actualGeoCodingResult != null) {
               GoToAreaEvent?.Invoke(this, new GoToAreaEventArgs(actualGeoCodingResult.Longitude,
                                                                 actualGeoCodingResult.Latitude,
                                                                 actualGeoCodingResult.BoundingLeft,
                                                                 actualGeoCodingResult.BoundingRight,
                                                                 actualGeoCodingResult.BoundingBottom,
                                                                 actualGeoCodingResult.BoundingTop,
                                                                 actualGeoCodingResult.Name));
               actualGeoCodingResult = null;
            }
         };
         ToolStripMenuItem_Copy.Click += (s, e) => {
            if (listView_Result.SelectedIndices?.Count > 0)
               copyListViewText(listView_Result.SelectedIndices[0]);
         };
         ToolStripMenuItem_CopyAll.Click += (s, e) => copyListViewText(-1);

         listView_Result.MouseDoubleClick += (s, e) => {
            ListViewHitTestInfo hit = (s as ListView).HitTest(e.Location);
            if (hit != null && hit.Item != null)
               if (hit.Item.Index >= 0) {
                  actualGeoCodingResult = hit.Item.Tag as GeoCodingResultOsm;
                  GoToPointEvent?.Invoke(this, new GoToPointEventArgs(actualGeoCodingResult.Longitude,
                                                                      actualGeoCodingResult.Latitude));
                  actualGeoCodingResult = null;
               }
         };

      }

      private async void buttonSearchStart_Click(object? sender, EventArgs e) {
         listView_Result.Items.Clear();
         buttonSearchStart.Enabled = false;
         Cursor cursor = Cursor;
         Cursor = Cursors.WaitCursor;
         GeoCodingResultOsm[]? geoCodingResultOsm = null;
         try {
            geoCodingResultOsm = await GeoCodingResultOsm.GetAsync(textBoxSearch.Text.Trim(), 10);
            foreach (GeoCodingResultOsm item in geoCodingResultOsm) {
               ListViewItem lvi = new ListViewItem([
                                                      item.Name,
                                                      string.Format("{0:N6}° {1:N6}°, {2}: {3}",
                                                                    item.Longitude,
                                                                    item.Latitude,
                                                                    item.OsmClass,
                                                                    item.OsmValue),
                                                   ]) {
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
         buttonSearchStart.Enabled = true;
         Cursor = cursor;
      }

      private void contextMenuStripSearch_Opening(object? sender, CancelEventArgs e) {
         actualGeoCodingResult = null;
         Point pt = listView_Result.PointToClient(MousePosition);
         ListViewItem? lvi = listView_Result.GetItemAt(pt.X, pt.Y);
         if (lvi != null && lvi.Tag != null && lvi.Tag is GeoCodingResultOsm) {
            actualGeoCodingResult = (GeoCodingResultOsm)lvi.Tag;
            ToolStripMenuItem_ShowArea.Enabled =
            ToolStripMenuItem_ShowAreaAndMarker.Enabled = actualGeoCodingResult.BoundingRight - actualGeoCodingResult.BoundingLeft != 0;
         } else
            e.Cancel = true;
      }

      void copyListViewText(int idx) {
         if (listView_Result.Items != null) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listView_Result.Items.Count; i++) {
               if (idx < 0 || idx == i) {
                  ListViewItem lvi = listView_Result.Items[i];
                  for (int j = 0; j < lvi.SubItems.Count; j++) {
                     ListViewItem.ListViewSubItem slvi = lvi.SubItems[j];
                     if (j > 0)
                        sb.Append("\t");
                     sb.Append(slvi.Text);
                  }
                  sb.AppendLine();
               }
            }
            if (sb.Length > 0)
               Clipboard.SetText(sb.ToString());
         }
      }

   }
}
