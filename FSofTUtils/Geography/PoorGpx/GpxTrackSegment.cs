using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Segmentdaten eines Tracks
   /// </summary>
   public class GpxTrackSegment : BaseElement {

      public const string NODENAME = "trkseg";

      public List<GpxTrackPoint> Points;


      public GpxTrackSegment(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxTrackSegment(GpxTrackSegment s) : base() {
         for (int p = 0; p < s.Points.Count; p++)
            Points.Add(new GpxTrackPoint(s.Points[p]));
      }


      protected override void Init() {
         if (Points == null)
            Points = new List<GpxTrackPoint>();
         else
            Points.Clear();
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         string[] tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxTrackPoint.NODENAME);
         if (tmp != null)
            for (int p = 0; p < tmp.Length; p++)
               Points.Add(new GpxTrackPoint(tmp[p]));

         // registrieren der unbehandelten Childs
         RegisterUnhandledChild(nav,
                                "/" + NODENAME + "/*",
                                new string[] {
                                   "<" + GpxTrackPoint.NODENAME + " ",     // '<trkpt ' !!
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale = int.MaxValue) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: trkpt (mehrfach), extensions
         for (int p = 0; p < Points.Count; p++)
            sb.Append(Points[p].AsXml(scale));

         int handled = 0; // für die Reihenfolge der handled Childs
         int lastidx = -1;
         string txt;
         foreach (KeyValuePair<int, string> item in UnhandledChildXml) {
            while (item.Key - 1 != lastidx) { // Lücke in der Folge der Childs, d.h. davor liegt min. 1 behandeltes Child
               txt = HandledAsXml(handled++, scale);
               if (txt != null)
                  sb.Append(txt);
               lastidx++;
            }
            sb.Append(item.Value);
            if (scale > 1)
               lastidx = item.Key;
         }
         while ((txt = HandledAsXml(handled++, scale)) != null) // noch alle behandelten Childs ausgegeben
            sb.Append(txt);

         return XWriteNode(NODENAME, sb.ToString());
      }

      protected string HandledAsXml(int handled, int scale) {
         switch (handled) {

            default:
               return null; // keine behandelten Childs mehr
         }
         //return "";
      }


      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public GpxTrackPoint GetPoint(int idx) {
         return idx < Points.Count ? Points[idx] : null;
      }

      /// <summary>
      /// entfernt den <see cref="GpxTrackPoint"/> aus der Liste
      /// </summary>
      /// <param name="p"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemovePoint(int p) {
         if (0 <= p && p < Points.Count) {
            Points.RemoveAt(p);
            return true;
         }
         return false;
      }

      /// <summary>
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertPoint(GpxTrackPoint p, int pos = -1) {
         if (pos < 0 || Points.Count <= pos)
            Points.Add(p);
         else
            Points.Insert(pos, p);
      }

      /// <summary>
      /// ändert die Richtung
      /// </summary>
      public void ChangeDirection() {
         List<GpxTrackPoint> tmp = new List<GpxTrackPoint>();
         for (int i = Points.Count - 1; i >= 0; i--)
            tmp.Add(Points[i]);
         Points = tmp;
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         sb.AppendFormat(" {0} Punkte", Points.Count);
         return sb.ToString();
      }

   }

}
