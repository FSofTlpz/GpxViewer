using GarminCore.OptimizedReader;
using System;

namespace GarminImageCreator.Garmin.Cache {
   public class SubdivMapDataCache : ThreadsafeCache<SubdivMapDataCache.DataItem> {

      public class DataItem {
         public readonly uint Mapnumber;
         public readonly int Idx;
         public readonly SubdivMapData? MapData;

         public DataItem(SubdivMapData? md, uint mapnumber, int idx) {
            MapData = md;
            Idx = idx;
            Mapnumber = mapnumber;
         }

      }


      public SubdivMapDataCache(int maxsize) : base(maxsize) { }

      protected override bool found(DataItem t, object obj) {
         if (obj is DataItem)
            return t.Mapnumber == ((DataItem)obj).Mapnumber &&
                   t.Idx == ((DataItem)obj).Idx;
         else
            throw new Exception("false type for compare: "+nameof(SubdivMapDataCache));
      }

      public SubdivMapData? Get(uint mapnumber, int idx) => base.Get(new DataItem(null, mapnumber, idx))?.MapData;

      public void Add(SubdivMapData md, uint mapnumber, int idx) => Add(new DataItem(md, mapnumber, idx));

   }
}
