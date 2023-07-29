using FSofTUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Xml.XPath;

#if Android
namespace TrackEddi.Common {
   public class Config : SimpleXmlDocument2 {
#else
namespace GpxViewer.Common {
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
      const string XML_CLICKTOLERANCE4TRACKS = "clicktolerance4tracks";

      const string XML_DEMPATH = "dem";
      const string XML_DEMCACHESIZE = "@cachesize";
      const string XML_DEMCACHEPATH = "@cachepath";
      const string XML_DEMMINZOOM = "@minzoom";
      const string XML_DEMHILLSHADINGAZIMUT = "@hillshadingazimut";
      const string XML_DEMHILLSHADINGALTITUDE = "@hillshadingaltitude";
      const string XML_DEMHILLSHADINGSCALE = "@hillshadingscale";

      const string XML_PROVIDERGROUP = "providergroup";
      const string XML_PROVIDERGROUPNAME = "@name";
      const string XML_PROVIDER = "provider";
      const string XML_MAPNAME = "@mapname";
      const string XML_MINZOOM = "@minzoom";
      const string XML_MAXZOOM = "@maxzoom";
      const string XML_ZOOM4DISPLAY = "@zoom4display";

      const string XML_LASTMAPNAMES = "lastmapnames";

      const string XML_HILLSHADING = "@hillshading";
      const string XML_HILLSHADINGALPHA = "@hillshadingalpha";

      const string XML_GARMIN_TDB = "@tdb";
      const string XML_GARMIN_TYP = "@typ";
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
      const string XML_LIVETRACK = "live";
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

      const string XML_SECTION_LIVELOCATION = "livelocation";
      const string XML_LOCATIONSYMBOLSIZE = "locationsymbolsize";
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


      public Config(string configfile, string xsdfile) :
         base(configfile, XML_ROOT, xsdfile) {
         Validating = false;
         LoadData();
      }

      /// <summary>
      /// für float, double und decimal wird ein '.' geliefert
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      string getInternationalString4Object(object value) {
         if (value is float)
            return ((float)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         else if (value is double)
            return ((double)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         else if (value is decimal)
            return ((decimal)value).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
         return value.ToString();
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
                              valuestring :
                              null);
               } else {                         // Attribut
                  Append(tmpxpath,
                         null,
                         null,
                         new System.Collections.Generic.Dictionary<string, string>() {
                            { xpathparts[i].Substring(1), valuestring }
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

      public double ClickTolerance4Tracks {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_CLICKTOLERANCE4TRACKS, 10);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_CLICKTOLERANCE4TRACKS, value);
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

      class IdxPathHelper {
         public int GroupIdx;
         public int ProviderIdx;

         public IdxPathHelper(int groupidx, int provideridx) {
            GroupIdx = groupidx;
            ProviderIdx = provideridx;
         }

         public override string ToString() =>
            "Group " + GroupIdx + ", Provider " + ProviderIdx;

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
         XPathNodeIterator maingroupnodesit = NavigatorSelect("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERGROUP);
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
         for (int i = 0; i < idxlst.Count; i++)
            Debug.WriteLine(string.Join<int>("-", idxlst[i]));
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
         if (it.Current.LocalName == XML_PROVIDERGROUP) {
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
         } else if (it.Current.LocalName == XML_PROVIDER) {
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
         return isprovider;
      }

      string getXPath4ProviderGroup(IList<int> idxlst, int length) {
         StringBuilder sb = new StringBuilder("/" + XML_ROOT + "/" + XML_MAP);
         if (length < 0)
            length = idxlst.Count;
         for (int i = 0; i < length; i++)
            sb.Append("/" + XML_PROVIDERGROUP + "[" + (idxlst[i] + 1) + "]");
         return sb.ToString();
      }

      string getXPath4Provider(IList<int> idxlst) {
         StringBuilder sb = new StringBuilder("/" + XML_ROOT + "/" + XML_MAP);
         for (int i = 0; i < idxlst.Count; i++)
            sb.Append("/" + (i < idxlst.Count - 1 ? XML_PROVIDERGROUP : XML_PROVIDER) + "[" + (idxlst[i] + 1) + "]");
         return sb.ToString();
      }

      /// <summary>
      /// ermittelt den Namen der Providergruppe
      /// </summary>
      /// <param name="providxlst"></param>
      /// <param name="length">wenn kleiner 0, wird die gesamte IDX-Liste verwendet, sonst nur ein Teil</param>
      /// <returns></returns>
      public string ProviderGroupName(IList<int> providxlst, int length = -1) {
         return ReadValue(getXPath4ProviderGroup(providxlst, length) + "/" + XML_PROVIDERGROUPNAME, "");
      }

      public string ProviderName(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst), "");
      }

      public string MapName(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_MAPNAME, "");
      }

      public int MinZoom(IList<int> providxlst) {
         return Math.Max(0, ReadValue(getXPath4Provider(providxlst) + "/" + XML_MINZOOM, 0));
      }

      public int MaxZoom(IList<int> providxlst) {
         return Math.Min(ReadValue(getXPath4Provider(providxlst) + "/" + XML_MAXZOOM, 24), 24);
      }

      public double GetZoom4Display(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_ZOOM4DISPLAY, 1.0);
      }

      public bool Hillshading(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_HILLSHADING, false);
      }

      public byte HillshadingAlpha(IList<int> providxlst) {
         return (byte)(ReadValue(getXPath4Provider(providxlst) + "/" + XML_HILLSHADINGALPHA, 100) & 0xFF);
      }

      #endregion

      #region spez. Providereigenschaften für Garmin

      public string GarminTdb(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMIN_TDB, "");
      }

      public string GarminTyp(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMIN_TYP, "");
      }

      public double GarminTextFactor(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMIN_TEXTFACTOR, 1.0);
      }

      public double GarminSymbolFactor(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMIN_SYMBOLFACTOR, 1.0);
      }

      public double GarminLineFactor(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMIN_LINEFACTOR, 1.0);
      }

      //public string GarminTdb(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, "");
      //}

      //public void SetGarminTdb(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, value);
      //}

      //public string GarminTyp(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, "");
      //}

      //public void SetGarminTyp(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, value);
      //}

      //public int[] GarminLocalCacheLevels(int providx) {
      //   string text = ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, "");
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
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LEVELS4CACHE, string.Join(",", value));
      //}

      //public int GarminMaxSubdiv(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, 1000000);
      //}

      //public void SetGarminMaxSubdiv(int providx, int value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, value);
      //}

      //public double GarminTextFactor(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, 1.0);
      //}

      //public void SetGarminTextFactor(int providx, double value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, value);
      //}

      //public double GarminSymbolFactor(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, 1.0);
      //}

      //public void SetGarminSymbolFactor(int providx, double value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, value);
      //}

      //public double GarminLineFactor(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, 1.0);
      //}

      //public void SetGarminLineFactor(int providx, double value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, value);
      //}

      #endregion

      #region spez. Providereigenschaften für Garmin-KMZ

      public string GarminKmzFile(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_GARMINKMZ_KMZFILE, "");
      }

      //public string GarminKmzFile(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, "");
      //}

      //public void SetGarminKmzFile(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, value);
      //}

      #endregion

      #region spez. Providereigenschaften für WMS

      public string WmsUrl(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_URL, "");
      }

      public string WmsVersion(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_VERSION, "");
      }

      public string WmsSrs(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_SRS, "");
      }

      public string WmsPictFormat(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_PICTFORMAT, "");
      }

      public string WmsLayers(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_LAYERS, "");
      }

      public string WmsExtend(IList<int> providxlst) {
         return ReadValue(getXPath4Provider(providxlst) + "/" + XML_WMS_EXT, "");
      }

      //public string WmsUrl(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, "");
      //}

      //public void SetWmsUrl(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, value);
      //}

      //public string WmsVersion(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, "");
      //}

      //public void SetWmsVersion(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, value);
      //}

      //public string WmsSrs(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, "");
      //}

      //public void SetWmsSrs(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, value);
      //}

      //public string WmsPictFormat(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, "");
      //}

      //public void SetWmsPictFormat(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, value);
      //}

      //public string WmsLayers(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, "");
      //}

      //public void SetWmsLayers(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, value);
      //}

      //public string WmsExtend(int providx) {
      //   return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, "");
      //}

      //public void SetWmsExtend(int providx, string value) {
      //   setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, value);
      //}

      #endregion

      #region Map-Menü der zuletzt genutzten Karten

      public int LastUsedMapsCount {
         get => ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_LASTMAPNAMES, 3);
         set => setXPath("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_LASTMAPNAMES, value);
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

      public Color LiveTrackColor {
         get => getPenColor(XML_LIVETRACK);
         set => setPenColor(XML_LIVETRACK, value);
      }

      public float LiveTrackWidth {
         get => getPenWidth(XML_LIVETRACK);
         set => setPenWidth(XML_LIVETRACK, value);
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

         if (a != null && r != null && g != null && b != null && p != null)
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

      #region Geo-Location

      public int LocationSymbolsize {
         get => ReadValue("/" + XML_ROOT + "/" + XML_SECTION_LIVELOCATION + "/" + XML_LOCATIONSYMBOLSIZE, 50);
         set => setXPath("/" + XML_ROOT + "/" + XML_SECTION_LIVELOCATION + "/" + XML_LOCATIONSYMBOLSIZE, value);
      }

      public double TrackingMinimalPointdistance {
         get => ReadValue("/" + XML_ROOT + "/" + XML_TRACKING + "/" + XML_MINIMALPOINTDISTANCE, 2.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_TRACKING + "/" + XML_MINIMALPOINTDISTANCE, value);
      }

      public double TrackingMinimalHeightdistance {
         get => ReadValue("/" + XML_ROOT + "/" + XML_TRACKING + "/" + XML_MINIMALHEIGHTDISTANCE, 5.0);
         set => setXPath("/" + XML_ROOT + "/" + XML_TRACKING + "/" + XML_MINIMALHEIGHTDISTANCE, value);
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

      #region Map-Section

      /// <summary>
      /// Inhalt der "Hauptkartengruppe" löschen
      /// </summary>
      /// <returns></returns>
      public bool RemoveMapsSectionContent() =>
         Remove("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDERGROUP + "/*");

      /// <summary>
      /// Standardkarte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="zoom4display"></param>
      /// <param name="idxpathprovidergroup"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            double zoom4display,
                            IList<int> idxpathprovidergroup) =>
         Append(getXPath4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                XML_PROVIDER,
                providername,
                new Dictionary<string, string> {
                  { XML_MAPNAME.Substring(1), name},
                  { XML_MINZOOM.Substring(1), getInternationalString4Object(minzoom) },
                  { XML_MAXZOOM.Substring(1), getInternationalString4Object(maxzoom) },
                  { XML_ZOOM4DISPLAY.Substring(1), getInternationalString4Object(zoom4display) },
                });

      /// <summary>
      /// Garmin-KMZ-Karte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="zoom4display"></param>
      /// <param name="kmzfile"></param>
      /// <param name="idxpathprovidergroup"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            double zoom4display,
                            string kmzfile,
                            bool hillshading,
                            int hillshadingalpha,
                            IList<int> idxpathprovidergroup) =>
         Append(getXPath4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                XML_PROVIDER,
                providername,
                new Dictionary<string, string> {
                  { XML_MAPNAME.Substring(1), name},
                  { XML_MINZOOM.Substring(1), getInternationalString4Object(minzoom) },
                  { XML_MAXZOOM.Substring(1), getInternationalString4Object(maxzoom) },
                  { XML_ZOOM4DISPLAY.Substring(1), getInternationalString4Object(zoom4display) },
                  { XML_GARMINKMZ_KMZFILE.Substring(1), kmzfile },
                  { XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading) },
                  { XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha) },
                });

      /// <summary>
      /// Garminkarte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="zoom4display"></param>
      /// <param name="tdbfile"></param>
      /// <param name="typfile"></param>
      /// <param name="levels4cache"></param>
      /// <param name="maxsubdiv"></param>
      /// <param name="textfactor"></param>
      /// <param name="symbolfactor"></param>
      /// <param name="linefactor"></param>
      /// <param name="hillshading"></param>
      /// <param name="hillshadingalpha"></param>
      /// <param name="idxpathprovidergroup"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            double zoom4display,
                            string tdbfile,
                            string typfile,
                            double textfactor,
                            double symbolfactor,
                            double linefactor,
                            bool hillshading,
                            int hillshadingalpha,
                            IList<int> idxpathprovidergroup) =>
         Append(getXPath4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                XML_PROVIDER,
                providername,
                new Dictionary<string, string> {
                  { XML_MAPNAME.Substring(1), name},
                  { XML_MINZOOM.Substring(1), getInternationalString4Object(minzoom) },
                  { XML_MAXZOOM.Substring(1), getInternationalString4Object(maxzoom) },
                  { XML_ZOOM4DISPLAY.Substring(1), getInternationalString4Object(zoom4display) },
                  { XML_GARMIN_TDB.Substring(1), tdbfile },
                  { XML_GARMIN_TYP.Substring(1), typfile },
                  { XML_GARMIN_TEXTFACTOR.Substring(1), getInternationalString4Object(textfactor) },
                  { XML_GARMIN_SYMBOLFACTOR.Substring(1), getInternationalString4Object(symbolfactor) },
                  { XML_GARMIN_LINEFACTOR.Substring(1), getInternationalString4Object(linefactor) },
                  { XML_HILLSHADING.Substring(1), getInternationalString4Object(hillshading) },
                  { XML_HILLSHADINGALPHA.Substring(1), getInternationalString4Object(hillshadingalpha) },
                });

      /// <summary>
      /// WMS-Karte anhängen
      /// </summary>
      /// <param name="name"></param>
      /// <param name="providername"></param>
      /// <param name="minzoom"></param>
      /// <param name="maxzoom"></param>
      /// <param name="zoom4display"></param>
      /// <param name="url"></param>
      /// <param name="version"></param>
      /// <param name="srs"></param>
      /// <param name="format"></param>
      /// <param name="layers"></param>
      /// <param name="extended"></param>
      /// <param name="idxpathprovidergroup"></param>
      /// <returns></returns>
      public bool AppendMap(string name,
                            string providername,
                            int minzoom,
                            int maxzoom,
                            double zoom4display,
                            string url,
                            string version,
                            string srs,
                            string format,
                            string layers,
                            string extended,
                            IList<int> idxpathprovidergroup) =>
         Append(getXPath4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                XML_PROVIDER,
                providername,
                new Dictionary<string, string> {
                  { XML_MAPNAME.Substring(1), name},
                  { XML_MINZOOM.Substring(1), getInternationalString4Object(minzoom) },
                  { XML_MAXZOOM.Substring(1), getInternationalString4Object(maxzoom) },
                  { XML_ZOOM4DISPLAY.Substring(1), getInternationalString4Object(zoom4display) },
                  { XML_WMS_URL.Substring(1), url },
                  { XML_WMS_VERSION.Substring(1), version },
                  { XML_WMS_SRS.Substring(1), srs },
                  { XML_WMS_PICTFORMAT.Substring(1), format },
                  { XML_WMS_LAYERS.Substring(1), layers },
                  { XML_WMS_EXT.Substring(1), extended },
                });

      public bool AppendMapGroup(string goupname, IList<int> idxpathprovidergroup) {
         return Append(getXPath4ProviderGroup(idxpathprovidergroup, idxpathprovidergroup.Count),
                       XML_PROVIDERGROUP,
                       null,
                       new Dictionary<string, string> {
                         { XML_PROVIDERGROUPNAME.Substring(1), goupname},
                       });
      }

      #endregion

   }
}
