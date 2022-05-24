namespace Sqlite4Garmin {

   public class GarminPoint {

      /// <summary>
      /// 0x0100 ..0x7FFF (Typ und Subtyp) und erweiterte Typen (0x1**00 .. 0x1**1F)
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
            if (MainType < 0x100) {
               FullType = (MainType << 8) + (value & 0xFF);
            } else {
               FullType = (MainType << 8) + (value & 0x1F);
            }
         }
      }

      /// <summary>
      /// geografische Koordinaten
      /// </summary>
      public GeoPoint Point;

      /// <summary>
      /// Label für dieses Objekt
      /// </summary>
      public string Label;

      public GarminPoint(int type, GeoPoint pt, string label = null) {
         Point = new GeoPoint(pt);
         MainType = type >> 8;
         SubType = type & 0xFF;
         Label = label;
      }

      public GarminPoint() :
         this(0, new GeoPoint(0, 0), null) { }

      public GarminPoint(int type, float lon, float lat, string label = null) :
         this(type, new GeoPoint(lon, lat), label) { }

      public GarminPoint(GarminPoint gpt) :
         this(gpt.FullType, gpt.Point, gpt.Label) { }

      public override string ToString() {
         return string.Format("FullType=0x{0}, Point=[{1}], Label='{2}'",
                              FullType.ToString(FullType < 0x100 ? "x4" : "x5"),
                              Point,
                              Label);
      }

   }


}
