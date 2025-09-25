namespace GpxViewer.PictureEdit {
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
         components = new System.ComponentModel.Container();
         toolStripContainer1 = new ToolStripContainer();
         statusStrip1 = new StatusStrip();
         toolStripStatusLabel_Path = new ToolStripStatusLabel();
         toolStripStatusLabel_Count = new ToolStripStatusLabel();
         toolStripStatusLabel_Filename = new ToolStripStatusLabel();
         splitContainer1 = new SplitContainer();
         listView1 = new ListView();
         columnPicture = new ColumnHeader();
         columnFile = new ColumnHeader();
         columnFileDate = new ColumnHeader();
         columnPictureDate = new ColumnHeader();
         columnCoordinates = new ColumnHeader();
         columnDirection = new ColumnHeader();
         columnComment = new ColumnHeader();
         contextMenuStripListView = new ContextMenuStrip(components);
         ToolStripMenuItemSave = new ToolStripMenuItem();
         ToolStripMenuItemShow = new ToolStripMenuItem();
         ToolStripMenuItemSet = new ToolStripMenuItem();
         ToolStripMenuItemSet2 = new ToolStripMenuItem();
         ToolStripMenuItemEditComment = new ToolStripMenuItem();
         ToolStripMenuItemEditFilename = new ToolStripMenuItem();
         ToolStripMenuItemEditDateTime = new ToolStripMenuItem();
         pictureBox1 = new PictureBox();
         contextMenuStripPicturebox = new ContextMenuStrip(components);
         toolStripMenuItemShowPictExtern = new ToolStripMenuItem();
         toolStripMenuItemShowPictExternExt = new ToolStripMenuItem();
         toolStrip1 = new ToolStrip();
         toolStripButton_OpenPath = new ToolStripButton();
         toolStripButton_Reload = new ToolStripButton();
         toolStripButton_WithSubDirs = new ToolStripButton();
         toolStripButton_SaveAll = new ToolStripButton();
         toolStripButton_SaveGpx = new ToolStripButton();
         toolStripSeparator2 = new ToolStripSeparator();
         toolStripDropDownButton1 = new ToolStripDropDownButton();
         ToolStripMenuItem_ViewAll = new ToolStripMenuItem();
         ToolStripMenuItem_ViewWithGeo = new ToolStripMenuItem();
         ToolStripMenuItem_ViewWithoutGeo = new ToolStripMenuItem();
         toolStripDropDownButton2 = new ToolStripDropDownButton();
         ToolStripMenuItem_FilenameAsc = new ToolStripMenuItem();
         ToolStripMenuItem_FilenameDesc = new ToolStripMenuItem();
         ToolStripMenuItem_FiledateAsc = new ToolStripMenuItem();
         ToolStripMenuItem_FiledateDesc = new ToolStripMenuItem();
         ToolStripMenuItem_GeodateAsc = new ToolStripMenuItem();
         ToolStripMenuItem_GeodateDesc = new ToolStripMenuItem();
         toolStripSeparator1 = new ToolStripSeparator();
         toolStripButton_SwapView = new ToolStripButton();
         folderBrowserDialog1 = new FolderBrowserDialog();
         saveFileDialog1 = new SaveFileDialog();
         toolStripContainer1.BottomToolStripPanel.SuspendLayout();
         toolStripContainer1.ContentPanel.SuspendLayout();
         toolStripContainer1.TopToolStripPanel.SuspendLayout();
         toolStripContainer1.SuspendLayout();
         statusStrip1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         contextMenuStripListView.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
         contextMenuStripPicturebox.SuspendLayout();
         toolStrip1.SuspendLayout();
         SuspendLayout();
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
         toolStripContainer1.ContentPanel.Controls.Add(splitContainer1);
         toolStripContainer1.ContentPanel.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer1.ContentPanel.Size = new Size(798, 530);
         toolStripContainer1.Dock = DockStyle.Fill;
         toolStripContainer1.Location = new Point(0, 0);
         toolStripContainer1.Margin = new Padding(4, 3, 4, 3);
         toolStripContainer1.Name = "toolStripContainer1";
         toolStripContainer1.Size = new Size(798, 579);
         toolStripContainer1.TabIndex = 0;
         toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
         // 
         // statusStrip1
         // 
         statusStrip1.Dock = DockStyle.None;
         statusStrip1.ImageScalingSize = new Size(20, 20);
         statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_Path, toolStripStatusLabel_Count, toolStripStatusLabel_Filename });
         statusStrip1.Location = new Point(0, 0);
         statusStrip1.Name = "statusStrip1";
         statusStrip1.Size = new Size(798, 22);
         statusStrip1.TabIndex = 0;
         // 
         // toolStripStatusLabel_Path
         // 
         toolStripStatusLabel_Path.Name = "toolStripStatusLabel_Path";
         toolStripStatusLabel_Path.Size = new Size(0, 17);
         // 
         // toolStripStatusLabel_Count
         // 
         toolStripStatusLabel_Count.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
         toolStripStatusLabel_Count.BorderStyle = Border3DStyle.Sunken;
         toolStripStatusLabel_Count.Font = new Font("Segoe UI", 9F);
         toolStripStatusLabel_Count.Name = "toolStripStatusLabel_Count";
         toolStripStatusLabel_Count.Size = new Size(4, 17);
         // 
         // toolStripStatusLabel_Filename
         // 
         toolStripStatusLabel_Filename.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
         toolStripStatusLabel_Filename.BorderStyle = Border3DStyle.Sunken;
         toolStripStatusLabel_Filename.Name = "toolStripStatusLabel_Filename";
         toolStripStatusLabel_Filename.Size = new Size(4, 17);
         // 
         // splitContainer1
         // 
         splitContainer1.BorderStyle = BorderStyle.Fixed3D;
         splitContainer1.Dock = DockStyle.Fill;
         splitContainer1.Location = new Point(0, 0);
         splitContainer1.Margin = new Padding(4, 2, 4, 2);
         splitContainer1.Name = "splitContainer1";
         splitContainer1.Orientation = Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         splitContainer1.Panel1.Controls.Add(listView1);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.Controls.Add(pictureBox1);
         splitContainer1.Panel2.Padding = new Padding(4, 3, 4, 3);
         splitContainer1.Size = new Size(798, 530);
         splitContainer1.SplitterDistance = 252;
         splitContainer1.SplitterWidth = 3;
         splitContainer1.TabIndex = 3;
         // 
         // listView1
         // 
         listView1.AllowColumnReorder = true;
         listView1.Columns.AddRange(new ColumnHeader[] { columnPicture, columnFile, columnFileDate, columnPictureDate, columnCoordinates, columnDirection, columnComment });
         listView1.ContextMenuStrip = contextMenuStripListView;
         listView1.Dock = DockStyle.Fill;
         listView1.ForeColor = SystemColors.WindowText;
         listView1.FullRowSelect = true;
         listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
         listView1.Location = new Point(0, 0);
         listView1.Margin = new Padding(4, 3, 4, 3);
         listView1.Name = "listView1";
         listView1.ShowItemToolTips = true;
         listView1.Size = new Size(794, 248);
         listView1.TabIndex = 0;
         listView1.TileSize = new Size(228, 100);
         listView1.UseCompatibleStateImageBehavior = false;
         listView1.View = View.Tile;
         // 
         // columnPicture
         // 
         columnPicture.Text = "Bild";
         // 
         // columnFile
         // 
         columnFile.Text = "Dateiname";
         columnFile.Width = 50;
         // 
         // columnFileDate
         // 
         columnFileDate.Text = "Dateidatum";
         // 
         // columnPictureDate
         // 
         columnPictureDate.Text = "Bilddatum";
         // 
         // columnCoordinates
         // 
         columnCoordinates.Text = "Koordinaten";
         // 
         // columnDirection
         // 
         columnDirection.Text = "Richtung";
         // 
         // columnComment
         // 
         columnComment.Text = "Kommentar";
         // 
         // contextMenuStrip1
         // 
         contextMenuStripListView.ImageScalingSize = new Size(20, 20);
         contextMenuStripListView.Items.AddRange(new ToolStripItem[] { ToolStripMenuItemSave, ToolStripMenuItemShow, ToolStripMenuItemSet, ToolStripMenuItemSet2, ToolStripMenuItemEditComment, ToolStripMenuItemEditFilename, ToolStripMenuItemEditDateTime });
         contextMenuStripListView.Name = "contextMenuStripListView";
         contextMenuStripListView.Size = new Size(362, 158);
         contextMenuStripListView.Opening += contextMenuStripListView_Opening;
         // 
         // ToolStripMenuItemSave
         // 
         ToolStripMenuItemSave.Name = "ToolStripMenuItemSave";
         ToolStripMenuItemSave.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
         ToolStripMenuItemSave.Size = new Size(361, 22);
         ToolStripMenuItemSave.Text = "akt. Daten des Bildes &speichern";
         ToolStripMenuItemSave.Click += ToolStripMenuItemSave_Click;
         // 
         // ToolStripMenuItemShow
         // 
         ToolStripMenuItemShow.Name = "ToolStripMenuItemShow";
         ToolStripMenuItemShow.ShortcutKeys = Keys.Control | Keys.A;
         ToolStripMenuItemShow.Size = new Size(361, 22);
         ToolStripMenuItemShow.Text = "Position auf Karte &anzeigen";
         ToolStripMenuItemShow.Click += ToolStripMenuItemShow_Click;
         // 
         // ToolStripMenuItemSet
         // 
         ToolStripMenuItemSet.Name = "ToolStripMenuItemSet";
         ToolStripMenuItemSet.ShortcutKeys = Keys.Control | Keys.P;
         ToolStripMenuItemSet.Size = new Size(361, 22);
         ToolStripMenuItemSet.Text = "neue &Position setzen";
         ToolStripMenuItemSet.Click += ToolStripMenuItemSet_Click;
         // 
         // ToolStripMenuItemSet2
         // 
         ToolStripMenuItemSet2.Name = "ToolStripMenuItemSet2";
         ToolStripMenuItemSet2.Size = new Size(361, 22);
         ToolStripMenuItemSet2.Text = "neue Position über angezeigte GP&X-Tracks setzen";
         ToolStripMenuItemSet2.Click += ToolStripMenuItemSet2_Click;
         // 
         // ToolStripMenuItemEditComment
         // 
         ToolStripMenuItemEditComment.Name = "ToolStripMenuItemEditComment";
         ToolStripMenuItemEditComment.ShortcutKeys = Keys.F2;
         ToolStripMenuItemEditComment.Size = new Size(361, 22);
         ToolStripMenuItemEditComment.Text = "&Kommentar ändern";
         ToolStripMenuItemEditComment.Click += ToolStripMenuItemEditComment_Click;
         // 
         // ToolStripMenuItemEditFilename
         // 
         ToolStripMenuItemEditFilename.Name = "ToolStripMenuItemEditFilename";
         ToolStripMenuItemEditFilename.ShortcutKeys = Keys.Control | Keys.D;
         ToolStripMenuItemEditFilename.Size = new Size(361, 22);
         ToolStripMenuItemEditFilename.Text = "&Dateiname ändern";
         ToolStripMenuItemEditFilename.Click += ToolStripMenuItemEditFilename_Click;
         // 
         // ToolStripMenuItemEditDateTime
         // 
         ToolStripMenuItemEditDateTime.Name = "ToolStripMenuItemEditDateTime";
         ToolStripMenuItemEditDateTime.Size = new Size(361, 22);
         ToolStripMenuItemEditDateTime.Text = "Datum/&Uhrzeit ändern";
         ToolStripMenuItemEditDateTime.Click += ToolStripMenuItemEditDateTime_Click;
         // 
         // pictureBox1
         // 
         pictureBox1.ContextMenuStrip = contextMenuStripPicturebox;
         pictureBox1.Dock = DockStyle.Fill;
         pictureBox1.Location = new Point(4, 3);
         pictureBox1.Margin = new Padding(4, 2, 4, 2);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Size = new Size(786, 265);
         pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
         pictureBox1.TabIndex = 0;
         pictureBox1.TabStop = false;
         pictureBox1.DoubleClick += pictureBox1_DoubleClick;
         // 
         // contextMenuStrip2
         // 
         contextMenuStripPicturebox.Items.AddRange(new ToolStripItem[] { toolStripMenuItemShowPictExtern, toolStripMenuItemShowPictExternExt });
         contextMenuStripPicturebox.Name = "contextMenuStripPicturebox";
         contextMenuStripPicturebox.Size = new Size(304, 70);
         contextMenuStripPicturebox.Opening += contextMenuStripPicturebox_Opening;
         // 
         // toolStripMenuItemShowPictExtern
         // 
         toolStripMenuItemShowPictExtern.Name = "toolStripMenuItemShowPictExtern";
         toolStripMenuItemShowPictExtern.Size = new Size(303, 22);
         toolStripMenuItemShowPictExtern.Text = "Bild extern anzeigen";
         toolStripMenuItemShowPictExtern.Click += toolStripMenuItemShowPictExtern_Click;
         // 
         // toolStripMenuItemShowPictExternExt
         // 
         toolStripMenuItemShowPictExternExt.Name = "toolStripMenuItemShowPictExternExt";
         toolStripMenuItemShowPictExternExt.Size = new Size(303, 22);
         toolStripMenuItemShowPictExternExt.Text = "Bild extern mit Programmauswahl anzeigen";
         toolStripMenuItemShowPictExternExt.Click += toolStripMenuItemShowPictExternExt_Click;
         // 
         // toolStrip1
         // 
         toolStrip1.Dock = DockStyle.None;
         toolStrip1.ImageScalingSize = new Size(20, 20);
         toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton_OpenPath, toolStripButton_Reload, toolStripButton_WithSubDirs, toolStripButton_SaveAll, toolStripButton_SaveGpx, toolStripSeparator2, toolStripDropDownButton1, toolStripDropDownButton2, toolStripSeparator1, toolStripButton_SwapView });
         toolStrip1.Location = new Point(3, 0);
         toolStrip1.Name = "toolStrip1";
         toolStrip1.Size = new Size(234, 27);
         toolStrip1.TabIndex = 0;
         // 
         // toolStripButton_OpenPath
         // 
         toolStripButton_OpenPath.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_OpenPath.Image = Properties.Resources.Open;
         toolStripButton_OpenPath.ImageTransparentColor = Color.Magenta;
         toolStripButton_OpenPath.Name = "toolStripButton_OpenPath";
         toolStripButton_OpenPath.Size = new Size(24, 24);
         toolStripButton_OpenPath.Text = "Bildverzeichnis auswählen (STRG+O)";
         toolStripButton_OpenPath.Click += toolStripButton_OpenPath_Click;
         // 
         // toolStripButton_Reload
         // 
         toolStripButton_Reload.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Reload.Image = Properties.Resources.arrow_refresh;
         toolStripButton_Reload.ImageTransparentColor = Color.Magenta;
         toolStripButton_Reload.Name = "toolStripButton_Reload";
         toolStripButton_Reload.Size = new Size(24, 24);
         toolStripButton_Reload.Text = "neu einlesen (STRG+R)";
         toolStripButton_Reload.Click += toolStripButton_Reload_Click;
         // 
         // toolStripButton_WithSubDirs
         // 
         toolStripButton_WithSubDirs.CheckOnClick = true;
         toolStripButton_WithSubDirs.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_WithSubDirs.Image = Properties.Resources.subfolder;
         toolStripButton_WithSubDirs.ImageTransparentColor = Color.Magenta;
         toolStripButton_WithSubDirs.Name = "toolStripButton_WithSubDirs";
         toolStripButton_WithSubDirs.Size = new Size(24, 24);
         toolStripButton_WithSubDirs.Text = "Unterordner einbeziehen";
         toolStripButton_WithSubDirs.Click += toolStripButton_WithSubDirs_Click;
         // 
         // toolStripButton_SaveAll
         // 
         toolStripButton_SaveAll.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SaveAll.Image = Properties.Resources.speichern;
         toolStripButton_SaveAll.ImageTransparentColor = Color.Magenta;
         toolStripButton_SaveAll.Name = "toolStripButton_SaveAll";
         toolStripButton_SaveAll.Size = new Size(24, 24);
         toolStripButton_SaveAll.Text = "alle speichern (STRG+S)";
         toolStripButton_SaveAll.Click += toolStripButton_SaveAll_Click;
         // 
         // toolStripButton_SaveGpx
         // 
         toolStripButton_SaveGpx.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SaveGpx.Image = Properties.Resources.speicherngpx;
         toolStripButton_SaveGpx.ImageTransparentColor = Color.Magenta;
         toolStripButton_SaveGpx.Name = "toolStripButton_SaveGpx";
         toolStripButton_SaveGpx.Size = new Size(24, 24);
         toolStripButton_SaveGpx.Text = "markierte Bilder als Verweise in GPX-Datei speichern";
         toolStripButton_SaveGpx.Click += toolStripButton_SaveGpx_Click;
         // 
         // toolStripSeparator2
         // 
         toolStripSeparator2.Name = "toolStripSeparator2";
         toolStripSeparator2.Size = new Size(6, 27);
         // 
         // toolStripDropDownButton1
         // 
         toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { ToolStripMenuItem_ViewAll, ToolStripMenuItem_ViewWithGeo, ToolStripMenuItem_ViewWithoutGeo });
         toolStripDropDownButton1.Image = Properties.Resources.Filter;
         toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
         toolStripDropDownButton1.Name = "toolStripDropDownButton1";
         toolStripDropDownButton1.Size = new Size(33, 24);
         toolStripDropDownButton1.Text = "Bildfilter";
         // 
         // ToolStripMenuItem_ViewAll
         // 
         ToolStripMenuItem_ViewAll.Name = "ToolStripMenuItem_ViewAll";
         ToolStripMenuItem_ViewAll.Size = new Size(277, 22);
         ToolStripMenuItem_ViewAll.Text = "alle Bilder anzeigen";
         ToolStripMenuItem_ViewAll.Click += ToolStripMenuItem_ViewAll_Click;
         // 
         // ToolStripMenuItem_ViewWithGeo
         // 
         ToolStripMenuItem_ViewWithGeo.Name = "ToolStripMenuItem_ViewWithGeo";
         ToolStripMenuItem_ViewWithGeo.Size = new Size(277, 22);
         ToolStripMenuItem_ViewWithGeo.Text = "nur Bilder mit Geo-Position anzeigen";
         ToolStripMenuItem_ViewWithGeo.Click += ToolStripMenuItem_ViewWithGeo_Click;
         // 
         // ToolStripMenuItem_ViewWithoutGeo
         // 
         ToolStripMenuItem_ViewWithoutGeo.Name = "ToolStripMenuItem_ViewWithoutGeo";
         ToolStripMenuItem_ViewWithoutGeo.Size = new Size(277, 22);
         ToolStripMenuItem_ViewWithoutGeo.Text = "nur Bilder ohne Geo-Position anzeigen";
         ToolStripMenuItem_ViewWithoutGeo.Click += ToolStripMenuItem_ViewWithoutGeo_Click;
         // 
         // toolStripDropDownButton2
         // 
         toolStripDropDownButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { ToolStripMenuItem_FilenameAsc, ToolStripMenuItem_FilenameDesc, ToolStripMenuItem_FiledateAsc, ToolStripMenuItem_FiledateDesc, ToolStripMenuItem_GeodateAsc, ToolStripMenuItem_GeodateDesc });
         toolStripDropDownButton2.Image = Properties.Resources.Sort;
         toolStripDropDownButton2.ImageTransparentColor = Color.Magenta;
         toolStripDropDownButton2.Name = "toolStripDropDownButton2";
         toolStripDropDownButton2.Size = new Size(33, 24);
         toolStripDropDownButton2.Text = "Bildsortierung";
         // 
         // ToolStripMenuItem_FilenameAsc
         // 
         ToolStripMenuItem_FilenameAsc.Name = "ToolStripMenuItem_FilenameAsc";
         ToolStripMenuItem_FilenameAsc.Size = new Size(230, 22);
         ToolStripMenuItem_FilenameAsc.Text = "Dateiname aufsteigend";
         ToolStripMenuItem_FilenameAsc.Click += ToolStripMenuItem_FilenameAsc_Click;
         // 
         // ToolStripMenuItem_FilenameDesc
         // 
         ToolStripMenuItem_FilenameDesc.Name = "ToolStripMenuItem_FilenameDesc";
         ToolStripMenuItem_FilenameDesc.Size = new Size(230, 22);
         ToolStripMenuItem_FilenameDesc.Text = "Dateiname absteigend";
         ToolStripMenuItem_FilenameDesc.Click += ToolStripMenuItem_FilenameDesc_Click;
         // 
         // ToolStripMenuItem_FiledateAsc
         // 
         ToolStripMenuItem_FiledateAsc.Name = "ToolStripMenuItem_FiledateAsc";
         ToolStripMenuItem_FiledateAsc.Size = new Size(230, 22);
         ToolStripMenuItem_FiledateAsc.Text = "Dateidatum aufsteigend";
         ToolStripMenuItem_FiledateAsc.Click += ToolStripMenuItem_FiledateAsc_Click;
         // 
         // ToolStripMenuItem_FiledateDesc
         // 
         ToolStripMenuItem_FiledateDesc.Name = "ToolStripMenuItem_FiledateDesc";
         ToolStripMenuItem_FiledateDesc.Size = new Size(230, 22);
         ToolStripMenuItem_FiledateDesc.Text = "Dateidatum absteigend";
         ToolStripMenuItem_FiledateDesc.Click += ToolStripMenuItem_FiledateDesc_Click;
         // 
         // ToolStripMenuItem_GeodateAsc
         // 
         ToolStripMenuItem_GeodateAsc.Name = "ToolStripMenuItem_GeodateAsc";
         ToolStripMenuItem_GeodateAsc.Size = new Size(230, 22);
         ToolStripMenuItem_GeodateAsc.Text = "Aufnahmedatum aufsteigend";
         ToolStripMenuItem_GeodateAsc.Click += ToolStripMenuItem_GeodateAsc_Click;
         // 
         // ToolStripMenuItem_GeodateDesc
         // 
         ToolStripMenuItem_GeodateDesc.Name = "ToolStripMenuItem_GeodateDesc";
         ToolStripMenuItem_GeodateDesc.Size = new Size(230, 22);
         ToolStripMenuItem_GeodateDesc.Text = "Aufnahmedatum absteigend";
         ToolStripMenuItem_GeodateDesc.Click += ToolStripMenuItem_GeodateDesc_Click;
         // 
         // toolStripSeparator1
         // 
         toolStripSeparator1.Name = "toolStripSeparator1";
         toolStripSeparator1.Size = new Size(6, 27);
         // 
         // toolStripButton_SwapView
         // 
         toolStripButton_SwapView.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_SwapView.Image = Properties.Resources.table;
         toolStripButton_SwapView.ImageTransparentColor = Color.Magenta;
         toolStripButton_SwapView.Name = "toolStripButton_SwapView";
         toolStripButton_SwapView.Size = new Size(24, 24);
         toolStripButton_SwapView.Text = "Ansicht wechseln (STRG+W)";
         toolStripButton_SwapView.Click += toolStripButton_SwapView_Click;
         // 
         // folderBrowserDialog1
         // 
         folderBrowserDialog1.Description = "Ordner mit den Bildern auswählen";
         folderBrowserDialog1.ShowNewFolderButton = false;
         // 
         // saveFileDialog1
         // 
         saveFileDialog1.Filter = "GPX-Dateien|*.gpx";
         saveFileDialog1.Title = "Bilder als Verweise in einer GPX-Datei speichern";
         // 
         // PictureManager
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(toolStripContainer1);
         Margin = new Padding(4, 3, 4, 3);
         Name = "PictureManager";
         Size = new Size(798, 579);
         Load += PictureManager_Load;
         toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
         toolStripContainer1.BottomToolStripPanel.PerformLayout();
         toolStripContainer1.ContentPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.PerformLayout();
         toolStripContainer1.ResumeLayout(false);
         toolStripContainer1.PerformLayout();
         statusStrip1.ResumeLayout(false);
         statusStrip1.PerformLayout();
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
         splitContainer1.ResumeLayout(false);
         contextMenuStripListView.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
         contextMenuStripPicturebox.ResumeLayout(false);
         toolStrip1.ResumeLayout(false);
         toolStrip1.PerformLayout();
         ResumeLayout(false);

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

      private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
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
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemEditDateTime;
      private ContextMenuStrip contextMenuStripPicturebox;
      private ToolStripMenuItem toolStripMenuItemShowPictExtern;
      private ToolStripMenuItem toolStripMenuItemShowPictExternExt;
   }
}
