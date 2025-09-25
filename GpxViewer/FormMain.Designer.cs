using GpxViewer.PictureEdit;

namespace GpxViewer {
   partial class FormMain {
      /// <summary>
      /// Erforderliche Designervariable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Verwendete Ressourcen bereinigen.
      /// </summary>
      /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Vom Windows Form-Designer generierter Code

      /// <summary>
      /// Erforderliche Methode für die Designerunterstützung.
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent() {
         components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
         panelMap = new Panel();
         trackBarZoom = new TrackBar();
         splitContainer1 = new SplitContainer();
         tabControl1 = new TabControl();
         tabPageFiles = new TabPage();
         readOnlyTracklistControl1 = new ReadOnlyGpxControl();
         contextMenuStripReadOnlyTracks = new ContextMenuStrip(components);
         toolStripMenuItem_ReadOnlyTrackShow = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyTrackShowSlope = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyTrackZoom = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyGpxShowMarker = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyTrackInfo = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyTrackExtInfo = new ToolStripMenuItem();
         toolStripSeparator1 = new ToolStripSeparator();
         toolStripMenuItem_ReadOnlyTracksHide = new ToolStripMenuItem();
         toolStripSeparator10 = new ToolStripSeparator();
         toolStripMenuItem_ReadOnlyTrackColor = new ToolStripMenuItem();
         numericUpDownMenuItem_ReadOnlyLineThickness = new NumericUpDownMenuItem();
         toolStripSeparator11 = new ToolStripSeparator();
         toolStripMenuItem_ReadOnlyTrackClone = new ToolStripMenuItem();
         toolStripMenuItem_ReadOnlyGpxRemove = new ToolStripMenuItem();
         tabPageEditable = new TabPage();
         toolStripContainer2 = new ToolStripContainer();
         editableTracklistControl1 = new EditableGpxControl();
         toolStrip_Edit = new ToolStrip();
         toolStripButton_ViewerMode = new ToolStripButton();
         toolStripButton_SetMarker = new ToolStripButton();
         toolStripButton_TrackDraw = new ToolStripButton();
         toolStripButton_EditEnd = new ToolStripButton();
         toolStripButton_EditCancel = new ToolStripButton();
         toolStripButton_ClearEditable = new ToolStripButton();
         toolStripButton_UniqueNames = new ToolStripButton();
         tabPageSearch = new TabPage();
         searchControl1 = new SearchControl();
         tabPageLocation = new TabPage();
         splitContainer2 = new SplitContainer();
         locationControl1 = new LocationControl();
         geoLocationControl1 = new GeoLocationControl();
         tabPageFoto = new TabPage();
         pictureManager1 = new PictureManager();
         mapCtrl = new SpecialMapCtrl.SpecialMapCtrl();
         contextMenuStripEditableTracks = new ContextMenuStrip(components);
         toolStripMenuItem_EditableTrackDraw = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackSplit = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackAppend = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackPointRemove = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackReverse = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackClone = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackDelete = new ToolStripMenuItem();
         toolStripSeparator15 = new ToolStripSeparator();
         toolStripMenuItem_EditableTrackShow = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackShowSlope = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackZoom = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackInfo = new ToolStripMenuItem();
         toolStripMenuItem_EditableTrackExtInfo = new ToolStripMenuItem();
         toolStripSeparator12 = new ToolStripSeparator();
         toolStripMenuItem_EditableTrackColor = new ToolStripMenuItem();
         numericUpDownMenuItem_EditableLineThickness = new NumericUpDownMenuItem();
         toolStripSeparator4 = new ToolStripSeparator();
         ToolStripMenuItem_EditableTrackSimplify = new ToolStripMenuItem();
         toolStripSeparator2 = new ToolStripSeparator();
         ToolStripMenuItem_EditableGroupInsert1 = new ToolStripMenuItem();
         ToolStripMenuItem_EditableGroupDelete1 = new ToolStripMenuItem();
         toolStripSeparator5 = new ToolStripSeparator();
         ToolStripMenuItem_ShowAllEditableObjects1 = new ToolStripMenuItem();
         ToolStripMenuItem_HideAllEditableObjects1 = new ToolStripMenuItem();
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1 = new ToolStripMenuItem();
         contextMenuStripEditableMarker = new ContextMenuStrip(components);
         ToolStripMenuItem_WaypointZoom = new ToolStripMenuItem();
         ToolStripMenuItem_WaypointShow = new ToolStripMenuItem();
         ToolStripMenuItem_WaypointEdit = new ToolStripMenuItem();
         ToolStripMenuItem_WaypointClone = new ToolStripMenuItem();
         ToolStripMenuItem_WaypointSet = new ToolStripMenuItem();
         ToolStripMenuItem_WaypointDelete = new ToolStripMenuItem();
         xToolStripMenuItem = new ToolStripSeparator();
         ToolStripMenuItem_EditableGroupInsert3 = new ToolStripMenuItem();
         ToolStripMenuItem_EditableGroupDelete3 = new ToolStripMenuItem();
         toolStripSeparator7 = new ToolStripSeparator();
         ToolStripMenuItem_ShowAllEditableObjects3 = new ToolStripMenuItem();
         ToolStripMenuItem_HideAllEditableObjects3 = new ToolStripMenuItem();
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3 = new ToolStripMenuItem();
         toolTipRouteInfo = new ToolTip(components);
         toolStripContainer1 = new ToolStripContainer();
         statusStrip1 = new StatusStrip();
         toolStripStatusLabel_MapLoad = new ToolStripStatusLabel();
         toolStripStatusLabel_Zoom = new ToolStripStatusLabel();
         toolStripStatusLabel_Pos = new ToolStripStatusLabel();
         toolStripStatusLabel_TrackMiniInfo = new ToolStripStatusLabel();
         toolStripStatusLabel_TrackInfo = new ToolStripStatusLabel();
         toolStripStatusLabel_GpxLoad = new ToolStripStatusLabel();
         menuStrip1 = new MenuStrip();
         ToolStripMenuItemMaps = new ToolStripMenuItem();
         toolStrip_Standard = new ToolStrip();
         toolStripButton_Config = new ToolStripButton();
         toolStripSeparator9 = new ToolStripSeparator();
         toolStripButton_CancelMapLoading = new ToolStripButton();
         toolStripButton_ReloadMap = new ToolStripButton();
         toolStripButton_ClearCache = new ToolStripButton();
         toolStripSeparator3 = new ToolStripSeparator();
         toolStripButton_OpenGpxfile = new ToolStripButton();
         toolStripButton_SaveGpxFileExt = new ToolStripButton();
         toolStripButton_SaveGpxFiles = new ToolStripButton();
         toolStripButton_SaveWithGarminExt = new ToolStripButton();
         toolStripButton_CopyMap = new ToolStripButton();
         toolStripButton_PrintMap = new ToolStripButton();
         toolStripSeparator20 = new ToolStripSeparator();
         toolStripButton_ZoomIn = new ToolStripButton();
         toolStripButton_ZoomOut = new ToolStripButton();
         toolStripButton_TrackZoom = new ToolStripButton();
         toolStripSeparator8 = new ToolStripSeparator();
         toolStripButton_TrackSearch = new ToolStripButton();
         toolStripSeparator6 = new ToolStripSeparator();
         toolStripButton_MiniHelp = new ToolStripButton();
         colorDialog1 = new ColorDialog();
         openFileDialogGpx = new OpenFileDialog();
         saveFileDialogGpx = new SaveFileDialog();
         contextMenuStripEditableGroupOrNothing = new ContextMenuStrip(components);
         ToolStripMenuItem_EditableGroupInsert2 = new ToolStripMenuItem();
         ToolStripMenuItem_EditableGroupDelete2 = new ToolStripMenuItem();
         toolStripSeparator17 = new ToolStripSeparator();
         ToolStripMenuItem_ShowAllEditableObjects2 = new ToolStripMenuItem();
         ToolStripMenuItem_HideAllEditableObjects2 = new ToolStripMenuItem();
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2 = new ToolStripMenuItem();
         contextMenuStripReadOnlyMarker = new ContextMenuStrip(components);
         toolStripMenuItem_ShowMarkerProperties = new ToolStripMenuItem();
         toolStripMenuItem_CloneMarker = new ToolStripMenuItem();
         toolStripContainer3 = new ToolStripContainer();
         panelMap.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)trackBarZoom).BeginInit();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         tabControl1.SuspendLayout();
         tabPageFiles.SuspendLayout();
         contextMenuStripReadOnlyTracks.SuspendLayout();
         tabPageEditable.SuspendLayout();
         toolStripContainer2.ContentPanel.SuspendLayout();
         toolStripContainer2.TopToolStripPanel.SuspendLayout();
         toolStripContainer2.SuspendLayout();
         toolStrip_Edit.SuspendLayout();
         tabPageSearch.SuspendLayout();
         tabPageLocation.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
         splitContainer2.Panel1.SuspendLayout();
         splitContainer2.Panel2.SuspendLayout();
         splitContainer2.SuspendLayout();
         tabPageFoto.SuspendLayout();
         contextMenuStripEditableTracks.SuspendLayout();
         contextMenuStripEditableMarker.SuspendLayout();
         toolStripContainer1.BottomToolStripPanel.SuspendLayout();
         toolStripContainer1.ContentPanel.SuspendLayout();
         toolStripContainer1.TopToolStripPanel.SuspendLayout();
         toolStripContainer1.SuspendLayout();
         statusStrip1.SuspendLayout();
         menuStrip1.SuspendLayout();
         toolStrip_Standard.SuspendLayout();
         contextMenuStripEditableGroupOrNothing.SuspendLayout();
         contextMenuStripReadOnlyMarker.SuspendLayout();
         toolStripContainer3.SuspendLayout();
         SuspendLayout();
         // 
         // panelMap
         // 
         panelMap.BorderStyle = BorderStyle.FixedSingle;
         panelMap.Controls.Add(trackBarZoom);
         panelMap.Controls.Add(splitContainer1);
         panelMap.Dock = DockStyle.Fill;
         panelMap.Location = new Point(0, 0);
         panelMap.Margin = new Padding(4, 3, 4, 3);
         panelMap.Name = "panelMap";
         panelMap.Size = new Size(1058, 610);
         panelMap.TabIndex = 2;
         // 
         // trackBarZoom
         // 
         trackBarZoom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
         trackBarZoom.LargeChange = 10;
         trackBarZoom.Location = new Point(1012, -1);
         trackBarZoom.Maximum = 240;
         trackBarZoom.Name = "trackBarZoom";
         trackBarZoom.Orientation = Orientation.Vertical;
         trackBarZoom.Size = new Size(45, 610);
         trackBarZoom.TabIndex = 1;
         trackBarZoom.TickFrequency = 10;
         trackBarZoom.Value = 120;
         // 
         // splitContainer1
         // 
         splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         splitContainer1.Location = new Point(0, 0);
         splitContainer1.Margin = new Padding(4, 3, 4, 3);
         splitContainer1.Name = "splitContainer1";
         // 
         // splitContainer1.Panel1
         // 
         splitContainer1.Panel1.Controls.Add(tabControl1);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.Controls.Add(mapCtrl);
         splitContainer1.Size = new Size(1005, 608);
         splitContainer1.SplitterDistance = 246;
         splitContainer1.SplitterWidth = 5;
         splitContainer1.TabIndex = 0;
         // 
         // tabControl1
         // 
         tabControl1.Controls.Add(tabPageFiles);
         tabControl1.Controls.Add(tabPageEditable);
         tabControl1.Controls.Add(tabPageSearch);
         tabControl1.Controls.Add(tabPageLocation);
         tabControl1.Controls.Add(tabPageFoto);
         tabControl1.Dock = DockStyle.Fill;
         tabControl1.Location = new Point(0, 0);
         tabControl1.Margin = new Padding(4, 3, 4, 3);
         tabControl1.Multiline = true;
         tabControl1.Name = "tabControl1";
         tabControl1.SelectedIndex = 0;
         tabControl1.Size = new Size(246, 608);
         tabControl1.TabIndex = 8;
         tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
         // 
         // tabPageFiles
         // 
         tabPageFiles.BackColor = Color.FromArgb(224, 224, 224);
         tabPageFiles.Controls.Add(readOnlyTracklistControl1);
         tabPageFiles.Location = new Point(4, 44);
         tabPageFiles.Margin = new Padding(4, 3, 4, 3);
         tabPageFiles.Name = "tabPageFiles";
         tabPageFiles.Padding = new Padding(4, 3, 4, 3);
         tabPageFiles.Size = new Size(238, 560);
         tabPageFiles.TabIndex = 0;
         tabPageFiles.Text = "Dateien";
         // 
         // readOnlyTracklistControl1
         // 
         readOnlyTracklistControl1.AllowDrop = true;
         readOnlyTracklistControl1.BorderStyle = BorderStyle.FixedSingle;
         readOnlyTracklistControl1.ContextMenuStrip = contextMenuStripReadOnlyTracks;
         readOnlyTracklistControl1.Dock = DockStyle.Fill;
         readOnlyTracklistControl1.LoadGpxfilesCancel = false;
         readOnlyTracklistControl1.Location = new Point(4, 3);
         readOnlyTracklistControl1.Margin = new Padding(5);
         readOnlyTracklistControl1.Name = "readOnlyTracklistControl1";
         readOnlyTracklistControl1.Size = new Size(230, 554);
         readOnlyTracklistControl1.TabIndex = 8;
         // 
         // contextMenuStripReadOnlyTracks
         // 
         contextMenuStripReadOnlyTracks.ImageScalingSize = new Size(20, 20);
         contextMenuStripReadOnlyTracks.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_ReadOnlyTrackShow, toolStripMenuItem_ReadOnlyTrackShowSlope, toolStripMenuItem_ReadOnlyTrackZoom, toolStripMenuItem_ReadOnlyGpxShowMarker, toolStripMenuItem_ReadOnlyGpxShowPictureMarker, toolStripMenuItem_ReadOnlyTrackInfo, toolStripMenuItem_ReadOnlyTrackExtInfo, toolStripSeparator1, toolStripMenuItem_ReadOnlyTracksHide, toolStripSeparator10, toolStripMenuItem_ReadOnlyTrackColor, numericUpDownMenuItem_ReadOnlyLineThickness, toolStripSeparator11, toolStripMenuItem_ReadOnlyTrackClone, toolStripMenuItem_ReadOnlyGpxRemove });
         contextMenuStripReadOnlyTracks.Name = "contextMenuStripTrack";
         contextMenuStripReadOnlyTracks.Size = new Size(270, 344);
         contextMenuStripReadOnlyTracks.Closed += contextMenuStripReadOnlyTracks_Closed;
         contextMenuStripReadOnlyTracks.Opening += contextMenuStripReadOnlyTracks_Opening;
         // 
         // toolStripMenuItem_ReadOnlyTrackShow
         // 
         toolStripMenuItem_ReadOnlyTrackShow.Image = Properties.Resources.Track;
         toolStripMenuItem_ReadOnlyTrackShow.Name = "toolStripMenuItem_ReadOnlyTrackShow";
         toolStripMenuItem_ReadOnlyTrackShow.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackShow.Text = "Track &anzeigen";
         toolStripMenuItem_ReadOnlyTrackShow.Click += toolStripMenuItem_ReadOnlyTrackShow_Click;
         // 
         // toolStripMenuItem_ReadOnlyTrackShowSlope
         // 
         toolStripMenuItem_ReadOnlyTrackShowSlope.Name = "toolStripMenuItem_ReadOnlyTrackShowSlope";
         toolStripMenuItem_ReadOnlyTrackShowSlope.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackShowSlope.Text = "Anstiegssymbole anzeigen";
         toolStripMenuItem_ReadOnlyTrackShowSlope.Click += toolStripMenuItem_ReadOnlyTrackShowSlope_Click;
         // 
         // toolStripMenuItem_ReadOnlyTrackZoom
         // 
         toolStripMenuItem_ReadOnlyTrackZoom.Image = Properties.Resources.zoom1;
         toolStripMenuItem_ReadOnlyTrackZoom.Name = "toolStripMenuItem_ReadOnlyTrackZoom";
         toolStripMenuItem_ReadOnlyTrackZoom.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackZoom.Text = "&Zoom auf diesen Track";
         toolStripMenuItem_ReadOnlyTrackZoom.Click += toolStripMenuItem_ReadOnlyTrackZoom_Click;
         // 
         // toolStripMenuItem_ReadOnlyGpxShowMarker
         // 
         toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = true;
         toolStripMenuItem_ReadOnlyGpxShowMarker.CheckOnClick = true;
         toolStripMenuItem_ReadOnlyGpxShowMarker.CheckState = CheckState.Checked;
         toolStripMenuItem_ReadOnlyGpxShowMarker.Name = "toolStripMenuItem_ReadOnlyGpxShowMarker";
         toolStripMenuItem_ReadOnlyGpxShowMarker.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyGpxShowMarker.Text = "&Wegpunkte auch anzeigen";
         toolStripMenuItem_ReadOnlyGpxShowMarker.Click += toolStripMenuItem_ReadOnlyGpxShowMarker_Click;
         // 
         // toolStripMenuItem_ReadOnlyGpxShowPictureMarker
         // 
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.CheckOnClick = true;
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Image = Properties.Resources.Foto;
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Name = "toolStripMenuItem_ReadOnlyGpxShowPictureMarker";
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Text = "&Bildwegpunkte auch anzeigen";
         toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Click += toolStripMenuItem_ReadOnlyGpxShowPictureMarker_Click;
         // 
         // toolStripMenuItem_ReadOnlyTrackInfo
         // 
         toolStripMenuItem_ReadOnlyTrackInfo.Image = Properties.Resources.info;
         toolStripMenuItem_ReadOnlyTrackInfo.Name = "toolStripMenuItem_ReadOnlyTrackInfo";
         toolStripMenuItem_ReadOnlyTrackInfo.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackInfo.Text = "&Info anzeigen";
         toolStripMenuItem_ReadOnlyTrackInfo.Click += toolStripMenuItem_ReadOnlyTrackInfo_Click;
         // 
         // toolStripMenuItem_ReadOnlyTrackExtInfo
         // 
         toolStripMenuItem_ReadOnlyTrackExtInfo.Image = Properties.Resources.edit;
         toolStripMenuItem_ReadOnlyTrackExtInfo.Name = "toolStripMenuItem_ReadOnlyTrackExtInfo";
         toolStripMenuItem_ReadOnlyTrackExtInfo.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackExtInfo.Text = "&erweiterte Infos anzeigen";
         toolStripMenuItem_ReadOnlyTrackExtInfo.Click += toolStripMenuItem_ReadOnlyTrackExtInfo_Click;
         // 
         // toolStripSeparator1
         // 
         toolStripSeparator1.Name = "toolStripSeparator1";
         toolStripSeparator1.Size = new Size(266, 6);
         // 
         // toolStripMenuItem_ReadOnlyTracksHide
         // 
         toolStripMenuItem_ReadOnlyTracksHide.Name = "toolStripMenuItem_ReadOnlyTracksHide";
         toolStripMenuItem_ReadOnlyTracksHide.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTracksHide.Text = "alle (!) angezeigten Tracks &verbergen";
         toolStripMenuItem_ReadOnlyTracksHide.Click += toolStripMenuItem_ReadOnlyTracksHide_Click;
         // 
         // toolStripSeparator10
         // 
         toolStripSeparator10.Name = "toolStripSeparator10";
         toolStripSeparator10.Size = new Size(266, 6);
         // 
         // toolStripMenuItem_ReadOnlyTrackColor
         // 
         toolStripMenuItem_ReadOnlyTrackColor.BackColor = SystemColors.Control;
         toolStripMenuItem_ReadOnlyTrackColor.Name = "toolStripMenuItem_ReadOnlyTrackColor";
         toolStripMenuItem_ReadOnlyTrackColor.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackColor.Text = "&Trackfarbe ändern";
         toolStripMenuItem_ReadOnlyTrackColor.Click += toolStripMenuItem_ReadOnlyTrackColor_Click;
         // 
         // numericUpDownMenuItem_ReadOnlyLineThickness
         // 
         numericUpDownMenuItem_ReadOnlyLineThickness.BackColor = SystemColors.Control;
         numericUpDownMenuItem_ReadOnlyLineThickness.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
         numericUpDownMenuItem_ReadOnlyLineThickness.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
         numericUpDownMenuItem_ReadOnlyLineThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
         numericUpDownMenuItem_ReadOnlyLineThickness.Name = "numericUpDownMenuItem_ReadOnlyLineThickness";
         numericUpDownMenuItem_ReadOnlyLineThickness.Size = new Size(139, 33);
         numericUpDownMenuItem_ReadOnlyLineThickness.Text = "Liniendicke";
         numericUpDownMenuItem_ReadOnlyLineThickness.Value = new decimal(new int[] { 5, 0, 0, 0 });
         // 
         // toolStripSeparator11
         // 
         toolStripSeparator11.Name = "toolStripSeparator11";
         toolStripSeparator11.Size = new Size(266, 6);
         // 
         // toolStripMenuItem_ReadOnlyTrackClone
         // 
         toolStripMenuItem_ReadOnlyTrackClone.Image = Properties.Resources.kopie;
         toolStripMenuItem_ReadOnlyTrackClone.Name = "toolStripMenuItem_ReadOnlyTrackClone";
         toolStripMenuItem_ReadOnlyTrackClone.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyTrackClone.Text = "&bearbeitbare Kopie erzeugen";
         toolStripMenuItem_ReadOnlyTrackClone.Click += toolStripMenuItem_ReadOnlyTrackClone_Click;
         // 
         // toolStripMenuItem_ReadOnlyGpxRemove
         // 
         toolStripMenuItem_ReadOnlyGpxRemove.Image = Properties.Resources.delete;
         toolStripMenuItem_ReadOnlyGpxRemove.Name = "toolStripMenuItem_ReadOnlyGpxRemove";
         toolStripMenuItem_ReadOnlyGpxRemove.Size = new Size(269, 26);
         toolStripMenuItem_ReadOnlyGpxRemove.Text = "GPX-Datei aus der Liste entfernen";
         toolStripMenuItem_ReadOnlyGpxRemove.Click += toolStripMenuItem_ReadOnlyGpxRemove_Click;
         // 
         // tabPageEditable
         // 
         tabPageEditable.Controls.Add(toolStripContainer2);
         tabPageEditable.Location = new Point(4, 44);
         tabPageEditable.Margin = new Padding(4, 3, 4, 3);
         tabPageEditable.Name = "tabPageEditable";
         tabPageEditable.Size = new Size(238, 560);
         tabPageEditable.TabIndex = 3;
         tabPageEditable.Text = "bearbeitbare Tracks/Marker";
         tabPageEditable.UseVisualStyleBackColor = true;
         // 
         // toolStripContainer2
         // 
         // 
         // toolStripContainer2.ContentPanel
         // 
         toolStripContainer2.ContentPanel.Controls.Add(editableTracklistControl1);
         toolStripContainer2.ContentPanel.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer2.ContentPanel.Size = new Size(238, 533);
         toolStripContainer2.Dock = DockStyle.Fill;
         toolStripContainer2.Location = new Point(0, 0);
         toolStripContainer2.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer2.Name = "toolStripContainer2";
         toolStripContainer2.Size = new Size(238, 560);
         toolStripContainer2.TabIndex = 1;
         toolStripContainer2.Text = "toolStripContainer2";
         // 
         // toolStripContainer2.TopToolStripPanel
         // 
         toolStripContainer2.TopToolStripPanel.Controls.Add(toolStrip_Edit);
         // 
         // editableTracklistControl1
         // 
         editableTracklistControl1.AllowDrop = true;
         editableTracklistControl1.BorderStyle = BorderStyle.Fixed3D;
         editableTracklistControl1.Dock = DockStyle.Fill;
         editableTracklistControl1.GpxWorkbench = null;
         editableTracklistControl1.ListBackColor = Color.FromArgb(192, 255, 192);
         editableTracklistControl1.Location = new Point(0, 0);
         editableTracklistControl1.Margin = new Padding(5);
         editableTracklistControl1.Name = "editableTracklistControl1";
         editableTracklistControl1.Size = new Size(238, 533);
         editableTracklistControl1.TabIndex = 0;
         // 
         // toolStrip_Edit
         // 
         toolStrip_Edit.Dock = DockStyle.None;
         toolStrip_Edit.ImageScalingSize = new Size(20, 20);
         toolStrip_Edit.Items.AddRange(new ToolStripItem[] { toolStripButton_ViewerMode, toolStripButton_SetMarker, toolStripButton_TrackDraw, toolStripButton_EditEnd, toolStripButton_EditCancel, toolStripButton_ClearEditable, toolStripButton_UniqueNames });
         toolStrip_Edit.Location = new Point(3, 0);
         toolStrip_Edit.Name = "toolStrip_Edit";
         toolStrip_Edit.Size = new Size(180, 27);
         toolStrip_Edit.TabIndex = 1;
         // 
         // toolStripButton_ViewerMode
         // 
         toolStripButton_ViewerMode.Checked = true;
         toolStripButton_ViewerMode.CheckOnClick = true;
         toolStripButton_ViewerMode.CheckState = CheckState.Checked;
         toolStripButton_ViewerMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ViewerMode.Image = Properties.Resources.Hand;
         toolStripButton_ViewerMode.ImageTransparentColor = Color.Magenta;
         toolStripButton_ViewerMode.Name = "toolStripButton_ViewerMode";
         toolStripButton_ViewerMode.Size = new Size(24, 24);
         toolStripButton_ViewerMode.Text = "Karte verschieben";
         toolStripButton_ViewerMode.Click += toolStripButton_ViewerMode_Click;
         // 
         // toolStripButton_SetMarker
         // 
         toolStripButton_SetMarker.CheckOnClick = true;
         toolStripButton_SetMarker.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SetMarker.Image = Properties.Resources.Flag16x16;
         toolStripButton_SetMarker.ImageTransparentColor = Color.Magenta;
         toolStripButton_SetMarker.Name = "toolStripButton_SetMarker";
         toolStripButton_SetMarker.Size = new Size(24, 24);
         toolStripButton_SetMarker.Text = "neue Markierung setzen";
         toolStripButton_SetMarker.Click += toolStripButton_SetMarker_Click;
         // 
         // toolStripButton_TrackDraw
         // 
         toolStripButton_TrackDraw.CheckOnClick = true;
         toolStripButton_TrackDraw.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_TrackDraw.Image = Properties.Resources.TrackDraw;
         toolStripButton_TrackDraw.ImageTransparentColor = Color.Magenta;
         toolStripButton_TrackDraw.Name = "toolStripButton_TrackDraw";
         toolStripButton_TrackDraw.Size = new Size(24, 24);
         toolStripButton_TrackDraw.Text = "neuen Track zeichnen";
         toolStripButton_TrackDraw.Click += toolStripButton_TrackDraw_Click;
         // 
         // toolStripButton_EditEnd
         // 
         toolStripButton_EditEnd.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_EditEnd.Enabled = false;
         toolStripButton_EditEnd.Image = Properties.Resources.ok;
         toolStripButton_EditEnd.ImageTransparentColor = Color.Magenta;
         toolStripButton_EditEnd.Name = "toolStripButton_EditEnd";
         toolStripButton_EditEnd.Size = new Size(24, 24);
         toolStripButton_EditEnd.Text = "Track zeichnen beenden";
         toolStripButton_EditEnd.Click += toolStripButton_EditEnd_Click;
         // 
         // toolStripButton_EditCancel
         // 
         toolStripButton_EditCancel.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_EditCancel.Enabled = false;
         toolStripButton_EditCancel.Image = Properties.Resources.cancel;
         toolStripButton_EditCancel.ImageTransparentColor = Color.Magenta;
         toolStripButton_EditCancel.Name = "toolStripButton_EditCancel";
         toolStripButton_EditCancel.Size = new Size(24, 24);
         toolStripButton_EditCancel.Text = "Trackbearbeitung abbrechen";
         toolStripButton_EditCancel.Click += toolStripButton_EditCancel_Click;
         // 
         // toolStripButton_ClearEditable
         // 
         toolStripButton_ClearEditable.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ClearEditable.Image = Properties.Resources.delete;
         toolStripButton_ClearEditable.ImageTransparentColor = Color.Magenta;
         toolStripButton_ClearEditable.Name = "toolStripButton_ClearEditable";
         toolStripButton_ClearEditable.Size = new Size(24, 24);
         toolStripButton_ClearEditable.Text = "alle editierbaren Tracks und Markierungen löschen";
         toolStripButton_ClearEditable.Click += toolStripButton_ClearEditable_Click;
         // 
         // toolStripButton_UniqueNames
         // 
         toolStripButton_UniqueNames.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_UniqueNames.Image = Properties.Resources.list_numbers;
         toolStripButton_UniqueNames.ImageTransparentColor = Color.Magenta;
         toolStripButton_UniqueNames.Name = "toolStripButton_UniqueNames";
         toolStripButton_UniqueNames.Size = new Size(24, 24);
         toolStripButton_UniqueNames.Text = "Namen der Tracks und Marker eindeutig machen";
         toolStripButton_UniqueNames.Click += toolStripButton_UniqueNames_Click;
         // 
         // tabPageSearch
         // 
         tabPageSearch.Controls.Add(searchControl1);
         tabPageSearch.Location = new Point(4, 44);
         tabPageSearch.Name = "tabPageSearch";
         tabPageSearch.Size = new Size(238, 560);
         tabPageSearch.TabIndex = 7;
         tabPageSearch.Text = "Suche";
         tabPageSearch.UseVisualStyleBackColor = true;
         // 
         // searchControl1
         // 
         searchControl1.BackColor = SystemColors.Control;
         searchControl1.Dock = DockStyle.Fill;
         searchControl1.Location = new Point(0, 0);
         searchControl1.Name = "searchControl1";
         searchControl1.Size = new Size(238, 560);
         searchControl1.TabIndex = 0;
         // 
         // tabPageLocation
         // 
         tabPageLocation.BackColor = SystemColors.Control;
         tabPageLocation.Controls.Add(splitContainer2);
         tabPageLocation.Location = new Point(4, 44);
         tabPageLocation.Name = "tabPageLocation";
         tabPageLocation.Padding = new Padding(3);
         tabPageLocation.Size = new Size(238, 560);
         tabPageLocation.TabIndex = 6;
         tabPageLocation.Text = "geografische Positionen";
         // 
         // splitContainer2
         // 
         splitContainer2.Dock = DockStyle.Fill;
         splitContainer2.Location = new Point(3, 3);
         splitContainer2.Name = "splitContainer2";
         splitContainer2.Orientation = Orientation.Horizontal;
         // 
         // splitContainer2.Panel1
         // 
         splitContainer2.Panel1.Controls.Add(locationControl1);
         // 
         // splitContainer2.Panel2
         // 
         splitContainer2.Panel2.Controls.Add(geoLocationControl1);
         splitContainer2.Size = new Size(232, 554);
         splitContainer2.SplitterDistance = 351;
         splitContainer2.TabIndex = 14;
         // 
         // locationControl1
         // 
         locationControl1.Dock = DockStyle.Fill;
         locationControl1.Location = new Point(0, 0);
         locationControl1.Name = "locationControl1";
         locationControl1.Size = new Size(232, 351);
         locationControl1.TabIndex = 0;
         // 
         // geoLocationControl1
         // 
         geoLocationControl1.Location = new Point(0, 2);
         geoLocationControl1.Name = "geoLocationControl1";
         geoLocationControl1.Size = new Size(424, 166);
         geoLocationControl1.TabIndex = 0;
         // 
         // tabPageFoto
         // 
         tabPageFoto.Controls.Add(pictureManager1);
         tabPageFoto.Location = new Point(4, 44);
         tabPageFoto.Name = "tabPageFoto";
         tabPageFoto.Size = new Size(238, 560);
         tabPageFoto.TabIndex = 4;
         tabPageFoto.Text = "Fotos";
         tabPageFoto.UseVisualStyleBackColor = true;
         // 
         // pictureManager1
         // 
         pictureManager1.ActualPicturePath = "C:\\Users\\Petra und Frank\\Pictures";
         pictureManager1.Dock = DockStyle.Fill;
         pictureManager1.Location = new Point(0, 0);
         pictureManager1.Margin = new Padding(4, 3, 4, 3);
         pictureManager1.Name = "pictureManager1";
         pictureManager1.Size = new Size(238, 560);
         pictureManager1.TabIndex = 0;
         // 
         // mapCtrl
         // 
         mapCtrl.Dock = DockStyle.Fill;
         mapCtrl.ForeColor = SystemColors.ControlDark;
         mapCtrl.Location = new Point(0, 0);
         mapCtrl.M_CacheLocation = "C:\\Users\\Petra und Frank\\AppData\\Local\\GMap.NET\\";
         mapCtrl.M_CanDragMap = true;
         mapCtrl.M_ClickTolerance4Tracks = 1F;
         mapCtrl.M_CopyrightFont = new Font("Microsoft Sans Serif", 7F);
         mapCtrl.M_Cursor = Cursors.Default;
         mapCtrl.M_DeviceZoom = 1D;
         mapCtrl.M_DragButton = MouseButtons.Right;
         mapCtrl.M_EmptyMapBackgroundColor = Color.LightGray;
         mapCtrl.M_EmptyTileColor = Color.DarkGray;
         mapCtrl.M_EmptyTileText = "keine Daten";
         mapCtrl.M_FillEmptyTiles = false;
         mapCtrl.M_LevelsKeepInMemory = 5;
         mapCtrl.M_MarkersEnabled = true;
         mapCtrl.M_MinZoom = 0;
         mapCtrl.M_MouseWheelZoomEnabled = true;
         mapCtrl.M_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         mapCtrl.M_PolygonsEnabled = true;
         mapCtrl.M_RetryLoadTile = 0;
         mapCtrl.M_ScaleAlpha = 180;
         mapCtrl.M_ScaleKind = SpecialMapCtrl.Scale4Map.ScaleKind.Around;
         mapCtrl.M_ScaleMode = SpecialMapCtrl.SpecialMapCtrl.ScaleModes.Fractional;
         mapCtrl.M_SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);
         mapCtrl.M_ShowCenter = false;
         mapCtrl.M_ShowTileGridLines = true;
         mapCtrl.M_TracksEnabled = true;
         mapCtrl.Margin = new Padding(0);
         mapCtrl.Name = "mapCtrl";
         mapCtrl.Size = new Size(754, 608);
         mapCtrl.TabIndex = 1;
         // 
         // contextMenuStripEditableTracks
         // 
         contextMenuStripEditableTracks.ImageScalingSize = new Size(20, 20);
         contextMenuStripEditableTracks.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_EditableTrackDraw, toolStripMenuItem_EditableTrackSplit, toolStripMenuItem_EditableTrackAppend, toolStripMenuItem_EditableTrackPointRemove, toolStripMenuItem_EditableTrackReverse, toolStripMenuItem_EditableTrackClone, toolStripMenuItem_EditableTrackDelete, toolStripSeparator15, toolStripMenuItem_EditableTrackShow, toolStripMenuItem_EditableTrackShowSlope, toolStripMenuItem_EditableTrackZoom, toolStripMenuItem_EditableTrackInfo, toolStripMenuItem_EditableTrackExtInfo, toolStripSeparator12, toolStripMenuItem_EditableTrackColor, numericUpDownMenuItem_EditableLineThickness, toolStripSeparator4, ToolStripMenuItem_EditableTrackSimplify, toolStripSeparator2, ToolStripMenuItem_EditableGroupInsert1, ToolStripMenuItem_EditableGroupDelete1, toolStripSeparator5, ToolStripMenuItem_ShowAllEditableObjects1, ToolStripMenuItem_HideAllEditableObjects1, ToolStripMenuItem_DeleteAllVisibleEditableObjects1 });
         contextMenuStripEditableTracks.Name = "contextMenuStripTrack";
         contextMenuStripEditableTracks.Size = new Size(266, 564);
         contextMenuStripEditableTracks.Closed += contextMenuStripEditableTracks_Closed;
         contextMenuStripEditableTracks.Opening += contextMenuStripEditableTracks_Opening;
         // 
         // toolStripMenuItem_EditableTrackDraw
         // 
         toolStripMenuItem_EditableTrackDraw.Image = Properties.Resources.TrackDraw;
         toolStripMenuItem_EditableTrackDraw.Name = "toolStripMenuItem_EditableTrackDraw";
         toolStripMenuItem_EditableTrackDraw.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackDraw.Text = "Track &weiter zeichnen";
         toolStripMenuItem_EditableTrackDraw.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackSplit
         // 
         toolStripMenuItem_EditableTrackSplit.Image = Properties.Resources.TrackSplit;
         toolStripMenuItem_EditableTrackSplit.Name = "toolStripMenuItem_EditableTrackSplit";
         toolStripMenuItem_EditableTrackSplit.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackSplit.Text = "Track &trennen";
         toolStripMenuItem_EditableTrackSplit.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackAppend
         // 
         toolStripMenuItem_EditableTrackAppend.Image = Properties.Resources.TrackConcat;
         toolStripMenuItem_EditableTrackAppend.Name = "toolStripMenuItem_EditableTrackAppend";
         toolStripMenuItem_EditableTrackAppend.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackAppend.Text = "anderen Track &anhängen";
         toolStripMenuItem_EditableTrackAppend.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackPointRemove
         // 
         toolStripMenuItem_EditableTrackPointRemove.Image = Properties.Resources.TrackPointRemove;
         toolStripMenuItem_EditableTrackPointRemove.Name = "toolStripMenuItem_EditableTrackPointRemove";
         toolStripMenuItem_EditableTrackPointRemove.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackPointRemove.Text = "Trackpunkt entfernen";
         toolStripMenuItem_EditableTrackPointRemove.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackReverse
         // 
         toolStripMenuItem_EditableTrackReverse.Image = Properties.Resources.arrow_undo;
         toolStripMenuItem_EditableTrackReverse.Name = "toolStripMenuItem_EditableTrackReverse";
         toolStripMenuItem_EditableTrackReverse.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackReverse.Text = "Track &umkehren";
         toolStripMenuItem_EditableTrackReverse.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackClone
         // 
         toolStripMenuItem_EditableTrackClone.Image = Properties.Resources.kopie;
         toolStripMenuItem_EditableTrackClone.Name = "toolStripMenuItem_EditableTrackClone";
         toolStripMenuItem_EditableTrackClone.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackClone.Text = "&Kopie erzeugen";
         toolStripMenuItem_EditableTrackClone.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackDelete
         // 
         toolStripMenuItem_EditableTrackDelete.Image = Properties.Resources.delete;
         toolStripMenuItem_EditableTrackDelete.Name = "toolStripMenuItem_EditableTrackDelete";
         toolStripMenuItem_EditableTrackDelete.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackDelete.Text = "Track &löschen";
         toolStripMenuItem_EditableTrackDelete.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripSeparator15
         // 
         toolStripSeparator15.Name = "toolStripSeparator15";
         toolStripSeparator15.Size = new Size(262, 6);
         // 
         // toolStripMenuItem_EditableTrackShow
         // 
         toolStripMenuItem_EditableTrackShow.Image = Properties.Resources.Track;
         toolStripMenuItem_EditableTrackShow.Name = "toolStripMenuItem_EditableTrackShow";
         toolStripMenuItem_EditableTrackShow.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackShow.Text = "&Track anzeigen";
         toolStripMenuItem_EditableTrackShow.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackShowSlope
         // 
         toolStripMenuItem_EditableTrackShowSlope.Name = "toolStripMenuItem_EditableTrackShowSlope";
         toolStripMenuItem_EditableTrackShowSlope.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackShowSlope.Text = "Anstiegssymbole anzeigen";
         toolStripMenuItem_EditableTrackShowSlope.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackZoom
         // 
         toolStripMenuItem_EditableTrackZoom.Image = Properties.Resources.zoom1;
         toolStripMenuItem_EditableTrackZoom.Name = "toolStripMenuItem_EditableTrackZoom";
         toolStripMenuItem_EditableTrackZoom.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackZoom.Text = "&Zoom auf diesen Track";
         toolStripMenuItem_EditableTrackZoom.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackInfo
         // 
         toolStripMenuItem_EditableTrackInfo.Image = Properties.Resources.info;
         toolStripMenuItem_EditableTrackInfo.Name = "toolStripMenuItem_EditableTrackInfo";
         toolStripMenuItem_EditableTrackInfo.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackInfo.Text = "&Info anzeigen";
         toolStripMenuItem_EditableTrackInfo.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripMenuItem_EditableTrackExtInfo
         // 
         toolStripMenuItem_EditableTrackExtInfo.Image = Properties.Resources.edit;
         toolStripMenuItem_EditableTrackExtInfo.Name = "toolStripMenuItem_EditableTrackExtInfo";
         toolStripMenuItem_EditableTrackExtInfo.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackExtInfo.Text = "&erweiterte Infos";
         toolStripMenuItem_EditableTrackExtInfo.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripSeparator12
         // 
         toolStripSeparator12.Name = "toolStripSeparator12";
         toolStripSeparator12.Size = new Size(262, 6);
         // 
         // toolStripMenuItem_EditableTrackColor
         // 
         toolStripMenuItem_EditableTrackColor.BackColor = SystemColors.Control;
         toolStripMenuItem_EditableTrackColor.Name = "toolStripMenuItem_EditableTrackColor";
         toolStripMenuItem_EditableTrackColor.Size = new Size(265, 26);
         toolStripMenuItem_EditableTrackColor.Text = "Track&farbe ändern";
         toolStripMenuItem_EditableTrackColor.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // numericUpDownMenuItem_EditableLineThickness
         // 
         numericUpDownMenuItem_EditableLineThickness.BackColor = SystemColors.Control;
         numericUpDownMenuItem_EditableLineThickness.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
         numericUpDownMenuItem_EditableLineThickness.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
         numericUpDownMenuItem_EditableLineThickness.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
         numericUpDownMenuItem_EditableLineThickness.Name = "numericUpDownMenuItem_EditableLineThickness";
         numericUpDownMenuItem_EditableLineThickness.Size = new Size(164, 33);
         numericUpDownMenuItem_EditableLineThickness.Text = "Trackliniendicke";
         numericUpDownMenuItem_EditableLineThickness.Value = new decimal(new int[] { 5, 0, 0, 0 });
         // 
         // toolStripSeparator4
         // 
         toolStripSeparator4.Name = "toolStripSeparator4";
         toolStripSeparator4.Size = new Size(262, 6);
         // 
         // ToolStripMenuItem_EditableTrackSimplify
         // 
         ToolStripMenuItem_EditableTrackSimplify.Image = Properties.Resources.TrackSimpl;
         ToolStripMenuItem_EditableTrackSimplify.Name = "ToolStripMenuItem_EditableTrackSimplify";
         ToolStripMenuItem_EditableTrackSimplify.Size = new Size(265, 26);
         ToolStripMenuItem_EditableTrackSimplify.Text = "Track &vereinfachen";
         ToolStripMenuItem_EditableTrackSimplify.Click += toolStripMenuItem_EditableForOneTrack;
         // 
         // toolStripSeparator2
         // 
         toolStripSeparator2.Name = "toolStripSeparator2";
         toolStripSeparator2.Size = new Size(262, 6);
         // 
         // ToolStripMenuItem_EditableGroupInsert1
         // 
         ToolStripMenuItem_EditableGroupInsert1.Image = Properties.Resources.Open;
         ToolStripMenuItem_EditableGroupInsert1.Name = "ToolStripMenuItem_EditableGroupInsert1";
         ToolStripMenuItem_EditableGroupInsert1.Size = new Size(265, 26);
         ToolStripMenuItem_EditableGroupInsert1.Text = "neue Gruppe anlegen";
         ToolStripMenuItem_EditableGroupInsert1.Click += ToolStripMenuItem_EditableGroupInsert_Click;
         // 
         // ToolStripMenuItem_EditableGroupDelete1
         // 
         ToolStripMenuItem_EditableGroupDelete1.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_EditableGroupDelete1.Image = Properties.Resources.delete;
         ToolStripMenuItem_EditableGroupDelete1.Name = "ToolStripMenuItem_EditableGroupDelete1";
         ToolStripMenuItem_EditableGroupDelete1.Size = new Size(265, 26);
         ToolStripMenuItem_EditableGroupDelete1.Text = "gesamte (!) Gruppe löschen";
         ToolStripMenuItem_EditableGroupDelete1.Click += ToolStripMenuItem_EditableGroupDelete_Click;
         // 
         // toolStripSeparator5
         // 
         toolStripSeparator5.Name = "toolStripSeparator5";
         toolStripSeparator5.Size = new Size(262, 6);
         // 
         // ToolStripMenuItem_ShowAllEditableObjects1
         // 
         ToolStripMenuItem_ShowAllEditableObjects1.Name = "ToolStripMenuItem_ShowAllEditableObjects1";
         ToolStripMenuItem_ShowAllEditableObjects1.Size = new Size(265, 26);
         ToolStripMenuItem_ShowAllEditableObjects1.Text = "alle Objekte anzeigen";
         ToolStripMenuItem_ShowAllEditableObjects1.Click += ToolStripMenuItem_ShowAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_HideAllEditableObjects1
         // 
         ToolStripMenuItem_HideAllEditableObjects1.Name = "ToolStripMenuItem_HideAllEditableObjects1";
         ToolStripMenuItem_HideAllEditableObjects1.Size = new Size(265, 26);
         ToolStripMenuItem_HideAllEditableObjects1.Text = "alle Objekte verbergen";
         ToolStripMenuItem_HideAllEditableObjects1.Click += ToolStripMenuItem_HideAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_DeleteAllVisibleEditableObjects1
         // 
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Image = Properties.Resources.delete;
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Name = "ToolStripMenuItem_DeleteAllVisibleEditableObjects1";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Size = new Size(265, 26);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Text = "alle angezeigten (!) Objekte löschen";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects1.Click += ToolStripMenuItem_DeleteAllVisibleEditableObjects_Click;
         // 
         // contextMenuStripEditableMarker
         // 
         contextMenuStripEditableMarker.ImageScalingSize = new Size(20, 20);
         contextMenuStripEditableMarker.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_WaypointZoom, ToolStripMenuItem_WaypointShow, ToolStripMenuItem_WaypointEdit, ToolStripMenuItem_WaypointClone, ToolStripMenuItem_WaypointSet, ToolStripMenuItem_WaypointDelete, xToolStripMenuItem, ToolStripMenuItem_EditableGroupInsert3, ToolStripMenuItem_EditableGroupDelete3, toolStripSeparator7, ToolStripMenuItem_ShowAllEditableObjects3, ToolStripMenuItem_HideAllEditableObjects3, ToolStripMenuItem_DeleteAllVisibleEditableObjects3 });
         contextMenuStripEditableMarker.Name = "contextMenuStripEditMarker";
         contextMenuStripEditableMarker.Size = new Size(270, 302);
         contextMenuStripEditableMarker.Opening += contextMenuStripMarker_Opening;
         // 
         // ToolStripMenuItem_WaypointZoom
         // 
         ToolStripMenuItem_WaypointZoom.Image = Properties.Resources.zoom1;
         ToolStripMenuItem_WaypointZoom.Name = "ToolStripMenuItem_WaypointZoom";
         ToolStripMenuItem_WaypointZoom.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointZoom.Text = "Zoom auf diese Markierung";
         ToolStripMenuItem_WaypointZoom.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // ToolStripMenuItem_WaypointShow
         // 
         ToolStripMenuItem_WaypointShow.Image = Properties.Resources.Flag16x16;
         ToolStripMenuItem_WaypointShow.Name = "ToolStripMenuItem_WaypointShow";
         ToolStripMenuItem_WaypointShow.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointShow.Text = "Markierung anzeigen";
         ToolStripMenuItem_WaypointShow.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // ToolStripMenuItem_WaypointEdit
         // 
         ToolStripMenuItem_WaypointEdit.Image = Properties.Resources.edit;
         ToolStripMenuItem_WaypointEdit.Name = "ToolStripMenuItem_WaypointEdit";
         ToolStripMenuItem_WaypointEdit.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointEdit.Text = "Eigenschaften anzeigen / &bearbeiten";
         ToolStripMenuItem_WaypointEdit.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // ToolStripMenuItem_WaypointClone
         // 
         ToolStripMenuItem_WaypointClone.Image = Properties.Resources.kopie;
         ToolStripMenuItem_WaypointClone.Name = "ToolStripMenuItem_WaypointClone";
         ToolStripMenuItem_WaypointClone.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointClone.Text = "bearbeitbare Kopie erzeugen";
         ToolStripMenuItem_WaypointClone.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // ToolStripMenuItem_WaypointSet
         // 
         ToolStripMenuItem_WaypointSet.Name = "ToolStripMenuItem_WaypointSet";
         ToolStripMenuItem_WaypointSet.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointSet.Text = "neue Position &setzen";
         ToolStripMenuItem_WaypointSet.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // ToolStripMenuItem_WaypointDelete
         // 
         ToolStripMenuItem_WaypointDelete.Image = Properties.Resources.delete;
         ToolStripMenuItem_WaypointDelete.Name = "ToolStripMenuItem_WaypointDelete";
         ToolStripMenuItem_WaypointDelete.Size = new Size(269, 26);
         ToolStripMenuItem_WaypointDelete.Text = "Marker &löschen";
         ToolStripMenuItem_WaypointDelete.Click += ToolStripMenuItem_ForOneWaypoint;
         // 
         // xToolStripMenuItem
         // 
         xToolStripMenuItem.Name = "xToolStripMenuItem";
         xToolStripMenuItem.Size = new Size(266, 6);
         // 
         // ToolStripMenuItem_EditableGroupInsert3
         // 
         ToolStripMenuItem_EditableGroupInsert3.Image = Properties.Resources.Open;
         ToolStripMenuItem_EditableGroupInsert3.Name = "ToolStripMenuItem_EditableGroupInsert3";
         ToolStripMenuItem_EditableGroupInsert3.Size = new Size(269, 26);
         ToolStripMenuItem_EditableGroupInsert3.Text = "Gruppe anlegen";
         ToolStripMenuItem_EditableGroupInsert3.Click += ToolStripMenuItem_EditableGroupInsert_Click;
         // 
         // ToolStripMenuItem_EditableGroupDelete3
         // 
         ToolStripMenuItem_EditableGroupDelete3.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_EditableGroupDelete3.Image = Properties.Resources.delete;
         ToolStripMenuItem_EditableGroupDelete3.Name = "ToolStripMenuItem_EditableGroupDelete3";
         ToolStripMenuItem_EditableGroupDelete3.Size = new Size(269, 26);
         ToolStripMenuItem_EditableGroupDelete3.Text = "gesamte (!) Gruppe löschen";
         ToolStripMenuItem_EditableGroupDelete3.Click += ToolStripMenuItem_EditableGroupDelete_Click;
         // 
         // toolStripSeparator7
         // 
         toolStripSeparator7.Name = "toolStripSeparator7";
         toolStripSeparator7.Size = new Size(266, 6);
         // 
         // ToolStripMenuItem_ShowAllEditableObjects3
         // 
         ToolStripMenuItem_ShowAllEditableObjects3.Name = "ToolStripMenuItem_ShowAllEditableObjects3";
         ToolStripMenuItem_ShowAllEditableObjects3.Size = new Size(269, 26);
         ToolStripMenuItem_ShowAllEditableObjects3.Text = "alle Objekte anzeigen";
         ToolStripMenuItem_ShowAllEditableObjects3.Click += ToolStripMenuItem_ShowAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_HideAllEditableObjects3
         // 
         ToolStripMenuItem_HideAllEditableObjects3.Name = "ToolStripMenuItem_HideAllEditableObjects3";
         ToolStripMenuItem_HideAllEditableObjects3.Size = new Size(269, 26);
         ToolStripMenuItem_HideAllEditableObjects3.Text = "alle Objekte verbergen";
         ToolStripMenuItem_HideAllEditableObjects3.Click += ToolStripMenuItem_HideAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_DeleteAllVisibleEditableObjects3
         // 
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Image = Properties.Resources.delete;
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Name = "ToolStripMenuItem_DeleteAllVisibleEditableObjects3";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Size = new Size(269, 26);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Text = "alle angezeigten (!) Objekte löschen";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects3.Click += ToolStripMenuItem_DeleteAllVisibleEditableObjects_Click;
         // 
         // toolStripContainer1
         // 
         // 
         // toolStripContainer1.BottomToolStripPanel
         // 
         toolStripContainer1.BottomToolStripPanel.Controls.Add(statusStrip1);
         // 
         // toolStripContainer1.ContentPanel
         // 
         toolStripContainer1.ContentPanel.Controls.Add(panelMap);
         toolStripContainer1.ContentPanel.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer1.ContentPanel.Size = new Size(1058, 610);
         toolStripContainer1.Dock = DockStyle.Fill;
         toolStripContainer1.Location = new Point(0, 0);
         toolStripContainer1.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer1.Name = "toolStripContainer1";
         toolStripContainer1.Size = new Size(1058, 661);
         toolStripContainer1.TabIndex = 7;
         toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         toolStripContainer1.TopToolStripPanel.Controls.Add(menuStrip1);
         toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip_Standard);
         // 
         // statusStrip1
         // 
         statusStrip1.Dock = DockStyle.None;
         statusStrip1.ImageScalingSize = new Size(20, 20);
         statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_MapLoad, toolStripStatusLabel_Zoom, toolStripStatusLabel_Pos, toolStripStatusLabel_TrackMiniInfo, toolStripStatusLabel_TrackInfo, toolStripStatusLabel_GpxLoad });
         statusStrip1.Location = new Point(0, 0);
         statusStrip1.Name = "statusStrip1";
         statusStrip1.Size = new Size(1058, 24);
         statusStrip1.TabIndex = 0;
         // 
         // toolStripStatusLabel_MapLoad
         // 
         toolStripStatusLabel_MapLoad.BackColor = SystemColors.Control;
         toolStripStatusLabel_MapLoad.Font = new Font("Segoe UI", 9F);
         toolStripStatusLabel_MapLoad.Name = "toolStripStatusLabel_MapLoad";
         toolStripStatusLabel_MapLoad.Size = new Size(14, 19);
         toolStripStatusLabel_MapLoad.Text = "X";
         // 
         // toolStripStatusLabel_Zoom
         // 
         toolStripStatusLabel_Zoom.Name = "toolStripStatusLabel_Zoom";
         toolStripStatusLabel_Zoom.Size = new Size(39, 19);
         toolStripStatusLabel_Zoom.Text = "Zoom";
         // 
         // toolStripStatusLabel_Pos
         // 
         toolStripStatusLabel_Pos.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
         toolStripStatusLabel_Pos.BorderStyle = Border3DStyle.SunkenInner;
         toolStripStatusLabel_Pos.Name = "toolStripStatusLabel_Pos";
         toolStripStatusLabel_Pos.Size = new Size(30, 19);
         toolStripStatusLabel_Pos.Text = "Pos";
         // 
         // toolStripStatusLabel_TrackMiniInfo
         // 
         toolStripStatusLabel_TrackMiniInfo.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
         toolStripStatusLabel_TrackMiniInfo.BorderStyle = Border3DStyle.SunkenInner;
         toolStripStatusLabel_TrackMiniInfo.Name = "toolStripStatusLabel_TrackMiniInfo";
         toolStripStatusLabel_TrackMiniInfo.Size = new Size(4, 19);
         // 
         // toolStripStatusLabel_TrackInfo
         // 
         toolStripStatusLabel_TrackInfo.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
         toolStripStatusLabel_TrackInfo.BorderStyle = Border3DStyle.SunkenInner;
         toolStripStatusLabel_TrackInfo.ForeColor = SystemColors.GrayText;
         toolStripStatusLabel_TrackInfo.Name = "toolStripStatusLabel_TrackInfo";
         toolStripStatusLabel_TrackInfo.Size = new Size(4, 19);
         // 
         // toolStripStatusLabel_GpxLoad
         // 
         toolStripStatusLabel_GpxLoad.BackColor = Color.FromArgb(255, 192, 192);
         toolStripStatusLabel_GpxLoad.Name = "toolStripStatusLabel_GpxLoad";
         toolStripStatusLabel_GpxLoad.Size = new Size(14, 19);
         toolStripStatusLabel_GpxLoad.Text = "X";
         // 
         // menuStrip1
         // 
         menuStrip1.Dock = DockStyle.None;
         menuStrip1.ImageScalingSize = new Size(20, 20);
         menuStrip1.Items.AddRange(new ToolStripItem[] { ToolStripMenuItemMaps });
         menuStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
         menuStrip1.Location = new Point(3, 0);
         menuStrip1.Name = "menuStrip1";
         menuStrip1.Size = new Size(61, 24);
         menuStrip1.Stretch = false;
         menuStrip1.TabIndex = 2;
         menuStrip1.Text = "menuStrip1";
         // 
         // ToolStripMenuItemMaps
         // 
         ToolStripMenuItemMaps.Name = "ToolStripMenuItemMaps";
         ToolStripMenuItemMaps.Size = new Size(53, 20);
         ToolStripMenuItemMaps.Text = "Karten";
         // 
         // toolStrip_Standard
         // 
         toolStrip_Standard.Dock = DockStyle.None;
         toolStrip_Standard.GripStyle = ToolStripGripStyle.Hidden;
         toolStrip_Standard.ImageScalingSize = new Size(20, 20);
         toolStrip_Standard.Items.AddRange(new ToolStripItem[] { toolStripButton_Config, toolStripSeparator9, toolStripButton_CancelMapLoading, toolStripButton_ReloadMap, toolStripButton_ClearCache, toolStripSeparator3, toolStripButton_OpenGpxfile, toolStripButton_SaveGpxFileExt, toolStripButton_SaveGpxFiles, toolStripButton_SaveWithGarminExt, toolStripButton_CopyMap, toolStripButton_PrintMap, toolStripSeparator20, toolStripButton_ZoomIn, toolStripButton_ZoomOut, toolStripButton_TrackZoom, toolStripSeparator8, toolStripButton_TrackSearch, toolStripSeparator6, toolStripButton_MiniHelp });
         toolStrip_Standard.Location = new Point(64, 0);
         toolStrip_Standard.Name = "toolStrip_Standard";
         toolStrip_Standard.Size = new Size(473, 27);
         toolStrip_Standard.TabIndex = 0;
         // 
         // toolStripButton_Config
         // 
         toolStripButton_Config.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Config.Image = Properties.Resources.props;
         toolStripButton_Config.ImageTransparentColor = Color.Magenta;
         toolStripButton_Config.Name = "toolStripButton_Config";
         toolStripButton_Config.Size = new Size(24, 24);
         toolStripButton_Config.Text = "Einstellungen";
         toolStripButton_Config.Click += toolStripButton_Config_Click;
         // 
         // toolStripSeparator9
         // 
         toolStripSeparator9.Name = "toolStripSeparator9";
         toolStripSeparator9.Size = new Size(6, 27);
         // 
         // toolStripButton_CancelMapLoading
         // 
         toolStripButton_CancelMapLoading.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_CancelMapLoading.Enabled = false;
         toolStripButton_CancelMapLoading.Image = Properties.Resources.cancel;
         toolStripButton_CancelMapLoading.ImageTransparentColor = Color.Magenta;
         toolStripButton_CancelMapLoading.Name = "toolStripButton_CancelMapLoading";
         toolStripButton_CancelMapLoading.Size = new Size(24, 24);
         toolStripButton_CancelMapLoading.Text = "Laden der Karte abbrechen";
         toolStripButton_CancelMapLoading.Click += toolStripButton_CancelMapLoading_Click;
         // 
         // toolStripButton_ReloadMap
         // 
         toolStripButton_ReloadMap.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ReloadMap.Image = Properties.Resources.reload;
         toolStripButton_ReloadMap.ImageTransparentColor = Color.Magenta;
         toolStripButton_ReloadMap.Name = "toolStripButton_ReloadMap";
         toolStripButton_ReloadMap.Size = new Size(24, 24);
         toolStripButton_ReloadMap.Text = "Karte neu zeichnen";
         toolStripButton_ReloadMap.Click += toolStripButton_ReloadMap_Click;
         // 
         // toolStripButton_ClearCache
         // 
         toolStripButton_ClearCache.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ClearCache.Image = Properties.Resources.database_delete;
         toolStripButton_ClearCache.ImageTransparentColor = Color.Magenta;
         toolStripButton_ClearCache.Name = "toolStripButton_ClearCache";
         toolStripButton_ClearCache.Size = new Size(24, 24);
         toolStripButton_ClearCache.Text = "intern gespeicherte Kartendaten löschen";
         toolStripButton_ClearCache.Click += toolStripButton_ClearCache_Click;
         // 
         // toolStripSeparator3
         // 
         toolStripSeparator3.Margin = new Padding(10, 0, 10, 0);
         toolStripSeparator3.Name = "toolStripSeparator3";
         toolStripSeparator3.Size = new Size(6, 27);
         // 
         // toolStripButton_OpenGpxfile
         // 
         toolStripButton_OpenGpxfile.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_OpenGpxfile.Image = Properties.Resources.Open;
         toolStripButton_OpenGpxfile.ImageTransparentColor = Color.Magenta;
         toolStripButton_OpenGpxfile.Name = "toolStripButton_OpenGpxfile";
         toolStripButton_OpenGpxfile.Size = new Size(24, 24);
         toolStripButton_OpenGpxfile.Text = "GPX-Datei öffnen (Strg+X)";
         toolStripButton_OpenGpxfile.Click += toolStripButton_OpenGpxfile_Click;
         // 
         // toolStripButton_SaveGpxFileExt
         // 
         toolStripButton_SaveGpxFileExt.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SaveGpxFileExt.Enabled = false;
         toolStripButton_SaveGpxFileExt.Image = Properties.Resources.speichernu;
         toolStripButton_SaveGpxFileExt.ImageTransparentColor = Color.Magenta;
         toolStripButton_SaveGpxFileExt.Name = "toolStripButton_SaveGpxFileExt";
         toolStripButton_SaveGpxFileExt.Size = new Size(24, 24);
         toolStripButton_SaveGpxFileExt.Text = "angezeigte Objekte speichern unter ...";
         toolStripButton_SaveGpxFileExt.Click += toolStripButton_SaveGpxFileExt_Click;
         // 
         // toolStripButton_SaveGpxFiles
         // 
         toolStripButton_SaveGpxFiles.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SaveGpxFiles.Enabled = false;
         toolStripButton_SaveGpxFiles.Image = Properties.Resources.speichernmulti;
         toolStripButton_SaveGpxFiles.ImageTransparentColor = Color.Magenta;
         toolStripButton_SaveGpxFiles.Name = "toolStripButton_SaveGpxFiles";
         toolStripButton_SaveGpxFiles.Size = new Size(24, 24);
         toolStripButton_SaveGpxFiles.Text = "angezeigte Objekte speichern unter ... als Einzeldateien";
         toolStripButton_SaveGpxFiles.Click += toolStripButton_SaveGpxFiles_Click;
         // 
         // toolStripButton_SaveWithGarminExt
         // 
         toolStripButton_SaveWithGarminExt.Checked = true;
         toolStripButton_SaveWithGarminExt.CheckOnClick = true;
         toolStripButton_SaveWithGarminExt.CheckState = CheckState.Checked;
         toolStripButton_SaveWithGarminExt.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SaveWithGarminExt.Image = Properties.Resources.garmin1;
         toolStripButton_SaveWithGarminExt.ImageTransparentColor = Color.Magenta;
         toolStripButton_SaveWithGarminExt.Name = "toolStripButton_SaveWithGarminExt";
         toolStripButton_SaveWithGarminExt.Size = new Size(24, 24);
         toolStripButton_SaveWithGarminExt.Text = "mit Garminerweiterungen speichern";
         // 
         // toolStripButton_CopyMap
         // 
         toolStripButton_CopyMap.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_CopyMap.Image = Properties.Resources.copy;
         toolStripButton_CopyMap.ImageTransparentColor = Color.Magenta;
         toolStripButton_CopyMap.Name = "toolStripButton_CopyMap";
         toolStripButton_CopyMap.Size = new Size(24, 24);
         toolStripButton_CopyMap.Text = "Karte in Zwischenablage kopieren ...";
         toolStripButton_CopyMap.Click += toolStripButton_CopyMap_Click;
         // 
         // toolStripButton_PrintMap
         // 
         toolStripButton_PrintMap.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_PrintMap.Image = Properties.Resources.printer;
         toolStripButton_PrintMap.ImageTransparentColor = Color.Magenta;
         toolStripButton_PrintMap.Name = "toolStripButton_PrintMap";
         toolStripButton_PrintMap.Size = new Size(24, 24);
         toolStripButton_PrintMap.Text = "Karte drucken";
         toolStripButton_PrintMap.Click += toolStripButton_PrintMap_Click;
         // 
         // toolStripSeparator20
         // 
         toolStripSeparator20.Margin = new Padding(10, 0, 10, 0);
         toolStripSeparator20.Name = "toolStripSeparator20";
         toolStripSeparator20.Size = new Size(6, 27);
         // 
         // toolStripButton_ZoomIn
         // 
         toolStripButton_ZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ZoomIn.Image = Properties.Resources.zoom_in;
         toolStripButton_ZoomIn.ImageTransparentColor = Color.Magenta;
         toolStripButton_ZoomIn.Name = "toolStripButton_ZoomIn";
         toolStripButton_ZoomIn.Size = new Size(24, 24);
         toolStripButton_ZoomIn.Text = "hineinzoomen (Strg+ +)";
         toolStripButton_ZoomIn.Click += toolStripButton_ZoomIn_Click;
         // 
         // toolStripButton_ZoomOut
         // 
         toolStripButton_ZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_ZoomOut.Image = Properties.Resources.zoom_out;
         toolStripButton_ZoomOut.ImageTransparentColor = Color.Magenta;
         toolStripButton_ZoomOut.Name = "toolStripButton_ZoomOut";
         toolStripButton_ZoomOut.Size = new Size(24, 24);
         toolStripButton_ZoomOut.Text = "herauszoomen (Strg+ -)";
         toolStripButton_ZoomOut.Click += toolStripButton_ZoomOut_Click;
         // 
         // toolStripButton_TrackZoom
         // 
         toolStripButton_TrackZoom.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_TrackZoom.Image = Properties.Resources.zoom1;
         toolStripButton_TrackZoom.ImageTransparentColor = Color.Magenta;
         toolStripButton_TrackZoom.Name = "toolStripButton_TrackZoom";
         toolStripButton_TrackZoom.Size = new Size(24, 24);
         toolStripButton_TrackZoom.Text = "Zoom auf angezeigte Tracks";
         toolStripButton_TrackZoom.Click += toolStripButton_TrackZoom_Click;
         // 
         // toolStripSeparator8
         // 
         toolStripSeparator8.Margin = new Padding(10, 0, 10, 0);
         toolStripSeparator8.Name = "toolStripSeparator8";
         toolStripSeparator8.Size = new Size(6, 27);
         // 
         // toolStripButton_TrackSearch
         // 
         toolStripButton_TrackSearch.CheckOnClick = true;
         toolStripButton_TrackSearch.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_TrackSearch.Image = Properties.Resources.Search;
         toolStripButton_TrackSearch.ImageTransparentColor = Color.Magenta;
         toolStripButton_TrackSearch.Name = "toolStripButton_TrackSearch";
         toolStripButton_TrackSearch.Size = new Size(24, 24);
         toolStripButton_TrackSearch.Text = "Tracks im markierten Bereich suchen";
         toolStripButton_TrackSearch.Click += toolStripButton_TrackSearch_Click;
         // 
         // toolStripSeparator6
         // 
         toolStripSeparator6.Margin = new Padding(10, 0, 10, 0);
         toolStripSeparator6.Name = "toolStripSeparator6";
         toolStripSeparator6.Size = new Size(6, 27);
         // 
         // toolStripButton_MiniHelp
         // 
         toolStripButton_MiniHelp.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_MiniHelp.Image = Properties.Resources.help;
         toolStripButton_MiniHelp.ImageTransparentColor = Color.Magenta;
         toolStripButton_MiniHelp.Name = "toolStripButton_MiniHelp";
         toolStripButton_MiniHelp.Size = new Size(24, 24);
         toolStripButton_MiniHelp.Text = "Hilfe";
         toolStripButton_MiniHelp.Click += toolStripButton_MiniHelp_Click;
         // 
         // colorDialog1
         // 
         colorDialog1.AnyColor = true;
         colorDialog1.FullOpen = true;
         // 
         // openFileDialogGpx
         // 
         openFileDialogGpx.Filter = "GPX-Dateien|*.gpx|GDB-Dateien|*.gdb|alle Dateien|*.*";
         openFileDialogGpx.Title = "GPX-Datei öffnen";
         // 
         // saveFileDialogGpx
         // 
         saveFileDialogGpx.DefaultExt = "gpx";
         saveFileDialogGpx.Filter = "Gpx-Dateien|*.gpx|KMZ-Dateien|*.kmz|KML-Dateien|*.kml";
         saveFileDialogGpx.OverwritePrompt = false;
         saveFileDialogGpx.Title = "speichern unter ...";
         // 
         // contextMenuStripEditableGroupOrNothing
         // 
         contextMenuStripEditableGroupOrNothing.ImageScalingSize = new Size(20, 20);
         contextMenuStripEditableGroupOrNothing.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_EditableGroupInsert2, ToolStripMenuItem_EditableGroupDelete2, toolStripSeparator17, ToolStripMenuItem_ShowAllEditableObjects2, ToolStripMenuItem_HideAllEditableObjects2, ToolStripMenuItem_DeleteAllVisibleEditableObjects2 });
         contextMenuStripEditableGroupOrNothing.Name = "contextMenuStripTrack";
         contextMenuStripEditableGroupOrNothing.Size = new Size(266, 140);
         contextMenuStripEditableGroupOrNothing.Opening += contextMenuStripEditableGroupOrNothing_Opening;
         // 
         // ToolStripMenuItem_EditableGroupInsert2
         // 
         ToolStripMenuItem_EditableGroupInsert2.Image = Properties.Resources.Open;
         ToolStripMenuItem_EditableGroupInsert2.Name = "ToolStripMenuItem_EditableGroupInsert2";
         ToolStripMenuItem_EditableGroupInsert2.Size = new Size(265, 26);
         ToolStripMenuItem_EditableGroupInsert2.Text = "Gruppe anlegen";
         ToolStripMenuItem_EditableGroupInsert2.Click += ToolStripMenuItem_EditableGroupInsert_Click;
         // 
         // ToolStripMenuItem_EditableGroupDelete2
         // 
         ToolStripMenuItem_EditableGroupDelete2.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_EditableGroupDelete2.Image = Properties.Resources.delete;
         ToolStripMenuItem_EditableGroupDelete2.Name = "ToolStripMenuItem_EditableGroupDelete2";
         ToolStripMenuItem_EditableGroupDelete2.Size = new Size(265, 26);
         ToolStripMenuItem_EditableGroupDelete2.Text = "gesamte (!) Gruppe löschen";
         ToolStripMenuItem_EditableGroupDelete2.Click += ToolStripMenuItem_EditableGroupDelete_Click;
         // 
         // toolStripSeparator17
         // 
         toolStripSeparator17.Name = "toolStripSeparator17";
         toolStripSeparator17.Size = new Size(262, 6);
         // 
         // ToolStripMenuItem_ShowAllEditableObjects2
         // 
         ToolStripMenuItem_ShowAllEditableObjects2.Name = "ToolStripMenuItem_ShowAllEditableObjects2";
         ToolStripMenuItem_ShowAllEditableObjects2.Size = new Size(265, 26);
         ToolStripMenuItem_ShowAllEditableObjects2.Text = "alle Objekte anzeigen";
         ToolStripMenuItem_ShowAllEditableObjects2.Click += ToolStripMenuItem_ShowAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_HideAllEditableObjects2
         // 
         ToolStripMenuItem_HideAllEditableObjects2.Name = "ToolStripMenuItem_HideAllEditableObjects2";
         ToolStripMenuItem_HideAllEditableObjects2.Size = new Size(265, 26);
         ToolStripMenuItem_HideAllEditableObjects2.Text = "alle Objekte verbergen";
         ToolStripMenuItem_HideAllEditableObjects2.Click += ToolStripMenuItem_HideAllEditableObjects_Click;
         // 
         // ToolStripMenuItem_DeleteAllVisibleEditableObjects2
         // 
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.ForeColor = Color.FromArgb(192, 0, 0);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Image = Properties.Resources.delete;
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Name = "ToolStripMenuItem_DeleteAllVisibleEditableObjects2";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Size = new Size(265, 26);
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Text = "alle angezeigten (!) Objekte löschen";
         ToolStripMenuItem_DeleteAllVisibleEditableObjects2.Click += ToolStripMenuItem_DeleteAllVisibleEditableObjects_Click;
         // 
         // contextMenuStripReadOnlyMarker
         // 
         contextMenuStripReadOnlyMarker.ImageScalingSize = new Size(20, 20);
         contextMenuStripReadOnlyMarker.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_ShowMarkerProperties, toolStripMenuItem_CloneMarker });
         contextMenuStripReadOnlyMarker.Name = "contextMenuStripEditMarker";
         contextMenuStripReadOnlyMarker.Size = new Size(229, 56);
         contextMenuStripReadOnlyMarker.Opening += contextMenuStripReadOnlyMarker_Opening;
         // 
         // toolStripMenuItem_ShowMarkerProperties
         // 
         toolStripMenuItem_ShowMarkerProperties.Image = Properties.Resources.edit;
         toolStripMenuItem_ShowMarkerProperties.Name = "toolStripMenuItem_ShowMarkerProperties";
         toolStripMenuItem_ShowMarkerProperties.Size = new Size(228, 26);
         toolStripMenuItem_ShowMarkerProperties.Text = "Eigenschaften anzeigen ";
         toolStripMenuItem_ShowMarkerProperties.Click += toolStripMenuItem_ShowMarkerProperties_Click;
         // 
         // toolStripMenuItem_CloneMarker
         // 
         toolStripMenuItem_CloneMarker.Image = Properties.Resources.kopie;
         toolStripMenuItem_CloneMarker.Name = "toolStripMenuItem_CloneMarker";
         toolStripMenuItem_CloneMarker.Size = new Size(228, 26);
         toolStripMenuItem_CloneMarker.Text = "bearbeitbare Kopie erzeugen";
         toolStripMenuItem_CloneMarker.Click += toolStripMenuItem_CloneMarker_Click;
         // 
         // toolStripContainer3
         // 
         // 
         // toolStripContainer3.ContentPanel
         // 
         toolStripContainer3.ContentPanel.Size = new Size(1058, 636);
         toolStripContainer3.Dock = DockStyle.Fill;
         toolStripContainer3.Location = new Point(0, 0);
         toolStripContainer3.Name = "toolStripContainer3";
         toolStripContainer3.Size = new Size(1058, 661);
         toolStripContainer3.TabIndex = 0;
         toolStripContainer3.Text = "toolStripContainer3";
         // 
         // FormMain
         // 
         AllowDrop = true;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(1058, 661);
         Controls.Add(toolStripContainer1);
         Controls.Add(toolStripContainer3);
         Icon = (Icon)resources.GetObject("$this.Icon");
         KeyPreview = true;
         MainMenuStrip = menuStrip1;
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormMain";
         Text = "Form1";
         FormClosing += FormMain_FormClosing;
         Load += FormMain_Load;
         Shown += FormMain_Shown;
         KeyDown += FormMain_KeyDown;
         panelMap.ResumeLayout(false);
         panelMap.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)trackBarZoom).EndInit();
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
         splitContainer1.ResumeLayout(false);
         tabControl1.ResumeLayout(false);
         tabPageFiles.ResumeLayout(false);
         contextMenuStripReadOnlyTracks.ResumeLayout(false);
         contextMenuStripReadOnlyTracks.PerformLayout();
         tabPageEditable.ResumeLayout(false);
         toolStripContainer2.ContentPanel.ResumeLayout(false);
         toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
         toolStripContainer2.TopToolStripPanel.PerformLayout();
         toolStripContainer2.ResumeLayout(false);
         toolStripContainer2.PerformLayout();
         toolStrip_Edit.ResumeLayout(false);
         toolStrip_Edit.PerformLayout();
         tabPageSearch.ResumeLayout(false);
         tabPageLocation.ResumeLayout(false);
         splitContainer2.Panel1.ResumeLayout(false);
         splitContainer2.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
         splitContainer2.ResumeLayout(false);
         tabPageFoto.ResumeLayout(false);
         contextMenuStripEditableTracks.ResumeLayout(false);
         contextMenuStripEditableTracks.PerformLayout();
         contextMenuStripEditableMarker.ResumeLayout(false);
         toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
         toolStripContainer1.BottomToolStripPanel.PerformLayout();
         toolStripContainer1.ContentPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.PerformLayout();
         toolStripContainer1.ResumeLayout(false);
         toolStripContainer1.PerformLayout();
         statusStrip1.ResumeLayout(false);
         statusStrip1.PerformLayout();
         menuStrip1.ResumeLayout(false);
         menuStrip1.PerformLayout();
         toolStrip_Standard.ResumeLayout(false);
         toolStrip_Standard.PerformLayout();
         contextMenuStripEditableGroupOrNothing.ResumeLayout(false);
         contextMenuStripReadOnlyMarker.ResumeLayout(false);
         toolStripContainer3.ResumeLayout(false);
         toolStripContainer3.PerformLayout();
         ResumeLayout(false);
      }

      #endregion
      private System.Windows.Forms.Panel panelMap;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ToolTip toolTipRouteInfo;
      private System.Windows.Forms.ToolStripContainer toolStripContainer1;
      private System.Windows.Forms.ToolStrip toolStrip_Standard;
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_MapLoad;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Pos;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_TrackInfo;
      private System.Windows.Forms.ToolStripButton toolStripButton_ReloadMap;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackZoom;
      private System.Windows.Forms.ColorDialog colorDialog1;
      private System.Windows.Forms.ToolStripButton toolStripButton_OpenGpxfile;
      private System.Windows.Forms.OpenFileDialog openFileDialogGpx;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripEditableMarker;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointEdit;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointSet;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointDelete;
      private System.Windows.Forms.TabControl tabControl1;
      private System.Windows.Forms.TabPage tabPageFiles;
      private System.Windows.Forms.ToolStripButton toolStripButton_ViewerMode;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
      private System.Windows.Forms.ToolStripButton toolStripButton_CopyMap;
      private System.Windows.Forms.SaveFileDialog saveFileDialogGpx;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointClone;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveGpxFileExt;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointZoom;
      private System.Windows.Forms.ToolStripButton toolStripButton_ZoomIn;
      private System.Windows.Forms.ToolStripButton toolStripButton_ZoomOut;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
      private System.Windows.Forms.ToolStripButton toolStripButton_MiniHelp;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Zoom;
      private SpecialMapCtrl.SpecialMapCtrl mapCtrl;
      private System.Windows.Forms.ToolStripButton toolStripButton_SetMarker;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackDraw;
      private System.Windows.Forms.ToolStripButton toolStripButton_EditEnd;
      private System.Windows.Forms.ToolStripButton toolStripButton_PrintMap;
      private System.Windows.Forms.ToolStripButton toolStripButton_ClearEditable;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripReadOnlyTracks;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackShow;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackZoom;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyGpxShowMarker;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyGpxShowPictureMarker;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackInfo;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackExtInfo;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackColor;
      private NumericUpDownMenuItem numericUpDownMenuItem_ReadOnlyLineThickness;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackClone;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyGpxRemove;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripEditableTracks;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackShow;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackZoom;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackInfo;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackExtInfo;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackColor;
      private NumericUpDownMenuItem numericUpDownMenuItem_EditableLineThickness;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackClone;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackDraw;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackSplit;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackAppend;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackReverse;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackDelete;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_GpxLoad;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointShow;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTracksHide;
      private System.Windows.Forms.ToolStripButton toolStripButton_ClearCache;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveWithGarminExt;
      private System.Windows.Forms.ToolStripButton toolStripButton_UniqueNames;
      private ReadOnlyGpxControl readOnlyTracklistControl1;
      private System.Windows.Forms.TabPage tabPageEditable;
      private EditableGpxControl editableTracklistControl1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_TrackMiniInfo;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_DeleteAllVisibleEditableObjects3;
      private System.Windows.Forms.ToolStrip toolStrip_Edit;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemMaps;
      private System.Windows.Forms.ToolStripSeparator xToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowAllEditableObjects3;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_HideAllEditableObjects3;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableTrackSimplify;
      private System.Windows.Forms.ToolStripButton toolStripButton_CancelMapLoading;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveGpxFiles;
      private System.Windows.Forms.ToolStripContainer toolStripContainer2;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReadOnlyTrackShowSlope;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_EditableTrackShowSlope;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemExtra;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuIemConfig;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripEditableGroupOrNothing;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupInsert2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupDelete2;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowAllEditableObjects2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_HideAllEditableObjects2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_DeleteAllVisibleEditableObjects2;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripReadOnlyMarker;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ShowMarkerProperties;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_CloneMarker;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowAllEditableObjects1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_HideAllEditableObjects1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_DeleteAllVisibleEditableObjects1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupInsert1;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupDelete1;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupInsert3;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_EditableGroupDelete3;
      private TabPage tabPageFoto;
      private PictureManager pictureManager1;
      private TabPage tabPageLocation;
      private SplitContainer splitContainer2;
      private ToolStripContainer toolStripContainer3;
      private LocationControl locationControl1;
      private GeoLocationControl geoLocationControl1;
      private TabPage tabPageSearch;
      private SearchControl searchControl1;
      private ToolStripMenuItem toolStripMenuItem_EditableTrackPointRemove;
      private ToolStripButton toolStripButton_EditCancel;
      private ToolStripButton toolStripButton_Config;
      private ToolStripSeparator toolStripSeparator9;
      private TrackBar trackBarZoom;
   }
}

