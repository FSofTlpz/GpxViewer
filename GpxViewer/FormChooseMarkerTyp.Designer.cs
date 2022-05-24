
namespace GpxViewer {
   partial class FormChooseMarkerTyp {
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
         this.listView1 = new System.Windows.Forms.ListView();
         this.contextMenuStrip_LVType = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.toolStripMenuItem_LargeIcon = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem_Tile = new System.Windows.Forms.ToolStripMenuItem();
         this.button_Cancel = new System.Windows.Forms.Button();
         this.button_OK = new System.Windows.Forms.Button();
         this.contextMenuStrip_LVType.SuspendLayout();
         this.SuspendLayout();
         // 
         // listView1
         // 
         this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.listView1.ContextMenuStrip = this.contextMenuStrip_LVType;
         this.listView1.HideSelection = false;
         this.listView1.Location = new System.Drawing.Point(0, 0);
         this.listView1.Name = "listView1";
         this.listView1.Size = new System.Drawing.Size(570, 374);
         this.listView1.TabIndex = 0;
         this.listView1.UseCompatibleStateImageBehavior = false;
         this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
         // 
         // contextMenuStrip_LVType
         // 
         this.contextMenuStrip_LVType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_LargeIcon,
            this.toolStripMenuItem_Tile});
         this.contextMenuStrip_LVType.Name = "contextMenuStrip1";
         this.contextMenuStrip_LVType.Size = new System.Drawing.Size(181, 70);
         this.contextMenuStrip_LVType.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_LVType_Opening);
         // 
         // toolStripMenuItem_LargeIcon
         // 
         this.toolStripMenuItem_LargeIcon.Name = "toolStripMenuItem_LargeIcon";
         this.toolStripMenuItem_LargeIcon.Size = new System.Drawing.Size(180, 22);
         this.toolStripMenuItem_LargeIcon.Text = "Ansicht 1";
         this.toolStripMenuItem_LargeIcon.Click += new System.EventHandler(this.toolStripMenuItem_LargeIcon_Click);
         // 
         // toolStripMenuItem_Tile
         // 
         this.toolStripMenuItem_Tile.Name = "toolStripMenuItem_Tile";
         this.toolStripMenuItem_Tile.Size = new System.Drawing.Size(180, 22);
         this.toolStripMenuItem_Tile.Text = "Ansicht 2";
         this.toolStripMenuItem_Tile.Click += new System.EventHandler(this.toolStripMenuItem_Tile_Click);
         // 
         // button_Cancel
         // 
         this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_Cancel.Image = global::GpxViewer.Properties.Resources.cancel;
         this.button_Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Cancel.Location = new System.Drawing.Point(325, 389);
         this.button_Cancel.Name = "button_Cancel";
         this.button_Cancel.Size = new System.Drawing.Size(99, 28);
         this.button_Cancel.TabIndex = 1;
         this.button_Cancel.Text = "Abbruch";
         this.button_Cancel.UseVisualStyleBackColor = true;
         this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
         // 
         // button_OK
         // 
         this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_OK.Image = global::GpxViewer.Properties.Resources.ok;
         this.button_OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_OK.Location = new System.Drawing.Point(459, 389);
         this.button_OK.Name = "button_OK";
         this.button_OK.Size = new System.Drawing.Size(99, 28);
         this.button_OK.TabIndex = 2;
         this.button_OK.Text = "Auswahl";
         this.button_OK.UseVisualStyleBackColor = true;
         this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
         // 
         // FormChooseMarkerTyp
         // 
         this.AcceptButton = this.button_OK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_Cancel;
         this.ClientSize = new System.Drawing.Size(570, 429);
         this.Controls.Add(this.button_OK);
         this.Controls.Add(this.button_Cancel);
         this.Controls.Add(this.listView1);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormChooseMarkerTyp";
         this.ShowInTaskbar = false;
         this.Text = "Markerauswahl";
         this.Load += new System.EventHandler(this.FormChooseMarkerTyp_Load);
         this.contextMenuStrip_LVType.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ListView listView1;
      private System.Windows.Forms.Button button_Cancel;
      private System.Windows.Forms.Button button_OK;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip_LVType;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_LargeIcon;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Tile;
   }
}