using System;
using System.Diagnostics;

namespace GMap.NET.FSofTExtented.Projections {

   public class UTMProjection : PureProjection {
      public static readonly UTMProjection Instance = new UTMProjection();

      const int TILESIZEEXP = 9;
      const int TILESIZE = 1 << TILESIZEEXP;

      /// <summary>
      /// UTM-Bereich
      /// </summary>
      static readonly double MinLatitude = -80;
      static readonly double MaxLatitude = 84;
      static readonly double MinLongitude = -180;
      static readonly double MaxLongitude = 180;

      /*
      UTM:
      6° breite Zonen
      i.A. jeweils 8° hohe Bereiche (-80°..84°) zusätzlich mit Buchstaben bezeichnet (C,D,...,X, ohne I und O) (X mit 12°, Skandinavien 32V 9° breit)
      Äquator ist x-Achse, Mittelmeridian y-Achse

      x am Mittelmeridian ist immer 500.000m („false easting“); Rechtswerte liegen demnach zwischen 100.000 und 899.999 Metern, sind also immer sechsstellig. (166.021,4 ... 833.978,6)

      y am Äquator für Nordhalbkugel ist immer 0m (am Pol etwa 9.999.000)
      y am Äquator für Südhalbkugel ist immer 10.000.000m (am Pol etwa 19.999.000)

      Der X-(Rechts-)Wert ist die Entfernung zum Mittelmeridian. Er muss mit dem Maßstabsfaktor 0,9996 multipliziert werden. So erhält man den Rechtswert der UTM-Koordinate.
      Hochwert analog.


      Die (interne) Karte besteht in der Projektion aus X x Y Pixel (aufgeteilt in Tiles). Der Koordinatenursprung ist links-oben.
      o-->
      |
      V
      
      FromLatLngToPixel() liefert den Kartenpixel in dieser Karte zu den WGS84-Koordinaten.
      FromPixelToLatLng() liefert die WGS84-Koordinaten zum Kartenpixel.

      z.B. wird bei Google-Map die gesamte Erdoberfläche auf einer quadratischen internen Karte abgebildet. (insofern ist die Maßstabsangabe natürlich falsch bzw. gilt nur für den Äquator o.ä.)

      Es ist davon auszugehen, das Lat/Lng in gewissen Sinn als symbolische Werte anzusehen sind, die per einfachem "Maßstab" unverzerrt auf die interne Karte abgebildet werden.
      Insofern wäre eine einfache lineare Umrechnung von UTM auf das interne KS und eine einfache lineare Umrechnung von UTM auf die symbol. Lat/Lng sinnvoll.
      Problem: Zusammenführung von 2 Streifen nebeneinander?
         UTM bei 0° etwa 166021.443 .. 833978.557 -> bei diesem Wert schließt der nächste Streifen an
                50° etwa 285015.763 .. 714984.237 ->                   "
             -> deshalb ist die Umrechnung von UTM auf _echte_ Lng, und dann die einfache lineare Umrechnung auf das interne KS sinnvoll
                -> Problem: Rückrechnung von x zu UTM
                            x zu Lng ist einfache lineare Umrechnung
                            für exakte Umrechnung Lng zu UTM ist Lat nötig

      */


      public override RectLatLng Bounds {
         get {
            return RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);
         }
      }

      public override GSize TileSize {
         get;
      } = new GSize(TILESIZE, TILESIZE);

      double _axis = 6378137;
      public override double Axis => _axis;

      double _flattening = 1.0 / 298.257223563;
      public override double Flattening => _flattening;

      /// <summary>
      /// muss mit EPSG der URL-Abfrage zusammenpassen, sonst fkt. <see cref="FromPixelToLatLng"/>() nicht korrekt
      /// </summary>
      public int UTMZone = 33;

      /// <summary>
      /// setzt die Daten für den Ellipsoiden des geografischen Koordinatensystems
      /// </summary>
      /// <param name="axis"></param>
      /// <param name="inversflattening"></param>
      public void SetEllipseData(double axis, double inversflattening) {
         _axis = axis;
         _flattening = 1.0 / inversflattening;
      }

      /// <summary>
      /// Umwandlung der WGS84-Koordinaten in das Pixel der internen Karte
      /// </summary>
      /// <param name="lat"></param>
      /// <param name="lng"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GPoint FromLatLngToPixel(double lat, double lng, int zoom) {
         PointLatLng latlng = new PointLatLng(Clip(lat, MinLatitude, MaxLatitude), Clip(lng, MinLongitude, MaxLongitude));
         PointUTM utm = WGS84ToUTM(latlng);
         //Debug.WriteLine(string.Format("FromLatLngToPixel(lat={0}, lng={1}, zoom={2}) -> UTM: {3}", latlng.Lat, latlng.Lng, zoom, utm));

         GPoint gp = UTM2Pixel(utm, zoom);
         //Debug.WriteLine(string.Format("   -> Pixel {0}", gp));

         return gp;
      }

      /// <summary>
      /// Umwandlung des Pixel der internen Karte in WGS84-Koordinaten
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override PointLatLng FromPixelToLatLng(long x, long y, int zoom) {
         PointUTM utm = Pixel2UTM(x, y, zoom);
         //Debug.WriteLine(string.Format("FromPixelToLatLng(x={0}, y={1}, zoom={2}) -> UTM: {3} / {4} zone={5}", x, y, zoom, utm[0], utm[1], UTMZone));

         PointLatLng lanlng = UTMToWGS84(utm);
         //Debug.WriteLine(string.Format("   -> Lat={0} Lng={1}", lanlng.Lat, lanlng.Lng));

         return new PointLatLng(Clip(lanlng.Lat, MinLatitude, MaxLatitude),
                                Clip(lanlng.Lng, MinLongitude, MaxLongitude));
      }

      /// <summary>
      /// The ground resolution indicates the distance (in meters) on the ground that’s represented by a single pixel in the map.
      /// For example, at a ground resolution of 10 meters/pixel, each pixel represents a ground distance of 10 meters.
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="latitude"></param>
      /// <returns></returns>
      public override double GetGroundResolution(int zoom, double latitude) {
         double utm1 = Pixx2RealUTMx(500000, GetTileMatrixSizePixel(zoom).Width, UTMZone);
         double utm2 = Pixx2RealUTMx(500001, GetTileMatrixSizePixel(zoom).Width, UTMZone);
         return utm2 - utm1;
      }



      /// <summary>
      /// Höhe der "internen" Karte in Pixel
      /// </summary>
      const double UTMMAXY = 20000000;
      /// <summary>
      /// Breite der "internen" Karte in Pixel
      /// </summary>
      const double UTMMAXX = 40000000;

      /*
       Zur Umrechnung der echten x-Werte wird zunächst ein "globaler" x-Wert erzeugt und dieser dann linear auf den Bereich 0..20000000 umgerechnet.
       
         Am Äquator liegt der echte x-Werte etwa im Bereich 166021..833979. Die Differenz zum Startwert ist also x-166021.

       Eigentlich müsste der proz. Wert auf der Länge des zugehörigen Breitenkreises ermittelt werden.(?)
         
       Der y-Wert wird einfach linear umgerechnet:
            UTM-Werte im Bereich 0..20000000 <-> Pixel GetTileMatrixSizePixel(zoom).Height
       */

      public PointUTM Pixel2UTM(long x, long y, int zoom) {
         Debug.WriteLine(string.Format("Pixel2UTM: x={0}, y={1}", x, y));

         GSize s = GetTileMatrixSizePixel(zoom);

         Debug.WriteLine(string.Format("Pixel2UTM: GetTileMatrixSizePixel {0}", s));

         double utmy = Pixy2RealUTMy(y, s.Height);

         Debug.WriteLine(string.Format("Pixel2UTM: utmy={0}", utmy));

         double utmx = Pixx2RealUTMx(x, s.Width, UTMZone);

         PointUTM utm = new PointUTM(utmx, utmy, UTMZone);

         Debug.WriteLine(string.Format("Pixel2UTM: utm={0}", utm));

         return utm;
      }

      /// <summary>
      /// echte projizierte Koordinaten auf Pixel des Gesamtbildes abbilden
      /// </summary>
      /// <param name="utm"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public GPoint UTM2Pixel(PointUTM utm, int zoom) {
         GSize s = GetTileMatrixSizePixel(zoom);

         Debug.WriteLine(string.Format("UTM2Pixel: utm={0}", utm));
         Debug.WriteLine(string.Format("UTM2Pixel: GetTileMatrixSizePixel {0}", s));

         long py = RealUTMy2Pixy(utm.Y, s.Height);

         Debug.WriteLine(string.Format("UTM2Pixel: utm.Y={0}", py));

         long px = RealUTMx2Pixx(utm, s.Width);

         Debug.WriteLine(string.Format("UTM2Pixel: utm.X={0}", px));

         GPoint p = new GPoint(px, py);

         Debug.WriteLine(string.Format("UTM2Pixel: pixel={0}", p));

         return p;
      }


      long RealUTMy2Pixy(double utmy, long height) {
         // einen echten UTM-Wert in den fortlaufenden Bereich 0..20000000 (S..N) umsetzten
         if (utmy < 10000000)
            utmy += 10000000;    // N
         else
            utmy = -utmy + 20000000;   // S
         // Koordinatensystem ist aber nach unten gerichtet
         utmy = UTMMAXY - utmy;
         return (long)Math.Round(utmy * height / UTMMAXY);
      }

      double Pixy2RealUTMy(long y, long height) {
         double utmy = UTMMAXY * y / height;     // 0 .. UTMMAXY
         // Koordinatensystem ist nach unten gerichtet
         utmy = UTMMAXY - utmy;

         // UTM-Wert aus dem fortlaufenden Bereich 0..20000000 (S..N) in einen echten Wert umsetzen
         if (utmy < 10000000)
            utmy = 20000000 - utmy;    // S
         else
            utmy -= 10000000;    // N
         return utmy;
      }

      /// <summary>
      /// min. UTMx-Wert am Äquator
      /// </summary>
      const double utmx_min = 166021.443096077;

      long RealUTMx2Pixx(PointUTM utm, long width) {
         double utmx = (utm.Zone - 1) / 60.0 * UTMMAXX + (utm.X - utmx_min);
         return (long)Math.Round(utmx * width / UTMMAXX);
      }

      double Pixx2RealUTMx(long x, long width, int zone) {
         return -(zone - 1) / 60.0 * UTMMAXX + utmx_min + x * UTMMAXX / width;
      }

#if VAR1
      long RealUTMx2Pixx(PointUTM utm, long width) {
         double utmx = (utm.Zone - 1) / 60.0 * UTMMAXX + (utm.X - utm_min);
         return (long)Math.Round(utmx * width / UTMMAXX);
     }

      double Pixx2RealUTMx(long x, long width) {
         return -(UTMZone - 1) / 60.0 * UTMMAXX + utm_min + x * UTMMAXX / width;
      }
#endif


#if VAR2
      // MIST

      long RealUTMx2Pixx(PointUTM utm, long width) {
         double lng = UTMToWGS84(utm).Lng;  // -180 .. +180
         lng /= 360;                        // -0.5 .. +0.5
         lng += 0.5;                        //    0 .. 1
         lng *= UTMMAXX;                    //    0 .. UTMMAXX
         return (long)Math.Round(lng * width / UTMMAXX);
      }

      double Pixx2RealUTMx(long x, long width, double lat, out int zone) {
         double lng = x * UTMMAXX / width;   //    0 .. UTMMAXX
         lng /= UTMMAXX;             // 0 .. 1
         lng -= 0.5;                 // -0.5 .. +0.5
         lng *= 360;                 // -180 .. +180

         PointUTM utm = WGS84ToUTM(new PointLatLng(lat, lng));

         zone = utm.Zone;

         return utm.X;
      }
#endif

      /*
      /// <summary>
      /// Größe des Tile-Arrays
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixSizeXY(int zoom) {
         // Std.:
         //GSize sMin = GetTileMatrixMinXY(zoom);
         //GSize sMax = GetTileMatrixMaxXY(zoom);
         //return new GSize(sMax.Width - sMin.Width + 1, sMax.Height - sMin.Height + 1);

         return new GSize(1 << zoom, 1 << zoom);
      }

      /// <summary>
      /// Pixelbereich über das gesamte Tile-Array
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixSizePixel(int zoom) {
         GSize s = GetTileMatrixSizeXY(zoom);
         return new GSize(s.Width << 8, s.Height << 8);        // Std.: new GSize(s.Width * TileSize.Width, s.Height * TileSize.Height);
      }
      */

      /// <summary>
      /// kleinster Tile-Index in x- und y-Richtung
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixMinXY(int zoom) => new GSize(0, 0);

      /// <summary>
      /// größer Tile-Index in x- und y-Richtung
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public override GSize GetTileMatrixMaxXY(int zoom) {
         long xy = 1 << (zoom + 8 - TILESIZEEXP);
         //long xy = 1 << zoom;  // Anzahl 2^zoom
         return new GSize(2 * xy - 1, xy - 1);     // ein Rechteck mit den Seitenlängen 2 : 1
      }


      #region WGS84 <==> UTM

      /// <summary>
      ///     calculates UTM zone number
      /// </summary>
      /// <param name="lon">Longitude in degrees</param>
      /// <returns></returns>
      protected static new long GetUTMZone(double lon) => (long)((lon + 180.0) / 6.0 + 1.0);

      static readonly ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctFac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

      /// <summary>
      /// liefert UTM-Koordinaten (x, y, zone, north) für WGS84-Koordinaten
      /// </summary>
      /// <param name="latlng"></param>
      /// <param name="zone">nur, wenn eine bestimmte Zone erzwungen werden soll (i.A. sinnlos)</param>
      /// <returns></returns>
      public static PointUTM WGS84ToUTM(PointLatLng latlng, int zone = 0) {
         if (zone <= 0)
            zone = (int)GetUTMZone(latlng.Lng);
         double lat = latlng.Lat;
         bool north = lat >= 0;

         if (lat < 0)
            lat = -lat;

         var ct = ctFac.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                                                    ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(zone, north));
         double[] xy = ct.MathTransform.Transform(new double[] { latlng.Lng, lat });
         return new PointUTM(xy[0], xy[1], zone);

         //(double x, double y) = ct.MathTransform.Transform(latlng.Lng, lat);
         //return new PointUTM(x, y, zone);
      }

      /// <summary>
      /// liefert WGS84-Koordinaten für UTM-Koordinaten
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y">Werte unter 10.000.000 gelten für die nördl. Zonen, größere für die südl. Zonen</param>
      /// <param name="zone">UTM-Zone 1..60</param>
      /// <returns></returns>
      public static PointLatLng UTMToWGS84(PointUTM utm) {
         var ct = ctFac.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(utm.Zone, utm.North),
                                                    ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);

         double[] latlng = ct.MathTransform.Transform(new double[] { utm.X, utm.Y });  // liefert lng/lat !
         return new PointLatLng(latlng[1], latlng[0]);
      }

      #endregion


   }
}
