using System.Drawing;

namespace GarminCore {
   public class GeoPoint : GeoObject {

      /// <summary>
      /// Punktkoordinaten
      /// </summary>
      public PointF Point { get; protected set; }


      public GeoPoint(int type, string? txt, float lon, float lat) : base(type, txt) {
         Point = new PointF(lon, lat);
      }

      public GeoPoint(int type, string? txt, double lon, double lat) : this(type, txt, (float)lon, (float)lat) { }

      public GeoPoint(int type, string? txt, PointF pt, bool ptcopy = false) : base(type, txt) {
         if (ptcopy)
            Point = pt;
         else
            Point = new PointF(pt.X, pt.Y);
      }

      public GeoPoint(GeoPoint point, bool ptcopy) :
         this(point.Type, point.Text, point.Point, ptcopy) { }

      public bool IsInBound(Bound bound) {
         return bound.IsEnclosed(Point.X, Point.Y);
      }

      public override string ToString() {
         return string.Format("{0}, lon={1}, lat={2}", base.ToString(), Point.X, Point.Y);
      }

   }
}
