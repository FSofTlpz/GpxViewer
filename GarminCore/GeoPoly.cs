using System.Collections.Generic;
using System.Drawing;

namespace GarminCore {
   public class GeoPoly : GeoObject {

      /// <summary>
      /// Koordinaten der Punkte
      /// </summary>
      public PointF[]? Points { get; protected set; }

      public Bound Bound { get; protected set; }

      public bool DirectionIndicator { get; protected set; }


      public GeoPoly(int type,
                     string? txt,
                     IList<PointF>? pt,
                     double boundleft,
                     double boundright,
                     double boundbottom,
                     double boundtop,
                     bool directionindicator,
                     bool ptcopy = false) : base(type, txt) {
         Points = new PointF[pt != null ? pt.Count : 0];
         if (ptcopy) {
            if (pt != null)
               for (int i = 0; i < pt.Count; i++)
                  Points[i] = new PointF(pt[i].X, pt[i].Y);
         } else {
            if (pt != null)
               for (int i = 0; i < pt.Count; i++)
                  Points[i] = pt[i];
         }
         Bound = new Bound(boundleft, boundright, boundbottom, boundtop);

         DirectionIndicator = directionindicator;
      }

      public GeoPoly(GeoPoly poly, bool ptcopy) :
         this(poly.Type, poly.Text, poly.Points, poly.Bound.LeftDegree, poly.Bound.RightDegree, poly.Bound.BottomDegree, poly.Bound.TopDegree, poly.DirectionIndicator, ptcopy) { }

      public bool OverlappedWithBound(Bound bound) {
         return bound.IsOverlapped(Bound);
      }

      public override string ToString() {
         return string.Format("{0}, points {1}", base.ToString(), Points != null ? Points.Length : 0);
      }

      bool _isdisposed = false;

      protected override void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {
            if (notfromfinalizer) { // nur dann alle managed Ressourcen freigeben
               Points = null;
            }

            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;
            base.Dispose(notfromfinalizer);
         }
      }
   }
}
