//#define LOCALDEBUG

using GarminCore;
using GarminCore.DskImg;
using GarminCore.Files;
using GarminCore.OptimizedReader;
using GarminImageCreator.Garmin.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GarminImageCreator.Garmin {

   internal class OptimizedGeoDataReader : IDisposable {

      string mappath;

      File_TDB tdb;

      /// <summary>
      /// erspart z.T. das Umrechnen der Raw-Daten und das Erzeugen der Geo-Objekte
      /// </summary>
      SubdivMapDataCache? sdcache;

      string? overviewMapFilename;

      int overviewMaxBitLevel;

      /// <summary>
      /// geografische Breite des Mittelpunktes
      /// </summary>
      public readonly double CenterLat;


      public OptimizedGeoDataReader(string mappath, string tdbfile, int cachesize = 0) {
         this.mappath = mappath;

         sdcache = cachesize > 0 ? new SubdivMapDataCache(cachesize) : null;

         tdb = new File_TDB();
         using (BinaryReaderWriter tdbreader = new BinaryReaderWriter(Path.Combine(mappath, tdbfile))) {
            tdb.Read(tdbreader);
         }

         double north = -90;
         double south = 90;
         foreach (var tm in tdb.Tilemap) {
            north = Math.Max(north, tm.North);
            south = Math.Min(south, tm.South);
         }
         CenterLat = south + (north - south) / 2;

         registerOverviewData();
      }

      bool isCancellationRequested(CancellationToken? cancellationToken) => cancellationToken != null && cancellationToken.Value.IsCancellationRequested;


      /// <summary>
      /// holt alle geografischen Daten für diesen Bereich
      /// </summary>
      /// <param name="area">geografischer Bereich</param>
      /// <param name="bits">Daten für diese Bitanzahl (10..24) (Zoom)</param>
      /// <param name="areas">Liste der Gebiete</param>
      /// <param name="lines">Liste der Linien</param>
      /// <param name="points">Liste der Punkte</param>
      /// <param name="imgreaddirect">bei true wird direkt aus der Datei (ohnePuffer im Hauptspeicher) gelesen; etwas langsamer aber weniger Speicher nötig</param>
      /// <param name="cancellationToken"></param>
      /// <returns>false bei Abbruch (muss threadsicher gelesen werden !)</returns>
      public bool Read(Bound area,
                       int bits,          // 10..24
                       out List<GeoPoly> areas,
                       out List<GeoPoly> lines,
                       out List<GeoPoint> points,
                       bool imgreaddirect,
                       CancellationToken? cancellationToken) {
         areas = new List<GeoPoly>();
         lines = new List<GeoPoly>();
         points = new List<GeoPoint>();

         if (0 < overviewMaxBitLevel &&
             bits <= overviewMaxBitLevel) {
            Bound rectTile = new Bound(tdb.Overviewmap.West,
                                       tdb.Overviewmap.East,
                                       tdb.Overviewmap.South,
                                       tdb.Overviewmap.North);



            if (isCancellationRequested(cancellationToken))
               return false;
            if (area.IsOverlapped(rectTile)) {
               read(uint.MaxValue,
                    area,
                    bits,
                    areas,
                    lines,
                    points,
                    imgreaddirect,
                    cancellationToken);
            }
         } else {
            // notwendige Tiles ermitteln und deren Daten einlesen
            for (int i = 0; i < tdb.Tilemap.Count; i++) {
               Bound rectTile = new Bound(tdb.Tilemap[i].West,
                                          tdb.Tilemap[i].East,
                                          tdb.Tilemap[i].South,
                                          tdb.Tilemap[i].North);
               if (area.IsOverlapped(rectTile)) {
                  if (isCancellationRequested(cancellationToken))
                     return false;
                  read(tdb.Tilemap[i].Mapnumber,
                       area,
                       bits,
                       areas,
                       lines,
                       points,
                       imgreaddirect,
                       cancellationToken);
               }
            }
         }
         return !isCancellationRequested(cancellationToken);
      }

      void registerOverviewData() {
         if (tdb.Overviewmap != null &&
             tdb.Overviewmap.Mapnumber > 0) {
            overviewMapFilename = "";
            foreach (string imgfile in Directory.GetFiles(mappath, "*.img", SearchOption.TopDirectoryOnly)) {
               string basename = Path.GetFileNameWithoutExtension(imgfile);

               int number = basenameAsNumber(basename);
               if (number > 0) {
                  if (number == tdb.Overviewmap.Mapnumber) {
                     overviewMapFilename = imgfile;
                     break;
                  }
               } else {
                  SimpleFilesystemReadOnly sro = new SimpleFilesystemReadOnly(imgfile);
                  for (int i = 0; i < sro.FileCount; i++) {
                     string filename = sro.Filename(i);
                     if (Path.GetExtension(filename).ToUpper() == ".TRE") {
                        number = basenameAsNumber(Path.GetFileNameWithoutExtension(filename));
                        if (number == tdb.Overviewmap.Mapnumber) {
                           overviewMapFilename = imgfile;
                           break;
                        }
                     }
                  }
               }
               if (!string.IsNullOrEmpty(overviewMapFilename)) {
                  using (BinaryReaderWriter imgreader = getBinaryReaderWriter4File(overviewMapFilename, true)) {
                     ImgReader img = new ImgReader(imgreader);
                     for (int i = 0; i < img.FileCount; i++) {
                        string filename = img.Filename(i);
                        if (Path.GetExtension(filename).ToUpper() == ".TRE") {
                           using (BinaryReaderWriter? trereader = img.GetBinaryReaderWriter4File(filename)) {
                              if (trereader != null) {
                                 GarminCore.OptimizedReader.StdFile_TRE tre = new GarminCore.OptimizedReader.StdFile_TRE();
                                 tre.ReadMinimalData(trereader);
                                 overviewMaxBitLevel = 0;
                                 for (int m = 0; m < tre.MaplevelList.Count; m++)
                                    overviewMaxBitLevel = Math.Max(overviewMaxBitLevel, tre.MaplevelList[m].CoordBits);
                              }
                           }
                           break;
                        }
                     }
                  }
                  break;
               }
            }
         }
      }

      static int basenameAsNumber(string basename) {
         for (int i = 0; i < basename.Length; i++)
            if (!char.IsDigit(basename[i]))
               return -1;
         return Convert.ToInt32(basename);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="readdirect">direkt aus der Datei lesen oder die gesamte Datei in einen Puffer einlesen</param>
      /// <returns></returns>
      BinaryReaderWriter getBinaryReaderWriter4File(string filename, bool readdirect) {
         if (readdirect) {
            return new BinaryReaderWriter(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
         } else {
            byte[]? buffer = null;
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
               buffer = new byte[stream.Length];
               int len = stream.Read(buffer, 0, buffer.Length);
               if (len != buffer.Length)
                  throw new Exception(nameof(GarminGraphicData) + "." + nameof(getBinaryReaderWriter4File) + "(): Nicht genug Daten gelesen.");
            }
            return new BinaryReaderWriter(buffer, 0, buffer.Length, null, false);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="img"></param>
      /// <param name="mapnumber"></param>
      /// <param name="extension"></param>
      /// <param name="readdirect">bei false wird explizit ein eigener Puffer im Hauptspeicher mit den Date erzeugt</param>
      /// <returns></returns>
      BinaryReaderWriter? getBinaryReaderWriter4PseudoFile(ImgReader img, uint mapnumber, string extension, bool readdirect) {
         if (readdirect)
            return img.GetBinaryReaderWriter4File(mapnumber.ToString() + extension);
         else {
            byte[] buffer = Array.Empty<byte>();
            using (BinaryReaderWriter? br = img.GetBinaryReaderWriter4File(mapnumber.ToString() + extension)) {
               if (br != null) {
                  buffer = new byte[br.Length];
                  br.Read(buffer, 0, buffer.Length);
               }
            }
            return new BinaryReaderWriter(buffer, 0, buffer.Length, null, false);
         }
      }

      void read(uint mapnumber,
                Bound area,
                int bits,          // 10..24
                List<GeoPoly> areas,
                List<GeoPoly> lines,
                List<GeoPoint> points,
                bool imgreaddirect,
                CancellationToken? cancellationToken) {
         DateTime start = DateTime.Now;

         try {
            string? file = mapnumber < uint.MaxValue ?
                              Path.Combine(mappath, mapnumber.ToString() + ".img") :
                              overviewMapFilename;

            if (mapnumber == uint.MaxValue)
               mapnumber = tdb.Overviewmap.Mapnumber;
#if LOCALDEBUG
            Debug.WriteLine(nameof(OptimizedGeoDataReader) + "." + nameof(read) + ": " + file);
#endif
            if (file != null)
               using (BinaryReaderWriter imgreader = getBinaryReaderWriter4File(file, imgreaddirect)) {
                  // liest das Dateisystem, aber nicht die Dateien ein
                  ImgReader img = new ImgReader(imgreader);

                  GarminCore.OptimizedReader.StdFile_TRE tre = new GarminCore.OptimizedReader.StdFile_TRE();
                  GarminCore.OptimizedReader.StdFile_TRE.SubdivInfoBasic[]? tresubdiv = null;

                  using (BinaryReaderWriter? trereader = getBinaryReaderWriter4PseudoFile(img, mapnumber, ".TRE", true)) {
                     if (trereader != null) {
                        tre.ReadMinimalData(trereader);

                        // passenden maplevel suchen
                        GarminCore.OptimizedReader.StdFile_TRE.MapLevel ml = tre.MaplevelList[tre.MaplevelList.Count - 1];
                        for (int m = 1; m < tre.MaplevelList.Count; m++) {  // Level 0 hat die niedrigste Bitanzahl und ist i.A. (immer ?) leer
                           if (tre.MaplevelList[m].CoordBits >= bits) {
                              ml = tre.MaplevelList[m];
                              break;
                           }
                        }

                        if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                           Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": A " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                           return;
                        } // else Thread.Sleep(1000);

                        // Index-Liste der betroffenen Subdivs ermitteln
                        List<int> subdividxlst = tre.GetSubdivIdxList(ml.FirstSubdivInfoNumber - 1,
                                                                   ml.SubdivInfos,
                                                                   area,
                                                                   ml.CoordBits);
                        tresubdiv = tre.GetSubdivs(subdividxlst);
                        if (tresubdiv.Length > 0) {
                           GarminCore.OptimizedReader.StdFile_RGN rgn = new GarminCore.OptimizedReader.StdFile_RGN(tre);

                           if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                              Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": B " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                              return;
                           } // else Thread.Sleep(1000);

                           BinaryReaderWriter? rgnreader = getBinaryReaderWriter4PseudoFile(img, mapnumber, ".RGN", true);
                           if (rgnreader != null) {
                              rgn.SetValidSubdivIdx(subdividxlst);
                              rgn.ReadMinimalData(rgnreader);

                              if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                                 Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": C " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                                 return;
                              } // else Thread.Sleep(1000);

                              rgn.ReadGeoData(rgnreader);
                              rgn.ReadExtData(rgnreader);
                              GarminCore.OptimizedReader.StdFile_RGN.SubdivData?[] rgnsubdivdata = rgn.GetSubdivs(subdividxlst);

                              if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                                 Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": D " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                                 return;
                              } // else Thread.Sleep(1000);

                              GarminCore.OptimizedReader.StdFile_LBL lbl = new GarminCore.OptimizedReader.StdFile_LBL();
                              BinaryReaderWriter? lblreader = getBinaryReaderWriter4PseudoFile(img, mapnumber, ".LBL", true);
                              // lblreader muss ex. bis ALLE Daten eigelesen sind.
                              if (lblreader != null)
                                 lbl.ReadMinimalData(lblreader);

                              if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                                 Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": E " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                                 return;
                              } // else Thread.Sleep(1000);

                              GarminCore.OptimizedReader.StdFile_NET net = new GarminCore.OptimizedReader.StdFile_NET();
                              net.Lbl = lbl;
                              BinaryReaderWriter? netreader = getBinaryReaderWriter4PseudoFile(img, mapnumber, ".NET", true);
                              // netreader muss ex. bis ALLE Daten eigelesen sind.
                              if (netreader != null)     // in Overview-Map i.A. NICHT vorhanden
                                 net.ReadMinimalData(netreader);

                              // Raw-Daten umrechnen und die Geo-Objekte erzeugen
                              for (int i = 0; i < tresubdiv.Length && i < rgnsubdivdata.Length; i++) {
                                 if (isCancellationRequested(cancellationToken)) {
#if LOCALDEBUG
                                    Debug.WriteLine("GARMIN READ CANCEL " + mapnumber + ": F " + DateTime.Now.Subtract(start).TotalMilliseconds + "ms");
#endif
                                    return;
                                 } // else Thread.Sleep(1000);

                                 SubdivMapData? smd = sdcache?.Get(mapnumber, subdividxlst[i]);
                                 if (smd == null && rgnsubdivdata[i] != null) {
                                    smd = new SubdivMapData();
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                                    smd.ReadData(tresubdiv[i], rgnsubdivdata[i], ml.CoordBits, lbl, rgn, net);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
                                    sdcache?.Add(smd, mapnumber, subdividxlst[i]);
                                 }

                                 if (smd != null) {
                                    if (smd.Areas != null)
                                       areas.AddRange(smd.Areas);
                                    if (smd.Lines != null)
                                       lines.AddRange(smd.Lines);
                                    if (smd.Points != null)
                                       points.AddRange(smd.Points);
                                 }
                              }
                           }
                        }
                     }
                  }
               }
         } catch (Exception ex) {
            throw new Exception("Fehler beim Ermitteln der Geodaten aus " + mapnumber + ".IMG", ex);
         }
      }

      bool isCancel(ref long cancel) {
         return Interlocked.Read(ref cancel) != 0;
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               sdcache?.Clear();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
