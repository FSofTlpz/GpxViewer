using System;
using System.Drawing;

namespace FSofTUtils.Geometry {
   public class RectangleCommon {

      PointF[] points;

      public PointF TopLeft {
         get {
            return points[0];
         }
      }
      public PointF TopRight {
         get {
            return points[1];
         }
      }
      public PointF BottomRight {
         get {
            return points[2];
         }
      }
      public PointF BottomLeft {
         get {
            return points[3];
         }
      }

      public float Height { get; protected set; }
      public float Width { get; protected set; }

      public float TopLeftAngle { get; protected set; }

      public enum RotationPoint {
         TopLeft,
         Center,
      }



      public RectangleCommon(float topleftx, float toplefty, float width, float height, float topleftangle, RotationPoint rotationPoint = RotationPoint.TopLeft) {
         points = new PointF[4];
         points[0].X = topleftx;
         points[0].Y = toplefty;
         Height = height;
         Width = width;
         TopLeftAngle = topleftangle;
         switch (rotationPoint) {
            case RotationPoint.TopLeft:
               recalculatePoints(points[0]);
               break;

            case RotationPoint.Center:
               recalculatePoints(new PointF(topleftx + width / 2, toplefty + height / 2));
               break;
         }
      }

      public RectangleCommon(PointF topleft, float width, float height, float topleftangle, RotationPoint rotationPoint = RotationPoint.TopLeft) :
         this(topleft.X, topleft.Y, width, height, topleftangle, rotationPoint) { }

      void recalculatePoints(PointF ptRotate) {
         points[1].X = TopLeft.X + Width;
         points[1].Y = TopLeft.Y;

         points[3].X = TopLeft.X;
         points[3].Y = TopLeft.Y + Height;

         points[2].X = points[1].X;
         points[2].Y = points[3].Y;

         if (TopLeftAngle != 0) {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.RotateAt(TopLeftAngle, ptRotate, System.Drawing.Drawing2D.MatrixOrder.Append);

            m.TransformPoints(points);
         }
      }

      public bool IsIntersect(RectangleCommon rectangleCommon) {
         return Utilities.IsRectangleIntersect(TopLeft.X, TopLeft.Y,
                                               TopRight.X, TopRight.Y,
                                               BottomRight.X, BottomRight.Y,
                                               BottomLeft.X, BottomLeft.Y,
                                               rectangleCommon.TopLeft.X, rectangleCommon.TopLeft.Y,
                                               rectangleCommon.TopRight.X, rectangleCommon.TopRight.Y,
                                               rectangleCommon.BottomRight.X, rectangleCommon.BottomRight.Y,
                                               rectangleCommon.BottomLeft.X, rectangleCommon.BottomLeft.Y);
      }

      public bool IsIntersect(RectangleF rectangle) {
         return IsIntersect(new RectangleCommon(rectangle.Location, rectangle.Width, rectangle.Height, 0));
      }

      public void Move(float dx, float dy) {
         SizeF move = new SizeF(dx, dy);
         for (int i = 0; i < points.Length; i++) {
            points[i].X += dx;
            points[i].Y += dy;
         }
      }

      public RectangleF GetBounds() {
         float minx = Math.Min(Math.Min(points[0].X, points[1].X), Math.Min(points[2].X, points[3].X));
         float miny = Math.Min(Math.Min(points[0].Y, points[1].Y), Math.Min(points[2].Y, points[3].Y));
         return new RectangleF(minx,
                               miny,
                               Math.Max(Math.Max(points[0].X, points[1].X), Math.Max(points[2].X, points[3].X)) - minx,
                               Math.Max(Math.Max(points[0].Y, points[1].Y), Math.Max(points[2].Y, points[3].Y)) - miny);
      }


      public override string ToString() {
         return string.Format("TopLeft={0}, Width={1}, Height={2}, TopLeftAngle={3}°", TopLeft, Width, Height, TopLeftAngle);
      }
   }
}
