using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Daten einer Route
   /// </summary>
   public class GpxRoute : BaseElement {

      public const string NODENAME = "rte";

      public List<GpxRoutePoint> Points;

      public string Name;

      public string Comment;

      public string Description;

      public string Source;


      public GpxRoute(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxRoute(GpxRoute r) : base() {
         Name = r.Name;
         for (int p = 0; p < r.Points.Count; p++)
            Points.Add(new GpxRoutePoint(r.Points[p]));
      }


      protected override void Init() {
         if (Points == null)
            Points = new List<GpxRoutePoint>();
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

         string[] tmp = XReadOuterXml(nav, "/" + NODENAME + "/ " + GpxRoutePoint.NODENAME);
         for (int p = 0; p < tmp.Length; p++)
            Points.Add(new GpxRoutePoint(tmp[p]));
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
                                   "<" + GpxRoutePoint.NODENAME + " ",
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: name, cmt, desc, src, link (mehrfach), number, type, extensions, rtept (mehrfach)
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

         for (int p = 0; p < Points.Count; p++)
            sb.Append(Points[p].AsXml(scale));

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
      /// liefert den <see cref="GpxRoutePoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public GpxRoutePoint GetPoint(int p) {
         return p < Points.Count ? Points[p] : null;
      }


      /// <summary>
      /// entfernt den <see cref="GpxRoutePoint"/> aus der Liste
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
      /// fügt einen <see cref="GpxRoutePoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertPoint(GpxRoutePoint p, int pos = -1) {
         if (pos < 0 || Points.Count <= pos)
            Points.Add(p);
         else
            Points.Insert(pos, p);
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         sb.AppendFormat(" {0} Punkte", Points.Count);
         return sb.ToString();
      }

   }

}
