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
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable 0661,0660,IDE1006

namespace GarminCore.Files {

   /// <summary>
   /// zum Lesen der RGN-Datei (enthält die geografischen Daten)
   /// <para>Die "normalen" geografischen Objekte sind in <see cref="SubdivData"/> organisiert (also abhängig vom Zoomlevel), die "erweiterten" Objekte
   /// in jeweils einer Gesamttabelle. Allerdings sind die Gesamttabellen auch in Bereiche je <see cref="SubdivData"/> gegliedert. Diese Bereiche
   /// sind in der TRE-Datei definiert.</para>
   /// </summary>
   public class StdFile_RGN : StdFile {

      #region Header-Daten

      /// <summary>
      /// Datenbereich für den <see cref="SubdivData"/>-Inhalt (0x15)
      /// </summary>
      public DataBlock? SubdivContentBlock { get; private set; }

      // --------- Headerlänge > 29 Byte

      /// <summary>
      /// Datenbereich für erweiterte Polygone
      /// </summary>
      public DataBlock? ExtAreasBlock { get; private set; }

      public byte[] Unknown_0x25 = new byte[0x14];

      /// <summary>
      /// Datenbereich für erweiterte Polylinien
      /// </summary>
      public DataBlock? ExtLinesBlock { get; private set; }

      public byte[] Unknown_0x41 = new byte[0x14];

      /// <summary>
      /// Datenbereich für erweiterte Punkte
      /// </summary>
      public DataBlock? ExtPointsBlock { get; private set; }

      public byte[] Unknown_0x5D = new byte[0x14];

      public DataBlock? UnknownBlock_0x71 { get; private set; }

      public byte[] Unknown_0x79 = new byte[4];


      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         SubdivContentBlock,
         ExtAreasBlock,
         ExtLinesBlock,
         ExtPointsBlock,
         UnknownBlock_0x71,
      }

      /// <summary>
      /// enthält die Daten eines Subdivs
      /// <para>Wegen der 16-Bit-Offsets darf eine Subdiv für Punkte, Indexpunkte und Linien nicht mehr als ushort.MaxValue Byte Umfang erreichen.
      /// Für die nachfolgenden Polygone dürfte diese Einschränkung aber nicht mehr gelten(?).</para>
      /// <para>Da die Verweise aus der Indextabelle der RGN-Datei nur einen 1-Byte-Index enthalten, sollte die Summe der Punkte vermutlich nicht größer als 255 sein (?).</para>
      /// </summary>
      public class SubdivData : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Liste der Punkte
         /// </summary>
         public List<RawPointData> PointList1;

         /// <summary>
         /// Liste der Punkte (wahrscheinlich nur für "Stadt"-Typen, kleiner 0x12)
         /// </summary>
         public List<RawPointData> PointList2;

         /// <summary>
         /// Liste der Polylines
         /// </summary>
         public List<RawPolyData> LineList;

         /// <summary>
         /// Liste der Polygone
         /// </summary>
         public List<RawPolyData> AreaList;

         // --------------------------------------------------------------------

         // Die Listen für die erweiterten Daten werden hier nur verwaltet. Die Speicherung dieser Daten erfolgt NICHT im Subdiv-Bereich.

         /// <summary>
         /// Listen der erweiterten Linien (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPolyData> ExtLineList;

         /// <summary>
         /// Listen der erweiterten Flächen (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPolyData> ExtAreaList;

         /// <summary>
         /// Listen der erweiterten Punkte (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPointData> ExtPointList;


         public SubdivData() {
            PointList1 = new List<RawPointData>();
            PointList2 = new List<RawPointData>();
            LineList = new List<RawPolyData>();
            AreaList = new List<RawPolyData>();

            ExtLineList = new List<ExtRawPolyData>();
            ExtAreaList = new List<ExtRawPolyData>();
            ExtPointList = new List<ExtRawPointData>();
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            if (extdata == null)
               return;
            UInt32 ext = (UInt32)extdata;
            StdFile_TRE.SubdivInfoBasic.SubdivContent Content = (StdFile_TRE.SubdivInfoBasic.SubdivContent)(ext >> 24);
            uint SubdivLength = ext & 0xFFFF;

            DataBlock data_points = new DataBlock(UInt32.MaxValue, 0);
            DataBlock data_idxpoints = new DataBlock(UInt32.MaxValue, 0);
            DataBlock data_polylines = new DataBlock(UInt32.MaxValue, 0);
            DataBlock data_polygons = new DataBlock(UInt32.MaxValue, 0);

            PointList1.Clear();
            PointList2.Clear();
            LineList.Clear();
            AreaList.Clear();

            if (SubdivLength == 0 ||
                Content == StdFile_TRE.SubdivInfoBasic.SubdivContent.nothing) {
               br.Seek(SubdivLength, SeekOrigin.Current);
               Debug.WriteLine("Unbekannter Subdiv-Inhalt/leer");
               return;
            }

            long subdivstart = br.Position;       // Startpunkt für die Offsetberechnung

            // ----- Ermittlung der Offsets für die einzelnen Objektarten -----

            Queue<StdFile_TRE.SubdivInfoBasic.SubdivContent> offstype = new Queue<StdFile_TRE.SubdivInfoBasic.SubdivContent>();

            // Anzahl der nötigen Offsets ermitteln (dabei den Offset als Kennung auf 0 setzen)
            int types = 0;
            if ((Content & StdFile_TRE.SubdivInfoBasic.SubdivContent.poi) != 0) {
               data_points.Offset = 0;
               offstype.Enqueue(StdFile_TRE.SubdivInfoBasic.SubdivContent.poi);
               types++;
            }
            if ((Content & StdFile_TRE.SubdivInfoBasic.SubdivContent.idxpoi) != 0) {
               data_idxpoints.Offset = 0;
               offstype.Enqueue(StdFile_TRE.SubdivInfoBasic.SubdivContent.idxpoi);
               types++;
            }
            if ((Content & StdFile_TRE.SubdivInfoBasic.SubdivContent.line) != 0) {
               data_polylines.Offset = 0;
               offstype.Enqueue(StdFile_TRE.SubdivInfoBasic.SubdivContent.line);
               types++;
            }
            if ((Content & StdFile_TRE.SubdivInfoBasic.SubdivContent.area) != 0) {
               data_polygons.Offset = 0;
               offstype.Enqueue(StdFile_TRE.SubdivInfoBasic.SubdivContent.area);
               types++;
            }

            // alle Offsets einlesen (für die 1. Objektart existiert niemals ein Offset)
            // Die Reihenfolge der Objektarten ist festgelegt: points, indexed points, polylines and then polygons.
            // Für die erste vorhandene Objektart ist kein Offset vorhanden, da sie immer direkt hinter der Offsetliste beginnt.
            offstype.Dequeue();
            while (offstype.Count > 0) {
               // Da die Offsets nur als 2-Byte-Zahl gespeichert werden, ist die Größe eines Subdiv auf 65kB begrenzt!
               UInt16 offset = br.Read2AsUShort();
               switch (offstype.Dequeue()) {
                  case StdFile_TRE.SubdivInfoBasic.SubdivContent.poi:
                     data_points.Offset = offset;
                     break;

                  case StdFile_TRE.SubdivInfoBasic.SubdivContent.idxpoi:
                     data_idxpoints.Offset = offset;
                     break;

                  case StdFile_TRE.SubdivInfoBasic.SubdivContent.line:
                     data_polylines.Offset = offset;
                     break;

                  case StdFile_TRE.SubdivInfoBasic.SubdivContent.area:
                     data_polygons.Offset = offset;
                     break;
               }
            }

            if (types > 1)
               // Der Offset, der jetzt noch 0 ist, wird auf den Wert hinter die Offsetliste gesetzt.
               if (data_points.Offset == 0)
                  data_points.Offset = (UInt32)((types - 1) * 2);
               else if (data_idxpoints.Offset == 0)
                  data_idxpoints.Offset = (UInt32)((types - 1) * 2);
               else if (data_polylines.Offset == 0)
                  data_polylines.Offset = (UInt32)((types - 1) * 2);
               else if (data_polygons.Offset == 0)
                  data_polygons.Offset = (UInt32)((types - 1) * 2);

            // Länge der Datenbereiche bestimmen
            if (data_points.Offset != UInt32.MaxValue) {
               if (data_idxpoints.Offset != UInt32.MaxValue)
                  data_points.Length = data_idxpoints.Offset - data_points.Offset;
               else if (data_polylines.Offset != UInt32.MaxValue)
                  data_points.Length = data_polylines.Offset - data_points.Offset;
               else if (data_polygons.Offset != UInt32.MaxValue)
                  data_points.Length = data_polygons.Offset - data_points.Offset;
               else
                  data_points.Length = SubdivLength - data_points.Offset;
            }
            if (data_idxpoints.Offset != UInt32.MaxValue) {
               if (data_polylines.Offset != UInt32.MaxValue)
                  data_idxpoints.Length = data_polylines.Offset - data_idxpoints.Offset;
               else if (data_polygons.Offset != UInt32.MaxValue)
                  data_idxpoints.Length = data_polygons.Offset - data_idxpoints.Offset;
               else
                  data_idxpoints.Length = SubdivLength - data_idxpoints.Offset;
            }
            if (data_polylines.Offset != UInt32.MaxValue) {
               if (data_polygons.Offset != UInt32.MaxValue)
                  data_polylines.Length = data_polygons.Offset - data_polylines.Offset;
               else
                  data_polylines.Length = SubdivLength - data_polylines.Offset;
            }
            if (data_polygons.Offset != UInt32.MaxValue) {
               data_polygons.Length = SubdivLength - data_polygons.Offset;
            }


            // Objekte einlesen
            if (data_points.Offset != UInt32.MaxValue) {
               if (br.Position != subdivstart + data_points.Offset) {
                  Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des Point-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + data_points.Offset));
               }
               br.Seek(subdivstart + data_points.Offset);
               long endpos = br.Position + data_points.Length;
               while (br.Position < endpos)
                  PointList1.Add(new RawPointData(br));
            }

            if (data_idxpoints.Offset != UInt32.MaxValue) {
               if (br.Position != subdivstart + data_idxpoints.Offset) {
                  Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des IdxPoint-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + data_idxpoints.Offset));
               }
               br.Seek(subdivstart + data_idxpoints.Offset);
               long endpos = br.Position + data_idxpoints.Length;
               while (br.Position < endpos)
                  PointList2.Add(new RawPointData(br));
            }

            if (data_polylines.Offset != UInt32.MaxValue) {
               if (br.Position != subdivstart + data_polylines.Offset) {
                  Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des Polyline-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + data_polylines.Offset));
               }
               br.Seek(subdivstart + data_polylines.Offset);
               long endpos = br.Position + data_polylines.Length;
               while (br.Position < endpos)
                  LineList.Add(new RawPolyData(br));
            }

            if (data_polygons.Offset != UInt32.MaxValue) {
               if (br.Position != subdivstart + data_polygons.Offset) {
                  Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des Polygon-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + data_polygons.Offset));
               }
               br.Seek(subdivstart + data_polygons.Offset);
               long endpos = br.Position + data_polygons.Length;
               while (br.Position < endpos)
                  AreaList.Add(new RawPolyData(br, true));
            }

            if (br.Position != subdivstart + SubdivLength) {
               Debug.WriteLine("Vermutlich Fehler beim Einlesen der Datens einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + SubdivLength));
            }
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

         /// <summary>
         /// <see cref="Bound"/> aller Objekte der Subdiv
         /// </summary>
         /// <returns></returns>
         public Bound? GetBound4Deltas(int coordbits, MapUnitPoint subdiv_center) {
            Bound? br = GetRawBound4Deltas();
            return br != null ? new Bound(Longitude.RawUnits2MapUnits(br.Left, coordbits) + subdiv_center.Longitude,
                                          Longitude.RawUnits2MapUnits(br.Right, coordbits) + subdiv_center.Longitude,
                                          Longitude.RawUnits2MapUnits(br.Bottom, coordbits) + subdiv_center.Latitude,
                                          Longitude.RawUnits2MapUnits(br.Top, coordbits) + subdiv_center.Latitude) :
                                null;
         }

         /// <summary>
         /// <see cref="Bound"/> der "rohen" Differenzen zum Mittelpunkt der Subdiv
         /// </summary>
         /// <returns>null, wenn keine Punkte x.</returns>
         protected Bound? GetRawBound4Deltas() {
            List<Bound?> rbs = new List<Bound?> {
               GetRawBoundDelta4PointDataList(PointList1),
               GetRawBoundDelta4PointDataList(PointList2),
               GetRawBoundDelta4PolyDataList(LineList),
               GetRawBoundDelta4PolyDataList(AreaList),
               GetRawBoundDelta4ExtPointDataList(ExtPointList),
               GetRawBoundDelta4ExtPolyDataList(ExtLineList),
               GetRawBoundDelta4ExtPolyDataList(ExtAreaList)
            };

            Bound? rb = null;
            int idx = -1;
            for (int i = 0; i < rbs.Count; i++) {
               if (rbs[i] != null) {
                  rb = rbs[i];
                  idx = i;
                  break;
               }
            }

            if (rb != null)
               for (int i = 0; i < rbs.Count; i++)
                  if (i != idx && rbs[i] != null)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                     rb.Embed(rbs[i]);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.

            return rb;
         }

         protected Bound? GetRawBoundDelta4PointDataList(List<RawPointData> lst) {
            Bound? rb = null;
            if (lst.Count > 0) {
               rb = new Bound(lst[0].RawDeltaLongitude, lst[0].RawDeltaLatitude);
               for (int i = 1; i < lst.Count; i++)
                  rb.Embed(lst[i].RawDeltaLongitude, lst[i].RawDeltaLatitude);
            }
            return rb;
         }

         protected Bound? GetRawBoundDelta4ExtPointDataList(List<ExtRawPointData> lst) {
            Bound? rb = null;
            if (lst.Count > 0) {
               rb = new Bound(lst[0].RawDeltaLongitude, lst[0].RawDeltaLatitude);
               for (int i = 1; i < lst.Count; i++)
                  rb.Embed(lst[i].RawDeltaLongitude, lst[i].RawDeltaLatitude);
            }
            return rb;
         }

         protected Bound? GetRawBoundDelta4PolyDataList(List<RawPolyData> lst) {
            Bound? rb = null;
            if (lst.Count > 0) {
               for (int i = 1; i < lst.Count; i++) {
                  Bound? b = lst[i].GetRawBoundDelta();
                  if (b != null)
                     if (rb != null)
                        rb.Embed(b);
                     else
                        rb = new Bound(b);
               }
            }
            return rb;
         }

         protected Bound? GetRawBoundDelta4ExtPolyDataList(List<ExtRawPolyData> lst) {
            Bound? rb = null;
            if (lst.Count > 0) {
               for (int i = 0; i < lst.Count; i++) {
                  Bound? b = lst[i].GetRawBoundDelta();
                  if (b != null)
                     if (rb != null)
                        rb.Embed(b);
                     else
                        rb = new Bound(b);
               }
            }
            return rb;
         }

         /// <summary>
         /// ermittelt die notwendige Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public uint DataLength() {
            uint len = 0;
            for (int i = 0; i < PointList1.Count; i++)
               len += PointList1[i].DataLength;
            for (int i = 0; i < PointList2.Count; i++)
               len += PointList2[i].DataLength;
            for (int i = 0; i < LineList.Count; i++)
               len += LineList[i].DataLength;
            for (int i = 0; i < AreaList.Count; i++)
               len += AreaList[i].DataLength;

            if (len > 0) {          // sonst Subdiv ohne Inhalt
               int types = 0;
               if (PointList1.Count > 0)
                  types++;
               if (PointList2.Count > 0)
                  types++;
               if (LineList.Count > 0)
                  types++;
               if (AreaList.Count > 0)
                  types++;
               len += (uint)((types - 1) * 2);
            }

            return len;
         }

         public override string ToString() {
            return string.Format("Points1 {0}, Points2 {1}, Lines {2}, Areas {3}; Ext.: Points {4}, Lines {5}, Areas {6}",
                                 PointList1.Count,
                                 PointList2.Count,
                                 LineList.Count,
                                 AreaList.Count,
                                 ExtPointList.Count,
                                 ExtLineList.Count,
                                 ExtAreaList.Count);
         }

      }

      /* Technische Einschränkungen für Objekttypennummern in der RGN-Datei
       * ==================================================================
       * ACHTUNG: Objekttypennummern, die die technischen Einschränkungen erfüllen, müssen trotzdem nicht unbedingt vom GPS-Gerät dargestellt werden.
       * 
       * Point:      Typ 7 Bit; Subtyp 8 Bit                                                    Typ 0x00..0x7F; Subtyp 0x00..0xFF
       *             aber Subtyp-Einschränkung in Typefile 5 Bit                                   Subtyp 0x00..0x1F
       * Polygon:    Typ-Bit 7 zweckentfremdet, deshalb nur 7 Bit; kein Subtyp                  Typ 0x00..0x7F
       * Polyline:   Typ-Bit 6 für Richtungseinschränkung, deshalb nur 6 Bit; kein Subtyp       Typ 0x00..0x3F
       * Ext:        Typ 8 Bit; Subtyp-Bit 5 Labelkennung, 
       *             Subtyp-Bit 7 Extrabytekennung, deshalb nur 5 Bit                           Typ 0x100..0x1FF, Subtyp 0x00..0x1F
       *             Die führende 1 im Typ symbolisiert den erweiterten Typ. Sie wird
       *             NICHT gespeichert.
       */

      #region geografische Objekte

      /// <summary>
      /// Basisklasse für Punkte, Linien und Polygone
      /// </summary>
      abstract public class GraphicObjectData : IComparable {

         protected byte _Type;
         protected UInt32 _LabelOffset;

         /// <summary>
         /// Differenz zum Mittelpunkt der zugehörigen Subdiv
         /// </summary>
         public Longitude RawDeltaLongitude;
         /// <summary>
         /// Differenz zum Mittelpunkt der zugehörigen Subdiv
         /// </summary>
         public Latitude RawDeltaLatitude;

         /// <summary>
         /// theoretisch bis 0x7f für Punkte, bis 0x7f (oder 0x3f?) für Polygone und Linien
         /// </summary>
         public int Type {
            get {
               return _Type & 0x7F;
            }
            set {
               _Type = (byte)((_Type & 0x80) | (value & 0x7F));
            }
         }

         /// <summary>
         /// Subtype 0x00..0xFF (nur bei Punkten)
         /// </summary>
         public int Subtype { get { return 0; } }

         /// <summary>
         /// Offset in der LBL-Datei (3 Byte)
         /// </summary>
         public UInt32 LabelOffsetInLBL {
            get {
               return _LabelOffset;
            }
            set {
               _LabelOffset = value & 0xFFFFFF;
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei (Typ, Labeloffset, Lon, Lat)
         /// </summary>
         public uint DataLength {
            get {
               return 8;
            }
         }

         public GraphicObjectData() {
            _LabelOffset = 0;
            RawDeltaLongitude = new Longitude(0);
            RawDeltaLatitude = new Latitude(0);
         }

         /// <summary>
         /// Bound der Differenzen zum Mittelpunkt der zugehörigen Subdiv
         /// </summary>
         /// <returns></returns>
         public virtual Bound? GetRawBoundDelta() {
            return new Bound(RawDeltaLongitude, RawDeltaLatitude);
         }

         /// <summary>
         /// liefert den Text zum LabelOffset
         /// </summary>
         /// <param name="lbl"></param>
         /// <returns></returns>
         public virtual string? GetText(StdFile_LBL lbl, bool clear = false) {
            if (LabelOffsetInLBL > 0)
               return lbl.GetText(LabelOffsetInLBL, clear);
            return "";
         }

         /// <summary>
         /// Hilfsfunktion zum Vergleichen für die Sortierung (über Typ und Subtyp)
         /// </summary>
         /// <param name="obj"></param>
         /// <returns></returns>
         public int CompareTo(object? obj) {
            if (obj is GraphicObjectData go) {
               if (go == null)
                  return 1;
               if (Type == go.Type) {
                  if (Subtype > go.Subtype)
                     return 1;
                  if (Subtype < go.Subtype)
                     return -1;
                  else
                     return 0;
               } else
                  if (Type > go.Type)
                  return 1;
               else
                  return -1;
            }
            throw new ArgumentException("Falsche Objektart beim Vergleich.");
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Typ {0:x2}", Type);
            if (Subtype > 0)
               sb.AppendFormat(", Subtyp {0:x2}", Subtype);
            sb.AppendFormat(", LabelOffset {0}", LabelOffsetInLBL);
            sb.AppendFormat(", RawDeltaLongitude {0}", RawDeltaLongitude);
            sb.AppendFormat(", RawDeltaLatitude {0}", RawDeltaLatitude);
            return sb.ToString();
         }

      }

      /// <summary>
      /// für erweiterte Objekte (werden nicht in den Subdiv gespeichert)
      /// <para>Es können ein Subtyp 0x00..0x1F und zusätzliche Daten ex.</para>
      /// </summary>
      abstract public class ExtGraphicObjectData : GraphicObjectData {

         protected byte _Subtype;
         protected byte[]? _ExtraBytes;
         protected byte[]? _UnknownKey;
         protected byte[]? _UnknownBytes;

         /// <summary>
         /// 8-Bit-Werte
         /// </summary>
         public new int Type {
            get {
               return _Type;
            }
            set {
               _Type = (byte)(value & 0xFF);
            }
         }

         /// <summary>
         /// Subtype 0x00..0x1F
         /// </summary>
         public new int Subtype {
            get {
               return _Subtype & 0x1f;                         // Bit 0..4 (0..0x1f)
            }
            set {
               if (value > 0)
                  _Subtype = (byte)(value & 0x1F);
               else
                  _Subtype = 0;
            }
         }

         /// <summary>
         /// Offset in der LBL-Datei (3 Byte); ungültig setzen mit UInt32.MaxValue
         /// </summary>
         public new UInt32 LabelOffsetInLBL {
            get {
               return HasLabel ?
                           _LabelOffset & 0x3fffff :  // ?
                           UInt32.MaxValue;
            }
            set {
               if (value != UInt32.MaxValue) {
                  _LabelOffset = value & 0xFFFFFF;
                  HasLabel = true;
               } else {
                  _LabelOffset = UInt32.MaxValue;
                  HasLabel = false;
               }
            }
         }

         /// <summary>
         /// Ex. ein Label?
         /// </summary>
         public bool HasLabel {
            get {
               return (_Subtype & 0x20) != 0;
            }
            private set {
               if (value)
                  _Subtype |= 0x20;
               else
                  _Subtype &= unchecked((byte)~0x20);
            }
         }

         /// <summary>
         /// unbekanntes Flag
         /// </summary>
         public bool HasUnknownFlag {
            get {
               return (_Subtype & 0x40) != 0;
            }
            private set {
               if (value)
                  _Subtype |= 0x40;
               else
                  _Subtype &= unchecked((byte)~0x40);
            }
         }

         /// <summary>
         /// Ex. Extra-Bytes?
         /// </summary>
         public bool HasExtraBytes {
            get {
               return (_Subtype & 0x80) != 0;
            }
            private set {
               if (value)
                  _Subtype |= 0x80;
               else
                  _Subtype &= unchecked((byte)~0x80);
            }
         }

         /// <summary>
         /// Array der Extra-Bytes (oder null)
         /// </summary>
         public byte[]? ExtraBytes {
            get {
               return HasExtraBytes ? _ExtraBytes : null;
            }
            set {
               if (value != null && value.Length > 0) {
                  _ExtraBytes = new byte[value.Length];
                  value.CopyTo(_ExtraBytes, 0);
                  HasExtraBytes = true;
               } else {
                  HasExtraBytes = false;
                  _ExtraBytes = null;
               }
            }
         }

         /// <summary>
         /// 3-Byte Key wenn <see cref="HasUnknownFlag"/> gesetzt ist
         /// </summary>
         public byte[]? UnknownKey {
            get {
               return HasUnknownFlag ? _UnknownKey : null;
            }
            set {
               if (value != null && value.Length > 0) {
                  _UnknownKey = new byte[value.Length];
                  if (_ExtraBytes != null && _ExtraBytes.Length >= value.Length)
                     value.CopyTo(_ExtraBytes, 0);
                  HasUnknownFlag = true;
               } else {
                  HasUnknownFlag = false;
                  _UnknownKey = null;
               }
            }
         }

         /// <summary>
         /// zusätzliche Bytes
         /// </summary>
         public byte[]? UnknownBytes {
            get {
               return HasUnknownFlag ? _UnknownBytes : null;
            }
            set {
               if (value != null && value.Length > 0) {
                  _UnknownBytes = new byte[value.Length];
                  value.CopyTo(_UnknownBytes, 0);
                  HasUnknownFlag = true;
               } else {
                  HasUnknownFlag = false;
                  _UnknownBytes = null;
               }
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength {
            get {
               return 5 +                                                     // Typ + 2 * Delta
                      1 +                                                     // Subtyp
                      (uint)(HasLabel ? 3 : 0) +                              // Label
                      (uint)(HasExtraBytes && _ExtraBytes != null ? _ExtraBytes.Length : 0);         // Extrabytes
            }
         }


         public ExtGraphicObjectData()
            : base() {
            _Subtype = 0;
            _ExtraBytes = null;
            _UnknownKey = null;
            _UnknownBytes = null;
         }

         /// <summary>
         /// liefert für erweiterte Objekttypen das Array der Extra-Bytes (wenn vorhanden) oder null
         /// </summary>
         /// <param name="br"></param>
         /// <returns></returns>
         protected byte[]? ReadExtraBytes(BinaryReaderWriter br) {
            if (HasExtraBytes) {
               // vgl. Funktion encodeExtraBytes() in ExtTypeAttributes.java in MKGMAP
               /*    Vermutlich wird in Bit 7..5 des ersten Bytes die Anzahl der verwendeten Extrabytes codiert:
                *       000 -> 1 Byte
                *       100 -> 2 Bytes
                *       101 -> 3 Bytes
                *       111 -> mehr als 3 Bytes, im nächsten Byte steht:
                * 
                * original:
                     extraBytes = new byte[nob + 2];
                     int i = 0;
                     extraBytes[i++] = (byte)(0xe0 | flags0);     // -> Bit 5, 6, 7 als Kennung gesetzt, Bit 0..4 Daten
                     extraBytes[i++] = (byte)((nob << 1) | 1);    // bit0 always set?
                 
                *    nob = extraBytes[1] >> 1;
                *    Arraygröße: nob + 2
                *    --> Arraygröße = (extraBytes[1] >> 1) + 2;
                *       z.B. (0x19 >> 1) + 2 = 0x0E;
                */
               byte b1 = br.ReadByte();
               switch (b1 >> 5) {
                  case 0:           // 1 Byte insgesamt
                     return new byte[] { (byte)(b1 & 0x1F) };

                  case 0x04:        // 2 Bytes insgesamt
                     return new byte[] { (byte)(b1 & 0x1F), br.ReadByte() };

                  case 0x05:        // 3 Bytes insgesamt
                     return new byte[] { (byte)(b1 & 0x1F), br.ReadByte(), br.ReadByte() };

                  case 0x07:        // mehr als 3 Bytes insgesamt
                     byte b2 = br.ReadByte();
                     Debug.WriteLineIf((b2 & 0x1) == 0, "Bit 0 bei Länge der Extra-Bytes ist 0.");
                     byte[] b = new byte[(b2 >> 1) + 2];
                     b[0] = (byte)(b1 & 0x1F);
                     b[1] = b2;
                     for (int i = 2; i < b.Length; i++)
                        b[i] = br.ReadByte();
                     return b;

                  default:
                     throw new Exception("Unbekannte Anzahl Extra-Bytes in erweitertem Objekttyp.");
               }
            }
            return null;
         }

         /// <summary>
         /// schreibt bei erweiterten Objekttypen das Array der Extra-Bytes (wenn vorhanden)
         /// <para>Das Array muss schon die korrekte Länge haben.</para>
         /// </summary>
         /// <param name="bw"></param>
         /// <param name="extrabytes"></param>
         protected void WriteExtraBytes(BinaryReaderWriter bw) {
            if (HasExtraBytes &&
                _ExtraBytes != null &&
                _ExtraBytes.Length > 0) {
               _ExtraBytes[0] &= 0x1F;        // Bit, 7,6,5 auf 000 setzen
               switch (_ExtraBytes.Length) {
                  case 1:           // Bit, 7,6,5 auf 000 setzen
                     break;

                  case 2:           // Bit, 7,6,5 auf 100 setzen
                     _ExtraBytes[0] |= 0x80;
                     break;

                  case 3:           // Bit, 7,6,5 auf 101 setzen
                     _ExtraBytes[0] |= 0xA0;
                     break;

                  default:          // Bit, 7,6,5 auf 111 setzen
                     _ExtraBytes[0] |= 0xE0;
                     _ExtraBytes[1] = (byte)(((_ExtraBytes.Length - 2) << 1) & 0x01);
                     break;

               }
               bw.Write(_ExtraBytes);
            }
         }


         public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Type {0:x2}", Type);
            if (Subtype > 0)
               sb.AppendFormat(", Subtype {0:x2}", Subtype);
            if (HasLabel)
               sb.AppendFormat(", LabelOffset {0}", LabelOffsetInLBL);
            sb.AppendFormat(", RawDeltaLongitude {0}", RawDeltaLongitude);
            sb.AppendFormat(", RawDeltaLatitude {0}", RawDeltaLatitude);
            if (HasExtraBytes && ExtraBytes != null)
               sb.AppendFormat(", Anzahl ExtraBytes {0}", ExtraBytes.Length);
            return sb.ToString();
         }

      }


      /// <summary>
      /// Rohdaten für Punkte 0x00..0x7F, ev. mit Subtype
      /// </summary>
      public class RawPointData : GraphicObjectData {

         protected byte _Subtype;

         /// <summary>
         /// Subtype 0x00..0xFF (0, wenn <see cref="HasSubtype"/> false ist)
         /// </summary>
         public new int Subtype {
            get {
               if (HasSubtype)
                  return _Subtype;
               else
                  return 0;
            }
            set {
               if (value > 0)
                  _Subtype = (byte)(value & 0xFF);
               else
                  _Subtype = 0;
               HasSubtype = value > 0;
            }
         }

         /// <summary>
         /// Text-Offset in der LBL-Datei oder Offset in der LBL-POI-Tabelle (Bit 0..21, d.h. bis 0x3FFFFF möglich)
         /// </summary>
         public new UInt32 LabelOffsetInLBL {
            get {
               return _LabelOffset & 0x3FFFFF;
            }
            set {
               _LabelOffset = value & 0x3FFFFF;
            }
         }

         /// <summary>
         /// es gibt einen Subtyp
         /// </summary>
         public bool HasSubtype {
            get {
               return (_LabelOffset & 0x800000) != 0;
            }
            set {
               if (value)
                  _LabelOffset |= 0x800000;
               else
                  _LabelOffset &= 0x7FFFFF;
            }
         }

         /// <summary>
         /// Offset für POI (auf einen "POIRecord")
         /// </summary>
         public bool IsPoiOffset {
            get {
               return (_LabelOffset & 0x400000) != 0;
            }
            set {
               if (value)
                  _LabelOffset |= 0x400000;
               else
                  _LabelOffset &= 0xBFFFFF;
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength {
            get {
               return (uint)(base.DataLength + (HasSubtype ? 1 : 0));
            }
         }


         public RawPointData()
            : base() {
            _Subtype = 0;
         }

         public RawPointData(BinaryReaderWriter br)
            : this() {
            Read(br);
         }

         public MapUnitPoint GetMapUnitPoint(int coordbits, MapUnitPoint subdiv_center) {
            return new MapUnitPoint(RawDeltaLongitude, RawDeltaLatitude, coordbits) + subdiv_center;
         }

         public void Read(BinaryReaderWriter br) {
            _Type = br.ReadByte();
            _LabelOffset = br.Read3AsUInt();
            RawDeltaLongitude = br.Read2AsShort();
            RawDeltaLatitude = br.Read2AsShort();
            if (HasSubtype)
               Subtype = br.ReadByte();
         }

         public void Write(BinaryReaderWriter bw) {
            bw.Write(_Type);
            bw.Write3(_LabelOffset);
            bw.Write((Int16)RawDeltaLongitude);
            bw.Write((Int16)RawDeltaLatitude);
            if (HasSubtype)
               bw.Write(_Subtype);
         }

         /// <summary>
         /// liefert den ev. vorhandenen Text zum Punkt
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="clear"></param>
         /// <returns></returns>
         public override string? GetText(StdFile_LBL lbl, bool clear = false) {
            if (!IsPoiOffset) {
               if (LabelOffsetInLBL > 0)
                  return lbl.GetText(LabelOffsetInLBL, clear);
            } else { // dann aus der POI-Tabelle
               if (lbl.PointPropertiesListOffsets.TryGetValue(LabelOffsetInLBL, out int poidx)) { // das sollte immer so sein
                  if (poidx < lbl.PointPropertiesList.Count) {
                     return lbl.GetText(lbl.PointPropertiesList[poidx].TextOffset, clear);
                  }
               }
            }
            return "";
         }


         public static bool operator ==(RawPointData x, RawPointData y) {
            if (x._Type == y._Type &&
                (!x.HasSubtype || (x._Subtype == y._Subtype)) &&
                x.RawDeltaLongitude == y.RawDeltaLongitude &&
                x.RawDeltaLatitude == y.RawDeltaLatitude &&
                x._LabelOffset == y._LabelOffset)
               return true;
            return false;
         }

         public static bool operator !=(RawPointData x, RawPointData y) {
            return x != y;
         }

         public override string ToString() {
            return base.ToString() + ", IsPoiOffset " + IsPoiOffset.ToString();
         }

      }

      /// <summary>
      /// Typ 0..0x3f für Linien und 0..0x7f für Polygone, Subtyp 0
      /// </summary>
      public class RawPolyData : GraphicObjectData {

         /// <summary>
         /// Daten für Polygon (oder Polylinie)
         /// </summary>
         public bool IsPolygon { get; set; }

         /// <summary>
         /// bis 0x7F für Polygone und 0x3F Linien
         /// </summary>
         public new int Type {
            get {
               return _Type & (IsPolygon ? 0x7F : 0x3F);
            }
            set {
               if (IsPolygon)
                  _Type = (byte)((_Type & 0x80) | (value & 0x7F));
               else
                  _Type = (byte)((_Type & 0xC0) | (value & 0x3F));
            }
         }

         /// <summary>
         /// Offset in der LBL-Datei (Bit 0..21)
         /// </summary>
         public new UInt32 LabelOffsetInLBL {
            get {
               return _LabelOffset & 0x3FFFFF;
            }
            set {
               _LabelOffset = value & 0x3FFFFF;
            }
         }


         /// <summary>
         /// Bits je Koordinate (codiert)
         /// </summary>
         public byte bitstreamInfo { get; private set; }

         /// <summary>
         /// Bitstream der Geodaten
         /// </summary>
         byte[]? _bitstream;

         public byte[]? bitstream { get { return _bitstream; } }

         /// <summary>
         /// Längenangabe für den gesamten Datenbereich in 1 oder 2 Byte (wenn der Bitstream länger als 255 Byte ist)
         /// </summary>
         public bool TwoByteLength {
            get {
               return Bit.IsSet(_Type, 7);
            }
            private set {
               _Type = (byte)Bit.Set(_Type, 7, value);
            }
         }


         /// <summary>
         /// mit Richtungsangabe ? (nur für Polylinien sinnvoll)
         /// </summary>
         public bool DirectionIndicator {
            get {
               return !IsPolygon && Bit.IsSet(_Type, 6);
            }
            set {
               if (!IsPolygon)
                  _Type = (byte)Bit.Set(_Type, 6, value);
            }
         }

         /// <summary>
         /// Label-Offset bezieht sich auf LBL oder NET-Datei (dann routable, also nur für Straßen)
         /// </summary>
         public bool LabelInNET {
            get {
               return Bit.IsSet(_LabelOffset, 23);
            }
            set {
               _LabelOffset = Bit.Set(_LabelOffset, 23, value);
            }
         }

         /// <summary>
         /// wenn true, dann 1 Bit zusätzlich je Punkt (die Straße sollte dann routable sein und Zusatzinfos in NET / NOD enthalten)
         /// </summary>
         public bool WithExtraBit {
            get {
               return Bit.IsSet(_LabelOffset, 22);
            }
            private set {
               _LabelOffset = Bit.Set(_LabelOffset, 22, value);
            }
         }

         /// <summary>
         /// Extrabit je Punkt (nur für routable Straßen)
         /// </summary>
         public List<bool> ExtraBit { get; protected set; }

         /// <summary>
         /// Ex. Punkte?
         /// </summary>
         public bool WithPoints {
            get {
               return _bitstream != null && _bitstream.Length > 0;
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength {
            get {
               return (uint)(base.DataLength + (TwoByteLength ? 2 : 1) + 1 + (_bitstream != null ? _bitstream.Length : 0));
            }
         }


         public RawPolyData(bool isPolygon = false)
            : base() {
            bitstreamInfo = 0xFF;
            IsPolygon = isPolygon;
            ExtraBit = new List<bool>();
         }

         /// <summary>
         /// liest die Daten für Polygon oder Linie ein
         /// </summary>
         /// <param name="br"></param>
         /// <param name="b4Polygon"></param>
         public RawPolyData(BinaryReaderWriter br, bool isPolygon = false)
            : this(isPolygon) {
            Read(br);
         }

         /// <summary>
         /// liest die Daten für Polygon oder Linie ein
         /// </summary>
         /// <param name="br"></param>
         public void Read(BinaryReaderWriter br) {
            _Type = br.ReadByte();
            _LabelOffset = br.Read3AsUInt();
            RawDeltaLongitude = br.Read2AsShort();
            RawDeltaLatitude = br.Read2AsShort();
            int BitstreamLength = TwoByteLength ? br.Read2AsUShort() : br.ReadByte();
            bitstreamInfo = br.ReadByte();
            _bitstream = br.ReadBytes(BitstreamLength);  // _bitstreamInfo zählt nicht mit!

            ExtraBit.Clear();
         }

         /// <summary>
         /// schreibt die Daten für Polygon oder Linie
         /// </summary>
         /// <param name="bw"></param>
         public void Write(BinaryReaderWriter bw) {
            bw.Write(_Type);
            bw.Write3(_LabelOffset);
            bw.Write((Int16)RawDeltaLongitude);
            bw.Write((Int16)RawDeltaLatitude);
            if (_bitstream != null)
               if (TwoByteLength)
                  bw.Write((UInt16)_bitstream.Length);
               else
                  bw.Write((byte)_bitstream.Length);
            bw.Write(bitstreamInfo);
            if (_bitstream != null)
               bw.Write(_bitstream);
         }

         /// <summary>
         /// liefert eine Liste aller Punkte und setzt dabei auch die Liste <see cref="ExtraBit"/> entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// <para><see cref="RawDeltaLongitude"/> und <see cref="RawDeltaLatitude"/> stellen den Startpunkt dar. Die Koordinaten beziehen sich auf den Mittelpunkt der zugehörigen Subdiv.</para>
         /// </summary>
         /// <returns></returns>
         public List<GeoDataBitstream.RawPoint>? GetRawPoints() {
            ExtraBit.Clear();
            return _bitstream != null ?
               GeoDataBitstream.GetRawPoints(ref _bitstream,
                                             bitstreamInfo & 0x0F,
                                             (bitstreamInfo & 0xF0) >> 4,
                                             RawDeltaLongitude,
                                             RawDeltaLatitude,
                                             WithExtraBit ? ExtraBit : null,
                                             false) :
               null;
         }

         /// <summary>
         /// liefert eine Liste aller Punkte und setzt dabei auch die Liste <see cref="ExtraBit"/> entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// </summary>
         /// <param name="coordbits"></param>
         /// <param name="subdiv_center"></param>
         /// <returns></returns>
         public List<MapUnitPoint> GetMapUnitPoints(int coordbits, MapUnitPoint subdiv_center) {
            List<MapUnitPoint> lst = new List<MapUnitPoint>();
            List<GeoDataBitstream.RawPoint>? rpt = GetRawPoints();
            if (rpt != null)
               foreach (var item in rpt)
                  lst.Add(item.GetMapUnitPoint(coordbits, subdiv_center));
            return lst;
         }

         /// <summary>
         /// liefert eine Liste aller Punkte und setzt dabei auch die Liste <see cref="ExtraBit"/> entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// </summary>
         /// <param name="coordbits"></param>
         /// <param name="subdiv_center"></param>
         /// <returns></returns>
         public MapUnitPoint[] GetMapUnitPoints2(int coordbits, MapUnitPoint subdiv_center) {
            List<GeoDataBitstream.RawPoint>? rpt = GetRawPoints();
            if (rpt != null) {
               MapUnitPoint[] lst = new MapUnitPoint[rpt.Count];
               for (int i = 0; i < rpt.Count; i++)
                  lst[i] = rpt[i].GetMapUnitPoint(coordbits, subdiv_center);
               return lst;
            } else
               return new MapUnitPoint[0];
         }

         /// <summary>
         /// setzt alle Punkte, gegebenenfalls auch die Liste <see cref="ExtraBit"/>
         /// </summary>
         /// <param name="pt">Punkte</param>
         /// <param name="extra">Liste der Extrabist je Punkt</param>
         /// <returns></returns>
         bool SetRawPoints(IList<GeoDataBitstream.RawPoint> pt, IList<bool>? extra = null) {
            ExtraBit.Clear();
            if (extra != null &&
                extra.Count == pt.Count)
               ExtraBit.AddRange(extra);
            byte[]? tmp = GeoDataBitstream.SetRawPoints(pt, out int basebits4lon, out int basebits4lat, ExtraBit, false);
            if (tmp != null) {
               _bitstream = tmp;
               bitstreamInfo = (byte)(basebits4lat << 4 | basebits4lon);
               TwoByteLength = _bitstream.Length > 255;
               RawDeltaLongitude = (Int16)pt[0].RawUnitsLon;
               RawDeltaLatitude = (Int16)pt[0].RawUnitsLat;
               return true;
            }
            Debug.WriteLineIf(tmp == null, string.Format("SetPoints() hat keinen Bitstream erzeugt (für {0} Punkte)", pt.Count));
            return false;
         }

         /// <summary>
         /// setzt alle Punkte, gegebenenfalls auch die Liste <see cref="ExtraBit"/>
         /// </summary>
         /// <param name="coordbits"></param>
         /// <param name="subdiv_center"></param>
         /// <param name="pt"></param>
         /// <param name="extra"></param>
         /// <returns></returns>
         public bool SetMapUnitPoints(int coordbits, MapUnitPoint subdiv_center, IList<MapUnitPoint> pt, IList<bool>? extra = null) {
            GeoDataBitstream.RawPoint[] ptlst = new GeoDataBitstream.RawPoint[pt.Count];
            for (int i = 0; i < pt.Count; i++)
               ptlst[i] = new GeoDataBitstream.RawPoint(pt[i], coordbits, subdiv_center);
            return SetRawPoints(ptlst, extra);
         }

         public static bool operator ==(RawPolyData x, RawPolyData y) {
            if (x._Type == y._Type &&
                x.RawDeltaLongitude == y.RawDeltaLongitude &&
                x.RawDeltaLatitude == y.RawDeltaLatitude &&
                x.WithExtraBit == y.WithExtraBit) {

               List<GeoDataBitstream.RawPoint>? px = x.GetRawPoints();
               List<GeoDataBitstream.RawPoint>? py = y.GetRawPoints();
               if (px == null || py == null || px.Count != py.Count)
                  return false;
               for (int i = 0; i < px.Count; i++)
                  if (px[i] != py[i])
                     return false;

               if (x.WithExtraBit) {
                  if (x.ExtraBit.Count != y.ExtraBit.Count)
                     return false;

                  for (int i = 0; i < x.ExtraBit.Count; i++)
                     if (x.ExtraBit[i] != y.ExtraBit[i])
                        return false;
               }

               return true;
            }
            return false;
         }

         public static bool operator !=(RawPolyData x, RawPolyData y) {
            return x != y;
         }

         /// <summary>
         /// RawBound der Differenzen zum Mittelpunkt der zugehörigen Subdiv
         /// </summary>
         /// <returns>null, wenn keine Punkte x.</returns>
         public override Bound? GetRawBoundDelta() {
            Bound? rb = null;
            List<GeoDataBitstream.RawPoint>? pts = GetRawPoints();
            if (pts != null && pts.Count > 0) {
               rb = new Bound(pts[0].RawUnitsLon, pts[0].RawUnitsLat);
               for (int i = 1; i < pts.Count; i++)
                  rb.Embed(pts[i].RawUnitsLon, pts[i].RawUnitsLat);
            }
            return rb;
         }

         public override string ToString() {
            return string.Format("Typ {0:x2}, LabelOffset {1}, LabelInNET {2}, RawDeltaLongitude {3}, RawDeltaLatitude {4}, WithExtraBit {5}, Datenbytes {6}",
                                 Type,
                                 LabelOffsetInLBL,
                                 LabelInNET,
                                 RawDeltaLongitude,
                                 RawDeltaLatitude,
                                 WithExtraBit,
                                 _bitstream != null ? _bitstream.Length : 0);
         }

      }


      /// <summary>
      /// Daten der erweiterten Punkte, Typ >=0x100, Subtyp 0..0x1f
      /// </summary>
      public class ExtRawPointData : ExtGraphicObjectData {

         public ExtRawPointData()
            : base() { }

         public ExtRawPointData(BinaryReaderWriter br)
            : base() {
            Read(br);
         }

         public void Read(BinaryReaderWriter br) {
            _Type = br.ReadByte();
            _Subtype = br.ReadByte();

            RawDeltaLongitude = br.Read2AsShort();
            RawDeltaLatitude = br.Read2AsShort();

            if (HasLabel)
               _LabelOffset = br.Read3AsUInt();

            ExtraBytes = ReadExtraBytes(br);

            if (HasUnknownFlag) {
               _UnknownKey = br.ReadBytes(3);

               if (_UnknownKey[0] == 0x41) {              // 41 xx yy

               } else if (_UnknownKey[0] == 0x03 &&
                          _UnknownKey[2] == 0x5A) {       // 03 xx 5A
                  int len = _UnknownKey[1];                    // "mittleres" Byte
                  len >>= 3;                                   // Anzahl der "Datensätze" zu je 4 Byte
                  _UnknownBytes = new byte[len * 4];
                  br.ReadBytes(_UnknownBytes);
               } else {

                  Debug.WriteLine(string.Format("ExtPointData mit unbekanntem Key: 0x{0:x} 0x{1:x} 0x{2:x}",
                                                _UnknownKey[0],
                                                _UnknownKey[1],
                                                _UnknownKey[2]));

               }
            }
         }

         public void Write(BinaryReaderWriter bw) {
            bw.Write(_Type);
            bw.Write(_Subtype);
            bw.Write((Int16)RawDeltaLongitude);
            bw.Write((Int16)RawDeltaLatitude);
            if (HasLabel)
               bw.Write3(_LabelOffset);
            if (HasExtraBytes)
               WriteExtraBytes(bw);
         }

         public MapUnitPoint GetMapUnitPoint(int coordbits, MapUnitPoint subdiv_center) {
            return new MapUnitPoint(RawDeltaLongitude, RawDeltaLatitude, coordbits) + subdiv_center;
         }

         public static bool operator ==(ExtRawPointData x, ExtRawPointData y) {
            if (x._Type == y._Type &&
                x._Subtype == y._Subtype &&
                x.RawDeltaLongitude == y.RawDeltaLongitude &&
                x.RawDeltaLatitude == y.RawDeltaLatitude &&
                (!x.HasLabel || (x._LabelOffset == y._LabelOffset)))
               if (!x.HasExtraBytes)
                  return true;
               else {
                  if (x._ExtraBytes != null &&
                      y._ExtraBytes != null) {
                     if (x._ExtraBytes.Length != y._ExtraBytes.Length)
                        return false;
                     for (int i = 0; i < x._ExtraBytes.Length; i++)
                        if (x._ExtraBytes[i] != y._ExtraBytes[i])
                           return false;
                  }
                  return true;
               }
            return false;
         }

         public static bool operator !=(ExtRawPointData x, ExtRawPointData y) {
            return x != y;
         }
      }

      /// <summary>
      /// erweiterte Linien und Polygone, Typ >=0x100, Subtyp 0..0x1f
      /// </summary>
      public class ExtRawPolyData : ExtGraphicObjectData {

         public byte bitstreamInfo { get; private set; }

         byte[]? _bitstream;

         public byte[]? bitstream { get { return _bitstream; } }

         /// <summary>
         /// Ex. Punkte?
         /// </summary>
         public bool WithPoints {
            get {
               return _bitstream != null && _bitstream.Length > 0;
            }
         }

         /// <summary>
         /// 7-Bit-Werte 0x00..0x7F
         /// </summary>
         public new int Type {
            get {
               return _Type & 0x7F;
            }
            set {
               _Type = (byte)(value & 0x7F);
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength {
            get {
               return (uint)(base.DataLength +
                             (_bitstream != null ? (_bitstream.Length + 1 < 0x7F ? 1 : 2) : 0) +   // Länge des Bitstreams
                             1 +                                                                   // Codierung des Bitstreams
                             (_bitstream != null ? _bitstream.Length : 0));                        // Bitstream
            }
         }

         /// <summary>
         /// Originalbytes für die gelesene Bitstreamlänge
         /// </summary>
         public byte[]? RawBitStreamLengthBytes { get; private set; }

         /// <summary>
         /// Länge des gelesenen Bitstreams
         /// </summary>
         public uint BitstreamLength { get; private set; }

         public ExtRawPolyData()
            : base() {
            bitstreamInfo = 0;
            _bitstream = null;
         }

         public ExtRawPolyData(BinaryReaderWriter br)
            : this() {
            Read(br);
         }

         public void Read(BinaryReaderWriter br) {
            _Type = br.ReadByte();
            _Subtype = br.ReadByte();

            RawDeltaLongitude = br.Read2AsShort();
            RawDeltaLatitude = br.Read2AsShort();

            /*
		if (blen >= 0x7f) {
			stream.write((blen << 2) | 2);               Bit 0 NICHT gesetz, Bit 1 gesetzt, Bit 6, 7 usw. sind verloren gegangen
			stream.write((blen << 2) >> 8);              ab Bit 6
		}
		else {
			stream.write((blen << 1) | 1);               Bit 0 gesetzt
		}

		stream.write(bw.getBytes(), 0, blen); 
             * */

            BitstreamLength = br.ReadByte();
            if ((BitstreamLength & 0x01) != 0) {      // Bit 0 Kennung für 1 Byte-Länge
               RawBitStreamLengthBytes = new byte[] { (byte)BitstreamLength };
               BitstreamLength >>= 1;
            } else {                                    // 2-Byte-Länge
               RawBitStreamLengthBytes = new byte[] { (byte)BitstreamLength, 0 };
               BitstreamLength >>= 2;
               RawBitStreamLengthBytes[1] = br.ReadByte();
               BitstreamLength |= (uint)(RawBitStreamLengthBytes[1] << 6);
            }

            bitstreamInfo = br.ReadByte();
            _bitstream = br.ReadBytes((int)BitstreamLength - 1);     // _bitstreamInfo ist in BitstreamLength eingeschlossen!

            if (HasLabel)
               _LabelOffset = br.Read3AsUInt();

            if (HasUnknownFlag)
               Debug.WriteLine("ExtPolyData mit unbekanntem Flag");

            ExtraBytes = ReadExtraBytes(br);

            /*    einfacher Test
              
            if (LabelOffset == 393029 && LongitudeDelta == 10285) //4 && BitstreamLength == 5)
               Console.WriteLine("");
            if (br.Position == 0x86af)
               Console.WriteLine("");


            List<GeoData4Polys.RawPoint> pt = null;
            List<GeoData4Polys.RawPoint> pt2 = null;

            try {
               pt = GetPoints();
            } catch (Exception ex) {
               Console.WriteLine(ex.Message);
            }

            try {
               SetPoints(pt);
            } catch (Exception ex) {
               Console.WriteLine(ex.Message);
            }

            try {
               pt2 = GetPoints();
            } catch (Exception ex) {
               Console.WriteLine(ex.Message);
            }

            if (pt.Count != pt2.Count)
               Console.WriteLine("ERROR Punktanzahl: " + this.ToString());
            else
               for (int i = 0; i < pt.Count; i++)
                  if (pt[i].Latitude != pt2[i].Latitude ||
                      pt[i].Longitude != pt2[i].Longitude)
                     Console.WriteLine("ERROR: " + this.ToString());
            */

         }

         public void Write(BinaryReaderWriter bw) {
            bw.Write(_Type);
            bw.Write(_Subtype);
            bw.Write((Int16)RawDeltaLongitude);
            bw.Write((Int16)RawDeltaLatitude);

            uint bitstreamLength = (uint)(_bitstream != null ? _bitstream.Length + 1 : 0);
            if (bitstreamLength < 0x7F) {
               bitstreamLength <<= 1;
               bitstreamLength |= 0x01;         // Bit 0 Kennung für 1 Byte-Länge
               bw.Write((byte)bitstreamLength);
            } else {
               bitstreamLength <<= 2;
               bw.Write((byte)((bitstreamLength | 0x02) & 0xFF));    // Bit 0 ist 0, Bit 1 ist 1; die Bits 0..5 des Originalwertes werden geschrieben
               bitstreamLength >>= 8;
               bw.Write((byte)(bitstreamLength & 0xFF));             // die Bits 6, 7, 8, ... des Originalwertes werden geschrieben
            }

            bw.Write(bitstreamInfo);
            if (_bitstream != null)
               bw.Write(_bitstream);

            if (HasLabel)
               bw.Write3(_LabelOffset);

            if (HasExtraBytes)
               WriteExtraBytes(bw);
         }

         /// <summary>
         /// liefert eine Liste aller Punkte entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// <para><see cref="LongitudeDelta"/> und <see cref="LatitudeDelta"/> stellen den Startpunkt dar. Die Koordinaten beziehen sich auf den Mittelpunkt der zugehörigen Subdiv.</para>
         /// </summary>
         /// <returns></returns>
         public List<GeoDataBitstream.RawPoint> GetRawPoints() {
            return GeoDataBitstream.GetRawPoints(ref _bitstream,
                                                 bitstreamInfo & 0x0F,
                                                 (bitstreamInfo & 0xF0) >> 4,
                                                 RawDeltaLongitude,
                                                 RawDeltaLatitude,
                                                 null,
                                                 true);
         }

         public List<MapUnitPoint> GetMapUnitPoints(int coordbits, MapUnitPoint subdiv_center) {
            List<MapUnitPoint> lst = new List<MapUnitPoint>();
            foreach (var item in GetRawPoints()) {
               MapUnitPoint pt = new MapUnitPoint(item.RawUnitsLon, item.RawUnitsLat, coordbits);
               pt.Add(subdiv_center);
               lst.Add(pt);
            }
            return lst;
         }

         /// <summary>
         /// setzt alle Punkte
         /// <para>Die Daten werden entsprechend <see cref="LongitudeDelta"/> und <see cref="LatitudeDelta"/> intern korrigiert.</para>
         /// </summary>
         /// <param name="pt">Punkte</param>
         /// <returns></returns>
         bool SetRawPoints(IList<GeoDataBitstream.RawPoint> pt) {
            byte[]? tmp = GeoDataBitstream.SetRawPoints(pt, out int basebits4lon, out int basebits4lat, null, true);
            if (tmp != null) {
               _bitstream = tmp;
               bitstreamInfo = (byte)(basebits4lat << 4 | basebits4lon);
               RawDeltaLongitude = (Int16)pt[0].RawUnitsLon;
               RawDeltaLatitude = (Int16)pt[0].RawUnitsLat;
               return true;
            }
            return false;
         }

         /// <summary>
         /// setzt alle Punkte, gegebenenfalls auch die Liste <see cref="ExtraBit"/>
         /// </summary>
         /// <param name="coordbits"></param>
         /// <param name="subdiv_center"></param>
         /// <param name="pt"></param>
         /// <param name="extra"></param>
         /// <returns></returns>
         public bool SetMapUnitPoints(int coordbits, MapUnitPoint subdiv_center, IList<MapUnitPoint> pt) {
            GeoDataBitstream.RawPoint[] ptlst = new GeoDataBitstream.RawPoint[pt.Count];
            for (int i = 0; i < pt.Count; i++)
               ptlst[i] = new GeoDataBitstream.RawPoint(pt[i], coordbits, subdiv_center);

            return SetRawPoints(ptlst);
         }

         public static bool operator ==(ExtRawPolyData x, ExtRawPolyData y) {
            if (x._Type == y._Type &&
                x._Subtype == y._Subtype &&
                x.RawDeltaLongitude == y.RawDeltaLongitude &&
                x.RawDeltaLatitude == y.RawDeltaLatitude &&
                (!x.HasLabel || (x._LabelOffset == y._LabelOffset)))
               if (!x.HasExtraBytes)
                  return true;
               else {
                  if (x._ExtraBytes != null &&
                      y._ExtraBytes != null) {
                     if (x._ExtraBytes.Length != y._ExtraBytes.Length)
                        return false;
                     for (int i = 0; i < x._ExtraBytes.Length; i++)
                        if (x._ExtraBytes[i] != y._ExtraBytes[i])
                           return false;
                  }

                  List<GeoDataBitstream.RawPoint> px = x.GetRawPoints();
                  List<GeoDataBitstream.RawPoint> py = y.GetRawPoints();
                  if (px.Count != py.Count)
                     return false;
                  for (int i = 0; i < px.Count; i++)
                     if (px[i] != py[i])
                        return false;

                  return true;
               }
            return false;
         }

         public static bool operator !=(ExtRawPolyData x, ExtRawPolyData y) {
            return x != y;
         }

         /// <summary>
         /// RawBound der Differenzen zum Mittelpunkt der zugehörigen Subdiv
         /// </summary>
         /// <returns>null, wenn keine Punkte x.</returns>
         public override Bound? GetRawBoundDelta() {
            Bound? rb = null;
            List<GeoDataBitstream.RawPoint> pts = GetRawPoints();
            if (pts.Count > 0) {
               rb = new Bound(pts[0].RawUnitsLon, pts[0].RawUnitsLat);
               for (int i = 1; i < pts.Count; i++)
                  rb.Embed(pts[i].RawUnitsLon, pts[i].RawUnitsLat);
            }
            return rb;
         }

         public override string ToString() {
            return base.ToString() + string.Format(", Länge Bitstream {0}", _bitstream != null ? _bitstream.Length : 0);
         }

      }

      #endregion

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }


      /// <summary>
      /// zur De- und Encodierung der geografischen Daten für Polylines und Polygones als Bitstream
      /// </summary>
      static public class GeoDataBitstream {

         /* Bitstream
          * 
          * Folge der Bits 0..7 vom ersten Byte, Bits 0..7 vom zweiten Byte usw.
          */

         /// <summary>
         /// Punkt in Garmin-Rohdaten (Differenzen zur Subdiv-Mitte und ohne Berücksichtigung einer Bitanzahl)
         /// </summary>
         public class RawPoint {

            /// <summary>
            /// Länge in RawUnits
            /// </summary>
            public int RawUnitsLon;

            /// <summary>
            /// Breite in RawUnits
            /// </summary>
            public int RawUnitsLat;


            public RawPoint(int lon = 0, int lat = 0) {
               RawUnitsLon = lon;
               RawUnitsLat = lat;
            }

            public RawPoint(RawPoint pt) {
               RawUnitsLon = pt.RawUnitsLon;
               RawUnitsLat = pt.RawUnitsLat;
            }

            /// <summary>
            /// erzeugt einen <see cref="RawPoint"/> aus dem <see cref="MapUnitPoint"/> mit der Bitanzahl und dem Mittelpunkt der Subdiv
            /// </summary>
            /// <param name="pt"></param>
            /// <param name="coordbits"></param>
            /// <param name="subdiv_center"></param>
            public RawPoint(MapUnitPoint pt, int coordbits, MapUnitPoint subdiv_center) {
               MapUnitPoint diff = pt - subdiv_center;
               RawUnitsLon = diff.LongitudeRawUnits(coordbits);
               RawUnitsLat = diff.LatitudeRawUnits(coordbits);
            }

            /// <summary>
            /// erzeugt einen <see cref="MapUnitPoint"/> mit der Bitanzahl und dem Mittelpunkt der Subdiv
            /// </summary>
            /// <param name="coordbits"></param>
            /// <param name="subdiv_center"></param>
            /// <returns></returns>
            public MapUnitPoint GetMapUnitPoint(int coordbits, MapUnitPoint subdiv_center) {
               MapUnitPoint p = new MapUnitPoint(RawUnitsLon, RawUnitsLat, coordbits); // Diff. zum Mittelpunkt der Subdiv
               p.Longitude += subdiv_center.Longitude;
               p.Latitude += subdiv_center.Latitude;
               return p;
            }

            public override string ToString() {
               return string.Format("RawUnitsLon {0}, RawUnitsLat {1}", RawUnitsLon, RawUnitsLat);
            }
         }

         enum SignType {
            /// <summary>
            /// unbekannt
            /// </summary>
            unknown,
            /// <summary>
            /// alle nichtnegativ
            /// </summary>
            allpos,
            /// <summary>
            /// alle negativ
            /// </summary>
            allneg,
            /// <summary>
            /// sowohl negativ als auch nichtnegativ
            /// </summary>
            different,
         }

         #region Hilfsfunktionen zum Decodieren des Bitstreams

         /// <summary>
         /// liest die Vorzeichenbehandlung für den Bitstream ein
         /// </summary>
         /// <param name="bitstream"></param>
         /// <returns>Anzahl der verwendeten Bits</returns>
         static int ReadBitstreamSigns(ref byte[] bitstream, out SignType lon_sign, out SignType lat_sign) {
            int bitstreampos = 0;

            lon_sign = SignType.different;
            if (GetBitFromByteArray(bitstreampos++, ref bitstream))     // 1-Bit -> gleiches Vorzeichen -> Art des Vorzeichen abfragen
               lon_sign = GetBitFromByteArray(bitstreampos++, ref bitstream) ? SignType.allneg : SignType.allpos;    // 1-Bit -> neg. Vorzeichen

            lat_sign = SignType.different;
            if (GetBitFromByteArray(bitstreampos++, ref bitstream))
               lat_sign = GetBitFromByteArray(bitstreampos++, ref bitstream) ? SignType.allneg : SignType.allpos;

            return bitstreampos;
         }

         /// <summary>
         /// testet, ob das Bit im Bytearray gesetzt ist
         /// <para>Die Bits zählen je Byte immer von 0 bis 7. Bit 9 ist z.B. das 2. Bit (1) im 2. Byte.</para>
         /// </summary>
         /// <param name="bitoffset"></param>
         /// <param name="bitstream"></param>
         /// <returns></returns>
         static bool GetBitFromByteArray(int bitoffset, ref byte[] bitstream) {
            byte b = bitstream[bitoffset / 8];  // Index des betroffenen Bytes
            b >>= bitoffset % 8;
            return (b & 0x01) != 0;
         }

         /* Für die Codierung werden              2, 3, 4, ... , 10, 11, 13, 15, ... Bits verwendet.
          * Als "BaseBits" werden dafür die Werte 0, 1, 2, ... ,  8,  9, 10, 11, ... verwendet.
          * Für die beiden BaseBits-Angaben steht nur 1 Byte zur Verfügung, d.h. es gilt immer BaseBits <= 15 (Bits <= 23)
          */

         /// <summary>
         /// Umrechnung der tatsächlich verwendeten Bits in die gespeicherte Bitangabe
         /// </summary>
         /// <param name="realbits">reale Bitanzahl</param>
         /// <returns></returns>
         static int BaseBits4RealBits(int realbits) {
            if (realbits <= 2)
               return 0;

            else if (realbits <= 11)
               // 3 -> 1
               // ...
               // 11 -> 9
               return realbits - 2;

            // 12 -> 10
            // 13 -> 10
            // 14 -> 11
            // 15 -> 11
            // 16 -> 12
            // ...
            return realbits / 2 + 4;
         }

         /// <summary>
         /// Umrechnung der gespeicherten Bitangabe in die tatsächlich verwendeten Bits
         /// </summary>
         /// <param name="basebits">gespeicherten Bitangabe</param>
         /// <returns></returns>
         static int RealBits4BaseBits(int basebits) {
            if (basebits <= 9)
               return basebits + 2;

            // 10 -> 13
            // 11 -> 15
            // 12 -> 17
            return 2 * basebits - 7;
         }

         /* Es wird im Prinzip die Standardcodierung für int-Zahlen verwendet. Der einzige Unterschied zu Int16 oder Int32 ist die vorgegebene max. Bitanzahl.
          * Wird kein variables Vorzeichen verwendet, handelt es sich einfach um unsigned Zahlen.
          * Andernfalls steht im höchstwertigen Bit das Vorzeichen (1 für +, 0 für -).
          * Codierung für n-Bit-Werte:    Wert(n-1) - Bitn * 2^(n-1)
          * z.B. n=5, var. Vorzeichen, val=12        b01100
          *      n=5, var. Vorzeichen, val=-12       b10100         4 - 16 = -12
          *      n=5, festes Vorzeichen, val=+-12  +-b01100
          *      
          * Ist NUR das höchstwertige Bit gesetzt (signed -2^(n-1) bzw. unsigned 2^(n-1)) liegt ein Spezialfall für größere Werte vor. 
          * Es werden so lange die nächsten n Bit ausgewertet, bis kein Spezialfall mehr vorliegt. Der dann ermittelte Wert wird um die Anzahl der Spezialfälle
          * multipliziert mit 2^(n-1)-1 vergrößert.
          */

         /// <summary>
         /// liefert den Wert aus den ersten n Bit
         /// <para>Wird long.MinValue geliefert, wurde der Spezialwert für eine Verlängerung des Bitbereiches gefunden.</para>
         /// </summary>
         /// <param name="bits">Bitmuster (ab Bit 0)</param>
         /// <param name="bitcount">Anzahl der gültigen Bits (Rest bleibt unberücksichtigt)</param>
         /// <param name="signed">als signed oder unsigned interpretieren</param>
         /// <returns></returns>
         static long GetNBitValue(ulong bits, int bitcount, bool signed) {
            int stdbits = signed ? bitcount - 1 : bitcount;
            long v = 0;

            for (int i = 0; i < stdbits; i++) {
               if ((bits & 0x1) != 0) {
#pragma warning disable CS0675 // Bitweiser OR-Operator, der bei einem signaturerweiterten Operanden verwendet wurde.
                  v |= 0x1 << i;
#pragma warning restore CS0675 // Bitweiser OR-Operator, der bei einem signaturerweiterten Operanden verwendet wurde.
               }
               bits >>= 1;
            }

            if (signed) {  // höchstwertiges Bit (Vorzeichen) auswerten
               if ((bits & 0x1) != 0) {      // neg. Vorzeichen
                  if (v != 0)
                     v -= 1 << (bitcount - 1);  // Wert(n-1) - 1 * 2^(n-1)
                  else
                     v = long.MinValue;      // Kennung für Spezialfall '-0'
               }
            }

            return v;
         }

         /// <summary>
         /// liefert den Bitbereich aus n Bits aus dem Bytearray als Zahl
         /// <para>Wenn der Bitbereich mit 1 oder mehreren Spezialwerten beginnt, werden jeweils die folgenden Bitreiche mitausgewertet. Dadurch kann die Gesamtlänge 
         /// des gelesenen Bereiches größer als ursprünglich gewünscht sein.</para>
         /// </summary>
         /// <param name="bitoffset">Nummer des Startbits im Array 0..</param>
         /// <param name="bits">Anzahl der zusammengehörenden Bits</param>
         /// <param name="reallength">Anzahl der tatsächlich berücksichtigten Bits (kann größer als <see cref="length"/> sein)</param>
         /// <param name="sign">Vorzeichentyp</param>
         /// <param name="bitstream">Byte-Array</param>
         /// <returns></returns>
         static int GetValueFromBytetArray(int bitoffset,
                                          int bits,
                                          ref int reallength,
                                          SignType sign,
                                          ref byte[] bitstream) {
            int byteoffset = bitoffset / 8;
            int bitinbyte = bitoffset % 8;

            if ((bitoffset + bits - 1) / 8 > bitstream.Length) { // Index des letzten benötigten Bytes zu groß: Fehler !!!
               reallength = -1;
               return 0;
            }

            // alle benötigten Bits in einer Var zusammenfassen
            ulong tmp;        // 64 Bit
            if (bitinbyte + bits > 32) {              // betrifft 5 Byte (z.Z. nicht möglich, da max 24 Bit verwendet werden; selbt bei ungünstiger Lage werden max. 4 Byte benötigt)
               tmp = (ulong)(bitstream[byteoffset] +
                            (bitstream[byteoffset + 1] << 8) +
                            (bitstream[byteoffset + 2] << 16) +
                            (bitstream[byteoffset + 3] << 24) +
                            (bitstream[byteoffset + 4] << 32));
            } else if (bitinbyte + bits > 24) {       // betrifft 4 Byte
               tmp = (ulong)(bitstream[byteoffset] +
                             (bitstream[byteoffset + 1] << 8) +
                             (bitstream[byteoffset + 2] << 16) +
                             (bitstream[byteoffset + 3] << 24));
            } else if (bitinbyte + bits > 16) {       // betrifft 3 Byte
               tmp = (ulong)(bitstream[byteoffset] +
                             (bitstream[byteoffset + 1] << 8) +
                             (bitstream[byteoffset + 2] << 16));
            } else if (bitinbyte + bits > 8) {        // betrifft 2 Byte
               tmp = (ulong)(bitstream[byteoffset] +
                             (bitstream[byteoffset + 1] << 8));
            } else {                                  // betrifft 1 Byte
               tmp = bitstream[byteoffset];
            }

            tmp >>= bitinbyte;   // Bitmuster fängt jetzt bei Bit an.

            reallength += bits;  // Anzahl der "verbrauchten" Bits

            if (sign != SignType.different) {                     // bei konstantem Vorzeichen wird der Wert entsprechend des Vorzeichens geliefert

               int tmp1 = (int)GetNBitValue(tmp, bits, false);
               return sign == SignType.allpos ?
                                    tmp1 :
                                    -tmp1;

            } else {

               long tmp1 = GetNBitValue(tmp, bits, true);

               if (tmp1 != long.MinValue)
                  return (int)tmp1;
               else {
                  int tmp2 = GetValueFromBytetArray(bitoffset + bits, bits, ref reallength, SignType.different, ref bitstream);
                  int tmp3 = (0x1 << (bits - 1)) - 1;
                  return tmp2 + (tmp2 >= 0 ? tmp3 : -tmp3);
               }

            }
         }

         #endregion

         /// <summary>
         /// liefert die Liste der Punkte (als Differenzwerte bezüglich des Subdiv-Mittelpunktes) aus dem aktuell gespeicherten Byte-Array
         /// <para>Es muß noch der Mittelpunkt des Subdivs und die Bitverschiebung berücksichtigt werden.</para>
         /// </summary>
         /// <param name="bitstream">Byte-Array</param>
         /// <param name="basebits4lon">Bit je Longitude (in codierter Form; Basebits)</param>
         /// <param name="basebits4lat">Bit je Latitude (in codierter Form; Basebits)</param>
         /// <param name="start_lon">Longitude für den Startpunkt</param>
         /// <param name="start_lat">Latitude für den Startpunkt</param>
         /// <param name="extrabit">Liste die die Extrabits aufnimmt (oder null)</param>
         /// <param name="extendedtype">true wenn es sich um Daten für einen extended Typ handelt</param>
         /// <returns></returns>
         static public List<RawPoint> GetRawPoints(ref byte[]? bitstream,
                                                   int basebits4lon,
                                                   int basebits4lat,
                                                   int start_lon,
                                                   int start_lat,
                                                   List<bool>? extrabit,
                                                   bool extendedtype) {
            List<RawPoint> rawpoints = new List<RawPoint>();

            if (bitstream != null && bitstream.Length > 0) {
               int bitstreampos = ReadBitstreamSigns(ref bitstream, out SignType lon_sign, out SignType lat_sign);

               /* MKGMAP probiert in einer Funktion makeBitStream() ausgehend von theoretisch nötigen Werten die besten Werte für basebits4lon / basebits4lat aus.
               * Wegene des Spezialwertes '-0' können kleinere Werte u.U. zu einem kürzeren Bitstream führen.
               * Falls lon und/oder lat ein individuelles Vorzeichen haben, wird basebits4lon und/oder basebits4lat vorher noch inkrementiert!
               */

               if (extendedtype)
                  GetBitFromByteArray(bitstreampos++, ref bitstream);        // unklar; immer 0?

               if (extrabit != null)
                  extrabit.Add(GetBitFromByteArray(bitstreampos++, ref bitstream));

               int bitstreamoffset = bitstreampos;
               int bits = 8 * bitstream.Length;
               int bits4Longitude = RealBits4BaseBits(basebits4lon);
               int bits4Latitude = RealBits4BaseBits(basebits4lat);
               if (lon_sign == SignType.different)
                  bits4Longitude++;
               if (lat_sign == SignType.different)
                  bits4Latitude++;
               int bits4point = bits4Longitude + bits4Latitude + (extrabit != null ? 1 : 0);

               // The starting point of the polyline and polygon are defined by longitude_delta and latitude_delta.
               rawpoints.Add(new RawPoint(start_lon, start_lat));

               int reallength;
               while (bitstreamoffset + bits4point <= bits) {
                  reallength = 0;
                  int lon = GetValueFromBytetArray(bitstreamoffset, bits4Longitude, ref reallength, lon_sign, ref bitstream);
                  bitstreamoffset += reallength;
                  if (reallength <= 0) {
                     bitstreamoffset += 2 * reallength;
                     Debug.WriteLine("Längenüberschreitung bei bitstreamoffset={0} für Lon: {1}",
                                       bitstreamoffset,
                                       GetBitStreamString(ref bitstream, basebits4lon, basebits4lat, extrabit != null));
                     break;
                  }

                  reallength = 0;
                  int lat = GetValueFromBytetArray(bitstreamoffset, bits4Latitude, ref reallength, lat_sign, ref bitstream);
                  bitstreamoffset += reallength;
                  if (reallength <= 0) {
                     bitstreamoffset += 2 * reallength;
                     Debug.WriteLine("Längenüberschreitung bei bitstreamoffset={0} für Lat: {1}",
                                       bitstreamoffset,
                                       GetBitStreamString(ref bitstream, basebits4lon, basebits4lat, extrabit != null));
                     break;
                  }

                  if (extrabit != null)
                     extrabit.Add(GetBitFromByteArray(bitstreamoffset++, ref bitstream));

                  // Each point in a poly object is defined relative to the previous point.
                  rawpoints.Add(new RawPoint(rawpoints[rawpoints.Count - 1].RawUnitsLon + lon, rawpoints[rawpoints.Count - 1].RawUnitsLat + lat));
               }
#if DEBUG
               // restliche Bits prüfen; sollten 0 sein
               bool err = false;
               StringBuilder restbits = new StringBuilder();
               while (bitstreamoffset < bits) {
                  if (GetBitFromByteArray(bitstreamoffset++, ref bitstream)) {
                     err = true;
                     restbits.Append("1");
                  } else
                     restbits.Append("0");
               }
               if (err)
                  Debug.WriteLine("vermutlich Fehler bei den \"Restbits\" " + restbits.ToString() + ": " + GetBitStreamString(ref bitstream, basebits4lon, basebits4lat, extrabit != null));
#endif
            }

            return rawpoints;
         }

         #region Hilfsfunktionen zum Codieren des Bitstreams

         /// <summary>
         /// liefert die Anzahl der nötigen Bits um den Wert (ohne Vorzeichen!) zu codieren 
         /// </summary>
         /// <param name="val"></param>
         /// <returns></returns>
         static int BitsNeeded(int val) {
            int bits = val < 0 ? 1 : 0;
            if (val < 0)
               val = -val; // nur noch abs. Teil untersuchen
            while (val != 0) {
               val >>= 1;
               bits++;
            }
            return bits;
         }

         static int SetNBitValue(uint bits, int bitcount, int bitoffset, List<byte> bitstream) {
            uint val = bits & (uint)((1 << bitcount) - 1);   // begrenzt val auf die zulässige Bitanzahl
            int n = bitcount;

            while (n > 0) {
               int byteidx = bitoffset / 8;
               int bitno = bitoffset % 8;

               while (bitstream.Count <= byteidx)
                  bitstream.Add(0);
               bitstream[byteidx] |= (byte)((val << bitno) & 0xff);

               val >>= 8 - bitno; // nächstes Byte

               int nput = 8 - bitno;
               if (nput > n)
                  nput = n;
               bitoffset += nput;
               n -= nput;
            }

            return bitoffset;
         }

         static int SetNBitSignedValue(int bits, int bitcount, int bitoffset, List<byte> bitstream) {
            int top = 1 << (bitcount - 1);      // Maske für Vorzeichen
            int mask = top - 1;           // Maske für Wert-Bits
            int val = bits;
            if (val < 0)
               val = -val;

            while (val > mask) { // solange der Wertebereich überschritten ist
               bitoffset = SetNBitValue((uint)top, bitcount, bitoffset, bitstream);
               val -= mask;
            }
            if (bits < 0) {      // für neg. Werte
               bitoffset = SetNBitValue((uint)((top - val) | top), bitcount, bitoffset, bitstream);   // Codierung neg. Werte:  Wert(n-1) - Bitn * 2^(n-1)       Bitn=[0,1]
            } else {             // für nichtneg. Werte
               bitoffset = SetNBitValue((uint)val, bitcount, bitoffset, bitstream);
            }

            return bitoffset;
         }

         static int Set1Bit(int bitoffset, List<byte> bitstream) {
            return SetNBitValue(0x1, 1, bitoffset, bitstream);
         }

         /// <summary>
         /// erzeugt die Liste der Deltawerte (der 1. Originalpunkt ist der Bezugspunkt), ermittelt Minima und Maxima und die notwendige Vorzeichenregelung
         /// </summary>
         /// <param name="pt"></param>
         /// <param name="lon_sign"></param>
         /// <param name="lat_sign"></param>
         /// <param name="minlon"></param>
         /// <param name="maxlon"></param>
         /// <param name="minlat"></param>
         /// <param name="maxlat"></param>
         /// <returns></returns>
         static List<RawPoint> GetDeltaAndSignAndMinMax(IList<RawPoint> pt, out SignType lon_sign, out SignType lat_sign, out int minlon, out int maxlon, out int minlat, out int maxlat) {
            lon_sign = SignType.unknown;
            lat_sign = SignType.unknown;
            // Vorzeichenwechsel testen und Min/Max ermitteln
            minlon = int.MaxValue;
            maxlon = int.MinValue;
            minlat = int.MaxValue;
            maxlat = int.MinValue;
            List<RawPoint> delta = new List<RawPoint>();
            for (int i = 1; i < pt.Count; i++) {
               int lon_delta = pt[i].RawUnitsLon - pt[i - 1].RawUnitsLon;
               int lat_delta = pt[i].RawUnitsLat - pt[i - 1].RawUnitsLat;

               switch (lon_sign) {
                  case SignType.unknown:
                     lon_sign = lon_delta >= 0 ? SignType.allpos : SignType.allneg;
                     break;

                  case SignType.allneg:
                     if (lon_delta >= 0)
                        lon_sign = SignType.different;
                     break;

                  case SignType.allpos:
                     if (lon_delta < 0)
                        lon_sign = SignType.different;
                     break;
               }

               switch (lat_sign) {
                  case SignType.unknown:
                     lat_sign = lat_delta >= 0 ? SignType.allpos : SignType.allneg;
                     break;

                  case SignType.allneg:
                     if (lat_delta >= 0)
                        lat_sign = SignType.different;
                     break;

                  case SignType.allpos:
                     if (lat_delta < 0)
                        lat_sign = SignType.different;
                     break;
               }

               minlon = Math.Min(minlon, lon_delta);
               maxlon = Math.Max(maxlon, lon_delta);
               minlat = Math.Min(minlat, lat_delta);
               maxlat = Math.Max(maxlat, lat_delta);

               delta.Add(new RawPoint(lon_delta, lat_delta));        // Diff. speichern
            }
            return delta;
         }

         /// <summary>
         /// erzeugt einen Bitstream für die entsprechenden Werte
         /// </summary>
         /// <param name="delta">Differenzwerte (bezogen auf den Mittelpunktes des Subdivs)</param>
         /// <param name="basebits4lon">Basiswert für Bitanzahl</param>
         /// <param name="basebits4lat">Basiswert für Bitanzahl</param>
         /// <param name="extra">Liste der Extrabits</param>
         /// <param name="extendedtype"></param>
         /// <returns></returns>
         static List<byte> buildBitstreamBuffer(IList<RawPoint> delta,
                                                int basebits4lon,
                                                int basebits4lat,
                                                SignType lon_sign,
                                                SignType lat_sign,
                                                IList<bool>? extra,
                                                bool extendedtype) {
            // Bit-Anzahl ev. regelkonform machen
            int bits4lon = RealBits4BaseBits(basebits4lon);
            int bits4lat = RealBits4BaseBits(basebits4lat);

            if (lon_sign == SignType.different)
               bits4lon++;       // wegen Vorzeichen
            if (lat_sign == SignType.different)
               bits4lat++;

            bool bWithExtraBit = extra != null &&
                                 extra.Count == delta.Count;

            int bitstreampos = 0;
            List<byte> bitstream = new List<byte>();

            // Vorzeichenregeln speichern
            if (lon_sign != SignType.different) {
               Set1Bit(bitstreampos++, bitstream);
               if (lon_sign == SignType.allneg)
                  Set1Bit(bitstreampos++, bitstream);
               else
                  bitstreampos++;
            } else
               bitstreampos++;

            if (lat_sign != SignType.different) {
               Set1Bit(bitstreampos++, bitstream);
               if (lat_sign == SignType.allneg)
                  Set1Bit(bitstreampos++, bitstream);
               else
                  bitstreampos++;
            } else
               bitstreampos++;

            if (extendedtype)
               bitstreampos++;

            if (bWithExtraBit)
               bitstreampos++;

            // Werte speichern
            for (int i = 0; i < delta.Count; i++) {

               if (lon_sign != SignType.different)
                  bitstreampos = SetNBitValue((uint)Math.Abs(delta[i].RawUnitsLon), bits4lon, bitstreampos, bitstream);
               else
                  bitstreampos = SetNBitSignedValue(delta[i].RawUnitsLon, bits4lon, bitstreampos, bitstream);

               if (lat_sign != SignType.different)
                  bitstreampos = SetNBitValue((uint)Math.Abs(delta[i].RawUnitsLat), bits4lat, bitstreampos, bitstream);
               else
                  bitstreampos = SetNBitSignedValue(delta[i].RawUnitsLat, bits4lat, bitstreampos, bitstream);

               if (bWithExtraBit && extra != null)
                  if (extra[i + 1])
                     Set1Bit(bitstreampos++, bitstream);
                  else
                     bitstreampos++;
            }

            return bitstream;
         }

         /// <summary>
         /// nur für Analysezwecke: Umwandlung des Bitstreams in eine Zeichenkette
         /// </summary>
         /// <param name="bitstream"></param>
         /// <param name="basebits4lon"></param>
         /// <param name="basebits4lat"></param>
         /// <param name="extrabit"></param>
         /// <returns></returns>
         static public string GetBitStreamString(ref byte[] bitstream, int basebits4lon, int basebits4lat, bool extrabit = false) {
            StringBuilder sb = new StringBuilder();

            if (bitstream != null && bitstream.Length > 0) {
               for (int i = 0; i < bitstream.Length; i++) {
                  sb.Append((bitstream[i] & 0x01) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x02) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x04) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x08) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x10) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x20) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x40) > 0 ? "1" : "0");
                  sb.Append((bitstream[i] & 0x80) > 0 ? "1" : "0");
               }

               int insertpos = ReadBitstreamSigns(ref bitstream, out SignType lon_sign, out SignType lat_sign);
               sb.Insert(insertpos++, ":");

               if (extrabit) {
                  sb.Insert(insertpos++, "[");
                  insertpos++;
                  sb.Insert(insertpos++, "]");
               }

               int bits4Longitude = RealBits4BaseBits(basebits4lon);
               int bits4Latitude = RealBits4BaseBits(basebits4lat);
               if (lon_sign == SignType.different)
                  bits4Longitude++;
               if (lat_sign == SignType.different)
                  bits4Latitude++;

               while (insertpos < sb.Length) {
                  insertpos += bits4Longitude;
                  if (insertpos < sb.Length)
                     sb.Insert(insertpos++, " ");
                  insertpos += bits4Latitude;
                  if (extrabit) {
                     if (insertpos < sb.Length)
                        sb.Insert(insertpos++, "[");
                     insertpos++;
                     if (insertpos < sb.Length)
                        sb.Insert(insertpos++, "]");
                  }
                  if (insertpos < sb.Length)
                     sb.Insert(insertpos++, ",");
               }

               sb.Insert(0,
                         string.Format("Bits Lon {0}->{1} Lat {2}->{3}, Vorzeichen {4}{5}, ",
                                        basebits4lon,
                                        bits4Longitude,
                                        basebits4lat,
                                        bits4Latitude,
                                        lon_sign == SignType.different ? "*" : lon_sign == SignType.allpos ? "+" : "-",
                                        lat_sign == SignType.different ? "*" : lat_sign == SignType.allpos ? "+" : "-"));
            }
            return sb.ToString();
         }

         #endregion

         /// <summary>
         /// setzt die Punkte (Differenzwerte bezüglich des Mittelpunktes des Subdivs und mit korrekter Bitverschiebung) im Byte-Array
         /// </summary>
         /// <param name="pt">Punkte</param>
         /// <param name="basebits4lon">nimmt die Anzahl der verwendeten Bits je Longitude auf (in codierter Form; Basebits)</param>
         /// <param name="basebits4lat">nimmt die Anzahl der verwendeten Bits je Latitude auf (in codierter Form; Basebits)</param>
         /// <param name="extra">Extrabits je Punkt oder null</param>
         /// <param name="extendedtype">true wenn es sich um Daten für einen extended Typ handelt</param>
         /// <returns></returns>
         static public byte[]? SetRawPoints(IList<RawPoint> pt, out int basebits4lon, out int basebits4lat, IList<bool>? extra, bool extendedtype) {
            basebits4lon = basebits4lat = 0;

            if (0xFFFF < Math.Abs(pt[0].RawUnitsLon) ||    // nur UInt16 möglich
                0xFFFF < Math.Abs(pt[0].RawUnitsLat) ||
                pt.Count < 2) // zu wenig Punkte
               return null;

            List<RawPoint> delta = GetDeltaAndSignAndMinMax(pt, out SignType lon_sign, out SignType lat_sign, out int minlon, out int maxlon, out int minlat, out int maxlat);

            // max. nötige Bitanzahl (ohne Vorzeichen) bestimmen
            int bits4lon = Math.Max(BitsNeeded(minlon), BitsNeeded(maxlon));
            int bits4lat = Math.Max(BitsNeeded(minlat), BitsNeeded(maxlat));

            basebits4lon = BaseBits4RealBits(bits4lon);
            basebits4lat = BaseBits4RealBits(bits4lat);

            List<byte> bs_best = buildBitstreamBuffer(delta, basebits4lon, basebits4lat, lon_sign, lat_sign, extra, extendedtype); // Standardcodierung

            if (lon_sign == SignType.different) // ev. kürzerer Bitstream möglich
               for (int bb4lon = basebits4lon - 1; bb4lon >= 0; bb4lon--) {
                  List<byte> bs = buildBitstreamBuffer(delta, bb4lon, basebits4lat, lon_sign, lat_sign, extra, extendedtype);
                  if (bs.Count < bs_best.Count) {
                     bs_best = bs;
                     basebits4lon = bb4lon;
                  } else
                     break;
               }

            if (lat_sign == SignType.different) // ev. kürzerer Bitstream möglich
               for (int bb4lat = basebits4lat - 1; bb4lat >= 0; bb4lat--) {
                  List<byte> bs = buildBitstreamBuffer(delta, basebits4lon, bb4lat, lon_sign, lat_sign, extra, extendedtype);
                  if (bs.Count < bs_best.Count) {
                     bs_best = bs;
                     basebits4lat = bb4lat;
                  } else
                     break;
               }

            return bs_best.ToArray();
         }

         #region zum Test der Bitstreamcodierung und -decodierung

#if DEBUG
         static public void SimpleTest() {

            try {

               //List<RawPoint> orgpt = new List<GeoDataBitstream.RawPoint>();
               //orgpt.Add(new RawPoint(8, 30));
               //orgpt.Add(new RawPoint(6, 30));
               //SimpleTest(orgpt);

               //List<RawPoint> orgpt = new List<GeoDataBitstream.RawPoint>();
               //orgpt.Add(new RawPoint(4, 36));
               //orgpt.Add(new RawPoint(11, 47));
               //orgpt.Add(new RawPoint(49, 43));
               //SimpleTest(orgpt);

               SimpleTest(50, 1);
               for (int i = 0; i < 10000000; i++)
                  SimpleTest(1500, 0);

            } catch (Exception ex) {
               throw new Exception(ex.Message);
            }

         }

         static void SimpleTest(int absmax = 50, int seed = 0) {
            // zufällige RawPoints-Liste
            Random r = seed != 0 ?
                           new Random(seed) :
                           new Random(); // Init. mit der Zeit

            int l = 2 + r.Next(100);
            List<RawPoint> orgpt = new List<RawPoint>();
            for (int i = 0; i < l; i++)
               orgpt.Add(new RawPoint(r.Next(absmax), r.Next(absmax)));

            SimpleTest(orgpt);
         }

         static void SimpleTest(List<RawPoint> orgpt) {
            byte[]? encoded = SetRawPoints(orgpt, out int basebits4lon_org, out int basebits4lat_org, null, false);
            if (encoded == null)
               return;
            List<RawPoint> decodedpt = GetRawPoints(ref encoded, basebits4lon_org, basebits4lat_org, orgpt[0].RawUnitsLon, orgpt[0].RawUnitsLat, null, false);

            // Vergleich
            if (orgpt.Count != decodedpt.Count) {  // darf nur durch restliche 0-Bits im letzten Byte entstehen -> nur 0-Differenzen
               bool only0 = false;
               if (orgpt.Count < decodedpt.Count) {
                  int lastorgidx = orgpt.Count - 1;
                  only0 = true;
                  for (int i = lastorgidx + 1; i < decodedpt.Count; i++) {
                     if (decodedpt[lastorgidx].RawUnitsLon != decodedpt[i].RawUnitsLon ||
                         decodedpt[lastorgidx].RawUnitsLat != decodedpt[i].RawUnitsLat) {
                        only0 = false;
                        break;
                     }
                  }
               }
               if (!only0) {
                  string.Format("orgpt.Count <> decodedpt.Count ({0} <> {1})", orgpt.Count, decodedpt.Count);
                  Debug.WriteLine("orgpt.Count={0}, basebits4lon_org={1}, basebits4lat_org={2}", orgpt.Count, basebits4lon_org, basebits4lat_org);
                  for (int j = 0; j < orgpt.Count; j++)
                     Debug.WriteLine(orgpt[j].ToString());
                  return;
               }
            }

            for (int i = 0; i < orgpt.Count && i < decodedpt.Count; i++) {
               if (orgpt[i].RawUnitsLon != decodedpt[i].RawUnitsLon ||
                   orgpt[i].RawUnitsLat != decodedpt[i].RawUnitsLat) {
                  Debug.WriteLine("orgpt[{0}] = {1}  <>  decodedpt[{0}] = {2}", i, orgpt[i].ToString(), decodedpt[i].ToString());
                  Debug.WriteLine("orgpt.Count={0}, basebits4lon={1}, basebits4lat={2}", orgpt.Count, basebits4lon_org, basebits4lat_org);
                  for (int j = 0; j < orgpt.Count; j++)
                     Debug.WriteLine(orgpt[j].ToString());
                  break;
               }
            }
         }

#endif

         #endregion

      }


      /// <summary>
      /// übergeordnete TRE-Datei (im Konstruktor gesetzt)
      /// </summary>
      public StdFile_TRE TREFile { get; private set; }

      /// <summary>
      /// Liste aller Subdivs mit ihren Daten
      /// </summary>
      public List<SubdivData?> SubdivList { get; private set; }



      public StdFile_RGN(StdFile_TRE tre) : base("RGN") {
         Headerlength = 0x7D;

         TREFile = tre;
         SubdivList = new List<SubdivData?>();
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         Filesections.ClearSections();

         SubdivContentBlock = new DataBlock(br);

         // --------- Headerlänge > 29 Byte

         if (Headerlength > 0x1D) {
            ExtAreasBlock = new DataBlock(br);
            br.ReadBytes(Unknown_0x25);
            ExtLinesBlock = new DataBlock(br);
            br.ReadBytes(Unknown_0x41);
            ExtPointsBlock = new DataBlock(br);
            br.ReadBytes(Unknown_0x5D);
            UnknownBlock_0x71 = new DataBlock(br);
            br.ReadBytes(Unknown_0x79);

         }
      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         Filesections.AddSection((int)InternalFileSections.SubdivContentBlock, new DataBlock(SubdivContentBlock));
         Filesections.AddSection((int)InternalFileSections.ExtAreasBlock, new DataBlock(ExtAreasBlock));
         Filesections.AddSection((int)InternalFileSections.ExtLinesBlock, new DataBlock(ExtLinesBlock));
         Filesections.AddSection((int)InternalFileSections.ExtPointsBlock, new DataBlock(ExtPointsBlock));
         //Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x71, new DataBlock(UnknownBlock_0x71));

         // GapOffset und DataOffset setzen
         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);

         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            PostHeaderDataBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, PostHeaderDataBlock);
         }

         // Datenblöcke einlesen
         Filesections.ReadSections(br);
      }

      /// <summary>
      /// wird bei jedem Read(), dass nicht nur ein RawRead ist, aufgerufen
      /// </summary>
      protected override void DecodeSections() {
         if (Locked != 0) {
            RawRead = true;
            return;
         }

         // Datenblöcke "interpretieren"
         int filesectiontype;
         DataBlockWithRecordsize? tmpblrs;

         if (TREFile == null)
            throw new Exception("Ohne dazugehörende TRE-Datei können keine Subdiv-Infos gelesen werden.");

         while (SubdivList.Count < TREFile.SubdivInfoList.Count)    // ev. Standardliste mit null für jede Subdiv erzeugen
            SubdivList.Add(null);

         // alle Subdiv-Daten "interpretieren"
         List<StdFile_TRE.SubdivInfoBasic> subdivinfoList = TREFile.SubdivInfoList;
         if (subdivinfoList != null &&
             subdivinfoList.Count > 0) {

            filesectiontype = (int)InternalFileSections.SubdivContentBlock;

            // Die Offsets für den zugehörigen Datenbereich für jedes Subdiv sind zwar schon gesetzt, aber die Länge der entsprechenden Blöcke fehlt noch.
            // Die letzte Subdiv nimmt den Rest des gesamten Datenblocks ein.
            //for (int i = 0; i < subdivinfoList.Count - 1; i++)
            //   subdivinfoList[i].Data.Length = subdivinfoList[i + 1].Data.Offset - subdivinfoList[i].Data.Offset;
            //subdivinfoList[subdivinfoList.Count - 1].Data.Length = Filesections.GetLength(filesectiontype) - subdivinfoList[subdivinfoList.Count - 1].Data.Offset;

            tmpblrs = Filesections.GetPosition(filesectiontype);
            if (tmpblrs != null && tmpblrs.Length > 0) {
               tmpblrs = new DataBlockWithRecordsize(tmpblrs) {
                  Offset = 0
               };
               Decode_SubdivContentBlock(Filesections.GetSectionDataReader(filesectiontype), tmpblrs);
               Filesections.RemoveSection(filesectiontype);
            }
         } //else subdivinfoList = new List<StdFile_TRE.SubdivInfoBasic>();
         // throw new Exception("Die TRE-Datei enthält keine Subdiv-Infos.");

         // alle Ext-Daten "interpretieren"
         filesectiontype = (int)InternalFileSections.ExtAreasBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_ExtAreasBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.ExtLinesBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_ExtLinesBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.ExtPointsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_ExtPointsBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }
      }

      /// <summary>
      /// Liste der gültigen Subdiv-Index bei unvollständigem Einlesen
      /// </summary>
      int[]? validsubdividx = new int[0];

      /// <summary>
      /// erzeugt eine (neue) Subdiv-Liste mit den Daten der Subdivs
      /// <para>Für die nicht gewünschten Subdivs steht null in der Liste.</para>
      /// <para>NICHT multithreadsicher!</para>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="subdividxlst">Indexe der gewünschtenSubdivs oder null (alles einlesen)</param>
      /// <param name="tre"><see cref="TREFile"/> kann, wenn ungleich null, neu gesetzt werden</param>
      public void ReadOnlySpecialSubdivs(BinaryReaderWriter br, IList<int> subdividxlst, StdFile_TRE? tre = null) {
         if (subdividxlst != null) {
            List<int> tmp = new List<int>();
            foreach (var item in subdividxlst)
               if (SubdivList.Count <= item ||
                   SubdivList[item] == null)    // noch nicht eingelesen
                  tmp.Add(item);
            validsubdividx = new int[tmp.Count];
            tmp.CopyTo(validsubdividx, 0);
         } else
            validsubdividx = null;

         if (tre != null)
            TREFile = tre;

         Read(br);      // löst DecodeSections() aus

         validsubdividx = null;
      }

      #region Decodierung der Datenblöcke

      /// <summary>
      /// liefert den nächsten gültigen Index, der in der <see cref="SubdivList"/> nicht null ergibt
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="subdivIdx"></param>
      /// <returns>kleiner 0, wenn kein gültiger Index mehr ex.</returns>
      int getNextValidIdx(int idx, int[] subdivIdx) {
         while (idx + 1 < subdivIdx.Length &&
                SubdivList[subdivIdx[++idx]] == null) ;
         return idx < subdivIdx.Length &&
                subdivIdx[idx] < SubdivList.Count &&
                SubdivList[subdivIdx[idx]] != null ? idx : -1;
      }

      void Decode_SubdivContentBlock(BinaryReaderWriter? br, DataBlock src) { //, bool selftest = false) {
         if (br != null) {
            List<StdFile_TRE.SubdivInfoBasic> subdivinfoList = TREFile.SubdivInfoList;
            // Länge und Inhalt als Zusatzdaten liefern
            object[] extdata = new object[subdivinfoList.Count];
            //uint start = 0;
            for (int i = 0; i < subdivinfoList.Count; i++) {
               //if (selftest) {

               //   // !!! Für NT-Karten scheint das sinnlos zu sein. Vermutlich sind Punkte und Linien auf eine andere Art kodiert als bisher. !!!

               //   DataBlock block = new DataBlock(src.Offset + start, subdivinfoList[i].Data.Length);
               //   start += subdivinfoList[i].Data.Length;
               //   List<StdFile_TRE.SubdivInfoBasic.SubdivContent> contentlst = new Subdiv().ContentTest(br, block);

               //   if (contentlst.Count == 0)
               //      subdivinfoList[i].Content = StdFile_TRE.SubdivInfoBasic.SubdivContent.nothing;
               //   else {
               //      subdivinfoList[i].Content = contentlst[0];
               //      Debug.WriteLineIf(contentlst.Count > 1, string.Format("Inhalt der Subdiv nicht eindeutig erkannt ({0} Möglichkeiten).", contentlst.Count));
               //   }
               //}

               extdata[i] = subdivinfoList[i].Data.Length | ((uint)subdivinfoList[i].Content << 24);
            }
            if (validsubdividx == null ||
                validsubdividx.Length == 0)   // Standard -> alles einlesen
               SubdivList = br.ReadArray<SubdivData?>(src, extdata);
            else {
               while (SubdivList.Count < subdivinfoList.Count) // ev. Standardliste mit null für jede Subdiv erzeugen
                  SubdivList.Add(null);

               foreach (int idx in validsubdividx) { // die gewünschten Subdivs einlesen
                  if (SubdivList[idx] == null) {         // tatsächlich noch nicht eingelesen
                     List<SubdivData> lst;
                     if (subdivinfoList[idx].Data.Length > 0)
                        lst = br.ReadArray<SubdivData>(new DataBlock(subdivinfoList[idx].Data.Offset + src.Offset, subdivinfoList[idx].Data.Length), new object[] { extdata[idx] });
                     else
                        lst = new List<SubdivData>() { new SubdivData() }; // keine Daten -> leere Subdiv
                     SubdivList[idx] = lst[0];
                  }
               }
            }
         }
      }

      void Decode_ExtAreasBlock(BinaryReaderWriter? br, DataBlock src) {
         long startadr = src.Offset;
         long endpos = src.Offset + src.Length;

         // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Polygone enthalten
         int[] SubdivIdx = new int[TREFile.ExtAreaBlock4Subdiv.Count];
         TREFile.ExtAreaBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0); // die Schlüssel sind die Subdiv-Indexe

         // ??? unklar, ob das IMMER 1-basiert ist ???
         //for (int i = 0; i < SubdivIdx.Length; i++)  // 1-basierten Index in 0-basierten Index umwandeln
         //   SubdivIdx[i]--;

         if (br != null) {
            int idx = -1;
            int subdividx;
            DataBlock tre_block;
            long blockend = br.Position; // Blockende simulieren
            List<ExtRawPolyData>? lst = null;

            br.Seek(startadr);
            while (br.Position < endpos) {
               if (br.Position >= blockend) {          // alles für die aktuelle Subdiv eingelesen
                  if (br.Position > blockend)
                     throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Flächen) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
                  if (idx > SubdivIdx.Length - 2 ||
                      SubdivIdx[idx + 1] > SubdivList.Count - 1) {
                     Debug.WriteLine("Ev. Fehler beim Einlesen der ExtPolyData (Flächen)");  // Bei einer Originalkarte wurde beobachtet, dass Daten für eine nicht ex. Subdiv enthalten waren.
                     break;
                  }

                  // Vorbereitung für nächste Subdiv
                  idx = getNextValidIdx(idx, SubdivIdx);       //subdividx = SubdivIdx[++idx];
                  if (idx < 0)   // keine Subdiv mehr
                     break;
                  subdividx = SubdivIdx[idx];
                  if (SubdivList[subdividx] == null)
                     throw new Exception("Decode_ExtAreasBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");
                  tre_block = TREFile.ExtAreaBlock4Subdiv[subdividx];
                  br.Seek(tre_block.Offset - startadr);
                  blockend = tre_block.Offset + tre_block.Length - startadr;

                  if (validsubdividx != null &&
                      validsubdividx.Length > 0 &&
                      !validsubdividx.Contains(subdividx)) {
                     br.Seek(blockend);                     // Daten dieser Subdiv überspringen
                     continue;
                  }

                  SubdivData? sd = SubdivList[subdividx];
                  if (sd != null) {
                     lst = sd.ExtAreaList;
                     lst.Clear();
                  }
               }
               lst?.Add(new ExtRawPolyData(br));
            }
         }
      }

      void Decode_ExtLinesBlock(BinaryReaderWriter? br, DataBlock src) {
         long startadr = src.Offset;
         long endpos = src.Offset + src.Length;

         // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Polygone enthalten
         int[] SubdivIdx = new int[TREFile.ExtLineBlock4Subdiv.Count];
         TREFile.ExtLineBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0);

         if (br != null) {
            int idx = -1;
            int subdividx;
            DataBlock tre_block;
            long blockend = br.Position; // Blockende simulieren
            List<ExtRawPolyData>? lst = null;

            br.Seek(startadr);
            while (br.Position < endpos) {
               if (br.Position >= blockend) {
                  if (br.Position > blockend)
                     throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Linien) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
                  if (idx > SubdivIdx.Length - 2 ||
                      SubdivIdx[idx + 1] > SubdivList.Count - 1) {
                     Debug.WriteLine("Ev. Fehler beim Einlesen der ExtPolyData (Linien)");  // Bei einer Originalkarte wurde beobachtet, dass Daten für eine nicht ex. Subdiv enthalten waren.
                     break;
                  }

                  // Vorbereitung für nächste Subdiv
                  idx = getNextValidIdx(idx, SubdivIdx);       //subdividx = SubdivIdx[++idx];
                  if (idx < 0)   // keine Subdiv mehr
                     break;
                  subdividx = SubdivIdx[idx];
                  if (SubdivList[subdividx] == null)
                     throw new Exception("Decode_ExtLinesBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");
                  tre_block = TREFile.ExtLineBlock4Subdiv[subdividx];
                  br.Seek(tre_block.Offset - startadr);
                  blockend = tre_block.Offset + tre_block.Length - startadr;

                  if (validsubdividx != null &&
                      validsubdividx.Length > 0 &&
                      !validsubdividx.Contains(subdividx)) {
                     br.Seek(blockend);                     // Daten dieser Subdiv überspringen
                     continue;
                  }

                  SubdivData? sd = SubdivList[subdividx];
                  if (sd != null) {
                     lst = sd.ExtLineList;
                     lst.Clear();
                  }
               }
               lst?.Add(new ExtRawPolyData(br));
            }
         }
      }

      void Decode_ExtPointsBlock(BinaryReaderWriter? br, DataBlock src) {
         long startadr = src.Offset;
         long endpos = src.Offset + src.Length;

         // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Punkte enthalten
         int[] SubdivIdx = new int[TREFile.ExtPointBlock4Subdiv.Count];
         TREFile.ExtPointBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0);

         if (br != null) {
            int idx = -1;
            int subdividx;
            DataBlock tre_block;
            long blockend = br.Position; // Blockende simulieren
            List<ExtRawPointData>? lst = null;

            br.Seek(startadr);
            while (br.Position < endpos) {
               //if (br.Position > blockend - 6) {      // min. 6 Byte sind für einen Punkt nötig; jetzt neue Subdiv
               if (br.Position >= blockend) {
                  if (br.Position > blockend)
                     throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Points) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
                  //if (br.Position < blockend)
                  //   Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches nicht erreicht. Noch " + (blockend - br.Position).ToString() + " Bytes übrig.");
                  //else if (blockend < br.Position)
                  //   Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches um " + (br.Position - blockend).ToString() + " Bytes überschritten.");

                  // Vorbereitung für nächste Subdiv
                  idx = getNextValidIdx(idx, SubdivIdx);       //subdividx = SubdivIdx[++idx];
                  if (idx < 0)   // keine Subdiv mehr
                     break;
                  subdividx = SubdivIdx[idx];
                  if (SubdivList[subdividx] == null)
                     throw new Exception("Decode_ExtPointsBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");
                  tre_block = TREFile.ExtPointBlock4Subdiv[subdividx];
                  br.Seek(tre_block.Offset - startadr);
                  blockend = tre_block.Offset + tre_block.Length - startadr;

                  if (validsubdividx != null &&
                      validsubdividx.Length > 0 &&
                      !validsubdividx.Contains(subdividx)) {
                     br.Seek(blockend);                     // Daten dieser Subdiv überspringen
                     continue;
                  }

                  SubdivData? sd = SubdivList[subdividx];
                  if (sd != null) {
                     lst = sd.ExtPointList;
                     lst.Clear();
                  }
               }
               lst?.Add(new ExtRawPointData(br));

               //if (lst[lst.Count - 1].HasUnknownFlag) {
               //   if (lst[lst.Count - 1].UnknownKey[0] == 0x41) {

               //   } else if (lst[lst.Count - 1].UnknownKey[0] == 0x03 &&
               //              lst[lst.Count - 1].UnknownKey[2] == 0x5A) {

               //   } else {

               //      Debug.WriteLine(string.Format("{0} {1}", tre_block, br));
               //      br.Seek(tre_block.Offset);
               //      Debug.WriteLine(Helper.DumpMemory(br.ToArray(), 0, (int)tre_block.Length, 16));

               //      br.Seek(blockend);

               //   }
               //}
            }
         }
      }

      //void Decode_ExtPointsBlock1(BinaryReaderWriter br, DataBlock src) {
      //   long startadr = src.Offset;
      //   long endpos = src.Offset + src.Length;
      //   br.Seek(startadr);

      //   // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Punkte enthalten
      //   int[] SubdivIdx = new int[TREFile.ExtPointBlock4Subdiv.Count];
      //   TREFile.ExtPointBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0);


      //   List<ExtRawPointData> lst = null;
      //   DataBlock tre_block = null;
      //   int idx = -1;
      //   long blockend = 0;
      //   if (br != null)
      //      while (br.Position < endpos) {
      //         if (br.Position > blockend - 6) {      // min. 6 Byte sind für einen Punkt nötig; jetzt neue Subdiv
      //            if (br.Position < blockend)
      //               Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches nicht erreicht. Noch " + (blockend - br.Position).ToString() + " Bytes übrig.");
      //            else if (blockend < br.Position)
      //               Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches um " + (br.Position - blockend).ToString() + " Bytes überschritten.");

      //            //idx++;
      //            idx = getNextValidIdx(idx, SubdivIdx);
      //            if (idx < 0)
      //               break;
      //            if (SubdivIdx[idx] >= SubdivList.Count) {
      //               Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Subdiv " + SubdivIdx[idx].ToString() + " existiert nicht.");
      //               return;
      //            }

      //            lst = SubdivList[SubdivIdx[idx]].ExtPointList;
      //            lst.Clear();
      //            tre_block = TREFile.ExtPointBlock4Subdiv[SubdivIdx[idx]];
      //            blockend = tre_block.Offset + tre_block.Length - startadr;
      //            br.Seek(tre_block.Offset - startadr);
      //         }
      //         lst.Add(new ExtRawPointData(br));
      //         //new ExtRawPointData(br);

      //         //if (lst[lst.Count - 1].HasUnknownFlag) {
      //         //   if (lst[lst.Count - 1].UnknownKey[0] == 0x41) {

      //         //   } else if (lst[lst.Count - 1].UnknownKey[0] == 0x03 &&
      //         //              lst[lst.Count - 1].UnknownKey[2] == 0x5A) {

      //         //   } else {

      //         //      Debug.WriteLine(string.Format("{0} {1}", tre_block, br));
      //         //      br.Seek(tre_block.Offset);
      //         //      Debug.WriteLine(Helper.DumpMemory(br.ToArray(), 0, (int)tre_block.Length, 16));

      //         //      br.Seek(blockend);

      //         //   }
      //         //}
      //      }
      //}

      #endregion

      public override void Encode_Sections() { }
      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) { }
      public override void SetSectionsAlign() { }
      protected override void Encode_Header(BinaryReaderWriter bw) { }


      /// <summary>
      /// liefert einen Punkt aus einer Subdiv (<see cref="SubdivData.PointList1"/>) (oder null)
      /// <para>Achtung: Vgl. auf null mit ReferenceEquals()</para>
      /// </summary>
      /// <param name="subdividx"></param>
      /// <param name="ptidx"></param>
      /// <returns></returns>
      public RawPointData? GetPoint1(int subdividx, int ptidx) {
         if (subdividx < SubdivList.Count) {
            SubdivData? lst = SubdivList[subdividx];
            if (lst != null &&
                ptidx < lst.PointList1.Count)
               return lst.PointList1[ptidx];
         }
         return null;
      }


   }

}
