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
         this.components = new System.ComponentModel.Container();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.listView_Result = new System.Windows.Forms.ListView();
         this.columnGeoName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.columnInfo1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.ToolStripMenuItem_ShowPosition = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_ShowArea = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_ShowPositionAndMarker = new System.Windows.Forms.ToolStripMenuItem();
         this.ToolStripMenuItem_ShowAreaAndMarker = new System.Windows.Forms.ToolStripMenuItem();
         this.button_Start = new System.Windows.Forms.Button();
         this.contextMenuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.Location = new System.Drawing.Point(74, 10);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(245, 20);
         this.textBox1.TabIndex = 1;
         this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(13, 13);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(52, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "Suchtext:";
         // 
         // listView_Result
         // 
         this.listView_Result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.listView_Result.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnGeoName,
            this.columnInfo1});
         this.listView_Result.ContextMenuStrip = this.contextMenuStrip1;
         this.listView_Result.FullRowSelect = true;
         this.listView_Result.GridLines = true;
         this.listView_Result.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.listView_Result.HideSelection = false;
         this.listView_Result.Location = new System.Drawing.Point(16, 53);
         this.listView_Result.MultiSelect = false;
         this.listView_Result.Name = "listView_Result";
         this.listView_Result.Size = new System.Drawing.Size(384, 166);
         this.listView_Result.TabIndex = 3;
         this.listView_Result.UseCompatibleStateImageBehavior = false;
         this.listView_Result.View = System.Windows.Forms.View.Details;
         this.listView_Result.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_Result_MouseDoubleClick);
         // 
         // columnGeoName
         // 
         this.columnGeoName.Text = "Name";
         // 
         // columnInfo1
         // 
         this.columnInfo1.Text = "Info";
         // 
         // contextMenuStrip1
         // 
         this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_ShowPosition,
            this.ToolStripMenuItem_ShowArea,
            this.ToolStripMenuItem_ShowPositionAndMarker,
            this.ToolStripMenuItem_ShowAreaAndMarker});
         this.contextMenuStrip1.Name = "contextMenuStrip1";
         this.contextMenuStrip1.Size = new System.Drawing.Size(268, 92);
         this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
         // 
         // ToolStripMenuItem_ShowPosition
         // 
         this.ToolStripMenuItem_ShowPosition.Name = "ToolStripMenuItem_ShowPosition";
         this.ToolStripMenuItem_ShowPosition.Size = new System.Drawing.Size(267, 22);
         this.ToolStripMenuItem_ShowPosition.Text = "Position anzeigen";
         this.ToolStripMenuItem_ShowPosition.Click += new System.EventHandler(this.ToolStripMenuItem_ShowPosition_Click);
         // 
         // ToolStripMenuItem_ShowArea
         // 
         this.ToolStripMenuItem_ShowArea.Name = "ToolStripMenuItem_ShowArea";
         this.ToolStripMenuItem_ShowArea.Size = new System.Drawing.Size(267, 22);
         this.ToolStripMenuItem_ShowArea.Text = "Gebiet anzeigen";
         this.ToolStripMenuItem_ShowArea.Click += new System.EventHandler(this.ToolStripMenuItem_ShowArea_Click);
         // 
         // ToolStripMenuItem_ShowPositionAndMarker
         // 
         this.ToolStripMenuItem_ShowPositionAndMarker.Name = "ToolStripMenuItem_ShowPositionAndMarker";
         this.ToolStripMenuItem_ShowPositionAndMarker.Size = new System.Drawing.Size(267, 22);
         this.ToolStripMenuItem_ShowPositionAndMarker.Text = "Position anzeigen und Marker setzen";
         this.ToolStripMenuItem_ShowPositionAndMarker.Click += new System.EventHandler(this.ToolStripMenuItem_ShowPositionAndMarker_Click);
         // 
         // ToolStripMenuItem_ShowAreaAndMarker
         // 
         this.ToolStripMenuItem_ShowAreaAndMarker.Name = "ToolStripMenuItem_ShowAreaAndMarker";
         this.ToolStripMenuItem_ShowAreaAndMarker.Size = new System.Drawing.Size(267, 22);
         this.ToolStripMenuItem_ShowAreaAndMarker.Text = "Gebiet anzeigen und Marker setzen";
         this.ToolStripMenuItem_ShowAreaAndMarker.Click += new System.EventHandler(this.ToolStripMenuItem_ShowAreaAndMarker_Click);
         // 
         // button_Start
         // 
         this.button_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button_Start.Enabled = false;
         this.button_Start.Location = new System.Drawing.Point(325, 8);
         this.button_Start.Name = "button_Start";
         this.button_Start.Size = new System.Drawing.Size(75, 23);
         this.button_Start.TabIndex = 2;
         this.button_Start.Text = "suchen";
         this.button_Start.UseVisualStyleBackColor = true;
         this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
         // 
         // FormSearch
         // 
         this.AcceptButton = this.button_Start;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(412, 231);
         this.Controls.Add(this.button_Start);
         this.Controls.Add(this.listView_Result);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.textBox1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.KeyPreview = true;
         this.Name = "FormSearch";
         this.ShowInTaskbar = false;
         this.Text = "Suche nach geografischen Objekten";
         this.Load += new System.EventHandler(this.FormGarminInfo_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGarminInfo_KeyDown);
         this.contextMenuStrip1.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

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
   }
}