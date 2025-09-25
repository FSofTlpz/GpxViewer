using GMap.NET.FSofTExtented.MapProviders;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpecialMapCtrl {
   public class FilecacheInfo {

      /// <summary>
      /// Name der Cache-Infodatei
      /// </summary>
      const string CACHEINFOFILE = "info.txt";

      public class MultiUseProviderInfos {
         /// <summary>
         /// Differenz zur Standard-ID des Providers
         /// </summary>
         public readonly int Delta;

         /// <summary>
         /// Kartenname
         /// </summary>
         public readonly string Mapname = string.Empty;

         /// <summary>
         /// Karten-Hash
         /// </summary>
         public readonly string MapHash = string.Empty;

         /// <summary>
         /// Verknüpfung (Index) zur Liste der verwendeten Provider (wird nur extern gesetzt)
         /// </summary>
         public int Using4Idx = -1;


         public MultiUseProviderInfos(int delta, string mapname, string maphash) {
            Delta = delta;
            Mapname = mapname;
            MapHash = maphash;
         }

         public override string ToString() => "Delta=" + Delta + ", Mapname=" + Mapname + ", MapHash=" + MapHash + ", Using4Idx=" + Using4Idx;
      }

      /// <summary>
      /// Info für das Cacheverzeichnis einer Karte
      /// </summary>
      public class Filecache4MapInfo {

         /// <summary>
         /// Kartenname
         /// </summary>
         public readonly string Name = string.Empty;

         /// <summary>
         /// Name des Cacheverzeichnisses
         /// </summary>
         public readonly string CacheName = string.Empty;

         /// <summary>
         /// Provider-ID
         /// </summary>
         public readonly string ProviderID = string.Empty;

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
         public readonly int TileCount;

         /// <summary>
         /// Byteanzahl (Summe der Tile-Größen)
         /// </summary>
         public readonly long Bytes;

         /// <summary>
         /// Verknüpfung (Index) zur Liste der verwendeten Provider (wird nur extern gesetzt)
         /// </summary>
         public int Using4Idx = -1;

         /// <summary>
         /// Datenbank-ID (entspricht dem Cache-Verzeichnis der Karte)
         /// </summary>
         public int DbId => Convert.ToInt32(CacheName);

         /// <summary>
         /// Providername
         /// </summary>
         public string ProviderName = string.Empty;


         /// <summary>
         /// ermittelt die Daten eines Cacheverzeichnisses (die Info-Datei muss ex.!)
         /// </summary>
         /// <param name="subdirname">Name und Pfad des Cacheverzeichnisses</param>
         /// <param name="cacheinfofile">Name der Cache-Infodatei</param>
         public Filecache4MapInfo(string subdirname, string cacheinfofile) {
            if (Directory.Exists(subdirname)) {
               DirectoryInfo dir = new DirectoryInfo(subdirname);
               string infofile = Path.Combine(dir.FullName, cacheinfofile);
               if (File.Exists(infofile)) {
                  string[] lines = File.ReadAllText(infofile, Encoding.UTF8).Split(System.Environment.NewLine);
                  CacheName = dir.Name;
                  if (lines.Length > 0)
                     Name = lines[0];
                  if (lines.Length > 1)
                     ProviderID = lines[1];

                  DirectoryInfo[] zoomdirs = dir.GetDirectories();
                  ZoomLevels = new int[zoomdirs.Length];
                  for (int i = 0; i < zoomdirs.Length; i++)
                     ZoomLevels[i] = Convert.ToInt32(zoomdirs[i].Name);

                  FileInfo[] tileFiles = dir.GetFiles("*.", SearchOption.AllDirectories);
                  TileCount = tileFiles.Length;

                  Bytes = 0;
                  foreach (var item1 in tileFiles)
                     Bytes += item1.Length;
               }
            }
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

         public override string ToString() => CacheName + ", " + ProviderID + ", " + Name + ", " + Bytes + " Bytes, " + Using4Idx;
      }

      /// <summary>
      /// Liste der Cacheinfos (je Cacheverzeichnis eine Info)
      /// <para>
      /// Wenn <see cref="Filecache4MapInfo.Using4Idx"/> nichtnegativ ist, wird der zugehörige Cache vom entsprechenden Provider genutzt.
      /// </para>
      /// </summary>
      public readonly List<Filecache4MapInfo> MapInfos;

      /// <summary>
      /// Summe aller Cachegrößen
      /// </summary>
      public readonly long Bytes;

      /// <summary>
      /// Auflistung der im Cache vorhandenen <see cref="MultiUseBaseProvider"/> mit ihren Standard-ID
      /// </summary>
      public readonly Dictionary<GMapProvider, int> StandardDbId4MultiUseProvider;

      /// <summary>
      /// Listung der im Cache vorhandenen Standard-ID mit ihren <see cref="MultiUseProviderInfos"/>
      /// <para>
      /// Wenn <see cref="MultiUseProviderInfos.Using4Idx"/> nichtnegativ ist, wird der zugehörige Cache vom entsprechenden Provider genutzt.
      /// </para>
      /// </summary>
      public readonly Dictionary<int, List<MultiUseProviderInfos>> MultiUseProviderInfos4StandardDbId;


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
         MapInfos = new List<Filecache4MapInfo>();
         MultiUseProviderInfos4StandardDbId = new Dictionary<int, List<MultiUseProviderInfos>>();
         StandardDbId4MultiUseProvider = new Dictionary<GMapProvider, int>();

         if (Directory.Exists(cachepath)) {
            DirectoryInfo di = new DirectoryInfo(cachepath);

            // für jedes vorhandene Unterverzeichnis werden die Cacheinfos ermittelt
            DirectoryInfo[] mapdirs = di.GetDirectories();  // Cacheverzeichnisse für die einzelnen Karten
            Bytes = 0;
            foreach (var mapir in mapdirs) {
               if (Filecache4MapInfo.IsValidDirName(mapir.Name)) {
                  Filecache4MapInfo filecache4Map = new Filecache4MapInfo(mapir.FullName, CACHEINFOFILE);
                  Bytes += filecache4Map.Bytes;
                  MapInfos.Add(filecache4Map);
               }
            }

            // für jeden Multiuse-Provider werden die Cacheinfos ermittelt
            (GMapProvider, string, int)[] multiprovs = [
               ( GarminProvider.Instance, GarminProvider.GarminMapDefinition.IDDELTAFILE, GarminProvider.Instance.StandardDbId ),
               ( GarminKmzProvider.Instance, GarminKmzProvider.KmzMapDefinition.IDDELTAFILE, GarminKmzProvider.Instance.StandardDbId ),
               ( WMSProvider.Instance, WMSProvider.WMSMapDefinition.IDDELTAFILE, WMSProvider.Instance.StandardDbId ),
               ( HillshadingProvider.Instance, HillshadingProvider.HillshadingMapDefinition.IDDELTAFILE, HillshadingProvider.Instance.StandardDbId ),
               ( MultiMapProvider.Instance, MultiMapProvider.MultiMapDefinition.IDDELTAFILE, MultiMapProvider.Instance.StandardDbId ),
            ];

            for (int i = 0; i < multiprovs.Length; i++) {
               string idfile = Path.Combine(cachepath, multiprovs[i].Item2);        // "iddelta.*"-Datei für diesen Provider
               if (File.Exists(idfile)) {
                  UniqueIDDelta uniqueIDDelta = new UniqueIDDelta(idfile);
                  Dictionary<string, (int, string)> data = uniqueIDDelta.AllData(); // Hash, Delta, Name

                  StandardDbId4MultiUseProvider.Add(multiprovs[i].Item1, multiprovs[i].Item3);

                  List<MultiUseProviderInfos> infos = new List<MultiUseProviderInfos>();  // für jeden Eintrag in der "iddelta.*"-Datei werden die Infos ermittelt
                  MultiUseProviderInfos4StandardDbId.Add(multiprovs[i].Item3, infos);
                  foreach (var key in data.Keys) {
                     MultiUseProviderInfos multiInfos = new MultiUseProviderInfos(data[key].Item1, data[key].Item2, key);
                     infos.Add(multiInfos);
                  }
               }
            }

            if (usedprovdefs != null) {   // Verknüpfung der ermittelten Infos mit den akt. verwendete Kartendefinitionen
               for (int i = 0; i < usedprovdefs.Count; i++) {
                  if (usedprovdefs[i].Provider is MultiUseBaseProvider) {

                     MultiUseBaseProvider mInstanceProv = (MultiUseBaseProvider)usedprovdefs[i].Provider;

                     int delta = ((MultiUseBaseProvider.MultiUseBaseProviderDefinition)usedprovdefs[i]).DbIdDelta;
                     if (delta >= 0) {
                        if (MultiUseProviderInfos4StandardDbId.ContainsKey(mInstanceProv.StandardDbId)) {
                           List<MultiUseProviderInfos> muinfolst = MultiUseProviderInfos4StandardDbId[mInstanceProv.StandardDbId];
                           for (int j = 0; j < muinfolst.Count; j++) {
                              if (muinfolst[j].Delta == delta) {
                                 muinfolst[j].Using4Idx = i;                                       // Cacheinfo ergänzen: Info gehört zum Provider mit diesem Index
                                 break;
                              }
                           }
                        }

                        string providstring = usedprovdefs[i].Provider.Id.ToString();
                        for (int j = 0; j < MapInfos.Count; j++) {
                           if (providstring == MapInfos[j].ProviderID &&
                               mInstanceProv.StandardDbId + delta == MapInfos[j].DbId) {           // Cacheinfo für verwendeten Provider ergänzen
                              MapInfos[j].Using4Idx = i;                                           // Info gehört zum Provider mit diesem Index
                              MapInfos[j].ProviderName = usedprovdefs[i].Provider.Name;
                              break;

                           }
                        }
                     }

                  } else {    // ein üblicher Provider

                     for (int j = 0; j < MapInfos.Count; j++) {
                        if (usedprovdefs[i].Provider.Id.ToString() == MapInfos[j].ProviderID) {    // Cacheinfo für verwendeten Provider ergänzen
                           MapInfos[j].Using4Idx = i;
                           MapInfos[j].ProviderName = usedprovdefs[i].Provider.Name;
                           break;
                        }
                     }

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
}
