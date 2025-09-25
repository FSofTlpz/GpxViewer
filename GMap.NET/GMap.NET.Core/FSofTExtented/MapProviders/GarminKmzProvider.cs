using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace GMap.NET.FSofTExtented.MapProviders {

   /// <summary>
   /// für Garmin-IMG-Karten
   /// </summary>
   public class GarminKmzProvider : GMapProviderWithHillshade, IHasJobManager, IArbitraryArea {

      readonly static Guid ID = new("314C2936-7350-4DEF-AA9E-E768491A57E6");
      const string PROVIDERNAME = "GarminKMZ";

      readonly public static GarminKmzProvider Instance;

      public override Guid Id => ID;

      public override string Name => PROVIDERNAME;

      static JobManager jobManager;


      static GarminKmzProvider() {
         Instance = new GarminKmzProvider();
         jobManager = new JobManager(PROVIDERNAME);
      }

      GarminKmzProvider() {
         MaxZoom = 24;
         _projection = Projections.GarminProjection.Instance;
         StandardDbId = DbId;
      }

      public class KmzMapDefinition : MultiUseBaseProviderDefinition {

         public const string IDDELTAFILE = "iddelta.garminKmz";

         static UniqueIDDelta uniqueIDDelta = null;


         /// <summary>
         /// KMZ-Datei
         /// </summary>
         public string KmzFile { get; set; }

         /// <summary>
         /// Mit Hillshading ?
         /// </summary>
         public bool HillShading { get; set; }

         /// <summary>
         /// Transparenz für Hillshading
         /// </summary>
         public byte HillShadingAlpha { get; set; }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="name">Name der Karte</param>
         /// <param name="dbiddelta">Delta zur Standard-DbId des Providers</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="kmzfile"></param>
         public KmzMapDefinition(string mapname,
                                 int minzoom,
                                 int maxzoom,
                                 string kmzfile,
                                 bool hillShading = false,
                                 byte hillShadingAlpha = 100) :
            base(mapname, Instance.Name, minzoom, maxzoom) {
            KmzFile = kmzfile;

            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, IDDELTAFILE));

            string hash4delta = string.Empty;
            try {
               hash4delta = UniqueIDDelta.GetHashString(mapname + File.GetLastWriteTime(kmzfile).Ticks,
                                                        ProviderHelper.GetBytesFromFile(kmzfile, 0, 1024));
               DbIdDelta = uniqueIDDelta.GetDelta(hash4delta, mapname);
            } catch {
               DbIdDelta = int.MinValue;
            }

            HillShading = hillShading;
            HillShadingAlpha = hillShadingAlpha;
         }

         public KmzMapDefinition(KmzMapDefinition def) :
            base(def.MapName, def.ProviderName, def.MinZoom, def.MaxZoom) {
            KmzFile = def.KmzFile;
            DbIdDelta = def.DbIdDelta;
            HillShading = def.HillShading;
            HillShadingAlpha = def.HillShadingAlpha;
         }

         public override string ToString() {
            return string.Format("{0}, {1}", base.ToString(), KmzFile);
         }

      }

      string _kmzfile = string.Empty;

      string kmzFile {
         get => Interlocked.Exchange(ref _kmzfile, _kmzfile);
         set => Interlocked.Exchange(ref _kmzfile, value);
      }

      FSofTUtils.Geography.KmzMap _kmzmap;

      /// <summary>
      /// die akt. verwendete KMZ-Karte
      /// </summary>
      FSofTUtils.Geography.KmzMap kmzMap {
         get => Interlocked.Exchange(ref _kmzmap, _kmzmap);
         set => Interlocked.Exchange(ref _kmzmap, value);
      }

      /// <summary>
      /// setzt eine (neue) KMZ-Datei
      /// </summary>
      /// <param name="kmzfile"></param>
      public void SetDef(KmzMapDefinition def) {
         base.SetDef(def);
         if (kmzFile != def.KmzFile) { // wenn die Datei gleich ist unnötig
            kmzFile = def.KmzFile;
            kmzMap = new FSofTUtils.Geography.KmzMap(kmzFile);
         }
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

         } catch (Exception ex) {
            throw new Exception("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <param name="def"></param>
      /// <returns></returns>
      protected override Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         KmzMapDefinition mapDefinition = def as KmzMapDefinition;

         jobManager.AddJob(DbId - StandardDbId, p1, zoom, out uint jobid, out CancellationToken? cancellationtoken);

         Bitmap bm = new Bitmap(width, height);

         try {
            FSofTUtils.Geography.KmzMap kmz = null;
            if (def != null &&
                (kmzFile != mapDefinition.KmzFile))
               kmz = new FSofTUtils.Geography.KmzMap(mapDefinition.KmzFile);
            else
               kmz = kmzMap;

            if (kmz != null)
               bm = kmz.GetImage(p1.Lng, p2.Lng, p1.Lat, p2.Lat, width, height);

            // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
            if (DEM != null && 
                mapDefinition.HillShading &&
                bm != null) {
               drawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, cancellationtoken);
            }

         } catch (AggregateException aex) {
            string txt = "";
            foreach (var ex in aex.InnerExceptions)
               txt += ex.Message + Environment.NewLine;
            throw new Exception(nameof(GarminKmzProvider) + "." + nameof(getBitmap) + "(): " + aex.Message + Environment.NewLine + txt);
         } catch (Exception ex) {
            throw new Exception(nameof(GarminKmzProvider) + "." + nameof(getBitmap) + "(): " + ex.Message);
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