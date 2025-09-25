using SpecialMapCtrl;
using System.Text;

namespace GpxViewer {
   public partial class FormCache : Form {

      SpecialMapCtrl.SpecialMapCtrl? mapControl;

      int mapProviderDefIdx = -1;
      int actualListIdx = -1;

      FilecacheManager.FilecacheInfo? filecacheInfo;


      public FormCache(SpecialMapCtrl.SpecialMapCtrl? mapcontrol, int mapproviderdefidx) {
         InitializeComponent();
         mapControl = mapcontrol;
         mapProviderDefIdx = mapproviderdefidx;
         labelCachelocation.Text = "Cache-Verzeichnis: " + mapcontrol.M_CacheLocation;
      }

      private async void FormCache_Load(object sender, EventArgs e) => await showData();

      async Task<FilecacheManager.FilecacheInfo?> getData() {
         FilecacheManager.FilecacheInfo? cacheInfo = null;
         await Task.Run(() => {
            cacheInfo = mapControl.GetFilecacheInfo();
         });
         return cacheInfo;
      }

      async Task showData() {
         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;

         labelActualMap.Text =
         labelSum.Text = string.Empty;
         listBox1.Items.Clear();
         actualListIdx = -1;

         filecacheInfo = await getData();
         showSumBytes(filecacheInfo);

         if (filecacheInfo != null) {
            foreach (var mi in filecacheInfo.CacheInfos) {
               StringBuilder sb = new StringBuilder();
               sb.Append("(" + mi.CacheName + ")");
               if (mi.DbIdDelta > 0)
                  sb.Append(" (" + mi.DbIdDelta + ")");
               sb.Append(" \"" + mi.Mapname + "\"");
               sb.Append(" (" + mi.ProviderName + "):");
               if (mi.CacheExists)
                  sb.Append(" " + (mi.Bytes / 1024.0 / 1024.0).ToString("f2") + " MB in " +
                            mi.TileCount + " Kartenteil" + (mi.TileCount == 1 ? string.Empty : "en") + " und " +
                            mi.ZoomLevelsCount + " Zoomstufe" + (mi.ZoomLevelsCount == 1 ? string.Empty : "n"));

               if (!mi.CacheExists || !mi.IsUsed) {
                  sb.Append(" [CACHE ");
                  if (!mi.CacheExists) {
                     sb.Append("NICHT VORHANDEN");
                     if (!mi.IsUsed)
                        sb.Append(" und ");
                  }
                  if (!mi.IsUsed)
                     sb.Append("NICHT VERWENDET");
                  sb.Append("]");
               }

               listBox1.Items.Add(sb.ToString());

               if (mi.MapProviderDefIdx == mapProviderDefIdx) {
                  showActualMapBytes(mi.Bytes);
                  actualListIdx = listBox1.Items.Count - 1;
                  listBox1.SelectedIndex = actualListIdx;
               }
            }
         }
         Cursor = orgCursor;

         buttonClearActual.Enabled = actualListIdx >= 0 && filecacheInfo != null ?
                                          filecacheInfo.CacheInfos[actualListIdx].Bytes > 0 :
                                          false;
         buttonClearAll.Enabled = filecacheInfo?.Bytes > 0;
      }

      string getBytesText(long bytes) => (bytes / 1024.0 / 1024.0).ToString("f2") + " MB";

      void showSumBytes(FilecacheManager.FilecacheInfo? cacheInfo) => labelSum.Text = getBytesText(cacheInfo != null ? cacheInfo.Bytes : 0);

      void showActualMapBytes(long bytes) => labelActualMap.Text = mapControl.M_ProviderDefinitions[mapProviderDefIdx].MapName + " (" + getBytesText(bytes) + ")";

      private async void buttonRefresh_Click(object sender, EventArgs e) => await showData();

      private async void buttonClearActual_Click(object sender, EventArgs e) => await clear(actualListIdx);

      private async void buttonClearAll_Click(object sender, EventArgs e) => await clear(-1);

      private async void listBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
         int idx = ((ListBox)sender).SelectedIndex;
         if (idx >= 0)
            await clear(idx);
      }

      async Task clear(int listidx) {
         if (MessageBox.Show(listidx >= 0 ?
                                    "Cache für die Karte " + Environment.NewLine + Environment.NewLine +
                                       "'(" + filecacheInfo.CacheInfos[listidx].CacheName + ") " + filecacheInfo.CacheInfos[listidx].Mapname + "'" + Environment.NewLine + Environment.NewLine +
                                       " löschen?" :
                                    "Gesamten Cache löschen?",
                             "Cache löschen",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question,
                             MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
            Cursor orgCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            mapControl.M_ClearMemoryCache();

            string error;
            int count = 0;
            bool multiuseremove = false;
            if (listidx >= 0)
               (count, multiuseremove, error) = await mapControl.ClearFileCache(filecacheInfo.CacheInfos[listidx]);
            else
               (count, multiuseremove, error) = await mapControl.ClearFileCache(filecacheInfo.CacheInfos);

            Cursor = orgCursor;

            if (error != string.Empty)
               MessageBox.Show("Fehler beim Löschen: " + System.Environment.NewLine + System.Environment.NewLine + error,
                               "Ergebnis",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
            else {
               MessageBox.Show(count + " Kartenteile gelöscht", "Ergebnis", MessageBoxButtons.OK, MessageBoxIcon.Information);
               if (listidx >= 0) {
                  listBox1.Items.RemoveAt(listidx);
                  filecacheInfo.CacheInfos.RemoveAt(listidx);
                  if (actualListIdx == listidx)
                     showActualMapBytes(0);
                  showSumBytes(filecacheInfo);
                  if (listidx < listBox1.Items.Count)
                     listBox1.SelectedIndex = listidx;
               } else {
                  listBox1.Items.Clear();
                  filecacheInfo.CacheInfos.Clear();
                  await showData();
               }
            }
            mapControl.M_Refresh(true, true, false, false);    // auch den Memory-Cache löschen
         }
      }

   }
}
