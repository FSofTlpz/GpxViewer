using System;
using System.Collections.Generic;
using System.Drawing;
using FSofTUtils;

namespace GpxViewer {
   class Config : SimpleXmlDocument2 {

      const string XML_ROOT = "gpxview";

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

      const string XML_DEMPATH = "dem";
      const string XML_DEMCACHESIZE = "@cachesize";
      const string XML_DEMHILLSHADINGAZIMUT = "@hillshadingazimut";
      const string XML_DEMHILLSHADINGALTITUDE = "@hillshadingaltitude";
      const string XML_DEMHILLSHADINGSCALE = "@hillshadingscale";
      const string XML_DEMHILLSHADINGZ = "@hillshadingz";

      const string XML_PROVIDER = "provider";
      const string XML_MAPNAME = "@mapname";
      const string XML_DBIDDELTA = "@dbiddelta";
      const string XML_MINZOOM = "@minzoom";
      const string XML_MAXZOOM = "@maxzoom";
      const string XML_ZOOM4DISPLAY = "@zoom4display";

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
      const string XML_TRACKCOLORA = "@a";
      const string XML_TRACKCOLORR = "@r";
      const string XML_TRACKCOLORG = "@g";
      const string XML_TRACKCOLORB = "@b";
      const string XML_TRACKWIDTH = "@width";


      const string XML_GARMINSYMBOLS = "garminsymbols";
      const string XML_GARMINSYMBOLGROUP = "group";
      const string XML_GARMINSYMBOLGROUPNAME = "@name";
      const string XML_GARMINSYMBOL = "symbol";
      const string XML_GARMINSYMBOLNAME = "@name";
      const string XML_GARMINSYMBOLTEXT = "@text";


      public Config(string configfile, string xsdfile) :
         base(configfile, XML_ROOT, xsdfile) {
         Validating = false;
         LoadData();
      }

      public int MinimalTrackpointDistanceX {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_X, 14);
         }
      }

      public int MinimalTrackpointDistanceY {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MINIMALTRACKPOINTDISTANCE + "/" + XML_MINIMALTRACKPOINTDISTANCE_Y, 14);
         }
      }

      public string CacheLocation {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_CACHELOCATION, null);
         }
      }

      public bool ServerOnly {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_SERVERONLY, true);
         }
      }

      public int StartProvider {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTPROVIDER, 0);
         }
      }

      public int StartZoom {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTZOOM, 16);
         }
      }

      public double StartLatitude {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLATITUDE, 51.30);
         }
      }

      public double StartLongitude {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_STARTLONGITUDE, 12.40);
         }
      }

      public string[] Provider {
         get {
            return ReadString("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER);
         }
      }

      #region XML_MAP / XML_DEMPATH

      public string DemPath {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH, "");
         }
      }

      public int DemCachesize {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMCACHESIZE, 16);
         }
      }

      public double DemHillshadingAzimut {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGAZIMUT, 315.0);
         }
      }

      public double DemHillshadingAltitude {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGALTITUDE, 45.0);
         }
      }

      public double DemHillshadingScale {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGSCALE, 1.0);
         }
      }

      public double DemHillshadingZ {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_DEMPATH + "/" + XML_DEMHILLSHADINGZ, 1.0);
         }
      }

      #endregion

      #region allgemeine Providereigenschaften

      public string MapName(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAPNAME, "");
      }

      public int DbIdDelta(int providx) {
         return Math.Max(0, ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_DBIDDELTA, 0));
      }

      public int MinZoom(int providx) {
         return Math.Max(0, ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MINZOOM, 0));
      }

      public int MaxZoom(int providx) {
         return Math.Min(ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_MAXZOOM, 24), 24);
      }

      public double Zoom4Display(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_ZOOM4DISPLAY, 1.0);
      }

      public bool Hillshading(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADING, false);
      }

      public int HillshadingAlpha(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_HILLSHADINGALPHA, 100) & 0xFF;
      }

      #endregion

      #region spez. Providereigenschaften für Garmin

      public string GarminTdb(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TDB, "");
      }

      public string GarminTyp(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TYP, "");
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

      public int GarminMaxSubdiv(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_MAXSUBDIV, 1000000);
      }

      public double GarminTextFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_TEXTFACTOR, 1.0);
      }

      public double GarminSymbolFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_SYMBOLFACTOR, 1.0);
      }

      public double GarminLineFactor(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMIN_LINEFACTOR, 1.0);
      }

      #endregion

      #region spez. Providereigenschaften für Garmin-KMZ

      public string GarminKmzFile(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_GARMINKMZ_KMZFILE, "");
      }

      #endregion

      #region spez. Providereigenschaften für WMS

      public string WmsUrl(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_URL, "");
      }

      public string WmsVersion(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_VERSION, "");
      }

      public string WmsSrs(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_SRS, "");
      }

      public string WmsPictFormat(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_PICTFORMAT, "");
      }

      public string WmsLayers(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_LAYERS, "");
      }

      public string WmsExtend(int providx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_MAP + "/" + XML_PROVIDER + "[" + (providx + 1).ToString() + "]/" + XML_WMS_EXT, "");
      }

      #endregion

      #region Proxy-Definition

      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stadtproxy.stadt.leipzig.de"
      /// </summary>
      public string WebProxyName {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYNAME, null);
         }
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: 80
      /// </summary>
      public int WebProxyPort {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPORT, 0);
         }
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig: z.B.: "stinnerfr@leipzig.de"
      /// </summary>
      public string WebProxyUser {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYUSER, null);
         }
      }
      /// <summary>
      /// ev. für den Internetzugriff nötig
      /// </summary>
      public string WebProxyPassword {
         get {
            return ReadValue("/" + XML_ROOT + "/" + XML_SECTION_PROXY + "/" + XML_PROXYPASSWORD, null);
         }
      }

      #endregion

      Color getPenColor(string tracktype) {
         int a = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORA, 255);
         int r = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORR, 0);
         int g = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORG, 0);
         int b = ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKCOLORB, 0);
         return Color.FromArgb(a, r, g, b);
      }

      float getPenWidth(string tracktype) {
         return (float)ReadValue("/" + XML_ROOT + "/" + XML_SECTION_TRACKS + "/" + tracktype + "/" + XML_TRACKWIDTH, 1.0);
      }

      public Color StandardTrackColor {
         get {
            return getPenColor(XML_STANDARDTRACK);
         }
      }

      public Color StandardTrackColor2 {
         get {
            return getPenColor(XML_STANDARDTRACK2);
         }
      }

      public Color StandardTrackColor3 {
         get {
            return getPenColor(XML_STANDARDTRACK3);
         }
      }

      public Color StandardTrackColor4 {
         get {
            return getPenColor(XML_STANDARDTRACK4);
         }
      }

      public Color StandardTrackColor5 {
         get {
            return getPenColor(XML_STANDARDTRACK5);
         }
      }

      public float StandardTrackWidth {
         get {
            return getPenWidth(XML_STANDARDTRACK);
         }
      }

      public Color MarkedTrackColor {
         get {
            return getPenColor(XML_MARKEDTRACK);
         }
      }

      public float MarkedTrackWidth {
         get {
            return getPenWidth(XML_MARKEDTRACK);
         }
      }

      public Color EditableTrackColor {
         get {
            return getPenColor(XML_EDITABLETRACK);
         }
      }

      public float EditableTrackWidth {
         get {
            return getPenWidth(XML_EDITABLETRACK);
         }
      }

      public Color InEditTrackColor {
         get {
            return getPenColor(XML_INEDITTRACK);
         }
      }

      public float InEditTrackWidth {
         get {
            return getPenWidth(XML_INEDITTRACK);
         }
      }

      public Color SelectedPartTrackColor {
         get {
            return getPenColor(XML_SELPARTTRACK);
         }
      }

      public float SelectedPartTrackWidth {
         get {
            return getPenWidth(XML_SELPARTTRACK);
         }
      }


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
      /// liefert die Grafikdatei zu einem Symbol einer Gruppe von Garminsymbolen
      /// </summary>
      /// <param name="groupidx"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GetGarminMarkerSymbolfile(int groupidx, int idx) {
         return ReadValue("/" + XML_ROOT + "/" + XML_GARMINSYMBOLS + "/" + XML_GARMINSYMBOLGROUP + "[" + (groupidx + 1).ToString() + "]/" + XML_GARMINSYMBOL + "[" + (idx + 1).ToString() + "]", "");
      }


   }
}
