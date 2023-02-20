using FSofTUtils;
using System;
using System.Collections.Generic;
using System.Drawing;

#if Android
namespace TrackEddi {
   public class Config : SimpleXmlDocument2 {
#else
namespace GpxViewer {
   class Config : SimpleXmlDocument2 {
#endif

      const string XML_ROOT = "*";

      const string XML_MINIMALTRACKPOINTDISTANCE = "minimaltrackpointdistance";
      const string XML_MINIMALTRACKPOINTDISTANCE_X = "@x";
      const string XML_MINIMALTRACKPOINTDISTANCE_Y = "@y";

      const string XML_SECTION_PROXY = "proxy";
      const string XML_PROXYNAME = "proxyname";
      const string XML_PROXYPORT = "proxyport";
      const string XML_PROXYUSER = "proxyuser";
      const string XML_PROXYPASSWORD = "proxypassword";

      const string XML_MAP = "map";
      const string XML_CACHELOCATION = "cachelocation";
      const string XML_SERVERONLY = "serveronly";
      const string XML_STARTPROVIDER = "startprovider";
      const string XML_STARTLATITUDE = "startlatitude";
      const string XML_STARTLONGITUDE = "startlongitude";
      const string XML_STARTZOOM = "startzoom";
      const string XML_SYMBOLZOOMFACTOR = "symbolzoomfactor";
      const string XML_DELTAPERCENT4SEARCH = "deltapercent4search";

      const string XML_DEMPATH = "dem";
      const string XML_DEMCACHESIZE = "@cachesize";
      const string XML_DEMCACHEPATH = "@cachepath";
      const string XML_DEMMINZOOM = "@minzoom";
      const string XML_DEMHILLSHADINGAZIMUT = "@hillshadingazimut";
      const string XML_DEMHILLSHADINGALTITUDE = "@hillshadingaltitude";
      const string XML_DEMHILLSHADINGSCALE = "@hillshadingscale";

      const string XML_PROVIDER = "provider";
      const string XML_MAPNAME = "@mapname";
      const string XML_DBIDDELTA = "@dbiddelta";
      const string XML_MINZOOM = "@minzoom";
      const string XML_MAXZOOM = "@maxzoom";
      const string XML_ZOOM4DISPLAY = "@zoom4display";

      const string XML_PROVIDERMENU = "providermenu";
      const string XML_LASTUSEDITEMS = "@lastuseditems";
      const string XML_PROVIDERMENUITEM = "item";
      const string XML_PROVIDERMENUITEMTXT = "@txt";
      const string XML_PROVIDERMENUITEMNO = "@no";
      const string XML_PROVIDERMENUSUBITEM = "subitem";

      const string XML_HILLSHADING = "@hillshading";
      const string XML_HILLSHADINGALPHA = "@hillshadingalpha";

      const string XML_GARMIN_TDB = "@tdb";
      const string XML_GARMIN_TYP = "@typ";
      const string XML_GARMIN_LEVELS4CACHE = "@levels4cache";
      const string XML_GARMIN_MAXSUBDIV = "@maxsubdiv";
      const string XML_GARMIN_TEXTFACTOR = "@textfactor";
      const string XML_GARMIN_SYMBOLFACTOR = "@symbolfactor";
      const string XML_GARMIN_LINEFACTOR = "@linefactor";

      const string XML_GARMINKMZ_KMZFILE = "@kmzfile";

      const string XML_WMS_URL = "@url";
      const string XML_WMS_VERSION = "@version";
      const string XML_WMS_SRS = "@srs";
      const string XML_WMS_PICTFORMAT = "@format";
      const string XML_WMS_LAYERS = "@layers";
      const string XML_WMS_EXT = "@extended";


      const string XML_SECTION_TRACKS = "tracks";
      const string XML_STANDARDTRACK = "standard";
      const string XML_STANDARDTRACK2 = "standard2";
      const string XML_STANDARDTRACK3 = "standard3";
      const string XML_STANDARDTRACK4 = "standard4";
      const string XML_STANDARDTRACK5 = "standard5";
      const string XML_MARKEDTRACK = "marked";
      const string XML_EDITABLETRACK = "editable";
      const string XML_INEDITTRACK = "inedit";
      const string XML_SELPARTTRACK = "selectedpart";
      const string XML_HELPERLINE = "helperline";
      const string XML_TRACKCOLORA = "@a";
      const string XML_TRACKCOLORR = "@r";
      const string XML_TRACKCOLORG = "@g";
      const string XML_TRACKCOLORB = "@b";
      const string XML_TRACKWIDTH = "@width";
      const string XML_SECTION_SLOPE = "slope";
      const string XML_SLOPE = "slope";
      const string XML_SLOPECOLORA = "@a";
      const string XML_SLOPECOLORR = "@r";
      const string XML_SLOPECOLORG = "@g";
      const string XML_SLOPECOLORB = "@b";
      const string XML_SLOPEPERCENT = "@percent";


      const string XML_GARMINSYMBOLS = "garminsymbols";
      const string XML_GARMINSYMBOLGROUP = "group";
      const string XML_GARMINSYMBOLGROUPNAME = "@name";
      const string XML_GARMINSYMBOL = "symbol";
      const string XML_GARMINSYMBOLNAME = "@name";
      const string XML_GARMINSYMBOLTEXT = "@text";
      const string XML_GARMINSYMBOLOFFSET = "@offset";


      public Config(string configfile, string xsdfile) :
         base(configfile, XML_ROOT, xsdfile) {
         Validating = false;
         LoadData();
      }

      void setXPath(string xpath, object value) {
#if Android
         if (value is float)
            value = ((float)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         else if (value is double)
            value = ((double)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
#endif

         if (ExistXPath(xpath))
            Change(xpath, value.ToString());
         else {
            string[] xpathparts = xpath.Split('/');
            // letzten ex. xpath suchen
            int lastvalididx = -1;
            for (lastvalididx = xpathparts.Length - 2; lastvalididx > 0; lastvalididx--) {
               string tmpxpath = string.Join("/", xpathparts, 0, lastvalididx + 1);
               if (ExistXPath(tmpxpath))
                  break;
            }

            for (int i = lastvalididx + 1; i < xpathparts.Length; i++) {
               string tmpxpath = string.Join("/", xpathparts, 0, i);
               string name = xpathparts[i];
               if (name[0] != '@') {   // Nodename (ACHTUNG: Test ist zu einfach, falls [ nur im String)
                  int apos = xpathparts[i].IndexOf('[');
                  if (apos >= 0) {   // Array (bisher zu klein)
                     string nodename = name.Substring(0, apos);
                     while (!ExistXPath(tmpxpath + "/" + name)) {
                        Append(tmpxpath, nodename);
                     }
                     continue;
                  }
                  Append(tmpxpath,
                         name,
                         i == xpathparts.Length - 1 ?
                              value.ToString() :
                              null);
               } else {                         // Attribut
                  Append(tmpxpath,
                         null,
                         null,
                         new System.Collections.Generic.Dictionary<string, string>() {
                            { xpathparts[i].Substring(1), value.ToString() }
                         });
               }
            }

         }
      }

      public int MinimalTrackpointDistanceX {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_X, 14);
         set => setXPath("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_X, value);
      }

      public int MinimalTrackpointDistanceY {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_Y, 14);
         set => setXPath("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_Y, value);
      }

      public string CacheLocation {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_CACHELOCATION, null);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_CACHELOCATION, value);
      }

      public bool ServerOnly {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_SERVERONLY, true);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_SERVERONLY, value);
      }

      public int StartProvider {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTPROVIDER, 0);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTPROVIDER, value);
      }

      public int StartZoom {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTZOOM, 16);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTZOOM, value);
      }

      //public double Zoom4Display {
      //   get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_ZOOM4DISPLAY, 1.0);
      //   set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_ZOOM4DISPLAY, value);
      //}

      public int DeltaPercent4Search {
#if Android
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DELTAPERCENT4SEARCH, 10);
#else
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DELTAPERCENT4SEARCH, 1);
#endif
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DELTAPERCENT4SEARCH, value);
      }

      public double StartLatitude {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLATITUDE, 51.30);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLATITUDE, value);
      }

      public double StartLongitude {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLONGITUDE, 12.40);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLONGITUDE, value);
      }

      public double SymbolZoomfactor {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_SYMBOLZOOMFACTOR, 1.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_SYMBOLZOOMFACTOR, value);
      }

      public string[] Provider {
         get => ReadString("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER);
         set {
            for (int i = 0; i < value.Length; i++) {
               setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (i + 1) + "]", value[i]);
            }
         }
      }

      #region XML_MAP / XML_DEMPATH

      public string DemPath {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH, "");
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH, value);
      }

      public int DemCachesize {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMCACHESIZE, 16);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMCACHESIZE, value);
      }

      public string DemCachePath {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMCACHEPATH, "");
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMCACHEPATH, value);
      }

      public int DemMinZoom {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMMINZOOM, 11);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMMINZOOM, value);
      }

      public double DemHillshadingAzimut {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGAZIMUT, 315.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGAZIMUT, value);
      }

      public double DemHillshadingAltitude {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGALTITUDE, 45.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGALTITUDE, value);
      }

      public double DemHillshadingScale {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGSCALE, 1.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGSCALE, value);
      }

      #endregion


      public class ArrayProperty {

         static protected Config cfg;

         public ArrayProperty(Config cfgowner) {
            cfg = cfgowner;
         }
      }

      public class MapNameIdx : ArrayProperty {

         public MapNameIdx(Config cfg) : base(cfg) { }

         public string this[int providx] {
            get => cfg.ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAPNAME, "");
            set => cfg.setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAPNAME, value);
         }
      }

      #region allgemeine Providereigenschaften

      public string MapName(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAPNAME, "");
      }

      public void SetMapName(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAPNAME, value);
      }

      public int DbIdDelta(int providx) {
         return Math.Max(0, ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_DBIDDELTA, 0));
      }

      public void SetDbIdDelta(int providx, int value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_DBIDDELTA, value);
      }

      public int MinZoom(int providx) {
         return Math.Max(0, ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MINZOOM, 0));
      }

      public void SetMinZoom(int providx, int value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MINZOOM, value);
      }

      public int MaxZoom(int providx) {
         return Math.Min(ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAXZOOM, 24), 24);
      }

      public void SetMaxinZoom(int providx, int value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAXZOOM, value);
      }

      public double GetZoom4Display(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_ZOOM4DISPLAY, 1.0);
      }

      public void SetZoom4Display(int providx, double value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_ZOOM4DISPLAY, value);
      }

      public bool Hillshading(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADING, false);
      }

      public void SetHillshading(int providx, bool value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADING, value);
      }

      public int HillshadingAlpha(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADINGALPHA, 100) & 0xFF;
      }

      public void SetHillshadingAlpha(int providx, int value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADINGALPHA, value & 0xFF);
      }

      #endregion

      #region Provider-Menü

      public int ProvidermenuLastuseditems {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERMENU + "/" + XML_LASTUSEDITEMS, 3);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERMENU + "/" + XML_LASTUSEDITEMS, value);
      }

      public string ProvidermenuItemName(int itemidx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERMENU + "/" + XML_PROVIDERMENUITEM + "[" + (itemidx + 1).ToString() + "]/" + XML_PROVIDERMENUITEMTXT, "");
      }

      public int ProvidermenuItemNo(int itemidx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERMENU + "/" + XML_PROVIDERMENUITEM + "[" + (itemidx + 1).ToString() + "]/" + XML_PROVIDERMENUITEMNO, -1);
      }

      public int[] ProvidermenuItemSubItems(int itemidx) {
         string[] res = ReadString("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERMENU + "/" + XML_PROVIDERMENUITEM + "[" + (itemidx + 1).ToString() + "]/" + XML_PROVIDERMENUSUBITEM);
         int[] resno = new int[res.Length];
         for (int i = 0; i < res.Length; i++)
            resno[i] = Convert.ToInt32(res[i]);
         return resno;
      }

      #endregion

      #region spez. Providereigenschaften für Garmin

      public string GarminTdb(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, "");
      }

      public void SetGarminTdb(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, value);
      }

      public string GarminTyp(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, "");
      }

      public void SetGarminTyp(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, value);
      }

      public int[] GarminLocalCacheLevels(int providx) {
         string text = ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, "");
         string[] tmp = text.Split(new char[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
         int[] v = new int[tmp.Length];
         for (int i = 0; i < tmp.Length; i++)
            try {
               v[i] = Convert.ToInt32(tmp[i]);
            } catch {
               v[i] = 0;
            }
         return v;
      }

      public void SetGarminLocalCacheLevels(int providx, int[] value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, string.Join(",", value));
      }

      public int GarminMaxSubdiv(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, 1000000);
      }

      public void SetGarminMaxSubdiv(int providx, int value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, value);
      }

      public double GarminTextFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, 1.0);
      }

      public void SetGarminTextFactor(int providx, double value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, value);
      }

      public double GarminSymbolFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, 1.0);
      }

      public void SetGarminSymbolFactor(int providx, double value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, value);
      }

      public double GarminLineFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, 1.0);
      }

      public void SetGarminLineFactor(int providx, double value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, value);
      }

      #endregion

      #region spez. Providereigenschaften für Garmin-KMZ

      public string GarminKmzFile(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, "");
      }

      public void SetGarminKmzFile(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, value);
      }

      #endregion

      #region spez. Providereigenschaften für WMS

      public string WmsUrl(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, "");
      }

      public void SetWmsUrl(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, value);
      }

      public string WmsVersion(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, "");
      }

      public void SetWmsVersion(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, value);
      }

      public string WmsSrs(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, "");
      }

      public void SetWmsSrs(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, value);
      }

      public string WmsPictFormat(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, "");
      }

      public void SetWmsPictFormat(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, value);
      }

      public string WmsLayers(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, "");
      }

      public void SetWmsLayers(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, value);
      }

      public string WmsExtend(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, "");
      }

      public void SetWmsExtend(int providx, string value) {
         setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, value);
      }

      #endregion

      #region Proxy-Definition

      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stadtproxy.stadt.leipzig.de"
      /// </summary>
      public string WebProxyName {
         get => ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYNAME, null);
         set => setXPath("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYNAME, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: 80
      /// </summary>
      public int WebProxyPort {
         get => ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPORT, 0);
         set => setXPath("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPORT, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stinnerfr@leipzig.de"
      /// </summary>
      public string WebProxyUser {
         get => ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYUSER, null);
         set => setXPath("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYUSER, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig
      /// </summary>
      public string WebProxyPassword {
         get => ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPASSWORD, null);
         set => setXPath("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPASSWORD, value);
      }

      #endregion

      #region Trackfarben und -breiten

      Color getPenColor(string tracktype) {
         int a = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORA, 255);
         int r = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORR, 0);
         int g = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORG, 0);
         int b = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORB, 0);
         return Color.FromArgb(a, r, g, b);
      }

      void setPenColor(string tracktype, Color color) {
         setXPath("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORA, color.A);
         setXPath("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORR, color.R);
         setXPath("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORG, color.G);
         setXPath("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORB, color.B);
      }

      float getPenWidth(string tracktype) {
         return (float)ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKWIDTH, 1.0);
      }

      void setPenWidth(string tracktype, float width) {
         setXPath("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKWIDTH, width);
      }

      public Color StandardTrackColor {
         get => getPenColor(XML_STANDARDTRACK);
         set => setPenColor(XML_STANDARDTRACK, value);
      }

      public Color StandardTrackColor2 {
         get => getPenColor(XML_STANDARDTRACK2);
         set => setPenColor(XML_STANDARDTRACK2, value);
      }

      public Color StandardTrackColor3 {
         get => getPenColor(XML_STANDARDTRACK3);
         set => setPenColor(XML_STANDARDTRACK3, value);
      }

      public Color StandardTrackColor4 {
         get => getPenColor(XML_STANDARDTRACK4);
         set => setPenColor(XML_STANDARDTRACK4, value);
      }

      public Color StandardTrackColor5 {
         get => getPenColor(XML_STANDARDTRACK5);
         set => setPenColor(XML_STANDARDTRACK5, value);
      }

      public float StandardTrackWidth {
         get => getPenWidth(XML_STANDARDTRACK);
         set => setPenWidth(XML_STANDARDTRACK, value);
      }

      public float StandardTrackWidth2 {
         get => getPenWidth(XML_STANDARDTRACK2);
         set => setPenWidth(XML_STANDARDTRACK2, value);
      }

      public float StandardTrackWidth3 {
         get => getPenWidth(XML_STANDARDTRACK3);
         set => setPenWidth(XML_STANDARDTRACK3, value);
      }

      public float StandardTrackWidth4 {
         get => getPenWidth(XML_STANDARDTRACK4);
         set => setPenWidth(XML_STANDARDTRACK4, value);
      }

      public float StandardTrackWidth5 {
         get => getPenWidth(XML_STANDARDTRACK5);
         set => setPenWidth(XML_STANDARDTRACK5, value);
      }

      public Color MarkedTrackColor {
         get => getPenColor(XML_MARKEDTRACK);
         set => setPenColor(XML_MARKEDTRACK, value);
      }

      public float MarkedTrackWidth {
         get => getPenWidth(XML_MARKEDTRACK);
         set => setPenWidth(XML_MARKEDTRACK, value);
      }

      public Color EditableTrackColor {
         get => getPenColor(XML_EDITABLETRACK);
         set => setPenColor(XML_EDITABLETRACK, value);
      }

      public float EditableTrackWidth {
         get => getPenWidth(XML_EDITABLETRACK);
         set => setPenWidth(XML_EDITABLETRACK, value);
      }

      public Color InEditTrackColor {
         get => getPenColor(XML_INEDITTRACK);
         set => setPenColor(XML_INEDITTRACK, value);
      }

      public float InEditTrackWidth {
         get => getPenWidth(XML_INEDITTRACK);
         set => setPenWidth(XML_INEDITTRACK, value);
      }

      public Color HelperLineColor {
         get => getPenColor(XML_HELPERLINE);
         set => setPenColor(XML_HELPERLINE, value);
      }

      public float HelperLineWidth {
         get => getPenWidth(XML_HELPERLINE);
         set => setPenWidth(XML_HELPERLINE, value);
      }

      public Color SelectedPartTrackColor {
         get => getPenColor(XML_SELPARTTRACK);
         set => setPenColor(XML_SELPARTTRACK, value);
      }

      public float SelectedPartTrackWidth {
         get => getPenWidth(XML_SELPARTTRACK);
         set => setPenWidth(XML_SELPARTTRACK, value);
      }

      #endregion

      #region Slope

      public Color[] SlopeColors(out int[] percent) {
         string basepath = "/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + XML_SECTION_SLOPE + "/" + XML_SLOPE + "/";
         int[] a = ReadInt(basepath + XML_SLOPECOLORA, 255);
         int[] r = ReadInt(basepath + XML_SLOPECOLORR, 0);
         int[] g = ReadInt(basepath + XML_SLOPECOLORG, 0);
         int[] b = ReadInt(basepath + XML_SLOPECOLORB, 0);
         int[] p = ReadInt(basepath + XML_SLOPEPERCENT, 0);

         SortedDictionary<int, Color> tmp = new SortedDictionary<int, Color>();
         for (int i = 0; i < p.Length; i++)
            if (!tmp.ContainsKey(p[i]))
               tmp.Add(p[i], Color.FromArgb(a != null && i < a.Length ? a[i] : 255,
                                            r != null && i < r.Length ? r[i] : 0,
                                            g != null && i < g.Length ? g[i] : 0,
                                            b != null && i < b.Length ? b[i] : 0));
         percent = new int[tmp.Count];
         tmp.Keys.CopyTo(percent, 0);
         Color[] cols = new Color[tmp.Count];
         tmp.Values.CopyTo(cols, 0);

         return cols;
      }

      public void SetSlopeColors(int[] percent, Color[] color) {
         string basepath = "/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + XML_SECTION_SLOPE + "/" + XML_SLOPE;
         for (int i = 0; i < percent.Length && i < color.Length; i++) {
            string basepathi = basepath + "[" + (i + 1).ToString() + "]/";
            setXPath(basepathi + XML_SLOPECOLORA, color[i].A);
            setXPath(basepathi + XML_SLOPECOLORR, color[i].R);
            setXPath(basepathi + XML_SLOPECOLORG, color[i].G);
            setXPath(basepathi + XML_SLOPECOLORB, color[i].B);
            setXPath(basepathi + XML_SLOPEPERCENT, percent[i]);
         }
      }

      #endregion

      #region Garmin-Symbole

      /// <summary>
      /// liefert alle Gruppennamen der Garminsymbole
      /// </summary>
      /// <returns></returns>
      public string[] GetGarminMarkerSymbolGroupnames() {
         return ReadString("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "/" + XML_GARMINSYMBOLGROUPNAME);
      }

      /// <summary>
      /// liefert alle Symbolnamen einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx">Gruppenindex</param>
      /// <returns></returns>
      public string[] GetGarminMarkerSymbolnames(int groupidx) {
         return ReadString("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "[" + (groupidx + 1).ToString() + "]/" + XML_GARMINSYMBOL + "/" + XML_GARMINSYMBOLNAME);
      }

      /// <summary>
      /// liefert den anzuzeigenden Text zu einem Symbol einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx">Gruppenindex</param>
      /// <param name="idx">Symbolindex innerhalb der Gruppe</param>
      /// <returns></returns>
      public string GetGarminMarkerSymboltext(int groupidx, int idx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "[" + (groupidx + 1).ToString() + "]/" + XML_GARMINSYMBOL + "[" + (idx + 1).ToString() + "]/" + XML_GARMINSYMBOLTEXT, "");
      }

      /// <summary>
      /// liefert den Offset zum Bezugspunkt in Pixeln (der sonst in der Mitte des Bildes liegt)
      /// </summary>
      /// <param name="groupidx"></param>
      /// <param name="idx"></param>
      /// <param name="offsetx"></param>
      /// <param name="offsety"></param>
      /// <returns></returns>
      public bool GetGarminMarkerSymboloffset(int groupidx, int idx, out int offsetx, out int offsety) {
         offsetx = offsety = 0;
         string offsettext = ReadValue("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "[" + (groupidx + 1).ToString() + "]/" + XML_GARMINSYMBOL + "[" + (idx + 1).ToString() + "]/" + XML_GARMINSYMBOLOFFSET, "");
         if (!string.IsNullOrEmpty(offsettext)) {
            offsettext = offsettext.Trim();
            string[] offsets = offsettext.Split(',');
            if (offsets.Length == 2) {
               try {
                  offsetx = Convert.ToInt32(offsets[0]);
                  offsety = Convert.ToInt32(offsets[1]);
                  return true;
               } catch { }
            }
         }
         return false;
      }

      /// <summary>
      /// liefert die Grafikdatei zu einem Symbol einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GetGarminMarkerSymbolfile(int groupidx, int idx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "[" + (groupidx + 1).ToString() + "]/" + XML_GARMINSYMBOL + "[" + (idx + 1).ToString() + "]", "");
      }

      #endregion
   }
}
