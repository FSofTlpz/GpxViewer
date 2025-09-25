using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpecialMapCtrl {

   /// <summary>
   /// zur Anzeige eines Maßstabes für das <see cref="GMap.NET.WindowsForms.GMapControl"/>
   /// </summary>
   public class Scale4Map {

      public enum ScaleKind {
         /// <summary>
         /// ohne
         /// </summary>
         Nothing,
         /// <summary>
         /// Maßstab oben
         /// </summary>
         Top,
         /// <summary>
         /// Maßstab unten
         /// </summary>
         Bottom,
         /// <summary>
         /// Maßstab an allen 4 Seiten
         /// </summary>
         Around,
      }

      /// <summary>
      /// Art des Maßstabes
      /// </summary>
      public ScaleKind Kind = ScaleKind.Around;

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

      public byte Alpha = 180;

      /// <summary>
      /// Das <see cref="SpecialMapCtrl"/>-Control.
      /// </summary>
      readonly SpecialMapCtrl mapControl;

      /// <summary>
      /// Font für die Beschriftung
      /// </summary>
      readonly Font font;

      /// <summary>
      /// Textformat für die Beschriftung
      /// </summary>
      static readonly StringFormat stringFormatRT;
      static readonly StringFormat stringFormatLT;
      static readonly StringFormat stringFormatLB;
      static readonly StringFormat stringFormatRB;


      static Scale4Map() {
         stringFormatRT = new StringFormat {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near
         };
         stringFormatLT = new StringFormat {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
         };
         stringFormatLB = new StringFormat {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Far
         };
         stringFormatRB = new StringFormat {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Far
         };
      }

      public Scale4Map(SpecialMapCtrl mapctrl,
                       float fontsize =
#if GMAP4SKIA
                                        35) {
#else
                                        12) {
#endif
         mapControl = mapctrl;

         font = new Font(
#if GMAP4SKIA
                         mapControl.Font != null ? mapControl.Font.FontFamilyname : string.Empty,
#else
                         mapControl.Font.FontFamily,
#endif
                         fontsize,
                         FontStyle.Regular,
                         GraphicsUnit.Pixel);

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
      /// zeichnet einen Maßstab in das Control
      /// </summary>
      /// <param name="canvas"></param>
      public void Draw(Graphics canvas, float rendertranform = 1F) {
         switch (Kind) {
            case ScaleKind.Nothing:
               break;

            case ScaleKind.Top:
            case ScaleKind.Bottom:
               draw1(mapControl,
                     canvas,
                     font,
                     mapControl.Width,
                     mapControl.Height,
                     rendertranform,
                     StrokeWidth,
                     OutlineWidth,
                     Thickness,
                     DistanceHorizontal,
                     DistanceVertical,
                     Kind == ScaleKind.Top);
               break;

            case ScaleKind.Around:
               draw2(mapControl,
                     canvas,
                     font,
                     Alpha,
                     mapControl.Width,
                     mapControl.Height,
                     rendertranform,
                     StrokeWidth,
                     OutlineWidth,
                     Thickness);
               break;
         }
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
      static void draw1(
                   SpecialMapCtrl mapControl,
                   Graphics canvas,
                   Font font,
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

         if (mapControl != null) {

            float resolution = getCentreResolutionX(mapControl);

            float maxlen = getMeter4Pixel(clientwidth - 14, resolution) / rendertranform;    // max. Länge in m
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

            float partlen = getPixel4Meter(maxlen, resolution) / parts * rendertranform;

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
                     drawOutlinedString(canvas,
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
                                        stringFormatRT);
               }
            }

            canvas.Flush();
         }
      }

      static void draw2(
                  SpecialMapCtrl mapControl,
                  Graphics canvas,
                  Font font,
                  byte alpha,
                  int clientwidth,
                  int clientheight,
                  float rendertranform,
                  float strokewidth,
                  float outlinewidth,
                  float thickness) {
         canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
         canvas.SmoothingMode = SmoothingMode.HighQuality;

         if (mapControl != null) {

            Pen pen = new Pen(Color.Black);
            Brush blackbrush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
            Brush whitebrush = new SolidBrush(Color.FromArgb(alpha, Color.White));
            Brush txtbrush = new SolidBrush(Color.Black);
            Pen outlinepen = new Pen(Color.White, outlinewidth) {
               LineJoin = LineJoin.Round
            };

            /* Der rechteckig dargestellte Bereich (nach Lat/lon rechteckig) ist nach realen Entfernungen näherungsweise eher ein Trapetz.
             * Auf der Nordhalbkugel:  _______
             *                        /       \    
             *                       /_________\  
             *                          
             * Außerdem vergrößert sich der Abstand der Breitengrade jeweils zu den Polen hin                        .
            */

            // Kartenbegrenzung aus der Projektion holen ...
            FSofTUtils.Geometry.PointD boundsLefTopLatLon = new FSofTUtils.Geometry.PointD(mapControl.M_Provider.Projection.Bounds.Left,
                                                                                           mapControl.M_Provider.Projection.Bounds.Top);
            FSofTUtils.Geometry.PointD boundsRightBottomLatLon = new FSofTUtils.Geometry.PointD(mapControl.M_Provider.Projection.Bounds.Right,
                                                                                                mapControl.M_Provider.Projection.Bounds.Bottom);
            // ... und in Clientkoordinaten umrechnen. (Liegt der akt. Clientbereich innerhalb dieser Grenzen,
            // ist der Kartenbereich nirgends überschritten.)
            Point boundsLefTopClient = mapControl.M_LonLat2Client(boundsLefTopLatLon);
            Point boundsRightBottomTopClient = mapControl.M_LonLat2Client(boundsRightBottomLatLon);

            // real verwendeter Clientbereich für die Karte
            Point lefTopClient4Map = new Point(Math.Max(0, boundsLefTopClient.X),
                                               Math.Max(0, boundsLefTopClient.Y));
            Point rightBottomTopClient4Map = new Point(Math.Min(clientwidth, boundsRightBottomTopClient.X),
                                                       Math.Min(clientheight, boundsRightBottomTopClient.Y));

            FSofTUtils.Geometry.PointD leftopLatLon = mapControl.M_Client2LonLat(lefTopClient4Map);
            FSofTUtils.Geometry.PointD rightbottomLatLon = mapControl.M_Client2LonLat(rightBottomTopClient4Map);

            // Auflösung (m je Pixel) am oberen und am unteren Rand (unterschiedlich!)
            float resolutionTop = getResolutionX(mapControl, leftopLatLon.Y);
            float resolutionBottom = getResolutionX(mapControl, rightbottomLatLon.Y);
            // Basislänge in m für einen Abschnitt am oberen und unteren Rand ermitteln:
            int baselenTop = getBaseLengthX(distHorizontal(leftopLatLon.Y, leftopLatLon.X, rightbottomLatLon.X),
                                            resolutionTop,
                                            canvas,
                                            font);                 // Länge in m
            int baselenBottom = getBaseLengthX(distHorizontal(rightbottomLatLon.Y, leftopLatLon.X, rightbottomLatLon.X),
                                               resolutionBottom,
                                               canvas,
                                               font);        // Länge in m
            // Basislänge in Pixel für einen Abschnitt am oberen und unteren Rand ermitteln:
            float baselenpixelTop = getPixel4Meter(baselenTop, resolutionTop) * rendertranform;             // Länge in Pixel
            float baselenpixelBottom = getPixel4Meter(baselenBottom, resolutionBottom) * rendertranform;    // Länge in Pixel

            double minLat = 0;
            if (0 <= rightbottomLatLon.Y)
               minLat = rightbottomLatLon.Y;
            else if (leftopLatLon.Y <= 0)
               minLat = leftopLatLon.Y;
            int baselenLeftMin = getBaseLengthY(distVertical(leftopLatLon.X, leftopLatLon.Y, rightbottomLatLon.Y),
                                                (float)minLat,
                                                mapControl,
                                                canvas,
                                                font);

            if (lefTopClient4Map.X > 0 || lefTopClient4Map.Y > 0)
               canvas.TranslateTransform(lefTopClient4Map.X, lefTopClient4Map.Y);

            // Maßstab am oberen Rand zeichnen
            showHorizontalRule(
                     canvas,
                     true,
                     rightBottomTopClient4Map.X - lefTopClient4Map.X,
                     rightBottomTopClient4Map.Y - lefTopClient4Map.Y,
                     baselenpixelTop,
                     baselenTop,
                     outlinewidth,
                     thickness,
                     pen,
                     outlinepen,
                     blackbrush,
                     whitebrush,
                     txtbrush,
                     font);

            // Maßstab am unteren Rand zeichnen
            showHorizontalRule(
                     canvas,
                     false,
                     rightBottomTopClient4Map.X - lefTopClient4Map.X,
                     rightBottomTopClient4Map.Y - lefTopClient4Map.Y,
                     baselenpixelBottom,
                     baselenBottom,
                     outlinewidth,
                     thickness,
                     pen,
                     outlinepen,
                     blackbrush,
                     whitebrush,
                     txtbrush,
                     font);

            // Maßstab links und rechts zeichen
            showVerticalRule(
                     mapControl,
                     canvas,
                     rightBottomTopClient4Map.X - lefTopClient4Map.X,
                     rightBottomTopClient4Map.Y - lefTopClient4Map.Y,
                     lefTopClient4Map.Y > 0 ? lefTopClient4Map.Y : 0,
                     baselenLeftMin,
                     thickness,
                     (float)rightbottomLatLon.Y,
                     pen,
                     outlinepen,
                     blackbrush,
                     whitebrush,
                     txtbrush,
                     font);

            if (lefTopClient4Map.X > 0 || lefTopClient4Map.Y > 0)
               canvas.ResetTransform();

            canvas.Flush();
         }
      }

      static void showHorizontalRule(
                 Graphics canvas,
                 bool ontop,
                 int clientwidth,
                 int clientheight,
                 float baselenpixel,
                 int baselen,
                 float outlinewidth,
                 float thickness,
                 Pen pen,
                 Pen outlinepen,
                 Brush blackbrush,
                 Brush whitebrush,
                 Brush txtbrush,
                 Font font) {
         float y = ontop ?
                        0 :
                        clientheight - thickness;
         float ytxt = ontop ?
                        1.5F * thickness :
                        clientheight - 1.5F * thickness;

         for (int i = 0; ; i++) {
            float xstart = i * baselenpixel;
            if (xstart > clientwidth)
               break;

            float len = xstart + baselenpixel < clientwidth ?
                              baselenpixel :
                              clientwidth - xstart;
            canvas.DrawRectangle(pen, xstart, y, len, thickness);
            canvas.FillRectangle(i % 2 == 0 ? blackbrush : whitebrush, xstart, y, len, thickness);

            if (i != 0) {
               if (xstart - outlinewidth + 2 * thickness > clientwidth)     // zu dicht am vertikalen Text
                  break;

               string txt = getScaleTxt(i, baselen);
               if (ontop)
                  drawOutlinedString(canvas,
                                     txt,
                                     font,
                                     txtbrush,
                                     outlinepen,
                                     xstart - outlinewidth,
                                     ytxt,
                                     stringFormatRT);
               else
                  drawOutlinedString(canvas,
                                     txt,
                                     font,
                                     txtbrush,
                                     outlinepen,
                                     xstart - outlinewidth,
                                     ytxt,
                                     stringFormatRB);

            }
         }
      }

      static void showVerticalRule(
                  SpecialMapCtrl mapControl,
                  Graphics canvas,
                  int clientwidth,
                  int clientheight,
                  int deltaheight,
                  int baselen,
                  float thickness,
                  float latbottom,
                  Pen pen,
                  Pen outlinepen,
                  Brush blackbrush,
                  Brush whitebrush,
                  Brush txtbrush,
                  Font font) {
         // Textpos. links und rechts
         float x1txt = 1.5F * thickness;
         float x2txt = clientwidth - 1.5F * thickness;

         float fontheight = canvas.MeasureString("X", font).Height;

         float lastytop = clientheight;
         for (int i = 0; lastytop >= 0; i++) {
            // PROBLEM: Ermittlung des ytop zum (i + 1) * baselen
            float lat = getLatitude4Distance(latbottom, (i + 1) * baselen);

            float ytop = mapControl.M_LonLat2Client(0, lat).Y - deltaheight;
            float txty = ytop - thickness / 2;
            if (ytop < 0)
               ytop = 0;

            canvas.DrawRectangle(pen, 0, ytop, thickness, lastytop - ytop);
            canvas.FillRectangle(i % 2 == 0 ? blackbrush : whitebrush, 0, ytop, thickness, lastytop - ytop);

            canvas.DrawRectangle(pen, clientwidth - thickness, ytop, thickness, lastytop - ytop);
            canvas.FillRectangle(i % 2 == 0 ? blackbrush : whitebrush, clientwidth - thickness, ytop, thickness, lastytop - ytop);

            string txt = getScaleTxt(i + 1, baselen);
            if (txty < clientheight - thickness - 2 * fontheight &&
                txty > thickness + fontheight) {
               drawOutlinedString(canvas,
                                  txt,
                                  font,
                                  txtbrush,
                                  outlinepen,
                                  x1txt,
                                  txty,
                                  stringFormatLT);
               drawOutlinedString(canvas,
                                  txt,
                                  font,
                                  txtbrush,
                                  outlinepen,
                                  x2txt,
                                  txty,
                                  stringFormatRT);
            }

            if (ytop <= 0)
               break;

            lastytop = ytop;
         }
      }

      static float getResolutionX(SpecialMapCtrl mapControl, double lat) =>
         (float)mapControl.M_Provider.Projection.GetGroundResolution((int)mapControl.M_Zoom, lat);

      /// <summary>
      /// liefert die Auflösung (Pixel je Meter) für den Mittelpunkt der Karte
      /// </summary>
      /// <returns></returns>
      static float getCentreResolutionX(SpecialMapCtrl mapControl) => getResolutionX(mapControl, mapControl.M_Position.Lat);

      /// <summary>
      /// rechnet Meter in Pixel um (über die Auflösung)
      /// </summary>
      /// <param name="m"></param>
      /// <param name="resolution"></param>
      /// <returns></returns>
      static float getPixel4Meter(float m, float resolution) => m / resolution;

      /// <summary>
      /// rechnet Pixel in Meter um (über die Auflösung)
      /// </summary>
      /// <param name="pix"></param>
      /// <param name="resolution"></param>
      /// <returns></returns>
      static float getMeter4Pixel(float pix, float resolution) => pix * resolution;

      /// <summary>
      /// liefert die geogr. Breite zur vorgegeben Breite <paramref name="latbase"/> mit der Distanz <paramref name="dist2north"/> nach Norden
      /// </summary>
      /// <param name="latbase">Basis-Breite</param>
      /// <param name="dist2north">Entfernung in m zum nördlichen Punkt</param>
      /// <returns></returns>
      static float getLatitude4Distance(float latbase, float dist2north) {
         const double radius = 6370000;         // durchschnittlicher Erdradius
         double deltalat = 180 * dist2north / (Math.PI * radius);

         // Der Wert kann noch approximiert werden. Das lohnt sich für den Maßstab aber kaum.

         //int rounds = 0;
         //do {
         //   double realDist = distVertical(0, latbase, latbase + deltalat);
         //   double error = 1 - realDist / dist2north;
         //   rounds++;

         //   if (error > 0.0001) {            // delta etwas vergrößern
         //      deltalat += error * deltalat;
         //   } else if (error < -0.0001) {    // delta etwas verringern
         //      deltalat += error * deltalat;
         //   } else
         //      break;

         //} while (true && rounds < 5);

         latbase = Math.Min(90, latbase + (float)deltalat);
         return latbase;
      }

      /// <summary>
      /// vordef. Basislängen in m je Maßstabslänge in m
      /// </summary>
      static int[,] baselen4rulerlenx = new int[,] {
         { 100000000, 20000000 },
         { 50000000, 10000000 },
         { 20000000, 5000000 },
         { 10000000, 2000000 },
         { 5000000, 1000000 },
         { 2000000, 500000 },
         { 1000000, 200000 },
         { 500000, 100000 },
         { 200000, 50000 },
         { 100000, 20000 },
         { 50000, 10000 },
         { 20000, 5000 },
         { 10000, 2000 },
         { 5000, 1000 },
         { 2000, 500 },
         { 1000, 200 },
         { 500, 100 },
         { 200, 50 },
         { 100, 20 },
         { 50, 10 },
         { 20, 5 },
         { 10, 2 },
      };

      /// <summary>
      /// ermittelt aus der Gesamtlänge eine sinnvolle Länge (in m) für 1 Abschnitt des waagerechten Maßstabes
      /// </summary>
      /// <param name="maxlen">Gesamtlänge des Maßstabes</param>
      /// <param name="resolution">waagerechte Auflösung (m je Pixel) an der geogr. Breite des Maßstabes</param>
      /// <param name="canvas"></param>
      /// <param name="font">für die Beschriftung</param>
      /// <returns></returns>
      static int getBaseLengthX(float maxlen, float resolution, Graphics canvas, Font font) {
         int idx = 0;
         do {
            if (maxlen > baselen4rulerlenx[idx, 0]) {    // gefunden; jetzt noch Test auf Textlänge
               float txtlenpix, baselenpix;
               idx++;
               do {
                  idx--;
                  txtlenpix = canvas.MeasureString(getScaleTxt((int)maxlen, baselen4rulerlenx[idx, 1]), font).Width;
                  baselenpix = getPixel4Meter(baselen4rulerlenx[idx, 1], resolution);
               } while (baselenpix < txtlenpix && idx > 0);
               return baselen4rulerlenx[idx, 1];
            }
         } while (++idx < baselen4rulerlenx.Length);
         return 1;
      }

      /// <summary>
      /// ermittelt aus der Gesamtlänge eine sinnvolle Länge (in m) für 1 Abschnitt des senkrechten Maßstabes
      /// </summary>
      /// <param name="maxlen">Gesamtlänge des Maßstabes</param>
      /// <param name="minlat">(abs.) kleinste geogr. Breite im Maßstab (kleinster Abschnitt)</param>
      /// <param name="mapControl"></param>
      /// <param name="canvas"></param>
      /// <param name="font">für die Beschriftung</param>
      /// <returns></returns>
      static int getBaseLengthY(float maxlen, float minlat, SpecialMapCtrl mapControl, Graphics canvas, Font font) {
         int idx = 0;
         do {
            if (maxlen > baselen4rulerlenx[idx, 0]) {    // gefunden; jetzt noch Test auf Texthöhe

               float ybase = mapControl.M_LonLat2Client(0, minlat).Y;
               float txtpix, baselenpix;
               idx++;
               do {
                  idx--;
                  txtpix = canvas.MeasureString(getScaleTxt((int)maxlen, baselen4rulerlenx[idx, 1]), font).Height;

                  float y = mapControl.M_LonLat2Client(0, getLatitude4Distance(minlat, baselen4rulerlenx[idx, 1])).Y;

                  baselenpix = Math.Abs(y - ybase);
               } while (baselenpix < txtpix && idx > 0);
               return baselen4rulerlenx[idx, 1];

            }
         } while (++idx < baselen4rulerlenx.Length);
         return 1;
      }

      /// <summary>
      /// liefert einen Text für einen Maßstabsabschnitt
      /// <para>i.A. Wert in km, aber </para>
      /// <para>wenn basem < 500m dann Ausgabe in m, sonst km</para>
      /// <para>wenn basem < 1km dann 1 Kommastelle</para>
      /// </summary>
      /// <param name="idx">Index des Abschnitts (0 ..)</param>
      /// <param name="basem">Länge eines Abschnitts in m</param>
      /// <returns></returns>
      static string getScaleTxt(int idx, int basem) => basem < 500 ?
                   string.Format("{0}m", idx * basem) :
                   string.Format(basem < 1000 ? "{0:F1}km" : "{0}km", idx * basem / 1000.0);    // {0:F1}km

      /// <summary>
      /// horizontale Entfernung zwischen 2 geografischen Längen auf der geografischen Breite
      /// </summary>
      /// <param name="lat"></param>
      /// <param name="lon1"></param>
      /// <param name="lon2"></param>
      /// <returns></returns>
      static float distHorizontal(double lat, double lon1, double lon2) =>
         (float)FSofTUtils.Geography.GeoHelper.Wgs84Distance(lon1, lon2, lat, lat, FSofTUtils.Geography.GeoHelper.Wgs84DistanceCompute.ellipsoid);

      /// <summary>
      /// vertikale Entfernung zwischen 2 geografischen Breiten auf der geografischen Länge
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat1"></param>
      /// <param name="lat2"></param>
      /// <returns></returns>
      static float distVertical(double lon, double lat1, double lat2) =>
         (float)FSofTUtils.Geography.GeoHelper.Wgs84Distance(lon, lon, lat1, lat2, FSofTUtils.Geography.GeoHelper.Wgs84DistanceCompute.ellipsoid);

      static void drawOutlinedString(
                     Graphics canvas,
                     string txt,
                     Font font,
                     Brush fillbrush,
                     Pen outlinepen,
                     float x,
                     float y,
                     StringFormat sf) {
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
