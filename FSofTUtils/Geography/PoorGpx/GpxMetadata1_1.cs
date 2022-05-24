using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Metadaten für Version 1.1
   /// </summary>
   public class GpxMetadata1_1 : BaseElement {

      public const string NODENAME = "metadata";

      /// <summary>
      /// Zeitpunkt
      /// </summary>
      public DateTime Time;

      public GpxBounds Bounds;


      public GpxMetadata1_1(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      protected override void Init() {
         Time = NOTVALID_TIME;
         Bounds = new GpxBounds();
      }

      public void SetMaxDateTime(DateTime dt) {
         if (Time == NOTVALID_TIME)
            Time = dt;
         else {
            if (dt != NOTVALID_TIME)
               if (Time < dt)
                  Time = dt;
         }
      }

      public void SetMinDateTime(DateTime dt) {
         if (Time == NOTVALID_TIME)
            Time = dt;
         else {
            if (dt != NOTVALID_TIME)
               if (Time > dt)
                  Time = dt;
         }
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         Bounds.MinLat = XReadDouble(nav, "/" + NODENAME + "/bounds/@minlat");
         Bounds.MinLon = XReadDouble(nav, "/" + NODENAME + "/bounds/@minlon");
         Bounds.MaxLat = XReadDouble(nav, "/" + NODENAME + "/bounds/@maxlat");
         Bounds.MaxLon = XReadDouble(nav, "/" + NODENAME + "/bounds/@maxlon");
         Time = XReadDateTime(nav, "/" + NODENAME + "/time");

         // registrieren der unbehandelten Childs
         RegisterUnhandledChild(nav,
                                "/" + NODENAME + "/*",
                                new string[] {
                                   "<time>",
                                });
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale = int.MaxValue) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: name, desc, author, copyright, link (mehrfach), time, keywords, bounds, extensions
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

         return XWriteNode(NODENAME, sb.ToString());
      }

      protected string HandledAsXml(int handled, int scale) {
         switch (handled) {
            case 0:
               if (Time != NOTVALID_TIME)
                  return XWriteNode("time", Time);
               break;

            case 1:
               if (Bounds.IsValid()) 
                  return Bounds.AsXml(scale);
               break;

            default:
               return null; // keine behandelten Childs mehr
         }
         return "";
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (Time != NOTVALID_TIME)
            sb.AppendFormat(" {0}", Time);
         if (Bounds.IsValid())
            sb.AppendFormat(" {0}", Bounds.ToString());
         return sb.ToString();
      }

   }

}
