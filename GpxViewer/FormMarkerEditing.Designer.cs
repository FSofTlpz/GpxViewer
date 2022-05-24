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
         this.label1 = new System.Windows.Forms.Label();
         this.dateTimePickerDT = new System.Windows.Forms.DateTimePicker();
         this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
         this.label2 = new System.Windows.Forms.Label();
         this.button_Cancel = new System.Windows.Forms.Button();
         this.button_Save = new System.Windows.Forms.Button();
         this.checkBox_Height = new System.Windows.Forms.CheckBox();
         this.label3 = new System.Windows.Forms.Label();
         this.numericUpDownLat = new System.Windows.Forms.NumericUpDown();
         this.numericUpDownLon = new System.Windows.Forms.NumericUpDown();
         this.label4 = new System.Windows.Forms.Label();
         this.label5 = new System.Windows.Forms.Label();
         this.textBoxDescription = new System.Windows.Forms.TextBox();
         this.textBoxComment = new System.Windows.Forms.TextBox();
         this.label6 = new System.Windows.Forms.Label();
         this.label7 = new System.Windows.Forms.Label();
         this.comboBox_Name = new System.Windows.Forms.ComboBox();
         this.button_Marker = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLat)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLon)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 9);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(38, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "Name:";
         // 
         // dateTimePickerDT
         // 
         this.dateTimePickerDT.CustomFormat = "MMMM, d.M.yyyy, H:mm:ss";
         this.dateTimePickerDT.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
         this.dateTimePickerDT.Location = new System.Drawing.Point(109, 84);
         this.dateTimePickerDT.Name = "dateTimePickerDT";
         this.dateTimePickerDT.ShowCheckBox = true;
         this.dateTimePickerDT.Size = new System.Drawing.Size(258, 20);
         this.dateTimePickerDT.TabIndex = 7;
         // 
         // numericUpDownHeight
         // 
         this.numericUpDownHeight.Location = new System.Drawing.Point(109, 110);
         this.numericUpDownHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
         this.numericUpDownHeight.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
         this.numericUpDownHeight.Name = "numericUpDownHeight";
         this.numericUpDownHeight.Size = new System.Drawing.Size(91, 20);
         this.numericUpDownHeight.TabIndex = 9;
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(218, 112);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(74, 13);
         this.label2.TabIndex = 10;
         this.label2.Text = "Höhe in Meter";
         // 
         // button_Cancel
         // 
         this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_Cancel.Image = global::GpxViewer.Properties.Resources.cancel;
         this.button_Cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Cancel.Location = new System.Drawing.Point(238, 187);
         this.button_Cancel.Name = "button_Cancel";
         this.button_Cancel.Size = new System.Drawing.Size(101, 31);
         this.button_Cancel.TabIndex = 15;
         this.button_Cancel.Text = "zurück";
         this.button_Cancel.UseVisualStyleBackColor = true;
         this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
         // 
         // button_Save
         // 
         this.button_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_Save.Image = global::GpxViewer.Properties.Resources.ok;
         this.button_Save.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Save.Location = new System.Drawing.Point(356, 187);
         this.button_Save.Name = "button_Save";
         this.button_Save.Size = new System.Drawing.Size(101, 31);
         this.button_Save.TabIndex = 16;
         this.button_Save.Text = "speichern";
         this.button_Save.UseVisualStyleBackColor = true;
         this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
         // 
         // checkBox_Height
         // 
         this.checkBox_Height.AutoSize = true;
         this.checkBox_Height.Location = new System.Drawing.Point(15, 116);
         this.checkBox_Height.Name = "checkBox_Height";
         this.checkBox_Height.Size = new System.Drawing.Size(15, 14);
         this.checkBox_Height.TabIndex = 8;
         this.checkBox_Height.UseVisualStyleBackColor = true;
         this.checkBox_Height.CheckedChanged += new System.EventHandler(this.checkBox_Height_CheckedChanged);
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 90);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(29, 13);
         this.label3.TabIndex = 6;
         this.label3.Text = "UTC";
         // 
         // numericUpDownLat
         // 
         this.numericUpDownLat.DecimalPlaces = 8;
         this.numericUpDownLat.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownLat.Location = new System.Drawing.Point(221, 136);
         this.numericUpDownLat.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
         this.numericUpDownLat.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
         this.numericUpDownLat.Name = "numericUpDownLat";
         this.numericUpDownLat.Size = new System.Drawing.Size(109, 20);
         this.numericUpDownLat.TabIndex = 13;
         // 
         // numericUpDownLon
         // 
         this.numericUpDownLon.DecimalPlaces = 8;
         this.numericUpDownLon.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownLon.Location = new System.Drawing.Point(15, 136);
         this.numericUpDownLon.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
         this.numericUpDownLon.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
         this.numericUpDownLon.Name = "numericUpDownLon";
         this.numericUpDownLon.Size = new System.Drawing.Size(109, 20);
         this.numericUpDownLon.TabIndex = 11;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(130, 138);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(61, 13);
         this.label4.TabIndex = 12;
         this.label4.Text = "° Longitude";
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(336, 138);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(52, 13);
         this.label5.TabIndex = 14;
         this.label5.Text = "° Latitude";
         // 
         // textBoxDescription
         // 
         this.textBoxDescription.Location = new System.Drawing.Point(109, 32);
         this.textBoxDescription.Name = "textBoxDescription";
         this.textBoxDescription.Size = new System.Drawing.Size(279, 20);
         this.textBoxDescription.TabIndex = 3;
         // 
         // textBoxComment
         // 
         this.textBoxComment.Location = new System.Drawing.Point(109, 58);
         this.textBoxComment.Name = "textBoxComment";
         this.textBoxComment.Size = new System.Drawing.Size(279, 20);
         this.textBoxComment.TabIndex = 5;
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(12, 35);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(75, 13);
         this.label6.TabIndex = 2;
         this.label6.Text = "Beschreibung:";
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(12, 61);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(63, 13);
         this.label7.TabIndex = 4;
         this.label7.Text = "Kommentar:";
         // 
         // comboBox_Name
         // 
         this.comboBox_Name.FormattingEnabled = true;
         this.comboBox_Name.Location = new System.Drawing.Point(109, 6);
         this.comboBox_Name.Name = "comboBox_Name";
         this.comboBox_Name.Size = new System.Drawing.Size(279, 21);
         this.comboBox_Name.TabIndex = 1;
         // 
         // button_Marker
         // 
         this.button_Marker.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_Marker.Location = new System.Drawing.Point(394, 6);
         this.button_Marker.Name = "button_Marker";
         this.button_Marker.Size = new System.Drawing.Size(63, 39);
         this.button_Marker.TabIndex = 17;
         this.button_Marker.Text = "Typ";
         this.button_Marker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.button_Marker.UseVisualStyleBackColor = true;
         this.button_Marker.Click += new System.EventHandler(this.button_Marker_Click);
         // 
         // FormMarkerEditing
         // 
         this.AcceptButton = this.button_Save;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_Cancel;
         this.ClientSize = new System.Drawing.Size(469, 230);
         this.Controls.Add(this.button_Marker);
         this.Controls.Add(this.comboBox_Name);
         this.Controls.Add(this.label7);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.textBoxComment);
         this.Controls.Add(this.textBoxDescription);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.numericUpDownLon);
         this.Controls.Add(this.numericUpDownLat);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.checkBox_Height);
         this.Controls.Add(this.button_Save);
         this.Controls.Add(this.button_Cancel);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.numericUpDownHeight);
         this.Controls.Add(this.dateTimePickerDT);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormMarkerEditing";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Marker bearbeiten";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormExtMarkerEditing_FormClosing);
         this.Load += new System.EventHandler(this.FormExtMarkerEditing_Load);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLat)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLon)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

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