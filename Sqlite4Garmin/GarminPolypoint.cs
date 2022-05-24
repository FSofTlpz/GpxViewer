using System.Collections.Generic;

namespace Sqlite4Garmin {

   /// <summary>
   /// für Linien und Gebiete
   /// </summary>
   public class GarminPolypoint {

      /// <summary>
      /// bis 0x7F00 für Polygone und 0x3F00 Linien (bzw. 0x1**00 .. 0x1**1F)
      /// </summary>
      public int FullType { get; private set; }

      /// <summary>
      /// 0x01 .. 0x7F oder 0x100 .. 0x1FF
      /// </summary>
      public int MainType {
         get {
            return FullType >> 8;
         }
         set {
            if ((0x100 <= value && value < 0x200) ||
                (0x00 < value && value < 0x80))
               FullType = (value << 8) + (FullType & 0xFF);
         }
      }

      /// <summary>
      /// 0x00 .. 0xFF
      /// </summary>
      public int SubType {
         get {
            return FullType & 0xFF;
         }
         set {
            if (MainType >= 0x100) {
               FullType = (MainType << 8) + (value & 0x1F);
            }
         }
      }

      /// <summary>
      /// Label für dieses Objekt
      /// </summary>
      public string Label;

      public float North, East, South, West;

      /// <summary>
      /// Punktliste für die Linie bzw. das Polygon
      /// </summary>
      public GeoPoint[] Pointlist;

      public GarminPolypoint(int type, string label, float north, float east, float south, float west, IList<GeoPoint> ptlst) {
         MainType = type >> 8;
         SubType = type & 0x1F;
         Label = label;
         North = north;
         East = east;
         South = south;
         West = west;
         Pointlist = new GeoPoint[ptlst != null ? ptlst.Count : 0];
         if (ptlst != null)
            ptlst.CopyTo(Pointlist, 0);
      }

      public GarminPolypoint(int type, string label, float north, float east, float south, float west, GeoPoint[] ptlst, bool ptlstcopy = false) {
         MainType = type >> 8;
         SubType = type & 0x1F;
         Label = label;
         North = north;
         East = east;
         South = south;
         West = west;
         if (ptlstcopy) {
            Pointlist = new GeoPoint[ptlst != null ? ptlst.Length : 0];
            for (int i = 0; i < Pointlist.Length; i++)
               Pointlist[i] = new GeoPoint(ptlst[i]);
         } else {
            if (ptlst == null)
               Pointlist = new GeoPoint[0];
            else
               Pointlist = ptlst;
         }
      }

      public GarminPolypoint(int type, string label, float north, float east, float south, float west, byte[] b) :
         this(type, label, north, east, south, west, GeoPoint.PointsFromBytes(b)) { }

      /// <summary>
      /// liefert die Koordinaten als Byte-Array
      /// </summary>
      /// <returns></returns>
      public byte[] PointsToBytes() {
         return GeoPoint.PointsToBytes(Pointlist);
      }

      public override string ToString() {
         return string.Format("FullType=0x{0}, Points={1}, Label='{2}'",
                              FullType.ToString(FullType < 0x100 ? "x4" : "x5"),
                              Pointlist.Length,
                              Label);
      }

   }

}
