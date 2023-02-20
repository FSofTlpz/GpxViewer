using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   public abstract class BaseElement {

      /*
       Ein bisher ungelöstes Problem sind bei den "unhandled Childs" interne Texte. Diese Texte könnten XML-Entitäten enthalten, die beim Einlesen "entschlüsselt"
       werden, beim Schreiben ohne weitere Analyse aber nicht wieder "verschlüsselt" werden.
       */


      /// <summary>
      /// ungültiger Zahlenwert
      /// </summary>
      public const double NOTVALID_DOUBLE = double.MinValue;       // double.NaN ist leider nicht brauchbar, da nur über die Funktion double.isNaN() ein Vergleich erfolgen kann

      /// <summary>
      /// Zahlenwert soll nicht berücksichtigt werden
      /// </summary>
      public const double NOTUSE_DOUBLE = double.MaxValue;

      /// <summary>
      /// ungültiger Datumswert
      /// </summary>
      public static DateTime NOTVALID_TIME {
         get {
            return DateTime.MinValue;
         }
      }

      /// <summary>
      /// Datumswert soll nicht berücksichtigt werden
      /// </summary>
      public static DateTime NOTUSE_TIME {
         get {
            return DateTime.MaxValue;
         }
      }

      /// <summary>
      /// XML-Texte der unbearbeiteten Childs und ihre Position
      /// </summary>
      public Dictionary<int, string> UnhandledChildXml { get; protected set; }


      public BaseElement(string xmltext = null, bool removenamespace = false) {
         Init();
         UnhandledChildXml = new Dictionary<int, string>();
         if (xmltext != null)
            FromXml(xmltext, removenamespace);
      }


      /// <summary>
      /// Properties init.
      /// </summary>
      protected abstract void Init();

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public abstract void FromXml(string xmltxt, bool removenamespace = false);

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public abstract string AsXml(int scale);

      public static bool ValueIsValid(double val) {
         return val != NOTVALID_DOUBLE;
      }

      public static bool ValueIsValid(DateTime val) {
         return val != NOTVALID_TIME;
      }

      public static bool ValueIsUsed(double val) {
         return val != NOTUSE_DOUBLE;
      }

      public static bool ValueIsUsed(DateTime val) {
         return val != NOTUSE_TIME;
      }

      public static string RemoveNamespace(string xmltxt) {
         if (!string.IsNullOrEmpty(xmltxt)) {
            MatchCollection matchCol = Regex.Matches(xmltxt, "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"");
            foreach (var match in matchCol)
               xmltxt = xmltxt.Replace(match.ToString(), "");
         }
         return xmltxt;

         // ev. einfacher, d.h. schneller (aber noch mit while!):
         //// xmlns="http://www.topografix.com/GPX/1/1"
         //int pos1 = xml.IndexOf("xmlns");
         //if (pos1 >= 0) {
         //   int pos2 = xml.IndexOf(">", pos1);
         //   xml = xml.Remove(pos1 - 1, pos2 - pos1 + 1);
         //}
         //return xml;
      }

      protected XPathNavigator GetNavigator4XmlText(string xmltxt) {
         XmlDocument doc = new XmlDocument();
         doc.LoadXml(xmltxt);
         return doc.CreateNavigator();
      }

      /// <summary>
      /// liefert den XML-Text für eine weitere Analyse
      /// </summary>
      /// <param name="navigator"></param>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static string[] XReadOuterXml(XPathNavigator navigator, string xpath) {
         string[] ret = null;
         XPathNodeIterator nodes = navigator.Select(xpath);
         if (nodes.Count == 0)
            return null;
         ret = new string[nodes.Count];
         int i = 0;
         while (nodes.MoveNext())
            ret[i++] = nodes.Current.OuterXml;
         return ret;
      }

      /// <summary>
      /// liefert das Ergebnis-Array zum XPath mit min. 1 Element oder null
      /// </summary>
      /// <param name="navigator"></param>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static object[] XRead(XPathNavigator navigator, string xpath) {
         object[] ret = null;
         XPathNodeIterator nodes = navigator.Select(xpath);
         if (nodes.Count == 0)
            return null;
         ret = new object[nodes.Count];
         int i = 0;
         while (nodes.MoveNext())
            ret[i++] = nodes.Current.TypedValue;
         return ret;
      }

      /// <summary>
      /// liefert das 1. Ergebnis eines Ergebnis-Array zum XPath als string
      /// </summary>
      /// <param name="navigator"></param>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static string XReadString(XPathNavigator navigator, string xpath) {
         object[] ret = XRead(navigator, xpath);
         return ret != null ? ret[0] as string : null;
      }

      /// <summary>
      /// liefert das 1. Ergebnis eines Ergebnis-Array zum XPath als double
      /// </summary>
      /// <param name="navigator"></param>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static double XReadDouble(XPathNavigator navigator, string xpath) {
         object[] ret = XRead(navigator, xpath);
         return ret != null ? Convert.ToDouble(ret[0], CultureInfo.InvariantCulture) : NOTVALID_DOUBLE;
      }

      /// <summary>
      /// liefert das 1. Ergebnis eines Ergebnis-Array zum XPath als DateTime
      /// </summary>
      /// <param name="navigator"></param>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static DateTime XReadDateTime(XPathNavigator navigator, string xpath) {
         object[] ret = XRead(navigator, xpath);
         return ret != null && ret.Length > 0 ? ReadDateTime(ret[0] as string) : NOTVALID_TIME;
      }

      protected static DateTime ReadDateTime(string txt) {
         DateTime dt = NOTVALID_TIME;
         if (txt != null)
            try {
               dt = DateTime.Parse(txt, null, DateTimeStyles.RoundtripKind);
            } catch { }
         return dt;
      }

      /// <summary>
      /// liefert den DateTime-Wert im Format für die GPX-Datei
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      protected static string XWrite(DateTime dt) {
         return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
      }

      /// <summary>
      /// liefert den double-Wert im Format für die GPX-Datei
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      protected static string XWrite(double v) {
         return v.ToString(CultureInfo.InvariantCulture);
      }

      /// <summary>
      /// enternt alle nicht erlaubten Entities aus dem Text
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      protected static string XmlClean(string txt) {
         // System.Security.SecurityElement.Escape(nodetxt)
         //    <   ->   &lt;
         //    >   ->   &gt;
         //    "   ->   &quot;
         //    '   ->   &apos;
         //    &   ->   &amp;
         return System.Security.SecurityElement.Escape(txt);
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="attrname">Liste der Attributnamen</param>
      /// <param name="attrvalue">Liste der Attributwerte</param>
      /// <param name="nodetxt">Node-Text (reiner Text darf keine Entities mehr enthalten!)</param>
      /// <returns></returns>
      protected static string XWriteNode(string nodename, IList<string> attrname, IList<string> attrvalue, string nodetxt = null) {
         int attrcount = attrname != null && attrvalue != null ? Math.Min(attrname.Count, attrvalue.Count) : 0;
         string node = "<" + nodename;
         for (int i = 0; i < attrcount; i++)
            node += " " + attrname[i] + "=\"" + XmlClean(attrvalue[i]) + "\"";
         if (string.IsNullOrEmpty(nodetxt))
            node += "/>";
         else
            node += ">" + nodetxt + "</" + nodename + ">";
         return node;
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes ohne Attribute
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="nodetxt">Node-Text</param>
      /// <returns></returns>
      protected static string XWriteNode(string nodename, string nodetxt) {
         return XWriteNode(nodename, null, null, nodetxt);
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes ohne Attribute
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="dt">Datum als Node-Text</param>
      /// <returns></returns>
      protected static string XWriteNode(string nodename, DateTime dt) {
         return XWriteNode(nodename, null, null, XWrite(dt));
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes ohne Attribute
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="v">double-Zahl als Node-Text</param>
      /// <returns></returns>
      protected static string XWriteNode(string nodename, double v) {
         return XWriteNode(nodename, null, null, XWrite(v));
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="nav"></param>
      /// <param name="xpath"></param>
      /// <param name="exceptions">Liste der Ausnahmen (mit Klammern!)</param>
      protected void RegisterUnhandledChild(XPathNavigator nav, string xpath, IList<string> exceptions) {
         string[] tmp = XReadOuterXml(nav, xpath);
         if (tmp != null)
            for (int i = 0; i < tmp.Length; i++) {
               bool isexception = false;
               if (exceptions != null)
                  foreach (string item in exceptions)
                     if (tmp[i].StartsWith(item)) {
                        isexception = true;
                        break;
                     }
               if (!isexception)
                  UnhandledChildXml.Add(i, tmp[i]);
            }
      }


   }

}
