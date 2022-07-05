using ExifLibrary;
using System;
using System.IO;
using System.Linq;

namespace GeoTagger {
   class ExifGeo {

      /*
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

      public void GetLatLon(out double lat, out double lon) {
         lat = readExifLonOrLat(false);
         lon = readExifLonOrLat(true);
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
            data.Properties.Add(ExifTag.GPSImgDirection, v);
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

      bool setLonOrLat(double v, bool vislon, bool gpsoverwrite) {
         bool set = false;
         int degree, minute;
         float second;

         getDegreeMinuteSecond(v, out degree, out minute, out second);

         ExifProperty prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitude) : (p.Tag == ExifTag.GPSLatitude));
         if (prop == null) {
            data.Properties.Add(vislon ? ExifTag.GPSLongitude : ExifTag.GPSLatitude, degree, minute, second);
            set = true;
         } else {
            if (gpsoverwrite) {
               prop.Value = new MathEx.UFraction32[] { new MathEx.UFraction32(degree), new MathEx.UFraction32(minute), new MathEx.UFraction32(second) };
               set = true;
            }
         }

         prop = data.Properties.FirstOrDefault(p => vislon ? (p.Tag == ExifTag.GPSLongitudeRef) : (p.Tag == ExifTag.GPSLatitudeRef));
         if (prop == null) {
            if (vislon)
               data.Properties.Add(ExifTag.GPSLongitudeRef, v >= 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);
            else
               data.Properties.Add(ExifTag.GPSLatitudeRef, v >= 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);
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
