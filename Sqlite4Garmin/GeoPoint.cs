using System;
using System.Collections.Generic;

namespace Sqlite4Garmin {

   /// <summary>
   /// WGS84-Koordinaten eines einzelnen Punktes
   /// </summary>
   public class GeoPoint {
      /// <summary>
      /// WGS84-Koordinaten
      /// </summary>
      public float Lat, Lon;

      /// <summary>
      /// erzeugt den Punkt aus WGS84-Koordinaten
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public GeoPoint(float lon = 0, float lat = 0) {
         Lon = lon;
         Lat = lat;
      }

      /// <summary>
      /// erzeugt den Punkt aus WGS84-Koordinaten
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public GeoPoint(double lon, double lat) {
         Lon = (float)lon;
         Lat = (float)lat;
      }

      /// <summary>
      /// erzeugt den Punkt aus einem Byte-Array
      /// </summary>
      /// <param name="b"></param>
      /// <param name="start"></param>
      public GeoPoint(byte[] b, int start = 0) {
         FromBytes(b, start);
      }

      /// <summary>
      /// kopiert den Punkt
      /// </summary>
      /// <param name="pt"></param>
      public GeoPoint(GeoPoint pt) {
         Lon = pt.Lon;
         Lat = pt.Lat;
      }

      /// <summary>
      /// Umwandeln in 8 Byte
      /// </summary>
      /// <returns></returns>
      public byte[] ToBytes() {
         byte[] b = new byte[8];
         BitConverter.GetBytes(Lon).CopyTo(b, 0);
         BitConverter.GetBytes(Lat).CopyTo(b, 4);
         return b;
      }

      /// <summary>
      /// Umwandeln aus 8 Byte
      /// </summary>
      /// <param name="b"></param>
      /// <param name="start"></param>
      public void FromBytes(byte[] b, int start = 0) {
         if (b.Length - start >= 8) {
            Lon = BitConverter.ToSingle(b, start);
            Lat = BitConverter.ToSingle(b, start + 4);
         }
      }

      /// <summary>
      /// wandelt eine Punkt-Liste in eine Byte-Array um
      /// </summary>
      /// <param name="ptlist"></param>
      /// <returns></returns>
      public static byte[] PointsToBytes(IList<GeoPoint> ptlist) {
         byte[] b = new byte[8 * ptlist.Count];
         for (int i = 0; i < ptlist.Count; i++)
            ptlist[i].ToBytes().CopyTo(b, i * 8);
         return b;
      }

      /// <summary>
      /// wandelt eine Byte-Array in eine Punkt-Array um
      /// </summary>
      /// <param name="b"></param>
      /// <returns></returns>
      public static GeoPoint[] PointsFromBytes(byte[] b) {
         GeoPoint[] ptlst = new GeoPoint[b.Length / 8];
         for (int i = 0; i < b.Length / 8; i++) {
            ptlst[i] = new GeoPoint(b, i * 8);
         }
         return ptlst;
      }

      public override string ToString() {
         return string.Format("Lon={0}°, Lat={1}°", Lon, Lat);
      }

   }
}
