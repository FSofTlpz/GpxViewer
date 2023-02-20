#if GMAP4SKIA
using GMap.NET.Skia;
#else
using GMap.NET.WindowsForms;
#endif
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpecialMapCtrl {

   /// <summary>
   /// zur Anzeige eines Maßstabes für das <see cref="GMap.NET.WindowsForms.GMapControl"/>
   /// </summary>
   public class Scale4Map {

      /// <summary>
      /// am oberen oder unteren Rand zeichnen
      /// </summary>
      public bool OnTop = true;

      /// <summary>
      /// Breite des Zeichenstiftes
      /// </summary>
      public float StrokeWidth;

      /// <summary>
      /// Breite für den "Outline"-Stift
      /// </summary>
      public float OutlineWidth;

      /// <summary>
      /// "Dicke" des Maßstabes (ohne Schrift)
      /// </summary>
      public float Thickness;

      /// <summary>
      /// Abstand vom linken Rand
      /// </summary>
      public float DistanceHorizontal;

      /// <summary>
      /// Abstand vom oberen bzw. unteren Rand
      /// </summary>
      public float DistanceVertical;


      /// <summary>
      /// Das <see cref="GMapControl"/>-Control.
      /// </summary>
      readonly GMapControl gmapControl;

      /// <summary>
      /// Font für die Beschriftung
      /// </summary>
      readonly Font font;

      /// <summary>
      /// Textformat für die Beschriftung
      /// </summary>
      readonly StringFormat stringFormat;


      public Scale4Map(GMapControl gmapctrl,
                       float fontsize =
#if GMAP4SKIA
                                        35) {
#else
                                        12) {
#endif
         gmapControl = gmapctrl;

         font = new Font(
#if GMAP4SKIA
                         gmapControl.Font.FontFamilyname,
#else
                         gmapControl.Font.FontFamily,
#endif
                         fontsize,
                         FontStyle.Regular,
                         GraphicsUnit.Pixel);
         stringFormat = new StringFormat {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near
         };

#if GMAP4SKIA
         StrokeWidth = 4;
         OutlineWidth = 13;
         Thickness = 20;
         DistanceHorizontal = 15;
         DistanceVertical = 15;
#else
         StrokeWidth = 2;
         OutlineWidth = 4;
         Thickness = 7;
         DistanceHorizontal = 10;
         DistanceVertical = 10;
#endif
      }


      /// <summary>
      /// liefert die Auflösung (Pixel je Meter)
      /// </summary>
      /// <returns></returns>
      float GetResolution() {
         return (float)gmapControl.Map_Provider.Projection.GetGroundResolution((int)gmapControl.Map_Zoom, gmapControl.Map_Position.Lat);
      }

      float GetPixel4Meter(float m, float resolution) {
         return m / resolution;
      }

      float GetMeter4Pixel(float pix, float resolution) {
         return pix * resolution;
      }

      /// <summary>
      /// zeichnet einen Maßstab in das Control
      /// </summary>
      /// <param name="canvas"></param>
      public void Draw(Graphics canvas, float rendertranform = 1F) {
         draw(canvas, gmapControl.Width, gmapControl.Height, rendertranform, StrokeWidth, OutlineWidth, Thickness, DistanceHorizontal, DistanceVertical, OnTop);
      }

      /// <summary>
      /// zeichnet einen Maßstab in das Control
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="clientwidth">Breite des Controls</param>
      /// <param name="clientheight">Höhe des Controls</param>
      /// <param name="rendertranform">i.A. 1.0</param>
      /// <param name="strokewidth">Breite des Zeichenstiftes</param>
      /// <param name="outlinewidth">Breite für den "Outline"-Stift</param>
      /// <param name="thickness">"Dicke" des Maßstabes (ohne Schrift)</param>
      /// <param name="left">Abstand vom linken Rand</param>
      /// <param name="toporbottom">Abstand vom oberen bzw. unteren Rand</param>
      /// <param name="ontop">Am oberen oder unteren Rand zeichnen</param>
      void draw(Graphics canvas,
                int clientwidth,
                int clientheight,
                float rendertranform,
                float strokewidth,
                float outlinewidth,
                float thickness,
                float left,
                float toporbottom,
                bool ontop) {
         canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
         canvas.SmoothingMode = SmoothingMode.HighQuality;

         if (gmapControl != null) {

            float resolution = GetResolution();

            float maxlen = GetMeter4Pixel(clientwidth - 14, resolution) / rendertranform;    // max. Länge in m
            int parts;
            if (maxlen > 10000000) {
               maxlen = 10000000; parts = 10;
            } else if (maxlen > 5000000) {
               maxlen = 5000000; parts = 5;
            } else if (maxlen > 2000000) {
               maxlen = 2000000; parts = 4;
            } else if (maxlen > 1000000) {
               maxlen = 1000000; parts = 10;
            } else if (maxlen > 500000) {
               maxlen = 500000; parts = 5;
            } else if (maxlen > 200000) {
               maxlen = 200000; parts = 4;
            } else if (maxlen > 100000) {
               maxlen = 100000; parts = 10;
            } else if (maxlen > 50000) {
               maxlen = 50000; parts = 5;
            } else if (maxlen > 20000) {
               maxlen = 20000; parts = 4;
            } else if (maxlen > 10000) {
               maxlen = 10000; parts = 10;
            } else if (maxlen > 5000) {
               maxlen = 5000; parts = 5;
            } else if (maxlen > 2000) {
               maxlen = 2000; parts = 4;
            } else if (maxlen > 1000) {
               maxlen = 1000; parts = 10;
            } else if (maxlen > 500) {
               maxlen = 500; parts = 5;
            } else if (maxlen > 200) {
               maxlen = 200; parts = 4;
            } else if (maxlen > 100) {
               maxlen = 100; parts = 10;
            } else if (maxlen > 50) {
               maxlen = 50; parts = 5;
            } else if (maxlen > 20) {
               maxlen = 20; parts = 4;
            } else if (maxlen > 10) {
               maxlen = 10; parts = 10;
            } else if (maxlen > 5) {
               maxlen = 5; parts = 5;
            } else if (maxlen > 2) {
               maxlen = 2; parts = 4;
            } else {
               maxlen = 5; parts = 5;
            }

            float partlen = GetPixel4Meter(maxlen, resolution) / parts * rendertranform;

            Pen pen = new Pen(Color.Black);
            Brush blackbrush = new SolidBrush(Color.Black);
            Brush whitebrush = new SolidBrush(Color.White);

            if (!ontop)
               toporbottom = clientheight - toporbottom - thickness;

            canvas.DrawRectangle(pen, left, toporbottom, parts * partlen, thickness);
            canvas.FillRectangle(blackbrush, left, toporbottom, parts * partlen, thickness);
            for (int i = 1; i < parts; i += 2) {
               float x = left + i * partlen;
               canvas.FillRectangle(whitebrush, x, toporbottom, partlen, thickness);
               canvas.DrawRectangle(pen, x, toporbottom, partlen, thickness);
            }

            pen.Width = strokewidth;
            Pen outlinepen = new Pen(Color.White, outlinewidth) {
               LineJoin = LineJoin.Round
            };
            for (int i = 0; i <= parts; i++) {
               float x = left + i * partlen;
               if ((parts == 10 && (i == 0 || i == 5 || i == 10)) ||
                   (parts == 5 && (i == 0 || i == 5)) ||
                   (parts == 4 && (i == 0 || i == 2 || i == 4))) {

                  if (ontop)
                     canvas.DrawLine(pen, x, toporbottom, x, toporbottom + 2 * thickness);
                  else
                     canvas.DrawLine(pen, x, toporbottom + thickness, x, toporbottom - thickness);

                  float val = i * maxlen / parts;
                  if (i != 0)
                     DrawOutlinedString(canvas,
                                        val < 1000 ?
                                              string.Format("{0}m", val) :
                                              string.Format("{0}km", val / 1000),    // {0:F1}km
                                        font,
                                        blackbrush,
                                        outlinepen,
                                        x - outlinewidth,
                                        ontop ?
                                           toporbottom + 1.5F * thickness :
                                           toporbottom - 1F * thickness - font.GetHeight(),
                                        stringFormat);
               }
            }

            canvas.Flush();
         }
      }

      void DrawOutlinedString(Graphics canvas, string txt, Font font, Brush fillbrush, Pen outlinepen, float x, float y, StringFormat sf) {
         using (GraphicsPath gp = new GraphicsPath()) {
            gp.AddString(txt,
#if GMAP4SKIA
                         font.FontFamilyname,
#else
                         font.FontFamily,
#endif
                         (int)font.Style,
                         font.Size,
                         new PointF(x - outlinepen.Width / 4,
                                    y - outlinepen.Width / 4),
                         sf);
            canvas.DrawPath(outlinepen, gp);

            canvas.DrawString(txt,
                              font,
                              fillbrush,
                              new PointF(x, y),
                              sf);

            //canvas.FillPath(fillbrush, gp);
         }
      }

   }
}
