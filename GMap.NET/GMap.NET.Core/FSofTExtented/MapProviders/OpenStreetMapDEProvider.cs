using GMap.NET.Projections;
using System;
using System.Net;

namespace GMap.NET.MapProviders {

   /// <summary>
   /// analog zum <see cref="OpenStreetMapProvider"/> aber mit angepasster URL für etwas "deutschere" Ausgaben
   /// </summary>
   public class OpenStreetMapDEProvider : OpenStreetMapProviderBase {

      public static readonly OpenStreetMapDEProvider Instance;

      static readonly string UrlFormat = "https://{0}.tile.openstreetmap.de/{1}/{2}/{3}.png";


      static OpenStreetMapDEProvider() {
         Instance = new OpenStreetMapDEProvider();
      }

      #region GMapProvider Members

      public override Guid Id {
         get;
      } = new Guid("5422BB0A-BA4C-439D-92F9-97F2DC601829");

      public override string Name {
         get;
      } = "OpenStreetMapDE";

      public override PureProjection Projection {
         get {
            return MercatorProjection.Instance;
         }
      }

      GMapProvider[] _overlays;

      public override GMapProvider[] Overlays {
         get {
            if (_overlays == null)
               _overlays = [this];
            return _overlays;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom) =>
         GetTileImageUsingHttp(MakeTileImageUrl(pos, zoom, string.Empty));

      protected override void InitializeWebRequest(WebRequest request) {
         base.InitializeWebRequest(request);
         if (!string.IsNullOrEmpty(YoursClientName))
            request.Headers.Add("X-Yours-client", YoursClientName);
      }

      #endregion

      public string YoursClientName { get; set; }

      string MakeTileImageUrl(GPoint pos, int zoom, string language) =>
         string.Format(UrlFormat, ServerLetters[GetServerNum(pos, 3)], zoom, pos.X, pos.Y);

   }

}
