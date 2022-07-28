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
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
         this.panelMap = new System.Windows.Forms.Panel();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.tabControl1 = new System.Windows.Forms.TabControl();
         this.tabPageFiles = new System.Windows.Forms.TabPage();
         this.readOnlyTracklistControl1 = new GpxViewer.ReadOnlyTracklistControl();
         this.contextMenuStripReadOnlyTracks = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.toolStripMenuItem_ReadOnlyTrackShow = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyTrackZoom = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyGpxShowMarker = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyTrackInfo = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyTrackExtInfo = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripMenuItem_ReadOnlyTracksHide = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripMenuItem_ReadOnlyTrackColor = new System.Windows.Forms.ToolStripMenuItem();
         this.numericUpDownMenuItem_ReadOnlyLineThickness = new GpxViewer.NumericUpDownMenuItem();
         this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripMenuItem_ReadOnlyTrackClone = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_ReadOnlyGpxRemove = new System.Windows.Forms.ToolStripMenuItem();
         this.tabPageEditable = new System.Windows.Forms.TabPage();
         this.editableTracklistControl1 = new GpxViewer.EditableTracklistControl();
         this.mapControl1 = new GpxViewer.MapControl();
         this.contextMenuStripEditableTracks = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.toolStripMenuItem_EditableTrackDraw = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackSplit = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackAppend = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackReverse = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackClone = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackDelete = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripMenuItem_EditableTrackShow = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackZoom = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackInfo = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_EditableTrackExtInfo = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripMenuItem_EditableTrackColor = new System.Windows.Forms.ToolStripMenuItem();
         this.numericUpDownMenuItem_EditableLineThickness = new GpxViewer.NumericUpDownMenuItem();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.ToolStripMenuItem_RemoveAllEditableTracks = new System.Windows.Forms.ToolStripMenuItem();
         this.contextMenuStripMarker = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItem_WaypointZoom = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_WaypointShow = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_WaypointEdit = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_WaypointClone = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_WaypointSet = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_WaypointDelete = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_RemoveAllEditableMarkers = new System.Windows.Forms.ToolStripMenuItem();
         this.toolTipRouteInfo = new System.Windows.Forms.ToolTip(this.components);
         this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.toolStripStatusLabel_MapLoad = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_Zoom = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_Pos = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_TrackMiniInfo = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_EditInfo = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_GpxLoad = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStrip_Edit = new System.Windows.Forms.ToolStrip();
         this.toolStripButton_ViewerMode = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SetMarker = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_TrackDraw = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_TrackDrawEnd = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_ClearEditable = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_UniqueNames = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
         this.toolStrip_Standard = new System.Windows.Forms.ToolStrip();
         this.toolStripComboBoxMapSource = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripButton_ReloadMap = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_ClearCache = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton_OpenGpxfile = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SaveGpxFile = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SaveGpxFileExt = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SaveWithGarminExt = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_CopyMap = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_PrintMap = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_GeoTagging = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton_ZoomIn = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_ZoomOut = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_TrackZoom = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton_LocationForm = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_GeoSearch = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_TrackSearch = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton_MiniHelp = new System.Windows.Forms.ToolStripButton();
         this.colorDialog1 = new System.Windows.Forms.ColorDialog();
         this.openFileDialogGpx = new System.Windows.Forms.OpenFileDialog();
         this.saveFileDialogGpx = new System.Windows.Forms.SaveFileDialog();
         this.panelMap.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.tabControl1.SuspendLayout();
         this.tabPageFiles.SuspendLayout();
         this.contextMenuStripReadOnlyTracks.SuspendLayout();
         this.tabPageEditable.SuspendLayout();
         this.contextMenuStripEditableTracks.SuspendLayout();
         this.contextMenuStripMarker.SuspendLayout();
         this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
         this.toolStripContainer1.ContentPanel.SuspendLayout();
         this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
         this.toolStripContainer1.SuspendLayout();
         this.statusStrip1.SuspendLayout();
         this.toolStrip_Edit.SuspendLayout();
         this.toolStrip_Standard.SuspendLayout();
         this.SuspendLayout();
         // 
         // panelMap
         // 
         this.panelMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.panelMap.Controls.Add(this.splitContainer1);
         this.panelMap.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panelMap.Location = new System.Drawing.Point(0, 0);
         this.panelMap.Name = "panelMap";
         this.panelMap.Size = new System.Drawing.Size(1055, 689);
         this.panelMap.TabIndex = 2;
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.mapControl1);
         this.splitContainer1.Size = new System.Drawing.Size(1053, 687);
         this.splitContainer1.SplitterDistance = 275;
         this.splitContainer1.TabIndex = 0;
         // 
         // tabControl1
         // 
         this.tabControl1.Controls.Add(this.tabPageFiles);
         this.tabControl1.Controls.Add(this.tabPageEditable);
         this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tabControl1.Location = new System.Drawing.Point(0, 0);
         this.tabControl1.Name = "tabControl1";
         this.tabControl1.SelectedIndex = 0;
         this.tabControl1.Size = new System.Drawing.Size(275, 687);
         this.tabControl1.TabIndex = 8;
         // 
         // tabPageFiles
         // 
         this.tabPageFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.tabPageFiles.Controls.Add(this.readOnlyTracklistControl1);
         this.tabPageFiles.Location = new System.Drawing.Point(4, 22);
         this.tabPageFiles.Name = "tabPageFiles";
         this.tabPageFiles.Padding = new System.Windows.Forms.Padding(3);
         this.tabPageFiles.Size = new System.Drawing.Size(267, 661);
         this.tabPageFiles.TabIndex = 0;
         this.tabPageFiles.Text = "Dateien";
         // 
         // readOnlyTracklistControl1
         // 
         this.readOnlyTracklistControl1.AllowDrop = true;
         this.readOnlyTracklistControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.readOnlyTracklistControl1.ContextMenuStrip = this.contextMenuStripReadOnlyTracks;
         this.readOnlyTracklistControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.readOnlyTracklistControl1.LoadGpxfilesCancel = false;
         this.readOnlyTracklistControl1.Location = new System.Drawing.Point(3, 3);
         this.readOnlyTracklistControl1.Name = "readOnlyTracklistControl1";
         this.readOnlyTracklistControl1.Size = new System.Drawing.Size(261, 655);
         this.readOnlyTracklistControl1.TabIndex = 8;
         this.readOnlyTracklistControl1.SelectGpxEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ChooseEventArgs>(this.readOnlyTracklistControl1_SelectGpxEvent);
         this.readOnlyTracklistControl1.SelectTrackEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ChooseEventArgs>(this.readOnlyTracklistControl1_SelectTrackEvent);
         this.readOnlyTracklistControl1.ChooseGpxEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ChooseEventArgs>(this.readOnlyTracklistControl1_ChooseGpxEvent);
         this.readOnlyTracklistControl1.ChooseTrackEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ChooseEventArgs>(this.readOnlyTracklistControl1_ChooseTrackEvent);
         this.readOnlyTracklistControl1.LoadinfoEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.SendStringEventArgs>(this.readOnlyTracklistControl1_LoadinfoEvent);
         this.readOnlyTracklistControl1.ShowTrackEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ShowTrackEventArgs>(this.readOnlyTracklistControl1_ShowTrackEvent);
         this.readOnlyTracklistControl1.ShowAllMarkerEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ShowMarkerEventArgs>(this.readOnlyTracklistControl1_ShowAllMarkerEvent);
         this.readOnlyTracklistControl1.ShowAllFotoMarkerEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.ShowMarkerEventArgs>(this.readOnlyTracklistControl1_ShowAllFotoMarkerEvent);
         this.readOnlyTracklistControl1.RefreshProgramStateEvent += new System.EventHandler<System.EventArgs>(this.readOnlyTracklistControl1_RefreshProgramStateEvent);
         this.readOnlyTracklistControl1.ShowExceptionEvent += new System.EventHandler<GpxViewer.ReadOnlyTracklistControl.SendExceptionEventArgs>(this.readOnlyTracklistControl1_ShowExceptionEvent);
         // 
         // contextMenuStripReadOnlyTracks
         // 
         this.contextMenuStripReadOnlyTracks.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_ReadOnlyTrackShow,
            this.toolStripMenuItem_ReadOnlyTrackZoom,
            this.toolStripMenuItem_ReadOnlyGpxShowMarker,
            this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker,
            this.toolStripMenuItem_ReadOnlyTrackInfo,
            this.toolStripMenuItem_ReadOnlyTrackExtInfo,
            this.toolStripSeparator1,
            this.toolStripMenuItem_ReadOnlyTracksHide,
            this.toolStripSeparator10,
            this.toolStripMenuItem_ReadOnlyTrackColor,
            this.numericUpDownMenuItem_ReadOnlyLineThickness,
            this.toolStripSeparator11,
            this.toolStripMenuItem_ReadOnlyTrackClone,
            this.toolStripMenuItem_ReadOnlyGpxRemove});
         this.contextMenuStripReadOnlyTracks.Name = "contextMenuStripTrack";
         this.contextMenuStripReadOnlyTracks.Size = new System.Drawing.Size(265, 275);
         this.contextMenuStripReadOnlyTracks.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStripReadOnlyTracks_Closed);
         this.contextMenuStripReadOnlyTracks.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripReadOnlyTracks_Opening);
         // 
         // toolStripMenuItem_ReadOnlyTrackShow
         // 
         this.toolStripMenuItem_ReadOnlyTrackShow.Image = global::GpxViewer.Properties.Resources.Track;
         this.toolStripMenuItem_ReadOnlyTrackShow.Name = "toolStripMenuItem_ReadOnlyTrackShow";
         this.toolStripMenuItem_ReadOnlyTrackShow.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackShow.Text = "Track &anzeigen";
         this.toolStripMenuItem_ReadOnlyTrackShow.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackShow_Click);
         // 
         // toolStripMenuItem_ReadOnlyTrackZoom
         // 
         this.toolStripMenuItem_ReadOnlyTrackZoom.Image = global::GpxViewer.Properties.Resources.zoom1;
         this.toolStripMenuItem_ReadOnlyTrackZoom.Name = "toolStripMenuItem_ReadOnlyTrackZoom";
         this.toolStripMenuItem_ReadOnlyTrackZoom.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackZoom.Text = "&Zoom auf diesen Track";
         this.toolStripMenuItem_ReadOnlyTrackZoom.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackZoom_Click);
         // 
         // toolStripMenuItem_ReadOnlyGpxShowMarker
         // 
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.Checked = true;
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.CheckOnClick = true;
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.CheckState = System.Windows.Forms.CheckState.Checked;
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.Name = "toolStripMenuItem_ReadOnlyGpxShowMarker";
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.Text = "&Wegpunkte auch anzeigen";
         this.toolStripMenuItem_ReadOnlyGpxShowMarker.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyGpxShowMarker_Click);
         // 
         // toolStripMenuItem_ReadOnlyGpxShowPictureMarker
         // 
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.CheckOnClick = true;
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Image = global::GpxViewer.Properties.Resources.Foto;
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Name = "toolStripMenuItem_ReadOnlyGpxShowPictureMarker";
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Text = "&Bildwegpunkte auch anzeigen";
         this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyGpxShowPictureMarker_Click);
         // 
         // toolStripMenuItem_ReadOnlyTrackInfo
         // 
         this.toolStripMenuItem_ReadOnlyTrackInfo.Image = global::GpxViewer.Properties.Resources.info;
         this.toolStripMenuItem_ReadOnlyTrackInfo.Name = "toolStripMenuItem_ReadOnlyTrackInfo";
         this.toolStripMenuItem_ReadOnlyTrackInfo.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackInfo.Text = "&Info anzeigen";
         this.toolStripMenuItem_ReadOnlyTrackInfo.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackInfo_Click);
         // 
         // toolStripMenuItem_ReadOnlyTrackExtInfo
         // 
         this.toolStripMenuItem_ReadOnlyTrackExtInfo.Image = global::GpxViewer.Properties.Resources.edit;
         this.toolStripMenuItem_ReadOnlyTrackExtInfo.Name = "toolStripMenuItem_ReadOnlyTrackExtInfo";
         this.toolStripMenuItem_ReadOnlyTrackExtInfo.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackExtInfo.Text = "&erweiterte Infos anzeigen";
         this.toolStripMenuItem_ReadOnlyTrackExtInfo.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackExtInfo_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(261, 6);
         // 
         // toolStripMenuItem_ReadOnlyTracksHide
         // 
         this.toolStripMenuItem_ReadOnlyTracksHide.Name = "toolStripMenuItem_ReadOnlyTracksHide";
         this.toolStripMenuItem_ReadOnlyTracksHide.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTracksHide.Text = "alle (!) angezeigten Tracks &verbergen";
         this.toolStripMenuItem_ReadOnlyTracksHide.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTracksHide_Click);
         // 
         // toolStripSeparator10
         // 
         this.toolStripSeparator10.Name = "toolStripSeparator10";
         this.toolStripSeparator10.Size = new System.Drawing.Size(261, 6);
         // 
         // toolStripMenuItem_ReadOnlyTrackColor
         // 
         this.toolStripMenuItem_ReadOnlyTrackColor.BackColor = System.Drawing.SystemColors.Control;
         this.toolStripMenuItem_ReadOnlyTrackColor.Name = "toolStripMenuItem_ReadOnlyTrackColor";
         this.toolStripMenuItem_ReadOnlyTrackColor.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackColor.Text = "&Trackfarbe ändern";
         this.toolStripMenuItem_ReadOnlyTrackColor.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackColor_Click);
         // 
         // numericUpDownMenuItem_ReadOnlyLineThickness
         // 
         this.numericUpDownMenuItem_ReadOnlyLineThickness.BackColor = System.Drawing.SystemColors.Control;
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Name = "numericUpDownMenuItem_ReadOnlyLineThickness";
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Size = new System.Drawing.Size(133, 30);
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Text = "Liniendicke";
         this.numericUpDownMenuItem_ReadOnlyLineThickness.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
         // 
         // toolStripSeparator11
         // 
         this.toolStripSeparator11.Name = "toolStripSeparator11";
         this.toolStripSeparator11.Size = new System.Drawing.Size(261, 6);
         // 
         // toolStripMenuItem_ReadOnlyTrackClone
         // 
         this.toolStripMenuItem_ReadOnlyTrackClone.Image = global::GpxViewer.Properties.Resources.kopie;
         this.toolStripMenuItem_ReadOnlyTrackClone.Name = "toolStripMenuItem_ReadOnlyTrackClone";
         this.toolStripMenuItem_ReadOnlyTrackClone.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyTrackClone.Text = "&bearbeitbare Kopie erzeugen";
         this.toolStripMenuItem_ReadOnlyTrackClone.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyTrackClone_Click);
         // 
         // toolStripMenuItem_ReadOnlyGpxRemove
         // 
         this.toolStripMenuItem_ReadOnlyGpxRemove.Image = global::GpxViewer.Properties.Resources.delete;
         this.toolStripMenuItem_ReadOnlyGpxRemove.Name = "toolStripMenuItem_ReadOnlyGpxRemove";
         this.toolStripMenuItem_ReadOnlyGpxRemove.Size = new System.Drawing.Size(264, 22);
         this.toolStripMenuItem_ReadOnlyGpxRemove.Text = "GPX-Datei aus der Liste entfernen";
         this.toolStripMenuItem_ReadOnlyGpxRemove.Click += new System.EventHandler(this.toolStripMenuItem_ReadOnlyGpxRemove_Click);
         // 
         // tabPageEditable
         // 
         this.tabPageEditable.Controls.Add(this.editableTracklistControl1);
         this.tabPageEditable.Location = new System.Drawing.Point(4, 22);
         this.tabPageEditable.Name = "tabPageEditable";
         this.tabPageEditable.Size = new System.Drawing.Size(267, 634);
         this.tabPageEditable.TabIndex = 3;
         this.tabPageEditable.Text = "neue Tracks/Marker";
         this.tabPageEditable.UseVisualStyleBackColor = true;
         // 
         // editableTracklistControl1
         // 
         this.editableTracklistControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.editableTracklistControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.editableTracklistControl1.Location = new System.Drawing.Point(0, 0);
         this.editableTracklistControl1.Name = "editableTracklistControl1";
         this.editableTracklistControl1.Size = new System.Drawing.Size(267, 634);
         this.editableTracklistControl1.TabIndex = 0;
         this.editableTracklistControl1.SelectTrackEvent += new System.EventHandler<GpxViewer.EditableTracklistControl.IdxEventArgs>(this.editableTracklistControl1_SelectTrackEvent);
         // 
         // mapControl1
         // 
         this.mapControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.mapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.mapControl1.Location = new System.Drawing.Point(0, 0);
         this.mapControl1.MapCursor = System.Windows.Forms.Cursors.Default;
         this.mapControl1.MapDragButton = System.Windows.Forms.MouseButtons.Right;
         this.mapControl1.MapMinZoom = 3;
         this.mapControl1.Name = "mapControl1";
         this.mapControl1.Size = new System.Drawing.Size(774, 687);
         this.mapControl1.TabIndex = 1;
         // 
         // contextMenuStripEditableTracks
         // 
         this.contextMenuStripEditableTracks.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_EditableTrackDraw,
            this.toolStripMenuItem_EditableTrackSplit,
            this.toolStripMenuItem_EditableTrackAppend,
            this.toolStripMenuItem_EditableTrackReverse,
            this.toolStripMenuItem_EditableTrackClone,
            this.toolStripMenuItem_EditableTrackDelete,
            this.toolStripSeparator15,
            this.toolStripMenuItem_EditableTrackShow,
            this.toolStripMenuItem_EditableTrackZoom,
            this.toolStripMenuItem_EditableTrackInfo,
            this.toolStripMenuItem_EditableTrackExtInfo,
            this.toolStripSeparator12,
            this.toolStripMenuItem_EditableTrackColor,
            this.numericUpDownMenuItem_EditableLineThickness,
            this.toolStripSeparator2,
            this.ToolStripMenuItem_RemoveAllEditableTracks});
         this.contextMenuStripEditableTracks.Name = "contextMenuStripTrack";
         this.contextMenuStripEditableTracks.Size = new System.Drawing.Size(218, 319);
         this.contextMenuStripEditableTracks.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStripEditableTracks_Closed);
         this.contextMenuStripEditableTracks.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripEditableTracks_Opening);
         // 
         // toolStripMenuItem_EditableTrackDraw
         // 
         this.toolStripMenuItem_EditableTrackDraw.Image = global::GpxViewer.Properties.Resources.TrackDraw;
         this.toolStripMenuItem_EditableTrackDraw.Name = "toolStripMenuItem_EditableTrackDraw";
         this.toolStripMenuItem_EditableTrackDraw.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackDraw.Text = "Track &weiter zeichnen";
         this.toolStripMenuItem_EditableTrackDraw.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackDraw_Click);
         // 
         // toolStripMenuItem_EditableTrackSplit
         // 
         this.toolStripMenuItem_EditableTrackSplit.Image = global::GpxViewer.Properties.Resources.TrackSplit;
         this.toolStripMenuItem_EditableTrackSplit.Name = "toolStripMenuItem_EditableTrackSplit";
         this.toolStripMenuItem_EditableTrackSplit.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackSplit.Text = "Track &trennen";
         this.toolStripMenuItem_EditableTrackSplit.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackSplit_Click);
         // 
         // toolStripMenuItem_EditableTrackAppend
         // 
         this.toolStripMenuItem_EditableTrackAppend.Image = global::GpxViewer.Properties.Resources.TrackConcat;
         this.toolStripMenuItem_EditableTrackAppend.Name = "toolStripMenuItem_EditableTrackAppend";
         this.toolStripMenuItem_EditableTrackAppend.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackAppend.Text = "anderen Track &anhängen";
         this.toolStripMenuItem_EditableTrackAppend.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackAppend_Click);
         // 
         // toolStripMenuItem_EditableTrackReverse
         // 
         this.toolStripMenuItem_EditableTrackReverse.Image = global::GpxViewer.Properties.Resources.arrow_undo;
         this.toolStripMenuItem_EditableTrackReverse.Name = "toolStripMenuItem_EditableTrackReverse";
         this.toolStripMenuItem_EditableTrackReverse.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackReverse.Text = "Track &umkehren";
         this.toolStripMenuItem_EditableTrackReverse.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackReverse_Click);
         // 
         // toolStripMenuItem_EditableTrackClone
         // 
         this.toolStripMenuItem_EditableTrackClone.Image = global::GpxViewer.Properties.Resources.kopie;
         this.toolStripMenuItem_EditableTrackClone.Name = "toolStripMenuItem_EditableTrackClone";
         this.toolStripMenuItem_EditableTrackClone.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackClone.Text = "&Kopie erzeugen";
         this.toolStripMenuItem_EditableTrackClone.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackClone_Click);
         // 
         // toolStripMenuItem_EditableTrackDelete
         // 
         this.toolStripMenuItem_EditableTrackDelete.Image = global::GpxViewer.Properties.Resources.delete;
         this.toolStripMenuItem_EditableTrackDelete.Name = "toolStripMenuItem_EditableTrackDelete";
         this.toolStripMenuItem_EditableTrackDelete.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackDelete.Text = "Track &löschen";
         this.toolStripMenuItem_EditableTrackDelete.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackDelete_Click);
         // 
         // toolStripSeparator15
         // 
         this.toolStripSeparator15.Name = "toolStripSeparator15";
         this.toolStripSeparator15.Size = new System.Drawing.Size(214, 6);
         // 
         // toolStripMenuItem_EditableTrackShow
         // 
         this.toolStripMenuItem_EditableTrackShow.Image = global::GpxViewer.Properties.Resources.Track;
         this.toolStripMenuItem_EditableTrackShow.Name = "toolStripMenuItem_EditableTrackShow";
         this.toolStripMenuItem_EditableTrackShow.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackShow.Text = "&Track anzeigen";
         this.toolStripMenuItem_EditableTrackShow.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackShow_Click);
         // 
         // toolStripMenuItem_EditableTrackZoom
         // 
         this.toolStripMenuItem_EditableTrackZoom.Image = global::GpxViewer.Properties.Resources.zoom1;
         this.toolStripMenuItem_EditableTrackZoom.Name = "toolStripMenuItem_EditableTrackZoom";
         this.toolStripMenuItem_EditableTrackZoom.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackZoom.Text = "&Zoom auf diesen Track";
         this.toolStripMenuItem_EditableTrackZoom.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackZoom_Click);
         // 
         // toolStripMenuItem_EditableTrackInfo
         // 
         this.toolStripMenuItem_EditableTrackInfo.Image = global::GpxViewer.Properties.Resources.info;
         this.toolStripMenuItem_EditableTrackInfo.Name = "toolStripMenuItem_EditableTrackInfo";
         this.toolStripMenuItem_EditableTrackInfo.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackInfo.Text = "&Info anzeigen";
         this.toolStripMenuItem_EditableTrackInfo.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackInfo_Click);
         // 
         // toolStripMenuItem_EditableTrackExtInfo
         // 
         this.toolStripMenuItem_EditableTrackExtInfo.Image = global::GpxViewer.Properties.Resources.edit;
         this.toolStripMenuItem_EditableTrackExtInfo.Name = "toolStripMenuItem_EditableTrackExtInfo";
         this.toolStripMenuItem_EditableTrackExtInfo.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackExtInfo.Text = "&erweiterte Infos";
         this.toolStripMenuItem_EditableTrackExtInfo.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackExtInfo_Click);
         // 
         // toolStripSeparator12
         // 
         this.toolStripSeparator12.Name = "toolStripSeparator12";
         this.toolStripSeparator12.Size = new System.Drawing.Size(214, 6);
         // 
         // toolStripMenuItem_EditableTrackColor
         // 
         this.toolStripMenuItem_EditableTrackColor.BackColor = System.Drawing.SystemColors.Control;
         this.toolStripMenuItem_EditableTrackColor.Name = "toolStripMenuItem_EditableTrackColor";
         this.toolStripMenuItem_EditableTrackColor.Size = new System.Drawing.Size(217, 22);
         this.toolStripMenuItem_EditableTrackColor.Text = "Track&farbe ändern";
         this.toolStripMenuItem_EditableTrackColor.Click += new System.EventHandler(this.toolStripMenuItem_EditableTrackColor_Click);
         // 
         // numericUpDownMenuItem_EditableLineThickness
         // 
         this.numericUpDownMenuItem_EditableLineThickness.BackColor = System.Drawing.SystemColors.Control;
         this.numericUpDownMenuItem_EditableLineThickness.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
         this.numericUpDownMenuItem_EditableLineThickness.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
         this.numericUpDownMenuItem_EditableLineThickness.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
         this.numericUpDownMenuItem_EditableLineThickness.Name = "numericUpDownMenuItem_EditableLineThickness";
         this.numericUpDownMenuItem_EditableLineThickness.Size = new System.Drawing.Size(157, 30);
         this.numericUpDownMenuItem_EditableLineThickness.Text = "Trackliniendicke";
         this.numericUpDownMenuItem_EditableLineThickness.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(214, 6);
         // 
         // ToolStripMenuItem_RemoveAllEditableTracks
         // 
         this.ToolStripMenuItem_RemoveAllEditableTracks.Image = global::GpxViewer.Properties.Resources.TracksDelete;
         this.ToolStripMenuItem_RemoveAllEditableTracks.Name = "ToolStripMenuItem_RemoveAllEditableTracks";
         this.ToolStripMenuItem_RemoveAllEditableTracks.Size = new System.Drawing.Size(217, 22);
         this.ToolStripMenuItem_RemoveAllEditableTracks.Text = "alle (!) Tracks löschen";
         this.ToolStripMenuItem_RemoveAllEditableTracks.Click += new System.EventHandler(this.ToolStripMenuItem_RemoveAllEditableTracks_Click);
         // 
         // contextMenuStripMarker
         // 
         this.contextMenuStripMarker.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_WaypointZoom,
            this.ToolStripMenuItem_WaypointShow,
            this.ToolStripMenuItem_WaypointEdit,
            this.ToolStripMenuItem_WaypointClone,
            this.ToolStripMenuItem_WaypointSet,
            this.ToolStripMenuItem_WaypointDelete,
            this.ToolStripMenuItem_RemoveAllEditableMarkers});
         this.contextMenuStripMarker.Name = "contextMenuStripEditMarker";
         this.contextMenuStripMarker.Size = new System.Drawing.Size(266, 158);
         this.contextMenuStripMarker.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripMarker_Opening);
         // 
         // ToolStripMenuItem_WaypointZoom
         // 
         this.ToolStripMenuItem_WaypointZoom.Image = global::GpxViewer.Properties.Resources.zoom1;
         this.ToolStripMenuItem_WaypointZoom.Name = "ToolStripMenuItem_WaypointZoom";
         this.ToolStripMenuItem_WaypointZoom.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointZoom.Text = "Zoom auf diese Markierung";
         this.ToolStripMenuItem_WaypointZoom.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointZoom_Click);
         // 
         // ToolStripMenuItem_WaypointShow
         // 
         this.ToolStripMenuItem_WaypointShow.Name = "ToolStripMenuItem_WaypointShow";
         this.ToolStripMenuItem_WaypointShow.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointShow.Text = "Markierung anzeigen";
         this.ToolStripMenuItem_WaypointShow.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointShow_Click);
         // 
         // ToolStripMenuItem_WaypointEdit
         // 
         this.ToolStripMenuItem_WaypointEdit.Image = global::GpxViewer.Properties.Resources.edit;
         this.ToolStripMenuItem_WaypointEdit.Name = "ToolStripMenuItem_WaypointEdit";
         this.ToolStripMenuItem_WaypointEdit.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointEdit.Text = "Eigenschaften anzeigen / &bearbeiten";
         this.ToolStripMenuItem_WaypointEdit.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointEdit_Click);
         // 
         // ToolStripMenuItem_WaypointClone
         // 
         this.ToolStripMenuItem_WaypointClone.Image = global::GpxViewer.Properties.Resources.kopie;
         this.ToolStripMenuItem_WaypointClone.Name = "ToolStripMenuItem_WaypointClone";
         this.ToolStripMenuItem_WaypointClone.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointClone.Text = "bearbeitbare Kopie erzeugen";
         this.ToolStripMenuItem_WaypointClone.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointClone_Click);
         // 
         // ToolStripMenuItem_WaypointSet
         // 
         this.ToolStripMenuItem_WaypointSet.Name = "ToolStripMenuItem_WaypointSet";
         this.ToolStripMenuItem_WaypointSet.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointSet.Text = "neue Position &setzen";
         this.ToolStripMenuItem_WaypointSet.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointSet_Click);
         // 
         // ToolStripMenuItem_WaypointDelete
         // 
         this.ToolStripMenuItem_WaypointDelete.Image = global::GpxViewer.Properties.Resources.delete;
         this.ToolStripMenuItem_WaypointDelete.Name = "ToolStripMenuItem_WaypointDelete";
         this.ToolStripMenuItem_WaypointDelete.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_WaypointDelete.Text = "Marker &löschen";
         this.ToolStripMenuItem_WaypointDelete.Click += new System.EventHandler(this.ToolStripMenuItem_WaypointDelete_Click);
         // 
         // ToolStripMenuItem_RemoveAllEditableMarkers
         // 
         this.ToolStripMenuItem_RemoveAllEditableMarkers.Image = global::GpxViewer.Properties.Resources.MarkersDelete;
         this.ToolStripMenuItem_RemoveAllEditableMarkers.Name = "ToolStripMenuItem_RemoveAllEditableMarkers";
         this.ToolStripMenuItem_RemoveAllEditableMarkers.Size = new System.Drawing.Size(265, 22);
         this.ToolStripMenuItem_RemoveAllEditableMarkers.Text = "alle (!) Marker löschen";
         this.ToolStripMenuItem_RemoveAllEditableMarkers.Click += new System.EventHandler(this.ToolStripMenuItem_RemoveAllEditableMarkers_Click);
         // 
         // toolStripContainer1
         // 
         // 
         // toolStripContainer1.BottomToolStripPanel
         // 
         this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
         // 
         // toolStripContainer1.ContentPanel
         // 
         this.toolStripContainer1.ContentPanel.Controls.Add(this.panelMap);
         this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1055, 689);
         this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
         this.toolStripContainer1.Name = "toolStripContainer1";
         this.toolStripContainer1.Size = new System.Drawing.Size(1055, 738);
         this.toolStripContainer1.TabIndex = 7;
         this.toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip_Edit);
         this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip_Standard);
         // 
         // statusStrip1
         // 
         this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
         this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_MapLoad,
            this.toolStripStatusLabel_Zoom,
            this.toolStripStatusLabel_Pos,
            this.toolStripStatusLabel_TrackMiniInfo,
            this.toolStripStatusLabel_EditInfo,
            this.toolStripStatusLabel_GpxLoad});
         this.statusStrip1.Location = new System.Drawing.Point(0, 0);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(1055, 24);
         this.statusStrip1.TabIndex = 0;
         // 
         // toolStripStatusLabel_MapLoad
         // 
         this.toolStripStatusLabel_MapLoad.Name = "toolStripStatusLabel_MapLoad";
         this.toolStripStatusLabel_MapLoad.Size = new System.Drawing.Size(14, 19);
         this.toolStripStatusLabel_MapLoad.Text = "X";
         // 
         // toolStripStatusLabel_Zoom
         // 
         this.toolStripStatusLabel_Zoom.Name = "toolStripStatusLabel_Zoom";
         this.toolStripStatusLabel_Zoom.Size = new System.Drawing.Size(39, 19);
         this.toolStripStatusLabel_Zoom.Text = "Zoom";
         // 
         // toolStripStatusLabel_Pos
         // 
         this.toolStripStatusLabel_Pos.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.toolStripStatusLabel_Pos.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
         this.toolStripStatusLabel_Pos.Name = "toolStripStatusLabel_Pos";
         this.toolStripStatusLabel_Pos.Size = new System.Drawing.Size(30, 19);
         this.toolStripStatusLabel_Pos.Text = "Pos";
         // 
         // toolStripStatusLabel_TrackMiniInfo
         // 
         this.toolStripStatusLabel_TrackMiniInfo.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.toolStripStatusLabel_TrackMiniInfo.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
         this.toolStripStatusLabel_TrackMiniInfo.Name = "toolStripStatusLabel_TrackMiniInfo";
         this.toolStripStatusLabel_TrackMiniInfo.Size = new System.Drawing.Size(4, 19);
         // 
         // toolStripStatusLabel_EditInfo
         // 
         this.toolStripStatusLabel_EditInfo.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.toolStripStatusLabel_EditInfo.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
         this.toolStripStatusLabel_EditInfo.ForeColor = System.Drawing.SystemColors.GrayText;
         this.toolStripStatusLabel_EditInfo.Name = "toolStripStatusLabel_EditInfo";
         this.toolStripStatusLabel_EditInfo.Size = new System.Drawing.Size(4, 19);
         // 
         // toolStripStatusLabel_GpxLoad
         // 
         this.toolStripStatusLabel_GpxLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
         this.toolStripStatusLabel_GpxLoad.Name = "toolStripStatusLabel_GpxLoad";
         this.toolStripStatusLabel_GpxLoad.Size = new System.Drawing.Size(14, 19);
         this.toolStripStatusLabel_GpxLoad.Text = "X";
         // 
         // toolStrip_Edit
         // 
         this.toolStrip_Edit.Dock = System.Windows.Forms.DockStyle.None;
         this.toolStrip_Edit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_ViewerMode,
            this.toolStripButton_SetMarker,
            this.toolStripButton_TrackDraw,
            this.toolStripButton_TrackDrawEnd,
            this.toolStripButton_ClearEditable,
            this.toolStripButton_UniqueNames,
            this.toolStripButton1});
         this.toolStrip_Edit.Location = new System.Drawing.Point(662, 0);
         this.toolStrip_Edit.Name = "toolStrip_Edit";
         this.toolStrip_Edit.Size = new System.Drawing.Size(204, 25);
         this.toolStrip_Edit.TabIndex = 1;
         // 
         // toolStripButton_ViewerMode
         // 
         this.toolStripButton_ViewerMode.Checked = true;
         this.toolStripButton_ViewerMode.CheckOnClick = true;
         this.toolStripButton_ViewerMode.CheckState = System.Windows.Forms.CheckState.Checked;
         this.toolStripButton_ViewerMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ViewerMode.Image = global::GpxViewer.Properties.Resources.Hand;
         this.toolStripButton_ViewerMode.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ViewerMode.Name = "toolStripButton_ViewerMode";
         this.toolStripButton_ViewerMode.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ViewerMode.Text = "Karte verschieben";
         this.toolStripButton_ViewerMode.Click += new System.EventHandler(this.toolStripButton_ViewerMode_Click);
         // 
         // toolStripButton_SetMarker
         // 
         this.toolStripButton_SetMarker.CheckOnClick = true;
         this.toolStripButton_SetMarker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SetMarker.Image = global::GpxViewer.Properties.Resources.Flag16x16;
         this.toolStripButton_SetMarker.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SetMarker.Name = "toolStripButton_SetMarker";
         this.toolStripButton_SetMarker.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_SetMarker.Text = "neue Markierung setzen";
         this.toolStripButton_SetMarker.Click += new System.EventHandler(this.toolStripButton_SetMarker_Click);
         // 
         // toolStripButton_TrackDraw
         // 
         this.toolStripButton_TrackDraw.CheckOnClick = true;
         this.toolStripButton_TrackDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_TrackDraw.Image = global::GpxViewer.Properties.Resources.TrackDraw;
         this.toolStripButton_TrackDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_TrackDraw.Name = "toolStripButton_TrackDraw";
         this.toolStripButton_TrackDraw.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_TrackDraw.Text = "neuen Track zeichnen";
         this.toolStripButton_TrackDraw.Click += new System.EventHandler(this.toolStripButton_TrackDraw_Click);
         // 
         // toolStripButton_TrackDrawEnd
         // 
         this.toolStripButton_TrackDrawEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_TrackDrawEnd.Enabled = false;
         this.toolStripButton_TrackDrawEnd.Image = global::GpxViewer.Properties.Resources.ok;
         this.toolStripButton_TrackDrawEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_TrackDrawEnd.Name = "toolStripButton_TrackDrawEnd";
         this.toolStripButton_TrackDrawEnd.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_TrackDrawEnd.Text = "Track zeichnen beenden";
         this.toolStripButton_TrackDrawEnd.Click += new System.EventHandler(this.toolStripButton_TrackDrawEnd_Click);
         // 
         // toolStripButton_ClearEditable
         // 
         this.toolStripButton_ClearEditable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ClearEditable.Image = global::GpxViewer.Properties.Resources.delete;
         this.toolStripButton_ClearEditable.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ClearEditable.Name = "toolStripButton_ClearEditable";
         this.toolStripButton_ClearEditable.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ClearEditable.Text = "alle editierbaren Tracks und Markierungen löschen";
         this.toolStripButton_ClearEditable.Click += new System.EventHandler(this.toolStripButton_ClearEditable_Click);
         // 
         // toolStripButton_UniqueNames
         // 
         this.toolStripButton_UniqueNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_UniqueNames.Image = global::GpxViewer.Properties.Resources.list_numbers;
         this.toolStripButton_UniqueNames.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_UniqueNames.Name = "toolStripButton_UniqueNames";
         this.toolStripButton_UniqueNames.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_UniqueNames.Text = "Namen der Tracks und Marker eindeutig machen";
         this.toolStripButton_UniqueNames.Click += new System.EventHandler(this.toolStripButton_UniqueNames_Click);
         // 
         // toolStripButton1
         // 
         this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
         this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton1.Name = "toolStripButton1";
         this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton1.Text = "toolStripButton1";
         // 
         // toolStrip_Standard
         // 
         this.toolStrip_Standard.Dock = System.Windows.Forms.DockStyle.None;
         this.toolStrip_Standard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxMapSource,
            this.toolStripButton_ReloadMap,
            this.toolStripButton_ClearCache,
            this.toolStripSeparator3,
            this.toolStripButton_OpenGpxfile,
            this.toolStripButton_SaveGpxFile,
            this.toolStripButton_SaveGpxFileExt,
            this.toolStripButton_SaveWithGarminExt,
            this.toolStripButton_CopyMap,
            this.toolStripButton_PrintMap,
            this.toolStripButton_GeoTagging,
            this.toolStripSeparator20,
            this.toolStripButton_ZoomIn,
            this.toolStripButton_ZoomOut,
            this.toolStripButton_TrackZoom,
            this.toolStripSeparator8,
            this.toolStripButton_LocationForm,
            this.toolStripButton_GeoSearch,
            this.toolStripButton_TrackSearch,
            this.toolStripSeparator6,
            this.toolStripButton_MiniHelp});
         this.toolStrip_Standard.Location = new System.Drawing.Point(3, 0);
         this.toolStrip_Standard.Name = "toolStrip_Standard";
         this.toolStrip_Standard.Size = new System.Drawing.Size(656, 25);
         this.toolStrip_Standard.TabIndex = 0;
         // 
         // toolStripComboBoxMapSource
         // 
         this.toolStripComboBoxMapSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.toolStripComboBoxMapSource.Name = "toolStripComboBoxMapSource";
         this.toolStripComboBoxMapSource.Size = new System.Drawing.Size(250, 25);
         this.toolStripComboBoxMapSource.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxMapSource_SelectedIndexChanged);
         // 
         // toolStripButton_ReloadMap
         // 
         this.toolStripButton_ReloadMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ReloadMap.Image = global::GpxViewer.Properties.Resources.reload;
         this.toolStripButton_ReloadMap.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ReloadMap.Name = "toolStripButton_ReloadMap";
         this.toolStripButton_ReloadMap.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ReloadMap.Text = "Karte neu zeichnen";
         this.toolStripButton_ReloadMap.Click += new System.EventHandler(this.toolStripButton_ReloadMap_Click);
         // 
         // toolStripButton_ClearCache
         // 
         this.toolStripButton_ClearCache.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ClearCache.Image = global::GpxViewer.Properties.Resources.database_delete;
         this.toolStripButton_ClearCache.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ClearCache.Name = "toolStripButton_ClearCache";
         this.toolStripButton_ClearCache.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ClearCache.Text = "intern gespeicherte Daten löschen";
         this.toolStripButton_ClearCache.Click += new System.EventHandler(this.toolStripButton_ClearCache_Click);
         // 
         // toolStripSeparator3
         // 
         this.toolStripSeparator3.Name = "toolStripSeparator3";
         this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
         // 
         // toolStripButton_OpenGpxfile
         // 
         this.toolStripButton_OpenGpxfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_OpenGpxfile.Image = global::GpxViewer.Properties.Resources.Open;
         this.toolStripButton_OpenGpxfile.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_OpenGpxfile.Name = "toolStripButton_OpenGpxfile";
         this.toolStripButton_OpenGpxfile.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_OpenGpxfile.Text = "GPX-Datei öffnen";
         this.toolStripButton_OpenGpxfile.Click += new System.EventHandler(this.toolStripButton_OpenGpxfile_Click);
         // 
         // toolStripButton_SaveGpxFile
         // 
         this.toolStripButton_SaveGpxFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SaveGpxFile.Enabled = false;
         this.toolStripButton_SaveGpxFile.Image = global::GpxViewer.Properties.Resources.speichern;
         this.toolStripButton_SaveGpxFile.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SaveGpxFile.Name = "toolStripButton_SaveGpxFile";
         this.toolStripButton_SaveGpxFile.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_SaveGpxFile.Text = "speichern";
         this.toolStripButton_SaveGpxFile.Click += new System.EventHandler(this.toolStripButton_SaveGpxFile_Click);
         // 
         // toolStripButton_SaveGpxFileExt
         // 
         this.toolStripButton_SaveGpxFileExt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SaveGpxFileExt.Enabled = false;
         this.toolStripButton_SaveGpxFileExt.Image = global::GpxViewer.Properties.Resources.speichernu;
         this.toolStripButton_SaveGpxFileExt.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SaveGpxFileExt.Name = "toolStripButton_SaveGpxFileExt";
         this.toolStripButton_SaveGpxFileExt.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_SaveGpxFileExt.Text = "speichern unter ...";
         this.toolStripButton_SaveGpxFileExt.Click += new System.EventHandler(this.toolStripButton_SaveGpxFileExt_Click);
         // 
         // toolStripButton_SaveWithGarminExt
         // 
         this.toolStripButton_SaveWithGarminExt.Checked = true;
         this.toolStripButton_SaveWithGarminExt.CheckOnClick = true;
         this.toolStripButton_SaveWithGarminExt.CheckState = System.Windows.Forms.CheckState.Checked;
         this.toolStripButton_SaveWithGarminExt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SaveWithGarminExt.Image = global::GpxViewer.Properties.Resources.garmin1;
         this.toolStripButton_SaveWithGarminExt.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SaveWithGarminExt.Name = "toolStripButton_SaveWithGarminExt";
         this.toolStripButton_SaveWithGarminExt.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_SaveWithGarminExt.Text = "mit Garminerweiterungen speichern";
         // 
         // toolStripButton_CopyMap
         // 
         this.toolStripButton_CopyMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_CopyMap.Image = global::GpxViewer.Properties.Resources.copy;
         this.toolStripButton_CopyMap.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_CopyMap.Name = "toolStripButton_CopyMap";
         this.toolStripButton_CopyMap.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_CopyMap.Text = "Karte in Zwischenablage kopieren ...";
         this.toolStripButton_CopyMap.Click += new System.EventHandler(this.toolStripButton_CopyMap_Click);
         // 
         // toolStripButton_PrintMap
         // 
         this.toolStripButton_PrintMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_PrintMap.Image = global::GpxViewer.Properties.Resources.printer;
         this.toolStripButton_PrintMap.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_PrintMap.Name = "toolStripButton_PrintMap";
         this.toolStripButton_PrintMap.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_PrintMap.Text = "Karte drucken";
         this.toolStripButton_PrintMap.Click += new System.EventHandler(this.toolStripButton_PrintMap_Click);
         // 
         // toolStripButton_GeoTagging
         // 
         this.toolStripButton_GeoTagging.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_GeoTagging.Image = global::GpxViewer.Properties.Resources.GeoTagging;
         this.toolStripButton_GeoTagging.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_GeoTagging.Name = "toolStripButton_GeoTagging";
         this.toolStripButton_GeoTagging.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_GeoTagging.Text = "Geotagging für Fotos";
         this.toolStripButton_GeoTagging.Click += new System.EventHandler(this.toolStripButton_GeoTagging_Click);
         // 
         // toolStripSeparator20
         // 
         this.toolStripSeparator20.Name = "toolStripSeparator20";
         this.toolStripSeparator20.Size = new System.Drawing.Size(6, 25);
         // 
         // toolStripButton_ZoomIn
         // 
         this.toolStripButton_ZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ZoomIn.Image = global::GpxViewer.Properties.Resources.zoom_in;
         this.toolStripButton_ZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ZoomIn.Name = "toolStripButton_ZoomIn";
         this.toolStripButton_ZoomIn.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ZoomIn.Text = "hineinzoomen";
         this.toolStripButton_ZoomIn.Click += new System.EventHandler(this.toolStripButton_ZoomIn_Click);
         // 
         // toolStripButton_ZoomOut
         // 
         this.toolStripButton_ZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_ZoomOut.Image = global::GpxViewer.Properties.Resources.zoom_out;
         this.toolStripButton_ZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_ZoomOut.Name = "toolStripButton_ZoomOut";
         this.toolStripButton_ZoomOut.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_ZoomOut.Text = "herauszoomen";
         this.toolStripButton_ZoomOut.Click += new System.EventHandler(this.toolStripButton_ZoomOut_Click);
         // 
         // toolStripButton_TrackZoom
         // 
         this.toolStripButton_TrackZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_TrackZoom.Image = global::GpxViewer.Properties.Resources.zoom1;
         this.toolStripButton_TrackZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_TrackZoom.Name = "toolStripButton_TrackZoom";
         this.toolStripButton_TrackZoom.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_TrackZoom.Text = "Zoom auf angezeigte Tracks";
         this.toolStripButton_TrackZoom.Click += new System.EventHandler(this.toolStripButton_TrackZoom_Click);
         // 
         // toolStripSeparator8
         // 
         this.toolStripSeparator8.Name = "toolStripSeparator8";
         this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
         // 
         // toolStripButton_LocationForm
         // 
         this.toolStripButton_LocationForm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_LocationForm.Image = global::GpxViewer.Properties.Resources.map_go;
         this.toolStripButton_LocationForm.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_LocationForm.Name = "toolStripButton_LocationForm";
         this.toolStripButton_LocationForm.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_LocationForm.Text = "gespeicherte Orte";
         this.toolStripButton_LocationForm.Click += new System.EventHandler(this.toolStripButton_LocationForm_Click);
         // 
         // toolStripButton_GeoSearch
         // 
         this.toolStripButton_GeoSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_GeoSearch.Image = global::GpxViewer.Properties.Resources.Search2;
         this.toolStripButton_GeoSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_GeoSearch.Name = "toolStripButton_GeoSearch";
         this.toolStripButton_GeoSearch.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_GeoSearch.Text = "geografisches Objekt suchen";
         this.toolStripButton_GeoSearch.Click += new System.EventHandler(this.toolStripButton_GeoSearch_Click);
         // 
         // toolStripButton_TrackSearch
         // 
         this.toolStripButton_TrackSearch.CheckOnClick = true;
         this.toolStripButton_TrackSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_TrackSearch.Image = global::GpxViewer.Properties.Resources.Search;
         this.toolStripButton_TrackSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_TrackSearch.Name = "toolStripButton_TrackSearch";
         this.toolStripButton_TrackSearch.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_TrackSearch.Text = "Tracks im markierten Bereich suchen";
         this.toolStripButton_TrackSearch.Click += new System.EventHandler(this.toolStripButton_TrackSearch_Click);
         // 
         // toolStripSeparator6
         // 
         this.toolStripSeparator6.Name = "toolStripSeparator6";
         this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
         // 
         // toolStripButton_MiniHelp
         // 
         this.toolStripButton_MiniHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_MiniHelp.Image = global::GpxViewer.Properties.Resources.help;
         this.toolStripButton_MiniHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_MiniHelp.Name = "toolStripButton_MiniHelp";
         this.toolStripButton_MiniHelp.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_MiniHelp.Text = "Hilfe";
         this.toolStripButton_MiniHelp.Click += new System.EventHandler(this.toolStripButton_MiniHelp_Click);
         // 
         // colorDialog1
         // 
         this.colorDialog1.AnyColor = true;
         this.colorDialog1.FullOpen = true;
         // 
         // openFileDialogGpx
         // 
         this.openFileDialogGpx.Filter = "GPX-Dateien|*.gpx|GDB-Dateien|*.gdb|alle Dateien|*.*";
         this.openFileDialogGpx.Title = "GPX-Datei öffnen";
         // 
         // saveFileDialogGpx
         // 
         this.saveFileDialogGpx.DefaultExt = "gpx";
         this.saveFileDialogGpx.Filter = "Gpx-Dateien|*.gpx|KMZ-Dateien|*.kmz|KML-Dateien|*.kml";
         this.saveFileDialogGpx.Title = "speichern unter ...";
         // 
         // FormMain
         // 
         this.AllowDrop = true;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1055, 738);
         this.Controls.Add(this.toolStripContainer1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.KeyPreview = true;
         this.Name = "FormMain";
         this.Text = "Form1";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
         this.Load += new System.EventHandler(this.FormMain_Load);
         this.Shown += new System.EventHandler(this.FormMain_Shown);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
         this.panelMap.ResumeLayout(false);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.tabControl1.ResumeLayout(false);
         this.tabPageFiles.ResumeLayout(false);
         this.contextMenuStripReadOnlyTracks.ResumeLayout(false);
         this.contextMenuStripReadOnlyTracks.PerformLayout();
         this.tabPageEditable.ResumeLayout(false);
         this.contextMenuStripEditableTracks.ResumeLayout(false);
         this.contextMenuStripEditableTracks.PerformLayout();
         this.contextMenuStripMarker.ResumeLayout(false);
         this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
         this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
         this.toolStripContainer1.ContentPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.PerformLayout();
         this.toolStripContainer1.ResumeLayout(false);
         this.toolStripContainer1.PerformLayout();
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         this.toolStrip_Edit.ResumeLayout(false);
         this.toolStrip_Edit.PerformLayout();
         this.toolStrip_Standard.ResumeLayout(false);
         this.toolStrip_Standard.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion
      private System.Windows.Forms.Panel panelMap;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ToolTip toolTipRouteInfo;
      private System.Windows.Forms.ToolStripContainer toolStripContainer1;
      private System.Windows.Forms.ToolStrip toolStrip_Standard;
      private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMapSource;
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_MapLoad;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Pos;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_EditInfo;
      private System.Windows.Forms.ToolStripButton toolStripButton_ReloadMap;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackZoom;
      private System.Windows.Forms.ColorDialog colorDialog1;
      private System.Windows.Forms.ToolStripButton toolStripButton_OpenGpxfile;
      private System.Windows.Forms.OpenFileDialog openFileDialogGpx;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripMarker;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointEdit;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointSet;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointDelete;
      private System.Windows.Forms.TabControl tabControl1;
      private System.Windows.Forms.TabPage tabPageFiles;
      private System.Windows.Forms.ToolStripButton toolStripButton_ViewerMode;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveGpxFile;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
      private System.Windows.Forms.ToolStripButton toolStripButton_CopyMap;
      private System.Windows.Forms.SaveFileDialog saveFileDialogGpx;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointClone;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveGpxFileExt;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_WaypointZoom;
      private System.Windows.Forms.ToolStripButton toolStripButton_ZoomIn;
      private System.Windows.Forms.ToolStripButton toolStripButton_ZoomOut;
      private System.Windows.Forms.ToolStripButton toolStripButton_LocationForm;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
      private System.Windows.Forms.ToolStripButton toolStripButton_MiniHelp;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Zoom;
      private MapControl mapControl1;
      private System.Windows.Forms.ToolStripButton toolStripButton_SetMarker;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackDraw;
      private System.Windows.Forms.ToolStripButton toolStripButton_TrackDrawEnd;
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
      private ReadOnlyTracklistControl readOnlyTracklistControl1;
      private System.Windows.Forms.TabPage tabPageEditable;
      private EditableTracklistControl editableTracklistControl1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_TrackMiniInfo;
      private System.Windows.Forms.ToolStripButton toolStripButton_GeoSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_RemoveAllEditableTracks;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_RemoveAllEditableMarkers;
      private System.Windows.Forms.ToolStripButton toolStripButton_GeoTagging;
      private System.Windows.Forms.ToolStrip toolStrip_Edit;
      private System.Windows.Forms.ToolStripButton toolStripButton1;
   }
}

