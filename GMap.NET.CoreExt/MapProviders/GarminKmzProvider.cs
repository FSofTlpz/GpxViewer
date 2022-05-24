using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using GMap.NET.MapProviders;

namespace GMap.NET.CoreExt.MapProviders {

   /// <summary>
   /// für Garmin-IMG-Karten
   /// </summary>
   public class GarminKmzProvider : GMapProviderWithHillshade {
      public static readonly GarminKmzProvider Instance;

      readonly PureProjection _Projection = null;

      /// <summary>
      /// Standard-DbId des Providers
      /// </summary>
      static public int StandardDbId {
         get;
         protected set;
      }

      static GarminKmzProvider() {
         Instance = new GarminKmzProvider();
         StandardDbId = Instance.DbId;
      }

      GarminKmzProvider() {
         MaxZoom = 24;
         _Projection = Projections.GarminProjection.Instance;
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("314C2936-7350-4DEF-AA9E-E768491A57E6");

      public override Guid Id {
         get { return id; }
      }

      public override string Name {
         get {
            return "GarminKMZ";
         }
      }

      GMapProvider[] overlays;

      public override GMapProvider[] Overlays {
         get {
            if (overlays == null) {
               overlays = new GMapProvider[] { this };
            }
            return overlays;
         }
      }

      public override PureProjection Projection {
         get {
            return _Projection;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom) {
         var px1 = Projection.FromTileXYToPixel(pos);    // i.A. new GPoint((pos.X * TileSize.Width), (pos.Y * TileSize.Height));
         var px2 = px1;
         px1.Offset(0, Projection.TileSize.Height);   // Ecke links-oben (in Pixel des Gesamtbildes)
         px2.Offset(Projection.TileSize.Width, 0);    // Ecke rechts-unten (in Pixel des Gesamtbildes)

         PointLatLng p1 = Projection.FromPixelToLatLng(px1, zoom);
         PointLatLng p2 = Projection.FromPixelToLatLng(px2, zoom);
         PureImage img;
         try {

            img = getPureImage((int)Projection.TileSize.Width,
                               (int)Projection.TileSize.Height,
                               p1,
                               p2,
                               zoom);

         } catch (Exception ex) {
            throw new Exception("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      #endregion


      public class KmzMapDefinition : MapProviderDefinition {

         /// <summary>
         /// KMZ-Datei
         /// </summary>
         public string KmzFile { get; protected set; }

         /// <summary>
         /// spez. Delta für die DbId für diese Karte
         /// </summary>
         public int DbIdDelta { get; protected set; }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="name">Name der Karte</param>
         /// <param name="dbiddelta">Delta zur Standard-DbId des Providers</param>
         /// <param name="zoom4display">zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="kmzfile"></param>
         public KmzMapDefinition(string mapname,
                                 int dbiddelta,
                                 double zoom4display,
                                 int minzoom,
                                 int maxzoom,
                                 string kmzfile) :
            base(mapname, GarminKmzProvider.Instance.Name, zoom4display, minzoom, maxzoom) {
            KmzFile = kmzfile;
            DbIdDelta = dbiddelta;
         }

         public KmzMapDefinition(KmzMapDefinition def) :
            base(def.MapName, def.ProviderName, def.Zoom4Display, def.MinZoom, def.MaxZoom) {
            KmzFile = def.KmzFile;
         }

         public override string ToString() {
            return string.Format("{0}, {1}", base.ToString(), KmzFile);
         }

      }


      object lock_kmz = new object();


      /// <summary>
      /// die akt. verwendete KMZ-Karte
      /// </summary>
      public FSofTUtils.Geography.KmzMap KmzMap { get; protected set; }

      /// <summary>
      /// setzt eine (neue) KMZ-Datei
      /// </summary>
      /// <param name="kmzfile"></param>
      public void SetDef(KmzMapDefinition def) {
         FSofTUtils.Geography.KmzMap tmp = new FSofTUtils.Geography.KmzMap(def.KmzFile);
         lock (lock_kmz) {
            KmzMap = tmp;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns></returns>
      protected override Bitmap GetBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom) {
         Bitmap bm;

         lock (lock_kmz) {
            if (KmzMap != null)
               bm = KmzMap.GetImage(p1.Lng, p2.Lng, p1.Lat, p2.Lat, width, height);
            else
               bm = new Bitmap(width, height);
         }

         // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
         if (DEM != null &&
             DEM.WithHillshade &&
             bm != null)
            //DrawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha);
            DrawHillshadeAsync(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha).Wait(); // blockiert wahrscheinlich nicht ganz so stark wie die synchrone Methode

         return bm;
      }



   }

}