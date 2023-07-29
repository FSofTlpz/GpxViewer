using FSofTUtils.Geography.Garmin;
using GMap.NET.CoreExt.MapProviders;
using GpxViewer.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TrackEddi.ConfigEdit;

namespace GpxViewer.ConfigEdit {
   public partial class FormConfig : Form {

      internal Config Configuration;
      internal IList<MapProviderDefinition> ProviderDefs;
      internal IList<int[]> ProvIdxPaths;


      internal string ActualCachePath = "";

      Config newConfiguration;

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

            newConfiguration = new Config(Configuration.XmlFilename, Configuration.XsdFilename);

            setValueWithMinMax(numericUpDownMinimalTrackpointDistanceX, newConfiguration.MinimalTrackpointDistanceX);
            setValueWithMinMax(numericUpDownMinimalTrackpointDistanceY, newConfiguration.MinimalTrackpointDistanceY);
            path = FSofTUtils.PathHelper.ReplaceEnvironmentVars(newConfiguration.CacheLocation);
            if (path == "")
               path = ActualCachePath;
            if (!Path.IsPathRooted(path)) {
               isChachepathRel = true;
               path = Path.GetFullPath(path);
            }
            buttonCacheLocation.Text = path;

            setValueWithMinMax(numericUpDownDeltaPercent4Search, newConfiguration.DeltaPercent4Search);
            setValueWithMinMax(numericUpDownSymbolZoomfactor, newConfiguration.SymbolZoomfactor);
            setValueWithMinMax(numericUpDownClickTolerance4Tracks, newConfiguration.ClickTolerance4Tracks);

            path = FSofTUtils.PathHelper.ReplaceEnvironmentVars(newConfiguration.DemPath);
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

            if (buttonCacheLocation.Text != "") {
               path = buttonCacheLocation.Text;
               if (isChachepathRel)
                  path = FSofTUtils.PathHelper.GetRelativPath(path, Directory.GetCurrentDirectory());
               else
                  path = FSofTUtils.PathHelper.UseEnvironmentVars4Path(path);
               newConfiguration.CacheLocation = path;
            }
            newConfiguration.DeltaPercent4Search = (int)numericUpDownDeltaPercent4Search.Value;
            newConfiguration.SymbolZoomfactor = (double)numericUpDownSymbolZoomfactor.Value;
            newConfiguration.ClickTolerance4Tracks = (double)numericUpDownClickTolerance4Tracks.Value;

            path = buttonDemPath.Text;
            if (isDEMpathRel)
               path = FSofTUtils.PathHelper.GetRelativPath(path, Directory.GetCurrentDirectory());
            else
               path = FSofTUtils.PathHelper.UseEnvironmentVars4Path(path);
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

            File.Copy(Configuration.XmlFilename, Path.GetFileNameWithoutExtension(Configuration.XmlFilename) + "_backup" + Path.GetExtension(Configuration.XmlFilename), true);
            newConfiguration.SaveData();
            //newConfiguration.SaveData(Path.GetFileNameWithoutExtension(Configuration.XmlFilename) + "_test" + Path.GetExtension(Configuration.XmlFilename));

         } catch (Exception ex) {
            MessageBox.Show("Exception: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      void getColor4Button(Button button) {
         Color newcol = getColor(button.BackColor, true);
         if (newcol != Color.Empty)
            button.BackColor = newcol;
      }

      /// <summary>
      /// liefert eine Argb-Farbe (oder Color.Empty) über einen Dialog
      /// </summary>
      /// <param name="orgcol">beim Start ausgewählte Farbe</param>
      /// <param name="withgarmincolors">wenn true, dann 16 Garminfarben als vordefinierte anzeigen</param>
      /// <returns></returns>
      Color getColor(Color orgcol, bool withgarmincolors) {
         Unclassified.UI.SpecColorSelectorDialog dlg = new Unclassified.UI.SpecColorSelectorDialog() {
            SelectedColor = orgcol,
         };

         if (withgarmincolors) {
            Color[] cols = new Color[] {
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Black],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGray],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkBlue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkCyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkMagenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkRed],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkGreen],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.DarkYellow],

               GarminTrackColors.Colors[GarminTrackColors.Colorname.White],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.LightGray],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Blue],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Cyan],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Magenta],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Red],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Green],
               GarminTrackColors.Colors[GarminTrackColors.Colorname.Yellow],
            };
            for (int i = 0; i < cols.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, cols[i]);
            for (int i = cols.Length; i < dlg.ArrayColorsCount; i++)
               dlg.EnableArrayColor(i, false);
         } else {
            for (int i = 0; i < FormMain.PredefColors.Length && i < dlg.ArrayColorsCount; i++)
               dlg.SetArrayColor(i, FormMain.PredefColors[i]);
         }

         if (dlg.ShowDialog() == DialogResult.OK)
            return dlg.SelectedColor;
         return Color.Empty;
      }

      private void contextMenuStripMaps_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ContextMenuStrip cms = sender as ContextMenuStrip;
         if (cms.SourceControl != null) {
            TreeNode node = (cms.SourceControl as TreeView).SelectedNode;
            if (node != null) {
               ToolStripMenuItemMapEdit.Enabled = !MapTreeViewHelper.IsMapGroupNode(node);

            }
         }
      }




      #region Map-Config

      void createMapTreeView(TreeView tv, IList<MapProviderDefinition> providerDefs) {
         tv.SuspendLayout();
         MapTreeViewHelper.BuildTreeViewContent(newConfiguration, tv, providerDefs, ProvIdxPaths, 0);
         tv.ResumeLayout();
         tv.ExpandAll();
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
         };
         if (form.ShowDialog() == DialogResult.OK) {
            TreeNode tn = new TreeNode(form.MapProviderDefinition.MapName);
            MapTreeViewHelper.SetMapProviderDefinition(tn, form.MapProviderDefinition);

            TreeNode nodeSelected = treeViewMaps.SelectedNode;
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

      private void ToolStripMenuItemMapEdit_Click(object sender, EventArgs e) {
         if (treeViewMaps.SelectedNode != null) {
            TreeNode nodeSelected = treeViewMaps.SelectedNode;
            if (!MapTreeViewHelper.IsMapGroupNode(nodeSelected) &&
                new FormMapProviderDefinitionEdit() {
                   MapProviderDefinition = MapTreeViewHelper.GetMapProviderDefinition(nodeSelected),
                }.ShowDialog() == DialogResult.OK) {
               nodeSelected.Text = MapTreeViewHelper.GetMapProviderDefinition(nodeSelected).MapName;  // falls geändert
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

      private void treeViewMaps_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
         ToolStripMenuItemMapEdit_Click(null, null);
      }

      private void treeViewMaps_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
         if (!string.IsNullOrEmpty(e.Label) &&
             e.Label.Trim().Length > 0) {
            e.Node.EndEdit(false);
            e.Node.Text = e.Label.Trim();

            MapProviderDefinition mpd = MapTreeViewHelper.GetMapProviderDefinition(e.Node);
            if (mpd != null &&
                mpd.MapName != e.Node.Text.Trim()) {
               mpd.MapName = e.Node.Text.Trim();
               MapsConfigChanged = true;
            }
         } else
            e.CancelEdit = true;
      }

      #region TreeView for Maps


      //void createMapTreeView1(TreeView tv, IList<MapProviderDefinition> providerDefs) {
      //   tv.Nodes.Clear();
      //   //tv.SuspendLayout();
      //   for (int provideridx = 0; provideridx < providerDefs.Count; provideridx++) {
      //      TreeNode n = null;
      //      // ev. für übergeordnete Providergruppen noch die Treeitems erzeugen
      //      for (int level = 1; level < ProvIdxPaths[provideridx].Length; level++) {
      //         if (level < ProvIdxPaths[provideridx].Length - 1) {   // Providergroup
      //            int groupidx = ProvIdxPaths[provideridx][level];

      //            TreeNode groupnode = null;
      //            int destidx = -1;

      //            for (int i = 0; i < tv.Nodes.Count; i++) {
      //               if (MapTreeViewHelper.GetMapProviderDefinition(tv.Nodes[i]) == null) {
      //                  if (++destidx == groupidx) {
      //                     groupnode = tv.Nodes[i];
      //                     break;
      //                  }
      //               }
      //            }

      //            if (groupnode == null) {
      //               tv.Nodes.Add(newConfiguration.ProviderGroupName(ProvIdxPaths[provideridx], level + 1));
      //               n = tv.Nodes[tv.Nodes.Count - 1];
      //            } else
      //               n = groupnode;

      //         } else {                                              // Provideritem
      //            TreeNodeCollection tnc = n == null ?
      //                                          tv.Nodes :
      //                                          n.Nodes;
      //            TreeNode tn = tnc.Add(providerDefs[provideridx].MapName);

      //            if (providerDefs[provideridx] is GarminProvider.GarminMapDefinitionData) {
      //               MapTreeViewHelper.SetMapProviderDefinition(tn, new GarminProvider.GarminMapDefinitionData(providerDefs[provideridx] as GarminProvider.GarminMapDefinitionData));
      //            } else if (providerDefs[provideridx] is GarminKmzProvider.KmzMapDefinition) {
      //               MapTreeViewHelper.SetMapProviderDefinition(tn, new GarminKmzProvider.KmzMapDefinition(providerDefs[provideridx] as GarminKmzProvider.KmzMapDefinition));
      //            } else if (providerDefs[provideridx] is WMSProvider.WMSMapDefinition) {
      //               MapTreeViewHelper.SetMapProviderDefinition(tn, new WMSProvider.WMSMapDefinition(providerDefs[provideridx] as WMSProvider.WMSMapDefinition));
      //            } else
      //               MapTreeViewHelper.SetMapProviderDefinition(tn, new MapProviderDefinition(providerDefs[provideridx]));
      //         }
      //      }
      //   }

      //   tv.ExpandAll();
      //   //tv.ResumeLayout();
      //}

      //#region TreeNode-Verschiebungen

      ///// <summary>
      ///// liefert die <see cref="TreeNodeCollection"/> der der <see cref="TreeNode"/> angehört
      ///// </summary>
      ///// <param name="node"></param>
      ///// <returns></returns>
      //TreeNodeCollection getTreeNodeCollection(TreeNode node) {
      //   return node.Parent != null ?
      //                  node.Parent.Nodes :
      //                  node.TreeView != null ?
      //                        node.TreeView.Nodes :
      //                        null;
      //}

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      ///// <returns>true, wenn erfolgreich</returns>
      //bool treeNodeCollectionMoveUp(TreeNode node) {
      //   bool ok = false;
      //   TreeNodeCollection tnc = getTreeNodeCollection(node);
      //   if (tnc != null) {
      //      int idx = node.Index;
      //      if (0 < idx) {
      //         tnc.RemoveAt(idx);
      //         tnc.Insert(idx - 1, node);
      //         ok = true;
      //      }
      //   }
      //   return ok;
      //}

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      ///// <returns>true, wenn erfolgreich</returns>
      //bool treeNodeCollectionMoveDown(TreeNode node) {
      //   bool ok = false;
      //   TreeNodeCollection tnc = getTreeNodeCollection(node);
      //   if (tnc != null) {
      //      int idx = node.Index;
      //      if (idx < tnc.Count - 1) {
      //         tnc.RemoveAt(idx);
      //         tnc.Insert(idx + 1, node);
      //         ok = true;
      //      }
      //   }
      //   return ok;
      //}

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      ///// <returns>true, wenn erfolgreich</returns>
      //bool treeNodeCollectionMoveLeft(TreeNode node) {
      //   bool ok = false;
      //   int idx = node.Index;
      //   if (idx >= 0) {
      //      TreeNode parent = node.Parent;
      //      if (parent != null) {   // sonst nicht möglich
      //         TreeNodeCollection tncParent = getTreeNodeCollection(parent); // direkt über dem bisherigen Parent einfügen
      //         parent.Nodes.RemoveAt(idx);
      //         tncParent.Insert(parent.Index, node);
      //         ok = true;
      //      }
      //   }
      //   return ok;
      //}

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      ///// <param name="newgroupname">wenn vorgegeben, dann neuer Gruppen-Knoten</param>
      ///// <returns>true, wenn erfolgreich</returns>
      //bool treeNodeCollectionMoveRight(TreeNode node, string newgroupname) {
      //   bool ok = false;
      //   TreeNodeCollection tnc = getTreeNodeCollection(node);
      //   if (tnc != null) {
      //      int idx = node.Index;
      //      if (idx >= 0) {
      //         if (string.IsNullOrEmpty(newgroupname)) {
      //            if (node.Index < tnc.Count - 1 &&
      //                isMapGroupNode(tnc[node.Index + 1])) {
      //               tnc.RemoveAt(idx);
      //               tnc[idx].Nodes.Insert(0, node);
      //               ok = true;
      //            }
      //         } else {
      //            tnc.Insert(idx + 1, new TreeNode(newgroupname));
      //            return treeNodeCollectionMoveRight(node, null);
      //         }
      //      }
      //   }
      //   return ok;
      //}

      //#endregion

      /// <summary>
      /// liefert den Pfad der Indexe des <see cref="TreeNode"/> (Gruppen- und Kartenindex)
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      //int[] getIdxPath(TreeNode node) {
      //   List<int> path = new List<int>() { 0 };

      //   List<TreeNode> nodes = new List<TreeNode>();    // "Pfad" der Nodes
      //   do {
      //      nodes.Add(node);
      //      node = node.Parent;
      //   } while (node != null);

      //   for (int i = nodes.Count - 1; i >= 0; i--) {
      //      TreeNode n = nodes[i];
      //      int idx = 0;
      //      TreeNode prevnode = n.PrevNode;
      //      while (prevnode != null) {
      //         if (prevnode != null &&
      //             ((MapTreeViewHelper.GetMapProviderDefinition(n) == null && MapTreeViewHelper.GetMapProviderDefinition(prevnode) == null) ||
      //              (MapTreeViewHelper.GetMapProviderDefinition(n) != null && MapTreeViewHelper.GetMapProviderDefinition(prevnode) != null)))
      //            idx++;
      //         prevnode = prevnode.PrevNode;
      //      }
      //      path.Add(idx);
      //   }
      //   nodes.Clear();

      //   return path.ToArray();
      //}

      /// <summary>
      /// Steht dieser <see cref="TreeNode"/> für eine Kartengruppe (oder eine Karte)?
      /// </summary>
      /// <param name="n"></param>
      /// <returns></returns>
      //bool isMapGroupNode(TreeNode n) => MapTreeViewHelper.GetMapProviderDefinition(n) == null;

      /// <summary>
      /// liefert die <see cref="MapProviderDefinition"/> zum <see cref="TreeNode"/> (oder null)
      /// </summary>
      /// <param name="n"></param>
      /// <returns></returns>
      //MapProviderDefinition getMapProviderDefinition(TreeNode n) =>
      //   n.Tag != null && n.Tag is MapProviderDefinition ? n.Tag as MapProviderDefinition : null;

      //void setMapProviderDefinition(TreeNode n, MapProviderDefinition mpd) =>
      //   n.Tag = mpd;

      //void rebuildConfig4Maps(TreeView tv, Config config) {
      //   config.RemoveMapsSectionContent();
      //   _rebuildConfig4Maps(tv.Nodes, config);
      //}

      //void _rebuildConfig4Maps(TreeNodeCollection tnc, Config config) {
      //   foreach (TreeNode node in tnc) {
      //      int[] idxpath = MapTreeViewHelper.GetIdxPath(node);
      //      int[] idxpathgroup = new int[idxpath.Length - 1];
      //      for (int i = 0; i < idxpathgroup.Length; i++)
      //         idxpathgroup[i] = idxpath[i];
      //      if (MapTreeViewHelper.IsMapGroupNode(node)) {      // -> create Group
      //         config.AppendMapGroup(node.Text, idxpathgroup);
      //         _rebuildConfig4Maps(node.Nodes, config);
      //      } else {                         // create Map
      //         MapProviderDefinition mpd = MapTreeViewHelper.GetMapProviderDefinition(node);
      //         if (mpd is GarminProvider.GarminMapDefinitionData) {
      //            GarminProvider.GarminMapDefinitionData specmpd = mpd as GarminProvider.GarminMapDefinitionData;
      //            config.AppendMap(specmpd.MapName,
      //                             specmpd.ProviderName,
      //                             specmpd.MinZoom,
      //                             specmpd.MaxZoom,
      //                             specmpd.Zoom4Display,
      //                             specmpd.TDBfile[0],
      //                             specmpd.TYPfile[0],
      //                             specmpd.TextFactor,
      //                             specmpd.SymbolFactor,
      //                             specmpd.LineFactor,
      //                             idxpathgroup);
      //         } else if (mpd is GarminKmzProvider.KmzMapDefinition) {
      //            GarminKmzProvider.KmzMapDefinition specmpd = mpd as GarminKmzProvider.KmzMapDefinition;
      //            config.AppendMap(specmpd.MapName,
      //                             specmpd.ProviderName,
      //                             specmpd.MinZoom,
      //                             specmpd.MaxZoom,
      //                             specmpd.Zoom4Display,
      //                             specmpd.KmzFile,
      //                             idxpathgroup);
      //         } else if (mpd is WMSProvider.WMSMapDefinition) {
      //            WMSProvider.WMSMapDefinition specmpd = mpd as WMSProvider.WMSMapDefinition;
      //            config.AppendMap(specmpd.MapName,
      //                             specmpd.ProviderName,
      //                             specmpd.MinZoom,
      //                             specmpd.MaxZoom,
      //                             specmpd.Zoom4Display,
      //                             specmpd.URL,
      //                             specmpd.Version,
      //                             specmpd.SRS,
      //                             specmpd.PictureFormat,
      //                             specmpd.Layer,
      //                             specmpd.ExtendedParameters,
      //                             idxpathgroup);
      //         } else
      //            config.AppendMap(mpd.MapName,
      //                             mpd.ProviderName,
      //                             mpd.MinZoom,
      //                             mpd.MaxZoom,
      //                             mpd.Zoom4Display,
      //                             idxpathgroup);
      //      }
      //   }
      //   MapsConfigChanged = true;
      //}

      #endregion

      #endregion
   }
}
