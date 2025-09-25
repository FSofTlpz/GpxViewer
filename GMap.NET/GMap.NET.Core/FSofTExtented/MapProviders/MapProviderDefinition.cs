using GMap.NET.MapProviders;

namespace GMap.NET.FSofTExtented.MapProviders {
   /// <summary>
   /// Daten, die für einen Provider gelten sollen
   /// </summary>
   public class MapProviderDefinition {

      public GMapProvider Provider;

      /// <summary>
      /// Providername
      /// </summary>
      public string ProviderName { get; set; }

      /// <summary>
      /// Name zum Anzeigen
      /// </summary>
      public string MapName { get; set; }

      /// <summary>
      /// kleinster zulässiger Zoom
      /// </summary>
      public int MinZoom { get; set; }

      /// <summary>
      /// größter zulässiger Zoom
      /// </summary>
      public int MaxZoom { get; set; }

      ///// <summary>
      ///// wenn true, dann keine echte <see cref="MapProviderDefinition"/> sondern nur ein übergeordneter Gruppenname
      ///// </summary>
      //public bool IsProviderGroup =>
      //   string.IsNullOrEmpty(ProviderName);


      public MapProviderDefinition(string mapname, string provname = null, int minzoom = 10, int maxzoom = 24) {
         ProviderName = provname;
         MapName = mapname;
         MinZoom = minzoom;
         MaxZoom = maxzoom;

         if (string.IsNullOrEmpty(MapName))
            MapName = ProviderName;
      }

      public MapProviderDefinition() : this("", "") { }

      public MapProviderDefinition(MapProviderDefinition def) {
         ProviderName = def.ProviderName;
         MapName = def.MapName;
         MinZoom = def.MinZoom;
         MaxZoom = def.MaxZoom;
      }

      public override string ToString() {
         return string.Format("{0}, Zoom {1}..{2}", MapName, MinZoom, MaxZoom);
      }


   }
}
