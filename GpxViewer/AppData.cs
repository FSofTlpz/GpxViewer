using GpxViewer.FSofTUtils;
using System.Collections.Generic;

namespace GpxViewer {
   public class AppData {

      PersistentDataXml data;

      /// <summary>
      /// bei Programmbeendigung verwendeter Kartenname
      /// </summary>
      public string LastMapname {
         get => data.Get(nameof(LastMapname), "");
         set => data.Set(nameof(LastMapname), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendeter Zoom
      /// </summary>
      public double LastZoom {
         get => data.Get(nameof(LastZoom), 14.0);
         set => data.Set(nameof(LastZoom), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Latitude
      /// </summary>
      public double LastLatitude {
         get => data.Get(nameof(LastLatitude), 51.25);
         set => data.Set(nameof(LastLatitude), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Longitude
      /// </summary>
      public double LastLongitude {
         get => data.Get(nameof(LastLongitude), 12.33);
         set => data.Set(nameof(LastLongitude), value);
      }

      /// <summary>
      /// Liste der gespeicherten Positionen (Zoom, Position, Name)
      /// </summary>
      public List<string> PositionList {
         get => data.GetList<string>(nameof(PositionList));
         set => data.SetList(nameof(PositionList), value);
      }

      /// <summary>
      /// Liste der gespeicherten GPX-"Vereinfachungen"
      /// </summary>
      public List<string> SimplifyDatasetList {
         get => data.GetList<string>(nameof(SimplifyDatasetList));
         set => data.SetList(nameof(SimplifyDatasetList), value);
      }


      public AppData(string name, bool local = false) {
         data = new PersistentDataXml(name, local);
      }

      public void Save() {
         data.Save();
      }

      public void Reload() {
         data = data.Load();
      }

   }
}
