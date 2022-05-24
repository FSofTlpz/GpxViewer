using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// alle Daten einer GPX-Datei
   /// </summary>
   public class GpxAll : BaseElement {

      public const string NODENAME = "gpx";

      public GpxMetadata1_1 Metadata;

      public List<GpxWaypoint> Waypoints;

      public List<GpxRoute> Routes;

      public List<GpxTrack> Tracks;


      public GpxAll(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }


      protected override void Init() {
         Metadata = new GpxMetadata1_1();
         Waypoints = new List<GpxWaypoint>();
         Routes = new List<GpxRoute>();
         Tracks = new List<GpxTrack>();
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         string[] tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxMetadata1_1.NODENAME);
         if (tmp != null)
            Metadata = new GpxMetadata1_1(tmp[0]);
         else {
            Metadata = new GpxMetadata1_1();

            tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxBounds.NODENAME);
            if (tmp != null)
               Metadata.Bounds = new GpxBounds(tmp[0]);

            tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxTime1_0.NODENAME);
            if (tmp != null) {
               GpxTime1_0 time = new GpxTime1_0(tmp[0]);
               Metadata.Time = time.Time;
            }
         }

         Waypoints = new List<GpxWaypoint>();
         tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxWaypoint.NODENAME);
         if (tmp != null) {
            for (int w = 0; w < tmp.Length; w++)
               Waypoints.Add(new GpxWaypoint(tmp[w]));
         }

         Routes = new List<GpxRoute>();
         tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxRoute.NODENAME);
         if (tmp != null) {
            for (int r = 0; r < tmp.Length; r++)
               Routes.Add(new GpxRoute(tmp[r]));
         }

         Tracks = new List<GpxTrack>();
         tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxTrack.NODENAME);
         if (tmp != null) {
            for (int t = 0; t < tmp.Length; t++)
               Tracks.Add(new GpxTrack(tmp[t]));
         }

         // registrieren der unbehandelten Childs
         RegisterUnhandledChild(nav,
                                "/" + NODENAME + "/*",
                                new string[] {
                                   "<" + GpxMetadata1_1.NODENAME + ">",
                                   "<" + GpxBounds.NODENAME + ">",
                                   "<" + GpxTime1_0.NODENAME + ">",
                                   "<" + GpxWaypoint.NODENAME + ">",
                                   "<" + GpxRoute.NODENAME + ">",
                                   "<" + GpxTrack.NODENAME + ">",
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
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
            foreach (KeyValuePair<int, string> item in UnhandledChildXml)
               if (item.Value.StartsWith("<extensions>"))
                  sb.Append(item.Value);

         return XWriteNode(NODENAME, sb.ToString());
      }

      /// <summary>
      /// Bound in den Metadaten neu ermitteln
      /// </summary>
      public void RebuildMetadataBounds() {
         GpxBounds bounds = new GpxBounds();
         foreach (GpxWaypoint wp in Waypoints) {
            bounds.Union(wp);
         }
         foreach (GpxTrack track in Tracks) {
            foreach (GpxTrackSegment segment in track.Segments) {
               foreach (GpxTrackPoint pt in segment.Points) {
                  bounds.Union(pt);
               }
            }
         }
         foreach (GpxRoute route in Routes) {
            foreach (GpxRoutePoint pt in route.Points) {
               bounds.Union(pt);
            }
         }
         Metadata.Bounds = bounds;
      }

      /// <summary>
      /// liefert den <see cref="GpxWaypoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="w"></param>
      /// <returns></returns>
      public GpxWaypoint GetWaypoint(int w) {
         return w < Waypoints.Count ? Waypoints[w] : null;
      }

      /// <summary>
      /// liefert die <see cref="GpxRoute"/> aus der Liste oder null
      /// </summary>
      /// <param name="r"></param>
      /// <returns></returns>
      public GpxRoute GetRoute(int r) {
         return r < Routes.Count ? Routes[r] : null;
      }

      /// <summary>
      /// liefert den <see cref="GpxRoutePoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="r"></param>
      /// <returns></returns>
      public GpxRoutePoint GetRoutePoint(int r, int p) {
         return GetRoute(r)?.GetPoint(p);
      }

      /// <summary>
      /// liefert den <see cref="GpxTrack"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <returns></returns>
      public GpxTrack GetTrack(int t) {
         return t < Tracks.Count ? Tracks[t] : null;
      }

      /// <summary>
      /// liefert das <see cref="GpxTrackSegment"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackSegment GetTrackSegment(int t, int s) {
         return GetTrack(t)?.GetSegment(s);
      }

      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="t"></param>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackPoint GetTrackSegmentPoint(int t, int s, int p) {
         return GetTrack(t)?.GetSegmentPoint(s, p);
      }


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
      public void InsertRoutePoint(GpxRoutePoint p, int r, int pos = -1) {
         GetRoute(r)?.InsertPoint(p, pos);
      }

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
      public void InsertTrackSegment(GpxTrackSegment s, int t, int pos = -1) {
         GetTrack(t)?.InsertSegment(s, pos);
      }

      /// <summary>
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="t">Track</param>
      /// <param name="s">Segment</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertTrackSegmentPoint(GpxTrackPoint p, int t, int s, int pos = -1) {
         GetTrackSegment(t, s)?.InsertPoint(p, pos);
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         sb.AppendFormat(" {0} Waypoints", Waypoints.Count);
         sb.AppendFormat(" {0} Routes", Routes.Count);
         sb.AppendFormat(" {0} Tracks", Tracks.Count);
         return sb.ToString();
      }

   }

}
