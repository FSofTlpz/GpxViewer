using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormClearCache : Form {

      public MapControl Map;

      public int ProviderIndex;


      public FormClearCache() {
         InitializeComponent();
      }

      protected override void OnShown(EventArgs e) {
         base.OnShown(e);
         if (Map == null)
            Close();
         if (ProviderIndex < 0)
            button_actMap.Enabled = false;

         button_back.Focus();
      }

      private void button_actMap_Click(object sender, EventArgs e) {
         Map.MapClearMemoryCache();
         showResult(Map.MapClearCache(ProviderIndex));
         Close();
      }

      private void button_allMaps_Click(object sender, EventArgs e) {
         Map.MapClearMemoryCache();
         showResult(Map.MapClearCache(-1));
         Close();
      }

      void showResult(int count) {
         MessageBox.Show(count + " Kartenteile gelöscht", "Ergebnis", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }


   }
}
