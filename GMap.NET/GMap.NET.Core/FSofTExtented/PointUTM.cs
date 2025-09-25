using System;
using System.Globalization;

namespace GMap.NET.FSofTExtented {

   /// <summary>
   /// the UTM-point of coordinates
   /// </summary>
   [Serializable]
   public class PointUTM {

      public class SizeUTM {
         public double DeltaWidth;
         public double DeltaHeight;

         public SizeUTM(double deltaWidth = 0, double deltaHeight = 0) {
            DeltaWidth = deltaWidth;
            DeltaHeight = deltaHeight;
         }
      }



      public static readonly PointUTM Empty = new PointUTM();
      private double y;
      private double x;
      private int zone;

      bool NotEmpty;


      public PointUTM() : this(0, 0, 0) {
         NotEmpty = false;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y">wenn kleiner 0 wird Koordinatenbereich -10000000..10000000 angenommen</param>
      /// <param name="zone"></param>
      public PointUTM(double x, double y, int zone) {
         this.x = x;

         if (y < 0)  // Spezialfall:
            this.y = y;
         else
            Y = y;   // echte UTM-Koordinate

         this.zone = zone;
         NotEmpty = true;
      }

      /// <summary>
      /// returns true if coordinates wasn't assigned
      /// </summary>
      public bool IsEmpty {
         get {
            return !NotEmpty;
         }
      }

      public double X {
         get {
            return this.x;
         }
         set {
            this.x = value;
            NotEmpty = true;
         }
      }

      public double Y {
         get {
            // aus linearen Wert -10000000..10000000 in echtes UTM umwandeln
            if (y >= 0)
               return y;               // N (0..10000000)
            else
               return -y + 10000000;   // S (10000000..20000000)
         }
         set {
            // in linearen Wert -10000000..10000000 umwandeln
            if (value < 10000000)
               y = value;              // N
            else
               y = -value + 10000000;  // S
            NotEmpty = true;
         }
      }

      public int Zone {
         get {
            return this.zone;
         }
         set {
            this.zone = value;
            NotEmpty = true;
         }
      }

      /// <summary>
      /// nördliche oder südliche Halbkugel
      /// </summary>
      public bool North {
         get {
            return y < 10000000;
         }
      }

      /// <summary>
      /// liefert den Buchstaben des Zonenfeldes (außer Sonderfall Skandinavien: 32V 9° breit)
      /// </summary>
      /// <param name="lng"></param>
      /// <returns></returns>
      public static string Zonefield(double lng) {
         // i.A. jeweils 8° hohe Bereiche (-80°..84°) zusätzlich mit Buchstaben bezeichnet (C,D,...,X, ohne I und O) (X mit 12°, Skandinavien 32V 9° breit)
         if (lng < -72) return "C";
         if (lng < -64) return "D";
         if (lng < -56) return "E";
         if (lng < -48) return "F";
         if (lng < -40) return "G";
         if (lng < -32) return "H";
         if (lng < -24) return "J";
         if (lng < -16) return "K";
         if (lng < -8) return "L";
         if (lng < 0) return "M";
         if (lng < 8) return "N";
         if (lng < 16) return "P";
         if (lng < 24) return "Q";
         if (lng < 32) return "R";
         if (lng < 40) return "S";
         if (lng < 48) return "T";
         if (lng < 56) return "U";
         if (lng < 64) return "V";
         if (lng < 72) return "W";
         return "X";
      }

      public static PointUTM operator +(PointUTM pt, SizeUTM sz) {
         return Add(pt, sz);
      }

      public static PointUTM operator -(PointUTM pt, SizeUTM sz) {
         return Subtract(pt, sz);
      }

      /// <summary>
      /// sinnlos bei unterschiedl. <see cref="Zone"/>
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      public static SizeUTM operator -(PointUTM pt1, PointUTM pt2) {
         return new SizeUTM(pt1.Y - pt2.Y, pt2.X - pt1.X);
      }

      public static bool operator ==(PointUTM left, PointUTM right) {
         return ((left.X == right.X) && (left.Y == right.Y) && (left.Zone == right.Zone));
      }

      public static bool operator !=(PointUTM left, PointUTM right) {
         return !(left == right);
      }

      public static PointUTM Add(PointUTM pt, SizeUTM sz) {
         return new PointUTM(pt.Y - sz.DeltaHeight, pt.X + sz.DeltaWidth, pt.Zone);
      }

      public static PointUTM Subtract(PointUTM pt, SizeUTM sz) {
         return new PointUTM(pt.Y + sz.DeltaHeight, pt.X - sz.DeltaWidth, pt.Zone);
      }

      public override bool Equals(object obj) {
         if (!(obj is PointUTM)) {
            return false;
         }
         PointUTM tf = (PointUTM)obj;
         return (((tf.X == this.X) && (tf.Y == this.Y) && (tf.Zone == this.Zone)) && tf.GetType().Equals(base.GetType()));
      }

      public void Offset(PointUTM pos) {
         this.Offset(pos.Y, pos.X);
      }

      public void Offset(double x, double y) {
         this.X += x;
         this.Y -= y;
      }

      public override int GetHashCode() {
         return (this.X.GetHashCode() ^ this.Y.GetHashCode());
      }

      public override string ToString() {
         return string.Format(CultureInfo.CurrentCulture, "{{X={1}, Y={0}, Zone={2}, North={3}}}", Y, X, Zone, North);
      }
   }

}
