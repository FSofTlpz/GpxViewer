using FSofTUtils;
using SmallMapControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;
using System.Linq;

namespace GpxViewer {
   public partial class ReadOnlyTracklistControl : UserControl {

      // .\gpx-tracks.xml
      // p:\Gps\Touren\gpx-tracks.xml

      #region interne Klassen

      /// <summary>
      /// spez. <see cref="BaseTreenode"/> mit Verbindung zum zugehörigen <see cref="System.Windows.Forms.TreeNode"/> (<see cref="VisualNode"/>)
      /// </summary>
      internal class TNode : BaseTreenode {

         /// <summary>
         /// übergeordneter <see cref="TNode"/>
         /// </summary>
         public new TNode Parent {
            get => base.Parent as TNode;
            protected set => base.Parent = value;
         }

         /// <summary>
         /// untergeordnete Child-<see cref="TNode"/>
         /// </summary>
         public new List<TNode> ChildNodes => base.ChildNodes.Cast<TNode>().ToList();

         /// <summary>
         /// 1. Childnode (oder null)
         /// </summary>
         public new TNode FirstChildnode => base.FirstChildnode as TNode;

         /// <summary>
         /// letzter Childnode (oder null)
         /// </summary>
         public new TNode LastChildnode => base.LastChildnode as TNode;


         public enum NodeType {
            /// <summary>
            /// Root-Node (als <see cref="Tree"/>
            /// </summary>
            Root,
            /// <summary>
            /// Dummy-Node, damit der Parent-Knoten erstmal "aufklappbar" ist
            /// </summary>
            Dummy,
            /// <summary>
            /// Node enthält den Dateinamen der GPX-Datei
            /// </summary>
            GpxFilename,
            /// <summary>
            /// Node enthält das Datenobjekt für die GPX-Daten
            /// </summary>
            GpxObject,
            /// <summary>
            /// Node enthält einen einzelnen Track
            /// </summary>
            Track,
            /// <summary>
            /// Node ist nur ein Trenner in der Liste ( kann aber untergeordnete GPX-Datei-TreeNodes enthalten)
            /// </summary>
            Delimiter,
            /// <summary>
            /// Node steht für eine GPX-Listen-Datei
            /// </summary>
            Sampler,
         }


         /// <summary>
         /// liefert den Type der Daten
         /// </summary>
         public NodeType Nodetype { get; private set; }

         /// <summary>
         /// ext. vorgeg. Name
         /// </summary>
         public bool ExternName { get; private set; }

         /// <summary>
         /// Dateiname (oder null)
         /// </summary>
         public string GpxFilename { get; private set; }

         /// <summary>
         /// Dateiname (oder null) für die GPX-Datei mit Bildpunkten
         /// </summary>
         public string GpxPictureFilename { get; private set; }

         /// <summary>
         /// Referenzpfad (oder null, dann identisch zu <see cref="GpxPictureFilename"/>) für die Bilddateien
         /// </summary>
         public string PictureReferencePath { get; private set; }

         /// <summary>
         /// Gpx-Datenobjekt (oder null)
         /// </summary>
         public PoorGpxAllExt Gpx { get; private set; }

         long _poorGpxAllExtIsLoading = 0;
         /// <summary>
         /// Wird <see cref="Gpx"/> gerade eingelesen?
         /// </summary>
         public bool PoorGpxAllExtIsLoading {
            get {
               return Interlocked.Read(ref _poorGpxAllExtIsLoading) != 0;
            }
            set {
               Interlocked.Exchange(ref _poorGpxAllExtIsLoading, value ? 1 : 0);
            }
         }

         /// <summary>
         /// Track (Tracksegment)
         /// </summary>
         public Track Track { get; private set; }

         /// <summary>
         /// eine der 5 Standardfarben
         /// </summary>
         public int StdColorNo { get; private set; }

         /// <summary>
         /// der "visuelle" Node ist anwählbar oder nicht
         /// </summary>
         public bool IsEnabled { get; set; } = true;

         #region OS-abhängige Eigenschaften

         public TreeNode VisualNode {
            get;
            protected set;
         }

         string _visualText;

         /// <summary>
         /// Text der im "visuellen" Node angezeigt wird
         /// </summary>
         public string VisualNode_Text {
            get => _visualText;
            set {
               _visualText = value;
               if (VisualNode != null)
                  VisualNode.Text = _visualText;
            }
         }

         /// <summary>
         /// ist dieser Node markiert oder nicht
         /// </summary>
         public bool VisualNode_IsChecked {
            get => VisualNode != null ?
                        VisualNode.Checked :
                        false;
            set {
               if (VisualNode != null)
                  VisualNode.Checked = value;
            }
         }

         /// <summary>
         /// Ist der zugehörige <see cref="VisualNode"/> "aufgeklappt"?
         /// </summary>
         public bool VisualNode_IsExpanded {
            get => VisualNode != null ?
                        VisualNode.IsExpanded :
                        false;
         }

         /// <summary>
         /// Farbe des zugehörigen <see cref="VisualNode"/>
         /// </summary>
         public Color VisualNode_Backcolor {
            get => VisualNode.BackColor;
            set => VisualNode.BackColor = value;
         }

         #endregion

         object lockobj = new object();


         public TNode(NodeType tagtype = NodeType.Dummy,
                      string visualtext = "",
                      bool createvisualnode = true) {
            VisualNode = null;
            Nodetype = tagtype;
            ExternName = false;
            GpxFilename = null;
            Gpx = null;
            Track = null;
            VisualNode_Text = visualtext;
            if (createvisualnode)
               createAndAppendVisualNode();
         }

         public TNode(string gpxfilename,
                      string picturegpxfilename,
                      string picturereferencepath,
                      bool externname,
                      int stdcol,
                      string visualtext,
                      bool createvisualnode = true) :
            this(NodeType.GpxFilename, visualtext, false) {
            GpxFilename = gpxfilename;
            GpxPictureFilename = picturegpxfilename;
            PictureReferencePath = picturereferencepath;
            ExternName = externname;
            StdColorNo = stdcol;
            if (createvisualnode)
               createAndAppendVisualNode();
         }

         public TNode(PoorGpxAllExt gpx,
                      string visualtext,
                      bool createvisualnode = true) :
            this(NodeType.GpxObject, visualtext, false) {
            Gpx = gpx;
            if (createvisualnode)
               createAndAppendVisualNode();
         }

         public TNode(Track track,
                     bool createvisualnode = true) :
            this(NodeType.Track, track.VisualName, false) {
            Track = track;
            if (createvisualnode)
               createAndAppendVisualNode();
         }


         /// <summary>
         /// hängt den Childnode an die Childnodeliste an
         /// </summary>
         /// <param name="child"></param>
         public void AppendChild(TNode child) {
            base.AppendChild(child);
            visualNode_Move(child.VisualNode, VisualNode);
         }

         /// <summary>
         /// fügt den Childnode in die Childnodeliste ein
         /// </summary>
         /// <param name="idx">bei ungültigem Wert erfolgt automatisch <see cref="AppendChild"/></param>
         /// <param name="child"></param>
         public void InsertChild(int idx, TNode child) {
            base.InsertChild(idx, child);
            if (0 <= idx && idx < Childs - 1)
               visualNode_Move(child.VisualNode, VisualNode, idx);
            else
               visualNode_Move(child.VisualNode, VisualNode);
         }

         /// <summary>
         /// entfernt den Childnode an dieser Position aus der Childnodeliste
         /// </summary>
         /// <param name="idx"></param>
         public new void RemoveChildAt(int idx) {
            base.RemoveChildAt(idx);
            visualNode_Remove(VisualNode, idx);
         }

         /// <summary>
         /// entfernt den Childnode aus der Childnodeliste
         /// </summary>
         /// <param name="child"></param>
         public void RemoveChild(TNode child) {
            base.RemoveChild(child);
            if (child != null)
               visualNode_Remove(VisualNode, child.VisualNode);
         }

         /// <summary>
         /// tauscht ein Childnode gegen ein anderes aus
         /// </summary>
         /// <param name="oldchild"></param>
         /// <param name="newchild"></param>
         /// <returns></returns>
         public bool Substitute(TNode oldchild, TNode newchild) {
            lock (lockobj) { // weil mehrere Ops indexabh. sind
               int idx = ChildNodes.IndexOf(oldchild);
               if (idx >= 0) {
                  RemoveChildAt(idx);
                  InsertChild(idx, newchild);
                  return true;
               }
            }
            return false;
         }

         /// <summary>
         /// liefert (rekursiv) den übergeordneten <see cref="Tree"/> oder null
         /// </summary>
         /// <returns></returns>
         public Tree GetTree() {
            if (Parent != null)
               if (Parent is Tree)
                  return Parent as Tree;
               else
                  return (Parent as TNode).GetTree();

            return null;
         }

         #region OS-abhängige Funktionen

         /// <summary>
         /// liefert den <see cref="TNode"/> des visuellen Node oder null
         /// </summary>
         /// <param name="tn"></param>
         /// <returns></returns>
         public static TNode GetTN(TreeNode tn) {
            return (tn != null &&
                    tn.Tag != null &&
                    tn.Tag is TNode) ?
                           tn.Tag as TNode :
                           null;
         }

         /// <summary>
         /// klappt den visuellen Childnode auf
         /// </summary>
         public void VisualNode_Expand() {
            VisualNode?.Expand();
         }

         /// <summary>
         /// klappt den visuellen Childnode zusammen
         /// </summary>
         public void VisualNode_Collapse() {
            VisualNode?.Collapse();
         }


         Control getMasterAndTreeNodeCollection(TreeNode parent, out TreeNodeCollection tnc) {
            Control master = null;
            tnc = null;

            if (parent == null) {   // dann für root
               if (this is Tree) {
                  master = (this as Tree).VisualTree.Parent;
                  tnc = (this as Tree).VisualTree.Nodes;
               }
            } else {
               master = parent.TreeView.Parent;
               tnc = parent.Nodes;
            }
            return master;
         }


         /// <summary>
         /// löscht den visuellen Childnode
         /// </summary>
         /// <param name="parent"></param>
         /// <param name="child"></param>
         void visualNode_Remove(TreeNode parent, TreeNode child) {
            Control master = getMasterAndTreeNodeCollection(parent, out TreeNodeCollection tnc);
            if (master != null &&
                tnc != null &&
                child != null)
               InvokeMethod4TreeNodeCollection(master,
                                               tnc,
                                               "Remove",
                                               new object[] { child });
         }

         /// <summary>
         /// löscht den visuellen Childnode
         /// </summary>
         /// <param name="parent"></param>
         /// <param name="idx"></param>
         void visualNode_Remove(TreeNode parent, int idx) {
            //parent.Nodes.RemoveAt(idx);

            Control master = getMasterAndTreeNodeCollection(parent, out TreeNodeCollection tnc);
            if (master != null &&
                tnc != null &&
                0 <= idx && idx < tnc.Count)
               InvokeMethod4TreeNodeCollection(master,
                                               tnc,
                                               "RemoveAt",
                                               new object[] { idx });

            //if (0 <= idx && idx < VisualNode.Nodes.Count) 
            //   InvokeMethod4TreeNodeCollection(parent.TreeView.Parent, 
            //                                   parent.Nodes, 
            //                                   "RemoveAt", 
            //                                   new object[] { idx });
         }

         /// <summary>
         /// verschiebt einen visuellen Node in die Childnodeliste eines anderen Node
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="newparent"></param>
         /// <param name="newpos"></param>
         void visualNode_Move(TreeNode tn, TreeNode newparent, int newpos = -1) {
            if (tn != null) {
               if (tn.Parent != null)
                  tn.Parent.Nodes.Remove(tn);
               if (newparent != null) {
                  if (newpos < 0) {
                     if (newparent.TreeView == null)
                        newparent.Nodes.Add(tn);
                     else
                        InvokeMethod4TreeNodeCollection(newparent.TreeView.Parent, newparent.Nodes, "Add", new object[] { tn });
                  } else {
                     if (newparent.TreeView == null)
                        newparent.Nodes.Insert(newpos, tn);
                     else
                        InvokeMethod4TreeNodeCollection(newparent.TreeView.Parent, newparent.Nodes, "Insert", new object[] { newpos, tn });
                  }
               } else { // es gibt keinen Parent, d.h. in die Liste vom TreeView (für den das akt. Objekt steht) einfügen
                  if (this is Tree) {
                     TreeView tv = (this as Tree).VisualTree;
                     if (newpos < 0)
                        //tv.Nodes.Add(tn);
                        InvokeMethod4TreeNodeCollection(tv.Parent, tv.Nodes, "Add", new object[] { tn });
                     else
                        //tv.Nodes.Insert(newpos, tn);
                        InvokeMethod4TreeNodeCollection(tv.Parent, tv.Nodes, "Insert", new object[] { newpos, tn });
                  }
               }
            }
         }

         // Änderungen in einem Control in Win müssen im richtigen Thread erfolgen!
         // Deshalb hier spez. für Änderungen einer TreeNodeCollection:

         private delegate void MethodCaller4TreeNodeCollection<ControlType>(
                                       ControlType mastercontrol,
                                       TreeNodeCollection tnc,
                                       string methodName,
                                       params object[] parameters) where ControlType : Control;

         /// <summary>
         /// für Add(), Insert() und RemoveAt() einer TreeNodeCollection im Control
         /// </summary>
         /// <typeparam name="ControlType"></typeparam>
         /// <param name="mastercontrol"></param>
         /// <param name="tnc"></param>
         /// <param name="methodName"></param>
         /// <param name="parameters"></param>
         public static void InvokeMethod4TreeNodeCollection<ControlType>(
                                    ControlType mastercontrol,
                                    TreeNodeCollection tnc,
                                    string methodName,
                                    params object[] parameters)
                                    where ControlType : Control {
            if (mastercontrol.InvokeRequired) {
               MethodCaller4TreeNodeCollection<ControlType> callerDelegate = InvokeMethod4TreeNodeCollection;
               mastercontrol.Invoke(callerDelegate,
                                    new object[] {
                                       mastercontrol,
                                       tnc,
                                       methodName,
                                       parameters
                                    });
            } else {
               System.Reflection.MethodInfo method = methodName == "Add" ?
                                                         tnc.GetType().GetMethod(methodName, new Type[] { typeof(TreeNode) }) :
                                                     methodName == "Insert" ?
                                                         tnc.GetType().GetMethod(methodName, new Type[] { typeof(int), typeof(TreeNode) }) :
                                                         tnc.GetType().GetMethod(methodName);
               method.Invoke(tnc, parameters);
            }
         }

         /// <summary>
         /// erzeugt einen "visuellen" Node
         /// </summary>
         void createAndAppendVisualNode() {
            VisualNode = new TreeNode(VisualNode_Text != null ?
                                             VisualNode_Text :
                                             "") {
               Checked = false,
               Tag = this,                    // immer "verlinkt" mit dem aktuellen Node
            };
            if (Nodetype == NodeType.Delimiter)
               VisualNode.ToolTipText = "";

            foreach (TNode item in ChildNodes)
               visualNode_Move(item.VisualNode, VisualNode);
         }

         #endregion

         public override string ToString() {
            switch (Nodetype) {
               case NodeType.GpxFilename:
                  return string.Format("[Nodetype={0}, GpxFilename={1}], Childs={2}, Parent={3}]",
                                       Nodetype,
                                       GpxFilename,
                                       Childs,
                                       Parent);

               case NodeType.GpxObject:
                  return string.Format("[Nodetype={0}, Gpx={1}], Childs={2}, Parent={3}]",
                                       Nodetype,
                                       Gpx,
                                       Childs,
                                       Parent);

               case NodeType.Track:
                  return string.Format("[Nodetype={0}, Track={1}], Childs={2}, Parent={3}]",
                                       Nodetype,
                                       Track,
                                       Childs,
                                       Parent);

               default:
                  return string.Format("[Nodetype={0}, Childs={1}, Parent={2}]",
                                       Nodetype,
                                       Childs,
                                       Parent);
            }

         }

      }

      /// <summary>
      /// spez. <see cref="TNode"/> der als Root dient (OS-abhängig)
      /// </summary>
      internal class Tree : TNode {

         public TreeView VisualTree {
            get;
            protected set;
         }

         public TNode SelectedNode {
            get => VisualTree != null ? GetTN(VisualTree.SelectedNode) : null;

            set {
               if (value != null &&
                   (SelectedNode == null || !value.Equals(SelectedNode)))
                  VisualTree.SelectedNode = value.VisualNode;
            }
         }


         public Tree(TreeView tv) :
            base(NodeType.Root, "", false) {
            VisualTree = tv;
         }

         public void StopUpdate() {
            VisualTree.BeginUpdate();
         }

         public void ResumeUpdate() {
            VisualTree.EndUpdate();
         }

         public void EnsureNodeVisible(TNode tn) {
            tn.VisualNode.EnsureVisible();
         }

         public override string ToString() {
            return "Childs=" + Childs;
         }

      }


      /*
      - infos.Enqueue() benötigt kaum Zeit
      - die Aufteilung in echte Threads, die gleich ihre Arbeit als großes "Paket" bekommen bringt kaum etwas
      - die Aufteilung auf viele Threads/Tasks steigert zwar die CPU-Auslastung, dauert aber insgesamt deutlich länger -> es steigt offensichtlich nur der Verwaltungsaufwand, obwohl
        außer des Synchronisierung am Schluss kein Synchronisierungsaufwand vorhanden ist
      - Ev. ist das Speichermanagement (als gemeinsame Ressource) der Flaschenhals?

      - ein GC.Collect() nach jedem loadDataIfGpxFile() bremst noch wesentlich stärker
        GC.TryStartNoGCRegion()/GC.EndNoGCRegion() und/oder System.Runtime.GCSettings.LatencyMode brachte keinen Erfolg

       */

      /// <summary>
      /// Hier erfolgt das Einlesen der GPX-Dateien. (OS-unabhängig)
      /// </summary>
      internal class Loader4GpxFile {

         delegate void SafeCallDelegate4ConcurrentBag2Void(ConcurrentBag<TNode> changedTreeNodes);

         ReadOnlyTracklistControl ctrl;


         public Loader4GpxFile(ReadOnlyTracklistControl ctrl) {
            this.ctrl = ctrl;
         }

         /// <summary>
         /// liest für alle TreeNodes die GPX-Files mit der vorgegebenen Task-Anzahl asynchron ein
         /// <para>Während des Einlesens sind diese TreeNodes Disabled.</para>
         /// </summary>
         /// <param name="tnlst"></param>
         /// <param name="taskcount">bei 0 synchrones Einlesen, sonst immer asynchron</param>
         public void LoadGpxfiles4TreeNodes(IList<TNode> tnlst, int taskcount = 0) {
            List<TNode> allTreeNodesList = new List<TNode>();
            loadSampleAllTreeNodes(allTreeNodesList, tnlst);
            foreach (TNode tn in allTreeNodesList)
               tn.IsEnabled = false;

            ConcurrentBag<TNode> allTreeNodes = new ConcurrentBag<TNode>(allTreeNodesList);
            ConcurrentBag<TNode> changedTreeNodes = new ConcurrentBag<TNode>();
            ConcurrentQueue<string> infos = new ConcurrentQueue<string>();

            if (taskcount > 0) {
               for (int i = 0; i < taskcount; i++)
                  Task.Run(() => {
                     loadWorker(allTreeNodes, changedTreeNodes, infos);
                  });
            } else {
               loadWorker(allTreeNodes, changedTreeNodes, infos);
            }

         }

         void loadSampleAllTreeNodes(List<TNode> treeNodesSample, IList<TNode> tnlst) {
            foreach (TNode tn in tnlst) {
               treeNodesSample.Add(tn);
               if (tn.Childs > 0) {     // rekursiv für die Subnodes
                  List<TNode> subtnlist = new List<TNode>();
                  foreach (TNode subtn in tn.ChildNodes)
                     subtnlist.Add(subtn);
                  loadSampleAllTreeNodes(treeNodesSample, subtnlist);
               }
            }
         }

         void loadWorker(ConcurrentBag<TNode> treeNodesSample, ConcurrentBag<TNode> changedTreeNodes, ConcurrentQueue<string> infos) {
            TNode tn;
            while (treeNodesSample.TryTake(out tn)) {

               TNode newnode = loadDataIfGpxFile(tn, infos);
               if (newnode != null) {
                  changedTreeNodes.Add(newnode);

                  if (infos.Count > 10) {
                     string info;
                     while (infos.Count > 1)
                        infos.TryDequeue(out info);
                     if (infos.TryDequeue(out info))
                        ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(info));
                  }
               }
               tn.IsEnabled = true;

            }
            ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(""));

            if (ctrl.InvokeRequired) {
               var d = new SafeCallDelegate4ConcurrentBag2Void(loadUpdateTreeNodes);
               ctrl.Invoke(d, new object[] { changedTreeNodes });
            } else
               loadUpdateTreeNodes(changedTreeNodes);

         }

         void loadUpdateTreeNodes(ConcurrentBag<TNode> changedTreeNodes) {
            Tree root = null;
            if (changedTreeNodes.TryPeek(out TNode tntv))
               root = tntv.GetTree();

            root?.StopUpdate();

            while (changedTreeNodes.TryTake(out TNode tn)) {
               PoorGpxAllExt gpx = tn.Gpx;

               if (!tn.ExternName &&                        // Name nicht expl. vorgeg.
                   gpx.TrackList.Count == 1 &&              // genau 1 Track -> Trackname für TreeNode übernehmen
                   gpx.TrackList[0].VisualName.Length > 0)
                  tn.VisualNode_Text = gpx.TrackList[0].VisualName;

               // für jede Route (Tracksegment) ein Child-TreeNode anhängen
               tn.ChildNodes.Clear();
               if (gpx.TrackList.Count > 1)    // >1, sonst fehlt in Zukunft die "aufklapp"-Box (unnötig)
                  foreach (Track track in gpx.TrackList) {
                     tn.AppendChild(new TNode(track));
                  }
            }

            root?.ResumeUpdate();
         }

         /// <summary>
         /// falls der TreeNode den Tagtype <see cref="TreeNodeTagData.TagType.GpxFilename"/> hat, werden die Daten eingelesen und neue Tag-Daten im TreeNode gesetzt
         /// <para>NICHT threadsicher!</para>
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="infos"></param>
         /// <returns></returns>
         TNode loadDataIfGpxFile(TNode tn, ConcurrentQueue<string> infos) {
            TNode newnode = null;

            if (tn.Nodetype == TNode.NodeType.GpxFilename) { // bisher nur Dateiname vorhanden -> Daten einlesen
               if (!tn.PoorGpxAllExtIsLoading) {
                  tn.PoorGpxAllExtIsLoading = true;
                  infos.Enqueue("lese '" + tn.VisualNode_Text + "', (" + Path.GetFileName(tn.GpxFilename) + ") ...");

                  try {
                     newnode = new TNode(buildPoorGpxAllExt(tn.GpxFilename, tn.GpxPictureFilename, tn.PictureReferencePath, tn.StdColorNo),
                                         tn.VisualNode_Text);
                     tn.Parent.Substitute(tn, newnode);
                  } catch (Exception ex) {
                     ctrl.ShowExceptionEvent?.Invoke(this, new SendExceptionEventArgs(ex));
                     newnode = null;
                  } finally {
                     tn.PoorGpxAllExtIsLoading = false;
                  }
               }
            }

            return newnode;
         }

      }

      #endregion

      #region spez. Events des Controls

      #region Event-Args

      public class ChooseEventArgs {
         public readonly PoorGpxAllExt Gpx;
         public readonly Track Track;
         public readonly string Name;

         public ChooseEventArgs(PoorGpxAllExt gpx, Track track, string name) {
            Gpx = gpx;
            Track = track;
            Name = name;
         }
      }

      public class SendStringEventArgs {
         /// <summary>
         /// Text, der (ev. threadsicher) angezeigt werden soll
         /// </summary>
         public readonly string Text;

         public SendStringEventArgs(string txt) {
            Text = txt;
         }
      }

      public class SendExceptionEventArgs {
         public readonly Exception Exception;

         public SendExceptionEventArgs(Exception ex) {
            Exception = ex;
         }
      }

      public class ShowTrackEventArgs {
         public readonly Track Track;
         public readonly bool On;

         public ShowTrackEventArgs(Track track, bool on) {
            Track = track;
            On = on;
         }
      }

      public class ShowMarkerEventArgs {
         public readonly PoorGpxAllExt Gpx;
         public readonly bool On;

         public ShowMarkerEventArgs(PoorGpxAllExt gpx, bool on) {
            Gpx = gpx;
            On = on;
         }
      }

      public class IsSameTrackVisibilityInContainerEventArgs {
         public readonly Track Track;
         public bool On;

         public IsSameTrackVisibilityInContainerEventArgs(Track track) {
            Track = track;
            On = false;
         }
      }

      #endregion

      /// <summary>
      /// Eine GPX-Datei wurde markiert.
      /// </summary>
      public event EventHandler<ChooseEventArgs> SelectGpxEvent;

      /// <summary>
      /// Ein Track wurde markiert.
      /// </summary>
      public event EventHandler<ChooseEventArgs> SelectTrackEvent;

      /// <summary>
      /// Eine GPX-Datei wurde ausgewählt.
      /// </summary>
      public event EventHandler<ChooseEventArgs> ChooseGpxEvent;

      /// <summary>
      /// Ein Track wurde ausgewählt.
      /// </summary>
      public event EventHandler<ChooseEventArgs> ChooseTrackEvent;

      /// <summary>
      /// eine Textinformation über den Zustand des Dateieinlesens (muss threadsicher ausgewertet werden!)
      /// </summary>
      public event EventHandler<SendStringEventArgs> LoadinfoEvent;

      /// <summary>
      /// Die "Sichtbarkeit" eines Tracks wurde verändert.
      /// </summary>
      public event EventHandler<ShowTrackEventArgs> ShowTrackEvent;

      /// <summary>
      /// Die "Sichtbarkeit" der Marker wurde verändert.
      /// </summary>
      public event EventHandler<ShowMarkerEventArgs> ShowAllMarkerEvent;

      /// <summary>
      /// Die "Sichtbarkeit" der Foto-Marker wurde verändert.
      /// </summary>
      public event EventHandler<ShowMarkerEventArgs> ShowAllFotoMarkerEvent;

      /// <summary>
      /// Ev. sollte der Programmstaus threadsicher (!) neu ausgewertet werden (während des Ladens von Dateien)
      /// </summary>
      public event EventHandler<EventArgs> RefreshProgramStateEvent;

      /// <summary>
      /// Eine Exception ist aufgetreten.
      /// </summary>
      public event EventHandler<SendExceptionEventArgs> ShowExceptionEvent;

      #endregion


      long _loadGpxfilesIsRunning = 0;
      /// <summary>
      /// Werden gerade Gpx-Daten (asynchron) eingelesen?
      /// </summary>
      public bool LoadGpxfilesIsRunning {
         get {
            return Interlocked.Read(ref _loadGpxfilesIsRunning) != 0;
         }
         protected set {
            Interlocked.Exchange(ref _loadGpxfilesIsRunning, value ? 1 : 0);
            bool searchtextexist = (FSofTUtils.Threading.ThreadsafeInvoker.InvokeControlPropertyReader(textBox_SearchText, "Text") as string).Trim().Length > 0;
            FSofTUtils.Threading.ThreadsafeInvoker.InvokeControlPropertyWriter(button_Search, "Enabled", !value && searchtextexist);
         }
      }

      /// <summary>
      /// Sollte das Einlesen von Gpx-Daten abgebrochen werden?
      /// </summary>
      public bool LoadGpxfilesCancel {
         get {
            return Interlocked.Read(ref _loadGpxfilesIsRunning) > 1;
         }
         set {
            if (value)
               Interlocked.Exchange(ref _loadGpxfilesIsRunning, 2);
         }
      }


      bool bTreeViewSelfChecked = false;

      /// <summary>
      /// Rootnode
      /// </summary>
      Tree root;


      public ReadOnlyTracklistControl() {
         InitializeComponent();
         root = new Tree(treeView1);
      }

      #region Dra&Drop (Windows)

      private void ReadOnlyTracklistControl_DragDrop(object sender, DragEventArgs e) {
         string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
         for (int i = 0; i < files.Length; i++)
            AddFile(files[i]);
      }

      private void ReadOnlyTracklistControl_DragEnter(object sender, DragEventArgs e) {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.All;
         else
            e.Effect = DragDropEffects.None;
      }

      #endregion

      #region TreeView-Events (OS-abhängig)

      private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e) {
         if (!visualTreeBeforeCheck(TNode.GetTN(e.Node)))
            e.Cancel = true;
      }

      private void treeView1_AfterCheck(object sender, TreeViewEventArgs e) {
         visualTreeAfterCheck(TNode.GetTN(e.Node));
      }

      private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
         visualTreeAfterSelect(TNode.GetTN(e.Node));
      }

      private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
         if (!visualTreeBeforeExpand(TNode.GetTN(e.Node)))
            e.Cancel = true;
      }

      /// <summary>
      /// markiert den TreeNode bei einem Rechtsklick
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
         if (e.Button == MouseButtons.Right)
            visualTreeClick(TNode.GetTN(e.Node));
      }

      private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
         visualTreeDoubleClick(TNode.GetTN(e.Node));
      }

      // Darstellung der Nodes noch etwas experimentell

      Color delimiterTreeNodeColor = SystemColors.ActiveCaption;
      Color disabledTreeNodeForeColor = SystemColors.GrayText;

      private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e) {
         //if (e.Bounds.Left < 0 ||
         //    e.Bounds.Top < 0 ||
         //    e.Bounds.Width <= 0 ||
         //    e.Bounds.Height <= 0) { // offensichtlich für "eingeklappte" Nodes
         //   //e.DrawDefault = true;
         //   //return;
         //}

         TreeView tv = sender as TreeView;
         TNode tn = TNode.GetTN(e.Node);
         bool isselected = e.Node.Equals(tv.SelectedNode);

         Color foreColor = SystemColors.WindowText;
         Color backColor = tv.BackColor;

         if (isselected) {
            foreColor = SystemColors.HighlightText;
            switch (tn.Nodetype) {
               case TNode.NodeType.Delimiter:
                  backColor = delimiterTreeNodeColor;
                  break;

               case TNode.NodeType.Sampler:
                  backColor = SystemColors.ControlDark;
                  break;

               default:
                  backColor = SystemColors.Highlight; // SystemColors.ControlDark;
                  break;
            }
         } else {
            switch (tn.Nodetype) {
               case TNode.NodeType.Delimiter:
                  backColor = delimiterTreeNodeColor;
                  break;

               case TNode.NodeType.Sampler:
                  backColor = SystemColors.ControlDark;
                  break;

               default:
                  backColor = tv.BackColor;
                  break;
            }
         }

         if (!tn.IsEnabled)
            foreColor = disabledTreeNodeForeColor;

         Rectangle b = new Rectangle(e.Bounds.Left + 1, e.Bounds.Top, tv.ClientSize.Width - e.Bounds.Left, e.Bounds.Height);
         e.Graphics.FillRectangle(new SolidBrush(backColor), b);

         //ControlPaint.DrawFocusRectangle(e.Graphics, b, backColor, SystemColors.Highlight);
         if (isselected)
            using (Pen focusPen = new Pen(Color.Black)) {
               focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
               Rectangle focusBounds = b;
               focusBounds.Size = new Size(focusBounds.Width - 1, focusBounds.Height - 1);
               e.Graphics.DrawRectangle(focusPen, focusBounds);
            }

         //TextRenderer.DrawText(e.Graphics, e.Node.Text, treeFont, b, foreColor, TextFormatFlags.GlyphOverhangPadding);
         StringFormat sf = new StringFormat() {
            LineAlignment = StringAlignment.Center,
         };
         e.Graphics.DrawString(e.Node.Text,
                               e.Node.NodeFont ?? e.Node.TreeView.Font,
                               new SolidBrush(foreColor),
                               b.Left,
                               b.Top + b.Height / 2,
                               sf);

      }

      #endregion

      #region private Funktionen (OS-unabhängig)

      #region Reaktionen auf Events des visuellen Trees (OS-unabhängig)

      /// <summary>
      /// wird false geliefert, wird die Aktion abgebrochen
      /// </summary>
      /// <param name="tn"></param>
      /// <returns></returns>
      bool visualTreeBeforeCheck(TNode tn) {
         if (tn != null) {
            if (!tn.IsEnabled ||
                tn.Nodetype == TNode.NodeType.Sampler)
               return false;
         }
         return true;
      }

      void visualTreeAfterCheck(TNode tn) {
         if (tn != null) {
            root.SelectedNode = tn;
            switch (tn.Nodetype) {
               case TNode.NodeType.GpxObject:
                  if (!bTreeViewSelfChecked) { // "echte" Auswahl?
                     tn.Gpx.Markers4StandardVisible =
                     tn.Gpx.Markers4PicturesVisible = tn.VisualNode_IsChecked;
                     foreach (Track r in tn.Gpx.TrackList)
                        ShowTrackEvent?.Invoke(this, new ShowTrackEventArgs(r, tn.VisualNode_IsChecked));

                     // Status auf alle Child-Nodes übernehmen
                     foreach (TNode subtn in tn.ChildNodes) {
                        bTreeViewSelfChecked = true;
                        subtn.VisualNode_IsChecked = tn.VisualNode_IsChecked;
                        bTreeViewSelfChecked = false;
                     }

                     ShowAllMarkerEvent?.Invoke(this, new ShowMarkerEventArgs(tn.Gpx, tn.Gpx.Markers4StandardVisible));
                     ShowAllFotoMarkerEvent?.Invoke(this, new ShowMarkerEventArgs(tn.Gpx, tn.Gpx.Markers4PicturesVisible));
                  }
                  break;

               case TNode.NodeType.Track:
                  if (!bTreeViewSelfChecked) { // "echte" Auswahl?
                     ShowTrackEvent?.Invoke(this, new ShowTrackEventArgs(tn.Track, tn.VisualNode_IsChecked));

                     if (isSameTrackVisibilityInContainer(tn.Track) &&
                         tn.Parent != null &&
                         tn.Parent.Nodetype == TNode.NodeType.GpxObject)
                        tn.Parent.VisualNode_IsChecked = tn.VisualNode_IsChecked;

                  }
                  break;

               case TNode.NodeType.Delimiter:
               case TNode.NodeType.Sampler:
                  if (!bTreeViewSelfChecked) {
                     if (tn.Childs > 0) // es ex. Subnodes
                        foreach (TNode subtn in tn.ChildNodes)
                           subtn.VisualNode_IsChecked = tn.VisualNode_IsChecked;
                  }
                  break;

               default:
                  if (!bTreeViewSelfChecked) {
                     bTreeViewSelfChecked = true;
                     tn.VisualNode_IsChecked = false;
                     bTreeViewSelfChecked = false;
                  }
                  break;

            }
         }
      }

      void visualTreeAfterSelect(TNode tn) {
         if (tn != null &&
             !tn.IsEnabled) {
            switch (tn.Nodetype) {
               case TNode.NodeType.GpxObject:
                  if (tn.Gpx.TrackList.Count > 0)
                     SelectGpxEvent?.Invoke(this, new ChooseEventArgs(tn.Gpx,
                                                                      null,
                                                                      tn.Gpx.GpxFilename));
                  break;

               case TNode.NodeType.Track:
                  SelectTrackEvent?.Invoke(this, new ChooseEventArgs(null,
                                                                     tn.Track,
                                                                     tn.Track == null ?
                                                                           tn.VisualNode_Text :
                                                                           tn != null ?
                                                                              tn.VisualNode_Text :
                                                                              tn.Track.Trackname));
                  break;

               default:
                  SelectTrackEvent?.Invoke(this, null);
                  break;
            }
         }
      }

      /// <summary>
      /// wird false geliefert, wird die Aktion abgebrochen
      /// </summary>
      /// <param name="tn"></param>
      /// <returns></returns>
      bool visualTreeBeforeExpand(TNode tn) {
         if (tn == null)
            loadIfFile(tn);                  // dabei werden auch ev. Child-Nodes erzeugt
         if (tn == null ||
             (tn.Nodetype == TNode.NodeType.GpxObject && tn.Gpx.TrackList.Count == 0) ||
             !tn.IsEnabled)
            return false;
         return true;
      }

      /// <summary>
      /// markiert den Node
      /// </summary>
      /// <param name="tn"></param>
      void visualTreeClick(TNode tn) {
         if (tn != null &&
             tn.IsEnabled)
            tn.GetTree().SelectedNode = tn;
      }

      /// <summary>
      /// markiert den Node und führt je nach Art des Node eine Aktion aus
      /// </summary>
      /// <param name="tn"></param>
      void visualTreeDoubleClick(TNode tn) {
         if (tn != null &&
             tn.IsEnabled) {
            Tree root = tn.GetTree();
            root.SelectedNode = tn;

            switch (tn.Nodetype) {
               case TNode.NodeType.GpxObject:
                  if (tn.Gpx.TrackList.Count > 0) {
                     ChooseGpxEvent?.Invoke(this, new ChooseEventArgs(tn.Gpx,
                                                                      null,
                                                                      tn.Gpx.GpxFilename));
                  }
                  break;

               case TNode.NodeType.Track:
                  ChooseTrackEvent?.Invoke(this, new ChooseEventArgs(null,
                                                                     tn.Track,
                                                                     tn.Track == null ?
                                                                           tn.VisualNode_Text :
                                                                           tn != null ?
                                                                              tn.VisualNode_Text :
                                                                              tn.Track.Trackname));
                  break;

               case TNode.NodeType.Sampler:
               case TNode.NodeType.Delimiter:

                  // spez. für Windows:

                  if (tn.VisualNode.Bounds.Left + tn.VisualNode.Bounds.Width < root.VisualTree.PointToClient(MousePosition).X) {  // nicht mehr im "Standardklickbereich" des Nodes (reagiert hier nicht mehr auf Doppelklick <-> FullRowSelect fkt. nicht zusammen mit ShowLines)

                     // "The FullRowSelect property is ignored if ShowLines is set to true"

                     if (!tn.VisualNode_IsExpanded)
                        tn.VisualNode_Expand();
                     else
                        tn.VisualNode_Collapse();
                  }
                  break;
            }
         }
      }

      #endregion

      void loadIfFile(TNode tn) {
         bool shouldload = false;

         lock (tn) {
            if (tn.Nodetype == TNode.NodeType.GpxFilename)
               if (!tn.PoorGpxAllExtIsLoading) {
                  tn.PoorGpxAllExtIsLoading = true;
                  shouldload = true;
               }
         }

         if (shouldload) {
            PoorGpxAllExt gpx = null;

            LoadinfoEvent?.Invoke(this, new SendStringEventArgs("lese '" + tn.VisualNode_Text + "', (" + Path.GetFileName(tn.GpxFilename) + ") ..."));
            //Thread.Sleep(1000);        // nur für Test

            try {

               Task.Run(() => {

                  Debug.WriteLine(">>> Start: " + tn.GpxFilename);

                  gpx = buildPoorGpxAllExt(tn.GpxFilename, tn.GpxPictureFilename, tn.PictureReferencePath, tn.StdColorNo);

                  Debug.WriteLine(">>> Ende:  " + tn.GpxFilename);

               }).Wait();

            } catch (Exception ex) {
               ShowExceptionEvent?.Invoke(this, new SendExceptionEventArgs(ex));
               gpx = null;
            } finally {
               LoadinfoEvent?.Invoke(this, new SendStringEventArgs(""));
            }

            if (gpx != null) {
               if (InvokeRequired) {
                  var d = new SafeCallDelegate4TreeNodePoorGpxAllExtBool2Void(replaceDummynodeWithTracknodes);
                  Invoke(d, new object[] { tn, gpx, tn.ExternName });
               } else
                  replaceDummynodeWithTracknodes(tn, gpx, tn.ExternName);

               lock (tn) {
                  tn.PoorGpxAllExtIsLoading = false;
               }
            }
         }
      }

      /// <summary>
      /// erzeugt die notwendigen ChildNodes, ändert ev. den Namen
      /// </summary>
      /// <param name="tn"></param>
      /// <param name="gpx"></param>
      /// <param name="externname"></param>
      /// <returns></returns>
      void replaceDummynodeWithTracknodes(TNode tn, PoorGpxAllExt gpx, bool externname) {
         if (!externname &&                           // Name nicht expl. vorgeg.
             gpx.TrackList.Count == 1 &&              // genau 1 Track -> Trackname für TreeNode übernehmen
             gpx.TrackList[0].VisualName.Length > 0)
            tn.VisualNode_Text = gpx.TrackList[0].VisualName;

         root.StopUpdate();
         tn.ChildNodes.Clear();   // Dummy-Node löschen
                                  // für jeden Track ein Child-TreeNode anhängen
         if (gpx.TrackList.Count > 1)    // >1, sonst fehlt in Zukunft die "aufklapp"-Box (unnötig)
            foreach (Track track in gpx.TrackList) {
               tn.AppendChild(new TNode(track));
            }
         root.ResumeUpdate();
      }

      /// <summary>
      /// für den threadübergreifenden Aufruf von <see cref="replaceDummynodeWithTracknodes"/>() nötig 
      /// </summary>
      private delegate void SafeCallDelegate4TreeNodePoorGpxAllExtBool2Void(TNode tn, PoorGpxAllExt gpx, bool externname);


      /// <summary>
      /// eine GPX-Datei wird eingelesen und die ev. nötigen zusätzlichen Daten ermittelt
      /// </summary>
      /// <param name="gpxfilename"></param>
      /// <param name="gpxpicturefilename"></param>
      /// <param name="picturereferencepath"></param>
      /// <param name="stdcolor"></param>
      /// <returns></returns>
      static PoorGpxAllExt buildPoorGpxAllExt(string gpxfilename, string gpxpicturefilename, string picturereferencepath, int stdcolor) {
         Color col;
         switch (stdcolor) {
            case 2:
               col = VisualTrack.StandardColor2;
               break;

            case 3:
               col = VisualTrack.StandardColor3;
               break;

            case 4:
               col = VisualTrack.StandardColor4;
               break;

            case 5:
               col = VisualTrack.StandardColor5;
               break;

            default:
               col = VisualTrack.StandardColor;
               break;
         }

         PoorGpxAllExt gpx = new PoorGpxAllExt() {
            TrackColor = col,
            TrackWidth = (float)VisualTrack.StandardWidth,
         };
         gpx.Load(gpxfilename, true);                             // GPX-Datei einlesen
         gpx.GpxFilename = gpxfilename;
         gpx.GpxPictureFilename = gpxpicturefilename;

         // für alle Tracks die Standardfarbe des Containers setzen
         foreach (Track track in gpx.TrackList)
            track.LineColor = gpx.TrackColor;

         // Waypoint für Bilder einlesen
         if (!string.IsNullOrEmpty(gpxpicturefilename)) {
            Gpx.GpxAll pictgpx = new Gpx.GpxAll(File.ReadAllText(gpxpicturefilename), true);
            string referencepath = string.IsNullOrEmpty(picturereferencepath) ?
                                       Path.GetDirectoryName(Path.GetFullPath(gpxpicturefilename)) :
                                       picturereferencepath;
            foreach (Gpx.GpxWaypoint wp in pictgpx.Waypoints) {
               if (wp.Name.StartsWith("file:///"))
                  wp.Name = wp.Name.Substring(8);
               wp.Name = FSofTUtils.PathHelper.ReplaceEnvironmentVars(wp.Name);

               wp.Name = Path.GetFullPath(Path.Combine(referencepath, wp.Name)); // GetFullPath() -> entfernt unnötige "." und ".."
               if (File.Exists(wp.Name))
                  gpx.MarkerListPictures.Add(Marker.Create(gpx, wp, Marker.MarkerType.Foto));
            }
         }

         if (gpx.MarkerList.Count > 0)
            gpx.Markers4StandardVisible = true;

         if (gpx.MarkerListPictures.Count > 0)
            gpx.Markers4PicturesVisible = true;

         return gpx;
      }

      /// <summary>
      /// liefert (rekursiv) alle akt. markierten Nodes in und unterhalb der aufgelisteten Nodes
      /// </summary>
      /// <param name="tnlist"></param>
      /// <returns></returns>
      List<TNode> getMarkedNodesFromTreeView(IList<TNode> tnlst) {
         List<TNode> lst = new List<TNode>();
         foreach (TNode tn in tnlst)
            BaseTreenode.Walk(tn,
                              lst,
                              (BaseTreenode btn, List<TNode> result) => {
                                 TNode tnode = btn as TNode;
                                 if (tnode.VisualNode_IsChecked)
                                    result.Add(tnode);
                              });
         return lst;
      }

      /// <summary>
      /// erzeugt nur ein TreeNode für die GPX-Datei mit dem/den Dateinamen
      /// </summary>
      /// <param name="tnc">Node-Sammlung zu der der neue Node hinzugefügt wird</param>
      /// <param name="gpxfilename">Abs. Pfad!</param>
      /// <param name="picturegpxfilename">Abs. Pfad!</param>
      /// <param name="picturereferencepath">Abs. Pfad!</param>
      /// <param name="name">angezeigter Name</param>
      /// <param name="colno">Nummer der Standardfarbe (1..)</param>
      /// <param name="tnnext">null oder der TreeNode, vor dem der neue Node eingefügt wird</param>
      /// <returns>neuer TreeNode</returns>
      TNode addGpxfileTreeNode(TNode parent,
                               string gpxfilename,
                               string picturegpxfilename = null,
                               string picturereferencepath = null,
                               string name = null,
                               int colno = 1,
                               TNode tnnext = null) {
         string ext = Path.GetExtension(gpxfilename).ToLower();
         if ((ext == ".gpx" ||
              ext == ".kml" ||
              ext == ".kmz" ||
              ext == ".gdb")
             &&
             (string.IsNullOrEmpty(picturegpxfilename) || Path.GetExtension(picturegpxfilename).ToLower() == ".gpx")) {
            TNode tn = new TNode(gpxfilename,
                                 picturegpxfilename,
                                 picturereferencepath,
                                 !string.IsNullOrEmpty(name),
                                 colno,
                                 !string.IsNullOrEmpty(name) ?
                                                name :
                                                Path.GetFileNameWithoutExtension(gpxfilename));
            tn.AppendChild(new TNode()); // damit "aufklappbar"

            int idx = tnnext != null ?
                        parent.ChildNodes.IndexOf(tnnext) :
                        -1;
            parent.InsertChild(idx, tn); // idx < 0 => Append

            return tn;
         }
         return null;
      }

      /// <summary>
      /// erzeugt die Liste der <see cref="TNode"/> einer GPX-Dateiliste und liefert eine Liste aller erneuerten GPX-Datei-Nodes
      /// </summary>
      /// <param name="filenamelist"></param>
      /// <param name="tnnext">null oder der TreeNode, vor dem die neuen Nodes eingefügt wird</param>
      /// <returns></returns>
      List<TNode> addGpxFilelistTreeNodes(string filenamelist, TNode tnnext = null) {
         List<TNode> result = new List<TNode>();

         try {
            GpxListReader gpxListReader = new GpxListReader(filenamelist);

            string lstname = gpxListReader.Name;
            if (lstname == "")
               lstname = "GPX-Liste";

            TNode tnmain = new TNode(TNode.NodeType.Sampler) {
               VisualNode_Text = ">>> " + lstname,
               VisualNode_Backcolor = SystemColors.ControlLight,
            };

            for (int group = 0; group < gpxListReader.Groups; group++) {
               TNode tngroup = null;
               string groupname = gpxListReader.GroupName(group);
               if (groupname != "")     // eigener Name vorhanden, also echte Gruppe
                  tngroup = addDelimiterTreeNode(tnmain, "----- " + groupname, null);

               for (int gpx = 0; gpx < gpxListReader.GpxFiles(group); gpx++) {
                  string gpxfilename = gpxListReader.GpxFilename(group, gpx);

                  if (gpxfilename != "") {

                     // Rel. Pfade auf Standort der Listendatei beziehen!
                     string picturereferencepath = null;

                     gpxfilename = FSofTUtils.PathHelper.GetFullPathAppliedDirectory(FSofTUtils.PathHelper.ReplaceEnvironmentVars(gpxfilename),
                                                                                     filenamelist);
                     string name = gpxListReader.GpxName(group, gpx);
                     if (name == "")
                        name = Path.GetFileNameWithoutExtension(gpxfilename);

                     string gpxpicturefilename = gpxListReader.GpxPictureGpxFilename(group, gpx);
                     if (gpxpicturefilename != "") {
                        gpxpicturefilename = FSofTUtils.PathHelper.GetFullPathAppliedDirectory(FSofTUtils.PathHelper.ReplaceEnvironmentVars(gpxpicturefilename),
                                                                                               filenamelist);
                        picturereferencepath = Path.GetDirectoryName(filenamelist);
                     }

                     TNode tnnew = addGpxfileTreeNode(tngroup ?? tnmain,
                                                      gpxfilename,
                                                      gpxpicturefilename,
                                                      picturereferencepath,
                                                      name,
                                                      gpxListReader.GpxTrackcolorNo(group, gpx),
                                                      tnnext);
                  }
               }
            }
            root.AppendChild(tnmain);
            result.Add(tnmain);

         } catch (Exception ex) {
            ShowExceptionEvent?.Invoke(this, new SendExceptionEventArgs(ex));
         }

         return result; // Liste ist eigentlich nicht mehr nötig
      }

      /// <summary>
      /// erzeugt einen <see cref="TNode"/> für einen "Delimiter"
      /// </summary>
      /// <param name="tnc">Node-Sammlung zu der der neue Node hinzugefügt wird</param>
      /// <param name="txt"></param>
      /// <param name="tnnext">null oder der TreeNode, vor dem der Delimiter eingefügt wird</param>
      /// <returns></returns>
      TNode addDelimiterTreeNode(TNode parent, string txt, TNode tnnext = null) {
         TNode tn = new TNode(TNode.NodeType.Delimiter) {
            VisualNode_Text = txt,
            VisualNode_Backcolor = delimiterTreeNodeColor,
         };

         int idx = parent != null ?
                     parent.ChildNodes.IndexOf(tnnext) :
                     -1;
         parent.InsertChild(idx, tn); // idx < 0 => Append

         return tn;
      }

      /// <summary>
      /// entfernt den <see cref="TNode"/> und die Anzeige der GPX-Datei
      /// </summary>
      /// <param name="tn"></param>
      void removeGpxfileNode(TNode tn) {
         if (tn != null) {
            if (tn.Nodetype == TNode.NodeType.GpxObject) {
               tn.Gpx.Markers4StandardVisible = false;
               tn.Gpx.Markers4PicturesVisible = false;

               ShowAllMarkerEvent?.Invoke(this, new ShowMarkerEventArgs(tn.Gpx, tn.Gpx.Markers4StandardVisible));
               ShowAllFotoMarkerEvent?.Invoke(this, new ShowMarkerEventArgs(tn.Gpx, tn.Gpx.Markers4PicturesVisible));
               foreach (Track track in tn.Gpx.TrackList)
                  ShowTrackEvent?.Invoke(this, new ShowTrackEventArgs(track, false));

               tn.Parent.RemoveChild(tn);
            }
         }
      }

      List<Track> getMarkedTracksFromTreeView(List<TNode> tnlst) {
         List<Track> tracklst = new List<Track>();
         List<TNode> tdlst = tnlst == null ?
                                 root.ChildNodes :
                                 tnlst;
         for (int i = 0; i < tdlst.Count; i++) {
            TNode tn = tdlst[i];

            if (tn.VisualNode_IsChecked) {
               if (tn != null)
                  switch (tn.Nodetype) {
                     case TNode.NodeType.GpxObject:
                        tracklst.AddRange(tn.Gpx.TrackList);
                        break;

                     case TNode.NodeType.Track:
                        if (!tracklst.Contains(tn.Track))
                           tracklst.Add(tn.Track);
                        break;
                  }
            }
            if (tn.Childs > 0)
               tracklst.AddRange(getMarkedTracksFromTreeView(tn.ChildNodes));
         }
         return tracklst;
      }

      /// <summary>
      /// alle Tracks, die das Rechteck in irgendeiner Weise berühren, sichtbar machen
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="tnlst"></param>
      void makeTracksVisible(Gpx.GpxBounds bound, IList<TNode> tnlst) {
         foreach (TNode tn in tnlst) {
            switch (tn.Nodetype) {
               case TNode.NodeType.GpxObject:
                  if (tn.Gpx.TrackList.Count > 1)
                     makeTracksVisible(bound, tn.ChildNodes);
                  else {
                     foreach (Track r in tn.Gpx.TrackList) {
                        if (r.IsCrossing(bound))
                           tn.VisualNode_IsChecked = true;
                     }
                  }
                  break;

               case TNode.NodeType.Track:
                  if (tn.Track.IsCrossing(bound))
                     tn.VisualNode_IsChecked = true;
                  break;

               case TNode.NodeType.Delimiter:
               case TNode.NodeType.Sampler:
                  if (tn.Childs > 0)    // dann ist das eine Gruppe
                     makeTracksVisible(bound, tn.ChildNodes);
                  break;

            }
         }
      }

      /// <summary>
      /// rekursive Suche nach dem 1. <see cref="TNode"/> für das GPX-Objekt
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="tnlst"></param>
      /// <returns></returns>
      TNode getNode4Gpx(PoorGpxAllExt gpx, IList<TNode> tnlst) {
         TNode result = null;
         foreach (TNode tn in tnlst)
            BaseTreenode.Walk(tn,
                              (BaseTreenode btn) => {
                                 TNode tnode = btn as TNode;
                                 if (tnode.Nodetype == TNode.NodeType.GpxObject &&
                                     tnode.Gpx.Equals(gpx)) {
                                    result = tnode;
                                    return true;      // Abbruch der Suche
                                 }
                                 return false;
                              });
         return result;
      }


      /// <summary>
      /// rekursive Suche eines <see cref="TNode"/> für den Track
      /// </summary>
      /// <param name="track"></param>
      /// <param name="tnlst"></param>
      /// <returns></returns>
      TNode getNode4Track(Track track, IList<TNode> tnlst) {
         TNode result = null;
         foreach (TNode tn in tnlst)
            BaseTreenode.Walk(tn,
                              (BaseTreenode btn) => {
                                 TNode tnode = btn as TNode;
                                 switch (tnode.Nodetype) {
                                    case TNode.NodeType.Track:
                                       if (tnode.Track.Equals(track)) {
                                          result = tnode;
                                          return true;      // Abbruch der Suche
                                       }
                                       break;

                                    case TNode.NodeType.GpxObject: // falls GPX-Datei mit nur einem Track
                                       if (tnode.Gpx.TrackList.Count == 1 &&
                                           tnode.Gpx.TrackList[0].Equals(track)) {
                                          result = tnode;
                                          return true;      // Abbruch der Suche
                                       }
                                       break;
                                 }
                                 return false;
                              });
         return result;

         //foreach (TNode tn in tnlst) {
         //   switch (tn.Nodetype) {
         //      case TNode.NodeType.Track:             // Tracksegment-Ebene
         //         if (tn.Track.Equals(track))
         //            return tn;
         //         break;

         //      case TNode.NodeType.GpxObject:         // "Datei"-Ebene
         //         for (int i = 0; i < tn.Gpx.TrackList.Count; i++) {
         //            if (tn.Gpx.TrackList[i].Equals(track))
         //               return tn.Childs > i ?
         //                              tn.ChildNodes[i] :
         //                              tn;
         //         }
         //         break;
         //   }
         //   TNode tnresult = getNode4Track(track, tn.ChildNodes);
         //   if (tnresult != null)
         //      return tnresult;
         //}
         //return null;
      }

      /// <summary>
      /// liefert die Liste aller <see cref="PoorGpxAllExt"/> der Subnodes des TreeNode
      /// </summary>
      /// <param name="parent"></param>
      /// <returns></returns>
      List<PoorGpxAllExt> getAllGpxContainerFromSubnodes(TNode parent) {
         List<PoorGpxAllExt> lst = new List<PoorGpxAllExt>();

         foreach (TNode tn in parent.ChildNodes) {
            switch (tn.Nodetype) {
               case TNode.NodeType.Track:
                  if (tn.Track.GpxDataContainer != null &&
                      !lst.Contains(tn.Track.GpxDataContainer))
                     lst.Add(tn.Track.GpxDataContainer);
                  break;

               case TNode.NodeType.GpxObject:
                  if (!lst.Contains(tn.Gpx))
                     lst.Add(tn.Gpx);
                  break;
            }
         }

         return lst;
      }

      /// <summary>
      /// Ist die Sichtbarkeit aller <see cref="Track"/> im gemeinsamen <see cref="GpxDataContainer"/> dieses <see cref="Track"/> gleich?
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      bool isSameTrackVisibilityInContainer(Track track) {
         if (track.GpxDataContainer != null) {
            if (track.GpxDataContainer.TrackList.Count < 2)
               return true;
            TNode tn = getNode4Track(track, root.ChildNodes);
            if (tn != null) {
               foreach (Track t in track.GpxDataContainer.TrackList) {
                  if (!track.Equals(t)) {
                     TNode tnt = getNode4Track(t, root.ChildNodes);
                     if (tnt != null &&
                         tn.VisualNode_IsChecked != tnt.VisualNode_IsChecked)
                        return false;
                  }
               }
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// liefert (rekursiv) eine Liste aller <see cref="TNode"/>, in denen der Text enthalten ist (caseinsensitiv)
      /// </summary>
      /// <param name="tnlst">Startliste der Nodes</param>
      /// <param name="searchpattern">Suchtext</param>
      /// <returns></returns>
      List<TNode> getNodesWithText(IList<TNode> tnlst, string searchpattern) {
         searchpattern = searchpattern.ToUpper();
         List<TNode> lst = new List<TNode>();
         foreach (TNode tn in tnlst)
            BaseTreenode.Walk(tn,
                              lst,
                              (BaseTreenode btn, List<TNode> result) => {
                                 TNode tnode = btn as TNode;
                                 if (tnode.VisualNode_Text.ToUpper().Contains(searchpattern))
                                    result.Add(tnode);
                              });
         return lst;
      }

      #endregion

      #region Suche (OS-abhängig)

      private void button_Search_Click(object sender, EventArgs e) {
         string searchPattern = textBox_SearchText.Text.Trim();
         if (searchPattern != "") {
            Cursor orgcursor = Cursor;
            Cursor = Cursors.WaitCursor;

            List<TNode> result = getNodesWithText(root.ChildNodes, searchPattern.ToUpper());
            listBox_Found.Tag = result;
            listBox_Found.Items.Clear();
            foreach (TNode tn in result)
               listBox_Found.Items.Add(tn.VisualNode_Text);

            Cursor = orgcursor;
         }
      }

      private void listBox_Found_SelectedIndexChanged(object sender, EventArgs e) {
         ListBox lb = sender as ListBox;
         if (lb.SelectedIndex >= 0) {
            TNode tn = (lb.Tag as List<TNode>)[lb.SelectedIndex];
            root.SelectedNode = tn;
            root.EnsureNodeVisible(tn);
         }
      }

      private void textBox_SearchText_TextChanged(object sender, EventArgs e) {
         button_Search.Enabled = (sender as TextBox).Text.Trim().Length > 0 && !LoadGpxfilesIsRunning;
      }

      #endregion

      #region public Funktionen (OS-unabhängig)

      /* "Ausgewählt" (SelectedNode) kann immer nur 1 Objekt sein.
       * "Sichtbar/Unsichtbar" (Checked) können beliebig viele Objekte sein.
       */

      /// <summary>
      /// erzeugt die TreeNodes für eine GPX-Dateiliste oder eine einzelne GPX-Datei und startet das Laden der Daten für die neuen TreeNodes (asynchrone falls mehrere Dateien,
      /// synchron für Einzeldatei)
      /// </summary>
      /// <param name="filename"></param>
      public void AddFile(string filename) {
         filename = FSofTUtils.PathHelper.GetFullPathAppliedCurrentDirectory(FSofTUtils.PathHelper.ReplaceEnvironmentVars(filename));

         List<TNode> tnlst;
         string ext = Path.GetExtension(filename).ToLower();
         if (ext == ".gpx" ||
             ext == ".kml" ||
             ext == ".kmz" ||
             ext == ".gdb") { // einzelne GPX-Datei
            tnlst = new List<TNode> {
               addGpxfileTreeNode(root,
                                  filename,
                                  null,
                                  null,
                                  null,
                                  1,
                                  root.FirstChildnode)
            };
         } else {                                               // dann sollte es eine Datei-Liste sein
            tnlst = addGpxFilelistTreeNodes(filename,
                                            root.FirstChildnode);
         }

         if (tnlst.Count > 1 ||
             (tnlst.Count > 0 && tnlst[0].ChildNodes.Count > 1)) {  // asynchr. einlesen
            new Loader4GpxFile(this).LoadGpxfiles4TreeNodes(tnlst, 2);     //Debug.WriteLine("### " + Environment.ProcessorCount);
         } else {
            new Loader4GpxFile(this).LoadGpxfiles4TreeNodes(tnlst);     // synchr. Einlesen für Einzel-TreeNode
            if (root.FirstChildnode != null)
               root.FirstChildnode.VisualNode_IsChecked = true;
         }
      }


      /// <summary>
      /// liefert ALLE vorhandenen Tracks
      /// </summary>
      /// <returns></returns>
      public List<Track> GetAllTracks() {
         List<Track> tracks = new List<Track>();
         foreach (TNode tn in root.ChildNodes)
            BaseTreenode.Walk(tn,
                              tracks,
                              (BaseTreenode btn, List<Track> result) => {
                                 TNode tnode = btn as TNode;

                                 switch (tnode.Nodetype) {
                                    case TNode.NodeType.GpxObject:
                                       tracks.AddRange(tnode.Gpx.TrackList);
                                       break;

                                    case TNode.NodeType.Track:
                                       // Diese Tracks sind schon alle durch den übergeordneten GpxObject-Node registriert.
                                       //if (!tracks.Contains(tnode.Track))
                                       //   tracks.Add(tnode.Track);
                                       break;
                                 }
                              });
         return tracks;
      }

      /// <summary>
      /// liefert ALLE Marker
      /// </summary>
      /// <returns></returns>
      public List<Marker> GetAllMarkers() {
         List<Marker> markers = new List<Marker>();
         foreach (TNode tn in root.ChildNodes) {
            BaseTreenode.Walk(tn,
                              markers,
                              (BaseTreenode btn, List<Marker> result) => {
                                 TNode tnode = btn as TNode;
                                 if (tnode.Nodetype == TNode.NodeType.GpxObject) {
                                    markers.AddRange(tnode.Gpx.MarkerListPictures);
                                    markers.AddRange(tnode.Gpx.MarkerList);
                                 }
                              });
         }
         return markers;
      }


      /// <summary>
      /// alle Tracks, die das Rechteck in irgendeiner Weise berühren, werden auf "sichtbar" gesetzt
      /// </summary>
      /// <param name="bound"></param>
      public void ShowTracks(Gpx.GpxBounds bound) {
         makeTracksVisible(bound, root.ChildNodes);
      }

      /// <summary>
      /// setzt den Track auf "sichtbar" oder "unsichtbar"
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      public void ShowTrack(Track track, bool visible) {
         TNode tn = getNode4Track(track, root.ChildNodes);
         if (tn != null) {
            if (tn.VisualNode_IsChecked != visible)
               tn.VisualNode_IsChecked = visible;
         }
      }

      /// <summary>
      /// ALLE Tracks werden auf "unsichtbar" gesetzt
      /// </summary>
      public void HideAllTracks() {
         foreach (TNode tn in getMarkedNodesFromTreeView(root.ChildNodes))
            tn.VisualNode_IsChecked = false;
      }

      /// <summary>
      ///  liefert alle "sichtbaren" Tracks 
      /// </summary>
      /// <returns></returns>
      public List<Track> GetVisibleTracks() {
         return getMarkedTracksFromTreeView(root.ChildNodes);
      }

      /// <summary>
      /// Ist dieser Track markiert (sichtbar)
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool TrackIsVisible(Track track) {
         TNode tn = getNode4Track(track, root.ChildNodes);
         return tn != null ?
                        tn.VisualNode_IsChecked :
                        false;
      }

      /// <summary>
      /// setzt die GPX-Datei auf "sichtbar" oder "unsichtbar"
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      public void ShowGpx(PoorGpxAllExt gpx, bool visible) {
         TNode tn = getNode4Gpx(gpx, root.ChildNodes);
         if (tn != null) {
            if (tn.VisualNode_IsChecked != visible)
               tn.VisualNode_IsChecked = visible;
         }
      }

      /// <summary>
      /// Ist diese GPX-Datei markiert (sichtbar)
      /// </summary>
      /// <param name="gpx"></param>
      /// <returns></returns>
      public bool GpxIsVisible(PoorGpxAllExt gpx) {
         TNode tn = getNode4Gpx(gpx, root.ChildNodes);
         return tn != null ?
                        tn.VisualNode_IsChecked :
                        false;
      }

      /// <summary>
      /// setzt die Auswahl auf den Track
      /// </summary>
      /// <param name="track"></param>
      public void SelectTrack(Track track) {
         TNode tn = getNode4Track(track, root.ChildNodes);
         if (tn != null)
            root.SelectedNode = tn;
      }

      /// <summary>
      /// löscht das akt. ausgewählte Objekt (Track oder GPX-Datei)
      /// </summary>
      public void RemoveSelectedObject() {
         removeGpxfileNode(root.SelectedNode);
      }

      /// <summary>
      /// liefert die akt. ausgewählte <see cref="PoorGpxAllExt"/> oder den <see cref="Track"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="track"></param>
      /// <returns>false, wenn nicht (sinnvolles) ausgewählt ist</returns>
      public bool GetSelectedObject(out PoorGpxAllExt gpx, out Track track) {
         track = null;
         gpx = null;

         TNode tn = root.SelectedNode;
         if (tn != null) {
            switch (tn.Nodetype) {
               case TNode.NodeType.Track:
                  track = tn.Track;
                  break;

               case TNode.NodeType.GpxObject:
                  gpx = tn.Gpx;
                  break;

               case TNode.NodeType.Sampler:
               case TNode.NodeType.Delimiter: // kein Kontextmenü
                  return false;

               default:
                  throw new Exception("unknown TreeNodeTagData");
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// liefert alle GPX-Dateien unter dem aktuell ausgewälten Objekt
      /// </summary>
      /// <returns></returns>
      public List<PoorGpxAllExt> GetAllSubGpxContainerFromSelected() {
         return root.SelectedNode != null ?
                     getAllGpxContainerFromSubnodes(root.SelectedNode) :
                     new List<PoorGpxAllExt>();
      }

      /// <summary>
      /// für das akt. ausgewählte Objekt wird die Auswahl (Doppelklick) simuliert
      /// </summary>
      public void ChooseActualSelectedObject() {
         if (root.SelectedNode != null)
            visualTreeDoubleClick(root.SelectedNode);
      }

      #endregion


#if TESTVARIANTEN

      internal class Loader4GpxFile2 {

         delegate void SafeCallDelegate4ConcurrentBag2Void(List<TreeNode> changedTreeNodes);

         ReadOnlyTracklistControl ctrl;


         public Loader4GpxFile2(ReadOnlyTracklistControl ctrl) {
            this.ctrl = ctrl;
         }

         /// <summary>
         /// liest für alle TreeNodes die GPX-Files mit der vorgegebenen Task-Anzahl asynchron ein
         /// <para>Während des Einlesens sind diese TreeNodes Disabled.</para>
         /// </summary>
         /// <param name="tnlst"></param>
         /// <param name="taskcount">bei 0 synchrones Einlesen, sonst immer asynchron</param>
         public void LoadGpxfiles4TreeNodes(IList<TreeNode> tnlst, int taskcount = 0) {

            taskcount = 3;

            List<TreeNode> allTreeNodesList = new List<TreeNode>();
            loadSampleAllTreeNodes(allTreeNodesList, tnlst);
            foreach (TreeNode tn in allTreeNodesList)
               TreeNodeTagData.EnableTreeNode(tn, false);


            // aufteilen
            List<List<TreeNode>> joblist = new List<List<TreeNode>>();
            List<List<TreeNode>> resultlist = new List<List<TreeNode>>();
            for (int i = 0; i < taskcount; i++) {
               joblist.Add(new List<TreeNode>());
               resultlist.Add(new List<TreeNode>());
            }
            for (int i = 0, j = 0; i < allTreeNodesList.Count; i++, j++) {
               joblist[j].Add(allTreeNodesList[i]);
               if (j == joblist.Count - 1)
                  j = -1;
            }

            ConcurrentQueue<string> infos = new ConcurrentQueue<string>();

            Task.Run(() => {
               Task[] tasks = new Task[taskcount];
               for (int i = 0; i < taskcount; i++) {
                  List<TreeNode> list1 = joblist[i];
                  List<TreeNode> list2 = resultlist[i];

                  //tasks[i] = Task.Run(() => {
                  //   loadWorker(list1, list2, infos);
                  //});

                  tasks[i] = Task.Factory.StartNew(() => {
                     loadWorker(list1, list2, infos);
                  }, TaskCreationOptions.LongRunning);
               }
               Task.WaitAll(tasks);
            });

         }

         void loadSampleAllTreeNodes(List<TreeNode> treeNodesSample, IList<TreeNode> tnlst) {
            foreach (TreeNode tn in tnlst) {
               treeNodesSample.Add(tn);
               if (tn.Nodes.Count > 0) {     // rekursiv für die Subnodes
                  List<TreeNode> subtnlist = new List<TreeNode>();
                  foreach (TreeNode subtn in tn.Nodes)
                     subtnlist.Add(subtn);
                  loadSampleAllTreeNodes(treeNodesSample, subtnlist);
               }
            }
         }

         void loadWorker(List<TreeNode> treeNodesSample, List<TreeNode> changedTreeNodes, ConcurrentQueue<string> infos) {
            List<TreeNode> tnlst = new List<TreeNode>();
            TreeNode tn;
            while (treeNodesSample.Count > 0) {
               tn = treeNodesSample[0];
               treeNodesSample.RemoveAt(0);

               if (loadDataIfGpxFile(tn, infos)) {
                  changedTreeNodes.Add(tn);

                  if (infos.Count > 10) {
                     string info;
                     while (infos.Count > 1)
                        infos.TryDequeue(out info);
                     if (infos.TryDequeue(out info))
                        ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(info));
                  }
               }
               tnlst.Add(tn);

            }
            ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(""));

            foreach (TreeNode n in tnlst)
               TreeNodeTagData.EnableTreeNode(n, true);

            if (ctrl.InvokeRequired) {
               var d = new SafeCallDelegate4ConcurrentBag2Void(loadUpdateTreeNodes);
               ctrl.Invoke(d, new object[] { changedTreeNodes });
            } else
               loadUpdateTreeNodes(changedTreeNodes);

         }

         void loadUpdateTreeNodes(List<TreeNode> changedTreeNodes) {
            if (changedTreeNodes.Count > 0) {
               TreeView tv = changedTreeNodes[0].TreeView;
               tv.BeginUpdate();

               while (changedTreeNodes.Count > 0) {
                  TreeNode tn = changedTreeNodes[0];
                  changedTreeNodes.RemoveAt(0);

                  TreeNodeTagData tnd = getTreeNodeTagData(tn);
                  PoorGpxAllExt gpx = tnd.Gpx;

                  if (!tnd.ExternName &&                        // Name nicht expl. vorgeg.
                      gpx.TrackList.Count == 1 &&              // genau 1 Track -> Trackname für TreeNode übernehmen
                      gpx.TrackList[0].VisualName.Length > 0)
                     tn.Text = gpx.TrackList[0].VisualName;

                  // für jede Route (Tracksegment) ein Child-TreeNode anhängen
                  tn.Nodes.Clear();
                  if (gpx.TrackList.Count > 1)    // >1, sonst fehlt in Zukunft die "aufklapp"-Box (unnötig)
                     foreach (Track r in gpx.TrackList) {
                        tn.Nodes.Add(r.VisualName);
                        tn.LastNode.Tag = new TreeNodeTagData(r);
                     }
               }

               tv.EndUpdate();
            }
         }

         /// <summary>
         /// falls der TreeNode den Tagtype <see cref="TreeNodeTagData.TagType.GpxFilename"/> hat, werden die Daten eingelesen und neue Tag-Daten im TreeNode gesetzt
         /// <para>NICHT threadsicher!</para>
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="infos"></param>
         /// <returns></returns>
         bool loadDataIfGpxFile(TreeNode tn, ConcurrentQueue<string> infos) {
            TreeNodeTagData td;
            PoorGpxAllExt gpx = null;

            td = getTreeNodeTagData(tn);
            if (td != null &&
                td.Tagtype == TreeNodeTagData.TagType.GpxFilename) { // bisher nur Dateiname vorhanden -> Daten einlesen
               if (!td.PoorGpxAllExtIsLoading) {
                  td.PoorGpxAllExtIsLoading = true;
                  infos.Enqueue("lese '" + tn.Text + "', (" + Path.GetFileName(td.GpxFilename) + ") ...");

                  try {
                     //Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + " >>> Start: " + td.GpxFilename);
                     gpx = buildPoorGpxAllExt(td.GpxFilename, td.GpxPictureFilename, td.PictureReferencePath, td.StdColorNo);
                     //Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + " >>> Ende:  " + td.GpxFilename);
                     tn.Tag = new TreeNodeTagData(gpx);
                  } catch (Exception ex) {
                     ctrl.ShowExceptionEvent?.Invoke(this, new SendExceptionEventArgs(ex));
                     gpx = null;
                  } finally {
                     td.PoorGpxAllExtIsLoading = false;
                  }
               }
            }

            return gpx != null;
         }

      }

      internal class Loader4GpxFile {

         delegate void SafeCallDelegate4ConcurrentBag2Void(List<TreeNode> changedTreeNodes);

         ReadOnlyTracklistControl ctrl;


         public Loader4GpxFile(ReadOnlyTracklistControl ctrl) {
            this.ctrl = ctrl;
         }


         AutoResetEvent[] waitHandles = null;


         /// <summary>
         /// liest für alle TreeNodes die GPX-Files mit der vorgegebenen Task-Anzahl asynchron ein
         /// <para>Während des Einlesens sind diese TreeNodes Disabled.</para>
         /// </summary>
         /// <param name="tnlst"></param>
         /// <param name="taskcount">bei 0 synchrones Einlesen, sonst immer asynchron</param>
         public void LoadGpxfiles4TreeNodes(IList<TreeNode> tnlst, int taskcount = 0) {

            taskcount = 3;

            List<TreeNode> allTreeNodesList = new List<TreeNode>();
            loadSampleAllTreeNodes(allTreeNodesList, tnlst);
            foreach (TreeNode tn in allTreeNodesList)
               TreeNodeTagData.EnableTreeNode(tn, false);


            // aufteilen
            List<List<TreeNode>> joblist = new List<List<TreeNode>>();
            List<List<TreeNode>> resultlist = new List<List<TreeNode>>();
            for (int i = 0; i < taskcount; i++) {
               joblist.Add(new List<TreeNode>());
               resultlist.Add(new List<TreeNode>());
            }
            for (int i = 0, j = 0; i < allTreeNodesList.Count; i++, j++) {
               joblist[j].Add(allTreeNodesList[i]);
               if (j == joblist.Count - 1)
                  j = -1;
            }

            ConcurrentQueue<string> infos = new ConcurrentQueue<string>();

            // 51

            Task.Run(() => {

               System.Runtime.GCLatencyMode orgmode = System.Runtime.GCSettings.LatencyMode;
               //System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;

               GC.TryStartNoGCRegion(500*1000*1000, false);

               waitHandles = new AutoResetEvent[taskcount];
               for (int i = 0; i < taskcount; i++)
                  waitHandles[i] = new AutoResetEvent(false);

               for (int i = 0; i < taskcount; i++) {
                  List<TreeNode> list1 = joblist[i];
                  List<TreeNode> list2 = resultlist[i];
                  AutoResetEvent waitHandle = waitHandles[i];

                  Thread t = new Thread(loadWorkerThread);
                  t.Start(new object[] {
                     list1,
                     list2,
                     infos,
                     waitHandle
                  });
               }

               WaitHandle.WaitAll(waitHandles);


               GC.EndNoGCRegion();
               System.Runtime.GCSettings.LatencyMode = orgmode;
            });

         }

         void loadWorkerThread(object arg) {
            object[] args = arg as object[];

            loadWorker(
               args[0] as List<TreeNode>,
               args[1] as List<TreeNode>,
               args[2] as ConcurrentQueue<string>,
               args[3] as AutoResetEvent
            );
         }


         void loadSampleAllTreeNodes(List<TreeNode> treeNodesSample, IList<TreeNode> tnlst) {
            foreach (TreeNode tn in tnlst) {
               treeNodesSample.Add(tn);
               if (tn.Nodes.Count > 0) {     // rekursiv für die Subnodes
                  List<TreeNode> subtnlist = new List<TreeNode>();
                  foreach (TreeNode subtn in tn.Nodes)
                     subtnlist.Add(subtn);
                  loadSampleAllTreeNodes(treeNodesSample, subtnlist);
               }
            }
         }

         void loadWorker(List<TreeNode> treeNodesSample, List<TreeNode> changedTreeNodes, ConcurrentQueue<string> infos, AutoResetEvent waitHandle) {
            List<TreeNode> tnlst = new List<TreeNode>();
            TreeNode tn;
            while (treeNodesSample.Count > 0) {
               tn = treeNodesSample[0];
               treeNodesSample.RemoveAt(0);

               if (loadDataIfGpxFile(tn, infos)) {
                  changedTreeNodes.Add(tn);

                  if (infos.Count > 10) {
                     string info;
                     while (infos.Count > 1)
                        infos.TryDequeue(out info);
                     if (infos.TryDequeue(out info))
                        ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(info));
                  }
               }
               tnlst.Add(tn);

            }
            ctrl.LoadinfoEvent?.Invoke(this, new SendStringEventArgs(""));

            foreach (TreeNode n in tnlst)
               TreeNodeTagData.EnableTreeNode(n, true);

            if (ctrl.InvokeRequired) {
               var d = new SafeCallDelegate4ConcurrentBag2Void(loadUpdateTreeNodes);
               ctrl.Invoke(d, new object[] { changedTreeNodes });
            } else
               loadUpdateTreeNodes(changedTreeNodes);

            waitHandle.Set();
         }

         void loadUpdateTreeNodes(List<TreeNode> changedTreeNodes) {
            if (changedTreeNodes.Count > 0) {
               TreeView tv = changedTreeNodes[0].TreeView;
               tv.BeginUpdate();

               while (changedTreeNodes.Count > 0) {
                  TreeNode tn = changedTreeNodes[0];
                  changedTreeNodes.RemoveAt(0);

                  TreeNodeTagData tnd = getTreeNodeTagData(tn);
                  PoorGpxAllExt gpx = tnd.Gpx;

                  if (!tnd.ExternName &&                        // Name nicht expl. vorgeg.
                      gpx.TrackList.Count == 1 &&              // genau 1 Track -> Trackname für TreeNode übernehmen
                      gpx.TrackList[0].VisualName.Length > 0)
                     tn.Text = gpx.TrackList[0].VisualName;

                  // für jede Route (Tracksegment) ein Child-TreeNode anhängen
                  tn.Nodes.Clear();
                  if (gpx.TrackList.Count > 1)    // >1, sonst fehlt in Zukunft die "aufklapp"-Box (unnötig)
                     foreach (Track r in gpx.TrackList) {
                        tn.Nodes.Add(r.VisualName);
                        tn.LastNode.Tag = new TreeNodeTagData(r);
                     }
               }

               tv.EndUpdate();
            }
         }

         /// <summary>
         /// falls der TreeNode den Tagtype <see cref="TreeNodeTagData.TagType.GpxFilename"/> hat, werden die Daten eingelesen und neue Tag-Daten im TreeNode gesetzt
         /// <para>NICHT threadsicher!</para>
         /// </summary>
         /// <param name="tn"></param>
         /// <param name="infos"></param>
         /// <returns></returns>
         bool loadDataIfGpxFile(TreeNode tn, ConcurrentQueue<string> infos) {
            TreeNodeTagData td;
            PoorGpxAllExt gpx = null;

            td = getTreeNodeTagData(tn);
            if (td != null &&
                td.Tagtype == TreeNodeTagData.TagType.GpxFilename) { // bisher nur Dateiname vorhanden -> Daten einlesen
               if (!td.PoorGpxAllExtIsLoading) {
                  td.PoorGpxAllExtIsLoading = true;
                  infos.Enqueue("lese '" + tn.Text + "', (" + Path.GetFileName(td.GpxFilename) + ") ...");

                  try {
                     //Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + " >>> Start: " + td.GpxFilename);
                     gpx = buildPoorGpxAllExt(td.GpxFilename, td.GpxPictureFilename, td.PictureReferencePath, td.StdColorNo);
                     //Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + " >>> Ende:  " + td.GpxFilename);
                     tn.Tag = new TreeNodeTagData(gpx);

                     GC.Collect();

                  } catch (Exception ex) {
                     ctrl.ShowExceptionEvent?.Invoke(this, new SendExceptionEventArgs(ex));
                     gpx = null;
                  } finally {
                     td.PoorGpxAllExtIsLoading = false;
                  }
               }
            }

            return gpx != null;
         }

      }
#endif

   }
}
