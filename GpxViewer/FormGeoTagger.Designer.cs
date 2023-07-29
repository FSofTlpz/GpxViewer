namespace GpxViewer {
   partial class FormGeoTagger {
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
         this.pictureManager1 = new GpxViewer.PictureManager();
         this.SuspendLayout();
         // 
         // pictureManager1
         // 
         this.pictureManager1.Cursor = System.Windows.Forms.Cursors.Default;
         this.pictureManager1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pictureManager1.Location = new System.Drawing.Point(0, 0);
         this.pictureManager1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.pictureManager1.Name = "pictureManager1";
         this.pictureManager1.Size = new System.Drawing.Size(633, 652);
         this.pictureManager1.TabIndex = 0;
         // 
         // FormGeoTagger
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(633, 652);
         this.Controls.Add(this.pictureManager1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.KeyPreview = true;
         this.Name = "FormGeoTagger";
         this.ShowInTaskbar = false;
         this.Text = "Geodaten für Bilder";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGeoTagger_FormClosing);
         this.Load += new System.EventHandler(this.FormGeoTagger_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGeoTagger_KeyDown);
         this.ResumeLayout(false);

      }

      #endregion

      private PictureManager pictureManager1;
   }
}