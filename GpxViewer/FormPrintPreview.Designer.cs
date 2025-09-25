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
         printPreviewControl1 = new PrintPreviewControl();
         button_goon = new Button();
         button_cancel = new Button();
         SuspendLayout();
         // 
         // printPreviewControl1
         // 
         printPreviewControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         printPreviewControl1.Location = new Point(0, 0);
         printPreviewControl1.Margin = new Padding(4, 3, 4, 3);
         printPreviewControl1.Name = "printPreviewControl1";
         printPreviewControl1.Size = new Size(833, 534);
         printPreviewControl1.TabIndex = 0;
         // 
         // button_goon
         // 
         button_goon.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
         button_goon.DialogResult = DialogResult.OK;
         button_goon.Location = new Point(14, 550);
         button_goon.Margin = new Padding(4, 3, 4, 3);
         button_goon.Name = "button_goon";
         button_goon.Size = new Size(94, 27);
         button_goon.TabIndex = 1;
         button_goon.Text = "weiter";
         button_goon.UseVisualStyleBackColor = true;
         button_goon.Click += button_goon_Click;
         // 
         // button_cancel
         // 
         button_cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
         button_cancel.DialogResult = DialogResult.Cancel;
         button_cancel.Location = new Point(170, 550);
         button_cancel.Margin = new Padding(4, 3, 4, 3);
         button_cancel.Name = "button_cancel";
         button_cancel.Size = new Size(94, 27);
         button_cancel.TabIndex = 2;
         button_cancel.Text = "Abbruch";
         button_cancel.UseVisualStyleBackColor = true;
         button_cancel.Click += button_cancel_Click;
         // 
         // FormPrintPreview
         // 
         AcceptButton = button_goon;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = button_cancel;
         ClientSize = new Size(833, 591);
         ControlBox = false;
         Controls.Add(button_cancel);
         Controls.Add(button_goon);
         Controls.Add(printPreviewControl1);
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormPrintPreview";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Druckvorschau";
         Load += FormPrintPreview_Load;
         ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
      private System.Windows.Forms.Button button_goon;
      private System.Windows.Forms.Button button_cancel;
   }
}