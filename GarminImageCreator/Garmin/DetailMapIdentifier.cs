using System;

namespace GarminImageCreator.Garmin {

   /// <summary>
   /// identifiziert eine <see cref="Detailmap"/> bzw. eine Subdiv eindeutig über die Tilemapnummer und die Indexnummer
   /// </summary>
#pragma warning disable CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
#pragma warning disable CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
   public class DetailMapIdentifier : IComparable {
#pragma warning restore CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
#pragma warning restore CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)

      public readonly uint TilemapNumber;
      public readonly int SubdivIndex;

      public DetailMapIdentifier(uint tilemapnumber, int subdivindex) {
         TilemapNumber = tilemapnumber;
         SubdivIndex = subdivindex;
      }

      public DetailMapIdentifier(DetailMapIdentifier id) {
         TilemapNumber = id.TilemapNumber;
         SubdivIndex = id.SubdivIndex;
      }

      public static bool operator ==(DetailMapIdentifier i1, DetailMapIdentifier i2) {
         return //i1 != null &&
                //i2 != null &&
                (i1.TilemapNumber == i2.TilemapNumber) &&
                (i1.SubdivIndex == i2.SubdivIndex);
      }

      public static bool operator !=(DetailMapIdentifier i1, DetailMapIdentifier i2) {
         return !(i1 == i2);
      }

      public int CompareTo(object obj) {
         if (obj != null) {
            DetailMapIdentifier id = obj as DetailMapIdentifier;
            if (TilemapNumber > id.TilemapNumber)
               return 1;
            else if (TilemapNumber < id.TilemapNumber)
               return -1;
            if (SubdivIndex > id.SubdivIndex)
               return 1;
            else if (SubdivIndex < id.SubdivIndex)
               return -1;
            return 0;
         } else
            return 1;
      }

      public override string ToString() {
         return string.Format("TilemapNumber {0}, SubdivIndex {1}", TilemapNumber, SubdivIndex);
      }
   }
}
