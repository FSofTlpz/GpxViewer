namespace GpxViewer {
   partial class FormGoToPos {
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
         this.button_OK = new System.Windows.Forms.Button();
         this.button_Cancel = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.maskedTextBox_Latitude = new System.Windows.Forms.MaskedTextBox();
         this.maskedTextBox_Longitude = new System.Windows.Forms.MaskedTextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.maskedTextBox_LatitudeDec = new System.Windows.Forms.MaskedTextBox();
         this.maskedTextBox_LongitudeDec = new System.Windows.Forms.MaskedTextBox();
         this.label5 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // button_OK
         // 
         this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_OK.Image = global::GpxViewer.Properties.Resources.ok;
         this.button_OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_OK.Location = new System.Drawing.Point(127, 154);
         this.button_OK.Margin = new System.Windows.Forms.Padding(4);
         this.button_OK.Name = "button_OK";
         this.button_OK.Size = new System.Drawing.Size(132, 39);
         this.button_OK.TabIndex = 8;
         this.button_OK.Text = "&OK";
         this.button_OK.UseVisualStyleBackColor = true;
         // 
         // button_Cancel
         // 
         this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_Cancel.Image = global::GpxViewer.Properties.Resources.cancel;
         this.button_Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Cancel.Location = new System.Drawing.Point(284, 154);
         this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
         this.button_Cancel.Name = "button_Cancel";
         this.button_Cancel.Size = new System.Drawing.Size(132, 39);
         this.button_Cancel.TabIndex = 9;
         this.button_Cancel.Text = "Abbruch";
         this.button_Cancel.UseVisualStyleBackColor = true;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(16, 96);
         this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(42, 16);
         this.label1.TabIndex = 5;
         this.label1.Text = "Breite";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(16, 52);
         this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(45, 16);
         this.label2.TabIndex = 2;
         this.label2.Text = "Länge";
         // 
         // maskedTextBox_Latitude
         // 
         this.maskedTextBox_Latitude.AsciiOnly = true;
         this.maskedTextBox_Latitude.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
         this.maskedTextBox_Latitude.Location = new System.Drawing.Point(107, 48);
         this.maskedTextBox_Latitude.Margin = new System.Windows.Forms.Padding(4);
         this.maskedTextBox_Latitude.Mask = "990°90\'90.000\" A";
         this.maskedTextBox_Latitude.Name = "maskedTextBox_Latitude";
         this.maskedTextBox_Latitude.Size = new System.Drawing.Size(148, 22);
         this.maskedTextBox_Latitude.TabIndex = 3;
         // 
         // maskedTextBox_Longitude
         // 
         this.maskedTextBox_Longitude.AsciiOnly = true;
         this.maskedTextBox_Longitude.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
         this.maskedTextBox_Longitude.Location = new System.Drawing.Point(107, 92);
         this.maskedTextBox_Longitude.Margin = new System.Windows.Forms.Padding(4);
         this.maskedTextBox_Longitude.Mask = "990°90\'90.000\" A";
         this.maskedTextBox_Longitude.Name = "maskedTextBox_Longitude";
         this.maskedTextBox_Longitude.Size = new System.Drawing.Size(148, 22);
         this.maskedTextBox_Longitude.TabIndex = 6;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(103, 11);
         this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(156, 16);
         this.label3.TabIndex = 0;
         this.label3.Text = "Grad, Minuten, Sekunden";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(348, 11);
         this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(84, 16);
         this.label4.TabIndex = 1;
         this.label4.Text = "Dezimalgrad";
         // 
         // maskedTextBox_LatitudeDec
         // 
         this.maskedTextBox_LatitudeDec.AsciiOnly = true;
         this.maskedTextBox_LatitudeDec.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
         this.maskedTextBox_LatitudeDec.Location = new System.Drawing.Point(352, 48);
         this.maskedTextBox_LatitudeDec.Margin = new System.Windows.Forms.Padding(4);
         this.maskedTextBox_LatitudeDec.Mask = "990.000000° A";
         this.maskedTextBox_LatitudeDec.Name = "maskedTextBox_LatitudeDec";
         this.maskedTextBox_LatitudeDec.Size = new System.Drawing.Size(124, 22);
         this.maskedTextBox_LatitudeDec.TabIndex = 4;
         // 
         // maskedTextBox_LongitudeDec
         // 
         this.maskedTextBox_LongitudeDec.AsciiOnly = true;
         this.maskedTextBox_LongitudeDec.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
         this.maskedTextBox_LongitudeDec.Location = new System.Drawing.Point(352, 92);
         this.maskedTextBox_LongitudeDec.Margin = new System.Windows.Forms.Padding(4);
         this.maskedTextBox_LongitudeDec.Mask = "990.000000° A";
         this.maskedTextBox_LongitudeDec.Name = "maskedTextBox_LongitudeDec";
         this.maskedTextBox_LongitudeDec.Size = new System.Drawing.Size(124, 22);
         this.maskedTextBox_LongitudeDec.TabIndex = 7;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(296, 52);
         this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(14, 16);
         this.label5.TabIndex = 10;
         this.label5.Text = "=";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(296, 96);
         this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(14, 16);
         this.label6.TabIndex = 11;
         this.label6.Text = "=";
         // 
         // FormGoToPos
         // 
         this.AcceptButton = this.button_OK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_Cancel;
         this.ClientSize = new System.Drawing.Size(515, 219);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.maskedTextBox_LongitudeDec);
         this.Controls.Add(this.maskedTextBox_LatitudeDec);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.maskedTextBox_Longitude);
         this.Controls.Add(this.maskedTextBox_Latitude);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.button_Cancel);
         this.Controls.Add(this.button_OK);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Margin = new System.Windows.Forms.Padding(4);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormGoToPos";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Zielkoordinaten angeben";
         this.Load += new System.EventHandler(this.FormGoToPos_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button button_OK;
      private System.Windows.Forms.Button button_Cancel;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.MaskedTextBox maskedTextBox_Latitude;
      private System.Windows.Forms.MaskedTextBox maskedTextBox_Longitude;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.MaskedTextBox maskedTextBox_LatitudeDec;
      private System.Windows.Forms.MaskedTextBox maskedTextBox_LongitudeDec;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.Label label6;
   }
}