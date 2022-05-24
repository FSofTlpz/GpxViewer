namespace GpxViewer {
   partial class FormPrintPreview {
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
         this.printPreviewControl1 = new System.Windows.Forms.PrintPreviewControl();
         this.button_goon = new System.Windows.Forms.Button();
         this.button_cancel = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // printPreviewControl1
         // 
         this.printPreviewControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.printPreviewControl1.Location = new System.Drawing.Point(0, 0);
         this.printPreviewControl1.Name = "printPreviewControl1";
         this.printPreviewControl1.Size = new System.Drawing.Size(633, 463);
         this.printPreviewControl1.TabIndex = 0;
         // 
         // button_goon
         // 
         this.button_goon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button_goon.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_goon.Location = new System.Drawing.Point(12, 477);
         this.button_goon.Name = "button_goon";
         this.button_goon.Size = new System.Drawing.Size(81, 23);
         this.button_goon.TabIndex = 1;
         this.button_goon.Text = "weiter";
         this.button_goon.UseVisualStyleBackColor = true;
         this.button_goon.Click += new System.EventHandler(this.button_goon_Click);
         // 
         // button_cancel
         // 
         this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_cancel.Location = new System.Drawing.Point(146, 477);
         this.button_cancel.Name = "button_cancel";
         this.button_cancel.Size = new System.Drawing.Size(81, 23);
         this.button_cancel.TabIndex = 2;
         this.button_cancel.Text = "Abbruch";
         this.button_cancel.UseVisualStyleBackColor = true;
         this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
         // 
         // FormPrintPreview
         // 
         this.AcceptButton = this.button_goon;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_cancel;
         this.ClientSize = new System.Drawing.Size(633, 512);
         this.ControlBox = false;
         this.Controls.Add(this.button_cancel);
         this.Controls.Add(this.button_goon);
         this.Controls.Add(this.printPreviewControl1);
         this.Name = "FormPrintPreview";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Druckvorschau";
         this.Load += new System.EventHandler(this.FormPrintPreview_Load);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
      private System.Windows.Forms.Button button_goon;
      private System.Windows.Forms.Button button_cancel;
   }
}