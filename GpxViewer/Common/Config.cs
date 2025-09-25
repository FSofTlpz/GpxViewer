//#define LOCALDEBUG

using FSofTUtils;
using System.Diagnostics;
using System.Text;
using System.Xml.XPath;
using MyDrawing = System.Drawing;

#if ANDROID
namespace TrackEddi.Common {
   public class Config : SimpleXmlDocument2 {
#else
namespace GpxViewer.Common {
   class Config : SimpleXmlDocument2 {
#endif

      #region Xpaths

      /// <summary>
      /// liefert alle notwendigen XPaths
      /// </summary>
      class XPaths {

         public const string Slash = "/";
         const string BracketOn = "[";
         const string BracketOff = "]";


         public const string XML_ROOT = "*";

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
         const string XML_ZOOM4DISPLAYFACTOR = "zoom4displayfactor";
         const string XML_SYMBOLZOOMFACTOR = "symbolzoomfactor";
         const string XML_DELTAPERCENT4SEARCH = "deltapercent4search";
         const string XML_CLICKTOLERANCE4TRACKS = "clicktolerance4tracks";

         const string XML_DEMPATH = "dem";
         const string XML_DEMCACHESIZE = "@cachesize";
         const string XML_DEMCACHEPATH = "@cachepath";
         const string XML_DEMMINZOOM = "@minzoom";
         const string XML_DEMHILLSHADINGAZIMUT = "@hillshadingazimut";
         const string XML_DEMHILLSHADINGALTITUDE = "@hillshadingaltitude";
         const string XML_DEMHILLSHADINGSCALE = "@hillshadingscale";

         public const string XML_PROVIDERGROUP = "providergroup";
         public const string XML_PROVIDERGROUPNAME = "@name";
         public const string XML_PROVIDER = "provider";
         public const string XML_MAPNAME = "@mapname";
         public const string XML_MINZOOM = "@minzoom";
         public const string XML_MAXZOOM = "@maxzoom";

         const string XML_LASTMAPNAMES = "lastmapnames";

         //const string XML_HILLSHADINGSRTM = "@srtm";
         public const string XML_HILLSHADING = "@hillshading";
         public const string XML_HILLSHADINGALPHA = "@hillshadingalpha";

         public const string XML_GARMIN_TDB = "@tdb";
         public const string XML_GARMIN_TYP = "@typ";
         public const string XML_GARMIN_TEXTFACTOR = "@textfactor";
         public const string XML_GARMIN_SYMBOLFACTOR = "@symbolfactor";
         public const string XML_GARMIN_LINEFACTOR = "@linefactor";

         public const string XML_GARMINKMZ_KMZFILE = "@kmzfile";

         public const string XML_WMS_URL = "@url";
         public const string XML_WMS_VERSION = "@version";
         public const string XML_WMS_SRS = "@srs";
         public const string XML_WMS_PICTFORMAT = "@format";
         public const string XML_WMS_LAYERS = "@layers";
         public const string XML_WMS_EXT = "@extended";

         public const string XML_TYPEMULTIPROVIDER = "type";

         const string XML_SECTION_TRACKS = "tracks";
         public const string XML_STANDARDTRACK = "standard";
         public const string XML_STANDARDTRACK2 = "standard2";
         public const string XML_STANDARDTRACK3 = "standard3";
         public const string XML_STANDARDTRACK4 = "standard4";
         public const string XML_STANDARDTRACK5 = "standard5";
         public const string XML_LIVETRACK = "live";
         public const string XML_MARKEDTRACK = "marked";
         public const string XML_EDITABLETRACK = "editable";
         public const string XML_MARKED4EDITTRACK = "marked4edit";
         public const string XML_INEDITTRACK = "inedit";
         public const string XML_SELPARTTRACK = "selectedpart";
         public const string XML_HELPERLINE = "helperline";
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

         const string XML_SECTION_LIVELOCATION = "livelocation";
         const string XML_LOCATIONSYMBOLSIZE = "locationsymbolsize";
         const string XML_LOCATIONUPDATE = "update";
         const string XML_LOCATIONUPDATEINTERVALL = "@intervall";
         const string XML_LOCATIONUPDATEDISTANCE = "@distance";
         const string XML_TRACKING = "tracking";
         const string XML_MINIMALPOINTDISTANCE = "@minimalpointdistance";
         const string XML_MINIMALHEIGHTDISTANCE = "@minimalheightdistance";

         const string XML_GARMINSYMBOLS = "garminsymbols";
         const string XML_GARMINSYMBOLGROUP = "group";
         const string XML_GARMINSYMBOLGROUPNAME = "@name";
         const string XML_GARMINSYMBOL = "symbol";
         const string XML_GARMINSYMBOLNAME = "@name";
         const string XML_GARMINSYMBOLTEXT = "@text";
         const string XML_GARMINSYMBOLOFFSET = "@offset";


         static string idx(int i) => BracketOn + (i + 1).ToString() + BracketOff;

         const string minimalTrackpointDistance = Slash + XML_ROOT + Slash + XML_MINIMALTRACKPOINTDISTANCE + Slash;
         public const string MinimalTrackpointDistanceX = minimalTrackpointDistance + XML_MINIMALTRACKPOINTDISTANCE_X;
         public const string MinimalTrackpointDistanceY = minimalTrackpointDistance + XML_MINIMALTRACKPOINTDISTANCE_Y;
         const string map = Slash + XML_ROOT + Slash + XML_MAP + Slash;
         public const string CacheLocation = map + XML_CACHELOCATION;
         public const string ServerOnly = map + XML_SERVERONLY;
         public const string StartProvider = map + XML_STARTPROVIDER;
         public const string StartZoom = map + XML_STARTZOOM;
         public const string DeltaPercent4Search = map + XML_DELTAPERCENT4SEARCH;
         public const string StartLatitude = map + XML_STARTLATITUDE;
         public const string StartLongitude = map + XML_STARTLONGITUDE;
         public const string ScreenZoomfactor = map + XML_ZOOM4DISPLAYFACTOR;
         public const string SymbolZoomfactor = map + XML_SYMBOLZOOMFACTOR;
         public const string ClickTolerance4Tracks = map + XML_CLICKTOLERANCE4TRACKS;

         public const string DemPath = Slash + XML_ROOT + Slash + XML_MAP + Slash + XML_DEMPATH;
         public const string DemCachesize = DemPath + Slash + XML_DEMCACHESIZE;
         public const string DemCachePath = DemPath + Slash + XML_DEMCACHEPATH;
         public const string DemMinZoom = DemPath + Slash + XML_DEMMINZOOM;
         public const string DemHillshadingAzimut = DemPath + Slash + XML_DEMHILLSHADINGAZIMUT;
         public const string DemHillshadingAltitude = DemPath + Slash + XML_DEMHILLSHADINGALTITUDE;
         public const string DemHillshadingScale = DemPath + Slash + XML_DEMHILLSHADINGSCALE;

         public static string Path4ProviderGroup(IList<int> idxlst, int length) {
            StringBuilder sb = new StringBuilder(Slash + XML_ROOT + Slash + XML_MAP);
            if (length < 0)
               length = idxlst.Count;
            for (int i = 0; i < length; i++)
               sb.Append(Slash + XML_PROVIDERGROUP + idx(idxlst[i]));
            return sb.ToString();
         }

         /// <summary>
         /// Pfad mit ev. mehreren Providergruppen und einem Provider (oder einem 2. Provider bei Multiprovider)
         /// </summary>
         /// <param name="idxlst"></param>
         /// <param name="multiidx"></param>
         /// <returns></returns>
         static string getXPath4Provider(IList<int> idxlst, int multiidx) {
            StringBuilder sb = new StringBuilder(Slash + XML_ROOT + Slash + XML_MAP);
            for (int i = 0; i < idxlst.Count; i++)
               sb.Append(Slash + (i < idxlst.Count - 1 ? XML_PROVIDERGROUP : XML_PROVIDER) + idx(idxlst[i]));
            if (multiidx >= 0)
               sb.Append(Slash + XML_PROVIDER + idx(multiidx));
            return sb.ToString();
         }

         public static string ProviderGroupName(IList<int> providxlst, int length) => Path4ProviderGroup(providxlst, length) + Slash + XML_PROVIDERGROUPNAME;
         public static string ProviderName(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx);
         public static string ProviderNameExt(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_TYPEMULTIPROVIDER;
         public static string MapName(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_MAPNAME;
         public static string MinZoom(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_MINZOOM;
         public static string MaxZoom(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_MAXZOOM;
         //public static string Zoom4Display(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_ZOOM4DISPLAY;
         public static string Hillshading(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_HILLSHADING;
         public static string HillshadingAlpha(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_HILLSHADINGALPHA;

         public static string GarminTdb(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMIN_TDB;
         public static string GarminTyp(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMIN_TYP;
         public static string GarminTextFactor(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMIN_TEXTFACTOR;
         public static string GarminSymbolFactor(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMIN_SYMBOLFACTOR;
         public static string GarminLineFactor(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMIN_LINEFACTOR;

         public static string GarminKmzFile(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_GARMINKMZ_KMZFILE;
         public static string WmsUrl(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_URL;
         public static string WmsVersion(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_VERSION;
         public static string WmsSrs(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_SRS;
         public static string WmsPictFormat(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_PICTFORMAT;
         public static string WmsLayers(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_LAYERS;
         public static string WmsExtend(IList<int> providxlst, int multiidx) => getXPath4Provider(providxlst, multiidx) + Slash + XML_WMS_EXT;

         public const string LastUsedMapsCount = Slash + XML_ROOT + Slash + XML_MAP + Slash + XML_LASTMAPNAMES;

         public const string WebProxyName = Slash + XML_ROOT + Slash + XML_SECTION_PROXY + Slash + XML_PROXYNAME;
         public const string WebProxyPort = Slash + XML_ROOT + Slash + XML_SECTION_PROXY + Slash + XML_PROXYPORT;
         public const string WebProxyUser = Slash + XML_ROOT + Slash + XML_SECTION_PROXY + Slash + XML_PROXYUSER;
         public const string WebProxyPassword = Slash + XML_ROOT + Slash + XML_SECTION_PROXY + Slash + XML_PROXYPASSWORD;

         const string tracks = Slash + XML_ROOT + Slash + XML_SECTION_TRACKS;
         public static string TrackcolorA(string tracktype) => tracks + Slash + tracktype + Slash + XML_TRACKCOLORA;
         public static string TrackcolorR(string tracktype) => tracks + Slash + tracktype + Slash + XML_TRACKCOLORR;
         public static string TrackcolorG(string tracktype) => tracks + Slash + tracktype + Slash + XML_TRACKCOLORG;
         public static string TrackcolorB(string tracktype) => tracks + Slash + tracktype + Slash + XML_TRACKCOLORB;
         public static string PenWidth(string tracktype) => tracks + Slash + tracktype + Slash + XML_TRACKWIDTH;

         const string tracksslope = Slash + XML_ROOT + Slash + XML_SECTION_TRACKS + Slash + XML_SECTION_SLOPE + Slash + XML_SLOPE;
         public const string AllSlopecolorA = tracksslope + Slash + XML_SLOPECOLORA;
         public const string AllSlopecolorR = tracksslope + Slash + XML_SLOPECOLORR;
         public const string AllSlopecolorG = tracksslope + Slash + XML_SLOPECOLORG;
         public const string AllSlopecolorB = tracksslope + Slash + XML_SLOPECOLORB;
         public const string AllSlopePercent = tracksslope + Slash + XML_SLOPEPERCENT;
         static string tracksslope4idx(int i) => tracksslope + idx(i) + "/";
         public static string SlopecolorA(int i) => tracksslope4idx(i) + XML_SLOPECOLORA;
         public static string SlopecolorR(int i) => tracksslope4idx(i) + XML_SLOPECOLORR;
         public static string SlopecolorG(int i) => tracksslope4idx(i) + XML_SLOPECOLORG;
         public static string SlopecolorB(int i) => tracksslope4idx(i) + XML_SLOPECOLORB;
         public static string SlopePercent(int i) => tracksslope4idx(i) + XML_SLOPEPERCENT;

         const string liveLocation = Slash + XML_ROOT + Slash + XML_SECTION_LIVELOCATION + Slash;
         public const string LocationSymbolsize = liveLocation + XML_LOCATIONSYMBOLSIZE;
         public const string LocationUpdateIntervall = liveLocation + XML_LOCATIONUPDATE + Slash + XML_LOCATIONUPDATEINTERVALL;
         public const string LocationUpdateDistance = liveLocation + XML_LOCATIONUPDATE + Slash + XML_LOCATIONUPDATEDISTANCE;
         public const string TrackingMinimalPointdistance = liveLocation + XML_TRACKING + Slash + XML_MINIMALPOINTDISTANCE;
         public const string TrackingMinimalHeightdistance = liveLocation + XML_TRACKING + Slash + XML_MINIMALHEIGHTDISTANCE;

         const string garmingroup = Slash + XML_ROOT + Slash + XML_GARMINSYMBOLS + Slash + XML_GARMINSYMBOLGROUP;
         static string garmingroup4idx(int groupidx) => garmingroup + idx(groupidx) + "/";
         static string garmingroupandsymbol4idx(int groupidx, int symbolidx) => garmingroup4idx(groupidx) + XML_GARMINSYMBOL + idx(symbolidx);
         public const string GetGarminMarkerSymbolGroupnames = garmingroup + Slash + XML_GARMINSYMBOLGROUPNAME;
         public static string GetGarminMarkerSymbolnames(int groupidx) => garmingroup4idx(groupidx) + XML_GARMINSYMBOL + Slash + XML_GARMINSYMBOLNAME;
         public static string GetGarminMarkerSymboltext(int groupidx, int symbolidx) => garmingroupandsymbol4idx(groupidx, symbolidx) + Slash + XML_GARMINSYMBOLTEXT;
         public static string GetGarminMarkerSymbolfile(int groupidx, int symbolidx) => garmingroupandsymbol4idx(groupidx, symbolidx);
         public static string GetGarminMarkerSymboloffset(int groupidx, int symbolidx) => garmingroupandsymbol4idx(groupidx, symbolidx) + Slash + XML_GARMINSYMBOLOFFSET;

         public const string MapsSectionContent = Slash + XML_ROOT + Slash + XML_MAP + Slash + XML_PROVIDERGROUP;

         public static string MapName(int providx) => Slash + XML_ROOT + Slash + XML_MAP + Slash + XML_PROVIDER + idx(providx) + "/" + XML_MAPNAME;

      }

      #endregion

      #region einige Standardwerte

      const int STDDELTAPERCENT4SEARCH =
#if Android
                                          10;
#else
                                          1;
#endif
      const double STDZOOM4DISPLAYFACTOR = 1.0;
      const double STDSYMBOLZOOMFACTOR = 1.0;
      const double STDCLICKTOLERANCE4TRACKS = 1.0;

      const int STDDEMMINZOOM = 11;
      const double STDDEMHILLSHADINGAZIMUT = 315.0;
      const double STDDEMHILLSHADINGALTITUDE = 45.0;
      const double STDDEMHILLSHADINGSCALE = 1.0;

      const int STDMINZOOM = 0;
      const int STDMAXZOOM = 24;
      const double STDZOOM4DISPLAY = 1.0;
      const bool STDHILLSHADING = false;
      const int STDHILLSHADINGALPHA = 80;

      const double STDGARMINTEXTFACTOR = 1.0;
      const double STDGARMINSYMBOLFACTOR = 1.0;
      const double STDGARMINLINEFACTOR = 1.0;

      #endregion


      public Config(string? configfile, string? xsdfile) :
         base(configfile, XPaths.XML_ROOT, xsdfile) {
         Validating = false;
         LoadData();
      }

      /// <summary>
      /// für float, double und decimal wird ein '.' geliefert
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      static string getInternationalString4Object(object value) {
         if (value is float)
            return ((float)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         else if (value is double)
            return ((double)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         else if (value is decimal)
            return ((decimal)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         string? tmp = value.ToString();
         return tmp != null ? tmp : string.Empty;
      }

      void setXPath(string xpath, object value) {
         string valuestring = getInternationalString4Object(value);

         if (ExistXPath(xpath))
            Change(xpath, valuestring);
         else {
            string[] xpathparts = xpath.Split('/');
            // letzten ex. xpath suchen
            int lastvalididx = -1;
            for (lastvalididx = xpathparts.Length - 2; lastvalididx > 0; lastvalididx--) {
               string tmpxpath = string.Join(XPaths.Slash, xpathparts, 0, lastvalididx + 1);
               if (ExistXPath(tmpxpath))
                  break;
            }

            for (int i = lastvalididx + 1; i < xpathparts.Length; i++) {
               string tmpxpath = string.Join(XPaths.Slash, xpathparts, 0, i);
               string name = xpathparts[i];
               if (name[0] != '@') {   // Nodename (ACHTUNG: Test ist zu einfach, falls [ nur im String)
                  int apos = xpathparts[i].IndexOf('[');
                  if (apos >= 0) {   // Array (bisher zu klein)
                     string nodename = name.Substring(0, apos);
                     while (!ExistXPath(tmpxpath + XPaths.Slash + name)) {
                        Append(tmpxpath, nodename);
                     }
                     continue;
                  }
                  Append(tmpxpath,
                         name,
                         i == xpathparts.Length - 1 ?
                              valuestring :
                              null);
               } else {                         // Attribut
                  Append(tmpxpath,
                         null,
                         null,
                         new Dictionary<string, string>() {
                            { xpathparts[i].Substring(1), valuestring }
                         });
               }
            }

         }
      }

      public int MinimalTrackpointDistanceX {
         get => ReadValue(XPaths.MinimalTrackpointDistanceX, 14);
         set => setXPath(XPaths.MinimalTrackpointDistanceX, value);
      }

      public int MinimalTrackpointDistanceY {
         get => ReadValue(XPaths.MinimalTrackpointDistanceY, 14);
         set => setXPath(XPaths.MinimalTrackpointDistanceY, value);
      }

      public string CacheLocation {
         get => ReadValue(XPaths.CacheLocation, string.Empty);
         set => setXPath(XPaths.CacheLocation, value);
      }

      public bool ServerOnly {
         get => ReadValue(XPaths.ServerOnly, true);
         set => setXPath(XPaths.ServerOnly, value);
      }

      public int StartProvider {
         get => ReadValue(XPaths.StartProvider, 0);
         set => setXPath(XPaths.StartProvider, value);
      }

      public int StartZoom {
         get => ReadValue(XPaths.StartZoom, 16);
         set => setXPath(XPaths.StartZoom, value);
      }

      //public double Zoom4Display {
      //   get => ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_ZOOM4DISPLAY, 1.0);
      //   set => setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_ZOOM4DISPLAY, value);
      //}

      public int DeltaPercent4Search {
         get => ReadValue(XPaths.DeltaPercent4Search, STDDELTAPERCENT4SEARCH);
         set => setXPath(XPaths.DeltaPercent4Search, value);
      }

      public double StartLatitude {
         get => ReadValue(XPaths.StartLatitude, 51.30);
         set => setXPath(XPaths.StartLatitude, value);
      }

      public double StartLongitude {
         get => ReadValue(XPaths.StartLongitude, 12.40);
         set => setXPath(XPaths.StartLongitude, value);
      }

      public double SymbolZoomfactor {
         get => ReadValue(XPaths.SymbolZoomfactor, STDSYMBOLZOOMFACTOR);
         set => setXPath(XPaths.SymbolZoomfactor, value);
      }

      public double Zoom4Displayfactor {
         get => ReadValue(XPaths.ScreenZoomfactor, STDZOOM4DISPLAYFACTOR);
         set => setXPath(XPaths.ScreenZoomfactor, value);
      }

      public double ClickTolerance4Tracks {
         get => ReadValue(XPaths.ClickTolerance4Tracks, STDCLICKTOLERANCE4TRACKS);
         set => setXPath(XPaths.ClickTolerance4Tracks, value);
      }

      #region XML_MAP / XML_DEMPATH

      public string DemPath {
         get => ReadValue(XPaths.DemPath, string.Empty);
         set => setXPath(XPaths.DemPath, value);
      }

      public int DemCachesize {
         get => ReadValue(XPaths.DemCachesize, 16);
         set => setXPath(XPaths.DemCachesize, value);
      }

      public string DemCachePath {
         get => ReadValue(XPaths.DemCachePath, string.Empty);
         set => setXPath(XPaths.DemCachePath, value);
      }

      public int DemMinZoom {
         get => ReadValue(XPaths.DemMinZoom, STDDEMMINZOOM);
         set => setXPath(XPaths.DemMinZoom, value);
      }

      public double DemHillshadingAzimut {
         get => ReadValue(XPaths.DemHillshadingAzimut, STDDEMHILLSHADINGAZIMUT);
         set => setXPath(XPaths.DemHillshadingAzimut, value);
      }

      public double DemHillshadingAltitude {
         get => ReadValue(XPaths.DemHillshadingAltitude, STDDEMHILLSHADINGALTITUDE);
         set => setXPath(XPaths.DemHillshadingAltitude, value);
      }

      public double DemHillshadingScale {
         get => ReadValue(XPaths.DemHillshadingScale, STDDEMHILLSHADINGSCALE);
         set => setXPath(XPaths.DemHillshadingScale, value);
      }

      #endregion

      public class ArrayProperty {

         static protected Config? cfg;

         public ArrayProperty(Config cfgowner) {
            cfg = cfgowner;
         }
      }

      public class MapNameIdx : ArrayProperty {

         public MapNameIdx(Config cfg) : base(cfg) { }

         public string this[int providx] {
            get => cfg != null ?
                     cfg.ReadValue(XPaths.MapName(providx), string.Empty) :
                     string.Empty;
            set => cfg?.setXPath(XPaths.MapName(providx), value);
         }
      }

      #region allgemeine Providereigenschaften

      class IdxPathHelper {
         public int GroupIdx;
         public int ProviderIdx;

         public IdxPathHelper(int groupidx, int provideridx) {
            GroupIdx = groupidx;
            ProviderIdx = provideridx;
         }

         public override string ToString() => "Group " + GroupIdx + ", Provider " + ProviderIdx;

      }

      /// <summary>
      /// liefert die Indexpfade aller Karten (rekursiv, deshalb also "der Reihe nach")
      /// <para>Der letzte Index ist immer ein Kartenindex, alle davor sind Gruppenindexe.</para>
      /// <para>Der 1. Index ist immer 0 (Index der "Root-"-Providergruppe).</para>
      /// <para>Der 2. Index ist der Kartenindex ODER der Gruppenindex in der "Root-"-Providergruppe usw..</para>
      /// </summary>
      /// <returns></returns>
      public List<int[]> ProviderIdxPaths() {
         List<int[]> idxlst = new List<int[]>();
         XPathNodeIterator? maingroupnodesit = NavigatorSelect(XPaths.MapsSectionContent);
         if (maingroupnodesit != null) {
            if (maingroupnodesit.MoveNext()) {
               Stack<IdxPathHelper> stack = new Stack<IdxPathHelper>();
               stack.Push(new IdxPathHelper(0, 0));
               providerIdxPathsRecursiv(maingroupnodesit, stack, idxlst);
               /*
		<providergroup>                        0              G 0G
			<provider mapname="A" />               0           P    0P          0-0
			<providergroup name="Gruppe 1">        1           G    0G          
				<provider mapname="B" />               0        P       0P       0-0-0
				<provider mapname="C" />               1        P       1P       0-0-1
				<providergroup name="Gruppe 2">        2        G       0G       
					<provider mapname="D" />               0     P          0P    0-0-0-0
					<provider mapname="E" />               1     P          1P    0-0-0-1
				</providergroup >                                                
				<provider mapname="F" />               3        P       2P       0-0-2
			</providergroup>                                                    
			<provider mapname="G" />               2           P    1P          0-1
			<providergroup name="Gruppe 3">        3           G    1G          
				<provider mapname="H" />               0        P       0P       0-1-0
				<provider mapname="I" />               1        P       1P       0-1-1
			</providergroup >                                   
		</providergroup>                          
                */
            }
         }
#if LOCALDEBUG
         for (int i = 0; i < idxlst.Count; i++)
            Debug.WriteLine(string.Join<int>("-", idxlst[i]));
#endif
         return idxlst;
      }

      /// <summary>
      /// Helper for <see cref="ProviderIdxPaths"/>
      /// </summary>
      /// <param name="it"></param>
      /// <param name="stack"></param>
      /// <param name="idxpaths"></param>
      bool providerIdxPathsRecursiv(XPathNodeIterator it, Stack<IdxPathHelper> stack, List<int[]> idxpaths) {
         bool isprovider = false;
         if (it.Current != null) {
            if (it.Current.LocalName == XPaths.XML_PROVIDERGROUP) {
               XPathNodeIterator it2 = it.Current.SelectChildren(XPathNodeType.Element);
               int providx = 0;
               int groupidx = 0;
               while (it2.MoveNext()) {
                  stack.Push(new IdxPathHelper(groupidx, providx));
                  if (providerIdxPathsRecursiv(it2, stack, idxpaths))
                     providx++;
                  else
                     groupidx++;
               }
            } else if (it.Current.LocalName == XPaths.XML_PROVIDER) {
               IdxPathHelper[] tmparray = stack.ToArray();
               int[] idxpath = new int[tmparray.Length];
               for (int i = 0; i < tmparray.Length; i++)
                  idxpath[i] = i < tmparray.Length - 1 ?
                                             tmparray[tmparray.Length - 1 - i].GroupIdx :
                                             tmparray[tmparray.Length - 1 - i].ProviderIdx;
               idxpaths.Add(idxpath);
               isprovider = true;
            }
            stack.Pop();
         }
         return isprovider;
      }

      //string getXPath4ProviderGroup(IList<int> idxlst, int length) {
      //   StringBuilder sb = new StringBuilder(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP);
      //   if (length < 0)
      //      length = idxlst.Count;
      //   for (int i = 0; i < length; i++)
      //      sb.Append(XPaths.Slash + XML_PROVIDERGROUP + "[" + (idxlst[i] + 1) + "]");
      //   return sb.ToString();
      //}

      //string getXPath4Provider(IList<int> idxlst) {
      //   StringBuilder sb = new StringBuilder(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP);
      //   for (int i = 0; i < idxlst.Count; i++)
      //      sb.Append(XPaths.Slash + (i < idxlst.Count - 1 ? XML_PROVIDERGROUP : XML_PROVIDER) + "[" + (idxlst[i] + 1) + "]");
      //   return sb.ToString();
      //}

      /// <summary>
      /// ermittelt den Namen der Providergruppe
      /// </summary>
      /// <param name="providxlst"></param>
      /// <param name="length">wenn kleiner 0, wird die gesamte IDX-Liste verwendet, sonst nur ein Teil</param>
      /// <returns></returns>
      public string ProviderGroupName(IList<int> providxlst, int length = -1) => ReadValue(XPaths.ProviderGroupName(providxlst, length), string.Empty);

      public string ProviderName(IList<int> providxlst, int multiidx) {
         string name = ReadValue(XPaths.ProviderNameExt(providxlst, multiidx), string.Empty);     // falls dieser Knoten ex. (z.Z. nur bei MultiMap) 
         if (name == string.Empty)
            name = ReadValue(XPaths.ProviderName(providxlst, multiidx), string.Empty);
         return name;
      }

      public string MapName(IList<int> providxlst, int multiidx) => ReadValue(XPaths.MapName(providxlst, multiidx), string.Empty);

      public int MinZoom(IList<int> providxlst, int multiidx) => Math.Max(0, ReadValue(XPaths.MinZoom(providxlst, multiidx), STDMINZOOM));

      public int MaxZoom(IList<int> providxlst, int multiidx) => Math.Min(ReadValue(XPaths.MaxZoom(providxlst, multiidx), STDMAXZOOM), 24);

      public bool Hillshading(IList<int> providxlst, int multiidx) => ReadValue(XPaths.Hillshading(providxlst, multiidx), STDHILLSHADING);

      public byte HillshadingAlpha(IList<int> providxlst, int multiidx) => (byte)(ReadValue(XPaths.HillshadingAlpha(providxlst, multiidx), STDHILLSHADINGALPHA) & 0xFF);

      #endregion

      #region spez. Providereigenschaften für Garmin

      public string GarminTdb(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminTdb(providxlst, multiidx), string.Empty);

      public string GarminTyp(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminTyp(providxlst, multiidx), string.Empty);

      public double GarminTextFactor(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminTextFactor(providxlst, multiidx), STDGARMINTEXTFACTOR);

      public double GarminSymbolFactor(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminSymbolFactor(providxlst, multiidx), STDGARMINSYMBOLFACTOR);

      public double GarminLineFactor(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminLineFactor(providxlst, multiidx), STDGARMINLINEFACTOR);

      //public string GarminTdb(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, string.Empty);
      //}

      //public void SetGarminTdb(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, value);
      //}

      //public string GarminTyp(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, string.Empty);
      //}

      //public void SetGarminTyp(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, value);
      //}

      //public int[] GarminLocalCacheLevels(int providx) {
      //   string text = ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, string.Empty);
      //   string[] tmp = text.Split(new char[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
      //   int[] v = new int[tmp.Length];
      //   for (int i = 0; i < tmp.Length; i++)
      //      try {
      //         v[i] = Convert.ToInt32(tmp[i]);
      //      } catch {
      //         v[i] = 0;
      //      }
      //   return v;
      //}

      //public void SetGarminLocalCacheLevels(int providx, int[] value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, string.Join(",", value));
      //}

      //public int GarminMaxSubdiv(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, 1000000);
      //}

      //public void SetGarminMaxSubdiv(int providx, int value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, value);
      //}

      //public double GarminTextFactor(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, 1.0);
      //}

      //public void SetGarminTextFactor(int providx, double value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, value);
      //}

      //public double GarminSymbolFactor(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, 1.0);
      //}

      //public void SetGarminSymbolFactor(int providx, double value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, value);
      //}

      //public double GarminLineFactor(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, 1.0);
      //}

      //public void SetGarminLineFactor(int providx, double value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, value);
      //}

      #endregion

      #region spez. Providereigenschaften für Garmin-KMZ

      public string GarminKmzFile(IList<int> providxlst, int multiidx) => ReadValue(XPaths.GarminKmzFile(providxlst, multiidx), string.Empty);

      //public string GarminKmzFile(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, string.Empty);
      //}

      //public void SetGarminKmzFile(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, value);
      //}

      #endregion

      #region spez. Providereigenschaften für WMS

      public string WmsUrl(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsUrl(providxlst, multiidx), string.Empty);

      public string WmsVersion(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsVersion(providxlst, multiidx), string.Empty);

      public string WmsSrs(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsSrs(providxlst, multiidx), string.Empty);

      public string WmsPictFormat(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsPictFormat(providxlst, multiidx), string.Empty);

      public string WmsLayers(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsLayers(providxlst, multiidx), string.Empty);

      public string WmsExtend(IList<int> providxlst, int multiidx) => ReadValue(XPaths.WmsExtend(providxlst, multiidx), string.Empty);

      //public string WmsUrl(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, string.Empty);
      //}

      //public void SetWmsUrl(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, value);
      //}

      //public string WmsVersion(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, string.Empty);
      //}

      //public void SetWmsVersion(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, value);
      //}

      //public string WmsSrs(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, string.Empty);
      //}

      //public void SetWmsSrs(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, value);
      //}

      //public string WmsPictFormat(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, string.Empty);
      //}

      //public void SetWmsPictFormat(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, value);
      //}

      //public string WmsLayers(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, string.Empty);
      //}

      //public void SetWmsLayers(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, value);
      //}

      //public string WmsExtend(int providx) {
      //   return ReadValue(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, string.Empty);
      //}

      //public void SetWmsExtend(int providx, string value) {
      //   setXPath(XPaths.Slash + XML_ROOT + XPaths.Slash + XML_MAP + XPaths.Slash + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, value);
      //}

      #endregion

      #region Map-Menü der zuletzt genutzten Karten

      public int LastUsedMapsCount {
         get => ReadValue(XPaths.LastUsedMapsCount, 3);
         set => setXPath(XPaths.LastUsedMapsCount, value);
      }

      #endregion

      #region Proxy-Definition

      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stadtproxy.stadt.leipzig.de"
      /// </summary>
      public string WebProxyName {
         get => ReadValue(XPaths.WebProxyName, string.Empty);
         set => setXPath(XPaths.WebProxyName, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: 80
      /// </summary>
      public int WebProxyPort {
         get => ReadValue(XPaths.WebProxyPort, 0);
         set => setXPath(XPaths.WebProxyPort, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stinnerfr@leipzig.de"
      /// </summary>
      public string WebProxyUser {
         get => ReadValue(XPaths.WebProxyUser, string.Empty);
         set => setXPath(XPaths.WebProxyUser, value);
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig
      /// </summary>
      public string WebProxyPassword {
         get => ReadValue(XPaths.WebProxyPassword, string.Empty);
         set => setXPath(XPaths.WebProxyPassword, value);
      }

      #endregion

      #region Trackfarben und -breiten

      MyDrawing.Color getPenColor(string tracktype) {
         int a = ReadValue(XPaths.TrackcolorA(tracktype), 255);
         int r = ReadValue(XPaths.TrackcolorR(tracktype), 0);
         int g = ReadValue(XPaths.TrackcolorG(tracktype), 0);
         int b = ReadValue(XPaths.TrackcolorB(tracktype), 0);
         return MyDrawing.Color.FromArgb(a, r, g, b);
      }

      void setPenColor(string tracktype, MyDrawing.Color color) {
         setXPath(XPaths.TrackcolorA(tracktype), color.A);
         setXPath(XPaths.TrackcolorR(tracktype), color.R);
         setXPath(XPaths.TrackcolorG(tracktype), color.G);
         setXPath(XPaths.TrackcolorB(tracktype), color.B);
      }

      float getPenWidth(string tracktype) => (float)ReadValue(XPaths.PenWidth(tracktype), 1.0);

      void setPenWidth(string tracktype, float width) => setXPath(XPaths.PenWidth(tracktype), width);

      public MyDrawing.Color StandardTrackColor {
         get => getPenColor(XPaths.XML_STANDARDTRACK);
         set => setPenColor(XPaths.XML_STANDARDTRACK, value);
      }

      public MyDrawing.Color StandardTrackColor2 {
         get => getPenColor(XPaths.XML_STANDARDTRACK2);
         set => setPenColor(XPaths.XML_STANDARDTRACK2, value);
      }

      public MyDrawing.Color StandardTrackColor3 {
         get => getPenColor(XPaths.XML_STANDARDTRACK3);
         set => setPenColor(XPaths.XML_STANDARDTRACK3, value);
      }

      public MyDrawing.Color StandardTrackColor4 {
         get => getPenColor(XPaths.XML_STANDARDTRACK4);
         set => setPenColor(XPaths.XML_STANDARDTRACK4, value);
      }

      public MyDrawing.Color StandardTrackColor5 {
         get => getPenColor(XPaths.XML_STANDARDTRACK5);
         set => setPenColor(XPaths.XML_STANDARDTRACK5, value);
      }

      public float StandardTrackWidth {
         get => getPenWidth(XPaths.XML_STANDARDTRACK);
         set => setPenWidth(XPaths.XML_STANDARDTRACK, value);
      }

      public float StandardTrackWidth2 {
         get => getPenWidth(XPaths.XML_STANDARDTRACK2);
         set => setPenWidth(XPaths.XML_STANDARDTRACK2, value);
      }

      public float StandardTrackWidth3 {
         get => getPenWidth(XPaths.XML_STANDARDTRACK3);
         set => setPenWidth(XPaths.XML_STANDARDTRACK3, value);
      }

      public float StandardTrackWidth4 {
         get => getPenWidth(XPaths.XML_STANDARDTRACK4);
         set => setPenWidth(XPaths.XML_STANDARDTRACK4, value);
      }

      public float StandardTrackWidth5 {
         get => getPenWidth(XPaths.XML_STANDARDTRACK5);
         set => setPenWidth(XPaths.XML_STANDARDTRACK5, value);
      }

      public MyDrawing.Color MarkedTrackColor {
         get => getPenColor(XPaths.XML_MARKEDTRACK);
         set => setPenColor(XPaths.XML_MARKEDTRACK, value);
      }

      public float MarkedTrackWidth {
         get => getPenWidth(XPaths.XML_MARKEDTRACK);
         set => setPenWidth(XPaths.XML_MARKEDTRACK, value);
      }

      public MyDrawing.Color LiveTrackColor {
         get => getPenColor(XPaths.XML_LIVETRACK);
         set => setPenColor(XPaths.XML_LIVETRACK, value);
      }

      public float LiveTrackWidth {
         get => getPenWidth(XPaths.XML_LIVETRACK);
         set => setPenWidth(XPaths.XML_LIVETRACK, value);
      }

      public MyDrawing.Color EditableTrackColor {
         get => getPenColor(XPaths.XML_EDITABLETRACK);
         set => setPenColor(XPaths.XML_EDITABLETRACK, value);
      }

      public float EditableTrackWidth {
         get => getPenWidth(XPaths.XML_EDITABLETRACK);
         set => setPenWidth(XPaths.XML_EDITABLETRACK, value);
      }

      public MyDrawing.Color Marked4EditColor {
         get => getPenColor(XPaths.XML_MARKED4EDITTRACK);
         set => setPenColor(XPaths.XML_MARKED4EDITTRACK, value);
      }

      public float Marked4EditWidth {
         get => getPenWidth(XPaths.XML_MARKED4EDITTRACK);
         set => setPenWidth(XPaths.XML_MARKED4EDITTRACK, value);
      }

      public MyDrawing.Color InEditTrackColor {
         get => getPenColor(XPaths.XML_INEDITTRACK);
         set => setPenColor(XPaths.XML_INEDITTRACK, value);
      }

      public float InEditTrackWidth {
         get => getPenWidth(XPaths.XML_INEDITTRACK);
         set => setPenWidth(XPaths.XML_INEDITTRACK, value);
      }

      public MyDrawing.Color HelperLineColor {
         get => getPenColor(XPaths.XML_HELPERLINE);
         set => setPenColor(XPaths.XML_HELPERLINE, value);
      }

      public float HelperLineWidth {
         get => getPenWidth(XPaths.XML_HELPERLINE);
         set => setPenWidth(XPaths.XML_HELPERLINE, value);
      }

      public MyDrawing.Color SelectedPartTrackColor {
         get => getPenColor(XPaths.XML_SELPARTTRACK);
         set => setPenColor(XPaths.XML_SELPARTTRACK, value);
      }

      public float SelectedPartTrackWidth {
         get => getPenWidth(XPaths.XML_SELPARTTRACK);
         set => setPenWidth(XPaths.XML_SELPARTTRACK, value);
      }

      #endregion

      #region Slope

      public MyDrawing.Color[] SlopeColors(out int[] percent) {
         int[]? a = ReadInt(XPaths.AllSlopecolorA, 255);
         int[]? r = ReadInt(XPaths.AllSlopecolorR, 0);
         int[]? g = ReadInt(XPaths.AllSlopecolorG, 0);
         int[]? b = ReadInt(XPaths.AllSlopecolorB, 0);
         int[]? p = ReadInt(XPaths.AllSlopePercent, 0);

         SortedDictionary<int, MyDrawing.Color> tmp = new SortedDictionary<int, MyDrawing.Color>();

         if (a != null && r != null && g != null && b != null && p != null)
            for (int i = 0; i < p.Length; i++)
               if (!tmp.ContainsKey(p[i]))
                  tmp.Add(p[i], MyDrawing.Color.FromArgb(a != null && i < a.Length ? a[i] : 255,
                                               r != null && i < r.Length ? r[i] : 0,
                                               g != null && i < g.Length ? g[i] : 0,
                                               b != null && i < b.Length ? b[i] : 0));
         percent = new int[tmp.Count];
         tmp.Keys.CopyTo(percent, 0);
         MyDrawing.Color[] cols = new MyDrawing.Color[tmp.Count];
         tmp.Values.CopyTo(cols, 0);

         return cols;
      }

      public void SetSlopeColors(int[] percent, MyDrawing.Color[] color) {
         for (int i = 0; i < percent.Length && i < color.Length; i++) {
            setXPath(XPaths.SlopecolorA(i), color[i].A);
            setXPath(XPaths.SlopecolorA(i), color[i].R);
            setXPath(XPaths.SlopecolorA(i), color[i].G);
            setXPath(XPaths.SlopecolorA(i), color[i].B);
            setXPath(XPaths.SlopePercent(i), percent[i]);
         }
      }

      #endregion

      #region Geo-Location

      public int LocationSymbolsize {
         get => ReadValue(XPaths.LocationSymbolsize, 50);
         set => setXPath(XPaths.LocationSymbolsize, value);
      }

      /// <summary>
      /// in ms
      /// </summary>
      public int LocationUpdateIntervall {
         get => ReadValue(XPaths.LocationUpdateIntervall, 1000);
         set => setXPath(XPaths.LocationUpdateIntervall, value);
      }

      /// <summary>
      /// in m
      /// </summary>
      public int LocationUpdateDistance {
         get => ReadValue(XPaths.LocationUpdateDistance, 5);
         set => setXPath(XPaths.LocationUpdateDistance, value);
      }

      public double TrackingMinimalPointdistance {
         get => ReadValue(XPaths.TrackingMinimalPointdistance, 2.0);
         set => setXPath(XPaths.TrackingMinimalPointdistance, value);
      }

      public double TrackingMinimalHeightdistance {
         get => ReadValue(XPaths.TrackingMinimalHeightdistance, 5.0);
         set => setXPath(XPaths.TrackingMinimalHeightdistance, value);
      }

      #endregion

      #region Garmin-Symbole

      /// <summary>
      /// liefert alle Gruppennamen der Garminsymbole
      /// </summary>
      /// <returns></returns>
      public string[]? GetGarminMarkerSymbolGroupnames() => ReadString(XPaths.GetGarminMarkerSymbolGroupnames);

      /// <summary>
      /// liefert alle Symbolnamen einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx">Gruppenindex</param>
      /// <returns></returns>
      public string[]? GetGarminMarkerSymbolnames(int groupidx) => ReadString(XPaths.GetGarminMarkerSymbolnames(groupidx));

      /// <summary>
      /// liefert den anzuzeigenden Text zu einem Symbol einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx">Gruppenindex</param>
      /// <param name="symidx">Symbolindex innerhalb der Gruppe</param>
      /// <returns></returns>
      public string GetGarminMarkerSymboltext(int groupidx, int symidx) => ReadValue(XPaths.GetGarminMarkerSymboltext(groupidx, symidx), string.Empty);

      /// <summary>
      /// liefert den Offset zum Bezugspunkt in Pixeln (der sonst in der Mitte des Bildes liegt)
      /// </summary>
      /// <param name="groupidx"></param>
      /// <param name="symbolidx"></param>
      /// <param name="offsetx"></param>
      /// <param name="offsety"></param>
      /// <returns></returns>
      public bool GetGarminMarkerSymboloffset(int groupidx, int symbolidx, out int offsetx, out int offsety) {
         offsetx = offsety = 0;
         string offsettext = ReadValue(XPaths.GetGarminMarkerSymboloffset(groupidx, symbolidx), string.Empty);
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
      /// <param name="symidx"></param>
      /// <returns></returns>
      public string GetGarminMarkerSymbolfile(int groupidx, int symidx) => ReadValue(XPaths.GetGarminMarkerSymbolfile(groupidx, symidx), string.Empty);

      #endregion

      #region Map-Section

      /// <summary>
      /// Inhalt der "Hauptkartengruppe" löschen
      /// </summary>
      /// <returns></returns>
      public bool RemoveMapsSectionContent() => Remove(XPaths.MapsSectionContent + "/*");

      Dictionary<string, string> getMapAttributeDict(string name,
                                                     int minzoom,
                                                     int maxzoom,
                                                     Dictionary<string, string>? extdict = null) {
         Dictionary<string, string> dict = new Dictionary<string, string>() {
                  { XPaths.XML_MAPNAME.Substring(1), name},
         };
         if (minzoom != STDMINZOOM)
            dict.Add(XPaths.XML_MINZOOM.Substring(1), getInternationalString4Object(minzoom));
         if (maxzoom != STDMAXZOOM)
            dict.Add(XPaths.XML_MAXZOOM.Substring(1), getInternationalString4Object(maxzoom));
         if (extdict != null)
            foreach (var item in extdict)
               dict.Add(item.Key, item.Value);
         return dict;
      }

      /// <summary>
      /// Standardkarte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="parentxpath"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            string parentxpath) =>
         Append(parentxpath,
                XPaths.XML_PROVIDER,
                providername,
                getMapAttributeDict(name, minzoom, maxzoom));

      /// <summary>
      /// Garmin-KMZ-Karte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="kmzfile"></param>
      /// <param name="parentxpath"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            string kmzfile,
                            bool hillshading,
                            int hillshadingalpha,
                            string parentxpath) {
         Dictionary<string, string> dict = new Dictionary<string, string>() {
            { XPaths.XML_GARMINKMZ_KMZFILE.Substring(1), kmzfile },
         };
         if (hillshading != STDHILLSHADING)
            dict.Add(XPaths.XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading));
         if (hillshadingalpha != STDHILLSHADINGALPHA)
            dict.Add(XPaths.XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha));
         return Append(parentxpath,
                       XPaths.XML_PROVIDER,
                       providername,
                       getMapAttributeDict(name,
                                           minzoom,
                                           maxzoom,
                                           dict));
      }

      /// <summary>
      /// Garminkarte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="tdbfile"></param>
      /// <param name="typfile"></param>
      /// <param name="textfactor"></param>
      /// <param name="symbolfactor"></param>
      /// <param name="linefactor"></param>
      /// <param name="hillshading"></param>
      /// <param name="hillshadingalpha"></param>
      /// <param name="parentxpath"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            string tdbfile,
                            string typfile,
                            double textfactor,
                            double symbolfactor,
                            double linefactor,
                            bool hillshading,
                            int hillshadingalpha,
                            string parentxpath) {
         Dictionary<string, string> dict = new Dictionary<string, string>() {
            { XPaths.XML_GARMIN_TDB.Substring(1), tdbfile },
            { XPaths.XML_GARMIN_TYP.Substring(1), typfile },
         };
         if (textfactor != STDGARMINTEXTFACTOR)
            dict.Add(XPaths.XML_GARMIN_TEXTFACTOR.Substring(1), getInternationalString4Object(textfactor));
         if (symbolfactor != STDGARMINSYMBOLFACTOR)
            dict.Add(XPaths.XML_GARMIN_SYMBOLFACTOR.Substring(1), getInternationalString4Object(symbolfactor));
         if (linefactor != STDGARMINLINEFACTOR)
            dict.Add(XPaths.XML_GARMIN_LINEFACTOR.Substring(1), getInternationalString4Object(linefactor));
         if (hillshading != STDHILLSHADING)
            dict.Add(XPaths.XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading));
         if (hillshadingalpha != STDHILLSHADINGALPHA)
            dict.Add(XPaths.XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha));
         return Append(parentxpath,
                       XPaths.XML_PROVIDER,
                       providername,
                       getMapAttributeDict(name,
                                           minzoom,
                                           maxzoom,
                                           dict));
      }

      /// <summary>
      /// WMS-Karte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="url"></param>
      /// <param name="version"></param>
      /// <param name="srs"></param>
      /// <param name="format"></param>
      /// <param name="layers"></param>
      /// <param name="extended"></param>
      /// <param name="parentxpath"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            string url,
                            string version,
                            string srs,
                            string format,
                            string layers,
                            string extended,
                            bool hillshading,
                            int hillshadingalpha,
                            string parentxpath) {
         Dictionary<string, string> dict = new Dictionary<string, string>() {
            { XPaths.XML_WMS_URL.Substring(1), url },
            { XPaths.XML_WMS_VERSION.Substring(1), version },
            { XPaths.XML_WMS_SRS.Substring(1), srs },
            { XPaths.XML_WMS_PICTFORMAT.Substring(1), format },
            { XPaths.XML_WMS_LAYERS.Substring(1), layers },
            { XPaths.XML_WMS_EXT.Substring(1), extended },
         };
         if (hillshading != STDHILLSHADING)
            dict.Add(XPaths.XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading));
         if (hillshadingalpha != STDHILLSHADINGALPHA)
            dict.Add(XPaths.XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha));
         return Append(parentxpath,
                  XPaths.XML_PROVIDER,
                  providername,
                  getMapAttributeDict(name, minzoom, maxzoom, dict));
      }

      /// <summary>
      /// Multi-Karte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="mapidx"></param>
      /// <param name="parentxpath"></param>
      /// <returns></returns>
      public string AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            bool hillshading,
                            int hillshadingalpha,
                            int mapidx,
                            string parentxpath) {
         Dictionary<string, string> dict = new Dictionary<string, string>();
         if (hillshading != STDHILLSHADING)
            dict.Add(XPaths.XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading));
         if (hillshadingalpha != STDHILLSHADINGALPHA)
            dict.Add(XPaths.XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha));
         Append(parentxpath,
                XPaths.XML_PROVIDER,
                null,
                getMapAttributeDict(name, minzoom, maxzoom, dict));
         // jetzt die untergeordneten Elemente
         parentxpath += "/" + XPaths.XML_PROVIDER + "[" + (mapidx + 1) + "]";
         Append(parentxpath,
                XPaths.XML_TYPEMULTIPROVIDER,
                providername);
         return parentxpath;
      }

      public static string XPath4ProviderGroup(IList<int> idxpathprovidergroup) => XPaths.Path4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count);

      public bool AppendMapGroup(string goupname, IList<int> idxpathprovidergroup) =>
         Append(XPaths.Path4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                XPaths.XML_PROVIDERGROUP,
                null,
                new Dictionary<string, string> {
                  { XPaths.XML_PROVIDERGROUPNAME.Substring(1), goupname},
                });

      #endregion

      /// <summary>
      /// Bei einem (abs.) Pfad wird versucht, den Pfadanfang durch eine Environment-Variable zu ersetzen.
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string GetPathWithEnvironment(string path) => Path.IsPathRooted(path) ? FSofTUtils.PathHelper.UseEnvironmentVars4Path(path) : path;

      /// <summary>
      /// Bei einem (abs.) Pfad wird versucht, Environment-Variablen zu ersetzen.
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string GetPathWithoutEnvironment(string path) => PathHelper.ReplaceEnvironmentVars(path);

      /// <summary>
      /// alle abs. Pfadangaben werden, wenn möglich, z.T. durch Environment-Vars ersetzt
      /// </summary>
      public void UseEnvironementVarsInPaths() {
         CacheLocation = GetPathWithEnvironment(CacheLocation);
         DemCachePath = GetPathWithEnvironment(DemCachePath);
         DemPath = GetPathWithEnvironment(DemPath);

         List<int[]> providxpaths = ProviderIdxPaths();
         for (int providx = 0; providx < providxpaths.Count; providx++) {
            string xpathmm = XPaths.ProviderNameExt(providxpaths[providx], -1);
            if (ExistXPath(xpathmm)) {      // Multimap
               for (int providx2 = 0; ; providx2++) {
                  if (!ExistXPath(XPaths.ProviderName(providxpaths[providx], providx2)))  // ex. der Provider noch?
                     break;

                  if (changePath2Environment(XPaths.GarminKmzFile(providxpaths[providx], providx2)))
                     continue;   // kann kein anderer Pfad mehr kommen
                  changePath2Environment(XPaths.GarminTyp(providxpaths[providx], providx2));
                  changePath2Environment(XPaths.GarminTdb(providxpaths[providx], providx2));
               }
            } else {                         // normale Map
               if (changePath2Environment(XPaths.GarminKmzFile(providxpaths[providx], -1)))
                  continue;   // kann kein anderer Pfad mehr kommen
               changePath2Environment(XPaths.GarminTyp(providxpaths[providx], -1));
               changePath2Environment(XPaths.GarminTdb(providxpaths[providx], -1));
            }
         }
      }

      bool changePath2Environment(string xpath) {
         string orgvalue = ReadValue(xpath, string.Empty);
         if (orgvalue != string.Empty) {
            string newvalue = GetPathWithEnvironment(orgvalue);
            if (newvalue != orgvalue)
               return Change(xpath, newvalue);
         }
         return false;
      }

   }
}
