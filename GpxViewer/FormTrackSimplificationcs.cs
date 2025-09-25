using FSofTUtils.Geography;
using FSofTUtils.Geography.PoorGpx;
using SpecialMapCtrl;
using System.Text;

namespace GpxViewer {
   public partial class FormTrackSimplificationcs : Form {

      public static List<string>? SimplificationDataList;

      public Track? SrcTrack;

      public Track? DestTrack { get; private set; }

      class SimplificationData {

         /// <summary>
         /// einige einfache Standarddefinitionen
         /// </summary>
         public static string[] StdDefs = [
               new SimplificationData() {
               Name = "Wandern",
               AscendOutlier = 40,
               AscendOutlierLength = 50,
               SpeedOutlier = 10,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 100,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
               new SimplificationData() {
               Name = "Wandern (Smartphon)",
               AscendOutlier = 40,
               AscendOutlierLength = 50,
               RemoveSpikes = true,
               SpeedOutlier = 10,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 400,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
            new SimplificationData() {
               Name = "Radfahren",
               AscendOutlier = 25,
               AscendOutlierLength = 50,
               SpeedOutlier = 60,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 100,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
            new SimplificationData() {
               Name = "Radfahren (Smartphon)",
               AscendOutlier = 25,
               AscendOutlierLength = 50,
               RemoveSpikes = true,
               SpeedOutlier = 60,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 400,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
         ];

         public string Name = string.Empty;

         public GpxSimplification.HSimplification HSimplification = GpxSimplification.HSimplification.Nothing;
         public double HSimplificationWidth = 0.2;

         public GpxSimplification.VSimplification VSimplification = GpxSimplification.VSimplification.Nothing;
         public double VSimplificationWidth = 100;
         public int VSimplificationWidthPt = 50;
         public double VSimplificationLowPassFreq = 0.00140;
         public int VSimplificationLowPassSamplerate = 10;
         public double VSimplificationLowPassDelay = 0.023;
         public int VSimplificationFractionalDigits = 1;
         public int VSimplificationRRWidth = 50;
         public int VSimplificationRROverlap = 5;
         public double VSimplificationRRLambda = 0.0001;

         /// <summary>
         /// max. Geschwindigkeit in km/h
         /// </summary>
         public double SpeedOutlier = 0;

         /// <summary>
         /// max. An-/Abstieg in Prozent
         /// </summary>
         public double AscendOutlier = 0;
         /// <summary>
         /// Testwegstrecke in m
         /// </summary>
         public int AscendOutlierLength = 0;

         public bool RemoveTimestamps = false;
         public bool RemoveHeights = false;

         public bool MinimalHeightIsActiv = false;
         public double MinimalHeight = 0;

         public bool MaximalHeightIsActiv = false;
         public double MaximalHeight = 0;

         public bool HSimplificationIsActiv => HSimplification != GpxSimplification.HSimplification.Nothing &&
                                                      0 < HSimplificationWidth;
         public bool VSimplificationIsActiv => (VSimplification == GpxSimplification.VSimplification.SlidingIntegral &&
                                                      0 < VSimplificationWidth) ||
                                               (VSimplification == GpxSimplification.VSimplification.SlidingMean &&
                                                      0 < VSimplificationWidth) ||
                                               (VSimplification == GpxSimplification.VSimplification.LowPassFilter &&
                                                      0 < VSimplificationLowPassFreq &&
                                                      0 < VSimplificationLowPassSamplerate &&
                                                      0 <= VSimplificationLowPassDelay) ||
                                               (VSimplification == GpxSimplification.VSimplification.RidgeRegression &&
                                                      0 < VSimplificationRRLambda &&
                                                      0 <= VSimplificationRROverlap &&
                                                      2 < VSimplificationRRWidth);
         public bool SpeedOutlierIsActiv => 0 < SpeedOutlier;
         public bool RemoveSpikes = false;
         public bool AscendOutlierIsActiv => 0 < AscendOutlier && 1 < AscendOutlierLength;

         public bool PointRangeIsActiv = false;
         public double PointRangeHeight = 0;
         public int PointRangeStart = 0;
         public int PointRangeCount = 0;

         public bool GapFill4Time = false;

         public bool GapFill4Height = false;


         public SimplificationData() { }

         public SimplificationData(string name) {
            Name = name;
         }

         public string AsString() => AsString(this);

         // Muss mit FromString() korrespondieren.

         public static string AsString(SimplificationData sd) {
            // Neue Parameter immer an das Ende!
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
            sb.Append(sd.AscendOutlierLength);
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
            sb.Append(sd.GapFill4Time);
            sb.Append("\t");
            sb.Append(sd.GapFill4Height);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassFreq);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassSamplerate);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassDelay);
            sb.Append("\t");
            sb.Append(sd.VSimplificationFractionalDigits);
            sb.Append("\t");
            sb.Append(sd.RemoveSpikes);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRRWidth);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRROverlap);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRRLambda);
            sb.Append("\t");
            sb.Append(sd.VSimplificationWidthPt);

            return sb.ToString();
         }

         // Muss mit AsString() korrespondieren.

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
               sd.AscendOutlierLength = Convert.ToInt32(tmp[i++]);
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
               sd.GapFill4Time = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.GapFill4Height = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassFreq = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassSamplerate = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassDelay = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationFractionalDigits = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveSpikes = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRRWidth = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRROverlap = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRRLambda = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationWidthPt = Convert.ToInt32(tmp[i++]);

            return sd;
         }

      }

      /// <summary>
      /// einige einfache Standarddefinitionen (nützlich wenn <see cref="SimplificationDataList"/> noch leer ist)
      /// </summary>
      public static string[] StdDefs => SimplificationData.StdDefs;


      public FormTrackSimplificationcs() {
         InitializeComponent();
         SimplificationDataList = new List<string>();
         comboBoxDatasets.IntegralHeight = true;
      }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);
         Text = "Vereinfachung: " + SrcTrack.VisualName;

         if (SimplificationDataList.Count == 0)    // wenn keine Def. vorhanden sind werden Standarddefs. verwendet
            SimplificationDataList.AddRange(SimplificationData.StdDefs);

         DestTrack = null;

         checkBoxMinimalHeightIsActiv_CheckedChanged(checkBoxMinimalHeightIsActiv, EventArgs.Empty);
         checkBoxMaximalHeightIsActiv_CheckedChanged(checkBoxMaximalHeightIsActiv, EventArgs.Empty);
         checkBoxPointRangeIsActiv_CheckedChanged(checkBoxPointRangeIsActiv, EventArgs.Empty);
         checkBoxSpeedOutlier_CheckedChanged(checkBoxSpeedOutlier, EventArgs.Empty);
         checkBoxAscendOutlier_CheckedChanged(checkBoxAscendOutlier, EventArgs.Empty);
         radioButtonHSimplicationNothing_CheckedChanged(radioButtonHSimplicationNothing, EventArgs.Empty);
         radioButtonVSimplication_CheckedChanged(radioButtonVSimplicationNothing, EventArgs.Empty);

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
      SimplificationData getActualData() => new SimplificationData() {
         HSimplification = radioButtonHSimplicationDP.Checked ? GpxSimplification.HSimplification.Douglas_Peucker :
                              radioButtonHSimplicationRW.Checked ? GpxSimplification.HSimplification.Reumann_Witkam :
                                                                   GpxSimplification.HSimplification.Nothing,
         HSimplificationWidth = (double)numericUpDownHSimplicationWidth.Value,
         VSimplification = radioButtonVSimplicationSI.Checked ? GpxSimplification.VSimplification.SlidingIntegral :
                           radioButtonVSimplicationSM.Checked ? GpxSimplification.VSimplification.SlidingMean :
                           radioButtonVSimplicationLP.Checked ? GpxSimplification.VSimplification.LowPassFilter :
                           radioButtonVSimplicationRR.Checked ? GpxSimplification.VSimplification.RidgeRegression :
                                                                GpxSimplification.VSimplification.Nothing,
         VSimplificationWidth = (double)numericUpDownVSimplicationWidth.Value,
         VSimplificationWidthPt = (int)numericUpDownVSimplicationWidthPt.Value,
         VSimplificationLowPassFreq = (double)numericUpDownVSimplicationFreq.Value,
         VSimplificationLowPassSamplerate = (int)numericUpDownVSimplicationSamplerate.Value,
         VSimplificationLowPassDelay = (double)numericUpDownVSimplicationDelay.Value,
         VSimplificationFractionalDigits = (int)numericUpDownVSimplicationDigits.Value,
         VSimplificationRRWidth = (int)numericUpDownVSimplicationRRWidth.Value,
         VSimplificationRROverlap = (int)numericUpDownVSimplicationRROverlap.Value,
         VSimplificationRRLambda = (double)numericUpDownVSimplicationRRLambda.Value,
         RemoveSpikes = checkBoxSpikes.Checked,
         SpeedOutlier = checkBoxSpeedOutlier.Checked ? (double)numericUpDownSpeedOutlier.Value : -1,
         AscendOutlier = checkBoxAscendOutlier.Checked ? (double)numericUpDownAscendOutlier.Value : -1,
         AscendOutlierLength = checkBoxAscendOutlier.Checked ? (int)numericUpDownAscendOutlierLength.Value : -1,
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
         GapFill4Time = checkBoxGapFill4Time.Checked,
         GapFill4Height = checkBoxGapFill4Height.Checked,
      };

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
         setNumericUpDown(numericUpDownHSimplicationWidth, sd.HSimplificationWidth);

         switch (sd.VSimplification) {
            case GpxSimplification.VSimplification.SlidingMean: radioButtonVSimplicationSM.Checked = true; break;
            case GpxSimplification.VSimplification.SlidingIntegral: radioButtonVSimplicationSI.Checked = true; break;
            case GpxSimplification.VSimplification.LowPassFilter: radioButtonVSimplicationLP.Checked = true; break;
            case GpxSimplification.VSimplification.RidgeRegression: radioButtonVSimplicationRR.Checked = true; break;
            default: radioButtonVSimplicationNothing.Checked = true; break;
         }
         setNumericUpDown(numericUpDownVSimplicationWidth, sd.VSimplificationWidth);
         setNumericUpDown(numericUpDownVSimplicationWidthPt, sd.VSimplificationWidthPt);
         setNumericUpDown(numericUpDownVSimplicationFreq, sd.VSimplificationLowPassFreq);
         setNumericUpDown(numericUpDownVSimplicationSamplerate, sd.VSimplificationLowPassSamplerate);
         setNumericUpDown(numericUpDownVSimplicationDelay, sd.VSimplificationLowPassDelay);
         setNumericUpDown(numericUpDownVSimplicationDigits, sd.VSimplificationFractionalDigits);
         setNumericUpDown(numericUpDownVSimplicationRRWidth, sd.VSimplificationRRWidth);
         setNumericUpDown(numericUpDownVSimplicationRROverlap, sd.VSimplificationRROverlap);
         setNumericUpDown(numericUpDownVSimplicationRRLambda, sd.VSimplificationRRLambda);

         checkBoxSpeedOutlier.Checked = sd.SpeedOutlierIsActiv;
         setNumericUpDown(numericUpDownSpeedOutlier, sd.SpeedOutlier);

         checkBoxSpikes.Checked = sd.RemoveSpikes;

         checkBoxAscendOutlier.Checked = sd.AscendOutlierIsActiv;
         setNumericUpDown(numericUpDownAscendOutlier, sd.AscendOutlier);
         setNumericUpDown(numericUpDownAscendOutlierLength, sd.AscendOutlierLength);

         checkBoxRemoveTimestamps.Checked = sd.RemoveTimestamps;

         checkBoxRemoveHeights.Checked = sd.RemoveHeights;

         checkBoxMinimalHeightIsActiv.Checked = sd.MinimalHeightIsActiv;
         setNumericUpDown(numericUpDownMinimalHeight, sd.MinimalHeight);

         checkBoxMaximalHeightIsActiv.Checked = sd.MaximalHeightIsActiv;
         setNumericUpDown(numericUpDownMaximalHeight, sd.MaximalHeight);

         checkBoxPointRangeIsActiv.Checked = sd.PointRangeIsActiv;
         setNumericUpDown(numericUpDownPointRangeHeight, sd.PointRangeHeight);
         setNumericUpDown(numericUpDownPointRangeStart, sd.PointRangeStart);
         setNumericUpDown(numericUpDownPointRangeCount, sd.PointRangeCount);

         checkBoxGapFill4Time.Checked = sd.GapFill4Time;
      }

      void setNumericUpDown(NumericUpDown numericUpDown, double value) => numericUpDown.Value = Math.Min(Math.Max(numericUpDown.Minimum, Convert.ToDecimal(value)), numericUpDown.Maximum);

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
         numericUpDownAscendOutlierLength.Enabled = (sender as CheckBox).Checked;
      }

      private void radioButtonHSimplicationNothing_CheckedChanged(object sender, EventArgs e) {
         numericUpDownHSimplicationWidth.Enabled = !(sender as RadioButton).Checked;
      }

      private void radioButtonVSimplication_CheckedChanged(object sender, EventArgs e) {
         if ((sender as RadioButton).Checked) {
            numericUpDownVSimplicationWidth.Enabled = radioButtonVSimplicationSI.Checked;
            numericUpDownVSimplicationWidthPt.Enabled = radioButtonVSimplicationSM.Checked;
            numericUpDownVSimplicationFreq.Enabled =
            numericUpDownVSimplicationSamplerate.Enabled =
            numericUpDownVSimplicationDelay.Enabled = radioButtonVSimplicationLP.Checked;
            numericUpDownVSimplicationDigits.Enabled = !radioButtonVSimplicationNothing.Checked;
            numericUpDownVSimplicationRRWidth.Enabled =
            numericUpDownVSimplicationRROverlap.Enabled =
            numericUpDownVSimplicationRRLambda.Enabled = radioButtonVSimplicationRR.Checked;
         }
      }

      private void comboBoxDatasets_SelectedIndexChanged(object sender, EventArgs e) {
         if (0 <= comboBoxDatasets.SelectedIndex) {
            SimplificationData sd = SimplificationData.FromString(SimplificationDataList[comboBoxDatasets.SelectedIndex]);
            setActualData(sd);
         }
      }

      private void checkBoxRemoveTimestamps_CheckedChanged(object sender, EventArgs e) {
         groupBoxSpeedOutlier.Enabled =
         checkBoxGapFill4Time.Enabled = !checkBoxRemoveTimestamps.Checked;
      }

      private void checkBoxRemoveHeights_CheckedChanged(object sender, EventArgs e) {
         groupBoxAscendOutlier.Enabled =
         groupBoxMinHeight.Enabled =
         groupBoxMaxHeight.Enabled =
         groupBoxSetHeight.Enabled =
         groupBoxSimplificationHeight.Enabled =
         checkBoxGapFill4Height.Enabled = !checkBoxRemoveHeights.Checked;
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
            txt = comboBoxDatasets.Items[idx].ToString() ?? string.Empty;
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
            txt = comboBoxDatasets.Items[idx].ToString() ?? string.Empty;
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

      private async void button_Save_Click(object sender, EventArgs e) {
         ((Button)sender).Enabled = false;      // damit keine Mehrfachaufrufe im Task möglich sind

         int removedtimestamps = 0;
         int removedheights = 0;
         int setminheights = 0;
         int setmaxheights = 0;
         int setheights = 0;
         int spikes = 0;
         int speedoutliers = 0;
         int heightoutliers = 0;
         int gapfilledheights = 0;
         int gapfilledtimestamps = 0;
         int removedhsimpl = 0;
         int changedvsimpl = 0;

         SimplificationData sd = getActualData();
         List<GpxTrackPoint> gpxTrackPoints = new List<GpxTrackPoint>();

         Cursor orgcursor = Cursor;
         Cursor = Cursors.WaitCursor;

         await Task.Run(() => {
            for (int i = 0; i < SrcTrack.GpxSegment.Points.Count; i++)
               gpxTrackPoints.Add(new GpxTrackPoint(SrcTrack.GpxSegment.Points[i]));

            if (sd.RemoveTimestamps)
               removedtimestamps = GpxSimplification.RemoveTimestamp(gpxTrackPoints);

            if (sd.RemoveHeights)
               removedheights = GpxSimplification.RemoveHeight(gpxTrackPoints);

            if (!sd.RemoveHeights && sd.MinimalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out setminheights, out _, sd.MinimalHeight);

            if (!sd.RemoveHeights && sd.MaximalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out _, out setmaxheights, double.MinValue, sd.MaximalHeight);

            if (sd.PointRangeIsActiv)
               setheights = GpxSimplification.SetHeight(gpxTrackPoints, sd.PointRangeHeight, sd.PointRangeStart, sd.PointRangeCount);

            if (sd.RemoveSpikes)
               spikes = GpxSimplification.RemoveSpikes(gpxTrackPoints).Length;

            if (sd.SpeedOutlierIsActiv)
               speedoutliers = GpxSimplification.RemoveSpeedOutlier(gpxTrackPoints, sd.SpeedOutlier / 3.6).Length;

            if (!sd.RemoveHeights && sd.AscendOutlierIsActiv)
               heightoutliers = GpxSimplification.RemoveHeigthOutlier(gpxTrackPoints, sd.AscendOutlierLength, sd.AscendOutlier).Length;

            if (!sd.RemoveTimestamps && sd.GapFill4Time)
               gapfilledtimestamps = GpxSimplification.GapFill4Time(gpxTrackPoints).Length;

            if (!sd.RemoveHeights && sd.GapFill4Height)
               gapfilledheights = GpxSimplification.GapFill4Height(gpxTrackPoints).Length;

            if (!sd.RemoveHeights && sd.HSimplificationIsActiv)
               removedhsimpl = GpxSimplification.HorizontalSimplification(gpxTrackPoints, sd.HSimplification, sd.HSimplificationWidth).Length;

            if (sd.VSimplificationIsActiv) {
               double[] vparams = sd.VSimplification == GpxSimplification.VSimplification.SlidingMean ?
                                    [
                                    sd.VSimplificationWidthPt,
                                    sd.VSimplificationFractionalDigits,
                                    ] :
                                  sd.VSimplification == GpxSimplification.VSimplification.SlidingIntegral ?
                                    [
                                    sd.VSimplificationWidth,
                                    sd.VSimplificationFractionalDigits,
                                    ] :
                                  sd.VSimplification == GpxSimplification.VSimplification.LowPassFilter ?
                                    [
                                    sd.VSimplificationLowPassFreq,
                                    sd.VSimplificationLowPassSamplerate,
                                    sd.VSimplificationLowPassDelay,
                                    sd.VSimplificationFractionalDigits,
                                    ] :
                                    [
                                    sd.VSimplificationRRWidth,
                                    sd.VSimplificationRROverlap,
                                    sd.VSimplificationRRLambda,
                                    sd.VSimplificationFractionalDigits,
                                    ];

               changedvsimpl = GpxSimplification.VerticalSimplification(gpxTrackPoints, sd.VSimplification, vparams).Length;
            }
         });

         Cursor = orgcursor;

         if (removedtimestamps > 0 ||
             removedheights > 0 ||
             setminheights > 0 ||
             setmaxheights > 0 ||
             setheights > 0 ||
             spikes > 0 ||
             speedoutliers > 0 ||
             heightoutliers > 0 ||
             gapfilledheights > 0 ||
             gapfilledtimestamps > 0 ||
             removedhsimpl > 0 ||
             changedvsimpl > 0) {
            GpxTrack t = new GpxTrack();
            t.InsertSegment(new GpxTrackSegment(gpxTrackPoints));
            GpxSimplification.SimplifyFormat(t);

            //DestTrack = new Track(gpxTrackPoints, SrcTrack.VisualName + " (vereinfacht)") {
            DestTrack = new Track(t.Segments[0].Points.ToArray(), SrcTrack.VisualName + " (vereinfacht)") {
               LineColor = SrcTrack.LineColor,
               LineWidth = SrcTrack.LineWidth,
            };

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
            if (spikes > 0)
               sb.AppendLine("* " + spikes + " Punkte als Spikes entfernt");
            if (speedoutliers > 0)
               sb.AppendLine("* " + speedoutliers + " Punkte wegen Überschreitung der Maximalgeschwindigkeit " + sd.SpeedOutlier + "km/h entfernt");
            if (heightoutliers > 0)
               sb.AppendLine("* " + heightoutliers + " Höhen wegen Überschreitung der max. Anstiegs " + sd.AscendOutlier + "% angepasst");
            if (gapfilledheights > 0)
               sb.AppendLine("* " + gapfilledheights + " Punkte ohne Höhe mit interpolierter Höhe gesetzt");
            if (gapfilledtimestamps > 0)
               sb.AppendLine("* " + gapfilledtimestamps + " Punkte ohne Zeitstempel mit interpolierten Zeitstempel gesetzt");
            if (removedhsimpl > 0)
               sb.AppendLine("* " + removedhsimpl + " Punkte bei horizontaler Vereinfachung entfernt");
            if (changedvsimpl > 0)
               sb.AppendLine("* " + changedvsimpl + " Punkte bei vertikaler Vereinfachung verändert");

            MessageBox.Show(sb.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
         } else
            MessageBox.Show("Es gab keine Veränderungen am Track.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

         DialogResult = DialogResult.OK;
         Close();
      }
   }
}
