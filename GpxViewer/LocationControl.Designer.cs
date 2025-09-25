namespace GpxViewer {
   partial class LocationControl {
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
         toolStripContainer1 = new ToolStripContainer();
         listBox_Locations = new ListBox();
         toolStrip1 = new ToolStrip();
         toolStripButton_Save = new ToolStripButton();
         toolStripButton_Delete = new ToolStripButton();
         toolStripButton_Edit = new ToolStripButton();
         toolStripButton_Go = new ToolStripButton();
         toolStripContainer1.ContentPanel.SuspendLayout();
         toolStripContainer1.TopToolStripPanel.SuspendLayout();
         toolStripContainer1.SuspendLayout();
         toolStrip1.SuspendLayout();
         SuspendLayout();
         // 
         // toolStripContainer1
         // 
         // 
         // toolStripContainer1.ContentPanel
         // 
         toolStripContainer1.ContentPanel.Controls.Add(listBox_Locations);
         toolStripContainer1.ContentPanel.Size = new Size(323, 264);
         toolStripContainer1.Dock = DockStyle.Fill;
         toolStripContainer1.Location = new Point(0, 0);
         toolStripContainer1.Name = "toolStripContainer1";
         toolStripContainer1.Size = new Size(323, 289);
         toolStripContainer1.TabIndex = 0;
         toolStripContainer1.Text = "toolStripContainer1";
         // 
         // toolStripContainer1.TopToolStripPanel
         // 
         toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
         // 
         // listBox_Locations
         // 
         listBox_Locations.Dock = DockStyle.Fill;
         listBox_Locations.FormattingEnabled = true;
         listBox_Locations.IntegralHeight = false;
         listBox_Locations.Location = new Point(0, 0);
         listBox_Locations.Margin = new Padding(4, 3, 4, 3);
         listBox_Locations.Name = "listBox_Locations";
         listBox_Locations.Size = new Size(323, 264);
         listBox_Locations.TabIndex = 15;
         // 
         // toolStrip1
         // 
         toolStrip1.Dock = DockStyle.None;
         toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton_Save, toolStripButton_Delete, toolStripButton_Edit, toolStripButton_Go });
         toolStrip1.Location = new Point(3, 0);
         toolStrip1.Name = "toolStrip1";
         toolStrip1.Size = new Size(104, 25);
         toolStrip1.TabIndex = 14;
         // 
         // toolStripButton_Save
         // 
         toolStripButton_Save.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Save.Image = Properties.Resources.speichern;
         toolStripButton_Save.ImageTransparentColor = Color.Magenta;
         toolStripButton_Save.Name = "toolStripButton_Save";
         toolStripButton_Save.Size = new Size(23, 22);
         toolStripButton_Save.Text = "Position speichern";
         toolStripButton_Save.Click += toolStripButton_Save_Click;
         // 
         // toolStripButton_Delete
         // 
         toolStripButton_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Delete.Image = Properties.Resources.delete;
         toolStripButton_Delete.ImageTransparentColor = Color.Magenta;
         toolStripButton_Delete.Name = "toolStripButton_Delete";
         toolStripButton_Delete.Size = new Size(23, 22);
         toolStripButton_Delete.Text = "Position löschen";
         toolStripButton_Delete.Click += toolStripButton_Delete_Click;
         // 
         // toolStripButton_Edit
         // 
         toolStripButton_Edit.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Edit.Image = Properties.Resources.edit;
         toolStripButton_Edit.ImageTransparentColor = Color.Magenta;
         toolStripButton_Edit.Name = "toolStripButton_Edit";
         toolStripButton_Edit.Size = new Size(23, 22);
         toolStripButton_Edit.Text = "Name ändern";
         toolStripButton_Edit.Click += toolStripButton_Edit_Click;
         // 
         // toolStripButton_Go
         // 
         toolStripButton_Go.DisplayStyle = ToolStripItemDisplayStyle.Image;
         toolStripButton_Go.Image = Properties.Resources._goto;
         toolStripButton_Go.ImageTransparentColor = Color.Magenta;
         toolStripButton_Go.Name = "toolStripButton_Go";
         toolStripButton_Go.Size = new Size(23, 22);
         toolStripButton_Go.Text = "gehe zur Position";
         // 
         // LocationControl
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(toolStripContainer1);
         Name = "LocationControl";
         Size = new Size(323, 289);
         toolStripContainer1.ContentPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
         toolStripContainer1.TopToolStripPanel.PerformLayout();
         toolStripContainer1.ResumeLayout(false);
         toolStripContainer1.PerformLayout();
         toolStrip1.ResumeLayout(false);
         toolStrip1.PerformLayout();
         ResumeLayout(false);
      }

      #endregion

      private ToolStripContainer toolStripContainer1;
      private ToolStrip toolStrip1;
      private ToolStripButton toolStripButton_Save;
      private ToolStripButton toolStripButton_Delete;
      private ToolStripButton toolStripButton_Edit;
      private ToolStripButton toolStripButton_Go;
      private ListBox listBox_Locations;
   }
}
