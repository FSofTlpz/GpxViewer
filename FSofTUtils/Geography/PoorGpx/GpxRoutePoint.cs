using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Routenpunktdaten
   /// </summary>
   public class GpxRoutePoint : GpxPointBase {

      public const string NODENAME = "rtept";

      public string Name;

      public string Comment;

      public string Description;

      public string Symbol;


      public GpxRoutePoint(string xmltext = null, bool removenamespace = false) :
         base(NODENAME, xmltext, removenamespace) { }


      public GpxRoutePoint(GpxRoutePoint p) : base(NODENAME) {
         Lat = p.Lat;
         Lon = p.Lon;
         Elevation = p.Elevation;
         Time = p.Time;
      }

      public GpxRoutePoint(double lon, double lat, double ele = NOTVALID_DOUBLE) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
      }

      public GpxRoutePoint(double lon, double lat, double ele, DateTime time) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
         Time = time;
      }

      protected override void Init() {
         BaseInit();
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = BaseFromXml(xmltxt, removenamespace);

         string prefix = "/" + NODENAME + "/";
         Name = XReadString(nav, prefix + "name");
         Comment = XReadString(nav, prefix + "cmt");
         Description = XReadString(nav, prefix + "desc");
         Symbol = XReadString(nav, prefix + "sym");

         // registrieren der unbehandelten Childs
         RegisterUnhandledChild(nav,
                                "/" + NODENAME + "/*",
                                new string[] {
                                   "<ele>",     // in GpxPointBase
                                   "<time>",    //       "
                                   "<name>",
                                   "<cmt>",
                                   "<desc>",
                                   "<sym>",
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         List<string> attrname;
         List<string> attrvalue;

         StringBuilder sb = new StringBuilder(GetXmlNodeData(out attrname, out attrvalue));

         // Sequenz: ele, time, magvar, geoidheight, name, cmt, desc, src, link (mehrfach), sym, type, fix, sat, hdop, vdop, pdop, ageofdgpsdata, dgpsid, extensions
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

         return XWriteNode(NODENAME, attrname, attrvalue, sb.ToString());
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
               if (!string.IsNullOrEmpty(Symbol) && scale > 0)
                  return XWriteNode("sym", XmlClean(Symbol));
               break;

            default:
               return null; // keine behandelten Childs mehr
         }
         return "";
      }

      public override string ToString() {
         return base.ToString();
      }

   }

}
