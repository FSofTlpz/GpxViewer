//#define LOCALDEBUG

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GMap.NET.FSofTExtented.Projections;
using System.IO;
using System.Drawing;

namespace GMap.NET.FSofTExtented.MapProviders {

   /// <summary>
   /// WMS (WGS84 and UTM)
   /// </summary>
   public class WMSProvider : GMapProviderWithHillshade, IArbitraryArea {

      readonly Guid ID = new Guid("27954C9F-BAF9-4B36-81AC-7DD3715C91E3");
      const string PROVIDERNAME = "WMS";

      public static readonly WMSProvider Instance;

      public override Guid Id => ID;

      public override string Name => PROVIDERNAME;

      GeoAPI.CoordinateSystems.ICoordinateSystem csepsg = null;

      int _EPSG = 0;
      /// <summary>
      /// wird i.A. über die <see cref="ReferenceSystem4Url"/> gesetzt und setzt das Koordinatensystem <see cref="csepsg"/> und die Projektion (UTM oder MercatorProjection)
      /// </summary>
      protected int EPSG {
         get => _EPSG;
         set {
            if (value == 0)
               csepsg = null;
            else {
               //csepsg = FSofTUtils.Geography.SRIDReader.GetCSbyIDFromResource(value, typeof(GMap.NET.CoreExt.Properties.Resources), "SRID");
               csepsg = FSofTUtils.Geography.SRIDReader.GetCSbyIDFromInternalFile(value);
               if (csepsg == null)
                  throw new Exception("unknown epsg: " + value.ToString());
            }
            _EPSG = csepsg != null ? value : 0;

            // Projektion setzen
            _projection = MercatorProjection2.Instance;
            if (EPSG > 0) {
               if (csepsg is ProjNet.CoordinateSystems.ProjectedCoordinateSystem) {
                  ProjNet.CoordinateSystems.ProjectedCoordinateSystem cs = csepsg as ProjNet.CoordinateSystems.ProjectedCoordinateSystem;
                  //string proj = cs.Projection.Name;
                  string name = cs.Name;

                  if (name.IndexOf(" UTM zone ") >= 0) {
                     // <CS_ProjectionParameter Name=\"central_meridian\" Value=\"0\"/>
                     int central_meridian = -1;
                     Match match = Regex.Match(cs.Projection.XML, "<CS_ProjectionParameter Name=\"central_meridian\" Value=\"(\\d+)\"", RegexOptions.IgnoreCase);
                     if (match.Success && match.Groups.Count == 2) {
                        try {
                           central_meridian = Convert.ToInt32(match.Groups[1].Value);
                        } catch {
                           central_meridian = 0;
                        }
                     }

                     if (central_meridian >= 0) {
                        UTMProjection.Instance.SetEllipseData(cs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.SemiMajorAxis * cs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.AxisUnit.MetersPerUnit,
                                                              cs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.InverseFlattening);

                        UTMProjection.Instance.UTMZone = 1 + (180 + central_meridian) / 6;

                        _projection = UTMProjection.Instance;
                     }
                  }


               }
            }

         }
      }

      string _ReferenceSystem4Url = string.Empty;
      /// <summary>
      /// setzt auch <see cref="EPSG"/> (bei "srs=epsg:*", sonst 0)
      /// </summary>
      protected string ReferenceSystem4Url {
         get => _ReferenceSystem4Url;
         set {
            _ReferenceSystem4Url = value;

            Match match = Regex.Match(value, @"srs=epsg:(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count == 2) {
               try {
                  int val = Convert.ToInt32(match.Groups[1].Value);
                  EPSG = val != 4326 ? val : 0;
               } catch {
                  EPSG = 0;
               }
            } else
               EPSG = 0;
         }
      }


      static WMSProvider() {
         Instance = new WMSProvider();
      }

      WMSProvider() {
         MaxZoom = 24;
         _projection = Projections.MercatorProjection2.Instance;
         StandardDbId = DbId;
         //WMSTestInit(2);
      }

      #region diverse Beispiele
      /// <summary>
      /// diverse Beispiele
      /// </summary>
      /// <param name="v"></param>
      //void WMSTestInit(int v) {
      //   Version4Url = "1.1.1";
      //   PictureFormat4Url = "png";
      //   ReferenceSystem4Url = "srs=EPSG:4326";     // srs bei 1.1.1, crs bei 1.3.0.
      //   Layer4Url = "";   // <- Layer/Name
      //   ExtendedParameters4Url = "";

      //   // Capabilities:
      //   // ...?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetCapabilities
      //   // ...?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities

      //   switch (v) {
      //      #region UTM-WMS

      //      case 0:
      //         BaseURL = "http://10.125.4.164:88/ows/wms_getmap.php";
      //         ReferenceSystem4Url = "srs=EPSG:25833";
      //         Layer4Url = "ot";
      //         break;

      //      case 1:
      //         // fkt. nur bei nicht zu starkem Zoom
      //         BaseURL = "http://l000sa30/arcgis/services/wms/Stadtplan/MapServer/WMSServer";
      //         ReferenceSystem4Url = "srs=EPSG:25833";
      //         Layer4Url = "1,2,4,19,20,21,22";   // <- Layer/Name
      //         break;

      //      case 2:
      //         BaseURL = "http://l000sa30/arcgis/services/wms/Luftbild_2019/MapServer/WMSServer";
      //         ReferenceSystem4Url = "srs=EPSG:25833";
      //         Layer4Url = "1";                        // <- Layer/Name
      //         break;

      //      case 3:
      //         // fkt. erst bei starkem Zoom
      //         BaseURL = "https://geodienste.sachsen.de/wms_geosn_alkis-adv/guest";
      //         ReferenceSystem4Url = "srs=EPSG:25833";
      //         Layer4Url = "adv_alkis_tatsaechliche_nutzung,adv_alkis_gebaeude";   // <- Layer/Name
      //         break;

      //      #endregion

      //      #region WGS84-WMS

      //      case 4:
      //         BaseURL = "https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?";
      //         Layer4Url = "Siedlung,Vegetation,Gewaesser,Verkehr,Beschriftung";
      //         break;

      //      case 5:
      //         // bei starkem Zoom
      //         BaseURL = "https://geodienste.sachsen.de/wms_geosn_alkis-adv/guest";
      //         Layer4Url = "adv_alkis_tatsaechliche_nutzung,adv_alkis_gebaeude";   // <- Layer/Name
      //         break;

      //      case 6:
      //         // fkt. bei nicht zu starkem Zoom
      //         BaseURL = "http://l000sa30/arcgis/services/wms/Stadtplan/MapServer/WMSServer";
      //         Version4Url = "1.3.0";
      //         ReferenceSystem4Url = "crs=CRS:84";     // srs bei 1.1.1, crs bei 1.3.0.
      //         Layer4Url = "1,2,4,19,20,21,22";   // <- Layer/Name
      //         break;

      //      case 7:
      //         BaseURL = "http://l000sa30/arcgis/services/wms/Luftbild_2019/MapServer/WMSServer";
      //         Version4Url = "1.3.0";
      //         ReferenceSystem4Url = "crs=CRS:84";     // srs bei 1.1.1, crs bei 1.3.0.
      //         Layer4Url = "1";                        // <- Layer/Name
      //         break;

      //         #endregion
      //   }
      //}

      #endregion


      public class WMSMapDefinition : MultiUseBaseProviderDefinition {

         public const string IDDELTAFILE = "iddelta.wms";

         static UniqueIDDelta uniqueIDDelta = null;

         /// <summary>
         /// WMS-version ("1.1.1")
         /// </summary>
         public string Version;
         /// <summary>
         /// WMS-url
         /// </summary>
         public string URL;
         /// <summary>
         /// pictureformat ("png", "jpg", ...)
         /// </summary>
         public string PictureFormat;
         /// <summary>
         /// for example "srs=EPSG:25833", "srs=EPSG:4326", "crs=CRS:84", ...
         /// </summary>
         public string SRS;
         /// <summary>
         /// layerlist ("street,water", ...)
         /// </summary>
         public string Layer;
         /// <summary>
         /// ext. parameters for url
         /// </summary>
         public string ExtendedParameters;
         /// <summary>
         /// get all layer as array
         /// </summary>
         /// <returns></returns>
         public string[] Layers() => Layer.Split([','], StringSplitOptions.RemoveEmptyEntries);

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
         /// <param name="mapname">Name der Karte</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="layerlist">durch Komma getrennte Layernamen</param>
         /// <param name="url">z.B.: https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?</param>
         /// <param name="srs">Koordinatensystem</param>
         /// <param name="wmsversion">WMS-Version</param>
         /// <param name="pictureformat">Bildformat</param>
         /// <param name="extendedparameters"></param>
         public WMSMapDefinition(string mapname,
                                 int minzoom,
                                 int maxzoom,
                                 string layerlist,
                                 string url = "https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?",
                                 string srs = "srs=EPSG:4326",
                                 string wmsversion = "1.1.1",
                                 string pictureformat = "png",
                                 string extendedparameters = "",
                                 bool hillShading = false,
                                 byte hillShadingAlpha = 100) :
            base(mapname, Instance.Name, minzoom, maxzoom) {
            Version = wmsversion;
            Layer = layerlist;
            URL = url;
            PictureFormat = pictureformat;
            SRS = srs;
            ExtendedParameters = extendedparameters;

            string hash4delta = UniqueIDDelta.GetHashString(mapname + Version + Layer + URL + PictureFormat + SRS + ExtendedParameters,
                                                            new byte[0]);
            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, IDDELTAFILE));
            DbIdDelta = uniqueIDDelta.GetDelta(hash4delta);

            HillShading = hillShading;
            HillShadingAlpha = hillShadingAlpha;
         }

         public WMSMapDefinition(WMSMapDefinition def) :
            base(def.MapName, def.ProviderName, def.MinZoom, def.MaxZoom) {
            Version = def.Version;
            Layer = def.Layer;
            URL = def.URL;
            PictureFormat = def.PictureFormat;
            SRS = def.SRS;
            ExtendedParameters = def.ExtendedParameters;
            DbIdDelta = def.DbIdDelta;
            HillShading = def.HillShading;
            HillShadingAlpha = def.HillShadingAlpha;
         }

         public override string ToString() {
            return string.Format("Name={0}, Version={1}, URL={2}, SRS={3}, Layers={4}",
                                 MapName,
                                 Version,
                                 URL,
                                 SRS,
                                 string.Join(",", Layers())
                                 );
         }
      }

      //readonly object lock_def = new object();

      //WMSMapDefinition mapDefinition = null;


      /// <summary>
      /// liefert die URL um die Daten für ein bestimmtes Tile zu holen
      /// </summary>
      /// <param name="pos">x- und y-Index des gewünschten Tiles</param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      string MakeTileImageUrl(GPoint pos, int zoom, string language, WMSMapDefinition def) {
         var px1 = Projection.FromTileXYToPixel(pos);    // i.A. new GPoint((pos.X * TileSize.Width), (pos.Y * TileSize.Height));
         var px2 = px1;
         px1.Offset(0, Projection.TileSize.Height);   // Ecke links-oben (in Pixel des Gesamtbildes)
         px2.Offset(Projection.TileSize.Width, 0);    // Ecke rechts-unten (in Pixel des Gesamtbildes)

         PointLatLng p1 = Projection.FromPixelToLatLng(px1, zoom);
         PointLatLng p2 = Projection.FromPixelToLatLng(px2, zoom);

         PointUTM utmp1 = PointUTM.Empty;
         PointUTM utmp2 = PointUTM.Empty;

         if (EPSG > 0) {
            if (Projection is UTMProjection) {
               utmp1 = UTMProjection.WGS84ToUTM(p1, (Projection as UTMProjection).UTMZone);
               utmp2 = UTMProjection.WGS84ToUTM(p2, (Projection as UTMProjection).UTMZone);
#if LOCALDEBUG
               Debug.WriteLine(string.Format("MakeTileImageUrl: Tileindex {0} -> Pixel {1} / {2} -> Lat/Lon {3} {4} -> UTM {5} {6}",
                                             pos, px1, px2, p1, p2, utmp1, utmp2));
#endif
            } else {

            }
         }
#if LOCALDEBUG
         else
            Debug.WriteLine(string.Format("MakeTileImageUrl: Tileindex {0} -> Pixel {1} / {2} -> Lat/Lon {3} {4}",
                                          pos, px1, px2, p1, p2));
#endif

         StringBuilder url;

         url = new StringBuilder(def.URL);
         url.Append(def.URL.Contains("?") ? "&" : "?");
         url.Append("VERSION=" + def.Version);
         url.Append("&REQUEST=GetMap");
         url.Append("&SERVICE=WMS");
         url.Append("&" + def.SRS);
         url.Append("&styles=");
         if (def.Layer != string.Empty)
            url.Append("&layers=" + def.Layer);
         url.Append("&format=image/" + def.PictureFormat);
         if (EPSG > 0) {
            if (Projection is UTMProjection) {
               url.AppendFormat(CultureInfo.InvariantCulture, "&bbox={0},{1},{2},{3}", utmp1.X, utmp1.Y, utmp2.X, utmp2.Y);
            } else {

            }
         } else
            url.AppendFormat(CultureInfo.InvariantCulture, "&bbox={0},{1},{2},{3}", p1.Lng, p1.Lat, p2.Lng, p2.Lat);

         url.AppendFormat("&width={0}&height={1}", Projection.TileSize.Width, Projection.TileSize.Height);

         if (!string.IsNullOrEmpty(def.ExtendedParameters)) {
            url.Append("&");
            url.Append(def.ExtendedParameters);
         }

         return url.ToString();
      }

      /// <summary>
      /// setzt eine neue WMS-Def.
      /// </summary>
      /// <param name="newmapdefs"></param>
      public void SetDef(WMSMapDefinition data) {
         base.SetDef(data);
         ReferenceSystem4Url = data.SRS;
      }

      #region MultiUseBaseProvider

      public override PureImage GetTileImageWithMapDefinition(GPoint pos, int zoom, MapProviderDefinition def) {
         WMSMapDefinition mapDefinition = (WMSMapDefinition)def;

         string url = MakeTileImageUrl(pos, zoom, LanguageStr, mapDefinition);

         PureImage img = null;
         try {
#if LOCALDEBUG
            Debug.WriteLine(string.Format("GetTileImage: url={0}", url));
#endif
            img = GetTileImageUsingHttp(url);

            if (DEM != null &&
                mapDefinition.HillShading &&
                img != null) {
               using (Bitmap bm = (Bitmap)ProviderHelper.GetImage(img)) {
                  if (bm != null) {
                     var px1 = Projection.FromTileXYToPixel(pos);    // i.A. new GPoint((pos.X * TileSize.Width), (pos.Y * TileSize.Height));
                     var px2 = px1;
                     px1.Offset(0, Projection.TileSize.Height);   // Ecke links-oben (in Pixel des Gesamtbildes)
                     px2.Offset(Projection.TileSize.Width, 0);    // Ecke rechts-unten (in Pixel des Gesamtbildes)

                     PointLatLng p1 = Projection.FromPixelToLatLng(px1, zoom);
                     PointLatLng p2 = Projection.FromPixelToLatLng(px2, zoom);

                     drawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, null);
                     img = ProviderHelper.GetPureImage(bm);
                  }
               }
            }

            //var px1 = Projection.FromTileXYToPixel(pos);    // i.A. new GPoint((pos.X * TileSize.Width), (pos.Y * TileSize.Height));
            //var px2 = px1;
            //px1.Offset(0, Projection.TileSize.Height);   // Ecke links-oben (in Pixel des Gesamtbildes)
            //px2.Offset(Projection.TileSize.Width, 0);    // Ecke rechts-unten (in Pixel des Gesamtbildes)

            //PointLatLng p1 = Projection.FromPixelToLatLng(px1, zoom);
            //PointLatLng p2 = Projection.FromPixelToLatLng(px2, zoom);
            //img = getPureImage((int)Projection.TileSize.Width,
            //                   (int)Projection.TileSize.Height,
            //                   p1,
            //                   p2,
            //                   zoom,
            //                   def);




            // I.A. wird das anzuzeigende Bild später so aus dem Stream erzeugt:
            //    System.Drawing.Image wimg = System.Drawing.Image.FromStream(img.Data, true, true);
            // Hier könnte also manipuliert werden.

            //Debug.WriteLine(string.Format("              img.Data.Length={0}", img.Data.Length));

         } catch (Exception ex) {
            Debug.WriteLine("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      protected override Bitmap getBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         WMSMapDefinition mapDefinition = def as WMSMapDefinition;

         Bitmap bm = ProviderHelper.GetArbitraryBitmap(this, width, height, p1, p2, zoom, def);

         // Das Hillshading wird ev. über die eigentliche Karte darübergelegt.
         if (DEM != null &&
             mapDefinition.HillShading &&
             bm != null) {
            drawHillshade(DEM, bm, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Alpha, null);
         }

         return bm;
      }

      #endregion

      #region IArbitraryArea

      public PureImage GetArbitraryArea(int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) =>
         getPureImage(width, height, p1, p2, zoom, def);

      #endregion

   }
}
