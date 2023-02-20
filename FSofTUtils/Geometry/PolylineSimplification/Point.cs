using System;

namespace FSofTUtils.Geometry.PolylineSimplification {

   public class Point {
      public double X { get; set; }
      public double Y { get; set; }

      public bool IsValid { get; set; }
      public bool IsLocked { get; set; }

      public Point(Point p) {
         Set(p);
      }

      public Point(double x, double y) {
         X = x;
         Y = y;
         IsValid = true;
         IsLocked = false;
      }

      public Point(double x, double y, bool bIsLocked) {
         X = x;
         Y = y;
         IsValid = true;
         IsLocked = bIsLocked;
      }

      public Point() {
         X = 0;
         Y = 0;
         IsValid = true;
         IsLocked = false;
      }

      public void Set(Point p) {
         X = p.X;
         Y = p.Y;
         IsValid = p.IsValid;
         IsLocked = p.IsLocked;
      }

      /// <summary>
      /// Länge zum Nullpunkt
      /// </summary>
      /// <returns></returns>
      public double Absolute() {
         return Math.Sqrt(SquareAbsolute());
      }

      /// <summary>
      /// Quadrat der Länge
      /// </summary>
      /// <returns></returns>
      public double SquareAbsolute() {
         return X * X + Y * Y;
      }

      /// <summary>
      /// Skalarprodukt
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double DotProduct(Point p) {
         return X * p.X + Y * p.Y;
      }

      /// <summary>
      /// Winkel zwischen 2 Vektoren (0..Math.PI)
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double Arc(Point p) {
         return Math.Acos(DotProduct(p) / (Absolute() * p.Absolute()));
      }

      /// <summary>
      /// Quadrat des Abstandes
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double SquareDistance(Point p) {
         return (X - p.X) * (X - p.X) + (Y - p.Y) * (Y - p.Y);
      }

      /// <summary>
      /// Abstand
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double Distance(Point p) {
         return Math.Sqrt(SquareDistance(p));
      }


      public static Point operator -(Point p1, Point p2) {
         Point p = new Point(p1);
         p.X -= p2.X;
         p.Y -= p2.Y;
         return p;
      }

      public bool Equals(Point p) {
         return p != null && X == p.X && Y == p.Y;
      }

      public override string ToString() {
         return string.Format("({0}, {1}), IsValid={2}, IsLocked={3}", X, Y, IsValid, IsLocked);
      }
   }

}
