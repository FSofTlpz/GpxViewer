using GMap.NET.FSofTExtented.MapProviders;

namespace GpxViewer.ConfigEdit {
   public partial class FormMapProviderDefinitionEdit : Form {

      public MapProviderDefinition? MapProviderDefinition;

      /// <summary>
      /// Neue Karte oder Bearbeitung einer bestehenden Karte?
      /// </summary>
      public bool IsNewMapProviderDefinition = false;

      /// <summary>
      /// Karte für einen <see cref="MultiMapProvider"/>?
      /// </summary>
      public bool IsSubLayer = false;

      MultiMapProvider.MultiMapDefinition? multimapDef = null;

      bool isSaved = false;


      public FormMapProviderDefinitionEdit() {
         InitializeComponent();
      }

      private void FormMapProviderDefinitionEdit_Load(object sender, EventArgs e) {
         if (MapProviderDefinition != null)
            init(MapProviderDefinition);
      }

      private void FormMapProviderDefinitionEdit_FormClosing(object sender, FormClosingEventArgs e) {
         if (DialogResult == DialogResult.OK)
            e.Cancel = !isSaved;
      }

      void init(MapProviderDefinition definition) {
         // nur Kopie der Kartendef. anlegen
         if (definition is GarminProvider.GarminMapDefinition)
            MapProviderDefinition = new GarminProvider.GarminMapDefinition(definition as GarminProvider.GarminMapDefinition);
         else if (definition is GarminKmzProvider.KmzMapDefinition)
            MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(definition as GarminKmzProvider.KmzMapDefinition);
         else if (definition is WMSProvider.WMSMapDefinition)
            MapProviderDefinition = new WMSProvider.WMSMapDefinition(definition as WMSProvider.WMSMapDefinition);
         else if (definition is HillshadingProvider.HillshadingMapDefinition)
            MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(definition as HillshadingProvider.HillshadingMapDefinition);
         else if (definition is MultiMapProvider.MultiMapDefinition)
            MapProviderDefinition = new MultiMapProvider.MultiMapDefinition(definition as MultiMapProvider.MultiMapDefinition);
         else
            MapProviderDefinition = new MapProviderDefinition(definition);

         comboBoxProvider.Items.Add(GarminProvider.Instance.Name);
         comboBoxProvider.Items.Add(GarminKmzProvider.Instance.Name);
         comboBoxProvider.Items.Add(WMSProvider.Instance.Name);
         if (IsSubLayer)
            comboBoxProvider.Items.Add(HillshadingProvider.Instance.Name);
         if (!IsSubLayer)
            comboBoxProvider.Items.Add(MultiMapProvider.Instance.Name);
         foreach (var item in GMap.NET.MapProviders.GMapProviders.List)
            comboBoxProvider.Items.Add(item.Name);

         comboBoxProvider.Text = MapProviderDefinition.ProviderName;
         comboBoxProvider.Enabled = IsNewMapProviderDefinition;

         setText(textBoxMapName, MapProviderDefinition.MapName);
         setValue(numericUpDownMinZoom, MapProviderDefinition.MinZoom);
         setValue(numericUpDownMaxZoom, MapProviderDefinition.MaxZoom);

         if (IsSubLayer) {
            numericUpDownMinZoom.Enabled =
            numericUpDownMaxZoom.Enabled = false;
            setText(textBoxMapName, string.Empty);
         }

         // i.A. ohne Hillshading
         checkBoxHillShading.Enabled =
         numericUpDownHillShadingAlpha.Enabled = false;

         tabControlExtended_Selecting_Intern = true;
         tabControlExtended.SelectedTab = tabPageEmpty;

         if (MapProviderDefinition is GarminProvider.GarminMapDefinition) {

            GarminProvider.GarminMapDefinition? specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinition;
            if (specmpd != null) {
               setText(textBoxTdbFile, specmpd.TDBfile[0]);
               setText(textBoxTypFile, specmpd.TYPfile[0]);
               setValue(numericUpDownTextFactor, IsNewMapProviderDefinition ? 1 : specmpd.TextFactor);
               setValue(numericUpDownSymbolFactor, IsNewMapProviderDefinition ? 1 : specmpd.SymbolFactor);
               setValue(numericUpDownLineFactor, IsNewMapProviderDefinition ? 1 : specmpd.LineFactor);
               checkBoxHillShading.Checked = IsNewMapProviderDefinition ? false : specmpd.HillShading;
               setValue(numericUpDownHillShadingAlpha, IsNewMapProviderDefinition ? 100 : specmpd.HillShadingAlpha);
            }
            numericUpDownHillShadingAlpha.Enabled =
            checkBoxHillShading.Enabled = true;
            tabControlExtended.SelectedTab = tabPageGarmin;

         } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

            GarminKmzProvider.KmzMapDefinition? specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
            if (specmpd != null) {
               setText(textBoxKmzFile, specmpd.KmzFile);
               checkBoxHillShading.Checked = IsNewMapProviderDefinition ? false : specmpd.HillShading;
               setValue(numericUpDownHillShadingAlpha, IsNewMapProviderDefinition ? 100 : specmpd.HillShadingAlpha);
            }
            numericUpDownHillShadingAlpha.Enabled =
            checkBoxHillShading.Enabled = true;
            tabControlExtended.SelectedTab = tabPageKmz;

         } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

            WMSProvider.WMSMapDefinition? specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
            if (specmpd != null) {
               setText(textBoxUrl, specmpd.URL);
               setText(textBoxVersion, specmpd.Version);
               setText(textBoxSRS, specmpd.SRS);
               comboBoxPictureFormat.Text = specmpd.PictureFormat;
               setText(textBoxLayer, specmpd.Layer);
               setText(textBoxExtendedParams, specmpd.ExtendedParameters);
               checkBoxHillShading.Checked = IsNewMapProviderDefinition ? false : specmpd.HillShading;
               setValue(numericUpDownHillShadingAlpha, IsNewMapProviderDefinition ? 100 : specmpd.HillShadingAlpha);
            }
            numericUpDownHillShadingAlpha.Enabled =
            checkBoxHillShading.Enabled = true;
            tabControlExtended.SelectedTab = tabPageWMS;

         } else if (MapProviderDefinition is HillshadingProvider.HillshadingMapDefinition) {

            HillshadingProvider.HillshadingMapDefinition? specmpd = MapProviderDefinition as HillshadingProvider.HillshadingMapDefinition;
            if (specmpd != null)
               setValue(numericUpDownHillShadingAlpha, IsNewMapProviderDefinition ? 100 : specmpd.Alpha);
            numericUpDownHillShadingAlpha.Enabled = true;
            checkBoxHillShading.Checked = true;

         } else if (MapProviderDefinition is MultiMapProvider.MultiMapDefinition) {

            multimapDef = MapProviderDefinition as MultiMapProvider.MultiMapDefinition;
            if (multimapDef != null) {
               for (int i = 0; i < multimapDef.MapProviderDefinitions.Length; i++)
                  listBoxMaps.Items.Add(item4ListBoxMaps(multimapDef.MapProviderDefinitions[i]));
               if (multimapDef.MapProviderDefinitions.Length > 0)
                  listBoxMaps.SelectedIndex = 0;
               else {
                  buttonMultiMapDelete.Enabled = false;
                  buttonMultiMapDown.Enabled = false;
                  buttonMultiMapUp.Enabled = false;
               }
            }
            tabControlExtended.SelectedTab = tabPageMulti;

         }
         tabControlExtended_Selecting_Intern = false;
      }

      private void comboBoxProvider_SelectedIndexChanged(object sender, EventArgs e) {
         string providername = (sender as ComboBox).Text;
         checkBoxHillShading.Enabled =
         numericUpDownHillShadingAlpha.Enabled = false;
         numericUpDownHillShadingAlpha.Value = 0;

         tabControlExtended_Selecting_Intern = true;
         tabControlExtended.SelectedTab = tabPageEmpty;
         if (providername == GarminProvider.Instance.Name) {
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
            tabControlExtended.SelectedTab = tabPageGarmin;
         } else if (providername == GarminKmzProvider.Instance.Name) {
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
            tabControlExtended.SelectedTab = tabPageKmz;
         } else if (providername == WMSProvider.Instance.Name) {
            checkBoxHillShading.Enabled = true;
            numericUpDownHillShadingAlpha.Enabled = true;
            tabControlExtended.SelectedTab = tabPageWMS;
         } else if (providername == HillshadingProvider.Instance.Name) {
            checkBoxHillShading.Checked = true;
            numericUpDownHillShadingAlpha.Enabled = true;
         } else if (providername == MultiMapProvider.Instance.Name) {
            tabControlExtended.SelectedTab = tabPageMulti;
            if (IsNewMapProviderDefinition) {
               string mapname = MapProviderDefinition != null ? MapProviderDefinition.MapName : "neu Karte";
               MapProviderDefinition = multimapDef = new MultiMapProvider.MultiMapDefinition(
                                                            mapname,
                                                            MapProviderDefinition != null ? MapProviderDefinition.MinZoom : 10,
                                                            MapProviderDefinition != null ? MapProviderDefinition.MaxZoom : 24,
                                                            []);
            }
         }
         tabControlExtended_Selecting_Intern = false;
      }

      private void buttonSave_Click(object sender, EventArgs e) {
         if (!hasText(comboBoxProvider)) {
            showError("Ein Kartenprovider muss ausgewählt sein.");
            return;
         }
         if (!hasText(textBoxMapName) &&
             !IsSubLayer) {
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
            if (getBool(checkBoxHillShading) && getInt(numericUpDownHillShadingAlpha) <= 0) {
               showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == GarminKmzProvider.Instance.Name) {
            if (!hasText(textBoxKmzFile)) {
               showError("Eine KMZ-Datei muss angegeben sein.");
               return;
            }
            if (getBool(checkBoxHillShading) && getInt(numericUpDownHillShadingAlpha) <= 0) {
               showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
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
            if (getBool(checkBoxHillShading) && getInt(numericUpDownHillShadingAlpha) <= 0) {
               showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == HillshadingProvider.Instance.Name) {
            if (getInt(numericUpDownHillShadingAlpha) <= 0) {
               showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == MultiMapProvider.Instance.Name) {
            if (listBoxMaps.Items.Count == 0) {
               showError("Mindestens 1 Ebene muss angegeben sein.");
               return;
            }
         }

         // ACHTUNG!  Wenn bestimmte Daten geändert werden muss DbIdDelta neu ermittelt werden, d.h. eine neue Def. ist nötig!
         if (!IsNewMapProviderDefinition) {
            if (providername == GarminProvider.Instance.Name) {
               if (getText(textBoxMapName) != MapProviderDefinition.MapName ||
                   getText(textBoxTdbFile) != (MapProviderDefinition as GarminProvider.GarminMapDefinition).TDBfile[0] ||
                   getText(textBoxTypFile) != (MapProviderDefinition as GarminProvider.GarminMapDefinition).TYPfile[0]) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == GarminKmzProvider.Instance.Name) {
               if (getText(textBoxMapName) != MapProviderDefinition.MapName ||
                   getText(textBoxKmzFile) != (MapProviderDefinition as GarminKmzProvider.KmzMapDefinition).KmzFile) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == WMSProvider.Instance.Name) {
               if (getText(textBoxMapName) != MapProviderDefinition.MapName ||
                   getText(textBoxLayer) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Layer ||
                   getText(textBoxUrl) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).URL ||
                   getText(textBoxSRS) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).SRS ||
                   getText(textBoxVersion) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Version ||
                   getText(comboBoxPictureFormat) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).PictureFormat ||
                   getText(textBoxExtendedParams) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).ExtendedParameters) {
                  IsNewMapProviderDefinition = true;
               }
            }
         }

         // Übernahme der Werte nach MapProviderDefinition

         if (IsNewMapProviderDefinition) {
            if (providername == GarminProvider.Instance.Name) {
               MapProviderDefinition = new GarminProvider.GarminMapDefinition(
                                                getText(textBoxMapName),
                                                getInt(numericUpDownMinZoom),
                                                getInt(numericUpDownMaxZoom),
                                                [getText(textBoxTdbFile),],
                                                [getText(textBoxTypFile),],
                                                getDouble(numericUpDownTextFactor),
                                                getDouble(numericUpDownLineFactor),
                                                getDouble(numericUpDownSymbolFactor),
                                                getBool(checkBoxHillShading),
                                                getByte(numericUpDownHillShadingAlpha));
            } else if (providername == GarminKmzProvider.Instance.Name) {
               MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(
                                                getText(textBoxMapName),
                                                getInt(numericUpDownMinZoom),
                                                getInt(numericUpDownMaxZoom),
                                                getText(textBoxKmzFile),
                                                getBool(checkBoxHillShading),
                                                getByte(numericUpDownHillShadingAlpha));
            } else if (providername == WMSProvider.Instance.Name) {
               MapProviderDefinition = new WMSProvider.WMSMapDefinition(
                                                textBoxMapName.Text,
                                                getInt(numericUpDownMinZoom),
                                                getInt(numericUpDownMaxZoom),
                                                getText(textBoxLayer),
                                                getText(textBoxUrl),
                                                getText(textBoxSRS),
                                                getText(textBoxVersion),
                                                getText(comboBoxPictureFormat),
                                                getText(textBoxExtendedParams),
                                                getBool(checkBoxHillShading),
                                                getByte(numericUpDownHillShadingAlpha));
            } else if (providername == HillshadingProvider.Instance.Name) {

               MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(
                                                getText(textBoxMapName),
                                                getInt(numericUpDownMinZoom),
                                                getInt(numericUpDownMaxZoom),
                                                null,
                                                getByte(numericUpDownHillShadingAlpha));

            } else if (providername == MultiMapProvider.Instance.Name) {

               if (MapProviderDefinition != null) {
                  MultiMapProvider.MultiMapDefinition mmdef = (MultiMapProvider.MultiMapDefinition)MapProviderDefinition;
                  if (mmdef != null)
                     for (int i = 0; i < mmdef.MapProviderDefinitions.Length; i++) {
                        mmdef.MapProviderDefinitions[i].MinZoom = MapProviderDefinition.MinZoom;
                        mmdef.MapProviderDefinitions[i].MaxZoom = MapProviderDefinition.MaxZoom;
                     }
               }

            } else {
               MapProviderDefinition = new MapProviderDefinition();
               MapProviderDefinition.ProviderName = comboBoxProvider.Text;
               MapProviderDefinition.MapName = getText(textBoxMapName);
               MapProviderDefinition.MinZoom = getInt(numericUpDownMinZoom);
               MapProviderDefinition.MaxZoom = getInt(numericUpDownMaxZoom);
            }

         } else {       // nur Daten verändert

            if (MapProviderDefinition != null) {
               MapProviderDefinition.ProviderName = comboBoxProvider.Text;
               MapProviderDefinition.MapName = getText(textBoxMapName);
               MapProviderDefinition.MinZoom = getInt(numericUpDownMinZoom);
               MapProviderDefinition.MaxZoom = getInt(numericUpDownMaxZoom);

               if (MapProviderDefinition is GarminProvider.GarminMapDefinition) {

                  GarminProvider.GarminMapDefinition? specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinition;
                  if (specmpd != null) {
                     specmpd.TDBfile[0] = getText(textBoxTdbFile);
                     specmpd.TYPfile[0] = getText(textBoxTypFile);
                     specmpd.TextFactor = getDouble(numericUpDownTextFactor);
                     specmpd.SymbolFactor = getDouble(numericUpDownSymbolFactor);
                     specmpd.LineFactor = getDouble(numericUpDownLineFactor);
                     specmpd.HillShading = getBool(checkBoxHillShading);
                     specmpd.HillShadingAlpha = getByte(numericUpDownHillShadingAlpha);
                  }

               } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

                  GarminKmzProvider.KmzMapDefinition? specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
                  if (specmpd != null) {
                     specmpd.KmzFile = getText(textBoxKmzFile);
                     specmpd.HillShading = getBool(checkBoxHillShading);
                     specmpd.HillShadingAlpha = getByte(numericUpDownHillShadingAlpha);
                  }

               } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

                  WMSProvider.WMSMapDefinition? specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
                  if (specmpd != null) {
                     specmpd.URL = getText(textBoxUrl);
                     specmpd.Version = getText(textBoxVersion);
                     specmpd.SRS = getText(textBoxSRS);
                     specmpd.PictureFormat = getText(comboBoxPictureFormat);
                     specmpd.Layer = getText(textBoxLayer);
                     specmpd.ExtendedParameters = getText(textBoxExtendedParams);
                     specmpd.HillShading = getBool(checkBoxHillShading);
                     specmpd.HillShadingAlpha = getByte(numericUpDownHillShadingAlpha);
                  }

               } else if (MapProviderDefinition is HillshadingProvider.HillshadingMapDefinition) {

                  HillshadingProvider.HillshadingMapDefinition specmpd = (HillshadingProvider.HillshadingMapDefinition)MapProviderDefinition;
                  if (specmpd != null) {
                     MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(specmpd.MapName,
                                                                                              specmpd.MinZoom,
                                                                                              specmpd.MaxZoom,
                                                                                              specmpd.DEM,
                                                                                              getByte(numericUpDownHillShadingAlpha));
                  }

               } else if (MapProviderDefinition is MultiMapProvider.MultiMapDefinition) {

                  MapProviderDefinition = multimapDef;

                  //MultiMapProvider.MultiMapDefinition specmpd = (MultiMapProvider.MultiMapDefinition)MapProviderDefinition;
                  //if (specmpd != null) {


                  //}

               }

            }
         }
         isSaved = true;
      }

      #region KMZ-Karte

      private void buttonOpenKmzFile_Click(object sender, EventArgs e) {
         openFileDialog1.Filter = "KMZ-Dateien|*.kmz";
         openFileDialog1.DefaultExt = ".kmz";
         openFileDialog1.FileName = textBoxKmzFile.Text;
         if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openFileDialog1.FileName));
         if (openFileDialog1.ShowDialog() == DialogResult.OK)
            setText(textBoxKmzFile, openFileDialog1.FileName);
      }

      #endregion

      #region Garminkarte

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

      #endregion

      #region MultiMap-Karte

      private void listBoxMaps_SelectedIndexChanged(object sender, EventArgs e) {
         ListBox lb = (ListBox)sender;
         buttonMultiMapDelete.Enabled = lb.SelectedIndex >= 0;
         buttonMultiMapUp.Enabled = lb.SelectedIndex > 0;
         buttonMultiMapDown.Enabled = lb.SelectedIndex < lb.Items.Count - 1;
      }

      private void buttonMultiMapUp_Click(object sender, EventArgs e) {
         ListBox lb = listBoxMaps;
         int idx = lb.SelectedIndex;
         if (0 < idx &&
             idx < lb.Items.Count &&
             multimapDef != null) {

            object obj = lb.Items[idx];
            lb.Items.RemoveAt(idx);
            lb.Items.Insert(idx - 1, obj);
            lb.SelectedIndex = idx - 1;

            MapProviderDefinition tmpdef = multimapDef.MapProviderDefinitions[idx];
            multimapDef.MapProviderDefinitions[idx] = multimapDef.MapProviderDefinitions[idx - 1];
            multimapDef.MapProviderDefinitions[idx - 1] = tmpdef;
         }
      }

      private void buttonMultiMapDown_Click(object sender, EventArgs e) {
         ListBox lb = listBoxMaps;
         int idx = lb.SelectedIndex;
         if (0 <= idx &&
             idx < lb.Items.Count - 1 &&
             multimapDef != null) {

            object obj = lb.Items[idx];
            lb.Items.RemoveAt(idx);
            lb.Items.Insert(idx + 1, obj);
            lb.SelectedIndex = idx + 1;

            MapProviderDefinition tmpdef = multimapDef.MapProviderDefinitions[idx];
            multimapDef.MapProviderDefinitions[idx] = multimapDef.MapProviderDefinitions[idx + 1];
            multimapDef.MapProviderDefinitions[idx + 1] = tmpdef;
         }
      }

      private void buttonMultiMapDelete_Click(object sender, EventArgs e) {
         ListBox lb = listBoxMaps;
         deleteMultimap(multimapDef, lb.SelectedIndex, lb);
      }

      private void listBoxMaps_DoubleClick(object sender, EventArgs e) {
         ListBox lb = (ListBox)sender;
         editMultimap(multimapDef, lb.SelectedIndex, lb);
      }

      private void buttonMultiMapAdd_Click(object sender, EventArgs e) {
         ListBox lb = listBoxMaps;
         addMultimap(multimapDef, lb.SelectedIndex < 0 ? -1 : lb.SelectedIndex + 1, lb);
      }

      void deleteMultimap(MultiMapProvider.MultiMapDefinition? multimapdef, int idx, ListBox lb) {
         if (0 <= idx &&
             idx < lb.Items.Count &&
             multimapdef != null &&
             MessageBox.Show("Die Ebene '" + lb.Items[idx] + "' wirklich löschen?",
                             "Löschen",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question,
                             MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
            lb.Items.RemoveAt(idx);
            multimapdef.RemoveLevel(idx);
         }
      }

      void editMultimap(MultiMapProvider.MultiMapDefinition? multimapdef, int idx, ListBox lb) {
         if (0 <= idx &&
             idx < lb.Items.Count &&
             multimapdef != null) {
            MapProviderDefinition leveldef = multimapdef.MapProviderDefinitions[idx];
            if (leveldef != null) {

               leveldef.MinZoom = getInt(numericUpDownMinZoom);
               leveldef.MaxZoom = getInt(numericUpDownMaxZoom);
               FormMapProviderDefinitionEdit form = new FormMapProviderDefinitionEdit() {
                  MapProviderDefinition = leveldef,
                  IsNewMapProviderDefinition = false,
                  IsSubLayer = true,
               };

               if (form.ShowDialog() == DialogResult.OK) {
                  if (form.MapProviderDefinition != null &&
                      multimapdef.RemoveLevel(idx)) {
                     multimapdef.InsertLevel(form.MapProviderDefinition, idx);
                     string item = item4ListBoxMaps(form.MapProviderDefinition);
                     if (item != lb.Items[idx].ToString())
                        lb.Items[idx] = item;
                  }
               }

            }
         }
      }

      void addMultimap(MultiMapProvider.MultiMapDefinition? multimapdef, int idx, ListBox lb) {
         if (multimapdef != null) {
            FormMapProviderDefinitionEdit form = new FormMapProviderDefinitionEdit() {
               MapProviderDefinition = new MapProviderDefinition(string.Empty,
                                                                 GMap.NET.MapProviders.GMapProviders.OpenStreetMap.Name,
                                                                 getInt(numericUpDownMinZoom),
                                                                 getInt(numericUpDownMaxZoom)),
               IsNewMapProviderDefinition = true,
               IsSubLayer = true,
            };

            if (form.ShowDialog() == DialogResult.OK) {
               string item = item4ListBoxMaps(form.MapProviderDefinition);
               while (lb.Items.Contains(item)) {  // eindeutigen Namen erzeugen
                  form.MapProviderDefinition.MapName += "~";
                  item = item4ListBoxMaps(form.MapProviderDefinition);
               }

               if (idx < 0) {
                  multimapdef.InsertLevel(form.MapProviderDefinition);
                  lb.Items.Add(item);
               } else {
                  multimapdef.InsertLevel(form.MapProviderDefinition, idx);
                  lb.Items.Insert(idx, item);
               }
            }
         }
      }

      string item4ListBoxMaps(MapProviderDefinition def) {
         string mapitem = def.ProviderName + ": " +
                          def.MapName;
         if (def is GarminProvider.GarminMapDefinition) {
            mapitem += " - TDB=" + ((GarminProvider.GarminMapDefinition)def).TDBfile[0];
         } else if (def is GarminKmzProvider.KmzMapDefinition) {
            mapitem += " - KMZ=" + ((GarminKmzProvider.KmzMapDefinition)def).KmzFile;
         } else if (def is WMSProvider.WMSMapDefinition) {
            mapitem += " - URL=" + ((WMSProvider.WMSMapDefinition)def).URL;
         } else if (def is HillshadingProvider.HillshadingMapDefinition) {
            mapitem += " - Alpha=" + ((HillshadingProvider.HillshadingMapDefinition)def).Alpha.ToString();
         }
         return mapitem;
      }

      bool tabControlExtended_Selecting_Intern = false;

      private void tabControlExtended_Selecting(object sender, TabControlCancelEventArgs e) {
         if (!tabControlExtended_Selecting_Intern)
            e.Cancel = true;
      }

      #endregion

      #region Hilfsfunktionen

      void showError(string message) {
         MessageBox.Show(message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
         isSaved = false;
      }

      void setText(TextBox tb, string text) {
         tb.Text = text;
         tb.SelectionStart = tb.Text.Length;
      }

      bool hasText(Control ctrl) => !string.IsNullOrEmpty(ctrl.Text) && ctrl.Text.Trim().Length > 0;

      double getDouble(NumericUpDown ctrl) => Convert.ToDouble(ctrl.Value);

      int getInt(NumericUpDown ctrl) => Convert.ToInt32(ctrl.Value);

      byte getByte(NumericUpDown ctrl) => Convert.ToByte(getInt(ctrl));

      bool getBool(CheckBox ctrl) => ctrl.Checked;

      string getText(TextBox ctrl) => string.IsNullOrEmpty(ctrl.Text) ? string.Empty : ctrl.Text.Trim();

      string getText(ComboBox ctrl) => string.IsNullOrEmpty(ctrl.Text) ? string.Empty : ctrl.Text.Trim();

      void setValue(NumericUpDown ctrl, double val) => ctrl.Value = (decimal)val;

      void setValue(NumericUpDown ctrl, int val) => ctrl.Value = val;

      #endregion

   }
}
