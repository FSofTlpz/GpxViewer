using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Basisklasse für alle Arten von Punkten (lat/lon ist immer vorhanden)
   /// </summary>
   public abstract class GpxPointBase : BaseElement {

      /// <summary>
      /// Latitude (Breite, y), 0°...360°
      /// </summary>
      public double Lat;
      /// <summary>
      /// Longitude (Länge, x), -180°...180°
      /// </summary>
      public double Lon;
      /// <summary>
      /// Höhe
      /// </summary>
      public double Elevation;
      /// <summary>
      /// Zeitpunkt
      /// </summary>
      public DateTime Time;

      string nodename;


      protected GpxPointBase(string mainnode, string xmltext = null, bool removenamespace = false) :
         base() {
         nodename = mainnode;
         if (xmltext != null)
            FromXml(xmltext, removenamespace);
      }


      protected void BaseInit() {
         Lat = Lon = Elevation = NOTVALID_DOUBLE;
         Time = NOTVALID_TIME;
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      protected XPathNavigator BaseFromXml(string xmltxt, bool removenamespace) {
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         string prefix = "/" + nodename + "/";
         Lat = XReadDouble(nav, prefix + "@lat");
         Lon = XReadDouble(nav, prefix + "@lon");
         Elevation = XReadDouble(nav, prefix + "ele");
         Time = XReadDateTime(nav, prefix + "time");
         return nav;
      }

      /// <summary>
      /// liefert die Daten für die Erzeugung eines XML-Nodes
      /// </summary>
      /// <param name="attrname">Liste der Attributnamen</param>
      /// <param name="attrvalue">Liste der Attributwerte</param>
      /// <returns>vollständiger Text der Subnodes</returns>
      protected string GetXmlNodeData(out List<string> attrname, out List<string> attrvalue) {
         string subnodes = "";
         attrname = new List<string>();
         attrvalue = new List<string>();

         attrname.Add("lat");
         attrname.Add("lon");

         attrvalue.Add(XWrite(Lat));
         attrvalue.Add(XWrite(Lon));

         if (Elevation != NOTVALID_DOUBLE)
            subnodes += XWriteNode("ele", Elevation);

         if (Time != NOTVALID_TIME)
            subnodes += XWriteNode("time", Time);

         return subnodes;
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder(nodename + ":");
         if (Lat != NOTVALID_DOUBLE &&
             Lon != NOTVALID_DOUBLE)
            sb.AppendFormat(" lat={0} lon={1}", Lat, Lon);
         if (Elevation != NOTVALID_DOUBLE)
            sb.AppendFormat(" ele={0}", Elevation);
         if (Time != NOTVALID_TIME)
            sb.AppendFormat(" {0}", Time);
         return sb.ToString();
      }

   }

}
