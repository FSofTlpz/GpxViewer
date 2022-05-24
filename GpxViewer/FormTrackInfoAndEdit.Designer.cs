namespace GpxViewer {
   partial class FormTrackInfoAndEdit {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent() {
         this.components = new System.ComponentModel.Container();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
         System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
         this.contextMenuStripText = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItem_CopyText = new System.Windows.Forms.ToolStripMenuItem();
         this.contextMenuStripPicture = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItem_CopyPicture = new System.Windows.Forms.ToolStripMenuItem();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.dataGridViewPoints = new System.Windows.Forms.DataGridView();
         this.columnIdxDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.ColumnLat = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.columnLonDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.columnElevationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.columnTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.columnDistanceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.columnLengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.contextMenuStripPoints = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItem_PointsRemoving = new System.Windows.Forms.ToolStripMenuItem();
         this.dataSet1 = new System.Data.DataSet();
         this.dataTablePoints = new System.Data.DataTable();
         this.dataColumnIdx = new System.Data.DataColumn();
         this.dataColumnLat = new System.Data.DataColumn();
         this.dataColumnLon = new System.Data.DataColumn();
         this.dataColumnElevation = new System.Data.DataColumn();
         this.dataColumnDistance = new System.Data.DataColumn();
         this.dataColumnLength = new System.Data.DataColumn();
         this.dataColumnTime = new System.Data.DataColumn();
         this.labelProfile1 = new System.Windows.Forms.Label();
         this.pictureBoxProfile1 = new System.Windows.Forms.PictureBox();
         this.label_TrackLength = new System.Windows.Forms.Label();
         this.label5 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.textBoxSource = new System.Windows.Forms.TextBox();
         this.label7 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.textBoxComment = new System.Windows.Forms.TextBox();
         this.textBoxDescription = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.textBoxName = new System.Windows.Forms.TextBox();
         this.button_Save = new System.Windows.Forms.Button();
         this.colorDialog1 = new System.Windows.Forms.ColorDialog();
         this.contextMenuStripText.SuspendLayout();
         this.contextMenuStripPicture.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPoints)).BeginInit();
         this.contextMenuStripPoints.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.dataTablePoints)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile1)).BeginInit();
         this.SuspendLayout();
         // 
         // contextMenuStripText
         // 
         this.contextMenuStripText.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_CopyText});
         this.contextMenuStripText.Name = "contextMenuStripText";
         this.contextMenuStripText.Size = new System.Drawing.Size(265, 26);
         // 
         // ToolStripMenuItem_CopyText
         // 
         this.ToolStripMenuItem_CopyText.Name = "ToolStripMenuItem_CopyText";
         this.ToolStripMenuItem_CopyText.Size = new System.Drawing.Size(264, 22);
         this.ToolStripMenuItem_CopyText.Text = "Text in die Zwischenablage kopieren";
         this.ToolStripMenuItem_CopyText.Click += new System.EventHandler(this.ToolStripMenuItem_CopyText_Click);
         // 
         // contextMenuStripPicture
         // 
         this.contextMenuStripPicture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_CopyPicture});
         this.contextMenuStripPicture.Name = "contextMenuStripPicture";
         this.contextMenuStripPicture.Size = new System.Drawing.Size(275, 26);
         // 
         // ToolStripMenuItem_CopyPicture
         // 
         this.ToolStripMenuItem_CopyPicture.Name = "ToolStripMenuItem_CopyPicture";
         this.ToolStripMenuItem_CopyPicture.Size = new System.Drawing.Size(274, 22);
         this.ToolStripMenuItem_CopyPicture.Text = "Grafik in die Zwischenablage kopieren";
         this.ToolStripMenuItem_CopyPicture.Click += new System.EventHandler(this.ToolStripMenuItem_CopyPicture_Click);
         // 
         // splitContainer1
         // 
         this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.splitContainer1.Location = new System.Drawing.Point(12, 151);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.dataGridViewPoints);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.AutoScroll = true;
         this.splitContainer1.Panel2.Controls.Add(this.labelProfile1);
         this.splitContainer1.Panel2.Controls.Add(this.pictureBoxProfile1);
         this.splitContainer1.Size = new System.Drawing.Size(602, 518);
         this.splitContainer1.SplitterDistance = 164;
         this.splitContainer1.TabIndex = 14;
         // 
         // dataGridViewPoints
         // 
         this.dataGridViewPoints.AllowUserToAddRows = false;
         this.dataGridViewPoints.AllowUserToDeleteRows = false;
         this.dataGridViewPoints.AllowUserToOrderColumns = true;
         dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
         this.dataGridViewPoints.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
         this.dataGridViewPoints.AutoGenerateColumns = false;
         this.dataGridViewPoints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
         this.dataGridViewPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.dataGridViewPoints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnIdxDataGridViewTextBoxColumn,
            this.ColumnLat,
            this.columnLonDataGridViewTextBoxColumn,
            this.columnElevationDataGridViewTextBoxColumn,
            this.columnTimeDataGridViewTextBoxColumn,
            this.columnDistanceDataGridViewTextBoxColumn,
            this.columnLengthDataGridViewTextBoxColumn});
         this.dataGridViewPoints.ContextMenuStrip = this.contextMenuStripPoints;
         this.dataGridViewPoints.DataMember = "PointsTable";
         this.dataGridViewPoints.DataSource = this.dataSet1;
         this.dataGridViewPoints.Dock = System.Windows.Forms.DockStyle.Fill;
         this.dataGridViewPoints.Location = new System.Drawing.Point(0, 0);
         this.dataGridViewPoints.Name = "dataGridViewPoints";
         this.dataGridViewPoints.ReadOnly = true;
         this.dataGridViewPoints.RowHeadersVisible = false;
         this.dataGridViewPoints.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
         this.dataGridViewPoints.Size = new System.Drawing.Size(602, 164);
         this.dataGridViewPoints.TabIndex = 0;
         this.dataGridViewPoints.SelectionChanged += new System.EventHandler(this.dataGridViewPoints_SelectionChanged);
         this.dataGridViewPoints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewPoints_KeyDown);
         // 
         // columnIdxDataGridViewTextBoxColumn
         // 
         this.columnIdxDataGridViewTextBoxColumn.DataPropertyName = "ColumnIdx";
         this.columnIdxDataGridViewTextBoxColumn.HeaderText = "Nummer";
         this.columnIdxDataGridViewTextBoxColumn.Name = "columnIdxDataGridViewTextBoxColumn";
         this.columnIdxDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnIdxDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnIdxDataGridViewTextBoxColumn.Width = 52;
         // 
         // ColumnLat
         // 
         this.ColumnLat.DataPropertyName = "ColumnLat";
         this.ColumnLat.HeaderText = "geogr. Länge";
         this.ColumnLat.Name = "ColumnLat";
         this.ColumnLat.ReadOnly = true;
         this.ColumnLat.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.ColumnLat.Width = 68;
         // 
         // columnLonDataGridViewTextBoxColumn
         // 
         this.columnLonDataGridViewTextBoxColumn.DataPropertyName = "ColumnLon";
         this.columnLonDataGridViewTextBoxColumn.HeaderText = "geogr. Breite";
         this.columnLonDataGridViewTextBoxColumn.Name = "columnLonDataGridViewTextBoxColumn";
         this.columnLonDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnLonDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnLonDataGridViewTextBoxColumn.Width = 66;
         // 
         // columnElevationDataGridViewTextBoxColumn
         // 
         this.columnElevationDataGridViewTextBoxColumn.DataPropertyName = "ColumnElevation";
         dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
         this.columnElevationDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle7;
         this.columnElevationDataGridViewTextBoxColumn.HeaderText = "Höhe (m)";
         this.columnElevationDataGridViewTextBoxColumn.Name = "columnElevationDataGridViewTextBoxColumn";
         this.columnElevationDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnElevationDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnElevationDataGridViewTextBoxColumn.Width = 50;
         // 
         // columnTimeDataGridViewTextBoxColumn
         // 
         this.columnTimeDataGridViewTextBoxColumn.DataPropertyName = "ColumnTime";
         dataGridViewCellStyle8.Format = "G";
         dataGridViewCellStyle8.NullValue = null;
         this.columnTimeDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle8;
         this.columnTimeDataGridViewTextBoxColumn.HeaderText = "Zeitpunkt (UTC)";
         this.columnTimeDataGridViewTextBoxColumn.Name = "columnTimeDataGridViewTextBoxColumn";
         this.columnTimeDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnTimeDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnTimeDataGridViewTextBoxColumn.Width = 80;
         // 
         // columnDistanceDataGridViewTextBoxColumn
         // 
         this.columnDistanceDataGridViewTextBoxColumn.DataPropertyName = "ColumnDistance";
         dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
         this.columnDistanceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle9;
         this.columnDistanceDataGridViewTextBoxColumn.HeaderText = "Entfernung (m)";
         this.columnDistanceDataGridViewTextBoxColumn.Name = "columnDistanceDataGridViewTextBoxColumn";
         this.columnDistanceDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnDistanceDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnDistanceDataGridViewTextBoxColumn.Width = 74;
         // 
         // columnLengthDataGridViewTextBoxColumn
         // 
         this.columnLengthDataGridViewTextBoxColumn.DataPropertyName = "ColumnLength";
         dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
         this.columnLengthDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle10;
         this.columnLengthDataGridViewTextBoxColumn.HeaderText = "Streckenlänge (m)";
         this.columnLengthDataGridViewTextBoxColumn.Name = "columnLengthDataGridViewTextBoxColumn";
         this.columnLengthDataGridViewTextBoxColumn.ReadOnly = true;
         this.columnLengthDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
         this.columnLengthDataGridViewTextBoxColumn.Width = 89;
         // 
         // contextMenuStripPoints
         // 
         this.contextMenuStripPoints.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_PointsRemoving});
         this.contextMenuStripPoints.Name = "contextMenuStripPoints";
         this.contextMenuStripPoints.Size = new System.Drawing.Size(209, 26);
         this.contextMenuStripPoints.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripPoints_Opening);
         // 
         // ToolStripMenuItem_PointsRemoving
         // 
         this.ToolStripMenuItem_PointsRemoving.Image = global::GpxViewer.Properties.Resources.delete;
         this.ToolStripMenuItem_PointsRemoving.Name = "ToolStripMenuItem_PointsRemoving";
         this.ToolStripMenuItem_PointsRemoving.Size = new System.Drawing.Size(208, 22);
         this.ToolStripMenuItem_PointsRemoving.Text = "markierte Punkte löschen";
         this.ToolStripMenuItem_PointsRemoving.Click += new System.EventHandler(this.ToolStripMenuItem_PointsRemoving_Click);
         // 
         // dataSet1
         // 
         this.dataSet1.DataSetName = "NewDataSet";
         this.dataSet1.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTablePoints});
         // 
         // dataTablePoints
         // 
         this.dataTablePoints.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumnIdx,
            this.dataColumnLat,
            this.dataColumnLon,
            this.dataColumnElevation,
            this.dataColumnDistance,
            this.dataColumnLength,
            this.dataColumnTime});
         this.dataTablePoints.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "ColumnIdx"}, false)});
         this.dataTablePoints.TableName = "PointsTable";
         // 
         // dataColumnIdx
         // 
         this.dataColumnIdx.Caption = "ColumnIdx";
         this.dataColumnIdx.ColumnName = "ColumnIdx";
         this.dataColumnIdx.DataType = typeof(int);
         this.dataColumnIdx.ReadOnly = true;
         // 
         // dataColumnLat
         // 
         this.dataColumnLat.Caption = "ColumnLat";
         this.dataColumnLat.ColumnName = "ColumnLat";
         this.dataColumnLat.DataType = typeof(double);
         this.dataColumnLat.ReadOnly = true;
         // 
         // dataColumnLon
         // 
         this.dataColumnLon.Caption = "ColumnLon";
         this.dataColumnLon.ColumnName = "ColumnLon";
         this.dataColumnLon.DataType = typeof(double);
         this.dataColumnLon.ReadOnly = true;
         // 
         // dataColumnElevation
         // 
         this.dataColumnElevation.Caption = "ColumnElevation";
         this.dataColumnElevation.ColumnName = "ColumnElevation";
         this.dataColumnElevation.DataType = typeof(double);
         this.dataColumnElevation.ReadOnly = true;
         // 
         // dataColumnDistance
         // 
         this.dataColumnDistance.Caption = "ColumnDistance";
         this.dataColumnDistance.ColumnName = "ColumnDistance";
         this.dataColumnDistance.DataType = typeof(double);
         this.dataColumnDistance.ReadOnly = true;
         // 
         // dataColumnLength
         // 
         this.dataColumnLength.Caption = "ColumnLength";
         this.dataColumnLength.ColumnName = "ColumnLength";
         this.dataColumnLength.DataType = typeof(double);
         this.dataColumnLength.ReadOnly = true;
         // 
         // dataColumnTime
         // 
         this.dataColumnTime.Caption = "ColumnTime";
         this.dataColumnTime.ColumnName = "ColumnTime";
         this.dataColumnTime.DataType = typeof(System.DateTime);
         this.dataColumnTime.ReadOnly = true;
         // 
         // labelProfile1
         // 
         this.labelProfile1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.labelProfile1.BackColor = System.Drawing.Color.Gainsboro;
         this.labelProfile1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.labelProfile1.ContextMenuStrip = this.contextMenuStripText;
         this.labelProfile1.Location = new System.Drawing.Point(0, 0);
         this.labelProfile1.Name = "labelProfile1";
         this.labelProfile1.Size = new System.Drawing.Size(602, 26);
         this.labelProfile1.TabIndex = 1;
         this.labelProfile1.Text = "text";
         // 
         // pictureBoxProfile1
         // 
         this.pictureBoxProfile1.ContextMenuStrip = this.contextMenuStripPicture;
         this.pictureBoxProfile1.Location = new System.Drawing.Point(8, 40);
         this.pictureBoxProfile1.Name = "pictureBoxProfile1";
         this.pictureBoxProfile1.Size = new System.Drawing.Size(100, 50);
         this.pictureBoxProfile1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBoxProfile1.TabIndex = 0;
         this.pictureBoxProfile1.TabStop = false;
         // 
         // label_TrackLength
         // 
         this.label_TrackLength.AutoSize = true;
         this.label_TrackLength.Location = new System.Drawing.Point(105, 124);
         this.label_TrackLength.Name = "label_TrackLength";
         this.label_TrackLength.Size = new System.Drawing.Size(13, 13);
         this.label_TrackLength.TabIndex = 9;
         this.label_TrackLength.Text = "0";
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(9, 124);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(40, 13);
         this.label5.TabIndex = 8;
         this.label5.Text = "Länge:";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(9, 93);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(40, 13);
         this.label2.TabIndex = 6;
         this.label2.Text = "Quelle:";
         // 
         // textBoxSource
         // 
         this.textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxSource.Location = new System.Drawing.Point(106, 90);
         this.textBoxSource.Name = "textBoxSource";
         this.textBoxSource.Size = new System.Drawing.Size(508, 20);
         this.textBoxSource.TabIndex = 7;
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(9, 67);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(63, 13);
         this.label7.TabIndex = 4;
         this.label7.Text = "Kommentar:";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(9, 41);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(75, 13);
         this.label6.TabIndex = 2;
         this.label6.Text = "Beschreibung:";
         // 
         // textBoxComment
         // 
         this.textBoxComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxComment.Location = new System.Drawing.Point(106, 64);
         this.textBoxComment.Name = "textBoxComment";
         this.textBoxComment.Size = new System.Drawing.Size(508, 20);
         this.textBoxComment.TabIndex = 5;
         // 
         // textBoxDescription
         // 
         this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxDescription.Location = new System.Drawing.Point(106, 38);
         this.textBoxDescription.Name = "textBoxDescription";
         this.textBoxDescription.Size = new System.Drawing.Size(508, 20);
         this.textBoxDescription.TabIndex = 3;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(9, 15);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(38, 13);
         this.label4.TabIndex = 0;
         this.label4.Text = "Name:";
         // 
         // textBoxName
         // 
         this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxName.Location = new System.Drawing.Point(106, 12);
         this.textBoxName.Name = "textBoxName";
         this.textBoxName.Size = new System.Drawing.Size(508, 20);
         this.textBoxName.TabIndex = 1;
         // 
         // button_Save
         // 
         this.button_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_Save.Image = global::GpxViewer.Properties.Resources.speichern;
         this.button_Save.Location = new System.Drawing.Point(12, 675);
         this.button_Save.Name = "button_Save";
         this.button_Save.Size = new System.Drawing.Size(120, 32);
         this.button_Save.TabIndex = 10;
         this.button_Save.Text = "speichern";
         this.button_Save.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
         this.button_Save.UseVisualStyleBackColor = true;
         this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
         // 
         // FormTrackInfoAndEdit
         // 
         this.AcceptButton = this.button_Save;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.AutoScroll = true;
         this.ClientSize = new System.Drawing.Size(626, 719);
         this.Controls.Add(this.button_Save);
         this.Controls.Add(this.splitContainer1);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.label7);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.textBoxSource);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.textBoxName);
         this.Controls.Add(this.textBoxComment);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.textBoxDescription);
         this.Controls.Add(this.label_TrackLength);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.KeyPreview = true;
         this.MinimumSize = new System.Drawing.Size(507, 449);
         this.Name = "FormTrackInfoAndEdit";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "FormExteRouteInfo";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormExtTrackInfoAndEdit_FormClosing);
         this.Load += new System.EventHandler(this.FormExtTrackInfoAndEdit_Load);
         this.Shown += new System.EventHandler(this.FormExtTrackInfoAndEdit_Shown);
         this.ClientSizeChanged += new System.EventHandler(this.FormExtTrackInfoAndEdit_ClientSizeChanged);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormExtTrackInfoAndEdit_KeyDown);
         this.contextMenuStripText.ResumeLayout(false);
         this.contextMenuStripPicture.ResumeLayout(false);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPoints)).EndInit();
         this.contextMenuStripPoints.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.dataTablePoints)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStripText;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CopyText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPicture;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CopyPicture;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxComment;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.DataGridView dataGridViewPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnIdxDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLat;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLonDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnElevationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDistanceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLengthDataGridViewTextBoxColumn;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPoints;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_PointsRemoving;
        private System.Data.DataSet dataSet1;
        private System.Data.DataTable dataTablePoints;
        private System.Data.DataColumn dataColumnIdx;
        private System.Data.DataColumn dataColumnLat;
        private System.Data.DataColumn dataColumnLon;
        private System.Data.DataColumn dataColumnElevation;
        private System.Data.DataColumn dataColumnDistance;
        private System.Data.DataColumn dataColumnLength;
        private System.Data.DataColumn dataColumnTime;
        private System.Windows.Forms.ColorDialog colorDialog1;
      private System.Windows.Forms.Label label_TrackLength;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.PictureBox pictureBoxProfile1;
      private System.Windows.Forms.Label labelProfile1;
   }
}