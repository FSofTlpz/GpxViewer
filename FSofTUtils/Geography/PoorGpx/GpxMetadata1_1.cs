using System;
using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Metadaten für Version 1.1
   /// </summary>
   public class GpxMetadata1_1 : BaseElement {


      /* https://www.topografix.com/GPX/1/1/#type_metadataType
       
         <xsd:complexType name="metadataType">
            <xsd:sequence>
               <-- elements must appear in this order -->
               <xsd:element name="name" type="xsd:string" minOccurs="0"/>
               <xsd:element name="desc" type="xsd:string" minOccurs="0"/>
               <xsd:element name="author" type="personType" minOccurs="0"/>
               <xsd:element name="copyright" type="copyrightType" minOccurs="0"/>
               <xsd:element name="link" type="linkType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="time" type="xsd:dateTime" minOccurs="0"/>
               <xsd:element name="keywords" type="xsd:string" minOccurs="0"/>
               <xsd:element name="bounds" type="boundsType" minOccurs="0"/>
               <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
            </xsd:sequence>
         </xsd:complexType>
      */

      /// <summary>
      /// mögliche Childnodes (in DIESER Reihenfolge)
      /// </summary>
      protected static string[] definedChildnodeNames = {
         "<name>",
         "<desc>",
         "<author>",
         "<copyright>",
         "<link>",      // mehrfach möglich
         "<time>",
         "<keywords>",
         "<bounds ",       // es folgen Attribute
         "<extensions>",
         "<extensions ",
      };


      public const string NODENAME = "metadata";

      /// <summary>
      /// Zeitpunkt
      /// </summary>
      public DateTime Time;

      public GpxBounds? Bounds;


      public GpxMetadata1_1(string? xmltext = null, bool removenamespace = false) :
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

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();

         UnhandledChildXml = getChildCollection(xmltxt, removenamespace);  // alle Childs erstmal als UnhandledChildXml registrieren

         if (UnhandledChildXml != null) {
            if (UnhandledChildXml.Count > 0) {
               for (int i = UnhandledChildXml.Count - 1; i >= 0; i--) {
                  string childtxt = UnhandledChildXml[i];
                  string? tag = getFirstXmlTag(childtxt);

                  if (tag != null) {
                     bool getit = false;

                     if (tag.StartsWith("<bounds ")) {
                        Bounds = new GpxBounds(childtxt);
                        getit = true;
                     } else if (getDateTime4ChildXml(childtxt, "<time>", out DateTime dt)) {
                        Time = dt;
                        getit = true;
                     }

                     if (getit)
                        UnhandledChildXml.RemoveAt(i);
                  }
               }
            }
            if (UnhandledChildXml.Count == 0)
               UnhandledChildXml = null;        // wird nicht mehr benötigt
         }
      }

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale = int.MaxValue) =>
         xWriteNode(NODENAME,
                    collectAllChilds(definedChildnodeNames, getChildTxt4Props(scale), UnhandledChildXml, scale).ToString());

      /// <summary>
      /// hängt den vollständigen XML-Text für das Objekt an den StringBuilder an
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="scale">Umfang der Ausgabe</param>
      public void AsXml(StringBuilder sb, int scale = int.MaxValue) =>
         sb.Append(AsXml(scale));

      /// <summary>
      /// liefert alle Childtexte für die Properties der Klasse
      /// </summary>
      /// <param name="scale"></param>
      /// <returns></returns>
      protected override List<string> getChildTxt4Props(int scale) {
         List<string> childtxt = new List<string>();
         if (Time != NOTVALID_TIME)
            childtxt.Add(xWriteNode("time", Time));
         if (Bounds != null && Bounds.IsValid())
            childtxt.Add(Bounds.AsXml(scale));
         return childtxt;
      }

      #endregion

      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (Time != NOTVALID_TIME)
            sb.AppendFormat(" {0}", Time);
         if (Bounds != null && Bounds.IsValid())
            sb.AppendFormat(" {0}", Bounds.ToString());
         return sb.ToString();
      }

   }

}
