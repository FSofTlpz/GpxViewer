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
         checkedListBox1 = new CheckedListBox();
         SuspendLayout();
         // 
         // checkedListBox1
         // 
         checkedListBox1.Dock = DockStyle.Fill;
         checkedListBox1.FormattingEnabled = true;
         checkedListBox1.Location = new Point(0, 0);
         checkedListBox1.Margin = new Padding(4, 3, 4, 3);
         checkedListBox1.Name = "checkedListBox1";
         checkedListBox1.Size = new Size(268, 161);
         checkedListBox1.TabIndex = 0;
         checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
         // 
         // FormPictureMarkers
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(268, 161);
         Controls.Add(checkedListBox1);
         KeyPreview = true;
         Margin = new Padding(4, 3, 4, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormPictureMarkers";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Bilder";
         Load += FormPictureMarkers_Load;
         KeyDown += FormPictureMarkers_KeyDown;
         ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.CheckedListBox checkedListBox1;
    }
}