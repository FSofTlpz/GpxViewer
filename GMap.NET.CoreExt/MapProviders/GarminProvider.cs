using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using GMap.NET.MapProviders;

namespace GMap.NET.CoreExt.MapProviders {

   /// <summary>
   /// für Garmin-IMG-Karten
   /// </summary>
   public class GarminProvider : GMapProviderWithHillshade {
      public static readonly GarminProvider Instance;

      readonly PureProjection _Projection = null;


      static GarminProvider() {
         Instance = new GarminProvider();
         StandardDbId = Instance.DbId;
      }

      protected GarminProvider() {
         MaxZoom = 24;
         _Projection = Projections.GarminProjection.Instance;
      }

      /// <summary>
      /// Standard-DbId des Providers
      /// </summary>
      static public int StandardDbId {
         get;
         protected set;
      }

      #region GMapProvider Members

      Guid id = new Guid("740224CE-E688-47B9-B472-8AA700C62CBE");

      public override Guid Id {
         get { return id; }
      }

      public override string Name {
         get {
            return "Garmin";
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

            // I.A. wird das anzuzeigende Bild später so aus dem Stream erzeugt:
            //    System.Drawing.Image wimg = System.Drawing.Image.FromStream(img.Data, true, true);
            // Hier könnte also manipuliert werden.

         } catch (Exception ex) {
            throw new Exception("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      #endregion

      /// <summary>
      /// def. eine Garmin-Kartenansicht (Name und Zugriff auf TDB's und TYP's)
      /// </summary>
      public class GarminMapDefinitionData : MapProviderDefinition {

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
         public double TextFactor { get; protected set; }

         /// <summary>
         /// Anpassungsfaktor der Linienbreite
         /// </summary>
         public double LineFactor { get; protected set; }

         /// <summary>
         /// Anpassungsfaktor der Symbolgröße
         /// </summary>
         public double SymbolFactor { get; protected set; }

         /// <summary>
         /// spez. Delta für die DbId für diese Karte
         /// </summary>
         public int DbIdDelta { get; protected set; }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapname">Name der Gesamtkarte</param>
         /// <param name="dbiddelta">Delta zur Standard-DbId des Providers</param>
         /// <param name="zoom4display">zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="tdbfile">Liste der TDB-Dateien der zusammengeführten Garminkarten</param>
         /// <param name="typfile">Liste der TYP-Dateien der zusammengeführten Garminkarten</param>
         /// <param name="localchachelevels">Maplevels, die in den lokalen Cache aufgenommen werden sollen</param>
         /// <param name="maxsubdivs">max. Anzahl Subdivs je Bild</param>
         /// <param name="textfactor">Anpassungsfaktor der Textgröße (0 bedeutet: ohne Textausgabe)</param>
         /// <param name="linefactor">Anpassungsfaktor der Linienbreite</param>
         /// <param name="symbolfactor">Anpassungsfaktor der Symbolgröße</param>
         public GarminMapDefinitionData(string mapname,
                                        int dbiddelta,
                                        double zoom4display,
                                        int minzoom,
                                        int maxzoom,
                                        string[] tdbfile,
                                        string[] typfile,
                                        double textfactor = 1.0,
                                        double linefactor = 1.0,
                                        double symbolfactor = 1.0) :
            base(mapname, Instance.Name, zoom4display, minzoom, maxzoom) {

            TDBfile = new List<string>();
            for (int i = 0; i < tdbfile.Length; i++)
               TDBfile.Add(tdbfile[i]);

            TYPfile = new List<string>();
            for (int i = 0; i < typfile.Length; i++)
               TYPfile.Add(typfile[i]);

            TextFactor = textfactor;
            LineFactor = linefactor;
            SymbolFactor = symbolfactor;

            DbIdDelta = dbiddelta;
         }

         public GarminMapDefinitionData(GarminMapDefinitionData def) :
            base(def.MapName, def.ProviderName, def.Zoom4Display, def.MinZoom, def.MaxZoom) {

            TDBfile = new List<string>();
            for (int i = 0; i < def.TDBfile.Count; i++)
               TDBfile.Add(def.TDBfile[i]);

            TYPfile = new List<string>();
            for (int i = 0; i < def.TYPfile.Count; i++)
               TYPfile.Add(def.TYPfile[i]);

            TextFactor = def.TextFactor;
            LineFactor = def.LineFactor;
            SymbolFactor = def.SymbolFactor;
         }

         public override string ToString() {
            return string.Format("{0}, {1} Karte/n", base.ToString(), TDBfile.Count);
         }
      }


      /// <summary>
      /// zum eigentlichen Zeichnen der Garmin-Tiles
      /// </summary>
      public GarminImageCreator.ImageCreator GarminImageCreator;

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
      protected override Bitmap GetBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom) {
         GMapTileId gMapTileId = new GMapTileId(p1, zoom);
         jobs.TryAdd(gMapTileId.ID, gMapTileId);
         killUnnecessaryJobs(gMapTileId);

         Bitmap bm = new Bitmap(width, height);

         List<GarminImageCreator.GarminMapData> mapData = GarminImageCreator.GetGarminMapDefs();
         double[] groundresolution = new double[mapData.Count];
         for (int m = 0; m < mapData.Count; m++)
            groundresolution[m] = Projection.GetGroundResolution(zoom, mapData[m].GetMapCenterLat());

         object extdata = null;     // Objekt zum Übergeben der Liste der Garmintexte (die erst nach dem Hillshading ausgegeben werden)

         bool withHillshade = DEM != null && DEM.WithHillshade;

         bool result = false;
         // den gewünschten Bereich auf das Bitmap zeichnen
         if (GarminImageCreator.DrawImage(bm,
                                          p1.Lng, p1.Lat,
                                          p2.Lng, p2.Lat,
                                          mapData,              // wieder zurückliefern, falls inzwischen geändert
                                          groundresolution,
                                          withHillshade ?
                                              global::GarminImageCreator.ImageCreator.PictureDrawing.beforehillshade :
                                              global::GarminImageCreator.ImageCreator.PictureDrawing.all,
                                          ref extdata,
                                          ref gMapTileId.Cancel)) {
            // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
            if (withHillshade &&
                bm != null) {
               //DrawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha);
               DrawHillshadeAsync(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha).Wait(); // blockiert wahrscheinlich nicht ganz so stark wie die synchrone Methode

               if (!gMapTileId.IsCancel)
                  // den gewünschten Bereich auf das Bitmap zeichnen
                  if (GarminImageCreator.DrawImage(bm,
                                                   p1.Lng, p1.Lat,
                                                   p2.Lng, p2.Lat,
                                                   mapData,              // wieder zurückliefern, falls inzwischen geändert
                                                   groundresolution,
                                                   global::GarminImageCreator.ImageCreator.PictureDrawing.afterhillshade,
                                                   ref extdata,
                                                   ref gMapTileId.Cancel))
                     result = true;
            } else
               result = true;
         }
         if (!result) {
            bm.Dispose();
            bm = null;
            Debug.WriteLine(nameof(GarminProvider) + "." + nameof(GetBitmap) + " mit (" + p1 + ", zoom=" + zoom + ") abgebrochen.");
         }

         jobs.TryRemove(gMapTileId.ID, out _);

         return bm;
      }

      /// <summary>
      /// Unnötige Jobs werden als Cancel-bar markiert.
      /// </summary>
      /// <param name="actGMapTileId"></param>
      static void killUnnecessaryJobs(GMapTileId actGMapTileId) {
         foreach (var kv in jobs.ToArray())
            if ((kv.Value.CreationTime <= actGMapTileId.CreationTime &&    // Job ist älter und ...
                 kv.Value.Zoom != actGMapTileId.Zoom) ||                   // ... hat einen anderen Zoom ...
                (actGMapTileId.CreationTime.Subtract(kv.Value.CreationTime).TotalSeconds > 60)) { // ... oder ist schon älter als 1min -> nicht mehr benötigt
               kv.Value.IsCancel = true;
               Debug.WriteLine(nameof(GarminProvider) + "." + nameof(killUnnecessaryJobs) + " für (" +
                               kv.Value.Point + ", zoom=" + kv.Value.Zoom + ") IsCancel = true wegen (" +
                               actGMapTileId.Point + ", zoom=" + actGMapTileId.Zoom + ")");
            }
      }


      class GMapTileId {
         static uint id = 0;

         static object locker = new object();

         /// <summary>
         /// Zeitpunkt der Erzeugung
         /// </summary>
         public readonly DateTime CreationTime;

         public readonly PointLatLng Point;

         public readonly int Zoom;

         /// <summary>
         /// threadsicher erzeugte ID
         /// </summary>
         public readonly uint ID;

         // Um Cancel per ref an andere Funktionen weitergeben zu können kann leider keine Propertie verwendet werden.
         // Bequemer ist das Lesen/Schreiben mit IsCancel.

         /// <summary>
         /// wenn true, sollte der Job abgebrochen werden
         /// </summary>
         public long Cancel;

         public bool IsCancel {
            get => Interlocked.Read(ref Cancel) != 0;
            set => Interlocked.Exchange(ref Cancel, value ? 1 : 0);
         }


         public GMapTileId(PointLatLng point, int zoom) {
            lock (locker) {
               ID = id < uint.MaxValue ? ++id : 0;
            }
            CreationTime = DateTime.Now;
            Point = point;
            Zoom = zoom;
            Cancel = 0;
         }

         public override string ToString() {
            return ID + ": " + Point + ", " + Zoom + ", " + CreationTime.ToString("O");
         }

      }

      static ConcurrentDictionary<uint, GMapTileId> jobs = new ConcurrentDictionary<uint, GMapTileId>();

   }

}