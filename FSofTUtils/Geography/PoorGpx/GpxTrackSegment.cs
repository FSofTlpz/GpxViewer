using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Segmentdaten eines Tracks
   /// </summary>
   public class GpxTrackSegment : BaseElement {

      /*
       https://www.topografix.com/GPX/1/1/#type_trkType 
       
         <xsd:complexType name="trksegType">
            <xsd:sequence>
               <-- elements must appear in this order -->
               <xsd:element name="trkpt" type="wptType" minOccurs="0" maxOccurs="unbounded"/>
               <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
            </xsd:sequence>
         </xsd:complexType>
       
       */

      /// <summary>
      /// mögliche Childnodes (in DIESER Reihenfolge)
      /// </summary>
      protected static string[] definedChildnodeNames = {
         "<trkpt>",        // mehrfach möglich
         "<extensions>",
      };

      public const string NODENAME = "trkseg";

      public ListTS<GpxTrackPoint> Points = new ListTS<GpxTrackPoint>();

      //static XPathExpression pathExpressionsForChilds = XPathExpression.Compile("/" + NODENAME + "/*");

      static string nodename4point = "<" + GpxTrackPoint.NODENAME + " ";


      public GpxTrackSegment(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxTrackSegment(GpxTrackSegment s) : base() => Points.AddRange(s.Points);

      public GpxTrackSegment(ListTS<GpxTrackPoint> ptlst) : base() => Points.AddRange(ptlst);

      public GpxTrackSegment(IList<GpxTrackPoint> ptlst) : base() => Points.AddRange(ptlst);


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
            if (UnhandledChildXml[max - 1].StartsWith("<extensions>") ||
                UnhandledChildXml[max - 1].StartsWith("<extensions "))  // könnte als letztes Child enthalten sein
               max--;

            Points = new ListTS<GpxTrackPoint>(UnhandledChildXml.Count);

            // zuerst alle Pointobjekte erzeugen und danach FromXml() ist gerinfügig schneller als Points.Add(new GpxTrackPoint(txt));
            for (int i = 0; i < max; i++)
               Points.Add(new GpxTrackPoint());

            for (int i = max - 1; i >= 0; i--) {
               string txt = UnhandledChildXml[i];
               if (txt.StartsWith(nodename4point)) {
                  Points[i].FromXml(txt, false);
                  UnhandledChildXml.RemoveAt(i);
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
      public override string AsXml(int scale = int.MaxValue) => xWriteNode(NODENAME, asxml(scale).ToString());

      public void AsXml(StringBuilder sb, int scale = int.MaxValue) {
         StringBuilder sbtmp = asxml(scale);
         xWriteNode(sbtmp, NODENAME);
         sb.Append(sbtmp);
      }

      StringBuilder asxml(int scale) {
         StringBuilder sb = new StringBuilder();

         // Sequenz: trkpt (mehrfach), extensions
         for (int p = 0; p < Points.Count; p++)
            sb.Append(Points[p].AsXml(scale));

         sb.Append(collectAllChilds(definedChildnodeNames, null, UnhandledChildXml, scale));

         return sb;
      }

      #endregion

      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public GpxTrackPoint? GetPoint(int idx) => idx < Points.Count ? Points[idx] : null;

      /// <summary>
      /// entfernt den <see cref="GpxTrackPoint"/> aus der Liste
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
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertPoint(GpxTrackPoint p, int pos = -1) {
         if (pos < 0 || Points.Count <= pos)
            Points.Add(p);
         else
            Points.Insert(pos, p);
      }

      public void InsertPoints(IList<GpxTrackPoint> plst, int pos = -1) {
         if (pos < 0 || Points.Count <= pos)
            Points.AddRange(plst);
         else
            Points.InsertRange(pos, plst);
      }

      /// <summary>
      /// ändert die Richtung
      /// </summary>
      public void ChangeDirection() => Points.Reverse();

      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         sb.AppendFormat(" {0} Punkte", Points.Count);
         return sb.ToString();
      }

   }

}
