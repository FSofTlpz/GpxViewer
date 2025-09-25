using System;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Time-Metadaten für Version 1.0
   /// </summary>
   public class GpxTime1_0 : BaseElement {

      public const string NODENAME = "time";

      public DateTime Time;


      public GpxTime1_0(string? xmltext = null, bool removenamespace = false) :
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
         if (getDateTime4ChildXml(xmltxt, "<" + NODENAME + ">", out DateTime dt))
            Time = dt;
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) =>
         Time != NOTVALID_TIME ?
                           xWriteNode(NODENAME, Time) :
                           string.Empty;

      /// <summary>
      /// hängt den vollständigen XML-Text für das Objekt an den StringBuilder an
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="scale">Umfang der Ausgabe</param>
      public void AsXml(StringBuilder sb, int scale) => sb.Append(AsXml(scale));

      /// <summary>
      /// interpretiert den Text als <see cref="DateTime"/>
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public static DateTime String2DateTime(string txt) => xDateTime(txt);

      /// <summary>
      /// liefert <see cref="DateTime"/> als (GPX-)Text
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      public static string DateTime2String(DateTime dt) => xWriteText(dt);


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (Time != NOTVALID_TIME)
            sb.AppendFormat(" {0}", Time);
         return sb.ToString();
      }

   }

}
