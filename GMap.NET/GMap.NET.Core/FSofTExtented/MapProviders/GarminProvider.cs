//#define LOCALDEBUG

using GarminImageCreator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace GMap.NET.FSofTExtented.MapProviders {

   /// <summary>
   /// für Garmin-IMG-Karten
   /// </summary>
   public class GarminProvider : GMapProviderWithHillshade, IHasJobManager, IArbitraryArea {

      readonly static Guid ID = new Guid("740224CE-E688-47B9-B472-8AA700C62CBE");
      const string PROVIDERNAME = "Garmin";

      public static readonly GarminProvider Instance;

      public override Guid Id => ID;

      public override string Name => PROVIDERNAME;

      static JobManager jobManager;


      static GarminProvider() {
         Instance = new GarminProvider();
         jobManager = new JobManager(PROVIDERNAME);
      }

      protected GarminProvider() {
         MaxZoom = 24;
         _projection = Projections.GarminProjection.Instance;
         StandardDbId = DbId;
      }

      /// <summary>
      /// def. eine Garmin-Kartenansicht (Name und Zugriff auf TDB's und TYP's)
      /// </summary>
      public class GarminMapDefinition : MultiUseBaseProviderDefinition {

         public const string IDDELTAFILE = "iddelta.garmin";

         static UniqueIDDelta uniqueIDDelta = null;

         /// <summary>
         /// Liste der TDB-Dateien (i.A. nur 1)
         /// </summary>
         public List<string> TDBfile { get; protected set; }

         /// <summary>
         /// Liste der TYP-Dateien (i.A. nur 1)
         /// </summary>
         public List<string> TYPfile { get; protected set; }

         /// <summary>
         /// Anpassungsfaktor der Textgröße (0 bedeutet: ohne Textausgabe)
         /// </summary>
         public double TextFactor { get; set; }

         /// <summary>
         /// Anpassungsfaktor der Linienbreite
         /// </summary>
         public double LineFactor { get; set; }

         /// <summary>
         /// Anpassungsfaktor der Symbolgröße
         /// </summary>
         public double SymbolFactor { get; set; }

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
         public GarminMapDefinition(string mapname,
                                    int minzoom,
                                    int maxzoom,
                                    string[] tdbfile,
                                    string[] typfile,
                                    double textfactor = 1.0,
                                    double linefactor = 1.0,
                                    double symbolfactor = 1.0,
                                    bool hillshading = false,
                                    byte hillshadingalpha = 100) :
            base(mapname, Instance.Name, minzoom, maxzoom) {

            TDBfile = new List<string>();
            for (int i = 0; i < tdbfile.Length; i++)
               TDBfile.Add(tdbfile[i]);

            TYPfile = new List<string>();
            for (int i = 0; i < typfile.Length; i++)
               TYPfile.Add(typfile[i]);

            TextFactor = textfactor;
            LineFactor = linefactor;
            SymbolFactor = symbolfactor;

            HillShading = hillshading;
            HillShadingAlpha = hillshadingalpha;

            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, IDDELTAFILE));

            string hash4delta = string.Empty;
            try {
               hash4delta = UniqueIDDelta.GetHashString(mapname + File.GetLastWriteTime(tdbfile[0]).Ticks + File.GetLastWriteTime(typfile[0]).Ticks,
                                                        ProviderHelper.GetBytesFromFile(tdbfile[0], 0, 1024));
               DbIdDelta = uniqueIDDelta.GetDelta(hash4delta, mapname);
            } catch {
               DbIdDelta = int.MinValue;
            }
         }

         public GarminMapDefinition(GarminMapDefinition def) :
            base(def.MapName, def.ProviderName, def.MinZoom, def.MaxZoom) {

            DbIdDelta = def.DbIdDelta;

            TDBfile = new List<string>();
            for (int i = 0; i < def.TDBfile.Count; i++)
               TDBfile.Add(def.TDBfile[i]);

            TYPfile = new List<string>();
            for (int i = 0; i < def.TYPfile.Count; i++)
               TYPfile.Add(def.TYPfile[i]);

            TextFactor = def.TextFactor;
            LineFactor = def.LineFactor;
            SymbolFactor = def.SymbolFactor;

            HillShading = def.HillShading;
            HillShadingAlpha = def.HillShadingAlpha;
         }

         public override string ToString() {
            return string.Format("{0}, {1} Karte/n", base.ToString(), TDBfile.Count);
         }
      }

      /// <summary>
      /// zum eigentlichen Zeichnen der Garmin-Tiles
      /// </summary>
      public static ImageCreator GarminImagecreator;

      List<GarminMapData> getMapData(GarminMapDefinition mapDefinition) {
         List<GarminMapData> mapData = new List<GarminMapData>();
         for (int i = 0; i < mapDefinition.TDBfile.Count && i < mapDefinition.TYPfile.Count; i++) {
            mapData.Add(new GarminMapData(mapDefinition.TDBfile[i],
                                          mapDefinition.TYPfile[i],
#if GMAP4SKIA
                                          "StdFont",
#else
                                          "Arial",
#endif
                                          mapDefinition.TextFactor,
                                          mapDefinition.LineFactor,
                                          mapDefinition.SymbolFactor));
         }
         return mapData;
      }

      public List<SearchObject> GetObjectInfo(double lon,
                                        double lat,
                                        double deltalon,
                                        double deltalat,
                                        double groundresolution,
                                        CancellationToken cancellationToken) {

         List<GarminMapData> mapData = getMapData((GarminMapDefinition)MapProviderDefinition);
         return GarminImagecreator?.GetObjectInfo(lon, lat, deltalon, deltalat, groundresolution, mapData, cancellationToken);
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
      /// erzeugt ein Bitmap der gewünschten Höhe und Breite oder liefert null
      /// <para>(Null ist verwendbar für "Abbruch" bzw. "Kein Ergebnis".)</para>
      /// <para>Zuerst werden die grundlegenden Objekte gezeichnet, dann ev. das Hillshading und danach darüber liegende Objekte.</para>
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <param name="def"></param>
      /// <returns>Bild oder null</returns>
      protected override Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         GarminMapDefinition mapDefinition = def as GarminMapDefinition;

         jobManager.AddJob(DbId - StandardDbId, p1, zoom, out uint jobid, out CancellationToken? cancellationtoken);

         Bitmap bm = new Bitmap(width, height);

         try {
            List<GarminMapData> mapData = getMapData(mapDefinition);

            //List<GarminImageCreator.GarminMapData> mapData = GarminImageCreator.GetGarminMapDefs();
            double[] groundresolution = new double[mapData.Count];
            for (int m = 0; m < mapData.Count; m++)
               groundresolution[m] = Projection.GetGroundResolution(zoom, mapData[m].GetMapCenterLat());

            object extdata = null;     // Objekt zum Übergeben der Liste der Garmintexte (die erst nach dem Hillshading ausgegeben werden)

            bool withHillshade = DEM != null && mapDefinition.HillShading;

            bool result = false;
            // den gewünschten Bereich auf das Bitmap zeichnen
            if (GarminImagecreator.DrawImage(bm,
                                             p1.Lng, p1.Lat,
                                             p2.Lng, p2.Lat,
                                             mapData,
                                             groundresolution,
                                             withHillshade ?
                                                 ImageCreator.PictureDrawing.beforehillshade :
                                                 ImageCreator.PictureDrawing.all,
                                             ref extdata,
                                             cancellationtoken)) {

               //Debug.WriteLine(">>> GetBitmap 1 (" + gMapTileId + ") withHillshade=" + withHillshade);


               // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
               if (withHillshade &&
                   bm != null) {
                  drawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, cancellationtoken);

                  // blockiert wahrscheinlich nicht ganz so stark wie die synchrone Methode ABER manchmal fehlt das Hillshading im Ergebnis
                  //DrawHillshadeAsync(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, cancellationtoken).Wait();

#if LOCALDEBUG
                  Debug.WriteLine(">>> DrawHillshade 2 (" + jobid + "), IsCancellationRequested=" + cancellationtoken.Value.IsCancellationRequested);
#endif

                  if (!cancellationtoken.Value.IsCancellationRequested)
                     // den gewünschten Bereich auf das Bitmap zeichnen
                     if (GarminImagecreator.DrawImage(bm,
                                                      p1.Lng, p1.Lat,
                                                      p2.Lng, p2.Lat,
                                                      mapData,              // wieder zurückliefern, falls inzwischen geändert
                                                      groundresolution,
                                                      ImageCreator.PictureDrawing.afterhillshade,
                                                      ref extdata,
                                                      cancellationtoken))
                        result = true;

#if LOCALDEBUG
               Debug.WriteLine(">>> DrawHillshade 3 (" + jobid + "), result=" + result);
#endif

               } else
                  result = true;
            }
            if (!result) {
               bm.Dispose();
               bm = null;
#if LOCALDEBUG
            Debug.WriteLine(nameof(GarminProvider) + "." + nameof(getBitmap) + " mit (" + p1 + ", zoom=" + zoom + ") abgebrochen.");
#endif
            }

         } catch (AggregateException aex) {
            string txt = "";
            foreach (var ex in aex.InnerExceptions)
               txt += ex.Message + Environment.NewLine;
            throw new Exception(nameof(GarminProvider) + "." + nameof(getBitmap) + "(): " + aex.Message + Environment.NewLine + txt);
         } catch (Exception ex) {
            throw new Exception(nameof(GarminProvider) + "." + nameof(getBitmap) + "(): " + ex.Message);
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