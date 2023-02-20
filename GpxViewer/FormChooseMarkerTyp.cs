using FSofTUtils.Geography.Garmin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormChooseMarkerTyp : Form {

      public string GarminSymbolName;

      public List<GarminSymbol> GarminMarkerSymbols;


      public FormChooseMarkerTyp() {
         InitializeComponent();
      }

      private void FormChooseMarkerTyp_Load(object sender, EventArgs e) {
         if (GarminMarkerSymbols != null)
            initListViewMarker();
      }

      private void listView1_DoubleClick(object sender, EventArgs e) {
         button_OK_Click(null, null);
      }


      private void contextMenuStrip_LVType_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         switch (listView1.View) {
            case View.LargeIcon:
               toolStripMenuItem_LargeIcon.Checked = true;
               toolStripMenuItem_Tile.Checked = false;
               break;

            case View.Tile:
               toolStripMenuItem_LargeIcon.Checked = false;
               toolStripMenuItem_Tile.Checked = true;
               break;
         }
      }

      private void toolStripMenuItem_LargeIcon_Click(object sender, EventArgs e) {
         listView1.View = View.LargeIcon;
      }

      private void toolStripMenuItem_Tile_Click(object sender, EventArgs e) {
         listView1.View = View.Tile;
      }

      private void button_OK_Click(object sender, EventArgs e) {
         ListView.SelectedIndexCollection coll = listView1.SelectedIndices;
         if (coll != null && coll.Count > 0) {
            GarminSymbolName = coll != null && coll.Count > 0 ?
                                    GarminMarkerSymbols[listView1.SelectedIndices[0]].Name :
                                    null;
            DialogResult = DialogResult.OK;  // falls vom Doppelklick
         }
         Close();
      }

      private void button_Cancel_Click(object sender, EventArgs e) {
         Close();
      }

      void initListViewMarker() {
         Dictionary<string, int> Groups = new Dictionary<string, int>();
         Dictionary<string, int> FirstGroupItem = new Dictionary<string, int>();
         ImageList imageList = new ImageList {
            ImageSize = new Size(24, 24)
         };

         for (int i = 0; i < GarminMarkerSymbols.Count; i++) {
            if (!Groups.ContainsKey(GarminMarkerSymbols[i].Group)) {
               Groups.Add(GarminMarkerSymbols[i].Group, Groups.Count);
               FirstGroupItem.Add(GarminMarkerSymbols[i].Group, i);
            }
            imageList.Images.Add(GarminMarkerSymbols[i].Bitmap);
         }

         listView1.SmallImageList = imageList;
         listView1.LargeImageList = imageList;
         listView1.View = View.LargeIcon; //.LargeIcon Tile

         for (int i = 0; i < Groups.Count; i++) {
            foreach (var item in Groups.Keys) {
               if (Groups[item] == i) {
                  listView1.Groups.Add(new ListViewGroup(item));
                  break;
               }
            }
         }

         int selectedidx = -1;
         for (int i = 0; i < GarminMarkerSymbols.Count; i++) {
            listView1.Items.Add(new ListViewItem(GarminMarkerSymbols[i].Name, i) {
               Group = listView1.Groups[Groups[GarminMarkerSymbols[i].Group]],
            });
            if (!string.IsNullOrEmpty(GarminSymbolName) && 
                GarminMarkerSymbols[i].Name == GarminSymbolName) 
               selectedidx = i;
         }

         if (selectedidx < 0 && GarminMarkerSymbols.Count > 0) 
            selectedidx = 0;

         if (selectedidx >= 0) {
            listView1.Items[selectedidx].Selected = true;
            listView1.Items[selectedidx].EnsureVisible();
         }
      }
   }
}
