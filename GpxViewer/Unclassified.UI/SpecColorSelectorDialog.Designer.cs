
namespace Unclassified.UI {
   partial class SpecColorSelectorDialog {
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
         this.button_Cancel = new System.Windows.Forms.Button();
         this.button_OK = new System.Windows.Forms.Button();
         this.specColorSelector1 = new Unclassified.UI.SpecColorSelector();
         this.SuspendLayout();
         // 
         // button_Cancel
         // 
         this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_Cancel.Location = new System.Drawing.Point(289, 63);
         this.button_Cancel.Name = "button_Cancel";
         this.button_Cancel.Size = new System.Drawing.Size(75, 23);
         this.button_Cancel.TabIndex = 1;
         this.button_Cancel.Text = "Abbruch";
         this.button_Cancel.UseVisualStyleBackColor = true;
         // 
         // button_OK
         // 
         this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_OK.Location = new System.Drawing.Point(289, 11);
         this.button_OK.Name = "button_OK";
         this.button_OK.Size = new System.Drawing.Size(75, 23);
         this.button_OK.TabIndex = 2;
         this.button_OK.Text = "OK";
         this.button_OK.UseVisualStyleBackColor = true;
         // 
         // specColorSelector1
         // 
         this.specColorSelector1.AutoSize = true;
         this.specColorSelector1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         this.specColorSelector1.Location = new System.Drawing.Point(0, 0);
         this.specColorSelector1.Name = "specColorSelector1";
         this.specColorSelector1.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
         this.specColorSelector1.Size = new System.Drawing.Size(275, 494);
         this.specColorSelector1.T_Blue = "Blau:";
         this.specColorSelector1.T_ExtendedView = "Erweiterte Ansicht";
         this.specColorSelector1.T_Green = "Grün:";
         this.specColorSelector1.T_Hue = "Farbton:";
         this.specColorSelector1.T_Lightness = "Helligkeit:";
         this.specColorSelector1.T_Red = "Rot:";
         this.specColorSelector1.T_Saturation = "Sättigung:";
         this.specColorSelector1.TabIndex = 0;
         // 
         // SpecColorSelectorDialog
         // 
         this.AcceptButton = this.button_OK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_Cancel;
         this.ClientSize = new System.Drawing.Size(383, 500);
         this.ControlBox = false;
         this.Controls.Add(this.button_OK);
         this.Controls.Add(this.button_Cancel);
         this.Controls.Add(this.specColorSelector1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "SpecColorSelectorDialog";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Farbauswahl";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private SpecColorSelector specColorSelector1;
      private System.Windows.Forms.Button button_Cancel;
      private System.Windows.Forms.Button button_OK;
   }
}