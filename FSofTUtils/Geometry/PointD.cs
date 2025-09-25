using System;
using System.ComponentModel;
using System.Globalization;

namespace FSofTUtils.Geometry {

   [Serializable]
   public class PointD {

      public static readonly PointD Empty = new PointD(0, 0);

      protected double x;
      protected double y;

      public double X {
         get => x;
         set => x = value;
      }

      public double Y {
         get => y;
         set => y = value;
      }


      public PointD(double x, double y) {
         this.x = x;
         this.y = y;
      }

      [Browsable(false)]
      public bool IsEmpty {
         get => x == 0 && y == 0;
      }

      public static PointD operator +(PointD pt1, PointD pt2) => Add(pt1, pt2);

      public static PointD operator -(PointD pt1, PointD pt2) => Subtract(pt1, pt2);

      public static bool operator ==(PointD left, PointD right) => left.x == right.x && left.y == right.y;

      public static bool operator !=(PointD left, PointD right) => !(left == right);

      public static PointD Add(PointD pt1, PointD pt2) => new PointD(pt1.x + pt2.x, pt1.y + pt2.y);

      public static PointD Subtract(PointD pt1, PointD pt2) => new PointD(pt1.x - pt2.x, pt1.y - pt2.y);

      /// <summary>
      /// Länge zum Nullpunkt
      /// </summary>
      /// <returns></returns>
      public double Absolute() => Math.Sqrt(SquareAbsolute());

      /// <summary>
      /// Quadrat der Länge zum Nullpunkt
      /// </summary>
      /// <returns></returns>
      public double SquareAbsolute() => x * x + y * y;

      /// <summary>
      /// Skalarprodukt von 2 "Vektoren"
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double DotProduct(PointD p) => x * p.x + y * p.y;

      /// <summary>
      /// (kleinerer) Winkel zwischen 2 "Vektoren" (0..Math.PI)
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double Arc(PointD p) => Math.Acos(DotProduct(p) / (Absolute() * p.Absolute()));

      /// <summary>
      /// Quadrat des Abstandes
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double SquareDistance(PointD p) => (x - p.x) * (x - p.x) + (y - p.y) * (y - p.y);

      /// <summary>
      /// Abstand zum Punkt <paramref name="p"/>
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public double Distance(PointD p) => Math.Sqrt(SquareDistance(p));


      public override bool Equals(object? obj) {
         if (!(obj is PointD))
            return false;
         PointD comp = (PointD)obj;
         return comp.X == X &&
                comp.Y == Y;
      }

      public override int GetHashCode() => base.GetHashCode();

      public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", x, y);

   }
}
