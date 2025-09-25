using GMap.NET.FSofTExtented.MapProviders;
using GpxViewer.Common;
using TrackEddi.ConfigEdit;

namespace GpxViewer.ConfigEdit {
   public partial class FormConfig : Form {

      internal Config? Configuration;
      internal IList<MapProviderDefinition>? ProviderDefs;
      internal IList<int[]>? ProvIdxPaths;

      internal string ActualCachePath = string.Empty;

      Config? newConfiguration;

      bool isChachepathRel = false;
      bool isDEMpathRel = false;

      /// <summary>
      /// Wurde die Konfiguration für die Karten geändert?
      /// </summary>
      public bool MapsConfigChanged {
         get;
         protected set;
      } = false;


      public FormConfig() {
         InitializeComponent();
      }

      private void FormConfig_Load(object sender, EventArgs e) {
         if (Configuration != null) {
            string path;

            // Kopie der Originaldaten erzeugen
            newConfiguration = new Config(Configuration.XmlFilename, Configuration.XsdFilename);

            setValueWithMinMax(numericUpDownMinimalTrackpointDistanceX, newConfiguration.MinimalTrackpointDistanceX);
            setValueWithMinMax(numericUpDownMinimalTrackpointDistanceY, newConfiguration.MinimalTrackpointDistanceY);
            path = Config.GetPathWithoutEnvironment(newConfiguration.CacheLocation);
            if (path == string.Empty)
               path = ActualCachePath;
            if (!Path.IsPathRooted(path)) {
               isChachepathRel = true;
               path = Path.GetFullPath(path);
            }
            buttonCacheLocation.Text = path;

            setValueWithMinMax(numericUpDownZoom4Display, newConfiguration.Zoom4Displayfactor);
            setValueWithMinMax(numericUpDownDeltaPercent4Search, newConfiguration.DeltaPercent4Search);
            setValueWithMinMax(numericUpDownSymbolZoomfactor, newConfiguration.SymbolZoomfactor);
            setValueWithMinMax(numericUpDownClickTolerance4Tracks, newConfiguration.ClickTolerance4Tracks);

            path = Config.GetPathWithoutEnvironment(newConfiguration.DemPath);
            if (!Path.IsPathRooted(path)) {
               isDEMpathRel = true;
               path = Path.GetFullPath(path);
            }
            buttonDemPath.Text = path;
            setValueWithMinMax(numericUpDownDemMinZoom, newConfiguration.DemMinZoom);
            setValueWithMinMax(numericUpDownDemHillshadingAzimut, newConfiguration.DemHillshadingAzimut);
            setValueWithMinMax(numericUpDownDemHillshadingAltitude, newConfiguration.DemHillshadingAltitude);
            setValueWithMinMax(numericUpDownDemHillshadingScale, newConfiguration.DemHillshadingScale);

            setValueWithMinMax(numericUpDownLastUsedMapsCount, newConfiguration.LastUsedMapsCount);

            buttonStandardTrackColor.BackColor = newConfiguration.StandardTrackColor;
            buttonStandardTrackColor2.BackColor = newConfiguration.StandardTrackColor2;
            buttonStandardTrackColor3.BackColor = newConfiguration.StandardTrackColor3;
            buttonStandardTrackColor4.BackColor = newConfiguration.StandardTrackColor4;
            buttonStandardTrackColor5.BackColor = newConfiguration.StandardTrackColor5;

            setValueWithMinMax(numericUpDownStandardTrackWidth, newConfiguration.StandardTrackWidth);
            setValueWithMinMax(numericUpDownStandardTrackWidth2, newConfiguration.StandardTrackWidth2);
            setValueWithMinMax(numericUpDownStandardTrackWidth3, newConfiguration.StandardTrackWidth3);
            setValueWithMinMax(numericUpDownStandardTrackWidth4, newConfiguration.StandardTrackWidth4);
            setValueWithMinMax(numericUpDownStandardTrackWidth5, newConfiguration.StandardTrackWidth5);

            buttonMarkedTrackColor.BackColor = newConfiguration.MarkedTrackColor;
            buttonEditableTrackColor.BackColor = newConfiguration.EditableTrackColor;
            buttonInEditTrackColor.BackColor = newConfiguration.InEditTrackColor;
            buttonHelperLineColor.BackColor = newConfiguration.HelperLineColor;
            buttonSelectedPartTrackColor.BackColor = newConfiguration.SelectedPartTrackColor;

            setValueWithMinMax(numericUpDownMarkedTrackWidth, newConfiguration.MarkedTrackWidth);
            setValueWithMinMax(numericUpDownEditableTrackWidth, newConfiguration.EditableTrackWidth);
            setValueWithMinMax(numericUpDownInEditTrackWidth, newConfiguration.InEditTrackWidth);
            setValueWithMinMax(numericUpDownHelperLineWidth, newConfiguration.HelperLineWidth);
            setValueWithMinMax(numericUpDownSelectedPartTrackWidth, newConfiguration.SelectedPartTrackWidth);

            if (ProviderDefs != null)
               createMapTreeView(treeViewMaps, ProviderDefs);
         }
      }

      private void FormConfig_KeyDown(object sender, KeyEventArgs e) {
         if (e.KeyCode == Keys.F2 &&
             treeViewMaps.Focused) {
            if (treeViewMaps.SelectedNode != null)
               treeViewMaps.SelectedNode.BeginEdit();
         }
      }

      void setValueWithMinMax(NumericUpDown nud, int value) =>
         nud.Value = Math.Min(Math.Max(nud.Minimum, value), nud.Maximum);

      void setValueWithMinMax(NumericUpDown nud, float value) =>
         nud.Value = Math.Min(Math.Max(nud.Minimum, (decimal)value), nud.Maximum);

      void setValueWithMinMax(NumericUpDown nud, double value) =>
         nud.Value = Math.Min(Math.Max(nud.Minimum, (decimal)value), nud.Maximum);


      private void buttonCacheLocation_Click(object sender, EventArgs e) {
         folderBrowserDialogCacheLocation.SelectedPath = buttonCacheLocation.Text;
         if (folderBrowserDialogCacheLocation.ShowDialog() == DialogResult.OK)
            buttonCacheLocation.Text = folderBrowserDialogCacheLocation.SelectedPath;
      }

      private void buttonDemPath_Click(object sender, EventArgs e) {
         folderBrowserDialogDemPath.SelectedPath = buttonDemPath.Text;
         if (folderBrowserDialogDemPath.ShowDialog() == DialogResult.OK)
            buttonDemPath.Text = folderBrowserDialogDemPath.SelectedPath;
      }

      private void buttonStandardTrackColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonStandardTrackColor2_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonStandardTrackColor3_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonStandardTrackColor4_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonStandardTrackColor5_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonMarkedTrackColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonEditableTrackColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonInEditTrackColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonHelperLineColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonSelectedPartTrackColor_Click(object sender, EventArgs e) {
         getColor4Button(sender as Button);
      }

      private void buttonOK_Click(object sender, EventArgs e) {
         try {
            string path;

            newConfiguration.MinimalTrackpointDistanceX = (int)numericUpDownMinimalTrackpointDistanceX.Value;
            newConfiguration.MinimalTrackpointDistanceY = (int)numericUpDownMinimalTrackpointDistanceY.Value;

            if (buttonCacheLocation.Text != string.Empty) {
               path = buttonCacheLocation.Text;
               if (isChachepathRel)
                  path = FSofTUtils.PathHelper.GetRelativPath(path, Directory.GetCurrentDirectory());
               else
                  path = Config.GetPathWithEnvironment(path);
               newConfiguration.CacheLocation = path;
            }
            newConfiguration.Zoom4Displayfactor = (double)numericUpDownZoom4Display.Value;
            newConfiguration.DeltaPercent4Search = (int)numericUpDownDeltaPercent4Search.Value;
            newConfiguration.SymbolZoomfactor = (double)numericUpDownSymbolZoomfactor.Value;
            newConfiguration.ClickTolerance4Tracks = (double)numericUpDownClickTolerance4Tracks.Value;

            path = buttonDemPath.Text;
            if (isDEMpathRel)
               path = FSofTUtils.PathHelper.GetRelativPath(path, Directory.GetCurrentDirectory());
            else
               path = Config.GetPathWithEnvironment(path);
            newConfiguration.DemPath = path;
            newConfiguration.DemMinZoom = (int)numericUpDownDemMinZoom.Value;
            newConfiguration.DemHillshadingAzimut = (double)numericUpDownDemHillshadingAzimut.Value;
            newConfiguration.DemHillshadingAltitude = (double)numericUpDownDemHillshadingAltitude.Value;
            newConfiguration.DemHillshadingScale = (double)numericUpDownDemHillshadingScale.Value;

            newConfiguration.LastUsedMapsCount = (int)numericUpDownLastUsedMapsCount.Value;

            newConfiguration.StandardTrackColor = buttonStandardTrackColor.BackColor;
            newConfiguration.StandardTrackColor2 = buttonStandardTrackColor2.BackColor;
            newConfiguration.StandardTrackColor3 = buttonStandardTrackColor3.BackColor;
            newConfiguration.StandardTrackColor4 = buttonStandardTrackColor4.BackColor;
            newConfiguration.StandardTrackColor5 = buttonStandardTrackColor5.BackColor;

            newConfiguration.StandardTrackWidth = (float)numericUpDownStandardTrackWidth.Value;
            newConfiguration.StandardTrackWidth2 = (float)numericUpDownStandardTrackWidth2.Value;
            newConfiguration.StandardTrackWidth3 = (float)numericUpDownStandardTrackWidth3.Value;
            newConfiguration.StandardTrackWidth4 = (float)numericUpDownStandardTrackWidth4.Value;
            newConfiguration.StandardTrackWidth5 = (float)numericUpDownStandardTrackWidth5.Value;

            newConfiguration.MarkedTrackColor = buttonMarkedTrackColor.BackColor;
            newConfiguration.EditableTrackColor = buttonEditableTrackColor.BackColor;
            newConfiguration.InEditTrackColor = buttonInEditTrackColor.BackColor;
            newConfiguration.HelperLineColor = buttonHelperLineColor.BackColor;
            newConfiguration.SelectedPartTrackColor = buttonSelectedPartTrackColor.BackColor;

            newConfiguration.MarkedTrackWidth = (float)numericUpDownMarkedTrackWidth.Value;
            newConfiguration.EditableTrackWidth = (float)numericUpDownEditableTrackWidth.Value;
            newConfiguration.InEditTrackWidth = (float)numericUpDownInEditTrackWidth.Value;
            newConfiguration.HelperLineWidth = (float)numericUpDownHelperLineWidth.Value;
            newConfiguration.SelectedPartTrackWidth = (float)numericUpDownSelectedPartTrackWidth.Value;

            MapTreeViewHelper.RebuildConfig4Maps(treeViewMaps, newConfiguration);

            newConfiguration.UseEnvironementVarsInPaths();
            ConfigHelper.Save(newConfiguration);

         } catch (Exception ex) {
            MessageBox.Show("Exception: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      void getColor4Button(Button? button) {
         if (button != null &&
             FormMain.GetColor(button.BackColor, true, FormMain.PredefColors, out Color newcol))
            button.BackColor = newcol;
      }

      private void contextMenuStripMaps_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ContextMenuStrip? cms = sender as ContextMenuStrip;
         if (cms != null &&
             cms.SourceControl != null) {
            TreeNode? node = (cms.SourceControl as TreeView).SelectedNode;
            if (node != null) {
               ToolStripMenuItemMapEdit.Enabled = !MapTreeViewHelper.IsMapGroupNode(node);

            }
         }
      }


      #region Map-Config

      void createMapTreeView(TreeView tv, IList<MapProviderDefinition> providerDefs) {
         if (newConfiguration != null &&
             ProvIdxPaths != null) {
            tv.SuspendLayout();
            MapTreeViewHelper.BuildTreeViewContent(newConfiguration, tv, providerDefs, ProvIdxPaths, 0);
            tv.ResumeLayout();
            tv.ExpandAll();
         }
      }

      private void buttonMapMoveUp_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            MapTreeViewHelper.treeNodeCollectionMoveUp(nodeSelected);
            treeViewMaps.SelectedNode = nodeSelected;
         }
      }

      private void buttonMapMoveDown_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            MapTreeViewHelper.treeNodeCollectionMoveDown(nodeSelected);
            treeViewMaps.SelectedNode = nodeSelected;
         }
      }

      private void buttonMapMoveLeft_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            MapTreeViewHelper.treeNodeCollectionMoveLeft(nodeSelected);
            treeViewMaps.SelectedNode = nodeSelected;
         }
      }

      private void buttonMapMoveRight_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            MapTreeViewHelper.treeNodeCollectionMoveRight(nodeSelected, null);
            treeViewMaps.SelectedNode = nodeSelected;
         }
      }

      private void ToolStripMenuItemMapNew_Click(object sender, EventArgs e) {
         FormMapProviderDefinitionEdit form = new FormMapProviderDefinitionEdit() {
            MapProviderDefinition = new MapProviderDefinition() {
               MapName = "neue Karte",
            },
            IsNewMapProviderDefinition = true,
            IsSubLayer = false,
         };
         if (form.ShowDialog() == DialogResult.OK) {
            TreeNode tn = new TreeNode(form.MapProviderDefinition.MapName);
            MapTreeViewHelper.SetMapProviderDefinition(tn, form.MapProviderDefinition);

            TreeNode? nodeSelected = treeViewMaps.SelectedNode;
            if (nodeSelected != null) {
               if (MapTreeViewHelper.IsMapGroupNode(nodeSelected))   // Gruppe
                  nodeSelected.Nodes.Add(tn);
               else {
                  if (nodeSelected.Parent != null)
                     nodeSelected.Parent.Nodes.Insert(nodeSelected.Index + 1, tn);
                  else
                     treeViewMaps.Nodes.Insert(nodeSelected.Index + 1, tn);
               }
            } else
               treeViewMaps.Nodes.Insert(0, tn);

            treeViewMaps.SelectedNode = tn;
         }
      }

      private void ToolStripMenuItemMapGroupNew_Click(object sender, EventArgs e) {
         treeViewMaps.Nodes.Insert(0, "neue Kartengruppe");       // immer an 1. Pos. in der Hauptgruppe
         treeViewMaps.SelectedNode = treeViewMaps.Nodes[0];
         treeViewMaps.SelectedNode.EnsureVisible();
      }

      private void ToolStripMenuItemMapEdit_Click(object? sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            if (!MapTreeViewHelper.IsMapGroupNode(nodeSelected)) {
               FormMapProviderDefinitionEdit form = new FormMapProviderDefinitionEdit() {
                  MapProviderDefinition = MapTreeViewHelper.GetMapProviderDefinition(nodeSelected),
                  IsNewMapProviderDefinition = false,
                  IsSubLayer = false,
               };
               if (form.ShowDialog() == DialogResult.OK &&
                   form.MapProviderDefinition != null) {
                  MapTreeViewHelper.SetMapProviderDefinition(nodeSelected, form.MapProviderDefinition);
                  nodeSelected.Text = MapTreeViewHelper.GetMapProviderDefinition(nodeSelected).MapName;  // falls geändert
               }
            }
            treeViewMaps.SelectedNode = nodeSelected;
         }
      }

      private void ToolStripMenuItemMapDelete_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            if (MessageBox.Show(nodeSelected.Tag == null ?
                                    "Soll die Kartengruppe '" + nodeSelected.Text + "' gelöscht werden?" :
                                    "Soll die Karte '" + nodeSelected.Text + "' gelöscht werden?",
                                "Löschen",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes) {
               nodeSelected.Remove();
            }
         }
      }

      private void treeViewMaps_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
         e.Node.TreeView.SelectedNode = e.Node;
      }

      private void treeViewMaps_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) =>
         ToolStripMenuItemMapEdit_Click(null, EventArgs.Empty);

      private void treeViewMaps_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
         if (!string.IsNullOrEmpty(e.Label) &&
             e.Label.Trim().Length > 0) {
            MapProviderDefinition? mpd = MapTreeViewHelper.GetMapProviderDefinition(e.Node);
            if (mpd != null &&
                mpd.MapName != e.Label.Trim()) {
               e.Node.Text = e.Label.Trim();
               mpd.MapName = e.Node.Text.Trim();
               MapsConfigChanged = true;
            } else
               e.CancelEdit = true;
         } else
            e.CancelEdit = true;
      }

      #endregion

   }
}
