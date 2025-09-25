/*
Copyright (C) 2011, 2016 Frank Stinner

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
using System.Text;
using GarminCore.Files.Typ;

namespace GarminCore.Files {

   /// <summary>
   /// Hauptklasse zur Behandlung von Garmin-Typfiles
   /// </summary>
   public class StdFile_TYP : StdFile {

      #region Header-Daten

      /// <summary>
      /// beschreibt einen Dateibereich mit Offset und Länge (4 + 4 Byte) und einer Satzlänge (2 Byte)
      /// </summary>
      public class TypDataBlockWithRecordsize : DataBlockWithRecordsize {

         public TypDataBlockWithRecordsize() : base() { }

         public TypDataBlockWithRecordsize(DataBlockWithRecordsize? block) {
            if (block != null) {
               Offset = block.Offset;
               Length = block.Length;
               Recordsize = block.Recordsize;
            }
         }

         public TypDataBlockWithRecordsize(BinaryReaderWriter br) {
            Read(br);
         }

         /// <summary>
         /// liest die Blockdaten
         /// </summary>
         /// <param name="br"></param>
         public new void Read(BinaryReaderWriter br) {
            Offset = br.Read4UInt();
            Recordsize = br.Read2AsUShort();
            Length = br.Read4UInt();
         }

         /// <summary>
         /// schreibt die Blockdaten
         /// </summary>
         /// <param name="bw"></param>
         public new void Write(BinaryReaderWriter bw) {
            bw.Write(Offset);
            bw.Write(Recordsize);
            bw.Write(Length);
         }

      }

      /// <summary>
      /// Codepage, u.a.:
      /// <para>1250, Central & Eastern European</para>
      /// <para>1251, Cyrillic, mainly Slavic</para>
      /// <para>1252, West European</para>
      /// <para>1253, Greek</para>
      /// <para>1254, Turkish</para>
      /// <para>1255, Hebrew</para>
      /// <para>1256, Arabic</para>
      /// <para>1257, Baltic</para>
      /// <para>1258, Vietnamese</para>
      /// </summary>
      UInt16 codepage = 1252;

      /// <summary>
      /// Datenblock für POI's
      /// </summary>
      public DataBlock PointDatablock { get; private set; }
      /// <summary>
      /// Datenblock für Polygone
      /// </summary>
      public DataBlock PolygoneDatablock { get; private set; }
      /// <summary>
      /// Datenblock für Polylines
      /// </summary>
      public DataBlock PolylineDatablock { get; private set; }

      /// <summary>
      /// Codepage für Texte (z.B. 1252)
      /// </summary>
      public UInt16 Codepage {
         get {
            return codepage;
         }
         set {
            //if (value < 1250 || 1258 < value)
            //   throw new Exception("Die Codepage muß im Bereich 1250 bis 1258 sein");
            codepage = value;
         }
      }

      /// <summary>
      /// zur eindeutigen Kennzeichnung für eine bestimmte Karte mit der gleichen ID
      /// </summary>
      public UInt16 FamilyID { get; set; }

      /// <summary>
      /// Produkt-ID (i.A. 1)
      /// </summary>
      public UInt16 ProductID { get; set; }

      /// <summary>
      /// Tabelle für POI's
      /// </summary>
      public TypDataBlockWithRecordsize PointTableBlock { get; private set; }
      /// <summary>
      /// Tabelle für Polyline
      /// </summary>
      public TypDataBlockWithRecordsize PolylineTableBlock { get; private set; }
      /// <summary>
      /// Tabelle für Polygone
      /// </summary>
      public TypDataBlockWithRecordsize PolygoneTableBlock { get; private set; }
      /// <summary>
      /// Tabelle für Draworder der Polygone
      /// </summary>
      public TypDataBlockWithRecordsize PolygoneDraworderTableBlock { get; private set; }

      /// <summary>
      /// Liste der <see cref="TableItem"/> für Polygone (nach dem Einlesen)
      /// </summary>
      public List<TableItem>? PolygonTableItems { get; private set; }
      /// <summary>
      /// Liste der <see cref="TableItem"/> für Polylines (nach dem Einlesen)
      /// </summary>
      public List<TableItem>? PolylineTableItems { get; private set; }
      /// <summary>
      /// Liste der <see cref="TableItem"/> für POI's (nach dem Einlesen)
      /// </summary>
      public List<TableItem>? PointTableItems { get; private set; }
      /// <summary>
      /// Liste der <see cref="PolygonDraworderTableItem"/> für die Zeichenreihenfolge der Polygone (nach dem Einlesen)
      /// </summary>
      public List<PolygonDraworderTableItem>? PolygonDraworderTableItems { get; private set; }

      // ----------------- NT-Daten -----------------

      public TypDataBlockWithRecordsize NT_PointTableBlock { get; private set; }
      public byte nt_unknown_0x65 { get; private set; }
      public DataBlock NT_PointDatablock { get; private set; }
      public UInt32 nt_unknown_0x6E { get; private set; }
      public DataBlock NT_PointLabelblock { get; private set; }
      public UInt32 nt_unknown_0x7A { get; private set; }
      public UInt32 nt_unknown_0x7E { get; private set; }
      public DataBlock NT_LabelblockTable1 { get; private set; }
      public UInt32 nt_unknown_0x8A { get; private set; }
      public UInt32 nt_unknown_0x8E { get; private set; }
      public DataBlock NT_LabelblockTable2 { get; private set; }
      public UInt16 nt_unknown_0x9A { get; private set; }
      public byte[] nt_unknown_0x9C { get; private set; }
      public byte[] nt_unknown_0xA4 { get; private set; }
      public byte[]? nt_unknown_0xAE { get; private set; }

      public enum Headertyp {
         Unknown,
         /// <summary>
         /// Headerlänge 0x5B
         /// </summary>
         Standard,
         Type_6E,
         Type_9C,
         Type_A4,
         Type_AE,
      }

      Headertyp _HeaderTyp = Headertyp.Standard;

      public Headertyp HeaderTyp {
         get {
            return _HeaderTyp;
         }
         private set {
            _HeaderTyp = value;
            switch (_HeaderTyp) {
               case Headertyp.Standard:
                  Headerlength = 0x5b;
                  break;

               case Headertyp.Type_6E:
                  Headerlength = 0x6E;
                  break;

               case Headertyp.Type_9C:
                  Headerlength = 0x9C;
                  break;

               case Headertyp.Type_A4:
                  Headerlength = 0xA4;
                  break;

               case Headertyp.Type_AE:
                  Headerlength = 0xAE;
                  break;

               case Headertyp.Unknown:
                  break;

               default:
                  throw new Exception("Unbekannter Headertyp.");
            }
         }
      }

      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,

         NT_PointDatablock,
         NT_PointDatabtable,
         NT_PointLabelblock,
         NT_LabelblockTable1,
         NT_LabelblockTable2,
      }

      protected SortedList<Polygone, byte> polygone;
      protected SortedList<Polyline, byte> polyline;
      protected SortedList<POI, byte> poi;

      /// <summary>
      /// liefert die Anzahl der Punkte in der internen Liste
      /// </summary>
      public int PoiCount { get { return poi.Count; } }
      /// <summary>
      /// liefert die Anzahl der Polygone in der internen Liste
      /// </summary>
      public int PolygonCount { get { return polygone.Count; } }
      /// <summary>
      /// liefert die Anzahl der Linien in der internen Liste
      /// </summary>
      public int PolylineCount { get { return polyline.Count; } }

      /// <summary>
      /// Sammlung aller Fehler, die sich im Relaxed-Modus ergeben haben
      /// </summary>
      public string RelaxedModeErrors { get; private set; }

      Encoding encoding;



      public StdFile_TYP()
         : base("TYP") {
         RelaxedModeErrors = "";

         HeaderTyp = Headertyp.Standard;
         Codepage = 1252;
         FamilyID = 0;
         ProductID = 1;

         encoding = Encoding.GetEncoding(Codepage);

         PointDatablock = new DataBlock();
         PolygoneDatablock = new DataBlock();
         PolylineDatablock = new DataBlock();

         PointTableBlock = new TypDataBlockWithRecordsize();
         PolylineTableBlock = new TypDataBlockWithRecordsize();
         PolygoneTableBlock = new TypDataBlockWithRecordsize();
         PolygoneDraworderTableBlock = new TypDataBlockWithRecordsize();

         // für NT-Format
         NT_PointTableBlock = new TypDataBlockWithRecordsize();
         NT_PointDatablock = new DataBlock();
         NT_PointLabelblock = new DataBlock();

         NT_LabelblockTable1 = new DataBlock();
         NT_LabelblockTable2 = new DataBlock();

         polygone = new SortedList<Polygone, byte>();
         polyline = new SortedList<Polyline, byte>();
         poi = new SortedList<POI, byte>();

         nt_unknown_0x65 = 0x1f;

         nt_unknown_0x6E =
         nt_unknown_0x7A =
         nt_unknown_0x7E =
         nt_unknown_0x8A =
         nt_unknown_0x8E =
         nt_unknown_0x9A = 0;

         nt_unknown_0x9C = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
         nt_unknown_0xA4 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      }

      public StdFile_TYP(BinaryReaderWriter br)
         : this(br, false) {
      }

      public StdFile_TYP(BinaryReaderWriter br, bool bRelaxed)
         : this() {
         nonvirtual_Read(br, bRelaxed);
      }

      public StdFile_TYP(StdFile_TYP tf)
         : this() {
         CreationDate = tf.CreationDate;
         Codepage = tf.Codepage;
         FamilyID = tf.FamilyID;
         ProductID = tf.ProductID;
         encoding = tf.encoding;
         for (int i = 0; i < tf.PoiCount; i++) {
            POI? p = tf.GetPoi(i);
            if (p != null)
               Insert(p);
         }
         for (int i = 0; i < tf.PolygonCount; i++) {
            Polygone? p = tf.GetPolygone(i);
            if (p != null)
               Insert(p);
         }
         for (int i = 0; i < tf.PolylineCount; i++) {
            Polyline? p = tf.GetPolyline(i);
            if (p != null)
               Insert(p);
         }
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         if (Unknown_0x0C != 0x01) // Bedeutung unklar
            throw new Exception("Das ist keine Garmin-TYP-Datei.");

         Headertyp htyp = Headertyp.Unknown;

         Codepage = br.Read2AsUShort();
         encoding = Encoding.GetEncoding(Codepage);
         br.SetEncoding(Codepage);

         // Infos zu den Datenblöcken für POI, Polyline und Polygon einlesen (Offset, Länge)
         // (eigentlich uninteressant, da auf die Daten über die entsprechenden Tabellen zugegriffen wird)
         PointDatablock.Read(br);
         PolylineDatablock.Read(br);
         PolygoneDatablock.Read(br);

         FamilyID = br.Read2AsUShort();
         ProductID = br.Read2AsUShort();

         // Infos zu den Tabellen für POI, Polyline und Polygon einlesen (Offset, Länge, Länge der Tabelleneinträge)
         PointTableBlock = new TypDataBlockWithRecordsize(br);
         PolylineTableBlock = new TypDataBlockWithRecordsize(br);
         PolygoneTableBlock = new TypDataBlockWithRecordsize(br);
         PolygoneDraworderTableBlock = new TypDataBlockWithRecordsize(br);

         htyp = Headertyp.Standard;

         // ev. kommt noch NT-Zeugs
         if (Headerlength > 0x5b) { // Extra icons
            htyp = Headertyp.Type_6E;

            // spez. Daten für NT1-Punkte
            NT_PointTableBlock = new TypDataBlockWithRecordsize(br);
            nt_unknown_0x65 = br.ReadByte();               // sollte wohl immer 0x1F sein (?), auch 0x0D
            NT_PointDatablock.Read(br);

            if (Headerlength > 0x6e) {           // Extra POI Labels
               htyp = Headertyp.Type_9C;

               nt_unknown_0x6E = br.Read4UInt(); // 0
               NT_PointLabelblock.Read(br);       // Block-Offset und -Länge
               nt_unknown_0x7A = br.Read4UInt(); // 6    Datensatzlänge?
               nt_unknown_0x7E = br.Read4UInt(); // 0x1B
               NT_LabelblockTable1.Read(br);
               nt_unknown_0x8A = br.Read4UInt(); // 6
               nt_unknown_0x8E = br.Read4UInt(); // 0x1B
               NT_LabelblockTable2.Read(br);
               nt_unknown_0x9A = br.Read2AsUShort(); // 0x12

               if (Headerlength > 0x9C) { // Indexing a selection of POIs
                  htyp = Headertyp.Type_A4;

                  br.ReadBytes(nt_unknown_0x9C); // scheint nochmal der gleiche Datenblock wie LabelblockTable2 zu sein

                  if (Headerlength > 0xA4) { // Active Routing
                     htyp = Headertyp.Type_AE;

                     br.ReadBytes(nt_unknown_0xA4);

                     if (Headerlength > 0xAE) {
                        htyp = Headertyp.Unknown;

                        nt_unknown_0xA4 = br.ReadBytes(Headerlength - (int)br.Position); // Rest einlesen

                     }
                  }
               }
            }
         }
         _HeaderTyp = htyp;
      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden (nur NT) ---------
         Filesections.AddSection((int)InternalFileSections.NT_PointDatablock, new DataBlock(NT_PointDatablock));
         Filesections.AddSection((int)InternalFileSections.NT_PointDatabtable, new DataBlock(NT_PointTableBlock));
         Filesections.AddSection((int)InternalFileSections.NT_PointLabelblock, new DataBlock(NT_PointLabelblock));
         Filesections.AddSection((int)InternalFileSections.NT_LabelblockTable1, new DataBlock(NT_LabelblockTable1));
         Filesections.AddSection((int)InternalFileSections.NT_LabelblockTable2, new DataBlock(NT_LabelblockTable2));
         if (GapOffset > HeaderOffset + Headerlength) // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));

         // Datenblöcke einlesen
         Filesections.ReadSections(br);

         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);
      }

      protected override void DecodeSections() {
         // Datenblöcke "interpretieren"
         int filesectiontype;

         filesectiontype = (int)InternalFileSections.NT_PointDatabtable;
         if (Filesections.GetLength(filesectiontype) > 0) {
            //Decode_NT_PointDatabtable(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            //Filesections.RemoveSection(filesectiontype);
         }
         // usw.


      }

      /// <summary>
      /// liest die Daten in <see cref="polygone"/> ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="bRelaxed"></param>
      void Decode_PolygoneData(BinaryReaderWriter br, bool bRelaxed) {
         if (PolygoneTableBlock.Count > 0) {
            StringBuilder sb = new StringBuilder();

            // Tabelle für Typen und Offsets zu den eigentlichen Daten einlesen
            PolygonTableItems = new List<TableItem>();
            br.Seek(PolygoneTableBlock.Offset);
            for (int i = 0; i < PolygoneTableBlock.Count; i++)
               PolygonTableItems.Add(new TableItem(br, PolygoneTableBlock.Recordsize));

            // Draworder-Tabelle einlesen
            PolygonDraworderTableItems = new List<PolygonDraworderTableItem>();
            uint iLevel = 1;
            br.Seek(PolygoneDraworderTableBlock.Offset);
            int blocklen = (int)PolygoneDraworderTableBlock.Length;
            if (blocklen > 0)
               while (blocklen >= PolygoneDraworderTableBlock.Recordsize) {
                  PolygonDraworderTableItem dro = new PolygonDraworderTableItem(br, PolygoneDraworderTableBlock.Recordsize, iLevel);
                  blocklen -= PolygoneDraworderTableBlock.Recordsize;
                  PolygonDraworderTableItems.Add(dro);
                  if (dro.Type == 0) // nächster Level
                     iLevel++;
               }

            // Tabelle der Polygondaten einlesen
            polygone.Clear();
            for (int i = 0; i < PolygonTableItems.Count; i++) {
               br.Seek(PolygonTableItems[i].Offset + PolygoneDatablock.Offset);
               int datalen = i < PolygonTableItems.Count - 1 ?
                                    PolygonTableItems[i + 1].Offset - PolygonTableItems[i].Offset :
                                    (int)PolygoneTableBlock.Offset - (PolygonTableItems[i].Offset + (int)PolygoneDatablock.Offset);
               try {
                  long startpos = br.Position;
                  Polygone p = new Polygone(PolygonTableItems[i].Type, PolygonTableItems[i].Subtype);
                  p.Read(br);
                  Debug.WriteLineIf(startpos + datalen != br.Position,
                     string.Format("Diff. {0} der Datenlänge beim Lesen des Objektes 0x{1:x} 0x{2:x} (größer 0 bedeutet: zuviel gelesen)",
                                    br.Position - (startpos + datalen), PolygonTableItems[i].Type, PolygonTableItems[i].Subtype));
                  // zugehörige Draworder suchen
                  for (int j = 0; j < PolygonDraworderTableItems.Count; j++) {
                     if (p.Type == PolygonDraworderTableItems[j].Type) // Haupttyp gefunden
                        for (int k = 0; k < PolygonDraworderTableItems[j].Subtypes.Count; k++)
                           if (p.Subtype == PolygonDraworderTableItems[j].Subtypes[k]) { // auch Subtyp gefunden
                              p.Draworder = PolygonDraworderTableItems[j].Level;
                              j = PolygonDraworderTableItems.Count;       // 2. Schleifenabbruch
                              break;
                           }
                  }
                  polygone.Add(p, 0);
               } catch (Exception ex) {
                  if (bRelaxed) {
                     sb.AppendFormat("Fehler beim Einlesen von Polygon 0x{0:x2}, 0x{1:x2}: {2}", PolygonTableItems[i].Type, PolygonTableItems[i].Subtype, ex.Message);
                     sb.AppendLine();
                  } else
                     throw new Exception(ex.Message);
               }
            }
            if (bRelaxed)
               RelaxedModeErrors += sb.ToString();
         }
      }

      /// <summary>
      /// liest die Daten in <see cref="polyline"/> ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="bRelaxed"></param>
      void Decode_PolylineData(BinaryReaderWriter br, bool bRelaxed) {
         if (PolylineTableBlock.Count > 0) {
            StringBuilder sb = new StringBuilder();

            // Tabelle für Typen und Offsets zu den eigentlichen Daten einlesen
            PolylineTableItems = new List<TableItem>();
            br.Seek(PolylineTableBlock.Offset);
            for (int i = 0; i < PolylineTableBlock.Count; i++)
               PolylineTableItems.Add(new TableItem(br, PolylineTableBlock.Recordsize));

            // Tabelle der Polylinedaten einlesen
            polyline.Clear();
            for (int i = 0; i < PolylineTableItems.Count; i++) {
               br.Seek(PolylineTableItems[i].Offset + PolylineDatablock.Offset);
               int datalen = i < PolylineTableItems.Count - 1 ?
                                    PolylineTableItems[i + 1].Offset - PolylineTableItems[i].Offset :
                                    (int)PolylineTableBlock.Offset - (PolylineTableItems[i].Offset + (int)PolylineDatablock.Offset);
               try {
                  long startpos = br.Position;
                  Polyline p = new Polyline(PolylineTableItems[i].Type, PolylineTableItems[i].Subtype);
                  p.Read(br);
                  Debug.WriteLineIf(startpos + datalen != br.Position,
                     string.Format("Diff. {0} der Datenlänge beim Lesen des Objektes 0x{1:x} 0x{2:x} (größer 0 bedeutet: zuviel gelesen)",
                                    br.Position - (startpos + datalen), PolylineTableItems[i].Type, PolylineTableItems[i].Subtype));
                  polyline.Add(p, 0);
               } catch (Exception ex) {
                  if (bRelaxed) {
                     sb.AppendFormat("Fehler beim Einlesen von Linie 0x{0:x2}, 0x{1:x2}: {2}", PolylineTableItems[i].Type, PolylineTableItems[i].Subtype, ex.Message);
                     sb.AppendLine();
                  } else
                     throw new Exception(ex.Message);
               }
            }
            if (bRelaxed)
               RelaxedModeErrors += sb.ToString();
         }
      }

      /// <summary>
      /// liest die Daten in <see cref="poi"/> ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="bRelaxed"></param>
      void Decode_POIData(BinaryReaderWriter br, bool bRelaxed) {
         if (PointTableBlock.Count > 0) {
            StringBuilder sb = new StringBuilder();

            // Tabelle für Typen und Offsets zu den eigentlichen Daten einlesen
            PointTableItems = new List<TableItem>();
            br.Seek(PointTableBlock.Offset);
            for (int i = 0; i < PointTableBlock.Count; i++)
               PointTableItems.Add(new TableItem(br, PointTableBlock.Recordsize));

            // Tabelle der POI-Daten einlesen
            poi.Clear();
            for (int i = 0; i < PointTableItems.Count; i++) {
               br.Seek(PointTableItems[i].Offset + PointDatablock.Offset);
               int datalen = i < PointTableItems.Count - 1 ?
                                    PointTableItems[i + 1].Offset - PointTableItems[i].Offset :
                                    (int)PointTableBlock.Offset - (PointTableItems[i].Offset + (int)PointDatablock.Offset);
               try {
                  long startpos = br.Position;
                  POI p = new POI(PointTableItems[i].Type, PointTableItems[i].Subtype);
                  p.Read(br);
                  Debug.WriteLineIf(startpos + datalen != br.Position,
                     string.Format("Diff. {0} der Datenlänge beim Lesen des Objektes 0x{1:x} 0x{2:x} (größer 0 bedeutet: zuviel gelesen)",
                                    br.Position - (startpos + datalen), PointTableItems[i].Type, PointTableItems[i].Subtype));
                  poi.Add(p, 0);
               } catch (Exception ex) {
                  if (bRelaxed) {
                     sb.AppendFormat("Fehler beim Einlesen von Punkt 0x{0:x2}, 0x{1:x2}: {2}", PointTableItems[i].Type, PointTableItems[i].Subtype, ex.Message);
                     sb.AppendLine();
                  } else
                     throw new Exception(ex.Message);
               }
            }
            if (bRelaxed)
               RelaxedModeErrors += sb.ToString();
         }
      }

      /// <summary>
      /// zur Verwendung im Konstruktor
      /// </summary>
      /// <param name="br"></param>
      /// <param name="raw"></param>
      /// <param name="headeroffset"></param>
      /// <param name="gapoffset"></param>
      protected void nonvirtual_Read(BinaryReaderWriter br, bool raw = false, uint headeroffset = 0, uint gapoffset = 0) {
         base.Read(br, raw, headeroffset, gapoffset);

         br.SetEncoding(Codepage);

         RelaxedModeErrors = "";
         bool bRelaxed = true;

         Decode_PolygoneData(br, bRelaxed);
         Decode_PolylineData(br, bRelaxed);
         Decode_POIData(br, bRelaxed);

      }

      public override void Read(BinaryReaderWriter br, bool raw = false, uint headeroffset = 0, uint gapoffset = 0) {
         nonvirtual_Read(br, raw, headeroffset, gapoffset);
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         Unknown_0x0C = 0x01; // Bedeutung unklar
         base.Encode_Header(bw);

         bw.Write(Codepage);

         PointDatablock.Write(bw);
         PolylineDatablock.Write(bw);
         PolygoneDatablock.Write(bw);

         bw.Write(FamilyID);
         bw.Write(ProductID);

         PointTableBlock.Write(bw);
         PolylineTableBlock.Write(bw);
         PolygoneTableBlock.Write(bw);
         PolygoneDraworderTableBlock.Write(bw);

         if (Headerlength > 0x5b) {
            NT_PointTableBlock.Write(bw);
            bw.Write(nt_unknown_0x65);
            NT_PointDatablock.Write(bw);

            if (Headerlength > 0x6E) {
               bw.Write(nt_unknown_0x6E);
               NT_PointLabelblock.Write(bw);
               bw.Write(nt_unknown_0x7A);
               bw.Write(nt_unknown_0x7E);
               NT_LabelblockTable2.Write(bw);
               bw.Write(nt_unknown_0x8A);
               bw.Write(nt_unknown_0x8E);
               NT_LabelblockTable2.Write(bw);
               bw.Write(nt_unknown_0x9A);
               bw.Write(nt_unknown_0x9C);

               if (Headerlength > 0xA4) {
                  bw.Write(nt_unknown_0xA4);

                  if (Headerlength > 0xAE) {
                     nt_unknown_0xAE = new byte[Headerlength - 0xAE];
                     bw.Write(nt_unknown_0xAE);
                  }
               }
            }
         }
      }

      void Encode_PolygoneData(BinaryReaderWriter bw) {
         List<TableItem> table = new List<TableItem>();

         // ----- Polygonblock schreiben
         // sollte besser aus der max. notwendigen Offsetgröße bestimmt werden (5 --> Offset max. 3 Byte)
         PolygoneTableBlock.Recordsize = 5;
         PolygoneDatablock.Offset = (uint)bw.Position;
         foreach (Polygone p in polygone.Keys) {
            TableItem tableitem = new TableItem {
               Type = p.Type,
               Subtype = p.Subtype,
               Offset = (int)(bw.Position - PolygoneDatablock.Offset)
            };
            table.Add(tableitem);
            p.Write(bw, Codepage);
         }
         PolygoneDatablock.Length = (uint)bw.Position - PolygoneDatablock.Offset;

         // ----- Polygontabelle schreiben
         PolygoneTableBlock.Offset = (uint)bw.Position;    // Standort der Tabelle
         for (int i = 0; i < table.Count; i++)
            table[i].Write(bw, PolygoneTableBlock.Recordsize);
         PolygoneTableBlock.Length = (uint)bw.Position - PolygoneTableBlock.Offset;
      }

      void Encode_PolylineData(BinaryReaderWriter bw) {
         List<TableItem> table = new List<TableItem>();

         // sollte besser aus der max. notwendigen Offsetgröße bestimmt werden (5 --> Offset max. 3 Byte)
         PolylineTableBlock.Recordsize = 5;
         PolylineDatablock.Offset = (uint)bw.Position;
         table.Clear();
         foreach (Polyline p in polyline.Keys) {
            TableItem tableitem = new TableItem {
               Type = p.Type,
               Subtype = p.Subtype,
               Offset = (int)(bw.Position - PolylineDatablock.Offset)
            };
            table.Add(tableitem);
            p.Write(bw, Codepage);
         }
         PolylineDatablock.Length = (uint)bw.Position - PolylineDatablock.Offset;

         // ----- Polylinetabelle schreiben
         PolylineTableBlock.Offset = (uint)bw.Position;    // Standort der Tabelle
         for (int i = 0; i < table.Count; i++)
            table[i].Write(bw, PolylineTableBlock.Recordsize);
         PolylineTableBlock.Length = (uint)bw.Position - PolylineTableBlock.Offset;
      }

      void Encode_POIData(BinaryReaderWriter bw) {
         List<TableItem> table = new List<TableItem>();

         // ----- POI-Block schreiben
         // sollte besser aus der max. notwendigen Offsetgröße bestimmt werden (5 --> Offset max. 3 Byte)
         PointTableBlock.Recordsize = 5;
         PointDatablock.Offset = (uint)bw.Position;
         table.Clear();
         foreach (POI p in poi.Keys) {
            TableItem tableitem = new TableItem {
               Type = p.Type,
               Subtype = p.Subtype,
               Offset = (int)(bw.Position - PointDatablock.Offset)
            };
            table.Add(tableitem);
            p.Write(bw, Codepage);
         }
         PointDatablock.Length = (uint)bw.Position - PointDatablock.Offset;

         // ----- POI-Tabelle schreiben
         PointTableBlock.Offset = (uint)bw.Position;    // Standort der Tabelle
         for (int i = 0; i < table.Count; i++)
            table[i].Write(bw, PointTableBlock.Recordsize);
         PointTableBlock.Length = (uint)bw.Position - PointTableBlock.Offset;
      }

      void Encode_Draworder(BinaryReaderWriter bw) {
         // je Draworder eine Liste der Typen; je Typ eine Liste der Subtypes
         SortedList<uint, SortedList<uint, SortedList<uint, uint>>> draworderlist = new SortedList<uint, SortedList<uint, SortedList<uint, uint>>>();
         foreach (Polygone p in polygone.Keys) {
            if (!draworderlist.TryGetValue(p.Draworder, out SortedList<uint, SortedList<uint, uint>>? typelist)) {
               typelist = new SortedList<uint, SortedList<uint, uint>>();
               draworderlist.Add(p.Draworder, typelist);
            }
            if (!typelist.TryGetValue(p.Type, out SortedList<uint, uint>? subtypelist)) {
               subtypelist = new SortedList<uint, uint>();
               typelist.Add(p.Type, subtypelist);
            }
            subtypelist.Add(p.Subtype, 0);

         }

         PolygoneDraworderTableBlock.Recordsize = 5;
         PolygoneDraworderTableBlock.Offset = (uint)bw.Position;
         uint olddraworder = 0;

         foreach (uint draworder in draworderlist.Keys) {
            while (olddraworder > 0 &&
                   draworder != olddraworder) {                  // Kennung für Erhöhung der Draworder schreiben
               new PolygonDraworderTableItem(0, 0).Write(bw, PolygoneDraworderTableBlock.Recordsize);
               olddraworder++;
            }
            olddraworder = draworder;

            SortedList<uint, SortedList<uint, uint>> typelist = draworderlist[draworder];
            foreach (uint type in typelist.Keys) {                // für jeden Typ dieser Draworder einen Tabelleneintrag erzeugen
               PolygonDraworderTableItem ti = new PolygonDraworderTableItem(type, draworder);
               // ev. vorhandene Subtypes ergänzen
               SortedList<uint, uint> subtypelist = typelist[type];

               foreach (uint subtype in subtypelist.Keys)
                  ti.Subtypes.Add(subtype);
               ti.Write(bw, PolygoneDraworderTableBlock.Recordsize);
            }
         }
         PolygoneDraworderTableBlock.Length = (uint)bw.Position - PolygoneDraworderTableBlock.Offset;
      }

      public override void Encode_Sections() {
         //SetData2Filesection((int)InternalFileSections.NT_PointDatabtable, true);

      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         //switch ((InternalFileSections)filesectiontype) {
         //   case InternalFileSections.NT_PointDatabtable:
         //      Encode_NT_PointDatabtable(bw);
         //      break;

         //}
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.NT_PointDatablock, pos++);
         Filesections.SetOffset((int)InternalFileSections.NT_PointDatabtable, pos++);
         Filesections.SetOffset((int)InternalFileSections.NT_PointLabelblock, pos++);
         Filesections.SetOffset((int)InternalFileSections.NT_LabelblockTable1, pos++);
         Filesections.SetOffset((int)InternalFileSections.NT_LabelblockTable2, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         NT_PointTableBlock = new TypDataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.NT_PointDatabtable));
         NT_PointDatablock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.NT_PointDatablock));
         NT_PointLabelblock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.NT_PointLabelblock));
         NT_LabelblockTable1 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.NT_LabelblockTable1));
         NT_LabelblockTable2 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.NT_LabelblockTable2));
      }

      public override void Write(BinaryReaderWriter bw, uint headeroffset = 0, UInt16 headerlength = 0x5B, uint gapoffset = 0, uint dataoffset = 0, bool setsectiondata = true) {
         HeaderOffset = headeroffset;
         if (headerlength > 0)
            Headerlength = headerlength;

         CreationDate = DateTime.Now;

         GapOffset = gapoffset;
         DataOffset = dataoffset;

         bw.SetEncoding(Codepage);
         bw.Seek(Headerlength);

         Encode_PolygoneData(bw);
         Encode_PolylineData(bw);
         Encode_POIData(bw);
         Encode_Draworder(bw);

         SetSectionsAlign();

         Encode_Header(bw); // Header mit den akt. Offsets neu erzeugen

         Filesections.WriteSections(bw);

      }


      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public POI? GetPoi(int idx) {
         return (0 <= idx && idx < poi.Count) ? poi.Keys[idx] : null;
      }
      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="subtyp"></param>
      /// <returns></returns>
      public POI? GetPoi(uint typ, uint subtyp) {
         int idx = poi.IndexOfKey(new POI(typ, subtyp));
         return idx >= 0 ? GetPoi(idx) : null;
      }

      /// <summary>
      /// löscht einen POI aus der internen sortierte Liste
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public bool RemovePoi(int idx) {
         if (0 <= idx && idx < poi.Count) {
            poi.RemoveAt(idx);
            return true;
         }
         return false;
      }
      public bool RemovePoi(uint typ, uint subtyp) {
         return RemovePoi(poi.IndexOfKey(new POI(typ, subtyp)));
      }

      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Polygone? GetPolygone(int idx) {
         return (0 <= idx && idx < polygone.Count) ? polygone.Keys[idx] : null;
      }
      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="subtyp"></param>
      /// <returns></returns>
      public Polygone? GetPolygone(uint typ, uint subtyp) {
         int idx = polygone.IndexOfKey(new Polygone(typ, subtyp));
         return idx >= 0 ? GetPolygone(idx) : null;
      }

      /// <summary>
      /// löscht ein Polygone aus der internen sortierte Liste
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public bool RemovePolygone(int idx) {
         if (0 <= idx && idx < polygone.Count) {
            polygone.RemoveAt(idx);
            return true;
         }
         return false;
      }
      public bool RemovePolygone(uint typ, uint subtyp) {
         return RemovePolygone(polygone.IndexOfKey(new Polygone(typ, subtyp)));
      }

      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Polyline? GetPolyline(int idx) {
         return (0 <= idx && idx < polyline.Count) ? polyline.Keys[idx] : null;
      }
      /// <summary>
      /// liefert einen POI aus der internen sortierten Liste; bei Fehler oder Nichtexistenz null
      /// </summary>
      /// <param name="typ"></param>
      /// <param name="subtyp"></param>
      /// <returns></returns>
      public Polyline? GetPolyline(uint typ, uint subtyp) {
         int idx = polyline.IndexOfKey(new Polyline(typ, subtyp));
         return idx >= 0 ? GetPolyline(idx) : null;
      }
      /// <summary>
      /// löscht ein Polyline aus der internen sortierte Liste
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public bool RemovePolyline(int idx) {
         if (0 <= idx && idx < polyline.Count) {
            polyline.RemoveAt(idx);
            return true;
         }
         return false;
      }
      public bool RemovePolyline(uint typ, uint subtyp) {
         return RemovePolyline(polyline.IndexOfKey(new Polyline(typ, subtyp)));
      }

      /// <summary>
      /// löscht ein GraphicElement aus der entsprechenden internen Liste
      /// </summary>
      /// <param name="ge"></param>
      /// <returns></returns>
      public bool Remove(GraphicElement ge) {
         if (ge is Polygone)
            return polygone.Remove((Polygone)ge);
         if (ge is Polyline)
            return polyline.Remove((Polyline)ge);
         if (ge is POI)
            return poi.Remove((POI)ge);
         return false;
      }

      /// <summary>
      /// fügt ein GraphicElement in die entsprechende interne sortierte Liste ein; false, wenn es schon existiert
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public bool Insert(GraphicElement ge) {
         if (ge is Polygone) {
            if (polygone.ContainsKey((Polygone)ge))
               return false;
            else
               polygone.Add((Polygone)ge, 0);
            return true;
         }
         if (ge is Polyline) {
            if (polyline.ContainsKey((Polyline)ge))
               return false;
            else
               polyline.Add((Polyline)ge, 0);
            return true;
         }
         if (ge is POI) {
            if (poi.ContainsKey((POI)ge))
               return false;
            else
               poi.Add((POI)ge, 0);
            return true;
         }
         return false;
      }

      /// <summary>
      /// ändert Typ und Subtyp eines Elements
      /// </summary>
      /// <param name="ge"></param>
      /// <param name="typ"></param>
      /// <param name="subtyp"></param>
      /// <returns></returns>
      public bool ChangeTyp(GraphicElement ge, uint typ, uint subtyp) {
         if ((ge is Polygone && GetPolygone(typ, subtyp) != null) ||    // ex. schon
             (ge is Polyline && GetPolyline(typ, subtyp) != null) ||
             (ge is POI && GetPoi(typ, subtyp) != null))
            return false;
         Remove(ge);
         ge.Type = typ;
         ge.Subtype = subtyp;
         Insert(ge);
         return true;
      }


      #region nur zum Testen

      class NTTableItem {
         public UInt16 v1;
         public UInt16 v2;

         public NTTableItem(BinaryReaderWriter br) {
            v1 = br.Read2AsUShort();
            v2 = br.Read2AsUShort();
         }
         public override string ToString() {
            return string.Format("NTTableItem=[v1 0x{0:x}, v2 0x{1:x}]", v1, v2);
         }
      }

      protected void Test(BinaryReaderWriter br) {

         int txtcount = 0;
         uint offset = 0;
         string txt = "";
         while (offset < NT_PointLabelblock.Length) {
            txt = GetNTLabel(br, offset);
            offset = (uint)br.Position - NT_PointLabelblock.Offset;
            txtcount++;
         }
         Debug.WriteLine(string.Format("{0} Texte", txtcount));

         List<NTTableItem> block1 = new List<NTTableItem>();
         br.Seek(NT_LabelblockTable1.Offset);
         uint len = NT_LabelblockTable1.Length;
         while (len > 4) {
            block1.Add(new NTTableItem(br));
            len -= 4;
         }
         Debug.WriteLine(string.Format("{0} Einträge in nt2_block1", block1.Count));

         List<NTTableItem> block2 = new List<NTTableItem>();
         br.Seek(NT_LabelblockTable2.Offset);
         len = NT_LabelblockTable2.Length;
         while (len > 4) {
            block2.Add(new NTTableItem(br));
            len -= 4;
         }
         Debug.WriteLine(string.Format("{0} Einträge in nt2_block2", block2.Count));

         for (int i = 0; i < block1.Count; i++) {
            uint offs1 = block1[i].v1;
            uint key1 = block1[i].v2;
            string txt1 = GetNTLabel(br, offs1);

            //if (offs1 == 0xa552) {
            //   Debug.WriteLine(block1[i].ToString());
            //}

            bool found = false;
            for (int j = 0; j < block2.Count; j++) {
               uint key2 = block2[j].v1;
               if (key1 == key2) {
                  found = true;
                  uint offs2 = block2[j].v2;
                  string txt2 = GetNTLabel(br, offs2);
                  if (txt1 != txt2)
                     Debug.WriteLine(string.Format("{0}: {1} <--> {2} key 0x{3:x}", i, txt1, txt2, key1));
                  //else
                  //   Debug.WriteLine(string.Format("{0}: {1} OK", i, txt1));
                  break;
               }
            }
            if (!found)
               Debug.WriteLine(string.Format("{0}: {1} nicht gefunden", i, txt1));

         }



      }

      protected string GetNTLabel(BinaryReaderWriter br, uint offset) {
         br.Seek(NT_PointLabelblock.Offset + offset);
         return br.ReadString();
         //List<char> chars = new List<char>();
         //char c;
         //do {
         //   c = br.ReadChar();
         //   if (c != 0x0) chars.Add(c);
         //} while (c != 0x0);
         //return new string(chars.ToArray());
      }

      #endregion


      bool _isdisposed = false;

      protected override void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {
            if (notfromfinalizer) { // nur dann alle managed Ressourcen freigeben
               if (PolygonTableItems != null)
                  PolygonTableItems.Clear();
               if (PolylineTableItems != null)
                  PolylineTableItems.Clear();
               if (PointTableItems != null)
                  PointTableItems.Clear();
               if (PolygonDraworderTableItems != null)
                  PolygonDraworderTableItems.Clear();

               if (polygone != null)
                  polygone.Clear();
               if (polyline != null)
                  polyline.Clear();
               if (poi != null)
                  poi.Clear();
            }

            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;
            base.Dispose(notfromfinalizer);
         }
      }

   }

}
