using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Unclassified.UI {
   public partial class SpecColorSelectorDialog : Form {

      int bottom_fullview = 0;
      int bottom_shortview = 0;

      [Browsable(true), Category("dialogspezifisch"), DefaultValue(typeof(Color), "Blue"), Description("ausgewählte Farbe")]
      public Color SelectedColor {
         get => specColorSelector1.SelectedColor;
         set => specColorSelector1.SelectedColor = value;
      }

      [Browsable(true), Category("dialogspezifisch"), Description("Anzahl der Arrayfarben")]
      public int ArrayColorsCount {
         get => specColorSelector1.ArrayColorsCount;
      }


      public SpecColorSelectorDialog() {
         InitializeComponent();
         specColorSelector1.CloseWithOKRequested += SpecColorSelector1_CloseWithOKRequested;
         specColorSelector1.FullViewChanged += SpecColorSelector1_FullViewChanged;
      }

      protected override void OnLoad(EventArgs e) {
         base.OnLoad(e);
         if (specColorSelector1.FullView)
            bottom_fullview = specColorSelector1.ActualControlBottom;
         else
            bottom_shortview = specColorSelector1.ActualControlBottom;
      }

      private void SpecColorSelector1_FullViewChanged(object sender, EventArgs e) {
         int delta = 0;
         if (specColorSelector1.FullView) {
            bottom_fullview = specColorSelector1.ActualControlBottom;
            delta = bottom_fullview - bottom_shortview;
         } else {
            bottom_shortview = specColorSelector1.ActualControlBottom;
            delta = bottom_shortview - bottom_fullview;
         }

         button_OK.Location = new Point(button_OK.Location.X, button_OK.Location.Y + delta);
         button_Cancel.Location = new Point(button_Cancel.Location.X, button_Cancel.Location.Y + delta);

         Bounds = new Rectangle(Bounds.Location, new Size(Bounds.Width, Bounds.Height + delta));
      }

      private void SpecColorSelector1_CloseWithOKRequested(object sender, EventArgs e) {
         DialogResult = DialogResult.OK;
         Close();
      }

      public Color GetArrayColor(int idx) {
         return specColorSelector1.GetArrayColor(idx);
      }

      public void SetArrayColor(int idx, Color col) {
         specColorSelector1.SetArrayColor(idx, col);
      }

      public bool IsArrayColorEnabled(int idx) {
         return specColorSelector1.IsArrayColorEnabled(idx);
      }

      public void EnableArrayColor(int idx, bool enable) {
         specColorSelector1.EnableArrayColor(idx, enable);
      }

   }
}
