namespace GpxViewer {
   partial class FormMapLocationName {
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
         this.textBox_Name = new System.Windows.Forms.TextBox();
         this.button_OK = new System.Windows.Forms.Button();
         this.button_Cancel = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // textBox_Name
         // 
         this.textBox_Name.Location = new System.Drawing.Point(12, 12);
         this.textBox_Name.Name = "textBox_Name";
         this.textBox_Name.Size = new System.Drawing.Size(343, 20);
         this.textBox_Name.TabIndex = 0;
         // 
         // button_OK
         // 
         this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_OK.Image = global::GpxViewer.Properties.Resources.speichern;
         this.button_OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_OK.Location = new System.Drawing.Point(12, 38);
         this.button_OK.Name = "button_OK";
         this.button_OK.Size = new System.Drawing.Size(92, 31);
         this.button_OK.TabIndex = 1;
         this.button_OK.Text = "speichern";
         this.button_OK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.button_OK.UseVisualStyleBackColor = true;
         // 
         // button_Cancel
         // 
         this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_Cancel.Image = global::GpxViewer.Properties.Resources.cancel;
         this.button_Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Cancel.Location = new System.Drawing.Point(110, 38);
         this.button_Cancel.Name = "button_Cancel";
         this.button_Cancel.Size = new System.Drawing.Size(92, 31);
         this.button_Cancel.TabIndex = 2;
         this.button_Cancel.Text = "abbrechen";
         this.button_Cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.button_Cancel.UseVisualStyleBackColor = true;
         // 
         // FormMapLocationName
         // 
         this.AcceptButton = this.button_OK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_Cancel;
         this.ClientSize = new System.Drawing.Size(367, 80);
         this.ControlBox = false;
         this.Controls.Add(this.button_Cancel);
         this.Controls.Add(this.button_OK);
         this.Controls.Add(this.textBox_Name);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormMapLocationName";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Name für die Position";
         this.Load += new System.EventHandler(this.FormMapLocationName_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

        #endregion

        private System.Windows.Forms.TextBox textBox_Name;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}