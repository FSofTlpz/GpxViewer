namespace GpxViewer {
   partial class FormPictureMarkers {
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
         this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
         this.SuspendLayout();
         // 
         // checkedListBox1
         // 
         this.checkedListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.checkedListBox1.FormattingEnabled = true;
         this.checkedListBox1.Location = new System.Drawing.Point(0, 0);
         this.checkedListBox1.Name = "checkedListBox1";
         this.checkedListBox1.Size = new System.Drawing.Size(230, 156);
         this.checkedListBox1.TabIndex = 0;
         this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
         // 
         // FormPictureMarkers
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(230, 156);
         this.Controls.Add(this.checkedListBox1);
         this.KeyPreview = true;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormPictureMarkers";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Bilder";
         this.Load += new System.EventHandler(this.FormPictureMarkers_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormPictureMarkers_KeyDown);
         this.ResumeLayout(false);

      }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
    }
}