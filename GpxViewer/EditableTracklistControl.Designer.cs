
namespace GpxViewer {
   partial class EditableTracklistControl {
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
         this.treeView_Tracks = new System.Windows.Forms.TreeView();
         this.treeView_Marker = new System.Windows.Forms.TreeView();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.SuspendLayout();
         // 
         // treeView_Tracks
         // 
         this.treeView_Tracks.AllowDrop = true;
         this.treeView_Tracks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
         this.treeView_Tracks.CheckBoxes = true;
         this.treeView_Tracks.Dock = System.Windows.Forms.DockStyle.Fill;
         this.treeView_Tracks.FullRowSelect = true;
         this.treeView_Tracks.HideSelection = false;
         this.treeView_Tracks.Indent = 5;
         this.treeView_Tracks.Location = new System.Drawing.Point(0, 0);
         this.treeView_Tracks.Name = "treeView_Tracks";
         this.treeView_Tracks.ShowLines = false;
         this.treeView_Tracks.ShowNodeToolTips = true;
         this.treeView_Tracks.ShowPlusMinus = false;
         this.treeView_Tracks.ShowRootLines = false;
         this.treeView_Tracks.Size = new System.Drawing.Size(351, 199);
         this.treeView_Tracks.TabIndex = 0;
         this.treeView_Tracks.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tv_Editable_AfterLabelEdit);
         this.treeView_Tracks.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tv_Editable_AfterCheck);
         this.treeView_Tracks.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tv_Editable_ItemDrag);
         this.treeView_Tracks.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_Tracks_AfterSelect);
         this.treeView_Tracks.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_Tracks_DragDrop);
         this.treeView_Tracks.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_Tracks_DragEnter);
         this.treeView_Tracks.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_Tracks_DragOver);
         this.treeView_Tracks.DoubleClick += new System.EventHandler(this.tv_Editable_DoubleClick);
         this.treeView_Tracks.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tv_Editable_MouseDown);
         this.treeView_Tracks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_Tracks_MouseUp);
         this.treeView_Tracks.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tv_Editable_PreviewKeyDown);
         // 
         // treeView_Marker
         // 
         this.treeView_Marker.AllowDrop = true;
         this.treeView_Marker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
         this.treeView_Marker.CheckBoxes = true;
         this.treeView_Marker.Dock = System.Windows.Forms.DockStyle.Fill;
         this.treeView_Marker.FullRowSelect = true;
         this.treeView_Marker.HideSelection = false;
         this.treeView_Marker.Indent = 5;
         this.treeView_Marker.Location = new System.Drawing.Point(0, 0);
         this.treeView_Marker.Name = "treeView_Marker";
         this.treeView_Marker.ShowLines = false;
         this.treeView_Marker.ShowPlusMinus = false;
         this.treeView_Marker.ShowRootLines = false;
         this.treeView_Marker.Size = new System.Drawing.Size(351, 195);
         this.treeView_Marker.TabIndex = 1;
         this.treeView_Marker.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tv_Editable_AfterLabelEdit);
         this.treeView_Marker.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tv_Editable_AfterCheck);
         this.treeView_Marker.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tv_Editable_ItemDrag);
         this.treeView_Marker.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_Marker_DragDrop);
         this.treeView_Marker.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_Marker_DragEnter);
         this.treeView_Marker.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_Marker_DragOver);
         this.treeView_Marker.DoubleClick += new System.EventHandler(this.tv_Editable_DoubleClick);
         this.treeView_Marker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tv_Editable_MouseDown);
         this.treeView_Marker.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tv_Editable_PreviewKeyDown);
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
         this.splitContainer1.Panel1.Controls.Add(this.treeView_Tracks);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.treeView_Marker);
         this.splitContainer1.Size = new System.Drawing.Size(351, 398);
         this.splitContainer1.SplitterDistance = 199;
         this.splitContainer1.TabIndex = 2;
         // 
         // EditableTracklistControl
         // 
         this.AllowDrop = true;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.splitContainer1);
         this.Name = "EditableTracklistControl";
         this.Size = new System.Drawing.Size(351, 398);
         this.DragDrop += new System.Windows.Forms.DragEventHandler(this.EditableTracklistControl_DragDrop);
         this.DragEnter += new System.Windows.Forms.DragEventHandler(this.EditableTracklistControl_DragEnter);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TreeView treeView_Tracks;
      private System.Windows.Forms.TreeView treeView_Marker;
      private System.Windows.Forms.SplitContainer splitContainer1;
   }
}
