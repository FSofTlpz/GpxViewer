﻿using FSofTUtils;
using SpecialMapCtrl;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class EditableTracklistControl : UserControl {

      GpxAllExt gpx = null;

      /// <summary>
      /// zugehörige Gpx-Sammlung
      /// </summary>
      public GpxAllExt Gpx {
         get => gpx;
         set {
            if (gpx != null) {
               gpx.MarkerlistlistChanged -= Gpx_MarkerlistlistChanged;
               gpx.TracklistChanged -= Gpx_TracklistChanged;
               treeView_Tracks.Nodes.Clear();
               treeView_Marker.Nodes.Clear();
            }
            gpx = value;
            if (gpx != null) {
               treeView_Tracks.Nodes.Clear();
               treeView_Marker.Nodes.Clear();
               Gpx_TracklistChanged(gpx, new GpxAllExt.TracklistChangedEventArgs(GpxAllExt.TracklistChangedEventArgs.Kind.Add, -1, -1));
               Gpx_MarkerlistlistChanged(gpx, new GpxAllExt.MarkerlistChangedEventArgs(GpxAllExt.MarkerlistChangedEventArgs.Kind.Add, -1, -1));
               gpx.MarkerlistlistChanged += Gpx_MarkerlistlistChanged;
               gpx.TracklistChanged += Gpx_TracklistChanged;
            }
         }
      }

      /// <summary>
      /// Anzahl der Tracks
      /// </summary>
      public int Tracks {
         get => treeView_Tracks.Nodes.Count;       // aber auch in EditableGpx
      }

      /// <summary>
      /// Anzahl der Marker
      /// </summary>
      public int Markers {
         get => treeView_Marker.Nodes.Count;       // aber auch in EditableGpx
      }

      /// <summary>
      /// akt. ausgewählter Track (oder null)
      /// </summary>
      public Track SelectedTrack {
         get {
            int idx = tv_GetSelectedIndex(treeView_Tracks);
            return idx4TrackIsValid(idx) ?
                        gpx?.TrackList[idx] :
                        null;
         }
      }

      /// <summary>
      /// akt. ausgewählter Marker (oder null)
      /// </summary>
      public Marker SelectedMarker {
         get {
            int idx = tv_GetSelectedIndex(treeView_Marker);
            return idx4MarkerIsValid(idx) ?
                        gpx?.MarkerList[idx] :
                        null;
         }
      }

      #region Events

      #region EventArgs

      public class OrderChangedEventArgs {
         public readonly int OldIdx;
         public readonly int NewIdx;

         public OrderChangedEventArgs(int oldidx, int newidx) {
            OldIdx = oldidx;
            NewIdx = newidx;
         }
      }

      public class TrackEventArgs {
         public readonly Track Track;
         public readonly bool Visible;

         public TrackEventArgs(Track track, bool visible = false) {
            Track = track;
            Visible = visible;
         }
      }

      public class MarkerEventArgs {
         public readonly Marker Marker;
         public readonly bool Visible;

         public MarkerEventArgs(Marker marker, bool visible = false) {
            Marker = marker;
            Visible = visible;
         }
      }

      public class IdxEventArgs {
         public readonly int Idx;

         public IdxEventArgs(int idx) {
            Idx = idx;
         }
      }

      #endregion

      /// <summary>
      /// Ein Track hat sein Position in der Liste verändert.
      /// </summary>
      public event EventHandler<OrderChangedEventArgs> TrackOrderChangedEvent;

      /// <summary>
      /// Ein Marker hat sein Position in der Liste verändert.
      /// </summary>
      public event EventHandler<OrderChangedEventArgs> MarkerOrderChangedEvent;

      /// <summary>
      /// Ein Track soll angezeigt oder verborgen werden.
      /// </summary>
      public event EventHandler<TrackEventArgs> ShowTrackEvent;

      /// <summary>
      /// Ein Marker soll angezeigt oder verborgen werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs> ShowMarkerEvent;

      /// <summary>
      /// Ein Track wurde markiert.
      /// </summary>
      public event EventHandler<IdxEventArgs> SelectTrackEvent;

      /// <summary>
      /// Ein Track wurd mit Doppelklick ausgewählt.
      /// </summary>
      public event EventHandler<IdxEventArgs> ChooseTrackEvent;

      /// <summary>
      /// Ein Marker wurd mit Doppelklick ausgewählt.
      /// </summary>
      public event EventHandler<IdxEventArgs> ChooseMarkerEvent;

      /// <summary>
      /// Ein Kontextmenü für den Track sollte angezeigt werden.
      /// </summary>
      public event EventHandler<TrackEventArgs> ShowContextmenu4TrackEvent;

      /// <summary>
      /// Ein Kontextmenü für den Marker sollte angezeigt werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs> ShowContextmenu4MarkerEvent;

      /// <summary>
      /// Die Darstellung auf der Karte sollte für diesen Track akt. werden.
      /// </summary>
      public event EventHandler<TrackEventArgs> UpdateVisualTrackEvent;

      /// <summary>
      /// Die Darstellung auf der Karte sollte für diesen Marker akt. werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs> UpdateVisualMarkerEvent;

      #endregion


      public EditableTracklistControl() {
         InitializeComponent();

         gpx = new GpxAllExt();
      }

      #region Dra&Drop (Windows)

      private void EditableTracklistControl_DragDrop(object sender, DragEventArgs e) {
         insertFiles(e.Data.GetData(DataFormats.FileDrop, false) as string[]);
      }

      private void EditableTracklistControl_DragEnter(object sender, DragEventArgs e) {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
         else
            e.Effect = DragDropEffects.None;
      }

      #endregion

      #region Events von Gpx (PoorGpxAllExt)

      private void Gpx_TracklistChanged(object sender, GpxAllExt.TracklistChangedEventArgs e) {
         GpxAllExt gpx = sender as GpxAllExt;
         TreeView tv = treeView_Tracks;

         // Listbox anpassen
         if (e != null)
            switch (e.KindOfChanging) {
               case GpxAllExt.TracklistChangedEventArgs.Kind.Add:
                  if (0 <= e.To) {
                     listChanged_Add(tv, e.To, gpx.TrackList[e.To].VisualName);
                  } else { // gesamte Liste übernehmen
                     tv.Nodes.Clear();
                     for (int i = 0; i < gpx.TrackList.Count; i++) {
                        tv.Nodes.Add(gpx.TrackList[i].VisualName);
                        tv_SetSelectedIndex(tv, i);
                        tv.SelectedNode.Checked = gpx.TrackList[i].IsVisible;
                     }
                  }
                  break;

               case GpxAllExt.TracklistChangedEventArgs.Kind.Remove:
                  listChanged_Remove(tv, e.From);
                  break;

               case GpxAllExt.TracklistChangedEventArgs.Kind.Move:
                  TreeNode tn = tv.Nodes[e.From];
                  tv.Nodes.RemoveAt(e.From);
                  tv.Nodes.Insert(e.To, tn);

                  Track track = gpx.TrackList[e.To];
                  if (track.IsVisible) { // Neuanzeigen wegen Veränderung der Reihenfolge
                     if (ShowTrackEvent != null) {
                        ShowTrackEvent.Invoke(this, new TrackEventArgs(track, false));
                        ShowTrackEvent.Invoke(this, new TrackEventArgs(track, true));
                     }
                  }

                  tv_SetSelectedIndex(tv, e.To);
                  break;

            }
      }

      private void Gpx_MarkerlistlistChanged(object sender, GpxAllExt.MarkerlistChangedEventArgs e) {
         GpxAllExt gpx = sender as GpxAllExt;
         TreeView tv = treeView_Marker;

         if (e != null)
            switch (e.KindOfChanging) {
               case GpxAllExt.MarkerlistChangedEventArgs.Kind.Add:
                  if (0 <= e.To) {
                     listChanged_Add(tv, e.To, gpx.MarkerList[e.To].Text);
                  } else { // gesamte Liste übernehmen
                     tv.Nodes.Clear();
                     for (int i = 0; i < gpx.MarkerList.Count; i++) {
                        tv.Nodes.Add(gpx.MarkerList[i].Text);
                        tv_SetSelectedIndex(tv, i);
                        tv.SelectedNode.Checked = gpx.MarkerList[i].IsVisible;
                     }
                  }
                  break;

               case GpxAllExt.MarkerlistChangedEventArgs.Kind.Remove:
                  listChanged_Remove(tv, e.From);
                  break;

               case GpxAllExt.MarkerlistChangedEventArgs.Kind.Move:
                  TreeNode tn = tv.Nodes[e.From];
                  tv.Nodes.RemoveAt(e.From);
                  tv.Nodes.Insert(e.To, tn);

                  Marker marker = gpx.MarkerList[e.To];
                  ShowMarkerEvent?.Invoke(this, new MarkerEventArgs(marker, marker.IsVisible));

                  tv_SetSelectedIndex(tv, e.To);
                  break;
            }
      }

      void listChanged_Add(TreeView tv, int to, string txt) {
         if (0 <= to) {
            if (to < tv.Nodes.Count)
               tv.Nodes.Insert(to, txt);
            else
               tv.Nodes.Add(txt);
            tv_SetSelectedIndex(tv, to);
            tv.SelectedNode.Checked = true;
         }
      }

      void listChanged_Remove(TreeView tv, int from) {
         if (from >= 0)
            tv.Nodes.RemoveAt(from);
         else
            tv.Nodes.Clear();
         if (from > 0)
            tv_SetSelectedIndex(tv, from - 1);   // i.A. Vorgänger markieren
         else if (tv.Nodes.Count > 0)
            tv_SetSelectedIndex(tv, 0);
      }


      #endregion

      #region Events vom Track-TreeView

      #region Dra&Drop für Trackliste

      private void treeView_Tracks_DragDrop(object sender, DragEventArgs e) {
         TreeView tv = sender as TreeView;

         if (e.Effect == DragDropEffects.Move &&
             e.Data.GetDataPresent(typeof(Track))) {
            Track drag_track = (Track)e.Data.GetData(typeof(Track));
            int old_idx = gpx.TrackList.IndexOf(drag_track);
            TreeNode tn = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
            int new_idx = tn == null ?
                        tv.Nodes.Count - 1 :
                        tv.Nodes.IndexOf(tn);

            if (old_idx != new_idx) {
               TrackOrderChangedEvent?.Invoke(this, new OrderChangedEventArgs(old_idx, new_idx));
               tv_SetSelectedIndex(tv, new_idx);
            } else
               selectOnlyThisEditableTrack(drag_track);
         } else if (e.Effect == DragDropEffects.Copy &&
                    e.Data.GetDataPresent(DataFormats.FileDrop)) {
            insertFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
         }
      }

      private void treeView_Tracks_DragEnter(object sender, DragEventArgs e) {
         e.Effect = DragDropEffects.None;
         if (e.Data.GetDataPresent(typeof(Track)) &&              // richtiger Typ und ...
             (e.AllowedEffect & DragDropEffects.Move) != 0) {     // ... Move
            Track drag_item = (Track)e.Data.GetData(typeof(Track));
            if (gpx.TrackList.Contains(drag_item))
               e.Effect = DragDropEffects.Move;
         } else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   (e.AllowedEffect & DragDropEffects.Copy) != 0) {
            e.Effect = DragDropEffects.Copy;
         } else
            e.Effect = DragDropEffects.None;
      }

      private void treeView_Tracks_DragOver(object sender, DragEventArgs e) {
         if (e.Effect == DragDropEffects.Move) {
            TreeView tv = (sender as TreeView);
            tv.SelectedNode = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
         } else if (e.Effect == DragDropEffects.Copy) {

         }
      }

      #endregion

      /// <summary>
      /// Mausklick NICHT auf einen TreeNode
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void treeView_Tracks_MouseUp(object sender, MouseEventArgs e) {
         TreeView tv = sender as TreeView;
         if (tv.SelectedNode == null) {
            selectOnlyThisEditableTrack(null);
            SelectTrackEvent?.Invoke(this, new IdxEventArgs(-1));
         }
      }

      private void treeView_Tracks_AfterSelect(object sender, TreeViewEventArgs e) {
         int idx = tv_GetSelectedIndex(sender as TreeView);
         selectOnlyThisEditableTrack(0 <= idx && idx < gpx.TrackList.Count ?
                                          gpx.TrackList[idx] :
                                          null);
         SelectTrackEvent?.Invoke(this, new IdxEventArgs(tv_GetSelectedIndex(sender as TreeView)));
      }

      #endregion

      #region Events vom Marker-TreeView

      #region Dra&Drop für Markerliste

      private void treeView_Marker_DragDrop(object sender, DragEventArgs e) {
         TreeView tv = sender as TreeView;

         if (e.Effect == DragDropEffects.Move &&
             e.Data.GetDataPresent(typeof(Marker))) {
            Marker drag_marker = e.Data.GetData(typeof(Marker)) as Marker;
            int old_idx = gpx.MarkerList.IndexOf(drag_marker);
            TreeNode tn = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
            int new_idx = tn == null ?
                        tv.Nodes.Count - 1 :
                        tv.Nodes.IndexOf(tn);

            if (old_idx != new_idx) {
               MarkerOrderChangedEvent?.Invoke(this, new OrderChangedEventArgs(old_idx, new_idx));
               tv_SetSelectedIndex(tv, new_idx);
            }
         } else if (e.Effect == DragDropEffects.Copy &&
                    e.Data.GetDataPresent(DataFormats.FileDrop)) {
            insertFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
         }
      }

      private void treeView_Marker_DragEnter(object sender, DragEventArgs e) {
         e.Effect = DragDropEffects.None;
         if (e.Data.GetDataPresent(typeof(Marker)) &&    // richtiger Typ und ...
             (e.AllowedEffect & DragDropEffects.Move) != 0) {     // ... Move
            Marker drag_item = e.Data.GetData(typeof(Marker)) as Marker;
            if (gpx.MarkerList.Contains(drag_item))
               e.Effect = DragDropEffects.Move;
         } else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   (e.AllowedEffect & DragDropEffects.Copy) != 0) {
            e.Effect = DragDropEffects.Copy;
         } else
            e.Effect = DragDropEffects.None;
      }

      private void treeView_Marker_DragOver(object sender, DragEventArgs e) {
         if (e.Effect == DragDropEffects.Move) {
            TreeView tv = sender as TreeView;
            tv.SelectedNode = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
         }
      }

      #endregion

      #endregion

      #region Events für beide TreeViews

      /// <summary>
      /// für Kontextmenü 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_MouseDown(object sender, MouseEventArgs e) {
         if ((e.Button == MouseButtons.Right ||
              e.Button == MouseButtons.Left) &&             // damit ein Klick außerhalb der Liste zum "deselektieren" führt
             ModifierKeys == Keys.None) {                   // vor dem Aufruf des Kontextmenüs den TreeNode markieren

            TreeView tv = sender as TreeView;
            tv.SelectedNode = tv.GetNodeAt(e.Location);

            if (e.Button == MouseButtons.Right) {           // zum entsprechenden Kontextmenü
               if (tv.Equals(treeView_Tracks))
                  ShowContextmenu4TrackEvent?.Invoke(this, new TrackEventArgs(SelectedTrack));
               else
                  ShowContextmenu4MarkerEvent?.Invoke(this, new MarkerEventArgs(SelectedMarker));
            }
         }
      }

      /// <summary>
      /// zum Erkennen von F2
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
         TreeView tv = sender as TreeView;
         if (e.KeyCode == Keys.F2)
            if (tv.SelectedNode != null) {
               tv.LabelEdit = true;
               tv.SelectedNode.BeginEdit();
            }
      }

      /// <summary>
      /// Edit eines Node-Labels abgeschlossen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
         TreeView tv = sender as TreeView;
         if (e.Node != null &&
             e.Label != null) {
            string label = e.Label.Trim();
            if (label.Length == 0)
               e.CancelEdit = true;
            else {
               int idx = tv_GetSelectedIndex(tv);

               if (tv.Equals(treeView_Tracks)) {

                  Track track = gpx.TrackList[idx];
                  track.Trackname = label;
                  gpx.GpxDataChanged = true;
                  track.VisualName = label;
                  UpdateVisualTrackEvent?.Invoke(this, new TrackEventArgs(track));

               } else if (tv.Equals(treeView_Marker)) {

                  Marker marker = gpx.MarkerList[idx];
                  marker.Text = label;
                  UpdateVisualMarkerEvent?.Invoke(this, new MarkerEventArgs(marker));
               }
            }
         }
         tv.LabelEdit = false;
      }

      /// <summary>
      /// Reaktion auf Checkbox
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_AfterCheck(object sender, TreeViewEventArgs e) {
         TreeView tv = sender as TreeView;
         int idx = tv_GetIndex(e.Node);
         if (tv.Equals(treeView_Tracks)) {

            if (idx4TrackIsValid(idx))
               ShowTrackEvent?.Invoke(this, new TrackEventArgs(gpx.TrackList[idx], e.Node.Checked));

         } else if (tv.Equals(treeView_Marker)) {

            if (idx4MarkerIsValid(idx))
               ShowMarkerEvent?.Invoke(this, new MarkerEventArgs(gpx.MarkerList[idx], e.Node.Checked));

         }
      }

      /// <summary>
      /// Init DragDrop
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_ItemDrag(object sender, ItemDragEventArgs e) {
         TreeView tv = sender as TreeView;
         TreeNode tn = e.Item as TreeNode;
         if (tn != null) {
            tv.SelectedNode = tn;

            if (tv.Equals(treeView_Tracks))
               tv.DoDragDrop(gpx.TrackList[tv_GetSelectedIndex(tv)], DragDropEffects.Move);
            else if (tv.Equals(treeView_Marker))
               tv.DoDragDrop(gpx.MarkerList[tv_GetSelectedIndex(tv)], DragDropEffects.Move);
         } else
            tv.SelectedNode = null;
      }

      /// <summary>
      /// Info zum Objekt
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_Editable_DoubleClick(object sender, EventArgs e) {
         TreeView tv = sender as TreeView;
         if (tv.SelectedNode != null)
            if (tv.Equals(treeView_Tracks))
               ChooseTrackEvent?.Invoke(this, new IdxEventArgs(tv_GetSelectedIndex(tv)));
            else if (tv.Equals(treeView_Marker))
               ChooseMarkerEvent?.Invoke(this, new IdxEventArgs(tv_GetSelectedIndex(tv)));
      }

      #endregion

      #region allg. Hilfsfkt. für TreeViews

      /// <summary>
      /// setzt den ausgewählten TreeNode (der obersten Ebene) wenn idx gültig ist
      /// </summary>
      /// <param name="tv"></param>
      /// <param name="idx"></param>
      void tv_SetSelectedIndex(TreeView tv, int idx) {
         tv.SelectedNode = 0 <= idx && idx < tv.Nodes.Count ?
                              tv.Nodes[idx] :
                              null;
      }

      /// <summary>
      /// liefert den Index des ausgewählten TreeNode (der obersten Ebene) oder -1
      /// </summary>
      /// <param name="tv"></param>
      /// <returns></returns>
      int tv_GetSelectedIndex(TreeView tv) {
         return tv_GetIndex(tv.SelectedNode);
      }

      /// <summary>
      /// liefert den Index des TreeNode (der obersten Ebene) oder -1
      /// </summary>
      /// <param name="tn"></param>
      /// <returns></returns>
      int tv_GetIndex(TreeNode tn) {
         return tn != null ?
                     tn.TreeView.Nodes.IndexOf(tn) :
                     -1;
      }

      #endregion

      void insertFiles(string[] files) {
         foreach (var item in files) {
            try {
               string filename = PathHelper.GetFullPathAppliedCurrentDirectory(PathHelper.ReplaceEnvironmentVars(item));
               string ext = Path.GetExtension(filename).ToLower();
               if (ext == ".gpx" ||
                   ext == ".kml" ||
                   ext == ".kmz" ||
                   ext == ".gdb") { // einzelne GPX-Datei
                  GpxAllExt gpxnew = new GpxAllExt();
                  gpxnew.TrackColor = gpx.TrackColor;
                  gpxnew.TrackWidth = gpx.TrackWidth;
                  gpxnew.Load(filename, true);

                  for (int i = 0; i < gpxnew.TrackList.Count; i++)
                     gpx.TrackInsertCopy(gpxnew.TrackList[i], -1, true);

                  for (int i = 0; i < gpxnew.MarkerList.Count; i++)
                     gpx.MarkerInsertCopy(gpxnew.MarkerList[i]);
               }
            } catch (Exception ex) {
               MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
      }

      /// <summary>
      /// sorgt dafür, das nur dieser eine Track hervorgehoben wird, alle anderen nicht
      /// </summary>
      /// <param name="track"></param>
      void selectOnlyThisEditableTrack(Track track = null) {
         foreach (var t in gpx.TrackList) {
            if (!t.Equals(track) &&
                t.IsMarked4Edit) {
               t.IsMarked4Edit = false;
               UpdateVisualTrackEvent?.Invoke(this, new TrackEventArgs(t));
            }
         }
         if (track != null) {
            track.IsMarked4Edit = true;
            UpdateVisualTrackEvent?.Invoke(this, new TrackEventArgs(track));
         }
      }

      /// <summary>
      /// Ist der Index gültig?
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      bool idx4TrackIsValid(int idx) {
         return 0 <= idx && idx < Tracks;
      }

      /// <summary>
      /// Ist der Index gültig?
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      bool idx4MarkerIsValid(int idx) {
         return 0 <= idx && idx < Markers;
      }

      int idx4Track(Track track) {
         return gpx.TrackList.IndexOf(track);
      }

      int idx4Marker(Marker marker) {
         return gpx.MarkerList.IndexOf(marker);
      }

      #region public-Funktionen

      /// <summary>
      /// setzt einen Tracknamen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="name"></param>
      public void SetTrackName(int idx, string name) {
         if (idx4TrackIsValid(idx)) {
            if (treeView_Tracks.Nodes[idx].Text != name)
               treeView_Tracks.Nodes[idx].Text = name;
         }
      }

      public void SetTrackName(Track track, string name) {
         SetTrackName(idx4Track(track), name);
      }

      /// <summary>
      /// setzt die Auswahl auf den Track
      /// </summary>
      /// <param name="idx"></param>
      public void SelectTrack(int idx) {
         if (idx4TrackIsValid(idx))
            tv_SetSelectedIndex(treeView_Tracks, idx);
      }

      public void SelectTrack(Track track) {
         SelectTrack(idx4Track(track));
      }

      /// <summary>
      /// setzt die Sichtbarkeit des Tracks
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      public void ShowTrack(int idx, bool visible) {
         if (idx4TrackIsValid(idx) &&
             treeView_Tracks.Nodes[idx].Checked != visible)
            treeView_Tracks.Nodes[idx].Checked = visible;
      }

      public void ShowTrack(Track track, bool visible) {
         ShowTrack(idx4Track(track), visible);
      }


      /// <summary>
      /// setzt einen Markernamen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="name"></param>
      public void SetMarkerName(int idx, string name) {
         if (idx4MarkerIsValid(idx)) {
            if (treeView_Marker.Nodes[idx].Text != name)
               treeView_Marker.Nodes[idx].Text = name;
         }
      }

      public void SetMarkerName(Marker marker, string name) {
         SetMarkerName(idx4Marker(marker), name);
      }

      /// <summary>
      /// setzt die Auswahl auf den Marker
      /// </summary>
      /// <param name="idx"></param>
      public void SelectMarker(int idx) {
         if (idx4MarkerIsValid(idx))
            tv_SetSelectedIndex(treeView_Marker, idx);
      }

      public void SelectMarker(Marker marker) {
         SelectMarker(idx4Marker(marker));
      }

      /// <summary>
      /// setzt die Sichtbarkeit des Markers
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      public void ShowMarker(int idx, bool visible) {
         if (idx4MarkerIsValid(idx) &&
             treeView_Marker.Nodes[idx].Checked != visible)
            treeView_Marker.Nodes[idx].Checked = visible;
      }

      public void ShowMarker(Marker marker, bool visible) {
         ShowMarker(idx4Marker(marker), visible);
      }

      #endregion
   }
}
