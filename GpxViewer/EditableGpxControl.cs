using FSofTUtils;
using GpxViewer.Common;
using SpecialMapCtrl;
using System.Text;

namespace GpxViewer {
   public partial class EditableGpxControl : UserControl {

      GpxWorkbench? _wb;

      public GpxWorkbench? GpxWorkbench {
         get => _wb;
         set {
            if (_wb != null && _wb.Gpx != null) {
               _wb.Gpx.MarkerlistlistChanged -= Gpx_MarkerlistlistChanged;
               _wb.Gpx.TracklistChanged -= Gpx_TracklistChanged;
            }
            TVHelper.Remove(treeView_GeoObjects);
            _wb = value;
            if (_wb != null && _wb.Gpx != null) {
               // Strukturdaten der TreeViews einlesen (muss zur Gpx-Datei passen!)

               try {
                  TVHelper.ReadTreeViewStructur(treeView_GeoObjects,
                                                backupfileTreeView,
                                                GpxWorkbench.TrackList.Count,
                                                GpxWorkbench.MarkerList.Count);
               } catch { }

               for (int i = 0; i < Tracks; i++) {
                  TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Track, i);
                  Track? t = getTrack(i);
                  if (tn != null) {
                     tn.Text = t.VisualName;
                     setNodeImage(tn, t);
                     setTreeNodeVisibility(tn, t.IsVisible ? TVHelper.VisibilityStatus.On : TVHelper.VisibilityStatus.Off);
                     if (i == Tracks - 1)
                        TVHelper.Select4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Track, i);
                  } else {      // sollte nicht vorkommen, wenn die Struktur zu Gpx-Datei passt
                     TVHelper.Insert(treeView_GeoObjects,
                                     TVHelper.NodeType.Track,
                                     t.VisualName,
                                     i,
                                     i == Tracks - 1,
                                     t.IsVisible);
                     setNodeImage(tn, t);
                  }
               }

               for (int i = 0; i < Markers; i++) {
                  TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Marker, i);
                  Marker? m = getMarker(i);
                  if (tn != null) {
                     tn.Text = m.Text;
                     setNodeImage(tn, m);
                     setTreeNodeVisibility(tn, m.IsVisible ? TVHelper.VisibilityStatus.On : TVHelper.VisibilityStatus.Off);
                     if (i == Markers - 1)
                        TVHelper.Select4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Marker, i);
                  } else {      // sollte nicht vorkommen, wenn die Struktur zu Gpx-Datei passt
                     TVHelper.Insert(treeView_GeoObjects,
                                     TVHelper.NodeType.Marker,
                                     m.Text,
                                     i,
                                     i == Markers - 1,
                                     m.IsVisible);
                     setNodeImage(tn, m);
                  }
               }

               TVHelper.SaveTreeViewStructur(treeView_GeoObjects, backupfileTreeView);

               _wb.Gpx.MarkerlistlistChanged += Gpx_MarkerlistlistChanged;
               _wb.Gpx.TracklistChanged += Gpx_TracklistChanged;
            }
         }
      }

      public Color ListBackColor {
         get => treeView_GeoObjects.BackColor;
         set => treeView_GeoObjects.BackColor = value;
      }

      /// <summary>
      /// Anzahl der Tracks
      /// </summary>
      public int Tracks => GpxWorkbench != null ? GpxWorkbench.TrackList.Count : 0;

      /// <summary>
      /// Anzahl der Marker
      /// </summary>
      public int Markers => GpxWorkbench != null ? GpxWorkbench.MarkerList.Count : 0;

      /// <summary>
      /// akt. ausgewählter Track (oder null)
      /// </summary>
      public Track? SelectedTrack {
         get {
            int idx = TVHelper.GetListIdx4Selected(treeView_GeoObjects, out TVHelper.NodeType nodetype);
            if (nodetype == TVHelper.NodeType.Track)
               return idx4TrackIsValid(idx) ?
                           GpxWorkbench?.TrackList[idx] :
                           null;
            return null;
         }
      }

      /// <summary>
      /// akt. ausgewählter Marker (oder null)
      /// </summary>
      public Marker? SelectedMarker {
         get {
            int idx = TVHelper.GetListIdx4Selected(treeView_GeoObjects, out TVHelper.NodeType nodetype);
            if (nodetype == TVHelper.NodeType.Marker)
               return idx4MarkerIsValid(idx) ?
                        GpxWorkbench?.MarkerList[idx] :
                        null;
            return null;
         }
      }

      /// <summary>
      /// akt. ausgewählter Gruppe (oder null)
      /// </summary>
      public string? SelectedGroup {
         get {
            TVHelper.GetListIdx4Selected(treeView_GeoObjects, out TVHelper.NodeType nodetype);
            if (nodetype == TVHelper.NodeType.Group)
               return treeView_GeoObjects.SelectedNode.Text;
            return null;
         }
      }

      /// <summary>
      /// Ist aktuell ein Track ausgewählt?
      /// </summary>
      public bool IsTrackSelected => TVHelper.IsTreeNode4Track(treeView_GeoObjects.SelectedNode);

      /// <summary>
      /// Ist aktuell ein Marker ausgewählt?
      /// </summary>
      public bool IsMarkerSelected => TVHelper.IsTreeNode4Marker(treeView_GeoObjects.SelectedNode);

      /// <summary>
      /// Ist aktuell eine Gruppe ausgewählt?
      /// </summary>
      public bool IsGroupSelected => TVHelper.IsTreeNode4Group(treeView_GeoObjects.SelectedNode);

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
      public event EventHandler<OrderChangedEventArgs>? TrackOrderChangedEvent;

      /// <summary>
      /// Ein Marker hat sein Position in der Liste verändert.
      /// </summary>
      public event EventHandler<OrderChangedEventArgs>? MarkerOrderChangedEvent;

      /// <summary>
      /// Ein Track soll angezeigt oder verborgen werden.
      /// </summary>
      public event EventHandler<TrackEventArgs>? ShowTrackEvent;

      /// <summary>
      /// Ein Marker soll angezeigt oder verborgen werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs>? ShowMarkerEvent;

      /// <summary>
      /// Ein Track wurde markiert.
      /// </summary>
      public event EventHandler<IdxEventArgs>? SelectTrackEvent;

      /// <summary>
      /// Ein Marker wurde markiert.
      /// </summary>
      public event EventHandler<IdxEventArgs>? SelectMarkerEvent;

      /// <summary>
      /// Ein Track wurd mit Doppelklick ausgewählt.
      /// </summary>
      public event EventHandler<IdxEventArgs>? ChooseTrackEvent;

      /// <summary>
      /// Ein Marker wurd mit Doppelklick ausgewählt.
      /// </summary>
      public event EventHandler<IdxEventArgs>? ChooseMarkerEvent;

      /// <summary>
      /// Ein Kontextmenü für den Track sollte angezeigt werden.
      /// </summary>
      public event EventHandler<TrackEventArgs>? ShowContextmenu4TrackEvent;

      /// <summary>
      /// Ein Kontextmenü für den Marker sollte angezeigt werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs>? ShowContextmenu4MarkerEvent;

      /// <summary>
      /// Ein Kontextmenü für die Gruppe sollte angezeigt werden.
      /// </summary>
      public event EventHandler<EventArgs>? ShowContextmenu4GroupEvent;

      /// <summary>
      /// Ein Kontextmenü für "nichts" (leerer Bereich) sollte angezeigt werden.
      /// </summary>
      public event EventHandler<EventArgs>? ShowContextmenu4NothingEvent;

      /// <summary>
      /// Die Darstellung auf der Karte sollte für diesen Track akt. werden.
      /// </summary>
      public event EventHandler<TrackEventArgs>? UpdateVisualTrackEvent;

      /// <summary>
      /// Die Darstellung auf der Karte sollte für diesen Marker akt. werden.
      /// </summary>
      public event EventHandler<MarkerEventArgs>? UpdateVisualMarkerEvent;

      #endregion

      const string BACKUPFILETREEVIEW = "treeView";

      string backupfileTreeView = BACKUPFILETREEVIEW;

      bool isInternalChange = false;



      public EditableGpxControl() {
         InitializeComponent();

         TVHelper.PrepareTreeView(treeView_GeoObjects);
      }

      /// <summary>
      /// setzt den Pfad für die Strukturdatei des TreeViews
      /// </summary>
      /// <param name="path"></param>
      public void SetBackupPath(string path) {
         backupfileTreeView = Path.Combine(path, BACKUPFILETREEVIEW);
         //backupfileTreeViewTracks = Path.Combine(path, BACKUPFILETREEVIEWTRACKS);
         //backupfileTreeViewMarker = Path.Combine(path, BACKUPFILETREEVIEWMARKER);
      }

      #region public-Funktionen für Gruppen

      public bool InsertGroup(string groupname) => insertGroup(treeView_GeoObjects, groupname);

      public bool DeleteGroup() => deleteGroup(treeView_GeoObjects);

      #endregion

      #region public-Funktionen für Tracks

      /// <summary>
      /// setzt einen Tracknamen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="name"></param>
      public void SetTrackNameAndImage(int idx, string name) {
         TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Track, idx);
         if (tn != null) {
            if (tn.Text != name)
               tn.Text = name;
            Track? t = getTrack(idx);
            if (t != null)
               setNodeImage(tn, t);
         }
      }

      public void SetTrackNameAndImage(Track track, string name) => SetTrackNameAndImage(idx4Object(track), name);

      /// <summary>
      /// setzt die Auswahl auf den Track
      /// </summary>
      /// <param name="idx"></param>
      public void SelectTrack(int idx) {
         if (idx4TrackIsValid(idx)) {
            TVHelper.Select4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Track, idx);
            selectOnlyThisEditableTrack(GpxWorkbench?.TrackList[idx]);
            SelectTrackEvent?.Invoke(this, new IdxEventArgs(idx));
         }
      }

      /// <summary>
      /// setzt die Auswahl auf den Track
      /// </summary>
      /// <param name="track"></param>
      public void SelectTrack(Track track) => SelectTrack(idx4Object(track));

      /// <summary>
      /// setzt die Sichtbarkeit des Tracks in der zugehörige Listenanzeige 
      /// und ruft bei Bedarf <see cref="ShowTrackEvent"/> auf
      /// </summary>
      /// <param name="idx">Index für die Trackliste in der zugehörigen <see cref="GpxWorkbench"/></param>
      /// <param name="visible"></param>
      public void ShowTrack(int idx, bool visible) {
         TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Track, idx);
         if (tn != null) {
            bool on = TVHelper.GetVisibilityStatus(tn) == TVHelper.VisibilityStatus.On;
            if (on != visible)
               setTreeNodeVisibility(tn, visible);
         }
      }

      /// <summary>
      /// setzt die Sichtbarkeit des Tracks in der zugehörige Listenanzeige 
      /// und ruft bei Bedarf <see cref="ShowTrackEvent"/> auf
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      public void ShowTrack(Track track, bool visible) => ShowTrack(idx4Object(track), visible);

      #endregion

      #region public-Funktionen für Marker

      /// <summary>
      /// setzt einen Markernamen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="name"></param>
      public void SetMarkerNameAndImage(int idx, string name) {
         TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Marker, idx);
         if (tn != null) {
            if (tn.Text != name)
               tn.Text = name;
            Marker? m = getMarker(idx);
            if (m != null)
               setNodeImage(tn, m);
         }
      }

      public void SetMarkerNameAndImage(Marker marker, string name) => SetMarkerNameAndImage(idx4Object(marker), name);

      /// <summary>
      /// setzt die Auswahl auf den Marker
      /// </summary>
      /// <param name="idx"></param>
      public void SelectMarker(int idx) {
         if (idx4MarkerIsValid(idx))
            TVHelper.Select4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Marker, idx);
      }

      public void SelectMarker(Marker marker) => SelectMarker(idx4Object(marker));

      /// <summary>
      /// setzt die Sichtbarkeit des Markers
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      public void ShowMarker(int idx, bool visible) {
         TreeNode? tn = TVHelper.GetTreeNode4ListIdx(treeView_GeoObjects, TVHelper.NodeType.Marker, idx);
         if (tn != null) {
            bool on = TVHelper.GetVisibilityStatus(tn) == TVHelper.VisibilityStatus.On;
            if (on != visible)
               setTreeNodeVisibility(tn, visible);
         }
      }

      public void ShowMarker(Marker marker, bool visible) => ShowMarker(idx4Object(marker), visible);

      #endregion

      #region public-Funktionen für Info-Textbox

      public void InfotextClear() => textBox_Info.Clear();

      public void InfoTextAppend(string txt) {
         textBox_Info.AppendText(txt);
         textBox_Info.ScrollToCaret();
      }

      #endregion

      #region Dra&Drop (Windows)

      /// <summary>
      /// ein Datenobjekt wurde per DragDrop "fallen gelassen"
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void EditableTracklistControl_DragDrop(object sender, DragEventArgs e) {
         importFiles(null, e.Data.GetData(DataFormats.FileDrop, false) as string[], null, true);
      }

      /// <summary>
      /// ein Datenobjekt wurde per Drag in das Fenster gezogen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void EditableTracklistControl_DragEnter(object sender, DragEventArgs e) {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
         else
            e.Effect = DragDropEffects.None;
      }

      #endregion

      #region Events von GpxAllExt

      private void Gpx_TracklistChanged(object? sender, GpxData.ObjectListChangedEventArgs e) {
         // Der Auruf erfolgt nicht beim Setzen der Variable Gpx, d.h. das schon vorher erfolgte Einlesen der Daten
         // spielt hier keine Rolle mehr!

         if (!isInternalChange) {
            GpxData? gpx = sender as GpxData;
            TreeView tv = treeView_GeoObjects;
            Track? track;

            // Listbox anpassen
            if (e != null) {
               switch (e.KindOfChanging) {
                  case GpxData.ObjectListChangedEventArgs.Kind.Add:
                     switch (e.To) {
                        case GpxData.ObjectListChangedEventArgs.LISTPOS_ALL:   // gesamte Trackliste neu übernehmen
                           TVHelper.Remove(tv, TVHelper.NodeType.Track);
                           for (int i = 0; i < Tracks; i++) {
                              track = getTrack(i);
                              TVHelper.Insert(tv,
                                              TVHelper.NodeType.Track,
                                              track.VisualName,
                                              i,
                                              i == Tracks - 1,
                                              track.IsVisible);
                              TreeNode? tn = TVHelper.GetTreeNode4ListIdx(tv, TVHelper.NodeType.Track, gpx.TrackIndex(track));
                              if (tn != null) {
                                 setTreeNodeVisibility(tn, true);
                                 setNodeImage(tn, track);
                              }
                           }
                           adaptParentTreeNodeVisibility(treeView_GeoObjects);
                           break;

                        default:          // einzelnen Track aufnehmen
                           if (0 <= e.To) {
                              track = getTrack(e.To);
                              // jetzt auch in den TreeView aufnehemn
                              TVHelper.Insert(tv,
                                              TVHelper.NodeType.Track,
                                              track.VisualName,
                                              e.To,
                                              true,
                                              track.IsVisible);
                              TreeNode? tn = TVHelper.GetTreeNode4ListIdx(tv, TVHelper.NodeType.Track, gpx.TrackIndex(track));
                              if (tn != null) {
                                 if (track.GpxSegment.Points.Count == 0) { // i.A. ein Track der neu angelegt wurde -> immer an die 1. Pos. verschieben
                                    TreeNode nextnode = tv.Nodes[0];
                                    if (tn.Equals(nextnode)) {
                                       if (1 < tv.Nodes.Count)
                                          nextnode = tv.Nodes[1];
                                    }
                                    if (!tn.Equals(nextnode))
                                       doChanges(TVHelper.MoveTo(tv, tn, nextnode, true).TrackChanges, true);
                                 }
                                 setTreeNodeVisibility(tn, true);
                                 adaptParentTreeNodeVisibility(treeView_GeoObjects);
                                 setNodeImage(tn, track);
                                 tv.SelectedNode = tn;
                              }
                              selectOnlyThisEditableTrack(track);
                              SelectTrackEvent?.Invoke(this, new IdxEventArgs(e.To));
                           }
                           break;
                     }
                     break;

                  case GpxData.ObjectListChangedEventArgs.Kind.Remove:
                     listChanged_Remove(tv, TVHelper.NodeType.Track, e.From);
                     break;

                  case GpxData.ObjectListChangedEventArgs.Kind.Move:
                     TVHelper.Move(tv, TVHelper.NodeType.Track, e.From, e.To);

                     track = getTrack(e.To);
                     if (track.IsVisible) { // Neuanzeigen wegen Veränderung der Reihenfolge
                        ShowTrackEvent?.Invoke(this, new TrackEventArgs(track, false));
                        ShowTrackEvent?.Invoke(this, new TrackEventArgs(track, true));
                     }

                     TVHelper.Select4ListIdx(tv, TVHelper.NodeType.Track, e.To);
                     adaptParentTreeNodeVisibility(treeView_GeoObjects);
                     break;

               }
               TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
            }
         }
      }

      private void Gpx_MarkerlistlistChanged(object? sender, GpxData.ObjectListChangedEventArgs e) {
         if (!isInternalChange) {
            GpxData? gpx = sender as GpxData;
            TreeView tv = treeView_GeoObjects;
            Marker? marker;

            if (gpx != null && e != null) {
               switch (e.KindOfChanging) {
                  case GpxData.ObjectListChangedEventArgs.Kind.Add:
                     switch (e.To) {
                        case GpxData.ObjectListChangedEventArgs.LISTPOS_ALL:   // gesamte Trackliste neu übernehmen
                           TVHelper.Remove(tv, TVHelper.NodeType.Marker);
                           for (int i = 0; i < Markers; i++) {
                              marker = getMarker(i);
                              TVHelper.Insert(tv,
                                              TVHelper.NodeType.Marker,
                                              marker.Text,
                                              i,
                                              i == Markers - 1,
                                              marker.IsVisible);
                              TreeNode? tn = TVHelper.GetTreeNode4ListIdx(tv, TVHelper.NodeType.Track, gpx.MarkerIndex(marker));
                              if (tn != null) {
                                 setTreeNodeVisibility(tn, true);
                                 setNodeImage(tn, marker);
                              }
                           }
                           adaptParentTreeNodeVisibility(treeView_GeoObjects);
                           break;

                        default:          // einzelnen Marker aufnehmen
                           if (0 <= e.To) {
                              marker = getMarker(e.To);
                              TVHelper.Insert(tv,
                                              TVHelper.NodeType.Marker,
                                              marker.Text,
                                              e.To,
                                              true,
                                              marker.IsVisible);
                              TreeNode? tn = TVHelper.GetTreeNode4ListIdx(tv, TVHelper.NodeType.Marker, gpx.MarkerIndex(marker));
                              if (tn != null) {
                                 if (e.To == 0) { // i.A. ein Marker der neu angelegt wurde -> immer an die 1. Pos. verschieben
                                    TreeNode nextnode = tv.Nodes[0];
                                    if (tn.Equals(nextnode)) {
                                       if (1 < tv.Nodes.Count)
                                          nextnode = tv.Nodes[1];
                                    }
                                    if (!tn.Equals(nextnode))
                                       doChanges(TVHelper.MoveTo(tv, tn, nextnode, true).MarkerChanges, false);
                                 }
                                 setTreeNodeVisibility(tn, true);
                                 adaptParentTreeNodeVisibility(treeView_GeoObjects);
                                 setNodeImage(tn, marker);
                                 tv.SelectedNode = tn;
                              }
                           }
                           break;
                     }
                     break;

                  case GpxData.ObjectListChangedEventArgs.Kind.Remove:
                     listChanged_Remove(tv, TVHelper.NodeType.Marker, e.From);
                     break;

                  case GpxData.ObjectListChangedEventArgs.Kind.Move:
                     TVHelper.Move(tv, TVHelper.NodeType.Marker, e.From, e.To);

                     marker = getMarker(e.To);
                     if (marker.IsVisible) { // Neuanzeigen wegen Veränderung der Reihenfolge
                        ShowMarkerEvent?.Invoke(this, new MarkerEventArgs(marker, false));
                        ShowMarkerEvent?.Invoke(this, new MarkerEventArgs(marker, true));
                     }

                     TVHelper.Select4ListIdx(tv, TVHelper.NodeType.Marker, e.To);
                     adaptParentTreeNodeVisibility(treeView_GeoObjects);
                     break;
               }
               TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
            }
         }
      }

      /// <summary>
      /// in der Liste im <see cref="GpxData"/> wurde das Objekt am Index entfernt
      /// </summary>
      /// <param name="tv"></param>
      /// <param name="from"></param>
      void listChanged_Remove(TreeView tv, TVHelper.NodeType nodeType, int from) {
         if (from >= 0) {
            TreeNode? tn = TVHelper.GetTreeNode4ListIdx(tv, nodeType, from);
            if (tn != null) {
               TreeNode? parent = tn.Parent;
               TVHelper.Remove(tn);
               adaptParentTreeNodeVisibility(parent);
               tv.SelectedNode = TVHelper.GetTreeNode4ListIdx(tv, nodeType, from);
               int idx = TVHelper.GetListIdx4Selected(tv, out nodeType);
               if (nodeType == TVHelper.NodeType.Track)
                  selectOnlyThisEditableTrack(getTrack(idx));
               if (nodeType == TVHelper.NodeType.Marker)
                  SelectMarkerEvent?.Invoke(this, new IdxEventArgs(idx));

            }
         } else
            TVHelper.Remove(tv);
         if (tv.SelectedNode == null) {
            if (nodeType == TVHelper.NodeType.Track)
               SelectTrackEvent?.Invoke(this, new IdxEventArgs(-1));
            if (nodeType == TVHelper.NodeType.Marker)
               SelectMarkerEvent?.Invoke(this, new IdxEventArgs(-1));
         }
      }

      #endregion

      #region Events für TreeView

      #region Dra&Drop

      /// <summary>
      /// ein Datenobjekt wird über den TreeView gezogen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_DragOver(object sender, DragEventArgs e) {
         switch (e.Effect) {
            case DragDropEffects.Move:                   // der TreeNode unter der Maus wird ausgewählt
               TreeView? tv = sender as TreeView;
               tv.SelectedNode = tv?.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
               break;
         }
      }

      /// <summary>
      /// ein Datenobjekt wurde per Drag in den TreeView gezogen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_DragEnter(object sender, DragEventArgs e) {
         e.Effect = DragDropEffects.None;

         if ((e.Data.GetDataPresent(typeof(Track)) || e.Data.GetDataPresent(typeof(Marker))) &&              // richtiger Typ und ...
             (e.AllowedEffect & DragDropEffects.Move) != 0) {     // ... Move

            if (e.Data.GetDataPresent(typeof(Track))) {
               Track? drag_item = (Track?)e.Data.GetData(typeof(Track));
               if (drag_item != null && listContains(drag_item))
                  e.Effect = DragDropEffects.Move;
            } else {
               Marker? drag_item = (Marker?)e.Data.GetData(typeof(Marker));
               if (drag_item != null && listContains(drag_item))
                  e.Effect = DragDropEffects.Move;
            }

         } else if (e.Data.GetDataPresent(typeof(TreeNode)) &&       // "Gruppe" und ...
                   (e.AllowedEffect & DragDropEffects.Move) != 0) {  // ... Move

            e.Effect = DragDropEffects.Move;

         } else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   (e.AllowedEffect & DragDropEffects.Copy) != 0) {  // zum Hereinziehen von Dateien

            e.Effect = DragDropEffects.Copy;

         }
      }

      /// <summary>
      /// ein Datenobjekt wurde per DragDrop im TreeView "fallen gelassen"
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_DragDrop(object sender, DragEventArgs e) {
         TreeView? tv = sender as TreeView;
         if (tv != null) {
            TreeNode? tndest = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));

            /* Beim Drag&Drop können verschiedene Objekte geliefert werden: 
             *    Tracks/Marker, 
             *    (Gruppen-)TreeNode, 
             *    Dateinamen-Array
            */

            // Daten holen
            TreeNode? tndragobj = null;
            TreeNode? tndraggroup = null;
            string[]? dragfiles = null;

            if (e.Data.GetDataPresent(typeof(Track))) {                 // 1 einzelner Track wird verschoben
               Track? track = e.Data.GetData(typeof(Track)) as Track;
               if (track != null)
                  tndragobj = TVHelper.GetTreeNode4ListIdx(tv,
                                                           TVHelper.NodeType.Track,
                                                           idx4Object(track));
            } else if (e.Data.GetDataPresent(typeof(Marker))) {         // 1 einzelner Marker wird verschoben
               Marker? marker = e.Data.GetData(typeof(Marker)) as Marker;
               if (marker != null)
                  tndragobj = TVHelper.GetTreeNode4ListIdx(tv,
                                                      TVHelper.NodeType.Marker,
                                                      idx4Object(marker));
            } else if (e.Data.GetDataPresent(typeof(TreeNode))) {       // eine Gruppe wird verschoben
               tndraggroup = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            } else if (e.Effect == DragDropEffects.Copy &&
                       e.Data.GetDataPresent(DataFormats.FileDrop)) {   // 1 oder mehrere Dateien werden eingefügt
               dragfiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            }

            bool alt = ModifierKeys == Keys.Alt;    // zusätzlich (nur) die Alt-Taste gedrückt
            bool preinsert = TVHelper.IsTreeNode4Group(tndest) ?
                                 alt :          // Standard: darunter einfügen
                                 !alt;          // Standard: davor einfügen

            UseWaitCursor = true;

            // Aktion ausführen
            if (tndragobj != null ||            // Track/Marker
                tndraggroup != null) {          // Gruppe

               if (tndragobj == null)
                  tndragobj = tndraggroup;

               TVHelper.IdxListChanges changes = TVHelper.MoveTo(tv, tndragobj, tndest, preinsert);
               doChanges(changes.TrackChanges, true);
               doChanges(changes.MarkerChanges, false);
               adaptParentTreeNodeVisibility(treeView_GeoObjects);

               tv.SelectedNode = tndragobj;
               tv.SelectedNode.EnsureVisible();

            } else if (dragfiles != null) {     // Dateien

               importFiles(tv, dragfiles, tndest, preinsert);

            }

            UseWaitCursor = false;

            TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
         }
      }

      #endregion

      /// <summary>
      /// für Kontextmenü 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_MouseDown(object sender, MouseEventArgs e) {
         if ((e.Button == MouseButtons.Right ||
              e.Button == MouseButtons.Left) &&             // damit ein Klick außerhalb der Liste zum "deselektieren" führt
             ModifierKeys == Keys.None) {                   // vor dem Aufruf des Kontextmenüs den TreeNode markieren

            TreeView? tv = sender as TreeView;
            if (tv != null) {
               tv.SelectedNode = tv.GetNodeAt(e.Location);     // ev. auch null

               switch (e.Button) {
                  case MouseButtons.Left:
                     switch (tv.HitTest(e.Location).Location) {
                        case TreeViewHitTestLocations.StateImage:
                           checkboxChange(tv.SelectedNode);
                           break;

                        case TreeViewHitTestLocations.None:
                           selectOnlyThisEditableTrack(null);
                           SelectTrackEvent?.Invoke(this, new IdxEventArgs(-1));
                           SelectMarkerEvent?.Invoke(this, new IdxEventArgs(-1));
                           break;
                     }
                     break;

                  case MouseButtons.Right:                     // zum entsprechenden Kontextmenü
                     if (tv.SelectedNode == null) {
                        ShowContextmenu4NothingEvent.Invoke(this, EventArgs.Empty);
                     } else if (TVHelper.IsTreeNode4Track(tv.SelectedNode) && SelectedTrack != null) {
                        ShowContextmenu4TrackEvent?.Invoke(this, new TrackEventArgs(SelectedTrack));
                     } else if (TVHelper.IsTreeNode4Marker(tv.SelectedNode) && SelectedMarker != null) {
                        ShowContextmenu4MarkerEvent?.Invoke(this, new MarkerEventArgs(SelectedMarker));
                     } else { // Gruppe
                        ShowContextmenu4GroupEvent.Invoke(this, EventArgs.Empty);
                     }
                     break;
               }
            }
         }
      }

      private void tv_KeyDown(object sender, KeyEventArgs e) {
         if ((TreeView)sender != null) {
            TreeView tv = (TreeView)sender;
            if (tv.SelectedNode != null) {
               TreeNode tn = tv.SelectedNode;
               switch (e.KeyData) {
                  case Keys.Space:
                     checkboxChange(tn);
                     e.Handled = true;
                     break;

                  case Keys.F2:
                     if (!tv.LabelEdit) {
                        tv.LabelEdit = true;
                        tv.SelectedNode.BeginEdit();
                        e.Handled = true;
                     }
                     break;
               }
            }
         }
      }

      /// <summary>
      /// Edit eines Node-Labels abgeschlossen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
         TreeView? tv = sender as TreeView;
         if (tv != null &&
             e.Node != null &&
             e.Label != null) {
            string label = e.Label.Trim();
            if (label.Length == 0)
               e.CancelEdit = true;
            else {
               int idx = TVHelper.GetListIdx4Selected(tv, out TVHelper.NodeType nodetype);
               switch (nodetype) {
                  case TVHelper.NodeType.Track:
                     if (0 <= idx) {
                        Track? track = getTrack(idx);
                        if (track != null) {
                           track.Trackname = label;
                           GpxWorkbench.DataChanged = true;
                           track.VisualName = label;
                           UpdateVisualTrackEvent?.Invoke(this, new TrackEventArgs(track));
                           SelectTrackEvent?.Invoke(this, new IdxEventArgs(idx));
                        }
                     }
                     break;

                  case TVHelper.NodeType.Marker:
                     if (0 <= idx) {
                        Marker? marker = getMarker(idx);
                        if (marker != null) {
                           marker.Text = label;
                           UpdateVisualMarkerEvent?.Invoke(this, new MarkerEventArgs(marker));
                           SelectMarkerEvent?.Invoke(this, new IdxEventArgs(idx));
                        }
                     }
                     break;

                  case TVHelper.NodeType.Group:
                     e.Node.Text = label;       // MIST: Text ist noch NICHT in den Node übernommen
                     TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
                     break;

               }
            }
         }
         tv.LabelEdit = false;
      }

      /// <summary>
      /// Reaktion auf die Auswahl eines <see cref="TreeNode"/>
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_AfterSelect(object sender, TreeViewEventArgs e) {
         int idx = TVHelper.GetListIdx4TreeNode(e.Node, out TVHelper.NodeType nodetype);
         switch (nodetype) {
            case TVHelper.NodeType.Track:
               SelectMarkerEvent?.Invoke(this, new IdxEventArgs(-1));
               if (0 <= idx && idx < Tracks) {
                  selectOnlyThisEditableTrack(getTrack(idx));
                  SelectTrackEvent?.Invoke(this, new IdxEventArgs(idx));
               }
               break;

            case TVHelper.NodeType.Marker:
               selectOnlyThisEditableTrack(null);
               SelectTrackEvent?.Invoke(this, new IdxEventArgs(-1));
               if (0 <= idx && idx < Markers)
                  SelectMarkerEvent?.Invoke(this, new IdxEventArgs(idx));
               break;

            case TVHelper.NodeType.Group:
               selectOnlyThisEditableTrack(null);
               SelectTrackEvent?.Invoke(this, new IdxEventArgs(-1));
               SelectMarkerEvent?.Invoke(this, new IdxEventArgs(-1));
               break;
         }
      }

      /// <summary>
      /// Init DragDrop
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_ItemDrag(object sender, ItemDragEventArgs e) {
         TreeView? tv = sender as TreeView;
         TreeNode? tn = e.Item as TreeNode;
         if (tv != null &&
             tn != null) {
            tv.SelectedNode = tn;

            int idx = TVHelper.GetListIdx4Selected(tv, out TVHelper.NodeType nodetype);
            object? data = null;
            switch (nodetype) {
               case TVHelper.NodeType.Track:
                  data = getTrack(idx);
                  break;

               case TVHelper.NodeType.Marker:
                  data = getMarker(idx);
                  break;

               case TVHelper.NodeType.Group:
                  data = tn;     // Gruppe
                  break;
            }

            if (data != null)
               tv.DoDragDrop(data, DragDropEffects.Move);

         } else
            tv.SelectedNode = null;
      }

      /// <summary>
      /// Info zum Objekt
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tv_DoubleClick(object sender, EventArgs e) {
         TreeView? tv = sender as TreeView;
         if (tv != null &&
             tv.SelectedNode != null) {
            int idx = TVHelper.GetListIdx4Selected(tv, out TVHelper.NodeType nodetype);
            switch (nodetype) {
               case TVHelper.NodeType.Track:
                  ChooseTrackEvent?.Invoke(this, new IdxEventArgs(idx));
                  break;

               case TVHelper.NodeType.Marker:
                  ChooseMarkerEvent?.Invoke(this, new IdxEventArgs(idx));
                  break;
            }
         }
      }

      #endregion

      /// <summary>
      /// explizite Änderung des Checkboxstatus per Maus/Tastatur
      /// </summary>
      /// <param name="tn"></param>
      void checkboxChange(TreeNode? tn) {
         if (tn != null) {
            UseWaitCursor = true;
            switch (TVHelper.GetVisibilityStatus(tn)) {
               case TVHelper.VisibilityStatus.On:
                  setTreeNodeVisibility(tn, TVHelper.VisibilityStatus.Off);
                  break;

               case TVHelper.VisibilityStatus.Off:
               case TVHelper.VisibilityStatus.Mixed:
                  setTreeNodeVisibility(tn, TVHelper.VisibilityStatus.On);
                  break;
            }
            UseWaitCursor = false;
         }
      }

      /// <summary>
      /// Der <see cref="TVHelper.VisibilityStatus"/> eines TreeNode wird explizit gesetzt. Falls notwendig
      /// wird auch der <see cref="TVHelper.VisibilityStatus"/> übergeordneter Gruppen-TreeNode angepasst.
      /// </summary>
      /// <param name="tn"></param>
      /// <param name="visibility"></param>
      void setTreeNodeVisibility(TreeNode tn, TVHelper.VisibilityStatus visibility) {
         if (TVHelper.GetVisibilityStatus(tn) != visibility) {
            int idx = TVHelper.GetListIdx4TreeNode(tn, out TVHelper.NodeType nodetype);
            switch (nodetype) {
               case TVHelper.NodeType.Track:
                  if (idx4TrackIsValid(idx) &&
                      (visibility == TVHelper.VisibilityStatus.On || visibility == TVHelper.VisibilityStatus.Off)) {
                     TVHelper.SetVisibilityStatus(tn, visibility);
                     Track? track = getTrack(idx);
                     if (track != null)
                        ShowTrackEvent?.Invoke(this, new TrackEventArgs(track, visibility == TVHelper.VisibilityStatus.On));
                     adaptParentTreeNodeVisibility(tn.Parent);
                  }
                  break;

               case TVHelper.NodeType.Marker:
                  if (idx4MarkerIsValid(idx) &&
                      (visibility == TVHelper.VisibilityStatus.On || visibility == TVHelper.VisibilityStatus.Off)) {
                     TVHelper.SetVisibilityStatus(tn, visibility);
                     Marker? marker = getMarker(idx);
                     if (marker != null)
                        ShowMarkerEvent?.Invoke(this, new MarkerEventArgs(marker, visibility == TVHelper.VisibilityStatus.On));
                     adaptParentTreeNodeVisibility(tn.Parent);
                  }
                  break;

               case TVHelper.NodeType.Group:
                  TVHelper.SetVisibilityStatus(tn, visibility);
                  // Node für Gruppe -> Status für alle Subnodes übernehmen
                  if (visibility == TVHelper.VisibilityStatus.On || visibility == TVHelper.VisibilityStatus.Off) {
                     foreach (TreeNode item in tn.Nodes)
                        setTreeNodeVisibility(item, visibility);
                  }
                  adaptParentTreeNodeVisibility(tn.Parent);
                  break;
            }

         }
      }

      /// <summary>
      /// vereinfachte Variante von <see cref="setTreeNodeVisibility(TreeNode, TVHelper.VisibilityStatus)"/>
      /// </summary>
      /// <param name="tn"></param>
      /// <param name="on"></param>
      void setTreeNodeVisibility(TreeNode? tn, bool on) {
         if (tn != null)
            setTreeNodeVisibility(tn, on ? TVHelper.VisibilityStatus.On : TVHelper.VisibilityStatus.Off);
      }

      /// <summary>
      /// zur eventuellen Anpassung des <see cref="TVHelper.VisibilityStatus"/> übergeordneter Gruppen-TreeNode
      /// </summary>
      /// <param name="tn"></param>
      void adaptParentTreeNodeVisibility(TreeNode? tn) {
         if (tn != null) {
            TVHelper.VisibilityStatus? visibility = null;
            foreach (TreeNode child in tn.Nodes) {
               if (visibility == null)
                  visibility = TVHelper.GetVisibilityStatus(child);
               else if (visibility != TVHelper.GetVisibilityStatus(child)) {
                  visibility = TVHelper.VisibilityStatus.Mixed;
                  break;
               }
            }
            if (visibility != null)
               setTreeNodeVisibility(tn, (TVHelper.VisibilityStatus)visibility);
         }
      }

      void adaptParentTreeNodeVisibility(TreeView tv) {
         List<TreeNode> lst = new List<TreeNode>();
         sampleGroupNodes(tv.Nodes, lst);
         //for (int i = lst.Count - 1; 0 <= i; i--)
         //   if (lst.Contains(lst[i].Parent))

         foreach (TreeNode tn in lst)
            adaptParentTreeNodeVisibility(tn);
      }

      void sampleGroupNodes(TreeNodeCollection tnc, List<TreeNode> lst) {
         foreach (TreeNode tn in tnc) {
            if (TVHelper.IsTreeNode4Group(tn)) {
               lst.Add(tn);
               sampleGroupNodes(tn.Nodes, lst);
            }
         }
      }

      private void doChanges(List<TVHelper.IdxListChanges.Change> changes, bool changes4tracks) {
         isInternalChange = true;
         for (int i = 0; i < changes.Count; i++) {
            if (0 <= changes[i].FromIdx &&
                0 <= changes[i].ToIdx)       // Verschiebung
               if (changes4tracks) {
                  Track t = GpxWorkbench.Gpx.TrackList[changes[i].FromIdx];
                  bool visible = t.IsVisible;
                  if (visible)
                     ShowTrackEvent?.Invoke(this, new TrackEventArgs(t, false));

                  GpxWorkbench.Gpx.TrackOrderChangeWithLock(changes[i].FromIdx, changes[i].ToIdx);

                  if (visible)
                     ShowTrackEvent?.Invoke(this, new TrackEventArgs(t, true));
               } else
                  GpxWorkbench.Gpx.MarkerOrderChangeWithLock(changes[i].FromIdx, changes[i].ToIdx);

            else if (0 <= changes[i].FromIdx &&
                     changes[i].ToIdx < 0) {       // Löschen
               if (changes4tracks) {
                  Track? track = GpxWorkbench.GetTrack(changes[i].FromIdx);
                  if (track != null)
                     GpxWorkbench.TrackRemove(track);
               } else {
                  Marker? marker = GpxWorkbench.GetMarker(changes[i].FromIdx);
                  if (marker != null)
                     GpxWorkbench.MarkerRemove(marker);
               }
            }
         }
         isInternalChange = false;
      }

      /// <summary>
      /// importiert die Daten aus den Dateien (Tracks und Marker) in einem eigenen Task 
      /// und fügt sie bezüglich <paramref name="tndest"/> davor oder danach im
      /// Treeview ein
      /// </summary>
      /// <param name="tv"></param>
      /// <param name="files"></param>
      /// <param name="tndest">Bezugsnode oder null beim Einfügen "am Ende"</param>
      /// <param name="preinsert">vor oder nach <paramref name="tndest"/> einfügen</param>
      void importFiles(TreeView? tv, string[]? files, TreeNode? tndest, bool preinsert) {
         if (files != null) {
            string[] importfiles = new string[files.Length];
            files.CopyTo(importfiles, 0);

            // einen Task im gleichen Thread starten
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task task = new Task(() => {
               insertFiles(tv, importfiles, tndest, preinsert);
            });
            if (scheduler != null)
               task.Start(scheduler);
            else
               task.Start();
         }
      }

      /// <summary>
      /// importiert die Daten aus den Dateien (Tracks und Marker) und fügt sie bezüglich <paramref name="tndest"/> davor oder danach im
      /// Treeview ein
      /// </summary>
      /// <param name="tv"></param>
      /// <param name="files"></param>
      /// <param name="tndest">Bezugsnode oder null beim Einfügen "am Ende"</param>
      /// <param name="preinsert">vor oder nach <paramref name="tndest"/> einfügen</param>
      void insertFiles(TreeView? tv, string[] files, TreeNode? tndest, bool preinsert) {
         foreach (var item in files) {
            try {
               string filename = PathHelper.GetFullPathAppliedCurrentDirectory(PathHelper.ReplaceEnvironmentVars(item));
               string ext = Path.GetExtension(filename).ToLower();
               if (ext == ".gpx" ||
                   ext == ".kml" ||
                   ext == ".kmz" ||
                   ext == ".gdb") { // einzelne GPX-Datei
                  GpxData gpxnew = new GpxData();
                  gpxnew.TrackColor = GpxWorkbench.Gpx.TrackColor;
                  gpxnew.TrackWidth = GpxWorkbench.Gpx.TrackWidth;
                  List<System.Drawing.Color> trackcolors = gpxnew.Load(filename, true, GpxWorkbench.Gpx.TrackColor);
                  bool move = tv != null;

                  // Daten aus gpxnew übertragen
                  TreeView tvlst = treeView_GeoObjects;
                  for (int i = 0; i < gpxnew.TrackList.Count; i++) {
                     Track t = GpxWorkbench.Gpx.TrackInsertCopyWithLock(gpxnew.TrackList[i], -1, true);   // anhängen
                     t.LineColor = trackcolors[i];

                     TreeNode? tntrack = TVHelper.GetTreeNode4ListIdx(tvlst, TVHelper.NodeType.Track, Tracks - 1);
                     if (move)    // dann zur Zielpos. verschieben
                        doChanges(TVHelper.MoveTo(tvlst, tntrack, tndest, preinsert).TrackChanges, true);
                     tvlst.SelectedNode = tntrack;
                     setTreeNodeVisibility(tntrack, true);
                     tntrack.EnsureVisible();
                  }

                  tvlst = treeView_GeoObjects;
                  for (int i = 0; i < gpxnew.MarkerList.Count; i++) {
                     Marker m = GpxWorkbench.Gpx.MarkerInsertCopyWithLock(gpxnew.MarkerList[i], -1, Marker.MarkerType.EditableStandard);

                     TreeNode? tnmarker = TVHelper.GetTreeNode4ListIdx(tvlst, TVHelper.NodeType.Marker, Markers - 1);
                     if (move)    // dann zur Zielpos. verschieben
                        doChanges(TVHelper.MoveTo(tvlst, tnmarker, tndest, preinsert).MarkerChanges, false);
                     tvlst.SelectedNode = tnmarker;
                     setTreeNodeVisibility(tnmarker, true);
                     tnmarker.EnsureVisible();
                  }

                  adaptParentTreeNodeVisibility(treeView_GeoObjects);
               }
               TVHelper.SaveTreeViewStructur(treeView_GeoObjects, backupfileTreeView);
            } catch (Exception ex) {
               MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
      }

      /// <summary>
      /// sorgt dafür, das kein bzw. nur dieser eine Track <see cref="Track.IsMarked4Edit"/> erhält 
      /// </summary>
      /// <param name="track"></param>
      void selectOnlyThisEditableTrack(Track? track = null) {
         foreach (var t in GpxWorkbench.TrackList) {
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
      bool idx4TrackIsValid(int idx) => 0 <= idx && idx < Tracks;

      /// <summary>
      /// Ist der Index gültig?
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      bool idx4MarkerIsValid(int idx) => 0 <= idx && idx < Markers;

      int idx4Object(Track track) => GpxWorkbench != null ? GpxWorkbench.TrackList.IndexOf(track) : 0;

      int idx4Object(Marker marker) => GpxWorkbench != null ? GpxWorkbench.MarkerList.IndexOf(marker) : 0;

      Track? getTrack(int i) => GpxWorkbench != null && 0 <= i && i < GpxWorkbench.TrackList.Count ? GpxWorkbench.TrackList[i] : null;

      Marker? getMarker(int i) => GpxWorkbench != null && 0 <= i && i < GpxWorkbench.MarkerList.Count ? GpxWorkbench.MarkerList[i] : null;

      bool listContains(Track t) => GpxWorkbench != null ? GpxWorkbench.TrackList.Contains(t) : false;

      bool listContains(Marker m) => GpxWorkbench != null ? GpxWorkbench.Gpx.MarkerList.Contains(m) : false;

      bool insertGroup(TreeView tv, string groupname) {
         if (tv.SelectedNode != null) {
            TreeNode tn = new TreeNode(groupname);
            TVHelper.SetTreeNodeTypeAndImage(tn, TVHelper.NodeType.Group);
            TVHelper.IdxListChanges changes = TVHelper.Insert(tv,
                                                        tn,
                                                        tv.SelectedNode.Parent,
                                                        tv.SelectedNode);
            doChanges(changes.TrackChanges, true);
            doChanges(changes.MarkerChanges, false);
            TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
            return true;
         }
         return false;
      }

      bool deleteGroup(TreeView tv) {
         if (TVHelper.IsTreeNode4Group(tv.SelectedNode)) {
            if (MessageBox.Show("Soll die Gruppe '" + tv.SelectedNode + "' mit ALLEN untergeordneten Elementen vollständig gelöscht werden?",
                                "Achtung",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
               TVHelper.IdxListChanges? changes = TVHelper.Remove(tv.SelectedNode);
               if (changes != null) {
                  doChanges(changes.TrackChanges, true);
                  doChanges(changes.MarkerChanges, false);
                  TVHelper.SaveTreeViewStructur(tv, backupfileTreeView);
               }
            }
            return true;
         }
         return false;
      }

      void setNodeImage(TreeNode? tn, Marker marker) {
         if (!TVHelper.NodeImageExists(treeView_GeoObjects, marker.Symbolname))
            TVHelper.AppendNodeImage(treeView_GeoObjects, marker.Bitmap, marker.Symbolname);
         TVHelper.SetTreeNodeTypeAndImage(tn, TVHelper.NodeType.Marker, marker.Symbolname);
      }

      void setNodeImage(TreeNode? tn, Track track) {
         Color col = Color.FromArgb(255, track.LineColor);
         string symbolname = "trackcolor#" + col.R.ToString("x2") + col.G.ToString("x2") + col.B.ToString("x2");  // ohne Alpha
         if (!TVHelper.NodeImageExists(treeView_GeoObjects, symbolname)) {
            Bitmap bm = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bm)) {
               Point[] pts = [
                  new Point(0, 2 * bm.Height / 3),
                  new Point(bm.Width / 3, bm.Height / 3),
                  new Point(2 * bm.Width / 3, 2 * bm.Height / 3),
                  new Point(bm.Width, 0),
               ];

               g.Clear(Color.Transparent);
               using (Pen pen = new Pen(Color.White, 6))
                  g.DrawLines(pen, pts);
               using (Pen pen = new Pen(col, 3))
                  g.DrawLines(pen, pts);
            }
            TVHelper.AppendNodeImage(treeView_GeoObjects, bm, symbolname);
         }
         TVHelper.SetTreeNodeTypeAndImage(tn, TVHelper.NodeType.Track, symbolname);
      }



      class TVHelper {

         /*
         TreeView.Tag enthält ein Array aus (2) Listen der TreeNodes in der gleichen Reihenfolge in der 
         die Tracks/Marker aus GpxAllExt zugeordnet sind.

         Jeder TreeNode enthält in seinem Tag den Typ des Nodes (Gruppe, Track, Marker).

         Der TreeView enthält eine ImageList für den Typ der Nodes.

          */

         /// <summary>
         /// Typ eines TreeNode
         /// </summary>
         public enum NodeType {
            Track = 0,
            Marker = 1,
            Group,
            unknown
         }

         /// <summary>
         /// Darstellung der Checkbox eines TreeNode
         /// </summary>
         public enum VisibilityStatus {
            Off = 0,
            On = 1,
            Mixed = 2,
         }

         /// <summary>
         /// zur Verwaltungen von Veränderungen in den Indexlisten
         /// </summary>
         public class IdxListChanges {

            /// <summary>
            /// Daten einer Änderung (i.A. Verschiebung von ... nach ...)
            /// </summary>
            public class Change {

               public int FromIdx { get; protected set; }

               public int ToIdx { get; protected set; }

               public Change(int from, int to) => SetNew(from, to);

               public void SetNew(int from, int to) {
                  FromIdx = from;
                  ToIdx = to;
               }

               public override string ToString() => FromIdx + " => " + ToIdx;
            }

            /// <summary>
            /// Änderungen der Trackliste
            /// </summary>
            public readonly List<Change> TrackChanges;

            /// <summary>
            /// Änderungen der Markerliste
            /// </summary>
            public readonly List<Change> MarkerChanges;


            public IdxListChanges(List<Change> trackChanges, List<Change> markerChanges) {
               TrackChanges = trackChanges;
               MarkerChanges = markerChanges;
            }

         }


         static public void PrepareTreeView(TreeView tv) {
            tv.Nodes.Clear();

            tv.Tag = new List<TreeNode>[2] {
               new List<TreeNode>(),
               new List<TreeNode>(),
            };

            buildStateImageList(tv);
         }

         /// <summary>
         /// erzeugt die Checkbox-Bilderliste für jeden <see cref="VisibilityStatus"/>
         /// </summary>
         /// <param name="tv"></param>
         static void buildStateImageList(TreeView tv) {
            tv.StateImageList = new ImageList();
            for (int i = 0; i < 3; i++) {
               /* CheckBoxState 
                     CheckedDisabled 	8 	   The check box is checked and disabled.
                     CheckedHot 	6 	         The check box is checked and hot.
                     CheckedNormal 	5 	      The check box is checked.
                     CheckedPressed 	7 	   The check box is checked and pressed.
                     MixedDisabled 	12 	   The check box is three-state and disabled.
                     MixedHot 	10 	      The check box is three-state and hot.
                     MixedNormal 	9 	      The check box is three-state.
                     MixedPressed 	11 	   The check box is three-state and pressed.
                     UncheckedDisabled 	4 	The check box is unchecked and disabled.
                     UncheckedHot 	2 	      The check box is unchecked and hot.
                     UncheckedNormal 	1 	   The check box is unchecked.
                     UncheckedPressed 	3 	   The check box is unchecked and pressed.
                */
               System.Windows.Forms.VisualStyles.CheckBoxState state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal;
               switch (i) {
                  case (int)VisibilityStatus.Off:
                     state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                     break;
                  case (int)VisibilityStatus.On:
                     state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal;
                     break;
                  case (int)VisibilityStatus.Mixed:
                     state = System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal;
                     break;
               }
               Bitmap bmp = new Bitmap(16, 16);
               // 0,1 - offset the checkbox slightly so it positions in the correct place
               System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(Graphics.FromImage(bmp),
                                                                  new Point(0, 1),
                                                                  state);
               tv.StateImageList.Images.Add(bmp);
            }
         }

         /// <summary>
         /// liefert die TreeNode-Index-Liste des Baums für den <see cref="NodeType"/> (Tracks oder Marker) oder null 
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodetype">Tracks oder Marker</param>
         /// <returns></returns>
         static List<TreeNode>? getTreeNodeIdxList(TreeView? tv, NodeType nodetype) {
            if (tv != null &&
                tv.Tag is List<TreeNode>[]) {
               List<TreeNode>[]? lists = tv.Tag as List<TreeNode>[];
               if (lists != null)
                  switch (nodetype) {
                     case NodeType.Track:
                        return lists.Length > (int)NodeType.Track ?
                                    lists[(int)NodeType.Track] :
                                    null;
                     case NodeType.Marker:
                        return lists.Length > (int)NodeType.Marker ?
                                    lists[(int)NodeType.Marker] :
                                    null;
                  }
            }
            return null;
         }

         /// <summary>
         /// liefert die TreeNode-Index-Liste des Baums zu dem der TreeNode gehört für den <see cref="NodeType"/> (Tracks oder Marker) oder null 
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="nodetype">Tracks oder Marker</param>
         /// <returns></returns>
         static List<TreeNode>? getTreeNodeIdxList(TreeNode tn, NodeType nodetype) => getTreeNodeIdxList(tn.TreeView, nodetype);

         /// <summary>
         /// liefert den TreeNode mit dem <see cref="NodeType"/> (Tracks oder Marker) und dem Listenindex oder null 
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodetype"></param>
         /// <param name="idx"></param>
         /// <returns></returns>
         static public TreeNode? GetTreeNode4ListIdx(TreeView tv, NodeType nodetype, int idx) {
            List<TreeNode>? lst = getTreeNodeIdxList(tv, nodetype);
            if (lst != null &&
                0 <= idx &&
                idx < lst.Count)
               return lst[idx];
            return null;
         }

         /// <summary>
         /// liefert den Listenindex oder -1 und <see cref="NodeType"/> des TreeNode 
         /// (oder -1 und <see cref="NodeType.unknown"/>)
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="nodetype"></param>
         /// <returns></returns>
         static public int GetListIdx4TreeNode(TreeNode? tn, out NodeType nodetype) {
            if (tn != null) {
               nodetype = GetTreeNodeType(tn);
               switch (nodetype) {
                  case NodeType.Track:
                  case NodeType.Marker:
                     List<TreeNode>? lst = getTreeNodeIdxList(tn, nodetype);
                     if (lst != null)
                        return lst.IndexOf(tn);
                     break;

                  case NodeType.Group:
                     break;
               }
            } else
               nodetype = NodeType.unknown;
            return -1;
         }

         /// <summary>
         /// liefert den Listenindex oder -1 und <see cref="NodeType"/> des ausgewählten TreeNode 
         /// (oder -1 und <see cref="NodeType.unknown"/>)
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodetype"></param>
         /// <returns></returns>
         static public int GetListIdx4Selected(TreeView tv, out NodeType nodetype) {
            if (tv.SelectedNode != null)
               return GetListIdx4TreeNode(tv.SelectedNode, out nodetype);
            nodetype = NodeType.unknown;
            return -1;
         }

         /// <summary>
         /// wählt den TreeNode mit dem <see cref="NodeType"/> (Tracks oder Marker) und dem Listenindex aus
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodetype"></param>
         /// <param name="idx"></param>
         static public void Select4ListIdx(TreeView tv, NodeType nodetype, int idx) {
            tv.SelectedNode = GetTreeNode4ListIdx(tv, nodetype, idx);
            tv.SelectedNode.EnsureVisible();
         }

         /// <summary>
         /// Hat der TreeNode den <see cref="NodeType.Group"/>?
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         static public bool IsTreeNode4Group(TreeNode? tn) => tn != null && GetTreeNodeType(tn) == NodeType.Group;

         /// <summary>
         /// Hat der TreeNode den <see cref="NodeType.Track"/>?
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         static public bool IsTreeNode4Track(TreeNode? tn) => tn != null && GetTreeNodeType(tn) == NodeType.Track;

         /// <summary>
         /// Hat der TreeNode den <see cref="NodeType.Marker"/>?
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         static public bool IsTreeNode4Marker(TreeNode? tn) => tn != null && GetTreeNodeType(tn) == NodeType.Marker;

         #region NodeType eines TreeNode

         /// <summary>
         /// setzt den <see cref="NodeType"/> des TreeNode auf einen beliebigen Wert
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="nodetype"></param>
         static public void SetTreeNodeTypeAndImage(TreeNode? tn, NodeType nodetype, string? imgkey = null) {
            if (tn != null) {
               tn.Tag = (int)nodetype;
               switch (nodetype) {
                  case NodeType.Group:
                     tn.ImageKey = tn.SelectedImageKey = IMAGE_KEY_GROUP;
                     break;

                  case NodeType.Track:
                     tn.ImageKey = tn.SelectedImageKey = !string.IsNullOrEmpty(imgkey) ? imgkey : IMAGE_KEY_TRACK;
                     break;

                  case NodeType.Marker:
                     tn.ImageKey = tn.SelectedImageKey = !string.IsNullOrEmpty(imgkey) ? imgkey : IMAGE_KEY_MARKER;
                     break;

                  default:
                     tn.ImageIndex = -1;
                     break;
               }
            }
         }

         /// <summary>
         /// liefert den <see cref="NodeType"/> des TreeNode
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         static public NodeType GetTreeNodeType(TreeNode tn) => tn != null && tn.Tag != null && tn.Tag is int ?
                                                                        (NodeType)tn.Tag :
                                                                        NodeType.unknown;

         #endregion

         #region Imagelist des Treeviws verwalten

         const string IMAGE_KEY_GROUP = "Open.png";
         const string IMAGE_KEY_TRACK = "Track.png";
         const string IMAGE_KEY_MARKER = "Marker16x16.png";

         static public bool NodeImageExists(TreeView tv, string key) => tv.ImageList.Images.Keys.Contains(key);

         static public bool AppendNodeImage(TreeView tv, Image? img, string key) {
            if (img == null)
               return false;
            if (img.PhysicalDimension.Width != tv.ImageList.ImageSize.Width ||
                img.PhysicalDimension.Height != tv.ImageList.ImageSize.Height) {
               Bitmap bm = new Bitmap(tv.ImageList.ImageSize.Width, tv.ImageList.ImageSize.Height);
               using (Graphics g = Graphics.FromImage(bm)) {
                  g.Clear(Color.Transparent);
                  RectangleF rectDest = new RectangleF();

                  if (img.PhysicalDimension.Width / img.PhysicalDimension.Height >= tv.ImageList.ImageSize.Width / tv.ImageList.ImageSize.Height) {
                     // Original ist breiter bzw. flacher als Ziel -> Breite einpassen
                     rectDest.Width = tv.ImageList.ImageSize.Width;
                     rectDest.Height = tv.ImageList.ImageSize.Width / img.PhysicalDimension.Width * img.PhysicalDimension.Height;
                     rectDest.X = 0;
                     rectDest.Y = (tv.ImageList.ImageSize.Height - rectDest.Height) / 2;
                  } else {
                     rectDest.Height = tv.ImageList.ImageSize.Height;
                     rectDest.Width = tv.ImageList.ImageSize.Height / img.PhysicalDimension.Height * img.PhysicalDimension.Width;
                     rectDest.Y = 0;
                     rectDest.X = (tv.ImageList.ImageSize.Width - rectDest.Width) / 2;
                  }
                  g.DrawImage(img, rectDest);
               }
               tv.ImageList.Images.Add(key, bm);
            } else
               tv.ImageList.Images.Add(key, img);
            return true;
         }

         #endregion

         #region VisibilityStatus eines TreeNode

         static public void SetVisibilityStatus(TreeNode tn, VisibilityStatus status) => tn.StateImageIndex = (int)status;

         /// <summary>
         /// nur für <see cref="VisibilityStatus.On"/> und  <see cref="VisibilityStatus.Off"/>
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="visible"></param>
         static void setVisibilityStatus(TreeNode? tn, bool visible) {
            if (tn != null)
               SetVisibilityStatus(tn, visible ? VisibilityStatus.On : VisibilityStatus.Off);
         }

         static public VisibilityStatus GetVisibilityStatus(TreeNode tn) => (VisibilityStatus)tn.StateImageIndex;

         #endregion

         #region Einlesen/Speichern der TreeView-Struktur

         /// <summary>
         /// speichert die akt. TreeView-Struktur als Datei
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="file"></param>
         static public void SaveTreeViewStructur(TreeView tv, string file) {
            StringBuilder sb = new StringBuilder();
            saveTreeViewStructur(tv.Nodes, sb, 0);
            File.WriteAllText(file, sb.ToString());
         }

         /// <summary>
         /// liest die TreeView-Struktur aus der Datei ein
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="file"></param>
         /// <param name="trackcount"></param>
         /// <param name="markercount"></param>
         static public void ReadTreeViewStructur(TreeView tv, string file, int trackcount, int markercount) {
            string[] lines = File.ReadAllText(file).Split(new char[] { '\n' });
            Remove(tv);
            List<TreeNode>? tnlisttracks = getTreeNodeIdxList(tv, NodeType.Track);
            List<TreeNode>? tnlistmarker = getTreeNodeIdxList(tv, NodeType.Marker);

            int lastdepth = 0;
            TreeNode? lasttn = null;
            foreach (var item in lines) {
               string line = item.TrimEnd(new char[] { '\r', '\n' });
               if (0 < line.Length &&
                   line[0] == '>') {      // gültige Zeile
                  line = line.Substring(1);

                  int depth;
                  for (depth = 0; depth < line.Length; depth++)
                     if (line[depth] != ' ')
                        break;

                  if (depth < line.Length) {
                     string nodetypetxt = line.Substring(depth, 1);
                     string nodetxt = depth + 1 < line.Length ? line.Substring(depth + 1) : "";

                     NodeType treenodetype = NodeType.unknown;
                     if (nodetypetxt == "G")
                        treenodetype = NodeType.Group;
                     else if (nodetypetxt == "T")
                        treenodetype = NodeType.Track;
                     else if (nodetypetxt == "M")
                        treenodetype = NodeType.Marker;
                     if (treenodetype != NodeType.unknown) {

                        TreeNodeCollection? tnc = null;
                        if (lastdepth == depth) {
                           if (lasttn != null) {
                              tnc = getTreeNodeCollection4TreeNode(lasttn);
                           } else
                              tnc = tv.Nodes;
                        } else if (lastdepth < depth) {
                           tnc = lasttn.Nodes;
                        } else {
                           int levels = lastdepth - depth;
                           while (0 <= levels--) {
                              lasttn = lasttn.Parent;
                              tnc = lasttn != null ? lasttn.Nodes : tv.Nodes;
                           }
                        }

                        switch (treenodetype) {
                           case NodeType.Track:
                              if (tnlisttracks.Count < trackcount) {    // Test falls zuviele Strukturknoten
                                 lasttn = tnc.Add(nodetxt);
                                 setVisibilityStatus(lasttn, false);
                                 tnlisttracks.Add(lasttn);
                              }
                              break;

                           case NodeType.Marker:
                              if (tnlistmarker.Count < markercount) {   // Test falls zuviele Strukturknoten
                                 lasttn = tnc.Add(nodetxt);
                                 setVisibilityStatus(lasttn, false);
                                 tnlistmarker.Add(lasttn);
                              }
                              break;

                           case NodeType.Group:
                              lasttn = tnc.Add(nodetxt);
                              setVisibilityStatus(lasttn, false);
                              break;
                        }
                        if (lasttn != null)
                           SetTreeNodeTypeAndImage(lasttn, treenodetype);

                        lastdepth = depth;
                     }
                  }
               }
            }

            // Ex. genügend TreeNodes?
            while (tnlisttracks.Count < trackcount) {
               TreeNode tn = tv.Nodes.Add("");
               tnlisttracks.Add(tn);
               SetTreeNodeTypeAndImage(tn, NodeType.Track);
            }
            while (tnlistmarker.Count < markercount) {
               TreeNode tn = tv.Nodes.Add("");
               tnlistmarker.Add(tn);
               SetTreeNodeTypeAndImage(tn, NodeType.Marker);
            }
         }

         #endregion

         #region interne Hilfsfunktionen

         /// <summary>
         /// Ist der zu testende TreeNode ein Child vom TreeNode?
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="tntest"></param>
         /// <returns></returns>
         static bool isSubnode(TreeNode? tn, TreeNode tntest) => tn != null && tn.Nodes.Contains(tntest);

         /// <summary>
         /// rekursive Hilfsfunktion für <see cref="SaveTreeViewStructur(TreeView, string)"/>
         /// </summary>
         /// <param name="tnc"></param>
         /// <param name="sb"></param>
         /// <param name="depth"></param>
         static void saveTreeViewStructur(TreeNodeCollection tnc, StringBuilder sb, int depth) {
            foreach (TreeNode node in tnc) {
               sb.Append(">");                                          // Kennung für gültige Zeile
               if (0 < depth)
                  sb.Append(new string(' ', depth));

               switch (GetTreeNodeType(node)) {
                  case NodeType.Group:
                     sb.Append("G");                                       // Kennung für Gruppe
                     sb.AppendLine(node.Text);
                     saveTreeViewStructur(node.Nodes, sb, depth + 1);
                     break;

                  case NodeType.Track:
                     sb.AppendLine("T");
                     break;

                  case NodeType.Marker:
                     sb.AppendLine("M");
                     break;
               }
            }
         }

         /// <summary>
         /// liefert die <see cref="TreeNodeCollection"/> in der der <see cref="TreeNode"/> selbst enthalten ist
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         static TreeNodeCollection? getTreeNodeCollection4TreeNode(TreeNode tn) => tn != null && tn.Parent != null ?
                                                                                       tn.Parent.Nodes :
                                                                                       tn.TreeView != null ?
                                                                                             tn.TreeView.Nodes :
                                                                                             null;

         /// <summary>
         /// liefert den Index in der <see cref="TreeNodeCollection"/> für den n-ten TreeNode vom Typ <see cref="NodeType"/>
         /// </summary>
         /// <param name="tnc"></param>
         /// <param name="nodetype"></param>
         /// <param name="n"></param>
         /// <returns></returns>
         static int getCollectionIdx4NthTreeNode(TreeNodeCollection tnc, NodeType nodetype, int n) {
            for (int i = 0, j = -1; i < tnc.Count; i++) {
               if (nodetype == GetTreeNodeType(tnc[i]))
                  j++;
               if (j == n)
                  return i;
            }
            return -1;
         }

         /// <summary>
         /// liefert den n-ten TreeNode vom Typ <see cref="NodeType"/> aus der <see cref="TreeNodeCollection"/>
         /// </summary>
         /// <param name="tnc"></param>
         /// <param name="nodetype"></param>
         /// <param name="n"></param>
         /// <returns></returns>
         static TreeNode? getNthTreeNode(TreeNodeCollection tnc, NodeType nodetype, int n) {
            int idx = getCollectionIdx4NthTreeNode(tnc, nodetype, n);
            return 0 <= idx ? tnc[idx] : null;
         }

         /// <summary>
         /// baut die Idx-Liste für den <see cref="NodeType"/> neu auf
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodeType"></param>
         static void rebuildIdxList(TreeView tv, NodeType nodeType) {
            List<TreeNode>? lst = getTreeNodeIdxList(tv, nodeType);
            if (lst != null) {
               lst.Clear();
               fillIdxList(tv.Nodes, lst, nodeType);
            }
         }

         /// <summary>
         /// rekursive Hilfsfunktion für <see cref="rebuildIdxList(TreeView, NodeType)"/>
         /// </summary>
         /// <param name="tnc"></param>
         /// <param name="lst">passende Liste zum <see cref="NodeType"/></param>
         /// <param name="nodeType"></param>
         static void fillIdxList(TreeNodeCollection tnc, List<TreeNode> lst, NodeType nodeType) {
            foreach (TreeNode node in tnc) {
               switch (GetTreeNodeType(node)) {
                  case NodeType.Group:
                     fillIdxList(node.Nodes, lst, nodeType);
                     break;

                  case NodeType.Track:
                     if (nodeType == NodeType.Track)
                        lst.Add(node);
                     break;

                  case NodeType.Marker:
                     if (nodeType == NodeType.Marker)
                        lst.Add(node);
                     break;
               }
            }
         }

         /// <summary>
         /// baut die Indexliste für diesen <see cref="NodeType"/> neu auf und liefert die Liste der Änderungen 
         /// die in dieser Reihenfolge nachvollzogen werden können
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodeType"></param>
         /// <returns></returns>
         static List<IdxListChanges.Change> registerIdxListChanges(TreeView? tv, NodeType nodeType) {
            List<IdxListChanges.Change> cl = new List<IdxListChanges.Change>();

            if (tv != null) {
               List<TreeNode>? actlst = getTreeNodeIdxList(tv, nodeType);
               if (actlst != null) {
                  List<TreeNode> orglst = new List<TreeNode>(actlst);         // Kopie der Originalliste
                  rebuildIdxList(tv, nodeType);                               // Liste akt.

                  for (int i = 0; i < orglst.Count; i++) {
                     TreeNode tn = orglst[i];
                     if (actlst.IndexOf(tn) < 0) {
                        orglst.RemoveAt(i);
                        cl.Add(new IdxListChanges.Change(i, -1));         // entferntes Item mit (oldidx, -1)
                        i--;
                     }
                  }

                  for (int i = 0; i < actlst.Count; i++) {
                     TreeNode tn = actlst[i];
                     int orgpos = orglst.IndexOf(tn);
                     if (i != orgpos) {
                        if (orgpos < 0) {
                           cl.Add(new IdxListChanges.Change(-1, i));      // neues Item mit (-1, newidx)
                           orglst.Insert(i, tn);            // Änderung nachvollziehen
                        } else {
                           cl.Add(new IdxListChanges.Change(orgpos, i));  // verschobenes Item mit (oldidx, newidx)
                           if (i < orgpos) {
                              orglst.RemoveAt(orgpos);      // Änderung nachvollziehen
                              orglst.Insert(i, tn);
                           }
                        }
                     }
                  }
               }
            }

            return cl;
         }

         /// <summary>
         /// Ist eine gültige Einfügeposition angegeben?
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="tnnewparent"></param>
         /// <param name="tnnewnext"></param>
         /// <returns></returns>
         static bool insertDestinationIsValid(TreeView tv, TreeNode? tnnewparent, TreeNode? tnnewnext) {
            if (tnnewparent == null) {
               if (tnnewnext != null && !tv.Nodes.Contains(tnnewnext))
                  return false;
            } else {
               if (tnnewnext != null && !tnnewparent.Nodes.Contains(tnnewnext))
                  return false;
            }
            return true;
         }

         static void compresschanges(List<IdxListChanges.Change> changes_remove, List<IdxListChanges.Change> changes_insert) {
            if (changes_remove.Count > 0) {
               // Beide Änderungen werden jetzt kombiniert.
               for (int i = 0; i < changes_remove.Count; i++)
                  changes_remove[i].SetNew(changes_remove[i].FromIdx, changes_insert[i].ToIdx);

               // Ist der Quellindex des 1. Elements gleich dem Zielindex des 1. Elementes ändert sich die Reihenfolge
               // effektiv NICHT.
               if (changes_remove[0].FromIdx == changes_insert[0].ToIdx)
                  changes_remove.Clear();

               else if (changes_remove[0].FromIdx < changes_insert[0].ToIdx) {
                  // Werden z.B. die Elemente 2,3,4 nach dem Element 6 eingefügt ist die Verschiebung schrittweise:
                  //    2 -> 6
                  //    2 -> 6
                  //    2 -> 6
                  for (int i = 0; i < changes_remove.Count; i++)
                     changes_remove[i].SetNew(changes_remove[0].FromIdx, changes_remove[changes_remove.Count - 1].ToIdx);

               } else {
                  // Werden z.B. die Elemente 4,5,6 nach dem Element 1 eingefügt ist die Verschiebung schrittweise:
                  //    4 -> 2
                  //    5 -> 3
                  //    6 -> 4
                  for (int i = 0; i < changes_remove.Count; i++)
                     changes_remove[i].SetNew(changes_remove[0].FromIdx + i, changes_remove[0].ToIdx + i);
               }
            }
         }

         #endregion

         #region Einfügen, Löschen, Verschieben von TreeNodes

         static IdxListChanges emptyIdxListChanges()
            => new IdxListChanges(new List<IdxListChanges.Change>(), new List<IdxListChanges.Change>());

         /// <summary>
         /// entfernt den <see cref="TreeNode"/> und alle untergeordneten <see cref="TreeNode"/>, bereinigt die Objektliste und 
         /// liefert die Indexlisten der gelöschten Objekte
         /// </summary>
         /// <param name="tn"></param>
         /// <returns>sortierte Indexliste der gelöschten Objekte</returns>
         static public IdxListChanges? Remove(TreeNode? tn) {
            if (tn != null) {
               TreeView? tv = tn.TreeView;
               tn.Remove();
               return new IdxListChanges(registerIdxListChanges(tv, NodeType.Track),
                                         registerIdxListChanges(tv, NodeType.Marker));
            }
            return null;
         }

         /// <summary>
         /// löscht den gesamten Baum
         /// </summary>
         /// <param name="tv"></param>
         /// <returns></returns>
         static public IdxListChanges Remove(TreeView tv) {
            tv.Nodes.Clear();
            return new IdxListChanges(registerIdxListChanges(tv, NodeType.Track),
                                      registerIdxListChanges(tv, NodeType.Marker));
         }

         /// <summary>
         /// löscht alle TreeNodes mit dem vorgegebenen Typ
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodeType"></param>
         /// <returns></returns>
         static public IdxListChanges Remove(TreeView tv, NodeType nodeType) {
            removeNodeType(tv.Nodes, nodeType);
            return new IdxListChanges(registerIdxListChanges(tv, NodeType.Track),
                                      registerIdxListChanges(tv, NodeType.Marker));
         }

         /// <summary>
         /// rekursive Hilfsfunktion für <see cref="Remove(TreeView, NodeType)"/>
         /// </summary>
         /// <param name="tnc"></param>
         /// <param name="nodeType"></param>
         static void removeNodeType(TreeNodeCollection tnc, NodeType nodeType) {
            for (int i = tnc.Count - 1; 0 < i; i--) {
               removeNodeType(tnc[i].Nodes, nodeType);
               if (GetTreeNodeType(tnc[i]) == nodeType)
                  tnc[i].Remove();
            }
         }

         /// <summary>
         /// fügt einen Text als <see cref="TreeNode"/> ein, der NICHT für eine Gruppe steht
         /// <para>Der neue <see cref="TreeNode"/> wird an die Position des vorherigen <see cref="TreeNode"/> mit diesem
         /// Index in den <see cref="TreeView"/> eingefügt. Existierte noch kein <see cref="TreeNode"/> mit diesem Index
         /// wird der neue an die <see cref="TreeView"/>-Liste angehängt.</para>
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodeType"></param>
         /// <param name="txt"></param>
         /// <param name="listidx">Index in der Indexliste</param>
         /// <param name="selectnode">wenn true wird der Knoten ausgewählt</param>
         /// <param name="checkednode">Checked-Zustand des Knotens</param>
         static public IdxListChanges Insert(TreeView tv,
                                             NodeType nodeType,
                                             string txt,
                                             int listidx,
                                             bool selectnode,
                                             bool checkednode) {
            if (nodeType == NodeType.Track ||
                nodeType == NodeType.Marker) {
               TreeNode? tnnew;
               TreeNode? tnold = GetTreeNode4ListIdx(tv, nodeType, listidx);
               if (tnold != null) {
                  TreeNodeCollection? tnc = getTreeNodeCollection4TreeNode(tnold);
                  tnnew = tnc?.Insert(tnc.IndexOf(tnold), txt);
               } else
                  tnnew = tv.Nodes.Add(txt);
               if (tnnew != null && selectnode) {
                  tnnew.TreeView.SelectedNode = tnnew;
                  tnnew.EnsureVisible();
               }
               SetTreeNodeTypeAndImage(tnnew, nodeType);

               setVisibilityStatus(tnnew, checkednode);
            }
            return new IdxListChanges(registerIdxListChanges(tv, NodeType.Track),
                                      registerIdxListChanges(tv, NodeType.Marker));
         }

         /// <summary>
         /// fügt den <see cref="TreeNode"/> (mit seinem gesamten untergeordnetem Bereich) 
         /// unter dem Parent (bei null unter <see cref="TreeView"/>) 
         /// vor seinem Nachfolger (bei null am Ende) ein und liefert die Änderungsliste
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="tnnewparent"></param>
         /// <param name="tnnewnext"></param>
         /// <returns>Liste der Veränderungen</returns>
         static public IdxListChanges Insert(TreeView tv,
                                             TreeNode? tn,
                                             TreeNode? tnnewparent,
                                             TreeNode? tnnewnext) {
            if (tn != null &&
                tn.Parent == null &&
                tn.TreeView == null &&
                insertDestinationIsValid(tv, tnnewparent, tnnewnext)) {
               TreeNodeCollection tncdest = tnnewparent != null ? tnnewparent.Nodes : tv.Nodes;

               bool add = false;
               if (tnnewnext == null)
                  add = true;
               else if (tnnewparent != null &&
                        !tnnewparent.Equals(tnnewnext.Parent))
                  add = true;
               else if (tnnewparent == null &&
                        tnnewnext.Parent != null)
                  add = true;

               if (add) {
                  tncdest.Add(tn);
               } else {
                  tncdest.Insert(tnnewnext.Index, tn);
               }

               return new IdxListChanges(registerIdxListChanges(tn.TreeView, NodeType.Track),
                                         registerIdxListChanges(tn.TreeView, NodeType.Marker));
            }
            return emptyIdxListChanges();
         }

         /// <summary>
         /// entfernt den <see cref="TreeNode"/> und fügt ihn unter dem Parent (bei null unter <see cref="TreeView"/>) 
         /// vor seinem Nachfolger (bei null am Ende) ein und liefert die Änderungsliste
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="tn"></param>
         /// <param name="tnnewparent"></param>
         /// <param name="tnnewnext"></param>
         /// <returns></returns>
         static IdxListChanges move(TreeView tv,
                                    TreeNode? tn,
                                    TreeNode? tnnewparent,
                                    TreeNode? tnnewnext) {
            if (insertDestinationIsValid(tv, tnnewparent, tnnewnext) &&
                !(
                  (tnnewparent != null && isSubnode(tn, tnnewparent)) ||
                  (tnnewnext != null && isSubnode(tn, tnnewnext))
                )) {
               bool cancelmove = false;          // Verschiebung unnötig?
               if (tnnewnext != null) {
                  TreeNode? tnnext = tn.NextNode;
                  if (tnnext != null &&
                      tnnext.Equals(tnnewnext))
                     cancelmove = true;
               } else {
                  TreeNodeCollection tnc = tnnewparent != null ? tnnewparent.Nodes : tv.Nodes;
                  if (0 < tnc.Count && tnc[tnc.Count - 1].Equals(tn))
                     cancelmove = true;
               }

               if (!cancelmove) {
                  IdxListChanges? changes_remove = Remove(tn);
                  // changes enthält eine Liste der entfernten Indexe.
                  // Da die Elemente aufeinanderfolgen ist der Index immer gleich.

                  IdxListChanges changes_insert = Insert(tv, tn, tnnewparent, tnnewnext);
                  // Da die aufeinaderfolgenden Elemente nacheinander wieder eingefügt werden folgen ihre
                  // Indexe alle aufeinander.

                  compresschanges(changes_remove.TrackChanges, changes_insert.TrackChanges);
                  compresschanges(changes_remove.MarkerChanges, changes_insert.MarkerChanges);
                  return changes_remove;
               }
            }
            return emptyIdxListChanges();
         }

         /// <summary>
         /// entfernt den <see cref="TreeNode"/> mit dem Listenindex und fügt in an der Position ddes <see cref="TreeNode"/>
         /// mit dem anderen Listenindex ein
         /// <para>Existierte noch kein <see cref="TreeNode"/> mit diesem Zielindex wird er an die <see cref="TreeView"/>-Liste 
         /// angehängt.</para>
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="nodeType"></param>
         /// <param name="fromlistidx"></param>
         /// <param name="tolistidx"></param>
         /// <returns></returns>
         static public IdxListChanges Move(TreeView tv,
                                           NodeType nodeType,
                                           int fromlistidx,
                                           int tolistidx) {
            TreeNode? tn = GetTreeNode4ListIdx(tv, nodeType, fromlistidx);
            if (tn != null) {
               TreeNode? tnto = GetTreeNode4ListIdx(tv, nodeType, tolistidx);
               return move(tv, tn, tnto.Parent, tnto);
            }
            return emptyIdxListChanges();
         }

         /* Beim "Ablegen" eines TreeNode nach dem Ziehen über den TreeView ist nicht genau klar was damit erreicht werden soll:
          *    - beim Ablegen auf einem Gruppennode
          *          entweder unterhalb des Gruppennode (an welcher Stelle?) oder davor oder danach
          *    - beim Ablegen auf einen Track- oder Markernode
          *          entweder davor oder danach
          *          
          * Es gibt jeweils eine Standardaktion und eine alternative Aktion (+ Alt-Taste).   
          *    Standardaktion
          *       bei Gruppennode immer an die 1. Position unterordnen
          *       sonst immer davor einfügen
          *    alternative Aktion
          *       bei Gruppennode immer davor einfügen
          *       sonst immer danach einfügen
          */

         /// <summary>
         /// Der <see cref="TreeNode"/> (mit seinen ev. vorhanden Subnodes) wird VOR oder UNTER/NACH
         /// den Ziel-<see cref="TreeNode"/> verschoben.
         /// </summary>
         /// <param name="tv"></param>
         /// <param name="tn"></param>
         /// <param name="tndest"></param>
         /// <param name="preinsert"></param>
         /// <returns></returns>
         static public IdxListChanges MoveTo(TreeView tv, TreeNode? tn, TreeNode? tndest, bool preinsert) {
            if (tndest != null) {                  // Ziel-TreeNode ist angegeben
               if (!tn.Equals(tndest)) {           // nicht auf sich selbst
                  if (IsTreeNode4Group(tndest)) {     // Ziel-TreeNode ist Gruppe
                     if (preinsert) {
                        return move(tv, tn, tndest.Parent, tndest);
                     } else {
                        TreeNode? tnnext = null;
                        if (0 < tndest.Nodes.Count) {
                           tnnext = tndest.Nodes[0];           // an 1. Stelle einfügen
                           if (tnnext.Equals(tn))              // "vor sich selbst"
                              if (1 < tndest.Nodes.Count)
                                 tnnext = tndest.Nodes[1];
                              else
                                 tnnext = null;
                        }
                        return move(tv, tn, tndest, tnnext);
                     }
                  } else                              // Ziel-TreeNode ist Track/Marker
                     return move(tv, tn, tndest.Parent, preinsert ? tndest : tndest.NextNode);
               } else
                  return emptyIdxListChanges();
            } else                                 // ohne Ziel-TreeNode
               return move(tv, tn, null, null);
         }

         #endregion

      }

      private void toolStripMenuItem_CopyText_Click(object sender, EventArgs e) {
         if (!string.IsNullOrEmpty(textBox_Info.Text))
            Clipboard.SetText(textBox_Info.Text);
      }

      /// <summary>
      /// z.Z. nur für Keys.Space bei Checkbox und 
      /// F2 für Labeledit
      /// </summary>
      /// <param name="keys"></param>
      public void SendKeyDown(Keys keys) {
         if (treeView_GeoObjects.SelectedNode != null)
            tv_KeyDown(treeView_GeoObjects, new KeyEventArgs(keys));
      }

      private void contextMenuStrip_TextBox_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         e.Cancel = string.IsNullOrEmpty(textBox_Info.Text);
      }
   }
}
