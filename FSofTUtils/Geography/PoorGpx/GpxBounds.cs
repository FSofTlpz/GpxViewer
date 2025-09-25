using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FSofTUtils.Geography.PoorGpx {

   /// <summary>
   /// Bounds-Metadaten
   /// </summary>
   public class GpxBounds : BaseElement {

      /*
       https://www.topografix.com/GPX/1/1/#boundsType 
       
      <xsd:complexType name="boundsType">
         <xsd:attribute name="minlat" type="latitudeType" use="required"/>
         <xsd:attribute name="minlon" type="longitudeType" use="required"/>
         <xsd:attribute name="maxlat" type="latitudeType" use="required"/>
         <xsd:attribute name="maxlon" type="longitudeType" use="required"/>
      </xsd:complexType>

       */


      public const string NODENAME = "bounds";

      public double MinLat;
      public double MaxLat;
      public double MinLon;
      public double MaxLon;

      public double Width => MaxLon - MinLon;

      public double Height => MaxLat - MinLat;


      public GpxBounds(string? xmltext = null, bool removenamespace = false) :
         base(xmltext, removenamespace) { }

      public GpxBounds(GpxBounds b) : base() {
         MinLat = b.MinLat;
         MaxLat = b.MaxLat;
         MinLon = b.MinLon;
         MaxLon = b.MaxLon;
      }

      public GpxBounds(IList<GpxPointBase> pts) : base() {
         Union(pts);
      }

      public GpxBounds(IList<GpxTrackPoint> pts) : base() {
         Union(pts);
      }

      public GpxBounds(ListTS<GpxTrackPoint> pts) : base() {
         Union(pts);
      }

      public GpxBounds(double minlat, double maxlat, double minlon, double maxlon) : base() {
         MinLat = GetNormedLatitude(minlat);
         MaxLat = GetNormedLatitude(maxlat);
         MinLon = GetNormedLongitude(minlon);
         MaxLon = GetNormedLongitude(maxlon);
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
               unionLatLon(ref MinLon, ref MaxLon, bounds.MinLon, bounds.MaxLon, 180);
               unionLatLon(ref MinLat, ref MaxLat, bounds.MinLat, bounds.MaxLat, 360);
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

      public bool Union<T>(IList<T> pts) where T : GpxPointBase {
         foreach (var item in pts)
            if (!Union(item))
               return false;
         return true;
      }

      public bool Union<T>(ListTS<T> pts) where T : GpxPointBase {
         for (int i = 0; i < pts.Count; i++)
            if (!Union(pts[i]))
               return false;
         return true;
      }

      void unionLatLon(ref double min, ref double max, double min1, double max1, double period) {
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
      /// ACHTUNG<para>nur mathematisch; Überschreitung der "Datumsgrenze" noch NICHT berücksichtigt</para>
      /// </summary>
      /// <param name="bounds"></param>
      /// <returns></returns>
      public bool IntersectsWith(GpxBounds bounds) {
         if (bounds.MinLon < MinLon + Width &&
             MinLon < bounds.MinLon + bounds.Width &&
             bounds.MinLat < MinLat + Height) {
            return MinLat < bounds.MinLat + bounds.Height;
         }
         return false;
      }

      #region liest das Objekt aus einem XML-Text ein

      /// <summary>
      /// setzt die Objektdaten aus dem XML-Text
      /// </summary>
      /// <param name="xmltxt"></param>
      /// <param name="removenamespace"></param>
      public override void FromXml(string xmltxt, bool removenamespace = false) {
         Init();

         List<(string, string)> gpxattributes = getAttributeCollection(xmltxt, removenamespace);
         for (int i = 0; i < gpxattributes.Count; i++) {
            if (gpxattributes[i].Item1 == "minlat")
               MinLat = Convert.ToDouble(gpxattributes[i].Item2, CultureInfo.InvariantCulture);
            else if (gpxattributes[i].Item1 == "maxlat")
               MaxLat = Convert.ToDouble(gpxattributes[i].Item2, CultureInfo.InvariantCulture);
            if (gpxattributes[i].Item1 == "minlon")
               MinLon = Convert.ToDouble(gpxattributes[i].Item2, CultureInfo.InvariantCulture);
            else if (gpxattributes[i].Item1 == "maxlon")
               MaxLon = Convert.ToDouble(gpxattributes[i].Item2, CultureInfo.InvariantCulture);
         }
      }

      #endregion

      #region liefert das Objekt als XML

      /// <summary>
      /// liefert den vollständigen XML-Text für das Objekt
      /// </summary>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns>leere Zeichenkette wenn ungültig</returns>
      public override string AsXml(int scale = int.MaxValue) {
         return IsValid() ?
                     xWriteNode(NODENAME,
                                new string[] {
                                   "minlat",
                                   "minlon",
                                   "maxlat",
                                   "maxlon" },
                                new string[] {
                                   xWriteText(MinLat),
                                   xWriteText(MinLon),
                                   xWriteText(MaxLat),
                                   xWriteText(MaxLon) }) :
                     string.Empty;
      }

      /// <summary>
      /// hängt den vollständigen XML-Text für das Objekt an den StringBuilder an
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="scale">Umfang der Ausgabe</param>
      /// <returns></returns>
      public void AsXml(StringBuilder sb, int scale = int.MaxValue) => sb.Append(AsXml(scale));

      #endregion

      public override string ToString() {
         StringBuilder sb = new StringBuilder(NODENAME + ":");
         if (IsValid())
            sb.AppendFormat(" minlat={0} .. maxlat={1}, minlon={2} .. maxlon={3}", MinLat, MaxLat, MinLon, MaxLon);
         return sb.ToString();
      }

   }

}
