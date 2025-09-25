using FSofTUtils.Geography.PoorGpx;
using FSofTUtils.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FSofTUtils.Geography {

   /// <summary>
   /// diverse Hilfsfunktionen um eine (<see cref="GpxTrackPoint"/>-Liste) zu vereinfachen
   /// </summary>
   public class GpxSimplification {

      /// <summary>
      /// Typ der horizontalen Track-Vereinfachung
      /// </summary>
      public enum HSimplification {
         Nothing,
         Douglas_Peucker,
         Reumann_Witkam
      }

      /// <summary>
      /// Typ der vertikalen Track-Vereinfachung
      /// </summary>
      public enum VSimplification {
         Nothing,
         /// <summary>
         /// gleitenden Mittelwert mit Wichtung der zugehörigen Teil-Streckenlänge
         /// </summary>
         SlidingMean,
         /// <summary>
         /// Integration für variable Streifenbreite um den jeweiligen Punkt
         /// </summary>
         SlidingIntegral,
         /// <summary>
         /// Tiefpassfilter
         /// </summary>
         LowPassFilter,
         /// <summary>
         /// Ridge Regression
         /// </summary>
         RidgeRegression,
      }

      /// <summary>
      /// <see cref="GpxTrackPoint"/> ergänzt um <see cref="Changed"/>/>
      /// </summary>
      class gpxTrackPointExt : GpxTrackPoint {
         public bool Changed = false;

         public gpxTrackPointExt() { }

         public gpxTrackPointExt(GpxTrackPoint p) {
            Lat = p.Lat;
            Lon = p.Lon;
            Elevation = p.Elevation;
            Time = p.Time;
         }

         /// <summary>
         /// erzeugt aus einer <see cref="GpxTrackPoint"/>-Liste eine <see cref="gpxTrackPointExt"/>-Liste
         /// </summary>
         /// <param name="ptlist"></param>
         /// <returns></returns>
         public static List<gpxTrackPointExt> Convert(IList<GpxTrackPoint> ptlist) {
            List<gpxTrackPointExt> lst = new(ptlist.Count);
            for (int p = 0; p < ptlist.Count; p++)
               lst.Add(new gpxTrackPointExt(ptlist[p]));
            return lst;
         }

      }


      /// <summary>
      /// liefert die Länge, die sich aus einem Teil der Punktliste ergibt
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/> (i.A.. min. 2)</param>
      /// <returns></returns>
      public static double GetLength(IList<GpxTrackPoint> ptlst, int startidx = 0, int count = -1) {
         count = Math.Min(count, ptlst.Count - startidx);
         if (count < 0)
            count = ptlst.Count - startidx;
         double length = 0;
         for (int p = startidx + 1; p < startidx + count; p++)
            length += GpxInfos.PointListInfo.PointDistance(ptlst[p - 1], ptlst[p]);
         return length;
      }


      /// <summary>
      /// setzt für jeden <see cref="GpxTrackPoint"/> im Bereich die Höhe
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="height">neue Höhe</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/></param>
      /// <returns>Anzahl der geänderten Punkte</returns>
      public static int SetHeight(IList<GpxTrackPoint> ptlst,
                                   double height,
                                   int startidx = 0,
                                   int count = -1) {
         int changed = 0;
         startidx = Math.Max(0, startidx);
         int endidx = count < 0 ?
                           ptlst.Count :
                           Math.Min(startidx + count, ptlst.Count);
         for (int p = startidx; p < endidx; p++)
            if (ptlst[p].Elevation != height) {
               ptlst[p].Elevation = height;
               changed++;
            }
         return changed;
      }

      /// <summary>
      /// löscht für jeden <see cref="GpxTrackPoint"/> im Bereich die Höhe
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/></param>
      /// <returns>Anzahl der geänderten Punkte</returns>
      public static int RemoveHeight(IList<GpxTrackPoint> ptlst, int startidx = 0, int count = -1) =>
         SetHeight(ptlst, BaseElement.NOTVALID_DOUBLE, startidx, count);

      /// <summary>
      /// setzt für jeden <see cref="GpxTrackPoint"/> im Bereich den Zeitstempel
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="ts">neuer Zeitstempel</param>
      /// <param name="height">neue Höhe</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/></param>
      /// <returns>Anzahl der geänderten Punkte</returns>
      public static int SetTimestamp(IList<GpxTrackPoint> ptlst,
                                      DateTime ts,
                                      int startidx = 0,
                                      int count = -1) {
         int changed = 0;
         startidx = Math.Max(0, startidx);
         int endidx = count < 0 ?
                           ptlst.Count :
                           Math.Min(startidx + count, ptlst.Count);
         for (int p = startidx; p < endidx; p++)
            if (ptlst[p].Time != ts) {
               ptlst[p].Time = ts;
               changed++;
            }
         return changed;
      }

      /// <summary>
      /// löscht für jeden <see cref="GpxTrackPoint"/> im Bereich den Zeitstempel
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/></param>
      /// <returns>Anzahl der geänderten Punkte</returns>
      public static int RemoveTimestamp(IList<GpxTrackPoint> ptlst, int startidx = 0, int count = -1)
         => SetTimestamp(ptlst, BaseElement.NOTVALID_TIME, startidx, count);

      /// <summary>
      /// begrenzt für jeden <see cref="GpxTrackPoint"/> im Bereich die Höhe
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="setmincount">Anzahl der auf das Min. gesetzten <see cref="GpxTrackPoint"/></param>
      /// <param name="setmaxcount">Anzahl der auf das Max. gesetzten <see cref="GpxTrackPoint"/></param>
      /// <param name="minheight">min. zulässige Höhe</param>
      /// <param name="maxheight">max. zulässige Höhe</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/></param>
      public static void SetHeight(IList<GpxTrackPoint> ptlst,
                                   out int setmincount,
                                   out int setmaxcount,
                                   double minheight = double.MinValue,
                                   double maxheight = double.MaxValue,
                                   int startidx = 0,
                                   int count = -1) {
         startidx = Math.Max(0, startidx);
         int endidx = count < 0 ?
                           ptlst.Count :
                           Math.Min(startidx + count, ptlst.Count);
         setmincount = setmaxcount = 0;
         for (int p = startidx; p < endidx; p++) {
            double ele = ptlst[p].Elevation;
            if (ele < minheight || maxheight < ele) {
               if (ele < minheight) {
                  ptlst[p].Elevation = minheight;
                  setmincount++;
               } else if (maxheight < ele) {
                  ptlst[p].Elevation = maxheight;
                  setmaxcount++;
               }
            }
         }
      }

      /// <summary>
      /// versucht, Lücken bei den Höhen und Zeiten der <see cref="GpxTrackPoint"/>-Liste zu interpolieren
      /// <para>Es wird einfach nur eine konstante Höhenänderung von Punkt zu Punkt angenommen. 
      /// Andere Varianten (z.B. Länge der Teilstrecken) scheinen auch nicht sinnvoller zu sein.</para>
      /// <para>Es wird jeweils eine konstante Geschwindigkeit angenommen, die sich aus 2 "Randpunkten" des Bereiches ergibt.</para>
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="changedheight">Indexliste der <see cref="GpxTrackPoint"/> mit geänderter Höhe</param>
      /// <param name="changedtimestamp">Indexliste der <see cref="GpxTrackPoint"/> mit geändertem Zeitstempel</param>
      public static void GapFill(IList<GpxTrackPoint> ptlst, out int[] changedheight, out int[] changedtimestamp) {
         changedheight = GapFill4Height(ptlst);
         changedtimestamp = GapFill4Time(ptlst);
      }

      /// <summary>
      /// versucht, Lücken bei den Zeiten der <see cref="GpxTrackPoint"/>-Liste zu interpolieren
      /// <para>Es wird jeweils eine konstante Geschwindigkeit angenommen, die sich aus 2 "Randpunkten" des Bereiches ergibt.</para>
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <returns>Indexliste der interpolierten Werte</returns>
      public static int[] GapFill4Time(IList<GpxTrackPoint> ptlst) {
         List<int> changed = new List<int>();
         if (ptlst.Count > 0) {

            for (int i = 0; i < ptlst.Count; i++) {
               // Bereichsgrenzen ungültiger Höhen ermitteln
               if (ptlst[i].Time == BaseElement.NOTVALID_TIME) {
                  int startidx = i;
                  int endidx = ptlst.Count - 1;
                  for (int j = i; j < ptlst.Count; j++) {
                     if (ptlst[j].Time != BaseElement.NOTVALID_TIME) {
                        i = j - 1;
                        endidx = j - 1;
                        break;
                     }
                  }

                  DateTime time1 = BaseElement.NOTVALID_TIME;
                  DateTime time2 = BaseElement.NOTVALID_TIME;
                  DateTime time3 = BaseElement.NOTVALID_TIME;
                  DateTime time4 = BaseElement.NOTVALID_TIME;
                  if (startidx > 1) {
                     time1 = ptlst[startidx - 2].Time;
                     time2 = ptlst[startidx - 1].Time;
                  } else
                     if (startidx > 0)
                     time2 = ptlst[startidx - 1].Time;

                  if (endidx < ptlst.Count - 2) {
                     time3 = ptlst[endidx + 1].Time;
                     time4 = ptlst[endidx + 3].Time;
                  } else
                     if (endidx < ptlst.Count - 1)
                     time3 = ptlst[endidx + 1].Time;

                  double v = 0;              // Geschwindigkeit für die Interpolation
                  if (time2 != BaseElement.NOTVALID_TIME &&
                      time3 != BaseElement.NOTVALID_TIME) {
                     // Geschwindigkeit aus den beiden begrenzenden Punkten mit gültiger Zeit bestimmen
                     double length = GetLength(ptlst, startidx - 1, endidx - startidx + 3);
                     double sec = time3.Subtract(time2).TotalSeconds;
                     if (length > 0 && sec > 0)
                        v = length / sec;
                  } else
                     if (time1 != BaseElement.NOTVALID_TIME &&
                         time2 != BaseElement.NOTVALID_TIME) {
                     // Geschwindigkeit aus den beiden letzten Punkten mit gültiger Zeit bestimmen
                     double length = GetLength(ptlst, startidx - 1, 2);
                     double sec = time2.Subtract(time1).TotalSeconds;
                     if (length > 0 && sec > 0)
                        v = length / sec;
                  } else
                        if (time3 != BaseElement.NOTVALID_TIME &&
                            time4 != BaseElement.NOTVALID_TIME) {
                     // Geschwindigkeit aus den beiden ersten Punkten mit gültiger Zeit bestimmen
                     double length = GetLength(ptlst, endidx + 1, 2);
                     double sec = time4.Subtract(time3).TotalSeconds;
                     if (length > 0 && sec > 0)
                        v = length / sec;
                  }

                  if (v > 0) {            // sonst ist keine Interpolation möglich 
                     if (time2 == BaseElement.NOTVALID_TIME) {        // Bereich am Anfang
                        ptlst[startidx].Time = time2 = time3.AddSeconds(-GetLength(ptlst, 0, endidx + 2) / v);
                        changed.Add(startidx);
                        startidx++;
                     }
                     double difflength = 0;
                     for (int k = startidx; k <= endidx; k++) {
                        difflength += GetLength(ptlst, k - 1, 2);
                        ptlst[k].Time = time2.AddSeconds(difflength / v);
                        changed.Add(k);
                     }
                  } else {                // Wie??? Mehrere Punkte mit identischer Zeit scheinen sinnlos (?) zu sein.


                  }

               }
            }

         }
         return changed.ToArray();
      }

      /// <summary>
      /// versucht, Lücken bei den Höhen der <see cref="GpxTrackPoint"/>-Liste zu interpolieren
      /// <para>Es wird einfach nur eine konstante Höhenänderung von Punkt zu Punkt angenommen. 
      /// Andere Varianten (z.B. Länge der Teilstrecken) scheinen auch nicht sinnvoller zu sein.</para>
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <returns>Indexliste der interpolierten Werte</returns>
      public static int[] GapFill4Height(IList<GpxTrackPoint> ptlst) {
         List<int> changed = new List<int>();
         if (ptlst.Count > 0) {

            for (int i = 0; i < ptlst.Count; i++) {
               // Bereichsgrenzen ungültiger Höhen ermitteln
               if (ptlst[i].Elevation == BaseElement.NOTVALID_DOUBLE) {
                  int startidx = i;
                  int endidx = ptlst.Count - 1;
                  for (int j = i; j < ptlst.Count; j++) {
                     if (ptlst[j].Elevation != BaseElement.NOTVALID_DOUBLE) {
                        i = j - 1;
                        endidx = j - 1;
                        break;
                     }
                  }

                  double height1 = BaseElement.NOTVALID_DOUBLE;
                  double height2 = BaseElement.NOTVALID_DOUBLE;
                  if (startidx > 0)
                     height1 = ptlst[startidx - 1].Elevation;
                  if (endidx < ptlst.Count - 1)
                     height2 = ptlst[endidx + 1].Elevation;

                  if (height1 == BaseElement.NOTVALID_DOUBLE) {      // die ersten Punkte mit der ersten gültigen Höhe auffüllen (wenn vorhanden)
                     for (int k = startidx; k <= endidx; k++) {
                        ptlst[k].Elevation = height2;
                        changed.Add(k);
                     }
                  } else {
                     if (height2 == BaseElement.NOTVALID_DOUBLE) {   // die letzten Punkte mit der letzten gültigen Höhe auffüllen (wenn vorhanden)
                        for (int k = startidx; k <= endidx; k++) {
                           ptlst[k].Elevation = height1;
                           changed.Add(k);
                        }
                     } else {                            // interpolieren
                                                         // Es wird einfach nur eine konstante Höhenänderung von Punkt zu Punkt angenommen.
                                                         // Andere Varianten (z.B. Länge der Teilstrecken) scheinen auch nicht sinnvoller zu sein.
                        double step = (height2 - height1) / (2 + endidx - startidx);
                        for (int k = startidx; k <= endidx; k++) {
                           ptlst[k].Elevation = height1 + (k - startidx + 1) * step;
                           changed.Add(k);
                        }
                     }
                  }
               }
            }

         }
         return changed.ToArray();
      }

      /// <summary>
      /// horizontale Trackvereinfachung
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="type">Art des Vereinfachungalgorithmus</param>
      /// <param name="width">Streifenbreite, innerhalb der die Vereinfachung erfolgt</param>
      /// <returns>Indexliste der zu entfernenden Punkte</returns>
      public static int[] HorizontalSimplification(List<GpxTrackPoint> ptlst, HSimplification type, double width) {
         // ptlst NICHT IList wegen RemoveAt()
         List<int> removed = [];
         if (type != HSimplification.Nothing) {
            if (ptlst.Count > 2) {
               SimplificationPointList pl = createList4Simplification(ptlst);
               pl.Set(pl.Length - 1, true);

               switch (type) {
                  case HSimplification.Reumann_Witkam:
                     pl.ReumannWitkam(width);
                     break;

                  case HSimplification.Douglas_Peucker:
                     pl.DouglasPeucker(width);
                     break;
               }

               for (int i = 0; i < pl.Length; i++)
                  if (!pl.PointIsValid(i))
                     removed.Add(i);

               for (int i = pl.Length - 1; i >= 0; i--)
                  if (!pl.PointIsValid(i)) { 
                     ptlst.RemoveAt(i);
                     removed.Add(i);
                  }
            }
         }
         return removed.ToArray();
      }

      /// <summary>
      /// vertikale Trackvereinfachung/Höhenglättung
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="type">Art des Vereinfachungalgorithmus</param>
      /// <param name="param">Parameter-Array für den Vereinfachungalgorithmus</param>
      /// <returns>Indexliste der geänderten Höhen für Punkte</returns>
      public static int[] VerticalSimplification(IList<GpxTrackPoint> ptlst,
                                                 VSimplification? type,
                                                 double[] param) {
         List<int> changed = [];
         if (type != VSimplification.Nothing) {
            bool bPointsNotValid = false;
            for (int i = 0; i < ptlst.Count; i++)
               if (ptlst[i].Elevation == BaseElement.NOTUSE_DOUBLE ||
                   ptlst[i].Elevation == BaseElement.NOTVALID_DOUBLE) {
                  bPointsNotValid = true;
                  break;
               }
            if (bPointsNotValid || ptlst.Count < 2)
               throw new Exception("Zu wenig Punkte oder Punkte ohne Höhenangabe.");

            // Daten übernehmen
            SimplificationPointList? profile = null;

            int fractionaldigits = 1;

            switch (type) {
               case VSimplification.SlidingMean:
                  if (param == null || param.Length < 2)
                     throw new Exception("Zu wenig Parameter angegeben (2).");
                  int ptcount = Math.Max(3, (int)param[0]);    // >= 3
                  fractionaldigits = (int)param[1];

                  profile = createProfileList(ptlst);
                  profile.HeigthProfileWithSlidingMean(ptcount);
                  break;

               case VSimplification.SlidingIntegral:
                  if (param == null || param.Length < 2)
                     throw new Exception("Zu wenig Parameter angegeben (2).");
                  double dWidth = param[0];
                  fractionaldigits = (int)param[1];

                  profile = createProfileList(ptlst);
                  profile.HeigthProfileWithSlidingIntegral(dWidth);
                  break;

               case VSimplification.LowPassFilter:
                  if (param == null || param.Length < 4)
                     throw new Exception("Zu wenig Parameter angegeben (4).");
                  double dFreq = param[0];
                  double dSamplerate = param[1];
                  double dDelay = param[2];
                  fractionaldigits = (int)param[3];

                  ListTS<GpxTrackPoint> tmplst = new(ptlst);
                  if (ElevationFilter.Butterworth2Filter(tmplst, tmplst, dFreq, dSamplerate, dDelay, fractionaldigits) > 0)
                     for (int i = 0; i < tmplst.Count; i++)
                        if (ptlst[i].Elevation != tmplst[i].Elevation) {
                           ptlst[i].Elevation = tmplst[i].Elevation;
                           changed.Add(i);
                        }
                  break;

               case VSimplification.RidgeRegression:
                  if (param == null || param.Length < 4)
                     throw new Exception("Zu wenig Parameter angegeben (4).");
                  int iWidth = (int)param[0];
                  int iOverlap = (int)param[1];
                  double dLambda = param[2];
                  fractionaldigits = (int)param[3];

                  profile = createProfileList(ptlst);
                  profile.HeigthProfileWithRidgeRegression(iWidth, iOverlap, dLambda, true);
                  break;
            }

            // Daten speichern
            if (profile != null) {
               profile.RoundHeightProfile(fractionaldigits);
               for (int p = 0; p < profile.Length; p++) {
                  double v = profile.GetY(p);
                  if (ptlst[p].Elevation != v) {
                     ptlst[p].Elevation = v;
                     changed.Add(p);
                  }
               }
            }
         }

         return changed.ToArray();
      }

      /// <summary>
      /// Entfernung von Punkten mit "Ausreißer"-Höhen (zu starker An-/Abstieg)
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="width">Untersuchungslänge des Wegstückes</param>
      /// <param name="maxascend">max. An-/Abstieg in Prozent (0..100)</param>
      /// <returns>Indexliste der geänderten An-/Abstiege (Höhen)</returns>
      public static int[] RemoveHeigthOutlier(IList<GpxTrackPoint> ptlst, double width, double maxascend) {
         bool bPointsNotValid = false;
         for (int i = 0; i < ptlst.Count; i++)
            if (ptlst[i].Elevation == BaseElement.NOTUSE_DOUBLE ||
                ptlst[i].Elevation == BaseElement.NOTVALID_DOUBLE) {
               bPointsNotValid = true;
               break;
            }
         if (bPointsNotValid || ptlst.Count < 2)
            throw new Exception("Zu wenig Punkte (min. 2) oder Punkte ohne Höhenangabe.");

         List<gpxTrackPointExt> lst = gpxTrackPointExt.Convert(ptlst);

         maxascend /= 100; // 0..1

         // Höhen mit einem durchschnittlichen Anstieg neu berechnen, wenn der max. Anstieg überschritten wird
         int count = 0;
         for (int i = 1; i < lst.Count; i++) {
            double dist = GpxInfos.PointListInfo.PointDistance(lst[i - 1], lst[i]);
            if (maxascend < Math.Abs(lst[i].Elevation - lst[i - 1].Elevation) / dist) {
               double meanascend = getMeanAscendBefore(lst, i - 1, width);
               if (double.IsNaN(meanascend))
                  meanascend = 0;
               double meanelevation = lst[i - 1].Elevation + dist * meanascend; // wenn es mit dem bisher mittleren Anstieg weitergehen würde

               lst[i].Elevation -= (lst[i].Elevation - meanelevation) / 2; // auf 1/2 des zusätzl. Anstiegs abziehen -> "Ausreißer" wird gedämpft

               //lst[i].Elevation = lst[i - 1].Elevation + dist * meanascend;
               lst[i].Changed = true;
               count++;
            }
         }

         // Daten übernehmen
         int[] changed = new int[count];
         if (count > 0) {
            int c = 0;
            for (int p = 0; p < lst.Count; p++)
               if (lst[p].Changed) {
                  ptlst[p].Elevation = lst[p].Elevation;
                  changed[c++] = p;
               }
         }

         return changed;
      }

      /// <summary>
      /// Entfernung von Punkten mit "Ausreißer"-Geschwindigkeiten
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="maxv">Maximalgeschwindigkeit in m/s</param>
      /// <returns>Indexliste der entfernten Punkte</returns>
      public static int[] RemoveSpeedOutlier(List<GpxTrackPoint> ptlst, double maxv) {
         // ptlst NICHT IList wegen RemoveAt()
         List<gpxTrackPointExt> lst = gpxTrackPointExt.Convert(ptlst);

         Dictionary<int, int> removed = [];
         int idxa = 0;
         for (int idxe = 1; idxe < lst.Count; idxe++) {
            double v = vst(lst[idxa], lst[idxe], out double _, out double _);
            //Debug.WriteLine(idxa + " " + idxe + ": " + v);

            // Punkte, die mit einer Geschwindigkeit über der Maximalgeschwindigkeit erreicht werden, werden entfernt.
            if (v > maxv)
               removed.Add(idxe, 0);
            else
               do {
                  idxa++;
               }
               while (removed.ContainsKey(idxa));
         }
         int[] tmp = new int[removed.Count];
         removed.Keys.CopyTo(tmp, 0);  // Indexliste der entfernten Punkte
         for (int p = tmp.Length - 1; p >= 0; p--)
            ptlst.RemoveAt(tmp[p]);

         return tmp;
      }

      /// <summary>
      /// entfernt "Spikes"
      /// <para>ein einzelner Punkt der offensichtlich ein Ausreißer ist, 
      /// mit höherer Geschwindigkeit erreicht wird und
      /// im Bild oft eine schmale Spitze bildet</para>
      /// <para>D.h. es werden für jeden zu testenden Punkt die 2 Vorgänger- und die 2 Nachfolgerpunkte benötigt. Die Liste muss 
      /// also min. 5 Punkte enthalten damit der Algorithmus arbeitet.</para>
      /// </summary>
      /// <param name="ptlst"></param>
      /// <returns></returns>
      public static int[] RemoveSpikes(List<GpxTrackPoint> ptlst) {
         // ptlst NICHT IList wegen RemoveAt()
         const double minspikev = 2.0;          // min. Geschwindigkeitsfaktor vom und zum Spike-Punkt
         const double maxrelspikesides = 1.5;   // max. rel. Längenunterschied der beiden Spike-Wege

         List<int> removed = [];

         // für die geometr. Berechnungen nötig
         SimplificationPointList pl = createList4Simplification(ptlst);

         for (int i = 2; i < ptlst.Count - 2; i++) {
            PointD p1p0 = pl.GetSubstract(i, i - 1);
            PointD p1p2 = pl.GetSubstract(i, i + 1);
            //PointD p0p2 = pl.GetSubstract(i - 1, i + 1);

            double arc = p1p0.Arc(p1p2) * 180 / Math.PI;
            if (arc < 35) {
               double vpre = vst(ptlst[i - 1], ptlst[i - 2], out double s1, out double t1);
               double v01 = vst(ptlst[i], ptlst[i - 1], out double s2, out double t2);
               double v12 = vst(ptlst[i + 1], ptlst[i], out double s3, out double t3);
               double vpast = vst(ptlst[i + 2], ptlst[i + 1], out double s4, out double t4);

               //double s02 = p0p2.Absolute();

               //Debug.WriteLine(i + ": " +
               //   arc.ToString("f0") + " / " +
               //   (vpre == double.MaxValue ? "max" : vpre.ToString("f3")) + " " +
               //   (v01 == double.MaxValue ? "max" : v01.ToString("f3")) + " " +
               //   (v12 == double.MaxValue ? "max" : v12.ToString("f3")) + " " +
               //   (vpast == double.MaxValue ? "max" : vpast.ToString("f3")) + " / " +
               //   s1.ToString("f1") + " " + s2.ToString("f1") + " " + s3.ToString("f1") + " " + s4.ToString("f1") + " / " +
               //   s02.ToString("f1") + " / " +
               //   t1 + " " + t2 + " " + t3 + " " + t4 + " "
               //   );

               if (v01 / vpre >= minspikev &&     // wenn die Geschwindigkeit im Spike min. 2x so groß wie davor/danach ist ...
                   v12 / vpast >= minspikev) {
                  if (Math.Max(s2, s3) / Math.Min(s2, s3) < maxrelspikesides) { // wenn die beiden Seiten im Spike ähnlich lang sind (1..1,5)
                     removed.Add(i);
                  }
               }
            }
         }

         for (int p = removed.Count - 1; p >= 0; p--)
            ptlst.RemoveAt(removed[p]);

         return removed.ToArray();
      }

      /// <summary>
      /// Entfernung von Punkten für einen "Rastplatz" (eine Mindestanzahl von aufeinanderfolgenden Punkten innerhalb 
      /// eines bestimmten Radius mit einer bestimmten durchschnittlichen Mindestrichtungsänderung)
      /// <para>z.B. min. 10 Punkte mit min. 1 Kreuzung im Umkreis von 20 Metern und min. 60° durchschnittliche Winkelabweichung
      /// oder mit min. 2 Kreuzungen im Umkreis von 25 Metern und min. 50° durchschnittliche Winkelabweichung</para>
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="ptcount">Mindestlänge der Punktfolge</param>
      /// <param name="crossing1">1. Anzahl der notwendigen "Kreuzungen"</param>
      /// <param name="maxradius1">1. Maximalradius</param>
      /// <param name="minturnaround1">1. min. durchschnittliche Winkelabweichung</param>
      /// <param name="crossing2">2. Anzahl der notwendigen "Kreuzungen"</param>
      /// <param name="maxradius2">2. Maximalradius</param>
      /// <param name="minturnaround2">2. min. durchschnittliche Winkelabweichung</param>
      /// <param name="protfile">Dateiname für eine Protokolldatei</param>
      /// <returns>Indexliste der entfernten Punkte</returns>
      public static int[] RemoveRestingplace(List<GpxTrackPoint> ptlst,
                                           int ptcount,
                                           int crossing1, double maxradius1, double minturnaround1,
                                           int crossing2, double maxradius2, double minturnaround2,
                                           string? protfile = null) {
         // ptlst NICHT IList wegen RemoveAt()
         List<int> removed = new List<int>();
         if (ptcount >= 3 &&
             crossing1 >= 0 && maxradius1 > 0 && minturnaround1 > 0 &&
             crossing2 > 0 && maxradius2 > 0 && minturnaround2 > 0) {

            SimplificationPointList lst = createList4Simplification(ptlst);
            lst.RemoveRestingplace(ptcount, crossing1, maxradius1, minturnaround1, crossing2, maxradius2, minturnaround2, protfile);
            for (int p = lst.Length - 1; p > 0; p--)
               if (!lst.PointIsValid(p)) {
                  ptlst.RemoveAt(p);
                  removed.Add(p);
               }
         }
         return removed.ToArray();
      }

      /// <summary>
      /// Beschränkung auf das "Kernformat" (z.Z. einfach UnhandledChildXml entfernen)
      /// </summary>
      /// <param name="track"></param>
      public static void SimplifyFormat(GpxTrack? track) {
         track?.UnhandledChildXml?.Clear();

         //if (track.UnhandledChildXml != null) {
         //   for (int i = track.UnhandledChildXml.Count; i >= 0; i--) {
         //      string child = track.UnhandledChildXml[i];

         //   }
         //}
      }

      /// <summary>
      /// Beschränkung auf das "Kernformat" (z.Z. einfach UnhandledChildXml entfernen)
      /// </summary>
      /// <param name="wp"></param>
      public static void SimplifyFormat(GpxWaypoint? wp) {
         wp?.UnhandledChildXml?.Clear();
      }

      /// <summary>
      /// Beschränkung auf das "Kernformat" (z.Z. einfach UnhandledChildXml entfernen)
      /// </summary>
      /// <param name="rote"></param>
      public static void SimplifyFormat(GpxRoute? route) {
         route?.UnhandledChildXml?.Clear();
      }

      #region interne Hilfsfunktionen

      /// <summary>
      /// liefert die Geschwindigkeit, den Abstand und die Zeit für den (direkten) Weg zwischen 2 Punkten
      /// <para>Die Geschwindigkeit kann sehr ungenau sein wenn die Trackpunkte nur wenige Meter 
      /// auseinanderliegen da die Zeitangaben nur volle Sekunden liefern! Manchmal ist die Zeitdiff. sogar 0.</para>
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="distance"></param>
      /// <param name="seconds"></param>
      /// <returns>double.MaxValue bei Zeitdiff. 0</returns>
      static double vst(GpxTrackPoint p1, GpxTrackPoint p2, out double distance, out double seconds) {
         distance = Math.Abs(GpxInfos.PointListInfo.PointDistance(p1, p2));
         seconds = p1.Time != BaseElement.NOTUSE_TIME &&
                   p1.Time != BaseElement.NOTVALID_TIME &&
                   p2.Time != BaseElement.NOTUSE_TIME &&
                   p2.Time != BaseElement.NOTVALID_TIME ?
                                 Math.Abs(p2.Time.Subtract(p1.Time).TotalSeconds) :
                                 0;
         return seconds > 0 ? distance / seconds :
                  distance > 0 ? double.MaxValue : 0;
      }

      ///// <summary>
      ///// liefert die Länge, die sich aus einem Teil der Punktliste ergibt
      ///// </summary>
      ///// <param name="ptlst"><see cref="gpxTrackPointExt"/>-Liste</param>
      ///// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      ///// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/> (i.A.. min. 2)</param>
      ///// <returns></returns>
      //static double getLength(IList<gpxTrackPointExt> pt, int startidx = 0, int count = -1) {
      //   count = Math.Min(count, pt.Count - startidx);
      //   if (count < 0)
      //      count = pt.Count - startidx;
      //   double length = 0;
      //   for (int p = startidx + 1; p < startidx + count; p++)
      //      length += GpxInfos.PointListInfo.PointDistance(pt[p - 1], pt[p]);
      //   return length;
      //}

      /// <summary>
      /// erzeugt eine Profilliste (kumulierte Entfernungen und Höhen)
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      static SimplificationPointList createProfileList(IList<GpxTrackPoint> pt) {
         SimplificationPointList profile = new(pt.Count);
         profile.Set(0, 0, pt[0].Elevation);
         for (int i = 1; i < profile.Length; i++)
            profile.Set(i,
                        profile.GetX(i - 1) + GpxInfos.PointListInfo.PointDistance(pt[i - 1], pt[i]),
                        pt[i].Elevation);
         return profile;
      }

      static SimplificationPointList createList4Simplification(IList<GpxTrackPoint> pt) {
         SimplificationPointList lst = new(pt.Count);
         lst.Set(0, 0, 0);
         lst.Set(0, true);
         for (int i = 1; i < lst.Length; i++) {
            GeoHelper.Wgs84ShortXYDelta(pt[i - 1].Lon, pt[i].Lon, pt[i - 1].Lat, pt[i].Lat, out double dx, out double dy);
            lst.Set(i,
                    lst.GetX(i - 1) + dx,
                    lst.GetY(i - 1) + dy);
         }
         return lst;
      }

      /// <summary>
      /// ermittelt den durchschnittlichen Anstieg bis zum Punkt mit dem Index 'start', max. aber für eine Weglänge 'width'
      /// <para>Voraussetzung ist, dass alle Höhen der Punkte vorher gültig sind</para>
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="start"></param>
      /// <param name="width"></param>
      /// <returns></returns>
      static double getMeanAscendBefore(List<gpxTrackPointExt> pt, int start, double width) {
         double meanascend = double.NaN;
         if (start > 0 &&
             width > 0) {
            double length = 0;
            double h_start = pt[start].Elevation;
            double dist = 0;
            int i;
            for (i = start; i > 0; i--) {
               dist = GpxInfos.PointListInfo.PointDistance(pt[i], pt[i - 1]); // Punktabstand
               length += dist;
               meanascend = (h_start - pt[i - 1].Elevation) / length; // Näherungswert
               if (length >= width)
                  break;
            }
            if (length > width && i > 0) {
               if (pt[i].Elevation != BaseElement.NOTVALID_DOUBLE &&
                   pt[i - 1].Elevation != BaseElement.NOTVALID_DOUBLE &&
                   dist > 0) { // Höhe auf letzter Teilstrecke interpolieren
                  double h = pt[i - 1].Elevation;
                  h += (length - width) / dist * (pt[i].Elevation - pt[i - 1].Elevation);
                  meanascend = (h_start - h) / width;
               }
            }
         }
         return meanascend;
      }

      #endregion

      #region ElevationFilter

      public class ElevationFilter {

         const double NOVALUE = double.MinValue;

         const int IDXX = 0;
         const int IDXY = 1;
         const int IDXFY = 2;


         enum AD {
            LinearInterpolate,
            NextNeighbor,

         }


         /// <summary>
         /// wendet ein Butterworth-Tiefpass 2. Ordnung auf die Höhen aller Segmente an und liefert eine neue <see cref="GpxAll"/>
         /// </summary>
         /// <param name="gpxorg"></param>
         /// <param name="freq">Grenzfrequenz</param>
         /// <param name="sr">Samplerate</param>
         /// <param name="delayfactor">angenomene Verzögerung des gefilterten Signals (exper.: 0.023 * sr /freq)</param>
         /// <param name="fractionadigits">Anzahl Nachkommastellen für die Höhe</param>
         /// <param name="exportfile"></param>
         /// <returns></returns>
         public static GpxAll Butterworth2Filter(GpxAll gpxorg,
                                                 double freq = 0.0016,
                                                 double sr = 10,
                                                 double delayfactor = 0.023,
                                                 int fractionadigits = 0,
                                                 string? exportfile = null) {
            GpxAll gpxnew = new(gpxorg.AsXml(999));

            for (int t = 0; t < gpxorg.Tracks.Count; t++) {

               GpxTrack track = gpxorg.Tracks[t];
               for (int s = 0; s < track.Segments.Count; s++) {
                  GpxTrackSegment? segment = track.GetSegment(s);
                  if (segment != null && segment.Points.Count > 2) {
                     GpxTrackSegment? newsegment = gpxnew.Tracks[t].GetSegment(s);
                     if (newsegment != null) {
                        ListTS<GpxTrackPoint>? destpoints = newsegment.Points;
                        if (destpoints != null)
                           Butterworth2Filter(segment.Points,
                                              destpoints,
                                              freq,
                                              sr,
                                              delayfactor,
                                              fractionadigits,
                                              exportfile);
                     }
                  }
               }
            }
            return gpxnew;
         }

         /// <summary>
         /// wendet ein Butterworth-Tiefpass 2. Ordnung auf die Höhen aller Segmente an und liefert eine 
         /// neue <see cref="ListTS"/> mit <see cref="GpxTrackPoint"/>
         /// </summary>
         /// <param name="ptlst"></param>
         /// <param name="destptlst">muss (min.) genauso viele Punkte haben wie die Ausgangsdaten; es wird nur die Höhe angepasst</param>
         /// <param name="freq">Grenzfrequenz</param>
         /// <param name="sr">Samplerate</param>
         /// <param name="delayfactor">angenomene Verzögerung des gefilterten Signals (exper.: 0.023 * sr /freq)</param>
         /// <param name="fractionadigits">Anzahl Nachkommastellen für die Höhe</param>
         /// <param name="exportfile"></param>
         /// <returns>Anzahl der geänderten Höhen</returns>
         public static int Butterworth2Filter(ListTS<GpxTrackPoint> ptlst,
                                              ListTS<GpxTrackPoint> destptlst,
                                              double freq = 0.0016,
                                              double sr = 10,
                                              double delayfactor = 0.023,
                                              int fractionadigits = 0,
                                              string? exportfile = null) {
            if (ptlst.Count > 2) {
               double delay = delayfactor * sr / freq;
               double[,] orgdata = buildDataArray4Segment(ptlst, delay, out double startele);

               Butterworth2Filter(orgdata, freq, sr, delayfactor, fractionadigits);

               if (!string.IsNullOrEmpty(exportfile)) {
                  StringBuilder sb = new();
                  sb.AppendLine("x\ty\tfy");
                  for (int i = 0; i < rowsOfArray(orgdata); i++)
                     sb.AppendLine(orgdata[i, IDXX] + "\t" + orgdata[i, IDXY] + "\t" + orgdata[i, IDXFY]);
                  File.WriteAllText(exportfile, sb.ToString());
               }

               return setElevation4Segment(destptlst, orgdata, startele);
            }
            return -1;
         }

         /// <summary>
         /// wendet ein Butterworth-Tiefpass 2. Ordnung auf auf die Daten der ersten beiden Spalten an unf trägt das ewrgebnis in
         /// die 3. Spalte ein
         /// </summary>
         /// <param name="orgdata"></param>
         /// <param name="freq">Grenzfrequenz</param>
         /// <param name="sr">Samplerate</param>
         /// <param name="delayfactor">angenomene Verzögerung des gefilterten Signals (exper.: 0.023 * sr /freq)</param>
         /// <param name="fractionadigits">Anzahl Nachkommastellen für die Höhe</param>
         /// <exception cref="ArgumentException"></exception>
         public static void Butterworth2Filter(double[,] orgdata,
                                               double freq,
                                               double sr,
                                               double delayfactor,
                                               int fractionadigits) {
            if (orgdata.Rank < 2 ||
                orgdata.GetLength(1) < IDXFY + 1)
               throw new ArgumentException("Das Array muss min. 2 Dimensionen haben und " + (IDXFY + 1) + " Spalten haben.", nameof(orgdata));
            appendFilteredValues(orgdata, freq, sr, delayfactor * sr / freq, fractionadigits);
         }

         static double[,] buildDataArray4Segment(ListTS<GpxTrackPoint> ptlst, double delay, out double startele) {
            DateTime starttime = ptlst[0].Time;
            startele = ptlst[0].Elevation;
            bool is4time = starttime != BaseElement.NOTVALID_TIME;
            double[,] orgdata = new double[ptlst.Count + (delay > 0 ? 1 : 0), IDXFY + 1];

            for (int p = 0; p < ptlst.Count; p++) {
               double x;
               if (is4time) {
                  x = ptlst[p].Time != BaseElement.NOTVALID_TIME ?
                              ptlst[p].Time.Subtract(starttime).TotalSeconds :
                              NOVALUE;
               } else {
                  double dstep = p == 0 ?
                                       0 :
                                       GeoHelper.Wgs84Distance(ptlst[p].Lon,
                                                               ptlst[p - 1].Lon,
                                                               ptlst[p].Lat,
                                                               ptlst[p - 1].Lat,
                                                               GeoHelper.Wgs84DistanceCompute.ellipsoid);
                  x = p == 0 ?
                        0 :
                        orgdata[p - 1, IDXY] + dstep;
               }
               orgdata[p, IDXX] = x;
               orgdata[p, IDXY] = ptlst[p].Elevation != BaseElement.NOTVALID_DOUBLE ?
                                       ptlst[p].Elevation - startele :
                                       NOVALUE;
            }

            if (delay > 0) {
               orgdata[ptlst.Count, IDXX] = orgdata[ptlst.Count - 1, IDXX] + delay;
               orgdata[ptlst.Count, IDXY] = orgdata[ptlst.Count - 1, IDXY];
            }

            return orgdata;
         }

         /// <summary>
         /// ergänzt in der Spalte <see cref="IDXFY"/> die gefilterten Werte zu den Spalten <see cref="IDXX"/> / <see cref="IDXY"/>
         /// </summary>
         /// <param name="realdata"></param>
         /// <param name="freq">Filterfrequenz</param>
         /// <param name="sr">"Samplerate"</param>
         /// <param name="delay">angenomene Verzögerung des gefilterten Signals</param>
         /// <param name="fractionadigits">Anzahl Nachkommastellen für die Höhe</param>
         /// <exception cref="ArgumentException"></exception>
         static void appendFilteredValues(double[,] realdata,
                                          double freq,
                                          double sr,
                                          double delay,
                                          int fractionadigits) {
            if (realdata.Rank < 2 ||
                realdata.GetLength(1) < IDXFY + 1)
               throw new ArgumentException("Das Array muss min. 2 Dimensionen haben und " + (IDXFY + 1) + " Spalten haben.", nameof(realdata));

            double[,] ddata = getDigitizedAndFilteredData(realdata, freq, sr);

            double lasty = 0;
            for (int i = 0; i < rowsOfArray(realdata); i++) {
               if (isValidRow(realdata, i)) {
                  double x = realdata[i, IDXX];
                  if (delay > 0)
                     x += delay;
                  double y = getPolylineValue(x, ddata, IDXFY);
                  double newy = Math.Round(y != NOVALUE ?
                                                y :
                                                realdata[i, IDXY] != NOVALUE ?
                                                   realdata[i, IDXY] :
                                                   lasty,
                                           fractionadigits);
                  lasty = newy;
                  realdata[i, IDXFY] = newy;
               }
            }
         }

         /// <summary>
         /// "digitalisiert" und filtert die realen Werte und liefert das Ergebnis
         /// </summary>
         /// <param name="realdata"></param>
         /// <param name="digitizeinterval">Digitalisierungsintervall</param>
         /// <param name="freq">Filterfrequenz</param>
         /// <param name="sr">"Samplerate"</param>
         /// <returns></returns>
         static double[,] getDigitizedAndFilteredData(double[,] realdata,
                                                      double freq,
                                                      double sr) {
            // mit Q = 1 / Sqrt(2) Butterworth-Filter 2. Ordnung (40 dB je Dekade)
            BiQuadFilter filter = new(BiQuadFilter.FilterTypes.LPF,
                                      freq,
                                      sr,
                                      1 / Math.Sqrt(2));

            double[,] ddata = getDigitizedData(realdata, 1.0 / sr, AD.NextNeighbor);
            for (int i = 0; i < rowsOfArray(ddata); i++)
               ddata[i, IDXFY] = filter.ComputeSample((float)ddata[i, IDXY]);

            return ddata;
         }

         static int setElevation4Segment(ListTS<GpxTrackPoint> ptlst, double[,] orgdata, double startele) {
            int count = 0;
            for (int p = 0; p < ptlst.Count && p < rowsOfArray(orgdata); p++)
               if (orgdata[p, IDXX] != NOVALUE &&
                   orgdata[p, IDXFY] != NOVALUE) {
                  double ele = startele + orgdata[p, IDXFY];
                  if (ptlst[p].Elevation != ele) {
                     count++;
                     ptlst[p].Elevation = ele;
                  }
               }
            return count;
         }

         /// <summary>
         /// erzeugt aus den Realdaten interpolierte Daten für ein konstantes Intervall (das deutlich kleiner als
         /// der Abstand zweier Realwerte sein sollte) ("AD-Wandler")
         /// <para>Das 2-dim. Array hat 3 Spalten. Nur die ersten beiden Spalten sind gesetzt.</para>
         /// </summary>
         /// <param name="realdata"></param>
         /// <param name="digitizeinterval">Digitalisierungsintervall</param>
         /// <param name="ad">Interpolationsart</param>
         /// <returns></returns>
         static double[,] getDigitizedData(double[,] realdata,
                                           double digitizeinterval,
                                           AD adtype = AD.LinearInterpolate) {
            double maxx = realdata[rowsOfArray(realdata) - 1, IDXX];
            double[,] digitizeddata = new double[(int)(maxx / digitizeinterval), IDXFY + 1];
            for (int i = 0; i < rowsOfArray(digitizeddata); i++) {
               digitizeddata[i, IDXX] = i * digitizeinterval;
               double y = getInterpolatedValue(realdata,
                                               digitizeddata[i, IDXX],
                                               IDXY,
                                               adtype);
               if (y == NOVALUE) {
                  y = i > 0 ? digitizeddata[i - 1, IDXY] : 0;
               }
               digitizeddata[i, IDXY] = y;
            }
            return digitizeddata;
         }

         /// <summary>
         /// liefert einen interpolierten Wert zu x
         /// </summary>
         /// <param name="data">Datenarray</param>
         /// <param name="x"></param>
         /// <param name="yidx">Spaltenindex für y</param>
         /// <param name="ad">Interpolationsart</param>
         /// <returns></returns>
         static double getInterpolatedValue(double[,] data, double x, int yidx, AD? ad) {
            int idx;
            switch (ad) {
               case AD.NextNeighbor:
                  idx = getValidIntervalIndexPre(getIntervalIndex(x, data), data);
                  if (0 <= idx) {
                     if (idx == rowsOfArray(data) - 1)
                        return data[idx, yidx];
                     else {
                        int nextidx = getValidIntervalIndexPost(idx + 1, data);
                        if (idx < nextidx)
                           return x - data[idx, IDXX] < data[nextidx, IDXX] - x ?   // Wer liegt näher?
                                       data[idx, yidx] :
                                       data[nextidx, yidx];
                        else
                           return data[idx, yidx];
                     }
                  }
                  return NOVALUE;

               case AD.LinearInterpolate:
               default:
                  return getPolylineValue(x, data, yidx);
            }
         }

         static bool isValidRow(double[,] a, int idx) => a[idx, IDXX] != NOVALUE && a[idx, IDXY] != NOVALUE;

         static int getValidIntervalIndexPre(int idx, double[,] data) {
            for (int i = idx; i >= 0; i--)
               if (isValidRow(data, i))
                  return i;
            return -1;
         }

         static int getValidIntervalIndexPost(int idx, double[,] data) {
            for (int i = idx; i < rowsOfArray(data); i++)
               if (isValidRow(data, i))
                  return i;
            return -1;
         }

         /// <summary>
         /// liefert den kleinsten Index der (X,Y)-Paare für das X<=x gilt
         /// </summary>
         /// <param name="x"></param>
         /// <param name="data"></param>
         /// <returns></returns>
         static int getIntervalIndex(double x, double[,] data) => _getIntervalIndex(x, data, 0, rowsOfArray(data) - 1);

         /// <summary>
         /// Hilfsfunktion für <see cref="getIntervalIndex"/> zur binären Suche
         /// </summary>
         /// <param name="x"></param>
         /// <param name="data"></param>
         /// <param name="startidx"></param>
         /// <param name="lastidx"></param>
         /// <returns></returns>
         static int _getIntervalIndex(double x, double[,] data, int startidx, int lastidx) {
            if (x < data[startidx, IDXX] ||
                data[lastidx, IDXX] < x)
               return -1;
            else {
               if (lastidx - startidx > 1) {
                  int idx = startidx + (lastidx - startidx) / 2;
                  int result = _getIntervalIndex(x, data, startidx, idx);
                  if (result >= 0)
                     return result;
                  else
                     return _getIntervalIndex(x, data, idx, lastidx);
               } else
                  return startidx;
            }
         }

         /// <summary>
         /// liefert den interpolierten Wert (oder NODATA) zu x aus der Liste der (x, y)-Werte
         /// </summary>
         /// <param name="x"></param>
         /// <param name="data">Datenarray</param>
         /// <param name="yidx">Spaltenindex für y</param>
         /// <returns></returns>
         static double getPolylineValue(double x, double[,] data, int yidx) {
            int idx = getValidIntervalIndexPre(getIntervalIndex(x, data), data);
            if (0 <= idx) {
               int nextidx = getValidIntervalIndexPost(idx + 1, data);
               if (idx < nextidx) {
                  // Wert interpolieren
                  double x1 = data[idx, IDXX];
                  double x2 = data[nextidx, IDXX];
                  double y1 = data[idx, yidx];
                  double y2 = data[nextidx, yidx];
                  if (y1 != double.MinValue && y2 != double.MinValue)
                     return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
               }
            }
            return NOVALUE;
         }

         static int rowsOfArray(double[,] a) => a.GetLength(0);

      }

      #endregion

      #region Biquad-Filter

      public class BiQuadFilter {

         /*
            Die Flankensteilheit beschreibt den Abfall des Signalpegels im Arbeitsbereich eines Filters in Dezibel (dB) pro Oktave. 
            Diese Steilheit ist von der Filterordnung abhängig.
            Ein einfaches Analog-Filter hat eine Güte von 6db pro Oktave. Man spricht dabei auch von einem 1-Pol-Filter.

            Im Audiobereich sind folgende Filterschaltungen üblich:

             6 db/Oktave   20db/Dekade 1-Pol-Filter oder 6db-Filter    Filter 1. Ordung
            12 db/Oktave   40db/Dekade 2-Pol-Filter oder 12db-Filter   Filter 2. Ordung
            18 db/Oktave   60db/Dekade 3-Pol-Filter oder 18db-Filter   Filter 3. Ordung
            24 db/Oktave   80db/Dekade 4-Pol-Filter oder 24db-Filter   Filter 4. Ordung
         */

         protected struct BiquadParams {
            public double b0, b1, b2,
                          a1, a2,
                          x1, x2, y1, y2;
         }

         /// <summary>
         /// Filtertypen
         /// </summary>
         public enum FilterTypes {
            /// <summary>
            /// Tiefpass 
            /// </summary>
            LPF,
            /// <summary>
            /// Hochpass
            /// </summary>
            HPF,
            /// <summary>
            /// Bandpass/Kerbfilter (Glockenfilter) -> Peaking band
            /// </summary>
            PEQ,
            /// <summary>
            /// Bandpass
            /// </summary>
            BPF,
            /// <summary>
            /// Bandpass (Peak = Q)
            /// </summary>
            BPF2,
            /// <summary>
            /// Kerbfilter (Notch)
            /// </summary>
            NOTCH,
            /// <summary>
            /// Allpass
            /// </summary>
            APF,
            /// <summary>
            /// Tiefpass (Kuhschwanz) -> shelf (fangen erst mit leichter Wirkung an, die sich dann verstärkt)
            /// </summary>
            LSH,
            /// <summary>
            /// Hochpass (Kuhschwanz) -> shelf (fangen erst mit leichter Wirkung an, die sich dann verstärkt)
            /// </summary>
            HSH
         }

         public double FilterFreq { get; private set; }

         public double SamplingRate { get; private set; }

         public double Quality { get; private set; }

         public double BandWidth { get; private set; }

         public double Shelf { get; private set; }

         public double Gain { get; private set; }


         protected BiquadParams filterparam;
         protected FilterTypes type;


         /// <summary>
         /// erzeugt ein BiQuad-Filter
         /// <para>
         /// I.A. genügt die Angabe von Filterfrequenz, Samplingfrequenz und Q. Wird eine Bandbreite benötigt, kann diese auch
         /// mit <see cref="Bandwidth2Q(double, double, double)"/> in Q umgerechnet werden. Die Verstärkung wird nur für die 
         /// Filter <see cref="FilterTypes.PEQ"/>, <see cref="FilterTypes.HSH"/> und <see cref="FilterTypes.LSH"/> verwendet.
         /// </para>
         /// </summary>
         /// <param name="type">Filtertyp</param>
         /// <param name="filterFreq">Filterfrequenz (Bedeutung je nach Filtertyp)</param>
         /// <param name="samplingRate">Samplingfrequenz</param>
         /// <param name="Q">Filtergüte</param>
         /// <param name="bandWidth">Bandbreite in Oktaven für -3dB bei BPF und NOTCH oder 1/2 Verstärkung für PEQ</param>
         /// <param name="shelf">Parameter für "Kuhschwanzfilter"</param>
         /// <param name="dbGain">Verstärkung in dB</param>
         public BiQuadFilter(FilterTypes type,
                             double filterFreq,
                             double samplingRate,
                             double Q = 0.70710678,
                             double bandWidth = 0,
                             double shelf = 0,
                             double dbGain = 0) {
            this.type = type;
            FilterFreq = filterFreq;
            SamplingRate = samplingRate;
            Quality = Q;
            BandWidth = bandWidth;
            Shelf = shelf;
            Gain = dbGain;
            init(type, filterFreq, samplingRate, Q, bandWidth, shelf, dbGain);
         }

         public BiQuadFilter(BiQuadFilter bq) {
            filterparam = bq.filterparam;
            type = bq.type;
            filterparam = new BiquadParams() {
               b0 = bq.filterparam.b0,
               b1 = bq.filterparam.b1,
               b2 = bq.filterparam.b2,
               a1 = bq.filterparam.a1,
               a2 = bq.filterparam.a2,
               x1 = bq.filterparam.x1,
               x2 = bq.filterparam.x2,
               y1 = bq.filterparam.y1,
               y2 = bq.filterparam.y2,
            };
            FilterFreq = bq.FilterFreq;
            SamplingRate = bq.SamplingRate;
            Quality = bq.Quality;
            BandWidth = bq.BandWidth;
            Shelf = bq.Shelf;
            Gain = bq.Gain;
         }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="type"></param>
         /// <param name="f0">Center frequency or corner frequency or shelf midpoint frequency, depending on which type of filters you use</param>
         /// <param name="Fs">Sampling frequency</param>
         /// <param name="Q">Quality factor (except in peaking EQ, A·Q is the qualify factor)</param>
         /// <param name="BW">Bandwidth in octaves</param>
         /// <param name="S">Shelf slope (for shelving EQ only)</param>
         /// <param name="dbGain">(only for peaking and shelving filters)</param>
         void init(FilterTypes? type,
                   double f0,
                   double Fs,
                   double Q,
                   double BW = 0,
                   double S = 0,
                   double dbGain = 0) {
            double omega = 2 * Math.PI * f0 / Fs;
            double sinomega = Math.Sin(omega);
            double cosomega = Math.Cos(omega);

            /*
       dBgain     used only for peaking and shelving filters

       Q    the EE kind of definition, except for peakingEQ in which A*Q is the classic EE Q.  
            That adjustment in definition was made so that a boost of N dB followed by a cut of N dB for identical Q and
            f0/Fs results in a precisely flat unity gain filter or "wire".

       _or_ BW,   the bandwidth in octaves (between -3 dB frequencies for BPF and notch or between 
                  midpoint (dBgain/2) gain frequencies for peaking EQ

       _or_ S,    a "shelf slope" parameter (for shelving EQ only).  When S = 1, the shelf slope is as steep as it can be 
                  and remain monotonically increasing or decreasing gain with frequency.  
                  The shelf slope, in dB/octave, remains proportional to S for all other values for a fixed f0/Fs and dBgain.
             */
            double A = Math.Pow(10.0, dbGain / 40);
            double alpha = 0;
            if (Q != 0)
               alpha = sinomega / (2 * Q);
            else if (BW != 0)
               alpha = sinomega * Math.Sinh(Math.Log(2) / 2 * BW * omega / sinomega);
            else if (S != 0) {
               alpha = sinomega / 2 * Math.Sqrt((A + 1 / A) + (1 / S - 1) + 2);
            }

            /*
            sinomega / (2 * Q) = sinomega * Math.Sinh(Math.Log(2) / 2 * BW * omega / sinomega)
            1 = 2 * Q * Math.Sinh(Math.Log(2) / 2 * BW * omega / sinomega)
            Q = 1 / (2 * Math.Sinh(Math.Log(2) / 2 * BW * omega / sinomega))
            Q = 1 / (2 * Math.Sinh(Math.Log(2) / 2 * BW * omega / Math.Sin(omega)))
            Q = 1 / (2 * Math.Sinh(Math.Log(2) / 2 * BW * 2 * Math.PI * f0 / Fs / Math.Sin(2 * Math.PI * f0 / Fs)))
             */


            double _b0, _b1, _b2, _a0, _a1, _a2;
            switch (type) {
               case FilterTypes.LPF:      // 2nd Order Low Pass Filter: H(s) = 1 / (s^2 + s/Q + 1)
                                          //           b0 =  (1 - cos(w0))/2
                                          //           b1 =   1 - cos(w0)
                                          //           b2 =  (1 - cos(w0))/2
                                          //           a0 =   1 + alpha
                                          //           a1 =  -2*cos(w0)
                                          //           a2 =   1 - alpha
                  _b0 = (1 - cosomega) / 2;
                  _b1 = 1 - cosomega;
                  _b2 = (1 - cosomega) / 2;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.HPF:      // 2nd Order High Pass Filter: H(s) = s^2 / (s^2 + s/Q + 1)
                                          //            b0 =  (1 + cos(w0))/2
                                          //            b1 = -(1 + cos(w0))
                                          //            b2 =  (1 + cos(w0))/2
                                          //            a0 =   1 + alpha
                                          //            a1 =  -2*cos(w0)
                                          //            a2 =   1 - alpha
                  _b0 = (1 + cosomega) / 2;
                  _b1 = -(1 + cosomega);
                  _b2 = (1 + cosomega) / 2;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.PEQ:         // 2nd Order Peaking EQ:  H(s) = (s^2 + s*(A/Q) + 1) / (s^2 + s/(A*Q) + 1)
                                             //            b0 =   1 + alpha*A
                                             //            b1 =  -2*cos(w0)
                                             //            b2 =   1 - alpha*A
                                             //            a0 =   1 + alpha/A
                                             //            a1 =  -2*cos(w0)
                                             //            a2 =   1 - alpha/A
                  _b0 = 1 + (alpha * A);
                  _b1 = -2 * cosomega;
                  _b2 = 1 - (alpha * A);
                  _a0 = 1 + (alpha / A);
                  _a1 = -2 * cosomega;
                  _a2 = 1 - (alpha / A);
                  break;

               case FilterTypes.BPF:         // BPF: H(s) = (s/Q) / (s^2 + s/Q + 1)      (constant 0 dB peak gain)
                                             //            b0 =   alpha
                                             //            b1 =   0
                                             //            b2 =  -alpha
                                             //            a0 =   1 + alpha
                                             //            a1 =  -2*cos(w0)
                                             //            a2 =   1 - alpha
                  _b0 = alpha;
                  _b1 = 0;
                  _b2 = -alpha;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.BPF2:        // BPF: H(s) = s / (s^2 + s/Q + 1)  (constant skirt gain, peak gain = Q)
                                             //            b0 =   sin(w0)/2  =   Q*alpha
                                             //            b1 =   0
                                             //            b2 =  -sin(w0)/2  =  -Q*alpha
                                             //            a0 =   1 + alpha
                                             //            a1 =  -2*cos(w0)
                                             //            a2 =   1 - alpha
                  _b0 = alpha * Q;
                  _b1 = 0;
                  _b2 = -alpha * Q;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.NOTCH:       // notch: H(s) = (s^2 + 1) / (s^2 + s/Q + 1)
                                             //            b0 =   1
                                             //            b1 =  -2*cos(w0)
                                             //            b2 =   1
                                             //            a0 =   1 + alpha
                                             //            a1 =  -2*cos(w0)
                                             //            a2 =   1 - alpha
                  _b0 = 1;
                  _b1 = -2 * cosomega;
                  _b2 = 1;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.APF:         // APF: H(s) = (s^2 - s/Q + 1) / (s^2 + s/Q + 1)
                                             //            b0 =   1 - alpha
                                             //            b1 =  -2*cos(w0)
                                             //            b2 =   1 + alpha
                                             //            a0 =   1 + alpha
                                             //            a1 =  -2*cos(w0)
                                             //            a2 =   1 - alpha
                  _b0 = 1 - alpha;
                  _b1 = -2 * cosomega;
                  _b2 = 1 + alpha;
                  _a0 = 1 + alpha;
                  _a1 = -2 * cosomega;
                  _a2 = 1 - alpha;
                  break;

               case FilterTypes.LSH: {
                     //lowShelf: H(s) = A * (s^2 + (sqrt(A)/Q)*s + A)/(A*s^2 + (sqrt(A)/Q)*s + 1)
                     //            b0 =    A*( (A+1) - (A-1)*cos(w0) + 2*sqrt(A)*alpha )
                     //            b1 =  2*A*( (A-1) - (A+1)*cos(w0)                   )
                     //            b2 =    A*( (A+1) - (A-1)*cos(w0) - 2*sqrt(A)*alpha )
                     //            a0 =        (A+1) + (A-1)*cos(w0) + 2*sqrt(A)*alpha
                     //            a1 =   -2*( (A-1) + (A+1)*cos(w0)                   )
                     //            a2 =        (A+1) + (A-1)*cos(w0) - 2*sqrt(A)*alpha

                     // sqrt(2*A)*sinomega  <-->  2*sqrt(A)*alpha
                     double beta = Math.Sqrt(A + A);
                     _b0 = A * (A + 1 - (A - 1) * cosomega + beta * sinomega);
                     _b1 = 2 * A * (A - 1 - (A + 1) * cosomega);
                     _b2 = A * (A + 1 - (A - 1) * cosomega - beta * sinomega);
                     _a0 = A + 1 + (A - 1) * cosomega + beta * sinomega;
                     _a1 = -2 * (A - 1 + (A + 1) * cosomega);
                     _a2 = A + 1 + (A - 1) * cosomega - beta * sinomega;
                  }
                  break;

               case FilterTypes.HSH: {
                     //highShelf: H(s) = A * (A*s^2 + (sqrt(A)/Q)*s + 1)/(s^2 + (sqrt(A)/Q)*s + A)
                     //            b0 =    A*( (A+1) + (A-1)*cos(w0) + 2*sqrt(A)*alpha )
                     //            b1 = -2*A*( (A-1) + (A+1)*cos(w0)                   )
                     //            b2 =    A*( (A+1) + (A-1)*cos(w0) - 2*sqrt(A)*alpha )
                     //            a0 =        (A+1) - (A-1)*cos(w0) + 2*sqrt(A)*alpha
                     //            a1 =    2*( (A-1) - (A+1)*cos(w0)                   )
                     //            a2 =        (A+1) - (A-1)*cos(w0) - 2*sqrt(A)*alpha

                     // sqrt(2*A)*sinomega  <-->  2*sqrt(A)*alpha
                     double beta = Math.Sqrt(A + A);
                     _b0 = A * ((A + 1) + (A - 1) * cosomega + beta * sinomega);
                     _b1 = -2 * A * ((A - 1) + (A + 1) * cosomega);
                     _b2 = A * ((A + 1) + (A - 1) * cosomega - beta * sinomega);
                     _a0 = (A + 1) - (A - 1) * cosomega + beta * sinomega;
                     _a1 = 2 * ((A - 1) - (A + 1) * cosomega);
                     _a2 = (A + 1) - (A - 1) * cosomega - beta * sinomega;
                  }
                  break;

               default:
                  throw new Exception("Unbekannter Filtertyp!");
            }

            filterparam = new BiquadParams() {
               // Normierung auf a0=1
               b0 = _b0 / _a0,
               b1 = _b1 / _a0,
               b2 = _b2 / _a0,
               a1 = _a1 / _a0,
               a2 = _a2 / _a0,
               x1 = 0,
               x2 = 0,
               y1 = 0,
               y2 = 0,
            };
         }

         /// <summary>
         /// liefert Q für eine gewünschte Bandbreite
         /// </summary>
         /// <param name="bandwidth"></param>
         /// <param name="f0"></param>
         /// <param name="Fs"></param>
         /// <returns></returns>
         public static double Bandwidth2Q(double bandwidth,
                                          double f0,
                                          double Fs) {
            double omega = 2 * Math.PI * f0 / Fs;
            double sinomega = Math.Sin(omega);
            return 1 / (2 * Math.Sinh(Math.Log(2) / 2 * bandwidth * omega / sinomega)); ;
         }



         public static BiQuadFilter CreateFilterLP(double f, double samplefreq, double q) =>
            new(FilterTypes.LPF, f, samplefreq, q);

         public static BiQuadFilter CreateFilterHP(double f, double samplefreq, double q) =>
            new(FilterTypes.HPF, f, samplefreq, q);


         /// <summary>
         /// wendet den Filter auf ein Sample an
         /// </summary>
         /// <param name="sample">Input-Sample</param>
         /// <returns>Output-Sample</returns>
         public double ComputeSample(double sample) {
            double result = filterparam.b0 * sample +
                            filterparam.b1 * filterparam.x1 +
                            filterparam.b2 * filterparam.x2 -
                            filterparam.a1 * filterparam.y1 -
                            filterparam.a2 * filterparam.y2;
            // shift x1 to x2, sample to x1
            filterparam.x2 = filterparam.x1;
            filterparam.x1 = sample;
            // shift y1 to y2, result to y1
            filterparam.y2 = filterparam.y1;
            filterparam.y1 = result;
            return result;
         }

         /// <summary>
         /// Pegel für eine bestimmte Frequenz
         /// </summary>
         /// <param name="f"></param>
         /// <returns></returns>
         public double Level(double f, bool db = true) {
            double phi = Math.Pow(Math.Sin(2.0 * Math.PI * f / (2.0 * SamplingRate)), 2.0);
            double r = _levelhelper1(filterparam.b0, filterparam.b1, filterparam.b2, phi) /
                       _levelhelper1(1, filterparam.a1, filterparam.a2, phi);
            if (r < 0)
               r = 0;
            return db ?
               10 * Math.Log10(Math.Sqrt(r)) :
               Math.Sqrt(r);
         }

         static double _levelhelper1(double p0, double p1, double p2, double phi) =>
            k1(p0, p1, p2) -
            k2(p0, p1, p2) * phi +
            k3(p0, p1, p2) * phi * phi;

         static double k1(double p0, double p1, double p2) =>
            Math.Pow(p0 + p1 + p2, 2.0);

         static double k2(double p0, double p1, double p2) =>
            4.0 * (p0 * p1 + 4.0 * p0 * p2 + p1 * p2);

         static double k3(double p0, double p1, double p2) =>
               16.0 * p0 * p2;

         public double[] Level(double[] f, bool db = true) {
            double[] result = new double[f.Length];
            double k1b = k1(filterparam.b0, filterparam.b1, filterparam.b2);
            double k2b = k2(filterparam.b0, filterparam.b1, filterparam.b2);
            double k3b = k3(filterparam.b0, filterparam.b1, filterparam.b2);
            double k1a = k1(1, filterparam.a1, filterparam.a2);
            double k2a = k2(1, filterparam.a1, filterparam.a2);
            double k3a = k3(1, filterparam.a1, filterparam.a2);

            for (int i = 0; i < f.Length; i++) {
               double phi = Math.Pow(Math.Sin(Math.PI * f[i] / SamplingRate), 2.0);
               result[i] = (k1b - k2b * phi + k3b * phi * phi) /
                           (k1a - k2a * phi + k3a * phi * phi);
            }
            if (db)
               for (int i = 0; i < result.Length; i++)
                  result[i] = 10 * Math.Log10(result[i]);

            return result;
         }

         public override string ToString() {
            return string.Format("{0}: {1} Hz, {2}, {3} Okt", type, FilterFreq, Quality, BandWidth);
         }

      }

      #endregion

   }
}
