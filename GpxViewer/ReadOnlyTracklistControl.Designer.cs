
namespace GpxViewer {
   partial class ReadOnlyTracklistControl {
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
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.treeView1 = new GpxViewer.TreeViewExt();
         this.listBox_Found = new System.Windows.Forms.ListBox();
         this.textBox_SearchText = new System.Windows.Forms.TextBox();
         this.button_Search = new System.Windows.Forms.Button();
         this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.treeView1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.listBox_Found);
         this.splitContainer1.Panel2.Controls.Add(this.textBox_SearchText);
         this.splitContainer1.Panel2.Controls.Add(this.button_Search);
         this.splitContainer1.Size = new System.Drawing.Size(324, 427);
         this.splitContainer1.SplitterDistance = 311;
         this.splitContainer1.TabIndex = 1;
         // 
         // treeView1
         // 
         this.treeView1.BackColor = System.Drawing.SystemColors.Control;
         this.treeView1.CheckBoxes = true;
         this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
         this.treeView1.FullRowSelect = true;
         this.treeView1.HideSelection = false;
         this.treeView1.Location = new System.Drawing.Point(0, 0);
         this.treeView1.Name = "treeView1";
         this.treeView1.Size = new System.Drawing.Size(324, 311);
         this.treeView1.TabIndex = 0;
         this.treeView1.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeCheck);
         this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
         this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
         this.treeView1.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView1_DrawNode);
         this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
         this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
         this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
         // 
         // listBox_Found
         // 
         this.listBox_Found.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.listBox_Found.FormattingEnabled = true;
         this.listBox_Found.IntegralHeight = false;
         this.listBox_Found.Location = new System.Drawing.Point(0, 26);
         this.listBox_Found.Name = "listBox_Found";
         this.listBox_Found.Size = new System.Drawing.Size(324, 86);
         this.listBox_Found.TabIndex = 2;
         this.toolTip1.SetToolTip(this.listBox_Found, "Liste der Fundstellen");
         this.listBox_Found.SelectedIndexChanged += new System.EventHandler(this.listBox_Found_SelectedIndexChanged);
         // 
         // textBox_SearchText
         // 
         this.textBox_SearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_SearchText.Location = new System.Drawing.Point(0, 0);
         this.textBox_SearchText.Name = "textBox_SearchText";
         this.textBox_SearchText.Size = new System.Drawing.Size(245, 20);
         this.textBox_SearchText.TabIndex = 1;
         this.toolTip1.SetToolTip(this.textBox_SearchText, "Suchtext");
         this.textBox_SearchText.TextChanged += new System.EventHandler(this.textBox_SearchText_TextChanged);
         // 
         // button_Search
         // 
         this.button_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button_Search.Enabled = false;
         this.button_Search.Image = global::GpxViewer.Properties.Resources.Search2;
         this.button_Search.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Search.Location = new System.Drawing.Point(251, 0);
         this.button_Search.Name = "button_Search";
         this.button_Search.Size = new System.Drawing.Size(73, 20);
         this.button_Search.TabIndex = 0;
         this.button_Search.Text = "suchen";
         this.button_Search.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.toolTip1.SetToolTip(this.button_Search, "Suche starten");
         this.button_Search.UseVisualStyleBackColor = true;
         this.button_Search.Click += new System.EventHandler(this.button_Search_Click);
         // 
         // ReadOnlyTracklistControl
         // 
         this.AllowDrop = true;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.splitContainer1);
         this.Name = "ReadOnlyTracklistControl";
         this.Size = new System.Drawing.Size(324, 427);
         this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ReadOnlyTracklistControl_DragDrop);
         this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ReadOnlyTracklistControl_DragEnter);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         this.splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private TreeViewExt treeView1;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ListBox listBox_Found;
      private System.Windows.Forms.TextBox textBox_SearchText;
      private System.Windows.Forms.Button button_Search;
      private System.Windows.Forms.ToolTip toolTip1;
   }
}
