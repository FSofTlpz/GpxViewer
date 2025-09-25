using System;
using System.Drawing;

namespace GarminImageCreator {
   /// <summary>
   /// Garmin-Kartenobjekt einer Objektsuche
   /// </summary>
#pragma warning disable CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
#pragma warning disable CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
   public class SearchObject : IComparable {

      public enum ObjectType {
         Area = 0, Line = 1, Point = 2
      }

      public ObjectType Objecttype { get; protected set; }
      public int TypeNo { get; protected set; }
      public string TypeName { get; protected set; }
      public string Name { get; protected set; }
      public Bitmap? Bitmap { get; protected set; }

      public SearchObject(ObjectType objecttype,
                          int typeno,
                          string typename,
                          string name,
                          Bitmap? bitmap) {
         Objecttype = objecttype;
         TypeNo = typeno;
         TypeName = typename;
         Name = name;
         Bitmap = bitmap;
      }

      public int CompareTo(object? obj) {
         if (obj == null)
            return 1;
         SearchObject so = (SearchObject)obj;
         if ((int)Objecttype > (int)so.Objecttype)
            return 1;
         else if ((int)Objecttype < (int)so.Objecttype)
            return -1;

         if (TypeNo > so.TypeNo)
            return -1;
         else if (TypeNo < so.TypeNo)
            return 1;

         int ret = string.Compare(Name, so.Name);
         if (ret > 0)
            return 1;
         else if (ret < 0)
            return -1;

         return 0;
      }

      public static bool operator ==(SearchObject x, SearchObject y) {
         return x.CompareTo(y) == 0;
      }

      public static bool operator !=(SearchObject x, SearchObject y) {
         return x.CompareTo(y) != 0;
      }

      public override string ToString() {
         return string.Format("{0}, {1} {2}: {3}", Objecttype, TypeNo, TypeName, Name);
      }
   }
#pragma warning restore CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
#pragma warning restore CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
}
