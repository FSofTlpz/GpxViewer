using FSofTUtils.Geography;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   public partial class FormTrackInfoAndEdit : Form {

      #region Events

      public class SelectedPointsEventArgs : EventArgs {
         /// <summary>
         /// Route
         /// </summary>
         public Track Track { get; private set; }
         /// <summary>
         /// Index-Liste der markierten Punkte
         /// </summary>
         public List<int> PointList { get; private set; }

         public SelectedPointsEventArgs(Track route, List<int> ptlst) {
            Track = route;
            PointList = ptlst;
         }
      }

      /// <summary>
      /// die Selektion der Punkte hat sich verändert
      /// </summary>
      public event EventHandler<SelectedPointsEventArgs> SelectedPoints;

      /// <summary>
      /// die selektierten Punkte werden entfernt
      /// </summary>
      public event EventHandler<SelectedPointsEventArgs> SelectedPointsRemoving;

      #endregion

      /// <summary>
      /// <see cref="GpxViewer.Track"/> für die die Info ist
      /// </summary>
      public Track Track { get; protected set; } = null;

      /// <summary>
      /// <see cref="GpxAllExt"/> für die die Info ist
      /// </summary>
      public GpxAllExt GpxObject { get; protected set; } = null;

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

      private Label labelgpx;

      /// <summary>
      /// Liste der <see cref="Label"/> für Profile
      /// </summary>
      readonly List<Label> profileLabels = new List<Label>();

      /// <summary>
      /// Liste der <see cref="PictureBox"/> für Profile
      /// </summary>
      readonly List<PictureBox> profilePictureBoxes = new List<PictureBox>();




      public FormTrackInfoAndEdit() {
         InitializeComponent();
      }

      public FormTrackInfoAndEdit(Track track, string caption) :
         this() {
         Track = track;
         Text = caption;
      }

      public FormTrackInfoAndEdit(GpxAllExt gpx, string caption) :
         this() {
         GpxObject = gpx;

         if (GpxObject.TrackList.Count == 1)
            Track = GpxObject.TrackList[0];

         Text = caption;
      }

      private void FormExtTrackInfoAndEdit_Load(object sender, EventArgs e) {
         TrackChanged = false;

         BuildInfoControls(splitContainer1.Panel2,
                           labelProfile1,
                           pictureBoxProfile1);
         FillInfoControls();
         LocateAndResizeControls();

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

            FillDataGridView();
         } else {
            splitContainer1.Height += splitContainer1.Top - 2;
            splitContainer1.Top = 2;
            splitContainer1.Panel1.Controls.Clear();
            splitContainer1.SplitterDistance = 0;
            splitContainer1.IsSplitterFixed = true;
         }
      }

      private void FormExtTrackInfoAndEdit_Shown(object sender, EventArgs e) {
         for (int i = 0; i < profilePictureBoxes.Count; i++)
            profilePictureBoxes[i].Image = BuildImage4Track(profilePictureBoxes[i].ClientSize.Width,
                                                            profilePictureBoxes[i].ClientSize.Height,
                                                            GpxObject == null ? Track : GpxObject.TrackList[i],
                                                            GetSelectedPoints());
      }

      private void Pb_SizeChanged(object sender, EventArgs e) {
         PictureBox pb = sender as PictureBox;
         if (pb.Tag != null)
            pb.Image = BuildImage4Track(pb.ClientSize.Width,
                                        pb.ClientSize.Height,
                                        pb.Tag as Track,
                                        GetSelectedPoints());
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
         LocateAndResizeControls();
      }

      private void ToolStripMenuItem_CopyText_Click(object sender, EventArgs e) {
         ToolStripMenuItem tmi = sender as ToolStripMenuItem;
         ContextMenuStrip cms = tmi.GetCurrentParent() as ContextMenuStrip;
         Clipboard.SetText(cms.SourceControl.Text, TextDataFormat.Text);
      }

      private void ToolStripMenuItem_CopyPicture_Click(object sender, EventArgs e) {
         ToolStripMenuItem tmi = sender as ToolStripMenuItem;
         ContextMenuStrip cms = tmi.GetCurrentParent() as ContextMenuStrip;
         Clipboard.SetImage(new Bitmap((cms.SourceControl as PictureBox).Image));
      }

      private void button_Save_Click(object sender, EventArgs e) {
         if (TrackIsUnsaved) {
            saveTrack();
            TrackChanged = true;
         }
      }

      /// <summary>
      /// notwendige Info-Controls erzeugen
      /// </summary>
      /// <param name="parent"></param>
      /// <param name="masterlabel"></param>
      /// <param name="masterpicturebox"></param>
      void BuildInfoControls(Control parent,
                             Label masterlabel,
                             PictureBox masterpicturebox) {
         string baselabelname = masterlabel.Name;
         if (GpxObject != null ||                           // GPX-Datei oder ...
             (Track != null &&                              // ... Track mit ParentGpx die nur genau 1 Route enthält
              Track.GpxDataContainer != null &&
              Track.GpxDataContainer.TrackList.Count == 1)) {

            if (GpxObject == null)
               GpxObject = Track.GpxDataContainer;

            labelgpx = new Label() {
               Anchor = masterlabel.Anchor,
               AutoEllipsis = masterlabel.AutoEllipsis,
               AutoSize = masterlabel.AutoSize,
               BackColor = masterlabel.BackColor,
               BorderStyle = masterlabel.BorderStyle,
               ContextMenuStrip = masterlabel.ContextMenuStrip,
               Location = new Point(0, 2),
               Name = baselabelname + "gpx",
               Size = masterlabel.Size,
               TabIndex = 0,
            };
            labelgpx.Paint += Label_Paint;
            parent.Controls.Add(labelgpx);
         }

         masterlabel.Paint += Label_Paint;
         profileLabels.Add(masterlabel);
         profilePictureBoxes.Add(masterpicturebox);

         if (Track != null) { // nur 1 Route
            masterpicturebox.Tag = Track;
         } else {
            masterpicturebox.Tag = GpxObject.TrackList[0];

            if (labelgpx != null)
               profileLabels[0].TabIndex++;

            string basepictureboxname = pictureBoxProfile1.Name;
            for (int i = 1; i < GpxObject.TrackList.Count; i++) {
               Label label = new Label() {
                  Anchor = masterlabel.Anchor,
                  AutoEllipsis = masterlabel.AutoEllipsis,
                  AutoSize = masterlabel.AutoSize,
                  BackColor = masterlabel.BackColor,
                  BorderStyle = masterlabel.BorderStyle,
                  ContextMenuStrip = masterlabel.ContextMenuStrip,
                  Name = "label" + (i + 1).ToString(),
                  Size = masterlabel.Size,
                  TabIndex = masterlabel.TabIndex + 2 * i,
               };
               label.Paint += Label_Paint;
               parent.Controls.Add(label);
               profileLabels.Add(label);

               PictureBox pb = new PictureBox() {
                  ContextMenuStrip = masterpicturebox.ContextMenuStrip,
                  Name = masterpicturebox.Name = basepictureboxname + (i + 1).ToString(),
                  Size = masterpicturebox.Size,
                  SizeMode = masterpicturebox.SizeMode,
                  TabIndex = label.TabIndex + 1,
                  TabStop = masterpicturebox.TabStop,
               };
               pb.Tag = GpxObject.TrackList[i];
               parent.Controls.Add(pb);
               profilePictureBoxes.Add(pb);
            }
         }

         for (int i = 0; i < profilePictureBoxes.Count; i++) {
            profileLabels[i].Text = (profilePictureBoxes[i].Tag as Track).GetSimpleStatsText();
            profilePictureBoxes[i].Left = masterlabel.Left;
            profilePictureBoxes[i].SizeChanged += Pb_SizeChanged;
         }

      }

      /// <summary>
      /// zusätzliche (!) Paint nach dem internen Paint
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Label_Paint(object sender, PaintEventArgs e) {
         Label label = sender as Label;
         e.Graphics.Clear(label.BackColor);  // Ist nötig, um die Originalausgabe zu löschen!
         TextRenderer.DrawText(e.Graphics,
                               label.Text,
                               label.Font,
                               label.ClientRectangle,
                               label.ForeColor,
                               label.BackColor,
                               TextFormatFlags.WordEllipsis | TextFormatFlags.ModifyString);
      }

      /// <summary>
      /// Info-Controls füllen
      /// </summary>
      void FillInfoControls() {
         if (labelgpx != null) {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(GpxObject.GpxFilename)) {
               sb.AppendLine("GPX-Datei: " + GpxObject.GpxFilename);
               try {
                  FileInfo fi = new FileInfo(GpxObject.GpxFilename);
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
                     FileInfo fi = new FileInfo(GpxObject.GpxPictureFilename);
                     sb.AppendFormat("{0:F1} kB ({1} Bytes), ", fi.Length / 1024.0, fi.Length);
                     sb.AppendLine(fi.LastWriteTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
                  } catch (Exception ex) {
                     MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  }
               }
               sb.AppendLine("Bildpunkte: " + GpxObject.MarkerListPictures.Count.ToString());
            }

            labelgpx.Text = sb.ToString();
         }
      }

      /// <summary>
      /// Anpassung der Größe und Position der Info-Controls
      /// </summary>
      void LocateAndResizeControls() {
         if (labelgpx != null) {

            //labelgpx.Height = calculateLabelHeight(labelgpx);

            labelgpx.Height = TextRenderer.MeasureText(labelgpx.Text, labelgpx.Font).Height;


            profileLabels[0].Top = labelgpx.Bottom + 5;

         }

         int width = profilePictureBoxes[0].Parent.Width - 2 * profilePictureBoxes[0].Left - SystemInformation.VerticalScrollBarWidth;
         for (int i = 0; i < profilePictureBoxes.Count; i++) {
            if (i > 0)
               profileLabels[i].Top = profilePictureBoxes[i - 1].Bottom + 5;
            profileLabels[i].Height = TextRenderer.MeasureText(profileLabels[i].Text, profileLabels[i].Font).Height;
            profilePictureBoxes[i].Top = profileLabels[i].Bottom + 2;
            profilePictureBoxes[i].Size = new Size(width, (3 * width) / 4);
         }
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

      private void contextMenuStripPoints_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (!Track.IsEditable ||
             TrackIsReadOnly ||
             dataGridViewPoints.SelectedRows.Count == 0)
            e.Cancel = true;
      }

      private void ToolStripMenuItem_PointsRemoving_Click(object sender, EventArgs e) {
         for (int i = dataGridViewPoints.Rows.Count - 1; i >= 0; i--)
            if (dataGridViewPoints.Rows[i].Selected)
               Track.RemovePoint(i);
         pointschanged = true;
         Track.Refresh();     // falls sichtbar, Anzeige akt.
         FillDataGridView();

         showTrackLength();

         OnSelectedPointsRemoving(new SelectedPointsEventArgs(Track, GetSelectedPoints()));
      }

      private void dataGridViewPoints_SelectionChanged(object sender, EventArgs e) {
         OnSelectedPoints(new SelectedPointsEventArgs(Track, GetSelectedPoints()));
         Pb_SizeChanged(profilePictureBoxes[0], null);
      }

      private void dataGridViewPoints_KeyDown(object sender, KeyEventArgs e) {
         if (Track.IsEditable && !TrackIsReadOnly)
            if (e.KeyData == Keys.Delete &&
                dataGridViewPoints.SelectedRows.Count > 0)
               ToolStripMenuItem_PointsRemoving_Click(null, null);
      }

      protected virtual void OnSelectedPoints(SelectedPointsEventArgs e) {
         SelectedPoints?.Invoke(this, e);
      }

      protected virtual void OnSelectedPointsRemoving(SelectedPointsEventArgs e) {
         SelectedPointsRemoving?.Invoke(this, e);
      }

      void showTrackLength() {
         double len = Track.Length();
         label_TrackLength.Text = string.Format("{0} Punkte, {1:F1} km / {2:F0} m",
                                                Track.GpxSegment.Points.Count,
                                                len / 1000,
                                                len);
      }

      void FillDataGridView() {
         dataGridViewPoints.SuspendLayout();
         dataTablePoints.Rows.Clear();
         double length = 0;
         for (int i = 0; i < Track.GpxSegment.Points.Count; i++) {
            Gpx.GpxTrackPoint pt = Track.GpxSegment.Points[i];

            DataRow row = dataTablePoints.NewRow();
            row[dataColumnIdx] = i + 1;
            row[dataColumnLat] = string.Format("{0:F8}", pt.Lat);
            row[dataColumnLon] = string.Format("{0:F8}", pt.Lon);
            if (pt.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE)
               row[dataColumnElevation] = string.Format("{0:F0}", pt.Elevation);
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
      List<int> GetSelectedPoints() {
         List<int> ptlst = new List<int>();
         for (int i = 0; i < dataGridViewPoints.Rows.Count; i++)
            if (dataGridViewPoints.Rows[i].Selected)
               ptlst.Add(i);
         return ptlst;
      }

      #region Höhenprofil erzeugen

      /// <summary>
      /// Bild des Höhenprofils erzeugen
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="track"></param>
      /// <param name="selectedidx"></param>
      /// <returns></returns>
      Image BuildImage4Track(int width, int height, Track track, IList<int> selectedidxlst) {

         Dictionary<int, int> selectedidx = new Dictionary<int, int>();
         foreach (int idx in selectedidxlst) {
            selectedidx.Add(idx, 0);
         }

         if (track != null &&
             track.GpxSegment.Points.Count > 1) {
            float length = (float)track.StatLength;
            float baseheight = (float)track.StatMinHeigth;
            float deltaheight = (float)(track.StatMaxHeigth - baseheight);

            if (deltaheight > 0) {
               Color colDiagrBack = Color.FromArgb(220, 220, 220);
               Color colLine = Color.Black;
               Color colSelected = Color.OrangeRed;
               Color colRaster = Color.FromArgb(180, 180, 180);
               Brush brushText = new SolidBrush(Color.Black);

               // Diagrammfläche
               RectangleF rectDiagr = new RectangleF(0.1F * width,      // Koordinatenursprung
                                                     0.05F * height,
                                                     0.85F * width,
                                                     0.9F * height);

               Bitmap bm = new Bitmap(width, height);
               Graphics canvas = Graphics.FromImage(bm);
               canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
               canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
               canvas.Clear(Color.White);

               //canvas.FillRectangle(new SolidBrush(colDiagrBack), rectDiagr);
               canvas.FillRectangle(new LinearGradientBrush(new PointF(rectDiagr.Left, rectDiagr.Top),
                                                            new PointF(rectDiagr.Left, rectDiagr.Bottom),
                                                            Color.FromArgb(235, 235, 255),
                                                            Color.FromArgb(20, 20, 255)),
                                    rectDiagr);



               // Raster zeichnen
               Font font = new Font(this.Font.FontFamily,
                                    0.7F * (height - rectDiagr.Bottom),
                                    FontStyle.Regular,
                                    GraphicsUnit.Pixel);
               StringFormat stringFormatx = new StringFormat {
                  Alignment = StringAlignment.Center,
                  LineAlignment = StringAlignment.Near
               };
               StringFormat stringFormaty = new StringFormat {
                  Alignment = StringAlignment.Far,
                  LineAlignment = StringAlignment.Center
               };


               Pen pen = new Pen(colRaster);
               float rasterdelta;
               float rasterpictdelta;

               rasterdelta = CalculateRasterVertical(track.StatMinHeigth, track.StatMaxHeigth, out float rasterstart);     // Rasterweite in m und niedrigste Höhe für eine Rasterlinie ...
               rasterpictdelta = rasterdelta / deltaheight * rectDiagr.Height;                                             // ... umgerechnet in Bildkoordinaten für die Diagrammfläche
               float rasterpictstart = (rasterstart - baseheight) / deltaheight * rectDiagr.Height;                        // ... umgerechnet in Bildkoordinaten für die Diagrammfläche
               canvas.DrawLine(pen, rectDiagr.Left, rectDiagr.Bottom, rectDiagr.Right, rectDiagr.Bottom);                  // x-Achse
               for (int i = 0; ; i++) {
                  float y = rectDiagr.Bottom - rasterpictstart - i * rasterpictdelta;
                  if (y < rectDiagr.Top)
                     break;
                  canvas.DrawLine(pen, rectDiagr.Left, y, rectDiagr.Right, y);                                             // waagerechte Rasterlinien
                                                                                                                           // Achsen-Beschriftung
                  canvas.DrawString(string.Format("{0:F0}m", rasterstart + i * rasterdelta),    // {0:F1}km
                                    font,
                                    brushText,
                                    rectDiagr.Left,
                                    y,
                                    stringFormaty);
               }

               rasterdelta = CalculateRasterHorizontal(track.StatLength);                                                  // Rasterweite in m ...
               rasterpictdelta = rasterdelta / length * rectDiagr.Width;                                                   // ... umgerechnet in Bildkoordinaten für die Diagrammfläche
               for (int i = 0; ; i++) {
                  float x = rectDiagr.Left + i * rasterpictdelta;
                  if (x > rectDiagr.Right)
                     break;
                  canvas.DrawLine(pen, x, rectDiagr.Bottom, x, rectDiagr.Top);                                             // y-Achse (i=-1) und senkrechte Rasterlinien
                  if (i > 0)    // Achsen-Beschriftung
                     canvas.DrawString(string.Format("{0}km", i * rasterdelta / 1000),    // {0:F1}km
                                       font,
                                       brushText,
                                       x,
                                       rectDiagr.Bottom + 0.1F * (height - rectDiagr.Bottom),
                                       stringFormatx);
               }

               pen.Dispose();

               // Daten einsammeln
               bool[] validPt = new bool[track.GpxSegment.Points.Count];
               bool[] selectedPt = new bool[track.GpxSegment.Points.Count];
               List<PointF> ptContour = new List<PointF>();
               float startlength = 0;
               for (int i = 0; i < track.GpxSegment.Points.Count; i++) {
                  float endlength = startlength + (i > 0 ? (float)track.Length(i - 1, i) : 0);

                  float x = rectDiagr.Left + endlength / length * rectDiagr.Width;
                  double h = track.GetGpxPoint(i).Elevation;       // double wegen PoorGpx.BaseElement.NOTVALID_DOUBLE
                  if (h != Gpx.BaseElement.NOTVALID_DOUBLE) {
                     validPt[i] = true;
                     ptContour.Add(new PointF(x, rectDiagr.Bottom - ((float)h - baseheight) / deltaheight * rectDiagr.Height));
                  } else
                     ptContour.Add(new PointF(x, rectDiagr.Bottom));

                  startlength = endlength;

                  if (selectedidx != null &&
                      selectedidx.ContainsKey(i))
                     selectedPt[i] = true;
               }

               // Contourfläche zeichnen
               if (ptContour.Count > 1) {
                  ptContour.Add(new PointF(ptContour[ptContour.Count - 1].X, rectDiagr.Bottom));
                  ptContour.Add(new PointF(ptContour[0].X, rectDiagr.Bottom));

                  LinearGradientBrush brushHeight = new LinearGradientBrush(new PointF(rectDiagr.Left, rectDiagr.Top),
                                                                            new PointF(rectDiagr.Left, rectDiagr.Bottom),
                                                                            Color.FromArgb(255, 20, 20),
                                                                            Color.FromArgb(20, 255, 20));
                  canvas.FillPolygon(brushHeight, ptContour.ToArray());
                  brushHeight.Dispose();

                  ptContour.RemoveRange(ptContour.Count - 2, 2);
               }

               // Contour zeichnen
               Pen penstd = new Pen(colLine);
               Pen penselected = new Pen(colSelected);
               Brush brushSeleted = new SolidBrush(colSelected);
               for (int i = 0; i < ptContour.Count; i++) {
                  if (i > 0) {
                     pen = selectedidx != null &&
                           selectedidx.ContainsKey(i - 1) &&
                           selectedidx.ContainsKey(i) ?
                              penselected :
                              penstd;

                     if (validPt[i - 1] &&
                         validPt[i])
                        canvas.DrawLine(pen, ptContour[i - 1], ptContour[i]);
                  }

                  if (selectedPt[i])
                     canvas.FillEllipse(brushSeleted, ptContour[i].X - 2.5F, ptContour[i].Y - 2.5F, 5, 5);
               }

               penstd.Dispose();
               penselected.Dispose();
               brushSeleted.Dispose();

               return bm;
            }
         }
         return null;
      }

      /// <summary>
      /// liefert Rasterweite für den Maximalwert
      /// </summary>
      /// <param name="maxval">max. Wert</param>
      /// <returns></returns>
      float CalculateRasterHorizontal(double maxval) {
         //       ... <=100   10 (1..10)
         // <100  ... <=200   25 (4..8)
         // <200  ... <=500   50 (4..10)
         // <500  ... <=1000 100 (5..10)
         // <1000 ... <=2000 250 (4..8)
         // ...
         float rd = 0;
         if (maxval <= 100) rd = 10;
         else if (maxval <= 200) rd = 25;
         else if (maxval <= 500) rd = 50;
         else if (maxval <= 1000) rd = 100;
         else if (maxval <= 2000) rd = 250;
         else if (maxval <= 5000) rd = 500;
         else if (maxval <= 10000) rd = 1000;
         else if (maxval <= 20000) rd = 2500;
         else if (maxval <= 50000) rd = 5000;
         else if (maxval <= 100000) rd = 10000;
         else if (maxval <= 200000) rd = 25000;
         else if (maxval <= 500000) rd = 50000;
         else if (maxval <= 1000000) rd = 100000;
         else if (maxval <= 2000000) rd = 250000;
         else if (maxval <= 5000000) rd = 500000;
         else if (maxval <= 10000000) rd = 1000000;
         else if (maxval <= 20000000) rd = 2500000;
         else if (maxval <= 50000000) rd = 5000000;   // bis 50.000 km

         return rd;
      }

      /// <summary>
      /// liefert Rasterweite
      /// </summary>
      /// <param name="minval">max. Wert</param>
      /// <param name="maxval"></param>
      /// <param name="rasterminval">kleinsten Wert für eine Rasterlinie</param>
      /// <returns></returns>
      float CalculateRasterVertical(double minval, double maxval, out float rasterminval) {
         float d = CalculateRasterHorizontal(maxval - minval);
         rasterminval = (float)Math.Ceiling(minval / d) * d;
         return d;
      }

      #endregion


   }
}
