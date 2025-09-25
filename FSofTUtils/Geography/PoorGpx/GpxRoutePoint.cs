using System;
using System.Collections.Generic;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Routenpunktdaten
   /// </summary>
   public class GpxRoutePoint : GpxPointBase {

      public const string NODENAME = "rtept";

      public string Name = string.Empty;

      public string Comment = string.Empty;

      public string Description = string.Empty;

      public string Symbol = string.Empty;


      public GpxRoutePoint(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

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

      protected override void Init() => baseInit();

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         readDataFromXml(xmltxt, removenamespace, PointType.Routepoint);
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
         return base.ToString();
      }

   }

}
