using GMap.NET.Projections;

namespace GMap.NET.FSofTExtented.Projections {

   /// <summary>
   /// Ableitung nur wegen veränderter <see cref="TileSize"/>
   /// </summary>
   public class GarminProjection : MercatorProjection {

      const int TILESIZEEXP = 9;
      const int TILESIZE = 1 << TILESIZEEXP;


      public new static readonly GarminProjection Instance = new GarminProjection();

      public override GSize TileSize {
         get;
      } = new GSize(TILESIZE, TILESIZE);

      /// <summary>
      /// min. Tileindex für x und y
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixMinXY(int zoom) {
         return new GSize(0, 0);
      }

      /// <summary>
      /// max. Tileindex für x und y
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixMaxXY(int zoom) {
         long xy = 1 << (zoom + 8 - TILESIZEEXP);
         return new GSize(xy - 1, xy - 1);
      }


   }
}
