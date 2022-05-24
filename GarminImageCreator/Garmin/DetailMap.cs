/*
Copyright (C) 2015 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using GarminCore;
using GarminCore.Files;

namespace GarminImageCreator.Garmin {

   /// <summary>
   /// eine Teilkarte mit allen Objekten (entspricht einer <see cref="StdFile_RGN.SubdivData"/>)
   /// <para>alle Koordinaten in Grad</para>
   /// <para>Eine vollständige Pseudo-Garmin-Karte besteht aus einer Baumstruktur aus <see cref="DetailMap"/>-Objekten. Die Ebenen im
   /// Baum stehen für die Maplevel.</para>
   /// </summary>
   public class DetailMap : IDisposable {

      #region Geo-Objekte

      public abstract class GeoObject : IDisposable {

         int _type;

         /// <summary>
         /// vollständiger Typ (erweitert, Haupt- und Subtyp bilden eine max. 5stellige Hex-Zahl)
         /// </summary>
         public int Type {
            get {
               return _type;
            }
            set {
               _type = value;
            }
         }

         /// <summary>
         /// Haupttyp 0x00..0xFF
         /// </summary>
         public int MainType {
            get {
               return (_type & 0xFF00) >> 8;
            }
         }

         /// <summary>
         /// Subtyp 0x00..0xFF
         /// </summary>
         public int SubType {
            get {
               return _type & 0xFF;
            }
         }

         /// <summary>
         /// Text des Objektes
         /// </summary>
         public string Text { get; set; }


         public GeoObject(int type, string txt) {
            Type = type;
            Text = txt;
         }

         public override string ToString() {
            return string.Format("0x{0:x}", Type) + (!string.IsNullOrEmpty(Text) ? ", " + Text : "");
         }

         ~GeoObject() {
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

               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion
      }

      public class GeoPoint : GeoObject {

         /// <summary>
         /// Punktkoordinaten
         /// </summary>
         public PointF Point { get; protected set; }


         public GeoPoint(int type, string txt, float lon, float lat) : base(type, txt) {
            Point = new PointF(lon, lat);
         }

         public GeoPoint(int type, string txt, double lon, double lat) : this(type, txt, (float)lon, (float)lat) { }

         public GeoPoint(int type, string txt, PointF pt, bool ptcopy = false) : base(type, txt) {
            if (ptcopy)
               Point = pt;
            else
               Point = new PointF(pt.X, pt.Y);
         }

         public GeoPoint(GeoPoint point, bool ptcopy) :
            this(point.Type, point.Text, point.Point, ptcopy) { }

         public bool IsInBound(Bound bound) {
            return bound.IsEnclosed(Point.X, Point.Y);
         }

         public override string ToString() {
            return string.Format("{0}, lon={1}, lat={2}", base.ToString(), Point.X, Point.Y);
         }

      }

      public class GeoPoly : GeoObject {

         /// <summary>
         /// Koordinaten der Punkte
         /// </summary>
         public PointF[] Points { get; protected set; }

         public Bound Bound { get; protected set; }

         public bool DirectionIndicator { get; protected set; }


         public GeoPoly(int type,
                        string txt,
                        IList<PointF> pt,
                        double boundleft,
                        double boundright,
                        double boundbottom,
                        double boundtop,
                        bool directionindicator,
                        bool ptcopy = false) : base(type, txt) {
            Points = new PointF[pt.Count];
            if (ptcopy) {
               for (int i = 0; i < pt.Count; i++) {
                  Points[i] = new PointF(pt[i].X, pt[i].Y);
               }
            } else {
               for (int i = 0; i < pt.Count; i++) {
                  Points[i] = pt[i];
               }
            }
            Bound = new Bound(boundleft, boundright, boundbottom, boundtop);

            DirectionIndicator = directionindicator;
         }

         public GeoPoly(GeoPoly poly, bool ptcopy) :
            this(poly.Type, poly.Text, poly.Points, poly.Bound.LeftDegree, poly.Bound.RightDegree, poly.Bound.BottomDegree, poly.Bound.TopDegree, poly.DirectionIndicator, ptcopy) { }

         public bool OverlappedWithBound(Bound bound) {
            return bound.IsOverlapped(Bound);
         }

         public override string ToString() {
            return string.Format("{0}, points {1}", base.ToString(), Points.Length);
         }

         bool _isdisposed = false;

         protected override void Dispose(bool notfromfinalizer) {
            if (!_isdisposed) {
               if (notfromfinalizer) { // nur dann alle managed Ressourcen freigeben
                  Points = null;
               }

               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;
               base.Dispose(notfromfinalizer);
            }
         }
      }

      #endregion

      /// <summary>
      /// zusätzliche Daten für normale Punkte (aus der LBL-Datei)
      /// </summary>
      class PoiDataExt {

         public string Text;
         public string Country;
         public string Region;
         public string City;
         public string Zip;
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

         public string City;
         public string Zip;
         public List<string> Street;
         public uint RoadLength;

         public RoadDataExt(StdFile_NET.RoadData rd, StdFile_LBL lbl, StdFile_RGN rgn) {
            City = Zip = "";
            Street = new List<string>();
            if (rd.ZipIndex4Node != null)
               for (int side = 0; side < rd.ZipIndex4Node.Count; side++) {
                  if (rd.ZipIndex4Node[side].Count > 0) {
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
               for (int side = 0; side < rd.CityIndex4Node.Count; side++) {
                  if (rd.CityIndex4Node[side].Count > 0) {
                     // aus der NET-Datei
                     City = rd.GetCityText(lbl,
                                           rgn,
                                           side == 0 ?
                                                StdFile_NET.RoadData.Side.Left :
                                                StdFile_NET.RoadData.Side.Right,
                                           false);
                  }
               }

            for (int i = 0; i < rd.LabelInfo.Count; i++)
               Street.Add(lbl.GetText(rd.LabelInfo[i], true));

            RoadLength = rd.RoadLength * 2;
         }

         public RoadDataExt(RoadDataExt rd) {
            City = rd.City;
            Zip = rd.Zip;
            Street = new List<string>();
            foreach (string street in rd.Street)
               Street.Add(street);
            RoadLength = rd.RoadLength;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder();

            if (City.Length > 0) {
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
      public Bound Bound { get; protected set; } = null;

      public GeoPoint[] Points { get; protected set; } = null;

      public GeoPoly[] Areas { get; protected set; } = null;

      public GeoPoly[] Lines { get; protected set; } = null;

      /// <summary>
      /// Level 0, ...
      /// </summary>
      public int Level { get; protected set; } = 0;


      //long _dataHasRead = 0;
      ///// <summary>
      ///// Wurden die Daten schon eingelesen? (threadsicher)
      ///// </summary>
      //public bool DataHasRead {
      //   get {
      //      return Interlocked.Read(ref _dataHasRead) != 0;
      //   }
      //   protected set {
      //      Interlocked.Exchange(ref _dataHasRead, value ? 1 : 0);
      //   }
      //}


      public DetailMap() { }

      public DetailMap(int sdidx,
                       StdFile_TRE tre,
                       StdFile_LBL lbl,
                       StdFile_RGN rgn,
                       StdFile_NET net) {
         ReadData(sdidx, tre, lbl, rgn, net);
      }


      PointF[] getPolyPointsAndBound(List<MapUnitPoint> mupts, out GarminCore.Bound bound) {
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
         PointF[] pts = getPolyPointsAndBound(poly.GetMapUnitPoints(coordbits, subdiv_center), out GarminCore.Bound bound);

         string txt = "";
         if (poly.LabelOffsetInLBL != UInt32.MaxValue)
            if (!poly.LabelInNET)
               txt = lbl.GetText(poly.LabelOffsetInLBL, false);   // keine Garmin-Steuerzeichen für Symbole u.ä.
            else {
               RoadDataExt rd = new RoadDataExt(net.Roaddata[net.Idx4Offset[poly.LabelOffsetInLBL]], lbl, rgn);
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
         PointF[] pts = getPolyPointsAndBound(poly.GetMapUnitPoints(coordbits, subdiv_center), out GarminCore.Bound bound);

         string txt = "";
         if (poly.HasLabel)
            txt = lbl.GetText(poly.LabelOffsetInLBL, false);

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
         string txt = "";
         if (rawpt.LabelOffsetInLBL != 0)
            if (!rawpt.IsPoiOffset) {
               txt = lbl.GetText(rawpt.LabelOffsetInLBL, true);
            } else {
               int idx = -1;
               switch (listno) {
                  case 1:
                     if (lbl.PointPropertiesListOffsets.ContainsKey(rawpt.LabelOffsetInLBL)) {
                        idx = lbl.PointPropertiesListOffsets[rawpt.LabelOffsetInLBL];
                     } else
                        Debug.WriteLine("Fehler bei IsPoiOffset=" + rawpt.LabelOffsetInLBL.ToString() + ", aber ohne gültige POIPropertiesListOffsets?");
                     break;

                  case 2:
                     idx = lbl.PointPropertiesListOffsets[rawpt.LabelOffsetInLBL];
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
         string txt = "";
         if (rawpt.HasLabel)
            txt = lbl.GetText(rawpt.LabelOffsetInLBL, false);
         return new GeoPoint(((0x100 | rawpt.Type) << 8) | rawpt.Subtype,
                             txt,
                             mup.LongitudeDegree,
                             mup.LatitudeDegree);
      }

      /// <summary>
      /// liest alle Daten der <see cref="DetailMap"/> (Subdiv) ein
      /// </summary>
      /// <param name="sdidx"></param>
      /// <param name="tre"></param>
      /// <param name="lbl"></param>
      /// <param name="rgn"></param>
      /// <param name="net"></param>
      public void ReadData(int sdidx,
                           StdFile_TRE tre,
                           StdFile_LBL lbl,
                           StdFile_RGN rgn,
                           StdFile_NET net) {

         if (sdidx >= rgn.SubdivList.Count)
            throw new Exception("Die RGN-Datei hat nur " + rgn.SubdivList.Count.ToString() + "Subdivs (sdidx=" + sdidx.ToString() + ").");

         StdFile_RGN.SubdivData sd = rgn.SubdivList[sdidx];
         if (sd == null)
            throw new Exception("Die RGN-Daten der Subdiv " + sdidx.ToString() + " sind noch nicht eingelesen.");
         StdFile_TRE.SubdivInfoBasic sdi = tre.SubdivInfoList[sdidx];
         if (sdi == null)
            throw new Exception("Die TRE-Daten der Subdiv " + sdidx.ToString() + " sind noch nicht eingelesen.");
         Level = tre.SymbolicScaleDenominatorAndBitsLevel.Level4SubdivIdx1(sdidx + 1);
         int coordbits = tre.SymbolicScaleDenominatorAndBitsLevel.Bits4SubdivIdx1(sdidx + 1);
         Bound = sdi.GetBound(coordbits);

         // ================ Polygone verarbeiten

         Areas = new GeoPoly[sd.AreaList.Count + sd.ExtAreaList.Count];
         for (int i = 0; i < sd.AreaList.Count; i++) {
#if DEBUG
            try {
#endif
               Areas[i] = getGeoPoly(sd.AreaList[i], coordbits, sdi.Center, lbl, rgn, net, false);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei AreaList: " + ex.Message);
               throw;
            }
#endif
         }

         for (int i = 0, dest = sd.AreaList.Count; i < sd.ExtAreaList.Count; i++, dest++) {
#if DEBUG
            try {
#endif
               Areas[dest] = getGeoPoly(sd.ExtAreaList[i], coordbits, sdi.Center, lbl);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei ExtAreaList: " + ex.Message);
               throw;
            }
#endif
         }

         // ================ Linien verarbeiten

         Lines = new GeoPoly[sd.LineList.Count + sd.ExtLineList.Count];
         for (int i = 0; i < sd.LineList.Count; i++) {
#if DEBUG
            try {
#endif
               Lines[i] = getGeoPoly(sd.LineList[i], coordbits, sdi.Center, lbl, rgn, net, false);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei LineList: " + ex.Message);
               throw;
            }
#endif
         }

         for (int i = 0, dest = sd.LineList.Count; i < sd.ExtLineList.Count; i++, dest++) {
#if DEBUG
            try {
#endif
               Lines[dest] = getGeoPoly(sd.ExtLineList[i], coordbits, sdi.Center, lbl);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei ExtLineList: " + ex.Message);
               throw;
            }
#endif
         }

         for (int i = 0; i < Lines.Length; i++) {  // Höhenlinien: Feet -> Meter
            if (Lines[i].MainType == 0x20 ||
                Lines[i].MainType == 0x21 ||
                Lines[i].MainType == 0x22)
               if (!string.IsNullOrEmpty(Lines[i].Text))
                  Lines[i].Text = getMeter4Feet(Lines[i].Text);
         }

         // ================ Punkte verarbeiten

         Points = new GeoPoint[sd.PointList2.Count + sd.PointList1.Count + sd.ExtPointList.Count];
         for (int i = 0, dest = 0; i < sd.PointList2.Count; i++, dest++) { // vor den "normalen" Punkten einlesen, damit der ev. Index-Verweise stimmen (z.B. für Exits)
#if DEBUG
            try {
#endif
               Points[dest] = getGeoPoint(sd.PointList2[i], coordbits, sdi.Center, 2, lbl, rgn, false);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei PointList2: " + ex.Message);
               throw;
            }
#endif
         }

         for (int i = 0, dest = sd.PointList2.Count; i < sd.PointList1.Count; i++, dest++) {
#if DEBUG
            try {
#endif
               Points[dest] = getGeoPoint(sd.PointList1[i], coordbits, sdi.Center, 1, lbl, rgn, false);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei PointList1: " + ex.Message);
               throw;
            }
#endif
         }

         for (int i = 0, dest = sd.PointList2.Count + sd.PointList1.Count; i < sd.ExtPointList.Count; i++, dest++) {
#if DEBUG
            try {
#endif
               Points[dest] = getGeoPoint(sd.ExtPointList[i], coordbits, sdi.Center, lbl, rgn);
#if DEBUG
            } catch (Exception ex) {
               Debug.WriteLine("Exception bei PointList2: " + ex.Message);
               throw;
            }
#endif
         }
      }

      string getMeter4Feet(string text) {
         try {
            float f = Convert.ToSingle(text);
            return Math.Round(f * 0.3048).ToString();    // Fuss -> Meter
         } catch { }
         return text;
      }

      //readonly object read_locker = new object();

      /// <summary>
      /// liest alle Daten der <see cref="DetailMap"/> (Subdiv) ein (nur wenn <see cref="DataHasRead"/> false ist) und setzt <see cref="DataHasRead"/> auf true
      /// </summary>
      /// <param name="sdidx"></param>
      /// <param name="tre"></param>
      /// <param name="lbl"></param>
      /// <param name="rgn"></param>
      /// <param name="net"></param>
      /// <param name="coordbits"></param>
      //public void ReadDataOnce(int sdidx,
      //                         StdFile_TRE tre,
      //                         StdFile_LBL lbl,
      //                         StdFile_RGN rgn,
      //                         StdFile_NET net,
      //                         int coordbits) {
      //   if (!DataHasRead)
      //      lock (read_locker) {
      //         if (!DataHasRead) {
      //            ReadData(sdidx, tre, lbl, rgn, net, coordbits);
      //            DataHasRead = true;
      //         }
      //      }
      //}

      public override string ToString() {
         return string.Format("Level {0}, Points {1}, Lines {2}, Areas {3}, Bound {4}",
                              Level,
                              Points != null ? Points.Length : 0,
                              Lines != null ? Lines.Length : 0,
                              Areas != null ? Areas.Length : 0,
                              Bound);
      }

      ~DetailMap() {
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
               foreach (var item in Points) {
                  item.Dispose();
               }
               Points = null;
               foreach (var item in Areas) {
                  item.Dispose();
               }
               Areas = null;
               foreach (var item in Lines) {
                  item.Dispose();
               }
               Lines = null;

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
