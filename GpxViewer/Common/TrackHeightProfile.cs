using SpecialMapCtrl;
#if ANDROID
using System.Drawing;
#endif
using System.Drawing.Drawing2D;
using Gpx = FSofTUtils.Geography.PoorGpx;
using MyDrawing = System.Drawing;
using FSofTUtils.Geography.PoorGpx;

#if ANDROID
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif
   internal static class TrackHeightProfile {

      #region Höhenprofil erzeugen

      /// <summary>
      /// Bild des Höhenprofils erzeugen
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="track"></param>
      /// <param name="selectedidxlst"></param>
      /// <returns></returns>
      public static Bitmap BuildImage4Track(int width, int height, Track? track, IList<int> selectedidxlst) {
         Bitmap bm = new Bitmap(width, height);

         Dictionary<int, int> selectedidx = new Dictionary<int, int>();
         foreach (int idx in selectedidxlst) {
            selectedidx.Add(idx, 0);
         }

         if (track != null &&
             track.GpxSegment != null &&
             track.GpxSegment.Points.Count > 1) {
            float length = (float)track.StatLength;
            float baseheight = (float)track.StatMinHeigth;
            float deltaheight = (float)(track.StatMaxHeigth - baseheight);

            if (deltaheight > 0) {
               MyDrawing.Color colDiagrBack = MyDrawing.Color.FromArgb(220, 220, 220);
               MyDrawing.Color colLine = MyDrawing.Color.Black;
               MyDrawing.Color colSelected = MyDrawing.Color.OrangeRed;
               MyDrawing.Color colRaster = MyDrawing.Color.FromArgb(180, 180, 180);
               MyDrawing.Brush brushText = new SolidBrush(MyDrawing.Color.Black);

               // Diagrammfläche
               RectangleF rectDiagr = new RectangleF(0.1F * width,      // Koordinatenursprung
                                                     0.05F * height,
                                                     0.85F * width,
                                                     0.9F * height);

               Graphics canvas = Graphics.FromImage(bm);
               canvas.SmoothingMode = SmoothingMode.HighQuality;
               canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
               canvas.Clear(MyDrawing.Color.White);

               //canvas.FillRectangle(new SolidBrush(colDiagrBack), rectDiagr);
               canvas.FillRectangle(new MyDrawing.Drawing2D.LinearGradientBrush(
                                                   new MyDrawing.PointF(rectDiagr.Left, rectDiagr.Top),
                                                   new MyDrawing.PointF(rectDiagr.Left, rectDiagr.Bottom),
                                                   MyDrawing.Color.FromArgb(235, 235, 255),
                                                   MyDrawing.Color.FromArgb(20, 20, 255)),
                                    rectDiagr);



               // Raster zeichnen
               MyDrawing.Font font = new MyDrawing.Font("Arial",
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
                                    new MyDrawing.PointF(rectDiagr.Left, y),
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
                                       new MyDrawing.PointF(x, rectDiagr.Bottom + 0.1F * (height - rectDiagr.Bottom)),
                                       stringFormatx);
               }

               pen.Dispose();

               // Daten einsammeln
               bool[] validPt = new bool[track.GpxSegment.Points.Count];
               bool[] selectedPt = new bool[track.GpxSegment.Points.Count];
               List<MyDrawing.PointF> ptContour = new List<MyDrawing.PointF>();
               float startlength = 0;
               for (int i = 0; i < track.GpxSegment.Points.Count; i++) {
                  float endlength = startlength + (i > 0 ? (float)track.Length(i - 1, i) : 0);

                  float x = rectDiagr.Left + endlength / length * rectDiagr.Width;
                  GpxTrackPoint? tp = track.GetGpxPoint(i);
                  double h = tp != null ? tp.Elevation : BaseElement.NOTVALID_DOUBLE;       // double wegen PoorGpx.BaseElement.NOTVALID_DOUBLE
                  if (h != Gpx.BaseElement.NOTVALID_DOUBLE) {
                     validPt[i] = true;
                     ptContour.Add(new MyDrawing.PointF(x, rectDiagr.Bottom - ((float)h - baseheight) / deltaheight * rectDiagr.Height));
                  } else
                     ptContour.Add(new MyDrawing.PointF(x, rectDiagr.Bottom));

                  startlength = endlength;

                  if (selectedidx != null &&
                      selectedidx.ContainsKey(i))
                     selectedPt[i] = true;
               }

               // Contourfläche zeichnen
               if (ptContour.Count > 1) {
                  ptContour.Add(new MyDrawing.PointF(ptContour[ptContour.Count - 1].X, rectDiagr.Bottom));
                  ptContour.Add(new MyDrawing.PointF(ptContour[0].X, rectDiagr.Bottom));

                  MyDrawing.Drawing2D.LinearGradientBrush brushHeight = new MyDrawing.Drawing2D.LinearGradientBrush(
                                                                           new MyDrawing.PointF(rectDiagr.Left, rectDiagr.Top),
                                                                           new MyDrawing.PointF(rectDiagr.Left, rectDiagr.Bottom),
                                                                           MyDrawing.Color.FromArgb(255, 20, 20),
                                                                           MyDrawing.Color.FromArgb(20, 255, 20));
                  canvas.FillPolygon(brushHeight, ptContour.ToArray());
                  brushHeight.Dispose();

                  ptContour.RemoveRange(ptContour.Count - 2, 2);
               }

               // Contour zeichnen
               Pen penstd = new Pen(colLine);
               Pen penselected = new Pen(colSelected);
               MyDrawing.Brush brushSeleted = new MyDrawing.SolidBrush(colSelected);
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
            }
         }
         return bm;
      }

      /// <summary>
      /// liefert Rasterweite für den Maximalwert
      /// </summary>
      /// <param name="maxval">max. Wert</param>
      /// <returns></returns>
      static float CalculateRasterHorizontal(double maxval) {
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
      static float CalculateRasterVertical(double minval, double maxval, out float rasterminval) {
         float d = CalculateRasterHorizontal(maxval - minval);
         rasterminval = (float)Math.Ceiling(minval / d) * d;
         return d;
      }

      #endregion

   }
}
