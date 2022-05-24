//#define DRAWBITMAPSEQUENTIEL     // Bilder NACHEINANDER erzeugen (ohne Multithreading einfacher zu debuggen)
//#define WITHOUT_SQLITECACHE      // ohne Cache einiger Level in SQLite
//#define GARMINDRAWTEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
#if !WITHOUT_SQLITECACHE
using System.IO;
#endif
using System.Threading;
using GarminCore;
using GarminImageCreator.Garmin;

namespace GarminImageCreator {
   public class ImageCreator {

      /// <summary>
      /// Liste aller aktiven Garmin-Karten (i.A. nur 1); Abfrage über <see cref="GetGarminMapDefs"/>()!
      /// </summary>
      List<GarminMapData> garminMapData = null;

      /// <summary>
      /// dieser Pfad wird als Cache verwendet
      /// </summary>
      readonly string garminCachePath;

#if DRAWBITMAPSEQUENTIEL
      readonly object lock4drawbitmapsequentiel = new object();
#endif


      /// <summary>
      /// zum erzeugen einer Gesamtkarte
      /// </summary>
      /// <param name="mapdefs">def. der einzelnen Garminkarten die zur Gesamtkarte gehören</param>
      /// <param name="garmincachepath">Pfad für den Garmindatencache</param>
      public ImageCreator(IList<GarminMapData> mapdefs = null,
                          string garmincachepath = null) {
         if (!string.IsNullOrEmpty(garmincachepath))
            garminCachePath = garmincachepath;
         else {
            garminCachePath =
#if WITHOUT_SQLITECACHE
                              "";
#else
                              Path.Combine(Path.GetTempPath(), "garminimagecreator");
#endif
         }

         garminMapData = new List<GarminMapData>();

         if (mapdefs != null)
            SetGarminMapDefs(mapdefs);
      }

      /// <summary>
      /// setzt <see cref="GarminMapData"/>-Liste der zu verwendenden Garmin-Karten neu
      /// </summary>
      /// <param name="newmapdefs"></param>
      public void SetGarminMapDefs(IList<GarminMapData> newmapdefs) {
         if (newmapdefs != null) {
            Interlocked.Exchange(ref garminMapData, new List<GarminMapData>(newmapdefs));
         }
      }

      /// <summary>
      /// liefert die akt. registrierte <see cref="GarminMapData"/>-Liste
      /// </summary>
      /// <returns></returns>
      public List<GarminMapData> GetGarminMapDefs() {
         return Interlocked.Exchange(ref garminMapData, garminMapData);
      }

      /// <summary>
      /// Teil des Bildes, der gezeichnet werden soll
      /// </summary>
      public enum PictureDrawing {
         /// <summary>
         /// gesamtes Bild
         /// </summary>
         all,
         /// <summary>
         /// alles VOR dem Hillshading
         /// </summary>
         beforehillshade,
         /// <summary>
         /// alles NACH dem Hillshading
         /// </summary>
         afterhillshade,
      }


      /// <summary>
      /// Bild zeichnen (ev. nacheinander aus mehreren Garmin-Karten)
      /// </summary>
      /// <param name="bm">Zielbitmap</param>
      /// <param name="lon1">links</param>
      /// <param name="lat1">unten</param>
      /// <param name="lon2">rechts</param>
      /// <param name="lat2">oben</param>
      /// <param name="garminMapData"></param>
      /// <param name="zoom">Zoomstufe</param>
      public void DrawImage(Bitmap bm,
                            double lon1,
                            double lat1,
                            double lon2,
                            double lat2,
                            IList<GarminMapData> garminMapdata,
                            IList<double> groundResolution,
                            int zoom,
                            PictureDrawing picturePart,
                            ref object extdata) {
#if DRAWBITMAPSEQUENTIEL
         lock (lock4drawbitmapsequentiel) {
#endif

#if GARMINDRAWTEST
            global::FSofTUtils.Sys.HighResolutionWatch hrw = new global::FSofTUtils.Sys.HighResolutionWatch();
#endif
         try {
            GeoConverter conv = new GeoConverter(lon1,
                                                 lat2,
                                                 (lon2 - lon1) / bm.Width,
                                                 -(lat2 - lat1) / bm.Height);  // wegen Bitmap-Orientierung von oben nach unten
#if GARMINDRAWTEST
               hrw.Start();
               hrw.Store(string.Format("Start DrawImage(): {0}", picturePart));
#endif

            Graphics canvas = Graphics.FromImage(bm);
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            //canvas.CompositingQuality = CompositingQuality.HighQuality;
            //canvas.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            //canvas.InterpolationMode = InterpolationMode.High;

            if (picturePart == PictureDrawing.all ||
                picturePart == PictureDrawing.beforehillshade)
               //canvas.Clear(Color.White);
               canvas.Clear(Color.LightGray);
            //canvas.DrawRectangle(new Pen(Color.Black, 3), 0, 0, bm.Width, bm.Height);
            //Font font = new Font("Arial", 10);
            //canvas.DrawString(string.Format("Lon {0}, Lat {1}", lon1, lat1), font, new SolidBrush(Color.Red), 5, 5);
            //Debug.WriteLine(string.Format(">>> DrawImage: lon1={0} lon2={1} lat1={2} lat2={3}", lon1, lon2, lat1, lat2));

            Bound picturebound = new Bound(lon1, lon2, lat1, lat2);
            Dictionary<ObjectText.ObjectType, List<ObjectText>> objectTextLists = extdata == null ?
                                                         new Dictionary<ObjectText.ObjectType, List<ObjectText>>() :
                                                         extdata as Dictionary<ObjectText.ObjectType, List<ObjectText>>;
            extdata = objectTextLists;
            List<double> textFactor = new List<double>();


            if (garminMapdata != null) {
               try {

                  for (int m = 0; m < garminMapdata.Count; m++) {
                     GarminMapData gMapData = garminMapdata[m];
                     double groundresolution = groundResolution[m]; // Projection.GetGroundResolution(zoom, gMapData.detailMapManager.GetMapCenterLat());

#if GARMINDRAWTEST
                        hrw.Store(string.Format("Start DetailMapManager.GetAllData(), m={0}", m));
#endif
                     gMapData.DetailMapManager.GetAllData(picturebound,
                                                          groundresolution,
                                                          out SortedList<int, List<DetailMap.GeoPoint>> points,
                                                          out SortedList<int, List<DetailMap.GeoPoly>> lines,
                                                          out SortedList<int, List<DetailMap.GeoPoly>> areas);
#if GARMINDRAWTEST
                        hrw.Store(string.Format("End DetailMapManager.GetAllData(), m={0}", m));
#endif

                     textFactor.Add(gMapData.GraphicData.FontFactor);

#if GARMINDRAWTEST
                        hrw.Store(string.Format("Start drawAreas(), m={0}", m));
#endif
                     if (picturePart == PictureDrawing.all ||
                         picturePart == PictureDrawing.beforehillshade) {
                        List<ObjectText> objectTextList = new List<ObjectText>();
                        objectTextLists.Add(ObjectText.ObjectType.Area, objectTextList);
                        drawAreas(canvas,
                                  conv,
                                  groundresolution,
                                  gMapData,
                                  areas,
                                  objectTextList);
                     }

#if GARMINDRAWTEST
                        hrw.Store(string.Format("Start drawLines(), m={0}", m));
#endif
                     if (picturePart == PictureDrawing.all ||
                         picturePart == PictureDrawing.afterhillshade) {
                        List<ObjectText> objectTextList = new List<ObjectText>();
                        objectTextLists.Add(ObjectText.ObjectType.Line, objectTextList);
                        drawLines(canvas,
                                  conv,
                                  groundresolution,
                                  gMapData,
                                  lines,
                                  objectTextList);
                     }

#if GARMINDRAWTEST
                        hrw.Store(string.Format("Start drawPoints(), m={0}", m));
#endif
                     if (picturePart == PictureDrawing.all ||
                         picturePart == PictureDrawing.afterhillshade) {
                        List<ObjectText> objectTextList = new List<ObjectText>();
                        objectTextLists.Add(ObjectText.ObjectType.Point, objectTextList);
                        drawPoints(canvas,
                                   conv,
                                   groundresolution,
                                   gMapData,
                                   points,
                                   objectTextList);
                     }
                  }

#if GARMINDRAWTEST
                     hrw.Store("Start drawObjectTexts()");
#endif
                  if (picturePart == PictureDrawing.all ||
                      picturePart == PictureDrawing.afterhillshade) {
                     drawObjectTexts(canvas, objectTextLists, bm.Width, (float)(textFactor.Count == 0 ? 1.0 : textFactor[0]));

                     foreach (var type in objectTextLists.Keys) {
                        foreach (var item in objectTextLists[type]) {
                           item.Dispose();
                        }
                        objectTextLists[type].Clear();
                     }
                     objectTextLists.Clear();
                  }

               } catch (Exception ex) {

                  Font font = new Font("Arial", 10);
                  RectangleF rect = new RectangleF(0, 0, bm.Width, bm.Height);
                  canvas.DrawRectangle(new Pen(Color.DarkGray), 0, 0, rect.Width, rect.Height);
                  canvas.DrawString(ex.Message, font, new SolidBrush(Color.Red), rect);

               }
            }

            canvas.Flush();
#if GARMINDRAWTEST
               hrw.Store("End DrawImage()");
               hrw.Stop();
#endif

         } catch (Exception ex) {
            throw new Exception("DrawImage: " + ex.Message);
         }

#if GARMINDRAWTEST
            for (int i = 0; i < hrw.Count; i++) {
               Debug.WriteLine(string.Format("GARMINDRAWTEST: {0,7:F1} {1,7:F1} {2}",
                                             hrw.Seconds(i) * 1000,
                                             hrw.StepSeconds(i) * 1000,
                                             hrw.Description(i)));
            }
#endif

#if DRAWBITMAPSEQUENTIEL
         }
#endif

      }

      void drawAreas(Graphics canvas,
                     GeoConverter conv,
                     double groundresolution,
                     GarminMapData gMapData,
                     SortedList<int, List<DetailMap.GeoPoly>> areas,
                     List<ObjectText> objectTextList) {
         int[] types = new int[areas.Count];
         areas.Keys.CopyTo(types, 0);

         try {

            for (int type = types.Length - 1; type >= 0; type--) {   // höchste Typen zuerst zeichnen
               if (types[type] == 0x4b00 ||
                   types[type] == 0x4a00) // Garmin-Hintergrund
                  continue;
               GarminGraphicData.AreaData drawdata = gMapData.GraphicData.Areas.TryGetValue(types[type], out drawdata) ? drawdata : null;
               Brush brush = drawdata?.GetBrush();

               GarminGraphicData.ObjectData.FontSize fontSize = drawdata.Fontsize;
               Font font = gMapData.GraphicData.Fonts.TextFont(drawdata.Fontsize);

               foreach (DetailMap.GeoPoly area in areas[types[type]]) {
                  if (drawArea(canvas,
                               area,
                               conv,
                               brush,
                               groundresolution))
                     if (!string.IsNullOrEmpty(area.Text) &&
                         fontSize != GarminGraphicData.ObjectData.FontSize.NoFont)
                        objectTextList.Add(new ObjectText(area.Text,
                                                          ObjectText.ObjectType.Area,
                                                          types[type],
                                                          font,
                                                          drawdata.TextColor,
                                                          conv.Convert(area.Bound.CenterXDegree, area.Bound.CenterYDegree)));
               }

               brush?.Dispose();
            }

         } catch (Exception ex) {
            Debug.WriteLine("EXCEPTION DrawImage/DrawArea: " + ex.Message);
         }
      }

      void drawLines(Graphics canvas,
                     GeoConverter conv,
                     double groundresolution,
                     GarminMapData gMapData,
                     SortedList<int, List<DetailMap.GeoPoly>> lines,
                     List<ObjectText> objectTextList) {
         int[] types = new int[lines.Count];
         lines.Keys.CopyTo(types, 0);

         try {

            for (int type = types.Length - 1; type >= 0; type--) {   // höchste Typen zuerst zeichnen
               GarminGraphicData.LineData drawdata = gMapData.GraphicData.Lines.TryGetValue(types[type], out drawdata) ? drawdata : null;
               //if (types[type] == 0x0100) {
               //   Debug.WriteLine("");
               //}
               if (drawdata != null &&
                   !drawdata.IsTransparent) { // NICHT unsichtbare Linie
                  Pen pen = drawdata.GetPen();
                  Pen innerpen = drawdata != null ?
                                       drawdata.WithBorder ? drawdata.GetInnerPen() : null :
                                       null;

                  GarminGraphicData.ObjectData.FontSize fontSize = drawdata.Fontsize;
                  Font font = gMapData.GraphicData.Fonts.TextFont(drawdata.Fontsize);

                  foreach (DetailMap.GeoPoly line in lines[types[type]]) {
                     if (drawLine(canvas,
                                  line,
                                  conv,
                                  drawdata != null && drawdata.WithBitmap,
                                  pen,
                                  innerpen,
                                  groundresolution))
                        if (!string.IsNullOrEmpty(line.Text) &&
                            fontSize != GarminGraphicData.ObjectData.FontSize.NoFont) {

                           getRefLine4LineText(line.Points, out PointF p1, out PointF p2);
                           objectTextList.Add(new ObjectText(line.Text,
                                                             ObjectText.ObjectType.Line,
                                                             types[type],
                                                             font,
                                                             drawdata.TextColor,
                                                             conv.Convert(p1.X, p1.Y),
                                                             conv.Convert(p2.X, p2.Y)));
                        }

                  }

                  //font?.Dispose();

                  pen?.Dispose();
                  innerpen?.Dispose();
               }
            }

         } catch (Exception ex) {
            Debug.WriteLine("EXCEPTION DrawImage/DrawLine: " + ex.Message);
         }
      }

      void drawPoints(Graphics canvas,
                      GeoConverter conv,
                      double groundresolution,
                      GarminMapData gMapData,
                      SortedList<int, List<DetailMap.GeoPoint>> points,
                      List<ObjectText> objectTextList) {
         int[] types = new int[points.Count];
         points.Keys.CopyTo(types, 0);

         try {

            for (int type = types.Length - 1; type >= 0; type--) {   // höchste Typen zuerst zeichnen
               GarminGraphicData.PointData drawdata = gMapData.GraphicData.Points.TryGetValue(types[type], out drawdata) ? drawdata : null;
               Bitmap bitmap = drawdata != null && drawdata.WithBitmap ?
                                    drawdata.BitmapClone :
                                    null;

               GarminGraphicData.ObjectData.FontSize fontSize = drawdata.Fontsize;
               Font font = gMapData.GraphicData.Fonts.TextFont(drawdata.Fontsize);
               float pointdeltay = fontSize != GarminGraphicData.ObjectData.FontSize.NoFont ?
                                          (font.GetHeight() + bitmap.Height) / 2 :
                                          0;

               foreach (DetailMap.GeoPoint point in points[types[type]]) {
                  drawPoint(canvas,
                            point,
                            conv,
                            bitmap,
                            groundresolution);
                  if (!string.IsNullOrEmpty(point.Text) &&
                      fontSize != GarminGraphicData.ObjectData.FontSize.NoFont) {
                     PointF pt = conv.Convert(point);
                     pt.Y -= pointdeltay;
                     objectTextList.Add(new ObjectText(point.Text,
                                                       ObjectText.ObjectType.Point,
                                                       types[type],
                                                       font,
                                                       drawdata.TextColor,
                                                       pt));
                  }
               }

               bitmap?.Dispose();
            }

         } catch (Exception ex) {
            Debug.WriteLine("EXCEPTION DrawImage/DrawPoint: " + ex.Message);
         }
      }


      /// <summary>
      /// gibt die Texte nach Möglichkeit aus
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="objectTextLists"></param>
      /// <param name="tilesize"></param>
      /// <param name="fontFactor"></param>
      void drawObjectTexts(Graphics canvas,
                           Dictionary<ObjectText.ObjectType, List<ObjectText>> objectTextLists,
                           float tilesize,
                           float fontFactor) {
         StringFormat sf = new StringFormat() {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
         };

         // alle verwendeten SolidBrush bereitstellen 
         SortedDictionary<int, SolidBrush> brushes = new SortedDictionary<int, SolidBrush>();
         foreach (var type in objectTextLists.Keys) {
            foreach (var item in objectTextLists[type]) {
               if (!brushes.ContainsKey(item.Color.ToArgb()))
                  brushes.Add(item.Color.ToArgb(), new SolidBrush(item.Color));
            }
         }

         canvas.SmoothingMode = SmoothingMode.HighQuality;
         canvas.CompositingQuality = CompositingQuality.Default;
         canvas.TextRenderingHint = TextRenderingHint.SystemDefault;
         canvas.InterpolationMode = InterpolationMode.Default;

         Pen penOutline = new Pen(Color.White, fontFactor * 3) {      // Stift für die Umrandung der Schrift (bessere Lesbarkeit)
            LineJoin = LineJoin.Round        // damit u.a. die M's nicht spitz sind
         };


         List<ObjectText> usedText = new List<ObjectText>();

         foreach (var type in new ObjectText.ObjectType[] { ObjectText.ObjectType.Line,
                                                            ObjectText.ObjectType.Point,
                                                            ObjectText.ObjectType.Area}) {
            List<ObjectText> objectTextList = objectTextLists[type];
            objectTextList.Sort();

            for (int j = 0; j < objectTextList.Count; j++) {
               ObjectText objectText = objectTextList[j];
               RectangleF txtRect = objectText.GetTextArea(canvas, sf, tilesize);
               if (txtRect == RectangleF.Empty)
                  continue;

               if (areaIsFree4Text(objectText, usedText)) {
                  usedText.Add(objectText);

                  switch (objectText.ObjType) {
                     case ObjectText.ObjectType.Line:
                        myDrawString(canvas,
                                     objectText.Text,
                                     objectText.Font,
                                     brushes[objectText.Color.ToArgb()],
                                     penOutline,
                                     objectText.ReferencePoint,
                                     sf,
                                     objectText.Angle);
                        break;

                     case ObjectText.ObjectType.Area:
                     case ObjectText.ObjectType.Point:
                        myDrawString(canvas,
                                     objectText.Text,
                                     objectText.Font,
                                     brushes[objectText.Color.ToArgb()],
                                     penOutline,
                                     objectText.ReferencePoint,
                                     sf);
                        break;
                  }
               }
            }

         }

         usedText.Clear();

         foreach (var item in brushes)
            item.Value?.Dispose();

         penOutline?.Dispose();
      }

      void getRefLine4LineText(IList<PointF> linepts, out PointF p1, out PointF p2) {
         int p = linepts.Count / 2;
         // Für wäre auch der dem Schwerpunkt nächstliegende Punkt nützlich.

         // längstes Segment bestimmen
         //double maxlen = -1;
         //for (int i = 1; i < linepts.Count; i++) {
         //   double len = (linepts[i - 1].X - linepts[i].X) * (linepts[i - 1].X - linepts[i].X) +
         //                (linepts[i - 1].Y - linepts[i].Y) * (linepts[i - 1].Y - linepts[i].Y);
         //   if (len > maxlen) {
         //      maxlen = len;
         //      p = i;
         //   }
         //}


         if (linepts.Count > 2) {
            // Punkt vor und nachte der "Mitte"
            p1 = linepts[p - 1];
            p2 = linepts[p + 1];
         } else {
            // "mittlere" 2 Punkte
            p1 = linepts[p - 1];
            p2 = linepts[p];
         }


      }


      /// <summary>
      /// Test, ob der <see cref="ObjectText"/> an seine festgelegte Position gezeichnet werden darf (also keine Überdeckung mit anderen Texten erfolgt)
      /// </summary>
      /// <param name="text"></param>
      /// <param name="objectTextList"></param>
      /// <returns></returns>
      bool areaIsFree4Text(ObjectText text, List<ObjectText> objectTextList) {
         foreach (var item in objectTextList) {
            if (text.IntersectsWith(item, null))
               return false;
         }
         return true;
      }

      /// <summary>
      /// zeichnet einen Text mit "Outline" und einem Winkel
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="text"></param>
      /// <param name="font"></param>
      /// <param name="brush">Brush für den Text</param>
      /// <param name="outlinepen">Pen für die Umrandung</param>
      /// <param name="pt">Bezugspunkt</param>
      /// <param name="sf"></param>
      /// <param name="angle">Winkel (i.A. 0°)</param>
      void myDrawString(Graphics canvas, string text, Font font, Brush brush, Pen outlinepen, PointF pt, StringFormat sf, float angle = 0) {
         if (outlinepen == null) {

            if (angle != 0) {
               //canvas.Transform.RotateAt(angle, pt);     // fkt. NICHT: Transform muss eine neue, rotierte Matrix zugewiesen werden
               Matrix rotateMatrix = new Matrix();
               rotateMatrix.RotateAt(angle, pt);
               canvas.Transform = rotateMatrix;
            }
            canvas.DrawString(text,
                              font,
                              brush,
                              pt,
                              sf);
            if (angle != 0)
               canvas.ResetTransform();

         } else {

            using (GraphicsPath path = new GraphicsPath()) {
               path.AddString(
                   text,
#if DRAWWITHSKIA
                font.FontFamilyname,
#else
                font.FontFamily,
#endif
                (int)font.Style,
#if DRAWWITHSKIA
                font.SizeInPoints,
#else
                canvas.DpiY * font.SizeInPoints / 72,       // point -> em size
#endif
                pt,
                   sf);
               if (angle != 0) {
                  Matrix rotateMatrix = new Matrix();
                  rotateMatrix.RotateAt(angle, pt);
                  path.Transform(rotateMatrix);
               }
               canvas.DrawPath(outlinepen, path);
               canvas.FillPath(brush, path);
            }

         }
      }


      #region 1 Objekt auf Graphics zeichnen

      /// <summary>
      /// eine Polyline zeichnen
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="line"></param>
      /// <param name="conv"></param>
      /// <param name="bitmappen"></param>
      /// <param name="pen"></param>
      /// <param name="innerpen"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      bool drawLine(Graphics canvas,
                    DetailMap.GeoPoly line,
                    GeoConverter conv,
                    bool bitmappen,
                    Pen pen,
                    Pen innerpen,
                    double groundresolution) {
         if (!conv.HasMinSize(line.Bound))
            return false;

         PointF[] pts = conv.Convert(line);
         if (pts.Length < 2)
            return false;

         if (pen != null) {
            if (bitmappen) {
               drawBitmapLines(canvas, pen, pts, groundresolution);
            } else {
               canvas.DrawLines(pen, pts);
            }
            if (innerpen != null) {
               fakePoints4InnerLine(pts, (pen.Width - innerpen.Width) / 2);
               canvas.DrawLines(innerpen, pts);
            }
         } else // unbekannter Typ
            canvas.DrawLines(new Pen(Color.LightGray), pts);
         return true;
      }

      /// <summary>
      /// "verlängert" das erste und letzte Segment einer Line um einen kleinen Betrag, um die "äußere" Linie zu verdecken
      /// </summary>
      /// <param name="pts"></param>
      /// <param name="deltalength"></param>
      void fakePoints4InnerLine(PointF[] pts, float deltalength) {
         deltalength *= 1.1F;
         fakeEndPoint4InnerLine(pts, true, deltalength);
         fakeEndPoint4InnerLine(pts, false, deltalength);
      }

      void fakeEndPoint4InnerLine(PointF[] pts, bool startpt, float deltalength) {
         int idxend = startpt ? 0 : pts.Length - 1;
         int idxnext = startpt ? 1 : pts.Length - 2;

         float dx = pts[idxend].X - pts[idxnext].X;
         float dy = pts[idxend].Y - pts[idxnext].Y;

         float len = (float)Math.Sqrt(dx * dx + dy * dy);
         float f = (len + deltalength) / len;

         pts[idxend].X += dx * (f - 1);
         pts[idxend].Y += dy * (f - 1);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="area"></param>
      /// <param name="conv"></param>
      /// <param name="brush"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      bool drawArea(Graphics canvas,
                 DetailMap.GeoPoly area,
                 GeoConverter conv,
                 Brush brush,
                 double groundresolution) {
         if (!conv.HasMinSize(area.Bound))
            return false;

         if (brush == null)
            brush = new SolidBrush(Color.LightGray);
         canvas.FillPolygon(brush, conv.Convert(area));
         return true;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="point"></param>
      /// <param name="conv"></param>
      /// <param name="bitmap"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      void drawPoint(Graphics canvas, DetailMap.GeoPoint point, GeoConverter conv, Bitmap bitmap, double groundresolution) {
         PointF pt = conv.Convert(point);
         if (bitmap != null)
            canvas.DrawImageUnscaled(bitmap, (int)(pt.X - bitmap.Width / 2), (int)(pt.Y - bitmap.Height / 2));
         else
            canvas.DrawLine(new Pen(Color.Black), pt, pt);
      }

      /// <summary>
      /// eine Linienfolge zeichnen
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="pen"></param>
      /// <param name="pts"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      void drawBitmapLines(Graphics canvas, Pen pen, PointF[] pts, double groundresolution) {
         for (int i = 1; i < pts.Length; i++)
            drawBitmapLine(canvas, pts[i - 1], pts[i], pen, groundresolution);
      }

      /// <summary>
      /// eine einzelne Linie zeichnen
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="pen"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      void drawBitmapLine(Graphics canvas, PointF p1, PointF p2, Pen pen, double groundresolution) {
         try {
            float delaty = p2.Y - p1.Y;
            //if (delaty == 0) { // keine Drehung/Verschiebung nötig (doch: bei Muster)
            //   canvas.DrawLine(pen, p1, p2);
            //} else {
            canvas.TranslateTransform(p1.X, p1.Y); // Koordinatenursprung nach p1 verschoben
                                                   //Test: canvas.DrawLine(pen, 0F, 0F, p2.X - p1.X, p2.Y - p1.Y);

            float delatx = p2.X - p1.X;
            float distance = (float)Math.Sqrt(delatx * delatx + delaty * delaty);
            if (distance > 0) { // sonst lohnt sich das Zeichnen nicht

               //ACHTUNG: Koordinatensystem ist nach unten gerichtet
               //         Drehwinkel im Uhrzeigersinn
               float angle = (float)(Math.Asin(delaty / distance) * 180 / Math.PI); // -90°..+90°
               if (delatx < 0)
                  angle = 180 - angle;
               canvas.RotateTransform(angle); // Drehungswinkel in Grad.

               canvas.DrawLine(pen, 0F, 0F, distance, 0F);
            }
            //}
         } catch (Exception ex) {
            Debug.WriteLine("EXCEPTION in drawBitmapLine: " + ex.Message);
            throw new Exception("EXCEPTION in drawBitmapLine: " + ex.Message);
         } finally {
            canvas.ResetTransform();
         }
      }

      #endregion

      #region Objektsuche

      bool PointIsInPolygon(double ptx, double pty, PointF[] pts) {
         double[] polyx = new double[pts.Length];
         double[] polyy = new double[pts.Length];
         for (int i = 0; i < pts.Length; i++) {
            polyx[i] = pts[i].X;
            polyy[i] = pts[i].Y;
         }
         return global::FSofTUtils.Geography.GeoHelper.PointIsInPolygon(ptx, pty, polyx, polyy);
      }

      bool PointIsInNearPolyline(double ptx, double pty, PointF[] pts, double delta) {
         double[] polyx = new double[pts.Length];
         double[] polyy = new double[pts.Length];
         for (int i = 0; i < pts.Length; i++) {
            polyx[i] = pts[i].X;
            polyy[i] = pts[i].Y;
         }
         return FSofTUtils.Geography.GeoHelper.PointIsInNearPolyline(ptx, pty, polyx, polyy, delta);
      }

      bool PointIsInNearPoint(double ptx, double pty, PointF pt, double deltalatlon) {
         return pt.X - deltalatlon <= ptx && ptx <= pt.X + deltalatlon &&
                pt.Y - deltalatlon <= pty && pty <= pt.Y + deltalatlon;
      }

      /// <summary>
      /// holt Infos über Garminobjekte an dieser Position (und im Umfeld)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="deltalon"></param>
      /// <param name="deltalat"></param>
      /// <param name="groundresolution">Meter je Pixel</param>
      /// <returns></returns>
      public List<SearchObject> GetObjectInfo(double lon, double lat, double deltalon, double deltalat, double groundresolution) {
         List<SearchObject> info = new List<SearchObject>();
         List<GarminMapData> mapData = GetGarminMapDefs();

         if (mapData != null) {
            Bound searcharea = new Bound(lon - deltalon, lon + deltalon, lat - deltalat, lat + deltalat);
            foreach (GarminMapData gMapData in mapData) {
               gMapData.DetailMapManager.GetAllData(searcharea,
                                                    groundresolution,
                                                    out SortedList<int, List<Garmin.DetailMap.GeoPoint>> points,
                                                    out SortedList<int, List<Garmin.DetailMap.GeoPoly>> lines,
                                                    out SortedList<int, List<Garmin.DetailMap.GeoPoly>> areas);

               try {
                  foreach (int type in areas.Keys) {
                     if (type == 0x4b00) // Garmin-Hintergrund
                        continue;
                     string name = "";
                     if (gMapData.GraphicData.Areas.TryGetValue(type, out GarminGraphicData.AreaData d))
                        name = d.Name;
                     foreach (DetailMap.GeoPoly area in areas[type]) {
                        if (!PointIsInPolygon(lon, lat, area.Points))
                           continue;
                        if (name != "" || !string.IsNullOrEmpty(area.Text)) {
                           GarminGraphicData.AreaData drawdata = gMapData.GraphicData.Areas.TryGetValue(type, out drawdata) ? drawdata : null;
                           Bitmap bm = drawdata.GetAsBitmap(32);
                           info.Add(new SearchObject(SearchObject.ObjectType.Area,
                                                     type,
                                                     name,
                                                     ObjectText.SimpleGarminTextConvert(area.Text),
                                                     bm));
                        }
                     }
                  }
               } catch (Exception ex) {
                  throw new Exception("EXCEPTION GetObjectInfo() for Area: " + ex.Message);
               }

               try {
                  foreach (int type in lines.Keys) {
                     string name = "";
                     if (gMapData.GraphicData.Lines.TryGetValue(type, out GarminGraphicData.LineData d))
                        name = d.Name;
                     foreach (DetailMap.GeoPoly line in lines[type]) {
                        if (!PointIsInNearPolyline(lon, lat, line.Points, (deltalon + deltalat) / 2))
                           continue;
                        if (name != "" || line.Text.Length > 0) {
                           GarminGraphicData.LineData drawdata = gMapData.GraphicData.Lines.TryGetValue(type, out drawdata) ? drawdata : null;
                           Bitmap bm = drawdata.GetAsBitmap(32);
                           info.Add(new SearchObject(SearchObject.ObjectType.Line,
                                                     type,
                                                     name,
                                                     ObjectText.SimpleGarminTextConvert(line.Text),
                                                     bm));
                        }
                     }
                  }
               } catch (Exception ex) {
                  throw new Exception("EXCEPTION GetObjectInfo() for Line: " + ex.Message);
               }

               try {
                  foreach (int type in points.Keys) {
                     string name = "";
                     if (gMapData.GraphicData.Points.TryGetValue(type, out GarminGraphicData.PointData d))
                        name = d.Name + ": ";
                     foreach (DetailMap.GeoPoint point in points[type]) {
                        if (!PointIsInNearPoint(lon, lat, new PointF(point.Point.X, point.Point.Y), (deltalon + deltalat) / 2))
                           continue;
                        if (name != "" || point.Text.Length > 0) {
                           GarminGraphicData.PointData drawdata = gMapData.GraphicData.Points.TryGetValue(type, out drawdata) ? drawdata : null;
                           Bitmap bm = drawdata != null && drawdata.WithBitmap ? drawdata.BitmapClone : null;
                           info.Add(new SearchObject(SearchObject.ObjectType.Point,
                                                     type,
                                                     name,
                                                     ObjectText.SimpleGarminTextConvert(point.Text),
                                                     bm));
                        }
                     }
                  }
               } catch (Exception ex) {
                  throw new Exception("EXCEPTION GetObjectInfo() for Point: " + ex.Message);
               }
            }
         }

         info.Sort();
         for (int i = 0; i < info.Count; i++)
            while (i + 1 < info.Count &&
                   info[i] == info[i + 1])
               info.RemoveAt(i + 1);

         return info;
      }

      #endregion

   }
}
