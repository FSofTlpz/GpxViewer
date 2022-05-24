using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Daten eines Tracks
   /// </summary>
   public class GpxTrack : BaseElement {

      public const string NODENAME = "trk";

      public List<GpxTrackSegment> Segments;

      public string Name;

      public string Comment;

      public string Description;

      public string Source;


      public GpxTrack(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxTrack(GpxTrack t) : base() {
         Name = t.Name;
         Comment = t.Comment;
         Description = t.Description;
         Source = t.Source;
         for (int s = 0; s < t.Segments.Count; s++)
            Segments.Add(new GpxTrackSegment(t.Segments[s]));
      }

      protected override void Init() {
         if (Segments == null)
            Segments = new List<GpxTrackSegment>();
         else
            Segments.Clear();
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         string[] tmp = XReadOuterXml(nav, "/" + NODENAME + "/" + GpxTrackSegment.NODENAME);
         for (int s = 0; s < tmp.Length; s++)
            Segments.Add(new GpxTrackSegment(tmp[s]));
         string prefix = "/" + NODENAME + "/";
         Name = XReadString(nav, prefix + "name");
         Comment = XReadString(nav, prefix + "cmt");
         Description = XReadString(nav, prefix + "desc");
         Source = XReadString(nav, prefix + "src");

         // registrieren der unbehandelten Childs
         RegisterUnhandledChild(nav,
                                "/" + NODENAME + "/*",
                                new string[] {
                                   "<name>",
                                   "<cmt>",
                                   "<desc>",
                                   "<src>",
                                   "<" + GpxTrackSegment.NODENAME + ">",
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale = int.MaxValue) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: name, cmt, desc, src, link (mehrfach), number, type, extensions, trseg (mehrfach)
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
            if (scale > 1)
               sb.Append(item.Value);
            lastidx = item.Key;
         }
         while ((txt = HandledAsXml(handled++, scale)) != null) // noch alle behandelten Childs ausgegeben
            sb.Append(txt);

         for (int p = 0; p < Segments.Count; p++)
            sb.Append(Segments[p].AsXml(scale));

         return XWriteNode(NODENAME, sb.ToString());
      }

      protected string HandledAsXml(int handled, int scale) {
         switch (handled) {
            case 0:
               if (!string.IsNullOrEmpty(Name))
                  return XWriteNode("name", XmlClean(Name));
               break;

            case 1:
               if (!string.IsNullOrEmpty(Comment) && scale > 0)
                  return XWriteNode("cmt", XmlClean(Comment));
               break;

            case 2:
               if (!string.IsNullOrEmpty(Description) && scale > 0)
                  return XWriteNode("desc", XmlClean(Description));
               break;

            case 3:
               if (!string.IsNullOrEmpty(Source) && scale > 0)
                  return XWriteNode("src", XmlClean(Source));
               break;

            default:
               return null; // keine behandelten Childs mehr
         }
         return "";
      }

      /// <summary>
      /// liefert das <see cref="GpxTrackSegment"/> aus der Liste oder null
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackSegment GetSegment(int s) {
         return s < Segments.Count ? Segments[s] : null;
      }

      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackPoint GetSegmentPoint(int s, int p) {
         return GetSegment(s)?.GetPoint(p);
      }

      /// <summary>
      /// entfernt das <see cref="GpxTrackSegment"/> aus der Liste
      /// </summary>
      /// <param name="s"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveSegment(int s) {
         if (0 <= s && s < Segments.Count) {
            Segments.RemoveAt(s);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt den <see cref="GpxTrackPoint"/> aus der Liste
      /// </summary>
      /// <param name="s"></param>
      /// <param name="p"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveSegmentPoint(int s, int p) {
         if (0 <= s && s < Segments.Count)
            return Segments[s].RemovePoint(p);
         return false;
      }


      /// <summary>
      /// fügt ein <see cref="GpxTrackSegment"/> ein oder an
      /// </summary>
      /// <param name="s"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertSegment(GpxTrackSegment s, int pos = -1) {
         if (pos < 0 || Segments.Count <= pos)
            Segments.Add(s);
         else
            Segments.Insert(pos, s);
      }

      /// <summary>
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="s">Segment</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertSegmentPoint(GpxTrackPoint p, int s, int pos = -1) {
         GetSegment(s)?.InsertPoint(p, pos);
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (!string.IsNullOrEmpty(Name))
            sb.AppendFormat(" name=[{0}]", Name);
         sb.AppendFormat(" {0} Segmente", Segments.Count);
         return sb.ToString();
      }

   }

}
