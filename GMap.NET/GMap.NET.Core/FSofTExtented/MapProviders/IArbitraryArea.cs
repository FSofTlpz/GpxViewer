namespace GMap.NET.FSofTExtented.MapProviders {
   /// <summary>
   /// enthält eine Funktion um ein <see cref="PureImage"/> für einen beliebigen rechteckigen Bereich zu liefern
   /// </summary>
   public interface IArbitraryArea {

      public PureImage GetArbitraryArea(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def);

   }
}
