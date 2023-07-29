using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;

namespace FSofTUtils.Geography {

   /// <summary>
   /// diverse Hilfsfunktionen um ein Tracksegment (<see cref="GpxTrackPoint"/>-Liste) zu vereinfachen
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
         SlidingIntegral
      }


      class gpxTrackPointExt : GpxTrackPoint {
         public bool Changed = false;

         public gpxTrackPointExt() { }

         public gpxTrackPointExt(GpxTrackPoint p) {
            Lat = p.Lat;
            Lon = p.Lon;
            Elevation = p.Elevation;
            Time = p.Time;
         }

         public static List<gpxTrackPointExt> Convert(IList<GpxTrackPoint> ptlist) {
            List<gpxTrackPointExt> lst = new List<gpxTrackPointExt>(ptlist.Count);
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
            length += PointDistance(ptlst[p - 1], ptlst[p]);
         return length;
      }

      /// <summary>
      /// Punktabstand in m
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      public static double PointDistance(GpxTrackPoint pt1, GpxTrackPoint pt2) {
         return GeoHelper.Wgs84Distance(pt1.Lon, pt2.Lon, pt1.Lat, pt2.Lat, GeoHelper.Wgs84DistanceCompute.ellipsoid);
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
      public static int RemoveHeight(IList<GpxTrackPoint> ptlst, int startidx = 0, int count = -1)
         => SetHeight(ptlst, BaseElement.NOTVALID_DOUBLE, startidx, count);

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
      /// <param name="changedheight">Anzahl der <see cref="GpxTrackPoint"/> mit geänderter Höhe</param>
      /// <param name="changedtimestamp">Anzahl der <see cref="GpxTrackPoint"/> mit geändertem Zeitstempel</param>
      public static void GapFill(IList<GpxTrackPoint> ptlst, out int changedheight, out int changedtimestamp) {
         List<gpxTrackPointExt> lst = gpxTrackPointExt.Convert(ptlst);
         changedheight = interpolateHeigth(lst);
         changedtimestamp = interpolateTime(lst);
         if (0 < changedheight + changedtimestamp)
            for (int i = 0; i < lst.Count; i++)
               if (lst[i].Changed) {
                  ptlst[i].Elevation = lst[i].Elevation;
                  ptlst[i].Time = lst[i].Time;
               }
      }

      /// <summary>
      /// (horizontale) Trackvereinfachung
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="type">Art des Vereinfachungalgorithmus</param>
      /// <param name="width">Streifenbreite, innerhalb der die Vereinfachung erfolgt</param>
      /// <returns>Anzahl der entfernten Punkte</returns>
      public static int HorizontalSimplification(IList<GpxTrackPoint> ptlst, HSimplification type, double width) {
         int removed = 0;
         if (type != HSimplification.Nothing) {
            if (ptlst.Count > 2) {
               Geometry.PolylineSimplification.PointList pl = createList4Simplification(ptlst);
               pl.Get(pl.Length - 1).IsLocked = true;

               switch (type) {
                  case HSimplification.Reumann_Witkam:
                     pl.ReumannWitkam(width);
                     break;

                  case HSimplification.Douglas_Peucker:
                     pl.DouglasPeucker(width);
                     break;
               }

               removed = ptlst.Count;
               for (int p = pl.Length - 1; p > 0; p--)
                  if (!pl.Get(p).IsValid)
                     ptlst.RemoveAt(p);
               removed -= ptlst.Count;
            }
         }
         return removed;
      }

      /// <summary>
      /// (vertikale) Trackvereinfachung/Höhenglättung
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="type">Art des Vereinfachungalgorithmus</param>
      /// <param name="width">Parameter für den Vereinfachungalgorithmus (obsolet bei <see cref="VSimplification.SlidingMean"/>)</param>
      /// <returns>Anzahl der geänderten Höhen Punkte</returns>
      public static int VerticalSimplification(IList<GpxTrackPoint> ptlst, VSimplification type, double width) {
         int changed = 0;
         if (type != VSimplification.Nothing) {
            if (type == VSimplification.SlidingMean)
               width = Math.Max(2, (int)Math.Round(width));    // >= 2

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
            Geometry.PolylineSimplification.PointList profile = createProfileList(ptlst);
            switch (type) {
               case VSimplification.SlidingMean:
                  profile.HeigthProfileWithSlidingMean(Math.Max(3, (int)Math.Round(width)));
                  break;

               case VSimplification.SlidingIntegral:
                  profile.HeigthProfileWithSlidingIntegral(width);
                  break;
            }

            changed = 0;

            // Daten speichern
            for (int p = 0; p < profile.Length; p++) {
               double v = profile.Get(p).Y;
               if (ptlst[p].Elevation != v) {
                  ptlst[p].Elevation = v;
                  changed++;
               }
            }
         }
         return changed;
      }

      /// <summary>
      /// Entfernung von Punkten mit "Ausreißer"-Höhen (zu starker An-/Abstieg)
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="width">Untersuchungslänge des Wegstückes</param>
      /// <param name="maxascend">max. An-/Abstieg in Prozent (0..100)</param>
      /// <returns>Anzahl der geänderten An-/Abstiege (Höhen)</returns>
      public static int RemoveHeigthOutlier(IList<GpxTrackPoint> ptlst, double width, double maxascend) {
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
         for (int i = 1; i < lst.Count; i++) {
            double dist = PointDistance(lst[i - 1], lst[i]);
            if (maxascend < Math.Abs(lst[i].Elevation - lst[i - 1].Elevation) / dist) {
               double meanascend = getMeanAscendBefore(lst, i - 1, width);
               if (double.IsNaN(meanascend))
                  meanascend = 0;
               double meanelevation = lst[i - 1].Elevation + dist * meanascend; // wenn es mit dem bisher mittleren Anstieg weitergehen würde

               lst[i].Elevation -= (lst[i].Elevation - meanelevation) / 2; // auf 1/2 des zusätzl. Anstiegs abziehen -> "Ausreißer" wird gedämpft

               //lst[i].Elevation = lst[i - 1].Elevation + dist * meanascend;
               lst[i].Changed = true;
            }
         }

         // Daten übernehmen
         int changed = 0;
         for (int p = 0; p < lst.Count; p++)
            if (lst[p].Changed) {
               ptlst[p].Elevation = lst[p].Elevation;
               changed++;
            }

         return changed;
      }

      /// <summary>
      /// Entfernung von Punkten mit "Ausreißer"-Geschwindigkeiten
      /// </summary>
      /// <param name="ptlst"><see cref="GpxTrackPoint"/>-Liste</param>
      /// <param name="maxv">Maximalgeschwindigkeit in m/s</param>
      /// <returns>Anzahl der entfernten Punkte</returns>
      public static int RemoveSpeedOutlier(IList<GpxTrackPoint> ptlst, double maxv) {
         List<gpxTrackPointExt> lst = gpxTrackPointExt.Convert(ptlst);

         Dictionary<int, int> removed = new Dictionary<int, int>();
         int idxa = 0;
         for (int idxe = 1; idxe < lst.Count; idxe++) {
            double distance = Math.Abs(PointDistance(lst[idxe], lst[idxa]));
            double deltatime = lst[idxa].Time != BaseElement.NOTUSE_TIME ||
                               lst[idxa].Time != BaseElement.NOTVALID_TIME ||
                               lst[idxe].Time != BaseElement.NOTUSE_TIME ||
                               lst[idxe].Time != BaseElement.NOTVALID_TIME ?
                                    0 :
                                    Math.Abs(lst[idxe].Time.Subtract(lst[idxa].Time).TotalSeconds);
            double v = deltatime > 0 ? distance / deltatime : 0;

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
            ptlst.RemoveAt(p);

         return removed.Count;
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
      /// <returns>Anzahl der entfernten Punkte</returns>
      public static int RemoveRestingplace(IList<GpxTrackPoint> ptlst,
                                           int ptcount,
                                           int crossing1, double maxradius1, double minturnaround1,
                                           int crossing2, double maxradius2, double minturnaround2,
                                           string protfile = null) {
         int removed = 0;
         if (ptcount >= 3 &&
             crossing1 >= 0 && maxradius1 > 0 && minturnaround1 > 0 &&
             crossing2 > 0 && maxradius2 > 0 && minturnaround2 > 0) {

            Geometry.PolylineSimplification.PointList lst = createList4Simplification(ptlst);
            lst.RemoveRestingplace(ptcount, crossing1, maxradius1, minturnaround1, crossing2, maxradius2, minturnaround2, protfile);
            for (int p = lst.Length - 1; p > 0; p--)
               if (!lst.Get(p).IsValid) {
                  ptlst.RemoveAt(p);
                  removed++;
               }
         }
         return removed;
      }

      #region interne Hilfsfunktionen

      /// <summary>
      /// liefert die Länge, die sich aus einem Teil der Punktliste ergibt
      /// </summary>
      /// <param name="ptlst"><see cref="gpxTrackPointExt"/>-Liste</param>
      /// <param name="startidx">Index des 1. betroffenen <see cref="GpxTrackPoint"/></param>
      /// <param name="count">Anzahl der betroffenen <see cref="GpxTrackPoint"/> (i.A.. min. 2)</param>
      /// <returns></returns>
      static double getLength(IList<gpxTrackPointExt> pt, int startidx = 0, int count = -1) {
         count = Math.Min(count, pt.Count - startidx);
         if (count < 0)
            count = pt.Count - startidx;
         double length = 0;
         for (int p = startidx + 1; p < startidx + count; p++)
            length += PointDistance(pt[p - 1], pt[p]);
         return length;
      }

      /// <summary>
      /// erzeugt eine Profilliste (kumulierte Entfernungen und Höhen)
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      static Geometry.PolylineSimplification.PointList createProfileList(IList<GpxTrackPoint> pt) {
         Geometry.PolylineSimplification.PointList profile = new Geometry.PolylineSimplification.PointList(pt.Count);
         profile.Set(0, 0, pt[0].Elevation);
         for (int i = 1; i < profile.Length; i++)
            profile.Set(i,
                        profile.Get(i - 1).X + PointDistance(pt[i - 1], pt[i]),
                        pt[i].Elevation);
         return profile;
      }

      static Geometry.PolylineSimplification.PointList createList4Simplification(IList<GpxTrackPoint> pt) {
         Geometry.PolylineSimplification.PointList lst = new Geometry.PolylineSimplification.PointList(pt.Count);
         lst.Set(0, 0, 0);
         lst.Get(0).IsLocked = true;
         for (int i = 1; i < lst.Length; i++) {
            GeoHelper.Wgs84ShortXYDelta(pt[i - 1].Lon, pt[i].Lon, pt[i - 1].Lat, pt[i].Lat, out double dx, out double dy);
            lst.Set(i,
                    lst.Get(i - 1).X + dx,
                    lst.Get(i - 1).Y + dy);
         }
         return lst;
      }

      /// <summary>
      /// interpoliert unbekannte Höhen
      /// <para>Es wird einfach nur eine konstante Höhenänderung von Punkt zu Punkt angenommen. 
      /// Andere Varianten (z.B. Länge der Teilstrecken) scheinen auch nicht sinnvoller zu sein.</para>
      /// </summary>
      /// <param name="lst"></param>
      /// <returns>Anzahl der interpolierten Werte</returns>
      static int interpolateHeigth(IList<gpxTrackPointExt> ptlstext) {
         int changed = 0;
         for (int i = 0; i < ptlstext.Count; i++) {
            // Bereichsgrenzen ungültiger Höhen ermitteln
            if (ptlstext[i].Elevation == BaseElement.NOTVALID_DOUBLE) {
               int startidx = i;
               int endidx = ptlstext.Count - 1;
               for (int j = i; j < ptlstext.Count; j++) {
                  if (ptlstext[j].Elevation != BaseElement.NOTVALID_DOUBLE) {
                     i = j - 1;
                     endidx = j - 1;
                     break;
                  }
               }

               double height1 = BaseElement.NOTVALID_DOUBLE;
               double height2 = BaseElement.NOTVALID_DOUBLE;
               if (startidx > 0)
                  height1 = ptlstext[startidx - 1].Elevation;
               if (endidx < ptlstext.Count - 1)
                  height2 = ptlstext[endidx + 1].Elevation;

               if (height1 == BaseElement.NOTVALID_DOUBLE) {      // die ersten Punkte mit der ersten gültigen Höhe auffüllen (wenn vorhanden)
                  for (int k = startidx; k <= endidx; k++) {
                     ptlstext[k].Elevation = height2;
                     ptlstext[k].Changed = true;
                     changed++;
                  }
               } else
                  if (height2 == BaseElement.NOTVALID_DOUBLE) {   // die letzten Punkte mit der letzten gültigen Höhe auffüllen (wenn vorhanden)
                  for (int k = startidx; k <= endidx; k++) {
                     ptlstext[k].Elevation = height1;
                     ptlstext[k].Changed = true;
                     changed++;
                  }
               } else {                            // interpolieren
                                                   // Es wird einfach nur eine konstante Höhenänderung von Punkt zu Punkt angenommen.
                                                   // Andere Varianten (z.B. Länge der Teilstrecken) scheinen auch nicht sinnvoller zu sein.
                  double step = (height2 - height1) / (2 + endidx - startidx);
                  for (int k = startidx; k <= endidx; k++) {
                     ptlstext[k].Elevation = height1 + (k - startidx + 1) * step;
                     ptlstext[k].Changed = true;
                     changed++;
                  }
               }
            }
         }
         return changed;
      }

      /// <summary>
      /// interpoliert unbekannte Zeiten
      /// <para>Es wird jeweils eine konstante Geschwindigkeit angenommen, die sich aus 2 "Randpunkten" des Bereiches ergibt.</para>
      /// </summary>
      /// <param name="ptlstext"></param>
      /// <returns>Anzahl der interpolierten Werte</returns>
      static int interpolateTime(IList<gpxTrackPointExt> ptlstext) {
         int changed = 0;
         for (int i = 0; i < ptlstext.Count; i++) {
            // Bereichsgrenzen ungültiger Höhen ermitteln
            if (ptlstext[i].Time == BaseElement.NOTVALID_TIME) {
               int startidx = i;
               int endidx = ptlstext.Count - 1;
               for (int j = i; j < ptlstext.Count; j++) {
                  if (ptlstext[j].Time != BaseElement.NOTVALID_TIME) {
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
                  time1 = ptlstext[startidx - 2].Time;
                  time2 = ptlstext[startidx - 1].Time;
               } else
                  if (startidx > 0)
                  time2 = ptlstext[startidx - 1].Time;

               if (endidx < ptlstext.Count - 2) {
                  time3 = ptlstext[endidx + 1].Time;
                  time4 = ptlstext[endidx + 3].Time;
               } else
                  if (endidx < ptlstext.Count - 1)
                  time3 = ptlstext[endidx + 1].Time;

               double v = 0;              // Geschwindigkeit für die Interpolation
               if (time2 != BaseElement.NOTVALID_TIME &&
                   time3 != BaseElement.NOTVALID_TIME) {
                  // Geschwindigkeit aus den beiden begrenzenden Punkten mit gültiger Zeit bestimmen
                  double length = getLength(ptlstext, startidx - 1, endidx - startidx + 3);
                  double sec = time3.Subtract(time2).TotalSeconds;
                  if (length > 0 && sec > 0)
                     v = length / sec;
               } else
                  if (time1 != BaseElement.NOTVALID_TIME &&
                      time2 != BaseElement.NOTVALID_TIME) {
                  // Geschwindigkeit aus den beiden letzten Punkten mit gültiger Zeit bestimmen
                  double length = getLength(ptlstext, startidx - 1, 2);
                  double sec = time2.Subtract(time1).TotalSeconds;
                  if (length > 0 && sec > 0)
                     v = length / sec;
               } else
                     if (time3 != BaseElement.NOTVALID_TIME &&
                         time4 != BaseElement.NOTVALID_TIME) {
                  // Geschwindigkeit aus den beiden ersten Punkten mit gültiger Zeit bestimmen
                  double length = getLength(ptlstext, endidx + 1, 2);
                  double sec = time4.Subtract(time3).TotalSeconds;
                  if (length > 0 && sec > 0)
                     v = length / sec;
               }

               if (v > 0) {            // sonst ist keine Interpolation möglich 
                  if (time2 == BaseElement.NOTVALID_TIME) {        // Bereich am Anfang
                     ptlstext[startidx].Time = time2 = time3.AddSeconds(-getLength(ptlstext, 0, endidx + 2) / v);
                     ptlstext[startidx].Changed = true;
                     startidx++;
                     changed++;
                  }
                  double difflength = 0;
                  for (int k = startidx; k <= endidx; k++) {
                     difflength += getLength(ptlstext, k - 1, 2);
                     ptlstext[k].Time = time2.AddSeconds(difflength / v);
                     ptlstext[k].Changed = true;
                     changed++;
                  }
               } else {                // Wie??? Mehrere Punkte mit identischer Zeit scheinen sinnlos (?) zu sein.


               }

            }
         }
         return changed;
      }

      /// <summary>
      /// ermittelt den durchschnittlichen Anstieg bis zum Punkt mit dem Index 'start', max. aber für eine Länge 'width'
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
               dist = PointDistance(pt[i], pt[i - 1]); // Punktabstand
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

   }
}
