using GMap.NET.MapProviders;

namespace GMap.NET.CoreExt.MapProviders {
   /// <summary>
   /// Daten, die für einen Provider gelten sollen
   /// </summary>
   public class MapProviderDefinition {

      public GMapProvider Provider;

      /// <summary>
      /// Providername
      /// </summary>
      public string ProviderName { get; protected set; }

      /// <summary>
      /// Name zum Anzeigen
      /// </summary>
      public string MapName { get; protected set; }

      /// <summary>
      /// zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)
      /// </summary>
      public double Zoom4Display { get; protected set; }

      /// <summary>
      /// kleinster zulässiger Zoom
      /// </summary>
      public int MinZoom { get; protected set; }

      /// <summary>
      /// größter zulässiger Zoom
      /// </summary>
      public int MaxZoom { get; protected set; }


      public MapProviderDefinition(string mapname, string provname, double zoom4display = 1, int minzoom = 10, int maxzoom = 24) {
         ProviderName = provname;
         MapName = mapname;
         Zoom4Display = zoom4display;
         MinZoom = minzoom;
         MaxZoom = maxzoom;

         if (string.IsNullOrEmpty(MapName))
            MapName = ProviderName;
      }

      public MapProviderDefinition() : this("", "") { }

      public MapProviderDefinition(MapProviderDefinition def) {
         ProviderName = def.ProviderName;
         MapName = def.MapName;
         Zoom4Display = def.Zoom4Display;
         MinZoom = def.MinZoom;
         MaxZoom = def.MaxZoom;
      }

      public override string ToString() {
         return string.Format("{0}, Zoom {1}..{2}", MapName, MinZoom, MaxZoom);
      }


   }
}
