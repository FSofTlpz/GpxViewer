namespace GpxViewer {
   partial class GeoLocationControl {
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
         label3 = new Label();
         label6 = new Label();
         label5 = new Label();
         maskedTextBox_Longitude = new MaskedTextBox();
         button_goto = new Button();
         maskedTextBox_Latitude = new MaskedTextBox();
         maskedTextBox_LongitudeDec = new MaskedTextBox();
         label4 = new Label();
         label1 = new Label();
         label2 = new Label();
         maskedTextBox_LatitudeDec = new MaskedTextBox();
         button_get = new Button();
         SuspendLayout();
         // 
         // label3
         // 
         label3.AutoSize = true;
         label3.Location = new Point(84, 11);
         label3.Margin = new Padding(4, 0, 4, 0);
         label3.Name = "label3";
         label3.Size = new Size(141, 15);
         label3.TabIndex = 0;
         label3.Text = "Grad, Minuten, Sekunden";
         // 
         // label6
         // 
         label6.AutoSize = true;
         label6.Location = new Point(253, 91);
         label6.Margin = new Padding(4, 0, 4, 0);
         label6.Name = "label6";
         label6.Size = new Size(15, 15);
         label6.TabIndex = 8;
         label6.Text = "=";
         // 
         // label5
         // 
         label5.AutoSize = true;
         label5.Location = new Point(253, 50);
         label5.Margin = new Padding(4, 0, 4, 0);
         label5.Name = "label5";
         label5.Size = new Size(15, 15);
         label5.TabIndex = 6;
         label5.Text = "=";
         // 
         // maskedTextBox_Longitude
         // 
         maskedTextBox_Longitude.AsciiOnly = true;
         maskedTextBox_Longitude.InsertKeyMode = InsertKeyMode.Overwrite;
         maskedTextBox_Longitude.Location = new Point(88, 87);
         maskedTextBox_Longitude.Margin = new Padding(4);
         maskedTextBox_Longitude.Mask = "990°90'90.000\" A";
         maskedTextBox_Longitude.Name = "maskedTextBox_Longitude";
         maskedTextBox_Longitude.Size = new Size(130, 23);
         maskedTextBox_Longitude.TabIndex = 5;
         // 
         // button_goto
         // 
         button_goto.Image = Properties.Resources._goto;
         button_goto.ImageAlign = ContentAlignment.MiddleLeft;
         button_goto.Location = new Point(289, 133);
         button_goto.Margin = new Padding(4);
         button_goto.Name = "button_goto";
         button_goto.Size = new Size(122, 26);
         button_goto.TabIndex = 11;
         button_goto.Text = "&gehe zu ...";
         button_goto.UseVisualStyleBackColor = true;
         button_goto.Click += button_goto_Click;
         // 
         // maskedTextBox_Latitude
         // 
         maskedTextBox_Latitude.AsciiOnly = true;
         maskedTextBox_Latitude.InsertKeyMode = InsertKeyMode.Overwrite;
         maskedTextBox_Latitude.Location = new Point(88, 46);
         maskedTextBox_Latitude.Margin = new Padding(4);
         maskedTextBox_Latitude.Mask = "990°90'90.000\" A";
         maskedTextBox_Latitude.Name = "maskedTextBox_Latitude";
         maskedTextBox_Latitude.Size = new Size(130, 23);
         maskedTextBox_Latitude.TabIndex = 3;
         // 
         // maskedTextBox_LongitudeDec
         // 
         maskedTextBox_LongitudeDec.AsciiOnly = true;
         maskedTextBox_LongitudeDec.InsertKeyMode = InsertKeyMode.Overwrite;
         maskedTextBox_LongitudeDec.Location = new Point(302, 87);
         maskedTextBox_LongitudeDec.Margin = new Padding(4);
         maskedTextBox_LongitudeDec.Mask = "990.000000° A";
         maskedTextBox_LongitudeDec.Name = "maskedTextBox_LongitudeDec";
         maskedTextBox_LongitudeDec.Size = new Size(109, 23);
         maskedTextBox_LongitudeDec.TabIndex = 9;
         // 
         // label4
         // 
         label4.AutoSize = true;
         label4.Location = new Point(298, 11);
         label4.Margin = new Padding(4, 0, 4, 0);
         label4.Name = "label4";
         label4.Size = new Size(73, 15);
         label4.TabIndex = 1;
         label4.Text = "Dezimalgrad";
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(10, 49);
         label1.Margin = new Padding(4, 0, 4, 0);
         label1.Name = "label1";
         label1.Size = new Size(37, 15);
         label1.TabIndex = 4;
         label1.Text = "&Breite";
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(10, 90);
         label2.Margin = new Padding(4, 0, 4, 0);
         label2.Name = "label2";
         label2.Size = new Size(39, 15);
         label2.TabIndex = 2;
         label2.Text = "&Länge";
         // 
         // maskedTextBox_LatitudeDec
         // 
         maskedTextBox_LatitudeDec.AsciiOnly = true;
         maskedTextBox_LatitudeDec.InsertKeyMode = InsertKeyMode.Overwrite;
         maskedTextBox_LatitudeDec.Location = new Point(302, 46);
         maskedTextBox_LatitudeDec.Margin = new Padding(4);
         maskedTextBox_LatitudeDec.Mask = "990.000000° A";
         maskedTextBox_LatitudeDec.Name = "maskedTextBox_LatitudeDec";
         maskedTextBox_LatitudeDec.Size = new Size(109, 23);
         maskedTextBox_LatitudeDec.TabIndex = 7;
         // 
         // button_get
         // 
         button_get.Location = new Point(88, 133);
         button_get.Name = "button_get";
         button_get.Size = new Size(130, 26);
         button_get.TabIndex = 10;
         button_get.Text = "hole akt. Position";
         button_get.UseVisualStyleBackColor = true;
         button_get.Click += button_get_Click;
         // 
         // GeoLocationControl
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(button_get);
         Controls.Add(label3);
         Controls.Add(label6);
         Controls.Add(label5);
         Controls.Add(maskedTextBox_Longitude);
         Controls.Add(button_goto);
         Controls.Add(maskedTextBox_Latitude);
         Controls.Add(maskedTextBox_LongitudeDec);
         Controls.Add(label4);
         Controls.Add(label1);
         Controls.Add(label2);
         Controls.Add(maskedTextBox_LatitudeDec);
         Name = "GeoLocationControl";
         Size = new Size(424, 166);
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private Label label3;
      private Label label6;
      private Label label5;
      private MaskedTextBox maskedTextBox_Longitude;
      private Button button_goto;
      private MaskedTextBox maskedTextBox_Latitude;
      private MaskedTextBox maskedTextBox_LongitudeDec;
      private Label label4;
      private Label label1;
      private Label label2;
      private MaskedTextBox maskedTextBox_LatitudeDec;
      private Button button_get;
   }
}
