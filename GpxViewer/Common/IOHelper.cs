using FSofTUtils;
using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#if Android
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   internal static class IOHelper {

      /// <summary>
      /// liefert den abs. Pfad (bezüglich des akt. Verzeichnisses) und ersetzt gegebenenfalls Umgebungsvariablen
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      static public string GetFullPath(string path) {
         if (!string.IsNullOrEmpty(path)) {
            path = PathHelper.ReplaceEnvironmentVars(path);
            if (!Path.IsPathRooted(path))
               path = Path.GetFullPath(path);   //Path.Combine(FirstVolumePath, DATAPATH, path);
         }
         return path;
      }

      /// <summary>
      /// i.A. zum Speichern einer Workbench-Datei
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="gpx"></param>
      /// <param name="gpxbackupfile"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      static public
#if Android
         async Task SaveGpxBackup(
                       Page page,
#else
         void SaveGpxBackup(
#endif
                       GpxAllExt gpx,
                       string gpxbackupfile,
                       string creator,
                       GarminTrackColors.Colorname nocolor = GarminTrackColors.Colorname.DarkGray) {
         if (gpx != null &&
             gpx.GpxDataChanged) {
            try {
               if (!Directory.Exists(Path.GetDirectoryName(gpxbackupfile)))
                  Directory.CreateDirectory(Path.GetDirectoryName(gpxbackupfile));
               gpx.Save(gpxbackupfile, creator, true, nocolor);
               gpx.GpxDataChanged = false;
            } catch (Exception ex) {
#if Android
               await UIHelper.ShowExceptionMessage(
                     page,
#else
               UIHelper.ShowExceptionMessage(
                        null,
#endif
                        "Fehler beim Speichern des Backups",
                        ex,
                        null,
                        false);
            }
         }
      }

      /// <summary>
      /// speichert die GPX-Daten in einer oder mehreren Dateien
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="gpx"></param>
      /// <param name="filename"></param>
      /// <param name="multi"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <returns>true, wenn min. 1 Datei geschrieben wurde</returns>
#if Android
      static public async Task<bool> SaveGpx(Page page,
#else
      static public bool SaveGpx(
#endif
                           GpxAllExt gpx,
                           string filename,
                           bool multi,
                           string creator,
                           bool withgarminextensions,
                           GarminTrackColors.Colorname nocolor = GarminTrackColors.Colorname.DarkGray) {
         bool ok = false;
         string extension = Path.GetExtension(filename).ToLower();
         if (extension == "")     // dummy-Extension
            filename += ".gpx";
         else {
            if (!(extension == ".gpx" ||
                  extension == ".kml" ||
                  extension == ".kmz")) {
#if Android
            await UIHelper.ShowErrorMessage(
                        page,
#else
               UIHelper.ShowErrorMessage(
#endif
                           "Der Dateiname darf nicht mit '" + extension + "' enden (nur .gpx, .kml und .kmz erlaubt).");
               return ok;
            }
         }

         try {
            int count = 0;
            if (!File.Exists(filename)) {
               if (multi) {
                  count =
#if Android
                     await savegpxfiles(page,
#else
                     savegpxfiles(
#endif
                                 gpx, filename, false, creator, withgarminextensions, nocolor);
               } else {
#if Android
                  if (await savegpxfile(page,
#else
                  if (savegpxfile(
#endif
                                 gpx, filename, false, creator, withgarminextensions, nocolor)) {
                     count++;
                  }
               }
            } else {
#if Android
            bool overwrite = await UIHelper.ShowYesNoQuestion_StdIsYes(
                                 page,
#else
               bool overwrite = UIHelper.ShowYesNoQuestion_IsYes(
#endif
                                    "Die Datei '" + filename + "' existiert schon. Soll sie überschrieben werden?",
                                    "Achtung");
               if (overwrite)
                  if (multi) {
                     count =
#if Android
                        await savegpxfiles(page,
#else
                        savegpxfiles(

#endif
                                    gpx, filename, true, creator, withgarminextensions, nocolor);
                  } else {
#if Android
                     if (await savegpxfile(page,
#else
                     if (savegpxfile(
#endif
                                 gpx, filename, true, creator, withgarminextensions, nocolor)) {
                        count++;
                     }
                  }
            }

            if (count > 0) {
#if Android
            await UIHelper.ShowInfoMessage(
                              page,
#else
               UIHelper.ShowInfoMessage(
#endif
                                 multi ?
                                    count + " Dateien '" + filename + "' wurden geschrieben." :
                                    "Die Datei '" + filename + "' wurde geschrieben.");
               ok = true;
            } else
#if Android
            await UIHelper.ShowInfoMessage(
                              page,
#else
               UIHelper.ShowInfoMessage(
#endif
                                 "Die Datei '" + filename + "' wurde NICHT geschrieben.");
         } catch (Exception ex) {
#if Android
            await UIHelper.ShowExceptionMessage(
                              page,
#else
            UIHelper.ShowExceptionMessage(null,
#endif
                        "Fehler beim Schreiben der GPX-Daten", ex, null, false);
         }
         return ok;
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> werden jeweils in 1 Datei gespeichert und alle angezeigten <see cref="Marker"/> werden gemeinsam in einer Datei gespeichert
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="gpx"></param>
      /// <param name="basefilename"></param>
      /// <param name="overwrite"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <returns>Anzahl der gespeicherten Dateien</returns>
#if Android
      static async Task<int> savegpxfiles(
                     Page page,
#else
      static int savegpxfiles(
#endif
                     GpxAllExt gpx,
                     string basefilename,
                     bool overwrite,
                     string creator,
                     bool withgarminextensions,
                     GarminTrackColors.Colorname nocolor) {
         string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(basefilename);
         string fileNameExtension = Path.GetExtension(basefilename);
         string path = Path.GetDirectoryName(basefilename);

         Regex r = new Regex("[^A-Za-z0-9 \\-_.,;+*äöüÄÖÜß!@€]");    // nur sichere Zeichen zulassen
         int count = 0;
         foreach (var track in gpx.TrackList)
            if (track.IsVisible) {
#if Android
               if (await savegpxfile(page,
#else
               if (savegpxfile(
#endif
                              Path.Combine(path,
                                           fileNameWithoutExtension + "_track" + count.ToString() + "_" +
                                                      r.Replace(track.GpxTrack.Name, "_") +
                                                      fileNameExtension),
                              track,
                              overwrite,
                              creator,
                              withgarminextensions,
                              nocolor))
                  count++;
            }

         GpxAllExt tmp = new GpxAllExt();
         foreach (var marker in gpx.MarkerList)
            if (marker.IsVisible)
               tmp.MarkerInsertCopy(marker);
         if (tmp.MarkerList.Count > 0 &&
#if Android
            await savegpxfile(page,
#else
            savegpxfile(
#endif
                        Path.Combine(path, fileNameWithoutExtension + "_marker" + fileNameExtension),
                        tmp.MarkerList,
                        overwrite,
                        creator,
                        withgarminextensions,
                        nocolor))
            count++;

         return count;
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> und <see cref="Marker"/> werden als Datei gespeichert
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="gpx"></param>
      /// <param name="filename"></param>
      /// <param name="overwrite"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
#if Android
      static async Task<bool> savegpxfile(
               Page page,
#else
      static bool savegpxfile(
#endif
               GpxAllExt gpx,
               string filename,
               bool overwrite,
               string creator,
               bool withgarminextensions,
               GarminTrackColors.Colorname nocolor) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt(gpx.AsXml(int.MaxValue));

            // akt. "unsichtbare" Tracks und Marker entfernen
            for (int i = tmp.TrackList.Count - 1; i >= 0; i--)
               if (!gpx.TrackList[i].IsVisible)    // Original!
                  tmp.TrackRemove(i);
            for (int i = tmp.MarkerList.Count - 1; i >= 0; i--)
               if (!gpx.MarkerList[i].IsVisible)
                  tmp.MarkerRemove(i);

            tmp.Save(filename, creator, withgarminextensions, nocolor);

            return true;
         }
#if Android
         await UIHelper.ShowErrorMessage(
               page,
#else
         UIHelper.ShowErrorMessage(
#endif
               "Die Datei '" + filename + "' existiert schon.",
               "Abbruch des Speicherns");
         return false;
      }

      /// <summary>
      /// ein einzelner <see cref="Track"/> wird als Datei gespeichert
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="filename"></param>
      /// <param name="track"></param>
      /// <param name="overwrite"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <returns>true wenn gespeichert</returns>
#if Android
      static async Task<bool> savegpxfile(
               Page page,
#else
      static bool savegpxfile(
#endif
               string filename,
               Track track,
               bool overwrite,
               string creator,
               bool withgarminextensions,
               GarminTrackColors.Colorname nocolor) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt();
            tmp.TrackInsertCopy(track, -1, true);
            tmp.Save(filename, creator, withgarminextensions, nocolor);
            return true;
         }
#if Android
         await UIHelper.ShowErrorMessage(
               page,
#else
         UIHelper.ShowErrorMessage(
#endif
               "Die Datei '" + filename + "' existiert schon.",
               "Abbruch des Speicherns");
         return false;
      }

      /// <summary>
      /// alle <see cref="Marker"/> der Liste werden als Datei gespeichert
      /// </summary>
#if Android
      /// <param name="page"></param>
#endif
      /// <param name="filename"></param>
      /// <param name="markerlst"></param>
      /// <param name="overwrite"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <returns>true wenn gespeichert</returns>
#if Android
      static async Task<bool> savegpxfile(
               Page page,
#else
      static bool savegpxfile(
#endif
               string filename,
               IList<Marker> markerlst,
               bool overwrite,
               string creator,
               bool withgarminextensions,
               GarminTrackColors.Colorname nocolor) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt();
            foreach (var item in markerlst)
               tmp.MarkerInsertCopy(item);
            if (tmp.MarkerList.Count > 0) {
               tmp.Save(filename, creator, withgarminextensions, nocolor);
               return true;
            }
         } else
#if Android
            await UIHelper.ShowErrorMessage(
                  page,
#else
            UIHelper.ShowErrorMessage(
#endif
                  "Die Datei '" + filename + "' existiert schon.",
                  "Abbruch des Speicherns");

         return false;
      }

#if Android
      static public async Task Load(
                    Page page,
#else
      static public void Load(
#endif
                    GpxAllExt destgpx,
                    string file,
                    bool append,
                    double linewidth,
                    double symbolzoomfactor

           ) {
         try {
            if (!append &&
                (destgpx.TrackList.Count > 0 ||
                 destgpx.MarkerList.Count > 0)) {
               string txt1 = destgpx.TrackList.Count == 0 ?
                                 "" :
                                 destgpx.TrackList.Count == 1 ?
                                    "1 Track" :
                                    destgpx.TrackList.Count.ToString() + " Tracks";
               string txt2 = destgpx.MarkerList.Count == 0 ?
                                 "" :
                                 destgpx.MarkerList.Count == 1 ?
                                    "1 Marker" :
                                    destgpx.MarkerList.Count.ToString() + " Marker";

               if (txt1.Length > 0 && txt2.Length > 0) {
                  txt1 += " und " + txt2;
               } else {
                  if (txt2.Length > 0)
                     txt1 = txt2;
               }
               txt1 += (destgpx.TrackList.Count + destgpx.MarkerList.Count > 1) ? " ist" : " sind";
               txt1 += " breits vorhandenen. Sollen diese Daten überschrieben werden?";

#if Android
               bool overwrite = await UIHelper.ShowYesNoQuestion_StdIsYes(
                                       page,
#else
               bool overwrite = UIHelper.ShowYesNoQuestion_IsYes(
#endif
                                       txt1,
                                       "Achtung");
               if (!overwrite)
                  return;
            }

            GpxAllExt gpxnew = new GpxAllExt();
            gpxnew.Load(file, true);

            if (!append) {
               destgpx.TrackRemoveAll();
               destgpx.MarkerRemoveAll();
            }

            for (int i = 0; i < gpxnew.TrackList.Count; i++) {
               Track track = destgpx.TrackInsertCopy(gpxnew.TrackList[i]);
               track.LineWidth = linewidth;
            }
            for (int i = 0; i < gpxnew.MarkerList.Count; i++) {
               Marker marker = destgpx.MarkerInsertCopy(gpxnew.MarkerList[i]);
               marker.Symbolzoom = symbolzoomfactor;
            }

#if Android
            await UIHelper.ShowInfoMessage(
                              page,
#else
            UIHelper.ShowInfoMessage(
#endif
                              "Die Datei '" + file + "' wurde eingelesen (Tracks: " + gpxnew.TrackList.Count + ", Marker: " + gpxnew.MarkerList.Count + ").");

         } catch (Exception ex) {
#if Android
            await UIHelper.ShowExceptionMessage(
                  page,
#else
            UIHelper.ShowExceptionMessage(null,
#endif
                  "Fehler beim Lesen der GPX-Daten",
                  ex,
                  null,
                  false);
         }
      }




   }
}
