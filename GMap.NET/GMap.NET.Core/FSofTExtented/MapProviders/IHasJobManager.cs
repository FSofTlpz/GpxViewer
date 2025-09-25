using System;

namespace GMap.NET.FSofTExtented.MapProviders {
   public interface IHasJobManager {

      public void SetJobFilter(int[] deltaDbId);

      public void SetJobFilter(int zoom);

      public void SetJobFilter(int[] deltaDbId, int zoom);

      public void CancelAllJobs(DateTime time);

   }
}
