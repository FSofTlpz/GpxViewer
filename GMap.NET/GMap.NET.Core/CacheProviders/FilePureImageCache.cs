using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GMap.NET.CacheProviders {

   /// <summary>
   /// Filesystemcache
   /// </summary>
   public class FilePureImageCache : PureImageCache {

      /*
      Aus der Id (Guid) des Providers wird intern dessen DbId (readonly int) erzeugt:
               using (var hashProvider = new SHA1CryptoServiceProvider()) 
                  DbId = Math.Abs(BitConverter.ToInt32(hashProvider.ComputeHash(Id.ToByteArray()), 0));
      Diese DbId wird als Type in den Tiles verwendet.

      Damit ergibt sich als Dateiname: _cachedir/type/zoom/X/Y

      Normalerwiese ist ein Type damit immer eineindeutig mit einem Provider (i.A. 1 Karte) verbunden.

      Bei einigen Providern, z.B. KMZ, kann es viele verschiedene Karten geben. Diese müssen jeweils eine eigene eindeutige DbId
      erhalten. Hier wird z.Z. ein unschöner Trick verwendet: Für jede Karte ist ein eigens DbIdDelta nötig, dass zur DbId 
      des Providers addiert wird. Damit sind auch diese Typen (mit sehr hoher Wahrscheinlichkeit) eindeutig.

      Aus verschiedenen Daten einer solchen Karte wird ein Hash erzeugt. Ist dieser Hash noch unbekannt wird ein neues DbIdDelta 
      erzeugt. Die Zuordnung zwischen jedem DbIdDelta und seinem Hash wird in einer Datei im Cachverzeichnis gespeichert.

      */

      string _cachedir;

      /// <summary>
      /// cache location
      /// </summary>
      public string CacheLocation {
         get => _cachedir;
         set {
            _cachedir = value;
            if (!Directory.Exists(_cachedir))
               try {
                  Directory.CreateDirectory(_cachedir);
               } catch { }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="type">Provider-ID (Unterverzeichnis)</param>
      /// <param name="pos">Pseudokoordinaten</param>
      /// <param name="zoom">Zoom (Unterverzeichnis)</param>
      /// <param name="dirname">bei false wird der Verzeichnisname für die Dateien geliefert</param>
      /// <returns></returns>
      string getFilename(int type, GPoint pos, int zoom, bool dirname = false) => getFilename(_cachedir, type, pos, zoom, dirname);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cachelocation">vollständiger Pfad zum Cache</param>
      /// <param name="type">Provider-ID (Unterverzeichnis)</param>
      /// <param name="pos">Pseudokoordinaten</param>
      /// <param name="zoom">Zoom (Unterverzeichnis)</param>
      /// <param name="dirname">Dateiname oder übergeordnetes Verzeichnis gewünscht</param>
      /// <returns></returns>
      static string getFilename(string cachelocation, int type, GPoint pos, int zoom, bool dirname = false) =>
         dirname ?
            Path.Combine(cachelocation, type.ToString(), zoom.ToString(), pos.X.ToString()) :
            Path.Combine(cachelocation, type.ToString(), zoom.ToString(), pos.X.ToString(), pos.Y.ToString());

      /// <summary>
      /// akt. den letzten Dateizugriff
      /// </summary>
      /// <param name="filename"></param>
      void updateFiledate(string filename) {
         try {
            FileInfo fi = new FileInfo(filename);
            fi.LastWriteTime = DateTime.Now;
         } catch { }
      }

      /// <summary>
      /// löscht alle Dateien im Verzeichnis <paramref name="path"/> und den Unterverzeichnissen die älter als <paramref name="date"/> sind und 
      /// leere Verzeichnisse
      /// </summary>
      /// <param name="path"></param>
      /// <param name="date"></param>
      /// <returns>Anzahl der gelöschten Dateien</returns>
      static public int Delete(string path, DateTime date) {
         int count = 0;
         try {
            foreach (string file in Directory.GetFiles(path)) {
               FileInfo fi = new FileInfo(file);
               if (fi.LastWriteTime < date) {
                  fi.Delete();
                  count++;
               }
            }
            foreach (string dir in Directory.GetDirectories(path))
               count += Delete(dir, date);

            if (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length == 0)
               Directory.Delete(path);

         } catch { }
         return count;
      }

      /// <summary>
      /// löscht alle Dateien im Verzeichnis zur Provider-ID <paramref name="provid"/> und den Unterverzeichnissen die älter als <paramref name="date"/> sind und 
      /// leere Verzeichnisse
      /// </summary>
      /// <param name="cachelocation">vollständiger Pfad zum Cache</param>
      /// <param name="provid">Provider-ID (Unterverzeichnis)</param>
      /// <param name="date">Löschung für Tiles die älter sind</param>
      /// <returns></returns>
      static public int Delete(string cachelocation, int provid, DateTime date) => Delete(Path.Combine(cachelocation, provid.ToString()), date);

      /// <summary>
      /// löscht alle Dateien in den Provider-Unterverzeichnissen <paramref name="provid"/> und den Unterverzeichnissen die älter als <paramref name="date"/> sind und 
      /// leere Verzeichnisse
      /// </summary>
      /// <param name="cachelocation">vollständiger Pfad zum Cache</param>
      /// <param name="providerdirs">Liste der Provider (Unterverzeichnisse)</param>
      /// <param name="date">Löschung für Tiles die älter sind</param>
      /// <returns></returns>
      static public int Delete(string cachelocation, IList<string> providerdirs, DateTime date) {
         int count = 0;
         foreach (string dir in providerdirs) {
            string fullpath = Path.Combine(cachelocation, dir);
            if (Directory.Exists(fullpath))
               count += Delete(fullpath, date);
         }
         return count;
      }

      /// <summary>
      /// bestimmte Tiles aus dem Cache löschen
      /// </summary>
      /// <param name="cachelocation">vollständiger Pfad zum Cache</param>
      /// <param name="provid">Provider-ID (Unterverzeichnis)</param>
      /// <param name="zoom">Zoom (Unterverzeichnis)</param>
      /// <param name="tiles">Pseudokoordinaten der Tiles</param>
      /// <returns></returns>
      static public int Delete(string cachelocation, int provid, int zoom, IList<GPoint> tiles) {
         int count = 0;
         foreach (GPoint gp in tiles) {
            string filename = getFilename(cachelocation, provid, gp, zoom, false);
            if (File.Exists(filename)) {
               File.Delete(filename);
               count++;
            }
         }
         return count;
      }

      #region PureImageCache Members

      /// <summary>
      /// schreibt die Daten des <paramref name="tile"/> in den Cache
      /// </summary>
      /// <param name="tile">Bilddaten</param>
      /// <param name="type">Provider-ID</param>
      /// <param name="pos">Position des Tiles</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns></returns>
      bool PureImageCache.PutImageToCache(byte[] tile, int type, GPoint pos, int zoom) {
         string dirname = getFilename(type, pos, zoom, true);
         if (!Directory.Exists(dirname)) {
            try {
               Directory.CreateDirectory(dirname);
            } catch {
               return false;
            }
         }
         write(getFilename(type, pos, zoom), tile);
         return true;
      }

      /// <summary>
      /// holt die Daten aus dem Cache
      /// </summary>
      /// <param name="type">Provider-ID</param>
      /// <param name="pos">Position des Tiles</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns>null wenn keine Daten ex.</returns>
      PureImage PureImageCache.GetImageFromCache(int type, GPoint pos, int zoom) {
         PureImage ret = null;
         string filename = getFilename(type, pos, zoom);
         if (File.Exists(filename)) {
            byte[] tile = read(filename);
            updateFiledate(filename);
            if (GMapProvider.TileImageProxy != null)
               ret = GMapProvider.TileImageProxy.FromArray(tile);
         }
         return ret;
      }

      int PureImageCache.DeleteOlderThan(DateTime date, int? type) {
         string startdir = _cachedir;
         if (type != null)
            startdir = Path.Combine(_cachedir, type.ToString());
         return Delete(startdir, date);
      }

      // Das Lesen und Schreiben sollte "kollisionfrei" möglich sein.

      byte[] read(string filename) {
         for (int i = 0; i < 5; i++) {
            try {
               using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                  byte[] buff = new byte[fs.Length];
                  if (fs.Read(buff, 0, buff.Length) == fs.Length)
                     return buff;
               }
            } catch {
               Thread.Sleep(50);
            }
         }
         return [];

         //return File.ReadAllBytes(filename);
      }

      bool write(string filename, byte[] data) {
         for (int i = 0; i < 5; i++) {
            try {
               using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                  fs.Write(data, 0, data.Length);
               return true;
            } catch {
               Thread.Sleep(50);
            }
         }
         return false;

         //File.WriteAllBytes(filename, data);
      }

      #endregion
   }
}
