
namespace GpxViewer {
   partial class ReadOnlyGpxControl {
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
         splitContainer1 = new SplitContainer();
         splitContainer2 = new SplitContainer();
         treeView1 = new TreeViewExt();
         textBox_Info = new TextBox();
         contextMenuStripInfo = new ContextMenuStrip(components);
         toolStripMenuItem_Copy = new ToolStripMenuItem();
         listBox_Found = new ListBox();
         textBox_SearchText = new TextBox();
         button_Search = new Button();
         toolTip1 = new ToolTip(components);
         ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
         splitContainer2.Panel1.SuspendLayout();
         splitContainer2.Panel2.SuspendLayout();
         splitContainer2.SuspendLayout();
         contextMenuStripInfo.SuspendLayout();
         SuspendLayout();
         // 
         // splitContainer1
         // 
         splitContainer1.Dock = DockStyle.Fill;
         splitContainer1.Location = new Point(0, 0);
         splitContainer1.Margin = new Padding(4);
         splitContainer1.Name = "splitContainer1";
         splitContainer1.Orientation = Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         splitContainer1.Panel1.Controls.Add(splitContainer2);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.Controls.Add(listBox_Found);
         splitContainer1.Panel2.Controls.Add(textBox_SearchText);
         splitContainer1.Panel2.Controls.Add(button_Search);
         splitContainer1.Size = new Size(378, 493);
         splitContainer1.SplitterDistance = 358;
         splitContainer1.SplitterWidth = 5;
         splitContainer1.TabIndex = 1;
         // 
         // splitContainer2
         // 
         splitContainer2.Dock = DockStyle.Fill;
         splitContainer2.Location = new Point(0, 0);
         splitContainer2.Name = "splitContainer2";
         splitContainer2.Orientation = Orientation.Horizontal;
         // 
         // splitContainer2.Panel1
         // 
         splitContainer2.Panel1.Controls.Add(treeView1);
         // 
         // splitContainer2.Panel2
         // 
         splitContainer2.Panel2.Controls.Add(textBox_Info);
         splitContainer2.Size = new Size(378, 358);
         splitContainer2.SplitterDistance = 249;
         splitContainer2.TabIndex = 1;
         // 
         // treeView1
         // 
         treeView1.BackColor = SystemColors.Control;
         treeView1.CheckBoxes = true;
         treeView1.Dock = DockStyle.Fill;
         treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
         treeView1.FullRowSelect = true;
         treeView1.HideSelection = false;
         treeView1.Location = new Point(0, 0);
         treeView1.Margin = new Padding(4);
         treeView1.Name = "treeView1";
         treeView1.Size = new Size(378, 249);
         treeView1.TabIndex = 0;
         treeView1.BeforeCheck += treeView1_BeforeCheck;
         treeView1.AfterCheck += treeView1_AfterCheck;
         treeView1.BeforeExpand += treeView1_BeforeExpand;
         treeView1.DrawNode += treeView1_DrawNode;
         treeView1.AfterSelect += treeView1_AfterSelect;
         treeView1.NodeMouseClick += treeView1_NodeMouseClick;
         treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick;
         // 
         // textBox_Info
         // 
         textBox_Info.BackColor = Color.FromArgb(192, 192, 255);
         textBox_Info.ContextMenuStrip = contextMenuStripInfo;
         textBox_Info.Dock = DockStyle.Fill;
         textBox_Info.Location = new Point(0, 0);
         textBox_Info.Multiline = true;
         textBox_Info.Name = "textBox_Info";
         textBox_Info.ReadOnly = true;
         textBox_Info.ScrollBars = ScrollBars.Both;
         textBox_Info.Size = new Size(378, 105);
         textBox_Info.TabIndex = 0;
         // 
         // contextMenuStripInfo
         // 
         contextMenuStripInfo.ImageScalingSize = new Size(20, 20);
         contextMenuStripInfo.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_Copy });
         contextMenuStripInfo.Name = "contextMenuStripInfo";
         contextMenuStripInfo.Size = new Size(269, 52);
         contextMenuStripInfo.Opening += contextMenuStripInfo_Opening;
         // 
         // toolStripMenuItem_Copy
         // 
         toolStripMenuItem_Copy.Image = Properties.Resources.copy;
         toolStripMenuItem_Copy.Name = "toolStripMenuItem_Copy";
         toolStripMenuItem_Copy.Size = new Size(268, 26);
         toolStripMenuItem_Copy.Text = "Text in die Zwischenablage kopieren";
         toolStripMenuItem_Copy.Click += toolStripMenuItem_Copy_Click;
         // 
         // listBox_Found
         // 
         listBox_Found.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         listBox_Found.FormattingEnabled = true;
         listBox_Found.IntegralHeight = false;
         listBox_Found.Location = new Point(0, 30);
         listBox_Found.Margin = new Padding(4);
         listBox_Found.Name = "listBox_Found";
         listBox_Found.Size = new Size(378, 96);
         listBox_Found.TabIndex = 2;
         toolTip1.SetToolTip(listBox_Found, "Liste der Fundstellen");
         listBox_Found.SelectedIndexChanged += listBox_Found_SelectedIndexChanged;
         // 
         // textBox_SearchText
         // 
         textBox_SearchText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         textBox_SearchText.Location = new Point(0, 0);
         textBox_SearchText.Margin = new Padding(4);
         textBox_SearchText.Name = "textBox_SearchText";
         textBox_SearchText.Size = new Size(285, 23);
         textBox_SearchText.TabIndex = 1;
         toolTip1.SetToolTip(textBox_SearchText, "Suchtext");
         textBox_SearchText.TextChanged += textBox_SearchText_TextChanged;
         // 
         // button_Search
         // 
         button_Search.Anchor = AnchorStyles.Top | AnchorStyles.Right;
         button_Search.Enabled = false;
         button_Search.Image = Properties.Resources.Search2;
         button_Search.ImageAlign = ContentAlignment.MiddleLeft;
         button_Search.Location = new Point(293, 0);
         button_Search.Margin = new Padding(4);
         button_Search.Name = "button_Search";
         button_Search.Size = new Size(85, 23);
         button_Search.TabIndex = 0;
         button_Search.Text = "suchen";
         button_Search.TextAlign = ContentAlignment.MiddleRight;
         toolTip1.SetToolTip(button_Search, "Suche starten");
         button_Search.UseVisualStyleBackColor = true;
         button_Search.Click += button_Search_Click;
         // 
         // ReadOnlyGpxControl
         // 
         AllowDrop = true;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(splitContainer1);
         Margin = new Padding(4);
         Name = "ReadOnlyGpxControl";
         Size = new Size(378, 493);
         DragDrop += ReadOnlyTracklistControl_DragDrop;
         DragEnter += ReadOnlyTracklistControl_DragEnter;
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel2.ResumeLayout(false);
         splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
         splitContainer1.ResumeLayout(false);
         splitContainer2.Panel1.ResumeLayout(false);
         splitContainer2.Panel2.ResumeLayout(false);
         splitContainer2.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
         splitContainer2.ResumeLayout(false);
         contextMenuStripInfo.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private TreeViewExt treeView1;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ListBox listBox_Found;
      private System.Windows.Forms.TextBox textBox_SearchText;
      private System.Windows.Forms.Button button_Search;
      private System.Windows.Forms.ToolTip toolTip1;
      private System.Windows.Forms.SplitContainer splitContainer2;
      private System.Windows.Forms.TextBox textBox_Info;
      private System.Windows.Forms.ContextMenuStrip contextMenuStripInfo;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
   }
}
