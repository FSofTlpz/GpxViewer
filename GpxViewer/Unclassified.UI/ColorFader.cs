// Copyright (c) 2011, Yves Goergen, http://unclassified.software/source/colorfader
//
// Copying and distribution of this file, with or without modification, are permitted provided the
// copyright notice and this notice are preserved. This file is offered as-is, without any warranty.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Unclassified.Drawing;

namespace Unclassified.UI {
   public class ColorFader : Control {
      private Color color1;
      private Color color2;
      private Color colorMid;
      private bool hueMode;
      private byte ratio;
      private NumericUpDown numericControl;
      private int marginBottom = 3;
      private int paddingH = 4;

      public event EventHandler RatioChanged;

      [Category("Appearance")]
      [DefaultValue(typeof(Color), "Black")]
      public Color Color1 {
         get => color1;
         set { color1 = value; Invalidate(); }
      }

      [Category("Appearance")]
      [DefaultValue(typeof(Color), "White")]
      public Color Color2 {
         get => color2;
         set { color2 = value; Invalidate(); }
      }

      [Category("Appearance")]
      [DefaultValue(typeof(Color), "Empty")]
      public Color ColorMid {
         get => colorMid;
         set { colorMid = value; Invalidate(); }
      }

      [Category("Appearance")]
      [DefaultValue(false)]
      public bool HueMode {
         get => hueMode;
         set { hueMode = value; Invalidate(); }
      }

      [Category("Appearance")]
      [DefaultValue(0)]
      public byte Ratio {
         get => ratio;
         set {
            ratio = value;
            Invalidate();
            if (numericControl != null)
               numericControl.Value = ratio;
            OnRatioChanged();
         }
      }

      [Browsable(false)]
      public Color MixedColor {
         get {
            double d = (double)ratio / 255;
            int r = (int)(color1.R * (1 - d) + color2.R * d);
            int g = (int)(color1.G * (1 - d) + color2.G * d);
            int b = (int)(color1.B * (1 - d) + color2.B * d);
            return Color.FromArgb(r, g, b);
            // TODO: Regard ColorMid and HueMode, maybe by using the ColorMath class
         }
      }

      public NumericUpDown NumericControl {
         get { return numericControl; }
         set {
            if (numericControl != null)
               numericControl.ValueChanged -= new EventHandler(numericControl_ValueChanged);
            numericControl = value;
            if (numericControl != null)
               numericControl.ValueChanged += new EventHandler(numericControl_ValueChanged);
         }
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public new bool TabStop {
         get => base.TabStop;
         set => base.TabStop = false;
      }

      #region Disabled properties
      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override Image BackgroundImage {
         get => base.BackgroundImage;
         set => base.BackgroundImage = value;
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override ImageLayout BackgroundImageLayout {
         get => base.BackgroundImageLayout;
         set => base.BackgroundImageLayout = value;
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override Font Font {
         get => base.Font;
         set => base.Font = value;
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override Color ForeColor {
         get => base.ForeColor;
         set => base.ForeColor = value;
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override Point AutoScrollOffset {
         get => base.AutoScrollOffset;
         set => base.AutoScrollOffset = value;
      }

      [Browsable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override bool AutoSize {
         get => base.AutoSize;
         set => base.AutoSize = value;
      }
      #endregion Disabled properties

      public ColorFader() {
         SetStyle(ControlStyles.SupportsTransparentBackColor, true);

         DoubleBuffered = true;
         TabStop = false;
         color1 = Color.Black;
         color2 = Color.White;
         ratio = 0;
      }

      private void numericControl_ValueChanged(object sender, EventArgs e) {
         if (numericControl.Value == -1) {
            numericControl.Value = 255;
            return;
         }
         if (numericControl.Value == 256) {
            numericControl.Value = 0;
            return;
         }

         Ratio = (byte)numericControl.Value;
      }

      //protected override void OnPaintBackground(PaintEventArgs pevent)
      //{
      //    if (VisualStyleRenderer.IsSupported)
      //    {
      //        // The VisualStyleElement does not matter, we're only drawing the parent's background
      //        VisualStyleRenderer r = new VisualStyleRenderer(VisualStyleElement.Window.Dialog.Normal);
      //        r.DrawParentBackground(pevent.Graphics, ClientRectangle, this);
      //    }
      //    else
      //    {
      //        base.OnPaintBackground(pevent);
      //    }
      //}

      protected override void OnPaint(PaintEventArgs pe) {
         if (!hueMode) {
            if (color1.A < 255 || color2.A < 255) {
               Color backColor1 = Color.WhiteSmoke;
               Color backColor2 = Color.Silver;

               int size = 2 * 3;
               using (Bitmap bmp = new Bitmap(size, size)) {
                  for (int x = 0; x < bmp.Width; x++) {
                     for (int y = 0; y < bmp.Height; y++) {
                        if (x < bmp.Width / 2 && y < bmp.Height / 2 ||
                           x >= bmp.Width / 2 && y >= bmp.Height / 2) {
                           bmp.SetPixel(x, y, backColor1);
                        } else {
                           bmp.SetPixel(x, y, backColor2);
                        }
                     }
                  }
                  using (Brush b = new TextureBrush(bmp)) {
                     pe.Graphics.FillRectangle(b, new Rectangle(paddingH, 0, Width - 2 * paddingH, Height - marginBottom));
                  }
               }
            }

            if (colorMid.IsEmpty) {
               LinearGradientBrush gradient = new LinearGradientBrush(new Point(), new Point(Width, 0), color1, color2);
               pe.Graphics.FillRectangle(gradient, new Rectangle(paddingH, 0, Width - 2 * paddingH, Height - marginBottom));
               gradient.Dispose();
            } else {
               LinearGradientBrush gradient = new LinearGradientBrush(new Point(), new Point(Width / 2, 0), color1, colorMid);
               pe.Graphics.FillRectangle(gradient, new Rectangle(paddingH, 0, Width / 2 - paddingH, Height - marginBottom));
               gradient.Dispose();

               gradient = new LinearGradientBrush(new Point(Width / 2, 0), new Point(Width, 0), colorMid, color2);
               pe.Graphics.FillRectangle(gradient, new Rectangle(Width / 2, 0, Width / 2 - paddingH, Height - marginBottom));
               gradient.Dispose();
            }
         } else {
            for (int x = paddingH; x < Width - paddingH; x++) {
               byte h = (byte)Math.Round((double)(x - paddingH) / (Width - paddingH) * 255);
               using (Pen p = new Pen(new HslColor(h, 255, 128).ToRgb())) {
                  pe.Graphics.DrawLine(p, x, 0, x, Height - marginBottom);
               }
            }
         }

         pe.Graphics.InterpolationMode = InterpolationMode.High;
         pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;

         double d = (double)ratio / 255;
         int x0 = (int)((Width - 1 - 2 * paddingH) * d) + paddingH;
         int y0 = Height - 7;
         int triangleWidth = 5;
         int triangleHeight = 9;
         GraphicsPath trianglePath = new GraphicsPath();
         trianglePath.AddLine(x0 - triangleWidth, y0 + triangleHeight, x0 + 0, y0 + 0);
         trianglePath.AddLine(x0 + 0, y0 + 0, x0 + triangleWidth, y0 + triangleHeight);
         trianglePath.CloseFigure();
         SolidBrush triangleBrush = new SolidBrush(Color.Black);
         pe.Graphics.FillPath(triangleBrush, trianglePath);
         triangleBrush.Dispose();
         Pen trianglePen = new Pen(SystemColors.Control);
         pe.Graphics.DrawPath(trianglePen, trianglePath);
         trianglePen.Dispose();
         trianglePath.Dispose();
      }

      protected override void OnMouseDown(MouseEventArgs e) {
         if (e.Button == MouseButtons.Left) {
            int x = e.X;
            if (x < paddingH) x = paddingH;
            if (x > Width - paddingH) x = Width - paddingH;
            Ratio = (byte)((double)(x - paddingH) / (Width - 2 * paddingH) * 255);

            if (numericControl != null)
               numericControl.Focus();
         }

         base.OnMouseDown(e);
      }

      protected override void OnMouseMove(MouseEventArgs e) {
         if (e.Button == MouseButtons.Left) {
            int x = e.X;
            if (x < paddingH)
               x = paddingH;
            if (x > Width - paddingH)
               x = Width - paddingH;
            Ratio = (byte)((double)(x - paddingH) / (Width - 2 * paddingH) * 255);
         }

         base.OnMouseMove(e);
      }

      protected void OnRatioChanged() {
         RatioChanged?.Invoke(this, EventArgs.Empty);
      }
   }
}
