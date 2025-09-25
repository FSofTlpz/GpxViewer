using System.Collections.Generic;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Daten eines Tracks
   /// </summary>
   public class GpxTrack : BaseElement {

      /*
       https://www.topografix.com/GPX/1/1/#type_trkType 
       
         <xsd:sequence>
            <xsd:element name="name" type="xsd:string" minOccurs="0"/>
            <xsd:element name="cmt" type="xsd:string" minOccurs="0"/>
            <xsd:element name="desc" type="xsd:string" minOccurs="0"/>
            <xsd:element name="src" type="xsd:string" minOccurs="0"/>
            <xsd:element name="link" type="linkType" minOccurs="0" maxOccurs="unbounded"/>
            <xsd:element name="number" type="xsd:nonNegativeInteger" minOccurs="0"/>
            <xsd:element name="type" type="xsd:string" minOccurs="0"/>
            <xsd:element name="extensions" type="extensionsType" minOccurs="0"/>
            <xsd:element name="trkseg" type="trksegType" minOccurs="0" maxOccurs="unbounded"/>
         </xsd:sequence>
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
         "<trkseg>",  // mehrfach möglich
      };

      public const string NODENAME = "trk";

      public ListTS<GpxTrackSegment> Segments = new ListTS<GpxTrackSegment>();

      public string Name = string.Empty;

      public string Comment = string.Empty;

      public string Description = string.Empty;

      public string Source = string.Empty;

      protected static string[] unhandledChildExceptions = {
         "<name>",
         "<cmt>",
         "<desc>",
         "<src>",
         "<" + GpxTrackSegment.NODENAME + ">",
      };


      public GpxTrack(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      /// <summary>
      /// Es wird keine "echte" Kopie erzeugt. Es werden zwar eigene Segmente erzeugt, aber diese enthalten dieselben
      /// Punkte (Verweise auf die Originalpunkte) wie der Originaltrack.
      /// </summary>
      /// <param name="t"></param>
      public GpxTrack(GpxTrack t) : base() {
         Name = t.Name;
         Comment = t.Comment;
         Description = t.Description;
         Source = t.Source;
         Segments = new ListTS<GpxTrackSegment>();
         for (int s = 0; s < t.Segments.Count; s++)
            Segments.Add(new GpxTrackSegment(t.Segments[s]));
      }

      protected override void Init() {
         Segments.Clear();
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
            int found = 0;
            for (int i = 0; i < UnhandledChildXml.Count && found < 5; i++) {
               if (checkChilds(i)) {
                  found++;
                  i--;
               }
            }

            if (UnhandledChildXml.Count == 0)
               UnhandledChildXml = null;        // wird nicht mehr benötigt
         }
      }

      protected bool checkChilds(int idx) {
         bool getit = false;
         string childtxt = UnhandledChildXml != null ? UnhandledChildXml[idx] : string.Empty;
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
         } else if (getString4ChildXml(childtxt, "<src>", out tmp)) {
            Source = tmp != null ? tmp : string.Empty;
            getit = true;
         } else if (childtxt.StartsWith("<" + GpxTrackSegment.NODENAME + ">")) {
            Segments.Add(new GpxTrackSegment(childtxt));
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
      public override string AsXml(int scale = int.MaxValue) {
         return xWriteNode(NODENAME, asxml(scale).ToString());
      }

      public void AsXml(StringBuilder sb, int scale = int.MaxValue) {
         StringBuilder sbtmp = asxml(scale);
         xWriteNode(sbtmp, NODENAME);
         sb.Append(sbtmp);
      }

      StringBuilder asxml(int scale = int.MaxValue) {
         StringBuilder sbtmp = collectAllChilds(definedChildnodeNames, getChildTxt4Props(scale), UnhandledChildXml, scale);
         for (int p = 0; p < Segments.Count; p++)
            sbtmp.Append(Segments[p].AsXml(scale));
         return sbtmp;
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
      /// liefert das <see cref="GpxTrackSegment"/> aus der Liste oder null
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackSegment? GetSegment(int s) {
         return s < Segments.Count ? Segments[s] : null;
      }

      /// <summary>
      /// liefert den <see cref="GpxTrackPoint"/> aus der Liste oder null
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public GpxTrackPoint? GetSegmentPoint(int s, int p) {
         return GetSegment(s)?.GetPoint(p);
      }

      /// <summary>
      /// entfernt das <see cref="GpxTrackSegment"/> aus der Liste
      /// </summary>
      /// <param name="s"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveSegment(int s) {
         if (0 <= s && s < Segments.Count) {
            Segments.RemoveAt(s);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt den <see cref="GpxTrackPoint"/> aus der Liste
      /// </summary>
      /// <param name="s"></param>
      /// <param name="p"></param>
      /// <returns>false, wenn das Objekt nicht ex.</returns>
      public bool RemoveSegmentPoint(int s, int p) {
         if (0 <= s && s < Segments.Count)
            return Segments[s].RemovePoint(p);
         return false;
      }

      /// <summary>
      /// fügt ein <see cref="GpxTrackSegment"/> ein oder an
      /// </summary>
      /// <param name="s"></param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertSegment(GpxTrackSegment s, int pos = -1) {
         if (pos < 0 || Segments.Count <= pos)
            Segments.Add(s);
         else
            Segments.Insert(pos, s);
      }

      /// <summary>
      /// fügt einen <see cref="GpxTrackPoint"/> ein oder an
      /// </summary>
      /// <param name="p"></param>
      /// <param name="s">Segment</param>
      /// <param name="pos">negative Werte führen zum Anhängen an die Liste</param>
      public void InsertSegmentPoint(GpxTrackPoint p, int s, int pos = -1) {
         GetSegment(s)?.InsertPoint(p, pos);
      }


      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (!string.IsNullOrEmpty(Name))
            sb.AppendFormat(" name=[{0}]", Name);
         sb.AppendFormat(" {0} Segmente", Segments.Count);
         return sb.ToString();
      }

   }

}
