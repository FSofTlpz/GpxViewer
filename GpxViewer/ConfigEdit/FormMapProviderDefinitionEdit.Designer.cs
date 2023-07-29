namespace GpxViewer.ConfigEdit {
   partial class FormMapProviderDefinitionEdit {
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
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.numericUpDownMinZoom = new System.Windows.Forms.NumericUpDown();
         this.numericUpDownMaxZoom = new System.Windows.Forms.NumericUpDown();
         this.label4 = new System.Windows.Forms.Label();
         this.textBoxMapName = new System.Windows.Forms.TextBox();
         this.numericUpDownZoom4Display = new System.Windows.Forms.NumericUpDown();
         this.label5 = new System.Windows.Forms.Label();
         this.comboBoxProvider = new System.Windows.Forms.ComboBox();
         this.label6 = new System.Windows.Forms.Label();
         this.textBoxKmzFile = new System.Windows.Forms.TextBox();
         this.buttonOpenKmzFile = new System.Windows.Forms.Button();
         this.groupBoxGarminKMZ = new System.Windows.Forms.GroupBox();
         this.groupBoxGarmin = new System.Windows.Forms.GroupBox();
         this.numericUpDownLineFactor = new System.Windows.Forms.NumericUpDown();
         this.label11 = new System.Windows.Forms.Label();
         this.numericUpDownSymbolFactor = new System.Windows.Forms.NumericUpDown();
         this.label10 = new System.Windows.Forms.Label();
         this.numericUpDownTextFactor = new System.Windows.Forms.NumericUpDown();
         this.label9 = new System.Windows.Forms.Label();
         this.label8 = new System.Windows.Forms.Label();
         this.buttonOpenTypFile = new System.Windows.Forms.Button();
         this.textBoxTypFile = new System.Windows.Forms.TextBox();
         this.label7 = new System.Windows.Forms.Label();
         this.buttonOpenTdbFile = new System.Windows.Forms.Button();
         this.textBoxTdbFile = new System.Windows.Forms.TextBox();
         this.groupBoxWMS = new System.Windows.Forms.GroupBox();
         this.comboBoxPictureFormat = new System.Windows.Forms.ComboBox();
         this.label17 = new System.Windows.Forms.Label();
         this.textBoxExtendedParams = new System.Windows.Forms.TextBox();
         this.label15 = new System.Windows.Forms.Label();
         this.textBoxLayer = new System.Windows.Forms.TextBox();
         this.label14 = new System.Windows.Forms.Label();
         this.label13 = new System.Windows.Forms.Label();
         this.textBoxSRS = new System.Windows.Forms.TextBox();
         this.label12 = new System.Windows.Forms.Label();
         this.textBoxVersion = new System.Windows.Forms.TextBox();
         this.label16 = new System.Windows.Forms.Label();
         this.textBoxUrl = new System.Windows.Forms.TextBox();
         this.buttonSave = new System.Windows.Forms.Button();
         this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
         this.label18 = new System.Windows.Forms.Label();
         this.checkBoxHillShading = new System.Windows.Forms.CheckBox();
         this.label19 = new System.Windows.Forms.Label();
         this.numericUpDownHillShadingAlpha = new System.Windows.Forms.NumericUpDown();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinZoom)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxZoom)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom4Display)).BeginInit();
         this.groupBoxGarminKMZ.SuspendLayout();
         this.groupBoxGarmin.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineFactor)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSymbolFactor)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTextFactor)).BeginInit();
         this.groupBoxWMS.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHillShadingAlpha)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 36);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(67, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "Kartenname:";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(11, 61);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(92, 13);
         this.label2.TabIndex = 4;
         this.label2.Text = "Zoom von .. bis ..:";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 87);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(109, 13);
         this.label3.TabIndex = 8;
         this.label3.Text = "Zoom für das Display:";
         // 
         // numericUpDownMinZoom
         // 
         this.numericUpDownMinZoom.Location = new System.Drawing.Point(173, 59);
         this.numericUpDownMinZoom.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
         this.numericUpDownMinZoom.Name = "numericUpDownMinZoom";
         this.numericUpDownMinZoom.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownMinZoom.TabIndex = 5;
         // 
         // numericUpDownMaxZoom
         // 
         this.numericUpDownMaxZoom.Location = new System.Drawing.Point(275, 59);
         this.numericUpDownMaxZoom.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
         this.numericUpDownMaxZoom.Name = "numericUpDownMaxZoom";
         this.numericUpDownMaxZoom.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownMaxZoom.TabIndex = 7;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(245, 61);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(13, 13);
         this.label4.TabIndex = 6;
         this.label4.Text = "..";
         // 
         // textBoxMapName
         // 
         this.textBoxMapName.Location = new System.Drawing.Point(173, 33);
         this.textBoxMapName.Name = "textBoxMapName";
         this.textBoxMapName.ReadOnly = true;
         this.textBoxMapName.Size = new System.Drawing.Size(352, 20);
         this.textBoxMapName.TabIndex = 3;
         // 
         // numericUpDownZoom4Display
         // 
         this.numericUpDownZoom4Display.DecimalPlaces = 2;
         this.numericUpDownZoom4Display.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownZoom4Display.Location = new System.Drawing.Point(173, 85);
         this.numericUpDownZoom4Display.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
         this.numericUpDownZoom4Display.Name = "numericUpDownZoom4Display";
         this.numericUpDownZoom4Display.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownZoom4Display.TabIndex = 9;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(12, 9);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(49, 13);
         this.label5.TabIndex = 0;
         this.label5.Text = "Provider:";
         // 
         // comboBoxProvider
         // 
         this.comboBoxProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxProvider.Enabled = false;
         this.comboBoxProvider.FormattingEnabled = true;
         this.comboBoxProvider.Location = new System.Drawing.Point(173, 6);
         this.comboBoxProvider.Name = "comboBoxProvider";
         this.comboBoxProvider.Size = new System.Drawing.Size(171, 21);
         this.comboBoxProvider.TabIndex = 1;
         this.comboBoxProvider.SelectedIndexChanged += new System.EventHandler(this.comboBoxProvider_SelectedIndexChanged);
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(10, 27);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(61, 13);
         this.label6.TabIndex = 0;
         this.label6.Text = "KMZ-Datei:";
         // 
         // textBoxKmzFile
         // 
         this.textBoxKmzFile.Location = new System.Drawing.Point(159, 24);
         this.textBoxKmzFile.Name = "textBoxKmzFile";
         this.textBoxKmzFile.ReadOnly = true;
         this.textBoxKmzFile.Size = new System.Drawing.Size(307, 20);
         this.textBoxKmzFile.TabIndex = 1;
         // 
         // buttonOpenKmzFile
         // 
         this.buttonOpenKmzFile.Image = global::GpxViewer.Properties.Resources.Open;
         this.buttonOpenKmzFile.Location = new System.Drawing.Point(472, 22);
         this.buttonOpenKmzFile.Name = "buttonOpenKmzFile";
         this.buttonOpenKmzFile.Size = new System.Drawing.Size(33, 23);
         this.buttonOpenKmzFile.TabIndex = 2;
         this.buttonOpenKmzFile.UseVisualStyleBackColor = true;
         this.buttonOpenKmzFile.Click += new System.EventHandler(this.buttonOpenKmzFile_Click);
         // 
         // groupBoxGarminKMZ
         // 
         this.groupBoxGarminKMZ.Controls.Add(this.label6);
         this.groupBoxGarminKMZ.Controls.Add(this.buttonOpenKmzFile);
         this.groupBoxGarminKMZ.Controls.Add(this.textBoxKmzFile);
         this.groupBoxGarminKMZ.Enabled = false;
         this.groupBoxGarminKMZ.Location = new System.Drawing.Point(12, 169);
         this.groupBoxGarminKMZ.Name = "groupBoxGarminKMZ";
         this.groupBoxGarminKMZ.Size = new System.Drawing.Size(511, 67);
         this.groupBoxGarminKMZ.TabIndex = 14;
         this.groupBoxGarminKMZ.TabStop = false;
         this.groupBoxGarminKMZ.Text = "Garmin-KMZ";
         // 
         // groupBoxGarmin
         // 
         this.groupBoxGarmin.Controls.Add(this.numericUpDownLineFactor);
         this.groupBoxGarmin.Controls.Add(this.label11);
         this.groupBoxGarmin.Controls.Add(this.numericUpDownSymbolFactor);
         this.groupBoxGarmin.Controls.Add(this.label10);
         this.groupBoxGarmin.Controls.Add(this.numericUpDownTextFactor);
         this.groupBoxGarmin.Controls.Add(this.label9);
         this.groupBoxGarmin.Controls.Add(this.label8);
         this.groupBoxGarmin.Controls.Add(this.buttonOpenTypFile);
         this.groupBoxGarmin.Controls.Add(this.textBoxTypFile);
         this.groupBoxGarmin.Controls.Add(this.label7);
         this.groupBoxGarmin.Controls.Add(this.buttonOpenTdbFile);
         this.groupBoxGarmin.Controls.Add(this.textBoxTdbFile);
         this.groupBoxGarmin.Enabled = false;
         this.groupBoxGarmin.Location = new System.Drawing.Point(12, 253);
         this.groupBoxGarmin.Name = "groupBoxGarmin";
         this.groupBoxGarmin.Size = new System.Drawing.Size(511, 166);
         this.groupBoxGarmin.TabIndex = 15;
         this.groupBoxGarmin.TabStop = false;
         this.groupBoxGarmin.Text = "Garmin";
         // 
         // numericUpDownLineFactor
         // 
         this.numericUpDownLineFactor.DecimalPlaces = 2;
         this.numericUpDownLineFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownLineFactor.Location = new System.Drawing.Point(158, 128);
         this.numericUpDownLineFactor.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
         this.numericUpDownLineFactor.Name = "numericUpDownLineFactor";
         this.numericUpDownLineFactor.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownLineFactor.TabIndex = 11;
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(10, 130);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(112, 13);
         this.label11.TabIndex = 10;
         this.label11.Text = "Faktor für Liniendicke:";
         // 
         // numericUpDownSymbolFactor
         // 
         this.numericUpDownSymbolFactor.DecimalPlaces = 2;
         this.numericUpDownSymbolFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownSymbolFactor.Location = new System.Drawing.Point(158, 102);
         this.numericUpDownSymbolFactor.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
         this.numericUpDownSymbolFactor.Name = "numericUpDownSymbolFactor";
         this.numericUpDownSymbolFactor.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownSymbolFactor.TabIndex = 9;
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(10, 104);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(118, 13);
         this.label10.TabIndex = 8;
         this.label10.Text = "Faktor für Markergröße:";
         // 
         // numericUpDownTextFactor
         // 
         this.numericUpDownTextFactor.DecimalPlaces = 2;
         this.numericUpDownTextFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numericUpDownTextFactor.Location = new System.Drawing.Point(158, 76);
         this.numericUpDownTextFactor.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
         this.numericUpDownTextFactor.Name = "numericUpDownTextFactor";
         this.numericUpDownTextFactor.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownTextFactor.TabIndex = 7;
         // 
         // label9
         // 
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(10, 78);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(106, 13);
         this.label9.TabIndex = 6;
         this.label9.Text = "Faktor für Textgröße:";
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(10, 53);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(59, 13);
         this.label8.TabIndex = 3;
         this.label8.Text = "TYP-Datei:";
         // 
         // buttonOpenTypFile
         // 
         this.buttonOpenTypFile.Image = global::GpxViewer.Properties.Resources.Open;
         this.buttonOpenTypFile.Location = new System.Drawing.Point(472, 48);
         this.buttonOpenTypFile.Name = "buttonOpenTypFile";
         this.buttonOpenTypFile.Size = new System.Drawing.Size(33, 23);
         this.buttonOpenTypFile.TabIndex = 5;
         this.buttonOpenTypFile.UseVisualStyleBackColor = true;
         this.buttonOpenTypFile.Click += new System.EventHandler(this.buttonOpenTypFile_Click);
         // 
         // textBoxTypFile
         // 
         this.textBoxTypFile.Location = new System.Drawing.Point(158, 50);
         this.textBoxTypFile.Name = "textBoxTypFile";
         this.textBoxTypFile.ReadOnly = true;
         this.textBoxTypFile.Size = new System.Drawing.Size(307, 20);
         this.textBoxTypFile.TabIndex = 4;
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(10, 27);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(60, 13);
         this.label7.TabIndex = 0;
         this.label7.Text = "TDB-Datei:";
         // 
         // buttonOpenTdbFile
         // 
         this.buttonOpenTdbFile.Image = global::GpxViewer.Properties.Resources.Open;
         this.buttonOpenTdbFile.Location = new System.Drawing.Point(472, 22);
         this.buttonOpenTdbFile.Name = "buttonOpenTdbFile";
         this.buttonOpenTdbFile.Size = new System.Drawing.Size(33, 23);
         this.buttonOpenTdbFile.TabIndex = 2;
         this.buttonOpenTdbFile.UseVisualStyleBackColor = true;
         this.buttonOpenTdbFile.Click += new System.EventHandler(this.buttonOpenTdbFile_Click);
         // 
         // textBoxTdbFile
         // 
         this.textBoxTdbFile.Location = new System.Drawing.Point(159, 24);
         this.textBoxTdbFile.Name = "textBoxTdbFile";
         this.textBoxTdbFile.ReadOnly = true;
         this.textBoxTdbFile.Size = new System.Drawing.Size(307, 20);
         this.textBoxTdbFile.TabIndex = 1;
         // 
         // groupBoxWMS
         // 
         this.groupBoxWMS.Controls.Add(this.comboBoxPictureFormat);
         this.groupBoxWMS.Controls.Add(this.label17);
         this.groupBoxWMS.Controls.Add(this.textBoxExtendedParams);
         this.groupBoxWMS.Controls.Add(this.label15);
         this.groupBoxWMS.Controls.Add(this.textBoxLayer);
         this.groupBoxWMS.Controls.Add(this.label14);
         this.groupBoxWMS.Controls.Add(this.label13);
         this.groupBoxWMS.Controls.Add(this.textBoxSRS);
         this.groupBoxWMS.Controls.Add(this.label12);
         this.groupBoxWMS.Controls.Add(this.textBoxVersion);
         this.groupBoxWMS.Controls.Add(this.label16);
         this.groupBoxWMS.Controls.Add(this.textBoxUrl);
         this.groupBoxWMS.Enabled = false;
         this.groupBoxWMS.Location = new System.Drawing.Point(12, 441);
         this.groupBoxWMS.Name = "groupBoxWMS";
         this.groupBoxWMS.Size = new System.Drawing.Size(511, 191);
         this.groupBoxWMS.TabIndex = 16;
         this.groupBoxWMS.TabStop = false;
         this.groupBoxWMS.Text = "WMS (Web Map Service)";
         // 
         // comboBoxPictureFormat
         // 
         this.comboBoxPictureFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxPictureFormat.Enabled = false;
         this.comboBoxPictureFormat.FormattingEnabled = true;
         this.comboBoxPictureFormat.Items.AddRange(new object[] {
            "PNG",
            "JPG"});
         this.comboBoxPictureFormat.Location = new System.Drawing.Point(159, 102);
         this.comboBoxPictureFormat.Name = "comboBoxPictureFormat";
         this.comboBoxPictureFormat.Size = new System.Drawing.Size(109, 21);
         this.comboBoxPictureFormat.TabIndex = 7;
         // 
         // label17
         // 
         this.label17.AutoSize = true;
         this.label17.Location = new System.Drawing.Point(9, 157);
         this.label17.Name = "label17";
         this.label17.Size = new System.Drawing.Size(95, 13);
         this.label17.TabIndex = 10;
         this.label17.Text = "weitere Parameter:";
         // 
         // textBoxExtendedParams
         // 
         this.textBoxExtendedParams.Location = new System.Drawing.Point(158, 154);
         this.textBoxExtendedParams.Name = "textBoxExtendedParams";
         this.textBoxExtendedParams.ReadOnly = true;
         this.textBoxExtendedParams.Size = new System.Drawing.Size(346, 20);
         this.textBoxExtendedParams.TabIndex = 11;
         // 
         // label15
         // 
         this.label15.AutoSize = true;
         this.label15.Location = new System.Drawing.Point(9, 131);
         this.label15.Name = "label15";
         this.label15.Size = new System.Drawing.Size(36, 13);
         this.label15.TabIndex = 8;
         this.label15.Text = "Layer:";
         // 
         // textBoxLayer
         // 
         this.textBoxLayer.Location = new System.Drawing.Point(158, 128);
         this.textBoxLayer.Name = "textBoxLayer";
         this.textBoxLayer.ReadOnly = true;
         this.textBoxLayer.Size = new System.Drawing.Size(346, 20);
         this.textBoxLayer.TabIndex = 9;
         // 
         // label14
         // 
         this.label14.AutoSize = true;
         this.label14.Location = new System.Drawing.Point(9, 105);
         this.label14.Name = "label14";
         this.label14.Size = new System.Drawing.Size(56, 13);
         this.label14.TabIndex = 6;
         this.label14.Text = "Bildformat:";
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(9, 79);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(32, 13);
         this.label13.TabIndex = 4;
         this.label13.Text = "SRS:";
         // 
         // textBoxSRS
         // 
         this.textBoxSRS.Location = new System.Drawing.Point(158, 76);
         this.textBoxSRS.Name = "textBoxSRS";
         this.textBoxSRS.ReadOnly = true;
         this.textBoxSRS.Size = new System.Drawing.Size(110, 20);
         this.textBoxSRS.TabIndex = 5;
         // 
         // label12
         // 
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(9, 53);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(45, 13);
         this.label12.TabIndex = 2;
         this.label12.Text = "Version:";
         // 
         // textBoxVersion
         // 
         this.textBoxVersion.Location = new System.Drawing.Point(158, 50);
         this.textBoxVersion.Name = "textBoxVersion";
         this.textBoxVersion.ReadOnly = true;
         this.textBoxVersion.Size = new System.Drawing.Size(110, 20);
         this.textBoxVersion.TabIndex = 3;
         // 
         // label16
         // 
         this.label16.AutoSize = true;
         this.label16.Location = new System.Drawing.Point(10, 27);
         this.label16.Name = "label16";
         this.label16.Size = new System.Drawing.Size(32, 13);
         this.label16.TabIndex = 0;
         this.label16.Text = "URL:";
         // 
         // textBoxUrl
         // 
         this.textBoxUrl.Location = new System.Drawing.Point(159, 24);
         this.textBoxUrl.Name = "textBoxUrl";
         this.textBoxUrl.ReadOnly = true;
         this.textBoxUrl.Size = new System.Drawing.Size(346, 20);
         this.textBoxUrl.TabIndex = 1;
         // 
         // buttonSave
         // 
         this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonSave.Location = new System.Drawing.Point(209, 651);
         this.buttonSave.Name = "buttonSave";
         this.buttonSave.Size = new System.Drawing.Size(106, 23);
         this.buttonSave.TabIndex = 13;
         this.buttonSave.Text = "speichern";
         this.buttonSave.UseVisualStyleBackColor = true;
         this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
         // 
         // openFileDialog1
         // 
         this.openFileDialog1.FileName = "openFileDialog1";
         // 
         // label18
         // 
         this.label18.AutoSize = true;
         this.label18.Location = new System.Drawing.Point(11, 111);
         this.label18.Name = "label18";
         this.label18.Size = new System.Drawing.Size(61, 13);
         this.label18.TabIndex = 10;
         this.label18.Text = "Hillshading:";
         // 
         // checkBoxHillShading
         // 
         this.checkBoxHillShading.AutoSize = true;
         this.checkBoxHillShading.Location = new System.Drawing.Point(173, 111);
         this.checkBoxHillShading.Name = "checkBoxHillShading";
         this.checkBoxHillShading.Size = new System.Drawing.Size(15, 14);
         this.checkBoxHillShading.TabIndex = 11;
         this.checkBoxHillShading.UseVisualStyleBackColor = true;
         // 
         // label19
         // 
         this.label19.AutoSize = true;
         this.label19.Location = new System.Drawing.Point(12, 133);
         this.label19.Name = "label19";
         this.label19.Size = new System.Drawing.Size(87, 13);
         this.label19.TabIndex = 12;
         this.label19.Text = "Hillshadingalpha:";
         // 
         // numericUpDownHillShadingAlpha
         // 
         this.numericUpDownHillShadingAlpha.Location = new System.Drawing.Point(172, 131);
         this.numericUpDownHillShadingAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
         this.numericUpDownHillShadingAlpha.Name = "numericUpDownHillShadingAlpha";
         this.numericUpDownHillShadingAlpha.Size = new System.Drawing.Size(55, 20);
         this.numericUpDownHillShadingAlpha.TabIndex = 13;
         this.numericUpDownHillShadingAlpha.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
         // 
         // FormMapProviderDefinitionEdit
         // 
         this.AcceptButton = this.buttonSave;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(538, 698);
         this.Controls.Add(this.numericUpDownHillShadingAlpha);
         this.Controls.Add(this.label19);
         this.Controls.Add(this.checkBoxHillShading);
         this.Controls.Add(this.buttonSave);
         this.Controls.Add(this.label18);
         this.Controls.Add(this.groupBoxWMS);
         this.Controls.Add(this.groupBoxGarmin);
         this.Controls.Add(this.groupBoxGarminKMZ);
         this.Controls.Add(this.comboBoxProvider);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.numericUpDownZoom4Display);
         this.Controls.Add(this.textBoxMapName);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.numericUpDownMaxZoom);
         this.Controls.Add(this.numericUpDownMinZoom);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormMapProviderDefinitionEdit";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "FormMapProviderEdit";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMapProviderDefinitionEdit_FormClosing);
         this.Load += new System.EventHandler(this.FormMapProviderDefinitionEdit_Load);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinZoom)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxZoom)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom4Display)).EndInit();
         this.groupBoxGarminKMZ.ResumeLayout(false);
         this.groupBoxGarminKMZ.PerformLayout();
         this.groupBoxGarmin.ResumeLayout(false);
         this.groupBoxGarmin.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineFactor)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSymbolFactor)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTextFactor)).EndInit();
         this.groupBoxWMS.ResumeLayout(false);
         this.groupBoxWMS.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHillShadingAlpha)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.NumericUpDown numericUpDownMinZoom;
      private System.Windows.Forms.NumericUpDown numericUpDownMaxZoom;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox textBoxMapName;
      private System.Windows.Forms.NumericUpDown numericUpDownZoom4Display;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.ComboBox comboBoxProvider;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox textBoxKmzFile;
      private System.Windows.Forms.Button buttonOpenKmzFile;
      private System.Windows.Forms.GroupBox groupBoxGarminKMZ;
      private System.Windows.Forms.GroupBox groupBoxGarmin;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.Button buttonOpenTdbFile;
      private System.Windows.Forms.TextBox textBoxTdbFile;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.Button buttonOpenTypFile;
      private System.Windows.Forms.TextBox textBoxTypFile;
      private System.Windows.Forms.NumericUpDown numericUpDownSymbolFactor;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.NumericUpDown numericUpDownTextFactor;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.NumericUpDown numericUpDownLineFactor;
      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.GroupBox groupBoxWMS;
      private System.Windows.Forms.Label label16;
      private System.Windows.Forms.TextBox textBoxUrl;
      private System.Windows.Forms.Label label17;
      private System.Windows.Forms.TextBox textBoxExtendedParams;
      private System.Windows.Forms.Label label15;
      private System.Windows.Forms.TextBox textBoxLayer;
      private System.Windows.Forms.Label label14;
      private System.Windows.Forms.Label label13;
      private System.Windows.Forms.TextBox textBoxSRS;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.TextBox textBoxVersion;
      private System.Windows.Forms.ComboBox comboBoxPictureFormat;
      private System.Windows.Forms.Button buttonSave;
      private System.Windows.Forms.OpenFileDialog openFileDialog1;
      private System.Windows.Forms.Label label18;
      private System.Windows.Forms.CheckBox checkBoxHillShading;
      private System.Windows.Forms.Label label19;
      private System.Windows.Forms.NumericUpDown numericUpDownHillShadingAlpha;
   }
}