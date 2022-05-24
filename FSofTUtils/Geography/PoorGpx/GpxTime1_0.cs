using System;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Time-Metadaten für Version 1.0
   /// </summary>
   public class GpxTime1_0 : BaseElement {

      public const string NODENAME = "time";

      public DateTime Time;


      public GpxTime1_0(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxTime1_0(GpxTime1_0 t) : base() {
         Time = t.Time;
      }


      protected override void Init() {
         Time = NOTVALID_TIME;
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         Time = XReadDateTime(nav, "/" + NODENAME);
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         return Time != NOTVALID_TIME ?
                           XWriteNode(NODENAME, Time) :
                           "";
      }

      /// <summary>
      /// interpretiert den Text als <see cref="DateTime"/>
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public static DateTime String2DateTime(string txt) {
         return ReadDateTime(txt);
      }

      /// <summary>
      /// liefert <see cref="DateTime"/> als (GPX-)Text
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      public static string DateTime2String(DateTime dt) {
         return XWrite(dt);
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (Time != NOTVALID_TIME)
            sb.AppendFormat(" {0}", Time);
         return sb.ToString();
      }

   }

}
