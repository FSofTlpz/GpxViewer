
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
         components = new System.ComponentModel.Container();
         listView1 = new ListView();
         contextMenuStrip_LVType = new ContextMenuStrip(components);
         toolStripMenuItem_LargeIcon = new ToolStripMenuItem();
         toolStripMenuItem_Tile = new ToolStripMenuItem();
         button_Cancel = new Button();
         button_OK = new Button();
         contextMenuStrip_LVType.SuspendLayout();
         SuspendLayout();
         // 
         // listView1
         // 
         listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         listView1.ContextMenuStrip = contextMenuStrip_LVType;
         listView1.Location = new Point(0, 0);
         listView1.Margin = new Padding(4, 3, 4, 3);
         listView1.Name = "listView1";
         listView1.Size = new Size(664, 441);
         listView1.TabIndex = 0;
         listView1.UseCompatibleStateImageBehavior = false;
         listView1.DoubleClick += listView1_DoubleClick;
         // 
         // contextMenuStrip_LVType
         // 
         contextMenuStrip_LVType.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_LargeIcon, toolStripMenuItem_Tile });
         contextMenuStrip_LVType.Name = "contextMenuStrip1";
         contextMenuStrip_LVType.Size = new Size(124, 48);
         contextMenuStrip_LVType.Opening += contextMenuStrip_LVType_Opening;
         // 
         // toolStripMenuItem_LargeIcon
         // 
         toolStripMenuItem_LargeIcon.Name = "toolStripMenuItem_LargeIcon";
         toolStripMenuItem_LargeIcon.Size = new Size(123, 22);
         toolStripMenuItem_LargeIcon.Text = "Ansicht 1";
         toolStripMenuItem_LargeIcon.Click += toolStripMenuItem_LargeIcon_Click;
         // 
         // toolStripMenuItem_Tile
         // 
         toolStripMenuItem_Tile.Name = "toolStripMenuItem_Tile";
         toolStripMenuItem_Tile.Size = new Size(123, 22);
         toolStripMenuItem_Tile.Text = "Ansicht 2";
         toolStripMenuItem_Tile.Click += toolStripMenuItem_Tile_Click;
         // 
         // button_Cancel
         // 
         button_Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         button_Cancel.DialogResult = DialogResult.Cancel;
         button_Cancel.Image = Properties.Resources.cancel;
         button_Cancel.ImageAlign = ContentAlignment.MiddleLeft;
         button_Cancel.Location = new Point(379, 459);
         button_Cancel.Margin = new Padding(4, 3, 4, 3);
         button_Cancel.Name = "button_Cancel";
         button_Cancel.Size = new Size(115, 32);
         button_Cancel.TabIndex = 1;
         button_Cancel.Text = "Abbruch";
         button_Cancel.UseVisualStyleBackColor = true;
         button_Cancel.Click += button_Cancel_Click;
         // 
         // button_OK
         // 
         button_OK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         button_OK.DialogResult = DialogResult.OK;
         button_OK.Image = Properties.Resources.ok;
         button_OK.ImageAlign = ContentAlignment.MiddleLeft;
         button_OK.Location = new Point(536, 459);
         button_OK.Margin = new Padding(4, 3, 4, 3);
         button_OK.Name = "button_OK";
         button_OK.Size = new Size(115, 32);
         button_OK.TabIndex = 2;
         button_OK.Text = "Auswahl";
         button_OK.UseVisualStyleBackColor = true;
         button_OK.Click += button_OK_Click;
         // 
         // FormChooseMarkerTyp
         // 
         AcceptButton = button_OK;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = button_Cancel;
         ClientSize = new Size(665, 505);
         Controls.Add(button_OK);
         Controls.Add(button_Cancel);
         Controls.Add(listView1);
         Margin = new Padding(4, 3, 4, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormChooseMarkerTyp";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Markerauswahl";
         Load += FormChooseMarkerTyp_Load;
         contextMenuStrip_LVType.ResumeLayout(false);
         ResumeLayout(false);
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