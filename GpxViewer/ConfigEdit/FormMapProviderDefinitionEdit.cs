using GMap.NET.CoreExt.MapProviders;
using System;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer.ConfigEdit {
   public partial class FormMapProviderDefinitionEdit : Form {

      public MapProviderDefinition MapProviderDefinition;

      public bool IsNewMapProviderDefinition = false;

      bool canSave = true;


      public FormMapProviderDefinitionEdit() {
         InitializeComponent();
      }

      private void FormMapProviderDefinitionEdit_Load(object sender, EventArgs e) {
         comboBoxProvider.Items.Add(GarminProvider.Instance.Name);
         comboBoxProvider.Items.Add(GarminKmzProvider.Instance.Name);
         comboBoxProvider.Items.Add(WMSProvider.Instance.Name);
         foreach (var item in GMap.NET.MapProviders.GMapProviders.List) {
            comboBoxProvider.Items.Add(item.Name);
         }

         comboBoxProvider.Text = MapProviderDefinition.ProviderName;
         textBoxMapName.Text = MapProviderDefinition.MapName;
         numericUpDownMinZoom.Value = MapProviderDefinition.MinZoom;
         numericUpDownMaxZoom.Value = MapProviderDefinition.MaxZoom;
         numericUpDownZoom4Display.Value = (decimal)MapProviderDefinition.Zoom4Display;

         comboBoxProvider.Enabled = IsNewMapProviderDefinition;
         textBoxMapName.ReadOnly = false; // !IsNewMapProviderDefinition;

         checkBoxHillShading.Enabled = false;
         numericUpDownHillShadingAlpha.Enabled = false;

         if (MapProviderDefinition is GarminProvider.GarminMapDefinitionData) {

            GarminProvider.GarminMapDefinitionData specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinitionData;
            setText(textBoxTdbFile, specmpd.TDBfile[0]);
            setText(textBoxTypFile, specmpd.TYPfile[0]);
            numericUpDownTextFactor.Value = (decimal)specmpd.TextFactor;
            numericUpDownSymbolFactor.Value = (decimal)specmpd.SymbolFactor;
            numericUpDownLineFactor.Value = (decimal)specmpd.LineFactor;
            checkBoxHillShading.Checked = specmpd.HillShading;
            numericUpDownHillShadingAlpha.Value = specmpd.HillShadingAlpha;
            numericUpDownTextFactor.Enabled =
            numericUpDownSymbolFactor.Enabled =
            numericUpDownLineFactor.Enabled =
            numericUpDownHillShadingAlpha.Enabled = true;
            checkBoxHillShading.Enabled = true;
            buttonOpenTdbFile.Enabled =
            buttonOpenTypFile.Enabled = true;

         } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

            GarminKmzProvider.KmzMapDefinition specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
            setText(textBoxKmzFile, specmpd.KmzFile);
            checkBoxHillShading.Checked = specmpd.HillShading;
            numericUpDownHillShadingAlpha.Value = specmpd.HillShadingAlpha;
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
            buttonOpenKmzFile.Enabled = true;

         } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

            WMSProvider.WMSMapDefinition specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
            setText(textBoxUrl, specmpd.URL);
            setText(textBoxVersion, specmpd.Version);
            setText(textBoxSRS, specmpd.SRS);
            comboBoxPictureFormat.Text = specmpd.PictureFormat;
            setText(textBoxLayer, specmpd.Layer);
            setText(textBoxExtendedParams, specmpd.ExtendedParameters);
            textBoxUrl.ReadOnly =
            textBoxVersion.ReadOnly =
            textBoxSRS.ReadOnly =
            textBoxLayer.ReadOnly =
            textBoxExtendedParams.ReadOnly = false;
            comboBoxPictureFormat.Enabled = true;

         }
      }

      void setText(TextBox tb, string text) {
         tb.Text = text;
         tb.SelectionStart = tb.Text.Length;
      }

      private void comboBoxProvider_SelectedIndexChanged(object sender, EventArgs e) {
         string providername = (sender as ComboBox).Text;
         groupBoxGarmin.Enabled =
         groupBoxGarminKMZ.Enabled =
         groupBoxWMS.Enabled = false;
         checkBoxHillShading.Enabled =
         numericUpDownHillShadingAlpha.Enabled = false;
         if (providername == GarminProvider.Instance.Name) {
            groupBoxGarmin.Enabled = true;
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
         } else if (providername == GarminKmzProvider.Instance.Name) {
            groupBoxGarminKMZ.Enabled = true;
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
         } else if (providername == WMSProvider.Instance.Name) {
            groupBoxWMS.Enabled = true;
         } else {

         }
      }

      private void buttonOpenKmzFile_Click(object sender, EventArgs e) {
         openFileDialog1.Filter = "KMZ-Dateien|*.kmz";
         openFileDialog1.DefaultExt = ".kmz";
         openFileDialog1.FileName = textBoxKmzFile.Text;
         if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openFileDialog1.FileName));
         if (openFileDialog1.ShowDialog() == DialogResult.OK)
            setText(textBoxKmzFile, openFileDialog1.FileName);
      }

      private void buttonOpenTdbFile_Click(object sender, EventArgs e) {
         openFileDialog1.Filter = "TDB-Dateien|*.tdb";
         openFileDialog1.DefaultExt = ".tdb";
         openFileDialog1.FileName = textBoxTdbFile.Text;
         if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openFileDialog1.FileName));
         if (openFileDialog1.ShowDialog() == DialogResult.OK)
            setText(textBoxTdbFile, openFileDialog1.FileName);
      }

      private void buttonOpenTypFile_Click(object sender, EventArgs e) {
         openFileDialog1.Filter = "TYP-Dateien|*.typ";
         openFileDialog1.DefaultExt = ".typ";
         openFileDialog1.FileName = textBoxTypFile.Text;
         if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openFileDialog1.FileName));
         if (openFileDialog1.ShowDialog() == DialogResult.OK)
            setText(textBoxTypFile, openFileDialog1.FileName);
      }

      void showError(string message) {
         MessageBox.Show(message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
         canSave = false;
      }

      bool hasText(Control ctrl) =>
          !string.IsNullOrEmpty(ctrl.Text) && ctrl.Text.Trim().Length > 0;

      private void buttonSave_Click(object sender, EventArgs e) {
         canSave = true;

         if (!hasText(comboBoxProvider)) {
            showError("Ein Kartenprovider muss ausgewählt sein.");
            return;
         }
         if (!hasText(textBoxMapName)) {
            showError("Ein Kartenname muss angegeben sein.");
            return;
         }

         string providername = comboBoxProvider.Text;

         if (providername == GarminProvider.Instance.Name) {
            if (!hasText(textBoxTdbFile)) {
               showError("Eine TDB-Datei muss angegeben sein.");
               return;
            }
            if (!hasText(textBoxTypFile)) {
               showError("Eine TYP-Datei muss angegeben sein.");
               return;
            }
         } else if (providername == GarminKmzProvider.Instance.Name) {
            if (!hasText(textBoxKmzFile)) {
               showError("Eine KMZ-Datei muss angegeben sein.");
               return;
            }
         } else if (providername == WMSProvider.Instance.Name) {
            if (!hasText(textBoxUrl)) {
               showError("Eine URL muss angegeben sein.");
               return;
            }
            if (!hasText(textBoxSRS)) {
               showError("Eine SRS (Koordinatensystem) muss angegeben sein.");
               return;
            }
            if (!hasText(textBoxVersion)) {
               showError("Eine WMS-Version muss angegeben sein.");
               return;
            }
         }

          // ACHTUNG!  Wenn bestimmte Daten geändert werden muss DbIdDelta neu ermittelt werden, d.h. eine neue Def. ist nötig!
         if (!IsNewMapProviderDefinition) {
            if (providername == GarminProvider.Instance.Name) {
               if (textBoxMapName.Text.Trim() != MapProviderDefinition.MapName ||
                   textBoxTdbFile.Text.Trim() != (MapProviderDefinition as GarminProvider.GarminMapDefinitionData).TDBfile[0] ||
                   textBoxTypFile.Text.Trim() != (MapProviderDefinition as GarminProvider.GarminMapDefinitionData).TYPfile[0]) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == GarminKmzProvider.Instance.Name) {
               if (textBoxMapName.Text.Trim() != MapProviderDefinition.MapName ||
                   textBoxKmzFile.Text.Trim() != (MapProviderDefinition as GarminKmzProvider.KmzMapDefinition).KmzFile) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == WMSProvider.Instance.Name) {
               if (textBoxMapName.Text.Trim() != MapProviderDefinition.MapName ||
                   textBoxLayer.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Layer ||
                   textBoxUrl.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).URL ||
                   textBoxSRS.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).SRS ||
                   textBoxVersion.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Version ||
                   comboBoxPictureFormat.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).PictureFormat ||
                   textBoxExtendedParams.Text.Trim() != (MapProviderDefinition as WMSProvider.WMSMapDefinition).ExtendedParameters) {
                  IsNewMapProviderDefinition = true;
               }
            }
         }

         // Übernahme der Werte nach MapProviderDefinition

         if (IsNewMapProviderDefinition) {
           if (providername == GarminProvider.Instance.Name) {
               MapProviderDefinition = new GarminProvider.GarminMapDefinitionData(
                                                textBoxMapName.Text.Trim(),
                                                (double)numericUpDownZoom4Display.Value,
                                                (int)numericUpDownMinZoom.Value,
                                                (int)numericUpDownMaxZoom.Value,
                                                new string[] {
                                                   textBoxTdbFile.Text.Trim(),
                                                },
                                                new string[] {
                                                   textBoxTypFile.Text.Trim(),
                                                },
                                                (double)numericUpDownTextFactor.Value,
                                                (double)numericUpDownLineFactor.Value,
                                                (double)numericUpDownSymbolFactor.Value,
                                                checkBoxHillShading.Checked,
                                                (byte)numericUpDownHillShadingAlpha.Value);
            } else if (providername == GarminKmzProvider.Instance.Name) {
               MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(
                                                textBoxMapName.Text.Trim(),
                                                (double)numericUpDownZoom4Display.Value,
                                                (int)numericUpDownMinZoom.Value,
                                                (int)numericUpDownMaxZoom.Value,
                                                textBoxKmzFile.Text.Trim(),
                                                checkBoxHillShading.Checked,
                                                (byte)numericUpDownHillShadingAlpha.Value);
            } else if (providername == WMSProvider.Instance.Name) {
               MapProviderDefinition = new WMSProvider.WMSMapDefinition(
                                                textBoxMapName.Text,
                                                (double)numericUpDownZoom4Display.Value,
                                                (int)numericUpDownMinZoom.Value,
                                                (int)numericUpDownMaxZoom.Value,
                                                textBoxLayer.Text.Trim(),
                                                textBoxUrl.Text.Trim(),
                                                textBoxSRS.Text.Trim(),
                                                textBoxVersion.Text.Trim(),
                                                comboBoxPictureFormat.Text.Trim(),
                                                textBoxExtendedParams.Text.Trim());
            } else {
               MapProviderDefinition = new MapProviderDefinition();
               MapProviderDefinition.ProviderName = comboBoxProvider.Text;
               MapProviderDefinition.MapName = textBoxMapName.Text.Trim();
               MapProviderDefinition.MinZoom = (int)numericUpDownMinZoom.Value;
               MapProviderDefinition.MaxZoom = (int)numericUpDownMaxZoom.Value;
               MapProviderDefinition.Zoom4Display = (double)numericUpDownZoom4Display.Value;
            }

         } else {

            if (MapProviderDefinition is GarminProvider.GarminMapDefinitionData) {

               GarminProvider.GarminMapDefinitionData specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinitionData;
               specmpd.TDBfile[0] = textBoxTdbFile.Text.Trim();
               specmpd.TYPfile[0] = textBoxTypFile.Text.Trim();
               specmpd.TextFactor = (double)numericUpDownTextFactor.Value;
               specmpd.SymbolFactor = (double)numericUpDownSymbolFactor.Value;
               specmpd.LineFactor = (double)numericUpDownLineFactor.Value;
               specmpd.HillShading = checkBoxHillShading.Checked;
               specmpd.HillShadingAlpha = (byte)numericUpDownHillShadingAlpha.Value;

            } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

               GarminKmzProvider.KmzMapDefinition specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
               specmpd.KmzFile = textBoxKmzFile.Text.Trim();
               specmpd.HillShading = checkBoxHillShading.Checked;
               specmpd.HillShadingAlpha = (byte)numericUpDownHillShadingAlpha.Value;

            } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

               WMSProvider.WMSMapDefinition specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
               specmpd.URL = textBoxUrl.Text.Trim();
               specmpd.Version = textBoxVersion.Text.Trim();
               specmpd.SRS = textBoxSRS.Text.Trim();
               specmpd.PictureFormat = comboBoxPictureFormat.Text.Trim();
               specmpd.Layer = textBoxLayer.Text.Trim();
               specmpd.ExtendedParameters = textBoxExtendedParams.Text.Trim();

            }

            MapProviderDefinition.ProviderName = comboBoxProvider.Text;
            MapProviderDefinition.MapName = textBoxMapName.Text.Trim();
            MapProviderDefinition.MinZoom = (int)numericUpDownMinZoom.Value;
            MapProviderDefinition.MaxZoom = (int)numericUpDownMaxZoom.Value;
            MapProviderDefinition.Zoom4Display = (double)numericUpDownZoom4Display.Value;

         }
      }

      private void FormMapProviderDefinitionEdit_FormClosing(object sender, FormClosingEventArgs e) {
         e.Cancel = !canSave;
      }
   }
}
