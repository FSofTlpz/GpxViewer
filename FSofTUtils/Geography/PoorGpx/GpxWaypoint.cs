using System;
using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Waypointdaten
   /// </summary>
   public class GpxWaypoint : GpxPointBase {

      /*
       https://www.topografix.com/GPX/1/1/#wptType 
       
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
            <xsd:attribute name="lat" type="latitudeType" use="required"/>
            <xsd:attribute name="lon" type="longitudeType" use="required"/>
         </xsd:complexType>

       */

      public const string NODENAME = "wpt";

      // häufig verwendet

      public string Name = string.Empty;

      public string Comment = string.Empty;

      public string Description = string.Empty;

      public string Symbol = string.Empty;


      public GpxWaypoint(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxWaypoint(GpxWaypoint p) : base(NODENAME) {
         Lat = p.Lat;
         Lon = p.Lon;
         Elevation = p.Elevation;
         Time = p.Time;
         Name = p.Name;
         Comment = p.Comment;
         Description = p.Description;
         Symbol = p.Symbol;
      }

      public GpxWaypoint(double lon, double lat, double ele = NOTVALID_DOUBLE) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
      }

      public GpxWaypoint(double lon, double lat, double ele, DateTime time) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
         Time = time;
      }

      protected override void Init() {
         baseInit();
         Name = Comment = Description = Symbol = string.Empty;
      }

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         readDataFromXml(xmltxt, false, PointType.Waypoint);
      }

      protected override bool checkExtChilds(string childtxt) {
         bool getit = false;
         string? tmp;
         if (getString4ChildXml(childtxt, "<name>", out tmp)) {
            Name = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<cmt>", out tmp)) {
            Comment = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<desc>", out tmp)) {
            Description = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<sym>", out tmp)) {
            Symbol = tmp != null ? tmp : string.Empty;
            getit = true;
         }
         return getit;
      }

      protected override int getExtChildCount() => 4;

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert alle Childtexte für die Properties
      /// </summary>
      /// <param name="scale"></param>
      /// <returns></returns>
      protected override List<string> getChildTxt4Props(int scale) {
         List<string> childtxt = new List<string>();
         if (!string.IsNullOrEmpty(Name))
            childtxt.Add(xWriteNode("name", XmlEncode(Name)));
         if (!string.IsNullOrEmpty(Comment) && scale > 0)
            childtxt.Add(xWriteNode("cmt", XmlEncode(Comment)));
         if (!string.IsNullOrEmpty(Description) && scale > 0)
            childtxt.Add(xWriteNode("desc", XmlEncode(Description)));
         if (!string.IsNullOrEmpty(Symbol) && scale > 0)
            childtxt.Add(xWriteNode("sym", XmlEncode(Symbol)));
         return childtxt;
      }

      protected override string getNodename() => NODENAME;

      #endregion

      public override string ToString() {
         StringBuilder sb = new StringBuilder(base.ToString());
         if (!string.IsNullOrEmpty(Name))
            sb.AppendFormat(" name=[{0}]", Name);
         if (!string.IsNullOrEmpty(Comment))
            sb.AppendFormat(" cmt={0}", Comment);
         if (!string.IsNullOrEmpty(Description))
            sb.AppendFormat(" desc={0}", Description);
         if (!string.IsNullOrEmpty(Symbol))
            sb.AppendFormat(" sym={0}", Symbol);
         return sb.ToString();
      }

   }

}
