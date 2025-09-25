//#define LOCALDEBUG
//#define GARMINDRAWTEST
//#define SHADINGDRAWTEST

using FSofTUtils;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geometry;
using FSofTUtils.Threading;
using GMap.NET.FSofTExtented.MapProviders;
using GpxViewer.Common;
using GpxViewer.ConfigEdit;
using GpxViewer.PictureEdit;
using SpecialMapCtrl;
using System.Diagnostics;
using System.Reflection;
using System.Text;
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
      delegate void SafeCallDelegate4Void2Void();

      /// <summary>
      /// für den threadübergreifenden Aufruf von <see cref="setGpxLoadInfo"/>() nötig 
      /// </summary>
      delegate void SafeCallDelegate4String2Void(string text);


      AppData? appData;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      DemData? dem = null;

      /// <summary>
      /// Soll eine Speicherung mit den Garmin-Erweiterungen erfolgen? (notwendig z.B. für spez. Markerbilder)
      /// </summary>
      bool saveWithGarminExtensions => toolStripButton_SaveWithGarminExt.Checked;

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol>? garminMarkerSymbols;

      /// <summary>
      /// (ev. benutzerdef.) Farben für Auswahldialog
      /// </summary>
      internal static Color[] PredefColors = [
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
      ];

      List<int[]> providxpaths = [];

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
      readonly List<Track> highlightedTracks = [];

      /// <summary>
      /// nur für den Wechsel zwischen Standard- und Waitcursor
      /// </summary>
      Cursor? lastCursor = null;

      /// <summary>
      /// diverse Cursoren für die Karte
      /// </summary>
      Cursors4Map? cursors4Map;

      /// <summary>
      /// Konfigurationsdaten
      /// </summary>
      Config? config;

      /// <summary>
      /// Programmname (aus der Assemblyinfo)
      /// </summary>
      string Progname = string.Empty;

      /// <summary>
      /// Programmversion (aus der Assemblyinfo)
      /// </summary>
      string Progversion = string.Empty;

      string lastSaveFilename = string.Empty;

      /// <summary>
      /// Ist das Program in irgendeinem Editier-Status?
      /// </summary>
      bool progIsInAnyEditState => programState != ProgState.Viewer ||
                                   (gpxWorkbench != null && gpxWorkbench.MarkerIsInWork);

      FormSplashScreen? formSplashScreen = null;

      MapMenuManager? mapMenuManager;

#if LOCALDEBUG
      DateTime dtLoadTime = DateTime.Now;
#endif

      GpxWorkbench? gpxWorkbench;

      PhotoEdit? photoEdit;


      /// <summary>
      /// kapselt die Zoom-Trackbar
      /// </summary>
      class ZoomControler {

         public EventHandler? OnValueChanged;

         TrackBar ctrl;

         bool setInternal = false;


         public double Zoom {
            get => ctrl.Value * 24.0 / (ctrl.Maximum - ctrl.Minimum);
            set {
               setInternal = true;

               ThreadsafeInvoker.InvokeControlPropertyWriter(ctrl, nameof(TrackBar.Value), (int)Math.Round(value * (ctrl.Maximum - ctrl.Minimum) / 24.0));
               //ctrl.Value = (int)Math.Round(value * (ctrl.Maximum - ctrl.Minimum) / 24);
               setInternal = false;
            }
         }


         public ZoomControler(TrackBar trackBar) {
            ctrl = trackBar;

            ctrl.ValueChanged += (s, e) => {
               if (!setInternal)
                  OnValueChanged?.Invoke(this, EventArgs.Empty);
            };
         }

      }

      ZoomControler? zoomControler;




      public FormMain() {
         try {
            InitializeComponent();
            readOnlyTracklistControlEventInit();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);    // für Codepages in SimpleXmlDocument

            zoomControler = new ZoomControler(trackBarZoom);
            zoomControler.OnValueChanged += (s, e) => {
               ProgCore.SetMapZoomTS(mapCtrl, zoomControler.Zoom);
            };

#if LOCALDEBUG
            //GarminImageCreator.ImageCreator.test();

            //CheckRouteCrossing rc = new CheckRouteCrossing();
            //List<string> lst = new List<string>();
            //rc.Testpaths(new string[] { "c:\\Users\\puf\\Daten\\Programmierung\\GpxViewer\\bin\\Debug\\FSofT\\GpxViewer" },
            //             lst,
            //             11, 12,
            //             52, 52,
            //             null);

#endif

            int trackcount = 0, markercount = 0;
            GpxWorkbench.LoadInfoEvent += (sender, e) => {
               string txt = string.Empty;
               switch (e.LoadReason) {
                  case GpxWorkbench.LoadEventArgs.Reason.ReadXml:
                     txt = "GPX einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadGDB:
                     txt = "GDB einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadKml:
                     txt = "KML/KMZ einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertWaypoints:
                     txt = "Marker einfügen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertTracks:
                     txt = "Tracks einfügen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertWaypoint:
                     if (gpxWorkbench != null)
                        txt = " " + gpxWorkbench.MarkerCount + " Marker gelesen";
                     else {
                        markercount++;
                        txt = " " + markercount + " Marker gelesen";
                     }
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertTrack:
                     if (gpxWorkbench != null)
                        txt = " " + gpxWorkbench.TrackCount + " Track" + (gpxWorkbench.TrackCount != 1 ? "s" : string.Empty) + " gelesen";
                     else {
                        trackcount++;
                        txt = " " + trackcount + " Track" + (trackcount != 1 ? "s" : string.Empty) + " gelesen";
                     }
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.SplitMultiSegmentTracks:
                     txt = "MultiSegmentTracks aufteilen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RemoveEmptyTracks:
                     txt = "leere Tracks entfernen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RebuildTrackList:
                     txt = "Trackliste erzeugen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RebuildMarkerList:
                     txt = "Markerliste erzeugen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadIsReady:
                     txt = "Workbench eingelesen";
                     break;
               }
               appendStartInfo("  " + txt);
            };

         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
            BindingContextChanged += FormMain_BindingContextChanged;  // 1. Event nach HandleCreated
         }

      }

      #region Initialisierung

      void startSplashScreen() {
         if (formSplashScreen == null) {
            formSplashScreen = new FormSplashScreen();
            Thread splashThread = new(new ThreadStart(() => Application.Run(formSplashScreen)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();
         }
      }

      void endSplashScreen() {
         formSplashScreen?.End();
         formSplashScreen = null;
         Activate();    // FormMain im Vordergrund
      }

      void appendStartInfo(string txt) => formSplashScreen?.AppendTextLine(txt);

      /// <summary>
      /// gesamte Init. mit Anzeige im <see cref="FormSplashScreen"/>
      /// <para>Bei einem schweren Fehler wird das Programm geschlossen.</para>
      /// <param name="withworkbench"></param>
      /// <returns>false bei Abbruch</returns>
      async Task<bool> initAllWithInfoAsync(bool withworkbench) {
         startSplashScreen();
         appendStartInfo(Text + " startet ...");

         try {
            await initAllAsync(withworkbench);

            //throw new Exception("TEST");
         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
            Close();
            return false;
         } finally {
            endSplashScreen();
         }
         return true;
      }

      async Task initAllAsync(bool withworkbench) {
         appendStartInfo("Init ...");

         string progpath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty;
         string mainpath =
#if DEBUG
                           progpath;
#else
                           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
         appData = new AppData(PRIVATEAPPLICATIONDATAPATH, mainpath);

         try {
            appendStartInfo("initDepTools() ...");
            // Bis auf Ausnahmen muss die gesamte Init-Prozedur fehlerfrei laufen. Sonst erfolgt ein Prog-Abbruch.
            string datapath = Path.Combine(mainpath, PRIVATEAPPLICATIONDATAPATH);

            UIHelper.ExceptionLogfile = Path.Combine(datapath, ERRORLOGFILE);
            editableTracklistControl1.SetBackupPath(datapath);

            if (initDataPath(datapath)) {
               string currentpath = Directory.GetCurrentDirectory();
               Directory.SetCurrentDirectory(datapath);

               string configfile = Path.Combine(progpath, CONFIGFILE);
               appendStartInfo(nameof(initConfig) + "(" + configfile + ") ...");
               config = initConfig(configfile);

               appendStartInfo(nameof(initDEM) + "() ...");
               dem = initDEM(config);
               appendStartInfo("   DemPath " + config.DemPath);
               appendStartInfo("   DemCachesize " + config.DemCachesize);
               appendStartInfo("   DemCachePath " + config.DemCachePath);

               mapCtrl.M_CacheLocation = Path.Combine(datapath, "filecache");

               appendStartInfo(nameof(initMapProvider) + "() ...");
               Directory.SetCurrentDirectory(currentpath);
               initMapProvider(mapCtrl, config, dem);
               Directory.SetCurrentDirectory(datapath);

               appendStartInfo(nameof(initAndStartMapAsync) + "() ...");
               await initAndStartMapAsync(mapCtrl, config);

               appendStartInfo(nameof(ProgCore.SetProviderWithZoomPositionAsync) + "() ...");
               int idx = config.StartProvider;
               for (int i = 0; i < mapCtrl.M_ProviderDefinitions.Count; i++) {
                  if (mapCtrl.M_ProviderDefinitions[i].MapName == appData.LastMapname) {
                     idx = i;
                     break;
                  }
               }
               await ProgCore.SetProviderWithZoomPositionAsync(
                                                  idx,                 // entweder config.StartProvider oder entsprechend appData.LastMapname
                                                  appData.LastZoom,
                                                  appData.LastLongitude,
                                                  appData.LastLatitude,
                                                  dem,
                                                  providxpaths,
                                                  config.Zoom4Displayfactor,
                                                  mapCtrl);
               appendStartInfo(nameof(initVisualTrackData) + "() ...");
               initVisualTrackData(config);

               try {
                  appendStartInfo(nameof(initGarminMarkerSymbols) + "() ...");
                  garminMarkerSymbols = initGarminMarkerSymbols(progpath, config);
                  VisualMarker.RegisterExternSymbols(garminMarkerSymbols);
               } catch (Exception ex) {
                  UIHelper.ShowExceptionMessage(this, "Fehler beim Lesen der Garmin-Symbole", ex, null, false);
               }

               if (withworkbench) {
                  appendStartInfo(nameof(initWorkbench) + "() ...");
                  gpxWorkbench = initWorkbench(config, appData, Path.Combine(datapath, WORKBENCHGPXFILE), mapCtrl, dem);
                  appendStartInfo("   Tracks: " + gpxWorkbench.TrackCount);
                  appendStartInfo("   Marker: " + gpxWorkbench.MarkerCount);
               }

               Directory.SetCurrentDirectory(currentpath);
            }
         } catch (Exception ex) {
            UIHelper.ShowExceptionMessage(this, "Fehler", ex, null, false);  // Abbruch
            throw new Exception("Abbruch", ex);
         }

         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         //mapControl2.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;

      }

      static bool initDataPath(string datapath) {
         if (!Directory.Exists(datapath))
            try {
               Directory.CreateDirectory(datapath);
            } catch {
               return false;
            }
         return true;
      }

      static Config initConfig(string configfile) =>
         new(configfile, null);

      static DemData initDEM(Config cfg) => ConfigHelper.ReadDEMDefinition(cfg);

      void initMapProvider(MapCtrl map, Config cfg, DemData dem) {
         List<MapProviderDefinition> provdefs = ConfigHelper.ReadProviderDefinitions(cfg, out providxpaths, out List<string> providernames, dem);
         for (int i = 0; i < provdefs.Count; i++)
            appendStartInfo("   " + provdefs[i].MapName + " (" + provdefs[i].ProviderName + ")");
         map.M_RegisterProviders(providernames, provdefs);
      }

      async Task initAndStartMapAsync(MapCtrl map, Config cfg) {
         Screen myScreen = Screen.FromControl(this);
         int maxdisplaysize = Math.Max(myScreen.WorkingArea.Width, myScreen.WorkingArea.Height); // größte Bildschirmseite

         mapCtrl.M_TileLoadChange += map_MapTileLoadChange;

         map.M_SetAreaSelectionEndPointEvent += map_MapTrackSearch4PolygonEvent;
         map.M_NonFracionalZoomChanged += map_OnZoomChanged;
         map.M_ExceptionThrown += (Exception ex) =>
           UIHelper.ShowExceptionMessage(this, "Fehler bei " + nameof(map.M_ExceptionThrown), ex, null, false);

         map.M_Mouse += map_SpecMapMouseEvent;
         map.M_Marker += map_SpecMapMarkerEvent;
         map.M_Track += map_SpecMapTrackEvent;
         map.M_DrawOnTop += map_SpecMapDrawOnTopEvent;

         //map.ShowTileGridLines = true;                 // mit EmptyTileBorders gezeichnet
         //map.ShowCenter = true;                        // shows a little red cross on the map to show you exactly where the center is
         map.M_EmptyMapBackgroundColor = Color.LightYellow;   // Tile (noch) ohne Daten
         map.M_EmptyTileText = "keine Daten";            // Hinweistext für "Tile ohne Daten"
         map.M_EmptyTileColor = Color.LightGray;        // Tile (endgültig) ohne Daten

         MapCtrl.M_CacheIsActiv = !cfg.ServerOnly;
         MapCtrl.M_SetProxy(cfg.WebProxyName,
                            cfg.WebProxyPort,
                            cfg.WebProxyUser,
                            cfg.WebProxyPassword);

         map.M_ClickTolerance4Tracks = maxdisplaysize * (float)cfg.ClickTolerance4Tracks / 100F;

         List<MapProviderDefinition> provdefs = map.M_ProviderDefinitions;
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

         map.M_ShowTileGridLines = false; // auch bei DEBUG
         map.M_DragButton = MouseButtons.Left;

         if (startprovider >= 0)
            await ProgCore.SetProviderWithZoomPositionAsync(
                              startprovider,
                              appData.LastZoom,
                              appData.LastLongitude,
                              appData.LastLatitude,
                              dem,
                              providxpaths,
                              config.Zoom4Displayfactor,
                              mapCtrl);
      }

      static void initVisualTrackData(Config cfg) => ConfigHelper.ReadVisualTrackDefinitions(cfg);

      static List<GarminSymbol> initGarminMarkerSymbols(string datapath, Config cfg) =>
         ConfigHelper.ReadGarminMarkerSymbols(cfg, datapath);


      ThreadSafeBoolVariable markerShouldInsertEventInWorking = new ThreadSafeBoolVariable(false);

      GpxWorkbench initWorkbench(Config config, AppData appData, string gpxworkbenchfile, MapCtrl map, DemData dem) {
         GpxWorkbench wb = new(map,
                               dem,
                               gpxworkbenchfile,
                               config.HelperLineColor,
                               config.HelperLineWidth,
                               config.EditableTrackColor,
                               config.EditableTrackWidth,
                               config.SymbolZoomfactor,
                               appData.GpxDataChanged);
         //wb.SetBusyStatusEvent += (s, e) => showBusyStatus(s, e);

         if (map != null) {
            // Nach dem Einlesen sind alle Tracks und Marker "unsichtbar".
            List<bool> tmp = appData.VisibleStatusTrackList;
            for (int i = 0; i < tmp.Count && i < wb.TrackCount; i++)
               if (tmp[i])
                  ProgCore.ShowTrack(wb.GetTrack(i), true, mapCtrl, readOnlyTracklistControl1, editableTracklistControl1);

            tmp = appData.VisibleStatusMarkerList;
            for (int i = 0; i < tmp.Count && i < wb.MarkerCount; i++)
               if (tmp[i])
                  ProgCore.ShowMarker(wb.GetMarker(i), true, mapCtrl, readOnlyTracklistControl1, editableTracklistControl1);
         }

         // Eventbehandlung
         wb.Gpx.ChangeIsSet += (s, e) => {
            if (s is GpxData gpx)
               changeWorkbenchListChanged(gpx);
         };
         wb.Gpx.TracklistChanged += (s, e) => changeWorkbenchListChanged(s as GpxData);
         wb.Gpx.MarkerlistlistChanged += (s, e) => changeWorkbenchListChanged(s as GpxData);
         wb.MarkerShouldInsertEvent += async (s, e) => {
            if (!markerShouldInsertEventInWorking.Value) {
               markerShouldInsertEventInWorking.Value = true;

               showWaitCursor();
               string[]? names = gpxWorkbench != null ?
                                    await gpxWorkbench.GetNamesForGeoPointAsync(e.Marker.Longitude, e.Marker.Latitude) :
                                    null;
               showLastCursor();

               if (string.IsNullOrEmpty(e.Marker.Waypoint.Name))  // Dummy-Name erzeugen
                  e.Marker.Waypoint.Name = names != null && names.Length > 0 ? names[0] : string.Empty;

               FormMarkerEditing form = new() {
                  Marker = e.Marker,
                  GarminMarkerSymbols = garminMarkerSymbols,
                  Proposals = names,
               };

               if (form.ShowDialog() == DialogResult.OK) {
                  if (string.IsNullOrEmpty(e.Marker.Waypoint.Name))  // Dummy-Name erzeugen
                     e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
                  gpxWorkbench.MarkerInsertCopy(e.Marker, 0);
                  ProgCore.showWorkbenchMarker(0, true, gpxWorkbench, mapCtrl, editableTracklistControl1);
               }
               markerShouldInsertEventInWorking.Value = false;
            }
         };

         changeWorkbenchListChanged(wb.Gpx);

         return wb;
      }

      void changeWorkbenchListChanged(GpxData? gpx) {
         if (gpx != null) {
            toolStripButton_ClearEditable.Enabled =
            toolStripButton_SaveGpxFiles.Enabled =
            toolStripButton_SaveGpxFileExt.Enabled = (gpx.TrackList.Count > 0 || gpx.Waypoints.Count > 0);     // "speichern unter" ist immer aktiv, wenn min. 1 Track oder 1 Marker vorhanden ist
         }
      }

      #endregion

      #region Events der Form

      private void FormMain_BindingContextChanged(object? sender, EventArgs e) => Close();

      private async void FormMain_Load(object sender, EventArgs e) => await progStarting();

      private void FormMain_Shown(object sender, EventArgs e) => endSplashScreen();

      private void FormMain_FormClosing(object sender, FormClosingEventArgs e) => e.Cancel = progClosing();

      private async void FormMain_KeyDown(object sender, KeyEventArgs e) {
#if LOCALDEBUG
         Debug.WriteLine(">>> FormMain: KeyValue=" + e.KeyValue + ", KeyData=" + e.KeyData + ", KeyCode=" + e.KeyCode + ", Handled=" + e.Handled);
#endif
         e.Handled = await action4KeyAsync(e.KeyData,
                                           mapCtrl,
                                           gpxWorkbench,
                                           photoEdit,
                                           pictureManager1,
                                           editableTracklistControl1,
                                           tabControl1);
      }

      private async void Form_GoToEvent(object? sender, SearchControl.GoToPointEventArgs e) =>
         await ProgCore.GotoPointAndSetNewWorkbenchMarker(e.Longitude, e.Latitude, e.Name, mapCtrl, gpxWorkbench);

      private async void Form_GoToAreaEvent(object? sender, SearchControl.GoToAreaEventArgs e) =>
         await ProgCore.GotoMapareaAndSetNewWorkbenchMarker(
                                                new PointD(e.Left, e.Top),
                                                new PointD(e.Right, e.Bottom),
                                                e.Longitude,
                                                e.Latitude,
                                                e.Name,
                                                mapCtrl,
                                                gpxWorkbench);

      private void FormMain_SelectedPoints(object? sender, FormTrackInfoAndEdit.SelectedPointsEventArgs e) =>
         ProgCore.ShowSelectedPart4Track(e.Track, e.PointList, mapCtrl);

      #endregion

      #region Events der Haupt-Toolbar-Buttons des Programms

      private async void toolStripButton_Config_Click(object sender, EventArgs e) => await editProgConfig();

      private void toolStripButton_CancelMapLoading_Click(object sender, EventArgs e) => ProgCore.CancelMapLoading(mapCtrl);

      void toolStripButton_OpenGpxfile_Click(object? sender, EventArgs e) =>
         openReadonlyGpxfile(openFileDialogGpx, readOnlyTracklistControl1);

      //private async void toolStripButton_SaveGpxFile_Click(object sender, EventArgs e) => await saveWorkbenchAsync();

      private async void toolStripButton_SaveGpxFileExt_Click(object sender, EventArgs e) => await saveWorkbenchAsync(true);

      private async void toolStripButton_SaveGpxFiles_Click(object sender, EventArgs e) => await saveWorkbenchAsync(true, true);

      private void toolStripButton_ZoomIn_Click(object? sender, EventArgs e) => ProgCore.SetMapZoomTS(mapCtrl, getZoom() + 1);

      private void toolStripButton_ZoomOut_Click(object? sender, EventArgs e) => ProgCore.SetMapZoomTS(mapCtrl, getZoom() - 1);

      private void toolStripButton_CopyMap_Click(object sender, EventArgs e) => copyMap2Clipboard(mapCtrl);

      private void toolStripButton_PrintMap_Click(object sender, EventArgs e) => printMap(mapCtrl);

      private void toolStripButton_ReloadMap_Click(object sender, EventArgs e) => reloadMap();

      //private void toolStripButton_ClearCache_Click(object sender, EventArgs e) => clearMapCache(mapControl2, mapMenuManager.ActualProviderIdx);
      private void toolStripButton_ClearCache_Click(object sender, EventArgs e) => new FormCache(mapCtrl, mapMenuManager.ActualProviderIdx).ShowDialog(this);

      private async void toolStripButton_TrackZoom_Click(object sender, EventArgs e) => await zoom4Tracks(readOnlyTracklistControl1, gpxWorkbench);

      void toolStripButton_TrackSearch_Click(object? sender, EventArgs e) => trackSearch4Area(mapCtrl, readOnlyTracklistControl1);

      private void toolStripButton_MiniHelp_Click(object? sender, EventArgs e) => showMiniHelp(this);

      //private void toolStripMenuItem_TestClick(object sender, EventArgs e) => new FormCache(mapControl2, mapMenuManager.ActualProviderIdx).ShowDialog(this);

      #endregion

      #region Events der Edit-Toolbar-Buttons des Programms

      /// <summary>
      /// in den Viewer-Modus schalten
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void toolStripButton_ViewerMode_Click(object sender, EventArgs e) =>
         editToolStripButton_SetOnlyOneChecked(sender as ToolStripButton);

      /// <summary>
      /// in den Marker-Setz-Modus schalten
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void toolStripButton_SetMarker_Click(object sender, EventArgs e) =>
         editToolStripButton_SetOnlyOneChecked(sender as ToolStripButton);

      /// <summary>
      /// in den Track-Zeichnen-Modus setzen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void toolStripButton_TrackDraw_Click(object sender, EventArgs e) =>
         editToolStripButton_SetOnlyOneChecked(sender as ToolStripButton);

      private void toolStripButton_EditEnd_Click(object sender, EventArgs e) => drawTrackEnd(gpxWorkbench);

      private void toolStripButton_EditCancel_Click(object sender, EventArgs e) {
         if (gpxWorkbench.MarkerIsInWork) {
            gpxWorkbench.MarkerEndEdit(true);
            (sender as ToolStripButton).Enabled = false;
            programState = ProgState.Viewer;
         } else
            switch (programState) {
               case ProgState.Edit_SetNewMarker:
               case ProgState.Edit_MoveMarker:
                  //gpxWorkbench.TrackEndEdit(true);
                  programState = ProgState.Viewer;
                  break;

               case ProgState.Edit_DrawTrack:
               case ProgState.Edit_ConcatTracks:
               case ProgState.Edit_SplitTracks:
               case ProgState.Edit_RemoveTrackpoint:
                  gpxWorkbench.TrackEndEdit(true);
                  programState = ProgState.Viewer;
                  break;
            }
      }

      private void toolStripButton_ClearEditable_Click(object sender, EventArgs e) =>
         ProgCore.RemoveTracksAndMarkerInWorkbench(
                              mapCtrl,
                              gpxWorkbench,
                              readOnlyTracklistControl1,
                              editableTracklistControl1);

      private void toolStripButton_UniqueNames_Click(object sender, EventArgs e) =>
         ProgCore.UniqueNamesInWorkbench(gpxWorkbench, editableTracklistControl1);

      void editToolStripButton_SetOnlyOneChecked(ToolStripButton? btn) {
         // Das Click-Event wird NUR bei einem Click auf den Button ausgelöst, nicht bei einer Änderung durch das Prog.

         if (btn != null) {
            if (!btn.Checked) {
               // kann NICHT direkt ausgeschaltet werden, sondern nur durch Akt. eines anderen Buttons oder des Prog.
               btn.Checked = true;
            } else {
               if (btn.Equals(toolStripButton_ViewerMode)) programState = ProgState.Viewer;
               else if (btn.Equals(toolStripButton_SetMarker)) programState = ProgState.Edit_SetNewMarker;
               else if (btn.Equals(toolStripButton_TrackDraw)) programState = ProgState.Edit_DrawTrack;
            }
         }
      }

      void setEditToolStripButtons4ProgMode(ProgState mode) {
         toolStripButton_EditEnd.Enabled = false;
         toolStripButton_EditCancel.Enabled = false;
         toolStripButton_ClearEditable.Enabled = false;
         toolStripButton_UniqueNames.Enabled = false;
         bool editisactiv = false;

         switch (mode) {
            case ProgState.Viewer:
            case ProgState.Set_PicturePosition:
               toolStripButton_ViewerMode.Checked = true;
               toolStripButton_SetMarker.Checked = false;
               toolStripButton_TrackDraw.Checked = false;
               toolStripButton_ClearEditable.Enabled = true;
               toolStripButton_UniqueNames.Enabled = true;
               break;

            case ProgState.Edit_ConcatTracks:
            case ProgState.Edit_SplitTracks:
               toolStripButton_ViewerMode.Checked = true;
               toolStripButton_SetMarker.Checked = false;
               toolStripButton_TrackDraw.Checked = false;
               toolStripButton_ClearEditable.Enabled = true;
               toolStripButton_UniqueNames.Enabled = true;
               editisactiv = true;
               break;

            case ProgState.Edit_SetNewMarker:
               toolStripButton_ViewerMode.Checked = false;
               toolStripButton_SetMarker.Checked = true;
               toolStripButton_TrackDraw.Checked = false;
               editisactiv = true;
               break;

            case ProgState.Edit_MoveMarker:
               toolStripButton_ViewerMode.Checked = true;
               toolStripButton_SetMarker.Checked = false;
               toolStripButton_TrackDraw.Checked = false;

               toolStripButton_EditCancel.Enabled = true;
               editisactiv = true;
               break;

            case ProgState.Edit_DrawTrack:
               toolStripButton_ViewerMode.Checked = false;
               toolStripButton_SetMarker.Checked = false;
               toolStripButton_TrackDraw.Checked = true;

               toolStripButton_EditEnd.Enabled = true;
               toolStripButton_EditCancel.Enabled = true;
               editisactiv = true;
               break;

            case ProgState.Edit_RemoveTrackpoint:
               toolStripButton_ViewerMode.Checked = true;
               toolStripButton_SetMarker.Checked = false;
               toolStripButton_TrackDraw.Checked = false;

               toolStripButton_EditEnd.Enabled = true;
               toolStripButton_EditCancel.Enabled = true;
               editisactiv = true;
               break;
         }
         editableTracklistControl1.ListBackColor = editisactiv ?
            Color.FromArgb(255, 192, 192) :
            Color.FromArgb(192, 255, 192);
      }

      #endregion

      #region Events vom MapControl (auch Maus-Events !)

      /// <summary>
      /// Änderung des Zooms: DEM-Daten aktivieren oder deaktivieren
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void map_OnZoomChanged(object? sender, EventArgs e) {
         dem.IsActiv = getZoom() >= dem.MinimalZoom;
         zoomControler.Zoom = getZoom();
      }

      /// <summary>
      /// letzte Pos. bei einer Kartenverschiebung (als Quadrat, damit winzige Verschiebungen nicht 
      /// das Setzen eines Punktes verhinden)
      /// <para>(wird nur in <see cref="map_SpecMapMouseEvent"/> verwendet)</para>
      /// </summary>
      Rectangle rectLastMouseMovePosition = new(int.MinValue, int.MinValue, 0, 0);


      ThreadSafeBoolVariable specMapMouseEventInWork = new ThreadSafeBoolVariable(false);

      private async void map_SpecMapMouseEvent(object? sender, MapCtrl.MapMouseEventArgs e) {
         if (gpxWorkbench == null)
            return;

         if (!specMapMouseEventInWork.Value) {
            specMapMouseEventInWork.Value = true;

            switch (e.Eventtype) {
               case MapCtrl.MapMouseEventArgs.EventType.Move:
                  if (e.Button == MouseButtons.Left) {
                     rectLastMouseMovePosition.X = e.Location.X - rectLastMouseMovePosition.Width / 2;
                     rectLastMouseMovePosition.Y = e.Location.Y - rectLastMouseMovePosition.Height / 2;
                  }

                  double ele = Gpx.BaseElement.NOTVALID_DOUBLE;
                  if (getZoom() >= 9) // bei kleinerem Zoom nicht ermitteln/anzeigen
                     ele = gpxWorkbench.GetHeight(e.Location);

                  if (ele != Gpx.BaseElement.NOTVALID_DOUBLE)
                     toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°, {2:F0}m", e.Lon, e.Lat, gpxWorkbench.GetHeight(e.Location));
                  else
                     toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°", e.Lon, e.Lat);

                  if (progIsInAnyEditState) {
                     switch (programState) {
                        case ProgState.Edit_DrawTrack:
                        case ProgState.Edit_SplitTracks:
                        case ProgState.Edit_ConcatTracks:
                        case ProgState.Edit_RemoveTrackpoint:
                           gpxWorkbench.InEditRefresh();  // löst Paint() aus
                           break;

                        case ProgState.Edit_SetNewMarker:
                           gpxWorkbench.InEditRefresh();  // löst Paint() aus
                           break;

                        default:
                           if (gpxWorkbench.MarkerIsInWork)
                              gpxWorkbench.InEditRefresh();  // löst Paint() aus
                           break;
                     }
                  }
                  break;

               case MapCtrl.MapMouseEventArgs.EventType.Leave:       // die Maus verläßt den Bereich der Karte
                  clearHighlightedTracks(highlightedTracks);
                  rectLastMouseMovePosition.X = rectLastMouseMovePosition.Y = int.MinValue;
                  break;

               // Mausklicks in die Karte (DANACH erfolgt die Trackevent-Behandlung)
               case MapCtrl.MapMouseEventArgs.EventType.Click:
                  if (progIsInAnyEditState) {
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
                                    case ProgState.Edit_SetNewMarker:         // beim Marker setzen
                                       ProgCore.EditSetMarker2Position(e.Location, gpxWorkbench, editableTracklistControl1);
                                       e.IsHandled = true;
                                       break;

                                    case ProgState.Edit_DrawTrack:         // beim Trackzeichnen
                                       ProgCore.EditAddTrackpoint(e.Location, gpxWorkbench, editableTracklistControl1);
                                       e.IsHandled = true;
                                       break;

                                    case ProgState.Edit_SplitTracks:       // beim Tracksplitten
                                       ProgCore.EditEndTrackSplit(e.Location, gpxWorkbench, editableTracklistControl1);
                                       e.IsHandled = true;
                                       programState = ProgState.Viewer;
                                       break;

                                    case ProgState.Edit_ConcatTracks:      // beim Trackverbinden
                                       ProgCore.EditEndTrackConcat(highlightedTracks, gpxWorkbench, editableTracklistControl1);
                                       programState = ProgState.Viewer;
                                       break;

                                    case ProgState.Edit_RemoveTrackpoint:  // beim Trackpunkte löschen
                                       if (ProgCore.EditRemoveNearestTrackpoint(e.Location, gpxWorkbench, editableTracklistControl1))
                                          programState = ProgState.Viewer;
                                       e.IsHandled = true;
                                       break;

                                    case ProgState.Set_PicturePosition:
                                       photoEdit.setFotoPosition(e.Lon, e.Lat);
                                       programState = ProgState.Viewer;
                                       break;

                                    default:
                                       if (gpxWorkbench.MarkerIsInWork) {
                                          ProgCore.EditSetMarker2Position(e.Location, gpxWorkbench, editableTracklistControl1);
                                          e.IsHandled = true;
                                          programState = ProgState.Viewer;
                                       }
                                       break;
                                 }
                                 break;

                              case Keys.Shift:                          // Links-Klick + Shift im Edit-Modus
                                 switch (programState) {
                                    case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                       ProgCore.EditRemoveLastPointFromTrack(gpxWorkbench, editableTracklistControl1);
                                       e.IsHandled = true;
                                       break;
                                 }
                                 break;

                              case Keys.Control:                        // Links-Klick + Ctrl im Edit-Modus
                                 switch (programState) {
                                    case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                       ProgCore.EditEndDrawTrack(gpxWorkbench, editableTracklistControl1);
                                       e.IsHandled = true;
                                       break;
                                 }
                                 break;

                              case Keys.Alt:                            // Links-Klick + Alt im Edit-Modus
                                 await showObjectinfoAsync(e.Location);
                                 break;
                           }
                           break;

                        case MouseButtons.Right:      // Achtung: Rechtsklick mit Keys.None ist für Kontextmenü reserviert!
                           break;
                     }
                  } else {    // Normal-Modus
                     switch (e.Button) {
                        case MouseButtons.Left:
                           switch (ModifierKeys) {
                              case Keys.None:                          // Links-Klick im Normal-Modus


                                 break;

                              case Keys.Alt:                           // Links-Klick + Alt im Normal-Modus
                                 await showObjectinfoAsync(e.Location);
                                 break;
                           }
                           break;
                     }
                  }
                  break;
            }

            specMapMouseEventInWork.Value = false;
         }
      }

      void map_SpecMapMarkerEvent(object? sender, MapCtrl.MarkerEventArgs e) {
         switch (e.Eventtype) {
            case MapCtrl.MapMouseEventArgs.EventType.Leave:
               switch (programState) {
                  case ProgState.Edit_SetNewMarker:
                     gpxWorkbench.RefreshCursor();
                     break;
               }
               break;

            case MapCtrl.MapMouseEventArgs.EventType.Click:
               switch (e.Button) {
                  case MouseButtons.Left:
                     if (ModifierKeys == Keys.None)
                        switch (e.Marker.Markertype) {
                           case Marker.MarkerType.Standard:
                           case Marker.MarkerType.EditableStandard:
                              markMarkerAndShowInfo(e.Marker, editableTracklistControl1);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.Foto:
                              ShowFoto4Marker(e.Marker.Waypoint);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.GeoTagging:
                              break;

                           default:
                              throw new Exception("Unknown MarkerType");
                        }
                     break;

                  case MouseButtons.Right:
                     if (ModifierKeys == Keys.None)
                        switch (e.Marker.Markertype) {
                           case Marker.MarkerType.Foto:
                              showFotoMarkerList(e.Location, 1.5F);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.GeoTagging:
                              break;

                           default:
                              markMarkerAndShowContextmenu(e.Marker, e.X, e.Y);
                              e.IsHandled = true;
                              break;
                        }
                     break;
               }
               break;
         }
      }

      void map_SpecMapTrackEvent(object? sender, MapCtrl.TrackEventArgs e) {
         if (e.Track != null)
            switch (e.Eventtype) {
               case MapCtrl.MapMouseEventArgs.EventType.Click:
                  switch (e.Button) {
                     case MouseButtons.Left:
                        ProgCore.TrackMarking(e.Track, readOnlyTracklistControl1, editableTracklistControl1);
                        if (ModifierKeys == Keys.Control)
                           showInfo4Track(e.Track, e.Location);
                        e.IsHandled = true;
                        break;

                     case MouseButtons.Right:
                        if (ModifierKeys == Keys.None) {
                           markTrackAndShowContextmenu(e.Track, e.X, e.Y);
                           e.IsHandled = true;
                        }
                        break;
                  }
                  break;

               case MapCtrl.MapMouseEventArgs.EventType.Enter:
                  add2HighlightedTracks(e.Track, highlightedTracks);
                  break;

               case MapCtrl.MapMouseEventArgs.EventType.Leave:
                  removeFromHighlightedTracks(e.Track, highlightedTracks);
                  break;
            }
      }

      void map_SpecMapDrawOnTopEvent(object? sender, MapCtrl.DrawExtendedEventArgs e) =>
         mapDrawOnTop(e.Graphics, gpxWorkbench);

      private void map_MapTileLoadChange(object? sender, MapCtrl.TileLoadChangeEventArgs e) =>
         showTileLoadInfo(e.Count);

      private void map_MapTrackSearch4PolygonEvent(object? sender, MouseEventArgs e) =>
         trackSearch4Area(mapCtrl, readOnlyTracklistControl1);  // Ende der Eingabe simulieren

      #endregion

      #region Events des ReadOnlyTracklistControl

      void readOnlyTracklistControlEventInit() {
         readOnlyTracklistControl1.SelectGpxEvent += (s, e) => {
            if (e != null)
               ProgCore.ShowMiniReadonlyTrackInfo(e.Gpx.TrackList, readOnlyTracklistControl1);
         };
         readOnlyTracklistControl1.SelectTrackEvent += (s, e) =>
            ProgCore.ShowMiniReadonlyTrackInfo(e?.Track, readOnlyTracklistControl1);
         readOnlyTracklistControl1.ChooseGpxEvent += (s, e) =>
            infoAndEditTrackProps(e.Track,
                                  e.Gpx,
                                  e.Name,
                                  false);
         readOnlyTracklistControl1.ChooseTrackEvent += (s, e) =>
            infoAndEditTrackProps(e.Track,
                                  e.Gpx,
                                  e.Name,
                                  false);
         readOnlyTracklistControl1.LoadinfoEvent += (s, e) => {
            if (InvokeRequired) {
               //Debug.WriteLine("Invoke: setGpxLoadInfo_Threadsafe(" + text + ")");
               var d = new SafeCallDelegate4String2Void(setGpxLoadInfo);
               try {
                  Invoke(d, [e.Text]);
               } catch { }
            } else {
               //Debug.WriteLine("setGpxLoadInfo_Threadsafe(" + text + ")");
               setGpxLoadInfo(e.Text);
            }
         };
         readOnlyTracklistControl1.ShowTrackEvent += (s, e) =>
               ProgCore.ShowTrack(e.Track,
                                  e.On,
                                  mapCtrl,
                                  readOnlyTracklistControl1,
                                  editableTracklistControl1);
         readOnlyTracklistControl1.ShowAllMarkerEvent += (s, e) =>
               ProgCore.ShowAllMarker4GpxObject(e.Gpx, e.On, mapCtrl, gpxWorkbench, readOnlyTracklistControl1);
         readOnlyTracklistControl1.ShowAllFotoMarkerEvent += (s, e) =>
               ProgCore.ShowAllFotoMarker4GpxObject(e.Gpx, e.On, mapCtrl, gpxWorkbench, readOnlyTracklistControl1);
         readOnlyTracklistControl1.RefreshProgramStateEvent += (s, e) => {
            var de = new SafeCallDelegate4Void2Void(refreshProgramState);
            Invoke(de);

            if (formIsOnClosing) {     // Close() gewünscht
               var d = new SafeCallDelegate4Void2Void(Close);
               if (!IsDisposed)
                  Invoke(d);
            }
         };
         readOnlyTracklistControl1.ShowExceptionEvent += (s, e) => UIHelper.ShowExceptionError(e.Exception);
      }

      #endregion

      #region Events des readonly-Objekte-Kontextmenü

      bool getObjectFromTrackContextMenu(object sender, out Track? track, out GpxData? gpx) {
         gpx = null;
         track = null;

         ContextMenuStrip? cms = GetContextMenuStrip4ContextMenu(sender);
         if (cms != null) {
            if (cms.SourceControl is ReadOnlyGpxControl) {

               return readOnlyTracklistControl1.GetSelectedObject(out gpx, out track);

            } else if (//cms.SourceControl is GMap.NET.WindowsForms.GMapControl &&  // -> Klick in die Karte
                       cms.Tag != null &&
                       cms.Tag is Track) {

               track = cms.Tag as Track;
               return true;

            }
         }

         return false;
      }

      void contextMenuStripReadOnlyTracks_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {

            // Beim Bearbeiten eines Tracks sollte dieser Track nicht gelöscht/bearbeitet werden können.

            Bitmap bmcolor = new(16, 16);
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

      void contextMenuStripReadOnlyTracks_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
         if (sender is ContextMenuStrip cms) {
            NumericUpDownMenuItem? nud = getFirstNumericUpDownMenuItem(cms);
            if (nud != null &&
                nud.Tag != null) {    // Test, ob sich der Wert ev. geändert hat
               if (Convert.ToDecimal(nud.Tag) != nud.NumUpDown.Value) { // speichern der neu eingestellten Linienbreite
                  if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
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
      }

      void toolStripMenuItem_ReadOnlyTrackShow_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
            if (track != null)
               ProgCore.ShowTrack(track,
                         !toolStripMenuItem_ReadOnlyTrackShow.Checked,
                         mapCtrl,
                         readOnlyTracklistControl1,
                         editableTracklistControl1);   // noch nicht geändert
            else if (gpx != null) {
               // alle Tracks der GPX-Datei
               foreach (Track t in gpx.TrackList)
                  ProgCore.ShowTrack(t,
                            !toolStripMenuItem_ReadOnlyTrackShow.Checked,
                            mapCtrl,
                            readOnlyTracklistControl1,
                            editableTracklistControl1);
            }
         }
      }

      void toolStripMenuItem_ReadOnlyTrackShowSlope_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
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

      private async void toolStripMenuItem_ReadOnlyTrackZoom_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
            if (track != null) {
               await ProgCore.ZoomToTracksAsync([track], mapCtrl);
            } else if (gpx != null) {
               await ProgCore.ZoomToTracksAsync(gpx.TrackList, mapCtrl);
               //} else if (tn != null) {
               //   ZoomToTracks(readOnlyTracklistControl1.GetAllTracksFromSubnodes(tn));
            }
         }
      }

      void toolStripMenuItem_ReadOnlyGpxShowMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out GpxData? gpx)) {
            List<GpxData>? gpxlst = null;
            if (gpx != null)
               gpxlst = [gpx];
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (GpxData item in gpxlst) {
                  item.Markers4StandardAreVisible = ischecked;
                  if (item.Waypoints.Count > 0)
                     ProgCore.ShowAllMarker4GpxObject(item, item.Markers4StandardAreVisible, mapCtrl, gpxWorkbench, readOnlyTracklistControl1);
               }
            }
         }
      }

      void toolStripMenuItem_ReadOnlyGpxShowPictureMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out GpxData? gpx)) {
            List<GpxData>? gpxlst = null;
            if (gpx != null)
               gpxlst = [gpx];
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (GpxData item in gpxlst) {
                  item.Markers4PicturesAreVisible = ischecked;
                  if (item.MarkerListPictures.Count > 0)
                     ProgCore.ShowAllFotoMarker4GpxObject(item, item.Markers4PicturesAreVisible, mapCtrl, gpxWorkbench, readOnlyTracklistControl1);
               }
            }
         }
      }

      void toolStripMenuItem_ReadOnlyTrackInfo_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
            List<Track> tracklst = [];
            if (track != null)
               tracklst.Add(track);
            else if (gpx != null)
               tracklst.AddRange(gpx.TrackList);
            //else if (tn != null)
            //   tracklst.AddRange(readOnlyTracklistControl1.GetAllTracksFromSubnodes(tn));

            if (tracklst.Count > 0) {
               string msg = string.Empty;
               foreach (Track t in tracklst) {
                  if (msg.Length > 0) {
                     msg += "/////" + System.Environment.NewLine;
                  }
                  msg += t.GetSimpleStatsText();
               }
               string? caption = gpx != null ?
                                          gpx.GpxFilename :
                                          tracklst[0].VisualName;
               UIHelper.ShowInfoMessage(msg, caption ?? "?");
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackExtInfo_Click(object sender, EventArgs e) =>
         readOnlyTracklistControl1.ChooseActualSelectedObject();

      private void toolStripMenuItem_ReadOnlyTracksHide_Click(object sender, EventArgs e) =>
         readOnlyTracklistControl1.HideAllTracks();

      void toolStripMenuItem_ReadOnlyTrackColor_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
            if (track != null ||
                gpx != null) {
               List<GpxData>? gpxlst = null;

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

               if (GetColor(orgcol, saveWithGarminExtensions, PredefColors, out Color newcol)) {
                  if (track != null) {
                     track.LineColor = newcol;
                     track.UpdateVisualTrack(mapCtrl);
                  } else if (gpx != null) {
                     gpx.TrackColor = newcol;
                     foreach (var t in gpx.TrackList)
                        t.UpdateVisualTrack(mapCtrl);
                  } else {
                     foreach (var g in gpxlst) {
                        g.TrackColor = newcol;
                        foreach (var t in g.TrackList)
                           t.UpdateVisualTrack(mapCtrl);
                     }
                  }
               }
            }
         }
      }

      void toolStripMenuItem_ReadOnlyTrackClone_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track? track, out GpxData? gpx)) {
            if (gpx != null) {      // alle klonen
               for (int i = 0; i < gpx.TrackList.Count; i++)
                  ProgCore.CloneTrack2GpxWorkbench(gpx.TrackList[i], gpxWorkbench, mapCtrl, readOnlyTracklistControl1, editableTracklistControl1);
               for (int i = 0; i < gpx.MarkerList.Count; i++)
                  ProgCore.CloneMarker2GpxWorkbench(gpx.MarkerList[i], mapCtrl, gpxWorkbench, editableTracklistControl1);
            } else
               ProgCore.CloneTrack2GpxWorkbench(track, gpxWorkbench, mapCtrl, readOnlyTracklistControl1, editableTracklistControl1);
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxRemove_Click(object sender, EventArgs e) =>
         readOnlyTracklistControl1.RemoveSelectedObject();

      #endregion

      #region Events des Readonly-Marker-Kontextmenü

      void contextMenuStripReadOnlyMarker_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (sender is ContextMenuStrip cms) {
            Marker? marker = null;

            if (cms.SourceControl is ReadOnlyGpxControl) {

               // z.Z. nicht sinnvoll

            } else if (cms.SourceControl is MapCtrl &&
                       cms.Tag != null &&
                       cms.Tag is Marker) {

               marker = cms.Tag as Marker;

            }

            if (marker != null) {
               switch (marker.Markertype) {
                  case Marker.MarkerType.GeoTagging:
                  case Marker.MarkerType.Foto:
                  case Marker.MarkerType.EditableStandard:
                     e.Cancel = true;
                     return;
               }
            }
         }
      }

      private void toolStripMenuItem_ShowMarkerProperties_Click(object sender, EventArgs e) =>
         toolStripMenuItem_ForWaypoint(sender as ToolStripMenuItem);

      private void toolStripMenuItem_CloneMarker_Click(object sender, EventArgs e) =>
         toolStripMenuItem_ForWaypoint(sender as ToolStripMenuItem);

      #endregion

      #region Events des Track-Kontextmenü (editierbare Tracks)

      /// <summary>
      /// liefert den <see cref="Track"/> vom Kontextmenü
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      static Track? getTrackFromContextMenuStrip(ContextMenuStrip? cms) {
         return cms != null &&
                cms.Tag != null &&
                cms.Tag is Track ? cms.Tag as Track : null;
      }

      void contextMenuStripEditableTracks_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (programState != ProgState.Viewer) {   // Wenn ein Track in Bearbeitung ist gibt es für KEINEN Track ein Kontextmenü!
            e.Cancel = true;
         } else {

            if (sender is ContextMenuStrip cms) {
               Track? track = null;

               if (cms.SourceControl is EditableGpxControl) {

                  cms.Tag = track = editableTracklistControl1.SelectedTrack;

               } else { // vom Map-Control

                  track = cms.Tag as Track;
                  if (track != null)
                     editableTracklistControl1.SelectTrack(track);
               }

               e.Cancel = true;

               if (track != null &&
                   !gpxWorkbench.IsThisTrackInWork(track)) {

                  toolStripMenuItem_EditableTrackDraw.Enabled = track.GpxSegment.Points.Count > 0;
                  toolStripMenuItem_EditableTrackSplit.Enabled = track.GpxSegment.Points.Count > 2;
                  toolStripMenuItem_EditableTrackAppend.Enabled = gpxWorkbench.TrackCount > 1;
                  toolStripMenuItem_EditableTrackPointRemove.Enabled = track.GpxSegment.Points.Count > 0;
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

                  toolStripMenuItem_EditableTrackShow.Checked = track.IsVisible;
                  toolStripMenuItem_EditableTrackShowSlope.Checked = track.IsSlopeVisible;

                  Bitmap bmcolor = new(16, 16);
                  using (Graphics gr = Graphics.FromImage(bmcolor)) {
                     gr.Clear(track.LineColor);
                     gr.Flush();
                  }

                  toolStripMenuItem_EditableTrackColor.Image = bmcolor;

                  NumericUpDownMenuItem? nud = getFirstNumericUpDownMenuItem(cms);
                  if (nud != null)
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

                  ToolStripMenuItem_ShowAllEditableObjects1.Enabled =
                  ToolStripMenuItem_HideAllEditableObjects1.Enabled =
                  ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Enabled =
                        !gpxWorkbench.TrackIsInWork &&
                        !gpxWorkbench.MarkerIsInWork &&
                        (gpxWorkbench.TrackList.Count + gpxWorkbench.MarkerList.Count) > 0;

                  e.Cancel = false;

               }

               ToolStripMenuItem_EditableGroupDelete1.Enabled =
                     !gpxWorkbench.TrackIsInWork &&
                     !gpxWorkbench.MarkerIsInWork &&
                     editableTracklistControl1.IsGroupSelected;
            }
         }
      }

      void contextMenuStripEditableTracks_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
         if (sender is ContextMenuStrip cms) {
            NumericUpDownMenuItem? nud = getFirstNumericUpDownMenuItem(cms);
            if (nud != null &&
                nud.Tag != null) {    // Test, ob sich der Wert ev. geändert hat
               Track? track = getTrackFromContextMenuStrip(cms);
               if (Convert.ToDecimal(nud.Tag) != nud.NumUpDown.Value) { // speichern der neu eingestellten Linienbreite
                  if (track != null) {
                     track.LineWidth = Convert.ToSingle(nud.NumUpDown.Value);
                     gpxWorkbench.DataChanged = true;
                  }
               }
            }
         }
      }

      void toolStripMenuItem_EditableForOneTrack(object sender, EventArgs e) =>
         toolStripMenuItem_EditableForOneTrack(sender as ToolStripMenuItem);

      async void toolStripMenuItem_EditableForOneTrack(ToolStripMenuItem? mi) {
         if (mi != null) {
            Track? track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(mi));
            if (track != null) {
               if (mi == toolStripMenuItem_EditableTrackDraw) {

                  toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
                  programState = ProgState.Edit_DrawTrack;
                  gpxWorkbench.TrackStartEdit(true, track);

               } else if (mi == toolStripMenuItem_EditableTrackSplit) {

                  toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
                  programState = ProgState.Edit_SplitTracks;
                  gpxWorkbench.TrackStartEdit(true, track);

               } else if (mi == toolStripMenuItem_EditableTrackAppend) {

                  toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
                  programState = ProgState.Edit_ConcatTracks;
                  gpxWorkbench.TrackStartEdit(true, track);

               } else if (mi == toolStripMenuItem_EditableTrackPointRemove) {

                  toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
                  programState = ProgState.Edit_RemoveTrackpoint;
                  gpxWorkbench.TrackStartEdit(true, track);

               } else if (mi == toolStripMenuItem_EditableTrackReverse) {

                  toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
                  track.ChangeDirection();
                  track.Refresh();     // falls sichtbar, Anzeige akt.
                  track.GpxDataContainer.GpxDataChanged = true;

               } else if (mi == toolStripMenuItem_EditableTrackClone) {

                  ProgCore.CloneTrack2GpxWorkbench(track, gpxWorkbench, mapCtrl, readOnlyTracklistControl1, editableTracklistControl1);

               } else if (mi == toolStripMenuItem_EditableTrackDelete) {

                  removeWithAsking(track);

               } else if (mi == toolStripMenuItem_EditableTrackShow) {

                  ProgCore.ShowTrack(track,
                            !track.IsVisible,
                            mapCtrl,
                            readOnlyTracklistControl1,
                            editableTracklistControl1);

               } else if (mi == toolStripMenuItem_EditableTrackShowSlope) {

                  track.IsSlopeVisible = !track.IsSlopeVisible;

               } else if (mi == toolStripMenuItem_EditableTrackZoom) {

                  await ProgCore.ZoomToTracksAsync([track], mapCtrl);

               } else if (mi == toolStripMenuItem_EditableTrackInfo) {

                  UIHelper.ShowInfoMessage(track.GetSimpleStatsText(), track.VisualName);

               } else if (mi == toolStripMenuItem_EditableTrackExtInfo && track.GpxDataContainer != null) {

                  infoAndEditTrackProps(track,
                                        track.GpxDataContainer,
                                        track.Trackname,
                                        true);

               } else if (mi == toolStripMenuItem_EditableTrackColor) {

                  if (GetColor(track.LineColor, saveWithGarminExtensions, PredefColors, out Color newcol)) {
                     gpxWorkbench.SetTrackColor(track, newcol);
                     editableTracklistControl1.SetTrackNameAndImage(track, track.VisualName);
                  }

               } else if (mi == ToolStripMenuItem_EditableTrackSimplify) {

                  ProgCore.SimplifyTrack(track, appData, gpxWorkbench);

               }
            }
         }
      }

      #endregion

      #region Events des Marker-Kontextmenü (editierbare Marker)

      /// <summary>
      /// liefert den <see cref="Marker"/> und/oder den <see cref="Gpx.GpxWaypoint"/> vom Kontextmenü
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      static Marker? getMarkerFromContextMenuStrip(ContextMenuStrip? cms) {
         return cms != null &&
                cms.Tag != null &&
                cms.Tag is Marker ? cms.Tag as Marker : null;
      }

      void contextMenuStripMarker_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (sender is ContextMenuStrip cms) {
            Marker? marker = null;
            if (cms.SourceControl is EditableGpxControl) {

               cms.Tag = marker = editableTracklistControl1.SelectedMarker;  // Marker im Kontextmenü merken

            } else if (cms.SourceControl is MapCtrl &&
                       cms.Tag != null &&
                       cms.Tag is Marker) {

               marker = cms.Tag as Marker;

            }

            e.Cancel = true;

            if (programState != ProgState.Viewer)    // dann KEIN Menü
               return;

            if (marker != null) {
               switch (marker.Markertype) {              // dann KEIN Menü
                  case Marker.MarkerType.GeoTagging:
                  case Marker.MarkerType.Foto:
                     e.Cancel = true;
                     return;

                  case Marker.MarkerType.EditableStandard:
                     ToolStripMenuItem_WaypointShow.Enabled = true;
                     ToolStripMenuItem_WaypointShow.Checked = marker.IsVisible;
                     ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Enabled = !gpxWorkbench.MarkerIsInWork;

                     e.Cancel = false;
                     break;
               }
            } else {

               ToolStripMenuItem_ShowAllEditableObjects3.Enabled = true;
               ToolStripMenuItem_HideAllEditableObjects3.Enabled = true;
               ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Enabled = true;
               e.Cancel = false;

            }

            ToolStripMenuItem_EditableGroupDelete3.Enabled =
                  !gpxWorkbench.TrackIsInWork &&
                  !gpxWorkbench.MarkerIsInWork &&
                  editableTracklistControl1.IsGroupSelected;
         }
      }

      async void toolStripMenuItem_ForWaypoint(ToolStripMenuItem? mi) {
         if (mi != null) {
            ContextMenuStrip? cms = GetContextMenuStrip4ContextMenu(mi);
            if (cms != null) {
               Marker? marker = getMarkerFromContextMenuStrip(cms);
               if (marker != null) {
                  if (mi == ToolStripMenuItem_WaypointClone ||
                      mi == toolStripMenuItem_CloneMarker) {

                     ProgCore.CloneMarker2GpxWorkbench(marker, mapCtrl, gpxWorkbench, editableTracklistControl1);

                  } else if (mi == ToolStripMenuItem_WaypointShow) {

                     ProgCore.ShowWorkbenchMarker(marker, !marker.IsVisible, mapCtrl, gpxWorkbench, editableTracklistControl1);

                  } else if (mi == ToolStripMenuItem_WaypointEdit ||
                             mi == toolStripMenuItem_ShowMarkerProperties) {

                     infoAndEditMarkerProps(marker, marker.IsEditable && programState == ProgState.Edit_SetNewMarker);

                  } else if (mi == ToolStripMenuItem_WaypointSet) {

                     if (marker.IsEditable) {
                        programState = ProgState.Edit_MoveMarker;
                        gpxWorkbench.MarkerStartEdit(true, marker);
                     }

                  } else if (mi == ToolStripMenuItem_WaypointDelete) {

                     removeWithAsking(marker);

                  } else if (mi == ToolStripMenuItem_WaypointZoom) {

                     await ProgCore.ZoomToMarkersAsync([marker], mapCtrl);

                  }
               }
            }
         }
      }

      void ToolStripMenuItem_ForOneWaypoint(object sender, EventArgs e) =>
         toolStripMenuItem_ForWaypoint(sender as ToolStripMenuItem);

      #endregion

      #region Events des Kontextmenü contextMenuStripEditableGroupOrNothing

      void contextMenuStripEditableGroupOrNothing_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         string? group = null;
         if (sender is ContextMenuStrip cms && cms.SourceControl is EditableGpxControl) {

            group = editableTracklistControl1.SelectedGroup;

         } else { // vom Map-Control

         }

         e.Cancel = false;

         ToolStripMenuItem_EditableGroupDelete2.Enabled = group != null;

         ToolStripMenuItem_ShowAllEditableObjects2.Enabled =
         ToolStripMenuItem_HideAllEditableObjects2.Enabled =
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Enabled =
               !gpxWorkbench.TrackIsInWork &&
               !gpxWorkbench.MarkerIsInWork &&
               (gpxWorkbench.TrackList.Count + gpxWorkbench.MarkerList.Count) > 0;

         ToolStripMenuItem_EditableGroupDelete2.Enabled =
               !gpxWorkbench.TrackIsInWork &&
               !gpxWorkbench.MarkerIsInWork &&
               editableTracklistControl1.IsGroupSelected;
      }

      #endregion

      #region gemeinsame Events der Editable-Kontextmenüs

      private void ToolStripMenuItem_ShowAllEditableObjects_Click(object sender, EventArgs e) =>
         ProgCore.ShowAllGpxObjectsInWorkbench(true, mapCtrl, gpxWorkbench, readOnlyTracklistControl1, editableTracklistControl1);

      private void ToolStripMenuItem_HideAllEditableObjects_Click(object sender, EventArgs e) =>
         ProgCore.ShowAllGpxObjectsInWorkbench(false, mapCtrl, gpxWorkbench, readOnlyTracklistControl1, editableTracklistControl1);

      private void ToolStripMenuItem_DeleteAllVisibleEditableObjects_Click(object sender, EventArgs e) =>
         deleteAllVisibleGpxObjectsInWorkbench(gpxWorkbench);

      private void ToolStripMenuItem_EditableGroupInsert_Click(object sender, EventArgs e) =>
         insertGroupInEditableTracklistControl(editableTracklistControl1);

      private void ToolStripMenuItem_EditableGroupDelete_Click(object sender, EventArgs e) =>
         editableTracklistControl1.DeleteGroup();

      #endregion

      #region Hilfsfunktionen für die Kontextmenüs

      /// <summary>
      /// liefert das 1. <see cref="NumericUpDownMenuItem"/> des Kontextmenüs
      /// </summary>
      /// <param name="cms"></param>
      /// <returns></returns>
      static NumericUpDownMenuItem? getFirstNumericUpDownMenuItem(ContextMenuStrip cms) {
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
      static ContextMenuStrip? GetContextMenuStrip4ContextMenu(object sender) {
         if (sender is ContextMenuStrip) {

            return sender as ContextMenuStrip;

         } else if (sender is ToolStripMenuItem) {

            if ((sender as ToolStripMenuItem).Owner is ContextMenuStrip) {

               return (sender as ToolStripMenuItem).Owner as ContextMenuStrip;

            } else if ((sender as ToolStripMenuItem).Owner is ToolStripDropDownMenu) {
               if ((sender as ToolStripMenuItem).Owner is ToolStripDropDownMenu tsdm && tsdm.OwnerItem is ToolStripMenuItem)
                  return GetContextMenuStrip4ContextMenu(tsdm.OwnerItem);

            }

         }
         return null;
      }

      #endregion

      #region eine andere TabPage wird aktiviert

      TabPage? lastTabPage = null;

      void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
         TabControl? tc = (TabControl)sender;
         if (tc != null) {
            if (tc.SelectedTab.Equals(tabPageFoto))
               pictureManager1.ActualPicturePath = appData.LastPicturePath;
            else if (lastTabPage != null &&
                     lastTabPage.Equals(tabPageFoto))
               appData.LastPicturePath = pictureManager1.ActualPicturePath;
            lastTabPage = tc.SelectedTab;
         }
      }

      #endregion


      void reloadMap() {
         bool clearpartial = false;
         if (MyMessageBox.Show("Vor dem erneuten Laden erst den Kartencache" + Environment.NewLine + "für den angezeigten Kartenbereich löschen?",
                               "Achtung",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question,
                               MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            clearpartial = true;
         mapCtrl.M_Refresh(true, true, false, clearpartial);
      }

      async Task<bool> action4KeyAsync(Keys keys,
                                       MapCtrl mapCtrl,
                                       GpxWorkbench? gpxWorkbench,
                                       PhotoEdit? photoEdit,
                                       PictureManager pictMng,
                                       EditableGpxControl rwCtrl,
                                       TabControl tabCtrl) {
         bool handled = false;

         bool tabPageFilesIsActive = tabCtrl.SelectedTab.Equals(tabPageFiles);
         bool tabPageEditableIsActive = tabCtrl.SelectedTab.Equals(tabPageEditable);
         bool tabPageLocationIsActive = tabCtrl.SelectedTab.Equals(tabPageLocation);
         bool tabPageSearchIsActive = tabCtrl.SelectedTab.Equals(tabPageSearch);
         bool tabPageFotoIsActive = tabCtrl.SelectedTab.Equals(tabPageFoto);

         switch (keys) {
            case Keys.Escape:
               switch (programState) {
                  case ProgState.Edit_SetNewMarker:
                  case ProgState.Edit_MoveMarker:
                     programState = ProgState.Viewer;
                     handled = true;
                     break;

                  case ProgState.Edit_DrawTrack:
                  case ProgState.Edit_ConcatTracks:
                  case ProgState.Edit_SplitTracks:
                  case ProgState.Edit_RemoveTrackpoint:
                     gpxWorkbench?.TrackEndEdit(true);
                     programState = ProgState.Viewer;
                     handled = true;
                     break;

                  case ProgState.Set_PicturePosition:
                     photoEdit?.removeGeoTaggingMarker(photoEdit.lastFilename4OnShowExtern);
                     programState = ProgState.Viewer;
                     handled = true;
                     break;
               }
               break;

            case Keys.Control | Keys.X:
               toolStripButton_OpenGpxfile_Click(null, EventArgs.Empty);
               handled = true;
               break;

            // aktive Tabseite setzen

            case Keys.Control | Keys.Shift | Keys.X:
               tabCtrl.SelectedTab = tabPageFiles;
               handled = true;
               break;

            case Keys.Control | Keys.Shift | Keys.B:
               tabCtrl.SelectedTab = tabPageEditable;
               handled = true;
               break;

            case Keys.Control | Keys.Shift | Keys.G:
               tabCtrl.SelectedTab = tabPageLocation;
               handled = true;
               break;

            case Keys.Control | Keys.Shift | Keys.S:
               tabCtrl.SelectedTab = tabPageSearch;
               searchControl1.SetFocus2Inputfield();
               handled = true;
               break;

            case Keys.Control | Keys.Shift | Keys.F:
               tabCtrl.SelectedTab = tabPageFoto;
               handled = true;
               break;

            // Map Zoom/Move

            case Keys.Control | Keys.Add:
            case Keys.Control | Keys.Oemplus:
               toolStripButton_ZoomIn_Click(null, EventArgs.Empty);
               handled = true;
               break;

            case Keys.Control | Keys.Subtract:
            case Keys.Control | Keys.OemMinus:
               toolStripButton_ZoomOut_Click(null, EventArgs.Empty);
               handled = true;
               break;

            case Keys.Control | Keys.Left:
               await mapCtrl.M_MoveViewAsync(-.3, 0);
               handled = true;
               break;

            case Keys.Control | Keys.Right:
               await mapCtrl.M_MoveViewAsync(.3, 0);
               handled = true;
               break;

            case Keys.Control | Keys.Up:
               await mapCtrl.M_MoveViewAsync(0, .3);
               handled = true;
               break;

            case Keys.Control | Keys.Down:
               await mapCtrl.M_MoveViewAsync(0, -.3);
               handled = true;
               break;
         }

         if (tabPageEditableIsActive && !handled)
            // ----- Weiterleiten an das EditableTracklistControl
            switch (keys) {
               case Keys.Space:
               case Keys.F2:
                  editableTracklistControl1.SendKeyDown(keys);
                  break;

               case Keys.Delete:
                  if (editableTracklistControl1.SelectedTrack != null)
                     removeWithAsking(editableTracklistControl1.SelectedTrack);
                  else if (editableTracklistControl1.SelectedMarker != null)
                     removeWithAsking(editableTracklistControl1.SelectedMarker);
                  break;
            }

         if (tabPageFotoIsActive && !handled)
            // ----- Weiterleiten an das PictureManager
            switch (keys) {
               case Keys.Control | Keys.Alt | Keys.S:
                  // Einzelbild speichern
                  pictMng.ToolStripMenuItemSave_Click(new object(), EventArgs.Empty);
                  handled = true;
                  break;

               case Keys.Control | Keys.A:
                  pictMng.ToolStripMenuItemShow_Click(new object(), EventArgs.Empty);
                  handled = true;
                  break;

               case Keys.Control | Keys.P:
                  pictMng.ToolStripMenuItemSet_Click(new object(), EventArgs.Empty);
                  handled = true;
                  break;

               case Keys.F2:
                  pictMng.ToolStripMenuItemEditComment_Click(new object(), EventArgs.Empty);
                  handled = true;
                  break;

               case Keys.Control | Keys.D:
                  pictMng.ToolStripMenuItemEditFilename_Click(new object(), EventArgs.Empty);
                  handled = true;
                  break;

               case Keys.Control | Keys.O:
                  await pictMng.OpenPath();
                  handled = true;
                  break;

               case Keys.Control | Keys.S:
                  // alle Bilder speichern
                  pictMng.SavePicture(true);
                  handled = true;
                  break;

               case Keys.F5:
                  await pictMng.Reload();
                  handled = true;
                  break;

               case Keys.Control | Keys.W:
                  pictMng.SwapView();
                  handled = true;
                  break;
            }

         return handled;
      }

      void removeWithAsking(Track? track) {
         if (track != null)
            if (MyMessageBox.Show("Soll der Track" + Environment.NewLine + Environment.NewLine +
                                  "  '" + track.VisualName + "'" + Environment.NewLine + Environment.NewLine +
                                  "wirklich gelöscht werden?",
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               gpxWorkbench.TrackEndEdit(false);   // falls gerade ein Track in Bearbeitung
               programState = ProgState.Viewer;
               gpxWorkbench.TrackRemove(track);
            }
      }

      void removeWithAsking(Marker marker) {
         if (marker != null)
            if (marker.IsEditable &&
                MyMessageBox.Show("Soll der Marker" + Environment.NewLine + Environment.NewLine +
                                  "  '" + marker.Text + "'" + Environment.NewLine + Environment.NewLine +
                                  "wirklich gelöscht werden?",
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes)
               gpxWorkbench.MarkerRemove(marker);
      }

      void showWaitCursor() {
         lastCursor = Cursor;
         Cursor = Cursors.WaitCursor;
      }

      void showLastCursor() => Cursor = lastCursor;

      /// <summary>
      /// vorbereitende Arbeiten beim Starten des Programms
      /// </summary>
      /// <returns></returns>
      async Task progStarting() {
         Assembly a = Assembly.GetExecutingAssembly();
         FileVersionInfo versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);

         // unter "Projekteigenschaften" - "Paket" - "Allgemein" festgelegt

         //		FileDescription	"GpxViewer"	string
         //		Comments	"18.7.2024"	string
         //		LegalCopyright	"Copyright © FSofT 4/2020"	string
         // 	ProductName	"GpxViewer"	string
         //		CompanyName	"FSofT"	string
         Progname = versionInfo.ProductName ?? string.Empty;
         Progversion = versionInfo.Comments ?? string.Empty;

         //Progname = ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product;
         //Progversion = ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion;

         Text = Progname + " " + Progversion;

         base.KeyPreview = true;

         if (!await initAllWithInfoAsync(true))
            return;

         // Größe des Quadrates für Mausevents festgelegt
         rectLastMouseMovePosition = new Rectangle(int.MinValue, int.MinValue, config.MinimalTrackpointDistanceX, config.MinimalTrackpointDistanceY);

         map_OnZoomChanged(null, EventArgs.Empty);

         // Test: new FSofTUtils.Geography.KmlReader().Read("../../gpx/gx.kmz", out List<Color> cols);

#if GARMINDRAWTEST
         garminTest(1000, 1000, 12.36, 12.41, 51.31, 51.34, 16);
         Close();
         return;
#endif

         creatMapMenuManager();

         cursors4Map = new Cursors4Map(mapCtrl.Cursor);

         // Argumente der Kommandozeile jeweils als GPX-Liste einlesen
         string[] args = Environment.GetCommandLineArgs();
         for (int i = 1; i < args.Length; i++) {
            appendStartInfo("lese Datei " + args[i]);
            readOnlyTracklistControl1.AddFile(args[i]);
         }

         editableTracklistControl1.TrackOrderChangedEvent += (s, ea) => gpxWorkbench.TrackChangePositionInList(ea.OldIdx, ea.NewIdx);
         editableTracklistControl1.MarkerOrderChangedEvent += (s, ea) => gpxWorkbench.MarkerChangePositionInList(ea.OldIdx, ea.NewIdx);
         editableTracklistControl1.UpdateVisualTrackEvent += (s, ea) => ea.Track.UpdateVisualTrack(mapCtrl);
         editableTracklistControl1.UpdateVisualMarkerEvent += (s, ea) => ea.Marker.UpdateVisualMarker(mapCtrl);
         editableTracklistControl1.ShowTrackEvent += (s, ea) => {
            mapCtrl.M_ShowTrack(ea.Track,
                                     ea.Visible,
                                     ea.Visible ? ProgCore.NextVisibleTrack(ea.Track,
                                                                   readOnlyTracklistControl1,
                                                                   editableTracklistControl1) : null);
         };
         editableTracklistControl1.ShowMarkerEvent += (s, ea) => {
            if (ea.Marker != null &&
                ea.Marker.IsVisible != ea.Visible)
               mapCtrl.M_ShowMarker(ea.Marker,
                                                ea.Visible,
                                                ea.Visible ? ProgCore.NextVisibleMarker(ea.Marker, gpxWorkbench, readOnlyTracklistControl1) : null);
         };
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
                  contextMenuStripEditableMarker.Show(editableTracklistControl1,
                                                      editableTracklistControl1.PointToClient(MousePosition));
         editableTracklistControl1.ShowContextmenu4GroupEvent += (s, ea) =>
                  contextMenuStripEditableGroupOrNothing.Show(editableTracklistControl1,
                                                              editableTracklistControl1.PointToClient(MousePosition));
         editableTracklistControl1.ShowContextmenu4NothingEvent += (s, ea) =>
                  contextMenuStripEditableGroupOrNothing.Show(editableTracklistControl1,
                                                              editableTracklistControl1.PointToClient(MousePosition));
         editableTracklistControl1.SelectTrackEvent += (s, ea) =>
            ProgCore.ShowMiniEditableTrackInfo(editableTracklistControl1.SelectedTrack, editableTracklistControl1);
         editableTracklistControl1.SelectMarkerEvent += (s, ea) =>
            ProgCore.ShowMiniEditableMarkerInfo(editableTracklistControl1.SelectedMarker, editableTracklistControl1);
         editableTracklistControl1.GpxWorkbench = gpxWorkbench;
         //editableTracklistControl1.Gpx = gpxWorkbench?.Gpx;

         setGpxLoadInfo(string.Empty);

         programState = ProgState.Viewer;

         geoLocationControl1.SetLocationEvent += async (s, ea) =>
            await ProgCore.SetMapLocationAndZoomAsync(getZoom(), ea.Longitude, ea.Latitude, mapCtrl);
         geoLocationControl1.GetLocationEvent += (s, ea) =>
            ProgCore.GetMapLocationAndZoom(out ea.Longitude, out ea.Latitude, mapCtrl);

         locationControl1.SetZoomAndPositionEvent += async (s, ea) =>
            await ProgCore.SetMapLocationAndZoomAsync(ea.Zoom, ea.Longitude, ea.Latitude, mapCtrl);
         locationControl1.GetZoomAndPositionEvent += (s, ea) =>
            ea.Zoom = ProgCore.GetMapLocationAndZoom(out ea.Longitude, out ea.Latitude, mapCtrl);

         searchControl1.GoToPointEvent += Form_GoToEvent;
         searchControl1.GoToAreaEvent += Form_GoToAreaEvent;

         if (gpxWorkbench != null)
            photoEdit = new PhotoEdit(mapCtrl,
                                      readOnlyTracklistControl1,
                                      pictureManager1,
                                      gpxWorkbench,
                                      () => programState = ProgState.Set_PicturePosition);

         pictureManager1.OnDeselectPictures += photoEdit.PictureManagerDeselectPictures;
         pictureManager1.OnShowExtern += photoEdit.PictureManagerShowExtern;
         pictureManager1.OnNeedNewData += photoEdit.PictureManagerNeedNewData;
      }

      /// <summary>
      /// abschließende Arbeiten beim Beenden des Programms
      /// </summary>
      /// <returns></returns>
      bool progClosing() {
         bool cancel = false;
         try {
            appData.LastZoom = getZoom();
            appData.LastLongitude = mapCtrl.M_CenterLon;
            appData.LastLatitude = mapCtrl.M_CenterLat;
            appData.LastMapname = mapCtrl.M_ProviderDefinitions[mapCtrl.M_ActualMapIdx].MapName;

            appData.GpxDataChanged = gpxWorkbench != null && gpxWorkbench.DataChanged;
            appData.VisibleStatusMarkerList = gpxWorkbench.VisibleStatusMarkerList;
            appData.VisibleStatusTrackList = gpxWorkbench.VisibleStatusTrackList;

            appData.Save();
         } catch { }

         gpxWorkbench?.Save(true);

         if (readOnlyTracklistControl1.LoadGpxfilesIsRunning) {
            formIsOnClosing = true;
            readOnlyTracklistControl1.LoadGpxfilesCancel = true;
            cancel = true;
         }

         if (pictureManager1.UnsavedPictures > 0)
            if (MessageBox.Show("Es sind noch nicht alle geänderten Bilder gespeichert (" + pictureManager1.UnsavedPictures + ")." + Environment.NewLine +
                                Environment.NewLine +
                                "Programm trotzdem schließen?",
                                "Achtung",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2) == DialogResult.No)
               cancel = true;

         mapCtrl.M_ZoomChanged -= map_OnZoomChanged;
         mapCtrl.M_Mouse -= map_SpecMapMouseEvent;
         mapCtrl.M_Marker -= map_SpecMapMarkerEvent;
         mapCtrl.M_Track -= map_SpecMapTrackEvent;
         mapCtrl.M_DrawOnTop -= map_SpecMapDrawOnTopEvent;
         mapCtrl.M_TileLoadChange -= map_MapTileLoadChange;
         mapCtrl.M_SetAreaSelectionEndPointEvent -= map_MapTrackSearch4PolygonEvent;

         return cancel;
      }

      /// <summary>
      /// liefert den akt. Zoom threadsicher
      /// </summary>
      /// <returns></returns>
      double getZoom() => ProgCore.GetMapZoomTS(mapCtrl);

      #region Funktionen für Tracks

      /// <summary>
      /// zeigt erweiterte Infos zum Track bzw. zur GPX-Datei an
      /// <para>Der Track kann auch editiert werden, wenn <see cref="Track.IsEditable"/> true ist und der Parameter entsprechend true ist.</para>
      /// </summary>
      /// <param name="track">Track</param>
      /// <param name="gpx">GPX-Datei-(Objekt)</param>
      /// <param name="formcaption">Überschrift für das Formular</param>
      /// <param name="editable4track">true wenn der Track editiert werden kann</param>
      void infoAndEditTrackProps(Track? track, GpxData? gpx, string? formcaption, bool editable4track) {
         FormTrackInfoAndEdit? form = null;
         if (editable4track &&
             track != null &&
             track.IsEditable) {       // dann modale Form, damit immer nur 1 Track im Editiermodus ist

            bool orgtrackisvisible = track.IsVisible;

            Track trackcopy = Track.CreateCopy(track);
            ProgCore.ShowTrack(trackcopy,
                      false,
                      mapCtrl,
                      readOnlyTracklistControl1,
                      editableTracklistControl1);
            trackcopy.IsOnEdit = true;
            ProgCore.ShowTrack(trackcopy,
                      true,
                      mapCtrl,
                      readOnlyTracklistControl1,
                      editableTracklistControl1);

            form = new FormTrackInfoAndEdit(trackcopy, formcaption ?? "?") {
               TrackIsReadOnly = false,
            };
            form.SelectedPoints += FormMain_SelectedPoints;
            if (orgtrackisvisible)
               track.IsVisible = false;            // damit nicht beide Tracks gleichzeitig sichtbar sind
            if (form.ShowDialog() == DialogResult.OK &&
                form.TrackChanged) {               // dann Originaltrack gegen Kopie austauschen

               ProgCore.ShowTrack(trackcopy,
                         false,
                         mapCtrl,
                         readOnlyTracklistControl1,
                         editableTracklistControl1);        // aus der Ansicht wieder entfernen

               trackcopy.LineColor = track.LineColor;
               trackcopy.LineWidth = track.LineWidth;
               trackcopy.IsOnEdit = false;

               int pos = gpxWorkbench.TrackIndex(track);
               gpxWorkbench.TrackRemove(track);                                 // Originaltrack entfernen und ...
               Track newtrack = gpxWorkbench.TrackInsertCopy(trackcopy, pos);   // ... neuen Track einfügen und ...
               ProgCore.ShowTrack(newtrack,
                         true,
                         mapCtrl,
                         readOnlyTracklistControl1,
                         editableTracklistControl1);                                           // ... anzeigen

            } else {

               ProgCore.ShowTrack(trackcopy,
                         false,
                         mapCtrl,
                         readOnlyTracklistControl1,
                         editableTracklistControl1);     // aus der Ansicht wieder entfernen
               if (orgtrackisvisible)
                  track.IsVisible = true;       // wieder sichtbar machen

            }

            form.SelectedPoints -= FormMain_SelectedPoints;

            mapCtrl.M_ShowSelectedParts(trackcopy, null);

         } else {                   // nicht-modale Form

            // Ex. schon eine Form für dieses Objekt?
            FormTrackInfoAndEdit? existingform = null;
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
                  form = new FormTrackInfoAndEdit(track, formcaption ?? "?") {
                     TrackIsReadOnly = true,
                  };
               } else if (gpx != null &&
                          gpx.TrackList.Count > 0) {
                  form = new FormTrackInfoAndEdit(gpx, formcaption ?? "?") {
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
      /// zeigt einen Tooltip für die akt. markierten Tracks an
      /// </summary>
      void showToolTip4MarkedTracks() => ProgCore.ShowToolTip4Tracks(highlightedTracks, toolTipRouteInfo, mapCtrl);

      /// <summary>
      /// zeigt Infos für den Track und seinen nächstliegenden Trackpunkt an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="ptclient"></param>
      void showInfo4Track(Track? track, Point ptclient) {
         if (track != null) {
            string info = ProgCore.GetTrackPointInfo(track, ptclient, mapCtrl);
            if (info != string.Empty)
               UIHelper.ShowInfoMessage(info, track.VisualName);
         }
      }

      /// <summary>
      /// akt. Track-Zeichnen beenden (hat sonst keine Auswirkung)
      /// </summary>
      void drawTrackEnd(GpxWorkbench? gpxWorkbench) {
         if (programState == ProgState.Edit_DrawTrack ||
             programState == ProgState.Edit_RemoveTrackpoint) {
            gpxWorkbench?.TrackEndEdit(false);
            programState = ProgState.Viewer;
         }
      }

      /// <summary>
      /// markiert den Track und zeigt das Kontextmenü an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="xlient"></param>
      /// <param name="ylient"></param>
      void markTrackAndShowContextmenu(Track track, int xlient, int ylient) {
         ProgCore.TrackMarking(track, readOnlyTracklistControl1, editableTracklistControl1);
         // Kontextmenü für Route anzeigen
         if (!track.IsEditable) {
            contextMenuStripReadOnlyTracks.Tag = track;
            contextMenuStripEditableTracks.Show(mapCtrl, xlient, ylient);
         } else {
            contextMenuStripEditableTracks.Tag = track;
            contextMenuStripEditableTracks.Show(mapCtrl, xlient, ylient);
         }
      }

      /// <summary>
      /// Karte auf die akt. sichtbaren Tracks des RO-Moduls zoomen
      /// </summary>
      /// <returns></returns>
      async Task zoom4Tracks(ReadOnlyGpxControl roCtrl, GpxWorkbench? gpxWorkbench) {
         List<Track> lst = new(roCtrl.GetVisibleTracks());
         if (gpxWorkbench != null)
            lst.AddRange(gpxWorkbench.VisibleTracks());
         await ProgCore.ZoomToTracksAsync(lst, mapCtrl);
      }

      /// <summary>
      /// Tracksuche im Kartenbereich starten
      /// </summary>
      void trackSearch4Area(MapCtrl mapCtrl, ReadOnlyGpxControl roCtrl) {
         if (!mapCtrl.M_SelectionAreaIsStarted) {      // dann die Eingabe Auswahl-Rechteck starten
            mapCtrl.M_StartSelectionArea();
         } else {
            toolStripButton_TrackSearch.Checked = false;
            Gpx.GpxBounds? bounds = mapCtrl.M_EndSelectionArea();  // Eingabe Auswahl-Rechteck beenden
            if (bounds != null) {
               showWaitCursor();
               roCtrl.ShowTracks(bounds); // Tracks im ausgewählten Bereich sichtbar machen
               showLastCursor();
            }
         }
      }

      #region Highlighted Tracks (können mehrere sein, z.B. beim "daraufzeigen"; Track.IsMarked ist die Kennung)

      /// <summary>
      /// Track in die Liste der markierten Tracks aufnehmen
      /// </summary>
      /// <param name="track"></param>
      /// <param name="markedTracks"></param>
      void add2HighlightedTracks(Track track, List<Track> markedTracks) {
         if (!track.IsMarked &&
             !markedTracks.Contains(track)) {
            markedTracks.Add(track);
            track.IsMarked = true;
         }
         showToolTip4MarkedTracks();
      }

      /// <summary>
      /// Track aus der Liste der markierten Tracks entfernen
      /// </summary>
      /// <param name="track"></param>
      void removeFromHighlightedTracks(Track track, List<Track> markedTracks) {
         track.IsMarked = false;
         markedTracks.Remove(track);
         showToolTip4MarkedTracks();
      }

      /// <summary>
      /// Markierung aller markierten Tracks entfernen
      /// </summary>
      /// <param name="markedTracks"></param>
      void clearHighlightedTracks(List<Track> markedTracks) {
         foreach (Track track in markedTracks)
            track.IsMarked = false;
         markedTracks.Clear();
         showToolTip4MarkedTracks();
      }

      #endregion

      #endregion

      #region Funktionen für Marker

      /// <summary>
      /// markiert den (editierbaren) Marker und zeigt das Kontextmenü an
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="xlient"></param>
      /// <param name="ylient"></param>
      private void markMarkerAndShowContextmenu(Marker marker,
                                                int xlient,
                                                int ylient) {
         if (marker.IsEditable)  // Marker in der Liste markieren
            editableTracklistControl1.SelectMarker(marker);
         // Kontextmenü für "Nicht-Foto-Marker" anzeigen, wenn 
         ContextMenuStrip cms = marker.IsEditable ? contextMenuStripEditableMarker : contextMenuStripReadOnlyMarker;
         cms.Tag = marker;
         cms.Show(mapCtrl, xlient, ylient);
      }

      /// <summary>
      /// markiert den (editierbaren) Marker und zeigt Infos an
      /// </summary>
      /// <param name="marker"></param>
      void markMarkerAndShowInfo(Marker marker, EditableGpxControl rwCtrl) {
         if (marker.IsEditable) // Marker in der Liste markieren
            rwCtrl.SelectMarker(marker);
         if (MyMessageBox.Show(ProgCore.GetStdMarkerInfo(marker),
                               marker.Waypoint.Name,
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Information,
                               MessageBoxDefaultButton.Button2,
                               null,
                               false,
                               true,
                               false) == DialogResult.Yes) {
            infoAndEditMarkerProps(marker, false);
         }
      }

      /// <summary>
      /// Wenn tatsächlich Eigenschaften verändert werden, wird einer neuer (!) <see cref="Marker"/> mit diesen Eigenschaften erzeugt.
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="editable">veränderbar oder nur lesbar</param>
      void infoAndEditMarkerProps(Marker? marker, bool editable) {
         if (marker != null)
            if (marker.IsEditable &&
                editable) {
               FormMarkerEditing form = new() {
                  Marker = marker,
                  GarminMarkerSymbols = garminMarkerSymbols,
               };
               if (form.ShowDialog() == DialogResult.OK &&
                   form.WaypointChanged) {
                  if (gpxWorkbench.MarkerReplaceWaypoint(marker, marker))
                     editableTracklistControl1.SetMarkerNameAndImage(marker, marker.Text);  // falls sich der Name geändert hat
                  ProgCore.ShowMiniEditableMarkerInfo(marker, editableTracklistControl1);
               }
            } else {                   // nicht-modale Form
               FormMarkerEditing form = new() {
                  Marker = marker,
                  MarkerIsReadOnly = true,
                  GarminMarkerSymbols = garminMarkerSymbols,
               };
               AddOwnedForm(form);
               form.Show(this);
            }
      }

      #endregion

      #region Funktionen für Foto-Marker

      /// <summary>
      /// zeigt eine modale Auswahl-Liste aller Bild-Waypoint in der (engen) Umgebung an
      /// </summary>
      /// <param name="localcenter">Punkt in dessen Umgebung gesucht wird</param>
      /// <param name="deltafactor">Faktor für die Größe des Bereiches (bezogen auf die Markergröße)</param>
      void showFotoMarkerList(Point localcenter, float deltafactor) {
         List<Marker> markerlst = mapCtrl.M_GetPictureMarkersAround(localcenter,
                                                                            (int)Math.Round(deltafactor * VisualMarker.FotoMarker.Picture.Width),
                                                                            (int)Math.Round(deltafactor * VisualMarker.FotoMarker.Picture.Height));
         if (markerlst.Count > 0) {
            FormPictureMarkers form = new(markerlst);
            form.ShowDialog(this);
         }
      }

      /// <summary>
      /// liefert die <see cref="FormPicture"/> für diese Bilddatei (falls sie ex.)
      /// </summary>
      /// <param name="picturefilename"></param>
      /// <returns></returns>
      public FormPicture? GetForm4Picture(string picturefilename) {
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
      public void ShowFoto4Marker(Gpx.GpxWaypoint wp) {
         FormPicture? form = GetForm4Picture(wp.Name);
         if (form == null) { // ex. noch nicht
            form = new FormPicture(wp.Name, string.Format("Lng {0:F6}°, Lat {1:F6}°, {2}", wp.Lon, wp.Lat, wp.Name));
            AddOwnedForm(form);
            form.Show(this);
         } else
            form.Activate();
      }

      #endregion

      #region Farbe auswählen

      /// <summary>
      /// liefert eine Argb-Farbe (oder Color.Empty) über einen Dialog
      /// </summary>
      /// <param name="orgcol">beim Start ausgewählte Farbe</param>
      /// <param name="withgarmincolors">wenn true, dann 16 Garminfarben als vordefinierte anzeigen</param>
      /// <param name="predefcolors">Array der vordef. Farben</param>
      /// <param name="newcol">gewählte Farbe wenn true</param>
      /// <returns>false wenn keine Farbe ausgewählt wurde</returns>
      static public bool GetColor(Color orgcol, bool withgarmincolors, Color[] predefcolors, out Color newcol) {
         Unclassified.UI.SpecColorSelectorDialog dlg = new() {
            SelectedColor = orgcol,
         };

         if (withgarmincolors) {
            Color[] cols = [
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Black],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkRed],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGreen],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkYellow],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkBlue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkMagenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkCyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.LightGray],

               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGray],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Red],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Green],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Yellow],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Blue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Magenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Cyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.White],
            ];
            dlg.HighlightArrayColor(8, true);   // Standard-Trackfarbe
            for (int i = 0; i < cols.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, cols[i]);
            for (int i = cols.Length; i < dlg.ArrayColorsCount; i++)
               dlg.EnableArrayColor(i, false);
         } else {
            for (int i = 0; i < predefcolors.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, predefcolors[i]);
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
         /// Im Programm können Marker gesetzt werden. 
         /// </summary>
         Edit_SetNewMarker,

         /// <summary>
         /// Im Programm wird ein Marker verschoben. 
         /// </summary>
         Edit_MoveMarker,

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
         /// Im Programm können Trackpunkte entfernt werden.
         /// </summary>
         Edit_RemoveTrackpoint,

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
               case ProgState.Edit_SetNewMarker:
               case ProgState.Edit_MoveMarker:
                  gpxWorkbench.MarkerEndEdit(true);
                  break;

               case ProgState.Edit_RemoveTrackpoint:
                  gpxWorkbench.TrackRemoveNextPoint(Point.Empty); // notfalls Abbruch
                  break;

               case ProgState.Edit_DrawTrack:
                  gpxWorkbench.TrackEndEdit(true);
                  break;

               case ProgState.Edit_SplitTracks:
                  gpxWorkbench.TrackEndEdit(Point.Empty, true); // notfalls Abbruch
                  break;

               case ProgState.Edit_ConcatTracks:
                  gpxWorkbench.TrackEndEdit(null, true); // notfalls Abbruch
                  break;
            }
            mapCtrl.Refresh();


            toolStripButton_OpenGpxfile.Enabled =
            toolStripButton_TrackSearch.Enabled = !readOnlyTracklistControl1.LoadGpxfilesIsRunning;


            // Cursor setzen
            switch (value) {
               case ProgState.Viewer:
                  mapCtrl.Cursor = cursors4Map.Std;
                  break;

               case ProgState.Edit_SetNewMarker:
                  mapCtrl.Cursor = cursors4Map.SetMarker;
                  break;

               case ProgState.Edit_MoveMarker:
                  mapCtrl.Cursor = cursors4Map.SetMarker;
                  break;

               case ProgState.Edit_DrawTrack:
                  mapCtrl.Cursor = cursors4Map.DrawTrack;
                  if (!gpxWorkbench.TrackIsInWork)
                     gpxWorkbench.TrackStartEdit(true, null);
                  break;

               case ProgState.Edit_SplitTracks:
                  mapCtrl.Cursor = cursors4Map.Split;
                  break;

               case ProgState.Edit_RemoveTrackpoint:
                  mapCtrl.Cursor = cursors4Map.Remove;
                  break;

               case ProgState.Edit_ConcatTracks:
                  mapCtrl.Cursor = cursors4Map.Concat;
                  break;

               case ProgState.Set_PicturePosition:
                  mapCtrl.Cursor = cursors4Map.Foto;
                  break;
            }
            setEditToolStripButtons4ProgMode(value);

            _programState = value;
         }
      }

      #endregion

      void creatMapMenuManager() {
         if (config != null &&
             appData != null) {
            mapMenuManager = new MapMenuManager(config, appData, ToolStripMenuItemMaps, mapCtrl.M_ProviderDefinitions, providxpaths);
            mapMenuManager.ActivateIdx += async (s, ea) => {
               try {
                  dem.WithHillshade = config.Hillshading(providxpaths[ea.ProviderIdx], -1);
                  await mapCtrl.M_SetActivProviderAsync(ea.ProviderIdx,
                                                             config.HillshadingAlpha(providxpaths[ea.ProviderIdx], -1),
                                                             dem,
                                                             config.Zoom4Displayfactor);
               } catch (Exception ex) {
                  UIHelper.ShowExceptionError(ex);
               }
            };

            string lastmapname = appData.LastMapname;
            int lastmapnameidx = -1;
            for (int i = 0; i < mapCtrl.M_ProviderDefinitions.Count; i++) {
               if (lastmapname == mapCtrl.M_ProviderDefinitions[i].MapName)
                  lastmapnameidx = i;
            }
            if (lastmapnameidx >= 0)
               mapMenuManager.ActualProviderIdx = lastmapnameidx;
            else {
               if (mapCtrl.M_ProviderDefinitions.Count > 0)
                  mapMenuManager.ActualProviderIdx = Math.Max(0, Math.Min(config.StartProvider, mapCtrl.M_ProviderDefinitions.Count - 1));
            }
         }
      }

      /// <summary>
      /// die Konfigurationsdatei des Programms bearbeiten
      /// </summary>
      /// <returns></returns>
      async Task editProgConfig() {
         if (config != null)
            try {
               if (new FormConfig() {
                  Configuration = config,
                  ActualCachePath = mapCtrl.M_CacheLocation,
                  ProviderDefs = mapCtrl.M_ProviderDefinitions,
                  ProvIdxPaths = providxpaths
               }.ShowDialog() == DialogResult.OK) { // neue Konfigurationsdatei geschrieben
                  appData.Save();
                  gpxWorkbench.Save(true);


                  Color[] trackcolor = new Color[gpxWorkbench.TrackCount];
                  double[] trackwidth = new double[gpxWorkbench.TrackCount];
                  for (int i = 0; i < gpxWorkbench.TrackCount; i++) {
                     Track t = gpxWorkbench.TrackList[i];
                     trackcolor[i] = VisualTrack.EditableColor.ToArgb() == t.LineColor.ToArgb() ?  // MS: ... For example, Black and FromArgb(0,0,0) are not considered equal, since Black is a named color and FromArgb(0,0,0) is not.
                                          Color.Transparent :
                                          t.LineColor;
                     trackwidth[i] = VisualTrack.EditableWidth == t.LineWidth ?
                                          0 :
                                          t.LineWidth;
                  }


                  if (!await initAllWithInfoAsync(false))
                     return;

                  for (int i = 0; i < gpxWorkbench.TrackCount; i++) {
                     Track t = gpxWorkbench.TrackList[i];
                     t.LineColor = trackcolor[i] != Color.Transparent ?
                                       trackcolor[i] :
                                       VisualTrack.EditableColor;
                     t.LineWidth = trackwidth[i] != 0 ?
                                       trackwidth[i] :
                                       VisualTrack.EditableWidth;
                     t.UpdateVisualTrack(mapCtrl);
                  }

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

      /// <summary>
      /// akt. angezeigte Karte als Bild in das Clipboard einfügen
      /// </summary>
      void copyMap2Clipboard(MapCtrl mapCtrl) {
         showWaitCursor();
         Image img = mapCtrl.M_GetViewAsImage();
         showLastCursor();
         Clipboard.SetImage(img);
      }

      /// <summary>
      /// akt. angezeigte Karte drucken
      /// </summary>
      void printMap(MapCtrl mapCtrl) {
         if (mapCtrl is null) {
            throw new ArgumentNullException(nameof(mapCtrl));
         }

         try {
            showWaitCursor();
            Image img = mapCtrl.M_GetViewAsImage();
            showLastCursor();
            ProgCore.PrintMap(img);
         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
         }
      }

      ///// <summary>
      ///// ev. den Karten-Cache löschen
      ///// </summary>
      //void clearMapCache(MapControl mapCtrl, int proidx) {
      //   if (mapCtrl is null) {
      //      throw new ArgumentNullException(nameof(mapCtrl));
      //   }

      //   FormClearCache form = new() {
      //      Map = mapControl2,
      //      ProviderIndex = proidx,
      //   };
      //   form.ShowDialog();
      //   if (form.Clear > 0)
      //      mapControl2.MapRefresh(true, true, false);
      //}

      /// <summary>
      /// ev. noch zusätzliche Dinge auf die fertige Karte zeichnen
      /// </summary>
      /// <param name="graphics"></param>
      private void mapDrawOnTop(Graphics graphics, GpxWorkbench? gpxWorkbench) {
         if (progIsInAnyEditState &&
             gpxWorkbench != null) {  // beim Editieren

            if (gpxWorkbench.MarkerIsInWork)
               gpxWorkbench.MarkerDrawDestinationLine(graphics, mapCtrl.M_LastMouseLocation);
            else if (gpxWorkbench.TrackIsInWork)


               switch (programState) {
                  case ProgState.Edit_SetNewMarker:
                     gpxWorkbench.MarkerDrawDestinationLine(graphics, mapCtrl.M_LastMouseLocation);
                     break;

                  case ProgState.Edit_DrawTrack:
                     gpxWorkbench.TrackDrawDestinationLine(graphics, mapCtrl.M_LastMouseLocation);
                     break;

                  case ProgState.Edit_SplitTracks:
                  case ProgState.Edit_RemoveTrackpoint:
                     gpxWorkbench.TrackDrawNextTrackPoint(graphics, mapCtrl.M_LastMouseLocation);
                     break;

                  case ProgState.Edit_ConcatTracks:
                     Track? trackappend = ProgCore.FirstMarkedTrack(gpxWorkbench.TrackInEdit, true, highlightedTracks);
                     if (trackappend != null)
                        gpxWorkbench.TrackDrawConcatLine(graphics, trackappend);
                     break;

               }
         }
      }

      /// <summary>
      /// <see cref="gpxWorkbench"/> "normal" speichern
      /// </summary>
      /// <param name="withdlg">bei true mit Speichern-Dialoganzeige</param>
      /// <param name="multifiles">bei true jeder Track in einzelne Datei</param>
      /// <returns>false, wenn speichern fehlerhaft</returns>
      async Task<bool> saveWorkbenchAsync(bool withdlg = false, bool multifiles = false) {
         await gpxWorkbench.SaveAsync(true);

         // Dateiname vorhanden?
         if (withdlg ||
             string.IsNullOrEmpty(lastSaveFilename)) {
            //saveFileDialogGpx.FileName = string.IsNullOrEmpty(lastSaveFilename) ?
            //                                          "neu.gpx" :
            //                                          lastSaveFilename;
            saveFileDialogGpx.FileName = "TrackEddi-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".gpx";
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
         //IOHelper.SetBusyStatusEvent += showBusyStatus;
         bool ok = await IOHelper.SaveGpx(
                                    this,
                                    gpxWorkbench.Gpx,
                                    lastSaveFilename,
                                    multifiles,
                                    creator,
                                    saveWithGarminExtensions,
                                    gpxWorkbench.GetTrackColors());
         //IOHelper.SetBusyStatusEvent -= showBusyStatus;
         if (ok)
            Text = creator + " - " + Path.GetFileName(lastSaveFilename);

         return ok;
      }

      /// <summary>
      /// GPX-Datei in das RO-Control einfügen
      /// </summary>
      /// <param name="dlg"></param>
      /// <param name="roCtrl"></param>
      void openReadonlyGpxfile(OpenFileDialog dlg, ReadOnlyGpxControl roCtrl) {
         if (dlg.ShowDialog() == DialogResult.OK) {
            showWaitCursor();
            roCtrl.AddFile(dlg.FileName);
            showLastCursor();
         }
      }

      /// <summary>
      /// fügt eine neue Gruppe in das <see cref="EditableGpxControl"/> ein
      /// </summary>
      void insertGroupInEditableTracklistControl(EditableGpxControl rwCtrl) {
         FormTextInput dlg = new() {
            Usercaption = "Gruppenname",
         };
         if (dlg.ShowDialog() == DialogResult.OK &&
             dlg.Usertext.Trim() != string.Empty)
            rwCtrl.InsertGroup(dlg.Usertext.Trim());
      }

      void deleteAllVisibleGpxObjectsInWorkbench(GpxWorkbench? gpxWorkbench) {
         string txt = ProgCore.GetQuestionText4DeleteAllVisibleGpxObjectsInWorkbench(gpxWorkbench);
         if (txt != string.Empty)
            if (MyMessageBox.Show(txt,
                                  "Achtung",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               gpxWorkbench.RemoveVisibleTracks();
               gpxWorkbench.RemoveVisibleMarkers();
            }
      }

      void setGpxLoadInfo(string text) => toolStripStatusLabel_GpxLoad.Text = !string.IsNullOrEmpty(text) ?
                                                                                 text :
                                                                                 string.Empty;

      /// <summary>
      /// falls eine Garmin-Karte angezeigt wird, werden Infos für Garmin-Objekte im Bereich des Client-Punktes ermittelt
      /// sonst für OSM-Objekte
      /// </summary>
      /// <param name="ptclient"></param>
      async Task showObjectinfoAsync(Point ptclient) {
         showWaitCursor();
         (List<GarminImageCreator.SearchObject>?, List<string>?) info = await ProgCore.GetObjectinfoAsync(ptclient, config, mapCtrl);
         showLastCursor();

         if (info.Item1 != null) {
            FormGarminInfo? form = null;
            foreach (Form oform in OwnedForms) {
               if (oform is FormGarminInfo) {
                  form = oform as FormGarminInfo;
                  break;
               }
            }

            if (info.Item1.Count > 0) {
               if (form == null) {
                  form = new FormGarminInfo();
                  AddOwnedForm(form);
                  form.ClearListBox();
                  foreach (var item in info.Item1) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Show(this);
               } else {
                  form.ClearListBox();
                  foreach (var item in info.Item1) {
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
            FormObjectInfo? form = null;
            foreach (Form oform in OwnedForms) {
               if (oform is FormObjectInfo) {
                  form = oform as FormObjectInfo;
                  break;
               }
            }

            if (info.Item2.Count > 0) {
               if (form == null) {
                  form = new FormObjectInfo();
                  AddOwnedForm(form);
                  form.ClearListBox();
                  foreach (var item in info.Item2) {
                     form.InfoList.Items.Add(item);
                  }
                  form.Show(this);
               } else {
                  form.ClearListBox();
                  foreach (var item in info.Item2) {
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

      void showTileLoadInfo(int tilesinwork) {
         // bool complete = /*tilesinwork <= 0 ||*/ Interlocked.Read(ref tileLoadIsRunning) == 0;
         //Debug.WriteLine(">>> tilesinwork=" + tilesinwork + ", tileLoadIsRunning=" + Interlocked.Read(ref tileLoadIsRunning) + ", complete=" + complete);

         toolStripStatusLabel_MapLoad.Text = tilesinwork <= 0 ? "OK" : ("load " + tilesinwork);
         // fkt. bei .NET9 nicht mehr:
         //toolStripStatusLabel_MapLoad.BackColor = complete ?
         //                                             Color.LightGreen :
         //                                             Color.LightSalmon;
         toolStripStatusLabel_MapLoad.ForeColor = tilesinwork <= 0 ?
                                                      Color.Black :
                                                      Color.Red;

         toolStripButton_CancelMapLoading.Enabled = tilesinwork > 0;
#if LOCALDEBUG
         if (tilesinwork <= 0) {
            Debug.WriteLine(":::: Map-LoadTime: " + (DateTime.Now.Subtract(dtLoadTime).TotalSeconds.ToString("F1") + "s ::::"));
         } else {
            dtLoadTime = DateTime.Now;
         }
#endif
      }

      /// <summary>
      /// eine einfache Hilfe anzeigen
      /// </summary>
      void showMiniHelp(Form mainform) {
         string text = @"Kurzhilfe:

- Die Karte wird mit der linken Maustaste verschoben.
- Mit STRG+Cursortaste kann die Karte i.A. ebenfalls verschoben werden
  (falls der Schieberegler für den Zoom nicht den Focus hat).
- Mit dem Scrollrad der Maus wird gezoomt.

- Linksklick auf Track
    - Track markieren
- Linksklick + Strg auf Track
    - Info über diesen Track und den Teilabschnitt bis zum nächstliegenden Trackpunkt
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

         FormInfo? form = null;
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
            form.Show(mainform);
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



      // Funktionen ohne konkreten UI-Bezug

   }
}