using GMap.NET.MapProviders;
using System.Drawing;
using System.Threading;

namespace GMap.NET.FSofTExtented.MapProviders {
   /// <summary>
   /// Basisprovider die für mehr als eine Karte verwendet werden
   /// <para>Dafür muss i.W. die DbId angepasst werden können.</para>
   /// </summary>
   public abstract class MultiUseBaseProvider : GMapProvider {

      protected PureProjection _projection = null;

      #region GMapProvider Members

      GMapProvider[] overlays;

      public override GMapProvider[] Overlays {
         get {
            if (overlays == null)
               overlays = [this];
            return overlays;
         }
      }

      public override PureProjection Projection => _projection;

      public override PureImage GetTileImage(GPoint pos, int zoom) => GetTileImageWithMapDefinition(pos, zoom, MapProviderDefinition);

      #endregion


      /// <summary>
      /// Standard-DbId des Providers
      /// </summary>
      public int StandardDbId { get; protected set; }

      /// <summary>
      /// Delta zur Standard-DbId des Providers
      /// </summary>
      public int DeltaDbId { get; protected set; }


      public class MultiUseBaseProviderDefinition : MapProviderDefinition {

         /// <summary>
         /// spez. Delta für die DbId für diese Karte
         /// </summary>
         public int DbIdDelta { get; protected set; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapname">Name der Gesamtkarte</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="tdbfile">Liste der TDB-Dateien der zusammengeführten Garminkarten</param>
         /// <param name="typfile">Liste der TYP-Dateien der zusammengeführten Garminkarten</param>
         /// <param name="localchachelevels">Maplevels, die in den lokalen Cache aufgenommen werden sollen</param>
         /// <param name="maxsubdivs">max. Anzahl Subdivs je Bild</param>
         /// <param name="textfactor">Anpassungsfaktor der Textgröße (0 bedeutet: ohne Textausgabe)</param>
         /// <param name="linefactor">Anpassungsfaktor der Linienbreite</param>
         /// <param name="symbolfactor">Anpassungsfaktor der Symbolgröße</param>
         public MultiUseBaseProviderDefinition(string mapname,
                                               string providername,
                                               int minzoom,
                                               int maxzoom) :
            base(mapname, providername, minzoom, maxzoom) { }

      }






      MapProviderDefinition _actualdef;

      public MapProviderDefinition MapProviderDefinition {
         get => Interlocked.Exchange(ref _actualdef, _actualdef);
         set => Interlocked.Exchange(ref _actualdef, value);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="def"></param>
      public void SetDef(MapProviderDefinition def) => Interlocked.Exchange(ref _actualdef, def);

      /// <summary>
      /// setzt eine andere DbId
      /// </summary>
      /// <param name="dbid"></param>
      /// <returns></returns>
      public int ChangeDbId(int dbid) {
         DeltaDbId = dbid - StandardDbId;
         return ProviderHelper.ChangeDbId(this, dbid);
      }

      public MapProviderDefinition GetDefinition() => Interlocked.Exchange(ref _actualdef, _actualdef);

      /// <summary>
      /// erzeugt mit <see cref="getBitmap(int, int, PointLatLng, PointLatLng, int)"/> das Bitmap des abgeleiteten Providers und daraus das <see cref="PureImage"/>
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns></returns>
      protected PureImage getPureImage(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         Bitmap bm = getBitmap(width, height, p1, p2, zoom, def);
         if (bm != null)
            return ProviderHelper.GetPureImage(bm);
         return null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="width">Breite des Bitmaps</param>
      /// <param name="height">Höhe des Bitmaps</param>
      /// <param name="p1">Ecke links oben</param>
      /// <param name="p2">Ecke rechts unten</param>
      /// <param name="zoom">Zoom</param>
      /// <param name="def">Definition</param>
      /// <returns></returns>
      abstract protected Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def);

      abstract public PureImage GetTileImageWithMapDefinition(GPoint pos, int zoom, MapProviderDefinition def);

      public override string ToString() =>
         base.ToString() + " DeltaDbId=" + DeltaDbId;

   }
}
