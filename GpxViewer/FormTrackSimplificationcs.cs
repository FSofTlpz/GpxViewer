using FSofTUtils.Geography;
using FSofTUtils.Geography.PoorGpx;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GpxViewer {
   public partial class FormTrackSimplificationcs : Form {

      public static List<string> SimplificationDataList;


      class SimplificationData {

         public string Name;

         public GpxSimplification.HSimplification HSimplification = GpxSimplification.HSimplification.Nothing;
         public double HSimplificationWidth = 0;

         public GpxSimplification.VSimplification VSimplification = GpxSimplification.VSimplification.Nothing;
         public double VSimplificationWidth = 0;

         public double SpeedOutlier = 0;

         public double AscendOutlier = 0;
         public int AscendOutlierPointCount = 0;

         public bool RemoveTimestamps = false;
         public bool RemoveHeights = false;

         public bool MinimalHeightIsActiv = false;
         public double MinimalHeight = 0;

         public bool MaximalHeightIsActiv = false;
         public double MaximalHeight = 0;

         public bool HSimplificationIsActiv => HSimplification != GpxSimplification.HSimplification.Nothing && 0 < HSimplificationWidth;
         public bool VSimplificationIsActiv => VSimplification != GpxSimplification.VSimplification.Nothing && 0 < VSimplificationWidth;
         public bool SpeedOutlierIsActiv => 0 < SpeedOutlier;
         public bool AscendOutlierIsActiv => 0 < AscendOutlier && 1 < AscendOutlierPointCount;

         public bool PointRangeIsActiv = false;
         public double PointRangeHeight = 0;
         public int PointRangeStart = 0;
         public int PointRangeCount = 0;

         public bool GapFill = false;


         public SimplificationData() { }

         public SimplificationData(string name) {
            Name = name;
         }

         public string AsString() => AsString(this);

         public static string AsString(SimplificationData sd) {
            StringBuilder sb = new StringBuilder(sd.Name);
            sb.Append("\t");
            sb.Append((int)sd.HSimplification);
            sb.Append("\t");
            sb.Append(sd.HSimplificationWidth);
            sb.Append("\t");
            sb.Append((int)sd.VSimplification);
            sb.Append("\t");
            sb.Append(sd.VSimplificationWidth);
            sb.Append("\t");
            sb.Append(sd.SpeedOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlierPointCount);
            sb.Append("\t");
            sb.Append(sd.RemoveTimestamps);
            sb.Append("\t");
            sb.Append(sd.RemoveHeights);
            sb.Append("\t");
            sb.Append(sd.MinimalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MinimalHeight);
            sb.Append("\t");
            sb.Append(sd.MaximalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MaximalHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeIsActiv);
            sb.Append("\t");
            sb.Append(sd.PointRangeHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeStart);
            sb.Append("\t");
            sb.Append(sd.PointRangeCount);
            sb.Append("\t");
            sb.Append(sd.GapFill);

            return sb.ToString();
         }

         public static SimplificationData FromString(string txt) {
            SimplificationData sd = new SimplificationData();
            string[] tmp = txt.Split('\t');
            int i = 0;
            if (i < tmp.Length)
               sd.Name = tmp[i++];
            if (i < tmp.Length)
               sd.HSimplification = (GpxSimplification.HSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.HSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplification = (GpxSimplification.VSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.SpeedOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlierPointCount = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveTimestamps = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveHeights = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeStart = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeCount = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.GapFill = Convert.ToBoolean(tmp[i++]);

            return sd;
         }

      }

      public Track SrcTrack;

      public Track DestTrack { get; private set; }


      public FormTrackSimplificationcs() {
         InitializeComponent();
         SimplificationDataList = new List<string>();
         comboBoxDatasets.IntegralHeight = true;
      }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);
         Text = "Vereinfachung: " + SrcTrack.VisualName;

         DestTrack = null;

         checkBoxMinimalHeightIsActiv_CheckedChanged(checkBoxMinimalHeightIsActiv, null);
         checkBoxMaximalHeightIsActiv_CheckedChanged(checkBoxMaximalHeightIsActiv, null);
         checkBoxPointRangeIsActiv_CheckedChanged(checkBoxPointRangeIsActiv, null);
         checkBoxSpeedOutlier_CheckedChanged(checkBoxSpeedOutlier, null);
         checkBoxAscendOutlier_CheckedChanged(checkBoxAscendOutlier, null);
         radioButtonHSimplicationNothing_CheckedChanged(radioButtonHSimplicationNothing, null);
         radioButtonVSimplicationNothing_CheckedChanged(radioButtonVSimplicationNothing, null);

         for (int i = 0; i < SimplificationDataList.Count; i++) {
            SimplificationData sd = SimplificationData.FromString(SimplificationDataList[i]);
            comboBoxDatasets.Items.Add(sd.Name);
         }

         if (comboBoxDatasets.Items.Count > 0)
            comboBoxDatasets.SelectedIndex = 0;
      }

      /// <summary>
      /// liefert ein <see cref="SimplificationData"/>-Objekt entsprechend der akt. Daten im Form
      /// </summary>
      /// <returns></returns>
      SimplificationData getActualData() {
         return new SimplificationData() {
            HSimplification = radioButtonHSimplicationDP.Checked ? GpxSimplification.HSimplification.Douglas_Peucker :
                              radioButtonHSimplicationRW.Checked ? GpxSimplification.HSimplification.Reumann_Witkam :
                                                                   GpxSimplification.HSimplification.Nothing,
            HSimplificationWidth = (double)numericUpDownHSimplicationWidth.Value,
            VSimplification = radioButtonVSimplicationSI.Checked ? GpxSimplification.VSimplification.SlidingIntegral :
                              radioButtonVSimplicationSM.Checked ? GpxSimplification.VSimplification.SlidingMean :
                                                                   GpxSimplification.VSimplification.Nothing,
            VSimplificationWidth = (double)numericUpDownVSimplicationWidth.Value,
            SpeedOutlier = checkBoxSpeedOutlier.Checked ? (double)numericUpDownSpeedOutlier.Value : -1,
            AscendOutlier = checkBoxAscendOutlier.Checked ? (double)numericUpDownAscendOutlier.Value : -1,
            AscendOutlierPointCount = checkBoxAscendOutlier.Checked ? (int)numericUpDownAscendOutlierPoints.Value : -1,
            RemoveTimestamps = checkBoxRemoveTimestamps.Checked,
            RemoveHeights = checkBoxRemoveHeights.Checked,
            MinimalHeightIsActiv = checkBoxMinimalHeightIsActiv.Checked,
            MinimalHeight = (double)numericUpDownMinimalHeight.Value,
            MaximalHeightIsActiv = checkBoxMaximalHeightIsActiv.Checked,
            MaximalHeight = (double)numericUpDownMaximalHeight.Value,
            PointRangeIsActiv = checkBoxPointRangeIsActiv.Checked,
            PointRangeHeight = (double)numericUpDownPointRangeHeight.Value,
            PointRangeStart = (int)numericUpDownPointRangeStart.Value,
            PointRangeCount = (int)numericUpDownPointRangeCount.Value,
            GapFill = checkBoxGapFill.Checked,
         };
      }

      /// <summary>
      /// setzt die Daten im Form entsprechend des <see cref="SimplificationData"/>
      /// </summary>
      /// <param name="sd"></param>
      void setActualData(SimplificationData sd) {
         switch (sd.HSimplification) {
            case GpxSimplification.HSimplification.Douglas_Peucker: radioButtonHSimplicationDP.Checked = true; break;
            case GpxSimplification.HSimplification.Reumann_Witkam: radioButtonHSimplicationRW.Checked = true; break;
            default: radioButtonHSimplicationNothing.Checked = true; break;
         }
         numericUpDownHSimplicationWidth.Value = Math.Max(numericUpDownHSimplicationWidth.Minimum, Convert.ToDecimal(sd.HSimplificationWidth));

         switch (sd.VSimplification) {
            case GpxSimplification.VSimplification.SlidingMean: radioButtonVSimplicationSM.Checked = true; break;
            case GpxSimplification.VSimplification.SlidingIntegral: radioButtonVSimplicationSI.Checked = true; break;
            default: radioButtonVSimplicationNothing.Checked = true; break;
         }
         numericUpDownVSimplicationWidth.Value = Math.Max(numericUpDownVSimplicationWidth.Minimum, Convert.ToDecimal(sd.VSimplificationWidth));

         checkBoxSpeedOutlier.Checked = sd.SpeedOutlierIsActiv;
         numericUpDownSpeedOutlier.Value = Math.Max(numericUpDownSpeedOutlier.Minimum, Convert.ToDecimal(sd.SpeedOutlier));

         checkBoxAscendOutlier.Checked = sd.AscendOutlierIsActiv;
         numericUpDownAscendOutlier.Value = Math.Max(numericUpDownAscendOutlier.Minimum, Convert.ToDecimal(sd.AscendOutlier));
         numericUpDownAscendOutlierPoints.Value = Math.Max(numericUpDownAscendOutlierPoints.Minimum, sd.AscendOutlierPointCount);

         checkBoxRemoveTimestamps.Checked = sd.RemoveTimestamps;

         checkBoxRemoveHeights.Checked = sd.RemoveHeights;

         checkBoxMinimalHeightIsActiv.Checked = sd.MinimalHeightIsActiv;
         numericUpDownMinimalHeight.Value = Math.Max(numericUpDownMinimalHeight.Minimum, Convert.ToDecimal(sd.MinimalHeight));

         checkBoxMaximalHeightIsActiv.Checked = sd.MaximalHeightIsActiv;
         numericUpDownMaximalHeight.Value = Math.Max(numericUpDownMaximalHeight.Minimum, Convert.ToDecimal(sd.MaximalHeight));

         checkBoxPointRangeIsActiv.Checked = sd.PointRangeIsActiv;
         numericUpDownPointRangeHeight.Value = Math.Max(numericUpDownPointRangeHeight.Minimum, Convert.ToDecimal(sd.PointRangeHeight));
         numericUpDownPointRangeStart.Value = Math.Max(numericUpDownPointRangeStart.Minimum, Convert.ToInt32(sd.PointRangeStart));
         numericUpDownPointRangeCount.Value = Math.Max(numericUpDownPointRangeCount.Minimum, Convert.ToInt32(sd.PointRangeCount));

         checkBoxGapFill.Checked = sd.GapFill;
      }

      private void checkBoxMinimalHeightIsActiv_CheckedChanged(object sender, EventArgs e) {
         numericUpDownMinimalHeight.Enabled = (sender as CheckBox).Checked;
      }

      private void checkBoxMaximalHeightIsActiv_CheckedChanged(object sender, EventArgs e) {
         numericUpDownMaximalHeight.Enabled = (sender as CheckBox).Checked;
      }

      private void checkBoxPointRangeIsActiv_CheckedChanged(object sender, EventArgs e) {
         numericUpDownPointRangeHeight.Enabled =
         numericUpDownPointRangeStart.Enabled =
         numericUpDownPointRangeCount.Enabled = (sender as CheckBox).Checked;
      }

      private void checkBoxSpeedOutlier_CheckedChanged(object sender, EventArgs e) {
         numericUpDownSpeedOutlier.Enabled = (sender as CheckBox).Checked;
      }

      private void checkBoxAscendOutlier_CheckedChanged(object sender, EventArgs e) {
         numericUpDownAscendOutlier.Enabled =
         numericUpDownAscendOutlierPoints.Enabled = (sender as CheckBox).Checked;
      }

      private void radioButtonHSimplicationNothing_CheckedChanged(object sender, EventArgs e) {
         numericUpDownHSimplicationWidth.Enabled = !(sender as RadioButton).Checked;
      }

      private void radioButtonVSimplicationNothing_CheckedChanged(object sender, EventArgs e) {
         numericUpDownVSimplicationWidth.Enabled = !(sender as RadioButton).Checked;
      }

      private void comboBoxDatasets_SelectedIndexChanged(object sender, EventArgs e) {
         if (0 <= comboBoxDatasets.SelectedIndex) {
            SimplificationData sd = SimplificationData.FromString(SimplificationDataList[comboBoxDatasets.SelectedIndex]);
            setActualData(sd);
         }
      }

      private void contextMenuStripDatasets_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         ToolStripMenuItemDatasetSave.Enabled = !string.IsNullOrEmpty(comboBoxDatasets.Text);
         ToolStripMenuItemDatasetUp.Enabled = 0 < comboBoxDatasets.SelectedIndex;
         ToolStripMenuItemDatasetDown.Enabled = 0 <= comboBoxDatasets.SelectedIndex && comboBoxDatasets.SelectedIndex < comboBoxDatasets.Items.Count - 1;
         ToolStripMenuItemDatasetDelete.Enabled = 0 <= comboBoxDatasets.SelectedIndex;
      }

      private void ToolStripMenuItemDatasetSave_Click(object sender, EventArgs e) {
         if (!string.IsNullOrEmpty(comboBoxDatasets.Text)) {
            SimplificationData sd = getActualData();
            sd.Name = comboBoxDatasets.Text;
            SimplificationDataList.Add(sd.AsString());
            comboBoxDatasets.Items.Add(sd.Name);
            comboBoxDatasets.SelectedIndex = comboBoxDatasets.Items.Count - 1;
         }
      }

      private void ToolStripMenuItemDatasetUp_Click(object sender, EventArgs e) {
         if (0 < comboBoxDatasets.SelectedIndex) {
            int idx = comboBoxDatasets.SelectedIndex;
            string txt = SimplificationDataList[idx];
            SimplificationDataList.RemoveAt(idx);
            SimplificationDataList.Insert(idx - 1, txt);
            txt = comboBoxDatasets.Items[idx].ToString();
            comboBoxDatasets.Items.RemoveAt(idx);
            comboBoxDatasets.Items.Insert(idx - 1, txt);
            comboBoxDatasets.SelectedIndex = idx - 1;
         }
      }

      private void ToolStripMenuItemDatasetDown_Click(object sender, EventArgs e) {
         if (0 <= comboBoxDatasets.SelectedIndex && comboBoxDatasets.SelectedIndex < comboBoxDatasets.Items.Count - 1) {
            int idx = comboBoxDatasets.SelectedIndex;
            string txt = SimplificationDataList[idx];
            SimplificationDataList.RemoveAt(idx);
            SimplificationDataList.Insert(idx + 1, txt);
            txt = comboBoxDatasets.Items[idx].ToString();
            comboBoxDatasets.Items.RemoveAt(idx);
            comboBoxDatasets.Items.Insert(idx + 1, txt);
            comboBoxDatasets.SelectedIndex = idx - 1;
         }
      }

      private void ToolStripMenuItemDatasetDelete_Click(object sender, EventArgs e) {
         if (0 <= comboBoxDatasets.SelectedIndex) {
            int idx = comboBoxDatasets.SelectedIndex;
            SimplificationDataList.RemoveAt(idx);
            comboBoxDatasets.Items.RemoveAt(idx);
            if (idx > 0)
               comboBoxDatasets.SelectedIndex = idx - 1;
            else if (comboBoxDatasets.Items.Count > 0)
               comboBoxDatasets.SelectedIndex = 0;
         }
      }

      private void button_Save_Click(object sender, EventArgs e) {
         int removedtimestamps = 0;
         int removedheights = 0;
         int setminheights = 0;
         int setmaxheights = 0;
         int setheights = 0;
         int speedoutliers = 0;
         int heightoutliers = 0;
         int gapfilledheights = 0;
         int gapfilledtimestamps = 0;
         int removedhsimpl = 0;
         int removedvsimpl = 0;

         SimplificationData sd = getActualData();

         List<GpxTrackPoint> gpxTrackPoints = new List<GpxTrackPoint>();
         for (int i = 0; i < SrcTrack.GpxSegment.Points.Count; i++)
            gpxTrackPoints.Add(new GpxTrackPoint(SrcTrack.GpxSegment.Points[i]));

         if (sd.RemoveTimestamps)
            removedtimestamps = GpxSimplification.RemoveTimestamp(gpxTrackPoints);

         if (sd.RemoveHeights)
            removedheights = GpxSimplification.RemoveHeight(gpxTrackPoints);

         if (sd.MinimalHeightIsActiv)
            GpxSimplification.SetHeight(gpxTrackPoints, out setminheights, out _, sd.MinimalHeight);

         if (sd.MaximalHeightIsActiv)
            GpxSimplification.SetHeight(gpxTrackPoints, out _, out setmaxheights, double.MinValue, sd.MaximalHeight);

         if (sd.PointRangeIsActiv)
            setheights = GpxSimplification.SetHeight(gpxTrackPoints, sd.PointRangeHeight, sd.PointRangeStart, sd.PointRangeCount);

         if (sd.SpeedOutlierIsActiv)
            speedoutliers = GpxSimplification.RemoveSpeedOutlier(gpxTrackPoints, sd.SpeedOutlier / 3.6);

         if (sd.AscendOutlierIsActiv)
            heightoutliers = GpxSimplification.RemoveHeigthOutlier(gpxTrackPoints, sd.AscendOutlier, sd.AscendOutlierPointCount);
         
         if (sd.GapFill)
            GpxSimplification.GapFill(gpxTrackPoints, out gapfilledheights, out gapfilledtimestamps);

         if (sd.HSimplificationIsActiv)
            removedhsimpl = GpxSimplification.HorizontalSimplification(gpxTrackPoints, sd.HSimplification, sd.HSimplificationWidth);

         if (sd.VSimplificationIsActiv)
            removedvsimpl = GpxSimplification.VerticalSimplification(gpxTrackPoints, sd.VSimplification, sd.VSimplificationWidth);

         if (removedtimestamps > 0 ||
             removedheights > 0 ||
             setminheights > 0 ||
             setmaxheights > 0 ||
             setheights > 0 ||
             speedoutliers > 0 ||
             heightoutliers > 0 ||
             gapfilledheights > 0 ||
             gapfilledtimestamps > 0 ||
             removedhsimpl > 0 ||
             removedvsimpl > 0) {
            DestTrack = new Track(gpxTrackPoints, SrcTrack.VisualName + " (vereinfacht)");
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Änderungen:");
            sb.AppendLine();
            if (removedtimestamps > 0)
               sb.AppendLine("* " + removedtimestamps + " Zeitstempel entfernt");
            if (removedheights > 0)
               sb.AppendLine("* " + removedheights + " Höhen entfernt");
            if (setminheights > 0)
               sb.AppendLine("* " + setminheights + " Höhen auf Minimum " + sd.MinimalHeight + "m gesetzt");
            if (setmaxheights > 0)
               sb.AppendLine("* " + setmaxheights + " Höhen auf Maximum " + sd.MaximalHeight + "m gesetzt");
            if (setheights > 0)
               sb.AppendLine("* " + setmaxheights + " Höhen auf " + sd.PointRangeHeight + "m gesetzt");
            if (speedoutliers > 0)
               sb.AppendLine("* " + speedoutliers + " Punkte wegen Überschreitung der Maximalgeschwindigkeit " + sd.PointRangeHeight + "km/h entfernt");
            if (heightoutliers > 0)
               sb.AppendLine("* " + heightoutliers + " Höhen wegen Überschreitung der max. Anstiegs " + sd.AscendOutlier + "% angepasst");
            if (gapfilledheights > 0)
               sb.AppendLine("* " + gapfilledheights + " Punkte ohne Höhe mit interpolierter Höhe gesetzt");
            if (gapfilledtimestamps > 0)
               sb.AppendLine("* " + gapfilledtimestamps + " Punkte ohne Zeitstempel mit interpolierten Zeitstempel gesetzt");
            if (removedhsimpl > 0)
               sb.AppendLine("* " + removedhsimpl + " Punkte bei horizontaler Vereinfachung entfernt");
            if (removedvsimpl > 0)
               sb.AppendLine("* " + removedvsimpl + " Punkte bei vertikaler Vereinfachung entfernt");

            MessageBox.Show(sb.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
         } else
            MessageBox.Show("Es gab keine Veränderungen am Track.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

         Close();
      }
   }
}
