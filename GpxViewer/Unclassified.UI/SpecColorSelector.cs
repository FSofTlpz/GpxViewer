using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Unclassified.Drawing;

namespace Unclassified.UI {
   public partial class SpecColorSelector : UserControl {

      #region Private fields

      private bool updatingFaders;
      private Color selectedColor;
      bool isonload = true;


      readonly StringFormat sfText = new StringFormat() {
         Alignment = StringAlignment.Center,
         LineAlignment = StringAlignment.Center,
      };

      Font font;


      #endregion Private fields

      #region Events

      /// <summary>
      /// die Farbauswahl wurde verändert
      /// </summary>
      public event EventHandler SelectedColorChanged;

      /// <summary>
      /// Doppelklick auf das Farbarray
      /// </summary>
      public event EventHandler CloseWithOKRequested;

      /// <summary>
      /// <see cref="FullView"/> wurde geändert
      /// </summary>
      public event EventHandler FullViewChanged;

      #endregion Events

      #region Localisation

      [Browsable(false)]
      public string T_ExtendedView {
         get { return ExtendedViewCheck.Text; }
         set { ExtendedViewCheck.Text = value; }
      }

      [Browsable(false)]
      public string T_Red {
         get { return RedLabel.Text; }
         set { RedLabel.Text = value; }
      }

      [Browsable(false)]
      public string T_Green {
         get { return GreenLabel.Text; }
         set { GreenLabel.Text = value; }
      }

      [Browsable(false)]
      public string T_Blue {
         get { return BlueLabel.Text; }
         set { BlueLabel.Text = value; }
      }

      [Browsable(false)]
      public string T_Hue {
         get { return HueLabel.Text; }
         set { HueLabel.Text = value; }
      }

      [Browsable(false)]
      public string T_Saturation {
         get { return SaturationLabel.Text; }
         set { SaturationLabel.Text = value; }
      }

      [Browsable(false)]
      public string T_Lightness {
         get { return LightnessLabel.Text; }
         set { LightnessLabel.Text = value; }
      }

      public void AutoLocalise() {
         switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) {
            case "de":
               T_ExtendedView = "Erweiterte Ansicht";
               T_Red = "Rot:";
               T_Green = "Grün:";
               T_Blue = "Blau:";
               T_Hue = "Farbton:";
               T_Saturation = "Sättigung:";
               T_Lightness = "Helligkeit:";
               break;
            case "en":
            default:
               T_ExtendedView = "Extended view";
               T_Red = "Red:";
               T_Green = "Green:";
               T_Blue = "Blue:";
               T_Hue = "Hue:";
               T_Saturation = "Saturation:";
               T_Lightness = "Lightness:";
               break;
            case "es":
               T_ExtendedView = "Vista extendida";
               T_Red = "Rojo:";
               T_Green = "Verde:";
               T_Blue = "Azul:";
               T_Hue = "Matiz:";
               T_Saturation = "Saturación:";
               T_Lightness = "Luminosidad:";
               break;
            case "fr":
               T_ExtendedView = "Vue étendue";
               T_Red = "Rouge:";
               T_Green = "Vert:";
               T_Blue = "Bleu:";
               T_Hue = "Teinte:";
               T_Saturation = "Saturation:";
               T_Lightness = "Luminosité:";
               break;
            case "nl":
               T_ExtendedView = "Extended view";
               T_Red = "Rood:";
               T_Green = "Groen:";
               T_Blue = "Blauw:";
               T_Hue = "Tint:";
               T_Saturation = "Verzadiging:";
               T_Lightness = "Helderheid:";
               break;
         }
      }

      #endregion Localisation

      #region Public properties

      [Browsable(true), Category("SpecColorSelector"), DefaultValue(typeof(Color), "Blue"), Description("ausgewählte Farbe")]
      public Color SelectedColor {
         get => selectedColor;
         set {
            if (value != selectedColor) {
               selectedColor = value;

               if (!updatingFaders) {
                  faderRed.Ratio = value.R;
                  faderGreen.Ratio = value.G;
                  faderBlue.Ratio = value.B;
                  faderAlpha.Ratio = value.A;
               }

               UpdateColors();

               BuildDemoBitmap(pictureBox_Demo);

               SelectedColorChanged?.Invoke(this, EventArgs.Empty);
            }
         }
      }

      [Browsable(true), Category("SpecColorSelector"), DefaultValue(true), Description("vollständige Ansicht des Dialoges")]
      public bool FullView {
         get => ExtendedViewCheck.Checked;
         set {
            if (ExtendedViewCheck.Checked != value)
               ExtendedViewCheck.Checked = value;     // löst extendedViewCheck_CheckedChanged() aus
         }
      }

      [Browsable(true), Category("SpecColorSelector"), DefaultValue(true), Description("Unterkante des Controls")]
      public int ActualControlBottom {
         get {
            int[] rowHeights = tableLayoutPanel_Main.GetRowHeights();
            int h = 0;
            if (FullView) {
               for (int i = 0; i < rowHeights.Length; i++)
                  h += rowHeights[i];
            } else {
               int idx = tableLayoutPanel_Main.GetRow(colorWheel1);
               for (int i = 0; i < idx; i++)
                  h += rowHeights[i];
            }
            return h;
         }
      }

      #endregion Public properties

      #region Constructors
      public SpecColorSelector() {
         InitializeComponent();

         AutoLocalise();

         RGBfader_RatioChanged(null, null);
      }

      #endregion Constructors

      #region Control event handlers
      protected override void OnLoad(EventArgs e) {
         isonload = true;
         base.OnLoad(e);
         extendedViewCheck_CheckedChanged(null, null);
         font = new Font(Font.FontFamily, 2 * Font.Size);
         pictureBox_Demo.Image = new Bitmap(pictureBox_Demo.ClientSize.Width, pictureBox_Demo.ClientSize.Height);
         BuildDemoBitmap(pictureBox_Demo);
         isonload = false;
      }

      #endregion Control event handlers

      #region Sub-control event handlers
      private void RGBfader_RatioChanged(object sender, EventArgs e) {
         if (!updatingFaders) {
            updatingFaders = true;

            Color rgb = Color.FromArgb(faderAlpha.Ratio, faderRed.Ratio, faderGreen.Ratio, faderBlue.Ratio);
            HslColor hsl = new HslColor(rgb);

            faderHue.Ratio = hsl.H;
            faderSaturation.Ratio = hsl.S;
            faderLightness.Ratio = hsl.L;

            SelectedColor = rgb;

            updatingFaders = false;
         }
      }

      private void HSLfader_RatioChanged(object sender, EventArgs e) {
         if (!updatingFaders) {
            updatingFaders = true;

            Color rgb = colorFromHSLFader();

            faderRed.Ratio = rgb.R;
            faderGreen.Ratio = rgb.G;
            faderBlue.Ratio = rgb.B;

            SelectedColor = rgb;

            updatingFaders = false;
         }
      }

      private void faderAlpha_RatioChanged(object sender, EventArgs e) {
         if (!updatingFaders) {
            updatingFaders = true;
            SelectedColor = Color.FromArgb(faderAlpha.Ratio, faderRed.Ratio, faderGreen.Ratio, faderBlue.Ratio);
            updatingFaders = false;
         }
      }

      private void colorWheel1_HueChanged(object sender, EventArgs e) {
         updatingFaders = true;

         faderHue.Ratio = colorWheel1.Hue;

         Color rgb = colorFromHSLFader();

         faderRed.Ratio = rgb.R;
         faderGreen.Ratio = rgb.G;
         faderBlue.Ratio = rgb.B;

         SelectedColor = rgb;

         updatingFaders = false;
      }

      private void colorWheel1_SLChanged(object sender, EventArgs e) {
         updatingFaders = true;

         faderSaturation.Ratio = colorWheel1.Saturation;
         faderLightness.Ratio = colorWheel1.Lightness;

         Color rgb = colorFromHSLFader();

         faderRed.Ratio = rgb.R;
         faderGreen.Ratio = rgb.G;
         faderBlue.Ratio = rgb.B;

         SelectedColor = rgb;

         updatingFaders = false;
      }

      Color colorFromHSLFader() {
         return new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio, faderAlpha.Ratio).ToRgb();
      }

      private void picturePalette_Click(object sender, EventArgs e) {
         Control c = sender as Control;
         Color color = c.BackColor;
         faderRed.Ratio = color.R;
         faderGreen.Ratio = color.G;
         faderBlue.Ratio = color.B;

         if (!ExtendedViewCheck.Checked)
            CloseWithOKRequested?.Invoke(this, EventArgs.Empty);
      }

      private void extendedViewCheck_CheckedChanged(object sender, EventArgs e) {
         tableLayoutPanel_Faders.SuspendLayout();
         tableLayoutPanel_Faders.Visible = colorWheel1.Visible = FullView = ExtendedViewCheck.Checked;
         tableLayoutPanel_Faders.ResumeLayout();
         if (!isonload)
            FullViewChanged?.Invoke(this, new EventArgs());
      }

      private void colorfield_Click(object sender, EventArgs e) {
         SelectedColor = (sender as ColorField).Color;
      }

      private void colorfield_DoubleClick(object sender, EventArgs e) {
         SelectedColor = (sender as ColorField).Color;
         CloseWithOKRequested?.Invoke(this, EventArgs.Empty);
      }

      #endregion Sub-control event handlers

      #region Management methods
      private void UpdateColors() {
         Color c = Color.FromArgb(faderAlpha.Ratio, faderRed.Ratio, faderGreen.Ratio, faderBlue.Ratio);

         faderRed.Color1 = Color.FromArgb(0, c.G, c.B);
         faderRed.Color2 = Color.FromArgb(255, c.G, c.B);

         faderGreen.Color1 = Color.FromArgb(c.R, 0, c.B);
         faderGreen.Color2 = Color.FromArgb(c.R, 255, c.B);

         faderBlue.Color1 = Color.FromArgb(c.R, c.G, 0);
         faderBlue.Color2 = Color.FromArgb(c.R, c.G, 255);

         HslColor hsl = new HslColor(faderHue.Ratio, faderSaturation.Ratio, faderLightness.Ratio, faderAlpha.Ratio);

         faderSaturation.Color1 = new HslColor(hsl.H, 0, hsl.L).ToRgb();
         faderSaturation.Color2 = new HslColor(hsl.H, 255, hsl.L).ToRgb();

         faderLightness.Color1 = new HslColor(hsl.H, hsl.S, 0).ToRgb();
         faderLightness.ColorMid = new HslColor(hsl.H, hsl.S, 128).ToRgb();
         faderLightness.Color2 = new HslColor(hsl.H, hsl.S, 255).ToRgb();

         colorWheel1.Hue = hsl.H;
         colorWheel1.Saturation = hsl.S;
         colorWheel1.Lightness = hsl.L;
      }

      void BuildDemoBitmap(PictureBox pb) {
         Bitmap bm = pb.Image as Bitmap;
         if (bm != null) {
            Graphics canvas = Graphics.FromImage(bm);
            canvas.Clear(Color.White);
            canvas.DrawString("Demo", font, new SolidBrush(Color.Black), bm.Width / 2, bm.Height / 2, sfText);
            canvas.FillRectangle(new SolidBrush(SelectedColor), 0, 0, bm.Width, bm.Height);
            canvas.Flush();
            pb.Image = bm;
         }
      }

      #endregion Management methods

      #region public functions

      public Color GetArrayColor(int idx) {
         ColorField cf = getColorField(idx);
         return cf != null ?
                        cf.Color :
                        Color.Empty;
      }

      public void SetArrayColor(int idx, Color col) {
         ColorField cf = getColorField(idx);
         if (cf != null)
            cf.Color = col;
      }

      public bool IsArrayColorEnabled(int idx) {
         ColorField cf = getColorField(idx);
         return cf != null ?
                     cf.Enabled :
                     false;
      }

      public void EnableArrayColor(int idx, bool enable) {
         ColorField cf = getColorField(idx);
         if (cf != null)
            cf.Enabled = enable;
      }


      public int ArrayColorsCount {
         get => flowLayoutPanel_ColorArray.Controls.Count;
      }

      ColorField getColorField(int idx) {
         return 0 <= idx && idx < ArrayColorsCount ?
                        flowLayoutPanel_ColorArray.Controls[idx] as ColorField :
                        null;
      }



      #endregion

   }
}
