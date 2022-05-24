namespace GpxViewer {
   partial class FormMapLocation {
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
         this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
         this.listBox_Locations = new System.Windows.Forms.ListBox();
         this.toolStrip1 = new System.Windows.Forms.ToolStrip();
         this.toolStripButton_Save = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_Delete = new System.Windows.Forms.ToolStripButton();
         this.toolStripButton_Go = new System.Windows.Forms.ToolStripButton();
         this.toolStripContainer1.ContentPanel.SuspendLayout();
         this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
         this.toolStripContainer1.SuspendLayout();
         this.toolStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // toolStripContainer1
         // 
         // 
         // toolStripContainer1.ContentPanel
         // 
         this.toolStripContainer1.ContentPanel.Controls.Add(this.listBox_Locations);
         this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(316, 219);
         this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
         this.toolStripContainer1.Name = "toolStripContainer1";
         this.toolStripContainer1.Size = new System.Drawing.Size(316, 244);
         this.toolStripContainer1.TabIndex = 0;
         this.toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
         // 
         // listBox_Locations
         // 
         this.listBox_Locations.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listBox_Locations.FormattingEnabled = true;
         this.listBox_Locations.IntegralHeight = false;
         this.listBox_Locations.Location = new System.Drawing.Point(0, 0);
         this.listBox_Locations.Name = "listBox_Locations";
         this.listBox_Locations.Size = new System.Drawing.Size(316, 219);
         this.listBox_Locations.TabIndex = 0;
         this.listBox_Locations.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_Locations_MouseDoubleClick);
         // 
         // toolStrip1
         // 
         this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
         this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Save,
            this.toolStripButton_Delete,
            this.toolStripButton_Go});
         this.toolStrip1.Location = new System.Drawing.Point(3, 0);
         this.toolStrip1.Name = "toolStrip1";
         this.toolStrip1.Size = new System.Drawing.Size(81, 25);
         this.toolStrip1.TabIndex = 0;
         // 
         // toolStripButton_Save
         // 
         this.toolStripButton_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_Save.Image = global::GpxViewer.Properties.Resources.speichern;
         this.toolStripButton_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_Save.Name = "toolStripButton_Save";
         this.toolStripButton_Save.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_Save.Text = "Position speichern";
         this.toolStripButton_Save.Click += new System.EventHandler(this.toolStripButton_Save_Click);
         // 
         // toolStripButton_Delete
         // 
         this.toolStripButton_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_Delete.Image = global::GpxViewer.Properties.Resources.delete;
         this.toolStripButton_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_Delete.Name = "toolStripButton_Delete";
         this.toolStripButton_Delete.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_Delete.Text = "Position löschen";
         this.toolStripButton_Delete.Click += new System.EventHandler(this.toolStripButton_Delete_Click);
         // 
         // toolStripButton_Go
         // 
         this.toolStripButton_Go.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton_Go.Image = global::GpxViewer.Properties.Resources._goto;
         this.toolStripButton_Go.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton_Go.Name = "toolStripButton_Go";
         this.toolStripButton_Go.Size = new System.Drawing.Size(23, 22);
         this.toolStripButton_Go.Text = "gehe zur Position";
         this.toolStripButton_Go.Click += new System.EventHandler(this.toolStripButton_Go_Click);
         // 
         // FormMapLocation
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(316, 244);
         this.Controls.Add(this.toolStripContainer1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.Name = "FormMapLocation";
         this.ShowInTaskbar = false;
         this.Text = "gespeicherte Orte";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMapLocation_FormClosing);
         this.Load += new System.EventHandler(this.FormMapLocation_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMapLocation_KeyDown);
         this.toolStripContainer1.ContentPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         this.toolStripContainer1.TopToolStripPanel.PerformLayout();
         this.toolStripContainer1.ResumeLayout(false);
         this.toolStripContainer1.PerformLayout();
         this.toolStrip1.ResumeLayout(false);
         this.toolStrip1.PerformLayout();
         this.ResumeLayout(false);

      }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Save;
        private System.Windows.Forms.ToolStripButton toolStripButton_Delete;
        private System.Windows.Forms.ListBox listBox_Locations;
        private System.Windows.Forms.ToolStripButton toolStripButton_Go;
    }
}