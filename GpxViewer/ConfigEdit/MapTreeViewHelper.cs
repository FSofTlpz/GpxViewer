﻿#if Android
using FSofTUtils.Xamarin.Control;
using TrackEddi.Common;
#else
using GpxViewer.Common;
using System.Windows.Forms;
using TreeViewNode = System.Windows.Forms.TreeNode;         // Alias
using System.Linq;
#endif
using GMap.NET.CoreExt.MapProviders;
using System.Collections.Generic;

namespace TrackEddi.ConfigEdit {
   internal class MapTreeViewHelper {

      public enum MapMove {
         Left, Right, Up, Down
      }


      static List<TreeViewNode> GetChildNodes(TreeView tv) =>
#if Android
         tv.GetChildNodes();
#else
         tv.Nodes.OfType<TreeViewNode>().ToList();
#endif

      static List<TreeViewNode> GetChildNodes(TreeViewNode n) =>
#if Android
         n.GetChildNodes();
#else
         n.Nodes.OfType<TreeViewNode>().ToList();
#endif

      static void AddChildNode(TreeView tv, TreeViewNode n) =>
#if Android
         tv.AddChildNode(n);
#else
         tv.Nodes.Add(n);
#endif 

      static void AddChildNode(TreeViewNode nodeParent, TreeViewNode n) =>
#if Android
         nodeParent.AddChildNode(n);
#else
         nodeParent.Nodes.Add(n);
#endif 

      static TreeViewNode ParentNode(TreeViewNode n) =>
#if Android
         n.ParentNode;
#else
         n.Parent;
#endif

      static bool HasChildNodes(TreeViewNode n) =>
#if Android
         n.HasChildNodes;
#else
         n.Nodes?.Count > 0;
#endif

      static bool HasChildNodes(TreeView tv) =>
#if Android
         tv.HasChildNodes;
#else
         tv.Nodes?.Count > 0;
#endif

      static void InsertChildNode(TreeViewNode parent, int pos, TreeViewNode n) =>
#if Android
         parent.InsertChildNode(pos, n);
#else
         parent.Nodes.Insert(pos, n);
#endif

      static void InsertChildNode(TreeView parent, int pos, TreeViewNode n) =>
#if Android
         parent.InsertChildNode(pos, n);
#else
         parent.Nodes.Insert(pos, n);
#endif

      /// <summary>
      /// alle Childnodes entfernen
      /// </summary>
      /// <param name="parent"></param>
      static void RemoveChildNodes(TreeViewNode parent) =>
#if Android
         parent.RemoveChildNodes();
#else
         parent.Nodes?.Clear();
#endif

      /// <summary>
      /// alle Childnodes entfernen
      /// </summary>
      /// <param name="parent"></param>
      static void RemoveChildNodes(TreeView parent) =>
#if Android
         parent.RemoveChildNodes();
#else
         parent.Nodes?.Clear();
#endif

      static void RemoveChildNode(TreeViewNode parent, TreeViewNode n) =>
#if Android
         parent.RemoveChildNode(n);
#else
         parent.Nodes.Remove(n);
#endif

      static void RemoveChildNode(TreeView parent, TreeViewNode n) =>
#if Android
         parent.RemoveChildNode(n);
#else
         parent.Nodes.Remove(n);
#endif

      static void RemoveChildNode(TreeViewNode parent, int pos) =>
#if Android
         parent.RemoveChildNode(pos);
#else
         parent.Nodes.RemoveAt(pos);
#endif

      static void RemoveChildNode(TreeView parent, int pos) =>
#if Android
         parent.RemoveChildNode(pos);
#else
         parent.Nodes.RemoveAt(pos);
#endif

      /// <summary>
      /// entfernt diesen Node aus seiner Auflistung
      /// </summary>
      /// <param name="child"></param>
      static void RemoveChildNode(TreeViewNode child) {
         TreeViewNode parent = ParentNode(child);
         if (parent != null)
            RemoveChildNode(parent, child);
         else
            RemoveChildNode(child.TreeView, child);
      }


      //int getIdx4Mapname(string mapname, IList<MapProviderDefinition> providerdefs) {
      //   for (int j = 0; j < providerdefs.Count; j++)
      //      if (providerdefs[j].MapName == mapname)
      //         return j;
      //   return -1;
      //}

      static public void BuildTreeViewContent(Config config,
                                              TreeView tv,
                                              IList<MapProviderDefinition> providerdefs,
                                              IList<int[]> providxpaths,
                                              int selectedIdx) {
         TreeViewNode nodeSelected = null;
         clearTreeViewNodes(tv);
         for (int provideridx = 0; provideridx < providerdefs.Count; provideridx++) {
            TreeViewNode nodeParent = null;
            List<TreeViewNode> nodes = GetChildNodes(tv);

            for (int level = 1; level < providxpaths[provideridx].Length; level++) {
               if (level < providxpaths[provideridx].Length - 1) {   // Providergroup
                  int groupidx = providxpaths[provideridx][level];

                  TreeViewNode node4group = null;
                  int destidx = -1;
                  for (int i = 0; i < nodes.Count; i++) {
                     if (IsMapGroupNode(nodes[i])) {
                        if (++destidx == groupidx) {
                           node4group = nodes[i];
                           break;
                        }
                     }
                  }

                  if (node4group == null) {
                     TreeViewNode n = new TreeViewNode(config.ProviderGroupName(providxpaths[provideridx], level + 1));
                     if (nodeParent == null)
                        AddChildNode(tv, n);
                     else
                        AddChildNode(nodeParent, n);
                     nodeParent = n;
                  } else
                     nodeParent = node4group;

               } else {                                              // Provideritem
                  TreeViewNode n = new TreeViewNode(providerdefs[provideridx].MapName);
                  if (nodeParent == null)
                     AddChildNode(tv, n);
                  else
                     AddChildNode(nodeParent, n);
                  if (providerdefs[provideridx] is GarminProvider.GarminMapDefinitionData) {
                     SetMapProviderDefinition(n, new GarminProvider.GarminMapDefinitionData(providerdefs[provideridx] as GarminProvider.GarminMapDefinitionData));
                  } else if (providerdefs[provideridx] is GarminKmzProvider.KmzMapDefinition) {
                     SetMapProviderDefinition(n, new GarminKmzProvider.KmzMapDefinition(providerdefs[provideridx] as GarminKmzProvider.KmzMapDefinition));
                  } else if (providerdefs[provideridx] is WMSProvider.WMSMapDefinition) {
                     SetMapProviderDefinition(n, new WMSProvider.WMSMapDefinition(providerdefs[provideridx] as WMSProvider.WMSMapDefinition));
                  } else
                     SetMapProviderDefinition(n, new MapProviderDefinition(providerdefs[provideridx]));

                  if (provideridx == selectedIdx)
                     nodeSelected = n;
               }
            }
         }

         if (nodeSelected != null)
            tv.SelectedNode = nodeSelected;
      }

      /// <summary>
      /// liefert den Pfad der Indexe des <see cref="TreeNode"/> (Gruppen- und Kartenindex)
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      static int[] GetIdxPath(TreeViewNode node) {
         List<int> path = new List<int>() { 0 };

         List<TreeViewNode> nodes = new List<TreeViewNode>();    // "Pfad" der Nodes
         do {
            nodes.Add(node);
            node = ParentNode(node);
         } while (node != null);

         for (int i = nodes.Count - 1; i >= 0; i--) {
            TreeViewNode n = nodes[i];
            int idx = 0;
            TreeViewNode prevnode = getPrevNode(n);
            while (prevnode != null) {
               if (prevnode != null &&
                   ((GetMapProviderDefinition(n) == null && GetMapProviderDefinition(prevnode) == null) ||
                    (GetMapProviderDefinition(n) != null && GetMapProviderDefinition(prevnode) != null)))
                  idx++;
               prevnode = getPrevNode(prevnode);
            }
            path.Add(idx);
         }
         nodes.Clear();

         return path.ToArray();
      }

      static public int GetNodeIndex(TreeViewNode node) {
#if Android
         List<TreeViewNode> lst = getTreeNodeCollection(node);
         return lst != null ? lst.IndexOf(node) : -1;
#else
         return node.Index;
#endif
      }

      /// <summary>
      /// Steht dieser <see cref="TreeNode"/> für eine Kartengruppe (oder eine Karte)?
      /// </summary>
      /// <param name="n"></param>
      /// <returns></returns>
      static public bool IsMapGroupNode(TreeViewNode n) => GetMapProviderDefinition(n) == null;

      /// <summary>
      /// liefert die <see cref="MapProviderDefinition"/> zum <see cref="TreeNode"/> (oder null)
      /// </summary>
      /// <param name="n"></param>
      /// <returns></returns>
      static public MapProviderDefinition GetMapProviderDefinition(TreeViewNode n) =>
#if Android
         n.ExtendedData != null && n.ExtendedData is MapProviderDefinition ? n.ExtendedData as MapProviderDefinition : null;
#else
         n.Tag != null && n.Tag is MapProviderDefinition ? n.Tag as MapProviderDefinition : null;
#endif

      static public void SetMapProviderDefinition(TreeViewNode n, MapProviderDefinition mpd) =>
#if Android
         n.ExtendedData = mpd;
#else
         n.Tag = mpd;
#endif


      /// <summary>
      /// liefert den vorhergehenden <see cref="TreeViewNode"/> in der Auflistung der <see cref="TreeViewNode"/> zu der dieser <see cref="TreeViewNode"/> gehört
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      static void clearTreeViewSubNodes(IList<TreeViewNode> nodes) {
         if (nodes != null) {
            foreach (TreeViewNode node in nodes) {
               if (HasChildNodes(node))
                  clearTreeViewSubNodes(GetChildNodes(node));
               RemoveChildNodes(node);
            }
            //nodes.Clear();
         }
      }

      static void clearTreeViewNodes(TreeView tv) {
         if (HasChildNodes(tv)) {
            IList<TreeViewNode> nodes = GetChildNodes(tv);
            clearTreeViewSubNodes(nodes);
            //nodes.Clear();
         }
         RemoveChildNodes(tv);
      }

      static TreeViewNode getPrevNode(TreeViewNode node) {
         List<TreeViewNode> lst = getTreeNodeCollection(node);
         if (lst != null) {
            int idx = lst.IndexOf(node);
            if (0 < idx)
               return lst[idx - 1];
         }
         return null;
      }

      /// <summary>
      /// liefert die <see cref="TreeNodeCollection"/> der der <see cref="TreeNode"/> angehört
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      static List<TreeViewNode> getTreeNodeCollection(TreeViewNode node) {
         return ParentNode(node) != null ?
                        GetChildNodes(ParentNode(node)) :
                        node.TreeView != null ?
                              GetChildNodes(node.TreeView) :
                              null;
      }

      #region TreeNode-Verschiebungen (static)

      static void moveNode2Idx(TreeViewNode node, int idx) {
         TreeViewNode parentnode = ParentNode(node);
         if (parentnode != null) {
            RemoveChildNode(parentnode, node);
            InsertChildNode(parentnode, idx, node);
         } else {
            TreeView tv = node.TreeView;
            RemoveChildNode(tv, node);
            InsertChildNode(tv, idx, node);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      /// <returns>true, wenn erfolgreich</returns>
      static public bool treeNodeCollectionMoveUp(TreeViewNode node) {
         bool ok = false;
         int idx = GetNodeIndex(node);
         if (0 < idx) {
            moveNode2Idx(node, idx - 1);
            ok = true;
         }
         return ok;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      /// <returns>true, wenn erfolgreich</returns>
      static public bool treeNodeCollectionMoveDown(TreeViewNode node) {
         bool ok = false;
         int idx = GetNodeIndex(node);
         //if (idx < tnc.Count - 1) {
         if (0 <= idx) {
            moveNode2Idx(node, idx + 1);
            ok = true;
         }
         return ok;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      /// <returns>true, wenn erfolgreich</returns>
      static public bool treeNodeCollectionMoveLeft(TreeViewNode node) {
         bool ok = false;
         if (ParentNode(node) != null) {   // sonst kein Schieben nach möglich
            int idx = GetNodeIndex(node);
            if (0 <= idx) {
               TreeViewNode parent = ParentNode(node);
               RemoveChildNode(parent, idx);
               idx = GetNodeIndex(parent);    // direkt über dem bisherigen Parent einfügen
               if (ParentNode(parent) != null)
                  InsertChildNode(ParentNode(parent), idx, node);
               else
                  InsertChildNode(parent.TreeView, idx, node);
               ok = true;
            }
         }
         return ok;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="node">zu verschiebender <see cref="TreeNode"/></param>
      /// <param name="newgroupname">wenn vorgegeben, dann neuer Gruppen-Knoten</param>
      /// <returns>true, wenn erfolgreich</returns>
      static public bool treeNodeCollectionMoveRight(TreeViewNode node, string newgroupname) {
         bool ok = false;
         List<TreeViewNode> tnc = getTreeNodeCollection(node);
         if (tnc != null) {
            int idx = tnc.IndexOf(node);
            if (idx >= 0) {
               if (string.IsNullOrEmpty(newgroupname)) {
                  if (idx < tnc.Count - 1 &&
                      IsMapGroupNode(tnc[idx + 1])) {
                     TreeViewNode newParentNode = tnc[idx + 1];
                     RemoveChildNode(node);
                     InsertChildNode(newParentNode, 0, node);
                     ok = true;
                  }
               } else {
                  tnc.Insert(idx + 1, new TreeViewNode(newgroupname));
                  return treeNodeCollectionMoveRight(node, null);
               }
            }
         }
         return ok;
      }

      #endregion

      public static void RebuildConfig4Maps(TreeView tv, Config config) {
         config.RemoveMapsSectionContent();
         _rebuildConfig4Maps(GetChildNodes(tv), config);
      }

      static void _rebuildConfig4Maps(List<TreeViewNode> tnc, Config config) {
         foreach (TreeViewNode node in tnc) {
            int[] idxpath = GetIdxPath(node);
            int[] idxpathgroup = new int[idxpath.Length - 1];
            for (int i = 0; i < idxpathgroup.Length; i++)
               idxpathgroup[i] = idxpath[i];
            if (IsMapGroupNode(node)) {      // -> create Group
               config.AppendMapGroup(node.Text, idxpathgroup);
               _rebuildConfig4Maps(GetChildNodes(node), config);
            } else {                         // create Map
               MapProviderDefinition mpd = GetMapProviderDefinition(node);
               if (mpd is GarminProvider.GarminMapDefinitionData) {
                  GarminProvider.GarminMapDefinitionData specmpd = mpd as GarminProvider.GarminMapDefinitionData;
                  config.AppendMap(specmpd.MapName,
                                   specmpd.ProviderName,
                                   specmpd.MinZoom,
                                   specmpd.MaxZoom,
                                   specmpd.Zoom4Display,
                                   specmpd.TDBfile[0],
                                   specmpd.TYPfile[0],
                                   specmpd.TextFactor,
                                   specmpd.SymbolFactor,
                                   specmpd.LineFactor,
                                   specmpd.HillShading,
                                   specmpd.HillShadingAlpha,
                                   idxpathgroup);
               } else if (mpd is GarminKmzProvider.KmzMapDefinition) {
                  GarminKmzProvider.KmzMapDefinition specmpd = mpd as GarminKmzProvider.KmzMapDefinition;
                  config.AppendMap(specmpd.MapName,
                                   specmpd.ProviderName,
                                   specmpd.MinZoom,
                                   specmpd.MaxZoom,
                                   specmpd.Zoom4Display,
                                   specmpd.KmzFile,
                                   specmpd.HillShading,
                                   specmpd.HillShadingAlpha,
                                   idxpathgroup);
               } else if (mpd is WMSProvider.WMSMapDefinition) {
                  WMSProvider.WMSMapDefinition specmpd = mpd as WMSProvider.WMSMapDefinition;
                  config.AppendMap(specmpd.MapName,
                                   specmpd.ProviderName,
                                   specmpd.MinZoom,
                                   specmpd.MaxZoom,
                                   specmpd.Zoom4Display,
                                   specmpd.URL,
                                   specmpd.Version,
                                   specmpd.SRS,
                                   specmpd.PictureFormat,
                                   specmpd.Layer,
                                   specmpd.ExtendedParameters,
                                   idxpathgroup);
               } else
                  config.AppendMap(mpd.MapName,
                                   mpd.ProviderName,
                                   mpd.MinZoom,
                                   mpd.MaxZoom,
                                   mpd.Zoom4Display,
                                   idxpathgroup);
            }
         }
      }
   }
}
