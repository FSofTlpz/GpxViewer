using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Basisklasse für alle Arten von Punkten (lat/lon ist immer vorhanden)
   /// </summary>
   public abstract class GpxPointBase : BaseElement {

      /*
       https://www.topografix.com/GPX/1/1/#wptType 

      ACHTUNG!
      Alle Punktarten (Waypoints, Trackpoints, Routepoints) können die gleichen (!) Daten enthalten.
       
         <xsd:complexType name="wptType">
            <xsd:sequence>
               <-- elements must appear in this order -->
               <-- Position info -->
               <xsd:element name="ele" type="xsd:decimal" minOccurs="0"/>
               <xsd:element name="time" type="xsd:dateTime" minOccurs="0"/>
               <xsd:element name="magvar" type="degreesType" minOccurs="0"/>
               <xsd:element name="geoidheight" type="xsd:decimal" minOccurs="0"/>
               <-- Description info -->
               <xsd:element name="name" type="xsd:string" minOccurs="0"/>
               <xsd:element name="cmt" type="xsd:string" minOccurs="0"/>
               <xsd:element name="desc" type="xsd:string" minOccurs="0"/>
               <xsd:element name="src" type="xsd:string" minOccurs="0"/>
               <xsd:element name="link" type="linkType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="sym" type="xsd:string" minOccurs="0"/>
               <xsd:element name="type" type="xsd:string" minOccurs="0"/>
               <-- Accuracy info -->
               <xsd:element name="fix" type="fixType" minOccurs="0"/>
               <xsd:element name="sat" type="xsd:nonNegativeInteger" minOccurs="0"/>
               <xsd:element name="hdop" type="xsd:decimal" minOccurs="0"/>
               <xsd:element name="vdop" type="xsd:decimal" minOccurs="0"/>
               <xsd:element name="pdop" type="xsd:decimal" minOccurs="0"/>
               <xsd:element name="ageofdgpsdata" type="xsd:decimal" minOccurs="0"/>
               <xsd:element name="dgpsid" type="dgpsStationType" minOccurs="0"/>
               <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
            </xsd:sequence>
            <xsd:attribute name="lat" type="latitudeType" use="required"/>                      -90.0 <= value <= 90.0
            <xsd:attribute name="lon" type="longitudeType" use="required"/>                     -180.0 <= value < 180.0
         </xsd:complexType>

       */

      /// <summary>
      /// mögliche Childnodes (in DIESER Reihenfolge)
      /// </summary>
      protected static string[] definedChildnodeNames = {
         "<ele>",
         "<time>",
         "<magvar>",
         "<geoidheight>",
         "<name>",
         "<cmt>",
         "<desc>",
         "<src>",
         "<link>",  // mehrfach möglich
         "<sym>",
         "<type>",
         "<fix>",
         "<sat>",
         "<hdop>",
         "<vdop>",
         "<pdop>",
         "<ageofdgpsdata>",
         "<dgpsid>",
         "<extensions>",
      };

      // unbedingt nötig

      /// <summary>
      /// Latitude (Breite, y), -90°...90°
      /// </summary>
      public double Lat;

      /// <summary>
      /// Longitude (Länge, x), -180°...180°
      /// </summary>
      public double Lon;

      // häufig verwendet

      /// <summary>
      /// Höhe
      /// </summary>
      public double Elevation;

      /// <summary>
      /// Zeitpunkt
      /// </summary>
      public DateTime Time;


      public enum PointType {
         Waypoint = 0,
         Trackpoint = 1,
         Routepoint = 2,
         unknown,
      }


      protected GpxPointBase(string? xmltext = null, bool removenamespace = false) :
         base() {
         if (xmltext != null)
            FromXml(xmltext, removenamespace);
      }

      /// <summary>
      /// Basisdaten init.
      /// </summary>
      protected void baseInit() {
         Lat = Lon = Elevation = NOTVALID_DOUBLE;
         Time = NOTVALID_TIME;
      }

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text und alle noch nicht behandelten Childsnodes als <see cref="UnhandledChildXml"/>
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      /// <param name="pointtype"></param>
      protected void readDataFromXml(string xmltxt, bool removenamespace, PointType pointtype) {
         if (pointtype == PointType.unknown) {

            throw new ArgumentException("falscher " + nameof(pointtype));

         } else {

            List<(string, string)> attr = getAttributeCollection(xmltxt, removenamespace);
            for (int i = 0; i < attr.Count; i++) {
               if (attr[i].Item1 == "lat")
                  Lat = Convert.ToDouble(attr[i].Item2, CultureInfo.InvariantCulture);
               else if (attr[i].Item1 == "lon")
                  Lon = Convert.ToDouble(attr[i].Item2, CultureInfo.InvariantCulture);
            }

            UnhandledChildXml = getChildCollection(xmltxt, removenamespace);  // alle Childs erstmal als UnhandledChildXml registrieren

            if (UnhandledChildXml != null) {
               if (UnhandledChildXml.Count > 0) {
                  int found = 0;
                  int maxfound = 2 + getExtChildCount();
                  for (int i = 0; i < UnhandledChildXml.Count && found < maxfound; i++) {
                     if (checkChilds(i)) {
                        found++;
                        i--;
                     }
                  }
               }
               if (UnhandledChildXml.Count == 0)
                  UnhandledChildXml = null;        // wird nicht mehr benötigt
            }
         }
      }

      /// <summary>
      /// liest die Standard-Properties ein
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      protected bool checkChilds(int idx) {
         bool getit = false;
         string childtxt = UnhandledChildXml != null ? UnhandledChildXml[idx] : string.Empty;
         if (getDouble4ChildXml(childtxt, "<ele>", out double v)) {
            Elevation = v;
            getit = true;
         } else if (getDateTime4ChildXml(childtxt, "<time>", out DateTime dt)) {
            Time = dt;
            getit = true;
         } else if (checkExtChilds(childtxt))
            getit = true;

         if (getit && UnhandledChildXml != null)
            UnhandledChildXml.RemoveAt(idx);
         return getit;
      }

      /// <summary>
      /// testet das zusätzliche Child
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="childtxt"></param>
      /// <returns></returns>
      protected abstract bool checkExtChilds(string childtxt);

      /// <summary>
      /// liefert die Anzahl der zusätzlichen Childs
      /// </summary>
      /// <returns></returns>
      protected abstract int getExtChildCount();

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         StringBuilder sb = new StringBuilder(getStandardXmlNodeData(out List<string> attrname, out List<string> attrvalue));
         sb.Append(collectAllChilds(definedChildnodeNames, getChildTxt4Props(scale), UnhandledChildXml, scale));
         return xWriteNode(getNodename(), attrname, attrvalue, sb.ToString());
      }

      /// <summary>
      /// schreibt den vollständigen XML-Text für das Objekt in den StringBuilder
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="scale"></param>
      public void AsXml(StringBuilder sb, int scale = int.MaxValue) {
         StringBuilder sbtmp = new StringBuilder(getStandardXmlNodeData(out List<string> attrname, out List<string> attrvalue));
         sbtmp.Append(collectAllChilds(definedChildnodeNames, getChildTxt4Props(scale), UnhandledChildXml, scale));
         xWriteNode(sbtmp, getNodename(), attrname, attrvalue);
         sb.Append(sbtmp);
         sbtmp.Clear();
      }

      /// <summary>
      /// liefert den akt. Nodenamen des Objektes
      /// </summary>
      /// <returns></returns>
      protected abstract string getNodename();

      /// <summary>
      /// liefert die Atribut-Daten für die Erzeugung Point-XML-Nodes und den Text für die ersten 2 Childs (ele, time)
      /// </summary>
      /// <param name="attrname">Liste der Attributnamen</param>
      /// <param name="attrvalue">Liste der Attributwerte</param>
      /// <returns>vollständiger Text der Subnodes</returns>
      protected string getStandardXmlNodeData(out List<string> attrname, out List<string> attrvalue) {
         attrname = new List<string>();
         attrvalue = new List<string>();

         attrname.Add("lat");
         attrname.Add("lon");

         attrvalue.Add(xWriteText(Lat));
         attrvalue.Add(xWriteText(Lon));

         string subnodes = string.Empty;
         if (Elevation != NOTVALID_DOUBLE)
            subnodes += xWriteNode("ele", Elevation);

         if (Time != NOTVALID_TIME)
            subnodes += xWriteNode("time", Time);

         return subnodes;
      }

      #endregion

      public override string ToString() {
         StringBuilder sb = new StringBuilder(getNodename() + ":");
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
