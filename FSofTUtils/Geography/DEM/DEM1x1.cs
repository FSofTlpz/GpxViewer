//#define WRITEBITMAPS
using System;
using System.Diagnostics;

namespace FSofTUtils.Geography.DEM {

   /// <summary>
   /// This class hold the DEM data for 1°x1°. The number of columns can be different from the number of rows. 
   /// You need a derived class for filling in the data.
   /// </summary>
   [Serializable]
   abstract public class DEM1x1 : IDisposable {

      /// <summary>
      /// int value for "no data"
      /// </summary>
      public const int DEMNOVALUE = -32768;

      /// <summary>
      /// double value for "no data" (from interpolation)
      /// </summary>
      public const double NOVALUED = double.MinValue;

      /// <summary>
      /// feet in meter
      /// </summary>
      public const double FOOT = 0.3048;

      public enum InterpolationType {
         standard,
         bicubic_catmull_rom,
      }


      /// <summary>
      /// left border (longitude)
      /// </summary>
      public double Left { get; protected set; }
      /// <summary>
      /// lower border (latitude)
      /// </summary>
      public double Bottom { get; protected set; }

      /// <summary>
      /// number of Rows (e.g. 1201, 3601, ...)
      /// </summary>
      public int Rows { get; protected set; }
      /// <summary>
      /// number of Columns (e.g. 1201, 3601, ...)
      /// </summary>
      public int Columns { get; protected set; }
      /// <summary>
      /// horizontal distance between 2 points (in degree)
      /// </summary>
      public double DeltaX { get { return Width / (Columns - 1); } }
      /// <summary>
      /// vertical distance between 2 points (in degree)
      /// </summary>
      public double DeltaY { get { return Width / (Rows - 1); } }

      /// <summary>
      /// minimal value
      /// </summary>
      public int Minimum { get; protected set; }
      /// <summary>
      /// maximal value
      /// </summary>
      public int Maximum { get; protected set; }
      /// <summary>
      /// number of unvalid values
      /// </summary>
      public long NotValid { get; protected set; }


      /// <summary>
      /// upper border (latitude)
      /// </summary>
      public double Top { get { return Bottom + Height; } }
      /// <summary>
      /// right border (longitude)
      /// </summary>
      public double Right { get { return Left + Width; } }
      /// <summary>
      /// width (in degree)
      /// </summary>
      public double Width { get { return 1; } }
      /// <summary>
      /// height (in degree)
      /// </summary>
      public double Height { get { return 1; } }

      /// <summary>
      /// dem data-array
      /// </summary>
      protected short[] data;

      /// <summary>
      /// data for hillshading
      /// </summary>
      protected byte[]? hillshade;

      /// <summary>
      /// last shading for this value
      /// </summary>
      public double HillShadeAzimut { get; protected set; } = double.MinValue;

      /// <summary>
      /// last shading for this value
      /// </summary>
      public double HillShadeAltitude { get; protected set; } = double.MinValue;

      /// <summary>
      /// last shading for this value
      /// </summary>
      public double HillShadeScale { get; protected set; } = double.MinValue;


      public DEM1x1() {
         Left = 0;
         Bottom = 0;
         data = Array.Empty<short>();
         hillshade = null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="left">left longitude</param>
      /// <param name="bottom">bottom latitude</param>
      public DEM1x1(double left, double bottom) {
         Left = left;
         Bottom = bottom;
         data = Array.Empty<short>();
      }


      /// <summary>
      /// set the data array
      /// </summary>
      abstract public void SetDataArray();

      /// <summary>
      /// analog <see cref="Get(int, int)"/>, aber ohne Überprüfung der Parameter
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      protected int fastget(int row, int col) => data[row * Columns + col];

      /// <summary>
      /// get a value (coordinate origin left-top)
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      public int Get(int row, int col) {
         if (row < 0 || Rows <= row ||
             col < 0 || Columns <= col)
            throw new Exception(string.Format("({0}, {1}) unvalid row and/or column ({2}x{3})", col, row, Columns, Rows));
         return data[row * Columns + col];
      }

      /// <summary>
      /// analog <see cref="Get4XY(int, int)"/>, aber ohne Überprüfung der Parameter
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      protected int fastget4XY(int x, int y) => fastget(Rows - 1 - y, x);

      /// <summary>
      /// get a value (coordinate origin left-bottom)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Get4XY(int x, int y) {
         return Get(Rows - 1 - y, x);
      }

      //int get4XY(int x, int y) {
      //   return data[(Rows - 1 - y) * Columns + x];
      //}

      /// <summary>
      /// exist 1 or more valid values
      /// </summary>
      /// <returns></returns>
      public bool HasValidValues() => Minimum != DEMNOVALUE || Maximum != DEMNOVALUE;

      /// <summary>
      /// get the interpolated value (or <see cref="NOVALUED"/>)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public double InterpolatedHeight(double lon, double lat, InterpolationType intpol) {
         double h = NOVALUED;

         lon -= Left;
         lat -= Bottom; // Koordinaten auf die Ecke links unten bezogen

         switch (intpol) {
            case InterpolationType.standard:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {
                  // x-y-Index des Eckpunktes links unten des umschließenden Quadrats bestimmen
                  int x = (int)(lon / DeltaX);
                  int y = (int)(lat / DeltaY);
                  if (x == Columns - 1) // liegt auf rechtem Rand
                     x--;
                  if (y == Rows - 1) // liegt auf oberem Rand
                     y--;

                  // lon/lat jetzt bzgl. der Ecke links-unten des umschließenden Quadrats bilden (0 .. <Delta)
                  double delta_lon = lon - x * DeltaX;
                  double delta_lat = lat - y * DeltaY;

                  if (delta_lon == 0) {            // linker Rand
                     if (delta_lat == 0)
                        h = fastgetHeight4XY(x, y);          // Eckpunkt links unten
                     else if (delta_lat >= DeltaY)
                        h = fastgetHeight4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(fastget4XY(x, y),
                                                     fastget4XY(x, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lon >= DeltaX) { // rechter Rand (eigentlich nicht möglich)
                     if (delta_lat == 0)
                        h = fastgetHeight4XY(x + 1, y);      // Eckpunkt rechts unten
                     else if (delta_lat >= DeltaY)
                        h = fastgetHeight4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(fastget4XY(x + 1, y),
                                                     fastget4XY(x + 1, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
                     h = linearInterpolatedValue(fastget4XY(x, y),
                                                  fastget4XY(x + 1, y),
                                                  delta_lon / DeltaX);
                  } else if (delta_lat >= DeltaY) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
                     h = linearInterpolatedValue(fastget4XY(x, y + 1),
                                                  fastget4XY(x + 1, y + 1),
                                                  delta_lon / DeltaX);

                  } else {                         // Punkt innerhalb des Rechtecks

                     //int leftbottom, rightbottom, righttop, lefttop;
                     //Get4XYSquare(x, y, out leftbottom, out rightbottom, out righttop, out lefttop);  // etwas schneller als die obere Version

                     int idx = (Rows - 1 - y) * Columns; // Anfang der unteren Zeile
                     idx += x;
                     int leftbottom = data[idx++];
                     int rightbottom = data[idx];
                     idx -= Columns;
                     int righttop = data[idx--];
                     int lefttop = data[idx];
                     h = interpolatedHeightInNormatedRectangle_New(delta_lon / DeltaX,
                                                                   delta_lat / DeltaY,
                                                                   lefttop,
                                                                   righttop,
                                                                   rightbottom,
                                                                   leftbottom);
                  }
               }
               break;

            case InterpolationType.bicubic_catmull_rom:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {

                  if (Columns >= 4 && Rows >= 4) {
                     // x-y-Index des Punktes link-unterhalb bestimmen
                     int x = (int)(lon / DeltaX);
                     int y = (int)(lat / DeltaY);
                     // x-y-Index des Punktes link-unterhalb dieses Punktes bestimmen
                     x--;
                     y--;
                     if (x < 0)
                        x = 0;
                     else if (x >= Columns - 4)
                        x = Columns - 4;
                     if (y < 0)
                        y = 0;
                     else if (y >= Rows - 4)
                        y = Rows - 4;

                     double[][] p = new double[4][];
                     for (int i = 0; i < 4; i++)
                        p[i] = new double[] { fastget4XY(x, y + 3-i),
                                              fastget4XY(x + 1, y + 3-i),
                                              fastget4XY(x + 2, y + 3-i),
                                              fastget4XY(x + 3, y + 3-i) };

                     bool allvalid = true;
                     for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 4; j++)
                           if (p[i][j] == DEMNOVALUE) {
                              allvalid = false;
                              i = j = 5;
                           }

                     if (allvalid)
                        h = Dim2CubicInterpolation(p, lon / DeltaX - x, lat / DeltaY - y);
                     else
                        h = InterpolatedHeight(lon + Left, lat + Bottom, InterpolationType.standard);
                  } else
                     h = InterpolatedHeight(lon + Left, lat + Bottom, InterpolationType.standard);
               }
               break;
         }

         return h;
      }

      //protected void Get4XYSquare1(int x, int y,
      //                            out int leftbottom, out int rightbottom, out int righttop, out int lefttop) {
      //   //if (xleft < 0 || Columns <= xleft + 1 ||
      //   //    ybottom < 0 || Rows <= ybottom + 1)
      //   //   throw new Exception(string.Format("({0}, {1}) is out of area ({2}x{3})", xleft, ybottom, Columns, Rows));
      //   int idx = (Rows - 1 - y) * Columns; // Anfang der unteren Zeile
      //   idx += x;
      //   leftbottom = data[idx++];
      //   rightbottom = data[idx];
      //   idx -= Columns;
      //   righttop = data[idx--];
      //   lefttop = data[idx];
      //}

      /// <summary>
      /// resize the internal datatable
      /// </summary>
      /// <param name="newcols"></param>
      /// <param name="newrows"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public bool ResizeDatatable(int newcols, int newrows, InterpolationType intpol) {
         if (newcols < 3 || newrows < 3)
            throw new Exception("New tablesize less 3 not permitted.");

         if (newcols != Columns ||
             newrows != Rows) {
            NotValid = 0;
            short[] newdata = new short[newcols * newcols];
            double deltax = 1.0 / (newcols - 1);
            double deltay = 1.0 / (newrows - 1);

            for (int row = 0; row < newrows; row++) {
               for (int col = 0; col < newcols; col++) {
                  double hi = InterpolatedHeight(Left + col * deltax, Bottom + 1 - row * deltay, intpol);
                  if (hi == NOVALUED) {
                     NotValid++;
                     newdata[row * newcols + col] = DEMNOVALUE;
                  } else {
                     short h = (short)Math.Round(hi);
                     if (Maximum < h)
                        Maximum = h;
                     else if (Minimum > h)
                        Minimum = h;
                     newdata[row * newcols + col] = h;
                  }
               }
            }
            data = newdata;
            Columns = newcols;
            Rows = newrows;
            return true;
         }
         return false;
      }

      double fastgetHeight4XY(int x, int y) {
         int h = fastget4XY(x, y);
         return h == DEMNOVALUE ? NOVALUED : h;
      }

      #region Bilinear Interpolation

      /// <summary>
      /// get surrounding 4 point
      /// </summary>
      /// <param name="xleft"></param>
      /// <param name="ybottom"></param>
      /// <param name="leftbottom"></param>
      /// <param name="rightbottom"></param>
      /// <param name="righttop"></param>
      /// <param name="lefttop"></param>
      //protected void Get4XYSquare(int xleft, int ybottom,
      //                            out int leftbottom, out int rightbottom, out int righttop, out int lefttop) {
      //   //if (xleft < 0 || Columns <= xleft + 1 ||
      //   //    ybottom < 0 || Rows <= ybottom + 1)
      //   //   throw new Exception(string.Format("({0}, {1}) is out of area ({2}x{3})", xleft, ybottom, Columns, Rows));
      //   int idx = (Rows - 1 - ybottom) * Columns; // Anfang der unteren Zeile
      //   idx += xleft;
      //   leftbottom = data[idx++];
      //   rightbottom = data[idx];
      //   idx -= Columns;
      //   righttop = data[idx--];
      //   lefttop = data[idx];
      //}

      /// <summary>
      /// liefert den "gewichteten" Wert zwischen den beiden Werten in Relation zum jeweiligen Abstand (alle 3 Werte auf einer Linie)
      /// </summary>
      /// <param name="h1"></param>
      /// <param name="l1"></param>
      /// <param name="h2"></param>
      /// <param name="q1"></param>
      /// <returns></returns>
      double linearInterpolatedValue(int h1, int h2, double q1) {
         if (h1 == DEMNOVALUE)
            return q1 < .5 ? NOVALUED : // wenn dichter am NOVALUE, dann NOVALUE sonst gleiche Höhe wie der andere Punkt
                     h2 == DEMNOVALUE ? NOVALUED : h2;
         if (h2 == DEMNOVALUE)
            return q1 > .5 ? NOVALUED :
                     h1 == DEMNOVALUE ? NOVALUED : h1;
         return h1 + q1 * (h2 - h1);
      }

      /// <summary>
      /// der Wert für den Punkt P im umschließenden Rechteck (normierte Seitenlänge 1) aus 4 Eckpunkten wird interpoliert
      /// </summary>
      /// <param name="qx">Abstand P vom linken Rand des Rechtecks (Bruchteil 0..1)</param>
      /// <param name="qy">Abstand P vom unteren Rand des Rechtecks (Bruchteil 0..1)</param>
      /// <param name="hlt">Wert links oben</param>
      /// <param name="hrt">Wert rechts oben</param>
      /// <param name="hrb">Wert rechts unten</param>
      /// <param name="hlb">Wert links unten</param>
      /// <returns></returns>
      double interpolatedHeightInNormatedRectangle_New(double qx, double qy, int hlt, int hrt, int hrb, int hlb) {
         int novalue = 0;
         if (hlb == DEMNOVALUE)
            novalue++;
         if (hlt == DEMNOVALUE)
            novalue++;
         if (hrt == DEMNOVALUE)
            novalue++;
         if (hrb == DEMNOVALUE)
            novalue++;

         switch (novalue) {
            case 0: // bilinear, standard
               return (1 - qy) * (hlb + qx * (hrb - hlb)) + qy * (hlt + qx * (hrt - hlt));

            case 1:
               if (hlb == DEMNOVALUE) {            //    valid triangle \|

                  if (qx >= 1 - qy)
                     // z = z1 + (1 - nx) * (z3 - z1) + (1 - ny) * (z2 - z1)
                     return hrt + (1 - qx) * (hlt - hrt) + (1 - qy) * (hrb - hrt);

               } else if (hlt == DEMNOVALUE) {     //    valid triangle /|

                  if (qx <= qy)
                     // z = z1 + (1 - nx) * (z2 - z1) + ny * (z3 - z1)
                     return hrb + (1 - qx) * (hlb - hrb) + qy * (hrt - hrb);

               } else if (hrt == DEMNOVALUE) {     //    valid triangle |\

                  if (qx <= 1 - qy)
                     //z = z1 + nx * (z3 - z1) + ny * (z2 - z1)
                     return hlb + qx * (hrb - hlb) + qy * (hlt - hlb);

               } else if (hrb == DEMNOVALUE) {     //    valid triangle |/

                  if (qx <= qy)
                     // z = z1 + nx * (z2 - z1) + (1 - ny) * (z3 - z1)
                     return hlt + qx * (hrt - hlt) + (1 - qy) * (hlb - hlt);

               }
               break;

            case 2:
               if (hlb != DEMNOVALUE && hrt != DEMNOVALUE)  // diagonal
                  return (hlb + hrt) / 2.0;
               if (hlt != DEMNOVALUE && hrb != DEMNOVALUE)  // diagonal
                  return (hlt + hrb) / 2.0;
               return NOVALUED;

            default:
               return NOVALUED;
         }

         return NOVALUED;
      }

      //double InterpolatedHeightInNormatedRectangle_Old(double qx, double qy, int hlt, int hrt, int hrb, int hlb) {
      //   if (hlb == DEMNOVALUE ||
      //       hrt == DEMNOVALUE)
      //      return NOVALUED; // keine Berechnung möglich

      //   /* In welchem Dreieck liegt der Punkt? 
      //    *    oben  +-/
      //    *          |/
      //    *          
      //    *    unten  /|
      //    *          /-+
      //    */
      //   if (qy >= qx) { // oberes Dreieck aus hlb, hrt und hlt (Anstieg py/px ist größer als height/width)

      //      if (hlt == NOVALUED)
      //         return NOVALUED;

      //      // hlt als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
      //      hrt -= hlt;
      //      hlb -= hlt;
      //      qy -= 1;

      //      return hlt + qx * hrt - qy * hlb;

      //   } else { // unteres Dreieck aus hlb, hrb und hrt

      //      if (hrb == NOVALUED)
      //         return NOVALUED;

      //      // hrb als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
      //      hrt -= hrb;
      //      hlb -= hrb;
      //      qx -= 1;

      //      return hrb - qx * hlb + qy * hrt;
      //   }
      //}

      #endregion

      #region Cubic Interpolation (Catmull–Rom spline)

      /// <summary>
      /// 1-dimensionale kubische Interpolation mit Catmull–Rom Spline
      /// <para>
      /// Die Gleichung vereinfacht nur den folgenden Zusammenhang:
      /// 
      /// p0  p1   p2    p3
      /// -------------------------
      /// 0    1    0     0     q^0 
      /// -t   0    t     0     q^1 
      /// 2t  t-3  3-2t  -t     q^2 
      /// -t  2-t  t-2    t     q^3
      /// 
      /// üblich mit t=0.5 (auch mit 1 gesehen)
      /// 
      /// q^3 * (-0.5*p0 + 1.5*p1 - 1.5*p2.5*p3) +
      /// q^2 * (     p0 - 1.5*p1 + 2.5*p2 - 0.5*p3) +
      /// q^1 * (-0.5*p0          + 0.5*p2         ) +
      ///                      p1
      /// </para>
      /// </summary>
      /// <param name="p">4 Werte (bekannte Stützpunkte), die den Spline definieren (abgesehen von t).</param>
      /// <param name="q">Für diesen Wert (als Faktor 0..1) zwischen den Stützpunkten ist der Funktionswert gesucht.</param>
      /// <returns></returns>
      static double Dim1CubicInterpolation(double[] p, double q) {
         return p[1] + 0.5 * q * (p[2] - p[0] + q * (2 * p[0] - 5 * p[1] + 4 * p[2] - p[3] + q * (3 * (p[1] - p[2]) + p[3] - p[0])));
      }

      /// <summary>
      /// 2-dimensionale (bi)kubische Interpolation mit Catmull–Rom Spline
      /// </summary>
      /// <param name="p">4x4 Stützpunkte</param>
      /// <param name="qx">Faktor 0..1 des x-Wertes der Position der gesuchten Höhe</param>
      /// <param name="qy">Faktor 0..1 des y-Wertes der Position der gesuchten Höhe</param>
      /// <returns></returns>
      static double Dim2CubicInterpolation(double[][] p, double qx, double qy) {
         return Dim1CubicInterpolation(new double[] {
                                          Dim1CubicInterpolation(p[0], qy),   // Array der y-Werte für x=0
                                          Dim1CubicInterpolation(p[1], qy),
                                          Dim1CubicInterpolation(p[2], qy),
                                          Dim1CubicInterpolation(p[3], qy)},  // Array der y-Werte für x=1
                                       qx);
      }

      //double BiCubicInterpolation(int x, int y, double qx, double qy) {
      double BiCubicInterpolation(double lon, double lat) {
         double[][] p = new double[4][];


         return 0; // Dim2CubicInterpolation(p, qx, qy);
      }



      #endregion

      /// <summary>
      /// get the standard basefilename for this object with upper chars (without extension)
      /// </summary>
      /// <returns></returns>
      public string GetStandardBasefilename() {
         return GetStandardBasefilename((int)Math.Round(Left), (int)Math.Round(Bottom));
      }

      /// <summary>
      /// get the standard basefilename with upper chars (without extension)
      /// </summary>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <returns></returns>
      static public string GetStandardBasefilename(int left, int bottom) {
         string name;
         if (bottom >= 0)
            name = string.Format("N{0:d2}", bottom);
         else
            name = string.Format("S{0:d2}", -bottom);

         if (left >= 0)
            name += string.Format("E{0:d3}", left);
         else
            name += string.Format("W{0:d3}", -left);

         return name;
      }

      /// <summary>
      /// if not exists, you can compute with <see cref="ComputeHillShadeData"()/>
      /// </summary>
      public bool ExistsHillShadeData => hillshade != null;

      const double rearth = 6378.137;        // radius of earth in km

      /// <summary>
      /// distance between 2 Points
      /// <para>https://en.wikipedia.org/wiki/Haversine_formula</para>
      /// </summary>
      /// <param name="lat1">in degree</param>
      /// <param name="lon1">in degree</param>
      /// <param name="lat2">in degree</param>
      /// <param name="lon2">in degree</param>
      /// <returns>meters</returns>
      static double dist4Points(double lat1, double lon1, double lat2, double lon2) {
         lat1 *= Math.PI / 180;
         lat2 *= Math.PI / 180;
         double sin_dlat_half = Math.Sin((lat2 - lat1) / 2);
         double sin_dlon_half = Math.Sin((lon2 - lon1) * Math.PI / 360);
         double a = sin_dlat_half * sin_dlat_half + Math.Cos(lat1) * Math.Cos(lat2) * sin_dlon_half * sin_dlon_half;
         return 2000 * rearth * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      }

      const double degreesToRadians = Math.PI / 180.0;

      /// <summary>
      /// compute the hillshadedata
      /// </summary>
      /// <param name="azimut"></param>
      /// <param name="altitude"></param>
      /// <param name="scale"></param>
      public void ComputeHillShadeData(double azimut = 315, double altitude = 45, double scale = 20.0) {
         if (hillshade == null)
            hillshade = new byte[data.Length];

         altitude = Math.Max(0, Math.Min(altitude, 90));
         azimut = Math.Max(0, Math.Min(azimut, 360));

         HillShadeAzimut = azimut;
         HillShadeAltitude = altitude;
         HillShadeScale = scale;

         altitude *= degreesToRadians;
         azimut *= degreesToRadians;

         // nützliche Konstanten
         double sinalt = Math.Sin(altitude);
         double cosalt = Math.Cos(altitude);
         double sinaz = Math.Sin(azimut);
         double cosaz = Math.Cos(azimut);
         double cosalt_cosaz = cosalt * cosaz;
         double cosalt_sinaz = cosalt * sinaz;

         double latm = Bottom + (Top - Bottom) / 2;
         double ew_resm = dist4Points(latm, Left, latm, Right) / Columns;     /* w-e pixel resolution / pixel width in m */
         double ns_resm = -dist4Points(Top, 0, Bottom, 0) / Rows;             /* n-s pixel resolution / pixel height (negative value) in m */

         ew_resm /= scale;
         ns_resm /= scale;

         /* ------------------------------------------
          * Move a 3x3 window over each cell (#4)
          *              a b c
          *              d e f
          *              g h i
          */
         int a = 0, b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, h = 0, i = 0;
         bool inner = false;
         for (int row = 0; row < Rows; row++) {
            for (int col = 0; col < Columns; col++) {
               double cang = 0;

               // fill 3x3 window for e
               e = fastget(row, col);
               if (row == 0) {                           // top
                  if (col == 0) {                        // top-left
                     f = fastget(row, col + 1);
                     h = fastget(row + 1, col);
                     i = fastget(row + 1, col + 1);
                     a = 2 * e - i;
                     b = 2 * e - h;
                     c = 2 * f - i;
                     d = 2 * e - f;
                     g = 2 * h - i;
                  } else if (col == Columns - 1) {       // top-right
                     d = fastget(row, col - 1);
                     g = fastget(row + 1, col - 1);
                     h = fastget(row + 1, col);
                     a = 2 * d - g;
                     b = 2 * e - h;
                     c = 2 * e - g;
                     f = 2 * e - d;
                     i = 2 * h - g;
                  } else {                               // top-edge
                     d = fastget(row, col - 1);
                     f = fastget(row, col + 1);
                     g = fastget(row + 1, col - 1);
                     h = fastget(row + 1, col);
                     i = fastget(row + 1, col + 1);
                     a = 2 * d - g;
                     b = 2 * e - h;
                     c = 2 * f - i;
                  }
                  inner = false;
               } else if (row == Rows - 1) {             // bottom
                  if (col == 0) {                        // bottom-left
                     b = fastget(row - 1, col);
                     c = fastget(row - 1, col + 1);
                     f = fastget(row, col + 1);
                     a = 2 * b - c;
                     d = 2 * e - f;
                     g = 2 * e - c;
                     h = 2 * e - b;
                     i = 2 * f - c;
                  } else if (col == Columns - 1) {       // bottom-right
                     a = fastget(row - 1, col - 1);
                     b = fastget(row - 1, col);
                     d = fastget(row, col - 1);
                     g = 2 * d - a;
                     h = 2 * e - b;
                     c = 2 * b - a;
                     f = 2 * e - d;
                     i = 2 * e - a;
                  } else {                               // bottom-edge
                     a = fastget(row - 1, col - 1);
                     b = fastget(row - 1, col);
                     c = fastget(row - 1, col + 1);
                     d = fastget(row, col - 1);
                     f = fastget(row, col + 1);
                     g = 2 * d - a;
                     h = 2 * e - b;
                     i = 2 * f - c;
                  }
                  inner = false;
               } else {                                  // not top or bottom
                  if (col == 0) {                        // not top or bottom, left
                     b = fastget(row - 1, col);
                     c = fastget(row - 1, col + 1);
                     f = fastget(row, col + 1);
                     h = fastget(row + 1, col);
                     i = fastget(row + 1, col + 1);
                     a = 2 * b - c;
                     d = 2 * e - f;
                     g = 2 * h - i;
                     inner = false;
                  } else if (col == Columns - 1) {       // not top or bottom, right
                     a = fastget(row - 1, col - 1);
                     b = fastget(row - 1, col);
                     d = fastget(row, col - 1);
                     g = fastget(row + 1, col - 1);
                     h = fastget(row + 1, col);
                     c = 2 * b - a;
                     f = 2 * e - d;
                     i = 2 * h - g;
                     inner = false;
                  } else {                               // inner

                     if (inner) {      // 3x3 window is moved 1 step left and 6 values are valid
                        c = fastget(row - 1, col + 1);
                        f = fastget(row, col + 1);
                        i = fastget(row + 1, col + 1);
                     } else {
                        a = fastget(row - 1, col - 1);
                        b = fastget(row - 1, col);
                        c = fastget(row - 1, col + 1);

                        d = fastget(row, col - 1);
                        f = fastget(row, col + 1);

                        g = fastget(row + 1, col - 1);
                        h = fastget(row + 1, col);
                        i = fastget(row + 1, col + 1);
                     }

                     inner = true;
                  }
               }

               // Check if window has null value
               if (!(a == DEMNOVALUE ||
                     b == DEMNOVALUE ||
                     c == DEMNOVALUE ||
                     d == DEMNOVALUE ||
                     e == DEMNOVALUE ||
                     f == DEMNOVALUE ||
                     g == DEMNOVALUE ||
                     h == DEMNOVALUE ||
                     i == DEMNOVALUE)) {

                  // Zevenbergen - Thorne Alg.
                  //double dx = (f - d) / (2 * ew_resm);
                  //double dy = (b - h) / (2 * ns_resm);

                  // Horn's Alg.
                  double dx = (a + 2 * d + g - c - 2 * f - i) / (8 * ew_resm);
                  double dy = (g + 2 * h + i - a - 2 * b - c) / (8 * ns_resm);

                  cang = (sinalt - dy * cosalt_cosaz + dx * cosalt_sinaz) / Math.Sqrt(1 + dx * dx + dy * dy);

               }

               if (inner) {
                  a = b; b = c;
                  d = e; e = f;
                  g = h; h = i;
               }

               hillshade[row * Columns + col] = (byte)Math.Round(((cang + 1) / 2) * 255);     // cang -1 .. +1 auf 0 .. 255 normieren

            }
         }
#if WRITEBITMAPS
         writeHillshadeBitmap();
         writeHeightBitmap();
#endif
      }

      /*
      Variante 1: (org)
         +  :  - 
         +2 :  -2
         +  :  - 
            dzdx = ((a + 2*d + g) - (c + 2*f + i))/8*kernelsize

         -  -2 -
         :  :  :
         +  +2 +
            dzdy = ((g + 2*h + i) - (a + 2*b + c))/8*kernelsize;

         slope = Math.PI / 2 - Math.Atan(Math.Sqrt(dzdx * dzdx + dzdy * dzdy));
         azimut =  Math.Atan2(dzdx, dzdy);

         cang = sin(alt) * sin(slope) +
                cos(alt) * cos(slope) * cos((az-Math.PI/2) - aspect);


Unoptimized formulas are :
slope = atan(sqrt(x*x + y*y));
aspect = atan2(y,x);
cang = sin(alt) * cos(slope) + cos(alt) * sin(slope) * cos(az - M_PI/2 - aspect);

We can avoid a lot of trigonometric computations:

since cos(atan(x)) = 1 / sqrt(1+x^2)           ==> cos(slope) = 1 / sqrt(1+ x*x+y*y)
and sin(atan(x)) = x / sqrt(1+x^2)           ==> sin(slope) = sqrt(x*x + y*y) / sqrt(1+ x*x+y*y)

and cos(az - M_PI/2 - aspect)
= cos(-az + M_PI/2 + aspect)
= cos(M_PI/2 - (az - aspect))
= sin(az - aspect)
= -sin(aspect-az)

==> cang = (sin(alt) - cos(alt) * sqrt(x*x + y*y)  * sin(aspect-as)) / sqrt(1+ x*x+y*y)

But:
sin(aspect - az) = sin(aspect)*cos(az) - cos(aspect)*sin(az))

and as sin(aspect)=sin(atan2(y,x)) = y / sqrt(xx_plus_yy)
and cos(aspect)=cos(atan2(y,x)) = x / sqrt(xx_plus_yy)

sin(aspect - az) = (y * cos(az) - x * sin(az)) / sqrt(xx_plus_yy)

so we get a final formula with just one transcendental function (reciprocal of square root):

cang = (psData->sin_altRadians -
(y * psData->cos_az_mul_cos_alt_mul_z -
x * psData->sin_az_mul_cos_alt_mul_z)) /
sqrt(1 + psData->square_z * xx_plus_yy);





      //double cang = sinalt * Math.Sin(slope) +
      //              cosalt * Math.Cos(slope) * Math.Cos(azimut2 - Math.Atan2(X, Y));
      // 5x trigonometr. Fkt., 1x Wurzel
      // analog:
      cang = (sinalt + cosalt * Math.Sqrt(dzdx * dzdx + dzdy * dzdy) * Math.Sin(azimut - Math.Atan2(dzdx, dzdy))) / Math.Sqrt(1 + dzdx * dzdx + dzdy * dzdy);



      Variante 2
         -  -2 -
         :  :  :
         +  +2 +
            dzdx = ((g + 2*h + i) - (a + 2*b + c))/8*kernelsize

         -  :  + 
         -2 :  +2
         -  :  + 
            dzdy = ((c + 2*f + i) - (a + 2*d + g))/8*kernelsize

            slope = Math.Atan(Math.Sqrt(dzdx * dzdx + dzdy * dzdy));
            aspect = Math.Atan2(dzdy, dzdx);
            L = cos(90° - alt) * cos(slope) + sin(90° - alt) * sin(slope) * cos(azimut - aspect)


      Variante 3: Zevenbergen-Thorne (?)
         :  :  : 
         -  :  +
         :  :  :
            dzdx = (f - d)/2;

         :  +  : 
         :  :  :
         :  -  :
            dzdy = (b - h)/2;


      Variante 4: Horn (?)
         -  :  + 
         -2 :  +2
         -  :  + 
            dzdx = ((g + 2*h + i) - (a + 2*b + c))/8;

         +  +2 +
         :  :  :
         -  -2 -
            dzdy = ((a + 2*b + c) - (g + 2*h + i))/8;

            slope = Math.Atan(Math.Sqrt(dzdx * dzdx + dzdy * dzdy));
            aspect = Math.Atan2(dzdy, dzdx);
            L = cos(90° - alt) * cos(slope) + sin(90° - alt) * sin(slope) * cos(90° - azimut - aspect)


      Variante 5: ArcGis
         -  :  + 
         -2 :  +2
         -  :  + 
            dzdx = ((c + 2*f + i) - (a + 2*d + g))/8*kernelsize

         -  -2 -
         :  :  :
         +  +2 +
            dzdy = ((g + 2*h + i) - (a + 2*b + c))/8*kernelsize

            slope = Math.Atan(Math.Sqrt(dzdx * dzdx + dzdy * dzdy));
            aspect:  if (dzdx != 0) {
                        if (dzdy >= 0)
                           aspect = Math.Atan2(dzdy, dzdx) 
                        else
                           aspect = 2 * Math.Pi + Math.Atan2(dzdy, dzdx);
                     } else {
                        if (dzdy > 0)
                           aspect = Math.Pi / 2;         // 90°
                        else if (dzdy < 0)
                           aspect = 5 * Math.Pi / 2;     // 270°
                        else
                           ? 0 / 0
                     }
            L = cos(90° - alt) * cos(slope) + sin(90° - alt) * sin(slope) * cos(azimut - aspect)


      Ritter's Alg.
         :  :  : 
         +  :  -
         :  :  :
            dzdx = (d - f);

         :  -  : 
         :  :  :
         :  +  :
            dzdy = (h - b);

            S = Math.Sqrt(dzdx * dzdx + dzdy * dzdy))/2*d;
            D = Math.Atan2(dzdy, dzdx);

      Horn's Alg.
         +  :  - 
         +2 :  -2
         +  :  - 
            dzdx = (a + 2*b + c) - (g + 2*h + i);

         -  -2 -
         :  :  :
         +  +2 +
            dzdy = (g + 2*h + i) - (a + 2*b + c);

            S = Math.Sqrt(dzdx * dzdx + dzdy * dzdy)/8*d;
            D = Math.Atan2(dzdy, dzdx);



       */

      /// <summary>
      /// get a value (coordinate origin left-top)
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      byte fastgetShadingValue(int row, int col) => (byte)(hillshade != null ? hillshade[row * Columns + col] : 0);

      /// <summary>
      /// get a value (coordinate origin left-bottom)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      virtual protected byte fastgetShadingValue4XY(int x, int y) => (byte)(hillshade != null ? hillshade[(Rows - 1 - y) * Columns + x] : 0);

      /// <summary>
      /// get the interpolated value
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      virtual public byte InterpolatedShadingValue(double lon, double lat, InterpolationType intpol) {
         double h = NOVALUED;

         lon -= Left;
         lat -= Bottom; // Koordinaten auf die Ecke links unten bezogen

         switch (intpol) {
            case InterpolationType.standard:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {
                  // x-y-Index des Eckpunktes links unten des umschließenden Quadrats bestimmen
                  int x = (int)(lon / DeltaX);
                  int y = (int)(lat / DeltaY);
                  if (x == Columns - 1) // liegt auf rechtem Rand
                     x--;
                  if (y == Rows - 1) // liegt auf oberem Rand
                     y--;

                  // lon/lat jetzt bzgl. der Ecke links-unten des umschließenden Quadrats bilden (0 .. <Delta)
                  double delta_lon = lon - x * DeltaX;
                  double delta_lat = lat - y * DeltaY;

                  if (delta_lon == 0) {            // linker Rand
                     if (delta_lat == 0)
                        h = fastgetShadingValue4XY(x, y);          // Eckpunkt links unten
                     else if (delta_lat >= DeltaY)
                        h = fastgetShadingValue4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(fastgetShadingValue4XY(x, y),
                                                    fastgetShadingValue4XY(x, y + 1),
                                                    delta_lat / DeltaY);
                  } else if (delta_lon >= DeltaX) { // rechter Rand (eigentlich nicht möglich)
                     if (delta_lat == 0)
                        h = fastgetShadingValue4XY(x + 1, y);      // Eckpunkt rechts unten
                     else if (delta_lat >= DeltaY)
                        h = fastgetShadingValue4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(fastgetShadingValue4XY(x + 1, y),
                                                    fastgetShadingValue4XY(x + 1, y + 1),
                                                    delta_lat / DeltaY);
                  } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
                     h = linearInterpolatedValue(fastgetShadingValue4XY(x, y),
                                                 fastgetShadingValue4XY(x + 1, y),
                                                 delta_lon / DeltaX);
                  } else if (delta_lat >= DeltaY) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
                     h = linearInterpolatedValue(fastgetShadingValue4XY(x, y + 1),
                                                 fastgetShadingValue4XY(x + 1, y + 1),
                                                 delta_lon / DeltaX);

                  } else {                         // Punkt innerhalb des Rechtecks

                     //int leftbottom, rightbottom, righttop, lefttop;
                     //GetShadingValue4XYSquare(x, y, out leftbottom, out rightbottom, out righttop, out lefttop);  // etwas schneller als die obere Version

                     if (hillshade != null) {
                        int idx = (Rows - 1 - y) * Columns; // Anfang der unteren Zeile
                        idx += x;
                        int leftbottom = hillshade[idx++];
                        int rightbottom = hillshade[idx];
                        idx -= Columns;
                        int righttop = hillshade[idx--];
                        int lefttop = hillshade[idx];
                        h = interpolatedHeightInNormatedRectangle_New(delta_lon / DeltaX,
                                                                      delta_lat / DeltaY,
                                                                      lefttop,
                                                                      righttop,
                                                                      rightbottom,
                                                                      leftbottom);
                     }
                  }
               }
               break;

            case InterpolationType.bicubic_catmull_rom:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {

                  if (Columns >= 4 && Rows >= 4) {
                     // x-y-Index des Punktes link-unterhalb bestimmen
                     int x = (int)(lon / DeltaX);
                     int y = (int)(lat / DeltaY);
                     // x-y-Index des Punktes link-unterhalb dieses Punktes bestimmen
                     x--;
                     y--;
                     if (x < 0)
                        x = 0;
                     else if (x >= Columns - 4)
                        x = Columns - 4;
                     if (y < 0)
                        y = 0;
                     else if (y >= Rows - 4)
                        y = Rows - 4;

                     double[][] p = new double[4][];
                     for (int i = 0; i < 4; i++)
                        p[i] = new double[] { fastgetShadingValue4XY(x, y + 3-i),
                                              fastgetShadingValue4XY(x + 1, y + 3-i),
                                              fastgetShadingValue4XY(x + 2, y + 3-i),
                                              fastgetShadingValue4XY(x + 3, y + 3-i) };

                     h = Dim2CubicInterpolation(p, lon / DeltaX - x, lat / DeltaY - y);
                  } else
                     h = InterpolatedHeight(lon + Left, lat + Bottom, InterpolationType.standard);
               }
               break;
         }

         return (byte)Math.Round(h);
      }

#if WRITEBITMAPS
      void writeHillshadeBitmap() {
         if (hillshade != null) {
            try {
               string filename = GetStandardBasefilename() + "_hs.png";
               System.Drawing.Bitmap bm = new System.Drawing.Bitmap(Columns, Rows);
               for (int x = 0; x < Columns; x++)
                  for (int y = 0; y < Rows; y++) {
                     int v = GetShadingValue(y, x);
                     bm.SetPixel(x, y, System.Drawing.Color.FromArgb(v, v, v));
                  }
               if (System.IO.File.Exists(filename))
                  System.IO.File.Delete(filename);
               bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("writeHillshadeBitmap: " + ex.Message);
            }
         }
      }

      void writeHeightBitmap() {
         try {
            string filename = GetStandardBasefilename() + ".png";
            int min = Minimum;
            int delta = Maximum - Minimum;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(Columns, Rows);
            for (int x = 0; x < Columns; x++)
               for (int y = 0; y < Rows; y++) {
                  int v = Get(y, x);
                  if (v != DEMNOVALUE) {
                     if (delta > 0)
                        v = (int)(255.0 * (v - min) / delta);
                     bm.SetPixel(x, y, System.Drawing.Color.FromArgb(v, v, v));
                  }
               }
            if (System.IO.File.Exists(filename))
               System.IO.File.Delete(filename);
            bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
         } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine("writeHeightBitmap: " + ex.Message);
         }
      }
#endif

      #region Implementierung der IDisposable-Schnittstelle

      ~DEM1x1() {
         Dispose(false);
      }

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

               hillshade = null;

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

      public override string ToString() {
         return string.Format("DEM1x1: {0}{1}° {2}{3}°, {4}x{5}, {6}m..{7}m, unvalid values: {8} ({9}%)",
                              Bottom >= 0 ? "N" : "S",
                              Bottom >= 0 ? Bottom : -Bottom,
                              Left >= 0 ? "E" : "W",
                              Left >= 0 ? Left : -Left,
                              Rows,
                              Columns,
                              Minimum,
                              Maximum,
                              NotValid,
                              (100.0 * NotValid) / (Rows * Columns));
      }

   }
}
