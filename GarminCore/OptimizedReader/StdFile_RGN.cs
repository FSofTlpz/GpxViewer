#define READEXT_VARIANT2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable 0661,0660,IDE1006

namespace GarminCore.OptimizedReader {

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
      DataBlock? SubdivContentBlock;

      // --------- Headerlänge > 29 Byte

      /// <summary>
      /// Datenbereich für erweiterte Polygone
      /// </summary>
      DataBlock? ExtAreasBlock;

      byte[] Unknown_0x25 = new byte[0x14];

      /// <summary>
      /// Datenbereich für erweiterte Polylinien
      /// </summary>
      DataBlock? ExtLinesBlock;

      byte[] Unknown_0x41 = new byte[0x14];

      /// <summary>
      /// Datenbereich für erweiterte Punkte
      /// </summary>
      DataBlock? ExtPointsBlock;

      byte[] Unknown_0x5D = new byte[0x14];

      DataBlock? UnknownBlock_0x71;

      byte[] Unknown_0x79 = new byte[4];


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

         long subdivstart;

         DataBlock? data_points;
         DataBlock? data_idxpoints;
         DataBlock? data_polylines;
         DataBlock? data_polygons;

         /// <summary>
         /// Liste der Punkte
         /// </summary>
         public List<RawPointData>? PointList1;

         /// <summary>
         /// Liste der Punkte (wahrscheinlich nur für "Stadt"-Typen, kleiner 0x12)
         /// </summary>
         public List<RawPointData>? PointList2;

         /// <summary>
         /// Liste der Polylines
         /// </summary>
         public List<RawPolyData>? LineList;

         /// <summary>
         /// Liste der Polygone
         /// </summary>
         public List<RawPolyData>? AreaList;

         // --------------------------------------------------------------------

         // Die Listen für die erweiterten Daten werden hier nur verwaltet. Die Speicherung dieser Daten erfolgt NICHT im Subdiv-Bereich.

         /// <summary>
         /// Listen der erweiterten Linien (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPolyData>? ExtLineList;

         /// <summary>
         /// Listen der erweiterten Flächen (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPolyData>? ExtAreaList;

         /// <summary>
         /// Listen der erweiterten Punkte (nur zur Datenverwaltung; die Speicherung erfolgt außerhalb der <see cref="SubdivData"/>!)
         /// </summary>
         public List<ExtRawPointData>? ExtPointList;


         public SubdivData() { }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            if (extdata == null)
               return;
            UInt32 ext = (UInt32)extdata;
            StdFile_TRE.SubdivInfoBasic.SubdivContent Content = (StdFile_TRE.SubdivInfoBasic.SubdivContent)(ext >> 24);
            uint SubdivLength = ext & 0xFFFF;

            data_points = new DataBlock(UInt32.MaxValue, 0);
            data_idxpoints = new DataBlock(UInt32.MaxValue, 0);
            data_polylines = new DataBlock(UInt32.MaxValue, 0);
            data_polygons = new DataBlock(UInt32.MaxValue, 0);

            if (SubdivLength == 0 ||
                Content == StdFile_TRE.SubdivInfoBasic.SubdivContent.nothing) {
               br.Seek(SubdivLength, SeekOrigin.Current);
               Debug.WriteLine("Unbekannter Subdiv-Inhalt/leer");
               return;
            }


            subdivstart = br.Position;       // Startpunkt für die Offsetberechnung

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
               // Da die Offsets nur als 2-Byte-Zahl gespeichert werden, ist die Größe eines Subdiv auf 64kB begrenzt!
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

            br.Position = subdivstart + SubdivLength;
         }

         public void ReadGeoData(BinaryReaderWriter br) {
            // Objekte einlesen
            PointList1 = readPoints(br, data_points, subdivstart);
            PointList2 = readIdxPoints(br, data_idxpoints, subdivstart);
            LineList = readPolylines(br, data_polylines, subdivstart);
            AreaList = readPolygons(br, data_polygons, subdivstart);
         }

         List<RawPointData>? readPoints(BinaryReaderWriter br, DataBlock? block, long subdivstart) {
            List<RawPointData>? lst = null;
            if (block != null && block.Offset != UInt32.MaxValue && block.Length > 0) {
               lst = new List<RawPointData>();
               //if (br.Position != subdivstart + block.Offset) {
               //   Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des Point-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + block.Offset));
               //}
               br.Seek(subdivstart + block.Offset);
               long endpos = br.Position + block.Length;
               while (br.Position < endpos)
                  lst.Add(new RawPointData(br));
            }
            return lst;
         }

         List<RawPointData>? readIdxPoints(BinaryReaderWriter br, DataBlock? block, long subdivstart) {
            List<RawPointData>? lst = null;
            if (block != null && block.Offset != UInt32.MaxValue && block.Length > 0) {
               lst = new List<RawPointData>();
               //if (br.Position != subdivstart + block.Offset) {
               //   Debug.WriteLine("Vermutlich Fehler vor dem Einlesen des IdxPoint-Bereiches einer Subdiv. Offset-Differenz {0} Bytes.", br.Position - (subdivstart + block.Offset));
               //}
               br.Seek(subdivstart + block.Offset);
               long endpos = br.Position + block.Length;
               while (br.Position < endpos)
                  lst.Add(new RawPointData(br));
            }
            return lst;
         }

         List<RawPolyData>? readPolylines(BinaryReaderWriter br, DataBlock? block, long subdivstart) {
            List<RawPolyData>? lst = null;
            if (block != null && block.Offset != UInt32.MaxValue && block.Length > 0) {
               lst = new List<RawPolyData>();
               br.Seek(subdivstart + block.Offset);
               long endpos = br.Position + block.Length;
               while (br.Position < endpos)
                  lst.Add(new RawPolyData(br));
            }
            return lst;
         }

         List<RawPolyData>? readPolygons(BinaryReaderWriter br, DataBlock? block, long subdivstart) {
            List<RawPolyData>? lst = null;
            if (block != null && block.Offset != UInt32.MaxValue && block.Length > 0) {
               lst = new List<RawPolyData>();
               br.Seek(subdivstart + block.Offset);
               long endpos = br.Position + block.Length;
               while (br.Position < endpos)
                  lst.Add(new RawPolyData(br, true));
            }
            return lst;
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

         public override string ToString() {
            return string.Format("Points1 {0}, Points2 {1}, Lines {2}, Areas {3}; Ext.: Points {4}, Lines {5}, Areas {6}",
                                 PointList1?.Count,
                                 PointList2?.Count,
                                 LineList?.Count,
                                 AreaList?.Count,
                                 ExtPointList?.Count,
                                 ExtLineList?.Count,
                                 ExtAreaList?.Count);
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
         public int Type => _Type & 0x7F;

         /// <summary>
         /// Subtype 0x00..0xFF (nur bei Punkten)
         /// </summary>
         public int Subtype => 0;

         /// <summary>
         /// Offset in der LBL- oder NET-Datei (3 Byte)
         /// </summary>
         public UInt32 LabelOffset => _LabelOffset;

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei (Typ, Labeloffset, Lon, Lat)
         /// </summary>
         public uint DataLength => 8;

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
            if (LabelOffset > 0)
               return lbl.GetText(LabelOffset, clear);
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
            sb.AppendFormat(", LabelOffset {0}", LabelOffset);
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
         public new int Type => _Type;

         /// <summary>
         /// Subtype 0x00..0x1F
         /// </summary>
         public new int Subtype => _Subtype & 0x1f;                         // Bit 0..4 (0..0x1f)

         /// <summary>
         /// Offset in der LBL- oder NET-Datei (3 Byte); ungültig setzen mit UInt32.MaxValue
         /// </summary>
         public new UInt32 LabelOffset => HasLabel ?
                                                   _LabelOffset & 0x3fffff :  // ?
                                                   UInt32.MaxValue;

         /// <summary>
         /// Ex. ein Label?
         /// </summary>
         public bool HasLabel => (_Subtype & 0x20) != 0;

         /// <summary>
         /// unbekanntes Flag
         /// </summary>
         public bool HasUnknownFlag {
            get => (_Subtype & 0x40) != 0;
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
            get => (_Subtype & 0x80) != 0;
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
            get => HasExtraBytes ? _ExtraBytes : null;
            protected set {
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
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength => 5 +                                                     // Typ + 2 * Delta
                                       1 +                                                     // Subtyp
                                       (uint)(HasLabel ? 3 : 0) +                              // Label
                                       (uint)(HasExtraBytes && _ExtraBytes != null ? _ExtraBytes.Length : 0);         // Extrabytes


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
               sb.AppendFormat(", LabelOffset {0}", LabelOffset);
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
            protected set {
               if (value > 0)
                  _Subtype = (byte)(value & 0xFF);
               else
                  _Subtype = 0;
               HasSubtype = value > 0;
            }
         }

         /// <summary>
         /// Text-Offset in der LBL-oder Offset in der LBL-POI-Tabelle (Bit 0..21, d.h. bis 0x3FFFFF möglich)
         /// </summary>
         public new UInt32 LabelOffset => _LabelOffset & 0x3FFFFF;

         /// <summary>
         /// es gibt einen Subtyp
         /// </summary>
         public bool HasSubtype {
            get => (_LabelOffset & 0x800000) != 0;
            protected set {
               if (value)
                  _LabelOffset |= 0x800000;
               else
                  _LabelOffset &= 0x7FFFFF;
            }
         }

         /// <summary>
         /// Offset für POI (auf einen "POIRecord")
         /// </summary>
         public bool IsPoiOffset => (_LabelOffset & 0x400000) != 0;

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength => (uint)(base.DataLength + (HasSubtype ? 1 : 0));


         public RawPointData()
            : base() {
            _Subtype = 0;
         }

         public RawPointData(BinaryReaderWriter br)
            : this() {
            Read(br);
         }

         public void Read(BinaryReaderWriter br) {
            _Type = br.ReadByte();
            _LabelOffset = br.Read3AsUInt();
            RawDeltaLongitude = br.Read2AsShort();
            RawDeltaLatitude = br.Read2AsShort();
            if (HasSubtype)
               Subtype = br.ReadByte();
         }

         /// <summary>
         /// liefert den ev. vorhandenen Text zum Punkt
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="clear"></param>
         /// <returns></returns>
         public override string? GetText(StdFile_LBL lbl, bool clear = false) {
            if (!IsPoiOffset) {
               if (LabelOffset > 0)
                  return lbl.GetText(LabelOffset, clear);
            } else { // dann aus der POI-Tabelle
               if (lbl.PointPropertiesListOffsets.TryGetValue(LabelOffset, out int poidx)) { // das sollte immer so sein
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
         public bool IsPolygon { get; }

         /// <summary>
         /// bis 0x7F für Polygone und 0x3F Linien
         /// </summary>
         public new int Type => _Type & (IsPolygon ? 0x7F : 0x3F);

         /// <summary>
         /// Offset in der LBL-oder NET-Datei (Bit 0..21)
         /// </summary>
         public new UInt32 LabelOffset => _LabelOffset & 0x3FFFFF;

         /// <summary>
         /// Längenangabe für den gesamten Datenbereich in 1 oder 2 Byte (wenn der Bitstream länger als 255 Byte ist)
         /// </summary>
         public bool TwoByteLength => Bit.IsSet(_Type, 7);

         /// <summary>
         /// mit Richtungsangabe ? (nur für Polylinien sinnvoll)
         /// </summary>
         public bool DirectionIndicator => !IsPolygon && Bit.IsSet(_Type, 6);

         /// <summary>
         /// <see cref="LabelOffset"/> bezieht sich auf LBL oder NET-Datei (dann routable, also nur für Straßen)
         /// </summary>
         public bool LabelInNET => Bit.IsSet(_LabelOffset, 23);

         /// <summary>
         /// wenn true, dann 1 Bit zusätzlich je Punkt (die Straße sollte dann routable sein und Zusatzinfos in NET / NOD enthalten)
         /// </summary>
         public bool WithExtraBit => Bit.IsSet(_LabelOffset, 22);

         /// <summary>
         /// Extrabit je Punkt (nur für routable Straßen)
         /// </summary>
         public List<bool> ExtraBit { get; protected set; }

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength => (uint)(base.DataLength + (TwoByteLength ? 2 : 1) + 1 + (_bitstream != null ? _bitstream.Length : 0));


         /// <summary>
         /// Bits je Koordinate (codiert)
         /// </summary>
         byte bitstreamInfo;

         /// <summary>
         /// Bitstream der Geodaten
         /// </summary>
         byte[]? _bitstream;


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
         /// liefert eine Liste aller Punkte und setzt dabei auch die Liste <see cref="ExtraBit"/> entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// <para><see cref="RawDeltaLongitude"/> und <see cref="RawDeltaLatitude"/> stellen den Startpunkt dar. Die Koordinaten beziehen sich auf den Mittelpunkt der zugehörigen Subdiv.</para>
         /// </summary>
         /// <returns></returns>
         public List<GeoDataBitstream.RawPoint> GetRawPoints() {
            ExtraBit.Clear();
            return GeoDataBitstream.GetRawPoints(ref _bitstream,
                                                 bitstreamInfo & 0x0F,
                                                 (bitstreamInfo & 0xF0) >> 4,
                                                 RawDeltaLongitude,
                                                 RawDeltaLatitude,
                                                 WithExtraBit ? ExtraBit : null,
                                                 false);
         }

         /// <summary>
         /// liefert eine Liste aller Punkte und setzt dabei auch die Liste <see cref="ExtraBit"/> entsprechend der aktuellen Daten in <see cref="_bitstream"/>
         /// </summary>
         /// <param name="coordbits"></param>
         /// <param name="subdiv_center"></param>
         /// <returns></returns>
         public List<MapUnitPoint> GetMapUnitPoints(int coordbits, MapUnitPoint subdiv_center) {
            List<MapUnitPoint> lst = new List<MapUnitPoint>();
            foreach (var item in GetRawPoints())
               lst.Add(item.GetMapUnitPoint(coordbits, subdiv_center));
            return lst;
         }

         public static bool operator ==(RawPolyData x, RawPolyData y) {
            if (x._Type == y._Type &&
                x.RawDeltaLongitude == y.RawDeltaLongitude &&
                x.RawDeltaLatitude == y.RawDeltaLatitude &&
                x.WithExtraBit == y.WithExtraBit) {

               List<GeoDataBitstream.RawPoint> px = x.GetRawPoints();
               List<GeoDataBitstream.RawPoint> py = y.GetRawPoints();
               if (px.Count != py.Count)
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
            List<GeoDataBitstream.RawPoint> pts = GetRawPoints();
            if (pts.Count > 0) {
               rb = new Bound(pts[0].RawUnitsLon, pts[0].RawUnitsLat);
               for (int i = 1; i < pts.Count; i++)
                  rb.Embed(pts[i].RawUnitsLon, pts[i].RawUnitsLat);
            }
            return rb;
         }

         public override string ToString() {
            return string.Format("Typ {0:x2}, LabelOffset {1}, LabelInNET {2}, RawDeltaLongitude {3}, RawDeltaLatitude {4}, WithExtraBit {5}, Datenbytes {6}",
                                 Type,
                                 LabelOffset,
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

         protected byte bitstreamInfo;

         byte[]? _bitstream;

         /// <summary>
         /// Ex. Punkte?
         /// </summary>
         public bool WithPoints => _bitstream != null && _bitstream.Length > 0;

         /// <summary>
         /// 7-Bit-Werte 0x00..0x7F
         /// </summary>
         public new int Type => _Type & 0x7F;

         /// <summary>
         /// Größe des Speicherbereiches in der RGN-Datei
         /// </summary>
         public new uint DataLength => (uint)(base.DataLength +
                                       (_bitstream != null ? (_bitstream.Length + 1 < 0x7F ? 1 : 2) : 0) +   // Länge des Bitstreams
                                       1 +                                                                   // Codierung des Bitstreams
                                       (_bitstream != null ? _bitstream.Length : 0));                        // Bitstream

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
            // <=9 -> 11
            // 10  -> 13
            // 11  -> 15
            // 12  -> 17
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
                     break;
                  }

                  reallength = 0;
                  int lat = GetValueFromBytetArray(bitstreamoffset, bits4Latitude, ref reallength, lat_sign, ref bitstream);
                  bitstreamoffset += reallength;
                  if (reallength <= 0) {
                     bitstreamoffset += 2 * reallength;
                     break;
                  }

                  if (extrabit != null)
                     extrabit.Add(GetBitFromByteArray(bitstreamoffset++, ref bitstream));

                  // Each point in a poly object is defined relative to the previous point.
                  rawpoints.Add(new RawPoint(rawpoints[rawpoints.Count - 1].RawUnitsLon + lon, rawpoints[rawpoints.Count - 1].RawUnitsLat + lat));
               }
            }

            return rawpoints;
         }

      }


      /// <summary>
      /// übergeordnete TRE-Datei (im Konstruktor gesetzt)
      /// </summary>
      StdFile_TRE TREFile;

      /// <summary>
      /// Liste aller Subdivs mit ihren Daten
      /// </summary>
      List<SubdivData?> SubdivList;



      public StdFile_RGN(StdFile_TRE tre) : base("RGN") {
         Headerlength = 0x7D;

         TREFile = tre;
         SubdivList = new List<SubdivData?>();
      }

      public override void ReadHeader(BinaryReaderWriter reader) {
         readCommonHeader(reader, Type);

         SubdivContentBlock = new DataBlock(reader);

         // --------- Headerlänge > 29 Byte

         if (Headerlength > 0x1D) {
            ExtAreasBlock = new DataBlock(reader);
            reader.ReadBytes(Unknown_0x25);
            ExtLinesBlock = new DataBlock(reader);
            reader.ReadBytes(Unknown_0x41);
            ExtPointsBlock = new DataBlock(reader);
            reader.ReadBytes(Unknown_0x5D);
            UnknownBlock_0x71 = new DataBlock(reader);
            reader.ReadBytes(Unknown_0x79);

         }
      }

      public override void ReadMinimalSections(BinaryReaderWriter reader) {
         if (TREFile == null)
            throw new Exception("Ohne dazugehörende TRE-Datei können keine Subdiv-Infos gelesen werden.");

         while (SubdivList.Count < TREFile.SubdivInfoList.Count)    // ev. Standardliste mit null für jede Subdiv erzeugen
            SubdivList.Add(null);

         // alle Subdiv-Daten "interpretieren"
         List<StdFile_TRE.SubdivInfoBasic> subdivinfoList = TREFile.SubdivInfoList;
         if (subdivinfoList != null &&
             subdivinfoList.Count > 0) {
            // Die Offsets für den zugehörigen Datenbereich für jedes Subdiv sind zwar schon gesetzt, aber die Länge der entsprechenden Blöcke fehlt noch.
            // Die letzte Subdiv nimmt den Rest des gesamten Datenblocks ein.
            //for (int i = 0; i < subdivinfoList.Count - 1; i++)
            //   subdivinfoList[i].Data.Length = subdivinfoList[i + 1].Data.Offset - subdivinfoList[i].Data.Offset;
            //subdivinfoList[subdivinfoList.Count - 1].Data.Length = Filesections.GetLength(filesectiontype) - subdivinfoList[subdivinfoList.Count - 1].Data.Offset;

            Decode_SubdivContentBlock(reader, SubdivContentBlock);
         }
      }

      public void ReadExtData(BinaryReaderWriter reader) {
         // alle Ext-Daten "interpretieren"
         Decode_ExtObjectBlock(reader, ExtAreasBlock, TREFile.ExtAreaBlock4Subdiv, objectType.ExtArea);
         Decode_ExtObjectBlock(reader, ExtLinesBlock, TREFile.ExtLineBlock4Subdiv, objectType.ExtPolyline);
         Decode_ExtObjectBlock(reader, ExtPointsBlock, TREFile.ExtPointBlock4Subdiv, objectType.ExtPoint);
      }

      public SubdivData?[] GetSubdivs(IList<int> idxlst) {
         SubdivData?[] subdivs = new SubdivData[idxlst.Count];
         for (int i = 0; i < idxlst.Count; i++)
            subdivs[i] = SubdivList[idxlst[i]];
         return subdivs;
      }

      /// <summary>
      /// liest die Standard-Geo-Objekte aus den gültigen Subdivs (wenn keine gültigen ex., dann aus allen)
      /// </summary>
      /// <param name="reader"></param>
      public void ReadGeoData(BinaryReaderWriter reader) {
         if (validsubdividx.Length > 0)
            for (int i = 0; i < validsubdividx.Length; i++)
               SubdivList[validsubdividx[i]]?.ReadGeoData(reader);
         else {
            foreach (var sd in SubdivList) {
               sd?.ReadGeoData(reader);
            }
         }
      }

      /// <summary>
      /// Liste der gültigen Subdiv-Index bei unvollständigem Einlesen
      /// </summary>
      int[] validsubdividx = new int[0];

      public void SetValidSubdivIdx(IList<int> validsubdividx) {
         this.validsubdividx = new int[validsubdividx.Count];
         validsubdividx.CopyTo(this.validsubdividx, 0);
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

      void Decode_SubdivContentBlock(BinaryReaderWriter br, DataBlock? src) {
         if (br != null &&
             src != null &&
             src.Length > 0) {
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
#pragma warning disable CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
               SubdivList = br.ReadArray<SubdivData>(src, extdata);
#pragma warning restore CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
            else {
               while (SubdivList.Count < subdivinfoList.Count) // ev. Standardliste mit null für jede Subdiv erzeugen
                  SubdivList.Add(null);

               foreach (int idx in validsubdividx) { // die gewünschten Subdivs einlesen
                  if (SubdivList[idx] == null) {         // tatsächlich noch nicht eingelesen
                     List<SubdivData> lst;

                     //lst = br.ReadArray<SubdivData>(new DataBlock(subdivinfoList[idx].Data.Offset + src.Offset, subdivinfoList[idx].Data.Length), new object[] { extdata[idx] });


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

      int[] getExtSubdivIdx(SortedDictionary<int, DataBlock> data) {
         int[] subdivIdx = new int[data.Count];
         data.Keys.CopyTo(subdivIdx, 0); // die Schlüssel sind die Subdiv-Indexe (die Werte sind die Datenblöcke)
         return subdivIdx;
      }


      enum objectType {
         ExtArea,
         ExtPolyline,
         ExtPoint,
      }


      void Decode_ExtObjectBlock(BinaryReaderWriter br,
                                 DataBlock? src,
                                 SortedDictionary<int, DataBlock> objdata,
                                 objectType objectType) {
         if (br != null &&
             src != null &&
             src.Length > 0) {

            long startadr = src.Offset;
            long endpos = src.Offset + src.Length;
            br.Seek(startadr);

#if READEXT_VARIANT2
            // Subdivs werden u.U. nicht nach Index sortiert eingelesen, d.h. die Lesepos. "springt" ev. mehr hin und her
            // (Beim Einlesen über einen Memory-BinaryReaderWriter kein Nachteil)

            List<ExtRawPolyData>? lstpoly = null;
            List<ExtRawPointData>? lstpt = null;

            // Indexliste aller Subdiv's aus diesem Dictionary aus der TRE-Datei 
            foreach (var item in objdata) {
               int subdividx = item.Key;
               DataBlock tre_block = item.Value;

               br.Seek(startadr + tre_block.Offset);
               long blockend = br.Position + tre_block.Length;

               if (validsubdividx != null &&
                   validsubdividx.Length > 0 &&
                   !validsubdividx.Contains(subdividx))
                  continue;

               if (lstpoly == null)
                  lstpoly = new List<ExtRawPolyData>();
               if (lstpt == null)
                  lstpt = new List<ExtRawPointData>();

               while (br.Position < blockend) {
                  // neues Objekt einlesen und in die Liste übernehmen
                  switch (objectType) {
                     case objectType.ExtArea:
                     case objectType.ExtPolyline:
                        if (lstpoly != null)
                           lstpoly.Add(new ExtRawPolyData(br));
                        else
                           br.Position = blockend;
                        break;

                     case objectType.ExtPoint:
                        if (lstpt != null)
                           lstpt.Add(new ExtRawPointData(br));
                        else
                           br.Position = blockend;
                        break;
                  }
               }

               if (SubdivList != null) {
                  SubdivData? sd = SubdivList[subdividx];
                  if (sd == null) {
                     SubdivList[subdividx] = sd = new SubdivData();
                  }
                  switch (objectType) {
                     case objectType.ExtArea:
                        if (lstpoly != null && lstpoly.Count > 0) {
                           sd.ExtAreaList = lstpoly;
                           lstpoly = null;
                        }
                        break;

                     case objectType.ExtPolyline:
                        if (lstpoly != null && lstpoly.Count > 0) {
                           sd.ExtLineList = lstpoly;
                           lstpoly = null;
                        }
                        break;

                     case objectType.ExtPoint:
                        if (lstpt != null && lstpt.Count > 0) {
                           sd.ExtPointList = lstpt;
                           lstpt = null;
                        }
                        break;
                  }
               }
            }

#else
            int[] subdivIdx = getExtSubdivIdx(objdata);
            if (subdivIdx.Length > 0) {
               int idx = -1;
               int subdividx;
               DataBlock tre_block;
               List<ExtRawPolyData> lstpoly = null;
               List<ExtRawPointData> lstpt = null;
               long blockend = br.Position; // Blockende simulieren

               while (br.Position < endpos) {
                  if (br.Position >= blockend) {          // alles für die aktuelle Subdiv eingelesen

                     if (br.Position > blockend)
                        throw new Exception("TRE-Blockgröße beim Einlesen der " +
                                            (objectType == objectType.ExtArea ? "ExtRawPolyData (Flächen)" :
                                             objectType == objectType.ExtPolyline ? "ExtRawPolyData (Linien)" : "ExtRawPointData (Points)") +
                                            " um " + (br.Position - blockend).ToString() + " Bytes überschritten");
                     if (idx > subdivIdx.Length - 2 ||
                         subdivIdx[idx + 1] > SubdivList.Count - 1) {
                        // Bei einer Originalkarte wurde beobachtet, dass Daten für eine nicht ex. Subdiv enthalten waren.
                        Debug.WriteLine("Ev. Fehler beim Einlesen der " +
                                         (objectType == objectType.ExtArea ? "ExtRawPolyData (Flächen)" :
                                          objectType == objectType.ExtPolyline ? "ExtRawPolyData (Linien)" : "ExtRawPointData (Points)"));
                        break;
                     }

                     // Vorbereitung für nächste Subdiv
                     idx = getNextValidIdx(idx, subdivIdx);       //subdividx = SubdivIdx[++idx];
                     if (idx < 0)   // keine Subdiv mehr
                        break;
                     subdividx = subdivIdx[idx];
                     if (SubdivList[subdividx] == null)
                        throw new Exception(nameof(Decode_ExtObjectBlock) + ": Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");

                     tre_block = objdata[subdividx];
                     br.Seek(startadr + tre_block.Offset);
                     blockend = br.Position + tre_block.Length;

                     if (validsubdividx != null &&
                         validsubdividx.Length > 0 &&
                         !validsubdividx.Contains(subdividx)) {
                        br.Seek(blockend);                     // Daten dieser Subdiv überspringen
                        continue;
                     }

                     // neue, leere Liste
                     switch (objectType) {
                        case objectType.ExtArea:
                           lstpoly = SubdivList[subdividx].ExtAreaList;
                           lstpoly.Clear();
                           break;

                        case objectType.ExtPolyline:
                           lstpoly = SubdivList[subdividx].ExtLineList;
                           lstpoly.Clear();
                           break;

                        case objectType.ExtPoint:
                           lstpt = SubdivList[subdividx].ExtPointList;
                           lstpt.Clear();
                           break;
                     }
                  }
                  // neues Objekt einlesen und in die Liste übernehmen
                  switch (objectType) {
                     case objectType.ExtArea:
                     case objectType.ExtPolyline:
                        lstpoly.Add(new ExtRawPolyData(br));
                        break;

                     case objectType.ExtPoint:
                        lstpt.Add(new ExtRawPointData(br));
                        break;
                  }
               }
            }
#endif

         }
      }

      //void Decode_ExtAreasBlock(BinaryReaderWriter br, DataBlock src) {
      //   if (br != null &&
      //       src != null &&
      //       src.Length > 0) {

      //      long startadr = src.Offset;
      //      long endpos = src.Offset + src.Length;
      //      br.Seek(startadr);

      //      // Indexliste aller Subdiv's die erweiterte Polygone enthalten aus der TRE-Datei 
      //      int[] subdivIdx = getExtSubdivIdx(TREFile.ExtAreaBlock4Subdiv);
      //      if (subdivIdx.Length > 0) {
      //         int idx = -1;
      //         int subdividx;
      //         DataBlock tre_block;
      //         List<ExtRawPolyData> lst = null;
      //         long blockend = br.Position; // Blockende simulieren

      //         while (br.Position < endpos) {
      //            if (br.Position >= blockend) {          // alles für die aktuelle Subdiv eingelesen

      //               if (br.Position > blockend)
      //                  throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Flächen) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
      //               if (idx > subdivIdx.Length - 2 ||
      //                   subdivIdx[idx + 1] > SubdivList.Count - 1) {
      //                  Debug.WriteLine("Ev. Fehler beim Einlesen der ExtPolyData (Flächen)");  // Bei einer Originalkarte wurde beobachtet, dass Daten für eine nicht ex. Subdiv enthalten waren.
      //                  break;
      //               }

      //               // Vorbereitung für nächste Subdiv
      //               idx = getNextValidIdx(idx, subdivIdx);       //subdividx = SubdivIdx[++idx];
      //               if (idx < 0)   // keine Subdiv mehr
      //                  break;
      //               subdividx = subdivIdx[idx];
      //               if (SubdivList[subdividx] == null)
      //                  throw new Exception("Decode_ExtAreasBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");

      //               tre_block = TREFile.ExtAreaBlock4Subdiv[subdividx];
      //               br.Seek(startadr + tre_block.Offset);
      //               blockend = br.Position + tre_block.Length;

      //               //br.Seek(tre_block.Offset - startadr);
      //               //blockend = tre_block.Offset + tre_block.Length - startadr;

      //               if (validsubdividx != null &&
      //                   validsubdividx.Length > 0 &&
      //                   !validsubdividx.Contains(subdividx)) {
      //                  br.Seek(blockend);                     // Daten dieser Subdiv überspringen
      //                  continue;
      //               }

      //               lst = SubdivList[subdividx].ExtAreaList;
      //               lst.Clear();
      //            }
      //            lst.Add(new ExtRawPolyData(br));
      //         }
      //      }
      //   }
      //}

      //void Decode_ExtLinesBlock(BinaryReaderWriter br, DataBlock src) {
      //   long startadr = src.Offset;
      //   long endpos = src.Offset + src.Length;
      //   br.Seek(startadr);

      //   // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Polygone enthalten
      //   int[] SubdivIdx = new int[TREFile.ExtLineBlock4Subdiv.Count];
      //   TREFile.ExtLineBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0);

      //   if (br != null &&
      //       src != null &&
      //       src.Length > 0) {
      //      int idx = -1;
      //      int subdividx;
      //      DataBlock tre_block;
      //      long blockend = br.Position; // Blockende simulieren
      //      List<ExtRawPolyData> lst = null;

      //      while (br.Position < endpos) {
      //         if (br.Position >= blockend) {
      //            if (br.Position > blockend)
      //               throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Linien) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
      //            if (idx > SubdivIdx.Length - 2 ||
      //                SubdivIdx[idx + 1] > SubdivList.Count - 1) {
      //               Debug.WriteLine("Ev. Fehler beim Einlesen der ExtPolyData (Linien)");  // Bei einer Originalkarte wurde beobachtet, dass Daten für eine nicht ex. Subdiv enthalten waren.
      //               break;
      //            }

      //            // Vorbereitung für nächste Subdiv
      //            idx = getNextValidIdx(idx, SubdivIdx);       //subdividx = SubdivIdx[++idx];
      //            if (idx < 0)   // keine Subdiv mehr
      //               break;
      //            subdividx = SubdivIdx[idx];
      //            if (SubdivList[subdividx] == null)
      //               throw new Exception("Decode_ExtLinesBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");

      //            tre_block = TREFile.ExtLineBlock4Subdiv[subdividx];
      //            br.Seek(startadr + tre_block.Offset);
      //            blockend = br.Position + tre_block.Length;

      //            if (validsubdividx != null &&
      //                validsubdividx.Length > 0 &&
      //                !validsubdividx.Contains(subdividx)) {
      //               br.Seek(blockend);                     // Daten dieser Subdiv überspringen
      //               continue;
      //            }

      //            lst = SubdivList[subdividx].ExtLineList;
      //            lst.Clear();
      //         }
      //         lst.Add(new ExtRawPolyData(br));
      //      }
      //   }
      //}

      //void Decode_ExtPointsBlock(BinaryReaderWriter br, DataBlock src) {
      //   long startadr = src.Offset;
      //   long endpos = src.Offset + src.Length;
      //   br.Seek(startadr);

      //   // Indexliste aller Subdiv's aus der TRE-Datei erzeugen/kopieren, die erweiterte Punkte enthalten
      //   int[] SubdivIdx = new int[TREFile.ExtPointBlock4Subdiv.Count];
      //   TREFile.ExtPointBlock4Subdiv.Keys.CopyTo(SubdivIdx, 0);

      //   if (br != null &&
      //       src != null &&
      //       src.Length > 0) {
      //      int idx = -1;
      //      int subdividx;
      //      DataBlock tre_block;
      //      long blockend = br.Position; // Blockende simulieren
      //      List<ExtRawPointData> lst = null;

      //      while (br.Position < endpos) {
      //         //if (br.Position > blockend - 6) {      // min. 6 Byte sind für einen Punkt nötig; jetzt neue Subdiv
      //         if (br.Position >= blockend) {
      //            if (br.Position > blockend)
      //               throw new Exception("TRE-Blockgröße beim Einlesen der ExtPolyData (Points) um " + (br.Position - blockend).ToString() + " Bytes überschritten");
      //            //if (br.Position < blockend)
      //            //   Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches nicht erreicht. Noch " + (blockend - br.Position).ToString() + " Bytes übrig.");
      //            //else if (blockend < br.Position)
      //            //   Debug.WriteLine("Fehler beim Einlesen der ExtPointData: Ende des Subdiv-Bereiches um " + (br.Position - blockend).ToString() + " Bytes überschritten.");

      //            // Vorbereitung für nächste Subdiv
      //            idx = getNextValidIdx(idx, SubdivIdx);       //subdividx = SubdivIdx[++idx];
      //            if (idx < 0)   // keine Subdiv mehr
      //               break;
      //            subdividx = SubdivIdx[idx];
      //            if (SubdivList[subdividx] == null)
      //               throw new Exception("Decode_ExtPointsBlock: Subdiv " + subdividx.ToString() + " noch nicht erzeugt.");


      //            tre_block = TREFile.ExtPointBlock4Subdiv[subdividx];
      //            br.Seek(startadr + tre_block.Offset);
      //            blockend = br.Position + tre_block.Length;

      //            if (validsubdividx != null &&
      //                validsubdividx.Length > 0 &&
      //                !validsubdividx.Contains(subdividx)) {
      //               br.Seek(blockend);                     // Daten dieser Subdiv überspringen
      //               continue;
      //            }

      //            lst = SubdivList[subdividx].ExtPointList;
      //            lst.Clear();
      //         }
      //         lst.Add(new ExtRawPointData(br));

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
      //   }
      //}

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
                lst.PointList1 != null &&
                ptidx < lst.PointList1.Count)
               return lst.PointList1[ptidx];
         }
         return null;
      }

   }

}
