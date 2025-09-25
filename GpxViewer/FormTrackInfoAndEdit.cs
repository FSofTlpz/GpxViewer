using FSofTUtils.Geography;
using SpecialMapCtrl;
using System.Data;
using System.Text;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public partial class FormTrackInfoAndEdit : Form {

      #region Events

      public class SelectedPointsEventArgs : EventArgs {
         /// <summary>
         /// Track
         /// </summary>
         public Track Track { get; private set; }

         /// <summary>
         /// Index-Liste der markierten Punkte
         /// </summary>
         public List<int> PointList { get; private set; }

         public SelectedPointsEventArgs(Track track, List<int> ptlst) {
            Track = track;
            PointList = ptlst;
         }
      }

      /// <summary>
      /// die Selektion der Punkte hat sich verändert
      /// </summary>
      public event EventHandler<SelectedPointsEventArgs>? SelectedPoints;

      /// <summary>
      /// die selektierten Punkte werden entfernt
      /// </summary>
      public event EventHandler<SelectedPointsEventArgs>? SelectedPointsRemoving;

      #endregion

      /// <summary>
      /// <see cref="GpxViewer.Track"/> für die die Info ist
      /// </summary>
      public Track? Track { get; protected set; } = null;

      /// <summary>
      /// <see cref="GpxData"/> für die die Info ist
      /// </summary>
      public GpxData? GpxObject { get; protected set; } = null;

      /// <summary>
      /// Wurden Daten geändert?
      /// </summary>
      public bool TrackChanged { get; private set; }

      /// <summary>
      /// Stimmt die interne Datenstruktur mit den Form-Daten überein?
      /// </summary>
      /// <returns></returns>
      bool TrackIsUnsaved {
         get {
            return !TrackIsReadOnly &&
                    (
                     (Track.GpxTrack.Name ?? "") != textBoxName.Text.Trim() ||                                                 // Name geändert
                     (Track.GpxTrack.Description ?? "") != textBoxDescription.Text.Trim() ||
                     (Track.GpxTrack.Comment ?? "") != textBoxComment.Text.Trim() ||
                     (Track.GpxTrack.Source ?? "") != textBoxSource.Text.Trim()
                    );
         }
      }

      /// <summary>
      /// Änderungen in der Punktliste?
      /// </summary>
      bool pointschanged = false;

      /// <summary>
      /// Kann der Track verändert werden?
      /// </summary>
      public bool TrackIsReadOnly = false;

      /// <summary>
      /// Control für die Info zur gesamten GPX-Datei
      /// </summary>
      RichTextBox? ctrlGpxfileInfo;

      /// <summary>
      /// Liste der <see cref="RichTextBox"/> für die einzelnen Trackprofile
      /// </summary>
      readonly List<RichTextBox> ctrlTrackInfo = new List<RichTextBox>();

      /// <summary>
      /// Liste der <see cref="PictureBox"/> für die einzelnen Trackprofile
      /// </summary>
      readonly List<PictureBox> ctrlProfilePicture = new List<PictureBox>();


      public FormTrackInfoAndEdit() {
         InitializeComponent();
      }

      public FormTrackInfoAndEdit(Track track, string caption) :
         this() {
         Track = track;
         Text = caption;
      }

      public FormTrackInfoAndEdit(GpxData gpx, string caption) :
         this() {
         GpxObject = gpx;

         if (GpxObject.TrackList.Count == 1)
            Track = GpxObject.TrackList[0];

         Text = caption;
      }

      private void FormExtTrackInfoAndEdit_Load(object sender, EventArgs e) {
         TrackChanged = false;

         buildInfoControls(splitContainer1.Panel2,
                           richTextBoxInfo1,
                           pictureBoxProfile1);
         fillGpxInfoControl(ctrlGpxfileInfo);
         locateAndResizeControls();

         //Gpx.Metadata.Time        lohnt nicht

         if (Track == null ||
             !Track.IsEditable ||
             TrackIsReadOnly) {
            dataGridViewPoints.ReadOnly =
            textBoxName.ReadOnly =
            textBoxDescription.ReadOnly =
            textBoxComment.ReadOnly =
            textBoxSource.ReadOnly = true;
            button_Save.Enabled = false;
         }

         if (Track != null) {
            textBoxName.Text = Track.GpxTrack.Name;
            textBoxDescription.Text = Track.GpxTrack.Description;
            textBoxComment.Text = Track.GpxTrack.Comment;
            textBoxSource.Text = Track.GpxTrack.Source;
            showTrackLength();

            fillDataGridView();
         } else {
            splitContainer1.Height += splitContainer1.Top - 2;
            splitContainer1.Top = 2;
            splitContainer1.Panel1.Controls.Clear();
            splitContainer1.SplitterDistance = 0;
            splitContainer1.IsSplitterFixed = true;
         }
      }

      private void FormExtTrackInfoAndEdit_Shown(object sender, EventArgs e) {
         for (int i = 0; i < ctrlProfilePicture.Count; i++)
            ctrlProfilePicture[i].Image = Common.TrackHeightProfile.BuildImage4Track(
                                                            ctrlProfilePicture[i].ClientSize.Width,
                                                            ctrlProfilePicture[i].ClientSize.Height,
                                                            GpxObject == null ? Track : GpxObject.TrackList[i],
                                                            getSelectedPoints());
      }

      private void Pb_SizeChanged(object? sender, EventArgs e) {
         PictureBox? pb = sender as PictureBox;
         if (pb.Tag != null)
            pb.Image = Common.TrackHeightProfile.BuildImage4Track(pb.ClientSize.Width,
                                                                  pb.ClientSize.Height,
                                                                  (Track)pb.Tag,
                                                                  getSelectedPoints());
      }

      private void FormExtTrackInfoAndEdit_KeyDown(object sender, KeyEventArgs e) {
         switch (e.KeyData) {
            case Keys.Escape:
               Close();
               break;
         }
      }

      private void FormExtTrackInfoAndEdit_FormClosing(object sender, FormClosingEventArgs e) {
         base.OnClosing(e);

         if (TrackIsUnsaved)
            if (MessageBox.Show("Geänderte Track-Daten übernehmen?", "Speichern", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
               saveTrack();

         if (!TrackIsReadOnly &&
             !TrackChanged)         // kann nur durch button_Save_Click() true sein
            TrackChanged = pointschanged || TrackIsUnsaved;

         if (!Modal)
            Owner.RemoveOwnedForm(this);     // Owner ist danach null !
      }

      private void FormExtTrackInfoAndEdit_ClientSizeChanged(object sender, EventArgs e) {
         locateAndResizeControls();
      }

      private void ToolStripMenuItem_CopyText_Click(object sender, EventArgs e) {
         ToolStripMenuItem? tmi = sender as ToolStripMenuItem;
         ContextMenuStrip? cms = tmi.GetCurrentParent() as ContextMenuStrip;
         Clipboard.SetText(cms.SourceControl.Text, TextDataFormat.Text);
      }

      private void ToolStripMenuItem_CopyMarkedText_Click(object sender, EventArgs e) {
         ToolStripMenuItem? tmi = sender as ToolStripMenuItem;
         ContextMenuStrip? cms = tmi.GetCurrentParent() as ContextMenuStrip;
         Clipboard.SetText((cms.SourceControl as RichTextBox).SelectedText, TextDataFormat.Text);
      }

      private void ToolStripMenuItem_CopyPicture_Click(object sender, EventArgs e) {
         ToolStripMenuItem? tmi = (ToolStripMenuItem)sender;
         ContextMenuStrip? cms = (ContextMenuStrip?)tmi.GetCurrentParent();
         if (cms.SourceControl != null &&
             (cms.SourceControl as PictureBox).Image != null)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
            Clipboard.SetImage(new Bitmap(((PictureBox)cms.SourceControl).Image));
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
      }

      private void button_Save_Click(object sender, EventArgs e) {
         if (TrackIsUnsaved) {
            saveTrack();
            TrackChanged = true;
         }
      }

      private void contextMenuStripText_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ContextMenuStrip? cms = sender as ContextMenuStrip;
         RichTextBox? rtb = cms.SourceControl as RichTextBox;
         ToolStripMenuItem_CopyMarkedText.Enabled = false;
         if (rtb != null &&
             !string.IsNullOrEmpty(rtb.SelectedText))
            ToolStripMenuItem_CopyMarkedText.Enabled = true;
      }

      /// <summary>
      /// notwendige Info-Controls erzeugen
      /// </summary>
      /// <param name="parent"></param>
      /// <param name="masterctrl">1. (im Designer angelegtes) Control für die Trackinfo</param>
      /// <param name="masterpicturebox">1. (im Designer angelegtes) Control für das Trackprofil</param>
      void buildInfoControls(Control parent,
                             RichTextBox masterctrl,
                             PictureBox masterpicturebox) {
         string baselabelname = masterctrl.Name;

         if (GpxObject != null ||                           // GPX-Datei oder ...
             (Track != null &&                              // ... Track mit ParentGpx die nur genau 1 Route enthält
              Track.GpxDataContainer != null &&
              Track.GpxDataContainer.TrackList.Count == 1)) {

            if (GpxObject == null)
#pragma warning disable CS8601 // Mögliche Nullverweiszuweisung.
               GpxObject = Track.GpxDataContainer;
#pragma warning restore CS8601 // Mögliche Nullverweiszuweisung.

            ctrlGpxfileInfo = new RichTextBox() {
               Anchor = masterctrl.Anchor,
               AutoSize = masterctrl.AutoSize,
               BackColor = masterctrl.BackColor,
               BorderStyle = masterctrl.BorderStyle,
               ContextMenuStrip = masterctrl.ContextMenuStrip,
               Location = new Point(0, 2),
               Name = baselabelname + "gpx",
               Size = masterctrl.Size,
               Multiline = masterctrl.Multiline,
               ScrollBars = masterctrl.ScrollBars,
               WordWrap = masterctrl.WordWrap,
               ReadOnly = masterctrl.ReadOnly,
               TabIndex = 0,
            };
            parent.Controls.Add(ctrlGpxfileInfo);
         }

         ctrlTrackInfo.Add(masterctrl);
         ctrlProfilePicture.Add(masterpicturebox);

         if (Track != null) { // nur 1 Route
            masterpicturebox.Tag = Track;
         } else {
            masterpicturebox.Tag = GpxObject.TrackList[0];

            if (ctrlGpxfileInfo != null)
               ctrlTrackInfo[0].TabIndex++;

            // Controls für die Tracks 2, ... usw. erzeugen
            string basepictureboxname = masterpicturebox.Name;
            for (int i = 1; i < GpxObject.TrackList.Count; i++) {
               RichTextBox? label = new RichTextBox() {
                  Anchor = masterctrl.Anchor,
                  AutoSize = masterctrl.AutoSize,
                  BackColor = masterctrl.BackColor,
                  BorderStyle = masterctrl.BorderStyle,
                  ContextMenuStrip = masterctrl.ContextMenuStrip,
                  Name = "label" + (i + 1).ToString(),
                  Size = masterctrl.Size,
                  Multiline = masterctrl.Multiline,
                  ScrollBars = masterctrl.ScrollBars,
                  WordWrap = masterctrl.WordWrap,
                  ReadOnly = masterctrl.ReadOnly,
                  TabIndex = masterctrl.TabIndex + 2 * i,
               };
               parent.Controls.Add(label);
               ctrlTrackInfo.Add(label);

               PictureBox? pb = new PictureBox() {
                  ContextMenuStrip = masterpicturebox.ContextMenuStrip,
                  Name = masterpicturebox.Name = basepictureboxname + (i + 1).ToString(),
                  Size = masterpicturebox.Size,
                  SizeMode = masterpicturebox.SizeMode,
                  TabIndex = label.TabIndex + 1,
                  TabStop = masterpicturebox.TabStop,
               };
               pb.Tag = GpxObject.TrackList[i];
               parent.Controls.Add(pb);
               ctrlProfilePicture.Add(pb);
            }
         }

         for (int i = 0; i < ctrlProfilePicture.Count; i++) {
            ctrlTrackInfo[i].Text = (ctrlProfilePicture[i].Tag as Track).GetSimpleStatsText().Trim();
            ctrlProfilePicture[i].Left = masterctrl.Left;
            ctrlProfilePicture[i].SizeChanged += Pb_SizeChanged;
         }

      }

      /// <summary>
      /// Info-Control für GPX-Datei füllen
      /// </summary>
      /// <param name="ctrl"></param>
      void fillGpxInfoControl(Control? ctrl) {
         if (ctrl != null) {
            StringBuilder? sb = new StringBuilder();

            if (!string.IsNullOrEmpty(GpxObject.GpxFilename)) {
               sb.AppendLine("GPX-Datei: " + GpxObject.GpxFilename);
               try {
                  FileInfo? fi = new FileInfo(GpxObject.GpxFilename);
                  sb.AppendFormat("{0:F1} kB ({1} Bytes), ", fi.Length / 1024.0, fi.Length);
                  sb.AppendLine(fi.LastWriteTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
               } catch (Exception ex) {
                  MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
               }
            }

            if (GpxObject.Metadata.Bounds.IsValid())
               sb.AppendLine(string.Format("Metadaten Bounds: Lon {0}° .. {1}°, Lat {2}° .. {3}°", GpxObject.Metadata.Bounds.MinLon, GpxObject.Metadata.Bounds.MaxLon, GpxObject.Metadata.Bounds.MinLat, GpxObject.Metadata.Bounds.MaxLat));
            if (GpxObject.Metadata.Time != Gpx.BaseElement.NOTUSE_TIME)
               sb.AppendLine(string.Format("Metadaten Zeit: " + GpxObject.Metadata.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)")));

            sb.AppendLine("Waypoints: " + GpxObject.Waypoints.Count.ToString());
            sb.AppendLine("Tracks: " + GpxObject.Tracks.Count.ToString());
            for (int i = 0; i < GpxObject.Tracks.Count; i++) {
               sb.AppendLine("   Track " + (i + 1).ToString());
               sb.AppendLine("      Segmente: " + GpxObject.Tracks[i].Segments.Count.ToString());
               int p = 0;
               for (int j = 0; j < GpxObject.Tracks[i].Segments.Count; j++)
                  p += GpxObject.Tracks[i].Segments[j].Points.Count;
               sb.AppendLine("      Punkte: " + p.ToString());
            }

            if (GpxObject.MarkerListPictures.Count > 0) {
               if (!string.IsNullOrEmpty(GpxObject.GpxPictureFilename)) {
                  sb.AppendLine("GPX-Bilderdatei: " + GpxObject.GpxPictureFilename);
                  try {
                     FileInfo? fi = new FileInfo(GpxObject.GpxPictureFilename);
                     sb.AppendFormat("{0:F1} kB ({1} Bytes), ", fi.Length / 1024.0, fi.Length);
                     sb.AppendLine(fi.LastWriteTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
                  } catch (Exception ex) {
                     MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  }
               }
               sb.AppendLine("Bildpunkte: " + GpxObject.MarkerListPictures.Count.ToString());
            }

            ctrl.Text = sb.ToString().Trim();
         }
      }

      /// <summary>
      /// Anpassung der Größe und Position der Info-Controls
      /// </summary>
      void locateAndResizeControls() {
         if (ctrlGpxfileInfo != null) {
            setControlHeight(ctrlGpxfileInfo);
            ctrlTrackInfo[0].Top = ctrlGpxfileInfo.Bottom + 5;
         }

         int width = ctrlProfilePicture[0].Parent.Width - 2 * ctrlProfilePicture[0].Left - SystemInformation.VerticalScrollBarWidth;
         for (int i = 0; i < ctrlProfilePicture.Count; i++) {
            if (i > 0)
               ctrlTrackInfo[i].Top = ctrlProfilePicture[i - 1].Bottom + 5;
            setControlHeight(ctrlTrackInfo[i]);
            ctrlProfilePicture[i].Top = ctrlTrackInfo[i].Bottom + 2;
            ctrlProfilePicture[i].Size = new Size(width, (3 * width) / 4);
         }
      }

      void setControlHeight(RichTextBox ctrl) {
         Size szText = TextRenderer.MeasureText(ctrl.Text, ctrl.Font);
         int ext = 0;
         switch (ctrl.BorderStyle) {
            case BorderStyle.FixedSingle: ext += 2 * SystemInformation.Border3DSize.Height; break;
            case BorderStyle.Fixed3D: ext += 2 * SystemInformation.BorderSize.Height; break;
         }
         if (ctrl.Lines.Length > 0)
            ext += TextRenderer.MeasureText("X", ctrl.Font).Height / 2;  // 1/2 Zeile (exp.; sollte eigentlich nicht nötig sein)

         ctrl.Height = szText.Height + ext;
         if (ctrl.Height - ctrl.ClientSize.Height >= SystemInformation.HorizontalScrollBarHeight)   // hor. Scrollbar sichtbar
            ctrl.Height += SystemInformation.HorizontalScrollBarHeight;
      }

      /// <summary>
      /// übernimmt die akt. Daten in den <see cref="Track"/>
      /// </summary>
      void saveTrack() {
         if (Track.GpxDataContainer != null)
            Track.GpxDataContainer.GpxDataChanged = true;

         Track.GpxTrack.Name = textBoxName.Text.Trim();
         Track.VisualName = Track.GpxTrack.Name;
         Track.GpxTrack.Description = textBoxDescription.Text.Trim();
         Track.GpxTrack.Comment = textBoxComment.Text.Trim();
         Track.GpxTrack.Source = textBoxSource.Text.Trim();
      }

      #region Punkt/e markieren

      private void contextMenuStripPoints_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (!Track.IsEditable ||
             TrackIsReadOnly ||
             dataGridViewPoints.SelectedRows.Count == 0)
            e.Cancel = true;
      }

      private void ToolStripMenuItem_PointsRemoving_Click(object? sender, EventArgs e) {
         for (int i = dataGridViewPoints.Rows.Count - 1; i >= 0; i--)
            if (dataGridViewPoints.Rows[i].Selected)
               Track.RemovePoint(i);
         pointschanged = true;
         Track.Refresh();     // falls sichtbar, Anzeige akt.
         fillDataGridView();

         showTrackLength();

         OnSelectedPointsRemoving(new SelectedPointsEventArgs(Track, getSelectedPoints()));
      }

      private void dataGridViewPoints_SelectionChanged(object sender, EventArgs e) {
         if (Track != null) {
            OnSelectedPoints(new SelectedPointsEventArgs(Track, getSelectedPoints()));
            Pb_SizeChanged(ctrlProfilePicture[0], EventArgs.Empty);
         }
      }

      private void dataGridViewPoints_KeyDown(object sender, KeyEventArgs e) {
         if (Track.IsEditable && !TrackIsReadOnly)
            if (e.KeyData == Keys.Delete &&
                dataGridViewPoints.SelectedRows.Count > 0)
               ToolStripMenuItem_PointsRemoving_Click(null, EventArgs.Empty);
      }

      protected virtual void OnSelectedPoints(SelectedPointsEventArgs e) => SelectedPoints?.Invoke(this, e);

      protected virtual void OnSelectedPointsRemoving(SelectedPointsEventArgs e) => SelectedPointsRemoving?.Invoke(this, e);

      #endregion

      void showTrackLength() {
         double len = Track.Length();
         label_TrackLength.Text = string.Format("{0} Punkte, {1:F1} km / {2:F0} m",
                                                Track.GpxSegment.Points.Count,
                                                len / 1000,
                                                len);
      }

      void fillDataGridView() {
         dataGridViewPoints.SuspendLayout();
         dataTablePoints.Rows.Clear();
         double length = 0;
         for (int i = 0; i < Track.GpxSegment.Points.Count; i++) {
            Gpx.GpxTrackPoint pt = Track.GpxSegment.Points[i];

            DataRow? row = dataTablePoints.NewRow();
            row[dataColumnIdx] = i + 1;
            row[dataColumnLat] = string.Format("{0:F8}", pt.Lat);
            row[dataColumnLon] = string.Format("{0:F8}", pt.Lon);
            if (pt.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE)
               row[dataColumnElevation] = string.Format("{0:F3}", pt.Elevation);
            if (pt.Time != Gpx.BaseElement.NOTVALID_TIME)
               row[dataColumnTime] = pt.Time;
            if (i > 0) {
               double dist = GeoHelper.Wgs84Distance(pt.Lon, Track.GpxSegment.Points[i - 1].Lon, pt.Lat, Track.GpxSegment.Points[i - 1].Lat);
               row[dataColumnDistance] = string.Format("{0:F0}", dist);
               length += dist;
               row[dataColumnLength] = string.Format("{0:F0}", length);
            }

            dataTablePoints.Rows.Add(row);
         }
         dataGridViewPoints.ResumeLayout();
      }

      /// <summary>
      /// liefert die Index-Liste der akt. markierten Punkte
      /// </summary>
      /// <returns></returns>
      List<int> getSelectedPoints() {
         List<int> ptlst = new List<int>();
         for (int i = 0; i < dataGridViewPoints.Rows.Count; i++)
            if (dataGridViewPoints.Rows[i].Selected)
               ptlst.Add(i);
         return ptlst;
      }
   }
}
