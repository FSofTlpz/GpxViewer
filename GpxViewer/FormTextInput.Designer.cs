namespace GpxViewer {
   partial class FormTextInput {
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
         textBox_Name = new TextBox();
         button_OK = new Button();
         button_Cancel = new Button();
         SuspendLayout();
         // 
         // textBox_Name
         // 
         textBox_Name.Location = new Point(14, 14);
         textBox_Name.Margin = new Padding(4, 3, 4, 3);
         textBox_Name.Name = "textBox_Name";
         textBox_Name.Size = new Size(400, 23);
         textBox_Name.TabIndex = 0;
         // 
         // button_OK
         // 
         button_OK.DialogResult = DialogResult.OK;
         button_OK.Image = Properties.Resources.speichern;
         button_OK.ImageAlign = ContentAlignment.MiddleLeft;
         button_OK.Location = new Point(14, 44);
         button_OK.Margin = new Padding(4, 3, 4, 3);
         button_OK.Name = "button_OK";
         button_OK.Size = new Size(107, 36);
         button_OK.TabIndex = 1;
         button_OK.Text = "speichern";
         button_OK.TextAlign = ContentAlignment.MiddleRight;
         button_OK.UseVisualStyleBackColor = true;
         // 
         // button_Cancel
         // 
         button_Cancel.DialogResult = DialogResult.Cancel;
         button_Cancel.Image = Properties.Resources.cancel;
         button_Cancel.ImageAlign = ContentAlignment.MiddleLeft;
         button_Cancel.Location = new Point(128, 44);
         button_Cancel.Margin = new Padding(4, 3, 4, 3);
         button_Cancel.Name = "button_Cancel";
         button_Cancel.Size = new Size(107, 36);
         button_Cancel.TabIndex = 2;
         button_Cancel.Text = "abbrechen";
         button_Cancel.TextAlign = ContentAlignment.MiddleRight;
         button_Cancel.UseVisualStyleBackColor = true;
         // 
         // FormTextInput
         // 
         AcceptButton = button_OK;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = button_Cancel;
         ClientSize = new Size(424, 92);
         ControlBox = false;
         Controls.Add(button_Cancel);
         Controls.Add(button_OK);
         Controls.Add(textBox_Name);
         FormBorderStyle = FormBorderStyle.FixedDialog;
         Margin = new Padding(4, 3, 4, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormTextInput";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Name für die Position";
         Load += FormTextInput_Load;
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private System.Windows.Forms.TextBox textBox_Name;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}