//#define GPXONLYLOADONDEMAND          // (zum Testen) GPX-Dateien erst bei Bedarf einlesen
//#define ONLY_PRINT_PREVIEW       // zum Testen der Druckerausgabe 'True'
//#define GARMINDRAWTEST
//#define SHADINGDRAWTEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSofTUtils;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geometry;
using GMap.NET.CoreExt.MapProviders;
using SmallMapControl;
using SmallMapControl.EditHelper;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public partial class FormMain : Form {

      /// <summary>
      /// für den threadübergreifenden Aufruf von Close() und <see cref="RefreshProgramState"/>() nötig (keine Parameter, kein Ergebnis)
      /// </summary>
      private delegate void SafeCallDelegate4Void2Void();

      /// <summary>
      /// für den threadübergreifenden Aufruf von <see cref="setGpxLoadInfo"/>() nötig 
      /// </summary>
      private delegate void SafeCallDelegate4String2Void(string text);


      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      FSofTUtils.Geography.DEM.DemData dem = null;

      /// <summary>
      /// editierbare Gpx-Sammlung
      /// </summary>
      PoorGpxAllExt EditableGpx;

      //long _loadGpxfilesIsRunning = 0;
      ///// <summary>
      ///// Werden gerade Gpx-Daten (asynchron) eingelesen?
      ///// </summary>
      //bool LoadGpxfilesIsRunning {
      //   get {
      //      return Interlocked.Read(ref _loadGpxfilesIsRunning) != 0;
      //   }
      //   set {
      //      Interlocked.Exchange(ref _loadGpxfilesIsRunning, value ? 1 : 0);
      //   }
      //}
      ///// <summary>
      ///// Sollte das Einlesen von Gpx-Daten abgebrochen werden?
      ///// </summary>
      //bool LoadGpxfilesCancel {
      //   get {
      //      return Interlocked.Read(ref _loadGpxfilesIsRunning) > 1;
      //   }
      //   set {
      //      if (value)
      //         Interlocked.Exchange(ref _loadGpxfilesIsRunning, 2);
      //   }
      //}

      long _formIsOnClosing = 0;
      /// <summary>
      /// Soll das Programm gerade beendet werden (FormClosing() wurde abgebrochen)?
      /// </summary>
      bool FormIsOnClosing {
         get {
            return Interlocked.Read(ref _formIsOnClosing) != 0;
         }
         set {
            Interlocked.Exchange(ref _formIsOnClosing, value ? 1 : 0);
         }
      }

      /// <summary>
      /// Hilfsfunktionen für das Editieren von Tracks
      /// </summary>
      EditTrackHelper editTrackHelper;

      /// <summary>
      /// Hilfsfunktionen für das Editieren von Markern
      /// </summary>
      EditMarkerHelper editMarkerHelper;

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

      /// <summary>
      /// Soll eine Speicherung mit den Garmin-Erweiterungen erfolgen? (notwendig z.B. für spez. Markerbilder)
      /// </summary>
      bool SaveWithGarminExtensions {
         get {
            return toolStripButton_SaveWithGarminExt.Checked;
         }
      }

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol> GarminMarkerSymbols;

      /// <summary>
      /// (ev. benutzerdef.) Farben für Auswahldialog
      /// </summary>
      static Color[] predefColors = new Color[] {
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


      public FormMain() {
         try {
            InitializeComponent();
         } catch (Exception ex) {
            ShowExceptionError(ex);
            this.BindingContextChanged += formMain_BindingContextChanged;  // 1. Event nach HandleCreated
         }
      }

      #region Events der Form

      private void formMain_BindingContextChanged(object sender, EventArgs e) {
         Close();
      }

      private void FormMain_Load(object sender, EventArgs e) {
         Assembly a = Assembly.GetExecutingAssembly();
         Progname = ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product;
         Progversion = ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion;

         Text = Progname + " " + Progversion;

         // Test: new FSofTUtils.Geography.KmlReader().Read("../../gpx/gx.kmz", out List<Color> cols);

         try {
            ReadConfigData();
         } catch (Exception ex) {
            ShowExceptionError(ex);
            Close();
            return;
         }

#if GARMINDRAWTEST
         garminTest(1000, 1000, 12.36, 12.41, 51.31, 51.34, 16);
         Close();
         return;
#endif

         EditableGpx = new PoorGpxAllExt {
            GpxFileEditable = true,
            TrackColor = VisualTrack.EditableColor,
            TrackWidth = VisualTrack.EditableWidth,
         };
         EditableGpx.ChangeIsSet += EditableGpx_ChangeIsSet;

         editTrackHelper = mapControl1.MapCreateEditTrackHelper(EditableGpx);
         editTrackHelper.TrackEditShowEvent += editTrackHelper_TrackEditShowEvent;

         editMarkerHelper = mapControl1.MapCreateEditMarkerHelper(EditableGpx);
         editMarkerHelper.RefreshProgramStateEvent += editMarkerHelper_RefreshProgramStateEvent;
         editMarkerHelper.MarkerShouldInsertEvent += editMarkerHelper_MarkerShouldInsertEvent;

         for (int i = 0; i < mapControl1.MapProviderDefinitions.Count; i++)
            toolStripComboBoxMapSource.Items.Add(mapControl1.MapProviderDefinitions[i].MapName);
         if (mapControl1.MapProviderDefinitions.Count > 0)
            toolStripComboBoxMapSource.SelectedIndex = Math.Max(0, Math.Min(config.StartProvider, mapControl1.MapProviderDefinitions.Count - 1));

         mapControl1.MapZoomChangedEvent += mapControl1_MapZoomChangedEvent;
         mapControl1.MapMouseEvent += mapControl1_MapMouseEvent;
         mapControl1.MapMarkerEvent += mapControl1_MapMarkerEvent;
         mapControl1.MapTrackEvent += mapControl1_MapTrackEvent;
         mapControl1.MapPaintEvent += mapControl1_MapPaintEvent;
         mapControl1.MapTileLoadCompleteEvent += mapControl1_MapTileLoadCompleteEvent;
         mapControl1.MapTrackSearch4PolygonEvent += mapControl1_MapTrackSearch4PolygonEvent;

         cursors4Map = new Cursors4Map(mapControl1.MapCursor);

         mapControl1.MapDragButton = MouseButtons.Left;

         // Argumente der Kommandozeile jeweils als GPX-Liste einlesen
         string[] args = Environment.GetCommandLineArgs();
         for (int i = 1; i < args.Length; i++)
            readOnlyTracklistControl1.AddFile(args[i]);

         editableTracklistControl1.Gpx = EditableGpx;
         editableTracklistControl1.TrackOrderChangedEvent += EditableTracklistControl1_TrackOrderChangedEvent;
         editableTracklistControl1.MarkerOrderChangedEvent += EditableTracklistControl1_MarkerOrderChangedEvent;
         editableTracklistControl1.UpdateVisualTrackEvent += EditableTracklistControl1_UpdateVisualTrackEvent;
         editableTracklistControl1.UpdateVisualMarkerEvent += EditableTracklistControl1_UpdateVisualMarkerEvent;
         editableTracklistControl1.ShowTrackEvent += EditableTracklistControl1_ShowTrackEvent;
         editableTracklistControl1.ShowMarkerEvent += EditableTracklistControl1_ShowMarkerEvent;
         editableTracklistControl1.ChooseTrackEvent += EditableTracklistControl1_ChooseTrackEvent;
         editableTracklistControl1.ChooseMarkerEvent += EditableTracklistControl1_ChooseMarkerEvent;
         editableTracklistControl1.ShowContextmenu4TrackEvent += EditableTracklistControl1_ShowContextmenu4TrackEvent;
         editableTracklistControl1.ShowContextmenu4MarkerEvent += EditableTracklistControl1_ShowContextmenu4MarkerEvent;

         setGpxLoadInfo("");

         toolStripButton_ViewerMode_Click(null, null);
      }

      private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
         if (EditableGpx != null &&
             EditableGpx.GpxFileChanged &&
             (EditableGpx.TrackList.Count > 0 ||
              EditableGpx.Waypoints.Count > 0)) {
            if (MessageBox.Show("Geänderte Daten speichern?", "Speichern", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
               if (!SaveEditableGpx()) {
                  e.Cancel = true;
                  return;
               }
            }
         }

         if (readOnlyTracklistControl1.LoadGpxfilesIsRunning) {
            FormIsOnClosing = true;
            readOnlyTracklistControl1.LoadGpxfilesCancel = true;
            e.Cancel = true;
         }

         mapControl1.MapZoomChangedEvent -= mapControl1_MapZoomChangedEvent;
         mapControl1.MapMouseEvent -= mapControl1_MapMouseEvent;
         mapControl1.MapMarkerEvent -= mapControl1_MapMarkerEvent;
         mapControl1.MapTrackEvent -= mapControl1_MapTrackEvent;
         mapControl1.MapPaintEvent -= mapControl1_MapPaintEvent;
         mapControl1.MapTileLoadCompleteEvent -= mapControl1_MapTileLoadCompleteEvent;
         mapControl1.MapTrackSearch4PolygonEvent -= mapControl1_MapTrackSearch4PolygonEvent;

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
      protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
         if (keyData.HasFlag(Keys.Control)) {
            switch (keyData) {
               case (Keys.Left | Keys.Control):
                  mapControl1.MapMoveView(-.3, 0);
                  break;

               case (Keys.Right | Keys.Control):
                  mapControl1.MapMoveView(.3, 0);
                  break;

               case (Keys.Up | Keys.Control):
                  mapControl1.MapMoveView(0, .3);
                  break;

               case (Keys.Down | Keys.Control):
                  mapControl1.MapMoveView(0, -.3);
                  break;
            }
         }
         return base.ProcessCmdKey(ref msg, keyData);
      }

      private void FormMain_SelectedPoints(object sender, FormTrackInfoAndEdit.SelectedPointsEventArgs e) {
         mapControl1.MapShowSelectedParts(e.Track, null);
         mapControl1.MapShowSelectedParts(e.Track, e.PointList);
      }

      #endregion

      #region Events der Toolbar-Buttons des Programms

      private void toolStripButton_OpenGpxfile_Click(object sender, EventArgs e) {
         if (openFileDialogGpx.ShowDialog() == DialogResult.OK)
            readOnlyTracklistControl1.AddFile(openFileDialogGpx.FileName);
      }

      private void toolStripButton_SaveGpxFile_Click(object sender, EventArgs e) {
         SaveEditableGpx();
      }

      private void toolStripButton_SaveGpxFileExt_Click(object sender, EventArgs e) {
         SaveEditableGpx(true);
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
            pdoc.PrintPage += (doc, args) => DocumentPrintPage(args, img);

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
            ShowExceptionError(ex);
         }

      }

      private void toolStripComboBoxMapSource_SelectedIndexChanged(object sender, EventArgs e) {
         try {
            int idx = (sender as ToolStripComboBox).SelectedIndex;
            dem.WithHillshade = config.Hillshading(idx);
            mapControl1.MapSetActivProvider(idx, config.HillshadingAlpha(idx), dem);
         } catch (Exception ex) {
            ShowExceptionError(ex);
         }
      }

      private void toolStripButton_ReloadMap_Click(object sender, EventArgs e) {
         mapControl1.MapRefresh(true, false);
      }

      private void toolStripButton_ClearCache_Click(object sender, EventArgs e) {
         new FormClearCache() {
            Map = mapControl1,
            ProviderIndex = toolStripComboBoxMapSource.SelectedIndex,
         }.ShowDialog();
      }

      private void toolStripButton_TrackZoom_Click(object sender, EventArgs e) {
         ZoomToTracks(readOnlyTracklistControl1.GetVisibleTracks());
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

      private void toolStripButton1_Click(object sender, EventArgs e) {
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
            ProgramState = ProgState.Viewer;
         } else
            toolStripButton_ViewerMode.Checked = true;
      }

      private void toolStripButton_SetMarker_Click(object sender, EventArgs e) {
         if (toolStripButton_SetMarker.Checked) {
            toolStripButton_ViewerMode.Checked =
            toolStripButton_TrackDraw.Checked = false;
            toolStripButton_TrackDrawEnd.Enabled = false;
            ProgramState = ProgState.Edit_SetMarker;
         } else
            toolStripButton_SetMarker.Checked = true;
      }

      private void toolStripButton_TrackDraw_Click(object sender, EventArgs e) {
         if (toolStripButton_TrackDraw.Checked) {
            toolStripButton_SetMarker.Checked =
            toolStripButton_ViewerMode.Checked = false;
            toolStripButton_TrackDrawEnd.Enabled = true;
            ProgramState = ProgState.Edit_DrawTrack;
         } else
            toolStripButton_TrackDraw.Checked = true;
      }

      private void toolStripButton_TrackDrawEnd_Click(object sender, EventArgs e) {
         if (ProgramState == ProgState.Edit_DrawTrack) {
            editTrackHelper.EditEndDraw();
            editTrackHelper.EditStartNew();
         }
      }

      private void toolStripButton_ClearEditable_Click(object sender, EventArgs e) {
         if (EditableGpx.Tracks.Count > 0 || EditableGpx.Waypoints.Count > 0) {

            StringBuilder sb = new StringBuilder();
            if (EditableGpx.Tracks.Count > 1)
               sb.AppendFormat("Sollen die {0} Tracks", EditableGpx.Tracks.Count);
            else if (EditableGpx.Tracks.Count == 1)
               sb.Append("Soll der Tracks");

            if (EditableGpx.Waypoints.Count > 1)
               sb.AppendFormat("{0}die {1} Markierungen",
                               EditableGpx.Tracks.Count > 0 ? " und " : "Sollen ",
                               EditableGpx.Waypoints.Count);
            else if (EditableGpx.Waypoints.Count == 1)
               sb.AppendFormat("{0}die Markierung",
                               EditableGpx.Tracks.Count > 0 ? " und " : "Soll ");

            sb.Append(" wirklich entfernt werden?");
            sb.AppendLine();

            if (EditableGpx.GpxFileChanged) {
               sb.AppendLine("Die Daten wurden noch nicht gespeichert!");
            } else {
               sb.AppendLine("Die Daten wurden schon in der Datei '" + EditableGpx.GpxFilename + "' gespeichert.");
            }

            if (MyMessageBox.Show(sb.ToString(),
                                  "alle Tracks und/oder Markierungen entfernen",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               for (int i = EditableGpx.TrackList.Count - 1; i >= 0; i--) {
                  Track track = EditableGpx.TrackList[i];
                  ShowTrack(track, false);
                  editTrackHelper.Remove(track);
               }

               for (int i = EditableGpx.MarkerList.Count - 1; i >= 0; i--)
                  editMarkerHelper.Remove(EditableGpx.MarkerList[i]);

               EditableGpx.GpxFileChanged = false;
            }
         }
      }

      /// <summary>
      /// bei Bedarf eindeutige Namen für Marker und Tracks erzeugen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void toolStripButton_UniqueNames_Click(object sender, EventArgs e) {
         SortedSet<string> testnames = new SortedSet<string>();

         for (int i = 0; i < EditableGpx.Waypoints.Count; i++) {
            string name = EditableGpx.Waypoints[i].Name;
            int no = 2;
            while (testnames.Contains(name)) {
               name = EditableGpx.Waypoints[i].Name + " (" + no++ + ")";
            }
            if (EditableGpx.Waypoints[i].Name != name) {
               Marker marker = EditableGpx.MarkerList[i];
               marker.Text = name;
               editableTracklistControl1.SetMarkerName(marker, name);
               editMarkerHelper.RefreshOnMap(marker);
               EditableGpx.GpxFileChanged = true;
            }
            testnames.Add(name);
         }

         testnames.Clear();
         for (int i = 0; i < EditableGpx.TrackList.Count; i++) {
            Track track = EditableGpx.TrackList[i];
            string name = track.Trackname;
            int no = 2;
            while (testnames.Contains(name)) {
               name = track.Trackname + " (" + no++ + ")";
            }
            if (track.Trackname != name) {
               track.Trackname = name;
               editableTracklistControl1.SetTrackName(track, name);
               editTrackHelper.Refresh();
               EditableGpx.GpxFileChanged = true;
            }
            testnames.Add(name);
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


      //mapControl1.MapGetPointsForText("leipzig", mapControl1.MapGetActiveProviderIdx());


      #region Events vom MapControl (auch Maus-Events !)

      private void mapControl1_MapZoomChangedEvent(object sender, EventArgs e) {
         toolStripStatusLabel_Zoom.Text = "Zoomstufe " + mapControl1.MapZoom.ToString();
      }

      /// <summary>
      /// letzte Pos. bei einer Kartenverschiebung (als Quadrat, damit winzige Verschiebungen nicht das Setzen eines Punktes verhinden)
      /// </summary>
      Rectangle rectLastMouseMovePosition = new Rectangle(int.MinValue, int.MinValue, 0, 0);

      private void mapControl1_MapMouseEvent(object sender, SmallMapCtrl.MapMouseEventArgs e) {
         Track markedtrack;
         switch (e.Eventtype) {
            case SmallMapCtrl.MapMouseEventArgs.EventType.Move:
               if (e.Button == MouseButtons.Left) {
                  rectLastMouseMovePosition.X = e.Location.X - rectLastMouseMovePosition.Width / 2;
                  rectLastMouseMovePosition.Y = e.Location.Y - rectLastMouseMovePosition.Height / 2;
               }

               double ele = Gpx.BaseElement.NOTVALID_DOUBLE;
               if (mapControl1.MapZoom >= 9) // bei kleinerem Zoom nicht ermitteln/anzeigen
                  ele = editMarkerHelper.GetHeight(e.Location, dem);

               if (ele != Gpx.BaseElement.NOTVALID_DOUBLE)
                  toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°, {2:F0}m", e.Lon, e.Lat, editMarkerHelper.GetHeight(e.Location, dem));
               else
                  toolStripStatusLabel_Pos.Text = string.Format("Lng {0:F6}°, Lat {1:F6}°", e.Lon, e.Lat);

               if (ProgInEditState) {
                  switch (ProgramState) {
                     case ProgState.Edit_DrawTrack:
                     case ProgState.Edit_SplitTracks:
                     case ProgState.Edit_ConcatTracks:
                        editTrackHelper.Refresh(); // löst Paint() aus
                        break;

                     case ProgState.Edit_SetMarker:
                        editMarkerHelper.Refresh(); // löst Paint() aus
                        break;
                  }
               }
               break;

            case SmallMapCtrl.MapMouseEventArgs.EventType.Leave:       // die Maus verläßt den Bereich der Karte
               foreach (Track track in highlightedTrackSegments)
                  track.IsMarked = false;
               highlightedTrackSegments.Clear();
               ShowToolTip4MarkedTracks();
               rectLastMouseMovePosition.X = rectLastMouseMovePosition.Y = int.MinValue;
               break;

            // Mausklicks in die Karte (DANACH erfolgt die Trackevent-Behandlung)
            case SmallMapCtrl.MapMouseEventArgs.EventType.Click:
               if (ProgInEditState) {
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

                              switch (ProgramState) {
                                 case ProgState.Edit_SetMarker:         // beim Marker setzen
                                    editMarkerHelper.EditSetNewPos(e.Location, dem);
                                    editMarkerHelper.EditEnd();
                                    e.IsHandled = true;
                                    break;

                                 case ProgState.Edit_DrawTrack:         // beim Trackzeichnen
                                    editTrackHelper.EditDraw_AppendPoint(e.Location, dem);
                                    e.IsHandled = true;
                                    break;

                                 case ProgState.Edit_SplitTracks:       // beim Tracksplitten
                                    editTrackHelper.EndSplit(e.Location);
                                    e.IsHandled = true;
                                    ProgramState = ProgState.Edit_DrawTrack;
                                    break;

                                 case ProgState.Edit_ConcatTracks:      // beim Trackverbinden
                                    markedtrack = GetFirstMarkedTrack(editTrackHelper.TrackInEdit, true);
                                    if (markedtrack != null) {
                                       editTrackHelper.EndConcat(markedtrack);
                                       e.IsHandled = true;
                                    }
                                    ProgramState = ProgState.Edit_DrawTrack;
                                    break;

                              }
                              break;

                           case Keys.Shift:                          // Links-Klick + Shift im Edit-Modus
                              switch (ProgramState) {
                                 case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                    editTrackHelper.EditDraw_RemoveLastPoint();
                                    e.IsHandled = true;
                                    break;
                              }
                              break;

                           case Keys.Control:                        // Links-Klick + Ctrl im Edit-Modus
                              switch (ProgramState) {
                                 case ProgState.Edit_DrawTrack:            // beim Trackzeichnen
                                    editTrackHelper.EditEndDraw();
                                    e.IsHandled = true;
                                    break;
                              }
                              break;

                           case Keys.Alt:                            // Links-Klick + Alt im Edit-Modus
                              ShowObjectinfo4Garmin(e.Location);
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
                              ShowObjectinfo4Garmin(e.Location);
                              break;
                        }
                        break;
                  }
               }
               break;
         }
      }

      private void mapControl1_MapMarkerEvent(object sender, SmallMapCtrl.MarkerEventArgs e) {
         switch (e.Eventtype) {
            case SmallMapCtrl.MapMouseEventArgs.EventType.Leave:
               switch (ProgramState) {
                  case ProgState.Edit_SetMarker:
                     editMarkerHelper.RefreshCursor();
                     break;
               }
               break;

            case SmallMapCtrl.MapMouseEventArgs.EventType.Click:
               switch (e.Button) {
                  case MouseButtons.Left:
                     if (ModifierKeys == Keys.None) {
                        switch (e.Marker.Markertype) {
                           case Marker.MarkerType.Standard:
                           case Marker.MarkerType.EditableStandard:
                              if (e.Marker.IsEditable) // Marker in der Liste markieren
                                 editableTracklistControl1.SelectMarker(e.Marker);
                              ShowStdMarkerInfo(e.Marker.Waypoint);
                              e.IsHandled = true;
                              break;

                           case Marker.MarkerType.Foto:
                              ShowPicture(e.Marker.Waypoint);
                              e.IsHandled = true;
                              break;

                           default:
                              throw new Exception("Unknown MarkerType");
                        }
                     }
                     break;

                  case MouseButtons.Right:
                     if (ModifierKeys == Keys.None)
                        if (e.Marker.Markertype == Marker.MarkerType.Foto) {  // Rechtsklick auf einen Fotomarker
                           ShowPictureMarkerList(e.Location, 1.5F);
                           e.IsHandled = true;
                        } else {
                           if (e.Marker.IsEditable)  // Marker in der Liste markieren
                              editableTracklistControl1.SelectMarker(e.Marker);
                           // Kontextmenü für "Nicht-Foto-Marker" anzeigen, wenn 
                           contextMenuStripMarker.Tag = e.Marker;
                           mapControl1.MapShowContextMenu(contextMenuStripMarker, e.X, e.Y);
                           e.IsHandled = true;
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

      private void mapControl1_MapTrackEvent(object sender, SmallMapCtrl.TrackEventArgs e) {
         switch (e.Eventtype) {
            case SmallMapCtrl.MapMouseEventArgs.EventType.Click:
               switch (e.Button) {
                  case MouseButtons.Left:
                     trackMarking(e.Track);
                     if (ModifierKeys == Keys.Control) {
                        if (e.Track != null)
                           ShowTrackPointInfo(e.Track, e.Location);
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

            case SmallMapCtrl.MapMouseEventArgs.EventType.Enter:
               if (!e.Track.IsMarked &&
                   !highlightedTrackSegments.Contains(e.Track)) {
                  highlightedTrackSegments.Add(e.Track);
                  e.Track.IsMarked = true;
               }
               ShowToolTip4MarkedTracks();
               break;

            case SmallMapCtrl.MapMouseEventArgs.EventType.Leave:
               e.Track.IsMarked = false;
               highlightedTrackSegments.Remove(e.Track);
               ShowToolTip4MarkedTracks();
               break;
         }
      }

      private void mapControl1_MapPaintEvent(object sender, PaintEventArgs e) {
         if (ProgInEditState) {  // beim Editieren
            switch (ProgramState) {
               case ProgState.Edit_SetMarker:
                  editMarkerHelper.EditDrawDestinationLine(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_DrawTrack:
                  editTrackHelper.DrawDestinationLine(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_SplitTracks:
                  editTrackHelper.DrawSplitPoint(e.Graphics, mapControl1.MapLastMouseLocation);
                  break;

               case ProgState.Edit_ConcatTracks:
                  Track trackappend = GetFirstMarkedTrack(editTrackHelper.TrackInEdit, true);
                  if (trackappend != null)
                     editTrackHelper.DrawConcatLine(e.Graphics, trackappend);
                  break;

            }
         }
      }

      private void mapControl1_MapTileLoadCompleteEvent(object sender, SmallMapCtrl.TileLoadCompleteEventArgs e) {
         toolStripStatusLabel_MapLoad.Text = e.Complete ? "OK" : "load";
         toolStripStatusLabel_MapLoad.BackColor = e.Complete ? Color.LightGreen : Color.LightSalmon;
      }

      private void mapControl1_MapTrackSearch4PolygonEvent(object sender, MouseEventArgs e) {
         toolStripButton_TrackSearch_Click(null, null);  // Ende der Eingabe simulieren
      }

      #endregion

      #region Events des ReadOnlyTracklistControl

      private void readOnlyTracklistControl1_SelectGpxEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         if (e != null)
            ShowMiniTrackInfo(e.Gpx.TrackList);
      }

      private void readOnlyTracklistControl1_SelectTrackEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         ShowMiniTrackInfo(e != null ? e.Track : null);
      }

      private void readOnlyTracklistControl1_ChooseGpxEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         InfoAndEditTrackProps(e.Track,
                               e.Gpx,
                               e.Name,
                               false);
      }

      private void readOnlyTracklistControl1_ChooseTrackEvent(object sender, ReadOnlyTracklistControl.ChooseEventArgs e) {
         InfoAndEditTrackProps(e.Track,
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
         var de = new SafeCallDelegate4Void2Void(RefreshProgramState);
         Invoke(de);

         if (FormIsOnClosing) {     // Close() gewünscht
            var d = new SafeCallDelegate4Void2Void(Close);
            if (!IsDisposed)
               Invoke(d);
         }
      }

      private void readOnlyTracklistControl1_ShowAllFotoMarkerEvent(object sender, ReadOnlyTracklistControl.ShowMarkerEventArgs e) {
         ShowAllFotoMarker4GpxObject(e.Gpx, e.On);
      }

      private void readOnlyTracklistControl1_ShowAllMarkerEvent(object sender, ReadOnlyTracklistControl.ShowMarkerEventArgs e) {
         ShowAllMarker4GpxObject(e.Gpx, e.On);
      }

      private void readOnlyTracklistControl1_ShowExceptionEvent(object sender, ReadOnlyTracklistControl.SendExceptionEventArgs e) {
         ShowExceptionError(e.Exception);
      }

      private void readOnlyTracklistControl1_ShowTrackEvent(object sender, ReadOnlyTracklistControl.ShowTrackEventArgs e) {
         ShowTrack(e.Track, e.On);
      }

      #endregion

      #region Events des EditableTracklistControl
      private void EditableTracklistControl1_ChooseMarkerEvent(object sender, EditableTracklistControl.IdxEventArgs e) {
         InfoAndEditMarkerProps(EditableGpx.MarkerList[e.Idx], true);
      }

      private void EditableTracklistControl1_ChooseTrackEvent(object sender, EditableTracklistControl.IdxEventArgs e) {
         InfoAndEditTrackProps(EditableGpx.TrackList[e.Idx],
                               EditableGpx,
                               EditableGpx.TrackList[e.Idx].VisualName,
                               true);
      }

      private void EditableTracklistControl1_ShowMarkerEvent(object sender, EditableTracklistControl.MarkerEventArgs e) {
         ShowEditableMarker(e.Marker, e.Visible);
      }

      private void EditableTracklistControl1_ShowTrackEvent(object sender, EditableTracklistControl.TrackEventArgs e) {
         ShowTrack(e.Track, e.Visible);
      }

      private void EditableTracklistControl1_UpdateVisualMarkerEvent(object sender, EditableTracklistControl.MarkerEventArgs e) {
         mapControl1.UpdateVisualMarker(e.Marker);
      }

      private void EditableTracklistControl1_UpdateVisualTrackEvent(object sender, EditableTracklistControl.TrackEventArgs e) {
         mapControl1.UpdateVisualTrack(e.Track);
      }

      private void EditableTracklistControl1_MarkerOrderChangedEvent(object sender, EditableTracklistControl.OrderChangedEventArgs e) {
         editMarkerHelper.ChangeOrder(e.OldIdx, e.NewIdx);
      }

      private void EditableTracklistControl1_TrackOrderChangedEvent(object sender, EditableTracklistControl.OrderChangedEventArgs e) {
         editTrackHelper.ChangeOrder(e.OldIdx, e.NewIdx);
      }

      private void EditableTracklistControl1_ShowContextmenu4MarkerEvent(object sender, EditableTracklistControl.MarkerEventArgs e) {
         contextMenuStripMarker.Show(editableTracklistControl1, editableTracklistControl1.PointToClient(MousePosition));
      }

      private void EditableTracklistControl1_ShowContextmenu4TrackEvent(object sender, EditableTracklistControl.TrackEventArgs e) {
         contextMenuStripEditableTracks.Show(editableTracklistControl1, editableTracklistControl1.PointToClient(MousePosition));
      }

      private void editableTracklistControl1_SelectTrackEvent(object sender, EditableTracklistControl.IdxEventArgs e) {
         ShowMiniEditableTrackInfo(editableTracklistControl1.SelectedTrack);
      }

      #endregion

      #region Events des readonly-Objekte-Kontextmenü

      bool getObjectFromTrackContextMenu(object sender, out Track track, out PoorGpxAllExt gpx) {
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
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {

            // Beim Bearbeiten eines Tracks sollte dieser Track nicht gelöscht/bearbeitet werden können.

            Bitmap bmcolor = new Bitmap(16, 16);
            Graphics gr = Graphics.FromImage(bmcolor);

            toolStripMenuItem_ReadOnlyTrackShow.Enabled = true;
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

                  toolStripMenuItem_ReadOnlyGpxShowMarker.Enabled = gpx.Waypoints.Count > 0;
                  toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = gpx.Markers4StandardVisible;

                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Enabled = gpx.MarkerListPictures.Count > 0;
                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Checked = gpx.Markers4PicturesVisible;

                  if (gpx.TrackList.Count == 0) {
                     toolStripMenuItem_ReadOnlyTrackShow.Enabled =
                     toolStripMenuItem_ReadOnlyTrackZoom.Enabled =
                     toolStripMenuItem_ReadOnlyTrackInfo.Enabled =
                     toolStripMenuItem_ReadOnlyTrackExtInfo.Enabled =
                     toolStripMenuItem_ReadOnlyTrackColor.Enabled =
                     numericUpDownMenuItem_ReadOnlyLineThickness.Enabled = false;
                  }

               } else
                  e.Cancel = true;

            } else if (track != null) {

               if (!editTrackHelper.TrackIsInWork(track)) {

                  gr.Clear(track.LineColor);
                  gr.Flush();

                  toolStripMenuItem_ReadOnlyTrackShow.Checked = track.IsVisible;
                  toolStripMenuItem_ReadOnlyTrackZoom.Enabled = track.IsVisible;
                  toolStripMenuItem_ReadOnlyTrackColor.Image = bmcolor;
                  numericUpDownMenuItem_ReadOnlyLineThickness.Tag = numericUpDownMenuItem_ReadOnlyLineThickness.NumUpDown.Value = Convert.ToDecimal(track.LineWidth);  // alten Wert im Tag speichern

                  toolStripMenuItem_ReadOnlyGpxShowMarker.Enabled = false;
                  toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = track.GpxDataContainer.Markers4StandardVisible;

                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Enabled = false;
                  toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Checked = track.GpxDataContainer.Markers4PicturesVisible;

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
               if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
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
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
            if (track != null)
               ShowTrack(track, !toolStripMenuItem_ReadOnlyTrackShow.Checked);   // noch nicht geändert
            else if (gpx != null) {
               // alle Tracks der GPX-Datei
               foreach (Track t in gpx.TrackList)
                  ShowTrack(t, !toolStripMenuItem_ReadOnlyTrackShow.Checked);
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackZoom_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
            if (track != null) {
               ZoomToTracks(new List<Track>() { track });
            } else if (gpx != null) {
               ZoomToTracks(gpx.TrackList);
               //} else if (tn != null) {
               //   ZoomToTracks(readOnlyTracklistControl1.GetAllTracksFromSubnodes(tn));
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxShowMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out PoorGpxAllExt gpx)) {
            List<PoorGpxAllExt> gpxlst = null;
            if (gpx != null)
               gpxlst = new List<PoorGpxAllExt>() { gpx };
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (PoorGpxAllExt item in gpxlst) {
                  item.Markers4StandardVisible = ischecked;
                  if (item.Waypoints.Count > 0)
                     ShowAllMarker4GpxObject(item, item.Markers4StandardVisible);
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyGpxShowPictureMarker_Click(object sender, EventArgs e) {
         bool ischecked = (sender as ToolStripMenuItem).Checked;
         if (getObjectFromTrackContextMenu(sender, out _, out PoorGpxAllExt gpx)) {
            List<PoorGpxAllExt> gpxlst = null;
            if (gpx != null)
               gpxlst = new List<PoorGpxAllExt>() { gpx };
            else
               gpxlst = readOnlyTracklistControl1.GetAllSubGpxContainerFromSelected();

            if (gpxlst != null) {
               foreach (PoorGpxAllExt item in gpxlst) {
                  item.Markers4PicturesVisible = ischecked;
                  if (item.MarkerListPictures.Count > 0)
                     ShowAllFotoMarker4GpxObject(item, item.Markers4PicturesVisible);
               }
            }
         }
      }

      private void toolStripMenuItem_ReadOnlyTrackInfo_Click(object sender, EventArgs e) {
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
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
               ShowInfoMessage(msg,
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
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
            if (track != null ||
                gpx != null) {
               List<PoorGpxAllExt> gpxlst = null;

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

               Color newcol = GetColor(orgcol, SaveWithGarminExtensions);

               if (newcol != Color.Empty) {
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
         if (getObjectFromTrackContextMenu(sender, out Track track, out PoorGpxAllExt gpx)) {
            if (gpx != null) {      // alle klonen
               for (int i = 0; i < gpx.TrackList.Count; i++)
                  CloneTrack2EditableGpx(gpx.TrackList[i]);
               for (int i = 0; i < gpx.MarkerList.Count; i++)
                  CloneMarker2EditableGpx(gpx.MarkerList[i]);
            } else
               CloneTrack2EditableGpx(track);
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
             !editTrackHelper.TrackIsInWork(track)) {
            toolStripMenuItem_EditableTrackDraw.Enabled = track.GpxSegment.Points.Count > 0;
            toolStripMenuItem_EditableTrackSplit.Enabled = track.GpxSegment.Points.Count > 2;
            toolStripMenuItem_EditableTrackAppend.Enabled = EditableGpx.TrackList.Count > 1;
            toolStripMenuItem_EditableTrackReverse.Enabled = track.GpxSegment.Points.Count > 1;
            toolStripMenuItem_EditableTrackClone.Enabled = track.GpxSegment.Points.Count > 1;
            toolStripMenuItem_EditableTrackDelete.Enabled = EditableGpx.TrackList.Count > 0;
            toolStripMenuItem_EditableTrackShow.Enabled = true;
            toolStripMenuItem_EditableTrackZoom.Enabled = track.IsVisible;
            toolStripMenuItem_EditableTrackInfo.Enabled = true;
            toolStripMenuItem_EditableTrackExtInfo.Enabled = true;
            toolStripMenuItem_EditableTrackColor.Enabled = true;

            toolStripMenuItem_EditableTrackShow.Checked = track.IsVisible;

            Bitmap bmcolor = new Bitmap(16, 16);
            Graphics gr = Graphics.FromImage(bmcolor);
            gr.Clear(track.LineColor);
            gr.Flush();

            toolStripMenuItem_EditableTrackColor.Image = bmcolor;

            NumericUpDownMenuItem nud = getFirstNumericUpDownMenuItem(cms);
            nud.Tag = nud.NumUpDown.Value = Convert.ToDecimal(track.LineWidth);  // alten Wert im Tag speichern

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
               if (track != null)
                  track.LineWidth = Convert.ToSingle(nud.NumUpDown.Value);
            }
         }
      }

      private void toolStripMenuItem_EditableTrackDraw_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            ProgramState = ProgState.Edit_DrawTrack;
            editTrackHelper.EditStart(track);
         }
      }

      private void toolStripMenuItem_EditableTrackSplit_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            ProgramState = ProgState.Edit_SplitTracks;
            editTrackHelper.EditStart(track);
         }
      }

      private void toolStripMenuItem_EditableTrackAppend_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            ProgramState = ProgState.Edit_ConcatTracks;
            editTrackHelper.EditStart(track);
         }
      }

      private void toolStripMenuItem_EditableTrackReverse_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            track.ChangeDirection();
            track.Refresh();     // falls sichtbar, Anzeige akt.
            track.GpxDataContainer.GpxFileChanged = true;
         }
      }

      private void toolStripMenuItem_EditableTrackClone_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            CloneTrack2EditableGpx(track);
      }

      private void toolStripMenuItem_EditableTrackDelete_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            toolStripButton_TrackDraw.PerformClick(); // falls gerade nicht aktiv
            editTrackHelper.Remove(track);
         }
      }

      private void toolStripMenuItem_EditableTrackShow_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            ShowTrack(track, !track.IsVisible);
      }

      private void toolStripMenuItem_EditableTrackZoom_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            ZoomToTracks(new List<Track>() { track });
      }

      private void toolStripMenuItem_EditableTrackInfo_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            ShowInfoMessage(track.GetSimpleStatsText(), track.VisualName);
      }

      private void toolStripMenuItem_EditableTrackExtInfo_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null)
            InfoAndEditTrackProps(track,
                                  track.GpxDataContainer,
                                  track.Trackname,
                                  true);
      }

      private void toolStripMenuItem_EditableTrackColor_Click(object sender, EventArgs e) {
         Track track = getTrackFromContextMenuStrip(GetContextMenuStrip4ContextMenu(sender));
         if (track != null) {
            Color orgcol = track.LineColor;
            Color newcol = GetColor(orgcol, SaveWithGarminExtensions);
            if (newcol != Color.Empty)
               track.LineColor = newcol;
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
             !editMarkerHelper.InWork) { // Ein editierbarer Marker soll während eines Move-Vorgangs nicht verändert/gelöscht werden können.
                                         // damit der akt. Marker nicht per Menü bearbeitet werden kann

            ToolStripMenuItem_WaypointClone.Enabled = ProgInEditState;

            ToolStripMenuItem_WaypointEdit.Enabled = true;  // nichteditierbare Marker nur im readonly-Modus
            ToolStripMenuItem_WaypointSet.Enabled =
            ToolStripMenuItem_WaypointDelete.Enabled = marker.IsEditable;

            ToolStripMenuItem_WaypointShow.Enabled = true;
            ToolStripMenuItem_WaypointShow.Checked = marker.IsVisible;

            e.Cancel = false;
         }
      }

      private void ToolStripMenuItem_WaypointClone_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         CloneMarker2EditableGpx(getMarkerFromContextMenuStrip(cms));
      }

      private void ToolStripMenuItem_WaypointShow_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null)
            ShowEditableMarker(marker, !marker.IsVisible);
      }

      private void ToolStripMenuItem_WaypointEdit_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null)
            InfoAndEditMarkerProps(marker, marker.IsEditable);
      }

      private void ToolStripMenuItem_WaypointSet_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null && marker.IsEditable) {
            ProgramState = ProgState.Edit_SetMarker;     // u.a. "MarkerInEdit = null"
            editMarkerHelper.EditStart(marker);
         }
      }

      private void ToolStripMenuItem_WaypointDelete_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null &&
             marker.IsEditable)
            editMarkerHelper.Remove(marker);
      }

      private void ToolStripMenuItem_WaypointZoom_Click(object sender, EventArgs e) {
         ContextMenuStrip cms = GetContextMenuStrip4ContextMenu(sender);
         Marker marker = getMarkerFromContextMenuStrip(cms);
         if (marker != null && marker.IsEditable)
            ZoomToMarkers(new List<Marker>() { marker });
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

      #region Events von EditTrackHelper

      private void editTrackHelper_TrackEditShowEvent(object sender, EditTrackHelper.TrackEventArgs e) {
         ShowMiniEditableTrackInfo(e.Track);
      }

      #endregion

      #region Events von EditMarkerHelper

      private void editMarkerHelper_RefreshProgramStateEvent(object sender, EventArgs e) {
         RefreshProgramState();
      }

      private void editMarkerHelper_MarkerShouldInsertEvent(object sender, EditMarkerHelper.MarkerEventArgs e) {
         FormMarkerEditing form = new FormMarkerEditing() {
            Marker = e.Marker,
            GarminMarkerSymbols = GarminMarkerSymbols,
         };

         string[] names = null;
         int providx = toolStripComboBoxMapSource.SelectedIndex;
         if (0 <= providx && providx < mapControl1.MapProviderDefinitions.Count) {
            if (mapControl1.MapProviderDefinitions[providx].Provider is GarminProvider) { // falls Garminkarte, dann Textvorschläge holen
               List<GarminImageCreator.SearchObject> info = mapControl1.MapGetGarminObjectInfos(mapControl1.MapLonLat2Client(e.Marker.Longitude, e.Marker.Latitude), 5, 5);
               if (info.Count > 0) {
                  names = new string[info.Count];
                  for (int i = 0; i < info.Count; i++)
                     names[i] = !string.IsNullOrEmpty(info[i].Name) ? info[i].Name : info[i].TypeName;
               }
            }
         }
         form.Proposals = names;

         if (form.ShowDialog() == DialogResult.OK) {
            if (string.IsNullOrEmpty(e.Marker.Waypoint.Name)) {
               e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
            }
            editMarkerHelper.InsertCopy(e.Marker, 0);
            ShowEditableMarker(0);
         }
      }

      #endregion

      /// <summary>
      /// die Eigenschaft <see cref="PoorGpxAllExt.GpxFileChanged"/> des <see cref="EditableGpx"/> wurde geändert
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void EditableGpx_ChangeIsSet(object sender, EventArgs e) {
         PoorGpxAllExt gpx = sender as PoorGpxAllExt;

         toolStripButton_ClearEditable.Enabled =
         toolStripButton_SaveGpxFileExt.Enabled = (gpx.TrackList.Count > 0 || gpx.Waypoints.Count > 0);     // "speichern unter" ist immer aktiv, wenn min. 1 Track oder 1 Marker vorhanden ist

         toolStripButton_SaveGpxFile.Enabled = gpx.GpxFileChanged &&                                               // "speichern" ist aktiv wenn Veränderung und 
                                               (gpx.TrackList.Count > 0 || gpx.Waypoints.Count > 0) &&      //       min. 1 Track oder 1 Marker vorhanden ist und
                                               !string.IsNullOrEmpty(EditableGpx.GpxFilename);              //       ein Dateiname geg. ist
      }

      /// <summary>
      /// liest die Datei gpxviewer.xml (im Arbeitsverzeichnis) ein
      /// </summary>
      void ReadConfigData() {
         string progpath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

         config = new Config(Path.Combine(progpath, "gpxviewer.xml"), null);

         try {
            GarminMarkerSymbols = new List<GarminSymbol>();
            string[] garmingroups = config.GetGarminMarkerSymbolGroupnames();
            if (garmingroups != null)
               for (int g = 0; g < garmingroups.Length; g++) {
                  string[] garminnames = config.GetGarminMarkerSymbolnames(g);
                  if (garminnames != null)
                     for (int i = 0; i < garminnames.Length; i++)
                        GarminMarkerSymbols.Add(new GarminSymbol(garminnames[i],
                                                                 garmingroups[g],
                                                                 config.GetGarminMarkerSymboltext(g, i),
                                                                 Path.Combine(progpath, config.GetGarminMarkerSymbolfile(g, i))));
               }
         } catch (Exception ex) {
            throw new Exception("Fehler beim Einlesen der Garminsymbole: " + ex.Message);
         }

         VisualMarker.RegisterExternSymbols(GarminMarkerSymbols);

         // Größe des Quadrates festgelegt
         rectLastMouseMovePosition = new Rectangle(int.MinValue, int.MinValue, config.MinimalTrackpointDistanceX, config.MinimalTrackpointDistanceY);

         MapControl.MapCacheIsActiv = !config.ServerOnly;
         MapControl.MapSetProxy(config.WebProxyName,
                                config.WebProxyPort,
                                config.WebProxyUser,
                                config.WebProxyPassword);

         SetMapLocationAndZoom(config.StartZoom,
                               config.StartLongitude,
                               config.StartLatitude);
         if (!string.IsNullOrEmpty(config.CacheLocation))
            mapControl1.MapCacheLocation = PathHelper.ReplaceEnvironmentVars(config.CacheLocation);

         string[] providernames = config.Provider;

         List<MapProviderDefinition> provdefs = new List<MapProviderDefinition>();

         for (int providx = 0; providx < providernames.Length; providx++) {
            if (providernames[providx] == GarminProvider.Instance.Name)
               provdefs.Add(new GarminProvider.GarminMapDefinitionData(config.MapName(providx),
                                                                       config.DbIdDelta(providx),
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
                                                                       config.GarminSymbolFactor(providx)));

            else if (providernames[providx] == GarminKmzProvider.Instance.Name)
               provdefs.Add(new GarminKmzProvider.KmzMapDefinition(config.MapName(providx),
                                                                   config.DbIdDelta(providx),
                                                                   config.Zoom4Display(providx),
                                                                   config.MinZoom(providx),
                                                                   config.MaxZoom(providx),
                                                                   PathHelper.ReplaceEnvironmentVars(config.GarminKmzFile(providx))));

            else if (providernames[providx] == WMSProvider.Instance.Name)
               provdefs.Add(new WMSProvider.WMSMapDefinition(config.MapName(providx),
                                                             config.DbIdDelta(providx),
                                                             config.Zoom4Display(providx),
                                                             config.MinZoom(providx),
                                                             config.MaxZoom(providx),
                                                             config.WmsLayers(providx),
                                                             config.WmsUrl(providx),
                                                             config.WmsSrs(providx),
                                                             config.WmsVersion(providx),
                                                             config.WmsPictFormat(providx),
                                                             config.WmsExtend(providx)));

            else
               provdefs.Add(new MapProviderDefinition(config.MapName(providx),
                                                      providernames[providx],
                                                      config.Zoom4Display(providx),
                                                      config.MinZoom(providx),
                                                      config.MaxZoom(providx)));
         }

         mapControl1.MapRegisterProviders(providernames, provdefs);

         dem = new FSofTUtils.Geography.DEM.DemData(string.IsNullOrEmpty(config.DemPath) ?
                                                                "" :
                                                                PathHelper.ReplaceEnvironmentVars(config.DemPath),
                                                    config.DemCachesize);
         dem.WithHillshade = true;
         dem.SetNewHillshadingData(config.DemHillshadingAzimut,
                                   config.DemHillshadingAltitude,
                                   config.DemHillshadingScale,
                                   config.DemHillshadingZ);
         dem.GetHeight(config.StartLongitude, config.StartLatitude);  // liest die DEM-Datei ein

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
      }

      void setGpxLoadInfo(string text) {
         toolStripStatusLabel_GpxLoad.Text = !string.IsNullOrEmpty(text) ?
                                                   text :
                                                   "";
      }

      #region Tracks anzeigen oder verbergen

      /// <summary>
      /// fügt den Track (sichtbar) zum Overlay in der richtigen Ebene hinzu oder entfernt ihn (aber nur bei Veränderung des Sichtbarkeitsstatus)
      /// <para>Außerdem wird der Status der zugehörigen Control (Checkbox) angepasst.</para>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      void ShowTrack(Track track, bool visible = true) {
         if (track.IsVisible != visible) {
            mapControl1.MapShowTrack(track,
                                     visible,
                                     visible ? nextVisibleTrack(track) : null);

            // Control-Status anpassen
            if (track.IsEditable) {
               editableTracklistControl1.ShowTrack(track, visible);
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
            return EditableGpx.NextVisibleEditableTrack(track);
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

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      void ShowEditableMarker(int idx, bool visible = true) {
         if (0 <= idx && idx < EditableGpx.MarkerList.Count)
            ShowEditableMarker(EditableGpx.MarkerList[idx], visible);
      }

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="editablemarker"></param>
      /// <param name="visible"></param>
      /// <returns></returns>
      public int ShowEditableMarker(Marker editablemarker, bool visible) {
         if (editablemarker.IsVisible == visible)
            return -1;
         mapControl1.MapShowMarker(editablemarker,
                                   visible,
                                   visible ? nextVisibleMarker(editablemarker) : null);
         editableTracklistControl1.ShowMarker(editablemarker, visible);
         return EditableGpx.MarkerIndex(editablemarker);
      }

      #endregion

      #region Marker anzeigen oder verbergen

      /// <summary>
      /// aller Markers des <see cref="PoorGpxAllExt"/>-Objektes anzeigen oder verbergen entsprechend <see cref="MarkersVisible"/> aus <see cref="PoorGpxAllExt"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      void ShowAllMarker4GpxObject(PoorGpxAllExt gpx, bool visible) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerList.Count; i++) {
               Marker marker = gpx.MarkerList[i];
               mapControl1.MapShowMarker(marker,
                                         visible,
                                         visible ? nextVisibleMarker(marker) : null);
            }
         }
      }

      /// <summary>
      /// aller Fotos des <see cref="PoorGpxAllExt"/>-Objektes anzeigen oder verbergen entsprechend <see cref="PicturesVisible"/> aus <see cref="PoorGpxAllExt"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      void ShowAllFotoMarker4GpxObject(PoorGpxAllExt gpx, bool visible) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerListPictures.Count; i++) {
               Marker marker = gpx.MarkerListPictures[i];
               mapControl1.MapShowMarker(marker,
                                         visible,
                                         visible ? nextVisibleMarker(marker) : null);
            }
         }
      }

      Marker nextVisibleMarker(Marker marker) {
         if (marker.IsEditable)
            return EditableGpx.NextVisibleEditableMarker(marker);
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
      void ZoomToTracks(List<Track> tracklst) {
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
      void ZoomToMarkers(List<Marker> markerlst) {
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
      List<TreeNode> GetMarkedNodesFromTreeView(TreeNodeCollection tnc) {
         List<TreeNode> nodelst = new List<TreeNode>();
         foreach (TreeNode tn in tnc) {
            if (tn.Nodes.Count > 0)
               nodelst.AddRange(GetMarkedNodesFromTreeView(tn.Nodes));
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
      Track CloneTrack2EditableGpx(Track orgtrack) {
         if (orgtrack != null) {
            Track newtrack = editTrackHelper.InsertCopy(orgtrack, 0);
            ShowTrack(newtrack); // sichtbar!
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
      void InfoAndEditTrackProps(Track track, PoorGpxAllExt gpx, string formcaption, bool editable4track) {
         FormTrackInfoAndEdit form = null;
         if (editable4track &&
             track != null &&
             track.IsEditable) {       // dann modale Form, damit immer nur 1 Track im Editiermodus ist

            Track trackcopy = Track.CreateCopy(track);
            ShowTrack(trackcopy, false);
            trackcopy.IsOnEdit = true;
            ShowTrack(trackcopy);

            form = new FormTrackInfoAndEdit(trackcopy, formcaption) {
               TrackIsReadOnly = false,
            };
            form.SelectedPoints += FormMain_SelectedPoints;
            if (form.ShowDialog() == DialogResult.OK &&
                form.TrackChanged) {      // dann Originaltrack gegen Kopie austauschen

               ShowTrack(trackcopy, false);     // aus der Ansicht wieder entfernen

               trackcopy.LineColor = track.LineColor;
               trackcopy.LineWidth = track.LineWidth;
               trackcopy.IsOnEdit = false;

               int pos = EditableGpx.TrackIndex(track);
               editTrackHelper.Remove(track);                                 // Originaltrack entfernen und ...
               Track newtrack = editTrackHelper.InsertCopy(trackcopy, pos);   // ... neuen Track einfügen und ...
               ShowTrack(newtrack);                                           // ... anzeigen

            } else {

               ShowTrack(trackcopy, false);     // aus der Ansicht wieder entfernen

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
      Track GetFirstMarkedTrack(Track thisnot = null, bool onlyeditable = false) {
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
      void ShowTrackPointInfo(Track track, Point ptclient) {
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

            ShowInfoMessage(sb.ToString(), track.VisualName);
         }
      }

      /// <summary>
      /// zeigt einen Tooltip für die akt. markierten Tracks an
      /// </summary>
      void ShowToolTip4MarkedTracks() {
         if (highlightedTrackSegments.Count > 0) {
            string txt = "";
            for (int i = 0; i < highlightedTrackSegments.Count; i++) {
               if (i > 0)
                  txt += Environment.NewLine;
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

      #endregion

      #region Funktionen für Marker

      /// <summary>
      /// Wenn tatsächlich Eigenschaften verändert werden, wird einer neuer (!) <see cref="Marker"/> mit diesen Eigenschaften erzeugt.
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="editable">veränderbar oder nur lesbar</param>
      void InfoAndEditMarkerProps(Marker marker, bool editable) {
         if (marker != null)
            if (marker.IsEditable &&
                editable) {
               FormMarkerEditing form = new FormMarkerEditing() {
                  Marker = marker,
                  GarminMarkerSymbols = GarminMarkerSymbols,
               };
               if (form.ShowDialog() == DialogResult.OK &&
                   form.WaypointChanged) {
                  int idx = marker.GpxDataContainerIndex;
                  if (idx >= 0) {
                     EditableGpx.GpxFileChanged = true;
                     EditableGpx.Waypoints[idx] = marker.Waypoint;
                     editableTracklistControl1.SetMarkerName(marker, marker.Waypoint.Name);  // falls sich der Name geändert hat
                     editMarkerHelper.RefreshOnMap(marker);
                  }
               }
            } else {                   // nicht-modale Form
               FormMarkerEditing form = new FormMarkerEditing() {
                  Marker = marker,
                  MarkerIsReadOnly = true,
                  GarminMarkerSymbols = GarminMarkerSymbols,
               };
               AddOwnedForm(form);
               form.Show(this);
            }
      }

      /// <summary>
      /// erzeugt eine bearbeitbare Kopie des <see cref="Marker"/> und fügt sie in die Liste ein
      /// </summary>
      /// <param name="orgmarker"></param>
      void CloneMarker2EditableGpx(Marker orgmarker) {
         if (orgmarker != null) {
            editMarkerHelper.InsertCopy(orgmarker, 0);
            ShowEditableMarker(0);
         }
      }

      /// <summary>
      /// zeigt die Infos zu einem Standard-Waypoint an
      /// </summary>
      /// <param name="wp"></param>
      void ShowStdMarkerInfo(Gpx.GpxWaypoint wp) {
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
         ShowInfoMessage(sb.ToString(), wp.Name);
      }

      #endregion

      #region Funktionen für Bilder-Marker

      /// <summary>
      /// zeigt eine modale Auswahl-Liste aller Bild-Waypoint in der (engen) Umgebung an
      /// </summary>
      /// <param name="localcenter">Punkt in dessen Umgebung gesucht wird</param>
      /// <param name="deltafactor">Faktor für die Größe des Bereiches (bezogen auf die Markergröße)</param>
      void ShowPictureMarkerList(Point localcenter, float deltafactor) {
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
      void DocumentPrintPage(PrintPageEventArgs e, Image img) {
         e.HasMorePages = false;                         // sicherheitshalber erstmal die Druckerei beenden
         try {

            float fwidth = (float)e.MarginBounds.Width / img.Width;
            float fheight = (float)e.MarginBounds.Height / img.Height;
            float f = Math.Min(fwidth, fheight);
            e.Graphics.DrawImage(img, e.MarginBounds.Left, e.MarginBounds.Top, f * img.Width, f * img.Height);
            e.HasMorePages = false;

         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler beim Drucken", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      #endregion

      #region Farbe auswählen

      /// <summary>
      /// liefert eine Argb-Farbe (oder Color.Empty) über einen Dialog
      /// </summary>
      /// <param name="orgcol">beim Start ausgewählte Farbe</param>
      /// <param name="withgarmincolors">wenn true, dann 16 Garminfarben als vordefinierte anzeigen</param>
      /// <returns></returns>
      Color GetColor(Color orgcol, bool withgarmincolors) {
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
            for (int i = 0; i < predefColors.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, predefColors[i]);
         }

         if (dlg.ShowDialog() == DialogResult.OK)
            return dlg.SelectedColor;
         return Color.Empty;
      }

      #endregion

      #region Programmstatus

      /// <summary>
      /// der akt. Status wird noch einmal gesetzt um bestimmte Korrekturen zu erzwingen
      /// </summary>
      void RefreshProgramState() {
         ProgramState = ProgramState;
      }

      /// <summary>
      /// Ist das Program in irgendeinem Editier-Status?
      /// </summary>
      bool ProgInEditState {
         get {
            return ProgramState != ProgState.Viewer;
         }
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
      };

      ProgState _programState = ProgState.Viewer;
      /// <summary>
      /// akt. Programm-Status
      /// </summary>
      ProgState ProgramState {
         get {
            return _programState;
         }
         set {
            // KEIN Abbruch der Auswertung bei "_programState == value", weil sonst kein Refresh möglich wäre.

            // ev. noch "Aufräumarbeiten" für den bisherigen Status
            switch (_programState) {               // alter Status
               case ProgState.Edit_SetMarker:
                  editMarkerHelper.EditEnd();
                  break;

               case ProgState.Edit_DrawTrack:
                  editTrackHelper.EditEndDraw();
                  break;

               case ProgState.Edit_SplitTracks:
                  editTrackHelper.EndSplit(Point.Empty); // notfalls Abbruch
                  break;

               case ProgState.Edit_ConcatTracks:
                  editTrackHelper.EndConcat(null); // notfalls Abbruch
                  break;
            }
            mapControl1.Refresh();


            toolStripButton_OpenGpxfile.Enabled =
            toolStripButton_TrackSearch.Enabled = !readOnlyTracklistControl1.LoadGpxfilesIsRunning;

            toolStripStatusLabel_EditInfo.Text = "";

            switch (value) {
               case ProgState.Viewer:
                  mapControl1.MapCursor = cursors4Map.Std;
                  break;

               case ProgState.Edit_SetMarker:
                  mapControl1.MapCursor = cursors4Map.SetMarker;
                  break;

               case ProgState.Edit_DrawTrack:
                  mapControl1.MapCursor = cursors4Map.DrawTrack;
                  if (!editTrackHelper.InWork)
                     editTrackHelper.EditStartNew();
                  toolStripButton_TrackDrawEnd.Enabled = true;    // beim Fortsetzen eines Tracks nötigs
                  break;

               case ProgState.Edit_SplitTracks:
                  mapControl1.MapCursor = cursors4Map.Split;
                  break;

               case ProgState.Edit_ConcatTracks:
                  mapControl1.MapCursor = cursors4Map.Concat;
                  break;

            }

            _programState = value;
         }
      }

      #endregion

      /// <summary>
      /// den Zoom und den Mittelpunkt der Karte setzen
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      public void SetMapLocationAndZoom(double zoom, double lon, double lat) {
         mapControl1.MapSetLocationAndZoom(zoom, lon, lat);
      }

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
      /// editierbare Daten speichern
      /// </summary>
      /// <param name="withdlg"></param>
      /// <returns>false, wenn speichern fehlerhaft</returns>
      bool SaveEditableGpx(bool withdlg = false) {
         // Dateiname vorhanden?
         if (withdlg ||
             string.IsNullOrEmpty(EditableGpx.GpxFilename)) {
            saveFileDialogGpx.FileName = string.IsNullOrEmpty(EditableGpx.GpxFilename) ?
                                                      "neu.gpx" :
                                                      EditableGpx.GpxFilename;
            saveFileDialogGpx.DefaultExt = "gpx";
            if (saveFileDialogGpx.ShowDialog() == DialogResult.OK)
               EditableGpx.GpxFilename = saveFileDialogGpx.FileName;
            else
               return false;
         }

         // zunächst Gültigkeit testen (z.B. Markernamen/Tracknamen)
         SortedSet<string> testnames = new SortedSet<string>();
         foreach (var item in EditableGpx.Waypoints) {
            if (!testnames.Add(item.Name)) {
               MessageBox.Show("Eine Markierung mit dem Namen \"" + item.Name + "\" existiert mehrfach.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return false;
            }
         }
         testnames.Clear();
         foreach (var item in EditableGpx.Tracks) {
            if (!testnames.Add(item.Name)) {
               MessageBox.Show("Ein Track mit dem Namen \"" + item.Name + "\" existiert mehrfach.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return false;
            }
         }

         bool ok = true;
         try {
            EditableGpx.Save(EditableGpx.GpxFilename, Progname + " " + Progversion, SaveWithGarminExtensions);
            EditableGpx.GpxFileChanged = false;
            Text = Progname + " " + Progversion + " - " + Path.GetFileName(EditableGpx.GpxFilename);
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ok = false;
         }
         return ok;
      }

      /// <summary>
      /// falls eine Garmin-Karte angezeigt wird, werden Infos für Objekte im Bereich des Client-Punktes ermittelt
      /// </summary>
      /// <param name="ptclient"></param>
      void ShowObjectinfo4Garmin(Point ptclient) {
         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;
         List<GarminImageCreator.SearchObject> info = mapControl1.MapGetGarminObjectInfos(ptclient, 5, 5);
         Cursor = orgCursor;

         FormGarminInfo form = null;
         foreach (Form oform in OwnedForms) {
            if (oform is FormGarminInfo) {
               form = oform as FormGarminInfo;
               break;
            }
         }

         if (info.Count > 0) {
            if (form == null) {
               form = new FormGarminInfo();
               AddOwnedForm(form);
               form.ClearListBox();
               foreach (var item in info) {
                  form.InfoList.Items.Add(item);
               }

               form.Show(this);
            } else {
               form.ClearListBox();
               foreach (var item in info) {
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

      /// <summary>
      /// zeigt einen Info-Text an
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      void ShowInfoMessage(string txt, string caption) {
         FSofTUtils.MyMessageBox.Show(txt, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, null, false, true, false);
      }

      /// <summary>
      /// zeigt einen Fehlertext an (z.B. aus einer Exception)
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      void ShowErrorMessage(string txt, string caption = "Fehler") {
         FSofTUtils.MyMessageBox.Show(txt, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Color.FromArgb(255, 220, 220));
      }

      /// <summary>
      /// nach Möglichkeit "ausführliche" Anzeige einer Exception
      /// </summary>
      /// <param name="ex"></param>
      void ShowExceptionError(Exception ex) {
         StringBuilder sb = new StringBuilder();

         do {

            sb.AppendLine(ex.Message);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ex.StackTrace)) {
               sb.AppendLine();
               sb.AppendLine("StackTrace:");
               sb.AppendLine(ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(ex.Source)) {
               sb.AppendLine();
               sb.AppendLine("Source:");
               sb.AppendLine(ex.Source);
            }

            ex = ex.InnerException;
         } while (ex != null);

         ShowErrorMessage(sb.ToString());
      }

      void ShowMiniTrackInfo(Track track) {
         ShowMiniTrackInfo(track != null ?
                              new Track[] { track } :
                              new Track[] { });
      }

      void ShowMiniTrackInfo(IList<Track> tracks) {
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

      void ShowMiniEditableTrackInfo(Track track) {
         string info = "";
         if (track != null) {
            double len = track.Length();
            info = string.Format("{0} Punkte, {1:F1} km ({2:F0} m)",
                                 track.GpxSegment.Points.Count,
                                 len / 1000,
                                 len);
         }
         toolStripStatusLabel_EditInfo.Text = info;
      }

      private void Form_GoToEvent(object sender, FormSearch.GoToPointEventArgs e) {
         SetMapLocationAndZoom(mapControl1.MapZoom, e.Longitude, e.Latitude);
         if (!string.IsNullOrEmpty(e.Name)) 
            mapControl1.MapShowMarker(editMarkerHelper.InsertCopy(new Marker(new Gpx.GpxWaypoint(e.Longitude, e.Latitude) { Name = e.Name }, 
                                                                             Marker.MarkerType.EditableStandard,
                                                                             "")), 
                                                                  true);
      }

      private void Form_GoToAreaEvent(object sender, FormSearch.GoToAreaEventArgs e) {
         mapControl1.MapZoomToRange(new PointD(e.Left, e.Top),
                                    new PointD(e.Right, e.Bottom));
         if (!string.IsNullOrEmpty(e.Name))
            mapControl1.MapShowMarker(editMarkerHelper.InsertCopy(new Marker(new Gpx.GpxWaypoint(e.Longitude, e.Latitude) { Name = e.Name },
                                                                             Marker.MarkerType.EditableStandard,
                                                                             "")),
                                                                  true);
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

   }
}