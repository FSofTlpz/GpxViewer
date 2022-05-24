using System.Collections.Generic;
using GMap.NET.WindowsForms;

namespace GMap.NET.CoreExt.WindowsForms {

   public class GMapPolygonExt : GMapPolygon {

      /// <summary>
      /// Polygon mit Textmarker
      /// </summary>
      public Markers.GMarkerText Text = null;


      public GMapPolygonExt(List<PointLatLng> points, string name, PointLatLng txtpoint, string txt)
         : base(points, name) {

         if (!string.IsNullOrEmpty(txt) &&
             txtpoint != null)
            Text = new Markers.GMarkerText(txtpoint, txt);
      }

      public GMapPolygonExt(List<PointLatLng> points, string name, Markers.GMarkerText txt)
         : base(points, name) {
         Text = txt;
      }

   }

}
