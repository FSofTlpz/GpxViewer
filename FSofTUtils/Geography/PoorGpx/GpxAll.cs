using System;
using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// alle Daten einer GPX-Datei
   /// </summary>
   public class GpxAll : BaseElement {

      /* https://www.topografix.com/GPX/1/1/

         <xsd:complexType name="gpxType">
            <xsd:sequence>
               <xsd:element name="metadata" type="metadataType" minOccurs="0"/>
               <xsd:element name="wpt" type="wptType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="rte" type="rteType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="trk" type="trkType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
            </xsd:sequence>
            <xsd:attribute name="version" type="xsd:string" use="required" fixed="1.1"/>
            <xsd:attribute name="creator" type="xsd:string" use="required"/>
         </xsd:complexType>
   */

      public const string NODENAME = "gpx";

      public class LoadEventArgs {
         public enum LoadReason {
            InsertWaypoints,
            InsertTracks,
            InsertRoutes,
            InsertWaypoint,
            InsertTrack,
            InsertRoute,
         }


         public readonly LoadReason Reason;

         public LoadEventArgs(LoadReason reason) => Reason = reason;
      }

      public event EventHandler<LoadEventArgs>? LoadInfoEvent;

      /// <summary>
      /// GPX-Version (i.A. "1.1")
      /// </summary>
      public string Version = "1.1";

      /// <summary>
      /// GPX-Erzeuger
      /// </summary>
      public string Creator = string.Empty;

      /// <summary>
      /// Metadaten (Umgrenzung und Zeitpunkt)
      /// </summary>
      public GpxMetadata1_1 Metadata = new GpxMetadata1_1();

      /// <summary>
      /// Liste der Wegpunkte
      /// </summary>
      public ListTS<GpxWaypoint> Waypoints = new ListTS<GpxWaypoint>();

      /// <summary>
      /// Liste der Routen
      /// </summary>
      public ListTS<GpxRoute> Routes = new ListTS<GpxRoute>();

      /// <summary>
      /// Liste der Tracks
      /// </summary>
      public ListTS<GpxTrack> Tracks = new ListTS<GpxTrack>();

      /// <summary>
      /// Attribute des GPX-Tags
      /// </summary>
      protected List<(string, string)>? gpxattributes;

      /// <summary>
      /// Attribute des "&lt;?xml"-Tags
      /// </summary>
      protected List<(string, string)>? xmlattributes;


      public GpxAll(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      protected override void Init() {
         Metadata = new GpxMetadata1_1();
      }

      #region liest das Objekt aus einem XML-Text ein

      protected const string TAGMETADATA = "<" + GpxMetadata1_1.NODENAME + ">";
      protected const string TAGTIME1_0 = "<" + GpxTime1_0.NODENAME + ">";      // eigentlich nicht zulässig aber manchmal verwendet
      protected const string TAGWAYPOINT = "<" + GpxWaypoint.NODENAME + " ";    // mit Attr. !
      protected const string TAGROUTE = "<" + GpxRoute.NODENAME + ">";
      protected const string TAGTRACK = "<" + GpxTrack.NODENAME + ">";

      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();

         string? firsttag = getFirstXmlTag(xmltxt);

         if (firsttag != null) {
            if (firsttag.StartsWith("<?xml ")) {
               xmlattributes = getAttributeCollection(firsttag, false);    // Attribute von "<?xml ...>" immer MIT Namespace
               xmltxt = xmltxt.Substring(firsttag.Length);
            }

            gpxattributes = getAttributeCollection(xmltxt, false);         // Attribute von "<gpx ...>" immer MIT Namespace
            for (int i = 0; i < gpxattributes.Count; i++) {
               if (gpxattributes[i].Item1 == "version")
                  Version = gpxattributes[i].Item2;
               else if (gpxattributes[i].Item1 == "creator")
                  Creator = gpxattributes[i].Item2;
            }

            UnhandledChildXml = getChildCollection(xmltxt, removenamespace);  // alle Childs erstmal als UnhandledChildXml registrieren

            if (UnhandledChildXml != null) {

               for (int i = 0; i < UnhandledChildXml.Count; i++) {
                  string childtxt = UnhandledChildXml[i];
                  string? tag = getFirstXmlTag(childtxt);

                  if (tag != null) {
                     bool getit = false;
                     if (tag == TAGMETADATA) {
                        Metadata = new GpxMetadata1_1(childtxt);
                        getit = true;
                     } else if (tag.StartsWith(TAGTIME1_0)) {
                        GpxTime1_0 time = new GpxTime1_0(childtxt);
                        Metadata.Time = time.Time;
                        getit = true;
                     } else if (tag.StartsWith(TAGWAYPOINT)) {
                        if (Waypoints.Count == 0)
                           LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertWaypoints));
                        Waypoints.Add(new GpxWaypoint(childtxt));
                        LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertWaypoint));
                        getit = true;
                     } else if (tag == TAGROUTE) {
                        if (Routes.Count == 0)
                           LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertRoutes));
                        Routes.Add(new GpxRoute(childtxt));
                        LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertRoute));
                        getit = true;
                     } else if (tag == TAGTRACK) {
                        if (Tracks.Count == 0)
                           LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertTracks));
                        Tracks.Add(new GpxTrack(childtxt));
                        LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.LoadReason.InsertTrack));
                        getit = true;
                     }
                     if (getit) {
                        UnhandledChildXml.RemoveAt(i);
                        i--;
                     }
                  }
               }

               if (UnhandledChildXml.Count == 0)
                  UnhandledChildXml = null;        // wird nicht mehr benötigt
            }
         }
      }

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale = int.MaxValue) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: metadata, wpt (mehrfach), rte (mehrfach), trk (mehrfach), extensions

         sb.Append(Metadata.AsXml(scale));

         for (int i = 0; i < Waypoints.Count; i++)
            sb.Append(Waypoints[i].AsXml(scale));

         for (int i = 0; i < Routes.Count; i++)
            sb.Append(Routes[i].AsXml(scale));

         for (int i = 0; i < Tracks.Count; i++)
            sb.Append(Tracks[i].AsXml(scale));

         if (scale > 1)
            if (UnhandledChildXml != null)
               foreach (var item in UnhandledChildXml)
                  if (item.StartsWith("<extensions>"))
                     sb.Append(item);

         getGpxAttributes(out List<string> attr, out List<string> values);
         return xWriteNode(NODENAME,
                           attr,
                           values,
                           sb.ToString());
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public void AsXml(StringBuilder sb, int scale = int.MaxValue) {
         // Sequenz: metadata, wpt (mehrfach), rte (mehrfach), trk (mehrfach), extensions

         Metadata.AsXml(sb, scale);

         for (int i = 0; i < Waypoints.Count; i++)
            Waypoints[i].AsXml(sb, scale);

         for (int i = 0; i < Routes.Count; i++)
            Routes[i].AsXml(sb, scale);

         for (int i = 0; i < Tracks.Count; i++)
            Tracks[i].AsXml(sb, scale);

         if (scale > 1)
            if (UnhandledChildXml != null)
               foreach (var item in UnhandledChildXml)
                  if (item.StartsWith("<extensions>"))
                     sb.Append(item);

         getGpxAttributes(out List<string> attr, out List<string> values);
         xWriteNode(sb,
                    NODENAME,
                    attr,
                    values);
      }

      void getGpxAttributes(out List<string> attr, out List<string> values) {
         attr = new List<string>(new string[] { "version", "creator" });
         values = new List<string>(new string[] { Version, Creator });
         if (gpxattributes != null)
            for (int i = 0; i < gpxattributes.Count; i++) {
               if (gpxattributes[i].Item1 != "version" &&
                   gpxattributes[i].Item1 != "creator") {
                  attr.Add(gpxattributes[i].Item1);
                  values.Add(gpxattributes[i].Item2);
               }
            }
      }

      /// <summary>
      /// liefert das XML-Tag mit den Originalattributen
      /// </summary>
      /// <returns></returns>
      public string GetXmlTag() {
         List<string> attr = new List<string>();
         List<string> values = new List<string>();
         if (xmlattributes != null)
            for (int i = 0; i < xmlattributes.Count; i++) {
               attr.Add(xmlattributes[i].Item1);
               values.Add(xmlattributes[i].Item2);
            }
         return xWriteNode("?xml", attr, values);
      }

      #endregion

      /// <summary>
      /// Bound in den Metadaten neu ermitteln
      /// </summary>
      public void RebuildMetadataBounds() {
         GpxBounds bounds = new GpxBounds();

         for (int i = 0; i < Waypoints.Count; i++)
            bounds.Union(Waypoints[i]);

         for (int t = 0; t < Tracks.Count; t++)
            for (int s = 0; s < Tracks[t].Segments.Count; s++)
               for (int p = 0; p < Tracks[t].Segments[s].Points.Count; p++)
                  bounds.Union(Tracks[t].Segments[s].Points[p]);

         for (int r = 0; r < Routes.Count; r++)
            for (int p = 0; p < Routes[r].Points.Count; p++)
               bounds.Union(Routes[r].Points[p]);

         Metadata.Bounds = bounds;
      }

      /// <summary>
      /// liefert den <see cref="GpxWaypoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="w"></param>
      /// <returns></returns>
      public GpxWaypoint? GetWaypoint(int w) => w < Waypoints.Count ? Waypoints[w] : null;

      /// <summary>
      /// liefert die <see cref="GpxRoute"/> aus der Liste oder null
      /// </summary>
      /// <param name="r"></param>
      /// <returns></returns>
      public GpxRoute? GetRoute(int r) => r < Routes.Count ? Routes[r] : null;

      /// <summary>
      /// liefert den <see cref="GpxRoutePoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="r"></param>
      /// <returns></returns>
      public GpxRoutePoint? GetRoutePoint(int r, int p) => GetRoute(r)?.GetPoint(p);

      /// <summary>
      /// liefert den <see cref="GpxTrack"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <returns></returns>
      public GpxTrack? GetTrack(int t) => t < Tracks.Count ? Tracks[t] : null;

      /// <summary>
      /// liefert das <see cref="GpxTrackSegment"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackSegment? GetTrackSegment(int t, int s) => GetTrack(t)?.GetSegment(s);

      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackPoint? GetTrackSegmentPoint(int t, int s, int p) => GetTrack(t)?.GetSegmentPoint(s, p);


      /// <summary>
      /// entfernt den <see cref="Waypoint"/> aus der Liste
      /// </summary>
      /// <param name="w"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveWaypoint(int w) {
         if (0 <= w && w < Waypoints.Count) {
            Waypoints.RemoveAt(w);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt die <see cref="GpxRoute"/> aus der Liste
      /// </summary>
      /// <param name="r"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveRoute(int r) {
         if (0 <= r && r < Routes.Count) {
            Routes.RemoveAt(r);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt den <see cref="GpxRoutePoint"/> aus der Liste
      /// </summary>
      /// <param name="r"></param>
      /// <param name="p"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveRoutePoint(int r, int p) {
         if (0 <= r && r < Routes.Count)
            return Routes[r].RemovePoint(p);
         return false;
      }

      /// <summary>
      /// entfernt den <see cref="GpxTrack"/> aus der Liste
      /// </summary>
      /// <param name="t"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveTrack(int t) {
         if (0 <= t && t < Tracks.Count) {
            Tracks.RemoveAt(t);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt das <see cref="GpxTrackSegment"/> aus der Liste
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveTrackSegment(int t, int s) {
         if (0 <= t && t < Tracks.Count)
            return Tracks[t].RemoveSegment(s);
         return false;
      }

      /// <summary>
      /// entfernt den <see cref="GpxTrackPoint"/> aus der Liste
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <param name="p"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveTrackSegmentPoint(int t, int s, int p) {
         if (0 <= t && t < Tracks.Count)
            return Tracks[t].RemoveSegmentPoint(s, p);
         return false;
      }


      /// <summary>
      /// fügt einen <see cref="Waypoint"/> ein oder an
      /// </summary>
      /// <param name="wp"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertWaypoint(GpxWaypoint wp, int pos = -1) {
         if (pos < 0 || Waypoints.Count <= pos)
            Waypoints.Add(wp);
         else
            Waypoints.Insert(pos, wp);
      }

      /// <summary>
      /// fügt eine <see cref="GpxRoute"/> ein oder an
      /// </summary>
      /// <param name="r"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertRoute(GpxRoute r, int pos = -1) {
         if (pos < 0 || Routes.Count <= pos)
            Routes.Add(r);
         else
            Routes.Insert(pos, r);
      }

      /// <summary>
      /// fügt einen <see cref="GpxRoutePoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="r">Track</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertRoutePoint(GpxRoutePoint p, int r, int pos = -1) => GetRoute(r)?.InsertPoint(p, pos);

      /// <summary>
      /// fügt einen <see cref="GpxTrack"/> ein oder an
      /// </summary>
      /// <param name="t"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertTrack(GpxTrack t, int pos = -1) {
         if (pos < 0 || Tracks.Count <= pos)
            Tracks.Add(t);
         else
            Tracks.Insert(pos, t);
      }

      /// <summary>
      /// fügt ein <see cref="GpxTrackSegment"/> ein oder an
      /// </summary>
      /// <param name="s"></param>
      /// <param name="t">Track</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertTrackSegment(GpxTrackSegment s, int t, int pos = -1) => GetTrack(t)?.InsertSegment(s, pos);

      /// <summary>
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="t">Track</param>
      /// <param name="s">Segment</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertTrackSegmentPoint(GpxTrackPoint p, int t, int s, int pos = -1) => GetTrackSegment(t, s)?.InsertPoint(p, pos);


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         sb.AppendFormat(" {0} Waypoints", Waypoints.Count);
         sb.AppendFormat(" {0} Routes", Routes.Count);
         sb.AppendFormat(" {0} Tracks", Tracks.Count);
         return sb.ToString();
      }

   }

}
