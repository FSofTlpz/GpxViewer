using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
      public static DateTime NOTVALID_TIME => DateTime.MinValue;

      /// <summary>
      /// Datumswert soll nicht berücksichtigt werden
      /// </summary>
      public static DateTime NOTUSE_TIME => DateTime.MaxValue;

      /// <summary>
      /// XML-Texte der unbearbeiteten Childs
      /// </summary>
      public List<string>? UnhandledChildXml { get; protected set; }


      public BaseElement(string? xmltext = null, bool removenamespace = false) {
         Init();
         UnhandledChildXml = new List<string>();
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

      public static bool ValueIsValid(double val) => val != NOTVALID_DOUBLE;

      public static bool ValueIsValid(DateTime val) => val != NOTVALID_TIME;

      public static bool ValueIsUsed(double val) => val != NOTUSE_DOUBLE;

      public static bool ValueIsUsed(DateTime val) => val != NOTUSE_TIME;

      /// <summary>
      /// geograf. Länge -180° .. 180°
      /// </summary>
      /// <param name="lat"></param>
      /// <returns></returns>
      public static double GetNormedLongitude(double lat) {
         while (lat < -180)
            lat += 360;
         while (180 < lat)
            lat -= 360;
         return lat;
      }

      /// <summary>
      /// geograf. Breite -90° .. 90°
      /// </summary>
      /// <param name="lon"></param>
      /// <returns></returns>
      public static double GetNormedLatitude(double lon) {
         while (lon < -90)
            lon += 180;
         while (90 < lon)
            lon -= 180;
         return lon;
      }

      /// <summary>
      /// (selten) notwendig wenn "von außen" eine solche Liste erzeugt werden muss
      /// </summary>
      public void CreateUnhandledChildXmlList() =>
         UnhandledChildXml = new List<string>();

      /// <summary>
      /// decodiert nicht erlaubten Entities eines XML-Textes
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public static string XmlDecode(string txt) => System.Net.WebUtility.HtmlDecode(txt);

      /// <summary>
      /// enternt alle nicht erlaubten Entities aus dem Text
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public static string XmlEncode(string txt) {
         // System.Security.SecurityElement.Escape(nodetxt)
         //    <   ->   &lt;
         //    >   ->   &gt;
         //    "   ->   &quot;
         //    '   ->   &apos;
         //    &   ->   &amp;
         return System.Security.SecurityElement.Escape(txt);
      }

      #region read XML

      /// <summary>
      /// liefert den Wert aus <paramref name="childxml"/> (in der Art "&lt;abc&gt;text&lt;/&lt;abc&gt;") als Text
      /// </summary>
      /// <param name="childxml"></param>
      /// <param name="nodename">gesuchter Nodename</param>
      /// <param name="v">null wenn <paramref name="nodename"/> icht passt</param>
      /// <returns>true wenn der <paramref name="nodename"/> stimmt</returns>
      protected static bool getString4ChildXml(string childxml, string nodename, out string? v) {
         v = null;
         if (childxml.StartsWith(nodename)) {
            v = XmlDecode(childxml.Substring(nodename.Length, childxml.Length - (2 * nodename.Length + 1)).Trim());
            return true;
         }
         return false;
      }

      /// <summary>
      /// liefert den Wert aus <paramref name="childxml"/> (in der Art "&lt;abc&gt;text&lt;/&lt;abc&gt;") als double-Zahl
      /// </summary>
      /// <param name="childxml"></param>
      /// <param name="nodename">gesuchter Nodename</param>
      /// <param name="v">null wenn <paramref name="nodename"/> icht passt</param>
      /// <returns>true wenn der <paramref name="nodename"/> stimmt</returns>
      protected static bool getDouble4ChildXml(string childxml, string nodename, out double v) {
         v = NOTVALID_DOUBLE;
         if (getString4ChildXml(childxml, nodename, out string? txt)) {
            if (txt != null && txt.Length > 0)
               v = Convert.ToDouble(txt, System.Globalization.CultureInfo.InvariantCulture);
            return true;
         }
         return false;
      }

      /// <summary>
      /// liefert den Wert aus <paramref name="childxml"/> (in der Art "&lt;abc&gt;text&lt;/&lt;abc&gt;") als DateTime
      /// </summary>
      /// <param name="childxml"></param>
      /// <param name="nodename">gesuchter Nodename</param>
      /// <param name="v">null wenn <paramref name="nodename"/> icht passt</param>
      /// <returns>true wenn der <paramref name="nodename"/> stimmt</returns>
      protected static bool getDateTime4ChildXml(string childxml, string nodename, out DateTime v) {
         v = NOTVALID_TIME;
         if (getString4ChildXml(childxml, nodename, out string? txt)) {
            if (txt != null && txt.Length > 0)
               v = xDateTime(txt);
            return true;
         }
         return false;
      }

      /// <summary>
      /// liefert den Text für alle angegebenen Childnode-Texte
      /// </summary>
      /// <param name="definedChildname">(in dieser Reihenfolge) definierte (mögliche) Childnodes</param>
      /// <param name="handledChildTxt">Texte für schon behandelte Childs</param>
      /// <param name="unhandledChildTxt">Texte für nicht behandelte Childs</param>
      /// <param name="scale"></param>
      /// <returns></returns>
      protected static StringBuilder collectAllChilds(IList<string> definedChildname,
                                                      IList<string>? handledChildTxt,
                                                      IList<string>? unhandledChildTxt,
                                                      int scale) {
         StringBuilder sb = new StringBuilder();

         if (definedChildname.Count > 0 &&
             ((handledChildTxt != null ? handledChildTxt.Count : 0) +               // es gibt schon behandelte Childs
              (unhandledChildTxt != null ? unhandledChildTxt.Count : 0) > 0)) {     // es gibt unbehandelte Childs
            foreach (var childname in definedChildname) {
               bool found = false;

               if (handledChildTxt != null &&
                   handledChildTxt.Count > 0)
                  // alle (nacheinanderfolgenden !) Childs mit dem passenden Namen werden ausgegeben
                  foreach (var childtxt in handledChildTxt) {
                     if (childtxt.StartsWith(childname)) {
                        sb.Append(childtxt);
                        found = true;
                     } else {
                        if (found)
                           break;
                     }
                  }
               if (found)
                  continue;

               if (unhandledChildTxt != null &&
                   unhandledChildTxt.Count > 0) {
                  // alle (nacheinanderfolgenden !) Childs mit dem passenden Namen werden ausgegeben
                  foreach (var childtxt in unhandledChildTxt) {
                     if (childtxt.StartsWith(childname)) {
                        sb.Append(childtxt);
                        found = true;
                     } else {
                        if (found)
                           break;
                     }
                  }
               }
            }
         }
         return sb;
      }

      /// <summary>
      /// liefert alle Childtexte für die Properties der Klasse
      /// </summary>
      /// <param name="scale"></param>
      /// <returns></returns>
      protected virtual List<string>? getChildTxt4Props(int scale) => null;

      /*
      <nodename>
         <childnode1> .... </childnode1> 
         <childnode2 /> 

         <childnoden> .... </childnoden>  
      </nodename>


       */

      /// <summary>
      /// "überliest" das erste vorhandene XML-Tag ("Nodename") und liefert alle Pseudochilds
      /// <para>Bei den Pseudochilds wird NICHT getestet ob öffnendes und schließendes XML-Tag den gleichen Namen haben!</para>
      /// </summary>
      /// <param name="xml"></param>
      /// <param name="removenamespace">bei true werden die Namespaceangaben vor den eigentlichen Childnodenamen entfernt</param>
      /// <returns></returns>
      public static List<string> getChildCollection(string xml, bool removenamespace = false) {
         List<string> childs = new List<string>();

         if (xml != null) {
            int p = xml.IndexOf('>');     // akt. Nodename übersprungen
            if (p > 0) {
               if (xml[p - 1] != '/') {   // sonst keine Childnodes vorhanden (<nodename .../>)
                  int tagpairstart;
                  p++;
                  do {
                     tagpairstart = findNextXmlTagPair(xml, p, out int tagpairlength);
                     if (tagpairstart >= 0) {
                        if (removenamespace) {
                           string child = xml.Substring(tagpairstart, tagpairlength);

                           // Starttag:
                           int colon = child.IndexOf(':');
                           if (colon >= 0) {
                              int endpos = child.IndexOf('>');
                              int space = child.IndexOf(' ');
                              if (0 <= space && space < endpos)   // es folgen noch Attribute
                                 endpos = space;
                              if (colon < endpos)                 // ":" im Tagnamen
                                 child = child.Remove(1, colon);
                           }
                           // Endtag:
                           colon = child.LastIndexOf(':');
                           if (colon >= 0) {
                              int slash = child.LastIndexOf('/');
                              if (slash < colon)
                                 child = child.Remove(slash + 1, colon - slash);
                           }

                           childs.Add(child);
                        } else
                           childs.Add(xml.Substring(tagpairstart, tagpairlength));
                        p = tagpairstart + tagpairlength;
                     }
                  } while (tagpairstart >= 0);
               }
            }
         }

         return childs;
      }

      /// <summary>
      /// liefert das erste vollständige Tag (einschließlich &lt; und &gt;)
      /// </summary>
      /// <param name="xml"></param>
      /// <param name="removenamespace"></param>
      /// <returns></returns>
      protected static string? getFirstXmlTag(string xml) {
         int startpos = findNextXmlTag(xml, 0, out int taglength, out bool _, out bool _);
         if (startpos >= 0)
            return xml.Substring(startpos, taglength);
         return null;
      }

      /// <summary>
      /// sucht das nächste XML-Tag und analysiert es
      /// </summary>
      /// <param name="xml">(min.) das vollständige Tag</param>
      /// <param name="startpos">Startpos. der Suche</param>
      /// <param name="taglength">gesamte Länge des Tags (einschließlich &lt; und &gt;)</param>
      /// <param name="isstarttag">ist Starttag</param>
      /// <param name="isendtag">ist Endtag</param>
      /// <returns>Startpos. (&lt;) des gefundenen Tags (oder -1)</returns>
      static int findNextXmlTag(string xml, int startpos, out int taglength, out bool isstarttag, out bool isendtag) {
         taglength = 0;
         isstarttag = isendtag = false;
         startpos = xml.IndexOf('<', startpos);
         if (startpos >= 0) {
            int endpos = xml.IndexOf('>', startpos + 1);
            if (endpos >= 0) {
               taglength = 1 + endpos - startpos;
               if (xml[startpos + 1] == '/') {  // </...>
                  isendtag = true;
               } else {
                  isstarttag = true;            // <...>
                  if (xml[endpos - 1] == '/') { // <.../>
                     isendtag = true;
                  }
               }
            }
         }
         return startpos;
      }

      static int findNextXmlTagPair(string xml, int startpos, out int tagpairlength) {
         int p = startpos;
         int level = 0;
         int tagpairstart = -1;
         tagpairlength = 0;

         do {
            int tagstart = findNextXmlTag(xml, p, out int taglength, out bool isstarttag, out bool isendtag);
            if (tagstart < 0 || taglength <= 0) {  // (Fehler)
               break;
            } else {
               if (tagpairstart < 0)
                  tagpairstart = tagstart;

               if (isstarttag)
                  level++;
               if (isendtag) {
                  if (level > 0)
                     level--;
                  else
                     tagpairstart = -1;
               }

               p = tagstart + taglength;
            }
         } while (level > 0);
         if (tagpairstart >= 0)
            tagpairlength = p - tagpairstart;
         return tagpairstart;
      }

      /// <summary>
      /// liefert alle Attribute mit ihren Texten aus einem (Start-)Tag
      /// </summary>
      /// <param name="xml">gesamtes (Start-)Tag einschließlich der Klammern (führende Leerzeichen sind erlaubt)</param>
      /// <param name="removenamespace">bei true werden die Namespaceangaben vor den eigentlichen Attributnamen entfernt</param>
      /// <returns></returns>
      protected static List<(string, string)> getAttributeCollection(string xml, bool removenamespace = false) {
         List<(string, string)> attr = new List<(string, string)>();

         if (xml != null && xml.Length > 2) {
            int p = xml.IndexOf('<');
            if (p >= 0) {
               int p2 = xml.IndexOf('>');
               if (p2 > p) {
                  if (xml[p + 1] != '/') {
                     while (xml[p] == ' ') p++;
                     p = xml.IndexOfAny([' ', '>']);
                     if (xml[p] == ' ') {
                        while (true) {
                           p = findNextAttribute(xml, p, p2 - 1, out string? attribute, out string? value, out _);
                           if (string.IsNullOrEmpty(attribute) ||
                               value == null)
                              break;
                           if (removenamespace) {
                              int colon = attribute.IndexOf(':');
                              if (colon >= 0)
                                 attribute = attribute.Substring(colon + 1);
                           }
                           attr.Add((attribute, value));
                        }
                     }
                  }
               }
            }
         }

         return attr;
      }

      /// <summary>
      /// überliest führende Leerzeichen und sammelt dann wenn möglich ein Paar 'attributname="value"' ein
      /// </summary>
      /// <param name="xml"></param>
      /// <param name="startpos"></param>
      /// <param name="endpos"></param>
      /// <param name="attribute"></param>
      /// <param name="value"></param>
      /// <param name="attributestart"></param>
      /// <returns>Index ab dem weiter gesucht werden kann</returns>
      static int findNextAttribute(string xml, 
                                   int startpos, 
                                   int endpos, 
                                   out string? attribute, 
                                   out string? value, 
                                   out int attributestart) {
         attribute = null;
         value = null;
         int p = startpos;

         while (p <= endpos &&
                xml[p] == ' ') p++;
         attributestart = p;

         while (p <= endpos &&
                xml[p] != '=' &&
                xml[p] != ' ') p++;

         if (p > 1 + startpos) {
            attribute = xml.Substring(attributestart, p - startpos - 1);

            while (p <= endpos &&
                   xml[p] == ' ') p++;

            if (p <= endpos &&
                xml[p] == '=') {
               p++;

               while (p <= endpos &&
                      xml[p] == ' ') p++;

               if (p <= endpos &&
                   xml[p] == '"') {
                  p++;
                  int valuestart = p;

                  while (p <= endpos &&
                         !(xml[p] == '"' &&
                           xml[p - 1] != '\\')) p++;
                  value = xml.Substring(valuestart, p - valuestart);
                  p++;
               }
            }
         }
         return p;
      }

      protected static DateTime xDateTime(string txt) {
         DateTime dt = NOTVALID_TIME;
         if (txt != null)
            try {
               dt = DateTime.Parse(txt, null, DateTimeStyles.RoundtripKind);
            } catch { }
         return dt;
      }

      #endregion

      #region write XML

      /// <summary>
      /// liefert den DateTime-Wert im Format für die GPX-Datei
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      protected static string xWriteText(DateTime dt) => dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");

      /// <summary>
      /// liefert den double-Wert im Format für die GPX-Datei
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      protected static string xWriteText(double v) => v.ToString("g8", CultureInfo.InvariantCulture);    // NICHT Exp.; max. 8 Nachkommastellen

      /// <summary>
      /// liefert den XML-Text für die Attribute (mit führendem Leerzeichen)
      /// </summary>
      /// <param name="attrname"></param>
      /// <param name="attrvalue"></param>
      /// <returns>leere Zeichenkette wenn keine Attribute gegeben sind</returns>
      protected static string xWriteAttr(IList<string>? attrname,
                                         IList<string>? attrvalue) {
         string node = string.Empty;
         int attrcount = attrname != null && attrvalue != null ?
                              Math.Min(attrname.Count, attrvalue.Count) :
                              0;
         for (int i = 0; i < attrcount; i++)
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
            node += " " + attrname[i] + "=\"" + XmlEncode(attrvalue[i]) + "\"";
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         return node;
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="attrname">Liste der Attributnamen</param>
      /// <param name="attrvalue">Liste der Attributwerte</param>
      /// <param name="nodetxt">Node-Text (reiner Text darf keine Entities mehr enthalten!)</param>
      /// <returns></returns>
      protected static string xWriteNode(string nodename,
                                         IList<string>? attrname,
                                         IList<string>? attrvalue,
                                         string? nodetxt = null) {
         string node = "<" + nodename + xWriteAttr(attrname, attrvalue);

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
      protected static string xWriteNode(string nodename, string nodetxt) => xWriteNode(nodename, null, null, nodetxt);

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes für eine DateTime ohne Attribute
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="dt">Datum als Node-Text</param>
      /// <returns></returns>
      protected static string xWriteNode(string nodename, DateTime dt) => xWriteNode(nodename, null, null, xWriteText(dt));

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes für eine double-Zahl ohne Attribute
      /// </summary>
      /// <param name="nodename">Node-Name</param>
      /// <param name="v">double-Zahl als Node-Text</param>
      /// <returns></returns>
      protected static string xWriteNode(string nodename, double v) => xWriteNode(nodename, null, null, xWriteText(v));

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes im StringBuilder
      /// <para>
      /// I.A. ist <paramref name="sb_is_nodetext"/> true, d.h. der im Stringbuilder vorhandene Text ist der Nodetext des neuen Nodes. 
      /// Bei false wird der neue Node an den bisherigen Text angehängt.
      /// </para>
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="nodename">Node-Name</param>
      /// <param name="attrname">Liste der Attributnamen</param>
      /// <param name="attrvalue">Liste der Attributwerte</param>
      /// <param name="sb_is_nodetext"><paramref name="sb"/>enthält den Node-Text (reiner Text darf keine Entities mehr enthalten!)</param>
      protected static void xWriteNode(StringBuilder sb,
                                       string nodename,
                                       IList<string>? attrname,
                                       IList<string>? attrvalue,
                                       bool sb_is_nodetext = true) {
         string attr = xWriteAttr(attrname, attrvalue);

         if (sb.Length == 0)     // kein Nodetext vorhanden
            sb.Append("<" + nodename + attr + "/>");
         else {
            if (sb_is_nodetext)  // Text im Stringbuilder ist der Nodetext
               sb.Insert(0, "<" + nodename + attr + ">");
            else                 // Text im Stringbuilder ist KEIN Nodetext; der neue Node soll angehängt werden
               sb.Append("<" + nodename + attr + ">");
            sb.Append("</" + nodename + ">");
         }
      }

      /// <summary>
      /// liefert den vollständigen Text eines XML-Nodes ohne Attribute
      /// <para>
      /// I.A. ist <paramref name="sb_is_nodetext"/> true, d.h. der im Stringbuilder vorhandene Text ist der Nodetext des neuen Nodes. 
      /// Bei false wird der neue Node an den bisherigen Text angehängt.
      /// </para>
      /// </summary>
      /// <param name="sb_with_nodetext"></param>
      /// <param name="nodename">Node-Name</param>
      /// <param name="sb_is_nodetext"><paramref name="sb_with_nodetext"/> enthält den Node-Text (reiner Text darf keine Entities mehr enthalten!)</param>
      /// <returns></returns>
      protected static void xWriteNode(StringBuilder sb_with_nodetext,
                                       string nodename) =>
         xWriteNode(sb_with_nodetext, nodename, null, null);

      #endregion

   }

}
