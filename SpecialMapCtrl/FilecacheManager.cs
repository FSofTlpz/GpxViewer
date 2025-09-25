using GMap.NET;
using GMap.NET.CacheProviders;
using GMap.NET.FSofTExtented;
using GMap.NET.FSofTExtented.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SpecialMapCtrl {
   public class FilecacheManager {

      #region FilecacheInfo

      public class FilecacheInfo {

         /// <summary>
         /// Name der Cache-Infodatei
         /// </summary>
         const string CACHEINFOFILE = "info.txt";

         // für jeden Multiuse-Provider werden die Cacheinfos ermittelt
         static (MultiUseBaseProvider, string, int)[] multiUseProvs = [
            ( GarminProvider.Instance, GarminProvider.GarminMapDefinition.IDDELTAFILE, GarminProvider.Instance.StandardDbId ),
            ( GarminKmzProvider.Instance, GarminKmzProvider.KmzMapDefinition.IDDELTAFILE, GarminKmzProvider.Instance.StandardDbId ),
            ( WMSProvider.Instance, WMSProvider.WMSMapDefinition.IDDELTAFILE, WMSProvider.Instance.StandardDbId ),
            ( HillshadingProvider.Instance, HillshadingProvider.HillshadingMapDefinition.IDDELTAFILE, HillshadingProvider.Instance.StandardDbId ),
            ( MultiMapProvider.Instance, MultiMapProvider.MultiMapDefinition.IDDELTAFILE, MultiMapProvider.Instance.StandardDbId ),
         ];


         /// <summary>
         /// Info für das Cacheverzeichnis einer Karte
         /// </summary>
         public class CacheInfo {

            /// <summary>
            /// Kartenname
            /// </summary>
            public string Mapname = string.Empty;

            /// <summary>
            /// Name des Cacheverzeichnisses
            /// </summary>
            public string CacheName => DbId <= 0 ? string.Empty : DbId.ToString();

            /// <summary>
            /// Ex. das Cacheverzeichnis?
            /// </summary>
            public bool CacheExists = false;

            /// <summary>
            /// Datenbank-ID (einschließlich <see cref="DbIdDelta"/> für <see cref="MultiUseBaseProvider"/>)
            /// </summary>
            public int DbId = -1;

            /// <summary>
            /// Delta für Datenbank-ID (nur bei <see cref="MultiUseBaseProvider"/> größer 0)
            /// </summary>
            public int DbIdDelta = 0;

            /// <summary>
            /// Providername
            /// </summary>
            public string ProviderName = string.Empty;

            /// <summary>
            /// Provider-ID (GUID als string)
            /// </summary>
            public string ProviderID = string.Empty;

            /// <summary>
            /// Verknüpfung (Index) zur Liste der verwendeten Kartendefinitionen (wird nur extern gesetzt)
            /// </summary>
            public int MapProviderDefIdx = -1;

            /// <summary>
            /// Wird der Provider aktuell im Programm verwendet?
            /// </summary>
            public bool IsUsed => MapProviderDefIdx >= 0;


            /// <summary>
            /// verwendete Zoomlevel
            /// </summary>
            public readonly int[] ZoomLevels = [];

            /// <summary>
            /// Anzahl der Zoomlevel
            /// </summary>
            public int ZoomLevelsCount => ZoomLevels.Length;

            /// <summary>
            /// Anzahl der Tiles (Kartenkacheln)
            /// </summary>
            public readonly int TileCount = 0;

            /// <summary>
            /// Byteanzahl (Summe der Tile-Größen)
            /// </summary>
            public readonly long Bytes = 0;

            /// <summary>
            /// Karten-Hash (eindeutig!)
            /// </summary>
            public string MultiUseMapHash = string.Empty;




            /// <summary>
            /// ermittelt die Daten eines Cacheverzeichnisses (für <see cref="Mapname"/> und <see cref="ProviderID"/>) muss die Info-Datei ex.!)
            /// </summary>
            /// <param name="subdirname">Name und Pfad des Cacheverzeichnisses</param>
            /// <param name="cacheinfofile">Name der Cache-Infodatei</param>
            public CacheInfo(string subdirname, string cacheinfofile) {
               if (Directory.Exists(subdirname)) {
                  DirectoryInfo dir = new DirectoryInfo(subdirname);

                  DbId = Convert.ToInt32(dir.Name);
                  CacheExists = true;

                  DirectoryInfo[] zoomdirs = dir.GetDirectories();
                  ZoomLevels = new int[zoomdirs.Length];
                  for (int i = 0; i < zoomdirs.Length; i++)
                     ZoomLevels[i] = Convert.ToInt32(zoomdirs[i].Name);

                  FileInfo[] tileFiles = dir.GetFiles("*.", SearchOption.AllDirectories);
                  TileCount = tileFiles.Length;

                  Bytes = 0;
                  foreach (var item1 in tileFiles)
                     Bytes += item1.Length;

                  string infofile = Path.Combine(dir.FullName, cacheinfofile);
                  if (File.Exists(infofile)) {
                     string[] lines = File.ReadAllText(infofile, Encoding.UTF8).Split(System.Environment.NewLine);
                     if (lines.Length > 0)
                        Mapname = lines[0];
                     if (lines.Length > 1)
                        ProviderID = lines[1];
                  }
               }
            }

            public CacheInfo() {

            }


            /// <summary>
            /// Ist das ein gültiger Name für ein Cacheverzeichnis einer Karte?
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            static public bool IsValidDirName(string name) {
               foreach (char c in name)
                  if (c < '0' || '9' < c)
                     return false;
               return true;
            }

            public override string ToString() => CacheName + " " + CacheExists + ", " + DbId + ", " + Mapname + ", " + Bytes + " Bytes, " + MapProviderDefIdx;
         }

         /// <summary>
         /// Liste der Cacheinfos (je Cacheverzeichnis eine Info)
         /// <para>
         /// Wenn <see cref="CacheInfo.MapProviderDefIdx"/> nichtnegativ ist, wird der zugehörige Cache vom entsprechenden Provider genutzt.
         /// </para>
         /// </summary>
         public readonly List<CacheInfo> CacheInfos;

         /// <summary>
         /// Summe aller Cachegrößen
         /// </summary>
         public long Bytes {
            get {
               long bytes = 0;
               foreach (var item in CacheInfos)
                  bytes += item.Bytes;
               return bytes;
            }
         }


         /*

         Für die im Cache vorhandenen iddelta.*-Dateien wird der Provider mit seiner StandardID registriert.
         Für jede dieser iddelta.*-Dateien werden (für die StandardID) die enthaltenen MultiUse-Kartendefinitionen registriert.


         */

         /// <summary>
         /// ermittelt Infos zum Kartencache
         /// </summary>
         /// <param name="cachepath">Pfad zum Kartencache</param>
         /// <param name="usedprovdefs">akt. verwendete Kartendefinitionen</param>
         public FilecacheInfo(string cachepath, List<MapProviderDefinition>? usedprovdefs) {
            if (Directory.Exists(cachepath)) {
               CacheInfos = getMapInfos(cachepath, usedprovdefs);
               getMultiUseInfos(cachepath, multiUseProvs, usedprovdefs, CacheInfos);
               if (usedprovdefs != null)
                  setMapProviderDefIdx(CacheInfos, usedprovdefs);
            } else
               CacheInfos = new List<CacheInfo>();
         }

         /// <summary>
         /// liefert die Infos über die Unterverzeichnisse und die Summe der Bytes aller Kartenkacheln
         /// </summary>
         /// <param name="cachepath"></param>
         /// <returns></returns>
         List<CacheInfo> getMapInfos(string cachepath, List<MapProviderDefinition>? usedprovdefs) {
            List<CacheInfo> lst = new List<CacheInfo>();
            DirectoryInfo di = new DirectoryInfo(cachepath);

            // für jedes vorhandene Unterverzeichnis werden die Cacheinfos ermittelt
            DirectoryInfo[] mapdirs = di.GetDirectories();  // Cacheverzeichnisse für die einzelnen Karten
            Array.Sort(mapdirs,
                       delegate (DirectoryInfo di1, DirectoryInfo di2) { return di1.Name.CompareTo(di2.Name); });
            foreach (var mapdir in mapdirs) {
               if (CacheInfo.IsValidDirName(mapdir.Name)) {
                  CacheInfo filecache4Map = new CacheInfo(mapdir.FullName, CACHEINFOFILE);
                  int dbid = filecache4Map.DbId;
                  for (int i = 0; i < GMap.NET.MapProviders.GMapProviders.List.Count; i++)
                     if (GMap.NET.MapProviders.GMapProviders.List[i].DbId == dbid) {
                        filecache4Map.ProviderName = GMap.NET.MapProviders.GMapProviders.List[i].Name;
                        filecache4Map.ProviderID = GMap.NET.MapProviders.GMapProviders.List[i].Id.ToString();
                        break;
                     }
                  lst.Add(filecache4Map);
               }
            }
            return lst;
         }

         /// <summary>
         /// liefert alle Infos aus den MultiUse-Dateien (IDDELTA.*)
         /// </summary>
         /// <param name="cachepath">Pfad zum Cache</param>
         /// <param name="multiuseprovs">Daten der prinzipiell vorhandenen MultiUseProvider</param>
         /// <param name="usedprovdefs">akt. verwendete Kartendefinitionen</param>
         /// <param name="infos">Info-Liste der vorhandenen Unterverzeichnisse</param>
         void getMultiUseInfos(string cachepath,
                               (MultiUseBaseProvider, string, int)[] multiuseprovs,
                               List<MapProviderDefinition>? usedprovdefs,
                               List<CacheInfo> infos) {
            for (int i = 0; i < multiuseprovs.Length; i++) {      // i.A. für jeden bekannten MultiUseProvider
               string idfile = Path.Combine(cachepath, multiuseprovs[i].Item2);        // "iddelta.*"-Datei für diesen Provider
               if (File.Exists(idfile)) {
                  MultiUseBaseProvider provider = multiuseprovs[i].Item1;
                  int basedbid = provider.StandardDbId;                                // Standard-DbId ohne Delta für diesen Provider
                  UniqueIDDelta uniqueIDDelta = new UniqueIDDelta(idfile);
                  Dictionary<string, (int, string)> data = uniqueIDDelta.AllData();    // eindeutiger Hash, Delta, Mapname
                  foreach (var key in data.Keys) {
                     string maphash = key;
                     int deltadbid = data[key].Item1;
                     string mapname = data[key].Item2;

                     int idx = -1;
                     for (int j = 0; j < infos.Count; j++) {
                        if (infos[j].DbId == basedbid + deltadbid) {
                           idx = j;
                           infos[j].MultiUseMapHash = maphash;
                           infos[j].DbIdDelta = deltadbid;
                           if (infos[j].Mapname == string.Empty)
                              infos[j].Mapname = mapname;
                           infos[j].ProviderName = provider.Name;
                           infos[j].ProviderID = provider.Id.ToString();
                           break;
                        }
                     }
                     if (idx < 0)
                        infos.Add(new CacheInfo() {
                           Mapname = mapname,
                           DbId = basedbid + deltadbid,
                           DbIdDelta = deltadbid,
                           MultiUseMapHash = maphash,
                           ProviderName = provider.Name,
                           ProviderID = provider.Id.ToString(),
                        });
                  }
               }
            }
         }

         /// <summary>
         /// Verknüpfung def Infos mit den Kartendef.
         /// </summary>
         /// <param name="infos"></param>
         /// <param name="usedprovdefs"></param>
         void setMapProviderDefIdx(List<CacheInfo> infos, List<MapProviderDefinition> usedprovdefs) {
            for (int i = 0; i < infos.Count; i++) {
               for (int j = 0; j < usedprovdefs.Count; j++) {
                  if (usedprovdefs[j].Provider.Id.ToString() == infos[i].ProviderID) { // gleiche GUID
                     if (usedprovdefs[j] is MultiUseBaseProvider.MultiUseBaseProviderDefinition def) {
                        if (def.DbIdDelta == infos[i].DbIdDelta) {
                           infos[i].MapProviderDefIdx = j;
                           break;
                        }
                     } else {
                        infos[i].MapProviderDefIdx = j;
                        break;
                     }
                  }
               }
            }
         }


         /// <summary>
         /// schreibt die Infodatei in das Cache-Verzeichnis
         /// </summary>
         /// <param name="cachepath">Pfad des Filecaches</param>
         /// <param name="dbid">ID der Karte (Name des Unterverzeichnisses)</param>
         /// <param name="mapname">Kartenname</param>
         /// <param name="id">Provider-ID</param>
         static public void WriteCacheInfo(string cachepath, int dbid, string mapname, Guid id) {
            string filechachepath = Path.Combine(cachepath, dbid.ToString());
            if (!Directory.Exists(filechachepath))
               Directory.CreateDirectory(filechachepath);
            if (Directory.Exists(filechachepath)) {
               string infofile = Path.Combine(filechachepath, CACHEINFOFILE);
               if (!File.Exists(infofile))
                  File.AppendAllText(infofile, mapname + System.Environment.NewLine + id.ToString());
            }
         }

      }

      #endregion

      static public void WriteCacheInfo(int dbid, string mapname, Guid id) => FilecacheInfo.WriteCacheInfo(PublicCore.MapCacheLocation, dbid, mapname, id);

      /// <summary>
      /// löscht den lokalen Cache für die ID <paramref name="provid"/> (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="provid">wenn 0 oder kleiner wird der gesamte Cache gelöscht</param>
      /// <returns>Anzahl der gelöschten Dateien/Kartenteile; Fehlertext (Exception)</returns>
      static public async Task<(int, string)> ClearCache4DbIdAsync(int provid) {
         int count = 0;
         string error = string.Empty;
         await Task.Run(() => {
            if (GMaps.Instance.PrimaryCache != null &&
                GMaps.Instance.PrimaryCache is FilePureImageCache) {

               try {
                  if (provid >= 0)
                     count = FilePureImageCache.Delete(PublicCore.MapCacheLocation, provid, DateTime.MaxValue);
                  else {
                     List<string> dirs = new List<string>();
                     foreach (var dir in new DirectoryInfo(PublicCore.MapCacheLocation).GetDirectories())
                        if (FilecacheInfo.CacheInfo.IsValidDirName(dir.Name))
                           dirs.Add(dir.Name);
                     count = FilePureImageCache.Delete(PublicCore.MapCacheLocation, dirs, DateTime.MaxValue);
                  }
               } catch (Exception ex) {
                  error = ex.Message;
               }
            }
         });
         return (count, error);
      }

      /// <summary>
      /// löscht den lokalen Cache für die ID <paramref name="dbid"/> (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="info"></param>
      /// <returns>Fehler (Exception); Anzahl der gelöschten Dateien; Erfolg der Entfernung der ev. MultiUseProvider-Registrierung (false nur bei Misserfolg)</returns>
      static public async Task<(int, bool, string)> ClearCache(FilecacheInfo.CacheInfo info) => await ClearCache([info]);

      /// <summary>
      /// löscht den lokalen Cache für die ID <paramref name="dbid"/> (nur wenn er ein <see cref="FilePureImageCache"/> ist!)
      /// </summary>
      /// <param name="info"></param>
      /// <returns>Anzahl der gelöschten Dateien; Erfolg der Entfernung der ev. MultiUseProvider-Registrierung (false nur bei Misserfolg); Fehlertext</returns>
      static public async Task<(int, bool, string)> ClearCache(IList<FilecacheInfo.CacheInfo> info) {
         string error = string.Empty;
         int count = 0;
         bool result = false;
         if (info.Count == 1) {

            (count, result, error) = await clearCache1Item(info[0]);

         } else {       // alles löschen

            for (int i = 0; i < info.Count; i++) {
               (int count1, bool result1, error) = await clearCache1Item(info[i]);
               count += count1;
               result = result && result1;
            }

         }
         return (count, result, error);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="info"></param>
      /// <returns>Anzahl der gelöschten Dateien; Erfolg der Entfernung der ev. MultiUseProvider-Registrierung (false nur bei Misserfolg); Fehlertext</returns>
      static public async Task<(int, bool, string)> clearCache1Item(FilecacheInfo.CacheInfo info) {
         string error = string.Empty;
         int count = 0;
         bool multiprovresult = true;
         if (info.CacheExists &&
             info.CacheName != string.Empty &&
             info.DbId > 0) {
            // count = await ClearCache4DbIdAsync(-1);               alles löschen
            // count = await ClearCache4DbIdAsync(info.DbId);        für DbId löschen

            (count, error) = await ClearCache4DbIdAsync(info.DbId);
         }

         if (error == string.Empty &&
             0 < info.DbIdDelta &&
             !info.IsUsed) {
            try {
               multiprovresult = removeMultiUseProviderRegistration(info);
            } catch (Exception ex) {
               if (error != string.Empty)
                  error += "; ";
               error += ex.Message;
            }
         }

         return (count, multiprovresult, error);
      }

      /// <summary>
      /// Provider-Registrierung löschen (fkt. nur, wenn <see cref="FilecacheInfo.CacheInfo.DbIdDelta"/> größer 0 ist 
      /// und <see cref="FilecacheInfo.CacheInfo.IsUsed"/> false ist)
      /// </summary>
      /// <param name="info"></param>
      /// <returns>true wenn erfolgreich</returns>
      static bool removeMultiUseProviderRegistration(FilecacheInfo.CacheInfo info) {
         UniqueIDDelta? uniqueIDDelta = null;

         if (0 < info.DbIdDelta &&
             !info.IsUsed) {
            if (info.ProviderName == GarminKmzProvider.Instance.Name)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, GarminKmzProvider.KmzMapDefinition.IDDELTAFILE));
            else if (info.ProviderName == GarminProvider.Instance.Name)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, GarminProvider.GarminMapDefinition.IDDELTAFILE));
            else if (info.ProviderName == WMSProvider.Instance.Name)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, WMSProvider.WMSMapDefinition.IDDELTAFILE));
            else if (info.ProviderName == HillshadingProvider.Instance.Name)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, HillshadingProvider.HillshadingMapDefinition.IDDELTAFILE));
            else if (info.ProviderName == MultiMapProvider.Instance.Name)
               uniqueIDDelta = new UniqueIDDelta(Path.Combine(PublicCore.MapCacheLocation, MultiMapProvider.MultiMapDefinition.IDDELTAFILE));
         }

         return uniqueIDDelta != null ? uniqueIDDelta.RemoveDelta(info.DbIdDelta) : false;
      }

      /// <summary>
      /// löscht im lokalen Cache einige (wenige) Kartenteile
      /// </summary>
      /// <param name="zoom">Zoom</param>
      /// <param name="tiles">Liste der Tiles (Pseudokoordinaten)</param>
      /// <param name="provid">Provider-ID</param>
      /// <returns></returns>
      static public int ClearCache(int zoom, IList<GPoint> tiles, int provid) => FilePureImageCache.Delete(PublicCore.MapCacheLocation, provid, zoom, tiles);

   }
}
