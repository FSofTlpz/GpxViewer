using GMap.NET;
using System.Collections.Generic;

namespace SpecialMapCtrl {

   public class MapPolygonExt : MapPolygon {

      /// <summary>
      /// Polygon mit Textmarker
      /// </summary>
      public MarkerText? Text = null;


      public MapPolygonExt(List<PointLatLng> points, string name, PointLatLng txtpoint, string txt)
         : base(points, name) {

         if (!string.IsNullOrEmpty(txt))
            Text = new MarkerText(txtpoint, txt);
      }

      public MapPolygonExt(List<PointLatLng> points, string name, MarkerText txt)
         : base(points, name) {
         Text = txt;
      }

   }

}
