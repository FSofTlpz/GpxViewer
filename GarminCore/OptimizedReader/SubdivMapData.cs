using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace GarminCore.OptimizedReader {

   /// <summary>
   /// eine Teilkarte mit allen Objekten (entspricht einer <see cref="StdFile_RGN.SubdivData"/>)
   /// <para>alle Koordinaten in Grad</para>
   /// <para>Eine vollständige Pseudo-Garmin-Karte besteht aus einer Baumstruktur aus <see cref="SubdivMapData"/>-Objekten. Die Ebenen im
   /// Baum stehen für die Maplevel.</para>
   /// </summary>
   public class SubdivMapData : IDisposable {

      /// <summary>
      /// zusätzliche Daten für normale Punkte (aus der LBL-Datei)
      /// </summary>
      class PoiDataExt {

         public string? Text;
         public string Country;
         public string Region;
         public string? City;
         public string? Zip;
         public string Street;
         public string StreetNumber;
         //public string PhoneNumber;
         //public string ExitHighway;
         //public int ExitOffset;
         //public int ExitIndex;

         public PoiDataExt(StdFile_LBL.PointDataRecord data, StdFile_LBL lbl, StdFile_RGN rgn) {
            Text = Country = Region = City = Zip = Street = StreetNumber = "";
            //PhoneNumber = ExitHighway = "";
            //ExitOffset = ExitIndex = -1;

            if (data.TextOffset > 0)
               Text = lbl.GetText(data.TextOffset, true);

            if (data.ZipIsSet)
               Zip = lbl.GetText_FromZipList(data.ZipIndex - 1, true);


            if (data.CityIsSet) {
               StdFile_LBL.CityAndRegionOrCountryRecord cr = lbl.CityAndRegionOrCountryDataList[data.CityIndex - 1];
               City = cr.GetCityText(lbl, rgn, true);
               //if (cr.IsCountry)
               //   Country = cr.GetCountryText(lbl, true);
               //else
               //   Country = cr.GetRegionText(lbl, true);
            }

            /*
            if (data.StreetIsSet)
               Street = lbl.GetText(data.StreetOffset, true);

            if (data.StreetNumberIsSet)
               if (data.StreetNumberIsCoded)
                  StreetNumber = data.StreetNumber;
               else
                  StreetNumber = lbl.GetText(data.StreetNumberOffset, true);

            if (data.PhoneIsSet)
               if (data.PhoneNumberIsCoded)
                  StreetNumber = data.PhoneNumber;
               else
                  StreetNumber = lbl.GetText(data.PhoneNumberOffset, true);
            */

            /*
            if (data.ExitIsSet) {
               if (data.ExitIndexIsSet) {
                  StdFile_LBL.ExitRecord er = lbl.ExitList[data.ExitIndex];
                  ExitIndex = data.ExitIndex;

                  Debug.WriteLine("ExitIndex {0}; Direction {1}, Type {2}, Facilities {3}, LastFacilitie {4}, Text {5}",
                                    data.ExitIndex,
                                    er.Direction,
                                    er.Type,
                                    er.Facilities,
                                    er.LastFacilitie,
                                    lbl.GetText(er.TextOffsetInLBL, false));

               } else if (data.ExitHighwayIndex != 0xFFFF) {
                  if (0 < data.ExitHighwayIndex && data.ExitHighwayIndex <= lbl.HighwayWithExitList.Count)
                     ExitHighway = lbl.GetText(lbl.HighwayWithExitList[data.ExitHighwayIndex - 1].TextOffset, true);

                  else
                     Debug.WriteLine("ExitHighwayIndex {0}, ExitOffset {1} (LBL_File.ExitList.Count {2}, LBL_File.HighwayList.Count {3})",
                                       data.ExitHighwayIndex,
                                       data.ExitOffset,
                                       lbl.ExitList.Count,
                                       lbl.HighwayWithExitList.Count);

               } else if (data.ExitOffset != 0xFFFF) {
                  ExitOffset = data.ExitOffset;

                  Debug.WriteLine("ExitHighwayIndex {0}, ExitOffset {1} (LBL_File.ExitList.Count {2}, LBL_File.HighwayList.Count {3})",
                                    data.ExitHighwayIndex,
                                    data.ExitOffset,
                                    lbl.ExitList.Count,
                                    lbl.HighwayWithExitList.Count);

               } else
                  Debug.WriteLine("LblPoiData.ExitIsSet, aber wie?");
            }
            */

         }

         public PoiDataExt(PoiDataExt pd) {
            Text = pd.Text;
            Country = pd.Country;
            Region = pd.Region;
            City = pd.City;
            Zip = pd.Zip;
            Street = pd.Street;
            StreetNumber = pd.StreetNumber;
            //PhoneNumber = pd.PhoneNumber;
            //ExitHighway = pd.ExitHighway;
            //ExitOffset = pd.ExitOffset;
            //ExitIndex = pd.ExitIndex;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Text)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Text [{0}]", Text));
            }

            if (!string.IsNullOrEmpty(Country)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Country [{0}]", Country));
            }

            if (!string.IsNullOrEmpty(Region)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Region [{0}]", Region));
            }

            if (!string.IsNullOrEmpty(City)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("City [{0}]", City));
            }

            if (!string.IsNullOrEmpty(Zip)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Zip [{0}]", Zip));
            }

            if (!string.IsNullOrEmpty(Street)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Street [{0}]", Street));
            }

            if (!string.IsNullOrEmpty(StreetNumber)) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("StreetNo [{0}]", StreetNumber));
            }

            //if (!string.IsNullOrEmpty(PhoneNumber)) {
            //   if (sb.Length > 0)
            //      sb.Append(", ");
            //   sb.Append(string.Format("Phone [{0}]", PhoneNumber));
            //}

            //if (!string.IsNullOrEmpty(ExitHighway)) {
            //   if (sb.Length > 0)
            //      sb.Append(", ");
            //   sb.Append(string.Format("ExitHighway [{0}]", ExitHighway));
            //}

            //if (ExitOffset >= 0) {
            //   if (sb.Length > 0)
            //      sb.Append(", ");
            //   sb.Append(string.Format("ExitOffset [{0}]", ExitOffset));
            //}

            //if (ExitIndex >= 0) {
            //   if (sb.Length > 0)
            //      sb.Append(", ");
            //   sb.Append(string.Format("ExitIndex [{0}]", ExitIndex));
            //}

            return sb.ToString();
         }

      }

      /// <summary>
      /// zusätzliche Daten für normale Straßen (aus der NET-Datei)
      /// </summary>
      class RoadDataExt {

         public string? City;
         public string Zip;
         public List<string> Street;
         public uint RoadLength;

         public RoadDataExt(StdFile_NET.RoadData rd, StdFile_LBL lbl, StdFile_RGN rgn) {
            City = Zip = "";
            Street = new List<string>();
            if (rd.ZipIndex4Node != null)
               for (int side = 0; side < rd.ZipIndex4Node.Length; side++) {
                  if (rd.ZipIndex4Node[side]?.Count > 0) {
                     if (Zip.Length > 0)
                        Zip += " / ";

                     Zip += rd.GetZipText(lbl,
                                   side == 0 ?
                                         StdFile_NET.RoadData.Side.Left :
                                         StdFile_NET.RoadData.Side.Right,
                                   false);
                  }
               }

            if (rd.CityIndex4Node != null)
               for (int side = 0; side < rd.CityIndex4Node.Length; side++) {
                  if (rd.CityIndex4Node[side]?.Count > 0) {
                     // aus der NET-Datei
                     City = rd.GetCityText(lbl,
                                           rgn,
                                           side == 0 ?
                                                StdFile_NET.RoadData.Side.Left :
                                                StdFile_NET.RoadData.Side.Right,
                                           false);
                  }
               }

            for (int i = 0; i < rd.LabelInfo.Count; i++) {
               string? street = lbl.GetText(rd.LabelInfo[i], true);
               if (street != null)
                  Street.Add(street);
            }

            RoadLength = rd.RoadLength * 2;
         }

         public RoadDataExt(RoadDataExt rd) {
            City = rd.City;
            Zip = rd.Zip;
            Street = new List<string>();
            for (int i = 0; i < rd.Street.Count; i++) {
               string street = rd.Street[i];
               Street.Add(street);
            }

            RoadLength = rd.RoadLength;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder();

            if (City != null && City.Length > 0) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("City [{0}]", City));
            }

            if (Zip.Length > 0) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Zip [{0}]", Zip));
            }

            for (int i = 0; i < Street.Count; i++) {
               if (sb.Length > 0)
                  sb.Append(", ");
               sb.Append(string.Format("Street [{0}]", Street[i]));
            }

            return sb.ToString();
         }

      }

      /// <summary>
      /// umgrenzendes Rechteck
      /// </summary>
      public Bound? Bound { get; protected set; } = null;

      public GeoPoint[]? Points { get; protected set; } = null;

      public GeoPoly[]? Areas { get; protected set; } = null;

      public GeoPoly[]? Lines { get; protected set; } = null;


      public SubdivMapData() { }


      int listCount<T>(List<T>? lst) => lst != null ? lst.Count : 0;


      /// <summary>
      /// liest alle Daten der <see cref="SubdivMapData"/> (Subdiv) ein
      /// </summary>
      /// <param name="sdi"></param>
      /// <param name="sd"></param>
      /// <param name="coordbits">10 .. 24</param>
      /// <param name="lbl"></param>
      /// <param name="rgn"></param>
      /// <param name="net"></param>
      public void ReadData(StdFile_TRE.SubdivInfoBasic sdi,
                           StdFile_RGN.SubdivData sd,
                           int coordbits,
                           StdFile_LBL lbl,
                           StdFile_RGN rgn,
                           StdFile_NET net) {

         Bound = sdi.GetBound(coordbits);

         // ================ Polygone verarbeiten

         Areas = new GeoPoly[listCount(sd.AreaList) +
                             listCount(sd.ExtAreaList)];
         if (sd.AreaList != null && listCount(sd.AreaList) > 0)
            for (int i = 0; i < sd.AreaList.Count; i++)
               Areas[i] = getGeoPoly(sd.AreaList[i], coordbits, sdi.Center, lbl, rgn, net, false);

         if (sd.ExtAreaList != null)
            for (int i = 0, dest = listCount(sd.AreaList); i < sd.ExtAreaList.Count; i++, dest++)
               Areas[dest] = getGeoPoly(sd.ExtAreaList[i], coordbits, sdi.Center, lbl);

         // ================ Linien verarbeiten

         Lines = new GeoPoly[listCount(sd.LineList) +
                             listCount(sd.ExtLineList)];
         if (sd.LineList != null && listCount(sd.LineList) > 0)
            for (int i = 0; i < sd.LineList.Count; i++)
               Lines[i] = getGeoPoly(sd.LineList[i], coordbits, sdi.Center, lbl, rgn, net, false);

         if (sd.ExtLineList != null && listCount(sd.ExtLineList) > 0)
            for (int i = 0, dest = listCount(sd.LineList); i < sd.ExtLineList.Count; i++, dest++)
               Lines[dest] = getGeoPoly(sd.ExtLineList[i], coordbits, sdi.Center, lbl);

         for (int i = 0; i < Lines.Length; i++) {  // Höhenlinien: Feet -> Meter
            if (Lines[i].MainType == 0x20 ||
                Lines[i].MainType == 0x21 ||
                Lines[i].MainType == 0x22)
               if (!string.IsNullOrEmpty(Lines[i].Text))
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                  Lines[i].Text = getMeter4Feet(Lines[i].Text);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
         }

         // ================ Punkte verarbeiten

         Points = new GeoPoint[listCount(sd.PointList2) +
                               listCount(sd.PointList1) +
                               listCount(sd.ExtPointList)];
         if (sd.PointList2 != null && listCount(sd.PointList2) > 0)
            for (int i = 0, dest = 0; i < sd.PointList2.Count; i++, dest++) { // vor den "normalen" Punkten einlesen, damit der ev. Index-Verweise stimmen (z.B. für Exits)
               Points[dest] = getGeoPoint(sd.PointList2[i], coordbits, sdi.Center, 2, lbl, rgn, false);
            }

         if (sd.PointList1 != null && listCount(sd.PointList1) > 0)
            for (int i = 0, dest = listCount(sd.PointList2); i < sd.PointList1.Count; i++, dest++) {
               Points[dest] = getGeoPoint(sd.PointList1[i], coordbits, sdi.Center, 1, lbl, rgn, false);
            }

         if (sd.ExtPointList != null && listCount(sd.ExtPointList) > 0)
            for (int i = 0, dest = listCount(sd.PointList2) + listCount(sd.PointList1); i < sd.ExtPointList.Count; i++, dest++) {
               Points[dest] = getGeoPoint(sd.ExtPointList[i], coordbits, sdi.Center, lbl, rgn);
            }
      }


      /// <summary>
      /// rechnet die Punkte aus Mapunits um 
      /// </summary>
      /// <param name="mupts"></param>
      /// <param name="bound">umgebendes Rechteck (null wenn keine Punkte vorhanden sind)</param>
      /// <returns></returns>
      PointF[] getPolyPointsAndBound(List<MapUnitPoint> mupts, out GarminCore.Bound? bound) {
         PointF[] pts = new PointF[mupts.Count];
         for (int i = 0; i < mupts.Count; i++)
            pts[i] = new PointF((float)mupts[i].LongitudeDegree, (float)mupts[i].LatitudeDegree);

         switch (mupts.Count) {
            case 0:
               bound = null;
               break;

            case 1:
               bound = new Bound(mupts[0]);
               break;

            default:
               bound = new Bound(mupts);
               break;
         }

         return pts;
      }

      GeoPoly getGeoPoly(StdFile_RGN.RawPolyData poly,
                         int coordbits,
                         MapUnitPoint subdiv_center,
                         StdFile_LBL lbl,
                         StdFile_RGN rgn,
                         StdFile_NET net,
                         bool withexttxt) {
         PointF[] pts = getPolyPointsAndBound(poly.GetMapUnitPoints(coordbits, subdiv_center), out GarminCore.Bound? bound);

         string? txt = "";
         if (poly.LabelOffset != UInt32.MaxValue)
            if (!poly.LabelInNET)
               txt = lbl.GetText(poly.LabelOffset, false);   // keine Garmin-Steuerzeichen für Symbole u.ä.
            else {
               // Wenn poly.LabelInNET dann ist poly.LabelOffset ein Offset in der NET-Datei.
               //RoadDataExt rd = new RoadDataExt(net.Roaddata[net.Idx4Offset[poly.LabelOffset]], lbl, rgn);
               RoadDataExt rd = new RoadDataExt(net.Decode_RoadDefinition(poly.LabelOffset, lbl), lbl, rgn);

               if (rd.Street.Count > 0)
                  foreach (string street in rd.Street) {
                     if (!string.IsNullOrEmpty(street)) {
                        if (txt.Length > 0)
                           txt += ", ";
                        txt += street.Trim();
                     }
                  }
               if (withexttxt) {
                  if (!string.IsNullOrEmpty(rd.City)) {
                     if (txt.Length > 0)
                        txt += ", ";
                     txt += rd.City;
                  }
                  if (!string.IsNullOrEmpty(rd.Zip)) {
                     if (txt.Length > 0)
                        txt += ", ";
                     txt += rd.Zip;
                  }
               }
            }

         if (bound == null)
            bound = new Bound();    // es sind dann gar keine Punkte vorhanden

         return new GeoPoly((poly.Type << 8) | poly.Subtype,
                            txt,
                            pts,
                            bound.LeftDegree,
                            bound.RightDegree,
                            bound.BottomDegree,
                            bound.TopDegree,
                            poly.DirectionIndicator,
                            false);
      }

      GeoPoly getGeoPoly(StdFile_RGN.ExtRawPolyData poly,
                         int coordbits,
                         MapUnitPoint subdiv_center,
                         StdFile_LBL lbl) {
         PointF[] pts = getPolyPointsAndBound(poly.GetMapUnitPoints(coordbits, subdiv_center), out GarminCore.Bound? bound);

         string? txt = "";
         if (poly.HasLabel)
            txt = lbl.GetText(poly.LabelOffset, false);

         if (bound == null)
            bound = new Bound();    // es sind dann gar keine Punkte vorhanden

         return new GeoPoly(((0x100 | poly.Type) << 8) | poly.Subtype,
                            txt,
                            pts,
                            bound.LeftDegree,
                            bound.RightDegree,
                            bound.BottomDegree,
                            bound.TopDegree,
                            false,
                            false);
      }

      GeoPoint getGeoPoint(StdFile_RGN.RawPointData rawpt,
                           int coordbits,
                           MapUnitPoint subdiv_center,
                           int listno,
                           StdFile_LBL lbl,
                           StdFile_RGN rgn,
                           bool withexttxt) {
         MapUnitPoint mup = new MapUnitPoint(subdiv_center.Longitude + Coord.RawUnits2MapUnits(rawpt.RawDeltaLongitude, coordbits),
                                             subdiv_center.Latitude + Coord.RawUnits2MapUnits(rawpt.RawDeltaLatitude, coordbits));
         string? txt = "";
         if (rawpt.LabelOffset != 0)
            if (!rawpt.IsPoiOffset) {
               txt = lbl.GetText(rawpt.LabelOffset, true);
            } else {
               int idx = -1;
               switch (listno) {
                  case 1:
                     if (lbl.PointPropertiesListOffsets.ContainsKey(rawpt.LabelOffset)) {
                        idx = lbl.PointPropertiesListOffsets[rawpt.LabelOffset];
                     } else
                        Debug.WriteLine("Fehler bei IsPoiOffset=" + rawpt.LabelOffset.ToString() + ", aber ohne gültige POIPropertiesListOffsets?");
                     break;

                  case 2:
                     idx = lbl.PointPropertiesListOffsets[rawpt.LabelOffset];
                     break;
               }
               if (idx >= 0) {
                  PoiDataExt pd = new PoiDataExt(lbl.PointPropertiesList[idx], lbl, rgn);
                  if (pd != null) {
                     if (!string.IsNullOrEmpty(pd.Text))
                        txt = pd.Text;
                     if (withexttxt) {
                        string txt2 = "";
                        if (!string.IsNullOrEmpty(pd.Country)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.Country;
                        }
                        if (!string.IsNullOrEmpty(pd.Region)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.Region;
                        }
                        if (!string.IsNullOrEmpty(pd.City)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.City;
                        }
                        if (!string.IsNullOrEmpty(pd.Zip)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.Zip;
                        }
                        if (!string.IsNullOrEmpty(pd.Street)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.Street;
                        }
                        if (!string.IsNullOrEmpty(pd.StreetNumber)) {
                           if (txt2 == "")
                              txt2 += ", ";
                           txt2 += pd.StreetNumber;
                        }

                        if (txt2 != "")
                           txt += " (" + txt2.Substring(2) + ")";
                     }
                  }
               }
            }

         return new GeoPoint((rawpt.Type << 8) | rawpt.Subtype,
                             txt,
                             mup.LongitudeDegree,
                             mup.LatitudeDegree);
      }

      GeoPoint getGeoPoint(StdFile_RGN.ExtRawPointData rawpt,
                           int coordbits,
                           MapUnitPoint subdiv_center,
                           StdFile_LBL lbl,
                           StdFile_RGN rgn) {
         MapUnitPoint mup = new MapUnitPoint(subdiv_center.Longitude + Coord.RawUnits2MapUnits(rawpt.RawDeltaLongitude, coordbits),
                                             subdiv_center.Latitude + Coord.RawUnits2MapUnits(rawpt.RawDeltaLatitude, coordbits));
         string? txt = "";
         if (rawpt.HasLabel)
            txt = lbl.GetText(rawpt.LabelOffset, false);
         return new GeoPoint(((0x100 | rawpt.Type) << 8) | rawpt.Subtype,
                             txt,
                             mup.LongitudeDegree,
                             mup.LatitudeDegree);
      }

      string getMeter4Feet(string text) {
         try {
            return Math.Round(Convert.ToSingle(text) * 0.3048).ToString();    // Fuss -> Meter
         } catch { }
         return text;
      }

      public override string ToString() {
         return string.Format("Points {0}, Lines {1}, Areas {2}, Bound {3}",
                              Points != null ? Points.Length : 0,
                              Lines != null ? Lines.Length : 0,
                              Areas != null ? Areas.Length : 0,
                              Bound);
      }

      ~SubdivMapData() {
         Dispose(false);
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               if (Points != null)
                  foreach (var item in Points)
                     item.Dispose();
               Points = null;

               if (Areas != null)
                  foreach (var item in Areas)
                     item.Dispose();
               Areas = null;

               if (Lines != null)
                  foreach (var item in Lines)
                     item.Dispose();
               Lines = null;

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
