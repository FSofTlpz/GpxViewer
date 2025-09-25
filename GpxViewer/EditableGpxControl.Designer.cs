
namespace GpxViewer {
   partial class EditableGpxControl {
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditableGpxControl));
         treeView_GeoObjects = new TreeView();
         imageListTreeView = new ImageList(components);
         splitContainer1 = new SplitContainer();
         textBox_Info = new TextBox();
         contextMenuStrip_TextBox = new ContextMenuStrip(components);
         toolStripMenuItem_CopyText = new ToolStripMenuItem();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         contextMenuStrip_TextBox.SuspendLayout();
         SuspendLayout();
         // 
         // treeView_GeoObjects
         // 
         treeView_GeoObjects.AllowDrop = true;
         treeView_GeoObjects.BackColor = Color.FromArgb(192, 255, 192);
         treeView_GeoObjects.Dock = DockStyle.Fill;
         treeView_GeoObjects.FullRowSelect = true;
         treeView_GeoObjects.HideSelection = false;
         treeView_GeoObjects.ImageIndex = 0;
         treeView_GeoObjects.ImageList = imageListTreeView;
         treeView_GeoObjects.Location = new Point(0, 0);
         treeView_GeoObjects.Margin = new Padding(4);
         treeView_GeoObjects.Name = "treeView_GeoObjects";
         treeView_GeoObjects.SelectedImageIndex = 0;
         treeView_GeoObjects.Size = new Size(410, 334);
         treeView_GeoObjects.TabIndex = 0;
         treeView_GeoObjects.AfterLabelEdit += tv_AfterLabelEdit;
         treeView_GeoObjects.ItemDrag += tv_ItemDrag;
         treeView_GeoObjects.AfterSelect += tv_AfterSelect;
         treeView_GeoObjects.DragDrop += tv_DragDrop;
         treeView_GeoObjects.DragEnter += tv_DragEnter;
         treeView_GeoObjects.DragOver += tv_DragOver;
         treeView_GeoObjects.DoubleClick += tv_DoubleClick;
         treeView_GeoObjects.KeyDown += tv_KeyDown;
         treeView_GeoObjects.MouseDown += tv_MouseDown;
         // 
         // imageListTreeView
         // 
         imageListTreeView.ColorDepth = ColorDepth.Depth8Bit;
         imageListTreeView.ImageStream = (ImageListStreamer)resources.GetObject("imageListTreeView.ImageStream");
         imageListTreeView.TransparentColor = Color.Transparent;
         imageListTreeView.Images.SetKeyName(0, "Open.png");
         imageListTreeView.Images.SetKeyName(1, "Track.png");
         imageListTreeView.Images.SetKeyName(2, "Marker16x16.png");
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
         splitContainer1.Panel1.Controls.Add(treeView_GeoObjects);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.Controls.Add(textBox_Info);
         splitContainer1.Size = new Size(410, 459);
         splitContainer1.SplitterDistance = 334;
         splitContainer1.SplitterWidth = 5;
         splitContainer1.TabIndex = 2;
         // 
         // textBox_Info
         // 
         textBox_Info.BackColor = Color.FromArgb(192, 192, 255);
         textBox_Info.ContextMenuStrip = contextMenuStrip_TextBox;
         textBox_Info.Dock = DockStyle.Fill;
         textBox_Info.Location = new Point(0, 0);
         textBox_Info.Multiline = true;
         textBox_Info.Name = "textBox_Info";
         textBox_Info.ReadOnly = true;
         textBox_Info.ScrollBars = ScrollBars.Both;
         textBox_Info.Size = new Size(410, 120);
         textBox_Info.TabIndex = 0;
         textBox_Info.WordWrap = false;
         // 
         // contextMenuStrip_TextBox
         // 
         contextMenuStrip_TextBox.ImageScalingSize = new Size(20, 20);
         contextMenuStrip_TextBox.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_CopyText });
         contextMenuStrip_TextBox.Name = "contextMenuStrip_TextBox";
         contextMenuStrip_TextBox.Size = new Size(269, 52);
         contextMenuStrip_TextBox.Opening += contextMenuStrip_TextBox_Opening;
         // 
         // toolStripMenuItem_CopyText
         // 
         toolStripMenuItem_CopyText.Image = Properties.Resources.copy;
         toolStripMenuItem_CopyText.Name = "toolStripMenuItem_CopyText";
         toolStripMenuItem_CopyText.Size = new Size(268, 26);
         toolStripMenuItem_CopyText.Text = "Text in die Zwischenablage kopieren";
         toolStripMenuItem_CopyText.Click += toolStripMenuItem_CopyText_Click;
         // 
         // EditableGpxControl
         // 
         AllowDrop = true;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(splitContainer1);
         Margin = new Padding(4);
         Name = "EditableGpxControl";
         Size = new Size(410, 459);
         DragDrop += EditableTracklistControl_DragDrop;
         DragEnter += EditableTracklistControl_DragEnter;
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel2.ResumeLayout(false);
         splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
         splitContainer1.ResumeLayout(false);
         contextMenuStrip_TextBox.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TreeView treeView_GeoObjects;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ImageList imageListTreeView;
      private System.Windows.Forms.TextBox textBox_Info;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip_TextBox;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_CopyText;
   }
}
