//#define LOCALDEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using GMap.NET.MapProviders;

namespace GMap.NET.FSofTExtented.MapProviders {

   /// <summary>
   /// für Multi-Karten
   /// </summary>
   public class MultiMapProvider : GMapProviderWithHillshade, IHasJobManager {

      readonly Guid ID = new Guid("D9AB3662-86A0-4EF6-BB97-C0CC1669B3A9");
      const string PROVIDERNAME = "MultiMap";

      public static readonly MultiMapProvider Instance;

      public override Guid Id => ID;

      public override string Name => PROVIDERNAME;

      static JobManager jobManager;

      static int staticStandardDbId;


      static MultiMapProvider() {
         Instance = new MultiMapProvider();
         jobManager = new JobManager(PROVIDERNAME);
         staticStandardDbId = Instance.DbId;
      }

      protected MultiMapProvider() {
         MaxZoom = 24;
         _projection = Projections.UTMProjection.Instance;
         StandardDbId = DbId;
      }

      /// <summary>
      /// def. einer Multi-Map
      /// </summary>
      public class MultiMapDefinition : MultiUseBaseProviderDefinition {

         public const string IDDELTAFILE = "iddelta.multi";

         static UniqueIDDelta uniqueIDDelta = null;

         public int Layer => MapProviderDefinitions.Length;

         /// <summary>
         /// Mit Hillshading ?
         /// </summary>
         public bool HillShading { get; set; }

         /// <summary>
         /// Transparenz für Hillshading
         /// </summary>
         public byte HillShadingAlpha { get; set; }

         /// <summary>
         /// Liste der verwendeten Kartendefinitionen
         /// </summary>
         public MapProviderDefinition[] MapProviderDefinitions { get; protected set; }



         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapname">Name der Gesamtkarte</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="mapproviderlst">Liste der Mapprovider</param>
         public MultiMapDefinition(string mapname,
                                   int minzoom,
                                   int maxzoom,
                                   IList<MapProviderDefinition> mapproviderlst,
                                   bool hillShading = false,
                                   byte hillShadingAlpha = 100) :
            base(mapname, Instance.Name, minzoom, maxzoom) {

            HillShading = hillShading;
            HillShadingAlpha = hillShadingAlpha;

            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, IDDELTAFILE));

            MapProviderDefinitions = new MapProviderDefinition[mapproviderlst.Count];
            for (int i = 0; i < mapproviderlst.Count; i++)
               MapProviderDefinitions[i] = mapproviderlst[i];

            string hash4delta = string.Empty;
            try {
               string sumnames = string.Empty;
               for (int i = 0; i < MapProviderDefinitions.Length; i++)
                  sumnames += MapProviderDefinitions[i].MapName;

               hash4delta = UniqueIDDelta.GetHashString(mapname + sumnames, []);
               DbIdDelta = uniqueIDDelta.GetDelta(hash4delta, mapname);
            } catch {
               DbIdDelta = int.MinValue;
            }
         }

         public MultiMapDefinition(MultiMapDefinition def) :
            this(def.MapName,
                 def.MinZoom,
                 def.MaxZoom,
                 def.MapProviderDefinitions,
                 def.HillShading,
                 def.HillShadingAlpha) { }

         /// <summary>
         /// entfernt die <see cref="MapProviderDefinition"/> auf der Pos. <paramref name="level"/>
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public bool RemoveLevel(int level) {
            if (0 <= level && level < MapProviderDefinitions.Length) {
               List<MapProviderDefinition> tmp = new(MapProviderDefinitions);
               tmp.RemoveAt(level);
               MapProviderDefinitions = tmp.ToArray();
               return true;
            }
            return false;
         }

         /// <summary>
         /// fügt eine <see cref="MapProviderDefinition"/> ein oder an
         /// <para>Danach steht die Def., wenn möglich, auf der Pos. mit dem Index <paramref name="level"/>.</para>
         /// </summary>
         /// <param name="def"></param>
         /// <param name="level"></param>
         public void InsertLevel(MapProviderDefinition def, int level = int.MaxValue) {
            if (0 <= level) {
               List<MapProviderDefinition> tmp = new List<MapProviderDefinition>(this.MapProviderDefinitions);
               if (level < MapProviderDefinitions.Length)
                  tmp.Insert(level, def);
               else
                  tmp.Add(def);
               MapProviderDefinitions = tmp.ToArray();
            }
         }

         /// <summary>
         /// ersetzt die <see cref="MapProviderDefinition"/> auf der Pos. <paramref name="level"/>
         /// </summary>
         /// <param name="def"></param>
         /// <param name="level"></param>
         /// <returns></returns>
         public bool NewLevel(MapProviderDefinition def, int level) {
            if (RemoveLevel(level)) {
               InsertLevel(def, level);
               return true;
            }
            return false;
         }

         public override string ToString() {
            return string.Format("{0}, {1} Layer", base.ToString(), Layer);
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

         PureImage img = null;
         try {

            img = ProviderHelper.GetPureImage(getBitmap((int)Projection.TileSize.Width,
                                                        (int)Projection.TileSize.Height,
                                                        pos,
                                                        p1,
                                                        p2,
                                                        zoom,
                                                        def));

            // I.A. wird das anzuzeigende Bild später so aus dem Stream erzeugt:
            //    System.Drawing.Image wimg = System.Drawing.Image.FromStream(img.Data, true, true);
            // Hier könnte also manipuliert werden.

         } catch (Exception ex) {
            throw new Exception("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      protected override Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         throw new NotImplementedException();
      }

      #endregion

      public void SetNewProjection(PureProjection projection) => _projection = projection;

      /// <summary>
      /// erzeugt ein Bitmap der gewünschten Höhe und Breite oder liefert null
      /// <para>(Null ist verwendbar für "Abbruch" bzw. "Kein Ergebnis".)</para>
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns>Bild oder null</returns>
      protected Bitmap getBitmap(int width, int height, GPoint pos, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         MultiMapDefinition mapDefinition = def as MultiMapDefinition;

         jobManager.AddJob(DbId - StandardDbId, p1, zoom, out uint jobid, out CancellationToken? cancellationtoken);

         Bitmap bm = new Bitmap(width, height);

         try {

            bool result = true;

            for (int i = 0; i < mapDefinition.MapProviderDefinitions.Length; i++) {
               if (!jobManager.IsCanceled(jobid)) {

                  GMapProvider provider = mapDefinition.MapProviderDefinitions[i].Provider;
                  if (provider != null) {
                     PureImage pimg = null;
                     Bitmap bm1 = null;
                     // PureImage für diesen Layer erzeugen
                     if (provider is MultiUseBaseProvider) {   // Provider die für mehrere Karten verwendet werden können
                        if (provider is GarminProvider) {

                           GarminProvider.GarminMapDefinition subdef = (GarminProvider.GarminMapDefinition)mapDefinition.MapProviderDefinitions[i];
                           if (provider is IArbitraryArea)
                              pimg = ((IArbitraryArea)provider).GetArbitraryArea(width, height, p1, p2, zoom, subdef);
                           else
                              throw new NotImplementedException(nameof(IArbitraryArea.GetArbitraryArea) + " for " + nameof(GarminProvider) + " not implemented");

                        } else if (provider is GarminKmzProvider) {

                           GarminKmzProvider.KmzMapDefinition subdef = (GarminKmzProvider.KmzMapDefinition)mapDefinition.MapProviderDefinitions[i];
                           if (provider is IArbitraryArea)
                              pimg = ((IArbitraryArea)provider).GetArbitraryArea(width, height, p1, p2, zoom, subdef);
                           else
                              throw new NotImplementedException(nameof(IArbitraryArea.GetArbitraryArea) + " for " + nameof(GarminKmzProvider) + " not implemented");

                        } else if (provider is WMSProvider) {

                           WMSProvider.WMSMapDefinition subdef = (WMSProvider.WMSMapDefinition)mapDefinition.MapProviderDefinitions[i];
                           if (provider is IArbitraryArea)
                              pimg = ((IArbitraryArea)provider).GetArbitraryArea(width, height, p1, p2, zoom, subdef);
                           else
                              throw new NotImplementedException(nameof(IArbitraryArea.GetArbitraryArea) + " for " + nameof(WMSProvider) + " not implemented");

                        } else if (provider is HillshadingProvider) {

                           HillshadingProvider.HillshadingMapDefinition subdef = (HillshadingProvider.HillshadingMapDefinition)mapDefinition.MapProviderDefinitions[i];
                           if (provider is IArbitraryArea)
                              pimg = ((IArbitraryArea)provider).GetArbitraryArea(width, height, p1, p2, zoom, subdef);
                           else
                              throw new NotImplementedException(nameof(IArbitraryArea.GetArbitraryArea) + " for " + nameof(HillshadingProvider) + " not implemented");

                        }
                        if (pimg != null)
                           bm1 = (Bitmap)ProviderHelper.GetImage(pimg);
                     } else {  // "normaler" Provider

                        // GetArbitraryArea muss nachgebildet werden:
                        bm1 = ProviderHelper.GetArbitraryBitmap(provider, width, height, p1, p2, zoom, null);

                     }

#if LOCALDEBUG
                     Debug.WriteLine(">>> " + pos + " " + i + " " + (pimg != null ? pimg.Data.Length : "null"));
#endif

                     // falls PureImage ex. auf das bisherige Bitmap zeichen
                     if (bm1 != null) {
                        using (Graphics g = Graphics.FromImage(bm))
                           g.DrawImageUnscaled(bm1, 0, 0);



                        // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
                        if (DEM != null &&
                            mapDefinition.HillShading &&
                            bm != null) {
                           drawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, cancellationtoken);
                        }



                     } else
                        result = false;
                  } else
                     result = false;
               } else
                  result = false;

               if (!result)
                  break;
            }

            if (!result) {
               bm?.Dispose();
               bm = null;
            }

         } catch (AggregateException aex) {
            string txt = "";
            foreach (var ex in aex.InnerExceptions)
               txt += ex.Message + Environment.NewLine;
            throw new Exception(nameof(MultiMapProvider) + "." + nameof(getBitmap) + "(): " + aex.Message + Environment.NewLine + txt);
         } catch (Exception ex) {
            throw new Exception(nameof(MultiMapProvider) + "." + nameof(getBitmap) + "(): " + ex.Message);
         }

         jobManager.RemoveJob(jobid);

         return bm;
      }

      PureImage getArbitraryArea(GMapProvider provider, int width, int height, GPoint pos, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         if (provider is IArbitraryArea)
            return ((IArbitraryArea)provider).GetArbitraryArea(width, height, p1, p2, zoom, def);
         else
            throw new NotImplementedException(nameof(IArbitraryArea.GetArbitraryArea) + " not implemented");
      }

      /// <summary>
      /// liefert ein Dictionary mit den einem DeltaDbId-Array je Providername (für <see cref="MultiUseBaseProvider"/>)
      /// </summary>
      /// <returns></returns>
      protected Dictionary<string, List<int>> sampleDeltas() {
         MultiMapDefinition mdef = (MultiMapDefinition)MapProviderDefinition;
         MapProviderDefinition[] defs = ((MultiMapDefinition)MapProviderDefinition).MapProviderDefinitions;

         Dictionary<string, List<int>> dbideltas = new Dictionary<string, List<int>>();
         for (int i = 0; i < mdef.Layer; i++) {
            if (defs[i].Provider is MultiUseBaseProvider &&
                defs[i].Provider is IHasJobManager) {
               MultiUseBaseProvider prov = (MultiUseBaseProvider)defs[i].Provider;
               int deltaid = prov.DeltaDbId;
               if (!dbideltas.ContainsKey(prov.Name))
                  dbideltas.Add(prov.Name, new List<int>());
               dbideltas[prov.Name].Add(deltaid);
            }
         }
         return dbideltas;
      }

      #region IHasJobManager

      public void SetJobFilter(int[] deltaDbId) => SetJobFilter(deltaDbId, -1);

      public void SetJobFilter(int zoom) => SetJobFilter(null, zoom);

      public void SetJobFilter(int[] deltaDbId, int zoom) {
         jobManager?.SetJobFilter(deltaDbId, zoom);
         if (MapProviderDefinition != null) {
            MapProviderDefinition[] defs = ((MultiMapDefinition)MapProviderDefinition).MapProviderDefinitions;
            Dictionary<string, List<int>> dbideltas = sampleDeltas();
            foreach (var item in dbideltas) {
               GMapProvider prov = ProviderHelper.GetProvider4Providername(item.Key);
               if (prov is IHasJobManager)
                  ((IHasJobManager)prov).SetJobFilter(item.Value.ToArray(), zoom);
            }
         }
      }

      public void CancelAllJobs(DateTime time) {
         jobManager?.CancelAll(time);
         if (MapProviderDefinition != null)
            foreach (MapProviderDefinition mmd in ((MultiMapDefinition)MapProviderDefinition).MapProviderDefinitions) {
               if (mmd.Provider is IHasJobManager)
                  ((IHasJobManager)mmd.Provider).CancelAllJobs(time);
            }
      }

      #endregion

      public override string ToString() =>
         base.ToString() + " StandardDbId=" + (StandardDbId > 0 ? StandardDbId : Instance.DbId);

   }

}