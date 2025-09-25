using FSofTUtils.Drawing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GMap.NET.FSofTExtented.MapProviders {
   public class HillshadingProvider : GMapProviderWithHillshade/*MultiUseBaseProvider*/, IHasJobManager, IArbitraryArea {

      readonly Guid ID = new Guid("85493AF6-3F45-4BE2-A8EA-262ED16C243E");
      const string PROVIDERNAME = "Hillshading";

      public static readonly HillshadingProvider Instance;

      public override Guid Id => ID;

      public override string Name => PROVIDERNAME;

      static JobManager jobManager;


      static HillshadingProvider() {
         Instance = new HillshadingProvider();
         jobManager = new JobManager(PROVIDERNAME);
      }

      protected HillshadingProvider() {
         MaxZoom = 24;
         _projection = Projections.GarminProjection.Instance;
         StandardDbId = DbId;
      }

      /// <summary>
      /// Def. einer Hillshading-Map
      /// </summary>
      public class HillshadingMapDefinition : MultiUseBaseProviderDefinition {

         public const string IDDELTAFILE = "iddelta.hillshading";

         static UniqueIDDelta uniqueIDDelta = null;

         public int Alpha { get; protected set; }

         public FSofTUtils.Geography.DEM.DemData DEM { get; protected set; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapname">Name der Gesamtkarte</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="alpha">Alpha</param>
         public HillshadingMapDefinition(string mapname,
                                         int minzoom,
                                         int maxzoom,
                                         FSofTUtils.Geography.DEM.DemData dem,
                                         int alpha) :
            base(mapname, Instance.Name, minzoom, maxzoom) {
            DEM = dem;
            Alpha = alpha;

            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, IDDELTAFILE));

            string hash4delta = string.Empty;
            try {
               hash4delta = UniqueIDDelta.GetHashString(mapname + alpha.ToString(), []);
               DbIdDelta = uniqueIDDelta.GetDelta(hash4delta, mapname);
            } catch {
               DbIdDelta = int.MinValue;
            }
         }

         public HillshadingMapDefinition(HillshadingMapDefinition def) :
            this(def.MapName,
                 def.MinZoom,
                 def.MaxZoom,
                 def.DEM,
                 def.Alpha) { }

         public override string ToString() => string.Format("{0}, Alpha={1}", base.ToString(), Alpha);

      }

      #region MultiUseBaseProvider

      public override PureImage GetTileImageWithMapDefinition(GPoint pos, int zoom, MapProviderDefinition def) {
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
                               zoom,
                               def);

            // I.A. wird das anzuzeigende Bild später so aus dem Stream erzeugt:
            //    System.Drawing.Image wimg = System.Drawing.Image.FromStream(img.Data, true, true);
            // Hier könnte also manipuliert werden.

         } catch (Exception ex) {
            throw new Exception("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      /// <summary>
      /// Damit muss die Karte gezeichnet werden.
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="zoom"></param>
      /// <param name="def"></param>
      /// <returns></returns>
      protected override Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         HillshadingMapDefinition specdef = (HillshadingMapDefinition)def;

         jobManager.AddJob(DbId - StandardDbId, p1, zoom, out uint jobid, out CancellationToken? cancellationtoken);

         Bitmap bm = new Bitmap(width, height);
         try {
            bool result = false;

            specdef.DEM.WithHillshade = true;   // sonst sinnlos

            if (specdef != null &&
                specdef.DEM != null &&
                specdef.DEM.WithHillshade &&
                bm != null) {

               int a = specdef.Alpha;
               //a = 0;

               drawHillshade(specdef.DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, a, cancellationtoken);
               // blockiert wahrscheinlich nicht ganz so stark wie die synchrone Methode ABER manchmal fehlt das Hillshading im Ergebnis
               //drawHillshadeAsync(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, CancellationToken).Wait();

               result = true;
            }

            if (!result) {
               bm.Dispose();
               bm = null;
            }

         } catch (AggregateException aex) {
            string txt = "";
            foreach (var ex in aex.InnerExceptions)
               txt += ex.Message + Environment.NewLine;
            throw new Exception(nameof(HillshadingProvider) + "." + nameof(getBitmap) + "(): " + aex.Message + Environment.NewLine + txt);
         } catch (Exception ex) {
            throw new Exception(nameof(HillshadingProvider) + "." + nameof(getBitmap) + "(): " + ex.Message);
         }

         jobManager.RemoveJob(jobid);

         return bm;
      }

      #endregion

      #region IArbitraryArea

      public PureImage GetArbitraryArea(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) =>
         getPureImage(width, height, p1, p2, zoom, def);

      #endregion

      #region IHasJobManager

      public void SetJobFilter(int[] deltaDbId) => SetJobFilter(deltaDbId, -1);

      public void SetJobFilter(int zoom) => SetJobFilter(null, zoom);

      public void SetJobFilter(int[] deltaDbId, int zoom) => jobManager?.SetJobFilter(deltaDbId, zoom);

      public void CancelAllJobs(DateTime time) => jobManager?.CancelAll(time);

      #endregion

   }
}
