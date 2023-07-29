namespace GpxViewer {
   partial class PictureManager {
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

      #region Vom Komponenten-Designer generierter Code

      /// <summary> 
      /// Erforderliche Methode für die Designerunterstützung. 
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent() {
         this.components = new System.ComponentModel.Container();
         this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.toolStripStatusLabel_Path = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_Count = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabel_Filename = new System.Windows.Forms.ToolStripStatusLabel();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.listView1 = new System.Windows.Forms.ListView();
         this.columnPicture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnFileDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnPictureDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnCoordinates = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnDirection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItemSave = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItemShow = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItemSet = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItemSet2 = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItemEditComment = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItemEditFilename = new System.Windows.Forms.ToolStripMenuItem();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.toolStrip1 = new System.Windows.Forms.ToolStrip();
         this.toolStripButton_OpenPath = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_Reload = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_WithSubDirs = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SaveAll = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_SaveGpx = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
         this.ToolStripMenuItem_ViewAll = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_ViewWithGeo = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_ViewWithoutGeo = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
         this.ToolStripMenuItem_FilenameAsc = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_FilenameDesc = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_FiledateAsc = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_FiledateDesc = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_GeodateAsc = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_GeodateDesc = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton_SwapView = new System.Windows.Forms.ToolStripButton();
         this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
         this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
         this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
         this.toolStripContainer1.ContentPanel.SuspendLayout();
         this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
         this.toolStripContainer1.SuspendLayout();
         this.statusStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.contextMenuStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.toolStrip1.SuspendLayout();
         this.SuspendLayout();
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
         this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
         this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(684, 453);
         this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
         this.toolStripContainer1.Name = "toolStripContainer1";
         this.toolStripContainer1.Size = new System.Drawing.Size(684, 502);
         this.toolStripContainer1.TabIndex = 0;
         this.toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
         // 
         // statusStrip1
         // 
         this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
         this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_Path,
            this.toolStripStatusLabel_Count,
            this.toolStripStatusLabel_Filename});
         this.statusStrip1.Location = new System.Drawing.Point(0, 0);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(684, 22);
         this.statusStrip1.TabIndex = 0;
         // 
         // toolStripStatusLabel_Path
         // 
         this.toolStripStatusLabel_Path.Name = "toolStripStatusLabel_Path";
         this.toolStripStatusLabel_Path.Size = new System.Drawing.Size(0, 17);
         // 
         // toolStripStatusLabel_Count
         // 
         this.toolStripStatusLabel_Count.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.toolStripStatusLabel_Count.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.toolStripStatusLabel_Count.Font = new System.Drawing.Font("Segoe UI", 9F);
         this.toolStripStatusLabel_Count.Name = "toolStripStatusLabel_Count";
         this.toolStripStatusLabel_Count.Size = new System.Drawing.Size(4, 17);
         // 
         // toolStripStatusLabel_Filename
         // 
         this.toolStripStatusLabel_Filename.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.toolStripStatusLabel_Filename.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.toolStripStatusLabel_Filename.Name = "toolStripStatusLabel_Filename";
         this.toolStripStatusLabel_Filename.Size = new System.Drawing.Size(4, 17);
         // 
         // splitContainer1
         // 
         this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.listView1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
         this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3);
         this.splitContainer1.Size = new System.Drawing.Size(684, 453);
         this.splitContainer1.SplitterDistance = 216;
         this.splitContainer1.SplitterWidth = 3;
         this.splitContainer1.TabIndex = 3;
         // 
         // listView1
         // 
         this.listView1.AllowColumnReorder = true;
         this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnPicture,
            this.columnFile,
            this.columnFileDate,
            this.columnPictureDate,
            this.columnCoordinates,
            this.columnDirection,
            this.columnComment});
         this.listView1.ContextMenuStrip = this.contextMenuStrip1;
         this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listView1.ForeColor = System.Drawing.SystemColors.WindowText;
         this.listView1.FullRowSelect = true;
         this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.listView1.HideSelection = false;
         this.listView1.Location = new System.Drawing.Point(0, 0);
         this.listView1.Name = "listView1";
         this.listView1.ShowItemToolTips = true;
         this.listView1.Size = new System.Drawing.Size(680, 212);
         this.listView1.TabIndex = 0;
         this.listView1.TileSize = new System.Drawing.Size(228, 100);
         this.listView1.UseCompatibleStateImageBehavior = false;
         this.listView1.View = System.Windows.Forms.View.Tile;
         this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
         // 
         // columnPicture
         // 
         this.columnPicture.Text = "Bild";
         // 
         // columnFile
         // 
         this.columnFile.Text = "Dateiname";
         this.columnFile.Width = 50;
         // 
         // columnFileDate
         // 
         this.columnFileDate.Text = "Dateidatum";
         // 
         // columnPictureDate
         // 
         this.columnPictureDate.Text = "Bilddatum";
         // 
         // columnCoordinates
         // 
         this.columnCoordinates.Text = "Koordinaten";
         // 
         // columnDirection
         // 
         this.columnDirection.Text = "Richtung";
         // 
         // columnComment
         // 
         this.columnComment.Text = "Kommentar";
         // 
         // contextMenuStrip1
         // 
         this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemSave,
            this.ToolStripMenuItemShow,
            this.ToolStripMenuItemSet,
            this.ToolStripMenuItemSet2,
            this.ToolStripMenuItemEditComment,
            this.ToolStripMenuItemEditFilename});
         this.contextMenuStrip1.Name = "contextMenuStrip1";
         this.contextMenuStrip1.Size = new System.Drawing.Size(375, 158);
         this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
         // 
         // ToolStripMenuItemSave
         // 
         this.ToolStripMenuItemSave.Name = "ToolStripMenuItemSave";
         this.ToolStripMenuItemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
         this.ToolStripMenuItemSave.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemSave.Text = "akt. Daten des Bildes &speichern";
         this.ToolStripMenuItemSave.Click += new System.EventHandler(this.ToolStripMenuItemSave_Click);
         // 
         // ToolStripMenuItemShow
         // 
         this.ToolStripMenuItemShow.Name = "ToolStripMenuItemShow";
         this.ToolStripMenuItemShow.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
         this.ToolStripMenuItemShow.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemShow.Text = "Position auf Karte &anzeigen";
         this.ToolStripMenuItemShow.Click += new System.EventHandler(this.ToolStripMenuItemShow_Click);
         // 
         // ToolStripMenuItemSet
         // 
         this.ToolStripMenuItemSet.Name = "ToolStripMenuItemSet";
         this.ToolStripMenuItemSet.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
         this.ToolStripMenuItemSet.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemSet.Text = "neue &Position setzen";
         this.ToolStripMenuItemSet.Click += new System.EventHandler(this.ToolStripMenuItemSet_Click);
         // 
         // ToolStripMenuItemSet2
         // 
         this.ToolStripMenuItemSet2.Name = "ToolStripMenuItemSet2";
         this.ToolStripMenuItemSet2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
         this.ToolStripMenuItemSet2.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemSet2.Text = "neue Position über angezeigte GP&X-Tracks setzen";
         this.ToolStripMenuItemSet2.Click += new System.EventHandler(this.ToolStripMenuItemSet2_Click);
         // 
         // ToolStripMenuItemEditComment
         // 
         this.ToolStripMenuItemEditComment.Name = "ToolStripMenuItemEditComment";
         this.ToolStripMenuItemEditComment.ShortcutKeys = System.Windows.Forms.Keys.F2;
         this.ToolStripMenuItemEditComment.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemEditComment.Text = "&Kommentar ändern";
         this.ToolStripMenuItemEditComment.Click += new System.EventHandler(this.ToolStripMenuItemEditComment_Click);
         // 
         // ToolStripMenuItemEditFilename
         // 
         this.ToolStripMenuItemEditFilename.Name = "ToolStripMenuItemEditFilename";
         this.ToolStripMenuItemEditFilename.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
         this.ToolStripMenuItemEditFilename.Size = new System.Drawing.Size(374, 22);
         this.ToolStripMenuItemEditFilename.Text = "&Dateiname ändern";
         this.ToolStripMenuItemEditFilename.Click += new System.EventHandler(this.ToolStripMenuItemEditFilename_Click);
         // 
         // pictureBox1
         // 
         this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pictureBox1.Location = new System.Drawing.Point(3, 3);
         this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(674, 224);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBox1.TabIndex = 0;
         this.pictureBox1.TabStop = false;
         // 
         // toolStrip1
         // 
         this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
         this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_OpenPath,
            this.toolStripButton_Reload,
            this.toolStripButton_WithSubDirs,
            this.toolStripButton_SaveAll,
            this.toolStripButton_SaveGpx,
            this.toolStripSeparator2,
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2,
            this.toolStripSeparator1,
            this.toolStripButton_SwapView});
         this.toolStrip1.Location = new System.Drawing.Point(3, 0);
         this.toolStrip1.Name = "toolStrip1";
         this.toolStrip1.Size = new System.Drawing.Size(234, 27);
         this.toolStrip1.TabIndex = 0;
         // 
         // toolStripButton_OpenPath
         // 
         this.toolStripButton_OpenPath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_OpenPath.Image = global::GpxViewer.Properties.Resources.Open;
         this.toolStripButton_OpenPath.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_OpenPath.Name = "toolStripButton_OpenPath";
         this.toolStripButton_OpenPath.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_OpenPath.Text = "Bildverzeichnis auswählen (STRG+V)";
         this.toolStripButton_OpenPath.Click += new System.EventHandler(this.toolStripButton_OpenPath_Click);
         // 
         // toolStripButton_Reload
         // 
         this.toolStripButton_Reload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_Reload.Image = global::GpxViewer.Properties.Resources.arrow_refresh;
         this.toolStripButton_Reload.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_Reload.Name = "toolStripButton_Reload";
         this.toolStripButton_Reload.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_Reload.Text = "neu einlesen (STRG+R)";
         this.toolStripButton_Reload.Click += new System.EventHandler(this.toolStripButton_Reload_Click);
         // 
         // toolStripButton_WithSubDirs
         // 
         this.toolStripButton_WithSubDirs.CheckOnClick = true;
         this.toolStripButton_WithSubDirs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_WithSubDirs.Image = global::GpxViewer.Properties.Resources.subfolder;
         this.toolStripButton_WithSubDirs.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_WithSubDirs.Name = "toolStripButton_WithSubDirs";
         this.toolStripButton_WithSubDirs.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_WithSubDirs.Text = "Unterordner einbeziehen";
         this.toolStripButton_WithSubDirs.Click += new System.EventHandler(this.toolStripButton_WithSubDirs_Click);
         // 
         // toolStripButton_SaveAll
         // 
         this.toolStripButton_SaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SaveAll.Image = global::GpxViewer.Properties.Resources.speichern;
         this.toolStripButton_SaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SaveAll.Name = "toolStripButton_SaveAll";
         this.toolStripButton_SaveAll.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_SaveAll.Text = "alle speichern";
         this.toolStripButton_SaveAll.Click += new System.EventHandler(this.toolStripButton_SaveAll_Click);
         // 
         // toolStripButton_SaveGpx
         // 
         this.toolStripButton_SaveGpx.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SaveGpx.Image = global::GpxViewer.Properties.Resources.speicherngpx;
         this.toolStripButton_SaveGpx.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SaveGpx.Name = "toolStripButton_SaveGpx";
         this.toolStripButton_SaveGpx.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_SaveGpx.Text = "markierte Bilder als Verweise in GPX-Datei speichern";
         this.toolStripButton_SaveGpx.Click += new System.EventHandler(this.toolStripButton_SaveGpx_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
         // 
         // toolStripDropDownButton1
         // 
         this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_ViewAll,
            this.ToolStripMenuItem_ViewWithGeo,
            this.ToolStripMenuItem_ViewWithoutGeo});
         this.toolStripDropDownButton1.Image = global::GpxViewer.Properties.Resources.Filter;
         this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
         this.toolStripDropDownButton1.Size = new System.Drawing.Size(33, 24);
         this.toolStripDropDownButton1.Text = "Bildfilter";
         // 
         // ToolStripMenuItem_ViewAll
         // 
         this.ToolStripMenuItem_ViewAll.Name = "ToolStripMenuItem_ViewAll";
         this.ToolStripMenuItem_ViewAll.Size = new System.Drawing.Size(277, 22);
         this.ToolStripMenuItem_ViewAll.Text = "alle Bilder anzeigen";
         this.ToolStripMenuItem_ViewAll.Click += new System.EventHandler(this.ToolStripMenuItem_ViewAll_Click);
         // 
         // ToolStripMenuItem_ViewWithGeo
         // 
         this.ToolStripMenuItem_ViewWithGeo.Name = "ToolStripMenuItem_ViewWithGeo";
         this.ToolStripMenuItem_ViewWithGeo.Size = new System.Drawing.Size(277, 22);
         this.ToolStripMenuItem_ViewWithGeo.Text = "nur Bilder mit Geo-Position anzeigen";
         this.ToolStripMenuItem_ViewWithGeo.Click += new System.EventHandler(this.ToolStripMenuItem_ViewWithGeo_Click);
         // 
         // ToolStripMenuItem_ViewWithoutGeo
         // 
         this.ToolStripMenuItem_ViewWithoutGeo.Name = "ToolStripMenuItem_ViewWithoutGeo";
         this.ToolStripMenuItem_ViewWithoutGeo.Size = new System.Drawing.Size(277, 22);
         this.ToolStripMenuItem_ViewWithoutGeo.Text = "nur Bilder ohne Geo-Position anzeigen";
         this.ToolStripMenuItem_ViewWithoutGeo.Click += new System.EventHandler(this.ToolStripMenuItem_ViewWithoutGeo_Click);
         // 
         // toolStripDropDownButton2
         // 
         this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_FilenameAsc,
            this.ToolStripMenuItem_FilenameDesc,
            this.ToolStripMenuItem_FiledateAsc,
            this.ToolStripMenuItem_FiledateDesc,
            this.ToolStripMenuItem_GeodateAsc,
            this.ToolStripMenuItem_GeodateDesc});
         this.toolStripDropDownButton2.Image = global::GpxViewer.Properties.Resources.Sort;
         this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
         this.toolStripDropDownButton2.Size = new System.Drawing.Size(33, 24);
         this.toolStripDropDownButton2.Text = "Bildsortierung";
         // 
         // ToolStripMenuItem_FilenameAsc
         // 
         this.ToolStripMenuItem_FilenameAsc.Name = "ToolStripMenuItem_FilenameAsc";
         this.ToolStripMenuItem_FilenameAsc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_FilenameAsc.Text = "Dateiname aufsteigend";
         this.ToolStripMenuItem_FilenameAsc.Click += new System.EventHandler(this.ToolStripMenuItem_FilenameAsc_Click);
         // 
         // ToolStripMenuItem_FilenameDesc
         // 
         this.ToolStripMenuItem_FilenameDesc.Name = "ToolStripMenuItem_FilenameDesc";
         this.ToolStripMenuItem_FilenameDesc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_FilenameDesc.Text = "Dateiname absteigend";
         this.ToolStripMenuItem_FilenameDesc.Click += new System.EventHandler(this.ToolStripMenuItem_FilenameDesc_Click);
         // 
         // ToolStripMenuItem_FiledateAsc
         // 
         this.ToolStripMenuItem_FiledateAsc.Name = "ToolStripMenuItem_FiledateAsc";
         this.ToolStripMenuItem_FiledateAsc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_FiledateAsc.Text = "Dateidatum aufsteigend";
         this.ToolStripMenuItem_FiledateAsc.Click += new System.EventHandler(this.ToolStripMenuItem_FiledateAsc_Click);
         // 
         // ToolStripMenuItem_FiledateDesc
         // 
         this.ToolStripMenuItem_FiledateDesc.Name = "ToolStripMenuItem_FiledateDesc";
         this.ToolStripMenuItem_FiledateDesc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_FiledateDesc.Text = "Dateidatum absteigend";
         this.ToolStripMenuItem_FiledateDesc.Click += new System.EventHandler(this.ToolStripMenuItem_FiledateDesc_Click);
         // 
         // ToolStripMenuItem_GeodateAsc
         // 
         this.ToolStripMenuItem_GeodateAsc.Name = "ToolStripMenuItem_GeodateAsc";
         this.ToolStripMenuItem_GeodateAsc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_GeodateAsc.Text = "Aufnahmedatum aufsteigend";
         this.ToolStripMenuItem_GeodateAsc.Click += new System.EventHandler(this.ToolStripMenuItem_GeodateAsc_Click);
         // 
         // ToolStripMenuItem_GeodateDesc
         // 
         this.ToolStripMenuItem_GeodateDesc.Name = "ToolStripMenuItem_GeodateDesc";
         this.ToolStripMenuItem_GeodateDesc.Size = new System.Drawing.Size(230, 22);
         this.ToolStripMenuItem_GeodateDesc.Text = "Aufnahmedatum absteigend";
         this.ToolStripMenuItem_GeodateDesc.Click += new System.EventHandler(this.ToolStripMenuItem_GeodateDesc_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
         // 
         // toolStripButton_SwapView
         // 
         this.toolStripButton_SwapView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_SwapView.Image = global::GpxViewer.Properties.Resources.table;
         this.toolStripButton_SwapView.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_SwapView.Name = "toolStripButton_SwapView";
         this.toolStripButton_SwapView.Size = new System.Drawing.Size(24, 24);
         this.toolStripButton_SwapView.Text = "Ansicht wechseln (STRG+W)";
         this.toolStripButton_SwapView.Click += new System.EventHandler(this.toolStripButton_SwapView_Click);
         // 
         // folderBrowserDialog1
         // 
         this.folderBrowserDialog1.Description = "Ordner mit den Bildern auswählen";
         this.folderBrowserDialog1.ShowNewFolderButton = false;
         // 
         // saveFileDialog1
         // 
         this.saveFileDialog1.Filter = "GPX-Dateien|*.gpx";
         this.saveFileDialog1.Title = "Bilder als Verweise in einer GPX-Datei speichern";
         // 
         // PictureManager
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.toolStripContainer1);
         this.Name = "PictureManager";
         this.Size = new System.Drawing.Size(684, 502);
         this.Load += new System.EventHandler(this.PictureManager_Load);
         this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
         this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
         this.toolStripContainer1.ContentPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.PerformLayout();
         this.toolStripContainer1.ResumeLayout(false);
         this.toolStripContainer1.PerformLayout();
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.contextMenuStrip1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.toolStrip1.ResumeLayout(false);
         this.toolStrip1.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ToolStripContainer toolStripContainer1;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ListView listView1;
      private System.Windows.Forms.ColumnHeader columnPicture;
      private System.Windows.Forms.ColumnHeader columnFile;
      private System.Windows.Forms.ColumnHeader columnFileDate;
      private System.Windows.Forms.ColumnHeader columnPictureDate;
      private System.Windows.Forms.ColumnHeader columnCoordinates;
      private System.Windows.Forms.ColumnHeader columnDirection;
      private System.Windows.Forms.PictureBox pictureBox1;

      private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSave;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemShow;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSet;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSet2;
      private System.Windows.Forms.ToolStrip toolStrip1;
      private System.Windows.Forms.ToolStripButton toolStripButton_SwapView;
      private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ViewAll;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ViewWithGeo;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ViewWithoutGeo;
      private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_FilenameAsc;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_FilenameDesc;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_FiledateAsc;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_FiledateDesc;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_GeodateAsc;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_GeodateDesc;
      private System.Windows.Forms.ToolStripButton toolStripButton_OpenPath;
      private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
      private System.Windows.Forms.ToolStripButton toolStripButton_WithSubDirs;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveAll;
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Path;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Count;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Filename;
      private System.Windows.Forms.ToolStripButton toolStripButton_SaveGpx;
      private System.Windows.Forms.SaveFileDialog saveFileDialog1;
      private System.Windows.Forms.ToolStripButton toolStripButton_Reload;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemEditFilename;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemEditComment;
      private System.Windows.Forms.ColumnHeader columnComment;
   }
}
