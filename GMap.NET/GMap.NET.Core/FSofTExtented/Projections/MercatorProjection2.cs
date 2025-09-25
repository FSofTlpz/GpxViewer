using GMap.NET.Projections;

namespace GMap.NET.FSofTExtented.Projections {

   /// <summary>
   ///     The Mercator projection (mit eigener TILESIZE)
   /// </summary>
   public class MercatorProjection2 : MercatorProjection {
      public new static readonly MercatorProjection2 Instance = new MercatorProjection2();

      const int TILESIZEEXP = 9;
      const int TILESIZE = 1 << TILESIZEEXP;

      public override GSize TileSize {
         get;
      } = new GSize(TILESIZE, TILESIZE);

      public override GSize GetTileMatrixMinXY(int zoom) {
         return new GSize(0, 0);
      }

      public override GSize GetTileMatrixMaxXY(int zoom) {
         long xy = 1 << (zoom + 8 - TILESIZEEXP);
         return new GSize(xy - 1, xy - 1);
      }
   }
}
