namespace GpxViewer {
   partial class FormMarkerEditing {
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
         label1 = new Label();
         dateTimePickerDT = new DateTimePicker();
         numericUpDownHeight = new NumericUpDown();
         label2 = new Label();
         button_Cancel = new Button();
         button_Save = new Button();
         checkBox_Height = new CheckBox();
         label3 = new Label();
         numericUpDownLat = new NumericUpDown();
         numericUpDownLon = new NumericUpDown();
         label4 = new Label();
         label5 = new Label();
         textBoxDescription = new TextBox();
         textBoxComment = new TextBox();
         label6 = new Label();
         label7 = new Label();
         comboBox_Name = new ComboBox();
         button_Marker = new Button();
         ((System.ComponentModel.ISupportInitialize)numericUpDownHeight).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLat).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLon).BeginInit();
         SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(14, 10);
         label1.Margin = new Padding(4, 0, 4, 0);
         label1.Name = "label1";
         label1.Size = new Size(42, 15);
         label1.TabIndex = 0;
         label1.Text = "Name:";
         // 
         // dateTimePickerDT
         // 
         dateTimePickerDT.CustomFormat = "MMMM, d.M.yyyy, H:mm:ss";
         dateTimePickerDT.Format = DateTimePickerFormat.Custom;
         dateTimePickerDT.Location = new Point(127, 97);
         dateTimePickerDT.Margin = new Padding(4, 3, 4, 3);
         dateTimePickerDT.Name = "dateTimePickerDT";
         dateTimePickerDT.ShowCheckBox = true;
         dateTimePickerDT.Size = new Size(300, 23);
         dateTimePickerDT.TabIndex = 7;
         // 
         // numericUpDownHeight
         // 
         numericUpDownHeight.Location = new Point(127, 127);
         numericUpDownHeight.Margin = new Padding(4, 3, 4, 3);
         numericUpDownHeight.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
         numericUpDownHeight.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
         numericUpDownHeight.Name = "numericUpDownHeight";
         numericUpDownHeight.Size = new Size(106, 23);
         numericUpDownHeight.TabIndex = 9;
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(254, 129);
         label2.Margin = new Padding(4, 0, 4, 0);
         label2.Name = "label2";
         label2.Size = new Size(83, 15);
         label2.TabIndex = 10;
         label2.Text = "Höhe in Meter";
         // 
         // button_Cancel
         // 
         button_Cancel.DialogResult = DialogResult.Cancel;
         button_Cancel.Image = Properties.Resources.cancel;
         button_Cancel.ImageAlign = ContentAlignment.MiddleLeft;
         button_Cancel.Location = new Point(278, 216);
         button_Cancel.Margin = new Padding(4, 3, 4, 3);
         button_Cancel.Name = "button_Cancel";
         button_Cancel.Size = new Size(118, 36);
         button_Cancel.TabIndex = 15;
         button_Cancel.Text = "zurück";
         button_Cancel.UseVisualStyleBackColor = true;
         button_Cancel.Click += button_Cancel_Click;
         // 
         // button_Save
         // 
         button_Save.DialogResult = DialogResult.OK;
         button_Save.Image = Properties.Resources.ok;
         button_Save.ImageAlign = ContentAlignment.MiddleLeft;
         button_Save.Location = new Point(415, 216);
         button_Save.Margin = new Padding(4, 3, 4, 3);
         button_Save.Name = "button_Save";
         button_Save.Size = new Size(118, 36);
         button_Save.TabIndex = 16;
         button_Save.Text = "speichern";
         button_Save.UseVisualStyleBackColor = true;
         button_Save.Click += button_Save_Click;
         // 
         // checkBox_Height
         // 
         checkBox_Height.AutoSize = true;
         checkBox_Height.Location = new Point(18, 134);
         checkBox_Height.Margin = new Padding(4, 3, 4, 3);
         checkBox_Height.Name = "checkBox_Height";
         checkBox_Height.Size = new Size(15, 14);
         checkBox_Height.TabIndex = 8;
         checkBox_Height.UseVisualStyleBackColor = true;
         checkBox_Height.CheckedChanged += checkBox_Height_CheckedChanged;
         // 
         // label3
         // 
         label3.AutoSize = true;
         label3.Location = new Point(14, 104);
         label3.Margin = new Padding(4, 0, 4, 0);
         label3.Name = "label3";
         label3.Size = new Size(28, 15);
         label3.TabIndex = 6;
         label3.Text = "UTC";
         // 
         // numericUpDownLat
         // 
         numericUpDownLat.DecimalPlaces = 8;
         numericUpDownLat.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
         numericUpDownLat.Location = new Point(258, 157);
         numericUpDownLat.Margin = new Padding(4, 3, 4, 3);
         numericUpDownLat.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
         numericUpDownLat.Minimum = new decimal(new int[] { 90, 0, 0, int.MinValue });
         numericUpDownLat.Name = "numericUpDownLat";
         numericUpDownLat.Size = new Size(127, 23);
         numericUpDownLat.TabIndex = 13;
         // 
         // numericUpDownLon
         // 
         numericUpDownLon.DecimalPlaces = 8;
         numericUpDownLon.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
         numericUpDownLon.Location = new Point(18, 157);
         numericUpDownLon.Margin = new Padding(4, 3, 4, 3);
         numericUpDownLon.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
         numericUpDownLon.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
         numericUpDownLon.Name = "numericUpDownLon";
         numericUpDownLon.Size = new Size(127, 23);
         numericUpDownLon.TabIndex = 11;
         // 
         // label4
         // 
         label4.AutoSize = true;
         label4.Location = new Point(152, 159);
         label4.Margin = new Padding(4, 0, 4, 0);
         label4.Name = "label4";
         label4.Size = new Size(69, 15);
         label4.TabIndex = 12;
         label4.Text = "° Longitude";
         // 
         // label5
         // 
         label5.AutoSize = true;
         label5.Location = new Point(392, 159);
         label5.Margin = new Padding(4, 0, 4, 0);
         label5.Name = "label5";
         label5.Size = new Size(58, 15);
         label5.TabIndex = 14;
         label5.Text = "° Latitude";
         // 
         // textBoxDescription
         // 
         textBoxDescription.Location = new Point(127, 37);
         textBoxDescription.Margin = new Padding(4, 3, 4, 3);
         textBoxDescription.Name = "textBoxDescription";
         textBoxDescription.Size = new Size(325, 23);
         textBoxDescription.TabIndex = 3;
         // 
         // textBoxComment
         // 
         textBoxComment.Location = new Point(127, 67);
         textBoxComment.Margin = new Padding(4, 3, 4, 3);
         textBoxComment.Name = "textBoxComment";
         textBoxComment.Size = new Size(325, 23);
         textBoxComment.TabIndex = 5;
         // 
         // label6
         // 
         label6.AutoSize = true;
         label6.Location = new Point(14, 40);
         label6.Margin = new Padding(4, 0, 4, 0);
         label6.Name = "label6";
         label6.Size = new Size(82, 15);
         label6.TabIndex = 2;
         label6.Text = "Beschreibung:";
         // 
         // label7
         // 
         label7.AutoSize = true;
         label7.Location = new Point(14, 70);
         label7.Margin = new Padding(4, 0, 4, 0);
         label7.Name = "label7";
         label7.Size = new Size(73, 15);
         label7.TabIndex = 4;
         label7.Text = "Kommentar:";
         // 
         // comboBox_Name
         // 
         comboBox_Name.FormattingEnabled = true;
         comboBox_Name.Location = new Point(127, 7);
         comboBox_Name.Margin = new Padding(4, 3, 4, 3);
         comboBox_Name.Name = "comboBox_Name";
         comboBox_Name.Size = new Size(325, 23);
         comboBox_Name.TabIndex = 1;
         // 
         // button_Marker
         // 
         button_Marker.ImageAlign = ContentAlignment.MiddleLeft;
         button_Marker.Location = new Point(460, 7);
         button_Marker.Margin = new Padding(4, 3, 4, 3);
         button_Marker.Name = "button_Marker";
         button_Marker.Size = new Size(74, 45);
         button_Marker.TabIndex = 17;
         button_Marker.Text = "Typ";
         button_Marker.TextAlign = ContentAlignment.MiddleRight;
         button_Marker.UseVisualStyleBackColor = true;
         button_Marker.Click += button_Marker_Click;
         // 
         // FormMarkerEditing
         // 
         AcceptButton = button_Save;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = button_Cancel;
         ClientSize = new Size(547, 268);
         Controls.Add(button_Marker);
         Controls.Add(comboBox_Name);
         Controls.Add(label7);
         Controls.Add(label6);
         Controls.Add(textBoxComment);
         Controls.Add(textBoxDescription);
         Controls.Add(label5);
         Controls.Add(label4);
         Controls.Add(numericUpDownLon);
         Controls.Add(numericUpDownLat);
         Controls.Add(label3);
         Controls.Add(checkBox_Height);
         Controls.Add(button_Save);
         Controls.Add(button_Cancel);
         Controls.Add(label2);
         Controls.Add(numericUpDownHeight);
         Controls.Add(dateTimePickerDT);
         Controls.Add(label1);
         FormBorderStyle = FormBorderStyle.FixedDialog;
         Margin = new Padding(4, 3, 4, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormMarkerEditing";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Marker bearbeiten";
         FormClosing += FormMarkerEditing_FormClosing;
         Load += FormMarkerEditing_Load;
         ((System.ComponentModel.ISupportInitialize)numericUpDownHeight).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLat).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLon).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion
      private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePickerDT;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.CheckBox checkBox_Height;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownLat;
        private System.Windows.Forms.NumericUpDown numericUpDownLon;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.TextBox textBoxComment;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
      private System.Windows.Forms.ComboBox comboBox_Name;
      private System.Windows.Forms.Button button_Marker;
   }
}