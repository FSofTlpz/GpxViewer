using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialMapCtrl {

   /// <summary>
   /// zum Testen ob Gpx-Dateien Daten enthalten, die einen rechteckigen Koordinatenbereich betreffen
   /// </summary>
   public class CheckRouteCrossing {

      int checkedfiles = 0;
      int foundfiles = 0;
      bool isrunning = false;

      long _isCancellationRequested = 0;

      bool isCancellationRequested {
         get => Interlocked.Read(ref _isCancellationRequested) != 0;
         set => Interlocked.Exchange(ref _isCancellationRequested, value ? 1 : 0);
      }


      /// <summary>
      /// Abbruch eines ev. laufenden Tests
      /// </summary>
      public void CancelTest() {
         if (isrunning)
            isCancellationRequested = true;
         isrunning = false;
      }

      /// <summary>
      /// testet in allen Verzeichnissen und deren Unterverzeichnissen die GPX-Dateien
      /// </summary>
      /// <param name="path">Verzeichnisse</param>
      /// <param name="resultgpxfiles">Liste der gefundenen GPX-Dateien (wird neu gefüllt)</param>
      /// <param name="lonfrom">von Länge</param>
      /// <param name="lonto">bis Länge</param>
      /// <param name="latfrom">von Breite</param>
      /// <param name="latto">bis Breite</param>
      /// <param name="infoaction">Benachrichtungsfunktion</param>
      /// <param name="cts">für den vorzeitigen Abbruch</param>
      /// <returns></returns>
      public async Task TestpathsAsync(IList<string> path,
                                       List<string> resultgpxfiles,
                                       double lonfrom, double lonto, double latfrom, double latto,
                                       Action<int, int, bool, string> infoaction) {
         if (isrunning)
            return;

         checkedfiles = foundfiles = 0;
         isCancellationRequested = false;
         resultgpxfiles.Clear();
         isrunning = true;

         await Task.Run(async () => {     // gesamte Suche in einem eigenen Task (Thread) damit die UI etwas besser reagieren kann
            foreach (var item in path)
               await testpath(item, resultgpxfiles, lonfrom, lonto, latfrom, latto, infoaction);
         });

         isrunning = false;
      }

      async Task testpath(string path,
                          List<string> gpxfiles,
                          double lonfrom, double lonto, double latfrom, double latto,
                          Action<int, int, bool, string>? infoaction) {
         string[] dirs = Directory.GetDirectories(path);
         Array.Sort(dirs);
         if (isCancellationRequested)
            return;
         string[] files = Directory.GetFiles(path, "*.gpx");
         Array.Sort(files);
         if (isCancellationRequested)
            return;
         foreach (string file in files) {
            if (isCancellationRequested)
               return;
            checkedfiles++;
            if (await checkfile(file, lonfrom, lonto, latfrom, latto)) {
               gpxfiles.Add(file);
               if (infoaction != null)
                  infoaction(checkedfiles, ++foundfiles, true, file);
            } else {
               if (infoaction != null)
                  infoaction(checkedfiles, foundfiles, false, file);
            }
         }
         foreach (string dir in dirs) {
            if (isCancellationRequested)
               return;
            await testpath(dir, gpxfiles, lonfrom, lonto, latfrom, latto, infoaction);
         }
      }

      async Task<bool> checkfile(string gpxfile, double lonfrom, double lonto, double latfrom, double latto) {
         try {
            GpxData gpx = new GpxData();
            return await gpx.CheckAreaFromFileAsync(gpxfile,
                                                    new FSofTUtils.Geography.PoorGpx.GpxBounds(latfrom, latto, lonfrom, lonto),
                                                    false,
                                                    (b, p) => RouteCrossing.IsRouteCrossing(b, p, null));
         } catch (Exception ex) {
            Debug.WriteLine("Exception bei " + gpxfile + ": " + ex.Message);
         }
         return false;
      }

   }
}
