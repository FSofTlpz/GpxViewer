using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using GMap.NET.FSofTExtented.MapProviders;
using GMap.NET.MapProviders;
using SpecialMapCtrl;
using MyDrawing = System.Drawing;

#if ANDROID
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   internal static class ConfigHelper {

      static public List<GarminSymbol> ReadGarminMarkerSymbols(Config config, string progpath) {
         List<GarminSymbol> GarminMarkerSymbols = new List<GarminSymbol>();
         string[]? garmingroups = config.GetGarminMarkerSymbolGroupnames();
         if (garmingroups != null)
            for (int g = 0; g < garmingroups.Length; g++) {
               string[]? garminnames = config.GetGarminMarkerSymbolnames(g);
               if (garminnames != null)
                  for (int i = 0; i < garminnames.Length; i++) {
                     bool withoffset = config.GetGarminMarkerSymboloffset(g, i, out int offsetx, out int offsety);
                     GarminMarkerSymbols.Add(new GarminSymbol(garminnames[i],
                                                              garmingroups[g],
                                                              config.GetGarminMarkerSymboltext(g, i),
                                                              Path.Combine(progpath, config.GetGarminMarkerSymbolfile(g, i)),
                                                              withoffset ? offsetx : int.MinValue,
                                                              withoffset ? offsety : int.MinValue));
                  }
            }
         return GarminMarkerSymbols;
      }


      /// <summary>
      /// liest aus <see cref="Config"/> alle Providerdefinitionen ein
      /// </summary>
      /// <param name="config"></param>
      /// <param name="providxpaths"></param>
      /// <param name="providernames"></param>
      /// <returns></returns>
      static public List<MapProviderDefinition> ReadProviderDefinitions(Config config,
                                                                        out List<int[]> providxpaths,
                                                                        out List<string> providernames,
                                                                        DemData dem) {
         providxpaths = config.ProviderIdxPaths();
         List<MapProviderDefinition> provdefs = new List<MapProviderDefinition>();
         providernames = new List<string>();
         List<int> pathsremove = new List<int>();
         for (int providx = 0; providx < providxpaths.Count; providx++) {
            MapProviderDefinition? mpd = getMapProviderDefinition(config, providxpaths[providx], -1, out string provname, dem);
            if (mpd != null) {
               providernames.Add(provname);
               provdefs.Add(mpd);
            }
         }
         return provdefs;
      }

      /// <summary>
      /// Sind die Gruppen beider Pfade gleich?
      /// </summary>
      /// <param name="path1"></param>
      /// <param name="path2"></param>
      /// <returns></returns>
      static bool isSameProvIdxGroups(int[] path1, int[] path2) {
         if (path1.Length == path2.Length) {
            for (int i = 0; i < path1.Length - 1; i++)
               if (path1[i] != path2[i])
                  return false;
            return true;
         }
         return false;
      }

      static MapProviderDefinition? getMapProviderDefinition(Config config, IList<int> providxpath, int multiidx, out string provname, DemData dem) {
         MapProviderDefinition? mpd = null;
         provname = string.Empty;
         try {
            provname = config.ProviderName(providxpath, multiidx);
            if (provname == string.Empty)
               return null;
            if (provname == GarminProvider.Instance.Name)
               mpd = readGarminMapProviderDefinition(config, providxpath, multiidx);
            else if (provname == GarminKmzProvider.Instance.Name)
               mpd = readKmzMapProviderDefinition(config, providxpath, multiidx);
            else if (provname == WMSProvider.Instance.Name)
               mpd = readWmsMapProviderDefinition(config, providxpath, multiidx);
            else if (provname == HillshadingProvider.Instance.Name)
               mpd = readHillshadingMapProviderDefinition(config, providxpath, multiidx, dem);
            else if (provname == MultiMapProvider.Instance.Name)
               mpd = readMultiMapProviderDefinition(config, providxpath, dem);
            else
               mpd = readStdMapProviderDefinition(config, provname, providxpath, multiidx);
            // keine Exception -> alles OK
         } catch {
            // wird i.A. nicht passieren aber für den Notfall:
            mpd = new MapProviderDefinition("FEHLER: " + provname, EmptyProvider.Instance.Name);
            provname = mpd.ProviderName;
         }
         return mpd;
      }

      static MapProviderDefinition readWmsMapProviderDefinition(Config config, IList<int> providxpath, int multiidx) =>
         new WMSProvider.WMSMapDefinition(config.MapName(providxpath, multiidx),
                                          config.MinZoom(providxpath, multiidx),
                                          config.MaxZoom(providxpath, multiidx),
                                          config.WmsLayers(providxpath, multiidx),
                                          config.WmsUrl(providxpath, multiidx),
                                          config.WmsSrs(providxpath, multiidx),
                                          config.WmsVersion(providxpath, multiidx),
                                          config.WmsPictFormat(providxpath, multiidx),
                                          config.WmsExtend(providxpath, multiidx),
                                          config.Hillshading(providxpath, multiidx),
                                          config.HillshadingAlpha(providxpath, multiidx));

      static MapProviderDefinition readGarminMapProviderDefinition(Config config, IList<int> providxpath, int multiidx) =>
         new GarminProvider.GarminMapDefinition(config.MapName(providxpath, multiidx),
                                                config.MinZoom(providxpath, multiidx),
                                                config.MaxZoom(providxpath, multiidx),
                                                [Config.GetPathWithoutEnvironment(config.GarminTdb(providxpath, multiidx)),],
                                                [Config.GetPathWithoutEnvironment(config.GarminTyp(providxpath, multiidx)),],
                                                config.GarminTextFactor(providxpath, multiidx),
                                                config.GarminLineFactor(providxpath, multiidx),
                                                config.GarminSymbolFactor(providxpath, multiidx),
                                                config.Hillshading(providxpath, multiidx),
                                                config.HillshadingAlpha(providxpath, multiidx));

      static MapProviderDefinition readKmzMapProviderDefinition(Config config, IList<int> providxpath, int multiidx) =>
         new GarminKmzProvider.KmzMapDefinition(config.MapName(providxpath, multiidx),
                                                config.MinZoom(providxpath, multiidx),
                                                config.MaxZoom(providxpath, multiidx),
                                                Config.GetPathWithoutEnvironment(config.GarminKmzFile(providxpath, multiidx)),
                                                config.Hillshading(providxpath, multiidx),
                                                config.HillshadingAlpha(providxpath, multiidx));

      static MapProviderDefinition readHillshadingMapProviderDefinition(Config config, IList<int> providxpath, int multiidx, DemData dem) =>
         new HillshadingProvider.HillshadingMapDefinition(config.MapName(providxpath, multiidx),
                                                          config.MinZoom(providxpath, multiidx),
                                                          config.MaxZoom(providxpath, multiidx),
                                                          dem,
                                                          config.HillshadingAlpha(providxpath, multiidx));

      static MapProviderDefinition readMultiMapProviderDefinition(Config config, IList<int> providxpath, DemData dem) {
         List<MapProviderDefinition> provdefs = new List<MapProviderDefinition>();
         for (int providx = 0; ; providx++) {
            MapProviderDefinition? mpd = getMapProviderDefinition(config, providxpath, providx, out _, dem);
            if (mpd != null)
               provdefs.Add(mpd);
            else
               break;
         }

         return new MultiMapProvider.MultiMapDefinition(config.MapName(providxpath, -1),
                                                        config.MinZoom(providxpath, -1),
                                                        config.MaxZoom(providxpath, -1),
                                                        provdefs);
      }

      static MapProviderDefinition readStdMapProviderDefinition(Config config, string providername, IList<int> providxpath, int multiidx) =>
         new MapProviderDefinition(config.MapName(providxpath, multiidx),
                                   providername,
                                   config.MinZoom(providxpath, multiidx),
                                   config.MaxZoom(providxpath, multiidx));

      static public DemData ReadDEMDefinition(Config config) {
         DemData dem = new DemData(string.IsNullOrEmpty(config.DemPath) ?
                                             string.Empty :
                                             IOHelper.GetFullPath(config.DemPath),
                                   config.DemCachesize,
                                   string.IsNullOrEmpty(config.DemCachePath) ?
                                             string.Empty :
                                             IOHelper.GetFullPath(config.DemCachePath),
                                   config.DemMinZoom);
         dem.WithHillshade = !string.IsNullOrEmpty(config.DemPath);
         dem.SetNewHillshadingData(config.DemHillshadingAzimut,
                                   config.DemHillshadingAltitude,
                                   config.DemHillshadingScale);

         dem.GetHeight(config.StartLongitude, config.StartLatitude);  // liest die DEM-Datei ein
         return dem;
      }

      static public void ReadVisualTrackDefinitions(Config config) {
         VisualTrack.StandardColor = config.StandardTrackColor;
         VisualTrack.StandardColor2 = config.StandardTrackColor2;
         VisualTrack.StandardColor3 = config.StandardTrackColor3;
         VisualTrack.StandardColor4 = config.StandardTrackColor4;
         VisualTrack.StandardColor5 = config.StandardTrackColor5;
         VisualTrack.StandardWidth = config.StandardTrackWidth;
         VisualTrack.MarkedColor = config.MarkedTrackColor;
         VisualTrack.MarkedWidth = config.MarkedTrackWidth;
         VisualTrack.EditableColor = config.EditableTrackColor;
         VisualTrack.EditableWidth = config.EditableTrackWidth;
         VisualTrack.Marked4EditColor = config.Marked4EditColor;
         VisualTrack.Marked4EditWidth = config.Marked4EditWidth;
         VisualTrack.InEditableColor = config.InEditTrackColor;
         VisualTrack.InEditableWidth = config.InEditTrackWidth;
         VisualTrack.SelectedPartColor = config.SelectedPartTrackColor;
         VisualTrack.SelectedPartWidth = config.SelectedPartTrackWidth;
         VisualTrack.LiveDrawColor = config.LiveTrackColor;
         VisualTrack.LiveDrawWidth = config.LiveTrackWidth;

         MyDrawing.Color[] slopecols = config.SlopeColors(out int[] slopepercent);
         VisualTrack.SetSlopeValues(slopecols, slopepercent);
      }

      static public bool Save(Config? config) {
         if (config != null) {
            if (config.XmlFilename != null) {
               if (File.Exists(config.XmlFilename))
                  File.Copy(config.XmlFilename,
                            Path.GetFileNameWithoutExtension(config.XmlFilename) + "_backup" + Path.GetExtension(config.XmlFilename),
                            true);
               return config.SaveData(); // null, true, null, null, true);
            }
         }
         return false;
      }


   }
}
