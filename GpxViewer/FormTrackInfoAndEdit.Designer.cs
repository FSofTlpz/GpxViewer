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
         components = new System.ComponentModel.Container();
         DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
         DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
         DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
         DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
         DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTrackInfoAndEdit));
         contextMenuStripText = new ContextMenuStrip(components);
         ToolStripMenuItem_CopyText = new ToolStripMenuItem();
         ToolStripMenuItem_CopyMarkedText = new ToolStripMenuItem();
         contextMenuStripPicture = new ContextMenuStrip(components);
         ToolStripMenuItem_CopyPicture = new ToolStripMenuItem();
         splitContainer1 = new SplitContainer();
         dataGridViewPoints = new DataGridView();
         columnIdxDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         ColumnLat = new DataGridViewTextBoxColumn();
         columnLonDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         columnElevationDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         columnTimeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         columnDistanceDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         columnLengthDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
         contextMenuStripPoints = new ContextMenuStrip(components);
         ToolStripMenuItem_PointsRemoving = new ToolStripMenuItem();
         dataSet1 = new System.Data.DataSet();
         dataTablePoints = new System.Data.DataTable();
         dataColumnIdx = new System.Data.DataColumn();
         dataColumnLat = new System.Data.DataColumn();
         dataColumnLon = new System.Data.DataColumn();
         dataColumnElevation = new System.Data.DataColumn();
         dataColumnDistance = new System.Data.DataColumn();
         dataColumnLength = new System.Data.DataColumn();
         dataColumnTime = new System.Data.DataColumn();
         richTextBoxInfo1 = new RichTextBox();
         pictureBoxProfile1 = new PictureBox();
         label_TrackLength = new Label();
         label5 = new Label();
         label2 = new Label();
         textBoxSource = new TextBox();
         label7 = new Label();
         label6 = new Label();
         textBoxComment = new TextBox();
         textBoxDescription = new TextBox();
         label4 = new Label();
         textBoxName = new TextBox();
         button_Save = new Button();
         colorDialog1 = new ColorDialog();
         contextMenuStripText.SuspendLayout();
         contextMenuStripPicture.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)dataGridViewPoints).BeginInit();
         contextMenuStripPoints.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)dataSet1).BeginInit();
         ((System.ComponentModel.ISupportInitialize)dataTablePoints).BeginInit();
         ((System.ComponentModel.ISupportInitialize)pictureBoxProfile1).BeginInit();
         SuspendLayout();
         // 
         // contextMenuStripText
         // 
         contextMenuStripText.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_CopyText, ToolStripMenuItem_CopyMarkedText });
         contextMenuStripText.Name = "contextMenuStripText";
         contextMenuStripText.Size = new Size(325, 48);
         contextMenuStripText.Opening += contextMenuStripText_Opening;
         // 
         // ToolStripMenuItem_CopyText
         // 
         ToolStripMenuItem_CopyText.Name = "ToolStripMenuItem_CopyText";
         ToolStripMenuItem_CopyText.Size = new Size(324, 22);
         ToolStripMenuItem_CopyText.Text = "Text in die Zwischenablage kopieren";
         ToolStripMenuItem_CopyText.Click += ToolStripMenuItem_CopyText_Click;
         // 
         // ToolStripMenuItem_CopyMarkedText
         // 
         ToolStripMenuItem_CopyMarkedText.Name = "ToolStripMenuItem_CopyMarkedText";
         ToolStripMenuItem_CopyMarkedText.Size = new Size(324, 22);
         ToolStripMenuItem_CopyMarkedText.Text = "markierten Text in die Zwischenablage kopieren";
         ToolStripMenuItem_CopyMarkedText.Click += ToolStripMenuItem_CopyMarkedText_Click;
         // 
         // contextMenuStripPicture
         // 
         contextMenuStripPicture.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_CopyPicture });
         contextMenuStripPicture.Name = "contextMenuStripPicture";
         contextMenuStripPicture.Size = new Size(275, 26);
         // 
         // ToolStripMenuItem_CopyPicture
         // 
         ToolStripMenuItem_CopyPicture.Name = "ToolStripMenuItem_CopyPicture";
         ToolStripMenuItem_CopyPicture.Size = new Size(274, 22);
         ToolStripMenuItem_CopyPicture.Text = "Grafik in die Zwischenablage kopieren";
         ToolStripMenuItem_CopyPicture.Click += ToolStripMenuItem_CopyPicture_Click;
         // 
         // splitContainer1
         // 
         splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         splitContainer1.Location = new Point(14, 174);
         splitContainer1.Margin = new Padding(4, 3, 4, 3);
         splitContainer1.Name = "splitContainer1";
         splitContainer1.Orientation = Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         splitContainer1.Panel1.Controls.Add(dataGridViewPoints);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.AutoScroll = true;
         splitContainer1.Panel2.Controls.Add(richTextBoxInfo1);
         splitContainer1.Panel2.Controls.Add(pictureBoxProfile1);
         splitContainer1.Size = new Size(707, 484);
         splitContainer1.SplitterDistance = 153;
         splitContainer1.SplitterWidth = 5;
         splitContainer1.TabIndex = 14;
         // 
         // dataGridViewPoints
         // 
         dataGridViewPoints.AllowUserToAddRows = false;
         dataGridViewPoints.AllowUserToDeleteRows = false;
         dataGridViewPoints.AllowUserToOrderColumns = true;
         dataGridViewCellStyle1.BackColor = Color.FromArgb(192, 192, 255);
         dataGridViewPoints.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
         dataGridViewPoints.AutoGenerateColumns = false;
         dataGridViewPoints.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
         dataGridViewPoints.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         dataGridViewPoints.Columns.AddRange(new DataGridViewColumn[] { columnIdxDataGridViewTextBoxColumn, ColumnLat, columnLonDataGridViewTextBoxColumn, columnElevationDataGridViewTextBoxColumn, columnTimeDataGridViewTextBoxColumn, columnDistanceDataGridViewTextBoxColumn, columnLengthDataGridViewTextBoxColumn });
         dataGridViewPoints.ContextMenuStrip = contextMenuStripPoints;
         dataGridViewPoints.DataMember = "PointsTable";
         dataGridViewPoints.DataSource = dataSet1;
         dataGridViewPoints.Dock = DockStyle.Fill;
         dataGridViewPoints.Location = new Point(0, 0);
         dataGridViewPoints.Margin = new Padding(4, 3, 4, 3);
         dataGridViewPoints.Name = "dataGridViewPoints";
         dataGridViewPoints.ReadOnly = true;
         dataGridViewPoints.RowHeadersVisible = false;
         dataGridViewPoints.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
         dataGridViewPoints.Size = new Size(707, 153);
         dataGridViewPoints.TabIndex = 0;
         dataGridViewPoints.SelectionChanged += dataGridViewPoints_SelectionChanged;
         dataGridViewPoints.KeyDown += dataGridViewPoints_KeyDown;
         // 
         // columnIdxDataGridViewTextBoxColumn
         // 
         columnIdxDataGridViewTextBoxColumn.DataPropertyName = "ColumnIdx";
         columnIdxDataGridViewTextBoxColumn.HeaderText = "Nummer";
         columnIdxDataGridViewTextBoxColumn.Name = "columnIdxDataGridViewTextBoxColumn";
         columnIdxDataGridViewTextBoxColumn.ReadOnly = true;
         columnIdxDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnIdxDataGridViewTextBoxColumn.Width = 61;
         // 
         // ColumnLat
         // 
         ColumnLat.DataPropertyName = "ColumnLat";
         ColumnLat.HeaderText = "geogr. Länge";
         ColumnLat.Name = "ColumnLat";
         ColumnLat.ReadOnly = true;
         ColumnLat.SortMode = DataGridViewColumnSortMode.NotSortable;
         ColumnLat.Width = 74;
         // 
         // columnLonDataGridViewTextBoxColumn
         // 
         columnLonDataGridViewTextBoxColumn.DataPropertyName = "ColumnLon";
         columnLonDataGridViewTextBoxColumn.HeaderText = "geogr. Breite";
         columnLonDataGridViewTextBoxColumn.Name = "columnLonDataGridViewTextBoxColumn";
         columnLonDataGridViewTextBoxColumn.ReadOnly = true;
         columnLonDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnLonDataGridViewTextBoxColumn.Width = 72;
         // 
         // columnElevationDataGridViewTextBoxColumn
         // 
         columnElevationDataGridViewTextBoxColumn.DataPropertyName = "ColumnElevation";
         dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
         columnElevationDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
         columnElevationDataGridViewTextBoxColumn.HeaderText = "Höhe (m)";
         columnElevationDataGridViewTextBoxColumn.Name = "columnElevationDataGridViewTextBoxColumn";
         columnElevationDataGridViewTextBoxColumn.ReadOnly = true;
         columnElevationDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnElevationDataGridViewTextBoxColumn.Width = 58;
         // 
         // columnTimeDataGridViewTextBoxColumn
         // 
         columnTimeDataGridViewTextBoxColumn.DataPropertyName = "ColumnTime";
         dataGridViewCellStyle3.Format = "G";
         dataGridViewCellStyle3.NullValue = null;
         columnTimeDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
         columnTimeDataGridViewTextBoxColumn.HeaderText = "Zeitpunkt (UTC)";
         columnTimeDataGridViewTextBoxColumn.Name = "columnTimeDataGridViewTextBoxColumn";
         columnTimeDataGridViewTextBoxColumn.ReadOnly = true;
         columnTimeDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnTimeDataGridViewTextBoxColumn.Width = 87;
         // 
         // columnDistanceDataGridViewTextBoxColumn
         // 
         columnDistanceDataGridViewTextBoxColumn.DataPropertyName = "ColumnDistance";
         dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleRight;
         columnDistanceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
         columnDistanceDataGridViewTextBoxColumn.HeaderText = "Entfernung (m)";
         columnDistanceDataGridViewTextBoxColumn.Name = "columnDistanceDataGridViewTextBoxColumn";
         columnDistanceDataGridViewTextBoxColumn.ReadOnly = true;
         columnDistanceDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnDistanceDataGridViewTextBoxColumn.Width = 85;
         // 
         // columnLengthDataGridViewTextBoxColumn
         // 
         columnLengthDataGridViewTextBoxColumn.DataPropertyName = "ColumnLength";
         dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleRight;
         columnLengthDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
         columnLengthDataGridViewTextBoxColumn.HeaderText = "Streckenlänge (m)";
         columnLengthDataGridViewTextBoxColumn.Name = "columnLengthDataGridViewTextBoxColumn";
         columnLengthDataGridViewTextBoxColumn.ReadOnly = true;
         columnLengthDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
         columnLengthDataGridViewTextBoxColumn.Width = 98;
         // 
         // contextMenuStripPoints
         // 
         contextMenuStripPoints.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_PointsRemoving });
         contextMenuStripPoints.Name = "contextMenuStripPoints";
         contextMenuStripPoints.Size = new Size(209, 26);
         contextMenuStripPoints.Opening += contextMenuStripPoints_Opening;
         // 
         // ToolStripMenuItem_PointsRemoving
         // 
         ToolStripMenuItem_PointsRemoving.Image = Properties.Resources.delete;
         ToolStripMenuItem_PointsRemoving.Name = "ToolStripMenuItem_PointsRemoving";
         ToolStripMenuItem_PointsRemoving.Size = new Size(208, 22);
         ToolStripMenuItem_PointsRemoving.Text = "markierte Punkte löschen";
         ToolStripMenuItem_PointsRemoving.Click += ToolStripMenuItem_PointsRemoving_Click;
         // 
         // dataSet1
         // 
         dataSet1.DataSetName = "NewDataSet";
         dataSet1.Tables.AddRange(new System.Data.DataTable[] { dataTablePoints });
         // 
         // dataTablePoints
         // 
         dataTablePoints.Columns.AddRange(new System.Data.DataColumn[] { dataColumnIdx, dataColumnLat, dataColumnLon, dataColumnElevation, dataColumnDistance, dataColumnLength, dataColumnTime });
         dataTablePoints.Constraints.AddRange(new System.Data.Constraint[] { new System.Data.UniqueConstraint("Constraint1", new string[] { "ColumnIdx" }, false) });
         dataTablePoints.Namespace = "";
         dataTablePoints.TableName = "PointsTable";
         // 
         // dataColumnIdx
         // 
         dataColumnIdx.Caption = "ColumnIdx";
         dataColumnIdx.ColumnName = "ColumnIdx";
         dataColumnIdx.DataType = typeof(int);
         dataColumnIdx.DefaultValue = resources.GetObject("dataColumnIdx.DefaultValue");
         dataColumnIdx.Namespace = "";
         dataColumnIdx.ReadOnly = true;
         // 
         // dataColumnLat
         // 
         dataColumnLat.Caption = "ColumnLat";
         dataColumnLat.ColumnName = "ColumnLat";
         dataColumnLat.DataType = typeof(double);
         dataColumnLat.DefaultValue = resources.GetObject("dataColumnLat.DefaultValue");
         dataColumnLat.Namespace = "";
         dataColumnLat.ReadOnly = true;
         // 
         // dataColumnLon
         // 
         dataColumnLon.Caption = "ColumnLon";
         dataColumnLon.ColumnName = "ColumnLon";
         dataColumnLon.DataType = typeof(double);
         dataColumnLon.DefaultValue = resources.GetObject("dataColumnLon.DefaultValue");
         dataColumnLon.Namespace = "";
         dataColumnLon.ReadOnly = true;
         // 
         // dataColumnElevation
         // 
         dataColumnElevation.Caption = "ColumnElevation";
         dataColumnElevation.ColumnName = "ColumnElevation";
         dataColumnElevation.DataType = typeof(double);
         dataColumnElevation.DefaultValue = resources.GetObject("dataColumnElevation.DefaultValue");
         dataColumnElevation.Namespace = "";
         dataColumnElevation.ReadOnly = true;
         // 
         // dataColumnDistance
         // 
         dataColumnDistance.Caption = "ColumnDistance";
         dataColumnDistance.ColumnName = "ColumnDistance";
         dataColumnDistance.DataType = typeof(double);
         dataColumnDistance.DefaultValue = resources.GetObject("dataColumnDistance.DefaultValue");
         dataColumnDistance.Namespace = "";
         dataColumnDistance.ReadOnly = true;
         // 
         // dataColumnLength
         // 
         dataColumnLength.Caption = "ColumnLength";
         dataColumnLength.ColumnName = "ColumnLength";
         dataColumnLength.DataType = typeof(double);
         dataColumnLength.DefaultValue = resources.GetObject("dataColumnLength.DefaultValue");
         dataColumnLength.Namespace = "";
         dataColumnLength.ReadOnly = true;
         // 
         // dataColumnTime
         // 
         dataColumnTime.Caption = "ColumnTime";
         dataColumnTime.ColumnName = "ColumnTime";
         dataColumnTime.DataType = typeof(DateTime);
         dataColumnTime.DefaultValue = resources.GetObject("dataColumnTime.DefaultValue");
         dataColumnTime.Namespace = "";
         dataColumnTime.ReadOnly = true;
         // 
         // richTextBoxInfo1
         // 
         richTextBoxInfo1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         richTextBoxInfo1.ContextMenuStrip = contextMenuStripText;
         richTextBoxInfo1.DetectUrls = false;
         richTextBoxInfo1.HideSelection = false;
         richTextBoxInfo1.Location = new Point(0, 0);
         richTextBoxInfo1.Margin = new Padding(4, 3, 4, 3);
         richTextBoxInfo1.Name = "richTextBoxInfo1";
         richTextBoxInfo1.ReadOnly = true;
         richTextBoxInfo1.ScrollBars = RichTextBoxScrollBars.Horizontal;
         richTextBoxInfo1.Size = new Size(707, 28);
         richTextBoxInfo1.TabIndex = 2;
         richTextBoxInfo1.Text = "";
         richTextBoxInfo1.WordWrap = false;
         // 
         // pictureBoxProfile1
         // 
         pictureBoxProfile1.ContextMenuStrip = contextMenuStripPicture;
         pictureBoxProfile1.Location = new Point(9, 46);
         pictureBoxProfile1.Margin = new Padding(4, 3, 4, 3);
         pictureBoxProfile1.Name = "pictureBoxProfile1";
         pictureBoxProfile1.Size = new Size(117, 58);
         pictureBoxProfile1.SizeMode = PictureBoxSizeMode.Zoom;
         pictureBoxProfile1.TabIndex = 0;
         pictureBoxProfile1.TabStop = false;
         // 
         // label_TrackLength
         // 
         label_TrackLength.AutoSize = true;
         label_TrackLength.Location = new Point(122, 143);
         label_TrackLength.Margin = new Padding(4, 0, 4, 0);
         label_TrackLength.Name = "label_TrackLength";
         label_TrackLength.Size = new Size(13, 15);
         label_TrackLength.TabIndex = 9;
         label_TrackLength.Text = "0";
         // 
         // label5
         // 
         label5.AutoSize = true;
         label5.Location = new Point(10, 143);
         label5.Margin = new Padding(4, 0, 4, 0);
         label5.Name = "label5";
         label5.Size = new Size(42, 15);
         label5.TabIndex = 8;
         label5.Text = "Länge:";
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(10, 107);
         label2.Margin = new Padding(4, 0, 4, 0);
         label2.Name = "label2";
         label2.Size = new Size(44, 15);
         label2.TabIndex = 6;
         label2.Text = "Quelle:";
         // 
         // textBoxSource
         // 
         textBoxSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBoxSource.Location = new Point(124, 104);
         textBoxSource.Margin = new Padding(4, 3, 4, 3);
         textBoxSource.Name = "textBoxSource";
         textBoxSource.Size = new Size(597, 23);
         textBoxSource.TabIndex = 7;
         // 
         // label7
         // 
         label7.AutoSize = true;
         label7.Location = new Point(10, 77);
         label7.Margin = new Padding(4, 0, 4, 0);
         label7.Name = "label7";
         label7.Size = new Size(73, 15);
         label7.TabIndex = 4;
         label7.Text = "Kommentar:";
         // 
         // label6
         // 
         label6.AutoSize = true;
         label6.Location = new Point(10, 47);
         label6.Margin = new Padding(4, 0, 4, 0);
         label6.Name = "label6";
         label6.Size = new Size(82, 15);
         label6.TabIndex = 2;
         label6.Text = "Beschreibung:";
         // 
         // textBoxComment
         // 
         textBoxComment.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBoxComment.Location = new Point(124, 74);
         textBoxComment.Margin = new Padding(4, 3, 4, 3);
         textBoxComment.Name = "textBoxComment";
         textBoxComment.Size = new Size(597, 23);
         textBoxComment.TabIndex = 5;
         // 
         // textBoxDescription
         // 
         textBoxDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBoxDescription.Location = new Point(124, 44);
         textBoxDescription.Margin = new Padding(4, 3, 4, 3);
         textBoxDescription.Name = "textBoxDescription";
         textBoxDescription.Size = new Size(597, 23);
         textBoxDescription.TabIndex = 3;
         // 
         // label4
         // 
         label4.AutoSize = true;
         label4.Location = new Point(10, 17);
         label4.Margin = new Padding(4, 0, 4, 0);
         label4.Name = "label4";
         label4.Size = new Size(42, 15);
         label4.TabIndex = 0;
         label4.Text = "Name:";
         // 
         // textBoxName
         // 
         textBoxName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBoxName.Location = new Point(124, 14);
         textBoxName.Margin = new Padding(4, 3, 4, 3);
         textBoxName.Name = "textBoxName";
         textBoxName.Size = new Size(597, 23);
         textBoxName.TabIndex = 1;
         // 
         // button_Save
         // 
         button_Save.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
         button_Save.DialogResult = DialogResult.OK;
         button_Save.Image = Properties.Resources.speichern;
         button_Save.Location = new Point(14, 665);
         button_Save.Margin = new Padding(4, 3, 4, 3);
         button_Save.Name = "button_Save";
         button_Save.Size = new Size(140, 37);
         button_Save.TabIndex = 10;
         button_Save.Text = "speichern";
         button_Save.TextImageRelation = TextImageRelation.TextBeforeImage;
         button_Save.UseVisualStyleBackColor = true;
         button_Save.Click += button_Save_Click;
         // 
         // FormTrackInfoAndEdit
         // 
         AcceptButton = button_Save;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         AutoScroll = true;
         ClientSize = new Size(735, 716);
         Controls.Add(button_Save);
         Controls.Add(splitContainer1);
         Controls.Add(label5);
         Controls.Add(label7);
         Controls.Add(label2);
         Controls.Add(textBoxSource);
         Controls.Add(label6);
         Controls.Add(textBoxName);
         Controls.Add(textBoxComment);
         Controls.Add(label4);
         Controls.Add(textBoxDescription);
         Controls.Add(label_TrackLength);
         FormBorderStyle = FormBorderStyle.SizableToolWindow;
         KeyPreview = true;
         Margin = new Padding(4, 3, 4, 3);
         MinimumSize = new Size(589, 512);
         Name = "FormTrackInfoAndEdit";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "FormExteRouteInfo";
         FormClosing += FormExtTrackInfoAndEdit_FormClosing;
         Load += FormExtTrackInfoAndEdit_Load;
         Shown += FormExtTrackInfoAndEdit_Shown;
         ClientSizeChanged += FormExtTrackInfoAndEdit_ClientSizeChanged;
         KeyDown += FormExtTrackInfoAndEdit_KeyDown;
         contextMenuStripText.ResumeLayout(false);
         contextMenuStripPicture.ResumeLayout(false);
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
         splitContainer1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)dataGridViewPoints).EndInit();
         contextMenuStripPoints.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)dataSet1).EndInit();
         ((System.ComponentModel.ISupportInitialize)dataTablePoints).EndInit();
         ((System.ComponentModel.ISupportInitialize)pictureBoxProfile1).EndInit();
         ResumeLayout(false);
         PerformLayout();
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
      private System.Windows.Forms.RichTextBox richTextBoxInfo1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CopyMarkedText;
   }
}