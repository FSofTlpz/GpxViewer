using System;
using System.Text;
using System.Xml.XPath;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Bounds-Metadaten
   /// </summary>
   public class GpxBounds : BaseElement {

      public const string NODENAME = "bounds";

      public double MinLat;
      public double MaxLat;
      public double MinLon;
      public double MaxLon;


      public GpxBounds(string xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxBounds(GpxBounds b) : base() {
         MinLat = b.MinLat;
         MaxLat = b.MaxLat;
         MinLon = b.MinLon;
         MaxLon = b.MaxLon;
      }

      public GpxBounds(double minlat, double maxlat, double minlon, double maxlon) : base() {
         MinLat = minlat;
         MaxLat = maxlat;
         MinLon = minlon;
         MaxLon = maxlon;
      }

      protected override void Init() {
         MinLat = MaxLat = MinLon = MaxLon = NOTVALID_DOUBLE;
      }

      /// <summary>
      /// Sind die Daten gültig (vorhanden)?
      /// </summary>
      /// <returns></returns>
      public bool IsValid() {
         return MinLat != NOTVALID_DOUBLE &&
                MaxLat != NOTVALID_DOUBLE &&
                MinLon != NOTVALID_DOUBLE &&
                MaxLon != NOTVALID_DOUBLE;
      }

      /// <summary>
      /// vereinigt, wenn möglich, die beiden Bereiche
      /// </summary>
      /// <param name="bounds"></param>
      /// <returns></returns>
      public bool Union(GpxBounds bounds) {
         if (!IsValid()) {
            MinLat = bounds.MinLat;
            MaxLat = bounds.MaxLat;
            MinLon = bounds.MinLon;
            MaxLon = bounds.MaxLon;
         } else {
            if (bounds.IsValid()) {
               UnionLatLon(ref MinLon, ref MaxLon, bounds.MinLon, bounds.MaxLon, 180);
               UnionLatLon(ref MinLat, ref MaxLat, bounds.MinLat, bounds.MaxLat, 360);
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// vereinigt, wenn möglich, den Bereich mit dem Punkt
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      public bool Union(GpxPointBase pt) {
         if (IsValid())
            return Union(new GpxBounds(pt.Lat, pt.Lat, pt.Lon, pt.Lon));
         else {
            MinLat = pt.Lat;
            MaxLat = pt.Lat;
            MinLon = pt.Lon;
            MaxLon = pt.Lon;
            return true;
         }
      }

      void UnionLatLon(ref double min, ref double max, double min1, double max1, double period) {
         // falls eine Bereichsgrenze ungültig ist, wird zunächst ein "punktförmiger" Bereich angenommen
         if (min == NOTVALID_DOUBLE)
            min = max;
         else if (max == NOTVALID_DOUBLE)
            max = min;

         if (min1 == NOTVALID_DOUBLE)
            min1 = max1;
         else if (max1 == NOTVALID_DOUBLE)
            max1 = min1;

         // falls der Ausgangsbereich ungültig ist, wird der Zusatzbereich übernommen
         if (min == NOTVALID_DOUBLE) {
            min = min1;
            max = max1;
         } else if (min1 != NOTVALID_DOUBLE) { // Normalfall: 2 gültige Bereiche liegen vor

            // i.A. gilt min <= max; wenn aber min > max, dann geht der Bereich über period/2 hinaus
            if (min > max)
               min -= period; // damit der standardmäßige Größenvergleich fkt.
            if (min1 > max1)
               min1 -= period; // damit der standardmäßige Größenvergleich fkt.

            if (min <= min1 && max1 <= max) { // Zusatzbereich vollständig vom Ausgangsbereich eingeschlossen -> keine Veränderung
               if (min < -period / 2)
                  min += period; // wieder in den gültigen Wertebereich (geht über period/2 hinaus)
               return;
            }

            if (min1 < min && max < max1) { // Ausgangsbereich vollständig vom Zusatzbereich eingeschlossen -> Übernahme
               min = min1;
               max = max1;
               if (min < -period / 2)
                  min += period; // wieder in den gültigen Wertebereich (geht über period/2 hinaus)
               return;
            }

            max = Math.Max(max, max1);
            min = Math.Min(min, min1);
            if (min < -period / 2)
               min += period; // wieder in den gültigen Wertebereich (geht über period/2 hinaus)
         }
      }

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();
         XPathNavigator nav = GetNavigator4XmlText(removenamespace ? RemoveNamespace(xmltxt) : xmltxt);

         MinLat = XReadDouble(nav, "/" + NODENAME + "/@minlat");
         MaxLat = XReadDouble(nav, "/" + NODENAME + "/@maxlat");
         MinLon = XReadDouble(nav, "/" + NODENAME + "/@minlon");
         MaxLon = XReadDouble(nav, "/" + NODENAME + "/@maxlon");
      }

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public override string AsXml(int scale) {
         return (IsValid()) ?
                     XWriteNode(NODENAME,
                                new string[] {
                                   "minlat",
                                   "minlon",
                                   "maxlat",
                                   "maxlon" },
                                new string[] {
                                   XWrite(MinLat),
                                   XWrite(MinLon),
                                   XWrite(MaxLat),
                                   XWrite(MaxLon) }) :
                     "";
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (IsValid())
            sb.AppendFormat(" minlat={0} .. maxlat={1}, minlon={2} .. maxlon={3}", MinLat, MaxLat, MinLon, MaxLon);
         return sb.ToString();
      }

   }

}
