using ExifLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GpxViewer {
   class ExifGeo {

      /* https://github.com/oozcitak/exiflibrary
       * http://oozcitak.github.io/exiflibrary/articles/ReadFile.html
       * 
       * 
         GPSImgDirection - 237.869995
         GPSImgDirectionRef - Magnetic direction
         GPSLongitude - 13  5  25.989475 (13.090553)
         GPSLongitudeRef - E
         GPSLatitude - 54  18  56.911766 (54.315809)
         GPSLatitudeRef - N
       */

      ImageFile data;


      public ExifGeo(string filename) {
         data = ImageFile.FromFile(filename);
      }

      public DateTime GetDateTime() {
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.DateTime);
         if (prop != null)
            return (prop as ExifDateTime).Value;
         return DateTime.MinValue;
      }

      public DateTime GetDateTimeOriginal() {
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.DateTimeOriginal);
         if (prop != null)
            return (prop as ExifDateTime).Value;
         return DateTime.MinValue;
      }

      public double GetLat() {
         return readExifLonOrLat(false);
      }

      public double GetLon() {
         return readExifLonOrLat(true);
      }

      public bool SetLatLon(double lat, double lon, bool gpsoverwrite) {
         return setLonOrLat(lon, true, gpsoverwrite) &&
                setLonOrLat(lat, false, gpsoverwrite);
      }

      public double GetDirection() {
         double v = double.NaN;
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.GPSImgDirection);
         if (prop != null)
            v = (double)(prop as ExifURational).Value;
         return v;
      }

      public bool SetDirection(double v, bool gpsoverwrite) {
         bool set = false;
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.GPSImgDirection);
         if (prop == null) {
            data.Properties.Set(ExifTag.GPSImgDirection, v);
            set = true;
         } else {
            if (gpsoverwrite) {
               prop.Value = new MathEx.UFraction32(v);
               set = true;
            }
         }
         return set;
      }

      /// <summary>
      /// entfernt, falls vorhanden, alle Lat/Lon-Tags
      /// </summary>
      /// <param name="data"></param>
      /// <returns>false, falls nicht vorhanden</returns>
      public bool RemoveGeoLocation() {
         bool changed = false;

         if (removeExifProperty(ExifTag.GPSLongitude))
            changed = true;
         if (removeExifProperty(ExifTag.GPSLatitude))
            changed = true;
         if (removeExifProperty(ExifTag.GPSLongitudeRef))
            changed = true;
         if (removeExifProperty(ExifTag.GPSLatitudeRef))
            changed = true;

         return changed;
      }

      public string GetUserComment() {
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.UserComment);
         return prop?.Value as string;
      }

      public bool SetUserComment(string txt) {
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == ExifTag.UserComment);
         if (string.IsNullOrEmpty(txt)) {
            if (prop != null) {
               data.Properties.Remove(prop);
               return true;
            }
         } else {
            if (prop?.Value as string == txt)
               return false;
            if (prop != null)
               data.Properties.Remove(prop);
            data.Properties.Set(ExifTag.UserComment, txt, System.Text.Encoding.Unicode);
            return true;
         }
         return false;
      }

      public void SaveImage(string imgfile) {
         string tmp = imgfile + "~~~";
         data.Save(tmp);
         File.Delete(imgfile);
         File.Move(tmp, imgfile);
      }

      double readExifLonOrLat(bool vislon) {
         double v = double.NaN;
         ExifProperty prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitude) : (p.Tag == ExifTag.GPSLatitude));
         if (prop != null) {
            v = (double)(prop as ExifLibrary.GPSLatitudeLongitude).Degrees +
                (double)(prop as ExifLibrary.GPSLatitudeLongitude).Minutes / 60.0 +
                (double)(prop as ExifLibrary.GPSLatitudeLongitude).Seconds / 3600.0;    // Grad, Minuten, Sekunden
            prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitudeRef) : (p.Tag == ExifTag.GPSLatitudeRef));
            if (prop != null) {
               if (vislon) {
                  if ((GPSLongitudeRef)prop.Value == GPSLongitudeRef.West)
                     v = -v;
               } else {
                  if ((GPSLatitudeRef)prop.Value == GPSLatitudeRef.South)
                     v = -v;
               }
            }
         }
         return v;
      }

      /// <summary>
      /// Der Konstruktor für MathEx.UFraction32 arbeitet für floats überraschend ungenau. Deshalb diese Variante.
      /// </summary>
      /// <param name="f"></param>
      /// <returns></returns>
      MathEx.UFraction32 getUFraction32ForFloat(float f) {
         uint denominator = f < 42.9 ? 100000000u : 10000000u;    // uint max. 0xFFFFFFFF !
         uint numerator = (uint)(f * denominator);
         while (numerator == (numerator / 10) * 10) {
            numerator /= 10;
            denominator /= 10;
         }
         return new MathEx.UFraction32(numerator, denominator);
      }

#if TEST1   // Original
      private const uint MaximumIterations = 10000000;

      public static MathEx.UFraction32 FromDouble(double value) {
         if (value < 0)
            throw new ArgumentException("value cannot be negative.", "value");

         if (double.IsNaN(value))
            return MathEx.UFraction32.NaN;
         else if (double.IsInfinity(value))
            return MathEx.UFraction32.Infinity;

         double f = value;
         double forg = f;
         uint lnum = 0;
         uint lden = 1;
         uint num = 1;
         uint den = 0;
         double lasterr = 1.0;
         uint a = 0;
         int currIteration = 0;
         while (true) {
            if (++currIteration > MaximumIterations) break;

            a = (uint)Math.Floor(f);
            f = f - (double)a;
            if (Math.Abs(f) < double.Epsilon)
               break;
            f = 1.0 / f;
            if (double.IsInfinity(f))
               break;
            uint cnum = num * a + lnum;
            uint cden = den * a + lden;
            if (Math.Abs((double)cnum / (double)cden - forg) < double.Epsilon)
               break;
            double err = ((double)cnum / (double)cden - (double)num / (double)den) / ((double)num / (double)den);
            // Are we converging?
            if (err >= lasterr)
               break;
            lasterr = err;
            lnum = num;
            lden = den;
            num = cnum;
            den = cden;
         }
         uint fnum = num * a + lnum;
         uint fden = den * a + lden;

         if (fden > 0)
            lasterr = value - ((double)fnum / (double)fden);
         else
            lasterr = double.PositiveInfinity;

         return new MathEx.UFraction32(fnum, fden, lasterr);
      }
#endif

      bool setLonOrLat(double v, bool vislon, bool gpsoverwrite) {
         bool set = false;
         if (!double.IsNaN(v)) {
            int degree, minute;
            float second;
            getDegreeMinuteSecond(v, out degree, out minute, out second);
            ExifProperty prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitude) : (p.Tag == ExifTag.GPSLatitude));
            if (prop == null) {
               data.Properties.Set(vislon ? ExifTag.GPSLongitude : ExifTag.GPSLatitude, degree, minute, second);
               set = true;
            } else {
               if (gpsoverwrite) {
                  prop.Value = new MathEx.UFraction32[] {
                                 new MathEx.UFraction32(degree),
                                 new MathEx.UFraction32(minute),
                                 getUFraction32ForFloat(second)
                            };
                  set = true;
               }
            }

            prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitudeRef) : (p.Tag == ExifTag.GPSLatitudeRef));
            if (prop == null) {
               if (vislon)
                  data.Properties.Set(ExifTag.GPSLongitudeRef, v >= 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);
               else
                  data.Properties.Set(ExifTag.GPSLatitudeRef, v >= 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);
               set = true;
            } else {
               if (gpsoverwrite) {
                  if (vislon)
                     prop.Value = v >= 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West;
                  else
                     prop.Value = v >= 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South;
                  set = true;
               }
            }

         }
         return set;
      }

      void getDegreeMinuteSecond(double latlon, out int degree, out int minute, out float second) {
         if (latlon < 0)
            latlon = -latlon;

         degree = (int)latlon;

         latlon -= degree;
         latlon *= 60.0;      // -> Umrechnung in Minuten

         minute = (int)latlon;

         latlon -= minute;
         latlon *= 60.0;      // -> Umrechnung in Sekunden

         second = (float)latlon;

      }

      /// <summary>
      /// entfernt ein einzelnes Tag
      /// </summary>
      /// <param name="data"></param>
      /// <param name="tag"></param>
      /// <returns>false, falls nicht vorhanden</returns>
      bool removeExifProperty(ExifTag tag) {
         bool changed = false;
         ExifProperty prop = data.Properties.FirstOrDefault(p => p.Tag == tag);
         if (prop != null) {
            data.Properties.Remove(prop);
            changed = true;
         }
         return changed;
      }

   }
}
