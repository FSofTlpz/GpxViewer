using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FSofTUtils.Geography {

   /// <summary>
   /// erweitert <see cref="GpxAll"/> i.W. um das speichern und einlesen von Dateien sowie die Verwendung einiger Garmin-Erweiterungen
   /// </summary>
   public class GpxFileGarmin : GpxAll {

      public const string STDGPXVERSION = "1.1";

      #region Events

      public class ExtLoadEventArgs {
         public enum Reason {
            ReadXml,
            ReadGDB,
            ReadKml,
            InsertWaypoints,
            InsertTracks,
            InsertWaypoint,
            InsertTrack,
         }

         public Reason LoadReason;


         public ExtLoadEventArgs(Reason reason) => LoadReason = reason;
      }

      /// <summary>
      /// zusätzliche Infos über den Zustand des Einlesens
      /// </summary>
      public event EventHandler<ExtLoadEventArgs>? ExtLoadEvent;

      #endregion

      public enum ObjectType {
         Waypoint,
         Route,
         Track
      }


      public GpxFileGarmin(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) {
         if (!string.IsNullOrEmpty(xmltext))
            postImportXml(out _);
      }

      public GpxFileGarmin(out List<Color> trackcolor, string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) {
         trackcolor = new List<Color>();
         if (!string.IsNullOrEmpty(xmltext))
            postImportXml(out trackcolor);
      }

      /// <summary>
      /// Ex. der Objektname schon ?
      /// </summary>
      /// <param name="name"></param>
      /// <param name="type"></param>
      /// <returns></returns>
      public bool ObjectNameExists(string name, ObjectType type) {
         int count = 0;
         switch (type) {
            case ObjectType.Waypoint:
               count = Waypoints.Count;
               break;

            case ObjectType.Route:
               count = Routes.Count;
               break;

            case ObjectType.Track:
               count = Tracks.Count;
               break;
         }

         for (int i = 0; i < count; i++)
            switch (type) {
               case ObjectType.Waypoint:
                  if (Waypoints[i].Name == name)
                     return true;
                  break;

               case ObjectType.Route:
                  if (Routes[i].Name == name)
                     return true;
                  break;

               case ObjectType.Track:
                  if (Tracks[i].Name == name)
                     return true;
                  break;
            }
         return false;
      }

      /// <summary>
      /// falls der Name für den <see cref="ObjectType"/> nicht eindeutig ist, wird so lange
      /// " *" angehängt bis der Name eindeutig ist oder eine Zahl angehängt
      /// </summary>
      /// <param name="name">Ausgangsname</param>
      /// <param name="type">Objekttyp</param>
      /// <param name="withcounter">entweder eine Folge von " *" oder eine Zahl anhängen</param>
      /// <returns></returns>
      public string GetUniqueObjectName(string? name, ObjectType type, bool withcounter) {
         if (name == null)
            name = string.Empty;
         if (withcounter) {
            int no = 1;
            string testname = name;
            while (ObjectNameExists(testname, type)) {
               testname = name + " " + no.ToString("d2");
               no++;
            }
         } else
            while (ObjectNameExists(name, type))
               name += " *";
         return name;
      }

      const string ATTR_XMLNS = "xmlns";
      const string ATTR_XMLNSXSI = "xmlns:xsi";
      const string ATTR_XSISCHEMALOCATION = "xsi:schemaLocation";

      const string XMLNS = "http://www.topografix.com/GPX/1/1";
      const string XMLNSXSI = "http://www.w3.org/2001/XMLSchema-instance";
      readonly string[] XSISCHEMALOCATION = [ "http://www.topografix.com/GPX/1/1",
                                              "http://www.topografix.com/GPX/1/1/gpx.xsd" ];

      const string XMLS_EXTENSIONS = "<extensions>";




      /// <summary>
      /// Daten als Datei abspeichern
      /// </summary>
      /// <param name="filename">die Extension muss GPX, KML oder KMZ sein</param>
      /// <param name="creator"></param>
      /// <param name="withgarminext">mit oder ohne Garmin-Erweiterungen</param>
      /// <param name="xmlformatindent">Einzug bei formatierter XML-Ausgabe, z.B. " "</param>
      /// <param name="trackcolor">Liste der Trackfarben (nicht vorhandene Farben bzw. <see cref="Color.Empty"/> 
      /// (schwarz volltransparent) und <see cref="Color.Transparent"/> (weiss volltransparent) werden NICHT gespeichert)</param>
      /// <param name="kmltrackwidth">Liste der Trackbreiten (nur KML/KMZ)</param>
      /// <param name="gpxversion"></param>
      /// <param name="withxmlcolor">wenn true dann Hex-Codierung '#AARRGGBB' speichern (NICHT-Garmin-Form!)</param>
      public void Save(string filename,
                       string creator,
                       bool withgarminext,
                       string? xmlformatindent = null,
                       IList<Color>? trackcolor = null,
                       IList<uint>? kmltrackwidth = null,
                       string gpxversion = STDGPXVERSION,
                       bool withxmlcolor = false) {
         RebuildMetadataBounds();
         Metadata.Time = DateTime.Now;

         if (withgarminext) {
            // Trackfarben
            if (trackcolor != null)
               for (int i = 0; i < trackcolor.Count && i < Tracks.Count; i++) {
                  Color? xmlcolor = withxmlcolor ? trackcolor[i] : null;
                  GarminTrackColors.Colorname garmincol = trackcolor[i].ToArgb() != Color.Transparent.ToArgb() &&
                                                          trackcolor[i].ToArgb() != Color.Empty.ToArgb() ?
                                                               GarminTrackColors.GetColorname(trackcolor[i], true) :
                                                               GarminTrackColors.Colorname.Unknown;

                  int extensionsidx = -1;
                  string? xmlextensions = null;
                  if (Tracks[i].UnhandledChildXml != null)
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                     for (int j = 0; j < Tracks[i].UnhandledChildXml.Count; j++) {           // suche Child "<extensions>"
                        if (Tracks[i].UnhandledChildXml[j].StartsWith(XMLS_EXTENSIONS)) {
                           extensionsidx = j;
                           xmlextensions = Tracks[i].UnhandledChildXml[extensionsidx];
                           break;
                        }
                     }
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  // enweder extensionsidx<0 und xmlextensions=null oder beides gültig

                  if (xmlcolor != null ||
                      garmincol != GarminTrackColors.Colorname.Unknown &&
                       garmincol != GarminTrackColors.Colorname.Transparent) {                                     // Farbe setzen

                     xmlextensions = GarminSpecial.SetGarminColor(xmlextensions, garmincol, xmlcolor);
                     if (xmlextensions != null)
                        if (extensionsidx < 0) {    // ex. bisher noch nicht
                           if (Tracks[i].UnhandledChildXml == null)
                              Tracks[i].CreateUnhandledChildXmlList();
                           Tracks[i].UnhandledChildXml!.Add(xmlextensions);
                        } else            // ersetzen
                           Tracks[i].UnhandledChildXml![extensionsidx] = xmlextensions;

                  } else {                                                          // ev. vorhandene Farbe aus den UnhandledChildXml entfernen

                     xmlextensions = GarminSpecial.SetGarminColor(xmlextensions, GarminTrackColors.Colorname.Unknown, null);
                     if (xmlextensions != null)
                        if (0 <= extensionsidx) {
                           if (string.IsNullOrEmpty(xmlextensions))
                              Tracks[i].UnhandledChildXml!.RemoveAt(extensionsidx);         // gesamte Extension entfernt ? ...
                           else
                              Tracks[i].UnhandledChildXml![extensionsidx] = xmlextensions;
                        }

                  }
               }

            // Markertypen
            for (int i = 0; i < Waypoints.Count; i++) {
               if (!string.IsNullOrEmpty(Waypoints[i].Symbol)) {
                  int extensionsidx = -1;
                  string? xmlextensions = null;
                  if (Waypoints[i].UnhandledChildXml != null) {
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                     for (int j = 0; j < Waypoints[i].UnhandledChildXml.Count; j++) {
                        if (Waypoints[i].UnhandledChildXml[j].StartsWith(XMLS_EXTENSIONS)) {
                           extensionsidx = j;
                           xmlextensions = Waypoints[i].UnhandledChildXml[extensionsidx];
                           break;
                        }
                     }
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  }
                  // enweder extensionsidx<0 und xmlextensions=null oder beides gültig

                  xmlextensions = GarminSpecial.SetGarminSymbolExt(xmlextensions);
                  if (extensionsidx < 0) {
                     if (Waypoints[i].UnhandledChildXml == null)
                        Waypoints[i].CreateUnhandledChildXmlList();
                     Waypoints[i].UnhandledChildXml!.Add(xmlextensions);
                  } else
                     Waypoints[i].UnhandledChildXml![extensionsidx] = xmlextensions;
               }
            }
         }

         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx") {

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

            Creator = creator;
            Version = gpxversion;

            bool found_xmlns = false;
            bool found_xmlnsxsi = false;
            bool found_xsi = false;
            if (gpxattributes != null) {
               for (int i = 0; i < gpxattributes.Count; i++) {
                  if (gpxattributes[i].Item1 == ATTR_XMLNS) {
                     found_xmlns = true;
                     if (gpxattributes[i].Item2 != XMLNS)
                        gpxattributes[i] = new(ATTR_XMLNS, XMLNS);
                  }
                  if (gpxattributes[i].Item1 == ATTR_XMLNSXSI) {
                     found_xmlnsxsi = true;
                     if (gpxattributes[i].Item2 != XMLNSXSI)
                        gpxattributes[i] = new(ATTR_XMLNSXSI, XMLNSXSI);
                  }
                  if (gpxattributes[i].Item1 == ATTR_XSISCHEMALOCATION) {
                     found_xsi = true;
                     bool[] found = new bool[XSISCHEMALOCATION.Length];
                     // Test ob XSISCHEMALOCATION enthalten ist:
                     string[] content = gpxattributes[i].Item2.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                     for (int j = 0; j < content.Length; j++) {
                        for (int k = 0; k < XSISCHEMALOCATION.Length; k++) {
                           if (content[j] == XSISCHEMALOCATION[k])
                              found[k] = true;
                        }
                     }

                     string tmp = string.Empty;
                     for (int k = 0; k < XSISCHEMALOCATION.Length; k++)
                        if (!found[k])
                           tmp += XSISCHEMALOCATION[k] + " ";
                     gpxattributes[i] = new(ATTR_XSISCHEMALOCATION, (tmp + " " + gpxattributes[i].Item2).Trim());
                  }
               }
            }
            if (gpxattributes == null && (!found_xmlns || !found_xmlnsxsi || !found_xsi))
               gpxattributes = new List<(string, string)>();

            if (!found_xmlns)
               gpxattributes?.Insert(0, (ATTR_XMLNS, XMLNS));
            if (!found_xmlnsxsi)
               gpxattributes?.Add((ATTR_XMLNSXSI, XMLNSXSI));
            if (!found_xsi)
               gpxattributes?.Add((ATTR_XSISCHEMALOCATION, string.Join(" ", XSISCHEMALOCATION)));

            StringBuilder sb2 = new StringBuilder();
            AsXml(sb2);    // Der StringBuilder enthält einen vollständigen XML-Text. Ev. enthält das GPX-Tag auch diverse Attribute. 
            sb.Append(sb2);

            if (!string.IsNullOrEmpty(xmlformatindent))
               sb = formatXml(sb, xmlformatindent);

            File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);

         } else if (ext == ".kml" || ext == ".kmz") {

            List<Color> kmlcol = new List<Color>();
            List<uint> kmlwidth = new List<uint>();
            if (trackcolor != null)
               for (int i = 0; i < trackcolor.Count && i < Tracks.Count; i++)
                  kmlcol.Add(trackcolor[i]);
            if (kmltrackwidth != null)
               for (int i = 0; i < kmltrackwidth.Count && i < Tracks.Count; i++)
                  kmlwidth.Add(kmltrackwidth[i]);

            new KmlWriter().Write_gdal(filename, this, true, kmlcol, kmlwidth);
         }
      }

      /// <summary>
      /// (das geht wahrscheinlich auch einfacher direkt als Text)
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="indenttxt"></param>
      /// <returns></returns>
      StringBuilder formatXml(StringBuilder sb, string indenttxt = " ") {
         using (Stream stream = new MemoryStream()) {
            using StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);  // nötig, damit die XML-Deklaration UTF-8 enthält
            using XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() {
               Indent = true,
               IndentChars = indenttxt,
               Encoding = Encoding.UTF8,
            });

            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.LoadXml(sb.ToString());
            xml.Save(xmlWriter);

            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new StreamReader(stream);
            return new StringBuilder(reader.ReadToEnd());
         }
      }


      /// <summary>
      /// Daten asynchron als Datei abspeichern
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="creator"></param>
      /// <param name="withgarminext"></param>
      /// <param name="xmlformatindent">Einzug bei formatierter XML-Ausgabe, z.B. " "</param>
      /// <param name="trackcolor">Liste der Trackfarben</param>
      /// <param name="kmltrackwidth">Liste der Trackbreiten (nur KML/KMZ)</param>
      /// <param name="colorname4unused">diese Farbe wird NICHT gespeichert</param>
      /// <param name="gpxversion"></param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      public async Task SaveAsync(string filename,
                                  string creator,
                                  bool withgarminext,
                                  string? xmlformatindent = null,
                                  IList<Color>? trackcolor = null,
                                  IList<uint>? kmltrackwidth = null,
                                  string gpxversion = STDGPXVERSION,
                                  bool withxmlcolor = false) =>
         await Task.Run(() => Save(filename,
                                   creator,
                                   withgarminext,
                                   xmlformatindent,
                                   trackcolor,
                                   kmltrackwidth,
                                   gpxversion,
                                   withxmlcolor));

      private void base_LoadInfoEvent(object? sender, LoadEventArgs? e) {
         if (e != null)
            switch (e.Reason) {
               case LoadEventArgs.LoadReason.InsertWaypoints:
                  sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoints);
                  break;

               case LoadEventArgs.LoadReason.InsertWaypoint:
                  sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoint);
                  break;

               case LoadEventArgs.LoadReason.InsertTracks:
                  sendLoadInfo(ExtLoadEventArgs.Reason.InsertTracks);
                  break;

               case LoadEventArgs.LoadReason.InsertTrack:
                  sendLoadInfo(ExtLoadEventArgs.Reason.InsertTrack);
                  break;
            }
      }

      void sendLoadInfo(ExtLoadEventArgs.Reason reason) => ExtLoadEvent?.Invoke(this, new ExtLoadEventArgs(reason));

      /// <summary>
      /// Daten aus der Datei lesen
      /// <para>Falls keine Trackfarbe angegeben ist, wird <see cref="Color.Empty"/> (schwarz, volltransparent) verwendet.</para>
      /// </summary>
      /// <param name="filename">Die Extension darf GPX, GDB, KML oder KMZ sein</param>
      /// <param name="removenamespace"></param>
      /// <returns>Liste der Trackfarben (Color.Empty für Tracks ohne Farbe)</returns>
      public List<Color> Load(string filename,
                              bool removenamespace = false) => Load(filename, removenamespace, Color.Empty);

      /// <summary>
      /// Daten aus der Datei lesen
      /// </summary>
      /// <param name="filename">Die Extension darf GPX, GDB, KML oder KMZ sein</param>
      /// <param name="removenamespace"></param>
      /// <param name="dummycolor">Trackfarbe, falls keine in der Datei angegeben ist</param>
      /// <returns>Liste der Trackfarben (Color.Empty für Tracks ohne Farbe)</returns>
      public List<Color> Load(string filename,
                              bool removenamespace,
                              Color dummycolor) {
         List<Color> trackcolor = new List<Color>();
         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx") {

            LoadInfoEvent += base_LoadInfoEvent;
            sendLoadInfo(ExtLoadEventArgs.Reason.ReadXml);
            FromXml(File.ReadAllText(filename), removenamespace);
            postImportXml(out trackcolor);
            LoadInfoEvent -= base_LoadInfoEvent;

         } else if (ext == ".gdb") {

            sendLoadInfo(ExtLoadEventArgs.Reason.ReadGDB);
            List<GDB.Object> objlst = GDB.ReadGDBObjectList(filename);
            long dtunix = new DateTime(1970, 1, 1).Ticks;
            foreach (GDB.Object obj in objlst) {
               switch (obj.ObjectHeader.ObjectType) {
                  case GDB.ObjectHeader.GDBObjectType.WAYPOINT:
                     GDB.Waypoint wp = (GDB.Waypoint)obj;
                     string? symbol = GDB.GetIconName4Symbolnumber(wp.IconIdx);
                     GpxWaypoint waypoint = new GpxWaypoint(wp.Lon, wp.Lat) {
                        Elevation = wp.Ele == double.MinValue ? double.MinValue : wp.Ele,
                        Name = wp.Name,
                        Description = wp.Description,
                        Time = wp.CreationTime,
                        Symbol = symbol != null ? symbol : string.Empty,
                     };
                     if (wp.IconIdx > 0) {
                        string? name = GDB.GetIconName4Symbolnumber(wp.IconIdx);
                        if (!string.IsNullOrEmpty(name))
                           waypoint.Symbol = name;
                     }
                     InsertWaypoint(waypoint);
                     sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoint);
                     break;

                  case GDB.ObjectHeader.GDBObjectType.TRACK:
                     GDB.Track track = (GDB.Track)obj;
                     GpxTrackSegment gpxsegment = new GpxTrackSegment();
                     foreach (GDB.TrackPoint pt in track.Points) {
                        if (pt.DateTime.Ticks > dtunix)
                           gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele,
                                                  pt.DateTime));
                        else
                           gpxsegment.InsertPoint(new GpxTrackPoint(pt.Lon,
                                                  pt.Lat,
                                                  pt.Ele == double.MinValue ? double.MinValue : pt.Ele));
                     }
                     GpxTrack gpxtrack = new GpxTrack() {
                        Name = track.Name,
                     };
                     gpxtrack.InsertSegment(gpxsegment);
                     InsertTrack(gpxtrack);
                     sendLoadInfo(ExtLoadEventArgs.Reason.InsertTrack);
                     break;

                     //case GDB.ObjectHeader.GDBObjectType.ROUTE:

                     //   break;
               }
            }
            postLoad();

         } else if (ext == ".kml" || ext == ".kmz") {

            sendLoadInfo(ExtLoadEventArgs.Reason.ReadKml);
            GpxAll gpx4kml = new KmlReader().Read(filename, out trackcolor);

            for (int i = 0; i < trackcolor.Count; i++)
               if (trackcolor[i].ToArgb() == Color.Empty.ToArgb())
                  trackcolor[i] = dummycolor;

            sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoints);
            for (int i = 0; i < gpx4kml.Waypoints.Count; i++) {
               InsertWaypoint(gpx4kml.Waypoints[i]);
               sendLoadInfo(ExtLoadEventArgs.Reason.InsertWaypoint);
            }

            sendLoadInfo(ExtLoadEventArgs.Reason.InsertTracks);
            for (int i = 0; i < gpx4kml.Tracks.Count; i++) {
               InsertTrack(gpx4kml.Tracks[i]);
               sendLoadInfo(ExtLoadEventArgs.Reason.InsertTrack);
            }

            postLoad();
         }

         for (int i = 0; i < trackcolor.Count; i++)
            if (trackcolor[i].ToArgb() == Color.Empty.ToArgb() ||
                trackcolor[i].ToArgb() == Color.Transparent.ToArgb())
               trackcolor[i] = dummycolor;

         return trackcolor;
      }

      public async Task<List<Color>> LoadAsync(string filename,
                                               bool removenamespace = false) =>
         await Task.Run(() => Load(filename, removenamespace));

      /// <summary>
      /// ev. Garmin-Zusatzinfos aus dem XML-Text holen (z.Z. nur Farbe)
      /// </summary>
      /// <param name="trackcolor">Liste der Trackfarben (Color.Empty für Tracks ohne Farbe)</param>
      protected virtual void postImportXml(out List<Color> trackcolor) {
         postLoad();

         trackcolor = new List<Color>();
         for (int i = 0; i < Tracks.Count; i++) {
            Color col = Color.Empty;
            // falls individuelle Farben def. sind:
            if (Tracks[i].UnhandledChildXml != null) {
               foreach (var item in Tracks[i].UnhandledChildXml!) {  // "!" wegen CS8602
                  if (item.StartsWith(XMLS_EXTENSIONS)) {
                     col = GarminSpecial.GetGarminColor(item);
                     break;
                  }
               }
            }
            trackcolor.Add(col);
         }
      }

      /// <summary>
      /// ev. notwendige Aufbereitung der GPX-Daten
      /// </summary>
      protected virtual void postLoad() { }

      #region Garmin-Farben und -Symbole

      public static class GarminSpecial {

         /* Trackfarbe bei Garmin:

              <trk>
                <name>Track</name>
                <extensions>
                  <gpxx:TrackExtension xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3">
                    <gpxx:DisplayColor>DarkRed</gpxx:DisplayColor>
                  </gpxx:TrackExtension>
                </extensions>
                <trkseg>
                  ...
                </trkseg>
              </trk>

            oder auch:

               <extensions>
                  <gpxx:TrackExtension>
                     <gpxx:DisplayColor>Black</gpxx:DisplayColor>
                  </gpxx:TrackExtension>
                  <gpxtrkx:TrackStatsExtension>
                     <gpxtrkx:Distance>21374.490</gpxtrkx:Distance>
                  </gpxtrkx:TrackStatsExtension>
               </extensions>
          */

         const string XML_GARMINSCHEME = "xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\"";

         const string XMLS_DISPLAYCOLOR = "<DisplayColor>";
         const string XMLS_EXTENSIONS = "<extensions>";
         const string XMLE_EXTENSIONS = "</extensions>";
         const string XMLS_TRACKEXTENSION = "<TrackExtension>";
         const string XMLS_TRACKEXTENSION1 = "<TrackExtension ";
         const string XMLS_GPXXTRACKEXTENSION = "<gpxx:TrackExtension>";
         const string XMLS_GPXXTRACKEXTENSION1 = "<gpxx:TrackExtension ";
         const string XMLE_GPXXTRACKEXTENSION = "</gpxx:TrackExtension>";
         const string XMLS_GPXXDISPLAYCOLOR = "<gpxx:DisplayColor>";
         const string XMLE_GPXXDISPLAYCOLOR = "</gpxx:DisplayColor>";

         const string XMLS_GPXXDISPLAYMODE = "<gpxx:DisplayMode>";
         const string XMLE_GPXXDISPLAYMODE = "</gpxx:DisplayMode>";
         const string XML_SYMBOLANDNAME = "SymbolAndName";
         const string XML_GPXXDISPLAYMODE_SYMBOLANDNAME = XMLS_GPXXDISPLAYMODE + XML_SYMBOLANDNAME + XMLE_GPXXDISPLAYMODE;
         const string XMLS_WAYPOINTEXTENSION0 = "<gpxx:WaypointExtension";
         const string XMLE_WAYPOINTEXTENSION = "</gpxx:WaypointExtension>";

         /// <summary>
         /// Garminfarbe lesen (xpath="/extensions/...:TrackExtension/...:DisplayColor") 
         /// </summary>
         /// <param name="xmlextension"></param>
         /// <returns>Color.Empty wenn keine Farbe gelesen wurde</returns>
         public static Color GetGarminColor(string xmlextension) {
            if (xmlextension.StartsWith(XMLS_EXTENSIONS)) {
               List<string> extensions_childs = getChildCollection(xmlextension, true);
               if (extensions_childs != null) {
                  for (int i = 0; i < extensions_childs.Count; i++) {
                     if (extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION) ||
                         extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION1) ||
                         extensions_childs[i].StartsWith(XMLS_TRACKEXTENSION) ||
                         extensions_childs[i].StartsWith(XMLS_TRACKEXTENSION1)) {
                        List<string> trackextension_childs = getChildCollection(extensions_childs[i], true);
                        if (trackextension_childs != null)
                           for (int j = 0; j < trackextension_childs.Count; j++) {
                              if (trackextension_childs[j].StartsWith(XMLS_DISPLAYCOLOR)) {
                                 string colortext = trackextension_childs[j].Substring(XMLS_DISPLAYCOLOR.Length,
                                                                                       trackextension_childs[j].Length - 2 * XMLS_DISPLAYCOLOR.Length - 1);
                                 if (colortext.StartsWith('#') && colortext.Length == 9) {    // XML-Farbe
                                    try {
                                       int a = Convert.ToInt32(colortext.Substring(1, 2), 16);
                                       int r = Convert.ToInt32(colortext.Substring(3, 2), 16);
                                       int g = Convert.ToInt32(colortext.Substring(5, 2), 16);
                                       int b = Convert.ToInt32(colortext.Substring(7, 2), 16);
                                       return Color.FromArgb(a, r, g, b);
                                    } catch {
                                       return Color.Empty;
                                    }
                                 } else
                                    return string.IsNullOrEmpty(colortext) ?
                                                      Color.Empty :
                                                      GarminTrackColors.Colors[GarminTrackColors.GetColorname(colortext)];
                              }
                           }
                        break;
                     }
                  }
               }
            }
            return Color.Empty;
         }

         static string getXmlTextWithChilds(string fullstarttag, List<string> childs, string endtag) {
            string xmltxt = fullstarttag;
            if (childs != null)
               foreach (string child in childs)
                  xmltxt += child;
            return xmltxt + endtag;
         }

         /// <summary>
         /// liefert die Hex-Codierung '#AARRGGBB'
         /// </summary>
         /// <param name="color"></param>
         /// <returns></returns>
         static string getXmlColor(Color? color) =>
            color != null ?
                        "#" +
                        color.Value.A.ToString("x2") +
                        color.Value.R.ToString("x2") +
                        color.Value.G.ToString("x2") +
                        color.Value.B.ToString("x2") : string.Empty;

         /// <summary>
         /// für die Garminfarbe wird der Garmintext für die geliefert; andernfalls die Farbe in Hex-Codierung: '#AARRGGBB'
         /// </summary>
         /// <param name="garmincol"></param>
         /// <param name="xmlcolor"></param>
         /// <returns></returns>
         static string getXmlText4GarminDisplayColor(GarminTrackColors.Colorname garmincol, Color? xmlcolor) =>
             XMLS_GPXXDISPLAYCOLOR +
             (xmlcolor == null ? GarminTrackColors.GetColorname(garmincol) : getXmlColor(xmlcolor)) +
             XMLE_GPXXDISPLAYCOLOR;

         static string getXmlText4GarminTrackExtension(List<string> trackextension_childs) =>
            getXmlTextWithChilds(XMLS_GPXXTRACKEXTENSION1 + XML_GARMINSCHEME + ">",
                                 trackextension_childs,
                                 XMLE_GPXXTRACKEXTENSION);

         static int getGarinDisplayColorIndex(List<string> trackextension_childs) {
            if (trackextension_childs != null)
               for (int i = 0; i < trackextension_childs.Count; i++)
                  if (trackextension_childs[i].StartsWith(XMLS_GPXXDISPLAYCOLOR))
                     return i;
            return -1;
         }

         /// <summary>
         /// Garmin-Trackfarbe schreiben (oder in NICHT-Garmin-Form als Hex-Codierung '#AARRGGBB')
         /// </summary>
         /// <param name="xmlextension">bisheriger Extension-Text (auch null)</param>
         /// <param name="garmincol">Garminfarbe</param>
         /// <param name="xmlcolor">wenn ungleich null dann Farbe als XMLText '#AARRGGBB'</param>
         /// <returns><paramref name="xmlextension"/> oder bei Veränderung neuer Text</returns>
         public static string? SetGarminColor(string? xmlextension, GarminTrackColors.Colorname garmincol, Color? xmlcolor) {
            //if (garmincol != GarminTrackColors.Colorname.Unknown)
            if (!string.IsNullOrEmpty(xmlextension)) {       // es ext. schon (irgend)eine Extension

               if (garmincol != GarminTrackColors.Colorname.Unknown ||  // Garminfarbe setzen/ersetzen
                   xmlcolor != null) {                                  // XML-Farbe setzen/ersetzen

                  List<string> extensions_childs = getChildCollection(xmlextension, false);


                  if (extensions_childs != null) {
                     bool found = false; ;
                     for (int i = 0; i < extensions_childs.Count; i++) {
                        if (extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION) ||
                            extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION1)) {
                           List<string> trackextension_childs = getChildCollection(extensions_childs[i], false);
                           int idxDisplayColor = getGarinDisplayColorIndex(trackextension_childs);
                           if (idxDisplayColor >= 0) {
                              trackextension_childs[idxDisplayColor] = getXmlText4GarminDisplayColor(garmincol, xmlcolor);
                              found = true;
                              extensions_childs[i] = getXmlText4GarminTrackExtension(trackextension_childs);
                              break;
                           } else {
                              if (trackextension_childs != null)                    //  "==" ?
                                 trackextension_childs = new List<string>();
                              trackextension_childs?.Add(getXmlText4GarminDisplayColor(garmincol, xmlcolor));
                           }
                        }
                     }
                     if (!found) {

                        extensions_childs.Add(getXmlText4GarminTrackExtension(new List<string> {
                           getXmlText4GarminDisplayColor(garmincol,xmlcolor)
                        }));

                     }

                  } else {       // es gibt noch keine Extension-Childs
                     extensions_childs = new List<string> {
                        getXmlText4GarminTrackExtension(new List<string> {
                           getXmlText4GarminDisplayColor(garmincol, xmlcolor)
                        })
                     };
                  }

                  xmlextension = getXmlTextWithChilds(XMLS_EXTENSIONS, extensions_childs, XMLE_EXTENSIONS);

               } else {                                           // Garminfarbe entfernen

                  List<string> extensions_childs = getChildCollection(xmlextension, false);
                  if (extensions_childs != null) {
                     for (int i = 0; i < extensions_childs.Count; i++) {
                        if (extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION) ||
                            extensions_childs[i].StartsWith(XMLS_GPXXTRACKEXTENSION1)) {
                           List<string> trackextension_childs = getChildCollection(extensions_childs[i], false);
                           int idxDisplayColor = getGarinDisplayColorIndex(trackextension_childs);
                           if (idxDisplayColor >= 0) {
                              trackextension_childs.RemoveAt(idxDisplayColor);
                              extensions_childs[i] = getXmlText4GarminTrackExtension(trackextension_childs);
                              break;
                           }
                        }
                     }
                     xmlextension = getXmlTextWithChilds(XMLS_EXTENSIONS, extensions_childs, XMLE_EXTENSIONS);
                  }

               }

            } else {                                        // es ext. noch keine Extension

               if (garmincol != GarminTrackColors.Colorname.Unknown ||  // Garminfarbe setzen/ersetzen
                   xmlcolor != null)                                    // XML-Farbe setzen/ersetzen

                  xmlextension = getXmlTextWithChilds(XMLS_EXTENSIONS,
                                                      new List<string> {
                                                         getXmlText4GarminTrackExtension(new List<string> {
                                                            getXmlText4GarminDisplayColor(garmincol, xmlcolor)
                                                         })
                                                      },
                                                      XMLE_EXTENSIONS);

            }
            return xmlextension;
         }

         /* <wpt>
               <sym>Flag, Red</sym>
               <extensions>
                 <gpxx:WaypointExtension xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3">
                   <gpxx:DisplayMode>SymbolAndName</gpxx:DisplayMode>
                 </gpxx:WaypointExtension>
               </extensions>
         */

         public static string SetGarminSymbolExt(string? xmlextension) {
            if (string.IsNullOrEmpty(xmlextension)) {
               xmlextension += XMLS_EXTENSIONS +
                               XMLS_WAYPOINTEXTENSION0 + " " + XML_GARMINSCHEME + ">" +
                               XML_GPXXDISPLAYMODE_SYMBOLANDNAME +
                               XMLE_WAYPOINTEXTENSION +
                               XMLE_EXTENSIONS;
            } else {
               if (xmlextension.StartsWith(XMLS_EXTENSIONS)) {
                  int start = xmlextension.IndexOf(XMLS_GPXXDISPLAYMODE);
                  if (start >= 0) {
                     start = xmlextension.IndexOf("<", start + 1);
                     int end = xmlextension.IndexOf(XMLE_GPXXDISPLAYMODE, start + 1);
                     if (start < end) {
                        xmlextension = xmlextension.Substring(0, start) +
                                       XML_SYMBOLANDNAME +
                                       xmlextension.Substring(end);
                     }
                  } else {
                     start = xmlextension.IndexOf(XMLS_EXTENSIONS + XMLS_WAYPOINTEXTENSION0);
                     if (start >= 0) {
                        start = xmlextension.IndexOf("<", start + 1);
                        xmlextension = xmlextension.Substring(0, start) + XML_GPXXDISPLAYMODE_SYMBOLANDNAME + xmlextension.Substring(start);
                     } else {
                        start = xmlextension.IndexOf("<", 1);
                        xmlextension = xmlextension.Substring(0, start) +
                                       XMLS_EXTENSIONS +
                                       XMLS_WAYPOINTEXTENSION0 + " " + XML_GARMINSCHEME + ">" +
                                       XML_GPXXDISPLAYMODE_SYMBOLANDNAME +
                                       XMLE_WAYPOINTEXTENSION +
                                       XMLE_EXTENSIONS +
                                       xmlextension.Substring(start);
                     }
                  }
               }
            }

            return xmlextension;
         }
      }

      #endregion


   }

}
