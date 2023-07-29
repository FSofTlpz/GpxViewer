using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SpecialMapCtrl {

   /// <summary>
   /// zum Testen ob Gpx-Dateien Daten enthalten, die einen rechteckigen Koordinatenbereich betreffen
   /// </summary>
   class CheckRouteCrossing {

      int checkedfiles = 0;
      int foundfiles = 0;

      public Task TestpathsAsync(IList<string> path, List<string> gpxfiles,
                                double lonfrom, double lonto, double latfrom, double latto,
                                Action<int, int, bool, string> infoaction) {
         return Task.Run(() => Testpaths(path, gpxfiles, lonfrom, lonto, latfrom, latto, infoaction));
      }

      public void Testpaths(IList<string> path, List<string> gpxfiles,
                            double lonfrom, double lonto, double latfrom, double latto,
                            Action<int, int, bool, string> infoaction) {
         checkedfiles = foundfiles = 0;
         gpxfiles.Clear();
         foreach (var item in path)
            testpath(item, gpxfiles, lonfrom, lonto, latfrom, latto, infoaction);
      }

      void testpath(string path, List<string> gpxfiles,
                    double lonfrom, double lonto, double latfrom, double latto,
                    Action<int, int, bool, string> infoaction) {
         string[] dirs = Directory.GetDirectories(path);
         string[] files = Directory.GetFiles(path, "*.gpx");
         foreach (string file in files) {
            checkedfiles++;
            if (checkfile(file, lonfrom, lonto, latfrom, latto)) {
               gpxfiles.Add(file);
               infoaction(checkedfiles, ++foundfiles, true, file);
            } else
               infoaction(checkedfiles, foundfiles, false, file);
         }
         foreach (string dir in dirs)
            testpath(dir, gpxfiles, lonfrom, lonto, latfrom, latto, infoaction);
      }

      bool checkfile(string gpxfile, double lonfrom, double lonto, double latfrom, double latto) {
         try {
            GpxAllExt gpx = new GpxAllExt();
            return gpx.CheckAreaFromFile(gpxfile,
                                         new FSofTUtils.Geography.PoorGpx.GpxBounds(latfrom, latto, lonfrom, lonto),
                                         false,
                                         (b, p) => RouteCrossing.IsRouteCrossing(b, p, null));
         } catch (System.Exception ex) {
            Debug.WriteLine("Exception bei " + gpxfile + ": " + ex.Message);
         }
         return false;
      }

   }
}
