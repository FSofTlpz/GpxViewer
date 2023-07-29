using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormClearCache : Form {

      public MapControl Map;

      public int ProviderIndex;

      /// <summary>
      /// 0 ohne löschen, 1 nur für akt. Provider gelöscht, 2 alles gelöscht
      /// </summary>
      public int Clear { get; protected set; }


      public FormClearCache() {
         InitializeComponent();
      }

      protected override void OnShown(EventArgs e) {
         base.OnShown(e);

         Clear = 0;

         if (Map == null)
            Close();
         if (ProviderIndex < 0)
            button_actMap.Enabled = false;

         button_back.Focus();
      }

      private void button_actMap_Click(object sender, EventArgs e) {
         Map.MapClearMemoryCache();
         showResult(Map.MapClearCache(ProviderIndex));
         Clear = 1;
         Close();
      }

      private void button_allMaps_Click(object sender, EventArgs e) {
         Map.MapClearMemoryCache();
         showResult(Map.MapClearCache(-1));
         Clear = 2;
         Close();
      }

      void showResult(int count) {
         MessageBox.Show(count + " Kartenteile gelöscht", "Ergebnis", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }


   }
}
