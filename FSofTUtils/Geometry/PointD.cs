using System;
using System.ComponentModel;
using System.Globalization;

namespace FSofTUtils.Geometry {

   //[Serializable]
   //[System.Runtime.InteropServices.ComVisible(true)]

   [Serializable]
   public struct PointD {

      public static readonly PointD Empty = new PointD();
      
      private double x;
      private double y;

      public PointD(double x, double y) {
         this.x = x;
         this.y = y;
      }

      [Browsable(false)]
      public bool IsEmpty {
         get {
            return x == 0 &&
                   y == 0;
         }
      }

      public double X {
         get {
            return x;
         }
         set {
            x = value;
         }
      }

      public double Y {
         get {
            return y;
         }
         set {
            y = value;
         }
      }

      public static PointD operator +(PointD pt1, PointD pt2) {
         return Add(pt1, pt2);
      }

      public static PointD operator -(PointD pt1, PointD pt2) {
         return Subtract(pt1, pt2);
      }

      public static bool operator ==(PointD left, PointD right) {
         return left.X == right.X && left.Y == right.Y;
      }

      public static bool operator !=(PointD left, PointD right) {
         return !(left == right);
      }

      public static PointD Add(PointD pt1, PointD pt2) {
         return new PointD(pt1.X + pt2.X, pt1.Y + pt2.Y);
      }

      public static PointD Subtract(PointD pt1, PointD pt2) {
         return new PointD(pt1.X - pt2.X, pt1.Y - pt2.Y);
      }

      public override bool Equals(object obj) {
         if (!(obj is PointD))
            return false;
         PointD comp = (PointD)obj;
         return comp.X == X &&
                comp.Y == Y;
      }

      public override int GetHashCode() {
         return base.GetHashCode();
      }

      public override string ToString() {
         return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", x, y);
      }
   }
}
