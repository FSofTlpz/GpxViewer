//#define WRITEBITMAPS
using System;

namespace FSofTUtils.Geography.DEM {

   /// <summary>
   /// This class hold the DEM data for 1°x1°. The number of columns can be different from the number of rows. 
   /// You need a derived class for filling in the data.
   /// </summary>
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
      /// left border
      /// </summary>
      public double Left { get; protected set; }
      /// <summary>
      /// lower border
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
      /// horizontal distance between 2 points
      /// </summary>
      public double DeltaX { get { return Width / (Columns - 1); } }
      /// <summary>
      /// vertical distance between 2 points
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
      /// upper border
      /// </summary>
      public double Top { get { return Bottom + Height; } }
      /// <summary>
      /// right border
      /// </summary>
      public double Right { get { return Left + Width; } }
      /// <summary>
      /// width
      /// </summary>
      public double Width { get { return 1; } }
      /// <summary>
      /// height
      /// </summary>
      public double Height { get { return 1; } }

      /// <summary>
      /// dem data-array
      /// </summary>
      protected short[] data;

      /// <summary>
      /// data for hillshading
      /// </summary>
      protected byte[] hillshade;


      public DEM1x1() {
         Left = 0;
         Bottom = 0;
         data = new short[0];
         hillshade = null;
      }

      public DEM1x1(double left, double bottom) {
         Left = left;
         Bottom = bottom;
         data = new short[0];
      }


      /// <summary>
      /// set the data array
      /// </summary>
      abstract public void SetDataArray();


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
      public bool HasValidValues() {
         return (Minimum != DEMNOVALUE || Maximum != DEMNOVALUE);
      }

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
                        h = GetHeight4XY(x, y);          // Eckpunkt links unten
                     else if (delta_lat >= DeltaY)
                        h = GetHeight4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(Get4XY(x, y),
                                                     Get4XY(x, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lon >= DeltaX) { // rechter Rand (eigentlich nicht möglich)
                     if (delta_lat == 0)
                        h = GetHeight4XY(x + 1, y);      // Eckpunkt rechts unten
                     else if (delta_lat >= DeltaY)
                        h = GetHeight4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(Get4XY(x + 1, y),
                                                     Get4XY(x + 1, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
                     h = linearInterpolatedValue(Get4XY(x, y),
                                                  Get4XY(x + 1, y),
                                                  delta_lon / DeltaX);
                  } else if (delta_lat >= DeltaY) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
                     h = linearInterpolatedValue(Get4XY(x, y + 1),
                                                  Get4XY(x + 1, y + 1),
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
                        p[i] = new double[] { Get4XY(x, y + 3-i),
                                              Get4XY(x + 1, y + 3-i),
                                              Get4XY(x + 2, y + 3-i),
                                              Get4XY(x + 3, y + 3-i) };

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

      double GetHeight4XY(int x, int y) {
         int h = Get4XY(x, y);
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
      /// q^3 * (-0.5*p0 + 1.5*p1 - 1.5*p2 + 0.5*p3) +
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
         if (left >= 0)
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
      public bool ExistsHillShadeData {
         get {
            return hillshade != null;
         }
      }

      const double degreesToRadians = Math.PI / 180.0;

      /// <summary>
      /// compute the hillshadedata
      /// </summary>
      /// <param name="azimut"></param>
      /// <param name="altitude"></param>
      /// <param name="scale"></param>
      /// <param name="z"></param>
      public void ComputeHillShadeData(double azimut = 315, double altitude = 45, double scale = 1.0, double z = 1.0) {
         if (hillshade == null)
            hillshade = new byte[data.Length];

         azimut *= degreesToRadians;
         altitude *= degreesToRadians;
         double sinalt = Math.Sin(altitude);
         double cosalt = Math.Cos(altitude);

         double ewres = DeltaX; /* w-e pixel resolution / pixel width */
         double nsres = -DeltaY; /* n-s pixel resolution / pixel height (negative value) */

         int[] win = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

         /* ------------------------------------------
          * Move a 3x3 window over each cell 
          * (where the cell in question is #4)
          *                 0 1 2
          *                 -----
          *              0| 0 1 2
          *              1| 3 4 5
          *              2| 6 7 8
          *
          */
         for (int row = 0; row < Rows; row++) {
            for (int col = 0; col < Columns; col++) {
               if (row == 0) { // oberer Rand
                  if (col == 0) {  // Ecke link oben
                                   //poBand->RasterIO(GF_Read, col, row, 2, 2, win, 2, 2, GDT_Float32, 0, 0);
                                   //                          int nXOff
                                   //                             int nYOff, 
                                   //                                int nXSize, 
                                   //                                   int nYSize, void* pData, int nBufXSize, int nBufYSize,
                     win[0] = Get(0, 0);
                     win[1] = Get(0, 1);

                     win[2] = Get(1, 0);
                     win[3] = Get(1, 1);

                     win[8] = win[3];
                     win[5] = win[2];
                     win[7] = win[1];
                     win[4] = win[0];
                     win[0] = 2 * win[4] - win[8];
                     win[3] = 2 * win[4] - win[5];
                     win[6] = 2 * win[7] - win[8];
                     win[1] = 2 * win[4] - win[7];
                     win[1] = 2 * win[5] - win[8];
                  } else if (col == Columns - 1) {   // Ecke rechts oben
                                                     //poBand->RasterIO(GF_Read, col - 1, row, 2, 2, win, 2, 2, GDT_Float32, 0, 0);
                     win[0] = Get(0, col - 1);
                     win[1] = Get(0, col);

                     win[2] = Get(1, col - 1);
                     win[3] = Get(1, col);

                     win[7] = win[3];
                     win[4] = win[2];
                     win[6] = win[1];
                     win[3] = win[0];
                     win[0] = 2 * win[3] - win[6];
                     win[1] = 2 * win[4] - win[7];
                     win[1] = 2 * win[4] - win[8];
                     win[5] = 2 * win[4] - win[3];
                     win[8] = 2 * win[7] - win[6];
                  } else { // Rand oben
                           //poBand->RasterIO(GF_Read, col - 1, row, 3, 2, win, 3, 2, GDT_Float32, 0, 0);
                     win[0] = Get(0, col - 1);
                     win[1] = Get(0, col);
                     win[2] = Get(0, col + 1);

                     win[3] = Get(1, col - 1);
                     win[4] = Get(1, col);
                     win[5] = Get(1, col + 1);

                     win[8] = win[5];
                     win[5] = win[4];
                     win[7] = win[3];
                     win[4] = win[2];
                     win[6] = win[1];
                     win[3] = win[0];
                     win[0] = 2 * win[3] - win[6];
                     win[1] = 2 * win[4] - win[7];
                     win[1] = 2 * win[5] - win[8];
                  }
               } else if (row == Rows - 1) { // unterer Rand
                  if (col == 0) { // Ecke links unten
                                  //poBand->RasterIO(GF_Read, col, row - 1, 2, 2, win, 2, 2, GDT_Float32, 0, 0);
                     win[0] = Get(row - 1, col);
                     win[1] = Get(row - 1, col + 1);

                     win[2] = Get(row, col);
                     win[3] = Get(row, col + 1);

                     win[5] = win[3];
                     win[1] = win[2];
                     win[4] = win[1];
                     win[1] = win[0];
                     win[0] = 2 * win[1] - win[1];
                     win[3] = 2 * win[4] - win[5];
                     win[6] = 2 * win[4] - win[1];
                     win[7] = 2 * win[4] - win[1];
                     win[8] = 2 * win[5] - win[1];
                  } else if (col == Columns - 1) { // Ecke rechts unten
                                                   //poBand->RasterIO(GF_Read, col - 1, row - 1, 2, 2, win, 2, 2, GDT_Float32, 0, 0);
                     win[0] = Get(row - 1, col - 1);
                     win[1] = Get(row - 1, col);

                     win[2] = Get(row, col - 1);
                     win[3] = Get(row, col);

                     win[4] = win[3];
                     win[1] = win[2];
                     win[3] = win[1];
                     win[0] = win[0];
                     win[6] = 2 * win[3] - win[0];
                     win[7] = 2 * win[4] - win[1];
                     win[1] = 2 * win[1] - win[0];
                     win[5] = 2 * win[4] - win[3];
                     win[8] = 2 * win[4] - win[0];
                  } else { // Rand unten
                           //poBand->RasterIO(GF_Read, col - 1, row - 1, 3, 2, win, 3, 2, GDT_Float32, 0, 0);
                     win[0] = Get(row - 1, col - 1);
                     win[1] = Get(row - 1, col);
                     win[2] = Get(row - 1, col + 1);

                     win[3] = Get(row, col - 1);
                     win[4] = Get(row, col);
                     win[5] = Get(row, col + 1);

                     win[5] = win[5];
                     win[1] = win[4];
                     win[4] = win[3];
                     win[1] = win[2];
                     win[3] = win[1];
                     win[0] = win[0];
                     win[6] = 2 * win[3] - win[0];
                     win[7] = 2 * win[4] - win[1];
                     win[8] = 2 * win[5] - win[1];
                  }
               } else { // zwischen unten und oben
                  if (col == 0) { // Rand links
                                  //poBand->RasterIO(GF_Read, col, row - 1, 2, 3, win, 2, 3, GDT_Float32, 0, 0);
                     win[0] = Get(row - 1, col);
                     win[1] = Get(row - 1, col + 1);

                     win[2] = Get(row, col);
                     win[3] = Get(row, col + 1);

                     win[4] = Get(row + 1, col);
                     win[5] = Get(row + 1, col + 1);

                     win[8] = win[5];
                     win[5] = win[4];
                     win[1] = win[3];
                     win[7] = win[2];
                     win[4] = win[1];
                     win[1] = win[0];
                     win[0] = 2 * win[1] - win[1];
                     win[3] = 2 * win[4] - win[5];
                     win[6] = 2 * win[7] - win[8];
                  } else if (col == Columns - 1) { // Rand rechts
                                                   //poBand->RasterIO(GF_Read, col - 1, row - 1, 2, 3, win, 2, 3, GDT_Float32, 0, 0);
                     win[0] = Get(row - 1, col - 1);
                     win[1] = Get(row - 1, col);

                     win[2] = Get(row, col - 1);
                     win[3] = Get(row, col);

                     win[4] = Get(row + 1, col - 1);
                     win[5] = Get(row + 1, col);

                     win[7] = win[5];
                     win[4] = win[4];
                     win[1] = win[3];
                     win[6] = win[2];
                     win[3] = win[1];
                     win[0] = win[0];
                     win[1] = 2 * win[1] - win[0];
                     win[5] = 2 * win[4] - win[3];
                     win[8] = 2 * win[7] - win[6];
                  } else { // innerhalb
                           // Read in 3x3 window
                           //poBand->RasterIO(GF_Read, col - 1, row - 1, 3, 3, win, 3, 3, GDT_Float32, 0, 0);

                     win[0] = Get(row - 1, col - 1);
                     win[1] = Get(row - 1, col);
                     win[2] = Get(row - 1, col + 1);

                     win[3] = Get(row, col - 1);
                     win[4] = Get(row, col);
                     win[5] = Get(row, col + 1);

                     win[6] = Get(row + 1, col - 1);
                     win[7] = Get(row + 1, col);
                     win[8] = Get(row + 1, col + 1);
                  }
               }

               double cang = 0;

               // Check if window has null value
               bool containsNull = false;
               for (int n = 0; n <= 8; n++) {
                  if (win[n] == DEMNOVALUE) {
                     containsNull = true;
                     break;
                  }
               }

               if (!containsNull) {
                  double X = ((z * win[0] + z * win[3] + z * win[3] + z * win[6]) -
                              (z * win[2] + z * win[5] + z * win[5] + z * win[8])) / (8.0 * ewres * scale);
                  double Y = ((z * win[6] + z * win[7] + z * win[7] + z * win[8]) -
                              (z * win[0] + z * win[1] + z * win[1] + z * win[2])) / (8.0 * nsres * scale);

                  //double slope = Math.PI / 2 - Math.Atan(Math.Sqrt(X * X + Y * Y));
                  //double cang = sinalt * Math.Sin(slope) +
                  //              cosalt * Math.Cos(slope) * Math.Cos(azimut2 - Math.Atan2(X, Y));
                  // 5x trigonometr. Fkt., 1x Wurzel
                  // analog:
                  cang = (sinalt + cosalt * Math.Sqrt(X * X + Y * Y) * Math.Sin(azimut - Math.Atan2(X, Y))) / Math.Sqrt(1 + X * X + Y * Y);
                  // -> 2x trigonometr. Fkt., 2x Wurzel
               }

               setShadingValue(row, col, (byte)Math.Round(((cang + 2) / 4) * 255));
            }
         }
#if WRITEBITMAPS
         writeHillshadeBitmap();
         writeHeightBitmap();
#endif
      }


      /// <summary>
      /// get a value (coordinate origin left-top)
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      public byte GetShadingValue(int row, int col) {
         if (hillshade == null ||
             row < 0 || Rows <= row ||
             col < 0 || Columns <= col)
            throw new Exception(string.Format("no data or ({0}, {1}) unvalid row and/or column ({2}x{3})", col, row, Columns, Rows));
         return hillshade[row * Columns + col];
      }

      /// <summary>
      /// get a value (coordinate origin left-bottom)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public byte GetShadingValue4XY(int x, int y) {
         return GetShadingValue(Rows - 1 - y, x);
      }

      /// <summary>
      /// get a value (coordinate origin left-top)
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <param name="value"></param>
      void setShadingValue(int row, int col, byte value) {
         hillshade[row * Columns + col] = value;
      }

      /// <summary>
      /// get the interpolated value
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public byte InterpolatedShadingValue(double lon, double lat, InterpolationType intpol) {
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
                        h = GetShadingValue4XY(x, y);          // Eckpunkt links unten
                     else if (delta_lat >= DeltaY)
                        h = GetShadingValue4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(GetShadingValue4XY(x, y),
                                                    GetShadingValue4XY(x, y + 1),
                                                    delta_lat / DeltaY);
                  } else if (delta_lon >= DeltaX) { // rechter Rand (eigentlich nicht möglich)
                     if (delta_lat == 0)
                        h = GetShadingValue4XY(x + 1, y);      // Eckpunkt rechts unten
                     else if (delta_lat >= DeltaY)
                        h = GetShadingValue4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
                     else
                        h = linearInterpolatedValue(GetShadingValue4XY(x + 1, y),
                                                    GetShadingValue4XY(x + 1, y + 1),
                                                    delta_lat / DeltaY);
                  } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
                     h = linearInterpolatedValue(GetShadingValue4XY(x, y),
                                                 GetShadingValue4XY(x + 1, y),
                                                 delta_lon / DeltaX);
                  } else if (delta_lat >= DeltaY) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
                     h = linearInterpolatedValue(GetShadingValue4XY(x, y + 1),
                                                 GetShadingValue4XY(x + 1, y + 1),
                                                 delta_lon / DeltaX);

                  } else {                         // Punkt innerhalb des Rechtecks

                     //int leftbottom, rightbottom, righttop, lefttop;
                     //GetShadingValue4XYSquare(x, y, out leftbottom, out rightbottom, out righttop, out lefttop);  // etwas schneller als die obere Version

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
                        p[i] = new double[] { GetShadingValue4XY(x, y + 3-i),
                                              GetShadingValue4XY(x + 1, y + 3-i),
                                              GetShadingValue4XY(x + 2, y + 3-i),
                                              GetShadingValue4XY(x + 3, y + 3-i) };

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

               data = null;
               hillshade = null;

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

      public override string ToString() {
         return string.Format("DEM1x1: {0}{1}° {2}{3}°, {4}x{5}, {6}m..{7}m, unvalid values: {8} ({9}%)",
            Bottom >= 0 ? "N" : "S", Bottom >= 0 ? Bottom : -Bottom,
            Left >= 0 ? "E" : "W", Left >= 0 ? Left : -Left,
            Rows, Columns,
            Minimum, Maximum,
            NotValid, (100.0 * NotValid) / (Rows * Columns));
      }

   }
}
