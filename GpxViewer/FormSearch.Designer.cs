namespace GpxViewer {
   partial class FormSearch {
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
         textBox1 = new TextBox();
         label1 = new Label();
         listView_Result = new ListView();
         columnGeoName = new ColumnHeader();
         columnInfo1 = new ColumnHeader();
         contextMenuStrip1 = new ContextMenuStrip(components);
         ToolStripMenuItem_ShowPosition = new ToolStripMenuItem();
         ToolStripMenuItem_ShowArea = new ToolStripMenuItem();
         ToolStripMenuItem_ShowPositionAndMarker = new ToolStripMenuItem();
         ToolStripMenuItem_ShowAreaAndMarker = new ToolStripMenuItem();
         toolStripSeparator1 = new ToolStripSeparator();
         ToolStripMenuItem_Copy = new ToolStripMenuItem();
         ToolStripMenuItem_CopyAll = new ToolStripMenuItem();
         button_Start = new Button();
         contextMenuStrip1.SuspendLayout();
         SuspendLayout();
         // 
         // textBox1
         // 
         textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBox1.Location = new Point(87, 11);
         textBox1.Margin = new Padding(4);
         textBox1.Name = "textBox1";
         textBox1.Size = new Size(303, 23);
         textBox1.TabIndex = 1;
         textBox1.TextChanged += textBox1_TextChanged;
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(15, 15);
         label1.Margin = new Padding(4, 0, 4, 0);
         label1.Name = "label1";
         label1.Size = new Size(56, 15);
         label1.TabIndex = 0;
         label1.Text = "Suchtext:";
         // 
         // listView_Result
         // 
         listView_Result.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         listView_Result.Columns.AddRange(new ColumnHeader[] { columnGeoName, columnInfo1 });
         listView_Result.ContextMenuStrip = contextMenuStrip1;
         listView_Result.FullRowSelect = true;
         listView_Result.GridLines = true;
         listView_Result.HeaderStyle = ColumnHeaderStyle.Nonclickable;
         listView_Result.Location = new Point(18, 61);
         listView_Result.Margin = new Padding(4);
         listView_Result.MultiSelect = false;
         listView_Result.Name = "listView_Result";
         listView_Result.Size = new Size(466, 191);
         listView_Result.TabIndex = 3;
         listView_Result.UseCompatibleStateImageBehavior = false;
         listView_Result.View = View.Details;
         listView_Result.MouseDoubleClick += listView_Result_MouseDoubleClick;
         // 
         // columnGeoName
         // 
         columnGeoName.Text = "Name";
         // 
         // columnInfo1
         // 
         columnInfo1.Text = "Info";
         // 
         // contextMenuStrip1
         // 
         contextMenuStrip1.ImageScalingSize = new Size(20, 20);
         contextMenuStrip1.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_ShowPosition, ToolStripMenuItem_ShowArea, ToolStripMenuItem_ShowPositionAndMarker, ToolStripMenuItem_ShowAreaAndMarker, toolStripSeparator1, ToolStripMenuItem_Copy, ToolStripMenuItem_CopyAll });
         contextMenuStrip1.Name = "contextMenuStrip1";
         contextMenuStrip1.Size = new Size(296, 166);
         contextMenuStrip1.Opening += contextMenuStrip1_Opening;
         // 
         // ToolStripMenuItem_ShowPosition
         // 
         ToolStripMenuItem_ShowPosition.Name = "ToolStripMenuItem_ShowPosition";
         ToolStripMenuItem_ShowPosition.Size = new Size(295, 26);
         ToolStripMenuItem_ShowPosition.Text = "Position anzeigen";
         ToolStripMenuItem_ShowPosition.Click += ToolStripMenuItem_ShowPosition_Click;
         // 
         // ToolStripMenuItem_ShowArea
         // 
         ToolStripMenuItem_ShowArea.Name = "ToolStripMenuItem_ShowArea";
         ToolStripMenuItem_ShowArea.Size = new Size(295, 26);
         ToolStripMenuItem_ShowArea.Text = "Gebiet anzeigen";
         ToolStripMenuItem_ShowArea.Click += ToolStripMenuItem_ShowArea_Click;
         // 
         // ToolStripMenuItem_ShowPositionAndMarker
         // 
         ToolStripMenuItem_ShowPositionAndMarker.Name = "ToolStripMenuItem_ShowPositionAndMarker";
         ToolStripMenuItem_ShowPositionAndMarker.Size = new Size(295, 26);
         ToolStripMenuItem_ShowPositionAndMarker.Text = "Position anzeigen und Marker setzen";
         ToolStripMenuItem_ShowPositionAndMarker.Click += ToolStripMenuItem_ShowPositionAndMarker_Click;
         // 
         // ToolStripMenuItem_ShowAreaAndMarker
         // 
         ToolStripMenuItem_ShowAreaAndMarker.Name = "ToolStripMenuItem_ShowAreaAndMarker";
         ToolStripMenuItem_ShowAreaAndMarker.Size = new Size(295, 26);
         ToolStripMenuItem_ShowAreaAndMarker.Text = "Gebiet anzeigen und Marker setzen";
         ToolStripMenuItem_ShowAreaAndMarker.Click += ToolStripMenuItem_ShowAreaAndMarker_Click;
         // 
         // toolStripSeparator1
         // 
         toolStripSeparator1.Name = "toolStripSeparator1";
         toolStripSeparator1.Size = new Size(292, 6);
         // 
         // ToolStripMenuItem_Copy
         // 
         ToolStripMenuItem_Copy.Image = Properties.Resources.copy;
         ToolStripMenuItem_Copy.Name = "ToolStripMenuItem_Copy";
         ToolStripMenuItem_Copy.Size = new Size(295, 26);
         ToolStripMenuItem_Copy.Text = "Text in die Zwischenablage kopieren";
         ToolStripMenuItem_Copy.Click += ToolStripMenuItem_Copy_Click;
         // 
         // ToolStripMenuItem_CopyAll
         // 
         ToolStripMenuItem_CopyAll.Name = "ToolStripMenuItem_CopyAll";
         ToolStripMenuItem_CopyAll.Size = new Size(295, 26);
         ToolStripMenuItem_CopyAll.Text = "alle Texte in die Zwischenablage kopieren";
         ToolStripMenuItem_CopyAll.Click += ToolStripMenuItem_CopyAll_Click;
         // 
         // button_Start
         // 
         button_Start.Anchor = AnchorStyles.Top | AnchorStyles.Right;
         button_Start.Enabled = false;
         button_Start.Location = new Point(397, 9);
         button_Start.Margin = new Padding(4);
         button_Start.Name = "button_Start";
         button_Start.Size = new Size(88, 26);
         button_Start.TabIndex = 2;
         button_Start.Text = "suchen";
         button_Start.UseVisualStyleBackColor = true;
         button_Start.Click += button_Start_Click;
         // 
         // FormSearch
         // 
         AcceptButton = button_Start;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(498, 266);
         Controls.Add(button_Start);
         Controls.Add(listView_Result);
         Controls.Add(label1);
         Controls.Add(textBox1);
         FormBorderStyle = FormBorderStyle.SizableToolWindow;
         KeyPreview = true;
         Margin = new Padding(4);
         Name = "FormSearch";
         ShowInTaskbar = false;
         Text = "Suche nach geografischen Objekten";
         Load += FormGarminInfo_Load;
         KeyDown += FormGarminInfo_KeyDown;
         contextMenuStrip1.ResumeLayout(false);
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ListView listView_Result;
      private System.Windows.Forms.ColumnHeader columnGeoName;
      private System.Windows.Forms.ColumnHeader columnInfo1;
      private System.Windows.Forms.Button button_Start;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowPosition;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowArea;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowPositionAndMarker;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ShowAreaAndMarker;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Copy;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CopyAll;
   }
}