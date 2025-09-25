using FSofTUtils;
using SpecialMapCtrl;
using System.Text.RegularExpressions;

/* In der ANDROID-Version wird page.IsBusy auf true bzw. false gesetzt.
 * Daraus könnte ev. die page eine entsprechende Anzeige erzeugen.
 */

#if ANDROID

namespace TrackEddi.Common {
#else

namespace GpxViewer.Common {
#endif

   public static class IOHelper {

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
      /// <param name="page"></param>
      /// <param name="gpx"></param>
      /// <param name="gpxbackupfile"></param>
      /// <param name="creator"></param>
      /// <param name="colors">Trackfarben</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns></returns>
      static public
#if ANDROID
         async Task<bool> SaveGpxBackup(Page page,
#else
         bool SaveGpxBackup(Form page,
#endif
                        GpxData gpx,
                        string gpxbackupfile,
                        string creator,
                        IList<System.Drawing.Color> colors,
                        bool withxmlcolor = false) {
         bool ok = true;
         if (gpx != null &&
             gpx.GpxDataChanged) {
            UIHelper.SetBusyStatus(page);
            try {
               if (!Directory.Exists(Path.GetDirectoryName(gpxbackupfile))) {
                  string? dir = Path.GetDirectoryName(gpxbackupfile);
                  if (dir != null)
                     Directory.CreateDirectory(dir);
               }
#if ANDROID
               await gpx.SaveAsyncWithLock(
#else
               gpx.SaveWithLock(
#endif
                        gpxbackupfile,
                        creator,
                        true,
                        colors,
                        FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
                        withxmlcolor);

               gpx.GpxDataChanged = false;
            } catch (Exception ex) {
               ok = false;
               UIHelper.SetBusyStatus(page, false);
#if ANDROID
               await UIHelper.ShowExceptionMessage(page,
#else
               UIHelper.ShowExceptionMessage(null,
#endif
                  "Fehler beim Speichern des Backups",
                  ex,
                  null,
                  false);
            } finally {
               UIHelper.SetBusyStatus(page, false);
            }
         }
         return ok;
      }

      /// <summary>
      /// speichert die GPX-Daten in einer oder mehreren Dateien
      /// </summary>
      /// <param name="page"></param>
      /// <param name="gpx"></param>
      /// <param name="filename"></param>
      /// <param name="multi"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns>true, wenn min. 1 Datei geschrieben wurde</returns>
      static public async Task<bool> SaveGpx(
#if ANDROID
                           Page page,
#else
                           Form page,
#endif
                           GpxData gpx,
                           string filename,
                           bool multi,
                           string creator,
                           bool withgarminextensions,
                           IList<System.Drawing.Color> colors,
                           bool withxmlcolor = false) {
         bool ok = false;
         string extension = Path.GetExtension(filename).ToLower();
         if (extension == "")     // dummy-Extension
            filename += ".gpx";
         else {
            if (!(extension == ".gpx" ||
                  extension == ".kml" ||
                  extension == ".kmz")) {
#if ANDROID
               await UIHelper.ShowErrorMessage(page,
#else
               UIHelper.ShowErrorMessage(
#endif
                  "Der Dateiname darf nicht mit '" + extension + "' enden (nur .gpx, .kml und .kmz erlaubt).");
               return ok;
            }
         }

         try {
            UIHelper.SetBusyStatus(page);
            if (!multi) {  // Einzeldatei
               ok =
#if ANDROID
                  await
#endif
                  savegpxfile(
                     page,
                     gpx,
                     filename,
                     creator,
                     withgarminextensions,
                     colors,
                     withxmlcolor);

            } else {    // mehrere Dateien

               (List<string> files, int canceled) =
                  await savegpxfiles(
                              page,
                              gpx,
                              filename,
                              creator,
                              withgarminextensions,
                              colors,
                              withxmlcolor);

               string filenames = "";
               if (files.Count > 0)
                  filenames = "   '" + string.Join("'" + Environment.NewLine + "   '", files) + "'";

               string msg = files.Count == 0 ?
                              "Es wurden KEINE Dateien geschrieben." :
                              files.Count == 1 ?
                                 "Die Datei" + Environment.NewLine + filenames + "wurde geschrieben." :
                                 "Die Dateien" + Environment.NewLine + filenames + "wurden geschrieben.";
               ok = files.Count > 0 && canceled == 0;
               UIHelper.SetBusyStatus(page, false);

#if ANDROID
               await UIHelper.ShowInfoMessage(page,
#else
               UIHelper.ShowInfoMessage(
#endif
                  msg);
            }

         } catch (Exception ex) {
            ok = false;
            UIHelper.SetBusyStatus(page, false);
#if ANDROID
            await UIHelper.ShowExceptionMessage(page,
#else
            UIHelper.ShowExceptionMessage(null,
#endif
               "Fehler beim Schreiben der GPX-Daten", ex, null, false);
         } finally {
            UIHelper.SetBusyStatus(page, false);
         }
         return ok;
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> werden jeweils in 1 Datei gespeichert und alle angezeigten <see cref="Marker"/> werden gemeinsam in einer Datei gespeichert
      /// </summary>
      /// <param name="page"></param>
      /// <param name="gpx"></param>
      /// <param name="basefilename"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <param name="colors">Trackfarben</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns>Liste der gespeicherten Dateien</returns>
      static async Task<(List<string>, int)> savegpxfiles(
#if ANDROID
                                       Page page,
#else
                                       Form page,
#endif
                                       GpxData gpx,
                                       string basefilename,
                                       string creator,
                                       bool withgarminextensions,
                                       IList<System.Drawing.Color> colors,
                                       bool withxmlcolor) {
         string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(basefilename);
         string fileNameExtension = Path.GetExtension(basefilename);
         string? path = Path.GetDirectoryName(basefilename);

         //Regex r = new Regex("[^A-Za-z0-9 \\-_.,;+äöüÄÖÜß!@€]");    // nur sichere Zeichen zulassen

         // Filenames: https://en.wikipedia.org/wiki/Filename

         // Prinzipiell besteht eine Abhängigkeit vom Betriebssystem UND vom Dateisystem.
#if ANDROID
         // Path.GetInvalidFileNameChars() liefert ev. nur '\0' und '/' (UNIX-üblich)
         // Für FAT sind weitere Zeichen verboten (außerdem "." und ".."), deshalb:
         Regex r = new Regex("[" + "\"*/:<>?\\|\x7F\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F" + "]");    // nur sichere Zeichen zulassen
#else
         // Path.GetInvalidPathChars() ist (unter Windows) eine Teilmenge von Path.GetInvalidFileNameChars()
         Regex r = new Regex("[" + new string(Path.GetInvalidFileNameChars()) + "]");    // nur sichere Zeichen zulassen
         // ABER: The array returned from this method is not guaranteed to contain
         //       the complete set of characters that are invalid in file and directory names.
#endif

         List<string> files = new List<string>();
         int count = 0;
         int canceled = 0;
         if (path != null) {
            for (int i = 0; i < gpx.TrackList.Count; i++) {
               Track track = gpx.TrackList[i];
               if (track.IsVisible) {
                  string newtrackfile = Path.Combine(path,
                                                     fileNameWithoutExtension + "_track" + count.ToString() + "_" +
                                                         r.Replace(track.GpxTrack.Name, "_") +
                                                         fileNameExtension);
#if ANDROID
                  if (await SaveGpx(
#else
                  if (await savegpxfile(
#endif
                                 page,
                                 newtrackfile,
                                 track,
                                 creator,
                                 withgarminextensions,
                                 colors[i],
                                 withxmlcolor)) {
                     files.Add(newtrackfile);
                     count++;
                  } else
                     canceled++;
               }
            }

            GpxData tmp = new GpxData();
            string newmarkerfile = Path.Combine(path, fileNameWithoutExtension + "_marker" + fileNameExtension);
            foreach (var marker in gpx.MarkerList)
               if (marker.IsVisible)
                  tmp.MarkerInsertCopyWithLock(marker);

            if (tmp.MarkerList.Count > 0) {
#if ANDROID
               if (await savegpxfile(page,
#else
               if (savegpxfile(page,
#endif
                     newmarkerfile,
                     tmp.MarkerList,
                     creator,
                     withgarminextensions,
                     withxmlcolor)) {
                  files.Add(newmarkerfile);
                  count++;
               } else
                  canceled++;
            }
         }

         return (files, canceled);
      }

      /// <summary>
      /// falls die Datei schon ex. kann der Vorgang abgebrochen werden
      /// </summary>
      /// <param name="page"></param>
      /// <param name="filename"></param>
      /// <returns></returns>
#if ANDROID
      static async Task<bool> cancel_write(Page page,
#else
      static bool cancel_write(Form page,
#endif
               string filename) {
         bool overwrite = false;
         if (File.Exists(filename)) {
            FileInfo fileinfo = new FileInfo(filename);
            DateTime dtLastWriteTime = fileinfo.LastWriteTime;
            long le = fileinfo.Length;
            UIHelper.SetBusyStatus(page, false);

#if ANDROID
            overwrite = await UIHelper.ShowYesNoQuestion_RealYes(page,
#else
            overwrite = UIHelper.ShowYesNoQuestion_RealYes(
#endif
                           "Die Datei '" + fileinfo.FullName + "' existiert schon. " + Environment.NewLine +
                           "(" + fileinfo.Length + " Bytes, " + fileinfo.LastWriteTime.ToString("G") + ")" + Environment.NewLine +
                           "Soll sie überschrieben werden?",
                           "Achtung");
            UIHelper.SetBusyStatus(page);
         } else
            return false;

         if (!overwrite) {
            UIHelper.SetBusyStatus(page, false);
#if ANDROID
            await UIHelper.ShowErrorMessage(page,
#else
            UIHelper.ShowErrorMessage(
#endif
               "Die Datei '" + filename + "' wird NICHT überschrieben.",
               "Abbruch des Speicherns");
            UIHelper.SetBusyStatus(page);

         }

         return !overwrite;
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> und <see cref="Marker"/> werden gemeinsam als Datei gespeichert
      /// <para>Falls die Zieldatei schon ex. erfolgt eine Rückfrage ob sie überschrieben werden soll.</para>
      /// </summary>
      /// <param name="page"></param>
      /// <param name="gpx"></param>
      /// <param name="filename"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <param name="colors">Trackfarben</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
#if ANDROID
      static async Task<bool> savegpxfile(Page page,
#else
      static bool savegpxfile(Form page,
#endif
               GpxData gpx,
               string filename,
               string creator,
               bool withgarminextensions,
               IList<System.Drawing.Color> colors,
               bool withxmlcolor) {
         if (
#if ANDROID
             await
#endif
             cancel_write(page, filename))
            return false;

         GpxData tmp = new GpxData(gpx.AsXml(int.MaxValue));

         // akt. "unsichtbare" Tracks und Marker entfernen
         List<System.Drawing.Color> alltrackcolors = new(colors);
         for (int i = tmp.TrackList.Count - 1; i >= 0; i--)
            if (!gpx.TrackList[i].IsVisible) {     // Original!
               tmp.TrackRemoveWithLock(i);
               alltrackcolors.RemoveAt(i);
            }
         for (int i = tmp.MarkerList.Count - 1; i >= 0; i--)
            if (!gpx.MarkerList[i].IsVisible)
               tmp.MarkerRemoveWithLock(i);

         bool ok = true;
         if (tmp.TrackList.Count + tmp.MarkerList.Count > 0)
#if ANDROID
            await tmp.SaveAsyncWithLock(
#else
            tmp.SaveWithLock(
#endif
                        filename,
                        creator,
                        withgarminextensions,
                        alltrackcolors.ToArray(),
                        FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
                        withxmlcolor);
         else
            ok = false;

         UIHelper.SetBusyStatus(page, false);
#if ANDROID
         await UIHelper.ShowInfoMessage(page,
#else
         UIHelper.ShowInfoMessage(
#endif
            "Die Datei '" + filename + "' wurde " + (ok ? "" : "NICHT") + " geschrieben.");

         return ok;
      }

      /// <summary>
      /// ein einzelner <see cref="Track"/> wird als Datei gespeichert
      /// <para>Falls die Zieldatei schon ex. erfolgt eine Rückfrage ob sie überschrieben werden soll.</para>
      /// </summary>
      /// <param name="page"></param>
      /// <param name="filename"></param>
      /// <param name="track"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <param name="color">Trackfarbe</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns>true wenn gespeichert</returns>
#if ANDROID
      static public async Task<bool> SaveGpx(Page page,
#else
      static public async Task<bool> savegpxfile(Form page,
#endif
               string filename,
               Track track,
               string creator,
               bool withgarminextensions,
               System.Drawing.Color color,
               bool withxmlcolor = false) {
         if (
#if ANDROID
             await
#endif
             cancel_write(page, filename))
            return false;

         GpxData tmp = new GpxData();
         tmp.TrackInsertCopyWithLock(track, -1, true);
         await tmp.SaveAsyncWithLock(
                        filename,
                        creator,
                        withgarminextensions,
                        [color],
                        FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
                        withxmlcolor);
         return true;
      }

      /// <summary>
      /// alle <see cref="Marker"/> der Liste werden als Datei gespeichert
      /// <para>Falls die Zieldatei schon ex. erfolgt eine Rückfrage ob sie überschrieben werden soll.</para>
      /// </summary>
      /// <param name="page"></param>
      /// <param name="filename"></param>
      /// <param name="markerlst"></param>
      /// <param name="creator">Erzeuger der XML-Datei</param>
      /// <param name="withgarminextensions">Mit Garmin-Erweiterungen?</param>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns>true wenn Datei gespeichert wurde</returns>
#if ANDROID
      static async Task<bool> savegpxfile(Page page,
#else
      static bool savegpxfile(Form page,
#endif
               string filename,
               IList<Marker> markerlst,
               string creator,
               bool withgarminextensions,
               bool withxmlcolor) {
         if (
#if ANDROID
             await
#endif
             cancel_write(page, filename))
            return false;

         GpxData tmp = new GpxData();
         foreach (var item in markerlst)
            tmp.MarkerInsertCopyWithLock(item);
         if (tmp.MarkerList.Count > 0) {
#if ANDROID
            await tmp.SaveAsyncWithLock(
#else
            tmp.SaveWithLock(
#endif
               filename,
               creator,
               withgarminextensions,
               [],
               FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
               withxmlcolor);
            return true;
         }
         return false;
      }

#if ANDROID
      static public async Task<bool> Load(Page page,
#else
      static public bool Load(Form page,
#endif
                    GpxData destgpx,
                    string file,
                    bool append,
                    bool appendatendoflist,
                    double linewidth,
                    double symbolzoomfactor,
                    System.Drawing.Color dummytrackcolor
           ) {
         bool result = true;
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
               txt1 += (destgpx.TrackList.Count + destgpx.MarkerList.Count) > 1 ? " sind" : " ist";
               txt1 += " breits vorhandenen. Sollen diese Daten ALLE überschrieben werden?";

#if ANDROID
               bool overwrite = await UIHelper.ShowYesNoQuestion_RealYes(page,
#else
               bool overwrite = UIHelper.ShowYesNoQuestion_RealYes(
#endif
                                   txt1,
                                   "Achtung");
               if (!overwrite)
                  return false;
            }

            UIHelper.SetBusyStatus(page);

            if (!append) {
               destgpx.TrackRemoveAllWithLock();
               destgpx.MarkerRemoveAllWithLock();
            }

            GpxData gpxnew = new GpxData();
            List<System.Drawing.Color> trackcolors = gpxnew.Load(file, true, dummytrackcolor);

            for (int i = 0; i < gpxnew.TrackList.Count; i++) {
               Track track = destgpx.TrackInsertCopyWithLock(gpxnew.TrackList[i],
                                                     appendatendoflist ? -1 : i,    // an den Anfang oder das Ende der Liste einfügen
                                                     true);
               track.LineColor = trackcolors[i];
               track.LineWidth = linewidth;
            }
            for (int i = 0; i < gpxnew.MarkerList.Count; i++) {
               Marker marker = destgpx.MarkerInsertCopyWithLock(gpxnew.MarkerList[i],
                                                        appendatendoflist ? -1 : i);   // an den Anfang oder das Ende der Liste einfügen
               marker.Symbolzoom = symbolzoomfactor;
            }

            UIHelper.SetBusyStatus(page, false);
#if ANDROID
            await UIHelper.ShowInfoMessage(page,
#else
            UIHelper.ShowInfoMessage(
#endif
               "Die Datei '" + file + "' wurde eingelesen (Tracks: " + gpxnew.TrackList.Count + ", Marker: " + gpxnew.MarkerList.Count + ").");

         } catch (Exception ex) {
            result = false;
            UIHelper.SetBusyStatus(page, false);
#if ANDROID
            await UIHelper.ShowExceptionMessage(page,
#else
            UIHelper.ShowExceptionMessage(null,
#endif
               "Fehler beim Lesen der GPX-Daten",
               ex,
               null,
               false);
         } finally {
            UIHelper.SetBusyStatus(page, false);
         }
         return result;
      }


      //      static void SetBusyStatus(
      //#if ANDROID
      //                           Page page,
      //#else
      //                           Form page,
      //#endif
      //                           bool busy = true) =>
      //         SetBusyStatusEvent?.Invoke(null, new BusyEventArgs(page, busy));

   }
}
