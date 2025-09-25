//#define LOCALDEBUG

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace GMap.NET.FSofTExtented.MapProviders {
   internal class JobManager {

      internal class GMapTileId {
         static uint id = 0;

         static object locker = new object();

         /// <summary>
         /// Zeitpunkt der Erzeugung
         /// </summary>
         public readonly DateTime CreationTime;

         public readonly int DeltaDbId;

         public readonly PointLatLng Point;

         public readonly int Zoom;

         /// <summary>
         /// threadsicher erzeugte ID
         /// </summary>
         public readonly uint ID;

         public readonly CancellationTokenSource cancellationTokenSource;

         public CancellationToken CancellationToken => cancellationTokenSource.Token;

         public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;


         public GMapTileId(int deltadbid, PointLatLng point, int zoom) {
            lock (locker) {
               ID = id < uint.MaxValue ? ++id : 0;
            }
            CreationTime = DateTime.Now;
            DeltaDbId = deltadbid;
            Point = point;
            Zoom = zoom;
            cancellationTokenSource = new CancellationTokenSource();
         }

         public void RequestCancellation() => cancellationTokenSource.Cancel();

         public override string ToString() => ID +
                                              ": DeltaDbId=" + DeltaDbId +
                                              ", Point=" + Point +
                                              ", Zoom=" + Zoom +
                                              ", IsCancellationRequested=" + IsCancellationRequested +
                                              ", Time=" + CreationTime.ToString("O");

      }

      ConcurrentDictionary<uint, GMapTileId> jobs = new ConcurrentDictionary<uint, GMapTileId>();

      int _filterZoom = -1;

      int filterZoom {
         get => Interlocked.Exchange(ref _filterZoom, _filterZoom);
         set => Interlocked.Exchange(ref _filterZoom, value);
      }

      object lock_filterDeltaDbId = new object();

      int[] filterDeltaDbId = null;

      public readonly string Name;


      public JobManager(string name) {
         Name = name;
      }

      public bool AddJob(int deltaDbId,
                         PointLatLng pt,
                         int zoom,
                         out uint jobid,
                         out CancellationToken? cancellationtoken) {
#if LOCALDEBUG
         Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                         nameof(AddJob) + " for deltaDbId=" + deltaDbId + " zoom=" + zoom + ", pt=" + pt);
#endif
         if (!checkIdAndZoom(deltaDbId, zoom)) {
            jobid = 0;
            cancellationtoken = null;
#if LOCALDEBUG
            Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                            nameof(AddJob) + " for deltaDbId=" + deltaDbId + " zoom=" + zoom + " => FALSE");
#endif
            return false;
         }

         GMapTileId gMapTileId = new GMapTileId(deltaDbId, pt, zoom);
         bool result = jobs.TryAdd(gMapTileId.ID, gMapTileId);
         cancellationtoken = gMapTileId.CancellationToken;
         jobid = gMapTileId.ID;
#if LOCALDEBUG
         listContent4Debug();
#endif
         return result;
      }

      /// <summary>
      /// Prüfung der Filterbedingungen
      /// </summary>
      /// <param name="deltaDbId"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      bool checkIdAndZoom(int deltaDbId, int zoom) {
         bool found = false;
         if (filterZoom < 0 || zoom == filterZoom)
            lock (lock_filterDeltaDbId) {
               if (filterDeltaDbId != null && filterDeltaDbId.Length > 0) {
                  for (int i = 0; i < filterDeltaDbId.Length; i++)
                     if (filterDeltaDbId[i] == deltaDbId) {
                        found = true;
                        break;
                     }
               } else
                  found = true;
            }
         return found;
      }

      /// <summary>
      /// entfernt den Job
      /// </summary>
      /// <param name="jobid"></param>
      /// <returns></returns>
      public bool RemoveJob(uint jobid) {
#if LOCALDEBUG
         Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                         nameof(RemoveJob) + " for jobid=" + jobid);
#endif
         bool result = jobs.TryRemove(jobid, out _);
#if LOCALDEBUG
         listContent4Debug();
#endif
         return result;
      }

      /// <summary>
      /// Job auf Cancel gesetzt?
      /// </summary>
      /// <param name="jobid"></param>
      /// <returns></returns>
      public bool IsCanceled(uint jobid) {
         if (jobs.TryGetValue(jobid, out GMapTileId job))
            return job.IsCancellationRequested;
         return true;
      }

      ///// <summary>
      ///// Unnötige Jobs werden als Cancel-bar markiert.
      ///// </summary>
      ///// <param name="actGMapTileId"></param>
      //void killUnnecessaryJobs(GMapTileId actGMapTileId) {
      //   foreach (var kv in jobs.ToArray())
      //      if ((kv.Value.CreationTime <= actGMapTileId.CreationTime &&    // Job ist älter und ...
      //           (kv.Value.Zoom != actGMapTileId.Zoom ||                   // ... hat einen anderen Zoom ...
      //           kv.Value.DeltaDbId != actGMapTileId.DeltaDbId)) ||        // ... oder eine andere Provider-(Delta-)ID
      //          (actGMapTileId.CreationTime.Subtract(kv.Value.CreationTime).TotalSeconds > 60)) { // ... oder ist schon älter als 1min -> nicht mehr benötigt
      //         kv.Value.RequestCancellation();
      //         Debug.WriteLine(nameof(GarminProvider) + "." + nameof(killUnnecessaryJobs) + " für (" +
      //                         kv.Value.Point + ", zoom=" + kv.Value.Zoom + ") IsCancel = true wegen (" +
      //                         actGMapTileId.Point + ", zoom=" + actGMapTileId.Zoom + ")");
      //      }
      //}

      /// <summary>
      /// alle Jobs auf Cancel setzen
      /// </summary>
      public void CancelAll(DateTime time) {
#if LOCALDEBUG
         Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " + nameof(CancelAll));
#endif
         foreach (var kv in jobs)
            if (kv.Value.CreationTime < time)      // alle älteren Jobs
               kv.Value.RequestCancellation();
#if LOCALDEBUG
         listContent4Debug();
#endif
      }

      public void SetJobFilter(int[] deltaDbId) => SetJobFilter(deltaDbId, -1);

      public void SetJobFilter(int zoom) => SetJobFilter(null, zoom);

      public void SetJobFilter(int[] deltaDbId, int zoom) {
         filterZoom = zoom;
         lock (lock_filterDeltaDbId) {
            filterDeltaDbId = deltaDbId != null ? (int[])deltaDbId.Clone() : null;
         }
         removeUnnecessaryJobs(deltaDbId, zoom);
      }

      /// <summary>
      /// alle unnötigen Jobs auf Cancel setzen
      /// </summary>
      /// <param name="deltaDbId"></param>
      /// <param name="zoom"></param>
      void removeUnnecessaryJobs(int[] deltaDbId, int zoom) {
         if (zoom >= 0 || deltaDbId != null) {
#if LOCALDEBUG
            Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                            nameof(removeUnnecessaryJobs) + " for valid ID=" + string.Join(',', deltaDbId) + " Zoom=" + zoom);
            listContent4Debug();
#endif
            foreach (var kv in jobs) {
               if (zoom >= 0)
                  if (kv.Value.Zoom != zoom) {
#if LOCALDEBUG
                     Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                                     nameof(removeUnnecessaryJobs) + " for valid ID=" + string.Join(',', deltaDbId) + " Zoom=" + zoom +
                                     ", RequestCancellation for JobID=" + kv.Value.ID + " (zoom=" + kv.Value.ID + ")");
#endif
                     kv.Value.RequestCancellation();
                     RemoveJob(kv.Value.ID);
                     continue;
                  }

               if (deltaDbId != null) {
                  bool found = false;
                  for (int i = 0; i < deltaDbId.Length; i++)
                     if (kv.Value.DeltaDbId == deltaDbId[i]) {
                        found = true;
                        break;
                     }
                  if (!found) {
#if LOCALDEBUG
                     Debug.WriteLine(nameof(JobManager) + " [" + this + "]: " +
                                     nameof(removeUnnecessaryJobs) + " for valid ID=" + string.Join(',', deltaDbId) + " Zoom=" + zoom +
                                     ", RequestCancellation for JobID=" + kv.Value.ID + " (DeltaDbId=" + kv.Value.DeltaDbId + ")");
#endif
                     kv.Value.RequestCancellation();
                     RemoveJob(kv.Value.ID);
                  }
               }
            }
#if LOCALDEBUG
            listContent4Debug();
#endif
         }
      }

#if LOCALDEBUG
      void listContent4Debug() {
         Debug.WriteLine("   " + nameof(JobManager) + " [" + this + "]: List");
         foreach (var kv in jobs)
            Debug.WriteLine("   " + nameof(JobManager) + " [" + this + "]: " + kv.Value);
      }
#endif

      public override string ToString() => Name + ": " + jobs.Count.ToString() + " jobs";

   }
}
