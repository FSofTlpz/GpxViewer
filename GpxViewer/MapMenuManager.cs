using GMap.NET.FSofTExtented.MapProviders;
using GpxViewer.Common;

namespace GpxViewer {
   internal class MapMenuManager {

      public class ActivateIdxArgs {

         public readonly int ProviderIdx;

         public ActivateIdxArgs(int idx) {
            ProviderIdx = idx;
         }
      }

      public event EventHandler<ActivateIdxArgs>? ActivateIdx;

      /// <summary>
      /// Programmdaten
      /// </summary>
      readonly AppData appData;

      /// <summary>
      /// übergeordnetes Item für alle Menüitems
      /// </summary>
      readonly ToolStripMenuItem masteritem;

      readonly int maxidx = -1;

      /// <summary>
      /// IDX-Liste der zuletzt verwendeten Karten
      /// </summary>
      readonly List<int> lastusedmapsidx;

      /// <summary>
      /// max. Anzahl der zuletzt verwendeten Karten
      /// </summary>
      readonly int lastusedmapsmax = 0;

      readonly int lastusedstartidx = -1;

      /// <summary>
      /// Liste der Providerdefinitionen
      /// </summary>
      readonly IList<MapProviderDefinition> providerdefs;

      int _actidx = -1;

      public int ActualProviderIdx {
         get => _actidx;
         set {
            if (0 <= value && value <= maxidx) {
               insertLastUsedMaps(value);
               ActivateIdx?.Invoke(this, new ActivateIdxArgs(value));
               _actidx = value;

               masteritem.Text = "Karte: [" + providerdefs[_actidx].MapName + "]";

               if (lastusedmapsidx.Count > 0) {
                  List<string> mapnames = [];
                  for (int i = 0; i < lastusedmapsidx.Count; i++)
                     mapnames.Add(providerdefs[lastusedmapsidx[i]].MapName);
                  appData.LastUsedMapnames = mapnames;
               }
            }
         }
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="config">Konfigurationsdaten</param>
      /// <param name="appData">Programmdaten</param>
      /// <param name="masteritem">übergeordnetes Menüitem</param>
      /// <param name="providerdefs">Liste der Providerdefinitionen</param>
      /// <param name="providxpaths">Liste der Indexpfade für das Providermenü</param>
      public MapMenuManager(Config config,
                            AppData appData,
                            ToolStripMenuItem masteritem,
                            IList<MapProviderDefinition> providerdefs,
                            IList<int[]> providxpaths) {
         this.appData = appData;
         this.masteritem = masteritem;
         this.providerdefs = providerdefs;
         maxidx = providerdefs.Count - 1;
         lastusedmapsidx = [];

         masteritem.DropDownItems.Clear();
         for (int provideridx = 0; provideridx < providerdefs.Count; provideridx++) {
            // ev. für übergeordnete Providergruppen noch die Menüitems erzeugen
            ToolStripMenuItem? mi = masteritem;
            for (int level = 1; level < providxpaths[provideridx].Length; level++) {
               if (level < providxpaths[provideridx].Length - 1) {   // Providergroup
                  int groupidx = providxpaths[provideridx][level];

                  ToolStripMenuItem? mi4group = null;
                  int destidx = -1;
                  for (int i = 0; i < mi.DropDownItems.Count; i++) {
                     if ((mi.DropDownItems[i] as ToolStripMenuItem).Tag == null) {
                        if (++destidx == groupidx) {
                           mi4group = mi.DropDownItems[i] as ToolStripMenuItem;
                           break;
                        }
                     }
                  }

                  if (mi4group == null) {
                     mi.DropDownItems.Add(config.ProviderGroupName(providxpaths[provideridx], level + 1));
                     mi = mi.DropDownItems[mi.DropDownItems.Count - 1] as ToolStripMenuItem;
                  } else
                     mi = mi4group;

               } else {                                              // Provideritem

                  //ToolStripItem it = mi.DropDownItems.Add(config.MapName(providxpaths[provideridx]));
                  ToolStripItem it = mi.DropDownItems.Add(providerdefs[provideridx].MapName);
                  it.Tag = provideridx;
                  it.Click += (object? s, EventArgs ea) => {  // Eventhandler setzt den aktuellen Provider-IDX auf den Idx im Tag
                     if (s is ToolStripMenuItem tsmi &&
                         tsmi.Tag != null)
                        ActualProviderIdx = Convert.ToInt32(tsmi.Tag);
                  };

               }
            }
         }

         lastusedmapsmax = Math.Max(0, config.LastUsedMapsCount);
         if (lastusedmapsmax > 0) {
            masteritem.DropDownItems.Add(new ToolStripSeparator());
            masteritem.DropDownItems[masteritem.DropDownItems.Count - 1].Tag = -1;

            lastusedstartidx = masteritem.DropDownItems.Count;

            List<string> mapnames = appData.LastUsedMapnames;
            if (mapnames != null)
               for (int i = mapnames.Count - 1; i >= 0; i--) {
                  int idx = getIdx4Mapname(mapnames[i]);
                  if (0 <= idx)
                     insertLastUsedMaps(idx);
               }
         }
      }

      void insertLastUsedMaps(int mapidx) {
         if (0 <= mapidx && mapidx <= maxidx) {
            lastusedmapsidx.Insert(0, mapidx);

            for (int i = lastusedmapsidx.Count - 1; i > 0; i--)
               if (lastusedmapsidx[i] == mapidx)
                  lastusedmapsidx.RemoveAt(i);

            // max. Anzahl einhalten
            while (lastusedmapsidx.Count > lastusedmapsmax)
               lastusedmapsidx.RemoveAt(lastusedmapsidx.Count - 1);

            while (lastusedmapsidx.Count > masteritem.DropDownItems.Count - lastusedstartidx) {
               masteritem.DropDownItems.Add(string.Empty);
               masteritem.DropDownItems[masteritem.DropDownItems.Count - 1].Click += (object? s, EventArgs ea) => {
                  ToolStripMenuItem? mi = s as ToolStripMenuItem;
                  if (mi.Tag != null)
                     ActualProviderIdx = Convert.ToInt32(mi.Tag);
               };
            }

            for (int i = lastusedstartidx, j = 0; i < masteritem.DropDownItems.Count; i++, j++) {
               int idx = lastusedmapsidx[i - lastusedstartidx];
               masteritem.DropDownItems[i].Tag = idx;
               masteritem.DropDownItems[i].Text = providerdefs[idx].MapName;

               // Shortcutkey setzen
               if (0 < j && j <= 10) { // ab dem 2. Item (1. ist sinnlos, da aktuell)
                  Keys keys = Keys.D1;
                  switch (j) {
                     case 1: keys = Keys.D1; break;
                     case 2: keys = Keys.D2; break;
                     case 3: keys = Keys.D3; break;
                     case 4: keys = Keys.D4; break;
                     case 5: keys = Keys.D5; break;
                     case 6: keys = Keys.D6; break;
                     case 7: keys = Keys.D7; break;
                     case 8: keys = Keys.D8; break;
                     case 9: keys = Keys.D9; break;
                  }
                  (masteritem.DropDownItems[i] as ToolStripMenuItem).ShortcutKeys = keys | Keys.Control;
               }
            }
         }
      }

      int getIdx4Mapname(string mapname) {
         for (int j = 0; j < providerdefs.Count; j++)
            if (providerdefs[j].MapName == mapname)
               return j;
         return -1;
      }

   }
}
