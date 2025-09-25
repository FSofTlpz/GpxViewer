namespace GpxViewer {
   partial class SearchControl {
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
         buttonSearchStart = new Button();
         listView_Result = new ListView();
         columnGeoName = new ColumnHeader();
         columnInfo1 = new ColumnHeader();
         label7 = new Label();
         textBoxSearch = new TextBox();
         contextMenuStripSearch = new ContextMenuStrip(components);
         ToolStripMenuItem_ShowPosition = new ToolStripMenuItem();
         ToolStripMenuItem_ShowArea = new ToolStripMenuItem();
         ToolStripMenuItem_ShowPositionAndMarker = new ToolStripMenuItem();
         ToolStripMenuItem_ShowAreaAndMarker = new ToolStripMenuItem();
         toolStripSeparator1 = new ToolStripSeparator();
         ToolStripMenuItem_Copy = new ToolStripMenuItem();
         ToolStripMenuItem_CopyAll = new ToolStripMenuItem();
         contextMenuStripSearch.SuspendLayout();
         SuspendLayout();
         // 
         // buttonSearchStart
         // 
         buttonSearchStart.Enabled = false;
         buttonSearchStart.Location = new Point(287, 5);
         buttonSearchStart.Margin = new Padding(4);
         buttonSearchStart.Name = "buttonSearchStart";
         buttonSearchStart.Size = new Size(88, 26);
         buttonSearchStart.TabIndex = 6;
         buttonSearchStart.Text = "&suchen";
         buttonSearchStart.UseVisualStyleBackColor = true;
         // 
         // listView_Result
         // 
         listView_Result.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         listView_Result.Columns.AddRange(new ColumnHeader[] { columnGeoName, columnInfo1 });
         listView_Result.ContextMenuStrip = contextMenuStripSearch;
         listView_Result.FullRowSelect = true;
         listView_Result.GridLines = true;
         listView_Result.HeaderStyle = ColumnHeaderStyle.Nonclickable;
         listView_Result.Location = new Point(14, 39);
         listView_Result.Margin = new Padding(4);
         listView_Result.MultiSelect = false;
         listView_Result.Name = "listView_Result";
         listView_Result.Size = new Size(409, 312);
         listView_Result.TabIndex = 7;
         listView_Result.UseCompatibleStateImageBehavior = false;
         listView_Result.View = View.Details;
         // 
         // columnGeoName
         // 
         columnGeoName.Text = "Name";
         // 
         // columnInfo1
         // 
         columnInfo1.Text = "Info";
         // 
         // label7
         // 
         label7.AutoSize = true;
         label7.Location = new Point(11, 11);
         label7.Margin = new Padding(4, 0, 4, 0);
         label7.Name = "label7";
         label7.Size = new Size(56, 15);
         label7.TabIndex = 4;
         label7.Text = "Such&text:";
         // 
         // textBoxSearch
         // 
         textBoxSearch.Location = new Point(83, 7);
         textBoxSearch.Margin = new Padding(4);
         textBoxSearch.Name = "textBoxSearch";
         textBoxSearch.Size = new Size(182, 23);
         textBoxSearch.TabIndex = 5;
         // 
         // contextMenuStripSearch
         // 
         contextMenuStripSearch.ImageScalingSize = new Size(20, 20);
         contextMenuStripSearch.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_ShowPosition, ToolStripMenuItem_ShowArea, ToolStripMenuItem_ShowPositionAndMarker, ToolStripMenuItem_ShowAreaAndMarker, toolStripSeparator1, ToolStripMenuItem_Copy, ToolStripMenuItem_CopyAll });
         contextMenuStripSearch.Name = "contextMenuStrip1";
         contextMenuStripSearch.Size = new Size(296, 166);
         // 
         // ToolStripMenuItem_ShowPosition
         // 
         ToolStripMenuItem_ShowPosition.Name = "ToolStripMenuItem_ShowPosition";
         ToolStripMenuItem_ShowPosition.Size = new Size(295, 26);
         ToolStripMenuItem_ShowPosition.Text = "Position anzeigen";
         // 
         // ToolStripMenuItem_ShowArea
         // 
         ToolStripMenuItem_ShowArea.Name = "ToolStripMenuItem_ShowArea";
         ToolStripMenuItem_ShowArea.Size = new Size(295, 26);
         ToolStripMenuItem_ShowArea.Text = "Gebiet anzeigen";
         // 
         // ToolStripMenuItem_ShowPositionAndMarker
         // 
         ToolStripMenuItem_ShowPositionAndMarker.Name = "ToolStripMenuItem_ShowPositionAndMarker";
         ToolStripMenuItem_ShowPositionAndMarker.Size = new Size(295, 26);
         ToolStripMenuItem_ShowPositionAndMarker.Text = "Position anzeigen und Marker setzen";
         // 
         // ToolStripMenuItem_ShowAreaAndMarker
         // 
         ToolStripMenuItem_ShowAreaAndMarker.Name = "ToolStripMenuItem_ShowAreaAndMarker";
         ToolStripMenuItem_ShowAreaAndMarker.Size = new Size(295, 26);
         ToolStripMenuItem_ShowAreaAndMarker.Text = "Gebiet anzeigen und Marker setzen";
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
         // 
         // ToolStripMenuItem_CopyAll
         // 
         ToolStripMenuItem_CopyAll.Name = "ToolStripMenuItem_CopyAll";
         ToolStripMenuItem_CopyAll.Size = new Size(295, 26);
         ToolStripMenuItem_CopyAll.Text = "alle Texte in die Zwischenablage kopieren";
         // 
         // SearchControl
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(buttonSearchStart);
         Controls.Add(listView_Result);
         Controls.Add(label7);
         Controls.Add(textBoxSearch);
         Name = "SearchControl";
         Size = new Size(436, 369);
         contextMenuStripSearch.ResumeLayout(false);
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private Button buttonSearchStart;
      private ListView listView_Result;
      private ColumnHeader columnGeoName;
      private ColumnHeader columnInfo1;
      private Label label7;
      private TextBox textBoxSearch;
      private ContextMenuStrip contextMenuStripSearch;
      private ToolStripMenuItem ToolStripMenuItem_ShowPosition;
      private ToolStripMenuItem ToolStripMenuItem_ShowArea;
      private ToolStripMenuItem ToolStripMenuItem_ShowPositionAndMarker;
      private ToolStripMenuItem ToolStripMenuItem_ShowAreaAndMarker;
      private ToolStripSeparator toolStripSeparator1;
      private ToolStripMenuItem ToolStripMenuItem_Copy;
      private ToolStripMenuItem ToolStripMenuItem_CopyAll;
   }
}
