//#define ONLY_PRINT_PREVIEW       // zum Testen der Druckerausgabe 'True'
//#define GARMINDRAWTEST
//#define SHADINGDRAWTEST

using FSofTUtils;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.WindowsForms;
using GpxViewer.Common;
using GpxViewer.ConfigEdit;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AppData = GpxViewer.Common.AppData;
using Gpx = FSofTUtils.Geography.PoorGpx;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace GpxViewer {
   public partial class FormMain : Form {

      /// <summary>
      /// Subdirectory für das Verzeichnis der ApplicationData (lokal, d.h. Environment.SpecialFolder.LocalApplicationData)
      /// </summary>
      const string PRIVATEAPPLICATIONDATAPATH = @"FSofT\GpxViewer";

      /// <summary>
      /// Name der Konfigurationsdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string CONFIGFILE = "gpxviewer.xml";

      /// <summary>
      /// Logdatei für Exceptions (im <see cref="DATAPATH"/>)
      /// </summary>
      const string ERRORLOGFILE = "error.txt";

      /// <summary>
      /// normale Logdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string LOGFILE = "log.txt";

      /// <summary>
      /// (private) Datei für die Workbench-Daten
      /// </summary>
      const string WORKBENCHGPXFILE = "persistent.gpx";


      /// <summary>
      /// für den threadübergreifenden Aufruf von Close() und <see cref="refreshProgramState"/>() nötig (keine Parameter, kein Ergebnis)
      /// </summary>
      private delegate void SafeCallDelegate4Void2Void();

      /// <summary>
      /// für den threadübergreifenden Aufruf von <see cref="setGpxLoadInfo"/>() nötig 
      /// </summary>
      private delegate void SafeCallDelegate4String2Void(string text);


      class MapMenuManager {

         public class ActivateIdxArgs {

            public readonly int ProviderIdx;

            public ActivateIdxArgs(int idx) {
               ProviderIdx = idx;
            }
         }

         public event EventHandler<ActivateIdxArgs> ActivateIdx;

         /// <summary>
         /// Programmdaten
         /// </summary>
         AppData appData;

         /// <summary>
         /// übergeordnetes Item für alle Menüitems
         /// </summary>
         ToolStripMenuItem masteritem;

         int maxidx = -1;

         /// <summary>
         /// IDX-Liste der zuletzt verwendeten Karten
         /// </summary>
         List<int> lastusedmapsidx;

         /// <summary>
         /// max. Anzahl der zuletzt verwendeten Karten
         /// </summary>
         readonly int lastusedmapsmax = 0;

         int lastusedstartidx = -1;

         /// <summary>
         /// Liste der Providerdefinitionen
         /// </summary>
         IList<MapProviderDefinition> providerdefs;

         int _actidx = -1;

         public int ActualProviderIdx {
            get => _actidx;
            set {
               if (0 <= value && value <= maxidx) {
                  insertLastUsedMaps(value);
                  ActivateIdx?.Invoke(this, new ActivateIdxArgs(value));
                  _actidx = value;

                  masteritem.Text = "Karte: [" + providerdefs[_actidx].MapName + "]";

                  if (lastusedmapsidx.Count > 0) {
                     List<string> mapnames = new List<string>();
                     for (int i = 0; i < lastusedmapsidx.Count; i++)
                        mapnames.Add(providerdefs[lastusedmapsidx[i]].MapName);
                     appData.LastUsedMapnames = mapnames;
                  }
               }
            }
         }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="config">Konfigurationsdaten</param>
         /// <param name="appData">Programmdaten</param>
         /// <param name="masteritem">übergeordnetes Menüitem</param>
         /// <param name="providerdefs">Liste der Providerdefinitionen</param>
         /// <param name="providxpaths">Liste der Indexpfade für das Providermenü</param>
         public MapMenuManager(Config config,
                               AppData appData,
                               ToolStripMenuItem masteritem,
                               IList<MapProviderDefinition> providerdefs,
                               IList<int[]> providxpaths) {
            this.appData = appData;
            this.masteritem = masteritem;
            this.providerdefs = providerdefs;
            maxidx = providerdefs.Count - 1;
            lastusedmapsidx = new List<int>();

            masteritem.DropDownItems.Clear();
            for (int provideridx = 0; provideridx < providerdefs.Count; provideridx++) {
               // ev. für übergeordnete Providergruppen noch die Menüitems erzeugen
               ToolStripMenuItem mi = masteritem;
               for (int level = 1; level < providxpaths[provideridx].Length; level++) {
                  if (level < providxpaths[provideridx].Length - 1) {   // Providergroup
                     int groupidx = providxpaths[provideridx][level];

                     ToolStripMenuItem mi4group = null;
                     int destidx = -1;
                     for (int i = 0; i < mi.DropDownItems.Count; i++) {
                        if ((mi.DropDownItems[i] as ToolStripMenuItem).Tag == null) {
                           if (++destidx == groupidx) {
                              mi4group = mi.DropDownItems[i] as ToolStripMenuItem;
                              break;
                           }
                        }
                     }

                     if (mi4group == null) {
                        mi.DropDownItems.Add(config.ProviderGroupName(providxpaths[provideridx], level + 1));
                        mi = mi.DropDownItems[mi.DropDownItems.Count - 1] as ToolStripMenuItem;
                     } else
                        mi = mi4group;

                  } else {                                              // Provideritem

                     //ToolStripItem it = mi.DropDownItems.Add(config.MapName(providxpaths[provideridx]));
                     ToolStripItem it = mi.DropDownItems.Add(providerdefs[provideridx].MapName);
                     it.Tag = provideridx;
                     it.Click += (object s, EventArgs ea) => {  // Eventhandler setzt den aktuellen Provider-IDX auf den Idx im Tag
                        ToolStripMenuItem tsmi = s as ToolStripMenuItem;
                        if (tsmi.Tag != null)
                           ActualProviderIdx = Convert.ToInt32(tsmi.Tag);
                     };

                  }
               }
            }

            lastusedmapsmax = Math.Max(0, config.LastUsedMapsCount);
            if (lastusedmapsmax > 0) {
               masteritem.DropDownItems.Add(new ToolStripSeparator());
               masteritem.DropDownItems[masteritem.DropDownItems.Count - 1].Tag = -1;

               lastusedstartidx = masteritem.DropDownItems.Count;

               List<string> mapnames = appData.LastUsedMapnames;
               if (mapnames != null)
                  for (int i = mapnames.Count - 1; i >= 0; i--) {
                     int idx = getIdx4Mapname(mapnames[i]);
                     if (0 <= idx)
                        insertLastUsedMaps(idx);
                  }
            }
         }

         void insertLastUsedMaps(int mapidx) {
            if (0 <= mapidx && mapidx <= maxidx) {
               lastusedmapsidx.Insert(0, mapidx);

               for (int i = lastusedmapsidx.Count - 1; i > 0; i--)
                  if (lastusedmapsidx[i] == mapidx)
                     lastusedmapsidx.RemoveAt(i);

               // max. Anzahl einhalten
               while (lastusedmapsidx.Count > lastusedmapsmax)
                  lastusedmapsidx.RemoveAt(lastusedmapsidx.Count - 1);

               while (lastusedmapsidx.Count > masteritem.DropDownItems.Count - lastusedstartidx) {
                  masteritem.DropDownItems.Add("");
                  masteritem.DropDownItems[masteritem.DropDownItems.Count - 1].Click += (object s, EventArgs ea) => {
                     ToolStripMenuItem mi = s as ToolStripMenuItem;
                     if (mi.Tag != null)
                        ActualProviderIdx = Convert.ToInt32(mi.Tag);
                  };
               }

               for (int i = lastusedstartidx, j = 0; i < masteritem.DropDownItems.Count; i++, j++) {
                  int idx = lastusedmapsidx[i - lastusedstartidx];
                  masteritem.DropDownItems[i].Tag = idx;
                  masteritem.DropDownItems[i].Text = providerdefs[idx].MapName;

                  // Shortcutkey setzen
                  if (0 < j && j <= 10) { // ab dem 2. Item (1. ist sinnlos, da aktuell)
                     Keys keys = Keys.D1;
                     switch (j) {
                        case 1: keys = Keys.D1; break;
                        case 2: keys = Keys.D2; break;
                        case 3: keys = Keys.D3; break;
                        case 4: keys = Keys.D4; break;
                        case 5: keys = Keys.D5; break;
                        case 6: keys = Keys.D6; break;
                        case 7: keys = Keys.D7; break;
                        case 8: keys = Keys.D8; break;
                        case 9: keys = Keys.D9; break;
                     }
                     (masteritem.DropDownItems[i] as ToolStripMenuItem).ShortcutKeys = keys | Keys.Control;
                  }
               }
            }
         }

         int getIdx4Mapname(string mapname) {
            for (int j = 0; j < providerdefs.Count; j++)
               if (providerdefs[j].MapName == mapname)
                  return j;
            return -1;
         }

      }

      AppData appData;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      DemData dem = null;

      /// <summary>
      /// Soll eine Speicherung mit den Garmin-Erweiterungen erfolgen? (notwendig z.B. für spez. Markerbilder)
      /// </summary>
      bool saveWithGarminExtensions {
         get {
            return toolStripButton_SaveWithGarminExt.Checked;
         }
      }

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols;

      /// <summary>
      /// (ev. benutzerdef.) Farben für Auswahldialog
      /// </summary>
      internal static Color[] PredefColors = new Color[] {
         Color.Black,
         Color.FromArgb(192, 0, 0),
         Color.FromArgb(192, 64, 0),
         Color.FromArgb(192, 192, 0),
         Color.FromArgb(0, 192, 0),
         Color.FromArgb(0, 192, 192),
         Color.FromArgb(0, 0, 192),
         Color.FromArgb(192, 0, 192),
         Color.Silver,
         Color.Red,
         Color.FromArgb(255, 128, 0),
         Color.Yellow,
         Color.Lime,
         Color.Aqua,
         Color.Blue,
         Color.Fuchsia,
         Color.FromArgb(224, 224, 224),
         Color.FromArgb(255, 128, 128),
         Color.FromArgb(255, 192, 128),
         Color.FromArgb(255, 255, 128),
         Color.FromArgb(128, 255, 128),
         Color.FromArgb(128, 255, 255),
         Color.FromArgb(128, 128, 255),
         Color.FromArgb(255, 128, 255),
         Color.White,
         Color.FromArgb(255, 192, 192),
         Color.FromArgb(255, 224, 192),
         Color.FromArgb(255, 255, 192),
         Color.FromArgb(192, 255, 192),
         Color.FromArgb(192, 255, 255),
         Color.FromArgb(192, 192, 255),
         Color.FromArgb(255, 192, 255),
      };

      List<int[]> providxpaths = new List<int[]>();

      long _formIsOnClosing = 0;
      /// <summary>
      /// Soll das Programm gerade beendet werden (FormClosing() wurde abgebrochen)?
      /// </summary>
      bool formIsOnClosing {
         get => Interlocked.Read(ref _formIsOnClosing) != 0;
         set => Interlocked.Exchange(ref _formIsOnClosing, value ? 1 : 0);
      }

      /// <summary>
      /// Liste der akt. hervorgehobenen <see cref="Track"/>
      /// </summary>
      readonly List<Track> highlightedTrackSegments = new List<Track>();

      /// <summary>
      /// diverse Cursoren für die Karte
      /// </summary>
      Cursors4Map cursors4Map;

      /// <summary>
      /// Konfigurationsdaten
      /// </summary>
      Config config;

      /// <summary>
      /// Programmname (aus der Assemblyinfo)
      /// </summary>
      string Progname;

      /// <summary>
      /// Programmversion (aus der Assemblyinfo)
      /// </summary>
      string Progversion;

      string lastSaveFilename = "";

      /// <summary>
      /// Ist das Program in irgendeinem Editier-Status?
      /// </summary>
      bool progIsInEditState => programState != ProgState.Viewer;

      FormSplashScreen formSplashScreen = null;

      MapMenuManager mapMenuManager;

#if DEBUG
      DateTime dtLoadTime = DateTime.Now;
#endif

      GpxWorkbench gpxWorkbench;

      /// <summary>
      /// wenn Tiles geladen werden 1, sonst 0 (threadsichere Abfrage!)
      /// </summary>
      long tileLoadIsRunning = 0;


      public FormMain() {
         try {
            InitializeComponent();

#if DEBUG
            GarminImageCreator.ImageCreator.test();
#endif

            GpxWorkbench.LoadInfoEvent += (sender, e) => appendStartInfo("  " + e.Info);

         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
            BindingContextChanged += FormMain_BindingContextChanged;  // 1. Event nach HandleCreated
         }
      }

      #region Initialisierung

      void startSplashScreen() {
         if (formSplashScreen == null) {
            formSplashScreen = new FormSplashScreen();
            Thread splashThread = new Thread(new ThreadStart(() => Application.Run(formSplashScreen)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();
         }
      }

      void endSplashScreen() {
         formSplashScreen?.End();
         formSplashScreen = null;
         Activate();    // FormMain im Vordergrund
      }

      void appendStartInfo(string txt) =>
         formSplashScreen?.AppendTextLine(txt);

      /// <summary>
      /// gesamte Init. mit Anzeige im <see cref="FormSplashScreen"/>
      /// <para>Bei einem schweren Fehler wird das Programm geschlossen.</para>
      /// </summary>
      void initAllWithInfo() {
         startSplashScreen();
         appendStartInfo(Text + " startet ...");

         try {
            initAll();

            //throw new Exception("TEST");
         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
            Close();
            return;
         } finally {
            endSplashScreen();
         }
      }

      void initAll() {
         appendStartInfo("Init ...");

         string progpath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

#if DEBUG
         appData = new AppData(PRIVATEAPPLICATIONDATAPATH, progpath);
#else
         appData = new AppData(PRIVATEAPPLICATIONDATAPATH);
#endif

         try {
            appendStartInfo("initDepTools() ...");
            // Bis auf Ausnahmen muss die gesamte Init-Prozedur fehlerfrei laufen. Sonst erfolgt ein Prog-Abbruch.
#if DEBUG
            string datapath = Path.Combine(progpath, PRIVATEAPPLICATIONDATAPATH);
#else
            string datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PRIVATEAPPLICATIONDATAPATH);
#endif

            UIHelper.ExceptionLogfile = Path.Combine(datapath, ERRORLOGFILE);
            //logfile = Path.Combine(datapath, LOGFILE);

            if (initDataPath(datapath)) {
               string currentpath = Directory.GetCurrentDirectory();
               Directory.SetCurrentDirectory(datapath); // Directory.GetCurrentDirectory() liefert z.B.: /storage/emulated/0/TrackEddi

               string configfile = Path.Combine(progpath, CONFIGFILE);
               appendStartInfo(nameof(initConfig) + "(" + configfile + ") ...");
               config = initConfig(configfile);

               appendStartInfo(nameof(initDEM) + "() ...");
               dem = initDEM(config);
               appendStartInfo("   DemPath " + config.DemPath);
               appendStartInfo("   DemCachesize " + config.DemCachesize);
               appendStartInfo("   DemCachePath " + config.DemCachePath);

               mapControl1.MapCtrl.SpecMapCacheLocation = datapath;
               appendStartInfo(nameof(initMapProvider) + "() ...");
               Directory.SetCurrentDirectory(currentpath);
               initMapProvider(mapControl1.MapCtrl, config);
               Directory.SetCurrentDirectory(datapath);

               appendStartInfo(nameof(initAndStartMap) + "() ...");
               initAndStartMap(mapControl1.MapCtrl, config);

               appendStartInfo(nameof(setProviderZoomPosition) + "() ...");
               int idx = config.StartProvider;
               for (int i = 0; i < mapControl1.MapCtrl.SpecMapProviderDefinitions.Count; i++) {
                  if (mapControl1.MapCtrl.SpecMapProviderDefinitions[i].MapName == appData.LastMapname) {
                     idx = i;
                     break;
                  }
               }
               setProviderZoomPosition(idx,                 // entweder config.StartProvider oder entsprechend appData.LastMapname
                                       appData.LastZoom,
                                       appData.LastLongitude,
                                       appData.LastLatitude);
               appendStartInfo(nameof(initVisualTrackData) + "() ...");
               initVisualTrackData(config);

               try {
                  appendStartInfo(nameof(initGarminMarkerSymbols) + "() ...");
                  garminMarkerSymbols = initGarminMarkerSymbols(progpath, config);
                  VisualMarker.RegisterExternSymbols(garminMarkerSymbols);
               } catch (Exception ex) {
                  UIHelper.ShowExceptionMessage(this, "Fehler beim Lesen der Garmin-Symbole", ex, null, false);
               }

               appendStartInfo(nameof(initWorkbench) + "() ...");
               gpxWorkbench = initWorkbench(config, appData, Path.Combine(datapath, WORKBENCHGPXFILE), mapControl1.MapCtrl, dem);
               appendStartInfo("   Tracks: " + gpxWorkbench.TrackCount);
               appendStartInfo("   Marker: " + gpxWorkbench.MarkerCount);

               Directory.SetCurrentDirectory(currentpath);
            }
         } catch (Exception ex) {
            UIHelper.ShowExceptionMessage(this, "Fehler", ex, null, false);  // Abbruch
            throw new Exception("Abbruch", ex);
         }

         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         //mapControl1.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;

      }

      bool initDataPath(string datapath) {
         if (!Directory.Exists(datapath))
            try {
               Directory.CreateDirectory(datapath);
            } catch {
               return false;
            }
         return true;
      }

      Config initConfig(string configfile) =>
         new Config(configfile, null);

      DemData initDEM(Config cfg) => ConfigHelper.ReadDEMDefinition(cfg);

      void initMapProvider(MapCtrl map, Config cfg) {
         List<MapProviderDefinition> provdefs = ConfigHelper.ReadProviderDefinitions(cfg, out providxpaths, out List<string> providernames);
         for (int i = 0; i < provdefs.Count; i++)
            appendStartInfo("   " + provdefs[i].MapName + " (" + provdefs[i].ProviderName + ")");
         map.SpecMapRegisterProviders(providernames, provdefs);
      }

      void initAndStartMap(MapCtrl map, Config cfg) {
         map.OnMapTileLoadStart += map_OnMapTileLoadStartEvent;
         map.SpecMapTileLoadCompleteEvent += map_MapTileLoadCompleteEvent;
         map.SpecMapTrackSearch4PolygonEvent += map_MapTrackSearch4PolygonEvent;
         map.OnMapZoomChanged += map_OnZoomChanged;
         map.OnMapExceptionThrown += (Exception ex) =>
           UIHelper.ShowExceptionMessage(this, "Fehler bei " + nameof(map.OnMapExceptionThrown), ex, null, false);

         map.SpecMapMouseEvent += map_SpecMapMouseEvent;
         map.SpecMapMarkerEvent += map_SpecMapMarkerEvent;
         map.SpecMapTrackEvent += map_SpecMapTrackEvent;
         map.SpecMapDrawOnTop += map_SpecMapDrawOnTopEvent;

         //map.ShowTileGridLines = true;                 // mit EmptyTileBorders gezeichnet
         //map.ShowCenter = true;                        // shows a little red cross on the map to show you exactly where the center is
         map.Map_EmptyMapBackgroundColor = Color.LightYellow;   // Tile (noch) ohne Daten
         map.Map_EmptyTileText = "keine Daten";            // Hinweistext für "Tile ohne Daten"
         map.Map_EmptyTileColor = Color.LightGray;        // Tile (endgültig) ohne Daten

         MapCtrl.SpecMapCacheIsActiv = !cfg.ServerOnly;
         MapCtrl.SpecMapSetProxy(cfg.WebProxyName,
                                 cfg.WebProxyPort,
                                 cfg.WebProxyUser,
                                 cfg.WebProxyPassword);

         map.Map_ClickTolerance4Tracks = (float)cfg.ClickTolerance4Tracks;

         List<MapProviderDefinition> provdefs = map.SpecMapProviderDefinitions;
         int startprovider = config.StartProvider;       // EmptyProvider.Instance, GoogleMapProvider.Instance
         if (!appData.IsCreated) {     // wurde noch nie verwendet
            appData.LastLatitude = config.StartLatitude;
            appData.LastLongitude = config.StartLongitude;
            appData.LastZoom = config.StartZoom;
            appData.IsCreated = true;
         } else {
            string mapname = appData.LastMapname;
            for (int i = 0; i < provdefs.Count; i++) {
               if (provdefs[i].MapName == mapname) {
                  startprovider = i;
                  break;
               }
            }
         }
         if (startprovider >= provdefs.Count)
            startprovider = -1;

         //map.MapServiceStart(appData.LastLongitude,
         //                    appData.LastLatitude,
         //                    IOHelper.GetFullPath(config.CacheLocation),
         //                    (int)appData.LastZoom,
         //                    GMapControl.ScaleModes.Fractional);

         map.Map_ShowTileGridLines = false; // auch bei DEBUG
         map.Map_DragButton = MouseButtons.Left;

         if (startprovider >= 0)
            setProviderZoomPosition(startprovider, appData.LastZoom, appData.LastLongitude, appData.LastLatitude);
      }

      void initVisualTrackData(Config cfg) => ConfigHelper.ReadVisualTrackDefinitions(cfg);

      List<GarminSymbol> initGarminMarkerSymbols(string datapath, Config cfg) => ConfigHelper.ReadGarminMarkerSymbols(cfg, datapath);

      GpxWorkbench initWorkbench(Config config, AppData appData, string gpxworkbenchfile, MapCtrl map, DemData dem) {
         GpxWorkbench wb = new GpxWorkbench(map,
                                            dem,
                                            gpxworkbenchfile,
                                            config.HelperLineColor,
                                            config.HelperLineWidth,
                                            config.EditableTrackColor,
                                            config.EditableTrackWidth,
                                            config.SymbolZoomfactor,
                                            appData.GpxDataChanged);
         if (map != null) {
            // Nach dem Einlesen sind alle Tracks "unsichtbar".
            List<bool> tmp = appData.VisibleStatusTrackList;
            for (int i = 0; i < tmp.Count && i < wb.TrackCount; i++)
               if (tmp[i])
                  showTrack(wb.GetTrack(i));

            tmp = appData.VisibleStatusMarkerList;
            for (int i = 0; i < tmp.Count && i < wb.MarkerCount; i++)
               if (tmp[i])
                  showMarker(wb.GetMarker(i));
         }

         wb.Gpx.ChangeIsSet += gpxWorkbench_ChangeIsSet;
         wb.Gpx.TracklistChanged += gpx_TracklistChanged;
         wb.Gpx.MarkerlistlistChanged += gpx_MarkerlistlistChanged;

         wb.MarkerShouldInsertEvent += gpxWorkbench_MarkerShouldInsertEvent;
         wb.RefreshProgramStateEvent += (sender, e) => refreshProgramState();
         wb.TrackEditShowEvent += (sender, e) => showMiniEditableTrackInfo(e.Track);

         workbenchListChanged(wb.Gpx);

         return wb;
      }

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.SpecMapProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      void setProviderZoomPosition(int mapidx, double zoom, double lon, double lat) {
         try {
            // Zoom und Pos. einstellen
            if (zoom != mapControl1.MapZoom ||
                lon != mapControl1.MapCenterLon ||
                lat != mapControl1.MapCenterLat)
               mapControl1.MapSetLocationAndZoom(zoom, lon, lat);

            if (0 <= mapidx &&
                mapidx < providxpaths.Count &&
                mapidx != mapControl1.MapActualMapIdx) {     // andere Karte anzeigen
               bool hillshade = false;
               byte hillshadealpha = 0;
               bool hillshadeisactiv = mapControl1.MapZoom >= dem.MinimalZoom;

               if (0 <= mapidx) {
                  mapControl1.MapClearWaitingTaskList();

                  MapProviderDefinition mapProviderDefinition = mapControl1.MapProviderDefinitions[mapidx];
                  if (mapProviderDefinition.ProviderName == "Garmin") {
                     mapControl1.MapCancelTileBuilds();
                     hillshadealpha = (mapProviderDefinition as GarminProvider.GarminMapDefinitionData).HillShadingAlpha;
                     hillshade = (mapProviderDefinition as GarminProvider.GarminMapDefinitionData).HillShading;
                  } else if (mapProviderDefinition.ProviderName == "GarminKMZ") {
                     mapControl1.MapCancelTileBuilds();
                     hillshadealpha = (mapProviderDefinition as GarminKmzProvider.KmzMapDefinition).HillShadingAlpha;
                     hillshade = (mapProviderDefinition as GarminKmzProvider.KmzMapDefinition).HillShading;
                  }
               }
               dem.WithHillshade = hillshade;
               dem.IsActiv = hillshadeisactiv;
               mapControl1.MapSetActivProvider(mapidx, hillshadealpha, dem);
            }
         } catch (Exception ex) {
            UIHelper.ShowExceptionMessage(null, "Fehler bei " + nameof(setProviderZoomPosition), ex, null, false);
         }
      }

      #endregion

      #region Events der GpxWorkbench

      private void gpx_MarkerlistlistChanged(object sender, GpxAllExt.MarkerlistChangedEventArgs e) => workbenchListChanged(sender as GpxAllExt);

      private void gpx_TracklistChanged(object sender, GpxAllExt.TracklistChangedEventArgs e) => workbenchListChanged(sender as GpxAllExt);

      void workbenchListChanged(GpxAllExt gpx) {
         toolStripButton_ClearEditable.Enabled =
         toolStripButton_SaveGpxFiles.Enabled =
         toolStripButton_SaveGpxFileExt.Enabled = (gpx.TrackList.Count > 0 || gpx.Waypoints.Count > 0);     // "speichern unter" ist immer aktiv, wenn min. 1 Track oder 1 Marker vorhanden ist
      }

      /// <summary>
      /// die Eigenschaft <see cref="GpxAllExt.GpxDataChanged"/> wurde gesetzt (aber nicht notwendigerweise geändert!)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void gpxWorkbench_ChangeIsSet(object sender, EventArgs e) {
         GpxAllExt gpx = sender as GpxAllExt;
         if (gpx.GpxDataChanged) // es wurde (noch etwas) verändert
            gpxWorkbench.Save();
         workbenchListChanged(gpx);
      }

      private async void gpxWorkbench_MarkerShouldInsertEvent(object sender, EditHelper.MarkerEventArgs e) {
         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;
         string[] names = await gpxWorkbench.GetNamesForGeoPointAsync(e.Marker.Longitude, e.Marker.Latitude);
         Cursor = orgCursor;

         FormMarkerEditing form = new FormMarkerEditing() {
            Marker = e.Marker,
            GarminMarkerSymbols = garminMarkerSymbols,
            Proposals = names,
         };

         if (form.ShowDialog() == DialogResult.OK) {
            if (string.IsNullOrEmpty(e.Marker.Waypoint.Name)) {
               e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
            }
            gpxWorkbench.MarkerInsertCopy(e.Marker, 0);
            showEditableMarker(0);
         }
      }

      //private void gpxWorkbench_TrackEditShowEvent(object sender, EditHelper.TrackEventArgs e) {
      //   showMiniEditableTrackInfo(e.Track);
      //}

      #endregion

      #region Events der Form

      private void FormMain_BindingContextChanged(object sender, EventArgs e) {
         Close();
      }

      private void FormMain_Load(object sender, EventArgs e) {
         Assembly a = Assembly.GetExecutingAssembly();
         Progname = ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product;
         Progversion = ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion;

         Text = Progname + " " + Progversion;

         initAllWithInfo();

         // Größe des Quadrates festgelegt
         rectLastMouseMovePosition = new Rectangle(int.MinValue, int.MinValue, config.MinimalTrackpointDistanceX, config.MinimalTrackpointDistanceY);

         map_OnZoomChanged(null, EventArgs.Empty);

         // Test: new FSofTUtils.Geography.KmlReader().Read("../../gpx/gx.kmz", out List<Color> cols);

#if GARMINDRAWTEST
         garminTest(1000, 1000, 12.36, 12.41, 51.31, 51.34, 16);
         Close();
         return;
#endif

         creatMapMenuManager();

         //mapControl1.MapZoomChangedEvent += map_OnZoomChanged;
         //mapControl1.MapMouseEvent += map_SpecMapMouseEvent;
         //mapControl1.MapMarkerEvent += map_SpecMapMarkerEvent;
         //mapControl1.MapTrackEvent += map_SpecMapTrackEvent;
         //mapControl1.MapDrawOnTopEvent += map_SpecMapDrawOnTopEvent;
         //mapControl1.MapTileLoadCompleteEvent += map_MapTileLoadCompleteEvent;
         //mapControl1.MapTrackSearch4PolygonEvent += map_MapTrackSearch4PolygonEvent;

         cursors4Map = new Cursors4Map(mapControl1.MapCursor);

         //mapControl1.MapDragButton = MouseButtons.Left;

         // Argumente der Kommandozeile jeweils als GPX-Liste einlesen
         string[] args = Environment.GetCommandLineArgs();
         for (int i = 1; i < args.Length; i++) {
            appendStartInfo("lese Datei " + args[i]);
            readOnlyTracklistControl1.AddFile(args[i]);
         }

         editableTracklistControl1.TrackOrderChangedEvent += (s, ea) => gpxWorkbench.TrackChangePositionInList(ea.OldIdx, ea.NewIdx);
         editableTracklistControl1.MarkerOrderChangedEvent += (s, ea) => gpxWorkbench.MarkerChangePositionInList(ea.OldIdx, ea.NewIdx);
         editableTracklistControl1.UpdateVisualTrackEvent += (s, ea) => mapControl1.UpdateVisualTrack(ea.Track);
         editableTracklistControl1.UpdateVisualMarkerEvent += (s, ea) => mapControl1.UpdateVisualMarker(ea.Marker);
         editableTracklistControl1.ShowTrackEvent += (s, ea) => showTrack(ea.Track, ea.Visible);
         editableTracklistControl1.ShowMarkerEvent += (s, ea) => showEditableMarker(ea.Marker, ea.Visible);
         editableTracklistControl1.ChooseTrackEvent += (s, ea) => infoAndEditTrackProps(gpxWorkbench.GetTrack(ea.Idx),
                                                                                        gpxWorkbench.Gpx,
                                                                                        gpxWorkbench.GetTrack(ea.Idx).VisualName,
                                                                                        true);
         editableTracklistControl1.ChooseMarkerEvent += (s, ea) => infoAndEditMarkerProps(gpxWorkbench.GetMarker(ea.Idx),
                                                                                          true);
         editableTracklistControl1.ShowContextmenu4TrackEvent += (s, ea) =>
                  contextMenuStripEditableTracks.Show(editableTracklistControl1,
                                                      editableTracklistControl1.PointToClient(MousePosition));
         editableTracklistControl1.ShowContextmenu4MarkerEvent += (s, ea) =>
                  contextMenuStripMarker.Show(editableTracklistControl1,
                                              editableTracklistControl1.PointToClient(MousePosition));
         editableTracklistControl1.SelectTrackEvent += (s, ea) => showMiniEditableTrackInfo(editableTracklistControl1.SelectedTrack);
         editableTracklistControl1.Gpx = gpxWorkbench?.Gpx;

         setGpxLoadInfo("");

         toolStripButton_ViewerMode_Click(null, null);

         //if (File.Exists(editablegpxbackupfile)) {
         //   saveGpxBackupIsActiv = false;
         //   EditableGpx.Load(editablegpxbackupfile, true);
         //   saveGpxBackupIsActiv = true;
         //   EditableGpx.GpxFilename = "";
         //}
      }

      private void FormMain_Shown(object sender, EventArgs e) {
         endSplashScreen();
      }

      private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
         try {
            appData.LastZoom = mapControl1.MapZoom;
            appData.LastLongitude = mapControl1.MapCenterLon;
            appData.LastLatitude = mapControl1.MapCenterLat;
            appData.LastMapname = mapControl1.MapProviderDefinitions[mapControl1.MapActualMapIdx].MapName;

            appData.GpxDataChanged = gpxWorkbench != null ? gpxWorkbench.DataChanged : false;
            appData.VisibleStatusMarkerList = gpxWorkbench.VisibleStatusMarkerList;
            appData.VisibleStatusTrackList = gpxWorkbench.VisibleStatusTrackList;

            appData.Save();
         } catch { }

         gpxWorkbench?.Save();

         if (readOnlyTracklistControl1.LoadGpxfilesIsRunning) {
            formIsOnClosing = true;
            readOnlyTracklistControl1.LoadGpxfilesCancel = true;
            e.Cancel = true;
         }

         mapControl1.MapZoomChangedEvent -= map_OnZoomChanged;
         mapControl1.MapMouseEvent -= map_SpecMapMouseEvent;
         mapControl1.MapMarkerEvent -= map_SpecMapMarkerEvent;
         mapControl1.MapTrackEvent -= map_SpecMapTrackEvent;
         mapControl1.MapDrawOnTopEvent -= map_SpecMapDrawOnTopEvent;
         mapControl1.MapTileLoadCompleteEvent -= map_MapTileLoadCompleteEvent;
         mapControl1.MapTrackSearch4PolygonEvent -= map_MapTrackSearch4PolygonEvent;
      }

      private void FormMain_KeyDown(object sender, KeyEventArgs e) {
         Debug.WriteLine(">>> KeyValue=" + e.KeyValue + ", KeyData=" + e.KeyData + ", KeyCode=" + e.KeyCode);
         switch (e.KeyData) {
            case Keys.Escape:
               switch (programState) {
                  case ProgState.Edit_SetMarker:
                  case ProgState.Set_PicturePosition:
                  case ProgState.Edit_DrawTrack:
                  case ProgState.Edit_ConcatTracks:
                  case ProgState.Edit_SplitTracks:
                     programState = ProgState.Viewer;
                     break;
               }
               break;

            case Keys.Control | Keys.O:
               toolStripButton_OpenGpxfile_Click(null, EventArgs.Empty);
               break;

            case Keys.Control | Keys.S:
               toolStripButton_GeoSearch_Click(null, EventArgs.Empty);
               break;

            case Keys.Control | Keys.Add:
            case Keys.Control | Keys.Oemplus:
               toolStripButton_ZoomIn_Click(null, EventArgs.Empty);
               break;

            case Keys.Control | Keys.Subtract:
            case Keys.Control | Keys.OemMinus:
               toolStripButton_ZoomOut_Click(null, EventArgs.Empty);
               break;

            case Keys.Control | Keys.Left:
               mapControl1.MapMoveView(-.3, 0);
               break;

            case Keys.Control | Keys.Right:
               mapControl1.MapMoveView(.3, 0);
               break;

            case Keys.Control | Keys.Up:
               mapControl1.MapMoveView(0, .3);
               break;

            case Keys.Control | Keys.Down:
               mapControl1.MapMoveView(0, -.3);
               break;

            case Keys.Control | Keys.Shift | Keys.O:
               toolStripButton_LocationForm_Click(null, EventArgs.Empty);
               break;

            case Keys.Control | Keys.Shift | Keys.K:
               toolStripButton_GoToPos_Click(null, EventArgs.Empty);
               break;

         }
      }

      private void Form_GoToEvent(object sender, FormSearch.GoToPointEventArgs e) {
         SetMapLocationAndZoom(mapControl1.MapZoom, e.Longitude, e.Latitude);
         if (!string.IsNullOrEmpty(e.Name))
            mapControl1.MapShowMarker(gpxWorkbench.MarkerInsertCopy(new Marker(new Gpx.GpxWaypoint(e.Longitude, e.Latitude) { Name = e.Name },
                                                                             Marker.MarkerType.EditableStandard,
                                                                             "")),
                                      true);
      }

      private void Form_GoToAreaEvent(object sender, FormSearch.GoToAreaEventArgs e) {
         mapControl1.MapZoomToRange(new PointD(e.Left, e.Top),
                                    new PointD(e.Right, e.Bottom));
         if (!string.IsNullOrEmpty(e.Name))
            mapControl1.MapShowMarker(gpxWorkbench.MarkerInsertCopy(new Marker(new Gpx.GpxWaypoint(e.Longitude, e.Latitude) { Name = e.Name },
                                                                             Marker.MarkerType.EditableStandard,
                                                                             "")),
                                      true);
      }

      #region Dra&Drop für Prog

      private void FormMain_DragEnter(object sender, DragEventArgs e) {
         //if (e.Data.GetDataPresent(DataFormats.FileDrop))
         //   e.Effect = DragDropEffects.All;
         //else
         //   e.Effect = DragDropEffects.None;
      }

      private void FormMain_DragDrop(object sender, DragEventArgs e) {
         //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
         //for (int i = 0; i < files.Length; i++)
         //   readOnlyTracklistControl1.AddFile(files[i]);
      }

      #endregion

      /// <summary>
      /// filtert die spez. Tastatureingaben heraus, die für das Verschieben der Karte verwendet werden
      /// </summary>
      /// <param name="msg"></param>
      /// <param name="keyData"></param>
      /// <returns></returns>
      //protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
      //   if (keyData.HasFlag(Keys.Control)) {
      //      switch (keyData) {
      //         case (Keys.Left | Keys.Control):
      //            mapControl1.MapMoveView(-.3, 0);
      //            break;

      //         case (Keys.Right | Keys.Control):
      //            mapControl1.MapMoveView(.3, 0);
      //            break;

      //         case (Keys.Up | Keys.Control):
      //            mapControl1.MapMoveView(0, .3);
      //            break;

      //         case (Keys.Down | Keys.Control):
      //            mapControl1.MapMoveView(0, -.3);
      //            break;
      //      }
      //   }
      //   return base.ProcessCmdKey(ref msg, keyData);
      //}

      private void FormMain_SelectedPoints(object sender, FormTrackInfoAndEdit.SelectedPointsEventArgs e) {
         mapControl1.MapShowSelectedParts(e.Track, null);
         mapControl1.MapShowSelectedParts(e.Track, e.PointList);
      }

      #endregion

      #region Events der Toolbar-Buttons des Programms

      private void toolStripButton_CancelMapLoading_Click(object sender, EventArgs e) {
         mapControl1.MapClearWaitingTaskList();
         mapControl1.MapCancelTileBuilds();
      }

      private void toolStripButton_OpenGpxfile_Click(object sender, EventArgs e) {
         if (openFileDialogGpx.ShowDialog() == DialogResult.OK)
            readOnlyTracklistControl1.AddFile(openFileDialogGpx.FileName);
      }

      private void toolStripButton_SaveGpxFile_Click(object sender, EventArgs e) {
         saveWorkbench();
      }

      private void toolStripButton_SaveGpxFileExt_Click(object sender, EventArgs e) {
         saveWorkbench(true);
      }

      private void toolStripButton_SaveGpxFiles_Click(object sender, EventArgs e) {
         saveWorkbench(true, true);
      }

      private void toolStripButton_CopyMap_Click(object sender, EventArgs e) {
         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;
         Image img = mapControl1.MapGetViewAsImage();
         Cursor = orgCursor;

         Clipboard.SetImage(img);
      }

      private void toolStripButton_PrintMap_Click(object sender, EventArgs e) {
         try {
            if (pageSettings == null)
               pageSettings = new PageSettings() {
                  Color = true,
                  Margins = new Margins((int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10))),
                  Landscape = true,
               };
            if (printerSettings == null)
               printerSettings = new PrinterSettings();

            Cursor orgCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            Image img = mapControl1.MapGetViewAsImage();
            Cursor = orgCursor;

            PrintDocument pdoc = new PrintDocument {
               PrinterSettings = printerSettings,
               DocumentName = "Karte"
            };
            pdoc.PrintPage += (doc, args) => documentPrintPage(args, img);

            PageSetupDialog psd = new PageSetupDialog() {
               AllowMargins = true,
               AllowOrientation = true,
               AllowPaper = true,
               AllowPrinter = true,          // fkt. seit Vista nicht mehr (obsolet by MS)
               EnableMetric = true,
               Document = pdoc,
               PrinterSettings = printerSettings,
               PageSettings = pageSettings,
            };
            if (psd.ShowDialog() == DialogResult.OK) {

               pageSettings = psd.PageSettings;
               printerSettings = psd.PrinterSettings;

               pdoc.PrinterSettings = printerSettings;
               pdoc.DefaultPageSettings = pageSettings;

               if (printDialog == null)
                  printDialog = new PrintDialog() {
                     AllowPrintToFile = false,
                     AllowCurrentPage = false,     // Option "akt. Seite"
                     AllowSelection = false,       // Option "Markierung"
                     AllowSomePages = false,       // Option "Seitenauswahl"
                     UseEXDialog = true,
                     PrinterSettings = printerSettings,
                     Document = pdoc,
                  };
               else {
                  printDialog.PrinterSettings = printerSettings;
                  printDialog.Document = pdoc;
               }

               FormPrintPreview formPrintPreview = new FormPrintPreview() {
                  Document = printDialog.Document,
               };
               if (formPrintPreview.ShowDialog() == DialogResult.OK) {
                  if (printDialog.ShowDialog() == DialogResult.OK) {    // u.a. auch mit Druckerauswahl
                     printDialog.Document.Print();
                     printerSettings = printDialog.PrinterSettings;
                  }

                  //#if ONLY_PRINT_PREVIEW
                  //               PrintPreviewDialog PrevDlg = new PrintPreviewDialog() {

                  //                  Document = printDialog.Document,
                  //                  WindowState = FormWindowState.Maximized,
                  //                  ShowInTaskbar = false,
                  //                  ShowIcon = false,
                  //               };
                  //               PrevDlg.ShowDialog(this);
                  //               PrevDlg = null;

                  //               if (printDialog.ShowDialog() == DialogResult.OK) {
                  //                  //printDialog.Document.Print();
                  //                  printerSettings = printDialog.PrinterSettings;
                  //               }
                  //#else
                  //               if (printDialog.ShowDialog() == DialogResult.OK) {    // u.a. auch mit Druckerauswahl
                  //                  printDialog.Document.Print();
                  //                  printerSettings = printDialog.PrinterSettings;
                  //               }
                  //#endif

               }
            }

         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
         }

      }

      //private void toolStripComboBoxMapSource_SelectedIndexChanged(object sender, EventArgs e) {
      //   try {
      //      int idx = (sender as ToolStripComboBox).SelectedIndex;
      //      dem.WithHillshade = config.Hillshading(idx);
      //      mapControl1.MapSetActivProvider(idx, config.HillshadingAlpha(idx), dem);
      //   } catch (Exception ex) {
      //      ShowExceptionError(ex);
      //   }
      //}

      private void toolStripButton_ReloadMap_Click(object sender, EventArgs e) {
         mapControl1.MapRefresh(true, true, false);
      }

      private void toolStripButton_ClearCache_Click(object sender, EventArgs e) {
         FormClearCache form = new FormClearCache() {
            Map = mapControl1,
            ProviderIndex = mapMenuManager.ActualProviderIdx,
         };
         form.ShowDialog();
         if (form.Clear > 0)
            mapControl1.MapRefresh(true, true, false);
      }

      private void toolStripButton_TrackZoom_Click(object sender, EventArgs e) {
         List<Track> lst = new List<Track>(readOnlyTracklistControl1.GetVisibleTracks());
         lst.AddRange(gpxWorkbench.VisibleTracks());
         zoomToTracks(lst);
      }

      private void toolStripButton_TrackSearch_Click(object sender, EventArgs e) {
         if (!mapControl1.MapSelectionAreaIsStarted) {      // Eingabe Auswahl-Rechteck starten
            mapControl1.MapStartSelectionArea();
         } else {
            toolStripButton_TrackSearch.Checked = false;
            Gpx.GpxBounds bounds = mapControl1.MapEndSelectionArea();
            if (bounds != null) {
               Cursor orgcursor = Cursor;
               Cursor = Cursors.WaitCursor;
               readOnlyTracklistControl1.ShowTracks(bounds); // Tracks im ausgewählten Bereich sichtbar machen
               Cursor = orgcursor;
            }
         }
      }

      private void toolStripButton_LocationForm_Click(object sender, EventArgs e) {
         FormMapLocation form = null;
         foreach (Form oform in OwnedForms) {
            if (oform is FormMapLocation) {
               form = oform as FormMapLocation;
               break;
            }
         }
         if (form == null) {
            form = new FormMapLocation();
            form.AppData = appData;
            AddOwnedForm(form);
            form.Show(this);
         } else {
            form.Activate();
         }
      }

      private void toolStripButton_ZoomIn_Click(object sender, EventArgs e) {
         mapControl1.MapZoom++;
      }

      private void toolStripButton_ZoomOut_Click(object sender, EventArgs e) {
         mapControl1.MapZoom--;
      }

      private void toolStripButton_GoToPos_Click(object sender, EventArgs e) {
         FormGoToPos form = new FormGoToPos() {
            Latitude = mapControl1.MapCenterLat,
            Longitude = mapControl1.MapCenterLon,
         };
         if (form.ShowDialog() == DialogResult.OK)
            mapControl1.MapSetLocationAndZoom(mapControl1.MapZoom, form.Longitude, form.Latitude);
      }

      private void toolStripButton_GeoSearch_Click(object sender, EventArgs e) {
         FormSearch form = null;
         foreach (Form oform in OwnedForms) {
            if (oform is FormSearch) {
               form = oform as FormSearch;
               break;
            }
         }

         if (form == null) {
            form = new FormSearch();
            form.GoToPointEvent += Form_GoToEvent;
            form.GoToAreaEvent += Form_GoToAreaEvent;
            AddOwnedForm(form);
            form.Show(this);
         }
      }

      private void toolStripButton_ViewerMode_Click(object sender, EventArgs e) {
         if (toolStripButton_ViewerMode.Checked) {
            toolStripButton_SetMarker.Checked =
            toolStripButton_TrackDraw.Checked = false;
            toolStripButton_TrackDrawEnd.Enabled = false;
            programState = ProgState.Viewer;
         } else
            toolStripButton_ViewerMode.Checked = true;
      }

      private void toolStripButton_SetMarker_Click(object sender, EventArgs e) {
         if (toolStripButton_SetMarker.Checked) {
            toolStripButton_ViewerMode.Checked =
            toolStripButton_TrackDraw.Checked = false;
            toolStripButton_TrackDrawEnd.Enabled = false;
            programState = ProgState.Edit_SetMarker;
         } else
            toolStripButton_SetMarker.Checked = true;
      }

      private void toolStripButton_TrackDraw_Click(object sender, EventArgs e) {
         if (toolStripButton_TrackDraw.Checked) {
            toolStripButton_SetMarker.Checked =
            toolStripButton_ViewerMode.Checked = false;
            toolStripButton_TrackDrawEnd.Enabled = true;
            programState = ProgState.Edit_DrawTrack;
         } else
            toolStripButton_TrackDraw.Checked = true;
      }

      private void toolStripButton_TrackDrawEnd_Click(object sender, EventArgs e) {
         if (programState == ProgState.Edit_DrawTrack) {
            gpxWorkbench.TrackEndDraw();
            gpxWorkbench.TrackStartEdit();
         }
      }

      private void toolStripButton_ClearEditable_Click(object sender, EventArgs e) {
         if (gpxWorkbench.TrackCount > 0 || gpxWorkbench.MarkerCount > 0) {

            StringBuilder sb = new StringBuilder();
            if (gpxWorkbench.TrackCount > 1)
               sb.AppendFormat("Sollen die {0} Tracks", gpxWorkbench.TrackCount);
            else if (gpxWorkbench.TrackCount == 1)
               sb.Append("Soll der Tracks");

            if (gpxWorkbench.MarkerCount > 1)
               sb.AppendFormat("{0}die {1} Markierungen",
                               gpxWorkbench.TrackCount > 0 ? " und " : "Sollen ",
                               gpxWorkbench.MarkerCount);
            else if (gpxWorkbench.MarkerCount == 1)
               sb.AppendFormat("{0}die Markierung",
                               gpxWorkbench.TrackCount > 0 ? " und " : "Soll ");

            sb.Append(" wirklich entfernt werden?");
            sb.AppendLine();

            if (gpxWorkbench.DataChanged) {
               sb.AppendLine("Die Daten wurden noch nicht gespeichert!");
            } else {
               sb.AppendLine("Die Daten wurden schon in der Datei '" + gpxWorkbench.InternalFilename + "' gespeichert.");
            }

            if (MyMessageBox.Show(sb.ToString(),
                                  "alle Tracks und/oder Markierungen entfernen",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               for (int i = gpxWorkbench.TrackCount - 1; i >= 0; i--) {
                  Track track = gpxWorkbench.GetTrack(i);
                  showTrack(track, false);
                  gpxWorkbench.TrackRemove(track);
               }

               for (int i = gpxWorkbench.MarkerCount - 1; i >= 0; i--) {
                  Marker marker = gpxWorkbench.GetMarker(i);
                  showMarker(marker, false);
                  gpxWorkbench.MarkerRemove(marker);
               }


               gpxWorkbench.DataChanged = false;
            }
         }
      }

      /// <summary>
      /// bei Bedarf eindeutige Namen für Marker und Tracks erzeugen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void toolStripButton_UniqueNames_Click(object sender, EventArgs e) {
         if (gpxWorkbench.SetUniqueNames4TracksAndMarkers(out List<int> markerlst, out List<int> tracklst)) {
            foreach (int idx in markerlst) {
               Marker marker = gpxWorkbench.GetMarker(idx);
               editableTracklistControl1.SetMarkerName(marker, marker.Text);
            }
            foreach (int idx in tracklst) {
               Track track = gpxWorkbench.GetTrack(idx);
               editableTracklistControl1.SetTrackName(track, track.Trackname);
            }
         }
      }

      private void toolStripButton_MiniHelp_Click(object sender, EventArgs e) {
         string text = @"Kurzhilfe:

- Die Karte wird mit der linken Maustaste verschoben.
- Mit STRG+Cursortaste kann die Karte i.A. ebenfalls verschoben werden
  (falls der Schieberegler für den Zoom nicht den Focus hat).
- Mit dem Scrollrad der Maus wird gezoomt.

- Linksklick auf Trackpunkt
    - Info über diesen Trackpunkt
- Rechtsklick auf Track
    - Kontextmenü des Tracks

- Linksklick auf Marker
    - Anzeige der Infos zum Marker oder Anzeige des zugehörigen Bildes
- Rechtsklick auf Marker
    - Anzeige der Liste aller Bildmarker der Umgebung oder Kontextmenü des Markers

- Linksklick + Alt
    - Anzeige der Objektinfos der Umgebung bei Garmin-Karten

im 'Marker setzen'-Modus:
- Linksklick
   - setzt die neue Position eines Markers

im 'Track zeichnen'-Modus:
- Linksklick
   - hängt einen neuen Punkt an einen Track oder
   - trennt einen Track auf oder
   - verbindet 2 Tracks
- Linksklick + Shift
   - löscht den letzten Punkt eines Tracks
- Linksklick + Strg
   - beendet das Zeichnen eines Tracks
";

         FormInfo form = null;
         foreach (Form f in OwnedForms) {
            if (f is FormInfo) {
               form = f as FormInfo;
               form.Activate();
               break;
            }
         }
         if (form == null) { // ex. noch nicht
            form = new FormInfo(text);
            AddOwnedForm(form);
            form.Width = 700;
            form.Show(this);
         }
      }

      #endregion

      #region Events vom MapControl (auch Maus-Events !)

      private void map_OnZoomChanged(object sender, EventArgs e) {
         toolStripStatusLabel_Zoom.Text = "Zoomstufe " + mapControl1.MapZoom.ToString();
         dem.IsActiv = mapControl1.MapZoom >= dem.MinimalZoom;
      }

      /// <summary>
      /// letzte Pos. bei einer Kartenverschiebung (als Quadrat, damit winzige Verschiebungen nicht das Setzen eines Punktes verhinden)
      /// </summary>
      Rectangle rectLastMouseMovePosition = new Rectangle(int.MinValue, int.MinValue, 0, 0);

      private void map_SpecMapMouseEvent(object sender, MapCtrl.MapMouseEventArgs e) {
         Track markedtrack;
         switch (e.Eventtype) {
            case MapCtrl.MapMouseEventArgs.EventType.Move:
               if (e.Button == MouseButtons.Left) {
                  rectLastMouseMovePosition.X = e.Location.X - rectLastMouseMovePosition.Width / 2;
                  rectLastMouseMovePosition.Y = e.Location.Y - rectLastMouseMovePosition.Height / 2;
               }

               double ele = Gpx.BaseElement.NOTVALID_DOUBLE;
               if (mapControl1.MapZoom >= 9) // bei kleinerem Zoom nicht ermitteln/anzeigen
                  ele = gpxWorkbench.GetHeight(e.Location);

               if (ele != Gpx.BaseElement.NOTVALID_DOUBLE)
                  toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°, {2:F0}m", e.Lon, e.Lat, gpxWorkbench.GetHeight(e.Location));
               else
                  toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°", e.Lon, e.Lat);

               if (progIsInEditState) {
                  switch (programState) {
                     case ProgState.Edit_DrawTrack:
                     case ProgState.Edit_SplitTracks:
                     case ProgState.Edit_ConcatTracks:
                        gpxWorkbench.InEditRefresh();  // löst Paint() aus
                        break;

                     case ProgState.Edit_SetMarker:
                        gpxWorkbench.InEditRefresh();  // löst Paint() aus
                        break;
                  }
               }
               break;

            case MapCtrl.MapMouseEventArgs.EventType.Leave:       // die Maus verläßt den Bereich der Karte
               foreach (Track track in highlightedTrackSegments)
                  track.IsMarked = false;
               highlightedTrackSegments.Clear();
               showToolTip4MarkedTracks();
               rectLastMouseMovePosition.X = rectLastMouseMovePosition.Y = int.MinValue;
               break;

            // Mausklicks in die Karte (DANACH erfolgt die Trackevent-Behandlung)
            case MapCtrl.MapMouseEventArgs.EventType.Click:
               if (progIsInEditState) {
                  switch (e.Button) {
                     case MouseButtons.Left:
                        switch (ModifierKeys) {
                           case Keys.None:                           // Links-Klick im Edit-Modus
                              if (rectLastMouseMovePosition.Left <= e.Location.X && e.Location.X <= rectLastMouseMovePosition.Right &&
                                  rectLastMouseMovePosition.Top <= e.Location.Y && e.Location.Y <= rectLastMouseMovePosition.Bottom) {
                                 rectLastMouseMovePosition.X = rectLastMouseMovePosition.Y = int.MinValue;
                                 rectLastMouseMovePosition.Width = config.MinimalTrackpointDistanceX;
                                 rectLastMouseMovePosition.Height = config.MinimalTrackpointDistanceY;
                                 break;
                              }

                              switch (programState) {
                                 case ProgState.Edit_SetMarker:         // beim Marker setzen
                                    gpxWorkbench.MarkerEndEdit(e.Location);
                                    e.IsHandled = true;
                                    break;

                                 case ProgState.Edit_DrawTrack:         // beim Trackzeichnen
                                    gpxWorkbench.TrackAddPoint(e.Location);
                                    e.IsHandled = true;
                                    break;

                                 case ProgState.Edit_SplitTracks:       // beim Tracksplitten
                                    gpxWorkbench.TrackEndSplit(e.Location);
                                    e.IsHandled = true;
                                    programState = ProgState.Edit_DrawTrack;
                                    break;

                                 case ProgState.Edit_ConcatTracks:      // beim Trackverbinden
                                    markedtrack = getFirstMarkedTrack(gpxWorkbench.TrackInEdit, true);
                                    if (markedtrack != null) {
                                       gpxWorkbench.TrackEndConcat(markedtrack);
                                       e.IsHandled = true;
                                    }
                                    programState = ProgState.Edit_DrawTrack;
                                    break;

                                 case ProgState.Set_PicturePosition:
                                    setFotoPosition(e.Lon, e.Lat);
                                    programState = ProgState.Viewer;
                                    break;
                              }
                              break;

                           case Keys.Shift:                          // Links-Klick + Shift im Edit-Modus
                              switch (programState) {
                                 case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                    gpxWorkbench.TrackRemovePoint();
                                    e.IsHandled = true;
                                    break;
                              }
                              break;

                           case Keys.Control:                        // Links-Klick + Ctrl im Edit-Modus
                              switch (programState) {
                                 case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                    gpxWorkbench.TrackEndDraw();
                                    e.IsHandled = true;
                                    break;
                              }
                              break;

                           case Keys.Alt:                            // Links-Klick + Alt im Edit-Modus
                              showObjectinfo(e.Location);
                              break;
                        }
                        break;

                     case MouseButtons.Right:      // Achtung: Rechtsklick mit Keys.None ist für Kontextmenü reserviert!

                        //Debug.WriteLine(">>> MouseButtons.Right " + e.Location);

                        //switch (ModifierKeys) {
                        //   case Keys.Control:                         // Rechts-Klick im Edit-Modus
                        //      switch (ProgramState) {
                        //         case ProgState.Edit_DrawTrack:
                        //            break;
                        //      }
                        //      break;
                        //}
                        break;
                  }
               } else {    // Normal-Modus
                  switch (e.Button) {
                     case MouseButtons.Left:
                        switch (ModifierKeys) {
                           case Keys.None:                          // Links-Klick im Normal-Modus


                              break;

                           case Keys.Alt:                           // Links-Klick + Alt im Normal-Modus
                              showObjectinfo(e.Location);
                              break;
                        }
                        break;
                  }
               }
               break;
         }
      }

      private void map_SpecMapMarkerEvent(object sender, MapCtrl.MarkerEventArgs e) {
         switch (e.Eventtype) {
            case SpecialMapCtrl.SpecialMapCtrl.MapMouseEventArgs.EventType.Leave:
               switch (programState) {
                  case ProgState.Edit_SetMarker:
                     gpxWorkbench.RefreshCursor();
                     break;
               }
               break;

            case SpecialMapCtrl.SpecialMapCtrl.MapMouseEventArgs.EventType.Click:
               switch (e.Button) {
                  case MouseButtons.Left:
                     if (ModifierKeys == Keys.None) {
                        switch (e.Marker.Markertype) {
                           case Marker.MarkerType.Standard:
                           case Marker.MarkerType.EditableStandard:
                              if (e.Marker.IsEditable) // Marker in der Liste markieren
                                 editableTracklistControl1.SelectMarker(e.Marker);
                              showStdMarkerInfo(e.Marker.Waypoint);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.Foto:
                              ShowPicture(e.Marker.Waypoint);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.GeoTagging:
                              break;

                           default:
                              throw new Exception("Unknown MarkerType");
                        }
                     }
                     break;

                  case MouseButtons.Right:
                     if (ModifierKeys == Keys.None)
                        switch (e.Marker.Markertype) {
                           case Marker.MarkerType.Foto:
                              showPictureMarkerList(e.Location, 1.5F);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.GeoTagging:
                              break;

                           default:
                              if (e.Marker.IsEditable)  // Marker in der Liste markieren
                                 editableTracklistControl1.SelectMarker(e.Marker);
                              // Kontextmenü für "Nicht-Foto-Marker" anzeigen, wenn 
                              contextMenuStripMarker.Tag = e.Marker;
                              mapControl1.MapShowContextMenu(contextMenuStripMarker, e.X, e.Y);
                              e.IsHandled = true;
                              break;
                        }
                     break;
               }
               break;
         }
      }

      /// <summary>
      /// einen Track in der Karte und der entsprechenden Liste markieren
      /// </summary>
      /// <param name="track"></param>
      void trackMarking(Track track) {
         if (track != null) {
            if (track.IsEditable)
               editableTracklistControl1.SelectTrack(track);
            else
               readOnlyTracklistControl1.SelectTrack(track);
         }
      }

      private void map_SpecMapTrackEvent(object sender, MapCtrl.TrackEventArgs e) {
         switch (e.Eventtype) {
            case SpecialMapCtrl.SpecialMapCtrl.MapMouseEventArgs.EventType.Click:
               switch (e.Button) {
                  case MouseButtons.Left:
                     trackMarking(e.Track);
                     if (ModifierKeys == Keys.Control) {
                        if (e.Track != null)
                           showTrackPointInfo(e.Track, e.Location);
                     }
                     e.IsHandled = true;
                     break;

                  case MouseButtons.Right:
                     if (ModifierKeys == Keys.None) {
                        trackMarking(e.Track);
                        // Kontextmenü für Route anzeigen
                        if (!e.Track.IsEditable) {
                           contextMenuStripReadOnlyTracks.Tag = e.Track;
                           mapControl1.MapShowContextMenu(contextMenuStripReadOnlyTracks, e.X, e.Y);
                        } else {
                           contextMenuStripEditableTracks.Tag = e.Track;
                           mapControl1.MapShowContextMenu(contextMenuStripEditableTracks, e.X, e.Y);
                        }
                        e.IsHandled = true;
                     }
                     break;
               }
               break;

            case SpecialMapCtrl.SpecialMapCtrl.MapMouseEventArgs.EventType.Enter:
               if (!e.Track.IsMarked &&
                   !highlightedTrackSegments.Contains(e.Track)) {
                  highlightedTrackSegments.Add(e.Track);
                  e.Track.IsMarked = true;
               }
               showToolTip4MarkedTracks();
               break;

            case SpecialMapCtrl.SpecialMapCtrl.MapMouseEventArgs.EventType.Leave:
               e.Track.IsMarked = false;
               highlightedTrackSegments.Remove(e.Track);
               showToolTip4MarkedTracks();
               break;
         }
      }

      private void map_SpecMapDrawOnTopEvent(object sender, GMapControl.DrawExtendedEventArgs e) {
         if (progIsInEditState) {  // beim Editieren
            switch (programState) {
               case ProgState.Edit_SetMarker:
                  gpxWorkbench.MarkerDrawDestinationLine(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_DrawTrack:
                  gpxWorkbench.TrackDrawDestinationLine(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_SplitTracks:
                  gpxWorkbench.TrackDrawSplitPoint(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_ConcatTracks:
                  Track trackappend = getFirstMarkedTrack(gpxWorkbench.TrackInEdit, true);
                  if (trackappend != null)
                     gpxWorkbench.TrackDrawConcatLine(e.Graphics, trackappend);
                  break;

            }
         }

         showTileLoadInfo(mapControl1.MapCtrl.SpecMapWaitingTasks());
      }

      private void map_OnMapTileLoadStartEvent(object sender, EventArgs e) {
         Interlocked.Exchange(ref tileLoadIsRunning, 1);
         showTileLoadInfo(mapControl1.MapCtrl.SpecMapWaitingTasks());
      }

      private void map_MapTileLoadCompleteEvent(object sender, MapCtrl.TileLoadEventArgs e) {
         showTileLoadInfo(0);
         if (e.Complete)
            Interlocked.Exchange(ref tileLoadIsRunning, 0);
      }

      private void map_MapTrackSearch4PolygonEvent(object sender, MouseEventArgs e) {
         toolStripButton_TrackSearch_Click(null, null);  // Ende der Eingabe simulieren
      }

      #endregion

      #region Events des ReadOnlyTracklistControl

      private void readOnlyTracklistControl1_SelectGpxEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         if (e != null)
            showMiniTrackInfo(e.Gpx.TrackList);
      }

      private void readOnlyTracklistControl1_SelectTrackEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         showMiniTrackInfo(e != null ? e.Track : null);
      }

      private void readOnlyTracklistControl1_ChooseGpxEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         infoAndEditTrackProps(e.Track,
                               e.Gpx,
                               e.Name,
                               false);
      }

      private void readOnlyTracklistControl1_ChooseTrackEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         infoAndEditTrackProps(e.Track,
                               e.Gpx,
                               e.Name,
                               false);
      }

      private void readOnlyTracklistControl1_LoadinfoEvent(object sender, ReadOnlyTracklistControl.SendStringEventArgs e) {
         if (InvokeRequired) {
            //Debug.WriteLine("Invoke: setGpxLoadInfo_Threadsafe(" + text + ")");
            var d = new SafeCallDelegate4String2Void(setGpxLoadInfo);
            try {
               Invoke(d, new object[] { e.Text });
            } catch { }
         } else {
            //Debug.WriteLine("setGpxLoadInfo_Threadsafe(" + text + ")");
            setGpxLoadInfo(e.Text);
         }
      }

      private void readOnlyTracklistControl1_RefreshProgramStateEvent(object sender, EventArgs e) {
         var de = new SafeCallDelegate4Void2Void(refreshProgramState);
         Invoke(de);

         if (formIsOnClosing) {     // Close() gewünscht
            var d = new SafeCallDelegate4Void2Void(Close);
            if (!IsDisposed)
               Invoke(d);
         }
      }

      private void readOnlyTracklistControl1_ShowAllFotoMarkerEvent(object sender, ReadOnlyTracklistControl.ShowMarkerEventArgs e) {
         showAllFotoMarker4GpxObject(e.Gpx, e.On);
      }

      private void readOnlyTracklistControl1_ShowAllMarkerEvent(object sender, ReadOnlyTracklistControl.ShowMarkerEventArgs e) {
         showAllMarker4GpxObject(e.Gpx, e.On);
      }

      private void readOnlyTracklistControl1_ShowExceptionEvent(object sender, ReadOnlyTracklistControl.SendExceptionEventArgs e) {
         UIHelper.ShowExceptionError(e.Exception);
      }

      private void readOnlyTracklistControl1_ShowTrackEvent(object sender, ReadOnlyTracklistControl.ShowTrackEventArgs e) {
         showTrack(e.Track, e.On);
      }

      #endregion

      #region Events des readonly-Objekte-Kontextmenü

      bool getObjectFromTrackContextMenu(object sender, out Track track, out GpxAllExt gpx) {
         gpx = null;
         track = null;

         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);

         if (cms.SourceControl is ReadOnlyTracklistControl) {

            return readOnlyTracklistControl1.GetSelectedObject(out gpx, out track);

         } else if (//cms.SourceControl is GMap.NET.WindowsForms.GMapControl &&  // -> Klick in die Karte
                    cms.Tag != null &&
                    cms.Tag is Track) {

            track = cms.Tag as Track;
            return true;

         }

         return false;
      }

      private void contextMenuStripReadOnlyTracks_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {

            // Beim Bearbeiten eines Tracks sollte dieser Track nicht gelöscht/bearbeitet werden können.

            Bitmap bmcolor = new Bitmap(16, 16);
            Graphics gr = Graphics.FromImage(bmcolor);

            toolStripMenuItem_ReadOnlyTrackShow.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackShowSlope.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackZoom.Enabled = true;
            toolStripMenuItem_ReadOnlyGpxShowMarker.Enabled = true;
            toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackInfo.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackExtInfo.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackColor.Enabled = true;
            toolStripMenuItem_ReadOnlyTrackClone.Enabled = true;
            numericUpDownMenuItem_ReadOnlyLineThickness.Enabled = true;
            toolStripMenuItem_ReadOnlyGpxRemove.Enabled = true;

            if (gpx != null) {      // kann nur vom TreeView kommen

               if (gpx.TrackList.Count > 0 ||
                   gpx.Waypoints.Count > 0) {   // min. 1 Track

                  gr.Clear(gpx.TrackColor);
                  gr.Flush();

                  bool visible = readOnlyTracklistControl1.GpxIsVisible(gpx);
                  toolStripMenuItem_ReadOnlyTrackZoom.Enabled = visible;
                  toolStripMenuItem_ReadOnlyTrackColor.Image = bmcolor;
                  numericUpDownMenuItem_ReadOnlyLineThickness.Tag = numericUpDownMenuItem_ReadOnlyLineThickness.NumUpDown.Value = Convert.ToDecimal(gpx.TrackWidth);  // alten Wert im Tag speichern
                  toolStripMenuItem_ReadOnlyTrackShow.Checked = visible;
                  toolStripMenuItem_ReadOnlyTrackShowSlope.Enabled = true;
                  toolStripMenuItem_ReadOnlyTrackShowSlope.Checked = gpx.TrackList.Count > 0 &&
                                                                     gpx.TrackList[0].IsSlopeVisible; // sollte für alle Tracks im Container gelten

                  toolStripMenuItem_ReadOnlyGpxShowMarker.Enabled = gpx.Waypoints.Count > 0;
                  toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = gpx.Markers4StandardAreVisible;

                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Enabled = gpx.MarkerListPictures.Count > 0;
                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Checked = gpx.Markers4PicturesAreVisible;

                  if (gpx.TrackList.Count == 0) {
                     toolStripMenuItem_ReadOnlyTrackShow.Enabled =
                     toolStripMenuItem_ReadOnlyTrackShowSlope.Enabled =
                     toolStripMenuItem_ReadOnlyTrackZoom.Enabled =
                     toolStripMenuItem_ReadOnlyTrackInfo.Enabled =
                     toolStripMenuItem_ReadOnlyTrackExtInfo.Enabled =
                     toolStripMenuItem_ReadOnlyTrackColor.Enabled =
                     numericUpDownMenuItem_ReadOnlyLineThickness.Enabled = false;
                  }

               } else
                  e.Cancel = true;

            } else if (track != null) {

               if (!gpxWorkbench.IsThisTrackInWork(track)) {

                  gr.Clear(track.LineColor);
                  gr.Flush();

                  toolStripMenuItem_ReadOnlyTrackShow.Checked =
                  toolStripMenuItem_ReadOnlyTrackZoom.Enabled = track.IsVisible;
                  toolStripMenuItem_ReadOnlyTrackShowSlope.Checked = track.IsSlopeVisible;
                  toolStripMenuItem_ReadOnlyTrackColor.Image = bmcolor;
                  numericUpDownMenuItem_ReadOnlyLineThickness.Tag = numericUpDownMenuItem_ReadOnlyLineThickness.NumUpDown.Value = Convert.ToDecimal(track.LineWidth);  // alten Wert im Tag speichern

                  toolStripMenuItem_ReadOnlyGpxShowMarker.Enabled = false;
                  toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = track.GpxDataContainer.Markers4StandardAreVisible;

                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Enabled = false;
                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Checked = track.GpxDataContainer.Markers4PicturesAreVisible;

                  toolStripMenuItem_ReadOnlyGpxRemove.Enabled = false;

               } else
                  e.Cancel = true;

               //} else if (tn != null) {

               //   // Wofür???
               //   toolStripMenuItem_ReadOnlyTrackShow.Checked = tn.Checked;
               //   toolStripMenuItem_ReadOnlyTrackZoom.Enabled = tn.Checked;
               //   toolStripMenuItem_ReadOnlyTrackExtInfo.Enabled = false;
               //   toolStripMenuItem_ReadOnlyTrackClone.Enabled = false;
               //   toolStripMenuItem_ReadOnlyGpxRemove.Enabled = false;

               //   // Ev. die Daten vom 1. GPX setzen?
               //   //toolStripMenuItem_ReadOnlyTrackColor.Image = bmcolor;
               //   //numericUpDownMenuItem_ReadOnlyLineThickness.Tag = numericUpDownMenuItem_ReadOnlyLineThickness.NumUpDown.Value = Convert.ToDecimal(track.LineWidth);  // alten Wert im Tag speichern

            } else {

               e.Cancel = true;

            }
         } else
            e.Cancel = true;
      }

      private void contextMenuStripReadOnlyTracks_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
         ContextMenuStrip cms = sender as ContextMenuStrip;
         NumericUpDownMenuItem nud = getFirstNumericUpDownMenuItem(cms);
         if (nud != null &&
             nud.Tag != null) {    // Test, ob sich der Wert ev. geändert hat
            if (Convert.ToDecimal(nud.Tag) != nud.NumUpDown.Value) { // speichern der neu eingestellten Linienbreite
               if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
                  if (track != null) {
                     track.LineWidth = Convert.ToSingle(nud.NumUpDown.Value);
                  } else if (gpx != null) {
                     gpx.TrackWidth = Convert.ToSingle(nud.NumUpDown.Value);
                  } else {
                     foreach (var item in readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected()) {
                        item.TrackWidth = Convert.ToSingle(nud.NumUpDown.Value);
                     }
                  }
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackShow_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            if (track != null)
               showTrack(track, !toolStripMenuItem_ReadOnlyTrackShow.Checked);   // noch nicht geändert
            else if (gpx != null) {
               // alle Tracks der GPX-Datei
               foreach (Track t in gpx.TrackList)
                  showTrack(t, !toolStripMenuItem_ReadOnlyTrackShow.Checked);
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackShowSlope_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            if (track != null) {
               track.IsSlopeVisible = !toolStripMenuItem_ReadOnlyTrackShowSlope.Checked;   // noch nicht geändert
               if (track.IsVisible)
                  track.Refresh();
            } else if (gpx != null) {
               // alle Tracks der GPX-Datei
               foreach (Track t in gpx.TrackList) {
                  t.IsSlopeVisible = !toolStripMenuItem_ReadOnlyTrackShowSlope.Checked;
                  if (t.IsVisible)
                     t.Refresh();
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackZoom_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            if (track != null) {
               zoomToTracks(new List<Track>() { track });
            } else if (gpx != null) {
               zoomToTracks(gpx.TrackList);
               //} else if (tn != null) {
               //   ZoomToTracks(readOnlyTracklistControl1.GetAllTracksFromSubnodes(tn));
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxShowMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out GpxAllExt gpx)) {
            List<GpxAllExt> gpxlst = null;
            if (gpx != null)
               gpxlst = new List<GpxAllExt>() { gpx };
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (GpxAllExt item in gpxlst) {
                  item.Markers4StandardAreVisible = ischecked;
                  if (item.Waypoints.Count > 0)
                     showAllMarker4GpxObject(item, item.Markers4StandardAreVisible);
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxShowPictureMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out GpxAllExt gpx)) {
            List<GpxAllExt> gpxlst = null;
            if (gpx != null)
               gpxlst = new List<GpxAllExt>() { gpx };
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (GpxAllExt item in gpxlst) {
                  item.Markers4PicturesAreVisible = ischecked;
                  if (item.MarkerListPictures.Count > 0)
                     showAllFotoMarker4GpxObject(item, item.Markers4PicturesAreVisible);
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackInfo_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            List<Track> tracklst = new List<Track>();
            if (track != null)
               tracklst.Add(track);
            else if (gpx != null)
               tracklst.AddRange(gpx.TrackList);
            //else if (tn != null)
            //   tracklst.AddRange(readOnlyTracklistControl1.GetAllTracksFromSubnodes(tn));

            if (tracklst.Count > 0) {
               string msg = "";
               foreach (Track t in tracklst) {
                  if (msg.Length > 0) {
                     msg += "/////" + System.Environment.NewLine;
                  }
                  msg += t.GetSimpleStatsText();
               }
               UIHelper.ShowInfoMessage(msg,
                                        gpx != null ?
                                          gpx.GpxFilename :
                                          tracklst[0].VisualName);
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackExtInfo_Click(object sender, EventArgs e) {
         readOnlyTracklistControl1.ChooseActualSelectedObject();
      }

      private void toolStripMenuItem_ReadOnlyTracksHide_Click(object sender, EventArgs e) {
         readOnlyTracklistControl1.HideAllTracks();
      }

      private void toolStripMenuItem_ReadOnlyTrackColor_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            if (track != null ||
                gpx != null) {
               List<GpxAllExt> gpxlst = null;

               Color orgcol = Color.Black;
               if (track != null)
                  orgcol = track.LineColor;
               else if (gpx != null)
                  orgcol = gpx.TrackColor;
               else {
                  gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();
                  if (gpxlst.Count > 0)
                     orgcol = gpxlst[0].TrackColor;
               }

               if (getColor(orgcol, saveWithGarminExtensions, out Color newcol)) {
                  if (track != null) {
                     track.LineColor = newcol;
                     mapControl1.UpdateVisualTrack(track);
                  } else if (gpx != null) {
                     gpx.TrackColor = newcol;
                     foreach (var t in gpx.TrackList)
                        mapControl1.UpdateVisualTrack(t);
                  } else {
                     foreach (var g in gpxlst) {
                        g.TrackColor = newcol;
                        foreach (var t in g.TrackList)
                           mapControl1.UpdateVisualTrack(t);
                     }
                  }
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackClone_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out GpxAllExt gpx)) {
            if (gpx != null) {      // alle klonen
               for (int i = 0; i < gpx.TrackList.Count; i++)
                  cloneTrack2GpxWorkbench(gpx.TrackList[i]);
               for (int i = 0; i < gpx.MarkerList.Count; i++)
                  cloneMarker2GpxWorkbench(gpx.MarkerList[i]);
            } else
               cloneTrack2GpxWorkbench(track);
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxRemove_Click(object sender, EventArgs e) {
         readOnlyTracklistControl1.RemoveSelectedObject();
         //TreeNode tn = GetTreeNodeFromContextMenu(sender, e);
         //if (tn != null)
         //   removeGpxfileTreeNode(tn);
      }

      #endregion

      #region Events des Track-Kontextmenü (editierbare Tracks)

      /// <summary>
      /// liefert den <see cref="Track"/> vom Kontextmenü
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      Track getTrackFromContextMenuStrip(ContextMenuStrip cms) {
         return cms.Tag != null &&
                cms.Tag is Track ? cms.Tag as Track : null;
      }

      private void contextMenuStripEditableTracks_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ContextMenuStrip cms = sender as ContextMenuStrip;
         Track track = null;
         if (cms.SourceControl is EditableTracklistControl) {

            cms.Tag = track = editableTracklistControl1.SelectedTrack;

         } else { // vom Map-Control

            track = cms.Tag as Track;
            editableTracklistControl1.SelectTrack(track);
         }

         e.Cancel = true;

         if (track != null &&
             !gpxWorkbench.IsThisTrackInWork(track)) {

            toolStripMenuItem_EditableTrackDraw.Enabled = track.GpxSegment.Points.Count > 0;
            toolStripMenuItem_EditableTrackSplit.Enabled = track.GpxSegment.Points.Count > 2;
            toolStripMenuItem_EditableTrackAppend.Enabled = gpxWorkbench.TrackCount > 1;
            toolStripMenuItem_EditableTrackReverse.Enabled = track.GpxSegment.Points.Count > 1;
            toolStripMenuItem_EditableTrackClone.Enabled = track.GpxSegment.Points.Count > 1;
            toolStripMenuItem_EditableTrackDelete.Enabled = gpxWorkbench.TrackCount > 0;
            toolStripMenuItem_EditableTrackShow.Enabled = true;
            toolStripMenuItem_EditableTrackShowSlope.Enabled = true;
            toolStripMenuItem_EditableTrackZoom.Enabled = track.IsVisible;
            toolStripMenuItem_EditableTrackInfo.Enabled = true;
            toolStripMenuItem_EditableTrackExtInfo.Enabled = true;
            toolStripMenuItem_EditableTrackColor.Enabled = true;
            numericUpDownMenuItem_EditableLineThickness.Enabled = true;
            ToolStripMenuItem_EditableTrackSimplify.Enabled = true;
            ToolStripMenuItem_RemoveVisibleEditableTracks.Enabled = !gpxWorkbench.TrackIsInWork;

            toolStripMenuItem_EditableTrackShow.Checked = track.IsVisible;
            toolStripMenuItem_EditableTrackShowSlope.Checked = track.IsSlopeVisible;

            Bitmap bmcolor = new Bitmap(16, 16);
            Graphics gr = Graphics.FromImage(bmcolor);
            gr.Clear(track.LineColor);
            gr.Flush();

            toolStripMenuItem_EditableTrackColor.Image = bmcolor;

            NumericUpDownMenuItem nud = getFirstNumericUpDownMenuItem(cms);
            nud.Tag = nud.NumUpDown.Value = Convert.ToDecimal(track.LineWidth);  // alten Wert im Tag speichern

            e.Cancel = false;

         } else if (track == null &&
                    !gpxWorkbench.TrackIsInWork &&
                    editableTracklistControl1.Tracks > 0) {    // sonst gibt es nicht zu löschen

            foreach (var item in cms.Items) {
               if (item is ToolStripMenuItem)
                  (item as ToolStripMenuItem).Enabled = false;
               if (item is NumericUpDownMenuItem)
                  (item as NumericUpDownMenuItem).Enabled = false;
            }
            ToolStripMenuItem_ShowAllEditableTracks.Enabled = true;
            ToolStripMenuItem_HideAllEditableTracks.Enabled = true;
            ToolStripMenuItem_RemoveVisibleEditableTracks.Enabled = true;
            e.Cancel = false;

         }
      }

      private void contextMenuStripEditableTracks_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
         ContextMenuStrip cms = sender as ContextMenuStrip;
         NumericUpDownMenuItem nud = getFirstNumericUpDownMenuItem(cms);
         if (nud != null &&
             nud.Tag != null) {    // Test, ob sich der Wert ev. geändert hat
            Track track = getTrackFromContextMenuStrip(cms);
            if (Convert.ToDecimal(nud.Tag) != nud.NumUpDown.Value) { // speichern der neu eingestellten Linienbreite
               if (track != null) {
                  track.LineWidth = Convert.ToSingle(nud.NumUpDown.Value);
                  gpxWorkbench.DataChanged = true;
               }
            }
         }
      }

      private void toolStripMenuItem_EditableTrackDraw_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            programState = ProgState.Edit_DrawTrack;
            gpxWorkbench.TrackStartEdit(track);
         }
      }

      private void toolStripMenuItem_EditableTrackSplit_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            programState = ProgState.Edit_SplitTracks;
            gpxWorkbench.TrackStartEdit(track);
         }
      }

      private void toolStripMenuItem_EditableTrackAppend_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            programState = ProgState.Edit_ConcatTracks;
            gpxWorkbench.TrackStartEdit(track);
         }
      }

      private void toolStripMenuItem_EditableTrackReverse_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            track.ChangeDirection();
            track.Refresh();     // falls sichtbar, Anzeige akt.
            track.GpxDataContainer.GpxDataChanged = true;
         }
      }

      private void toolStripMenuItem_EditableTrackClone_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            cloneTrack2GpxWorkbench(track);
      }

      private void toolStripMenuItem_EditableTrackDelete_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            if (MyMessageBox.Show("Sollen der Track '" + track.VisualName + "' wirklich gelöscht werden?",
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               toolStripButton_TrackDraw.PerformClick();
               gpxWorkbench.TrackRemove(track);
            }
         }
      }

      private void toolStripMenuItem_EditableTrackShow_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            showTrack(track, !track.IsVisible);
      }

      private void toolStripMenuItem_EditableTrackShowSlope_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            track.IsSlopeVisible = !track.IsSlopeVisible;
      }

      private void toolStripMenuItem_EditableTrackZoom_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            zoomToTracks(new List<Track>() { track });
      }

      private void toolStripMenuItem_EditableTrackInfo_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            UIHelper.ShowInfoMessage(track.GetSimpleStatsText(), track.VisualName);
      }

      private void toolStripMenuItem_EditableTrackExtInfo_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            infoAndEditTrackProps(track,
                                  track.GpxDataContainer,
                                  track.Trackname,
                                  true);
      }

      private void toolStripMenuItem_EditableTrackColor_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            if (getColor(track.LineColor, saveWithGarminExtensions, out Color newcol))
               gpxWorkbench.SetTrackColor(track, newcol);
         }
      }

      private void ToolStripMenuItem_EditableTrackSimplify_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            simplifyTrack(track);
      }

      private void ToolStripMenuItem_ShowAllEditableTracks_Click(object sender, EventArgs e) {
         foreach (Track track in gpxWorkbench.TrackList)
            showTrack(track, true);
      }

      private void ToolStripMenuItem_HideAllEditableTracks_Click(object sender, EventArgs e) {
         foreach (Track track in gpxWorkbench.TrackList)
            showTrack(track, false);
      }

      private void ToolStripMenuItem_RemoveVisibleEditableTracks_Click(object sender, EventArgs e) {
         int visibletracks = gpxWorkbench.VisibleTracks().Count;
         if (visibletracks > 0) {
            if (MyMessageBox.Show(visibletracks > 1 ?
                                       "Sollen wirklich ALLE " + visibletracks + " angezeigten Tracks gelöscht werden?" :
                                       "Soll der angezeigte Track gelöscht werden?",
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               toolStripButton_TrackDraw.PerformClick();
               gpxWorkbench.RemoveVisibleTracks();
            }
         }
      }

      #endregion

      #region Events des Marker-Kontextmenü (readonly + editierbar)

      /// <summary>
      /// liefert den <see cref="Marker"/> und/oder den <see cref="Gpx.GpxWaypoint"/> vom Kontextmenü
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      Marker getMarkerFromContextMenuStrip(ContextMenuStrip cms) {
         return cms.Tag != null &&
                cms.Tag is Marker ? cms.Tag as Marker : null;
      }

      private void contextMenuStripMarker_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ContextMenuStrip cms = sender as ContextMenuStrip;
         Marker marker = null;

         if (cms.SourceControl is EditableTracklistControl) {

            cms.Tag = marker = editableTracklistControl1.SelectedMarker;  // Marker im Kontextmenü merken

         } else if (cms.SourceControl is GMap.NET.WindowsForms.GMapControl &&
                    cms.Tag != null &&
                    cms.Tag is Marker) {

            marker = cms.Tag as Marker;

         }

         e.Cancel = true;

         if (marker != null &&
             marker.Markertype == Marker.MarkerType.GeoTagging)   // KEIN Kontextmenü.
            return;

         if (marker != null &&
             !gpxWorkbench.MarkerIsInWork()) {  // Ein editierbarer Marker soll während eines Move-Vorgangs nicht verändert/gelöscht werden können.
                                                // damit der akt. Marker nicht per Menü bearbeitet werden kann

            ToolStripMenuItem_WaypointClone.Enabled = progIsInEditState;

            ToolStripMenuItem_WaypointEdit.Enabled = true;  // nichteditierbare Marker nur im readonly-Modus
            ToolStripMenuItem_WaypointSet.Enabled =
            ToolStripMenuItem_WaypointDelete.Enabled = marker.IsEditable;

            ToolStripMenuItem_WaypointShow.Enabled = true;
            ToolStripMenuItem_WaypointShow.Checked = marker.IsVisible;
            ToolStripMenuItem_RemoveVisibleEditableMarkers.Enabled = !gpxWorkbench.MarkerIsInWork();

            e.Cancel = false;

         } else if (marker == null &&
                    !gpxWorkbench.MarkerIsInWork() &&
                    editableTracklistControl1.Markers > 0) {    // sonst gibt es nicht zu löschen

            foreach (var item in cms.Items) {
               if (item is ToolStripMenuItem)
                  (item as ToolStripMenuItem).Enabled = false;
            }
            ToolStripMenuItem_ShowAllEditableMarkers.Enabled = true;
            ToolStripMenuItem_HideAllEditableMarkers.Enabled = true;
            ToolStripMenuItem_RemoveVisibleEditableMarkers.Enabled = true;
            e.Cancel = false;

         }
      }

      private void ToolStripMenuItem_WaypointClone_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         cloneMarker2GpxWorkbench(getMarkerFromContextMenuStrip(cms));
      }

      private void ToolStripMenuItem_WaypointShow_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null)
            showEditableMarker(marker, !marker.IsVisible);
      }

      private void ToolStripMenuItem_WaypointEdit_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null)
            infoAndEditMarkerProps(marker, marker.IsEditable);
      }

      private void ToolStripMenuItem_WaypointSet_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null && marker.IsEditable) {
            programState = ProgState.Edit_SetMarker;     // u.a. "MarkerInEdit = null"
            gpxWorkbench.MarkerStartEdit(marker);
         }
      }

      private void ToolStripMenuItem_WaypointDelete_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null &&
             marker.IsEditable &&
             MyMessageBox.Show("Sollen der Marker '" + marker.Text + "' wirklich gelöscht werden?",
                               "Achtung",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question,
                               MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            gpxWorkbench.MarkerRemove(marker);
      }

      private void ToolStripMenuItem_WaypointZoom_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null && marker.IsEditable)
            zoomToMarkers(new List<Marker>() { marker });
      }

      private void ToolStripMenuItem_ShowAllEditableMarkers_Click(object sender, EventArgs e) {
         foreach (Marker marker in gpxWorkbench.MarkerList)
            showEditableMarker(marker, true);
      }

      private void ToolStripMenuItem_HideAllEditableMarkers_Click(object sender, EventArgs e) {
         foreach (Marker marker in gpxWorkbench.MarkerList)
            showEditableMarker(marker, false);
      }

      private void ToolStripMenuItem_RemoveVisibleEditableMarkers_Click(object sender, EventArgs e) {
         int visiblemarker = gpxWorkbench.VisibleMarkers().Count;
         if (visiblemarker > 0) {
            if (MyMessageBox.Show(visiblemarker > 1 ?
                                       "Sollen wirklich ALLE " + visiblemarker + " angezeigten Marker gelöscht werden?" :
                                       "Sollen der angezeigte Marker gelöscht werden?",
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               gpxWorkbench.RemoveVisibleMarkers();
            }
         }
      }

      #endregion

      #region Hilfsfunktionen für die Kontextmenüs

      /// <summary>
      /// liefert das 1. <see cref="NumericUpDownMenuItem"/> des Kontextmenüs
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      NumericUpDownMenuItem getFirstNumericUpDownMenuItem(ContextMenuStrip cms) {
         foreach (var item in cms.Items) {   // 1. NumericUpDownMenuItem für Linienbreite
            if (item is NumericUpDownMenuItem)
               return item as NumericUpDownMenuItem;
         }
         return null;
      }

      /// <summary>
      /// es wird der ContextMenuStrip für den 'sender' bei der Auswahl eines Items oder beim Öffnen/Schließen des Menüs geliefert
      /// </summary>
      /// <param name="sender"></param>
      /// <returns></returns>
      ContextMenuStrip GetContextMenuStrip4ContextMenu(object sender) {
         if (sender is ContextMenuStrip) {

            return sender as ContextMenuStrip;

         } else if (sender is ToolStripMenuItem) {

            if ((sender as ToolStripMenuItem).Owner is ContextMenuStrip) {

               return (sender as ToolStripMenuItem).Owner as ContextMenuStrip;

            } else if ((sender as ToolStripMenuItem).Owner is ToolStripDropDownMenu) {

               ToolStripDropDownMenu tsdm = (sender as ToolStripMenuItem).Owner as ToolStripDropDownMenu;
               if (tsdm.OwnerItem is ToolStripMenuItem)
                  return GetContextMenuStrip4ContextMenu(tsdm.OwnerItem);

            }

         }
         return null;
      }

      #endregion

      void setGpxLoadInfo(string text) => toolStripStatusLabel_GpxLoad.Text = !string.IsNullOrEmpty(text) ?
                                                                                 text :
                                                                                 "";

      #region Tracks anzeigen oder verbergen

      /// <summary>
      /// fügt den Track (sichtbar) zum Overlay in der richtigen Ebene hinzu oder entfernt ihn (aber nur bei Veränderung des Sichtbarkeitsstatus)
      /// <para>Außerdem wird der Status der zugehörigen Control (Checkbox) angepasst.</para>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      void showTrack(Track track, bool visible = true) {
         if (track.IsVisible != visible) {
            mapControl1.MapShowTrack(track,
                                     visible,
                                     visible ? nextVisibleTrack(track) : null);

            // Control-Status anpassen
            if (track.IsEditable) {
               editableTracklistControl1.ShowTrack(track, visible);
               if (track.GpxDataContainer != null)
                  track.GpxDataContainer.GpxDataChanged = true;
            } else {
               mapControl1.MapShowTrack(track, visible, visible ? nextVisibleTrack(track) : null);
               readOnlyTracklistControl1.ShowTrack(track, visible);
            }
         }
      }

      /// <summary>
      /// liefert den nächsten (darüber liegenden sichtbaren) Track
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      Track nextVisibleTrack(Track track) {
         if (track.IsEditable)
            return track.GpxDataContainer?.NextVisibleTrack(track);
         else {
            List<Track> tracks = readOnlyTracklistControl1.GetAllTracks();
            for (int i = 0; i < tracks.Count; i++) {
               if (tracks[i].Equals(track)) {
                  for (int j = i - 1; j >= 0; j--) {
                     if (tracks[j].IsVisible)
                        return tracks[j];
                  }
               }
            }
            return null;
         }
      }

      #endregion

      #region Marker anzeigen oder verbergen

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      void showEditableMarker(int idx, bool visible = true) {
         if (0 <= idx && idx < gpxWorkbench.MarkerCount)
            showEditableMarker(gpxWorkbench.GetMarker(idx), visible);
      }

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="editablemarker"></param>
      /// <param name="visible"></param>
      /// <returns></returns>
      int showEditableMarker(Marker editablemarker, bool visible) {
         if (editablemarker.IsVisible == visible)
            return -1;
         mapControl1.MapShowMarker(editablemarker,
                                   visible,
                                   visible ? nextVisibleMarker(editablemarker) : null);
         editableTracklistControl1.ShowMarker(editablemarker, visible);
         GpxAllExt gpx = editablemarker.GpxDataContainer;
         if (gpx != null) {
            gpx.GpxDataChanged = true;
            return gpx.MarkerIndex(editablemarker);
         }
         return -1;
      }

      void showMarker(Marker marker, bool visible = true) {
         if (marker.IsVisible != visible)
            mapControl1.MapShowMarker(marker,
                                      visible,
                                      visible ? nextVisibleMarker(marker) : null);
      }

      /// <summary>
      /// aller Markers des <see cref="GpxAllExt"/>-Objektes anzeigen oder verbergen entsprechend <see cref="MarkersVisible"/> aus <see cref="GpxAllExt"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      void showAllMarker4GpxObject(GpxAllExt gpx, bool visible) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerList.Count; i++)
               showMarker(gpx.MarkerList[i], visible);
         }
      }

      /// <summary>
      /// aller Fotos des <see cref="GpxAllExt"/>-Objektes anzeigen oder verbergen entsprechend <see cref="PicturesVisible"/> aus <see cref="GpxAllExt"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      void showAllFotoMarker4GpxObject(GpxAllExt gpx, bool visible) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerListPictures.Count; i++)
               showMarker(gpx.MarkerListPictures[i], visible);
         }
      }

      Marker nextVisibleMarker(Marker marker) {
         if (marker.IsEditable)
            return gpxWorkbench?.Gpx.NextVisibleMarker(marker);
         else {
            List<Marker> markers = readOnlyTracklistControl1.GetAllMarkers();
            for (int i = 0; i < markers.Count; i++) {
               if (markers[i].Equals(marker)) {
                  for (int j = i - 1; j >= 0; j--) {
                     if (markers[j].IsVisible)
                        return markers[j];
                  }
               }
            }

            return null;
         }
      }

      #endregion

      #region Funktionen für Tracks

      /// <summary>
      /// zoomt auf die Tracks der Liste
      /// </summary>
      /// <param name="tracklst"></param>
      void zoomToTracks(IList<Track> tracklst) {
         if (tracklst != null &&
             tracklst.Count > 0) {
            Gpx.GpxBounds bounds = tracklst[0].Bounds;
            for (int i = 1; i < tracklst.Count; i++)
               bounds.Union(tracklst[i].Bounds);
            mapControl1.MapZoomToRange(new PointD(bounds.MinLon, bounds.MaxLat), new PointD(bounds.MaxLon, bounds.MinLat));
         }
      }

      /// <summary>
      /// zoomt auf die Marker der Liste
      /// </summary>
      /// <param name="markerlst"></param>
      void zoomToMarkers(List<Marker> markerlst) {
         if (markerlst != null &&
             markerlst.Count > 0) {
            Gpx.GpxBounds bounds = new Gpx.GpxBounds();
            foreach (var item in markerlst) {
               bounds.Union(item.Waypoint);
            }
            mapControl1.MapZoomToRange(new PointD(bounds.MinLon, bounds.MaxLat), new PointD(bounds.MaxLon, bounds.MinLat));
         }
      }

      /// <summary>
      /// liefert (rekursiv) alle akt. markierten Nodes im Treeview
      /// </summary>
      /// <param name="tnc"></param>
      /// <returns></returns>
      List<TreeNode> getMarkedNodesFromTreeView(TreeNodeCollection tnc) {
         List<TreeNode> nodelst = new List<TreeNode>();
         foreach (TreeNode tn in tnc) {
            if (tn.Nodes.Count > 0)
               nodelst.AddRange(getMarkedNodesFromTreeView(tn.Nodes));
            if (tn.Checked)
               nodelst.Add(tn);
         }
         return nodelst;
      }

      /// <summary>
      /// erzeugt eine bearbeitbare Kopie der <see cref="Track"/> und fügt sie in die Liste ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <returns></returns>
      Track cloneTrack2GpxWorkbench(Track orgtrack) {
         if (orgtrack != null) {
            Track newtrack = gpxWorkbench.TrackInsertCopy(orgtrack);
            showTrack(newtrack); // sichtbar!
            return newtrack;
         }
         return null;
      }

      /// <summary>
      /// zeigt erweiterte Infos zum Track bzw. zur GPX-Datei an
      /// <para>Der Track kann auch editiert werden, wenn <see cref="Track.IsEditable"/> true ist und der Parameter entsprechend true ist.</para>
      /// </summary>
      /// <param name="track">Track</param>
      /// <param name="gpx">GPX-Datei-(Objekt)</param>
      /// <param name="formcaption">Überschrift für das Formular</param>
      /// <param name="editable4track">true wenn der Track editiert werden kann</param>
      void infoAndEditTrackProps(Track track, GpxAllExt gpx, string formcaption, bool editable4track) {
         FormTrackInfoAndEdit form = null;
         if (editable4track &&
             track != null &&
             track.IsEditable) {       // dann modale Form, damit immer nur 1 Track im Editiermodus ist

            Track trackcopy = Track.CreateCopy(track);
            showTrack(trackcopy, false);
            trackcopy.IsOnEdit = true;
            showTrack(trackcopy);

            form = new FormTrackInfoAndEdit(trackcopy, formcaption) {
               TrackIsReadOnly = false,
            };
            form.SelectedPoints += FormMain_SelectedPoints;
            if (form.ShowDialog() == DialogResult.OK &&
                form.TrackChanged) {      // dann Originaltrack gegen Kopie austauschen

               showTrack(trackcopy, false);     // aus der Ansicht wieder entfernen

               trackcopy.LineColor = track.LineColor;
               trackcopy.LineWidth = track.LineWidth;
               trackcopy.IsOnEdit = false;

               int pos = gpxWorkbench.TrackIndex(track);
               gpxWorkbench.TrackRemove(track);                                 // Originaltrack entfernen und ...
               Track newtrack = gpxWorkbench.TrackInsertCopy(trackcopy, pos);   // ... neuen Track einfügen und ...
               showTrack(newtrack);                                           // ... anzeigen

            } else {

               showTrack(trackcopy, false);     // aus der Ansicht wieder entfernen

            }

            form.SelectedPoints -= FormMain_SelectedPoints;

            mapControl1.MapShowSelectedParts(trackcopy, null);

         } else {                   // nicht-modale Form

            // Ex. schon eine Form für dieses Objekt?
            FormTrackInfoAndEdit existingform = null;
            foreach (var oform in OwnedForms) {
               if (oform is FormTrackInfoAndEdit) {
                  if (track != null) {
                     if ((oform as FormTrackInfoAndEdit).Track.Equals(track)) {
                        existingform = oform as FormTrackInfoAndEdit;
                        break;
                     }
                  } else if (gpx != null &&
                             gpx.TrackList.Count > 0) {
                     if ((oform as FormTrackInfoAndEdit).GpxObject.Equals(gpx)) {
                        existingform = oform as FormTrackInfoAndEdit;
                        break;
                     }
                  }
               }
            }

            if (existingform != null) {
               existingform.Focus();
            } else {
               if (track != null) {
                  form = new FormTrackInfoAndEdit(track, formcaption) {
                     TrackIsReadOnly = true,
                  };
               } else if (gpx != null &&
                          gpx.TrackList.Count > 0) {
                  form = new FormTrackInfoAndEdit(gpx, formcaption) {
                     TrackIsReadOnly = true,
                  };
               }

               if (form != null) {
                  AddOwnedForm(form);
                  form.Show(this);
               }
            }

         }
      }

      /// <summary>
      /// liefert den ersten markierten Track oder null
      /// </summary>
      /// <param name="thisnot">wenn ungleich null, dann wird NICHT dieser Track geliefert</param>
      /// <param name="onlyeditable">wenn true, dann nur editierbare Tracks berücksichtigen</param>
      /// <returns></returns>
      Track getFirstMarkedTrack(Track thisnot = null, bool onlyeditable = false) {
         Track track = null;
         for (int i = 0; i < highlightedTrackSegments.Count; i++) {
            if ((thisnot == null ||
                 !highlightedTrackSegments[i].Equals(thisnot)) &&
                 (!onlyeditable ||
                  highlightedTrackSegments[i].IsEditable)) {
               track = highlightedTrackSegments[i];
               break;
            }
         }
         return track;
      }

      /// <summary>
      /// zeigt Infos zum nächstliegenden zum Point liegenden <see cref="Track"/>-Punkt des <see cref="Track"/>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="ptclient"></param>
      void showTrackPointInfo(Track track, Point ptclient) {
         int idx = track.GetNearestPtIdx(mapControl1.MapClient2LonLat(ptclient));
         if (idx >= 0) {
            Gpx.GpxTrackPoint pt = track.GetGpxPoint(idx);
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("nächstliegender Trackpunkt:");
            sb.AppendLine();
            sb.AppendFormat("Lng {0:F6}°, Lat {1:F6}°", pt.Lon, pt.Lat);
            if (pt.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE)
               sb.AppendFormat(", Höhe {0:F0} m", pt.Elevation);
            sb.AppendLine();
            double length = track.Length(0, idx);
            sb.AppendFormat("Streckenlänge bis zum Punkt: {0:F1} km ({1:F0} m)", length / 1000, length);
            sb.AppendLine();
            if (pt.Time != Gpx.BaseElement.NOTVALID_TIME) {
               sb.AppendLine(pt.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
            }
            sb.AppendLine();

            sb.AppendFormat("Gesamtlänge: {0:F1} km ({1:F0} m)", track.StatLength / 1000, track.StatLength);
            sb.AppendLine();
            if (track.StatMinDateTimeIdx >= 0 &&
                track.StatMaxDateTimeIdx > track.StatMinDateTimeIdx) {
               TimeSpan ts = track.StatMaxDateTime.Subtract(track.StatMinDateTime);
               sb.AppendFormat("Gesamt Datum/Zeit: {0} .. {1} (Dauer: {2} Stunden)",
                               track.StatMinDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                               track.StatMaxDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                               ts.ToString(@"h\:mm\:ss"));
               sb.AppendLine();
               sb.AppendFormat("Durchschnittsgeschwindigkeit: {0:F1} km/h", track.StatLengthWithTime / ts.TotalSeconds * 3.6);
               sb.AppendLine();
            }

            UIHelper.ShowInfoMessage(sb.ToString(), track.VisualName);
         }
      }

      /// <summary>
      /// zeigt einen Tooltip für die akt. markierten Tracks an
      /// </summary>
      void showToolTip4MarkedTracks() {
         if (highlightedTrackSegments.Count > 0) {
            string txt = "";
            for (int i = 0; i < highlightedTrackSegments.Count; i++) {
               if (i > 0)
                  txt += Environment.NewLine;
               highlightedTrackSegments[i].CalculateStats();
               txt += string.Format("{0} [{1:F1} km / {2:F0} m]",
                                    highlightedTrackSegments[i].VisualName,
                                    highlightedTrackSegments[i].StatLength / 1000,
                                    highlightedTrackSegments[i].StatLength);
            }
            mapControl1.MapShowToolTip(toolTipRouteInfo,
                                       txt,
                                       mapControl1.MapLastMouseLocation.X + 10,
                                       mapControl1.MapLastMouseLocation.Y - 10);
         } else
            mapControl1.MapHideToolTip(toolTipRouteInfo);
      }

      /// <summary>
      /// erzeugt einen "vereinfachten" Track zum vorgegebenen Track
      /// </summary>
      /// <param name="track"></param>
      void simplifyTrack(Track track) {
         FormTrackSimplificationcs dlg = new FormTrackSimplificationcs() {
            SrcTrack = track,
         };
         FormTrackSimplificationcs.SimplificationDataList.Clear();
         FormTrackSimplificationcs.SimplificationDataList.AddRange(appData.SimplifyDatasetList);

         if (dlg.ShowDialog() == DialogResult.OK &&
             dlg.DestTrack != null &&
             dlg.DestTrack.GpxSegment.Points.Count > 1) {

            int orgidx = gpxWorkbench.TrackIndex(track);
            gpxWorkbench.TrackInsertCopy(dlg.DestTrack, orgidx + 1);
         }

         appData.SimplifyDatasetList = FormTrackSimplificationcs.SimplificationDataList;
      }

      #endregion

      #region Funktionen für Marker

      /// <summary>
      /// Wenn tatsächlich Eigenschaften verändert werden, wird einer neuer (!) <see cref="Marker"/> mit diesen Eigenschaften erzeugt.
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="editable">veränderbar oder nur lesbar</param>
      void infoAndEditMarkerProps(Marker marker, bool editable) {
         if (marker != null)
            if (marker.IsEditable &&
                editable) {
               FormMarkerEditing form = new FormMarkerEditing() {
                  Marker = marker,
                  GarminMarkerSymbols = garminMarkerSymbols,
               };
               if (form.ShowDialog() == DialogResult.OK &&
                   form.WaypointChanged) {
                  if (gpxWorkbench.MarkerReplaceWaypoint(marker, marker))
                     editableTracklistControl1.SetMarkerName(marker, marker.Text);  // falls sich der Name geändert hat
               }
            } else {                   // nicht-modale Form
               FormMarkerEditing form = new FormMarkerEditing() {
                  Marker = marker,
                  MarkerIsReadOnly = true,
                  GarminMarkerSymbols = garminMarkerSymbols,
               };
               AddOwnedForm(form);
               form.Show(this);
            }
      }

      /// <summary>
      /// erzeugt eine bearbeitbare Kopie des <see cref="Marker"/> und fügt sie in die Liste ein
      /// </summary>
      /// <param name="orgmarker"></param>
      void cloneMarker2GpxWorkbench(Marker orgmarker) {
         if (orgmarker != null) {
            gpxWorkbench.MarkerInsertCopy(orgmarker, 0);
            showEditableMarker(0);
         }
      }

      /// <summary>
      /// zeigt die Infos zu einem Standard-Waypoint an
      /// </summary>
      /// <param name="wp"></param>
      void showStdMarkerInfo(Gpx.GpxWaypoint wp) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("Lng {0:F6}°, Lat {1:F6}°", wp.Lon, wp.Lat);
         sb.AppendLine();
         if (wp.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE) {
            sb.AppendFormat("Höhe {0:F0} m", wp.Elevation);
            sb.AppendLine();
         }
         if (wp.Time != Gpx.BaseElement.NOTVALID_TIME) {
            sb.AppendLine(wp.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
         }
         UIHelper.ShowInfoMessage(sb.ToString(), wp.Name);
      }

      #endregion

      #region Funktionen für Bilder-Marker

      /// <summary>
      /// zeigt eine modale Auswahl-Liste aller Bild-Waypoint in der (engen) Umgebung an
      /// </summary>
      /// <param name="localcenter">Punkt in dessen Umgebung gesucht wird</param>
      /// <param name="deltafactor">Faktor für die Größe des Bereiches (bezogen auf die Markergröße)</param>
      void showPictureMarkerList(Point localcenter, float deltafactor) {
         List<Marker> markerlst = mapControl1.MapGetPictureMarkersAround(localcenter,
                                                                            (int)Math.Round(deltafactor * VisualMarker.FotoMarker.Picture.Width),
                                                                            (int)Math.Round(deltafactor * VisualMarker.FotoMarker.Picture.Height));
         if (markerlst.Count > 0) {
            FormPictureMarkers form = new FormPictureMarkers(markerlst);
            form.ShowDialog(this);
         }
      }

      /// <summary>
      /// liefert die <see cref="FormPicture"/> für diese Bilddatei (falls sie ex.)
      /// </summary>
      /// <param name="picturefilename"></param>
      /// <returns></returns>
      public FormPicture GetForm4Picture(string picturefilename) {
         foreach (Form form in OwnedForms) {
            if (form is FormPicture) {
               if ((form as FormPicture).PictureFilename == picturefilename)
                  return form as FormPicture;
            }
         }
         return null;
      }

      /// <summary>
      /// zeigt das Bild zum Waypoint an ("Name" ist der Dateiname)
      /// </summary>
      /// <param name="wp"></param>
      public void ShowPicture(Gpx.GpxWaypoint wp) {
         FormPicture form = GetForm4Picture(wp.Name);
         if (form == null) { // ex. noch nicht
            form = new FormPicture(wp.Name, string.Format("Lng {0:F6}°, Lat {1:F6}°, {2}", wp.Lon, wp.Lat, wp.Name));
            AddOwnedForm(form);
            form.Show(this);
         } else
            form.Activate();
      }

      #endregion

      #region drucken

      PageSettings pageSettings = null;
      PrinterSettings printerSettings = null;
      PrintDialog printDialog = null;

      /// <summary>
      /// Umrechnung mm in 1/100 Zoll
      /// </summary>
      /// <param name="mm"></param>
      /// <returns></returns>
      double mm2inch100(double mm) {
         return mm * 100 / 25.4;
      }

      /// <summary>
      /// drucken einer Seite
      /// </summary>
      /// <param name="e"></param>
      /// <param name="pd"></param>
      void documentPrintPage(PrintPageEventArgs e, Image img) {
         e.HasMorePages = false;                         // sicherheitshalber erstmal die Druckerei beenden
         try {

            float fwidth = (float)e.MarginBounds.Width / img.Width;
            float fheight = (float)e.MarginBounds.Height / img.Height;
            float f = Math.Min(fwidth, fheight);
            e.Graphics.DrawImage(img, e.MarginBounds.Left, e.MarginBounds.Top, f * img.Width, f * img.Height);
            e.HasMorePages = false;

         } catch (Exception ex) {
            MyMessageBox.Show(ex.Message, "Fehler beim Drucken", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      #endregion

      #region Farbe auswählen

      /// <summary>
      /// liefert eine Argb-Farbe (oder Color.Empty) über einen Dialog
      /// </summary>
      /// <param name="orgcol">beim Start ausgewählte Farbe</param>
      /// <param name="withgarmincolors">wenn true, dann 16 Garminfarben als vordefinierte anzeigen</param>
      /// <param name="newcol">gewählte Farbe wenn true</param>
      /// <returns></returns>
      bool getColor(Color orgcol, bool withgarmincolors, out Color newcol) {
         Unclassified.UI.SpecColorSelectorDialog dlg = new Unclassified.UI.SpecColorSelectorDialog() {
            SelectedColor = orgcol,
         };

         if (withgarmincolors) {
            Color[] cols = new Color[] {
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Black],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGray],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkBlue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkCyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkMagenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkRed],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGreen],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkYellow],

               GarminTrackColors.Colors[GarminTrackColors.Colorname.White],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.LightGray],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Blue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Cyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Magenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Red],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Green],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Yellow],
            };
            for (int i = 0; i < cols.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, cols[i]);
            for (int i = cols.Length; i < dlg.ArrayColorsCount; i++)
               dlg.EnableArrayColor(i, false);
         } else {
            for (int i = 0; i < PredefColors.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, PredefColors[i]);
         }

         newcol = Color.Empty;
         if (dlg.ShowDialog() == DialogResult.OK) {
            newcol = dlg.SelectedColor;
            return true;
         }
         return false;
      }

      #endregion

      #region Programmstatus

      /// <summary>
      /// der akt. Status wird noch einmal gesetzt um bestimmte Korrekturen zu erzwingen
      /// </summary>
      void refreshProgramState() {
         programState = programState;
      }

      enum ProgState {
         /// <summary>
         /// Im Programm können keine Daten verändert werden. 
         /// </summary>
         Viewer,

         /// <summary>
         /// Im Programm können Marker gesetzt/verschoben werden. 
         /// </summary>
         Edit_SetMarker,
         /// <summary>
         /// Im Programm können Tracks gezeichnet werden. 
         /// </summary>
         Edit_DrawTrack,
         /// <summary>
         /// Im Programm können Tracks verbunden werden.
         /// </summary>
         Edit_ConcatTracks,
         /// <summary>
         /// Im Programm können Tracks getrennt werden.
         /// </summary>
         Edit_SplitTracks,
         /// <summary>
         /// eine neue Pos. für ein Bild wird angegeben
         /// </summary>
         Set_PicturePosition,
      };

      ProgState _programState = ProgState.Viewer;
      /// <summary>
      /// akt. Programm-Status
      /// </summary>
      ProgState programState {
         get => _programState;
         set {
            // KEIN Abbruch der Auswertung bei "_programState == value", weil sonst kein Refresh möglich wäre.

            // ev. noch "Aufräumarbeiten" für den bisherigen Status
            switch (_programState) {               // alter Status
               case ProgState.Edit_SetMarker:
                  gpxWorkbench.MarkerEndEdit();
                  break;

               case ProgState.Edit_DrawTrack:
                  gpxWorkbench.TrackEndDraw();
                  break;

               case ProgState.Edit_SplitTracks:
                  gpxWorkbench.TrackEndSplit(Point.Empty); // notfalls Abbruch
                  break;

               case ProgState.Edit_ConcatTracks:
                  gpxWorkbench.TrackEndConcat(null); // notfalls Abbruch
                  break;
            }
            mapControl1.Refresh();


            toolStripButton_OpenGpxfile.Enabled =
            toolStripButton_TrackSearch.Enabled = !readOnlyTracklistControl1.LoadGpxfilesIsRunning;

            switch (value) {
               case ProgState.Viewer:
                  mapControl1.MapCursor = cursors4Map.Std;
                  break;

               case ProgState.Edit_SetMarker:
                  mapControl1.MapCursor = cursors4Map.SetMarker;
                  break;

               case ProgState.Edit_DrawTrack:
                  mapControl1.MapCursor = cursors4Map.DrawTrack;
                  if (!gpxWorkbench.TrackIsInWork)
                     gpxWorkbench.TrackStartEdit(null);
                  toolStripButton_TrackDrawEnd.Enabled = true;    // beim Fortsetzen eines Tracks nötigs
                  break;

               case ProgState.Edit_SplitTracks:
                  mapControl1.MapCursor = cursors4Map.Split;
                  break;

               case ProgState.Edit_ConcatTracks:
                  mapControl1.MapCursor = cursors4Map.Concat;
                  break;

               case ProgState.Set_PicturePosition:
                  mapControl1.MapCursor = cursors4Map.Foto;
                  break;

            }

            _programState = value;
         }
      }

      #endregion

      void creatMapMenuManager() {
         mapMenuManager = new MapMenuManager(config, appData, ToolStripMenuItemMaps, mapControl1.MapProviderDefinitions, providxpaths);
         mapMenuManager.ActivateIdx += (s, ea) => {
            try {
               dem.WithHillshade = config.Hillshading(providxpaths[ea.ProviderIdx]);
               mapControl1.MapSetActivProvider(ea.ProviderIdx, config.HillshadingAlpha(providxpaths[ea.ProviderIdx]), dem);
            } catch (Exception ex) {
               UIHelper.ShowExceptionError(ex);
            }
         };

         string lastmapname = appData.LastMapname;
         int lastmapnameidx = -1;
         for (int i = 0; i < mapControl1.MapProviderDefinitions.Count; i++) {
            if (lastmapname == mapControl1.MapProviderDefinitions[i].MapName)
               lastmapnameidx = i;
         }
         if (lastmapnameidx >= 0)
            mapMenuManager.ActualProviderIdx = lastmapnameidx;
         else {
            if (mapControl1.MapProviderDefinitions.Count > 0)
               mapMenuManager.ActualProviderIdx = Math.Max(0, Math.Min(config.StartProvider, mapControl1.MapProviderDefinitions.Count - 1));
         }
      }

      /// <summary>
      /// den Zoom und den Mittelpunkt der Karte setzen
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void SetMapLocationAndZoom(double zoom, double lon, double lat) =>
         mapControl1.MapSetLocationAndZoom(zoom, lon, lat);

      /// <summary>
      /// liefert den aktuellen Zoom und Mittelpunkt der Karte
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public double GetMapLocationAndZoom(out double lon, out double lat) {
         lon = mapControl1.MapCenterLon;
         lat = mapControl1.MapCenterLat;
         return mapControl1.MapZoom;
      }

      /// <summary>
      /// <see cref="GpxWorkbench"/> "normal" speichern
      /// </summary>
      /// <param name="withdlg"></param>
      /// <param name="multifiles"></param>
      /// <returns>false, wenn speichern fehlerhaft</returns>
      bool saveWorkbench(bool withdlg = false, bool multifiles = false) {
         gpxWorkbench.Save();

         // Dateiname vorhanden?
         if (withdlg ||
             string.IsNullOrEmpty(lastSaveFilename)) {
            saveFileDialogGpx.FileName = string.IsNullOrEmpty(lastSaveFilename) ?
                                                      "neu.gpx" :
                                                      lastSaveFilename;
            saveFileDialogGpx.DefaultExt = "gpx";
            saveFileDialogGpx.Title = !multifiles ?
                                          "speichern als Datei ..." :
                                          "speichern als Einzeldateien ... (Basisdateiname)";
            if (saveFileDialogGpx.ShowDialog() == DialogResult.OK)
               lastSaveFilename = saveFileDialogGpx.FileName;
            else
               return false;
         }

         string creator = Progname + " " + Progversion;
         bool ok = IOHelper.SaveGpx(gpxWorkbench.Gpx, lastSaveFilename, multifiles, creator, saveWithGarminExtensions);
         if (ok)
            Text = creator + " - " + Path.GetFileName(lastSaveFilename);
         return ok;
      }

      /// <summary>
      /// falls eine Garmin-Karte angezeigt wird, werden Infos für Objekte im Bereich des Client-Punktes ermittelt
      /// </summary>
      /// <param name="ptclient"></param>
      void showObjectinfo(Point ptclient) {
         bool isGarmin = mapControl1.MapProviderDefinitions[mapControl1.MapGetActiveProviderIdx()].Provider is GarminProvider;

         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;

         List<GarminImageCreator.SearchObject> garmininfo = null;
         List<string> stdinfo = null;

         if (isGarmin) {
            int delta = (Math.Min(mapControl1.ClientSize.Height, mapControl1.ClientSize.Width) * config.DeltaPercent4Search) / 100;
            garmininfo = mapControl1.MapGetGarminObjectInfos(ptclient, delta, delta);

         } else {
            stdinfo = new List<string>();
            PointD pt = mapControl1.MapClient2LonLat(ptclient);
            foreach (GeoCodingReverseResultOsm item in GeoCodingReverseResultOsm.Get(pt.X, pt.Y)) {
               stdinfo.Add(item.Name);
            }
         }

         Cursor = orgCursor;

         if (garmininfo != null) {
            FormGarminInfo form = null;
            foreach (Form oform in OwnedForms) {
               if (oform is FormGarminInfo) {
                  form = oform as FormGarminInfo;
                  break;
               }
            }

            if (garmininfo.Count > 0) {
               if (form == null) {
                  form = new FormGarminInfo();
                  AddOwnedForm(form);
                  form.ClearListBox();
                  foreach (var item in garmininfo) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Show(this);
               } else {
                  form.ClearListBox();
                  foreach (var item in garmininfo) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Activate();
               }
            } else {
               if (form != null) {
                  form.ClearListBox();
                  form.Activate();
               }
            }
         } else {
            FormObjectInfo form = null;
            foreach (Form oform in OwnedForms) {
               if (oform is FormObjectInfo) {
                  form = oform as FormObjectInfo;
                  break;
               }
            }

            if (stdinfo.Count > 0) {
               if (form == null) {
                  form = new FormObjectInfo();
                  AddOwnedForm(form);
                  form.ClearListBox();
                  foreach (var item in stdinfo) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Show(this);
               } else {
                  form.ClearListBox();
                  foreach (var item in stdinfo) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Activate();
               }
            } else {
               if (form != null) {
                  form.ClearListBox();
                  form.Activate();
               }
            }
         }
      }

      void showMiniTrackInfo(Track track) {
         showMiniTrackInfo(track != null ?
                              new Track[] { track } :
                              new Track[] { });
      }

      void showMiniTrackInfo(IList<Track> tracks) {
         string info = "";
         if (tracks != null && tracks.Count > 0) {
            double len = 0;
            int pts = 0;
            foreach (Track track in tracks) {
               len += track.Length();
               pts += track.GpxSegment.Points.Count;
            }
            info = string.Format("{0} Punkte, {1:F1} km ({2:F0} m)",
                                 pts,
                                 len / 1000,
                                 len);
         }

         toolStripStatusLabel_TrackMiniInfo.Text = info;
      }

      void showMiniEditableTrackInfo(Track track) {
         string info = "";
         if (track != null) {
            double len = track.Length();
            info = string.Format("{0} Punkte, {1:F1} km ({2:F0} m)",
                                 track.GpxSegment.Points.Count,
                                 len / 1000,
                                 len);
         }
         toolStripStatusLabel_TrackInfo.Text = info;
      }

      #region Setting Geodata for Picture

      Marker geoTaggingMarker = null;

      private void toolStripButton_GeoTagging_Click(object sender, EventArgs e) {
         FormGeoTagger form = null;
         foreach (Form oform in OwnedForms) {
            if (oform is FormGeoTagger) {
               form = oform as FormGeoTagger;
               break;
            }
         }

         if (form == null) {
            form = new FormGeoTagger() {
               PicturePath = appData.LastPicturePath,
            };
            form.OnShowExtern += FormGeoTagger_OnShowExtern;

            form.OnHideExtern += (s, ea) => removeGeoTaggingMarker(ea.Filename);

            form.OnNeedNewData += (s, ea) => {
               if (ea.Latitude != double.MinValue &&
                   ea.Latitude != double.MinValue)
                  programState = ProgState.Set_PicturePosition;
               else {
                  if (getGpxLocation4Timestamp(ea.Timestamp, out double lat, out double lon))
                     form.SetExtern(ea.Filename, lon, lat);
               }
            };

            form.FormClosed += (s, ea) => appData.LastPicturePath = (s as FormGeoTagger).PicturePath;

            AddOwnedForm(form);
            form.Show(this);
         }
         form.Activate();
      }

      TimeSpan offsetPictureTime2GpxTime = new TimeSpan(0);

      bool getGpxLocation4Timestamp(DateTime timestamp, out double lat, out double lon) {
         lat = lon = double.MinValue;

         List<Track> tracks = new List<Track>();
         tracks.AddRange(readOnlyTracklistControl1.GetVisibleTracks());
         tracks.AddRange(gpxWorkbench.VisibleTracks());

         if (tracks.Count == 0) {
            MessageBox.Show("Es sind keine (sichtbaren) Tracks für die Suche vorhanden.",
                            "Achtung",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
            return false;
         }

         // Offset ermitteln (GPX i.A. in UTC, Bilddatum aus EXIF in lokaler Zeit)
         FormOffsetPictureGpx formOffsetPictureGpx = new FormOffsetPictureGpx() { Offset = offsetPictureTime2GpxTime };
         formOffsetPictureGpx.ShowDialog();
         offsetPictureTime2GpxTime = formOffsetPictureGpx.Offset;
         timestamp = timestamp.Subtract(offsetPictureTime2GpxTime);

         Track srctrack = null;
         foreach (var track in tracks) {
            if (track.IsVisible) {
               Gpx.GpxTrackPoint p1 = track.GpxSegment.Points[0];
               for (int i = 1; i < track.GpxSegment.Points.Count; i++) {
                  Gpx.GpxTrackPoint p2 = track.GpxSegment.Points[i];
                  if (Gpx.BaseElement.ValueIsValid(p1.Time) &&
                      Gpx.BaseElement.ValueIsValid(p2.Time) &&
                      p1.Time <= timestamp && timestamp <= p2.Time) {
                     TimeSpan ts1 = timestamp.Subtract(p1.Time);
                     TimeSpan ts2 = p2.Time.Subtract(p1.Time);
                     double f = ts1.TotalMilliseconds / ts2.TotalMilliseconds;
                     lat = p1.Lat + f * (p2.Lat - p1.Lat);
                     lon = p1.Lon + f * (p2.Lon - p1.Lon);
                     srctrack = track;
                     break;
                  } else
                     p1 = p2;
               }
            }
            if (lat != double.MinValue)
               break;
         }
         if (lat != double.MinValue) {
            if (MessageBox.Show("Sollen die gefundenen Koordinaten aus dem Track \"" + srctrack.VisualName + "\" übernommen werden?",
                                "Koordinaten übernehmen",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
               return true;
            return false;
         }

         MessageBox.Show("Für den Zeitpunkt " + timestamp.ToString("G") + " UTC" + Environment.NewLine + "wurden keine passenden GPX-Daten gefunden.",
                         "Achtung",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information);
         return false;
      }

      void setFotoPosition(double lon, double lat) {
         FormGeoTagger form = null;
         foreach (Form oform in OwnedForms) {
            if (oform is FormGeoTagger) {
               form = oform as FormGeoTagger;
               break;
            }
         }
         if (form != null && geoTaggingMarker != null)
            form.SetExtern(geoTaggingMarker.Text, lon, lat);
      }

      void removeGeoTaggingMarker(string filename) {
         if (geoTaggingMarker != null) {
            //ev. über geoTaggingMarker.Text==filename filtern

            mapControl1.MapShowMarker(geoTaggingMarker, false);
            geoTaggingMarker = null;
         }
      }

      private void FormGeoTagger_OnShowExtern(object sender, PictureManager.PictureDataEventArgs e) {
         //Debug.WriteLine("!!! OnShowExtern: " + e);
         removeGeoTaggingMarker(e.Filename);    // falls noch einer angezeigt wird
         geoTaggingMarker = new Marker(new Gpx.GpxWaypoint(double.IsNaN(e.Longitude) ? mapControl1.MapCenterLon : e.Longitude,
                                                           double.IsNaN(e.Latitude) ? mapControl1.MapCenterLat : e.Latitude),
                                       Marker.MarkerType.GeoTagging,
                                       null);
         geoTaggingMarker.Text = e.Filename;
         showMarker(geoTaggingMarker);
         if (geoTaggingMarker.Longitude != mapControl1.MapCenterLon ||
             geoTaggingMarker.Latitude != mapControl1.MapCenterLat)
            mapControl1.MapSetLocationAndZoom(mapControl1.MapZoom, geoTaggingMarker.Longitude, geoTaggingMarker.Latitude);
      }

      #endregion

      void showTileLoadInfo(int tilesinwork) {
         bool complete = /*tilesinwork <= 0 ||*/ Interlocked.Read(ref tileLoadIsRunning) == 0;
         //Debug.WriteLine(">>> tilesinwork=" + tilesinwork + ", tileLoadIsRunning=" + Interlocked.Read(ref tileLoadIsRunning) + ", complete=" + complete);
         toolStripStatusLabel_MapLoad.Text = complete ? "OK" : ("load " + tilesinwork);
         toolStripStatusLabel_MapLoad.BackColor = complete ?
                                                      Color.LightGreen :
                                                      Color.LightSalmon;
         toolStripButton_CancelMapLoading.Enabled = !complete;
#if DEBUG
         if (tilesinwork <= 0) {
            Debug.WriteLine(":::: Map-LoadTime: " + (DateTime.Now.Subtract(dtLoadTime).TotalSeconds.ToString("F1") + "s ::::"));
         } else {
            dtLoadTime = DateTime.Now;
         }
#endif
      }

      private void ToolStripMenuIemConfig_Click(object sender, EventArgs e) {
         try {
            if (new FormConfig() {
               Configuration = config,
               ActualCachePath = mapControl1.MapCacheLocation,
               ProviderDefs = mapControl1.MapProviderDefinitions,
               ProvIdxPaths = providxpaths
            }.ShowDialog() == DialogResult.OK) { // neue Konfigurationsdatei geschrieben
               appData.Save();
               gpxWorkbench.Save();

               initAllWithInfo();
               creatMapMenuManager();

               MyMessageBox.Show(@"Je nach veränderten Daten der Konfiguration muss ev. der Kartencache für eine oder alle Karten gelöscht werden!
                                   
Sonst werden die Änderungen ev. nicht wirksam.",
                                 "Achtung",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Exclamation);
            }
         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
         }
      }


#if GARMINDRAWTEST

      FSofTUtils.Sys.HighResolutionWatch hrw = new FSofTUtils.Sys.HighResolutionWatch();

      void garminTest(int width, int height,
                      double lonleft, double lonright, double latbottom, double lattop,
                      int zoom) {

         //Bitmap bmtest = BitmapHelper.Testbild2(200, 150, 128);
         //bmtest.Save("garminTest.png", System.Drawing.Imaging.ImageFormat.Png);
         //return;

         string[] providernames = config.Provider;
         GarminProvider.GarminMapDefinitionData garminMapDefinitionData = null;
         int hillshadingAlpha = 0;
         for (int providx = 0; providx < providernames.Length; providx++) {
            if (providernames[providx] == GarminProvider.Instance.Name) {
               garminMapDefinitionData = new GarminProvider.GarminMapDefinitionData(config.MapName(providx),
                                                                       config.Zoom4Display(providx),
                                                                       config.MinZoom(providx),
                                                                       config.MaxZoom(providx),
                                                                       new string[] {
                                                                             PathHelper.ReplaceEnvironmentVars(config.GarminTdb(providx)),
                                                                       },
                                                                       new string[] {
                                                                             PathHelper.ReplaceEnvironmentVars(config.GarminTyp(providx)),
                                                                       },
                                                                       config.GarminLocalCacheLevels(providx),
                                                                       config.GarminMaxSubdiv(providx),
                                                                       config.GarminTextFactor(providx),
                                                                       config.GarminLineFactor(providx),
                                                                       config.GarminSymbolFactor(providx));
               hillshadingAlpha = config.HillshadingAlpha(providx);
               break;
            }
         }

         if (garminMapDefinitionData == null)
            return;

         List<GarminImageCreator.GarminMapData> mapdata = new List<GarminImageCreator.GarminMapData>();
         for (int i = 0; i < garminMapDefinitionData.TDBfile.Count && i < garminMapDefinitionData.TYPfile.Count; i++) {
            mapdata.Add(new GarminImageCreator.GarminMapData(garminMapDefinitionData.TDBfile[i],
                                                             garminMapDefinitionData.TYPfile[i],
                                                             "",
                                                             garminMapDefinitionData.Levels4LocalCache,
                                                             garminMapDefinitionData.MaxSubdivs,
                                                             garminMapDefinitionData.TextFactor,
                                                             garminMapDefinitionData.LineFactor,
                                                             garminMapDefinitionData.SymbolFactor));
         }
         GarminImageCreator.ImageCreator ic = new GarminImageCreator.ImageCreator(mapdata);

         List<GarminImageCreator.GarminMapData> mapData = ic.GetGarminMapDefs();
         double[] groundresolution = new double[mapData.Count];
         for (int m = 0; m < mapData.Count; m++)
            groundresolution[m] = 0;

         // den gewünschten Bereich auf das Bitmap zeichnen
         Bitmap bm = new Bitmap(width, height);


         //for (int i = 0; i < 20; i++) {
         //   hillshadeTest(bm, lonleft, latbottom, lonright, lattop, hillshadingAlpha);
         //}
         //return;


         for (int i = 0; i < 3; i++) {

            object extdata = null;

            ic.DrawImage(bm,
                         lonleft, latbottom,
                         lonright, lattop,
                         mapData,              // wieder zurückliefern, falls inzwischen geändert
                         groundresolution,
                         zoom,
                         GarminImageCreator.ImageCreator.PictureDrawing.beforehillshade,
                         ref extdata);

            //hillshadeTest(bm, lonleft, latbottom, lonright, lattop, hillshadingAlpha);

            ic.DrawImage(bm,
                         lonleft, latbottom,
                         lonright, lattop,
                         mapData,              // wieder zurückliefern, falls inzwischen geändert
                         groundresolution,
                         zoom,
                         GarminImageCreator.ImageCreator.PictureDrawing.afterhillshade,
                         ref extdata);

            GC.Collect();
         }

         bm.Save("garminTest.png", System.Drawing.Imaging.ImageFormat.Png);
         bm.Dispose();
         //return bm;
      }

      void hillshadeTest(Bitmap bm,
                         double left,
                         double bottom,
                         double rigth,
                         double top,
                         int alpha = 100) {
         hrw.Start();
         hrw.Store("Start hillshadeTest()");

         FSofTUtils.Geography.DEM.DemData
         dem = new FSofTUtils.Geography.DEM.DemData(string.IsNullOrEmpty(config.DemPath) ?
                                                                "" :
                                                                PathHelper.ReplaceEnvironmentVars(config.DemPath),
                                                    config.DemCachesize);
         dem.WithHillshade = true;
         dem.SetNewHillshadingData(config.DemHillshadingAzimut,
                                   config.DemHillshadingAltitude,
                                   config.DemHillshadingScale,
                                   config.DemHillshadingZ);

         hrw.Store("SetNewHillshadingData() ready");

         double deltalon = (rigth - left) / bm.Width;
         double deltalat = -(bottom - top) / bm.Height;

         //Bitmap bmhs = new Bitmap(bm.Width, bm.Height);
         //for (int y = 0; y < bm.Width; y++)
         //   for (int x = 0; x < bm.Height; x++) {
         //      byte s = dem.GetShadingValue(left + x * deltalon, top - y * deltalat);

         //      bmhs.SetPixel(x, y, Color.FromArgb(alpha, s, s, s));
         //      //bmhs.SetPixel(x, y, Color.FromArgb(255 - s, s, s, s));
         //      //bmhs.SetPixel(x, y, Color.FromArgb(255 - s, 120, 120, 120));
         //   }

         // etwa 10..15% schneller:
         uint[] pixel = new uint[bm.Width * bm.Height];
         for (int y = 0; y < bm.Width; y++)
            for (int x = 0; x < bm.Height; x++) {
               byte s = dem.GetShadingValue(left + x * deltalon, top - y * deltalat);
               pixel[x + y * bm.Width] = BitmapHelper.GetUInt4Color(alpha, s, s, s);
            }
         Bitmap bmhs = BitmapHelper.CreateBitmap32(bm.Width, bm.Height, pixel);

         hrw.Store("Hillshading-Bitmap ready");

         Graphics canvas = Graphics.FromImage(bm);
         canvas.DrawImage(bmhs, 0, 0);

         canvas.Flush();
         canvas.Dispose();

         hrw.Store("End hillshadeTest()");
         hrw.Stop();

         bmhs.Dispose();
         dem.Dispose();


         for (int i = 0; i < hrw.Count; i++) {
            System.Diagnostics.Debug.WriteLine(string.Format("HILLSHADETEST:  {0,7:F1} {1,7:F1} {2}",
                                                             hrw.Seconds(i) * 1000,
                                                             hrw.StepSeconds(i) * 1000,
                                                             hrw.Description(i)));
         }

      }

#endif

      //Sqlite4Dem.DemDatabase d = new Sqlite4Dem.DemDatabase("dem.sqlite");

      //d.Insert(1, 2, new byte[] { 99 });
      //d.Insert(1, -2, new byte[] { 77 });

      //byte[] b1 = d.Get(0, 0);
      //byte[] b2 = d.Get(1, -2);

   }
}