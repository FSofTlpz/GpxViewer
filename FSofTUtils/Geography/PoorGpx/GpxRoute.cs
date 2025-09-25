using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Daten einer Route
   /// </summary>
   public class GpxRoute : BaseElement {

      /*
       https://www.topografix.com/GPX/1/1/#type_trkType 

         <xsd:complexType name="rteType">
            <xsd:sequence>
               <xsd:element name="name" type="xsd:string" minOccurs="0"/>
               <xsd:element name="cmt" type="xsd:string" minOccurs="0"/>
               <xsd:element name="desc" type="xsd:string" minOccurs="0"/>
               <xsd:element name="src" type="xsd:string" minOccurs="0"/>
               <xsd:element name="link" type="linkType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="number" type="xsd:nonNegativeInteger" minOccurs="0"/>
               <xsd:element name="type" type="xsd:string" minOccurs="0"/>
               <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
               <xsd:element name="rtept" type="wptType" minOccurs="0" maxOccurs="unbounded"/>
            </xsd:sequence>
         </xsd:complexType>       
       
       
       */

      /// <summary>
      /// mögliche Childnodes (in DIESER Reihenfolge)
      /// </summary>
      protected static string[] definedChildnodeNames = {
         "<name>",
         "<cmt>",
         "<desc>",
         "<src>",
         "<link>",  // mehrfach möglich
         "<number>",
         "<type>",
         "<type>",
         "<extensions>",
         "<rtept>",  // mehrfach möglich
      };


      public const string NODENAME = "rte";

      public ListTS<GpxRoutePoint> Points = new ListTS<GpxRoutePoint>();

      public string Name = string.Empty;

      public string Comment = string.Empty;

      public string Description = string.Empty;

      public string Source = string.Empty;

      protected static string[] unhandledChildExceptions = {
         "<name>",
         "<cmt>",
         "<desc>",
         "<src>",
         "<" + GpxRoutePoint.NODENAME + " ",
      };

      protected static string nodename4point = "<" + GpxRoutePoint.NODENAME + " ";


      public GpxRoute(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxRoute(GpxRoute r) : base() {
         Name = r.Name;
         Points = new ListTS<GpxRoutePoint>(r.Points.Count);
         for (int p = 0; p < r.Points.Count; p++)
            Points.Add(new GpxRoutePoint(r.Points[p]));
      }

      protected override void Init() {
         Points.Clear();
      }

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();

         UnhandledChildXml = getChildCollection(xmltxt, removenamespace);  // alle Childs erstmal als UnhandledChildXml registrieren
         if (UnhandledChildXml != null &&
             UnhandledChildXml.Count > 0) {
            int max = UnhandledChildXml.Count;
            for (int i = 0; i < max; i++) {
               if (checkChilds(i))
                  i--;
            }
            if (UnhandledChildXml.Count == 0)
               UnhandledChildXml = null;        // wird nicht mehr benötigt
         }
      }

      protected bool checkChilds(int idx) {
         bool getit = false;
         string childtxt = UnhandledChildXml != null ? UnhandledChildXml[idx] : string.Empty;
         string? tmp;
         if (childtxt.StartsWith(nodename4point)) {
            Points.Add(new GpxRoutePoint(childtxt));
            getit = true;
         } else if (getString4ChildXml(childtxt, "<name>", out tmp)) {
            Name = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<cmt>", out tmp)) {
            Comment = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<desc>", out tmp)) {
            Description = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (getString4ChildXml(childtxt, "<src>", out tmp)) {
            Source = tmp != null ? tmp : string.Empty;
            getit = true;
         }

         if (getit && UnhandledChildXml != null)
            UnhandledChildXml.RemoveAt(idx);
         return getit;
      }

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         return xWriteNode(NODENAME, asxml(scale).ToString());
      }

      public void AsXml(StringBuilder sb, int scale = int.MaxValue) {
         StringBuilder sbtmp = asxml(scale);
         xWriteNode(sbtmp, NODENAME);
         sb.Append(sbtmp);
      }

      StringBuilder asxml(int scale) {
         StringBuilder sb = collectAllChilds(definedChildnodeNames, getChildTxt4Props(scale), UnhandledChildXml, scale);
         for (int p = 0; p < Points.Count; p++)
            sb.Append(Points[p].AsXml(scale));
         return sb;
      }

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
         if (!string.IsNullOrEmpty(Source) && scale > 0)
            childtxt.Add(xWriteNode("src", XmlEncode(Source)));
         return childtxt;
      }

      #endregion

      /// <summary>
      /// liefert den <see cref="GpxRoutePoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public GpxRoutePoint? GetPoint(int p) {
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
