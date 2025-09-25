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
         label1 = new Label();
         label2 = new Label();
         numericUpDownMinZoom = new NumericUpDown();
         numericUpDownMaxZoom = new NumericUpDown();
         label4 = new Label();
         textBoxMapName = new TextBox();
         label5 = new Label();
         comboBoxProvider = new ComboBox();
         label6 = new Label();
         textBoxKmzFile = new TextBox();
         buttonOpenKmzFile = new Button();
         groupBoxGarminKMZ = new GroupBox();
         groupBoxGarmin = new GroupBox();
         numericUpDownLineFactor = new NumericUpDown();
         label11 = new Label();
         numericUpDownSymbolFactor = new NumericUpDown();
         label10 = new Label();
         numericUpDownTextFactor = new NumericUpDown();
         label9 = new Label();
         label8 = new Label();
         buttonOpenTypFile = new Button();
         textBoxTypFile = new TextBox();
         label7 = new Label();
         buttonOpenTdbFile = new Button();
         textBoxTdbFile = new TextBox();
         groupBoxWMS = new GroupBox();
         comboBoxPictureFormat = new ComboBox();
         label17 = new Label();
         textBoxExtendedParams = new TextBox();
         label15 = new Label();
         textBoxLayer = new TextBox();
         label14 = new Label();
         label13 = new Label();
         textBoxSRS = new TextBox();
         label12 = new Label();
         textBoxVersion = new TextBox();
         label16 = new Label();
         textBoxUrl = new TextBox();
         buttonSave = new Button();
         openFileDialog1 = new OpenFileDialog();
         label18 = new Label();
         checkBoxHillShading = new CheckBox();
         label19 = new Label();
         numericUpDownHillShadingAlpha = new NumericUpDown();
         groupBoxMulti = new GroupBox();
         buttonMultiMapAdd = new Button();
         buttonMultiMapDelete = new Button();
         buttonMultiMapDown = new Button();
         buttonMultiMapUp = new Button();
         listBoxMaps = new ListBox();
         groupBox1 = new GroupBox();
         tabControlExtended = new TabControl();
         tabPageKmz = new TabPage();
         tabPageGarmin = new TabPage();
         tabPageWMS = new TabPage();
         tabPageMulti = new TabPage();
         tabPageEmpty = new TabPage();
         buttonCancel = new Button();
         ((System.ComponentModel.ISupportInitialize)numericUpDownMinZoom).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownMaxZoom).BeginInit();
         groupBoxGarminKMZ.SuspendLayout();
         groupBoxGarmin.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLineFactor).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownSymbolFactor).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownTextFactor).BeginInit();
         groupBoxWMS.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)numericUpDownHillShadingAlpha).BeginInit();
         groupBoxMulti.SuspendLayout();
         groupBox1.SuspendLayout();
         tabControlExtended.SuspendLayout();
         tabPageKmz.SuspendLayout();
         tabPageGarmin.SuspendLayout();
         tabPageWMS.SuspendLayout();
         tabPageMulti.SuspendLayout();
         SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(7, 51);
         label1.Margin = new Padding(4, 0, 4, 0);
         label1.Name = "label1";
         label1.Size = new Size(74, 15);
         label1.TabIndex = 2;
         label1.Text = "Kartenname:";
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(6, 79);
         label2.Margin = new Padding(4, 0, 4, 0);
         label2.Name = "label2";
         label2.Size = new Size(104, 15);
         label2.TabIndex = 4;
         label2.Text = "Zoom von .. bis .. :";
         // 
         // numericUpDownMinZoom
         // 
         numericUpDownMinZoom.Location = new Point(195, 77);
         numericUpDownMinZoom.Margin = new Padding(4, 3, 4, 3);
         numericUpDownMinZoom.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
         numericUpDownMinZoom.Name = "numericUpDownMinZoom";
         numericUpDownMinZoom.Size = new Size(64, 23);
         numericUpDownMinZoom.TabIndex = 5;
         // 
         // numericUpDownMaxZoom
         // 
         numericUpDownMaxZoom.Location = new Point(314, 77);
         numericUpDownMaxZoom.Margin = new Padding(4, 3, 4, 3);
         numericUpDownMaxZoom.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
         numericUpDownMaxZoom.Name = "numericUpDownMaxZoom";
         numericUpDownMaxZoom.Size = new Size(64, 23);
         numericUpDownMaxZoom.TabIndex = 7;
         // 
         // label4
         // 
         label4.AutoSize = true;
         label4.Location = new Point(279, 79);
         label4.Margin = new Padding(4, 0, 4, 0);
         label4.Name = "label4";
         label4.Size = new Size(13, 15);
         label4.TabIndex = 6;
         label4.Text = "..";
         // 
         // textBoxMapName
         // 
         textBoxMapName.Location = new Point(195, 47);
         textBoxMapName.Margin = new Padding(4, 3, 4, 3);
         textBoxMapName.Name = "textBoxMapName";
         textBoxMapName.Size = new Size(410, 23);
         textBoxMapName.TabIndex = 3;
         // 
         // label5
         // 
         label5.AutoSize = true;
         label5.Location = new Point(7, 19);
         label5.Margin = new Padding(4, 0, 4, 0);
         label5.Name = "label5";
         label5.Size = new Size(54, 15);
         label5.TabIndex = 0;
         label5.Text = "Provider:";
         // 
         // comboBoxProvider
         // 
         comboBoxProvider.DropDownStyle = ComboBoxStyle.DropDownList;
         comboBoxProvider.Enabled = false;
         comboBoxProvider.FormattingEnabled = true;
         comboBoxProvider.Location = new Point(195, 16);
         comboBoxProvider.Margin = new Padding(4, 3, 4, 3);
         comboBoxProvider.Name = "comboBoxProvider";
         comboBoxProvider.Size = new Size(199, 23);
         comboBoxProvider.TabIndex = 1;
         comboBoxProvider.SelectedIndexChanged += comboBoxProvider_SelectedIndexChanged;
         // 
         // label6
         // 
         label6.AutoSize = true;
         label6.Location = new Point(12, 31);
         label6.Margin = new Padding(4, 0, 4, 0);
         label6.Name = "label6";
         label6.Size = new Size(67, 15);
         label6.TabIndex = 0;
         label6.Text = "KMZ-Datei:";
         // 
         // textBoxKmzFile
         // 
         textBoxKmzFile.Location = new Point(186, 28);
         textBoxKmzFile.Margin = new Padding(4, 3, 4, 3);
         textBoxKmzFile.Name = "textBoxKmzFile";
         textBoxKmzFile.ReadOnly = true;
         textBoxKmzFile.Size = new Size(358, 23);
         textBoxKmzFile.TabIndex = 1;
         // 
         // buttonOpenKmzFile
         // 
         buttonOpenKmzFile.Image = Properties.Resources.Open;
         buttonOpenKmzFile.Location = new Point(551, 25);
         buttonOpenKmzFile.Margin = new Padding(4, 3, 4, 3);
         buttonOpenKmzFile.Name = "buttonOpenKmzFile";
         buttonOpenKmzFile.Size = new Size(38, 27);
         buttonOpenKmzFile.TabIndex = 2;
         buttonOpenKmzFile.UseVisualStyleBackColor = true;
         buttonOpenKmzFile.Click += buttonOpenKmzFile_Click;
         // 
         // groupBoxGarminKMZ
         // 
         groupBoxGarminKMZ.Controls.Add(label6);
         groupBoxGarminKMZ.Controls.Add(buttonOpenKmzFile);
         groupBoxGarminKMZ.Controls.Add(textBoxKmzFile);
         groupBoxGarminKMZ.Location = new Point(5, 6);
         groupBoxGarminKMZ.Margin = new Padding(4, 3, 4, 3);
         groupBoxGarminKMZ.Name = "groupBoxGarminKMZ";
         groupBoxGarminKMZ.Padding = new Padding(4, 3, 4, 3);
         groupBoxGarminKMZ.Size = new Size(598, 77);
         groupBoxGarminKMZ.TabIndex = 14;
         groupBoxGarminKMZ.TabStop = false;
         groupBoxGarminKMZ.Text = "Garmin-KMZ";
         // 
         // groupBoxGarmin
         // 
         groupBoxGarmin.Controls.Add(numericUpDownLineFactor);
         groupBoxGarmin.Controls.Add(label11);
         groupBoxGarmin.Controls.Add(numericUpDownSymbolFactor);
         groupBoxGarmin.Controls.Add(label10);
         groupBoxGarmin.Controls.Add(numericUpDownTextFactor);
         groupBoxGarmin.Controls.Add(label9);
         groupBoxGarmin.Controls.Add(label8);
         groupBoxGarmin.Controls.Add(buttonOpenTypFile);
         groupBoxGarmin.Controls.Add(textBoxTypFile);
         groupBoxGarmin.Controls.Add(label7);
         groupBoxGarmin.Controls.Add(buttonOpenTdbFile);
         groupBoxGarmin.Controls.Add(textBoxTdbFile);
         groupBoxGarmin.Location = new Point(5, 6);
         groupBoxGarmin.Margin = new Padding(4, 3, 4, 3);
         groupBoxGarmin.Name = "groupBoxGarmin";
         groupBoxGarmin.Padding = new Padding(4, 3, 4, 3);
         groupBoxGarmin.Size = new Size(598, 192);
         groupBoxGarmin.TabIndex = 15;
         groupBoxGarmin.TabStop = false;
         groupBoxGarmin.Text = "Garmin";
         // 
         // numericUpDownLineFactor
         // 
         numericUpDownLineFactor.DecimalPlaces = 2;
         numericUpDownLineFactor.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
         numericUpDownLineFactor.Location = new Point(184, 148);
         numericUpDownLineFactor.Margin = new Padding(4, 3, 4, 3);
         numericUpDownLineFactor.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
         numericUpDownLineFactor.Name = "numericUpDownLineFactor";
         numericUpDownLineFactor.Size = new Size(64, 23);
         numericUpDownLineFactor.TabIndex = 11;
         // 
         // label11
         // 
         label11.AutoSize = true;
         label11.Location = new Point(12, 150);
         label11.Margin = new Padding(4, 0, 4, 0);
         label11.Name = "label11";
         label11.Size = new Size(124, 15);
         label11.TabIndex = 10;
         label11.Text = "Faktor für Liniendicke:";
         // 
         // numericUpDownSymbolFactor
         // 
         numericUpDownSymbolFactor.DecimalPlaces = 2;
         numericUpDownSymbolFactor.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
         numericUpDownSymbolFactor.Location = new Point(184, 118);
         numericUpDownSymbolFactor.Margin = new Padding(4, 3, 4, 3);
         numericUpDownSymbolFactor.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
         numericUpDownSymbolFactor.Name = "numericUpDownSymbolFactor";
         numericUpDownSymbolFactor.Size = new Size(64, 23);
         numericUpDownSymbolFactor.TabIndex = 9;
         // 
         // label10
         // 
         label10.AutoSize = true;
         label10.Location = new Point(12, 120);
         label10.Margin = new Padding(4, 0, 4, 0);
         label10.Name = "label10";
         label10.Size = new Size(132, 15);
         label10.TabIndex = 8;
         label10.Text = "Faktor für Markergröße:";
         // 
         // numericUpDownTextFactor
         // 
         numericUpDownTextFactor.DecimalPlaces = 2;
         numericUpDownTextFactor.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
         numericUpDownTextFactor.Location = new Point(184, 88);
         numericUpDownTextFactor.Margin = new Padding(4, 3, 4, 3);
         numericUpDownTextFactor.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
         numericUpDownTextFactor.Name = "numericUpDownTextFactor";
         numericUpDownTextFactor.Size = new Size(64, 23);
         numericUpDownTextFactor.TabIndex = 7;
         // 
         // label9
         // 
         label9.AutoSize = true;
         label9.Location = new Point(12, 90);
         label9.Margin = new Padding(4, 0, 4, 0);
         label9.Name = "label9";
         label9.Size = new Size(116, 15);
         label9.TabIndex = 6;
         label9.Text = "Faktor für Textgröße:";
         // 
         // label8
         // 
         label8.AutoSize = true;
         label8.Location = new Point(12, 61);
         label8.Margin = new Padding(4, 0, 4, 0);
         label8.Name = "label8";
         label8.Size = new Size(63, 15);
         label8.TabIndex = 3;
         label8.Text = "TYP-Datei:";
         // 
         // buttonOpenTypFile
         // 
         buttonOpenTypFile.Image = Properties.Resources.Open;
         buttonOpenTypFile.Location = new Point(551, 55);
         buttonOpenTypFile.Margin = new Padding(4, 3, 4, 3);
         buttonOpenTypFile.Name = "buttonOpenTypFile";
         buttonOpenTypFile.Size = new Size(38, 27);
         buttonOpenTypFile.TabIndex = 5;
         buttonOpenTypFile.UseVisualStyleBackColor = true;
         buttonOpenTypFile.Click += buttonOpenTypFile_Click;
         // 
         // textBoxTypFile
         // 
         textBoxTypFile.Location = new Point(184, 58);
         textBoxTypFile.Margin = new Padding(4, 3, 4, 3);
         textBoxTypFile.Name = "textBoxTypFile";
         textBoxTypFile.ReadOnly = true;
         textBoxTypFile.Size = new Size(358, 23);
         textBoxTypFile.TabIndex = 4;
         // 
         // label7
         // 
         label7.AutoSize = true;
         label7.Location = new Point(12, 31);
         label7.Margin = new Padding(4, 0, 4, 0);
         label7.Name = "label7";
         label7.Size = new Size(64, 15);
         label7.TabIndex = 0;
         label7.Text = "TDB-Datei:";
         // 
         // buttonOpenTdbFile
         // 
         buttonOpenTdbFile.Image = Properties.Resources.Open;
         buttonOpenTdbFile.Location = new Point(551, 25);
         buttonOpenTdbFile.Margin = new Padding(4, 3, 4, 3);
         buttonOpenTdbFile.Name = "buttonOpenTdbFile";
         buttonOpenTdbFile.Size = new Size(38, 27);
         buttonOpenTdbFile.TabIndex = 2;
         buttonOpenTdbFile.UseVisualStyleBackColor = true;
         buttonOpenTdbFile.Click += buttonOpenTdbFile_Click;
         // 
         // textBoxTdbFile
         // 
         textBoxTdbFile.Location = new Point(186, 28);
         textBoxTdbFile.Margin = new Padding(4, 3, 4, 3);
         textBoxTdbFile.Name = "textBoxTdbFile";
         textBoxTdbFile.ReadOnly = true;
         textBoxTdbFile.Size = new Size(358, 23);
         textBoxTdbFile.TabIndex = 1;
         // 
         // groupBoxWMS
         // 
         groupBoxWMS.Controls.Add(comboBoxPictureFormat);
         groupBoxWMS.Controls.Add(label17);
         groupBoxWMS.Controls.Add(textBoxExtendedParams);
         groupBoxWMS.Controls.Add(label15);
         groupBoxWMS.Controls.Add(textBoxLayer);
         groupBoxWMS.Controls.Add(label14);
         groupBoxWMS.Controls.Add(label13);
         groupBoxWMS.Controls.Add(textBoxSRS);
         groupBoxWMS.Controls.Add(label12);
         groupBoxWMS.Controls.Add(textBoxVersion);
         groupBoxWMS.Controls.Add(label16);
         groupBoxWMS.Controls.Add(textBoxUrl);
         groupBoxWMS.Location = new Point(5, 6);
         groupBoxWMS.Margin = new Padding(4, 3, 4, 3);
         groupBoxWMS.Name = "groupBoxWMS";
         groupBoxWMS.Padding = new Padding(4, 3, 4, 3);
         groupBoxWMS.Size = new Size(598, 220);
         groupBoxWMS.TabIndex = 16;
         groupBoxWMS.TabStop = false;
         groupBoxWMS.Text = "WMS (Web Map Service)";
         // 
         // comboBoxPictureFormat
         // 
         comboBoxPictureFormat.DropDownStyle = ComboBoxStyle.DropDownList;
         comboBoxPictureFormat.Enabled = false;
         comboBoxPictureFormat.FormattingEnabled = true;
         comboBoxPictureFormat.Items.AddRange(new object[] { "PNG", "JPG" });
         comboBoxPictureFormat.Location = new Point(186, 118);
         comboBoxPictureFormat.Margin = new Padding(4, 3, 4, 3);
         comboBoxPictureFormat.Name = "comboBoxPictureFormat";
         comboBoxPictureFormat.Size = new Size(126, 23);
         comboBoxPictureFormat.TabIndex = 7;
         // 
         // label17
         // 
         label17.AutoSize = true;
         label17.Location = new Point(10, 181);
         label17.Margin = new Padding(4, 0, 4, 0);
         label17.Name = "label17";
         label17.Size = new Size(105, 15);
         label17.TabIndex = 10;
         label17.Text = "weitere Parameter:";
         // 
         // textBoxExtendedParams
         // 
         textBoxExtendedParams.Location = new Point(184, 178);
         textBoxExtendedParams.Margin = new Padding(4, 3, 4, 3);
         textBoxExtendedParams.Name = "textBoxExtendedParams";
         textBoxExtendedParams.ReadOnly = true;
         textBoxExtendedParams.Size = new Size(403, 23);
         textBoxExtendedParams.TabIndex = 11;
         // 
         // label15
         // 
         label15.AutoSize = true;
         label15.Location = new Point(10, 151);
         label15.Margin = new Padding(4, 0, 4, 0);
         label15.Name = "label15";
         label15.Size = new Size(38, 15);
         label15.TabIndex = 8;
         label15.Text = "Layer:";
         // 
         // textBoxLayer
         // 
         textBoxLayer.Location = new Point(184, 148);
         textBoxLayer.Margin = new Padding(4, 3, 4, 3);
         textBoxLayer.Name = "textBoxLayer";
         textBoxLayer.ReadOnly = true;
         textBoxLayer.Size = new Size(403, 23);
         textBoxLayer.TabIndex = 9;
         // 
         // label14
         // 
         label14.AutoSize = true;
         label14.Location = new Point(10, 121);
         label14.Margin = new Padding(4, 0, 4, 0);
         label14.Name = "label14";
         label14.Size = new Size(66, 15);
         label14.TabIndex = 6;
         label14.Text = "Bildformat:";
         // 
         // label13
         // 
         label13.AutoSize = true;
         label13.Location = new Point(10, 91);
         label13.Margin = new Padding(4, 0, 4, 0);
         label13.Name = "label13";
         label13.Size = new Size(29, 15);
         label13.TabIndex = 4;
         label13.Text = "SRS:";
         // 
         // textBoxSRS
         // 
         textBoxSRS.Location = new Point(184, 88);
         textBoxSRS.Margin = new Padding(4, 3, 4, 3);
         textBoxSRS.Name = "textBoxSRS";
         textBoxSRS.ReadOnly = true;
         textBoxSRS.Size = new Size(128, 23);
         textBoxSRS.TabIndex = 5;
         // 
         // label12
         // 
         label12.AutoSize = true;
         label12.Location = new Point(10, 61);
         label12.Margin = new Padding(4, 0, 4, 0);
         label12.Name = "label12";
         label12.Size = new Size(48, 15);
         label12.TabIndex = 2;
         label12.Text = "Version:";
         // 
         // textBoxVersion
         // 
         textBoxVersion.Location = new Point(184, 58);
         textBoxVersion.Margin = new Padding(4, 3, 4, 3);
         textBoxVersion.Name = "textBoxVersion";
         textBoxVersion.ReadOnly = true;
         textBoxVersion.Size = new Size(128, 23);
         textBoxVersion.TabIndex = 3;
         // 
         // label16
         // 
         label16.AutoSize = true;
         label16.Location = new Point(12, 31);
         label16.Margin = new Padding(4, 0, 4, 0);
         label16.Name = "label16";
         label16.Size = new Size(31, 15);
         label16.TabIndex = 0;
         label16.Text = "URL:";
         // 
         // textBoxUrl
         // 
         textBoxUrl.Location = new Point(184, 28);
         textBoxUrl.Margin = new Padding(4, 3, 4, 3);
         textBoxUrl.Name = "textBoxUrl";
         textBoxUrl.ReadOnly = true;
         textBoxUrl.Size = new Size(405, 23);
         textBoxUrl.TabIndex = 1;
         // 
         // buttonSave
         // 
         buttonSave.DialogResult = DialogResult.OK;
         buttonSave.Image = Properties.Resources.speichern;
         buttonSave.ImageAlign = ContentAlignment.MiddleLeft;
         buttonSave.Location = new Point(208, 504);
         buttonSave.Margin = new Padding(4, 3, 4, 3);
         buttonSave.Name = "buttonSave";
         buttonSave.Size = new Size(96, 27);
         buttonSave.TabIndex = 2;
         buttonSave.Text = "speichern";
         buttonSave.TextAlign = ContentAlignment.MiddleRight;
         buttonSave.UseVisualStyleBackColor = true;
         buttonSave.Click += buttonSave_Click;
         // 
         // openFileDialog1
         // 
         openFileDialog1.FileName = "openFileDialog1";
         // 
         // label18
         // 
         label18.AutoSize = true;
         label18.Location = new Point(7, 119);
         label18.Margin = new Padding(4, 0, 4, 0);
         label18.Name = "label18";
         label18.Size = new Size(70, 15);
         label18.TabIndex = 10;
         label18.Text = "Hillshading:";
         // 
         // checkBoxHillShading
         // 
         checkBoxHillShading.AutoSize = true;
         checkBoxHillShading.Location = new Point(196, 119);
         checkBoxHillShading.Margin = new Padding(4, 3, 4, 3);
         checkBoxHillShading.Name = "checkBoxHillShading";
         checkBoxHillShading.Size = new Size(15, 14);
         checkBoxHillShading.TabIndex = 11;
         checkBoxHillShading.UseVisualStyleBackColor = true;
         // 
         // label19
         // 
         label19.AutoSize = true;
         label19.Location = new Point(8, 144);
         label19.Margin = new Padding(4, 0, 4, 0);
         label19.Name = "label19";
         label19.Size = new Size(99, 15);
         label19.TabIndex = 12;
         label19.Text = "Hillshadingalpha:";
         // 
         // numericUpDownHillShadingAlpha
         // 
         numericUpDownHillShadingAlpha.Location = new Point(195, 142);
         numericUpDownHillShadingAlpha.Margin = new Padding(4, 3, 4, 3);
         numericUpDownHillShadingAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
         numericUpDownHillShadingAlpha.Name = "numericUpDownHillShadingAlpha";
         numericUpDownHillShadingAlpha.Size = new Size(64, 23);
         numericUpDownHillShadingAlpha.TabIndex = 13;
         numericUpDownHillShadingAlpha.Value = new decimal(new int[] { 100, 0, 0, 0 });
         // 
         // groupBoxMulti
         // 
         groupBoxMulti.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         groupBoxMulti.Controls.Add(buttonMultiMapAdd);
         groupBoxMulti.Controls.Add(buttonMultiMapDelete);
         groupBoxMulti.Controls.Add(buttonMultiMapDown);
         groupBoxMulti.Controls.Add(buttonMultiMapUp);
         groupBoxMulti.Controls.Add(listBoxMaps);
         groupBoxMulti.Location = new Point(5, 6);
         groupBoxMulti.Name = "groupBoxMulti";
         groupBoxMulti.Size = new Size(598, 150);
         groupBoxMulti.TabIndex = 0;
         groupBoxMulti.TabStop = false;
         groupBoxMulti.Text = "Multilayer";
         // 
         // buttonMultiMapAdd
         // 
         buttonMultiMapAdd.Image = Properties.Resources.kopie;
         buttonMultiMapAdd.ImageAlign = ContentAlignment.MiddleLeft;
         buttonMultiMapAdd.Location = new Point(128, 109);
         buttonMultiMapAdd.Margin = new Padding(4, 3, 4, 3);
         buttonMultiMapAdd.Name = "buttonMultiMapAdd";
         buttonMultiMapAdd.Size = new Size(124, 27);
         buttonMultiMapAdd.TabIndex = 3;
         buttonMultiMapAdd.Text = "Ebene anhängen";
         buttonMultiMapAdd.TextAlign = ContentAlignment.MiddleRight;
         buttonMultiMapAdd.UseVisualStyleBackColor = true;
         buttonMultiMapAdd.Click += buttonMultiMapAdd_Click;
         // 
         // buttonMultiMapDelete
         // 
         buttonMultiMapDelete.Image = Properties.Resources.delete;
         buttonMultiMapDelete.ImageAlign = ContentAlignment.MiddleLeft;
         buttonMultiMapDelete.Location = new Point(341, 109);
         buttonMultiMapDelete.Margin = new Padding(4, 3, 4, 3);
         buttonMultiMapDelete.Name = "buttonMultiMapDelete";
         buttonMultiMapDelete.Size = new Size(124, 27);
         buttonMultiMapDelete.TabIndex = 4;
         buttonMultiMapDelete.Text = "Ebene löschen";
         buttonMultiMapDelete.TextAlign = ContentAlignment.MiddleRight;
         buttonMultiMapDelete.UseVisualStyleBackColor = true;
         buttonMultiMapDelete.Click += buttonMultiMapDelete_Click;
         // 
         // buttonMultiMapDown
         // 
         buttonMultiMapDown.Image = Properties.Resources.arrow_down;
         buttonMultiMapDown.Location = new Point(548, 65);
         buttonMultiMapDown.Name = "buttonMultiMapDown";
         buttonMultiMapDown.Size = new Size(44, 27);
         buttonMultiMapDown.TabIndex = 2;
         buttonMultiMapDown.UseVisualStyleBackColor = true;
         buttonMultiMapDown.Click += buttonMultiMapDown_Click;
         // 
         // buttonMultiMapUp
         // 
         buttonMultiMapUp.Image = Properties.Resources.arrow_up;
         buttonMultiMapUp.Location = new Point(548, 22);
         buttonMultiMapUp.Name = "buttonMultiMapUp";
         buttonMultiMapUp.Size = new Size(44, 27);
         buttonMultiMapUp.TabIndex = 1;
         buttonMultiMapUp.UseVisualStyleBackColor = true;
         buttonMultiMapUp.Click += buttonMultiMapUp_Click;
         // 
         // listBoxMaps
         // 
         listBoxMaps.FormattingEnabled = true;
         listBoxMaps.IntegralHeight = false;
         listBoxMaps.Location = new Point(10, 22);
         listBoxMaps.Name = "listBoxMaps";
         listBoxMaps.Size = new Size(532, 70);
         listBoxMaps.TabIndex = 0;
         listBoxMaps.SelectedIndexChanged += listBoxMaps_SelectedIndexChanged;
         listBoxMaps.DoubleClick += listBoxMaps_DoubleClick;
         // 
         // groupBox1
         // 
         groupBox1.Controls.Add(label5);
         groupBox1.Controls.Add(label1);
         groupBox1.Controls.Add(numericUpDownHillShadingAlpha);
         groupBox1.Controls.Add(label2);
         groupBox1.Controls.Add(label19);
         groupBox1.Controls.Add(checkBoxHillShading);
         groupBox1.Controls.Add(numericUpDownMinZoom);
         groupBox1.Controls.Add(numericUpDownMaxZoom);
         groupBox1.Controls.Add(label18);
         groupBox1.Controls.Add(label4);
         groupBox1.Controls.Add(comboBoxProvider);
         groupBox1.Controls.Add(textBoxMapName);
         groupBox1.Location = new Point(5, 12);
         groupBox1.Name = "groupBox1";
         groupBox1.Size = new Size(617, 180);
         groupBox1.TabIndex = 0;
         groupBox1.TabStop = false;
         groupBox1.Text = "allgemein";
         // 
         // tabControlExtended
         // 
         tabControlExtended.Controls.Add(tabPageKmz);
         tabControlExtended.Controls.Add(tabPageGarmin);
         tabControlExtended.Controls.Add(tabPageWMS);
         tabControlExtended.Controls.Add(tabPageMulti);
         tabControlExtended.Controls.Add(tabPageEmpty);
         tabControlExtended.Location = new Point(5, 198);
         tabControlExtended.Name = "tabControlExtended";
         tabControlExtended.SelectedIndex = 0;
         tabControlExtended.Size = new Size(617, 288);
         tabControlExtended.TabIndex = 1;
         tabControlExtended.Selecting += tabControlExtended_Selecting;
         // 
         // tabPageKmz
         // 
         tabPageKmz.Controls.Add(groupBoxGarminKMZ);
         tabPageKmz.Location = new Point(4, 24);
         tabPageKmz.Name = "tabPageKmz";
         tabPageKmz.Padding = new Padding(3);
         tabPageKmz.Size = new Size(609, 260);
         tabPageKmz.TabIndex = 0;
         tabPageKmz.Text = "KMZ";
         tabPageKmz.UseVisualStyleBackColor = true;
         // 
         // tabPageGarmin
         // 
         tabPageGarmin.Controls.Add(groupBoxGarmin);
         tabPageGarmin.Location = new Point(4, 24);
         tabPageGarmin.Name = "tabPageGarmin";
         tabPageGarmin.Padding = new Padding(3);
         tabPageGarmin.Size = new Size(609, 233);
         tabPageGarmin.TabIndex = 1;
         tabPageGarmin.Text = "Garmin";
         tabPageGarmin.UseVisualStyleBackColor = true;
         // 
         // tabPageWMS
         // 
         tabPageWMS.Controls.Add(groupBoxWMS);
         tabPageWMS.Location = new Point(4, 24);
         tabPageWMS.Name = "tabPageWMS";
         tabPageWMS.Size = new Size(609, 233);
         tabPageWMS.TabIndex = 2;
         tabPageWMS.Text = "WMS";
         tabPageWMS.UseVisualStyleBackColor = true;
         // 
         // tabPageMulti
         // 
         tabPageMulti.Controls.Add(groupBoxMulti);
         tabPageMulti.Location = new Point(4, 24);
         tabPageMulti.Name = "tabPageMulti";
         tabPageMulti.Size = new Size(609, 233);
         tabPageMulti.TabIndex = 3;
         tabPageMulti.Text = "Multimap";
         tabPageMulti.UseVisualStyleBackColor = true;
         // 
         // tabPageEmpty
         // 
         tabPageEmpty.Location = new Point(4, 24);
         tabPageEmpty.Name = "tabPageEmpty";
         tabPageEmpty.Size = new Size(609, 233);
         tabPageEmpty.TabIndex = 4;
         tabPageEmpty.Text = "-";
         tabPageEmpty.UseVisualStyleBackColor = true;
         // 
         // buttonCancel
         // 
         buttonCancel.DialogResult = DialogResult.Cancel;
         buttonCancel.Image = Properties.Resources.cancel;
         buttonCancel.ImageAlign = ContentAlignment.MiddleLeft;
         buttonCancel.Location = new Point(329, 504);
         buttonCancel.Name = "buttonCancel";
         buttonCancel.Size = new Size(97, 27);
         buttonCancel.TabIndex = 3;
         buttonCancel.Text = "abbrechen";
         buttonCancel.TextAlign = ContentAlignment.MiddleRight;
         buttonCancel.UseVisualStyleBackColor = true;
         // 
         // FormMapProviderDefinitionEdit
         // 
         AcceptButton = buttonSave;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = buttonCancel;
         ClientSize = new Size(630, 543);
         ControlBox = false;
         Controls.Add(buttonCancel);
         Controls.Add(tabControlExtended);
         Controls.Add(groupBox1);
         Controls.Add(buttonSave);
         FormBorderStyle = FormBorderStyle.FixedDialog;
         Margin = new Padding(4, 3, 4, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormMapProviderDefinitionEdit";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Kartendefinition";
         FormClosing += FormMapProviderDefinitionEdit_FormClosing;
         Load += FormMapProviderDefinitionEdit_Load;
         ((System.ComponentModel.ISupportInitialize)numericUpDownMinZoom).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownMaxZoom).EndInit();
         groupBoxGarminKMZ.ResumeLayout(false);
         groupBoxGarminKMZ.PerformLayout();
         groupBoxGarmin.ResumeLayout(false);
         groupBoxGarmin.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)numericUpDownLineFactor).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownSymbolFactor).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownTextFactor).EndInit();
         groupBoxWMS.ResumeLayout(false);
         groupBoxWMS.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)numericUpDownHillShadingAlpha).EndInit();
         groupBoxMulti.ResumeLayout(false);
         groupBox1.ResumeLayout(false);
         groupBox1.PerformLayout();
         tabControlExtended.ResumeLayout(false);
         tabPageKmz.ResumeLayout(false);
         tabPageGarmin.ResumeLayout(false);
         tabPageWMS.ResumeLayout(false);
         tabPageMulti.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.NumericUpDown numericUpDownMinZoom;
      private System.Windows.Forms.NumericUpDown numericUpDownMaxZoom;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox textBoxMapName;
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
      private GroupBox groupBoxMulti;
      private ListBox listBoxMaps;
      private Button buttonMultiMapAdd;
      private Button buttonMultiMapDelete;
      private Button buttonMultiMapDown;
      private Button buttonMultiMapUp;
      private GroupBox groupBox1;
      private TabControl tabControlExtended;
      private TabPage tabPageKmz;
      private TabPage tabPageGarmin;
      private TabPage tabPageWMS;
      private TabPage tabPageMulti;
      private TabPage tabPageEmpty;
      private Button buttonCancel;
   }
}