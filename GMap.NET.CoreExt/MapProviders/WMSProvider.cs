using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GMap.NET.MapProviders;
using GMap.NET.CoreExt.Projections;
using System.IO;

namespace GMap.NET.CoreExt.MapProviders {

   /// <summary>
   /// WMS (WGS84 and UTM)
   /// </summary>
   public class WMSProvider : GMapProvider {
      public static readonly WMSProvider Instance;

      PureProjection _Projection = null;
      GeoAPI.CoordinateSystems.ICoordinateSystem csepsg = null;

      int _EPSG = 0;
      /// <summary>
      /// wird i.A. über die <see cref="ReferenceSystem4Url"/> gesetzt und setzt das Koordinatensystem <see cref="csepsg"/> und die Projektion (UTM oder MercatorProjection)
      /// </summary>
      protected int EPSG {
         get {
            return _EPSG;
         }
         set {
            if (value == 0)
               csepsg = null;
            else {
               csepsg = FSofTUtils.Geography.SRIDReader.GetCSbyIDFromResource(value, typeof(GMap.NET.CoreExt.Properties.Resources), "SRID");
               if (csepsg == null)
                  throw new Exception("unknown epsg: " + value.ToString());
            }
            _EPSG = csepsg != null ? value : 0;

            // Projektion setzen
            _Projection = MercatorProjection2.Instance;
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

                        _Projection = UTMProjection.Instance;
                     }
                  }


               }
            }

         }
      }

      string _ReferenceSystem4Url = "";
      /// <summary>
      /// setzt auch <see cref="EPSG"/> (bei "srs=epsg:*", sonst 0)
      /// </summary>
      protected string ReferenceSystem4Url {
         get {
            return _ReferenceSystem4Url;
         }
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

      protected string BaseURL = "";
      protected string Version4Url = "1.1.1";
      protected string Layer4Url = "";
      protected string PictureFormat4Url = "png";
      protected string ExtendedParameters4Url = "";

      /// <summary>
      /// Standard-DbId des Providers
      /// </summary>
      static public int StandardDbId {
         get;
         protected set;
      }

      static WMSProvider() {
         Instance = new WMSProvider();
         StandardDbId = Instance.DbId;
      }

      WMSProvider() {
         MaxZoom = 24;
         //WMSTestInit(2);
      }

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

      /// <summary>
      /// setzt eine andere DbId
      /// </summary>
      /// <param name="dbid"></param>
      /// <returns></returns>
      public int ChangeDbId(int dbid) {
         int olddbid = DbId;
         unregisterProvider(this);
         setField(typeof(GMapProvider), this, "DbId", dbid);
         registerProvider(this);
         return olddbid;
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("27954C9F-BAF9-4B36-81AC-7DD3715C91E3");

      public override Guid Id {
         get { return id; }
      }

      public override string Name {
         get {
            return "WMS";
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
         string url = MakeTileImageUrl(pos, zoom, LanguageStr);

         PureImage img = null;
         try {

            Debug.WriteLine(string.Format("GetTileImage: url={0}", url));

            img = GetTileImageUsingHttp(url);

            // I.A. wird das anzuzeigende Bild später so aus dem Stream erzeugt:
            //    System.Drawing.Image wimg = System.Drawing.Image.FromStream(img.Data, true, true);
            // Hier könnte also manipuliert werden.

            //Debug.WriteLine(string.Format("              img.Data.Length={0}", img.Data.Length));

         } catch (Exception ex) {
            Debug.WriteLine("Exception bei GetTileImage(): " + ex.Message);
         }
         return img;
      }

      #endregion

      public class WMSMapDefinition : MapProviderDefinition {

         static UniqueIDDelta uniqueIDDelta = null;

         /// <summary>
         /// WMS-version
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
         public string[] Layers() {
            return Layer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
         }

         /// <summary>
         /// spez. Delta für die DbId für diese Karte
         /// </summary>
         public int DbIdDelta { get; protected set; }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapname">Name der Karte</param>
         /// <param name="zoom4display">zusätzlicher Vergrößerungsfaktor falls das Display eine zu hohe DPI hat (null oder 1.0 ...)</param>
         /// <param name="minzoom">kleinster zulässiger Zoom</param>
         /// <param name="maxzoom">größter zulässiger Zoom</param>
         /// <param name="layerlist">durch Komma getrennte Layernamen</param>
         /// <param name="url">z.B.: https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?</param>
         /// <param name="srs">Koordinatensystem</param>
         /// <param name="wmsversion">WMS-Version</param>
         /// <param name="pictureformat">Bildformat</param>
         /// <param name="extendedparameters"></param>
         public WMSMapDefinition(string mapname,
                                 double zoom4display,
                                 int minzoom,
                                 int maxzoom,
                                 string layerlist,
                                 string url = "https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?",
                                 string srs = "srs=EPSG:4326",
                                 string wmsversion = "1.1.1",
                                 string pictureformat = "png",
                                 string extendedparameters = "") :
            base(mapname, Instance.Name, zoom4display, minzoom, maxzoom) {
            Version = wmsversion;
            Layer = layerlist;
            URL = url;
            PictureFormat = pictureformat;
            SRS = srs;
            ExtendedParameters = extendedparameters;

            string hash4delta = UniqueIDDelta.GetHashString(mapname + Version + Layer + URL + PictureFormat + SRS + ExtendedParameters, 
                                                            new byte[0]);
            if (uniqueIDDelta == null)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, "iddelta.wms"));
            DbIdDelta = uniqueIDDelta.GetDelta(hash4delta);
         }

         public WMSMapDefinition(WMSMapDefinition def) :
            base(def.MapName, def.ProviderName, def.Zoom4Display, def.MinZoom, def.MaxZoom) {
            Version = def.Version;
            Layer = def.Layer;
            URL = def.URL;
            PictureFormat = def.PictureFormat;
            SRS = def.SRS;
            ExtendedParameters = def.ExtendedParameters;
            DbIdDelta = def.DbIdDelta;
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

      readonly object lock_def = new object();

      /// <summary>
      /// liefert die URL um die Daten für ein bestimmtes Tile zu holen
      /// </summary>
      /// <param name="pos">x- und y-Index des gewünschten Tiles</param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      string MakeTileImageUrl(GPoint pos, int zoom, string language) {
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
               Debug.WriteLine(string.Format("MakeTileImageUrl: Tileindex {0} -> Pixel {1} / {2} -> Lat/Lon {3} {4} -> UTM {5} {6}",
                                             pos, px1, px2, p1, p2, utmp1, utmp2));
            } else {

            }
         } else
            Debug.WriteLine(string.Format("MakeTileImageUrl: Tileindex {0} -> Pixel {1} / {2} -> Lat/Lon {3} {4}",
                                          pos, px1, px2, p1, p2));

         StringBuilder url;
         lock (lock_def) {
            url = new StringBuilder(BaseURL);
            url.Append(BaseURL.Contains("?") ? "&" : "?");
            url.Append("VERSION=" + Version4Url);
            url.Append("&REQUEST=GetMap");
            url.Append("&SERVICE=WMS");
            url.Append("&" + ReferenceSystem4Url);
            url.Append("&styles=");
            if (Layer4Url != "")
               url.Append("&layers=" + Layer4Url);
            url.Append("&format=image/" + PictureFormat4Url);
            if (EPSG > 0) {
               if (Projection is UTMProjection) {
                  url.AppendFormat(CultureInfo.InvariantCulture, "&bbox={0},{1},{2},{3}", utmp1.X, utmp1.Y, utmp2.X, utmp2.Y);
               } else {

               }
            } else
               url.AppendFormat(CultureInfo.InvariantCulture, "&bbox={0},{1},{2},{3}", p1.Lng, p1.Lat, p2.Lng, p2.Lat);

            url.AppendFormat("&width={0}&height={1}", Projection.TileSize.Width, Projection.TileSize.Height);

            if (!string.IsNullOrEmpty(ExtendedParameters4Url)) {
               url.Append("&");
               url.Append(ExtendedParameters4Url);
            }
         }

         return url.ToString();
      }

      /// <summary>
      /// setzt eine neue WMS-Def.
      /// </summary>
      /// <param name="newmapdefs"></param>
      public void SetDef(WMSMapDefinition data) {
         lock (lock_def) {
            Version4Url = data.Version;
            BaseURL = data.URL;
            ReferenceSystem4Url = data.SRS;
            Layer4Url = data.Layer;
            PictureFormat4Url = data.PictureFormat;
            ExtendedParameters4Url = data.ExtendedParameters;
         }
      }


      #region spez. Zugriff auf Props und Fields über Reflection

      static protected object getProperty(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) {
         return classtype.GetProperty(name, flags).GetValue(obj);
      }

      static protected void setProperty(Type classtype, object obj, string name, object value, BindingFlags flags = BindingFlags.Default) {
         classtype.GetProperty(name, flags).SetValue(obj, value);
      }

      static protected object getField(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) {
         return classtype.GetField(name, flags).GetValue(obj);
      }

      static protected void setField(Type classtype, object obj, string name, object value, BindingFlags flags) {
         classtype.GetField(name, flags).SetValue(obj, value);
      }

      static protected void setField(Type classtype, object obj, string name, object value) {
         classtype.GetField(name).SetValue(obj, value);
      }

      #endregion

      /// <summary>
      /// fügt den Provider an die Liste der vordef. Provider an
      /// <para>Tritt dabei ein Fehler auf, erfolgt eine Exception.</para>
      /// </summary>
      /// <param name="provider"></param>
      static protected void registerProvider(GMapProvider provider) {
         try {
            List<GMapProvider> mapProviders = (List<GMapProvider>)getField(typeof(GMapProvider), provider, "MapProviders", BindingFlags.Static | BindingFlags.NonPublic);
            mapProviders.Add(provider);

            List<GMapProvider> List = (List<GMapProvider>)getProperty(typeof(GMapProviders), provider, "List", BindingFlags.Static | BindingFlags.Public);
            List.Add(provider);

            Dictionary<Guid, GMapProvider> Hash = (Dictionary<Guid, GMapProvider>)getField(typeof(GMapProviders), provider, "Hash", BindingFlags.Static | BindingFlags.NonPublic);
            Hash.Add(provider.Id, provider);

            Dictionary<int, GMapProvider> DbHash = (Dictionary<int, GMapProvider>)getField(typeof(GMapProviders), provider, "DbHash", BindingFlags.Static | BindingFlags.NonPublic);
            DbHash.Add(provider.DbId, provider);
         } catch (Exception ex) {
            throw new Exception("Der Kartenprovider '" + provider.Name + "' kann nicht registriert werden." + System.Environment.NewLine + ex.Message);
         }
      }

      static protected void unregisterProvider(GMapProvider provider) {
         try {
            List<GMapProvider> mapProviders = (List<GMapProvider>)getField(typeof(GMapProvider), provider, "MapProviders", BindingFlags.Static | BindingFlags.NonPublic);
            mapProviders.Remove(provider);

            List<GMapProvider> List = (List<GMapProvider>)getProperty(typeof(GMapProviders), provider, "List", BindingFlags.Static | BindingFlags.Public);
            List.Remove(provider);

            Dictionary<Guid, GMapProvider> Hash = (Dictionary<Guid, GMapProvider>)getField(typeof(GMapProviders), provider, "Hash", BindingFlags.Static | BindingFlags.NonPublic);
            Hash.Remove(provider.Id);

            Dictionary<int, GMapProvider> DbHash = (Dictionary<int, GMapProvider>)getField(typeof(GMapProviders), provider, "DbHash", BindingFlags.Static | BindingFlags.NonPublic);
            DbHash.Remove(provider.DbId);
         } catch (Exception ex) {
            throw new Exception("Der Kartenprovider '" + provider.Name + "' kann nicht deregistriert werden." + System.Environment.NewLine + ex.Message);
         }
      }


   }
}