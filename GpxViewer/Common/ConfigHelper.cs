using FSofTUtils;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using GMap.NET.CoreExt.MapProviders;
using SpecialMapCtrl;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

#if Android
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   internal static class ConfigHelper {

      static public List<GarminSymbol> ReadGarminMarkerSymbols(Config config, string progpath) {
         List<GarminSymbol> GarminMarkerSymbols = new List<GarminSymbol>();
         string[] garmingroups = config.GetGarminMarkerSymbolGroupnames();
         if (garmingroups != null)
            for (int g = 0; g < garmingroups.Length; g++) {
               string[] garminnames = config.GetGarminMarkerSymbolnames(g);
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
                                                                        out List<string> providernames) {
         providxpaths = config.ProviderIdxPaths();
         List<MapProviderDefinition> provdefs = new List<MapProviderDefinition>();
         providernames = new List<string>();
         for (int providx = 0; providx < providxpaths.Count; providx++) {
            try {
               MapProviderDefinition mpd = null;
               string provname = config.ProviderName(providxpaths[providx]);
               if (provname == GarminProvider.Instance.Name)
                  mpd = readGarminMapProviderDefinition(config, providxpaths[providx]);
               else if (provname == GarminKmzProvider.Instance.Name)
                  mpd = readKmzMapProviderDefinition(config, providxpaths[providx]);
               else if (provname == WMSProvider.Instance.Name)
                  mpd = readWmsMapProviderDefinition(config, providxpaths[providx]);
               else
                  mpd = readStdMapProviderDefinition(config, provname, providxpaths[providx]);
               // keine Exception -> alles OK -> registrieren
               providernames.Add(provname);
               provdefs.Add(mpd);
            } catch { }
         }
         return provdefs;
      }

      static MapProviderDefinition readWmsMapProviderDefinition(Config config, IList<int> providxpath) {
         return new WMSProvider.WMSMapDefinition(config.MapName(providxpath),
                                                 config.GetZoom4Display(providxpath),
                                                 config.MinZoom(providxpath),
                                                 config.MaxZoom(providxpath),
                                                 config.WmsLayers(providxpath),
                                                 config.WmsUrl(providxpath),
                                                 config.WmsSrs(providxpath),
                                                 config.WmsVersion(providxpath),
                                                 config.WmsPictFormat(providxpath),
                                                 config.WmsExtend(providxpath));
      }

      static MapProviderDefinition readGarminMapProviderDefinition(Config config, IList<int> providxpath) {
         return new GarminProvider.GarminMapDefinitionData(config.MapName(providxpath),
                                                           config.GetZoom4Display(providxpath),
                                                           config.MinZoom(providxpath),
                                                           config.MaxZoom(providxpath),
                                                           new string[] {
                                                                 PathHelper.ReplaceEnvironmentVars(config.GarminTdb(providxpath)),
                                                           },
                                                           new string[] {
                                                                 PathHelper.ReplaceEnvironmentVars(config.GarminTyp(providxpath)),
                                                           },
                                                           config.GarminTextFactor(providxpath),
                                                           config.GarminLineFactor(providxpath),
                                                           config.GarminSymbolFactor(providxpath),
                                                           config.Hillshading(providxpath),
                                                           config.HillshadingAlpha(providxpath));
      }

      static MapProviderDefinition readKmzMapProviderDefinition(Config config, IList<int> providxpath) {
         return new GarminKmzProvider.KmzMapDefinition(config.MapName(providxpath),
                                                       config.GetZoom4Display(providxpath),
                                                       config.MinZoom(providxpath),
                                                       config.MaxZoom(providxpath),
                                                       PathHelper.ReplaceEnvironmentVars(config.GarminKmzFile(providxpath)),
                                                       config.Hillshading(providxpath),
                                                       config.HillshadingAlpha(providxpath));
      }

      static MapProviderDefinition readStdMapProviderDefinition(Config config, string providername, IList<int> providxpath) {
         return new MapProviderDefinition(config.MapName(providxpath),
                                          providername,
                                          config.GetZoom4Display(providxpath),
                                          config.MinZoom(providxpath),
                                          config.MaxZoom(providxpath));
      }

      static public DemData ReadDEMDefinition(Config config) {
         DemData dem = new DemData(string.IsNullOrEmpty(config.DemPath) ?
                                             "" :
                                             IOHelper.GetFullPath(config.DemPath),
                                   config.DemCachesize,
                                   string.IsNullOrEmpty(config.DemCachePath) ?
                                             "" :
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
         VisualTrack.InEditableColor = config.InEditTrackColor;
         VisualTrack.InEditableWidth = config.InEditTrackWidth;
         VisualTrack.SelectedPartColor = config.SelectedPartTrackColor;
         VisualTrack.SelectedPartWidth = config.SelectedPartTrackWidth;
         VisualTrack.LiveDrawColor = config.LiveTrackColor;
         VisualTrack.LiveDrawWidth = config.LiveTrackWidth;

         Color[] slopecols = config.SlopeColors(out int[] slopepercent);
         VisualTrack.SetSlopeValues(slopecols, slopepercent);
      }



   }
}
