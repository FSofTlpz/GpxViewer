using System.Collections.Generic;

#if !GMAP4SKIA
namespace GMap.NET.WindowsForms {
#else
namespace GMap.NET.Skia {
#endif

   public class GMapPolygonExt : GMapPolygon {

      /// <summary>
      /// Polygon mit Textmarker
      /// </summary>
      public GMarkerText Text = null;


      public GMapPolygonExt(List<PointLatLng> points, string name, PointLatLng txtpoint, string txt)
         : base(points, name) {

         if (!string.IsNullOrEmpty(txt) &&
             txtpoint != null)
            Text = new GMarkerText(txtpoint, txt);
      }

      public GMapPolygonExt(List<PointLatLng> points, string name, GMarkerText txt)
         : base(points, name) {
         Text = txt;
      }

   }

}
