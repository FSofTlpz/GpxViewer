using System;
using System.Collections.Generic;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Trackpointdaten
   /// </summary>
   public class GpxTrackPoint : GpxPointBase {

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

      public const string NODENAME = "trkpt";

 
      public GpxTrackPoint(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxTrackPoint(GpxTrackPoint p) : base(NODENAME) {
         Lat = p.Lat;
         Lon = p.Lon;
         Elevation = p.Elevation;
         Time = p.Time;
      }

      public GpxTrackPoint(double lon, double lat, double ele = NOTVALID_DOUBLE) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
      }

      public GpxTrackPoint(double lon, double lat, double ele, DateTime time) : this() {
         Lon = lon;
         Lat = lat;
         Elevation = ele;
         Time = time;
      }

      protected override void Init() => baseInit();

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         readDataFromXml(xmltxt, removenamespace, PointType.Trackpoint);
      }

      protected override bool checkExtChilds(string childtxt) => false;

      protected override int getExtChildCount() => 0;

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert alle Childtexte für die Properties dieser Klasse
      /// </summary>
      /// <param name="scale"></param>
      /// <returns></returns>
      protected override List<string>? getChildTxt4Props(int scale) => null;

      protected override string getNodename() => NODENAME;

      #endregion

      public override string ToString() {
         return base.ToString();
      }

   }

}
